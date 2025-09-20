using Zarichney.Cookbook.Prompts;
using Zarichney.Cookbook.Recipes;
using Zarichney.Server.Tests.Framework.Helpers;

namespace Zarichney.Server.Tests.TestData.Builders;

/// <summary>
/// Builder for Recipe test data.
/// </summary>
public class RecipeBuilder
{
  private Recipe _recipe;

  /// <summary>
  /// Creates a new RecipeBuilder with default values.
  /// </summary>
  public RecipeBuilder()
  {
    _recipe = new Recipe
    {
      Id = Guid.NewGuid().ToString(),
      Title = "Test Recipe",
      Description = "A test recipe description",
      Servings = "4",
      PrepTime = "15 minutes",
      CookTime = "30 minutes",
      TotalTime = "45 minutes",
      Ingredients = ["Ingredient 1", "Ingredient 2"],
      Directions = ["Step 1", "Step 2"],
      Notes = "Test notes",
      Aliases = [],
      IndexTitle = "test recipe",
      Relevancy = new Dictionary<string, RelevancyResult>()
    };
  }

  /// <summary>
  /// Creates a new instance of the recipe with the configured values.
  /// </summary>
  /// <returns>A new instance of the recipe.</returns>
  public Recipe Build()
  {
    return _recipe;
  }

  /// <summary>
  /// Returns the builder for method chaining.
  /// </summary>
  /// <returns>The builder instance.</returns>
  protected RecipeBuilder Self()
  {
    return this;
  }

  /// <summary>
  /// Creates a new RecipeBuilder with random values.
  /// </summary>
  /// <returns>A new RecipeBuilder with random values.</returns>
  public static RecipeBuilder CreateRandom()
  {
    var builder = new RecipeBuilder();
    builder._recipe = new Recipe
    {
      Id = Guid.NewGuid().ToString(),
      Title = GetRandom.String(10),
      Description = GetRandom.String(50),
      Servings = GetRandom.Int(1, 8).ToString(),
      PrepTime = $"{GetRandom.Int(5, 60)} minutes",
      CookTime = $"{GetRandom.Int(10, 120)} minutes",
      TotalTime = $"{GetRandom.Int(15, 180)} minutes",
      Ingredients = [
        GetRandom.String(8),
        GetRandom.String(8),
        GetRandom.String(8)
      ],
      Directions = [
        GetRandom.String(20),
        GetRandom.String(20),
        GetRandom.String(20)
      ],
      Notes = GetRandom.String(30),
      Aliases = [],
      IndexTitle = GetRandom.String(10).ToLowerInvariant(),
      Relevancy = new Dictionary<string, RelevancyResult>()
    };
    return builder;
  }

  /// <summary>
  /// Sets the ID of the recipe.
  /// </summary>
  /// <param name="id">The ID to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithId(string id)
  {
    _recipe.Id = id;
    return Self();
  }

  /// <summary>
  /// Sets the title of the recipe.
  /// </summary>
  /// <param name="title">The title to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithTitle(string title)
  {
    _recipe.Title = title;
    return Self();
  }

  /// <summary>
  /// Sets the description of the recipe.
  /// </summary>
  /// <param name="description">The description to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithDescription(string description)
  {
    _recipe.Description = description;
    return Self();
  }

  /// <summary>
  /// Sets the ingredients of the recipe.
  /// </summary>
  /// <param name="ingredients">The ingredients to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithIngredients(List<string> ingredients)
  {
    _recipe.Ingredients = ingredients;
    return Self();
  }

  /// <summary>
  /// Sets the directions of the recipe.
  /// </summary>
  /// <param name="directions">The directions to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithDirections(List<string> directions)
  {
    _recipe.Directions = directions;
    return Self();
  }

  /// <summary>
  /// Sets the servings of the recipe.
  /// </summary>
  /// <param name="servings">The servings to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithServings(string servings)
  {
    _recipe.Servings = servings;
    return Self();
  }

  /// <summary>
  /// Sets the prep time of the recipe.
  /// </summary>
  /// <param name="prepTime">The prep time to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithPrepTime(string prepTime)
  {
    _recipe.PrepTime = prepTime;
    return Self();
  }

  /// <summary>
  /// Sets the cook time of the recipe.
  /// </summary>
  /// <param name="cookTime">The cook time to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithCookTime(string cookTime)
  {
    _recipe.CookTime = cookTime;
    return Self();
  }

  /// <summary>
  /// Sets the total time of the recipe.
  /// </summary>
  /// <param name="totalTime">The total time to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithTotalTime(string totalTime)
  {
    _recipe.TotalTime = totalTime;
    return Self();
  }

  /// <summary>
  /// Sets the notes of the recipe.
  /// </summary>
  /// <param name="notes">The notes to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithNotes(string notes)
  {
    _recipe.Notes = notes;
    return Self();
  }

  /// <summary>
  /// Sets the aliases of the recipe.
  /// </summary>
  /// <param name="aliases">The aliases to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithAliases(List<string> aliases)
  {
    _recipe.Aliases = aliases;
    return Self();
  }

  /// <summary>
  /// Sets the index title of the recipe.
  /// </summary>
  /// <param name="indexTitle">The index title to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithIndexTitle(string? indexTitle)
  {
    _recipe.IndexTitle = indexTitle;
    return Self();
  }

  /// <summary>
  /// Sets the relevancy scores of the recipe.
  /// </summary>
  /// <param name="relevancy">The relevancy scores to set.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithRelevancy(Dictionary<string, RelevancyResult> relevancy)
  {
    _recipe.Relevancy = relevancy;
    return Self();
  }

  /// <summary>
  /// Adds a relevancy score for a specific query.
  /// </summary>
  /// <param name="query">The query string.</param>
  /// <param name="score">The relevancy score (0-100).</param>
  /// <param name="reasoning">The reasoning for the score.</param>
  /// <returns>The builder for method chaining.</returns>
  public RecipeBuilder WithRelevancyScore(string query, int score, string? reasoning = null)
  {
    _recipe.Relevancy[query] = new RelevancyResult
    {
      Query = query,
      Score = score,
      Reasoning = reasoning ?? $"Test relevancy for {query}"
    };
    return Self();
  }
}
