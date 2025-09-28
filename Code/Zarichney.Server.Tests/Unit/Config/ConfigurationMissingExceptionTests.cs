using FluentAssertions;
using Xunit;
using Zarichney.Config;

namespace Zarichney.Tests.Unit.Config;

public class ConfigurationMissingExceptionTests
{
  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_WithMessage_SetsMessageCorrectly()
  {
    // Arrange
    const string expectedMessage = "Configuration is missing";

    // Act
    var exception = new ConfigurationMissingException(expectedMessage);

    // Assert
    exception.Message.Should().Be(expectedMessage,
        "because the message should be set from the constructor");
    exception.ConfigurationSection.Should().BeEmpty(
        "because no configuration section was specified");
    exception.MissingKeyDetails.Should().BeEmpty(
        "because no missing key details were specified");
    exception.InnerException.Should().BeNull(
        "because no inner exception was provided");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_WithMessageAndInnerException_SetsBothCorrectly()
  {
    // Arrange
    const string expectedMessage = "Configuration error occurred";
    var innerException = new InvalidOperationException("Inner error");

    // Act
    var exception = new ConfigurationMissingException(expectedMessage, innerException);

    // Assert
    exception.Message.Should().Be(expectedMessage,
        "because the message should be set from the constructor");
    exception.InnerException.Should().BeSameAs(innerException,
        "because the inner exception should be preserved");
    exception.ConfigurationSection.Should().BeEmpty(
        "because no configuration section was specified");
    exception.MissingKeyDetails.Should().BeEmpty(
        "because no missing key details were specified");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_WithSectionAndDetails_GeneratesCorrectMessage()
  {
    // Arrange
    const string section = "EmailService";
    const string details = "ApiKey is required";

    // Act
    var exception = new ConfigurationMissingException(section, details);

    // Assert
    exception.Message.Should().Contain(section,
        "because the message should reference the configuration section");
    exception.Message.Should().Contain(details,
        "because the message should include the missing key details");
    exception.Message.Should().Contain("cannot operate",
        "because the message should indicate service impact");
    exception.ConfigurationSection.Should().Be(section,
        "because the section should be stored");
    exception.MissingKeyDetails.Should().Be(details,
        "because the details should be stored");
    exception.InnerException.Should().BeNull(
        "because no inner exception was provided");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_WithSectionDetailsAndInnerException_SetsAllPropertiesCorrectly()
  {
    // Arrange
    const string section = "PaymentGateway";
    const string details = "SecretKey and MerchantId are missing";
    var innerException = new ArgumentNullException("config");

    // Act
    var exception = new ConfigurationMissingException(section, details, innerException);

    // Assert
    exception.ConfigurationSection.Should().Be(section,
        "because the section should be stored");
    exception.MissingKeyDetails.Should().Be(details,
        "because the details should be stored");
    exception.InnerException.Should().BeSameAs(innerException,
        "because the inner exception should be preserved");
    exception.Message.Should().Contain(section,
        "because the message should reference the configuration section");
    exception.Message.Should().Contain(details,
        "because the message should include the missing key details");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Exception_IsInvalidOperationException_ForProperInheritance()
  {
    // Arrange & Act
    var exception = new ConfigurationMissingException("test");

    // Assert
    exception.Should().BeAssignableTo<InvalidOperationException>(
        "because ConfigurationMissingException inherits from InvalidOperationException");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_WithEmptySection_StillGeneratesMessage()
  {
    // Arrange & Act
    var exception = new ConfigurationMissingException(string.Empty, "details");

    // Assert
    exception.Message.Should().NotBeNullOrEmpty(
        "because a message should be generated even with empty section");
    exception.Message.Should().Contain("details",
        "because the details should still be included");
    exception.ConfigurationSection.Should().BeEmpty(
        "because empty section was provided");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_WithEmptyDetails_StillGeneratesMessage()
  {
    // Arrange & Act
    var exception = new ConfigurationMissingException("Section", string.Empty);

    // Assert
    exception.Message.Should().NotBeNullOrEmpty(
        "because a message should be generated even with empty details");
    exception.Message.Should().Contain("Section",
        "because the section should still be included");
    exception.MissingKeyDetails.Should().BeEmpty(
        "because empty details were provided");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_WithNullValues_HandlesGracefully()
  {
    // Arrange & Act
    var exception = new ConfigurationMissingException((string)null!, (string)null!);

    // Assert
    exception.Should().NotBeNull(
        "because the exception should be created even with null values");
    exception.Message.Should().NotBeNullOrEmpty(
        "because a message should be generated");
    exception.ConfigurationSection.Should().BeNull(
        "because null was provided");
    exception.MissingKeyDetails.Should().BeNull(
        "because null was provided");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void GeneratedMessage_HasConsistentFormat()
  {
    // Arrange
    const string section = "TestSection";
    const string details = "TestKey";

    // Act
    var exception = new ConfigurationMissingException(section, details);

    // Assert
    var expectedMessage = $"Configuration is missing or invalid for '{section}'. " +
                        $"Required details: {details}. The service cannot operate.";
    exception.Message.Should().Be(expectedMessage,
        "because the message should follow the expected format");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Exception_CanBeSerializedForLogging()
  {
    // Arrange
    var exception = new ConfigurationMissingException("LoggingConfig", "LogLevel");

    // Act
    var toString = exception.ToString();

    // Assert
    toString.Should().Contain("ConfigurationMissingException",
        "because the exception type should be in the string representation");
    toString.Should().Contain("LoggingConfig",
        "because the configuration section should be included");
    toString.Should().Contain("LogLevel",
        "because the missing key details should be included");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Exception_PropertiesAreReadOnly()
  {
    // Arrange
    var exception = new ConfigurationMissingException("Section", "Details");

    // Act & Assert
    exception.ConfigurationSection.Should().Be("Section",
        "because properties should be immutable after construction");
    exception.MissingKeyDetails.Should().Be("Details",
        "because properties should be immutable after construction");

    // Properties are read-only, so no need to test mutation
    var properties = typeof(ConfigurationMissingException).GetProperties();
    properties.Should().OnlyContain(p => p.CanRead,
        "because all properties should be readable");
    properties.Where(p => p.DeclaringType == typeof(ConfigurationMissingException))
        .Should().OnlyContain(p => !p.CanWrite || (p.GetSetMethod() != null && p.GetSetMethod()!.IsPrivate),
        "because custom properties should not have public setters");
  }
}
