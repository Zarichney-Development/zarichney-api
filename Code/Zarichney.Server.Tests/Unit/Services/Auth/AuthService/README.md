# Module/Directory: /Unit/Services/Auth/AuthService

**Last Updated:** 2025-04-18

> **Parent:** [`Auth`](../README.md)
> **Related:**
> * **Source:** [`Services/Auth/AuthService.cs`](../../../../../Zarichney.Server/Services/Auth/AuthService.cs)
> * **Dependencies:** `MediatR.IMediator`, `ILogger<AuthService>`
> * **Commands:** [`Commands/`](../../../../../Zarichney.Server/Services/Auth/Commands/) (Various command definitions like `RegisterCommand`, `LoginCommand`, etc.)
> * **Models:** [`Models/AuthResult.cs`](../../../../../Zarichney.Server/Services/Auth/Models/AuthResult.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `AuthService` class. This service typically acts as a high-level facade or orchestrator for authentication and authorization operations, primarily by creating and dispatching specific commands using the MediatR library.

* **Why Unit Tests?** To validate the logic within `AuthService` itself in isolation. These tests ensure that:
    * Input parameters received by `AuthService` methods are correctly mapped into the appropriate MediatR command objects (e.g., `RegisterCommand`, `LoginCommand`).
    * The `IMediator.Send()` method is invoked correctly with the constructed command.
    * The `AuthResult` returned by the (mocked) MediatR pipeline is correctly returned or handled by the `AuthService` method.
* **Isolation:** Achieved by mocking the core dependencies: `IMediator` and `ILogger`. The tests do *not* execute the actual MediatR command handlers; that logic is tested separately in the handler unit tests (`Unit/Services/Auth/Commands/`).

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `AuthService` focus on its public methods (e.g., `RegisterAsync`, `LoginAsync`, `RefreshTokenAsync`, `ForgotPasswordAsync`, etc.):

* **Parameter-to-Command Mapping:** Verifying that arguments passed to `AuthService` methods are accurately transferred into the properties of the corresponding MediatR command object.
* **`IMediator.Send()` Invocation:** Ensuring `_mediator.Send()` is called exactly once with the expected command object for each service method invocation. Argument captors or matchers (`It.Is<TCommand>`) are often used here.
* **Result Passthrough:** Verifying that the `AuthResult` object returned by the mocked `_mediator.Send()` call is directly returned by the `AuthService` method without modification (unless `AuthService` adds its own logic, which would also be tested).
* **Exception Handling:** Testing how exceptions potentially thrown by the mocked `_mediator.Send()` call are handled or propagated by the `AuthService` method.

## 3. Test Environment Setup

* **Instantiation:** `AuthService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<IMediator>`: Configured to return specific `AuthResult` instances (e.g., `AuthResult.Success(...)`, `AuthResult.Failure(...)`) when `Send()` is called with specific command types.
    * `Mock<ILogger<AuthService>>`.

## 4. Maintenance Notes & Troubleshooting

* **Focus:** Remember these tests verify the `AuthService`'s role as a dispatcher. The complex business logic resides in the command handlers, which have their own dedicated tests.
* **Command Changes:** If the structure of a MediatR command object changes (e.g., new properties), tests verifying the mapping in the corresponding `AuthService` method must be updated.
* **New Methods:** When adding new methods to `AuthService` that dispatch commands, corresponding unit tests should be added here.
* **Mocking `AuthResult`:** Ensure the `AuthResult` objects returned by the mocked `IMediator.Send()` accurately reflect the scenarios being tested (success with/without data, failure with specific error messages).

## 5. Test Cases & TODOs

### `AuthServiceTests.cs`
*(Assuming methods like RegisterAsync, LoginAsync, etc. exist)*

* **TODO (`RegisterAsync`):** Test input parameters mapped correctly to `RegisterCommand` properties.
* **TODO (`RegisterAsync`):** Test `_mediator.Send()` called once with the correct `RegisterCommand` instance.
* **TODO (`RegisterAsync`):** Test returns the `AuthResult` provided by the mocked `_mediator.Send()`.
* **TODO (`LoginAsync`):** Test input parameters mapped correctly to `LoginCommand` properties.
* **TODO (`LoginAsync`):** Test `_mediator.Send()` called once with the correct `LoginCommand` instance.
* **TODO (`LoginAsync`):** Test returns the `AuthResult` provided by the mocked `_mediator.Send()`.
* **TODO (`RefreshTokenAsync`):** Test input parameters mapped correctly to `RefreshTokenCommand` properties.
* **TODO (`RefreshTokenAsync`):** Test `_mediator.Send()` called once with the correct `RefreshTokenCommand` instance.
* **TODO (`RefreshTokenAsync`):** Test returns the `AuthResult` provided by the mocked `_mediator.Send()`.
* **TODO (`RevokeTokenAsync`):** Test input parameters mapped correctly to `RevokeTokenCommand` properties.
* **TODO (`RevokeTokenAsync`):** Test `_mediator.Send()` called once with the correct `RevokeTokenCommand` instance.
* **TODO (`RevokeTokenAsync`):** Test returns the `AuthResult` provided by the mocked `_mediator.Send()`.
* **TODO (`ForgotPasswordAsync`):** Test input parameters mapped correctly to `ForgotPasswordCommand` properties.
* **TODO (`ForgotPasswordAsync`):** Test `_mediator.Send()` called once with the correct `ForgotPasswordCommand` instance.
* **TODO (`ForgotPasswordAsync`):** Test returns the `AuthResult` provided by the mocked `_mediator.Send()`.
* **TODO (`ResetPasswordAsync`):** Test input parameters mapped correctly to `ResetPasswordCommand` properties.
* **TODO (`ResetPasswordAsync`):** Test `_mediator.Send()` called once with the correct `ResetPasswordCommand` instance.
* **TODO (`ResetPasswordAsync`):** Test returns the `AuthResult` provided by the mocked `_mediator.Send()`.
* **TODO:** *(Add similar tests for any other methods in `AuthService` that dispatch MediatR commands)*
* **TODO:** Test handling of exceptions thrown by `_mediator.Send()`.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `AuthService` unit tests. (Gemini)

