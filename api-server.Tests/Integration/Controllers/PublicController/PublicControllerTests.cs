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
/// These tests include both minimal functionality tests that should always run
/// and more comprehensive tests that require proper configuration.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Component, TestCategories.Controller)]
[Trait(TestCategories.Feature, "Public")]
public class PublicControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    /// <summary>
    /// Tests that the health check endpoint returns an OK result.
    /// This test should run even when configuration is missing.
    /// </summary>
    [Fact]
    [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
    public async Task GetHealth_WhenCalled_ReturnsOkStatusAndTimeInfo()
    {
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
    /// Tests that the configuration status endpoint returns the expected items.
    /// This test should run even when configuration is missing.
    /// </summary>
    [Fact]
    [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
    public async Task GetConfigurationStatus_WhenCalled_ReturnsAllExpectedConfigItems()
    {
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
        
        // Check for specific configuration items that should be in the response
        statusItems.Should().Contain(item => item.Name == "OpenAI API Key", 
            because: "the response should contain the OpenAI API Key status");
        statusItems.Should().Contain(item => item.Name == "Email AzureTenantId", 
            because: "the response should contain the Email AzureTenantId status");
        statusItems.Should().Contain(item => item.Name == "Email AzureAppId", 
            because: "the response should contain the Email AzureAppId status");
        statusItems.Should().Contain(item => item.Name == "Email AzureAppSecret", 
            because: "the response should contain the Email AzureAppSecret status");
        statusItems.Should().Contain(item => item.Name == "Email FromEmail", 
            because: "the response should contain the Email FromEmail status");
        statusItems.Should().Contain(item => item.Name == "Email MailCheckApiKey", 
            because: "the response should contain the Email MailCheckApiKey status");
        statusItems.Should().Contain(item => item.Name == "GitHub Access Token", 
            because: "the response should contain the GitHub Access Token status");
        statusItems.Should().Contain(item => item.Name == "Stripe Secret Key", 
            because: "the response should contain the Stripe Secret Key status");
        statusItems.Should().Contain(item => item.Name == "Stripe Webhook Secret", 
            because: "the response should contain the Stripe Webhook Secret status");
        statusItems.Should().Contain(item => item.Name == "Database Connection", 
            because: "the response should contain the Database Connection status");
        
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
        }
    }

    // Add other PublicController tests that require proper configuration here
    // These tests will be skipped when dependencies are missing
}
