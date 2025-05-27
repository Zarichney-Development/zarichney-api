using Zarichney.Services.Status;

namespace Zarichney.TestingFramework.Helpers;

/// <summary>
/// Factory methods for creating test objects with appropriate defaults
/// to make tests more maintainable and less brittle.
/// </summary>
public static class TestFactories
{
  /// <summary>
  /// Creates a ServiceStatusInfo with sensible defaults for testing.
  /// This isolates tests from constructor parameter changes.
  /// </summary>
  /// <param name="serviceName">The service name (defaults to OpenAiApi)</param>
  /// <param name="isAvailable">Whether the service is available (defaults to true)</param>
  /// <param name="missingConfigurations">Missing configurations (defaults to empty list)</param>
  /// <returns>A ServiceStatusInfo instance</returns>
  public static ServiceStatusInfo CreateServiceStatus(
      ExternalServices serviceName = ExternalServices.OpenAiApi,
      bool isAvailable = true,
      List<string>? missingConfigurations = null)
  {
    return new ServiceStatusInfo(
        serviceName: serviceName,
        IsAvailable: isAvailable,
        MissingConfigurations: missingConfigurations ?? []
    );
  }

  /// <summary>
  /// Creates a ConfigurationItemStatus with sensible defaults for testing.
  /// </summary>
  /// <param name="name">The configuration item name (defaults to "Test Configuration")</param>
  /// <param name="status">The status (defaults to "Configured")</param>
  /// <param name="details">Optional details about the configuration item</param>
  /// <returns>A ConfigurationItemStatus instance</returns>
  public static ConfigurationItemStatus CreateConfigurationStatus(
      string name = "Test Configuration",
      string status = "Configured",
      string? details = null)
  {
    return new ConfigurationItemStatus(
        Name: name,
        Status: status,
        Details: details
    );
  }

  /// <summary>
  /// Creates a list of ServiceStatusInfo objects for testing the API response format.
  /// </summary>
  /// <param name="count">Number of status items to create (defaults to 3)</param>
  /// <param name="allAvailable">Whether all services should be available (defaults to true)</param>
  /// <returns>A list of ServiceStatusInfo instances</returns>
  public static List<ServiceStatusInfo> CreateServiceStatusList(
      int count = 3,
      bool allAvailable = true)
  {
    var result = new List<ServiceStatusInfo>();
    var services = new[]
    {
            ExternalServices.OpenAiApi,
            ExternalServices.GitHubAccess,
            ExternalServices.Stripe,
            ExternalServices.MsGraph,
            ExternalServices.MailCheck
        };

    for (int i = 0; i < count && i < services.Length; i++)
    {
      result.Add(CreateServiceStatus(
          serviceName: services[i],
          isAvailable: allAvailable,
          missingConfigurations: allAvailable ? [] : [$"TestConfig{i}:ApiKey"]
      ));
    }

    return result;
  }

  /// <summary>
  /// Creates a dictionary mapping ExternalServices to ServiceStatusInfo objects for testing.
  /// </summary>
  /// <param name="count">Number of status items to create (defaults to 3)</param>
  /// <param name="allAvailable">Whether all services should be available (defaults to true)</param>
  /// <returns>A dictionary of ExternalServices to ServiceStatusInfo instances</returns>
  public static Dictionary<ExternalServices, ServiceStatusInfo> CreateServiceStatusDictionary(
      int count = 3,
      bool allAvailable = true)
  {
    var result = new Dictionary<ExternalServices, ServiceStatusInfo>();
    var services = new[]
    {
            ExternalServices.OpenAiApi,
            ExternalServices.GitHubAccess,
            ExternalServices.Stripe,
            ExternalServices.MsGraph,
            ExternalServices.MailCheck
        };

    for (int i = 0; i < count && i < services.Length; i++)
    {
      result[services[i]] = CreateServiceStatus(
          serviceName: services[i],
          isAvailable: allAvailable,
          missingConfigurations: allAvailable ? [] : [$"TestConfig{i}:ApiKey"]
      );
    }

    return result;
  }
}
