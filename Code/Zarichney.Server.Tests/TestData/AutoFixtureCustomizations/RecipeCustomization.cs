using AutoFixture;
using Zarichney.Cookbook.Prompts;
using Zarichney.Cookbook.Recipes;

namespace Zarichney.Tests.TestData.AutoFixtureCustomizations;

/// <summary>
/// AutoFixture customization for Recipe and related objects to generate valid test data.
/// Ensures Recipe objects have proper IDs, titles, and required properties populated.
/// </summary>
public class RecipeCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // Customize Recipe generation
    fixture.Customize<Recipe>(composer => composer
      .With(r => r.Id, () => Guid.NewGuid().ToString())
      .With(r => r.Title, () => GenerateRecipeTitle())
      .With(r => r.Description, () => fixture.Create<string>())
      .With(r => r.Servings, () => fixture.Create<Generator<int>>().First(x => x > 0 && x <= 12).ToString())
      .With(r => r.PrepTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 60)} minutes")
      .With(r => r.CookTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 180)} minutes")
      .With(r => r.TotalTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 240)} minutes")
      .With(r => r.Ingredients, () => GenerateIngredientList(fixture))
      .With(r => r.Directions, () => GenerateDirectionsList(fixture))
      .With(r => r.Notes, () => fixture.Create<string>())
      .With(r => r.Aliases, () => GenerateAliases(fixture))
      .With(r => r.IndexTitle, () => GenerateRecipeTitle().ToLowerInvariant())
      .With(r => r.Relevancy, () => new Dictionary<string, RelevancyResult>())
      .With(r => r.Cleaned, false)
      .Without(r => r.RecipeUrl)
      .Without(r => r.ImageUrl));

    // Customize RelevancyResult generation
    fixture.Customize<RelevancyResult>(composer => composer
      .With(r => r.Query, () => fixture.Create<string>())
      .With(r => r.Score, () => fixture.Create<Generator<int>>().First(x => x >= 0 && x <= 100))
      .With(r => r.Reasoning, () => fixture.Create<string>()));

    // Customize ScrapedRecipe generation
    fixture.Customize<ScrapedRecipe>(composer => composer
      .With(r => r.Id, () => Guid.NewGuid().ToString())
      .With(r => r.Title, () => GenerateRecipeTitle())
      .With(r => r.Description, () => fixture.Create<string>())
      .With(r => r.Servings, () => fixture.Create<Generator<int>>().First(x => x > 0 && x <= 12).ToString())
      .With(r => r.PrepTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 60)} minutes")
      .With(r => r.CookTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 180)} minutes")
      .With(r => r.TotalTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 240)} minutes")
      .With(r => r.Ingredients, () => GenerateIngredientList(fixture))
      .With(r => r.Directions, () => GenerateDirectionsList(fixture))
      .With(r => r.Notes, () => fixture.Create<string>())
      .Without(r => r.RecipeUrl)
      .Without(r => r.ImageUrl));

    // Customize CleanedRecipe generation
    fixture.Customize<CleanedRecipe>(composer => composer
      .With(r => r.Cleaned, true)
      .With(r => r.Title, () => GenerateRecipeTitle())
      .With(r => r.Description, () => fixture.Create<string>())
      .With(r => r.Servings, () => fixture.Create<Generator<int>>().First(x => x > 0 && x <= 12).ToString())
      .With(r => r.PrepTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 60)} minutes")
      .With(r => r.CookTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 180)} minutes")
      .With(r => r.TotalTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 240)} minutes")
      .With(r => r.Ingredients, () => GenerateIngredientList(fixture))
      .With(r => r.Directions, () => GenerateDirectionsList(fixture))
      .With(r => r.Notes, () => fixture.Create<string>()));

    // Customize SynthesizedRecipe generation
    fixture.Customize<SynthesizedRecipe>(composer => composer
      .With(r => r.Title, () => GenerateRecipeTitle())
      .With(r => r.Description, () => fixture.Create<string>())
      .With(r => r.Servings, () => fixture.Create<Generator<int>>().First(x => x > 0 && x <= 12).ToString())
      .With(r => r.PrepTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 60)} minutes")
      .With(r => r.CookTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 180)} minutes")
      .With(r => r.TotalTime, () => $"{fixture.Create<Generator<int>>().First(x => x > 0 && x <= 240)} minutes")
      .With(r => r.Ingredients, () => GenerateIngredientList(fixture))
      .With(r => r.Directions, () => GenerateDirectionsList(fixture))
      .With(r => r.Notes, () => fixture.Create<string>())
      .Without(r => r.InspiredBy)
      .Without(r => r.ImageUrls)
      .Without(r => r.SourceRecipes)
      .Without(r => r.QualityScore)
      .Without(r => r.Analysis)
      .Without(r => r.Suggestions)
      .With(r => r.AttemptCount, 0)
      .Without(r => r.Revisions));

    // Customize RecipeConfig generation
    fixture.Customize<RecipeConfig>(composer => composer
      .With(c => c.MaxSearchResults, 8)
      .With(c => c.RecipesToReturnPerRetrieval, 3)
      .With(c => c.AcceptableScoreThreshold, 70)
      .With(c => c.SynthesisQualityThreshold, 80)
      .With(c => c.MaxNewRecipeNameAttempts, 6)
      .With(c => c.MaxParallelTasks, 5)
      .With(c => c.OutputDirectory, "Data/Recipes"));
  }

  private static string GenerateRecipeTitle()
  {
    var dishes = new[]
    {
      "Chocolate Cake", "Vanilla Ice Cream", "Beef Stew", "Chicken Curry",
      "Spaghetti Carbonara", "Caesar Salad", "Mushroom Risotto", "Fish Tacos",
      "Banana Bread", "Apple Pie", "Tomato Soup", "Grilled Cheese",
      "Pad Thai", "Sushi Roll", "BBQ Ribs", "Pizza Margherita"
    };
    return dishes[Random.Shared.Next(dishes.Length)];
  }

  private static List<string> GenerateIngredientList(IFixture fixture)
  {
    var count = fixture.Create<Generator<int>>().First(x => x >= 2 && x <= 8);
    var ingredients = new List<string>();
    for (int i = 0; i < count; i++)
    {
      ingredients.Add($"{fixture.Create<string>().Substring(0, Math.Min(20, fixture.Create<string>().Length))} ingredient");
    }
    return ingredients;
  }

  private static List<string> GenerateDirectionsList(IFixture fixture)
  {
    var count = fixture.Create<Generator<int>>().First(x => x >= 2 && x <= 6);
    var directions = new List<string>();
    for (int i = 0; i < count; i++)
    {
      directions.Add($"Step {i + 1}: {fixture.Create<string>()}");
    }
    return directions;
  }

  private static List<string> GenerateAliases(IFixture fixture)
  {
    var count = fixture.Create<Generator<int>>().First(x => x >= 0 && x <= 3);
    var aliases = new List<string>();
    for (int i = 0; i < count; i++)
    {
      aliases.Add($"Alias {fixture.Create<string>().Substring(0, Math.Min(15, fixture.Create<string>().Length))}");
    }
    return aliases;
  }
}