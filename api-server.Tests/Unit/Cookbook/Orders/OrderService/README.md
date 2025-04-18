# Module/Directory: /Unit/Cookbook/Orders/OrderService

**Last Updated:** 2025-04-18

> **Parent:** [`Orders`](../README.md)
> **Related:**
> * **Source:** [`Cookbook/Orders/OrderService.cs`](../../../../../api-server/Cookbook/Orders/OrderService.cs)
> * **Interface:** [`Cookbook/Orders/IOrderService.cs`](../../../../../api-server/Cookbook/Orders/OrderService.cs) (Implicit)
> * **Dependencies:** `IOrderRepository`, `ICustomerRepository` (potentially), `IRecipeService` (potentially), `IEmailService`, `IBackgroundWorker`, `IPdfCompiler`, `IFileService`, `ILogger<OrderService>`
> * **Models:** [`Cookbook/Orders/OrderModels.cs`](../../../../../api-server/Cookbook/Orders/OrderModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `OrderService` class. This service orchestrates the cookbook order lifecycle, including creation, status updates, triggering background processing (like PDF generation), and potentially sending notifications. It coordinates interactions between repositories and other services.

* **Why Unit Tests?** To validate the complex orchestration logic within `OrderService` in isolation from the data access layer (`IOrderRepository`) and other external services (`IBackgroundWorker`, `IEmailService`, `IPdfCompiler`, `IFileService`). Tests ensure the service correctly manages order state transitions, calls dependencies appropriately, and handles results or errors from those dependencies.
* **Isolation:** Achieved by mocking all injected dependencies: `IOrderRepository`, `IEmailService`, `IBackgroundWorker`, `IPdfCompiler`, `IFileService`, `ILogger`, and any others.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `OrderService` focus on its public methods:

* **`CreateOrderAsync(...)`:**
    * Verifying input validation.
    * Verifying `_orderRepository.AddOrderAsync` is called with correctly constructed `Order` data.
    * Verifying `_backgroundWorker.QueueWorkItemAsync` is called (potentially with a delegate targeting a processing method like `ProcessOrderAsync`) upon successful order creation.
    * Handling results/exceptions from mocked repository and background worker.
* **`GetOrderByIdAsync(int orderId)` / `GetOrdersByUserIdAsync(string userId)`:**
    * Verifying `_orderRepository` methods are called with correct parameters.
    * Handling `Order` objects or lists returned by the mocked repository.
    * Handling not found scenarios (repository returns null).
* **`UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? resultPath = null)`:**
    * Retrieving the order via mocked `_orderRepository`.
    * Validating status transitions (if applicable).
    * Updating the order object's status and potentially result path.
    * Verifying `_orderRepository.UpdateOrderAsync` is called with the updated order.
    * Verifying `_emailService.SendEmailAsync` is called with appropriate parameters based on the `newStatus` (e.g., "Cookbook Ready", "Order Failed").
    * Handling order not found or repository update failures.
* **`ProcessOrderAsync(int orderId)`:** (If this logic resides directly in `OrderService` and is called by the background worker delegate)
    * Retrieving order and related data (recipes) via mocked repositories.
    * Calling `_pdfCompiler.CompileCookbookAsync` with correct data.
    * Calling `_fileService.WriteToFileAsync` with the generated PDF stream.
    * Calling `UpdateOrderStatusAsync` to mark the order as Complete or Failed.
    * Handling exceptions during PDF compilation or file writing.

## 3. Test Environment Setup

* **Instantiation:** `OrderService` is instantiated directly.
* **Mocking:** Dependencies are mocked using Moq. Key mocks include:
    * `Mock<IOrderRepository>`: Setup `AddOrderAsync`, `GetOrderByIdAsync`, `GetOrdersByUserIdAsync`, `UpdateOrderAsync`.
    * `Mock<IEmailService>`: Verify `SendEmailAsync` calls.
    * `Mock<IBackgroundWorker>`: Verify `QueueWorkItemAsync` calls.
    * `Mock<IPdfCompiler>`: Setup `CompileCookbookAsync` to return a stream or throw.
    * `Mock<IFileService>`: Setup/Verify `WriteToFileAsync`.
    * `Mock<ILogger<OrderService>>`.
    * Mocks for `ICustomerRepository`, `IRecipeService` if used directly.

## 4. Maintenance Notes & Troubleshooting

* **Complex Dependencies:** `OrderService` often has many dependencies. Ensure all are correctly mocked for each test scenario.
* **State Transitions:** Pay close attention to testing different order status transitions and verifying the correct side effects (repository updates, emails sent, background tasks queued).
* **Background Task Logic:** If testing methods called *by* the background worker (like `ProcessOrderAsync`), ensure the test setup simulates the context correctly. Verifying that the task was *queued* correctly is often tested in the method that calls `QueueWorkItemAsync` (like `CreateOrderAsync`).

## 5. Test Cases & TODOs

### `OrderServiceTests.cs`
* **TODO (`CreateOrderAsync`):** Test success -> verify repo `AddOrderAsync` called, verify `_backgroundWorker.QueueWorkItemAsync` called, return success/order ID.
* **TODO (`CreateOrderAsync`):** Test repo `AddOrderAsync` fails -> verify error handling.
* **TODO (`GetOrderByIdAsync`):** Test found -> mock repo returns order -> verify order returned.
* **TODO (`GetOrderByIdAsync`):** Test not found -> mock repo returns null -> verify null returned.
* **TODO (`GetOrdersByUserIdAsync`):** Test returns list from mock repo.
* **TODO (`UpdateOrderStatusAsync` - To Complete):** Test success -> mock repo `Get/Update`, verify `_emailService.SendEmailAsync` called (Cookbook Ready), verify success result.
* **TODO (`UpdateOrderStatusAsync` - To Failed):** Test success -> mock repo `Get/Update`, verify `_emailService.SendEmailAsync` called (Order Failed), verify success result.
* **TODO (`UpdateOrderStatusAsync` - Order Not Found):** Mock repo `GetOrderByIdAsync` returns null -> verify failure.
* **TODO (`UpdateOrderStatusAsync` - Repo Update Fails):** Mock repo `UpdateOrderAsync` throws -> verify failure.
* **TODO (`ProcessOrderAsync` - If tested here):** Test success -> mock repo/compiler/fileService success -> verify `UpdateOrderStatusAsync` called with Complete status.
* **TODO (`ProcessOrderAsync` - PDF Compile Fails):** Mock `_pdfCompiler` throws -> verify `UpdateOrderStatusAsync` called with Failed status.
* **TODO (`ProcessOrderAsync` - File Write Fails):** Mock `_fileService` throws -> verify `UpdateOrderStatusAsync` called with Failed status.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `OrderService` unit tests. (Gemini)

