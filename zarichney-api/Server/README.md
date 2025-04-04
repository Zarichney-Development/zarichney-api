# Module/Directory: Server

**Last Updated:** 2025-04-03

## 1. Purpose & Responsibility

* **What it is:** This directory contains the backend API server for the Zarichney application, providing API endpoints and core logic for features including personalized cookbook generation, AI interactions, user authentication, and payment processing.
* **Key Responsibilities:**
  * Exposing RESTful API endpoints for client interaction ([`Controllers`](./Controllers/README.md)).
  * Managing user authentication, authorization, sessions, and API keys ([`Auth`](./Auth/README.md), [`Services/Sessions`](./Services/Sessions/README.md)).
  * Handling cookbook order submission and coordinating processing ([`Cookbook/Orders`](./Cookbook/Orders/README.md)).
  * Managing recipe data: searching, scraping, cleaning, indexing, and AI-driven synthesis ([`Cookbook/Recipes`](./Cookbook/Recipes/README.md)).
  * Interacting with external services: OpenAI, Stripe, GitHub, Microsoft Graph (Email), Playwright (`Services`).
  * Managing application configuration ([`Config`](./Config/README.md)).
  * Centralizing application startup logic ([`Startup`](./Startup/README.md)).
  * Executing background tasks (`Services/BackgroundWorker`).
* **Why it exists:** To provide the core backend logic and API interface for the Zarichney application, separating concerns into distinct modules powering the `zarichney.com` website and integrated AI features.
* **Submodules:**
  * [`Auth`](./Auth/README.md) - Handles user identity, authentication (JWT/Cookie/API Key), authorization, and user database interactions.
  * [`Config`](./Config/README.md) - Defines configuration models and shared utilities.
  * [`Controllers`](./Controllers/README.md) - Defines the external API endpoints exposed by the server.
  * [`Cookbook`](./Cookbook/README.md) - Contains the core domain logic for customers, orders, recipes, and AI prompts related to cookbook generation.
  * [`Services`](./Services/README.md) - Implements cross-cutting concerns, background tasks, session management, and integrations with external systems (AI, Stripe, GitHub, Filesystem, Email, Browser).
  * [`Startup`](./Startup/README.md) - Centralizes application startup configuration, service registration, and middleware setup logic.

## 2. Architecture & Key Concepts

* **High-Level Design:** ASP.NET Core Web API application. Exposes API endpoints via [`Controllers`](./Controllers/README.md) which orchestrate calls to various [`Services`](./Services/README.md) and domain-specific logic within [`Cookbook`](./Cookbook/README.md) and [`Auth`](./Auth/README.md). Employs Dependency Injection and standard ASP.NET Core middleware patterns.
* **Core Logic Flow (Example: API Request):**
  1.  Request received by ASP.NET Core.
  2.  Middleware executes (Logging, Error Handling, Auth, Session).
  3.  Request routed to a specific `Controller` action.
  4.  Controller action validates input and calls relevant `Service`(s) (e.g., `IOrderService`, `IRecipeService`, `IAuthService`).
  5.  Services perform business logic, potentially interacting with repositories (`IOrderRepository`, `IRecipeRepository`, `ICustomerRepository`), external APIs (`LlmService`, `PaymentService`, `EmailService`, `GitHubService`), or queuing background tasks (`IBackgroundWorker`).
  6.  Results are returned to the Controller, which forms the HTTP response.
* **Key Data Structures:** `CookbookOrder`, `Recipe`, `Customer` ([`Cookbook`](./Cookbook/README.md)), `ApplicationUser`, `ApiKey`, `RefreshToken` ([`Auth`](./Auth/README.md)), `Session`, `ScopeContainer` ([`Services/Sessions`](./Services/Sessions/README.md)). Various configuration models ([`Config`](./Config/README.md)).
* **State Management:**
  * User authentication state managed via JWT cookies and validated by ASP.NET Core Identity + JWT Bearer middleware ([`Auth`](./Auth/README.md)). API Key authentication is also supported ([`Auth/ApiKeyAuthMiddleware.cs`](./Auth/ApiKeyAuthMiddleware.cs)).
  * User identity data stored in PostgreSQL via EF Core ([`Auth/UserDbContext.cs`](./Auth/UserDbContext.cs)).
  * Request/operation state managed via custom `SessionManager` using in-memory dictionaries, tied to scopes ([`Services/Sessions`](./Services/Sessions/README.md)).
  * Cookbook orders, customer data, and recipe data persisted primarily via `FileService` (to the filesystem) and `GitHubService` (to a GitHub repository) ([`Services/FileService.cs`](./Services/FileService.cs), [`Services/GitHubService.cs`](./Services/GitHubService.cs), [`Cookbook/Orders/OrderRepository.cs`](./Cookbook/Orders/OrderRepository.cs), [`Cookbook/Recipes/RecipeRepository.cs`](./Cookbook/Recipes/RecipeRepository.cs), [`Cookbook/Customers/CustomerRepository.cs`](./Cookbook/Customers/CustomerRepository.cs)).
  * LLM conversation logs persisted via `LlmRepository` to GitHub ([`Services/AI/LlmRepository.cs`](./Services/AI/LlmRepository.cs)).

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (API Endpoints):** Defined within the [`Controllers`](./Controllers/README.md) directory, providing external access to authentication, cookbook, payment, AI, and general API functionalities. Specific contracts are detailed within the respective controller READMEs.
* **Critical Assumptions:**
  * **External Systems Availability & Configuration:** Relies on the availability and correct configuration (API keys, secrets, URLs, connection strings) of:
    * OpenAI API ([`Services/AI`](./Services/AI/README.md))
    * Stripe API ([`Services/Payment`](./Services/Payment/README.md))
    * GitHub API ([`Services/GitHubService.cs`](./Services/GitHubService.cs))
    * Microsoft Graph API (for email) ([`Services/Emails`](./Services/Emails/README.md))
    * MailCheck API (for email validation) ([`Services/Emails`](./Services/Emails/README.md))
    * Target websites for scraping ([`Services/BrowserService.cs`](./Services/BrowserService.cs), [`Config/site_selectors.json`](./Config/site_selectors.json))
    * PostgreSQL Database ([`Auth/UserDbContext.cs`](./Auth/UserDbContext.cs))
  * **Playwright Dependencies:** Assumes the necessary Playwright browser executables are installed in the runtime environment ([`Services/BrowserService.cs`](./Services/BrowserService.cs)).
  * **Data Persistence:** Assumes the file system paths specified in configuration are writable. Assumes the GitHub repository used by `GitHubService` is accessible and configured.
  * **Background Processing:** Relies on the ASP.NET Core hosting environment to keep background services (`BackgroundTaskService`, `SessionCleanupService`, `RefreshTokenCleanupService`, `GitHubService`) running reliably.
  * **Session Management:** Assumes client interactions (especially long-running ones like cookbook generation) can span multiple requests and relies on the `SessionManager` to maintain state. Session expiration and cleanup are critical.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:** Driven by `appsettings.json`, environment variables, and user secrets, loaded via [`Startup/StartupExtensions.cs`](./Startup/StartupExtensions.cs). Specific configuration classes (e.g., `RecipeConfig`, `JwtSettings`) are injected via DI. External `site_selectors.json` defines scraping targets.
* **Directory Structure:** Modules organized by feature area (`Auth`, `Cookbook`, `Controllers`, `Services`, `Config`, `Startup`). Data persistence targets `Data/` subfolders by default.
* **Technology Choices:** ASP.NET Core 8, EF Core/PostgreSQL (Identity), Serilog, MediatR, Polly, Playwright, QuestPDF, Octokit, OpenAI SDK, Stripe SDK, Handlebars.Net, AngleSharp.
* **Performance/Resource Notes:** Web scraping (`BrowserService`) and LLM interactions (`LlmService`) are key areas for performance monitoring and potential bottlenecks. Background tasks and session cleanup frequency impact resource usage.
* **Security Notes:** Authentication handled via JWT Cookies & API Keys. Secure management of secrets is paramount. CSRF protection depends on client interaction patterns (likely needed for web apps).

## 5. Dependencies

* **Internal Code Dependencies:** Components are designed to be relatively modular, but rely on shared services and configuration:
  * [Auth](./Auth/README.md)
  * [Config](./Config/README.md)
  * [Controllers](./Controllers/README.md)
  * [Cookbook](./Cookbook/README.md)
  * [Services](./Services/README.md)
  * [Startup](./Startup/README.md)
* **External Library Dependencies:** `Microsoft.AspNetCore.*`, `Microsoft.EntityFrameworkCore.*` (Npgsql), `Serilog.*`, `MediatR`, `Polly`, `OpenAI`, `Stripe.net`, `Microsoft.Playwright`, `AngleSharp`, `QuestPDF`, `Handlebars.Net`, `Octokit`, `Swashbuckle.AspNetCore`. (Specific versions managed via project files).
* **Dependents (Impact of Changes):**
  * **Primary:** `zarichney.com` (Angular SSR web application) - Consumes the `/api/*` endpoints. Changes to API contracts in `Controllers` require corresponding updates in the frontend.
  * **Secondary:** Custom GPT (OpenAI GPT Store) - Uses API Key authentication to interact with specific API endpoints (likely related to cookbook/AI features).

## 6. Rationale & Key Historical Context

* **Project Evolution:** This project originated as a personal development playground and evolved into an experimental platform for integrating AI capabilities, eventually becoming a side hustle and showcase application (`zarichney.com`). The long-term vision is to expand its capabilities towards a comprehensive AI platform.
* **Architectural Approach:** Standard ASP.NET Core patterns provide a familiar and maintainable structure. The separation into `Services`, `Cookbook`, `Auth`, etc., aims for modularity.
* **Data Persistence Choices:**
  * **PostgreSQL (EF Core):** Used for structured user identity data leveraging ASP.NET Core Identity's robust features.
  * **File System / GitHub:** Chosen for storing cookbook orders, recipes, customer data (outside identity), and LLM logs. This was likely selected for simplicity, cost-effectiveness (compared to scaling database storage), ease of backup/versioning (via Git), and potentially for easier inspection/manual editing during development or for specific debugging/logging needs. This choice implies trade-offs regarding transactional integrity, complex querying, and potential performance bottlenecks compared to a full database solution for this data.
* **Background Tasks:** Essential for decoupling long-running operations (order processing, scraping, PDF generation, email sending) from the initial web request, improving responsiveness.
* **Custom Session Management:** Implemented to manage state across multiple requests, particularly relevant for the multi-step cookbook generation process which involves user interaction, background tasks, and potentially long delays.
* **Startup Refactoring:** The application startup logic was recently centralized in the `Startup` module to improve organization and maintainability by separating it from the main program entry point.

## 8. Known Issues & TODOs

* Web scraping fragility requires continuous monitoring and updates to `site_selectors.json`.
* Dependence on external API stability (OpenAI, Stripe, GitHub, etc.).
* Potential scalability limitations of file-based storage for high volumes of orders/recipes.
* Session management complexity requires careful testing, especially around cleanup and concurrency.
* Currently lacks automated tests; adding unit, integration, and potentially end-to-end tests is a future goal.
* PDF generation performance might need optimization for very large cookbooks.