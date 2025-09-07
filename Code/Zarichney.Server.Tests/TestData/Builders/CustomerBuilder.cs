using Zarichney.Cookbook.Customers;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Builder for creating Customer test data with flexible configuration
/// </summary>
public class CustomerBuilder
{
    private string _email = "test@example.com";
    private int _availableRecipes = 20;
    private int _lifetimeRecipesUsed = 0;
    private int _lifetimePurchasedRecipes = 0;

    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CustomerBuilder WithAvailableRecipes(int count)
    {
        _availableRecipes = count;
        return this;
    }

    public CustomerBuilder WithLifetimeRecipesUsed(int count)
    {
        _lifetimeRecipesUsed = count;
        return this;
    }

    public CustomerBuilder WithLifetimePurchasedRecipes(int count)
    {
        _lifetimePurchasedRecipes = count;
        return this;
    }

    public CustomerBuilder WithDefaultValues()
    {
        _email = "test@example.com";
        _availableRecipes = 20;
        _lifetimeRecipesUsed = 0;
        _lifetimePurchasedRecipes = 0;
        return this;
    }

    public CustomerBuilder WithNewCustomerDefaults()
    {
        return WithDefaultValues()
            .WithAvailableRecipes(20)
            .WithLifetimeRecipesUsed(0)
            .WithLifetimePurchasedRecipes(0);
    }

    public CustomerBuilder WithExistingCustomer(int used = 5, int purchased = 10)
    {
        // Only set specific fields, don't override email 
        _availableRecipes = 15;
        _lifetimeRecipesUsed = used;
        _lifetimePurchasedRecipes = purchased;
        return this;
    }

    public CustomerBuilder WithNoRemainingRecipes()
    {
        return WithAvailableRecipes(0);
    }

    public Customer Build()
    {
        return new Customer
        {
            Email = _email,
            AvailableRecipes = _availableRecipes,
            LifetimeRecipesUsed = _lifetimeRecipesUsed,
            LifetimePurchasedRecipes = _lifetimePurchasedRecipes
        };
    }
}