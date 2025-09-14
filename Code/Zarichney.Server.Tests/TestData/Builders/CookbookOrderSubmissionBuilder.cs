using Zarichney.Cookbook.Orders;
using System.Reflection;

namespace Zarichney.Tests.TestData.Builders;

public class CookbookOrderSubmissionBuilder
{
    private readonly CookbookOrderSubmission _submission;

    public CookbookOrderSubmissionBuilder()
    {
        _submission = new CookbookOrderSubmission
        {
            Email = "test@example.com",
            CookbookContent = new CookbookContent
            {
                RecipeSpecificationType = "General",
                GeneralMealTypes = new List<string> { "Dinner", "Dessert" },
                ExpectedRecipeCount = 3
            },
            CookbookDetails = new CookbookDetails
            {
                Theme = "Italian Cuisine",
                PrimaryPurpose = "Family Dinners",
                DesiredCuisines = new List<string> { "Italian", "Mediterranean" },
                OverallStyle = "Traditional"
            },
            UserDetails = new UserDetails
            {
                SkillLevel = "Intermediate",
                TimeConstraints = "30-60 minutes",
                ServingSize = 4,
                DietaryRestrictions = new List<string>()
            }
        };
    }

    public CookbookOrderSubmissionBuilder WithEmail(string email)
    {
        _submission.Email = email;
        return this;
    }

    public CookbookOrderSubmissionBuilder WithCookbookContent(CookbookContent content)
    {
        var property = typeof(CookbookOrderSubmission).GetProperty(nameof(CookbookOrderSubmission.CookbookContent));
        property?.SetValue(_submission, content);
        return this;
    }

    public CookbookOrderSubmissionBuilder WithCookbookDetails(CookbookDetails details)
    {
        var property = typeof(CookbookOrderSubmission).GetProperty(nameof(CookbookOrderSubmission.CookbookDetails));
        property?.SetValue(_submission, details);
        return this;
    }

    public CookbookOrderSubmissionBuilder WithUserDetails(UserDetails details)
    {
        var property = typeof(CookbookOrderSubmission).GetProperty(nameof(CookbookOrderSubmission.UserDetails));
        property?.SetValue(_submission, details);
        return this;
    }

    public CookbookOrderSubmissionBuilder WithSpecificRecipes(params string[] recipes)
    {
        _submission.CookbookContent.SpecificRecipes = recipes.ToList();
        _submission.CookbookContent.RecipeSpecificationType = "Specific";
        _submission.CookbookContent.ExpectedRecipeCount = recipes.Length;
        return this;
    }

    public CookbookOrderSubmissionBuilder WithGeneralMealTypes(params string[] mealTypes)
    {
        _submission.CookbookContent.GeneralMealTypes = mealTypes.ToList();
        _submission.CookbookContent.RecipeSpecificationType = "General";
        return this;
    }

    public CookbookOrderSubmissionBuilder WithExpectedRecipeCount(int count)
    {
        _submission.CookbookContent.ExpectedRecipeCount = count;
        return this;
    }

    public CookbookOrderSubmissionBuilder WithDietaryRestrictions(params string[] restrictions)
    {
        _submission.UserDetails ??= new UserDetails();
        _submission.UserDetails.DietaryRestrictions = restrictions.ToList();
        return this;
    }

    public CookbookOrderSubmissionBuilder WithAllergies(params string[] allergies)
    {
        _submission.UserDetails ??= new UserDetails();
        _submission.UserDetails.Allergies = allergies.ToList();
        return this;
    }

    public CookbookOrderSubmissionBuilder WithSkillLevel(string skillLevel)
    {
        _submission.UserDetails ??= new UserDetails();
        _submission.UserDetails.SkillLevel = skillLevel;
        return this;
    }

    public CookbookOrderSubmissionBuilder AsMinimal()
    {
        var detailsProperty = typeof(CookbookOrderSubmission).GetProperty(nameof(CookbookOrderSubmission.CookbookDetails));
        detailsProperty?.SetValue(_submission, null);
        var userProperty = typeof(CookbookOrderSubmission).GetProperty(nameof(CookbookOrderSubmission.UserDetails));
        userProperty?.SetValue(_submission, null);
        return this;
    }

    public CookbookOrderSubmission Build() => _submission;
}