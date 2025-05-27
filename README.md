# Zarichney API Server & AI Workflow Testbed

**Last Updated:** 2025-05-04

---

## Overview

Welcome! This repository hosts the **Zarichney API Server**, a modular backend built with **.NET 8**, serving as both a functional API and a practical testbed for exploring and refining **AI-assisted software development workflows**.

Think of this repo in two ways:

1.  **A Functional API:** It powers applications like the **Cookbook Factory AI**, which uses Large Language Models (LLMs) to scrape, analyze, clean, and synthesize recipes into custom cookbooks (PDF generation included!). It features secure authentication, payment integration (Stripe), background job processing, and more.
2.  **An Experiment in Development:** It's actively developed using a **structured, AI-assisted workflow**. Specialized AI agents, guided by detailed prompts and rigorous standards documentation (all included in this repo!), handle significant portions of the coding, testing, and documentation tasks. This allows for rapid iteration while maintaining high quality.

Whether you're interested in the API's features or the cutting-edge development methodology, explore the code and documentation to see it in action!

## Key Features & Capabilities

* **Modular .NET 8 API Server:** Designed to be extensible for various "micro-apps" or features.
* **Cookbook Factory AI:**
    * Web scraping and recipe data extraction.
    * LLM-powered recipe analysis, cleaning, ranking, naming, and synthesis (using OpenAI).
    * Automated generation of customized cookbook PDFs (using QuestPDF).
    * User authentication (JWT/Refresh Tokens, API Keys via ASP.NET Core Identity).
    * Payment processing via Stripe integration.
    * Customer and order management.
* **Core Services:** Email (MS Graph), File Storage, Background Tasks, Secure Configuration, Session Management.
* **AI-Assisted Development Workflow (Meta-Feature):**
    * Leverages AI Planning Assistants and AI Coders for development tasks.
    * Includes comprehensive standards documentation (`/Zarichney.Standards/Standards/`) governing coding, testing, documentation, diagramming, and task management.
    * Features detailed workflow definitions (`/Zarichney.Standards/Development/`) and prompt templates (`/Zarichney.Standards/Templates/`) for guiding AI agents.
    * Aims for high code quality, test coverage, and maintainability through this structured AI collaboration. ([Learn More](./Zarichney.Standards/Development/README.md))

## Technology Highlights

* **Backend:** C# 12, .NET 8, ASP.NET Core Web API
* **AI:** OpenAI API integration
* **Database:** PostgreSQL (primarily for Identity; application data currently file-based)
* **Authentication:** ASP.NET Core Identity, JWT, Refresh Tokens (Cookies), API Keys
* **Testing:** xUnit, Moq, FluentAssertions, Testcontainers (for isolated DB testing), Refit (for API client generation) - *Aiming for >90% coverage.*
* **API Documentation:** Swagger / OpenAPI
* **Key Libraries:** Serilog, Polly, MediatR, AutoMapper, RestSharp, Stripe.net, Octokit, QuestPDF, PlaywrightSharp, AngleSharp
* **Development Process:** Git, GitHub Actions (planned for CI/CD), AI Agent Assistance (via custom prompts/workflows).

## Exploring the Repository

* **`api-server/`:** Contains the main ASP.NET Core application code. Start with [`api-server/README.md`](./api-server/README.md) for an overview.
* **`api-server.Tests/`:** Contains the unit and integration tests. See [`api-server.Tests/README.md`](./api-server.Tests/README.md).
* **`Zarichney.Standards/`:** The heart of the documentation and AI workflow definitions.
    * **`Zarichney.Standards/Standards/`:** Defines the rules (coding, testing, docs, etc.).
    * **`Zarichney.Standards/Development/`:** Defines the AI-assisted workflow processes and roadmap.
    * **`Zarichney.Standards/Templates/`:** Contains templates for AI prompts and GitHub Issues.
    * Each module within `api-server/` also has its own detailed `README.md`.
* **`Scripts/`:** Utility scripts (e.g., regenerating the test API client).

## Getting Started (High Level)

1.  Clone the repository.
2.  Ensure [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for integration tests) are installed.
3.  Review `api-server/README.md` for configuration prerequisites (secrets, API keys).
4.  Build the solution (`dotnet build`).
5.  Run the tests (`dotnet test`).
6.  Run the API (`dotnet run` from `api-server/`).

*(Detailed setup and contribution guidelines are part of the internal documentation standards.)*

---

This project is actively developed and evolves rapidly. Feel free to explore!
