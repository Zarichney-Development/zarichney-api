using Zarichney.Controllers.Responses;

namespace Zarichney.Server.Tests.TestData.Builders;

public class InnerExceptionDetailsBuilder
{
    private string _message = "Inner exception message";
    private string _type = "InnerException";
    private IReadOnlyList<string>? _stackTrace = null;

    public InnerExceptionDetailsBuilder WithMessage(string message)
    {
        _message = message;
        return this;
    }

    public InnerExceptionDetailsBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public InnerExceptionDetailsBuilder WithStackTrace(IReadOnlyList<string>? stackTrace)
    {
        _stackTrace = stackTrace;
        return this;
    }

    public InnerExceptionDetailsBuilder WithDefaults()
    {
        _message = "Database connection failed";
        _type = typeof(InvalidOperationException).Name;
        _stackTrace = new List<string> { "at DataLayer.Connect()", "at Service.Execute()" };
        return this;
    }

    public InnerExceptionDetailsBuilder WithTimeoutException()
    {
        _message = "The operation has timed out";
        _type = typeof(TimeoutException).Name;
        _stackTrace = new List<string> { "at HttpClient.SendAsync()", "at ApiClient.Request()" };
        return this;
    }

    public InnerExceptionDetailsBuilder WithNullReferenceException()
    {
        _message = "Object reference not set to an instance of an object";
        _type = typeof(NullReferenceException).Name;
        _stackTrace = new List<string> { "at Object.Method()" };
        return this;
    }

    public InnerExceptionDetails Build()
    {
        return new InnerExceptionDetails(_message, _type, _stackTrace);
    }
}