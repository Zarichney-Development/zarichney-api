using Xunit;
using Xunit.Sdk;
using Zarichney.Services.Status;

namespace Zarichney.Tests.Framework.Attributes;

/// <summary>
/// Custom Fact attribute that skips tests when dependencies are missing.
/// This attribute works with a custom test case discoverer that properly
/// skips tests when the test class indicates dependencies are missing.
///
/// The attribute can be used in two ways:
/// 1. With specific ApiFeature enums: [DependencyFact(ApiFeature.LLM, ApiFeature.Transcription)]
/// 2. Without parameters, relying on string-based trait dependencies: [DependencyFact]
///    combined with [Trait(TestCategories.Dependency, TestCategories.Database)]
/// </summary>
[XunitTestCaseDiscoverer("Zarichney.Tests.Framework.Helpers.SkipMissingDependencyDiscoverer", "Zarichney.Tests")]
public sealed class DependencyFactAttribute : FactAttribute
{
  /// <summary>
  /// Gets the array of required ApiFeature values that must be available for the test to run.
  /// Will be null if the attribute was created without specifying ApiFeature dependencies.
  /// </summary>
  public ApiFeature[]? RequiredFeatures { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="DependencyFactAttribute"/> class.
  /// This constructor is used when no specific ApiFeature dependencies are specified.
  /// The test will rely on string-based trait dependencies using [Trait(TestCategories.Dependency, ...)]
  /// </summary>
  public DependencyFactAttribute()
  {
    RequiredFeatures = null;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DependencyFactAttribute"/> class with specific
  /// ApiFeature dependencies that must be available for the test to run.
  /// </summary>
  /// <param name="requiredFeatures">One or more ApiFeature enum values that must be available.</param>
  public DependencyFactAttribute(params ApiFeature[] requiredFeatures)
  {
    RequiredFeatures = requiredFeatures?.Length > 0 ? requiredFeatures : null;
  }
}
