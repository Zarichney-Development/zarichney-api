using Zarichney.Cookbook.Recipes;
using Zarichney.Server.Tests.TestData;

namespace Zarichney.Server.Tests.Unit.Services;

/// <summary>
/// Interface for recipe repository operations.
/// </summary>
public interface IRecipeRepository
{
  /// <summary>
  /// Gets a recipe by its ID.
  /// </summary>
  /// <param name="id">The ID of the recipe to get.</param>
  /// <returns>The recipe, or null if not found.</returns>
  Task<Recipe?> GetByIdAsync(string id);

  /// <summary>
  /// Gets all recipes.
  /// </summary>
  /// <returns>A list of all recipes.</returns>
  Task<List<Recipe>> GetAllAsync();

  /// <summary>
  /// Creates a new recipe.
  /// </summary>
  /// <param name="recipe">The recipe to create.</param>
  /// <returns>The created recipe.</returns>
  Task<Recipe> CreateAsync(Recipe recipe);

  /// <summary>
  /// Updates an existing recipe.
  /// </summary>
  /// <param name="recipe">The recipe to update.</param>
  /// <returns>The updated recipe.</returns>
  Task<Recipe> UpdateAsync(Recipe recipe);

  /// <summary>
  /// Deletes a recipe by its ID.
  /// </summary>
  /// <param name="id">The ID of the recipe to delete.</param>
  /// <returns>True if the recipe was deleted, false otherwise.</returns>
  Task<bool> DeleteAsync(string id);
}
