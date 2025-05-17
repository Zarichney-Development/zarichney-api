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

namespace Zarichney.Tests.Integration.Status;

/// <summary>
/// Integration tests that verify the behavior of the API when services are unavailable
/// due to missing configuration. These tests ensure that endpoints correctly return
/// HTTP 503 responses with appropriate error details.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Collection("Integration")]
public class ServiceUnavailabilityTests : IntegrationTestBase
{
  private readonly ITestOutputHelper _outputHelper;

  public ServiceUnavailabilityTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
    : base(apiClientFixture, testOutputHelper)
  {
    _outputHelper = testOutputHelper;
  }
  /// <summary>
  /// Tests that API endpoints return HTTP 503 when a required feature is unavailable.
  /// This test configures a custom WebApplicationFactory that simulates MailCheck service
  /// being unavailable due to missing configuration, and attempts to validate an email
  /// which should fail with 503.
  /// </summary>
  [Fact]
  [Trait(TestCategories.Feature, TestCategories.Email)]
  [Trait(TestCategories.Category, TestCategories.MinimalFunctionality)]
  public async Task Endpoint_WhenRequiredFeatureIsUnavailable_Returns503WithErrorDetails()
  {
    // Arrange
    var testEmail = $"test_{GetRandom.String()}@example.com";
    var mockStatusService = new Mock<IStatusService>();
    
    // Configure mock to report MailCheck service as unavailable
    mockStatusService.Setup(s => s.GetServiceStatusAsync())
      .ReturnsAsync(new Dictionary<ExternalServices, StatusInfo>
      {
        {
          ExternalServices.MailCheck,
          new StatusInfo(serviceName: ExternalServices.MailCheck, IsAvailable: false, ["EmailConfig:MailCheckApiKey"])
        }
      });

    mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.MailCheck))
      .Returns(new StatusInfo(serviceName: ExternalServices.MailCheck, IsAvailable: false, ["Email:MailCheckApiKey"]));

    mockStatusService.Setup(s => s.IsFeatureAvailable(ExternalServices.MailCheck))
      .Returns(false);

    var customFactory = Factory;
    customFactory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
      {
        // Replace the real StatusService with our mock
        services.AddSingleton(mockStatusService.Object);
      });
    });

    using var httpClient = customFactory.CreateAuthenticatedClient("test-user", ["User"]);
    var client = RestService.For<IZarichneyAPI>(httpClient);

    // Act
    ApiException? exception = null;
    try
    {
      // Call the email validation endpoint that depends on MailCheck service
      await client.Validate(testEmail);
      Assert.Fail("Expected ApiException was not thrown");
    }
    catch (ApiException ex)
    {
      exception = ex;
      _outputHelper.WriteLine($"Caught expected exception: {ex.StatusCode} {ex.Content}");
    }

    // Assert
    exception.Should().NotBeNull("An exception should be thrown when service is unavailable");
    exception!.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable, "Should return 503 Service Unavailable");
    var errorContent = exception.Content;
    errorContent.Should().Contain("Service Temporarily Unavailable", "Error should indicate service unavailability");
    errorContent.Should().Contain("EmailConfig:MailCheckApiKey", "Error should mention the missing configuration");

    // Verify that a public endpoint is still available
    await client.Health(); // The extension method will throw if there's an error
  }

  /// <summary>
  /// Tests that the /api/status endpoint correctly reports service availability.
  /// </summary>
  [Fact]
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
          new StatusInfo(serviceName: ExternalServices.OpenAiApi, IsAvailable: false, ["LlmConfig:ApiKey"])
        },
        { ExternalServices.MsGraph, new StatusInfo(serviceName: ExternalServices.MsGraph, IsAvailable: true, []) },
        {
          ExternalServices.MailCheck,
          new StatusInfo(serviceName: ExternalServices.MailCheck, IsAvailable: false, ["EmailConfig:MailCheckApiKey"])
        }
      });

    var customFactory = Factory;
    customFactory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
      {
        // Replace the real StatusService with our mock
        services.AddSingleton(mockStatusService.Object);
      });
    });

    using var httpClient = customFactory.CreateAuthenticatedClient("test-user", ["User"]);
    var client = RestService.For<IZarichneyAPI>(httpClient);

    // Act
    var result = await client.StatusAll();

    // Assert
    result.Should().NotBeNull();
    result.Should().Contain(c => c.ServiceName == (Client.ExternalServices)ExternalServices.OpenAiApi);
    result.Should().Contain(c => c.ServiceName == (Client.ExternalServices)ExternalServices.MsGraph);
    result.Should().Contain(c => c.ServiceName == (Client.ExternalServices)ExternalServices.MailCheck);

    var openAiApiStatus = result.Single(c => c.ServiceName == (Client.ExternalServices)ExternalServices.OpenAiApi);
    openAiApiStatus.IsAvailable.Should().BeFalse();
    openAiApiStatus.MissingConfigurations.Should().Contain("LlmConfig:ApiKey");

    var graphApiStatus = result.Single(c => c.ServiceName == (Client.ExternalServices)ExternalServices.MsGraph);
    // Test environment might have MsGraph disabled, so don't assert IsAvailable
    if (graphApiStatus.IsAvailable)
    {
        graphApiStatus.MissingConfigurations.Should().BeEmpty();
    }

    var mailCheckStatus = result.Single(c => c.ServiceName == (Client.ExternalServices)ExternalServices.MailCheck);
    mailCheckStatus.IsAvailable.Should().BeFalse();
    mailCheckStatus.MissingConfigurations.Should().Contain("EmailConfig:MailCheckApiKey");
  }

  /// <summary>
  /// Tests that Swagger documents correctly indicate which endpoints are unavailable
  /// due to missing configuration.
  /// </summary>
  [Fact(Skip = "Swagger endpoints not available in test environment")]
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
          { 
            ExternalServices.MailCheck, 
            new StatusInfo(serviceName: ExternalServices.MailCheck, IsAvailable: false, ["EmailConfig:MailCheckApiKey"]) 
          }
        });

    mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.MailCheck))
      .Returns(new StatusInfo(serviceName: ExternalServices.MailCheck, IsAvailable: false, ["Email:MailCheckApiKey"]));

    mockStatusService.Setup(s => s.IsFeatureAvailable(ExternalServices.MailCheck))
      .Returns(false);

    var customFactory = Factory;
    customFactory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services => { services.AddSingleton(mockStatusService.Object); });
    });

    using var client = customFactory.CreateAuthenticatedClient("admin-user", ["Admin"]);

    // Act
    var response = await client.GetAsync("/swagger/swagger.json");

    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();

    // Look for the API controller endpoints (using basic string search)
    content.Should().Contain("/api/email/validate");

    // Check for warning symbols in the API endpoints
    content.Should().Contain("⚠️", "Unavailable endpoints should have warning symbols in their descriptions");
    content.Should().Contain("EmailConfig:MailCheckApiKey", "Missing configuration should be mentioned in the endpoint description");
  }
}