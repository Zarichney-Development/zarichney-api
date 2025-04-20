# Project Coding Standards for AI Assistants

**Version:** 1.4
**Last Updated:** 2025-04-20

## 1. Core Principles

* **Clarity:** Write code that is easy for others (human or AI) to understand. Prioritize readability.
* **Consistency:** Follow the patterns and conventions already established in this codebase. When in doubt, match the style of surrounding code. Refer to existing services, controllers, and configuration classes as examples.
* **Correctness:** Ensure code behaves as intended and meets requirements specified in the task prompt and associated `README.md` files. Verify against documented contracts and diagrams.
* **Robustness:** Write code that anticipates and handles potential errors and edge cases gracefully. Validate inputs and manage external dependencies reliably.
* **Maintainability:** Create code that is easy to debug, modify, extend, and test. Favor modularity and clear separation of concerns. Ensure documentation and diagrams are kept current.

## 2. Language & Formatting (C#)

* **Naming Conventions:**
  * `PascalCase` for classes, interfaces, enums, methods, properties, events, and public fields.
  * `camelCase` for local variables and method parameters.
  * Use descriptive and unambiguous names. Avoid abbreviations unless they are standard and widely understood (e.g., `Id`, `Dto`).
  * Prefix interfaces with `I` (e.g., `IFileService`, `IRecipeRepository`).
  * Suffix configuration classes with `Config` (e.g., `RecipeConfig`, `JwtSettings`).
  * Suffix command/query records for MediatR with `Command`/`Query` (e.g., `LoginCommand`, `GetApiKeysQuery`).
* **Style & Modern C# Features:**
  * Use file-scoped namespaces (`namespace Zarichney.Services;`). [cite: api-server/Services/FileService.cs, api-server/Services/AI/LlmService.cs]
  * Utilize primary constructors where they simplify class definitions and initialization. [cite: api-server/Services/Sessions/SessionManager.cs, api-server/Cookbook/Orders/OrderService.cs]
  * Use `record` types for immutable data transfer objects (DTOs) or simple data carriers where appropriate. [cite: api-server/Services/Auth/Commands/LoginCommand.cs, api-server/Cookbook/Recipes/RecipeModels.cs]
  * Keep methods concise and focused on a single responsibility. Aim for methods that fit on one screen.
  * Adhere to standard C# formatting (indentation, spacing, braces). *(Consider enforcing with .editorconfig)*

## 3. Architecture & Design

* **Dependency Injection (DI):**
  * **MUST** use constructor injection for all dependencies (services, configuration objects, loggers). [cite: api-server/Cookbook/Orders/OrderService.cs, api-server/Services/AI/LlmService.cs, api-server/Program.cs]
  * Define interfaces (`IXService`) for services to facilitate DI, mocking, and testing. [cite: api-server/Services/IFileService.cs, api-server/Cookbook/Recipes/IRecipeRepository.cs]
  * Register services in `Program.cs` (or relevant extension methods like `AddSessionManagement`, `AddIdentityServices`) with the appropriate lifetime (Singleton, Scoped, Transient), following existing examples. [cite: api-server/Program.cs, api-server/Services/Sessions/SessionExtensions.cs]
* **File Structure:** Maintain the established project structure (e.g., `/Services/`, `/Cookbook/Recipes/`, `/Services/Auth/Commands/`). Place new files in the logically correct directory based on their domain and function.
* **Configuration:**
  * Define configuration sections in dedicated `XConfig` classes implementing `IConfig`. [cite: api-server/Cookbook/Recipes/RecipeModels.cs, api-server/Services/Auth/AuthConfigurations.cs]
  * Register and bind configuration in `Program.cs` using `ConfigurationExtensions.RegisterConfigurationServices`. [cite: api-server/Program.cs, api-server/Config/ConfigurationExtensions.cs]
  * Inject specific configuration classes (e.g., `RecipeConfig`, `JwtSettings`) via constructor DI where needed. [cite: api-server/Cookbook/Recipes/RecipeRepository.cs, api-server/Services/Auth/AuthService.cs]

## 4. Asynchronous Programming

* **MUST** use `async`/`await` for all potentially blocking I/O operations (file access via `IFileService`, database calls via EF Core/Identity, HTTP requests via `HttpClient` or `RestClient`, `Channel` operations, `SemaphoreSlim.WaitAsync`). [cite: api-server/Services/FileService.cs, api-server/Services/Auth/Commands/LoginCommand.cs, api-server/Services/AI/LlmService.cs, api-server/Services/BrowserService.cs]
* **MUST NOT** use blocking calls like `.Result` or `.Wait()` on `Task` or `ValueTask`.
* Use `CancellationToken` where appropriate, especially in long-running operations or methods involved in request handling, background tasks, or parallel processing. [cite: api-server/Services/Sessions/SessionManager.cs, api-server/Cookbook/Recipes/RecipeRepository.cs]
* Understand and correctly use concurrency primitives like `Channel` (`BackgroundWorker`) and `SemaphoreSlim` (`BrowserService`, `FileService`, `SessionCleanupService`) as demonstrated in the existing codebase. [cite: api-server/Services/BackgroundWorker.cs, api-server/Services/BrowserService.cs, api-server/Services/Sessions/SessionCleanup.cs]

## 5. Error Handling & Logging

* **MUST** use `try`/`catch` blocks appropriately, especially around I/O, external API calls (OpenAI, GitHub, Stripe, Graph API), potentially failing parsing/deserialization, and concurrency operations.
* **MUST** log exceptions using the injected `ILogger<T>`. Use Serilog's structured logging capabilities (e.g., `_logger.LogError(ex, "Error processing order {OrderId} for customer {Email}", order.OrderId, order.Email)`). [cite: api-server/Program.cs, api-server/Cookbook/Orders/OrderService.cs, api-server/Services/FileService.cs]
* Utilize Polly retry policies (`_retryPolicy`) for transient errors (network issues, rate limits, temporary file locks) where appropriate, following patterns in `FileService`, `GitHubService`, `LlmService`. [cite: api-server/Services/FileService.cs, api-server/Services/GitHubService.cs, api-server/Services/AI/LlmService.cs]
* **MUST** validate critical method parameters. Use `ArgumentNullException.ThrowIfNull` for non-null parameters. Check for empty strings/collections where applicable. [cite: api-server/Services/BackgroundWorker.cs, api-server/Cookbook/Orders/OrderService.cs]
* Throw specific, appropriate exceptions where applicable (e.g., `ArgumentException`, `KeyNotFoundException`, `InvalidOperationException`, `NotExpectedException`, `InvalidEmailException`, `NoRecipeException`). This aids in upstream error handling. [cite: api-server/Cookbook/Recipes/RecipeService.cs, api-server/Services/Email/EmailService.cs]
* Use the custom `ApiErrorResult` or standard `BadRequest`, `NotFound` results in Controllers for appropriate client error responses. [cite: api-server/Controllers/CookbookController.cs, api-server/Controllers/AuthController.cs]

## 6. Null Handling

* **MUST** respect nullable reference types (`?`). The project has nullable context enabled.
* Check for `null` before dereferencing potentially null variables or properties.
* Use null-conditional (`?.`) and null-coalescing (`??`, `??=`) operators appropriately to handle potential nulls safely and concisely.

## 7. Data & Collections

* Use LINQ effectively for querying and manipulating collections where it enhances readability and conciseness.
* Prefer returning interfaces like `IReadOnlyList<T>` or `IEnumerable<T>` from methods when the caller should not modify the returned collection. Return `List<T>` when modification is intended or necessary for performance.
* Use thread-safe collections (`ConcurrentDictionary`, `ConcurrentBag`, `ConcurrentQueue`) when data structures are accessed or modified by multiple threads concurrently (e.g., `FileService._writeQueue`, `SessionManager.Sessions`, `RecipeIndexer._recipes`). [cite: api-server/Services/FileService.cs, api-server/Services/Sessions/SessionManager.cs, api-server/Cookbook/Recipes/RecipeIndexer.cs]

## 8. Resource Management

* **MUST** use `using` statements or `await using` for all `IDisposable` objects to ensure proper resource cleanup. This includes streams (`MemoryStream`, `FileStream`), `HttpClient`, `DbContext` instances obtained via scope, `SemaphoreSlim`, `ChannelReader/Writer`, and custom disposable services like `BrowserService`. [cite: api-server/Services/FileService.cs, api-server/Services/BrowserService.cs, api-server/Services/Auth/RefreshTokenCleanupService.cs]

## 9. Documentation, Diagrams & Testing

* **Code Comments:**
  * **MUST** write clear XML documentation comments (`/// <summary>...`) for all new or significantly modified public types (classes, interfaces, enums) and members (methods, properties). Follow existing examples for detail level.
  * Explain the **purpose (why)** and **usage/contract (how)**, not just *what* the code does literally. Document parameters (`<param>`) and return values (`<returns>`) clearly.
  * Use inline comments (`//`) sparingly, only to clarify particularly complex, non-obvious, or potentially confusing sections of logic.
* **Testing:**
  * **WHEN Code Changes Occur:** Any task (performed by human or AI) that modifies the code within a directory **MUST** also add or update relevant unit and/or integration tests within the same commit/change.
  * **HOW to Write Tests:** For specific standards, structure, frameworks, naming, and quality expectations for automated tests, you **MUST** consult and adhere to the **[`Docs/Standards/TestingStandards.md`](./TestingStandards.md)** document. [cite: Docs/Standards/TestingStandards.md]
* **README.md & Diagram Updates:**
  * **WHEN Code/Test Changes Impact Documentation/Diagrams:** Any task (performed by human or AI) that modifies the code or tests within a directory in a way that impacts its documented purpose, architecture, interface contracts, assumptions, dependencies, testing strategy, or **visualized architecture/flows** **MUST** also update the corresponding `README.md` file and any relevant Mermaid diagrams within the same commit/change.
  * **HOW to Update READMEs:** For the specific standards, structure, content guidelines, and linking strategy for `README.md` files, you **MUST** consult and adhere to the **[`Docs/Standards/DocumentationStandards.md`](./DocumentationStandards.md)** document. [cite: Docs/Standards/DocumentationStandards.md]
  * **HOW to Update Diagrams:** For the specific standards, diagram types, styling, linking, and maintenance requirements for Mermaid diagrams, you **MUST** consult and adhere to the **[`Docs/Standards/DiagrammingStandards.md`](./DiagrammingStandards.md)** document. [cite: Docs/Standards/DiagrammingStandards.md]

## 10. Security

* Be mindful of potential security vulnerabilities:
  * Validate and sanitize inputs, especially those originating from external sources (user input, API requests, scraped data).
  * Avoid constructing SQL queries or file paths directly from user input.
  * Ensure proper authorization checks are in place (`[Authorize]`, role checks).
* Do not log sensitive data like passwords, full API keys, or complete JWTs. Use masking where appropriate (see `LoggingMiddleware` for header masking). [cite: api-server/Config/LoggingMiddleware.cs]
* Handle API keys and secrets securely. Do not hardcode them; use configuration providers like User Secrets, Environment Variables, AWS Secrets Manager, or Parameter Store.
* Adhere to existing authentication (`[Authorize]`, `AuthenticationMiddleware`, `CookieAuthManager`) and authorization (`RoleManager`, `[Authorize(Roles = "...")]`) patterns. [cite: api-server/Controllers/CookbookController.cs, api-server/Services/Auth/AuthenticationMiddleware.cs]

## 11. External Libraries

* Utilize the existing libraries (e.g., Serilog, Polly, MediatR, AutoMapper, EF Core, OpenAI, Playwright, Octokit, QuestPDF, AngleSharp, RestSharp) according to established patterns within the codebase.
* Do not add new major dependencies (NuGet packages) without prior architectural review/approval.

## 12. AI Coder Specific Workflow (**MANDATORY**)

1.  **Consult Documentation First:** Before writing *any* code, **MUST** review:
  * The specific task prompt provided to you.
  * The `README.md` file in the primary directory/directories related to your task. Pay close attention to Section 3 (Interface Contract & Assumptions), Section 4 (Local Conventions), and any embedded diagrams.
  * This `CodingStandards.md` document.
  * The **[`Docs/Standards/TestingStandards.md`](./TestingStandards.md)** document for rules on writing tests.
  * The **[`Docs/Standards/DocumentationStandards.md`](./DocumentationStandards.md)** document for rules on updating READMEs.
  * The **[`Docs/Standards/DiagrammingStandards.md`](./DiagrammingStandards.md)** document for rules on updating diagrams. [cite: Docs/Standards/DiagrammingStandards.md]
2.  **Adhere to Standards:** Strictly follow all guidelines in this document and the local conventions noted in the relevant `README.md`. Match the style and patterns of the surrounding code.
3.  **Respect Contracts:** Do not violate the explicit or implicit contracts defined in the relevant `README.md` (Section 3) or visualized in diagrams.
4.  **Add/Update Tests:** Add or update unit/integration tests according to **[`Docs/Standards/TestingStandards.md`](./TestingStandards.md)** to cover your code changes.
5.  **Run Tests:** Run the tests you added/updated and ensure they pass. Run all unit tests (`dotnet test --filter "Category=Unit"`) and ensure they pass. Fix any failures.
6.  **Update Documentation & Diagrams:** If your code or test changes alter the module's documented aspects (purpose, architecture, interface, assumptions, dependencies, testing, visualized flows/structures, etc.), you **MUST**:
  * Update the relevant `README.md` file(s) following the rules in **[`Docs/Standards/DocumentationStandards.md`](./DocumentationStandards.md)**.
  * Update the relevant Mermaid diagram(s) following the rules in **[`Docs/Standards/DiagrammingStandards.md`](./DiagrammingStandards.md)**.
  * Update the `Last Updated:` date in the affected `README.md`.
7.  **Implement Thoroughly:** Ensure code includes necessary error handling, logging, parameter validation, and resource disposal (`using`).
8.  **XML Docs:** Write clear XML documentation comments for any new public members or types you create or modify significantly.

