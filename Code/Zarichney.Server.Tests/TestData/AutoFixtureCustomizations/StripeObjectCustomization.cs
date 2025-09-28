using AutoFixture;
using Stripe;
using Stripe.Checkout;

namespace Zarichney.Tests.TestData.AutoFixtureCustomizations;

/// <summary>
/// AutoFixture customization for Stripe SDK objects.
/// Creates concrete Stripe objects with realistic test data for payment processing tests.
/// </summary>
public class StripeObjectCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // Create concrete Session objects instead of trying to mock interfaces
    fixture.Register(() => new Session
    {
      Id = "cs_test_" + fixture.Create<string>()[..20],
      Status = "complete",
      CustomerEmail = fixture.Create<string>() + "@test.com",
      AmountTotal = fixture.Create<int>() % 10000 + 1000, // Between $10-$100
      Currency = "usd",
      PaymentStatus = "paid",
      Metadata = new Dictionary<string, string>
      {
        ["payment_type"] = "OrderCompletion",
        ["customer_email"] = fixture.Create<string>() + "@test.com"
      }
    });

    // Create concrete Event objects for webhook testing
    fixture.Register(() => new Event
    {
      Id = "evt_test_" + fixture.Create<string>()[..20],
      Type = "checkout.session.completed",
      Data = new EventData
      {
        Object = fixture.Create<Session>()
      }
    });
  }
}
