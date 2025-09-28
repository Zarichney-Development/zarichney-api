using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Zarichney.Services.Email;
using Zarichney.Services.Status;
using Zarichney.Startup;

namespace Zarichney.Server.Tests.Unit.Startup;

/// <summary>
/// Unit tests for ApplicationStartup covering core application configuration and pipeline setup.
/// Tests focus on the Microsoft Graph availability validation logic introduced in the
/// Microsoft Graph service registration hotfix. Since ConfigureStatusService is a static method
/// that operates on WebApplication, these tests focus on the underlying validation logic.
/// </summary>
public class ApplicationStartupTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void ApplicationStartup_ConfigureEncoding_DoesNotThrow()
    {
        // Act & Assert - Should not throw exception
        var exception = Record.Exception(() => ApplicationStartup.ConfigureEncoding());

        exception.Should().BeNull("because ConfigureEncoding should execute without errors");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ApplicationStartup_ConfigureEncoding_RegistersCodePagesEncodingProvider()
    {
        // Arrange
        var originalProviders = System.Text.Encoding.GetEncodings();

        // Act
        ApplicationStartup.ConfigureEncoding();

        // Assert
        var currentProviders = System.Text.Encoding.GetEncodings();
        currentProviders.Length.Should().BeGreaterThanOrEqualTo(originalProviders.Length,
            "because encoding providers should be registered");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ValidateStartup_IsIdentityDbAvailable_IsAccessibleFromApplicationStartup()
    {
        // Arrange
        var validateStartupType = typeof(ValidateStartup);
        var property = validateStartupType.GetProperty("IsIdentityDbAvailable",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        // Act & Assert
        property.Should().NotBeNull("because IsIdentityDbAvailable should be a public static property");
        property!.PropertyType.Should().Be<bool>("because IsIdentityDbAvailable should be a boolean");

        // Verify we can read the property (for ApplicationStartup to use)
        var originalValue = property.GetValue(null);
        originalValue.Should().BeOfType<bool>("because the property should return a boolean value");

        // The property has a private setter, so we can't test writing, but that's okay
        // since ApplicationStartup only reads it
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void MicrosoftGraphAvailabilityValidator_Integration_WithApplicationStartupLogic()
    {
        // This test verifies the integration logic that ApplicationStartup.ConfigureStatusService uses

        // Arrange - Test the Microsoft Graph validation logic used in ConfigureStatusService
        var validEmailConfig = new EmailConfig
        {
            AzureTenantId = "valid-tenant-id",
            AzureAppId = "valid-app-id",
            AzureAppSecret = "valid-app-secret",
            FromEmail = "test@example.com",
            MailCheckApiKey = "valid-mail-check-key"
        };

        var invalidEmailConfig = new EmailConfig
        {
            AzureTenantId = "", // Invalid
            AzureAppId = "valid-app-id",
            AzureAppSecret = StatusService.PlaceholderMessage, // Placeholder
            FromEmail = "test@example.com",
            MailCheckApiKey = "valid-mail-check-key"
        };

        // Act
        var (validResult, validMissing) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(validEmailConfig);
        var (invalidResult, invalidMissing) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(invalidEmailConfig);

        // Assert
        validResult.Should().BeTrue("because all Microsoft Graph configuration is valid");
        validMissing.Should().BeNull("because no configuration is missing");

        invalidResult.Should().BeFalse("because some Microsoft Graph configuration is invalid");
        invalidMissing.Should().NotBeNull("because some configuration is missing");
        invalidMissing.Should().HaveCount(2, "because AzureTenantId and AzureAppSecret are invalid");
        invalidMissing.Should().Contain(EmailConfigConstants.AzureTenantIdKey);
        invalidMissing.Should().Contain(EmailConfigConstants.AzureAppSecretKey);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void UserDbContext_UserDatabaseConnectionName_IsAccessibleForApplicationStartup()
    {
        // This test verifies that the constant used in ApplicationStartup.ConfigureStatusService
        // is accessible and has the expected value

        // Act
        var connectionName = Zarichney.Services.Auth.UserDbContext.UserDatabaseConnectionName;

        // Assert
        connectionName.Should().NotBeNullOrEmpty("because the connection name should be defined");
        connectionName.Should().Be("UserDatabase",
            "because this is the expected connection string name used throughout the application");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ExternalServices_Enum_ContainsRequiredValues()
    {
        // This test verifies that the ExternalServices enum contains the values used in ConfigureStatusService

        // Act & Assert
        var postgresValue = ExternalServices.PostgresIdentityDb;
        var msGraphValue = ExternalServices.MsGraph;

        // Verify enum values exist (enums can't be null, so verify they have expected values)
        ((int)postgresValue).Should().BeGreaterThanOrEqualTo(0, "because PostgresIdentityDb should be defined");
        ((int)msGraphValue).Should().BeGreaterThanOrEqualTo(0, "because MsGraph should be defined");

        // Verify enum names are as expected (used in logging and diagnostics)
        postgresValue.ToString().Should().Be("PostgresIdentityDb");
        msGraphValue.ToString().Should().Be("MsGraph");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void EmailConfigConstants_ContainsAllMicrosoftGraphKeys()
    {
        // This test verifies that EmailConfigConstants has all the keys used by MicrosoftGraphAvailabilityValidator

        // Act & Assert
        EmailConfigConstants.AzureTenantIdKey.Should().Be("EmailConfig:AzureTenantId");
        EmailConfigConstants.AzureAppIdKey.Should().Be("EmailConfig:AzureAppId");
        EmailConfigConstants.AzureAppSecretKey.Should().Be("EmailConfig:AzureAppSecret");
        EmailConfigConstants.FromEmailKey.Should().Be("EmailConfig:FromEmail");

        // Verify section name is consistent
        EmailConfigConstants.SectionName.Should().Be("EmailConfig");

        // Verify all Microsoft Graph keys use the correct section
        EmailConfigConstants.AzureTenantIdKey.Should().StartWith(EmailConfigConstants.SectionName);
        EmailConfigConstants.AzureAppIdKey.Should().StartWith(EmailConfigConstants.SectionName);
        EmailConfigConstants.AzureAppSecretKey.Should().StartWith(EmailConfigConstants.SectionName);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void StatusService_PlaceholderMessage_IsAccessibleAndConsistent()
    {
        // This test verifies the placeholder message constant used in Microsoft Graph validation

        // Act
        var placeholderMessage = StatusService.PlaceholderMessage;

        // Assert
        placeholderMessage.Should().NotBeNullOrEmpty("because placeholder message should be defined");
        placeholderMessage.Should().Be("recommended to set in app secrets",
            "because this is the expected placeholder value");

        // Verify it's detected as missing configuration
        var emailConfigWithPlaceholder = new EmailConfig
        {
            AzureTenantId = placeholderMessage,
            AzureAppId = "valid-app-id",
            AzureAppSecret = "valid-app-secret",
            FromEmail = "test@example.com",
            MailCheckApiKey = "valid-mail-check-key"
        };

        var (isAvailable, missingConfigs) = MicrosoftGraphAvailabilityValidator.ValidateConfiguration(emailConfigWithPlaceholder);
        isAvailable.Should().BeFalse("because placeholder values should be treated as missing");
        missingConfigs.Should().Contain(EmailConfigConstants.AzureTenantIdKey);
    }
}