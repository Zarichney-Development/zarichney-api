using Moq;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Sessions;
using Microsoft.Extensions.Logging;

namespace Zarichney.Tests.Framework.Mocks;

/// <summary>
/// Factory for creating mock instances of BackgroundTasks service dependencies
/// </summary>
public static class BackgroundTaskMockFactory
{
    /// <summary>
    /// Creates a mock IBackgroundWorker with default behavior
    /// </summary>
    public static Mock<IBackgroundWorker> CreateBackgroundWorker()
    {
        var mock = new Mock<IBackgroundWorker>();
        
        // Default setup - queue accepts work items without throwing
        mock.Setup(x => x.QueueBackgroundWorkAsync(It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(), It.IsAny<Session?>()))
            .Verifiable();
        
        return mock;
    }

    /// <summary>
    /// Creates a mock IBackgroundWorker with a functional queue implementation using a real Channel
    /// </summary>
    public static Mock<IBackgroundWorker> CreateFunctionalBackgroundWorker()
    {
        var mock = new Mock<IBackgroundWorker>();
        var actualWorker = new BackgroundWorker(100); // Use real implementation internally
        
        mock.Setup(x => x.QueueBackgroundWorkAsync(It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(), It.IsAny<Session?>()))
            .Callback<Func<IScopeContainer, CancellationToken, Task>, Session?>((work, session) =>
            {
                actualWorker.QueueBackgroundWorkAsync(work, session);
            });
        
        mock.Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(token => actualWorker.DequeueAsync(token));
        
        return mock;
    }

    /// <summary>
    /// Creates a mock IBackgroundWorker that returns specified work items when dequeued
    /// </summary>
    public static Mock<IBackgroundWorker> CreateBackgroundWorkerWithItems(params BackgroundWorkItem[] items)
    {
        var mock = new Mock<IBackgroundWorker>();
        var queue = new Queue<BackgroundWorkItem>(items);
        
        mock.Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => queue.Count > 0 ? queue.Dequeue() : throw new InvalidOperationException("No more items"));
        
        return mock;
    }

    /// <summary>
    /// Creates a mock IScopeFactory with default behavior
    /// </summary>
    public static Mock<IScopeFactory> CreateScopeFactory(IScopeContainer? scope = null)
    {
        var mock = new Mock<IScopeFactory>();
        var mockScope = scope ?? CreateScopeContainer().Object;
        
        mock.Setup(x => x.CreateScope())
            .Returns(mockScope);
        
        return mock;
    }
    
    /// <summary>
    /// Creates a mock IScopeContainer with default values
    /// </summary>
    public static Mock<IScopeContainer> CreateScopeContainer()
    {
        var mock = new Mock<IScopeContainer>();
        mock.SetupProperty(x => x.SessionId);
        mock.SetupGet(x => x.Id).Returns(Guid.NewGuid());
        return mock;
    }

    /// <summary>
    /// Creates a mock IScopeContainer with configurable properties
    /// </summary>
    public static Mock<IScopeContainer> CreateScopeContainer(Guid? scopeId = null, Guid? sessionId = null)
    {
        var mock = new Mock<IScopeContainer>();
        
        mock.SetupProperty(x => x.SessionId, sessionId);
        mock.SetupGet(x => x.Id).Returns(scopeId ?? Guid.NewGuid());
        
        return mock;
    }

    /// <summary>
    /// Creates a mock ISessionManager with default behavior
    /// </summary>
    public static Mock<ISessionManager> CreateSessionManager()
    {
        var mock = new Mock<ISessionManager>();
        
        // Default behavior - create new sessions and handle scope operations
        mock.Setup(x => x.CreateSession(It.IsAny<Guid>()))
            .ReturnsAsync((Guid scopeId) =>
            {
                var idProperty = typeof(Session).GetProperty(nameof(Session.Id));
                var createdAtProperty = typeof(Session).GetProperty(nameof(Session.CreatedAt));
                var session = new Session();
                idProperty?.SetValue(session, Guid.NewGuid());
                createdAtProperty?.SetValue(session, DateTime.UtcNow);
                return session;
            });
        
        mock.Setup(x => x.AddScopeToSession(It.IsAny<Session>(), It.IsAny<Guid>()))
            .Verifiable();
        
        mock.Setup(x => x.EndSession(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        
        return mock;
    }

    /// <summary>
    /// Creates a mock ISessionManager that tracks session operations
    /// </summary>
    public static Mock<ISessionManager> CreateTrackingSessionManager(out List<Guid> createdSessionIds, out List<Guid> endedScopeIds)
    {
        var mock = CreateSessionManager();
        var created = new List<Guid>();
        var ended = new List<Guid>();
        
        mock.Setup(x => x.CreateSession(It.IsAny<Guid>()))
            .ReturnsAsync((Guid scopeId) =>
            {
                var sessionId = Guid.NewGuid();
                created.Add(sessionId);
                var session = new Session();
                var idProperty = typeof(Session).GetProperty(nameof(Session.Id));
                var createdAtProperty = typeof(Session).GetProperty(nameof(Session.CreatedAt));
                idProperty?.SetValue(session, sessionId);
                createdAtProperty?.SetValue(session, DateTime.UtcNow);
                return session;
            });
        
        mock.Setup(x => x.EndSession(It.IsAny<Guid>()))
            .Returns<Guid>((scopeId) =>
            {
                ended.Add(scopeId);
                return Task.CompletedTask;
            });
        
        createdSessionIds = created;
        endedScopeIds = ended;
        return mock;
    }

    /// <summary>
    /// Creates a mock ISessionManager that throws exceptions
    /// </summary>
    public static Mock<ISessionManager> CreateFailingSessionManager(Exception exception)
    {
        var mock = new Mock<ISessionManager>();
        
        mock.Setup(x => x.CreateSession(It.IsAny<Guid>()))
            .ThrowsAsync(exception);
        
        mock.Setup(x => x.EndSession(It.IsAny<Guid>()))
            .ThrowsAsync(exception);
        
        mock.Setup(x => x.AddScopeToSession(It.IsAny<Session>(), It.IsAny<Guid>()))
            .Throws(exception);
        
        return mock;
    }

    /// <summary>
    /// Creates a mock ILogger for BackgroundTaskService
    /// </summary>
    public static Mock<ILogger<BackgroundTaskService>> CreateLogger()
    {
        return new Mock<ILogger<BackgroundTaskService>>();
    }
}