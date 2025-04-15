using System.Collections.Concurrent;

namespace Zarichney.Cookbook.Recipes;

public interface IRecipeIndexer
{
  void AddRecipe(Recipe recipe);
  bool TryGetExactMatches(string key, out ConcurrentDictionary<string, Recipe> matches);
  IEnumerable<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>> GetAllRecipes();
}

internal class RecipeIndexer(
  ILogger<RecipeIndexer> logger
) : IRecipeIndexer
{
  private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Recipe>> _recipes =
    new(StringComparer.OrdinalIgnoreCase);

  private readonly ConcurrentDictionary<string, byte> _recipeIds = new();

  public void AddRecipe(Recipe recipe)
  {
    if (string.IsNullOrEmpty(recipe.Id))
    {
      throw new ArgumentException("Recipe must have an ID", nameof(recipe));
    }

    _recipeIds.TryAdd(recipe.Id, 0);
    logger.LogDebug("Adding recipe to index: {Title} (ID: {Id})", recipe.Title, recipe.Id);

    // For each title & alias, map this recipe to the dictionary for fast access
    AddToDictionary(recipe.Title!, recipe);
    foreach (var alias in recipe.Aliases)
    {
      AddToDictionary(alias, recipe);
    }
  }

  public bool TryGetExactMatches(string key, out ConcurrentDictionary<string, Recipe> matches)
  {
    var result = _recipes.TryGetValue(key, out matches!);
    logger.LogDebug("Exact match lookup for key '{Key}': {Result}", key, result);
    return result;
  }

  public IEnumerable<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>> GetAllRecipes()
  {
    var count = _recipes.Count;
    logger.LogDebug("Getting all recipes. Total keys in index: {Count}", count);
    return _recipes;
  }

  private void AddToDictionary(string key, Recipe recipe)
  {
    var recipeDict = _recipes.GetOrAdd(key, _ => new ConcurrentDictionary<string, Recipe>());
    recipeDict.AddOrUpdate(recipe.Id!, recipe, (_, _) => recipe);
    logger.LogDebug("Added/Updated recipe '{Title}' under key '{Key}'", recipe.Title, key);
  }
}