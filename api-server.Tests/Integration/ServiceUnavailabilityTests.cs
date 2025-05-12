using Zarichney.Services.Status;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Refit;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Client;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Framework.Helpers;

// Import the ServiceStatusInfo from Status namespace with alias
using StatusInfo = Zarichney.Services.Status.ServiceStatusInfo;

namespace Zarichney.Tests.Integration;

/// <summary>
/// Integration tests that verify the behavior of the API when services are unavailable
/// due to missing configuration. These tests ensure that endpoints correctly return
/// HTTP 503 responses with appropriate error details.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Collection("Integration")]
public class ServiceUnavailabilityTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(apiClientFixture, testOutputHelper)
{
  /// <summary>
  /// Tests that API endpoints return HTTP 503 when a required feature is unavailable.
  /// This test configures a custom WebApplicationFactory that simulates LLM service
  /// being unavailable due to missing configuration.
  /// </summary>
  // [Fact(Skip = "The test is currently unstable due to HTTP 400 instead of 503 response")]
  [Fact]
  // TODO: fix this
  [Trait(TestCategories.Feature, TestCategories.AI)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task Endpoint_WhenRequiredFeatureIsUnavailable_Returns503WithErrorDetails()
  {
    // Arrange
    // Prepare a request to the AI completion endpoint
    var textPrompt = GetRandom.String();

    // Act
    // Test that the action throws an ApiException
    ApiException? exception = null;
    try
    {
      // should be triggering the logging sink output and succeeding
      await AuthenticatedApiClient.Health();
      // todo: troubleshoot whats wrong with client to trigger a 400 on request call, not hitting the endpoint handler
      await AuthenticatedApiClient.Completion(textPrompt);
      Assert.Fail("Expected ApiException was not thrown");
    }
    catch (ApiException ex)
    {
      exception = ex;
    }

    // Verify the response
    Assert.NotNull(exception);
    exception.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var errorContent = exception.Content;
    errorContent.Should().Contain("Service Temporarily Unavailable");
    errorContent.Should().Contain("LlmConfig:ApiKey");

    // Verify that a public endpoint is still available
    await ApiClient.Health(); // The extension method will throw if there's an error
  }

  /// <summary>
  /// Tests that the /api/status endpoint correctly reports service availability.
  /// </summary>
  [Fact(Skip = "The test is currently unstable due to HttpClient extraction from Refit client")]
  // TODO: fix this

  [Trait(TestCategories.Feature, "Status")]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task StatusEndpoint_ReturnsServiceAvailabilityInformation()
  {
    // Arrange
    // Create a custom factory that indicates mixed service availability
    var mockStatusService = new Mock<IStatusService>();
    mockStatusService.Setup(s => s.GetServiceStatusAsync())
      .ReturnsAsync(new Dictionary<string, StatusInfo>
      {
        { "Llm", new StatusInfo(IsAvailable: false, ["Llm:ApiKey"]) },
        { "Email", new StatusInfo(IsAvailable: true, []) },
        { "Payment", new StatusInfo(IsAvailable: false, ["Payment:StripeKey"]) }
      });

    var customFactory = Factory as CustomWebApplicationFactory;
    var factoryWithWebHostBuilder = customFactory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
      {
        // Replace the real ConfigurationStatusService with our mock
        services.AddSingleton(mockStatusService.Object);
      });
    });

    using var httpClient = customFactory.CreateClient();
    var client = RestService.For<IZarichneyAPI>(httpClient);

    // Act
    var result = await client.Status2();

    // Assert
    result.Should().NotBeNull();
    result.Should().ContainKey("Llm");
    result.Should().ContainKey("Email");
    result.Should().ContainKey("Payment");

    result["Llm"].IsAvailable.Should().BeFalse();
    result["Llm"].MissingConfigurations.Should().Contain("Llm:ApiKey");

    result["Email"].IsAvailable.Should().BeTrue();
    result["Email"].MissingConfigurations.Should().BeEmpty();

    result["Payment"].IsAvailable.Should().BeFalse();
    result["Payment"].MissingConfigurations.Should().Contain("Payment:StripeKey");
  }

  /// <summary>
  /// Tests that Swagger documents correctly indicate which endpoints are unavailable
  /// due to missing configuration.
  /// </summary>
  [Fact(Skip = "need to address this")]
  // TODO: fix this
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerDocument_WhenFeaturesUnavailable_IncludesWarningsInOperations()
  {
    // Arrange
    // Create a custom factory with a mock configuration status service
    var mockStatusService = new Mock<IStatusService>();
    mockStatusService.Setup(s => s.GetServiceStatusAsync())
      .ReturnsAsync(
        new Dictionary<string, StatusInfo> { { "Llm", new StatusInfo(IsAvailable: false, ["Llm:ApiKey"]) } });

    mockStatusService.Setup(s => s.GetFeatureStatus("Llm"))
      .Returns(new StatusInfo(IsAvailable: false, ["Llm:ApiKey"]));

    mockStatusService.Setup(s => s.IsFeatureAvailable("Llm"))
      .Returns(false);

    var customFactory = Factory as CustomWebApplicationFactory;
    var factoryWithWebHostBuilder = customFactory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services => { services.AddSingleton(mockStatusService.Object); });
    });

    using var client = customFactory.CreateAuthenticatedClient("admin-user", ["Admin"]);

    // Act
    var response = await client.GetAsync("/swagger/swagger.json");

    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();

    // Look for the AI controller endpoints (using basic string search)
    content.Should().Contain("/api/completion");

    // Check for warning symbols in the AI endpoints
    content.Should().Contain("⚠️", "Unavailable endpoints should have warning symbols in their descriptions");
    content.Should().Contain("Llm:ApiKey", "Missing configuration should be mentioned in the endpoint description");
  }
}
