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
    var attribute = new RequiresConfigurationAttribute(ExternalServices.OpenAiApi, ExternalServices.EmailValidation);

    // Assert
    attribute.Features.Should().HaveCount(2); // Two features were provided
    attribute.Features.Should().Contain(ExternalServices.OpenAiApi).And.Contain(ExternalServices.EmailValidation);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void Constructor_SingleFeature_InitializesCorrectly()
  {
    // Arrange & Act
    var attribute = new RequiresConfigurationAttribute(ExternalServices.FrontEnd);

    // Assert
    attribute.Features.Should().HaveCount(1); // One feature was provided
    attribute.Features.Should().Contain(ExternalServices.FrontEnd);
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
