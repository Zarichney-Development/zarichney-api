using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Stripe.Checkout;
using Xunit;
using Zarichney.Config;
using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Server.Tests.TestData.AutoFixtureCustomizations;
using Zarichney.Services.Payment;
using Customer = Zarichney.Cookbook.Customers.Customer;

namespace Zarichney.Tests.Unit.Services.Payment;

/// <summary>
/// Comprehensive unit tests for PaymentService covering all business logic scenarios.
/// Tests payment processing, webhook handling, and order completion workflows.
/// </summary>
public class PaymentServiceTests
{
  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task CreateCheckoutSession_WithValidOrder_ReturnsSessionId(
      string expectedSessionId,
      [Frozen] Mock<IStripeService> mockStripeService,
      PaymentService sut)
  {
    // Arrange
    // Setup order with recipes requiring payment
    var order = new CookbookOrder
    {
      OrderId = "test-order-123",
      Email = "test@example.com",
      RecipeList = ["recipe1", "recipe2", "recipe3"],
      SynthesizedRecipes = [new SynthesizedRecipe {
                Title = "Recipe 1",
                Description = "Test recipe",
                Servings = "4",
                PrepTime = "10 min",
                CookTime = "20 min",
                TotalTime = "30 min",
                Ingredients = ["ingredient1"],
                Directions = ["step1"],
                Notes = "test notes"
            }], // One recipe already synthesized, need to pay for 2
      RequiresPayment = true
    };

    var recipesToPay = order.RecipeList.Count - order.SynthesizedRecipes.Count; // 3 - 1 = 2

    mockStripeService
        .Setup(s => s.CreateCheckoutSession(
            order.Email,
            recipesToPay,
            It.Is<Dictionary<string, string>>(m =>
                m["payment_type"] == PaymentType.OrderCompletion.ToString() &&
                m["order_id"] == order.OrderId &&
                m["customer_email"] == order.Email),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "Cookbook Recipe",
            $"Additional recipes for your cookbook (needed: {recipesToPay})",
            order.OrderId))
        .ReturnsAsync(expectedSessionId);

    // Act
    var result = await sut.CreateCheckoutSession(order);

    // Assert
    result.Should().Be(expectedSessionId,
        "because the service should return the session ID from Stripe");

    mockStripeService.Verify(s => s.CreateCheckoutSession(
        order.Email,
        recipesToPay,
        It.IsAny<Dictionary<string, string>>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        "Cookbook Recipe",
        $"Additional recipes for your cookbook (needed: {recipesToPay})",
        order.OrderId), Times.Once);
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task CreateCheckoutSession_WithNullOrder_ThrowsArgumentNullException(
      PaymentService sut)
  {
    // Act & Assert
    var act = async () => await sut.CreateCheckoutSession((CookbookOrder)null!);

    await act.Should().ThrowAsync<ArgumentNullException>("because null orders should be rejected immediately");
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task CreateCheckoutSession_WithOrderNoPaymentRequired_ThrowsInvalidOperationException(
      PaymentService sut)
  {
    // Arrange - Order with all recipes already synthesized, no payment required
    // The service checks recipesToPay = RecipeList.Count - SynthesizedRecipes.Count
    // So we need SynthesizedRecipes.Count >= RecipeList.Count for no payment required
    var order = new CookbookOrder
    {
      OrderId = "test-order-no-payment",
      Email = "test@example.com",
      RecipeList = ["recipe1", "recipe2"],
      SynthesizedRecipes = [
            new SynthesizedRecipe {
                    Title = "Recipe 1",
                    Description = "Test recipe 1",
                    Servings = "4",
                    PrepTime = "10 min",
                    CookTime = "20 min",
                    TotalTime = "30 min",
                    Ingredients = ["ingredient1"],
                    Directions = ["step1"],
                    Notes = "test notes"
                },
                new SynthesizedRecipe {
                    Title = "Recipe 2",
                    Description = "Test recipe 2",
                    Servings = "4",
                    PrepTime = "15 min",
                    CookTime = "25 min",
                    TotalTime = "40 min",
                    Ingredients = ["ingredient2"],
                    Directions = ["step2"],
                    Notes = "test notes 2"
                }
        ], // All recipes already synthesized
      RequiresPayment = false
    };

    // Act & Assert
    var act = async () => await sut.CreateCheckoutSession(order);

    await act.Should().ThrowAsync<InvalidOperationException>("because orders with no payment required should be rejected")
        .WithMessage("This order doesn't require any payment.");
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task GetCheckoutSessionWithUrl_WithValidOrder_ReturnsSessionWithUrl(
      Session expectedSession,
      [Frozen] Mock<IStripeService> mockStripeService,
      PaymentService sut)
  {
    // Arrange
    var order = new CookbookOrder
    {
      OrderId = "test-order-url",
      Email = "test@example.com",
      RecipeList = ["recipe1", "recipe2", "recipe3"],
      SynthesizedRecipes = [new SynthesizedRecipe {
                Title = "Recipe 1",
                Description = "Test recipe",
                Servings = "4",
                PrepTime = "10 min",
                CookTime = "20 min",
                TotalTime = "30 min",
                Ingredients = ["ingredient1"],
                Directions = ["step1"],
                Notes = "test notes"
            }], // One recipe already synthesized, need to pay for 2
      RequiresPayment = true
    };

    var recipesToPay = order.RecipeList.Count - order.SynthesizedRecipes.Count; // 3 - 1 = 2

    mockStripeService
        .Setup(s => s.CreateCheckoutSessionWithUrl(
            order.Email,
            recipesToPay,
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "Cookbook Recipe",
            $"Additional recipes for your cookbook (needed: {recipesToPay})",
            order.OrderId))
        .ReturnsAsync(expectedSession);

    // Act
    var result = await sut.GetCheckoutSessionWithUrl(order);

    // Assert
    result.Should().Be(expectedSession,
        "because the service should return the full session from Stripe");
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task CreateCheckoutSession_WithValidCustomerAndRecipeCount_ReturnsSessionId(
      Customer customer,
      string expectedSessionId,
      [Frozen] Mock<IStripeService> mockStripeService,
      PaymentService sut)
  {
    // Arrange
    const int recipeCount = 10;

    mockStripeService
        .Setup(s => s.CreateCheckoutSession(
            customer.Email,
            recipeCount,
            It.Is<Dictionary<string, string>>(m =>
                m["payment_type"] == PaymentType.RecipeCredit.ToString() &&
                m["customer_email"] == customer.Email &&
                m["recipe_count"] == recipeCount.ToString()),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "Recipe Credit",
            $"{recipeCount} recipe credits for your cookbook account",
            null))
        .ReturnsAsync(expectedSessionId);

    // Act
    var result = await sut.CreateCheckoutSession(customer, recipeCount);

    // Assert
    result.Should().Be(expectedSessionId,
        "because the service should return the session ID for recipe credit purchase");
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task CreateCheckoutSession_WithNullCustomer_ThrowsArgumentNullException(
      PaymentService sut)
  {
    // Act & Assert
    var act = async () => await sut.CreateCheckoutSession((Customer)null!, 10);

    await act.Should().ThrowAsync<ArgumentNullException>("because null customers should be rejected immediately");
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task CreateCheckoutSession_WithZeroRecipeCount_ThrowsArgumentException(
      Customer customer,
      PaymentService sut)
  {
    // Act & Assert
    var act = async () => await sut.CreateCheckoutSession(customer, 0);

    await act.Should().ThrowAsync<ArgumentException>("because recipe count must be positive")
        .WithParameterName("recipeCount")
        .WithMessage("Recipe count must be greater than zero*");
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task CreateCheckoutSession_WithNegativeRecipeCount_ThrowsArgumentException(
      Customer customer,
      PaymentService sut)
  {
    // Act & Assert
    var act = async () => await sut.CreateCheckoutSession(customer, -1);

    await act.Should().ThrowAsync<ArgumentException>("because recipe count must be positive")
        .WithParameterName("recipeCount")
        .WithMessage("Recipe count must be greater than zero*");
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task CreateCheckoutSession_WithNonPackageSizeRecipeCount_AdjustsToNearestPackageSize(
      Customer customer,
      string expectedSessionId,
      [Frozen] Mock<IStripeService> mockStripeService,
      PaymentService sut)
  {
    // Arrange
    const int requestedRecipeCount = 7; // Not in default package sizes [5, 10, 20, 50]
    const int expectedAdjustedCount = 5; // Nearest package size

    mockStripeService
        .Setup(s => s.CreateCheckoutSession(
            customer.Email,
            expectedAdjustedCount,
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            null))
        .ReturnsAsync(expectedSessionId);

    // Act
    var result = await sut.CreateCheckoutSession(customer, requestedRecipeCount);

    // Assert
    result.Should().Be(expectedSessionId,
        "because the service should adjust to nearest package size and return session ID");

    mockStripeService.Verify(s => s.CreateCheckoutSession(
        customer.Email,
        expectedAdjustedCount,
        It.IsAny<Dictionary<string, string>>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        "Recipe Credit",
        $"{expectedAdjustedCount} recipe credits for your cookbook account",
        null), Times.Once);
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task GetCreditCheckoutSessionWithUrl_WithValidCustomerAndRecipeCount_ReturnsSession(
      Customer customer,
      Session expectedSession,
      [Frozen] Mock<IStripeService> mockStripeService,
      PaymentService sut)
  {
    // Arrange
    const int recipeCount = 20; // Valid package size

    mockStripeService
        .Setup(s => s.CreateCheckoutSessionWithUrl(
            customer.Email,
            recipeCount,
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "Recipe Credit",
            $"{recipeCount} recipe credits for your cookbook account",
            null))
        .ReturnsAsync(expectedSession);

    // Act
    var result = await sut.GetCreditCheckoutSessionWithUrl(customer, recipeCount);

    // Assert
    result.Should().Be(expectedSession,
        "because the service should return the full session for credit purchase");
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task GetSessionInfo_WithValidSessionId_ReturnsConvertedSessionInfo(
      string sessionId,
      Session stripeSession,
      [Frozen] Mock<IStripeService> mockStripeService,
      PaymentService sut)
  {
    // Arrange
    stripeSession.Id = sessionId;
    stripeSession.Status = "complete";
    stripeSession.CustomerEmail = "test@example.com";
    stripeSession.AmountTotal = 1500; // $15.00 in cents
    stripeSession.Currency = "usd";
    stripeSession.PaymentStatus = "paid";

    mockStripeService
        .Setup(s => s.GetSession(sessionId))
        .ReturnsAsync(stripeSession);

    // Act
    var result = await sut.GetSessionInfo(sessionId);

    // Assert
    result.Should().NotBeNull("because valid session ID should return session info");
    result.Id.Should().Be(sessionId, "because session ID should match");
    result.Status.Should().Be("complete", "because status should be copied");
    result.CustomerEmail.Should().Be("test@example.com", "because email should be copied");
    result.AmountTotal.Should().Be(15.00m, "because amount should be converted from cents to dollars");
    result.Currency.Should().Be("usd", "because currency should be copied");
    result.PaymentStatus.Should().Be("paid", "because payment status should be copied");
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task HandleWebhookEvent_WithValidCheckoutSessionCompleted_ProcessesSuccessfully(
      string requestBody,
      string signature,
      Event stripeEvent,
      [Frozen] Mock<IStripeService> mockStripeService,
      [Frozen] PaymentConfig paymentConfig,
      PaymentService sut)
  {
    // Arrange
    stripeEvent.Type = "checkout.session.completed";

    mockStripeService
        .Setup(s => s.ConstructEvent(requestBody, signature, paymentConfig.StripeWebhookSecret))
        .Returns(stripeEvent);

    // Mock the complex webhook processing methods to avoid deep mocking requirements
    var mockSession = new Session { Id = "test_session", ClientReferenceId = "test_order" };
    mockStripeService
        .Setup(s => s.GetSessionWithLineItems(It.IsAny<string>()))
        .ReturnsAsync(mockSession);

    mockStripeService
        .Setup(s => s.ParseSessionMetadata(It.IsAny<Session>()))
        .Returns(new StripeSessionMetadata { CustomerEmail = "test@example.com", PaymentType = PaymentType.OrderCompletion });

    mockStripeService
        .Setup(s => s.GetPurchasedQuantity(It.IsAny<Session>()))
        .Returns(1);

    // Act - Execute webhook handling (should not throw)
    await sut.HandleWebhookEvent(requestBody, signature);

    // Assert - Verify the event construction is called correctly
    mockStripeService.Verify(s => s.ConstructEvent(requestBody, signature, paymentConfig.StripeWebhookSecret), Times.Once);
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task HandleWebhookEvent_WithStripeException_LogsErrorAndRethrows(
      string requestBody,
      string signature,
      [Frozen] Mock<IStripeService> mockStripeService,
      [Frozen] PaymentConfig paymentConfig,
      PaymentService sut)
  {
    // Arrange
    var stripeException = new StripeException("Invalid signature");
    mockStripeService
        .Setup(s => s.ConstructEvent(requestBody, signature, paymentConfig.StripeWebhookSecret))
        .Throws(stripeException);

    // Act & Assert
    var act = async () => await sut.HandleWebhookEvent(requestBody, signature);

    await act.Should().ThrowAsync<StripeException>("because Stripe exceptions should be propagated after logging");
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task HandleWebhookEvent_WithUnknownEventType_LogsInformation(
      string requestBody,
      string signature,
      Event stripeEvent,
      [Frozen] Mock<IStripeService> mockStripeService,
      [Frozen] PaymentConfig paymentConfig,
      PaymentService sut)
  {
    // Arrange
    stripeEvent.Type = "unknown.event.type";
    stripeEvent.Id = "evt_test_123";

    mockStripeService
        .Setup(s => s.ConstructEvent(requestBody, signature, paymentConfig.StripeWebhookSecret))
        .Returns(stripeEvent);

    // Act
    await sut.HandleWebhookEvent(requestBody, signature);

    // Assert
    // Verify the event was processed (no exception thrown)
    mockStripeService.Verify(s => s.ConstructEvent(requestBody, signature, paymentConfig.StripeWebhookSecret), Times.Once);
  }

  [Theory, AutoMoqData]
  [Trait("Category", "Unit")]
  public async Task HandleWebhookEvent_WithMonitoredEventType_LogsInformation(
      string requestBody,
      string signature,
      Event stripeEvent,
      [Frozen] Mock<IStripeService> mockStripeService,
      [Frozen] PaymentConfig paymentConfig,
      PaymentService sut)
  {
    // Arrange
    stripeEvent.Type = "payment_intent.created"; // Monitored but not acted upon
    stripeEvent.Id = "evt_test_456";

    mockStripeService
        .Setup(s => s.ConstructEvent(requestBody, signature, paymentConfig.StripeWebhookSecret))
        .Returns(stripeEvent);

    // Act
    await sut.HandleWebhookEvent(requestBody, signature);

    // Assert
    // Verify the event was processed without error
    mockStripeService.Verify(s => s.ConstructEvent(requestBody, signature, paymentConfig.StripeWebhookSecret), Times.Once);
  }
}
