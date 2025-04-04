using Zarichney.Server.Services.Auth;
using System.Security.Claims;

namespace Zarichney.Server.Services.Sessions;

/// <summary>
/// Middleware that ensures each request is associated with a session.
/// Handles session creation, lookup, and cleanup based on authenticated user or creates anonymous sessions.
/// </summary>
public class SessionMiddleware(
    RequestDelegate next,
    ISessionManager sessionManager,
    ILogger<SessionMiddleware> logger,
    IScopeFactory scopeFactory)
{

  public async Task InvokeAsync(HttpContext context)
  {
    var path = context.Request.Path.Value ?? string.Empty;

    if (MiddlewareConfiguration.Routes.ShouldBypass(path))
    {
      await next(context);
      return;
    }

    var scope = scopeFactory.CreateScope();
    
    // TODO: restore X-Session-Id functionality. I believe it's still required for the ai service (completion endpoint)

    // Get authenticated user ID if present (from JWT or API Key)
    var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
    string? userId = null;

    if (isAuthenticated)
    {
      // Get user ID from ClaimTypes.NameIdentifier claim
      userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      logger.LogInformation("Request authenticated with user ID: {UserId}", userId);
    }
    // Check for API key authentication in HttpContext.Items
    else if (context.Items.TryGetValue("ApiKeyUserId", out var apiKeyUserId) && apiKeyUserId is string apiKeyUserIdStr)
    {
      isAuthenticated = true;
      userId = apiKeyUserIdStr;
      logger.LogInformation("Request authenticated with API key for user ID: {UserId}", userId);
    }

    // Find existing session for user or create a new one
    Session session;

    if (isAuthenticated && !string.IsNullOrEmpty(userId))
    {
      // Find session by user ID or create new one
      session = await sessionManager.GetSessionByUserId(userId, scope.Id);

      // Set the UserId property on the session to ensure it's preserved
      session.UserId ??= userId;

      logger.LogInformation("Using session {SessionId} for user {UserId}", session.Id, userId);
    }
    else
    {
      // Create an anonymous session
      session = await sessionManager.CreateSession(scope.Id);
      
      if (!string.IsNullOrEmpty(userId))
      {
        if (session.UserId != null && session.UserId != userId)
        {
          logger.LogWarning("Session {SessionId} already has a different user ID {ExistingUserId}, ignoring new user ID {NewUserId}", session.Id, session.UserId, userId);
        }
        
        session.UserId ??= userId;
      }
      logger.LogInformation("Created new anonymous session {SessionId}", session.Id);
    }

    // Add API key to session if present
    if (context.Items.TryGetValue("ApiKey", out var apiKey) && apiKey is string apiKeyStr)
    {
      session.ApiKeyValue = apiKeyStr;
    }

    sessionManager.AddScopeToSession(session, scope.Id);

    scope.SessionId = session.Id;

    context.Features.Set(scope);

    try
    {
      using (Serilog.Context.LogContext.PushProperty("ScopeId", scope.Id))
      using (Serilog.Context.LogContext.PushProperty("SessionId", session.Id))
      using (Serilog.Context.LogContext.PushProperty("UserId", userId))
      {
        await next(context);
      }
    }
    finally
    {
      sessionManager.RemoveScopeFromSession(scope.Id);

      if (session is { ExpiresImmediately: true, Scopes.IsEmpty: true }
          || session.ExpiresAt < DateTime.UtcNow)
      {
        try
        {
          await sessionManager.EndSession(session);
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error ending session {SessionId}", session.Id);
        }
      }
    }
  }
}