using Serilog;
using Zarichney.Services.Auth;
using Zarichney.Services.Status;

namespace Zarichney.Startup;

/// <summary>
/// Handles critical validation checks during application startup to ensure required configurations
/// are present and valid before the application starts.
/// </summary>
public static class ValidateStartup
{
  /// <summary>
  /// Indicates whether the Identity Database connection string is available
  /// </summary>
  public static bool IsIdentityDbAvailable { get; private set; } = true;

  /// <summary>
  /// Validates critical configuration settings when running in Production environment.
  /// The application will exit with an error code if required configuration is missing.
  /// In non-Production environments, it will allow the application to start but will
  /// mark the Identity Database as unavailable if the connection string is missing.
  /// </summary>
  /// <param name="builder">The WebApplicationBuilder containing configuration</param>
  /// <returns>True if all validations pass, false if any non-critical validations fail in non-Production</returns>
  public static bool ValidateProductionConfiguration(WebApplicationBuilder builder)
  {
    return ValidateProductionConfiguration(builder.Environment, builder.Configuration);
  }

  /// <summary>
  /// Validates critical configuration settings when running in Production environment.
  /// The application will exit with an error code if required configuration is missing.
  /// In non-Production environments, it will allow the application to start but will
  /// mark the Identity Database as unavailable if the connection string is missing.
  /// </summary>
  /// <param name="environment">The web host environment</param>
  /// <param name="configuration">The configuration</param>
  /// <returns>True if all validations pass, false if any non-critical validations fail in non-Production</returns>
  public static bool ValidateProductionConfiguration(IWebHostEnvironment environment, IConfiguration configuration)
  {
    // Check for Identity database connection string
    var connectionString = configuration["ConnectionStrings:" + UserDbContext.UserDatabaseConnectionName];
    var isConnectionStringEmpty = string.IsNullOrEmpty(connectionString);

    // Production environment requires the Identity DB connection string
    if (environment.EnvironmentName.Equals("Production", StringComparison.OrdinalIgnoreCase))
    {
      if (isConnectionStringEmpty)
      {
        // Log a fatal error for the missing configuration
        Log.Fatal("CRITICAL ERROR: Identity Database connection string '{ConnectionName}' is missing or empty. " +
                  "Application cannot start in Production environment.",
                  UserDbContext.UserDatabaseConnectionName);

        // For unit testing, throw an exception instead of exiting when running in test mode
        bool isTestMode = AppDomain.CurrentDomain.GetAssemblies()
          .Any(a => a.FullName?.Contains("xunit") == true || a.FullName?.Contains("TestHost") == true);

        if (isTestMode)
        {
          IsIdentityDbAvailable = false;
          throw new InvalidOperationException($"Identity Database connection string '{UserDbContext.UserDatabaseConnectionName}' is missing. Application cannot start in Production environment.");
        }
        else
        {
          // Exit the application with a non-zero exit code in normal operation
          Environment.Exit(1);
        }

        // This line will never be reached, but it's here for clarity
        return false;
      }

      IsIdentityDbAvailable = true;
      return true;
    }

    // In non-Production environments, mark the Identity DB as unavailable but allow the app to start
    if (isConnectionStringEmpty)
    {
      Log.Warning("Identity Database connection string '{ConnectionName}' is missing or empty. " +
                 "Application will start, but authentication functionality will be degraded.",
                 UserDbContext.UserDatabaseConnectionName);

      IsIdentityDbAvailable = false;
      return false;
    }

    IsIdentityDbAvailable = true;
    return true;
  }
}
