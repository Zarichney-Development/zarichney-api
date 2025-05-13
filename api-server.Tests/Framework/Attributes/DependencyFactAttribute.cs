using System.Collections.Generic;
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
///    This automatically maps to the appropriate dependency traits for filtering/reporting.
/// 2. Without parameters, relying on string-based trait dependencies: [DependencyFact]
///    combined with [Trait(TestCategories.Dependency, TestCategories.Database)]
/// </summary>
[XunitTestCaseDiscoverer("Zarichney.Tests.Framework.Helpers.SkipMissingDependencyDiscoverer", "Zarichney.Tests")]
public sealed class DependencyFactAttribute : FactAttribute
{
  // Maps ApiFeature enums to TestCategories.Dependency trait values
  private static readonly Dictionary<ApiFeature, string> ApiFeatureToTraitMap = new()
  {
    { ApiFeature.LLM, TestCategories.ExternalOpenAI },
    { ApiFeature.Transcription, TestCategories.ExternalOpenAI },
    { ApiFeature.EmailSending, TestCategories.ExternalMSGraph },
    { ApiFeature.Payments, TestCategories.ExternalStripe },
    { ApiFeature.GitHubAccess, TestCategories.ExternalGitHub },
    { ApiFeature.AiServices, TestCategories.ExternalOpenAI },
    // Core feature doesn't map to a specific external dependency
  };

  // Store the mapped trait values for testing and for use by test discoverer
  private readonly List<(string Name, string Value)> _dependencyTraits = new();

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
  /// Maps to appropriate TestCategories.Dependency traits for filtering/reporting.
  /// </summary>
  /// <param name="requiredFeatures">One or more ApiFeature enum values that must be available.</param>
  public DependencyFactAttribute(params ApiFeature[] requiredFeatures)
  {
    RequiredFeatures = requiredFeatures?.Length > 0 ? requiredFeatures : null;
    
    // If RequiredFeatures is set, generate trait mappings for each feature
    if (RequiredFeatures != null)
    {
      foreach (var feature in RequiredFeatures)
      {
        if (ApiFeatureToTraitMap.TryGetValue(feature, out var traitValue))
        {
          // Store the trait mapping information
          _dependencyTraits.Add((TestCategories.Dependency, traitValue));
        }
      }
    }
  }

  /// <summary>
  /// Gets the collection of dependency traits that correspond to the ApiFeature values.
  /// This is used both for testing and to allow the test discoverer to see the trait mappings.
  /// </summary>
  /// <returns>Collection of trait name/value pairs.</returns>
  public IReadOnlyCollection<(string Name, string Value)> GetDependencyTraits() => _dependencyTraits;
}