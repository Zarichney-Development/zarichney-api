# Module/Directory: /Unit/Cookbook/Recipes/RecipeSearcher

**Last Updated:** 2025-04-18

> **Parent:** [`Recipes`](../README.md)
> **Related:**
> * **Source:** [`Cookbook/Recipes/RecipeSearcher.cs`](../../../../../api-server/Cookbook/Recipes/RecipeSearcher.cs)
> * **Dependencies:** Search Client Library Interface (e.g., `ISearchClient`, `IElasticClient`, `SearchClient`), `ILogger<RecipeSearcher>`
> * **Models:** [`Cookbook/Recipes/RecipeModels.cs`](../../../../../api-server/Cookbook/Recipes/RecipeModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `RecipeSearcher` class. This service is responsible for querying an external search engine (like Azure Cognitive Search, Elasticsearch, etc.) via its client library to find recipes based on user queries and potentially filters or other search options.

* **Why Unit Tests?** To validate the logic for constructing search queries, interacting with the (mocked) search client library interface, parsing search results, and handling errors, all in isolation from the actual search engine.
* **Isolation:** Achieved by mocking the search client library interface (e.g., `SearchClient` from Azure.Search.Documents) and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `RecipeSearcher` focus on its public methods, such as:

* **`SearchRecipesAsync(string query, SearchOptions options)` (or similar):**
    * Correctly translating the input `query` string and `options` (like paging, filtering, sorting) into the format expected by the search client library (e.g., setting properties on `Azure.Search.Documents.SearchOptions`).
    * Correctly invoking the search method on the mocked search client (e.g., `SearchClient.SearchAsync<RecipeIndexModel>`) with the constructed query and options.
    * Parsing and mapping the search results returned by the mocked client (e.g., iterating through `SearchResults<RecipeIndexModel>.GetResults()`) back into application-level `Recipe` models or search result DTOs.
    * Handling scenarios where the search returns multiple results, zero results, or errors.
    * Handling specific errors or exceptions returned by the mocked search client (e.g., related to query syntax errors, service unavailable).

## 3. Test Environment Setup

* **Instantiation:** `RecipeSearcher` is instantiated directly.
* **Mocking:** Dependencies are mocked using Moq. Key mocks include:
    * `Mock<ISearchClient>`: (Or the specific interface/class for the search library being used). Setup the core `SearchAsync` method to return mock `SearchResults<T>` objects containing mock `SearchResult<T>` items based on the input query/options. Configure it to simulate scenarios with results, no results, or to throw exceptions.
    * `Mock<ILogger<RecipeSearcher>>`.
* **Test Data:** Prepare sample search document objects (e.g., `RecipeIndexModel`) that the mocked search client will return, and the expected corresponding `Recipe` objects after mapping.

## 4. Maintenance Notes & Troubleshooting

* **Search Client Mocking:** Mocking the search client library, especially the structure of `SearchResults<T>` and `SearchResult<T>`, requires understanding the specific library's API. Ensure mocks accurately simulate the data structure returned by the actual search engine for different queries.
* **Query Construction:** Tests should verify that search terms, filters, paging options (`Skip`, `Size`), and sorting options are correctly applied to the search client's options object.
* **Mapping Logic:** Verify the mapping logic from the search index document model (e.g., `RecipeIndexModel`) back to the application's `Recipe` model is correct.
* **Library Updates:** Updates to the search client library might introduce breaking changes requiring test updates.

## 5. Test Cases & TODOs

### `RecipeSearcherTests.cs`
* **TODO (`SearchRecipesAsync` - Basic Search):** Test with simple query text -> verify `SearchClient.SearchAsync` called with correct text and default options.
* **TODO (`SearchRecipesAsync` - With Options):** Test with query text and specific options (paging, filters, sorting) -> verify `SearchClient.SearchAsync` called with correctly configured `SearchOptions`.
* **TODO (`SearchRecipesAsync` - Mapping):** Test successful search -> mock client returns `SearchResults` with data -> verify results correctly mapped to `Recipe` models or DTOs.
* **TODO (`SearchRecipesAsync` - No Results):** Test successful search -> mock client returns `SearchResults` with zero results -> verify empty list or appropriate response returned.
* **TODO (`SearchRecipesAsync` - Client Error):** Test when mocked `SearchClient.SearchAsync` throws an exception (e.g., `RequestFailedException`) -> verify exception handled/logged appropriately.
* **TODO (`SearchRecipesAsync` - Invalid Query):** Test scenario where query might be invalid (if pre-validation exists in `RecipeSearcher`).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `RecipeSearcher` unit tests. (Gemini)

