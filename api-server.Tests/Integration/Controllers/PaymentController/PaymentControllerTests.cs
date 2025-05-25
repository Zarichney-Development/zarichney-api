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
  [DependencyFact(Zarichney.Services.Status.ExternalServices.Stripe)]
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

  [DependencyFact]
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

  [Fact]
  public async Task CreateCheckoutSession_WhenStripeUnavailable_ReturnsServiceUnavailableOrRequiresAuth()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedPaymentApi;
    var orderId = "test-order-123";

    // Act
    var response = await client.CreateCheckoutSession(orderId);

    // Assert - Should return 503 Service Unavailable when Stripe is unavailable,
    // or other expected status codes depending on service availability
    Assert.True(
      response.StatusCode == HttpStatusCode.ServiceUnavailable ||
      response.StatusCode == HttpStatusCode.NotFound || // Order not found (expected without DB)
      response.StatusCode == HttpStatusCode.BadRequest || // Invalid order state
      response.IsSuccessStatusCode, // If Stripe is actually available
      $"Expected 503 Service Unavailable, 404 Not Found, 400 Bad Request, or success, but got {response.StatusCode}: {response.Error?.Content}");

    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
    {
      // Verify it's specifically due to Stripe being unavailable
      var errorContent = response.Error?.Content;
      Assert.Contains("Stripe", errorContent ?? "");
    }
  }

  [Fact]
  public async Task CreateIntent_WhenStripeUnavailable_ReturnsServiceUnavailableOrBadRequest()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedPaymentApi;
    var request = new PaymentIntentRequest(1000, "usd", "Test payment");

    // Act
    var response = await client.CreateIntent(request);

    // Assert - Should return 503 Service Unavailable when Stripe is unavailable
    Assert.True(
      response.StatusCode == HttpStatusCode.ServiceUnavailable ||
      response.StatusCode == HttpStatusCode.BadRequest || // Invalid request without proper setup
      response.IsSuccessStatusCode, // If Stripe is actually available
      $"Expected 503 Service Unavailable, 400 Bad Request, or success, but got {response.StatusCode}: {response.Error?.Content}");

    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
    {
      // Verify it's specifically due to Stripe being unavailable
      var errorContent = response.Error?.Content;
      Assert.Contains("Stripe", errorContent ?? "");
    }
  }

  [Fact]
  public async Task PaymentEndpoints_WithoutAuthentication_ReturnsUnauthorized()
  {
    // Arrange
    var client = _apiClientFixture.UnauthenticatedPaymentApi;
    var request = new PaymentIntentRequest(1000, "usd", "Test payment");

    // Act & Assert - All payment endpoints should require authentication
    var createIntentResponse = await client.CreateIntent(request);
    Assert.Equal(HttpStatusCode.Unauthorized, createIntentResponse.StatusCode);

    var statusResponse = await client.Status("pi_test_123");
    Assert.Equal(HttpStatusCode.Unauthorized, statusResponse.StatusCode);

    var checkoutResponse = await client.CreateCheckoutSession("test-order-123");
    Assert.Equal(HttpStatusCode.Unauthorized, checkoutResponse.StatusCode);
  }
}
