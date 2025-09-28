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
  }
}
