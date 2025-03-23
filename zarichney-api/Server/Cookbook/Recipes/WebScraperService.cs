using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using Zarichney.Server.Config;
using Zarichney.Server.Cookbook.Prompts;
using Zarichney.Server.Services;

namespace Zarichney.Server.Cookbook.Recipes;

public class WebscraperConfig : IConfig
{
  public int MaxNumResultsPerQuery { get; init; } = 3;
  public int MaxParallelTasks { get; init; } = 5;
  public int MaxParallelSites { get; init; } = 5;
  public int MaxWaitTimeMs { get; init; } = 10000;
  public int MaxParallelPages { get; init; } = 2;
  public int ErrorBuffer { get; init; } = 5; // in case of scraping errors, can fallback up to this amount
}

public class WebScraperService(
  WebscraperConfig config,
  RecipeConfig recipeConfig,
  ChooseRecipesPrompt chooseRecipesPrompt,
  ILlmService llmService,
  IFileService fileService,
  IBrowserService browserService,
  ILogger<WebScraperService> logger,
  IRecipeRepository recipeRepository
)
{
  private static Dictionary<string, Dictionary<string, string>>? _siteSelectors;
  private static Dictionary<string, Dictionary<string, string>>? _siteTemplates;

  internal async Task<List<ScrapedRecipe>> ScrapeForRecipesAsync(string query, int? acceptableScore = null,
    int? recipesNeeded = null,
    string? targetSite = null)
  {
    await LoadSiteSelectors();

    var sitesToProcess = GetSitesToProcess(targetSite);
    var urlsBySite = await CollectUrlsFromAllSitesAsync(sitesToProcess, query); // Dic<site_key, List<url>>

    if (urlsBySite.Count == 0)
    {
      logger.LogInformation("No recipe URLs found for query: {query}", query);
      return [];
    }

    // Filter down list for all those that already exist in the repository
    urlsBySite = urlsBySite
      .ToDictionary(r => r.Key, r => r.Value
        .Where(s => !recipeRepository.ContainsRecipeUrl(s)).ToList());

    var rankedUrls = await RankUrlsByRelevanceAsync(urlsBySite, query, acceptableScore, recipesNeeded);
    var recipes = await ScrapeRecipesInParallelAsync(rankedUrls, query, recipesNeeded);
    logger.LogInformation("Web scraped a total of {count} recipes for {query}", recipes.Count, query);

    return recipes;
  }

  /// <summary>
  /// Gets the sites to process based on the optional target site
  /// </summary>
  private IEnumerable<KeyValuePair<string, Dictionary<string, string>>> GetSitesToProcess(string? targetSite)
  {
    return _siteSelectors!.Where(site =>
      string.IsNullOrEmpty(targetSite) || site.Key == targetSite);
  }

  /// <summary>
  /// Collects recipe URLs from all sites in parallel
  /// </summary>
  private async Task<Dictionary<string, List<string>>> CollectUrlsFromAllSitesAsync(
    IEnumerable<KeyValuePair<string, Dictionary<string, string>>> sitesToProcess,
    string query, CancellationToken cancellationToken = default)
  {
    var urlsBySite = new ConcurrentDictionary<string, List<string>>();

    await Parallel.ForEachAsync(sitesToProcess,
      new ParallelOptions
      {
        MaxDegreeOfParallelism = config.MaxParallelSites,
        CancellationToken = cancellationToken
      }, async (site, ct) =>
      {
        try
        {
          var urls = await SearchSiteForRecipeUrls(site.Key, query, ct);
          if (urls.Count > 0)
          {
            urlsBySite.TryAdd(site.Key, urls);
          }
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error collecting URLs from site: {site}", site.Key);
        }
      });

    return new Dictionary<string, List<string>>(urlsBySite);
  }

  /// <summary>
  /// Filters all collected URLs for relevance in a single batch
  /// </summary>
  private async Task<List<(string SiteKey, string Url)>> RankUrlsByRelevanceAsync(
    Dictionary<string, List<string>> urlsBySite, string query, int? acceptableScore, int? recipesNeeded = null)
  {
    // Combine all URLs while maintaining site information
    var allUrlsWithSites = urlsBySite
      .SelectMany(kvp => kvp.Value.Select(url => (SiteKey: kvp.Key, Url: url)))
      .ToList();

    var relevantUrls =
      await SelectMostRelevantUrls(allUrlsWithSites.Select(x => x.Url).ToList(), query, acceptableScore, recipesNeeded);

    // Regroup relevant URLs by site
    return relevantUrls
      .Select(url => allUrlsWithSites.First(x => x.Url == url))
      .ToList();
  }

  /// <summary>
  /// Scrapes recipes from the filtered relevant URLs
  /// </summary>
  private async Task<List<ScrapedRecipe>> ScrapeRecipesInParallelAsync(
    List<(string SiteKey, string Url)> rankedUrls,
    string query,
    int? recipesNeeded = null,
    CancellationToken externalCancellation = default)
  {
    var recipes = new ConcurrentDictionary<int, ScrapedRecipe>();
    recipesNeeded ??= rankedUrls.Count; // Default to all URLs

    // Create a linked cancellation source that respects external cancellation
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCancellation);

    try
    {
      await Parallel.ForEachAsync(rankedUrls.Select((url, index) => (url.SiteKey, url.Url, Index: index)),
        new ParallelOptions
        {
          MaxDegreeOfParallelism = config.MaxParallelSites,
          CancellationToken = cts.Token
        },
        async (item, ct) =>
        {
          // Check cancellation first
          if (ct.IsCancellationRequested)
          {
            return;
          }

          try
          {
            // Scrape single URL
            var scrapedRecipes = await ScrapeSiteForRecipesAsync(item.SiteKey, [item.Url], query, ct);

            if (scrapedRecipes.Count > 0)
            {
              if (recipes.TryAdd(item.Index, scrapedRecipes[0]))
              {
                // Cancel remaining operations if we have enough recipes
                if (recipes.Count >= recipesNeeded)
                {
                  await cts.CancelAsync();
                }
              }
            }
          }
          catch (OperationCanceledException)
          {
            // Expected when we cancel after getting enough recipes
          }
          catch (Exception ex)
          {
            logger.LogError(ex, "Error scraping recipe from {site}: {url}", item.SiteKey, item.Url);
            throw; // Rethrow to allow handling by calling code
          }
        });
    }
    catch (OperationCanceledException)
    {
      // Operation was cancelled either externally or because we found enough recipes
      logger.LogInformation("Recipe scraping operation cancelled after collecting {count} recipes", recipes.Count);
    }

    // Return recipes in original relevancy order
    return recipes
      .OrderBy(kvp => kvp.Key)
      .Select(kvp => kvp.Value)
      .ToList();
  }

  private async Task<List<string>> SelectMostRelevantUrls(List<string> recipeUrls, string? query, int? acceptableScore,
    int? recipesNeeded = null)
  {
    var maxResults = recipesNeeded ?? config.MaxNumResultsPerQuery;
    if (recipeUrls.Count <= maxResults)
    {
      return recipeUrls;
    }

    try
    {
      var result = await llmService.CallFunction<SearchResult>(
        chooseRecipesPrompt.SystemPrompt,
        chooseRecipesPrompt.GetUserPrompt(
          query,
          recipeUrls,
          maxResults + config.ErrorBuffer,
          acceptableScore ?? recipeConfig.AcceptableScoreThreshold
        ),
        chooseRecipesPrompt.GetFunction()
      );

      var indices = result.SelectedIndices;

      if (indices.Count == 0)
      {
        throw new Exception("No indices selected");
      }

      var selectedUrls = recipeUrls
        .Where((_, index) => indices.Contains(index + 1))
        .Take(maxResults)
        .ToList();
      logger.LogInformation("Selected {count} URLs for {query}", selectedUrls.Count, query);

      if (selectedUrls.Count == 0)
      {
        throw new Exception("Index mismatch issue");
      }

      return selectedUrls;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error selecting URLs for {query}. Urls: {@Urls}", query, recipeUrls);
    }

    return recipeUrls;
  }

  private async Task<List<string>> SearchSiteForRecipeUrls(string site, string query,
    CancellationToken cancellationToken = default)
  {
    var selectors = _siteSelectors![site];
    var siteUrl = selectors.GetValueOrDefault("base_url", $"https://{site}.com");

    var searchQuery = selectors["search_page"];

    if (searchQuery.Contains("{query}"))
    {
      searchQuery = searchQuery.Replace("{query}", Uri.EscapeDataString(query));
    }
    else
    {
      searchQuery += Uri.EscapeDataString(query);
    }

    var searchUrl = $"{siteUrl}{searchQuery}";

    var isStreamSearch = selectors.TryGetValue("stream_search", out var streamSearchValue) &&
                         streamSearchValue == "true";

    var recipeUrls = isStreamSearch
      ? await browserService.GetContentAsync(searchUrl, selectors["search_results"], cancellationToken)
      : await ExtractRecipeUrls(searchUrl, selectors);

    if (recipeUrls.Count == 0)
    {
      logger.LogInformation("No search results found for '{query}' on site {site}", query, site);
      return [];
    }

    logger.LogInformation("Returned {count} search results for recipe '{query}' on site: {site}", recipeUrls.Count,
      query, site);
    return recipeUrls;
  }

  private async Task<List<string>> ExtractRecipeUrls(string url, Dictionary<string, string> selectors)
  {
    // Extract the html from the search page
    var html = await SendGetRequestForHtml(url);

    if (string.IsNullOrEmpty(html))
    {
      logger.LogWarning("Failed to retrieve HTML content for URL: {url}", url);
      return [];
    }

    var browserContext = BrowsingContext.New(Configuration.Default);
    var htmlDoc = await browserContext.OpenAsync(req => req.Content(html));

    var searchResultSelector = selectors["search_results"].Replace("\\\"", "\"");

    var links = htmlDoc.QuerySelectorAll(searchResultSelector);
    return links.Select(link => link.GetAttribute("href"))
      .Where(urlAttribute => !string.IsNullOrEmpty(urlAttribute))
      .Distinct()
      .ToList()!;
  }

  private async Task<List<ScrapedRecipe>> ScrapeSiteForRecipesAsync(string site, List<string> recipeUrls,
    string? query, CancellationToken cancellationToken = default)
  {
    var scrapedRecipes = new ConcurrentBag<ScrapedRecipe>();

    logger.LogInformation("Scraping {count} recipes from site '{site}' for recipe '{recipe}'",
      recipeUrls.Count, site, query);

    await Parallel.ForEachAsync(recipeUrls, new ParallelOptions
      {
        MaxDegreeOfParallelism = config.MaxParallelTasks,
        CancellationToken = cancellationToken
      },
      async (url, ct) =>
      {
        try
        {
          var fullUrl = url switch
          {
            _ when url.StartsWith("https://") => url,
            _ when url.StartsWith("//") => $"https:{url}",
            _ => $"{_siteSelectors![site]["base_url"]}{url}"
          };

          try
          {
            logger.LogInformation("Scraping {recipe} recipe from {url}", query, url);
            var recipe = await ParseRecipeFromSite(fullUrl, _siteSelectors![site], ct);
            scrapedRecipes.Add(recipe);
          }
          catch (Exception ex)
          {
            logger.LogWarning(ex, $"Error parsing recipe from URL: {fullUrl}");
          }
        }
        catch (Exception ex)
        {
          logger.LogError(ex, $"Error in scraping URL: {url}");
        }
      });

    return scrapedRecipes.ToList();
  }

  private async Task<ScrapedRecipe> ParseRecipeFromSite(string url, Dictionary<string, string> selectors,
    CancellationToken ct = default)
  {
    var html = await SendGetRequestForHtml(url, ct);
    if (string.IsNullOrEmpty(html))
    {
      throw new Exception("Failed to retrieve HTML content");
    }

    var browserContext = BrowsingContext.New(Configuration.Default);
    var htmlDoc = await browserContext.OpenAsync(req => req.Content(html), cancel: ct);

    string? imageUrl = null;
    try
    {
      imageUrl = ExtractText(htmlDoc, selectors["image"], "data-lazy-src") ??
                 ExtractText(htmlDoc, selectors["image"], "src") ??
                 ExtractText(htmlDoc, selectors["image"], "srcset")?.Split(" ")[0];
    }
    catch (Exception)
    {
      logger.LogDebug("No image found for {url}", url);
    }

    return new ScrapedRecipe
    {
      Id = GenerateUrlFingerprint(url),
      Ingredients = ExtractList(htmlDoc, selectors["ingredients"])
                    ?? throw new Exception("No ingredients found"),
      Directions = ExtractList(htmlDoc, selectors["directions"])
                   ?? throw new Exception("No ingredients found"),
      RecipeUrl = url,
      Title = ExtractText(htmlDoc, selectors["title"]),
      Description = ExtractText(htmlDoc, selectors["description"]),
      Servings = ExtractText(htmlDoc, selectors["servings"]),
      PrepTime = ExtractText(htmlDoc, selectors["prep_time"]),
      CookTime = ExtractText(htmlDoc, selectors["cook_time"]),
      TotalTime = ExtractText(htmlDoc, selectors["total_time"]),
      Notes = ExtractText(htmlDoc, selectors["notes"]),
      ImageUrl = imageUrl,
    };
  }

  public static string GenerateUrlFingerprint(string url)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url));
    var builder = new StringBuilder();
    foreach (var b in bytes)
    {
      builder.Append(b.ToString("x2"));
    }

    return builder.ToString();
  }

  private string? ExtractText(IDocument document, string selector, string? attribute = null)
    => ExtractTextFromHtmlDoc(document, selector, attribute);

  private string? ExtractTextFromHtmlDoc(IDocument document, string selector, string? attribute = null)
  {
    if (string.IsNullOrEmpty(selector)) return null;

    try
    {
      var element = document.QuerySelector(selector);
      if (element != null)
      {
        return attribute != null ? element.GetAttribute(attribute)! : element.TextContent.Trim();
      }
    }
    catch (Exception e)
    {
      logger.LogError(e, $"Error occurred with selector {selector} during extract_text");
    }

    return null;
  }

  private async Task<string> SendGetRequestForHtml(string url, CancellationToken ctsToken = default)
  {
    try
    {
      logger.LogInformation("Running GET request for URL: {url}", url);

      // Create an HttpClientHandler with automatic decompression
      var handler = new HttpClientHandler
      {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
      };

      using var client = new HttpClient(handler);

      // Create the request message
      var request = new HttpRequestMessage(HttpMethod.Get, url);

      // Add headers to mimic a real browser
      request.Headers.Add("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
      request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
      request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
      // Note: AutomaticDecompression handles Accept-Encoding

      // Send the request
      var response = await client.SendAsync(request, ctsToken);
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadAsStringAsync(ctsToken);
    }
    catch (HttpRequestException e)
    {
      logger.LogWarning(e, "HTTP error occurred for url {urL}", url);
    }
    catch (TaskCanceledException e)
    {
      logger.LogWarning(e, "Timeout occurred for url {urL}", url);
    }
    catch (Exception e)
    {
      logger.LogWarning(e, "Error occurred during GetHtmlAsync for url {urL}", url);
    }

    return null!;
  }

  private static List<string>? ExtractList(IDocument document, string selector)
  {
    var results = document.QuerySelectorAll(selector).Select(e => e.TextContent.Trim()).ToList();
    return results.Count == 0 ? null : results;
  }

  private async Task LoadSiteSelectors()
  {
    if (_siteSelectors != null && _siteTemplates != null)
      return;

    var selectorsData = await fileService.ReadFromFile<SiteSelectors>("Config", "site_selectors");
    _siteTemplates = selectorsData.Templates;
    _siteSelectors = new Dictionary<string, Dictionary<string, string>>();

    // Process sites and apply templates
    foreach (var (siteKey, siteConfig) in selectorsData.Sites)
    {
      if (siteConfig.TryGetValue("use_template", out var templateName))
      {
        if (!_siteTemplates.TryGetValue(templateName, out var templateConfig))
        {
          throw new Exception($"Template {templateName} not found for site {siteKey}");
        }

        // Merge template and site config
        var mergedConfig = new Dictionary<string, string>(templateConfig);

        // Overwrite with site-specific values
        foreach (var kvp in siteConfig)
        {
          mergedConfig[kvp.Key] = kvp.Value;
        }

        _siteSelectors[siteKey] = mergedConfig;
      }
      else
      {
        _siteSelectors[siteKey] = siteConfig;
      }
    }
  }
}

class SiteSelectors
{
  public Dictionary<string, Dictionary<string, string>> Sites { get; set; } = new();
  public Dictionary<string, Dictionary<string, string>> Templates { get; set; } = new();
}