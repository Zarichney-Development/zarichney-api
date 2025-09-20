namespace Zarichney.Controllers.Responses;

/// <summary>
/// Inner exception details for nested error information
/// </summary>
/// <param name="Message">Inner exception message</param>
/// <param name="Type">Inner exception type name</param>
/// <param name="StackTrace">Formatted inner exception stack trace lines</param>
public record InnerExceptionDetails(
    string Message,
    string Type,
    IReadOnlyList<string>? StackTrace = null
);
