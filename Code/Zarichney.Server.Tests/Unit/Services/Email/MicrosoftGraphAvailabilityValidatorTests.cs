using FluentAssertions;
using Xunit;
using Zarichney.Services.Email;
using Zarichney.Services.Status;

namespace Zarichney.Server.Tests.Unit.Services.Email;

/// <summary>
/// Unit tests for MicrosoftGraphAvailabilityValidator covering configuration validation logic
/// for Microsoft Graph service availability. Tests focus on validation of configuration
/// values and placeholder detection introduced in the Microsoft Graph service registration hotfix.
/// </summary>
public class MicrosoftGraphAvailabilityValidatorTests
{
  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_WhenAllConfigurationsAreValid_ReturnsAvailableWithNoMissingConfigs()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "valid-tenant-id",
      AzureAppId = "valid-app-id",
      AzureAppSecret = "valid-app-secret",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeTrue("because all Microsoft Graph configuration values are properly set");
    missingConfigs.Should().BeNull("because no configuration values are missing");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_WhenAzureTenantIdIsNull_ReturnsUnavailableWithMissingConfig()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = null!,
      AzureAppId = "valid-app-id",
      AzureAppSecret = "valid-app-secret",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeFalse("because AzureTenantId is null");
    missingConfigs.Should().NotBeNull("because configuration is missing");
    missingConfigs.Should().ContainSingle("because only AzureTenantId is missing");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureTenantIdKey);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_WhenAzureTenantIdIsEmpty_ReturnsUnavailableWithMissingConfig()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "",
      AzureAppId = "valid-app-id",
      AzureAppSecret = "valid-app-secret",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeFalse("because AzureTenantId is empty");
    missingConfigs.Should().NotBeNull("because configuration is missing");
    missingConfigs.Should().ContainSingle("because only AzureTenantId is missing");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureTenantIdKey);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_WhenAzureTenantIdIsWhitespace_ReturnsUnavailableWithMissingConfig()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "   ",
      AzureAppId = "valid-app-id",
      AzureAppSecret = "valid-app-secret",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeFalse("because AzureTenantId is whitespace only");
    missingConfigs.Should().NotBeNull("because configuration is missing");
    missingConfigs.Should().ContainSingle("because only AzureTenantId is missing");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureTenantIdKey);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_WhenAzureTenantIdIsPlaceholder_ReturnsUnavailableWithMissingConfig()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = StatusService.PlaceholderMessage,
      AzureAppId = "valid-app-id",
      AzureAppSecret = "valid-app-secret",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeFalse("because AzureTenantId contains placeholder message");
    missingConfigs.Should().NotBeNull("because configuration is missing");
    missingConfigs.Should().ContainSingle("because only AzureTenantId is missing");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureTenantIdKey);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_WhenAzureAppIdIsMissing_ReturnsUnavailableWithMissingConfig()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "valid-tenant-id",
      AzureAppId = null!,
      AzureAppSecret = "valid-app-secret",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeFalse("because AzureAppId is null");
    missingConfigs.Should().NotBeNull("because configuration is missing");
    missingConfigs.Should().ContainSingle("because only AzureAppId is missing");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureAppIdKey);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_WhenAzureAppSecretIsMissing_ReturnsUnavailableWithMissingConfig()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "valid-tenant-id",
      AzureAppId = "valid-app-id",
      AzureAppSecret = "",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeFalse("because AzureAppSecret is empty");
    missingConfigs.Should().NotBeNull("because configuration is missing");
    missingConfigs.Should().ContainSingle("because only AzureAppSecret is missing");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureAppSecretKey);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_WhenMultipleConfigurationsAreMissing_ReturnsAllMissingConfigs()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "",
      AzureAppId = StatusService.PlaceholderMessage,
      AzureAppSecret = null!,
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeFalse("because multiple Microsoft Graph configuration values are missing");
    missingConfigs.Should().NotBeNull("because configuration values are missing");
    missingConfigs.Should().HaveCount(3, "because three configuration values are missing");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureTenantIdKey);
    missingConfigs.Should().Contain(EmailConfigConstants.AzureAppIdKey);
    missingConfigs.Should().Contain(EmailConfigConstants.AzureAppSecretKey);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_WhenAllConfigurationsAreMissing_ReturnsAllMissingConfigs()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = null!,
      AzureAppId = "",
      AzureAppSecret = "   ",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeFalse("because all Microsoft Graph configuration values are missing");
    missingConfigs.Should().NotBeNull("because configuration values are missing");
    missingConfigs.Should().HaveCount(3, "because all three Microsoft Graph configuration values are missing");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureTenantIdKey);
    missingConfigs.Should().Contain(EmailConfigConstants.AzureAppIdKey);
    missingConfigs.Should().Contain(EmailConfigConstants.AzureAppSecretKey);
  }

  [Trait("Category", "Unit")]
  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("\t")]
  [InlineData("\n")]
  [InlineData("\r\n")]
  [InlineData(StatusService.PlaceholderMessage)]
  public void ValidateConfiguration_WhenConfigurationValueIsInvalid_ReturnsUnavailable(string invalidValue)
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = invalidValue,
      AzureAppId = "valid-app-id",
      AzureAppSecret = "valid-app-secret",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeFalse($"because '{invalidValue}' is not a valid configuration value");
    missingConfigs.Should().NotBeNull("because configuration is missing or invalid");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureTenantIdKey);
  }

  [Trait("Category", "Unit")]
  [Theory]
  [InlineData("valid-value")]
  [InlineData("12345")]
  [InlineData("tenant-id-with-special-chars-!@#$%")]
  [InlineData("very-long-configuration-value-that-should-still-be-considered-valid-1234567890")]
  public void ValidateConfiguration_WhenConfigurationValueIsValid_ReturnsAvailable(string validValue)
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = validValue,
      AzureAppId = validValue,
      AzureAppSecret = validValue,
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeTrue($"because '{validValue}' is a valid configuration value");
    missingConfigs.Should().BeNull("because all configuration values are valid");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_WhenEmailConfigIsNull_ThrowsArgumentNullException()
  {
    // Act & Assert
    var exception = Assert.Throws<NullReferenceException>(() =>
        MicrosoftGraphAvailabilityValidator.ValidateConfiguration(null!));

    exception.Should().NotBeNull("because null EmailConfig should throw an exception");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_ReturnsTupleWithCorrectStructure()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "valid-tenant-id",
      AzureAppId = "valid-app-id",
      AzureAppSecret = "valid-app-secret",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var result = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    result.Should().BeOfType<(bool IsAvailable, List<string>? MissingConfigs)>("because the method returns a tuple");
    result.IsAvailable.Should().BeTrue("because all configuration values are valid");
    result.MissingConfigs.Should().BeNull("because MissingConfigs should be null when all configs are valid");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_UsesCorrectEmailConfigConstants()
  {
    // Arrange
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "",
      AzureAppId = "",
      AzureAppSecret = "",
      FromEmail = "test@example.com",
      MailCheckApiKey = "valid-mail-check-key"
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    missingConfigs.Should().NotBeNull("because configuration values are missing");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureTenantIdKey,
        "because the method should use the correct constant for AzureTenantId");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureAppIdKey,
        "because the method should use the correct constant for AzureAppId");
    missingConfigs.Should().Contain(EmailConfigConstants.AzureAppSecretKey,
        "because the method should use the correct constant for AzureAppSecret");

    // Verify the constants have the expected values
    EmailConfigConstants.AzureTenantIdKey.Should().Be("EmailConfig:AzureTenantId");
    EmailConfigConstants.AzureAppIdKey.Should().Be("EmailConfig:AzureAppId");
    EmailConfigConstants.AzureAppSecretKey.Should().Be("EmailConfig:AzureAppSecret");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ValidateConfiguration_DoesNotValidateNonMicrosoftGraphProperties()
  {
    // Arrange - FromEmail and MailCheckApiKey are missing, but they're not Microsoft Graph specific
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "valid-tenant-id",
      AzureAppId = "valid-app-id",
      AzureAppSecret = "valid-app-secret",
      FromEmail = "", // This is missing but not Microsoft Graph specific
      MailCheckApiKey = "" // This is missing but not Microsoft Graph specific
    };

    // Act
    var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfig);

    // Assert
    isAvailable.Should().BeTrue("because all Microsoft Graph specific configurations are present");
    missingConfigs.Should().BeNull("because Microsoft Graph validator only checks Graph-specific properties");
  }
}
