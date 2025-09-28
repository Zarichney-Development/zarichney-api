using Zarichney.Cookbook.Customers;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Test data builder for Customer objects.
/// Provides a fluent interface for creating customer test data with sensible defaults.
/// </summary>
public class CustomerBuilder
{
  private string _email = "test@example.com";
  private int _availableRecipes = 20;
  private int _lifetimeRecipesUsed = 0;
  private int _lifetimePurchasedRecipes = 0;

  /// <summary>
  /// Sets the customer email address.
  /// </summary>
  /// <param name="email">The email address</param>
  /// <returns>The builder instance for method chaining</returns>
  public CustomerBuilder WithEmail(string email)
  {
    _email = email;
    return this;
  }

  /// <summary>
  /// Sets the number of available recipes for the customer.
  /// </summary>
  /// <param name="availableRecipes">Number of available recipes</param>
  /// <returns>The builder instance for method chaining</returns>
  public CustomerBuilder WithAvailableRecipes(int availableRecipes)
  {
    _availableRecipes = availableRecipes;
    return this;
  }

  /// <summary>
  /// Sets the lifetime recipes used count.
  /// </summary>
  /// <param name="lifetimeRecipesUsed">Number of recipes used historically</param>
  /// <returns>The builder instance for method chaining</returns>
  public CustomerBuilder WithLifetimeRecipesUsed(int lifetimeRecipesUsed)
  {
    _lifetimeRecipesUsed = lifetimeRecipesUsed;
    return this;
  }

  /// <summary>
  /// Sets the lifetime purchased recipes count.
  /// </summary>
  /// <param name="lifetimePurchasedRecipes">Number of recipes purchased historically</param>
  /// <returns>The builder instance for method chaining</returns>
  public CustomerBuilder WithLifetimePurchasedRecipes(int lifetimePurchasedRecipes)
  {
    _lifetimePurchasedRecipes = lifetimePurchasedRecipes;
    return this;
  }

  /// <summary>
  /// Configures the customer as a new customer with default initial values.
  /// </summary>
  /// <param name="email">The customer's email address</param>
  /// <param name="initialFreeRecipes">Number of initial free recipes (default: 20)</param>
  /// <returns>The builder instance for method chaining</returns>
  public CustomerBuilder AsNewCustomer(string email = "new@example.com", int initialFreeRecipes = 20)
  {
    _email = email;
    _availableRecipes = initialFreeRecipes;
    _lifetimeRecipesUsed = 0;
    _lifetimePurchasedRecipes = 0;
    return this;
  }

  /// <summary>
  /// Configures the customer as an existing customer with some usage history.
  /// </summary>
  /// <param name="email">The customer's email address</param>
  /// <param name="availableRecipes">Number of recipes still available</param>
  /// <param name="recipesUsed">Number of recipes used historically</param>
  /// <returns>The builder instance for method chaining</returns>
  public CustomerBuilder AsExistingCustomer(string email = "existing@example.com",
      int availableRecipes = 15, int recipesUsed = 5)
  {
    _email = email;
    _availableRecipes = availableRecipes;
    _lifetimeRecipesUsed = recipesUsed;
    _lifetimePurchasedRecipes = 0;
    return this;
  }

  /// <summary>
  /// Configures the customer as a paying customer with purchase history.
  /// </summary>
  /// <param name="email">The customer's email address</param>
  /// <param name="availableRecipes">Number of recipes still available</param>
  /// <param name="recipesUsed">Number of recipes used historically</param>
  /// <param name="recipesPurchased">Number of recipes purchased</param>
  /// <returns>The builder instance for method chaining</returns>
  public CustomerBuilder AsPayingCustomer(string email = "paying@example.com",
      int availableRecipes = 25, int recipesUsed = 10, int recipesPurchased = 15)
  {
    _email = email;
    _availableRecipes = availableRecipes;
    _lifetimeRecipesUsed = recipesUsed;
    _lifetimePurchasedRecipes = recipesPurchased;
    return this;
  }

  /// <summary>
  /// Configures the customer as having no available recipes (needs to purchase more).
  /// </summary>
  /// <param name="email">The customer's email address</param>
  /// <param name="recipesUsed">Number of recipes used historically</param>
  /// <returns>The builder instance for method chaining</returns>
  public CustomerBuilder WithNoCredits(string email = "nocredits@example.com", int recipesUsed = 20)
  {
    _email = email;
    _availableRecipes = 0;
    _lifetimeRecipesUsed = recipesUsed;
    _lifetimePurchasedRecipes = 0;
    return this;
  }

  /// <summary>
  /// Builds and returns the Customer instance with the configured values.
  /// </summary>
  /// <returns>A new Customer instance</returns>
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

  /// <summary>
  /// Creates multiple customers with sequential email addresses.
  /// </summary>
  /// <param name="count">Number of customers to create</param>
  /// <param name="baseEmail">Base email pattern (will have numbers appended)</param>
  /// <returns>List of customers</returns>
  public static List<Customer> BuildMultiple(int count, string baseEmail = "customer{0}@example.com")
  {
    List<Customer> customers = [];
    for (int i = 1; i <= count; i++)
    {
      customers.Add(new CustomerBuilder()
          .WithEmail(string.Format(baseEmail, i))
          .Build());
    }
    return customers;
  }
}
