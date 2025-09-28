using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using StatusInfo = Zarichney.Services.Status.ServiceStatusInfo;

namespace Zarichney.Tests.Integration.Services.Status;

/// <summary>
/// Integration tests for the <see cref="FeatureAvailabilityMiddleware"/> to verify it correctly
/// blocks endpoints when their required features are unavailable.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Collection("IntegrationInfra")]
public class FeatureAvailabilityMiddlewareTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(apiClientFixture, testOutputHelper)
{
  /// <summary>
  /// Tests that endpoints with DependsOnService attribute return 503 when required features are unavailable.
  /// </summary>
  [SkippableFact]
  [Trait(TestCategories.Feature, "Middleware")]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task Endpoint_WithDependsOnServiceAttribute_WhenFeatureUnavailable_Returns503()
  {
    // Skip infrastructure tests in test/validation branches or when explicitly disabled
    var skipInfraTests = Environment.GetEnvironmentVariable("SKIP_INFRASTRUCTURE_TESTS") == "true" ||
                         Environment.GetEnvironmentVariable("GITHUB_HEAD_REF")?.StartsWith("test/") == true ||
                         Environment.GetEnvironmentVariable("CI_ENVIRONMENT") == "true";

    Skip.If(skipInfraTests, "Infrastructure tests skipped for test/validation branches or CI environment");

    // Arrange
    var mockStatusService = new Mock<IStatusService>();

    // Configure the mock to return specific statuses for different features
    mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.OpenAiApi))
        .Returns(new StatusInfo(ExternalServices.OpenAiApi, false, ["LlmConfig:ApiKey"]));

    // Use a WebHostBuilder to replace the service with our mock
    var customFactory = Factory;
    var factoryWithMockStatus = customFactory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
          {
            services.AddSingleton(mockStatusService.Object);
            // Register the test controller
            services.AddControllers().AddApplicationPart(typeof(Framework.TestControllers.FeatureTestController).Assembly);
          });
    });

    // Create a client
    using var client = factoryWithMockStatus.CreateClient();

    // Add authentication headers
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
        "Test", Framework.Helpers.AuthTestHelper.GenerateTestToken("test-user", ["User"]));

    // Configure test client to accept 404 responses (since the endpoint might not actually exist in test)
    client.DefaultRequestHeaders.Add("Accept-Test-Status-Codes", "404");

    // Act - Call the test endpoint that requires LLM feature
    var response = await client.GetAsync("/api/test-feature/llm");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("External Service Unavailable");
    content.Should().Contain("LlmConfig:ApiKey");

    mockStatusService.Verify(s => s.GetFeatureStatus(ExternalServices.OpenAiApi), Times.AtLeastOnce);
  }

  /// <summary>
  /// Tests that endpoints without DependsOnService attribute remain accessible
  /// even when some features are unavailable.
  /// </summary>
  [SkippableFact]
  [Trait(TestCategories.Feature, "Middleware")]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task Endpoint_WithoutDependsOnServiceAttribute_WhenFeaturesUnavailable_RemainsAccessible()
  {
    // Skip infrastructure tests in test/validation branches or when explicitly disabled
    var skipInfraTests = Environment.GetEnvironmentVariable("SKIP_INFRASTRUCTURE_TESTS") == "true" ||
                         Environment.GetEnvironmentVariable("GITHUB_HEAD_REF")?.StartsWith("test/") == true ||
                         Environment.GetEnvironmentVariable("CI_ENVIRONMENT") == "true";

    Skip.If(skipInfraTests, "Infrastructure tests skipped for test/validation branches or CI environment");
    // Arrange
    var mockStatusService = new Mock<IStatusService>();

    // Configure all features to be unavailable
    foreach (ExternalServices feature in Enum.GetValues(typeof(ExternalServices)))
    {
      mockStatusService.Setup(s => s.GetFeatureStatus(feature))
          .Returns(new StatusInfo(feature, false, [$"{feature}Config:MissingKey"]));

      mockStatusService.Setup(s => s.IsFeatureAvailable(feature))
          .Returns(false);
    }

    // Use a WebHostBuilder to replace the service with our mock
    var customFactory = Factory;
    var factoryWithMockStatus = customFactory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
          {
            services.AddSingleton(mockStatusService.Object);
            // Register the test controller
            services.AddControllers().AddApplicationPart(typeof(Framework.TestControllers.FeatureTestController).Assembly);
          });
    });

    // Create a client
    using var client = factoryWithMockStatus.CreateClient();

    // Add authentication headers
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
        "Test", Framework.Helpers.AuthTestHelper.GenerateTestToken("test-user", ["User"]));

    // Act - Call an endpoint without feature requirements
    var response = await client.GetAsync("/api/test-feature/available");

    // Assert - Should still be accessible
    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  /// <summary>
  /// Tests that endpoints with multiple required features aggregate all missing configurations
  /// when throwing the ServiceUnavailableException.
  /// </summary>
  [SkippableFact]
  [Trait(TestCategories.Feature, "Middleware")]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task Endpoint_WithMultipleRequiredFeatures_WhenSomeUnavailable_Returns503WithAllMissingConfigs()
  {
    // Skip infrastructure tests in test/validation branches or when explicitly disabled
    var skipInfraTests = Environment.GetEnvironmentVariable("SKIP_INFRASTRUCTURE_TESTS") == "true" ||
                         Environment.GetEnvironmentVariable("GITHUB_HEAD_REF")?.StartsWith("test/") == true ||
                         Environment.GetEnvironmentVariable("CI_ENVIRONMENT") == "true";

    Skip.If(skipInfraTests, "Infrastructure tests skipped for test/validation branches or CI environment");
    // Arrange
    var mockStatusService = new Mock<IStatusService>();

    // Configure the mock to return specific statuses for different features
    mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.OpenAiApi))
        .Returns(new StatusInfo(ExternalServices.OpenAiApi, false, ["LlmConfig:ApiKey"]));

    mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.MsGraph))
        .Returns(new StatusInfo(ExternalServices.MsGraph, false, ["EmailConfig:FromEmail"]));

    mockStatusService.Setup(s => s.IsFeatureAvailable(It.IsAny<ExternalServices>()))
        .Returns<ExternalServices>(f => f != ExternalServices.OpenAiApi);

    // Use a WebHostBuilder to replace the service with our mock
    var customFactory = Factory;
    var factoryWithMockStatus = customFactory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
          {
            services.AddSingleton(mockStatusService.Object);
            // Register the test controller
            services.AddControllers().AddApplicationPart(typeof(Framework.TestControllers.FeatureTestController).Assembly);
          });
    });

    // Create a client
    using var client = factoryWithMockStatus.CreateClient();

    // Add authentication headers
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
        "Test", Framework.Helpers.AuthTestHelper.GenerateTestToken("test-user", ["User"]));

    // Configure test client to accept 404 responses (since the endpoint might not actually exist in test)
    client.DefaultRequestHeaders.Add("Accept-Test-Status-Codes", "404");

    // Act - Call the test endpoint that requires multiple features
    var response = await client.GetAsync("/api/test-feature/multi");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("External Service Unavailable");

    // Should contain all missing configurations
    content.Should().Contain("LlmConfig:ApiKey");
    content.Should().Contain("EmailConfig:FromEmail");
  }
}
