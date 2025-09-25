using Microsoft.Extensions.Logging;
using Moq;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Sessions;

namespace Zarichney.Server.Tests.Framework.Mocks;

/// <summary>
/// Factory for creating pre-configured mocks for background task services.
/// Provides standard setups for IBackgroundWorker, IScopeFactory, ISessionManager,
/// and related services to reduce duplication across background task tests.
/// </summary>
public static class BackgroundTaskMockFactory
{
    /// <summary>
    /// Creates a default mock IBackgroundWorker that successfully queues and dequeues items.
    /// </summary>
    public static Mock<IBackgroundWorker> CreateDefaultBackgroundWorker()
    {
        var mock = new Mock<IBackgroundWorker>();
        var queue = new Queue<BackgroundWorkItem>();

        mock.Setup(x => x.QueueBackgroundWorkAsync(
                It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
                It.IsAny<Session?>()))
            .Callback<Func<IScopeContainer, CancellationToken, Task>, Session?>((workItem, session) =>
            {
                queue.Enqueue(new BackgroundWorkItem(workItem, session));
            });

        mock.Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => queue.Count > 0 ? queue.Dequeue() : null!);

        return mock;
    }

    /// <summary>
    /// Creates a mock IBackgroundWorker that simulates capacity limitations.
    /// </summary>
    public static Mock<IBackgroundWorker> CreateCapacityLimitedBackgroundWorker(int capacity)
    {
        var mock = new Mock<IBackgroundWorker>();
        var queue = new Queue<BackgroundWorkItem>();

        mock.Setup(x => x.QueueBackgroundWorkAsync(
                It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
                It.IsAny<Session?>()))
            .Callback<Func<IScopeContainer, CancellationToken, Task>, Session?>((workItem, session) =>
            {
                if (queue.Count >= capacity)
                {
                    throw new InvalidOperationException($"Queue is at capacity ({capacity})");
                }
                queue.Enqueue(new BackgroundWorkItem(workItem, session));
            });

        mock.Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => queue.Count > 0 ? queue.Dequeue() : null!);

        return mock;
    }

    /// <summary>
    /// Creates a mock IBackgroundWorker that throws exceptions on dequeue.
    /// </summary>
    public static Mock<IBackgroundWorker> CreateFailingBackgroundWorker(Exception exceptionToThrow)
    {
        var mock = new Mock<IBackgroundWorker>();

        mock.Setup(x => x.QueueBackgroundWorkAsync(
                It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
                It.IsAny<Session?>()))
            .Verifiable();

        mock.Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exceptionToThrow);

        return mock;
    }

    /// <summary>
    /// Creates a mock IBackgroundWorker with sequential dequeue behavior.
    /// </summary>
    public static Mock<IBackgroundWorker> CreateSequentialBackgroundWorker(params BackgroundWorkItem[] items)
    {
        var mock = new Mock<IBackgroundWorker>();
        var setupSequence = mock.SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()));

        foreach (var item in items)
        {
            setupSequence = setupSequence.ReturnsAsync(item);
        }

        setupSequence.ThrowsAsync(new OperationCanceledException());

        mock.Setup(x => x.QueueBackgroundWorkAsync(
                It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
                It.IsAny<Session?>()))
            .Verifiable();

        return mock;
    }

    /// <summary>
    /// Creates a default mock IScopeFactory with standard configuration.
    /// </summary>
    public static Mock<IScopeFactory> CreateDefaultScopeFactory(Guid? scopeId = null)
    {
        var mock = new Mock<IScopeFactory>();
        var actualScopeId = scopeId ?? Guid.NewGuid();
        var mockScope = CreateMockScope(actualScopeId);

        mock.Setup(x => x.CreateScope(It.IsAny<IScopeContainer?>()))
            .Returns(mockScope.Object);

        return mock;
    }

    /// <summary>
    /// Creates a mock IScopeFactory that returns different scopes for each call.
    /// </summary>
    public static Mock<IScopeFactory> CreateMultiScopeFactory(params Guid[] scopeIds)
    {
        var mock = new Mock<IScopeFactory>();
        var scopes = scopeIds.Select(id => CreateMockScope(id).Object).ToArray();
        var index = 0;

        mock.Setup(x => x.CreateScope(It.IsAny<IScopeContainer?>()))
            .Returns(() => scopes[index++ % scopes.Length]);

        return mock;
    }

    /// <summary>
    /// Creates a default mock ISessionManager with standard session handling.
    /// </summary>
    public static Mock<ISessionManager> CreateDefaultSessionManager()
    {
        var mock = new Mock<ISessionManager>();
        var sessions = new Dictionary<Guid, Session>();

        mock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync((Guid scopeId, TimeSpan? _) =>
            {
                var session = new SessionBuilder().Build();
                sessions[scopeId] = session;
                return session;
            });

        mock.Setup(x => x.AddScopeToSession(It.IsAny<Session>(), It.IsAny<Guid>()))
            .Callback<Session, Guid>((session, scopeId) =>
            {
                sessions[scopeId] = session;
            });

        mock.Setup(x => x.EndSession(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask)
            .Callback<Guid>(scopeId =>
            {
                sessions.Remove(scopeId);
            });

        return mock;
    }

    /// <summary>
    /// Creates a mock ISessionManager that throws exceptions.
    /// </summary>
    public static Mock<ISessionManager> CreateFailingSessionManager(Exception exceptionToThrow)
    {
        var mock = new Mock<ISessionManager>();

        mock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
            .ThrowsAsync(exceptionToThrow);

        mock.Setup(x => x.AddScopeToSession(It.IsAny<Session>(), It.IsAny<Guid>()))
            .Throws(exceptionToThrow);

        mock.Setup(x => x.EndSession(It.IsAny<Guid>()))
            .ThrowsAsync(exceptionToThrow);

        return mock;
    }

    /// <summary>
    /// Creates a mock IScopeContainer with configurable properties.
    /// </summary>
    public static Mock<IScopeContainer> CreateMockScope(Guid scopeId, Guid? sessionId = null)
    {
        var mockScope = new Mock<IScopeContainer>();
        mockScope.SetupProperty(x => x.SessionId, sessionId);
        mockScope.SetupGet(x => x.Id).Returns(scopeId);
        return mockScope;
    }

    /// <summary>
    /// Creates a mock Logger for BackgroundTaskService.
    /// </summary>
    public static Mock<ILogger<BackgroundTaskService>> CreateBackgroundTaskServiceLogger()
    {
        var mock = new Mock<ILogger<BackgroundTaskService>>();

        // Setup to capture log calls for verification
        mock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        return mock;
    }

    /// <summary>
    /// Creates a mock work item that tracks execution.
    /// </summary>
    public static (Func<IScopeContainer, CancellationToken, Task> WorkItem, Mock<Action<IScopeContainer>> ExecutionTracker)
        CreateTrackableWorkItem(Task? taskToReturn = null)
    {
        var executionTracker = new Mock<Action<IScopeContainer>>();
        taskToReturn ??= Task.CompletedTask;

        Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
        {
            executionTracker.Object(scope);
            await taskToReturn;
        };

        return (workItem, executionTracker);
    }

    /// <summary>
    /// Creates a work item that throws an exception.
    /// </summary>
    public static Func<IScopeContainer, CancellationToken, Task> CreateFailingWorkItem(Exception exceptionToThrow)
    {
        return async (scope, ct) =>
        {
            await Task.CompletedTask;
            throw exceptionToThrow;
        };
    }

    /// <summary>
    /// Creates a work item that respects cancellation.
    /// </summary>
    public static Func<IScopeContainer, CancellationToken, Task> CreateCancellableWorkItem(int delayMs = 100)
    {
        return async (scope, ct) =>
        {
            await Task.Delay(delayMs, ct);
        };
    }
}