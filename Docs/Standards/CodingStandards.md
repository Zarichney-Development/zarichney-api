# Project Coding Standards for AI Assistants

**Version:** 1.5
**Last Updated:** 2025-05-03

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
  * Use file-scoped namespaces (`namespace Zarichney.Services;`).
  * Utilize primary constructors where they simplify class definitions and initialization.
  * Use `record` types for immutable data transfer objects (DTOs) or simple data carriers where appropriate.
  * Keep methods concise and focused on a single responsibility. Aim for methods that fit on one screen.
  * Adhere to standard C# formatting (indentation, spacing, braces) primarily enforced by the project's `.editorconfig` file.
  * Prefer comparing 'Count' to 0 rather than using Any(). Instead of `if (!allAttributes.Any())` use `if (allAttributes.Count == 0)`

## 3. Architecture & Design

* **Dependency Injection (DI):**
  * **MUST** use constructor injection for all dependencies (services, configuration objects, loggers).
  * Define interfaces (`IXService`) for services to facilitate DI, mocking, and testing.
  * Register services in `Program.cs` (or relevant extension methods like `AddSessionManagement`, `AddIdentityServices`) with the appropriate lifetime (Singleton, Scoped, Transient), following existing examples.
* **File Structure:** Maintain the established project structure (e.g., `/Services/`, `/Cookbook/Recipes/`, `/Services/Auth/Commands/`). Place new files in the logically correct directory based on their domain and function.
* **Configuration:**
  * Define configuration sections in dedicated `XConfig` classes implementing `IConfig`.
  * Register and bind configuration in `Program.cs` using `ConfigurationExtensions.RegisterConfigurationServices`.
  * Inject specific configuration classes (e.g., `RecipeConfig`, `JwtSettings`) via constructor DI where needed.
  * **Validation Strategy:** Missing required configuration values (marked `[Required]` or using the placeholder `recommended to set in app secrets`) will generate warnings during startup but **will not prevent the application from starting**. Services **MUST** check for null or placeholder values at runtime and throw `ConfigurationMissingException` if they attempt to use a required configuration that is missing or invalid. (*Future Enhancement:* This may evolve towards disabling services instead of throwing runtime exceptions).
  * **API Endpoint Availability:** Controllers and actions that require specific features to be properly configured **SHOULD** be decorated with the `[DependsOnService]` attribute (e.g., `[DependsOnService("Llm")]`, `[DependsOnService("Payments")]`). This helps developers and API consumers understand which endpoints may be unavailable due to missing configuration by providing visual indications in the Swagger UI.

## 4. Asynchronous Programming

* **MUST** use `async`/`await` for all potentially blocking I/O operations (file access via `IFileService`, database calls via EF Core/Identity, HTTP requests via `HttpClient` or `RestClient`, `Channel` operations, `SemaphoreSlim.WaitAsync`).
* **MUST NOT** use blocking calls like `.Result` or `.Wait()` on `Task` or `ValueTask`.
* Use `CancellationToken` where appropriate, especially in long-running operations or methods involved in request handling, background tasks, or parallel processing.
* Understand and correctly use concurrency primitives like `Channel` (`BackgroundWorker`) and `SemaphoreSlim` (`BrowserService`, `FileService`, `SessionCleanupService`) as demonstrated in the existing codebase.

## 5. Error Handling & Logging

* **MUST** use `try`/`catch` blocks appropriately, especially around I/O, external API calls (OpenAI, GitHub, Stripe, Graph API), potentially failing parsing/deserialization, and concurrency operations.
* **MUST** log exceptions using the injected `ILogger<T>`. Use Serilog's structured logging capabilities (e.g., `_logger.LogError(ex, "Error processing order {OrderId} for customer {Email}", order.OrderId, order.Email)`).
* Utilize Polly retry policies (`_retryPolicy`) for transient errors (network issues, rate limits, temporary file locks) where appropriate, following patterns in `FileService`, `GitHubService`, `LlmService`.
* **MUST** validate critical method parameters. Use `ArgumentNullException.ThrowIfNull` for non-null parameters. Check for empty strings/collections where applicable.
* Throw specific, appropriate exceptions where applicable (e.g., `ArgumentException`, `KeyNotFoundException`, `InvalidOperationException`, `NotExpectedException`, `InvalidEmailException`, `NoRecipeException`, `ConfigurationMissingException`). This aids in upstream error handling.
* Use the custom `ApiErrorResult` or standard `BadRequest`, `NotFound` results in Controllers for appropriate client error responses.

## 6. Null Handling

* **MUST** respect nullable reference types (`?`). The project has nullable context enabled.
* Check for `null` before dereferencing potentially null variables or properties.
* Use null-conditional (`?.`) and null-coalescing (`??`, `??=`) operators appropriately to handle potential nulls safely and concisely.

## 7. Data & Collections

* Use LINQ effectively for querying and manipulating collections where it enhances readability and conciseness.
* Prefer returning interfaces like `IReadOnlyList<T>` or `IEnumerable<T>` from methods when the caller should not modify the returned collection. Return `List<T>` when modification is intended or necessary for performance.
* Use thread-safe collections (`ConcurrentDictionary`, `ConcurrentBag`, `ConcurrentQueue`) when data structures are accessed or modified by multiple threads concurrently (e.g., `FileService._writeQueue`, `SessionManager.Sessions`, `RecipeIndexer._recipes`).
* Prefer collection expressions. Use `["stringVal"]` instead of `new List<string> { "stringVal" }`.

## 8. Resource Management

* **MUST** use `using` statements or `await using` for all `IDisposable` objects to ensure proper resource cleanup. This includes streams (`MemoryStream`, `FileStream`), `HttpClient`, `DbContext` instances obtained via scope, `SemaphoreSlim`, `ChannelReader/Writer`, and custom disposable services like `BrowserService`.

## 9. Documentation, Diagrams & Testing

* **Code Comments:**
  * **MUST** write clear XML documentation comments (`/// <summary>...`) for all new or significantly modified public types (classes, interfaces, enums) and members (methods, properties). Follow existing examples for detail level.
  * Explain the **purpose (why)** and **usage/contract (how)**, not just *what* the code does literally. Document parameters (`<param>`) and return values (`<returns>`) clearly.
  * Use inline comments (`//`) sparingly, only to clarify particularly complex, non-obvious, or potentially confusing sections of logic.
* **Testing:**
  * **WHEN Code Changes Occur:** Any task (performed by human or AI) that modifies the code within a directory **MUST** also add or update relevant unit and/or integration tests within the same commit/change.
  * **HOW to Write Tests:** For specific standards, structure, frameworks, naming, and quality expectations for automated tests, you **MUST** consult and adhere to the **[`/Docs/Standards/TestingStandards.md`](./TestingStandards.md)** document.
* **README.md & Diagram Updates:**
  * **WHEN Code/Test Changes Impact Documentation/Diagrams:** Any task (performed by human or AI) that modifies the code or tests within a directory in a way that impacts its documented purpose, architecture, interface contracts, assumptions, dependencies, testing strategy, or **visualized architecture/flows** **MUST** also update the corresponding `README.md` file and any relevant Mermaid diagrams within the same commit/change.
  * **HOW to Update READMEs:** For the specific standards, structure, content guidelines, and linking strategy for `README.md` files, you **MUST** consult and adhere to the **[`/Docs/Standards/DocumentationStandards.md`](./DocumentationStandards.md)** document.
  * **HOW to Update Diagrams:** For the specific standards, diagram types, styling, linking, and maintenance requirements for Mermaid diagrams, you **MUST** consult and adhere to the **[`/Docs/Standards/DiagrammingStandards.md`](./DiagrammingStandards.md)** document.

## 10. Style Guide
* **Spacing:** Use spaces, not tabs, with an indentation size of two spaces.
* **Formatting:** Primarily rely on the project's `.editorconfig` file for detailed formatting rules. Ensure code formatting aligns with these rules.

## 11. Security

* Be mindful of potential security vulnerabilities:
  * Validate and sanitize inputs, especially those originating from external sources (user input, API requests, scraped data).
  * Avoid constructing SQL queries or file paths directly from user input.
  * Ensure proper authorization checks are in place (`[Authorize]`, role checks).
* Do not log sensitive data like passwords, full API keys, or complete JWTs. Use masking where appropriate (see `LoggingMiddleware` for header masking).
* Handle API keys and secrets securely. Do not hardcode them; use configuration providers like User Secrets, Environment Variables, AWS Secrets Manager, or Parameter Store.
* Adhere to existing authentication (`[Authorize]`, `AuthenticationMiddleware`, `CookieAuthManager`) and authorization (`RoleManager`, `[Authorize(Roles = "...")]`) patterns.

## 12. External Libraries

* Utilize the existing libraries (e.g., Serilog, Polly, MediatR, AutoMapper, EF Core, OpenAI, Playwright, Octokit, QuestPDF, AngleSharp, RestSharp) according to established patterns within the codebase.
* Do not add new major dependencies (NuGet packages) without prior architectural review/approval.

## 13. AI Coder Specific Workflow (**MANDATORY**)

*(Section Removed - Superseded by explicit steps in `/Docs/Templates/AICoderPromptTemplate.md`)*

---
