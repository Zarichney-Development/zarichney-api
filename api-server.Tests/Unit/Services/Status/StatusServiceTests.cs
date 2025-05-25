using Zarichney.Services.Status;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.AI;
using Zarichney.Services.Email;
using Zarichney.Services.GitHub;
using Zarichney.Services.Payment;

namespace Zarichney.Tests.Unit.Services.Status;

public class StatusServiceTests
{
  private const string ValidApiKey = "valid-api-key";
  private const string ValidConnectionString = "valid-connection-string";

  private readonly Mock<IConfiguration> _mockConfiguration;
  private readonly Mock<IServiceProvider> _mockServiceProvider;
  private readonly Mock<ILogger<StatusService>> _mockLogger;
  private readonly Mock<Assembly> _mockAssembly;
  private StatusService _statusService;
  private LlmConfig _llmConfig = null!;
  private EmailConfig _emailConfig = null!;
  private GitHubConfig _gitHubConfig = null!;
  private PaymentConfig _paymentConfig = null!;

  public StatusServiceTests()
  {
    _mockConfiguration = new Mock<IConfiguration>();
    _mockServiceProvider = new Mock<IServiceProvider>();
    _mockLogger = new Mock<ILogger<StatusService>>();
    _mockAssembly = new Mock<Assembly>();

    // Initialize configs with missing values
    InitializeConfigs(StatusService.PlaceholderMessage);

    // Mock configuration to return connection string using indexer rather than the extension method
    _mockConfiguration.Setup(c => c[$"ConnectionStrings:{Zarichney.Services.Auth.UserDbContext.UserDatabaseConnectionName}"]).Returns(ValidConnectionString);

    // Setup mock assembly to return test config types
    _mockAssembly.Setup(asm => asm.GetTypes()).Returns([typeof(TestService1Config), typeof(TestService2Config)]);

    _statusService = new StatusService(
        _llmConfig,
        _emailConfig,
        _gitHubConfig,
        _paymentConfig,
        _mockConfiguration.Object,
        _mockServiceProvider.Object,
        _mockLogger.Object,
        _mockAssembly.Object
    );
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetConfigurationStatus_WhenAllServicesAreConfigured_ReturnsAllItemsAsConfigured()
  {
    // Arrange
    InitializeConfigs(ValidApiKey);
    _statusService = new StatusService(
        _llmConfig,
        _emailConfig,
        _gitHubConfig,
        _paymentConfig,
        _mockConfiguration.Object,
        _mockServiceProvider.Object,
        _mockLogger.Object,
        _mockAssembly.Object
    );

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
    _mockConfiguration.Setup(c => c[$"ConnectionStrings:{Zarichney.Services.Auth.UserDbContext.UserDatabaseConnectionName}"]).Returns((string?)null);

    // Act
    var result = await _statusService.GetConfigurationStatusAsync();

    // Assert
    result.Should().Contain(status =>
        status.Name == "Identity Database Connection" &&
        status.Status == "Missing/Invalid" &&
        status.Details == $"{Zarichney.Services.Auth.UserDbContext.UserDatabaseConnectionName} is missing or placeholder");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetConfigurationStatus_WhenLlmApiKeyIsMissing_ReturnsOpenAiItemAsNotConfigured()
  {
    // Arrange
    InitializeConfigs(ValidApiKey);
    _llmConfig = new LlmConfig { ApiKey = StatusService.PlaceholderMessage };
    _statusService = new StatusService(
        _llmConfig,
        _emailConfig,
        _gitHubConfig,
        _paymentConfig,
        _mockConfiguration.Object,
        _mockServiceProvider.Object,
        _mockLogger.Object,
        _mockAssembly.Object
    );

    // Act
    var result = await _statusService.GetConfigurationStatusAsync();

    // Assert
    result.Should().Contain(status =>
        status.Name == "OpenAI API Key" &&
        status.Status == "Missing/Invalid" &&
        status.Details == "ApiKey is missing or placeholder");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetServiceStatusAsync_WithNoRegisteredConfigs_ReturnsEmptyDictionary()
  {
    // Arrange
    SetupEmptyServiceProvider();

    // Act
    var result = await _statusService.GetServiceStatusAsync();

    // Assert
    result.Should().BeEmpty();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetServiceStatusAsync_WithAllConfigsAvailable_ReturnsAvailableServices()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithValidValues();

    // Act
    var result = await _statusService.GetServiceStatusAsync();

    // Assert
    result.Should().NotBeEmpty("service status result should contain entries");

    // More resilient approach using dictionary lookups that handles structure changes better
    result.Should().ContainKey(ExternalServices.OpenAiApi, "OpenAI API should be included in the results");

    var openAiStatus = result[ExternalServices.OpenAiApi];
    openAiStatus.Should().NotBeNull("OpenAI status should be available");
    openAiStatus.serviceName.Should().Be(ExternalServices.OpenAiApi, "service name should match the key");
    openAiStatus.IsAvailable.Should().BeTrue("OpenAI API should be available with valid configuration");
    openAiStatus.MissingConfigurations.Should().BeEmpty("no missing configurations should be reported");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetServiceStatusAsync_WithMissingConfigs_ReturnsUnavailableServices()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithMissingValues();

    // Act
    var result = await _statusService.GetServiceStatusAsync();

    // Assert
    result.Should().NotBeEmpty();
    result.Should().ContainKey(ExternalServices.OpenAiApi);
    result[ExternalServices.OpenAiApi].IsAvailable.Should().BeFalse();
    result[ExternalServices.OpenAiApi].MissingConfigurations.Should().Contain("TestService1Config:ApiKey");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetServiceStatusAsync_WithPlaceholderValues_ReturnsUnavailableServices()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithPlaceholderValues();

    // Act
    var result = await _statusService.GetServiceStatusAsync();

    // Assert
    result.Should().NotBeEmpty();
    result.Should().ContainKey(ExternalServices.OpenAiApi);
    result[ExternalServices.OpenAiApi].IsAvailable.Should().BeFalse();
    result[ExternalServices.OpenAiApi].MissingConfigurations.Should().Contain("TestService1Config:ApiKey");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetFeatureStatus_WithValidFeature_ReturnsCorrectStatus()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithValidValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _statusService.GetServiceStatusAsync();
    var result = _statusService.GetFeatureStatus(ExternalServices.OpenAiApi);

    // Assert
    result.Should().NotBeNull();
    result.IsAvailable.Should().BeTrue();
    result.MissingConfigurations.Should().BeEmpty();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetFeatureStatus_WithInvalidFeature_ReturnsNull()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithValidValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _statusService.GetServiceStatusAsync();
    // Use a value that doesn't exist in our test data
    var result = _statusService.GetFeatureStatus(ExternalServices.MsGraph);

    // Assert
    result.Should().BeNull();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetFeatureStatus_WithUnavailableFeature_ReturnsCorrectStatus()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithMissingValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _statusService.GetServiceStatusAsync();
    var result = _statusService.GetFeatureStatus(ExternalServices.OpenAiApi);

    // Assert
    result.Should().NotBeNull();
    result.IsAvailable.Should().BeFalse();
    result.MissingConfigurations.Should().Contain("TestService1Config:ApiKey");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task IsFeatureAvailable_WithAvailableFeature_ReturnsTrue()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithValidValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _statusService.GetServiceStatusAsync();
    var result = _statusService.IsFeatureAvailable(ExternalServices.OpenAiApi);

    // Assert
    result.Should().BeTrue();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task IsFeatureAvailable_WithUnavailableFeature_ReturnsFalse()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithMissingValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _statusService.GetServiceStatusAsync();
    var result = _statusService.IsFeatureAvailable(ExternalServices.OpenAiApi);

    // Assert
    result.Should().BeFalse();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task IsFeatureAvailable_WithNonExistentFeature_ReturnsFalse()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithValidValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _statusService.GetServiceStatusAsync();
    // Use a value that doesn't exist in our test data
    var result = _statusService.IsFeatureAvailable(ExternalServices.MsGraph);

    // Assert
    result.Should().BeFalse();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void SetServiceAvailability_ForIdentityDb_ShouldUpdateStatus()
  {
    // Arrange
    var missingConfigs = new List<string> { $"ConnectionStrings:{Zarichney.Services.Auth.UserDbContext.UserDatabaseConnectionName}" };

    // Act
    _statusService.SetServiceAvailability(ExternalServices.PostgresIdentityDb, false, missingConfigs);
    var result = _statusService.GetFeatureStatus(ExternalServices.PostgresIdentityDb);

    // Assert
    result.Should().NotBeNull();
    result!.IsAvailable.Should().BeFalse();
    result.MissingConfigurations.Should().ContainSingle()
      .Which.Should().Be("ConnectionStrings:UserDatabase");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task SetServiceAvailability_ForIdentityDb_ShouldUpdateAvailabilityEvenAfterGetServiceStatusAsync()
  {
    // Arrange - Mark the service as unavailable
    _statusService.SetServiceAvailability(ExternalServices.PostgresIdentityDb, false,
      new List<string> { "ConnectionStrings:UserDatabase" });

    // Act - Call GetServiceStatusAsync() which shouldn't override our setting
    _ = await _statusService.GetServiceStatusAsync();
    var dbStatus = _statusService.GetFeatureStatus(ExternalServices.PostgresIdentityDb);

    // Assert - Service should still be unavailable
    dbStatus.Should().NotBeNull();
    dbStatus!.IsAvailable.Should().BeFalse();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void IsFeatureAvailable_ForIdentityDb_ShouldReturnCorrectValue()
  {
    // Arrange
    _statusService.SetServiceAvailability(ExternalServices.PostgresIdentityDb, true);

    // Act
    var result = _statusService.IsFeatureAvailable(ExternalServices.PostgresIdentityDb);

    // Assert
    result.Should().BeTrue();

    // Change the status and verify the result changes
    _statusService.SetServiceAvailability(ExternalServices.PostgresIdentityDb, false);
    result = _statusService.IsFeatureAvailable(ExternalServices.PostgresIdentityDb);
    result.Should().BeFalse();
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

  private void SetupEmptyServiceProvider()
  {
    // Ensure GetTypes returns an empty array for this test case
    _mockAssembly.Setup(asm => asm.GetTypes()).Returns(Type.EmptyTypes);

    // Mock GetService to return null for any type, simulating no registered IConfig services
    _mockServiceProvider.Setup(p => p.GetService(It.IsAny<Type>())).Returns(null!);
  }

  private void SetupServiceProviderWithTestConfigs()
  {
    // Reset GetTypes to return test config types for other tests
    _mockAssembly.Setup(asm => asm.GetTypes()).Returns([typeof(TestService1Config), typeof(TestService2Config)]);

    _mockServiceProvider.Setup(p => p.GetService(typeof(TestService1Config)))
        .Returns(new TestService1Config());
    _mockServiceProvider.Setup(p => p.GetService(typeof(TestService2Config)))
        .Returns(new TestService2Config());
  }

  private void SetupConfigurationWithValidValues()
  {
    _mockConfiguration.Setup(c => c["TestService1Config:ApiKey"]).Returns("valid-api-key");
    _mockConfiguration.Setup(c => c["TestService1Config:ApiSecret"]).Returns("valid-api-secret");
    _mockConfiguration.Setup(c => c["TestService2Config:Setting"]).Returns("valid-setting");
  }

  private void SetupConfigurationWithMissingValues()
  {
    _mockConfiguration.Setup(c => c["TestService1Config:ApiKey"]).Returns((string?)null);
  }

  private void SetupConfigurationWithPlaceholderValues()
  {
    _mockConfiguration.Setup(c => c["TestService1Config:ApiKey"]).Returns(StatusService.PlaceholderMessage);
  }

  // Test Config Classes
  private class TestService1Config : IConfig
  {
    [RequiresConfiguration(ExternalServices.OpenAiApi)]
    public string ApiKey { get; set; } = "valid_key"; // Default to valid for simplicity in some tests

    [RequiresConfiguration(ExternalServices.OpenAiApi)]
    public string ApiSecret { get; set; } = "valid_secret";
  }

  private class TestService2Config : IConfig
  {
    [RequiresConfiguration(ExternalServices.FrontEnd)]
    public string Setting { get; set; } = "valid_setting";
  }
}
