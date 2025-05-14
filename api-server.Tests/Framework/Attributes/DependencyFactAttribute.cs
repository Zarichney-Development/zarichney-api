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
/// 1. With specific ExternalServices enums: [DependencyFact(ExternalServices.LLM, ExternalServices.Transcription)]
///    This automatically maps to the appropriate dependency traits for filtering/reporting.
/// 2. Without parameters, relying on string-based trait dependencies: [DependencyFact]
///    combined with [Trait(TestCategories.Dependency, TestCategories.Database)]
/// </summary>
[XunitTestCaseDiscoverer("Zarichney.Tests.Framework.Helpers.SkipMissingDependencyDiscoverer", "Zarichney.Tests")]
public sealed class DependencyFactAttribute : FactAttribute
{
  // Maps ExternalServices enums to TestCategories.Dependency trait values
  private static readonly Dictionary<ExternalServices, string> ExternalServicesToTraitMap = new()
  {
    { ExternalServices.LLM, TestCategories.ExternalOpenAI },
    { ExternalServices.Transcription, TestCategories.ExternalOpenAI },
    { ExternalServices.EmailSending, TestCategories.ExternalMSGraph },
    { ExternalServices.Payments, TestCategories.ExternalStripe },
    { ExternalServices.GitHubAccess, TestCategories.ExternalGitHub },
    { ExternalServices.AiServices, TestCategories.ExternalOpenAI },
    // Core feature doesn't map to a specific external dependency
  };

  // Store the mapped trait values for testing and for use by test discoverer
  private readonly List<(string Name, string Value)> _dependencyTraits = new();

  /// <summary>
  /// Gets the array of required ExternalServices values that must be available for the test to run.
  /// Will be null if the attribute was created without specifying ExternalServices dependencies.
  /// </summary>
  public ExternalServices[]? RequiredFeatures { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="DependencyFactAttribute"/> class.
  /// This constructor is used when no specific ExternalServices dependencies are specified.
  /// The test will rely on string-based trait dependencies using [Trait(TestCategories.Dependency, ...)]
  /// </summary>
  public DependencyFactAttribute()
  {
    RequiredFeatures = null;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DependencyFactAttribute"/> class with specific
  /// ExternalServices dependencies that must be available for the test to run.
  /// Maps to appropriate TestCategories.Dependency traits for filtering/reporting.
  /// </summary>
  /// <param name="requiredFeatures">One or more ExternalServices enum values that must be available.</param>
  public DependencyFactAttribute(params ExternalServices[] requiredFeatures)
  {
    RequiredFeatures = requiredFeatures?.Length > 0 ? requiredFeatures : null;

    // If RequiredFeatures is set, generate trait mappings for each feature
    if (RequiredFeatures != null)
    {
      foreach (var feature in RequiredFeatures)
      {
        if (ExternalServicesToTraitMap.TryGetValue(feature, out var traitValue))
        {
          // Store the trait mapping information
          _dependencyTraits.Add((TestCategories.Dependency, traitValue));
        }
      }
    }
  }

  /// <summary>
  /// Gets the collection of dependency traits that correspond to the ExternalServices values.
  /// This is used both for testing and to allow the test discoverer to see the trait mappings.
  /// </summary>
  /// <returns>Collection of trait name/value pairs.</returns>
  public IReadOnlyCollection<(string Name, string Value)> GetDependencyTraits() => _dependencyTraits;
}
