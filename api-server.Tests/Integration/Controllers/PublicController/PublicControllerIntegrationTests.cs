using FluentAssertions;
using Refit;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Controllers.PublicController;

/// <summary>
/// Integration tests for the PublicController.
/// These tests verify the behavior of endpoints accessible without authentication,
/// ensuring they function correctly within the ASP.NET Core pipeline.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Component, TestCategories.Controller)]
[Trait(TestCategories.Feature, "Public")]
[Collection("Integration")]
public class PublicControllerIntegrationTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
  : IntegrationTestBase(apiClientFixture, testOutputHelper)
{
  /// <summary>
  /// Tests that the health check endpoint returns an OK result with the expected structure.
  /// This test should run even when configuration is missing.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  [LogTestStartEnd]
  public async Task GetHealth_WhenCalled_ReturnsOkStatus()
  {
    using (CreateTestMethodContext(nameof(GetHealth_WhenCalled_ReturnsOkStatus)))
    {
      // Arrange
      // ApiClient is obtained from IntegrationTestBase, represents unauthenticated Refit client

      // Act & Assert: calling Health should not throw and thus return 200 OK
      var act = () => _apiClientFixture.UnauthenticatedPublicApi.Health();
      await act.Should().NotThrowAsync<ApiException>(
        because: "health endpoint should always return OK status even without configuration");
    }
  }

  /// <summary>
  /// Tests that the service status endpoint returns an OK result with the service status information.
  /// This corresponds to the GetConfigurationStatus method in PublicController, mapped to /api/status.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  [LogTestStartEnd]
  public async Task
    GetServiceStatus_WhenCalled_ReturnsServiceStatusInfo() // Renamed from GetStatus_WhenCalled_ReturnsServiceStatusInfo
  {
    // Arrange
    // Get all enum values as strings to avoid hard-coding service names
    var expectedServices = Enum.GetValues(typeof(ExternalServices))
                               .Cast<ExternalServices>()
                               .Select(e => e.ToString())
                               .ToArray();

    // Act
    var serviceStatus = await _apiClientFixture.UnauthenticatedPublicApi.StatusAll();

    // Assert
    serviceStatus.Should().NotBeNull(
      because: "response should contain service status information");

    serviceStatus.Should().NotBeEmpty(
      because: "response should contain at least some service status information");

    // Check that all the expected service enums are present in the response
    var serviceNames = serviceStatus.Select(s => s.ServiceName.ToString()).ToList();
    serviceNames.Should().Contain(expectedServices,
      because: "the response should contain status for all critical services");

    foreach (var status in serviceStatus)
    {
      status.Should().NotBeNull(
        because: $"status for {status.ServiceName} should not be null");

      if (!status.IsAvailable)
      {
        status.MissingConfigurations.Should().NotBeEmpty(
          because: $"unavailable service {status.ServiceName} should have missing configurations");
      }
      else
      {
        status.MissingConfigurations.Should().BeEmpty(
          because: $"available service {status.ServiceName} should not have missing configurations");
      }
    }
  }

  /// <summary>
  /// Tests that the configuration status endpoint (/api/config) returns an OK result with configuration item status information.
  /// This corresponds to the Config method in PublicController.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task GetConfig_WhenCalled_ReturnsConfigurationItemStatus()
  {
    // Arrange
    // We don't know the exact items without deeper knowledge of IStatusService implementation,
    // but we can check the structure and that it's not empty if successful.

    // Act
    var configStatus = await _apiClientFixture.UnauthenticatedPublicApi.Config();

    // Assert
    configStatus.Should().NotBeNull(
      because: "response should contain configuration item status information");

    // This assertion might be too strict if the list can be legitimately empty.
    // For now, assuming it should return some items if the service is working.
    configStatus.Should().NotBeEmpty(
      because: "response should contain at least some configuration item status information");

    foreach (var item in configStatus)
    {
      item.Should().NotBeNull(because: "each configuration item should not be null");
      item.Name.Should().NotBeNullOrWhiteSpace(because: "each item should have a name");
    }
  }
}
