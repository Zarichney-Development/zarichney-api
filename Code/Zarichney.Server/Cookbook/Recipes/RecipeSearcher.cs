namespace Zarichney.Cookbook.Recipes;

public interface IRecipeSearcher
{
  Task<List<Recipe>> SearchRecipes(
    string query,
    int? minimumScore = null,
    int? requiredCount = null,
    CancellationToken cancellationToken = default);
}

public class RecipeSearcher(
  IRecipeIndexer recipeIndexer,
  ILogger<RecipeSearcher> logger,
  RecipeConfig config
)
  : IRecipeSearcher
{
  /// <summary>
  /// Primary entry point for searching recipes by query, returning up to `requiredCount`.
  /// - First prioritizes any with LLM Relevancy >= minimumScore,
  /// - Then includes fallback matches (title/alias, fuzzy matches).
  /// - Returns enough to fill `requiredCount` (if available).
  /// </summary>
  public Task<List<Recipe>> SearchRecipes(
    string query,
    int? minimumScore = null,
    int? requiredCount = null,
    CancellationToken cancellationToken = default)
  {
    requiredCount ??= config.MaxSearchResults;
    query = NormalizeQuery(query);

    ValidateSearchParameters(query, minimumScore, requiredCount);

    logger.LogInformation(
      "Starting recipe search (Modified). Query='{Query}', MinScore={MinScore}, RequiredCount={ReqCount}",
      query, minimumScore, requiredCount);

    // 1) Gather all potential matches (both exact and fuzzy).
    var allPotentialResults = new Dictionary<string, RepositorySearchResult>();

    // ---- Phase A: Exact dictionary key matches ----
    if (recipeIndexer.TryGetExactMatches(query, out var exactMatchDict))
    {
      foreach (var recipe in exactMatchDict.Values)
      {
        AddOrUpdateResult(allPotentialResults, recipe, query);
      }
    }

    // ---- Phase B: Exact alias matches (in case not indexed directly under the alias) ----
    var allIndexed = recipeIndexer.GetAllRecipes().ToList();
    foreach (var recipe in from kvp in allIndexed
                           from recipe in kvp.Value.Values
                           where !allPotentialResults.ContainsKey(recipe.Id!)
                           select recipe)
    {
      if (cancellationToken.IsCancellationRequested)
        return Task.FromResult(new List<Recipe>());

      var hasExactAlias = recipe.Aliases.Any(a => NormalizeQuery(a) == query);
      if (hasExactAlias)
      {
        AddOrUpdateResult(allPotentialResults, recipe, query);
      }
    }

    // ---- Phase C: Fuzzy matches ----
    // Only do fuzzy if we haven't yet reached enough "definitely relevant" items.
    if (!WeHaveEnoughRelevant(allPotentialResults.Values, minimumScore, requiredCount))
    {
      // (C1) Fuzzy match on each dictionary key
      foreach (var kvp in allIndexed)
      {
        if (cancellationToken.IsCancellationRequested)
          return Task.FromResult(new List<Recipe>());

        if (IsFuzzyMatch(kvp.Key, query))
        {
          foreach (var recipe in kvp.Value.Values)
          {
            if (allPotentialResults.ContainsKey(recipe.Id!))
              continue;
            AddOrUpdateResult(allPotentialResults, recipe, query);

            if (WeHaveEnoughRelevant(allPotentialResults.Values, minimumScore, requiredCount))
              break;
          }
        }

        if (WeHaveEnoughRelevant(allPotentialResults.Values, minimumScore, requiredCount))
          break;
      }

      // (C2) Fuzzy match on aliases
      if (!WeHaveEnoughRelevant(allPotentialResults.Values, minimumScore, requiredCount))
      {
        var distinctRecipes = allIndexed
          .SelectMany(kvp => kvp.Value.Values)
          .DistinctBy(r => r.Id!)
          .ToList();

        foreach (var recipe in distinctRecipes)
        {
          if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(new List<Recipe>());
          if (allPotentialResults.ContainsKey(recipe.Id!))
            continue;

          if (recipe.Aliases.Any(a => IsFuzzyMatch(a, query)))
          {
            AddOrUpdateResult(allPotentialResults, recipe, query);

            if (WeHaveEnoughRelevant(allPotentialResults.Values, minimumScore, requiredCount))
              break;
          }
        }
      }
    }

    // 2) Partition results into "already relevant" vs. fallback
    var relevantList = new List<RepositorySearchResult>();
    var fallbackList = new List<RepositorySearchResult>();

    foreach (var res in allPotentialResults.Values)
    {
      if (res.RelevancyScore >= (minimumScore ?? 0))
        relevantList.Add(res);
      else
        fallbackList.Add(res);
    }

    // 3) Sort each subset individually
    relevantList = relevantList
      .OrderByDescending(r => ComputeFinalScore(r.Score, r.RelevancyScore))
      .ToList();

    fallbackList = fallbackList
      .OrderByDescending(r => ComputeFinalScore(r.Score, r.RelevancyScore))
      .ToList();

    // 4) Combine them: first definitely relevant, then fallback until we hit requiredCount
    var combined = new List<Recipe>((int)requiredCount);

    // Take only up to requiredCount from relevant results
    combined.AddRange(relevantList.Take((int)requiredCount).Select(r => r.Recipe));

    if (combined.Count < (int)requiredCount)
    {
      var spotsLeft = (int)requiredCount - combined.Count;
      // Only add fallback items if minimumScore is not set or if the original requiredCount was explicitly set
      // This prevents adding low-scoring items when user specified a minimumScore but not requiredCount
      var shouldAddFallback = !minimumScore.HasValue || (requiredCount != config.MaxSearchResults);

      if (shouldAddFallback)
      {
        combined.AddRange(fallbackList
          .Select(r => r.Recipe)
          .Take(spotsLeft));
      }
    }

    logger.LogInformation(
      "Found {Count} total matches, returning {FinalCount} (Relevant={RelCount}, Fallback={FbkCount})",
      allPotentialResults.Count, combined.Count, relevantList.Count, fallbackList.Count);

    return Task.FromResult(combined);
  }

  /// <summary>
  /// Helper that calculates or updates a match in the collection.
  /// Uses "title/alias" matching for Score (0..1),
  /// and existing LLM-based Relevancy (0..100) for RelevancyScore.
  /// </summary>
  private static void AddOrUpdateResult(
    Dictionary<string, RepositorySearchResult> results,
    Recipe recipe,
    string query)
  {
    var matchScore = CalculateTitleAliasScore(recipe, query); // 0..1
    var relevancyVal = GetRelevancyScore(recipe, query); // 0..100

    results[recipe.Id!] = new RepositorySearchResult
    {
      Recipe = recipe,
      Score = matchScore,
      RelevancyScore = relevancyVal
    };
  }

  /// <summary>
  /// Helper that checks if we have "enough" results meeting the relevancy threshold.
  /// </summary>
  private static bool WeHaveEnoughRelevant(
    IEnumerable<RepositorySearchResult> results,
    int? minScore,
    int? requiredCount)
  {
    if (!minScore.HasValue || !requiredCount.HasValue)
      return false;

    var countMeetingScore = results.Count(r => r.RelevancyScore >= minScore.Value);
    return countMeetingScore >= requiredCount.Value;
  }

  /// <summary>
  /// Final weighting function that merges the 0..1 "title score" + 0..100 "relevancy".
  /// Adjust the ratio to your preference.
  /// </summary>
  private static double ComputeFinalScore(double matchScore, double relevancyScore)
  {
    // Example weighting: 80% LLM-based relevancy, 20% title/alias match
    return (relevancyScore / 100.0 * 0.8) + (matchScore * 0.2);
  }

  private static string NormalizeQuery(string query)
  {
    if (string.IsNullOrWhiteSpace(query))
      return string.Empty;

    // Collapse multiple whitespace into single spaces
    return System.Text.RegularExpressions.Regex.Replace(query.Trim(), @"\s+", " ").ToLowerInvariant();
  }

  private static void ValidateSearchParameters(string query, int? minScore, int? requiredCount)
  {
    if (string.IsNullOrEmpty(query))
    {
      throw new ArgumentException("Search query cannot be empty.", nameof(query));
    }

    // If these constraints aren't relevant, you can remove them
    if (minScore is <= 0 or >= 100)
    {
      throw new ArgumentException("Minimum score must be between 1 and 99.", nameof(minScore));
    }

    if (requiredCount is <= 0)
    {
      throw new ArgumentException("Required count must be greater than 0.", nameof(requiredCount));
    }
  }

  /// <summary>
  /// Very simple substring-based "fuzzy" check:
  /// Does `str` contain `query` or vice versa?
  /// </summary>
  private static bool IsFuzzyMatch(string str, string query)
  {
    var normStr = NormalizeQuery(str);
    return normStr.Contains(query) || query.Contains(normStr);
  }

  /// <summary>
  /// Key method: calculates a 0..1 measure of how well the recipe's title or any alias
  /// overlaps with the query text.
  ///
  /// - Splits the query into words, checks how many appear in (title + aliases).
  /// - If only 1 out of 7 query words are found, the overlap is 1/7 = ~0.14 (low).
  /// - If 6 out of 7 words match, the overlap is ~0.86 (high).
  /// </summary>
  private static double CalculateTitleAliasScore(Recipe recipe, string query)
  {
    // Quick checks
    if (string.IsNullOrEmpty(recipe.Title))
      return 0.0;
    if (string.IsNullOrEmpty(query))
      return 0.0;

    // 1) Gather all words from the recipe's title + aliases
    var titleWords = NormalizeQuery(recipe.Title).Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var aliasWords = recipe.Aliases
      .SelectMany(a => NormalizeQuery(a).Split(' ', StringSplitOptions.RemoveEmptyEntries))
      .ToHashSet(); // distinct

    // Combine into a single set for quick membership checking
    // (some folks prefer to keep them separate if they want different weighting for titles vs aliases)
    var recipeWords = titleWords.Concat(aliasWords).ToHashSet();

    // 2) Split the query text into words
    var queryWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    // 3) Count how many of those words appear in recipeWords
    var matchedCount = queryWords.Count(recipeWords.Contains);

    // 4) The overlap ratio is matchedCount / total query words
    // If the user typed 8 words, but only 1 is found (like "broccoli"), ratio = 1/8 => 0.125
    var overlapRatio = (double)matchedCount / queryWords.Length;

    // Optionally, do a small bonus for an exact phrase match, e.g.:
    // if (NormalizeQuery(recipe.Title).Contains(query)) overlapRatio = Math.Max(overlapRatio, 0.9);

    return overlapRatio;
  }

  /// <summary>
  /// Retrieve the known LLM-based relevancy from 0..100 for the given query (if present).
  /// Otherwise 0 if no rating is found.
  /// </summary>
  private static double GetRelevancyScore(Recipe recipe, string query)
  {
    if (recipe.Relevancy.Count == 0)
      return 0.0;

    if (recipe.Relevancy.TryGetValue(query, out var exactMatch))
    {
      return exactMatch.Score;
    }

    // fallback if we stored it under a different case
    var possibleKeys = recipe.Relevancy.Keys
      .Where(k => NormalizeQuery(k) == query)
      .ToList();

    if (possibleKeys.Count > 0)
    {
      return possibleKeys
        .Select(pk => recipe.Relevancy[pk].Score)
        .Max();
    }

    return 0.0;
  }
}
