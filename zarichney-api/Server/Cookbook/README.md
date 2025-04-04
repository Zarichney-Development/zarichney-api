# Module/Directory: Server/Cookbook

**Last Updated:** 2025-04-03

> **Parent:** [`Server`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory serves as the primary domain module for the "Cookbook Factory" feature of the Zarichney API. It encapsulates all logic related to customer interactions, order processing, recipe management (acquisition, synthesis, storage), and AI interactions specific to cookbook creation.
* **Key Responsibilities:**
    * Orchestrating the end-to-end cookbook generation process, from order submission to PDF compilation and delivery. [cite: zarichney-api/Server/Cookbook/Orders/OrderService.cs]
    * Managing customer data and recipe credits. [cite: zarichney-api/Server/Cookbook/Customers/CustomerService.cs]
    * Handling the lifecycle of cookbook orders. [cite: zarichney-api/Server/Cookbook/Orders/OrderService.cs]
    * Managing recipe data, including scraping, cleaning, indexing, searching, ranking, and AI-driven synthesis. [cite: zarichney-api/Server/Cookbook/Recipes/RecipeService.cs]
    * Defining specific prompts for interactions with AI services related to cookbook tasks. [cite: zarichney-api/Server/Cookbook/Prompts/]
* **Why it exists:** To group all related functionalities for the cookbook feature into a cohesive and maintainable module, separating it from concerns like core authentication, generic AI services, or payment processing.
* **Submodules:**
    * [`Customers`](./Customers/README.md) - Manages customer data, including available recipe credits.
    * [`Orders`](./Orders/README.md) - Handles cookbook order submission, processing logic, status tracking, and PDF generation coordination.
    * [`Prompts`](./Prompts/README.md) - Defines specific `PromptBase` implementations for AI tasks related to recipes and orders (e.g., cleaning, ranking, synthesizing, analyzing).
    * [`Recipes`](./Recipes/README.md) - Contains logic for acquiring (scraping), storing (file repository), indexing, searching, and synthesizing recipes.

## 2. Architecture & Key Concepts

* **Modular Design:** Functionality is broken down into specific subdirectories (`Customers`, `Orders`, `Prompts`, `Recipes`) each handling a distinct part of the cookbook domain.
* **Orchestration:** The `OrderService` typically acts as the central orchestrator, coordinating interactions between `CustomerService`, `RecipeService`, `PdfCompiler`, and `EmailService` to fulfill a cookbook order. [cite: zarichney-api/Server/Cookbook/Orders/OrderService.cs]
* **Recipe Lifecycle:** Managed primarily within the `Recipes` submodule, involving `WebScraperService` for acquisition, `RecipeRepository` for storage/indexing/cleaning/naming, `RecipeSearcher` for retrieval, and `RecipeService` for overall orchestration and synthesis. [cite: zarichney-api/Server/Cookbook/Recipes/README.md]
* **AI Integration:** Heavily relies on AI for various tasks, with specific prompt logic defined in `Prompts` and interactions handled via the central `LlmService`. [cite: zarichney-api/Server/Cookbook/Prompts/, zarichney-api/Server/Services/AI/LlmService.cs]
* **Data Persistence:** Uses a file-based approach via `IFileService` for storing customer, order, and recipe data within the respective submodules. [cite: zarichney-api/Server/Cookbook/Customers/CustomerRepository.cs, zarichney-api/Server/Cookbook/Orders/OrderRepository.cs, zarichney-api/Server/Cookbook/Recipes/RecipeRepository.cs]
* **Key Data Structures:** Defines core domain models within subdirectories (e.g., `Customer`, `CookbookOrder`, `Recipe`, `SynthesizedRecipe`). [cite: zarichney-api/Server/Cookbook/Customers/CustomerModels.cs, zarichney-api/Server/Cookbook/Orders/OrderModels.cs, zarichney-api/Server/Cookbook/Recipes/RecipeModels.cs]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):** Primarily the service interfaces exposed by the submodules and consumed by controllers or other services:
    * `IOrderService`: Interface for submitting, retrieving, and managing cookbook orders. [cite: zarichney-api/Server/Cookbook/Orders/OrderService.cs]
    * `IRecipeService`: Interface for searching, retrieving, and synthesizing recipes. [cite: zarichney-api/Server/Cookbook/Recipes/RecipeService.cs]
    * `ICustomerService`: Interface for managing customer data and credits. [cite: zarichney-api/Server/Cookbook/Customers/CustomerService.cs]
* **Assumptions:**
    * **Configuration:** Assumes relevant configuration sections (`OrderConfig`, `CustomerConfig`, `RecipeConfig`, `WebscraperConfig`, `LlmConfig`, `PdfCompilerConfig`) are correctly bound and available via DI. [cite: zarichney-api/appsettings.json, zarichney-api/Server/Config/ConfigurationExtensions.cs]
    * **Service Availability:** Assumes dependent services ([`Server/Services/AI`](../../Services/AI/README.md), [`Server/Services/Emails`](../../Services/Emails/README.md), [`Server/Services/Payment`](../../Services/Payment/README.md), [`Server/Services`](../../Services/README.md) like `IFileService`, `IBackgroundWorker`, `IBrowserService`) are registered and functional.
    * **Data Storage:** Assumes the file system paths configured for data output (`Data/Customers`, `Data/Orders`, `Data/Recipes`) are writable by the application user (`ec2-user` in production). [cite: zarichney-api/Docs/AwsMaintenance.md]
    * **AI Model Access:** Assumes valid credentials/API keys for AI services (e.g., OpenAI) are configured.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Subdirectory Structure:** Organizes code strictly by domain concern (Customers, Orders, Prompts, Recipes).
* **AI Prompts:** All prompts specific to cookbook generation logic reside in the `Prompts` subdirectory, inheriting from `PromptBase`. [cite: zarichney-api/Server/Cookbook/Prompts/]
* **Data Storage:** Adheres to a file-based persistence strategy using `IFileService`. Filenames often derived from IDs or email addresses (sanitized). [cite: zarichney-api/Server/Cookbook/Customers/CustomerRepository.cs, zarichney-api/Server/Cookbook/Orders/OrderRepository.cs, zarichney-api/Server/Cookbook/Recipes/RecipeRepository.cs]
* **Training Data:** Example data for specific AI prompt fine-tuning (like `RecipeNamer`) is stored in the `Training` subdirectory within `Recipes`. [cite: zarichney-api/Server/Cookbook/Training/]

## 5. How to Work With This Code

* **Understanding Flow:** Start by examining `CookbookController` to see how requests enter the module, then follow the calls into `OrderService`, `RecipeService`, and `CustomerService` to understand the orchestration.
* **Adding Features:** New cookbook-related functionality should likely reside within one of the existing submodules or potentially a new submodule if it represents a distinct concern.
* **AI Modifications:** Changes to AI behavior typically involve modifying the relevant `PromptBase` implementation in the `Prompts` directory and potentially the corresponding service logic that uses it.
* **Testing:** Requires mocking dependencies between the submodules and core services (e.g., mocking `IRecipeService` when testing `OrderService`, mocking `ILlmService` when testing `RecipeService`). Integration tests are important to verify the end-to-end flow.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Server/Services`](../../Services/README.md): Relies heavily on `IFileService`, `ILlmService`, `IEmailService`, `PdfCompiler`, `IBackgroundWorker`, `IBrowserService`, `ISessionManager`.
    * [`Server/Config`](../../Config/README.md): Consumes various `*Config` objects.
* **External Library Dependencies:**
    * `AutoMapper`: Used for mapping between different recipe model types. [cite: zarichney-api/Server/Cookbook/Recipes/RecipeModels.cs]
    * `MediatR`: May be used implicitly if commands/queries are defined within submodules (though primarily seen in Auth).
    * `OpenAI`: Core dependency for AI interactions via `LlmService`.
    * `Polly`: Used for retries, especially within `OrderService` for email. [cite: zarichney-api/Server/Cookbook/Orders/OrderService.cs]
    * `QuestPDF`: Used by `PdfCompiler` for PDF generation. [cite: zarichney-api/Server/Services/PdfCompiler.cs]
    * `AngleSharp`, `Playwright`: Used by `WebScraperService` (within Recipes). [cite: zarichney-api/Server/Cookbook/Recipes/WebScraperService.cs]
* **Dependents (Impact of Changes):**
    * [`Server/Controllers/CookbookController.cs`](../../Controllers/CookbookController.cs): Primary consumer of the services defined within this module (`IOrderService`, `IRecipeService`).
    * [`Server/Controllers/PaymentController.cs`](../../Controllers/PaymentController.cs): Consumes `IOrderService`, `ICustomerService`.

## 7. Rationale & Key Historical Context

* **Modular Design:** The separation into `Customers`, `Orders`, `Recipes`, and `Prompts` was chosen to manage the complexity of the cookbook generation feature and improve maintainability.
* **AI-Centric Approach:** Core functionalities like recipe cleaning, ranking, naming, synthesis, and analysis are delegated to AI, making it central to the module's operation.
* **File-Based Persistence:** Selected for simplicity within the scope of this project, offering ease of inspection and modification during development, while acknowledging potential scalability limitations compared to a database.

## 8. Known Issues & TODOs

* File-based storage for recipes, orders, and customers may encounter performance bottlenecks or concurrency issues at very high scale. Consider migrating to a database solution if needed.
* Web scraping logic in the `Recipes` submodule is inherently brittle and dependent on external website structures remaining consistent. [cite: zarichney-api/Server/Cookbook/Recipes/README.md]
* Error handling for AI service failures could be further refined (e.g., more specific fallback strategies).
* Lack of comprehensive unit and integration tests covering the interactions between the various services within this module.