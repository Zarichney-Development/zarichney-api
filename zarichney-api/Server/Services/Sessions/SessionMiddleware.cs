using Zarichney.Server.Auth;
using ILogger = Serilog.ILogger;
using System.Security.Claims;

namespace Zarichney.Server.Services.Sessions;

/// <summary>
/// Middleware that ensures each request is associated with a session.
/// Handles session creation, lookup, and cleanup based on authenticated user or creates anonymous sessions.
/// </summary>
public class SessionMiddleware(
  RequestDelegate next,
  ISessionManager sessionManager,
  ILogger logger,
  IScopeFactory scopeFactory)
{
  private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

  private readonly ILogger _logger = logger.ForContext<SessionMiddleware>()
                                     ?? throw new ArgumentNullException(nameof(logger));

  public async Task InvokeAsync(HttpContext context)
  {
    var path = context.Request.Path.Value ?? string.Empty;

    if (MiddlewareConfiguration.Routes.ShouldBypass(path))
    {
      await _next(context);
      return;
    }

    var scope = scopeFactory.CreateScope();

    // Get authenticated user ID if present
    var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
    string? userId = null;

    if (isAuthenticated)
    {
      // Get user ID from ClaimTypes.NameIdentifier claim
      userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      _logger.Information("Request authenticated with user ID: {UserId}", userId);
    }

    // Find existing session for user or create a new one
    Session session;

    if (isAuthenticated && !string.IsNullOrEmpty(userId))
    {
      // Find session by user ID or create new one
      var existingSession = await sessionManager.GetSessionByUserId(userId, scope.Id);
      session = existingSession;
      _logger.Information("Using existing session {SessionId} for user {UserId}", session.Id, userId);
    }
    else
    {
      // Create an anonymous session
      session = await sessionManager.CreateSession(scope.Id);
      _logger.Information("Created new anonymous session {SessionId}", session.Id);
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
        await _next(context);
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
          _logger.Error(ex, "Error ending session {SessionId}", session.Id);
        }
      }
    }
  }
}