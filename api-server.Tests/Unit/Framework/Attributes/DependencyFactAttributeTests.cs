using FluentAssertions;
using Xunit;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;

namespace Zarichney.Tests.Unit.Framework.Attributes;

[Trait(TestCategories.Category, TestCategories.Unit)]
public class DependencyFactAttributeTests
{
  [Fact]
  public void Constructor_WhenNoParametersProvided_RequiredFeaturesAndInfrastructureAreNull()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute();

    // Assert
    attribute.RequiredFeatures.Should().BeNull();
    attribute.RequiredInfrastructure.Should().BeNull();
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
    attribute.RequiredInfrastructure.Should().BeNull();
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
  
  [Fact]
  public void Constructor_WithFeatures_CreatesTraitMappings()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute(ApiFeature.LLM, ApiFeature.GitHubAccess);
    
    // Assert - Check the dependency trait mappings
    var traitMappings = attribute.GetDependencyTraits();
    
    traitMappings.Should().NotBeNull();
    traitMappings.Should().HaveCount(2, "because each ApiFeature should map to a dependency trait");
    
    // Verify each ApiFeature maps to the expected trait value
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
    traits.Should().BeEmpty("because no ApiFeatures were provided");
  }
  
  [Fact]
  public void Constructor_WhenInfrastructureDependencyProvided_RequiredInfrastructureContainsThatDependency()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute(InfrastructureDependency.Database);
    
    // Assert
    attribute.RequiredInfrastructure.Should().NotBeNull();
    attribute.RequiredInfrastructure.Should().HaveCount(1);
    attribute.RequiredInfrastructure.Should().Contain(InfrastructureDependency.Database);
    attribute.RequiredFeatures.Should().BeNull();
  }
  
  [Fact]
  public void Constructor_WithMultipleInfrastructureDependencies_RequiredInfrastructureContainsAllDependencies()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute(
      InfrastructureDependency.Database, 
      InfrastructureDependency.Docker);
    
    // Assert
    attribute.RequiredInfrastructure.Should().NotBeNull();
    attribute.RequiredInfrastructure.Should().HaveCount(2);
    attribute.RequiredInfrastructure.Should().Contain(InfrastructureDependency.Database);
    attribute.RequiredInfrastructure.Should().Contain(InfrastructureDependency.Docker);
    attribute.RequiredFeatures.Should().BeNull();
  }
  
  [Fact]
  public void Constructor_WithInfrastructureDependencies_CreatesTraitMappings()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute(
      InfrastructureDependency.Database, 
      InfrastructureDependency.Docker);
    
    // Assert - Check the dependency trait mappings
    var traitMappings = attribute.GetDependencyTraits();
    
    traitMappings.Should().NotBeNull();
    traitMappings.Should().HaveCount(2, "because each InfrastructureDependency should map to a dependency trait");
    
    // Verify each InfrastructureDependency maps to the expected trait value
    traitMappings.Should().Contain(t => t.Name == TestCategories.Dependency && 
                                       t.Value == TestCategories.Database,
                                  "because Database maps to Database trait");
    traitMappings.Should().Contain(t => t.Name == TestCategories.Dependency && 
                                       t.Value == TestCategories.Docker,
                                  "because Docker maps to Docker trait");
  }
  
  [Fact]
  public void Constructor_WithApiFeatureAndInfrastructureDependency_SetsPropertiesAndTraitMappingsCorrectly()
  {
    // Arrange & Act
    var attribute = new DependencyFactAttribute(ApiFeature.LLM, InfrastructureDependency.Database);
    
    // Assert
    attribute.RequiredFeatures.Should().NotBeNull();
    attribute.RequiredFeatures.Should().HaveCount(1);
    attribute.RequiredFeatures.Should().Contain(ApiFeature.LLM);
    
    attribute.RequiredInfrastructure.Should().NotBeNull();
    attribute.RequiredInfrastructure.Should().HaveCount(1);
    attribute.RequiredInfrastructure.Should().Contain(InfrastructureDependency.Database);
    
    // Check trait mappings
    var traitMappings = attribute.GetDependencyTraits();
    traitMappings.Should().HaveCount(2, "because both dependencies should map to a trait");
    
    traitMappings.Should().Contain(t => t.Name == TestCategories.Dependency && 
                                       t.Value == TestCategories.ExternalOpenAI,
                                  "because LLM maps to ExternalOpenAI trait");
    traitMappings.Should().Contain(t => t.Name == TestCategories.Dependency && 
                                       t.Value == TestCategories.Database,
                                  "because Database maps to Database trait");
  }
}