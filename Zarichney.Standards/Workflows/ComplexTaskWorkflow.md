# AI Coder Workflow Steps: TDD / Plan-First

**Version:** 1.1
**Last Updated:** 2025-05-04

## Overview

This document details the mandatory step-by-step workflow for an AI Coder agent performing complex development tasks, such as implementing new features, significant refactoring, or tasks with ambiguous requirements. This workflow emphasizes upfront planning and a Test-Driven Development (TDD) approach. These steps are referenced by the AI Coder Prompt Template (`/Docs/Templates/AICoderPromptTemplate.md`) when the "TDD/Plan-First" workflow is selected.

**AI Coder Agent:** You MUST follow these steps sequentially and precisely.

## TDD / Plan-First Workflow Steps

1.  **Identify & Acknowledge Task:**
    * Locate the task details using the 'Associated Task' link/ID provided in Section 2 of the prompt.
    * Read the issue description and comments on GitHub.
    * Briefly acknowledge your understanding of the task requirements (e.g., "Acknowledged: Implement new PDF generation service, issue #456.").

2.  **Review Assignment:**
    * Thoroughly understand the Context Recap (Section 1 of the prompt) and the Specific Coding Task details (Section 5 of the prompt).

3.  **Review Standards & Context:**
    * Carefully read all documentation listed in Section 3 of the prompt, paying close attention to the specified Key Sections.
    * Internalize the rules from:
        * `/Docs/Standards/CodingStandards.md` (references `.editorconfig`)
        * `/Docs/Standards/TaskManagementStandards.md`
        * `/Docs/Standards/DocumentationStandards.md`
        * `/Docs/Standards/DiagrammingStandards.md`
        * `/Docs/Standards/TestingStandards.md`
    * Understand the local context, contracts, and assumptions from the relevant module `README.md`(s) and associated diagrams identified in the prompt.

4.  **Create Feature Branch:**
    * Ensure your local repository clone is up-to-date with the specified base branch (e.g., `develop`, `main`).
      ```bash
      git checkout [BASE_BRANCH_FROM_PROMPT]
      git pull origin [BASE_BRANCH_FROM_PROMPT]
      ```
    * Create and checkout a new feature branch using the naming convention specified in `/Docs/Standards/TaskManagementStandards.md` Section 4 (e.g., `feature/issue-456-add-pdf-service`).
      ```bash
      git checkout -b feature/issue-{ISSUE_ID}-{brief-description}
      ```

5.  **Develop Implementation Plan:**
    * Based on the task requirements and your understanding of the context/standards, outline the specific steps, components, and interactions needed to implement the feature or change.
    * Consider potential edge cases, required dependencies, and necessary configuration.
    * **Output:** Provide the detailed implementation plan for review before proceeding. *(The AI Planner may handle this step or delegate it based on complexity).* **Stop and wait for approval of the plan if instructed by the prompt.**

6.  **Write/Update Tests (TDD):**
    * Based on the approved plan and task requirements, add **new** unit or integration tests that cover the desired functionality.
    * These tests **SHOULD initially FAIL** because the implementation code does not exist yet.
    * If modifying existing functionality, update relevant existing tests to reflect the new requirements; ensure these updates also cause failures initially.
    * Tests **MUST** adhere strictly to `/Docs/Standards/TestingStandards.md`.

7.  **Verify New/Updated Tests Fail as Expected:**
    * Run the specific tests you added or modified in step 6.
    * Ensure they **FAIL** for the expected reasons (e.g., `NotImplementedException`, incorrect output, missing components). Debug test setup if they fail unexpectedly or pass prematurely.

8.  **Implement Code Changes:**
    * Write the **minimum** amount of code required to make the tests written/updated in step 6 pass.
    * Execute the specific coding actions detailed in Section 5 of the prompt, guided by the plan from step 5.
    * Strictly adhere to `/Docs/Standards/CodingStandards.md`.
    * Respect module boundaries and contracts defined in relevant `README.md`(s).
    * Adhere to any specified boundaries (what *not* to change).

9.  **Verify New/Updated Tests Pass:**
    * Run the specific tests you added or modified in step 6 *again*.
    * Ensure they now **PASS** consistently. Debug and fix any failures in the implementation code or tests until they pass.

10. **Verify All Tests:**
    * Run the **entire test suite** using the command: `dotnet test`.
    * Ensure **ALL** unit and integration tests pass. Fix any failures caused by your changes, potentially iterating steps 8 and 9.

11. **Refactor (Optional but Recommended):**
    * Review the code written in step 8. Refactor for clarity, conciseness, and adherence to principles (DRY, SOLID) *without changing the behavior verified by the tests*.
    * Ensure all tests *still pass* after refactoring.

12. **Update Documentation & Diagrams:**
    * Review the `README.md` file(s) listed in Section 3 of the prompt and the rules in `/Docs/Standards/DocumentationStandards.md` and `/Docs/Standards/DiagrammingStandards.md`.
    * If your code or test changes impact the module's purpose, architecture, interface, assumptions, dependencies, testing strategy, or visualized structures: Update the `README.md` and relevant Mermaid diagram(s) accordingly. Be precise, prune obsolete info, ensure correct relative paths, and adhere to diagramming standards. Update `Last Updated:` date.

13. **Verify Formatting:**
    * Run the .NET formatting tool to ensure code style compliance with `.editorconfig`.
    * Check for violations:
      ```bash
      dotnet format --verify-no-changes --verbosity diagnostic
      ```
    * If violations are reported, fix them automatically:
      ```bash
      dotnet format
      ```
    * Stage the formatting changes if any were made (`git add .`). Ensure the formatter runs successfully without reporting further violations before proceeding.

14. **Commit Changes:** 
    * Stage all relevant changes (code, tests, documentation, diagrams, formatting fixes).
    * Commit the changes to your feature branch using a Conventional Commit message referencing the Associated Task ID. Adhere to `/Docs/Standards/TaskManagementStandards.md` Section 5.
      ```bash
      git add . # Ensure formatting fixes are staged
      git commit -m "feat: Add initial PDF generation service (#456)" # Example commit
      ```

15. **Create Pull Request:** 
    * Push your feature branch to the origin repository.
      ```bash
      git push origin feature/issue-{ISSUE_ID}-{brief-description}
      ```
    * Create a Pull Request using the GitHub CLI (`gh`). Ensure the title and body adhere to `/Docs/Standards/TaskManagementStandards.md` Section 6, targeting the branch specified in the prompt.
      ```bash
      # Example: gh pr create --base develop --title "feat: Add initial PDF generation service (#456)" --body "Closes #456. Implements the core PDF service using QuestPDF."
      gh pr create --base [TARGET_BRANCH_FROM_PROMPT] --title "<type>: <Brief description> (#ISSUE_ID)" --body "Closes #{ISSUE_ID}. [Summary of changes]"
      ```

16. **Provide Output:** 
    * Report the final commit hash on your feature branch and the URL of the created Pull Request as specified in Section 6 of the prompt.

---