using Zarichney.Services;

namespace Zarichney.Cookbook.Orders;

public interface IOrderRepository
{
  Task<CookbookOrder?> GetOrder(string orderId);
  void AddUpdateOrderAsync(CookbookOrder order);
}

public class OrderFileRepository(
  OrderConfig config,
  IFileService fileService) : IOrderRepository
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
}