using Moq;

namespace Zarichney.Tests.Framework.Mocks.Factories;

/// <summary>
/// Factory for creating mock Stripe payment service.
/// </summary>
public class MockStripeServiceFactory : BaseMockFactory<IStripeService>
{
    /// <summary>
    /// Creates a mock Stripe service with default implementations.
    /// </summary>
    /// <returns>A Mock of the IStripeService.</returns>
    public new static Mock<IStripeService> CreateMock()
    {
        var factory = new MockStripeServiceFactory();
        return factory.CreateDefaultMock();
    }

    /// <summary>
    /// Sets up default behaviors for the mock Stripe service.
    /// </summary>
    /// <param name="mock">The mock to set up.</param>
    protected override void SetupDefaultMock(Mock<IStripeService> mock)
    {
        // Set up default success responses for payment operations
        mock.Setup(s => s.CreatePaymentIntentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new PaymentIntent
            {
                Id = "pi_" + Guid.NewGuid().ToString("N"),
                ClientSecret = "pi_secret_" + Guid.NewGuid().ToString("N"),
                Amount = 1000, // $10.00
                Status = "requires_payment_method"
            });

        mock.Setup(s => s.GetPaymentIntentAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => new PaymentIntent
            {
                Id = id,
                ClientSecret = "pi_secret_" + Guid.NewGuid().ToString("N"),
                Amount = 1000,
                Status = "succeeded"
            });

        mock.Setup(s => s.CreateSubscriptionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Subscription
            {
                Id = "sub_" + Guid.NewGuid().ToString("N"),
                CustomerId = "cus_" + Guid.NewGuid().ToString("N"),
                Status = "active",
                CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
            });
    }
}

/// <summary>
/// Interface for Stripe payment service.
/// This would typically be defined in the actual service project.
/// </summary>
public interface IStripeService
{
    Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string customerId);
    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);
    Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId);
    Task<Subscription> CancelSubscriptionAsync(string subscriptionId);
    Task<Customer> CreateCustomerAsync(string email, string name);
}

/// <summary>
/// Represents a Stripe payment intent.
/// </summary>
public class PaymentIntent
{
    public string Id { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Stripe subscription.
/// </summary>
public class Subscription
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CurrentPeriodEnd { get; set; }
}

/// <summary>
/// Represents a Stripe customer.
/// </summary>
public class Customer
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
