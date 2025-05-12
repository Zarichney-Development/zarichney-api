using Zarichney.Services.Status;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Zarichney.Config;
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
// These tests are skipped as they require a real environment to properly test the
// ServiceAvailabilityOperationFilter in context of the Swagger UI.
// The live service tests in SwaggerLiveServiceStatusTests.cs provide similar verification using real services.
public class SwaggerFeatureAvailabilityTests(ApiClientFixture apiClientFixture) : IntegrationTestBase(apiClientFixture)
{
  private const string _swaggerJsonUrl = "/api/swagger/swagger.json";

  [Fact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenAllFeaturesAvailable_NoWarningsInSummary()
  {
    // Arrange
    // Create a mock configuration status service that reports all features as available
    var mockStatusService = new Mock<IStatusService>();

    // Set up the mock to return available status for any feature (enum-based)
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.IsAny<Feature>()))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.IsAny<Feature>()))
        .Returns(true);

    // For backward compatibility, support string-based lookups too
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

  [Fact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenAiFeaturesUnavailable_WarningsInAiControllerEndpoints()
  {
    // Arrange
    // Create a mock configuration status service that reports LLM and Transcription features as unavailable
    var mockStatusService = new Mock<IStatusService>();

    // Set up the mock to return unavailable status for Transcription
    mockStatusService
        .Setup(s => s.GetFeatureStatus(Feature.Transcription))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["TranscribeConfig:ModelName"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(Feature.Transcription))
        .Returns(false);

    // Also make LLM unavailable
    mockStatusService
        .Setup(s => s.GetFeatureStatus(Feature.LLM))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["LlmConfig:ApiKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(Feature.LLM))
        .Returns(false);

    // Make GitHub access unavailable (needed for transcribe endpoint)
    mockStatusService
        .Setup(s => s.GetFeatureStatus(Feature.GitHubAccess))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["GitHubConfig:AccessToken"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(Feature.GitHubAccess))
        .Returns(false);

    // For backward compatibility, support string-based lookups too
    mockStatusService
        .Setup(s => s.GetFeatureStatus("Transcription"))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["TranscribeConfig:ModelName"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable("Transcription"))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus("Llm"))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["LlmConfig:ApiKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable("Llm"))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus("GitHub"))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["GitHubConfig:AccessToken"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable("GitHub"))
        .Returns(false);

    // Other features are available - catch-all for any other Feature enum value
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<Feature>(f =>
            f != Feature.LLM && f != Feature.Transcription && f != Feature.GitHubAccess)))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<Feature>(f =>
            f != Feature.LLM && f != Feature.Transcription && f != Feature.GitHubAccess)))
        .Returns(true);

    // Also catch-all for any other string-based feature name
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name =>
            name != "Llm" && name != "Transcription" && name != "GitHub")))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name =>
            name != "Llm" && name != "Transcription" && name != "GitHub")))
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

    // Find completion endpoint specifically
    var completionEndpoint = swagger.Paths
        .Where(path => path.Key.StartsWith("/api/completion"))
        .SelectMany(path => path.Value.Operations)
        .FirstOrDefault();

    if (completionEndpoint.Value != null)
    {
      completionEndpoint.Value.Summary.Should().Contain("⚠️", "Completion endpoint should have warning when LLM is unavailable");
      completionEndpoint.Value.Summary.Should().Contain("LLM", "Warning should mention LLM feature");
      completionEndpoint.Value.Description.Should().Contain("unavailable", "Description should indicate endpoint is unavailable");
    }

    // Find transcribe endpoint specifically
    var transcribeEndpoint = swagger.Paths
        .Where(path => path.Key.StartsWith("/api/transcribe"))
        .SelectMany(path => path.Value.Operations)
        .FirstOrDefault();

    if (transcribeEndpoint.Value != null)
    {
      transcribeEndpoint.Value.Summary.Should().Contain("⚠️", "Transcribe endpoint should have warning when required features are unavailable");
      // The transcribe endpoint requires both Transcription and GitHubAccess features
      transcribeEndpoint.Value.Summary.Should().Contain("Transcription", "Warning should mention Transcription feature");
      transcribeEndpoint.Value.Summary.Should().Contain("GitHubAccess", "Warning should mention GitHubAccess feature");
      transcribeEndpoint.Value.Description.Should().Contain("unavailable", "Description should indicate endpoint is unavailable");
    }
  }

  [Fact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenPaymentsFeatureUnavailable_WarningsInPaymentControllerEndpoints()
  {
    // Arrange
    // Create a mock configuration status service that reports Payments feature as unavailable
    var mockStatusService = new Mock<IStatusService>();

    // Set up the mock to return available status for all features except Payments
    mockStatusService
        .Setup(s => s.GetFeatureStatus(Feature.Payments))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["PaymentConfig:StripeSecretKey", "PaymentConfig:StripeWebhookSecret"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(Feature.Payments))
        .Returns(false);

    // For backward compatibility, support string-based lookups too
    mockStatusService
        .Setup(s => s.GetFeatureStatus("Payment"))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["PaymentConfig:StripeSecretKey", "PaymentConfig:StripeWebhookSecret"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable("Payment"))
        .Returns(false);

    // Other features are available - catch-all for any other Feature enum value
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<Feature>(f => f != Feature.Payments)))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<Feature>(f => f != Feature.Payments)))
        .Returns(true);

    // Also catch-all for any other string-based feature name
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name => name != "Payment")))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name => name != "Payment")))
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
        .Where(path => path.Key.StartsWith("/api/payment"))
        .SelectMany(path => path.Value.Operations)
        .ToList();

    paymentEndpoints.Should().NotBeEmpty("Payment controller endpoints should be present in Swagger");

    // All Payment endpoints should have warnings
    foreach (var endpoint in paymentEndpoints)
    {
      endpoint.Value.Summary.Should().Contain("⚠️", "Payment endpoint should have warning when Payment is unavailable");
      endpoint.Value.Summary.Should().Contain("Payment", "Warning should mention missing Payment configuration");
      endpoint.Value.Description.Should().Contain("unavailable", "Description should indicate endpoint is unavailable");
    }
  }

  [Fact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenMultipleFeaturesUnavailable_AllRelevantEndpointsHaveWarnings()
  {
    // Arrange
    // Create a mock configuration status service that reports multiple features as unavailable
    var mockStatusService = new Mock<IStatusService>();

    // Set up the mock to return unavailable status for LLM, Transcription, GitHub, and Payments
    mockStatusService
        .Setup(s => s.GetFeatureStatus(Feature.LLM))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["LlmConfig:ApiKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(Feature.LLM))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus(Feature.Transcription))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["TranscribeConfig:ModelName"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(Feature.Transcription))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus(Feature.GitHubAccess))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["GitHubConfig:AccessToken"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(Feature.GitHubAccess))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus(Feature.Payments))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["PaymentConfig:StripeSecretKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(Feature.Payments))
        .Returns(false);

    // For backward compatibility, support string-based lookups too
    mockStatusService
        .Setup(s => s.GetFeatureStatus("Llm"))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["LlmConfig:ApiKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable("Llm"))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus("Transcription"))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["TranscribeConfig:ModelName"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable("Transcription"))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus("GitHub"))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["GitHubConfig:AccessToken"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable("GitHub"))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus("Payment"))
        .Returns(new ServiceStatusInfo(
            IsAvailable: false,
            ["PaymentConfig:StripeSecretKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable("Payment"))
        .Returns(false);

    // Other features are available - catch-all for any other Feature enum value
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<Feature>(f =>
            f != Feature.LLM && f != Feature.Transcription && f != Feature.GitHubAccess && f != Feature.Payments)))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<Feature>(f =>
            f != Feature.LLM && f != Feature.Transcription && f != Feature.GitHubAccess && f != Feature.Payments)))
        .Returns(true);

    // Also catch-all for any other string-based feature name
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<string>(name =>
            name != "Llm" && name != "Transcription" && name != "GitHub" && name != "Payment")))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<string>(name =>
            name != "Llm" && name != "Transcription" && name != "GitHub" && name != "Payment")))
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

    var paymentEndpoints = swagger.Paths
        .Where(path => path.Key.StartsWith("/api/payment"))
        .SelectMany(path => path.Value.Operations)
        .ToList();

    // Check that AI controller endpoints have appropriate warnings
    // Transcribe endpoint should mention both Transcription and GitHubAccess in the warning
    var transcribeEndpoint = swagger.Paths
        .Where(path => path.Key.StartsWith("/api/transcribe"))
        .SelectMany(path => path.Value.Operations)
        .FirstOrDefault();

    if (transcribeEndpoint.Value != null)
    {
      transcribeEndpoint.Value.Summary.Should().Contain("⚠️", "Transcribe endpoint should have warning");
      transcribeEndpoint.Value.Summary.Should().Contain("Transcription", "Warning should mention Transcription feature");
      transcribeEndpoint.Value.Summary.Should().Contain("GitHubAccess", "Warning should mention GitHubAccess feature");
    }

    // Check that Payment controller endpoints have appropriate warnings
    foreach (var endpoint in paymentEndpoints)
    {
      endpoint.Value.Summary.Should().Contain("⚠️", "Payment endpoint should have warning");
      endpoint.Value.Summary.Should().Contain("Payments", "Warning should mention Payments");
    }
  }
}
