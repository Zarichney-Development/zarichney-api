using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Startup;

namespace Zarichney.Tests.Unit.Startup;

/// <summary>
/// Tests the validation of the Identity Database connection string in both Production and non-Production environments.
/// </summary>
[Trait("Category", "Unit")]
public class ProductionIdentityDbValidationTests
{
  /// <summary>
  /// This test verifies that the required UserDatabaseConnectionName constant exists in UserDbContext
  /// and has the expected value, as this is used in our validation logic.
  /// </summary>
  [Fact]
  public void UserDbContext_ShouldHave_UserDatabaseConnectionName_Constant()
  {
    // Arrange & Act
    var field = typeof(UserDbContext).GetField(
        "UserDatabaseConnectionName",
        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

    // Assert
    field.Should().NotBeNull("UserDbContext should have a UserDatabaseConnectionName constant");

    var value = field.GetValue(null) as string;
    value.Should().NotBeNull("UserDatabaseConnectionName should have a non-null value");
    value.Should().Be("UserDatabase", "UserDatabaseConnectionName should have the expected value");
  }

  /// <summary>
  /// This test verifies that ValidateStartup has the ValidateProductionConfiguration method
  /// that is used to validate the UserDatabase connection string in Production.
  /// </summary>
  [Fact]
  public void ValidateStartup_ShouldHave_ValidateProductionConfiguration_Method()
  {
    // Arrange & Act
    var methods = typeof(ValidateStartup).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.Name == "ValidateProductionConfiguration")
        .ToList();

    // Assert
    methods.Should().NotBeEmpty("ValidateStartup should have ValidateProductionConfiguration methods");

    // Verify we have both overloads
    var builderOverload = methods.FirstOrDefault(m => m.GetParameters().Length == 1 &&
        m.GetParameters()[0].ParameterType == typeof(WebApplicationBuilder));
    builderOverload.Should().NotBeNull("Should have a ValidateProductionConfiguration method that takes a WebApplicationBuilder");

    var environmentConfigOverload = methods.FirstOrDefault(m => m.GetParameters().Length == 2 &&
        m.GetParameters()[0].ParameterType == typeof(IWebHostEnvironment) &&
        m.GetParameters()[1].ParameterType == typeof(IConfiguration));
    environmentConfigOverload.Should().NotBeNull("Should have a ValidateProductionConfiguration method that takes IWebHostEnvironment and IConfiguration");
  }

  /// <summary>
  /// Verifies that the ValidateStartup class has the IsIdentityDbAvailable property.
  /// </summary>
  [Fact]
  public void ValidateStartup_ShouldHave_IsIdentityDbAvailable_Property()
  {
    // Arrange & Act
    var property = typeof(ValidateStartup).GetProperty(
        "IsIdentityDbAvailable",
        BindingFlags.Public | BindingFlags.Static);

    // Assert
    property.Should().NotBeNull("ValidateStartup should have an IsIdentityDbAvailable property");
    property.PropertyType.Should().Be(typeof(bool), "IsIdentityDbAvailable should be of type bool");
    property.CanRead.Should().BeTrue("IsIdentityDbAvailable should be readable");
  }

  /// <summary>
  /// Verifies that ValidateProductionConfiguration returns true in Production environment when configuration is valid.
  /// </summary>
  [Fact]
  public void ValidateProductionConfiguration_InProduction_WithValidConfig_ReturnsTrue()
  {
    // Arrange
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
    // No need to setup IsProduction as it's an extension method

    var mockConfiguration = new Mock<IConfiguration>();
    mockConfiguration.Setup(c => c["ConnectionStrings:" + UserDbContext.UserDatabaseConnectionName])
        .Returns("valid-connection-string");

    // Act - The method should not throw an exception
    var result = ValidateStartup.ValidateProductionConfiguration(mockEnvironment.Object, mockConfiguration.Object);

    // Assert
    result.Should().BeTrue("ValidateProductionConfiguration should return true with valid configuration");
    ValidateStartup.IsIdentityDbAvailable.Should().BeTrue("IsIdentityDbAvailable should be true with valid configuration");
  }

  /// <summary>
  /// Verifies that ValidateProductionConfiguration returns false in Development environment when configuration is missing.
  /// </summary>
  [Fact]
  public void ValidateProductionConfiguration_InDevelopment_WithMissingConfig_ReturnsFalse()
  {
    // Arrange
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
    // No need to setup IsProduction as it's an extension method

    var mockConfiguration = new Mock<IConfiguration>();
    mockConfiguration.Setup(c => c["ConnectionStrings:" + UserDbContext.UserDatabaseConnectionName])
        .Returns((string?)null);

    // Act
    var result = ValidateStartup.ValidateProductionConfiguration(mockEnvironment.Object, mockConfiguration.Object);

    // Assert
    result.Should().BeFalse("ValidateProductionConfiguration should return false with missing configuration");
    ValidateStartup.IsIdentityDbAvailable.Should().BeFalse("IsIdentityDbAvailable should be false with missing configuration");
  }

  /// <summary>
  /// Verifies that ValidateProductionConfiguration returns true in Development environment when configuration is valid.
  /// </summary>
  [Fact]
  public void ValidateProductionConfiguration_InDevelopment_WithValidConfig_ReturnsTrue()
  {
    // Arrange
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
    // No need to setup IsProduction as it's an extension method

    var mockConfiguration = new Mock<IConfiguration>();
    mockConfiguration.Setup(c => c["ConnectionStrings:" + UserDbContext.UserDatabaseConnectionName])
        .Returns("valid-connection-string");

    // Act
    var result = ValidateStartup.ValidateProductionConfiguration(mockEnvironment.Object, mockConfiguration.Object);

    // Assert
    result.Should().BeTrue("ValidateProductionConfiguration should return true with valid configuration");
    ValidateStartup.IsIdentityDbAvailable.Should().BeTrue("IsIdentityDbAvailable should be true with valid configuration");
  }
}
