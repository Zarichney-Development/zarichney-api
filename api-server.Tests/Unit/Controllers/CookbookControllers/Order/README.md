# Module/Directory: /Unit/Controllers/CookbookControllers/Order

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Unit/Controllers/CookbookControllers/` may be needed)*
> **Related:**
> * **Source:** [`CookbookController.cs`](../../../../../api-server/Controllers/CookbookController.cs)
> * **Service:** [`Cookbook/Orders/IOrderService.cs`](../../../../../api-server/Cookbook/Orders/IOrderService.cs)
> * **Models:** [`Cookbook/Orders/OrderModels.cs`](../../../../../api-server/Cookbook/Orders/OrderModels.cs)
> * **Other Services:** [`IPdfCompiler.cs`](../../../../../api-server/Services/PdfGeneration/IPdfCompiler.cs), [`IFileService.cs`](../../../../../api-server/Services/FileSystem/IFileService.cs), [`IBackgroundWorker.cs`](../../../../../api-server/Services/BackgroundTasks/IBackgroundWorker.cs), [`IEmailService.cs`](../../../../../api-server/Services/Email/IEmailService.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests specifically for the **internal logic of the Order-related action methods** within the `CookbookController`, isolated from the ASP.NET Core pipeline and real service implementations.

* **Why Unit Tests?** To provide fast, focused feedback on the controller's logic for handling cookbook order requests. They ensure the controller correctly:
    * Extracts user identifiers from the context.
    * Maps request DTOs (e.g., for creating orders) to service parameters.
    * Calls the appropriate methods on mocked services (`IOrderService`, `IPdfCompiler`, `IFileService`, `IBackgroundWorker`).
    * Handles results (e.g., `Order` objects, statuses, file streams) returned by mocked services.
    * Translates service results or exceptions into the correct `ActionResult` (e.g., `OkObjectResult`, `NotFoundResult`, `FileStreamResult`, `AcceptedResult`).
* **Why Mock Dependencies?** To isolate the controller's logic. All key dependencies (`IOrderService`, `IPdfCompiler`, `IFileService`, `IBackgroundWorker`, `ILogger`) are mocked. `HttpContext` is also mocked to provide user context.
* **Email Notifications:** Note that the underlying order process involves sending emails (e.g., "Cookbook Ready", "Order Failed") via `IEmailService`. While this controller indirectly triggers those actions via `IOrderService` or `IBackgroundWorker`, the responsibility for *verifying email sending logic* lies within the **unit tests for `OrderService`** or **integration tests** where `IEmailService` is mocked directly. These controller unit tests focus solely on the controller's interaction with its immediate dependencies.

## 2. Scope & Key Functionality Tested (What?)

These tests focus on the code *within* the Order-related actions of `CookbookController`:

* **User Identification:** Verifying correct extraction of the user ID from the mocked `HttpContext.User`.
* **Service Invocation:** Ensuring the correct methods on `IOrderService`, `IPdfCompiler`, `IFileService`, and `IBackgroundWorker` are called with expected parameters.
* **Result Handling:** Testing how `Order` objects, statuses, `Stream` objects, or exceptions from mocked services are processed and mapped to `ActionResult` types.
* **Background Task Queuing:** Verifying that `IBackgroundWorker.QueueWorkItemAsync` is called correctly when an order is created.
* **File Stream Handling:** Testing the creation of `FileStreamResult` for PDF downloads, including setting content type and file name.
* **Input Mapping/Validation:** Testing the mapping of request DTOs for order creation.

## 3. Test Environment Setup

* **Instantiation:** `CookbookController` is instantiated directly in tests.
* **Mocking:** Mocks for `IOrderService`, `IPdfCompiler`, `IFileService`, `IBackgroundWorker`, and `ILogger<CookbookController>` must be provided.
* **HttpContext Mocking:** A mock `HttpContext` with a mock `ClaimsPrincipal` (User) containing necessary claims (e.g., `NameIdentifier`) is required and assigned to the controller's `ControllerContext`.

## 4. Maintenance Notes & Troubleshooting

* **Mocking Complexity:** Mocking the interactions between multiple services (`IOrderService`, `IFileService`, `IPdfCompiler`) for scenarios like PDF download requires careful setup.
* **`IBackgroundWorker` Verification:** Use `Mock.Verify` to ensure background tasks are queued as expected.
* **`FileStreamResult` Assertions:** Check the `ContentType` and `FileDownloadName` properties of the returned `FileStreamResult`. Mock file streams using `MemoryStream` for testing purposes.
* **User Context:** Ensure the mocked `HttpContext.User` provides the correct user ID for testing ownership logic within the controller actions.
* **Email Testing:** Remember that verifying *if* an email should be sent based on order status changes happens in `OrderService` tests or integration tests, not directly here.

## 5. Test Cases & TODOs

### `CreateOrder` Action (POST /orders)
* **TODO:** Test mapping request DTO to `IOrderService.CreateOrderAsync` parameters.
* **TODO:** Test user ID extraction for `CreateOrderAsync`.
* **TODO:** Test `_orderService.CreateOrderAsync` called.
* **TODO:** Test `_backgroundWorker.QueueWorkItemAsync` called on successful order creation.
* **TODO:** Test handling successful order creation -> verify `AcceptedResult` or `CreatedAtActionResult` with order details. (Note: Email sending verification belongs elsewhere).
* **TODO:** Test handling failure from `_orderService.CreateOrderAsync` -> verify appropriate error `ActionResult`.

### `GetMyOrders` Action (GET /orders)
* **TODO:** Test user ID extraction.
* **TODO:** Test `_orderService.GetOrdersByUserIdAsync` called with correct ID.
* **TODO:** Test handling successful result -> verify `OkObjectResult` with list of orders.
* **TODO:** Test handling exception from service -> verify exception propagation.

### `GetOrderById` Action (GET /orders/{orderId})
* **TODO:** Test user ID extraction for ownership check.
* **TODO:** Test `_orderService.GetOrderByIdAsync` called with correct order ID.
* **TODO:** Test handling order found, user is owner -> verify `OkObjectResult` with order.
* **TODO:** Test handling order found, user is *not* owner -> verify `ForbidResult` or `NotFoundResult`.
* **TODO:** Test handling order not found (service returns null) -> verify `NotFoundResult`.
* **TODO:** Test handling exception from service -> verify exception propagation.

### `GetOrderStatus` Action (GET /orders/{orderId}/status)
* **TODO:** Test user ID extraction for ownership check.
* **TODO:** Test `_orderService.GetOrderByIdAsync` called.
* **TODO:** Test handling order found, user is owner -> verify `OkObjectResult` with status.
* **TODO:** Test handling order found, user is *not* owner -> verify `ForbidResult` or `NotFoundResult`.
* **TODO:** Test handling order not found -> verify `NotFoundResult`.

### `DownloadCookbookPdf` Action (GET /orders/{orderId}/pdf)
* **TODO:** Test user ID extraction for ownership check.
* **TODO:** Test `_orderService.GetOrderByIdAsync` called.
* **TODO:** Test handling order not found -> verify `NotFoundResult`.
* **TODO:** Test handling order found, user is *not* owner -> verify `ForbidResult` or `NotFoundResult`.
* **TODO:** Test handling order found, status is not 'Complete' -> verify `NotFoundResult` or `ConflictResult`.
* **TODO:** Test handling order found, status 'Complete' -> verify `_fileService.ReadFileStreamAsync` called with correct path derived from order. (Note: Email sending verification belongs elsewhere).
* **TODO:** Test handling file found by `_fileService` -> verify `FileStreamResult` with correct stream, content type, and filename.
* **TODO:** Test handling file *not* found by `_fileService` (throws `FileNotFoundException`) -> verify `NotFoundResult`.
* **TODO:** Test handling other exceptions from `_fileService` -> verify exception propagation.

## 6. Changelog

* **2025-04-18:** Revision 2 - Added clarification regarding email notification testing scope. (Gemini)
* **2025-04-18:** Revision 1 - Initial creation - Defined purpose, scope, setup, and TODOs for Order-related `CookbookController` unit tests. (Gemini)

