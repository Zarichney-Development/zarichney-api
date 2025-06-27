# Module/Directory: /Integration/Controllers/AuthController

**Last Updated:** 2025-04-18

> **Parent:** [`Controllers`](../README.md)
> **Related:**
> * **Source:** [`AuthController.cs`](../../../../Zarichney.Server/Controllers/AuthController.cs)
> * **Commands:** [`Services/Auth/Commands/`](../../../../Zarichney.Server/Services/Auth/Commands/)
> * **Services:** [`AuthService.cs`](../../../../Zarichney.Server/Services/Auth/AuthService.cs), [`CookieAuthManager.cs`](../../../../Zarichney.Server/Services/Auth/CookieAuthManager.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs), [`DatabaseFixture.cs`](../../../Framework/Fixtures/DatabaseFixture.cs), [`AuthTestHelper.cs`](../../../Framework/Helpers/AuthTestHelper.cs)

## 1. Purpose & Rationale (Why?)

This directory contains integration tests for the `AuthController` endpoints, which handle user registration, login, session management, password recovery, API key management, and role administration.

* **Why Integration Tests?** To verify that these critical authentication and authorization flows work correctly end-to-end within the ASP.NET Core pipeline. This includes request routing, model binding, validation, interaction with authentication middleware, correct dispatching of `MediatR` commands, handling of command results (`AuthResult`), interaction with `CookieAuthManager`, and generation of appropriate HTTP responses (status codes, JWTs, cookies, error details).
* **Why Mock Dependencies?** While some tests might use a real test database (`DatabaseFixture`) for deeper integration, key external interactions (like email sending via `IEmailService`) and potentially the direct outcome of `MediatR` commands or `CookieAuthManager` actions can be mocked within the `CustomWebApplicationFactory` to isolate specific behaviors and ensure test stability.

## 2. Scope & Key Functionality Tested (What?)

These tests cover the core authentication and user management endpoints:

* **`/register`:** User registration process, input validation, handling success/failure (e.g., email already exists).
* **`/login`:** User login with credentials, handling success (JWT/cookie generation) and failure (invalid credentials, locked out).
* **`/refresh`:** Refreshing authentication tokens using a refresh token (often passed via cookie).
* **`/revoke`:** Revoking refresh tokens (requires authentication).
* **`/forgot-password`:** Initiating the password reset flow (input validation, interaction with email service mock).
* **`/reset-password`:** Completing the password reset flow with a token (input validation, handling success/failure).
* **`/confirm-email`:** Endpoint linked from verification email (tested via `ApiController`).
* **`/resend-confirmation`:** Resending the email verification link (requires authentication).
* **`/api-keys` (various methods):** Creating, listing, revoking API keys (requires authentication, potentially specific roles).
* **`/roles` (various methods):** Assigning/removing user roles (requires authentication, specific Admin role).

## 3. Test Environment Setup

* **Test Server:** Provided by `CustomWebApplicationFactory<Program>`.
* **Database:** May utilize a test database container managed by `DatabaseFixture` for tests verifying interactions with user data, or rely on mocked MediatR results for simpler cases. Test classes may inherit from `DatabaseIntegrationTestBase` if using the fixture.
* **Authentication:** Simulated using `TestAuthHandler` and `AuthTestHelper` for endpoints requiring prior authentication (`/revoke`, `/resend-confirmation`, `/api-keys`, `/roles`).
* **Mocked Dependencies:** Configured in `CustomWebApplicationFactory`. Key mocks often include:
    * `MediatR.IMediator` (to control the `AuthResult` returned by commands without executing handlers).
    * `Zarichnyi.ApiServer.Services.Auth.ICookieAuthManager` (to verify cookie setting/clearing).
    * `Zarichnyi.ApiServer.Services.Email.IEmailService` (to prevent actual email sending).
    * Potentially `UserManager`, `SignInManager`, `RoleManager` if not mocking MediatR results fully.

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Tests are ideally organized by endpoint or feature within this directory (e.g., `LoginEndpointTests.cs`, `RegisterEndpointTests.cs`, `ApiKeyManagementTests.cs`).
* **MediatR Mocking:** Mocking `IMediator.Send` is crucial for controlling test outcomes without executing the full command handler logic + database interaction. Ensure the mocked `AuthResult` accurately reflects the scenario being tested (success, specific errors).
* **Cookie Verification:** Use `HttpClient` response inspection (`response.Headers.GetValues("Set-Cookie")`) to verify cookies (like refresh tokens) are set or cleared correctly.
* **Database State:** If using `DatabaseFixture`, ensure tests clean up after themselves or use transactions to maintain isolation between tests interacting with the database.
* **Auth Context:** For protected endpoints, ensure `AuthTestHelper` creates clients with the necessary authentication state and claims/roles.

## 5. Test Cases & TODOs

*(Note: This is a high-level list; each endpoint needs multiple detailed tests)*

### `/register` (`RegisterEndpointTests.cs`)
* **TODO:** Test successful registration -> verify 200 OK, potentially mock email service call.
* **TODO:** Test registration with existing email -> verify 400 Bad Request with specific error message (mock MediatR failure).
* **TODO:** Test registration with invalid input (e.g., weak password, invalid email format) -> verify 400 Bad Request.
* **TODO:** Test registration when underlying command fails unexpectedly -> verify 500 Internal Server Error.

### `/login` (`LoginEndpointTests.cs`)
* **TODO:** Test successful login with valid credentials -> verify 200 OK, presence of JWT in response body, and refresh token cookie set.
* **TODO:** Test login with invalid password -> verify 401 Unauthorized (mock MediatR failure).
* **TODO:** Test login with non-existent user -> verify 401 Unauthorized (mock MediatR failure).
* **TODO:** Test login with locked-out user -> verify 401/403 Forbidden (mock MediatR failure).
* **TODO:** Test login with unconfirmed email (if required) -> verify 401/403 Forbidden (mock MediatR failure).
* **TODO:** Test login with invalid request body -> verify 400 Bad Request.

### `/refresh` (`RefreshEndpointTests.cs`)
* **TODO:** Test successful refresh with valid refresh token cookie -> verify 200 OK, new JWT, new refresh token cookie.
* **TODO:** Test refresh with missing/invalid refresh token cookie -> verify 401 Unauthorized.
* **TODO:** Test refresh with revoked refresh token -> verify 401 Unauthorized (requires DB interaction or specific MediatR mock).

### `/revoke` (`RevokeEndpointTests.cs`)
* **TODO:** Test successful revoke for authenticated user (using refresh token from cookie or specific token if provided) -> verify 200 OK, verify refresh token cookie is cleared.
* **TODO:** Test revoke without authentication -> verify 401 Unauthorized.
* **TODO:** Test revoke with invalid/missing token -> verify 400/401.

### `/forgot-password` (`ForgotPasswordEndpointTests.cs`)
* **TODO:** Test with valid email for existing user -> verify 200 OK, mock email service call verification.
* **TODO:** Test with email for non-existent user -> verify 200 OK (to prevent user enumeration), but no email mock call.
* **TODO:** Test with invalid email format -> verify 400 Bad Request.

### `/reset-password` (`ResetPasswordEndpointTests.cs`)
* **TODO:** Test with valid email, token, and new password -> verify 200 OK (mock MediatR success).
* **TODO:** Test with invalid/expired token -> verify 400 Bad Request (mock MediatR failure).
* **TODO:** Test with non-existent email -> verify 400 Bad Request (mock MediatR failure).
* **TODO:** Test with weak new password -> verify 400 Bad Request.
* **TODO:** Test with missing parameters -> verify 400 Bad Request.

### `/resend-confirmation` (`ResendConfirmationEndpointTests.cs`)
* **TODO:** Test successful resend for authenticated user -> verify 200 OK, mock email service call.
* **TODO:** Test resend for user whose email is already confirmed -> verify 400 Bad Request (mock MediatR failure).
* **TODO:** Test resend without authentication -> verify 401 Unauthorized.

### `/api-keys` (`ApiKeyManagementTests.cs`)
* **TODO:** Test POST to create key (authenticated) -> verify 201 Created, response contains key.
* **TODO:** Test GET to list keys (authenticated) -> verify 200 OK, response contains user's keys.
* **TODO:** Test DELETE to revoke key (authenticated, owns key or Admin) -> verify 204 No Content.
* **TODO:** Test access denied scenarios (unauthenticated, insufficient permissions).

### `/roles` (`RoleManagementTests.cs`)
* **TODO:** Test POST to assign role (Admin) -> verify 200 OK.
* **TODO:** Test DELETE to remove role (Admin) -> verify 200 OK.
* **TODO:** Test access denied scenarios (unauthenticated, non-Admin).
* **TODO:** Test assigning non-existent role -> verify 400/404.
* **TODO:** Test assigning role to non-existent user -> verify 404.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `AuthController` integration tests. (Gemini)

