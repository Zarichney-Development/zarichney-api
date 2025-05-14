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
  public void Constructor_WhenExternalServicesParametersProvided_RequiredFeaturesContainsThoseFeatures()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute(ExternalServices.LLM, ExternalServices.Transcription);

    // Assert
    attribute.RequiredFeatures.Should().NotBeNull();
    attribute.RequiredFeatures.Should().HaveCount(2);
    attribute.RequiredFeatures.Should().Contain(ExternalServices.LLM);
    attribute.RequiredFeatures.Should().Contain(ExternalServices.Transcription);
  }

  [Fact]
  public void Constructor_WhenEmptyExternalServicesArrayProvided_RequiredFeaturesIsNull()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute(Array.Empty<ExternalServices>());

    // Assert
    attribute.RequiredFeatures.Should().BeNull();
  }

  [Fact]
  public void Constructor_WhenNullExternalServicesArrayProvided_RequiredFeaturesIsNull()
  {
    // Arrange & Act
    ExternalServices[] nullArray = null;
    var attribute = new DependencyFactAttribute(nullArray);

    // Assert
    attribute.RequiredFeatures.Should().BeNull();
  }

  [Fact]
  public void Constructor_WithFeatures_CreatesTraitMappings()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute(ExternalServices.LLM, ExternalServices.GitHubAccess);

    // Assert - Check the dependency trait mappings
    var traitMappings = attribute.GetDependencyTraits();

    traitMappings.Should().NotBeNull();
    traitMappings.Should().HaveCount(2, "because each ExternalServices should map to a dependency trait");

    // Verify each ExternalServices maps to the expected trait value
    traitMappings.Should().Contain(t => t.Name == TestCategories.Dependency &&
                                       t.Value == TestCategories.ExternalOpenAI,
                                  "because LLM maps to ExternalOpenAI trait");
    traitMappings.Should().Contain(t => t.Name == TestCategories.Dependency &&
                                       t.Value == TestCategories.ExternalGitHub,
                                  "because GitHubAccess maps to ExternalGitHub trait");
  }

  [Fact]
  public void GetDependencyTraits_NoFeatures_ReturnsEmptyCollection()
  {
    // Arrange
    var attribute = new DependencyFactAttribute();

    // Act
    var traits = attribute.GetDependencyTraits();

    // Assert
    traits.Should().BeEmpty("because no ExternalServicess were provided");
  }
}
