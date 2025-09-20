using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Security.Claims;
using Stripe;

namespace Zarichney.Server.Tests.Framework.Helpers;

/// <summary>
/// Shared test helpers for common Moq setup patterns.
/// Reduces duplication across test files by providing standard mock configurations.
/// Register as singleton in DI container for access via GetService&lt;MockHelper&gt;().
/// </summary>
public class MockHelper
{
  /// <summary>
  /// Creates a mock logger with standard setup that accepts any log level and parameters.
  /// Includes verification helpers for common logging scenarios.
  /// </summary>
  /// <typeparam name="T">The type being logged</typeparam>
  /// <returns>Mock logger configured for testing</returns>
  public Mock<ILogger<T>> CreateMockLogger<T>()
  {
    var mockLogger = new Mock<ILogger<T>>();

    // Setup standard logging behavior that accepts any parameters
    mockLogger
        .Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
        .Callback<LogLevel, EventId, object, Exception?, Delegate>((level, eventId, state, exception, formatter) =>
        {
          // Optional: Add debugging output for test troubleshooting
          // Console.WriteLine($"Log: {level} - {formatter.DynamicInvoke(state, exception)}");
        });

    return mockLogger;
  }

  /// <summary>
  /// Creates a mock IOptions&lt;T&gt; with the specified configuration value.
  /// </summary>
  /// <typeparam name="T">The configuration type</typeparam>
  /// <param name="configValue">The configuration instance to wrap</param>
  /// <returns>Mock IOptions configured with the provided value</returns>
  public Mock<IOptions<T>> CreateMockOptions<T>(T configValue) where T : class
  {
    var mockOptions = new Mock<IOptions<T>>();
    mockOptions.Setup(x => x.Value).Returns(configValue);
    return mockOptions;
  }

  /// <summary>
  /// Creates a properly configured DefaultHttpContext for controller testing.
  /// Includes standard features needed for form handling and request processing.
  /// </summary>
  /// <param name="contentType">Optional content type (defaults to multipart/form-data)</param>
  /// <param name="setupForm">Whether to pre-configure form handling features</param>
  /// <returns>Configured HttpContext ready for controller testing</returns>
  public DefaultHttpContext CreateTestHttpContext(
      string? contentType = null,
      bool setupForm = true)
  {
    var httpContext = new DefaultHttpContext();

    // Set default content type for form handling
    httpContext.Request.ContentType = contentType ??
        "multipart/form-data; boundary=---------------------------9051914041544843365972754266";

    if (setupForm)
    {
      // Pre-configure form features to avoid parsing issues in tests
      var emptyFiles = new FormFileCollection();
      var formCollection = new FormCollection(new Dictionary<string, StringValues>(), emptyFiles);
      httpContext.Features.Set<IFormFeature>(new FormFeature(formCollection));
    }

    return httpContext;
  }

  /// <summary>
  /// Creates a controller context with a properly configured test HttpContext.
  /// Simplifies controller testing setup by providing standard configuration.
  /// </summary>
  /// <param name="contentType">Optional content type</param>
  /// <param name="setupForm">Whether to pre-configure form handling</param>
  /// <returns>ControllerContext ready for testing</returns>
  public ControllerContext CreateTestControllerContext(
      string? contentType = null,
      bool setupForm = true)
  {
    var httpContext = CreateTestHttpContext(contentType, setupForm);
    return new ControllerContext { HttpContext = httpContext };
  }

  /// <summary>
  /// Creates a ClaimsPrincipal for authentication testing.
  /// </summary>
  /// <param name="userId">User identifier</param>
  /// <param name="userEmail">User email</param>
  /// <param name="additionalClaims">Additional claims to include</param>
  /// <returns>ClaimsPrincipal configured for testing</returns>
  public ClaimsPrincipal CreateTestUser(
      string userId = "test-user-123",
      string userEmail = "test@zarichney.com",
      params Claim[] additionalClaims)
  {
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, userEmail),
            new(ClaimTypes.Name, userEmail)
        };

    if (additionalClaims.Any())
    {
      claims.AddRange(additionalClaims);
    }

    return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
  }

  /// <summary>
  /// Verifies that a logger was called with a specific log level.
  /// Provides more readable assertion than raw Mock verification.
  /// </summary>
  /// <typeparam name="T">The logged type</typeparam>
  /// <param name="mockLogger">The mock logger to verify</param>
  /// <param name="logLevel">Expected log level</param>
  /// <param name="times">Expected number of calls</param>
  public void VerifyLogLevel<T>(
      Mock<ILogger<T>> mockLogger,
      LogLevel logLevel,
      Times times)
  {
    mockLogger.Verify(
        x => x.Log(
            logLevel,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        times);
  }

  /// <summary>
  /// Verifies that a logger was called with a message containing specific text.
  /// Useful for validating log content in tests.
  /// </summary>
  /// <typeparam name="T">The logged type</typeparam>
  /// <param name="mockLogger">The mock logger to verify</param>
  /// <param name="logLevel">Expected log level</param>
  /// <param name="expectedText">Text that should be in the log message</param>
  /// <param name="times">Expected number of calls</param>
  public void VerifyLogContains<T>(
      Mock<ILogger<T>> mockLogger,
      LogLevel logLevel,
      string expectedText,
      Times times)
  {
    mockLogger.Verify(
        x => x.Log(
            logLevel,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedText)),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        times);
  }

  /// <summary>
  /// Creates a mock HttpClient with standard test configuration.
  /// Useful for services that depend on HttpClient.
  /// </summary>
  /// <returns>HttpClient configured for testing</returns>
  public HttpClient CreateTestHttpClient()
  {
    var handler = new Mock<HttpMessageHandler>();
    return new HttpClient(handler.Object)
    {
      BaseAddress = new Uri("https://test.api.example.com/")
    };
  }

  /// <summary>
  /// Sets up a mock to return a successful task result.
  /// Simplifies async mock setup patterns.
  /// </summary>
  /// <typeparam name="T">Return type</typeparam>
  /// <param name="returnValue">Value to return</param>
  /// <returns>Task wrapping the return value</returns>
  public Task<T> SuccessTask<T>(T returnValue)
  {
    return Task.FromResult(returnValue);
  }

  /// <summary>
  /// Sets up a mock to return a failed task with the specified exception.
  /// Simplifies error testing patterns.
  /// </summary>
  /// <param name="exception">Exception to throw</param>
  /// <returns>Faulted task</returns>
  public Task FailedTask(Exception exception)
  {
    var tcs = new TaskCompletionSource();
    tcs.SetException(exception);
    return tcs.Task;
  }

  /// <summary>
  /// Sets up a mock to return a failed task with the specified exception.
  /// Simplifies error testing patterns for generic tasks.
  /// </summary>
  /// <typeparam name="T">Return type</typeparam>
  /// <param name="exception">Exception to throw</param>
  /// <returns>Faulted task</returns>
  public Task<T> FailedTask<T>(Exception exception)
  {
    var tcs = new TaskCompletionSource<T>();
    tcs.SetException(exception);
    return tcs.Task;
  }

  /// <summary>
  /// Creates a mock Stripe event for testing payment webhook scenarios.
  /// Replaces the removed production method StripeService.CreateMockEvent.
  /// </summary>
  /// <typeparam name="T">The Stripe entity type</typeparam>
  /// <param name="stripeObject">The Stripe entity to wrap in the event</param>
  /// <returns>Mock Stripe event for testing</returns>
  public Event CreateMockStripeEvent<T>(T stripeObject) where T : IHasObject
  {
    return new Event
    {
      Data = new EventData
      {
        Object = (IHasObject)stripeObject
      }
    };
  }
}
