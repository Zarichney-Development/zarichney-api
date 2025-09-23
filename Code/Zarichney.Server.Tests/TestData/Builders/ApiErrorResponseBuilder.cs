using Zarichney.Controllers.Responses;

namespace Zarichney.Server.Tests.TestData.Builders;

public class ApiErrorResponseBuilder
{
    private ErrorDetails _error = new ErrorDetailsBuilder().Build();
    private RequestDetails _request = new RequestDetailsBuilder().Build();
    private string _traceId = Guid.NewGuid().ToString();
    private InnerExceptionDetails? _innerException = null;

    public ApiErrorResponseBuilder WithError(ErrorDetails error)
    {
        _error = error;
        return this;
    }

    public ApiErrorResponseBuilder WithRequest(RequestDetails request)
    {
        _request = request;
        return this;
    }

    public ApiErrorResponseBuilder WithTraceId(string traceId)
    {
        _traceId = traceId;
        return this;
    }

    public ApiErrorResponseBuilder WithInnerException(InnerExceptionDetails innerException)
    {
        _innerException = innerException;
        return this;
    }

    public ApiErrorResponseBuilder WithDefaults()
    {
        _error = new ErrorDetailsBuilder().WithDefaults().Build();
        _request = new RequestDetailsBuilder().WithDefaults().Build();
        _traceId = "test-trace-id";
        return this;
    }

    public ApiErrorResponseBuilder WithServerError()
    {
        _error = new ErrorDetailsBuilder()
            .WithMessage("Internal server error occurred")
            .WithType(typeof(InvalidOperationException).Name)
            .WithDetails("An unexpected error occurred while processing the request")
            .Build();
        return this;
    }

    public ApiErrorResponseBuilder WithValidationError()
    {
        _error = new ErrorDetailsBuilder()
            .WithMessage("Validation failed")
            .WithType(typeof(ArgumentException).Name)
            .WithDetails("One or more validation errors occurred")
            .Build();
        return this;
    }

    public ApiErrorResponse Build()
    {
        return new ApiErrorResponse(_error, _request, _traceId, _innerException);
    }
}