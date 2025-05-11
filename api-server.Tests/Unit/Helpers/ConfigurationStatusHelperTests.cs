using FluentAssertions;
using Xunit;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
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

  #region Service Status Tests

  [Fact]
  public void IsFeatureDependencyAvailable_WhenServiceStatusesNull_ThrowsArgumentNullException()
  {
    // Arrange
    Dictionary<string, ServiceStatusInfo>? serviceStatuses = null;
    var dependencyTraitValue = TestCategories.ExternalOpenAI;

    // Act & Assert
    var action = () => ConfigurationStatusHelper.IsFeatureDependencyAvailable(serviceStatuses!, dependencyTraitValue);

    action.Should().Throw<ArgumentNullException>()
        .WithParameterName("serviceStatuses");
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void IsFeatureDependencyAvailable_WhenDependencyTraitValueNullOrWhitespace_ThrowsArgumentException(string? dependencyTraitValue)
  {
    // Arrange
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>();

    // Act & Assert
    var action = () => ConfigurationStatusHelper.IsFeatureDependencyAvailable(serviceStatuses, dependencyTraitValue!);

    action.Should().Throw<ArgumentException>()
        .WithParameterName("dependencyTraitValue");
  }

  [Fact]
  public void IsFeatureDependencyAvailable_WhenMappedFeaturesAvailable_ReturnsTrue()
  {
    // Arrange
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>
    {
      ["Llm"] = new ServiceStatusInfo(true, new List<string>())
    };

    // Act
    var result = ConfigurationStatusHelper.IsFeatureDependencyAvailable(serviceStatuses, TestCategories.ExternalOpenAI);

    // Assert
    result.Should().BeTrue(
        "Because the Llm feature is available and is mapped to ExternalOpenAI");
  }

  [Fact]
  public void IsFeatureDependencyAvailable_WhenMappedFeaturesUnavailable_ReturnsFalse()
  {
    // Arrange
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>
    {
      ["Llm"] = new ServiceStatusInfo(false, new List<string> { "Llm:ApiKey" })
    };

    // Act
    var result = ConfigurationStatusHelper.IsFeatureDependencyAvailable(serviceStatuses, TestCategories.ExternalOpenAI);

    // Assert
    result.Should().BeFalse(
        "Because the Llm feature is unavailable and is mapped to ExternalOpenAI");
  }

  [Fact]
  public void IsFeatureDependencyAvailable_WhenMappedFeaturesMissing_ReturnsFalse()
  {
    // Arrange
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>
    {
      ["Email"] = new ServiceStatusInfo(true, new List<string>())
      // Llm is missing from the dictionary
    };

    // Act
    var result = ConfigurationStatusHelper.IsFeatureDependencyAvailable(serviceStatuses, TestCategories.ExternalOpenAI);

    // Assert
    result.Should().BeFalse(
        "Because the Llm feature is missing from the status dictionary");
  }

  [Fact]
  public void IsFeatureDependencyAvailable_WhenDependencyNotMapped_ReturnsTrue()
  {
    // Arrange
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>();
    var unmappedDependency = "UnmappedDependency";

    // Act
    var result = ConfigurationStatusHelper.IsFeatureDependencyAvailable(serviceStatuses, unmappedDependency);

    // Assert
    result.Should().BeTrue(
        "Because unmapped dependencies are considered available by default");
  }

  [Fact]
  public void GetMissingConfigurationsForDependency_WhenServiceStatusesNull_ThrowsArgumentNullException()
  {
    // Arrange
    Dictionary<string, ServiceStatusInfo>? serviceStatuses = null;
    var dependencyTraitValue = TestCategories.ExternalOpenAI;

    // Act & Assert
    var action = () => ConfigurationStatusHelper.GetMissingConfigurationsForDependency(serviceStatuses!, dependencyTraitValue);

    action.Should().Throw<ArgumentNullException>()
        .WithParameterName("serviceStatuses");
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void GetMissingConfigurationsForDependency_WhenDependencyTraitValueNullOrWhitespace_ThrowsArgumentException(string? dependencyTraitValue)
  {
    // Arrange
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>();

    // Act & Assert
    var action = () => ConfigurationStatusHelper.GetMissingConfigurationsForDependency(serviceStatuses, dependencyTraitValue!);

    action.Should().Throw<ArgumentException>()
        .WithParameterName("dependencyTraitValue");
  }

  [Fact]
  public void GetMissingConfigurationsForDependency_WhenMappedFeaturesAvailable_ReturnsEmptyList()
  {
    // Arrange
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>
    {
      ["Llm"] = new ServiceStatusInfo(true, new List<string>())
    };

    // Act
    var result = ConfigurationStatusHelper.GetMissingConfigurationsForDependency(serviceStatuses, TestCategories.ExternalOpenAI);

    // Assert
    result.Should().BeEmpty(
        "Because the Llm feature is available and has no missing configurations");
  }

  [Fact]
  public void GetMissingConfigurationsForDependency_WhenMappedFeaturesUnavailable_ReturnsMissingConfigurations()
  {
    // Arrange
    var missingConfigs = new List<string> { "Llm:ApiKey" };
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>
    {
      ["Llm"] = new ServiceStatusInfo(false, missingConfigs)
    };

    // Act
    var result = ConfigurationStatusHelper.GetMissingConfigurationsForDependency(serviceStatuses, TestCategories.ExternalOpenAI);

    // Assert
    result.Should().BeEquivalentTo(missingConfigs,
        "Because the Llm feature is unavailable and its missing configurations should be returned");
  }

  [Fact]
  public void GetMissingConfigurationsForDependency_WhenMultipleFeaturesUnavailable_ReturnsAllMissingConfigurations()
  {
    // Arrange
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>
    {
      ["Payment"] = new ServiceStatusInfo(false, new List<string> { "Payment:ApiKey" }),
      ["Stripe"] = new ServiceStatusInfo(false, new List<string> { "Stripe:Secret" })
    };

    // Act
    var result = ConfigurationStatusHelper.GetMissingConfigurationsForDependency(serviceStatuses, TestCategories.ExternalStripe);

    // Assert
    result.Should().BeEquivalentTo(new[] { "Payment:ApiKey", "Stripe:Secret" },
        "Because both mapped features are unavailable and all missing configurations should be returned");
  }

  [Fact]
  public void GetMissingConfigurationsForDependency_WhenMappedFeaturesMissing_ReturnsEmptyList()
  {
    // Arrange
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>
    {
      ["Email"] = new ServiceStatusInfo(true, new List<string>())
      // Llm is missing from the dictionary
    };

    // Act
    var result = ConfigurationStatusHelper.GetMissingConfigurationsForDependency(serviceStatuses, TestCategories.ExternalOpenAI);

    // Assert
    result.Should().BeEmpty(
        "Because the Llm feature is missing from the status dictionary, so there are no known missing configurations");
  }

  [Fact]
  public void GetMissingConfigurationsForDependency_WhenDependencyNotMapped_ReturnsEmptyList()
  {
    // Arrange
    var serviceStatuses = new Dictionary<string, ServiceStatusInfo>();
    var unmappedDependency = "UnmappedDependency";

    // Act
    var result = ConfigurationStatusHelper.GetMissingConfigurationsForDependency(serviceStatuses, unmappedDependency);

    // Assert
    result.Should().BeEmpty(
        "Because unmapped dependencies have no known required configurations");
  }

  #endregion
}
