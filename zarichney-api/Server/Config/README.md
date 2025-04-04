# Module/Directory: Server/Config

**Last Updated:** 2025-04-03

> **Parent:** [`Server`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory provides shared configuration models, core middleware components, and utilities used across the application. Configuration registration logic and `PromptBase` have been moved to the `Server/Startup` and `Server/Services/AI` modules respectively. [cite: zarichney-api/Server/Config/ConfigModels.cs, zarichney-api/Server/Config/ErrorHandlingMiddleware.cs, zarichney-api/Server/Config/LoggingMiddleware.cs]
* **Key Responsibilities:**
    * Defining shared configuration models (e.g., `ServerConfig`, `ClientConfig`) identified by the `IConfig` interface. [cite: zarichney-api/Server/Config/ConfigModels.cs]
    * Providing core middleware for request/response logging and global error handling. [cite: zarichney-api/Server/Config/LoggingMiddleware.cs, zarichney-api/Server/Config/ErrorHandlingMiddleware.cs]
    * Providing custom utilities like JSON converters and exception types. [cite: zarichney-api/Server/Config/JsonConverter.cs, zarichney-api/Server/Config/NotExpectedException.cs]
    * Supplying filters for external integrations like Swagger (`FormFileOperationFilter`). [cite: zarichney-api/Server/Config/FormFileOperationFilter.cs]
    * Hosting static configuration files like `site_selectors.json`. [cite: zarichney-api/Server/Config/site_selectors.json]
* **Why it exists:** To define shared configuration models, provide reusable cross-cutting concerns via middleware, and centralize utilities needed by multiple modules.

## 2. Architecture & Key Concepts

* **Configuration Loading:** The actual loading and registration of configuration is now handled by the `Server/Startup/Configuration` module. This module focuses on defining the configuration models and the `IConfig` interface. [cite: zarichney-api/Server/Startup/Configuration/ConfigurationStartup.cs]
* **Strongly-Typed Configuration:** Configuration is accessed via injected, strongly-typed `XConfig` objects (e.g., `LlmConfig`, `RecipeConfig`). [cite: zarichney-api/Server/Cookbook/Recipes/RecipeService.cs, zarichney-api/Server/Services/AI/LlmService.cs]
* **Middleware:** Standard ASP.NET Core middleware pattern is used for logging (`LoggingMiddleware`) and error handling (`ErrorHandlingMiddleware`). Their order of registration in the application pipeline is important. [cite: zarichney-api/Server/Startup/App/ApplicationStartup.cs, zarichney-api/Server/Config/LoggingMiddleware.cs, zarichney-api/Server/Config/ErrorHandlingMiddleware.cs]
* **Static Configuration Files:** Files like `site_selectors.json` contain data loaded and used by specific services (`WebScraperService`), although the file itself resides within this config directory for organization. [cite: zarichney-api/Server/Config/site_selectors.json, zarichney-api/Server/Cookbook/Recipes/WebScraperService.cs]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `IConfig`: Marker interface for configuration classes automatically registered by the system.
    * Middleware (`ErrorHandlingMiddleware`, `LoggingMiddleware`): Consumed implicitly via registration in the application pipeline. They alter the request pipeline behavior.
* **Assumptions:**
    * **Configuration Registration:** Assumes configuration is registered via the `RegisterConfigurationServices` method in `Server/Startup/Configuration/ConfigurationStartup.cs`.
    * **Middleware Registration:** Assumes the middleware components (`UseMiddleware<T>`) are registered in the correct order in the application pipeline to function as intended (e.g., ErrorHandling typically early, Logging often early). [cite: zarichney-api/Server/Startup/App/ApplicationStartup.cs]
    * **File Existence:** Assumes `site_selectors.json` exists at the expected path relative to the application root when needed by services consuming it (like `WebScraperService`). [cite: zarichney-api/Server/Cookbook/Recipes/WebScraperService.cs]

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration Class Naming:** Classes holding configuration sections must implement `IConfig` and typically end with the suffix `Config` (e.g., `RecipeConfig`). [cite: zarichney-api/Server/Config/ConfigModels.cs]
* **Required Configuration:** Properties marked with `[Required]` attribute in `IConfig` classes will cause startup failure if missing or left as placeholder values. [cite: zarichney-api/Server/Startup/Configuration/ConfigurationStartup.cs]
* **Middleware Order:** The order in which `LoggingMiddleware` and `ErrorHandlingMiddleware` are added in the application pipeline affects their behavior (e.g., whether errors are logged before the error handler formats the response). [cite: zarichney-api/Server/Startup/App/ApplicationStartup.cs]
* **Static JSON Files:** Format and schema of `site_selectors.json` are specific to the needs of the `WebScraperService`.

## 5. How to Work With This Code

* **Adding New Configuration:**
    1.  Create a new class `MyNewConfig : IConfig` in an appropriate module (or here if truly generic). [cite: zarichney-api/Server/Startup/Configuration/ConfigurationStartup.cs]
    2.  Add properties matching the desired configuration structure.
    3.  Add a corresponding section (e.g., `"MyNewConfig": { ... }`) to `appsettings.json` or other config sources.
    4.  The `RegisterConfigurationServices` extension in `Server/Startup/Configuration/ConfigurationStartup.cs` will automatically handle binding, validation, and DI registration.
    5.  Inject `MyNewConfig` via constructor DI where needed.
* **Adding New Middleware:**
    1.  Create a new middleware class following the ASP.NET Core pattern (constructor with `RequestDelegate`, `InvokeAsync` method).
    2.  Register the middleware using `app.UseMiddleware<MyNewMiddleware>()` in `Server/Startup/App/ApplicationStartup.cs` at the appropriate stage in the request pipeline.
* **Testing:**
    * **Configuration:** Testing services that consume config usually involves creating mock `IOptions<T>` or directly instantiating config objects with test values.
    * **Middleware:** Often requires integration tests using `WebApplicationFactory` or specialized middleware testing harnesses to mock `HttpContext` and `RequestDelegate`.
* **Common Pitfalls / Gotchas:** Missing required configuration values causing startup failure. Incorrect middleware registration order leading to unexpected behavior (e.g., errors not being logged). Invalid format in `site_selectors.json` causing scraping failures.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Server/Startup/Configuration`](../Startup/Configuration/) - For registration of configuration.
    * [`Server/Startup/App`](../Startup/App/) - For registration of middleware.
* **External Library Dependencies:**
    * `Microsoft.Extensions.Configuration.Abstractions`, `.Binder`, `.Json`: Core .NET configuration handling.
    * `Microsoft.Extensions.Options`: Used for the options pattern with configuration.
    * `Microsoft.AspNetCore.Http.Abstractions`: Used by middleware components.
    * `Serilog`: Used by logging middleware.
    * `Swashbuckle.AspNetCore.SwaggerGen`: Used by `FormFileOperationFilter`.
    * `System.Text.Json`: Used for JSON handling within utilities.
* **Dependents (Impact of Changes):**
    * [`Server/Startup`](../Startup/README.md): Uses the configuration models and middleware components defined here.
    * Almost all other `Server/*` modules: Consume configuration objects (e.g., [`Server/Services/Auth`](../Services/Auth/README.md), [`Server/Cookbook/Recipes`](../Cookbook/Recipes/README.md), [`Server/Services/AI`](../Services/AI/README.md)).
    * [`Server/Cookbook/Recipes/WebScraperService.cs`](../Cookbook/Recipes/WebScraperService.cs): Consumes `site_selectors.json` hosted here.
    * Any code relying on global error handling or request logging provided by the middleware.

## 7. Rationale & Key Historical Context

* **Refactored Configuration Registration:** The configuration registration logic has been moved to the `Server/Startup/Configuration` module to centralize all startup-related code and improve the organization of the codebase.
* **`IConfig` Interface:** Used as a marker interface for automatic discovery and registration of configuration classes.
* **Middleware for Cross-Cutting Concerns:** Logging and Error Handling are implemented as middleware to apply these concerns globally without cluttering individual controller actions or services.
* **PromptBase Relocation:** The `PromptBase` class has been moved to `Server/Services/AI` where it more logically belongs with other AI-related functionality.

## 8. Known Issues & TODOs

* The validation in `ConfigurationStartup.ValidateAndReplaceProperties` currently only checks for `null` or a specific placeholder string for `[Required]` properties; more sophisticated validation (e.g., range checks, regex) could be added if needed.
* `site_selectors.json` parsing within `WebScraperService` could benefit from more robust error handling or schema validation upon loading.
* Error Handling middleware provides a generic error response; could be enhanced to provide slightly more specific (but still safe) error details based on exception types in non-production environments.