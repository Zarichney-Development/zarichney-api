namespace Zarichney.Controllers.Responses;

/// <summary>
/// Request context information for error responses
/// </summary>
/// <param name="Path">Request path</param>
/// <param name="Method">HTTP method</param>
/// <param name="Controller">Controller action identifier</param>
public record RequestDetails(
    string? Path,
    string Method,
    string? Controller
);
