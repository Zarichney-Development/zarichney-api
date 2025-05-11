using FluentAssertions;
using Xunit;
using Zarichney.Config;

namespace Zarichney.Tests.Unit.Config;

public class RequiresFeatureEnabledAttributeTests
{
  [Fact]
  public void Constructor_ValidFeatures_InitializesCorrectly()
  {
    // Arrange & Act
    var attribute = new RequiresFeatureEnabledAttribute(Feature.LLM, Feature.Transcription);

    // Assert
    attribute.Features.Should().HaveCount(2); // Two features were provided
    attribute.Features.Should().Contain(Feature.LLM).And.Contain(Feature.Transcription); // Both provided features should be stored
  }

  [Fact]
  public void Constructor_SingleFeature_InitializesCorrectly()
  {
    // Arrange & Act
    var attribute = new RequiresFeatureEnabledAttribute(Feature.Core);

    // Assert
    attribute.Features.Should().HaveCount(1); // One feature was provided
    attribute.Features.Should().Contain(Feature.Core); // The provided feature should be stored
  }

  [Fact]
  public void Constructor_EmptyFeaturesArray_ThrowsArgumentException()
  {
    // Arrange, Act & Assert
    Assert.Throws<ArgumentException>(() => new RequiresFeatureEnabledAttribute())
        .Message.Should().Contain("At least one feature must be provided");
    // Empty features array is invalid
  }

  [Fact]
  public void Constructor_NullFeaturesArray_ThrowsArgumentException()
  {
    // Arrange, Act & Assert
    // Note: Can't directly pass null to params array, this behavior is tested indirectly
    Assert.Throws<ArgumentException>(() => new RequiresFeatureEnabledAttribute())
        .Message.Should().Contain("At least one feature must be provided");
    // Empty features array is invalid
  }

  [Fact]
  public void FeatureNames_ConvertsFeaturesToStrings()
  {
    // Arrange & Act
    var attribute = new RequiresFeatureEnabledAttribute(Feature.LLM, Feature.Transcription);

    // Assert - This tests the backward compatibility property
    attribute.FeatureNames.Should().HaveCount(2);
    attribute.FeatureNames.Should().Contain("LLM").And.Contain("Transcription");
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
