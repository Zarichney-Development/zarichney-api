# Module/Directory: /Integration/Controllers/PaymentController

**Last Updated:** 2025-04-18

> **Parent:** [`Controllers`](../README.md)
> **Related:**
> * **Source:** [`PaymentController.cs`](../../../../Zarichney.Server/Controllers/PaymentController.cs)
> * **Service:** [`Services/Payment/PaymentService.cs`](../../../../Zarichney.Server/Services/Payment/PaymentService.cs), [`Services/Payment/StripeService.cs`](../../../../Zarichney.Server/Services/Payment/StripeService.cs)
> * **Models:** [`Services/Payment/PaymentModels.cs`](../../../../Zarichney.Server/Services/Payment/PaymentModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs), [`AuthTestHelper.cs`](../../../Framework/Helpers/AuthTestHelper.cs), [`MockStripeServiceFactory.cs`](../../../Mocks/Factories/MockStripeServiceFactory.cs)

## 1. Purpose & Rationale (Why?)

This directory contains integration tests for the `PaymentController` endpoints, which handle interactions with the Stripe payment gateway, including creating checkout sessions, handling webhooks, and managing billing portals.

* **Why Integration Tests?** To ensure these critical payment-related endpoints function correctly within the ASP.NET Core pipeline. This involves verifying routing, authentication, model binding, middleware interactions, correct calls to the underlying (mocked) `IPaymentService` / `IStripeService`, and validating HTTP responses (redirects, session IDs, status codes, webhook acknowledgments).
* **Why Mock Dependencies?** Direct interaction with the live Stripe API during automated testing is impractical and undesirable. The `IPaymentService` and/or `IStripeService` are mocked (often via `MockStripeServiceFactory`) within the `CustomWebApplicationFactory` to simulate Stripe API responses in a controlled manner, isolating the controller and application logic.

## 2. Scope & Key Functionality Tested (What?)

These tests cover endpoints managing the payment lifecycle:

* **`POST /api/payment/checkout-session`:** Creating a Stripe Checkout session for purchasing a product or service. Verifies request handling and response containing the session ID/URL. Requires authentication.
* **`POST /api/payment/webhook`:** Handling incoming webhook events from Stripe (e.g., `checkout.session.completed`, `invoice.payment_succeeded`). Verifies event parsing, signature validation (or bypass), interaction with `IPaymentService` to process the event, and correct acknowledgment response (e.g., 200 OK). This endpoint is typically `[AllowAnonymous]` but performs its own signature validation.
* **`GET /api/payment/portal-session`:** Creating a Stripe Billing Portal session for customers to manage their subscriptions/billing info. Verifies response containing the portal session URL. Requires authentication.
* **`GET /api/payment/status`:** Retrieving the current user's payment or subscription status. Requires authentication.
* **Authorization:** Ensuring endpoints correctly enforce authentication where required.
* **Error Handling:** Verifying correct responses for invalid requests, service failures, or webhook processing errors.

## 3. Test Environment Setup

* **Test Server:** Provided by `CustomWebApplicationFactory<Program>`.
* **Authentication:** Simulated using `TestAuthHandler` and `AuthTestHelper` for authenticated endpoints (`/checkout-session`, `/portal-session`, `/status`).
* **Mocked Dependencies:** Configured in `CustomWebApplicationFactory`. Key mocks include:
    * `Zarichnyi.ApiServer.Services.Payment.IPaymentService`
    * `Zarichnyi.ApiServer.Services.Payment.IStripeService` (often managed via `MockStripeServiceFactory`)
* **Webhook Testing:** Requires constructing an `HttpRequestMessage` that accurately simulates a request from Stripe, including the correct `Stripe-Signature` header (if testing signature validation) and a JSON payload matching a Stripe event object. Signature validation might be bypassed in tests by configuring the mock service or application settings appropriately.

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Tests should be organized logically, potentially by endpoint (e.g., `CheckoutSessionTests.cs`, `WebhookHandlerTests.cs`, `PortalSessionTests.cs`).
* **Stripe Service Mocking:** Ensure `MockStripeServiceFactory` (or direct mocks) are configured to simulate expected responses from Stripe API calls (e.g., returning checkout session objects, portal session objects, handling webhook event processing).
* **Webhook Simulation:** Accurately simulating Stripe webhook requests is crucial. Use real (but anonymized) Stripe event JSON payloads if possible. Pay close attention to the raw request body and signature header generation/validation logic. Consider helper methods for constructing valid webhook test requests.
* **Configuration:** Webhook signing secrets and Stripe API keys used by the application need to be configured correctly (or mocked/bypassed) in the test environment (`appsettings.Testing.json` or factory configuration).

## 5. Test Cases & TODOs

### Create Checkout Session (`CheckoutSessionTests.cs`)
* **TODO:** Test `POST /checkout-session` authenticated with valid input -> mock `IPaymentService.CreateCheckoutSessionAsync` success -> verify 200 OK with session URL/ID.
* **TODO:** Test `POST /checkout-session` with invalid input data -> verify 400 Bad Request.
* **TODO:** Test `POST /checkout-session` unauthenticated -> verify 401 Unauthorized.
* **TODO:** Test `POST /checkout-session` when `IPaymentService.CreateCheckoutSessionAsync` fails (e.g., Stripe API error simulation) -> verify appropriate error response (e.g., 500).

### Handle Webhook (`WebhookHandlerTests.cs`)
* **TODO:** Test `POST /webhook` with valid `checkout.session.completed` event and valid signature -> mock `IPaymentService.HandleWebhookAsync` success -> verify 200 OK.
* **TODO:** Test `POST /webhook` with valid `invoice.payment_succeeded` event and valid signature -> mock `IPaymentService.HandleWebhookAsync` success -> verify 200 OK.
* **TODO:** Test `POST /webhook` with other relevant event types.
* **TODO:** Test `POST /webhook` with an *invalid* signature -> verify 400 Bad Request.
* **TODO:** Test `POST /webhook` with valid signature but event processing fails in `IPaymentService.HandleWebhookAsync` -> verify 500 Internal Server Error (or appropriate error).
* **TODO:** Test `POST /webhook` with malformed/unparseable JSON payload -> verify 400 Bad Request.
* **TODO:** Test `POST /webhook` signature validation bypass scenario (if configured for testing).

### Create Portal Session (`PortalSessionTests.cs`)
* **TODO:** Test `GET /portal-session` authenticated -> mock `IPaymentService.CreatePortalSessionAsync` success -> verify 200 OK with portal session URL.
* **TODO:** Test `GET /portal-session` unauthenticated -> verify 401 Unauthorized.
* **TODO:** Test `GET /portal-session` when `IPaymentService.CreatePortalSessionAsync` fails -> verify appropriate error response (e.g., 500).
* **TODO:** Test `GET /portal-session` for user with no Stripe customer ID (if applicable) -> verify 404 Not Found or appropriate response.

### Get Payment Status (`PaymentStatusTests.cs`)
* **TODO:** Test `GET /status` authenticated, user has active status -> mock `IPaymentService.GetPaymentStatusAsync` returns active status -> verify 200 OK with status.
* **TODO:** Test `GET /status` authenticated, user has inactive status -> mock `IPaymentService.GetPaymentStatusAsync` returns inactive status -> verify 200 OK with status.
* **TODO:** Test `GET /status` authenticated, user has no payment history -> mock `IPaymentService.GetPaymentStatusAsync` returns appropriate status -> verify 200 OK.
* **TODO:** Test `GET /status` unauthenticated -> verify 401 Unauthorized.
* **TODO:** Test `GET /status` when `IPaymentService.GetPaymentStatusAsync` fails -> verify 500 Internal Server Error.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `PaymentController` integration tests. (Gemini)

