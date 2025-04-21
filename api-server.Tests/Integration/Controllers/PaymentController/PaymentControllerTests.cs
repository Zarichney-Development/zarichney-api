using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Zarichney.Services.Payment;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Framework.Helpers;
using Refit;
using Zarichney.Client;

namespace Zarichney.Tests.Integration.Controllers.PaymentController;

/// <summary>
/// Integration tests for the PaymentController.
/// Demonstrates database testing and external service mocking.
/// </summary>
[Collection("Integration")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
[Trait(TestCategories.Dependency, TestCategories.ExternalStripe)]
public class PaymentControllerTests(ApiClientFixture apiClientFixture)
  : DatabaseIntegrationTestBase(apiClientFixture)
{
  [DependencyFact]
  public async Task CreatePaymentIntent_ValidOrder_ReturnsPaymentIntent()
  {
    // Arrange
    await ResetDatabaseAsync();
    var userId = "user-" + GetRandom.String();
    var roles = new[] { "User" };
    var apiClient = AuthenticatedApiClient;
    var requestDto = new PaymentIntentRequest
    {
      Amount = 1000,
      Currency = "usd",
      Description = "Test order"
    };

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
    var userId = "user-" + GetRandom.String();
    var roles = new[] { "User" };
    var apiClient = AuthenticatedApiClient;
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
    Func<Task> actStatus = () => apiClient.Status(paymentId);
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