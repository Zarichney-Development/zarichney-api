using Zarichney.Server.Services;

namespace Zarichney.Server.Cookbook.Orders;

public static class OrderServiceExtensions
{
    /// <summary>
    /// Queue an order for processing in the background
    /// </summary>
    public static void QueueOrderProcessing(this IOrderService orderService, string orderId)
    {
        if (string.IsNullOrEmpty(orderId))
        {
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
        }
        
        // Get the background worker through DI
        if (orderService is not IBackgroundWorkerProvider scopeProvider)
        {
            throw new InvalidOperationException("OrderService must implement IBackgroundWorkerProvider");
        }
        
        var backgroundService = scopeProvider.BackgroundWorker;
        if (backgroundService == null)
        {
            throw new InvalidOperationException("BackgroundWorker service is not available");
        }
        
        // Queue the cookbook generation task
        backgroundService.QueueBackgroundWorkAsync(async (newScope, _) =>
        {
            var backgroundOrderService = newScope.GetService<IOrderService>();
            await backgroundOrderService.ProcessOrder(orderId);
        });
    }
}

/// <summary>
/// Interface to provide access to the background worker
/// </summary>
public interface IBackgroundWorkerProvider
{
    IBackgroundWorker BackgroundWorker { get; }
}