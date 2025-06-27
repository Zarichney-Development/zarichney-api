# Module/Directory: /Unit/Cookbook/Recipes/RecipeIndexer

**Last Updated:** 2025-04-18

> **Parent:** [`Recipes`](../README.md)
> **Related:**
> * **Source:** [`Cookbook/Recipes/RecipeIndexer.cs`](../../../../../Zarichney.Server/Cookbook/Recipes/RecipeIndexer.cs)
> * **Dependencies:** Search Client Library Interface (e.g., `ISearchClient`, `IElasticClient`, `SearchClient`), `ILogger<RecipeIndexer>`
> * **Models:** [`Cookbook/Recipes/RecipeModels.cs`](../../../../../Zarichney.Server/Cookbook/Recipes/RecipeModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `RecipeIndexer` class. This service is responsible for taking `Recipe` objects and indexing them (adding or updating) into an external search engine (like Azure Cognitive Search, Elasticsearch, etc.) via its client library.

* **Why Unit Tests?** To validate the logic for mapping `Recipe` objects to the document structure expected by the search index, interacting correctly with the (mocked) search client library interface, and handling potential indexing errors, all in isolation from the actual search engine.
* **Isolation:** Achieved by mocking the search client library interface (e.g., `SearchClient` from Azure.Search.Documents) and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `RecipeIndexer` focus on its public methods, such as:

* **`IndexRecipeAsync(Recipe recipe)` / `IndexRecipesAsync(IEnumerable<Recipe> recipes)`:**
    * Correctly mapping properties from the input `Recipe` object(s) to the fields of the search index document model.
    * Handling potential data transformations or cleaning required before indexing.
    * Correctly invoking the appropriate method on the mocked search client library (e.g., `SearchClient.MergeOrUploadDocumentsAsync`, `IndexDocumentsAsync`) with the mapped document(s).
    * Handling successful responses from the mocked search client.
    * Handling specific errors or exceptions returned by the mocked search client (e.g., related to indexing failures, invalid documents, service unavailable).
* **`DeleteRecipeAsync(string recipeId)`:**
    * Correctly invoking the deletion method on the mocked search client (e.g., `SearchClient.DeleteDocumentsAsync`) with the appropriate document ID.
    * Handling success and failure responses from the mocked search client.

## 3. Test Environment Setup

* **Instantiation:** `RecipeIndexer` is instantiated directly.
* **Mocking:** Dependencies are mocked using Moq. Key mocks include:
    * `Mock<ISearchClient>`: (Or the specific interface/class for the search library being used). Setup methods like `MergeOrUploadDocumentsAsync`, `IndexDocumentsAsync`, `DeleteDocumentsAsync` to simulate success or throw specific exceptions/return error results provided by the search library.
    * `Mock<ILogger<RecipeIndexer>>`.
* **Test Data:** Prepare sample `Recipe` objects to be indexed.

## 4. Maintenance Notes & Troubleshooting

* **Search Client Mocking:** Mocking the search client library requires understanding its API for indexing and deletion, including response objects and potential exceptions. Ensure mocks cover both success and various failure modes.
* **Mapping Logic:** Tests should verify that the mapping from the `Recipe` model to the search document model is accurate, especially if field names or data types differ.
* **Library Updates:** Updates to the search client library might introduce breaking changes requiring test updates.

## 5. Test Cases & TODOs

### `RecipeIndexerTests.cs`
* **TODO (`IndexRecipeAsync`/`IndexRecipesAsync` - Success):** Test indexing a single valid recipe -> verify mapping logic, verify `SearchClient.MergeOrUploadDocumentsAsync` (or similar) called with correct document, mock success response.
* **TODO (`IndexRecipeAsync`/`IndexRecipesAsync` - Batch):** Test indexing multiple valid recipes -> verify mapping, verify `SearchClient.MergeOrUploadDocumentsAsync` called with correct batch of documents, mock success response.
* **TODO (`IndexRecipeAsync`/`IndexRecipesAsync` - Client Error):** Test when mocked search client method throws an exception (e.g., `RequestFailedException`) -> verify exception handled/logged.
* **TODO (`IndexRecipeAsync`/`IndexRecipesAsync` - Partial Failure):** Test when mocked search client response indicates partial failure (if applicable to the library) -> verify handling/logging.
* **TODO (`DeleteRecipeAsync` - Success):** Test deleting an existing recipe ID -> verify `SearchClient.DeleteDocumentsAsync` called with correct ID, mock success response.
* **TODO (`DeleteRecipeAsync` - Client Error):** Test when mocked `SearchClient.DeleteDocumentsAsync` throws an exception -> verify exception handled/logged.
* **TODO (`DeleteRecipeAsync` - Not Found):** Test deleting a non-existent ID -> verify behavior based on search client (e.g., success response, specific error).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `RecipeIndexer` unit tests. (Gemini)

