# Module/Directory: Docs/Development

**Last Updated:** 2025-09-01

> **Parent:** [`Docs`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory houses documentation defining the *workflows and processes* specifically governing the development of the Zarichney API application, with a focus on facilitating **12-agent orchestrated development**.
* **Key Responsibilities:**
    * Outlining the strategic codebase manager orchestration model with specialized agent coordination.
    * Documenting the architectural evolution from executor to orchestrator (`CodebaseManagerEvolution.md`).
    * Providing comprehensive documentation grounding protocols that ensure agents maintain contextual awareness.
    * Documenting the short-term technical roadmap and deferred items.
* **Why it exists:** To establish a clear and effective **multi-agent development process**, ensuring tasks are well-defined, agents operate with comprehensive context, and standards are consistently applied across the development team.
* **Core Documents within this Directory:**
    * **[`CodebaseManagerEvolution.md`](./CodebaseManagerEvolution.md): ARCHITECTURAL FOUNDATION** - Documents the evolution from executor to orchestrator model with 12-agent specialization.
    * [`CodingPlannerAssistant.md`](./CodingPlannerAssistant.md): Legacy workflow documentation (superseded by orchestration model).
    * [`StandardWorkflow.md`](./StandardWorkflow.md): Legacy workflow documentation (superseded by specialized agent protocols).
    * [`ComplexTaskWorkflow.md`](./ComplexTaskWorkflow.md): Legacy workflow documentation (superseded by specialized agent protocols).
    * [`TestCoverageWorkflow.md`](./TestCoverageWorkflow.md): Legacy workflow documentation (superseded by TestEngineer agent).
    * [`LoggingGuide.md`](./LoggingGuide.md): Comprehensive guide for the enhanced logging system, including configuration and best practices.
    * [`TestArtifactsGuide.md`](./TestArtifactsGuide.md): Guide for understanding and using CI/CD test artifacts including coverage reports and test results.
    * [`TestSuiteBaselineGuide.md`](./TestSuiteBaselineGuide.md): Practical interpretation guide for test suite baseline validation results, troubleshooting workflows, and actionable guidance for achieving progressive coverage targets.
    * [`TestSuiteEnvironmentSetup.md`](./TestSuiteEnvironmentSetup.md): Comprehensive environment setup requirements for all test classifications, external service configuration, and optimization strategies to minimize skip rates.
    * [`ShortTermRoadmap.md`](./ShortTermRoadmap.md): Captures planned enhancements and deferred items for the codebase and development workflow.
* **Related Templates (Located in /Docs/Templates/):**
    * [`../Templates/AICoderPromptTemplate.md`](../Templates/AICoderPromptTemplate.md): The mandatory template structure used by the Planning Assistant to generate prompts for AI Coders performing coding tasks.
    * [`../Templates/TestCaseDevelopmentTemplate.md`](../Templates/TestCaseDevelopmentTemplate.md): The mandatory template structure used by the Planning Assistant to generate prompts for AI Coders performing test coverage tasks.
    * [`../Templates/GHCoderTaskTemplate.md`](../Templates/GHCoderTaskTemplate.md): Template for GitHub Issues related to general coding tasks.
    * [`../Templates/GHTestCoverageTaskTemplate.md`](../Templates/GHTestCoverageTaskTemplate.md): Template for GitHub Issues related to test coverage tasks.
    * [`../Templates/ReadmeTemplate.md`](../Templates/ReadmeTemplate.md): The mandatory template for per-directory READMEs.
* **Core Standards (Located in /Docs/Standards/):**
    * (Links to CodingStandards.md, DocumentationStandards.md, DiagrammingStandards.md, TestingStandards.md, TaskManagementStandards.md)

## 2. 12-Agent Orchestrated Development Workflow Overview

The core workflow leverages a **strategic codebase manager** (Claude) as the team leader orchestrating 11 specialized AI agents in a structured, coordinated process. This evolution includes pre-PR validation through the Compliance Officer partnership and rich inter-agent communication via the `/working-dir/` system. Each agent employs comprehensive **documentation grounding protocols** to ensure contextual awareness and standards alignment.

* **Orchestration Workflow Diagram:**
    *(Diagram follows conventions defined in [`../Standards/DiagrammingStandards.md`](../Standards/DiagrammingStandards.md))*

```mermaid
---
config:
  layout: Dagre
---
flowchart TD
    subgraph INPUT ["GitHub Issue Assignment"]
       direction LR
       Issue["GitHub Issue\n(Requirements & Context)"] --> Claude["Claude\n(team leader & Orchestrator)"]
    end

    subgraph CLAUDE_ORCHESTRATION ["Strategic Codebase Management + Adaptive Coordination"]
       direction TB
       Step1["1. Mission Understanding\n(Analyze issue requirements)"]
       Step2["2. Context Ingestion + Working Dir Setup\n(Load documentation & initialize session)"] 
       Step3["3. Task Decomposition\n(Break into specialized subtasks)"]
       Step4["4. Adaptive Delegation\n(Assign to agents with context)"]
       Claude --> Step1 --> Step2 --> Step3 --> Step4
    end

    subgraph DOCUMENTATION ["Self-Contained Knowledge System"]
       direction TB
       subgraph Standards ["/Docs/Standards/"]
           STD_CS([CodingStandards.md])
           STD_TS([TestingStandards.md])
           STD_DS([DocumentationStandards.md])
           STD_DG([DiagrammingStandards.md])
           STD_TM([TaskManagementStandards.md])
       end
       subgraph AgentInstructions ["/.claude/agents/"]
           AGT_CO([compliance-officer.md])
           AGT_CC([code-changer.md])
           AGT_TE([test-engineer.md])
           AGT_SA([security-auditor.md])
           AGT_FS([frontend-specialist.md])
           AGT_BS([backend-specialist.md])
           AGT_WE([workflow-engineer.md])
           AGT_BI([bug-investigator.md])
           AGT_DM([documentation-maintainer.md])
           AGT_AA([architectural-analyst.md])
           AGT_PE([prompt-engineer.md])
       end
       LocalREADMEs["Module READMEs\n(Production Code Context)"]
    end

    subgraph AGENT_TEAM ["12-Agent Specialized Team"]
       direction TB
       CO["ComplianceOfficer\n(Pre-PR Validation)"]
       CC["CodeChanger\n(Implementation)"]
       TE["TestEngineer\n(Quality Assurance)"]
       SA["SecurityAuditor\n(Security Review)"]
       FS["FrontendSpecialist\n(Angular/TypeScript)"]
       BS["BackendSpecialist\n(.NET/C#)"]
       WE["WorkflowEngineer\n(CI/CD)"]
       BI["BugInvestigator\n(Root Cause Analysis)"]
       DM["DocumentationMaintainer\n(Standards Compliance)"]
       AA["ArchitecturalAnalyst\n(Design Decisions)"]
       PE["PromptEngineer\n(AI Enhancement)"]
    end

    subgraph WORKING_DIR ["/working-dir/ Communication Hub"]
       direction TB
       SessionState["session-state.md\n(Progress Tracking)"]
       Artifacts["Agent Artifacts\n(Analysis, Decisions, Notes)"]
       Handoffs["Inter-Agent Handoffs\n(Rich Context Transfer)"]
    end

    subgraph INTEGRATION ["Integration & Pre-PR Validation"]
       direction TB
       Step5["5. Integration Oversight\n(Validate agent outputs)"]
       Step6["6. Compliance Partnership\n(Dual validation with ComplianceOfficer)"]
       Step7A["7a. Pre-PR Gate\n(ComplianceOfficer assessment)"]
       Step7B{"Validation Decision"}
       Step8["8. Final Assembly\n(Commit, push, create PR)"]
       Step9["9. AI Sentinel Review\n(5 AI reviewers analyze PR)"]
       Step5 --> Step6 --> Step7A --> Step7B
       Step7B -->|Approved| Step8
       Step7B -->|Needs Work| Step4
       Step8 --> Step9
    end

    %% Strategic Manager Workflow
    Step2 -->|Loads & Initializes| DOCUMENTATION
    Step2 -->|Creates| WORKING_DIR
    Step4 -->|Delegates to| AGENT_TEAM

    %% Agent Documentation Grounding
    CO -->|Grounds in| AGT_CO
    CC -->|Grounds in| AGT_CC
    TE -->|Grounds in| AGT_TE
    SA -->|Grounds in| AGT_SA
    FS -->|Grounds in| AGT_FS
    BS -->|Grounds in| AGT_BS
    WE -->|Grounds in| AGT_WE
    BI -->|Grounds in| AGT_BI
    DM -->|Grounds in| AGT_DM
    AA -->|Grounds in| AGT_AA
    PE -->|Grounds in| AGT_PE

    %% Agent Context Loading & Communication
    AGENT_TEAM -->|Systematically Loads| Standards
    AGENT_TEAM -->|Reviews| LocalREADMEs
    AGENT_TEAM <==>|Creates & Consumes| WORKING_DIR

    %% ComplianceOfficer Integration
    Step6 -->|Partners with| CO
    CO -->|Validates using| WORKING_DIR

    %% Integration Flow
    AGENT_TEAM --> Step5
    Step9 --> FinalOutput["Pull Request\n(Ready for Merge)"]

    %% Adaptive Feedback Loop
    WORKING_DIR -.->|Critical Updates| Claude
    Claude -.->|Plan Adjustments| Step4

    classDef manager fill:#e6f2ff,stroke:#004080,stroke-width:2px;
    classDef compliance fill:#d4a5ff,stroke:#6b21a8,stroke-width:2px;
    classDef agent fill:#e6ffe6,stroke:#006400,stroke-width:1px;
    classDef document fill:#f5f5f5,stroke:#666,stroke-width:1px;
    classDef working fill:#ffeb99,stroke:#d69e2e,stroke-width:1px;
    classDef process fill:#fff,stroke:#333,stroke-width:1px;
    classDef output fill:#fff0b3,stroke:#cca300,stroke-width:1px;

    class Claude,CLAUDE_ORCHESTRATION manager;
    class CO,Step6,Step7A,Step7B compliance;
    class AGENT_TEAM,CC,TE,SA,FS,BS,WE,BI,DM,AA,PE agent;
    class DOCUMENTATION,Standards,AgentInstructions,LocalREADMEs,STD_CS,STD_TS,STD_DS,STD_DG,STD_TM,AGT_CO,AGT_CC,AGT_TE,AGT_SA,AGT_FS,AGT_BS,AGT_WE,AGT_BI,AGT_DM,AGT_AA,AGT_PE document;
    class WORKING_DIR,SessionState,Artifacts,Handoffs working;
    class Step1,Step2,Step3,Step4,Step5,Step8,Step9 process;
    class Issue,FinalOutput output;

```

## 3. How to Use This Directory

* **Understanding the Architecture:** Start with [`CodebaseManagerEvolution.md`](./CodebaseManagerEvolution.md) to understand the strategic orchestration model and 12-agent specialization (11 subagents + codebase manager).
* **Agent Coordination:** Review the specialized agent instruction files in [`/.claude/agents/`](../../.claude/agents/) to understand individual agent capabilities and documentation grounding protocols.
* **Legacy Workflows:** Historical workflow files (`CodingPlannerAssistant.md`, `StandardWorkflow.md`, etc.) are maintained for reference but have been superseded by the agent orchestration model.
* **Templates:** Refer to files in [`/Docs/Templates/`](../Templates/) for the structure of GitHub Issues and documentation templates.
* **Future Plans:** Consult [`ShortTermRoadmap.md`](./ShortTermRoadmap.md) for planned features and refactoring efforts.
* **Standards:** Always ensure development aligns with the rules defined in [`/Docs/Standards/`](../Standards/) - these are systematically loaded by all agents through documentation grounding protocols.

-----
