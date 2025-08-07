using FluentAssertions;
using Refit;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Client.Contracts;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using ExternalServices = Zarichney.Services.Status.ExternalServices;

namespace Zarichney.Tests.Integration.Controllers.PublicController;

/// <summary>
/// Integration tests for the PublicController.
/// These tests verify the behavior of endpoints accessible without authentication,
/// ensuring they function correctly within the ASP.NET Core pipeline.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Component, TestCategories.Controller)]
[Trait(TestCategories.Feature, "Public")]
[Collection("IntegrationCore")]
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
    var statusResponse = await _apiClientFixture.UnauthenticatedPublicApi.StatusAll();
    var serviceStatuses = statusResponse.Content;

    // Assert
    statusResponse.Should().NotBeNull(
      because: "response should contain service status information");

    serviceStatuses.Should().NotBeEmpty(
      because: "response should contain at least some service status information");

    // Check that all the expected service enums are present in the response
    var serviceNames = serviceStatuses.Select(s => s.ServiceName.ToString()).ToList();
    serviceNames.Should().Contain(expectedServices,
      because: "the response should contain status for all critical services");

    foreach (var status in serviceStatuses)
    {
      status.Should().NotBeNull(
        because: $"status for {status.ServiceName} should not be null");

      if (!(status.IsAvailable ?? false))
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
    var configResponse = await _apiClientFixture.UnauthenticatedPublicApi.Config();
    var configItems = configResponse.Content;

    // Assert
    configResponse.Should().NotBeNull(
      because: "response should contain configuration item status information");

    // This assertion might be too strict if the list can be legitimately empty.
    // For now, assuming it should return some items if the service is working.
    configItems.Should().NotBeEmpty(
      because: "response should contain at least some configuration item status information");

    foreach (var item in configItems)
    {
      item.Should().NotBeNull(because: "each configuration item should not be null");
      item.Name.Should().NotBeNullOrWhiteSpace(because: "each item should have a name");
    }
  }

  /// <summary>
  /// Tests that the logging status endpoint returns an OK result with logging system status information.
  /// This corresponds to the GetLoggingStatus method in PublicController, mapped to /api/logging/status.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  [LogTestStartEnd]
  public async Task GetLoggingStatus_WhenCalled_ReturnsLoggingStatusInfo()
  {
    using (CreateTestMethodContext(nameof(GetLoggingStatus_WhenCalled_ReturnsLoggingStatusInfo)))
    {
      // Act
      var statusResponse = await _apiClientFixture.UnauthenticatedPublicApi.Status2();
      var loggingStatus = statusResponse.Content;

      // Assert
      statusResponse.Should().NotBeNull(
        because: "response should contain logging status information");

      loggingStatus.Should().NotBeNull(
        because: "logging status should not be null");

      loggingStatus.SeqAvailable.Should().NotBeNull(
        "Seq availability status should be provided");

      loggingStatus.Method.Should().NotBeNull(
        because: "active logging method should be specified");

      // SeqUrl can be null if no Seq is configured, which is acceptable
    }
  }

  /// <summary>
  /// Tests that the logging test Seq connectivity endpoint handles requests properly.
  /// This corresponds to the TestSeqConnectivity method in PublicController, mapped to /api/logging/test-seq.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  [LogTestStartEnd]
  public async Task TestSeqConnectivity_WhenCalledWithValidRequest_ReturnsResult()
  {
    using (CreateTestMethodContext(nameof(TestSeqConnectivity_WhenCalledWithValidRequest_ReturnsResult)))
    {
      // Arrange
      var testRequest = new TestSeqRequest("http://localhost:5341");

      // Act
      var connectivityResponse = await _apiClientFixture.UnauthenticatedPublicApi.TestSeq(testRequest);
      var connectivityResult = connectivityResponse.Content;

      // Assert
      connectivityResponse.Should().NotBeNull(
        because: "response should contain connectivity test results");

      connectivityResult.Should().NotBeNull(
        because: "connectivity test result should not be null");

      connectivityResult.IsConnected.Should().NotBeNull(
        "connectivity test should indicate success or failure");

      connectivityResult.Url.Should().NotBeNullOrWhiteSpace(
        because: "tested URL should be provided");

      // Response time can be -1 for failed connections, which is acceptable
      connectivityResult.ResponseTime.Should().NotBeNull(
        because: "response time should be provided");

      // Error message is provided if connection fails, but we won't enforce it
      // since the exact behavior may vary based on network conditions
    }
  }

  /// <summary>
  /// Tests that the logging methods endpoint returns available logging methods.
  /// This corresponds to the GetAvailableLoggingMethods method in PublicController, mapped to /api/logging/methods.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  [LogTestStartEnd]
  public async Task GetLoggingMethods_WhenCalled_ReturnsAvailableLogingMethods()
  {
    using (CreateTestMethodContext(nameof(GetLoggingMethods_WhenCalled_ReturnsAvailableLogingMethods)))
    {
      // Act
      var methodsResponse = await _apiClientFixture.UnauthenticatedPublicApi.Methods();
      var loggingMethods = methodsResponse.Content;

      // Assert
      methodsResponse.Should().NotBeNull(
        because: "response should contain logging methods information");

      loggingMethods.Should().NotBeNull(
        because: "logging methods result should not be null");

      // Check that at least one logging method is available
      var hasAnyMethod = loggingMethods.NativeSeq != null || 
                        loggingMethods.DockerSeq != null || 
                        loggingMethods.FileLogging != null;
      hasAnyMethod.Should().BeTrue(
        because: "at least one logging method should be available");

      loggingMethods.CurrentMethod.Should().NotBeNullOrWhiteSpace(
        because: "current logging method should be specified");

      // Verify current method is one of the expected method names
      // The actual method names are more descriptive than the property names
      var validMethods = new[] { "Native Seq", "Docker Seq", "File Logging (Fallback)" };
      validMethods.Should().Contain(loggingMethods.CurrentMethod,
        because: "current method should be one of the valid logging methods");
    }
  }
}
