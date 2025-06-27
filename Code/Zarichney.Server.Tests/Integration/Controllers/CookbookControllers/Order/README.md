# Module/Directory: /Integration/Controllers/CookbookControllers/Order

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Integration/Controllers/CookbookControllers/` may be needed)*
> **Related:**
> * **Source:** [`CookbookController.cs`](../../../../../Zarichney.Server/Controllers/CookbookController.cs)
> * **Service:** [`Cookbook/Orders/OrderService.cs`](../../../../../Zarichney.Server/Cookbook/Orders/OrderService.cs)
> * **Models:** [`Cookbook/Orders/OrderModels.cs`](../../../../../Zarichney.Server/Cookbook/Orders/OrderModels.cs)
> * **Other Services:** [`PdfCompiler.cs`](../../../../../Zarichney.Server/Services/PdfGeneration/PdfCompiler.cs), [`FileService.cs`](../../../../../Zarichney.Server/Services/FileSystem/FileService.cs), [`BackgroundWorker.cs`](../../../../../Zarichney.Server/Services/BackgroundTasks/BackgroundWorker.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../../../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs), [`AuthTestHelper.cs`](../../../Framework/Helpers/AuthTestHelper.cs)

## 1. Purpose & Rationale (Why?)

This directory contains integration tests specifically for the **Order-related endpoints** exposed by the `CookbookController`. These tests validate the behavior of actions related to creating, querying, and managing cookbook generation orders.

* **Why Integration Tests?** To ensure these endpoints function correctly within the ASP.NET Core pipeline. This includes routing, authentication, model binding, interaction with middleware, and verifying the controller's interaction with underlying (mocked) services like `IOrderService`, `IPdfCompiler`, `IFileService`, and `IBackgroundWorker`.
* **Why Mock Dependencies?** Key services involved in order processing (especially PDF generation and background tasks) are typically mocked in the `CustomWebApplicationFactory`. This isolates the controller's endpoint logic, ensuring tests are focused, fast, and reliable without depending on actual file system operations, PDF rendering, or background job execution.

## 2. Scope & Key Functionality Tested (What?)

These tests cover endpoints related to cookbook order management:

* **`POST /api/cookbook/orders`:** Creating a new cookbook order (e.g., based on recipe criteria or selection). Verifies input validation and triggering of the order process (likely via `IOrderService` and potentially queuing work via `IBackgroundWorker`).
* **`GET /api/cookbook/orders`:** Listing orders belonging to the currently authenticated user.
* **`GET /api/cookbook/orders/{orderId}`:** Retrieving details of a specific order.
* **`GET /api/cookbook/orders/{orderId}/status`:** Checking the current status of an order (e.g., Pending, Processing, Complete, Failed).
* **`GET /api/cookbook/orders/{orderId}/pdf`:** Downloading the generated cookbook PDF file for a completed order.
* **Authorization:** Ensuring endpoints correctly enforce authentication and potentially ownership (users can only access their own orders). Admin access might also be tested if applicable.
* **Error Handling:** Verifying correct responses for scenarios like "Order Not Found", "PDF Not Ready", or validation failures.

## 3. Test Environment Setup

* **Test Server:** Provided by `CustomWebApplicationFactory<Program>`.
* **Authentication:** Simulated using `TestAuthHandler` and `AuthTestHelper`. Essential for all order endpoints.
* **Mocked Dependencies:** Configured in `CustomWebApplicationFactory`. Key mocks for these tests typically include:
    * `Zarichnyi.ApiServer.Cookbook.Orders.IOrderService`
    * `Zarichnyi.ApiServer.Services.PdfGeneration.IPdfCompiler` (or the service using it)
    * `Zarichnyi.ApiServer.Services.FileSystem.IFileService` (for retrieving generated PDFs)
    * `Zarichnyi.ApiServer.Services.BackgroundTasks.IBackgroundWorker` (to verify work queuing)

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Tests related to order endpoints should reside within this directory (e.g., `CreateOrderTests.cs`, `GetOrderTests.cs`, `DownloadPdfTests.cs`).
* **Service Mocking:** Ensure mocks for `IOrderService`, `IPdfCompiler`, `IFileService`, and `IBackgroundWorker` are configured correctly in the factory to simulate various states (order found/not found, PDF ready/not ready, file exists/doesn't exist, background task queued successfully).
* **File Downloads:** Testing the `/pdf` endpoint requires special handling to assert the response is a file stream with the correct content type (`application/pdf`) and potentially `Content-Disposition` header. Mock `IFileService` or `IPdfCompiler` to return a test `MemoryStream`.
* **Auth Context:** Ensure authenticated clients created via `AuthTestHelper` have the necessary user ID to test ownership logic (e.g., retrieving *their* orders).

## 5. Test Cases & TODOs

### Create Order (`CreateOrderTests.cs`)
* **TODO:** Test `POST /orders` with valid input -> mock `IOrderService.CreateOrderAsync` success, mock `IBackgroundWorker.QueueWorkItemAsync` -> verify 201 Created / 202 Accepted with order details/ID.
* **TODO:** Test `POST /orders` with invalid input data -> verify 400 Bad Request.
* **TODO:** Test `POST /orders` unauthenticated -> verify 401 Unauthorized.
* **TODO:** Test `POST /orders` when `IOrderService.CreateOrderAsync` fails -> verify appropriate error response (e.g., 500 or specific error).

### Get Orders (`GetOrderTests.cs`)
* **TODO:** Test `GET /orders` authenticated -> mock `IOrderService.GetOrdersByUserIdAsync` returns list -> verify 200 OK with list.
* **TODO:** Test `GET /orders` unauthenticated -> verify 401 Unauthorized.
* **TODO:** Test `GET /orders/{orderId}` authenticated, user owns order -> mock `IOrderService.GetOrderByIdAsync` returns order -> verify 200 OK with order details.
* **TODO:** Test `GET /orders/{orderId}` authenticated, order not found -> mock `IOrderService.GetOrderByIdAsync` returns null -> verify 404 Not Found.
* **TODO:** Test `GET /orders/{orderId}` authenticated, user does *not* own order -> mock `IOrderService.GetOrderByIdAsync` returns order -> verify 403 Forbidden / 404 Not Found (depending on desired behavior).
* **TODO:** Test `GET /orders/{orderId}` unauthenticated -> verify 401 Unauthorized.

### Get Order Status (`GetOrderStatusTests.cs`)
* **TODO:** Test `GET /orders/{orderId}/status` authenticated, order exists -> mock `IOrderService.GetOrderByIdAsync` returns order with specific status -> verify 200 OK with status.
* **TODO:** Test `GET /orders/{orderId}/status` authenticated, order not found -> verify 404 Not Found.
* **TODO:** Test `GET /orders/{orderId}/status` authenticated, user does not own order -> verify 403/404.
* **TODO:** Test `GET /orders/{orderId}/status` unauthenticated -> verify 401 Unauthorized.

### Download PDF (`DownloadPdfTests.cs`)
* **TODO:** Test `GET /orders/{orderId}/pdf` authenticated, order complete, PDF exists -> mock `IOrderService` returns completed order, mock `IFileService.ReadFileStreamAsync` returns stream -> verify 200 OK, correct Content-Type (`application/pdf`), Content-Disposition.
* **TODO:** Test `GET /orders/{orderId}/pdf` authenticated, order not complete -> mock `IOrderService` returns incomplete order -> verify 404 Not Found or 409 Conflict ("PDF not ready").
* **TODO:** Test `GET /orders/{orderId}/pdf` authenticated, order complete, PDF file missing -> mock `IOrderService` returns completed order, mock `IFileService.ReadFileStreamAsync` throws `FileNotFoundException` -> verify 404 Not Found.
* **TODO:** Test `GET /orders/{orderId}/pdf` authenticated, user does not own order -> verify 403/404.
* **TODO:** Test `GET /orders/{orderId}/pdf` unauthenticated -> verify 401 Unauthorized.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for Order-related `CookbookController` integration tests. (Gemini)

