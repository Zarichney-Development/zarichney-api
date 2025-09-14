using Zarichney.Cookbook.Recipes;

namespace Zarichney.Tests.TestData.Builders;

public class SynthesizedRecipeBuilder
{
    private readonly SynthesizedRecipe _recipe;

    public SynthesizedRecipeBuilder()
    {
        _recipe = new SynthesizedRecipe
        {
            Title = "Test Recipe",
            Description = "A delicious test recipe",
            Servings = "4",
            PrepTime = "15 minutes",
            CookTime = "30 minutes",
            TotalTime = "45 minutes",
            Ingredients = new List<string>
            {
                "2 cups flour",
                "1 cup sugar",
                "3 eggs",
                "1 cup milk"
            },
            Directions = new List<string>
            {
                "Preheat oven to 350°F",
                "Mix dry ingredients",
                "Add wet ingredients",
                "Bake for 30 minutes"
            },
            Notes = "This is a test recipe for unit testing",
            InspiredBy = new List<string>(),
            ImageUrls = new List<string>(),
            SourceRecipes = new List<Recipe>(),
            AttemptCount = 1
        };
    }

    public SynthesizedRecipeBuilder WithTitle(string title)
    {
        _recipe.Title = title;
        return this;
    }

    public SynthesizedRecipeBuilder WithDescription(string description)
    {
        _recipe.Description = description;
        return this;
    }

    public SynthesizedRecipeBuilder WithServings(string servings)
    {
        _recipe.Servings = servings;
        return this;
    }

    public SynthesizedRecipeBuilder WithPrepTime(string prepTime)
    {
        _recipe.PrepTime = prepTime;
        return this;
    }

    public SynthesizedRecipeBuilder WithCookTime(string cookTime)
    {
        _recipe.CookTime = cookTime;
        return this;
    }

    public SynthesizedRecipeBuilder WithTotalTime(string totalTime)
    {
        _recipe.TotalTime = totalTime;
        return this;
    }

    public SynthesizedRecipeBuilder WithIngredients(params string[] ingredients)
    {
        _recipe.Ingredients = ingredients.ToList();
        return this;
    }

    public SynthesizedRecipeBuilder WithDirections(params string[] directions)
    {
        _recipe.Directions = directions.ToList();
        return this;
    }

    public SynthesizedRecipeBuilder WithNotes(string notes)
    {
        _recipe.Notes = notes;
        return this;
    }

    public SynthesizedRecipeBuilder WithInspiredBy(params string[] urls)
    {
        _recipe.InspiredBy = urls.ToList();
        return this;
    }

    public SynthesizedRecipeBuilder WithImageUrls(params string[] urls)
    {
        _recipe.ImageUrls = urls.ToList();
        return this;
    }

    public SynthesizedRecipeBuilder WithSourceRecipes(params Recipe[] recipes)
    {
        _recipe.SourceRecipes = recipes.ToList();
        return this;
    }

    public SynthesizedRecipeBuilder WithQualityScore(int score)
    {
        _recipe.QualityScore = score;
        return this;
    }

    public SynthesizedRecipeBuilder WithAnalysis(string analysis)
    {
        _recipe.Analysis = analysis;
        return this;
    }

    public SynthesizedRecipeBuilder WithSuggestions(string suggestions)
    {
        _recipe.Suggestions = suggestions;
        return this;
    }

    public SynthesizedRecipeBuilder WithAttemptCount(int count)
    {
        _recipe.AttemptCount = count;
        return this;
    }

    public SynthesizedRecipeBuilder WithRevisions(params SynthesizedRecipe[] revisions)
    {
        _recipe.Revisions = revisions.ToList();
        return this;
    }

    public SynthesizedRecipeBuilder AsAnalyzed(int qualityScore = 85)
    {
        _recipe.QualityScore = qualityScore;
        _recipe.Analysis = "This recipe has been analyzed and meets quality standards.";
        _recipe.Suggestions = "Consider adding more seasoning for enhanced flavor.";
        return this;
    }

    public SynthesizedRecipeBuilder AsSimple()
    {
        _recipe.Ingredients = new List<string> { "1 ingredient" };
        _recipe.Directions = new List<string> { "1 step" };
        _recipe.PrepTime = "5 minutes";
        _recipe.CookTime = "10 minutes";
        _recipe.TotalTime = "15 minutes";
        return this;
    }

    public SynthesizedRecipe Build() => _recipe;
}