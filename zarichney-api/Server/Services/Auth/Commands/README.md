# Module/Directory: Server/Services/Auth/Commands

**Last Updated:** 2025-04-03

> **Parent:** [`Server/Services/Auth`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory implements the Command and Query handlers for the Authentication module, following the Command Query Responsibility Segregation (CQRS) pattern using the MediatR library. It encapsulates the business logic for specific authentication, authorization, and user management actions. [cite: zarichney-api/Docs/AuthRefactoring.md]
* **Key Responsibilities:**
    * Handling user registration (`RegisterCommand`). [cite: zarichney-api/Server/Services/Auth/Commands/RegisterCommand.cs]
    * Handling user login (`LoginCommand`). [cite: zarichney-api/Server/Services/Auth/Commands/LoginCommand.cs]
    * Handling email confirmation (`ConfirmEmailCommand`) and resending confirmation (`ResendConfirmationCommand`). [cite: zarichney-api/Server/Services/Auth/Commands/ConfirmEmailCommand.cs, zarichney-api/Server/Services/Auth/Commands/ResendConfirmationCommand.cs]
    * Handling password reset flow (forgot password request `ForgotPasswordCommand`, actual reset `ResetPasswordCommand`). [cite: zarichney-api/Server/Services/Auth/Commands/ForgotPasswordCommand.cs, zarichney-api/Server/Services/Auth/Commands/ResetPasswordCommand.cs]
    * Handling JWT refresh token validation and issuance (`RefreshTokenCommand`). [cite: zarichney-api/Server/Services/Auth/Commands/RefreshTokenCommand.cs]
    * Handling refresh token revocation (`RevokeTokenCommand`). [cite: zarichney-api/Server/Services/Auth/Commands/RevokeTokenCommand.cs]
    * Handling API key management (create `CreateApiKeyCommand`, revoke `RevokeApiKeyCommand`, get `GetApiKeysQuery`, `GetApiKeyByIdQuery`). [cite: zarichney-api/Server/Services/Auth/Commands/ApiKeyCommands.cs]
    * Handling role management (add user to role `AddUserToRoleCommand`, remove user from role `RemoveUserFromRoleCommand`, get user roles `GetUserRolesQuery`, get users in role `GetUsersInRoleQuery`). [cite: zarichney-api/Server/Services/Auth/Commands/RoleCommands.cs]
    * Handling user claims refresh (`RefreshUserClaimsCommand`). [cite: zarichney-api/Server/Services/Auth/Commands/RefreshUserClaimsCommand.cs]
* **Why it exists:** To decouple the core business logic of authentication actions from the API controllers (`AuthController`), improve testability by isolating logic in handlers, and adhere to the CQRS pattern for better organization. [cite: zarichney-api/Docs/AuthRefactoring.md]

## 2. Architecture & Key Concepts

* **Pattern:** CQRS using MediatR. Each public action exposed by `AuthController` typically corresponds to a Command or Query record defined here. [cite: zarichney-api/Server/Controllers/AuthController.cs]
* **Structure:** Each `.cs` file generally contains:
    * A `record` defining the Command/Query (implementing `IRequest<TResponse>`).
    * An `IRequestHandler<TRequest, TResponse>` implementation class containing the business logic.
* **Execution Flow:**
    1. `AuthController` receives an HTTP request.
    2. Controller creates a Command/Query record with data from the request.
    3. Controller uses injected `IMediator` to `Send` the command/query.
    4. MediatR routes the request to the appropriate handler within this directory.
    5. The handler executes the business logic, interacting with services like `UserManager`, `IAuthService`, `IApiKeyService`, `IRoleManager`, `IEmailService`, etc., via constructor injection. [cite: zarichney-api/Server/Services/Auth/Commands/LoginCommand.cs, zarichney-api/Server/Services/Auth/Commands/RegisterCommand.cs, zarichney-api/Server/Services/Auth/Commands/ApiKeyCommands.cs]
    6. Handler returns a result (often `AuthResult` or a specific query result). [cite: zarichney-api/Server/Services/Auth/Models/AuthResult.cs]
    7. Controller maps the result to an appropriate HTTP response (`Ok`, `BadRequest`, `NotFound`, etc.).
* **Key Data Structures:** Primarily uses Command/Query records defined within each file (e.g., `LoginCommand`, `CreateApiKeyCommand`) and common response types like `AuthResult`, `RoleCommandResult`, `ApiKeyResponse`. Also interacts with entities defined in [`Server/Services/Auth/Models`](../Models/README.md) (e.g., `ApplicationUser`, `RefreshToken`, `ApiKey`).

## 3. Interface Contract & Assumptions

* **Key Public Interfaces:** The `IRequest<T>` records (Commands/Queries) are the public contracts consumed by MediatR. The handlers themselves are internal implementation details invoked by the MediatR pipeline.
* **Assumptions:**
    * **MediatR Configuration:** Assumes MediatR is correctly registered in `Program.cs` and can discover handlers within this assembly.
    * **Dependency Injection:** Assumes all necessary dependencies (`UserManager`, `IAuthService`, `IEmailService`, `IApiKeyService`, `IRoleManager`, `ILogger`, etc.) are registered in the DI container and correctly injected into handlers via constructors.
    * **Input Validation:** Assumes basic input model validation (e.g., required fields) occurs in the `AuthController` before commands are sent. Handlers may perform additional business logic validation.
    * **Service Availability:** Assumes dependent services (Database via `UserDbContext`, Email Service) are available and functional.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Naming:** Command/Query records suffixed with `Command`/`Query`. Handler classes suffixed with `CommandHandler`/`QueryHandler`. One command/query and its handler per file is the standard pattern. [cite: zarichney-api/Server/Services/Auth/Commands/LoginCommand.cs, zarichney-api/Server/Services/Auth/Commands/ApiKeyCommands.cs]
* **Return Types:** Many command handlers return `AuthResult` or `RoleCommandResult` for consistency, providing a `Success` flag and a `Message`. Queries return specific result types (e.g., `List<ApiKeyResponse>`). [cite: zarichney-api/Server/Services/Auth/Models/AuthResult.cs, zarichney-api/Server/Services/Auth/Commands/RoleCommands.cs]
* **Dependency Scope:** Handlers are typically registered as transient or scoped by MediatR and receive dependencies via constructor injection.

## 5. How to Work With This Code

* **Adding a New Auth Action:**
    1. Define a new `record MyActionCommand(...) : IRequest<MyActionResult>` in a new `.cs` file.
    2. Define the `MyActionResult` type (if not using `AuthResult`).
    3. Create a `class MyActionCommandHandler(...) : IRequestHandler<MyActionCommand, MyActionResult>` in the same file.
    4. Inject necessary dependencies into the handler's constructor.
    5. Implement the business logic within the `Handle` method.
    6. Ensure the corresponding `AuthController` action is created to send this command via `IMediator`.
* **Testing:** Handlers can be unit tested by:
    * Mocking their constructor dependencies (`UserManager`, `IAuthService`, `IEmailService`, etc.) using a library like Moq or NSubstitute.
    * Instantiating the handler directly with the mocks.
    * Calling the `Handle` method with a test command object and asserting the result.
* **Common Pitfalls / Gotchas:** Forgetting to register a necessary dependency for a handler. Logic errors within the `Handle` method. Incorrectly handling exceptions or failing to return appropriate `AuthResult`/`RoleCommandResult` status.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Server/Services/Auth`](../README.md) - Consumes interfaces `IAuthService`, `IApiKeyService`, `IRoleManager`.
    * [`Server/Services/Auth/Models`](../Models/README.md) - Uses entities (`ApplicationUser`, `RefreshToken`, `ApiKey`) and result models (`AuthResult`, `RoleCommandResult`, `ApiKeyResponse`).
    * [`Server/Services/Emails`](../../Services/Emails/README.md) - Consumed by handlers needing to send emails (Register, ForgotPassword, ResetPassword). [cite: zarichney-api/Server/Services/Auth/Commands/RegisterCommand.cs, zarichney-api/Server/Services/Auth/Commands/ForgotPasswordCommand.cs]
    * [`Server/Config`](../../Config/README.md) - Consumes `ClientConfig`, `ServerConfig` indirectly via injected services or for constructing URLs.
* **External Library Dependencies:**
    * `MediatR`: Core library for implementing the CQRS pattern.
    * `Microsoft.AspNetCore.Identity.EntityFrameworkCore`: Specifically `UserManager<ApplicationUser>` used heavily by handlers.
* **Dependents (Impact of Changes):**
    * [`Server/Controllers/AuthController.cs`](../../Controllers/AuthController.cs) - Directly sends commands/queries defined here. Changes to command signatures require controller updates.

## 7. Rationale & Key Historical Context

* **CQRS with MediatR:** This pattern was adopted during a refactor to separate command (write) logic from query (read) logic (though most "queries" here are simple lookups within command handlers or dedicated query handlers like `GetApiKeysQuery`). This significantly improves the testability and maintainability of authentication logic compared to having it all within a single large service or controller. [cite: zarichney-api/Docs/AuthRefactoring.md] It isolates the business logic for each specific action (Login, Register, etc.).

## 8. Known Issues & TODOs

* Consider adding more granular input validation within command handlers using libraries like FluentValidation, rather than relying solely on controller model state or basic checks.
* Some error messages returned in `AuthResult` or `RoleCommandResult` could potentially be more specific for debugging purposes (while remaining safe for client consumption).