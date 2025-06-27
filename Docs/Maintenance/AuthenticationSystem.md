# Authentication System Maintenance Guide

**Version:** 1.1
**Last Updated:** 2025-04-13

This document provides guidelines for maintaining and troubleshooting the Zarichney API authentication system, including user management, token handling (JWT & Refresh Tokens), API Key management, email verification, and related configuration.

## 1. System Overview

The authentication system consists of:

1.  **User Management** - Registration, login, and profile management using ASP.NET Core Identity.
2.  **Token Management** - Handles session authentication using JWT access tokens and refresh tokens, primarily via HttpOnly cookies.
3.  **API Key Management** - Allows for non-interactive authentication using API keys associated with user accounts.
4.  **Email Verification** - Ensures users confirm their email address upon registration.
5.  **Password Reset** - Provides secure password reset functionality.
6.  **Role-Based Authorization** - Manages access control using roles (e.g., 'admin').

## 2. Authentication Flows

The API supports two primary authentication methods, with JWT taking precedence if both are provided:

### 2.1. JWT Authentication Flow (Cookie-Based)

1.  User logs in with email/password via `/api/auth/login` [cite: Zarichney.Server/Controllers/AuthController.cs].
2.  Server validates credentials and checks email confirmation [cite: Zarichney.Server/Services/Auth/Commands/LoginCommand.cs].
3.  Server generates a short-lived JWT access token and a longer-lived refresh token [cite: Zarichney.Server/Services/Auth/AuthService.cs].
4.  Server sets these tokens in secure, HttpOnly cookies (`AuthAccessToken`, `AuthRefreshToken`) in the response [cite: Zarichney.Server/Services/Auth/CookieAuthManager.cs, Docs/AuthRefactoring.md].
5.  The browser automatically includes these cookies in subsequent requests to the API.
6.  When the access token expires (detected via a 401 response), the client calls `/api/auth/refresh` [cite: Zarichney.Server/Controllers/AuthController.cs].
7.  The server validates the refresh token cookie, issues new tokens, and updates the cookies [cite: Zarichney.Server/Services/Auth/Commands/RefreshTokenCommand.cs, Zarichney.Server/Services/Auth/CookieAuthManager.cs].

### 2.2. API Key Authentication Flow

1.  An authenticated user (typically an admin) creates an API key via the `/api/auth/api-keys` endpoint [cite: Docs/ApiKeyAuthentication.md].
2.  The generated API key value (returned only once) is stored securely by the client/service [cite: Docs/ApiKeyAuthentication.md].
3.  For subsequent requests, the client includes the key in the `X-Api-Key` HTTP header [cite: Docs/ApiKeyAuthentication.md].
4.  The `AuthenticationMiddleware` intercepts the request, validates the key against the database using `IApiKeyService`, and checks its active status and expiry [cite: Zarichney.Server/Services/Auth/AuthenticationMiddleware.cs, Zarichney.Server/Services/Auth/ApiKeyService.cs].
5.  If valid, the middleware identifies the associated user and creates an authenticated `ClaimsPrincipal` for the request, allowing access similar to JWT authentication [cite: Docs/ApiKeyAuthentication.md].

### 2.3. Precedence Rules

If a request includes both JWT authentication (via cookies) and an API key (via header) [cite: Docs/ApiKeyAuthentication.md]:

1.  JWT authentication is validated first.
2.  If JWT is valid, the API key is ignored.
3.  If JWT is invalid or missing, API key authentication is attempted.

## 3. Configuration Settings

Authentication settings are defined in `appsettings.json` and rely on secure configuration sources (User Secrets, Environment Variables, AWS Secrets Manager/Parameter Store) for sensitive values [cite: Docs/AuthSystemMaintenance.md, Docs/PostgreSqlMaintenance.md].

```json
 {
   "AppUrl": "http://localhost:5173", // Base URL for client links
   "ConnectionStrings": {
     "IdentityConnection": "..." // PostgreSQL connection string
   },
   "JwtSettings": {
     "SecretKey": "...", // MUST be securely configured
     "Issuer": "ZarichneyApi",
     "Audience": "ZarichneyClients",
     "ExpiryMinutes": 60,
     "RefreshTokenExpiryDays": 30,
     "TokenCleanupIntervalHours": 24
   },
   "EmailConfig": {
     "FromEmail": "...", // Sending email address
     "AzureTenantId": "...", // MS Graph Credentials
     "AzureAppId": "...",
     "AzureAppSecret": "...",
     "MailCheckApiKey": "...", // Email validation API key
     "TemplateDirectory": "EmailTemplates"
   }
 }
```

### Key Configuration Recommendations

| Setting                           | Description                      | Recommendations                                      |
| :-------------------------------- | :------------------------------- | :--------------------------------------------------- |
| `JwtSettings:SecretKey`           | Secret for signing JWTs          | Use \>= 32 random chars; Store securely; Rotate      |
| `JwtSettings:ExpiryMinutes`       | Access token lifetime            | 15-60 minutes typical                                |
| `JwtSettings:RefreshTokenExpiryDays` | Refresh token lifetime           | 7-30 days typical; Balance security & convenience    |
| `ConnectionStrings:IdentityConnection` | DB connection string             | Use secure methods (Secrets Mgr/SSM) for password |
| `EmailConfig` credentials         | Azure App & MailCheck keys     | Store securely                                       |
| `AppUrl`                          | Client base URL for email links | Match frontend deployment URL                        |

## 4. Token Management

### 4.1. JWT Access Tokens

* **Purpose**: Short-lived tokens for API authentication [cite: Docs/AuthSystemMaintenance.md].
* **Storage**: Client-side only in secure, HttpOnly `AuthAccessToken` cookie [cite: Zarichney.Server/Services/Auth/CookieAuthManager.cs].
* **Expiration**: Configured via `JwtSettings:ExpiryMinutes` [cite: Docs/AuthSystemMaintenance.md].
* **Claims**: Includes User ID, Email, Roles, and standard JWT claims (sub, jti, exp, iss, aud) [cite: Zarichney.Server/Services/Auth/AuthService.cs].

### 4.2. Refresh Tokens

* **Purpose**: Long-lived tokens to obtain new access tokens without re-login [cite: Docs/AuthSystemMaintenance.md].
* **Storage**: Server-side in the `RefreshTokens` database table; reference token stored in HttpOnly `AuthRefreshToken` cookie [cite: Zarichney.Server/Services/Auth/Models/RefreshToken.cs, Zarichney.Server/Services/Auth/CookieAuthManager.cs].
* **Expiration**: Configured via `JwtSettings:RefreshTokenExpiryDays` [cite: Docs/AuthSystemMaintenance.md].
* **Security Features**:
    * One-time use (marked `IsUsed=true` after first use) [cite: Zarichney.Server/Services/Auth/AuthService.cs].
    * Can be revoked by user or admin (`IsRevoked=true`) [cite: Zarichney.Server/Services/Auth/AuthService.cs].
    * Automatically cleaned up by `RefreshTokenCleanupService` background task [cite: Zarichney.Server/Services/Auth/RefreshTokenCleanupService.cs].

## 5. API Key Management

### 5.1. API Key Concepts

* API keys provide an alternative authentication method for non-interactive clients (e.g., scripts, other services) [cite: Docs/ApiKeyAuthentication.md].
* Keys are associated with individual user accounts [cite: Docs/ApiKeyAuthentication.md].
* Each user can have multiple API keys [cite: Docs/ApiKeyAuthentication.md].
* Keys can optionally be set to expire [cite: Docs/ApiKeyAuthentication.md].
* An API key grants the same level of access as the associated user [cite: Docs/ApiKeyAuthentication.md].

### 5.2. Creating an API Key (Admin Only)

* Endpoint: `POST /api/auth/api-keys` [cite: Docs/ApiKeyAuthentication.md]
* Requires: JWT Authentication with 'admin' role [cite: Zarichney.Server/Controllers/AuthController.cs].
* Request Body:
  ```json
  {
    "name": "My Key Name", // Required
    "description": "Optional description",
    "expiresAt": "YYYY-MM-DDTHH:mm:ssZ" // Optional UTC expiry
  }
  ```
* Response: Returns the full API key details, including the `keyValue` **only once**. Store the `keyValue` securely immediately [cite: Docs/ApiKeyAuthentication.md].
  ```json
  {
    "id": 1,
    "keyValue": "generated-api-key-value",
    "name": "My Key Name",
    "createdAt": "...",
    "expiresAt": "...",
    "isActive": true,
    "description": "..."
  }
  ```

### 5.3. Listing API Keys (Admin Only)

* Endpoint: `GET /api/auth/api-keys` [cite: Docs/ApiKeyAuthentication.md]
* Requires: JWT Authentication with 'admin' role [cite: Zarichney.Server/Controllers/AuthController.cs].
* Response: Returns a list of API key metadata (ID, name, description, dates, status) for the calling admin's user ID. **Does not return the `keyValue`** [cite: Zarichney.Server/Services/Auth/Commands/ApiKeyCommands.cs].

### 5.4. Revoking an API Key

* Endpoint: `DELETE /api/auth/api-keys/{id}` [cite: Docs/ApiKeyAuthentication.md]
* Requires: JWT Authentication. **Warning:** Current implementation allows any authenticated user to revoke *any* key by ID if they know it. This should likely be restricted to Admins or the key owner [cite: Zarichney.Server/Controllers/AuthController.cs].
* Response: Success message [cite: Docs/ApiKeyAuthentication.md].

### 5.5. Using API Keys for Authentication

* Include the `X-Api-Key` header with the key value in the request [cite: Docs/ApiKeyAuthentication.md]:
  ```http
  GET /api/some-protected-endpoint
  X-Api-Key: your-api-key-value
  ```
* The `AuthenticationMiddleware` validates the key [cite: Zarichney.Server/Services/Auth/AuthenticationMiddleware.cs].

## 6. Email Verification

* New users receive an email with a verification link upon registration [cite: Zarichney.Server/Services/Auth/Commands/RegisterCommand.cs].
* The link targets the `/api/auth/confirm-email` endpoint with `userId` and `token` parameters [cite: Zarichney.Server/Controllers/AuthController.cs].
* Users cannot log in until their email is confirmed (`EmailConfirmed = true` in `AspNetUsers` table) [cite: Zarichney.Server/Services/Auth/Commands/LoginCommand.cs].
* Email templates are stored in the `EmailTemplates` directory (`email-verification.html`, `base.html`) [cite: Docs/AuthSystemMaintenance.md].

## 7. Password Reset

* User initiates via `/api/auth/forgot-password` with their email [cite: Zarichney.Server/Controllers/AuthController.cs].
* System sends an email with a reset link containing a token (template: `password-reset.html`) [cite: Zarichney.Server/Services/Auth/Commands/ForgotPasswordCommand.cs].
* User clicks link, which redirects to the frontend application.
* Frontend submits email, token, and new password to `/api/auth/reset-password` [cite: Zarichney.Server/Controllers/AuthController.cs].
* If successful, password is reset, and a confirmation email is sent (`password-reset-confirmation.html`) [cite: Zarichney.Server/Services/Auth/Commands/ResetPasswordCommand.cs].

## 8. Role-Based Authorization

* Uses ASP.NET Identity roles [cite: Docs/ApiKeyAuthentication.md].
* Currently defined role: `admin` [cite: Docs/ApiKeyAuthentication.md].
* Admin users can manage API keys for all users and manage roles via specific endpoints (`/api/auth/roles/*`) [cite: Docs/ApiKeyAuthentication.md].
* Role management endpoints require JWT authentication with the 'admin' role [cite: Zarichney.Server/Controllers/AuthController.cs].

## 9. Database Maintenance

* The auth system uses the `UserDbContext` with key tables [cite: Docs/AuthSystemMaintenance.md]:
    * `AspNetUsers`: User accounts.
    * `AspNetRoles`, `AspNetUserRoles`: Role assignments.
    * `AspNetUserTokens`: Stores tokens like password reset tokens.
    * `RefreshTokens`: Stores refresh tokens for session management [cite: Zarichney.Server/Services/Auth/Models/RefreshToken.cs].
    * `ApiKeys`: Stores API key metadata [cite: Zarichney.Server/Services/Auth/Models/ApiKey.cs].
* Refer to `Docs/PostgreSqlMaintenance.md` for details on migrations, backups, and direct SQL management [cite: Docs/PostgreSqlMaintenance.md].

## 10. Regular Maintenance Tasks

* **Token Cleanup**: Verify the `RefreshTokenCleanupService` background task runs correctly (check logs). Manually clean up expired/used/revoked tokens via SQL if needed [cite: Docs/AuthSystemMaintenance.md, Zarichney.Server/Services/Auth/RefreshTokenCleanupService.cs].
* **User Account Auditing**: Periodically review inactive accounts, unverified accounts, and failed login attempts [cite: Docs/AuthSystemMaintenance.md].
* **Email Deliverability**: Monitor email delivery rates; verify DNS records (SPF, DKIM, DMARC) [cite: Docs/AuthSystemMaintenance.md].
* **Configuration Review**: Periodically review and rotate sensitive keys (`JwtSettings:SecretKey`, API keys, `EmailConfig` secrets) [cite: Docs/AuthSystemMaintenance.md].

## 11. Troubleshooting

### User Cannot Log In

1.  Verify user exists in `AspNetUsers`.
2.  Check `EmailConfirmed` is `true`.
3.  Check for account lockout (`LockoutEnd` is not null, `AccessFailedCount` \> threshold) [cite: Docs/AuthSystemMaintenance.md]. Unlock if necessary (see `Docs/PostgreSqlMaintenance.md` [cite: Docs/PostgreSqlMaintenance.md]).
4.  Check application logs for specific errors (`journalctl -u cookbook-api | grep -i "auth\|login"`).

### Token Validation Failures (JWT/Refresh)

1.  Verify `JwtSettings:SecretKey` matches across all instances/environments [cite: Docs/AuthSystemMaintenance.md].
2.  Check token expiration times (Access: `JwtSettings:ExpiryMinutes`, Refresh: `JwtSettings:RefreshTokenExpiryDays`) [cite: Docs/AuthSystemMaintenance.md].
3.  Check server clock synchronization.
4.  For refresh token failures, check `RefreshTokens` table for `IsUsed`, `IsRevoked`, `ExpiresAt` status [cite: Docs/PostgreSqlMaintenance.md].

### API Key Authentication Issues

1.  **Error: "API key or JWT authentication is required"**: Ensure `X-Api-Key` header is present and spelled correctly [cite: Zarichney.Server/Services/Auth/AuthenticationMiddleware.cs].
2.  **Error: "Invalid API key"**:
    * Verify the key value matches an entry in the `ApiKeys` table.
    * Check `IsActive` is `true` and `ExpiresAt` (if set) is in the future [cite: Docs/ApiKeyAuthentication.md].
3.  **Error: "User associated with API key not found"**: The `UserId` linked to the API key in the `ApiKeys` table does not correspond to a valid user in `AspNetUsers` [cite: Zarichney.Server/Services/Auth/AuthenticationMiddleware.cs].
4.  **Error: "Authorization failed. These requirements were not met..."**: The `AuthenticationMiddleware` successfully authenticated the key, but the user lacks the required roles/claims for the specific endpoint being accessed, OR the `ClaimsIdentity` wasn't properly created by the middleware (check `AuthenticationType` is non-null) [cite: Docs/ApiKeyAuthentication.md].
5.  Use the `/api/test-auth` endpoint with the `X-Api-Key` header to check authentication status, assigned roles, and whether API key auth was recognized [cite: Docs/ApiKeyAuthentication.md].

### Email Verification Issues

1.  Check `EmailConfig` credentials in application configuration [cite: Docs/AuthSystemMaintenance.md].
2.  Verify `email-verification.html` template exists and renders correctly [cite: Docs/AuthSystemMaintenance.md].
3.  Check email service logs (e.g., Microsoft Graph activity) for delivery errors.
4.  Ensure confirmation tokens haven't expired (default lifespan set by ASP.NET Core Identity).

## 12. Security Best Practices

1.  **Rotate JWT Secret Keys**: Change `JwtSettings:SecretKey` periodically [cite: Docs/AuthSystemMaintenance.md].
2.  **Secure Secret Storage**: Use AWS Secrets Manager/SSM, environment variables, or .NET User Secrets for `JwtSettings:SecretKey`, `EmailConfig` credentials, `ConnectionStrings` password, and API keys [cite: Docs/PostgreSqlMaintenance.md].
3.  **Monitor Login Patterns**: Watch logs for unusual activity (e.g., high failed login rates) [cite: Docs/AuthSystemMaintenance.md].
4.  **Rate Limiting**: Implement rate limiting on auth endpoints (`/login`, `/register`, `/forgot-password`) [cite: Docs/AuthSystemMaintenance.md].
5.  **API Key Security**:
    * Treat API keys like passwords; store securely on the client-side [cite: Docs/ApiKeyAuthentication.md].
    * Avoid embedding keys directly in client-side code or version control [cite: Docs/ApiKeyAuthentication.md].
    * Use HTTPS for all API requests [cite: Docs/ApiKeyAuthentication.md].
    * Set expiration dates on keys where appropriate [cite: Docs/ApiKeyAuthentication.md].
    * Rotate keys periodically [cite: Docs/ApiKeyAuthentication.md].
    * Revoke keys when no longer needed or if compromised [cite: Docs/ApiKeyAuthentication.md].
6.  **Use HttpOnly Cookies**: The system uses HttpOnly cookies for JWT and refresh tokens, mitigating XSS risks [cite: Zarichney.Server/Services/Auth/CookieAuthManager.cs, Docs/AuthRefactoring.md].
7.  **Token Lifetimes**: Keep access tokens short-lived (e.g., 15-60 min). Balance refresh token lifetime based on security vs. user convenience [cite: Docs/AuthSystemMaintenance.md].

## 13. Testing

* Test scripts are available in the `Scripts` directory (`test-auth-endpoints.ps1`, `test-auth-endpoints.sh`) to verify auth endpoints [cite: Docs/AuthSystemMaintenance.md].
* Use the `/api/test-auth` endpoint with `Authorization: Bearer <token>` or `X-Api-Key: <key>` to check current authentication status [cite: Docs/ApiKeyAuthentication.md].

## 14. Deployment Considerations

* Ensure all secrets (`JwtSettings:SecretKey`, connection string password, email credentials, API Key service config if any) are configured securely in the production environment (e.g., via environment variables injected from AWS Secrets Manager/SSM) [cite: Docs/AuthSystemMaintenance.md].
* Verify database migrations are applied correctly during deployment [cite: Docs/PostgreSqlMaintenance.md].
* Ensure the `RefreshTokenCleanupService` background task is running in production.
* Test all auth flows thoroughly in a staging environment before deploying to production [cite: Docs/AuthSystemMaintenance.md].

## 15. Future Enhancements

* Multi-factor Authentication (MFA) [cite: Docs/AuthSystemMaintenance.md].
* Social Login integration (Google, etc.) [cite: Docs/AuthSystemMaintenance.md].
* More granular Role-Based Access Control (RBAC) [cite: Docs/AuthSystemMaintenance.md].
* Enhanced Session Management (e.g., "logout everywhere") [cite: Docs/AuthSystemMaintenance.md].
* More robust Account Lockout policies [cite: Docs/AuthSystemMaintenance.md].

## 16. Related Documentation

* [/Services/Auth/README.md](../..//Services/Auth/README.md) (Detailed Module Architecture)
* [Docs/PostgreSqlDatabase.md](./PostgreSqlDatabase.md) (Database Specifics)
* [Docs/AmazonWebServices.md](./AmazonWebServices.md) (AWS Environment Details)
* [ASP.NET Core Identity Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity) [cite: Docs/AuthSystemMaintenance.md]
* [JWT Introduction](https://jwt.io/introduction) [cite: Docs/AuthSystemMaintenance.md]
* [Refresh Token Best Practices](https://auth0.com/blog/refresh-tokens-what-are-they-and-when-to-use-them/) [cite: Docs/AuthSystemMaintenance.md]
