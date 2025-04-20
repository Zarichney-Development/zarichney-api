# Module/Directory: api-server (Root)

**Last Updated:** 2025-04-20

## 1. Purpose & Responsibility

* **What it is:** This project contains the backend API server for the Zarichney application, responsible for handling user authentication, managing cookbook creation workflows, processing payments, interacting with AI services, and serving data to client applications.
* **Key Responsibilities:**
    * Providing a secure HTTP API for client interactions.
    * Orchestrating the personalized cookbook generation process.
    * Managing user accounts, authentication, and authorization.
    * Integrating with external services like OpenAI (AI tasks), Stripe (payments), MS Graph (email), Playwright (web scraping), and GitHub (logging).
    * Providing configuration and startup logic for the application.
* **Why it exists:** To serve as the central backend logic and data hub for the Zarichney application suite.
* **Top-Level Subdirectories:**
    * [`Config/`](./Config/README.md) - Shared configuration models, middleware, and utilities. [cite: api-server/Config/README.md]
    * [`Controllers/`](./Controllers/README.md) - Defines the external HTTP API endpoints. [cite: api-server/Controllers/README.md]
    * [`Cookbook/`](./Cookbook/README.md) - Core domain logic for cookbook generation (recipes, orders, customers). [cite: api-server/Cookbook/README.md]
    * [`Docs/`](./Docs/README.md) - Project documentation, including development standards.
    * [`Properties/`](./Properties/launchSettings.json) - Visual Studio launch settings.
    * [`Services/`](./Services/README.md) - Cross-cutting concerns and external service integrations (Auth, AI, Payment, Email, etc.). [cite: api-server/Services/README.md]
    * [`Startup/`](./Startup/README.md) - Application startup and service registration logic. [cite: api-server/Startup/README.md]
    * [`api-server.Tests/`](../api-server.Tests/README.md) - (Assumed location) Automated tests project.

## 2. Architecture & Key Concepts

* **High-Level Design:** The application is an ASP.NET Core Web API built using a modular, service-oriented architecture. It leverages dependency injection heavily. Key architectural components include Controllers handling API requests, Services encapsulating business logic and external integrations, and specific domain logic within the `Cookbook` module. Persistence uses a mix of file system storage (`FileService`) for cookbook data and PostgreSQL (`UserDbContext`) for authentication data. Background tasks are handled via `IHostedService`. AI capabilities are integrated via dedicated services interacting with OpenAI.
* **Core Logic Flow:** Client applications interact via HTTP requests routed through Controllers. Controllers typically delegate work to Services (often via MediatR commands/queries in the Auth module). Services coordinate actions, interact with repositories or other services, manage state (via `SessionManager` or persistence), and potentially queue background work (`BackgroundWorker`). External APIs (OpenAI, Stripe, GitHub, MS Graph) are accessed through dedicated service abstractions.
* **Architectural Overview Diagram:**
    *(Diagram follows conventions defined in [`Docs/Standards/DiagrammingStandards.md`](./Docs/Standards/DiagrammingStandards.md))*
    ```mermaid
    %%{init: {'theme': 'base'}}%%
    graph TD
        %% Define Standard Styles (as per DiagrammingStandards.md)
        classDef controller fill:#f9f,stroke:#333,stroke-width:2px;
        classDef service fill:#ccf,stroke:#333,stroke-width:2px;
        classDef repository fill:#ccf,stroke:#333,stroke-width:2px,stroke-dasharray: 5 5;
        classDef database fill:#f5deb3,stroke:#8b4513,stroke-width:2px;
        classDef externalApi fill:#ffcc99,stroke:#ff6600,stroke-width:2px;
        classDef middleware fill:#eee,stroke:#666,stroke-width:1px;
        classDef background fill:#fff0b3,stroke:#cca300,stroke-width:1px;
        classDef config fill:#d9ead3,stroke:#6aa84f,stroke-width:1px;
        classDef startup fill:#d9ead3,stroke:#6aa84f,stroke-width:1px;
        classDef domain fill:#cfe2f3,stroke:#3d85c6,stroke-width:2px;
        classDef ui fill:#cceeff,stroke:#007acc,stroke-width:1px;

        %% Nodes
        subgraph ext [External Systems]
            direction LR
            ClientApp[Client App]:::ui
            PostgresDB[(PostgreSQL)]:::database
            OpenAI[OpenAI API]:::externalApi
            Stripe[Stripe API]:::externalApi
            GitHub[GitHub API]:::externalApi
            MSGraph[MS Graph API]:::externalApi
        end

        subgraph apisrv [API Server Application]
            direction TB
            subgraph req_pipeline [Request Pipeline]
                direction TB
                Controllers[Controllers]:::controller
                Middleware[Middleware]:::middleware
                StartupLogic[Startup / Config]:::startup
            end

            subgraph core_logic [Core Logic]
                direction TB
                Services["Services (Auth, AI, Email, etc.)"]:::service
                CookbookDomain[Cookbook Domain]:::domain
                BackgroundTasks[Background Tasks]:::background
            end

            subgraph persistence [Persistence]
                direction TB
                UserRepo[User/Auth Repository]:::repository
                FileRepo[File-based Repositories]:::repository
            end

            %% Internal High-Level Connections
            Controllers --> Services
            Controllers --> CookbookDomain
            Middleware -.-> Controllers
            %% Middleware intercepts before controllers
            Services --> Persistence
            CookbookDomain --> Services
            %% e.g., Cookbook uses AI Service
            CookbookDomain --> Persistence
            BackgroundTasks -.-> Services
            %% Background tasks use services via scope factory
            Persistence --> UserRepo
            Persistence --> FileRepo

            StartupLogic -- configures --> Middleware
            StartupLogic -- registers --> Services
            StartupLogic -- registers --> CookbookDomain
            StartupLogic -- registers --> Persistence
            StartupLogic -- registers --> BackgroundTasks
        end

        %% External Connections
        ClientApp --> Controllers
        UserRepo --> PostgresDB
        FileRepo -- Stores Data --> Filesystem[(File System)]:::database
        %% Representing FileService target
        Services --> OpenAI
        Services --> Stripe
        Services --> GitHub
        Services --> MSGraph
        CookbookDomain --> OpenAI
        %% Specifically for recipe processing

        %% Apply Styles
        class ClientApp ui;
        class PostgresDB database;
        class OpenAI,Stripe,GitHub,MSGraph externalApi;
        class Controllers controller;
        class Middleware middleware;
        class StartupLogic startup;
        class Services service;
        class CookbookDomain domain;
        class BackgroundTasks background;
        class UserRepo,FileRepo repository;
        class Filesystem database;


    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * The primary interface is the **HTTP RESTful API** exposed by the Controllers. Specific endpoints, request/response formats, and authentication requirements are detailed within the `/Controllers` module and potentially via Swagger/OpenAPI documentation (if generated).
* **Critical Assumptions:**
    * **Deployment Environment:** Assumes a hosting environment capable of running ASP.NET Core 8, with necessary network access to external APIs (OpenAI, Stripe, GitHub, MS Graph) and the PostgreSQL database. Assumes write access to a local or mounted file system for `FileService`.
    * **Configuration:** Assumes all required configuration values (API keys, connection strings, service URLs) are correctly provided via `appsettings.json`, environment variables, user secrets, or other configured providers. Missing critical configuration will likely cause startup failures or runtime errors.
    * **External Services:** Assumes availability and correct functioning of all integrated external services (Database, OpenAI, Stripe, GitHub, MS Graph). Service outages will impact corresponding application features.
    * **Client Behavior:** Assumes clients adhere to API contracts regarding request formats, authentication tokens, and rate limits (if applicable).

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Global Standards:** All code and documentation adhere to the standards defined in [`Docs/Standards/`](./Docs/Standards/).
* **Technology Stack:** ASP.NET Core 8, C#, Serilog (Logging), EF Core (DB access for Auth), MediatR (Auth Commands), Polly (Retries), RestSharp/HttpClient (HTTP Clients), Playwright (Scraping), QuestPDF (PDF Gen), Testcontainers (Integration Testing), xUnit (Testing), Moq (Mocking), FluentAssertions (Assertions).
* **Branching Strategy:** (Define your branching strategy here, e.g., Gitflow, GitHub Flow).
* **Dependency Management:** Uses .NET SDK built-in package management (NuGet).

## 5. How to Work With This Code

* **Setup:**
    1.  Clone the repository.
    2.  Ensure [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) is installed.
    3.  Configure required secrets (API Keys for OpenAI, Stripe, GitHub; Connection String for PostgreSQL) using User Secrets (`dotnet user-secrets set "Key" "Value"`) or other appropriate methods. Refer to `appsettings.json` and specific service `README.md` files for required keys.
    4.  Set up a PostgreSQL database instance accessible to the application.
    5.  Run Entity Framework migrations for the Auth database: `dotnet ef database update --project api-server --startup-project api-server` (adjust paths if needed).
    6.  (If scraping) Install Playwright browsers: `pwsh ./Scripts/Install-Playwright.ps1` (or equivalent).
* **Building:** `dotnet build` from the root directory.
* **Running Locally:** `dotnet run --project api-server` (uses `Properties/launchSettings.json`).
* **Testing:**
    * **Run All Tests:** `dotnet test` from the root directory.
    * **Run Unit Tests:** `dotnet test --filter "Category=Unit"`
    * **Run Integration Tests:** `dotnet test --filter "Category=Integration"` (Requires Docker for Testcontainers).
    * Refer to [`Docs/Standards/TestingStandards.md`](./Docs/Standards/TestingStandards.md) for details.
* **Documentation:**
    * Per-directory `README.md` files provide detailed context. Start exploration from the relevant directory.
    * Adhere to [`Docs/Standards/DocumentationStandards.md`](./Docs/Standards/DocumentationStandards.md) and [`Docs/Standards/DiagrammingStandards.md`](./Docs/Standards/DiagrammingStandards.md).

## 6. Dependencies

* **Internal Code Dependencies:** See Section 1 for links to top-level module READMEs. Key modules rely on each other (e.g., Controllers use Services, Services use Config, Cookbook uses Services).
* **External Library Dependencies:** Major dependencies include ASP.NET Core 8, EF Core, Serilog, MediatR, Polly, OpenAI SDK, Stripe SDK, Octokit.NET, MS Graph SDK, Playwright, QuestPDF, Testcontainers, xUnit, Moq, FluentAssertions. See `.csproj` files for a complete list.
* **External Service Dependencies:** PostgreSQL Database, OpenAI API, Stripe API, GitHub API, Microsoft Graph API.
* **Dependents (Impact of Changes):** Primarily consumed by client applications (Web UI, Mobile App - not included in this repo). Changes to API contracts in `/Controllers` will impact these clients.

## 7. Rationale & Key Historical Context

* This API was developed to provide a robust, scalable, and maintainable backend for the Zarichney personalized cookbook application.
* The architecture prioritizes separation of concerns, dependency injection for testability, and asynchronous processing for potentially long-running tasks (AI interactions, scraping, PDF generation).
* File-based storage for cookbook data was chosen initially for simplicity/cost, while a relational database (PostgreSQL) was used for structured authentication data via ASP.NET Core Identity.

## 8. Known Issues & TODOs

* **Scalability:** File-based storage for recipes/orders may face scalability limitations under very high load; consider migration to a database or blob storage in the future.
* **Scraping Fragility:** Web scraping logic (`WebScraperService`) is inherently brittle and may break if target website structures change. Requires monitoring and maintenance.
* **Configuration Management:** Explore more robust configuration management for production environments (e.g., Azure App Configuration, AWS Parameter Store).
* **Monitoring & Alerting:** Implement comprehensive application monitoring, logging aggregation, and alerting for production readiness.

