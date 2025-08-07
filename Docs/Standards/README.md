# README: /Standards Directory

**Version:** 1.3
**Last Updated:** 2025-08-07
**Parent:** `../README.md`

## 1. Purpose & Importance

This directory, `/Docs/Standards/`, contains all **mandatory** development, operational, and quality assurance standards for the `zarichney-api` project. These documents collectively define the rules, best practices, and conventions that all contributors must adhere to.

The primary importance of these standards is to ensure:
* **High Quality & Reliability:** Producing robust, correct, and well-tested software.
* **Consistency:** Maintaining a uniform approach to coding, documentation, testing, and task management across the project.
* **Maintainability & Scalability:** Creating a codebase that is easy to understand, modify, extend, and debug over time.
* **Testability:** Ensuring that code is designed and written in a way that facilitates comprehensive automated testing.
* **Effective Collaboration:** Providing a common understanding and set of expectations for all team members, including human developers and AI coding assistants.

**Adherence to these standards is non-negotiable for all contributions to this project.** They are foundational to our development process and critical for achieving our quality goals, especially in an AI-assisted development workflow.

## 2. Overview of Standards Documents

Below is a list of the key standards documents within this directory. Each document provides detailed guidelines for its respective area. It is expected that all contributors familiarize themselves with these standards relevant to their tasks.

* **`./CodingStandards.md`**
    * **Description:** Defines the mandatory guidelines for writing C# code, including naming conventions, style, architectural patterns (like Dependency Injection and SOLID), designing for testability (e.g., Humble Object Pattern), error handling, and security best practices.
* **`./TestingStandards.md`**
    * **Description:** Outlines the overarching philosophy, mandatory tooling, project-wide conventions (naming, categorization), developer workflows, and general quality expectations for all automated testing activities. This document also includes strategies for AI agent consumption of testing standards.
* **`./UnitTestCaseDevelopment.md`**
    * **Description:** A detailed guide on how to write effective unit tests, covering principles of isolation, mocking dependencies (Moq), writing assertions (FluentAssertions), test data management (AutoFixture), testing specific scenarios (async, exceptions), and avoiding common pitfalls. (Companion to `TestingStandards.md`).
* **`./IntegrationTestCaseDevelopment.md`**
    * **Description:** A comprehensive guide for writing integration tests, detailing the use of the testing framework (`CustomWebApplicationFactory`, `DatabaseFixture`, `ApiClientFixture`), Testcontainers, Refit client, simulating authentication, virtualizing external services (WireMock.Net), and managing data for integration scenarios. (Companion to `TestingStandards.md`).
* **`./DocumentationStandards.md`**
    * **Description:** Specifies the requirements for creating and maintaining per-directory `README.md` files and XML documentation comments within the codebase. It emphasizes clear, contextual documentation targeted at stateless AI coding assistants.
* **`./DiagrammingStandards.md`**
    * **Description:** Provides guidelines for creating, maintaining, and embedding Mermaid diagrams used for visualizing architecture, workflows, and component relationships.
* **`./TaskManagementStandards.md`**
    * **Description:** Defines the process for managing development tasks, including issue tracking conventions (e.g., GitHub Issues), pull request procedures, branching strategies, and commit message formatting.
* **`./TestSuiteStandards.md`**
    * **Description:** Comprehensive standards for test suite baseline management, progressive coverage framework (14.22% → 90% by Jan 2026), environment-specific quality gates, skip categorization taxonomy, and AI-powered analysis integration. Establishes the foundation for systematic test suite health monitoring and improvement.

## 3. Target Audience & Application

These standards are intended for:
* **Software Developers:** All human engineers contributing to the project.
* **AI Coding Assistants:** AI agents tasked with code generation, modification, testing, or documentation. The clarity and explicitness of these standards are particularly important for effective AI collaboration.
* **Architects & Technical Leads:** For defining and upholding the technical vision and quality benchmarks.
* **QA Engineers / Testers:** For understanding testing expectations and strategies.

**How to Use:**
* **Onboarding:** New contributors (human or AI configurations) must review relevant standards as part of their onboarding.
* **Task Initiation:** Before starting any development task, review the applicable standards (e.g., `CodingStandards.md` and testing guides before coding and writing tests).
* **During Development:** Continuously refer to these standards to ensure compliance.
* **Code/Documentation Reviews:** Standards serve as the objective criteria for reviewing contributions.
* **AI Prompting:** When prompting AI assistants, explicitly refer them to the relevant standards documents to guide their output.

## 4. Maintenance & Evolution of Standards

These standards are living documents. They will be reviewed periodically and updated as the project evolves, new technologies are adopted, or improved practices are identified.
* **Versioning:** Each standards document includes a version number and a "Last Updated" date to track changes.
* **Change Proposals:** Suggestions for improving these standards can be made through the project's standard contribution process (e.g., by raising an issue or a pull request against the relevant standards document).
* **Review:** Significant changes to standards will be reviewed by the technical leadership before adoption.

## 5. Key Interdependencies Between Standards

It's important to recognize that these standards are interconnected and often reinforce each other:
* Adherence to **`./CodingStandards.md`** (e.g., DI, SOLID, Humble Object pattern) is fundamental for creating code that can be effectively tested according to **`./TestingStandards.md`** and its detailed guides.
* **`./DocumentationStandards.md`** ensures that modules are well-explained, which aids in understanding how to test them and how to apply coding standards consistently.
* **`./TaskManagementStandards.md`** ensures that the process of developing code, tests, and documentation according to these standards is orderly and traceable.

By understanding and applying these standards comprehensively, all contributors can help build a high-quality, maintainable, and robust `zarichney-api` application.

---
