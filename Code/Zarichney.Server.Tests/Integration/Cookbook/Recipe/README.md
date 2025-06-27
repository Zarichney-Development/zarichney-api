# Module/Directory: /Integration/Cookbook/Recipes

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Integration/Cookbook/` may be needed)*
> **Related:**
> * **Source:** [`Cookbook/Recipes/`](../../../../Zarichney.Server/Cookbook/Recipes/)
> * **Controller Tests:** [`Integration/Controllers/Cookbook/Recipe/README.md`](../../Controllers/Cookbook/Recipe/README.md)
> * **Unit Tests:** [`Unit/Cookbook/Recipes/README.md`](../../../Unit/Cookbook/Recipes/README.md)
> * **Test Infrastructure:** [`DatabaseIntegrationTestBase.cs`](../../../Integration/DatabaseIntegrationTestBase.cs), [`DatabaseFixture.cs`](../../../Framework/Fixtures/DatabaseFixture.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory is intended for **integration tests** that specifically target the **Recipe domain logic**, potentially involving interactions between services (`RecipeService`), repositories (`RecipeRepository`), indexers/searchers (`RecipeIndexer`, `RecipeSearcher`), and real infrastructure like a **test database instance** or a **test search index instance**.

* **Why Integration Tests Here?** While controller integration tests (`Integration/Controllers/Cookbook/Recipe/`) cover the HTTP end-to-end flow (usually with mocked services), and unit tests verify components in isolation, tests in this directory could be used for:
    * Verifying complex database queries or data integrity logic within `RecipeRepository` against an actual database schema (via `DatabaseFixture`).
    * Testing the interaction between `RecipeService` and `RecipeRepository` using a real database connection.
    * Validating the end-to-end indexing (`RecipeIndexer`) and searching (`RecipeSearcher`) flow against a real (test instance) search engine, ensuring schema mapping and query logic work correctly.
    * Testing complex orchestration in `RecipeService` that might be difficult to fully simulate with mocks alone, potentially using a mix of real infrastructure (DB, search index) and mocked external services (AI, Web Scraping).
* **Scope:** Tests here typically bypass the HTTP layer and interact directly with resolved instances of services/repositories, using test infrastructure like `DatabaseFixture` or potentially Testcontainers for search indexes.

## 2. Scope & Key Functionality Tested (What?)

Potential tests could include:

* **Repository Layer:** Testing specific complex LINQ queries in `RecipeRepository` (e.g., filtering by multiple criteria, including related data like ingredients) against the test database.
* **Service Layer:** Testing `RecipeService` methods (like `GetRecipesAsync`) involving data retrieval from the test database, potentially followed by interaction with mocked AI services for ranking.
* **Search Indexing/Searching:** Testing `RecipeIndexer` successfully adds/updates documents in a test search index and `RecipeSearcher` successfully retrieves them using various queries.
* **Data Consistency:** Tests verifying consistency between data stored in the database and the search index after updates.

## 3. Test Environment Setup

* **Test Server/Services:** Uses `CustomWebApplicationFactory` to resolve scoped instances of services (`IRecipeService`, `IRecipeIndexer`, etc.), repositories (`IRecipeRepository`), and `DbContext`.
* **Test Database:** May rely on `DatabaseFixture` to provide a connection to a clean, containerized test database instance. Tests might inherit from `DatabaseIntegrationTestBase`.
* **Test Search Index:** (If applicable) May require setting up a dedicated test search index instance (e.g., local Elasticsearch/OpenSearch via Testcontainers, a dedicated Azure Cognitive Search instance). Configuration for this test index would be needed.
* **Mocked Boundaries:** External dependencies like AI (`ILlmService`) and Web Scraping (`IBrowserService`) would typically still be mocked even in these integration tests.
* **Data Management:** Requires careful management of test data within the database and/or search index to ensure test isolation.

## 4. Maintenance Notes & Troubleshooting

* **Necessity & Focus:** Carefully evaluate if integration tests are needed here beyond controller/unit tests. Focus on scenarios where real infrastructure interaction provides significant value (e.g., complex queries, indexing/searching validation). Avoid duplicating tests covered elsewhere.
* **Infrastructure Dependency:** These tests depend heavily on the setup of the test database (`DatabaseFixture`) and potentially a test search index. Failures can stem from connectivity, schema mismatches, container issues, or data conflicts.
* **Test Isolation & Cleanup:** Ensure robust cleanup of any data created in the test database or search index.
* **Test Speed:** These tests will likely be slower than unit or controller integration tests (that use more mocks).

## 5. Test Cases & TODOs

* **TODO:** Evaluate need for specific integration tests here beyond controller and unit tests.
* **TODO (Repository - If needed):** Test complex `RecipeRepository` queries against the test database (e.g., `FindRecipesByIngredients`).
* **TODO (Service - If needed):** Test `RecipeService.GetRecipesAsync` retrieving data from test DB and interacting with mocked `ILlmService` for ranking.
* **TODO (Search - If needed):** Test `RecipeIndexer.IndexRecipeAsync` successfully indexes data into a test search index instance.
* **TODO (Search - If needed):** Test `RecipeSearcher.SearchRecipesAsync` retrieves data correctly from the test search index based on various queries/filters.
* **TODO (If none planned):** State "No specific integration tests planned for Recipe domain logic beyond those covered in controller and unit tests."

## 6. Changelog

* **2025-04-18:** Initial creation - Overview for Recipe domain integration tests. (Gemini)

