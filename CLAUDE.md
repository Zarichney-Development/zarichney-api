# Project Context & Operating Guide for AI Coding Assistant (Claude)

**Version:** 1.1
**Last Updated:** 2025-05-25

## 1. My Purpose & Your Role

* **My Purpose:** I am a set of standing instructions and key references for an AI Coding Assistant (like you, Claude) working on the `zarichney-api` repository. My goal is to help you understand project conventions, key commands, and essential documentation quickly.
* **Your Role:** When working on tasks in this repository, always consider the information here alongside any specific task prompt you receive. If the task prompt conflicts with a general standard mentioned here, the task prompt takes precedence for that specific instruction, but be mindful of project-wide standards.

## 2. Core Project Structure

* **`/Code/Zarichney.Server/`**: Main ASP.NET 8 application code. ([View README](../Code/Zarichney.Server/README.md))
* **`/Code/Zarichney.Server.Tests/`**: Unit and integration tests. ([View README](../Code/Zarichney.Server.Tests/README.md))
* **`/Code/Zarichney.Website/`**: Angular frontend application. ([View README](../Code/Zarichney.Website/README.md))
* **`/Docs/`**: All project documentation.
    * **`/Docs/Standards/`**: **CRITICAL STANDARDS** - Review these first. ([View README](../Docs/Standards/README.md))
    * **`/Docs/Development/`**: AI-assisted workflow definitions. ([View README](../Docs/Development/README.md))
    * **`/Docs/Templates/`**: Templates for prompts, issues, etc. ([View README](../Docs/Templates/README.md))
* **Module-Specific `README.md` files:** Each significant directory within `/Code/Zarichney.Server/` and `/Code/Zarichney.Server.Tests/` has its own `README.md`. **Always review the local `README.md` for the specific module you are working on.**

## 3. High-Level Development Workflow (When I give you a task)

Generally, your work will follow these phases. Refer to `/Docs/Standards/TaskManagementStandards.md` and the specific workflow file (e.g., `StandardWorkflow.md`) referenced in your task prompt for full details.

1.  **Understand Task:** Review the task prompt thoroughly, the related github issue and the project tree structure.
2.  **Review Context:** Use read tool on all standards and relevant local `README.md` files.
3.  **Branch:** Ensure you are on the correct branch, switch if needed or create a feature/test branch (`feature/issue-XXX-desc` or `test/issue-XXX-desc`), no committing on main.
4.  **Code/Test:** Implement changes and add/update tests `dotnet test`.
5.  **Format:** Verify and apply formatting (`dotnet format`).
6.  **Document:** Update relevant `README.md` files and diagrams if architecture/behavior changed.
7.  **Commit:** Use Conventional Commits referencing the Issue ID.
8.  **Pull Request:** Utilize the open PR or create a new PR using `gh pr create`.

## 4. Essential Commands & Tools

* **Build Project:**
    ```bash
    dotnet build Zarichney.sln
    ```
* **Run Project:**
    ```bash
    dotnet run --project Code/Zarichney.Server
    ```
* **Run All Tests:** (Ensure Docker Desktop is running for integration tests)
    ```bash
    # Standard execution (if Docker group membership is active)
    dotnet test Zarichney.sln
    
    # For environments where Docker group membership isn't active in current shell
    sg docker -c "dotnet test Zarichney.sln"
    ```
* **Run Specific Test Categories:**
    ```bash
    # Standard execution (if Docker group membership is active)
    dotnet test --filter "Category=Unit"
    dotnet test --filter "Category=Integration"
    
    # For environments where Docker group membership isn't active in current shell
    sg docker -c "dotnet test --filter 'Category=Unit'"
    sg docker -c "dotnet test --filter 'Category=Integration'"
    ```
* **Code Formatting:**
    * Check: `dotnet format --verify-no-changes --verbosity diagnostic`
    * Apply: `dotnet format`
* **Git Operations (Summary - See `TaskManagementStandards.md` for full details):**
    * Create branch: `git checkout -b [branch-name]` (e.g., `feature/issue-123-my-feature`)
    * Commit: `git commit -m "<type>: <description> (#ISSUE_ID)"`
    * Create PR: `gh pr create --base [target-branch] --title "<type>: <description> (#ISSUE_ID)" --body "Closes #ISSUE_ID. [Summary]"`
* **Regenerate API Client (for `/Code/Zarichney.Server.Tests/`):** If API contracts change.
    ```powershell
    # PowerShell
    ./Scripts/GenerateApiClient.ps1
    
    # Bash
    ./Scripts/generate-api-client.sh
    ```

## 5. MUST ALWAYS CONSULT: Key Standards Documents

Before implementing any significant code, test, or documentation changes, you **MUST** be familiar with and adhere to the following standards. The task prompt will list specific documents, but these are foundational:

* **Primary Code Rules:** [`/Docs/Standards/CodingStandards.md`](../Docs/Standards/CodingStandards.md) (Includes `/.editorconfig` reference)
* **Task/Git Rules:** [`/Docs/Standards/TaskManagementStandards.md`](../Docs/Standards/TaskManagementStandards.md)
* **Testing Rules:** [`/Docs/Standards/TestingStandards.md`](../Docs/Standards/TestingStandards.md)
* **Documentation Rules (READMEs):** [`/Docs/Standards/DocumentationStandards.md`](../Docs/Standards/DocumentationStandards.md) (Uses [`/Docs/Templates/ReadmeTemplate.md`](../Docs/Templates/ReadmeTemplate.md))
* **Localized README.md:** Each module has its own `README.md` file. Always check the local `README.md` for specific instructions or context.

## 6. Important Reminders

* **Focus on the Given Task:** Address the specific objectives outlined in your current prompt and linked GitHub Issue.
* **Statelessness:** Assume you have no memory of prior interactions unless explicitly provided in the current prompt.
* **Clarity & Explicitness:** If instructions are unclear, state your interpretation or ask for clarification (if interacting with a human).
* **Adhere to Boundaries:** Respect any "what *not* to change" instructions in the prompt, you're at liberty if not specified.
* **Update Documentation:** Changes to code or tests that impact documented purpose, architecture, contracts, or diagrams **MUST** be accompanied by updates to the relevant `README.md` and diagrams.

---