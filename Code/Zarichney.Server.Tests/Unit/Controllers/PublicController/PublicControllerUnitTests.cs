using Zarichney.Services.Status;
using Zarichney.Services.Logging;
using Zarichney.Services.Logging.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using AutoFixture;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Helpers;
using Zarichney.Controllers.Responses;
using Zarichney.Controllers;

namespace Zarichney.Tests.Unit.Controllers.PublicController;

/// <summary>
/// Unit tests for the <see cref="Zarichney.Controllers.PublicController"/>.
/// These tests focus on the controller's internal logic in isolation, mocking dependencies.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Component, TestCategories.Controller)]
public class PublicControllerUnitTests
{
  private readonly Mock<IStatusService> _mockStatusService;
  private readonly Mock<ILoggingService> _mockLoggingService;
  private readonly Zarichney.Controllers.PublicController _controller;
  private readonly IFixture _fixture;

  public PublicControllerUnitTests()
  {
    _mockStatusService = new Mock<IStatusService>();
    _mockLoggingService = new Mock<ILoggingService>();
    _controller = new Zarichney.Controllers.PublicController(_mockStatusService.Object, _mockLoggingService.Object);
    _fixture = new Fixture();

    // Setup HttpContext for controller
    var httpContext = new DefaultHttpContext();
    _controller.ControllerContext = new ControllerContext
    {
      HttpContext = httpContext
    };
  }

  /// <summary>
  /// Verifies that the HealthCheck action returns an OK result with the expected structure.
  /// Corresponds to README TODO: Test action returns OkObjectResult or OkResult with the expected value/status code.
  /// </summary>
  [Fact]
  public void HealthCheck_Always_ReturnsOkWithSuccessAndTime()
  {
    // Arrange
    // No arrangement needed as HealthCheck has no dependencies

    // Act
    var result = _controller.HealthCheck();

    // Assert
    result.Should().BeOfType<OkObjectResult>(because: "the health check should return an OK status with content");
    var okResult = result as OkObjectResult;
    okResult.Should().NotBeNull();

    // Now using strongly typed DTO instead of reflection
    var healthCheckResponse = okResult.Value as HealthCheckResponse;
    healthCheckResponse.Should().NotBeNull(because: "the response should be a HealthCheckResponse");

    // Verify all properties
    healthCheckResponse.Success.Should().BeTrue(because: "the health check should indicate success");
    healthCheckResponse.Time.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5),
      because: "the time should be approximately the current time");
    healthCheckResponse.Environment.Should().NotBeNullOrWhiteSpace(
      because: "the environment should be specified");
  }

  /// <summary>
  /// Verifies that GetServicesStatus returns an OK result with the status list
  /// when the IStatusService succeeds.
  /// </summary>
  [Fact]
  public async Task GetServicesStatus_WhenServiceSucceeds_ReturnsOkWithStatusList()
  {
    // Arrange
    var service1 = ExternalServices.OpenAiApi;
    var service2 = ExternalServices.MsGraph;

    var expectedStatusDict = new Dictionary<ExternalServices, ServiceStatusInfo>
    {
      { service1, new ServiceStatusInfo(service1, true, []) },
      { service2, new ServiceStatusInfo(service2, false, [GetRandom.String()]) }
    };

    var expectedList = expectedStatusDict.Values.ToList();

    _mockStatusService
      .Setup(s => s.GetServiceStatusAsync())
      .ReturnsAsync(expectedStatusDict)
      .Verifiable();

    // Act
    var result = await _controller.GetServicesStatus();

    // Assert
    result.Should()
      .BeOfType<OkObjectResult>(because: "a successful service call should return an OK status with content");
    var okResult = result as OkObjectResult;
    okResult.Should().NotBeNull();
    okResult.Value.Should().BeEquivalentTo(expectedList,
      because: "the returned value should match the list of service statuses from the service");

    _mockStatusService.Verify();
  }

  /// <summary>
  /// Verifies that GetServicesStatus propagates exceptions thrown by the IStatusService.
  /// </summary>
  [Fact]
  public async Task GetServicesStatus_WhenServiceThrows_ThrowsException()
  {
    // Arrange
    var expectedException = new InvalidOperationException("Service failure simulation");
    _mockStatusService
      .Setup(s => s.GetServiceStatusAsync())
      .ThrowsAsync(expectedException)
      .Verifiable();

    // Act
    Func<Task> act = async () => await _controller.GetServicesStatus();

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>(because: "exceptions from the service should propagate")
      .WithMessage(expectedException.Message);

    _mockStatusService.Verify();
  }

  /// <summary>
  /// Verifies that Config returns an OK result with the configuration status list
  /// when the IStatusService succeeds.
  /// </summary>
  [Fact]
  public async Task Config_WhenServiceSucceeds_ReturnsOkWithConfigurationStatus()
  {
    // Arrange
    var expectedStatus = new List<ConfigurationItemStatus>
    {
      new ConfigurationItemStatus("Setting1", "Configured", "Value1"),
      new ConfigurationItemStatus("Setting2", "Missing", null)
    };

    _mockStatusService
      .Setup(s => s.GetConfigurationStatusAsync())
      .ReturnsAsync(expectedStatus)
      .Verifiable();

    // Act
    var result = await _controller.Config();

    // Assert
    result.Should()
      .BeOfType<OkObjectResult>(because: "a successful service call should return an OK status with content");
    var okResult = result as OkObjectResult;
    okResult.Should().NotBeNull();
    okResult.Value.Should().BeEquivalentTo(expectedStatus,
      because: "the returned value should match the configuration status list from the service");

    _mockStatusService.Verify();
  }

  /// <summary>
  /// Verifies that Config propagates exceptions thrown by the IStatusService.
  /// </summary>
  [Fact]
  public async Task Config_WhenServiceThrows_ThrowsException()
  {
    // Arrange
    var expectedException = new InvalidOperationException("Configuration service failure");
    _mockStatusService
      .Setup(s => s.GetConfigurationStatusAsync())
      .ThrowsAsync(expectedException)
      .Verifiable();

    // Act
    Func<Task> act = async () => await _controller.Config();

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>(
      because: "exceptions from the service should propagate")
      .WithMessage(expectedException.Message);

    _mockStatusService.Verify();
  }

  #region Logging Endpoint Tests

  /// <summary>
  /// Verifies that GetLoggingStatus returns an OK result with the logging status
  /// when the ILoggingService succeeds.
  /// </summary>
  [Fact]
  public async Task GetLoggingStatus_WhenServiceSucceeds_ReturnsOkWithLoggingStatus()
  {
    // Arrange
    var expectedStatus = _fixture.Create<LoggingStatusResult>();

    _mockLoggingService
      .Setup(s => s.GetLoggingStatusAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedStatus)
      .Verifiable();

    // Act
    var result = await _controller.GetLoggingStatus();

    // Assert
    result.Should()
      .BeOfType<OkObjectResult>(because: "a successful service call should return an OK status with content");
    var okResult = result as OkObjectResult;
    okResult.Should().NotBeNull();
    okResult!.Value.Should().BeEquivalentTo(expectedStatus,
      because: "the returned value should match the logging status from the service");

    _mockLoggingService.Verify();
  }

  /// <summary>
  /// Verifies that GetLoggingStatus returns a 500 status code when the service throws an exception.
  /// </summary>
  [Fact]
  public async Task GetLoggingStatus_WhenServiceThrows_Returns500WithError()
  {
    // Arrange
    var expectedException = new InvalidOperationException("Logging service failure");
    _mockLoggingService
      .Setup(s => s.GetLoggingStatusAsync(It.IsAny<CancellationToken>()))
      .ThrowsAsync(expectedException)
      .Verifiable();

    // Act
    var result = await _controller.GetLoggingStatus();

    // Assert
    result.Should().BeOfType<ObjectResult>(because: "an error should return an object result");
    var objectResult = result as ObjectResult;
    objectResult.Should().NotBeNull();
    objectResult!.StatusCode.Should().Be(500, because: "service failure should return 500 status");

    // Verify error response structure
    var value = objectResult.Value;
    value.Should().NotBeNull();
    value!.GetType().GetProperty("error")?.GetValue(value).Should().Be("Failed to retrieve logging status");
    value.GetType().GetProperty("details")?.GetValue(value).Should().Be(expectedException.Message);

    _mockLoggingService.Verify();
  }

  /// <summary>
  /// Verifies that TestSeqConnectivity returns an OK result with connectivity results
  /// when provided with a URL in the request body.
  /// </summary>
  [Fact]
  public async Task TestSeqConnectivity_WithUrlInRequest_ReturnsOkWithConnectivityResult()
  {
    // Arrange
    var testUrl = "http://test.seq:5341";
    var request = new TestSeqRequest { Url = testUrl };
    var expectedResult = _fixture.Create<SeqConnectivityResult>();

    _mockLoggingService
      .Setup(s => s.TestSeqConnectivityAsync(testUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult)
      .Verifiable();

    // Act
    var result = await _controller.TestSeqConnectivity(request);

    // Assert
    result.Should()
      .BeOfType<OkObjectResult>(because: "a successful test should return an OK status with content");
    var okResult = result as OkObjectResult;
    okResult.Should().NotBeNull();
    okResult!.Value.Should().BeEquivalentTo(expectedResult,
      because: "the returned value should match the connectivity result from the service");

    _mockLoggingService.Verify();
  }

  /// <summary>
  /// Verifies that TestSeqConnectivity uses null URL when no request body is provided.
  /// </summary>
  [Fact]
  public async Task TestSeqConnectivity_WithNullRequest_UsesNullUrl()
  {
    // Arrange
    var expectedResult = _fixture.Create<SeqConnectivityResult>();

    _mockLoggingService
      .Setup(s => s.TestSeqConnectivityAsync(null, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult)
      .Verifiable();

    // Act
    var result = await _controller.TestSeqConnectivity(null);

    // Assert
    result.Should()
      .BeOfType<OkObjectResult>(because: "a successful test should return an OK status with content");
    var okResult = result as OkObjectResult;
    okResult.Should().NotBeNull();
    okResult!.Value.Should().BeEquivalentTo(expectedResult,
      because: "the returned value should match the connectivity result from the service");

    _mockLoggingService.Verify(
      s => s.TestSeqConnectivityAsync(null, It.IsAny<CancellationToken>()),
      Times.Once,
      "should call the service with null URL when no request is provided");
  }

  /// <summary>
  /// Verifies that TestSeqConnectivity handles empty request body correctly.
  /// </summary>
  [Fact]
  public async Task TestSeqConnectivity_WithEmptyRequest_UsesNullUrl()
  {
    // Arrange
    var request = new TestSeqRequest { Url = null };
    var expectedResult = _fixture.Create<SeqConnectivityResult>();

    _mockLoggingService
      .Setup(s => s.TestSeqConnectivityAsync(null, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult)
      .Verifiable();

    // Act
    var result = await _controller.TestSeqConnectivity(request);

    // Assert
    result.Should()
      .BeOfType<OkObjectResult>(because: "a successful test should return an OK status with content");

    _mockLoggingService.Verify(
      s => s.TestSeqConnectivityAsync(null, It.IsAny<CancellationToken>()),
      Times.Once,
      "should call the service with null URL when request URL is null");
  }

  /// <summary>
  /// Verifies that GetAvailableLoggingMethods returns an OK result with logging methods
  /// when the ILoggingService succeeds.
  /// </summary>
  [Fact]
  public async Task GetAvailableLoggingMethods_WhenServiceSucceeds_ReturnsOkWithMethods()
  {
    // Arrange
    var expectedMethods = _fixture.Create<LoggingMethodsResult>();

    _mockLoggingService
      .Setup(s => s.GetAvailableLoggingMethodsAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedMethods)
      .Verifiable();

    // Act
    var result = await _controller.GetAvailableLoggingMethods();

    // Assert
    result.Should()
      .BeOfType<OkObjectResult>(because: "a successful service call should return an OK status with content");
    var okResult = result as OkObjectResult;
    okResult.Should().NotBeNull();
    okResult!.Value.Should().BeEquivalentTo(expectedMethods,
      because: "the returned value should match the logging methods from the service");

    _mockLoggingService.Verify();
  }

  /// <summary>
  /// Verifies that GetAvailableLoggingMethods propagates exceptions thrown by the ILoggingService.
  /// </summary>
  [Fact]
  public async Task GetAvailableLoggingMethods_WhenServiceThrows_ThrowsException()
  {
    // Arrange
    var expectedException = new InvalidOperationException("Logging methods service failure");
    _mockLoggingService
      .Setup(s => s.GetAvailableLoggingMethodsAsync(It.IsAny<CancellationToken>()))
      .ThrowsAsync(expectedException)
      .Verifiable();

    // Act
    Func<Task> act = async () => await _controller.GetAvailableLoggingMethods();

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>(
      because: "exceptions from the service should propagate")
      .WithMessage(expectedException.Message);

    _mockLoggingService.Verify();
  }

  /// <summary>
  /// Verifies that all logging endpoints properly pass the CancellationToken from HttpContext.
  /// </summary>
  [Fact]
  public async Task LoggingEndpoints_ProperlyCancellationTokenPropagation()
  {
    // Arrange
    using var cts = new CancellationTokenSource();
    _controller.ControllerContext.HttpContext.RequestAborted = cts.Token;

    var statusResult = _fixture.Create<LoggingStatusResult>();
    var connectivityResult = _fixture.Create<SeqConnectivityResult>();
    var methodsResult = _fixture.Create<LoggingMethodsResult>();

    _mockLoggingService
      .Setup(s => s.GetLoggingStatusAsync(cts.Token))
      .ReturnsAsync(statusResult);

    _mockLoggingService
      .Setup(s => s.TestSeqConnectivityAsync(It.IsAny<string?>(), cts.Token))
      .ReturnsAsync(connectivityResult);

    _mockLoggingService
      .Setup(s => s.GetAvailableLoggingMethodsAsync(cts.Token))
      .ReturnsAsync(methodsResult);

    // Act
    await _controller.GetLoggingStatus();
    await _controller.TestSeqConnectivity(null);
    await _controller.GetAvailableLoggingMethods();

    // Assert
    _mockLoggingService.Verify(s => s.GetLoggingStatusAsync(cts.Token), Times.Once,
      "GetLoggingStatus should pass the cancellation token");
    _mockLoggingService.Verify(s => s.TestSeqConnectivityAsync(It.IsAny<string?>(), cts.Token), Times.Once,
      "TestSeqConnectivity should pass the cancellation token");
    _mockLoggingService.Verify(s => s.GetAvailableLoggingMethodsAsync(cts.Token), Times.Once,
      "GetAvailableLoggingMethods should pass the cancellation token");
  }

  #endregion
}
