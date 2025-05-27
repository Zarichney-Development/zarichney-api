# Module/Directory: Zarichney.ApiClient

**Last Updated:** 2025-05-26

**(Parent Directory's README)**
> **Parent:** [`zarichney-api (root)`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** A dedicated .NET class library providing strongly-typed client interfaces for interacting with the Zarichney API. Contains auto-generated Refit interfaces and associated models/contracts.
* **Key Responsibilities:**
  - Auto-generated Refit client interfaces grouped by API functionality (AI, Auth, Cookbook, Payment, Public)
  - Data Transfer Objects (DTOs) for API requests and responses
  - Dependency injection configuration for HTTP clients
  - Type-safe API consumption for consuming applications
* **Why it exists:** To provide a modular, reusable client library that separates API client definitions from test projects and server code. Enables multiple projects to consume the API without duplicating client code.

## 2. Architecture & Key Concepts

* **High-Level Design:** Auto-generated using Refitter from OpenAPI specifications. Contains separate interfaces for each API controller area (e.g., `IAiApi`, `IAuthApi`, `ICookbookApi`, `IPaymentApi`, `IPublicApi`) with corresponding models in the Models namespace. The `IServiceCollectionExtensions` class provides easy dependency injection setup.
* **Core Logic Flow:** 
  1. API specification changes trigger regeneration via scripts
  2. Refitter generates interfaces and models from swagger.json
  3. Consuming applications register clients via `ConfigureRefitClients()`
  4. Refit creates HTTP client implementations at runtime
* **Key Data Structures:** 
  - API client interfaces (all implementing Refit patterns)
  - Request/Response DTOs (AuthResponse, Recipe, CookbookOrder, etc.)
  - Configuration and DI extension methods
* **State Management:** Stateless - all state managed by underlying HTTP clients and Refit infrastructure
* **Diagram(s):**
    ```mermaid
    graph TD
        A[Zarichney.ApiClient] --> B[Interfaces/]
        A --> C[Models/]
        A --> D[Configuration/]
        B --> E[IAiApi]
        B --> F[IAuthApi] 
        B --> G[ICookbookApi]
        B --> H[IPaymentApi]
        B --> I[IPublicApi]
        C --> J[DTOs/Models]
        D --> K[DependencyInjection]
        L[Consumer App] --> K
        K --> M[HttpClient Factory]
        M --> N[Refit Implementation]
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
  * `IServiceCollectionExtensions.ConfigureRefitClients(services, builder?, settings?)`:
    * **Purpose:** Register all Refit clients with dependency injection container
    * **Critical Preconditions:** Valid `IServiceCollection` instance
    * **Critical Postconditions:** All API clients registered and configured with base URL
    * **Non-Obvious Error Handling:** May throw configuration exceptions if Refit setup fails
  * Individual API interfaces (`IAiApi`, `IAuthApi`, etc.) provide typed methods matching server endpoints
* **Critical Assumptions:**
  * **External Systems/Config:** Assumes API server is running at configured base URL (default: http://localhost:5000)
  * **Data Integrity:** Assumes server responses match generated contract models
  * **Implicit Constraints:** Generated code should not be manually modified as it will be overwritten

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:** Base URL configured in DependencyInjection.cs (default: http://localhost:5000). Can be overridden by consumer applications.
* **Directory Structure:** 
  - `Interfaces/` - Generated Refit interface definitions
  - `Models/` - Generated DTOs and response models (Contracts.cs)
  - `Configuration/` - DI setup and extension methods
* **Technology Choices:** 
  - Refit for HTTP client generation
  - System.Text.Json for serialization
  - Microsoft DI container integration
* **Performance/Resource Notes:** HTTP clients are pooled and managed by Refit/HttpClientFactory
* **Security Notes:** Inherits authentication handling from configured HTTP clients

## 5. How to Work With This Code

* **Setup:** 
  - Reference `Zarichney.ApiClient` NuGet package or project
  - Call `services.ConfigureRefitClients()` in DI setup
  - Inject desired API interfaces (e.g., `IAiApi`) into consuming classes
* **Testing:**
  * **Location:** Tests will be in separate `Zarichney.ApiClient.Tests` project (planned)
  * **How to Run:** `dotnet test` (once test project exists)
  * **Testing Strategy:** Mock Refit interfaces for unit tests, use real clients for integration tests
* **Common Pitfalls / Gotchas:** 
  - Do not manually modify generated files - changes will be overwritten
  - Regenerate clients when API contracts change using generation scripts
  - Base URL configuration may need adjustment for different environments

## 6. Dependencies

* **Internal Code Dependencies:** None - this is a standalone client library
* **External Library Dependencies:** 
  - `Refit` and `Refit.HttpClientFactory` - HTTP client generation and DI integration
  - `Microsoft.Extensions.DependencyInjection` - DI container support
  - `Microsoft.Extensions.Http` - HttpClientFactory support
  - `System.Text.Json` - JSON serialization
* **Dependents (Impact of Changes):**
  - `api-server.Tests` project (will consume this after migration)
  - Any future client applications consuming the API

## 7. Rationale & Key Historical Context

* **Generated Code Approach:** Chosen to maintain consistency with server API contracts and reduce manual synchronization overhead. Refitter provides excellent Refit integration with OpenAPI specifications.
* **Modular Design:** Separated from test projects to enable reuse across multiple consuming applications and improve solution organization.

## 8. Known Issues & TODOs

* **Auto-generation Scripts:** Scripts updated to target this project but may need refinement as API evolves
* **Test Coverage:** Dedicated test project (`Zarichney.ApiClient.Tests`) needs to be created
* **Documentation Warnings:** Generated code has XML doc warnings for cancellation tokens - acceptable for auto-generated content
* **Environment Configuration:** Base URL hardcoded - may need environment-specific configuration support

---