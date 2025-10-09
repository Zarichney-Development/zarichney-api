using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Cookbook.Prompts;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services.AI;
using Zarichney.Services.Sessions;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Mocks.Factories;

namespace Zarichney.Tests.Unit.Cookbook.Recipes
{
  /// <summary>
  /// Unit tests for the RecipeService class.
  /// </summary>
  [Trait(TestCategories.Category, TestCategories.Unit)]
  [Trait(TestCategories.Component, TestCategories.Service)]
  [Trait(TestCategories.Feature, TestCategories.Cookbook)]
  public class RecipeServiceTests
  {
    private readonly Mock<IRecipeRepository> _mockRepository;
    private readonly Mock<IWebScraperService> _mockWebScraper;
    private readonly Mock<IMapper> _mockMapper;

    // Test data fields
    private readonly List<Recipe> _recipes;
    private readonly string _query = "chicken soup";
    private readonly int _acceptableScore = 70;
    private readonly int _requiredCount = 3;

    private readonly IRecipeService _service;

    public RecipeServiceTests()
    {
      // Arrange - Setup mocks
      _mockRepository = new Mock<IRecipeRepository>();
      Mock<ILlmService> mockLlmService = MockOpenAIServiceFactory.CreateMock();
      _mockWebScraper = new Mock<IWebScraperService>();
      _mockMapper = new Mock<IMapper>();
      Mock<ISessionManager> mockSessionManager = new();
      Mock<IScopeContainer> mockScopeContainer = new();
      Mock<ILogger<RecipeService>> mockLogger = new();
      var rankRecipePrompt = new RankRecipePrompt();
      var synthesizeRecipePrompt = new SynthesizeRecipePrompt(_mockMapper.Object);
      var analyzeRecipePrompt = new AnalyzeRecipePrompt();
      var getAlternativeQueryPrompt = new GetAlternativeQueryPrompt();

      // Setup test data
      _recipes =
      [
        new Recipe
        {
          Id = "recipe1",
          Title = "Chicken Noodle Soup",
          Description = "A classic chicken soup",
          Ingredients = ["Chicken", "Noodles", "Broth"],
          Directions = ["Boil water", "Add ingredients", "Simmer"],
          IndexTitle = "Chicken Noodle Soup",
          Servings = "4",
          PrepTime = "10 minutes",
          CookTime = "30 minutes",
          TotalTime = "40 minutes",
          Notes = "",
          Aliases = [],
          Relevancy = new Dictionary<string, RelevancyResult>
          {
            {
              _query, new RelevancyResult { Query = _query, Score = 85, Reasoning = "Great match for chicken soup" }
            }
          }
        },

        new Recipe
        {
          Id = "recipe2",
          Title = "Basic Broth",
          Description = "A simple broth",
          Ingredients = ["Water", "Vegetables"],
          Directions = ["Boil water", "Add vegetables", "Simmer"],
          IndexTitle = "Basic Broth",
          Servings = "4",
          PrepTime = "5 minutes",
          CookTime = "20 minutes",
          TotalTime = "25 minutes",
          Notes = "",
          Aliases = [],
          Relevancy = new Dictionary<string, RelevancyResult>
          {
            {
              _query,
              new RelevancyResult { Query = _query, Score = 60, Reasoning = "Not a complete match for chicken soup" }
            }
          }
        },

        new Recipe
        {
          Id = "recipe3",
          Title = "Chicken Stock",
          Description = "A rich chicken stock",
          Ingredients = ["Chicken bones", "Vegetables", "Water"],
          Directions = ["Roast bones", "Add water", "Simmer"],
          IndexTitle = "Chicken Stock",
          Servings = "8",
          PrepTime = "15 minutes",
          CookTime = "4 hours",
          TotalTime = "4 hours 15 minutes",
          Notes = "",
          Aliases = [],
          Relevancy = new Dictionary<string, RelevancyResult>
          {
            {
              _query,
              new RelevancyResult { Query = _query, Score = 80, Reasoning = "Good match for chicken soup base" }
            }
          }
        }
      ];

      // Setup default repository search to return test data
      _mockRepository.Setup(r => r.SearchRecipes(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(_recipes);

      // Setup default LLM service behavior
      mockLlmService.Setup(s => s.CallFunction<RelevancyResult>(
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<FunctionDefinition>(),
          It.IsAny<string?>(),
          It.IsAny<int?>()))
          .ReturnsAsync(new LlmResult<RelevancyResult>
          {
            Data = new RelevancyResult { Query = _query, Score = 90, Reasoning = "Perfect match" },
            ConversationId = Guid.NewGuid().ToString()
          });

      // Setup default web scraper to return empty by default
      _mockWebScraper.Setup(w => w.ScrapeForRecipesAsync(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>()))
          .ReturnsAsync([]);

      // Setup default mapper to return empty list
      _mockMapper.Setup(m => m.Map<List<Recipe>>(It.IsAny<IEnumerable<ScrapedRecipe>>()))
          .Returns([]);

      // Setup config
      var recipeConfig = new RecipeConfig
      {
        AcceptableScoreThreshold = 70,
        RecipesToReturnPerRetrieval = 3,
        MaxSearchResults = 8
      };

      // Create the service with mocked dependencies
      _service = new RecipeService(
          _mockRepository.Object,
          recipeConfig,
          mockLlmService.Object,
          _mockWebScraper.Object,
          _mockMapper.Object,
          rankRecipePrompt,
          synthesizeRecipePrompt,
          analyzeRecipePrompt,
          getAlternativeQueryPrompt,
          mockSessionManager.Object,
          mockLogger.Object,
          mockScopeContainer.Object);
    }

    [Fact]
    public async Task GetRecipes_WhenQueryMatchesAboveThreshold_ReturnsRelevantRecipesOnly()
    {
      // Arrange - override repository for this scenario
      _mockRepository.Setup(r => r.SearchRecipes(
          It.Is<string>(q => q == _query),
          It.Is<int?>(s => s == _acceptableScore),
          It.IsAny<int?>(),
          It.IsAny<CancellationToken>()))
          .ReturnsAsync(_recipes);

      // Act
      var result = await _service.GetRecipes(
          _query,
          false,
          _acceptableScore,
          _requiredCount,
          null,
          CancellationToken.None);

      // Assert
      result.Should().NotBeNull("Because The service should return a list of recipes");
      result.Should().HaveCount(2, "Because Only 2 recipes have relevancy scores above the threshold of 70");
      result.Should().Contain(r => r.Id == "recipe1", "Because Recipe1 has a relevancy score of 85");
      result.Should().Contain(r => r.Id == "recipe3", "Because Recipe3 has a relevancy score of 80");
      result.Should().NotContain(r => r.Id == "recipe2", "Because Recipe2 has a relevancy score of 60, below threshold");

      _mockRepository.Verify(r => r.SearchRecipes(
          It.Is<string>(q => q == _query),
          It.Is<int?>(s => s == _acceptableScore),
          It.IsAny<int?>(),
          It.IsAny<CancellationToken>()),
          Times.Once);
    }

    [Fact]
    public async Task GetRecipes_WhenNoMatchesFoundAndScrapingEnabled_ReturnsScrappedAndProcessedRecipes()
    {
      // Arrange - simulate no initial cached recipes
      _mockRepository.Setup(r => r.SearchRecipes(
          It.Is<string>(q => q == _query),
          It.Is<int?>(s => s == _acceptableScore),
          It.IsAny<int?>(),
          It.IsAny<CancellationToken>()))
          .ReturnsAsync([]);

      var scrapedRecipes = new List<ScrapedRecipe>
            {
                new()
                {
                    Id = "scraped1",
                    Title = "Exotic Curry",
                    Description = "An exotic curry dish",
                    Ingredients = ["Spices", "Protein", "Vegetables"],
                    Directions = ["Prepare ingredients", "Cook"]
                }
            };

      _mockWebScraper.Setup(w => w.ScrapeForRecipesAsync(
          It.Is<string>(q => q == _query),
          It.Is<int?>(s => s == _acceptableScore),
          It.Is<int?>(r => r == _requiredCount),
          It.IsAny<string?>()))
          .ReturnsAsync(scrapedRecipes);

      var mappedRecipes = new List<Recipe>
            {
                new()
                {
                    Id = "scraped1",
                    Title = "Exotic Curry",
                    Description = "An exotic curry dish",
                    Ingredients = ["Spices", "Protein", "Vegetables"],
                    Directions = ["Prepare ingredients", "Cook"],
                    Servings = "4",
                    PrepTime = "20 minutes",
                    CookTime = "30 minutes",
                    TotalTime = "50 minutes",
                    Notes = ", ",
                    Aliases = [],
                    IndexTitle = "Exotic Curry",
                    Relevancy = new Dictionary<string, RelevancyResult>
                    {
                        {
                            _query,
                            new RelevancyResult
                            {
                                Query = _query,
                                Score = 90,
                                Reasoning = "Perfect match for exotic dish"
                            }
                        }
                    }
                }
            };

      _mockMapper.Setup(m => m.Map<List<Recipe>>(It.IsAny<IEnumerable<ScrapedRecipe>>()))
          .Returns(mappedRecipes);

      _mockRepository.Setup(r => r.ContainsRecipe(It.IsAny<string>()))
          .Returns(false);

      // Use real RankRecipePrompt; no mocking of its members

      // Act
      var result = await _service.GetRecipes(
          _query,
          true, // Enable scraping
          _acceptableScore,
          _requiredCount,
          null,
          CancellationToken.None);

      // Assert
      result.Should().NotBeNull("Because The service should return a list of recipes");
      result.Should().HaveCount(1, "Because One recipe was found through web scraping");
      result[0].Id.Should().Be("scraped1", "Because This is the ID of the scraped recipe");

      _mockRepository.Verify(r => r.SearchRecipes(
          It.Is<string>(q => q == _query),
          It.Is<int?>(s => s == _acceptableScore),
          It.IsAny<int?>(),
          It.IsAny<CancellationToken>()), Times.Once);

      _mockWebScraper.Verify(w => w.ScrapeForRecipesAsync(
          It.Is<string>(q => q == _query),
          It.Is<int?>(s => s == _acceptableScore),
          It.Is<int?>(r => r == _requiredCount),
          It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task GetRecipes_WhenRecipesFoundImmediately_ReturnsRecipesWithoutRetry()
    {
      // Arrange
      var requestedRecipeName = "Chicken Soup";
      var acceptableScore = 70;

      _mockRepository.Setup(r => r.SearchRecipes(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(_recipes);

      // Act
      var result = await _service.GetRecipes(
          requestedRecipeName,
          acceptableScore,
          null,
          CancellationToken.None);

      // Assert
      result.Should().NotBeNull("Because the service should return recipes");
      result.Should().HaveCount(2, "Because two recipes meet the acceptance threshold");
      result.Should().Contain(r => r.Id == "recipe1");
      result.Should().Contain(r => r.Id == "recipe3");

      _mockRepository.Verify(r => r.SearchRecipes(
          It.IsAny<string>(),
          It.IsAny<int?>(),
          It.IsAny<int?>(),
          It.IsAny<CancellationToken>()),
          Times.Once,
          "Because recipes were found on the first attempt");
    }

    [Fact]
    public async Task GetRecipes_WhenNoRecipesFoundAfterMaxAttempts_ThrowsNoRecipeException()
    {
      // Arrange
      var requestedRecipeName = "Nonexistent Recipe";
      var maxAttempts = 3;

      _mockRepository.Setup(r => r.SearchRecipes(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync([]);

      _mockWebScraper.Setup(w => w.ScrapeForRecipesAsync(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>()))
          .ReturnsAsync([]);

      var recipeConfig = new RecipeConfig
      {
        AcceptableScoreThreshold = 70,
        RecipesToReturnPerRetrieval = 3,
        MaxSearchResults = 8,
        MaxNewRecipeNameAttempts = maxAttempts
      };

      var mockLlmService = MockOpenAIServiceFactory.CreateMock();
      Mock<ISessionManager> mockSessionManager = new();
      Mock<IScopeContainer> mockScopeContainer = new();
      Mock<ILogger<RecipeService>> mockLogger = new();
      var rankRecipePrompt = new RankRecipePrompt();
      var synthesizeRecipePrompt = new SynthesizeRecipePrompt(_mockMapper.Object);
      var analyzeRecipePrompt = new AnalyzeRecipePrompt();
      var getAlternativeQueryPrompt = new GetAlternativeQueryPrompt();

      var service = new RecipeService(
          _mockRepository.Object,
          recipeConfig,
          mockLlmService.Object,
          _mockWebScraper.Object,
          _mockMapper.Object,
          rankRecipePrompt,
          synthesizeRecipePrompt,
          analyzeRecipePrompt,
          getAlternativeQueryPrompt,
          mockSessionManager.Object,
          mockLogger.Object,
          mockScopeContainer.Object);

      // Act
      Func<Task> act = async () => await service.GetRecipes(
          requestedRecipeName,
          70,
          null,
          CancellationToken.None);

      // Assert
      await act.Should().ThrowAsync<NoRecipeException>(
          "Because no recipes could be found after maximum attempts");

      _mockRepository.Verify(r => r.SearchRecipes(
          It.IsAny<string>(),
          It.IsAny<int?>(),
          It.IsAny<int?>(),
          It.IsAny<CancellationToken>()),
          Times.AtLeast(maxAttempts),
          "Because the service should retry up to the maximum number of attempts");
    }

    [Fact]
    public async Task GetRecipes_WhenRepositoryReturnsRecipesBelowThreshold_ReturnsEmptyWithoutScraping()
    {
      // Arrange
      var query = "unrelated query";
      var acceptableScore = 95;

      // Recipes exist but none meet the high threshold for this query
      var recipesWithLowScores = new List<Recipe>
      {
        new()
        {
          Id = "low-score-recipe",
          Title = "Low Score Recipe",
          Description = "Test",
          Ingredients = ["Test"],
          Directions = ["Test"],
          Servings = "4",
          PrepTime = "10 minutes",
          CookTime = "20 minutes",
          TotalTime = "30 minutes",
          Notes = "",
          Aliases = [],
          IndexTitle = "low score",
          Relevancy = new Dictionary<string, RelevancyResult>
          {
            {
              query,
              new RelevancyResult { Query = query, Score = 50, Reasoning = "Low relevance" }
            }
          }
        }
      };

      _mockRepository.Setup(r => r.SearchRecipes(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(recipesWithLowScores);

      // Act
      var result = await _service.GetRecipes(
          query,
          false, // No scraping
          acceptableScore,
          null,
          null,
          CancellationToken.None);

      // Assert
      result.Should().BeEmpty("Because no recipes meet the threshold of 95");

      _mockRepository.Verify(r => r.SearchRecipes(
          It.IsAny<string>(),
          It.IsAny<int?>(),
          It.IsAny<int?>(),
          It.IsAny<CancellationToken>()),
          Times.Once,
          "Because repository was queried once");
    }

    [Fact]
    public async Task RankUnrankedRecipesAsync_WhenGivenScrapedRecipes_MapsAndRanksRecipes()
    {
      // Arrange
      var query = "test query";
      var scrapedRecipes = new List<ScrapedRecipe>
      {
        new()
        {
          Id = "scraped1",
          Title = "Scraped Recipe",
          Description = "Test description",
          Ingredients = ["Ingredient 1"],
          Directions = ["Direction 1"]
        }
      };

      var mappedRecipes = new List<Recipe>
      {
        new()
        {
          Id = "scraped1",
          Title = "Scraped Recipe",
          Description = "Test description",
          Ingredients = ["Ingredient 1"],
          Directions = ["Direction 1"],
          Servings = "4",
          PrepTime = "10 minutes",
          CookTime = "20 minutes",
          TotalTime = "30 minutes",
          Notes = "",
          Aliases = [],
          IndexTitle = "scraped recipe",
          Relevancy = new Dictionary<string, RelevancyResult>()
        }
      };

      _mockMapper.Setup(m => m.Map<List<Recipe>>(It.IsAny<IEnumerable<ScrapedRecipe>>()))
          .Returns(mappedRecipes);

      // Act
      var result = await _service.RankUnrankedRecipesAsync(scrapedRecipes, query);

      // Assert
      result.Should().NotBeNull("Because the service should return ranked recipes");
      result.Should().HaveCount(1, "Because one recipe was provided");

      _mockMapper.Verify(m => m.Map<List<Recipe>>(It.IsAny<IEnumerable<ScrapedRecipe>>()), Times.Once,
          "Because scraped recipes should be mapped to Recipe objects");
    }

    [Fact]
    public async Task GetRecipes_WhenCachedRecipesMeetRequirement_DoesNotScrape()
    {
      // Arrange
      var query = "chicken soup";
      var acceptableScore = 70;
      var requiredCount = 2;

      _mockRepository.Setup(r => r.SearchRecipes(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(_recipes);

      // Act
      var result = await _service.GetRecipes(
          query,
          true,
          acceptableScore,
          requiredCount,
          null,
          CancellationToken.None);

      // Assert
      result.Should().HaveCount(2, "Because two recipes meet the threshold from cache");

      _mockWebScraper.Verify(w => w.ScrapeForRecipesAsync(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>()),
          Times.Never,
          "Because cached recipes satisfied the requirement");
    }

    [Fact]
    public async Task GetRecipes_WhenScrapingDisabled_OnlyReturnsRepositoryResults()
    {
      // Arrange
      var query = _query; // Use the same query that recipes are ranked for
      var acceptableScore = 70;

      _mockRepository.Setup(r => r.SearchRecipes(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(_recipes);

      // Act
      var result = await _service.GetRecipes(
          query,
          false, // Scraping disabled
          acceptableScore,
          null,
          null,
          CancellationToken.None);

      // Assert
      result.Should().HaveCount(2, "Because recipe1 (85) and recipe3 (80) meet the threshold of 70");
      result.Should().NotContain(r => r.Id == "recipe2", "Because recipe2 has a score of 60");

      _mockWebScraper.Verify(w => w.ScrapeForRecipesAsync(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>()),
          Times.Never,
          "Because scraping was explicitly disabled");
    }

    [Fact]
    public async Task GetRecipes_WhenInsufficientCachedRecipes_PerformsWebScraping()
    {
      // Arrange
      var query = "exotic dish";
      var acceptableScore = 70;
      var requiredCount = 3;

      // Only one recipe in repository
      var singleRecipe = new List<Recipe> { _recipes[0] };

      _mockRepository.Setup(r => r.SearchRecipes(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(singleRecipe);

      var scrapedRecipes = new List<ScrapedRecipe>
      {
        new()
        {
          Id = "scraped1",
          Title = "Scraped Recipe",
          Description = "Test",
          Ingredients = ["Ingredient"],
          Directions = ["Direction"]
        }
      };

      _mockWebScraper.Setup(w => w.ScrapeForRecipesAsync(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>()))
          .ReturnsAsync(scrapedRecipes);

      var mappedRecipe = new Recipe
      {
        Id = "scraped1",
        Title = "Scraped Recipe",
        Description = "Test",
        Ingredients = ["Ingredient"],
        Directions = ["Direction"],
        Servings = "4",
        PrepTime = "10 minutes",
        CookTime = "20 minutes",
        TotalTime = "30 minutes",
        Notes = "",
        Aliases = [],
        IndexTitle = "scraped recipe",
        Relevancy = new Dictionary<string, RelevancyResult>
        {
          {
            query,
            new RelevancyResult { Query = query, Score = 85, Reasoning = "Good match" }
          }
        }
      };

      _mockMapper.Setup(m => m.Map<List<Recipe>>(It.IsAny<IEnumerable<ScrapedRecipe>>()))
          .Returns([mappedRecipe]);

      _mockRepository.Setup(r => r.ContainsRecipe(It.IsAny<string>()))
          .Returns(false);

      // Act
      var result = await _service.GetRecipes(
          query,
          true,
          acceptableScore,
          requiredCount,
          null,
          CancellationToken.None);

      // Assert
      result.Should().HaveCountGreaterThanOrEqualTo(1,
          "Because web scraping should supplement insufficient cached recipes");

      _mockWebScraper.Verify(w => w.ScrapeForRecipesAsync(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>()),
          Times.Once,
          "Because cached recipes were insufficient");

      _mockRepository.Verify(r => r.AddUpdateRecipesAsync(It.IsAny<List<Recipe>>()), Times.Once,
          "Because new recipes should be saved to repository");
    }

    [Fact]
    public async Task GetRecipes_WhenDuplicateRecipesScraped_FiltersOutExistingRecipes()
    {
      // Arrange
      var query = "test query";
      var acceptableScore = 70;

      _mockRepository.Setup(r => r.SearchRecipes(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync([]);

      var scrapedRecipes = new List<ScrapedRecipe>
      {
        new()
        {
          Id = "existing-recipe",
          Title = "Existing Recipe",
          Description = "Already in repository",
          Ingredients = ["Ingredient"],
          Directions = ["Direction"]
        },
        new()
        {
          Id = "new-recipe",
          Title = "New Recipe",
          Description = "Not in repository",
          Ingredients = ["Ingredient"],
          Directions = ["Direction"]
        }
      };

      _mockWebScraper.Setup(w => w.ScrapeForRecipesAsync(
          It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>()))
          .ReturnsAsync(scrapedRecipes);

      _mockRepository.Setup(r => r.ContainsRecipe("existing-recipe"))
          .Returns(true);
      _mockRepository.Setup(r => r.ContainsRecipe("new-recipe"))
          .Returns(false);

      var newMappedRecipe = new Recipe
      {
        Id = "new-recipe",
        Title = "New Recipe",
        Description = "Not in repository",
        Ingredients = ["Ingredient"],
        Directions = ["Direction"],
        Servings = "4",
        PrepTime = "10 minutes",
        CookTime = "20 minutes",
        TotalTime = "30 minutes",
        Notes = "",
        Aliases = [],
        IndexTitle = "new recipe",
        Relevancy = new Dictionary<string, RelevancyResult>
        {
          {
            query,
            new RelevancyResult { Query = query, Score = 80, Reasoning = "Good match" }
          }
        }
      };

      _mockMapper.Setup(m => m.Map<List<Recipe>>(It.Is<IEnumerable<ScrapedRecipe>>(
          recipes => recipes.Count() == 1 && recipes.First().Id == "new-recipe")))
          .Returns([newMappedRecipe]);

      // Act
      var result = await _service.GetRecipes(
          query,
          true,
          acceptableScore,
          null,
          null,
          CancellationToken.None);

      // Assert
      result.Should().HaveCount(1, "Because only the new recipe should be added");
      result[0].Id.Should().Be("new-recipe", "Because existing recipes should be filtered out");
    }
  }
}
