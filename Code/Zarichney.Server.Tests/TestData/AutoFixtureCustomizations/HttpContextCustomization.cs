using AutoFixture;
using System.Net.Http.Headers;

namespace Zarichney.Tests.TestData.AutoFixtureCustomizations;

/// <summary>
/// AutoFixture customization for HTTP-related objects that cannot be directly instantiated.
/// Handles HttpResponseHeaders and other HTTP infrastructure types that require special handling in tests.
/// </summary>
public class HttpContextCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // HttpResponseHeaders cannot be created directly, so we omit auto-properties
    fixture.Customize<HttpResponseHeaders>(c => c.OmitAutoProperties());

    // For StripeResponse and similar objects that depend on HttpResponseHeaders,
    // we provide null as a safe default
    fixture.Register<HttpResponseHeaders?>(() => null);
  }
}
