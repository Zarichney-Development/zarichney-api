using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Zarichney.Services.Payment;

namespace Zarichney.Server.Tests.TestData.AutoFixtureCustomizations;

/// <summary>
/// AutoFixture customization for ILogger dependencies.
/// Ensures proper mock creation for logger instances used throughout the application.
/// </summary>
public class LoggerCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // Ensure ILogger<PaymentService> is properly handled by AutoMoq
    // This explicit registration ensures proper dependency injection for PaymentService constructor
    fixture.Customize<ILogger<PaymentService>>(composer =>
        composer.FromFactory(() => Mock.Of<ILogger<PaymentService>>()));

    // Generic logger customization for any ILogger<T> not explicitly handled
    // This helps AutoFixture create appropriate mocks for any logger dependency
    // Note: AutoMoqCustomization is applied in the main AutoMoqDataAttribute, so we don't need to re-apply it here
  }
}
