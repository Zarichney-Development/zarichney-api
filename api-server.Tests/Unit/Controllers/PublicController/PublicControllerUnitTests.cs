using Zarichney.Services.Status;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Zarichney.TestingFramework.Attributes;
using Zarichney.TestingFramework.Helpers;

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
  private readonly Zarichney.Controllers.PublicController _controller;

  public PublicControllerUnitTests()
  {
    _mockStatusService = new Mock<IStatusService>();
    _controller = new Zarichney.Controllers.PublicController(_mockStatusService.Object);
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

    // Use reflection or dynamic to check anonymous type properties
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
}
