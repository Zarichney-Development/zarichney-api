using Zarichney.Services.Status;
using FluentAssertions;
using Xunit;

namespace Zarichney.Tests.Unit.Services.Status;

public class ServiceStatusInfoTests
{
  [Trait("Category", "Unit")]
  [Fact]
  public void Constructor_WithIsAvailableTrue_SetsPropertiesCorrectly()
  {
    // Arrange & Act
    var statusInfo = new ServiceStatusInfo(
        IsAvailable: true,
        MissingConfigurations: new List<string>());

    // Assert
    statusInfo.IsAvailable.Should().BeTrue();
    statusInfo.MissingConfigurations.Should().BeEmpty();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void Constructor_WithIsAvailableFalseAndMissingConfigs_SetsPropertiesCorrectly()
  {
    // Arrange
    var missingConfigs = new List<string> { "Config1", "Config2" };

    // Act
    var statusInfo = new ServiceStatusInfo(
        IsAvailable: false,
        MissingConfigurations: missingConfigs);

    // Assert
    statusInfo.IsAvailable.Should().BeFalse();
    statusInfo.MissingConfigurations.Should().BeEquivalentTo(missingConfigs);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void Record_EqualityBehavior_WorksAsExpected()
  {
    // Arrange
    var missingConfigs = new List<string> { "Config1", "Config2" };
    var statusInfo1 = new ServiceStatusInfo(false, missingConfigs);
    var statusInfo2 = new ServiceStatusInfo(false, missingConfigs);
    var statusInfo3 = new ServiceStatusInfo(true, new List<string>());

    // Act & Assert
    statusInfo1.Should().Be(statusInfo2);
    statusInfo1.Should().NotBe(statusInfo3);
  }
}
