using System.Reflection;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Integration;

namespace Zarichney.Tests.Unit.Integration;

[Trait(TestCategories.Category, TestCategories.Unit)]
public class IntegrationTestBaseTests
{
  [Fact]
  public void GetTraitToConfigNamesMap_WhenAccessed_ContainsAllRequiredDependencyCategories()
  {
    // Use reflection to access the private static field in IntegrationTestBase
    var traitToConfigNamesMapField = typeof(IntegrationTestBase)
        .GetField("TraitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

    var traitToConfigNamesMap = traitToConfigNamesMapField?.GetValue(null)
        as Dictionary<string, List<string>>;

    // Assert
    traitToConfigNamesMap.Should().NotBeNull();
    traitToConfigNamesMap.Should().ContainKey(TestCategories.Database);
    traitToConfigNamesMap.Should().ContainKey(TestCategories.ExternalStripe);
    traitToConfigNamesMap.Should().ContainKey(TestCategories.ExternalOpenAI);
    traitToConfigNamesMap.Should().ContainKey(TestCategories.ExternalGitHub);
    traitToConfigNamesMap.Should().ContainKey(TestCategories.ExternalMSGraph);
  }

  [Fact]
  public void GetTraitToConfigNamesMap_WhenAccessingDatabaseCategory_ContainsDatabaseConnectionConfig()
  {
    var traitToConfigNamesMapField = typeof(IntegrationTestBase)
        .GetField("TraitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

    var traitToConfigNamesMap = traitToConfigNamesMapField?.GetValue(null)
        as Dictionary<string, List<string>>;

    // Assert
    var dbConfigs = traitToConfigNamesMap?[TestCategories.Database];
    dbConfigs.Should().NotBeNull();
    dbConfigs.Should().Contain("Database Connection");
  }

  [Fact]
  public void GetRequiredFeaturesFromTestClass_WhenMethodHasDependencyFactWithFeatures_ReturnsFeatures()
  {
    // Arrange
    var testMethodInfo = typeof(DummyTestClassWithApiFeatures).GetMethod(nameof(DummyTestClassWithApiFeatures.TestWithLlmDependency));
    testMethodInfo.Should().NotBeNull();

    // Act - Use reflection to invoke private method GetRequiredFeaturesFromTestClass
    var getRequiredFeaturesMethod = typeof(IntegrationTestBase)
        .GetMethod("GetRequiredFeaturesFromTestClass", BindingFlags.Instance | BindingFlags.NonPublic);

    // Create an instance of IntegrationTestBase using dummy arguments
    var mockFixture = new Mock<ApiClientFixture>().Object;
    var mockOutputHelper = new Mock<ITestOutputHelper>().Object;

    // Create an instance of IntegrationTestBase using a mock fixture
    var mockIntegrationTestBase = new Mock<IntegrationTestBase>(mockFixture, mockOutputHelper) { CallBase = true }.Object;

    var result = getRequiredFeaturesMethod?.Invoke(mockIntegrationTestBase, new object[] { typeof(DummyTestClassWithApiFeatures) }) as ApiFeature[];

    // Assert
    result.Should().NotBeNull();
    result.Should().Contain(ApiFeature.LLM);
  }
  
  [Fact]
  public void GetDependencyFactAttributeFromTestClass_WithInfrastructureDependency_ReturnsAttributeWithInfrastructure()
  {
    // Arrange
    var mockFixture = new Mock<ApiClientFixture>().Object;
    var mockOutputHelper = new Mock<ITestOutputHelper>().Object;
    
    // Create an instance of IntegrationTestBase using a mock fixture
    var mockIntegrationTestBase = new Mock<IntegrationTestBase>(mockFixture, mockOutputHelper) { CallBase = true }.Object;
    
    // Use reflection to invoke the private method
    var getDependencyFactAttributeMethod = typeof(IntegrationTestBase)
        .GetMethod("GetDependencyFactAttributeFromTestClass", BindingFlags.Instance | BindingFlags.NonPublic);
        
    // Act
    var result = getDependencyFactAttributeMethod?.Invoke(mockIntegrationTestBase, 
        new object[] { typeof(DummyTestClassWithInfrastructureDependency) }) as DependencyFactAttribute;
    
    // Assert
    result.Should().NotBeNull();
    result.RequiredInfrastructure.Should().NotBeNull();
    result.RequiredInfrastructure.Should().Contain(InfrastructureDependency.Database);
    result.RequiredFeatures.Should().BeNull();
  }
}

// Helper classes for testing
public class DummyTestClassWithApiFeatures
{
  [DependencyFact(ApiFeature.LLM)]
  public void TestWithLlmDependency() { }
}

public class DummyTestClassWithInfrastructureDependency
{
  [DependencyFact(InfrastructureDependency.Database)]
  public void TestWithDatabaseDependency() { }
}
