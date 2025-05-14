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
  public void GetTraitToConfigNamesMap_WhenAccessingOpenAICategory_ContainsApiKeyConfig()
  {
    var traitToConfigNamesMapField = typeof(IntegrationTestBase)
        .GetField("TraitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

    var traitToConfigNamesMap = traitToConfigNamesMapField?.GetValue(null)
        as Dictionary<string, List<string>>;

    // Assert
    var openAiConfigs = traitToConfigNamesMap?[TestCategories.ExternalOpenAI];
    openAiConfigs.Should().NotBeNull();
    openAiConfigs.Should().Contain("OpenAI API Key");
  }

  [Fact]
  public void GetTraitToConfigNamesMap_WhenAccessingStripeCategory_ContainsSecretAndWebhookConfigs()
  {
    var traitToConfigNamesMapField = typeof(IntegrationTestBase)
        .GetField("TraitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

    var traitToConfigNamesMap = traitToConfigNamesMapField?.GetValue(null)
        as Dictionary<string, List<string>>;

    // Assert
    var stripeConfigs = traitToConfigNamesMap?[TestCategories.ExternalStripe];
    stripeConfigs.Should().NotBeNull();
    stripeConfigs.Should().Contain("Stripe Secret Key");
    stripeConfigs.Should().Contain("Stripe Webhook Secret");
  }

  [Fact]
  public void GetTraitToConfigNamesMap_WhenAccessingGitHubCategory_ContainsAccessTokenConfig()
  {
    var traitToConfigNamesMapField = typeof(IntegrationTestBase)
        .GetField("TraitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

    var traitToConfigNamesMap = traitToConfigNamesMapField?.GetValue(null)
        as Dictionary<string, List<string>>;

    // Assert
    var gitHubConfigs = traitToConfigNamesMap?[TestCategories.ExternalGitHub];
    gitHubConfigs.Should().NotBeNull();
    gitHubConfigs.Should().Contain("GitHub Access Token");
  }

  [Fact]
  public void GetTraitToConfigNamesMap_WhenAccessingMSGraphCategory_ContainsAllEmailConfigs()
  {
    var traitToConfigNamesMapField = typeof(IntegrationTestBase)
        .GetField("TraitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

    var traitToConfigNamesMap = traitToConfigNamesMapField?.GetValue(null)
        as Dictionary<string, List<string>>;

    // Assert
    var msGraphConfigs = traitToConfigNamesMap?[TestCategories.ExternalMSGraph];
    msGraphConfigs.Should().NotBeNull();
    msGraphConfigs.Should().Contain("Email AzureTenantId");
    msGraphConfigs.Should().Contain("Email AzureAppId");
    msGraphConfigs.Should().Contain("Email AzureAppSecret");
    msGraphConfigs.Should().Contain("Email FromEmail");
    msGraphConfigs.Should().Contain("Email MailCheckApiKey");
  }

  [Fact]
  public void GetRequiredFeaturesFromTestClass_WhenMethodHasDependencyFactWithFeatures_ReturnsFeatures()
  {
    // Arrange
    var testMethodInfo = typeof(DummyTestClassWithExternalServicess).GetMethod(nameof(DummyTestClassWithExternalServicess.TestWithLlmDependency));
    testMethodInfo.Should().NotBeNull();

    // Act - Use reflection to invoke private method GetRequiredFeaturesFromTestClass
    var getRequiredFeaturesMethod = typeof(IntegrationTestBase)
        .GetMethod("GetRequiredFeaturesFromTestClass", BindingFlags.Instance | BindingFlags.NonPublic);

    // Create an instance of IntegrationTestBase using dummy arguments
    var mockFixture = new Mock<ApiClientFixture>().Object;
    var mockOutputHelper = new Mock<ITestOutputHelper>().Object;

    // Create an instance of IntegrationTestBase using a mock fixture
    var mockIntegrationTestBase = new Mock<IntegrationTestBase>(mockFixture, mockOutputHelper) { CallBase = true }.Object;

    var result = getRequiredFeaturesMethod?.Invoke(mockIntegrationTestBase, new object[] { typeof(DummyTestClassWithExternalServicess) }) as ExternalServices[];

    // Assert
    result.Should().NotBeNull();
    result.Should().Contain(ExternalServices.LLM);
  }

  [Fact]
  public void GetRequiredFeaturesFromTestClass_WhenMethodHasEmptyDependencyFact_ReturnsNull()
  {
    // Arrange
    var testMethodInfo = typeof(DummyTestClassWithoutExternalServicess).GetMethod(nameof(DummyTestClassWithoutExternalServicess.TestWithoutExternalServicesDependency));
    testMethodInfo.Should().NotBeNull();

    // Act - Use reflection to invoke private method GetRequiredFeaturesFromTestClass
    var getRequiredFeaturesMethod = typeof(IntegrationTestBase)
        .GetMethod("GetRequiredFeaturesFromTestClass", BindingFlags.Instance | BindingFlags.NonPublic);

    var mockFixture = new Mock<ApiClientFixture>().Object;
    var mockOutputHelper = new Mock<ITestOutputHelper>().Object;

    // Create an instance of IntegrationTestBase using a mock fixture
    var mockIntegrationTestBase = new Mock<IntegrationTestBase>(mockFixture, mockOutputHelper) { CallBase = true }.Object;

    var result = getRequiredFeaturesMethod?.Invoke(mockIntegrationTestBase, new object[] { typeof(DummyTestClassWithoutExternalServicess) }) as ExternalServices[];

    // Assert
    result.Should().BeNull();
  }
}

// Helper classes for testing the GetRequiredFeaturesFromTestClass method
public class DummyTestClassWithExternalServicess
{
  [DependencyFact(ExternalServices.LLM)]
  public void TestWithLlmDependency() { }
}

public class DummyTestClassWithoutExternalServicess
{
  [DependencyFact]
  [Trait(TestCategories.Dependency, TestCategories.ExternalOpenAI)]
  public void TestWithoutExternalServicesDependency() { }
}
