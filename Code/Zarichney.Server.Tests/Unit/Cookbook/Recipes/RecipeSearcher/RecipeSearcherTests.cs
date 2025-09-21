using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Cookbook.Recipes;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Server.Tests.Unit.Cookbook.Recipes.RecipeSearcher;

public class RecipeSearcherTests
{
  private readonly Mock<IRecipeIndexer> _mockIndexer;
  private readonly Mock<ILogger<Zarichney.Cookbook.Recipes.RecipeSearcher>> _mockLogger;
  private readonly RecipeConfig _config;
  private readonly Zarichney.Cookbook.Recipes.RecipeSearcher _sut;

  public RecipeSearcherTests()
  {
    _mockIndexer = new Mock<IRecipeIndexer>();
    _mockLogger = new Mock<ILogger<Zarichney.Cookbook.Recipes.RecipeSearcher>>();
    _config = new RecipeConfig
    {
      MaxSearchResults = 8,
      AcceptableScoreThreshold = 70
    };
    _sut = new Zarichney.Cookbook.Recipes.RecipeSearcher(_mockIndexer.Object, _mockLogger.Object, _config);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_EmptyQuery_ThrowsArgumentException()
  {
    // Arrange
    var query = "";

    // Act
    Func<Task> act = async () => await _sut.SearchRecipes(query);

    // Assert
    await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*query*", "empty queries should not be allowed");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_NullQuery_ThrowsArgumentException()
  {
    // Arrange
    string? query = null;

    // Act
    Func<Task> act = async () => await _sut.SearchRecipes(query!);

    // Assert
    await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*query*", "null queries should not be allowed");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_InvalidMinimumScore_ThrowsArgumentException()
  {
    // Arrange
    var query = "chicken";
    var invalidScore = 0;

    // Act
    Func<Task> act = async () => await _sut.SearchRecipes(query, invalidScore);

    // Assert
    await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*score*", "minimum score must be between 1 and 99");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_MinimumScoreTooHigh_ThrowsArgumentException()
  {
    // Arrange
    var query = "chicken";
    var invalidScore = 100;

    // Act
    Func<Task> act = async () => await _sut.SearchRecipes(query, invalidScore);

    // Assert
    await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*score*", "minimum score must be between 1 and 99");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_InvalidRequiredCount_ThrowsArgumentException()
  {
    // Arrange
    var query = "chicken";
    var invalidCount = 0;

    // Act
    Func<Task> act = async () => await _sut.SearchRecipes(query, requiredCount: invalidCount);

    // Assert
    await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*count*", "required count must be greater than 0");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_ExactMatchByTitle_ReturnsMatchedRecipe()
  {
    // Arrange
    var query = "chicken soup";
    var recipe = new RecipeBuilder()
      .WithTitle("Chicken Soup")
      .WithIndexTitle("chicken soup")
      .Build();

    var matchDict = new ConcurrentDictionary<string, Recipe>();
    matchDict.TryAdd(recipe.Id!, recipe);

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(query, out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns((string key, out ConcurrentDictionary<string, Recipe> matches) =>
      {
        matches = matchDict;
        return true;
      });

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(new List<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    // Act
    var result = await _sut.SearchRecipes(query);

    // Assert
    result.Should().HaveCount(1, "exact title matches should be returned")
      .And.Contain(r => r.Id == recipe.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_MultipleExactMatches_ReturnsAllMatches()
  {
    // Arrange
    var query = "pasta";
    var recipe1 = new RecipeBuilder()
      .WithTitle("Pasta Carbonara")
      .WithId("recipe-1")
      .Build();
    var recipe2 = new RecipeBuilder()
      .WithTitle("Pasta Bolognese")
      .WithId("recipe-2")
      .Build();

    var matchDict = new ConcurrentDictionary<string, Recipe>();
    matchDict.TryAdd(recipe1.Id!, recipe1);
    matchDict.TryAdd(recipe2.Id!, recipe2);

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(query, out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns((string key, out ConcurrentDictionary<string, Recipe> matches) =>
      {
        matches = matchDict;
        return true;
      });

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(new List<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    // Act
    var result = await _sut.SearchRecipes(query);

    // Assert
    result.Should().HaveCount(2, "all exact matches should be returned")
      .And.Contain(r => r.Id == recipe1.Id)
      .And.Contain(r => r.Id == recipe2.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_ExactMatchByAlias_ReturnsMatchedRecipe()
  {
    // Arrange
    var query = "mac and cheese";
    var recipe = new RecipeBuilder()
      .WithTitle("Macaroni and Cheese")
      .WithAliases(["mac and cheese", "mac n cheese"])
      .WithId("recipe-1")
      .Build();

    var allRecipes = new Dictionary<string, ConcurrentDictionary<string, Recipe>>
    {
      ["macaroni and cheese"] = new ConcurrentDictionary<string, Recipe>(new[] { new KeyValuePair<string, Recipe>(recipe.Id!, recipe) })
    };

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns(false);

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(allRecipes.Select(kvp => new KeyValuePair<string, ConcurrentDictionary<string, Recipe>>(kvp.Key, kvp.Value)));

    // Act
    var result = await _sut.SearchRecipes(query);

    // Assert
    result.Should().HaveCount(1,
      "recipes should be found by exact alias match")
      .And.Contain(r => r.Id == recipe.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_FuzzyMatchOnTitle_ReturnsMatchedRecipe()
  {
    // Arrange
    var query = "chicken";
    var recipe = new RecipeBuilder()
      .WithTitle("Grilled Chicken Sandwich")
      .WithId("recipe-1")
      .Build();

    var allRecipes = new Dictionary<string, ConcurrentDictionary<string, Recipe>>
    {
      ["grilled chicken sandwich"] = new ConcurrentDictionary<string, Recipe>(new[] { new KeyValuePair<string, Recipe>(recipe.Id!, recipe) })
    };

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns(false);

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(allRecipes.Select(kvp => new KeyValuePair<string, ConcurrentDictionary<string, Recipe>>(kvp.Key, kvp.Value)));

    // Act
    var result = await _sut.SearchRecipes(query);

    // Assert
    result.Should().HaveCount(1,
      "fuzzy matching should find partial matches in titles")
      .And.Contain(r => r.Id == recipe.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_FuzzyMatchOnAlias_ReturnsMatchedRecipe()
  {
    // Arrange
    var query = "bbq";
    var recipe = new RecipeBuilder()
      .WithTitle("Southern Style Ribs")
      .WithAliases(["bbq ribs", "barbecue ribs"])
      .WithId("recipe-1")
      .Build();

    var allRecipes = new Dictionary<string, ConcurrentDictionary<string, Recipe>>
    {
      ["southern style ribs"] = new ConcurrentDictionary<string, Recipe>(new[] { new KeyValuePair<string, Recipe>(recipe.Id!, recipe) })
    };

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns(false);

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(allRecipes.Select(kvp => new KeyValuePair<string, ConcurrentDictionary<string, Recipe>>(kvp.Key, kvp.Value)));

    // Act
    var result = await _sut.SearchRecipes(query);

    // Assert
    result.Should().HaveCount(1,
      "fuzzy matching should find partial matches in aliases")
      .And.Contain(r => r.Id == recipe.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_WithRelevancyScore_PrioritizesHighScoringRecipes()
  {
    // Arrange
    var query = "healthy salad";
    var minScore = 70;

    var highRelevancyRecipe = new RecipeBuilder()
      .WithTitle("Green Salad")
      .WithId("high-relevancy")
      .WithRelevancyScore(query, 85, "Very relevant to healthy salad")
      .Build();

    var lowRelevancyRecipe = new RecipeBuilder()
      .WithTitle("Caesar Salad")
      .WithId("low-relevancy")
      .WithRelevancyScore(query, 40, "Less relevant to healthy salad")
      .Build();

    var matchDict = new ConcurrentDictionary<string, Recipe>();
    matchDict.TryAdd(highRelevancyRecipe.Id!, highRelevancyRecipe);
    matchDict.TryAdd(lowRelevancyRecipe.Id!, lowRelevancyRecipe);

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns((string key, out ConcurrentDictionary<string, Recipe> matches) =>
      {
        matches = matchDict;
        return true;
      });

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(new List<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    // Act
    var result = await _sut.SearchRecipes(query, minScore, requiredCount: 1);

    // Assert
    result.Should().HaveCount(1);
    result.First().Id.Should().Be(highRelevancyRecipe.Id,
      "recipes with relevancy scores above the minimum should be prioritized");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_RequiredCountLimitsResults_ReturnsExactCount()
  {
    // Arrange
    var query = "recipe";
    var requiredCount = 2;

    var recipes = Enumerable.Range(1, 5)
      .Select(i => new RecipeBuilder()
        .WithTitle($"Recipe {i}")
        .WithId($"recipe-{i}")
        .Build())
      .ToList();

    var matchDict = new ConcurrentDictionary<string, Recipe>();

    // Add all 5 recipes to the dictionary to simulate many matches
    recipes.ForEach(r => matchDict.TryAdd(r.Id!, r));

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns((string key, out ConcurrentDictionary<string, Recipe> matches) =>
      {
        matches = matchDict;
        return true;
      });

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(new List<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    // Act
    var result = await _sut.SearchRecipes(query, requiredCount: requiredCount);

    // Assert
    result.Should().HaveCount(requiredCount,
      $"the required count of {requiredCount} should limit the results");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_NoMatches_ReturnsEmptyList()
  {
    // Arrange
    var query = "nonexistent recipe";

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns(false);

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(new List<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    // Act
    var result = await _sut.SearchRecipes(query);

    // Assert
    result.Should().BeEmpty(
      "no matches should return an empty list");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_CancellationRequested_ReturnsEmptyList()
  {
    // Arrange
    var query = "chicken";
    var cts = new CancellationTokenSource();
    cts.Cancel();

    var recipe = new RecipeBuilder()
      .WithTitle("Chicken Curry")
      .Build();

    var allRecipes = new Dictionary<string, ConcurrentDictionary<string, Recipe>>
    {
      ["chicken curry"] = new ConcurrentDictionary<string, Recipe>(new[] { new KeyValuePair<string, Recipe>(recipe.Id!, recipe) })
    };

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns(false);

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(allRecipes.Select(kvp => new KeyValuePair<string, ConcurrentDictionary<string, Recipe>>(kvp.Key, kvp.Value)));

    // Act
    var result = await _sut.SearchRecipes(query, cancellationToken: cts.Token);

    // Assert
    result.Should().BeEmpty(
      "cancelled operations should return empty results");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_MixedRelevancyScores_OrdersByFinalScore()
  {
    // Arrange
    var query = "pasta";

    var highRelevancyLowMatch = new RecipeBuilder()
      .WithTitle("Spaghetti")
      .WithId("recipe-1")
      .WithRelevancyScore(query, 90, "High relevancy")
      .Build();

    var mediumRelevancyHighMatch = new RecipeBuilder()
      .WithTitle("Pasta Primavera")
      .WithId("recipe-2")
      .WithRelevancyScore(query, 60, "Medium relevancy")
      .Build();

    var lowRelevancyMediumMatch = new RecipeBuilder()
      .WithTitle("Lasagna with Pasta")
      .WithId("recipe-3")
      .WithRelevancyScore(query, 30, "Low relevancy")
      .Build();

    var matchDict = new ConcurrentDictionary<string, Recipe>();
    matchDict.TryAdd(highRelevancyLowMatch.Id!, highRelevancyLowMatch);
    matchDict.TryAdd(mediumRelevancyHighMatch.Id!, mediumRelevancyHighMatch);
    matchDict.TryAdd(lowRelevancyMediumMatch.Id!, lowRelevancyMediumMatch);

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns((string key, out ConcurrentDictionary<string, Recipe> matches) =>
      {
        matches = matchDict;
        return true;
      });

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(new List<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    // Act
    var result = await _sut.SearchRecipes(query, minimumScore: 50);

    // Assert
    result.Should().NotBeEmpty("results should include recipes above minimum score");
    result.Should().HaveCount(2, "only recipes with score >= 50 should be returned");
    result[0].Id.Should().Be(highRelevancyLowMatch.Id,
      "results should be ordered by final score calculation with highest first");
    result[1].Id.Should().Be(mediumRelevancyHighMatch.Id,
      "medium scoring recipe should be second");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_CaseInsensitiveMatching_FindsRecipesRegardlessOfCase()
  {
    // Arrange
    var query = "CHICKEN SOUP";
    var recipe = new RecipeBuilder()
      .WithTitle("chicken soup")
      .WithId("recipe-1")
      .Build();

    var matchDict = new ConcurrentDictionary<string, Recipe>();
    matchDict.TryAdd(recipe.Id!, recipe);

    _mockIndexer
      .Setup(x => x.TryGetExactMatches("chicken soup", out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns((string key, out ConcurrentDictionary<string, Recipe> matches) =>
      {
        matches = matchDict;
        return true;
      });

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(new List<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    // Act
    var result = await _sut.SearchRecipes(query);

    // Assert
    result.Should().HaveCount(1,
      "search should be case-insensitive")
      .And.Contain(r => r.Id == recipe.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_DuplicateRecipesInIndex_ReturnsUniqueResults()
  {
    // Arrange
    var query = "salad";
    var recipe = new RecipeBuilder()
      .WithTitle("Garden Salad")
      .WithAliases(["green salad", "fresh salad"])
      .WithId("recipe-1")
      .Build();

    var allRecipes = new Dictionary<string, ConcurrentDictionary<string, Recipe>>
    {
      ["garden salad"] = new ConcurrentDictionary<string, Recipe>(new[] { new KeyValuePair<string, Recipe>(recipe.Id!, recipe) }),
      ["green salad"] = new ConcurrentDictionary<string, Recipe>(new[] { new KeyValuePair<string, Recipe>(recipe.Id!, recipe) }),
      ["fresh salad"] = new ConcurrentDictionary<string, Recipe>(new[] { new KeyValuePair<string, Recipe>(recipe.Id!, recipe) })
    };

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns(false);

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(allRecipes.Select(kvp => new KeyValuePair<string, ConcurrentDictionary<string, Recipe>>(kvp.Key, kvp.Value)));

    // Act
    var result = await _sut.SearchRecipes(query);

    // Assert
    result.Should().HaveCount(1,
      "duplicate recipes should be deduplicated in results")
      .And.Contain(r => r.Id == recipe.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_FallbackWithLowRelevancy_IncludesAsSecondaryResults()
  {
    // Arrange
    var query = "healthy";
    var minScore = 70;
    var requiredCount = 3;

    var highScoreRecipe = new RecipeBuilder()
      .WithTitle("Healthy Smoothie")
      .WithId("high-1")
      .WithRelevancyScore(query, 85)
      .Build();

    var lowScoreRecipe1 = new RecipeBuilder()
      .WithTitle("Green Juice")
      .WithId("low-1")
      .WithRelevancyScore(query, 40)
      .Build();

    var lowScoreRecipe2 = new RecipeBuilder()
      .WithTitle("Fruit Bowl")
      .WithId("low-2")
      .WithRelevancyScore(query, 30)
      .Build();

    var matchDict = new ConcurrentDictionary<string, Recipe>();
    matchDict.TryAdd(highScoreRecipe.Id!, highScoreRecipe);
    matchDict.TryAdd(lowScoreRecipe1.Id!, lowScoreRecipe1);
    matchDict.TryAdd(lowScoreRecipe2.Id!, lowScoreRecipe2);

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns((string key, out ConcurrentDictionary<string, Recipe> matches) =>
      {
        matches = matchDict;
        return true;
      });

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(new List<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    // Act
    var result = await _sut.SearchRecipes(query, minScore, requiredCount);

    // Assert
    result.Should().HaveCount(3,
      "high relevancy recipes should come first, followed by fallback recipes to meet required count");
    result.First().Id.Should().Be(highScoreRecipe.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_WhitespaceInQuery_NormalizesCorrectly()
  {
    // Arrange
    var query = "  chicken   soup  ";
    var normalizedQuery = "chicken soup";  // After trimming and lowercasing
    var recipe = new RecipeBuilder()
      .WithTitle("Chicken Soup")
      .WithId("recipe-1")
      .Build();

    var matchDict = new ConcurrentDictionary<string, Recipe>();
    matchDict.TryAdd(recipe.Id!, recipe);

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(normalizedQuery, out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns((string key, out ConcurrentDictionary<string, Recipe> matches) =>
      {
        matches = matchDict;
        return true;
      });

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(new List<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    // Act
    var result = await _sut.SearchRecipes(query);

    // Assert
    result.Should().HaveCount(1,
      "whitespace should be normalized in queries")
      .And.Contain(r => r.Id == recipe.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SearchRecipes_LogsSearchOperation_VerifyLogging()
  {
    // Arrange
    var query = "test";
    var minScore = 70;
    var requiredCount = 5;

    _mockIndexer
      .Setup(x => x.TryGetExactMatches(It.IsAny<string>(), out It.Ref<ConcurrentDictionary<string, Recipe>>.IsAny))
      .Returns(false);

    _mockIndexer
      .Setup(x => x.GetAllRecipes())
      .Returns(new List<KeyValuePair<string, ConcurrentDictionary<string, Recipe>>>());

    // Act
    await _sut.SearchRecipes(query, minScore, requiredCount);

    // Assert
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting recipe search")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once,
      "search operation should be logged");
  }
}
