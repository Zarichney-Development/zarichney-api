using Serilog;
using Zarichney.Cookbook.Models;
using Zarichney.Cookbook.Services;
using Zarichney.Services;
using ILogger = Serilog.ILogger;

namespace Zarichney.Cookbook.Repositories;

public interface IOrderRepository
{
  Task<CookbookOrder?> GetOrder(string orderId);
  void AddUpdateOrderAsync(CookbookOrder order);
}

public class OrderFileRepository(
  OrderConfig config,
  IFileService fileService) : IOrderRepository
{
  private readonly ILogger _log = Log.ForContext<OrderFileRepository>();

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