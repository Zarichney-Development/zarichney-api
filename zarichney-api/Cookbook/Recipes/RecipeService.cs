using System.Collections.Concurrent;
using System.Text.Json;
using AutoMapper;
using OpenAI.Assistants;
using OpenAI.Chat;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Prompts;
using Zarichney.Services;
using Zarichney.Services.Sessions;

namespace Zarichney.Cookbook.Recipes;

public interface IRecipeService
{
  Task<List<Recipe>> GetRecipes(
    string requestedRecipeName,
    CookbookOrder? cookbookOrder = null,
    int? acceptableScore = null,
    CancellationToken ct = default
  );

  Task<List<Recipe>> GetRecipes(
    string query,
    bool scrape = true,
    int? acceptableScore = null,
    int? requiredCount = null,
    string? requestedRecipeName = null,
    CancellationToken ct = default
  );

  Task<List<Recipe>> RankUnrankedRecipesAsync(IEnumerable<ScrapedRecipe> recipes, string query);
  Task<SynthesizedRecipe> SynthesizeRecipe(List<Recipe> recipes, CookbookOrder order, string recipeName);
}

public class RecipeService(
  IRecipeRepository recipeRepository,
  RecipeConfig config,
  ILlmService llmService,
  WebScraperService webscraper,
  IMapper mapper,
  RankRecipePrompt rankRecipePrompt,
  SynthesizeRecipePrompt synthesizeRecipePrompt,
  AnalyzeRecipePrompt analyzeRecipePrompt,
  ProcessOrderPrompt processOrderPrompt,
  ISessionManager sessionManager,
  ILogger<RecipeService> logger,
  IScopeContainer scope
) : IRecipeService
{
  public async Task<List<Recipe>> GetRecipes(
    string requestedRecipeName,
    CookbookOrder? cookbookOrder = null,
    int? acceptableScore = null,
    CancellationToken ct = default
  )
  {
    var searchQuery = requestedRecipeName.Trim().ToLowerInvariant();
    acceptableScore ??= config.AcceptableScoreThreshold;

    var recipes = await GetRecipes(
      query: searchQuery,
      scrape: true,
      acceptableScore: acceptableScore,
      requestedRecipeName: requestedRecipeName,
      ct: ct
    );

    var previousAttempts = new List<string>();

    while (recipes.Count == 0)
    {
      previousAttempts.Add(searchQuery);

      var attempt = previousAttempts.Count + 1;
      logger.LogWarning("[{RecipeName}] - No recipes found using '{SearchQuery}'", requestedRecipeName, searchQuery);

      if (attempt > config.MaxNewRecipeNameAttempts)
      {
        logger.LogError("[{RecipeName}] - Aborting recipe searching after '{attempt}' attempts",
          requestedRecipeName, attempt - 1);
        throw new NoRecipeException(previousAttempts);
      }

      // Lower the bar of acceptable for every attempt
      acceptableScore -= 5;

      // With a lower bar, first attempt to check local sources before performing a new web scrape
      recipes = await GetRecipes(
        query: searchQuery,
        scrape: false,
        acceptableScore: acceptableScore,
        requestedRecipeName: requestedRecipeName,
        ct: ct
      );

      if (recipes.Count > 0)
      {
        break;
      }

      // Request for new query to scrap with
      searchQuery = await GetSearchQueryForRecipe(requestedRecipeName, previousAttempts, cookbookOrder);
      searchQuery = searchQuery.Trim().ToLowerInvariant();

      logger.LogInformation(
        "[{RecipeName}] - Attempting an alternative search query: '{NewRecipeName}'. Acceptable score of {AcceptableScore}.",
        requestedRecipeName, searchQuery, acceptableScore);

      recipes = await GetRecipes(
        query: searchQuery,
        scrape: true,
        acceptableScore: acceptableScore,
        requestedRecipeName: requestedRecipeName,
        ct: ct);
    }

    return recipes;
  }

  private async Task<string> GetSearchQueryForRecipe(string recipeName, List<string> previousAttempts,
    CookbookOrder? cookbookOrder = null)
  {
    var primaryApproach = previousAttempts.Count < Math.Ceiling(config.MaxNewRecipeNameAttempts / 2.0);

    var messages = new List<ChatMessage>();

    // For the first half of the attempts, request for alternatives
    if (primaryApproach)
    {
      messages.Add(new SystemChatMessage(processOrderPrompt.SystemPrompt));

      if (cookbookOrder != null)
      {
        messages.AddRange([
            new UserChatMessage(processOrderPrompt.GetUserPrompt(cookbookOrder)),
            new SystemChatMessage(string.Join(", ", cookbookOrder.RecipeList))
          ]
        );
      }

      messages.Add(new UserChatMessage(
        $"""
         Thank you. The recipe name "{recipeName}" didn't yield any search results, likely due to its uniqueness or obscurity.
         Please provide a more generic search query to retrieve similar recipes. With each failed attempt, generalize the search query.
         Only respond with a new recipe name.
         """));

      foreach (var previousAttempt in previousAttempts.Where(previousAttempt => previousAttempt != recipeName))
      {
        messages.AddRange([
          new SystemChatMessage(previousAttempt),
          new UserChatMessage(
            $"Sorry, that search also returned no matches. Suggest a query that is more ideal for finding relevant results but is still is relevant to '{recipeName}'.")
        ]);
      }
    }
    else
    {
      // For the second half of attempts, aggressively generalize the search query
      messages.AddRange([
        new SystemChatMessage(
          """
          <SystemPrompt>
              <Context>Online Recipe Searching</Context>
              <Goal>Your task is to provide an ideal search query that aims to returns search results yielding recipes that forms the basis of the user's requested recipe.</Goal>
              <Input>A unique recipe name that does not return search results.</Input>
              <Output>Respond with only the new search query and nothing else.</Output>
              <Examples>
                  <Example>
                      <Input>Pan-Seared Partridge with Herb Infusion</Input>
                      <Output>Partridge</Output>
                  </Example>
                  <Example>
                      <Input>Luigi’s Veggie Power-Up Pizza</Input>
                      <Output>Vegetable Pizza</Output>
                  </Example>
                  <Example>
                      <Input>Herb-Crusted Venison with Seasonal Vegetables</Input>
                      <Output>Venison</Output>
                  </Example>
              </Examples>
              <Rules>
                  <Rule>Omit Previous Attempts, dont respond with something already tried.</Rule>
                  <Rule>The more attempts made, the more generalized the search query should be</Rule>
                  <Rule>As part of your search query response suggestion, do not append 'Recipe' or 'Recipes'.</Rule>
              </Rules>
          </SystemPrompt>
          """
        ),
        new UserChatMessage(
          $"""
           Recipe: {recipeName}
           Previous Attempts: {string.Join(", ", previousAttempts)}
           """
        )
      ]);
    }

    var result = await llmService.GetCompletionContent(messages);

    return result.Trim('"').Trim();
  }

  public async Task<List<Recipe>> GetRecipes(
    string query,
    bool scrape = true,
    int? acceptableScore = null,
    int? requiredCount = null,
    string? requestedRecipeName = null,
    CancellationToken ct = default
  )
  {
    query = query.Trim().ToLowerInvariant();
    acceptableScore ??= config.AcceptableScoreThreshold;
    var recipesNeeded = requiredCount ?? config.RecipesToReturnPerRetrieval;
    var amountToRetrieveFromRepository = Math.Max(recipesNeeded, config.MaxSearchResults);

    // First, attempt to retrieve via local source of JSON files
    var recipes = await recipeRepository.SearchRecipes(query, acceptableScore, amountToRetrieveFromRepository, ct);
    if (recipes.Count != 0)
    {
      logger.LogInformation("Retrieved {count} cached recipes for query '{query}'.", recipes.Count, query);
    }

    var cachedRecipes = recipes
      .Where(r => r.Relevancy.TryGetValue(query, out var value) && value.Score >= acceptableScore)
      .Take(recipesNeeded)
      .ToList();

    if (cachedRecipes.Count >= recipesNeeded)
    {
      return cachedRecipes.ToList();
    }

    await RankUnrankedRecipesAsync(recipes, query, acceptableScore.Value, requestedRecipeName);

    // Check if additional recipes should be web-scraped
    if (scrape && recipes.Count(r => r.Relevancy.TryGetValue(query, out var v) && v.Score >= acceptableScore)
        < recipesNeeded)
    {
      // Proceed with web scraping
      var scrapedRecipes = await webscraper.ScrapeForRecipesAsync(query, acceptableScore, recipesNeeded);

      // Exclude any recipes already in the repository
      var newRecipes = scrapedRecipes.Where(r => !recipeRepository.ContainsRecipe(r.Id!));

      // Map scraped recipes to Recipe objects and add to the list
      recipes.AddRange(mapper.Map<List<Recipe>>(newRecipes));

      // Rank any new unranked recipes
      await RankUnrankedRecipesAsync(recipes, query, acceptableScore.Value, requestedRecipeName);
    }

    // Save recipes to the repository
    if (recipes.Count != 0)
    {
      recipeRepository.AddUpdateRecipesAsync(recipes);
    }

    // Return the top recipes
    return recipes
      .Where(r => r.Relevancy.TryGetValue(query, out var value) && value.Score >= acceptableScore)
      .Take(recipesNeeded)
      .ToList();
  }

  public async Task<List<Recipe>> RankUnrankedRecipesAsync(IEnumerable<ScrapedRecipe> recipes, string query)
    => await RankUnrankedRecipesAsync(mapper.Map<List<Recipe>>(recipes), query, config.AcceptableScoreThreshold);

  private async Task<List<Recipe>> RankUnrankedRecipesAsync(List<Recipe> recipes, string query, int acceptableScore,
    string? requestedRecipeName = null)
  {
    query = query.Trim().ToLowerInvariant();
    var unrankedRecipes = recipes.Where(r => !r.Relevancy.ContainsKey(query)).ToList();
    if (unrankedRecipes.Count != 0)
    {
      await RankRecipesAsync(unrankedRecipes, query, acceptableScore, requestedRecipeName);
      // At this point, unrankedRecipes have their Relevancy updated.
      // No need to replace items in the list since the recipes are reference types
      // and their properties have been updated in place.
    }

    // Now sort the recipes list based on the updated relevancy scores.
    recipes.Sort((r1, r2) =>
    {
      var score1 = r1.Relevancy.TryGetValue(query, out var v1) ? v1.Score : 0;
      var score2 = r2.Relevancy.TryGetValue(query, out var v2) ? v2.Score : 0;
      return score2.CompareTo(score1);
    });

    return recipes;
  }

  private async Task RankRecipesAsync(List<Recipe> recipes, string query, int? acceptableScore = null,
    string? requestedRecipeName = null)
  {
    query = query.Trim().ToLowerInvariant();
    acceptableScore ??= config.AcceptableScoreThreshold;

    var rankedRecipes = new ConcurrentBag<Recipe>();
    using var cts = new CancellationTokenSource();

    try
    {
      await sessionManager.ParallelForEachAsync(scope, recipes, async (_, recipe, ct) =>
        {
          // Check if cancellation has been requested
          if (ct.IsCancellationRequested)
            return;

          try
          {
            // If we haven't ranked this recipe or if previous score < acceptableScore, re-rank it
            if (!recipe.Relevancy.TryGetValue(query, out var existingVal) || existingVal.Score < acceptableScore)
            {
              // Evaluate relevancy if not already
              var newVal = await RankRecipe(recipe, query, requestedRecipeName);
              recipe.Relevancy[query] = newVal;
            }

            if (recipe.Relevancy[query].Score >= acceptableScore)
            {
              rankedRecipes.Add(recipe);

              // Check if we have collected enough recipes
              if (rankedRecipes.Count >= config.RecipesToReturnPerRetrieval)
              {
                await cts.CancelAsync(); // Cancel remaining tasks
              }
            }
          }
          catch (OperationCanceledException)
          {
            // Task was cancelled, just return
          }
          catch (Exception ex)
          {
            logger.LogError(ex, $"Error ranking recipe: {recipe.Id}");
            throw;
          }
        },
        config.MaxParallelTasks,
        cts.Token
      );
    }
    catch (OperationCanceledException)
    {
      // The operation was cancelled because enough recipes were found
    }
  }

  private async Task<RelevancyResult> RankRecipe(Recipe recipe, string query, string? requestedRecipeName)
  {
    var result = await llmService.CallFunction<RelevancyResult>(
      rankRecipePrompt.SystemPrompt,
      rankRecipePrompt.GetUserPrompt(recipe, query, requestedRecipeName),
      rankRecipePrompt.GetFunction()
    );

    result.Query = query;

    logger.LogInformation("Relevancy filter result: {@Result}", result);

    return result;
  }

  public async Task<SynthesizedRecipe> SynthesizeRecipe(List<Recipe> recipes, CookbookOrder order, string recipeName)
  {
    SynthesizedRecipe synthesizedRecipe;

    var synthesizingAssistantId = await llmService.CreateAssistant(synthesizeRecipePrompt);
    var analyzeAssistantId = await llmService.CreateAssistant(analyzeRecipePrompt);

    var synthesizingThreadId = await llmService.CreateThread();
    var analyzeThreadId = await llmService.CreateThread();

    string synthesizeRunId = null!;
    string analysisRunId = null!;

    logger.LogInformation("[{Recipe}] Synthesizing using recipes: {@Recipes}", recipeName, recipes);

    try
    {
      var synthesizePrompt = synthesizeRecipePrompt.GetUserPrompt(recipeName, recipes, order);

      await llmService.CreateMessage(synthesizingThreadId, synthesizePrompt);

      synthesizeRunId = await llmService.CreateRun(synthesizingThreadId, synthesizingAssistantId);

      var count = 0;
      string? analysisToolCallId = null;

      while (true)
      {
        count++;

        (var synthesizeToolCallId, synthesizedRecipe) =
          await ProcessSynthesisRun(synthesizingThreadId, synthesizeRunId);

        synthesizedRecipe.AttemptCount = count;
        synthesizedRecipe.Revisions ??= [];

        logger.LogInformation("[{Recipe} - Run {count}] Synthesized recipe: {@SynthesizedRecipe}", recipeName, count,
          synthesizedRecipe);

        if (count == 1)
        {
          var analysisPrompt = analyzeRecipePrompt.GetUserPrompt(synthesizedRecipe, order, recipeName);
          await llmService.CreateMessage(analyzeThreadId, analysisPrompt);

          analysisRunId = await llmService.CreateRun(analyzeThreadId, analyzeAssistantId);
        }
        else
        {
          await ProvideRevisedRecipe(analyzeThreadId, analysisRunId, analysisToolCallId!, synthesizedRecipe);
        }

        (analysisToolCallId, var analysisResult) = await ProcessAnalysisRun(analyzeThreadId, analysisRunId);

        logger.LogInformation("[{Recipe} - Run {count}] Analysis result: {@Analysis}",
          recipeName, count, analysisResult);

        synthesizedRecipe.AddAnalysisResult(analysisResult);

        if (analysisResult.QualityScore >= config.SynthesisQualityThreshold)
        {
          break;
        }

        // In case LLM fails to provide theses
        if (string.IsNullOrWhiteSpace(analysisResult.Suggestions))
        {
          analysisResult.Suggestions =
            "Please pay attention to what is desired from the cookbook order and synthesize another one.";
        }

        if (string.IsNullOrEmpty(analysisResult.Analysis))
        {
          analysisResult.Analysis = "The recipe is not suitable enough for the cookbook order.";
        }

        synthesizedRecipe.Revisions.Add(synthesizedRecipe);

        await ProvideAnalysisFeedback(synthesizingThreadId, synthesizeRunId, synthesizeToolCallId,
          analysisResult);
      }

      logger.LogInformation("[{Recipe}] Synthesized: {@Recipes}", recipeName, synthesizedRecipe);
    }
    catch (Exception ex)
    {
      logger.LogError(ex,
        "[{Recipe}] Error synthesizing recipe: {Message}. Synthesizer Assistant: {SynthesizerAssistant}, Thread: {SynthesizingThread}, Run: {SynthesizingRun}. Analyzer Assistant: {AnalyzerAssistant}, Thread: {AnalyzeThread}, Run: {AnalyzeRun}",
        recipeName, ex.Message, synthesizingAssistantId, synthesizingThreadId, synthesizeRunId,
        analyzeAssistantId, analyzeThreadId, analysisRunId);
      throw;
    }
    finally
    {
      await llmService.CancelRun(analyzeThreadId, analysisRunId);
      await llmService.CancelRun(synthesizingThreadId, synthesizeRunId);
      await llmService.DeleteAssistant(synthesizingAssistantId);
      await llmService.DeleteAssistant(analyzeAssistantId);
      await llmService.DeleteThread(synthesizingThreadId);
      await llmService.DeleteThread(analyzeThreadId);
    }

    return synthesizedRecipe;
  }

  private async Task<(string, SynthesizedRecipe)> ProcessSynthesisRun(string threadId, string runId)
  {
    while (true)
    {
      var (isComplete, status) = await llmService.GetRun(threadId, runId);

      if (isComplete)
      {
        throw new Exception("Synthesis run completed without producing a recipe");
      }

      if (status == RunStatus.RequiresAction)
      {
        var (toolCallId, result) = await llmService.GetRunAction<SynthesizedRecipe>(threadId, runId,
          synthesizeRecipePrompt.GetFunction().Name);

        result.InspiredBy ??= [];

        return (toolCallId, result);
      }

      await Task.Delay(TimeSpan.FromSeconds(1));
    }
  }

  private async Task<(string, RecipeAnalysis)> ProcessAnalysisRun(string threadId, string runId)
  {
    while (true)
    {
      var (isComplete, status) = await llmService.GetRun(threadId, runId);

      if (isComplete)
      {
        throw new Exception("Analysis run completed without producing a result");
      }

      if (status == RunStatus.RequiresAction)
      {
        return await llmService.GetRunAction<RecipeAnalysis>(threadId, runId,
          analyzeRecipePrompt.GetFunction().Name);
      }

      await Task.Delay(TimeSpan.FromSeconds(1));
    }
  }

  private async Task ProvideRevisedRecipe(string threadId, string runId, string toolCallId, SynthesizedRecipe recipe)
  {
    await llmService.SubmitToolOutputToRun(
      threadId,
      runId,
      toolCallId,
      JsonSerializer.Serialize(recipe)
    );
  }

  private async Task ProvideAnalysisFeedback(string threadId, string runId, string toolCallId,
    RecipeAnalysis analysis)
  {
    var toolOutput =
      $"""
         A new revision is required. Refer to the QA analysis:
         ```json
         {JsonSerializer.Serialize(analysis)}
         ```
         """.Trim();

    await llmService.SubmitToolOutputToRun(
      threadId,
      runId,
      toolCallId,
      toolOutput
    );
  }
}

public class NoRecipeException(List<string> previousAttempts) : Exception("No recipes found")
{
  public List<string> PreviousAttempts { get; } = previousAttempts;
}