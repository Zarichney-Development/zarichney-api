# Zarichney Platform & AI Workflow Testbed

**Last Updated:** 2025-08-30  

---

## Overview

Welcome! This repository hosts the **Zarichney Platform**, a full-stack application featuring a modular **.NET 8 backend** and modern **Angular frontend**, serving as both a functional web platform and a practical testbed for exploring and refining **AI-assisted software development workflows**.

Think of this repo in two ways:

1.  **A Functional Web Platform:** It powers the **Cookbook Factory AI** application, which uses Large Language Models (LLMs) to scrape, analyze, clean, and synthesize recipes into custom cookbooks (PDF generation included). Users interact through a modern Angular web interface with features like secure authentication, payment integration (Stripe), real-time order tracking, and administrative dashboards (coming soon!).
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
    * **9-Agent Specialized Development Team:** Features a coordinated team of specialized AI agents (CodeChanger, TestEngineer, SecurityAuditor, etc.) working under strategic codebase manager supervision.
    * **Documentation Grounding Architecture:** Self-contained knowledge system where agents systematically load project documentation to ensure contextual awareness and standards alignment.
    * Includes comprehensive standards documentation (`/Docs/Standards/`) governing coding, testing, documentation, diagramming, and task management.
    * Features detailed agent instruction files (`/.claude/agents/`) defining specialized capabilities and coordination protocols.
    * Aims for high code quality, test coverage, and maintainability through this structured team collaboration. ([Learn More](./Docs/Development/README.md))

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
* **Development Process:** Git, **GitHub Actions (Advanced CI/CD with AI-powered QA, Parallel Test Execution, Dynamic Quality Gates)**, **9-Agent Specialized Development Team** (via coordinated subagent orchestration).

## Exploring the Repository

* **`Code/Zarichney.Server/`:** Contains the main ASP.NET Core application code. Start with [`Code/Zarichney.Server/README.md`](./Code/Zarichney.Server/README.md) for an overview.
* **`Code/Zarichney.Server.Tests/`:** Contains the unit and integration tests. See [`Code/Zarichney.Server.Tests/README.md`](./Code/Zarichney.Server.Tests/README.md).
* **`Code/Zarichney.Website/`:** Contains the Angular frontend application. See [`Code/Zarichney.Website/README.md`](./Code/Zarichney.Website/README.md).
* **`Docs/`:** The heart of the documentation and AI workflow definitions.
    * **`Docs/Standards/`:** Defines the rules (coding, testing, docs, etc.).
    * **`Docs/Development/`:** Defines the AI-assisted workflow processes and roadmap.
    * **`Docs/Templates/`:** Contains templates for AI prompts and GitHub Issues.
    * Each module within `Code/` also has its own detailed `README.md`.
* **`.claude/agents/`:** **9-Agent Development Team Instructions** - Specialized agent configurations with documentation grounding protocols for coordinated development work.
* **`Scripts/`:** Utility scripts including the unified test suite, API client generation, and various development tools. See [`Scripts/README.md`](./Scripts/README.md).

## Getting Started

**Quick Setup**: For immediate development without external services (~15 minutes)  
**Complete Setup**: For full platform features including payments, AI, and email (~60 minutes)

📋 **Comprehensive Setup Guide**: [`Docs/Development/LocalSetup.md`](./Docs/Development/LocalSetup.md)

**Essential Commands**:
```bash
# Clone and build
git clone [repository-url]
dotnet build Zarichney.sln

# Quick validation
./Scripts/run-test-suite.sh report summary

# Start development
dotnet run --project Code/Zarichney.Server  # Backend API
cd Code/Zarichney.Website && npm start      # Frontend (separate terminal)
```

---

This project is actively developed and evolves rapidly. Feel free to explore!
