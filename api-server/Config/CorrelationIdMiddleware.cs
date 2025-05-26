namespace Zarichney.Config;

/// <summary>
/// Middleware that handles request correlation ID for tracing requests across logs.
/// Extracts correlation ID from incoming request headers or generates a new one,
/// adds it to the log context, and includes it in response headers.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
  private const string CorrelationIdHeaderName = "X-Correlation-ID";
  private readonly RequestDelegate _next = next;

  /// <summary>
  /// Processes the HTTP request and manages correlation ID for logging and response headers.
  /// </summary>
  /// <param name="context">The HTTP context for the current request.</param>
  public async Task InvokeAsync(HttpContext context)
  {
    // Extract or generate correlation ID
    var correlationId = GetOrGenerateCorrelationId(context);

    // Store in HttpContext.Items for potential access by other components
    context.Items["CorrelationId"] = correlationId;

    // Add correlation ID to response headers early (before response starts)
    context.Response.OnStarting(() =>
    {
      if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
      {
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;
      }
      return Task.CompletedTask;
    });

    // Push correlation ID to Serilog's LogContext for all log events in this request
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
      await _next(context);
    }
  }

  /// <summary>
  /// Extracts correlation ID from request headers or generates a new one if not present.
  /// </summary>
  /// <param name="context">The HTTP context containing request headers.</param>
  /// <returns>The correlation ID to use for this request.</returns>
  private static string GetOrGenerateCorrelationId(HttpContext context)
  {
    // Check if correlation ID is provided in request headers
    if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue) &&
        !string.IsNullOrWhiteSpace(headerValue.ToString()))
    {
      return headerValue.ToString();
    }

    // Generate new correlation ID if not provided
    return Guid.NewGuid().ToString();
  }
}
