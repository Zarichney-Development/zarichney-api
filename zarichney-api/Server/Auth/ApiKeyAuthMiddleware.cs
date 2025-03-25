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

    // Fetch the user from the database to get roles and other info
    var userManager = context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
    var user = await userManager.FindByIdAsync(userId);
    
    if (user == null)
    {
      logger.Warning("User {UserId} associated with API key not found for path: {Path}", userId, path);
      context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
      await context.Response.WriteAsJsonAsync(new
      {
        error = "User associated with API key not found",
        path,
        timestamp = DateTimeOffset.UtcNow
      });
      return;
    }

    // Get user roles
    var roles = await userManager.GetRolesAsync(user);

    // If we reach here, the API key is valid and we have a user ID
    // Create and associate a ClaimsIdentity with the current User
    // IMPORTANT: Pass "true" as the second parameter to mark the identity as authenticated
    var identity = new ClaimsIdentity("ApiKey", ClaimTypes.NameIdentifier);
    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
    
    // Add email claim if available
    if (!string.IsNullOrEmpty(user.Email))
    {
      identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
    }
    
    // Add role claims
    foreach (var role in roles)
    {
      identity.AddClaim(new Claim(ClaimTypes.Role, role));
    }

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