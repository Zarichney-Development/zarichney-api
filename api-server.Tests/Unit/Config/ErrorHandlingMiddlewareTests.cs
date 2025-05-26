using Zarichney.Services.Status;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Config;

namespace Zarichney.Tests.Unit.Config;

[Trait("Category", "Unit")]
public class ErrorHandlingMiddlewareTests
{
  private readonly Mock<ILogger<ErrorHandlingMiddleware>> _mockLogger;
  private readonly ErrorHandlingMiddleware _middleware;
  private readonly Mock<RequestDelegate> _mockNextDelegate;
  private readonly DefaultHttpContext _httpContext;

  public ErrorHandlingMiddlewareTests()
  {
    _mockLogger = new Mock<ILogger<ErrorHandlingMiddleware>>();
    _mockNextDelegate = new Mock<RequestDelegate>();
    _middleware = new ErrorHandlingMiddleware(_mockNextDelegate.Object, _mockLogger.Object);
    _httpContext = new DefaultHttpContext { Response = { Body = new MemoryStream() }, TraceIdentifier = "test-trace-id" };
  }

  [Fact]
  public async Task Invoke_WithoutException_CallsNextDelegate()
  {
    // Arrange
    _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>()))
        .Returns(Task.CompletedTask);

    // Act
    await _middleware.Invoke(_httpContext);

    // Assert
    _mockNextDelegate.Verify(next => next(_httpContext), Times.Once);
    _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);
  }

  [Fact]
  public async Task Invoke_WithServiceUnavailableException_Returns503WithErrorResponse()
  {
    // Arrange
    var missingConfigs = new List<string> { "Llm:ApiKey", "Llm:ApiEndpoint" };
    var exception = new ServiceUnavailableException("Service is unavailable", missingConfigs);

    _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>()))
        .Throws(exception);

    // Act
    await _middleware.Invoke(_httpContext);

    // Assert
    _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);
    _httpContext.Response.ContentType.Should().StartWith("application/json");

    // Reset the position to read from the start
    _httpContext.Response.Body.Position = 0;
    string responseBody;
    using (var reader = new StreamReader(_httpContext.Response.Body))
    {
      responseBody = await reader.ReadToEndAsync();
    }
    var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    response.Should().NotBeNull();
    response.Should().ContainKey("error");
    ((JsonElement)response["error"]).GetString().Should().Be("External Service Unavailable");
    response.Should().ContainKey("message");
    ((JsonElement)response["message"]).GetString().Should().Be("Service is unavailable");
    response.Should().ContainKey("missingConfigurations");
    var missingConfigsElement = (JsonElement)response["missingConfigurations"];
    var extractedConfigs = new List<string>();
    foreach (var element in missingConfigsElement.EnumerateArray())
    {
      extractedConfigs.Add(element.GetString() ?? string.Empty);
    }
    extractedConfigs.Should().BeEquivalentTo(missingConfigs);
    response.Should().ContainKey("traceId");
    ((JsonElement)response["traceId"]).GetString().Should().Be("test-trace-id");
  }

  [Fact]
  public async Task Invoke_WithConfigurationMissingException_Returns503WithErrorResponse()
  {
    // Arrange
    var exception = new ConfigurationMissingException("LlmConfig", "ApiKey is missing or using the default placeholder");

    _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>()))
        .Throws(exception);

    // Act
    await _middleware.Invoke(_httpContext);

    // Assert
    _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);
    _httpContext.Response.ContentType.Should().StartWith("application/json");

    // Reset the position to read from the start
    _httpContext.Response.Body.Position = 0;
    string responseBody;
    using (var reader = new StreamReader(_httpContext.Response.Body))
    {
      responseBody = await reader.ReadToEndAsync();
    }
    var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    response.Should().NotBeNull();
    response.Should().ContainKey("error");
    ((JsonElement)response["error"]).GetString().Should().Be("Configuration Missing");
    response.Should().ContainKey("message");
    ((JsonElement)response["message"]).GetString().Should().Be("A required configuration for a service is missing or invalid. Section: LlmConfig");
    response.Should().ContainKey("traceId");
    ((JsonElement)response["traceId"]).GetString().Should().Be("test-trace-id");
  }

  [Fact]
  public async Task Invoke_WithGenericException_Returns500WithErrorResponse()
  {
    // Arrange
    var exception = new InvalidOperationException("An unexpected error occurred during processing");

    _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>()))
        .Throws(exception);

    // Act
    await _middleware.Invoke(_httpContext);

    // Assert
    _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    _httpContext.Response.ContentType.Should().StartWith("application/json");

    // Reset the position to read from the start
    _httpContext.Response.Body.Position = 0;
    string responseBody;
    using (var reader = new StreamReader(_httpContext.Response.Body))
    {
      responseBody = await reader.ReadToEndAsync();
    }
    var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    response.Should().NotBeNull();
    response.Should().ContainKey("error");
    var errorElement = (JsonElement)response["error"];
    errorElement.GetProperty("message").GetString().Should().Be("An unexpected error occurred");
    errorElement.GetProperty("type").GetString().Should().Be("InvalidOperationException");
    response.Should().ContainKey("request");
    response.Should().ContainKey("traceId");
    ((JsonElement)response["traceId"]).GetString().Should().Be("test-trace-id");
  }

  [Fact]
  public async Task Invoke_WithServiceUnavailableException_LogsException()
  {
    // Arrange
    var missingConfigs = new List<string> { "Llm:ApiKey", "Llm:ApiEndpoint" };
    var exception = new ServiceUnavailableException("Service is unavailable", missingConfigs);

    _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>()))
        .Throws(exception);

    // Setup logger verification
    _mockLogger.Setup(logger => logger.Log(
        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => true),
        It.Is<Exception>(ex => ex == exception),
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
    ));

    // Act
    await _middleware.Invoke(_httpContext);

    // Assert
    _mockLogger.Verify(logger => logger.Log(
        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => true),
        It.Is<Exception>(ex => ex == exception),
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
    ), Times.Once);
  }
}
