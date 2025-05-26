using Xunit;
using Xunit.Sdk;
using Zarichney.Services.Status;

namespace Zarichney.Tests.Framework.Attributes;

/// <summary>
/// Custom Fact attribute that runs tests ONLY when the specified external service is UNAVAILABLE.
/// This attribute is used for integration tests that verify HTTP 503 responses from dependency-aware
/// API endpoints when their specific external dependencies are unavailable.
/// 
/// If the specified service is available, the test will be skipped.
/// If the specified service is unavailable, the test will run normally.
/// 
/// Usage example:
/// [ServiceUnavailableFact(ExternalServices.OpenAiApi)]
/// public async Task GetAiResponse_WhenOpenAiUnavailable_Returns503()
/// {
///     // This test only runs when OpenAI is unavailable
///     var response = await client.GetAiResponse();
///     response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
/// }
/// </summary>
[XunitTestCaseDiscoverer("Zarichney.Tests.Framework.Helpers.ServiceUnavailableFactDiscoverer", "Zarichney.Tests")]
public sealed class ServiceUnavailableFactAttribute : FactAttribute
{
  /// <summary>
  /// Gets the external service that must be UNAVAILABLE for the test to run.
  /// </summary>
  public ExternalServices RequiredUnavailableService { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceUnavailableFactAttribute"/> class.
  /// The test will only run if the specified service is unavailable.
  /// </summary>
  /// <param name="requiredUnavailableService">The ExternalServices enum value that must be unavailable for the test to run.</param>
  public ServiceUnavailableFactAttribute(ExternalServices requiredUnavailableService)
  {
    RequiredUnavailableService = requiredUnavailableService;
  }
}
