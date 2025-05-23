using Serilog;
using Zarichney.Services.Status;
using Zarichney.Startup;

namespace Zarichney;

public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);
    ConfigureBuilder(builder);

    // Validate critical configuration and capture Identity DB availability status
    // In Production, this will exit the app if the Identity DB is not available
    // In non-Production, this will set the availability status but allow the app to continue
    _ = ValidateStartup.ValidateProductionConfiguration(builder);

    var app = builder.Build();

    // Update the StatusService with the Identity DB availability
    if (app.Services.GetService<IStatusService>() is StatusService statusService)
    {
      // Include missing configuration details when Identity DB is unavailable
      var missingConfigurations = ValidateStartup.IsIdentityDbAvailable
        ? null
        : new List<string> { $"ConnectionStrings:{Zarichney.Services.Auth.UserDbContext.UserDatabaseConnectionName}" };

      statusService.SetServiceAvailability(ExternalServices.PostgresIdentityDb, ValidateStartup.IsIdentityDbAvailable, missingConfigurations);
    }

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
