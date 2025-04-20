# Module/Directory: /Unit/Cookbook/Orders

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Unit/Cookbook/` may be needed)*
> **Related:**
> * **Source:** [`Cookbook/Orders/`](../../../../api-server/Cookbook/Orders/)
> * **Subdirectories:** [`OrderService/`](OrderService/README.md), [`OrderRepository/`](OrderRepository/README.md)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the domain logic related to **Orders** within the Cookbook feature. This typically involves creating cookbook generation orders, tracking their status, and managing associated data. It includes tests for the `OrderService` (orchestration and business logic) and the `OrderRepository` (data access logic).

* **Why Unit Tests?** To validate the specific responsibilities of the service and repository layers in isolation from each other, the database, and other services involved in the order lifecycle (like AI prompts, PDF generation, background tasks, email notifications).
* **Isolation:** `OrderService` tests mock `IOrderRepository` and other service dependencies (`IBackgroundWorker`, `IEmailService`, etc.). `OrderRepository` tests mock the underlying data store (e.g., `DbContext`).

## 2. Scope & Key Functionality Tested (What?)

* **`OrderServiceTests`:** Focuses on testing the orchestration logic within `OrderService`, ensuring it correctly calls methods on the mocked `IOrderRepository` and other mocked services (e.g., queuing background tasks, sending emails on status changes), handles results, and implements order-specific business rules. See `OrderService/README.md` for details.
* **`OrderRepositoryTests`:** Focuses on testing the data access logic within `OrderRepository`, ensuring it correctly interacts with the mocked data store (e.g., constructing queries for orders and related data, performing CRUD operations). See `OrderRepository/README.md` for details.

## 3. Test Environment Setup

* **Instantiation:** Services and repositories under test are instantiated directly.
* **Mocking:** Dependencies are mocked using frameworks like Moq. `OrderService` mocks `IOrderRepository` and potentially many other services. `OrderRepository` mocks its specific data source interface (e.g., `DbContext`). Loggers are also mocked.

## 4. Maintenance Notes & Troubleshooting

* **Structure:** Tests are organized into subdirectories mirroring the source structure (`OrderService/`, `OrderRepository/`).
* **Dependency Mocking:** Correctly mocking the numerous dependencies for `OrderService` and the data source for `OrderRepository` is crucial.
* **Focus:** Ensure service tests focus on orchestration/business logic, and repository tests focus on data access logic.

## 5. Test Cases & TODOs

Detailed TODOs reside within the READMEs and test files of the subdirectories:

* See `OrderService/README.md` for `OrderService` test plans.
* See `OrderRepository/README.md` for `OrderRepository` test plans.

## 6. Changelog

* **2025-04-18:** Initial creation - Overview for Order domain unit tests. (Gemini)

