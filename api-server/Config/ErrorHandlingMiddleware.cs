using System.Net;
using System.Text.Json;
using Zarichney.Services.Status;

namespace Zarichney.Config;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
  public async Task Invoke(HttpContext context)
  {
    try
    {
      await next(context);
    }
    catch (ServiceUnavailableException serviceEx)
    {
      logger.LogError(serviceEx,
        "ServiceUnavailableException caught by middleware for {Method} {Path}. Reasons: {Reasons}",
        context.Request.Method,
        context.Request.Path,
        string.Join(", ", serviceEx.Reasons!));

      context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable; // 503
      context.Response.ContentType = "application/json";

      var errorResponse = new
      {
        Error = "External Service Unavailable",
        Message = serviceEx.Message,
        MissingConfigurations = serviceEx.Reasons,
        TraceId = context.TraceIdentifier
      };
      await context.Response.WriteAsJsonAsync(errorResponse);
    }
    catch (ConfigurationMissingException configEx)
    {
      logger.LogError(configEx,
        "ConfigurationMissingException caught by middleware for {Method} {Path}. Section: {Section}, Details: {Details}",
        context.Request.Method,
        context.Request.Path,
        configEx.ConfigurationSection,
        configEx.MissingKeyDetails);

      context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable; // 503
      context.Response.ContentType = "application/json";

      var errorResponse = new
      {
        Error = "Configuration Missing",
        Message = $"A required configuration for a service is missing or invalid. Section: {configEx.ConfigurationSection}",
        TraceId = context.TraceIdentifier
      };
      await context.Response.WriteAsJsonAsync(errorResponse);
    }
    catch (Exception ex)
    {
      logger.LogError(ex,
        "{Middleware}: Unhandled exception occurred while processing {Method} {Path}",
        nameof(ErrorHandlingMiddleware),
        context.Request.Method,
        context.Request.Path);

      // Only handle truly unexpected errors
      var error = new
      {
        Error = new
        {
          Message = "An unexpected error occurred",
          Type = ex.GetType().Name,
          ex.Source,
        },
        Request = new
        {
          Path = context.Request.Path.Value,
          context.Request.Method,
        },
        TraceId = context.TraceIdentifier
      };

      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      context.Response.ContentType = "application/json";

      await context.Response.WriteAsJsonAsync(error, new JsonSerializerOptions
      {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      });
    }
  }
}
