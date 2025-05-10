namespace Zarichney.Services.Status;

/// <summary>
/// Represents the status of a service based on its configuration availability.
/// </summary>
/// <param name="IsAvailable">Whether the service is available based on its configuration.</param>
/// <param name="MissingConfigurations">A list of missing configuration keys if the service is unavailable.</param>
public record ServiceStatusInfo(bool IsAvailable, List<string> MissingConfigurations);

public record ConfigurationItemStatus(
  string Name,
  string Status,
  string? Details = null
);
