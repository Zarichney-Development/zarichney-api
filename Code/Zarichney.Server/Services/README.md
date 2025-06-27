# Module/Directory: /Services

**Last Updated:** 2025-04-14

> **Parent:** [`Server`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory acts as a container for shared infrastructure services, external system integrations, and common utilities used throughout the Zarichney API application. It provides foundational capabilities consumed by domain-specific modules like Auth and Cookbook.
* **Key Responsibilities:** Encapsulates distinct service areas into subdirectories, each responsible for specific cross-cutting concerns or external interactions. Provides general utility functions directly within this directory (`Utils.cs`).
* **Why it exists:** To centralize shared services and utilities, promoting reusability and separating infrastructure concerns from core domain logic. The subdirectory structure enhances organization.
* **Submodules:**
    * [`AI`](./AI/README.md) - Handles interactions with Language Models (LLMs) and transcription services.
    * [`BackgroundTasks`](./BackgroundTasks/README.md) - Provides infrastructure for queuing and executing background work asynchronously. [cite: Zarichney.Server/Services/BackgroundWorker.cs]
    * [`Browse`](./Browse/README.md) - Manages web browser automation (via Playwright) for tasks like web scraping. [cite: Zarichney.Server/Services/BrowserService.cs]
    * [`Emails`](./Email/README.md) - Manages email sending, template rendering, and validation.
    * [`FileSystem`](./FileSystem/README.md) - Provides an abstraction layer for file system operations with concurrency handling. [cite: Zarichney.Server/Services/FileService.cs]
    * [`GitHub`](./GitHub/README.md) - Handles interactions with the GitHub API, primarily for storing LLM conversation logs. [cite: Zarichney.Server/Services/GitHubService.cs]
    * [`Payment`](./Payment/README.md) - Encapsulates interactions with the payment provider (Stripe).
    * [`PdfGeneration`](./PdfGeneration/README.md) - Responsible for compiling data (e.g., Markdown) into PDF documents. [cite: Zarichney.Server/Services/PdfCompiler.cs]
    * [`Sessions`](./Sessions/README.md) - Manages user/request sessions and associated scoped data/state.
    * [`Status`](./Status/README.md) - Provides runtime status/health reporting for configuration and other system checks.
* **Utilities:**
    * `Utils.cs`: Contains miscellaneous static helper methods (ID generation, deserialization, string manipulation, Markdown generation, HTML stripping). [cite: Zarichney.Server/Services/Utils.cs]

## 2. Architecture & Key Concepts

* **Service-Oriented:** Primarily composed of distinct services, often defined by interfaces (e.g., `IFileService`, `IBrowserService`, `ILlmService`) and corresponding implementations.
* **Dependency Injection:** Services are designed to be registered and consumed via .NET's Dependency Injection system.
* **Subdirectory Organization:** Services are grouped into subdirectories based on their core responsibility or the external system they interact with.
* **Cross-Cutting Concerns:** Some services represent cross-cutting concerns applicable across multiple modules (e.g., FileSystem, BackgroundTasks, Sessions).
* **External Integrations:** Several submodules encapsulate interactions with external APIs or tools (AI, Emails, Payment, Browse, GitHub).
* **Utilities:** `Utils.cs` provides static helper functions accessible throughout the application.

## 3. Interface Contract & Assumptions

* **Interfaces:** The primary contracts are the C# interfaces defined within each submodule (e.g., `IFileService`, `IEmailService`, `ISessionManager`). Consumers should depend on these interfaces, not concrete implementations.
* **Assumptions:** Each service assumes its own specific dependencies and configurations are met (e.g., `BrowserService` assumes Playwright is installed, `EmailService` assumes Graph API credentials are valid, `GitHubService` assumes access token is valid). Assumes consumers (other modules) use the services according to their intended contracts.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Structure:** Strong emphasis on organizing services into dedicated subdirectories with their own README files for detailed context.
* **Interfaces:** Consistent use of the `IXService` / `XService` pattern for dependency injection.
* **Configuration:** Services often rely on specific `XConfig` objects injected via DI, following the pattern established in [`/Config`]