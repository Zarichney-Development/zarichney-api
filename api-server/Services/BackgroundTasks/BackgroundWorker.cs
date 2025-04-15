using System.Threading.Channels;
using Zarichney.Services.Sessions;

namespace Zarichney.Services.BackgroundTasks;

/// <summary>
/// Internal record to hold both the work item delegate and the optional parent session
/// </summary>
public record BackgroundWorkItem(Func<IScopeContainer, CancellationToken, Task> WorkItem, Session? ParentSession);

/// <summary>
/// Interface for queuing and managing background work items
/// </summary>
public interface IBackgroundWorker
{
  void QueueBackgroundWorkAsync(Func<IScopeContainer, CancellationToken, Task> workItem, Session? parentSession = null);
  Task<BackgroundWorkItem> DequeueAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Implementation of IBackgroundWorker using Channel for work item queuing
/// </summary>
public class BackgroundWorker : IBackgroundWorker
{
  private readonly Channel<BackgroundWorkItem> _queue;

  public BackgroundWorker(int capacity)
  {
    var options = new BoundedChannelOptions(capacity)
    {
      FullMode = BoundedChannelFullMode.Wait
    };
    _queue = Channel.CreateBounded<BackgroundWorkItem>(options);
  }

  public void QueueBackgroundWorkAsync(Func<IScopeContainer, CancellationToken, Task> workItem,
    Session? parentSession = null)
  {
    ArgumentNullException.ThrowIfNull(workItem);

    var itemToQueue = new BackgroundWorkItem(workItem, parentSession);
    var valueTask = _queue.Writer.WriteAsync(itemToQueue);
    if (valueTask.IsCompleted)
    {
      valueTask.GetAwaiter().GetResult();
    }
  }

  public async Task<BackgroundWorkItem> DequeueAsync(
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
        var backgroundWorkItem = await _worker.DequeueAsync(stoppingToken);

        var scope = _scopeFactory.CreateScope();
        var scopeId = scope.Id;

        // Set the current scope in the AsyncLocal context
        ScopeHolder.CurrentScope = scope;

        // Use the parent session if provided, otherwise create a new session
        if (backgroundWorkItem.ParentSession != null)
        {
          _sessionManager.AddScopeToSession(backgroundWorkItem.ParentSession, scopeId);
          scope.SessionId = backgroundWorkItem.ParentSession.Id;
        }
        else
        {
          var session = await _sessionManager.CreateSession(scopeId);
          scope.SessionId = session.Id;
        }

        try
        {
          await backgroundWorkItem.WorkItem(scope, stoppingToken);
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
      catch (OperationCanceledException)
      {
        _logger.LogInformation("Background task was cancelled");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred executing background task");
      }
    }
  }
}