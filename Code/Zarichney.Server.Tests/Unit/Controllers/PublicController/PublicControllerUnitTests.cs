using Zarichney.Services.Status;
using Zarichney.Services.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Helpers;
using Zarichney.Controllers.Responses;

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

  public PublicControllerUnitTests()
  {
    _mockStatusService = new Mock<IStatusService>();
    _mockLoggingService = new Mock<ILoggingService>();
    _controller = new Zarichney.Controllers.PublicController(_mockStatusService.Object, _mockLoggingService.Object);
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
}
