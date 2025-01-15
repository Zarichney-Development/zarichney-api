using System.Collections.Concurrent;
using AutoMapper;
using Zarichney.Cookbook.Models;
using Zarichney.Cookbook.Prompts;
using Zarichney.Cookbook.Services;
using Zarichney.Services;
using Zarichney.Services.Sessions;

namespace Zarichney.Cookbook.Repositories;

public interface IRecipeRepository
{
  Task InitializeAsync();
  Task<List<Recipe>> SearchRecipes(string? query);
  void AddUpdateRecipesAsync(List<Recipe> recipes);
  bool ContainsRecipe(string recipeId);
}

public class RecipeFileRepository(
  RecipeConfig config,
  IFileService fileService,
  IMapper mapper,
  CleanRecipePrompt cleanRecipePrompt,
  RecipeNamerPrompt recipeNamerPrompt,
  IBackgroundWorker worker,
  ISessionManager sessionManager,
  ILogger<RecipeFileRepository> logger
)
  : IRecipeRepository
{
  private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Recipe>> _recipes =
    new(StringComparer.OrdinalIgnoreCase);

  private readonly ConcurrentDictionary<string, byte> _recipeIds = new();

  private readonly SemaphoreSlim _initializationLock = new(1, 1);
  private bool _isInitialized;

  public async Task InitializeAsync()
  {
    if (_isInitialized) return;

    await _initializationLock.WaitAsync();
    try
    {
      if (_isInitialized) return;

      logger.LogInformation("Initializing RecipeRepository...");
      await LoadRecipesAsync();
      _isInitialized = true;
      logger.LogInformation("RecipeRepository initialized successfully.");
    }
    finally
    {
      _initializationLock.Release();
    }
  }

  private async Task LoadRecipesAsync()
  {
    try
    {
      var recipeFiles = fileService.GetFiles(config.OutputDirectory);
      logger.LogInformation("Found {count} recipe files.", recipeFiles.Length);

      var loadTasks = recipeFiles
        .Where(fileName => fileName == "Data/Recipes\\Banana_Bread.json")
        .Select(async file =>
        {
          var fileName = Path.GetFileNameWithoutExtension(file);
          try
          {
            var recipes = await fileService.ReadFromFile<List<Recipe>>(config.OutputDirectory, fileName);
            foreach (var recipe in recipes)
            {
              AddRecipeToRepository(recipe);
            }
          }
          catch (Exception ex)
          {
            logger.LogError(ex, "Error loading recipes from file: {FileName}", fileName);
          }
        });

      await Task.WhenAll(loadTasks);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error loading recipes");
      throw;
    }
  }

  public bool ContainsRecipe(string recipeId)
  {
    return _recipeIds.ContainsKey(recipeId);
  }

  private void AddRecipeToRepository(Recipe recipe)
  {
    if (string.IsNullOrEmpty(recipe.Id))
    {
      if (string.IsNullOrEmpty(recipe.RecipeUrl))
      {
        logger.LogWarning("Recipe with missing ID and URL cannot be added.");
        return;
      }

      recipe.Id = WebScraperService.GenerateUrlFingerprint(recipe.RecipeUrl);
    }

    _recipeIds.TryAdd(recipe.Id, 0);

    // For each title & alias, map this recipe to the dictionary for fast access
    AddToDictionary(recipe.Title!, recipe);
    foreach (var alias in recipe.Aliases)
    {
      AddToDictionary(alias, recipe);
    }
  }

  private void AddToDictionary(string key, Recipe recipe)
  {
    var recipeDict = _recipes.GetOrAdd(key, _ => new ConcurrentDictionary<string, Recipe>());
    recipeDict.AddOrUpdate(recipe.Id!, recipe, (_, _) => recipe);
  }

  public async Task<List<Recipe>> SearchRecipes(string? query)
  {
    if (!_isInitialized)
    {
      logger.LogWarning("Attempting to search before initialization. Initializing now.");
      await InitializeAsync();
    }

    if (string.IsNullOrWhiteSpace(query))
    {
      throw new ArgumentException("Search query cannot be empty", nameof(query));
    }

    logger.LogInformation("Looking up repository for recipes matching query: {Query}", query);

    return await Task.Run(() =>
    {
      var results = new ConcurrentDictionary<string, Recipe>();

      // Exact matches
      if (_recipes.TryGetValue(query, out var exactMatches))
      {
        foreach (var recipe in exactMatches.Values)
        {
          results.TryAdd(recipe.Id!, recipe);
        }
      }

      // Fuzzy matches
      foreach (var (key, value) in _recipes)
      {
        if (key.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            query.Contains(key, StringComparison.OrdinalIgnoreCase))
        {
          foreach (var recipe in value.Values)
          {
            results.TryAdd(recipe.Id!, recipe);
          }
        }
      }

      var recipes = results.Values
        .OrderByDescending(r => CalculateRelevanceScore(r, query))
        .ToList();

      logger.LogInformation("Found {count} recipes matching query: {query}", recipes.Count, query);

      return recipes;
    });
  }

  public void AddUpdateRecipesAsync(List<Recipe> recipes)
  {
    // Process in the background, creating a new scope for this work
    worker.QueueBackgroundWorkAsync(async (_, cancellationToken) =>
    {
      await CleanUncleanedRecipesAsync(recipes);

      // Process recipes and organize them into new files
      var filesToWrite = new ConcurrentDictionary<string, ConcurrentBag<Recipe>>();

      await sessionManager.ParallelForEachAsync(recipes, async (scope, recipe, _) =>
      {
        try
        {
          await IndexAndRenameRecipeAsync(scope, recipe);

          // Add to filesToWrite
          var recipeBag = filesToWrite.GetOrAdd(recipe.IndexTitle!, _ => []);
          recipeBag.Add(recipe);
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error processing recipe '{RecipeId}'", recipe.Id);
        }
      }, config.MaxParallelTasks, cancellationToken);

      foreach (var (title, recipeBag) in filesToWrite)
      {
        var recipeList = recipeBag.ToList();

        try
        {
          foreach (var recipe in recipeList)
          {
            AddRecipeToRepository(recipe);
          }

          var existingRecipes =
            await fileService.ReadFromFile<List<Recipe>?>(config.OutputDirectory, title) ?? [];

          var combinedRecipes = UpdateExistingRecipes(existingRecipes, recipeList);

          fileService.WriteToFileAsync(config.OutputDirectory, title, combinedRecipes);
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error writing recipes to file: {Title}", title);
        }
      }
    });
  }

  private List<Recipe> UpdateExistingRecipes(List<Recipe> existingRecipes, List<Recipe> newRecipes)
  {
    // Create a dictionary for existing recipes by ID
    var existingRecipesDict = existingRecipes.ToDictionary(r => r.Id!);

    foreach (var newRecipe in newRecipes)
    {
      var recipeId = newRecipe.Id!;
      if (existingRecipesDict.TryGetValue(recipeId, out var existingRecipe))
      {
        if (newRecipe.Relevancy.Count == 1)
        {
          // Scenario: newly scraped being added
          // Merge the new entry into the existing dictionary
          existingRecipe.Relevancy[newRecipe.Relevancy.Keys.First()] = newRecipe.Relevancy.Values.First();
        }
        else
        {
          // Scenario: The new one was pulled from storage, and a new ranking has been added
          // Replace the existing entry with the new one
          existingRecipe.Relevancy = newRecipe.Relevancy;
        }

        if (!existingRecipe.Cleaned && newRecipe.Cleaned)
        {
          existingRecipe.Title = newRecipe.Title;
          existingRecipe.Description = newRecipe.Description;
          existingRecipe.Ingredients = newRecipe.Ingredients;
          existingRecipe.Servings = newRecipe.Servings;
          existingRecipe.CookTime = newRecipe.CookTime;
          existingRecipe.PrepTime = newRecipe.PrepTime;
          existingRecipe.TotalTime = newRecipe.TotalTime;
          existingRecipe.Notes = newRecipe.Notes;
          existingRecipe.Cleaned = true;
        }
      }
      else
      {
        // Add new recipe
        existingRecipesDict[recipeId] = newRecipe;
      }
    }

    // Return the combined list
    return existingRecipesDict.Values.ToList();
  }

  private async Task IndexAndRenameRecipeAsync(IScopeContainer scope, Recipe recipe)
  {
    if (string.IsNullOrEmpty(recipe.IndexTitle))
    {
      try
      {
        var llmService = scope.GetService<ILlmService>();
        var result = await llmService.CallFunction<RenamerResult>(
          recipeNamerPrompt.SystemPrompt,
          recipeNamerPrompt.GetUserPrompt(recipe),
          recipeNamerPrompt.GetFunction()
        );

        logger.LogInformation("Received response from model for recipe {RecipeTitle}: {@Result}", recipe.Title,
          result);

        recipe.Aliases = result.Aliases.Select(a => a.Replace("Print Pin It", "").Trim()).ToList();
        recipe.IndexTitle = result.IndexTitle;
        recipe.Title = recipe.Title?.Replace("Print Pin It", "").Trim();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error processing recipe '{RecipeId}'", recipe.Id);
      }
    }
  }

  private static double CalculateRelevanceScore(Recipe recipe, string query)
  {
    // Simple relevance scoring based on title match
    if (recipe.Title!.Equals(query, StringComparison.OrdinalIgnoreCase))
      return 1.0;
    if (recipe.Title!.Contains(query, StringComparison.OrdinalIgnoreCase))
      return 0.8;
    if (recipe.Aliases.Any(a => a.Equals(query, StringComparison.OrdinalIgnoreCase)))
      return 0.6;
    return recipe.Aliases.Any(a => a.Contains(query, StringComparison.OrdinalIgnoreCase))
      ? 0.4
      : 0.2; // Fallback score for partial matches
  }

  private async Task CleanUncleanedRecipesAsync(List<Recipe> recipes)
  {
    var uncleanedRecipes = recipes.Where(r => !r.Cleaned).ToList();
    if (uncleanedRecipes.Count != 0)
    {
      var cleanedRecipes = await CleanRecipesAsync(uncleanedRecipes);

      // Remove uncleaned recipes from the list
      recipes.RemoveAll(r => !r.Cleaned);

      // Add cleaned recipes to the list
      recipes.AddRange(cleanedRecipes);
    }
  }

  private async Task<List<Recipe>> CleanRecipesAsync(List<Recipe> recipes)
  {
    if (recipes.Count == 0)
    {
      return [];
    }

    var cleanedRecipes = new ConcurrentBag<Recipe>();

    await sessionManager.ParallelForEachAsync(recipes, async (scope, recipe, _) =>
      {
        try
        {
          var cleanedRecipe = await CleanRecipeData(scope, recipe);
          cleanedRecipes.Add(cleanedRecipe);
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error cleaning recipe with Id: {RecipeId}", recipe.Id);
        }
      },
      config.MaxParallelTasks);

    return cleanedRecipes.ToList();
  }

  private async Task<Recipe> CleanRecipeData(IScopeContainer scope, Recipe recipe)
  {
    if (recipe.Cleaned)
    {
      return recipe;
    }

    try
    {
      var llmService = scope.GetService<ILlmService>();
      var cleanedRecipe = await llmService.CallFunction<CleanedRecipe>(
        cleanRecipePrompt.SystemPrompt,
        cleanRecipePrompt.GetUserPrompt(recipe),
        cleanRecipePrompt.GetFunction(),
        null,
        1 // Don't retry
      );

      logger.LogInformation("Cleaned recipe data: {@CleanedRecipe}", cleanedRecipe);

      // Create a new Recipe instance
      var newRecipe = mapper.Map<Recipe>(cleanedRecipe);

      // Copy over properties not part of CleanedRecipe
      mapper.Map(recipe, newRecipe);

      // Ensure Cleaned flag is set
      newRecipe.Cleaned = true;

      return newRecipe;
    }
    catch (OpenAiContentFilterException ex)
    {
      logger.LogWarning(ex, "Unable to clean recipe data due to getting flagged by content filtering");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error cleaning recipe data: {Message}", ex.Message);
    }

    // Return the original recipe
    return recipe;
  }
}