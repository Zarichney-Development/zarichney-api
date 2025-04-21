using Microsoft.Extensions.Configuration;
using Zarichney.Tests.Framework.Configuration;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Framework.Helpers;

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
        Testing,
        Production
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
                configValues["Logging:LogLevel:Default"] = "Information";
                configValues["TestSettings:UseInMemoryServices"] = "true";
                configValues["Services:Stripe:UseTestMode"] = "true";
                break;
                
            case TestEnvironment.Testing:
                configValues["Logging:LogLevel:Default"] = "Debug";
                configValues["TestSettings:UseInMemoryServices"] = "false";
                configValues["Services:Stripe:UseTestMode"] = "true";
                break;
                
            case TestEnvironment.Production:
                configValues["Logging:LogLevel:Default"] = "Warning";
                configValues["TestSettings:UseInMemoryServices"] = "true";
                configValues["Services:Stripe:UseTestMode"] = "false";
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