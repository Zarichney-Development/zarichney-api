using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Sessions;
using Zarichney.Tests.Framework.Mocks;
using Zarichney.Tests.TestData.Builders;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.BackgroundTasks;

/// <summary>
/// Unit tests for BackgroundTaskService hosted service
/// </summary>
public class BackgroundTaskServiceTests
{
    private readonly Mock<IBackgroundWorker> _mockWorker;
    private readonly Mock<ILogger<BackgroundTaskService>> _mockLogger;
    private readonly Mock<IScopeFactory> _mockScopeFactory;
    private readonly Mock<ISessionManager> _mockSessionManager;
    private readonly BackgroundTaskService _sut;

    public BackgroundTaskServiceTests()
    {
        _mockWorker = BackgroundTaskMockFactory.CreateBackgroundWorker();
        _mockLogger = BackgroundTaskMockFactory.CreateLogger();
        _mockScopeFactory = BackgroundTaskMockFactory.CreateScopeFactory();
        _mockSessionManager = BackgroundTaskMockFactory.CreateSessionManager();
        
        _sut = new BackgroundTaskService(
            _mockWorker.Object,
            _mockLogger.Object,
            _mockScopeFactory.Object,
            _mockSessionManager.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNullWorker_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new BackgroundTaskService(
            null!,
            _mockLogger.Object,
            _mockScopeFactory.Object,
            _mockSessionManager.Object);
        
        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("worker");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new BackgroundTaskService(
            _mockWorker.Object,
            null!,
            _mockScopeFactory.Object,
            _mockSessionManager.Object);
        
        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNullScopeFactory_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new BackgroundTaskService(
            _mockWorker.Object,
            _mockLogger.Object,
            null!,
            _mockSessionManager.Object);
        
        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("scopeFactory");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNullSessionManager_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new BackgroundTaskService(
            _mockWorker.Object,
            _mockLogger.Object,
            _mockScopeFactory.Object,
            null!);
        
        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("sessionManager");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var service = new BackgroundTaskService(
            _mockWorker.Object,
            _mockLogger.Object,
            _mockScopeFactory.Object,
            _mockSessionManager.Object);
        
        // Assert
        service.Should().NotBeNull("because valid dependencies should create a valid instance");
        service.Should().BeAssignableTo<BackgroundService>("because it should extend BackgroundService");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithCancellationRequested_ExitsCleanly()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancel
        
        _mockWorker.Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        
        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100); // Give time for the background task to process
        await _sut.StopAsync(CancellationToken.None);
        
        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("cancelled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithWorkItemNoParentSession_CreatesNewSession()
    {
        // Arrange
        var scopeId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var mockScope = BackgroundTaskMockFactory.CreateScopeContainer(scopeId);
        
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        _mockSessionManager.Setup(x => x.CreateSession(scopeId))
            .ReturnsAsync(new Session { Id = sessionId });
        
        var workItem = new BackgroundWorkItemBuilder()
            .WithoutParentSession()
            .Build();
        
        var taskCompletionSource = new TaskCompletionSource<bool>();
        _mockWorker.SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem)
            .Returns(() => taskCompletionSource.Task.ContinueWith(_ => workItem));
        
        using var cts = new CancellationTokenSource();
        
        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100); // Give time for processing
        cts.Cancel();
        taskCompletionSource.SetResult(true);
        await _sut.StopAsync(CancellationToken.None);
        
        // Assert
        _mockSessionManager.Verify(x => x.CreateSession(scopeId), Times.Once());
        mockScope.VerifySet(x => x.SessionId = sessionId, Times.Once());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithWorkItemWithParentSession_UsesExistingSession()
    {
        // Arrange
        var scopeId = Guid.NewGuid();
        var parentSession = new SessionBuilder().WithDefaults().Build();
        var mockScope = BackgroundTaskMockFactory.CreateScopeContainer(scopeId);
        
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        
        var workItem = new BackgroundWorkItemBuilder()
            .WithParentSession(parentSession)
            .Build();
        
        var taskCompletionSource = new TaskCompletionSource<bool>();
        _mockWorker.SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem)
            .Returns(() => taskCompletionSource.Task.ContinueWith(_ => workItem));
        
        using var cts = new CancellationTokenSource();
        
        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100); // Give time for processing
        cts.Cancel();
        taskCompletionSource.SetResult(true);
        await _sut.StopAsync(CancellationToken.None);
        
        // Assert
        _mockSessionManager.Verify(x => x.AddScopeToSession(parentSession, scopeId), Times.Once());
        mockScope.VerifySet(x => x.SessionId = parentSession.Id, Times.Once());
        _mockSessionManager.Verify(x => x.CreateSession(It.IsAny<Guid>()), Times.Never());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WorkItemThrowsException_LogsErrorAndContinues()
    {
        // Arrange
        var testException = new InvalidOperationException("Test exception");
        var failingWorkItem = new BackgroundWorkItemBuilder()
            .WithFailingWorkItem(testException)
            .Build();
        
        var successWorkItem = new BackgroundWorkItemBuilder()
            .WithSimpleWorkItem(_ => { })
            .Build();
        
        var taskCompletionSource = new TaskCompletionSource<bool>();
        _mockWorker.SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(failingWorkItem)
            .ReturnsAsync(successWorkItem)
            .Returns(() => taskCompletionSource.Task.ContinueWith(_ => successWorkItem));
        
        _mockScopeFactory.Setup(x => x.CreateScope())
            .Returns(() => BackgroundTaskMockFactory.CreateScopeContainer().Object);
        
        using var cts = new CancellationTokenSource();
        
        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(200); // Give time for processing both items
        cts.Cancel();
        taskCompletionSource.SetResult(true);
        await _sut.StopAsync(CancellationToken.None);
        
        // Assert
        _mockWorker.Verify(x => x.DequeueAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
        _mockSessionManager.Verify(x => x.EndSession(It.IsAny<Guid>()), Times.AtLeast(2));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_SessionEndThrowsException_LogsErrorButContinues()
    {
        // Arrange
        var sessionException = new InvalidOperationException("Session end failed");
        _mockSessionManager.Setup(x => x.EndSession(It.IsAny<Guid>()))
            .ThrowsAsync(sessionException);
        
        var workItem = new BackgroundWorkItemBuilder()
            .WithSimpleWorkItem(_ => { })
            .Build();
        
        var taskCompletionSource = new TaskCompletionSource<bool>();
        _mockWorker.SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem)
            .Returns(() => taskCompletionSource.Task.ContinueWith(_ => workItem));
        
        _mockScopeFactory.Setup(x => x.CreateScope())
            .Returns(() => BackgroundTaskMockFactory.CreateScopeContainer().Object);
        
        using var cts = new CancellationTokenSource();
        
        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100); // Give time for processing
        cts.Cancel();
        taskCompletionSource.SetResult(true);
        await _sut.StopAsync(CancellationToken.None);
        
        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error ending session")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_MultipleWorkItems_ProcessesAllInOrder()
    {
        // Arrange
        var executionOrder = new List<int>();
        var workItem1 = new BackgroundWorkItemBuilder()
            .WithSimpleWorkItem(_ => executionOrder.Add(1))
            .Build();
        var workItem2 = new BackgroundWorkItemBuilder()
            .WithSimpleWorkItem(_ => executionOrder.Add(2))
            .Build();
        var workItem3 = new BackgroundWorkItemBuilder()
            .WithSimpleWorkItem(_ => executionOrder.Add(3))
            .Build();
        
        var itemIndex = 0;
        var items = new[] { workItem1, workItem2, workItem3 };
        var taskCompletionSource = new TaskCompletionSource<bool>();
        
        _mockWorker.Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                if (itemIndex < items.Length)
                    return items[itemIndex++];
                
                taskCompletionSource.Task.Wait();
                throw new OperationCanceledException();
            });
        
        _mockScopeFactory.Setup(x => x.CreateScope())
            .Returns(() => BackgroundTaskMockFactory.CreateScopeContainer().Object);
        
        using var cts = new CancellationTokenSource();
        
        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(200); // Give time for processing all items
        cts.Cancel();
        taskCompletionSource.SetResult(true);
        await _sut.StopAsync(CancellationToken.None);
        
        // Assert
        executionOrder.Should().Equal(new[] { 1, 2, 3 },
            "because work items should be processed in the order they were dequeued");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WorkItemWithCancellationToken_PassesTokenToWorkItem()
    {
        // Arrange
        var tokenReceived = CancellationToken.None;
        var workItem = new BackgroundWorkItemBuilder()
            .WithCancellableWorkItem(token =>
            {
                tokenReceived = token;
                return Task.CompletedTask;
            })
            .Build();
        
        var taskCompletionSource = new TaskCompletionSource<bool>();
        _mockWorker.SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(workItem)
            .Returns(() => taskCompletionSource.Task.ContinueWith(_ => workItem));
        
        _mockScopeFactory.Setup(x => x.CreateScope())
            .Returns(() => BackgroundTaskMockFactory.CreateScopeContainer().Object);
        
        using var cts = new CancellationTokenSource();
        
        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100); // Give time for processing
        cts.Cancel();
        taskCompletionSource.SetResult(true);
        await _sut.StopAsync(CancellationToken.None);
        
        // Assert
        tokenReceived.Should().NotBe(CancellationToken.None,
            "because the cancellation token should be passed to work items");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_GeneralExceptionDuringDequeue_LogsErrorAndContinues()
    {
        // Arrange
        var dequeueException = new InvalidOperationException("Dequeue failed");
        var workItem = new BackgroundWorkItemBuilder()
            .WithSimpleWorkItem(_ => { })
            .Build();
        
        var taskCompletionSource = new TaskCompletionSource<bool>();
        _mockWorker.SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dequeueException)
            .ReturnsAsync(workItem)
            .Returns(() => taskCompletionSource.Task.ContinueWith(_ => workItem));
        
        _mockScopeFactory.Setup(x => x.CreateScope())
            .Returns(() => BackgroundTaskMockFactory.CreateScopeContainer().Object);
        
        using var cts = new CancellationTokenSource();
        
        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(200); // Give time for processing
        cts.Cancel();
        taskCompletionSource.SetResult(true);
        await _sut.StopAsync(CancellationToken.None);
        
        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error occurred executing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
        
        _mockWorker.Verify(x => x.DequeueAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }
}