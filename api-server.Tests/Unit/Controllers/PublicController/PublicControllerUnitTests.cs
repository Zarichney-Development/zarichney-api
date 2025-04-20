using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Controllers;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes; // Assuming TestCategories is here

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
    private readonly Mock<ILogger<Zarichney.Controllers.PublicController>> _mockLogger; // Although not used by current methods, include for completeness
    private readonly Zarichney.Controllers.PublicController _controller;

    public PublicControllerUnitTests()
    {
        _mockStatusService = new Mock<IStatusService>();
        _mockLogger = new Mock<ILogger<Zarichney.Controllers.PublicController>>(); // Mock logger dependency
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
        var value = okResult?.Value;
        value.Should().NotBeNull();

        // Check for Success property
        var successProperty = value?.GetType().GetProperty("Success");
        successProperty.Should().NotBeNull(because: "the response should have a 'Success' property");
        successProperty?.GetValue(value).Should().Be(true, because: "the 'Success' property should be true");

        // Check for Time property
        var timeProperty = value?.GetType().GetProperty("Time");
        timeProperty.Should().NotBeNull(because: "the response should have a 'Time' property");
        timeProperty?.PropertyType.Should().Be(typeof(DateTime), because: "the 'Time' property should be a DateTime");
    }

    /// <summary>
    /// Verifies that GetConfigurationStatus returns an OK result with the status list
    /// when the IStatusService succeeds.
    /// Corresponds to README TODOs:
    /// - Test `_statusService.GetPublicStatusAsync` (or similar) called. (Adapted for GetConfigurationStatusAsync)
    /// - Test handling successful service result -> verify `OkObjectResult` with status data.
    /// </summary>
    [Fact]
    public async Task GetConfigurationStatus_WhenServiceSucceeds_ReturnsOkWithStatusList()
    {
        // Arrange
        var expectedStatusList = new List<ConfigurationItemStatus>
        {
            new("Test Item 1", "Configured", null),
            new("Test Item 2", "Missing/Invalid", "Missing value")
        };
        _mockStatusService
            .Setup(s => s.GetConfigurationStatusAsync())
            .ReturnsAsync(expectedStatusList)
            .Verifiable(); // Removed 'because' parameter

        // Act
        var result = await _controller.GetConfigurationStatus();

        // Assert
        result.Should().BeOfType<OkObjectResult>(because: "a successful service call should return an OK status with content");
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult?.Value.Should().BeEquivalentTo(expectedStatusList, because: "the returned value should match the list from the service");

        _mockStatusService.Verify(); // Verify the setup was called
    }

    /// <summary>
    /// Verifies that GetConfigurationStatus propagates exceptions thrown by the IStatusService.
    /// Corresponds to README TODO: Test handling failure/exception from service -> verify appropriate error ActionResult.
    /// (Note: In unit tests, we verify the exception propagates; middleware handles the final ActionResult in integration).
    /// </summary>
    [Fact]
    public async Task GetConfigurationStatus_WhenServiceThrows_ThrowsException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Service failure simulation");
        _mockStatusService
            .Setup(s => s.GetConfigurationStatusAsync())
            .ThrowsAsync(expectedException)
            .Verifiable(); // Removed 'because' parameter

        // Act
        Func<Task> act = async () => await _controller.GetConfigurationStatus();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>(because: "exceptions from the service should propagate")
                 .WithMessage(expectedException.Message);

        _mockStatusService.Verify(); // Verify the setup was called
    }
}