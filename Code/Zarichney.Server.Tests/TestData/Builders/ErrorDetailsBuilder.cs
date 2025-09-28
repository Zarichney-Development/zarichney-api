using Zarichney.Controllers.Responses;

namespace Zarichney.Server.Tests.TestData.Builders;

public class ErrorDetailsBuilder
{
  private string _message = "An error occurred";
  private string? _type = null;
  private string? _details = null;
  private string? _source = null;
  private IReadOnlyList<string>? _stackTrace = null;

  public ErrorDetailsBuilder WithMessage(string message)
  {
    _message = message;
    return this;
  }

  public ErrorDetailsBuilder WithType(string? type)
  {
    _type = type;
    return this;
  }

  public ErrorDetailsBuilder WithDetails(string? details)
  {
    _details = details;
    return this;
  }

  public ErrorDetailsBuilder WithSource(string? source)
  {
    _source = source;
    return this;
  }

  public ErrorDetailsBuilder WithStackTrace(IReadOnlyList<string>? stackTrace)
  {
    _stackTrace = stackTrace;
    return this;
  }

  public ErrorDetailsBuilder WithDefaults()
  {
    _message = "Test error message";
    _type = "TestException";
    _details = "Test error details";
    _source = "TestSource";
    _stackTrace = new List<string> { "at TestMethod() line 1", "at TestCaller() line 2" };
    return this;
  }

  public ErrorDetailsBuilder WithValidationError()
  {
    _message = "Validation failed";
    _type = typeof(ArgumentException).Name;
    _details = "The provided value is invalid";
    _source = "ValidationService";
    return this;
  }

  public ErrorDetailsBuilder WithNullReferenceError()
  {
    _message = "Object reference not set to an instance of an object";
    _type = typeof(NullReferenceException).Name;
    _details = "A null reference was encountered";
    _source = "System";
    _stackTrace = new List<string> { "at Object.Method()" };
    return this;
  }

  public ErrorDetailsBuilder WithMinimalInfo()
  {
    _message = "Error occurred";
    _type = null;
    _details = null;
    _source = null;
    _stackTrace = null;
    return this;
  }

  public ErrorDetails Build()
  {
    return new ErrorDetails(_message, _type, _details, _source, _stackTrace);
  }
}
