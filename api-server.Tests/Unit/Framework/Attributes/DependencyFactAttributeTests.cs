using FluentAssertions;
using Xunit;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;

namespace Zarichney.Tests.Unit.Framework.Attributes;

[Trait(TestCategories.Category, TestCategories.Unit)]
public class DependencyFactAttributeTests
{
  [Fact]
  public void Constructor_WhenNoParametersProvided_RequiredFeaturesIsNull()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute();

    // Assert
    attribute.RequiredFeatures.Should().BeNull();
  }

  [Fact]
  public void Constructor_WhenApiFeatureParametersProvided_RequiredFeaturesContainsThoseFeatures()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute(ApiFeature.LLM, ApiFeature.Transcription);

    // Assert
    attribute.RequiredFeatures.Should().NotBeNull();
    attribute.RequiredFeatures.Should().HaveCount(2);
    attribute.RequiredFeatures.Should().Contain(ApiFeature.LLM);
    attribute.RequiredFeatures.Should().Contain(ApiFeature.Transcription);
  }

  [Fact]
  public void Constructor_WhenEmptyApiFeatureArrayProvided_RequiredFeaturesIsNull()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute(Array.Empty<ApiFeature>());

    // Assert
    attribute.RequiredFeatures.Should().BeNull();
  }

  [Fact]
  public void Constructor_WhenNullApiFeatureArrayProvided_RequiredFeaturesIsNull()
  {
    // Arrange & Act
    ApiFeature[] nullArray = null;
    var attribute = new DependencyFactAttribute(nullArray);

    // Assert
    attribute.RequiredFeatures.Should().BeNull();
  }
}
