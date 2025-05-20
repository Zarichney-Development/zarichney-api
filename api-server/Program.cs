using Serilog;
using Zarichney.Config;
using Zarichney.Services.Auth;
using Zarichney.Startup;

namespace Zarichney;

public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);
    
    // Configure initial services, configuration, and logging
    ConfigureBuilder(builder);
    
    // Check for critical configuration in Production environment
    ValidateProductionConfiguration(builder);
    
    var app = builder.Build();
    await ConfigureApplication(app);
    try
    {
      app.Run();
    }
    catch (Exception ex)
    {
      Log.Fatal(ex, "Application terminated unexpectedly");
    }
    finally
    {
      Log.CloseAndFlush();
    }
  }

  /// <summary>
  /// Validates critical configuration settings when running in Production environment.
  /// The application will exit with an error code if required configuration is missing.
  /// </summary>
  /// <param name="builder">The WebApplicationBuilder containing configuration</param>
  private static void ValidateProductionConfiguration(WebApplicationBuilder builder)
  {
    // Only enforce strict validation in Production environment
    if (!builder.Environment.IsProduction())
    {
      return;
    }
    
    // Check for required Identity database connection string
    var connectionString = builder.Configuration.GetConnectionString(UserDbContext.UserDatabaseConnectionName);
    
    if (string.IsNullOrEmpty(connectionString))
    {
      // Log a fatal error for the missing configuration
      Log.Fatal("CRITICAL ERROR: Identity Database connection string '{ConnectionName}' is missing or empty. " +
                "Application cannot start in Production environment.",
                UserDbContext.UserDatabaseConnectionName);
      
      // Exit the application with a non-zero exit code
      Environment.Exit(1);
    }
  }

  private static void ConfigureBuilder(WebApplicationBuilder webBuilder)
  {
    ApplicationStartup.ConfigureEncoding();
    ApplicationStartup.ConfigureKestrel(webBuilder);
    ConfigurationStartup.ConfigureConfiguration(webBuilder);
    ConfigurationStartup.ConfigureLogging(webBuilder);
    ServiceStartup.ConfigureServices(webBuilder);
    AuthenticationStartup.ConfigureIdentity(webBuilder);
    ServiceStartup.ConfigureSwagger(webBuilder);
    ApplicationStartup.ConfigureCors(webBuilder);
  }

  private static async Task ConfigureApplication(WebApplication application)
  {
    await ApplicationStartup.ConfigureApplication(application);
  }
}
