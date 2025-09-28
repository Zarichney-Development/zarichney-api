using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Server.Tests.Framework.Mocks;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Sessions;

namespace Zarichney.Server.Tests.Unit.Services.BackgroundTasks;

/// <summary>
/// Advanced unit tests for BackgroundTaskService covering complex scenarios,
/// edge cases, and concurrency patterns not covered in the basic test suite.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "BackgroundTasks")]
public class BackgroundTaskServiceAdvancedTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public BackgroundTaskServiceAdvancedTests()
    {
        _fixture = new Fixture();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    #region Advanced Session Management Tests

    [Fact]
    public async Task ExecuteAsync_WithSessionCreationFailure_LogsAndContinues()
    {
        // Arrange
        var mockWorker = BackgroundTaskMockFactory.CreateDefaultBackgroundWorker();
        var mockLogger = BackgroundTaskMockFactory.CreateBackgroundTaskServiceLogger();
        var mockScopeFactory = BackgroundTaskMockFactory.CreateDefaultScopeFactory();
        var mockSessionManager = new Mock<ISessionManager>();

        var sessionException = new InvalidOperationException("Session creation failed");
        mockSessionManager
            .Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
            .ThrowsAsync(sessionException);

        mockSessionManager
            .Setup(x => x.EndSession(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
        {
            await Task.CompletedTask;
        };

        mockWorker.Object.QueueBackgroundWorkAsync(workItem);

        var sut = new BackgroundTaskService(
            mockWorker.Object,
            mockLogger.Object,
            mockScopeFactory.Object,
            mockSessionManager.Object);

        // Act
        _cancellationTokenSource.CancelAfter(100);
        await sut.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(50);

        // Assert
        // Even if session creation fails, the service should log the error and continue
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred executing background task")),
                sessionException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "session creation failure should be logged as an error");
    }

    [Fact]
    public async Task ExecuteAsync_WithNestedSessions_HandlesCorrectly()
    {
        // Arrange
        var parentSession = new SessionBuilder().WithId(Guid.NewGuid()).Build();
        var childSession = new SessionBuilder().WithId(Guid.NewGuid()).Build();

        var mockWorker = BackgroundTaskMockFactory.CreateDefaultBackgroundWorker();
        var mockLogger = BackgroundTaskMockFactory.CreateBackgroundTaskServiceLogger();
        var mockScopeFactory = BackgroundTaskMockFactory.CreateMultiScopeFactory(Guid.NewGuid(), Guid.NewGuid());
        var mockSessionManager = BackgroundTaskMockFactory.CreateDefaultSessionManager();

        // First work item with parent session
        Func<IScopeContainer, CancellationToken, Task> parentWorkItem = async (scope, ct) =>
        {
            await Task.CompletedTask;
        };

        // Second work item with child session
        Func<IScopeContainer, CancellationToken, Task> childWorkItem = async (scope, ct) =>
        {
            await Task.CompletedTask;
        };

        mockWorker.Object.QueueBackgroundWorkAsync(parentWorkItem, parentSession);
        mockWorker.Object.QueueBackgroundWorkAsync(childWorkItem, childSession);

        var sut = new BackgroundTaskService(
            mockWorker.Object,
            mockLogger.Object,
            mockScopeFactory.Object,
            mockSessionManager.Object);

        // Act
        _cancellationTokenSource.CancelAfter(150);
        await sut.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(100);

        // Assert
        mockSessionManager.Verify(
            x => x.AddScopeToSession(It.IsAny<Session>(), It.IsAny<Guid>()),
            Times.Exactly(2),
            "both sessions should be handled independently");
    }

    #endregion

    #region Scope Management Tests

    [Fact]
    public async Task ExecuteAsync_WithScopeHolderContext_SetsScopeCorrectly()
    {
        // Arrange
        var mockWorker = BackgroundTaskMockFactory.CreateDefaultBackgroundWorker();
        var mockLogger = BackgroundTaskMockFactory.CreateBackgroundTaskServiceLogger();
        var scopeId = Guid.NewGuid();
        var mockScope = BackgroundTaskMockFactory.CreateMockScope(scopeId);
        var mockScopeFactory = new Mock<IScopeFactory>();
        mockScopeFactory
            .Setup(x => x.CreateScope(It.IsAny<IScopeContainer?>()))
            .Returns(mockScope.Object);

        var mockSessionManager = BackgroundTaskMockFactory.CreateDefaultSessionManager();

        IScopeContainer? capturedScope = null;
        Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
        {
            // Capture the current scope from AsyncLocal context
            capturedScope = ScopeHolder.CurrentScope;
            await Task.CompletedTask;
        };

        mockWorker.Object.QueueBackgroundWorkAsync(workItem);

        var sut = new BackgroundTaskService(
            mockWorker.Object,
            mockLogger.Object,
            mockScopeFactory.Object,
            mockSessionManager.Object);

        // Act
        _cancellationTokenSource.CancelAfter(100);
        await sut.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(50);

        // Assert
        capturedScope.Should().NotBeNull("scope should be set in AsyncLocal context during execution");
        capturedScope?.Id.Should().Be(scopeId, "scope ID should match the created scope");
    }

    [Fact]
    public async Task ExecuteAsync_AfterWorkItemCompletion_ClearsScopeContext()
    {
        // Arrange
        var mockWorker = BackgroundTaskMockFactory.CreateDefaultBackgroundWorker();
        var mockLogger = BackgroundTaskMockFactory.CreateBackgroundTaskServiceLogger();
        var mockScopeFactory = BackgroundTaskMockFactory.CreateDefaultScopeFactory();
        var mockSessionManager = BackgroundTaskMockFactory.CreateDefaultSessionManager();

        var scopeDuringExecution = default(IScopeContainer);
        var scopeAfterCompletion = default(IScopeContainer);

        Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
        {
            scopeDuringExecution = ScopeHolder.CurrentScope;
            await Task.CompletedTask;
        };

        mockWorker.Object.QueueBackgroundWorkAsync(workItem);

        var sut = new BackgroundTaskService(
            mockWorker.Object,
            mockLogger.Object,
            mockScopeFactory.Object,
            mockSessionManager.Object);

        // Act
        _cancellationTokenSource.CancelAfter(100);
        var serviceTask = sut.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(50);

        // Check scope after work item completion
        scopeAfterCompletion = ScopeHolder.CurrentScope;

        await serviceTask;

        // Assert
        scopeDuringExecution.Should().NotBeNull("scope should be set during work item execution");
        scopeAfterCompletion.Should().BeNull("scope should be cleared after work item completion");
    }

    #endregion

    #region Concurrent Processing Tests

    [Fact]
    public async Task ExecuteAsync_WithRapidWorkItemQueuing_ProcessesAllItems()
    {
        // Arrange
        var processedCount = 0;
        var lockObj = new object();

        var mockWorker = new Mock<IBackgroundWorker>();
        var workItems = new Queue<BackgroundWorkItem>();

        // Setup rapid queuing
        for (int i = 0; i < 20; i++)
        {
            Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
            {
                lock (lockObj)
                {
                    processedCount++;
                }
                await Task.CompletedTask;
            };
            workItems.Enqueue(new BackgroundWorkItem(workItem, null));
        }

        mockWorker
            .Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => workItems.Count > 0 ? workItems.Dequeue() : throw new OperationCanceledException());

        var mockLogger = BackgroundTaskMockFactory.CreateBackgroundTaskServiceLogger();
        var mockScopeFactory = BackgroundTaskMockFactory.CreateDefaultScopeFactory();
        var mockSessionManager = BackgroundTaskMockFactory.CreateDefaultSessionManager();

        var sut = new BackgroundTaskService(
            mockWorker.Object,
            mockLogger.Object,
            mockScopeFactory.Object,
            mockSessionManager.Object);

        // Act
        _cancellationTokenSource.CancelAfter(500);
        await sut.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);

        // Assert
        processedCount.Should().Be(20, "all rapidly queued items should be processed");
    }

    #endregion

    #region Error Recovery Tests

    [Fact]
    public async Task ExecuteAsync_WithIntermittentFailures_ContinuesProcessing()
    {
        // Arrange
        var successCount = 0;
        var errorCount = 0;
        var lockObj = new object();

    List<BackgroundWorkItem> workItems = [];

        // Mix of successful and failing work items
        for (int i = 0; i < 10; i++)
        {
            var shouldFail = i % 3 == 0;
            Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
            {
                if (shouldFail)
                {
                    lock (lockObj) { errorCount++; }
                    throw new InvalidOperationException("Planned failure");
                }
                lock (lockObj) { successCount++; }
                await Task.CompletedTask;
            };
            workItems.Add(new BackgroundWorkItem(workItem, null));
        }

        var mockWorker = BackgroundTaskMockFactory.CreateSequentialBackgroundWorker(workItems.ToArray());
        var mockLogger = BackgroundTaskMockFactory.CreateBackgroundTaskServiceLogger();
        var mockScopeFactory = BackgroundTaskMockFactory.CreateDefaultScopeFactory();
        var mockSessionManager = BackgroundTaskMockFactory.CreateDefaultSessionManager();

        var sut = new BackgroundTaskService(
            mockWorker.Object,
            mockLogger.Object,
            mockScopeFactory.Object,
            mockSessionManager.Object);

        // Act
        _cancellationTokenSource.CancelAfter(300);
        await sut.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(200);

        // Assert
        successCount.Should().Be(6, "successful work items should complete");
        errorCount.Should().Be(4, "failing work items should be counted");
        mockSessionManager.Verify(
            x => x.EndSession(It.IsAny<Guid>()),
            Times.Exactly(10),
            "session should end for all work items, including failed ones");
    }

    [Fact]
    public async Task ExecuteAsync_WithSessionEndFailureAndWorkItemFailure_LogsBothErrors()
    {
        // Arrange
        var workItemException = new InvalidOperationException("Work item failure");
        var sessionException = new InvalidOperationException("Session end failure");

        Func<IScopeContainer, CancellationToken, Task> failingWorkItem =
            BackgroundTaskMockFactory.CreateFailingWorkItem(workItemException);

        var mockWorker = BackgroundTaskMockFactory.CreateSequentialBackgroundWorker(
            new BackgroundWorkItem(failingWorkItem, null));

        var mockLogger = BackgroundTaskMockFactory.CreateBackgroundTaskServiceLogger();
        var mockScopeFactory = BackgroundTaskMockFactory.CreateDefaultScopeFactory();

        var mockSessionManager = new Mock<ISessionManager>();
        mockSessionManager
            .Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(new SessionBuilder().Build());
        mockSessionManager
            .Setup(x => x.EndSession(It.IsAny<Guid>()))
            .ThrowsAsync(sessionException);

        var sut = new BackgroundTaskService(
            mockWorker.Object,
            mockLogger.Object,
            mockScopeFactory.Object,
            mockSessionManager.Object);

        // Act
        _cancellationTokenSource.CancelAfter(100);
        await sut.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(50);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error ending session")),
                sessionException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "session end error should be logged");
    }

    #endregion

    #region Long Running Task Tests

    [Fact]
    public async Task ExecuteAsync_WithLongRunningTask_ProcessesCorrectly()
    {
        // Arrange
        var taskStarted = false;
        var taskCompleted = false;

        Func<IScopeContainer, CancellationToken, Task> longRunningTask = async (scope, ct) =>
        {
            taskStarted = true;
            await Task.Delay(200, ct); // Simulate long-running work
            taskCompleted = true;
        };

        var mockWorker = BackgroundTaskMockFactory.CreateSequentialBackgroundWorker(
            new BackgroundWorkItem(longRunningTask, null));

        var mockLogger = BackgroundTaskMockFactory.CreateBackgroundTaskServiceLogger();
        var mockScopeFactory = BackgroundTaskMockFactory.CreateDefaultScopeFactory();
        var mockSessionManager = BackgroundTaskMockFactory.CreateDefaultSessionManager();

        var sut = new BackgroundTaskService(
            mockWorker.Object,
            mockLogger.Object,
            mockScopeFactory.Object,
            mockSessionManager.Object);

        // Act
        _cancellationTokenSource.CancelAfter(500);
        var serviceTask = sut.StartAsync(_cancellationTokenSource.Token);

        await Task.Delay(50);
        taskStarted.Should().BeTrue("task should have started");
        taskCompleted.Should().BeFalse("task should still be running");

        await Task.Delay(200);
        taskCompleted.Should().BeTrue("task should have completed");

        await serviceTask;

        // Assert
        mockSessionManager.Verify(
            x => x.EndSession(It.IsAny<Guid>()),
            Times.Once,
            "session should end after long-running task completes");
    }

    #endregion

    #region Memory and Resource Management Tests

    [Fact]
    public async Task ExecuteAsync_WithManyWorkItems_DoesNotLeakResources()
    {
        // Arrange
        var completedCount = 0;
        var mockWorker = new Mock<IBackgroundWorker>();
        var workItemQueue = new Queue<BackgroundWorkItem>();

        // Create many lightweight work items
        for (int i = 0; i < 100; i++)
        {
            Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
            {
                Interlocked.Increment(ref completedCount);
                await Task.CompletedTask;
            };
            workItemQueue.Enqueue(new BackgroundWorkItem(workItem, null));
        }

        mockWorker
            .Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => workItemQueue.Count > 0 ? workItemQueue.Dequeue() : throw new OperationCanceledException());

        var mockLogger = BackgroundTaskMockFactory.CreateBackgroundTaskServiceLogger();
        var mockScopeFactory = BackgroundTaskMockFactory.CreateDefaultScopeFactory();
        var mockSessionManager = BackgroundTaskMockFactory.CreateDefaultSessionManager();

        var sut = new BackgroundTaskService(
            mockWorker.Object,
            mockLogger.Object,
            mockScopeFactory.Object,
            mockSessionManager.Object);

        var initialMemory = GC.GetTotalMemory(true);

        // Act
        _cancellationTokenSource.CancelAfter(1000);
        await sut.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(500);

        var finalMemory = GC.GetTotalMemory(true);
        var memoryGrowth = finalMemory - initialMemory;

        // Assert
        completedCount.Should().Be(100, "all work items should complete");
        mockSessionManager.Verify(
            x => x.EndSession(It.IsAny<Guid>()),
            Times.Exactly(100),
            "sessions should be properly cleaned up for all items");

        // Memory growth should be reasonable (less than 50MB for 100 small tasks)
        // Note: Test environment may have additional overhead
        memoryGrowth.Should().BeLessThan(50 * 1024 * 1024,
            "memory usage should not grow excessively");
    }

    #endregion

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}
