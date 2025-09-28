using Xunit;
using Xunit.Sdk;
using Zarichney.Services.Status;

namespace Zarichney.Tests.Framework.Attributes;

/// <summary>
/// Custom Fact attribute that skips tests when dependencies are missing.
/// This attribute works with a custom test case discoverer that properly
/// skips tests when the test class indicates dependencies are missing.
///
/// The attribute can be used in these ways:
/// 1. With specific ExternalServices enums: [DependencyFact(ExternalServices.OpenAiApi, ExternalServices.Transcription)]
///    This automatically maps to the appropriate dependency traits for filtering/reporting.
/// 2. With InfrastructureDependency enums: [DependencyFact(InfrastructureDependency.Database)]
///    This explicitly indicates infrastructure-level dependencies like Database or Docker.
/// 3. With a combination of both: [DependencyFact(ApiFeature.LLM, InfrastructureDependency.Database)]
///    For tests requiring both API features and infrastructure dependencies.
/// 4. Without parameters, relying on string-based trait dependencies: [DependencyFact]
///    combined with [Trait(TestCategories.Dependency, TestCategories.Database)]
///    This approach is maintained for backward compatibility.
/// </summary>
[XunitTestCaseDiscoverer("Zarichney.Tests.Framework.Helpers.SkipMissingDependencyDiscoverer", "Zarichney.Tests")]
public sealed class DependencyFactAttribute : FactAttribute
{
  // Maps ExternalServices enums to TestCategories.Dependency trait values
  private static readonly Dictionary<ExternalServices, string> ExternalServicesToTraitMap = new()
  {
    { ExternalServices.OpenAiApi, TestCategories.ExternalOpenAI },
    { ExternalServices.MsGraph, TestCategories.ExternalMSGraph },
    { ExternalServices.Stripe, TestCategories.ExternalStripe },
    { ExternalServices.GitHubAccess, TestCategories.ExternalGitHub },
    { ExternalServices.MailCheck, TestCategories.ExternalMSGraph },
    // FrontEnd doesn't map to a specific external dependency
  };

  // Maps InfrastructureDependency enums to TestCategories.Dependency trait values
  private static readonly Dictionary<InfrastructureDependency, string> InfrastructureToTraitMap = new()
  {
    { InfrastructureDependency.Database, TestCategories.Database },
    { InfrastructureDependency.Docker, TestCategories.Docker }
  };

  // Store the mapped trait values for testing and for use by test discoverer
  private readonly List<(string Name, string Value)> _dependencyTraits = [];

  /// <summary>
  /// Gets the array of required ExternalServices values that must be available for the test to run.
  /// Will be null if the attribute was created without specifying ExternalServices dependencies.
  /// </summary>
  public ExternalServices[]? RequiredExternalServices { get; }

  /// <summary>
  /// Gets the array of required InfrastructureDependency values that must be available for the test to run.
  /// Will be null if the attribute was created without specifying InfrastructureDependency dependencies.
  /// </summary>
  public InfrastructureDependency[]? RequiredInfrastructure { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="DependencyFactAttribute"/> class.
  /// This constructor is used when no specific dependencies are specified.
  /// The test will rely on string-based trait dependencies using [Trait(TestCategories.Dependency, ...)]
  /// </summary>
  public DependencyFactAttribute()
  {
    RequiredExternalServices = null;
    RequiredInfrastructure = null;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DependencyFactAttribute"/> class with specific
  /// ExternalServices dependencies that must be available for the test to run.
  /// Maps to appropriate TestCategories.Dependency traits for filtering/reporting.
  /// </summary>
  /// <param name="requiredFeatures">One or more ExternalServices enum values that must be available.</param>
  public DependencyFactAttribute(params ExternalServices[] requiredFeatures)
  {
    RequiredExternalServices = requiredFeatures.Length > 0 ? requiredFeatures : null;
    RequiredInfrastructure = null;

    // If RequiredExternalServices is set, generate trait mappings for each feature
    if (RequiredExternalServices != null)
    {
      foreach (var feature in RequiredExternalServices)
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
  /// Initializes a new instance of the <see cref="DependencyFactAttribute"/> class with specific
  /// InfrastructureDependency dependencies that must be available for the test to run.
  /// Maps to appropriate TestCategories.Dependency traits for filtering/reporting.
  /// </summary>
  /// <param name="requiredInfrastructure">One or more InfrastructureDependency enum values that must be available.</param>
  public DependencyFactAttribute(params InfrastructureDependency[] requiredInfrastructure)
  {
    RequiredExternalServices = null;
    RequiredInfrastructure = requiredInfrastructure.Length > 0 ? requiredInfrastructure : null;

    // If RequiredInfrastructure is set, generate trait mappings for each infrastructure dependency
    if (RequiredInfrastructure != null)
    {
      foreach (var infrastructure in RequiredInfrastructure)
      {
        if (InfrastructureToTraitMap.TryGetValue(infrastructure, out var traitValue))
        {
          // Store the trait mapping information
          _dependencyTraits.Add((TestCategories.Dependency, traitValue));
        }
      }
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DependencyFactAttribute"/> class with specific
  /// ExternalServices and InfrastructureDependency dependencies that must be available for the test to run.
  /// Maps to appropriate TestCategories.Dependency traits for filtering/reporting.
  /// </summary>
  /// <param name="requiredFeature">An ExternalServices enum value that must be available.</param>
  /// <param name="requiredInfrastructure">An InfrastructureDependency enum value that must be available.</param>
  public DependencyFactAttribute(ExternalServices requiredFeature, InfrastructureDependency requiredInfrastructure)
  {
    RequiredExternalServices = [requiredFeature];
    RequiredInfrastructure = [requiredInfrastructure];

    // Generate trait mappings for the feature
    if (ExternalServicesToTraitMap.TryGetValue(requiredFeature, out var featureTraitValue))
    {
      _dependencyTraits.Add((TestCategories.Dependency, featureTraitValue));
    }

    // Generate trait mappings for the infrastructure dependency
    if (InfrastructureToTraitMap.TryGetValue(requiredInfrastructure, out var infraTraitValue))
    {
      _dependencyTraits.Add((TestCategories.Dependency, infraTraitValue));
    }
  }

  /// <summary>
  /// Gets the collection of dependency traits that correspond to the ExternalServices and InfrastructureDependency values.
  /// This is used both for testing and to allow the test discoverer to see the trait mappings.
  /// </summary>
  /// <returns>Collection of trait name/value pairs.</returns>
  public IReadOnlyCollection<(string Name, string Value)> GetDependencyTraits() => _dependencyTraits;
}
