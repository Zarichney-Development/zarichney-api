# Module/Directory: /Integration/Middleware

**Last Updated:** 2025-04-18

> **Parent:** [`Integration`](../README.md)
> **Related:**
> * **Middleware Sources:**
>   * [`Config/ErrorHandlingMiddleware.cs`](../../../api-server/Config/ErrorHandlingMiddleware.cs)
>   * [`Config/LoggingMiddleware.cs`](../../../api-server/Config/LoggingMiddleware.cs)
>   * [`Services/Auth/AuthenticationMiddleware.cs`](../../../api-server/Services/Auth/AuthenticationMiddleware.cs)
>   * [`Services/Sessions/SessionMiddleware.cs`](../../../api-server/Services/Sessions/SessionMiddleware.cs)
> * **Startup:** [`Startup/MiddlewareStartup.cs`](../../../api-server/Startup/MiddlewareStartup.cs)
> * **Standards:** [`TestingStandards.md`](../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../Docs/Development/DocumentationStandards.md)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../Framework/Fixtures/CustomWebApplicationFactory.cs)

## 1. Purpose & Rationale (Why?)

This directory contains integration tests specifically designed to validate the behavior of custom ASP.NET Core middleware components used within the `api-server` request pipeline.

* **Why Integration Tests for Middleware?** Middleware components operate directly within the request/response flow. Integration tests are essential to verify they:
    * Are correctly registered and ordered in the pipeline.
    * Intercept requests and responses as intended.
    * Perform their specific cross-cutting concerns (error handling, logging, session management, authentication context setup) correctly.
    * Interact properly with their own dependencies (e.g., loggers, services).
    * Correctly invoke the next middleware component (`_next(context)`).
    * Handle exceptions gracefully (both internal and from downstream).
* **Why Mock Dependencies?** Services injected into middleware constructors (e.g., `ILogger`, `ISessionManager`, `IApiKeyService`, `ICookieAuthManager`) are mocked within the `CustomWebApplicationFactory` to isolate the middleware logic and provide controlled behavior during tests.

## 2. Scope & Key Functionality Tested (What?)

These tests focus on the observable effects of the custom middleware components:

* **`ErrorHandlingMiddleware`:**
    * Catching various exception types thrown by downstream middleware or endpoints.
    * Logging appropriate error details (verified via mock `ILogger`).
    * Generating standardized JSON error responses (`ApiErrorResult`) with correct status codes based on exception type.
    * Allowing requests to pass through when no exceptions occur.
* **`LoggingMiddleware`:**
    * Logging request start/end information, including path, method, status code, and duration (verified via mock `ILogger`).
* **`SessionMiddleware`:**
    * Identifying requests with/without the `X-Session-Id` header.
    * Calling `ISessionManager.GetOrCreateSessionAsync` with correct parameters.
    * Adding the `X-Session-Id` header to outgoing responses.
    * Populating `HttpContext.Items` with the session context.
* **`AuthenticationMiddleware`:** (Note: Often implicitly tested via controller tests, but dedicated tests can focus on specific scenarios)
    * Populating `HttpContext.User` based on valid API keys found in headers.
    * Populating `HttpContext.User` based on valid authentication cookies.
    * Handling requests with invalid or missing authentication details (leaving `HttpContext.User` unauthenticated).

## 3. Test Environment Setup

* **Test Server:** Provided by `CustomWebApplicationFactory<Program>`, ensuring the middleware is registered in the test pipeline as it is in production (via `MiddlewareStartup`).
* **Test Endpoints:** Tests often define minimal, dedicated endpoints within the test setup (`WebApplicationFactory` configuration) specifically designed to trigger or facilitate verification of middleware behavior. For example:
    * An endpoint that intentionally throws different exception types to test `ErrorHandlingMiddleware`.
    * A simple endpoint returning 200 OK to test `LoggingMiddleware` or `SessionMiddleware` pass-through.
* **Mocked Dependencies:** Configured in `CustomWebApplicationFactory`. Key mocks include:
    * `Microsoft.Extensions.Logging.ILogger` (for relevant middleware types)
    * `Zarichnyi.ApiServer.Services.Sessions.ISessionManager`
    * `Zarichnyi.ApiServer.Services.Auth.IApiKeyService`
    * `Zarichnyi.ApiServer.Services.Auth.ICookieAuthManager`

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Organize tests by the middleware component being tested (e.g., `ErrorHandlingMiddlewareTests.cs`, `SessionMiddlewareTests.cs`).
* **Middleware Order:** Integration tests implicitly test the configured middleware order. If order-dependent behavior is critical, ensure tests cover those scenarios. Changes in `MiddlewareStartup.cs` might break these tests.
* **Mocking Middleware Dependencies:** Ensure services injected into middleware constructors are correctly mocked in the factory. Verify interactions using `Mock.Verify`.
* **Triggering Middleware:** Craft HTTP requests carefully to exercise specific middleware logic (e.g., include/exclude headers like `X-Session-Id` or `X-Api-Key`, make requests to endpoints designed to throw exceptions).
* **Response Assertions:** Assertions often focus on the HTTP response status code, headers (like `X-Session-Id`), and potentially the response body (especially for `ErrorHandlingMiddleware`). Verifying logs requires inspecting the mocked `ILogger` instances.

## 5. Test Cases & TODOs

### `ErrorHandlingMiddlewareTests.cs`
* **TODO:** Test request to endpoint throwing `NotExpectedException` -> verify 500 status code, specific JSON error response structure, verify `ILogger.LogError` called.
* **TODO:** Test request to endpoint throwing `ConfigurationMissingException` -> verify 500 status code, specific JSON error response structure, verify `ILogger.LogCritical` called.
* **TODO:** Test request to endpoint throwing generic `System.Exception` -> verify 500 status code, generic JSON error response structure, verify `ILogger.LogError` called.
* **TODO:** Test request to endpoint throwing `HttpRequestException` (or other specific types if handled differently).
* **TODO:** Test request to endpoint that *does not* throw an exception -> verify original response (e.g., 200 OK) is passed through unmodified by this middleware.

### `LoggingMiddlewareTests.cs`
* **TODO:** Test request to simple endpoint (e.g., returning 200 OK) -> verify `ILogger.LogInformation` called with expected request start/end messages, method, path, status code, duration.
* **TODO:** Test request resulting in 4xx error -> verify logging reflects the 4xx status code.
* **TODO:** Test request resulting in 5xx error -> verify logging reflects the 5xx status code.

### `SessionMiddlewareTests.cs`
* **TODO:** Test request *without* `X-Session-Id` header -> verify `ISessionManager.GetOrCreateSessionAsync` called (likely with null ID), verify response includes `X-Session-Id` header with a new session ID.
* **TODO:** Test request *with* a valid `X-Session-Id` header -> verify `ISessionManager.GetOrCreateSessionAsync` called with the provided session ID, verify response includes the *same* `X-Session-Id` header.
* **TODO:** Test request *with* an *invalid* `X-Session-Id` header -> verify behavior (e.g., treated as missing, new session created, specific logging).
* **TODO:** Verify `HttpContext.Items` contains the session object after middleware execution.

### `AuthenticationMiddlewareTests.cs` (If testing directly)
* **TODO:** Test request with valid `X-Api-Key` header -> mock `IApiKeyService.AuthenticateWithApiKey` success -> verify downstream `HttpContext.User` is authenticated with correct claims.
* **TODO:** Test request with invalid/revoked `X-Api-Key` header -> mock `IApiKeyService.AuthenticateWithApiKey` failure -> verify downstream `HttpContext.User` is unauthenticated.
* **TODO:** Test request with valid auth cookie -> mock `ICookieAuthManager.AuthenticateWithCookie` success -> verify downstream `HttpContext.User` is authenticated.
* **TODO:** Test request with invalid/expired auth cookie -> mock `ICookieAuthManager.AuthenticateWithCookie` failure -> verify downstream `HttpContext.User` is unauthenticated.
* **TODO:** Test request with no auth details -> verify downstream `HttpContext.User` is unauthenticated.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for custom middleware integration tests. (Gemini)

