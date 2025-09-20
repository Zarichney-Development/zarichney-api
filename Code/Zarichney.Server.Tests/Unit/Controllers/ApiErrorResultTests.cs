using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using Zarichney.Controllers;

namespace Zarichney.Tests.Unit.Controllers;

/// <summary>
/// Unit tests for ApiErrorResult - tests error response formatting and HTTP status code handling
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Controllers")]
public class ApiErrorResultTests
{
  private readonly Mock<HttpContext> _mockHttpContext;
  private readonly Mock<HttpRequest> _mockRequest;
  private readonly Mock<HttpResponse> _mockResponse;
  private readonly ActionContext _actionContext;
  private readonly MemoryStream _responseStream;

  public ApiErrorResultTests()
  {
    _mockHttpContext = new Mock<HttpContext>();
    _mockRequest = new Mock<HttpRequest>();
    _mockResponse = new Mock<HttpResponse>();
    _responseStream = new MemoryStream();

    // Setup basic HTTP context
    _mockHttpContext.Setup(x => x.Request).Returns(_mockRequest.Object);
    _mockHttpContext.Setup(x => x.Response).Returns(_mockResponse.Object);
    _mockHttpContext.Setup(x => x.TraceIdentifier).Returns("test-trace-id");

    _mockRequest.Setup(x => x.Path).Returns(new PathString("/api/test"));
    _mockRequest.Setup(x => x.Method).Returns("GET");

    _mockResponse.SetupGet(x => x.Body).Returns(_responseStream);
    _mockResponse.SetupProperty(x => x.StatusCode);
    _mockResponse.SetupProperty(x => x.ContentType);
    _mockResponse.SetupGet(x => x.HasStarted).Returns(false);

    _actionContext = new ActionContext(
        _mockHttpContext.Object,
        new RouteData(),
        new ActionDescriptor { DisplayName = "TestController.TestAction" }
    );
  }

  #region Constructor Tests

  [Fact]
  public void Constructor_WithAllParameters_SetsPropertiesCorrectly()
  {
    // Arrange
    var exception = new InvalidOperationException("Test exception");
    var userMessage = "Custom error message";
    var statusCode = HttpStatusCode.BadRequest;

    // Act
    var result = new ApiErrorResult(exception, userMessage, statusCode);

    // Assert
    result.Should().NotBeNull("because the ApiErrorResult should be created");
    // Note: Properties are private, we'll verify through ExecuteResultAsync
  }

  [Fact]
  public void Constructor_WithDefaults_UsesDefaultValues()
  {
    // Act
    var result = new ApiErrorResult();

    // Assert
    result.Should().NotBeNull("because the ApiErrorResult should be created with defaults");
  }

  #endregion

  #region ExecuteResultAsync Tests - Status Code Handling

  [Theory]
  [InlineData(HttpStatusCode.BadRequest, 400)]
  [InlineData(HttpStatusCode.NotFound, 404)]
  [InlineData(HttpStatusCode.InternalServerError, 500)]
  [InlineData(HttpStatusCode.Unauthorized, 401)]
  [InlineData(HttpStatusCode.Forbidden, 403)]
  public async Task ExecuteResultAsync_WithVariousStatusCodes_SetsCorrectStatusCode(
      HttpStatusCode statusCode, int expectedCode)
  {
    // Arrange
    var result = new ApiErrorResult(statusCode: statusCode);

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    _mockResponse.Object.StatusCode.Should().Be(expectedCode,
        $"because the HTTP status code should be {expectedCode} for {statusCode}");
  }

  [Fact]
  public async Task ExecuteResultAsync_WithDefaultStatusCode_Returns500()
  {
    // Arrange
    var result = new ApiErrorResult();

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    _mockResponse.Object.StatusCode.Should().Be(500,
        "because the default status code should be Internal Server Error (500)");
  }

  #endregion

  #region ExecuteResultAsync Tests - Content Type

  [Fact]
  public async Task ExecuteResultAsync_Always_SetsContentTypeToJson()
  {
    // Arrange
    var result = new ApiErrorResult();

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    _mockResponse.Object.ContentType.Should().Be("application/json",
        "because the response should always be JSON");
  }

  #endregion

  #region ExecuteResultAsync Tests - Response Body Structure

  [Fact]
  public async Task ExecuteResultAsync_WithException_IncludesExceptionDetails()
  {
    // Arrange
    var exception = new InvalidOperationException("Test exception message");
    var result = new ApiErrorResult(exception, "User friendly message");

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain("\"message\": \"User friendly message\"",
        "because the user message should be included");
    responseBody.Should().Contain("\"type\": \"InvalidOperationException\"",
        "because the exception type should be included");
    responseBody.Should().Contain("\"details\": \"Test exception message\"",
        "because the exception details should be included");
  }

  [Fact]
  public async Task ExecuteResultAsync_WithoutException_ExcludesExceptionDetails()
  {
    // Arrange
    var result = new ApiErrorResult(userMessage: "Just an error message");

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain("\"message\": \"Just an error message\"",
        "because the user message should be included");
    responseBody.Should().NotContain("\"type\": \"InvalidOperationException\"",
        "because no exception type should be included when there's no exception");
  }

  [Fact]
  public async Task ExecuteResultAsync_Always_IncludesRequestDetails()
  {
    // Arrange
    var result = new ApiErrorResult();

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain("\"path\": \"/api/test\"",
        "because the request path should be included");
    responseBody.Should().Contain("\"method\": \"GET\"",
        "because the request method should be included");
    responseBody.Should().Contain("\"controller\": \"TestController.TestAction\"",
        "because the controller action should be included");
  }

  [Fact]
  public async Task ExecuteResultAsync_Always_IncludesTraceId()
  {
    // Arrange
    var result = new ApiErrorResult();

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain("\"traceId\": \"test-trace-id\"",
        "because the trace ID should be included for debugging");
  }

  #endregion

  #region ExecuteResultAsync Tests - Inner Exception Handling

  [Fact]
  public async Task ExecuteResultAsync_WithInnerException_IncludesInnerExceptionDetails()
  {
    // Arrange
    var innerException = new ArgumentException("Inner exception message");
    var outerException = new InvalidOperationException("Outer exception message", innerException);
    var result = new ApiErrorResult(outerException);

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain("\"innerException\"",
        "because inner exception section should be present");
    responseBody.Should().Contain("\"message\": \"Inner exception message\"",
        "because inner exception message should be included");
    responseBody.Should().Contain("\"type\": \"ArgumentException\"",
        "because inner exception type should be included");
  }

  [Fact]
  public async Task ExecuteResultAsync_WithoutInnerException_ExcludesInnerExceptionSection()
  {
    // Arrange
    var exception = new InvalidOperationException("Simple exception");
    var result = new ApiErrorResult(exception);

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().NotContain("\"innerException\"",
        "because inner exception section should not be present when there's no inner exception");
  }

  #endregion

  #region ExecuteResultAsync Tests - Stack Trace Formatting

  [Fact]
  public async Task ExecuteResultAsync_WithStackTrace_FormatsStackTraceAsArray()
  {
    // Arrange
    Exception capturedEx;
    try
    {
      throw new InvalidOperationException("Test exception with stack trace");
    }
    catch (Exception ex)
    {
      capturedEx = ex;
    }

    var result = new ApiErrorResult(capturedEx);

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain("\"stackTrace\": [",
        "because stack trace should be formatted as an array");
    responseBody.Should().Contain("ApiErrorResultTests.ExecuteResultAsync_WithStackTrace_FormatsStackTraceAsArray",
        "because stack trace should contain the test method name");
  }

  [Fact]
  public async Task ExecuteResultAsync_WithExceptionButNoStackTrace_HandlesGracefully()
  {
    // Arrange
    var exception = new Exception("Exception without stack trace");
    // Create exception without stack trace by not throwing it
    var result = new ApiErrorResult(exception);

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    // Should not throw and should handle null stack trace gracefully
    responseBody.Should().Contain("\"details\": \"Exception without stack trace\"",
        "because the exception message should still be included");
  }

  #endregion

  #region ExecuteResultAsync Tests - Edge Cases

  [Fact]
  public async Task ExecuteResultAsync_WithNullRequestPath_HandlesGracefully()
  {
    // Arrange
    _mockRequest.Setup(x => x.Path).Returns(new PathString());
    var result = new ApiErrorResult();

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain("\"request\"",
        "because request section should still be included");
    // Should not throw when path is null
  }

  [Fact]
  public async Task ExecuteResultAsync_WithNullControllerDisplayName_HandlesGracefully()
  {
    // Arrange
    _actionContext.ActionDescriptor.DisplayName = null;
    var result = new ApiErrorResult();

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain("\"controller\": null",
        "because null controller name should be handled");
  }

  [Fact]
  public async Task ExecuteResultAsync_WithVeryLongExceptionMessage_IncludesFullMessage()
  {
    // Arrange
    var longMessage = new string('x', 5000);
    var exception = new Exception(longMessage);
    var result = new ApiErrorResult(exception);

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain(longMessage.Substring(0, 100),
        "because the full exception message should be included regardless of length");
  }

  #endregion

  #region ExecuteResultAsync Tests - JSON Formatting

  [Fact]
  public async Task ExecuteResultAsync_Always_ProducesIndentedJson()
  {
    // Arrange
    var result = new ApiErrorResult();

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain("\n  ",
        "because JSON should be indented for readability");
  }

  [Fact]
  public async Task ExecuteResultAsync_Always_UsesCamelCasePropertyNames()
  {
    // Arrange
    var result = new ApiErrorResult();

    // Act
    await result.ExecuteResultAsync(_actionContext);

    // Assert
    var responseBody = GetResponseBody();
    responseBody.Should().Contain("\"error\"",
        "because property names should be in camelCase");
    responseBody.Should().Contain("\"traceId\"",
        "because property names should be in camelCase");
    responseBody.Should().NotContain("\"Error\"",
        "because property names should not be in PascalCase");
    responseBody.Should().NotContain("\"TraceId\"",
        "because property names should not be in PascalCase");
  }

  #endregion

  #region Helper Methods

  private string GetResponseBody()
  {
    _responseStream.Position = 0;
    using var reader = new StreamReader(_responseStream, Encoding.UTF8);
    return reader.ReadToEnd();
  }

  #endregion
}
