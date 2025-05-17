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
using ExternalServices = Zarichney.Services.Status.ExternalServices;

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
public class ServiceUnavailabilityTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
  : IntegrationTestBase(apiClientFixture, testOutputHelper)
{
  /// <summary>
  /// Tests that API endpoints return HTTP 503 when a required feature is unavailable.
  /// This test configures a custom WebApplicationFactory that simulates LLM service
  /// being unavailable due to missing configuration.
  /// </summary>
  [Fact(Skip = "TODO: upgrade refitter/refit client to properly supply form data")]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task Endpoint_WhenRequiredFeatureIsUnavailable_Returns503WithErrorDetails()
  {
    // Arrange
    var textPrompt = GetRandom.String();
    // todo: consider adding an inline test for each service types and hitting each endpoint that is expected to throw 504. skip test when everything is operational

    // Act
    // This should trigger the mock LLM service which throws ServiceUnavailableException
    ApiException? exception = null;
    try
    {
      // todo: fix refitter/refit client to properly supply form data
      await AuthenticatedApiClient.Completion(textPrompt, null!);
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
      .ReturnsAsync(new Dictionary<ExternalServices, StatusInfo>
      {
        {
          ExternalServices.OpenAiApi,
          new StatusInfo(serviceName: ExternalServices.OpenAiApi, IsAvailable: false, ["Llm:ApiKey"])
        },
        { ExternalServices.MsGraph, new StatusInfo(serviceName: ExternalServices.MsGraph, IsAvailable: true, []) },
        {
          ExternalServices.Stripe,
          new StatusInfo(serviceName: ExternalServices.Stripe, IsAvailable: false, ["Payment:StripeKey"])
        }
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
    var result = await client.StatusAll();

    // Assert
    result.Should().NotBeNull();
    result.Should().Contain(c => c.ServiceName == (Client.ExternalServices)ExternalServices.OpenAiApi);
    result.Should().Contain(c => c.ServiceName == (Client.ExternalServices)ExternalServices.MsGraph);
    result.Should().Contain(c => c.ServiceName == (Client.ExternalServices)ExternalServices.Stripe);

    var openAiApiStatus = result.Single(c => c.ServiceName == (Client.ExternalServices)ExternalServices.OpenAiApi);
    openAiApiStatus.IsAvailable.Should().BeFalse();
    openAiApiStatus.MissingConfigurations.Should().Contain("Llm:ApiKey");

    var graphApiStatus = result.Single(c => c.ServiceName == (Client.ExternalServices)ExternalServices.MsGraph);
    graphApiStatus.IsAvailable.Should().BeTrue();
    graphApiStatus.MissingConfigurations.Should().BeEmpty();

    var stripeApiStatus = result.Single(c => c.ServiceName == (Client.ExternalServices)ExternalServices.Stripe);
    stripeApiStatus.IsAvailable.Should().BeFalse();
    stripeApiStatus.MissingConfigurations.Should().Contain("Payment:StripeKey");
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
        new Dictionary<ExternalServices, StatusInfo>
        {
          { ExternalServices.OpenAiApi, new StatusInfo(serviceName: ExternalServices.OpenAiApi,IsAvailable: false, ["Llm:ApiKey"]) }
        });

    mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.OpenAiApi))
      .Returns(new StatusInfo(serviceName: ExternalServices.OpenAiApi, IsAvailable: false, ["Llm:ApiKey"]));

    mockStatusService.Setup(s => s.IsFeatureAvailable(ExternalServices.OpenAiApi))
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
