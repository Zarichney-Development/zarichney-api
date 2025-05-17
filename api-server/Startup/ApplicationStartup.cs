using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Zarichney.Config;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services.Status;

namespace Zarichney.Startup;

/// <summary>
/// Configures the core application and HTTP request pipeline
/// </summary>
public static class ApplicationStartup
{
  /// <summary>
  /// Configures text encoding options for the application
  /// </summary>
  public static void ConfigureEncoding()
  {
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
  }

  /// <summary>
  /// Configures Kestrel web server options for the application
  /// </summary>
  public static void ConfigureKestrel(WebApplicationBuilder builder)
  {
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
      serverOptions.ConfigureEndpointDefaults(listenOptions =>
      {
        listenOptions.KestrelServerOptions.ConfigureEndpointDefaults(_ => { });
      });

      serverOptions.ListenAnyIP(5000,
        options => { options.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2; });
    });
  }

  /// <summary>
  /// Configures Cross-Origin Resource Sharing (CORS) policies
  /// </summary>
  public static void ConfigureCors(WebApplicationBuilder builder)
  {
    builder.Services.AddCors(options =>
    {
      options.AddPolicy("AllowSpecificOrigin",
        policyBuilder =>
        {
          policyBuilder
            .WithOrigins(
              "http://localhost:3001",
              "http://localhost:4200",
              "http://localhost:5000",
              "https://zarichney.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });

    builder.Services.AddRequestResponseLogger(options =>
    {
      options.LogRequests = builder.Environment.IsEnvironment("Development");
      options.LogResponses = builder.Environment.IsEnvironment("Development");
      options.SensitiveHeaders = ["Authorization", "Cookie", "X-API-Key"];
      options.RequestFilter = context => !context.Request.Path.StartsWithSegments("/api/swagger");
      options.LogDirectory = Path.Combine(builder.Environment.ContentRootPath, "Logs");
    });
  }

  /// <summary>
  /// Configures the application request pipeline with middleware and services
  /// </summary>
  public static async Task ConfigureApplication(WebApplication application)
  {
    application.UseMiddleware<RequestResponseLoggerMiddleware>();
    application.UseMiddleware<ErrorHandlingMiddleware>();

    // Serve static files to make the custom CSS available - must come before UseSwagger
    application.UseStaticFiles();

    application.UseForwardedHeaders(new ForwardedHeadersOptions
    {
      // This tells the app to trust proxy headers (like X-Forwarded-Proto set by CloudFront/ALB) indicating the original request was HTTPS
      ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    application
      .UseSwagger(c =>
      {
        Log.Information("Configuring Swagger JSON at: api/swagger/swagger.json");
        c.RouteTemplate = "api/swagger/{documentName}.json";
      })
      .UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/api/swagger/swagger.json", "Zarichney API");
        c.RoutePrefix = "api/swagger";

        // Apply custom CSS for better warning visibility
        c.InjectStylesheet("/api/swagger-ui/custom.css");
        c.InjectJavascript("/api/swagger-ui/custom.js");
      });

    application.UseCors("AllowSpecificOrigin");
    application.UseCustomAuthentication();
    application.UseSessionManagement(); // Session's user detection requires authentication, must be after

    // Check feature availability before executing endpoints
    application.UseFeatureAvailability();

    application.MapControllers();

    if (application.Environment.IsProduction())
    {
      await application.Services.GetRequiredService<IRecipeRepository>().InitializeAsync();
    }
  }
}
