using System.Net.Http.Json;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Framework.Helpers;

/// <summary>
/// Helper for checking the status of configuration items in the application.
/// </summary>
public static class ConfigurationStatusHelper
{
    /// <summary>
    /// Gets the configuration status from the /api/status/config endpoint.
    /// </summary>
    /// <param name="factory">The CustomWebApplicationFactory used to create the HTTP client.</param>
    /// <returns>A list of configuration item statuses.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the request fails or returns null.</exception>
    public static async Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync(CustomWebApplicationFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/status/config");
        
        // Ensure the request was successful
        response.EnsureSuccessStatusCode();
        
        // Deserialize the response
        var statuses = await response.Content.ReadFromJsonAsync<List<ConfigurationItemStatus>>();
        
        if (statuses == null)
        {
            throw new InvalidOperationException("Failed to deserialize configuration status response");
        }
        
        return statuses;
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
