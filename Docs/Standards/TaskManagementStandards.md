# Task Management & Git Standards

**Version:** 1.1
**Last Updated:** 2025-05-04

## 1. Purpose and Scope

This document defines the mandatory standards for associating development tasks with GitHub Issues and managing Git workflow (branching, committing, pull requests) within the AI Coder workflow. Adherence ensures traceability, consistency, and facilitates automated processes.

These standards **MUST** be consulted and followed by AI Coders during task execution, as instructed by the AI Coder Prompt Template.

## 2. GitHub Issue Usage for AI Tasks (Mandatory)

* **Purpose:** GitHub Issues serve as the **canonical source of truth** for defining *what* needs to be done, *why* it's needed, and the **acceptance criteria** for verifying completion. They are the primary tool for task tracking and project management.
* **Task Definition:** All tasks intended for delegation to an AI Coder **MUST** have a corresponding GitHub Issue created.
* **Templates:** Use the specific GitHub Issue templates located in `/Docs/Templates/` to ensure necessary information is captured:
    * [`/Docs/Templates/GHCoderTaskTemplate.md`](../Templates/GHCoderTaskTemplate.md): For general coding tasks (features, fixes, refactors).
    * [`/Docs/Templates/GHTestCoverageTaskTemplate.md`](../Templates/GHTestCoverageTaskTemplate.md): For tasks focused solely on increasing test coverage.
* **Information Division:**
    * **GitHub Issue:** Contains the **Goal/Why**, **Requirements**, **Specific Objective**, and **Acceptance Criteria**. May contain high-level implementation notes.
    * **AI Coder Prompt:** Contains the **Execution Details (How/Where)**, links to **Technical Context** (READMEs, Standards, Workflow Steps), and a **link back to this GitHub Issue**. The prompt summarizes the objective but does *not* duplicate detailed background or acceptance criteria from the Issue.
* **AI Coder Interaction:**
    * The AI Coder **MUST** reference the linked GitHub Issue (provided in the prompt) to fully understand the task goal and acceptance criteria (Step 1 of all workflows).
    * The AI Coder **MUST** reference the Issue ID in commit messages and Pull Requests (see sections below).
    * The AI Coder **DOES NOT** update the Issue status directly (this is currently handled manually by the orchestrator).

## 3. Task Association (via Prompt)

* **Requirement:** Every AI Coder Prompt **MUST** include a direct link to the associated GitHub Issue ID in the "Associated Task" section. This links the execution context (prompt) to the task definition (issue).

## 4. Branching Strategy (Mandatory)

* **Requirement:** AI Coders **MUST** create a feature or test branch for each assigned task, unless explicitly instructed otherwise.
* **Base Branch:** Branches **MUST** be created from the base branch specified in the AI Coder prompt (typically `develop` or `main`).
* **Naming Convention:** Branch names **MUST** follow the format based on task type:
    * Coding Tasks: `feature/issue-{ISSUE_ID}-{brief-description}`
    * Testing Tasks: `test/issue-{ISSUE_ID}-{brief-description}`
    * `{ISSUE_ID}`: The numeric ID of the associated GitHub Issue (e.g., `123`).
    * `{brief-description}`: A short (2-5 words), lowercase, hyphen-separated description (e.g., `add-recipe-service`, `recipeservice-coverage`).
    * **Example (Coding):** `feature/issue-123-add-recipe-service`
    * **Example (Testing):** `test/issue-789-recipeservice-coverage`
* **Implementation:** Use standard Git commands within the relevant AI Coder Workflow steps file (e.g., `StandardWorkflow.md`):
    ```bash
    git checkout [BASE_BRANCH_FROM_PROMPT]
    git pull origin [BASE_BRANCH_FROM_PROMPT] # Ensure base is up-to-date
    git checkout -b [branch-name] # e.g., feature/issue-123-add-recipe-service
    ```

## 5. Commit Message Standard (Mandatory)

* **Requirement:** Commit messages **MUST** follow the Conventional Commits specification (v1.0.0).
* **Format:**
    ```
    <type>[optional scope]: <description>

    [optional body]

    [optional footer(s)]
    ```
    * **Type:** Must be one appropriate for the change (`feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`). Use `test:` for commits related to test coverage tasks.
    * **Description:** Concise summary of the change in present tense.
    * **Body (Optional):** Provides additional context or reasoning.
    * **Footer (Mandatory Issue Link):** **MUST** include a reference to the associated GitHub Issue ID. Use keywords like `Closes #XXX`, `Fixes #XXX`, or `Refs #XXX`.
        * **Example Footer:** `Refs #123`
* **Implementation:** Use standard Git commands within the relevant AI Coder Workflow steps file:
    ```bash
    git add .
    git commit -m "feat: Implement recipe search functionality (#123)" # Example
    git commit -m "test: Increase unit test coverage for RecipeService (#789)" # Example
    ```
* **Granularity:** Commit related changes (code, tests, docs) together logically. Avoid overly large commits.

## 6. Pull Request Standard (Mandatory)

* **Requirement:** Upon completing all steps for the task, the AI Coder **MUST** create a Pull Request (PR) using the GitHub CLI (`gh`).
* **Target Branch:** The PR **MUST** target the branch specified in the AI Coder prompt (typically `develop` or `main`).
* **Title:** PR titles **MUST** follow the Conventional Commit format, including the Issue ID.
    * **Format:** `<type>: <Brief description> (#ISSUE_ID)`
    * **Example (Coding):** `feat: Implement recipe search (#123)`
    * **Example (Testing):** `test: Increase RecipeService coverage (#789)`
* **Body:** PR bodies **MUST** include:
    * A link that closes/references the associated GitHub Issue (e.g., `Closes #123.`).
    * A brief summary of the changes made.
* **Implementation:** Use the GitHub CLI (`gh`) within the relevant AI Coder Workflow steps file. Ensure `gh` is authenticated (environment prerequisite).
    ```bash
    # Push the feature branch first
    git push origin [branch-name] # e.g., feature/issue-123-add-recipe-service

    # Create the Pull Request
    # Example: gh pr create --base develop --title "feat: Implement recipe search (#123)" --body "Closes #123. Implements backend search functionality."
    gh pr create --base [TARGET_BRANCH_FROM_PROMPT] --title "<type>: <Brief description> (#ISSUE_ID)" --body "Closes #{ISSUE_ID}. [Summary of changes]"
    ```
* **Output:** The URL of the created Pull Request is required as part of the task output specified in the AI Coder Prompt.


---