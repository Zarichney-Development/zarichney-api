using Xunit;
using Xunit.Sdk;

namespace Zarichney.Tests.Framework.Attributes;

/// <summary>
/// Custom Fact attribute that skips tests when dependencies are missing.
/// This attribute works with a custom test case discoverer that properly
/// skips tests when the test class indicates dependencies are missing.
/// </summary>
[XunitTestCaseDiscoverer("Zarichney.Tests.Helpers.SkipMissingDependencyDiscoverer", "api-server.Tests")]
public class DependencyFactAttribute : FactAttribute
{
}