using Serilog;
using Zarichney.Services.Auth;

namespace Zarichney.Startup;

/// <summary>
/// Handles critical validation checks during application startup to ensure required configurations
/// are present and valid before the application starts.
/// </summary>
public static class ValidateStartup
{
  /// <summary>
  /// Validates critical configuration settings when running in Production environment.
  /// The application will exit with an error code if required configuration is missing.
  /// </summary>
  /// <param name="builder">The WebApplicationBuilder containing configuration</param>
  public static void ValidateProductionConfiguration(WebApplicationBuilder builder)
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
}