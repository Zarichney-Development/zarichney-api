using AutoFixture;
using AutoFixture.Kernel;
using Zarichney.Cookbook.Recipes;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Tests.Framework.TestData.AutoFixtureCustomizations;

/// <summary>
/// AutoFixture customization for WebScraperService and related models.
/// Ensures consistent test data generation with realistic values.
/// </summary>
public class WebScraperCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // ScrapedRecipe customization with realistic data
    fixture.Customize<ScrapedRecipe>(composer => composer
      .FromFactory(() => new ScrapedRecipeBuilder().WithDefaults().Build())
      .OmitAutoProperties());

    // WebscraperConfig customization with reasonable test values
    fixture.Customize<WebscraperConfig>(composer => composer
      .With(x => x.MaxNumResultsPerQuery, 3)
      .With(x => x.MaxParallelTasks, 2) // Reduced for tests
      .With(x => x.MaxParallelSites, 2) // Reduced for tests
      .With(x => x.MaxWaitTimeMs, 5000) // Reduced for tests
      .With(x => x.MaxParallelPages, 1) // Reduced for tests
      .With(x => x.ErrorBuffer, 2)); // Reduced for tests

    // RecipeConfig customization with reasonable test values
    fixture.Customize<RecipeConfig>(composer => composer
      .With(x => x.MaxSearchResults, 5)
      .With(x => x.RecipesToReturnPerRetrieval, 3)
      .With(x => x.AcceptableScoreThreshold, 70)
      .With(x => x.SynthesisQualityThreshold, 80)
      .With(x => x.MaxNewRecipeNameAttempts, 3) // Reduced for tests
      .With(x => x.MaxParallelTasks, 2) // Reduced for tests
      .With(x => x.OutputDirectory, "TestData/Recipes"));

    // Ensure Lists are properly created for required properties
    fixture.Customize<List<string>>(composer => composer
      .FromFactory(() => ["test-item-1", "test-item-2", "test-item-3"]));

    // SearchResult customization for LLM responses
    fixture.Customize<Zarichney.Cookbook.Prompts.SearchResult>(composer => composer
      .With(x => x.SelectedIndices, new List<int> { 1, 2, 3 }));
  }
}
