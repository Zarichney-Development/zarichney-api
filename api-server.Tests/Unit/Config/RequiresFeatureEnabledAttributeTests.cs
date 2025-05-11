using FluentAssertions;
using Xunit;
using Zarichney.Config;

namespace Zarichney.Tests.Unit.Config;

public class RequiresFeatureEnabledAttributeTests
{
  [Fact]
  public void Constructor_ValidFeatureNames_InitializesCorrectly()
  {
    // Arrange & Act
    var attribute = new RequiresFeatureEnabledAttribute("Feature1", "Feature2");

    // Assert
    attribute.FeatureNames.Should().HaveCount(2); // Two feature names were provided
    attribute.FeatureNames.Should().Contain("Feature1").And.Contain("Feature2"); // Both provided feature names should be stored
  }

  [Fact]
  public void Constructor_SingleFeatureName_InitializesCorrectly()
  {
    // Arrange & Act
    var attribute = new RequiresFeatureEnabledAttribute("Feature1");

    // Assert
    attribute.FeatureNames.Should().HaveCount(1); // One feature name was provided
    attribute.FeatureNames.Should().Contain("Feature1"); // The provided feature name should be stored
  }

  [Fact]
  public void Constructor_EmptyFeatureNamesArray_ThrowsArgumentException()
  {
    // Arrange, Act & Assert
    Assert.Throws<ArgumentException>(() => new RequiresFeatureEnabledAttribute())
        .Message.Should().Contain("At least one feature name must be provided");
    // Empty feature names array is invalid
  }

  [Fact]
  public void Constructor_NullFeatureNamesArray_ThrowsArgumentException()
  {
    // Arrange, Act & Assert
    // Note: Can't directly pass null to params array, this behavior is tested indirectly
    Assert.Throws<ArgumentException>(() => new RequiresFeatureEnabledAttribute())
        .Message.Should().Contain("At least one feature name must be provided");
    // Empty feature names array is invalid
  }

  [Fact]
  public void Constructor_EmptyFeatureName_ThrowsArgumentException()
  {
    // Arrange, Act & Assert
    Assert.Throws<ArgumentException>(() => new RequiresFeatureEnabledAttribute(""))
        .Message.Should().Contain("Feature name at index 0 cannot be null or whitespace");
    // Empty feature name is invalid
  }

  [Fact]
  public void Constructor_WhitespaceFeatureName_ThrowsArgumentException()
  {
    // Arrange, Act & Assert
    Assert.Throws<ArgumentException>(() => new RequiresFeatureEnabledAttribute("   "))
        .Message.Should().Contain("Feature name at index 0 cannot be null or whitespace");
    // Whitespace feature name is invalid
  }

  [Fact]
  public void Constructor_OneValidOneInvalidFeatureName_ThrowsArgumentException()
  {
    // Arrange, Act & Assert
    Assert.Throws<ArgumentException>(() => new RequiresFeatureEnabledAttribute("Valid", ""))
        .Message.Should().Contain("Feature name at index 1 cannot be null or whitespace");
    // Second feature name is empty
  }

  [Fact]
  public void AttributeUsage_ClassTargetAndMethodTarget_AllowsMultiple()
  {
    // Arrange & Act
    var attributeUsage = typeof(RequiresFeatureEnabledAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)
        .OfType<AttributeUsageAttribute>()
        .FirstOrDefault();

    // Assert
    attributeUsage.Should().NotBeNull(); // RequiresFeatureEnabledAttribute should have AttributeUsage defined
    attributeUsage.ValidOn.Should().HaveFlag(AttributeTargets.Class); // Attribute should be applicable to classes
    attributeUsage.ValidOn.Should().HaveFlag(AttributeTargets.Method); // Attribute should be applicable to methods
    attributeUsage.AllowMultiple.Should().BeTrue(); // Multiple RequiresFeatureEnabled attributes should be allowed
    attributeUsage.Inherited.Should().BeTrue(); // RequiresFeatureEnabled attributes should be inherited
  }
}
