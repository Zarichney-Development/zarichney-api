using Zarichney.Cookbook.Orders;

namespace Zarichney.Tests.TestData.Builders;

public class CookbookOrderSubmissionBuilder
{
    private string _email = "test@example.com";
    private CookbookContent _content = new()
    {
        RecipeSpecificationType = "General",
        GeneralMealTypes = new List<string> { "Dinner", "Dessert" },
        ExpectedRecipeCount = 3
    };
    private CookbookDetails? _details = new()
    {
        Theme = "Italian Cuisine",
        PrimaryPurpose = "Family Dinners",
        DesiredCuisines = new List<string> { "Italian", "Mediterranean" },
        OverallStyle = "Traditional"
    };
    private UserDetails? _user = new()
    {
        SkillLevel = "Intermediate",
        TimeConstraints = "30-60 minutes",
        ServingSize = 4,
        DietaryRestrictions = new List<string>()
    };

    public CookbookOrderSubmissionBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CookbookOrderSubmissionBuilder WithCookbookContent(CookbookContent content)
    {
        _content = content;
        return this;
    }

    public CookbookOrderSubmissionBuilder WithCookbookDetails(CookbookDetails details)
    {
        _details = details;
        return this;
    }

    public CookbookOrderSubmissionBuilder WithUserDetails(UserDetails details)
    {
        _user = details;
        return this;
    }

    public CookbookOrderSubmissionBuilder WithSpecificRecipes(params string[] recipes)
    {
        _content.SpecificRecipes = recipes.ToList();
        _content.RecipeSpecificationType = "Specific";
        _content.ExpectedRecipeCount = recipes.Length;
        return this;
    }

    public CookbookOrderSubmissionBuilder WithGeneralMealTypes(params string[] mealTypes)
    {
        _content.GeneralMealTypes = mealTypes.ToList();
        _content.RecipeSpecificationType = "General";
        return this;
    }

    public CookbookOrderSubmissionBuilder WithExpectedRecipeCount(int count)
    {
        _content.ExpectedRecipeCount = count;
        return this;
    }

    public CookbookOrderSubmissionBuilder WithDietaryRestrictions(params string[] restrictions)
    {
        _user ??= new UserDetails();
        _user.DietaryRestrictions = restrictions.ToList();
        return this;
    }

    public CookbookOrderSubmissionBuilder WithAllergies(params string[] allergies)
    {
        _user ??= new UserDetails();
        _user.Allergies = allergies.ToList();
        return this;
    }

    public CookbookOrderSubmissionBuilder WithSkillLevel(string skillLevel)
    {
        _user ??= new UserDetails();
        _user.SkillLevel = skillLevel;
        return this;
    }

    public CookbookOrderSubmissionBuilder AsMinimal()
    {
        _details = null;
        _user = null;
        return this;
    }

    public CookbookOrderSubmission Build() => new()
    {
        Email = _email,
        CookbookContent = _content
        // Note: CookbookDetails/UserDetails have protected init setters and
        // cannot be set from test project. They will remain null by design.
    };
}
