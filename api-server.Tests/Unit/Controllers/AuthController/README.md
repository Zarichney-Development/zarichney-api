# Module/Directory: /Unit/Controllers/AuthController

**Last Updated:** 2025-04-18

> **Parent:** [`Controllers`](../README.md)
> **Related:**
> * **Source:** [`AuthController.cs`](../../../../api-server/Controllers/AuthController.cs)
> * **Commands:** [`Services/Auth/Commands/`](../../../../api-server/Services/Auth/Commands/)
> * **Services:** [`AuthService.cs`](../../../../api-server/Services/Auth/AuthService.cs), [`CookieAuthManager.cs`](../../../../api-server/Services/Auth/CookieAuthManager.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

These tests verify the **internal logic of the `AuthController` action methods in isolation** from the ASP.NET Core pipeline and actual service implementations (like database access or email sending).

* **Why Unit Tests?** To provide fast, targeted feedback on the controller's logic. They ensure the controller correctly:
    * Maps HTTP request data (DTOs) to the appropriate `MediatR` commands.
    * Dispatches commands using `IMediator.Send()`.
    * Interacts correctly with `ICookieAuthManager` (signing in/out).
    * Translates the `AuthResult` (or exceptions) returned by MediatR/services into the correct `ActionResult` (e.g., `OkObjectResult`, `BadRequestObjectResult`, `UnauthorizedObjectResult`).
* **Why Mock Dependencies?** To isolate the `AuthController`'s logic. Key dependencies `IMediator` and `ICookieAuthManager` (plus `ILogger`) are mocked to control their behavior and verify interactions.

## 2. Scope & Key Functionality Tested (What?)

These tests focus on the code *within* each `AuthController` action method:

* **Request-to-Command Mapping:** Verifying that data from request DTOs (e.g., `LoginRequest`, `RegisterRequest`) is correctly used to construct the corresponding MediatR command objects.
* **`IMediator.Send()` Invocation:** Ensuring `_mediator.Send()` is called with the correctly constructed command object.
* **`ICookieAuthManager` Interaction:** Verifying that methods like `SignInAsync` or `SignOutAsync` are called appropriately based on the MediatR command result (e.g., call `SignInAsync` after successful login/refresh).
* **Result Handling:** Testing how different `AuthResult` objects returned by the mocked `IMediator.Send()` (e.g., success, failure with specific errors) are handled and mapped to specific `ActionResult` types and content.
* **Exception Handling:** Verifying how exceptions potentially thrown by mocked `IMediator` or `ICookieAuthManager` are handled (though often error handling is delegated to middleware, tested in integration).

## 3. Test Environment Setup

* **Instantiation:** `AuthController` is instantiated directly in tests.
* **Mocking:** Mocks for `MediatR.IMediator`, `Zarichnyi.ApiServer.Services.Auth.ICookieAuthManager`, and `Microsoft.Extensions.Logging.ILogger<AuthController>` must be provided to the constructor. Mocks are configured using Moq to return specific `AuthResult` objects or simulate behavior for different test scenarios.

## 4. Maintenance Notes & Troubleshooting

* **Command/DTO Changes:** If request DTOs or MediatR command structures change, these unit tests will likely need updates to reflect the new mapping logic.
* **Mock Setup:** Ensure `IMediator` mocks are configured to return relevant `AuthResult` instances (use `AuthResult.Success(...)` and `AuthResult.Failure(...)` appropriately). Verify mock interactions (`Mock.Verify(...)`) for both `IMediator` and `ICookieAuthManager`.
* **Assertion Failures:** Check the type (`OkObjectResult`, `BadRequestObjectResult`, etc.) and value (status code, error messages in the body) of the `ActionResult` returned by the controller action. Trace back to the mocked `AuthResult` to ensure it matches the expected outcome.

## 5. Test Cases & TODOs

*(Focus is on interaction with MediatR and CookieManager, and ActionResult mapping)*

### `Register` Action
* **TODO:** Test mapping `RegisterRequest` to `RegisterCommand`.
* **TODO:** Test `_mediator.Send` called with correct `RegisterCommand`.
* **TODO:** Test handling `AuthResult.Success` from MediatR -> verify `OkObjectResult`.
* **TODO:** Test handling `AuthResult.Failure` (e.g., email exists) from MediatR -> verify `BadRequestObjectResult` with errors.

### `Login` Action
* **TODO:** Test mapping `LoginRequest` to `LoginCommand`.
* **TODO:** Test `_mediator.Send` called with correct `LoginCommand`.
* **TODO:** Test handling `AuthResult.Success` (with tokens) from MediatR -> verify `_cookieAuthManager.SignInAsync` called, verify `OkObjectResult` with tokens.
* **TODO:** Test handling `AuthResult.Failure` (invalid credentials) from MediatR -> verify `UnauthorizedObjectResult`.

### `Refresh` Action
* **TODO:** Test `_mediator.Send` called with `RefreshTokenCommand`.
* **TODO:** Test handling `AuthResult.Success` (with tokens) from MediatR -> verify `_cookieAuthManager.SignInAsync` called, verify `OkObjectResult` with tokens.
* **TODO:** Test handling `AuthResult.Failure` (invalid token) from MediatR -> verify `UnauthorizedObjectResult`.

### `Revoke` Action
* **TODO:** Test `_mediator.Send` called with `RevokeTokenCommand`.
* **TODO:** Test handling `AuthResult.Success` from MediatR -> verify `_cookieAuthManager.SignOutAsync` called, verify `OkResult`.
* **TODO:** Test handling `AuthResult.Failure` from MediatR -> verify `BadRequestObjectResult`.

### `ForgotPassword` Action
* **TODO:** Test mapping `ForgotPasswordRequest` to `ForgotPasswordCommand`.
* **TODO:** Test `_mediator.Send` called with correct `ForgotPasswordCommand`.
* **TODO:** Test handling `AuthResult.Success` -> verify `OkResult`.
* **TODO:** Test handling `AuthResult.Failure` -> verify `OkResult` (as per controller logic).

### `ResetPassword` Action
* **TODO:** Test mapping `ResetPasswordRequest` to `ResetPasswordCommand`.
* **TODO:** Test `_mediator.Send` called with correct `ResetPasswordCommand`.
* **TODO:** Test handling `AuthResult.Success` -> verify `OkResult`.
* **TODO:** Test handling `AuthResult.Failure` -> verify `BadRequestObjectResult` with errors.

### `ResendConfirmation` Action
* **TODO:** Test `_mediator.Send` called with `ResendConfirmationCommand`.
* **TODO:** Test handling `AuthResult.Success` -> verify `OkResult`.
* **TODO:** Test handling `AuthResult.Failure` -> verify `BadRequestObjectResult` with errors.

### API Key Actions (`CreateApiKey`, `GetApiKeys`, `RevokeApiKey`)
* **TODO:** Test mapping requests to `CreateApiKeyCommand`, `GetApiKeysQuery`, `RevokeApiKeyCommand`.
* **TODO:** Test `_mediator.Send` called with correct command/query.
* **TODO:** Test handling success/failure `AuthResult` from MediatR -> verify correct `ActionResult` (`OkObjectResult`, `NoContentResult`, `BadRequestObjectResult`).

### Role Actions (`AssignRole`, `RemoveRole`)
* **TODO:** Test mapping requests to `AssignRoleCommand`, `RemoveRoleCommand`.
* **TODO:** Test `_mediator.Send` called with correct command.
* **TODO:** Test handling success/failure `AuthResult` from MediatR -> verify correct `ActionResult`.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `AuthController` unit tests. (Gemini)

