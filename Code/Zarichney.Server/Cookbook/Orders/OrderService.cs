using System.Collections.Concurrent;
using System.Text;
using Microsoft.Graph.Models;
using Polly;
using Polly.Retry;
using Zarichney.Config;
using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Prompts;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services.AI;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Email;
using Zarichney.Services.PdfGeneration;
using Zarichney.Services.Sessions;

namespace Zarichney.Cookbook.Orders;

public class OrderConfig : IConfig
{
  public int MaxParallelTasks { get; init; } = 5;
  public string OutputDirectory { get; init; } = "Data/Orders";
}

public interface IOrderService
{
  Task<CookbookOrder> GetOrder(string orderId);
  Task<CookbookOrder> ProcessSubmission(CookbookOrderSubmission submission, bool processOrder = true);
  Task ProcessOrder(string orderId);
  Task CompilePdf(CookbookOrder order, bool waitForWrite = false);
  Task EmailCookbook(string orderId);
  Task<byte[]> GetPdf(string orderId);
  void QueueOrderProcessing(string orderId);
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
  ISessionManager sessionManager,
  ICustomerService customerService
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
      // Load or create the customer by email
      var customer = await customerService.GetOrCreateCustomer(submission.Email);

      // Generate the requested recipes from LLM
      var (cookbookRecipeList, conversationId) = await GetOrderRecipes(submission);

      // Create the order
      var order = new CookbookOrder(customer, submission, cookbookRecipeList)
      {
        LlmConversationId = conversationId
      };

      // Save to session
      await sessionManager.AddOrder(scope, order);

      logger.LogInformation("Order intake: {@Order}", order);

      if (processOrder)
      {
        var orderId = order.OrderId;

        // Queue the cookbook generation task
        backgroundService.QueueBackgroundWorkAsync(async (newScope, _) =>
        {
          var backgroundOrderService = newScope.GetService<IOrderService>();
          // todo: test to ensure that this will attach itself to the previous session, leaving the newly created session to be auto cleaned up
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
    // Retrieve the order from session
    var session = await sessionManager.GetSessionByOrder(orderId, scope.Id);
    var order = session.Order
                ?? throw new KeyNotFoundException($"No order found using ID: {orderId}");

    if (!await VerifyCredits(order))
    {
      logger.LogInformation(
        "Order {OrderId} not processed due to insufficient credits for {Email}. Email sent.",
        order.OrderId,
        order.Email
      );
      return;
    }

    try
    {
      order.Status = OrderStatus.InProgress;

      await ProcessRecipesAsync(order);

      await CreatePdf(order);

      await EmailCookbook(order);

      order.Status = order.RecipeList.Count == order.SynthesizedRecipes.Count
        ? OrderStatus.Completed
        : OrderStatus.AwaitingPayment;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to process order {OrderId}",
        nameof(ProcessOrder), orderId);
      order.Status = OrderStatus.Failed;
    }
  }

  private async Task<bool> VerifyCredits(CookbookOrder order)
  {
    // Check if customer has enough credits
    var customer = order.Customer;

    if (customer.AvailableRecipes > 0)
    {
      return true;
    }

    // They have no credits => send an email informing them that they need to purchase more credits
    await SendOrderPendingEmail(order);

    order.Status = OrderStatus.AwaitingPayment;

    return false;
  }

  private async Task SendOrderPendingEmail(CookbookOrder order)
  {
    await emailService.SendEmail(
      order.Email,
      "Cookbook Factory - Order Pending",
      "credits-needed",
      new Dictionary<string, object>
      {
        { "orderId", order.OrderId },
        { "recipes", order.RecipeList }
      }
    );
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

  public async Task<byte[]> GetPdf(string orderId)
  {
    var pdf = await orderRepository.GetCookbook(orderId);
    if (pdf == null || pdf.Length == 0)
    {
      throw new Exception($"Cookbook PDF not found for order {orderId}");
    }

    return pdf;
  }

  /// <summary>
  /// Queue an order for processing in the background
  /// </summary>
  public void QueueOrderProcessing(string orderId)
  {
    if (string.IsNullOrEmpty(orderId))
    {
      throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
    }

    backgroundService.QueueBackgroundWorkAsync(async (newScope, _) =>
    {
      var backgroundOrderService = newScope.GetService<IOrderService>();
      await backgroundOrderService.ProcessOrder(orderId);
    });
  }

  /// <summary>
  /// Uses AI to generate the list of recipes for the cookbook.
  /// Should be using the provided request, and/or generates the list based on the user's input.
  /// </summary>
  /// <param name="submission"></param>
  /// <returns>A list of recipe names and the LLM conversation ID in a tuple</returns>
  private async Task<(List<string> recipeList, string conversationId)> GetOrderRecipes(CookbookOrderSubmission submission)
  {
    var llmResult = await llmService.CallFunction<RecipeProposalResult>(
      processOrderPrompt.SystemPrompt,
      processOrderPrompt.GetUserPrompt(submission),
      processOrderPrompt.GetFunction()
    );

    return (llmResult.Data.Recipes, llmResult.ConversationId);
  }

  private async Task ProcessRecipesAsync(CookbookOrder order, bool force = false)
  {
    // 1) Gather the list of recipes that have NOT been synthesized yet.
    var alreadySynthesizedTitles = order.SynthesizedRecipes
      .Select(r => r.Title ?? "")
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    var pendingRecipes = order.RecipeList
      .Where(recipeName => force || !alreadySynthesizedTitles.Contains(recipeName))
      .ToList();

    if (pendingRecipes.Count == 0)
    {
      logger.LogInformation("No new recipes to process for order {OrderId}", order.OrderId);
      return;
    }

    // Check how many credits the user has right now
    var customer = order.Customer;
    var available = customer.AvailableRecipes;
    if (available <= 0)
    {
      logger.LogInformation("Customer {Email} has 0 available recipes left, skipping processing", customer.Email);
      return;
    }

    // Decide how many recipes we will process this pass
    var toProcessCount = Math.Min(available, pendingRecipes.Count);

    var newRecipesBag = new ConcurrentBag<SynthesizedRecipe>();
    var completedCount = 0;
    var maxParallelTasks = Math.Min(config.MaxParallelTasks, toProcessCount);

    using var cts = new CancellationTokenSource();
    try
    {
      await sessionManager.ParallelForEachAsync(scope, pendingRecipes,
        async (_, recipeName, ct) =>
        {
          // If we've already processed enough, cancel
          if (Interlocked.CompareExchange(ref completedCount, 0, 0) >= toProcessCount)
          {
            await cts.CancelAsync();
            return;
          }

          var sourceRecipes = await GetRecipes(recipeName, order, ct: ct);
          var newRecipe = await recipeService.SynthesizeRecipe(sourceRecipes, order, recipeName);

          newRecipe.ImageUrls = GetImageUrls(newRecipe, sourceRecipes);
          newRecipe.SourceRecipes = sourceRecipes
            .Where(r => !(newRecipe.InspiredBy?.Count > 0)
                        || newRecipe.InspiredBy.Contains(r.RecipeUrl!))
            .ToList();
          newRecipe.Title ??= recipeName; // fallback

          newRecipesBag.Add(newRecipe);

          // Check if we've hit the limit
          var after = Interlocked.Increment(ref completedCount);
          if (after >= toProcessCount)
          {
            await cts.CancelAsync();
          }
        },
        maxParallelTasks,
        cts.Token
      );
    }
    catch (OperationCanceledException)
    {
      // Normal partial-cancel scenario
    }
    catch (Exception ex)
    {
      logger.LogError(ex,
        "Unhandled exception during recipe processing for order {OrderId}",
        order.OrderId);
    }

    // 4) We have new recipes in `newRecipesBag`; add them to the order
    var newlyProcessedCount = newRecipesBag.Count;
    if (newlyProcessedCount > 0)
    {
      var newRecipesList = newRecipesBag.ToList();
      order.SynthesizedRecipes.AddRange(newRecipesList);

      // 5) Decrement the user's credits by however many were actually processed
      customerService.DecrementRecipes(customer, newlyProcessedCount);
    }

    // 6) If we haven't processed all recipes, set RequiresPayment to indicate there's more to do in a future re-run
    order.RequiresPayment = order.SynthesizedRecipes.Count < order.RecipeList.Count;

    logger.LogInformation(
      "Order {OrderId} processing complete. Synthesized {NewCount} new recipes. Total now: {TotalCount}. Still requires payment? {Pay}",
      order.OrderId,
      newlyProcessedCount,
      order.SynthesizedRecipes.Count,
      order.RequiresPayment
    );
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
      // Retrieve the conversation ID from the order
      var conversationId = order.LlmConversationId;

      // Pass the conversation ID to the recipe service
      return await recipeService.GetRecipes(recipeName, acceptableScore: null, conversationId: conversationId, ct: ct);
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

      var templateData = new Dictionary<string, object>
      {
        { "title", "Your Cookbook is Ready!" },
        { "isPartial", order.RequiresPayment }, // true if not all recipes are processed
        { "processedRecipes", order.SynthesizedRecipes.Count },
        { "totalRecipes", order.RecipeList.Count },
        { "orderId", order.OrderId }
      };

      logger.LogInformation("Sending cookbook email to {Email}", order.Email);

      await emailService.SendEmail(
        order.Email,
        "Your Cookbook is Ready!",
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
}
