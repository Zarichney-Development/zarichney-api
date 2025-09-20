using Xunit.Abstractions;
using Xunit.Sdk;
using Zarichney.Services.Status;
using Zarichney.Server.Tests.Framework.Attributes;

namespace Zarichney.Server.Tests.Framework.Helpers;

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
    var constructorArgs = factAttribute.GetConstructorArguments();
    var firstArg = constructorArgs.FirstOrDefault();

    // Handle different possible types for the enum value
    ExternalServices? requiredUnavailableService = null;
    if (firstArg is ExternalServices enumValue)
    {
      requiredUnavailableService = enumValue;
    }
    else if (firstArg is int intValue)
    {
      requiredUnavailableService = (ExternalServices)intValue;
    }
    else if (firstArg != null)
    {
      // Try to parse if it's some other representation
      if (Enum.TryParse<ExternalServices>(firstArg.ToString(), out var parsedValue))
      {
        requiredUnavailableService = parsedValue;
      }
    }

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
