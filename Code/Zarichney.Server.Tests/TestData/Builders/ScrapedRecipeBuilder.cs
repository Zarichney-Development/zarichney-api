using Zarichney.Cookbook.Recipes;

namespace Zarichney.Server.Tests.TestData.Builders;

/// <summary>
/// Builder for creating ScrapedRecipe instances for testing.
/// Provides fluent API for test data construction.
/// </summary>
public class ScrapedRecipeBuilder
{
  private ScrapedRecipe _scrapedRecipe = new()
  {
    Ingredients = [],
    Directions = []
  };

  public ScrapedRecipeBuilder WithDefaults()
  {
    _scrapedRecipe = new ScrapedRecipe
    {
      Id = $"test-recipe-{Guid.NewGuid():N}",
      RecipeUrl = "https://example.com/recipes/test-recipe",
      ImageUrl = "https://example.com/images/test-recipe.jpg",
      Title = "Test Recipe Title",
      Description = "A delicious test recipe for unit testing",
      Servings = "4",
      PrepTime = "15 min",
      CookTime = "30 min",
      TotalTime = "45 min",
      Notes = "Test recipe notes",
      Ingredients = ["1 cup test ingredient", "2 tbsp another ingredient", "Salt and pepper to taste"],
      Directions = ["Preheat oven to 350°F", "Mix ingredients together", "Bake for 30 minutes", "Serve immediately"]
    };
    return this;
  }

  public ScrapedRecipeBuilder WithId(string? id)
  {
    _scrapedRecipe.Id = id;
    return this;
  }

  public ScrapedRecipeBuilder WithRecipeUrl(string? recipeUrl)
  {
    _scrapedRecipe.RecipeUrl = recipeUrl;
    return this;
  }

  public ScrapedRecipeBuilder WithImageUrl(string? imageUrl)
  {
    _scrapedRecipe.ImageUrl = imageUrl;
    return this;
  }

  public ScrapedRecipeBuilder WithTitle(string? title)
  {
    _scrapedRecipe.Title = title;
    return this;
  }

  public ScrapedRecipeBuilder WithDescription(string? description)
  {
    _scrapedRecipe.Description = description;
    return this;
  }

  public ScrapedRecipeBuilder WithServings(string? servings)
  {
    _scrapedRecipe.Servings = servings;
    return this;
  }

  public ScrapedRecipeBuilder WithPrepTime(string? prepTime)
  {
    _scrapedRecipe.PrepTime = prepTime;
    return this;
  }

  public ScrapedRecipeBuilder WithCookTime(string? cookTime)
  {
    _scrapedRecipe.CookTime = cookTime;
    return this;
  }

  public ScrapedRecipeBuilder WithTotalTime(string? totalTime)
  {
    _scrapedRecipe.TotalTime = totalTime;
    return this;
  }

  public ScrapedRecipeBuilder WithNotes(string? notes)
  {
    _scrapedRecipe.Notes = notes;
    return this;
  }

  public ScrapedRecipeBuilder WithIngredients(params string[] ingredients)
  {
    _scrapedRecipe.Ingredients = ingredients.ToList();
    return this;
  }

  public ScrapedRecipeBuilder WithDirections(params string[] directions)
  {
    _scrapedRecipe.Directions = directions.ToList();
    return this;
  }

  public ScrapedRecipeBuilder WithEmptyIngredients()
  {
    _scrapedRecipe.Ingredients = [];
    return this;
  }

  public ScrapedRecipeBuilder WithEmptyDirections()
  {
    _scrapedRecipe.Directions = [];
    return this;
  }

  public ScrapedRecipe Build() => _scrapedRecipe;
}