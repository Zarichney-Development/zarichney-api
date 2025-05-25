using ILogger = Serilog.ILogger;

namespace Zarichney.Services.Sessions;

/// <summary>
/// Background service responsible for cleaning up expired sessions.
/// Runs on a configured interval and removes sessions that have exceeded their expiration time.
/// </summary>
public class SessionCleanupService : BackgroundService
{
  private readonly ISessionManager _sessionManager;
  private readonly ILogger _logger;
  private readonly SessionConfig _config;
  private readonly SemaphoreSlim _cleanupLock;

  public SessionCleanupService(
    ISessionManager sessionManager,
    SessionConfig config,
    ILogger logger)
  {
    _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _logger = logger.ForContext<SessionCleanupService>()
              ?? throw new ArgumentNullException(nameof(logger));
    _cleanupLock = new SemaphoreSlim(_config.MaxConcurrentCleanup, _config.MaxConcurrentCleanup);
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.Information("Session cleanup service started");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        await CleanupExpiredSessions(stoppingToken);
        await Task.Delay(_config.CleanupIntervalMins * 60 * 1000, stoppingToken);
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        // Normal shutdown, don't log as error
        break;
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "Error occurred during session cleanup");
        // Wait before retrying after error
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
      }
    }

    _logger.Information("Session cleanup service stopped");
  }

  private async Task CleanupExpiredSessions(CancellationToken cancellationToken)
  {
    var now = DateTime.UtcNow;
    var expiredSessions = _sessionManager.Sessions
      .Where(kvp => kvp.Value.ExpiresAt < now)
      .Select(kvp => kvp.Value)
      .ToList();

    if (expiredSessions.Count == 0)
    {
      return;
    }

    _logger.Information("Found {Count} expired sessions to clean up", expiredSessions.Count);

    var cleanupTasks = expiredSessions.Select(async session =>
    {
      try
      {
        await _cleanupLock.WaitAsync(cancellationToken);
        try
        {
          await CleanupSession(session);
        }
        finally
        {
          _cleanupLock.Release();
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "Error cleaning up session {SessionId}", session.Id);
      }
    });

    await Task.WhenAll(cleanupTasks);
  }

  private async Task CleanupSession(Session session)
  {
    if (session.Order != null)
    {
      await _sessionManager.EndSession(session.Order.OrderId);
      _logger.Information("Cleaned up expired session {SessionId} for order {OrderId}",
        session.Id, session.Order.OrderId);
    }
    else
    {
      if (_sessionManager.Sessions.TryRemove(session.Id, out _))
      {
        _logger.Information(
          "Removed expired session {SessionId} without associated order",
          session.Id);
      }
    }
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.Information("Session cleanup service is stopping");
    try
    {
      await base.StopAsync(cancellationToken);
    }
    finally
    {
      _cleanupLock.Dispose();
      _logger.Information("Session cleanup service stopped");
    }
  }

  public override void Dispose()
  {
    _cleanupLock.Dispose();
    base.Dispose();
    GC.SuppressFinalize(this);
  }
}
