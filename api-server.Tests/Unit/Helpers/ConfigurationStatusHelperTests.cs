using FluentAssertions;
using Xunit;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Helpers;

namespace Zarichney.Tests.Unit.Helpers;

/// <summary>
/// Unit tests for the ConfigurationStatusHelper class.
/// </summary>
[Trait("Category", "Unit")]
public class ConfigurationStatusHelperTests
{
  [Fact]
  public void IsConfigurationAvailable_WhenItemIsConfigured_ReturnsTrue()
  {
    // Arrange
    var configName = "Database Connection";
    var statuses = new List<ConfigurationItemStatus>
        {
            new("OpenAI API Key", "Missing/Invalid", "ApiKey is missing or placeholder"),
            new(configName, "Configured"),
            new("Stripe Secret Key", "Missing/Invalid", "StripeSecretKey is missing or placeholder")
        };

    // Act
    var result = ConfigurationStatusHelper.IsConfigurationAvailable(statuses, configName);

    // Assert
    result.Should().BeTrue(
        $"Because {configName} is in the list with Status='Configured'");
  }

  [Fact]
  public void IsConfigurationAvailable_WhenItemHasMissingInvalidStatus_ReturnsFalse()
  {
    // Arrange
    var configName = "OpenAI API Key";
    var statuses = new List<ConfigurationItemStatus>
        {
            new(configName, "Missing/Invalid", "ApiKey is missing or placeholder"),
            new("Database Connection", "Configured"),
            new("Stripe Secret Key", "Missing/Invalid", "StripeSecretKey is missing or placeholder")
        };

    // Act
    var result = ConfigurationStatusHelper.IsConfigurationAvailable(statuses, configName);

    // Assert
    result.Should().BeFalse(
        $"Because {configName} is in the list but Status='Missing/Invalid'");
  }

  [Fact]
  public void IsConfigurationAvailable_WhenItemDoesNotExist_ReturnsFalse()
  {
    // Arrange
    var configName = "Nonexistent Configuration";
    var statuses = new List<ConfigurationItemStatus>
        {
            new("OpenAI API Key", "Missing/Invalid", "ApiKey is missing or placeholder"),
            new("Database Connection", "Configured"),
            new("Stripe Secret Key", "Missing/Invalid", "StripeSecretKey is missing or placeholder")
        };

    // Act
    var result = ConfigurationStatusHelper.IsConfigurationAvailable(statuses, configName);

    // Assert
    result.Should().BeFalse(
        $"Because {configName} is not in the list");
  }

  [Fact]
  public void IsConfigurationAvailable_WhenStatusListIsEmpty_ReturnsFalse()
  {
    // Arrange
    var configName = "Database Connection";
    var statuses = new List<ConfigurationItemStatus>();

    // Act
    var result = ConfigurationStatusHelper.IsConfigurationAvailable(statuses, configName);

    // Assert
    result.Should().BeFalse(
        "Because the status list is empty");
  }

  [Fact]
  public void IsConfigurationAvailable_WhenStatusListIsNull_ThrowsArgumentNullException()
  {
    // Arrange
    List<ConfigurationItemStatus>? statuses = null;
    var configName = "Database Connection";

    // Act & Assert
    var action = () => ConfigurationStatusHelper.IsConfigurationAvailable(statuses!, configName);

    action.Should().Throw<ArgumentNullException>()
        .WithParameterName("statuses");
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void IsConfigurationAvailable_WhenConfigNameIsNullOrWhitespace_ThrowsArgumentException(string? configName)
  {
    // Arrange
    var statuses = new List<ConfigurationItemStatus>
        {
            new("Database Connection", "Configured")
        };

    // Act & Assert
    var action = () => ConfigurationStatusHelper.IsConfigurationAvailable(statuses, configName!);

    action.Should().Throw<ArgumentException>()
        .WithParameterName("configName");
  }

  [Fact]
  public void IsConfigurationAvailable_WhenConfigNameDiffersByCaseOnly_ReturnsFalse()
  {
    // Arrange
    var statuses = new List<ConfigurationItemStatus>
        {
            new("Database Connection", "Configured")
        };

    // Act
    var result = ConfigurationStatusHelper.IsConfigurationAvailable(statuses, "database connection");

    // Assert
    result.Should().BeFalse(
        "Because the configName comparison is case-sensitive");
  }

  [Fact]
  public void IsConfigurationAvailable_WhenStatusDiffersByCaseOnly_ReturnsFalse()
  {
    // Arrange
    var configName = "Database Connection";
    var statuses = new List<ConfigurationItemStatus>
        {
            new(configName, "configured") // Lowercase "configured"
        };

    // Act
    var result = ConfigurationStatusHelper.IsConfigurationAvailable(statuses, configName);

    // Assert
    result.Should().BeFalse(
        "Because the status comparison is case-sensitive and should be exactly 'Configured'");
  }
}
