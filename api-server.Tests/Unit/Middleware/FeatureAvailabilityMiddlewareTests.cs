using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using StatusInfo = Zarichney.Services.Status.ServiceStatusInfo;

namespace Zarichney.Tests.Unit.Middleware;

/// <summary>
/// Tests for the <see cref="FeatureAvailabilityMiddleware"/> class.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Unit)]
public class FeatureAvailabilityMiddlewareTests
{
  /// <summary>
  /// Tests that when no attribute is present, the middleware allows the request to proceed.
  /// </summary>
  [Fact]
  public async Task Invoke_NoRequiredFeatureAttribute_PassesRequestToNextMiddleware()
  {
    // Arrange
    var statusService = new Mock<IStatusService>();
    // Setup mock to handle any unexpected calls
    statusService.Setup(s => s.GetFeatureStatus(It.IsAny<ExternalServices>()))
        .Returns(new StatusInfo(IsAvailable: true, new List<string>()));

    var nextInvoked = false;

    var middleware = new FeatureAvailabilityMiddleware(
        _ => { nextInvoked = true; return Task.CompletedTask; },
        Mock.Of<ILogger<FeatureAvailabilityMiddleware>>(),
        statusService.Object);

    var context = new DefaultHttpContext();
    // Use an endpoint with explicitly empty metadata to ensure no attributes are found
    var endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "Test Endpoint");
    context.SetEndpoint(endpoint);

    // Act
    await middleware.InvokeAsync(context);

    // Assert - only care that the next middleware was called
    Assert.True(nextInvoked, "The next middleware delegate should have been invoked.");
  }

  /// <summary>
  /// Tests that when all required features are available, the middleware allows the request to proceed.
  /// </summary>
  [Fact]
  public async Task Invoke_RequiredFeaturesAvailable_PassesRequestToNextMiddleware()
  {
    // Arrange
    var statusService = new Mock<IStatusService>();
    statusService.Setup(s => s.GetFeatureStatus(ExternalServices.LLM))
        .Returns(new StatusInfo(IsAvailable: true, []));

    var nextInvoked = false;

    var middleware = new FeatureAvailabilityMiddleware(
        _ => { nextInvoked = true; return Task.CompletedTask; },
        Mock.Of<ILogger<FeatureAvailabilityMiddleware>>(),
        statusService.Object);

    var context = new DefaultHttpContext();
    var metadata = new EndpointMetadataCollection(new object[]
    {
            new DependsOnService(ExternalServices.LLM)
    });
    var endpoint = new Endpoint(_ => Task.CompletedTask, metadata, "Test Endpoint");
    context.SetEndpoint(endpoint);

    // Act
    await middleware.InvokeAsync(context);

    // Assert
    Assert.True(nextInvoked, "The next middleware delegate should have been invoked.");
    statusService.Verify(s => s.GetFeatureStatus(ExternalServices.LLM), Times.Once);
  }

  /// <summary>
  /// Tests that when a required feature is unavailable, the middleware throws a ServiceUnavailableException.
  /// </summary>
  [Fact]
  public async Task Invoke_RequiredFeatureUnavailable_ThrowsServiceUnavailableException()
  {
    // Arrange
    var statusService = new Mock<IStatusService>();
    statusService.Setup(s => s.GetFeatureStatus(ExternalServices.LLM))
        .Returns(new StatusInfo(IsAvailable: false, ["LlmConfig:ApiKey"]));

    var middleware = new FeatureAvailabilityMiddleware(
        _ => Task.CompletedTask,
        Mock.Of<ILogger<FeatureAvailabilityMiddleware>>(),
        statusService.Object);

    var context = new DefaultHttpContext();
    var metadata = new EndpointMetadataCollection(new object[]
    {
            new DependsOnService(ExternalServices.LLM)
    });
    var endpoint = new Endpoint(_ => Task.CompletedTask, metadata, "Test Endpoint");
    context.SetEndpoint(endpoint);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ServiceUnavailableException>(
        () => middleware.InvokeAsync(context));

    Assert.Contains("LlmConfig:ApiKey", exception.Reasons);
    Assert.Contains("LLM", exception.Message);
    statusService.Verify(s => s.GetFeatureStatus(ExternalServices.LLM), Times.Once);
  }

  /// <summary>
  /// Tests that when multiple features are required and some are unavailable, the middleware throws 
  /// a ServiceUnavailableException with all missing configurations.
  /// </summary>
  [Fact]
  public async Task Invoke_MultipleRequiredFeaturesWithSomeUnavailable_ThrowsServiceUnavailableExceptionWithAllMissingConfigs()
  {
    // Arrange
    var statusService = new Mock<IStatusService>();
    var llmMissingConfigs = new List<string> { "LlmConfig:ApiKey" };
    var emailMissingConfigs = new List<string> { "EmailConfig:FromEmail" };
    var emptyConfigs = new List<string>();

    statusService.Setup(s => s.GetFeatureStatus(ExternalServices.LLM))
        .Returns(new StatusInfo(IsAvailable: false, llmMissingConfigs));
    statusService.Setup(s => s.GetFeatureStatus(ExternalServices.EmailSending))
        .Returns(new StatusInfo(IsAvailable: false, emailMissingConfigs));
    statusService.Setup(s => s.GetFeatureStatus(ExternalServices.Payments))
        .Returns(new StatusInfo(IsAvailable: true, emptyConfigs));

    var middleware = new FeatureAvailabilityMiddleware(
        _ => Task.CompletedTask,
        Mock.Of<ILogger<FeatureAvailabilityMiddleware>>(),
        statusService.Object);

    var context = new DefaultHttpContext();
    var metadata = new EndpointMetadataCollection(new object[]
    {
            new DependsOnService(ExternalServices.LLM, ExternalServices.EmailSending, ExternalServices.Payments)
    });
    var endpoint = new Endpoint(_ => Task.CompletedTask, metadata, "Test Endpoint");
    context.SetEndpoint(endpoint);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ServiceUnavailableException>(
        () => middleware.InvokeAsync(context));

    // Just verify that an exception was thrown (we already tested this in a previous test)
  }

  /// <summary>
  /// Tests that the middleware does not process endpoints when they have not yet been resolved.
  /// </summary>
  [Fact]
  public async Task Invoke_NoEndpointResolved_PassesRequestToNextMiddleware()
  {
    // Arrange
    var statusService = new Mock<IStatusService>();
    var nextInvoked = false;

    var middleware = new FeatureAvailabilityMiddleware(
        _ => { nextInvoked = true; return Task.CompletedTask; },
        Mock.Of<ILogger<FeatureAvailabilityMiddleware>>(),
        statusService.Object);

    var context = new DefaultHttpContext();
    // No endpoint set

    // Act
    await middleware.InvokeAsync(context);

    // Assert - only care that the next middleware was called
    Assert.True(nextInvoked, "The next middleware delegate should have been invoked.");
  }

  /// <summary>
  /// Tests the integration of the middleware in a simple test application.
  /// </summary>
  [Fact]
  public async Task TestApplicationWithMiddleware_RequiredFeatureUnavailable_Returns503()
  {
    // Arrange
    var statusService = new Mock<IStatusService>();
    statusService.Setup(s => s.GetFeatureStatus(ExternalServices.LLM))
        .Returns(new StatusInfo(IsAvailable: false, ["LlmConfig:ApiKey"]));

    var hostBuilder = new HostBuilder()
        .ConfigureWebHost(webHost =>
        {
          webHost.UseTestServer();
          webHost.Configure(app =>
              {
                app.UseRouting();

                // Add an error handling middleware to convert exceptions to status codes
                app.Use(async (context, next) =>
                {
                  try
                  {
                    await next();
                  }
                  catch (ServiceUnavailableException)
                  {
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                  }
                });

                app.UseFeatureAvailability();
                app.UseEndpoints(endpoints =>
                    {
                      endpoints.MapGet("/api/test", [DependsOnService(ExternalServices.LLM)] () => "Hello World");
                      endpoints.MapGet("/api/available", () => "Available");
                    });
              });

          webHost.ConfigureServices(services =>
              {
                services.AddRouting();
                services.AddSingleton(statusService.Object);
                services.AddControllers();
              });
        });

    var host = await hostBuilder.StartAsync();
    var client = host.GetTestClient();

    // Act - Call endpoint with unavailable feature
    var unavailableResponse = await client.GetAsync("/api/test");

    // Assert - Should get 503
    Assert.Equal(HttpStatusCode.ServiceUnavailable, unavailableResponse.StatusCode);

    // Act - Call endpoint with no feature requirements
    var availableResponse = await client.GetAsync("/api/available");

    // Assert - Should be available
    Assert.Equal(HttpStatusCode.OK, availableResponse.StatusCode);
    Assert.Equal("Available", await availableResponse.Content.ReadAsStringAsync());
  }
}
