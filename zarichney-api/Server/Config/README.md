# Module/Directory: Server/Config

**Last Updated:** 2025-04-03

> **Parent:** [`Server`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory provides centralized application configuration loading, validation, and registration, along with shared configuration models, core middleware components, base classes, and utilities used across the application. [cite: zarichney-api/Server/Config/ConfigurationExtensions.cs, zarichney-api/Server/Config/ConfigModels.cs, zarichney-api/Server/Config/ErrorHandlingMiddleware.cs, zarichney-api/Server/Config/LoggingMiddleware.cs, zarichney-api/Server/Config/PromptBase.cs]
* **Key Responsibilities:**
    * Defining shared configuration models (e.g., `ServerConfig`, `ClientConfig`) identified by the `IConfig` interface. [cite: zarichney-api/Server/Config/ConfigModels.cs, zarichney-api/Server/Config/ConfigurationExtensions.cs]
    * Loading configuration from various sources (`appsettings.json`, environment variables, AWS SSM/Secrets Manager) and binding them to strongly-typed configuration objects. [cite: zarichney-api/Server/Config/ConfigurationExtensions.cs, zarichney-api/Program.cs]
    * Validating required configuration properties at startup. [cite: zarichney-api/Server/Config/ConfigurationExtensions.cs]
    * Registering configuration objects for Dependency Injection. [cite: zarichney-api/Server/Config/ConfigurationExtensions.cs]
    * Providing core middleware for request/response logging and global error handling. [cite: zarichney-api/Server/Config/LoggingMiddleware.cs, zarichney-api/Server/Config/ErrorHandlingMiddleware.cs]
    * Defining base classes and utilities for shared patterns (e.g., `PromptBase` for LLM interactions). [cite: zarichney-api/Server/Config/PromptBase.cs]
    * Providing custom utilities like JSON converters and exception types. [cite: zarichney-api/Server/Config/JsonConverter.cs, zarichney-api/Server/Config/NotExpectedException.cs]
    * Supplying filters for external integrations like Swagger (`FormFileOperationFilter`). [cite: zarichney-api/Server/Config/FormFileOperationFilter.cs]
    * Hosting static configuration files like `site_selectors.json`. [cite: zarichney-api/Server/Config/site_selectors.json]
* **Why it exists:** To establish a consistent pattern for application configuration, provide reusable cross-cutting concerns via middleware, and centralize base types/utilities needed by multiple modules.

## 2. Architecture & Key Concepts

* **Configuration Loading:** Leverages .NET's `IConfiguration` and the Options pattern. Configuration sources are layered (appsettings -> environment -> AWS). [cite: zarichney-api/Program.cs]
* **Configuration Registration:** The `ConfigurationExtensions.RegisterConfigurationServices` method automatically discovers types implementing `IConfig`, binds configuration sections to them, performs validation, and registers them as singletons for DI. [cite: zarichney-api/Server/Config/ConfigurationExtensions.cs]
* **Strongly-Typed Configuration:** Configuration is accessed via injected, strongly-typed `XConfig` objects (e.g., `LlmConfig`, `RecipeConfig`). [cite: zarichney-api/Server/Cookbook/Recipes/RecipeService.cs, zarichney-api/Server/Services/AI/LlmService.cs]
* **Middleware:** Standard ASP.NET Core middleware pattern is used for logging (`LoggingMiddleware`) and error handling (`ErrorHandlingMiddleware`). Their order of registration in `Program.cs` is important. [cite: zarichney-api/Program.cs, zarichney-api/Server/Config/LoggingMiddleware.cs, zarichney-api/Server/Config/ErrorHandlingMiddleware.cs]
* **Base Classes:** `PromptBase` defines a standard structure for creating prompt definitions used with the `LlmService`. [cite: zarichney-api/Server/Config/PromptBase.cs, zarichney-api/Server/Cookbook/Prompts/SynthesizeRecipe.cs]
* **Static Configuration Files:** Files like `site_selectors.json` contain data loaded and used by specific services (`WebScraperService`), although the file itself resides within this config directory for organization. [cite: zarichney-api/Server/Config/site_selectors.json, zarichney-api/Server/Cookbook/Recipes/WebScraperService.cs]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `IConfig`: Marker interface for configuration classes automatically registered by the system.
    * `ConfigurationExtensions.RegisterConfigurationServices(IServiceCollection, IConfiguration)`: Extension method called during startup in `Program.cs` to set up all `IConfig` objects.
    * `PromptBase`: Abstract class to be inherited by specific prompt implementations in other modules. [cite: zarichney-api/Server/Config/PromptBase.cs]
    * Middleware (`ErrorHandlingMiddleware`, `LoggingMiddleware`): Consumed implicitly via registration in `Program.cs`. They alter the request pipeline behavior.
* **Assumptions:**
    * **Configuration Sources:** Assumes configuration values are provided correctly through `appsettings.json`, environment variables, user secrets, or external providers (like AWS SSM/Secrets Manager) before `RegisterConfigurationServices` is called.
    * **DI Availability:** Assumes `IConfiguration` is available in the service collection when `RegisterConfigurationServices` runs. Assumes standard ASP.NET Core DI is set up.
    * **Middleware Registration:** Assumes the middleware components (`UseMiddleware<T>`) are registered in the correct order in `Program.cs` to function as intended (e.g., ErrorHandling typically early, Logging often early). [cite: zarichney-api/Program.cs]
    * **File Existence:** Assumes `site_selectors.json` exists at the expected path relative to the application root when needed by services consuming it (like `WebScraperService`). [cite: zarichney-api/Server/Cookbook/Recipes/WebScraperService.cs]

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration Class Naming:** Classes holding configuration sections must implement `IConfig` and typically end with the suffix `Config` (e.g., `RecipeConfig`). [cite: zarichney-api/Server/Config/ConfigurationExtensions.cs, zarichney-api/Server/Config/ConfigModels.cs]
* **Configuration Loading:** Relies entirely on the automated process in `ConfigurationExtensions`. Manual binding or registration of `IConfig` types is generally not needed.
* **Required Configuration:** Properties marked with `[Required]` attribute in `IConfig` classes will cause startup failure if missing or left as placeholder values. [cite: zarichney-api/Server/Config/ConfigurationExtensions.cs]
* **Middleware Order:** The order in which `LoggingMiddleware` and `ErrorHandlingMiddleware` are added in `Program.cs` affects their behavior (e.g., whether errors are logged before the error handler formats the response). [cite: zarichney-api/Program.cs]
* **Static JSON Files:** Format and schema of `site_selectors.json` are specific to the needs of the `WebScraperService`.

## 5. How to Work With This Code

* **Adding New Configuration:**
    1.  Create a new class `MyNewConfig : IConfig` in an appropriate module (or here if truly generic). [cite: zarichney-api/Server/Config/ConfigurationExtensions.cs]
    2.  Add properties matching the desired configuration structure.
    3.  Add a corresponding section (e.g., `"MyNewConfig": { ... }`) to `appsettings.json` or other config sources.
    4.  The `RegisterConfigurationServices` extension in `Program.cs` will automatically handle binding, validation, and DI registration.
    5.  Inject `MyNewConfig` via constructor DI where needed.
* **Adding New Middleware:**
    1.  Create a new middleware class following the ASP.NET Core pattern (constructor with `RequestDelegate`, `InvokeAsync` method).
    2.  Register the middleware using `app.UseMiddleware<MyNewMiddleware>()` in `Program.cs` at the appropriate stage in the request pipeline.
* **Testing:**
    * **Configuration:** Testing services that consume config usually involves creating mock `IOptions<T>` or directly instantiating config objects with test values.
    * **Middleware:** Often requires integration tests using `WebApplicationFactory` or specialized middleware testing harnesses to mock `HttpContext` and `RequestDelegate`.
* **Common Pitfalls / Gotchas:** Missing required configuration values causing startup failure. Incorrect middleware registration order leading to unexpected behavior (e.g., errors not being logged). Invalid format in `site_selectors.json` causing scraping failures.

## 6. Dependencies

* **Internal Code Dependencies:** None (This module is foundational).
* **External Library Dependencies:**
    * `Microsoft.Extensions.Configuration.Abstractions`, `.Binder`, `.Json`: Core .NET configuration handling.
    * `Microsoft.Extensions.Options`: Used for the options pattern with configuration.
    * `Microsoft.AspNetCore.Http.Abstractions`: Used by middleware components.
    * `Serilog`: Used by logging middleware.
    * `Swashbuckle.AspNetCore.SwaggerGen`: Used by `FormFileOperationFilter`.
    * `System.Text.Json`: Used for JSON handling within utilities/prompts.
    * `OpenAI` (Indirect): Referenced by `PromptBase` and its dependents.
* **Dependents (Impact of Changes):**
    * `Program.cs`: Heavily relies on this module for configuration registration and middleware setup.
    * Almost all other `Server/*` modules: Consume configuration objects (e.g., [`Server/Services/Auth`](../Auth/README.md), [`Server/Cookbook/Recipes`](../Cookbook/Recipes/README.md), [`Server/Services/AI`](../Services/AI/README.md)) registered by this module.
    * [`Server/Cookbook/Recipes/WebScraperService.cs`](../Cookbook/Recipes/WebScraperService.cs): Consumes `site_selectors.json` hosted here.
    * Any code relying on global error handling or request logging provided by the middleware.

## 7. Rationale & Key Historical Context

* **Centralized Configuration (`ConfigurationExtensions`):** Adopted to simplify `Program.cs` setup, ensure consistent configuration loading/validation across all modules, and make adding new configuration sections straightforward.
* **`IConfig` Interface:** Used as a marker interface for automatic discovery and registration of configuration classes.
* **`PromptBase` Class:** Created to establish a common structure and required elements for defining interactions with Language Models (LLMs), promoting consistency in prompt engineering across different features.
* **Middleware for Cross-Cutting Concerns:** Logging and Error Handling are implemented as middleware to apply these concerns globally without cluttering individual controller actions or services.

## 8. Known Issues & TODOs

* The validation in `ConfigurationExtensions` currently only checks for `null` or a specific placeholder string for `[Required]` properties; more sophisticated validation (e.g., range checks, regex) could be added if needed.
* `site_selectors.json` parsing within `WebScraperService` could benefit from more robust error handling or schema validation upon loading.
* Error Handling middleware provides a generic error response; could be enhanced to provide slightly more specific (but still safe) error details based on exception types in non-production environments.