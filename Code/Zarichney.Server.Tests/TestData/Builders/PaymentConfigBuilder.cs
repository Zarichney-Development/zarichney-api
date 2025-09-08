using Zarichney.Services.Payment;

namespace Zarichney.Server.Tests.TestData.Builders;

/// <summary>
/// Test data builder for PaymentConfig objects used in payment service testing.
/// Provides fluent interface for creating test-specific PaymentConfig instances.
/// </summary>
public class PaymentConfigBuilder
{
    private PaymentConfig _entity;

    /// <summary>
    /// Creates a builder with default PaymentConfig values suitable for testing.
    /// </summary>
    public PaymentConfigBuilder()
    {
        _entity = new PaymentConfig
        {
            StripeSecretKey = "sk_test_" + Guid.NewGuid().ToString("N")[..24],
            StripePublishableKey = "pk_test_" + Guid.NewGuid().ToString("N")[..24],
            StripeWebhookSecret = "whsec_" + Guid.NewGuid().ToString("N")[..24],
            SuccessUrl = "/order/success/{0}",
            CancelUrl = "/order/cancel/{0}",
            RecipePrice = 1.50m,
            Currency = "usd",
            RecipePackageSizes = [5, 10, 20, 50]
        };
    }

    /// <summary>
    /// Sets the Stripe secret key for testing.
    /// </summary>
    public PaymentConfigBuilder WithStripeSecretKey(string secretKey)
    {
        _entity = _entity with { StripeSecretKey = secretKey };
        return this;
    }

    /// <summary>
    /// Sets the Stripe publishable key for testing.
    /// </summary>
    public PaymentConfigBuilder WithStripePublishableKey(string publishableKey)
    {
        _entity = _entity with { StripePublishableKey = publishableKey };
        return this;
    }

    /// <summary>
    /// Sets the Stripe webhook secret for testing.
    /// </summary>
    public PaymentConfigBuilder WithStripeWebhookSecret(string webhookSecret)
    {
        _entity = _entity with { StripeWebhookSecret = webhookSecret };
        return this;
    }

    /// <summary>
    /// Sets the recipe price for testing.
    /// </summary>
    public PaymentConfigBuilder WithRecipePrice(decimal price)
    {
        _entity = _entity with { RecipePrice = price };
        return this;
    }

    /// <summary>
    /// Sets the currency for testing.
    /// </summary>
    public PaymentConfigBuilder WithCurrency(string currency)
    {
        _entity = _entity with { Currency = currency };
        return this;
    }

    /// <summary>
    /// Sets custom recipe package sizes for testing.
    /// </summary>
    public PaymentConfigBuilder WithRecipePackageSizes(params int[] packageSizes)
    {
        _entity = _entity with { RecipePackageSizes = packageSizes };
        return this;
    }

    /// <summary>
    /// Sets the success URL pattern for testing.
    /// </summary>
    public PaymentConfigBuilder WithSuccessUrl(string successUrl)
    {
        _entity = _entity with { SuccessUrl = successUrl };
        return this;
    }

    /// <summary>
    /// Sets the cancel URL pattern for testing.
    /// </summary>
    public PaymentConfigBuilder WithCancelUrl(string cancelUrl)
    {
        _entity = _entity with { CancelUrl = cancelUrl };
        return this;
    }

    /// <summary>
    /// Creates a PaymentConfig without Stripe credentials for testing scenarios where Stripe is unavailable.
    /// </summary>
    public PaymentConfigBuilder WithoutStripeCredentials()
    {
        _entity = _entity with 
        { 
            StripeSecretKey = string.Empty,
            StripePublishableKey = string.Empty,
            StripeWebhookSecret = string.Empty
        };
        return this;
    }

    /// <summary>
    /// Builds the PaymentConfig instance.
    /// </summary>
    public PaymentConfig Build() => _entity;
}