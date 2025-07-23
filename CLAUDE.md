# Project Context & Operating Guide for AI Coding Assistant (Claude)

**Version:** 1.2
**Last Updated:** 2025-07-23

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
    * **Enhanced GitHub Operations** (See Section 7 for AI-powered alternatives):
        * Issue analysis: `claude --dangerously-skip-permissions --print "Use GitHub MCP to analyze issue #ID"`
        * PR enhancement: `claude --dangerously-skip-permissions --print "Use GitHub MCP to review and enhance my PR"`
        * Repository health: `claude --dangerously-skip-permissions --print "Use GitHub MCP to check zarichney-api status"`
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

## 7. GitHub Integration & Automation

### GitHub Repository Context
This project operates within the **Zarichney-Development** organization:
- **Repository**: `Zarichney-Development/zarichney-api`
- **Status**: Public repository
- **Current Issues**: 5 open issues requiring attention
- **Recent Focus**: Monorepo consolidation and CI/CD unification

### GitHub Connection Methods (Reference System Documentation)
For comprehensive GitHub setup information, reference the system-wide documentation:
- **System GitHub Setup**: `@/home/zarichney/CLAUDE.md` (GitHub Integration section)
- **Connection Hierarchy**: MCP → GitHub CLI → Direct API
- **Authentication Status**: All methods configured and working

### Project-Specific GitHub Operations

#### **Enhanced Pull Request Workflow**
Beyond the basic `gh pr create` command, leverage AI-powered GitHub operations:

```bash
# Standard PR creation (existing workflow step 8)
gh pr create --base main --title "feat: implement feature (#ISSUE_ID)" --body "Closes #ISSUE_ID. [Summary]"

# AI-enhanced PR creation with comprehensive analysis
claude --dangerously-skip-permissions --print "Use GitHub MCP to analyze my recent commits and create a detailed PR description for the zarichney-api repository"

# Automated PR review preparation
claude --dangerously-skip-permissions --print "Use GitHub MCP to review open PRs in zarichney-api and suggest improvements"
```

#### **Issue-Driven Development Integration**
Since workflow step 1 references "related github issue", enhance issue management:

```bash
# Standard issue analysis
gh issue view ISSUE_ID

# AI-powered issue analysis and task planning
claude --dangerously-skip-permissions --print "Use GitHub MCP to analyze issue #ISSUE_ID in zarichney-api and create an implementation plan"

# Cross-issue pattern analysis
claude --dangerously-skip-permissions --print "Use GitHub MCP to analyze all open issues in zarichney-api and identify common themes"
```

#### **Repository Health Monitoring**
Integrate GitHub monitoring into development workflow:

```bash
# Project status before starting work
claude --dangerously-skip-permissions --print "Use GitHub MCP to provide a status summary of zarichney-api including recent activity and open issues"

# Pre-development environment check
claude --dangerously-skip-permissions --print "Use GitHub MCP to check if there are any security alerts or critical updates needed for zarichney-api"
```

### Claude Self-Delegation for Project Tasks

#### **Development Workflow Enhancement**
Integrate GitHub AI operations into the standard workflow steps:

1. **Enhanced Task Understanding** (Step 1 expansion):
   ```bash
   # After reviewing task prompt and issue
   claude --dangerously-skip-permissions --print "Use GitHub MCP to provide context for issue #ISSUE_ID including related issues and recent commits in zarichney-api"
   ```

2. **Intelligent Branch Strategy** (Step 3 enhancement):
   ```bash
   # Before creating feature branch
   claude --dangerously-skip-permissions --print "Use GitHub MCP to suggest an appropriate branch naming strategy based on the current issue and repository conventions"
   ```

3. **Pre-PR Validation** (Between steps 7-8):
   ```bash
   # Before creating PR
   claude --dangerously-skip-permissions --print "Use GitHub MCP to review my changes and suggest an appropriate PR title and description for zarichney-api"
   ```

#### **Project-Specific Automation Patterns**
```bash
# Monorepo-specific analysis
claude --dangerously-skip-permissions --print "Use GitHub MCP to analyze the impact of my changes across both Code/Zarichney.Server and Code/Zarichney.Website"

# CI/CD pipeline awareness
claude --dangerously-skip-permissions --print "Use GitHub MCP to check the status of GitHub Actions workflows for zarichney-api and identify any failures"

# Documentation impact assessment
claude --dangerously-skip-permissions --print "Use GitHub MCP to identify which documentation files might need updates based on my code changes"
```

### Integration with Existing Standards
- **TaskManagementStandards.md**: GitHub operations complement conventional commit and branching strategies
- **TestingStandards.md**: Use GitHub MCP to analyze test coverage and suggest additional test scenarios
- **DocumentationStandards.md**: Leverage GitHub MCP to ensure documentation stays current with code changes

---
# important-instruction-reminders
Do what has been asked; nothing more, nothing less.
NEVER create files unless they're absolutely necessary for achieving your goal.
ALWAYS prefer editing an existing file to creating a new one.
NEVER proactively create documentation files (*.md) or README files. Only create documentation files if explicitly requested by the User.