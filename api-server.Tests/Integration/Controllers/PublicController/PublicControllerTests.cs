using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Controllers.PublicController;

/// <summary>
/// Integration tests for the PublicController.
/// These tests verify the behavior of endpoints accessible without authentication,
/// ensuring they function correctly within the ASP.NET Core pipeline.
/// They cover both minimal functionality (always runnable) and configuration-dependent scenarios.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Component, TestCategories.Controller)]
[Trait(TestCategories.Feature, "Public")]
public class PublicControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
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
        // Client is obtained from IntegrationTestBase, representing an unauthenticated client

        // Act
        var response = await Client.GetAsync("/api/health");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            because: "health endpoint should always return OK status even without configuration");
        
        // Read response content as JsonElement to safely examine the structure
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        // Check if "success" property exists (case insensitive)
        bool hasSuccessProperty = content.TryGetProperty("success", out var successProperty) ||
                                content.TryGetProperty("Success", out successProperty);
        
        hasSuccessProperty.Should().BeTrue(
            because: "health endpoint response should contain a 'success' property");
        
        if (hasSuccessProperty)
        {
            successProperty.ValueKind.Should().Be(JsonValueKind.True,
                because: "success property should be true");
        }

        // Check if "time" property exists (case insensitive)
        bool hasTimeProperty = content.TryGetProperty("time", out _) ||
                            content.TryGetProperty("Time", out _);
        
        hasTimeProperty.Should().BeTrue(
            because: "health endpoint response should contain a 'time' property");
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
        // Client is obtained from IntegrationTestBase, representing an unauthenticated client
        // The CustomWebApplicationFactory handles mocking/providing IStatusService if necessary.
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
            "Database Connection"
        };

        // Act
        var response = await Client.GetAsync("/api/status/config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "configuration status endpoint should always return OK");

        var statusItems = await response.Content.ReadFromJsonAsync<List<ConfigurationItemStatus>>();

        // Verify the response contains the expected configuration items
        statusItems.Should().NotBeNull(
            because: "response should contain a list of configuration statuses");

        statusItems.Should().NotBeEmpty(
            because: "response should contain at least some configuration items");

        // Check that all expected configuration item names are present in the response
        statusItems.Select(item => item.Name).Should().BeEquivalentTo(expectedConfigItemNames,
            because: "the response should contain exactly the expected configuration item statuses");

        // Check that status values are either "Configured" or "Missing/Invalid"
        foreach (var item in statusItems)
        {
            item.Status.Should().Match(s =>
                s == "Configured" || s == "Missing/Invalid",
                because: $"Status for {item.Name} should be either 'Configured' or 'Missing/Invalid'");

            // Only check for details if the status is "Missing/Invalid"
            if (item.Status == "Missing/Invalid")
            {
                item.Details.Should().NotBeNullOrEmpty(
                    because: $"there should be details about why {item.Name} is missing or invalid");
                item.Details.Should().Contain("missing or placeholder",
                    because: $"{item.Name} should be reported as missing or a placeholder");
            }
            else // Status is "Configured"
            {
                 // Optionally, assert that Details is null or empty when configured
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
