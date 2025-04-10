using Serilog;
using Zarichney.Server.Startup.App;
using Zarichney.Server.Startup.Authentication;
using Zarichney.Server.Startup.Configuration;
using Zarichney.Server.Startup.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigureBuilder(builder);

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

return;

void ConfigureBuilder(WebApplicationBuilder webBuilder)
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

async Task ConfigureApplication(WebApplication application)
{
    await ApplicationStartup.ConfigureApplication(application);
}