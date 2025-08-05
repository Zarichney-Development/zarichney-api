# Project Coding Standards for AI Assistants

**Version:** 1.7
**Last Updated:** 2025-05-25

## 1. Core Principles

* **Clarity:** Write code that is easy for others (human or AI) to understand. Prioritize readability.
* **Consistency:** Follow the patterns and conventions already established in this codebase. When in doubt, match the style of surrounding code. Refer to existing services, controllers, and configuration classes as examples.
* **Correctness:** Ensure code behaves as intended and meets requirements specified in the task prompt and associated `README.md` files. Verify against documented contracts and diagrams.
* **Robustness:** Write code that anticipates and handles potential errors and edge cases gracefully. Validate inputs and manage external dependencies reliably.
* **Maintainability:** Create code that is easy to debug, modify, extend, and **test**. Favor modularity and clear separation of concerns. Ensure documentation and diagrams are kept current.
* **Design for Testability (NEW):**
    * Code **must** be designed with testability as a primary consideration from the outset. This includes favoring pure functions, minimizing side effects, ensuring clear separation of concerns, and applying patterns that facilitate isolated testing.
    * Consider how a unit of code will be tested *before* and *during* its implementation.
    * If a piece of code seems difficult to test, it's often a signal that its design can be improved (e.g., by breaking it down further, abstracting dependencies, or applying patterns like the Humble Object Pattern).
    * All code, including AI-generated code, must adhere to these standards to facilitate effective automated testing as outlined in `Docs/Standards/TestingStandards.md` and its subsidiary unit (`Docs/Standards/UnitTestCaseDevelopment.md`) and integration (`Docs/Standards/IntegrationTestCaseDevelopment.md`) testing guides.

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
    * Use `record` types for immutable data transfer objects (DTOs) or simple data carriers where appropriate. This promotes immutability, which simplifies testing.
    * Keep methods concise and focused on a single responsibility (SRP). Aim for methods that fit on one screen.
    * Prefer comparing 'Count' to 0 rather than using `Any()`. Instead of `if (!allAttributes.Any())` use `if (allAttributes.Count == 0)`.
    * **Embrace Immutability (NEW):** Where practical, prefer immutable data structures and classes (e.g., using `readonly` fields/properties, `record` types, read-only collections). Objects that don't change state after creation are inherently simpler to reason about and test.
    * **Pure Functions (NEW):** Strive to write pure functions (functions whose output depends only on their input arguments and which have no side effects) for business logic where possible. Pure functions are the easiest to test due to their deterministic nature and lack of reliance on external state.
    * **Avoid static classes and members for services/logic (Reinforced):** Core business logic and services **must** be instance-based to allow for dependency injection and mocking. Static helper methods are acceptable for genuinely stateless utility functions that do not involve external dependencies or complex logic requiring substitution in tests.

## 3. Architecture & Design

* **Dependency Injection (DI):**
    * **MUST** use constructor injection for all dependencies (services, configuration objects, loggers). This makes dependencies explicit and facilitates easier testing.
    * Define interfaces (`IXService`) for services to facilitate DI, mocking, and testing by allowing for test double substitution.
    * Register services in `Program.cs` (or relevant extension methods like `AddSessionManagement`, `AddIdentityServices`) with the appropriate lifetime (Singleton, Scoped, Transient), following existing examples. Be mindful of service lifetimes and their implications for state management and test setup. Incorrect lifetimes can lead to hard-to-debug issues in both production and test environments.
    * **Avoid Service Locator Anti-Pattern (NEW):** Explicitly avoid using the Service Locator pattern (e.g., directly calling `IServiceProvider.GetService<T>()` within application logic). Dependencies **must** be made explicit through constructor injection.
* **SOLID Principles for Testability (NEW):**
    * Adherence to SOLID principles is crucial for creating maintainable and testable code:
        * **Single Responsibility Principle (SRP):** Classes should have only one reason to change. This leads to smaller, focused classes that are easier to understand, maintain, and unit test in isolation.
        * **Open/Closed Principle (OCP):** Software entities should be open for extension but closed for modification. This often involves using abstractions and patterns that allow adding new functionality without altering existing, tested code, thus preserving test stability.
        * **Liskov Substitution Principle (LSP):** Subtypes must be substitutable for their base types without altering the correctness of the program. This ensures that test doubles (mocks or fakes) based on an interface or base class will behave predictably and consistently with the contract.
        * **Interface Segregation Principle (ISP):** Clients should not be forced to depend on interfaces they do not use. Define "lean" interfaces specific to client needs. This makes mocking simpler and more focused, as mocks only need to implement relevant methods. Avoid "fat" interfaces.
        * **Dependency Inversion Principle (DIP):** Depend on abstractions, not concretions. (This is the foundation of DI, covered above).
* **File Structure:** Maintain the established project structure (e.g., `/Services/`, `/Cookbook/Recipes/`, `/Services/Auth/Commands/`). Place new files in the logically correct directory based on their domain and function.
* **Configuration:**
    * Define configuration sections in dedicated `XConfig` classes implementing `IConfig`.
    * Register and bind configuration in `Program.cs` using `ConfigurationExtensions.RegisterConfigurationServices`.
    * Inject specific configuration classes (e.g., `RecipeConfig`, `JwtSettings`) via constructor DI where needed.
    * **Validation Strategy:** Missing required configuration values (marked `[Required]` or using the placeholder `recommended to set in app secrets`) will generate warnings during startup but **will not prevent the application from starting**. Services **MUST** check for null or placeholder values at runtime and throw `ConfigurationMissingException` if they attempt to use a required configuration that is missing or invalid.
    * **API Endpoint Availability:** Controllers and actions that require specific features to be properly configured **SHOULD** be decorated with the `[DependsOnService]` attribute (e.g., `[DependsOnService("Llm")]`, `[DependsOnService("Payments")]`). This helps developers and API consumers understand which endpoints may be unavailable due to missing configuration by providing visual indications in the Swagger UI.

## 4. Designing Testable Application Logic (NEW)

* **The Humble Object Pattern:**
    * For components interacting with difficult-to-test infrastructure (e.g., API controllers handling HTTP concerns, classes directly using `DbContext` extensively for complex operations), apply the Humble Object pattern.
    * **Separate complex business logic** into testable services, command handlers (e.g., MediatR), or query handlers. These core logic units can then be easily unit-tested with mocked dependencies.
    * Keep "boundary" objects (like API controllers or classes directly interfacing with external libraries) "humble." Their responsibility should primarily be to:
        1.  Validate and map incoming requests.
        2.  Delegate processing to the testable core logic units.
        3.  Map results from core logic units to appropriate responses.
    * Controllers, for instance, should avoid containing complex conditional logic, direct database manipulation, or intricate interactions with external systems themselves. This makes the core application logic highly testable in isolation.
* **Clear API Contracts (for Controllers/Endpoints):**
    * Design API request and response models (DTOs) to be clear, concise, and focused on the specific interaction.
    * Avoid overly large or complex DTOs that span multiple concerns, as they can be difficult to construct in tests and may indicate a violation of SRP at the API level.
    * Ensure consistent and predictable API error responses (e.g., using `ApiErrorResult` or standard `ProblemDetails`).

## 5. Asynchronous Programming

* **MUST** use `async`/`await` for all potentially blocking I/O operations (file access via `IFileService`, database calls via EF Core/Identity, HTTP requests via `HttpClient` or `RestClient`, `Channel` operations, `SemaphoreSlim.WaitAsync`).
* **MUST NOT** use blocking calls like `.Result` or `.Wait()` on `Task` or `ValueTask` in application code, as this can lead to deadlocks.
* Use `CancellationToken` where appropriate, especially in long-running operations or methods involved in request handling, background tasks, or parallel processing. Pass `CancellationToken.None` explicitly if a token is not applicable but the called method accepts one.
* Understand and correctly use concurrency primitives like `Channel` (`BackgroundWorker`) and `SemaphoreSlim` (`BrowserService`, `FileService`, `SessionCleanupService`) as demonstrated in the existing codebase.
* **`ConfigureAwait(false)`:** In this ASP.NET Core application, the default synchronization context behavior is generally non-restrictive. Therefore, widespread use of `ConfigureAwait(false)` is typically not required in application-level code (controllers, services). It is more critical in general-purpose library code. If developing shared library-like components within this project that might be used in different synchronization contexts, consider using `ConfigureAwait(false)` to avoid potential deadlocks.

## 6. Error Handling & Logging

* **MUST** use `try`/`catch` blocks appropriately, especially around I/O, external API calls (OpenAI, GitHub, Stripe, Graph API), potentially failing parsing/deserialization, and concurrency operations.
* **MUST** log exceptions using the injected `ILogger<T>`. Use Serilog's structured logging capabilities (e.g., `_logger.LogError(ex, "Error processing order {OrderId} for customer {Email}", order.OrderId, order.Email)`).
* Utilize Polly retry policies (`_retryPolicy`) for transient errors (network issues, rate limits, temporary file locks) where appropriate, following patterns in `FileService`, `GitHubService`, `LlmService`.
* **MUST** validate critical method parameters. Use `ArgumentNullException.ThrowIfNull` for non-null parameters. Check for empty strings/collections where applicable.
* Throw specific, appropriate exceptions where applicable (e.g., `ArgumentException`, `KeyNotFoundException`, `InvalidOperationException`, `NotExpectedException`, `InvalidEmailException`, `NoRecipeException`, `ConfigurationMissingException`). This aids in upstream error handling and allows for more precise assertions in tests.
* Use the custom `ApiErrorResult` or standard `BadRequest`, `NotFound` results in Controllers for appropriate client error responses.

## 7. Null Handling

* **MUST** respect nullable reference types (`?`). The project has nullable context enabled.
* Check for `null` before dereferencing potentially null variables or properties.
* Use null-conditional (`?.`) and null-coalescing (`??`, `??=`) operators appropriately to handle potential nulls safely and concisely.

## 8. Data & Collections

* Use LINQ effectively for querying and manipulating collections where it enhances readability and conciseness. Avoid overly complex LINQ queries in a single statement; break them down for clarity and testability if necessary.
* Prefer returning interfaces like `IReadOnlyList<T>` or `IEnumerable<T>` from methods when the caller should not modify the returned collection. Return `List<T>` when modification is intended or necessary for performance.
* Use thread-safe collections (`ConcurrentDictionary`, `ConcurrentBag`, `ConcurrentQueue`) when data structures are accessed or modified by multiple threads concurrently (e.g., `FileService._writeQueue`, `SessionManager.Sessions`, `RecipeIndexer._recipes`).
* Prefer collection expressions. Use `["stringVal"]` instead of `new List<string> { "stringVal" }`.

## 9. Resource Management

* **MUST** use `using` statements or `await using` for all `IDisposable` objects to ensure proper resource cleanup. This includes streams (`MemoryStream`, `FileStream`), `HttpClient`, `DbContext` instances obtained via scope, `SemaphoreSlim`, `ChannelReader/Writer`, and custom disposable services like `BrowserService`.

## 10. Documentation, Diagrams & Testing

* **Code Comments:**
    * **MUST** write clear XML documentation comments (`/// <summary>...`) for all new or significantly modified public types (classes, interfaces, enums) and members (methods, properties). Follow existing examples for detail level.
    * Explain the **purpose (why)** and **usage/contract (how)**, not just *what* the code does literally. Document parameters (`<param>`) and return values (`<returns>`) clearly.
    * Use inline comments (`//`) sparingly, only to clarify particularly complex, non-obvious, or potentially confusing sections of logic.
* **Testing:**
    * **WHEN Code Changes Occur:** Any task (performed by human or AI) that modifies the code within a directory **MUST** also add or update relevant unit and/or integration tests within the same commit/change. This includes refactoring code for testability.
    * **Time Estimation Policy:** Time estimates are not required for AI coding tasks as AI execution timelines differ significantly from human developer estimates. Focus on complexity and priority rather than duration.
    * **HOW to Write Tests:** For specific standards, structure, frameworks, naming, and quality expectations for automated tests, you **MUST** consult and adhere to the following documents:
        * **`Docs/Standards/TestingStandards.md`** (Overarching testing principles)
        * **`Docs/Standards/UnitTestCaseDevelopment.md`** (Detailed unit testing guide)
        * **`Docs/Standards/IntegrationTestCaseDevelopment.md`** (Detailed integration testing guide)
    * **Refactoring for Testability (Reinforced):** Code **should be written with testability in mind from the start**. If existing code is identified as difficult to test, it **must be refactored** to improve testability as part of the task, adhering to the principles in this document (e.g., DI, SOLID, Humble Object). Consult the TDD (Section 14) for the mandate on this.
* **README.md & Diagram Updates:**
    * **WHEN Code/Test Changes Impact Documentation/Diagrams:** Any task (performed by human or AI) that modifies the code or tests within a directory in a way that impacts its documented purpose, architecture, interface contracts, assumptions, dependencies, testing strategy, or **visualized architecture/flows** **MUST** also update the corresponding `README.md` file and any relevant Mermaid diagrams within the same commit/change.
    * **HOW to Update READMEs:** For the specific standards, structure, content guidelines, and linking strategy for `README.md` files, you **MUST** consult and adhere to the **[`/Docs/Standards/DocumentationStandards.md`](./DocumentationStandards.md)** document.
    * **HOW to Update Diagrams:** For the specific standards, diagram types, styling, linking, and maintenance requirements for Mermaid diagrams, you **MUST** consult and adhere to the **[`/Docs/Standards/DiagrammingStandards.md`](./DiagrammingStandards.md)** document.

## 11. Testability Anti-Patterns to Avoid (NEW)

To ensure code remains highly testable, actively avoid the following common anti-patterns:
* **God Classes/Objects:** Classes that do too much, have too many dependencies, and are difficult to instantiate or mock in tests. Apply SRP to break them down.
* **Static Cling:** Overuse of static methods, properties, or classes for dependencies or stateful logic. These are hard to mock or substitute in tests. Prefer instance-based designs with DI. (This reinforces section 2 guideline).
* **Service Locator:** As mentioned in DI, avoid resolving dependencies dynamically from `IServiceProvider` within classes. This hides dependencies and complicates test setup.
* **Deeply Nested Object Graphs without Abstractions (Law of Demeter Violations):** Code like `A.getB().getC().getD().doSomething()` makes mocking `D` cumbersome and couples tests to the entire chain. Introduce interfaces or methods at intermediate points if necessary.
* **Overly Complex Constructors:** Constructors with excessive logic or too many parameters can make object creation difficult in tests. Consider using Factory patterns or refactoring the class.
* **Time-Dependent Logic without Abstraction:** Directly using `DateTime.Now` or `DateTime.UtcNow` makes code hard to test deterministically. Inject `System.TimeProvider` instead (see `Code/Zarichney.Server.Tests/TechnicalDesignDocument.md` FRMK-001).

## 12. Security

* Be mindful of potential security vulnerabilities:
    * Validate and sanitize inputs, especially those originating from external sources (user input, API requests, scraped data).
    * Avoid constructing SQL queries or file paths directly from user input.
    * Ensure proper authorization checks are in place (`[Authorize]`, role checks).
* Do not log sensitive data like passwords, full API keys, or complete JWTs. Use masking where appropriate (see `LoggingMiddleware` for header masking).
* Handle API keys and secrets securely. Do not hardcode them; use configuration providers like User Secrets, Environment Variables, AWS Secrets Manager, or Parameter Store.
* Adhere to existing authentication (`[Authorize]`, `AuthenticationMiddleware`, `CookieAuthManager`) and authorization (`RoleManager`, `[Authorize(Roles = "...")]`) patterns.

## 13. Logging Standards

* **Logger Injection:**
    * **MUST** use constructor injection with the generic `ILogger<T>` type (e.g., `private readonly ILogger<MyService> _logger;`). This provides type safety, easier testing, and automatic source context.
    * **MUST NOT** use the non-generic `ILogger` followed by `.ForContext<T>()` pattern. This creates unnecessary complexity and reduces testability.
    * **Example:**
        ```csharp
        // Correct
        public class MyService
        {
          private readonly ILogger<MyService> _logger;
          
          public MyService(ILogger<MyService> logger)
          {
            _logger = logger;
          }
        }
        
        // Incorrect
        public class MyService
        {
          private readonly ILogger _logger;
          
          public MyService(ILogger logger)
          {
            _logger = logger.ForContext<MyService>();
          }
        }
        ```
* **Log Message Formatting:**
    * **MUST** use structured logging with named parameters (e.g., `_logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", order.Id, customer.Id);`) instead of string interpolation in the message template.
    * **MUST** use `nameof()` for logging method names or key operations where appropriate (e.g., `_logger.LogWarning("{Method}: No email provided in order", nameof(CreateCookbook));`).
    * Write clear and concise log messages that explain the context and purpose, not just the action.
    * Avoid overly verbose or redundant information that can be inferred from the log context.
* **Structured Property Naming:**
    * **MUST** use PascalCase for structured logging property names (e.g., `{OrderId}`, `{CustomerId}`, `{FileName}`) for consistency with C# property naming conventions.
    * Use descriptive and meaningful property names that clearly indicate what the value represents.
* **Log Levels:**
    * **Verbose:** Very detailed tracing information, typically only enabled during debugging. Use sparingly.
    * **Debug:** Detailed information useful for debugging application flow. Should provide insights into application behavior without being overly verbose.
    * **Information:** General application flow information, important events, and successful operations. This is the primary level for tracking normal application behavior.
    * **Warning:** Indicates potential issues that don't prevent the application from functioning, recoverable errors, or unexpected but handled conditions.
    * **Error:** Error events that might still allow the application to continue running. Use for exceptions and failures that need attention.
    * **Fatal:** Very severe error events that will presumably lead to application abort. Use sparingly for catastrophic failures.
* **Logging Exceptions:**
    * **MUST** pass the exception object as the first argument to logging methods (e.g., `_logger.LogError(ex, "An error occurred while processing {Data}", someData);`) to ensure stack traces and full exception details are captured by Serilog.
    * Include relevant context information as structured parameters to aid in debugging and analysis.
    * Use appropriate log levels: `LogError` for unexpected exceptions, `LogWarning` for expected/handled exceptions that indicate potential issues.
* **Performance Considerations:**
    * Be mindful of log volume in production environments. Use appropriate log levels and avoid excessive logging in tight loops.
    * Prefer structured logging over string concatenation or interpolation for better performance and searchability.
    * Consider the performance impact of logging expensive-to-compute values and evaluate if the information is necessary at the chosen log level.

## 14. External Libraries

* Utilize the existing libraries (e.g., Serilog, Polly, MediatR, AutoMapper, EF Core, OpenAI, Playwright, Octokit, QuestPDF, AngleSharp, RestSharp) according to established patterns within the codebase.
* Do not add new major dependencies (NuGet packages) without prior architectural review/approval.

---
