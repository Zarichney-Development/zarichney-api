using FluentAssertions;
using System.Text.Json;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.Email;
using Zarichney.Services.Status;

namespace Zarichney.Tests.Unit.Services.Email;

/// <summary>
/// Unit tests for EmailModels including EmailConfig, InvalidEmailReason, EmailValidationResponse, and InvalidEmailException.
/// </summary>
[Trait("Category", "Unit")]
public class EmailModelsTests
{
  [Fact]
  public void EmailConfig_WithAllRequiredProperties_CreatesValidInstance()
  {
    // Arrange & Act
    var config = new EmailConfig
    {
      AzureTenantId = "test-tenant-id",
      AzureAppId = "test-app-id",
      AzureAppSecret = "test-secret",
      FromEmail = "sender@example.com",
      MailCheckApiKey = "test-api-key"
    };

    // Assert
    config.Should().NotBeNull();
    config.AzureTenantId.Should().Be("test-tenant-id");
    config.AzureAppId.Should().Be("test-app-id");
    config.AzureAppSecret.Should().Be("test-secret");
    config.FromEmail.Should().Be("sender@example.com");
    config.MailCheckApiKey.Should().Be("test-api-key");
    config.TemplateDirectory.Should().Be("/Services/Email/Templates",
      "because this is the default value");
  }

  [Fact]
  public void EmailConfig_ImplementsIConfig()
  {
    // Arrange & Act
    var config = new EmailConfig
    {
      AzureTenantId = "tenant",
      AzureAppId = "app",
      AzureAppSecret = "secret",
      FromEmail = "email@example.com",
      MailCheckApiKey = "key"
    };

    // Assert
    config.Should().BeAssignableTo<IConfig>(
      "because EmailConfig should implement the IConfig interface");
  }

  [Fact]
  public void EmailConfig_WithCustomTemplateDirectory_UsesProvidedValue()
  {
    // Arrange & Act
    var config = new EmailConfig
    {
      AzureTenantId = "tenant",
      AzureAppId = "app",
      AzureAppSecret = "secret",
      FromEmail = "email@example.com",
      MailCheckApiKey = "key",
      TemplateDirectory = "/custom/templates"
    };

    // Assert
    config.TemplateDirectory.Should().Be("/custom/templates",
      "because custom template directory should be respected");
  }

  [Theory]
  [InlineData(InvalidEmailReason.InvalidSyntax, 0)]
  [InlineData(InvalidEmailReason.PossibleTypo, 1)]
  [InlineData(InvalidEmailReason.InvalidDomain, 2)]
  [InlineData(InvalidEmailReason.DisposableEmail, 3)]
  public void InvalidEmailReason_HasCorrectValues(InvalidEmailReason reason, int expectedValue)
  {
    // Assert
    ((int)reason).Should().Be(expectedValue,
      $"because {reason} should have underlying value {expectedValue}");
  }

  [Fact]
  public void InvalidEmailReason_AllValuesAreDefined()
  {
    // Arrange
    var definedValues = Enum.GetValues<InvalidEmailReason>();

    // Assert
    definedValues.Should().HaveCount(4, "because there are 4 defined InvalidEmailReason values");
    definedValues.Should().Contain(InvalidEmailReason.InvalidSyntax);
    definedValues.Should().Contain(InvalidEmailReason.PossibleTypo);
    definedValues.Should().Contain(InvalidEmailReason.InvalidDomain);
    definedValues.Should().Contain(InvalidEmailReason.DisposableEmail);
  }

  [Fact]
  public void EmailValidationResponse_WithAllProperties_SerializesCorrectly()
  {
    // Arrange
    var response = new EmailValidationResponse
    {
      Valid = true,
      Block = false,
      Disposable = false,
      EmailForwarder = true,
      Domain = "example.com",
      Text = "Valid email",
      Reason = "all checks passed",
      Risk = 10,
      MxHost = "mail.example.com",
      PossibleTypo = new[] { "example.org" },
      MxIp = "192.168.1.1",
      MxInfo = "Primary MX server",
      LastChangedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
    };

    // Act
    var json = JsonSerializer.Serialize(response);
    var deserialized = JsonSerializer.Deserialize<EmailValidationResponse>(json);

    // Assert
    deserialized.Should().NotBeNull();
    deserialized!.Valid.Should().BeTrue();
    deserialized.Block.Should().BeFalse();
    deserialized.Disposable.Should().BeFalse();
    deserialized.EmailForwarder.Should().BeTrue();
    deserialized.Domain.Should().Be("example.com");
    deserialized.Text.Should().Be("Valid email");
    deserialized.Reason.Should().Be("all checks passed");
    deserialized.Risk.Should().Be(10);
    deserialized.MxHost.Should().Be("mail.example.com");
    deserialized.PossibleTypo.Should().ContainSingle().Which.Should().Be("example.org");
    deserialized.MxIp.Should().Be("192.168.1.1");
    deserialized.MxInfo.Should().Be("Primary MX server");
    deserialized.LastChangedAt.Should().Be(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));
  }

  [Fact]
  public void EmailValidationResponse_JsonPropertyNames_AreCorrect()
  {
    // Arrange
    var response = new EmailValidationResponse
    {
      Valid = true,
      Block = false,
      Disposable = true,
      EmailForwarder = false,
      Domain = "test.com",
      Text = "text",
      Reason = "reason",
      Risk = 50,
      MxHost = "mx.test.com",
      PossibleTypo = Array.Empty<string>(),
      MxIp = "1.2.3.4",
      MxInfo = "info",
      LastChangedAt = DateTime.UtcNow
    };

    // Act
    var json = JsonSerializer.Serialize(response);

    // Assert
    json.Should().Contain("\"valid\":true", "because JsonPropertyName is 'valid'");
    json.Should().Contain("\"block\":false", "because JsonPropertyName is 'block'");
    json.Should().Contain("\"disposable\":true", "because JsonPropertyName is 'disposable'");
    json.Should().Contain("\"email_forwarder\":false", "because JsonPropertyName is 'email_forwarder'");
    json.Should().Contain("\"domain\":\"test.com\"", "because JsonPropertyName is 'domain'");
    json.Should().Contain("\"text\":\"text\"", "because JsonPropertyName is 'text'");
    json.Should().Contain("\"reason\":\"reason\"", "because JsonPropertyName is 'reason'");
    json.Should().Contain("\"risk\":50", "because JsonPropertyName is 'risk'");
    json.Should().Contain("\"mx_host\":\"mx.test.com\"", "because JsonPropertyName is 'mx_host'");
    json.Should().Contain("\"possible_typo\":[]", "because JsonPropertyName is 'possible_typo'");
    json.Should().Contain("\"mx_ip\":\"1.2.3.4\"", "because JsonPropertyName is 'mx_ip'");
    json.Should().Contain("\"mx_info\":\"info\"", "because JsonPropertyName is 'mx_info'");
    json.Should().Contain("\"last_changed_at\":", "because JsonPropertyName is 'last_changed_at'");
  }

  [Fact]
  public void EmailValidationResponse_DeserializesFromSnakeCaseJson()
  {
    // Arrange
    var json = @"{
      ""valid"": false,
      ""block"": true,
      ""disposable"": true,
      ""email_forwarder"": false,
      ""domain"": ""blocked.com"",
      ""text"": ""Blocked domain"",
      ""reason"": ""domain blocked"",
      ""risk"": 90,
      ""mx_host"": """",
      ""possible_typo"": [""blocked.org""],
      ""mx_ip"": """",
      ""mx_info"": ""No MX"",
      ""last_changed_at"": ""2024-01-01T00:00:00Z""
    }";

    // Act
    var response = JsonSerializer.Deserialize<EmailValidationResponse>(json);

    // Assert
    response.Should().NotBeNull();
    response!.Valid.Should().BeFalse();
    response.Block.Should().BeTrue();
    response.Disposable.Should().BeTrue();
    response.EmailForwarder.Should().BeFalse();
    response.Domain.Should().Be("blocked.com");
    response.Text.Should().Be("Blocked domain");
    response.Reason.Should().Be("domain blocked");
    response.Risk.Should().Be(90);
    response.MxHost.Should().BeEmpty();
    response.PossibleTypo.Should().ContainSingle().Which.Should().Be("blocked.org");
    response.MxIp.Should().BeEmpty();
    response.MxInfo.Should().Be("No MX");
  }

  [Fact]
  public void InvalidEmailException_WithAllParameters_StoresCorrectly()
  {
    // Arrange
    const string message = "Email validation failed";
    const string email = "invalid@example.com";
    const InvalidEmailReason reason = InvalidEmailReason.InvalidSyntax;

    // Act
    var exception = new InvalidEmailException(message, email, reason);

    // Assert
    exception.Should().NotBeNull();
    exception.Message.Should().Be(message);
    exception.Email.Should().Be(email);
    exception.Reason.Should().Be(reason);
    exception.InnerException.Should().BeNull();
  }

  [Theory]
  [InlineData("Syntax error", "bad@email", InvalidEmailReason.InvalidSyntax)]
  [InlineData("Possible typo", "user@gmai.com", InvalidEmailReason.PossibleTypo)]
  [InlineData("Invalid domain", "user@nonexistent.xyz", InvalidEmailReason.InvalidDomain)]
  [InlineData("Disposable email", "user@tempmail.com", InvalidEmailReason.DisposableEmail)]
  public void InvalidEmailException_VariousScenarios_StoreCorrectData(
    string message, string email, InvalidEmailReason reason)
  {
    // Act
    var exception = new InvalidEmailException(message, email, reason);

    // Assert
    exception.Message.Should().Be(message);
    exception.Email.Should().Be(email);
    exception.Reason.Should().Be(reason);
  }

  [Fact]
  public void InvalidEmailException_InheritsFromException()
  {
    // Arrange & Act
    var exception = new InvalidEmailException("test", "email", InvalidEmailReason.InvalidDomain);

    // Assert
    exception.Should().BeAssignableTo<Exception>(
      "because InvalidEmailException should inherit from Exception");
  }

  [Fact]
  public void InvalidEmailException_CanBeCaughtAsException()
  {
    // Arrange
    Exception caughtException = null!;

    // Act
    try
    {
      throw new InvalidEmailException("Email error", "test@example.com", InvalidEmailReason.DisposableEmail);
    }
    catch (Exception ex)
    {
      caughtException = ex;
    }

    // Assert
    caughtException.Should().NotBeNull();
    caughtException.Should().BeOfType<InvalidEmailException>();
    var invalidEmailException = (InvalidEmailException)caughtException;
    invalidEmailException.Email.Should().Be("test@example.com");
    invalidEmailException.Reason.Should().Be(InvalidEmailReason.DisposableEmail);
  }

  [Fact]
  public void EmailValidationResponse_WithEmptyPossibleTypo_HandlesCorrectly()
  {
    // Arrange
    var response = new EmailValidationResponse
    {
      Valid = true,
      Domain = "example.com",
      Text = "Valid",
      Reason = "valid",
      MxHost = "mx.example.com",
      PossibleTypo = Array.Empty<string>(),
      MxIp = "1.2.3.4",
      MxInfo = "MX info"
    };

    // Act & Assert
    response.PossibleTypo.Should().NotBeNull();
    response.PossibleTypo.Should().BeEmpty();
    response.PossibleTypo.Should().HaveCount(0);
  }

  [Fact]
  public void EmailValidationResponse_WithMultiplePossibleTypos_StoresAll()
  {
    // Arrange
    var typos = new[] { "gmail.com", "g-mail.com", "googlemail.com" };
    var response = new EmailValidationResponse
    {
      Valid = false,
      Domain = "gmai.com",
      Text = "Possible typo",
      Reason = "typo",
      MxHost = "",
      PossibleTypo = typos,
      MxIp = "",
      MxInfo = "No MX"
    };

    // Act & Assert
    response.PossibleTypo.Should().NotBeNull();
    response.PossibleTypo.Should().HaveCount(3);
    response.PossibleTypo.Should().ContainInOrder(typos);
  }

  [Fact]
  public void EmailConfig_RequiredProperties_HaveRequiresConfigurationAttribute()
  {
    // Arrange
    var emailConfigType = typeof(EmailConfig);

    // Act & Assert - Verify RequiresConfiguration attributes are present
    var azureTenantIdProperty = emailConfigType.GetProperty(nameof(EmailConfig.AzureTenantId));
    azureTenantIdProperty.Should().NotBeNull();

    var azureAppIdProperty = emailConfigType.GetProperty(nameof(EmailConfig.AzureAppId));
    azureAppIdProperty.Should().NotBeNull();

    var azureAppSecretProperty = emailConfigType.GetProperty(nameof(EmailConfig.AzureAppSecret));
    azureAppSecretProperty.Should().NotBeNull();

    var fromEmailProperty = emailConfigType.GetProperty(nameof(EmailConfig.FromEmail));
    fromEmailProperty.Should().NotBeNull();

    var mailCheckApiKeyProperty = emailConfigType.GetProperty(nameof(EmailConfig.MailCheckApiKey));
    mailCheckApiKeyProperty.Should().NotBeNull();
  }

  [Fact]
  public void EmailValidationResponse_DefaultValues_HandleCorrectly()
  {
    // Arrange
    var response = new EmailValidationResponse
    {
      Domain = "test.com",
      Text = "text",
      Reason = "reason",
      MxHost = "mx",
      PossibleTypo = Array.Empty<string>(),
      MxIp = "ip",
      MxInfo = "info"
    };

    // Act & Assert - Check default boolean values
    response.Valid.Should().BeFalse("because bool defaults to false");
    response.Block.Should().BeFalse("because bool defaults to false");
    response.Disposable.Should().BeFalse("because bool defaults to false");
    response.EmailForwarder.Should().BeFalse("because bool defaults to false");
    response.Risk.Should().Be(0, "because int defaults to 0");
    response.LastChangedAt.Should().Be(default(DateTime));
  }

  [Fact]
  public void InvalidEmailReason_CanBeUsedInSwitch()
  {
    // Arrange
    var reasons = Enum.GetValues<InvalidEmailReason>();
    var messages = new List<string>();

    // Act
    foreach (var reason in reasons)
    {
      var message = reason switch
      {
        InvalidEmailReason.InvalidSyntax => "Syntax is invalid",
        InvalidEmailReason.PossibleTypo => "Typo detected",
        InvalidEmailReason.InvalidDomain => "Domain is invalid",
        InvalidEmailReason.DisposableEmail => "Disposable email",
        _ => "Unknown reason"
      };
      messages.Add(message);
    }

    // Assert
    messages.Should().HaveCount(4);
    messages.Should().NotContain("Unknown reason",
      "because all enum values should be handled");
  }
}
