using System.Net;
using Zarichney.Controllers;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Test data builder for ApiErrorResult - provides fluent API for creating test error results
/// </summary>
public class ApiErrorResultBuilder
{
    private Exception? _exception;
    private string _userMessage = "An unexpected error occurred";
    private HttpStatusCode _statusCode = HttpStatusCode.InternalServerError;

    public ApiErrorResultBuilder WithException(Exception exception)
    {
        _exception = exception;
        return this;
    }

    public ApiErrorResultBuilder WithException(string message)
    {
        _exception = new Exception(message);
        return this;
    }

    public ApiErrorResultBuilder WithInnerException(Exception outerException, Exception innerException)
    {
        _exception = new Exception(outerException.Message, innerException);
        return this;
    }

    public ApiErrorResultBuilder WithInvalidOperationException(string message)
    {
        _exception = new InvalidOperationException(message);
        return this;
    }

    public ApiErrorResultBuilder WithArgumentException(string message, string paramName = "parameter")
    {
        _exception = new ArgumentException(message, paramName);
        return this;
    }

    public ApiErrorResultBuilder WithNullReferenceException(string message)
    {
        _exception = new NullReferenceException(message);
        return this;
    }

    public ApiErrorResultBuilder WithUserMessage(string userMessage)
    {
        _userMessage = userMessage;
        return this;
    }

    public ApiErrorResultBuilder WithStatusCode(HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
        return this;
    }

    public ApiErrorResultBuilder WithBadRequest(string userMessage = "Bad request")
    {
        _statusCode = HttpStatusCode.BadRequest;
        _userMessage = userMessage;
        return this;
    }

    public ApiErrorResultBuilder WithNotFound(string userMessage = "Resource not found")
    {
        _statusCode = HttpStatusCode.NotFound;
        _userMessage = userMessage;
        return this;
    }

    public ApiErrorResultBuilder WithUnauthorized(string userMessage = "Unauthorized access")
    {
        _statusCode = HttpStatusCode.Unauthorized;
        _userMessage = userMessage;
        return this;
    }

    public ApiErrorResultBuilder WithForbidden(string userMessage = "Access forbidden")
    {
        _statusCode = HttpStatusCode.Forbidden;
        _userMessage = userMessage;
        return this;
    }

    public ApiErrorResultBuilder WithInternalServerError(string userMessage = "Internal server error")
    {
        _statusCode = HttpStatusCode.InternalServerError;
        _userMessage = userMessage;
        return this;
    }

    public ApiErrorResultBuilder WithDefaults()
    {
        _exception = null;
        _userMessage = "An unexpected error occurred";
        _statusCode = HttpStatusCode.InternalServerError;
        return this;
    }

    public ApiErrorResult Build()
    {
        return new ApiErrorResult(_exception, _userMessage, _statusCode);
    }
}