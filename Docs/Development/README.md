# Module/Directory: Docs/Development

**Last Updated:** 2025-05-04

> **Parent:** [`Docs`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory houses documentation defining the *workflows and processes* specifically governing the development of the Zarichney API application, with a focus on facilitating AI-assisted development.
* **Key Responsibilities:**
    * Outlining the high-level AI-assisted workflow involving planning and coding agents.
    * Providing links to the specific prompts, templates, and detailed workflow steps used in this process.
    * Documenting the short-term technical roadmap and deferred items.
* **Why it exists:** To establish a clear and effective AI-assisted development process, ensuring tasks are well-defined, context is appropriately provided to AI assistants, and standards are consistently referenced.
* **Core Documents within this Directory:**
    * [`CodingPlannerAssistant.md`](./CodingPlannerAssistant.md): Defines the workflow and prompt for the AI assistant responsible for planning and decomposing tasks.
    * [`StandardWorkflow.md`](./StandardWorkflow.md): Details the step-by-step workflow for the AI Coder performing standard development tasks.
    * [`ComplexTaskWorkflow.md`](./ComplexTaskWorkflow.md): Details the step-by-step workflow for the AI Coder performing complex or novel tasks using a TDD/Plan-First approach.
    * [`TestCoverageWorkflow.md`](./TestCoverageWorkflow.md): Details the step-by-step workflow for the AI Coder performing test coverage enhancement tasks.
    * [`ShortTermRoadmap.md`](./ShortTermRoadmap.md): Captures planned enhancements and deferred items for the codebase and development workflow.
* **Related Templates (Located in /Docs/Templates/):**
    * [`../Templates/AICoderPromptTemplate.md`](../Templates/AICoderPromptTemplate.md): The mandatory template structure used by the Planning Assistant to generate prompts for AI Coders performing coding tasks.
    * [`../Templates/TestCaseDevelopmentTemplate.md`](../Templates/TestCaseDevelopmentTemplate.md): The mandatory template structure used by the Planning Assistant to generate prompts for AI Coders performing test coverage tasks.
    * [`../Templates/GHCoderTaskTemplate.md`](../Templates/GHCoderTaskTemplate.md): Template for GitHub Issues related to general coding tasks.
    * [`../Templates/GHTestCoverageTaskTemplate.md`](../Templates/GHTestCoverageTaskTemplate.md): Template for GitHub Issues related to test coverage tasks.
    * [`../Templates/ReadmeTemplate.md`](../Templates/ReadmeTemplate.md): The mandatory template for per-directory READMEs.
* **Core Standards (Located in /Docs/Standards/):**
    * (Links to CodingStandards.md, DocumentationStandards.md, DiagrammingStandards.md, TestingStandards.md, TaskManagementStandards.md)

## 2. AI-Assisted Development Workflow Overview

The core workflow leverages AI assistants in a structured, iterative process, aiming for high quality and consistency through adherence to documented standards and context. The AI Planning Assistant acts as the central orchestrator, analyzing goals and codebase state, and then generating precise instructions for stateless AI Coders by referencing standardized templates and workflow definitions.

* **Workflow Diagram:**
    *(Diagram follows conventions defined in [`../Standards/DiagrammingStandards.md`](../Standards/DiagrammingStandards.md))*

```mermaid
---
config:
  layout: Dagre
---
flowchart TD
    subgraph INPUTS ["Inputs & Context"]
       direction LR
       DevGoal["Developer Goal\n(via GitHub Issue)"] -- Provides --> PLANNER["AI Planning Assistant\n(Gemini)"]
       CodebaseState["Codebase State\n(Files, Git Status)"] -- Informs --> PLANNER
       PlannerPrompt["CodingPlannerAssistant.md"] -- Guides --> PLANNER
    end

    subgraph PLANNER_PROCESS ["Planner Task Execution"]
       direction TB
       Step1["1. Analyze Goal & Codebase"]
       Step2["2. Strategize Breakdown\n(Epic/Single Issue, Workflow Type, Session Context)"]
       Step3["3. Generate Coder Prompt"]
       PLANNER --> Step1 --> Step2 --> Step3
    end

    subgraph DOCUMENTATION ["Documentation Artifacts"]
       direction TB
       subgraph Standards ["/Docs/Standards"]
           STD_CS([CodingStandards.md])
           STD_TS([TestingStandards.md])
           STD_DS([DocumentationStandards.md])
           STD_DG([DiagrammingStandards.md])
           STD_TM([TaskManagementStandards.md])
       end
       subgraph Workflows ["/Docs/Development"]
           WF_STD([StandardWorkflow.md])
           WF_CMP([ComplexTaskWorkflow.md])
           WF_TC([TestCoverageWorkflow.md])
       end
       subgraph Templates ["/Docs/Templates"]
           TMP_CD([AICoderPromptTemplate.md])
           TMP_TC([TestCaseDevelopmentTemplate.md])
           TMP_GH([GHCoderTaskTemplate.md])
           TMP_GHTC([GHTestCoverageTaskTemplate.md])
           TMP_RM([ReadmeTemplate.md])
       end
       LocalREADMEs["Module READMEs"]
    end

    subgraph CODER_PROCESS ["Coder Task Execution"]
       direction TB
       GeneratedPrompt["Generated AI Coder Prompt"]
       CODER["Stateless AI Coder Agent"]
       Step4["4. Read Prompt & Referenced Docs"]
       Step5["5. Execute Referenced Workflow Steps\n(Code, Test, Format, Git, PR, Update Docs)"]
       FinalOutput["Final Output\n(Commit Hash, PR URL)"]
       GeneratedPrompt --> CODER --> Step4 --> Step5 --> FinalOutput
    end

    %% Planner Inputs & References
    Step1 -->|Reads| LocalREADMEs
    Step2 -->|Consults| Standards
    Step3 -->|Uses| Templates
    Step3 -->|References| Workflows

    %% Prompt Content
    Step3 -->|Creates| GeneratedPrompt

    %% Coder Inputs
    GeneratedPrompt -->|References| DOCUMENTATION
    Step4 -->|Reads| DOCUMENTATION

    %% Coder Output affects Codebase
    Step5 -->|Modifies| CodebaseState

    classDef aiPlanner fill:#e6f2ff,stroke:#004080,stroke-width:1px;
    classDef aiCoder fill:#e6ffe6,stroke:#006400,stroke-width:1px;
    classDef document fill:#f5f5f5,stroke:#666,stroke-width:1px,shape:note;
    classDef codebase fill:#f5deb3,stroke:#8b4513,stroke-width:1px,shape:cylinder;
    classDef input fill:#fff0b3,stroke:#cca300,stroke-width:1px,shape:parallelogram;
    classDef process fill:#fff,stroke:#333,stroke-width:1px;

    class PLANNER aiPlanner;
    class CODER aiCoder;
    class WF_STD,WF_CMP,WF_TC,TMP_CD,TMP_TC,TMP_GH,TMP_GHTC,TMP_RM,LocalREADMEs,PlannerPrompt,GeneratedPrompt document;
    class STD_CS,STD_TS,STD_DS,STD_DG,STD_TM document;
    class CodebaseState codebase;
    class DevGoal input;
    class Step1,Step2,Step3,Step4,Step5 process;
    class FinalOutput input;

```

## 3\. How to Use This Directory

  * **AI Planning:** Use [`CodingPlannerAssistant.md`](https://www.google.com/search?q=./CodingPlannerAssistant.md) as the starting prompt when engaging the AI Planning Assistant.
  * **AI Coding Workflows:** Review the detailed steps in [`StandardWorkflow.md`](https://www.google.com/search?q=./StandardWorkflow.md), [`ComplexTaskWorkflow.md`](https://www.google.com/search?q=./ComplexTaskWorkflow.md), and [`TestCoverageWorkflow.md`](https://www.google.com/search?q=./TestCoverageWorkflow.md) to understand the execution process for AI Coders.
  * **Templates:** Refer to files in [`/Docs/Templates/`](https://www.google.com/search?q=../Templates/) for the structure of AI Coder prompts and GitHub Issues.
  * **Future Plans:** Consult [`ShortTermRoadmap.md`](https://www.google.com/search?q=./ShortTermRoadmap.md) for planned features and refactoring efforts impacting the development process or codebase architecture.
  * **Standards:** Always ensure development aligns with the rules defined in [`/Docs/Standards/`](https://www.google.com/search?q=../Standards/).

-----
