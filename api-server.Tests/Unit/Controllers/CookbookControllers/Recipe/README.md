# Module/Directory: /Unit/Controllers/CookbookControllers/Recipe

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Unit/Controllers/CookbookControllers/` may be needed)*
> **Related:**
> * **Source:** [`CookbookController.cs`](../../../../../api-server/Controllers/CookbookController.cs)
> * **Service:** [`Cookbook/Recipes/IRecipeService.cs`](../../../../../api-server/Cookbook/Recipes/IRecipeService.cs)
> * **Models:** [`Cookbook/Recipes/RecipeModels.cs`](../../../../../api-server/Cookbook/Recipes/RecipeModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests specifically for the **internal logic of the Recipe-related action methods** within the `CookbookController`, isolated from the ASP.NET Core pipeline and the real `RecipeService` implementation.

* **Why Unit Tests?** To provide fast, focused feedback on the controller's logic for handling recipe requests. They ensure the controller correctly:
    * Extracts parameters (query strings, route parameters, request bodies).
    * Calls the appropriate methods on the mocked `IRecipeService` with the correct arguments.
    * Handles the results (e.g., `Recipe` objects, lists, nulls) returned by the mocked service.
    * Translates service results or exceptions into the correct `ActionResult` (e.g., `OkObjectResult`, `NotFoundResult`, `BadRequestObjectResult`).
* **Why Mock Dependencies?** To isolate the controller's logic. The primary dependency `IRecipeService` (and `ILogger`) is mocked. `HttpContext` might be mocked if user context is needed for specific actions.

## 2. Scope & Key Functionality Tested (What?)

These tests focus on the code *within* the Recipe-related actions of `CookbookController`:

* **Parameter Extraction:** Verifying correct binding/extraction of search queries, recipe IDs, and request body DTOs.
* **Service Invocation:** Ensuring the correct `IRecipeService` methods (`SearchRecipesAsync`, `GetRecipeByIdAsync`, `SynthesizeRecipeAsync`, `AnalyzeRecipeAsync`, etc.) are called with the expected parameters derived from the request.
* **Result Handling:** Testing how `Recipe` objects, lists, nulls, or exceptions from the mocked `IRecipeService` are processed and mapped to `ActionResult` types.
* **Input Mapping/Validation:** Testing the mapping of request DTOs (e.g., for synthesis/analysis) to service method parameters.

## 3. Test Environment Setup

* **Instantiation:** `CookbookController` is instantiated directly in tests.
* **Mocking:** Mocks for `Zarichnyi.ApiServer.Cookbook.Recipes.IRecipeService` and `Microsoft.Extensions.Logging.ILogger<CookbookController>` must be provided.
* **HttpContext Mocking:** Mock `HttpContext` and `User` claims if any actions require user context (less likely for public recipe search/view, more likely for synthesis/analysis if tied to user accounts).

## 4. Maintenance Notes & Troubleshooting

* **`IRecipeService` Mocking:** Ensure mocks accurately reflect the expected return values for different scenarios (results found, not found, empty list, service exceptions). Verify mock interactions (`Mock.Verify(...)`).
* **Parameter Handling:** Double-check that tests correctly simulate query parameters, route parameters, and request bodies as the controller actions expect them.
* **DTOs/Models:** Changes to recipe DTOs or the `Recipe` model might require updates to tests checking mapping or returned data.

## 5. Test Cases & TODOs

### `SearchRecipes` Action (GET /search)
* **TODO:** Test extraction of `query` parameter.
* **TODO:** Test `_recipeService.SearchRecipesAsync` called with correct query.
* **TODO:** Test handling successful service result (list) -> verify `OkObjectResult` with list.
* **TODO:** Test handling successful service result (empty list) -> verify `OkObjectResult` with empty list.
* **TODO:** Test handling exception from service -> verify exception propagation.

### `GetRecipeById` Action (GET /{recipeId})
* **TODO:** Test extraction of `recipeId` route parameter.
* **TODO:** Test `_recipeService.GetRecipeByIdAsync` called with correct ID.
* **TODO:** Test handling recipe found -> verify `OkObjectResult` with recipe data.
* **TODO:** Test handling recipe not found (service returns null) -> verify `NotFoundResult`.
* **TODO:** Test handling exception from service -> verify exception propagation.

### `SynthesizeRecipe` Action (POST /synthesize - If exists)
* **TODO:** Test mapping request body DTO to service parameters.
* **TODO:** Test `_recipeService.SynthesizeRecipeAsync` called with correct parameters.
* **TODO:** Test handling successful service result -> verify `OkObjectResult` or `AcceptedResult`.
* **TODO:** Test handling failure/exception from service -> verify appropriate error `ActionResult`.

### `AnalyzeRecipe` Action (POST /analyze - If exists)
* **TODO:** Test mapping request body DTO/parameters to service parameters.
* **TODO:** Test `_recipeService.AnalyzeRecipeAsync` called with correct parameters.
* **TODO:** Test handling successful service result -> verify `OkObjectResult` or `AcceptedResult`.
* **TODO:** Test handling failure/exception from service -> verify appropriate error `ActionResult`.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for Recipe-related `CookbookController` unit tests. (Gemini)

