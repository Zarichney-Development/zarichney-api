using System.Net;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace Zarichney.Server.Auth;

public class ApiKeyAuthMiddleware(
    RequestDelegate next,
    ILogger logger
    ) {
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

    // Validate the API key and get associated user ID
    var apiKeyService = context.RequestServices.GetRequiredService<IApiKeyService>();
    var (isValid, userId) = await apiKeyService.ValidateApiKey(extractedApiKey.ToString());

    if (!isValid || string.IsNullOrEmpty(userId))
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

    // If we reach here, the API key is valid and we have a user ID
    // Create and associate a ClaimsIdentity with the current User
    var identity = new ClaimsIdentity("ApiKey");
    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));

    // Store the original principal if it exists
    var originalPrincipal = context.User;

    // Create a new ClaimsPrincipal with both identities (if original had any)
    var identities = new List<ClaimsIdentity> { identity };
    if (originalPrincipal.Identity != null)
    {
      identities.AddRange(originalPrincipal.Identities);
    }

    // Set the new principal with all identities
    context.User = new ClaimsPrincipal(identities);

    // Add the API key to the HttpContext items for potential use in controllers
    context.Items["ApiKey"] = extractedApiKey.ToString();
    context.Items["ApiKeyUserId"] = userId;

    logger.Information("Request authenticated with API key for user {UserId} on path: {Path}", userId, path);

    await next(context);
  }
}