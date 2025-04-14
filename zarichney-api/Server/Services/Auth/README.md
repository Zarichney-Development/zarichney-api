# Module/Directory: Server/Auth

**Last Updated:** 2025-04-14

> **Parent:** [`Server`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory contains the core authentication and authorization logic for the Zarichney API, managing users, roles, API keys, and session tokens. [cite: zarichney-api/Docs/AuthSystemMaintenance.md, zarichney-api/Server/Controllers/AuthController.cs]
* **Key Responsibilities:**
    * User registration, login, email confirmation, and password reset flows. [cite: zarichney-api/Server/Auth/Commands/RegisterCommand.cs, zarichney-api/Server/Auth/Commands/LoginCommand.cs, zarichney-api/Server/Auth/Commands/ConfirmEmailCommand.cs, zarichney-api/Server/Auth/Commands/ForgotPasswordCommand.cs, zarichney-api/Server/Auth/Commands/ResetPasswordCommand.cs]
    * JWT access token generation and validation. [cite: zarichney-api/Server/Auth/AuthService.cs]
    * Refresh token generation, storage (database), validation, usage tracking, and revocation. [cite: zarichney-api/Server/Auth/AuthService.cs, zarichney-api/Server/Auth/Models/RefreshToken.cs, zarichney-api/Server/Auth/Commands/RefreshTokenCommand.cs, zarichney-api/Server/Auth/Commands/RevokeTokenCommand.cs]
    * Management of JWT and refresh tokens via secure, HttpOnly cookies. [cite: zarichney-api/Server/Auth/CookieAuthManager.cs, zarichney-api/Docs/AuthRefactoring.md]
    * API Key generation, validation, and revocation for non-interactive authentication. [cite: zarichney-api/Server/Auth/ApiKeyService.cs, zarichney-api/Server/Auth/Commands/ApiKeyCommands.cs]
    * Role definition (e.g., "admin") and management (assigning/removing roles). [cite: zarichney-api/Server/Auth/RoleManager.cs, zarichney-api/Server/Auth/Commands/RoleCommands.cs]
    * Database schema management for identity entities using EF Core. [cite: zarichney-api/Server/Auth/UserDbContext.cs, zarichney-api/Server/Auth/Migrations/]
    * Middleware for API Key and general authentication. [cite: zarichney-api/Server/Services/Auth/AuthenticationMiddleware.cs]
* **Why it exists:** To centralize all authentication and authorization concerns, separating them from other application logic like cookbook or payment processing. Provides a secure and maintainable way to manage user identity and access control. [cite: zarichney-api/Docs/AuthRefactoring.md]
* **Submodules:**
    * [`Commands`](./Commands/README.md) - Contains MediatR command handlers for auth operations (Register, Login, etc.).
    * [`Models`](./Models/README.md) - Defines data transfer objects and database entities related to authentication (ApiKey, RefreshToken, AuthResult).
    * [`Migrations`](./Migrations/README.md) - Holds Entity Framework Core database migration files for the UserDbContext.

## 2. Architecture & Key Concepts

* **Core Technology:** ASP.NET Core Identity provides the foundation for user management (users, roles, password hashing, claims). [cite: zarichney-api/Server/Auth/AuthConfigurations.cs, zarichney-api/Server/Auth/UserDbContext.cs]
* **Database:** PostgreSQL via Entity Framework Core (`UserDbContext`) stores user accounts, roles, refresh tokens, and API keys. [cite: zarichney-api/Server/Auth/UserDbContext.cs, zarichney-api/Server/Auth/Models/RefreshToken.cs, zarichney-api/Server/Auth/Models/ApiKey.cs]
* **Authentication Mechanisms:**
    * **JWT Bearer Tokens:** Standard mechanism using short-lived access tokens. [cite: zarichney-api/Server/Auth/AuthConfigurations.cs]
    * **Refresh Tokens:** Long-lived tokens stored in the database, used to obtain new access tokens. [cite: zarichney-api/Server/Auth/Models/RefreshToken.cs, zarichney-api/Server/Auth/AuthService.cs]
    * **API Keys:** For server-to-server or non-interactive clients via `X-Api-Key` header. [cite: zarichney-api/Server/Services/Auth/AuthenticationMiddleware.cs, zarichney-api/Server/Auth/ApiKeyService.cs]
* **Token Transport:** JWT access tokens and refresh tokens are primarily handled via secure, HttpOnly cookies (`AuthAccessToken`, `AuthRefreshToken`) managed by `CookieAuthManager`. [cite: zarichney-api/Server/Auth/CookieAuthManager.cs, zarichney-api/Docs/AuthRefactoring.md]
* **Design Pattern:** CQRS (Command Query Responsibility Segregation) implemented using MediatR. Authentication actions are handled by specific command handlers in the `Commands` directory. [cite: zarichney-api/Server/Auth/Commands/, zarichney-api/Docs/AuthRefactoring.md]
* **Key Classes:**
    * `AuthController`: Exposes HTTP endpoints for authentication actions. [cite: zarichney-api/Server/Controllers/AuthController.cs]
    * `AuthService`: Provides core token generation and refresh token DB operations. [cite: zarichney-api/Server/Auth/AuthService.cs]
    * `ApiKeyService`: Manages API key lifecycle, validation, and authentication. [cite: zarichney-api/Server/Auth/ApiKeyService.cs]
    * `CookieAuthManager`: Handles setting/clearing authentication cookies. [cite: zarichney-api/Server/Auth/CookieAuthManager.cs]
    * `RoleManager`: Manages application roles. [cite: zarichney-api/Server/Auth/RoleManager.cs]
    * `UserDbContext`: EF Core database context for Identity entities. [cite: zarichney-api/Server/Auth/UserDbContext.cs]
    * `AuthenticationMiddleware`: Middleware to handle authentication via `X-Api-Key` header or JWT. Respects endpoints marked with the `[AllowAnonymous]` attribute and skips authentication for them. [cite: zarichney-api/Server/Services/Auth/AuthenticationMiddleware.cs]
    * `RefreshTokenCleanupService`: Background service to remove expired/used tokens. [cite: zarichney-api/Server/Auth/RefreshTokenCleanupService.cs]
* **Core Logic Flow (Login):**
    1.  `AuthController.Login` receives email/password. [cite: zarichney-api/Server/Controllers/AuthController.cs]
    2.  Delegates to `LoginCommandHandler` (MediatR). [cite: zarichney-api/Server/Auth/Commands/LoginCommand.cs]
    3.  Handler uses `UserManager` to validate credentials and check email confirmation.
    4.  If valid, calls `AuthService.GenerateJwtTokenAsync` and `AuthService.GenerateRefreshToken`.
    5.  Saves refresh token via `AuthService.SaveRefreshTokenAsync`.
    6.  Handler returns `AuthResult` with tokens.
    7.  `AuthController` calls `CookieAuthManager.SetAuthCookies` to store tokens in response cookies. [cite: zarichney-api/Server/Controllers/AuthController.cs, zarichney-api/Server/Auth/CookieAuthManager.cs]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):** Primarily the HTTP endpoints defined in `AuthController` (e.g., `/register`, `/login`, `/refresh`, `/api-keys`). Secondary interfaces include `IApiKeyService` and `IRoleManager` if consumed directly by other internal services (though most interaction is via `AuthController`).
    * `POST /api/auth/login`:
        * **Purpose:** Authenticate user, set auth cookies.
        * **Preconditions:** User must exist and have a confirmed email address. Request body must contain valid email/password.
        * **Postconditions:** On success, HttpOnly `AuthAccessToken` and `AuthRefreshToken` cookies are set in the response. Refresh token is stored in the database.
        * **Error Handling:** Returns 400 Bad Request for invalid credentials, unconfirmed email, or missing input.
    * `POST /api/auth/refresh`:
        * **Purpose:** Obtain new access/refresh tokens using existing valid refresh token cookie.
        * **Preconditions:** Valid, non-expired, non-used, non-revoked `AuthRefreshToken` cookie must be present in the request. Associated user must exist.
        * **Postconditions:** On success, new HttpOnly `AuthAccessToken` and `AuthRefreshToken` cookies are set. Old refresh token is marked as used in DB, new one is stored.
        * **Error Handling:** Returns 401 Unauthorized if refresh token cookie is missing, invalid, expired, used, or revoked. Auth cookies are cleared on failure.
    * `POST /api/auth/api-keys` (Admin Only):
        * **Purpose:** Create an API key for the calling admin user.
        * **Preconditions:** Caller must be authenticated via JWT and have the 'admin' role. Request body must contain required fields (`Name`).
        * **Postconditions:** New API key record created in the database. The API key *value* is returned in the response (only time it's shown).
        * **Error Handling:** Returns 401/403 if not authenticated/authorized. Returns 400 for invalid input.
    * Any endpoint using `[Authorize]`:
        * **Purpose:** Secure access to resources/actions.
        * **Preconditions:** Request must include a valid, non-expired `AuthAccessToken` cookie OR a valid, active `X-Api-Key` header.
        * **Postconditions:** User identity (ID, roles, etc.) is populated in `HttpContext.User`.
        * **Error Handling:** Returns 401 Unauthorized if no valid token/key is provided or if the token/key is invalid/expired. Returns 403 Forbidden if authenticated but lacks required roles (`[Authorize(Roles = "...")]`).
    * Any endpoint using `[AllowAnonymous]`:
        * **Purpose:** Allow access without authentication.
        * **Preconditions:** None. Endpoint can be accessed without any authentication.
        * **Postconditions:** No user identity is required. The authentication middleware will skip validation for these endpoints.
        * **Error Handling:** Standard response based on the controller/action logic, not related to authentication.
* **Critical Assumptions:**
    * **External Systems/Config:** Assumes PostgreSQL database is available and connection string (`IdentityConnection`) is correct. Assumes `JwtSettings` (SecretKey, Issuer, Audience, Expiry) are securely configured and consistent across instances. Assumes `IEmailService` is configured and functional for registration/password reset flows.
    * **Data Integrity:** Assumes `UserDbContext` schema matches the entities defined in `Models/`. Relies on EF Core Identity for password hashing and token generation integrity. Assumes uniqueness constraints (Email, API Key value) are enforced by the database.
    * **Implicit Constraints:** Assumes client browsers correctly handle HttpOnly, Secure, SameSite=Lax cookies. Assumes clock synchronization between server instances for JWT validation (minimal skew allowed by default). `RefreshTokenCleanupService` relies on `BackgroundService` infrastructure.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:** Requires `ConnectionStrings:IdentityConnection` and `JwtSettings` section in configuration (`appsettings.json`, user secrets, environment variables, AWS SSM/Secrets Manager). [cite: zarichney-api/Server/Auth/AuthConfigurations.cs]
* **Directory Structure:** Commands in `Commands/`, DB context/entities/DTOs in `Models/`, Migrations in `Migrations/`.
* **Technology Choices:** ASP.NET Core Identity, EF Core with PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`), MediatR for CQRS, JWT for access tokens.
* **Security Notes:** JWT Secret Key is critical; must be kept confidential. Refresh tokens are stored in the DB but should be treated as sensitive. API Keys are stored hashed/validated securely. Password reset and email confirmation tokens have limited validity. `setup-admin` endpoint is potentially insecure and should be reviewed/removed post-setup. [cite: zarichney-api/Server/Controllers/AuthController.cs] HttpOnly cookies mitigate XSS risk for tokens.
* **Database Migrations:** Managed via EF Core CLI (`dotnet ef migrations add`, `dotnet ef database update`). Production migrations applied via SQL script generated by pipeline. [cite: zarichney-api/Server/Auth/Migrations/ApplyMigrations.sh, zarichney-api/Docs/PostgreSqlMaintenance.md]

## 5. How to Work With This Code

* **Setup:** Requires a running PostgreSQL instance. Configure `ConnectionStrings:IdentityConnection` and `JwtSettings` (especially `SecretKey`) via user secrets or environment variables for local development. Run EF Core migrations (`dotnet ef database update --context UserDbContext`). [cite: zarichney-api/Docs/PostgreSqlMaintenance.md]
* **Testing:** Requires mocking `UserManager`, `RoleManager`, `IAuthService`, `IEmailService`, `UserDbContext` (or using an in-memory provider/test database). Test command handlers individually. Integration tests needed for full flows (register -> confirm -> login -> refresh). Test scripts available in `Scripts/` (`test-auth-endpoints.*`). [cite: zarichney-api/Scripts/test-auth-endpoints.ps1, zarichney-api/Scripts/test-auth-endpoints.sh, zarichney-api/Scripts/TestAuthEndpoints.cs]
* **Common Pitfalls / Gotchas:** JWT secret mismatch between generation and validation. Incorrect cookie settings (`HttpOnly`, `Secure`, `SameSite`). Database connection issues. Migration mismatches. Handling token expiry and refresh logic correctly on the client-side (triggering `/refresh` on 401). Ensuring background cleanup service (`RefreshTokenCleanupService`) is running. Forgetting to include `credentials: 'include'` in frontend fetch calls.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Server/Config`](../../Config/README.md) - Consumes `JwtSettings`, `ClientConfig`, `ServerConfig`.
    * [`Server/Services/Emails`](../Emails/README.md) - Used by commands for sending confirmation/password reset emails.
* **External Library Dependencies:**
    * `Microsoft.AspNetCore.Identity.EntityFrameworkCore`: Core Identity framework.
    * `Microsoft.AspNetCore.Authentication.JwtBearer`: JWT validation middleware.
    * `Npgsql.EntityFrameworkCore.PostgreSQL`: EF Core provider for PostgreSQL.
    * `MediatR`: CQRS implementation.
    * `System.IdentityModel.Tokens.Jwt`: JWT generation.
* **Dependents (Impact of Changes):**
    * `Server/Controllers/` (Any using `[Authorize]`): Rely on this module's authentication middleware and claims principal setup.
    * `Program.cs`: Configures Identity, JWT authentication, API Key middleware, registers auth services.
    * `Server/Services/Sessions/SessionMiddleware.cs`: Uses `HttpContext.User` populated by this module's authentication.
    * `Server/Services/Auth/AuthenticationMiddleware.cs`: Relies on `IApiKeyService`.

## 7. Rationale & Key Historical Context

* **CQRS with MediatR:** Chosen to break down the monolithic `AuthService` into smaller, more manageable, testable command handlers, improving maintainability. [cite: zarichney-api/Docs/AuthRefactoring.md]
* **HttpOnly Cookies:** Implemented to enhance security against Cross-Site Scripting (XSS) attacks by preventing client-side JavaScript from accessing tokens. [cite: zarichney-api/Docs/AuthRefactoring.md]
* **Database-Stored Refresh Tokens:** Provides persistence and allows for features like revocation and tracking usage across devices. Enables longer user sessions without compromising access token security. [cite: zarichney-api/Docs/AuthRefactoring.md, zarichney-api/Server/Auth/Models/RefreshToken.cs]
* **API Keys:** Added to support non-interactive authentication scenarios (e.g., automated scripts, server-to-server). [cite: zarichney-api/Docs/ApiKeyAuthentication.md]
* **ASP.NET Core Identity:** Leveraged for its robust, built-in features for user management, password hashing, role management, and token providers (email confirmation, password reset).
* **[AllowAnonymous] Attribute Recognition:** Replaced the hardcoded route bypass list with standard ASP.NET Core `[AllowAnonymous]` attribute checking to improve maintainability and ensure all endpoints marked for anonymous access work correctly without API key validation.
* **Centralized Authentication Middleware:** Renamed from `ApiKeyAuthMiddleware` to `AuthenticationMiddleware` to better reflect its general authentication role. API key authentication logic was moved into the `ApiKeyService` class for better separation of concerns and maintainability.

## 8. Known Issues & TODOs

* Refresh token cleanup depends on the `RefreshTokenCleanupService` background task running reliably. Failures could lead to DB bloat.
* Consider adding Multi-Factor Authentication (MFA) support.
* Consider adding social login provider integration (Google, etc.).
* API key management currently relies on JWT auth for the admin; consider if API key auth should also be allowed for admins to manage keys.
