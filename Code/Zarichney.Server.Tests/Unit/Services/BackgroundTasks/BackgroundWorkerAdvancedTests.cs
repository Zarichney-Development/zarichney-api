using AutoFixture;
using FluentAssertions;
using Xunit;
using Zarichney.Server.Tests.Framework.Mocks;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Sessions;

namespace Zarichney.Server.Tests.Unit.Services.BackgroundTasks;

/// <summary>
/// Advanced unit tests for BackgroundWorker covering edge cases, concurrency scenarios,
/// and error conditions not covered in the basic test suite.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "BackgroundTasks")]
public class BackgroundWorkerAdvancedTests
{
  private readonly IFixture _fixture;

  public BackgroundWorkerAdvancedTests()
  {
    _fixture = new Fixture();
  }

  #region Edge Case Tests

  [Fact]
  public async Task QueueAndDequeue_WithImmediateValueTask_HandlesCorrectly()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 5);
    var executed = false;
    Func<IScopeContainer, CancellationToken, Task> workItem = (scope, ct) =>
    {
      executed = true;
      return Task.CompletedTask; // Immediately completed task
    };

    // Act
    sut.QueueBackgroundWorkAsync(workItem);
    var result = await sut.DequeueAsync(CancellationToken.None);
    await result.WorkItem(BackgroundTaskMockFactory.CreateMockScope(Guid.NewGuid()).Object, CancellationToken.None);

    // Assert
    executed.Should().BeTrue("work item should execute when invoked");
    result.WorkItem.Should().NotBeNull("dequeued work item should not be null");
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void Constructor_WithInvalidCapacity_ThrowsArgumentOutOfRangeException(int capacity)
  {
    // Act
    Action act = () => new BackgroundWorker(capacity);

    // Assert
    // BoundedChannelOptions requires capacity > 0
    act.Should().Throw<ArgumentOutOfRangeException>()
        .WithParameterName("capacity", "invalid capacity should throw ArgumentOutOfRangeException");
  }

  [Fact]
  public async Task DequeueAsync_WithImmediateCancellation_ThrowsOperationCanceledException()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 10);
    var cts = new CancellationTokenSource();
    cts.Cancel(); // Cancel immediately

    // Act
    Func<Task> act = async () => await sut.DequeueAsync(cts.Token);

    // Assert
    await act.Should().ThrowAsync<OperationCanceledException>()
        .WithMessage("*canceled*", "immediate cancellation should throw");
  }

  #endregion

  #region Concurrency Tests

  [Fact]
  public async Task QueueBackgroundWorkAsync_ConcurrentQueuing_MaintainsOrder()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 100);
    var executionOrder = new List<int>();
    var lockObj = new object();
    var tasks = new List<Task>();

    // Act - Queue items concurrently
    for (int i = 0; i < 50; i++)
    {
      var index = i;
      tasks.Add(Task.Run(() =>
      {
        Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
              {
                lock (lockObj)
                {
                  executionOrder.Add(index);
                }
                await Task.CompletedTask;
              };
        sut.QueueBackgroundWorkAsync(workItem);
      }));
    }

    await Task.WhenAll(tasks);

    // Dequeue and execute all items
    var dequeuedItems = new List<BackgroundWorkItem>();
    for (int i = 0; i < 50; i++)
    {
      var item = await sut.DequeueAsync(CancellationToken.None);
      dequeuedItems.Add(item);
      await item.WorkItem(BackgroundTaskMockFactory.CreateMockScope(Guid.NewGuid()).Object, CancellationToken.None);
    }

    // Assert
    executionOrder.Should().HaveCount(50, "all work items should have been executed");
    dequeuedItems.Should().HaveCount(50, "all items should have been dequeued");
  }

  [Fact]
  public async Task DequeueAsync_ConcurrentDequeuers_HandlesCorrectly()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 10);
    var dequeuedCount = 0;
    var lockObj = new object();

    // Queue 5 items
    for (int i = 0; i < 5; i++)
    {
      sut.QueueBackgroundWorkAsync(async (scope, ct) => await Task.CompletedTask);
    }

    // Act - Multiple concurrent dequeuers
    var dequeueTasks = new List<Task>();
    for (int i = 0; i < 5; i++)
    {
      dequeueTasks.Add(Task.Run(async () =>
      {
        var item = await sut.DequeueAsync(CancellationToken.None);
        if (item != null)
        {
          lock (lockObj)
          {
            dequeuedCount++;
          }
        }
      }));
    }

    await Task.WhenAll(dequeueTasks);

    // Assert
    dequeuedCount.Should().Be(5, "all queued items should be dequeued exactly once");
  }

  #endregion

  #region Session Handling Tests

  [Fact]
  public async Task QueueAndDequeue_WithNullSession_PreservesNullSession()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 5);
    Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) => await Task.CompletedTask;

    // Act
    sut.QueueBackgroundWorkAsync(workItem, null);
    var result = await sut.DequeueAsync(CancellationToken.None);

    // Assert
    result.ParentSession.Should().BeNull("null session should be preserved");
    result.WorkItem.Should().NotBeNull("work item should be preserved");
  }

  [Fact]
  public async Task QueueAndDequeue_WithMultipleDifferentSessions_PreservesEachSession()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 10);
    var sessions = new List<Session>();
    for (int i = 0; i < 5; i++)
    {
      sessions.Add(new SessionBuilder().WithId(Guid.NewGuid()).Build());
    }

    // Act - Queue with different sessions
    foreach (var session in sessions)
    {
      sut.QueueBackgroundWorkAsync(async (scope, ct) => await Task.CompletedTask, session);
    }

    // Dequeue and verify
    var dequeuedSessions = new List<Session?>();
    for (int i = 0; i < 5; i++)
    {
      var item = await sut.DequeueAsync(CancellationToken.None);
      dequeuedSessions.Add(item.ParentSession);
    }

    // Assert
    dequeuedSessions.Should().BeEquivalentTo(sessions, "all sessions should be preserved in order");
  }

  #endregion

  #region Capacity and Performance Tests

  [Fact]
  public void QueueBackgroundWorkAsync_RapidSuccessiveQueuing_HandlesCorrectly()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 1000);
    var workItems = new List<Func<IScopeContainer, CancellationToken, Task>>();

    for (int i = 0; i < 100; i++)
    {
      workItems.Add(async (scope, ct) => await Task.Delay(1));
    }

    // Act
    Action act = () =>
    {
      foreach (var item in workItems)
      {
        sut.QueueBackgroundWorkAsync(item);
      }
    };

    // Assert
    act.Should().NotThrow("rapid queuing should be handled without issues");
  }

  [Fact]
  public async Task DequeueAsync_EmptyQueueWithTimeout_WaitsAppropriately()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 5);
    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    // Act
    Func<Task> act = async () => await sut.DequeueAsync(cts.Token);

    // Assert
    await act.Should().ThrowAsync<OperationCanceledException>();
    stopwatch.Stop();
    stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(90,
        "should wait approximately the timeout duration");
  }

  #endregion

  #region Work Item Variations Tests

  [Fact]
  public async Task QueueAndDequeue_WithAsyncWorkItem_ExecutesCorrectly()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 5);
    var executionSteps = new List<string>();

    Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
    {
      executionSteps.Add("start");
      await Task.Delay(10);
      executionSteps.Add("middle");
      await Task.Delay(10);
      executionSteps.Add("end");
    };

    // Act
    sut.QueueBackgroundWorkAsync(workItem);
    var result = await sut.DequeueAsync(CancellationToken.None);
    await result.WorkItem(BackgroundTaskMockFactory.CreateMockScope(Guid.NewGuid()).Object, CancellationToken.None);

    // Assert
    executionSteps.Should().BeEquivalentTo(new[] { "start", "middle", "end" },
        options => options.WithStrictOrdering(),
        "async work item should execute all steps in order");
  }

  [Fact]
  public async Task QueueAndDequeue_WithExceptionThrowingWorkItem_PropagatesException()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 5);
    var expectedException = new InvalidOperationException("Test exception");

    Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
    {
      await Task.CompletedTask;
      throw expectedException;
    };

    // Act
    sut.QueueBackgroundWorkAsync(workItem);
    var result = await sut.DequeueAsync(CancellationToken.None);

    Func<Task> act = async () =>
        await result.WorkItem(BackgroundTaskMockFactory.CreateMockScope(Guid.NewGuid()).Object, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Test exception", "work item exception should propagate");
  }

  [Fact]
  public async Task QueueAndDequeue_WithCancellableWorkItem_RespectsToken()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 5);
    var wasCancelled = false;

    Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
    {
      try
      {
        await Task.Delay(1000, ct);
      }
      catch (OperationCanceledException)
      {
        wasCancelled = true;
        throw;
      }
    };

    // Act
    sut.QueueBackgroundWorkAsync(workItem);
    var result = await sut.DequeueAsync(CancellationToken.None);

    using var cts = new CancellationTokenSource(50);
    Func<Task> act = async () =>
        await result.WorkItem(BackgroundTaskMockFactory.CreateMockScope(Guid.NewGuid()).Object, cts.Token);

    // Assert
    await act.Should().ThrowAsync<OperationCanceledException>();
    wasCancelled.Should().BeTrue("work item should detect cancellation");
  }

  #endregion

  #region Complex Scenario Tests

  [Fact]
  public async Task QueueDequeue_WithMixedPatternsAndSessions_MaintainsIntegrity()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 20);
    var executionLog = new List<(int Index, Guid? SessionId, DateTime Timestamp)>();
    var lockObj = new object();

    // Create varied work items with different sessions
    var sessions = new[] { null, new SessionBuilder().Build(), null, new SessionBuilder().Build() };
    var workItems = new List<(Func<IScopeContainer, CancellationToken, Task> WorkItem, Session? Session)>();

    for (int i = 0; i < 10; i++)
    {
      var index = i;
      var session = sessions[i % sessions.Length];

      Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
      {
        await Task.Delay(Random.Shared.Next(1, 5));
        lock (lockObj)
        {
          executionLog.Add((index, session?.Id, DateTime.UtcNow));
        }
      };

      workItems.Add((workItem, session));
    }

    // Act - Queue all items
    foreach (var (workItem, session) in workItems)
    {
      sut.QueueBackgroundWorkAsync(workItem, session);
    }

    // Dequeue and execute all
    var dequeuedItems = new List<BackgroundWorkItem>();
    for (int i = 0; i < 10; i++)
    {
      var item = await sut.DequeueAsync(CancellationToken.None);
      dequeuedItems.Add(item);
    }

    // Execute all dequeued items
    var executionTasks = dequeuedItems.Select(item =>
        item.WorkItem(BackgroundTaskMockFactory.CreateMockScope(Guid.NewGuid()).Object, CancellationToken.None)
    );
    await Task.WhenAll(executionTasks);

    // Assert
    executionLog.Should().HaveCount(10, "all work items should have executed");
    dequeuedItems.Select(i => i.ParentSession?.Id).Should().BeEquivalentTo(
        workItems.Select(w => w.Session?.Id),
        options => options.WithStrictOrdering(),
        "sessions should be preserved in FIFO order");
  }

  [Fact]
  public async Task QueueDequeue_UnderStress_MaintainsConsistency()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 500);
    var successCount = 0;
    var errorCount = 0;
    var lockObj = new object();

    // Create mix of successful and failing work items
    var tasks = new List<Task>();

    // Act - Producer tasks
    for (int i = 0; i < 10; i++)
    {
      tasks.Add(Task.Run(() =>
      {
        for (int j = 0; j < 20; j++)
        {
          var shouldFail = j % 5 == 0;
          Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, ct) =>
                {
                  await Task.Delay(1);
                  if (shouldFail)
                  {
                    lock (lockObj) { errorCount++; }
                    throw new Exception("Planned failure");
                  }
                  lock (lockObj) { successCount++; }
                };
          sut.QueueBackgroundWorkAsync(workItem);
        }
      }));
    }

    await Task.WhenAll(tasks);

    // Consumer tasks
    var consumerTasks = new List<Task>();
    using var cts = new CancellationTokenSource();

    for (int i = 0; i < 5; i++)
    {
      consumerTasks.Add(Task.Run(async () =>
      {
        while (!cts.Token.IsCancellationRequested)
        {
          try
          {
            var item = await sut.DequeueAsync(cts.Token);
            try
            {
              await item.WorkItem(
                        BackgroundTaskMockFactory.CreateMockScope(Guid.NewGuid()).Object,
                        CancellationToken.None);
            }
            catch (Exception)
            {
              // Expected for some work items
            }
          }
          catch (OperationCanceledException)
          {
            break;
          }
        }
      }));
    }

    // Wait for processing
    await Task.Delay(500);
    cts.Cancel();
    await Task.WhenAll(consumerTasks);

    // Assert
    (successCount + errorCount).Should().Be(200, "all work items should have been processed");
    successCount.Should().Be(160, "80% should succeed (not divisible by 5)");
    errorCount.Should().Be(40, "20% should fail (divisible by 5)");
  }

  #endregion

  #region Boundary Tests

  [Fact]
  public void QueueBackgroundWorkAsync_WithMaxCapacityBoundary_HandlesCorrectly()
  {
    // Arrange
    var capacity = int.MaxValue / 2; // Use a very large capacity
    var sut = new BackgroundWorker(capacity);

    // Act
    Action act = () => sut.QueueBackgroundWorkAsync(async (scope, ct) => await Task.CompletedTask);

    // Assert
    act.Should().NotThrow("should handle large capacity values");
  }

  [Fact]
  public async Task DequeueAsync_AfterMultipleQueueDequeuesCycles_MaintainsCorrectState()
  {
    // Arrange
    var sut = new BackgroundWorker(capacity: 5);
    var cycleCount = 10;
    var itemsPerCycle = 3;

    // Act & Assert - Multiple cycles
    for (int cycle = 0; cycle < cycleCount; cycle++)
    {
      // Queue items
      for (int i = 0; i < itemsPerCycle; i++)
      {
        var cycleCapture = cycle;
        var itemCapture = i;
        sut.QueueBackgroundWorkAsync(async (scope, ct) =>
        {
          await Task.CompletedTask;
        });
      }

      // Dequeue items
      var dequeuedInCycle = new List<BackgroundWorkItem>();
      for (int i = 0; i < itemsPerCycle; i++)
      {
        var item = await sut.DequeueAsync(CancellationToken.None);
        dequeuedInCycle.Add(item);
      }

      dequeuedInCycle.Should().HaveCount(itemsPerCycle,
          $"cycle {cycle} should dequeue exactly {itemsPerCycle} items");
    }

    // Verify queue is empty after all cycles
    using var cts = new CancellationTokenSource(100);
    Func<Task> act = async () => await sut.DequeueAsync(cts.Token);
    await act.Should().ThrowAsync<OperationCanceledException>("queue should be empty after all cycles");
  }

  #endregion
}
