# Zarichney Platform & AI Workflow Testbed

**Last Updated:** 2025-07-27  
**DevOps Status:** ✅ Infrastructure deployment validated  
**Test Status:** 🧪 End-to-end validation - testing TRX parsing & AI workflow fixes

---

## Overview

Welcome! This repository hosts the **Zarichney Platform**, a full-stack application featuring a modular **.NET 8 backend** and modern **Angular frontend**, serving as both a functional web platform and a practical testbed for exploring and refining **AI-assisted software development workflows**.

Think of this repo in two ways:

1.  **A Functional Web Platform:** It powers the **Cookbook Factory AI** application, which uses Large Language Models (LLMs) to scrape, analyze, clean, and synthesize recipes into custom cookbooks (PDF generation included!). Users interact through a modern Angular web interface with features like secure authentication, payment integration (Stripe), real-time order tracking, and administrative dashboards.
2.  **An Experiment in Development:** It's actively developed using a **structured, AI-assisted workflow**. Specialized AI agents, guided by detailed prompts and rigorous standards documentation (all included in this repo!), handle significant portions of the coding, testing, and documentation tasks across both frontend and backend. This allows for rapid iteration while maintaining high quality.

Whether you're interested in the platform's features or the cutting-edge development methodology, explore the code and documentation to see it in action!

## Key Features & Capabilities

* **Modern Angular Frontend:** Responsive web application with SSR support, featuring user interfaces for recipe browsing, order management, payment processing, and administrative functions.
* **Modular .NET 8 API Server:** RESTful backend designed to be extensible for various "micro-apps" or features.
* **Cookbook Factory AI:**
    * Web scraping and recipe data extraction.
    * LLM-powered recipe analysis, cleaning, ranking, naming, and synthesis (using OpenAI).
    * Automated generation of customized cookbook PDFs (using QuestPDF).
    * Interactive recipe browsing and selection via responsive carousels.
    * Real-time order tracking and status updates.
    * User authentication (JWT/Refresh Tokens, API Keys via ASP.NET Core Identity).
    * Payment processing via Stripe integration with secure checkout flows.
    * Customer and order management with administrative dashboards.
* **Core Services:** Email (MS Graph), File Storage, Background Tasks, Secure Configuration, Session Management.
* **AI-Assisted Development Workflow (Meta-Feature):**
    * Leverages AI Planning Assistants and AI Coders for development tasks.
    * Includes comprehensive standards documentation (`/Docs/Standards/`) governing coding, testing, documentation, diagramming, and task management.
    * Features detailed workflow definitions (`/Docs/Development/`) and prompt templates (`/Docs/Templates/`) for guiding AI agents.
    * Aims for high code quality, test coverage, and maintainability through this structured AI collaboration. ([Learn More](./Docs/Development/README.md))

## Technology Highlights

* **Frontend:** TypeScript, Angular 19, NgRx (state management), Angular Material (UI), SSR (Server-Side Rendering), Responsive Design
* **Backend:** C# 12, .NET 8, ASP.NET Core Web API
* **AI:** OpenAI API integration
* **Database:** PostgreSQL (primarily for Identity; application data currently file-based)
* **Authentication:** ASP.NET Core Identity, JWT, Refresh Tokens (Cookies), API Keys
* **Testing:** xUnit, Moq, FluentAssertions, Testcontainers (for isolated DB testing), Refit (for API client generation), **AI-Powered Test Analysis** (unified test suite with parallel execution) - *Aiming for >90% coverage.*
* **API Documentation:** Swagger / OpenAPI
* **Key Libraries:** 
    * **Backend:** Serilog, Polly, MediatR, AutoMapper, RestSharp, Stripe.net, Octokit, QuestPDF, PlaywrightSharp, AngleSharp
    * **Frontend:** RxJS, Angular CDK, Stripe.js, Express (for SSR), Axios
* **Development Process:** Git, **GitHub Actions (Advanced CI/CD with AI-powered QA, Parallel Test Execution, Dynamic Quality Gates)**, AI Agent Assistance (via custom prompts/workflows).

## Exploring the Repository

* **`Code/Zarichney.Server/`:** Contains the main ASP.NET Core application code. Start with [`Code/Zarichney.Server/README.md`](./Code/Zarichney.Server/README.md) for an overview.
* **`Code/Zarichney.Server.Tests/`:** Contains the unit and integration tests. See [`Code/Zarichney.Server.Tests/README.md`](./Code/Zarichney.Server.Tests/README.md).
* **`Code/Zarichney.Website/`:** Contains the Angular frontend application. See [`Code/Zarichney.Website/README.md`](./Code/Zarichney.Website/README.md).
* **`Docs/`:** The heart of the documentation and AI workflow definitions.
    * **`Docs/Standards/`:** Defines the rules (coding, testing, docs, etc.).
    * **`Docs/Development/`:** Defines the AI-assisted workflow processes and roadmap.
    * **`Docs/Templates/`:** Contains templates for AI prompts and GitHub Issues.
    * Each module within `Code/` also has its own detailed `README.md`.
* **`Scripts/`:** Utility scripts including the unified test suite, API client generation, and various development tools. See [`Scripts/README.md`](./Scripts/README.md).

## Getting Started (High Level)

1.  Clone the repository.
2.  **Backend Setup:**
    * Ensure [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for integration tests) are installed.
    * Review `Code/Zarichney.Server/README.md` for configuration prerequisites (secrets, API keys).
    * Build the solution (`dotnet build`).
    * Run the tests with AI-powered analysis (`/test-report` or `Scripts/run-test-suite.sh`).
    * Run the API (`dotnet run --project Code/Zarichney.Server`).
3.  **Frontend Setup:**
    * Ensure [Node.js 18+](https://nodejs.org/) is installed.
    * Navigate to `Code/Zarichney.Website/` and run `npm install`.
    * Review `Code/Zarichney.Website/README.md` for configuration details.
    * Start development server (`npm start`) or build for production (`npm run build-prod`).

*(Detailed setup and contribution guidelines are part of the internal documentation standards.)*

---

This project is actively developed and evolves rapidly. Feel free to explore!
