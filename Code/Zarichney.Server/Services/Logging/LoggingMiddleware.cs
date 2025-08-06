using Microsoft.Extensions.Options;
using Zarichney.Services.Sessions;

namespace Zarichney.Services.Logging;

/// <summary>
/// Configuration options for request/response logging middleware
/// </summary>
public class RequestResponseLoggerOptions
{
  public bool LogRequests { get; set; } = true;
  public bool LogResponses { get; set; } = true;
  public string[] SensitiveHeaders { get; set; } = ["Authorization", "Cookie"];
  public Func<HttpContext, bool> RequestFilter { get; set; } = _ => true;
  public string LogDirectory { get; set; } = "Logs";
}

/// <summary>
/// Enhanced middleware for logging HTTP requests and responses with correlation tracking
/// </summary>
public class RequestResponseLoggerMiddleware(
  RequestDelegate next,
  IOptions<RequestResponseLoggerOptions> options,
  ILogger<RequestResponseLoggerMiddleware> logger,
  IServiceProvider serviceProvider)
{
  private readonly RequestResponseLoggerOptions _options = options.Value;

  public async Task InvokeAsync(HttpContext context)
  {
    if (!_options.RequestFilter(context))
    {
      await next(context);
      return;
    }

    var scopeContainer = context.Features.Get<IScopeContainer>();
    var scopeId = scopeContainer?.Id;
    var sessionId = scopeContainer?.SessionId;

    // Get logging method for enhanced diagnostics from request scope
    var loggingStatus = context.RequestServices.GetRequiredService<ILoggingStatus>();
    var loggingMethod = await loggingStatus.GetLoggingMethodAsync(context.RequestAborted);

    // Replace the current logger in the logging context
    using (Serilog.Context.LogContext.PushProperty("ScopeId", scopeId))
    using (Serilog.Context.LogContext.PushProperty("SessionId", sessionId))
    using (Serilog.Context.LogContext.PushProperty("LoggingMethod", loggingMethod))
    {
      if (_options.LogRequests)
      {
        await LogRequestAsync(context);
      }

      var originalBodyStream = context.Response.Body;
      using var responseBody = new MemoryStream();
      context.Response.Body = responseBody;

      try
      {
        await next(context);
      }
      finally
      {
        if (_options.LogResponses)
        {
          await LogResponseAsync(context, responseBody);
        }

        responseBody.Position = 0;
        await responseBody.CopyToAsync(originalBodyStream);
      }
    }
  }

  private async Task LogRequestAsync(HttpContext context)
  {
    context.Request.EnableBuffering();

    string requestBody;
    using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
    {
      requestBody = await reader.ReadToEndAsync();
    }
    context.Request.Body.Position = 0;

    var logContext = new
    {
      RequestMethod = context.Request.Method,
      RequestPath = context.Request.Path,
      RequestQueryString = context.Request.QueryString.ToString(),
      RequestHeaders = MaskSensitiveHeaders(context.Request.Headers),
      RequestBody = requestBody
    };

    logger.LogInformation("HTTP Request: {@RequestDetails}", logContext);
  }

  private async Task LogResponseAsync(HttpContext context, MemoryStream responseBody)
  {
    responseBody.Position = 0;
    string responseContent;
    using (var reader = new StreamReader(responseBody, leaveOpen: true))
    {
      responseContent = await reader.ReadToEndAsync();
    }

    var logContext = new
    {
      ResponseStatusCode = context.Response.StatusCode,
      ResponseHeaders = MaskSensitiveHeaders(context.Response.Headers),
      ResponseBody = responseContent
    };

    logger.LogInformation("HTTP Response: {@ResponseDetails}", logContext);
  }

  private object MaskSensitiveHeaders(IHeaderDictionary headers)
  {
    return headers.ToDictionary(
      h => h.Key,
      h => _options.SensitiveHeaders.Contains(h.Key) ? "******" : h.Value.ToString());
  }
}

/// <summary>
/// Service collection extensions for logging middleware
/// </summary>
public static partial class LoggingServiceCollectionExtensions
{
  public static void AddRequestResponseLogger(this IServiceCollection services,
    Action<RequestResponseLoggerOptions>? configureOptions = null)
  {
    if (configureOptions != null)
    {
      services.Configure(configureOptions);
    }
    else
    {
      services.Configure<RequestResponseLoggerOptions>(_ => { });
    }
  }
}