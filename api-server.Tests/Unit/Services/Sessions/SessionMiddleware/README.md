# Module/Directory: /Unit/Services/Sessions/SessionMiddleware

**Last Updated:** 2025-04-18

> **Parent:** [`Sessions`](../README.md)
> *(Note: A README for `/Unit/Services/Sessions/` may be needed)*
> **Related:**
> * **Source:** [`Services/Sessions/SessionMiddleware.cs`](../../../../../api-server/Services/Sessions/SessionMiddleware.cs)
> * **Dependencies:** `ISessionManager`, `RequestDelegate`, `ILogger<SessionMiddleware>`
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `SessionMiddleware` class. This middleware is responsible for establishing a session context for each incoming HTTP request, typically by interacting with the `ISessionManager`.

* **Why Unit Tests?** To validate the core logic of the middleware's `InvokeAsync` method in isolation from the rest of the ASP.NET Core pipeline and the actual `SessionManager` implementation. Tests ensure the middleware correctly:
    * Reads the `X-Session-Id` header from the request.
    * Calls `ISessionManager.GetOrCreateSessionAsync` with the appropriate ID (or null).
    * Stores the retrieved/created session object in `HttpContext.Items`.
    * Adds the `X-Session-Id` header to the response.
    * Calls the next middleware in the chain (`_next`).
* **Isolation:** Achieved by mocking `HttpContext` (including `Request.Headers`, `Response.Headers`, `Items`), the `RequestDelegate` (`_next`), `ISessionManager`, and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `SessionMiddleware` focus primarily on the `InvokeAsync` method:

* **Header Reading:** Verifying the middleware correctly attempts to read the `X-Session-Id` header from `HttpRequest.Headers`.
* **`ISessionManager` Invocation:** Ensuring `ISessionManager.GetOrCreateSessionAsync` is called with the correct session ID (or null if the header is missing/empty).
* **`HttpContext.Items` Population:** Verifying that the `Session` object returned by the mocked `ISessionManager` is correctly stored in `HttpContext.Items` using a defined key.
* **Response Header Writing:** Ensuring the `X-Session-Id` header is added to `HttpResponse.Headers` with the correct session ID value obtained from the session object.
* **Calling `_next`:** Ensuring the `_next(context)` delegate is always called to pass control down the pipeline.
* **Error Handling:** Verifying how exceptions thrown by the mocked `ISessionManager` are handled (e.g., logged, potentially allowed to propagate to error handling middleware).

## 3. Test Environment Setup

* **Instantiation:** `SessionMiddleware` is instantiated directly in test methods, requiring a mock `RequestDelegate`.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<RequestDelegate>`: To verify `_next` is invoked.
    * `Mock<ISessionManager>`: Mock `GetOrCreateSessionAsync` to return specific `Session` objects (with IDs) based on input ID.
    * `Mock<ILogger<SessionMiddleware>>`.
    * `Mock<HttpContext>`: Requires setup for:
        * `Mock<HttpRequest>`: Setup `Headers` collection (to simulate presence or absence of `X-Session-Id`).
        * `Mock<HttpResponse>`: Setup `Headers` collection (to verify `X-Session-Id` is added).
        * `Items` dictionary (to verify session object is stored).
        * `RequestServices` (if middleware resolves services directly).

## 4. Maintenance Notes & Troubleshooting

* **HttpContext Mocking:** Setting up the mock `HttpContext` with its request/response headers and Items dictionary is essential.
* **`ISessionManager` Mocking:** Ensure the mock for `GetOrCreateSessionAsync` returns a `Session` object containing a `SessionId` property, as the middleware relies on this to set the response header.
* **Header/Item Keys:** Ensure tests use the same keys for the header (`X-Session-Id`) and `HttpContext.Items` as the actual middleware implementation.

## 5. Test Cases & TODOs

### `SessionMiddlewareTests.cs` (`InvokeAsync` method)
* **TODO:** Test request *without* `X-Session-Id` header:
    * Verify `ISessionManager.GetOrCreateSessionAsync` called with null/empty ID.
    * Mock `ISessionManager` returns a *new* session object with an ID.
    * Verify `HttpContext.Items` contains the new session object.
    * Verify `HttpResponse.Headers` contains `X-Session-Id` with the *new* session ID.
    * Verify `_next` called.
* **TODO:** Test request *with* valid `X-Session-Id` header:
    * Verify `ISessionManager.GetOrCreateSessionAsync` called with the *correct* ID from the header.
    * Mock `ISessionManager` returns an *existing* session object with the *same* ID.
    * Verify `HttpContext.Items` contains the existing session object.
    * Verify `HttpResponse.Headers` contains `X-Session-Id` with the *same* session ID.
    * Verify `_next` called.
* **TODO:** Test request *with* `X-Session-Id` header, but `ISessionManager` finds no matching session (if `GetOrCreateSessionAsync` handles this by creating new):
    * Verify `ISessionManager.GetOrCreateSessionAsync` called with the ID from the header.
    * Mock `ISessionManager` returns a *new* session object with a *new* ID.
    * Verify `HttpContext.Items` contains the new session object.
    * Verify `HttpResponse.Headers` contains `X-Session-Id` with the *new* session ID.
    * Verify `_next` called.
* **TODO:** Test request when `ISessionManager.GetOrCreateSessionAsync` throws an exception:
    * Verify exception is handled/logged (via mock logger).
    * Verify `_next` is still called (or verify propagation if designed to fail).
    * Verify `HttpResponse.Headers` does *not* contain `X-Session-Id` (unless set before exception).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `SessionMiddleware` unit tests. (Gemini)

