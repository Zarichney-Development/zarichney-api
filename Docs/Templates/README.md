# Module/Directory: Docs/Templates

**Last Updated:** 2025-05-04

> **Parent:** [`Docs`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory contains standardized templates used throughout the AI-assisted development workflow and for general documentation consistency.
* **Goal:** To ensure uniformity and provide pre-defined structures for artifacts like AI Coder prompts, GitHub Issues, and per-directory README files. Using these templates helps ensure all necessary information is captured in a consistent format, which is crucial for both human understanding and AI agent processing.

## 2. Overview of Template Files

This directory contains the following key templates:

* **AI Coder Prompt Templates:** Used by specialized agents in AI-orchestrated workflows per CLAUDE.md.
    * **[`AICoderPromptTemplate.md`](./AICoderPromptTemplate.md):** The standard template for generating prompts related to general coding tasks (features, fixes, refactors). Used by CodeChanger and specialist agents.
    * **[`TestCaseDevelopmentTemplate.md`](./TestCaseDevelopmentTemplate.md):** The template for generating prompts related specifically to enhancing automated test coverage. Used by TestEngineer agent.

* **GitHub Issue Templates:** Used by the Human Developer (or potentially the AI Planning Assistant in the future) to create well-defined tasks in GitHub. These files are intended to be copied/used when creating new issues manually for now, but could eventually populate the `.github/ISSUE_TEMPLATE/` directory.
    * **[`GHCoderTaskTemplate.md`](./GHCoderTaskTemplate.md):** The standard template for creating GitHub Issues related to general coding tasks. Aligns with `AICoderPromptTemplate.md`.
    * **[`GHTestCoverageTaskTemplate.md`](./GHTestCoverageTaskTemplate.md):** The standard template for creating GitHub Issues related specifically to enhancing test coverage. Aligns with `TestCaseDevelopmentTemplate.md`.

* **Documentation Structure Templates:**
    * **[`ReadmeTemplate.md`](./ReadmeTemplate.md):** Defines the mandatory section structure for all per-directory `README.md` files throughout the codebase, as required by `DocumentationStandards.md`.

## 3. How to Use These Templates

* **AI Coder Prompts:** These templates are used by specialized agents in the orchestration model per CLAUDE.md.
* **GitHub Issues:** Manually copy the content from the relevant `.md` file here when creating a new GitHub Issue for an AI task until direct GitHub integration is implemented.
* **README Files:** Use `ReadmeTemplate.md` as the starting point for creating any new per-directory README file.

---