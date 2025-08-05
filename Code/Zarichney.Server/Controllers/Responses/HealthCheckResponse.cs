namespace Zarichney.Controllers.Responses;

/// <summary>
/// Response model for the health check endpoint.
/// </summary>
/// <param name="Success">Indicates whether the health check was successful.</param>
/// <param name="Time">The current time when the health check was performed.</param>
/// <param name="Environment">The current application environment (e.g., Development, Production).</param>
public record HealthCheckResponse(
    bool Success,
    DateTime Time,
    string Environment
);