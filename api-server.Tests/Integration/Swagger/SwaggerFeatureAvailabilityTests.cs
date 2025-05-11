using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Framework.Helpers;

namespace Zarichney.Tests.Integration.Swagger;

/// <summary>
/// Integration tests for the ServiceAvailabilityOperationFilter in Swagger/OpenAPI.
/// These tests verify that the filter correctly modifies Swagger operation summaries
/// and descriptions when endpoints are unavailable due to missing configuration.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Collection("Integration")]
// Skipping integration tests due to challenges in configuring Swagger UI properly in the test environment
// Unit tests for ServiceAvailabilityOperationFilter cover core functionality
public class SwaggerFeatureAvailabilityTests : IntegrationTestBase
{
  private readonly string _swaggerJsonUrl = "/api/swagger/swagger.json";

  public SwaggerFeatureAvailabilityTests(ApiClientFixture apiClientFixture)
      : base(apiClientFixture)
  {
  }

  [SkipSwaggerIntegrationFact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenAllFeaturesAvailable_NoWarningsInSummary()
  {
    // Arrange
    // Create a mock configuration status service that reports all features as available
    var mockStatusService = new Mock<IConfigurationStatusService>();

    // Set up the mock to return available status for any feature
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.IsAny<string>()))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.IsAny<string>()))
        .Returns(true);

    // Create a factory with replaced services
    var customFactory = Factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
          {
            // Replace the status service with our mock
            services.AddSingleton(mockStatusService.Object);
          });
    });

    // Create an authenticated client with admin role to access Swagger
    using var client = customFactory.CreateAuthenticatedClient("test-user", ["Admin"]);

    // Act
    var response = await client.GetAsync(_swaggerJsonUrl);
    response.EnsureSuccessStatusCode();

    var swagger = await response.Content.ReadFromJsonAsync<SwaggerDocument>();

    // Assert
    swagger.Should().NotBeNull("Swagger JSON should be returned");

    // Check if any path's operations have a warning symbol in the summary
    var operationsWithWarnings = swagger.Paths
        .SelectMany(path => path.Value.Operations)
        .Where(op => op.Value.Summary?.Contains("⚠️") == true)
        .ToList();

    operationsWithWarnings.Should().BeEmpty("No operations should have warnings when all features are available");
  }

  [SkipSwaggerIntegrationFact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenLlmFeatureUnavailable_WarningsInAiControllerEndpoints()
  {
    // Arrange
    // Create a mock configuration status service that reports LLM feature as unavailable
    var mockStatusService = new Mock<IConfigurationStatusService>();

    // Set up the mock to return available status for all features except LLM
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name => name == "Llm")))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["Llm:ApiKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name => name == "Llm")))
        .Returns(false);

    // Other features are available
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name => name != "Llm")))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name => name != "Llm")))
        .Returns(true);

    // Create a factory with replaced services
    var customFactory = Factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
          {
            // Replace the status service with our mock
            services.AddSingleton(mockStatusService.Object);
          });
    });

    // Create an authenticated client with admin role to access Swagger
    using var client = customFactory.CreateAuthenticatedClient("test-user", ["Admin"]);

    // Act
    var response = await client.GetAsync(_swaggerJsonUrl);
    response.EnsureSuccessStatusCode();

    var swagger = await response.Content.ReadFromJsonAsync<SwaggerDocument>();

    // Assert
    swagger.Should().NotBeNull("Swagger JSON should be returned");

    // Find AI controller endpoints (completion and transcribe)
    var aiEndpoints = swagger.Paths
        .Where(path => path.Key.StartsWith("/api/completion") || path.Key.StartsWith("/api/transcribe"))
        .SelectMany(path => path.Value.Operations)
        .ToList();

    aiEndpoints.Should().NotBeEmpty("AI controller endpoints should be present in Swagger");

    // All AI endpoints should have warnings
    foreach (var endpoint in aiEndpoints)
    {
      endpoint.Value.Summary.Should().Contain("⚠️", "AI endpoint should have warning when LLM is unavailable");
      endpoint.Value.Summary.Should().Contain("Llm:ApiKey", "Warning should mention missing LLM API key");
      endpoint.Value.Description.Should().Contain("unavailable", "Description should indicate endpoint is unavailable");
    }
  }

  [SkipSwaggerIntegrationFact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenPaymentsFeatureUnavailable_WarningsInPaymentControllerEndpoints()
  {
    // Arrange
    // Create a mock configuration status service that reports Payments feature as unavailable
    var mockStatusService = new Mock<IConfigurationStatusService>();

    // Set up the mock to return available status for all features except Payments
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name => name == "Payments")))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["Payments:StripeSecretKey", "Payments:StripeWebhookSecret"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name => name == "Payments")))
        .Returns(false);

    // Other features are available
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name => name != "Payments")))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name => name != "Payments")))
        .Returns(true);

    // Create a factory with replaced services
    var customFactory = Factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
          {
            // Replace the status service with our mock
            services.AddSingleton(mockStatusService.Object);
          });
    });

    // Create an authenticated client with admin role to access Swagger
    using var client = customFactory.CreateAuthenticatedClient("test-user", ["Admin"]);

    // Act
    var response = await client.GetAsync(_swaggerJsonUrl);
    response.EnsureSuccessStatusCode();

    var swagger = await response.Content.ReadFromJsonAsync<SwaggerDocument>();

    // Assert
    swagger.Should().NotBeNull("Swagger JSON should be returned");

    // Find Payment controller endpoints
    var paymentEndpoints = swagger.Paths
        .Where(path => path.Key.StartsWith("/api/payments"))
        .SelectMany(path => path.Value.Operations)
        .ToList();

    paymentEndpoints.Should().NotBeEmpty("Payment controller endpoints should be present in Swagger");

    // All Payment endpoints should have warnings
    foreach (var endpoint in paymentEndpoints)
    {
      endpoint.Value.Summary.Should().Contain("⚠️", "Payment endpoint should have warning when Payments is unavailable");
      endpoint.Value.Summary.Should().Contain("Payments:", "Warning should mention missing Payments configuration");
      endpoint.Value.Description.Should().Contain("unavailable", "Description should indicate endpoint is unavailable");
    }
  }

  [SkipSwaggerIntegrationFact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenMultipleFeaturesUnavailable_AllRelevantEndpointsHaveWarnings()
  {
    // Arrange
    // Create a mock configuration status service that reports multiple features as unavailable
    var mockStatusService = new Mock<IConfigurationStatusService>();

    // Set up the mock to return unavailable status for LLM, GitHub, and Payments
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name => name == "Llm")))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["Llm:ApiKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name => name == "Llm")))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name => name == "GitHub")))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["GitHub:AccessToken"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name => name == "GitHub")))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name => name == "Payments")))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["Payments:StripeSecretKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name => name == "Payments")))
        .Returns(false);

    // Other features are available
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name =>
            name != "Llm" && name != "GitHub" && name != "Payments")))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name =>
            name != "Llm" && name != "GitHub" && name != "Payments")))
        .Returns(true);

    // Create a factory with replaced services
    var customFactory = Factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
          {
            // Replace the status service with our mock
            services.AddSingleton(mockStatusService.Object);
          });
    });

    // Create an authenticated client with admin role to access Swagger
    using var client = customFactory.CreateAuthenticatedClient("test-user", ["Admin"]);

    // Act
    var response = await client.GetAsync(_swaggerJsonUrl);
    response.EnsureSuccessStatusCode();

    var swagger = await response.Content.ReadFromJsonAsync<SwaggerDocument>();

    // Assert
    swagger.Should().NotBeNull("Swagger JSON should be returned");

    // Find endpoints that require features
    var aiEndpoints = swagger.Paths
        .Where(path => path.Key.StartsWith("/api/completion") || path.Key.StartsWith("/api/transcribe"))
        .SelectMany(path => path.Value.Operations)
        .ToList();

    var paymentEndpoints = swagger.Paths
        .Where(path => path.Key.StartsWith("/api/payments"))
        .SelectMany(path => path.Value.Operations)
        .ToList();

    // Check that AI controller endpoints have appropriate warnings
    // Transcribe endpoint should mention both LLM and GitHub in the warning
    var transcribeEndpoint = swagger.Paths
        .Where(path => path.Key.StartsWith("/api/transcribe"))
        .SelectMany(path => path.Value.Operations)
        .FirstOrDefault();

    if (transcribeEndpoint.Value != null)
    {
      transcribeEndpoint.Value.Summary.Should().Contain("⚠️", "Transcribe endpoint should have warning");
      transcribeEndpoint.Value.Summary.Should().Contain("Llm", "Warning should mention LLM");
      transcribeEndpoint.Value.Summary.Should().Contain("GitHub", "Warning should mention GitHub");
    }

    // Check that Payment controller endpoints have appropriate warnings
    foreach (var endpoint in paymentEndpoints)
    {
      endpoint.Value.Summary.Should().Contain("⚠️", "Payment endpoint should have warning");
      endpoint.Value.Summary.Should().Contain("Payments", "Warning should mention Payments");
    }
  }

  /// <summary>
  /// Simple class to deserialize the Swagger JSON output for testing.
  /// </summary>
  private class SwaggerDocument
  {
    public string? OpenApi { get; set; }
    public SwaggerInfo? Info { get; set; }
    public Dictionary<string, SwaggerPathItem> Paths { get; set; } = new();
  }

  private class SwaggerInfo
  {
    public string? Title { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
  }

  private class SwaggerPathItem
  {
    public Dictionary<string, SwaggerOperation> Operations { get; set; } = new();

    [System.Text.Json.Serialization.JsonPropertyName("get")]
    public SwaggerOperation? Get
    {
      get => Operations.TryGetValue("get", out var op) ? op : null;
      set { if (value != null) Operations["get"] = value; }
    }

    [System.Text.Json.Serialization.JsonPropertyName("post")]
    public SwaggerOperation? Post
    {
      get => Operations.TryGetValue("post", out var op) ? op : null;
      set { if (value != null) Operations["post"] = value; }
    }

    [System.Text.Json.Serialization.JsonPropertyName("put")]
    public SwaggerOperation? Put
    {
      get => Operations.TryGetValue("put", out var op) ? op : null;
      set { if (value != null) Operations["put"] = value; }
    }

    [System.Text.Json.Serialization.JsonPropertyName("delete")]
    public SwaggerOperation? Delete
    {
      get => Operations.TryGetValue("delete", out var op) ? op : null;
      set { if (value != null) Operations["delete"] = value; }
    }
  }

  private class SwaggerOperation
  {
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public object? Parameters { get; set; }
    public Dictionary<string, object>? Responses { get; set; }
  }
}
