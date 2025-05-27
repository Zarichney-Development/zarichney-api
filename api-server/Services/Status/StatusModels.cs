namespace Zarichney.Services.Status;

/// <summary>
/// Represents the status of a service based on its configuration availability.
/// </summary>
/// <param name="serviceName">The name of the service.</param>
/// <param name="IsAvailable">Whether the service is available based on its configuration.</param>
/// <param name="MissingConfigurations">A list of missing configuration keys if the service is unavailable.</param>
public record ServiceStatusInfo(ExternalServices serviceName, bool IsAvailable, List<string> MissingConfigurations);

/// <summary>
///
/// </summary>
/// <param name="Name"></param>
/// <param name="Status"></param>
/// <param name="Details"></param>
public record ConfigurationItemStatus(
  string Name,
  string Status,
  string? Details = null
);
