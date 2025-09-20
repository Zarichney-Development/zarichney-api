namespace Zarichney.Controllers.Responses;

/// <summary>
/// Detailed error information for API responses
/// </summary>
/// <param name="Message">User-friendly error message</param>
/// <param name="Type">Exception type name (nullable for non-exception errors)</param>
/// <param name="Details">Detailed error message (nullable for non-exception errors)</param>
/// <param name="Source">Exception source (nullable for non-exception errors)</param>
/// <param name="StackTrace">Formatted stack trace lines (nullable)</param>
public record ErrorDetails(
    string Message,
    string? Type = null,
    string? Details = null,
    string? Source = null,
    IReadOnlyList<string>? StackTrace = null
);
