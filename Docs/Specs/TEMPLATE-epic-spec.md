# Epic Specification Template

**Purpose:** This template provides a standardized structure for creating epic specification documents that capture high-level objectives, success criteria, architectural decisions, and implementation roadmap for major development initiatives.

**Target Audience:** Development teams, architects, and AI assistants who need comprehensive epic context for implementation planning and progress tracking.

**How to Use:**
1. Replace placeholders like `[Epic Number]`, `[Epic Title]`, `[YYYY-MM-DD]` with specific details
2. Fill out each section based on epic objectives and scope
3. Embed relevant Mermaid diagrams in Section 2 following DiagrammingStandards.md
4. Maintain relative links to component specifications and related documentation
5. Update regularly as epic progresses and architectural decisions evolve

---

# Epic #[Epic Number]: [Epic Title]

**Last Updated:** [YYYY-MM-DD]
**Epic Status:** [Planning/In Progress/Complete/On Hold]
**Epic Owner:** [Owner Name/Team]

> **Parent:** [`Specs`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** [Concise description of the epic's overall objective and scope within the project. E.g., "Comprehensive enhancement of CI/CD workflows to improve build reliability, deployment automation, and developer productivity."]
* **Key Objectives:**
  - [Primary objective 1 with measurable outcome]
  - [Primary objective 2 with measurable outcome]
  - [Primary objective 3 with measurable outcome]
* **Success Criteria:**
  - [Specific, testable criteria for epic completion]
  - [Performance metrics or quality gates]
  - [User/developer experience improvements]
* **Why it exists:** [Strategic rationale connecting to broader organizational goals or addressing specific pain points]

### Component Specifications
* **Component 1:** [`[Component Name]`](./components/[component-name].md) - [Brief description of component role]
* **Component 2:** [`[Component Name]`](./components/[component-name].md) - [Brief description of component role]

## 2. Architecture & Key Concepts

* **High-Level Design:** [Overview of major architectural components, services, or systems affected by this epic. Describe how components interact and integrate.]
* **Core Implementation Flow:** [Typical sequence for epic deliverables, focusing on component interactions and data flow]
* **Key Architectural Decisions:**
  - [Decision 1: Rationale and implications]
  - [Decision 2: Rationale and implications]
  - [Decision 3: Rationale and implications]
* **Integration Points:** [How this epic integrates with existing systems, external services, or other concurrent epics]

* **Architecture Diagram:**
  ```mermaid
  %% Epic architecture diagram - replace with actual epic structure
  graph TD
      A[Epic Component A] --> B{Integration Point};
      B --> C[Existing System C];
      B --> D[New System D];
      C --> E[Output/Result];
      D --> E;
  ```

## 3. Interface Contract & Assumptions

* **Key Epic Deliverables:**
  - **[Deliverable 1]:**
    * **Purpose:** [What this deliverable accomplishes]
    * **Dependencies:** [What must exist before this deliverable can be completed]
    * **Outputs:** [What this deliverable produces for other components or systems]
    * **Quality Gates:** [Testing, performance, or acceptance criteria]

* **Critical Assumptions:**
  - **Technical Assumptions:** [e.g., "Current CI/CD infrastructure can support enhanced workflows", "Team has necessary permissions for deployment automation"]
  - **Resource Assumptions:** [e.g., "Development team availability for 3-month implementation", "Budget allocation for external tooling if needed"]
  - **External Dependencies:** [e.g., "Third-party service availability", "Platform API stability"]

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Epic-Specific Standards:**
  - [Any conventions specific to this epic's implementation]
  - [Code patterns or architectural patterns to follow]
  - [Documentation requirements beyond standard project documentation]
* **Technology Constraints:**
  - [Technology choices mandated or restricted for this epic]
  - [Platform limitations affecting implementation approach]
  - [Integration constraints with existing systems]
* **Timeline Constraints:**
  - [Key milestone dates or dependencies]
  - [Resource availability windows]
  - [External deadlines affecting epic scope or approach]

## 5. How to Work With This Epic

* **Implementation Approach:**
  - [Recommended implementation strategy and sequencing]
  - [Component development order and dependencies]
  - [Integration testing strategy across epic components]
* **Quality Assurance:**
  - **Testing Strategy:** [Epic-level testing approach covering component integration]
  - **Validation Approach:** [How to validate epic success criteria are met]
  - **Performance Validation:** [Epic-level performance testing and benchmarking]
* **Common Implementation Pitfalls:**
  - [Known challenges specific to this epic]
  - [Integration complexity areas requiring special attention]
  - [Resource or timing dependencies that frequently cause issues]

## 6. Dependencies

* **Internal Code Dependencies:**
  - [`[Dependency Module]`](../../Code/[ModulePath]/README.md) - [Why this dependency exists and how it's used]
  - [`[Another Module]`](../../Code/[AnotherPath]/README.md) - [Dependency relationship]

* **External Dependencies:**
  - [External services, APIs, or platforms required]
  - [Third-party libraries or tools needed]
  - [Infrastructure requirements or platform dependencies]

* **Dependent Epics/Features:**
  - [`[Related Epic]`](../[related-epic]/README.md) - [Nature of dependency relationship]
  - [Features or components that depend on this epic's completion]

## 7. Rationale & Key Historical Context

* **Strategic Context:** [Why this epic was prioritized over alternative approaches]
* **Historical Evolution:** [How epic scope or approach evolved during planning]
* **Architectural Decision Records:** [Link to any formal ADRs created for this epic]
* **Alternative Approaches Considered:** [Other approaches evaluated and why they were not selected]

## 8. Known Issues & TODOs

* **Outstanding Design Decisions:**
  - [Design questions requiring resolution before implementation]
  - [Technical spikes needed to validate approach]
* **Implementation Risks:**
  - [Technical risks and mitigation strategies]
  - [Resource or timeline risks]
* **Future Enhancements:**
  - [Potential extensions or improvements beyond current epic scope]
  - [Related work that could build upon this epic's foundation]

---