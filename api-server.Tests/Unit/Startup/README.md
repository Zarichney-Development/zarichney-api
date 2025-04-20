# Module/Directory: /Unit/Startup

**Last Updated:** 2025-04-18

> **Parent:** [`Unit`](../README.md)
> **Related:**
> * **Startup Sources:**
>   * [`Startup/ApplicationStartup.cs`](../../../api-server/Startup/ApplicationStartup.cs)
>   * [`Startup/AuthenticationStartup.cs`](../../../api-server/Startup/AuthenticationStartup.cs)
>   * [`Startup/ConfigurationStartup.cs`](../../../api-server/Startup/ConfigurationStartup.cs)
>   * [`Startup/MiddlewareStartup.cs`](../../../api-server/Startup/MiddlewareStartup.cs)
>   * [`Startup/ServiceStartup.cs`](../../../api-server/Startup/ServiceStartup.cs)
>   * [`Program.cs`](../../../api-server/Program.cs) (Entry point using startup logic)
> * **Standards:** [`TestingStandards.md`](../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests specifically designed to validate the application's **startup configuration logic in isolation**. These tests ensure that dependency injection (DI) services, configuration options, authentication schemes, and other application settings are registered and configured correctly by the various startup extension methods or classes.

* **Why Unit Tests for Startup?** To catch configuration errors, missing service registrations, or incorrect DI lifetimes *before* running the application. This provides fast feedback and helps prevent runtime failures caused by setup issues. These tests verify the *intent* of the configuration code without needing to build or run the full application host.
* **Isolation:** Tests operate directly on `IServiceCollection`, `IConfigurationBuilder`, `WebApplicationBuilder`, etc., without involving HTTP pipelines, databases, or external services.

## 2. Scope & Key Functionality Tested (What?)

These tests focus on verifying the outcome of calling specific configuration methods:

* **`ConfigurationStartup` (`ConfigureAppConfiguration`, `AddAppConfiguration`):**
    * Verification that necessary configuration sources (e.g., `appsettings.json`, environment variables, user secrets) are added.
    * Verification that strongly-typed options classes (e.g., `JwtSettings`, `StripeSettings`, `OpenAISettings`) are correctly configured, bound from `IConfiguration`, and potentially validated using data annotations or `IValidateOptions`.
* **`ServiceStartup` (`AddApplicationServices`):**
    * Verification that application services (e.g., `IOrderService`, `IPaymentService`, `ILlmService`, `IEmailService`, Repositories, MediatR handlers, AutoMapper profiles) are registered in the `IServiceCollection`.
    * Verification that services are registered with the correct **lifetime** (Singleton, Scoped, Transient).
* **`AuthenticationStartup` (`AddAppAuthentication`):**
    * Verification that ASP.NET Core Identity services are configured.
    * Verification that necessary authentication schemes (JWT Bearer, Cookie, potentially custom schemes like API Key) are added and configured correctly.
    * Verification that authorization policies are registered.
* **`ApplicationStartup` (`ConfigureApp` - parts usable without `IApplicationBuilder`):**
    * Verification of CORS policy configuration settings.
    * Verification of routing option configurations.
* **`MiddlewareStartup` (`ConfigureMiddleware`):**
    * Unit testing middleware registration is less common, as order and execution are better tested via integration tests. However, if complex helper methods are used *during* registration, they could potentially be unit tested here.

## 3. Test Environment Setup

* **Core Objects:** Tests typically start by creating instances of:
    * `Microsoft.Extensions.DependencyInjection.ServiceCollection`
    * `Microsoft.Extensions.Configuration.ConfigurationBuilder` (often populated with `AddInMemoryCollection`)
    * `Microsoft.AspNetCore.Builder.WebApplicationBuilder` (provides access to Services, Configuration, etc.)
* **Execution:** The specific startup extension method under test is called, passing in the appropriate builder or collection object (e.g., `services.AddApplicationServices(configuration)`).
* **Assertion:** Assertions are made against the state of the `ServiceCollection` (checking for registered types and lifetimes), the built `IConfiguration` object (checking if values are loaded), or the options retrieved from the `ServiceProvider` built from the `ServiceCollection`. Libraries like `FluentAssertions` are useful here.

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Organize tests logically, likely mirroring the startup classes (`ConfigurationStartupTests.cs`, `ServiceStartupTests.cs`, etc.).
* **New Services/Options:** When adding a new service dependency or configuration option class anywhere in the application, **add a corresponding unit test here** to verify its registration or configuration in the relevant startup method. This is crucial for preventing runtime DI or configuration errors.
* **Lifetime Mismatches:** These tests are excellent for catching incorrect service lifetimes (e.g., registering a Scoped service that depends on a Transient one incorrectly).
* **Configuration Validation:** Ensure tests cover scenarios where required configuration is missing or invalid, verifying that validation logic (e.g., `ValidateDataAnnotations`, `ValidateOnStart`) works as expected.

## 5. Test Cases & TODOs

### `ConfigurationStartupTests.cs`
* **TODO:** Test `AddAppConfiguration` adds expected sources (JSON files, environment variables, user secrets).
* **TODO:** Test `ConfigureAppConfiguration` successfully binds `JwtSettings` and validates its properties.
* **TODO:** Test `ConfigureAppConfiguration` successfully binds `StripeSettings` and validates its properties.
* **TODO:** Test `ConfigureAppConfiguration` successfully binds `OpenAISettings` and validates its properties.
* **TODO:** Test `ConfigureAppConfiguration` successfully binds `EmailSettings` and validates its properties.
* **TODO:** Test `ConfigureAppConfiguration` successfully binds `GitHubSettings` and validates its properties.
* **TODO:** Test `ConfigureAppConfiguration` throws expected exception if required configuration is missing/invalid.
* **TODO:** Test `AddJsonConfigConverter` registers the custom `JsonConverter`.

### `ServiceStartupTests.cs`
* **TODO:** Verify `IConfiguration` is registered.
* **TODO:** Verify `IHttpClientFactory` is registered.
* **TODO:** Verify MediatR services are registered.
* **TODO:** Verify AutoMapper services are registered.
* **TODO:** Verify `UserDbContext` is registered (likely Scoped).
* **TODO:** Verify `ICustomerService`, `IOrderService`, `IRecipeService` registered (check lifetime - likely Scoped).
* **TODO:** Verify `ICustomerRepository`, `IOrderRepository`, `IRecipeRepository`, `ILlmRepository` registered (check lifetime).
* **TODO:** Verify `ILlmService`, `ITranscribeService` registered (check lifetime).
* **TODO:** Verify `IPaymentService`, `IStripeService` registered (check lifetime).
* **TODO:** Verify `IEmailService`, `ITemplateService` registered (check lifetime).
* **TODO:** Verify `IFileService`, `IGitHubService`, `IBrowserService` registered (check lifetime).
* **TODO:** Verify `IPdfCompiler` registered (check lifetime).
* **TODO:** Verify `ISessionManager`, `ISessionCleanup` registered (check lifetime).
* **TODO:** Verify `IStatusService` registered (check lifetime).
* **TODO:** Verify `IBackgroundWorker`, `RefreshTokenCleanupService` registered (Hosted Services - Singleton).
* **TODO:** Verify `IApiKeyService`, `ICookieAuthManager`, `RoleInitializer` registered (check lifetime).
* **TODO:** Verify registration of specific Prompts (`AnalyzeRecipePrompt`, etc.) if done explicitly.

### `AuthenticationStartupTests.cs`
* **TODO:** Verify Identity services (`IdentityUser`, `IdentityRole`, `UserDbContext`) are added.
* **TODO:** Verify JWT Bearer authentication scheme is added and options configured.
* **TODO:** Verify Cookie authentication scheme is added and options configured.
* **TODO:** Verify API Key authentication handler/services are configured (if applicable).
* **TODO:** Verify required authorization policies are added.
* **TODO:** Verify `AddAppAuthentication` configures `JwtSettings` correctly.

### `ApplicationStartupTests.cs`
* **TODO:** Verify CORS policy is configured correctly based on settings.
* **TODO:** Verify routing options (lowercase URLs, append trailing slash) are configured.
* **TODO:** Verify Swagger/OpenAPI generation services are registered (in Development).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for startup configuration unit tests. (Gemini)

