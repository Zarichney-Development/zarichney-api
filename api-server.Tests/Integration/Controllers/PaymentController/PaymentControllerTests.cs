using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using Xunit;
using Zarichney.Services.Payment;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Framework.Helpers;
using Refit;
using Xunit.Abstractions;
using Zarichney.Client.Contracts;
using ExternalServices = Zarichney.Services.Status.ExternalServices;

namespace Zarichney.Tests.Integration.Controllers.PaymentController;

/// <summary>
/// Integration tests for the PaymentController.
/// Demonstrates database testing and external service mocking.
/// </summary>
[Collection("Integration")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
// Note: we're keeping the old trait for backward compatibility and demonstration, but we'll
// primarily rely on the ExternalServices-based dependency checking via the DependencyFact attribute
[Trait(TestCategories.Dependency, TestCategories.ExternalStripe)]
public class PaymentControllerTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
  : DatabaseIntegrationTestBase(apiClientFixture, testOutputHelper)
{
  // Using the new ExternalServices-based dependency checking approach
  [DependencyFact(ExternalServices.Stripe)]
  public async Task CreatePaymentIntent_ValidOrder_ReturnsPaymentIntent()
  {
    // Arrange
    await ResetDatabaseAsync();
    var apiClient = _apiClientFixture.AuthenticatedPaymentApi;
    var requestDto = new PaymentIntentRequest(1000, "usd", "Test order");

    // Mock the Stripe service
    var mockStripeService = GetMockStripeService();
    mockStripeService.Setup(s => s.CreatePaymentIntentAsync(
        It.IsAny<string>(),
        It.Is<long>(a => a == 1000),
        It.Is<string>(c => c == "usd"),
        It.IsAny<Dictionary<string, string>>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(new Stripe.PaymentIntent
      {
        Id = "pi_test_" + GetRandom.String(),
        ClientSecret = "pi_test_secret_" + GetRandom.String(),
        Status = "requires_payment_method",
        Amount = 1000,
        Currency = "usd"
      });

    // Act & Assert: call CreateIntent and ensure no exception is thrown (HTTP 2xx)
    var act = () => apiClient.CreateIntent(requestDto);
    await act.Should().NotThrowAsync<ApiException>(
      because: "valid order should create a payment intent");

    // Verify the mock was called
    mockStripeService.Verify(s => s.CreatePaymentIntentAsync(
      It.IsAny<string>(),
      It.Is<long>(a => a == 1000),
      It.Is<string>(c => c == "usd"),
      It.IsAny<Dictionary<string, string>>(),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [DependencyFact(ExternalServices.Stripe)]
  public async Task GetPaymentStatus_ValidPaymentId_ReturnsStatus()
  {
    // Arrange
    await ResetDatabaseAsync();
    var apiClient = _apiClientFixture.AuthenticatedPaymentApi;
    var paymentId = "pi_test_" + GetRandom.String();

    // Mock the Stripe service
    var mockStripeService = GetMockStripeService();
    mockStripeService.Setup(s => s.GetPaymentIntent(
        It.Is<string>(id => id == paymentId),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(new Stripe.PaymentIntent
      {
        Id = paymentId,
        Status = "succeeded",
        Amount = 1000,
        Currency = "usd"
      });

    // Act & Assert: call Status and ensure no exception is thrown (HTTP 2xx)
    var actStatus = () => apiClient.Status(paymentId);
    await actStatus.Should().NotThrowAsync<ApiException>(
      because: "valid payment ID should return payment status");

    // Verify the mock was called
    mockStripeService.Verify(s => s.GetPaymentIntent(
      It.Is<string>(id => id == paymentId),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  private Mock<IStripeService> GetMockStripeService()
  {
    // Get the mock service from the factory
    using var scope = Factory.Services.CreateScope();
    var mockService = scope.ServiceProvider.GetRequiredService<Mock<IStripeService>>();
    return mockService;
  }
}

/// <summary>
/// Simple integration tests for Payment Controller service availability behavior.
/// These tests verify 503 responses when Stripe is unavailable, without requiring database setup.
/// </summary>
[Collection("Integration")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Component, TestCategories.Controller)]
[Trait(TestCategories.Feature, TestCategories.Payment)]
public class PaymentControllerServiceAvailabilityTests : IntegrationTestBase
{
  public PaymentControllerServiceAvailabilityTests(ApiClientFixture apiClientFixture, ITestOutputHelper output)
    : base(apiClientFixture, output)
  {
  }

  [DependencyFact(ExternalServices.Stripe)]
  public async Task CreateCheckoutSession_Authenticated_ValidOrder_ReturnsExpectedResponse()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedPaymentApi;
    var orderId = "test-order-123";

    // Act
    var response = await client.CreateCheckoutSession(orderId);

    // Assert - Should return expected business logic response when Stripe is available
    // Could be 404 (order not found), 400 (invalid order state), or success
    (response.StatusCode == HttpStatusCode.NotFound ||
     response.StatusCode == HttpStatusCode.BadRequest ||
     response.IsSuccessStatusCode).Should().BeTrue(
        because: "when Stripe is available, should return business logic response (404/400/success)");
  }

  [ServiceUnavailableFact(ExternalServices.Stripe)]
  public async Task CreateCheckoutSession_Authenticated_ReturnsServiceUnavailable_WhenStripeUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedPaymentApi;
    var orderId = "test-order-123";

    // Act
    var response = await client.CreateCheckoutSession(orderId);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var errorContent = response.Error?.Content;
    errorContent.Should().NotBeNullOrEmpty();
    errorContent.Should().Contain(ExternalServices.Stripe.ToString(),
        because: "the error message should indicate that Stripe is the unavailable service");
  }

  [DependencyFact(ExternalServices.Stripe)]
  public async Task CreateIntent_Authenticated_ValidRequest_ReturnsSuccess()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedPaymentApi;
    var request = new PaymentIntentRequest(1000, "usd", "Test payment");

    // Act
    var response = await client.CreateIntent(request);

    // Assert - Should succeed when Stripe is available and request is valid
    response.IsSuccessStatusCode.Should().BeTrue(
        because: "valid payment intent request should succeed when Stripe is available");
  }

  [DependencyFact(ExternalServices.Stripe)]
  public async Task CreateIntent_Authenticated_InvalidRequest_ReturnsBadRequest()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedPaymentApi;
    var request = new PaymentIntentRequest(0, "", ""); // Invalid amount and currency

    // Act
    var response = await client.CreateIntent(request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
        because: "invalid payment intent request should return bad request when Stripe is available");
  }

  [ServiceUnavailableFact(ExternalServices.Stripe)]
  public async Task CreateIntent_Authenticated_ReturnsServiceUnavailable_WhenStripeUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedPaymentApi;
    var request = new PaymentIntentRequest(1000, "usd", "Test payment");

    // Act
    var response = await client.CreateIntent(request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var errorContent = response.Error?.Content;
    errorContent.Should().NotBeNullOrEmpty();
    errorContent.Should().Contain(ExternalServices.Stripe.ToString(),
        because: "the error message should indicate that Stripe is the unavailable service");
  }

  [Fact]
  public async Task CreateIntent_Unauthenticated_ReturnsUnauthorized()
  {
    // Arrange
    var client = _apiClientFixture.UnauthenticatedPaymentApi;
    var request = new PaymentIntentRequest(1000, "usd", "Test payment");

    // Act
    var response = await client.CreateIntent(request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
        because: "payment endpoints should require authentication");
  }

  [Fact]
  public async Task GetPaymentStatus_Unauthenticated_ReturnsUnauthorized()
  {
    // Arrange
    var client = _apiClientFixture.UnauthenticatedPaymentApi;

    // Act
    var response = await client.Status("pi_test_123");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
        because: "payment endpoints should require authentication");
  }

  [Fact]
  public async Task CreateCheckoutSession_Unauthenticated_ReturnsUnauthorized()
  {
    // Arrange
    var client = _apiClientFixture.UnauthenticatedPaymentApi;

    // Act
    var response = await client.CreateCheckoutSession("test-order-123");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
        because: "payment endpoints should require authentication");
  }

  [ServiceUnavailableFact(ExternalServices.Stripe)]
  public async Task GetPaymentStatus_Authenticated_ReturnsServiceUnavailable_WhenStripeUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedPaymentApi;

    // Act
    var response = await client.Status("pi_test_123");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var errorContent = response.Error?.Content;
    errorContent.Should().NotBeNullOrEmpty();
    errorContent.Should().Contain(ExternalServices.Stripe.ToString(),
        because: "the error message should indicate that Stripe is the unavailable service");
  }
}
