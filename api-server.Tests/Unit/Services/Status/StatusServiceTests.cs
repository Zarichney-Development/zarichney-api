using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Zarichney.Services.AI;
using Zarichney.Services.Email;
using Zarichney.Services.GitHub;
using Zarichney.Services.Payment;
using Zarichney.Services.Status;

namespace Zarichney.Tests.Unit.Services.Status;

public class StatusServiceTests
{
  private const string ValidApiKey = "valid-api-key";
  private const string ValidConnectionString = "valid-connection-string";
  private const string MissingValue = "recommended to set in app secrets";

  private readonly Mock<IConfiguration> _mockConfiguration;
  private StatusService _statusService;
  private LlmConfig _llmConfig = null!;
  private EmailConfig _emailConfig = null!;
  private GitHubConfig _gitHubConfig = null!;
  private PaymentConfig _paymentConfig = null!;

  public StatusServiceTests()
  {
    _mockConfiguration = new Mock<IConfiguration>();

    // Initialize configs with missing values
    InitializeConfigs(MissingValue);

    // Mock configuration to return connection string
    _mockConfiguration.Setup(c => c["ConnectionStrings:IdentityConnection"]).Returns(ValidConnectionString);

    _statusService = new StatusService(
        _llmConfig,
        _emailConfig,
        _gitHubConfig,
        _paymentConfig,
        _mockConfiguration.Object
    );
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetConfigurationStatus_WhenAllServicesAreConfigured_ReturnsAllItemsAsConfigured()
  {
    // Arrange
    InitializeConfigs(ValidApiKey);
    _statusService = new StatusService(_llmConfig, _emailConfig, _gitHubConfig, _paymentConfig, _mockConfiguration.Object);

    // Act
    var result = await _statusService.GetConfigurationStatusAsync();

    // Assert
    result.Should().NotBeNull();
    result.Should().AllSatisfy(status =>
    {
      status.Status.Should().Be("Configured",
              $"because {status.Name} should have a valid configuration");
    });
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetConfigurationStatus_WhenDatabaseConnectionIsMissing_ReturnsDatabaseItemAsNotConfigured()
  {
    // Arrange
    _mockConfiguration.Setup(c => c["ConnectionStrings:IdentityConnection"]).Returns((string?)null);

    // Act
    var result = await _statusService.GetConfigurationStatusAsync();

    // Assert
    result.Should().Contain(status =>
        status.Name == "Database Connection" &&
        status.Status == "Missing/Invalid" &&
        status.Details == "IdentityConnection is missing or placeholder");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetConfigurationStatus_WhenLlmApiKeyIsMissing_ReturnsOpenAiItemAsNotConfigured()
  {
    // Arrange
    InitializeConfigs(ValidApiKey);
    _llmConfig = new LlmConfig { ApiKey = MissingValue };
    _statusService = new StatusService(_llmConfig, _emailConfig, _gitHubConfig, _paymentConfig, _mockConfiguration.Object);

    // Act
    var result = await _statusService.GetConfigurationStatusAsync();

    // Assert
    result.Should().Contain(status =>
        status.Name == "OpenAI API Key" &&
        status.Status == "Missing/Invalid" &&
        status.Details == "ApiKey is missing or placeholder");
  }

  private void InitializeConfigs(string value)
  {
    _llmConfig = new LlmConfig { ApiKey = value };
    _emailConfig = new EmailConfig
    {
      AzureTenantId = value,
      AzureAppId = value,
      AzureAppSecret = value,
      FromEmail = "test@example.com",
      MailCheckApiKey = value
    };
    _gitHubConfig = new GitHubConfig { AccessToken = value };
    _paymentConfig = new PaymentConfig
    {
      StripeSecretKey = value,
      StripeWebhookSecret = value
    };
  }
}
