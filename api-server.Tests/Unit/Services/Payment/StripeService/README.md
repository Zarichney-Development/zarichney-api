# Module/Directory: /Unit/Services/Payment/StripeService

**Last Updated:** 2025-04-18

> **Parent:** [`Payment`](../README.md)
> *(Note: A README for `/Unit/Services/Payment/` may be needed)*
> **Related:**
> * **Source:** [`Services/Payment/StripeService.cs`](../../../../../api-server/Services/Payment/StripeService.cs)
> * **Interface:** [`Services/Payment/IStripeService.cs`](../../../../../api-server/Services/Payment/StripeService.cs)
> * **Dependencies:** `Stripe.net` SDK clients (e.g., `Stripe.SessionService`, `Stripe.CustomerService`, `Stripe.BillingPortal.SessionService`, `Stripe.WebhookUtility`), `IOptions<StripeSettings>`, `ILogger<StripeService>`
> * **Models:** [`Config/ConfigModels.cs`](../../../../../api-server/Config/ConfigModels.cs) (for `StripeSettings`)
> * **Standards:** [`TestingStandards.md`](../../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `StripeService` class. This service is responsible for the direct interaction with the Stripe payment gateway using the official `Stripe.net` SDK library.

* **Why Unit Tests?** To validate the service's logic for constructing Stripe API requests, calling the correct `Stripe.net` SDK methods, parsing responses from Stripe (simulated via mocks), and handling Stripe-specific exceptions, all in isolation from the actual Stripe API.
* **Isolation:** Achieved primarily by mocking the relevant `Stripe.net` service classes (e.g., `SessionService`, `CustomerService`, `BillingPortal.SessionService`) and static methods (`WebhookUtility.ConstructEvent`). Also mocks configuration (`IOptions<StripeSettings>`) and logging.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `StripeService` focus on its public methods implementing `IStripeService`:

* **`CreateStripeCheckoutSessionAsync(...)`:**
    * Correctly constructing the `Stripe.Checkout.SessionCreateOptions` object based on input parameters.
    * Invoking the mocked `Stripe.Checkout.SessionService.CreateAsync` method with the correct options.
    * Handling the `Stripe.Checkout.Session` object returned by the mocked call (e.g., extracting the URL or ID).
    * Handling `StripeException` thrown by the mocked call.
* **`HandleStripeWebhookAsync(...)`:**
    * Calling the mocked `Stripe.WebhookUtility.ConstructEvent` method with the correct JSON payload, signature header, and webhook secret (from mocked settings).
    * Handling the `Stripe.Event` object returned by the mocked utility.
    * Processing different event types (e.g., `checkout.session.completed`, `invoice.payment_succeeded`) based on the mocked event data.
    * Handling `StripeException` (e.g., invalid signature) thrown by the mocked utility.
* **`CreateStripePortalSessionAsync(...)`:**
    * Correctly constructing the `Stripe.BillingPortal.SessionCreateOptions` object.
    * Invoking the mocked `Stripe.BillingPortal.SessionService.CreateAsync` method.
    * Handling the `Stripe.BillingPortal.Session` object returned by the mocked call.
    * Handling `StripeException` from the mocked call.
* **Other methods** (e.g., `CreateStripeCustomerAsync`, `GetSubscriptionStatusAsync`): Tested similarly, focusing on options construction, mock SDK service calls, response handling, and exception handling.

## 3. Test Environment Setup

* **Instantiation:** `StripeService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * Mocks for **Stripe.net service classes** (e.g., `Mock<SessionService>`, `Mock<CustomerService>`, `Mock<BillingPortal.SessionService>`). Since these are often concrete classes, mocking might involve mocking their interfaces if available/used, creating test doubles, or potentially mocking the underlying `IStripeClient` they use if possible. *This is the most complex part.* Alternatively, mock the specific methods directly if using a mocking framework that supports it (like Moq's `Protected()` for non-virtual methods, though this is less common).
    * Mocking static methods like `WebhookUtility.ConstructEvent` requires specific techniques (e.g., wrapping it in a testable instance service or using frameworks that support static mocking, which is generally discouraged). A common approach is to have `StripeService` inject an interface wrapper around `WebhookUtility`.
    * `Mock<IOptions<StripeSettings>>`: Provide mock `StripeSettings` (API keys are usually configured globally for Stripe.net or passed to clients, webhook secret needed).
    * `Mock<ILogger<StripeService>>`.

## 4. Maintenance Notes & Troubleshooting

* **Stripe SDK Mocking:** Mocking `Stripe.net` can be challenging. Focus on mocking the specific service methods used (`CreateAsync`, `GetAsync`, etc.). Ensure mocks return realistic `Stripe` entity objects (`Session`, `Customer`, `Event`, etc.) or throw appropriate `StripeException` types for failure scenarios. Refer to `Stripe.net` documentation.
* **WebhookUtility Mocking:** Mocking static methods is hard. The recommended pattern is to create an instance wrapper interface/class (e.g., `IWebhookVerifier`) that calls the static method internally, and then mock the wrapper interface in your tests.
* **Configuration:** Ensure mocked `StripeSettings` provide necessary values like the webhook signing secret.
* **SDK Updates:** Updates to `Stripe.net` might introduce breaking changes requiring test updates.

## 5. Test Cases & TODOs

### `StripeServiceTests.cs`
* **TODO (`CreateStripeCheckoutSessionAsync`):** Test correct `SessionCreateOptions` built based on input.
* **TODO (`CreateStripeCheckoutSessionAsync`):** Test `SessionService.CreateAsync` called with correct options.
* **TODO (`CreateStripeCheckoutSessionAsync`):** Test handling successful `Session` response from mock.
* **TODO (`CreateStripeCheckoutSessionAsync`):** Test handling `StripeException` from mock `SessionService`.
* **TODO (`HandleStripeWebhookAsync`):** Test `WebhookUtility.ConstructEvent` called with correct args (requires wrapper/mocking strategy).
* **TODO (`HandleStripeWebhookAsync`):** Test handling successful `Event` construction from mock utility.
* **TODO (`HandleStripeWebhookAsync`):** Test processing different `event.Type` values.
* **TODO (`HandleStripeWebhookAsync`):** Test handling `StripeException` (e.g., invalid signature) from mock utility.
* **TODO (`CreateStripePortalSessionAsync`):** Test correct `SessionCreateOptions` built.
* **TODO (`CreateStripePortalSessionAsync`):** Test `BillingPortal.SessionService.CreateAsync` called.
* **TODO (`CreateStripePortalSessionAsync`):** Test handling successful `BillingPortal.Session` response.
* **TODO (`CreateStripePortalSessionAsync`):** Test handling `StripeException`.
* **TODO:** *(Add tests for other methods like creating customers, getting subscriptions, etc.)*

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `StripeService` unit tests. (Gemini)

