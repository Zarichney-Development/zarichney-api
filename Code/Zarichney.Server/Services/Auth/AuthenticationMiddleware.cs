using System.Net;
using Microsoft.AspNetCore.Authorization;
using ILogger = Serilog.ILogger;

namespace Zarichney.Services.Auth;

public class AuthenticationMiddleware(
  RequestDelegate next,
  ILogger logger
)
{
  private const string ApiKeyHeader = "X-Api-Key";

  public async Task InvokeAsync(HttpContext context)
  {
    var path = context.Request.Path.Value ?? string.Empty;

    // Skip API key check if the user is authenticated with JWT
    if (context.User.Identity?.IsAuthenticated == true)
    {
      logger.Debug("User is authenticated with JWT, skipping API key check for path: {Path}", path);
      await next(context);
      return;
    }

    // Check if the endpoint has the [AllowAnonymous] attribute
    var endpoint = context.GetEndpoint();
    var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>();
    if (allowAnonymous != null)
    {
      logger.Debug("Endpoint has [AllowAnonymous] attribute, skipping authentication for path: {Path}", path);
      await next(context);
      return;
    }

    // Check for API key in header
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

    // Get the scoped service from the request services
    var apiKeyService = context.RequestServices.GetRequiredService<IApiKeyService>();

    // Authenticate using API key
    var authenticated = await apiKeyService.AuthenticateWithApiKey(context, extractedApiKey!, logger);

    if (authenticated)
    {
      await next(context);
    }
  }
}
