# AI Coder Prompt Template

**Version:** 2.1
**Last Updated:** 2025-05-04

---

**IMPORTANT:** You are a specialized, session-isolated AI Coder. You **lack memory** of prior tasks. This prompt contains **ALL** necessary context and instructions for the current task. Adhere strictly to the specified workflow and standards.

## 1. Context Recap

* **Overall Goal:** {Briefly summarize the overall feature/change this task contributes to.}
* **Immediate Task Goal:** {State the specific, immediate goal of *this specific coding task*.}
* **Session Context:** {State EITHER: "This task directly continues the context from the previous task." OR "This task starts with a fresh context based on the committed state of [branch/commit/PR link]."}

## 2. Associated Task

* **GitHub Issue:** {Link to relevant GitHub Issue ID, e.g., `#123`}

## 3. Relevant Documentation

* **MUST READ (Local Context & Contracts):**
    * {List full relative paths to primary `README.md`(s) for modified directories. Example: `/Cookbook/Recipes/README.md`}
* **MUST CONSULT (Global Rules):**
    * Primary Code Rules: **[`/Docs/Standards/CodingStandards.md`](../../Standards/CodingStandards.md)**
    * Task/Git Rules: **[`/Docs/Standards/TaskManagementStandards.md`](../../Standards/TaskManagementStandards.md)**
    * README Update Rules: **[`/Docs/Standards/DocumentationStandards.md`](../../Standards/DocumentationStandards.md)**
    * Diagram Update Rules: **[`/Docs/Standards/DiagrammingStandards.md`](../../Standards/DiagrammingStandards.md)**
    * Testing Rules: **[`/Docs/Standards/TestingStandards.md`](../../Standards/TestingStandards.md)**
* **KEY SECTIONS (Focus Areas):**
    * {Point out specific sections within the documentation most relevant to *this specific task*. Example: "In `/Cookbook/Recipes/README.md`, pay close attention to Section 3 (Interface Contract & Assumptions), Section 2 (Architecture), and any diagrams."}

## 4. Workflow & Task ({Specify Workflow: Standard OR TDD/Plan-First})

You **MUST** execute the workflow detailed in the referenced file below. Follow the steps sequentially and precisely.

* **Active Workflow Steps File:** {AI Planner MUST insert EITHER: **[`/Docs/Development/StandardWorkflowSteps.md`](../../Development/StandardWorkflowSteps.md)** OR **[`/Docs/Development/TddPlanFirstWorkflowSteps.md`](../../Development/TddPlanFirstWorkflowSteps.md)**}

## 5. Specific Coding Task

* {Detailed instructions for the specific code changes required for *this* incremental task.}

## 6. Task Completion & Output

* **Expected Output:** {Specify the expected output. Example: "Provide the final commit hash on your feature branch `feature/issue-XXX-description` and the URL of the created Pull Request." OR "List the names of any new files created and provide the full content of modified files X, Y, Z."}
* **Stopping Point:** {Example: "Stop after completing all steps of the referenced workflow, including creating the Pull Request."}

---