# Module/Directory: /Unit/Services/Auth/Commands

**Last Updated:** 2025-04-18

> **Parent:** [`Auth`](../README.md)
> **Related:**
> * **Source:** [`Services/Auth/Commands/`](../../../../../api-server/Services/Auth/Commands/) (Contains command definitions and handlers)
> * **Dependencies:** ASP.NET Core Identity (`UserManager`, `SignInManager`, `RoleManager`), `UserDbContext`, `IEmailService`, `IConfiguration`/`IOptions`, MediatR (`IRequestHandler`)
> * **Models:** [`Models/AuthResult.cs`](../../../../../api-server/Services/Auth/Models/AuthResult.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the individual **MediatR command handlers** responsible for executing the core business logic of each authentication and authorization operation (e.g., user registration, login, password reset, API key management, role assignment). These tests are arguably the most critical for verifying the security and correctness of the authentication system.

* **Why Unit Tests?** To validate the detailed step-by-step logic within each command handler (`Handle` method) in complete isolation. This ensures that database operations (via mocked `DbContext`), Identity framework interactions (via mocked `UserManager`, `SignInManager`, `RoleManager`), email sending (via mocked `IEmailService`), token generation/validation, and result construction (`AuthResult`) are performed correctly according to business rules for every success and failure scenario.
* **Isolation:** Achieved by instantiating the specific command handler under test and providing mocks for *all* its injected dependencies.

## 2. Scope & Key Functionality Tested (What?)

Each test file within this directory focuses on **one specific command handler** (e.g., `RegisterCommandHandlerTests.cs` tests `RegisterCommandHandler`). The scope includes testing all code paths within the handler's `Handle` method:

* **Input Validation:** Checking command properties for validity (although often handled by FluentValidation elsewhere, handlers might have internal checks).
* **Identity Service Interactions:** Verifying correct calls to mocked `UserManager`, `SignInManager`, `RoleManager` methods (e.g., `CreateAsync`, `CheckPasswordSignInAsync`, `GeneratePasswordResetTokenAsync`, `AddToRoleAsync`) with expected arguments.
* **Database Interactions:** Verifying correct interaction with the mocked `UserDbContext` (e.g., querying for users/tokens, adding/updating `ApiKey` or `RefreshToken` entities, calling `SaveChangesAsync`).
* **External Service Interactions:** Verifying calls to mocked `IEmailService` with correct parameters (recipient, template data).
* **Helper/Utility Interactions:** Verifying calls to any internal helpers (e.g., JWT generation).
* **Conditional Logic:** Testing all branches (if/else, switch statements).
* **`AuthResult` Construction:** Ensuring the correct `AuthResult` (Success or Failure) is returned with appropriate data (tokens, user info) or error codes/messages for every possible outcome.

## 3. Test Environment Setup

* **Instantiation:** The specific `IRequestHandler<TCommand, AuthResult>` implementation is instantiated directly.
* **Mocking:** Extensive and careful mocking of *all* dependencies injected into the handler's constructor is required. This typically includes:
    * `Mock<UserManager<IdentityUser>>` (and its `IUserStore`)
    * `Mock<SignInManager<IdentityUser>>` (and its dependencies)
    * `Mock<RoleManager<IdentityRole>>` (and its `IRoleStore`)
    * `Mock<UserDbContext>` (including mocking `DbSet<T>` properties and `SaveChangesAsync`)
    * `Mock<IEmailService>`
    * `Mock<IOptions<JwtSettings>>` or `Mock<IConfiguration>`
    * Mocks for any other specific services or helpers used by the handler.
* **Command Object:** A valid instance of the specific MediatR command (e.g., `RegisterCommand`) is created and passed to the `Handle` method.

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Create a separate test class file for each command handler within this directory (e.g., `RegisterCommandHandlerTests.cs`, `LoginCommandHandlerTests.cs`). Use regions or nested classes within test files to organize tests by scenario (e.g., `Handle_Success`, `Handle_UserNotFound`, `Handle_InvalidPassword`).
* **Identity Mocking Complexity:** Setting up mocks for `UserManager`, `SignInManager`, and `RoleManager` is complex due to their dependencies (especially stores). Ensure mocks are configured to return expected results (`IdentityResult.Success`, `IdentityResult.Failed`, specific users/roles, tokens) for the scenarios being tested. Consider shared helper methods or base classes for common Identity mock setups.
* **DbContext Mocking:** Mocking `DbSet<T>` and `SaveChangesAsync` requires careful setup to simulate database state correctly for different test cases.
* **Thoroughness:** Aim for high coverage of the logic within each handler, testing all success paths and anticipated failure paths. Verify both the returned `AuthResult` and the interactions with mocked dependencies (`Mock.Verify(...)`).

## 5. Test Cases & TODOs

Detailed TODOs belong within the specific test files for each handler. This README serves as an index and overview. Ensure test files exist and have comprehensive coverage for at least the following handlers:

* **TODO:** Create/Populate `RegisterCommandHandlerTests.cs`
* **TODO:** Create/Populate `LoginCommandHandlerTests.cs`
* **TODO:** Create/Populate `RefreshTokenCommandHandlerTests.cs`
* **TODO:** Create/Populate `RevokeTokenCommandHandlerTests.cs`
* **TODO:** Create/Populate `ForgotPasswordCommandHandlerTests.cs`
* **TODO:** Create/Populate `ResetPasswordCommandHandlerTests.cs`
* **TODO:** Create/Populate `ConfirmEmailCommandHandlerTests.cs`
* **TODO:** Create/Populate `ResendConfirmationCommandHandlerTests.cs`
* **TODO:** Create/Populate `CreateApiKeyCommandHandlerTests.cs` (and other ApiKey command handlers)
* **TODO:** Create/Populate `GetApiKeysQueryHandlerTests.cs` (if using queries)
* **TODO:** Create/Populate `RevokeApiKeyCommandHandlerTests.cs`
* **TODO:** Create/Populate `AssignRoleCommandHandlerTests.cs` (and other Role command handlers)
* **TODO:** Create/Populate `RemoveRoleCommandHandlerTests.cs`
* **TODO:** Create/Populate `RefreshUserClaimsCommandHandlerTests.cs`
* **TODO:** *(Add any other command handlers present in the source directory)*

**For each handler test file, ensure TODOs cover:**
* Testing the primary success path.
* Testing all known failure paths (e.g., user not found, invalid password, email already exists, token expired, insufficient permissions).
* Verifying interactions with all mocked dependencies for each path.
* Verifying the correctness of the returned `AuthResult` (Success/Failure, data, error messages) for each path.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for Auth Command Handler unit tests. (Gemini)

