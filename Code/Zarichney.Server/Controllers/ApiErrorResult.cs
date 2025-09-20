using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Zarichney.Controllers.Responses;

namespace Zarichney.Controllers;

public class ApiErrorResult(
  Exception? exception = null,
  string userMessage = "An unexpected error occurred",
  HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
  : IActionResult
{
  public async Task ExecuteResultAsync(ActionContext context)
  {
    // Build structured DTO
    var response = BuildApiErrorResponse(context);

    // Use JsonResult with explicit formatting to match original behavior
    var jsonSerializerOptions = new JsonSerializerOptions
    {
      WriteIndented = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    var jsonResult = new JsonResult(response, jsonSerializerOptions)
    {
      StatusCode = (int)statusCode,
      ContentType = "application/json"
    };

    await jsonResult.ExecuteResultAsync(context);
  }

  private ApiErrorResponse BuildApiErrorResponse(ActionContext context)
  {
    var errorDetails = BuildErrorDetails();
    var requestDetails = BuildRequestDetails(context);
    var innerException = BuildInnerExceptionDetails();

    return new ApiErrorResponse(
      Error: errorDetails,
      Request: requestDetails,
      TraceId: context.HttpContext.TraceIdentifier,
      InnerException: innerException
    );
  }

  private ErrorDetails BuildErrorDetails()
  {
    var stackTrace = exception?.StackTrace?.Split(Environment.NewLine)
      .Select(line => line.TrimStart())
      .ToList()
      .AsReadOnly();

    return new ErrorDetails(
      Message: userMessage,
      Type: exception?.GetType().Name,
      Details: exception?.Message,
      Source: exception?.Source,
      StackTrace: stackTrace
    );
  }

  private RequestDetails BuildRequestDetails(ActionContext context)
  {
    return new RequestDetails(
      Path: context.HttpContext.Request.Path.Value,
      Method: context.HttpContext.Request.Method,
      Controller: context.ActionDescriptor.DisplayName
    );
  }

  private InnerExceptionDetails? BuildInnerExceptionDetails()
  {
    if (exception?.InnerException is null)
      return null;

    var innerStackTrace = exception.InnerException.StackTrace?.Split(Environment.NewLine)
      .Select(line => line.TrimStart())
      .ToList()
      .AsReadOnly();

    return new InnerExceptionDetails(
      Message: exception.InnerException.Message,
      Type: exception.InnerException.GetType().Name,
      StackTrace: innerStackTrace
    );
  }
}
