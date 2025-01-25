using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Services;

namespace Zarichney.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(
    ILogger<AdminController> logger,
    ICustomerRepository customerRepository,
    IFileService fileService,
    IOptions<OrderConfig> orderConfigOptions)
    : ControllerBase
{
    private readonly OrderConfig _orderConfig = orderConfigOptions.Value;

    /// <summary>
    /// One-time migration endpoint.
    /// Scans all order files to gather usage data per unique email,
    /// then creates or updates a Customer file for each email.
    /// </summary>
    [HttpPost("migrate-customers")]
    public async Task<IActionResult> MigrateCustomersFromOrders()
    {
        var ordersDirectory = _orderConfig.OutputDirectory; // e.g. "Data\\Orders" from appsettings

        if (!Directory.Exists(ordersDirectory))
        {
            var msg = $"Order directory not found: {ordersDirectory}";
            logger.LogError(msg);
            return BadRequest(msg);
        }

        // email => total synthesized recipes
        var emailToSynthesizedCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // 1) Enumerate all subdirectories, each presumably an Order folder
        //    e.g. Data/Orders/<orderId> 
        var orderDirectories = Directory.GetDirectories(ordersDirectory);

        foreach (var orderFolder in orderDirectories)
        {
            // We'll try reading the "Order.json" file first; if not found, try "Order" (no extension).
            var orderFileJson = Path.Combine(orderFolder, "Order.json");
            var orderFileNoExt = Path.Combine(orderFolder, "Order");

            // We'll pick whichever exists
            string? orderFileToUse = null;
            if (System.IO.File.Exists(orderFileJson))
            {
                orderFileToUse = orderFileJson;
            }
            else if (System.IO.File.Exists(orderFileNoExt))
            {
                orderFileToUse = orderFileNoExt;
            }

            if (orderFileToUse == null)
            {
                // no recognized order file in this folder
                continue;
            }

            // 2) Read the file content as a CookbookOrder
            //    We'll leverage your IFileService to keep it consistent with the rest of your code
            CookbookOrder? order;
            try
            {
                // The fileService method requires separate "directory", "fileName", "ext".
                // So we can replicate that usage:
                var directoryPart = Path.GetDirectoryName(orderFileToUse);
                var filePart = Path.GetFileNameWithoutExtension(orderFileToUse);
                var extPart = Path.GetExtension(orderFileToUse).TrimStart('.'); // might be "json" or ""

                if (directoryPart == null)
                    continue;

                // If extPart is empty (i.e. ""), pass null or an empty string to readFromFile
                order = await fileService.ReadFromFile<CookbookOrder>(
                    directoryPart,
                    filePart,
                    string.IsNullOrWhiteSpace(extPart) ? null : extPart
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to parse order file for folder {OrderFolder}", orderFolder);
                continue;
            }

            // 3) If the order is valid, accumulate the synthesized count for that email

            var email = order.Email;
            var synthesizedCount = order.SynthesizedRecipes.Count;

            emailToSynthesizedCount.TryAdd(email, 0);
            emailToSynthesizedCount[email] += synthesizedCount;
        }

        // 4) Now create or update a Customer record for each unique email
        int createdCount = 0, updatedCount = 0;

        foreach (var (email, totalUsed) in emailToSynthesizedCount)
        {
            // Attempt to load an existing Customer
            var existingCustomer = await customerRepository.GetCustomerByEmail(email);
            if (existingCustomer == null)
            {
                // Create new
                var newCustomer = new Customer
                {
                    Email = email,
                    LifetimeRecipesUsed = totalUsed
                };

                // If the user used more than the "free limit" (20 here),
                // AvailableRecipes is effectively 0. Otherwise, it's (20 - used).
                var leftover = 20 - totalUsed;
                if (leftover < 0) leftover = 0;
                newCustomer.AvailableRecipes = leftover;

                customerRepository.SaveCustomer(newCustomer);
                createdCount++;
            }
            else
            {
                // Update existing
                // You might choose to *increase* existingCustomer.LifetimeRecipesUsed 
                // if they've done more than previously recorded, or just set it 
                // if you want to overwrite.
                existingCustomer.LifetimeRecipesUsed = totalUsed;

                // Recompute available if you want to enforce "max(0, 20 - used)" logic
                var leftover = 20 - totalUsed;
                if (leftover < 0) leftover = 0;
                existingCustomer.AvailableRecipes = leftover;

                customerRepository.SaveCustomer(existingCustomer);
                updatedCount++;
            }
        }

        var message = $"Migration complete. Created={createdCount}, Updated={updatedCount}, UniqueEmails={emailToSynthesizedCount.Count}";
        logger.LogInformation(message);
        return Ok(new { message, createdCount, updatedCount, uniqueEmails = emailToSynthesizedCount.Count });
    }
}
