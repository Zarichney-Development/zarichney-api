using System.Text.Json.Serialization;
using Zarichney.Server.Cookbook.Customers;
using Zarichney.Server.Cookbook.Recipes;
using Zarichney.Server.Services;

namespace Zarichney.Server.Cookbook.Orders;

public enum OrderStatus
{
  Submitted,
  InProgress,
  Completed,
  Paid,
  Failed,
  AwaitingPayment
}

public class CookbookOrderSubmission
{
  public string Email { get; init; } = null!;
  public CookbookContent CookbookContent { get; init; } = null!;
  public CookbookDetails? CookbookDetails { get; protected init; }
  public UserDetails? UserDetails { get; protected init; }

  public string ToMarkdown()
    => $"""
        {CookbookContent}
        {CookbookDetails}
        {UserDetails}
        """.Trim();

  [JsonConstructor]
  public CookbookOrderSubmission()
  {
  }
}

public class CookbookOrder : CookbookOrderSubmission
{
  public string OrderId { get; init; } = null!;
  public List<string> RecipeList { get; init; } = null!;
  public List<SynthesizedRecipe> SynthesizedRecipes { get; init; } = [];
  public OrderStatus Status { get; set; } = OrderStatus.Submitted;
  public bool RequiresPayment { get; set; }
  public Customer Customer { get; set; } = null!;

  public CookbookOrder(Customer customer, CookbookOrderSubmission submission, List<string> recipeList)
  {
    Customer = customer;
    OrderId = Utils.GenerateId();
    Email = submission.Email;
    CookbookContent = submission.CookbookContent;
    CookbookDetails = submission.CookbookDetails;
    UserDetails = submission.UserDetails;
    RecipeList = recipeList;
    RequiresPayment = recipeList.Count > customer.AvailableRecipes;
  }

  [JsonConstructor]
  public CookbookOrder()
  {
  }
}

public class CookbookContent
{
  public string? RecipeSpecificationType { get; set; }
  public List<string>? SpecificRecipes { get; set; }
  public List<string>? GeneralMealTypes { get; set; }
  public int ExpectedRecipeCount { get; set; }

  public override string ToString()
    => this.ToMarkdown(Utils.SplitCamelCase(nameof(CookbookContent)));
}

public class CookbookDetails
{
  public string? Theme { get; set; }
  public string? PrimaryPurpose { get; set; }
  public List<string>? DesiredCuisines { get; set; }
  public string? CulturalExploration { get; set; }
  public string? NutritionalGuidance { get; set; }
  public string? RecipeModification { get; set; }
  public string? IngredientFlexibility { get; set; }
  public string? OverallStyle { get; set; }
  public string? Organization { get; set; }
  public List<string>? SpecialSections { get; set; }
  public string? Storytelling { get; set; }
  public List<string>? EducationalContent { get; set; }
  public List<string>? PracticalFeatures { get; set; }

  public override string ToString()
    => this.ToMarkdown(Utils.SplitCamelCase(nameof(CookbookDetails)));
}

public class UserDetails
{
  public List<string>? DietaryRestrictions { get; set; }
  public List<string>? Allergies { get; set; }
  public string? SkillLevel { get; set; }
  public List<string>? CookingGoals { get; set; }
  public string? TimeConstraints { get; set; }
  public string? HealthFocus { get; set; }
  public string? FamilyConsiderations { get; set; }
  public int ServingSize { get; set; }

  public override string ToString()
    => this.ToMarkdown(Utils.SplitCamelCase(nameof(UserDetails)));
}