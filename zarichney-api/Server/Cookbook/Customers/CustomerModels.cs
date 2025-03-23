namespace Zarichney.Server.Cookbook.Customers;

/// <summary>
/// Represents a user/customer of the Cookbook service.
/// Tracks how many recipe credits they have available (for free usage or otherwise).
/// </summary>
public class Customer
{
  /// <summary>
  /// Unique key for identifying the customer.
  /// Currently, using email as the primary key.
  /// </summary>
  public required string Email { get; init; }

  /// <summary>
  /// How many recipes the user can still generate without paying (e.g. free allotment or purchased credits).
  /// </summary>
  public int AvailableRecipes { get; set; }

  /// <summary>
  /// Total number of recipes that have been synthesized for this user across all orders (historical).
  /// </summary>
  public int LifetimeRecipesUsed { get; set; }

  /// <summary>
  /// Total number of recipes that have been purchased by this user (historical).
  /// </summary>
  public int LifetimePurchasedRecipes { get; set; }
}