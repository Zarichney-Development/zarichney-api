# AI Coder Workflow Steps: Test Coverage Enhancement

**Version:** 1.0
**Last Updated:** 2025-05-04

## Overview

This document details the mandatory step-by-step workflow for an AI Coder agent whose primary task is to **increase automated test coverage** for existing code, without intentionally altering the production code's logic. These steps are referenced by the Test Case Development Prompt Template (`/Docs/Templates/TestCaseDevelopmentTemplate.md`) when assigned a test coverage task.

**AI Coder Agent:** You MUST follow these steps sequentially and precisely. Your goal is to add meaningful tests that improve coverage metrics and validate existing behavior.

## Test Coverage Enhancement Workflow Steps

1.  **Identify & Acknowledge Task:**
    * Locate the task details using the 'Associated Task' link/ID provided in Section 2 of the prompt (which should use the `ai_test_coverage_task.md` template).
    * Read the issue description on GitHub, noting the target module(s) and any specific areas highlighted for coverage improvement.
    * Briefly acknowledge your understanding (e.g., "Acknowledged: Increase test coverage for RecipeService methods, issue #789.").

2.  **Review Assignment:**
    * Thoroughly understand the Context Recap (Section 1 of the prompt) and the Specific Testing Task details (Section 5 of the prompt).

3.  **Review Standards & Context:**
    * Carefully read all documentation listed in Section 3 of the prompt, paying close attention to Key Sections. **Crucially, internalize the `/Docs/Standards/TestingStandards.md`**.
    * Understand the existing code's behavior within the target module(s) by reviewing the code itself and its `README.md`.
    * Review existing tests in the corresponding `/api-server.Tests/` directory to understand current patterns and coverage.

4.  **Analyze Code & Identify Coverage Gaps:**
    * Examine the target production code (`.cs` files in `/api-server/`) identified in the prompt or GitHub Issue.
    * Identify specific methods, logic branches (if/else, switch cases), error handling paths, or edge cases that lack sufficient unit or integration test coverage based on the principles in `/Docs/Standards/TestingStandards.md`.
    * *(Optional: If tooling is available and instructed)* Use code coverage analysis tools locally (e.g., generate a coverage report) to pinpoint uncovered lines/branches.

5.  **Create Feature Branch:**
    * Ensure your local repository clone is up-to-date with the specified base branch (e.g., `develop`, `main`).
        ```bash
        git checkout [BASE_BRANCH_FROM_PROMPT]
        git pull origin [BASE_BRANCH_FROM_PROMPT]
        ```
    * Create and checkout a new feature branch using the naming convention specified in `/Docs/Standards/TaskManagementStandards.md` Section 2, indicating a testing focus (e.g., `test/issue-789-recipeservice-coverage`).
        ```bash
        git checkout -b test/issue-{ISSUE_ID}-{brief-description} # Note 'test/' prefix
        ```

6.  **Write New Tests:**
    * Add new unit or integration tests specifically targeting the coverage gaps identified in step 4.
    * Focus on testing the *existing behavior* of the production code. **Do NOT modify the production code itself** unless absolutely necessary to enable testing (e.g., adding an interface for mocking) and explicitly approved or instructed.
    * Tests **MUST** adhere strictly to `/Docs/Standards/TestingStandards.md` (AAA pattern, FluentAssertions, Moq, Traits, Naming, etc.).
    * Write clear, maintainable tests verifying different scenarios and edge cases.

7.  **Verify New Tests Pass:**
    * Run the specific tests you added.
    * Ensure they **PASS** consistently. Debug test setup or logic if they fail, ensuring they accurately reflect and validate the existing production code's behavior.

8.  **Verify All Tests:**
    * Run the **entire test suite** using the command: `dotnet test`.
    * Ensure **ALL** tests pass. Fix any issues introduced by your new tests (e.g., unintended interactions, fixture state problems).

9.  **Verify Formatting:**
    * Run the .NET formatting tool to ensure code style compliance with `.editorconfig`.
    * Check for violations: `dotnet format --verify-no-changes --verbosity diagnostic`
    * If violations exist, fix them: `dotnet format` and stage the changes (`git add .`).

10. **Update Documentation (If Applicable):**
    * If the new tests reveal important insights about the module's behavior or assumptions that aren't documented, or if the testing strategy section of the relevant `README.md` needs updating, make those changes according to `/Docs/Standards/DocumentationStandards.md`. (Changes to production code documentation are less likely in this workflow but possible).
    * Update `Last Updated:` date if `README.md` is changed.

11. **Commit Changes:**
    * Stage all relevant changes (new/updated test files, documentation updates).
    * Commit the changes to your feature branch using a Conventional Commit message (type `test:`) referencing the Associated Task ID. Adhere to `/Docs/Standards/TaskManagementStandards.md` Section 3.
        ```bash
        git add .
        git commit -m "test: Increase unit test coverage for RecipeService (#789)" # Example commit
        ```

12. **Create Pull Request:**
    * Push your feature branch to the origin repository.
        ```bash
        git push origin test/issue-{ISSUE_ID}-{brief-description}
        ```
    * Create a Pull Request using the GitHub CLI (`gh`). Ensure the title (type `test:`) and body adhere to `/Docs/Standards/TaskManagementStandards.md` Section 4, targeting the branch specified in the prompt.
        ```bash
        gh pr create --base [TARGET_BRANCH_FROM_PROMPT] --title "test: Increase RecipeService coverage (#789)" --body "Closes #789. Adds unit tests for X, Y, Z methods."
        ```

13. **Provide Output:**
    * Report the final commit hash on your feature branch and the URL of the created Pull Request as specified in Section 6 of the prompt. Optionally, list the names of the new test files created.

---