using System.Collections.Concurrent;
using System.Text;
using Microsoft.Graph.Models;
using Polly;
using Polly.Retry;
using Serilog;
using Zarichney.Config;
using Zarichney.Cookbook.Models;
using Zarichney.Cookbook.Prompts;
using Zarichney.Services;
using Zarichney.Services.Sessions;

namespace Zarichney.Cookbook.Services;

public class OrderConfig : IConfig
{
  public int MaxParallelTasks { get; init; } = 5;
  public int MaxSampleRecipes { get; init; } = 5;
  public string OutputDirectory { get; init; } = "Orders";
}

public interface IOrderService
{
  Task<CookbookOrder> ProcessSubmission(CookbookOrderSubmission submission);
  Task<CookbookOrder> GetOrder(string orderId);
  Task<CookbookOrder> GenerateCookbookAsync(CookbookOrder order, bool isSample = false);
  Task CompilePdf(CookbookOrder order, bool waitForWrite = false);
  Task EmailCookbook(string orderId);
  Task ProcessOrder(string orderId);
}

public class OrderService(
  OrderConfig config,
  ILlmService llmService,
  IFileService fileService,
  IRecipeService recipeService,
  ProcessOrderPrompt processOrderPrompt,
  PdfCompiler pdfCompiler,
  IEmailService emailService,
  EmailConfig emailConfig,
  LlmConfig llmConfig,
  ISessionManager sessionManager,
  IScopeContainer scope,
  ILogger<RecipeService> logger
) : IOrderService
{
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

  public async Task<CookbookOrder> ProcessSubmission(CookbookOrderSubmission submission)
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

    await sessionManager.AddOrder(scope.Id, order);

    logger.LogInformation("Order intake: {@Order}", order);

    return order;
  }

  public async Task<CookbookOrder> GetOrder(string orderId)
  {
    var session = await sessionManager.GetSessionByOrder(orderId, scope.Id);
    var order = session.Order;

    if (order == null)
    {
      await sessionManager.EndSession(orderId);
      throw new KeyNotFoundException($"No order found using ID: {orderId}");
    }

    return order;
  }

  public async Task<CookbookOrder> GenerateCookbookAsync(CookbookOrder order, bool isSample = false)
  {
    await ProcessRecipesAsync(order, isSample);
    
    return order;
  }

  private async Task ProcessRecipesAsync(CookbookOrder order, bool isSample)
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
      await sessionManager.ParallelForEachAsync(order.RecipeList, async (_, recipeName, ct) =>
        {
          try
          {
            // Check if sample size is reached
            if (isSample && Interlocked.CompareExchange(ref completedCount, 0, 0) >=
                config.MaxSampleRecipes)
            {
              logger.LogInformation("Sample size reached, stopping processing");
              await cts.CancelAsync();
              return;
            }

            await ProcessRecipe(recipeName, ct);

            // Increment the completed count after successful processing
            if (isSample && Interlocked.Increment(ref completedCount) >= config.MaxSampleRecipes)
            {
              logger.LogInformation("Sample size reached, stopping processing");
              await cts.CancelAsync();
            }
          }
          catch (OperationCanceledException)
          {
            logger.LogInformation("Processing of {RecipeName} was canceled.", recipeName);
          }
          catch (Exception ex)
          {
            logger.LogError(ex, "Unhandled exception in ProcessRecipe for {RecipeName}", recipeName);
          }
        },
        maxParallelTasks,
        cts.Token);
    }
    catch (OperationCanceledException)
    {
      // Expected when cancellation is requested
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Unhandled exception during processing.");
    }

    order.SynthesizedRecipes = completedRecipes.ToList();

    return;

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
        catch (NoRecipeException e)
        {
          await emailService.SendEmail(
            emailConfig.FromEmail,
            $"Cookbook Factory - No Recipe Found - {recipeName}",
            "no-recipe",
            new Dictionary<string, object>
            {
              { "recipeName", recipeName },
              { "previousAttempts", e.PreviousAttempts }
            }
          );
          logger.LogError(
            "Cannot include in cookbook. No recipe found for {RecipeName}. Previous attempts: {PreviousAttempts}",
            recipeName, e.PreviousAttempts);
          return;
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Recipe {RecipeName} aborted in error. Cannot include in cookbook.", recipeName);
          return;
        }

        var result = await recipeService.SynthesizeRecipe(recipes, order, recipeName);

        // Add image to recipe
        result.ImageUrls = GetImageUrls(result, recipes);
        result.SourceRecipes = recipes
          .Where(r => !(result.InspiredBy?.Count > 0) || result.InspiredBy.Contains(r.RecipeUrl!))
          .ToList();

        completedRecipes.Add(result);
      }
      catch (OperationCanceledException)
      {
        logger.LogInformation("Processing of {RecipeName} was canceled.", recipeName);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Unhandled exception in ProcessRecipe for {RecipeName}", recipeName);
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

  public async Task CompilePdf(CookbookOrder order, bool waitForWrite = false)
  {
    if (!(order.SynthesizedRecipes.Count > 0))
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

    logger.LogInformation("Cookbook compiled for order {OrderId}. Writing to disk", order.OrderId);

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
    var order = await GetOrder(orderId);
    await EmailCookbook(order);
  }

  private async Task EmailCookbook(CookbookOrder order)
  {
    var orderId = order.OrderId;

    var emailTitle = "Your Cookbook is Ready!";
    var templateData = new Dictionary<string, object>
    {
      { "title", emailTitle },
      { "company_name", "Zarichney Development" },
      { "current_year", DateTime.Now.Year },
      { "unsubscribe_link", "https://zarichney.com/unsubscribe" },
    };

    logger.LogInformation("Retrieved order {OrderId} for email", orderId);

    await _retryPolicy.ExecuteAsync(async () =>
    {
      var pdf = await fileService.ReadFromFile<byte[]>(
        Path.Combine(config.OutputDirectory, orderId),
        "Cookbook",
        "pdf"
      );

      if (pdf == null || pdf.Length == 0)
      {
        throw new Exception($"Cookbook PDF not found for order {orderId}");
      }

      logger.LogInformation("Sending cookbook email to {Email}", order.Email);

      await emailService.SendEmail(
        order.Email,
        emailTitle,
        "cookbook-ready",
        templateData,
        new FileAttachment
        {
          Name = "Cookbook.pdf",
          ContentType = "application/pdf",
          ContentBytes = pdf,
          Size = pdf.Length
        }
      );

      logger.LogInformation("Cookbook emailed to {Email}", order.Email);
    });
  }

  public async Task ProcessOrder(string orderId)
  {
    try
    {
      var order = await GetOrder(orderId);
      await GenerateCookbookAsync(order, true);
      await CompilePdf(order);
      await EmailCookbook(order);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to process order {OrderId}",
        nameof(ProcessOrder), orderId);
    }
    finally
    {
      await sessionManager.EndSession(orderId);
    }
  }
}