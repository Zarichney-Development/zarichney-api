using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Controllers;

/// <summary>
/// Unit tests for ApiErrorResult using the builder pattern - demonstrates builder usage and additional scenarios
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Controllers")]
public class ApiErrorResultBuilderTests
{
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpRequest> _mockRequest;
    private readonly Mock<HttpResponse> _mockResponse;
    private readonly ActionContext _actionContext;
    private readonly MemoryStream _responseStream;

    public ApiErrorResultBuilderTests()
    {
        _mockHttpContext = new Mock<HttpContext>();
        _mockRequest = new Mock<HttpRequest>();
        _mockResponse = new Mock<HttpResponse>();
        _responseStream = new MemoryStream();

        _mockHttpContext.Setup(x => x.Request).Returns(_mockRequest.Object);
        _mockHttpContext.Setup(x => x.Response).Returns(_mockResponse.Object);
        _mockHttpContext.Setup(x => x.TraceIdentifier).Returns("test-trace-id");
        
        _mockRequest.Setup(x => x.Path).Returns(new PathString("/api/test"));
        _mockRequest.Setup(x => x.Method).Returns("POST");

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

    #region Builder Pattern Tests

    [Fact]
    public async Task Builder_WithBadRequest_CreatesBadRequestError()
    {
        // Arrange
        var result = new ApiErrorResultBuilder()
            .WithBadRequest("Invalid input provided")
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        _mockResponse.Object.StatusCode.Should().Be(400, "because it's a bad request");
        var responseBody = GetResponseBody();
        responseBody.Should().Contain("\"message\": \"Invalid input provided\"");
    }

    [Fact]
    public async Task Builder_WithNotFound_CreatesNotFoundError()
    {
        // Arrange
        var result = new ApiErrorResultBuilder()
            .WithNotFound("User not found")
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        _mockResponse.Object.StatusCode.Should().Be(404, "because it's a not found error");
        var responseBody = GetResponseBody();
        responseBody.Should().Contain("\"message\": \"User not found\"");
    }

    [Fact]
    public async Task Builder_WithUnauthorized_CreatesUnauthorizedError()
    {
        // Arrange
        var result = new ApiErrorResultBuilder()
            .WithUnauthorized("Please login to continue")
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        _mockResponse.Object.StatusCode.Should().Be(401, "because it's an unauthorized error");
        var responseBody = GetResponseBody();
        responseBody.Should().Contain("\"message\": \"Please login to continue\"");
    }

    [Fact]
    public async Task Builder_WithForbidden_CreatesForbiddenError()
    {
        // Arrange
        var result = new ApiErrorResultBuilder()
            .WithForbidden("You don't have permission to access this resource")
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        _mockResponse.Object.StatusCode.Should().Be(403, "because it's a forbidden error");
    }

    #endregion

    #region Exception Builder Tests

    [Fact]
    public async Task Builder_WithInvalidOperationException_IncludesExceptionDetails()
    {
        // Arrange
        var result = new ApiErrorResultBuilder()
            .WithInvalidOperationException("Operation is not valid in current state")
            .WithStatusCode(HttpStatusCode.BadRequest)
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        var responseBody = GetResponseBody();
        responseBody.Should().Contain("\"type\": \"InvalidOperationException\"");
        responseBody.Should().Contain("\"details\": \"Operation is not valid in current state\"");
    }

    [Fact]
    public async Task Builder_WithArgumentException_IncludesParameterName()
    {
        // Arrange
        var result = new ApiErrorResultBuilder()
            .WithArgumentException("Invalid parameter value", "userId")
            .WithBadRequest()
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        var responseBody = GetResponseBody();
        responseBody.Should().Contain("\"type\": \"ArgumentException\"");
        responseBody.Should().Contain("userId");
    }

    [Fact]
    public async Task Builder_WithNullReferenceException_HandlesCorrectly()
    {
        // Arrange
        var result = new ApiErrorResultBuilder()
            .WithNullReferenceException("Object reference not set")
            .WithInternalServerError()
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        _mockResponse.Object.StatusCode.Should().Be(500);
        var responseBody = GetResponseBody();
        responseBody.Should().Contain("\"type\": \"NullReferenceException\"");
    }

    #endregion

    #region Complex Scenario Tests

    [Fact]
    public async Task Builder_ChainedConfiguration_AppliesAllSettings()
    {
        // Arrange
        var exception = new InvalidOperationException("Complex error");
        var result = new ApiErrorResultBuilder()
            .WithException(exception)
            .WithUserMessage("Something went wrong, please try again")
            .WithStatusCode(HttpStatusCode.ServiceUnavailable)
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        _mockResponse.Object.StatusCode.Should().Be(503);
        var responseBody = GetResponseBody();
        responseBody.Should().Contain("\"message\": \"Something went wrong, please try again\"");
        responseBody.Should().Contain("\"type\": \"InvalidOperationException\"");
    }

    [Fact]
    public async Task Builder_WithDefaults_CreatesDefaultError()
    {
        // Arrange
        var result = new ApiErrorResultBuilder()
            .WithDefaults()
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        _mockResponse.Object.StatusCode.Should().Be(500);
        var responseBody = GetResponseBody();
        responseBody.Should().Contain("\"message\": \"An unexpected error occurred\"");
    }

    [Fact]
    public async Task Builder_MultipleBuilds_CreatesIndependentInstances()
    {
        // Arrange
        var builder = new ApiErrorResultBuilder()
            .WithBadRequest("First error");
            
        var result1 = builder.Build();
        var result2 = builder
            .WithNotFound("Second error")
            .Build();

        // Act
        await result1.ExecuteResultAsync(_actionContext);
        var response1 = GetResponseBody();
        
        _responseStream.SetLength(0); // Clear stream
        _responseStream.Position = 0;
        
        await result2.ExecuteResultAsync(_actionContext);
        var response2 = GetResponseBody();

        // Assert
        response1.Should().Contain("First error");
        response2.Should().Contain("Second error");
        _mockResponse.Object.StatusCode.Should().Be(404, "because the second build used NotFound");
    }

    #endregion

    #region Edge Case Tests with Builder

    [Fact]
    public async Task Builder_WithExceptionString_CreatesGenericException()
    {
        // Arrange
        var result = new ApiErrorResultBuilder()
            .WithException("Simple error message")
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        var responseBody = GetResponseBody();
        responseBody.Should().Contain("\"type\": \"Exception\"");
        responseBody.Should().Contain("\"details\": \"Simple error message\"");
    }

    [Fact]
    public async Task Builder_WithNestedExceptions_HandlesInnerException()
    {
        // Arrange
        var inner = new ArgumentNullException("param", "Parameter cannot be null");
        var outer = new InvalidOperationException("Operation failed", inner);
        
        var result = new ApiErrorResultBuilder()
            .WithInnerException(outer, inner)
            .WithStatusCode(HttpStatusCode.BadRequest)
            .Build();

        // Act
        await result.ExecuteResultAsync(_actionContext);

        // Assert
        var responseBody = GetResponseBody();
        responseBody.Should().Contain("\"innerException\"");
        responseBody.Should().Contain("ArgumentNullException");
    }

    #endregion

    #region Helper Methods

    private string GetResponseBody()
    {
        _responseStream.Position = 0;
        using var reader = new StreamReader(_responseStream, Encoding.UTF8, leaveOpen: true);
        return reader.ReadToEnd();
    }

    #endregion
}
