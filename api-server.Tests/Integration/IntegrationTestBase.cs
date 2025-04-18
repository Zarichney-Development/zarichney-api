using System.Net.Http.Headers;
using Zarichney.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Framework.Helpers;

namespace Zarichney.Tests.Integration;

/// <summary>
/// Base class for integration tests that provides common setup and accessors.
/// Implements IAsyncLifetime to check for required configuration dependencies based on test traits.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private static readonly Dictionary<string, List<string>> _traitToConfigNamesMap = new()
    {
        { TestCategories.Database, ["Database Connection"] },
        { TestCategories.ExternalStripe, ["Stripe Secret Key", "Stripe Webhook Secret"] },
        { TestCategories.ExternalOpenAI, ["OpenAI API Key"] },
        { TestCategories.ExternalGitHub, ["GitHub Access Token"] },
        { TestCategories.ExternalMSGraph, [
                "Email AzureTenantId",
                "Email AzureAppId",
                "Email AzureAppSecret",
                "Email FromEmail",
                "Email MailCheckApiKey"
            ]
        }
    };

    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    
    private bool _dependenciesChecked;
    private string? SkipReason { get; set; }
    
    /// <summary>
    /// Sets a skip reason. This is used internally and by derived classes.
    /// </summary>
    private void SetSkipReason(string reason)
    {
        SkipReason = reason;
    }
    
    /// <summary>
    /// Checks if the test should be skipped due to missing dependencies.
    /// </summary>
    public bool ShouldSkip => !string.IsNullOrEmpty(SkipReason);

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTestBase"/> class.
    /// </summary>
    /// <param name="factory">The web application factory.</param>
    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        
        // Set a forced skip reason if database is not available
        if (!factory.IsDatabaseAvailable)
        {
            SetSkipReason("Database is not available.");
        }
    }
    
    /// <summary>
    /// Creates an authenticated API client for the specified user.
    /// </summary>
    /// <param name="userId">The user ID to authenticate as.</param>
    /// <param name="roles">The roles to assign to the user.</param>
    /// <returns>An API client configured with authentication.</returns>
    protected IZarichneyAPI CreateAuthenticatedApiClient(string userId, string[] roles)
    {
        var httpClient = Factory.CreateAuthenticatedClient(userId, roles);
        return Factory.CreateRefitClient(httpClient);
    }
    
    /// <summary>
    /// Creates an authenticated client with a bearer token.
    /// </summary>
    /// <param name="token">The bearer token to use.</param>
    /// <returns>An HttpClient configured with authentication.</returns>
    protected HttpClient CreateClientWithBearerToken(string token)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
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
        
        // Check if this is a minimal functionality test (which should run even without config)
        if (traits.Any(t => t is { Name: TestCategories.Category, Value: TestCategories.MinimalFunctionality }))
        {
            // Skip dependency checks for minimal functionality tests
            return;
        }
        
        // Filter the traits to only include dependency traits
        var dependencyTraits = traits
            .Where(t => t.Name == TestCategories.Dependency)
            .ToList();
        
        // If there are no dependency traits, return (no dependencies to check)
        if (!dependencyTraits.Any())
        {
            return;
        }
        
        // Determine the set of all required configuration item names based on the dependency traits
        var requiredConfigs = new HashSet<string>();
        foreach (var (_, value) in dependencyTraits)
        {
            if (_traitToConfigNamesMap.TryGetValue(value, out var configNames))
            {
                foreach (var configName in configNames)
                {
                    requiredConfigs.Add(configName);
                }
            }
        }
        
        // If there are no required configs after mapping, return
        if (!requiredConfigs.Any())
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
            
            // Fetch the actual configuration status
            var statuses = await ConfigurationStatusHelper.GetConfigurationStatusAsync(Factory);
        
            // Check if statuses is null or empty
            if (statuses.Count == 0)
            {
                SetSkipReason("Unable to fetch configuration status.");
                return;
            }
        
            // Check if each required config is available
            var missingConfigs = new List<string>();
            foreach (var configName in requiredConfigs)
            {
                bool isAvailable = ConfigurationStatusHelper.IsConfigurationAvailable(statuses, configName);
                
                if (!isAvailable)
                {
                    missingConfigs.Add(configName);
                }
            }
        
            // If there are missing configs, set SkipReason
            if (missingConfigs.Any())
            {
                SetSkipReason($"Missing required configurations: {string.Join(", ", missingConfigs)}");
            }
        }
        catch (Exception ex)
        {
            // If we encounter an error checking configuration, assume dependencies are missing
            SetSkipReason($"Error checking configuration: {ex.Message}");
        }
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
    public async Task InitializeAsync()
    {
        // Call CheckDependenciesAsync to ensure SkipReason is set correctly
        await CheckDependenciesAsync();
    }

    /// <summary>
    /// Disposes of resources used by the test.
    /// </summary>
    /// <returns>A completed task.</returns>
    public Task DisposeAsync() => Task.CompletedTask;
}
