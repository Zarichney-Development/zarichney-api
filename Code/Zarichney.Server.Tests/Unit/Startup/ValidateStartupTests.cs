using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Startup;

namespace Zarichney.Server.Tests.Unit.Startup;

[Trait("Category", "Unit")]
public class ValidateStartupTests
{
  private readonly Mock<IWebHostEnvironment> _mockEnvironment;
  private readonly Mock<IConfiguration> _mockConfiguration;

  public ValidateStartupTests()
  {
    _mockEnvironment = new Mock<IWebHostEnvironment>();
    _mockConfiguration = new Mock<IConfiguration>();
  }

  [Fact]
  public void ValidateProductionConfiguration_InProduction_WhenConnectionStringMissing_ReturnsFalse()
  {
    // Arrange
    _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
    _mockConfiguration.Setup(c => c[$"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}"]).Returns((string?)null);

    // The test cannot directly verify Environment.Exit(1), so we check the return value.
    // In the actual code, it would never return false in Production due to Environment.Exit,
    // but the test can check the logical return value.

    // Act & Assert
    Assert.ThrowsAny<Exception>(() =>
    {
      ValidateStartup.ValidateProductionConfiguration(_mockEnvironment.Object, _mockConfiguration.Object);
    });

    // Also verify that IsIdentityDbAvailable is set to false before the exception
    ValidateStartup.IsIdentityDbAvailable.Should().BeFalse();
  }

  [Fact]
  public void ValidateProductionConfiguration_InProduction_WhenConnectionStringPresent_ReturnsTrue()
  {
    // Arrange
    _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
    _mockConfiguration.Setup(c => c[$"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}"]).Returns("valid-connection-string");

    // Act
    var result = ValidateStartup.ValidateProductionConfiguration(_mockEnvironment.Object, _mockConfiguration.Object);

    // Assert
    result.Should().BeTrue();
    ValidateStartup.IsIdentityDbAvailable.Should().BeTrue();
  }

  [Fact]
  public void ValidateProductionConfiguration_InDevelopment_WhenConnectionStringMissing_ReturnsFalse()
  {
    // Arrange
    _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
    _mockConfiguration.Setup(c => c[$"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}"]).Returns((string?)null);

    // Act
    var result = ValidateStartup.ValidateProductionConfiguration(_mockEnvironment.Object, _mockConfiguration.Object);

    // Assert
    result.Should().BeFalse();
    ValidateStartup.IsIdentityDbAvailable.Should().BeFalse();
  }

  [Fact]
  public void ValidateProductionConfiguration_InDevelopment_WhenConnectionStringPresent_ReturnsTrue()
  {
    // Arrange
    _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
    _mockConfiguration.Setup(c => c[$"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}"]).Returns("valid-connection-string");

    // Act
    var result = ValidateStartup.ValidateProductionConfiguration(_mockEnvironment.Object, _mockConfiguration.Object);

    // Assert
    result.Should().BeTrue();
    ValidateStartup.IsIdentityDbAvailable.Should().BeTrue();
  }
}
