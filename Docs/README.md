# Module/Directory: Docs

**Last Updated:** 2025-09-07

> **Parent:** [`/`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory is the central repository for all documentation related to the Zarichney API project. It includes coding standards, development workflows, reusable templates, maintenance guides, and architectural context.
* **Goal:** To provide clear, comprehensive, and up-to-date documentation that supports both human developers and the **12-agent specialized development team** used in this project. High-quality documentation is critical for enabling effective agent delegation, documentation grounding protocols, and maintaining consistency across coordinated team efforts.

## 2. Documentation Structure Overview

The `/Docs` directory is organized into the following key areas:

* **[`/Docs/Development/`](./Development/README.md): AI-Orchestrated Development & Planning**
    * Defines the structured workflow leveraging a strategic codebase manager orchestrating 11 specialized AI agents.
    * Contains architectural evolution documentation (`CodebaseManagerEvolution.md` - archived, historical architectural evolution context) detailing the transformation from executor to orchestrator model.
    * Details the specific step-by-step workflows used by specialized agents with comprehensive documentation grounding protocols.
    * **Features Coverage Epic Merge Orchestrator** (`CoverageEpicMergeOrchestration.md`) for automated multi-PR consolidation with AI conflict resolution.
    * **Enhanced Coverage Epic Automation** (`AutomatedCoverageEpicWorkflow.md`) showing complete 3-phase pipeline from individual agent execution through orchestrator consolidation.
    * Houses the project's short-term technical roadmap (`ShortTermRoadmap.md`).
    * **Entry Point:** Start here to understand *how* development is performed using coordinated agent teams including orchestrator consolidation.

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

* **To understand the 12-Agent Development Process:** Start with [`/Docs/Development/README.md`](./Development/README.md) and [`/Docs/Archive/legacy-workflows/CodebaseManagerEvolution.md`](./Archive/legacy-workflows/CodebaseManagerEvolution.md) (archived - historical architectural evolution context).
* **To understand Coverage Epic Orchestration:** See [`/Docs/Development/CoverageEpicMergeOrchestration.md`](./Development/CoverageEpicMergeOrchestration.md) for multi-PR consolidation workflows.
* **To understand Complete Coverage Epic Pipeline:** Review [`/Docs/Development/AutomatedCoverageEpicWorkflow.md`](./Development/AutomatedCoverageEpicWorkflow.md) for 3-phase workflow from individual agents through orchestrator consolidation.
* **To understand Coding/Testing/Doc Rules:** Start with [`/Docs/Standards/README.md`](./Standards/README.md).
* **To find specific templates:** Look in [`/Docs/Templates/`](./Templates/README.md).
* **For system-specific operational info:** Look in [`/Docs/Maintenance/`](./Maintenance/README.md).
* **For specialized agent configurations:** See [`/.claude/agents/`](../.claude/agents/) for individual agent instruction files with documentation grounding protocols.

---