# Module/Directory: /Unit/Services/Payment/PaymentService

**Last Updated:** 2025-04-18

> **Parent:** [`Payment`](../README.md)
> *(Note: A README for `/Unit/Services/Payment/` may be needed)*
> **Related:**
> * **Source:** [`Services/Payment/PaymentService.cs`](../../../../../api-server/Services/Payment/PaymentService.cs)
> * **Dependencies:** `IStripeService`, `UserManager<IdentityUser>` / `ICustomerService` (potentially), `ILogger<PaymentService>`
> * **Interface:** [`Services/Payment/IStripeService.cs`](../../../../../api-server/Services/Payment/StripeService.cs) (Defines contract for StripeService)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `PaymentService` class. This service typically acts as a higher-level abstraction or orchestrator for payment-related operations, delegating the actual interactions with the payment provider (Stripe) to the `IStripeService`.

* **Why Unit Tests?** To validate the orchestration logic within `PaymentService` in isolation from the `StripeService` implementation and other dependencies like user management. Tests ensure that `PaymentService` correctly maps application-level requests to calls on `IStripeService`, potentially fetches or updates user/customer data related to payments, and handles the results or exceptions returned by `IStripeService`.
* **Isolation:** Achieved by mocking `IStripeService`, `UserManager`/`ICustomerService` (if used), and `ILogger`. The tests focus on the `PaymentService`'s logic, not the details of Stripe API interaction (which are tested in `StripeServiceTests`).

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `PaymentService` focus on its public methods, such as:

* **`CreateCheckoutSessionAsync(...)`:**
    * Retrieving user/customer information (via mocked `UserManager`/`ICustomerService`) if needed to pre-fill Stripe session.
    * Calling `_stripeService.CreateStripeCheckoutSessionAsync` with correctly mapped parameters (e.g., price IDs, user email, customer ID).
    * Handling the session URL/ID returned by the mocked `IStripeService`.
    * Handling exceptions from the mocked `IStripeService`.
* **`HandleWebhookAsync(string jsonPayload, string signatureHeader)`:**
    * Calling `_stripeService.HandleStripeWebhookAsync` with the raw payload and signature.
    * Potentially performing actions based on the outcome of webhook processing reported by `IStripeService` (e.g., updating user status via mocked `UserManager`).
    * Handling success/failure results from the mocked `IStripeService`.
* **`CreatePortalSessionAsync(...)`:**
    * Retrieving the user's Stripe Customer ID (via mocked `UserManager`/`ICustomerService`).
    * Calling `_stripeService.CreateStripePortalSessionAsync` with the correct customer ID.
    * Handling the portal session URL returned by the mocked `IStripeService`.
* **`GetPaymentStatusAsync(...)`:**
    * Retrieving user/customer information.
    * Calling `_stripeService.GetSubscriptionStatusAsync` (or similar) with relevant identifiers.
    * Mapping the status returned by the mocked `IStripeService` to an application-level status object.

## 3. Test Environment Setup

* **Instantiation:** `PaymentService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<IStripeService>`: Set up to return expected results (session URLs, customer IDs, statuses) or throw exceptions for its methods.
    * `Mock<UserManager<IdentityUser>>` / `Mock<ICustomerService>`: If `PaymentService` interacts with user/customer data.
    * `Mock<ILogger<PaymentService>>`.

## 4. Maintenance Notes & Troubleshooting

* **Focus on Orchestration:** These tests should verify that `PaymentService` correctly uses `IStripeService`, not the intricacies of Stripe itself.
* **`IStripeService` Mocking:** Ensure the mock setups for `IStripeService` methods accurately reflect the scenarios needed to test `PaymentService` logic paths (e.g., simulate Stripe returning a valid session, simulate Stripe throwing an error).
* **User/Customer Data:** If user/customer data is involved, ensure the relevant mocks (`UserManager`, `ICustomerService`) are set up to provide necessary test data (e.g., user email, Stripe customer ID).

## 5. Test Cases & TODOs

### `PaymentServiceTests.cs`
* **TODO (`CreateCheckoutSessionAsync`):** Test success path -> verify `_stripeService.CreateStripeCheckoutSessionAsync` called with correct args, verify result handled.
* **TODO (`CreateCheckoutSessionAsync`):** Test user lookup interaction (if applicable).
* **TODO (`CreateCheckoutSessionAsync`):** Test handling exception from `_stripeService`.
* **TODO (`HandleWebhookAsync`):** Test success path -> verify `_stripeService.HandleStripeWebhookAsync` called, verify potential user update calls based on result.
* **TODO (`HandleWebhookAsync`):** Test failure path -> verify handling of failure result from `_stripeService`.
* **TODO (`CreatePortalSessionAsync`):** Test success path -> verify user lookup, verify `_stripeService.CreateStripePortalSessionAsync` called, verify result handled.
* **TODO (`CreatePortalSessionAsync`):** Test user has no Stripe ID scenario (if applicable).
* **TODO (`CreatePortalSessionAsync`):** Test handling exception from `_stripeService`.
* **TODO (`GetPaymentStatusAsync`):** Test success path -> verify user lookup, verify `_stripeService.GetSubscriptionStatusAsync` called, verify status mapping.
* **TODO (`GetPaymentStatusAsync`):** Test handling exception from `_stripeService`.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `PaymentService` unit tests. (Gemini)

