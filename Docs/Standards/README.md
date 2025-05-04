# Module/Directory: Docs/Standards

**Last Updated:** 2025-05-04

> **Parent:** [`Docs`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory contains the official, **mandatory standards** that govern all aspects of software development for the Zarichney API project.
* **Goal:** To ensure consistency, quality, maintainability, and robustness across the codebase and its associated documentation and tests.
* **Critical Importance for AI Workflow:** Adherence to these standards is **essential** for the success of the AI-assisted development workflow. Both AI Planning Assistants and AI Coders rely heavily on these documents to understand expectations and perform tasks correctly and safely. Prompts and workflow steps will explicitly instruct AI agents to consult and adhere to these standards.

## 2. Overview of Standards Documents

This directory contains the following key standards documents:

* **[`CodingStandards.md`](./CodingStandards.md):**
    * Defines rules and best practices for writing C# code within this project.
    * Covers naming, formatting (referencing `.editorconfig`), architecture (DI, file structure), error handling, logging, async patterns, security considerations, and more.
    * *Mandates* updates to tests, documentation, and diagrams when code changes.
* **[`DocumentationStandards.md`](./DocumentationStandards.md):**
    * Defines rules for creating and maintaining the per-directory `README.md` files.
    * Focuses on structure (using `ReadmeTemplate.md`), content philosophy (context for AI), linking strategy, and mandatory updates.
* **[`TestingStandards.md`](./TestingStandards.md):**
    * Defines rules for automated unit and integration testing.
    * Covers required tooling (xUnit, Moq, FluentAssertions, Testcontainers), test structure (AAA), naming, categorization (Traits), mocking strategies, assertion requirements, and database handling.
* **[`DiagrammingStandards.md`](./DiagrammingStandards.md):**
    * Defines rules for creating and maintaining architecture and workflow diagrams using Mermaid.js.
    * Covers diagram types, location, styling conventions, linking, complexity management, and mandatory syntax rules for reliable rendering.
* **[`TaskManagementStandards.md`](./TaskManagementStandards.md):**
    * Defines rules for integrating development tasks with GitHub Issues and managing the Git workflow.
    * Covers issue usage/templates, branch naming conventions, Conventional Commits standard, and Pull Request creation procedures using the `gh` CLI. Includes mandatory formatting verification (`dotnet format`).
* **Related Template:** [`../Templates/ReadmeTemplate.md`](../Templates/ReadmeTemplate.md) defines the mandatory structure referenced by `DocumentationStandards.md`.

## 3. How to Use These Standards

* **Developers (Human & AI):** Before starting any development task (coding, testing, documentation), review the relevant standard(s) to ensure compliance.
* **AI Planning Assistant:** References these standards when validating plans and generating prompts.
* **AI Coder:** Explicitly instructed by prompts and workflow steps files to consult and strictly adhere to these standards during task execution.

---