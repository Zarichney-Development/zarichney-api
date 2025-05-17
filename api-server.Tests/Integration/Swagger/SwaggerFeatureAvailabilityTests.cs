using Zarichney.Services.Status;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Config;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Framework.Helpers;
using static Zarichney.Tests.Framework.Helpers.TestFactories;

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
public class SwaggerFeatureAvailabilityTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(apiClientFixture, testOutputHelper)
{
  private const string _swaggerJsonUrl = "/api/swagger/swagger.json";

  /// <summary>
  /// Tests that the Swagger UI doesn't show any warnings for endpoints
  /// when all required features are available.
  /// </summary>
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
        .Setup(s => s.GetFeatureStatus(It.IsAny<ExternalServices>()))
        .Returns((ExternalServices service) => new ServiceStatusInfo(serviceName: service, IsAvailable: true, MissingConfigurations: []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.IsAny<ExternalServices>()))
        .Returns(true);

    // The string overloads should be gone now, so no need to set these up
    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.IsAny<ExternalServices>()))
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

    // More resilient check using query projection rather than direct path navigation
    var operationsWithWarnings = swagger.Paths
        // Convert to dictionary entries for more flexible checks
        .Select(path => new { Path = path.Key, path.Value.Operations })
        // Get all operations
        .SelectMany(x => x.Operations.Select(op => new
        {
          x.Path,
          HttpMethod = op.Key,
          Operation = op.Value,
          HasWarning = op.Value.Summary?.Contains("⚠️") == true
        }))
        // Filter to only operations with warnings
        .Where(x => x.HasWarning)
        .ToList();

    // Better failure detail
    operationsWithWarnings.Should().BeEmpty(
        "No operations should have warnings when all features are available. " +
        "Found warnings in: {0}",
        string.Join(", ", operationsWithWarnings.Select(x => $"{x.Path} ({x.HttpMethod})")));
  }

  /// <summary>
  /// Tests that the Swagger UI shows appropriate warnings for AI endpoints
  /// when the OpenAI API and GitHub services are unavailable due to missing configuration.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenAiFeaturesUnavailable_WarningsInAiControllerEndpoints()
  {
    // Arrange
    // Create a mock configuration status service that reports LLM and Transcription features as unavailable
    var mockStatusService = new Mock<IStatusService>();

    // Also make LLM unavailable
    mockStatusService
        .Setup(s => s.GetFeatureStatus(ExternalServices.OpenAiApi))
        .Returns(CreateServiceStatus(
          serviceName: ExternalServices.OpenAiApi,
          isAvailable: false,
          missingConfigurations: ["LlmConfig:ApiKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(ExternalServices.OpenAiApi))
        .Returns(false);

    // Make GitHub access unavailable (needed for transcribe endpoint)
    mockStatusService
        .Setup(s => s.GetFeatureStatus(ExternalServices.GitHubAccess))
        .Returns(new ServiceStatusInfo(
          serviceName: ExternalServices.GitHubAccess,
          IsAvailable: false,
          MissingConfigurations: ["GitHubConfig:AccessToken"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(ExternalServices.GitHubAccess))
        .Returns(false);

    // Other features are available - catch-all for any other Feature enum value
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<ExternalServices>(f =>
            f != ExternalServices.OpenAiApi && f != ExternalServices.GitHubAccess)))
        .Returns((ExternalServices service) => new ServiceStatusInfo(service, true, []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<ExternalServices>(f =>
            f != ExternalServices.OpenAiApi && f != ExternalServices.GitHubAccess)))
        .Returns(true);

    // String-based API is gone, so no need for these setup calls

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
      completionEndpoint.Value.Summary.Should().Contain("LlmConfig:ApiKey", "Warning should mention missing config");
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
      transcribeEndpoint.Value.Summary.Should().Contain("LlmConfig:ApiKey", "Warning should mention missing config");
      transcribeEndpoint.Value.Description.Should().Contain("unavailable", "Description should indicate endpoint is unavailable");
    }
  }

  /// <summary>
  /// Tests that the Swagger UI shows appropriate warnings for Payment endpoints
  /// when the Stripe service is unavailable due to missing configuration.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenPaymentsFeatureUnavailable_WarningsInPaymentControllerEndpoints()
  {
    // Arrange
    // Create a mock configuration status service that reports Stripe feature as unavailable
    var mockStatusService = new Mock<IStatusService>();

    // Set up the mock to return available status for all features except Stripe
    mockStatusService
        .Setup(s => s.GetFeatureStatus(ExternalServices.Stripe))
        .Returns(new ServiceStatusInfo(
            serviceName: ExternalServices.Stripe,
            IsAvailable: false,
            MissingConfigurations: ["PaymentConfig:StripeSecretKey", "PaymentConfig:StripeWebhookSecret"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(ExternalServices.Stripe))
        .Returns(false);

    // No longer need string-based lookups

    // Other features are available - catch-all for any other Feature enum value
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<ExternalServices>(f => f != ExternalServices.Stripe)))
        .Returns((ExternalServices service) => new ServiceStatusInfo(serviceName: service, IsAvailable: true, MissingConfigurations: []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<ExternalServices>(f => f != ExternalServices.Stripe)))
        .Returns(true);

    // No longer need string-based lookups

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

  /// <summary>
  /// Tests that the Swagger UI shows appropriate warnings for all affected endpoints
  /// when multiple services (OpenAI API, GitHub, and Stripe) are unavailable due to missing configuration.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerOperationFilter_WhenMultipleFeaturesUnavailable_AllRelevantEndpointsHaveWarnings()
  {
    // Arrange
    // Create a mock configuration status service that reports multiple features as unavailable
    var mockStatusService = new Mock<IStatusService>();

    // Set up the mock to return unavailable status for LLM, Transcription, GitHub, and Stripe
    mockStatusService
        .Setup(s => s.GetFeatureStatus(ExternalServices.OpenAiApi))
        .Returns(new ServiceStatusInfo(
            serviceName: ExternalServices.OpenAiApi,
            IsAvailable: false,
            MissingConfigurations: ["LlmConfig:ApiKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(ExternalServices.OpenAiApi))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus(ExternalServices.GitHubAccess))
        .Returns(new ServiceStatusInfo(
            serviceName: ExternalServices.GitHubAccess,
            IsAvailable: false,
            MissingConfigurations: ["GitHubConfig:AccessToken"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(ExternalServices.GitHubAccess))
        .Returns(false);

    mockStatusService
        .Setup(s => s.GetFeatureStatus(ExternalServices.Stripe))
        .Returns(new ServiceStatusInfo(
            serviceName: ExternalServices.Stripe,
            IsAvailable: false,
            MissingConfigurations: ["PaymentConfig:StripeSecretKey"]
        ));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(ExternalServices.Stripe))
        .Returns(false);

    // No longer need string-based lookups - removed

    // Other features are available - catch-all for any other Feature enum value
    mockStatusService
        .Setup(s => s.GetFeatureStatus(It.Is<ExternalServices>(f =>
            f != ExternalServices.OpenAiApi && f != ExternalServices.GitHubAccess && f != ExternalServices.Stripe)))
        .Returns((ExternalServices service) => new ServiceStatusInfo(serviceName: service, IsAvailable: true, MissingConfigurations: []));

    mockStatusService
        .Setup(s => s.IsFeatureAvailable(It.Is<ExternalServices>(f =>
            f != ExternalServices.OpenAiApi && f != ExternalServices.GitHubAccess && f != ExternalServices.Stripe)))
        .Returns(true);

    // Also catch-all for any other string-based feature name
    // This setup has incorrect type (was using strings as enum)
    // Fixed to use proper enum comparison

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
      transcribeEndpoint.Value.Summary.Should().Contain("LlmConfig:ApiKey", "Warning should mention missing config");
    }

    // Check that Payment controller endpoints have appropriate warnings
    foreach (var endpoint in paymentEndpoints)
    {
      endpoint.Value.Summary.Should().Contain("⚠️", "Payment endpoint should have warning");
      endpoint.Value.Summary.Should().Contain("Stripe", "Warning should mention Stripe");
    }
  }
}
