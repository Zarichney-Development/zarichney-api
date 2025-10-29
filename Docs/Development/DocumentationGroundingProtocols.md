# Documentation Grounding Protocols Guide

**Last Updated:** 2025-10-26
**Purpose:** Comprehensive guide for systematic documentation context loading across all 11 agents
**Target Audience:** All agents (mandatory grounding before work), Claude (orchestration integration)

---

## Table of Contents

1. [Purpose & Philosophy](#1-purpose--philosophy)
2. [Agent Grounding Architecture](#2-agent-grounding-architecture)
3. [Grounding Protocol Phases](#3-grounding-protocol-phases)
4. [Agent-Specific Grounding Patterns](#4-agent-specific-grounding-patterns)
5. [Skills Integration](#5-skills-integration)
6. [Optimization Strategies](#6-optimization-strategies)
7. [Quality Validation](#7-quality-validation)

---

## 1. Purpose & Philosophy

### Self-Contained Knowledge for Stateless AI

Documentation grounding protocols embody the project's core philosophy: comprehensive context loading enables stateless AI assistants to operate effectively without prior session memory. Every agent engagement begins fresh—no memory of previous interactions, no inherent understanding of project structure, coding conventions, or architectural decisions. Documentation grounding transforms context-blind agents into fully-informed contributors who understand project standards and module-specific patterns before making any modifications.

### Why Grounding Matters: The Stateless Challenge

AI assistants in this project operate with complete state amnesia between engagements. When Claude delegates a task to BackendSpecialist, that agent:
- Has **NO memory** of previous backend implementations
- Does **NOT inherently know** coding standards, dependency injection patterns, or async/await conventions
- Cannot **recall** interface contracts, module dependencies, or architectural decisions
- Lacks **context** about testing strategies, local conventions, or historical rationale

Without systematic grounding, agents would:
- Violate established coding standards unknowingly (wrong naming conventions, improper DI usage)
- Break documented interface contracts (violating preconditions, ignoring postconditions)
- Ignore architectural patterns (creating inconsistencies across modules)
- Introduce regressions (through incomplete understanding of dependencies)
- Create technical debt (by deviating from project conventions)

### Benefits of Systematic Grounding

Comprehensive documentation grounding delivers measurable benefits:

**Consistency:** All agent modifications align with project standards (CodingStandards.md, TestingStandards.md, DocumentationStandards.md) through mandatory standards mastery.

**Quality:** Agents understand interface contracts (README Section 3), preventing contract violations and integration failures through behavioral comprehension.

**No Knowledge Drift:** Each engagement starts with current standards, preventing outdated pattern propagation and maintaining architectural coherence.

**Architectural Coherence:** Agents comprehend design decisions (README Section 7 rationale) and module relationships, creating modifications that fit the system design.

**Reduced Rework:** Fewer corrections needed when agents operate with comprehensive context, saving orchestration overhead and review cycles.

**Testability:** Agents understand testing strategies (README Section 5) and coverage requirements (TestingStandards.md), creating testable implementations.

### Self-Contained Knowledge Philosophy

Documentation grounding implements the project's self-contained knowledge philosophy:

**Complete Context in Each Document:** Standards documents and README files provide comprehensive information without assuming external knowledge. CodingStandards.md explains DI patterns fully; TestingStandards.md details AAA pattern completely; README Section 3 defines interface contracts explicitly.

**Explicit Assumptions and Constraints:** Nothing is implied or assumed—all preconditions, postconditions, and invariants are stated clearly. README files document "what must be true" for code to work correctly.

**Clear "Why" Explanations:** Documentation focuses on rationale, not just description. README Section 7 (Rationale & Historical Context) explains design decisions. Standards documents justify requirements.

**Interface Contracts:** Section 3 of every module README defines behavioral contracts beyond method signatures—preconditions (required input states), postconditions (expected output states), error handling (specific exceptions under certain conditions), and critical assumptions.

### Stateless AI Design Principles

Documentation grounding protocols are explicitly designed for stateless operation:

**No Memory Assumption:** Each grounding cycle assumes zero prior context. Agents load complete standards and module context every engagement.

**Progressive Complexity:** Grounding proceeds from foundation to specific (Phase 1 standards → Phase 2 architecture → Phase 3 domain-specific), building understanding systematically.

**Validation at Each Phase:** Agents confirm comprehension before proceeding (checklist completion, understanding validation).

**Self-Contained Execution:** Agents can complete grounding without external clarification through comprehensive documentation and clear workflows.

---

## 2. Agent Grounding Architecture

### Standards Loading Sequence (Foundation First)

Documentation grounding follows a strict loading order that builds understanding progressively:

**Why Order Matters:** Foundational knowledge must precede specialized knowledge. CodingStandards.md provides production code conventions that TestingStandards.md references for test quality. DocumentationStandards.md defines README structure that module READMEs use. Loading out-of-order creates knowledge gaps.

**Mandatory Phase 1 Sequence:**

1. **CodingStandards.md** (FIRST - Foundation)
   - Production code requirements (naming, patterns, conventions)
   - Dependency injection and service lifetimes
   - Asynchronous programming patterns
   - Error handling and logging
   - SOLID principles for testability
   - **Why First:** Establishes baseline code quality expectations all other standards reference

2. **TestingStandards.md** (SECOND - Quality Validation)
   - Test framework tooling (xUnit, FluentAssertions, Moq)
   - AAA pattern (Arrange-Act-Assert)
   - Test categorization and coverage requirements
   - Test isolation and determinism
   - **Why Second:** Builds on CodingStandards.md testability principles

3. **DocumentationStandards.md** (THIRD - Self-Contained Knowledge)
   - README template and 8-section structure
   - Self-contained knowledge philosophy
   - Linking strategy (parent, child, dependencies)
   - Maintenance triggers and pruning
   - **Why Third:** Defines how to navigate and interpret all module READMEs

4. **TaskManagementStandards.md** (FOURTH - Workflow Integration)
   - Git branching conventions
   - Conventional commit format
   - PR creation requirements
   - Issue tracking integration
   - **Why Fourth:** Builds on understanding of code, tests, and documentation changes

5. **DiagrammingStandards.md** (FIFTH - Visual Communication)
   - Mermaid diagram conventions
   - Diagram types and usage
   - Embedding vs. external storage
   - **Why Fifth:** Applies to architectural documentation after understanding code and docs

**Sequential Loading Rationale:** Each standard references concepts from prior standards. TestingStandards.md assumes knowledge of SOLID principles from CodingStandards.md. DocumentationStandards.md references testability from TestingStandards.md. Out-of-order loading creates comprehension gaps.

### Module README Discovery Patterns

After standards mastery, agents discover project architecture through systematic README navigation:

**Root README.md (Project Overview):**
- Entry point for understanding overall system structure
- Provides technology stack, architecture overview, and navigation to all subsystems
- Links to major module READMEs for deep-dive

**Module-Specific README Hierarchy:**
- Each directory with significant code has README.md
- Parent→Child linking enables navigation from general to specific
- Child→Parent linking provides context breadth
- Dependency linking maps integration relationships

**Self-Contained Knowledge in Each README:**
- Every README provides complete context for its module
- Section 1 (Purpose & Responsibility) explains module's role
- Section 2 (Architecture & Key Concepts) details internal design
- Section 3 (Interface Contract) defines behavioral contracts
- Section 6 (Dependencies) maps integration points
- Section 7 (Rationale) explains design decisions

### Architectural Pattern Recognition

Documentation grounding enables pattern recognition through systematic discovery:

**Identifying Established Patterns:**
- CodingStandards.md defines global patterns (DI, async/await, nullable types)
- Module READMEs Section 2 document module-specific patterns (repository pattern, CQRS, validation layers)
- Section 4 (Local Conventions) highlights deviations from global standards

**Understanding "Why" from Section 7:**
- Rationale & Historical Context explains non-obvious design choices
- Illuminates current implementation through historical lens
- Prevents repeating past mistakes by understanding what was tried and why it changed

**Pattern Application:**
- Agents apply discovered patterns to new implementations
- Maintain consistency across module boundaries
- Respect architectural decisions with documented rationale

### Integration Point Validation

Grounding enables agents to understand how components integrate:

**Dependency Discovery (Section 6):**
- Internal modules directly consumed (with README links for deep-dive)
- Internal modules that consume this module (impact analysis)
- External libraries with integration implications
- Testing considerations (mocking needs, virtualization requirements)

**Interface Contract Understanding (Section 3):**
- Preconditions: What must be true before calling this module
- Postconditions: What will be true after calling this module
- Error handling: Specific exceptions under specific conditions
- Invariants: Conditions always maintained

**Cross-Module Coherence:**
- Understanding how changes affect dependent modules
- Respecting interface contracts when modifying implementations
- Coordinating changes across module boundaries

---

## 3. Grounding Protocol Phases

### Phase 1: Standards Mastery (MANDATORY)

Every agent, every engagement, must load ALL relevant standards documents before making any modifications.

#### Universal Standards (Required for All Agents)

**Location:** `/Docs/Standards/`

**1. CodingStandards.md - Production Code Requirements**

Load this FIRST to establish baseline quality expectations.

**Key Sections to Master:**
- **Naming Conventions:** PascalCase for types, camelCase for local variables, `I` prefix for interfaces
- **Modern C# Features:** File-scoped namespaces, primary constructors, records, nullable reference types
- **Dependency Injection:** Constructor injection, service lifetimes (Singleton, Scoped, Transient), avoiding service locator anti-pattern
- **SOLID Principles:** Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion—emphasis on testability
- **Asynchronous Programming:** Async/await patterns, CancellationToken usage, avoiding async void, ConfigureAwait considerations
- **Error Handling:** Custom exception types, logging patterns, exception propagation vs. catching
- **Null Handling:** Nullable reference types, null-conditional operators, null-forgiving operator usage

**Validation:**
- [ ] Can explain why constructor injection is preferred over property injection
- [ ] Understand when to use Scoped vs. Transient service lifetime
- [ ] Know async/await best practices (CancellationToken, avoiding blocking calls)
- [ ] Recognize SOLID violations and how to refactor toward testability

**Why This Matters:** Every code modification must align with these conventions. Missing this grounding leads to immediate standards violations.

**2. TestingStandards.md - Test Quality Requirements**

Load this SECOND to understand test quality expectations.

**Key Sections to Master:**
- **Test Framework Tooling:** xUnit for test execution, FluentAssertions for readable assertions, Moq for mocking dependencies
- **Unit vs. Integration Tests:** Unit tests for isolated logic, integration tests for database/API interactions
- **AAA Pattern:** Arrange (setup), Act (execute), Assert (verify) structure for all tests
- **Test Categorization:** `[Trait("Category", "Unit")]` and `[Trait("Category", "Integration")]` for selective execution
- **Coverage Goals:** Comprehensive backend coverage initiative, 100% executable test pass rate
- **Test Isolation:** Each test independent, no shared state, deterministic execution
- **Test Naming:** Descriptive names following `MethodName_Scenario_ExpectedResult` pattern

**Validation:**
- [ ] Can structure tests using AAA pattern correctly
- [ ] Understand when to use unit vs. integration tests
- [ ] Know how to apply test categorization traits
- [ ] Recognize coverage expectations for new code

**Why This Matters:** Test quality directly impacts coverage excellence goals. Agents must understand testing strategy before implementing testable code.

**3. DocumentationStandards.md - README Structure and Self-Contained Knowledge**

Load this THIRD to understand how to navigate and interpret all module documentation.

**Key Sections to Master:**
- **Per-Directory README.md Mandate:** Every significant directory has README.md
- **Self-Contained Knowledge Philosophy:** Each README provides complete context without external assumptions
- **8-Section Structure Template:** Purpose & Responsibility, Architecture & Key Concepts, Interface Contract & Assumptions, Local Conventions & Constraints, How to Work With This Code, Dependencies, Rationale & Historical Context, Known Issues & TODOs
- **Linking Strategy:** Parent links (navigation upward), child links (navigation downward), dependency links (integration understanding)
- **Maintenance Triggers:** Update README when code changes affect documented contracts, architecture, or testing strategy
- **Section 3 Critical:** Interface Contract defines behavioral expectations beyond method signatures

**Validation:**
- [ ] Understand 8-section README structure and purpose of each section
- [ ] Know to review Section 3 (Interface Contract) before modifying public APIs
- [ ] Recognize linking patterns (parent, child, dependency)
- [ ] Understand self-contained knowledge requirement

**Why This Matters:** Enables navigation through module hierarchy and understanding of interface contracts. Missing this grounding prevents effective Phase 3 domain-specific loading.

**4. TaskManagementStandards.md - Git Workflow, Branching, Commits**

Load this FOURTH to understand Git workflow integration.

**Key Sections to Master:**
- **Branch Naming Conventions:** `feature/issue-XXX-description`, `test/issue-XXX-description`, `fix/issue-XXX-description`
- **Conventional Commit Format:** `<type>: <description> (#ISSUE_ID)` where type is feat/fix/test/docs/refactor/chore
- **PR Creation Requirements:** Comprehensive descriptions, links to issues, clear acceptance criteria
- **Git Workflow Patterns:** Feature → Epic → Develop → Main progression

**Validation:**
- [ ] Can create properly named branches
- [ ] Know conventional commit message format
- [ ] Understand PR description requirements

**Why This Matters:** Ensures agent work integrates properly with version control and CI/CD workflows.

**5. DiagrammingStandards.md - Visual Communication (Load if Architectural Changes Planned)**

Load this FIFTH if work involves architectural diagrams.

**Key Sections to Master:**
- **Mermaid Diagram Conventions:** flowchart, sequence, class diagram types
- **Embedding vs. External Storage:** Embed diagrams in README Section 2, use external files for complex diagrams
- **Diagram Maintenance:** Update diagrams when architecture changes
- **Standard Styles:** Consistent classDef styles across diagrams

**Validation:**
- [ ] Understand when to embed vs. externalize diagrams
- [ ] Know Mermaid diagram types and appropriate usage
- [ ] Recognize diagram maintenance triggers

**Why This Matters:** Architectural changes often require diagram updates. Missing this grounding leads to outdated visual documentation.

#### Standards Loading Workflow

**Complete Phase 1 Before Proceeding:**

1. **Load CodingStandards.md:** Review naming conventions, DI patterns, async/await, SOLID principles (~5-8 minutes reading time)
2. **Load TestingStandards.md:** Review test framework, AAA pattern, categorization, coverage goals (~3-5 minutes)
3. **Load DocumentationStandards.md:** Review 8-section template, linking strategy, Section 3 importance (~3-5 minutes)
4. **Load TaskManagementStandards.md:** Review branch naming, commit format, PR requirements (~2-3 minutes)
5. **Load DiagrammingStandards.md (if applicable):** Review Mermaid conventions, embedding patterns (~2-3 minutes)

**Total Phase 1 Time Investment:** ~15-25 minutes for comprehensive standards mastery

**Phase 1 Completion Checklist:**
- [ ] CodingStandards.md reviewed and key patterns understood
- [ ] TestingStandards.md reviewed for test quality requirements
- [ ] DocumentationStandards.md reviewed for README structure
- [ ] TaskManagementStandards.md reviewed for Git workflow
- [ ] DiagrammingStandards.md reviewed if architectural changes planned
- [ ] Can explain core concepts from each standard without re-reading

**Resource:** See `.claude/skills/documentation/documentation-grounding/resources/templates/standards-loading-checklist.md` for complete validation checklist.

---

### Phase 2: Project Architecture Context

After standards mastery, understand overall project structure and navigation hierarchy.

#### Root README Discovery

**Location:** `/README.md` (project root)

**What to Extract:**
- **Project Overview:** zarichney-api monorepo, .NET 8 backend + Angular 19 frontend
- **Technology Stack:** ASP.NET Core, EF Core, xUnit, Angular, NgRx, Material Design
- **Directory Structure:**
  - `/Code/Zarichney.Server/` - Main backend application
  - `/Code/Zarichney.Server.Tests/` - Backend tests
  - `/Code/Zarichney.Website/` - Frontend application
  - `/Docs/` - All project documentation
  - `/.claude/` - Multi-agent orchestration
  - `/Scripts/` - Automation and build scripts
- **Key Architectural Decisions:** Multi-agent development team, AI-powered code review, comprehensive testing excellence initiative
- **Development Setup:** Prerequisites, build commands, test execution
- **Navigation Links:** Links to major module READMEs for deep-dive

**Validation:**
- [ ] Understand project structure (backend, frontend, tests, docs)
- [ ] Know technology stack (.NET 8, Angular 19)
- [ ] Aware of multi-agent architecture and AI-powered workflows
- [ ] Can navigate to major module READMEs from root

**Why This Matters:** Provides context for how current task fits into overall system. Understanding project-wide patterns before module-specific work.

#### Module Hierarchy Mapping

**Process:**

1. **Identify Relevant Modules for Current Task:**
   - Backend task → Focus on `/Code/Zarichney.Server/` hierarchy
   - Frontend task → Focus on `/Code/Zarichney.Website/` hierarchy
   - Testing task → Focus on `/Code/Zarichney.Server.Tests/` hierarchy
   - Documentation task → Focus on `/Docs/` hierarchy

2. **Map Parent→Child Relationships:**
   - Start at root README
   - Navigate to relevant top-level module README
   - Follow child links to specific submodules
   - Build mental model of hierarchy

3. **Understand Dependency Relationships:**
   - Review Section 6 (Dependencies) of relevant READMEs
   - Map which modules consume which other modules
   - Identify external dependencies (NuGet packages, npm packages)

4. **Locate Architectural Diagrams:**
   - Review Section 2 (Architecture & Key Concepts) for embedded Mermaid diagrams
   - Understand data flow, component relationships, interaction patterns
   - Visualize system structure before modifications

**Example Hierarchy for Backend Work:**
```
/README.md (root)
  ↓
/Code/Zarichney.Server/README.md (backend overview)
  ↓
/Code/Zarichney.Server/Controllers/README.md (API layer)
  ↓
/Code/Zarichney.Server/Services/README.md (business logic layer)
  ↓
/Code/Zarichney.Server/Repositories/README.md (data access layer)
```

**Validation:**
- [ ] Module hierarchy relevant to task identified and mapped
- [ ] Parent→child relationships understood
- [ ] Dependency relationships reviewed (Section 6 of relevant READMEs)
- [ ] Architectural diagrams located and reviewed
- [ ] Mental model of system structure established

**Why This Matters:** Changes in one module may affect dependent modules. Understanding integration points prevents breaking changes.

#### Integration Points Understanding

**What to Identify:**

**API Contract Definitions:**
- Controller endpoints and their routes
- Request/response DTOs
- Versioning strategies
- Authentication/authorization requirements

**Service Layer Boundaries:**
- Service interfaces and implementations
- Dependency injection registration
- Transaction boundaries
- Business logic encapsulation

**Database Schema Relationships:**
- Entity models and relationships
- EF Core configuration
- Migration patterns
- Data seeding strategies

**External Dependency Integration Patterns:**
- Third-party NuGet packages
- External API integrations
- Configuration management
- Secret management

**Authentication and Authorization Flows:**
- Identity management
- JWT token handling
- Role-based access control
- Policy-based authorization

**Validation:**
- [ ] API contracts for relevant endpoints understood
- [ ] Service layer boundaries and DI registration patterns identified
- [ ] Database schema relationships mapped
- [ ] External dependencies and integration patterns reviewed
- [ ] Auth/authz flows understood if security-relevant

**Why This Matters:** Modifications must respect integration contracts. Understanding boundaries prevents integration failures.

**Phase 2 Completion Checklist:**
- [ ] Root README.md reviewed for project overview
- [ ] Module hierarchy relevant to task mapped and understood
- [ ] Dependency relationships identified (Section 6 analysis)
- [ ] Integration points validated (API contracts, service boundaries, database schemas)
- [ ] Architectural diagrams reviewed for system structure visualization
- [ ] Can explain how current task fits into overall architecture

**Resource:** See `.claude/skills/documentation/documentation-grounding/resources/templates/module-context-template.md` for structured analysis framework.

---

### Phase 3: Domain-Specific Context

Deep-dive into specific module context directly relevant to current task.

#### Module README Analysis (Section-by-Section)

**Location:** Module-specific `README.md` files at target directory level

**Complete 8-Section Analysis:**

**Section 1: Purpose & Responsibility**

Extract:
- Module's functional role (what it does)
- Why it exists as separate unit (separation of concerns rationale)
- Child modules and their relationships (if directory has subdirectories)

Validation:
- [ ] Understand module's single responsibility
- [ ] Know why it's separated from other modules
- [ ] Aware of child module structure

**Why This Matters:** Establishes scope boundaries. Modifications should align with documented responsibility.

---

**Section 2: Architecture & Key Concepts**

Extract:
- Internal design patterns (repository pattern, CQRS, validation layers)
- Key components and data structures (entities, value objects, DTOs)
- Embedded Mermaid diagrams visualizing architecture
- Workflow sequences and data flows

Validation:
- [ ] Understand internal design patterns used
- [ ] Know key components and their relationships
- [ ] Reviewed architectural diagrams if present
- [ ] Comprehend data flow through module

**Why This Matters:** New implementations must follow established patterns. Diagrams provide visual understanding of complex architectures.

---

**Section 3: Interface Contract & Assumptions (CRITICAL)**

**THIS IS THE MOST CRITICAL SECTION FOR PREVENTING INTEGRATION FAILURES.**

Extract:
- **Preconditions:** Required input states, valid ranges, format requirements, non-null assumptions
- **Postconditions:** Expected output states, side effects, database changes, state transitions
- **Error Handling:** Specific exceptions thrown under specific conditions, validation failure responses
- **Invariants:** Conditions always maintained (e.g., "Email must always be validated before persistence")
- **Critical Assumptions:** Expectations about calling context, environment, dependencies
- **Behavioral Contracts:** Behaviors not obvious from method signatures alone

Validation:
- [ ] All preconditions for public methods understood
- [ ] All postconditions and side effects identified
- [ ] Error handling patterns and specific exceptions noted
- [ ] Invariants and critical assumptions acknowledged
- [ ] Behavioral contracts beyond method signatures comprehended

**Why This Matters:** Interface contracts are behavioral specifications. Violating preconditions causes runtime errors. Ignoring postconditions breaks dependent code. Missing error handling creates unhandled exceptions.

**Example Contract Analysis:**
```csharp
// From UserService README Section 3
public async Task<User> GetUserByIdAsync(int userId, CancellationToken cancellationToken)

PRECONDITIONS:
- userId must be > 0 (throws ArgumentOutOfRangeException if <= 0)
- Database connection must be available
- CancellationToken should be passed from controller context

POSTCONDITIONS:
- Returns User entity if found
- Returns null if user does not exist (NOT throws exception)
- User entity includes related data (Address, Preferences) via eager loading

ERROR HANDLING:
- ArgumentOutOfRangeException: userId <= 0
- OperationCanceledException: If cancellation requested
- DbException: Database connectivity issues (propagated, not caught)

ASSUMPTIONS:
- Calling code handles null returns appropriately
- Calling code has validated user authorization before calling
- Database schema includes User.Address and User.Preferences relationships
```

---

**Section 4: Local Conventions & Constraints**

Extract:
- Module-specific deviations from global CodingStandards.md
- Configuration requirements unique to this module
- Environmental dependencies (database, external APIs)
- Testing setup requirements specific to module

Validation:
- [ ] Local deviations from global standards identified
- [ ] Configuration requirements understood
- [ ] Environmental dependencies noted
- [ ] Module-specific test setup requirements acknowledged

**Why This Matters:** Prevents applying global patterns where module-specific conventions exist. Identifies special setup needs.

---

**Section 5: How to Work With This Code**

Extract:
- **Setup Steps:** Module-specific initialization, configuration, seed data
- **Module-Specific Testing Strategy:** Primary testing approach (unit-heavy, integration-focused, E2E)
- **Key Test Scenarios:** Important scenarios requiring coverage (edge cases, error conditions, integration points)
- **Test Data Considerations:** Specific data generation strategies, fixture requirements, seed data needs
- **Commands:** Module-specific test execution commands
- **Known Pitfalls:** Gotchas, non-obvious behaviors, common mistakes

Validation:
- [ ] Setup steps for module work environment understood
- [ ] Testing strategy for module identified
- [ ] Key test scenarios requiring coverage noted
- [ ] Test data requirements understood
- [ ] Module-specific test commands known
- [ ] Known pitfalls and gotchas acknowledged

**Why This Matters:** Enables effective development and testing. Prevents common mistakes through explicit gotcha documentation.

---

**Section 6: Dependencies**

Extract:
- **Internal Modules Consumed:** Direct dependencies with links to their READMEs
- **Internal Modules That Consume This Module:** Dependent modules with links (impact analysis)
- **External Libraries:** NuGet packages, npm packages with version requirements
- **Testing Implications:** Mocking requirements, virtualization needs, test double strategies

Validation:
- [ ] Direct dependencies identified and their READMEs reviewed
- [ ] Dependent modules identified (understand change impact)
- [ ] External library dependencies and versions noted
- [ ] Testing implications for dependencies understood

**Why This Matters:** Changes affect dependent modules. Understanding dependencies guides mocking strategy and impact analysis.

**Workflow:**
1. Review Section 6 Dependencies list
2. Follow README links for consumed dependencies
3. Review their Section 3 (Interface Contract) to understand integration points
4. Identify dependent modules to assess change impact
5. Note external dependencies requiring test doubles or virtualization

---

**Section 7: Rationale & Key Historical Context**

Extract:
- **Design Decision Explanations:** Why current implementation chosen over alternatives
- **Non-Obvious Architectural Choices:** Decisions that aren't immediately apparent from code
- **Historical Notes:** Past approaches tried and why they changed (only if illuminating current state)

Validation:
- [ ] Design decisions and rationale understood
- [ ] Non-obvious architectural choices identified
- [ ] Historical context illuminating current implementation reviewed

**Why This Matters:** Prevents repeating past mistakes. Understanding "why" prevents undoing thoughtful decisions. Illuminates constraints that led to current design.

**Important:** Only historical context that illuminates CURRENT state belongs here. Obsolete historical notes should be pruned during README maintenance.

---

**Section 8: Known Issues & TODOs**

Extract:
- **Current Limitations:** Specific to this module
- **Planned Work:** Technical debt, feature additions, refactoring plans
- **Integration Concerns:** Known issues with dependent modules

Validation:
- [ ] Known limitations acknowledged
- [ ] Planned work and TODOs noted
- [ ] Integration concerns understood

**Why This Matters:** Prevents working on known broken functionality. Identifies areas requiring caution. Coordinates with planned work.

---

#### Domain-Specific Loading Workflow

**Complete Section-by-Section Analysis:**

1. **Identify Target Module:** Determine specific module requiring modification
2. **Locate Module README.md:** Navigate to module directory README
3. **Section 1 Analysis:** Understand purpose and responsibility (~2 minutes)
4. **Section 2 Analysis:** Study architecture and review diagrams (~5-8 minutes)
5. **Section 3 Analysis (CRITICAL):** Thoroughly analyze interface contracts, preconditions, postconditions, error handling (~8-12 minutes)
6. **Section 4 Analysis:** Note local conventions and constraints (~2-3 minutes)
7. **Section 5 Analysis:** Review testing strategy, key scenarios, known pitfalls (~3-5 minutes)
8. **Section 6 Analysis:** Map dependencies, follow links to dependency READMEs, assess impact (~5-8 minutes)
9. **Section 7 Analysis:** Understand design rationale and historical context (~3-5 minutes)
10. **Section 8 Analysis:** Acknowledge known issues and TODOs (~2 minutes)

**Total Phase 3 Time Investment:** ~30-50 minutes for comprehensive domain-specific grounding

**Phase 3 Completion Checklist:**
- [ ] Target module README.md identified and loaded
- [ ] Section 1 (Purpose & Responsibility) understood
- [ ] Section 2 (Architecture & Key Concepts) studied, diagrams reviewed
- [ ] Section 3 (Interface Contract) thoroughly analyzed—preconditions, postconditions, error handling, assumptions clear
- [ ] Section 4 (Local Conventions) noted
- [ ] Section 5 (How to Work With This Code) testing strategy and key scenarios identified
- [ ] Section 6 (Dependencies) mapped, dependency READMEs reviewed for integration understanding
- [ ] Section 7 (Rationale) design decisions understood
- [ ] Section 8 (Known Issues) acknowledged
- [ ] Can explain module's role, architecture, contracts, and integration without re-reading

**Resource:** See `.claude/skills/documentation/documentation-grounding/resources/examples/` for agent-specific realistic grounding demonstrations.

---

## 4. Agent-Specific Grounding Patterns

Different agents have different context priorities based on domain expertise. All agents complete Phase 1 (Standards Mastery), but Phase 2 and Phase 3 focus varies by agent role.

### 1. CodeChanger

**Domain:** Production code implementation, bug fixes, refactoring across backend and frontend

**Phase 1 Priority Standards:**
- **CodingStandards.md** (COMPREHENSIVE): Master naming conventions, DI patterns, async/await, SOLID principles, error handling, null handling
- **TestingStandards.md** (DESIGN FOR TESTABILITY): Understand how to write testable code (dependency injection, interface segregation)
- **DocumentationStandards.md** (MODERATE): Understand README Section 3 (Interface Contract) requirements for API changes
- **TaskManagementStandards.md** (BASIC): Branch naming, commit format

**Phase 2 Architecture:**
- Root README for project overview
- Backend hierarchy (`Code/Zarichney.Server/`) for backend tasks
- Frontend hierarchy (`Code/Zarichney.Website/`) for frontend tasks

**Phase 3 Domain Context:**
- **Target Module README** (CRITICAL SECTIONS):
  - Section 2 (Architecture): Understand internal patterns to maintain consistency
  - Section 3 (Interface Contract): MUST understand before modifying public APIs
  - Section 5 (How to Work): Testing strategy informs implementation approach
  - Section 6 (Dependencies): Integration points guide dependency usage
- **Dependency Module READMEs:** Follow Section 6 links to understand consumed services

**Grounding Emphasis:**
- Interface contracts (Section 3): Preconditions, postconditions, error handling
- Architectural patterns (Section 2): Repository pattern, validation layers, CQRS
- Testing implications (Section 5): How to implement testable code

**Example Grounding Workflow:**
```
Task: "Implement UserService.UpdateEmailAsync method"

Phase 1 Standards:
✅ CodingStandards.md: DI constructor injection, async/await with CancellationToken, nullable reference types
✅ TestingStandards.md: Design for unit testing (interface abstraction, dependency injection)
✅ DocumentationStandards.md: Section 3 interface contract requirements

Phase 2 Architecture:
✅ Root README: Project structure
✅ Code/Zarichney.Server/README.md: Backend architecture overview

Phase 3 Domain-Specific:
✅ Code/Zarichney.Server/Services/UserService README.md
   - Section 2: Repository pattern usage, validation layer interaction
   - Section 3: Preconditions (email format validation), postconditions (database update, audit log), error handling (DuplicateEmailException)
   - Section 5: Unit test strategy with mocked repository
   - Section 6: Dependencies on IUserRepository, IEmailValidator
✅ Code/Zarichney.Server/Repositories/IUserRepository README.md: Interface contract for database operations

Implementation Outcome:
- Follows async/await patterns from CodingStandards.md
- Uses constructor DI for IUserRepository and IEmailValidator
- Respects preconditions (validates email format before database call)
- Implements postconditions (updates database, writes audit log)
- Throws DuplicateEmailException per Section 3 error handling contract
- Designed for unit testing with injected dependencies
```

---

### 2. TestEngineer

**Domain:** Unit tests, integration tests, test coverage, quality assurance

**Phase 1 Priority Standards:**
- **TestingStandards.md** (COMPREHENSIVE): Master xUnit, FluentAssertions, Moq, AAA pattern, categorization, coverage goals
- **CodingStandards.md** (SUT UNDERSTANDING): Understand production code patterns to test effectively
- **DocumentationStandards.md** (MODERATE): Understand README Section 5 (Testing Strategy) requirements

**Phase 2 Architecture:**
- Root README for project overview
- Test project structure (`Code/Zarichney.Server.Tests/TechnicalDesignDocument.md`)
- Module under test hierarchy

**Phase 3 Domain Context:**
- **Module Under Test README** (CRITICAL SECTIONS):
  - Section 3 (Interface Contract): Test preconditions, postconditions, error handling comprehensively
  - Section 5 (How to Work): Module-specific testing strategy, key scenarios, test data considerations
  - Section 6 (Dependencies): Understand mocking requirements for dependencies
  - Section 8 (Known Issues): Avoid testing known broken functionality
- **Test Project README:** Test organization, fixture patterns, categorization strategy

**Grounding Emphasis:**
- Interface contracts (Section 3): Design tests covering all preconditions, postconditions, error conditions
- Testing strategy (Section 5): Understand module-specific testing approach
- Dependencies (Section 6): Identify mocking needs, test doubles, virtualization requirements

**Example Grounding Workflow:**
```
Task: "Create comprehensive tests for UserService.UpdateEmailAsync"

Phase 1 Standards:
✅ TestingStandards.md: AAA pattern, FluentAssertions syntax, Moq setup, Category=Unit trait
✅ CodingStandards.md: Understand async/await, DI patterns to test effectively

Phase 2 Architecture:
✅ Code/Zarichney.Server.Tests/README.md: Test project organization, fixture usage

Phase 3 Domain-Specific:
✅ Code/Zarichney.Server/Services/UserService README.md
   - Section 3 Interface Contract:
     * Preconditions: userId > 0, email non-null and valid format
     * Postconditions: Database updated, audit log written, returns updated User
     * Error handling: ArgumentOutOfRangeException (userId <= 0), ArgumentNullException (null email), InvalidEmailFormatException (invalid format), DuplicateEmailException (email already exists)
   - Section 5 Testing Strategy: Unit test focus with mocked IUserRepository and IEmailValidator
   - Section 6 Dependencies: IUserRepository (mock), IEmailValidator (mock)
✅ Identified test scenarios:
   - Happy path: Valid userId and email format → Database updated, audit logged
   - Precondition violations: userId <= 0, null email, invalid email format
   - Business rule violations: Duplicate email (already exists for different user)
   - Postcondition validation: Verify repository called, audit log written

Test Implementation Outcome:
- AAA pattern structure per TestingStandards.md
- FluentAssertions for readable assertions
- Moq for IUserRepository and IEmailValidator mocking
- Comprehensive coverage:
  * UpdateEmailAsync_ValidInputs_UpdatesDatabaseAndLogsAudit (happy path)
  * UpdateEmailAsync_UserIdZero_ThrowsArgumentOutOfRangeException (precondition)
  * UpdateEmailAsync_NullEmail_ThrowsArgumentNullException (precondition)
  * UpdateEmailAsync_InvalidEmailFormat_ThrowsInvalidEmailFormatException (validation)
  * UpdateEmailAsync_DuplicateEmail_ThrowsDuplicateEmailException (business rule)
- Category=Unit trait applied
```

---

### 3. DocumentationMaintainer

**Domain:** README files, project documentation, standards compliance

**Phase 1 Priority Standards:**
- **DocumentationStandards.md** (COMPREHENSIVE): Master 8-section template, self-contained knowledge philosophy, linking strategy, maintenance triggers
- **DiagrammingStandards.md** (COMPREHENSIVE): Master Mermaid conventions, embedding patterns
- **CodingStandards.md** (TECHNICAL ACCURACY): Understand code patterns to document them accurately
- **TestingStandards.md** (TECHNICAL ACCURACY): Understand testing patterns to document Section 5 accurately
- **TaskManagementStandards.md** (BASIC): Understand Git workflow for documentation commits

**Phase 2 Architecture:**
- Root README for navigation understanding
- Full module hierarchy to validate linking integrity
- Cross-reference network validation

**Phase 3 Domain Context:**
- **Target Module README** (ALL 8 SECTIONS):
  - Section 1: Validate purpose and child links
  - Section 2: Update architecture and diagrams
  - Section 3: Ensure interface contracts accurate
  - Section 4: Document local conventions
  - Section 5: Update testing strategy
  - Section 6: Validate dependency links
  - Section 7: Update rationale, prune obsolete historical notes
  - Section 8: Update known issues and TODOs
- **Parent README:** Validate parent link accuracy
- **Child READMEs:** Validate child links and consistency

**Grounding Emphasis:**
- Complete 8-section template compliance (DocumentationStandards.md)
- Self-contained knowledge philosophy (no external assumptions)
- Linking network integrity (parent, child, dependency links functional)
- Technical accuracy (reflects actual code behavior, not aspirational)

**Example Grounding Workflow:**
```
Task: "Update UserService README.md after UpdateEmailAsync implementation"

Phase 1 Standards:
✅ DocumentationStandards.md: 8-section template, Section 3 interface contract requirements, linking strategy
✅ DiagrammingStandards.md: Mermaid embedding patterns if architecture diagram update needed
✅ CodingStandards.md: Understand DI, async/await patterns for technical accuracy
✅ TestingStandards.md: Understand testing strategy to document Section 5 accurately

Phase 2 Architecture:
✅ Root README: Navigation structure
✅ Code/Zarichney.Server/README.md: Backend module hierarchy

Phase 3 Domain-Specific:
✅ Code/Zarichney.Server/Services/UserService README.md (ALL SECTIONS):
   - Section 1: Purpose & Responsibility (no change needed)
   - Section 2: Architecture (add UpdateEmailAsync to workflow diagram if present)
   - Section 3 (CRITICAL UPDATE): Add UpdateEmailAsync interface contract:
     * Preconditions: userId > 0, email non-null and valid format
     * Postconditions: Database updated, audit logged, returns User
     * Error handling: ArgumentOutOfRangeException, ArgumentNullException, InvalidEmailFormatException, DuplicateEmailException
   - Section 4: Local Conventions (no change)
   - Section 5: Testing Strategy (add UpdateEmailAsync to key test scenarios)
   - Section 6: Dependencies (validate IUserRepository, IEmailValidator links)
   - Section 7: Rationale (add if UpdateEmailAsync has non-obvious design decisions)
   - Section 8: Known Issues (remove if UpdateEmailAsync resolved any)
✅ Parent link validation: ../README.md correct
✅ Child links validation: No child modules
✅ Dependency links validation: IUserRepository README, IEmailValidator README links functional

Documentation Update Outcome:
- Section 3 updated with complete UpdateEmailAsync contract
- Section 5 updated with testing strategy for new method
- All links validated and functional
- Last Updated date updated to current date
- Self-contained knowledge preserved (contract fully explained)
- Technical accuracy validated against actual implementation
```

---

### 4. BackendSpecialist

**Domain:** .NET 8, C#, EF Core, ASP.NET Core expertise, API architecture, service layer design, database schemas

**Phase 1 Priority Standards:**
- **CodingStandards.md** (COMPREHENSIVE): Master DI, async patterns, SOLID, error handling
- **TestingStandards.md** (DESIGN FOR TESTABILITY): Understand test requirements
- **DocumentationStandards.md** (MODERATE): Section 3 interface contracts
- **TaskManagementStandards.md** (BASIC): Git workflow

**Phase 2 Architecture:**
- Backend module hierarchy (`Code/Zarichney.Server/`)
- Service layer, repository layer, controller layer navigation
- Database schema documentation

**Phase 3 Domain Context:**
- **API Controller README** (if endpoint work): Section 2 (routing patterns), Section 3 (API contracts)
- **Service Layer README** (if business logic work): Section 2 (service patterns), Section 3 (service contracts)
- **Repository Layer README** (if data access work): Section 2 (EF Core patterns), Section 3 (repository contracts)
- **Entity Models README** (if schema work): Section 2 (entity relationships), Section 3 (invariants)

**Grounding Emphasis:**
- Section 3 (Interface Contracts): API preconditions/postconditions, service contracts, repository contracts
- Section 6 (Dependencies): Service layer dependencies, repository abstractions
- Section 2 (Architecture): Layered architecture patterns, CQRS, repository pattern

**Example:** See `.claude/skills/documentation/documentation-grounding/resources/examples/backend-specialist-grounding.md` for complete realistic workflow.

---

### 5. FrontendSpecialist

**Domain:** Angular 19, TypeScript, NgRx, Material Design expertise, component architecture, state management

**Phase 1 Priority Standards:**
- **CodingStandards.md** (APPLICABLE PATTERNS): Naming conventions, SOLID principles, async patterns
- **DocumentationStandards.md** (MODERATE): Section 3 interface contracts for components
- **TaskManagementStandards.md** (BASIC): Git workflow

**Phase 2 Architecture:**
- Frontend module hierarchy (`Code/Zarichney.Website/`)
- Component organization, state management, routing navigation

**Phase 3 Domain Context:**
- **Component README** (if component work): Section 2 (component architecture), Section 3 (Input/Output contracts)
- **State Management README** (if NgRx work): Section 2 (state patterns), Section 3 (action contracts)
- **Service README** (if API integration): Section 2 (HTTP patterns), Section 3 (service contracts)

**Grounding Emphasis:**
- Section 2 (Architecture): Component communication patterns, state architecture
- Section 5 (Local Conventions): Frontend-specific conventions
- Section 6 (Dependencies): API client integration, third-party libraries

---

### 6. SecurityAuditor

**Domain:** Security hardening, vulnerability assessment, threat analysis, OWASP compliance

**Phase 1 Priority Standards:**
- **CodingStandards.md** (SECURITY SECTIONS): Error handling, input validation, auth patterns
- **TestingStandards.md** (SECURITY TESTING): Security test requirements

**Phase 2 Architecture:**
- Authentication/authorization module architecture
- Security-critical API endpoints

**Phase 3 Domain Context:**
- **Auth Services README**: Section 3 (authentication contracts), Section 8 (known security issues)
- **API Controllers README**: Section 3 (authorization requirements, input validation)

**Grounding Emphasis:**
- Section 3 (Interface Contracts): Security assumptions, input validation, auth requirements
- Section 8 (Known Issues): Known vulnerabilities, security concerns

---

### 7. WorkflowEngineer

**Domain:** GitHub Actions, CI/CD automation, pipeline optimization, build scripts

**Phase 1 Priority Standards:**
- **TaskManagementStandards.md** (COMPREHENSIVE): Git workflow, branch strategy, PR standards
- **TestingStandards.md** (TEST EXECUTION): Test categorization, coverage validation

**Phase 2 Architecture:**
- `.github/workflows/` structure
- `Scripts/` automation directory
- CI/CD pipeline architecture

**Phase 3 Domain Context:**
- **Workflow-specific documentation**: AI Sentinel prompts, automation patterns
- **Scripts README**: Automation script usage

**Grounding Emphasis:**
- Workflow patterns, automation strategies, test execution integration

---

### 8. BugInvestigator

**Domain:** Root cause analysis, diagnostic reporting, systematic debugging

**Phase 1 Priority Standards:**
- **CodingStandards.md** (ERROR HANDLING): Exception patterns, logging
- **TestingStandards.md** (REPRODUCTION): Test scenarios

**Phase 2 Architecture:**
- Root README system architecture
- Module hierarchy for affected components

**Phase 3 Domain Context:**
- **Affected Module README**: Section 5 (Known Pitfalls), Section 8 (Known Issues)

**Grounding Emphasis:**
- Section 5 (How to Work): Known gotchas, common mistakes
- Section 8 (Known Issues): Existing limitations, planned fixes

---

### 9. ArchitecturalAnalyst

**Domain:** System design decisions, architecture review, technical debt assessment

**Phase 1 Priority Standards:**
- **ALL STANDARDS** (COMPREHENSIVE): Complete understanding of coding, testing, documentation, workflow standards

**Phase 2 Architecture:**
- Root README: Complete project architecture
- Full module hierarchy: All major subsystems

**Phase 3 Domain Context:**
- **Multiple Module READMEs**: Section 2 (Architecture), Section 7 (Rationale)

**Grounding Emphasis:**
- Section 2 (Architecture): Design patterns across modules
- Section 7 (Rationale): Historical decisions, architectural choices

---

### 10. PromptEngineer

**Domain:** AI prompt optimization, agent definitions, skills/commands creation, `.claude/` structure

**Phase 1 Priority Standards:**
- **DocumentationStandards.md** (COMPREHENSIVE): Metadata, YAML frontmatter, self-contained knowledge
- **TaskManagementStandards.md** (MODERATE): Git workflow for prompt commits

**Phase 2 Architecture:**
- `.claude/` directory structure: `/agents/`, `/skills/`, `/commands/`
- Epic #291 specifications

**Phase 3 Domain Context:**
- **CLAUDE.md**: Orchestration patterns
- **Agent Definitions**: Multi-agent coordination patterns
- **AI Sentinel Prompts**: Code review prompt engineering

**Grounding Emphasis:**
- AI system configuration, orchestration, prompt engineering patterns

---

### 11. ComplianceOfficer

**Domain:** Pre-PR validation, standards verification, comprehensive quality gates

**Phase 1 Priority Standards:**
- **ALL STANDARDS** (COMPREHENSIVE MASTERY REQUIRED): No shortcuts—complete understanding of coding, testing, documentation, task management, diagramming standards

**Phase 2 Architecture:**
- Complete project architecture
- All affected module hierarchies

**Phase 3 Domain Context:**
- **All Relevant Module READMEs** for validation scope

**Grounding Emphasis:**
- Comprehensive understanding—validates all agent work against all standards

---

## 5. Skills Integration

### documentation-grounding Skill Reference

**Location:** `.claude/skills/documentation/documentation-grounding/SKILL.md`

**Purpose:** This guide documents the complete grounding protocols and methodology. The `documentation-grounding` skill provides the executable workflow implementation that agents invoke for systematic context loading.

**Skill Encapsulation:**

The skill encapsulates the grounding workflow through progressive loading architecture:

**Tier 1: Metadata Discovery (~100 tokens)**
```yaml
---
name: documentation-grounding
description: Systematic framework for loading project standards, module READMEs, and architectural patterns before agent work begins. Use when starting any agent engagement, switching between modules, or before modifying code or documentation.
---
```

**Discovery:** Agents scanning for grounding capability see YAML frontmatter describing when to use the skill (starting engagement, switching modules, before modifications).

**Tier 2: Instructions Loading (~2,800 tokens)**

Complete SKILL.md provides:
- 3-Phase Systematic Loading Workflow (this guide's Phase 1-3 executable format)
- Agent-Specific Grounding Patterns (tailored workflows for each of 11 agents)
- Progressive Loading Integration (context window optimization)
- Resources references (templates, examples, documentation)

**Tier 3: Resources Access (variable tokens, on-demand)**

Resources include:
- `resources/templates/standards-loading-checklist.md` - Phase 1 validation checklist
- `resources/templates/module-context-template.md` - Phase 3 structured analysis
- `resources/examples/backend-specialist-grounding.md` - Complete realistic workflow
- `resources/examples/test-engineer-grounding.md` - Testing-specific grounding
- `resources/documentation/grounding-optimization-guide.md` - Context window strategies

### Metadata-Driven Context Discovery

Skill metadata enables agents to discover grounding capability without loading full instructions:

**Agent Discovery Process:**
1. Agent receives context package from Claude
2. Reviews skill references: `documentation-grounding: Execute 3-phase grounding before work`
3. Loads skill metadata (YAML frontmatter) to confirm relevance
4. Description matches trigger: "Use when starting any agent engagement"
5. Agent loads full SKILL.md for complete workflow instructions

**Efficiency Benefit:** Agent discovers applicable skill in ~100 tokens vs. ~2,800 tokens for full instructions.

### Resource Bundling for Grounding Materials

Skill resources organize supplementary grounding content:

**Templates (resources/templates/):**
- **standards-loading-checklist.md**: Systematic Phase 1 validation
  - Comprehensive checklist covering all 5 core standards
  - Validation criteria for completion
  - Agent loads template when needing structured Phase 1 validation

**Examples (resources/examples/):**
- **backend-specialist-grounding.md**: Complete 3-phase workflow demonstration for backend work
  - Realistic scenario: Implementing UserService.GetUserById endpoint
  - Shows Phase 1 standards loading with backend emphasis
  - Demonstrates Phase 2 backend module hierarchy navigation
  - Details Phase 3 UserService README section-by-section analysis
  - Agent loads example when needing pattern demonstration

**Documentation (resources/documentation/):**
- **grounding-optimization-guide.md**: Context window optimization strategies
  - Selective loading based on task complexity
  - Token budget management
  - Progressive disclosure techniques
  - Agent loads documentation for complex scenarios or optimization needs

**Resource Loading Pattern:**
```
Agent executes documentation-grounding workflow:
1. Loads SKILL.md (~2,800 tokens) for 3-phase workflow
2. Executes Phase 1 from SKILL.md instructions (basic execution)
3. IF validation checklist needed → Loads standards-loading-checklist.md (~500 tokens)
4. IF realistic pattern demonstration helpful → Loads relevant example (~1,200 tokens)
5. IF context window optimization required → Loads optimization guide (~2,400 tokens)

Total context: 2,800 + selective resources (0-4,100 additional tokens)
```

### Dynamic Grounding Based on Task Context

Grounding depth adapts to task scope:

**Simple Task (Bug Fix, Minor Update):**
- Phase 1: CodingStandards.md + TestingStandards.md (core only)
- Phase 2: Root README (quick project orientation)
- Phase 3: Target module README Sections 2, 3, 5 (focused)
- Total grounding time: ~20 minutes
- Context loaded: ~15,000 tokens

**Complex Task (New Feature, Architectural Change):**
- Phase 1: All 5 standards (comprehensive)
- Phase 2: Root README + module hierarchy mapping
- Phase 3: Target module README (all 8 sections) + dependency module READMEs
- Total grounding time: ~50 minutes
- Context loaded: ~35,000 tokens

**Critical Task (Security Fix, Data Schema Change):**
- Phase 1: All 5 standards (exhaustive)
- Phase 2: Complete project architecture
- Phase 3: Multiple module READMEs (target + dependencies + dependents)
- Additional: Security-specific documentation, schema migration patterns
- Total grounding time: ~90 minutes
- Context loaded: ~50,000 tokens

**Skill Adaptation:**

The `documentation-grounding` skill provides guidance for task-based selective loading through resources:

```
SKILL.md includes:
- Standard 3-phase workflow (comprehensive grounding)
- References to optimization guide for selective loading

resources/documentation/grounding-optimization-guide.md includes:
- Simple task pattern: Core standards + focused module context
- Complex task pattern: Comprehensive standards + dependency mapping
- Critical task pattern: Exhaustive grounding + related subsystems

Agent adapts grounding depth based on:
- Task complexity (simple fix vs. new feature)
- Risk level (cosmetic change vs. security fix)
- Scope (single method vs. multi-module change)
```

### Agent Skill Reference Pattern

Agents include lightweight skill references in their definitions instead of embedding full grounding protocols:

**Agent Definition Reference (~20 tokens):**
```markdown
### documentation-grounding
**Purpose:** Systematic standards and module context loading
**Key Workflow:** Standards mastery → Project architecture → Domain-specific context
**Integration:** Execute 3-phase grounding before modifying any code or documentation
```

**vs. Embedded Grounding Protocol (~400 tokens):**
```markdown
## Documentation Grounding Protocol (Embedded - NOT RECOMMENDED)

Before starting any work, systematically load:

1. Phase 1: Standards Mastery
   - Load CodingStandards.md: Review naming conventions, DI patterns, async/await...
   - Load TestingStandards.md: Review AAA pattern, test categorization...
   - Load DocumentationStandards.md: Review 8-section template...
   [... 350 more tokens ...]
```

**Token Savings:** 95% reduction per agent (400 → 20 tokens) × 11 agents = ~4,180 tokens saved

**Ecosystem Efficiency:**
- All 11 agents reference skill: 11 × 20 = 220 tokens
- Skill SKILL.md: ~2,800 tokens (loaded when grounding needed)
- Skill resources: Variable (loaded selectively)
- Total ecosystem: 220 + 2,800 = 3,020 tokens for comprehensive grounding capability
- **vs. Embedded:** 11 × 400 = 4,400 tokens always loaded with no progressive disclosure

---

## 6. Optimization Strategies

### Context Window Management

Documentation grounding consumes significant context window capacity. Strategic optimization enables comprehensive grounding without exceeding token budgets.

#### Progressive Disclosure Approach

**Discovery Phase (Minimal Context):**
- Load only YAML frontmatter when scanning for applicable skills
- ~100 tokens per skill
- Agent identifies `documentation-grounding` relevance without loading full workflow

**Invocation Phase (Core Workflow):**
- Load SKILL.md when executing grounding
- ~2,800 tokens for 3-phase workflow instructions
- Agent completes basic grounding from SKILL.md alone

**Resource Phase (Selective Detail):**
- Load specific resources only when needed
- Templates: ~500 tokens each
- Examples: ~1,200 tokens each
- Documentation: ~2,400 tokens each
- Agent accesses supplementary content on-demand

**Total Context Range:**
- Minimal: ~100 tokens (discovery only)
- Typical: ~2,800 tokens (execution)
- Maximum: ~8,000 tokens (execution + resources)

**Efficiency Benefit:** Progressive loading prevents consuming ~8,000 tokens during discovery when only ~100 tokens needed for relevance assessment.

### Selective Loading Based on Task Scope

#### Simple Task Pattern (Bug Fix, Minor Update)

**Phase 1 Optimization:**
- CodingStandards.md: Focus on relevant sections (naming, DI, async if applicable)
- TestingStandards.md: AAA pattern, categorization only
- Skip DiagrammingStandards.md unless architecture changes
- **Time:** ~10 minutes
- **Context:** ~8,000 tokens

**Phase 2 Optimization:**
- Root README: Quick project orientation (skim)
- Skip complete module hierarchy mapping
- **Time:** ~3 minutes
- **Context:** ~2,000 tokens

**Phase 3 Optimization:**
- Target module README: Sections 2 (Architecture), 3 (Contract), 5 (Pitfalls) only
- Skip Section 7 (Rationale) unless change touches historical design decisions
- Skip Section 8 (Known Issues) unless bug-related
- **Time:** ~15 minutes
- **Context:** ~5,000 tokens

**Total Simple Task Grounding:**
- Time: ~28 minutes
- Context: ~15,000 tokens
- Trade-off: Faster grounding, sufficient for low-risk changes

#### Complex Task Pattern (New Feature, Architectural Change)

**Phase 1 Optimization:**
- All 5 standards: Comprehensive review
- **Time:** ~25 minutes
- **Context:** ~20,000 tokens

**Phase 2 Optimization:**
- Root README: Full review
- Module hierarchy: Complete mapping for affected subsystems
- Architectural diagrams: Review all relevant diagrams
- **Time:** ~15 minutes
- **Context:** ~8,000 tokens

**Phase 3 Optimization:**
- Target module README: All 8 sections
- Dependency module READMEs: Sections 2, 3, 6 (architecture, contracts, dependencies)
- **Time:** ~40 minutes
- **Context:** ~25,000 tokens

**Total Complex Task Grounding:**
- Time: ~80 minutes
- Context: ~53,000 tokens
- Trade-off: Comprehensive understanding, necessary for high-risk changes

#### Critical Task Pattern (Security Fix, Data Schema Change, Multi-Module Impact)

**Phase 1 Optimization:**
- All 5 standards: Exhaustive review with validation checklists
- Security-specific documentation if applicable
- **Time:** ~35 minutes
- **Context:** ~25,000 tokens

**Phase 2 Optimization:**
- Complete project architecture
- All subsystem hierarchies
- Integration point mapping
- **Time:** ~25 minutes
- **Context:** ~15,000 tokens

**Phase 3 Optimization:**
- Target module README: All 8 sections (exhaustive)
- Dependency module READMEs: All 8 sections
- Dependent module READMEs: Sections 2, 3, 6 (impact analysis)
- **Time:** ~60 minutes
- **Context:** ~40,000 tokens

**Total Critical Task Grounding:**
- Time: ~120 minutes
- Context: ~80,000 tokens
- Trade-off: Exhaustive understanding, essential for mission-critical changes

### Incremental Grounding for Iterative Work

When same agent works on related tasks across multiple engagements:

**Initial Engagement (Full Grounding):**
- Complete Phase 1-3 for comprehensive understanding
- Time: 60-120 minutes depending on task complexity
- Context: 40,000-80,000 tokens

**Subsequent Engagements (Incremental Refresh):**
- Phase 1: Skip if standards unchanged, quick refresh if updates
- Phase 2: Skip if project architecture unchanged
- Phase 3: Refresh only if module README updated or switching modules
- Time: 5-15 minutes (refresh validation)
- Context: 5,000-10,000 tokens (targeted refresh)

**Refresh Triggers:**
- Standards documents modified since last grounding
- Module README Last Updated date changed
- Switching to different module requiring new domain-specific context
- Significant time elapsed (>1 week) suggesting possible documentation drift

**Caution:** Stateless AI cannot rely on "memory" of prior grounding. Incremental refresh assumes recent comprehensive grounding documented in working directory artifacts for verification.

### Caching and Reuse Patterns

**Standards Caching (Session-Level):**
- Standards documents rarely change within single development session
- After Phase 1 grounding, standards content "cached" mentally for session
- Subsequent tasks in same session: Quick standards validation, not full re-read
- **Savings:** ~15-20 minutes per subsequent task
- **Risk:** Standards updates mid-session (low probability)

**Architecture Caching (Session-Level):**
- Project architecture changes infrequently
- After Phase 2 grounding, architecture context "cached" for session
- Subsequent tasks: Validate no architectural changes, skip full hierarchy mapping
- **Savings:** ~10-15 minutes per subsequent task
- **Risk:** Architectural changes during session (low probability for most tasks)

**Domain Context Refresh (Per-Task):**
- Module READMEs change frequently (maintenance triggers on code changes)
- Always refresh Phase 3 domain-specific context for new task
- Check Last Updated date on module README
- **No caching assumption:** Always validate current state

**Stateless Operation Caveat:**
- "Caching" in stateless AI context means workflow optimization within single engagement
- Agents cannot truly cache across engagements (no memory)
- Optimization relies on working directory artifacts documenting recent grounding completion

### Token Budget Optimization

**Measurement and Estimation:**

**Standards Documents:**
- CodingStandards.md: ~8,000 tokens
- TestingStandards.md: ~6,000 tokens
- DocumentationStandards.md: ~4,000 tokens
- TaskManagementStandards.md: ~3,000 tokens
- DiagrammingStandards.md: ~2,000 tokens
- **Total Phase 1:** ~23,000 tokens (comprehensive), ~15,000 tokens (selective)

**Project Architecture:**
- Root README: ~3,000 tokens
- Major module READMEs: ~2,000 tokens each
- **Total Phase 2:** ~10,000 tokens (comprehensive), ~5,000 tokens (selective)

**Module Context:**
- Module README (8 sections): ~5,000 tokens
- Dependency README: ~3,000 tokens each
- **Total Phase 3:** ~15,000 tokens (comprehensive), ~8,000 tokens (selective)

**Grounding Total:**
- Comprehensive: ~48,000 tokens
- Selective: ~28,000 tokens
- Minimal: ~15,000 tokens

**Budget Allocation Strategy:**

**200,000 Token Context Window:**
- Grounding: 40,000 tokens (20%)
- Agent definition: 2,000 tokens (1%)
- Working directory artifacts: 10,000 tokens (5%)
- Implementation workspace: 148,000 tokens (74%)

**Claude Code 100,000 Token Window:**
- Grounding: 25,000 tokens (25%) - Use selective loading
- Agent definition: 2,000 tokens (2%)
- Working directory artifacts: 8,000 tokens (8%)
- Implementation workspace: 65,000 tokens (65%)

**Optimization Recommendation:**
- Simple tasks: 15,000 token grounding (leaves 85,000 for implementation in 100k window)
- Complex tasks: 40,000 token grounding (use 200k window or distributed agent engagements)
- Critical tasks: 60,000+ token grounding (requires 200k window, may need multiple engagements)

---

## 7. Quality Validation

### Grounding Completeness Checks

Before proceeding with any code or documentation modifications, agents validate comprehensive grounding completion:

**Phase 1 Validation Checklist:**
- [ ] CodingStandards.md reviewed completely
  - [ ] Can explain DI patterns and service lifetimes
  - [ ] Understand async/await best practices
  - [ ] Know nullable reference type handling
  - [ ] Recognize SOLID principles application
- [ ] TestingStandards.md reviewed completely
  - [ ] Can structure tests using AAA pattern
  - [ ] Understand test categorization strategy
  - [ ] Know coverage expectations
- [ ] DocumentationStandards.md reviewed completely
  - [ ] Understand 8-section README template
  - [ ] Know Section 3 (Interface Contract) importance
  - [ ] Recognize linking strategy (parent, child, dependency)
- [ ] TaskManagementStandards.md reviewed completely
  - [ ] Can create properly named branches
  - [ ] Know conventional commit format
- [ ] DiagrammingStandards.md reviewed if applicable
  - [ ] Understand Mermaid conventions if architectural changes planned

**Phase 2 Validation Checklist:**
- [ ] Root README.md reviewed for project overview
- [ ] Module hierarchy relevant to task mapped and understood
- [ ] Dependency relationships identified through Section 6 analysis
- [ ] Integration points validated (API contracts, service boundaries)
- [ ] Architectural diagrams reviewed for system structure visualization
- [ ] Can explain how current task fits into overall architecture

**Phase 3 Validation Checklist:**
- [ ] Target module README.md identified and loaded
- [ ] Section 1 (Purpose & Responsibility) understood
- [ ] Section 2 (Architecture & Key Concepts) studied, diagrams reviewed
- [ ] Section 3 (Interface Contract & Assumptions) THOROUGHLY analyzed
  - [ ] All preconditions identified and understood
  - [ ] All postconditions and side effects noted
  - [ ] Error handling patterns and specific exceptions documented
  - [ ] Invariants and critical assumptions acknowledged
- [ ] Section 4 (Local Conventions & Constraints) noted
- [ ] Section 5 (How to Work With This Code) testing strategy identified
- [ ] Section 6 (Dependencies) mapped, dependency READMEs reviewed
- [ ] Section 7 (Rationale & Historical Context) design decisions understood
- [ ] Section 8 (Known Issues & TODOs) acknowledged

**Overall Grounding Validation:**
- [ ] Can explain task requirements in context of project standards
- [ ] Can identify which standards and patterns apply to current task
- [ ] Understand interface contracts agent will be modifying
- [ ] Aware of dependencies and integration points
- [ ] Know testing strategy and coverage expectations
- [ ] Recognize architectural patterns to maintain consistency

**Validation Failure Response:**
If any checklist item incomplete:
1. Identify knowledge gap
2. Return to relevant phase/section
3. Complete grounding for missing knowledge
4. Re-validate checklist
5. Do NOT proceed with modifications until validation complete

### Context Accuracy Validation

Grounding quality depends on documentation accuracy. Agents validate:

**Standards Version Currency:**
- [ ] Standards documents Last Updated date recent (within 6 months)
- [ ] No major framework version changes since standards updated (e.g., .NET 8→9, Angular 19→20)
- [ ] Standards reflect current project technology stack

**README Accuracy:**
- [ ] Module README Last Updated date reflects recent maintenance
- [ ] Section 3 (Interface Contract) matches actual code signatures and behavior
- [ ] Section 6 (Dependencies) links functional and dependencies exist in codebase
- [ ] Section 7 (Rationale) illuminates CURRENT state, not obsolete historical notes
- [ ] Section 8 (Known Issues) reflects current limitations, not resolved issues

**Pattern Validation:**
- [ ] Code patterns in repository match patterns documented in CodingStandards.md
- [ ] Test patterns match TestingStandards.md requirements (AAA, categorization)
- [ ] README files follow DocumentationStandards.md 8-section template

**Accuracy Failure Response:**
If documentation inaccurate or outdated:
1. Document discrepancy in working directory artifact
2. Alert Claude to documentation accuracy issue
3. Proceed with caution using actual code as source of truth
4. Flag documentation update needed (DocumentationMaintainer task)

### Integration Point Verification

Grounding enables integration point understanding. Agents verify:

**Dependency Understanding:**
- [ ] All Section 6 dependencies identified and their purpose understood
- [ ] Dependency READMEs reviewed for integration contracts
- [ ] Know which dependencies require mocking in tests
- [ ] Understand dependency injection registration for consumed services

**Interface Contract Comprehension:**
- [ ] Preconditions for all public methods in scope understood
- [ ] Postconditions and side effects identified
- [ ] Error handling patterns and specific exceptions documented
- [ ] Know behavioral contracts beyond method signatures

**Cross-Module Impact:**
- [ ] Identified modules that consume this module (dependents)
- [ ] Understand how changes affect dependent modules
- [ ] Know if interface changes constitute breaking changes
- [ ] Aware of API versioning implications if applicable

**Integration Verification Checklist:**
- [ ] Can explain how module integrates with dependencies
- [ ] Understand data flow across module boundaries
- [ ] Know authentication/authorization requirements if security-relevant
- [ ] Aware of transaction boundaries and consistency requirements

### Standards Compliance Confirmation

Grounding culminates in standards compliance confirmation:

**Pre-Modification Validation:**
- [ ] Work plan aligns with CodingStandards.md patterns
  - [ ] Will use constructor DI, not property injection or service locator
  - [ ] Will implement async methods with CancellationToken
  - [ ] Will follow nullable reference type conventions
  - [ ] Will maintain SOLID principles for testability
- [ ] Testing approach aligns with TestingStandards.md
  - [ ] Will structure tests using AAA pattern
  - [ ] Will apply appropriate categorization traits
  - [ ] Will achieve coverage expectations
- [ ] Documentation updates align with DocumentationStandards.md
  - [ ] Will update README Section 3 if interface changes
  - [ ] Will update README Section 5 if testing strategy changes
  - [ ] Will maintain linking integrity
  - [ ] Will update Last Updated date

**Compliance Deviations:**
- [ ] Any deviations from standards documented with rationale
- [ ] Deviations approved through architectural decision (Section 4 Local Conventions)
- [ ] No "silent" standards violations

**Compliance Reporting:**
After grounding completion, agents report compliance preparation:

```yaml
🎯 [AGENT] GROUNDING COMPLETION REPORT

Documentation Grounding Status: ✅ COMPLETE

Phase 1 Standards Loaded:
- CodingStandards.md: DI patterns, async/await, SOLID principles mastered
- TestingStandards.md: AAA pattern, categorization, coverage goals understood
- DocumentationStandards.md: 8-section template, Section 3 contract requirements clear

Phase 2 Architecture Context:
- Root README: Project overview and structure understood
- Backend module hierarchy: Mapped Controllers → Services → Repositories layers

Phase 3 Module Context:
- Code/Zarichney.Server/Services/UserService README.md analyzed
  - Section 2: Repository pattern usage understood
  - Section 3: Interface contracts (preconditions, postconditions, error handling) thoroughly analyzed
  - Section 5: Unit test strategy with mocked repository identified
  - Section 6: Dependencies on IUserRepository, IEmailValidator mapped

Standards Compliance Plan:
- Will use constructor DI for IUserRepository and IEmailValidator (CodingStandards.md)
- Will implement async with CancellationToken (CodingStandards.md async patterns)
- Will structure tests using AAA pattern with Category=Unit trait (TestingStandards.md)
- Will update UserService README Section 3 with new method contract (DocumentationStandards.md)

Integration Points Validated:
- IUserRepository contract understood from dependency README
- IEmailValidator contract understood from dependency README
- Dependent modules identified: UserController (will need integration test update)

Grounding Quality: HIGH - Comprehensive understanding achieved, ready for implementation
```

---

## Related Documentation

### Prerequisites
- [CodingStandards.md](../Standards/CodingStandards.md) - Phase 1 grounding foundation
- [TestingStandards.md](../Standards/TestingStandards.md) - Phase 1 grounding foundation
- [DocumentationStandards.md](../Standards/DocumentationStandards.md) - Phase 1 grounding foundation
- [TaskManagementStandards.md](../Standards/TaskManagementStandards.md) - Phase 1 grounding foundation
- [DiagrammingStandards.md](../Standards/DiagrammingStandards.md) - Phase 1 grounding foundation

### Integration Points
- [documentation-grounding skill](../../.claude/skills/documentation/documentation-grounding/SKILL.md) - Skill reference implementation
- [SkillsDevelopmentGuide.md](./SkillsDevelopmentGuide.md) - Skills integration patterns

### Orchestration Context
- [CLAUDE.md](../../CLAUDE.md) - Orchestration integration (see Section 2 "Documentation Grounding Protocols")
- [/.claude/agents/](../../.claude/agents/) - All 11 agent files for pattern reference

---

**Guide Status:** ✅ **COMPLETE**
**Word Count:** ~8,600 words
**Validation:** All 7 sections comprehensive, 11 agent-specific patterns specified, skills integration explained, cross-references functional, enables autonomous grounding without external clarification

**Success Test:** Any agent can perform systematic 3-phase grounding using this guide without asking clarification questions ✅
