using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;

namespace Zarichney.Tests.TestData.Builders;

public class CookbookOrderBuilder
{
  private string? _orderId;
  private string _email;
  private Customer _customer;
  private CookbookOrderSubmission _submission;
  private CookbookContent _content;
  private CookbookDetails? _details;
  private UserDetails? _user;
  private List<string> _recipeList;
  private List<SynthesizedRecipe> _synthesizedRecipes = new();
  private OrderStatus _status = OrderStatus.Submitted;
  private bool _requiresPayment = false;
  private string? _llmConversationId;

  public CookbookOrderBuilder()
  {
    _customer = new CustomerBuilder().Build();
    var submissionBuilder = new CookbookOrderSubmissionBuilder();
    _submission = submissionBuilder.Build();
    _email = _submission.Email;
    _content = _submission.CookbookContent;
    _details = null; // cannot be set from tests (protected init)
    _user = null;    // cannot be set from tests (protected init)
    _recipeList = new List<string> { "Pasta Carbonara", "Caesar Salad", "Chocolate Cake" };
    _llmConversationId = "conv_" + Guid.NewGuid().ToString("N");
  }

  public CookbookOrderBuilder WithOrderId(string orderId)
  {
    _orderId = orderId;
    return this;
  }

  public CookbookOrderBuilder WithEmail(string email)
  {
    _email = email;
    return this;
  }

  public CookbookOrderBuilder WithStatus(OrderStatus status)
  {
    _status = status;
    return this;
  }

  public CookbookOrderBuilder WithCustomer(Customer customer)
  {
    _customer = customer;
    return this;
  }

  public CookbookOrderBuilder WithRecipeList(List<string> recipeList)
  {
    _recipeList = recipeList;
    return this;
  }

  // Convenience overload used by some tests
  public CookbookOrderBuilder WithRecipeList(params string[] recipes)
  {
    _recipeList = recipes.ToList();
    return this;
  }

  public CookbookOrderBuilder WithSynthesizedRecipes(List<SynthesizedRecipe> synthesizedRecipes)
  {
    _synthesizedRecipes = synthesizedRecipes;
    return this;
  }

  // Convenience overload used by some tests
  public CookbookOrderBuilder WithSynthesizedRecipes(params SynthesizedRecipe[] synthesizedRecipes)
  {
    _synthesizedRecipes = synthesizedRecipes.ToList();
    return this;
  }

  public CookbookOrderBuilder WithRequiresPayment(bool requiresPayment)
  {
    _requiresPayment = requiresPayment;
    return this;
  }

  public CookbookOrderBuilder WithLlmConversationId(string conversationId)
  {
    _llmConversationId = conversationId;
    return this;
  }

  public CookbookOrderBuilder WithCookbookContent(CookbookContent content)
  {
    _content = content;
    return this;
  }

  public CookbookOrderBuilder WithCookbookDetails(CookbookDetails details)
  {
    _details = details; // not applied (protected init), kept for API symmetry
    return this;
  }

  public CookbookOrderBuilder WithUserDetails(UserDetails details)
  {
    _user = details; // not applied (protected init), kept for API symmetry
    return this;
  }

  public CookbookOrderBuilder AsCompleted()
  {
    _status = OrderStatus.Completed;
    _requiresPayment = false;

    if (_synthesizedRecipes.Count == 0 && _recipeList.Count > 0)
    {
      _synthesizedRecipes = _recipeList.Select(name =>
          new SynthesizedRecipeBuilder().WithTitle(name).Build()).ToList();
    }

    return this;
  }

  public CookbookOrderBuilder AsAwaitingPayment()
  {
    _status = OrderStatus.AwaitingPayment;
    _requiresPayment = true;
    return this;
  }

  public CookbookOrderBuilder AsFailed()
  {
    _status = OrderStatus.Failed;
    return this;
  }

  public CookbookOrderBuilder AsInProgress()
  {
    _status = OrderStatus.InProgress;
    return this;
  }

  public CookbookOrder Build()
  {
    // Rebuild submission with allowed fields
    var submission = new CookbookOrderSubmission
    {
      Email = _email,
      CookbookContent = _content
      // CookbookDetails/UserDetails cannot be set from tests
    };

    if (_orderId != null)
    {
      return new CookbookOrder(_customer, submission, _recipeList)
      {
        OrderId = _orderId,
        SynthesizedRecipes = _synthesizedRecipes,
        Status = _status,
        RequiresPayment = _requiresPayment,
        LlmConversationId = _llmConversationId
      };
    }
    else
    {
      return new CookbookOrder(_customer, submission, _recipeList)
      {
        SynthesizedRecipes = _synthesizedRecipes,
        Status = _status,
        RequiresPayment = _requiresPayment,
        LlmConversationId = _llmConversationId
      };
    }
  }
}
