using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Framework.Helpers;

namespace Zarichney.Tests.Integration;

/// <summary>
/// Base class for integration tests that provides common setup and accessors.
/// Implements IAsyncLifetime to check for required configuration dependencies based on test traits.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
  private static readonly Dictionary<string, List<string>> TraitToConfigNamesMap = new()
  {
    { TestCategories.Database, ["Database Connection"] },
    { TestCategories.Docker, ["Docker Availability"] },
    { TestCategories.ExternalStripe, ["Stripe Secret Key", "Stripe Webhook Secret"] },
    { TestCategories.ExternalOpenAI, ["OpenAI API Key"] },
    { TestCategories.ExternalGitHub, ["GitHub Access Token"] },
    {
      TestCategories.ExternalMSGraph, [
        "Email AzureTenantId",
        "Email AzureAppId",
        "Email AzureAppSecret",
        "Email FromEmail",
        "Email MailCheckApiKey"
      ]
    }
  };

  protected CustomWebApplicationFactory Factory => _apiClientFixture.Factory;
  protected readonly ApiClientFixture _apiClientFixture;
  private readonly IDisposable _testClassContext;

  private bool _dependenciesChecked;
  private string? SkipReason { get; set; }

  /// <summary>
  /// Sets a skip reason. This is used internally and by derived classes.
  /// </summary>
  protected void SetSkipReason(string reason)
  {
    SkipReason = reason;
  }

  /// <summary>
  /// Checks if the test should be skipped due to missing dependencies.
  /// </summary>
  private bool ShouldSkip => !string.IsNullOrEmpty(SkipReason);

  /// <summary>
  /// Initializes a new instance of the <see cref="IntegrationTestBase"/> class.
  /// </summary>
  /// <param name="apiClientFixture">The API client fixture.</param>
  /// <param name="testOutputHelper"></param>
  protected IntegrationTestBase(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
  {
    _apiClientFixture = apiClientFixture;
    apiClientFixture.AttachToSerilog(testOutputHelper);

    // Push test class name to logging context for all tests in this class
    _testClassContext = Serilog.Context.LogContext.PushProperty("TestClassName", GetType().Name);

    // Database availability will be checked based on dependency traits during InitializeAsync
  }


  /// <summary>
  /// Gets a service from the factory's service provider.
  /// </summary>
  /// <typeparam name="T">The type of service to get.</typeparam>
  /// <returns>The service instance.</returns>
  protected T GetService<T>() where T : class
  {
    using var scope = Factory.Services.CreateScope();
    return scope.ServiceProvider.GetRequiredService<T>();
  }

  /// <summary>
  /// Gets a mock service from the factory's service provider.
  /// </summary>
  /// <typeparam name="T">The type of service to get.</typeparam>
  /// <returns>The mock service instance.</returns>
  protected Moq.Mock<T> GetMockService<T>() where T : class
  {
    using var scope = Factory.Services.CreateScope();
    return scope.ServiceProvider.GetRequiredService<Moq.Mock<T>>();
  }

  /// <summary>
  /// Creates a logging context for a specific test method.
  /// Use this in a using statement to ensure the test method name appears in logs for that test.
  /// </summary>
  /// <param name="testMethodName">The name of the test method. Use nameof() to get the current method name.</param>
  /// <returns>A disposable that will clean up the logging context when disposed.</returns>
  protected IDisposable CreateTestMethodContext(string testMethodName)
  {
    return Serilog.Context.LogContext.PushProperty("TestMethodName", testMethodName);
  }

  /// <summary>
  /// Checks if the test has the required configuration dependencies and sets SkipReason if not.
  /// </summary>
  private async Task CheckDependenciesAsync()
  {
    if (_dependenciesChecked)
    {
      return;
    }

    _dependenciesChecked = true;

    // Get the type of the current test class
    var testClassType = GetType();

    // Get all traits defined on the test class via reflection
    var traits = GetTraitsForTestClass(testClassType);

    // Skip dependency checks for minimal functionality tests
    if (traits.Any(t => t is { Name: TestCategories.Category, Value: TestCategories.MinimalFunctionality }))
    {
      return;
    }

    // Check dependencies in the following order:
    // 1. First try using ApiFeature enum-based dependencies
    // 2. Then check InfrastructureDependency enum-based dependencies
    // 3. Fall back to string-based trait dependencies if no enum-based dependencies are specified

    // Get dependencies specified in DependencyFactAttribute
    var factAttribute = GetDependencyFactAttributeFromTestClass(testClassType);

    if (factAttribute != null)
    {
      // Process ApiFeature dependencies if present
      var requiredFeatures = factAttribute.RequiredExternalServices;
      if (requiredFeatures is { Length: > 0 })
      {
        await CheckExternalServicesDependenciesAsync(requiredFeatures);

        // If any ApiFeature check failed and set a skip reason, don't continue with other checks
        if (ShouldSkip)
        {
          return;
        }
      }

      // Process InfrastructureDependency if present
      var requiredInfrastructure = factAttribute.RequiredInfrastructure;
      if (requiredInfrastructure is { Length: > 0 })
      {
        CheckInfrastructureDependencies(requiredInfrastructure);

        // If any InfrastructureDependency check failed and set a skip reason, don't continue with other checks
        if (ShouldSkip)
        {
          return;
        }
      }

      // If neither enum-based dependency was specified but we have a DependencyFactAttribute,
      // fall back to string-based traits if available
      if ((requiredFeatures == null || requiredFeatures.Length == 0) &&
          (requiredInfrastructure == null || requiredInfrastructure.Length == 0))
      {
        await CheckLegacyTraitDependenciesAsync(traits);
      }
    }
    else
    {
      // No DependencyFactAttribute found, use traits-based approach as fallback
      await CheckLegacyTraitDependenciesAsync(traits);
    }
  }

  /// <summary>
  /// Checks ExternalServices-based dependencies using IStatusService.
  /// This is the preferred approach for API feature dependency checking.
  /// </summary>
  /// <param name="requiredFeatures">Array of required ExternalServices values.</param>
  private Task CheckExternalServicesDependenciesAsync(Zarichney.Services.Status.ExternalServices[] requiredFeatures)
  {
    try
    {
      // Get IStatusService from the factory
      using var scope = Factory.Services.CreateScope();
      var statusService = scope.ServiceProvider.GetRequiredService<IStatusService>();

      // Check each required feature
      var unavailableFeatures = new List<Zarichney.Services.Status.ExternalServices>();
      var allMissingConfigs = new List<string>();

      foreach (var feature in requiredFeatures)
      {
        var featureStatus = statusService.GetFeatureStatus(feature);
        if (featureStatus == null || !featureStatus.IsAvailable)
        {
          unavailableFeatures.Add(feature);
          if (featureStatus != null)
          {
            allMissingConfigs.AddRange(featureStatus.MissingConfigurations);
          }
        }
      }

      // If there are unavailable features, set SkipReason
      if (unavailableFeatures.Count > 0)
      {
        var reason = $"Required ExternalServices unavailable: {string.Join(", ", unavailableFeatures)}";
        if (allMissingConfigs.Count > 0)
        {
          reason += $" (Missing configurations: {string.Join(", ", allMissingConfigs.Distinct())})";
        }

        SetSkipReason(reason);
      }
    }
    catch (Exception ex)
    {
      // If we encounter an error checking ExternalServices dependencies, set SkipReason
      SetSkipReason($"Error checking ExternalServices dependencies: {ex.Message}");
    }

    return Task.CompletedTask;
  }

  /// <summary>
  /// Checks InfrastructureDependency-based dependencies by directly querying the Factory properties.
  /// This is the preferred approach for infrastructure dependency checking.
  /// </summary>
  /// <param name="requiredInfrastructure">Array of required InfrastructureDependency values.</param>
  private void CheckInfrastructureDependencies(InfrastructureDependency[] requiredInfrastructure)
  {
    try
    {
      foreach (var dependency in requiredInfrastructure)
      {
        switch (dependency)
        {
          case InfrastructureDependency.Database:
            if (!Factory.IsDatabaseAvailable)
            {
              SetSkipReason("Database is not available.");
              return;
            }
            break;

          case InfrastructureDependency.Docker:
            if (!Factory.IsDockerAvailable)
            {
              SetSkipReason("Docker is not available or misconfigured.");
              return;
            }
            break;

          default:
            // Unknown infrastructure dependency
            SetSkipReason($"Unknown infrastructure dependency: {dependency}");
            return;
        }
      }
    }
    catch (Exception ex)
    {
      // If we encounter an error checking infrastructure dependencies, set SkipReason
      SetSkipReason($"Error checking infrastructure dependencies: {ex.Message}");
    }
  }

  /// <summary>
  /// Checks string-based trait dependencies using trait values and ConfigurationStatusHelper.
  /// This is the legacy approach for dependency checking, maintained for backward compatibility.
  /// </summary>
  /// <param name="traits">Collection of trait name/value pairs.</param>
  private async Task CheckLegacyTraitDependenciesAsync(List<(string Name, string Value)> traits)
  {
    // Filter the traits to only include dependency traits
    var dependencyTraits = traits
      .Where(t => t.Name == TestCategories.Dependency)
      .ToList();

    // If there are no dependency traits, return (no dependencies to check)
    if (dependencyTraits.Count == 0)
    {
      return;
    }

    // Determine the set of all required configuration item names based on the dependency traits
    HashSet<string> requiredConfigs = [];
    foreach (var (_, value) in dependencyTraits)
    {
      if (TraitToConfigNamesMap.TryGetValue(value, out var configNames))
      {
        foreach (var configName in configNames)
        {
          requiredConfigs.Add(configName);
        }
      }
    }

    // If there are no required configs after mapping, return
    if (requiredConfigs.Count == 0)
    {
      return;
    }

    try
    {
      // Force skip for database-dependent tests if database is not available
      if (requiredConfigs.Contains("Database Connection") && !Factory.IsDatabaseAvailable)
      {
        SetSkipReason("Database is not available.");
        return;
      }

      // Force skip for Docker-dependent tests if Docker is not available
      if (requiredConfigs.Contains("Docker Availability") && !Factory.IsDockerAvailable)
      {
        SetSkipReason("Docker is not available or misconfigured.");
        return;
      }

      try
      {
        // Fetch the service statuses from the new endpoint
        var serviceStatuses = await ConfigurationStatusHelper.GetServiceStatusAsync(Factory);

        // Check if service statuses is null or empty
        if (serviceStatuses.Count == 0)
        {
          SetSkipReason("Unable to fetch service status, assuming dependencies are missing.");
          return;
        }

        // Check each dependency trait to see if its corresponding features are available
        var unavailableDependencies = new List<string>();
        var allMissingConfigs = new List<string>();

        foreach (var (_, dependencyTraitValue) in dependencyTraits)
        {
          // Skip Docker/Database dependencies as they are handled separately
          if (dependencyTraitValue == TestCategories.Docker || dependencyTraitValue == TestCategories.Database)
          {
            continue;
          }

          if (!ConfigurationStatusHelper.IsFeatureDependencyAvailable(serviceStatuses, dependencyTraitValue))
          {
            unavailableDependencies.Add(dependencyTraitValue);

            // Get missing configurations for this dependency
            var missingConfigs =
              ConfigurationStatusHelper.GetMissingConfigurationsForDependency(serviceStatuses, dependencyTraitValue);
            allMissingConfigs.AddRange(missingConfigs);
          }
        }

        // If there are unavailable dependencies, set SkipReason
        if (unavailableDependencies.Count != 0)
        {
          var reason = $"Dependencies unavailable: {string.Join(", ", unavailableDependencies)}";
          if (allMissingConfigs.Count != 0)
          {
            reason += $" (Missing configurations: {string.Join(", ", allMissingConfigs.Distinct())})";
          }

          SetSkipReason(reason);
        }
      }
      catch
      {
        // If we couldn't get configuration status, assume all external service dependencies are unavailable
        var externalDependencies = dependencyTraits
          .Where(t => t.Value.StartsWith("External"))
          .Select(t => t.Value)
          .ToList();

        SetSkipReason(externalDependencies.Count != 0
          ? $"External services unavailable: {string.Join(", ", externalDependencies)}"
          // For tests with no external dependencies, we still need configuration
          : "Configuration status unavailable, skipping test.");
      }
    }
    catch (Exception ex)
    {
      // If we encounter an error checking configuration, assume dependencies are missing
      SetSkipReason($"Error checking dependencies: {ex.Message}");
    }
  }

  /// <summary>
  /// Gets the DependencyFactAttribute from a test class.
  /// </summary>
  /// <param name="testClassType">The test class type to check.</param>
  /// <returns>The DependencyFactAttribute if found, or null if not present.</returns>
  private DependencyFactAttribute? GetDependencyFactAttributeFromTestClass(System.Type testClassType)
  {
    // Check test methods for DependencyFactAttribute
    for (var index = 0; index < testClassType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Length; index++)
    {
      var method = testClassType.GetMethods(BindingFlags.Public | BindingFlags.Instance)[index];
      var factAttr = method.GetCustomAttribute<DependencyFactAttribute>();
      if (factAttr != null)
      {
        return factAttr;
      }
    }

    return null;
  }

  /// <summary>
  /// Helper method to get all traits for a test class using reflection
  /// </summary>
  private List<(string Name, string Value)> GetTraitsForTestClass(System.Type testClassType)
  {
    var traitsList = new List<(string Name, string Value)>();
    var attributes = testClassType.GetCustomAttributes(typeof(TraitAttribute), true);

    foreach (TraitAttribute trait in attributes)
    {
      // Use PropertyInfo to get property values since TraitAttribute doesn't expose properties directly
      var nameProperty = typeof(TraitAttribute).GetProperty("Name");
      var valueProperty = typeof(TraitAttribute).GetProperty("Value");

      if (nameProperty != null && valueProperty != null)
      {
        var name = nameProperty.GetValue(trait)?.ToString();
        var value = valueProperty.GetValue(trait)?.ToString();

        if (name != null && value != null)
        {
          traitsList.Add((name, value));
        }
      }
    }

    return traitsList;
  }

  /// <summary>
  /// Initializes the test by checking if all required configuration dependencies are available.
  /// Sets SkipReason if any required configuration is missing.
  /// </summary>
  /// <returns>A task representing the asynchronous operation.</returns>
  public virtual async Task InitializeAsync()
  {
    // Call CheckDependenciesAsync to ensure SkipReason is set correctly
    await CheckDependenciesAsync();
  }

  /// <summary>
  /// Disposes of resources used by the test.
  /// </summary>
  /// <returns>A completed task.</returns>
  public virtual Task DisposeAsync()
  {
    _testClassContext?.Dispose();
    return Task.CompletedTask;
  }
}
