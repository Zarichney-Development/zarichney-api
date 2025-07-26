# AI Coder Workflow Steps: Standard

**Version:** 1.2
**Last Updated:** 2025-07-25

## Overview

This document details the mandatory step-by-step workflow for an AI Coder agent performing standard development tasks, such as implementing straightforward features, bug fixes, or simple refactoring. These steps are referenced by the AI Coder Prompt Template (`/Docs/Templates/AICoderPromptTemplate.md`) when the "Standard" workflow is selected.

**AI Coder Agent:** You MUST follow these steps sequentially and precisely.

## Standard Workflow Steps

1.  **Identify & Acknowledge Task:**
    * Locate the task details using the 'Associated Task' link/ID provided in Section 2 of the prompt.
    * Read the issue description and comments on GitHub.
    * Briefly acknowledge your understanding of the task requirements (e.g., "Acknowledged: Implement search endpoint for recipes, issue #123.").

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
    * Create and checkout a new feature branch using the naming convention specified in `/Docs/Standards/TaskManagementStandards.md` Section 4 (e.g., `feature/issue-123-add-search-endpoint`).
      ```bash
      git checkout -b feature/issue-{ISSUE_ID}-{brief-description}
      ```

5.  **Implement Code Changes:**
    * Execute the specific coding actions detailed in Section 5 of the prompt.
    * Strictly adhere to `/Docs/Standards/CodingStandards.md`.
    * Respect module boundaries and contracts defined in relevant `README.md`(s).
    * Implement the required logic, algorithms, conditions, and expected behavior.
    * Adhere to any specified boundaries (what *not* to change).

6.  **Add/Update Tests:**
    * Add new unit or integration tests (or update existing ones) to cover the changes made in step 5.
    * Tests **MUST** adhere strictly to `/Docs/Standards/TestingStandards.md`. Focus on testing behavior and ensuring resilience.

7.  **Verify New/Updated Tests:**
    * Run the specific tests you added or modified using the unified test suite:
      ```bash
      # Quick validation
      /test-report summary
      
      # Or for specific test categories
      ../../Scripts/run-test-suite.sh report --unit-only
      ```
    * Ensure they **PASS** consistently. Debug and fix any failures in the code or tests until they pass.

8.  **Verify All Tests:**
    * Run the **entire test suite** with AI-powered analysis:
      ```bash
      # Full test suite with recommendations
      /test-report
      
      # Or using the script directly
      ../../Scripts/run-test-suite.sh report
      ```
    * Ensure **ALL** unit and integration tests pass and quality gates are met.
    * Review any recommendations from the AI analysis and address critical issues.
    * Fix any failures caused by your changes.

9.  **Update Documentation & Diagrams:**
    * Review the `README.md` file(s) listed in Section 3 of the prompt and the rules in `/Docs/Standards/DocumentationStandards.md` and `/Docs/Standards/DiagrammingStandards.md`.
    * If your code or test changes impact the module's purpose, architecture, interface, assumptions, dependencies, testing strategy, or visualized structures: Update the `README.md` and relevant Mermaid diagram(s) accordingly. Be precise, prune obsolete info, ensure correct relative paths, and adhere to diagramming standards. Update `Last Updated:` date.

10. **Verify Formatting:**
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

11. **Commit Changes:**
    * Stage all relevant changes (code, tests, documentation, diagrams, formatting fixes).
    * Commit the changes to your feature branch using a Conventional Commit message referencing the Associated Task ID. Adhere to `/Docs/Standards/TaskManagementStandards.md` Section 5.
      ```bash
      git add . # Ensure formatting fixes are staged
      git commit -m "feat: Implement recipe search endpoint (#123)" # Example commit
      ```

12. **Create Pull Request:**
    * Push your feature branch to the origin repository.
      ```bash
      git push origin feature/issue-{ISSUE_ID}-{brief-description}
      ```
    * Create a Pull Request using the GitHub CLI (`gh`). Ensure the title and body adhere to `/Docs/Standards/TaskManagementStandards.md` Section 6, targeting the branch specified in the prompt.
      ```bash
      # Example: gh pr create --base develop --title "feat: Implement recipe search (#123)" --body "Closes #123. Adds the /api/recipes/search endpoint."
      gh pr create --base [TARGET_BRANCH_FROM_PROMPT] --title "<type>: <Brief description> (#ISSUE_ID)" --body "Closes #{ISSUE_ID}. [Summary of changes]"
      ```

13. **Provide Output:**
    * Report the final commit hash on your feature branch and the URL of the created Pull Request as specified in Section 6 of the prompt.

---