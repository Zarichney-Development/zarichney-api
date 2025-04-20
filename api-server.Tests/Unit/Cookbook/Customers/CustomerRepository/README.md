# Module/Directory: /Unit/Cookbook/Customers/CustomerRepository

**Last Updated:** 2025-04-18

> **Parent:** [`Customers`](../README.md)
> **Related:**
> * **Source:** [`Cookbook/Customers/CustomerRepository.cs`](../../../../../api-server/Cookbook/Customers/CustomerRepository.cs)
> * **Interface:** [`Cookbook/Customers/ICustomerRepository.cs`](../../../../../api-server/Cookbook/Customers/CustomerRepository.cs) (Implicit)
> * **Dependencies:** `DbContext` (e.g., `AppDbContext` or `UserDbContext`), `IConfiguration`, `ILogger<CustomerRepository>`
> * **Models:** [`Cookbook/Customers/CustomerModels.cs`](../../../../../api-server/Cookbook/Customers/CustomerModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `CustomerRepository` class. This repository is responsible for abstracting the data access logic for `Customer` entities, interacting directly with the underlying data store (likely an Entity Framework Core `DbContext`).

* **Why Unit Tests?** To validate the data access logic (querying, adding, updating) in isolation from the actual database. Tests ensure the repository correctly constructs queries, interacts with the (mocked) `DbContext` and `DbSet`, maps data, and handles potential data access exceptions.
* **Isolation:** Achieved by mocking the `DbContext` (e.g., `AppDbContext` or `UserDbContext`) including its `DbSet<Customer>` property, `IConfiguration` (if used), and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `CustomerRepository` focus on its public methods implementing `ICustomerRepository`:

* **Query Methods (`GetCustomerByIdAsync`, `GetCustomerByUserIdAsync`, `GetAllCustomersAsync`):**
    * Verifying that the correct LINQ queries (e.g., `FindAsync`, `FirstOrDefaultAsync`, `Where`, `ToListAsync`) are executed against the mocked `DbSet<Customer>`.
    * Ensuring correct filtering criteria (e.g., matching user ID) are applied.
    * Testing data mapping from the entity model (if different) to the returned model.
    * Handling scenarios where queries return data or no data (empty list/null).
* **Command Methods (`AddCustomerAsync`, `UpdateCustomerAsync`):**
    * Verifying that `DbSet<Customer>.AddAsync` or entity state modification (`_context.Entry(customer).State = EntityState.Modified`) is performed correctly based on input.
    * Ensuring `_context.SaveChangesAsync()` is called to persist changes.
* **Error Handling:** Testing how potential `DbUpdateException` or other database-related exceptions (simulated via mocks) are handled or propagated.

## 3. Test Environment Setup

* **Instantiation:** `CustomerRepository` is instantiated directly.
* **Mocking:** Dependencies are mocked using Moq. Key mocks include:
    * `Mock<YourDbContext>`: Requires setting up the relevant `DbSet<Customer>` property to return a mock `DbSet`. Mock `SaveChangesAsync()` to simulate success or throw exceptions.
    * `Mock<DbSet<Customer>>`: Needs to be configured to simulate EF Core query operations (`FindAsync`, `FirstOrDefaultAsync`, `ToListAsync`, LINQ `Where`, etc.) against an in-memory list of test `Customer` data. Also needs to simulate `AddAsync` and state tracking for updates. Libraries like `MockQueryable` can help significantly here.
    * `Mock<IConfiguration>`: If the repository uses configuration directly.
    * `Mock<ILogger<CustomerRepository>>`.
* **Test Data:** Prepare lists of `Customer` entity objects to back the mock `DbSet`.

## 4. Maintenance Notes & Troubleshooting

* **DbContext/DbSet Mocking:** This is the most complex part. Accurately simulating EF Core query behavior (especially `FindAsync`, `FirstOrDefaultAsync`, and LINQ `Where` clauses) and change tracking (`AddAsync`, state modification, `SaveChangesAsync`) on a mock `DbSet` requires careful setup. Use helper libraries (`MockQueryable`) or established patterns.
* **Query Logic:** Ensure tests verify that the LINQ queries constructed within the repository methods correctly filter and retrieve the intended data based on the mock `DbSet` setup.
* **Async Operations:** Remember to test the asynchronous nature (`async`/`await`) correctly.

## 5. Test Cases & TODOs

### `CustomerRepositoryTests.cs`
* **TODO (`GetCustomerByIdAsync`):** Test found -> mock `DbSet.FindAsync` returns customer -> verify customer returned.
* **TODO (`GetCustomerByIdAsync`):** Test not found -> mock `DbSet.FindAsync` returns null -> verify null returned.
* **TODO (`GetCustomerByUserIdAsync`):** Test found -> setup mock `DbSet` queryable, mock `FirstOrDefaultAsync` returns customer -> verify customer returned.
* **TODO (`GetCustomerByUserIdAsync`):** Test not found -> setup mock `DbSet` queryable, mock `FirstOrDefaultAsync` returns null -> verify null returned.
* **TODO (`GetAllCustomersAsync`):** Test returns list -> setup mock `DbSet` queryable, mock `ToListAsync` returns list -> verify list returned.
* **TODO (`AddCustomerAsync`):** Test success -> verify `DbSet.AddAsync` called, verify `_context.SaveChangesAsync` called.
* **TODO (`UpdateCustomerAsync`):** Test success -> verify entity state set to `Modified` (or `DbSet.Update` called), verify `_context.SaveChangesAsync` called.
* **TODO:** Test handling of `DbUpdateException` when mocking `SaveChangesAsync` to throw.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `CustomerRepository` unit tests. (Gemini)

