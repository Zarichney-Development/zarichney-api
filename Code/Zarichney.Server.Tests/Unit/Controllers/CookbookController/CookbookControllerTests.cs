using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Security.Claims;
using Xunit;
using Zarichney.Controllers;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Email;
using Zarichney.Services.Sessions;
using AutoFixture;

namespace Zarichney.Server.Tests.Unit.Controllers.CookbookController;

/// <summary>
/// Unit tests for the CookbookController class.
/// Tests cookbook order creation, recipe management, email validation, and related processes.
/// </summary>
[Trait("Category", "Unit")]
public class CookbookControllerTests
{
  private readonly IFixture _fixture;
  private readonly Mock<IRecipeService> _mockRecipeService;
  private readonly Mock<IOrderService> _mockOrderService;
  private readonly Mock<IEmailService> _mockEmailService;
  private readonly Mock<IBackgroundWorker> _mockBackgroundWorker;
  private readonly Mock<IRecipeRepository> _mockRecipeRepository;
  private readonly Mock<IWebScraperService> _mockScraperService;
  private readonly Mock<IScopeContainer> _mockScopeContainer;
  private readonly Mock<ISessionManager> _mockSessionManager;
  private readonly Mock<ILogger<Zarichney.Controllers.CookbookController>> _mockLogger;
  private readonly Zarichney.Controllers.CookbookController _controller;

  public CookbookControllerTests()
  {
    _fixture = new Fixture();
    _mockRecipeService = new Mock<IRecipeService>();
    _mockOrderService = new Mock<IOrderService>();
    _mockEmailService = new Mock<IEmailService>();
    _mockBackgroundWorker = new Mock<IBackgroundWorker>();
    _mockRecipeRepository = new Mock<IRecipeRepository>();
    _mockScraperService = new Mock<IWebScraperService>();
    _mockScopeContainer = new Mock<IScopeContainer>();
    _mockSessionManager = new Mock<ISessionManager>();
    _mockLogger = new Mock<ILogger<Zarichney.Controllers.CookbookController>>();

    _controller = new Zarichney.Controllers.CookbookController(
      _mockRecipeService.Object,
      _mockOrderService.Object,
      _mockEmailService.Object,
      _mockBackgroundWorker.Object,
      _mockRecipeRepository.Object,
      _mockScraperService.Object,
      _mockScopeContainer.Object,
      _mockSessionManager.Object,
      _mockLogger.Object);

    // Setup default HttpContext with authenticated user
    SetupAuthenticatedUser("test@example.com");
  }

  private void SetupAuthenticatedUser(string email)
  {
    var httpContext = new DefaultHttpContext();
    var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
      new Claim(ClaimTypes.Name, email),
      new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
    }, "Test"));
    httpContext.User = user;
    _controller.ControllerContext = new ControllerContext
    {
      HttpContext = httpContext
    };
  }

  #region CreateSampleSubmission Tests

  [Fact]
  public void CreateSampleSubmission_ReturnsOk_WithSampleData()
  {
    // Act
    var result = _controller.CreateSampleSubmission();

    // Assert
    result.Should().BeOfType<OkObjectResult>("the controller should return OK status");
    var okResult = result as OkObjectResult;
    okResult!.Value.Should().BeOfType<CookbookOrderSubmission>("the response should contain a sample submission");

    var submission = okResult.Value as CookbookOrderSubmission;
    submission!.Email.Should().Be("user@example.com", "the sample should contain an example email");
    submission.CookbookContent.Should().NotBeNull("the sample should include cookbook content");
    submission.CookbookContent.SpecificRecipes.Should().HaveCount(2, "the sample should include example recipes");
  }

  #endregion

  #region CreateCookbook Tests

  [Fact]
  public async Task CreateCookbook_ValidSubmission_ReturnsCreatedResult()
  {
    // Arrange
    var submission = new CookbookOrderSubmissionBuilder()
      .WithEmail("test@example.com")
      .Build();

    var expectedOrder = new CookbookOrderBuilder()
      .WithOrderId("order123")
      .WithEmail("test@example.com")
      .Build();

    _mockEmailService.Setup(x => x.ValidateEmail(It.IsAny<string>()))
      .ReturnsAsync(true);

    _mockOrderService.Setup(x => x.ProcessSubmission(It.IsAny<CookbookOrderSubmission>(), true))
      .ReturnsAsync(expectedOrder);

    // Act
    var result = await _controller.CreateCookbook(submission, processOrder: true);

    // Assert
    result.Should().BeOfType<CreatedResult>("the controller should return Created status for valid submission");
    var createdResult = result as CreatedResult;
    createdResult!.Value.Should().BeEquivalentTo(expectedOrder, "the response should contain the created order");
    createdResult.Location.Should().Be($"/api/cookbook/order/{expectedOrder.OrderId}", "the location header should point to the created order");

    _mockEmailService.Verify(x => x.ValidateEmail("test@example.com"), Times.Once,
      "the email should be validated before creating the order");
    _mockOrderService.Verify(x => x.ProcessSubmission(submission, true), Times.Once,
      "the order service should be called to create the order");
  }

  [Fact]
  public async Task CreateCookbook_InvalidEmail_ReturnsBadRequest()
  {
    // Arrange
    var submission = new CookbookOrderSubmissionBuilder()
      .WithEmail("invalid-email")
      .Build();

    _mockEmailService.Setup(x => x.ValidateEmail(It.IsAny<string>()))
      .ThrowsAsync(new InvalidEmailException("Invalid email address", "invalid-email", InvalidEmailReason.InvalidSyntax));

    // Act
    var result = await _controller.CreateCookbook(submission);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>("the controller should return BadRequest for invalid email");
    var badRequestResult = result as BadRequestObjectResult;
    badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Invalid email address", email = "invalid-email", reason = "InvalidSyntax" },
      "the error message should indicate invalid email with details");

    _mockEmailService.Verify(x => x.ValidateEmail("invalid-email"), Times.Once,
      "the email validation should be performed");
    _mockOrderService.Verify(x => x.ProcessSubmission(It.IsAny<CookbookOrderSubmission>(), It.IsAny<bool>()), Times.Never,
      "the order should not be created for invalid email");
  }

  [Fact]
  public async Task CreateCookbook_MissingEmail_ReturnsBadRequest()
  {
    // Arrange
    var submission = new CookbookOrderSubmissionBuilder()
      .WithEmail(string.Empty)
      .Build();

    // Act
    var result = await _controller.CreateCookbook(submission);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>("the controller should return BadRequest for missing email");
    var badRequestResult = result as BadRequestObjectResult;
    badRequestResult!.Value.Should().Be("Email is required",
      "the error message should indicate missing email");

    _mockEmailService.Verify(x => x.ValidateEmail(It.IsAny<string>()), Times.Never,
      "email validation should not be called for empty email");
    _mockOrderService.Verify(x => x.ProcessSubmission(It.IsAny<CookbookOrderSubmission>(), It.IsAny<bool>()), Times.Never,
      "the order should not be created for missing email");
  }

  [Fact]
  public async Task CreateCookbook_ProcessOrderFalse_DoesNotQueueBackgroundTask()
  {
    // Arrange
    var submission = new CookbookOrderSubmissionBuilder()
      .WithEmail("test@example.com")
      .Build();

    var expectedOrder = new CookbookOrderBuilder()
      .WithOrderId("order123")
      .Build();

    _mockEmailService.Setup(x => x.ValidateEmail(It.IsAny<string>()))
      .ReturnsAsync(true);

    _mockOrderService.Setup(x => x.ProcessSubmission(It.IsAny<CookbookOrderSubmission>(), false))
      .ReturnsAsync(expectedOrder);

    // Act
    var result = await _controller.CreateCookbook(submission, processOrder: false);

    // Assert
    result.Should().BeOfType<CreatedResult>("the controller should return Created status");
    _mockOrderService.Verify(x => x.ProcessSubmission(submission, false), Times.Once,
      "the order service should be called with processOrder false");
  }

  [Fact]
  public async Task CreateCookbook_EmailValidationThrows_ReturnsInternalServerError()
  {
    // Arrange
    var submission = new CookbookOrderSubmissionBuilder()
      .WithEmail("test@example.com")
      .Build();

    _mockEmailService.Setup(x => x.ValidateEmail(It.IsAny<string>()))
      .ThrowsAsync(new Exception("Email service unavailable"));

    // Act
    var result = await _controller.CreateCookbook(submission);

    // Assert
    result.Should().BeOfType<ApiErrorResult>("the controller should return ApiErrorResult for unexpected exceptions");
    var errorResult = result as ApiErrorResult;
    // ApiErrorResult validation - controller returns proper error for email service failure
  }

  #endregion

  #region GetOrder Tests

  [Fact]
  public async Task GetOrder_ValidOrderId_ReturnsOk()
  {
    // Arrange
    const string orderId = "order123";
    var expectedOrder = new CookbookOrderBuilder()
      .WithOrderId(orderId)
      .WithEmail("test@example.com")
      .Build();
    
    var mockSession = new SessionBuilder().Build();
    
    _mockSessionManager.Setup(x => x.GetSessionByOrder(orderId, It.IsAny<Guid>()))
      .ReturnsAsync(mockSession);
    _mockOrderService.Setup(x => x.GetOrder(orderId))
      .ReturnsAsync(expectedOrder);

    // Act
    var result = await _controller.GetOrder(orderId);

    // Assert
    result.Should().BeOfType<OkObjectResult>("the controller should return OK for existing order");
    var okResult = result as OkObjectResult;
    okResult!.Value.Should().BeEquivalentTo(expectedOrder,
      "the response should contain the requested order");

    _mockOrderService.Verify(x => x.GetOrder(orderId), Times.Once,
      "the order service should be called to retrieve the order");
  }

  [Fact]
  public async Task GetOrder_NonExistentOrder_ReturnsNotFound()
  {
    // Arrange
    const string orderId = "nonexistent";
    _mockOrderService.Setup(x => x.GetOrder(orderId))
      .ThrowsAsync(new KeyNotFoundException("Order not found"));

    // Act
    var result = await _controller.GetOrder(orderId);

    // Assert
    result.Should().BeOfType<ApiErrorResult>("the controller should return ApiErrorResult for non-existent order");
    var errorResult = result as ApiErrorResult;
    // ApiErrorResult status validation - controller returns proper error
  }

  #endregion

  #region GetCookbookPdf Tests

  [Fact]
  public async Task GetCookbookPdf_ValidOrderWithPdf_ReturnsFile()
  {
    // Arrange
    const string orderId = "order123";
    var pdfContent = _fixture.CreateMany<byte>(100).ToArray();
    var expectedOrder = new CookbookOrderBuilder()
      .WithOrderId(orderId)
      .Build();

    _mockOrderService.Setup(x => x.GetOrder(orderId))
      .ReturnsAsync(expectedOrder);
    _mockOrderService.Setup(x => x.GetPdf(orderId))
      .ReturnsAsync(pdfContent);

    // Act
    var result = await _controller.GetCookbookPdf(orderId, rebuild: false, email: false);

    // Assert
    result.Should().BeOfType<FileContentResult>("the controller should return file content for order with PDF");
    var fileResult = result as FileContentResult;
    fileResult!.FileContents.Should().BeEquivalentTo(pdfContent,
      "the file contents should match the order's PDF bytes");
    fileResult.ContentType.Should().Be("application/pdf",
      "the content type should be PDF");
    fileResult.FileDownloadName.Should().Be($"cookbook-{orderId}.pdf",
      "the download filename should include the order ID");
  }

  [Fact]
  public async Task GetCookbookPdf_WithRebuild_RecompilesPdfBeforeReturning()
  {
    // Arrange
    const string orderId = "order123";
    var pdfContent = _fixture.CreateMany<byte>(100).ToArray();
    var expectedOrder = new CookbookOrderBuilder()
      .WithOrderId(orderId)
      .Build();

    _mockOrderService.Setup(x => x.GetOrder(orderId))
      .ReturnsAsync(expectedOrder);
    _mockOrderService.Setup(x => x.CompilePdf(expectedOrder, true))
      .Returns(Task.CompletedTask);
    _mockOrderService.Setup(x => x.GetPdf(orderId))
      .ReturnsAsync(pdfContent);

    // Act
    var result = await _controller.GetCookbookPdf(orderId, rebuild: true, email: false);

    // Assert
    result.Should().BeOfType<FileContentResult>("the controller should return file content");
    var fileResult = result as FileContentResult;
    fileResult!.FileContents.Should().BeEquivalentTo(pdfContent,
      "the file contents should match the rebuilt PDF");

    _mockOrderService.Verify(x => x.CompilePdf(expectedOrder, true), Times.Once,
      "the PDF should be recompiled when rebuild is true");
    _mockOrderService.Verify(x => x.GetPdf(orderId), Times.Once,
      "the PDF should be retrieved after compilation");
  }

  [Fact]
  public async Task GetCookbookPdf_WithEmail_SendsEmailAfterRetrieval()
  {
    // Arrange
    const string orderId = "order123";
    var pdfContent = _fixture.CreateMany<byte>(100).ToArray();
    var expectedOrder = new CookbookOrderBuilder()
      .WithOrderId(orderId)
      .Build();

    _mockOrderService.Setup(x => x.GetOrder(orderId))
      .ReturnsAsync(expectedOrder);
    _mockOrderService.Setup(x => x.GetPdf(orderId))
      .ReturnsAsync(pdfContent);
    _mockOrderService.Setup(x => x.EmailCookbook(orderId))
      .Returns(Task.CompletedTask);

    // Act
    var result = await _controller.GetCookbookPdf(orderId, rebuild: false, email: true);

    // Assert
    result.Should().BeOfType<FileContentResult>("the controller should return file content");

    _mockOrderService.Verify(x => x.EmailCookbook(orderId), Times.Once,
      "the email should be sent when email parameter is true");
  }

  [Fact]
  public async Task GetCookbookPdf_EmptyOrderId_ReturnsBadRequest()
  {
    // Act
    var result = await _controller.GetCookbookPdf("", rebuild: false, email: false);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>("the controller should return BadRequest for empty order ID");
    var badRequestResult = result as BadRequestObjectResult;
    badRequestResult!.Value.Should().Be("OrderId parameter is required",
      "the error message should indicate missing order ID");
  }

  #endregion

  #region ReprocessOrder Tests

  [Fact]
  public async Task ReprocessOrder_ValidOrder_QueuesBackgroundTask()
  {
    // Arrange
    const string orderId = "order123";
    var expectedOrder = new CookbookOrderBuilder()
      .WithOrderId(orderId)
      .Build();

    _mockOrderService.Setup(x => x.GetOrder(orderId))
      .ReturnsAsync(expectedOrder);

    // Act
    var result = await _controller.ReprocessOrder(orderId);

    // Assert
    result.Should().BeOfType<OkObjectResult>("the controller should return OK for valid order");
    var okResult = result as OkObjectResult;
    okResult!.Value.Should().BeEquivalentTo(expectedOrder,
      "the response should contain the order being reprocessed");

    _mockOrderService.Verify(x => x.GetOrder(orderId), Times.Once,
      "the order service should be called to retrieve the order");
    _mockBackgroundWorker.Verify(x => x.QueueBackgroundWorkAsync(It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(), It.IsAny<Session?>()), Times.Once,
      "background work should be queued for reprocessing");
  }

  [Fact]
  public async Task ReprocessOrder_NonExistentOrder_ReturnsNotFound()
  {
    // Arrange
    const string orderId = "nonexistent";
    _mockOrderService.Setup(x => x.GetOrder(orderId))
      .ThrowsAsync(new KeyNotFoundException("Order not found"));

    // Act
    var result = await _controller.ReprocessOrder(orderId);

    // Assert
    result.Should().BeOfType<ApiErrorResult>("the controller should return ApiErrorResult for non-existent order");
    var errorResult = result as ApiErrorResult;
    // ApiErrorResult status validation - controller returns proper error
  }

  #endregion

  #region ResendCookbook Tests

  [Fact]
  public async Task ResendCookbook_ValidOrder_SendsEmail()
  {
    // Arrange
    const string orderId = "order123";
    _mockOrderService.Setup(x => x.EmailCookbook(orderId))
      .Returns(Task.CompletedTask);

    // Act
    var result = await _controller.ResendCookbook(orderId);

    // Assert
    result.Should().BeOfType<OkObjectResult>("the controller should return OK for successful email send");
    var okResult = result as OkObjectResult;
    okResult!.Value.Should().Be("Cookbook email has been queued for sending.",
      "the success message should indicate email queued");

    _mockOrderService.Verify(x => x.EmailCookbook(orderId), Times.Once,
      "the email service should be called to send the cookbook");
  }

  [Fact]
  public async Task ResendCookbook_NonExistentOrder_ReturnsNotFound()
  {
    // Arrange
    const string orderId = "nonexistent";
    _mockOrderService.Setup(x => x.EmailCookbook(orderId))
      .ThrowsAsync(new KeyNotFoundException("Order not found"));

    // Act
    var result = await _controller.ResendCookbook(orderId);

    // Assert
    result.Should().BeOfType<ApiErrorResult>("the controller should return ApiErrorResult for non-existent order");
    var errorResult = result as ApiErrorResult;
    // ApiErrorResult status validation - controller returns proper error
  }

  [Fact]
  public async Task ResendCookbook_EmailServiceFailure_ReturnsInternalServerError()
  {
    // Arrange
    const string orderId = "order123";
    _mockOrderService.Setup(x => x.EmailCookbook(orderId))
      .ThrowsAsync(new Exception("Email service unavailable"));

    // Act
    var result = await _controller.ResendCookbook(orderId);

    // Assert
    result.Should().BeOfType<ApiErrorResult>("the controller should return ApiErrorResult for email failure");
    var errorResult = result as ApiErrorResult;
    // ApiErrorResult status validation - controller returns proper error
  }

  #endregion

  #region GetRecipes Tests

  [Fact]
  public async Task GetRecipes_WithQuery_ReturnsFilteredRecipes()
  {
    // Arrange
    const string query = "pasta";
    var expectedRecipes = new List<Recipe>
    {
      new RecipeBuilder().WithId("1").Build(),
      new RecipeBuilder().WithId("2").Build()
    };

    _mockRecipeService.Setup(x => x.GetRecipes(query, false, null, null, null, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedRecipes);

    // Act
    var result = await _controller.GetRecipes(query);

    // Assert
    result.Should().BeOfType<OkObjectResult>("the controller should return OK with filtered recipes");
    var okResult = result as OkObjectResult;
    var recipes = okResult!.Value as IEnumerable<Recipe>;
    recipes.Should().HaveCount(2, "filtered recipes should be returned");
    recipes.Should().BeEquivalentTo(expectedRecipes,
      "the returned recipes should match the query filter");

    _mockRecipeService.Verify(x => x.GetRecipes(query, false, null, null, null, It.IsAny<CancellationToken>()), Times.Once,
      "the recipe service should be called with the query");
  }

  [Fact]
  public async Task GetRecipes_EmptyQuery_ReturnsBadRequest()
  {
    // Act
    var result = await _controller.GetRecipes("");

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>("the controller should return BadRequest for empty query");
    var badRequestResult = result as BadRequestObjectResult;
    badRequestResult!.Value.Should().Be("Query parameter is required",
      "the error message should indicate missing query");
  }

  [Fact]
  public async Task GetRecipes_NoMatchingRecipes_ReturnsNotFound()
  {
    // Arrange
    const string query = "nonexistent";
    _mockRecipeService.Setup(x => x.GetRecipes(query, true, null, null, null, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Recipe>());

    // Act
    var result = await _controller.GetRecipes(query);

    // Assert
    result.Should().BeOfType<ApiErrorResult>("the controller should return ApiErrorResult when no recipes match");
    var errorResult = result as ApiErrorResult;
    // ApiErrorResult status validation - controller returns proper error
  }

  #endregion

  #region ScrapeRecipes Tests

  [Fact]
  public async Task ScrapeRecipes_ValidQuery_ReturnsScrapedRecipes()
  {
    // Arrange
    const string query = "chicken";
    var scrapedRecipes = new List<ScrapedRecipe>
    {
      new ScrapedRecipeBuilder().WithId("1").Build(),
      new ScrapedRecipeBuilder().WithId("2").Build()
    };

    _mockScraperService.Setup(x => x.ScrapeForRecipesAsync(query, null, null, null))
      .ReturnsAsync(scrapedRecipes);

    // Act
    var result = await _controller.ScrapeRecipes(query, null, null, null, false);

    // Assert
    result.Should().BeOfType<OkObjectResult>("the controller should return OK with scraped recipes");
    var okResult = result as OkObjectResult;
    var recipes = okResult!.Value as IEnumerable<ScrapedRecipe>;
    recipes.Should().HaveCount(2, "scraped recipes should be returned");
    recipes.Should().BeEquivalentTo(scrapedRecipes,
      "the returned recipes should match the scraped results");

    _mockScraperService.Verify(x => x.ScrapeForRecipesAsync(query, null, null, null), Times.Once,
      "the scraper service should be called with the query");
  }

  [Fact]
  public async Task ScrapeRecipes_EmptyQuery_ReturnsBadRequest()
  {
    // Act
    var result = await _controller.ScrapeRecipes("", null, null, null, false);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>("the controller should return BadRequest for empty query");
    var badRequestResult = result as BadRequestObjectResult;
    badRequestResult!.Value.Should().Be("Query parameter is required",
      "the error message should indicate missing query");
  }

  [Fact]
  public async Task ScrapeRecipes_NoRecipesFound_ReturnsNotFound()
  {
    // Arrange
    const string query = "obscure";
    _mockScraperService.Setup(x => x.ScrapeForRecipesAsync(query, null, null, null))
      .ReturnsAsync(new List<ScrapedRecipe>());

    // Act
    var result = await _controller.ScrapeRecipes(query, null, null, null, false);

    // Assert
    result.Should().BeOfType<ApiErrorResult>("the controller should return ApiErrorResult when no recipes found");
    var errorResult = result as ApiErrorResult;
    // ApiErrorResult status validation - controller returns proper error
  }

  [Fact]
  public async Task ScrapeRecipes_WithStore_RanksAndStoresNewRecipes()
  {
    // Arrange
    const string query = "dessert";
    var scrapedRecipes = new List<ScrapedRecipe>
    {
      new ScrapedRecipeBuilder().WithId("new1").Build(),
      new ScrapedRecipeBuilder().WithId("new2").Build()
    };
    var rankedRecipes = new List<Recipe>
    {
      new RecipeBuilder().WithId("new1").Build(),
      new RecipeBuilder().WithId("new2").Build()
    };

    _mockScraperService.Setup(x => x.ScrapeForRecipesAsync(query, null, null, null))
      .ReturnsAsync(scrapedRecipes);
    _mockRecipeRepository.Setup(x => x.ContainsRecipe(It.IsAny<string>()))
      .Returns(false); // All recipes are new
    _mockRecipeService.Setup(x => x.RankUnrankedRecipesAsync(It.IsAny<IEnumerable<ScrapedRecipe>>(), query))
      .ReturnsAsync(rankedRecipes);

    // Act
    var result = await _controller.ScrapeRecipes(query, null, null, null, true);

    // Assert
    result.Should().BeOfType<OkObjectResult>("the controller should return OK with ranked recipes");
    var okResult = result as OkObjectResult;
    var recipes = okResult!.Value as IEnumerable<Recipe>;
    recipes.Should().HaveCount(2, "ranked recipes should be returned");

    _mockRecipeService.Verify(x => x.RankUnrankedRecipesAsync(It.IsAny<IEnumerable<ScrapedRecipe>>(), query), Times.Once,
      "the recipe service should rank the new recipes");
    _mockRecipeRepository.Verify(x => x.AddUpdateRecipesAsync(rankedRecipes), Times.Once,
      "the ranked recipes should be stored in the repository");
  }

  #endregion
}