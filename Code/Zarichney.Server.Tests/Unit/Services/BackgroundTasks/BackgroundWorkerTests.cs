using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Sessions;

namespace Zarichney.Tests.Unit.Services.BackgroundTasks;

/// <summary>
/// Unit tests for BackgroundWorker service - tests work item queuing and dequeuing with channel-based implementation
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "BackgroundTasks")]
public class BackgroundWorkerTests
{
  private readonly IFixture _fixture;
  private readonly BackgroundWorker _sut;

  public BackgroundWorkerTests()
  {
    _fixture = new Fixture();
    _sut = new BackgroundWorker(capacity: 10);
  }

  #region Constructor Tests

  [Fact]
  public void Constructor_WithValidCapacity_CreatesInstance()
  {
    // Arrange & Act
    var worker = new BackgroundWorker(capacity: 5);

    // Assert
    worker.Should().NotBeNull("BackgroundWorker should be created successfully");
    worker.Should().BeAssignableTo<IBackgroundWorker>("BackgroundWorker should implement IBackgroundWorker");
  }

  [Theory]
  [InlineData(1)]
  [InlineData(10)]
  [InlineData(100)]
  public void Constructor_WithVariousCapacities_CreatesInstance(int capacity)
  {
    // Arrange & Act
    var worker = new BackgroundWorker(capacity);

    // Assert
    worker.Should().NotBeNull($"BackgroundWorker should be created with capacity {capacity}");
  }

  #endregion

  #region QueueBackgroundWorkAsync Tests

  [Fact]
  public void QueueBackgroundWorkAsync_WithValidWorkItem_SuccessfullyQueues()
  {
    // Arrange
    var workItemExecuted = false;
    Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
    {
      workItemExecuted = true;
      await Task.CompletedTask;
    };

    // Act
    Action act = () => _sut.QueueBackgroundWorkAsync(workItem);

    // Assert
    act.Should().NotThrow("queuing a valid work item should not throw");
    workItemExecuted.Should().BeFalse("work item should not execute immediately upon queuing");
  }

  [Fact]
  public void QueueBackgroundWorkAsync_WithNullWorkItem_ThrowsArgumentNullException()
  {
    // Arrange
    Func<IScopeContainer, CancellationToken, Task>? workItem = null;

    // Act
    Action act = () => _sut.QueueBackgroundWorkAsync(workItem!);

    // Assert
    act.Should().Throw<ArgumentNullException>()
        .WithMessage("*workItem*", "null work item should throw ArgumentNullException");
  }

  [Fact]
  public void QueueBackgroundWorkAsync_WithParentSession_SuccessfullyQueues()
  {
    // Arrange
    var session = new SessionBuilder().Build();
    Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
    {
      await Task.CompletedTask;
    };

    // Act
    Action act = () => _sut.QueueBackgroundWorkAsync(workItem, session);

    // Assert
    act.Should().NotThrow("queuing a work item with parent session should not throw");
  }

  [Fact]
  public void QueueBackgroundWorkAsync_MultipleItems_QueuesAllSuccessfully()
  {
    // Arrange
    var workItems = new List<Func<IScopeContainer, CancellationToken, Task>>();
    for (int i = 0; i < 5; i++)
    {
      workItems.Add(async (scope, ct) => await Task.CompletedTask);
    }

    // Act
    Action act = () =>
    {
      foreach (var workItem in workItems)
      {
        _sut.QueueBackgroundWorkAsync(workItem);
      }
    };

    // Assert
    act.Should().NotThrow("queuing multiple work items should not throw");
  }

  #endregion

  #region DequeueAsync Tests

  [Fact]
  public async Task DequeueAsync_WithQueuedItem_ReturnsWorkItem()
  {
    // Arrange
    var expectedWorkItem = async (IScopeContainer scope, CancellationToken ct) =>
    {
      await Task.CompletedTask;
    };
    _sut.QueueBackgroundWorkAsync(expectedWorkItem);

    // Act
    var result = await _sut.DequeueAsync(CancellationToken.None);

    // Assert
    result.Should().NotBeNull("dequeued item should not be null");
    result.WorkItem.Should().Be(expectedWorkItem, "dequeued work item should match the queued item");
    result.ParentSession.Should().BeNull("parent session should be null when not provided");
  }

  [Fact]
  public async Task DequeueAsync_WithQueuedItemAndSession_ReturnsWorkItemWithSession()
  {
    // Arrange
    var session = new SessionBuilder().Build();
    var expectedWorkItem = async (IScopeContainer scope, CancellationToken ct) =>
    {
      await Task.CompletedTask;
    };
    _sut.QueueBackgroundWorkAsync(expectedWorkItem, session);

    // Act
    var result = await _sut.DequeueAsync(CancellationToken.None);

    // Assert
    result.Should().NotBeNull("dequeued item should not be null");
    result.WorkItem.Should().Be(expectedWorkItem, "dequeued work item should match the queued item");
    result.ParentSession.Should().Be(session, "parent session should match the provided session");
  }

  [Fact]
  public async Task DequeueAsync_WithMultipleQueuedItems_ReturnsFIFOOrder()
  {
    // Arrange
    var workItems = new List<Func<IScopeContainer, CancellationToken, Task>>();
    var executionOrder = new List<int>();

    for (int i = 0; i < 3; i++)
    {
      var index = i;
      workItems.Add(async (scope, ct) =>
      {
        executionOrder.Add(index);
        await Task.CompletedTask;
      });
      _sut.QueueBackgroundWorkAsync(workItems[i]);
    }

    // Act
    var results = new List<BackgroundWorkItem>();
    for (int i = 0; i < 3; i++)
    {
      results.Add(await _sut.DequeueAsync(CancellationToken.None));
    }

    // Assert
    results.Should().HaveCount(3, "all three items should be dequeued");
    results[0].WorkItem.Should().Be(workItems[0], "first dequeued item should be first queued item");
    results[1].WorkItem.Should().Be(workItems[1], "second dequeued item should be second queued item");
    results[2].WorkItem.Should().Be(workItems[2], "third dequeued item should be third queued item");
  }

  [Fact]
  public async Task DequeueAsync_WithCancellation_ThrowsOperationCanceledException()
  {
    // Arrange
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    // Act
    Func<Task> act = async () => await _sut.DequeueAsync(cts.Token);

    // Assert
    await act.Should().ThrowAsync<OperationCanceledException>()
        .WithMessage("*canceled*", "dequeue should throw when cancellation is requested");
  }

  [Fact]
  public async Task DequeueAsync_EmptyQueue_WaitsUntilItemQueued()
  {
    // Arrange
    var dequeueTask = _sut.DequeueAsync(CancellationToken.None);
    dequeueTask.IsCompleted.Should().BeFalse("dequeue should wait when queue is empty");

    var workItem = async (IScopeContainer scope, CancellationToken ct) =>
    {
      await Task.CompletedTask;
    };

    // Act
    _sut.QueueBackgroundWorkAsync(workItem);
    var result = await dequeueTask;

    // Assert
    result.Should().NotBeNull("dequeued item should not be null");
    result.WorkItem.Should().Be(workItem, "dequeued work item should match the queued item");
  }

  #endregion

  #region Capacity and Boundary Tests

  [Fact]
  public void QueueBackgroundWorkAsync_AtCapacity_StillQueuesSuccessfully()
  {
    // Arrange
    var worker = new BackgroundWorker(capacity: 3);
    var workItems = new List<Func<IScopeContainer, CancellationToken, Task>>();

    for (int i = 0; i < 3; i++)
    {
      workItems.Add(async (scope, ct) => await Task.CompletedTask);
    }

    // Act
    Action act = () =>
    {
      foreach (var workItem in workItems)
      {
        worker.QueueBackgroundWorkAsync(workItem);
      }
    };

    // Assert
    act.Should().NotThrow("queuing items up to capacity should not throw");
  }

  [Fact]
  public async Task DequeueAsync_MixedSessionAndNoSession_HandlesCorrectly()
  {
    // Arrange
    var session1 = new SessionBuilder().WithId(Guid.NewGuid()).Build();
    var session2 = new SessionBuilder().WithId(Guid.NewGuid()).Build();

    var workItem1 = async (IScopeContainer scope, CancellationToken ct) => await Task.Delay(1);
    var workItem2 = async (IScopeContainer scope, CancellationToken ct) => await Task.Delay(1);
    var workItem3 = async (IScopeContainer scope, CancellationToken ct) => await Task.Delay(1);

    _sut.QueueBackgroundWorkAsync(workItem1, session1);
    _sut.QueueBackgroundWorkAsync(workItem2);
    _sut.QueueBackgroundWorkAsync(workItem3, session2);

    // Act
    var result1 = await _sut.DequeueAsync(CancellationToken.None);
    var result2 = await _sut.DequeueAsync(CancellationToken.None);
    var result3 = await _sut.DequeueAsync(CancellationToken.None);

    // Assert
    result1.ParentSession.Should().Be(session1, "first item should have session1");
    result2.ParentSession.Should().BeNull("second item should have no session");
    result3.ParentSession.Should().Be(session2, "third item should have session2");
  }

  #endregion
}
