using System.Collections.Concurrent;
using AutoMapper;
using Zarichney.Server.Cookbook.Prompts;
using Zarichney.Server.Services.AI;
using Zarichney.Server.Services.BackgroundTasks;
using Zarichney.Server.Services.FileSystem;
using Zarichney.Server.Services.Sessions;

namespace Zarichney.Server.Cookbook.Recipes;

public interface IRecipeRepository
{
  Task InitializeAsync();

  Task<List<Recipe>> SearchRecipes(string? query, int? minimumScore = null, int? requiredCount = null,
    CancellationToken ct = default);

  void AddUpdateRecipesAsync(List<Recipe> recipes);
  bool ContainsRecipe(string recipeId);
  bool ContainsRecipeUrl(string url);
}

public class RecipeFileRepository(
  RecipeConfig config,
  IFileService fileService,
  IMapper mapper,
  CleanRecipePrompt cleanRecipePrompt,
  RecipeNamerPrompt recipeNamerPrompt,
  IBackgroundWorker worker,
  ISessionManager sessionManager,
  ILogger<RecipeFileRepository> logger,
  IRecipeSearcher searcher,
  IRecipeIndexer recipeIndexer
)
  : IRecipeRepository
{
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

  public bool ContainsRecipeUrl(string url)
  {
    var id = WebScraperService.GenerateUrlFingerprint(url);
    return ContainsRecipe(id);
  }

  public bool ContainsRecipe(string recipeId)
  {
    return recipeIndexer.GetAllRecipes()
      .Any(kvp => kvp.Value.ContainsKey(recipeId));
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

    recipeIndexer.AddRecipe(recipe);
  }

  public async Task<List<Recipe>> SearchRecipes(
    string? query,
    int? minimumScore = null,
    int? requiredCount = null,
    CancellationToken ct = default
  )
  {
    if (!_isInitialized)
    {
      logger.LogWarning("Attempting to search before initialization. Initializing now.");
      await InitializeAsync();
    }

    return await searcher.SearchRecipes(query!, minimumScore, requiredCount, ct);
  }

  public void AddUpdateRecipesAsync(List<Recipe> recipes)
  {
    // Process in the background, creating a new scope for this work
    worker.QueueBackgroundWorkAsync(async (parentScope, ct) =>
    {
      await CleanUncleanedRecipesAsync(parentScope, recipes);

      // Process recipes and organize them into new files
      var filesToWrite = new ConcurrentDictionary<string, ConcurrentBag<Recipe>>();

      await sessionManager.ParallelForEachAsync(parentScope, recipes, async (scope, recipe, _) =>
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
      }, config.MaxParallelTasks, ct);

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
          existingRecipe.Directions = newRecipe.Directions;
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
        var llmResult = await llmService.CallFunction<RenamerResult>(
          recipeNamerPrompt.SystemPrompt,
          recipeNamerPrompt.GetUserPrompt(recipe),
          recipeNamerPrompt.GetFunction()
        );

        // Extract the RenamerResult data from the LlmResult
        var result = llmResult.Data;

        logger.LogInformation("Received response from model for recipe {RecipeTitle}: {@Result}", recipe.Title,
          result);

        recipe.Aliases = result.Aliases.ToList();
        recipe.IndexTitle = result.IndexTitle;
        recipe.Title = recipe.Title?.Trim();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error processing recipe '{RecipeId}'", recipe.Id);
      }
    }
  }

  private async Task CleanUncleanedRecipesAsync(IScopeContainer scope, List<Recipe> recipes)
  {
    var uncleanedRecipes = recipes.Where(r => !r.Cleaned).ToList();
    if (uncleanedRecipes.Count != 0)
    {
      var cleanedRecipes = await CleanRecipesAsync(scope, uncleanedRecipes);

      // Remove uncleaned recipes from the list
      recipes.RemoveAll(r => !r.Cleaned);

      // Add cleaned recipes to the list
      recipes.AddRange(cleanedRecipes);
    }
  }

  private async Task<List<Recipe>> CleanRecipesAsync(IScopeContainer parentScope, List<Recipe> recipes)
  {
    if (recipes.Count == 0)
    {
      return [];
    }

    var cleanedRecipes = new ConcurrentBag<Recipe>();

    await sessionManager.ParallelForEachAsync(parentScope, recipes, async (scope, recipe, _) =>
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
    }, config.MaxParallelTasks);

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
      var llmResult = await llmService.CallFunction<CleanedRecipe>(
        cleanRecipePrompt.SystemPrompt,
        cleanRecipePrompt.GetUserPrompt(recipe),
        cleanRecipePrompt.GetFunction(),
        null,
        1 // Don't retry
      );

      var newRecipe = llmResult.Data;

      logger.LogInformation("Cleaned recipe data: {@CleanedRecipe}", newRecipe);

      // Create a new Recipe instance
      var cleanedData = mapper.Map<Recipe>(newRecipe);

      // Copy over properties not part of CleanedRecipe
      mapper.Map(cleanedData, recipe);

      // Ensure Cleaned flag is set
      recipe.Cleaned = true;

      return recipe;
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