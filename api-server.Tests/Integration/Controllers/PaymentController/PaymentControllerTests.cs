using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Zarichney.Services.Payment;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Framework.Helpers;

namespace Zarichney.Tests.Integration.Controllers.PaymentController;

/// <summary>
/// Integration tests for the PaymentController.
/// Demonstrates database testing and external service mocking.
/// </summary>
[Collection("Database")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
[Trait(TestCategories.Dependency, TestCategories.ExternalStripe)]
public class PaymentControllerTests(CustomWebApplicationFactory factory, DatabaseFixture dbFixture) 
    : DatabaseIntegrationTestBase(factory, dbFixture)
{
    [DependencyFact]
    public async Task CreatePaymentIntent_ValidOrder_ReturnsPaymentIntent()
    {
        // Arrange
        await DatabaseFixture.ResetDatabaseAsync();
        
        var userId = "user-" + GetRandom.String();
        var roles = new[] { "User" };
        var authenticatedClient = Factory.CreateAuthenticatedClient(userId, roles);
        
        var orderRequest = new
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
        
        // Act
        var response = await authenticatedClient.PostAsJsonAsync("/api/payments/create-intent", orderRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Because valid order should create a payment intent");
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("clientSecret", "Because response should contain the client secret for the payment intent");
        
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
        await DatabaseFixture.ResetDatabaseAsync();
        
        var userId = "user-" + GetRandom.String();
        var roles = new[] { "User" };
        var authenticatedClient = Factory.CreateAuthenticatedClient(userId, roles);
        
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
        
        // Act
        var response = await authenticatedClient.GetAsync($"/api/payments/status/{paymentId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Because valid payment ID should return payment status");
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("succeeded", "Because response should contain the payment status");
        
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
