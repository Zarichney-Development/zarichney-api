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
    var path = Path.Combine(config.OutputDirectory, orderId);
    
    // TODO: enhance to wait in case this file is in the pipeline to be written
    // problem at the moment is that the call to file service doesnt signal when the file has exited the queue and completed
    
    var order = await fileService.ReadFromFile<CookbookOrder?>(path, "Order");

    return order;
  }

  public void AddUpdateOrderAsync(CookbookOrder order)
  {
    var path = Path.Combine(config.OutputDirectory, order.OrderId);
    fileService.WriteToFileAsync(path, "Order", order);
  }
}