# Module/Directory: /Integration/Controllers/CookbookControllers/Recipe

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Integration/Controllers/CookbookControllers/` may be needed)*
> **Related:**
> * **Source:** [`CookbookController.cs`](../../../../../Zarichney.Server/Controllers/CookbookController.cs)
> * **Service:** [`Cookbook/Recipes/RecipeService.cs`](../../../../../Zarichney.Server/Cookbook/Recipes/RecipeService.cs)
> * **Models:** [`Cookbook/Recipes/RecipeModels.cs`](../../../../../Zarichney.Server/Cookbook/Recipes/RecipeModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Standards/DocumentationStandards.md)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../../../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs), [`AuthTestHelper.cs`](../../../Framework/Helpers/AuthTestHelper.cs)

## 1. Purpose & Rationale (Why?)

This directory contains integration tests specifically for the **Recipe-related endpoints** exposed by the `CookbookController`. These tests validate the behavior of actions related to searching, retrieving, and potentially initiating AI-driven processing (synthesis, analysis) of recipes.

* **Why Integration Tests?** To ensure these endpoints function correctly within the ASP.NET Core pipeline. This includes routing, authentication (if applicable), model binding, query parameter handling, interaction with middleware, and verifying the controller's interaction with the underlying (mocked) `IRecipeService`.
* **Why Mock Dependencies?** The `IRecipeService`, which encapsulates complex logic including external AI calls, searching, and potentially scraping, is mocked in the `CustomWebApplicationFactory`. This isolates the controller's endpoint logic, allowing focused testing of request handling, parameter passing, and response generation based on controlled service outcomes.

## 2. Scope & Key Functionality Tested (What?)

These tests cover endpoints related to recipe management and interaction:

* **`GET /api/cookbook/recipes/search`:** Searching for recipes based on query parameters. Verifies parameter binding and response structure for search results.
* **`GET /api/cookbook/recipes/{recipeId}`:** Retrieving details of a specific recipe by its ID.
* **`POST /api/cookbook/recipes/synthesize`:** (If applicable) Initiating AI recipe synthesis based on request criteria. Verifies request validation and response (e.g., task ID, immediate result).
* **`POST /api/cookbook/recipes/analyze`:** (If applicable) Initiating AI analysis of a recipe. Verifies request validation and response.
* **Authorization:** Ensuring endpoints correctly enforce authentication if required for specific actions (e.g., accessing private recipes, triggering synthesis).
* **Error Handling:** Verifying correct responses for scenarios like "Recipe Not Found", invalid search parameters, or failures during synthesis/analysis requests.

## 3. Test Environment Setup

* **Test Server:** Provided by `CustomWebApplicationFactory<Program>`.
* **Authentication:** Simulated using `TestAuthHandler` and `AuthTestHelper` if any recipe endpoints require authentication.
* **Mocked Dependencies:** Configured in `CustomWebApplicationFactory`. The key mock for these tests is:
    * `Zarichnyi.ApiServer.Cookbook.Recipes.IRecipeService`
    * Potentially `ILlmService` if the controller interacts with it directly (less likely, usually delegated to `RecipeService`).

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Tests related to recipe endpoints should reside within this directory (e.g., `SearchRecipesTests.cs`, `GetRecipeDetailsTests.cs`, `SynthesizeRecipeTests.cs`).
* **`IRecipeService` Mocking:** Ensure the mock is configured in the factory to return appropriate `Recipe` objects, lists of search results, nulls (for not found), or throw exceptions to simulate various scenarios based on input parameters (query, recipe ID, synthesis criteria).
* **Query Parameters:** Pay attention to constructing request URLs with query parameters correctly for search tests.
* **Complex Responses:** Recipe details and search results can be complex objects; use appropriate assertions (e.g., `FluentAssertions`) to validate the response structure and data based on the mock setup.

## 5. Test Cases & TODOs

### Search Recipes (`SearchRecipesTests.cs`)
* **TODO:** Test `GET /search` with valid query -> mock `IRecipeService.SearchRecipesAsync` returns results -> verify 200 OK with list of recipes/search results.
* **TODO:** Test `GET /search` with query yielding no results -> mock `IRecipeService.SearchRecipesAsync` returns empty list -> verify 200 OK with empty list.
* **TODO:** Test `GET /search` with missing/empty query parameter -> verify 400 Bad Request or default search behavior.
* **TODO:** Test `GET /search` with pagination parameters (if applicable) -> verify correct parameters passed to service mock, verify response structure.
* **TODO:** Test `GET /search` unauthenticated (if auth applies).

### Get Recipe Details (`GetRecipeDetailsTests.cs`)
* **TODO:** Test `GET /{recipeId}` with valid ID -> mock `IRecipeService.GetRecipeByIdAsync` returns recipe -> verify 200 OK with recipe details.
* **TODO:** Test `GET /{recipeId}` with non-existent ID -> mock `IRecipeService.GetRecipeByIdAsync` returns null -> verify 404 Not Found.
* **TODO:** Test `GET /{recipeId}` with invalid ID format -> verify 400 Bad Request.
* **TODO:** Test `GET /{recipeId}` unauthenticated (if auth applies).

### Synthesize Recipe (`SynthesizeRecipeTests.cs` - If endpoint exists)
* **TODO:** Test `POST /synthesize` with valid request body -> mock `IRecipeService.SynthesizeRecipeAsync` success -> verify 200 OK / 202 Accepted with appropriate response (e.g., task ID, initial result).
* **TODO:** Test `POST /synthesize` with invalid request body -> verify 400 Bad Request.
* **TODO:** Test `POST /synthesize` unauthenticated (if auth applies).
* **TODO:** Test `POST /synthesize` when `IRecipeService.SynthesizeRecipeAsync` fails -> verify appropriate error response.

### Analyze Recipe (`AnalyzeRecipeTests.cs` - If endpoint exists)
* **TODO:** Test `POST /analyze` with valid request body (e.g., recipe ID or content) -> mock `IRecipeService.AnalyzeRecipeAsync` success -> verify 200 OK / 202 Accepted.
* **TODO:** Test `POST /analyze` with invalid request body -> verify 400 Bad Request.
* **TODO:** Test `POST /analyze` unauthenticated (if auth applies).
* **TODO:** Test `POST /analyze` when `IRecipeService.AnalyzeRecipeAsync` fails -> verify appropriate error response.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for Recipe-related `CookbookController` integration tests. (Gemini)

