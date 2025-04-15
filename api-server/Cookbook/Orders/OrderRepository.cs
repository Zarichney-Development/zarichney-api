using System.Text.Json;
using Zarichney.Services.FileSystem;

namespace Zarichney.Cookbook.Orders;

public interface IOrderRepository
{
  
  /// <summary>
  /// Retrieves the order from the file system. Expected to only be called from the session manager
  /// </summary>
  /// <param name="orderId"></param>
  /// <returns></returns>
  Task<CookbookOrder?> GetOrder(string orderId);
  
  /// <summary>
  /// Persist the data to the file system. Expected to only be called from the session manager
  /// </summary>
  /// <param name="order"></param>
  void AddUpdateOrderAsync(CookbookOrder order);
  
  /// <summary>
  /// Creates the markdown file for the given recipe
  /// </summary>
  /// <param name="orderId">Used for the directory path</param>
  /// <param name="recipeTitle">Used for the file name</param>
  /// <param name="recipeMarkdown">The file content</param>
  void SaveRecipe(string orderId, string recipeTitle, string recipeMarkdown);
  
  /// <summary>
  /// Creates the markdown file
  /// </summary>
  /// <param name="orderId">Used for the directory path</param>
  /// <param name="cookbookMarkdown">The file content</param>
  void SaveCookbook(string orderId, string cookbookMarkdown);
  
  /// <summary>
  /// Reads the cookbook PDF from the file system
  /// </summary>
  /// <param name="orderId"></param>
  /// <returns></returns>
  Task<byte[]?> GetCookbook(string orderId);
  
  /// <summary>
  /// Synchronously saves a cookbook PDF to the file system. Waits for completion.
  /// </summary>
  /// <param name="orderId">Used for the directory path</param>
  /// <param name="pdf">The file content</param>
  Task SaveCookbook(string orderId, byte[] pdf);
  
  /// <summary>
  /// Asynchronously (by creating a background task) saves a cookbook PDF to the file system and returns immediately to not block the calling thread.
  /// </summary>
  /// <param name="orderId">Used for the directory path</param>
  /// <param name="pdf">The file content</param>
  void SaveCookbookAsync(string orderId, byte[] pdf);
  
  /// <summary>
  /// Gets all pending orders for a customer that are awaiting payment
  /// </summary>
  /// <param name="email">The customer's email</param>
  /// <returns>A list of pending orders</returns>
  Task<List<CookbookOrder>> GetPendingOrdersForCustomer(string email);
}

public class OrderFileRepository(
  OrderConfig config,
  IFileService fileService,
  ILogger<OrderFileRepository> logger
) : IOrderRepository
{
  public async Task<CookbookOrder?> GetOrder(string orderId)
  {
    var order = await fileService.ReadFromFile<CookbookOrder?>(
      Path.Combine(config.OutputDirectory, orderId),
      "Order"
    );

    return order;
  }

  public void AddUpdateOrderAsync(CookbookOrder order)
  {
    fileService.WriteToFileAsync(
      Path.Combine(config.OutputDirectory, order.OrderId),
      "Order",
      order
    );
  }

  public void SaveRecipe(string orderId, string recipeTitle, string recipeMarkdown)
  {
    fileService.WriteToFileAsync(
      Path.Combine(config.OutputDirectory, orderId, "recipes"),
      recipeTitle,
      recipeMarkdown,
      "md"
    );
  }

  public void SaveCookbook(string orderId, string cookbookMarkdown)
  {
    fileService.WriteToFileAsync(
      Path.Combine(config.OutputDirectory, orderId),
      "Cookbook",
      cookbookMarkdown,
      "md"
    );
  }

  public async Task<byte[]?> GetCookbook(string orderId)
  {
    return await fileService.ReadFromFile<byte[]>(
      Path.Combine(config.OutputDirectory, orderId),
      "Cookbook",
      "pdf"
    );
  }

  public async Task SaveCookbook(string orderId, byte[] pdf)
  {
    // Waits for completion
    await fileService.WriteToFile(
      Path.Combine(config.OutputDirectory, orderId),
      "Cookbook",
      pdf,
      "pdf"
    );
  }

  public void SaveCookbookAsync(string orderId, byte[] pdf)
  {
    // Send to background queue
    fileService.WriteToFileAsync(
      Path.Combine(config.OutputDirectory, orderId),
      "Cookbook",
      pdf,
      "pdf"
    );
  }
  
  public async Task<List<CookbookOrder>> GetPendingOrdersForCustomer(string email)
  {
    var pendingOrders = new List<CookbookOrder>();
    
    try
    {
      // Get all subdirectories in the orders directory (each is an order)
      var baseDir = Path.Combine(config.OutputDirectory);
      var orderDirs = Directory.GetDirectories(baseDir);
      
      logger.LogInformation("Found {Count} order directories to check for customer {Email}", 
        orderDirs.Length, email);
      
      foreach (var orderDir in orderDirs)
      {
        var orderFilePath = Path.Combine(orderDir, "Order.json");
        
        if (File.Exists(orderFilePath))
        {
          try
          {
            // Read the order file directly to check if it matches our criteria
            var orderJson = await File.ReadAllTextAsync(orderFilePath);
            var order = JsonSerializer.Deserialize<CookbookOrder>(orderJson);
            
            if (order != null && 
                string.Equals(order.Email, email, StringComparison.OrdinalIgnoreCase) &&
                (order.Status == OrderStatus.AwaitingPayment || order.RequiresPayment))
            {
              pendingOrders.Add(order);
            }
          }
          catch (Exception ex)
          {
            logger.LogWarning(ex, "Error reading order file {FilePath}", orderFilePath);
          }
        }
      }
      
      logger.LogInformation("Found {Count} pending orders for customer {Email}", 
        pendingOrders.Count, email);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error getting pending orders for customer {Email}", email);
    }
    
    return pendingOrders
      .OrderByDescending(o => o.SynthesizedRecipes.Count) // Orders with more progress first
      .ToList();
  }
}