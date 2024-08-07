using System.Collections.Concurrent;
using Cookbook.Factory.Models;
using Cookbook.Factory.Prompts;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Cookbook.Factory.Services;

public class OrderConfig : IConfig
{
    public int MaxParallelTasks { get; init; } = 1;
    public int MaxSampleRecipes { get; init; } = 3;
    public string OutputDirectory { get; init; } = "Orders";
}

public class OrderService(
    OrderConfig config,
    ILlmService llmService,
    FileService fileService,
    RecipeService recipeService,
    ProcessOrderPrompt processOrderPrompt,
    PdfCompiler pdfCompiler
)
{
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

    public async Task<CookbookOrder> GetOrder(string orderId)
        => await fileService.ReadFromFile<CookbookOrder>(Path.Combine(config.OutputDirectory, orderId), "Order");

    public async Task<CookbookOrder> GenerateCookbookAsync(CookbookOrder order, bool isSample = false)
    {
        var completedRecipes = new ConcurrentQueue<SynthesizedRecipe>();
        var semaphore = new SemaphoreSlim(Math.Min(config.MaxParallelTasks, config.MaxSampleRecipes));
        var processingTasks = new List<Task>();

        foreach (var recipeName in order.RecipeList)
        {
            if (isSample && completedRecipes.Count >= config.MaxSampleRecipes)
            {
                _log.Information("Sample size reached, stopping processing");
                break;
            }

            processingTasks.Add(ProcessRecipe(recipeName));

            if (processingTasks.Count >= config.MaxParallelTasks)
            {
                await Task.WhenAny(processingTasks);
                processingTasks.RemoveAll(t => t.IsCompleted);
            }
        }

        await Task.WhenAll(processingTasks);

        order.Recipes = completedRecipes.ToList();

        UpdateOrderFile(order);

        return order;

        async Task ProcessRecipe(string recipeName)
        {
            await semaphore.WaitAsync();
            try
            {
                if (isSample && completedRecipes.Count >= config.MaxSampleRecipes)
                {
                    _log.Information("Sample size reached, stopping processing");
                    return;
                }

                List<Recipe> recipes;
                try
                {
                    recipes = await recipeService.GetRecipes(recipeName, order);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Skipping recipe {RecipeName} due to no recipes found", recipeName);
                    return;
                }

                var (result, rejects) = await recipeService.SynthesizeRecipe(recipes, order, recipeName);

                var count = 0;
                foreach (var recipe in rejects)
                {
                    WriteRejectToOrderDir(order.OrderId, $"{++count}. {recipeName}", recipe);
                }

                // Add image to recipe
                result.ImageUrl = GetImageUrl(result, recipes);

                WriteRecipeToOrderDir(order.OrderId, recipeName, result);
                completedRecipes.Enqueue(result);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    private string? GetImageUrl(ISynthesizedRecipe result, List<Recipe> recipes)
    {
        var relevantRecipes = recipes.Where(r => result.InspiredBy?.Contains(r.RecipeUrl!) ?? false).ToList();

        if (relevantRecipes.Count == 0)
        {
            // Fallback to using any of the provided recipes
            relevantRecipes = recipes;
        }

        return relevantRecipes
            .Where(r => !string.IsNullOrWhiteSpace(r.ImageUrl))
            .Select(r => r.ImageUrl)
            .FirstOrDefault();
    }

    private void CreateOrderDirectory(CookbookOrder order)
        => UpdateOrderFile(order);

    private void UpdateOrderFile(CookbookOrder order)
        => fileService.WriteToFile(
            Path.Combine(config.OutputDirectory, order.OrderId),
            "Order",
            order
        );

    private void WriteRejectToOrderDir(string orderId, string recipeName, SynthesizedRecipe recipe)
        => fileService.WriteToFile(
            Path.Combine(config.OutputDirectory, orderId, "recipes", "rejects"),
            recipeName,
            recipe
        );

    private void WriteRecipeToOrderDir(string orderId, string recipeName, SynthesizedRecipe recipe)
        => fileService.WriteToFile(
            Path.Combine(config.OutputDirectory, orderId, "recipes"),
            recipeName,
            recipe
        );

    public void CompilePdf(CookbookOrder order)
    {
        if (!(order.Recipes?.Count > 0))
        {
            throw new Exception("No recipes found for order");
        }

        var markdown = new List<string>();

        foreach (var recipe in order.Recipes)
        {
            var recipeMarkdown = recipe.ToMarkdown();
            fileService.WriteToFile(
                Path.Combine(config.OutputDirectory, order.OrderId, "recipes"),
                recipe.Title,
                recipeMarkdown,
                "md"
            );
            markdown.Add(recipeMarkdown);
        }

        var pdf = pdfCompiler.CompileCookbook(markdown);

        fileService.WriteToFile(
            Path.Combine(config.OutputDirectory, order.OrderId),
            "Cookbook",
            pdf,
            "pdf"
        );
    }
}