using System.Collections.Concurrent;
using System.Text;
using Cookbook.Factory.Config;
using Cookbook.Factory.Models;
using Cookbook.Factory.Prompts;
using Polly;
using Polly.Retry;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Cookbook.Factory.Services;

public class OrderConfig : IConfig
{
    public int MaxParallelTasks { get; init; } = 5;
    public int MaxSampleRecipes { get; init; } = 5;
    public string OutputDirectory { get; init; } = "Orders";
}

public class OrderService(
    OrderConfig config,
    ILlmService llmService,
    IFileService fileService,
    RecipeService recipeService,
    ProcessOrderPrompt processOrderPrompt,
    PdfCompiler pdfCompiler,
    IEmailService emailService,
    LlmConfig llmConfig
)
{
    private readonly ILogger _log = Log.ForContext<OrderService>();

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: llmConfig.RetryAttempts,
            sleepDurationProvider: _ => TimeSpan.FromSeconds(1),
            onRetry: (exception, _, retryCount, context) =>
            {
                Log.Warning(exception,
                    "Email attempt {retryCount}: Retrying due to {exception}. Retry Context: {@Context}",
                    retryCount, exception.Message, context);
            }
        );

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
        order.SynthesizedRecipes = await ProcessRecipes(order, isSample);

        UpdateOrderFile(order);

        return order;
    }

    private async Task<List<SynthesizedRecipe>> ProcessRecipes(CookbookOrder order, bool isSample)
    {
        var completedRecipes = new ConcurrentBag<SynthesizedRecipe>();

        var maxParallelTasks = config.MaxParallelTasks;
        if (isSample)
        {
            maxParallelTasks = Math.Min(maxParallelTasks, config.MaxSampleRecipes);
        }

        var cts = new CancellationTokenSource();
        var completedCount = 0;

        try
        {
            await Parallel.ForEachAsync(order.RecipeList, new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxParallelTasks,
                    CancellationToken = cts.Token
                },
                async (recipeName, ct) =>
                {
                    try
                    {
                        // Check if sample size is reached
                        if (isSample && Interlocked.CompareExchange(ref completedCount, 0, 0) >=
                            config.MaxSampleRecipes)
                        {
                            _log.Information("Sample size reached, stopping processing");
                            cts.Cancel();
                            return;
                        }

                        await ProcessRecipe(recipeName, ct);

                        // Increment the completed count after successful processing
                        if (isSample && Interlocked.Increment(ref completedCount) >= config.MaxSampleRecipes)
                        {
                            _log.Information("Sample size reached, stopping processing");
                            cts.Cancel();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _log.Information("Processing of {RecipeName} was canceled.", recipeName);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Unhandled exception in ProcessRecipe for {RecipeName}", recipeName);
                    }
                });
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Unhandled exception during processing.");
        }

        return completedRecipes.ToList();

        async Task ProcessRecipe(string recipeName, CancellationToken ct)
        {
            try
            {
                // Check for cancellation
                ct.ThrowIfCancellationRequested();

                List<Recipe> recipes;
                try
                {
                    recipes = await recipeService.GetRecipes(recipeName, order);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Recipe {RecipeName} aborted in error. Cannot include in cookbook.", recipeName);
                    return;
                }

                var (result, rejects) = await recipeService.SynthesizeRecipe(recipes, order, recipeName);

                var count = 0;
                foreach (var recipe in rejects)
                {
                    WriteRejectToOrderDir(order.OrderId, $"{++count}. {recipeName}", recipe);
                }

                // Add image to recipe
                result.ImageUrls = GetImageUrls(result, recipes);

                WriteRecipeToOrderDir(order.OrderId, recipeName, result);
                completedRecipes.Add(result);
            }
            catch (OperationCanceledException)
            {
                _log.Information("Processing of {RecipeName} was canceled.", recipeName);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unhandled exception in ProcessRecipe for {RecipeName}", recipeName);
            }
        }
    }

    private List<string> GetImageUrls(SynthesizedRecipe result, List<Recipe> recipes)
    {
        var relevantRecipes = recipes.Where(r => result.InspiredBy?.Contains(r.RecipeUrl!) ?? false).ToList();

        if (relevantRecipes.Count == 0)
        {
            // Fallback to using any of the provided recipes
            relevantRecipes = recipes;
        }

        return relevantRecipes
            .Where(r => !string.IsNullOrWhiteSpace(r.ImageUrl))
            .Select(r => r.ImageUrl!)
            .ToList();
    }

    private void CreateOrderDirectory(CookbookOrder order)
        => UpdateOrderFile(order);

    private void UpdateOrderFile(CookbookOrder order)
        => fileService.WriteToFileAsync(
            Path.Combine(config.OutputDirectory, order.OrderId),
            "Order",
            order
        );

    private void WriteRejectToOrderDir(string orderId, string recipeName, SynthesizedRecipe recipe)
        => fileService.WriteToFileAsync(
            Path.Combine(config.OutputDirectory, orderId, "recipes", "rejects"),
            recipeName,
            recipe
        );

    private void WriteRecipeToOrderDir(string orderId, string recipeName, SynthesizedRecipe recipe)
        => fileService.WriteToFileAsync(
            Path.Combine(config.OutputDirectory, orderId, "recipes"),
            recipeName,
            recipe
        );

    public async Task CompilePdf(CookbookOrder order, bool waitForWrite = false)
    {
        if (!(order.SynthesizedRecipes?.Count > 0))
        {
            throw new Exception("Cannot assemble pdf as this order contains no recipes");
        }

        var cookbookMarkdown = new StringBuilder();

        foreach (var recipe in order.SynthesizedRecipes)
        {
            var recipeMarkdown = recipe.ToMarkdown();
            fileService.WriteToFileAsync(
                Path.Combine(config.OutputDirectory, order.OrderId, "recipes"),
                recipe.Title!,
                recipeMarkdown,
                "md"
            );
            cookbookMarkdown.AppendLine(recipeMarkdown);
        }

        fileService.WriteToFileAsync(
            Path.Combine(config.OutputDirectory, order.OrderId),
            "Cookbook",
            cookbookMarkdown.ToString(),
            "md"
        );

        var pdf = await pdfCompiler.CompileCookbook(order);

        _log.Information("Cookbook compiled for order {OrderId}. Writing to disk", order.OrderId);

        if (waitForWrite)
        {
            await fileService.WriteToFile(
                Path.Combine(config.OutputDirectory, order.OrderId),
                "Cookbook",
                pdf,
                "pdf"
            );
        }
        else
        {
            // Send to background queue
            fileService.WriteToFileAsync(
                Path.Combine(config.OutputDirectory, order.OrderId),
                "Cookbook",
                pdf,
                "pdf"
            );
        }
    }

    public async Task EmailCookbook(string orderId)
    {
        var emailTitle = "Your Cookbook is Ready!";
        var templateData = new Dictionary<string, object>
        {
            { "title", emailTitle },
            { "company_name", "Zarichney Development" },
            { "current_year", DateTime.Now.Year },
            { "unsubscribe_link", "https://zarichney.com/unsubscribe" },
        };

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var order = await GetOrder(orderId);

            _log.Information("Retrieved order {OrderId} for email", orderId);

            var pdf = await fileService.ReadFromFile<byte[]>(
                Path.Combine(config.OutputDirectory, orderId),
                "Cookbook",
                "pdf"
            );
            
            if (pdf == null || pdf.Length == 0)
            {
                throw new Exception($"Cookbook PDF not found for order {orderId}");
            }

            _log.Information("Sending cookbook email to {Email}", order.Email);

            await emailService.SendEmail(
                order.Email,
                emailTitle,
                "cookbook-ready",
                templateData,
                pdf
            );

            _log.Information("Cookbook emailed to {Email}", order.Email);
        });
    }
}