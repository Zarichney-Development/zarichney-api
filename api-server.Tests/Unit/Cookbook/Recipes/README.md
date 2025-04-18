# Module/Directory: /Unit/Cookbook/Recipes

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Unit/Cookbook/` may be needed)*
> **Related:**
> * **Source:** [`Cookbook/Recipes/`](../../../../api-server/Cookbook/Recipes/)
> * **Subdirectories:** [`RecipeIndexer/`](RecipeIndexer/README.md), [`RecipeRepository/`](RecipeRepository/README.md), [`RecipeSearcher/`](RecipeSearcher/README.md), [`RecipeService/`](RecipeService/README.md), [`WebScraperService/`](WebScraperService/README.md)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the domain logic related to **Recipes** within the Cookbook feature. This is a core part of the application, involving recipe storage, retrieval, web scraping, AI-driven processing (ranking, analysis, synthesis - often orchestrated by `RecipeService`), and search indexing/querying.

* **Why Unit Tests?** To validate the specific responsibilities of each component (`RecipeService`, `RecipeRepository`, `WebScraperService`, `RecipeIndexer`, `RecipeSearcher`) in isolation from each other, the database, external web pages, AI models, and the search index. This ensures the complex logic within each layer is correct independently.
* **Isolation:** Achieved by mocking dependencies for each component under test. For example, `RecipeService` tests mock `IRecipeRepository`, `IWebScraperService`, `ILlmService`, etc. `RecipeRepository` mocks the `DbContext`. `WebScraperService` mocks the `IBrowserService`. `RecipeIndexer`/`RecipeSearcher` mock the search client library interface.

## 2. Scope & Key Functionality Tested (What?)

* **`RecipeServiceTests`:** Focuses on testing the orchestration logic within `RecipeService` – coordinating calls to repositories, scrapers, AI services (prompts), and searchers to fulfill requests like getting or synthesizing recipes. See `RecipeService/README.md`.
* **`RecipeRepositoryTests`:** Focuses on testing the data access logic for `Recipe` entities, ensuring correct interaction with the mocked `DbContext`. See `RecipeRepository/README.md`.
* **`WebScraperServiceTests`:** Focuses on testing the logic for extracting recipe data from web page HTML, ensuring correct interaction with the mocked `IBrowserService` and HTML parsing logic. See `WebScraperService/README.md`.
* **`RecipeIndexerTests`:** Focuses on testing the logic for adding/updating recipe documents in the search index, ensuring correct interaction with the mocked search client library. See `RecipeIndexer/README.md`.
* **`RecipeSearcherTests`:** Focuses on testing the logic for querying the search index for recipes, ensuring correct query construction and interaction with the mocked search client library. See `RecipeSearcher/README.md`.

## 3. Test Environment Setup

* **Instantiation:** Services, repositories, and other components under test are instantiated directly.
* **Mocking:** Dependencies are mocked using frameworks like Moq. This includes mocking repository interfaces, other service interfaces (`IBrowserService`, `ILlmService`), `DbContext`, search clients, and loggers.

## 4. Maintenance Notes & Troubleshooting

* **Structure:** Tests are organized into subdirectories mirroring the source structure (`RecipeService/`, `RecipeRepository/`, etc.).
* **Dependency Mocking:** Correctly mocking the various dependencies (database, browser automation, AI service, search client) is crucial and can be complex. Ensure mocks accurately simulate success and failure scenarios for each dependency.
* **Focus:** Maintain clear separation of concerns in tests. Service tests verify orchestration, repository tests verify data access, scraper tests verify HTML parsing, indexer/searcher tests verify search client interaction.

## 5. Test Cases & TODOs

Detailed TODOs reside within the READMEs and test files of the subdirectories:

* See `RecipeService/README.md` for `RecipeService` test plans.
* See `RecipeRepository/README.md` for `RecipeRepository` test plans.
* See `WebScraperService/README.md` for `WebScraperService` test plans.
* See `RecipeIndexer/README.md` for `RecipeIndexer` test plans.
* See `RecipeSearcher/README.md` for `RecipeSearcher` test plans.

## 6. Changelog

* **2025-04-18:** Initial creation - Overview for Recipe domain unit tests. (Gemini)

