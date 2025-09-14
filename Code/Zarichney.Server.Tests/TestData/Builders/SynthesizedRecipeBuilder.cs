using Zarichney.Cookbook.Recipes;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Test data builder for SynthesizedRecipe objects.
/// Provides a fluent interface for creating synthesized recipe test data.
/// </summary>
public class SynthesizedRecipeBuilder
{
    private string? _title = "Test Recipe";
    private string _description = "A delicious test recipe";
    private string _servings = "4";
    private string _prepTime = "15 minutes";
    private string _cookTime = "30 minutes";
    private string _totalTime = "45 minutes";
    private List<string> _ingredients = ["Ingredient 1", "Ingredient 2", "Ingredient 3"];
    private List<string> _directions = ["Step 1", "Step 2", "Step 3"];
    private string _notes = "Test notes";
    private List<string>? _inspiredBy = [];
    private List<string>? _imageUrls = [];
    private List<Recipe>? _sourceRecipes = [];
    private int? _qualityScore = null;
    private string? _analysis = null;
    private string? _suggestions = null;
    private int _attemptCount = 0;
    private List<SynthesizedRecipe>? _revisions = null;

    /// <summary>
    /// Sets the recipe title.
    /// </summary>
    public SynthesizedRecipeBuilder WithTitle(string? title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the recipe description.
    /// </summary>
    public SynthesizedRecipeBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the servings.
    /// </summary>
    public SynthesizedRecipeBuilder WithServings(string servings)
    {
        _servings = servings;
        return this;
    }

    /// <summary>
    /// Sets the preparation time.
    /// </summary>
    public SynthesizedRecipeBuilder WithPrepTime(string prepTime)
    {
        _prepTime = prepTime;
        return this;
    }

    /// <summary>
    /// Sets the cooking time.
    /// </summary>
    public SynthesizedRecipeBuilder WithCookTime(string cookTime)
    {
        _cookTime = cookTime;
        return this;
    }

    /// <summary>
    /// Sets the total time.
    /// </summary>
    public SynthesizedRecipeBuilder WithTotalTime(string totalTime)
    {
        _totalTime = totalTime;
        return this;
    }

    /// <summary>
    /// Sets the ingredients list.
    /// </summary>
    public SynthesizedRecipeBuilder WithIngredients(params string[] ingredients)
    {
        _ingredients = ingredients.ToList();
        return this;
    }

    /// <summary>
    /// Sets the directions list.
    /// </summary>
    public SynthesizedRecipeBuilder WithDirections(params string[] directions)
    {
        _directions = directions.ToList();
        return this;
    }

    /// <summary>
    /// Sets the notes.
    /// </summary>
    public SynthesizedRecipeBuilder WithNotes(string notes)
    {
        _notes = notes;
        return this;
    }

    /// <summary>
    /// Sets the URLs this recipe was inspired by.
    /// </summary>
    public SynthesizedRecipeBuilder WithInspiredBy(params string[] urls)
    {
        _inspiredBy = urls.ToList();
        return this;
    }

    /// <summary>
    /// Sets the image URLs.
    /// </summary>
    public SynthesizedRecipeBuilder WithImageUrls(params string[] urls)
    {
        _imageUrls = urls.ToList();
        return this;
    }

    /// <summary>
    /// Sets the source recipes.
    /// </summary>
    public SynthesizedRecipeBuilder WithSourceRecipes(params Recipe[] recipes)
    {
        _sourceRecipes = recipes.ToList();
        return this;
    }

    /// <summary>
    /// Sets the quality score.
    /// </summary>
    public SynthesizedRecipeBuilder WithQualityScore(int? qualityScore)
    {
        _qualityScore = qualityScore;
        return this;
    }

    /// <summary>
    /// Sets the analysis.
    /// </summary>
    public SynthesizedRecipeBuilder WithAnalysis(string? analysis)
    {
        _analysis = analysis;
        return this;
    }

    /// <summary>
    /// Sets the suggestions.
    /// </summary>
    public SynthesizedRecipeBuilder WithSuggestions(string? suggestions)
    {
        _suggestions = suggestions;
        return this;
    }

    /// <summary>
    /// Sets the attempt count.
    /// </summary>
    public SynthesizedRecipeBuilder WithAttemptCount(int attemptCount)
    {
        _attemptCount = attemptCount;
        return this;
    }

    /// <summary>
    /// Sets the revisions.
    /// </summary>
    public SynthesizedRecipeBuilder WithRevisions(List<SynthesizedRecipe>? revisions)
    {
        _revisions = revisions;
        return this;
    }

    /// <summary>
    /// Configures as a simple recipe with minimal details.
    /// </summary>
    public SynthesizedRecipeBuilder AsSimpleRecipe()
    {
        _qualityScore = null;
        _analysis = null;
        _suggestions = null;
        _revisions = null;
        return this;
    }

    /// <summary>
    /// Configures as a detailed recipe with all information.
    /// </summary>
    public SynthesizedRecipeBuilder AsDetailedRecipe()
    {
        _qualityScore = 85;
        _analysis = "Well-structured recipe with clear instructions";
        _suggestions = "Consider adding garnish options";
        _inspiredBy = ["https://example.com/recipe1", "https://example.com/recipe2"];
        _imageUrls = ["https://example.com/image1.jpg", "https://example.com/image2.jpg"];
        return this;
    }

    /// <summary>
    /// Configures as an analyzed recipe with quality metrics.
    /// </summary>
    public SynthesizedRecipeBuilder AsAnalyzedRecipe()
    {
        _qualityScore = 90;
        _analysis = "Excellent recipe with comprehensive instructions and ingredient list";
        _suggestions = "Recipe is well-optimized, no changes needed";
        return this;
    }

    /// <summary>
    /// Builds and returns the SynthesizedRecipe instance.
    /// </summary>
    public SynthesizedRecipe Build()
    {
        return new SynthesizedRecipe
        {
            Title = _title,
            Description = _description,
            Servings = _servings,
            PrepTime = _prepTime,
            CookTime = _cookTime,
            TotalTime = _totalTime,
            Ingredients = _ingredients,
            Directions = _directions,
            Notes = _notes,
            InspiredBy = _inspiredBy,
            ImageUrls = _imageUrls,
            SourceRecipes = _sourceRecipes,
            QualityScore = _qualityScore,
            Analysis = _analysis,
            Suggestions = _suggestions,
            AttemptCount = _attemptCount,
            Revisions = _revisions
        };
    }

    /// <summary>
    /// Creates multiple recipes with different characteristics.
    /// </summary>
    public static List<SynthesizedRecipe> BuildMultiple(int count, string baseTitle = "Recipe {0}")
    {
        List<SynthesizedRecipe> recipes = [];
        for (int i = 1; i <= count; i++)
        {
            recipes.Add(new SynthesizedRecipeBuilder()
                .WithTitle(string.Format(baseTitle, i))
                .Build());
        }
        return recipes;
    }
}