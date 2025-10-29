# Standards Loading Checklist

Use this checklist to ensure systematic standards mastery before beginning work.

## Phase 1: Standards Mastery

### CodingStandards.md ✅
**Location:** `/Docs/Standards/CodingStandards.md`

**Review Checklist:**
- [ ] **Naming Conventions** understood
  - PascalCase for classes, interfaces, methods, properties
  - camelCase for local variables and parameters
  - Interface prefix `I` convention (e.g., `IFileService`)
  - Configuration suffix `Config` convention

- [ ] **Modern C# Features** clear
  - File-scoped namespaces pattern
  - Primary constructors usage
  - Record types for DTOs and immutable data
  - Nullable reference types (`?`) handling

- [ ] **Dependency Injection Patterns** known
  - Constructor injection for all dependencies
  - Interface definitions for services
  - Service lifetime implications (Singleton, Scoped, Transient)
  - Avoid Service Locator anti-pattern

- [ ] **SOLID Principles** for testability understood
  - Single Responsibility Principle (SRP)
  - Dependency Inversion Principle (DIP)
  - Interface Segregation Principle (ISP)

- [ ] **Asynchronous Programming** standards clear
  - async/await for all I/O operations
  - CancellationToken usage patterns
  - Avoid blocking calls (.Result, .Wait())

- [ ] **Error Handling** patterns known
  - try/catch around I/O and external calls
  - Structured logging with ILogger<T>
  - Polly retry policies for transient errors
  - Specific exception types (ArgumentException, InvalidOperationException, etc.)

- [ ] **Code Organization** patterns understood
  - File structure conventions
  - Configuration management patterns
  - Humble Object pattern for testability

---

### TestingStandards.md ✅
**Location:** `/Docs/Standards/TestingStandards.md`

**Review Checklist:**
- [ ] **Test Framework Tooling** understood
  - xUnit as test framework
  - FluentAssertions for assertions (mandatory)
  - Moq for mocking (mandatory)
  - AutoFixture for test data generation

- [ ] **Core Testing Philosophy** clear
  - Test behavior, not implementation
  - AAA pattern (Arrange-Act-Assert)
  - Complete isolation for unit tests
  - Determinism and repeatability requirements

- [ ] **Test Categorization** with Traits known
  - Mandatory base traits: "Unit", "Integration"
  - Dependency traits: "Database", "ExternalHttp:[ServiceName]"
  - Mutability traits: "ReadOnly", "DataMutating"

- [ ] **Unit Test Standards** understood
  - Mock ALL dependencies using Moq
  - Use FluentAssertions with reason messages
  - Leverage AutoFixture and custom Builders
  - Comprehensive coverage of non-trivial logic

- [ ] **Integration Test Standards** clear
  - CustomWebApplicationFactory usage
  - DatabaseFixture for database interactions
  - Refit client interfaces for API interaction
  - WireMock.Net for external HTTP virtualization

- [ ] **Test Structure & Naming** conventions known
  - Class naming: `[SystemUnderTest]Tests.cs`
  - Method naming: `[MethodName]_[Scenario]_[ExpectedOutcome]`
  - Folder structure mirrors production code

---

### DocumentationStandards.md ✅
**Location:** `/Docs/Standards/DocumentationStandards.md`

**Review Checklist:**
- [ ] **Core Philosophy** understood
  - Target audience: Stateless AI
  - Self-contained knowledge philosophy
  - Value over volume principle
  - Maintainability and pruning requirements

- [ ] **README Template Structure** (8 sections) clear
  - Section 1: Purpose & Responsibility
  - Section 2: Architecture & Key Concepts
  - Section 3: Interface Contract & Assumptions (CRITICAL)
  - Section 4: Local Conventions & Constraints
  - Section 5: How to Work With This Code
  - Section 6: Dependencies
  - Section 7: Rationale & Key Historical Context
  - Section 8: Known Issues & TODOs

- [ ] **Linking Strategy** (mandatory) understood
  - Parent link in every README (except root)
  - Child links to immediate subdirectories
  - Dependency/dependent links using relative paths
  - No `[cite]` tags for internal modules

- [ ] **Embedded Diagram Integration** clear
  - Mermaid diagrams in Section 2 (Architecture)
  - Diagram standards from DiagrammingStandards.md
  - Visual clarity supplementing textual descriptions

- [ ] **Maintenance Triggers** known
  - Update README when code changes affect documented contracts
  - Prune outdated historical context
  - Update `Last Updated` date
  - No explanatory annotations in content

---

### TaskManagementStandards.md ✅
**Location:** `/Docs/Standards/TaskManagementStandards.md`

**Review Checklist:**
- [ ] **Branch Naming Conventions** understood
  - `feature/issue-XXX-description` for features
  - `test/issue-XXX-description` for test coverage
  - `fix/issue-XXX-description` for bug fixes
  - Epic branch patterns for multi-issue work

- [ ] **Conventional Commit Format** clear
  - `<type>: <description> (#ISSUE_ID)`
  - Types: feat, fix, test, docs, refactor, chore
  - Concise, present-tense descriptions
  - Issue number reference required

- [ ] **PR Creation Standards** known
  - Comprehensive PR descriptions
  - Base branch selection (develop vs main)
  - AI Sentinel automatic activation logic
  - Closing issue references

- [ ] **Git Workflow Patterns** understood
  - feature→epic→develop→main progression
  - Branch-aware AI analysis depth
  - Quality gate requirements by target branch

---

### DiagrammingStandards.md (if applicable) ✅
**Location:** `/Docs/Standards/DiagrammingStandards.md`

**Review Checklist (when working with architecture visualization):**
- [ ] **Mermaid Diagram Conventions** understood
  - Diagram types and appropriate usage
  - Style consistency requirements
  - Naming and labeling patterns

- [ ] **Embedding vs External Files** clear
  - When to embed in README.md
  - When to use separate `.mmd` files
  - Linking strategy for external diagrams

- [ ] **Diagram Maintenance Requirements** known
  - Update diagrams with code changes
  - Prune outdated diagram elements
  - Validation of accuracy

---

## Phase 2: Project Architecture Context

### Root README Analysis ✅
**Location:** `/README.md`

- [ ] Project overview and purpose understood
- [ ] Technology stack summary reviewed
- [ ] Directory structure navigation mapped
- [ ] Key architectural decisions noted
- [ ] Development setup requirements clear
- [ ] Links to major module READMEs identified

### Module Hierarchy Mapping ✅
- [ ] Relevant modules to current task identified
- [ ] Dependency relationships between modules mapped
- [ ] Interface boundaries and contracts located
- [ ] Architectural diagrams reviewed
- [ ] Integration patterns understood

---

## Phase 3: Domain-Specific Context

### Module README Deep-Dive ✅
**Location:** `[specific module]/README.md`

- [ ] **Section 1 (Purpose & Responsibility)** reviewed
  - Module's functional role understood
  - Separation rationale clear
  - Child modules identified

- [ ] **Section 2 (Architecture & Key Concepts)** analyzed
  - Internal design patterns understood
  - Key components and data structures mapped
  - Embedded diagrams reviewed
  - Workflow sequences clear

- [ ] **Section 3 (Interface Contract & Assumptions)** CRITICAL ANALYSIS
  - Preconditions for inputs documented
  - Postconditions for outputs understood
  - Error handling patterns noted
  - Invariants and assumptions identified
  - Behavioral contracts beyond signatures clear

- [ ] **Section 4 (Local Conventions & Constraints)** understood
  - Module-specific deviations from global standards noted
  - Configuration requirements identified
  - Environmental dependencies clear
  - Testing setup specifics documented

- [ ] **Section 5 (How to Work With This Code)** reviewed
  - Setup steps unique to module clear
  - Module-specific testing strategy understood
  - Key test scenarios identified
  - Test data considerations noted
  - Module-specific commands known
  - Known pitfalls and gotchas documented

- [ ] **Section 6 (Dependencies)** mapped
  - Internal modules consumed identified (with README links)
  - Internal consumers of this module identified
  - External libraries and services noted
  - Testing implications (mocking, virtualization) understood

- [ ] **Section 7 (Rationale & Key Historical Context)** understood
  - Design decision explanations reviewed
  - Non-obvious choices documented
  - Historical illumination of current state clear

- [ ] **Section 8 (Known Issues & TODOs)** acknowledged
  - Current limitations noted
  - Planned work documented
  - Integration concerns identified

---

## Grounding Completion Validation

**All checkboxes marked?** ✅ Grounding complete - proceed with implementation

**Missing checkboxes?** ❌ Complete grounding before modifications

**Questions or ambiguities?** ⚠️ Escalate to Claude for clarification or additional context

---

**Usage Notes:**
- Copy this checklist for each agent engagement
- Mark checkboxes systematically as you load each document
- Focus on sections most relevant to your agent role and current task
- Don't skip phases - comprehensive grounding prevents rework
- Report grounding completion in your deliverable using agent completion report format

---

**Template Version:** 1.0.0
**Last Updated:** 2025-10-25
**Skill:** documentation-grounding
