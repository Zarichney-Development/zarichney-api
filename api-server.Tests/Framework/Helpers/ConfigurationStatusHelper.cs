using System.Net.Http.Json;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Framework.Helpers;

/// <summary>
/// Helper for checking the status of configuration items and feature dependencies in the application.
/// </summary>
public static class ConfigurationStatusHelper
{
  // Map from TestCategories dependency traits to feature/service names in the status API
  private static readonly Dictionary<string, string[]> _dependencyToFeatureMap = new()
  {
    { TestCategories.ExternalOpenAI, ["Llm"] },
    { TestCategories.ExternalMSGraph, ["Email"] },
    { TestCategories.ExternalStripe, ["Payment", "Stripe"] },
    { TestCategories.ExternalGitHub, ["GitHub"] },
    { TestCategories.Database, ["Database"] },
    { TestCategories.Docker, ["Docker"] }
    // Add other mappings as needed
  };

  /// <summary>
  /// Gets the service status information from the /api/status endpoint.
  /// </summary>
  /// <param name="factory">The CustomWebApplicationFactory used to create the HTTP client.</param>
  /// <returns>A dictionary mapping service names to their status info, or an empty dictionary if the request fails.</returns>
  public static async Task<Dictionary<string, ServiceStatusInfo>> GetServiceStatusAsync(CustomWebApplicationFactory factory)
  {
    ArgumentNullException.ThrowIfNull(factory, nameof(factory));

    try
    {
      using var client = factory.CreateClient();

      // Set a short timeout to prevent long waits when the service is unavailable
      client.Timeout = TimeSpan.FromSeconds(5);

      var response = await client.GetAsync("/api/status");

      // Check if the request was successful
      if (!response.IsSuccessStatusCode)
      {
        // Return empty dictionary if the request was not successful
        return new Dictionary<string, ServiceStatusInfo>();
      }

      // Deserialize the response
      var statuses = await response.Content.ReadFromJsonAsync<Dictionary<string, ServiceStatusInfo>>();

      if (statuses == null)
      {
        // Return empty dictionary if deserialization failed
        return new Dictionary<string, ServiceStatusInfo>();
      }

      return statuses;
    }
    catch
    {
      // Return empty dictionary if any exception occurs
      return new Dictionary<string, ServiceStatusInfo>();
    }
  }

  /// <summary>
  /// Gets the configuration status from the /api/status/config endpoint.
  /// </summary>
  /// <param name="factory">The CustomWebApplicationFactory used to create the HTTP client.</param>
  /// <returns>A list of configuration item statuses or an empty list if the request fails.</returns>
  public static async Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync(CustomWebApplicationFactory factory)
  {
    ArgumentNullException.ThrowIfNull(factory, nameof(factory));

    try
    {
      using var client = factory.CreateClient();

      // Set a short timeout to prevent long waits when the service is unavailable
      client.Timeout = TimeSpan.FromSeconds(5);

      var response = await client.GetAsync("/api/status/config");

      // Check if the request was successful
      if (!response.IsSuccessStatusCode)
      {
        // Return empty list if the request was not successful
        return new List<ConfigurationItemStatus>();
      }

      // Deserialize the response
      var statuses = await response.Content.ReadFromJsonAsync<List<ConfigurationItemStatus>>();

      if (statuses == null)
      {
        // Return empty list if deserialization failed
        return new List<ConfigurationItemStatus>();
      }

      return statuses;
    }
    catch
    {
      // Return empty list if any exception occurs
      return new List<ConfigurationItemStatus>();
    }
  }

  /// <summary>
  /// Checks if a specific feature dependency is available based on its dependency trait value.
  /// </summary>
  /// <param name="serviceStatuses">The dictionary of service status information.</param>
  /// <param name="dependencyTraitValue">The dependency trait value (e.g., TestCategories.ExternalOpenAI).</param>
  /// <returns>True if the dependency is available; otherwise, false.</returns>
  public static bool IsFeatureDependencyAvailable(Dictionary<string, ServiceStatusInfo> serviceStatuses, string dependencyTraitValue)
  {
    ArgumentNullException.ThrowIfNull(serviceStatuses, nameof(serviceStatuses));
    ArgumentException.ThrowIfNullOrWhiteSpace(dependencyTraitValue, nameof(dependencyTraitValue));

    // Special cases for Docker and Database that might be checked directly in CustomWebApplicationFactory
    if (dependencyTraitValue == TestCategories.Docker)
    {
      // Docker availability might be checked differently, this is just for the mapping
      return true;
    }

    // If the dependency trait doesn't map to any features, consider it available
    if (!_dependencyToFeatureMap.TryGetValue(dependencyTraitValue, out var featureNames) || featureNames.Length == 0)
    {
      return true;
    }

    // Check if all required features are available
    foreach (var featureName in featureNames)
    {
      if (serviceStatuses.TryGetValue(featureName, out var statusInfo))
      {
        if (!statusInfo.IsAvailable)
        {
          return false; // If any required feature is unavailable, the dependency is unavailable
        }
      }
      else
      {
        // If a required feature is not in the status dictionary, consider it unavailable
        return false;
      }
    }

    return true; // All required features are available
  }

  /// <summary>
  /// Gets a list of missing configuration items for a dependency trait value.
  /// </summary>
  /// <param name="serviceStatuses">The dictionary of service status information.</param>
  /// <param name="dependencyTraitValue">The dependency trait value (e.g., TestCategories.ExternalOpenAI).</param>
  /// <returns>A list of missing configuration items, or an empty list if none are missing.</returns>
  public static List<string> GetMissingConfigurationsForDependency(Dictionary<string, ServiceStatusInfo> serviceStatuses, string dependencyTraitValue)
  {
    ArgumentNullException.ThrowIfNull(serviceStatuses, nameof(serviceStatuses));
    ArgumentException.ThrowIfNullOrWhiteSpace(dependencyTraitValue, nameof(dependencyTraitValue));

    var missingConfigs = new List<string>();

    // If the dependency trait doesn't map to any features, return empty list
    if (!_dependencyToFeatureMap.TryGetValue(dependencyTraitValue, out var featureNames) || featureNames.Length == 0)
    {
      return missingConfigs;
    }

    // Collect missing configurations from all required features
    foreach (var featureName in featureNames)
    {
      if (serviceStatuses.TryGetValue(featureName, out var statusInfo) && !statusInfo.IsAvailable)
      {
        missingConfigs.AddRange(statusInfo.MissingConfigurations);
      }
    }

    return missingConfigs;
  }

  /// <summary>
  /// Checks if a specific configuration item is available (configured).
  /// </summary>
  /// <param name="statuses">The list of configuration item statuses.</param>
  /// <param name="configName">The exact name of the configuration item to check.</param>
  /// <returns>True if the configuration item exists and is configured; otherwise, false.</returns>
  public static bool IsConfigurationAvailable(List<ConfigurationItemStatus> statuses, string configName)
  {
    ArgumentNullException.ThrowIfNull(statuses, nameof(statuses));
    ArgumentException.ThrowIfNullOrWhiteSpace(configName, nameof(configName));

    var item = statuses.FirstOrDefault(s => s.Name == configName);
    return item is { Status: "Configured" };
  }
}
