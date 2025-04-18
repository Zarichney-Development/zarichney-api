# Module/Directory: /Unit/Cookbook/Recipes/RecipeService

**Last Updated:** 2025-04-18

> **Parent:** [`Recipes`](../README.md)
> **Related:**
> * **Source:** [`Cookbook/Recipes/RecipeService.cs`](../../../../../api-server/Cookbook/Recipes/RecipeService.cs)
> * **Interface:** [`Cookbook/Recipes/IRecipeService.cs`](../../../../../api-server/Cookbook/Recipes/RecipeService.cs) (Implicit)
> * **Dependencies:** `IRecipeRepository`, `IRecipeSearcher`, `IWebScraperService`, `ILlmService`, `ISessionManager`, `ILogger<RecipeService>`
> * **Prompts:** Various classes inheriting from `PromptBase` (e.g., `RankRecipe`, `AnalyzeRecipe`, `SynthesizeRecipe`, `GetAlternativeQuery`)
> * **Models:** [`Cookbook/Recipes/RecipeModels.cs`](../../../../../api-server/Cookbook/Recipes/RecipeModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `RecipeService` class. This is a core orchestration service responsible for complex logic involving finding, retrieving, ranking, analyzing, and synthesizing recipes by coordinating calls to repositories, searchers, web scrapers, and AI language models (via `ILlmService` and specific prompts).

* **Why Unit Tests?** To validate the intricate orchestration logic, conditional branching, and error handling within `RecipeService` in isolation from its numerous dependencies. Tests ensure the service correctly sequences calls to mocked dependencies based on different inputs and intermediate results, ultimately producing the expected outcome (e.g., a list of suitable recipes, a synthesized recipe).
* **Isolation:** Achieved by mocking all major dependencies: `IRecipeRepository`, `IRecipeSearcher`, `IWebScraperService`, `ILlmService`, `ISessionManager`, and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `RecipeService` focus on its complex public methods:

* **`GetRecipesAsync(...)`:**
    * Initial recipe retrieval logic (using mocked `IRecipeSearcher` and `IRecipeRepository`).
    * Logic for checking if retrieved recipes meet criteria (e.g., score threshold).
    * Conditional invocation of recipe ranking (mocked `ILlmService` with `RankRecipePrompt`).
    * Conditional invocation of web scraping (mocked `IWebScraperService`) if insufficient recipes are found and scraping is enabled.
    * Conditional invocation of alternative query generation (mocked `ILlmService` with `GetAlternativeQueryPrompt`) if initial search yields poor results.
    * Handling the loop for retrying searches with alternative queries.
    * Enforcing maximum attempt limits.
    * Aggregating and returning the final list of recipes.
    * Handling exceptions from any dependency.
* **`SynthesizeRecipeAsync(...)`:**
    * Calling mocked `ILlmService` with `SynthesizeRecipePrompt`.
    * Calling mocked `ILlmService` with `AnalyzeRecipePrompt` to evaluate the synthesized recipe.
    * Handling the feedback loop logic if analysis indicates revision is needed.
    * Handling maximum revision attempts.
    * Returning the final synthesized recipe upon success.
    * Handling exceptions from `ILlmService`.
* **`RankUnrankedRecipesAsync(...)`:**
    * Calling mocked `IRecipeRepository` to fetch unranked recipes.
    * Calling mocked `ILlmService` with `RankRecipePrompt` for each unranked recipe.
    * Calling mocked `IRecipeRepository` to update recipes with new scores.
* **`GetSearchQueryForRecipe(...)`:**
    * Interaction with mocked `ISessionManager` to get context.
    * Calling mocked `ILlmService` with `GetAlternativeQueryPrompt`.
    * Handling the `LlmResult` containing the alternative query.

## 3. Test Environment Setup

* **Instantiation:** `RecipeService` is instantiated directly.
* **Mocking:** Extensive mocking using Moq is required. Key mocks include:
    * `Mock<IRecipeRepository>`: Setup `GetRecipeByIdAsync`, `SearchRecipesAsync`, `AddRecipeAsync`, `UpdateRecipeAsync`, etc., to return various results (recipes, nulls, empty lists, specific scores).
    * `Mock<IRecipeSearcher>`: Setup `SearchRecipesAsync` to return results or empty lists.
    * `Mock<IWebScraperService>`: Setup `ScrapeRecipeAsync` to return scraped recipes or null/exceptions.
    * `Mock<ILlmService>`: Setup `GenerateResponseAsync` to return different `LlmResult` objects based on the specific prompt type (`RankRecipePrompt`, `AnalyzeRecipePrompt`, etc.) being simulated. This requires careful setup for different scenarios (e.g., high/low analysis scores, specific alternative queries).
    * `Mock<ISessionManager>`: Setup `GetOrCreateSessionAsync` to return session objects with relevant context.
    * `Mock<ILogger<RecipeService>>`.

## 4. Maintenance Notes & Troubleshooting

* **Complexity:** These tests are inherently complex due to the service's orchestration role. Clearly name tests based on the specific scenario/path being tested.
* **Dependency Mocking:** Setting up mocks, especially for `ILlmService` to simulate different prompt outcomes, requires careful planning for each test case. Ensure mocks accurately reflect the expected behavior needed to exercise specific logic paths in `RecipeService`.
* **Flow Logic:** Changes to the workflow within `GetRecipesAsync` or `SynthesizeRecipeAsync` (e.g., changing thresholds, altering the order of operations) will likely require significant test updates.

## 5. Test Cases & TODOs

### `RecipeServiceTests.cs`
* **TODO (`GetRecipesAsync` - Simple Success):** Test query finds enough acceptable recipes via mock `IRecipeSearcher`/`IRecipeRepository` -> verify no ranking/scraping/alt-query needed, verify correct recipes returned.
* **TODO (`GetRecipesAsync` - Ranking Needed):** Test query finds recipes, but scores too low -> verify `RankUnrankedRecipesAsync` (or direct LLM call) triggered via mock `ILlmService`, verify recipes with updated scores returned.
* **TODO (`GetRecipesAsync` - Scraping Needed & Enabled):** Test query finds insufficient recipes, `scrape=true` -> verify `IWebScraperService.ScrapeRecipeAsync` called, verify scraped recipes processed/ranked/returned.
* **TODO (`GetRecipesAsync` - Scraping Needed & Disabled):** Test query finds insufficient recipes, `scrape=false` -> verify `IWebScraperService` *not* called, verify alternative query logic triggered (or returns insufficient results).
* **TODO (`GetRecipesAsync` - Alt Query Needed):** Test initial search fails -> verify `GetSearchQueryForRecipe` called (mock `ILlmService`), verify subsequent search/repo calls with new query via mocks.
* **TODO (`GetRecipesAsync` - Max Attempts):** Test scenario simulating repeated failures to find recipes -> verify loop terminates, verify `NoRecipeException` (or similar) thrown.
* **TODO (`GetRecipesAsync` - Dependency Error):** Test handling of exceptions thrown by mocked dependencies (`IRecipeRepository`, `IRecipeSearcher`, `IWebScraperService`, `ILlmService`).
* **TODO (`SynthesizeRecipeAsync` - Success First Pass):** Mock `SynthesizeRecipePrompt` returns recipe, mock `AnalyzeRecipePrompt` returns high score -> verify single loop, verify final recipe returned.
* **TODO (`SynthesizeRecipeAsync` - Revision Loop):** Mock `SynthesizeRecipePrompt` returns recipe, mock `AnalyzeRecipePrompt` returns low score, mock subsequent `SynthesizeRecipePrompt` (with feedback) returns better recipe, mock final `AnalyzeRecipePrompt` returns high score -> verify feedback loop, verify final recipe returned.
* **TODO (`SynthesizeRecipeAsync` - Max Revisions):** Simulate analysis consistently returning low scores -> verify loop terminates after max attempts, verify appropriate error/result.
* **TODO (`SynthesizeRecipeAsync` - LLM Error):** Test handling of exceptions thrown by mocked `ILlmService` during synthesis or analysis.
* **TODO (`RankUnrankedRecipesAsync`):** Mock repo returns unranked recipes -> verify `ILlmService` called for ranking, verify repo `Update` called with ranked recipes.
* **TODO (`GetSearchQueryForRecipe`):** Mock `ISessionManager` context -> verify `ILlmService` called with correct prompt, verify handling of `LlmResult`.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `RecipeService` unit tests. (Gemini)

