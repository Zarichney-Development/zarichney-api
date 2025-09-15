using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Config;
using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Prompts;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services.AI;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Email;
using Zarichney.Services.FileSystem;
using Zarichney.Services.PdfGeneration;
using Zarichney.Services.Sessions;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Cookbook.Orders;

[Trait("Category", "Unit")]
public class OrderServiceTests
{
    private readonly Mock<IBackgroundWorker> _mockBackgroundWorker;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<RecipeService>> _mockLogger;
    private readonly Mock<ILlmService> _mockLlmService;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<PdfCompiler> _mockPdfCompiler;
    private readonly Mock<IRecipeService> _mockRecipeService;
    private readonly Mock<IScopeContainer> _mockScope;
    private readonly Mock<ISessionManager> _mockSessionManager;
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly OrderService _sut;
    private readonly OrderConfig _orderConfig;
    private readonly EmailConfig _emailConfig;
    private readonly LlmConfig _llmConfig;
    private readonly ProcessOrderPrompt _processOrderPrompt;

    public OrderServiceTests()
    {
        _mockBackgroundWorker = new Mock<IBackgroundWorker>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<RecipeService>>();
        _mockLlmService = new Mock<ILlmService>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockPdfCompiler = new Mock<PdfCompiler>(
            new PdfCompilerConfig(),
            Mock.Of<IFileService>(),
            Mock.Of<ILogger<PdfCompiler>>()
        );
        _mockRecipeService = new Mock<IRecipeService>();
        _mockScope = new Mock<IScopeContainer>();
        _mockSessionManager = new Mock<ISessionManager>();
        _mockCustomerService = new Mock<ICustomerService>();

        _orderConfig = new OrderConfig 
        { 
            MaxParallelTasks = 5,
            OutputDirectory = "Data/Orders"
        };
        _emailConfig = new EmailConfig 
        { 
            FromEmail = "noreply@test.com",
            AzureTenantId = "test-tenant",
            AzureAppId = "test-app",
            AzureAppSecret = "test-secret",
            MailCheckApiKey = "test-key"
        };
        _llmConfig = new LlmConfig 
        { 
            RetryAttempts = 3
        };
        _processOrderPrompt = new ProcessOrderPrompt();

        var scopeId = Guid.NewGuid();
        _mockScope.Setup(x => x.Id).Returns(scopeId);

        _sut = new OrderService(
            _mockBackgroundWorker.Object,
            _emailConfig,
            _mockEmailService.Object,
            _mockLogger.Object,
            _mockLlmService.Object,
            _llmConfig,
            _orderConfig,
            _mockOrderRepository.Object,
            _mockPdfCompiler.Object,
            _processOrderPrompt,
            _mockRecipeService.Object,
            _mockScope.Object,
            _mockSessionManager.Object,
            _mockCustomerService.Object
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
        
        var session = new SessionBuilder()
            .WithOrder(expectedOrder)
            .Build();

        _mockSessionManager
            .Setup(x => x.GetSessionByOrder(orderId, It.IsAny<Guid>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.GetOrder(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedOrder);
        _mockSessionManager.Verify(x => x.GetSessionByOrder(orderId, It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetOrder_OrderNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var orderId = "non-existent-order";
        var session = new SessionBuilder()
            .WithOrder(null)
            .Build();

        _mockSessionManager
            .Setup(x => x.GetSessionByOrder(orderId, It.IsAny<Guid>()))
            .ReturnsAsync(session);

        // Act
        Func<Task> act = async () => await _sut.GetOrder(orderId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"No order found using ID: {orderId}");
        
        _mockSessionManager.Verify(x => x.EndSession(orderId), Times.Once);
    }

    [Fact]
    public async Task ProcessSubmission_ValidSubmission_CreatesOrderSuccessfully()
    {
        // Arrange
        var submission = new CookbookOrderSubmissionBuilder()
            .WithEmail("customer@test.com")
            .WithSpecificRecipes("Recipe 1", "Recipe 2")
            .Build();

        var customer = new CustomerBuilder()
            .WithEmail("customer@test.com")
            .Build();

        var recipeList = new List<string> { "Recipe 1", "Recipe 2" };
        var conversationId = "conv-123";

        _mockCustomerService
            .Setup(x => x.GetOrCreateCustomer(submission.Email))
            .ReturnsAsync(customer);

        _mockLlmService
            .Setup(x => x.CallFunction<RecipeProposalResult>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<FunctionDefinition>(),
                It.IsAny<string?>(),
                It.IsAny<int?>()))
            .ReturnsAsync(new LlmResult<RecipeProposalResult>
            {
                Data = new RecipeProposalResult { Recipes = recipeList },
                ConversationId = conversationId
            });

        _mockSessionManager
            .Setup(x => x.AddOrder(It.IsAny<IScopeContainer>(), It.IsAny<CookbookOrder>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessSubmission(submission, processOrder: false);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(submission.Email);
        result.RecipeList.Should().BeEquivalentTo(recipeList);
        result.LlmConversationId.Should().Be(conversationId);
        result.Customer.Should().Be(customer);
        result.Status.Should().Be(OrderStatus.Submitted);

        _mockCustomerService.Verify(x => x.GetOrCreateCustomer(submission.Email), Times.Once);
        _mockSessionManager.Verify(x => x.AddOrder(_mockScope.Object, It.IsAny<CookbookOrder>()), Times.Once);
    }

    [Fact]
    public async Task ProcessSubmission_WithProcessOrder_QueuesBackgroundWork()
    {
        // Arrange
        var submission = new CookbookOrderSubmissionBuilder()
            .WithEmail("customer@test.com")
            .Build();

        var customer = new CustomerBuilder().Build();
        var recipeList = new List<string> { "Recipe 1" };

        _mockCustomerService
            .Setup(x => x.GetOrCreateCustomer(It.IsAny<string>()))
            .ReturnsAsync(customer);

        _mockLlmService
            .Setup(x => x.CallFunction<RecipeProposalResult>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<FunctionDefinition>(),
                It.IsAny<string?>(),
                It.IsAny<int?>()))
            .ReturnsAsync(new LlmResult<RecipeProposalResult>
            {
                Data = new RecipeProposalResult { Recipes = recipeList },
                ConversationId = "conv-123"
            });

        // Act
        var result = await _sut.ProcessSubmission(submission, processOrder: true);

        // Assert
        result.Should().NotBeNull();
        _mockBackgroundWorker.Verify(x => x.QueueBackgroundWorkAsync(
            It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
            null
        ), Times.Once);
    }

    [Fact]
    public async Task ProcessOrder_OrderNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var orderId = "non-existent";
        var session = new SessionBuilder()
            .WithOrder(null)
            .Build();

        _mockSessionManager
            .Setup(x => x.GetSessionByOrder(orderId, It.IsAny<Guid>()))
            .ReturnsAsync(session);

        // Act
        Func<Task> act = async () => await _sut.ProcessOrder(orderId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"No order found using ID: {orderId}");
    }

    [Fact]
    public async Task ProcessOrder_CustomerHasNoCredits_SendsEmailAndReturns()
    {
        // Arrange
        var order = new CookbookOrderBuilder()
            .WithCustomer(new CustomerBuilder().WithNoCredits().Build())
            .Build();

        var session = new SessionBuilder()
            .WithOrder(order)
            .Build();

        _mockSessionManager
            .Setup(x => x.GetSessionByOrder(order.OrderId, It.IsAny<Guid>()))
            .ReturnsAsync(session);

        // Act
        await _sut.ProcessOrder(order.OrderId);

        // Assert
        order.Status.Should().Be(OrderStatus.AwaitingPayment);
        _mockEmailService.Verify(x => x.SendEmail(
            order.Email,
            "Cookbook Factory - Order Pending",
            "credits-needed",
            It.IsAny<Dictionary<string, object>>(),
            null
        ), Times.Once);
    }

    [Fact]
    public async Task CompilePdf_ValidOrder_CallsPdfCompiler()
    {
        // Arrange
        var order = new CookbookOrderBuilder()
            .WithSynthesizedRecipes(
                new SynthesizedRecipeBuilder().WithTitle("Recipe 1").Build()
            )
            .Build();

        _mockOrderRepository
            .Setup(x => x.SaveCookbookAsync(order.OrderId, It.Is<byte[]>(b => b != null && b.Length > 0)))
            .Verifiable();

        // Act
        await _sut.CompilePdf(order, false);

        // Assert
        _mockOrderRepository.Verify(x => x.SaveCookbookAsync(order.OrderId, It.Is<byte[]>(b => b != null && b.Length > 0)), Times.Once);
    }

    [Fact]
    public async Task CompilePdf_NoRecipes_ThrowsException()
    {
        // Arrange
        var order = new CookbookOrderBuilder()
            .WithSynthesizedRecipes()
            .Build();

        // Act
        Func<Task> act = async () => await _sut.CompilePdf(order);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Cannot assemble pdf as this order contains no recipes");
    }

    [Fact]
    public async Task EmailCookbook_ValidOrderId_SendsEmailWithAttachment()
    {
        // Arrange
        var orderId = "test-order";
        var order = new CookbookOrderBuilder()
            .WithOrderId(orderId)
            .WithSynthesizedRecipes(
                new SynthesizedRecipeBuilder().Build()
            )
            .Build();

        var session = new SessionBuilder()
            .WithOrder(order)
            .Build();

        var pdfBytes = new byte[] { 1, 2, 3 };

        _mockSessionManager
            .Setup(x => x.GetSessionByOrder(orderId, It.IsAny<Guid>()))
            .ReturnsAsync(session);

        _mockOrderRepository
            .Setup(x => x.GetCookbook(orderId))
            .ReturnsAsync(pdfBytes);

        // Act
        await _sut.EmailCookbook(orderId);

        // Assert
        _mockEmailService.Verify(x => x.SendEmail(
            order.Email,
            "Your Cookbook is Ready!",
            "cookbook-ready",
            It.IsAny<Dictionary<string, object>>(),
            It.Is<Microsoft.Graph.Models.FileAttachment>(a => 
                a.Name == "Cookbook.pdf")
        ), Times.Once);
    }

    [Fact]
    public async Task GetPdf_ValidOrderId_ReturnsPdfBytes()
    {
        // Arrange
        var orderId = "test-order";
        var expectedPdf = new byte[] { 1, 2, 3, 4, 5 };

        _mockOrderRepository
            .Setup(x => x.GetCookbook(orderId))
            .ReturnsAsync(expectedPdf);

        // Act
        var result = await _sut.GetPdf(orderId);

        // Assert
        result.Should().BeEquivalentTo(expectedPdf);
        _mockOrderRepository.Verify(x => x.GetCookbook(orderId), Times.Once);
    }

    [Fact]
    public async Task GetPdf_PdfNotFound_ThrowsException()
    {
        // Arrange
        var orderId = "test-order";

        _mockOrderRepository
            .Setup(x => x.GetCookbook(orderId))
            .ReturnsAsync((byte[]?)null);

        // Act
        Func<Task> act = async () => await _sut.GetPdf(orderId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Cookbook PDF not found for order {orderId}");
    }

    [Fact]
    public void QueueOrderProcessing_ValidOrderId_QueuesBackgroundWork()
    {
        // Arrange
        var orderId = "test-order-123";

        // Act
        _sut.QueueOrderProcessing(orderId);

        // Assert
        _mockBackgroundWorker.Verify(x => x.QueueBackgroundWorkAsync(
            It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
            null
        ), Times.Once);
    }

    [Fact]
    public void QueueOrderProcessing_EmptyOrderId_ThrowsArgumentException()
    {
        // Arrange
        var orderId = "";

        // Act
        Action act = () => _sut.QueueOrderProcessing(orderId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("OrderId cannot be empty*")
            .WithParameterName("orderId");
    }

    [Fact]
    public void QueueOrderProcessing_NullOrderId_ThrowsArgumentException()
    {
        // Arrange
        string? orderId = null;

        // Act
        Action act = () => _sut.QueueOrderProcessing(orderId!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("OrderId cannot be empty*")
            .WithParameterName("orderId");
    }
}
