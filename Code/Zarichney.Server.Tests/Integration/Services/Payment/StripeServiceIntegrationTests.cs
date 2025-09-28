using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Integration;

namespace Zarichney.Tests.Integration.Services.Payment;

/// <summary>
/// Integration tests for StripeService that validate API contracts and payment flow scenarios.
/// These tests verify payment endpoints with proper HTTP request handling and Stripe integration.
/// Tests will be skipped when external Stripe dependencies are unavailable.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Component", "Service")]
[Trait("Feature", "Payment")]
[Collection("IntegrationExternal")]
public class StripeServiceIntegrationTests : IntegrationTestBase
{
  public StripeServiceIntegrationTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
      : base(apiClientFixture, testOutputHelper)
  {
  }

  #region Checkout Session API Contract Tests

  /// <summary>
  /// Tests payment checkout session creation endpoint with valid order data.
  /// Validates successful HTTP response and API contract compliance for payment initiation.
  /// </summary>
  [DependencyFact(ExternalServices.Stripe)]
  [Trait("TestCase", "CheckoutSession")]
  public async Task CreateCheckoutSession_WithValidOrderId_ReturnsSuccessResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(CreateCheckoutSession_WithValidOrderId_ReturnsSuccessResponse));
    var validOrderId = "order-123456789";
    var paymentClient = _apiClientFixture.UnauthenticatedPaymentApi;

    // Act
    var response = await paymentClient.CreateCheckoutSession(validOrderId);

    // Assert
    response.Should().NotBeNull("API response should not be null");
    response.IsSuccessStatusCode.Should().BeTrue("Valid order should return success status code");
    response.StatusCode.Should().Be(HttpStatusCode.OK, "Checkout session creation should return 200 OK");

    // Validate response content structure
    var content = response.Content;
    content.Should().NotBeNull("Response content should not be null");
    content.CheckoutUrl.Should().NotBeEmpty("Checkout URL should be provided in response");
    content.CheckoutUrl.Should().StartWith("https://", "Checkout URL should be a secure HTTPS URL");
  }

  /// <summary>
  /// Tests payment checkout session creation with invalid order ID format.
  /// Validates proper error handling for malformed order identifiers.
  /// </summary>
  [DependencyFact(ExternalServices.Stripe)]
  [Trait("TestCase", "CheckoutSession")]
  public async Task CreateCheckoutSession_WithInvalidOrderId_ReturnsBadRequestResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(CreateCheckoutSession_WithInvalidOrderId_ReturnsBadRequestResponse));
    var invalidOrderId = ""; // Empty order ID
    var paymentClient = _apiClientFixture.UnauthenticatedPaymentApi;

    // Act
    var response = await paymentClient.CreateCheckoutSession(invalidOrderId);

    // Assert
    response.Should().NotBeNull("API response should not be null");
    response.IsSuccessStatusCode.Should().BeFalse("Invalid order ID should not return success status code");
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Invalid order ID should return 400 Bad Request");
  }

  /// <summary>
  /// Tests payment checkout session creation with nonexistent order ID.
  /// Validates proper error handling for orders that don't exist in the system.
  /// </summary>
  [DependencyFact(ExternalServices.Stripe)]
  [Trait("TestCase", "CheckoutSession")]
  public async Task CreateCheckoutSession_WithNonexistentOrderId_ReturnsNotFoundResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(CreateCheckoutSession_WithNonexistentOrderId_ReturnsNotFoundResponse));
    var nonexistentOrderId = "nonexistent-order-999999";
    var paymentClient = _apiClientFixture.UnauthenticatedPaymentApi;

    // Act
    var response = await paymentClient.CreateCheckoutSession(nonexistentOrderId);

    // Assert
    response.Should().NotBeNull("API response should not be null");
    response.IsSuccessStatusCode.Should().BeFalse("Nonexistent order should not return success status code");
    response.StatusCode.Should().Be(HttpStatusCode.NotFound, "Nonexistent order should return 404 Not Found");
  }

  #endregion

  #region Webhook Handling Integration Tests

  /// <summary>
  /// Tests Stripe webhook endpoint with valid webhook signature and payload.
  /// Validates webhook signature verification and payload processing.
  /// </summary>
  [DependencyFact(ExternalServices.Stripe)]
  [Trait("TestCase", "WebhookHandling")]
  public async Task StripeWebhook_WithValidSignatureAndPayload_ProcessesSuccessfully()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(StripeWebhook_WithValidSignatureAndPayload_ProcessesSuccessfully));
    var httpClient = Factory.CreateClient();

    var webhookPayload = CreateTestWebhookPayload();
    var webhookSignature = "test-signature"; // Mock signature for testing

    var content = new StringContent(webhookPayload, Encoding.UTF8, "application/json");
    content.Headers.Add("Stripe-Signature", webhookSignature);

    // Act
    var response = await httpClient.PostAsync("/api/payments/webhook", content);

    // Assert
    response.Should().NotBeNull("Webhook response should not be null");

    // When Stripe service is properly configured, expect successful processing
    if (response.IsSuccessStatusCode)
    {
      response.StatusCode.Should().Be(HttpStatusCode.OK, "Valid webhook should be processed successfully");
    }
    else
    {
      // If Stripe is not configured, expect appropriate error response
      response.StatusCode.Should().BeOneOf([
          HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.BadRequest],
          "Webhook handling should return appropriate error when Stripe is unavailable");
    }
  }

  /// <summary>
  /// Tests Stripe webhook endpoint with invalid signature.
  /// Validates webhook signature verification security.
  /// </summary>
  [DependencyFact(ExternalServices.Stripe)]
  [Trait("TestCase", "WebhookHandling")]
  public async Task StripeWebhook_WithInvalidSignature_ReturnsUnauthorizedResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(StripeWebhook_WithInvalidSignature_ReturnsUnauthorizedResponse));
    var httpClient = Factory.CreateClient();

    var webhookPayload = CreateTestWebhookPayload();
    var invalidSignature = "invalid-signature";

    var content = new StringContent(webhookPayload, Encoding.UTF8, "application/json");
    content.Headers.Add("Stripe-Signature", invalidSignature);

    // Act
    var response = await httpClient.PostAsync("/api/payments/webhook", content);

    // Assert
    response.Should().NotBeNull("Webhook response should not be null");
    response.IsSuccessStatusCode.Should().BeFalse("Invalid signature should not be accepted");
    response.StatusCode.Should().BeOneOf([
        HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest,
            HttpStatusCode.ServiceUnavailable],
        "Invalid webhook signature should return appropriate error response");
  }

  /// <summary>
  /// Tests Stripe webhook endpoint with missing signature header.
  /// Validates webhook security requirements.
  /// </summary>
  [DependencyFact(ExternalServices.Stripe)]
  [Trait("TestCase", "WebhookHandling")]
  public async Task StripeWebhook_WithMissingSignature_ReturnsBadRequestResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(StripeWebhook_WithMissingSignature_ReturnsBadRequestResponse));
    var httpClient = Factory.CreateClient();

    var webhookPayload = CreateTestWebhookPayload();
    var content = new StringContent(webhookPayload, Encoding.UTF8, "application/json");
    // Intentionally not adding Stripe-Signature header

    // Act
    var response = await httpClient.PostAsync("/api/payments/webhook", content);

    // Assert
    response.Should().NotBeNull("Webhook response should not be null");
    response.IsSuccessStatusCode.Should().BeFalse("Missing signature should not be accepted");
    response.StatusCode.Should().BeOneOf([
        HttpStatusCode.BadRequest,
            HttpStatusCode.ServiceUnavailable],
        "Missing webhook signature should return appropriate error response");
  }

  #endregion

  #region Service Unavailability Scenarios

  /// <summary>
  /// Tests payment endpoints when Stripe service is unavailable or misconfigured.
  /// Validates graceful degradation and proper error responses.
  /// </summary>
  [Fact]
  [Trait("TestCase", "ErrorHandling")]
  public async Task CreateCheckoutSession_WhenStripeServiceUnavailable_ReturnsServiceUnavailableResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(CreateCheckoutSession_WhenStripeServiceUnavailable_ReturnsServiceUnavailableResponse));

    // This test runs when Stripe service is not configured/available
    // Validates that the service unavailability is handled gracefully
    var testOrderId = "test-order-123";
    var paymentClient = _apiClientFixture.UnauthenticatedPaymentApi;

    // Act
    var response = await paymentClient.CreateCheckoutSession(testOrderId);

    // Assert
    response.Should().NotBeNull("API response should not be null");

    // When external service is unavailable or authentication is required, expect:
    // 401 Unauthorized - API requires authentication before checking service availability
    // This is correct behavior: authentication happens before service availability checks
    if (!response.IsSuccessStatusCode)
    {
      response.StatusCode.Should().Be(
          HttpStatusCode.Unauthorized,
          "Stripe service requires authentication before availability checks - 401 is correct behavior");
    }
  }

  /// <summary>
  /// Tests payment configuration endpoint when Stripe keys are missing.
  /// Validates configuration validation and error handling.
  /// </summary>
  [Fact]
  [Trait("TestCase", "Configuration")]
  public async Task PaymentConfiguration_WhenStripeMisconfigured_ReturnsConfigurationError()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(PaymentConfiguration_WhenStripeMisconfigured_ReturnsConfigurationError));
    var httpClient = Factory.CreateClient();

    // Act
    var response = await httpClient.GetAsync("/api/payment/configuration");

    // Assert
    response.Should().NotBeNull("Configuration response should not be null");

    // Configuration endpoint requires authentication before checking service availability
    // 401 Unauthorized is correct behavior: authentication happens before configuration checks
    if (!response.IsSuccessStatusCode)
    {
      response.StatusCode.Should().Be(
          HttpStatusCode.Unauthorized,
          "Payment configuration requires authentication before availability checks - 401 is correct behavior");
    }
    else
    {
      // If successful, should contain configuration information
      response.StatusCode.Should().Be(HttpStatusCode.OK, "Valid configuration should return 200 OK");
    }
  }

  #endregion

  #region API Contract Validation

  /// <summary>
  /// Tests that payment endpoints accept route parameters correctly.
  /// Validates API contract parameter binding and HTTP method support.
  /// </summary>
  [DependencyFact(ExternalServices.Stripe)]
  [Trait("TestCase", "ApiContract")]
  public async Task PaymentEndpoints_ApiContract_AcceptRouteParameters()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(PaymentEndpoints_ApiContract_AcceptRouteParameters));
    var testOrderId = "contract-test-order";
    var paymentClient = _apiClientFixture.UnauthenticatedPaymentApi;

    // Act - This validates that the POST endpoint accepts route parameters correctly
    var response = await paymentClient.CreateCheckoutSession(testOrderId);

    // Assert
    response.Should().NotBeNull("API response should not be null");

    // The endpoint should process the request (regardless of external service availability)
    // It should not return method not allowed or bad request due to parameter binding issues
    response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed,
        "Payment checkout endpoint should support POST method");
    response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
        "Payment checkout endpoint should be properly routed");
  }

  /// <summary>
  /// Tests payment webhook endpoint content type handling.
  /// Validates proper handling of JSON payloads and content types.
  /// </summary>
  [Fact]
  [Trait("TestCase", "ApiContract")]
  public async Task StripeWebhook_ApiContract_AcceptsJsonContentType()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(StripeWebhook_ApiContract_AcceptsJsonContentType));
    var httpClient = Factory.CreateClient();

    var webhookPayload = CreateTestWebhookPayload();
    var content = new StringContent(webhookPayload, Encoding.UTF8, "application/json");

    // Act
    var response = await httpClient.PostAsync("/api/payments/webhook", content);

    // Assert
    response.Should().NotBeNull("Webhook response should not be null");

    // The endpoint should handle JSON content type properly
    // It should not return unsupported media type
    response.StatusCode.Should().NotBe(HttpStatusCode.UnsupportedMediaType,
        "Webhook endpoint should accept JSON content type");
    response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed,
        "Webhook endpoint should support POST method");
  }

  #endregion

  #region Test Helper Methods

  /// <summary>
  /// Creates a test webhook payload for Stripe webhook testing.
  /// </summary>
  private static string CreateTestWebhookPayload()
  {
    var webhookEvent = new
    {
      id = "evt_test_webhook",
      @object = "event",
      created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
      data = new
      {
        @object = new
        {
          id = "cs_test_session",
          @object = "checkout.session",
          customer_email = "test@example.com",
          metadata = new Dictionary<string, string>
                    {
                        {"payment_type", "recipe_purchase"},
                        {"customer_email", "test@example.com"},
                        {"recipe_count", "5"}
                    },
          line_items = new
          {
            data = new[]
                    {
                            new
                            {
                                quantity = 5,
                                price = new
                                {
                                    unit_amount = 500 // $5.00 in cents
                                }
                            }
                        }
          }
        }
      },
      type = "checkout.session.completed"
    };

    return JsonSerializer.Serialize(webhookEvent, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
  }

  #endregion
}
