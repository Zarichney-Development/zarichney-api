# Module/Directory: /Unit/Services/Auth/AuthenticationMiddleware

**Last Updated:** 2025-04-18

> **Parent:** [`Auth`](../README.md)
> **Related:**
> * **Source:** [`Services/Auth/AuthenticationMiddleware.cs`](../../../../../Zarichney.Server/Services/Auth/AuthenticationMiddleware.cs)
> * **Dependencies:** `IApiKeyService`, `ICookieAuthManager`, `RequestDelegate`, `ILogger<AuthenticationMiddleware>`
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `AuthenticationMiddleware` class. This middleware is responsible for inspecting incoming requests for authentication credentials (API Keys via headers, authentication cookies) and attempting to establish the user's identity (`ClaimsPrincipal`) on the `HttpContext`.

* **Why Unit Tests?** To validate the core logic of the middleware's `InvokeAsync` method in isolation from the rest of the ASP.NET Core pipeline and actual authentication service implementations. Tests ensure the middleware correctly checks for credentials, calls the appropriate (mocked) authentication services (`IApiKeyService`, `ICookieAuthManager`), sets the `HttpContext.User` based on the outcome, and correctly calls the next middleware in the chain (`_next`).
* **Isolation:** Achieved by mocking `HttpContext` (including `Request.Headers`, `Request.Cookies`, `User`), the `RequestDelegate` (`_next`), `IApiKeyService`, `ICookieAuthManager`, and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `AuthenticationMiddleware` focus primarily on the `InvokeAsync` method:

* **Credential Detection:** Verifying the middleware correctly checks for the presence of the `X-Api-Key` header and relevant authentication cookies.
* **Service Invocation:** Ensuring the correct authentication service method (`IApiKeyService.AuthenticateWithApiKey` or `ICookieAuthManager.AuthenticateWithCookie`) is called based on the credentials found (and potentially a defined priority order if both are present).
* **`HttpContext.User` Population:** Verifying that `HttpContext.User` is correctly set to an authenticated `ClaimsPrincipal` when authentication succeeds (based on mocked service responses), and remains unauthenticated otherwise.
* **Calling `_next`:** Ensuring the `_next(context)` delegate is always called to pass control down the pipeline, regardless of authentication success or failure.
* **Error Handling:** Verifying how exceptions thrown by the mocked authentication services are handled (e.g., logged, swallowed).

## 3. Test Environment Setup

* **Instantiation:** `AuthenticationMiddleware` is instantiated directly in test methods, requiring a mock `RequestDelegate`.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<RequestDelegate>`: To verify `_next` is invoked.
    * `Mock<IApiKeyService>`: Mock `AuthenticateWithApiKey` to simulate success/failure.
    * `Mock<ICookieAuthManager>`: Mock `AuthenticateWithCookie` to simulate success/failure.
    * `Mock<ILogger<AuthenticationMiddleware>>`.
    * `Mock<HttpContext>`: Crucial for the `InvokeAsync` method. Requires setup for:
        * `Mock<HttpRequest>`: Setup `Headers` (to simulate `X-Api-Key`) and `Cookies`.
        * `Mock<HttpResponse>`.
        * `User` property (to assert the outcome).
        * `RequestServices` (if middleware resolves services directly, less common).

## 4. Maintenance Notes & Troubleshooting

* **HttpContext Mocking:** Setting up the mock `HttpContext` with its request properties (Headers, Cookies) is essential for triggering the middleware's logic paths.
* **Authentication Priority:** If the middleware has logic to prioritize one credential type over another (e.g., API key over cookie), ensure tests explicitly cover this priority rule.
* **`ClaimsPrincipal` Assertion:** Asserting that `HttpContext.User` is correctly populated involves checking its `Identity.IsAuthenticated` property and potentially specific claims if the mocked auth services return detailed principals.

## 5. Test Cases & TODOs

### `AuthenticationMiddlewareTests.cs` (`InvokeAsync` method)
* **TODO:** Test request with NO authentication details -> verify NO auth service methods called, verify `_next` called, verify `HttpContext.User.Identity.IsAuthenticated` is false.
* **TODO:** Test request with valid `X-Api-Key` header -> mock `IApiKeyService.AuthenticateWithApiKey` returns true/principal -> verify method called, verify `HttpContext.User` is authenticated, verify `_next` called.
* **TODO:** Test request with invalid/revoked `X-Api-Key` header -> mock `IApiKeyService.AuthenticateWithApiKey` returns false/null -> verify method called, verify `HttpContext.User` is unauthenticated, verify `_next` called.
* **TODO:** Test request with valid auth cookie -> mock `ICookieAuthManager.AuthenticateWithCookie` returns true/principal -> verify method called, verify `HttpContext.User` is authenticated, verify `_next` called.
* **TODO:** Test request with invalid/expired auth cookie -> mock `ICookieAuthManager.AuthenticateWithCookie` returns false/null -> verify method called, verify `HttpContext.User` is unauthenticated, verify `_next` called.
* **TODO:** Test request with BOTH valid API Key and valid Cookie -> verify ONLY the prioritized service method is called (e.g., `IApiKeyService`), verify `HttpContext.User` reflects the prioritized auth method, verify `_next` called.
* **TODO:** Test request when `IApiKeyService.AuthenticateWithApiKey` throws an exception -> verify exception caught/logged (via mock logger), verify `HttpContext.User` is unauthenticated, verify `_next` called.
* **TODO:** Test request when `ICookieAuthManager.AuthenticateWithCookie` throws an exception -> verify exception caught/logged, verify `HttpContext.User` is unauthenticated, verify `_next` called.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `AuthenticationMiddleware` unit tests. (Gemini)

