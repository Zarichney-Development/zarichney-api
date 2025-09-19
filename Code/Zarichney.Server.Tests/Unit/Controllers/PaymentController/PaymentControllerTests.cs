using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;
using Zarichney.Controllers;
using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Services.Payment;
using Stripe.Checkout;
using System.IO;
using System.Text;

namespace Zarichney.Server.Tests.Unit.Controllers.PaymentController;

/// <summary>
/// Unit tests for the PaymentController class.
/// Tests payment processing functionality including checkout sessions, credit purchases, and webhook handling.
/// </summary>
[Trait("Category", "Unit")]
public class PaymentControllerTests
{
    private readonly Mock<ILogger<Zarichney.Controllers.PaymentController>> _mockLogger;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<IStripeService> _mockStripeService;
    private readonly Zarichney.Controllers.PaymentController _controller;

    public PaymentControllerTests()
    {
        _mockLogger = new Mock<ILogger<Zarichney.Controllers.PaymentController>>();
        _mockPaymentService = new Mock<IPaymentService>();
        _mockOrderService = new Mock<IOrderService>();
        _mockCustomerService = new Mock<ICustomerService>();
        _mockStripeService = new Mock<IStripeService>();

        _controller = new Zarichney.Controllers.PaymentController(
            _mockLogger.Object,
            _mockPaymentService.Object,
            _mockOrderService.Object,
            _mockCustomerService.Object,
            _mockStripeService.Object);

        // Setup default HttpContext with user identity
        var httpContext = new DefaultHttpContext();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "test@example.com")
        }, "Test"));
        httpContext.User = user;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #region CreateCheckoutSession Tests

    [Fact]
    public async Task CreateCheckoutSession_ValidOrderRequiringPayment_ReturnsCheckoutUrl()
    {
        // Arrange
        const string orderId = "order123";
        var order = new CookbookOrder
        {
            OrderId = orderId,
            RequiresPayment = true,
            Customer = new Customer { Email = "test@example.com", AvailableRecipes = 10 }
        };

        var session = new Session
        {
            Url = "https://checkout.stripe.com/session123"
        };

        _mockOrderService
            .Setup(x => x.GetOrder(orderId))
            .ReturnsAsync(order);

        _mockPaymentService
            .Setup(x => x.GetCheckoutSessionWithUrl(order))
            .ReturnsAsync(session);

        // Act
        var result = await _controller.CreateCheckoutSession(orderId);

        // Assert
        result.Should().BeOfType<OkObjectResult>("because a valid order should return OK");
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<CheckoutUrlResponse>();
        
        var response = okResult.Value as CheckoutUrlResponse;
        response!.CheckoutUrl.Should().Be("https://checkout.stripe.com/session123",
            "because the checkout URL should be returned from the payment service");

        _mockOrderService.Verify(x => x.GetOrder(orderId), Times.Once);
        _mockPaymentService.Verify(x => x.GetCheckoutSessionWithUrl(order), Times.Once);
    }

    [Fact]
    public async Task CreateCheckoutSession_OrderDoesNotRequirePayment_ReturnsBadRequest()
    {
        // Arrange
        const string orderId = "order456";
        var order = new CookbookOrder
        {
            OrderId = orderId,
            RequiresPayment = false,
            Customer = new Customer { Email = "test@example.com" }
        };

        _mockOrderService
            .Setup(x => x.GetOrder(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _controller.CreateCheckoutSession(orderId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("because orders not requiring payment should be rejected");
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Order does not require payment");

        _mockPaymentService.Verify(x => x.GetCheckoutSessionWithUrl(It.IsAny<CookbookOrder>()), Times.Never,
            "because payment service should not be called for orders not requiring payment");
    }

    [Fact]
    public async Task CreateCheckoutSession_OrderNotFound_ReturnsNotFound()
    {
        // Arrange
        const string orderId = "nonexistent";

        _mockOrderService
            .Setup(x => x.GetOrder(orderId))
            .ThrowsAsync(new KeyNotFoundException($"Order {orderId} not found"));

        // Act
        var result = await _controller.CreateCheckoutSession(orderId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>("because non-existent orders should return 404");
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be($"Order {orderId} not found");

        _mockPaymentService.Verify(x => x.GetCheckoutSessionWithUrl(It.IsAny<CookbookOrder>()), Times.Never);
    }

    [Fact]
    public async Task CreateCheckoutSession_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        const string orderId = "order789";
        var order = new CookbookOrder
        {
            OrderId = orderId,
            RequiresPayment = true,
            Customer = new Customer { Email = "test@example.com" }
        };

        _mockOrderService
            .Setup(x => x.GetOrder(orderId))
            .ReturnsAsync(order);

        _mockPaymentService
            .Setup(x => x.GetCheckoutSessionWithUrl(order))
            .ThrowsAsync(new InvalidOperationException("Invalid payment configuration"));

        // Act
        var result = await _controller.CreateCheckoutSession(orderId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("because invalid operations should return bad request");
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid payment configuration");
    }

    [Fact]
    public async Task CreateCheckoutSession_UnexpectedError_ReturnsInternalServerError()
    {
        // Arrange
        const string orderId = "order999";

        _mockOrderService
            .Setup(x => x.GetOrder(orderId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.CreateCheckoutSession(orderId);

        // Assert
        result.Should().BeOfType<ObjectResult>("because unexpected errors should return 500");
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("An error occurred while creating the checkout session");
    }

    #endregion

    #region CreateCreditSession Tests

    [Fact]
    public async Task CreateCreditSession_ValidRequest_ReturnsCheckoutUrl()
    {
        // Arrange
        var request = new RecipeCreditRequest
        {
            Email = "customer@example.com",
            RecipeCount = 10
        };

        var customer = new Customer
        {
            Email = request.Email,
            AvailableRecipes = 5
        };

        var session = new Session
        {
            Url = "https://checkout.stripe.com/credit-session"
        };

        _mockCustomerService
            .Setup(x => x.GetOrCreateCustomer(request.Email))
            .ReturnsAsync(customer);

        _mockPaymentService
            .Setup(x => x.GetCreditCheckoutSessionWithUrl(customer, request.RecipeCount))
            .ReturnsAsync(session);

        // Act
        var result = await _controller.CreateCreditSession(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>("because valid requests should return OK");
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<CheckoutUrlResponse>();
        
        var response = okResult.Value as CheckoutUrlResponse;
        response!.CheckoutUrl.Should().Be("https://checkout.stripe.com/credit-session");

        _mockCustomerService.Verify(x => x.GetOrCreateCustomer(request.Email), Times.Once);
        _mockPaymentService.Verify(x => x.GetCreditCheckoutSessionWithUrl(customer, request.RecipeCount), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateCreditSession_InvalidEmail_ReturnsBadRequest(string? invalidEmail)
    {
        // Arrange
        var request = new RecipeCreditRequest
        {
            Email = invalidEmail!,
            RecipeCount = 10
        };

        // Act
        var result = await _controller.CreateCreditSession(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("because invalid emails should be rejected");
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Email is required");

        _mockCustomerService.Verify(x => x.GetOrCreateCustomer(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateCreditSession_WhitespaceEmail_ProcessesSuccessfully()
    {
        // Arrange
        var request = new RecipeCreditRequest
        {
            Email = "   ", // Whitespace passes IsNullOrEmpty check
            RecipeCount = 10
        };

        var customer = new Customer
        {
            Email = "   ",
            AvailableRecipes = 0
        };

        var session = new Session
        {
            Url = "https://checkout.stripe.com/credit-whitespace"
        };

        _mockCustomerService
            .Setup(x => x.GetOrCreateCustomer("   "))
            .ReturnsAsync(customer);

        _mockPaymentService
            .Setup(x => x.GetCreditCheckoutSessionWithUrl(customer, request.RecipeCount))
            .ReturnsAsync(session);

        // Act
        var result = await _controller.CreateCreditSession(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>("because whitespace strings pass IsNullOrEmpty check");
        
        _mockCustomerService.Verify(x => x.GetOrCreateCustomer("   "), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task CreateCreditSession_InvalidRecipeCount_ReturnsBadRequest(int invalidCount)
    {
        // Arrange
        var request = new RecipeCreditRequest
        {
            Email = "customer@example.com",
            RecipeCount = invalidCount
        };

        // Act
        var result = await _controller.CreateCreditSession(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("because invalid recipe counts should be rejected");
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Recipe count must be greater than zero");

        _mockCustomerService.Verify(x => x.GetOrCreateCustomer(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateCreditSession_CustomerServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var request = new RecipeCreditRequest
        {
            Email = "invalid@domain",
            RecipeCount = 5
        };

        _mockCustomerService
            .Setup(x => x.GetOrCreateCustomer(request.Email))
            .ThrowsAsync(new ArgumentException("Invalid email format"));

        // Act
        var result = await _controller.CreateCreditSession(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("because argument exceptions should return bad request");
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid email format");
    }

    [Fact]
    public async Task CreateCreditSession_UnexpectedError_ReturnsInternalServerError()
    {
        // Arrange
        var request = new RecipeCreditRequest
        {
            Email = "customer@example.com",
            RecipeCount = 10
        };

        _mockCustomerService
            .Setup(x => x.GetOrCreateCustomer(request.Email))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        var result = await _controller.CreateCreditSession(request);

        // Assert
        result.Should().BeOfType<ObjectResult>("because unexpected errors should return 500");
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("An error occurred while creating the checkout session");
    }

    #endregion

    #region HandleWebhook Tests

    [Fact]
    public async Task HandleWebhook_ValidWebhookWithSignature_ReturnsOk()
    {
        // Arrange
        const string webhookPayload = "{\"type\":\"payment_intent.succeeded\"}";
        const string stripeSignature = "t=123,v1=abc123";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(webhookPayload));
        _controller.ControllerContext.HttpContext.Request.Body = stream;
        _controller.ControllerContext.HttpContext.Request.Headers["Stripe-Signature"] = stripeSignature;

        _mockPaymentService
            .Setup(x => x.HandleWebhookEvent(webhookPayload, stripeSignature))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<OkResult>("because valid webhooks should return 200 OK");
        _mockPaymentService.Verify(x => x.HandleWebhookEvent(webhookPayload, stripeSignature), Times.Once);
    }

    [Fact]
    public async Task HandleWebhook_MissingSignatureHeader_ReturnsBadRequest()
    {
        // Arrange
        const string webhookPayload = "{\"type\":\"payment_intent.succeeded\"}";
        
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(webhookPayload));
        _controller.ControllerContext.HttpContext.Request.Body = stream;
        // No Stripe-Signature header added

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<BadRequestResult>("because webhooks without signature should be rejected");
        _mockPaymentService.Verify(x => x.HandleWebhookEvent(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleWebhook_SignatureVerificationFailure_ReturnsOkWithMessage()
    {
        // Arrange
        const string webhookPayload = "{\"type\":\"payment_intent.succeeded\"}";
        const string stripeSignature = "t=123,v1=invalid";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(webhookPayload));
        _controller.ControllerContext.HttpContext.Request.Body = stream;
        _controller.ControllerContext.HttpContext.Request.Headers["Stripe-Signature"] = stripeSignature;

        var stripeException = new Stripe.StripeException("Invalid signature")
        {
            StripeError = new Stripe.StripeError
            {
                Type = "signature_verification_failure"
            }
        };

        _mockPaymentService
            .Setup(x => x.HandleWebhookEvent(webhookPayload, stripeSignature))
            .ThrowsAsync(stripeException);

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<OkObjectResult>("because signature failures should still return 200 to prevent retries");
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be("Signature verification failed, but acknowledging receipt");
    }

    [Fact]
    public async Task HandleWebhook_OtherStripeException_ReturnsBadRequest()
    {
        // Arrange
        const string webhookPayload = "{\"type\":\"payment_intent.succeeded\"}";
        const string stripeSignature = "t=123,v1=abc123";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(webhookPayload));
        _controller.ControllerContext.HttpContext.Request.Body = stream;
        _controller.ControllerContext.HttpContext.Request.Headers["Stripe-Signature"] = stripeSignature;

        var stripeException = new Stripe.StripeException("Rate limit exceeded")
        {
            StripeError = new Stripe.StripeError
            {
                Type = "rate_limit"
            }
        };

        _mockPaymentService
            .Setup(x => x.HandleWebhookEvent(webhookPayload, stripeSignature))
            .ThrowsAsync(stripeException);

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("because other Stripe errors should return bad request");
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Rate limit exceeded" });
    }

    [Fact]
    public async Task HandleWebhook_WithIdempotencyKey_ProcessesSuccessfully()
    {
        // Arrange
        const string webhookPayload = "{\"type\":\"payment_intent.succeeded\"}";
        const string stripeSignature = "t=123,v1=abc123";
        const string idempotencyKey = "idempotent-key-123";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(webhookPayload));
        _controller.ControllerContext.HttpContext.Request.Body = stream;
        _controller.ControllerContext.HttpContext.Request.Headers["Stripe-Signature"] = stripeSignature;
        _controller.ControllerContext.HttpContext.Request.Headers["Idempotency-Key"] = idempotencyKey;

        _mockPaymentService
            .Setup(x => x.HandleWebhookEvent(webhookPayload, stripeSignature))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<OkResult>("because webhooks with idempotency keys should process successfully");
        _mockPaymentService.Verify(x => x.HandleWebhookEvent(webhookPayload, stripeSignature), Times.Once);
    }

    #endregion

    #region GetSessionInfo Tests

    [Fact]
    public async Task GetSessionInfo_ValidSessionId_ReturnsSessionInfo()
    {
        // Arrange
        const string sessionId = "cs_test_123";
        var sessionInfo = new CheckoutSessionInfo
        {
            Id = sessionId,
            Status = "complete",
            PaymentStatus = "paid",
            CustomerEmail = "customer@example.com",
            AmountTotal = 1000
        };

        _mockPaymentService
            .Setup(x => x.GetSessionInfo(sessionId))
            .ReturnsAsync(sessionInfo);

        // Act
        var result = await _controller.GetSessionInfo(sessionId);

        // Assert
        result.Should().BeOfType<OkObjectResult>("because valid session IDs should return OK");
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<CheckoutSessionInfo>();
        
        var response = okResult.Value as CheckoutSessionInfo;
        response!.Id.Should().Be(sessionId);
        response!.Status.Should().Be("complete");
        response.PaymentStatus.Should().Be("paid");

        _mockPaymentService.Verify(x => x.GetSessionInfo(sessionId), Times.Once);
    }

    [Fact]
    public async Task GetSessionInfo_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        const string sessionId = "cs_test_invalid";

        _mockPaymentService
            .Setup(x => x.GetSessionInfo(sessionId))
            .ThrowsAsync(new Exception("Session not found"));

        // Act
        var result = await _controller.GetSessionInfo(sessionId);

        // Assert
        result.Should().BeOfType<ObjectResult>("because errors should return 500");
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("An error occurred while retrieving the session information");
    }

    #endregion

    #region CreatePaymentIntent Tests

    [Fact]
    public async Task CreatePaymentIntent_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new PaymentIntentRequest
        {
            Amount = 1000,
            Currency = "usd",
            Description = "Test payment"
        };

        var paymentIntent = new Stripe.PaymentIntent
        {
            Id = "pi_test_123",
            Amount = 1000,
            Currency = "usd",
            Status = "requires_payment_method"
        };

        _mockStripeService
            .Setup(x => x.CreatePaymentIntentAsync(
                It.IsAny<string>(),
                request.Amount,
                request.Currency,
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntent);

        // Act
        var result = await _controller.CreatePaymentIntent(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>("because valid requests should return OK");
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(paymentIntent);

        _mockStripeService.Verify(x => x.CreatePaymentIntentAsync(
            "test@example.com",
            request.Amount,
            request.Currency,
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, "usd")]
    [InlineData(-100, "usd")]
    [InlineData(1000, "")]
    [InlineData(1000, null)]
    public async Task CreatePaymentIntent_InvalidRequest_ReturnsBadRequest(long amount, string? currency)
    {
        // Arrange
        var request = new PaymentIntentRequest
        {
            Amount = amount,
            Currency = currency ?? string.Empty
        };

        // Act
        var result = await _controller.CreatePaymentIntent(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("because invalid requests should be rejected");
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid payment intent request");

        _mockStripeService.Verify(x => x.CreatePaymentIntentAsync(
            It.IsAny<string>(),
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetPaymentStatus Tests

    [Fact]
    public async Task GetPaymentStatus_ValidPaymentId_ReturnsOk()
    {
        // Arrange
        const string paymentId = "pi_test_123";
        var paymentIntent = new Stripe.PaymentIntent
        {
            Id = paymentId,
            Amount = 1000,
            Currency = "usd",
            Status = "succeeded"
        };

        _mockStripeService
            .Setup(x => x.GetPaymentIntent(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntent);

        // Act
        var result = await _controller.GetPaymentStatus(paymentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>("because valid payment IDs should return OK");
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(paymentIntent);

        _mockStripeService.Verify(x => x.GetPaymentIntent(paymentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetPaymentStatus_InvalidPaymentId_ReturnsBadRequest(string? paymentId)
    {
        // Act
        var result = await _controller.GetPaymentStatus(paymentId!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("because invalid payment IDs should be rejected");
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Payment ID is required");

        _mockStripeService.Verify(x => x.GetPaymentIntent(
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetPaymentStatus_WhitespacePaymentId_ProcessesAsValidInput()
    {
        // Arrange
        const string paymentId = "   ";
        
        // This test documents actual behavior - whitespace strings are not rejected by IsNullOrEmpty
        // This might be a bug in production code but test documents current behavior
        
        _mockStripeService
            .Setup(x => x.GetPaymentIntent(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Stripe.PaymentIntent { Id = "pi_whitespace", Status = "succeeded" });

        // Act
        var result = await _controller.GetPaymentStatus(paymentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>("because whitespace strings pass IsNullOrEmpty check");
        
        _mockStripeService.Verify(x => x.GetPaymentIntent(paymentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Edge Cases and Complex Scenarios

    [Fact]
    public async Task CreateCheckoutSession_ConcurrentRequests_HandledSafely()
    {
        // Arrange
        const string orderId = "order-concurrent";
        var order = new CookbookOrder
        {
            OrderId = orderId,
            RequiresPayment = true,
            Customer = new Customer { Email = "test@example.com" }
        };

        var session = new Session
        {
            Url = "https://checkout.stripe.com/concurrent-session"
        };

        _mockOrderService
            .Setup(x => x.GetOrder(orderId))
            .ReturnsAsync(order);

        _mockPaymentService
            .Setup(x => x.GetCheckoutSessionWithUrl(order))
            .ReturnsAsync(session);

        // Act - simulate concurrent requests
        var tasks = new List<Task<IActionResult>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_controller.CreateCheckoutSession(orderId));
        }
        
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBeOfType<OkObjectResult>("because all concurrent requests should succeed");
        results.Should().HaveCount(5);
        
        // Verify the services were called the expected number of times
        _mockOrderService.Verify(x => x.GetOrder(orderId), Times.Exactly(5));
        _mockPaymentService.Verify(x => x.GetCheckoutSessionWithUrl(order), Times.Exactly(5));
    }

    [Fact]
    public async Task HandleWebhook_LargePayload_ProcessedSuccessfully()
    {
        // Arrange
        var largePayload = "{\"type\":\"payment_intent.succeeded\",\"data\":\"" + 
                          new string('x', 10000) + "\"}";
        const string stripeSignature = "t=123,v1=abc123";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(largePayload));
        _controller.ControllerContext.HttpContext.Request.Body = stream;
        _controller.ControllerContext.HttpContext.Request.Headers["Stripe-Signature"] = stripeSignature;

        _mockPaymentService
            .Setup(x => x.HandleWebhookEvent(largePayload, stripeSignature))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<OkResult>("because large payloads should be handled successfully");
        _mockPaymentService.Verify(x => x.HandleWebhookEvent(largePayload, stripeSignature), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentIntent_WithNoUserIdentity_UsesEmptyEmail()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // No identity

        var request = new PaymentIntentRequest
        {
            Amount = 500,
            Currency = "eur",
            Description = "Anonymous payment"
        };

        var paymentIntent = new Stripe.PaymentIntent
        {
            Id = "pi_anon_123",
            Amount = 500,
            Currency = "eur"
        };

        _mockStripeService
            .Setup(x => x.CreatePaymentIntentAsync(
                string.Empty, // Should use empty string for anonymous users
                request.Amount,
                request.Currency,
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntent);

        // Act
        var result = await _controller.CreatePaymentIntent(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>("because anonymous users should be allowed");
        
        _mockStripeService.Verify(x => x.CreatePaymentIntentAsync(
            string.Empty,
            request.Amount,
            request.Currency,
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}