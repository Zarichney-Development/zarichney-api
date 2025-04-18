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
        // Create our custom test case that will check for missing dependencies
        yield return new SkipMissingDependencyTestCase(
            diagnosticMessageSink,
            discoveryOptions.MethodDisplayOrDefault(),
            discoveryOptions.MethodDisplayOptionsOrDefault(),
            testMethod);
    }
}