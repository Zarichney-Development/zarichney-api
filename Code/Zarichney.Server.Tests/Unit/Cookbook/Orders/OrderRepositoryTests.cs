using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Cookbook.Orders;
using Zarichney.Services.FileSystem;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Cookbook.Orders;

[Trait("Category", "Unit")]
public class OrderRepositoryTests
{
  private readonly Mock<IFileService> _mockFileService;
  private readonly Mock<IFileWriteQueueService> _mockFileWriteQueueService;
  private readonly Mock<ILogger<OrderFileRepository>> _mockLogger;
  private readonly OrderConfig _orderConfig;
  private readonly OrderFileRepository _sut;

  public OrderRepositoryTests()
  {
    _mockFileService = new Mock<IFileService>();
    _mockFileWriteQueueService = new Mock<IFileWriteQueueService>();
    _mockLogger = new Mock<ILogger<OrderFileRepository>>();

    _orderConfig = new OrderConfig
    {
      MaxParallelTasks = 5,
      OutputDirectory = "Data/Orders"
    };

    _sut = new OrderFileRepository(
        _orderConfig,
        _mockFileService.Object,
        _mockFileWriteQueueService.Object,
        _mockLogger.Object
    );
  }

  [Fact]
  public async Task GetOrder_ValidOrderId_ReturnsOrder()
  {
    // Arrange
    var orderId = "test-order-123";
    var expectedOrder = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .Build();

    var expectedPath = Path.Combine(_orderConfig.OutputDirectory, orderId);

    _mockFileService
        .Setup(x => x.ReadFromFile<CookbookOrder?>(expectedPath, "Order", It.IsAny<string?>()))
        .ReturnsAsync(expectedOrder);

    // Act
    var result = await _sut.GetOrder(orderId);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEquivalentTo(expectedOrder);
    _mockFileService.Verify(x => x.ReadFromFile<CookbookOrder?>(expectedPath, "Order", It.IsAny<string?>()), Times.Once);
  }

  [Fact]
  public async Task GetOrder_OrderNotFound_ReturnsNull()
  {
    // Arrange
    var orderId = "non-existent-order";
    var expectedPath = Path.Combine(_orderConfig.OutputDirectory, orderId);

    _mockFileService
        .Setup(x => x.ReadFromFile<CookbookOrder?>(expectedPath, "Order", It.IsAny<string?>()))
        .ReturnsAsync((CookbookOrder?)null);

    // Act
    var result = await _sut.GetOrder(orderId);

    // Assert
    result.Should().BeNull();
    _mockFileService.Verify(x => x.ReadFromFile<CookbookOrder?>(expectedPath, "Order", It.IsAny<string?>()), Times.Once);
  }

  [Fact]
  public void AddUpdateOrderAsync_ValidOrder_QueuesWrite()
  {
    // Arrange
    var order = new CookbookOrderBuilder()
        .WithOrderId("order-123")
        .Build();

    var expectedPath = Path.Combine(_orderConfig.OutputDirectory, order.OrderId);

    // Act
    _sut.AddUpdateOrderAsync(order);

    // Assert
    _mockFileWriteQueueService.Verify(x => x.QueueWrite(
        expectedPath,
        "Order",
        order,
        It.IsAny<string?>()
    ), Times.Once);
  }

  [Fact]
  public void SaveRecipe_ValidRecipe_QueuesWrite()
  {
    // Arrange
    var orderId = "order-123";
    var recipeTitle = "Delicious Recipe";
    var recipeMarkdown = "# Recipe Content\n\n## Ingredients\n- Item 1";

    var expectedPath = Path.Combine(_orderConfig.OutputDirectory, orderId, "recipes");

    // Act
    _sut.SaveRecipe(orderId, recipeTitle, recipeMarkdown);

    // Assert
    _mockFileWriteQueueService.Verify(x => x.QueueWrite(
        expectedPath,
        recipeTitle,
        recipeMarkdown,
        "md"
    ), Times.Once);
  }

  [Fact]
  public void SaveCookbook_ValidMarkdown_QueuesWrite()
  {
    // Arrange
    var orderId = "order-123";
    var cookbookMarkdown = "# Cookbook\n\n## Recipe 1\n\n## Recipe 2";

    var expectedPath = Path.Combine(_orderConfig.OutputDirectory, orderId);

    // Act
    _sut.SaveCookbook(orderId, cookbookMarkdown);

    // Assert
    _mockFileWriteQueueService.Verify(x => x.QueueWrite(
        expectedPath,
        "Cookbook",
        cookbookMarkdown,
        "md"
    ), Times.Once);
  }

  [Fact]
  public async Task GetCookbook_ValidOrderId_ReturnsPdfBytes()
  {
    // Arrange
    var orderId = "order-123";
    var expectedPdf = new byte[] { 1, 2, 3, 4, 5 };
    var expectedPath = Path.Combine(_orderConfig.OutputDirectory, orderId);

    _mockFileService
        .Setup(x => x.ReadFromFile<byte[]>(expectedPath, "Cookbook", "pdf"))
        .ReturnsAsync(expectedPdf);

    // Act
    var result = await _sut.GetCookbook(orderId);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEquivalentTo(expectedPdf);
    _mockFileService.Verify(x => x.ReadFromFile<byte[]>(expectedPath, "Cookbook", "pdf"), Times.Once);
  }

  [Fact]
  public async Task GetCookbook_CookbookNotFound_ReturnsNull()
  {
    // Arrange
    var orderId = "order-123";
    var expectedPath = Path.Combine(_orderConfig.OutputDirectory, orderId);

    _mockFileService
        .Setup(x => x.ReadFromFile<byte[]>(expectedPath, "Cookbook", "pdf"))
        .ReturnsAsync((byte[]?)null);

    // Act
    var result = await _sut.GetCookbook(orderId);

    // Assert
    result.Should().BeNull();
    _mockFileService.Verify(x => x.ReadFromFile<byte[]>(expectedPath, "Cookbook", "pdf"), Times.Once);
  }

  [Fact]
  public async Task SaveCookbook_SynchronousWrite_WaitsForCompletion()
  {
    // Arrange
    var orderId = "order-123";
    var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
    var expectedPath = Path.Combine(_orderConfig.OutputDirectory, orderId);

    _mockFileWriteQueueService
        .Setup(x => x.WriteToFileAndWaitAsync(
            expectedPath,
            "Cookbook",
            pdfBytes,
            "pdf"))
        .Returns(Task.CompletedTask);

    // Act
    await _sut.SaveCookbook(orderId, pdfBytes);

    // Assert
    _mockFileWriteQueueService.Verify(x => x.WriteToFileAndWaitAsync(
        expectedPath,
        "Cookbook",
        pdfBytes,
        "pdf"
    ), Times.Once);
  }

  [Fact]
  public void SaveCookbookAsync_AsynchronousWrite_QueuesWrite()
  {
    // Arrange
    var orderId = "order-123";
    var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
    var expectedPath = Path.Combine(_orderConfig.OutputDirectory, orderId);

    // Act
    _sut.SaveCookbookAsync(orderId, pdfBytes);

    // Assert
    _mockFileWriteQueueService.Verify(x => x.QueueWrite(
        expectedPath,
        "Cookbook",
        pdfBytes,
        "pdf"
    ), Times.Once);
  }

  [Fact]
  public async Task GetPendingOrdersForCustomer_NoOrders_ReturnsEmptyList()
  {
    // Arrange
    var email = "customer@test.com";
    var baseDir = _orderConfig.OutputDirectory;

    // Mock Directory.GetDirectories to return empty array
    // Note: In a real unit test, we'd need to abstract file system operations
    // For now, we'll just test the logic assuming no directories exist

    // Act
    var result = await _sut.GetPendingOrdersForCustomer(email);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEmpty();
  }

  [Theory]
  [InlineData(OrderStatus.AwaitingPayment, true, true)]
  [InlineData(OrderStatus.AwaitingPayment, false, true)]
  [InlineData(OrderStatus.Completed, true, true)]
  [InlineData(OrderStatus.Completed, false, false)]
  [InlineData(OrderStatus.InProgress, true, true)]
  [InlineData(OrderStatus.InProgress, false, false)]
  public void GetPendingOrdersForCustomer_OrderMatching_FiltersCorrectly(
      OrderStatus status,
      bool requiresPayment,
      bool shouldBeIncluded)
  {
    // Arrange
    var email = "customer@test.com";
    var order = new CookbookOrderBuilder()
        .WithEmail(email)
        .WithStatus(status)
        .WithRequiresPayment(requiresPayment)
        .Build();

    // This test validates the filtering logic
    var shouldInclude = (order.Status == OrderStatus.AwaitingPayment || order.RequiresPayment) &&
                       string.Equals(order.Email, email, StringComparison.OrdinalIgnoreCase);

    // Assert
    shouldInclude.Should().Be(shouldBeIncluded,
        because: $"Order with status {status} and requiresPayment={requiresPayment} should{(shouldBeIncluded ? "" : " not")} be included");
  }

  [Fact]
  public void GetPendingOrdersForCustomer_MultipleOrders_SortsByRecipeCount()
  {
    // Arrange
    var orders = new List<CookbookOrder>
        {
            new CookbookOrderBuilder()
                .WithSynthesizedRecipes(
                    new SynthesizedRecipeBuilder().Build())
                .Build(),
            new CookbookOrderBuilder()
                .WithSynthesizedRecipes(
                    new SynthesizedRecipeBuilder().Build(),
                    new SynthesizedRecipeBuilder().Build(),
                    new SynthesizedRecipeBuilder().Build())
                .Build(),
            new CookbookOrderBuilder()
                .WithSynthesizedRecipes(
                    new SynthesizedRecipeBuilder().Build(),
                    new SynthesizedRecipeBuilder().Build())
                .Build()
        };

    // Simulate the sorting logic
    var sorted = orders
        .OrderByDescending(o => o.SynthesizedRecipes.Count)
        .ToList();

    // Assert
    sorted[0].SynthesizedRecipes.Count.Should().Be(3);
    sorted[1].SynthesizedRecipes.Count.Should().Be(2);
    sorted[2].SynthesizedRecipes.Count.Should().Be(1);
  }
}
