using System.Threading.Channels;
using Zarichney.Services.Sessions;

namespace Zarichney.Services;

/// <summary>
/// Interface for queuing and managing background work items
/// </summary>
public interface IBackgroundWorker
{
  void QueueBackgroundWorkAsync(Func<IScopeContainer, CancellationToken, Task> workItem);
  Task<Func<IScopeContainer, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Implementation of IBackgroundWorker using Channel for work item queuing
/// </summary>
public class BackgroundWorker : IBackgroundWorker
{
  private readonly Channel<Func<IScopeContainer, CancellationToken, Task>> _queue;

  public BackgroundWorker(int capacity)
  {
    var options = new BoundedChannelOptions(capacity)
    {
      FullMode = BoundedChannelFullMode.Wait
    };
    _queue = Channel.CreateBounded<Func<IScopeContainer, CancellationToken, Task>>(options);
  }

  public void QueueBackgroundWorkAsync(Func<IScopeContainer, CancellationToken, Task> workItem)
  {
    ArgumentNullException.ThrowIfNull(workItem);

    var valueTask = _queue.Writer.WriteAsync(workItem);
    if (valueTask.IsCompleted)
    {
      valueTask.GetAwaiter().GetResult();
    }
  }

  public async Task<Func<IScopeContainer, CancellationToken, Task>> DequeueAsync(
    CancellationToken cancellationToken)
  {
    return await _queue.Reader.ReadAsync(cancellationToken);
  }
}

/// <summary>
/// Background service that processes queued work items with proper scope and session management
/// </summary>
public class BackgroundTaskService(
  IBackgroundWorker worker,
  ILogger<BackgroundTaskService> logger,
  IScopeFactory scopeFactory,
  ISessionManager sessionManager)
  : BackgroundService
{
  private readonly ILogger<BackgroundTaskService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  private readonly IBackgroundWorker _worker = worker ?? throw new ArgumentNullException(nameof(worker));
  private readonly IScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

  private readonly ISessionManager _sessionManager =
    sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var workItem = await _worker.DequeueAsync(stoppingToken);

        var scope = _scopeFactory.CreateScope();
        var scopeId = scope.Id;

        // Set the current scope in the AsyncLocal context
        ScopeHolder.CurrentScope = scope;

        // Create session for this background work
        var session = await _sessionManager.CreateSession(scopeId);
        scope.SessionId = session.Id;

        try
        {
          await workItem(scope, stoppingToken);
        }
        finally
        {
          try
          {
            // Clear the scope from the AsyncLocal context
            ScopeHolder.CurrentScope = null;

            // Always try to end the session
            if (scope.SessionId.HasValue)
            {
              await _sessionManager.EndSession(scopeId);
            }
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "Error ending session for background task. ScopeId: {ScopeId}", scopeId);
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred executing background task");
      }
    }
  }
}