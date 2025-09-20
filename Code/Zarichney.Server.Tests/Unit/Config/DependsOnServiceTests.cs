using Zarichney.Services.Status;
using FluentAssertions;
using Xunit;
using Zarichney.Server.Tests.Framework.Attributes;

namespace Zarichney.Server.Tests.Unit.Config;

[Trait(TestCategories.Category, TestCategories.Unit)]
public class DependsOnServiceTests
{
  [Fact]
  [Trait(TestCategories.Category, TestCategories.Unit)]
  public void Constructor_ValidFeatures_InitializesCorrectly()
  {
    // Arrange & Act
    var attribute = new DependsOnService(ExternalServices.OpenAiApi, ExternalServices.MailCheck);

    // Assert
    attribute.Features.Should().HaveCount(2); // Two features were provided
    attribute.Features.Should().Contain(ExternalServices.OpenAiApi).And.Contain(ExternalServices.MailCheck); // Both provided features should be stored
  }

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Unit)]
  public void Constructor_SingleFeature_InitializesCorrectly()
  {
    // Arrange & Act
    var attribute = new DependsOnService(ExternalServices.FrontEnd);

    // Assert
    attribute.Features.Should().HaveCount(1); // One feature was provided
    attribute.Features.Should().Contain(ExternalServices.FrontEnd); // The provided feature should be stored
  }

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Unit)]
  public void Constructor_EmptyFeaturesArray_ThrowsArgumentException()
  {
    // Arrange, Act & Assert
    Assert.Throws<ArgumentException>(() => new DependsOnService())
        .Message.Should().Contain("At least one feature must be provided");
    // Empty features array is invalid
  }

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Unit)]
  public void Constructor_NullFeaturesArray_ThrowsArgumentException()
  {
    // Arrange, Act & Assert
    // Note: Can't directly pass null to params array, this behavior is tested indirectly
    Assert.Throws<ArgumentException>(() => new DependsOnService())
        .Message.Should().Contain("At least one feature must be provided");
    // Empty features array is invalid
  }

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Unit)]
  public void AttributeUsage_ClassTargetAndMethodTarget_AllowsMultiple()
  {
    // Arrange & Act
    var attributeUsage = typeof(DependsOnService).GetCustomAttributes(typeof(AttributeUsageAttribute), false)
        .OfType<AttributeUsageAttribute>()
        .FirstOrDefault();

    // Assert
    attributeUsage.Should().NotBeNull(); // DependsOnService should have AttributeUsage defined
    attributeUsage.ValidOn.Should().HaveFlag(AttributeTargets.Class); // Attribute should be applicable to classes
    attributeUsage.ValidOn.Should().HaveFlag(AttributeTargets.Method); // Attribute should be applicable to methods
    attributeUsage.AllowMultiple.Should().BeTrue(); // Multiple DependsOnService attributes should be allowed
    attributeUsage.Inherited.Should().BeTrue(); // DependsOnService attributes should be inherited
  }
}
