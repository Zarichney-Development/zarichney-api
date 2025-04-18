# Module/Directory: /Unit/Cookbook/Orders/OrderRepository

**Last Updated:** 2025-04-18

> **Parent:** [`Orders`](../README.md)
> **Related:**
> * **Source:** [`Cookbook/Orders/OrderRepository.cs`](../../../../../api-server/Cookbook/Orders/OrderRepository.cs)
> * **Interface:** [`Cookbook/Orders/IOrderRepository.cs`](../../../../../api-server/Cookbook/Orders/OrderRepository.cs) (Implicit)
> * **Dependencies:** `DbContext` (e.g., `AppDbContext`), `ILogger<OrderRepository>`
> * **Models:** [`Cookbook/Orders/OrderModels.cs`](../../../../../api-server/Cookbook/Orders/OrderModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `OrderRepository` class. This repository is responsible for abstracting the data access logic for `Order` entities and potentially related entities (like selected recipes), interacting directly with the underlying data store (likely an Entity Framework Core `DbContext`).

* **Why Unit Tests?** To validate the data access logic (querying, adding, updating orders) in isolation from the actual database. Tests ensure the repository correctly constructs queries (including handling related data), interacts with the (mocked) `DbContext` and `DbSet`s, maps data, and performs CRUD operations as expected.
* **Isolation:** Achieved by mocking the `DbContext` (e.g., `AppDbContext`) including its relevant `DbSet<T>` properties (`DbSet<Order>`, `DbSet<RecipeSelection>`, etc.), and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `OrderRepository` focus on its public methods implementing `IOrderRepository`:

* **Query Methods (`GetOrderByIdAsync`, `GetOrdersByUserIdAsync`):**
  * Verifying that the correct LINQ queries (e.g., `FindAsync`, `FirstOrDefaultAsync`, `Where`, `Include`, `ToListAsync`) are executed against the mocked `DbSet<Order>`.
  * Ensuring correct filtering criteria (e.g., matching user ID, order ID) are applied.
  * Testing the inclusion of related data (e.g., using `.Include()` for recipes associated with the order) via mock setup.
  * Handling scenarios where queries return data or no data (empty list/null).
* **Command Methods (`AddOrderAsync`, `UpdateOrderAsync`):**
  * Verifying that `DbSet<Order>.AddAsync` or entity state modification (`_context.Entry(order).State = EntityState.Modified`) is performed correctly based on input.
  * Ensuring `_context.SaveChangesAsync()` is called to persist changes.
* **Error Handling:** Testing how potential `DbUpdateException` or other database-related exceptions (simulated via mocks) are handled or propagated.

## 3. Test Environment Setup

* **Instantiation:** `OrderRepository` is instantiated directly.
* **Mocking:** Dependencies are mocked using Moq. Key mocks include:
  * `Mock<YourDbContext>`: Requires setting up relevant `DbSet<T>` properties (e.g., `Orders`, `RecipeSelections`) to return mock `DbSet`s. Mock `SaveChangesAsync()`.
  * `Mock<DbSet<Order>>` (and other relevant `DbSet`s): Needs to be configured to simulate EF Core query operations (`FindAsync`, `FirstOrDefaultAsync`, `ToListAsync`, LINQ `Where`, `Include`) against an in-memory list of test `Order` data (potentially with related entities). Also needs to simulate `AddAsync` and state tracking for updates. Libraries like `MockQueryable` are highly recommended.
  * `Mock<ILogger<OrderRepository>>`.
* **Test Data:** Prepare lists of `Order` entity objects (and related entities if testing `.Include()`) to back the mock `DbSet`s.

## 4. Maintenance Notes & Troubleshooting

* **DbContext/DbSet Mocking:** This is complex, especially when testing `.Include()`. Ensure the mock `DbSet` setup accurately simulates the expected query behavior and data relationships. `MockQueryable` helps greatly.
* **Query Logic:** Verify that LINQ queries correctly filter, include related data, and order results based on the mock `DbSet` setup.
* **Async Operations:** Test asynchronous methods (`async`/`await`) correctly.

## 5. Test Cases & TODOs

### `OrderRepositoryTests.cs`
* **TODO (`GetOrderByIdAsync`):** Test found -> mock `DbSet.FindAsync` (or query) returns order -> verify order returned. Test includes related data if applicable.
* **TODO (`GetOrderByIdAsync`):** Test not found -> mock returns null -> verify null returned.
* **TODO (`GetOrdersByUserIdAsync`):** Test found -> setup mock `DbSet` queryable, mock `Where(...).ToListAsync` returns orders -> verify list returned. Test includes related data if applicable.
* **TODO (`GetOrdersByUserIdAsync`):** Test none found -> mock `ToListAsync` returns empty list -> verify empty list returned.
* **TODO (`AddOrderAsync`):** Test success -> verify `DbSet.AddAsync` called, verify `_context.SaveChangesAsync` called, verify returned order has ID (if set by mock).
* **TODO (`UpdateOrderAsync`):** Test success -> verify entity state set to `Modified` (or `DbSet.Update` called), verify `_context.SaveChangesAsync` called.
* **TODO:** Test handling of `DbUpdateException` when mocking `SaveChangesAsync` to throw.
* **TODO:** Test queries that specifically use `.Include()` to ensure related data is correctly simulated in the mock setup and returned.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `OrderRepository` unit tests. (Gemini)

