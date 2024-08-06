using System.Collections.Concurrent;
using Cookbook.Factory.Models;
using Cookbook.Factory.Prompts;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Cookbook.Factory.Services;

public class OrderService(
    ILlmService llmService,
    FileService fileService,
    RecipeService recipeService,
    ProcessOrderPrompt processOrderPrompt
)
{
    private const string OrdersOutputDirectoryName = "Orders";
    private const int MaxParallelTasks = 1;
    private const int MaxSampleRecipes = 3;
    private readonly ILogger _log = Log.ForContext<OrderService>();

    public async Task<CookbookOrder> ProcessOrderSubmission(CookbookOrderSubmission submission)
    {
        var result = await llmService.CallFunction<RecipeProposalResult>(
            processOrderPrompt.SystemPrompt,
            processOrderPrompt.GetUserPrompt(submission),
            processOrderPrompt.GetFunction()
        );
        
        var order = new CookbookOrder(submission, result.Recipes)
        {
            Email = submission.Email,
            CookbookContent = submission.CookbookContent,
            CookbookDetails = submission.CookbookDetails,
            UserDetails = submission.UserDetails
        };

        _log.Information("Order intake: {@Order}", order);

        CreateOrderDirectory(order);

        return order;
    }

    public async Task<CookbookOrder> GenerateCookbookAsync(CookbookOrder order, bool isSample = false)
    {
        var completedRecipes = new ConcurrentQueue<SynthesizedRecipe>();
        var semaphore = new SemaphoreSlim(Math.Min(MaxParallelTasks, MaxSampleRecipes));
        var processingTasks = new List<Task>();

        foreach (var recipeName in order.Recipes)
        {
            if (isSample && completedRecipes.Count >= MaxSampleRecipes)
            {
                _log.Information("Sample size reached, stopping processing");
                break;
            }

            processingTasks.Add(ProcessRecipe(recipeName));

            if (processingTasks.Count >= MaxParallelTasks)
            {
                await Task.WhenAny(processingTasks);
                processingTasks.RemoveAll(t => t.IsCompleted);
            }
        }

        await Task.WhenAll(processingTasks);

        // Update the order's recipe list to reflect what was actually processed
        order.Recipes = completedRecipes.Select(recipe => recipe.Title).ToList();

        return order;

        async Task ProcessRecipe(string recipeName)
        {
            await semaphore.WaitAsync();
            try
            {
                if (isSample && completedRecipes.Count >= MaxSampleRecipes)
                {
                    _log.Information("Sample size reached, stopping processing");
                    return;
                }

                var recipes = await recipeService.GetRecipes(recipeName, order);

                var (result, rejects) = await recipeService.SynthesizeRecipe(recipes, order, recipeName);

                foreach (var recipe in rejects)
                {
                    WriteRejectToOrderDir(order.OrderId, recipeName, recipe);
                }

                WriteRecipeToOrderDir(order.OrderId, recipeName, result);
                completedRecipes.Enqueue(result);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    private void CreateOrderDirectory(CookbookOrder order)
        => fileService.WriteToFile(
            Path.Combine(OrdersOutputDirectoryName, order.OrderId),
            "Order",
            order
        );

    private void WriteRejectToOrderDir(string orderId, string recipeName, SynthesizedRecipe recipe)
        => fileService.WriteToFile(
            Path.Combine(OrdersOutputDirectoryName, orderId, "recipes", "rejects"),
            recipeName,
            recipe
        );

    private void WriteRecipeToOrderDir(string orderId, string recipeName, SynthesizedRecipe recipe)
        => fileService.WriteToFile(
            Path.Combine(OrdersOutputDirectoryName, orderId, "recipes"),
            recipeName,
            recipe
        );
}