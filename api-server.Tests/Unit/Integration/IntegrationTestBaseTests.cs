using System.Reflection;
using FluentAssertions;
using Xunit;
using Zarichney.Tests.Framework.Attributes;
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
        .GetField("_traitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

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
        .GetField("_traitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

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
        .GetField("_traitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

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
        .GetField("_traitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

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
        .GetField("_traitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

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
        .GetField("_traitToConfigNamesMap", BindingFlags.Static | BindingFlags.NonPublic);

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
}
