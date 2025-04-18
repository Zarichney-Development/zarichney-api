using Microsoft.Extensions.Configuration;
using Zarichney.Tests.Configuration;
using Zarichney.Tests.Fixtures;

namespace Zarichney.Tests.Helpers;

/// <summary>
/// Helper for managing test environments.
/// Provides methods for creating environment-specific test configurations.
/// </summary>
public static class TestEnvironmentHelper
{
    /// <summary>
    /// Enumeration of supported test environments.
    /// </summary>
    public enum TestEnvironment
    {
        Development,
        Staging,
        CI
    }
    
    /// <summary>
    /// Creates a configuration for the specified test environment.
    /// </summary>
    /// <param name="environment">The test environment.</param>
    /// <param name="databaseFixture">Optional database fixture for connection string.</param>
    /// <returns>An IConfiguration for the specified environment.</returns>
    public static IConfiguration CreateEnvironmentConfiguration(
        TestEnvironment environment,
        DatabaseFixture? databaseFixture = null)
    {
        // Start with default test settings
        var configValues = new Dictionary<string, string?>
        {
            ["TestSettings:IsTestEnvironment"] = "true",
            ["TestSettings:Environment"] = environment.ToString()
        };

        // Add environment-specific settings
        switch (environment)
        {
            case TestEnvironment.Development:
                configValues["Logging:LogLevel:Default"] = "Debug";
                configValues["TestSettings:UseInMemoryServices"] = "true";
                configValues["Services:Stripe:UseTestMode"] = "true";
                break;
                
            case TestEnvironment.Staging:
                configValues["Logging:LogLevel:Default"] = "Information";
                configValues["TestSettings:UseInMemoryServices"] = "false";
                configValues["Services:Stripe:UseTestMode"] = "true";
                break;
                
            case TestEnvironment.CI:
                configValues["Logging:LogLevel:Default"] = "Warning";
                configValues["TestSettings:UseInMemoryServices"] = "true";
                configValues["Services:Stripe:UseTestMode"] = "true";
                break;
        }
        
        // Create the configuration
        var configuration = TestConfigurationHelper.CreateTestConfiguration(configValues);
        
        // Update the connection string if a database fixture is provided
        if (databaseFixture != null)
        {
            configuration = TestConfigurationHelper.UpdateConnectionString(
                configuration, databaseFixture.ConnectionString);
        }
        
        return configuration;
    }
}