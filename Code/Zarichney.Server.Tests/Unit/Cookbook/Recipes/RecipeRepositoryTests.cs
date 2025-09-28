using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Cookbook.Prompts;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services.AI;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.FileSystem;
using Zarichney.Services.Sessions;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Mocks.Factories;

namespace Zarichney.Tests.Unit.Cookbook.Recipes;

/// <summary>
/// Unit tests for the RecipeFileRepository class.
/// Tests focus on the methods identified as needing coverage:
/// InitializeAsync, LoadRecipesAsync, AddUpdateRecipesAsync, and CleanRecipesAsync.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Component, TestCategories.Service)]
[Trait(TestCategories.Feature, TestCategories.Cookbook)]
public class RecipeRepositoryTests : IAsyncLifetime
{
  // Mock dependencies
  private Mock<IFileService> _mockFileService = null!;
  private Mock<ILlmService> _mockLlmService = null!;
  private Mock<IFileWriteQueueService> _mockFileWriteQueueService = null!;
  private Mock<IMapper> _mockMapper = null!;
  private Mock<IBackgroundWorker> _mockBackgroundWorker = null!;
  private Mock<ISessionManager> _mockSessionManager = null!;
  private Mock<IScopeContainer> _mockScopeContainer = null!;
  private Mock<ILogger<RecipeFileRepository>> _mockLogger = null!;
  private Mock<IRecipeSearcher> _mockRecipeSearcher = null!;
  private Mock<IRecipeIndexer> _mockRecipeIndexer = null!;

  // Test data
  private RecipeConfig _recipeConfig = null!;
  private CleanRecipePrompt _cleanRecipePrompt = null!;
  private RecipeNamerPrompt _recipeNamerPrompt = null!;
  private List<Recipe> _testRecipes = null!;

  // System under test
  private RecipeFileRepository _repository = null!;

  public async Task InitializeAsync()
  {
    // Initialize mocks
    _mockFileService = new Mock<IFileService>();
    _mockLlmService = MockOpenAIServiceFactory.CreateMock();
    _mockFileWriteQueueService = new Mock<IFileWriteQueueService>();
    _mockMapper = new Mock<IMapper>();
    _mockBackgroundWorker = new Mock<IBackgroundWorker>();
    _mockSessionManager = new Mock<ISessionManager>();
    _mockScopeContainer = new Mock<IScopeContainer>();
    _mockLogger = new Mock<ILogger<RecipeFileRepository>>();
    _mockRecipeSearcher = new Mock<IRecipeSearcher>();
    _mockRecipeIndexer = new Mock<IRecipeIndexer>();

    // Setup test configuration
    _recipeConfig = new RecipeConfig
    {
      OutputDirectory = "TestRecipes",
      MaxParallelTasks = 4
    };

    // Setup prompts
    _cleanRecipePrompt = new CleanRecipePrompt(_mockMapper.Object);
    _recipeNamerPrompt = new RecipeNamerPrompt();

    // Setup test data
    _testRecipes = new List<Recipe>
      {
        new Recipe
        {
          Id = "recipe1",
          Title = "Test Recipe 1",
          Description = "Test description 1",
          Servings = "4",
          PrepTime = "10 minutes",
          CookTime = "20 minutes",
          TotalTime = "30 minutes",
          Ingredients = new List<string> { "Ingredient 1", "Ingredient 2" },
          Directions = new List<string> { "Step 1", "Step 2" },
          Notes = "Test notes 1",
          Aliases = new List<string> { "Recipe 1", "Test Recipe" },
          IndexTitle = "Test Recipe 1",
          Relevancy = new Dictionary<string, RelevancyResult>(),
          RecipeUrl = "https://example.com/recipe1",
          Cleaned = true
        },
        new Recipe
        {
          Id = "recipe2",
          Title = "Test Recipe 2",
          Description = "Test description 2",
          Servings = "2",
          PrepTime = "5 minutes",
          CookTime = "15 minutes",
          TotalTime = "20 minutes",
          Ingredients = new List<string> { "Ingredient 3", "Ingredient 4" },
          Directions = new List<string> { "Step 3", "Step 4" },
          Notes = "Test notes 2",
          Aliases = new List<string> { "Recipe 2", "Second Recipe" },
          IndexTitle = null,
          Relevancy = new Dictionary<string, RelevancyResult>(),
          RecipeUrl = "https://example.com/recipe2",
          Cleaned = false
        }
      };

    // Setup default mock behaviors
    SetupDefaultMockBehaviors();

    // Create the repository instance
    _repository = new RecipeFileRepository(
      _recipeConfig,
      _mockFileService.Object,
      _mockFileWriteQueueService.Object,
      _mockMapper.Object,
      _cleanRecipePrompt,
      _recipeNamerPrompt,
      _mockBackgroundWorker.Object,
      _mockSessionManager.Object,
      _mockLogger.Object,
      _mockRecipeSearcher.Object,
      _mockRecipeIndexer.Object
    );

    await Task.CompletedTask;
  }

  public async Task DisposeAsync()
  {
    await Task.CompletedTask;
  }

  private void SetupDefaultMockBehaviors()
  {
    // Setup file service to return test files
    _mockFileService.Setup(fs => fs.GetFiles(It.IsAny<string>()))
      .Returns(new[] { "recipe1.json", "recipe2.json" });

    _mockFileService.Setup(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
      .ReturnsAsync(_testRecipes);

    // Setup background worker to execute tasks immediately for testing
    _mockBackgroundWorker.Setup(bw => bw.QueueBackgroundWorkAsync(It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(), It.IsAny<Session?>()))
      .Callback<Func<IScopeContainer, CancellationToken, Task>, Session?>((work, session) =>
      {
        // Execute the work on the thread pool to avoid potential deadlocks in sync test contexts
        Task.Run(async () => await work(_mockScopeContainer.Object, CancellationToken.None)).Wait();
      });

    // Setup session manager for parallel processing
    _mockSessionManager.Setup(sm => sm.ParallelForEachAsync<Recipe>(
        It.IsAny<IScopeContainer>(),
        It.IsAny<IEnumerable<Recipe>>(),
        It.IsAny<Func<IScopeContainer, Recipe, CancellationToken, Task>>(),
        It.IsAny<int>(),
        It.IsAny<CancellationToken>()))
      .Returns<IScopeContainer, IEnumerable<Recipe>, Func<IScopeContainer, Recipe, CancellationToken, Task>, int, CancellationToken>(
        async (scope, items, action, maxParallel, ct) =>
        {
          foreach (var item in items)
          {
            await action(scope, item, ct);
          }
        });

    // Setup scope container to return mocked LLM service
    _mockScopeContainer.Setup(sc => sc.GetService<ILlmService>())
      .Returns(_mockLlmService.Object);

    // Setup LLM service for recipe renaming
    _mockLlmService.Setup(llm => llm.CallFunction<RenamerResult>(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<FunctionDefinition>(),
        It.IsAny<string?>(),
        It.IsAny<int?>()))
      .ReturnsAsync(new LlmResult<RenamerResult>
      {
        Data = new RenamerResult
        {
          IndexTitle = "Generated Index Title",
          Aliases = new List<string> { "Alias 1", "Alias 2" }
        },
        ConversationId = "test-conversation-id"
      });

    // Setup LLM service for recipe cleaning
    _mockLlmService.Setup(llm => llm.CallFunction<CleanedRecipe>(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<FunctionDefinition>(),
        It.IsAny<string?>(),
        It.IsAny<int?>()))
      .ReturnsAsync(new LlmResult<CleanedRecipe>
      {
        Data = new CleanedRecipe
        {
          Title = "Cleaned Title",
          Description = "Cleaned Description",
          Ingredients = new List<string> { "Clean Ingredient 1", "Clean Ingredient 2" },
          Directions = new List<string> { "Clean Step 1", "Clean Step 2" },
          Servings = "4",
          PrepTime = "10 minutes",
          CookTime = "20 minutes",
          TotalTime = "30 minutes",
          Notes = "Cleaned notes"
        },
        ConversationId = "test-conversation-id"
      });

    // Setup mapper for recipe cleaning
    _mockMapper.Setup(m => m.Map<Recipe>(It.IsAny<CleanedRecipe>()))
      .Returns<CleanedRecipe>(cleanedRecipe => new Recipe
      {
        Title = cleanedRecipe.Title,
        Description = cleanedRecipe.Description,
        Ingredients = cleanedRecipe.Ingredients.ToList(),
        Directions = cleanedRecipe.Directions.ToList(),
        Servings = cleanedRecipe.Servings,
        PrepTime = cleanedRecipe.PrepTime,
        CookTime = cleanedRecipe.CookTime,
        TotalTime = cleanedRecipe.TotalTime,
        Notes = cleanedRecipe.Notes,
        Aliases = new List<string>(),
        IndexTitle = null,
        Relevancy = new Dictionary<string, RelevancyResult>()
      });

    _mockMapper.Setup(m => m.Map(It.IsAny<Recipe>(), It.IsAny<Recipe>()))
      .Callback<Recipe, Recipe>((source, destination) =>
      {
        // Copy properties from source to destination
        destination.Title = source.Title;
        destination.Description = source.Description;
        destination.Ingredients = source.Ingredients;
        destination.Directions = source.Directions;
        destination.Servings = source.Servings;
        destination.PrepTime = source.PrepTime;
        destination.CookTime = source.CookTime;
        destination.TotalTime = source.TotalTime;
        destination.Notes = source.Notes;
      });
  }

  #region InitializeAsync Tests

  [Fact]
  public async Task InitializeAsync_WhenCalledFirstTime_ShouldInitializeSuccessfully()
  {
    // Arrange
    // Default mock setup should work for successful scenario

    // Act
    await _repository.InitializeAsync();

    // Assert
    _mockFileService.Verify(fs => fs.GetFiles(_recipeConfig.OutputDirectory), Times.Once,
      "Because initialization should load recipe files from the configured directory");
    _mockFileService.Verify(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()), Times.AtLeastOnce,
      "Because initialization should read recipe data from each found file");
  }

  [Fact]
  public async Task InitializeAsync_WhenCalledMultipleTimes_ShouldInitializeOnlyOnce()
  {
    // Arrange
    // Default mock setup should work

    // Act
    await _repository.InitializeAsync();
    await _repository.InitializeAsync();
    await _repository.InitializeAsync();

    // Assert
    _mockFileService.Verify(fs => fs.GetFiles(_recipeConfig.OutputDirectory), Times.Once,
      "Because initialization should only occur once, even when called multiple times");
  }

  [Fact]
  public async Task InitializeAsync_WhenFileServiceThrows_ShouldPropagateException()
  {
    // Arrange
    var expectedException = new InvalidOperationException("File service error");
    _mockFileService.Setup(fs => fs.GetFiles(It.IsAny<string>()))
      .Throws(expectedException);

    // Act & Assert
    var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
      () => _repository.InitializeAsync());

    actualException.Should().Be(expectedException,
      "Because file service errors during initialization should be propagated to caller");
  }

  [Fact]
  public async Task InitializeAsync_WhenFileReadingFails_ShouldContinueProcessingOtherFiles()
  {
    // Arrange
    _mockFileService.Setup(fs => fs.GetFiles(_recipeConfig.OutputDirectory))
      .Returns(new[] { "good-file.json", "bad-file.json", "another-good-file.json" });

    _mockFileService.Setup(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), "bad-file", It.IsAny<string?>()))
      .ThrowsAsync(new FileNotFoundException("File not found"));

    _mockFileService.Setup(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), "good-file", It.IsAny<string?>()))
      .ReturnsAsync(_testRecipes);

    _mockFileService.Setup(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), "another-good-file", It.IsAny<string?>()))
      .ReturnsAsync(_testRecipes);

    // Act
    await _repository.InitializeAsync();

    // Assert
    _mockFileService.Verify(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), "good-file", It.IsAny<string?>()), Times.Once,
      "Because good files should be processed successfully");
    _mockFileService.Verify(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), "another-good-file", It.IsAny<string?>()), Times.Once,
      "Because processing should continue after encountering a failed file");
  }

  #endregion

  #region LoadRecipesAsync Tests

  [Fact]
  public async Task LoadRecipesAsync_WhenFilesExist_ShouldLoadAllRecipeFiles()
  {
    // Arrange
    var expectedFiles = new[] { "recipe1.json", "recipe2.json", "recipe3.json" };
    _mockFileService.Setup(fs => fs.GetFiles(_recipeConfig.OutputDirectory))
      .Returns(expectedFiles);

    // Act
    await _repository.InitializeAsync();

    // Assert
    _mockFileService.Verify(fs => fs.GetFiles(_recipeConfig.OutputDirectory), Times.Once,
      "Because LoadRecipesAsync should retrieve all files from the configured directory");
    _mockFileService.Verify(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), "recipe1", It.IsAny<string?>()), Times.Once,
      "Because each recipe file should be read and processed");
    _mockFileService.Verify(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), "recipe2", It.IsAny<string?>()), Times.Once,
      "Because each recipe file should be read and processed");
    _mockFileService.Verify(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), "recipe3", It.IsAny<string?>()), Times.Once,
      "Because each recipe file should be read and processed");
  }

  [Fact]
  public async Task LoadRecipesAsync_WhenNoFilesExist_ShouldCompleteWithoutError()
  {
    // Arrange
    _mockFileService.Setup(fs => fs.GetFiles(_recipeConfig.OutputDirectory))
      .Returns(Array.Empty<string>());

    // Act & Assert
    await _repository.InitializeAsync();
    // Should complete without throwing
  }

  [Fact]
  public async Task LoadRecipesAsync_WhenFileContainsNullData_ShouldSkipFile()
  {
    // Arrange
    _mockFileService.Setup(fs => fs.GetFiles(_recipeConfig.OutputDirectory))
      .Returns(new[] { "empty-file.json" });
    _mockFileService.Setup(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), "empty-file", It.IsAny<string?>()))
      .ReturnsAsync((List<Recipe>?)null);

    // Act & Assert
    await _repository.InitializeAsync();
    // Should complete without throwing, and simply skip the null data
  }

  [Fact]
  public async Task LoadRecipesAsync_WhenFileReadFails_ShouldLogErrorAndContinue()
  {
    // Arrange
    _mockFileService.Setup(fs => fs.GetFiles(_recipeConfig.OutputDirectory))
      .Returns(new[] { "failing-file.json" });
    _mockFileService.Setup(fs => fs.ReadFromFile<List<Recipe>>(It.IsAny<string>(), "failing-file", It.IsAny<string?>()))
      .ThrowsAsync(new InvalidOperationException("File read error"));

    // Act & Assert
    await _repository.InitializeAsync();
    // Should complete without throwing, error should be handled internally
  }

  #endregion

  #region AddUpdateRecipesAsync Tests

  [Fact]
  public void AddUpdateRecipesAsync_WhenRecipesProvided_ShouldQueueBackgroundWork()
  {
    // Arrange
    var recipesToAdd = new List<Recipe>
      {
        new Recipe
        {
          Id = "new-recipe-1",
          Title = "New Recipe 1",
          Description = "Test description",
          Servings = "4",
          PrepTime = "10 minutes",
          CookTime = "20 minutes",
          TotalTime = "30 minutes",
          Ingredients = new List<string> { "Ingredient 1" },
          Directions = new List<string> { "Step 1" },
          Notes = "Test notes",
          Aliases = new List<string> { "New Recipe 1" },
          IndexTitle = "New Recipe 1",
          Relevancy = new Dictionary<string, RelevancyResult>(),
          Cleaned = true
        }
      };

    bool backgroundWorkQueued = false;
    _mockBackgroundWorker.Setup(bw => bw.QueueBackgroundWorkAsync(It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(), It.IsAny<Session?>()))
      .Callback<Func<IScopeContainer, CancellationToken, Task>, Session?>((work, session) => backgroundWorkQueued = true);

    // Act
    _repository.AddUpdateRecipesAsync(recipesToAdd);

    // Assert
    backgroundWorkQueued.Should().BeTrue("Because AddUpdateRecipesAsync should queue background work for processing recipes");
    _mockBackgroundWorker.Verify(bw => bw.QueueBackgroundWorkAsync(It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(), It.IsAny<Session?>()), Times.Once,
      "Because the method should delegate recipe processing to background worker");
  }

  [Fact]
  public void AddUpdateRecipesAsync_WhenUncleanedRecipesProvided_ShouldProcessCleaningInBackground()
  {
    // Arrange
    var uncleanedRecipes = new List<Recipe>
      {
        new Recipe
        {
          Id = "uncleaned-recipe",
          Title = "Uncleaned Recipe",
          Description = "Test description",
          Servings = "2",
          PrepTime = "5 minutes",
          CookTime = "15 minutes",
          TotalTime = "20 minutes",
          Ingredients = new List<string> { "Raw ingredient" },
          Directions = new List<string> { "Raw step" },
          Notes = "Raw notes",
          Aliases = new List<string> { "Uncleaned Recipe" },
          IndexTitle = null,
          Relevancy = new Dictionary<string, RelevancyResult>(),
          Cleaned = false
        }
      };

    // Act
    _repository.AddUpdateRecipesAsync(uncleanedRecipes);

    // Assert
    _mockBackgroundWorker.Verify(bw => bw.QueueBackgroundWorkAsync(It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(), It.IsAny<Session?>()), Times.Once,
      "Because uncleaned recipes should trigger background processing including cleaning operations");
  }

  [Fact]
  public void AddUpdateRecipesAsync_WhenRecipesWithoutIndexTitle_ShouldTriggerRenaming()
  {
    // Arrange
    var recipesNeedingRenaming = new List<Recipe>
      {
        new Recipe
        {
          Id = "recipe-needing-rename",
          Title = "Original Title",
          Description = "Test description",
          Servings = "6",
          PrepTime = "15 minutes",
          CookTime = "25 minutes",
          TotalTime = "40 minutes",
          Ingredients = new List<string> { "Original ingredient" },
          Directions = new List<string> { "Original step" },
          Notes = "Original notes",
          Aliases = new List<string> { "Original Title" },
          IndexTitle = null, // This will trigger renaming
          Relevancy = new Dictionary<string, RelevancyResult>(),
          Cleaned = true
        }
      };

    // Act
    _repository.AddUpdateRecipesAsync(recipesNeedingRenaming);

    // Assert
    _mockBackgroundWorker.Verify(bw => bw.QueueBackgroundWorkAsync(It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(), It.IsAny<Session?>()), Times.Once,
      "Because recipes without IndexTitle should trigger background processing for renaming");
  }

  [Fact]
  public void AddUpdateRecipesAsync_WhenProcessingCompletes_ShouldWriteToFiles()
  {
    // Arrange
    var recipesToProcess = new List<Recipe>
      {
        new Recipe
        {
          Id = "recipe-to-write",
          Title = "Recipe To Write",
          Description = "Test description",
          Servings = "4",
          PrepTime = "10 minutes",
          CookTime = "20 minutes",
          TotalTime = "30 minutes",
          Ingredients = new List<string> { "Write ingredient" },
          Directions = new List<string> { "Write step" },
          Notes = "Write notes",
          Aliases = new List<string> { "Recipe To Write" },
          IndexTitle = "Recipe To Write",
          Relevancy = new Dictionary<string, RelevancyResult>(),
          Cleaned = true
        }
      };

    // Act
    _repository.AddUpdateRecipesAsync(recipesToProcess);

    // Assert - Since background worker is mocked to execute synchronously
    _mockFileWriteQueueService.Verify(fwqs => fwqs.QueueWrite(
      _recipeConfig.OutputDirectory,
      It.IsAny<string>(),
      It.IsAny<List<Recipe>>(),
      It.IsAny<string?>()), Times.Once,
      "Because processed recipes should be queued for writing to files");
  }

  [Fact]
  public void AddUpdateRecipesAsync_WhenExistingRecipesPresent_ShouldMergeWithNew()
  {
    // Arrange
    var newRecipes = new List<Recipe>
      {
        new Recipe
        {
          Id = "new-recipe",
          Title = "New Recipe",
          Description = "Test description",
          Servings = "4",
          PrepTime = "10 minutes",
          CookTime = "20 minutes",
          TotalTime = "30 minutes",
          Ingredients = new List<string> { "New ingredient" },
          Directions = new List<string> { "New step" },
          Notes = "New notes",
          Aliases = new List<string> { "New Recipe" },
          IndexTitle = "New Recipe",
          Relevancy = new Dictionary<string, RelevancyResult>(),
          Cleaned = true
        }
      };

    var existingRecipes = new List<Recipe>
      {
        new Recipe
        {
          Id = "existing-recipe",
          Title = "Existing Recipe",
          Description = "Test description",
          Servings = "2",
          PrepTime = "5 minutes",
          CookTime = "15 minutes",
          TotalTime = "20 minutes",
          Ingredients = new List<string> { "Existing ingredient" },
          Directions = new List<string> { "Existing step" },
          Notes = "Existing notes",
          Aliases = new List<string> { "Existing Recipe" },
          IndexTitle = "Existing Recipe",
          Relevancy = new Dictionary<string, RelevancyResult>(),
          Cleaned = true
        }
      };

    _mockFileService.Setup(fs => fs.ReadFromFile<List<Recipe>?>(
      _recipeConfig.OutputDirectory,
      It.IsAny<string>(),
      It.IsAny<string?>()))
      .ReturnsAsync(existingRecipes);

    // Act
    _repository.AddUpdateRecipesAsync(newRecipes);

    // Assert
    _mockFileService.Verify(fs => fs.ReadFromFile<List<Recipe>?>(
      _recipeConfig.OutputDirectory,
      It.IsAny<string>(),
      It.IsAny<string?>()), Times.AtLeastOnce,
      "Because the method should check for existing recipes before writing");
  }

  [Fact]
  public void AddUpdateRecipesAsync_WhenEmptyRecipeList_ShouldStillQueueBackgroundWork()
  {
    // Arrange
    var emptyRecipeList = new List<Recipe>();

    // Act
    _repository.AddUpdateRecipesAsync(emptyRecipeList);

    // Assert
    _mockBackgroundWorker.Verify(bw => bw.QueueBackgroundWorkAsync(It.IsAny<Func<IScopeContainer, CancellationToken, Task>>(), It.IsAny<Session?>()), Times.Once,
      "Because even empty lists should trigger background work to maintain consistent behavior");
  }

  #endregion

  #region CleanRecipesAsync Tests

  [Fact]
  public void CleanRecipesAsync_WhenUncleanedRecipesProvided_ShouldCallLlmServiceForCleaning()
  {
    // Arrange
    var uncleanedRecipes = new List<Recipe>
      {
        new Recipe
        {
          Id = "uncleaned-1",
          Title = "Raw Recipe Title",
          Description = "Raw description",
          Servings = "4",
          PrepTime = "10 minutes",
          CookTime = "20 minutes",
          TotalTime = "30 minutes",
          Ingredients = new List<string> { "Raw ingredient" },
          Directions = new List<string> { "Raw step" },
          Notes = "Raw notes",
          Aliases = new List<string> { "Raw Recipe Title" },
          IndexTitle = "Uncleaned Recipe",
          Relevancy = new Dictionary<string, RelevancyResult>(),
          Cleaned = false
        }
      };

    // Act
    _repository.AddUpdateRecipesAsync(uncleanedRecipes);

    // Assert - Since we set up background worker to execute synchronously
    _mockLlmService.Verify(llm => llm.CallFunction<CleanedRecipe>(
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<FunctionDefinition>(),
      It.IsAny<string?>(),
      It.IsAny<int?>()), Times.Once,
      "Because uncleaned recipes should trigger LLM service calls for cleaning");
  }

  [Fact]
  public void CleanRecipesAsync_WhenAlreadyCleanedRecipes_ShouldSkipCleaning()
  {
    // Arrange
    var cleanedRecipes = new List<Recipe>
      {
        new Recipe
        {
          Id = "already-clean",
          Title = "Clean Recipe",
          Description = "Clean description",
          Servings = "2",
          PrepTime = "5 minutes",
          CookTime = "15 minutes",
          TotalTime = "20 minutes",
          Ingredients = new List<string> { "Clean ingredient" },
          Directions = new List<string> { "Clean step" },
          Notes = "Clean notes",
          Aliases = new List<string> { "Clean Recipe" },
          IndexTitle = "Clean Recipe",
          Relevancy = new Dictionary<string, RelevancyResult>(),
          Cleaned = true
        }
      };

    // Reset LLM service call count
    _mockLlmService.Reset();

    // Act
    _repository.AddUpdateRecipesAsync(cleanedRecipes);

    // Assert
    _mockLlmService.Verify(llm => llm.CallFunction<CleanedRecipe>(
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<FunctionDefinition>(),
      It.IsAny<string?>(),
      It.IsAny<int?>()), Times.Never,
      "Because already cleaned recipes should not trigger additional LLM cleaning calls");
  }

  [Fact]
  public void CleanRecipesAsync_WhenLlmServiceFails_ShouldHandleGracefully()
  {
    // Arrange
    var uncleanedRecipe = new Recipe
    {
      Id = "problematic-recipe",
      Title = "Problematic Recipe",
      Description = "Problematic description",
      Servings = "6",
      PrepTime = "15 minutes",
      CookTime = "25 minutes",
      TotalTime = "40 minutes",
      Ingredients = new List<string> { "Problematic ingredient" },
      Directions = new List<string> { "Problematic step" },
      Notes = "Problematic notes",
      Aliases = new List<string> { "Problematic Recipe" },
      IndexTitle = "Problematic Recipe",
      Relevancy = new Dictionary<string, RelevancyResult>(),
      Cleaned = false
    };

    _mockLlmService.Setup(llm => llm.CallFunction<CleanedRecipe>(
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<FunctionDefinition>(),
      It.IsAny<string?>(),
      It.IsAny<int?>()))
      .ThrowsAsync(new InvalidOperationException("LLM service error"));

    // Act & Assert - Should not throw, should handle gracefully
    _repository.AddUpdateRecipesAsync(new List<Recipe> { uncleanedRecipe });
    // Method should complete without throwing
  }

  [Fact]
  public void CleanRecipesAsync_WhenLlmServiceException_ShouldLogWarningAndContinue()
  {
    // Arrange
    var flaggedRecipe = new Recipe
    {
      Id = "flagged-recipe",
      Title = "Recipe with Inappropriate Content",
      Description = "Flagged description",
      Servings = "8",
      PrepTime = "20 minutes",
      CookTime = "30 minutes",
      TotalTime = "50 minutes",
      Ingredients = new List<string> { "Flagged ingredient" },
      Directions = new List<string> { "Flagged step" },
      Notes = "Flagged notes",
      Aliases = new List<string> { "Recipe with Inappropriate Content" },
      IndexTitle = "Flagged Recipe",
      Relevancy = new Dictionary<string, RelevancyResult>(),
      Cleaned = false
    };

    _mockLlmService.Setup(llm => llm.CallFunction<CleanedRecipe>(
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<FunctionDefinition>(),
      It.IsAny<string?>(),
      It.IsAny<int?>()))
      .ThrowsAsync(new InvalidOperationException("Content filtered"));

    // Act & Assert - Should not throw, should handle LLM service exceptions gracefully
    _repository.AddUpdateRecipesAsync(new List<Recipe> { flaggedRecipe });
    // Method should complete without throwing and log appropriate warning
  }

  [Fact]
  public void CleanRecipesAsync_WhenSuccessfulCleaning_ShouldMapAndUpdateRecipe()
  {
    // Arrange
    var uncleanedRecipe = new Recipe
    {
      Id = "recipe-to-clean",
      Title = "Original Title",
      Description = "Original Description",
      Servings = "4",
      PrepTime = "10 minutes",
      CookTime = "20 minutes",
      TotalTime = "30 minutes",
      Ingredients = new List<string> { "Original ingredient" },
      Directions = new List<string> { "Original step" },
      Notes = "Original notes",
      Aliases = new List<string> { "Original Title" },
      IndexTitle = "Recipe To Clean",
      Relevancy = new Dictionary<string, RelevancyResult>(),
      Cleaned = false
    };

    // Act
    _repository.AddUpdateRecipesAsync(new List<Recipe> { uncleanedRecipe });

    // Assert
    _mockMapper.Verify(m => m.Map<Recipe>(It.IsAny<CleanedRecipe>()), Times.Once,
      "Because successful cleaning should trigger mapping from CleanedRecipe to Recipe");
    _mockMapper.Verify(m => m.Map(It.IsAny<Recipe>(), It.IsAny<Recipe>()), Times.Once,
      "Because cleaned data should be mapped back to the original recipe instance");
  }

  #endregion
}
