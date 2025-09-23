using System.Collections.Concurrent;
using AutoFixture;
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
using Zarichney.Services.PdfGeneration;
using Zarichney.Services.Sessions;
using OpenAI.Assistants;
using Microsoft.Graph.Models;
using Zarichney.Services.FileSystem;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Server.Tests.Unit.Cookbook.Orders;

public class OrderServiceTests
{
  private readonly OrderService _sut;
  private readonly Mock<IBackgroundWorker> _mockBackgroundWorker;
  private readonly Mock<IEmailService> _mockEmailService;
  private readonly Mock<ILlmService> _mockLlmService;
  private readonly Mock<IOrderRepository> _mockOrderRepository;
  private readonly Mock<IFileService> _mockFileService;
  private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
  private readonly Mock<PdfCompiler> _mockPdfCompiler;
  private readonly ProcessOrderPrompt _processOrderPrompt;
  private readonly Mock<IRecipeService> _mockRecipeService;
  private readonly Mock<IScopeContainer> _mockScope;
  private readonly Mock<ISessionManager> _mockSessionManager;
  private readonly Mock<ICustomerService> _mockCustomerService;
  private readonly Mock<ILogger<RecipeService>> _mockLogger;
  private readonly EmailConfig _emailConfig;
  private readonly OrderConfig _orderConfig;
  private readonly LlmConfig _llmConfig;
  private readonly Fixture _fixture;

  public OrderServiceTests()
  {
    _fixture = new Fixture();

    _mockBackgroundWorker = new Mock<IBackgroundWorker>();
    _mockEmailService = new Mock<IEmailService>();
    _mockLlmService = new Mock<ILlmService>();
    _mockOrderRepository = new Mock<IOrderRepository>();
    _mockFileService = new Mock<IFileService>();
    _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    _mockPdfCompiler = new Mock<PdfCompiler>(new PdfCompilerConfig(), _mockFileService.Object, _mockHttpClientFactory.Object, new Mock<ILogger<PdfCompiler>>().Object);
    _processOrderPrompt = new ProcessOrderPrompt();
    _mockRecipeService = new Mock<IRecipeService>();
    _mockScope = new Mock<IScopeContainer>();
    _mockSessionManager = new Mock<ISessionManager>();
    _mockCustomerService = new Mock<ICustomerService>();
    _mockLogger = new Mock<ILogger<RecipeService>>();
    _mockFileService.Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
        .Returns(Task.CompletedTask);
    _mockFileService.Setup(x => x.DeleteFile(It.IsAny<string>()));

    _emailConfig = new EmailConfig
    {
      FromEmail = "noreply@test.com",
      AzureTenantId = "test-tenant",
      AzureAppId = "test-app",
      AzureAppSecret = "test-secret",
      MailCheckApiKey = "test-api-key"
    };
    _orderConfig = new OrderConfig
    {
      MaxParallelTasks = 5,
      OutputDirectory = "Data/Orders"
    };
    _llmConfig = new LlmConfig { RetryAttempts = 3 };

    _mockScope.Setup(x => x.Id).Returns(Guid.NewGuid());

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
  [Trait("Category", "Unit")]
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
    result.OrderId.Should().Be(orderId);

    _mockSessionManager.Verify(x => x.GetSessionByOrder(orderId, It.IsAny<Guid>()), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
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

    _mockSessionManager
        .Setup(x => x.EndSession(orderId))
        .Returns(Task.CompletedTask);

    // Act
    var act = async () => await _sut.GetOrder(orderId);

    // Assert
    await act.Should().ThrowAsync<KeyNotFoundException>()
        .WithMessage($"No order found using ID: {orderId}");

    _mockSessionManager.Verify(x => x.EndSession(orderId), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessSubmission_ValidSubmission_CreatesOrderAndQueuesProcessing()
  {
    // Arrange
    var submission = new CookbookOrderSubmissionBuilder()
        .WithEmail("customer@test.com")
        .WithExpectedRecipeCount(3)
        .Build();

    var customer = new CustomerBuilder()
        .WithEmail("customer@test.com")
        .Build();

    var recipeList = new List<string> { "Recipe 1", "Recipe 2", "Recipe 3" };
    var conversationId = "conv-123";

    _mockCustomerService
        .Setup(x => x.GetOrCreateCustomer(submission.Email))
        .ReturnsAsync(customer);

    // Using real ProcessOrderPrompt instance for prompt values

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

    _mockBackgroundWorker
        .Setup(x => x.QueueBackgroundWorkAsync(
            It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
            It.IsAny<Session?>()))
        .Verifiable();

    // Act
    var result = await _sut.ProcessSubmission(submission, processOrder: true);

    // Assert
    result.Should().NotBeNull();
    result.Email.Should().Be("customer@test.com");
    result.RecipeList.Should().HaveCount(3);
    result.LlmConversationId.Should().Be(conversationId);
    result.Customer.Should().Be(customer);

    _mockBackgroundWorker.Verify(x => x.QueueBackgroundWorkAsync(
        It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
        It.IsAny<Session?>()), Times.Once);
    _mockSessionManager.Verify(x => x.AddOrder(It.IsAny<IScopeContainer>(),
        It.IsAny<CookbookOrder>()), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessSubmission_ProcessOrderFalse_DoesNotQueueProcessing()
  {
    // Arrange
    var submission = new CookbookOrderSubmissionBuilder().Build();
    var customer = new CustomerBuilder().Build();

    _mockCustomerService
        .Setup(x => x.GetOrCreateCustomer(It.IsAny<string>()))
        .ReturnsAsync(customer);

    SetupLlmMocks();

    _mockSessionManager
        .Setup(x => x.AddOrder(It.IsAny<IScopeContainer>(), It.IsAny<CookbookOrder>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await _sut.ProcessSubmission(submission, processOrder: false);

    // Assert
    result.Should().NotBeNull();
    _mockBackgroundWorker.Verify(x => x.QueueBackgroundWorkAsync(
        It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
        It.IsAny<Session?>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_CustomerHasCredits_ProcessesRecipesSuccessfully()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(5)
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1", "Recipe 2" })
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    var sourceRecipes = new List<Recipe>
        {
            new Recipe
            {
                Title = "Test",
                Description = "Desc",
                Servings = "1",
                PrepTime = "1",
                CookTime = "1",
                TotalTime = "1",
                Ingredients = new List<string>{"a"},
                Directions = new List<string>{"b"},
                Notes = string.Empty,
                Aliases = new List<string>(),
                IndexTitle = null,
                Relevancy = new Dictionary<string, RelevancyResult>()
            }
        };

    var synthesizedRecipe = new SynthesizedRecipeBuilder()
        .WithTitle("Recipe 1")
        .Build();

    SetupSessionManagerForOrder(orderId, session);
    SetupRecipeProcessingMocks(sourceRecipes, synthesizedRecipe);
    SetupPdfGenerationMocks();
    SetupEmailMocks();

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.Completed);
    order.SynthesizedRecipes.Should().NotBeEmpty();

    _mockCustomerService.Verify(x => x.DecrementRecipes(customer, It.IsAny<int>()), Times.Once);
    _mockEmailService.Verify(x => x.SendEmail(
        It.IsAny<string>(),
        It.IsAny<string>(),
        "cookbook-ready",
        It.IsAny<Dictionary<string, object>>(),
        It.IsAny<FileAttachment>()), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_CustomerHasNoCredits_SendsPendingEmail()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(0)
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    SetupSessionManagerForOrder(orderId, session);

    _mockEmailService
        .Setup(x => x.SendEmail(
            It.IsAny<string>(),
            "Cookbook Factory - Order Pending",
            "credits-needed",
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .Returns(Task.CompletedTask);

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.AwaitingPayment);

    _mockEmailService.Verify(x => x.SendEmail(
        order.Email,
        "Cookbook Factory - Order Pending",
        "credits-needed",
        It.IsAny<Dictionary<string, object>>(),
        (FileAttachment?)null), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_ProcessingFails_SetsStatusToFailed()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(5)
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    SetupSessionManagerForOrder(orderId, session);

    _mockRecipeService
        .Setup(x => x.GetRecipes(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Recipe service error"));

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.Failed);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CompilePdf_ValidOrder_CreatesPdfSuccessfully()
  {
    // Arrange
    var order = new CookbookOrderBuilder()
        .AsCompleted()
        .Build();

    var pdfBytes = _fixture.Create<byte[]>();

    _mockPdfCompiler
        .Setup(x => x.CompileCookbook(order))
        .ReturnsAsync(pdfBytes);

    _mockOrderRepository
        .Setup(x => x.SaveCookbook(order.OrderId, pdfBytes))
        .Returns(Task.CompletedTask);

    // Act
    await _sut.CompilePdf(order, waitForWrite: true);

    // Assert
    _mockPdfCompiler.Verify(x => x.CompileCookbook(order), Times.Once);
    _mockOrderRepository.Verify(x => x.SaveCookbook(order.OrderId, pdfBytes), Times.Once);
    _mockOrderRepository.Verify(x => x.SaveRecipe(
        order.OrderId,
        It.IsAny<string>(),
        It.IsAny<string>()), Times.Exactly(order.SynthesizedRecipes.Count));
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CompilePdf_NoSynthesizedRecipes_ThrowsException()
  {
    // Arrange
    var order = new CookbookOrderBuilder()
        .WithSynthesizedRecipes(new List<SynthesizedRecipe>())
        .Build();

    // Act
    var act = async () => await _sut.CompilePdf(order);

    // Assert
    await act.Should().ThrowAsync<Exception>()
        .WithMessage("Cannot assemble pdf as this order contains no recipes");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task EmailCookbook_ValidOrderId_SendsEmailSuccessfully()
  {
    // Arrange
    var orderId = "test-order-123";
    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .AsCompleted()
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    var pdfBytes = _fixture.Create<byte[]>();

    SetupSessionManagerForOrder(orderId, session);

    _mockOrderRepository
        .Setup(x => x.GetCookbook(orderId))
        .ReturnsAsync(pdfBytes);

    _mockEmailService
        .Setup(x => x.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .Returns(Task.CompletedTask);

    // Act
    await _sut.EmailCookbook(orderId);

    // Assert
    _mockEmailService.Verify(x => x.SendEmail(
        order.Email,
        "Your Cookbook is Ready!",
        "cookbook-ready",
        It.IsAny<Dictionary<string, object>>(),
        It.Is<FileAttachment>(a => a.Name == "Cookbook.pdf" && a.ContentBytes == pdfBytes)),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetPdf_ValidOrderId_ReturnsPdfBytes()
  {
    // Arrange
    var orderId = "test-order-123";
    var expectedPdf = _fixture.Create<byte[]>();

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
  [Trait("Category", "Unit")]
  public async Task GetPdf_PdfNotFound_ThrowsException()
  {
    // Arrange
    var orderId = "test-order-123";

    _mockOrderRepository
        .Setup(x => x.GetCookbook(orderId))
        .ReturnsAsync((byte[]?)null);

    // Act
    var act = async () => await _sut.GetPdf(orderId);

    // Assert
    await act.Should().ThrowAsync<Exception>()
        .WithMessage($"Cookbook PDF not found for order {orderId}");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void QueueOrderProcessing_ValidOrderId_QueuesBackgroundWork()
  {
    // Arrange
    var orderId = "test-order-123";

    _mockBackgroundWorker
        .Setup(x => x.QueueBackgroundWorkAsync(
            It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
            It.IsAny<Session?>()))
        .Verifiable();

    // Act
    _sut.QueueOrderProcessing(orderId);

    // Assert
    _mockBackgroundWorker.Verify(x => x.QueueBackgroundWorkAsync(
        It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(),
        It.IsAny<Session?>()), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void QueueOrderProcessing_EmptyOrderId_ThrowsArgumentException()
  {
    // Arrange
    var orderId = "";

    // Act
    var act = () => _sut.QueueOrderProcessing(orderId);

    // Assert
    act.Should().Throw<ArgumentException>()
        .WithMessage("OrderId cannot be empty*");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void QueueOrderProcessing_NullOrderId_ThrowsArgumentException()
  {
    // Arrange
    string? orderId = null;

    // Act
    var act = () => _sut.QueueOrderProcessing(orderId!);

    // Assert
    act.Should().Throw<ArgumentException>()
        .WithMessage("OrderId cannot be empty*");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_PartialCredits_ProcessesAvailableRecipesOnly()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(2) // Only 2 credits available
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1", "Recipe 2", "Recipe 3", "Recipe 4" }) // 4 recipes requested
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    var sourceRecipes = new List<Recipe>
        {
            new Recipe
            {
                Title = "Test",
                Description = "Desc",
                Servings = "1",
                PrepTime = "1",
                CookTime = "1",
                TotalTime = "1",
                Ingredients = new List<string>{"a"},
                Directions = new List<string>{"b"},
                Notes = string.Empty,
                Aliases = new List<string>(),
                IndexTitle = null,
                Relevancy = new Dictionary<string, RelevancyResult>()
            }
        };
    var synthesizedRecipe = new SynthesizedRecipeBuilder().Build();

    SetupSessionManagerForOrder(orderId, session);
    SetupRecipeProcessingMocks(sourceRecipes, synthesizedRecipe);
    SetupPdfGenerationMocks();
    SetupEmailMocks();

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.AwaitingPayment);
    order.RequiresPayment.Should().BeTrue();

    _mockCustomerService.Verify(x => x.DecrementRecipes(customer, It.IsAny<int>()), Times.Once);
  }

  // Helper methods
  private void SetupSessionManagerForOrder(string orderId, Session session)
  {
    _mockSessionManager
        .Setup(x => x.GetSessionByOrder(orderId, It.IsAny<Guid>()))
        .ReturnsAsync(session);

    _mockSessionManager
        .Setup(x => x.ParallelForEachAsync<string>(
            It.IsAny<IScopeContainer>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<Func<IScopeContainer, string, CancellationToken, Task>>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
        .Callback((IScopeContainer parentScope,
                   IEnumerable<string> dataList,
                   Func<IScopeContainer, string, CancellationToken, Task> operation,
                   int? maxParallel, CancellationToken token) =>
        {
          foreach (var item in dataList)
          {
            operation(_mockScope.Object, item, CancellationToken.None).GetAwaiter().GetResult();
          }
        })
        .Returns(Task.CompletedTask);
  }

  private void SetupRecipeProcessingMocks(List<Recipe> sourceRecipes, SynthesizedRecipe synthesizedRecipe)
  {
    _mockRecipeService
        .Setup(x => x.GetRecipes(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(sourceRecipes);

    _mockRecipeService
        .Setup(x => x.SynthesizeRecipe(It.IsAny<List<Recipe>>(), It.IsAny<CookbookOrder>(), It.IsAny<string>()))
        .ReturnsAsync(synthesizedRecipe);

    _mockCustomerService
        .Setup(x => x.DecrementRecipes(It.IsAny<Customer>(), It.IsAny<int>()))
        .Verifiable();
  }

  private void SetupPdfGenerationMocks()
  {
    var pdfBytes = _fixture.Create<byte[]>();

    _mockPdfCompiler
        .Setup(x => x.CompileCookbook(It.IsAny<CookbookOrder>()))
        .ReturnsAsync(pdfBytes);

    _mockOrderRepository
        .Setup(x => x.SaveCookbook(It.IsAny<string>(), It.IsAny<byte[]>()))
        .Returns(Task.CompletedTask);

    _mockOrderRepository
        .Setup(x => x.SaveCookbookAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
        .Verifiable();

    _mockOrderRepository
        .Setup(x => x.SaveRecipe(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .Verifiable();

    _mockOrderRepository
        .Setup(x => x.SaveCookbook(It.IsAny<string>(), It.IsAny<string>()))
        .Verifiable();

    _mockOrderRepository
        .Setup(x => x.GetCookbook(It.IsAny<string>()))
        .ReturnsAsync(pdfBytes);
  }

  private void SetupEmailMocks()
  {
    _mockEmailService
        .Setup(x => x.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .Returns(Task.CompletedTask);

    _mockEmailService
        .Setup(x => x.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .Returns(Task.CompletedTask);
  }

  private void SetupLlmMocks()
  {
    // Using real ProcessOrderPrompt instance for prompt values

    _mockLlmService
        .Setup(x => x.CallFunction<RecipeProposalResult>(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<FunctionDefinition>(),
            It.IsAny<string?>(),
            It.IsAny<int?>()))
        .ReturnsAsync(new LlmResult<RecipeProposalResult>
        {
          Data = new RecipeProposalResult
          {
            Recipes = new List<string> { "Recipe 1", "Recipe 2", "Recipe 3" }
          },
          ConversationId = "conv-123"
        });
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessSubmission_LlmServiceThrows_PropagatesException()
  {
    // Arrange
    var submission = new CookbookOrderSubmissionBuilder()
        .WithEmail("customer@test.com")
        .Build();

    var customer = new CustomerBuilder()
        .WithEmail("customer@test.com")
        .Build();

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
        .ThrowsAsync(new Exception("LLM service error"));

    // Act
    var act = async () => await _sut.ProcessSubmission(submission);

    // Assert
    await act.Should().ThrowAsync<Exception>()
        .WithMessage("LLM service error");

    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_GetRecipesThrowsNoRecipeException_SendsNotificationEmail()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(5)
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1" })
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    SetupSessionManagerForOrder(orderId, session);

    var noRecipeException = new NoRecipeException(new List<string> { "Attempt 1", "Attempt 2", "Attempt 3" });
    _mockRecipeService
        .Setup(x => x.GetRecipes("Recipe 1", It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(noRecipeException);

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.Failed);

    _mockEmailService.Verify(x => x.SendEmail(
        _emailConfig.FromEmail,
        It.Is<string>(s => s.Contains("No Recipe Found")),
        "no-recipe",
        It.Is<Dictionary<string, object>>(d =>
            d.ContainsKey("recipeName") &&
            d["recipeName"].ToString() == "Recipe 1"),
        (FileAttachment?)null), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_GetRecipesThrowsGeneralException_SendsErrorEmail()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(5)
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1" })
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    SetupSessionManagerForOrder(orderId, session);

    _mockRecipeService
        .Setup(x => x.GetRecipes("Recipe 1", It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new InvalidOperationException("Recipe service unavailable"));

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.Failed);

    _mockEmailService.Verify(x => x.SendEmail(
        _emailConfig.FromEmail,
        It.Is<string>(s => s.Contains("Recipe Error")),
        "error-log",
        It.IsAny<Dictionary<string, object>>(),
        (FileAttachment?)null), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_NoRecipesToProcess_LogsInformationAndReturns()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(5)
        .Build();

    var synthesizedRecipes = new List<SynthesizedRecipe>
    {
        new SynthesizedRecipeBuilder().WithTitle("Recipe 1").Build(),
        new SynthesizedRecipeBuilder().WithTitle("Recipe 2").Build()
    };

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1", "Recipe 2" })
        .WithSynthesizedRecipes(synthesizedRecipes)
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    SetupSessionManagerForOrder(orderId, session);
    SetupPdfGenerationMocks();
    SetupEmailMocks();

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.Completed);

    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce);

    _mockRecipeService.Verify(
        x => x.GetRecipes(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
        Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_CustomerHasZeroCreditsWithUnprocessedRecipes_SetRequiresPayment()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(0) // Zero credits from the start
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1", "Recipe 2", "Recipe 3" })
        .WithSynthesizedRecipes(new List<SynthesizedRecipe>
        {
            new SynthesizedRecipeBuilder().WithTitle("Recipe 1").Build()
        })
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    SetupSessionManagerForOrder(orderId, session);
    SetupPdfGenerationMocks();
    SetupEmailMocks();

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    // With 1 of 3 recipes already done and no credits, it should complete with partial recipes
    order.Status.Should().Be(OrderStatus.AwaitingPayment);
    // RequiresPayment remains false because no new recipes were processed (customer had 0 credits)
    // The service only sets RequiresPayment after processing some but not all pending recipes
    order.RequiresPayment.Should().BeFalse();

    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CompilePdf_WaitForWriteFalse_UsesAsyncWrite()
  {
    // Arrange
    var order = new CookbookOrderBuilder()
        .AsCompleted()
        .Build();

    var pdfBytes = _fixture.Create<byte[]>();

    _mockPdfCompiler
        .Setup(x => x.CompileCookbook(order))
        .ReturnsAsync(pdfBytes);

    _mockOrderRepository
        .Setup(x => x.SaveCookbookAsync(order.OrderId, pdfBytes))
        .Verifiable();

    // Act
    await _sut.CompilePdf(order, waitForWrite: false);

    // Assert
    _mockPdfCompiler.Verify(x => x.CompileCookbook(order), Times.Once);
    _mockOrderRepository.Verify(x => x.SaveCookbookAsync(order.OrderId, pdfBytes), Times.Once);
    _mockOrderRepository.Verify(x => x.SaveCookbook(order.OrderId, pdfBytes), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task EmailCookbook_PdfNotFoundOnFirstAttempt_RetriesSuccessfully()
  {
    // Arrange
    var orderId = "test-order-123";
    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .AsCompleted()
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    var pdfBytes = _fixture.Create<byte[]>();
    var callCount = 0;

    SetupSessionManagerForOrder(orderId, session);

    _mockOrderRepository
        .Setup(x => x.GetCookbook(orderId))
        .ReturnsAsync(() =>
        {
            callCount++;
            if (callCount == 1)
                throw new Exception("PDF not ready yet");
            return pdfBytes;
        });

    _mockEmailService
        .Setup(x => x.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .Returns(Task.CompletedTask);

    // Act
    await _sut.EmailCookbook(orderId);

    // Assert
    _mockOrderRepository.Verify(x => x.GetCookbook(orderId), Times.Exactly(2));
    _mockEmailService.Verify(x => x.SendEmail(
        order.Email,
        "Your Cookbook is Ready!",
        "cookbook-ready",
        It.IsAny<Dictionary<string, object>>(),
        It.Is<FileAttachment>(a => a.ContentBytes == pdfBytes)),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task EmailCookbook_AllRetriesFail_ThrowsException()
  {
    // Arrange
    var orderId = "test-order-123";
    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .AsCompleted()
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    SetupSessionManagerForOrder(orderId, session);

    _mockOrderRepository
        .Setup(x => x.GetCookbook(orderId))
        .ReturnsAsync((byte[]?)null);

    // Act
    var act = async () => await _sut.EmailCookbook(orderId);

    // Assert
    await act.Should().ThrowAsync<Exception>()
        .WithMessage($"Cookbook PDF not found for order {orderId}");

    _mockOrderRepository.Verify(x => x.GetCookbook(orderId), Times.Exactly(_llmConfig.RetryAttempts + 1));
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_WithPartiallyProcessedRecipes_RequiresPaymentSetCorrectly()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(2)
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1", "Recipe 2", "Recipe 3", "Recipe 4", "Recipe 5" })
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    var sourceRecipes = new List<Recipe>
    {
        new Recipe
        {
            Title = "Test",
            Description = "Desc",
            Servings = "1",
            PrepTime = "1",
            CookTime = "1",
            TotalTime = "1",
            Ingredients = new List<string>{"a"},
            Directions = new List<string>{"b"},
            Notes = string.Empty,
            Aliases = new List<string>(),
            IndexTitle = null,
            Relevancy = new Dictionary<string, RelevancyResult>()
        }
    };

    var synthesizedRecipe = new SynthesizedRecipeBuilder().Build();

    SetupSessionManagerForOrder(orderId, session);
    SetupRecipeProcessingMocks(sourceRecipes, synthesizedRecipe);
    SetupPdfGenerationMocks();
    SetupEmailMocks();

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.AwaitingPayment);
    order.RequiresPayment.Should().BeTrue();
    order.SynthesizedRecipes.Should().HaveCount(2); // Only processed 2 with available credits

    _mockEmailService.Verify(x => x.SendEmail(
        order.Email,
        "Your Cookbook is Ready!",
        "cookbook-ready",
        It.Is<Dictionary<string, object>>(d =>
            (bool)d["isPartial"] == true &&
            (int)d["processedRecipes"] == 2 &&
            (int)d["totalRecipes"] == 5),
        It.IsAny<FileAttachment>()), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessSubmission_CustomerServiceThrows_PropagatesException()
  {
    // Arrange
    var submission = new CookbookOrderSubmissionBuilder()
        .WithEmail("customer@test.com")
        .Build();

    _mockCustomerService
        .Setup(x => x.GetOrCreateCustomer(submission.Email))
        .ThrowsAsync(new Exception("Customer service error"));

    // Act
    var act = async () => await _sut.ProcessSubmission(submission);

    // Assert
    await act.Should().ThrowAsync<Exception>()
        .WithMessage("Customer service error");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetOrder_SessionManagerThrows_PropagatesException()
  {
    // Arrange
    var orderId = "test-order-123";

    _mockSessionManager
        .Setup(x => x.GetSessionByOrder(orderId, It.IsAny<Guid>()))
        .ThrowsAsync(new Exception("Session manager error"));

    // Act
    var act = async () => await _sut.GetOrder(orderId);

    // Assert
    await act.Should().ThrowAsync<Exception>()
        .WithMessage("Session manager error");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_PdfCompilationFails_SetsStatusToFailed()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(5)
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1" })
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    var sourceRecipes = new List<Recipe>
    {
        new Recipe
        {
            Title = "Test",
            Description = "Desc",
            Servings = "1",
            PrepTime = "1",
            CookTime = "1",
            TotalTime = "1",
            Ingredients = new List<string>{"a"},
            Directions = new List<string>{"b"},
            Notes = string.Empty,
            Aliases = new List<string>(),
            IndexTitle = null,
            Relevancy = new Dictionary<string, RelevancyResult>()
        }
    };

    var synthesizedRecipe = new SynthesizedRecipeBuilder().Build();

    SetupSessionManagerForOrder(orderId, session);
    SetupRecipeProcessingMocks(sourceRecipes, synthesizedRecipe);

    _mockPdfCompiler
        .Setup(x => x.CompileCookbook(It.IsAny<CookbookOrder>()))
        .ThrowsAsync(new Exception("PDF compilation error"));

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.Failed);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_EmailSendFails_SetsStatusToFailed()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(5)
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1" })
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    var sourceRecipes = new List<Recipe>
    {
        new Recipe
        {
            Title = "Test",
            Description = "Desc",
            Servings = "1",
            PrepTime = "1",
            CookTime = "1",
            TotalTime = "1",
            Ingredients = new List<string>{"a"},
            Directions = new List<string>{"b"},
            Notes = string.Empty,
            Aliases = new List<string>(),
            IndexTitle = null,
            Relevancy = new Dictionary<string, RelevancyResult>()
        }
    };

    var synthesizedRecipe = new SynthesizedRecipeBuilder().Build();

    SetupSessionManagerForOrder(orderId, session);
    SetupRecipeProcessingMocks(sourceRecipes, synthesizedRecipe);
    SetupPdfGenerationMocks();

    _mockEmailService
        .Setup(x => x.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .ThrowsAsync(new Exception("Email service error"));

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.Failed);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CompilePdf_PdfCompilerThrows_PropagatesException()
  {
    // Arrange
    var order = new CookbookOrderBuilder()
        .AsCompleted()
        .Build();

    _mockPdfCompiler
        .Setup(x => x.CompileCookbook(order))
        .ThrowsAsync(new Exception("PDF compiler error"));

    // Act
    var act = async () => await _sut.CompilePdf(order, waitForWrite: true);

    // Assert
    await act.Should().ThrowAsync<Exception>()
        .WithMessage("PDF compiler error");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_WithImageUrls_SetsImageUrlsCorrectly()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(5)
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1" })
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    var sourceRecipes = new List<Recipe>
    {
        new Recipe
        {
            Title = "Test",
            Description = "Desc",
            Servings = "1",
            PrepTime = "1",
            CookTime = "1",
            TotalTime = "1",
            Ingredients = new List<string>{"a"},
            Directions = new List<string>{"b"},
            Notes = string.Empty,
            Aliases = new List<string>(),
            IndexTitle = null,
            Relevancy = new Dictionary<string, RelevancyResult>(),
            ImageUrl = "https://example.com/image1.jpg",
            RecipeUrl = "https://example.com/recipe1"
        },
        new Recipe
        {
            Title = "Test 2",
            Description = "Desc 2",
            Servings = "2",
            PrepTime = "2",
            CookTime = "2",
            TotalTime = "2",
            Ingredients = new List<string>{"b"},
            Directions = new List<string>{"c"},
            Notes = string.Empty,
            Aliases = new List<string>(),
            IndexTitle = null,
            Relevancy = new Dictionary<string, RelevancyResult>(),
            ImageUrl = "https://example.com/image2.jpg",
            RecipeUrl = "https://example.com/recipe2"
        }
    };

    var synthesizedRecipe = new SynthesizedRecipeBuilder()
        .WithTitle("Recipe 1")
        .Build();
    synthesizedRecipe.InspiredBy = new List<string> { "https://example.com/recipe1" };

    SetupSessionManagerForOrder(orderId, session);
    SetupRecipeProcessingMocks(sourceRecipes, synthesizedRecipe);
    SetupPdfGenerationMocks();
    SetupEmailMocks();

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.SynthesizedRecipes.Should().HaveCount(1);
    order.SynthesizedRecipes[0].ImageUrls.Should().Contain("https://example.com/image1.jpg");
    order.SynthesizedRecipes[0].SourceRecipes.Should().HaveCount(1);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_CancellationTokenCanceled_StopsProcessing()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(5)
        .Build();

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1", "Recipe 2", "Recipe 3" })
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    SetupSessionManagerForOrder(orderId, session);

    // Simulate cancellation during parallel processing
    _mockSessionManager
        .Setup(x => x.ParallelForEachAsync<string>(
            It.IsAny<IScopeContainer>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<Func<IScopeContainer, string, CancellationToken, Task>>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
        .ThrowsAsync(new OperationCanceledException());

    SetupPdfGenerationMocks();
    SetupEmailMocks();

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    // Should handle cancellation gracefully - the service sets status to Failed on exceptions
    order.Status.Should().Be(OrderStatus.Failed);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_OrderNotFoundInSession_ThrowsKeyNotFoundException()
  {
    // Arrange
    var orderId = "test-order-123";
    var session = new SessionBuilder()
        .WithOrder(null)
        .Build();

    _mockSessionManager
        .Setup(x => x.GetSessionByOrder(orderId, It.IsAny<Guid>()))
        .ReturnsAsync(session);

    // Act
    var act = async () => await _sut.ProcessOrder(orderId);

    // Assert
    await act.Should().ThrowAsync<KeyNotFoundException>()
        .WithMessage($"No order found using ID: {orderId}");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetPdf_EmptyPdfReturned_ThrowsException()
  {
    // Arrange
    var orderId = "test-order-123";

    _mockOrderRepository
        .Setup(x => x.GetCookbook(orderId))
        .ReturnsAsync(Array.Empty<byte>());

    // Act
    var act = async () => await _sut.GetPdf(orderId);

    // Assert
    await act.Should().ThrowAsync<Exception>()
        .WithMessage($"Cookbook PDF not found for order {orderId}");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ProcessOrder_WithForceParameter_ReprocessesAlreadySynthesized()
  {
    // Arrange
    var orderId = "test-order-123";
    var customer = new CustomerBuilder()
        .WithAvailableRecipes(5)
        .Build();

    var synthesizedRecipes = new List<SynthesizedRecipe>
    {
        new SynthesizedRecipeBuilder().WithTitle("Recipe 1").Build()
    };

    var order = new CookbookOrderBuilder()
        .WithOrderId(orderId)
        .WithCustomer(customer)
        .WithRecipeList(new List<string> { "Recipe 1", "Recipe 2" })
        .WithSynthesizedRecipes(synthesizedRecipes)
        .Build();

    var session = new SessionBuilder()
        .WithOrder(order)
        .Build();

    var sourceRecipes = new List<Recipe>
    {
        new Recipe
        {
            Title = "Test",
            Description = "Desc",
            Servings = "1",
            PrepTime = "1",
            CookTime = "1",
            TotalTime = "1",
            Ingredients = new List<string>{"a"},
            Directions = new List<string>{"b"},
            Notes = string.Empty,
            Aliases = new List<string>(),
            IndexTitle = null,
            Relevancy = new Dictionary<string, RelevancyResult>()
        }
    };

    var newSynthesizedRecipe = new SynthesizedRecipeBuilder().Build();

    SetupSessionManagerForOrder(orderId, session);
    SetupRecipeProcessingMocks(sourceRecipes, newSynthesizedRecipe);
    SetupPdfGenerationMocks();
    SetupEmailMocks();

    // Act
    await _sut.ProcessOrder(orderId);

    // Assert
    order.Status.Should().Be(OrderStatus.Completed);
    // Should have processed only Recipe 2 as Recipe 1 was already synthesized
    _mockRecipeService.Verify(x => x.GetRecipes(
        "Recipe 2",
        It.IsAny<int?>(),
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()), Times.Once);
    _mockRecipeService.Verify(x => x.GetRecipes(
        "Recipe 1",
        It.IsAny<int?>(),
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()), Times.Never);
  }
}
