using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Cookbook.Recipes;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Cookbook.Recipes.RecipeIndexer;

public class RecipeIndexerTests
{
  private readonly Mock<ILogger<Zarichney.Cookbook.Recipes.RecipeIndexer>> _loggerMock;
  private readonly Zarichney.Cookbook.Recipes.RecipeIndexer _sut;

  public RecipeIndexerTests()
  {
    _loggerMock = new Mock<ILogger<Zarichney.Cookbook.Recipes.RecipeIndexer>>();
    _sut = new Zarichney.Cookbook.Recipes.RecipeIndexer(_loggerMock.Object);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_WithValidRecipe_IndexesRecipeByTitle()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId(Guid.NewGuid().ToString())
      .WithTitle("Chocolate Cake")
      .Build();

    // Act
    _sut.AddRecipe(recipe);

    // Assert
    _sut.TryGetExactMatches("Chocolate Cake", out var matches).Should().BeTrue(
      "because the recipe was indexed with this title");
    matches.Should().ContainKey(recipe.Id!);
    matches[recipe.Id!].Should().BeEquivalentTo(recipe);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_WithAliases_IndexesRecipeByEachAlias()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId(Guid.NewGuid().ToString())
      .WithTitle("Chocolate Cake")
      .WithAliases(["Choco Cake", "Dark Chocolate Dessert"])
      .Build();

    // Act
    _sut.AddRecipe(recipe);

    // Assert
    _sut.TryGetExactMatches("Choco Cake", out var matches1).Should().BeTrue(
      "because the recipe was indexed with this alias");
    matches1.Should().ContainKey(recipe.Id!);

    _sut.TryGetExactMatches("Dark Chocolate Dessert", out var matches2).Should().BeTrue(
      "because the recipe was indexed with this alias");
    matches2.Should().ContainKey(recipe.Id!);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_WithNullId_ThrowsArgumentException()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId(null!)
      .Build();

    // Act
    var act = () => _sut.AddRecipe(recipe);

    // Assert
    act.Should().Throw<ArgumentException>()
      .WithMessage("*ID*")
      .WithParameterName("recipe");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_WithEmptyId_ThrowsArgumentException()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId(string.Empty)
      .Build();

    // Act
    var act = () => _sut.AddRecipe(recipe);

    // Assert
    act.Should().Throw<ArgumentException>()
      .WithMessage("*ID*")
      .WithParameterName("recipe");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_CaseInsensitive_IndexesWithCaseInsensitivity()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId(Guid.NewGuid().ToString())
      .WithTitle("Chocolate Cake")
      .Build();

    // Act
    _sut.AddRecipe(recipe);

    // Assert
    _sut.TryGetExactMatches("chocolate cake", out var matches1).Should().BeTrue(
      "because indexing should be case-insensitive");
    _sut.TryGetExactMatches("CHOCOLATE CAKE", out var matches2).Should().BeTrue(
      "because indexing should be case-insensitive");
    _sut.TryGetExactMatches("ChOcOlAtE cAkE", out var matches3).Should().BeTrue(
      "because indexing should be case-insensitive");

    matches1[recipe.Id!].Should().BeEquivalentTo(recipe);
    matches2[recipe.Id!].Should().BeEquivalentTo(recipe);
    matches3[recipe.Id!].Should().BeEquivalentTo(recipe);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_DuplicateRecipeWithSameId_UpdatesExistingEntry()
  {
    // Arrange
    var originalRecipe = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Original Title")
      .WithDescription("Original Description")
      .Build();

    var updatedRecipe = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Updated Title")
      .WithDescription("Updated Description")
      .Build();

    // Act
    _sut.AddRecipe(originalRecipe);
    _sut.AddRecipe(updatedRecipe);

    // Assert
    _sut.TryGetExactMatches("Updated Title", out var matches).Should().BeTrue(
      "because the recipe was re-indexed with new title");
    matches["recipe-1"].Description.Should().Be("Updated Description",
      "because the recipe should be updated");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void TryGetExactMatches_NonExistentKey_ReturnsFalse()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId(Guid.NewGuid().ToString())
      .WithTitle("Chocolate Cake")
      .Build();
    _sut.AddRecipe(recipe);

    // Act
    var result = _sut.TryGetExactMatches("Non-existent Recipe", out var matches);

    // Assert
    result.Should().BeFalse("because no recipe with this key exists");
    matches.Should().BeNull();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void TryGetExactMatches_WithMultipleRecipesForSameKey_ReturnsAllRecipes()
  {
    // Arrange
    var recipe1 = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Chocolate Cake")
      .Build();

    var recipe2 = new RecipeBuilder()
      .WithId("recipe-2")
      .WithTitle("Chocolate Cake")
      .Build();

    // Act
    _sut.AddRecipe(recipe1);
    _sut.AddRecipe(recipe2);

    // Assert
    _sut.TryGetExactMatches("Chocolate Cake", out var matches).Should().BeTrue(
      "because multiple recipes share this title");
    matches.Should().HaveCount(2);
    matches.Should().ContainKey("recipe-1");
    matches.Should().ContainKey("recipe-2");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void GetAllRecipes_EmptyIndexer_ReturnsEmptyCollection()
  {
    // Act
    var allRecipes = _sut.GetAllRecipes();

    // Assert
    allRecipes.Should().BeEmpty("because no recipes have been added");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void GetAllRecipes_WithMultipleRecipes_ReturnsAllIndexedEntries()
  {
    // Arrange
    var recipe1 = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Chocolate Cake")
      .WithAliases(["Choco Cake"])
      .Build();

    var recipe2 = new RecipeBuilder()
      .WithId("recipe-2")
      .WithTitle("Vanilla Cake")
      .Build();

    _sut.AddRecipe(recipe1);
    _sut.AddRecipe(recipe2);

    // Act
    var allRecipes = _sut.GetAllRecipes().ToList();

    // Assert
    allRecipes.Should().HaveCount(3,
      "because recipe1 is indexed under 2 keys (title + alias) and recipe2 under 1 key");

    var keys = allRecipes.Select(kvp => kvp.Key).ToList();
    keys.Should().Contain("Chocolate Cake", "because recipe1 was indexed with this title");
    keys.Should().Contain("Choco Cake", "because recipe1 was indexed with this alias");
    keys.Should().Contain("Vanilla Cake", "because recipe2 was indexed with this title");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_LogsDebugInformation()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId("test-id")
      .WithTitle("Test Recipe")
      .Build();

    // Act
    _sut.AddRecipe(recipe);

    // Assert
    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Debug,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Adding recipe to index")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once,
      "because adding a recipe should log debug information");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void TryGetExactMatches_LogsDebugInformation()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId("test-id")
      .WithTitle("Test Recipe")
      .Build();
    _sut.AddRecipe(recipe);

    // Act
    _sut.TryGetExactMatches("Test Recipe", out _);

    // Assert
    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Debug,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Exact match lookup")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once,
      "because looking up a recipe should log debug information");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void GetAllRecipes_LogsDebugInformation()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId("test-id")
      .WithTitle("Test Recipe")
      .Build();
    _sut.AddRecipe(recipe);

    // Act
    _ = _sut.GetAllRecipes().ToList();

    // Assert
    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Debug,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Getting all recipes")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once,
      "because getting all recipes should log debug information");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_WithNullTitle_ThrowsArgumentNullException()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle(null!)
      .WithAliases(["Alias1"])
      .Build();

    // Act
    var act = () => _sut.AddRecipe(recipe);

    // Assert
    act.Should().Throw<ArgumentNullException>()
      .WithParameterName("key",
        "because the indexer cannot handle null keys");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_WithEmptyAliasesList_IndexesOnlyByTitle()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Test Recipe")
      .WithAliases([])
      .Build();

    // Act
    _sut.AddRecipe(recipe);

    // Assert
    var allRecipes = _sut.GetAllRecipes().ToList();
    allRecipes.Should().HaveCount(1,
      "because recipe is only indexed by its title");
    allRecipes[0].Key.Should().Be("Test Recipe");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_ConcurrentAdditions_HandlesThreadSafety()
  {
    // Arrange
    var recipes = Enumerable.Range(1, 100)
      .Select(i => new RecipeBuilder()
        .WithId($"recipe-{i}")
        .WithTitle($"Recipe {i}")
        .Build())
      .ToList();

    // Act
    Parallel.ForEach(recipes, recipe => _sut.AddRecipe(recipe));

    // Assert
    var allRecipes = _sut.GetAllRecipes().ToList();
    allRecipes.Should().HaveCount(100,
      "because all recipes should be indexed despite concurrent additions");

    foreach (var recipe in recipes)
    {
      _sut.TryGetExactMatches(recipe.Title!, out var matches).Should().BeTrue();
      matches.Should().ContainKey(recipe.Id!);
    }
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void TryGetExactMatches_ConcurrentReads_HandlesThreadSafety()
  {
    // Arrange
    var recipe = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Test Recipe")
      .Build();
    _sut.AddRecipe(recipe);

    var results = new ConcurrentBag<bool>();

    // Act
    Parallel.For(0, 100, _ =>
    {
      var success = _sut.TryGetExactMatches("Test Recipe", out var matches);
      if (success)
      {
        results.Add(matches.ContainsKey("recipe-1"));
      }
    });

    // Assert
    results.Should().HaveCount(100);
    results.Should().AllBeEquivalentTo(true,
      "because all concurrent reads should successfully find the recipe");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void AddRecipe_SameRecipeMultipleTimes_OnlyKeepsLatestVersion()
  {
    // Arrange
    var recipeV1 = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Recipe Title")
      .WithDescription("Version 1")
      .Build();

    var recipeV2 = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Recipe Title")
      .WithDescription("Version 2")
      .Build();

    var recipeV3 = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Recipe Title")
      .WithDescription("Version 3")
      .Build();

    // Act
    _sut.AddRecipe(recipeV1);
    _sut.AddRecipe(recipeV2);
    _sut.AddRecipe(recipeV3);

    // Assert
    _sut.TryGetExactMatches("Recipe Title", out var matches).Should().BeTrue();
    matches.Should().HaveCount(1);
    matches["recipe-1"].Description.Should().Be("Version 3",
      "because the last added version should be kept");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void GetAllRecipes_ReturnsActualDictionaryReferences()
  {
    // Arrange
    var recipe1 = new RecipeBuilder()
      .WithId("recipe-1")
      .WithTitle("Recipe 1")
      .Build();

    _sut.AddRecipe(recipe1);

    // Act
    var allRecipesBefore = _sut.GetAllRecipes().ToList();
    var recipeDict = allRecipesBefore[0].Value;

    // Add another recipe with the same title
    var recipe2 = new RecipeBuilder()
      .WithId("recipe-2")
      .WithTitle("Recipe 1")
      .Build();
    _sut.AddRecipe(recipe2);

    // Assert - the dictionary reference should contain both recipes now
    recipeDict.Should().HaveCount(2,
      "because the returned dictionary is a reference to the actual internal dictionary");
    recipeDict.Should().ContainKey("recipe-1");
    recipeDict.Should().ContainKey("recipe-2");
  }
}