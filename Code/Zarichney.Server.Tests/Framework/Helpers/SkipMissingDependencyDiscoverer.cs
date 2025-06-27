using Xunit.Abstractions;
using Xunit.Sdk;

namespace Zarichney.Tests.Framework.Helpers;

/// <summary>
/// Custom test case discoverer for dependency-aware tests.
/// This discoverer creates test cases that check for missing dependencies and skip tests when needed.
/// </summary>
public class SkipMissingDependencyDiscoverer(IMessageSink diagnosticMessageSink) : IXunitTestCaseDiscoverer
{
  /// <summary>
  /// Discovers test cases for dependency-aware tests.
  /// </summary>
  public IEnumerable<IXunitTestCase> Discover(
      ITestFrameworkDiscoveryOptions discoveryOptions,
      ITestMethod testMethod,
      IAttributeInfo factAttribute)
  {
    // Get test name settings from discovery options
    var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
    var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

    // Create test case with proper display name settings
    yield return new SkipMissingDependencyTestCase(
        diagnosticMessageSink,
        methodDisplay,
        methodDisplayOptions,
        testMethod);
  }
}
