using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Zarichney.Controllers;

public class ApiErrorResult(
  Exception? exception = null,
  string userMessage = "An unexpected error occurred",
  HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
  : IActionResult
{
  public async Task ExecuteResultAsync(ActionContext context)
  {
    // Build error payload with explicit keys to control null properties
    var error = new Dictionary<string, object?>
    {
      ["message"] = userMessage,
      ["type"] = exception?.GetType().Name,
      ["details"] = exception?.Message,
      ["source"] = exception?.Source,
      ["stackTrace"] = exception?.StackTrace?.Split(Environment.NewLine)
        .Select(line => line.TrimStart())
        .ToList(),
    };

    var request = new Dictionary<string, object?>
    {
      ["path"] = context.HttpContext.Request.Path.Value,
      ["method"] = context.HttpContext.Request.Method,
      ["controller"] = context.ActionDescriptor.DisplayName
    };

    var response = new Dictionary<string, object?>
    {
      ["error"] = error,
      ["request"] = request,
      ["traceId"] = context.HttpContext.TraceIdentifier
    };

    if (exception?.InnerException is not null)
    {
      response["innerException"] = new Dictionary<string, object?>
      {
        ["message"] = exception.InnerException.Message,
        ["type"] = exception.InnerException.GetType().Name,
        ["stackTrace"] = exception.InnerException.StackTrace?.Split(Environment.NewLine)
          .Select(line => line.TrimStart())
          .ToList()
      };
    }

    context.HttpContext.Response.StatusCode = (int)statusCode;
    context.HttpContext.Response.ContentType = "application/json";

    // Serialize with camelCase and write to response body (works with mocked HttpResponse)
    var options = new JsonSerializerOptions
    {
      WriteIndented = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };
    var json = JsonSerializer.Serialize(response, options);
    var buffer = System.Text.Encoding.UTF8.GetBytes(json);
    await context.HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
  }
}
