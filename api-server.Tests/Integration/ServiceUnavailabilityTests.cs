using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Refit;
using Xunit;
using Zarichney.Client;
using Zarichney.Config;
using Zarichney.Services.AI;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

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
public class ServiceUnavailabilityTests(ApiClientFixture apiClientFixture) : IntegrationTestBase(apiClientFixture)
{
  private const string TestUserId = "test-user";
  private static readonly string[] TestUserRoles = ["User"];

  /// <summary>
  /// Tests that API endpoints return HTTP 503 when a required feature is unavailable.
  /// This test configures a custom WebApplicationFactory that simulates LLM service
  /// being unavailable due to missing configuration.
  /// </summary>
  [Fact(Skip = "The test is currently unstable due to HTTP 400 instead of 503 response")]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task Endpoint_WhenRequiredFeatureIsUnavailable_Returns503WithErrorDetails()
  {
    // Arrange
    // Create a custom factory that indicates the LLM service is unavailable
    var missingConfigs = new List<string> { "Llm:ApiKey" };
    var llmServiceStatus = new StatusInfo(IsAvailable: false, missingConfigs);

    var mockStatusService = new Mock<IConfigurationStatusService>();
    mockStatusService.Setup(s => s.GetServiceStatusAsync())
      .ReturnsAsync(new Dictionary<string, StatusInfo> { { "Llm", llmServiceStatus } });

    mockStatusService.Setup(s => s.IsFeatureAvailable("Llm"))
      .Returns(false);

    var customFactory = Factory as CustomWebApplicationFactory;
    var factoryWithServices = customFactory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
      {
        // Replace the real ConfigurationStatusService with our mock
        services.AddSingleton(mockStatusService.Object);

        // Replace the OpenAI client factory to throw ServiceUnavailableException
        services.AddScoped<OpenAI.OpenAIClient>(sp =>
        {
          var exception = new ServiceUnavailableException(
            "LLM service is unavailable due to missing configuration");
          if (exception.Reasons != null)
          {
            exception.Reasons.Add("Llm:ApiKey");
          }

          throw exception;
        });

        // Replace the LLM service to use the proxies
        services.AddScoped<ILlmService>(sp =>
        {
          var mockLlmService = new Mock<ILlmService>();
          var llmException = new ServiceUnavailableException(
            "LLM service is unavailable due to missing configuration");
          if (llmException.Reasons != null)
          {
            llmException.Reasons.Add("Llm:ApiKey");
          }

          // Use a callback approach to avoid expression tree limitations with It.IsAny<string>()
          mockLlmService.Setup(s => s.GetCompletionContent(
              It.IsAny<string>(),
              It.IsAny<string>(),
              null,
              null))
            .Callback<string, string?, OpenAI.Chat.ChatCompletionOptions?, int?>((prompt, convId, options, retry) =>
            {
              // The callback is empty, we just need to match the signature
            })
            .Throws(llmException);
          return mockLlmService.Object;
        });
      });
    });

    // Create an authenticated client for testing
    using var httpClient = customFactory.CreateAuthenticatedClient(TestUserId, TestUserRoles);
    var client = RestService.For<IZarichneyAPI>(httpClient);

    // Prepare a request to the AI completion endpoint
    var textPrompt = "Test prompt";

    // Act & Assert
    // Call the AI completion endpoint that requires LLM service
    var action = async () => await client.Completion(textPrompt, null!);
    // Test that the action throws an ApiException
    ApiException? exception = null;
    try
    {
      await action();
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
    errorContent.Should().Contain("Llm:ApiKey");

    // Verify that a public endpoint is still available
    await client.Health(); // The extension method will throw if there's an error
  }

  /// <summary>
  /// Tests that the /api/status endpoint correctly reports service availability.
  /// </summary>
  [Fact(Skip = "The test is currently unstable due to HttpClient extraction from Refit client")]
  [Trait(TestCategories.Feature, "Status")]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task StatusEndpoint_ReturnsServiceAvailabilityInformation()
  {
    // Arrange
    // Create a custom factory that indicates mixed service availability
    var mockStatusService = new Mock<IConfigurationStatusService>();
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
  [SkipSwaggerIntegrationFact]
  [Trait(TestCategories.Feature, TestCategories.Swagger)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task SwaggerDocument_WhenFeaturesUnavailable_IncludesWarningsInOperations()
  {
    // Arrange
    // Create a custom factory with a mock configuration status service
    var mockStatusService = new Mock<IConfigurationStatusService>();
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
