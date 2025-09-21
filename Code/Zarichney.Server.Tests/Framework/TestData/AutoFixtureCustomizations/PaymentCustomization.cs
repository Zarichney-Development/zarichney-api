using AutoFixture;
using Stripe;
using Stripe.Checkout;
using Zarichney.Services.Payment;

namespace Zarichney.Server.Tests.Framework.TestData.AutoFixtureCustomizations;

/// <summary>
/// AutoFixture customization for Payment-related types to ensure consistent test data generation.
/// Provides realistic defaults for Stripe entities, payment configurations, and payment models.
/// </summary>
public class PaymentCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // PaymentConfig customization
    fixture.Customize<PaymentConfig>(c => c
        .With(p => p.StripeSecretKey, () => "sk_test_" + fixture.Create<string>()[..24])
        .With(p => p.StripePublishableKey, () => "pk_test_" + fixture.Create<string>()[..24])
        .With(p => p.StripeWebhookSecret, () => "whsec_" + fixture.Create<string>()[..24])
        .With(p => p.SuccessUrl, "/order/success/{0}")
        .With(p => p.CancelUrl, "/order/cancel/{0}")
        .With(p => p.RecipePrice, 1.50m)
        .With(p => p.Currency, "usd")
        .With(p => p.RecipePackageSizes, new[] { 5, 10, 20, 50 }));

    // CheckoutSessionInfo customization
    fixture.Customize<CheckoutSessionInfo>(c => c
        .With(s => s.Id, () => "cs_test_" + fixture.Create<string>()[..24])
        .With(s => s.Status, "complete")
        .With(s => s.CustomerEmail, () => fixture.Create<string>() + "@example.com")
        .With(s => s.AmountTotal, () => Math.Round(fixture.Create<decimal>() % 1000, 2))
        .With(s => s.Currency, "usd")
        .With(s => s.PaymentStatus, "paid"));

    // StripeSessionMetadata customization
    fixture.Customize<StripeSessionMetadata>(c => c
        .With(m => m.PaymentType, PaymentType.OrderCompletion)
        .With(m => m.CustomerEmail, () => fixture.Create<string>() + "@example.com")
        .With(m => m.RecipeCount, () => fixture.Create<int>() % 50 + 1));

    // Stripe Session customization
    fixture.Customize<Session>(c => c
        .With(s => s.Id, () => "cs_test_" + fixture.Create<string>()[..24])
        .With(s => s.Status, "complete")
        .With(s => s.CustomerEmail, () => fixture.Create<string>() + "@example.com")
        .With(s => s.AmountTotal, () => (long)(Math.Round(fixture.Create<decimal>() % 1000, 2) * 100))
        .With(s => s.Currency, "usd")
        .With(s => s.PaymentStatus, "paid")
        .With(s => s.Url, () => "https://checkout.stripe.com/pay/" + fixture.Create<string>()[..24])
        .With(s => s.ClientReferenceId, () => Guid.NewGuid().ToString())
        .Without(s => s.LineItems) // Complex nested property handled separately
        .Without(s => s.Metadata)); // Handled with specific test data

    // Stripe PaymentIntent customization
    fixture.Customize<PaymentIntent>(c => c
        .With(p => p.Id, () => "pi_test_" + fixture.Create<string>()[..24])
        .With(p => p.Status, "succeeded")
        .With(p => p.Amount, () => (long)(Math.Round(fixture.Create<decimal>() % 1000, 2) * 100))
        .With(p => p.Currency, "usd")
        .Without(p => p.Metadata)); // Handled with specific test data

    // Stripe Event customization
    fixture.Customize<Event>(c => c
        .With(e => e.Id, () => "evt_test_" + fixture.Create<string>()[..24])
        .With(e => e.Type, "checkout.session.completed")
        .Without(e => e.Data)); // Complex property handled in specific tests
  }
}
