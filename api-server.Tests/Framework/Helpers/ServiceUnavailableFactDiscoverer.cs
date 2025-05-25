using Xunit.Abstractions;
using Xunit.Sdk;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;

namespace Zarichney.Tests.Framework.Helpers;

/// <summary>
/// Custom test case discoverer for ServiceUnavailableFact tests.
/// This discoverer creates test cases that run only when the specified external service is unavailable.
/// </summary>
public class ServiceUnavailableFactDiscoverer(IMessageSink diagnosticMessageSink) : IXunitTestCaseDiscoverer
{
  /// <summary>
  /// Discovers test cases for ServiceUnavailableFact tests.
  /// Creates custom test cases that check service availability and skip when the service is available.
  /// </summary>
  public IEnumerable<IXunitTestCase> Discover(
      ITestFrameworkDiscoveryOptions discoveryOptions,
      ITestMethod testMethod,
      IAttributeInfo factAttribute)
  {
    // Get test name settings from discovery options
    var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
    var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

    // Extract the required unavailable service from the attribute
    var requiredUnavailableService = factAttribute.GetConstructorArguments().FirstOrDefault() as ExternalServices?;

    if (!requiredUnavailableService.HasValue)
    {
      // If no service is specified, fall back to a standard test case
      yield return new XunitTestCase(diagnosticMessageSink, methodDisplay, methodDisplayOptions, testMethod);
      yield break;
    }

    // Create custom test case that checks service availability
    yield return new ServiceUnavailableTestCase(
        diagnosticMessageSink,
        methodDisplay,
        methodDisplayOptions,
        testMethod,
        requiredUnavailableService.Value);
  }
}
