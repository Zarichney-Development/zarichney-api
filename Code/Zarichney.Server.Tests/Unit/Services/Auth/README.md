# Module/Directory: /Unit/Services/Auth

**Last Updated:** 2025-01-20

> **Parent:** [../README.md](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Comprehensive unit test suite for authentication and authorization services in the zarichney-api application, covering all aspects of user authentication, API key management, role-based access control, and security middleware.
* **Key Responsibilities:**
  - Unit testing of authentication services and workflows
  - Validation of API key generation, validation, and management
  - Testing of role-based authorization and permission systems
  - Verification of authentication middleware and cookie management
  - Testing of user management and role initialization services
  - Security token handling and refresh token management testing
* **Why it exists:** To ensure the security foundation of the application is robust and reliable, providing comprehensive coverage of all authentication and authorization components that protect the API and user data.

### Child Test Modules:
* **ApiKeyService:** [./ApiKeyService/README.md](./ApiKeyService/README.md) - API key generation and validation testing
* **AuthService:** [./AuthService/README.md](./AuthService/README.md) - Core authentication service testing
* **AuthenticationMiddleware:** [./AuthenticationMiddleware/README.md](./AuthenticationMiddleware/README.md) - Authentication middleware pipeline testing
* **Commands:** [./Commands/README.md](./Commands/README.md) - Authentication command pattern testing
* **CookieAuthManager:** [./CookieAuthManager/README.md](./CookieAuthManager/README.md) - Cookie-based authentication testing
* **RefreshTokenCleanupService:** [./RefreshTokenCleanupService/README.md](./RefreshTokenCleanupService/README.md) - Token cleanup and maintenance testing
* **RoleInitializer:** [./RoleInitializer/README.md](./RoleInitializer/README.md) - Role system initialization testing
* **RoleManager:** [./RoleManager/README.md](./RoleManager/README.md) - Role management service testing

## 2. Architecture & Key Concepts

* **High-Level Design:** Authentication tests follow a layered security model with service-level unit tests that mock external dependencies while testing internal authentication logic. Tests cover the full authentication flow from initial request through token validation and role verification.
* **Core Logic Flow:**
  1. **Authentication Request:** User credentials or API keys are validated
  2. **Identity Resolution:** User identity is established and roles are determined
  3. **Token Generation:** Security tokens (JWT, refresh tokens) are created
  4. **Authorization Check:** Role-based permissions are verified
  5. **Session Management:** User sessions are tracked and maintained
* **Key Data Structures:**
  - `AuthUser` - Authenticated user identity with roles and permissions
  - `ApiKey` - API key entities with scope and expiration
  - `RefreshToken` - Token refresh management entities
  - `AuthCommand` - Command pattern for authentication operations
* **State Management:** Authentication state is managed through secure cookies, JWT tokens, and server-side session tracking
* **Security Testing Architecture:**
  ```mermaid
  graph TD
      A[Auth Request] --> B{Credential Type}
      B -->|Username/Password| C[AuthService]
      B -->|API Key| D[ApiKeyService]
      B -->|Cookie| E[CookieAuthManager]
      C --> F[Identity Resolution]
      D --> F
      E --> F
      F --> G[Role Assignment]
      G --> H[Token Generation]
      H --> I[Session Creation]
      I --> J[Authorization Ready]
  ```

## 3. Interface Contract & Assumptions

* **Key Public Testing Interfaces:**
  * `AuthServiceTests` - Core authentication service validation:
    * **Purpose:** Test user authentication, password validation, and identity establishment
    * **Critical Preconditions:** Valid user credentials, configured authentication providers, working database connection
    * **Critical Postconditions:** Authenticated user identity with proper roles, security tokens generated
    * **Non-Obvious Error Handling:** Tests account lockout scenarios, rate limiting, and concurrent login attempts
  * `ApiKeyServiceTests` - API key management testing:
    * **Purpose:** Validate API key generation, validation, and lifecycle management
    * **Critical Preconditions:** Properly configured API key settings, valid user context for key generation
    * **Critical Postconditions:** Unique API keys with proper scopes and expiration, secure storage
    * **Non-Obvious Error Handling:** Tests key collision scenarios, expired key handling, and scope validation failures
  * `MockAuthHandlerTests` - Authentication handler testing:
    * **Purpose:** Test custom authentication handlers and mock authentication scenarios
    * **Critical Preconditions:** Properly configured authentication schemes and handler registration
    * **Critical Postconditions:** Consistent authentication behavior across different auth methods
    * **Non-Obvious Error Handling:** Tests authentication scheme fallbacks and handler chain failures

* **Critical Assumptions:**
  * **Identity Store:** User identity data is properly stored and retrievable from configured identity provider
  * **Cryptographic Security:** Secure random number generation and cryptographic operations function correctly
  * **Token Security:** JWT tokens are properly signed and validated with secure keys
  * **Session Management:** Server-side session storage is reliable and secure
  * **Database Integrity:** User and role data maintains referential integrity
  * **Time Synchronization:** Token expiration relies on synchronized system time

## 4. Local Conventions & Constraints

* **Configuration:** Tests use secure test configuration with mock cryptographic keys and test-specific authentication settings
* **Directory Structure:**
  - Each authentication service has dedicated subdirectory with focused test coverage
  - Cross-cutting authentication tests in parent directory files
  - Command pattern tests isolated in Commands/ subdirectory
* **Technology Choices:**
  - ASP.NET Core Identity for user management testing
  - JWT Bearer token testing with mock keys
  - Entity Framework Core for identity store testing
  - Secure random number generation for API keys and tokens
* **Performance Notes:** Authentication tests must execute quickly while maintaining cryptographic security testing
* **Security Notes:** Tests use secure mock data and avoid exposing real cryptographic keys or sensitive authentication data

## 5. How to Work With This Code

* **Setup:**
  - Configure test identity database with Entity Framework In-Memory provider
  - Set up mock authentication handlers and test authentication schemes
  - Initialize test users, roles, and API keys for consistent test scenarios
  - Configure secure test JWT signing keys and authentication options

* **Module-Specific Testing Strategy:**
  - **Security-First Testing:** All authentication components tested with security as primary concern
  - **Isolation Testing:** Authentication services tested in isolation with mocked dependencies
  - **Cryptographic Testing:** Security token generation and validation thoroughly tested
  - **Edge Case Coverage:** Authentication failures, edge cases, and attack scenarios tested

* **Key Test Scenarios:**
  - **Authentication Success:** Valid credentials result in proper authentication and token generation
  - **Authentication Failure:** Invalid credentials properly rejected with appropriate error responses
  - **Token Lifecycle:** Token generation, validation, refresh, and expiration scenarios
  - **Role Authorization:** Role-based access control and permission verification
  - **Security Attacks:** Brute force, token manipulation, and session hijacking prevention
  - **Concurrency:** Multiple simultaneous authentication attempts and session management

* **Test Data Considerations:**
  - Use consistent test users with known credentials and role assignments
  - Generate predictable but secure test API keys and tokens
  - Test with various user roles and permission combinations
  - Include expired tokens, revoked credentials, and locked accounts in test scenarios

* **Testing Commands:**
  ```bash
  # Run all authentication tests
  dotnet test --filter "Category=Unit&FullyQualifiedName~Services.Auth"

  # Run specific authentication service tests
  dotnet test --filter "FullyQualifiedName~AuthServiceTests"

  # Run security-focused tests
  dotnet test --filter "Category=Security&FullyQualifiedName~Auth"

  # Run with coverage for authentication components
  dotnet test --collect:"XPlat Code Coverage" --filter "Category=Unit&FullyQualifiedName~Services.Auth"
  ```

## 6. Dependencies

* **Internal Code Dependencies:**
  * [`../../../Zarichney.Server/Services/Auth/`](../../../../Zarichney.Server/Services/Auth/README.md) - Actual authentication services under test
  * [`../../Framework/`](../../Framework/README.md) - Test framework utilities and authentication mock factories
  * [`../../../Zarichney.Server/Data/`](../../../../Zarichney.Server/Data/README.md) - Identity data models and database context

* **External Library Dependencies:**
  * `Microsoft.AspNetCore.Identity.EntityFrameworkCore` - Identity framework testing
  * `Microsoft.IdentityModel.Tokens` - JWT token testing and validation
  * `System.Security.Cryptography` - Cryptographic operations testing
  * `Microsoft.EntityFrameworkCore.InMemory` - In-memory database for identity testing
  * `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication testing

* **Dependents (Impact of Changes):**
  * [`../Controllers/Auth/`](../../Controllers/AuthController/README.md) - Authentication controller tests that depend on service contracts
  * [`../../Integration/Auth/`](../../Integration/Auth/README.md) - Integration tests for full authentication workflows
  * [`../Middleware/`](../Middleware/README.md) - Middleware tests that depend on authentication services

## 7. Rationale & Key Historical Context

Authentication testing was designed with security as the primary concern, leading to comprehensive test coverage of all security scenarios including attack vectors. The decision to mock cryptographic operations in unit tests while maintaining realistic security testing ensures fast test execution without compromising security validation. The command pattern implementation for authentication operations provides clear separation of concerns and enables comprehensive testing of each authentication workflow step.

## 8. Known Issues & TODOs

* Enhance multi-factor authentication testing coverage for advanced security scenarios
* Add performance testing for authentication operations under high load conditions
* Implement comprehensive audit logging testing for security compliance requirements
* Consider adding penetration testing scenarios for authentication bypass attempts
* Expand testing coverage for OAuth and external identity provider integration scenarios