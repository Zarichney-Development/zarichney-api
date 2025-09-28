using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Services.Payment;

namespace Zarichney.Tests.Unit.Services.Payment;

/// <summary>
/// Unit tests for payment-related model classes and enumerations.
/// Tests data integrity, validation, and proper value assignments.
/// </summary>
public class PaymentModelsTests
{
  [Theory]
  [InlineData(PaymentType.OrderCompletion, "OrderCompletion")]
  [InlineData(PaymentType.RecipeCredit, "RecipeCredit")]
  [Trait("Category", "Unit")]
  public void PaymentType_EnumValues_HaveCorrectStringRepresentation(PaymentType paymentType, string expectedString)
  {
    // Act
    var result = paymentType.ToString();

    // Assert
    result.Should().Be(expectedString, "because enum values should have consistent string representation");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void PaymentConfig_DefaultValues_AreSetCorrectly()
  {
    // Act
    var config = new PaymentConfig();

    // Assert
    config.StripeSecretKey.Should().BeEmpty("because default secret key should be empty");
    config.StripePublishableKey.Should().BeEmpty("because default publishable key should be empty");
    config.StripeWebhookSecret.Should().BeEmpty("because default webhook secret should be empty");
    config.SuccessUrl.Should().Be("/order/success/{0}", "because default success URL should match expected pattern");
    config.CancelUrl.Should().Be("/order/cancel/{0}", "because default cancel URL should match expected pattern");
    config.RecipePrice.Should().Be(1.00m, "because default recipe price should be $1.00");
    config.Currency.Should().Be("usd", "because default currency should be USD");
    config.RecipePackageSizes.Should().BeEquivalentTo(new[] { 5, 10, 20, 50 },
        "because default package sizes should match expected values");
  }

  [Theory, AutoData]
  [Trait("Category", "Unit")]
  public void PaymentConfig_WithBuilder_SetsPropertiesCorrectly(
      string secretKey,
      string publishableKey,
      string webhookSecret,
      decimal recipePrice,
      string currency,
      int[] packageSizes)
  {
    // Act
    var config = new PaymentConfigBuilder()
        .WithStripeSecretKey(secretKey)
        .WithStripePublishableKey(publishableKey)
        .WithStripeWebhookSecret(webhookSecret)
        .WithRecipePrice(recipePrice)
        .WithCurrency(currency)
        .WithRecipePackageSizes(packageSizes)
        .Build();

    // Assert
    config.StripeSecretKey.Should().Be(secretKey, "because secret key should be set correctly");
    config.StripePublishableKey.Should().Be(publishableKey, "because publishable key should be set correctly");
    config.StripeWebhookSecret.Should().Be(webhookSecret, "because webhook secret should be set correctly");
    config.RecipePrice.Should().Be(recipePrice, "because recipe price should be set correctly");
    config.Currency.Should().Be(currency, "because currency should be set correctly");
    config.RecipePackageSizes.Should().BeEquivalentTo(packageSizes, "because package sizes should be set correctly");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void CheckoutSessionInfo_DefaultValues_AreSetCorrectly()
  {
    // Act
    var sessionInfo = new CheckoutSessionInfo();

    // Assert
    sessionInfo.Id.Should().BeEmpty("because default ID should be empty");
    sessionInfo.Status.Should().BeEmpty("because default status should be empty");
    sessionInfo.CustomerEmail.Should().BeNull("because default customer email should be null");
    sessionInfo.AmountTotal.Should().BeNull("because default amount total should be null");
    sessionInfo.Currency.Should().BeNull("because default currency should be null");
    sessionInfo.PaymentStatus.Should().BeNull("because default payment status should be null");
  }

  [Theory, AutoData]
  [Trait("Category", "Unit")]
  public void CheckoutSessionInfo_WithBuilder_SetsPropertiesCorrectly(
      string id,
      string status,
      string customerEmail,
      decimal amountTotal,
      string currency,
      string paymentStatus)
  {
    // Act
    var sessionInfo = new CheckoutSessionInfoBuilder()
        .WithId(id)
        .WithStatus(status)
        .WithCustomerEmail(customerEmail)
        .WithAmountTotal(amountTotal)
        .WithCurrency(currency)
        .WithPaymentStatus(paymentStatus)
        .Build();

    // Assert
    sessionInfo.Id.Should().Be(id, "because ID should be set correctly");
    sessionInfo.Status.Should().Be(status, "because status should be set correctly");
    sessionInfo.CustomerEmail.Should().Be(customerEmail, "because customer email should be set correctly");
    sessionInfo.AmountTotal.Should().Be(amountTotal, "because amount total should be set correctly");
    sessionInfo.Currency.Should().Be(currency, "because currency should be set correctly");
    sessionInfo.PaymentStatus.Should().Be(paymentStatus, "because payment status should be set correctly");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void CheckoutSessionInfo_AsIncomplete_SetsCorrectProperties()
  {
    // Act
    var sessionInfo = new CheckoutSessionInfoBuilder()
        .AsIncomplete()
        .Build();

    // Assert
    sessionInfo.Status.Should().Be("open", "because incomplete sessions should have 'open' status");
    sessionInfo.PaymentStatus.Should().Be("unpaid", "because incomplete sessions should be unpaid");
    sessionInfo.AmountTotal.Should().BeNull("because incomplete sessions should have null amount");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void CheckoutSessionInfo_AsFailed_SetsCorrectProperties()
  {
    // Act
    var sessionInfo = new CheckoutSessionInfoBuilder()
        .AsFailed()
        .Build();

    // Assert
    sessionInfo.Status.Should().Be("complete", "because failed payments still complete the session");
    sessionInfo.PaymentStatus.Should().Be("failed", "because failed sessions should have 'failed' payment status");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void CheckoutSessionInfo_AsSuccessful_SetsCorrectProperties()
  {
    // Act
    var sessionInfo = new CheckoutSessionInfoBuilder()
        .AsSuccessful()
        .Build();

    // Assert
    sessionInfo.Status.Should().Be("complete", "because successful payments complete the session");
    sessionInfo.PaymentStatus.Should().Be("paid", "because successful sessions should be paid");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void StripeSessionMetadata_DefaultValues_AreSetCorrectly()
  {
    // Act
    var metadata = new StripeSessionMetadata();

    // Assert
    metadata.PaymentType.Should().Be(PaymentType.OrderCompletion, "because default payment type should be OrderCompletion");
    metadata.CustomerEmail.Should().BeEmpty("because default customer email should be empty");
    metadata.RecipeCount.Should().BeNull("because default recipe count should be null");
  }

  [Theory, AutoData]
  [Trait("Category", "Unit")]
  public void StripeSessionMetadata_PropertyAssignment_WorksCorrectly(
      PaymentType paymentType,
      string customerEmail,
      int recipeCount)
  {
    // Act
    var metadata = new StripeSessionMetadata
    {
      PaymentType = paymentType,
      CustomerEmail = customerEmail,
      RecipeCount = recipeCount
    };

    // Assert
    metadata.PaymentType.Should().Be(paymentType, "because payment type should be assignable");
    metadata.CustomerEmail.Should().Be(customerEmail, "because customer email should be assignable");
    metadata.RecipeCount.Should().Be(recipeCount, "because recipe count should be assignable");
  }

  [Theory]
  [InlineData(PaymentType.OrderCompletion, "test@example.com", null)]
  [InlineData(PaymentType.RecipeCredit, "customer@domain.com", 10)]
  [Trait("Category", "Unit")]
  public void StripeSessionMetadata_DifferentScenarios_AreHandledCorrectly(
      PaymentType paymentType,
      string customerEmail,
      int? recipeCount)
  {
    // Act
    var metadata = new StripeSessionMetadata
    {
      PaymentType = paymentType,
      CustomerEmail = customerEmail,
      RecipeCount = recipeCount
    };

    // Assert
    metadata.PaymentType.Should().Be(paymentType, "because payment type should match input");
    metadata.CustomerEmail.Should().Be(customerEmail, "because customer email should match input");
    metadata.RecipeCount.Should().Be(recipeCount, "because recipe count should match input (including null)");
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [Trait("Category", "Unit")]
  public void PaymentConfig_WithoutStripeCredentials_AllowsEmptyValues(string? emptyValue)
  {
    // Act
    var config = new PaymentConfigBuilder()
        .WithStripeSecretKey(emptyValue ?? string.Empty)
        .WithStripePublishableKey(emptyValue ?? string.Empty)
        .WithStripeWebhookSecret(emptyValue ?? string.Empty)
        .Build();

    // Assert
    config.StripeSecretKey.Should().Be(emptyValue ?? string.Empty, "because empty secret key should be allowed");
    config.StripePublishableKey.Should().Be(emptyValue ?? string.Empty, "because empty publishable key should be allowed");
    config.StripeWebhookSecret.Should().Be(emptyValue ?? string.Empty, "because empty webhook secret should be allowed");
  }

  [Theory]
  [InlineData(-1.00)]
  [InlineData(0.00)]
  [InlineData(999.99)]
  [Trait("Category", "Unit")]
  public void PaymentConfig_RecipePrice_AcceptsDifferentValues(decimal price)
  {
    // Act
    var config = new PaymentConfigBuilder()
        .WithRecipePrice(price)
        .Build();

    // Assert
    config.RecipePrice.Should().Be(price, "because recipe price should accept various decimal values");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void PaymentConfig_RecipePackageSizes_CanBeEmpty()
  {
    // Act
    var config = new PaymentConfigBuilder()
        .WithRecipePackageSizes()
        .Build();

    // Assert
    config.RecipePackageSizes.Should().BeEmpty("because empty package sizes array should be allowed");
  }
}
