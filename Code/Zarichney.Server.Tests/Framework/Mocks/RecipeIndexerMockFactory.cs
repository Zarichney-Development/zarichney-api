using System.Collections.Concurrent;
using Moq;
using Zarichney.Cookbook.Recipes;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Framework.Mocks;

/// <summary>
/// Factory for creating mock implementations of IRecipeIndexer with various configurations.
/// Centralizes common recipe indexer mock setups to reduce duplication across tests.
/// </summary>
public static class RecipeIndexerMockFactory
{
  /// <summary>
  /// Creates a basic mock of IRecipeIndexer with no setup.
  /// </summary>
  public static Mock<IRecipeIndexer> CreateDefault()
  {
    return new Mock<IRecipeIndexer>();
  }

  /// <summary>
  /// Creates a mock of IRecipeIndexer with empty index (no recipes).
  /// </summary>
  public static Mock<IRecipeIndexer> CreateEmpty()
  {
    var mock = new Mock<IRecipeIndexer>();

    mock.Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
        .Returns(false);

    mock.Setup(x => x.GetAllRecipes())
        .Returns(Enumerable.Empty<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    return mock;
  }

  /// <summary>
  /// Creates a mock of IRecipeIndexer with pre-populated recipes.
  /// </summary>
  /// <param name="recipes">The recipes to populate the index with.</param>
  public static Mock<IRecipeIndexer> CreateWithRecipes(params Recipe[] recipes)
  {
    var mock = new Mock<IRecipeIndexer>();
    var index = new Dictionary<string, ConcurrentDictionary<string, Recipe>>(StringComparer.OrdinalIgnoreCase);

    // Build the index from provided recipes
    foreach (var recipe in recipes)
    {
      // Index by title
      if (!string.IsNullOrEmpty(recipe.Title))
      {
        AddToIndex(index, recipe.Title, recipe);
      }

      // Index by aliases
      foreach (var alias in recipe.Aliases ?? new List<string>())
      {
        AddToIndex(index, alias, recipe);
      }
    }

    // Setup TryGetExactMatches
    mock.Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
        .Returns((string key, out ConcurrentDictionary<string, Recipe> matches) =>
        {
          if (index.TryGetValue(key, out var result))
          {
            matches = result;
            return true;
          }
          matches = null!;
          return false;
        });

    // Setup GetAllRecipes
    mock.Setup(x => x.GetAllRecipes())
        .Returns(index);

    return mock;
  }

  /// <summary>
  /// Creates a mock of IRecipeIndexer with sample test recipes for common scenarios.
  /// </summary>
  public static Mock<IRecipeIndexer> CreateWithSampleRecipes()
  {
    var chocolateCake = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Chocolate Cake")
      .WithAliases(["Choco Cake", "Dark Chocolate Dessert"])
      .WithRelevancyScore("chocolate", 95, "High relevance to chocolate query")
      .WithRelevancyScore("cake", 85, "High relevance to cake query")
      .WithRelevancyScore("dessert", 70, "Moderate relevance to dessert query")
      .Build();

    var vanillaCake = new RecipeBuilder()
      .WithId("recipe-2")
      .WithTitle("Vanilla Cake")
      .WithAliases(["Classic Vanilla Cake", "White Cake"])
      .WithRelevancyScore("vanilla", 95, "High relevance to vanilla query")
      .WithRelevancyScore("cake", 85, "High relevance to cake query")
      .WithRelevancyScore("dessert", 70, "Moderate relevance to dessert query")
      .Build();

    var beefStew = new RecipeBuilder()
      .WithId("recipe-3")
      .WithTitle("Beef Stew")
      .WithAliases(["Hearty Beef Stew", "Classic Beef Stew"])
      .WithRelevancyScore("beef", 95, "High relevance to beef query")
      .WithRelevancyScore("stew", 90, "High relevance to stew query")
      .WithRelevancyScore("dinner", 80, "High relevance to dinner query")
      .Build();

    return CreateWithRecipes(chocolateCake, vanillaCake, beefStew);
  }

  /// <summary>
  /// Creates a mock of IRecipeIndexer that simulates concurrent access behavior.
  /// </summary>
  public static Mock<IRecipeIndexer> CreateThreadSafe()
  {
    var mock = new Mock<IRecipeIndexer>();
    var index = new ConcurrentDictionary<string, ConcurrentDictionary<string, Recipe>>(StringComparer.OrdinalIgnoreCase);
    var recipeIds = new ConcurrentDictionary<string, byte>();

    // Setup AddRecipe to actually maintain state
    mock.Setup(x => x.AddRecipe(It.IsAny<Recipe>()))
        .Callback<Recipe>(recipe =>
        {
          if (string.IsNullOrEmpty(recipe.Id))
            throw new ArgumentException("Recipe must have an ID", nameof(recipe));

          recipeIds.TryAdd(recipe.Id, 0);

          // Add by title
          if (!string.IsNullOrEmpty(recipe.Title))
          {
            var recipeDict = index.GetOrAdd(recipe.Title, _ => new ConcurrentDictionary<string, Recipe>());
            recipeDict.AddOrUpdate(recipe.Id, recipe, (_, _) => recipe);
          }

          // Add by aliases
          foreach (var alias in recipe.Aliases ?? new List<string>())
          {
            var recipeDict = index.GetOrAdd(alias, _ => new ConcurrentDictionary<string, Recipe>());
            recipeDict.AddOrUpdate(recipe.Id, recipe, (_, _) => recipe);
          }
        });

    // Setup TryGetExactMatches
    mock.Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
        .Returns((string key, out ConcurrentDictionary<string, Recipe> matches) =>
        {
          if (index.TryGetValue(key, out var result))
          {
            matches = result;
            return true;
          }
          matches = null!;
          return false;
        });

    // Setup GetAllRecipes
    mock.Setup(x => x.GetAllRecipes())
        .Returns(() => index);

    return mock;
  }

  /// <summary>
  /// Creates a mock of IRecipeIndexer that throws exceptions for testing error handling.
  /// </summary>
  public static Mock<IRecipeIndexer> CreateFailing(Exception? exception = null)
  {
    var mock = new Mock<IRecipeIndexer>();
    var ex = exception ?? new InvalidOperationException("Recipe indexer is unavailable");

    mock.Setup(x => x.AddRecipe(It.IsAny<Recipe>()))
        .Throws(ex);

    mock.Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
        .Throws(ex);

    mock.Setup(x => x.GetAllRecipes())
        .Throws(ex);

    return mock;
  }

  private static void AddToIndex(Dictionary<string, ConcurrentDictionary<string, Recipe>> index, string key, Recipe recipe)
  {
    if (!index.TryGetValue(key, out var recipeDict))
    {
      recipeDict = new ConcurrentDictionary<string, Recipe>();
      index[key] = recipeDict;
    }
    recipeDict[recipe.Id!] = recipe;
  }
}