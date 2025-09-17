using FluentAssertions;
using Moq;
using Xunit;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Sessions;
using Zarichney.Tests.TestData.Builders;
using Zarichney.Server.Tests.TestData.Builders;
using System.Threading.Channels;

namespace Zarichney.Tests.Unit.Services.BackgroundTasks;

/// <summary>
/// Unit tests for BackgroundWorker service
/// </summary>
public class BackgroundWorkerTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithValidCapacity_CreatesInstance()
    {
        // Arrange
        const int capacity = 100;
        
        // Act
        var sut = new BackgroundWorker(capacity);
        
        // Assert
        sut.Should().NotBeNull("because the constructor should create a valid instance");
        sut.Should().BeAssignableTo<IBackgroundWorker>("because it should implement the interface");
    }
    
    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(1000)]
    public void Constructor_WithVariousCapacities_CreatesInstance(int capacity)
    {
        // Arrange & Act
        var sut = new BackgroundWorker(capacity);
        
        // Assert
        sut.Should().NotBeNull("because constructor should handle various positive capacities");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueBackgroundWorkAsync_WithNullWorkItem_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new BackgroundWorker(10);
        Func<IScopeContainer, CancellationToken, Task>? nullWorkItem = null;
        
        // Act
        Action act = () => sut.QueueBackgroundWorkAsync(nullWorkItem!, null);
        
        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("workItem");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueBackgroundWorkAsync_WithValidWorkItem_DoesNotThrow()
    {
        // Arrange
        var sut = new BackgroundWorker(10);
        Func<IScopeContainer, CancellationToken, Task> workItem = (_, _) => Task.CompletedTask;
        
        // Act
        Action act = () => sut.QueueBackgroundWorkAsync(workItem, null);
        
        // Assert
        act.Should().NotThrow("because valid work items should be accepted");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueBackgroundWorkAsync_WithWorkItemAndSession_AcceptsBoth()
    {
        // Arrange
        var sut = new BackgroundWorker(10);
        var session = new SessionBuilder().WithDefaults().Build();
        Func<IScopeContainer, CancellationToken, Task> workItem = (_, _) => Task.CompletedTask;
        
        // Act
        Action act = () => sut.QueueBackgroundWorkAsync(workItem, session);
        
        // Assert
        act.Should().NotThrow("because work items with parent sessions should be accepted");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DequeueAsync_AfterQueueing_ReturnsQueuedItem()
    {
        // Arrange
        var sut = new BackgroundWorker(10);
        var expectedWorkItem = new BackgroundWorkItemBuilder()
            .WithSimpleWorkItem(_ => { })
            .Build();
        
        sut.QueueBackgroundWorkAsync(expectedWorkItem.WorkItem, expectedWorkItem.ParentSession);
        
        // Act
        var result = await sut.DequeueAsync(CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull("because a queued item should be returned");
        result.WorkItem.Should().BeSameAs(expectedWorkItem.WorkItem, "because the same work item should be dequeued");
        result.ParentSession.Should().BeSameAs(expectedWorkItem.ParentSession, "because the session should be preserved");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DequeueAsync_MultipleItems_ReturnsFifoOrder()
    {
        // Arrange
        var sut = new BackgroundWorker(10);
        var firstItem = new BackgroundWorkItemBuilder().WithSimpleWorkItem(_ => { }).Build();
        var secondItem = new BackgroundWorkItemBuilder().WithSimpleWorkItem(_ => { }).Build();
        var thirdItem = new BackgroundWorkItemBuilder().WithSimpleWorkItem(_ => { }).Build();
        
        sut.QueueBackgroundWorkAsync(firstItem.WorkItem, firstItem.ParentSession);
        sut.QueueBackgroundWorkAsync(secondItem.WorkItem, secondItem.ParentSession);
        sut.QueueBackgroundWorkAsync(thirdItem.WorkItem, thirdItem.ParentSession);
        
        // Act
        var result1 = await sut.DequeueAsync(CancellationToken.None);
        var result2 = await sut.DequeueAsync(CancellationToken.None);
        var result3 = await sut.DequeueAsync(CancellationToken.None);
        
        // Assert
        result1.WorkItem.Should().BeSameAs(firstItem.WorkItem, "because items should be dequeued in FIFO order");
        result2.WorkItem.Should().BeSameAs(secondItem.WorkItem, "because items should be dequeued in FIFO order");
        result3.WorkItem.Should().BeSameAs(thirdItem.WorkItem, "because items should be dequeued in FIFO order");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DequeueAsync_WithCancellation_ThrowsOperationCancelledException()
    {
        // Arrange
        var sut = new BackgroundWorker(10);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancel the token
        
        // Act
        Func<Task> act = async () => await sut.DequeueAsync(cts.Token);
        
        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DequeueAsync_EmptyQueueThenCancel_ThrowsOperationCancelledException()
    {
        // Arrange
        var sut = new BackgroundWorker(10);
        using var cts = new CancellationTokenSource();
        
        // Act
        var dequeueTask = sut.DequeueAsync(cts.Token);
        cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // Cancel after a short delay
        
        Func<Task> act = async () => await dequeueTask;
        
        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueBackgroundWorkAsync_WithSessionNull_AcceptsWorkItem()
    {
        // Arrange
        var sut = new BackgroundWorker(10);
        Func<IScopeContainer, CancellationToken, Task> workItem = (_, _) => Task.CompletedTask;
        
        // Act
        Action act = () => sut.QueueBackgroundWorkAsync(workItem, parentSession: null);
        
        // Assert
        act.Should().NotThrow("because work items without parent sessions are valid");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task QueueAndDequeue_WithComplexWorkItem_PreservesFunction()
    {
        // Arrange
        var sut = new BackgroundWorker(10);
        var executionCount = 0;
        Func<IScopeContainer, CancellationToken, Task> workItem = async (scope, token) =>
        {
            executionCount++;
            await Task.Delay(1, token);
        };
        
        sut.QueueBackgroundWorkAsync(workItem, null);
        
        // Act
        var result = await sut.DequeueAsync(CancellationToken.None);
        await result.WorkItem(Mock.Of<IScopeContainer>(), CancellationToken.None);
        
        // Assert
        executionCount.Should().Be(1, "because the work item function should be preserved and executable");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueBackgroundWorkAsync_AtCapacityLimit_ShouldWaitButNotThrow()
    {
        // Arrange
        const int capacity = 2;
        var sut = new BackgroundWorker(capacity);
        Func<IScopeContainer, CancellationToken, Task> workItem = (_, _) => Task.CompletedTask;
        
        // Act - Fill the queue to capacity
        Action act = () =>
        {
            sut.QueueBackgroundWorkAsync(workItem, null);
            sut.QueueBackgroundWorkAsync(workItem, null);
        };
        
        // Assert
        act.Should().NotThrow("because the queue should accept items up to capacity");
    }
}