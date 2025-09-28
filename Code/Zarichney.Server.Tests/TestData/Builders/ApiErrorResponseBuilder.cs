using Zarichney.Controllers.Responses;

namespace Zarichney.Server.Tests.TestData.Builders;

/// <summary>
/// Builder pattern implementation for creating ApiErrorResponse objects in tests.
/// Provides fluent interface for constructing error responses with various configurations
/// including default values, server errors, and validation errors.
/// </summary>
/// <example>
/// Basic usage:
/// <code>
/// var errorResponse = new ApiErrorResponseBuilder()
///     .WithDefaults()
///     .Build();
/// </code>
/// 
/// Custom error configuration:
/// <code>
/// var validationError = new ApiErrorResponseBuilder()
///     .WithValidationError()
///     .WithTraceId("custom-trace-123")
///     .Build();
/// </code>
/// </example>
public class ApiErrorResponseBuilder
{
    private ErrorDetails _error = new ErrorDetailsBuilder().Build();
    private RequestDetails _request = new RequestDetailsBuilder().Build();
    private string _traceId = Guid.NewGuid().ToString();
    private InnerExceptionDetails? _innerException = null;

    /// <summary>
    /// Sets the error details for the API error response.
    /// </summary>
    /// <param name="error">The error details to include in the response</param>
    /// <returns>The builder instance for method chaining</returns>
    public ApiErrorResponseBuilder WithError(ErrorDetails error)
    {
        _error = error;
        return this;
    }

    /// <summary>
    /// Sets the request details for the API error response.
    /// </summary>
    /// <param name="request">The request details to include in the response</param>
    /// <returns>The builder instance for method chaining</returns>
    public ApiErrorResponseBuilder WithRequest(RequestDetails request)
    {
        _request = request;
        return this;
    }

    /// <summary>
    /// Sets the trace ID for the API error response.
    /// </summary>
    /// <param name="traceId">The trace ID to include in the response for correlation</param>
    /// <returns>The builder instance for method chaining</returns>
    public ApiErrorResponseBuilder WithTraceId(string traceId)
    {
        _traceId = traceId;
        return this;
    }

    /// <summary>
    /// Sets the inner exception details for the API error response.
    /// </summary>
    /// <param name="innerException">The inner exception details to include in the response</param>
    /// <returns>The builder instance for method chaining</returns>
    public ApiErrorResponseBuilder WithInnerException(InnerExceptionDetails innerException)
    {
        _innerException = innerException;
        return this;
    }

    /// <summary>
    /// Configures the builder with default test values for all properties.
    /// Sets up standard error details, request details, and a predictable trace ID.
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    public ApiErrorResponseBuilder WithDefaults()
    {
        _error = new ErrorDetailsBuilder().WithDefaults().Build();
        _request = new RequestDetailsBuilder().WithDefaults().Build();
        _traceId = "test-trace-id";
        return this;
    }

    /// <summary>
    /// Configures the builder to represent a server error (HTTP 500) scenario.
    /// Sets up error details with an internal server error message and InvalidOperationException type.
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    public ApiErrorResponseBuilder WithServerError()
    {
        _error = new ErrorDetailsBuilder()
            .WithMessage("Internal server error occurred")
            .WithType(typeof(InvalidOperationException).Name)
            .WithDetails("An unexpected error occurred while processing the request")
            .Build();
        return this;
    }

    /// <summary>
    /// Configures the builder to represent a validation error (HTTP 400) scenario.
    /// Sets up error details with a validation failure message and ArgumentException type.
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    public ApiErrorResponseBuilder WithValidationError()
    {
        _error = new ErrorDetailsBuilder()
            .WithMessage("Validation failed")
            .WithType(typeof(ArgumentException).Name)
            .WithDetails("One or more validation errors occurred")
            .Build();
        return this;
    }

    /// <summary>
    /// Builds and returns the configured ApiErrorResponse instance.
    /// </summary>
    /// <returns>A new ApiErrorResponse with the configured properties</returns>
    public ApiErrorResponse Build()
    {
        return new ApiErrorResponse(_error, _request, _traceId, _innerException);
    }
}