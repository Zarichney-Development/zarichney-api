using Zarichney.Tests.Helpers;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Builder for Recipe test data.
/// </summary>
public class RecipeBuilder : BaseBuilder<RecipeBuilder, Recipe>
{
    /// <summary>
    /// Creates a new RecipeBuilder with default values.
    /// </summary>
    public RecipeBuilder()
    {
        WithId(Guid.NewGuid().ToString())
            .WithTitle("Test Recipe")
            .WithDescription("A test recipe description")
            .WithIngredients(["Ingredient 1", "Ingredient 2"])
            .WithInstructions(["Step 1", "Step 2"]);
    }

    /// <summary>
    /// Creates a new RecipeBuilder with random values.
    /// </summary>
    /// <returns>A new RecipeBuilder with random values.</returns>
    public static RecipeBuilder CreateRandom()
    {
        return new RecipeBuilder()
            .WithId(Guid.NewGuid().ToString())
            .WithTitle(GetRandom.String(10))
            .WithDescription(GetRandom.String(50))
            .WithIngredients([
                GetRandom.String(8),
                GetRandom.String(8),
                GetRandom.String(8)
            ])
            .WithInstructions([
                GetRandom.String(20),
                GetRandom.String(20),
                GetRandom.String(20)
            ]);
    }

    /// <summary>
    /// Sets the ID of the recipe.
    /// </summary>
    /// <param name="id">The ID to set.</param>
    /// <returns>The builder for method chaining.</returns>
    public RecipeBuilder WithId(string id)
    {
        Entity.Id = id;
        return Self();
    }

    /// <summary>
    /// Sets the title of the recipe.
    /// </summary>
    /// <param name="title">The title to set.</param>
    /// <returns>The builder for method chaining.</returns>
    public RecipeBuilder WithTitle(string title)
    {
        Entity.Title = title;
        return Self();
    }

    /// <summary>
    /// Sets the description of the recipe.
    /// </summary>
    /// <param name="description">The description to set.</param>
    /// <returns>The builder for method chaining.</returns>
    public RecipeBuilder WithDescription(string description)
    {
        Entity.Description = description;
        return Self();
    }

    /// <summary>
    /// Sets the ingredients of the recipe.
    /// </summary>
    /// <param name="ingredients">The ingredients to set.</param>
    /// <returns>The builder for method chaining.</returns>
    public RecipeBuilder WithIngredients(List<string> ingredients)
    {
        Entity.Ingredients = ingredients;
        return Self();
    }

    /// <summary>
    /// Sets the instructions of the recipe.
    /// </summary>
    /// <param name="instructions">The instructions to set.</param>
    /// <returns>The builder for method chaining.</returns>
    public RecipeBuilder WithInstructions(List<string> instructions)
    {
        Entity.Instructions = instructions;
        return Self();
    }
}