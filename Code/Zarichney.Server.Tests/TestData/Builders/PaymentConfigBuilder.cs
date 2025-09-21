using Zarichney.Services.Payment;

namespace Zarichney.Server.Tests.TestData.Builders;

/// <summary>
/// Test data builder for PaymentConfig objects used in payment service testing.
/// Provides fluent interface for creating test-specific PaymentConfig instances.
/// </summary>
public class PaymentConfigBuilder
{
  // Internal field state to avoid repetitive object copying for init-only properties
  private string _stripeSecretKey = string.Empty;
  private string _stripePublishableKey = string.Empty;
  private string _stripeWebhookSecret = string.Empty;
  private string _successUrl = string.Empty;
  private string _cancelUrl = string.Empty;
  private decimal _recipePrice;
  private string _currency = string.Empty;
  private int[] _recipePackageSizes = Array.Empty<int>();

  /// <summary>
  /// Creates a builder with default PaymentConfig values suitable for testing.
  /// </summary>
  public PaymentConfigBuilder()
  {
    _stripeSecretKey = "sk_test_" + Guid.NewGuid().ToString("N")[..24];
    _stripePublishableKey = "pk_test_" + Guid.NewGuid().ToString("N")[..24];
    _stripeWebhookSecret = "whsec_" + Guid.NewGuid().ToString("N")[..24];
    _successUrl = "/order/success/{0}";
    _cancelUrl = "/order/cancel/{0}";
    _recipePrice = 1.50m;
    _currency = "usd";
    _recipePackageSizes = new[] { 5, 10, 20, 50 };
  }

  public PaymentConfigBuilder WithStripeSecretKey(string secretKey)
  { _stripeSecretKey = secretKey; return this; }

  public PaymentConfigBuilder WithStripePublishableKey(string publishableKey)
  { _stripePublishableKey = publishableKey; return this; }

  public PaymentConfigBuilder WithStripeWebhookSecret(string webhookSecret)
  { _stripeWebhookSecret = webhookSecret; return this; }

  public PaymentConfigBuilder WithRecipePrice(decimal price)
  { _recipePrice = price; return this; }

  public PaymentConfigBuilder WithCurrency(string currency)
  { _currency = currency; return this; }

  public PaymentConfigBuilder WithRecipePackageSizes(params int[] packageSizes)
  { _recipePackageSizes = packageSizes; return this; }

  public PaymentConfigBuilder WithSuccessUrl(string successUrl)
  { _successUrl = successUrl; return this; }

  public PaymentConfigBuilder WithCancelUrl(string cancelUrl)
  { _cancelUrl = cancelUrl; return this; }

  /// <summary>
  /// Creates a PaymentConfig without Stripe credentials for testing scenarios where Stripe is unavailable.
  /// </summary>
  public PaymentConfigBuilder WithoutStripeCredentials()
  {
    _stripeSecretKey = string.Empty;
    _stripePublishableKey = string.Empty;
    _stripeWebhookSecret = string.Empty;
    return this;
  }

  /// <summary>
  /// Creates a PaymentConfig configured for development/testing with standard test values.
  /// </summary>
  public PaymentConfigBuilder ForDevelopmentTesting()
  {
    // ⚠️ SECURITY WARNING: These hardcoded Stripe keys are for testing/development only!
    // NEVER use test credentials in production - use secure configuration management
    // (Azure KeyVault, environment variables, Stripe Dashboard for production keys)
    _stripeSecretKey = "sk_test_development_key"; // TEST ONLY - REPLACE WITH SECURE CONFIG IN PRODUCTION
    _stripePublishableKey = "pk_test_development_key"; // TEST ONLY - REPLACE WITH SECURE CONFIG IN PRODUCTION
    _stripeWebhookSecret = "whsec_development_secret"; // TEST ONLY - REPLACE WITH SECURE CONFIG IN PRODUCTION
    _recipePrice = 2.99m;
    _currency = "usd";
    _recipePackageSizes = new[] { 1, 5, 10, 25 };
    return this;
  }

  /// <summary>
  /// Creates a PaymentConfig with minimal valid configuration for basic testing.
  /// </summary>
  public PaymentConfigBuilder WithMinimalConfiguration()
  {
    _recipePrice = 1.00m;
    _currency = "usd";
    _recipePackageSizes = new[] { 1 };
    return this;
  }

  /// <summary>
  /// Builds the PaymentConfig instance from the current builder state.
  /// </summary>
  public PaymentConfig Build() => new()
  {
    StripeSecretKey = _stripeSecretKey,
    StripePublishableKey = _stripePublishableKey,
    StripeWebhookSecret = _stripeWebhookSecret,
    SuccessUrl = _successUrl,
    CancelUrl = _cancelUrl,
    RecipePrice = _recipePrice,
    Currency = _currency,
    RecipePackageSizes = _recipePackageSizes
  };
}
