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
        serviceName: ExternalServices.OpenAiApi,
        IsAvailable: true,
        MissingConfigurations: []);

    // Assert
    statusInfo.serviceName.Should().Be(ExternalServices.OpenAiApi);
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
        serviceName: ExternalServices.MsGraph,
        IsAvailable: false,
        MissingConfigurations: missingConfigs);

    // Assert
    statusInfo.serviceName.Should().Be(ExternalServices.MsGraph);
    statusInfo.IsAvailable.Should().BeFalse();
    statusInfo.MissingConfigurations.Should().BeEquivalentTo(missingConfigs);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void Record_EqualityBehavior_WorksAsExpected()
  {
    // Arrange
    var missingConfigs = new List<string> { "Config1", "Config2" };
    var statusInfo1 = new ServiceStatusInfo(ExternalServices.Stripe, false, missingConfigs);
    var statusInfo2 = new ServiceStatusInfo(ExternalServices.Stripe, false, missingConfigs);
    var statusInfo3 = new ServiceStatusInfo(ExternalServices.FrontEnd, true, []);
    var statusInfo4 = new ServiceStatusInfo(ExternalServices.GitHubAccess, false, missingConfigs);

    // Act & Assert
    statusInfo1.Should().Be(statusInfo2);
    statusInfo1.Should().NotBe(statusInfo3);
    statusInfo1.Should().NotBe(statusInfo4); // Different service names should not be equal
  }
}
