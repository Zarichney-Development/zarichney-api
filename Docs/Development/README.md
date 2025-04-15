# Module/Directory: Docs/Development

**Last Updated:** 2025-04-15

> **Parent:** [`Docs`](../README.md) [cite: Docs/README.md]

## 1. Purpose & Responsibility

* **What it is:** This directory houses documentation defining the standards, workflows, and templates specifically governing the *development process* of the Zarichney API application, with a focus on facilitating AI-assisted development.
* **Key Responsibilities:**
    * Defining coding standards and best practices (`CodingStandards.md`).
    * Defining documentation standards for per-directory README files (`DocumentationStandards.md`).
    * Defining automated testing standards (`TestingStandards.md`).
    * Providing the standard template for per-directory README files (`README_template.md`).
    * Outlining the AI-assisted workflow for code planning and implementation (`CodingPlannerAssistant.md`).
* **Why it exists:** To establish a clear, consistent, and high-quality development process supported by AI assistants, ensuring maintainability and effective collaboration between human developers and AI agents.
* **Documents within this Directory:**
    * [`CodingPlannerAssistant.md`](./CodingPlannerAssistant.md) - Defines the workflow for using an AI assistant to plan coding tasks and generate prompts for AI coders. [cite: Docs/Development/CodingPlannerAssistant.md]
    * [`CodingStandards.md`](./CodingStandards.md) - Defines the rules and best practices for writing C# code within this project. Must be consulted by AI Coders. [cite: Docs/Development/CodingStandards.md]
    * [`DocumentationStandards.md`](./DocumentationStandards.md) - Defines the purpose, structure, and maintenance requirements for per-directory `README.md` files. Must be consulted by AI Coders when updating documentation. [cite: Docs/Development/DocumentationStandards.md]
    * [`TestingStandards.md`](./TestingStandards.md) - Defines the rules, frameworks, and quality expectations for automated tests (`unit` and `integration`). Must be consulted by AI Coders.
    * [`README_template.md`](./README_template.md) - The mandatory template to be used for creating all new per-directory `README.md` files. [cite: Docs/Development/README_template.md]

## 2. Architecture & Key Concepts

* **AI-Assisted Development Workflow:** The core workflow leverages AI assistants:
    1.  **Planning:** A long-running "AI Coding Planning Assistant" instance is used, referencing these standards and existing READMEs, to discuss requirements, validate plans, and decompose work into incremental tasks [cite: Docs/Development/CodingPlannerAssistant.md].
    2.  **Prompt Generation:** The Planning Assistant generates detailed, self-contained prompts for each task, explicitly referencing relevant `README.md` files and standards documents (`CodingStandards.md`, `DocumentationStandards.md`, `TestingStandards.md`).
    3.  **Implementation:** Stateless "AI Coder" instances execute these prompts, modifying code according to `CodingStandards.md`, adding/updating tests according to `TestingStandards.md`, and updating the corresponding `README.md` files according to `DocumentationStandards.md`.
* **Documentation-Centric:** Per-directory `README.md` files (based on `README_template.md`) are crucial artifacts, providing essential context for both human developers and AI assistants, as mandated by `DocumentationStandards.md`.

## 3. Interface Contract & Assumptions

* Not applicable for this documentation directory. Contracts are defined within the codebase and documented in specific module READMEs.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* Not applicable for this documentation directory. Conventions are defined *within* the standard documents located here.

## 5. How to Work With This Documentation

* **Intended Workflow:**
    1.  **Engage with Planning Assistant:** Provide an AI Planning Assistant (like Gemini, with access to the full codebase via file upload) with the `CodingPlannerAssistant.md` prompt.
    2.  **Define Goals:** Explain the desired changes or features for the codebase.
    3.  **Collaborative Planning:** Work with the Planning Assistant to clarify requirements, validate the plan against existing documentation (module READMEs, standards), and decompose the work into logical, incremental coding tasks.
    4.  **Generate Coder Prompts:** The Planning Assistant will generate detailed, self-contained prompts for each incremental task, following the template in `CodingPlannerAssistant.md`.
    5.  **Delegate to AI Coder:** Use these generated prompts to delegate the implementation tasks to separate, stateless AI Coder instances.
* **Mandatory Reading:**
    * **Before Coding:** Developers and AI assistants **MUST** consult `CodingStandards.md`.
    * **Before Documenting:** Developers and AI assistants **MUST** consult `DocumentationStandards.md` and use `README_template.md`.
    * **Before Testing:** Developers and AI assistants **MUST** consult `TestingStandards.md`.
* **AI Collaboration Guidance:** Use `CodingPlannerAssistant.md` as the foundational prompt and guide for interacting with the Planning Assistant.

## 6. Dependencies

* **Parent:** [`Docs`](../README.md) - This directory is a child of the main documentation directory.
* **Codebase:** The standards defined here influence the entire codebase structure, code style, testing practices, and all per-directory `README.md` files throughout the project.

## 7. Rationale & Key Historical Context

* This centralized set of development documentation was created to ensure consistency, maintain quality, and enable effective collaboration with AI assistants in the development lifecycle. Defining explicit standards and workflows is crucial for guiding AI behavior.

## 8. Known Issues & TODOs

* **Maintenance:** All standards documents (`CodingStandards.md`, `DocumentationStandards.md`, `TestingStandards.md`) require periodic review and updates as the project evolves and best practices change.
* **Enforcement:** Consistency relies on adherence to these standards by both human developers and the AI assistants executing tasks.