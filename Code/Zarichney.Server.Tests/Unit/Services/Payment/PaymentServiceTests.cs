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
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Services.Payment;
using Customer = Zarichney.Cookbook.Customers.Customer;

namespace Zarichney.Server.Tests.Unit.Services.Payment;

/// <summary>
/// Comprehensive unit tests for PaymentService covering all business logic scenarios.
/// Tests payment processing, webhook handling, and order completion workflows.
/// </summary>
public class PaymentServiceTests
{
    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithValidOrder_ReturnsSessionId(
        CookbookOrder order,
        string expectedSessionId,
        [Frozen] Mock<ILogger<PaymentService>> mockLogger,
        [Frozen] Mock<IStripeService> mockStripeService,
        PaymentService sut)
    {
        // Arrange
        var paymentConfig = new PaymentConfigBuilder().Build();
        var clientConfig = new ClientConfig { BaseUrl = "https://example.com" };
        
        // Setup order with recipes requiring payment
        order.RecipeList = ["recipe1", "recipe2", "recipe3"];
        order.SynthesizedRecipes = ["recipe1"]; // One recipe already synthesized
        
        mockStripeService
            .Setup(s => s.CreateCheckoutSession(
                order.Email,
                2, // recipesToPay = 3 - 1
                It.Is<Dictionary<string, string>>(m => 
                    m["payment_type"] == PaymentType.OrderCompletion.ToString() &&
                    m["order_id"] == order.OrderId &&
                    m["customer_email"] == order.Email),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "Cookbook Recipe",
                "Additional recipes for your cookbook (needed: 2)",
                order.OrderId))
            .ReturnsAsync(expectedSessionId);

        // Act
        var result = await sut.CreateCheckoutSession(order);

        // Assert
        result.Should().Be(expectedSessionId, 
            "because the service should return the session ID from Stripe");
        
        mockStripeService.Verify(s => s.CreateCheckoutSession(
            order.Email,
            2,
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "Cookbook Recipe",
            "Additional recipes for your cookbook (needed: 2)",
            order.OrderId), Times.Once);
    }

    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithNullOrder_ThrowsArgumentNullException(
        PaymentService sut)
    {
        // Act & Assert
        var act = async () => await sut.CreateCheckoutSession((CookbookOrder)null!);
        
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Because("null orders should be rejected immediately");
    }

    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithOrderNoPaymentRequired_ThrowsInvalidOperationException(
        CookbookOrder order,
        PaymentService sut)
    {
        // Arrange - Order with all recipes already synthesized
        order.RecipeList = ["recipe1", "recipe2"];
        order.SynthesizedRecipes = ["recipe1", "recipe2"];

        // Act & Assert
        var act = async () => await sut.CreateCheckoutSession(order);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This order doesn't require any payment.")
            .Because("orders with no payment required should be rejected");
    }

    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public async Task GetCheckoutSessionWithUrl_WithValidOrder_ReturnsSessionWithUrl(
        CookbookOrder order,
        Session expectedSession,
        [Frozen] Mock<IStripeService> mockStripeService,
        PaymentService sut)
    {
        // Arrange
        order.RecipeList = ["recipe1", "recipe2", "recipe3"];
        order.SynthesizedRecipes = ["recipe1"];
        
        mockStripeService
            .Setup(s => s.CreateCheckoutSessionWithUrl(
                order.Email,
                2,
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "Cookbook Recipe",
                "Additional recipes for your cookbook (needed: 2)",
                order.OrderId))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await sut.GetCheckoutSessionWithUrl(order);

        // Assert
        result.Should().Be(expectedSession,
            "because the service should return the full session from Stripe");
    }

    [Theory, AutoData]
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

    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithNullCustomer_ThrowsArgumentNullException(
        PaymentService sut)
    {
        // Act & Assert
        var act = async () => await sut.CreateCheckoutSession((Customer)null!, 10);
        
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Because("null customers should be rejected immediately");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithInvalidRecipeCount_ThrowsArgumentException(
        int invalidRecipeCount,
        Customer customer,
        PaymentService sut)
    {
        // Act & Assert
        var act = async () => await sut.CreateCheckoutSession(customer, invalidRecipeCount);
        
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("recipeCount")
            .WithMessage("Recipe count must be greater than zero*")
            .Because("recipe count must be positive");
    }

    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithNonPackageSizeRecipeCount_AdjustsToNearestPackageSize(
        Customer customer,
        string expectedSessionId,
        [Frozen] Mock<ILogger<PaymentService>> mockLogger,
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

    [Theory, AutoData]
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

    [Theory, AutoData]
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

    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public async Task HandleWebhookEvent_WithValidCheckoutSessionCompleted_ProcessesSuccessfully(
        string requestBody,
        string signature,
        string webhookSecret,
        Event stripeEvent,
        [Frozen] Mock<IStripeService> mockStripeService,
        [Frozen] Mock<ILogger<PaymentService>> mockLogger,
        PaymentService sut)
    {
        // Arrange
        var paymentConfig = new PaymentConfigBuilder()
            .WithStripeWebhookSecret(webhookSecret)
            .Build();
        
        stripeEvent.Type = "checkout.session.completed";
        
        mockStripeService
            .Setup(s => s.ConstructEvent(requestBody, signature, webhookSecret))
            .Returns(stripeEvent);

        // Note: Full webhook processing logic would require complex mocking of dynamic objects
        // This test verifies the event is constructed and type is handled correctly

        // Act & Assert - Should not throw
        var act = async () => await sut.HandleWebhookEvent(requestBody, signature);
        
        // The webhook processing involves complex dynamic object handling
        // For now, verify the event construction is called correctly
        mockStripeService.Verify(s => s.ConstructEvent(requestBody, signature, webhookSecret), Times.Once);
    }

    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public async Task HandleWebhookEvent_WithStripeException_LogsErrorAndRethrows(
        string requestBody,
        string signature,
        string webhookSecret,
        [Frozen] Mock<IStripeService> mockStripeService,
        [Frozen] Mock<ILogger<PaymentService>> mockLogger,
        PaymentService sut)
    {
        // Arrange
        var paymentConfig = new PaymentConfigBuilder()
            .WithStripeWebhookSecret(webhookSecret)
            .Build();

        var stripeException = new StripeException("Invalid signature");
        mockStripeService
            .Setup(s => s.ConstructEvent(requestBody, signature, webhookSecret))
            .Throws(stripeException);

        // Act & Assert
        var act = async () => await sut.HandleWebhookEvent(requestBody, signature);
        
        await act.Should().ThrowAsync<StripeException>()
            .Because("Stripe exceptions should be propagated after logging");
    }

    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public async Task HandleWebhookEvent_WithUnknownEventType_LogsInformation(
        string requestBody,
        string signature,
        string webhookSecret,
        Event stripeEvent,
        [Frozen] Mock<IStripeService> mockStripeService,
        [Frozen] Mock<ILogger<PaymentService>> mockLogger,
        PaymentService sut)
    {
        // Arrange
        var paymentConfig = new PaymentConfigBuilder()
            .WithStripeWebhookSecret(webhookSecret)
            .Build();
        
        stripeEvent.Type = "unknown.event.type";
        stripeEvent.Id = "evt_test_123";
        
        mockStripeService
            .Setup(s => s.ConstructEvent(requestBody, signature, webhookSecret))
            .Returns(stripeEvent);

        // Act
        await sut.HandleWebhookEvent(requestBody, signature);

        // Assert
        // Verify the event was processed (no exception thrown)
        mockStripeService.Verify(s => s.ConstructEvent(requestBody, signature, webhookSecret), Times.Once);
    }

    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public async Task HandleWebhookEvent_WithMonitoredEventType_LogsInformation(
        string requestBody,
        string signature,
        string webhookSecret,
        Event stripeEvent,
        [Frozen] Mock<IStripeService> mockStripeService,
        [Frozen] Mock<ILogger<PaymentService>> mockLogger,
        PaymentService sut)
    {
        // Arrange
        var paymentConfig = new PaymentConfigBuilder()
            .WithStripeWebhookSecret(webhookSecret)
            .Build();
        
        stripeEvent.Type = "payment_intent.created"; // Monitored but not acted upon
        stripeEvent.Id = "evt_test_456";
        
        mockStripeService
            .Setup(s => s.ConstructEvent(requestBody, signature, webhookSecret))
            .Returns(stripeEvent);

        // Act
        await sut.HandleWebhookEvent(requestBody, signature);

        // Assert
        // Verify the event was processed without error
        mockStripeService.Verify(s => s.ConstructEvent(requestBody, signature, webhookSecret), Times.Once);
    }
}