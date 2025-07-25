# Project Documentation Standards (Per-Directory README.md Files)

**Version:** 1.5
**Last Updated:** 2025-05-22

## 1. Purpose and Scope

This document defines the mandatory standards and practices for creating and maintaining `README.md` files within *each directory* of this codebase.

* **Primary Goal:** To provide essential context, design rationale, interface contracts, and operational guidance specifically tailored for **stateless AI coding assistants** who perform maintenance and development tasks. These READMEs, potentially including embedded architectural diagrams, are critical for enabling AI to understand and safely modify code within a specific directory context.
* **Scope:** This document governs *only* the per-directory `README.md` files. It dictates their structure, content focus, linking strategy, and maintenance requirements.
* **Relationship to Other Standards:**
    * This document complements **[`./CodingStandards.md`](./CodingStandards.md)**. While `CodingStandards.md` focuses on *how to write C# code*, this document focuses exclusively on *how to document code modules via README.md files*.
    * Information within these READMEs, particularly regarding interface contracts, dependencies, and module-specific operational details, directly supports the testing activities outlined in **`Docs/Standards/TestingStandards.md`** (the overarching testing guide), **`Docs/Standards/UnitTestCaseDevelopment.md`**, and **`Docs/Standards/IntegrationTestCaseDevelopment.md`**.
    * Embedded diagrams within READMEs **MUST** adhere to **[`./DiagrammingStandards.md`](./DiagrammingStandards.md)**.
    * The mandatory structure for all READMEs is defined in the **[`./README_template.md`](./README_template.md)** file. This document explains *how* to effectively utilize that template.

## 2. Core Philosophy & Principles

* **Target Audience: Stateless AI:** Write clearly, explicitly, and unambiguously. Assume the reader (a future AI instance) has **no prior context** about the specific module beyond the code in the directory, *this* README, and any embedded diagrams. Structure information logically for efficient parsing.
* **Context is King:** Focus documentation efforts on the *'why'* behind design decisions, implicit assumptions the code makes, non-obvious behaviors, and the module's specific role and boundaries within the larger system. Maximize contextual value for the AI.
* **Visual Clarity:** Supplement textual descriptions with embedded Mermaid diagrams where appropriate (following `DiagrammingStandards.md`) to visually communicate architecture, key workflows, and component relationships.
* **Value over Volume:** Prioritize information *not* immediately obvious from the code or diagrams. **Avoid** simple code-to-English translation or exhaustive lists of public members readily available via static code analysis. The README should supplement, not repeat, the code or diagrams.
* **Maintainability & Pruning:** Documentation (text and diagrams) **MUST** be kept current as the code evolves. Outdated information, especially historical rationale (Section 7) or resolved issues (Section 8) that no longer illuminate the *current* state, **MUST be pruned** during updates. Keep the README focused and relevant.
* **Consistency:** Adhere strictly to the structure defined in **[`../Templates/ReadmeTemplate.md`](../Templates/ReadmeTemplate.md)** for all per-directory README files.
* **Discoverability (Linking):** Create a navigable documentation network by consistently linking between parent, child, and related module READMEs using relative paths. This is crucial for AI navigation.

## 3. Utilizing the `README_template.md`

The **[`../Templates/ReadmeTemplate.md`](../Templates/ReadmeTemplate.md)** file provides the mandatory structure for all directory-specific READMEs. Ensure every section is thoughtfully completed or explicitly marked as not applicable. Key sections and their intent for the AI audience:

* **Header & Parent Link:** Essential for identification and navigation within the documentation network.
* **Section 1 (Purpose & Responsibility):** High-level overview. *What* does this module do functionally? *Why* does it exist as a separate unit? Crucially links to child READMEs if they exist.
* **Section 2 (Architecture & Key Concepts):** Internal design overview. *How* does it work internally? Key components, data structures, and interactions with immediate neighbors. Focus on the conceptual model, not exhaustive detail. This section is the primary location for embedding relevant Mermaid diagrams visualizing the module's **architecture or key workflows/sequences**. Diagrams illustrating data flow, state transitions, or complex decision logic can be particularly useful for designing comprehensive test cases.
* **Section 3 (Interface Contract & Assumptions):** **CRITICAL Section.** Defines the rules for *interacting* with this module from the outside. **MUST** focus intensely on *preconditions (required input states/values), postconditions (expected output states/values, side effects), non-obvious error handling (specific exceptions thrown under certain conditions), invariants,* and *critical assumptions* the code makes about its inputs, dependencies, or environment. This information is vital for designing both positive and negative test cases. This is **NOT** for simply listing public method signatures but for explaining the *behavioral contract*. Diagrams in Section 2 may help illustrate these contracts.
* **Section 4 (Local Conventions & Constraints):** Rules specific to *this* directory that augment or override global standards. **MUST** detail local deviations. This includes any specific configuration values or environmental conditions that are critical for the module's operation *and testing*. For example, if certain features of this module behave differently or require special setup for testing (e.g., specific test data profiles, mock configurations not handled globally), detail them here.
* **Section 5 (How to Work With This Code):** Practical guidance for developers/AI. **MUST** include:
    * Setup steps unique to this module.
    * **Module-Specific Testing Strategy:** Briefly outline the primary testing approach for this module (e.g., "Primarily unit tested due to complex internal logic; integration tests cover API contract via `PublicControllerTests`").
    * **Key Test Scenarios:** Highlight any particularly important, complex, or non-obvious scenarios that **must** be covered by tests for this module.
    * **Test Data Considerations:** Mention any specific types of test data or data generation strategies that are particularly relevant for testing this module's logic or edge cases.
    * Specific commands to run tests relevant to this module (if different from global commands).
    * Known pitfalls or non-obvious behaviors ('gotchas') that could affect development or testing.
* **Section 6 (Dependencies):** Maps the module's place in the system. **MUST** list:
    * Key internal modules directly consumed by this module (link to their READMEs). *Understanding these is crucial for mocking dependencies in unit tests.*
    * Key internal modules that directly consume this module (link to their READMEs). *Understanding these helps assess impact of changes.*
    * Key external libraries or services (e.g., specific NuGet packages with non-obvious configurations, external SaaS providers). *Highlight any that have specific implications for testing, such as requiring virtualization in integration tests.*
      This information is critical for understanding change impact and for designing appropriate test doubles or test environment configurations.
* **Section 7 (Rationale & Key Historical Context):** Explain *why* the current design exists, especially non-obvious choices. Include historical notes *only* if they illuminate the *present* state. **Prune aggressively** when context becomes obsolete.
* **Section 8 (Known Issues & TODOs):** Track current limitations or planned work *specific* to this module. Remove items when resolved.

## 4. Linking Strategy (Mandatory)

Create a navigable documentation network for the AI:

* **Parent Link:** Every README (except the root `README.md`) MUST link to its immediate parent directory's README using a relative path (e.g., `../README.md`).
* **Child Links:** A README MUST link to the README of each immediate subdirectory that contains significant code/config and has its own README (Section 1). Use relative paths (e.g., `./SubModule/README.md`).
* **Dependency/Dependent Links:** When mentioning other internal modules in Section 6 (Dependencies/Dependents), ALWAYS link to their respective README files using relative paths (e.g., `../../OtherModule/README.md`). Do not cite files, using the `[cite]` tag.
* **Diagram Links (Optional):** If complex diagrams are stored in separate `.mmd` files (per `DiagrammingStandards.md`), link to them from Section 2 using relative paths (e.g., `../../../docs/diagrams/MyModule/DetailedFlow.mmd`).

## 5. Maintenance and Updates (AI Coder Responsibility)

* **Trigger:** Any task (performed by human or AI) that modifies the code or associated tests within a directory in a way that impacts its documented purpose, architecture, interface contracts, assumptions, dependencies, **module-specific testing considerations (as outlined in Section 5 of the README template)**, known issues, or **visualized architecture/flows** **MUST** also update the corresponding `README.md` file (including any embedded or linked diagrams) within the same commit/change, following the standards herein and in **[`./DiagrammingStandards.md`](./DiagrammingStandards.md)**.
* **Pruning:** When updating, review Section 7 (Rationale) and Section 8 (Known Issues) and **actively remove** any information that is no longer relevant due to the code changes. Keep the README focused on the *current* state.
* **`Last Updated` Date:** Always update the `Last Updated: [YYYY-MM-DD]` field at the top of the README when making any changes to its text or embedded diagrams.
* **Annotation Instruction:** **Do NOT** include explanatory annotations like "<-- UPDATED -->" or "`// Updated this line`" within the documentation content itself.

---
