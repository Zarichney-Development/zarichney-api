# Module/Directory: /Integration/Cookbook/Customers

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Integration/Cookbook/` may be needed)*
> **Related:**
> * **Source:** [`Cookbook/Customers/`](../../../../api-server/Cookbook/Customers/)
> * **Controller Tests:** [`Integration/Controllers/Cookbook/Customer/README.md`](../../Controllers/Cookbook/Customer/README.md)
> * **Test Infrastructure:** [`DatabaseIntegrationTestBase.cs`](../../../Integration/DatabaseIntegrationTestBase.cs), [`DatabaseFixture.cs`](../../../Framework/Fixtures/DatabaseFixture.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory is intended for **integration tests** that specifically target the **Customer domain logic**, potentially involving interactions between the `CustomerService`, `CustomerRepository`, and a **real test database instance**.

* **Why Integration Tests Here?** While controller integration tests (`Integration/Controllers/Cookbook/Customer/`) cover the end-to-end flow using mocked services, tests in this directory could be used for:
    * Verifying complex queries or data manipulation logic within `CustomerRepository` against an actual database schema (provided by `DatabaseFixture`).
    * Testing scenarios where the interaction between `CustomerService` and `CustomerRepository` with a real database is critical and difficult to fully simulate with mocks.
* **Scope:** Tests here typically bypass the HTTP layer and interact directly with resolved instances of `ICustomerService` or `ICustomerRepository`, using a test database connection.

## 2. Scope & Key Functionality Tested (What?)

Potential tests could include:

* **Repository Layer:** Testing specific complex LINQ queries in `CustomerRepository` against the test database to ensure correct SQL generation and results.
* **Service Layer:** Testing `CustomerService` methods in scenarios where verifying the actual database transaction or state change resulting from repository calls is important.
* **Data Seeding/Validation:** Tests that might seed specific customer data into the test database and then verify service/repository logic against that seeded data.

## 3. Test Environment Setup

* **Test Server/Services:** Uses `CustomWebApplicationFactory` to resolve scoped instances of `ICustomerService`, `ICustomerRepository`, and `DbContext`.
* **Test Database:** Relies on `DatabaseFixture` to provide a connection to a clean, containerized test database instance (e.g., PostgreSQL). Tests typically inherit from `DatabaseIntegrationTestBase`.
* **Data Management:** Requires careful management of test data within the database to ensure test isolation (e.g., using unique IDs, transactions, or cleanup scripts).

## 4. Maintenance Notes & Troubleshooting

* **Necessity:** Evaluate carefully if tests are needed here versus relying on controller integration tests and service/repository unit tests. Avoid redundant testing.
* **Database Dependency:** These tests depend on the `DatabaseFixture` setup and a running Docker environment (if using Testcontainers). Failures can be related to database connectivity, schema migrations, or test data conflicts.
* **Test Isolation:** Ensure tests clean up any data they create in the test database to avoid interfering with other tests.

## 5. Test Cases & TODOs

* **TODO:** Evaluate need for specific integration tests here beyond controller tests.
* **TODO (If needed):** Add tests for complex `CustomerRepository` queries against the test database.
* **TODO (If needed):** Add tests for `CustomerService` scenarios requiring verification of database state changes.
* **TODO (If none planned):** State "No specific integration tests planned for Customer domain logic beyond those covered in controller integration tests."

## 6. Changelog

* **2025-04-18:** Initial creation - Overview for Customer domain integration tests. (Gemini)

