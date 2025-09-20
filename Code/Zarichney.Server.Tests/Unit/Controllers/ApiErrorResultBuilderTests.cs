using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Zarichney.Controllers;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Server.Tests.Unit.Controllers;

/// <summary>
/// Unit tests for ApiErrorResult using the builder pattern - demonstrates builder usage and additional scenarios
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Controllers")]
public class ApiErrorResultBuilderTests : IDisposable
{
  private readonly IServiceProvider _serviceProvider;
  private bool _disposed;

  public ApiErrorResultBuilderTests()
  {
    _serviceProvider = CreateServiceProvider();
  }

  private static IServiceProvider CreateServiceProvider()
  {
    var services = new ServiceCollection();

    // Add required services for JsonResult to work
    services.AddLogging();
    services.AddOptions();
    services.AddControllers()
        .AddJsonOptions(options =>
        {
          options.JsonSerializerOptions.WriteIndented = true;
          options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

    return services.BuildServiceProvider();
  }

  #region Builder Pattern Tests

  [Fact]
  public async Task Builder_WithBadRequest_CreatesBadRequestError()
  {
    // Arrange
    var result = new ApiErrorResultBuilder()
        .WithBadRequest("Invalid input provided")
        .Build();
    var (actionContext, _) = CreateTestActionContext();

    // Act
    await result.ExecuteResultAsync(actionContext);

    // Assert
    actionContext.HttpContext.Response.StatusCode.Should().Be(400, "because it's a bad request");
    var responseBody = await GetJsonResultContent(result);
    responseBody.Should().Contain("\"message\": \"Invalid input provided\"");
  }

  [Fact]
  public async Task Builder_WithNotFound_CreatesNotFoundError()
  {
    // Arrange
    var result = new ApiErrorResultBuilder()
        .WithNotFound("User not found")
        .Build();
    var (actionContext, _) = CreateTestActionContext();

    // Act
    await result.ExecuteResultAsync(actionContext);

    // Assert
    actionContext.HttpContext.Response.StatusCode.Should().Be(404, "because it's a not found error");
    var responseBody = await GetJsonResultContent(result);
    responseBody.Should().Contain("\"message\": \"User not found\"");
  }

  [Fact]
  public async Task Builder_WithUnauthorized_CreatesUnauthorizedError()
  {
    // Arrange
    var result = new ApiErrorResultBuilder()
        .WithUnauthorized("Please login to continue")
        .Build();
    var (actionContext, _) = CreateTestActionContext();

    // Act
    await result.ExecuteResultAsync(actionContext);

    // Assert
    actionContext.HttpContext.Response.StatusCode.Should().Be(401, "because it's an unauthorized error");
    var responseBody = await GetJsonResultContent(result);
    responseBody.Should().Contain("\"message\": \"Please login to continue\"");
  }

  [Fact]
  public async Task Builder_WithForbidden_CreatesForbiddenError()
  {
    // Arrange
    var result = new ApiErrorResultBuilder()
        .WithForbidden("You don't have permission to access this resource")
        .Build();
    var (actionContext, _) = CreateTestActionContext();

    // Act
    await result.ExecuteResultAsync(actionContext);

    // Assert
    actionContext.HttpContext.Response.StatusCode.Should().Be(403, "because it's a forbidden error");
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
    var responseBody = await GetJsonResultContent(result);

    // Assert
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
    var responseBody = await GetJsonResultContent(result);

    // Assert
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
    var (actionContext, _) = CreateTestActionContext();

    // Act
    await result.ExecuteResultAsync(actionContext);

    // Assert
    actionContext.HttpContext.Response.StatusCode.Should().Be(500);
    var responseBody = await GetJsonResultContent(result);
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
    var (actionContext, _) = CreateTestActionContext();

    // Act
    await result.ExecuteResultAsync(actionContext);

    // Assert
    actionContext.HttpContext.Response.StatusCode.Should().Be(503);
    var responseBody = await GetJsonResultContent(result);
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
    var (actionContext, _) = CreateTestActionContext();

    // Act
    await result.ExecuteResultAsync(actionContext);

    // Assert
    actionContext.HttpContext.Response.StatusCode.Should().Be(500);
    var responseBody = await GetJsonResultContent(result);
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
    var response1 = await GetJsonResultContent(result1);
    var response2 = await GetJsonResultContent(result2);

    var (actionContext, _) = CreateTestActionContext();
    await result2.ExecuteResultAsync(actionContext);

    // Assert
    response1.Should().Contain("First error");
    response2.Should().Contain("Second error");
    actionContext.HttpContext.Response.StatusCode.Should().Be(404, "because the second build used NotFound");
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
    var responseBody = await GetJsonResultContent(result);

    // Assert
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
    var responseBody = await GetJsonResultContent(result);

    // Assert
    responseBody.Should().Contain("\"innerException\"");
    responseBody.Should().Contain("ArgumentNullException");
  }

  #endregion

  #region Helper Methods

  private (ActionContext actionContext, MemoryStream responseStream) CreateTestActionContext(
      string requestPath = "/api/test",
      string requestMethod = "POST",
      string? controllerDisplayName = "TestController.TestAction",
      string traceId = "test-trace-id")
  {
    var httpContext = new DefaultHttpContext();
    httpContext.RequestServices = _serviceProvider;
    httpContext.TraceIdentifier = traceId;
    httpContext.Request.Path = new PathString(requestPath);
    httpContext.Request.Method = requestMethod;

    var responseStream = new MemoryStream();
    httpContext.Response.Body = responseStream;

    var actionContext = new ActionContext(
        httpContext,
        new RouteData(),
        new ActionDescriptor { DisplayName = controllerDisplayName }
    );

    return (actionContext, responseStream);
  }

  private async Task<string> GetJsonResultContent(ApiErrorResult result, ActionContext? customActionContext = null)
  {
    ActionContext actionContext;
    MemoryStream responseStream;

    if (customActionContext != null)
    {
      actionContext = customActionContext;
      responseStream = (MemoryStream)actionContext.HttpContext.Response.Body;
    }
    else
    {
      (actionContext, responseStream) = CreateTestActionContext();
    }

    // Execute the result
    await result.ExecuteResultAsync(actionContext);

    // Read the JSON content
    responseStream.Position = 0;
    using var reader = new StreamReader(responseStream, Encoding.UTF8);
    return await reader.ReadToEndAsync();
  }

  #endregion

  #region IDisposable Implementation

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
      {
        (_serviceProvider as IDisposable)?.Dispose();
      }
      _disposed = true;
    }
  }

  #endregion
}
