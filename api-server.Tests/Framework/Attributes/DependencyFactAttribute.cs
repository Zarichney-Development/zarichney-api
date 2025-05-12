using Xunit;
using Xunit.Sdk;

namespace Zarichney.Tests.Framework.Attributes;

/// <summary>
/// Custom Fact attribute that skips tests when dependencies are missing.
/// This attribute works with a custom test case discoverer that properly
/// skips tests when the test class indicates dependencies are missing.
/// </summary>
[XunitTestCaseDiscoverer("Zarichney.Tests.Framework.Helpers.SkipMissingDependencyDiscoverer", "Zarichney.Tests")]
public sealed class DependencyFactAttribute : FactAttribute;
