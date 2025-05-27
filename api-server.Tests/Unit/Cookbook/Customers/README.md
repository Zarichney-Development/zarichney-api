# Module/Directory: /Unit/Cookbook/Customers

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Unit/Cookbook/` may be needed)*
> **Related:**
> * **Source:** [`Cookbook/Customers/`](../../../../api-server/Cookbook/Customers/)
> * **Subdirectories:** [`CustomerService/`](CustomerService/README.md), [`CustomerRepository/`](CustomerRepository/README.md)
> * **Standards:** [`TestingStandards.md`](../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the domain logic related to **Customers** within the Cookbook feature. It includes tests for the `CustomerService` (orchestration and business logic) and the `CustomerRepository` (data access logic).

* **Why Unit Tests?** To validate the specific responsibilities of the service and repository layers in isolation from each other, the database, and other parts of the application. This ensures the business logic (`CustomerService`) and data access logic (`CustomerRepository`) are correct independently.
* **Isolation:** `CustomerService` tests mock `ICustomerRepository`. `CustomerRepository` tests mock the underlying data store (e.g., `DbContext` or potentially file system access if applicable).

## 2. Scope & Key Functionality Tested (What?)

* **`CustomerServiceTests`:** Focuses on testing the orchestration logic within `CustomerService`, ensuring it correctly calls methods on the mocked `ICustomerRepository`, handles the results, and implements any customer-specific business rules. See `CustomerService/README.md` for details.
* **`CustomerRepositoryTests`:** Focuses on testing the data access logic within `CustomerRepository`, ensuring it correctly interacts with the mocked data store (e.g., constructing queries, mapping data, performing CRUD operations). See `CustomerRepository/README.md` for details.

## 3. Test Environment Setup

* **Instantiation:** Services and repositories under test are instantiated directly.
* **Mocking:** Dependencies are mocked using frameworks like Moq. `CustomerService` mocks `ICustomerRepository`. `CustomerRepository` mocks its specific data source interface (e.g., `DbContext`). Loggers are also mocked.

## 4. Maintenance Notes & Troubleshooting

* **Structure:** Tests are organized into subdirectories mirroring the source structure (`CustomerService/`, `CustomerRepository/`).
* **Dependency Mocking:** Correctly mocking the repository interface for service tests and the data source for repository tests is crucial.
* **Focus:** Ensure service tests focus on orchestration/business logic, and repository tests focus on data access logic.

## 5. Test Cases & TODOs

Detailed TODOs reside within the READMEs and test files of the subdirectories:

* See `CustomerService/README.md` for `CustomerService` test plans.
* See `CustomerRepository/README.md` for `CustomerRepository` test plans.

## 6. Changelog

* **2025-04-18:** Initial creation - Overview for Customer domain unit tests. (Gemini)

