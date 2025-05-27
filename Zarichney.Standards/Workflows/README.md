# Module/Directory: Zarichney.Standards/Workflows

**Version:** 2.0
**Last Updated:** 2025-05-26

> **Parent:** [`Zarichney.Standards`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory defines the comprehensive AI-assisted development workflows and processes used throughout the Zarichney API project development lifecycle.
* **Key Responsibilities:**
    * Defining the AI Planning Assistant workflow for task decomposition and prompt generation.
    * Establishing step-by-step workflows for different types of development tasks.
    * Providing the technical roadmap and task management standards.
    * Ensuring consistent, high-quality AI-assisted development processes.
* **Why it exists:** To enable structured, efficient AI-assisted development by providing clear workflows, standards, and processes that guide both AI Planning Assistants and AI Coding Assistants through complex development tasks.
* **Core Documents within this Directory:**
    * [`CodingPlannerAssistant.md`](./CodingPlannerAssistant.md): The comprehensive workflow definition for the AI Planning Assistant responsible for analyzing goals and generating AI Coder prompts.
    * [`StandardWorkflow.md`](./StandardWorkflow.md): Step-by-step workflow for standard coding tasks (features, fixes, refactors).
    * [`ComplexTaskWorkflow.md`](./ComplexTaskWorkflow.md): Workflow for complex or novel tasks using a TDD/Plan-First approach.
    * [`TestCoverageWorkflow.md`](./TestCoverageWorkflow.md): Specialized workflow for enhancing automated test coverage.
    * [`TaskManagementStandards.md`](./TaskManagementStandards.md): Standards for Git workflows, task management, and branching strategies.
    * [`ShortTermRoadmap.md`](./ShortTermRoadmap.md): Technical roadmap capturing planned enhancements and deferred items.

## 2. Architecture & Key Concepts

* **AI-Assisted Development Pipeline:** Structured process involving AI Planning Assistants that analyze goals and generate context-rich prompts for stateless AI Coding Assistants.
* **Workflow Specialization:** Different workflows tailored to specific types of development tasks (standard, complex, test coverage).
* **Context Management:** Sophisticated approach to session context management (Chain vs Reset) for multi-step tasks.
* **Standards Integration:** All workflows explicitly reference and enforce adherence to project standards.

## 3. Interface Contract & Assumptions

* **Planning Assistant Entry Point:** All AI-assisted development begins with the CodingPlannerAssistant workflow.
* **GitHub Integration:** Workflows assume GitHub Issues exist for all development tasks and integrate with GitHub CLI for PR management.
* **Template Dependencies:** Workflows reference templates in [`../Templates/`](../Templates/README.md) for consistent prompt generation.
* **Standards Compliance:** All workflows enforce adherence to standards defined in other Zarichney.Standards directories.

## 4. Local Conventions & Constraints

* **Stateless AI Design:** All workflows assume AI agents have no memory between sessions and provide complete context in each interaction.
* **Incremental Development:** Workflows emphasize incremental, testable changes with proper documentation updates.
* **Quality Gates:** All workflows include verification steps (testing, formatting, documentation) before completion.
* **Branching Strategy:** Follows established Git workflow patterns with feature branches and proper PR processes.

## 5. How to Work With This Documentation

* **Starting AI Development:** Begin with [`CodingPlannerAssistant.md`](./CodingPlannerAssistant.md) to understand the planning and task decomposition process.
* **Choosing Workflows:** Select appropriate workflow based on task complexity and type:
  * [`StandardWorkflow.md`](./StandardWorkflow.md) for routine development tasks
  * [`ComplexTaskWorkflow.md`](./ComplexTaskWorkflow.md) for novel or complex features
  * [`TestCoverageWorkflow.md`](./TestCoverageWorkflow.md) for testing improvements
* **Git and Task Management:** Follow [`TaskManagementStandards.md`](./TaskManagementStandards.md) for branching, commits, and PR processes.
* **Planning and Roadmap:** Consult [`ShortTermRoadmap.md`](./ShortTermRoadmap.md) for upcoming features and technical debt items.

## 6. Dependencies

* **Parent:** [`Zarichney.Standards`](../README.md) - This directory is part of the overall documentation structure.
* **Critical Dependencies:**
    * [`../Templates/AICoderPromptTemplate.md`](../Templates/AICoderPromptTemplate.md): Template for generating AI Coder prompts
    * [`../Templates/TestCaseDevelopmentTemplate.md`](../Templates/TestCaseDevelopmentTemplate.md): Template for test coverage tasks
    * All standards directories ([`../Coding/`](../Coding/README.md), [`../Testing/`](../Testing/README.md), [`../Documentation/`](../Documentation/README.md)) for workflow enforcement
* **External Tools:** GitHub CLI, Git, .NET SDK, and Docker for workflow execution.

## 7. Rationale & Key Historical Context

* Workflows were developed to systematize AI-assisted development and ensure consistent quality across all development activities.
* The AI Planning Assistant approach was adopted to bridge the gap between high-level requirements and detailed AI Coder instructions.
* Multiple specialized workflows emerged from practical experience with different types of development tasks and their unique requirements.
* Task management standards evolved to support both human and AI collaboration within established Git workflows.

## 8. Known Issues & TODOs

* Workflows require periodic refinement based on practical experience with AI-assisted development.
* Additional specialized workflows may be needed for specific types of tasks (e.g., performance optimization, security updates).
* Integration with automated CI/CD pipelines may require workflow adjustments.
* Metrics and feedback mechanisms for workflow effectiveness could be enhanced.