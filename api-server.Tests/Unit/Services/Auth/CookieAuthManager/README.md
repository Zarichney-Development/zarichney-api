# Module/Directory: /Unit/Services/Auth/CookieAuthManager

**Last Updated:** 2025-04-18

> **Parent:** [`Auth`](../README.md)
> **Related:**
> * **Source:** [`Services/Auth/CookieAuthManager.cs`](../../../../../api-server/Services/Auth/CookieAuthManager.cs)
> * **Dependencies:** `SignInManager<IdentityUser>`, `UserManager<IdentityUser>` (potentially), `IHttpContextAccessor`, `ILogger<CookieAuthManager>`
> * **Standards:** [`TestingStandards.md`](../../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `CookieAuthManager` class. This service encapsulates the logic specifically related to managing user authentication sessions via HTTP cookies, primarily by interacting with ASP.NET Core Identity's `SignInManager`.

* **Why Unit Tests?** To validate the internal logic of `CookieAuthManager` methods (like `SignInAsync`, `SignOutAsync`) in isolation from the actual Identity framework implementation, HTTP pipeline, and database. Tests ensure the service correctly calls the underlying (mocked) `SignInManager` methods with appropriate parameters and interacts correctly with the (mocked) `HttpContext`.
* **Isolation:** Achieved by mocking `SignInManager<IdentityUser>`, potentially `UserManager<IdentityUser>`, `IHttpContextAccessor` (and its `HttpContext`), and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `CookieAuthManager` focus on its public methods:

* **`SignInAsync(IdentityUser user, bool isPersistent, ...)`:**
    * Verifying that the appropriate `SignInManager` method (e.g., `SignInAsync`, `Context.SignInAsync`) is called with the correct user principal, persistence flag, and authentication scheme.
    * Ensuring interaction with the mocked `IHttpContextAccessor` to get the current `HttpContext` needed for sign-in operations.
* **`SignOutAsync()`:**
    * Verifying that `SignInManager.SignOutAsync` (or `Context.SignOutAsync`) is called with the correct authentication scheme.
    * Ensuring interaction with the mocked `IHttpContextAccessor`.
* **`AuthenticateWithCookie()`:** (If this method exists here instead of/in addition to middleware)
    * Verifying interaction with `SignInManager` (e.g., `ValidateSecurityStampAsync` or retrieving the principal via `_signInManager.Context.AuthenticateAsync`).
    * Correctly returning a `ClaimsPrincipal` or null based on the mocked `SignInManager` outcome.

## 3. Test Environment Setup

* **Instantiation:** `CookieAuthManager` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<SignInManager<IdentityUser>>`: Mocking `SignInAsync`, `SignOutAsync`, `ValidateSecurityStampAsync`, `Context.SignInAsync`, `Context.SignOutAsync`, `Context.AuthenticateAsync` as needed. Requires mocking its own dependencies (`UserManager`, `IHttpContextAccessor`, `IUserClaimsPrincipalFactory`, etc.) during setup.
    * `Mock<UserManager<IdentityUser>>`: Needed if `CookieAuthManager` interacts with it directly or for `SignInManager` mock setup. Requires mocking `IUserStore`.
    * `Mock<IHttpContextAccessor>`: Needs to be set up to return a `Mock<HttpContext>`.
    * `Mock<HttpContext>`: The `RequestServices` property might need mocking if `SignInManager` uses it. The `SignInAsync`/`SignOutAsync` extension methods on `HttpContext` might be called directly.
    * `Mock<ILogger<CookieAuthManager>>`.

## 4. Maintenance Notes & Troubleshooting

* **`SignInManager` Mocking:** This is complex due to `SignInManager`'s numerous dependencies. Focus on mocking only the methods directly called by `CookieAuthManager`. Ensure the mock setup for `SignInManager`'s dependencies (`UserManager`, `IHttpContextAccessor`, etc.) is correct.
* **`HttpContext` Interaction:** Tests need to verify how `CookieAuthManager` accesses and uses the `HttpContext` provided by the mocked `IHttpContextAccessor`. Mocking the `HttpContext.SignInAsync` and `SignOutAsync` extension methods might be necessary depending on the implementation.

## 5. Test Cases & TODOs

### `CookieAuthManagerTests.cs`

* **TODO (`SignInAsync`):** Test success path -> verify `SignInManager.SignInAsync` (or `Context.SignInAsync`) called with correct user principal, persistence flag, and scheme.
* **TODO (`SignInAsync`):** Test when `SignInManager.SignInAsync` fails or throws an exception -> verify behavior.
* **TODO (`SignOutAsync`):** Test success path -> verify `SignInManager.SignOutAsync` (or `Context.SignOutAsync`) called with correct scheme.
* **TODO (`SignOutAsync`):** Test when `SignInManager.SignOutAsync` throws an exception -> verify behavior.
* **TODO (`AuthenticateWithCookie` - if exists):** Test valid cookie scenario -> mock `SignInManager` validation success -> verify correct `ClaimsPrincipal` returned.
* **TODO (`AuthenticateWithCookie` - if exists):** Test invalid/expired cookie scenario -> mock `SignInManager` validation failure -> verify null or unauthenticated principal returned.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `CookieAuthManager` unit tests. (Gemini)

