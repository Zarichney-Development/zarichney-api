namespace Zarichney.Server.Services.Status;

public record ConfigurationItemStatus(
    string Name,
    string Status,
    string? Details = null
);
