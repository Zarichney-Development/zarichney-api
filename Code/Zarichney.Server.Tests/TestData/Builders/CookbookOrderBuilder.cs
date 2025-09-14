using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;
using System.Reflection;

namespace Zarichney.Tests.TestData.Builders;

public class CookbookOrderBuilder
{
    private readonly CookbookOrder _order;

    public CookbookOrderBuilder()
    {
        var customer = new CustomerBuilder().Build();
        var submission = new CookbookOrderSubmissionBuilder().Build();
        var recipeList = new List<string> { "Pasta Carbonara", "Caesar Salad", "Chocolate Cake" };
        
        _order = new CookbookOrder(customer, submission, recipeList)
        {
            Status = OrderStatus.Submitted,
            SynthesizedRecipes = new List<SynthesizedRecipe>(),
            LlmConversationId = "conv_" + Guid.NewGuid().ToString("N")
        };
    }

    public CookbookOrderBuilder WithOrderId(string orderId)
    {
        var property = typeof(CookbookOrder).GetProperty(nameof(CookbookOrder.OrderId));
        property?.SetValue(_order, orderId);
        return this;
    }

    public CookbookOrderBuilder WithEmail(string email)
    {
        _order.Email = email;
        return this;
    }

    public CookbookOrderBuilder WithStatus(OrderStatus status)
    {
        _order.Status = status;
        return this;
    }

    public CookbookOrderBuilder WithCustomer(Customer customer)
    {
        _order.Customer = customer;
        return this;
    }

    public CookbookOrderBuilder WithRecipeList(List<string> recipeList)
    {
        var property = typeof(CookbookOrder).GetProperty(nameof(CookbookOrder.RecipeList));
        property?.SetValue(_order, recipeList);
        return this;
    }

    public CookbookOrderBuilder WithSynthesizedRecipes(List<SynthesizedRecipe> synthesizedRecipes)
    {
        var property = typeof(CookbookOrder).GetProperty(nameof(CookbookOrder.SynthesizedRecipes));
        property?.SetValue(_order, synthesizedRecipes);
        return this;
    }

    public CookbookOrderBuilder WithRequiresPayment(bool requiresPayment)
    {
        _order.RequiresPayment = requiresPayment;
        return this;
    }

    public CookbookOrderBuilder WithLlmConversationId(string conversationId)
    {
        _order.LlmConversationId = conversationId;
        return this;
    }

    public CookbookOrderBuilder WithCookbookContent(CookbookContent content)
    {
        var property = typeof(CookbookOrder).GetProperty(nameof(CookbookOrder.CookbookContent));
        property?.SetValue(_order, content);
        return this;
    }

    public CookbookOrderBuilder WithCookbookDetails(CookbookDetails details)
    {
        var property = typeof(CookbookOrder).GetProperty(nameof(CookbookOrder.CookbookDetails));
        property?.SetValue(_order, details);
        return this;
    }

    public CookbookOrderBuilder WithUserDetails(UserDetails details)
    {
        var property = typeof(CookbookOrder).GetProperty(nameof(CookbookOrder.UserDetails));
        property?.SetValue(_order, details);
        return this;
    }

    public CookbookOrderBuilder AsCompleted()
    {
        _order.Status = OrderStatus.Completed;
        _order.RequiresPayment = false;
        
        // Ensure we have synthesized recipes matching the recipe list
        if (_order.SynthesizedRecipes.Count == 0 && _order.RecipeList.Count > 0)
        {
            var synthesizedRecipes = _order.RecipeList.Select(recipeName =>
                new SynthesizedRecipeBuilder()
                    .WithTitle(recipeName)
                    .Build()
            ).ToList();
            
            var property = typeof(CookbookOrder).GetProperty(nameof(CookbookOrder.SynthesizedRecipes));
            property?.SetValue(_order, synthesizedRecipes);
        }
        
        return this;
    }

    public CookbookOrderBuilder AsAwaitingPayment()
    {
        _order.Status = OrderStatus.AwaitingPayment;
        _order.RequiresPayment = true;
        return this;
    }

    public CookbookOrderBuilder AsFailed()
    {
        _order.Status = OrderStatus.Failed;
        return this;
    }

    public CookbookOrderBuilder AsInProgress()
    {
        _order.Status = OrderStatus.InProgress;
        return this;
    }

    public CookbookOrder Build() => _order;
}