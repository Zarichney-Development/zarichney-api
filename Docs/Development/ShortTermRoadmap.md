# Codebase & Workflow - Short-Term Roadmap & Future Enhancements

**Version:** 1.0
**Last Updated:** 2025-05-03

* **Context:** This document captures planned enhancements, refactoring goals, and potential future directions for the `Zarichney.Server` codebase and its associated AI-assisted development workflow. It translates broader strategic goals into specific technical or process objectives, including items deferred from the V2.0 documentation update cycle (May 2025).*

## 1. AI Workflow & SDLC Automation

* **Goal:** Progress towards a more fully automated SDLC leveraging AI agents, reducing manual intervention in core development activities.
* **Specific Enhancements:**
    * **Automated AI Coder Triggering:**
        * **Vision:** Eliminate manual delegation by triggering AI Coder tasks automatically when a GitHub Issue is moved to a specific state (e.g., "Ready for AI Dev").
        * **Considerations:** Requires GitHub Actions integration, robust context identification for the AI Planner/Coder, and resource management.
    * **Automated PR Review & Merge Workflow:**
        * **Vision:** Introduce AI agents for PR review, automated code tweaking based on feedback, and eventual auto-merging for validated changes.
        * **Considerations:** Requires sophisticated AI review capabilities, clear feedback loops, and well-defined merge criteria.
    * **AI Coder Task Status Updates:**
        * **Vision:** Enable the AI Coder to update the associated GitHub Issue status automatically (e.g., add comments, link commits/PRs, close issues).
        * **Status:** Deferred in V2.0 standards revision. Revisit after the core Git workflow (branching, commit, PR creation) proves stable.
    * **Advanced Parallel Workflows:**
        * **Vision:** Explore scalable parallel execution models (e.g., leveraging Git Worktrees) for multiple AI Coders working on independent tasks simultaneously. [Ref: Inspired by Anthropic Best Practices]
    * **Agent Framework (MCP-enabled API):**
        * **Vision:** Develop the internal "Agent Framework" API mentioned in the personal roadmap, potentially using concepts like the Model Context Protocol (MCP), to provide context-aware LLM capabilities with integrated skills (DB access, Git, etc.).
        * **Status:** Currently conceptual; requires significant design and implementation. Cookbook Factory serves as an initial validation.

## 2. Configuration Management & Service Availability

* **Refactor Configuration Handling:**
    * **Goal:** Transition from the current runtime `ConfigurationMissingException` model to one where missing configuration gracefully disables specific services/features instead of causing runtime errors upon use.
    * **Vision:** Allow the application to start and operate with partial functionality based on the provided configuration subset. Enable runtime discovery (e.g., via Swagger/Health Checks) of which features are active.
    * **Status:** Major refactoring task planned for the near future.

## 3. Architecture & Service Enhancements

* **Scalability Improvements (Persistence):**
    * **Goal:** Address potential scalability bottlenecks of the current file-based storage (`FileService`) for recipes, orders, and customers.
    * **Solution:** Migrate relevant repositories (`RecipeRepository`, `OrderRepository`, `CustomerRepository`) to use a more scalable database solution (e.g., PostgreSQL, leveraging JSONB or dedicated tables).
    * **Status:** Recognized future need; requires significant effort.
* **Error Handling Granularity:**
    * **Goal:** Improve error handling specificity in services like `PaymentService` (webhooks), `LlmService` (content filtering), `GitHubService`, etc., for better diagnostics and potential recovery.
* **Caching Strategy:**
    * **Goal:** Implement caching (e.g., in-memory or distributed) for services like `RecipeService` (AI ranking results, scraped data), `EmailService` (validation results), etc., to reduce latency and costs.

## 4. Documentation & Standards Refinements

* **Diagramming Standards (`DiagrammingStandards.md`):**
    * **Goal:** Refine based on practical usage and AI feedback to improve clarity and renderer compatibility.
    * **Status:** Deferred in V2.0 standards revision.
* **Test Data Builders (`TestData/Builders/`):**
    * **Goal:** Implement builders for remaining core entities/DTOs (`Customer`, `Order`, etc.) to improve test setup consistency.
* **Code Coverage:**
    * **Goal:** Systematically increase unit test coverage towards comprehensive coverage excellence defined in `TestingStandards.md`.
* **Documentation Audit:**
    * **Goal:** Establish a regular cadence for using the AI Documentation Auditor workflow (`Docs/Maintenance/DocAuditorAssistant.md`) to maintain high-quality, up-to-date documentation across the project.

## 5. Product & Feature Development (Cookbook & Beyond)

* **Cookbook Factory v1.1+:**
    * Implement remaining integration/E2E tests.
    * Reach target test coverage (>=80% initially per personal roadmap).
    * Develop a basic front-end interface (July 2025 target).
    * UX improvements based on initial user feedback (Nov 2025 target).
* **Article Generator:**
    * New feature using LLMs to generate content (Sep 2025 target).
* **Micro-app Strategy:**
    * Ideation and implementation of further plug-in micro-apps leveraging the modular API server template (Dec 2025 - Jan 2026 targets).

---