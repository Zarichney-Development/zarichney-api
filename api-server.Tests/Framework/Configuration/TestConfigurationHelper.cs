using Microsoft.Extensions.Configuration;

namespace Zarichney.Tests.Framework.Configuration;

/// <summary>
/// Helper for loading and managing test configuration.
/// Provides methods for creating test-specific IConfiguration instances.
/// </summary>
public static class TestConfigurationHelper
{
    /// <summary>
    /// Creates a test configuration with default test settings.
    /// </summary>
    /// <returns>An IConfiguration instance with test settings.</returns>
    public static IConfiguration CreateTestConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestSettings:IsTestEnvironment"] = "true",
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=localhost;Database=zarichney_test;User Id=postgres;Password=postgres;",
                // Add other test-specific configuration values here
            })
            .Build();
    }

    /// <summary>
    /// Creates a test configuration with custom settings.
    /// </summary>
    /// <param name="configValues">Dictionary of configuration key-value pairs to include.</param>
    /// <returns>An IConfiguration instance with the specified settings.</returns>
    public static IConfiguration CreateTestConfiguration(Dictionary<string, string?> configValues)
    {
        // Start with default test settings
        var settings = new Dictionary<string, string?>
        {
            ["TestSettings:IsTestEnvironment"] = "true",
            ["ConnectionStrings:DefaultConnection"] =
                "Server=localhost;Database=zarichney_test;User Id=postgres;Password=postgres;"
        };

        // Add or override with custom settings
        foreach (var kvp in configValues)
        {
            settings[kvp.Key] = kvp.Value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    /// <summary>
    /// Updates a connection string with values from a DatabaseFixture.
    /// </summary>
    /// <param name="configuration">The configuration to update.</param>
    /// <param name="connectionString">The new connection string from the DatabaseFixture.</param>
    /// <returns>A new IConfiguration instance with the updated connection string.</returns>
    public static IConfiguration UpdateConnectionString(IConfiguration configuration, string connectionString)
    {
        var configValues = configuration.AsEnumerable()
            .ToDictionary(c => c.Key, c => c.Value);
        configValues["ConnectionStrings:DefaultConnection"] = connectionString;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
    }

    /// <summary>
    /// Gets the test configuration from a JSON file.
    /// </summary>
    /// <returns>An IConfiguration instance loaded from appsettings.Testing.json.</returns>
    public static IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Testing.json", optional: false)
            .Build();
    }
}