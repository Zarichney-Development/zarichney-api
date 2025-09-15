using Zarichney.Cookbook.Orders;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Test data builder for CookbookOrderSubmission objects.
/// Provides a fluent interface for creating order submission test data.
/// </summary>
public class CookbookOrderSubmissionBuilder
{
    private string _email = "test@example.com";
    private CookbookContent _cookbookContent = new()
    {
        RecipeSpecificationType = "Specific",
        SpecificRecipes = ["Pasta Carbonara", "Chicken Tikka Masala"],
        ExpectedRecipeCount = 2
    };
    private CookbookDetails? _cookbookDetails = null;
    private UserDetails? _userDetails = null;

    /// <summary>
    /// Sets the email address for the submission.
    /// </summary>
    public CookbookOrderSubmissionBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the cookbook content for the submission.
    /// </summary>
    public CookbookOrderSubmissionBuilder WithCookbookContent(CookbookContent content)
    {
        _cookbookContent = content;
        return this;
    }

    /// <summary>
    /// Sets the cookbook details for the submission.
    /// </summary>
    public CookbookOrderSubmissionBuilder WithCookbookDetails(CookbookDetails? details)
    {
        _cookbookDetails = details;
        return this;
    }

    /// <summary>
    /// Sets the user details for the submission.
    /// </summary>
    public CookbookOrderSubmissionBuilder WithUserDetails(UserDetails? details)
    {
        _userDetails = details;
        return this;
    }

    /// <summary>
    /// Configures the submission with specific recipe requests.
    /// </summary>
    public CookbookOrderSubmissionBuilder WithSpecificRecipes(params string[] recipes)
    {
        _cookbookContent = new CookbookContent
        {
            RecipeSpecificationType = "Specific",
            SpecificRecipes = recipes.ToList(),
            ExpectedRecipeCount = recipes.Length
        };
        return this;
    }

    /// <summary>
    /// Configures the submission with general meal type requests.
    /// </summary>
    public CookbookOrderSubmissionBuilder WithGeneralMealTypes(params string[] mealTypes)
    {
        _cookbookContent = new CookbookContent
        {
            RecipeSpecificationType = "General",
            GeneralMealTypes = mealTypes.ToList(),
            ExpectedRecipeCount = mealTypes.Length
        };
        return this;
    }

    /// <summary>
    /// Configures the submission as a basic request with minimal details.
    /// </summary>
    public CookbookOrderSubmissionBuilder AsBasicSubmission()
    {
        _cookbookDetails = null;
        _userDetails = null;
        return this;
    }

    /// <summary>
    /// Configures the submission with full details including dietary restrictions.
    /// </summary>
    public CookbookOrderSubmissionBuilder AsDetailedSubmission()
    {
        _cookbookDetails = new CookbookDetails
        {
            Theme = "Mediterranean Summer",
            PrimaryPurpose = "Family Meals",
            DesiredCuisines = ["Italian", "Greek", "Spanish"],
            CulturalExploration = "Authentic regional dishes",
            NutritionalGuidance = "Balanced, heart-healthy",
            RecipeModification = "Allow substitutions",
            IngredientFlexibility = "High",
            OverallStyle = "Casual and approachable",
            Organization = "By meal type",
            SpecialSections = ["Quick weeknight meals", "Sunday dinners"],
            Storytelling = "Include origin stories",
            EducationalContent = ["Technique tips", "Ingredient guides"],
            PracticalFeatures = ["Shopping lists", "Prep timeline"]
        };

        _userDetails = new UserDetails
        {
            DietaryRestrictions = ["Vegetarian options needed"],
            Allergies = ["Nuts"],
            SkillLevel = "Intermediate",
            CookingGoals = ["Expand repertoire", "Cook healthier"],
            TimeConstraints = "30-45 minutes weekdays",
            HealthFocus = "Mediterranean diet",
            FamilyConsiderations = "Kid-friendly options",
            ServingSize = 4
        };

        return this;
    }

    /// <summary>
    /// Configures the submission with dietary restrictions.
    /// </summary>
    public CookbookOrderSubmissionBuilder WithDietaryRestrictions(params string[] restrictions)
    {
        _userDetails ??= new UserDetails();
        _userDetails.DietaryRestrictions = restrictions.ToList();
        return this;
    }

    /// <summary>
    /// Configures the submission with allergies.
    /// </summary>
    public CookbookOrderSubmissionBuilder WithAllergies(params string[] allergies)
    {
        _userDetails ??= new UserDetails();
        _userDetails.Allergies = allergies.ToList();
        return this;
    }

    /// <summary>
    /// Builds and returns the CookbookOrderSubmission instance.
    /// </summary>
    public CookbookOrderSubmission Build()
    {
        // Use reflection to set protected init properties
        var submission = new CookbookOrderSubmission
        {
            Email = _email,
            CookbookContent = _cookbookContent
        };

        // Set protected properties through reflection if needed
        var type = typeof(CookbookOrderSubmission);
        
        if (_cookbookDetails != null)
        {
            var detailsProperty = type.GetProperty("CookbookDetails");
            detailsProperty?.SetValue(submission, _cookbookDetails);
        }

        if (_userDetails != null)
        {
            var userProperty = type.GetProperty("UserDetails");
            userProperty?.SetValue(submission, _userDetails);
        }

        return submission;
    }
}