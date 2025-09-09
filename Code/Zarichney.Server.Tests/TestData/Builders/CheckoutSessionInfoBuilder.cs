using Zarichney.Services.Payment;

namespace Zarichney.Server.Tests.TestData.Builders;

/// <summary>
/// Test data builder for CheckoutSessionInfo objects used in payment service testing.
/// Provides fluent interface for creating test-specific CheckoutSessionInfo instances.
/// </summary>
public class CheckoutSessionInfoBuilder
{
  private CheckoutSessionInfo _entity;

  /// <summary>
  /// Creates a builder with default CheckoutSessionInfo values suitable for testing.
  /// </summary>
  public CheckoutSessionInfoBuilder()
  {
    _entity = new CheckoutSessionInfo
    {
      Id = "cs_test_" + Guid.NewGuid().ToString("N")[..24],
      Status = "complete",
      CustomerEmail = "test@example.com",
      AmountTotal = 150m, // $1.50 in dollars (converted from cents)
      Currency = "usd",
      PaymentStatus = "paid"
    };
  }

  /// <summary>
  /// Sets the session ID for testing.
  /// </summary>
  public CheckoutSessionInfoBuilder WithId(string id)
  {
    _entity.Id = id;
    return this;
  }

  /// <summary>
  /// Sets the session status for testing.
  /// </summary>
  public CheckoutSessionInfoBuilder WithStatus(string status)
  {
    _entity.Status = status;
    return this;
  }

  /// <summary>
  /// Sets the customer email for testing.
  /// </summary>
  public CheckoutSessionInfoBuilder WithCustomerEmail(string? customerEmail)
  {
    _entity.CustomerEmail = customerEmail;
    return this;
  }

  /// <summary>
  /// Sets the amount total in dollars for testing.
  /// </summary>
  public CheckoutSessionInfoBuilder WithAmountTotal(decimal? amountTotal)
  {
    _entity.AmountTotal = amountTotal;
    return this;
  }

  /// <summary>
  /// Sets the currency for testing.
  /// </summary>
  public CheckoutSessionInfoBuilder WithCurrency(string? currency)
  {
    _entity.Currency = currency;
    return this;
  }

  /// <summary>
  /// Sets the payment status for testing.
  /// </summary>
  public CheckoutSessionInfoBuilder WithPaymentStatus(string? paymentStatus)
  {
    _entity.PaymentStatus = paymentStatus;
    return this;
  }

  /// <summary>
  /// Creates a CheckoutSessionInfo representing an incomplete session.
  /// </summary>
  public CheckoutSessionInfoBuilder AsIncomplete()
  {
    _entity.Status = "open";
    _entity.PaymentStatus = "unpaid";
    _entity.AmountTotal = null;
    return this;
  }

  /// <summary>
  /// Creates a CheckoutSessionInfo representing a failed payment.
  /// </summary>
  public CheckoutSessionInfoBuilder AsFailed()
  {
    _entity.Status = "complete";
    _entity.PaymentStatus = "failed";
    return this;
  }

  /// <summary>
  /// Creates a CheckoutSessionInfo representing a successful payment.
  /// </summary>
  public CheckoutSessionInfoBuilder AsSuccessful()
  {
    _entity.Status = "complete";
    _entity.PaymentStatus = "paid";
    return this;
  }

  /// <summary>
  /// Builds the CheckoutSessionInfo instance.
  /// </summary>
  public CheckoutSessionInfo Build() => _entity;
}
