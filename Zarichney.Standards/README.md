# Module/Directory: Docs

**Version:** 2.0
**Last Updated:** 2025-05-04

> **Parent:** [`/`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory is the central repository for all documentation related to the Zarichney API project. It includes coding standards, development workflows, reusable templates, maintenance guides, and architectural context.
* **Goal:** To provide clear, comprehensive, and up-to-date documentation that supports both human developers and the AI-assisted development processes used in this project. High-quality documentation is critical for enabling effective AI Coder delegation and maintaining consistency.

## 2. Documentation Structure Overview

The `/Zarichney.Standards` directory is organized into the following key areas:

* **[`/Zarichney.Standards/Coding/`](./Coding/README.md): Coding Standards & Guidelines**
    * Contains the official coding standards that **MUST** be followed for all development work.
    * Includes rules for C# Coding (`CodingStandards.md`).
    * **Entry Point:** Consult these standards to understand the expected quality and conventions for code.

* **[`/Zarichney.Standards/Documentation/`](./Documentation/README.md): Documentation Standards**
    * Contains standards for creating and maintaining documentation.
    * Includes rules for Per-Directory Documentation (`DocumentationStandards.md`) and Mermaid Diagramming (`DiagrammingStandards.md`).
    * **Entry Point:** Consult these standards to understand documentation requirements.

* **[`/Zarichney.Standards/Development/`](./Development/README.md): Development Setup & Guides**
    * Provides guides and information for local development setup and common development tasks.
    * Includes local setup guides (`LocalSetup.md`), logging configuration (`LoggingGuide.md`), and testing setup (`TestingSetup.md`).
    * **Entry Point:** Start here for development environment setup and common development tasks.

* **[`/Zarichney.Standards/Testing/`](./Testing/README.md): Testing Standards & Guides**
    * Contains the official testing standards and development guides for writing tests.
    * Includes rules for Automated Testing (`TestingStandards.md`), Unit Test Development (`UnitTestCaseDevelopment.md`), and Integration Test Development (`IntegrationTestCaseDevelopment.md`).
    * **Entry Point:** Consult these standards to understand testing requirements and best practices.

* **[`/Zarichney.Standards/Workflows/`](./Workflows/README.md): AI-Assisted Development Workflows**
    * Defines the structured workflow leveraging AI Planning Assistants and AI Coders.
    * Contains the prompt for the AI Planning Assistant (`CodingPlannerAssistant.md`).
    * Details the specific step-by-step workflows used by AI Coders (`StandardWorkflow.md`, `ComplexTaskWorkflow.md`, `TestCoverageWorkflow.md`).
    * Houses the project's short-term technical roadmap (`ShortTermRoadmap.md`) and Git/Task Management (`TaskManagementStandards.md`).
    * **Entry Point:** Start here to understand *how* development is performed using AI assistance.

* **[`/Zarichney.Standards/Templates/`](./Templates/README.md): Reusable Templates**
    * Stores standardized templates used throughout the development workflow.
    * Includes templates for AI Coder Prompts (`AICoderPromptTemplate.md`, `TestCaseDevelopmentTemplate.md`), GitHub Issues (`GHCoderTaskTemplate.md`, `GHTestCoverageTask.md`), and the standard per-directory README structure (`ReadmeTemplate.md`).
    * **Entry Point:** Refer to these templates to understand the expected structure for generated artifacts.

* **[`/Zarichney.Standards/Maintenance/`](./Maintenance/README.md): System & Infrastructure Guides**
    * Provides operational guides, setup instructions, and context for specific systems or infrastructure components.
    * Includes information on Authentication (`AuthenticationSystem.md`), Database (`PostgreSqlDatabase.md`), Cloud Infrastructure (`AmazonWebServices.md`), and documentation quality assurance (`DocAuditorAssistant.md`).
    * **Entry Point:** Consult these guides for specific operational or setup tasks related to underlying systems.

## 3. How to Navigate

* **To understand the AI Development Process:** Start with [`/Zarichney.Standards/Workflows/README.md`](./Workflows/README.md).
* **To understand Coding Standards:** Start with [`/Zarichney.Standards/Coding/README.md`](./Coding/README.md).
* **To understand Testing Standards:** Start with [`/Zarichney.Standards/Testing/README.md`](./Testing/README.md).
* **To understand Documentation Standards:** Start with [`/Zarichney.Standards/Documentation/README.md`](./Documentation/README.md).
* **To find specific templates:** Look in [`/Zarichney.Standards/Templates/`](./Templates/README.md).
* **For development setup and guides:** Look in [`/Zarichney.Standards/Development/`](./Development/README.md).
* **For system-specific operational info:** Look in [`/Zarichney.Standards/Maintenance/`](./Maintenance/README.md).

---