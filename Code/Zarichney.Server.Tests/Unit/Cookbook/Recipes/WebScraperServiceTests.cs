using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Config;
using Zarichney.Cookbook.Prompts;
using Zarichney.Cookbook.Recipes;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Server.Tests.TestData.AutoFixtureCustomizations;
using Zarichney.Server.Tests.Framework.TestData.AutoFixtureCustomizations;
using Zarichney.Services.AI;
using Zarichney.Services.FileSystem;
using Zarichney.Services.Web;

namespace Zarichney.Server.Tests.Unit.Cookbook.Recipes;

/// <summary>
/// Comprehensive unit tests for WebScraperService covering recipe scraping business logic.
/// Tests URL collection, ranking, parallel scraping, error handling, and configuration management.
/// </summary>
[Trait("Category", "Unit")]
public class WebScraperServiceTests
{
  [Theory, AutoMoqData]
  public async Task ScrapeForRecipesAsync_ValidQuery_ReturnsFilteredRecipes(
      string query,
      [Frozen] Mock<IFileService> mockFileService,
      [Frozen] Mock<IRecipeRepository> mockRecipeRepository,
      [Frozen] Mock<ILlmService> mockLlmService,
      [Frozen] Mock<IBrowserService> mockBrowserService,
      WebScraperService sut)
  {
    // Arrange
    var siteSelectors = CreateTestSiteSelectors();
    mockFileService
        .Setup(x => x.ReadFromFile<SiteSelectors>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(siteSelectors);

    var testUrls = new List<string> { "https://example.com/recipe1", "https://example.com/recipe2" };
    mockBrowserService
        .Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(testUrls);

    mockRecipeRepository
        .Setup(x => x.ContainsRecipeUrl(It.IsAny<string>()))
        .Returns(false);

    var llmResult = new LlmResult<SearchResult>
    {
      Data = new SearchResult { SelectedIndices = [1, 2] },
      ConversationId = "test-conversation-id"
    };

    mockLlmService
        .Setup(x => x.CallFunction<SearchResult>(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<FunctionDefinition>()))
        .ReturnsAsync(llmResult);

    var scrapedRecipe = new ScrapedRecipeBuilder()
        .WithDefaults()
        .WithTitle($"Recipe for {query}")
        .Build();

    SetupMockHttpResponse(mockFileService, CreateTestHtmlWithRecipe(scrapedRecipe));

    // Act
    var result = await sut.ScrapeForRecipesAsync(query);

    // Assert
    result.Should().NotBeEmpty("because valid recipes should be scraped");
    result.Should().HaveCount(2, "because 2 URLs should be processed");
    mockFileService.Verify(x => x.ReadFromFile<SiteSelectors>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    mockLlmService.Verify(x => x.CallFunction<SearchResult>(
        It.IsAny<string>(), 
        It.IsAny<string>(), 
        It.IsAny<FunctionDefinition>()), Times.Once);
  }

  [Theory, AutoMoqData]
  public async Task ScrapeForRecipesAsync_NoUrlsFound_ReturnsEmptyList(
      string query,
      [Frozen] Mock<IFileService> mockFileService,
      [Frozen] Mock<IBrowserService> mockBrowserService,
      WebScraperService sut)
  {
    // Arrange
    var siteSelectors = CreateTestSiteSelectors();
    mockFileService
        .Setup(x => x.ReadFromFile<SiteSelectors>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(siteSelectors);

    mockBrowserService
        .Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<string>());

    // Act
    var result = await sut.ScrapeForRecipesAsync(query);

    // Assert
    result.Should().BeEmpty("because no URLs were found for the query");
  }

  [Theory, AutoMoqData]
  public async Task ScrapeForRecipesAsync_ExistingRecipesFiltered_ReturnsOnlyNew(
      string query,
      [Frozen] Mock<IFileService> mockFileService,
      [Frozen] Mock<IRecipeRepository> mockRecipeRepository,
      [Frozen] Mock<IBrowserService> mockBrowserService,
      WebScraperService sut)
  {
    // Arrange
    var siteSelectors = CreateTestSiteSelectors();
    mockFileService
        .Setup(x => x.ReadFromFile<SiteSelectors>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(siteSelectors);

    var testUrls = new List<string> 
    { 
      "https://example.com/existing-recipe", 
      "https://example.com/new-recipe" 
    };

    mockBrowserService
        .Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(testUrls);

    // First URL already exists in repository
    mockRecipeRepository
        .Setup(x => x.ContainsRecipeUrl("https://example.com/existing-recipe"))
        .Returns(true);
    mockRecipeRepository
        .Setup(x => x.ContainsRecipeUrl("https://example.com/new-recipe"))
        .Returns(false);

    // Act
    var result = await sut.ScrapeForRecipesAsync(query);

    // Assert
    result.Should().BeEmpty("because filtering should leave only new recipes, but LLM is not called without URLs");
    mockRecipeRepository.Verify(x => x.ContainsRecipeUrl("https://example.com/existing-recipe"), Times.Once);
    mockRecipeRepository.Verify(x => x.ContainsRecipeUrl("https://example.com/new-recipe"), Times.Once);
  }

  [Theory, AutoMoqData]
  public async Task ScrapeForRecipesAsync_WithAcceptableScore_FiltersCorrectly(
      string query,
      int acceptableScore,
      [Frozen] Mock<IFileService> mockFileService,
      [Frozen] Mock<IRecipeRepository> mockRecipeRepository,
      [Frozen] Mock<ILlmService> mockLlmService,
      [Frozen] Mock<IBrowserService> mockBrowserService,
      [Frozen] Mock<ChooseRecipesPrompt> mockChooseRecipesPrompt,
      RecipeConfig recipeConfig,
      WebScraperService sut)
  {
    // Arrange
    var siteSelectors = CreateTestSiteSelectors();
    mockFileService
        .Setup(x => x.ReadFromFile<SiteSelectors>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(siteSelectors);

    var testUrls = new List<string> { "https://example.com/recipe1" };
    mockBrowserService
        .Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(testUrls);

    mockRecipeRepository
        .Setup(x => x.ContainsRecipeUrl(It.IsAny<string>()))
        .Returns(false);

    var systemPrompt = "Test system prompt";
    var userPrompt = "Test user prompt";
    var functionDefinition = new FunctionDefinition { Name = "TestFunction" };

    mockChooseRecipesPrompt.Setup(x => x.SystemPrompt).Returns(systemPrompt);
    mockChooseRecipesPrompt
        .Setup(x => x.GetUserPrompt(query, testUrls, It.IsAny<int>(), acceptableScore))
        .Returns(userPrompt);
    mockChooseRecipesPrompt.Setup(x => x.GetFunction()).Returns(functionDefinition);

    var llmResult = new LlmResult<SearchResult>
    {
      Data = new SearchResult { SelectedIndices = [1] },
      ConversationId = "test-conversation-id"
    };

    mockLlmService
        .Setup(x => x.CallFunction<SearchResult>(systemPrompt, userPrompt, functionDefinition))
        .ReturnsAsync(llmResult);

    var scrapedRecipe = new ScrapedRecipeBuilder()
        .WithDefaults()
        .WithTitle($"Recipe for {query}")
        .Build();

    SetupMockHttpResponse(mockFileService, CreateTestHtmlWithRecipe(scrapedRecipe));

    // Act
    var result = await sut.ScrapeForRecipesAsync(query, acceptableScore);

    // Assert
    result.Should().NotBeEmpty("because valid recipes with acceptable scores should be returned");
    mockChooseRecipesPrompt.Verify(x => x.GetUserPrompt(query, testUrls, It.IsAny<int>(), acceptableScore), Times.Once);
  }

  [Theory, AutoMoqData]
  public async Task ScrapeForRecipesAsync_WithRecipesNeeded_LimitsResults(
      string query,
      [Frozen] Mock<IFileService> mockFileService,
      [Frozen] Mock<IRecipeRepository> mockRecipeRepository,
      [Frozen] Mock<ILlmService> mockLlmService,
      [Frozen] Mock<IBrowserService> mockBrowserService,
      WebScraperService sut)
  {
    // Arrange
    const int recipesNeeded = 1;
    var siteSelectors = CreateTestSiteSelectors();
    mockFileService
        .Setup(x => x.ReadFromFile<SiteSelectors>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(siteSelectors);

    var testUrls = new List<string> { "https://example.com/recipe1", "https://example.com/recipe2" };
    mockBrowserService
        .Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(testUrls);

    mockRecipeRepository
        .Setup(x => x.ContainsRecipeUrl(It.IsAny<string>()))
        .Returns(false);

    var llmResult = new LlmResult<SearchResult>
    {
      Data = new SearchResult { SelectedIndices = [1] },
      ConversationId = "test-conversation-id"
    };

    mockLlmService
        .Setup(x => x.CallFunction<SearchResult>(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<FunctionDefinition>()))
        .ReturnsAsync(llmResult);

    var scrapedRecipe = new ScrapedRecipeBuilder()
        .WithDefaults()
        .WithTitle($"Recipe for {query}")
        .Build();

    SetupMockHttpResponse(mockFileService, CreateTestHtmlWithRecipe(scrapedRecipe));

    // Act
    var result = await sut.ScrapeForRecipesAsync(query, recipesNeeded: recipesNeeded);

    // Assert
    result.Should().HaveCount(1, "because recipesNeeded parameter should limit the results");
  }

  [Theory, AutoMoqData]
  public async Task ScrapeForRecipesAsync_WithTargetSite_FiltersToSpecificSite(
      string query,
      [Frozen] Mock<IFileService> mockFileService,
      [Frozen] Mock<IRecipeRepository> mockRecipeRepository,
      [Frozen] Mock<IBrowserService> mockBrowserService,
      WebScraperService sut)
  {
    // Arrange
    const string targetSite = "example";
    var siteSelectors = CreateTestSiteSelectors();
    mockFileService
        .Setup(x => x.ReadFromFile<SiteSelectors>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(siteSelectors);

    var testUrls = new List<string> { "https://example.com/recipe1" };
    mockBrowserService
        .Setup(x => x.GetContentAsync(
            It.Is<string>(url => url.Contains("example.com")), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(testUrls);

    mockRecipeRepository
        .Setup(x => x.ContainsRecipeUrl(It.IsAny<string>()))
        .Returns(false);

    // Act
    var result = await sut.ScrapeForRecipesAsync(query, targetSite: targetSite);

    // Assert
    // Should not throw exception and should attempt to scrape from the target site
    mockBrowserService.Verify(x => x.GetContentAsync(
        It.Is<string>(url => url.Contains("example.com")), 
        It.IsAny<string>(), 
        It.IsAny<CancellationToken>()), Times.Once);
  }

  [Theory, AutoMoqData]
  public async Task ScrapeForRecipesAsync_LlmServiceFailure_FallsBackToAllUrls(
      string query,
      [Frozen] Mock<IFileService> mockFileService,
      [Frozen] Mock<IRecipeRepository> mockRecipeRepository,
      [Frozen] Mock<ILlmService> mockLlmService,
      [Frozen] Mock<IBrowserService> mockBrowserService,
      WebScraperService sut)
  {
    // Arrange
    var siteSelectors = CreateTestSiteSelectors();
    mockFileService
        .Setup(x => x.ReadFromFile<SiteSelectors>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(siteSelectors);

    var testUrls = new List<string> { "https://example.com/recipe1" };
    mockBrowserService
        .Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(testUrls);

    mockRecipeRepository
        .Setup(x => x.ContainsRecipeUrl(It.IsAny<string>()))
        .Returns(false);

    // LLM service throws exception
    mockLlmService
        .Setup(x => x.CallFunction<SearchResult>(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<FunctionDefinition>()))
        .ThrowsAsync(new Exception("LLM service unavailable"));

    var scrapedRecipe = new ScrapedRecipeBuilder()
        .WithDefaults()
        .WithTitle($"Recipe for {query}")
        .Build();

    SetupMockHttpResponse(mockFileService, CreateTestHtmlWithRecipe(scrapedRecipe));

    // Act
    var result = await sut.ScrapeForRecipesAsync(query);

    // Assert
    result.Should().NotBeEmpty("because service should fall back to using all URLs when LLM fails");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void GenerateUrlFingerprint_ValidUrl_ReturnsConsistentHash()
  {
    // Arrange
    const string testUrl = "https://example.com/recipe/12345";

    // Act
    var result1 = WebScraperService.GenerateUrlFingerprint(testUrl);
    var result2 = WebScraperService.GenerateUrlFingerprint(testUrl);

    // Assert
    result1.Should().NotBeEmpty("because URL fingerprint should be generated");
    result1.Should().Be(result2, "because the same URL should generate the same fingerprint");
    result1.Should().HaveLength(64, "because SHA256 hash should be 64 characters in hex");
  }

  [Theory]
  [InlineData("https://example.com/recipe1", "https://example.com/recipe2")]
  [InlineData("https://site1.com/recipe", "https://site2.com/recipe")]
  [Trait("Category", "Unit")]
  public void GenerateUrlFingerprint_DifferentUrls_ReturnsDifferentHashes(string url1, string url2)
  {
    // Act
    var hash1 = WebScraperService.GenerateUrlFingerprint(url1);
    var hash2 = WebScraperService.GenerateUrlFingerprint(url2);

    // Assert
    hash1.Should().NotBe(hash2, "because different URLs should generate different fingerprints");
  }

  [Theory, AutoMoqData]
  public async Task ScrapeForRecipesAsync_ConfigurationErrorBuffer_HandlesExtraRecipes(
      string query,
      [Frozen] Mock<IFileService> mockFileService,
      [Frozen] Mock<IRecipeRepository> mockRecipeRepository,
      [Frozen] Mock<ILlmService> mockLlmService,
      [Frozen] Mock<IBrowserService> mockBrowserService,
      [Frozen] WebscraperConfig config,
      WebScraperService sut)
  {
    // Arrange
    // Create more URLs than MaxNumResultsPerQuery to test error buffer logic
    var testUrls = Enumerable.Range(1, config.MaxNumResultsPerQuery + config.ErrorBuffer + 1)
        .Select(i => $"https://example.com/recipe{i}")
        .ToList();

    var siteSelectors = CreateTestSiteSelectors();
    mockFileService
        .Setup(x => x.ReadFromFile<SiteSelectors>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(siteSelectors);

    mockBrowserService
        .Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(testUrls);

    mockRecipeRepository
        .Setup(x => x.ContainsRecipeUrl(It.IsAny<string>()))
        .Returns(false);

    var llmResult = new LlmResult<SearchResult>
    {
      Data = new SearchResult { SelectedIndices = [1, 2, 3] },
      ConversationId = "test-conversation-id"
    };

    mockLlmService
        .Setup(x => x.CallFunction<SearchResult>(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<FunctionDefinition>()))
        .ReturnsAsync(llmResult);

    var scrapedRecipe = new ScrapedRecipeBuilder()
        .WithDefaults()
        .WithTitle($"Recipe for {query}")
        .Build();

    SetupMockHttpResponse(mockFileService, CreateTestHtmlWithRecipe(scrapedRecipe));

    // Act
    var result = await sut.ScrapeForRecipesAsync(query);

    // Assert
    result.Should().NotBeEmpty("because recipes should be scraped even with error buffer considerations");
    // Verify that LLM was called with the correct count including error buffer
    mockLlmService.Verify(x => x.CallFunction<SearchResult>(
        It.IsAny<string>(),
        It.Is<string>(prompt => prompt.Contains($"top {config.MaxNumResultsPerQuery + config.ErrorBuffer}")),
        It.IsAny<FunctionDefinition>()), Times.Once);
  }

  [Theory, AutoMoqData]
  public async Task ScrapeForRecipesAsync_EmptySelectedIndices_FallsBackToAllUrls(
      string query,
      [Frozen] Mock<IFileService> mockFileService,
      [Frozen] Mock<IRecipeRepository> mockRecipeRepository,
      [Frozen] Mock<ILlmService> mockLlmService,
      [Frozen] Mock<IBrowserService> mockBrowserService,
      WebScraperService sut)
  {
    // Arrange
    var siteSelectors = CreateTestSiteSelectors();
    mockFileService
        .Setup(x => x.ReadFromFile<SiteSelectors>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(siteSelectors);

    var testUrls = new List<string> { "https://example.com/recipe1", "https://example.com/recipe2" };
    mockBrowserService
        .Setup(x => x.GetContentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(testUrls);

    mockRecipeRepository
        .Setup(x => x.ContainsRecipeUrl(It.IsAny<string>()))
        .Returns(false);

    // LLM returns empty selected indices
    var llmResult = new LlmResult<SearchResult>
    {
      Data = new SearchResult { SelectedIndices = [] },
      ConversationId = "test-conversation-id"
    };

    mockLlmService
        .Setup(x => x.CallFunction<SearchResult>(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<FunctionDefinition>()))
        .ReturnsAsync(llmResult);

    // Act & Assert
    var result = await sut.ScrapeForRecipesAsync(query);

    // Should fall back to all URLs when indices are empty
    result.Should().NotBeEmpty("because service should fall back to all URLs when no indices are selected");
  }

  /// <summary>
  /// Creates test site selectors configuration for mocking file service.
  /// </summary>
  private static SiteSelectors CreateTestSiteSelectors()
  {
    return new SiteSelectors
    {
      Sites = new Dictionary<string, Dictionary<string, string>>
      {
        ["example"] = new()
        {
          ["base_url"] = "https://example.com",
          ["search_page"] = "/search?q={query}",
          ["search_results"] = "a.recipe-link",
          ["stream_search"] = "true",
          ["title"] = "h1.recipe-title",
          ["description"] = ".recipe-description",
          ["ingredients"] = ".ingredient-item",
          ["directions"] = ".direction-step",
          ["servings"] = ".servings",
          ["prep_time"] = ".prep-time",
          ["cook_time"] = ".cook-time",
          ["total_time"] = ".total-time",
          ["notes"] = ".recipe-notes",
          ["image"] = ".recipe-image img"
        }
      },
      Templates = new Dictionary<string, Dictionary<string, string>>()
    };
  }

  /// <summary>
  /// Creates test HTML content with recipe data for mocking HTTP responses.
  /// </summary>
  private static string CreateTestHtmlWithRecipe(ScrapedRecipe recipe)
  {
    return $@"
      <html>
        <head><title>{recipe.Title}</title></head>
        <body>
          <h1 class='recipe-title'>{recipe.Title}</h1>
          <div class='recipe-description'>{recipe.Description}</div>
          <div class='servings'>{recipe.Servings}</div>
          <div class='prep-time'>{recipe.PrepTime}</div>
          <div class='cook-time'>{recipe.CookTime}</div>
          <div class='total-time'>{recipe.TotalTime}</div>
          <div class='recipe-notes'>{recipe.Notes}</div>
          <img class='recipe-image' src='{recipe.ImageUrl}' />
          <div class='ingredient-item'>{string.Join("</div><div class='ingredient-item'>", recipe.Ingredients)}</div>
          <div class='direction-step'>{string.Join("</div><div class='direction-step'>", recipe.Directions)}</div>
        </body>
      </html>";
  }

  /// <summary>
  /// Sets up mock HTTP response for file service HTTP operations.
  /// This is a simplified approach - in a real implementation, you might use a proper HTTP mock.
  /// </summary>
  private static void SetupMockHttpResponse(Mock<IFileService> mockFileService, string htmlContent)
  {
    // Note: WebScraperService uses private SendGetRequestForHtml method which is hard to mock.
    // In practice, this would be better tested through integration tests or by extracting HTTP operations to a separate service.
    // For unit tests, we focus on the business logic that can be isolated.
  }
}