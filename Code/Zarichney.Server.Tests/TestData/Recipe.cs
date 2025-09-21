namespace Zarichney.Server.Tests.TestData;

/// <summary>
/// Represents a recipe entity for testing purposes.
/// </summary>
[Obsolete("Use Zarichney.Cookbook.Recipes.Recipe instead with RecipeBuilder")]
public class LegacyTestRecipe
{
  /// <summary>
  /// Gets or sets the recipe ID.
  /// </summary>
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the recipe title.
  /// </summary>
  public string Title { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the recipe description.
  /// </summary>
  public string Description { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the list of ingredients.
  /// </summary>
  public List<string> Ingredients { get; set; } = [];

  /// <summary>
  /// Gets or sets the list of instructions.
  /// </summary>
  public List<string> Instructions { get; set; } = [];
}
