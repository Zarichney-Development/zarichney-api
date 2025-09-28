using Zarichney.Services.Status;
using Zarichney.Services.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Zarichney.Server.Tests.Framework.Attributes;

namespace Zarichney.Tests.Unit.Controllers.PublicController;

/// <summary>
/// Unit tests for the <see cref="Zarichney.Controllers.PublicController"/>.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Component, TestCategories.Controller)]
public class PublicControllerTests
{
  private readonly Mock<IStatusService> _mockStatusService;
  private readonly Mock<ILoggingService> _mockLoggingService;
  private readonly Zarichney.Controllers.PublicController _controller;

  public PublicControllerTests()
  {
    _mockStatusService = new Mock<IStatusService>();
    _mockLoggingService = new Mock<ILoggingService>();
    _controller = new Zarichney.Controllers.PublicController(_mockStatusService.Object, _mockLoggingService.Object);
  }

  /// <summary>
  /// Verifies that the HealthCheck action returns an OK result with the expected structure.
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

    // Use reflection to check anonymous type properties
    var value = okResult.Value;
    value.Should().NotBeNull();

    // Check for Success property
    var successProperty = value.GetType().GetProperty("Success");
    successProperty.Should().NotBeNull(because: "the response should have a 'Success' property");
    successProperty.GetValue(value).Should().Be(true, because: "the 'Success' property should be true");

    // Check for Time property
    var timeProperty = value.GetType().GetProperty("Time");
    timeProperty.Should().NotBeNull(because: "the response should have a 'Time' property");
    timeProperty.PropertyType.Should().Be(typeof(DateTime), because: "the 'Time' property should be a DateTime");
  }

  /// <summary>
  /// Verifies that GetServicesStatus returns an OK result with the service status list
  /// when the IStatusService succeeds.
  /// </summary>
  [Fact]
  public async Task GetServicesStatus_WhenServiceSucceeds_ReturnsOkWithServiceStatusList()
  {
    // Arrange
    var expectedServiceStatus = new Dictionary<ExternalServices, ServiceStatusInfo>
    {
      [ExternalServices.OpenAiApi] = new(ExternalServices.OpenAiApi, true, []),
      [ExternalServices.MailCheck] = new(ExternalServices.MailCheck, false, ["EmailConfig:ApiKey"])
    };

    var expectedList = expectedServiceStatus.Values.ToList();

    _mockStatusService
        .Setup(s => s.GetServiceStatusAsync())
        .ReturnsAsync(expectedServiceStatus)
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
    Func<Task> act = async () => await _controller.GetServicesStatus(); // Changed from GetStatus()

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>(because: "exceptions from the service should propagate")
        .WithMessage(expectedException.Message);

    _mockStatusService.Verify();
  }

  // New tests for Config() endpoint
  /// <summary>
  /// Verifies that Config returns an OK result with the configuration item status
  /// when the IStatusService succeeds.
  /// </summary>
  [Fact]
  public async Task Config_WhenServiceSucceeds_ReturnsOkWithConfigurationItemStatus()
  {
    // Arrange
    var expectedConfigStatus = new List<ConfigurationItemStatus>
        {
            new("FeatureA", "Enabled"),
            new("FeatureB", "Disabled", "Missing license")
        };

    _mockStatusService
        .Setup(s => s.GetConfigurationStatusAsync()) // Assuming this method exists on IStatusService
        .ReturnsAsync(expectedConfigStatus)
        .Verifiable();

    // Act
    var result = await _controller.Config();

    // Assert
    result.Should()
        .BeOfType<OkObjectResult>(because: "a successful service call should return an OK status with content");
    var okResult = result as OkObjectResult;
    okResult.Should().NotBeNull();
    okResult.Value.Should().BeEquivalentTo(expectedConfigStatus,
        because: "the returned value should match the configuration status from the service");

    _mockStatusService.Verify();
  }

  /// <summary>
  /// Verifies that Config propagates exceptions thrown by the IStatusService.
  /// </summary>
  [Fact]
  public async Task Config_WhenServiceThrows_ThrowsException()
  {
    // Arrange
    var expectedException = new InvalidOperationException("Service failure simulation for config");
    _mockStatusService
        .Setup(s => s.GetConfigurationStatusAsync()) // Assuming this method exists on IStatusService
        .ThrowsAsync(expectedException)
        .Verifiable();

    // Act
    Func<Task> act = async () => await _controller.Config();

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>(because: "exceptions from the service should propagate")
        .WithMessage(expectedException.Message);

    _mockStatusService.Verify();
  }
}
