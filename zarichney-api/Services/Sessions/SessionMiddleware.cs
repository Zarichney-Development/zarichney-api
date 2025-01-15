using Zarichney.Middleware;
using ILogger = Serilog.ILogger;

namespace Zarichney.Services.Sessions;

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

  private readonly ISessionManager _sessionManager =
    sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

  private readonly ILogger _logger = logger.ForContext<SessionMiddleware>()
                                     ?? throw new ArgumentNullException(nameof(logger));

  private readonly IScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

  public async Task InvokeAsync(HttpContext context)
  {
    var path = context.Request.Path.Value ?? string.Empty;

    if (MiddlewareConfiguration.Routes.ShouldBypass(path))
    {
      await _next(context);
      return;
    }

    var scope = _scopeFactory.CreateScope();

    var session = context.Request.Headers.TryGetValue("X-Session-Id", out var sessionIdHeader)
                  && Guid.TryParse(sessionIdHeader, out var sessionId)
      ? await _sessionManager.GetSession(sessionId)
      : await _sessionManager.CreateSession(scope.Id);

    _sessionManager.AddScopeToSession(session, scope.Id);

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
      _sessionManager.RemoveScopeFromSession(scope.Id);

      if (session is { ExpiresImmediately: true, Scopes.IsEmpty: true }
          || session.ExpiresAt < DateTime.UtcNow)
      {
        try
        {
          await _sessionManager.EndSession(session);
        }
        catch (Exception ex)
        {
          _logger.Error(ex, "Error ending session {SessionId}", session.Id);
        }
      }
    }
  }
}