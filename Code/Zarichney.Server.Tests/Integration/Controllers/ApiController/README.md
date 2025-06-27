# Module/Directory: /Integration/Controllers/ApiController

**Last Updated:** 2025-04-18

> **Parent:** [`Controllers`](../README.md)
> **Related:**
> * **Source:** [`ApiController.cs`](../../../../Zarichney.Server/Controllers/ApiController.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs)

## 1. Purpose & Rationale (Why?)

This directory contains integration tests specifically for the **concrete endpoints defined directly within the `ApiController` base class**. While `ApiController` also serves as a base for other controllers, these tests focus *only* on the routes handled by `ApiController` itself.

* **Why Integration Tests?** To ensure these specific endpoints (`ValidateEmail`, `HealthCheckSecure`, `TestAuth`) function correctly within the ASP.NET Core request pipeline, interacting properly with routing, middleware (Auth, Error Handling, etc.), model binding, and mocked dependencies relevant to their specific functions.
* **Why Mock Dependencies?** To isolate the endpoint logic from external systems (like actual email sending) or complex internal services (like the full user management stack when only specific interactions are needed), ensuring reliable and focused tests.

## 2. Scope & Key Functionality Tested (What?)

These tests cover the following endpoints defined in `ApiController`:

* **`ValidateEmail` (e.g., `GET /api/validate-email?userId=...&token=...`)**
    * Handling of request parameters (userId, token).
    * Interaction with mocked `UserManager` (or equivalent service) to simulate email confirmation logic.
    * Correct redirection or response generation based on validation success/failure.
    * Handling of invalid/missing parameters.
* **`HealthCheckSecure` (e.g., `GET /api/health-check-secure`)**
    * Enforcement of authentication and potentially specific authorization policies (e.g., Admin role).
    * Interaction with mocked status/dependency checking services (if applicable).
    * Correct success response (e.g., 200 OK with status message).
    * Correct failure responses (401 Unauthorized, 403 Forbidden).
* **`TestAuth` (e.g., `GET /api/test-auth`)**
    * Enforcement of authentication.
    * Verification of user identity/claims retrieval from the request context (`HttpContext.User`).
    * Correct response payload containing relevant authentication/user information.
    * Handling different authentication states (e.g., different roles, claims) if the endpoint logic varies.

## 3. Test Environment Setup

* **Test Server:** Provided by `CustomWebApplicationFactory<Program>`.
* **Authentication:** Simulated using `TestAuthHandler` and helpers from `AuthTestHelper`. Crucial for testing `HealthCheckSecure` and `TestAuth`.
* **Mocked Dependencies:** Key dependencies used by these specific endpoints must be mocked in `CustomWebApplicationFactory.ConfigureWebHost`. Likely mocks include:
    * `Microsoft.AspNetCore.Identity.UserManager<IdentityUser>` (or relevant auth service interface)
    * `Microsoft.AspNetCore.Identity.RoleManager<IdentityRole>` (if roles are checked)
    * `Zarichnyi.ApiServer.Services.Status.IStatusService` (if used by health check)
    * `Zarichnyi.ApiServer.Services.Email.IEmailService` (if validation triggers emails)

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Tests for each endpoint are organized into separate files within this directory (e.g., `ValidateEmailEndpointTests.cs`, `HealthCheckSecureEndpointTests.cs`, `TestAuthEndpointTests.cs`).
* **Mocking Auth:** When testing `ValidateEmail`, ensure the `UserManager` mock is configured to simulate `FindByIdAsync` and `ConfirmEmailAsync` results correctly for various scenarios (valid token, invalid token, user not found).
* **Mocking Status:** For `HealthCheckSecure`, ensure any dependent service mocks (like `IStatusService`) are configured if the endpoint relies on them.
* **Auth Testing:** For `HealthCheckSecure` and `TestAuth`, ensure the `TestAuthHandler` provides clients with the appropriate authentication state and claims/roles needed to test success (200 OK), unauthorized (401), and forbidden (403) paths.
* **Base Class Changes:** Remember that changes to attributes or filters in `ApiController` can still affect derived controllers. Ensure tests in derived controller directories also pass after modifications here.

## 5. Test Cases & TODOs

### `ValidateEmail` Endpoint (`ValidateEmailEndpointTests.cs`)
* **TODO:** Test GET with valid `userId` and `token`, mock `UserManager.FindByIdAsync` returns user, mock `UserManager.ConfirmEmailAsync` returns `IdentityResult.Success` -> verify success response/redirect.
* **TODO:** Test GET with valid `userId` but invalid/expired `token`, mock `UserManager.FindByIdAsync` returns user, mock `UserManager.ConfirmEmailAsync` returns `IdentityResult.Failed` -> verify failure response.
* **TODO:** Test GET with non-existent `userId`, mock `UserManager.FindByIdAsync` returns `null` -> verify failure response (e.g., NotFound or BadRequest).
* **TODO:** Test GET with missing `userId` parameter -> verify 400 Bad Request.
* **TODO:** Test GET with missing `token` parameter -> verify 400 Bad Request.
* **TODO:** Test GET with invalid `userId` format (if applicable) -> verify 400 Bad Request.

### `HealthCheckSecure` Endpoint (`HealthCheckSecureEndpointTests.cs`)
* **TODO:** Test GET with required authentication/role (e.g., Admin role via `AuthTestHelper`) -> verify 200 OK and expected success message/status.
* **TODO:** Test GET without authentication -> verify 401 Unauthorized.
* **TODO:** Test GET with authentication but *missing* required role/claim -> verify 403 Forbidden.
* **TODO:** Test GET response content when underlying checks pass (mock dependencies if any).
* **TODO:** Test GET response content/status code when underlying checks fail (mock dependencies to simulate failure).

### `TestAuth` Endpoint (`TestAuthEndpointTests.cs`)
* **TODO:** Test GET with a standard authenticated user -> verify 200 OK and response contains expected user claims (e.g., NameIdentifier, Email).
* **TODO:** Test GET without authentication -> verify 401 Unauthorized.
* **TODO:** Test GET with a user having specific roles (e.g., "Admin", "User") -> verify response reflects these roles if applicable.
* **TODO:** Test GET with a user authenticated via API Key -> verify response reflects API key identity if different.
* **TODO:** Test GET with a user authenticated via Cookie -> verify response reflects cookie identity if different.

## 6. Changelog

* **2025-04-18:** Revision 2 - Updated to reflect concrete endpoints (`ValidateEmail`, `HealthCheckSecure`, `TestAuth`) in `ApiController`, added specific TODOs, acknowledged per-endpoint test file structure. (Gemini)
* **2025-04-18:** Revision 1 - Initial creation (assuming base class only). (Gemini)

