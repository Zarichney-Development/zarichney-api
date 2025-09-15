using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Test data builder for CookbookOrder objects.
/// Provides a fluent interface for creating order test data with sensible defaults.
/// </summary>
public class CookbookOrderBuilder
{
    private string _orderId = Utils.GenerateId();
    private string _email = "test@example.com";
    private List<string> _recipeList = ["Recipe 1", "Recipe 2", "Recipe 3"];
    private List<SynthesizedRecipe> _synthesizedRecipes = [];
    private OrderStatus _status = OrderStatus.Submitted;
    private bool _requiresPayment = false;
    private Customer _customer = new CustomerBuilder().Build();
    private string? _llmConversationId = "test-conversation-id";
    private CookbookContent _cookbookContent = new()
    {
        RecipeSpecificationType = "Specific",
        SpecificRecipes = ["Recipe 1", "Recipe 2"],
        ExpectedRecipeCount = 3
    };
    private CookbookDetails? _cookbookDetails = null;
    private UserDetails? _userDetails = null;

    /// <summary>
    /// Sets the order ID.
    /// </summary>
    public CookbookOrderBuilder WithOrderId(string orderId)
    {
        _orderId = orderId;
        return this;
    }

    /// <summary>
    /// Sets the email address for the order.
    /// </summary>
    public CookbookOrderBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the list of recipe names requested in the order.
    /// </summary>
    public CookbookOrderBuilder WithRecipeList(params string[] recipes)
    {
        _recipeList = recipes.ToList();
        return this;
    }

    /// <summary>
    /// Adds synthesized recipes to the order.
    /// </summary>
    public CookbookOrderBuilder WithSynthesizedRecipes(params SynthesizedRecipe[] recipes)
    {
        _synthesizedRecipes = recipes.ToList();
        return this;
    }

    /// <summary>
    /// Sets the order status.
    /// </summary>
    public CookbookOrderBuilder WithStatus(OrderStatus status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets whether the order requires payment.
    /// </summary>
    public CookbookOrderBuilder WithRequiresPayment(bool requiresPayment)
    {
        _requiresPayment = requiresPayment;
        return this;
    }

    /// <summary>
    /// Sets the customer for the order.
    /// </summary>
    public CookbookOrderBuilder WithCustomer(Customer customer)
    {
        _customer = customer;
        return this;
    }

    /// <summary>
    /// Sets the LLM conversation ID.
    /// </summary>
    public CookbookOrderBuilder WithLlmConversationId(string? conversationId)
    {
        _llmConversationId = conversationId;
        return this;
    }

    /// <summary>
    /// Sets the cookbook content details.
    /// </summary>
    public CookbookOrderBuilder WithCookbookContent(CookbookContent content)
    {
        _cookbookContent = content;
        return this;
    }

    /// <summary>
    /// Sets the cookbook details.
    /// </summary>
    public CookbookOrderBuilder WithCookbookDetails(CookbookDetails? details)
    {
        _cookbookDetails = details;
        return this;
    }

    /// <summary>
    /// Sets the user details.
    /// </summary>
    public CookbookOrderBuilder WithUserDetails(UserDetails? details)
    {
        _userDetails = details;
        return this;
    }

    /// <summary>
    /// Configures the order as a new order with default values.
    /// </summary>
    public CookbookOrderBuilder AsNewOrder()
    {
        _status = OrderStatus.Submitted;
        _synthesizedRecipes = [];
        _requiresPayment = false;
        return this;
    }

    /// <summary>
    /// Configures the order as in progress with some recipes synthesized.
    /// </summary>
    public CookbookOrderBuilder AsInProgressOrder()
    {
        _status = OrderStatus.InProgress;
        _synthesizedRecipes = [
            new SynthesizedRecipeBuilder().WithTitle("Recipe 1").Build()
        ];
        return this;
    }

    /// <summary>
    /// Configures the order as completed with all recipes synthesized.
    /// </summary>
    public CookbookOrderBuilder AsCompletedOrder()
    {
        _status = OrderStatus.Completed;
        _synthesizedRecipes = _recipeList.Select(r => 
            new SynthesizedRecipeBuilder().WithTitle(r).Build()
        ).ToList();
        _requiresPayment = false;
        return this;
    }

    /// <summary>
    /// Configures the order as awaiting payment.
    /// </summary>
    public CookbookOrderBuilder AsAwaitingPaymentOrder()
    {
        _status = OrderStatus.AwaitingPayment;
        _requiresPayment = true;
        _customer = new CustomerBuilder().WithNoCredits().Build();
        return this;
    }

    /// <summary>
    /// Configures the order as failed.
    /// </summary>
    public CookbookOrderBuilder AsFailedOrder()
    {
        _status = OrderStatus.Failed;
        return this;
    }

    /// <summary>
    /// Builds and returns the CookbookOrder instance with the configured values.
    /// </summary>
    public CookbookOrder Build()
    {
        var order = new CookbookOrder
        {
            OrderId = _orderId,
            Email = _email,
            RecipeList = _recipeList,
            SynthesizedRecipes = _synthesizedRecipes,
            Status = _status,
            RequiresPayment = _requiresPayment,
            Customer = _customer,
            LlmConversationId = _llmConversationId,
            CookbookContent = _cookbookContent
        };
        
        // Use reflection to set protected properties
        var type = typeof(CookbookOrder);
        
        if (_cookbookDetails != null)
        {
            var detailsProperty = type.GetProperty("CookbookDetails");
            detailsProperty?.SetValue(order, _cookbookDetails);
        }
        
        if (_userDetails != null)
        {
            var userProperty = type.GetProperty("UserDetails");
            userProperty?.SetValue(order, _userDetails);
        }
        
        return order;
    }

    /// <summary>
    /// Creates multiple orders with different statuses.
    /// </summary>
    public static List<CookbookOrder> BuildMultipleWithDifferentStatuses()
    {
        return
        [
            new CookbookOrderBuilder().AsNewOrder().Build(),
            new CookbookOrderBuilder().AsInProgressOrder().Build(),
            new CookbookOrderBuilder().AsCompletedOrder().Build(),
            new CookbookOrderBuilder().AsAwaitingPaymentOrder().Build()
        ];
    }
}