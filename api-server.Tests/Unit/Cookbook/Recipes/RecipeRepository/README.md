# Module/Directory: /Unit/Cookbook/Recipes/RecipeRepository

**Last Updated:** 2025-04-18

> **Parent:** [`Recipes`](../README.md)
> **Related:**
> * **Source:** [`Cookbook/Recipes/RecipeRepository.cs`](../../../../../api-server/Cookbook/Recipes/RecipeRepository.cs)
> * **Interface:** [`Cookbook/Recipes/IRecipeRepository.cs`](../../../../../api-server/Cookbook/Recipes/RecipeRepository.cs) (Implicit)
> * **Dependencies:** `DbContext` (e.g., `AppDbContext`), `ILogger<RecipeRepository>`
> * **Models:** [`Cookbook/Recipes/RecipeModels.cs`](../../../../../api-server/Cookbook/Recipes/RecipeModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `RecipeRepository` class. This repository is responsible for abstracting the data access logic for `Recipe` entities and potentially related data (like ingredients), interacting directly with the underlying data store (likely an Entity Framework Core `DbContext`).

* **Why Unit Tests?** To validate the data access logic (querying, adding, updating, deleting recipes) in isolation from the actual database. Tests ensure the repository correctly constructs queries (including handling related data via `Include`), interacts with the (mocked) `DbContext` and `DbSet`, maps data, and handles potential data access exceptions.
* **Isolation:** Achieved by mocking the `DbContext` (e.g., `AppDbContext`) including its `DbSet<Recipe>` property (and any other relevant `DbSet`s), and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `RecipeRepository` focus on its public methods implementing `IRecipeRepository`:

* **Query Methods (`GetRecipeByIdAsync`, `GetRecipesByIdsAsync`, `FindRecipesByCriteriaAsync`, etc.):**
    * Verifying that the correct LINQ queries (e.g., `FindAsync`, `FirstOrDefaultAsync`, `Where`, `Include`, `ToListAsync`) are executed against the mocked `DbSet<Recipe>`.
    * Ensuring correct filtering criteria (e.g., matching IDs, keywords) are applied.
    * Testing inclusion of related data (e.g., `Include(r => r.Ingredients)`) if applicable.
    * Handling scenarios where queries return data or no data (empty list/null).
* **Command Methods (`AddRecipeAsync`, `UpdateRecipeAsync`, `DeleteRecipeAsync`):**
    * Verifying that `DbSet<Recipe>.AddAsync` or entity state modification (`_context.Entry(recipe).State = EntityState.Modified` / `_context.Update(recipe)`) is performed correctly based on input.
    * Verifying that `DbSet<Recipe>.Remove` is called for deletions.
    * Ensuring `_context.SaveChangesAsync()` is called to persist changes for add, update, and delete operations.
* **Error Handling:** Testing how potential `DbUpdateException` or other database-related exceptions (simulated via mocks) are handled or propagated.

## 3. Test Environment Setup

* **Instantiation:** `RecipeRepository` is instantiated directly.
* **Mocking:** Dependencies are mocked using Moq. Key mocks include:
    * `Mock<YourDbContext>`: Requires setting up the relevant `DbSet<Recipe>` property (and others like `DbSet<Ingredient>` if needed) to return a mock `DbSet`. Mock `SaveChangesAsync()` to simulate success or throw exceptions.
    * `Mock<DbSet<Recipe>>`: Needs to be configured to simulate EF Core query operations (`FindAsync`, `FirstOrDefaultAsync`, `ToListAsync`, LINQ `Where`, `Include`, etc.) against an in-memory list of test `Recipe` data (potentially including related entities). Also needs to simulate `AddAsync`, `Remove`, and state tracking/`Update` for command methods. Libraries like `MockQueryable` are highly recommended for simplifying query mocking.
    * `Mock<ILogger<RecipeRepository>>`.
* **Test Data:** Prepare lists of `Recipe` entity objects (and related entities if testing `Include`) to back the mock `DbSet`.

## 4. Maintenance Notes & Troubleshooting

* **DbContext/DbSet Mocking:** Accurately simulating EF Core query behavior (especially `Include` and async LINQ methods) and change tracking on a mock `DbSet` requires careful setup. Use helper libraries (`MockQueryable`) or established patterns. Verify that `Include` calls in the repository are matched by appropriate setup in the mock `DbSet` if testing loaded related data.
* **Query Logic:** Ensure tests verify that the LINQ queries constructed within the repository methods correctly filter and retrieve the intended data based on the mock `DbSet` setup.
* **Async Operations:** Remember to test the asynchronous nature (`async`/`await`) correctly.

## 5. Test Cases & TODOs

### `RecipeRepositoryTests.cs`
* **TODO (`GetRecipeByIdAsync`):** Test found -> mock `DbSet.FindAsync` returns recipe -> verify recipe returned.
* **TODO (`GetRecipeByIdAsync` - Not Found):** Test not found -> mock `DbSet.FindAsync` returns null -> verify null returned.
* **TODO (`GetRecipeByIdAsync` - With Includes):** Test found with related data -> setup mock `Include` -> verify related data loaded.
* **TODO (`GetRecipesByIdsAsync`):** Test found all -> mock `Where(...).ToListAsync()` returns matching list -> verify correct list returned.
* **TODO (`GetRecipesByIdsAsync`):** Test found partial -> mock query returns partial list -> verify partial list returned.
* **TODO (`GetRecipesByIdsAsync`):** Test found none -> mock query returns empty list -> verify empty list returned.
* **TODO (`AddRecipeAsync`):** Test success -> verify `DbSet.AddAsync` called with correct recipe, verify `_context.SaveChangesAsync` called.
* **TODO (`UpdateRecipeAsync`):** Test success -> verify entity state set to `Modified` (or `DbSet.Update` called), verify `_context.SaveChangesAsync` called.
* **TODO (`DeleteRecipeAsync`):** Test success -> mock `FindAsync` returns recipe, verify `DbSet.Remove` called with recipe, verify `_context.SaveChangesAsync` called.
* **TODO (`DeleteRecipeAsync` - Not Found):** Test deleting non-existent ID -> mock `FindAsync` returns null -> verify `DbSet.Remove` not called, verify `SaveChangesAsync` not called (or handles gracefully).
* **TODO:** Test handling of `DbUpdateException` when mocking `SaveChangesAsync` to throw.
* **TODO:** *(Add tests for any other specific query or command methods)*

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `RecipeRepository` unit tests. (Gemini)

