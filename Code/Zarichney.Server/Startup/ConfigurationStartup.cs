using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Options;
using Serilog;
using Zarichney.Config;
using Zarichney.Services.AI;
using Zarichney.Services.Status;

namespace Zarichney.Startup;

/// <summary>
/// Handles application configuration from various sources
/// </summary>
public static class ConfigurationStartup
{
  /// <summary>
  /// Configures application configuration sources
  /// </summary>
  public static void ConfigureConfiguration(WebApplicationBuilder builder)
  {
    builder.Configuration
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

    if (builder.Environment.IsDevelopment())
    {
      builder.Configuration.AddUserSecrets<Program>(optional: true);
    }

    builder.Configuration.AddEnvironmentVariables();

    // Add path transformation for development and testing environments
    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    {
      var dataPath = DetermineDataPath(builder.Environment);
      builder.Configuration.AddPathTransformation(
        builder.Configuration, dataPath, DataFolderName);
    }

    if (builder.Environment.IsProduction())
    {
      builder.Configuration.AddSystemsManager("/cookbook-api", new Amazon.Extensions.NETCore.Setup.AWSOptions
      {
        Region = Amazon.RegionEndpoint.USEast2
      }, optional: true);
    }
  }

  /// <summary>
  /// Configures application logging with Serilog
  /// </summary>
  public static void ConfigureLogging(WebApplicationBuilder builder)
  {
    var logger = new LoggerConfiguration()
      .MinimumLevel.Warning()
      .ReadFrom.Configuration(builder.Configuration)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("CorrelationId", null)
      .Enrich.WithProperty("SessionId", null)
      .Enrich.WithProperty("ScopeId", null)
      .Enrich.WithProperty("TestClassName", null)
      .Enrich.WithProperty("TestMethodName", null);

    // Only add console sink if not in Testing environment (tests use their own xUnit sink)
    if (builder.Environment.EnvironmentName != "Testing")
    {
      logger = logger.WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId:-} {SessionId:-} {ScopeId:-} {TestClassName:-} {TestMethodName:-} {Message:lj}{NewLine}{Exception}"
      );
    }

    var seqUrl = builder.Configuration["LoggingConfig:SeqUrl"];
    if (!string.IsNullOrEmpty(seqUrl) && Uri.IsWellFormedUriString(seqUrl, UriKind.Absolute))
    {
      logger = logger.WriteTo.Seq(seqUrl);
    }
    else
    {
      var logPath = Path.Combine("logs", "Zarichney.Server.log");
      logger = logger.WriteTo.File(
        logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7
      );
    }

    Log.Logger = logger.CreateLogger();

    builder.Host.UseSerilog(Log.Logger, dispose: true);
  }

  #region Configuration Registration

  private const string DataFolderName = "Data/";

  /// <summary>
  /// Registers configuration services and binds configuration objects
  /// </summary>
  public static void RegisterConfigurationServices(this IServiceCollection services, IConfiguration configuration,
    IWebHostEnvironment environment)
  {
    // Configuration is already properly transformed by ConfigureConfiguration
    // No reflection needed - proceed directly to config type registration

    var configTypes = Assembly.GetAssembly(typeof(Program))!
      .GetTypes()
      .Where(t => typeof(IConfig).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

    foreach (var configType in configTypes)
    {
      // Use the config type name as the section name
      var sectionName = configType.Name;

      // Create an instance of the config object
      var config = Activator.CreateInstance(configType)!;

      // Attempt to bind the section to the config object
      configuration.GetSection(sectionName).Bind(config);

      // Explicitly reapply environment variable values (manually override if set)
      var properties = configType.GetProperties();
      foreach (var property in properties)
      {
        var envVariableName = $"{sectionName}__{property.Name}";
        var envValue = Environment.GetEnvironmentVariable(envVariableName);

        if (!string.IsNullOrEmpty(envValue))
        {
          var convertedValue = Convert.ChangeType(envValue, property.PropertyType);
          property.SetValue(config, convertedValue);
        }
      }

      ValidateAndReplaceProperties(config, sectionName);

      // Register the configuration as a singleton service
      services.AddSingleton(configType, config);

      // Also register as IOptions<T> for services that expect the Options pattern
      var optionsType = typeof(IOptions<>).MakeGenericType(configType);
      services.AddSingleton(optionsType, sp =>
      {
        var configInstance = sp.GetRequiredService(configType);
        var optionsWrapper = typeof(OptionsWrapper<>).MakeGenericType(configType);
        return Activator.CreateInstance(optionsWrapper, configInstance)!;
      });
    }
  }

  /// <summary>
  /// Registers prompt types from specified assemblies
  /// </summary>
  public static void AddPrompts(this IServiceCollection services, params Assembly[] assemblies)
  {
    var promptTypes = assemblies
      .SelectMany(a => a.GetTypes())
      .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(PromptBase).IsAssignableFrom(type));

    foreach (var promptType in promptTypes)
    {
      services.AddSingleton(promptType);
    }
  }

  private static void ValidateAndReplaceProperties(object config, string sectionName)
  {
    var properties = config.GetType().GetProperties();

    foreach (var property in properties)
    {
      var value = property.GetValue(config);

      if (property.GetCustomAttribute<RequiredAttribute>() == null || value is not (null or StatusService.PlaceholderMessage))
      {
        continue;
      }

      var warningMessage = $"Configuration Warning: Required property '{property.Name}' in section '{sectionName}'";

      warningMessage += value switch
      {
        null => " is missing.",
        StatusService.PlaceholderMessage => " has a placeholder value. Please set it in your user secrets.",
        _ => string.Empty
      };

      // Log a warning instead of throwing an exception
      Log.Warning(warningMessage + " Dependent services may fail at runtime.",
        new { ConfigSection = sectionName, ConfigProperty = property.Name });
    }
  }

  /// <summary>
  /// Determines the appropriate data path based on the environment.
  /// </summary>
  /// <param name="environment">The web host environment</param>
  /// <returns>The absolute path to use for data files</returns>
  private static string DetermineDataPath(IWebHostEnvironment environment)
  {
    if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
    {
      // Try to find the solution root directory by looking for a .sln file
      var currentDir = AppContext.BaseDirectory; // Use BaseDirectory for better consistency
      var directory = new DirectoryInfo(currentDir);

      while (directory != null && !directory.GetFiles("*.sln").Any())
      {
        directory = directory.Parent;
      }

      if (directory != null && directory.GetFiles("*.sln").Any())
      {
        var solutionRoot = directory;
        var dataPath = Path.GetFullPath(Path.Combine(solutionRoot.FullName, DataFolderName));
        Log.Information(
          "Development environment detected. Found solution root at {SolutionRoot}. Using calculated data path: {DataPath}",
          solutionRoot.FullName, dataPath);
        return dataPath;
      }
      else
      {
        // Fallback if .sln not found (might happen in specific deployment/test scenarios)
        Log.Warning(
          "Could not determine solution root directory. Falling back to relative path based on execution directory: {BaseDirectory}",
          currentDir);
        var parentDirectory = Directory.GetParent(Environment.CurrentDirectory)?.FullName;
        if (!string.IsNullOrEmpty(parentDirectory))
        {
          var dataPath = Path.GetFullPath(Path.Combine(parentDirectory, DataFolderName));
          Log.Debug("Fallback using parent of CurrentDirectory ({CurrentDirectory}). Path: {DataPath}",
            Environment.CurrentDirectory, dataPath);
          return dataPath;
        }
        else
        {
          var dataPath = Path.GetFullPath(DataFolderName); // Relative to current execution dir
          Log.Warning("Fallback using relative path from execution directory. Path: {DataPath}", dataPath);
          return dataPath;
        }
      }
    }
    else // Production or other environments
    {
      var dataPath = Environment.GetEnvironmentVariable("APP_DATA_PATH") ?? DataFolderName;
      Log.Information(
        "Production/Other environment detected. Using APP_DATA_PATH environment variable (or default): {DataPath}",
        dataPath);
      return dataPath;
    }
  }

  #endregion
}
