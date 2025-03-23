using Zarichney.Server.Auth;
using ILogger = Serilog.ILogger;

namespace Zarichney.Server.Services.Sessions;

/// <summary>
/// Middleware that ensures each request is associated with a session.
/// Handles session creation, lookup, and cleanup based on request headers.
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

    var session = context.Request.Headers.TryGetValue("X-Session-Id", out var sessionIdHeader)
                  && Guid.TryParse(sessionIdHeader, out var sessionId)
      ? await sessionManager.GetSession(sessionId)
      : await sessionManager.CreateSession(scope.Id);

    sessionManager.AddScopeToSession(session, scope.Id);

    scope.SessionId = session.Id;

    context.Features.Set(scope);

    try
    {
      using (Serilog.Context.LogContext.PushProperty("ScopeId", scope.Id))
      using (Serilog.Context.LogContext.PushProperty("SessionId", session.Id))
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