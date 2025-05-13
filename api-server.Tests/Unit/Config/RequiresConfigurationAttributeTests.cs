using Zarichney.Services.Status;
using FluentAssertions;
using Xunit;

namespace Zarichney.Tests.Unit.Config;

public class RequiresConfigurationAttributeTests
{
  [Trait("Category", "Unit")]
  [Fact]
  public void Constructor_ValidFeatures_InitializesCorrectly()
  {
    // Arrange & Act
    var attribute = new RequiresConfigurationAttribute(ApiFeature.LLM, ApiFeature.Transcription);

    // Assert
    attribute.Features.Should().HaveCount(2); // Two features were provided
    attribute.Features.Should().Contain(ApiFeature.LLM).And.Contain(ApiFeature.Transcription);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void Constructor_SingleFeature_InitializesCorrectly()
  {
    // Arrange & Act
    var attribute = new RequiresConfigurationAttribute(ApiFeature.Core);

    // Assert
    attribute.Features.Should().HaveCount(1); // One feature was provided
    attribute.Features.Should().Contain(ApiFeature.Core);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void Constructor_EmptyFeaturesArray_ThrowsArgumentException()
  {
    // Arrange, Act & Assert
    Assert.Throws<ArgumentException>(() => new RequiresConfigurationAttribute())
        .Message.Should().Contain("At least one feature must be provided");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void AttributeUsage_ShouldBeApplicableOnProperties_WithInheritance()
  {
    // Arrange & Act
    var attributeUsage = typeof(RequiresConfigurationAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), true)
        .OfType<AttributeUsageAttribute>()
        .FirstOrDefault();

    // Assert
    attributeUsage.Should().NotBeNull();
    attributeUsage.ValidOn.Should().Be(AttributeTargets.Property);
    attributeUsage.Inherited.Should().BeTrue();
    attributeUsage.AllowMultiple.Should().BeFalse();
  }
}
