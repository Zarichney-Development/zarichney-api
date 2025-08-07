# Module/Directory: Docs

**Last Updated:** 2025-08-07

> **Parent:** [`/`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory is the central repository for all documentation related to the Zarichney API project. It includes coding standards, development workflows, reusable templates, maintenance guides, and architectural context.
* **Goal:** To provide clear, comprehensive, and up-to-date documentation that supports both human developers and the AI-assisted development processes used in this project. High-quality documentation is critical for enabling effective AI Coder delegation and maintaining consistency.

## 2. Documentation Structure Overview

The `/Docs` directory is organized into the following key areas:

* **[`/Docs/Development/`](./Development/README.md): AI-Assisted Workflow & Planning**
    * Defines the structured workflow leveraging AI Planning Assistants and AI Coders.
    * Contains the prompt for the AI Planning Assistant (`CodingPlannerAssistant.md`).
    * Details the specific step-by-step workflows used by AI Coders (`StandardWorkflow.md`, `ComplexTaskWorkflow.md`, `TestCoverageWorkflow.md`).
    * Houses the project's short-term technical roadmap (`ShortTermRoadmap.md`).
    * **Entry Point:** Start here to understand *how* development is performed using AI assistance.

* **[`/Docs/Standards/`](./Standards/README.md): Mandatory Rules & Conventions**
    * Contains the official standards that **MUST** be followed for all development work.
    * Includes rules for C# Coding (`CodingStandards.md`), Automated Testing (`TestingStandards.md`), Test Suite Baselines & Progressive Coverage (`TestSuiteStandards.md`), Per-Directory Documentation (`DocumentationStandards.md`), Mermaid Diagramming (`DiagrammingStandards.md`), and Git/Task Management (`TaskManagementStandards.md`).
    * **Entry Point:** Consult these standards to understand the expected quality and conventions for code, tests, and documentation.

* **[`/Docs/Templates/`](./Templates/README.md): Reusable Templates**
    * Stores standardized templates used throughout the development workflow.
    * Includes templates for AI Coder Prompts (`AICoderPromptTemplate.md`, `TestCaseDevelopmentTemplate.md`), GitHub Issues (`GHCoderTaskTemplate.md`, `GHTestCoverageTaskTemplate.md`), and the standard per-directory README structure (`ReadmeTemplate.md`).
    * **Entry Point:** Refer to these templates to understand the expected structure for generated artifacts.

* **[`/Docs/Maintenance/`](./Maintenance/README.md): System & Infrastructure Guides**
    * Provides operational guides, setup instructions, and context for specific systems or infrastructure components.
    * Includes information on Authentication (`AuthenticationSystem.md`), Database (`PostgreSqlDatabase.md`), Cloud Infrastructure (`AmazonWebServices.md`), and potentially other specific maintenance procedures or tooling (`TestingSetup.md`, `DocAuditorAssistant.md`).
    * **Entry Point:** Consult these guides for specific operational or setup tasks related to underlying systems.

## 3. How to Navigate

* **To understand the AI Development Process:** Start with [`/Docs/Development/README.md`](./Development/README.md).
* **To understand Coding/Testing/Doc Rules:** Start with [`/Docs/Standards/README.md`](./Standards/README.md).
* **To find specific templates:** Look in [`/Docs/Templates/`](./Templates/README.md).
* **For system-specific operational info:** Look in [`/Docs/Maintenance/`](./Maintenance/README.md).

---