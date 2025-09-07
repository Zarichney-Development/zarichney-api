using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Config;
using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Services.Payment;
using Customer = Zarichney.Cookbook.Customers.Customer;

namespace Zarichney.Server.Tests.Unit.Services.Payment;

/// <summary>
/// Basic unit tests for PaymentService focusing on core business logic validation.
/// Tests critical payment processing scenarios and error handling.
/// </summary>
public class PaymentServiceBasicTests
{
    private readonly Mock<ILogger<PaymentService>> _mockLogger;
    private readonly Mock<IStripeService> _mockStripeService;
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly PaymentConfig _paymentConfig;
    private readonly ClientConfig _clientConfig;

    public PaymentServiceBasicTests()
    {
        _mockLogger = new Mock<ILogger<PaymentService>>();
        _mockStripeService = new Mock<IStripeService>();
        _mockOrderService = new Mock<IOrderService>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockCustomerService = new Mock<ICustomerService>();
        _paymentConfig = new PaymentConfigBuilder().Build();
        _clientConfig = new ClientConfig { BaseUrl = "https://example.com" };
    }

    private PaymentService CreateSut() =>
        new(_mockLogger.Object, _paymentConfig, _clientConfig, _mockOrderService.Object,
            _mockOrderRepository.Object, _mockCustomerService.Object, _mockStripeService.Object);

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithNullOrder_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        var act = async () => await sut.CreateCheckoutSession((CookbookOrder)null!);
        
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Because("null orders should be rejected immediately");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithOrderNoPaymentRequired_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = CreateSut();
        var order = new CookbookOrder
        {
            OrderId = "order123",
            Email = "test@example.com",
            RecipeList = ["recipe1", "recipe2"],
            SynthesizedRecipes = ["recipe1", "recipe2"] // All recipes already synthesized
        };

        // Act & Assert
        var act = async () => await sut.CreateCheckoutSession(order);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This order doesn't require any payment.")
            .Because("orders with no payment required should be rejected");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithValidOrder_CallsStripeService()
    {
        // Arrange
        var sut = CreateSut();
        var order = new CookbookOrder
        {
            OrderId = "order123",
            Email = "test@example.com",
            RecipeList = ["recipe1", "recipe2", "recipe3"],
            SynthesizedRecipes = ["recipe1"] // One recipe already synthesized, need to pay for 2
        };
        
        const string expectedSessionId = "cs_test_123";
        _mockStripeService
            .Setup(s => s.CreateCheckoutSession(
                order.Email,
                2, // recipesToPay = 3 - 1
                It.IsAny<Dictionary<string, string>>(),
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
        
        _mockStripeService.Verify(s => s.CreateCheckoutSession(
            order.Email,
            2,
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "Cookbook Recipe",
            "Additional recipes for your cookbook (needed: 2)",
            order.OrderId), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithNullCustomer_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        var act = async () => await sut.CreateCheckoutSession((Customer)null!, 10);
        
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Because("null customers should be rejected immediately");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithZeroRecipeCount_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        var customer = new Customer { Email = "test@example.com" };

        // Act & Assert
        var act = async () => await sut.CreateCheckoutSession(customer, 0);
        
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("recipeCount")
            .WithMessage("Recipe count must be greater than zero*")
            .Because("recipe count must be positive");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithNegativeRecipeCount_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        var customer = new Customer { Email = "test@example.com" };

        // Act & Assert
        var act = async () => await sut.CreateCheckoutSession(customer, -1);
        
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("recipeCount")
            .WithMessage("Recipe count must be greater than zero*")
            .Because("recipe count must be positive");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithValidCustomerAndRecipeCount_ReturnsSessionId()
    {
        // Arrange
        var sut = CreateSut();
        var customer = new Customer { Email = "test@example.com" };
        const int recipeCount = 10;
        const string expectedSessionId = "cs_test_456";
        
        _mockStripeService
            .Setup(s => s.CreateCheckoutSession(
                customer.Email,
                recipeCount,
                It.IsAny<Dictionary<string, string>>(),
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

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateCheckoutSession_WithNonPackageSizeRecipeCount_AdjustsToNearestPackageSize()
    {
        // Arrange
        var sut = CreateSut();
        var customer = new Customer { Email = "test@example.com" };
        const int requestedRecipeCount = 7; // Not in default package sizes [5, 10, 20, 50]
        const int expectedAdjustedCount = 5; // Nearest package size
        const string expectedSessionId = "cs_test_789";

        _mockStripeService
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

        _mockStripeService.Verify(s => s.CreateCheckoutSession(
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
    public async Task GetSessionInfo_WithValidSessionId_ReturnsConvertedSessionInfo(
        string sessionId)
    {
        // Arrange
        var sut = CreateSut();
        var stripeSession = new Stripe.Checkout.Session
        {
            Id = sessionId,
            Status = "complete",
            CustomerEmail = "test@example.com",
            AmountTotal = 1500, // $15.00 in cents
            Currency = "usd",
            PaymentStatus = "paid"
        };

        _mockStripeService
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
    public async Task HandleWebhookEvent_WithInvalidSignature_ThrowsException(
        string requestBody,
        string signature,
        string webhookSecret)
    {
        // Arrange
        var sut = CreateSut();
        
        _mockStripeService
            .Setup(s => s.ConstructEvent(requestBody, signature, webhookSecret))
            .Throws<Exception>();

        // Act & Assert
        var act = async () => await sut.HandleWebhookEvent(requestBody, signature);
        
        await act.Should().ThrowAsync<Exception>()
            .Because("invalid webhook signatures should result in exceptions being thrown");
    }
}