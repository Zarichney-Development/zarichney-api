using Microsoft.Extensions.Configuration;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
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
  /// Enumeration of test environment classifications for skip threshold evaluation.
  /// </summary>
  public enum TestEnvironmentClassification
  {
    /// <summary>
    /// Environment with no external services configured (26.7% skip rate expected).
    /// </summary>
    Unconfigured,
    /// <summary>
    /// Environment with all external services available (1.2% skip rate expected).
    /// </summary>
    Configured,
    /// <summary>
    /// Production environment where no test failures are acceptable (0% skip rate).
    /// </summary>
    Production
  }

  /// <summary>
  /// Detailed information about the current test environment classification.
  /// </summary>
  public record TestEnvironmentInfo(
    TestEnvironmentClassification Classification,
    double ExpectedSkipPercentage,
    string Description,
    Dictionary<string, bool> ServiceAvailability,
    Dictionary<string, bool> InfrastructureAvailability,
    List<string> MissingConfigurations);

  /// <summary>
  /// Detects and classifies the current test environment based on service and infrastructure availability.
  /// </summary>
  /// <param name="factory">The CustomWebApplicationFactory used to check service availability.</param>
  /// <param name="databaseFixture">Optional DatabaseFixture to check database availability.</param>
  /// <returns>Detailed information about the test environment classification.</returns>
  public static async Task<TestEnvironmentInfo> DetectCurrentTestEnvironmentAsync(
    CustomWebApplicationFactory factory,
    DatabaseFixture? databaseFixture = null)
  {
    ArgumentNullException.ThrowIfNull(factory, nameof(factory));

    // Check if running in production environment
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var isProduction = string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase);

    if (isProduction)
    {
      return new TestEnvironmentInfo(
        TestEnvironmentClassification.Production,
        0.0,
        "Production deployment validation - no test failures acceptable",
        new Dictionary<string, bool>(),
        new Dictionary<string, bool>(),
        new List<string>());
    }

    // Get service status information
    var serviceStatuses = await ConfigurationStatusHelper.GetServiceStatusAsync(factory);
    
    // Check external service availability
    var externalServices = new Dictionary<string, bool>
    {
      ["OpenAI"] = IsServiceAvailable(serviceStatuses, "Llm"),
      ["Stripe"] = IsServiceAvailable(serviceStatuses, "Payment") || IsServiceAvailable(serviceStatuses, "Stripe"),
      ["MsGraph"] = IsServiceAvailable(serviceStatuses, "Email"),
      ["GitHub"] = IsServiceAvailable(serviceStatuses, "GitHub")
    };

    // Check infrastructure availability
    var infrastructure = new Dictionary<string, bool>
    {
      ["Database"] = databaseFixture?.IsContainerAvailable ?? false,
      ["Docker"] = CheckDockerAvailability()
    };

    // Collect missing configurations
    var missingConfigurations = new List<string>();
    foreach (var service in externalServices.Where(s => !s.Value))
    {
      var missingForService = ConfigurationStatusHelper.GetMissingConfigurationsForDependency(
        serviceStatuses, GetTestCategoryForService(service.Key));
      missingConfigurations.AddRange(missingForService);
    }

    // Determine classification based on availability
    var externalServicesAvailable = externalServices.Values.Count(available => available);
    var totalExternalServices = externalServices.Count;
    var infrastructureAvailable = infrastructure.Values.All(available => available);

    // Classification logic
    if (externalServicesAvailable == totalExternalServices && infrastructureAvailable)
    {
      return new TestEnvironmentInfo(
        TestEnvironmentClassification.Configured,
        1.2,
        "Full external service configuration - comprehensive test coverage expected",
        externalServices,
        infrastructure,
        missingConfigurations);
    }
    else
    {
      return new TestEnvironmentInfo(
        TestEnvironmentClassification.Unconfigured,
        26.7,
        "Local dev / CI without external services - high skip rate expected",
        externalServices,
        infrastructure,
        missingConfigurations);
    }
  }

  /// <summary>
  /// Gets the current test environment classification synchronously.
  /// This is a simplified version that doesn't require async service status checks.
  /// </summary>
  /// <returns>The test environment classification based on environment variables.</returns>
  public static TestEnvironmentClassification GetCurrentTestEnvironmentClassification()
  {
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    
    return string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase)
      ? TestEnvironmentClassification.Production
      : TestEnvironmentClassification.Unconfigured; // Default to unconfigured for safety
  }

  /// <summary>
  /// Creates a configuration for the specified test environment.
  /// </summary>
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

  /// <summary>
  /// Checks if a service is available in the service status dictionary.
  /// </summary>
  /// <param name="serviceStatuses">Dictionary of service statuses.</param>
  /// <param name="serviceName">Name of the service to check.</param>
  /// <returns>True if the service is available, false otherwise.</returns>
  private static bool IsServiceAvailable(Dictionary<string, ServiceStatusInfo> serviceStatuses, string serviceName)
  {
    return serviceStatuses.TryGetValue(serviceName, out var status) && status.IsAvailable;
  }

  /// <summary>
  /// Maps service names to their corresponding test category constants.
  /// </summary>
  /// <param name="serviceName">The service name.</param>
  /// <returns>The corresponding test category constant.</returns>
  private static string GetTestCategoryForService(string serviceName)
  {
    return serviceName switch
    {
      "OpenAI" => TestCategories.ExternalOpenAI,
      "Stripe" => TestCategories.ExternalStripe,
      "MsGraph" => TestCategories.ExternalMSGraph,
      "GitHub" => TestCategories.ExternalGitHub,
      _ => string.Empty
    };
  }

  /// <summary>
  /// Checks if Docker is available in the current environment.
  /// </summary>
  /// <returns>True if Docker is available, false otherwise.</returns>
  private static bool CheckDockerAvailability()
  {
    try
    {
      using var process = new System.Diagnostics.Process();
      process.StartInfo.FileName = "docker";
      process.StartInfo.Arguments = "info";
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.CreateNoWindow = true;
      
      process.Start();
      process.WaitForExit(5000); // 5 second timeout
      
      return process.ExitCode == 0;
    }
    catch
    {
      return false;
    }
  }
}
