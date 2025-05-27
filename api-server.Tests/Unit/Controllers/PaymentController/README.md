# Module/Directory: /Unit/Controllers/PaymentController

**Last Updated:** 2025-04-18

> **Parent:** [`Controllers`](../README.md)
> **Related:**
> * **Source:** [`PaymentController.cs`](../../../../api-server/Controllers/PaymentController.cs)
> * **Service:** [`Services/Payment/IPaymentService.cs`](../../../../api-server/Services/Payment/IPaymentService.cs)
> * **Models:** [`Services/Payment/PaymentModels.cs`](../../../../api-server/Services/Payment/PaymentModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests specifically for the **internal logic of the `PaymentController` action methods**, isolated from the ASP.NET Core pipeline and the actual `PaymentService` / Stripe interactions.

* **Why Unit Tests?** To provide fast, focused feedback on the controller's logic for handling payment requests. They ensure the controller correctly:
    * Extracts user identifiers or request data.
    * Calls the appropriate methods on the mocked `IPaymentService`.
    * Handles results (e.g., session URLs, status objects, webhook processing outcomes) returned by the mocked service.
    * Translates service results or exceptions into the correct `ActionResult` (e.g., `OkObjectResult`, `BadRequestResult`, `RedirectResult`).
    * Correctly handles reading the request body for webhook events.
* **Why Mock Dependencies?** To isolate the controller's logic. The primary dependency `IPaymentService` (and `ILogger`) is mocked. `HttpContext` (including `HttpRequest` for webhooks) is mocked to provide user context and simulate incoming request details.

## 2. Scope & Key Functionality Tested (What?)

These tests focus on the code *within* the `PaymentController` actions:

* **User Identification:** Verifying correct extraction of the user ID from the mocked `HttpContext.User` for authenticated actions.
* **Service Invocation:** Ensuring the correct `IPaymentService` methods (`CreateCheckoutSessionAsync`, `HandleWebhookAsync`, `CreatePortalSessionAsync`, `GetPaymentStatusAsync`) are called with the expected parameters derived from the request context or body.
* **Result Handling:** Testing how different return values or exceptions from the mocked `IPaymentService` are processed and mapped to `ActionResult` types.
* **Webhook Request Handling:** Verifying the logic for reading the raw request body and potentially headers (`Stripe-Signature`) from the mocked `HttpRequest` and passing them to the `IPaymentService`.
* **Input Mapping/Validation:** Testing the mapping of request DTOs (if any) to service parameters.

## 3. Test Environment Setup

* **Instantiation:** `PaymentController` is instantiated directly in tests.
* **Mocking:** Mocks for `Zarichnyi.ApiServer.Services.Payment.IPaymentService` and `Microsoft.Extensions.Logging.ILogger<PaymentController>` must be provided.
* **HttpContext Mocking:** A mock `HttpContext` is required.
    * For authenticated actions: Mock `User` with necessary claims (e.g., `NameIdentifier`).
    * For webhook action: Mock `Request`, including `Body` (as a `MemoryStream` containing the JSON payload) and `Headers` (containing `Stripe-Signature` if testing header access). Assign the mock `HttpContext` to the controller's `ControllerContext`.

## 4. Maintenance Notes & Troubleshooting

* **`IPaymentService` Mocking:** Ensure mocks accurately reflect the expected return values or exception behavior for different scenarios. Verify mock interactions (`Mock.Verify(...)`).
* **Webhook Mocking:** Mocking `HttpRequest` to simulate the raw body stream and headers requires careful setup. Ensure the mocked stream is readable and contains the expected JSON payload.
* **User Context:** Ensure the mocked `HttpContext.User` is correctly set up for actions requiring authentication.

## 5. Test Cases & TODOs

### `CreateCheckoutSession` Action
* **TODO:** Test user ID extraction from mocked `HttpContext`.
* **TODO:** Test mapping request data (if any) to service parameters.
* **TODO:** Test `_paymentService.CreateCheckoutSessionAsync` called with correct user ID/data.
* **TODO:** Test handling successful service result (session URL/ID) -> verify `OkObjectResult`.
* **TODO:** Test handling failure/exception from service -> verify appropriate error `ActionResult`.

### `HandleWebhook` Action
* **TODO:** Test reading raw request body from mocked `HttpRequest.Body`.
* **TODO:** Test reading `Stripe-Signature` header from mocked `HttpRequest.Headers`.
* **TODO:** Test `_paymentService.HandleWebhookAsync` called with extracted body and signature.
* **TODO:** Test handling successful webhook processing -> verify `OkResult`.
* **TODO:** Test handling failure/exception from service (e.g., invalid signature, processing error) -> verify `BadRequestResult` or appropriate error.
* **TODO:** Test handling `IOException` during body reading -> verify `BadRequestResult`.

### `CreatePortalSession` Action
* **TODO:** Test user ID extraction from mocked `HttpContext`.
* **TODO:** Test `_paymentService.CreatePortalSessionAsync` called with correct user ID.
* **TODO:** Test handling successful service result (portal URL) -> verify `OkObjectResult` containing the URL.
* **TODO:** Test handling failure/exception from service -> verify appropriate error `ActionResult`.

### `GetPaymentStatus` Action
* **TODO:** Test user ID extraction from mocked `HttpContext`.
* **TODO:** Test `_paymentService.GetPaymentStatusAsync` called with correct user ID.
* **TODO:** Test handling successful service result (status object) -> verify `OkObjectResult` with status.
* **TODO:** Test handling failure/exception from service -> verify appropriate error `ActionResult`.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `PaymentController` unit tests. (Gemini)

