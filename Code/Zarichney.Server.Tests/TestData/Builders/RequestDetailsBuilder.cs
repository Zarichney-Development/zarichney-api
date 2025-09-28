using Zarichney.Controllers.Responses;

namespace Zarichney.Server.Tests.TestData.Builders;

public class RequestDetailsBuilder
{
  private string? _path = "/api/test";
  private string _method = "GET";
  private string? _controller = null;

  public RequestDetailsBuilder WithPath(string? path)
  {
    _path = path;
    return this;
  }

  public RequestDetailsBuilder WithMethod(string method)
  {
    _method = method;
    return this;
  }

  public RequestDetailsBuilder WithController(string? controller)
  {
    _controller = controller;
    return this;
  }

  public RequestDetailsBuilder WithDefaults()
  {
    _path = "/api/email/validate";
    _method = "POST";
    _controller = "ApiController.ValidateEmail";
    return this;
  }

  public RequestDetailsBuilder WithGetRequest(string path)
  {
    _path = path;
    _method = "GET";
    _controller = null;
    return this;
  }

  public RequestDetailsBuilder WithPostRequest(string path)
  {
    _path = path;
    _method = "POST";
    _controller = null;
    return this;
  }

  public RequestDetailsBuilder WithCompleteContext()
  {
    _path = "/api/users/123";
    _method = "PUT";
    _controller = "UsersController.UpdateUser";
    return this;
  }

  public RequestDetails Build()
  {
    return new RequestDetails(_path, _method, _controller);
  }
}
