using Zarichney.Config;

namespace Zarichney.Cookbook.Customers;

public interface ICustomerService
{
  /// <summary>
  /// Retrieve or create a Customer record by email.
  /// If none exists, initialize one with the default allotment from config.
  /// </summary>
  Task<Customer> GetOrCreateCustomer(string email);

  /// <summary>
  /// Decrement the customer's available recipes after usage
  /// (e.g., if they processed N recipes from an order).
  /// </summary>
  void DecrementRecipes(Customer customer, int numberOfRecipes);

  /// <summary>
  /// Manual save in case you do partial updates or manually add credits, etc.
  /// </summary>
  void SaveCustomer(Customer customer);
}

/// <summary>
/// Configuration for new customers:
/// how many free recipes (credits) they start with, etc.
/// </summary>
public class CustomerConfig : IConfig
{
  /// <summary>
  /// How many free recipe credits each new customer starts with
  /// (e.g. "20 free recipes" or "5 free recipes").
  /// </summary>
  public int InitialFreeRecipes { get; set; } = 20;
  public string OutputDirectory { get; init; } = "Data\\Customers";
}

public class CustomerService(
  ICustomerRepository customerRepository,
  CustomerConfig config,
  ILogger<CustomerService> logger
)
  : ICustomerService
{
  public async Task<Customer> GetOrCreateCustomer(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
      throw new ArgumentException("Email cannot be empty.", nameof(email));

    var existing = await customerRepository.GetCustomerByEmail(email);
    if (existing != null)
    {
      return existing;
    }

    // Create a new record
    var customer = new Customer
    {
      Email = email,
      AvailableRecipes = config.InitialFreeRecipes
    };

    logger.LogInformation(
      "Creating new Customer record for {Email}; initial free recipes = {Count}",
      email, customer.AvailableRecipes);

    // Immediately save brand-new customers to the repository
    // (so that we have them on disk even if the server restarts)
    customerRepository.SaveCustomer(customer);

    return customer;
  }

  public void DecrementRecipes(Customer customer, int numberOfRecipes)
  {
    ArgumentNullException.ThrowIfNull(customer);

    if (numberOfRecipes <= 0)
    {
      return; // nothing to do
    }

    var oldVal = customer.AvailableRecipes;
    customer.AvailableRecipes = Math.Max(0, oldVal - numberOfRecipes);
    customer.LifetimeRecipesUsed += numberOfRecipes;

    logger.LogInformation(
      "Customer {Email} used {UsedCount} recipes. OldVal={OldVal}, NewVal={NewVal}. Lifetime={Lifetime}",
      customer.Email, numberOfRecipes, oldVal, customer.AvailableRecipes, customer.LifetimeRecipesUsed
    );
  }

  public void SaveCustomer(Customer customer)
  {
    customerRepository.SaveCustomer(customer);
  }
}