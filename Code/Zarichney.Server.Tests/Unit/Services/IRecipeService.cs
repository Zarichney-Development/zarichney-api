using Zarichney.Tests.TestData;

namespace Zarichney.Tests.Unit.Services;

/// <summary>
/// Interface for recipe service operations.
/// </summary>
public interface IRecipeService
{
  /// <summary>
  /// Gets a recipe by its ID.
  /// </summary>
  /// <param name="id">The ID of the recipe to get.</param>
  /// <returns>The recipe, or null if not found.</returns>
  Task<Recipe?> GetRecipeByIdAsync(string id);

  /// <summary>
  /// Gets all recipes.
  /// </summary>
  /// <returns>A list of all recipes.</returns>
  Task<List<Recipe>> GetAllRecipesAsync();

  /// <summary>
  /// Creates a new recipe.
  /// </summary>
  /// <param name="recipe">The recipe to create.</param>
  /// <returns>The created recipe.</returns>
  Task<Recipe> CreateRecipeAsync(Recipe recipe);

  /// <summary>
  /// Updates an existing recipe.
  /// </summary>
  /// <param name="recipe">The recipe to update.</param>
  /// <returns>The updated recipe.</returns>
  Task<Recipe> UpdateRecipeAsync(Recipe recipe);

  /// <summary>
  /// Deletes a recipe by its ID.
  /// </summary>
  /// <param name="id">The ID of the recipe to delete.</param>
  /// <returns>True if the recipe was deleted, false otherwise.</returns>
  Task<bool> DeleteRecipeAsync(string id);

  /// <summary>
  /// Generates recipe suggestions based on a list of ingredients.
  /// </summary>
  /// <param name="ingredients">The list of ingredients to use.</param>
  /// <returns>A list of recipe suggestions.</returns>
  Task<List<Recipe>> GenerateRecipeSuggestionsAsync(List<string> ingredients);
}
