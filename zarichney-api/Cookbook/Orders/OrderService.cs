using System.Collections.Concurrent;
using System.Text;
using Microsoft.Graph.Models;
using Polly;
using Polly.Retry;
using Zarichney.Config;
using Zarichney.Cookbook.Prompts;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services;
using Zarichney.Services.Emails;
using Zarichney.Services.Sessions;

namespace Zarichney.Cookbook.Orders;

public class OrderConfig : IConfig
{
  public int MaxParallelTasks { get; init; } = 5;
  public int MaxSampleRecipes { get; init; } = 5;
  public string OutputDirectory { get; init; } = "Orders";
}

public interface IOrderService
{
  Task<CookbookOrder> GetOrder(string orderId);
  Task<CookbookOrder> ProcessSubmission(CookbookOrderSubmission submission, bool processOrder = true);
  Task ProcessOrder(string orderId);
  Task CompilePdf(CookbookOrder order, bool waitForWrite = false);
  Task EmailCookbook(string orderId);
}

public class OrderService(
  IBackgroundWorker backgroundService,
  EmailConfig emailConfig,
  IEmailService emailService,
  ILogger<RecipeService> logger,
  ILlmService llmService,
  LlmConfig llmConfig,
  OrderConfig config,
  IOrderRepository orderRepository,
  PdfCompiler pdfCompiler,
  ProcessOrderPrompt processOrderPrompt,
  IRecipeService recipeService,
  IScopeContainer scope,
  ISessionManager sessionManager
) : IOrderService
{
  private readonly AsyncRetryPolicy _retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(
      retryCount: llmConfig.RetryAttempts,
      sleepDurationProvider: _ => TimeSpan.FromSeconds(1),
      onRetry: (exception, _, retryCount, context) =>
      {
        logger.LogWarning(exception,
          "Email attempt {retryCount}: Retrying due to {exception}. Retry Context: {@Context}",
          retryCount, exception.Message, context);
      }
    );

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

  public async Task<CookbookOrder> ProcessSubmission(CookbookOrderSubmission submission, bool processOrder = true)
  {
    try
    {
      var cookbookRecipeList = await GetOrderRecipes(submission);

      var order = new CookbookOrder(submission, cookbookRecipeList);

      await sessionManager.AddOrder(scope.Id, order);

      logger.LogInformation("Order intake: {@Order}", order);

      if (processOrder)
      {
        var orderId = order.OrderId;

        // Queue the cookbook generation task
        backgroundService.QueueBackgroundWorkAsync(async (newScope, _) =>
        {
          var backgroundOrderService = newScope.GetService<IOrderService>();
          await backgroundOrderService.ProcessOrder(orderId);
        });
      }

      return order;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to process submission");
      throw;
    }
  }

  public async Task ProcessOrder(string orderId)
  {
    const bool isSample = true; // temp until properly implemented

    try
    {
      var order = await GetOrder(orderId);

      order.Status = OrderStatus.InProgress;

      await ProcessRecipesAsync(order, isSample);

      await CreatePdf(order);

      await EmailCookbook(order);

      order.Status = OrderStatus.Completed;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to process order {OrderId}",
        nameof(ProcessOrder), orderId);
    }
  }

  public async Task CompilePdf(CookbookOrder order, bool waitForWrite = false)
  {
    try
    {
      await CreatePdf(order, waitForWrite);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to compile pdf for order {OrderId}", order.OrderId);
      throw;
    }
  }

  public async Task EmailCookbook(string orderId)
  {
    try
    {
      var order = await GetOrder(orderId);

      await EmailCookbook(order);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to email cookbook for order {OrderId}", orderId);
      throw;
    }
  }

  /// <summary>
  /// Uses AI to generate the list of recipes for the cookbook.
  /// Should be using the provided request, and/or generates the list based on the user's input.
  /// </summary>
  /// <param name="submission"></param>
  /// <returns></returns>
  private async Task<List<string>> GetOrderRecipes(CookbookOrderSubmission submission)
  {
    var result = await llmService.CallFunction<RecipeProposalResult>(
      processOrderPrompt.SystemPrompt,
      processOrderPrompt.GetUserPrompt(submission),
      processOrderPrompt.GetFunction()
    );

    return result.Recipes;
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
      await sessionManager.ParallelForEachAsync(scope,
        order.RecipeList,
        async (_, recipeName, ct) =>
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
        }, maxParallelTasks, cts.Token);
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

        var sourceRecipes = await GetRecipes(recipeName, order, ct: ct);

        var newRecipe = await recipeService.SynthesizeRecipe(sourceRecipes, order, recipeName);

        // Add image to recipe
        newRecipe.ImageUrls = GetImageUrls(newRecipe, sourceRecipes);
        newRecipe.SourceRecipes = sourceRecipes
          .Where(r => !(newRecipe.InspiredBy?.Count > 0) || newRecipe.InspiredBy.Contains(r.RecipeUrl!))
          .ToList();

        completedRecipes.Add(newRecipe);
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

  /// <summary>
  /// Get recipes from the recipe service
  /// The error handling here is done at the order level because of monitoring production issues. Direct access to recipe service doesn't alert of issues.
  /// </summary>
  /// <param name="recipeName"></param>
  /// <param name="order"></param>
  /// <param name="ct"></param>
  /// <returns></returns>
  private async Task<List<Recipe>> GetRecipes(string recipeName, CookbookOrder order, CancellationToken ct)
  {
    try
    {
      return await recipeService.GetRecipes(recipeName, order, ct: ct);
    }
    catch (NoRecipeException e)
    {
      // Real time notification of recipe service failure

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
      throw;
    }
    catch (Exception ex)
    {
      var templateData = TemplateService.GetErrorTemplateData(ex);
      templateData["stage"] = "GetRecipes";
      templateData["serviceName"] = "Recipe";
      ((Dictionary<string, string>)templateData["additionalContext"])["Customer"] = order.Email;
      ((Dictionary<string, string>)templateData["additionalContext"])["OrderId"] = order.OrderId;
      ((Dictionary<string, string>)templateData["additionalContext"])["Status"] = order.Status.ToString();
      ((Dictionary<string, string>)templateData["additionalContext"])["RecipeName"] = recipeName;

      await emailService.SendEmail(
        emailConfig.FromEmail,
        $"Cookbook Factory - Recipe Error - {recipeName}",
        "error-log",
        templateData
      );

      logger.LogError(ex, "Recipe {RecipeName} aborted in error. Cannot include in cookbook.", recipeName);
      throw;
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

  /// <summary>
  /// Prepares the markdown, calls compiler for PDF bytes and writes to file
  /// </summary>
  /// <param name="order"></param>
  /// <param name="waitForWrite">When true, will perform synchronous write operations</param>
  /// <exception cref="Exception"></exception>
  private async Task CreatePdf(CookbookOrder order, bool waitForWrite = false)
  {
    if (!(order.SynthesizedRecipes.Count > 0))
    {
      throw new Exception("Cannot assemble pdf as this order contains no recipes");
    }

    var cookbookMarkdown = new StringBuilder();

    foreach (var recipe in order.SynthesizedRecipes)
    {
      var recipeMarkdown = recipe.ToMarkdown();

      if (waitForWrite)
      {
        // Mainly for testing purposes
        orderRepository.SaveRecipe(order.OrderId, recipe.Title!, recipeMarkdown);
      }

      cookbookMarkdown.AppendLine(recipeMarkdown);
    }

    if (waitForWrite)
    {
      orderRepository.SaveCookbook(order.OrderId, cookbookMarkdown.ToString());
    }

    var pdf = await pdfCompiler.CompileCookbook(order);

    logger.LogInformation("Cookbook compiled for order {OrderId}. Writing to disk", order.OrderId);

    if (waitForWrite)
    {
      await orderRepository.SaveCookbook(order.OrderId, pdf);
    }
    else
    {
      orderRepository.SaveCookbookAsync(order.OrderId, pdf);
    }
  }

  private async Task EmailCookbook(CookbookOrder order)
  {
    await _retryPolicy.ExecuteAsync(async () =>
    {
      var pdf = await orderRepository.GetCookbook(order.OrderId);

      if (pdf == null || pdf.Length == 0)
      {
        throw new Exception($"Cookbook PDF not found for order {order.OrderId}");
      }

      logger.LogInformation("Sending cookbook email to {Email}", order.Email);

      await emailService.SendEmail(
        order.Email,
        "Your Cookbook is Ready!",
        "cookbook-ready",
        null,
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
}