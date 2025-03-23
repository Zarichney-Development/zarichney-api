using System.Net;
using ILogger = Serilog.ILogger;

namespace Zarichney.Auth;

public class ApiKeyAuthMiddleware(
  RequestDelegate next,
  ILogger logger,
  ApiKeyConfig apiKeyConfig
)
{
  private const string ApiKeyHeader = "X-Api-Key";

  public async Task InvokeAsync(HttpContext context)
  {
    var path = context.Request.Path.Value ?? string.Empty;

    // Skip API key check if the path is in the bypass list
    if (MiddlewareConfiguration.Routes.ShouldBypass(path))
    {
      await next(context);
      return;
    }

    // Skip API key check if the user is authenticated with JWT
    if (context.User.Identity?.IsAuthenticated == true)
    {
      logger.Debug("User is authenticated with JWT, skipping API key check for path: {Path}", path);
      await next(context);
      return;
    }

    if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var extractedApiKey))
    {
      logger.Warning("Request missing API key header for path: {Path}", path);
      context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
      await context.Response.WriteAsJsonAsync(new
      {
        error = "API key or JWT authentication is required",
        path,
        timestamp = DateTimeOffset.UtcNow
      });
      return;
    }

    if (!apiKeyConfig.ValidApiKeys.Contains(extractedApiKey.ToString()))
    {
      logger.Warning("Invalid API key attempted for path: {Path}", path);
      context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
      await context.Response.WriteAsJsonAsync(new
      {
        error = "Invalid API key",
        path,
        timestamp = DateTimeOffset.UtcNow
      });
      return;
    }

    // Add the API key to the HttpContext items for potential use in controllers
    context.Items["ApiKey"] = extractedApiKey.ToString();

    await next(context);
  }
}