using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Serilog;
using Zarichney.Config;
using Zarichney.Services.AI;

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
      .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SessionId} {ScopeId} {Message:lj}{NewLine}{Exception}"
      )
      .Enrich.FromLogContext()
      .Enrich.WithProperty("SessionId", null)
      .Enrich.WithProperty("ScopeId", null);

    var seqUrl = builder.Configuration["LoggingConfig:SeqUrl"];
    if (!string.IsNullOrEmpty(seqUrl) && Uri.IsWellFormedUriString(seqUrl, UriKind.Absolute))
    {
      logger = logger.WriteTo.Seq(seqUrl);
    }
    else
    {
      var logPath = Path.Combine("logs", "api-server.log");
      logger = logger.WriteTo.File(
        logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7
      );
    }

    Log.Logger = logger.CreateLogger();
    Log.Information("Starting up Zarichney API...");

    builder.Host.UseSerilog();
  }

  #region Configuration Registration

  private const string PlaceholderValue = "recommended to set in app secrets";
  private const string DataFolderName = "Data/";

  /// <summary>
  /// Registers configuration services and binds configuration objects
  /// </summary>
  public static void RegisterConfigurationServices(this IServiceCollection services, IConfiguration configuration,
    IWebHostEnvironment environment)
  {
    string dataPath;
    if (environment.IsDevelopment())
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
        dataPath = Path.GetFullPath(Path.Combine(solutionRoot.FullName, DataFolderName));
        Log.Information(
          "Development environment detected. Found solution root at {SolutionRoot}. Using calculated data path: {DataPath}",
          solutionRoot.FullName, dataPath);
      }
      else
      {
        // Fallback if .sln not found (might happen in specific deployment/test scenarios)
        Log.Warning(
          "Could not determine solution root directory. Falling back to relative path based on execution directory: {BaseDirectory}",
          currentDir);
        // This fallback might still result in the test output path, consider if another fallback is better.
        // Maybe try the original parent logic as a secondary fallback?
        var parentDirectory = Directory.GetParent(Environment.CurrentDirectory)?.FullName;
        if (!string.IsNullOrEmpty(parentDirectory))
        {
          dataPath = Path.GetFullPath(Path.Combine(parentDirectory, DataFolderName));
          Log.Information("Fallback using parent of CurrentDirectory ({CurrentDirectory}). Path: {DataPath}",
            Environment.CurrentDirectory, dataPath);
        }
        else
        {
          dataPath = Path.GetFullPath(DataFolderName); // Relative to current execution dir
          Log.Warning("Fallback using relative path from execution directory. Path: {DataPath}", dataPath);
        }
      }
    }
    else // Production or other environments
    {
      dataPath = Environment.GetEnvironmentVariable("APP_DATA_PATH") ?? DataFolderName;
      Log.Information(
        "Production/Other environment detected. Using APP_DATA_PATH environment variable (or default): {DataPath}",
        dataPath);
      // Consider adding validation here to ensure APP_DATA_PATH provides an absolute path in production if required.
    }

    var pathConfigs = configuration.AsEnumerable()
      .Where(kvp => kvp.Value?.StartsWith(DataFolderName) == true)
      .ToList();

    Log.Information("Found {Count} {Path} paths in configuration:", pathConfigs.Count, DataFolderName);
    foreach (var kvp in pathConfigs.Where(kvp =>
               Path.Combine(dataPath, kvp.Value![DataFolderName.Length..]) != kvp.Value))
    {
      var newPath = Path.Combine(dataPath, kvp.Value![DataFolderName.Length..]);
      Log.Information("Transforming path: {OldPath} -> {NewPath}", kvp.Value, newPath);
    }

    var transformedPaths = pathConfigs
      .Select(kvp => new KeyValuePair<string, string>(
        kvp.Key,
        Path.Combine(dataPath, kvp.Value![DataFolderName.Length..])
      ))
      .ToList();

    if (transformedPaths.Count != 0)
    {
      ((IConfigurationBuilder)configuration).AddInMemoryCollection(transformedPaths!);

      // Verify final configuration
      Log.Information("Final configuration paths:");
      foreach (var kvp in pathConfigs)
      {
        var finalValue = configuration[kvp.Key];
        Log.Information("{Key}: {Value}", kvp.Key, finalValue);
      }
    }

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

      if (property.GetCustomAttribute<RequiredAttribute>() == null || value is not (null or PlaceholderValue))
      {
        continue;
      }

      var warningMessage = $"Configuration Warning: Required property '{property.Name}' in section '{sectionName}'";

      warningMessage += value switch
      {
        null => " is missing.",
        PlaceholderValue => " has a placeholder value. Please set it in your user secrets.",
        _ => string.Empty
      };

      // Log a warning instead of throwing an exception
      Log.Warning(warningMessage + " Dependent services may fail at runtime.",
        new { ConfigSection = sectionName, ConfigProperty = property.Name });
    }
  }

  #endregion
}