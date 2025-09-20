namespace Zarichney.Controllers.Responses;

/// <summary>
/// Structured error response model for API error handling
/// </summary>
/// <param name="Error">Detailed error information</param>
/// <param name="Request">Request context information</param>
/// <param name="TraceId">Unique trace identifier for debugging</param>
/// <param name="InnerException">Optional inner exception details</param>
public record ApiErrorResponse(
    ErrorDetails Error,
    RequestDetails Request,
    string TraceId,
    InnerExceptionDetails? InnerException = null
);
