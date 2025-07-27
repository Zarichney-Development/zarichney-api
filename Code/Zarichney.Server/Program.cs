using Serilog;
using Zarichney.Startup;

namespace Zarichney;

public class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);
    ConfigureBuilder(builder);

    var app = builder.Build();
    // TEMPORARY: Force trigger analysis workflows

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
    ValidateStartup.ConfigureValidation(webBuilder);
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
