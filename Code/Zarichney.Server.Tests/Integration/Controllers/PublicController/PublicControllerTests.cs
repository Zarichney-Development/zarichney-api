using FluentAssertions;
using Refit;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Server.Tests.Framework.Attributes;
using Zarichney.Server.Tests.Framework.Fixtures;

namespace Zarichney.Server.Tests.Integration.Controllers.PublicController;

/// <summary>
/// Integration tests for the PublicController.
/// These tests verify the behavior of endpoints accessible without authentication,
/// ensuring they function correctly within the ASP.NET Core pipeline.
/// They cover both minimal functionality (always runnable) and configuration-dependent scenarios.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Component, TestCategories.Controller)]
[Trait(TestCategories.Feature, "Public")]
[Collection("IntegrationCore")]
public class PublicControllerTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(apiClientFixture, testOutputHelper)
{
  /// <summary>
  /// Tests that the health check endpoint returns an OK result with the expected structure.
  /// This test should run even when configuration is missing.
  /// Corresponds to README TODO: Test `GET /health-check` without authentication -> verify 200 OK and expected simple response.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task GetHealth_WhenCalled_ReturnsOkStatusAndTimeInfo()
  {
    // Arrange
    // ApiClient is obtained from IntegrationTestBase, represents unauthenticated Refit client

    // Act & Assert: calling Health should not throw and thus return 200 OK
    var act = () => _apiClientFixture.UnauthenticatedPublicApi.Health();
    await act.Should().NotThrowAsync<ApiException>(
      because: "health endpoint should always return OK status even without configuration");
  }

  /// <summary>
  /// Tests that the configuration status endpoint returns the expected items with correct status details.
  /// This test should run even when configuration is missing.
  /// Corresponds to README TODO: Test `GET /config` without authentication -> verify 200 OK with expected public configuration values.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task GetConfigurationStatus_WhenCalled_ReturnsAllExpectedConfigItems()
  {
    // Arrange
    // TODO: make this less brittle by strongly tying it to whats expected from the enum ExternalServices
    var expectedConfigItemNames = new List<string>
    {
      "OpenAI API Key",
      "Email AzureTenantId",
      "Email AzureAppId",
      "Email AzureAppSecret",
      "Email FromEmail",
      "Email MailCheckApiKey",
      "GitHub Access Token",
      "Stripe Secret Key",
      "Stripe Webhook Secret",
      "Identity Database Connection"
    };

    // Act
    var configResponse = await _apiClientFixture.UnauthenticatedPublicApi.Config();
    var configItems = configResponse.Content;

    // Assert
    configResponse.Should().NotBeNull(
      because: "response should contain a list of configuration statuses");

    configItems.Should().NotBeNull().And.NotBeEmpty(
      because: "response should contain at least some configuration items");

    configItems.Select(item => item.Name).Should().BeEquivalentTo(expectedConfigItemNames,
      because: "the response should contain exactly the expected configuration item statuses");

    // Check that status values are either "Configured" or "Missing/Invalid"
    foreach (var item in configItems)
    {
      item.Status.Should().Match(s =>
          s == "Configured" || s == "Missing/Invalid",
        because: $"Status for {item.Name} should be either 'Configured' or 'Missing/Invalid'");

      if (item.Status == "Missing/Invalid")
      {
        item.Details.Should().NotBeNullOrEmpty(
          because: $"there should be details about why {item.Name} is missing or invalid");
        item.Details.Should().Contain("missing or placeholder",
          because: $"{item.Name} should be reported as missing or a placeholder");
      }
      else
      {
        item.Details.Should().BeNullOrEmpty(
          because: $"details should be empty when {item.Name} is configured");
      }
    }
  }

  // Note: The README mentions an optional test for /health-check *with* authentication.
  // This is generally less critical for public endpoints but could be added if needed.
  // Example:
  // [Fact]
  // public async Task GetHealth_WhenCalledWithAuthentication_ReturnsOk() { /* ... */ }

  // Note: The README mentions tests for /status based on IStatusService success/failure.
  // The current controller doesn't have a generic /status endpoint, only /status/config.
  // If a /status endpoint were added, tests mocking IStatusService would go here.

  // Add other PublicController tests that require proper configuration here
  // These tests will be skipped when dependencies are missing (if using specific Traits/Filters)
}
