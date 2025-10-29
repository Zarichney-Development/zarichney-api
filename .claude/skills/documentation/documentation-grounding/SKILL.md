---
name: documentation-grounding
description: Systematic framework for loading project standards, module READMEs, and architectural patterns before agent work begins. Use when starting any agent engagement, switching between modules, or before modifying code or documentation.
---

# Documentation Grounding Skill

**Version:** 1.0.0
**Category:** Documentation
**Mandatory For:** ALL agents (stateless operation support)

---

## PURPOSE

This skill provides a systematic framework for loading comprehensive project context before beginning any agent work, ensuring stateless AI agents operate with complete understanding of standards, architectural patterns, and module-specific conventions.

### Core Mission
Support stateless AI operation by establishing mandatory context loading protocols that transform context-blind agents into fully-informed contributors who understand project standards, architectural decisions, and domain-specific patterns before making any modifications.

### Why This Matters
AI agents operate statelessly with no inherent memory of project structure, coding standards, or architectural decisions. Without systematic context loading, agents:
- Violate established coding standards unknowingly
- Break documented interface contracts
- Ignore architectural patterns and conventions
- Create inconsistencies across module boundaries
- Introduce regressions through incomplete understanding

Systematic documentation grounding transforms every agent engagement into an informed contribution aligned with project standards and architectural coherence.

### Mandatory Application
- **Universal Requirement**: Every agent, every engagement, before any code or documentation modifications
- **Context Package Integration**: Claude includes grounding checklist in all context packages
- **Progressive Loading**: Standards mastery → project architecture → domain-specific context
- **Quality Gate**: No modifications without completed grounding validation

---

## WHEN TO USE

This skill applies in these MANDATORY scenarios:

### 1. At Start of Every Agent Engagement (REQUIRED)
**Trigger:** Agent receives context package from Claude and is about to begin work
**Action:** Execute 3-Phase Systematic Loading Workflow to establish comprehensive project context
**Rationale:** Ensures agents understand standards, architectural patterns, and module conventions before modifications

### 2. After Receiving Context Package from Claude (MANDATORY)
**Trigger:** Claude delegates task with context package specifying relevant standards and module context
**Action:** Load specified standards documents and module READMEs in priority order
**Rationale:** Context packages identify which standards are relevant; grounding ensures comprehensive understanding

### 3. Before Modifying Any Code or Documentation (REQUIRED)
**Trigger:** Agent is about to edit source files, tests, or documentation
**Action:** Validate grounding completion for affected modules and relevant standards
**Rationale:** Prevents standards violations, contract breakages, and architectural inconsistencies

### 4. When Switching Between Modules or Domains (REQUIRED)
**Trigger:** Agent work spans multiple modules or transitions from one domain to another
**Action:** Execute domain-specific context loading for each new module
**Rationale:** Each module has unique conventions, interface contracts, and historical context requiring fresh grounding

---

## 3-PHASE SYSTEMATIC LOADING WORKFLOW

### Phase 1: Standards Mastery (Mandatory for All Agents)

Load relevant project-wide standards documents to understand coding conventions, testing requirements, documentation structure, and workflow expectations.

#### Universal Standards (All Agents)
**Location:** `/Docs/Standards/`

**1. CodingStandards.md** - Production code requirements
- Naming conventions (PascalCase, camelCase, interface prefixes)
- Modern C# features (file-scoped namespaces, primary constructors, records)
- Dependency injection patterns and service lifetimes
- SOLID principles for testability
- Asynchronous programming standards (async/await, CancellationToken)
- Error handling and logging patterns
- Null handling with nullable reference types

**2. TestingStandards.md** - Test quality requirements
- Test framework tooling (xUnit, FluentAssertions, Moq)
- Unit vs integration test principles
- AAA pattern (Arrange-Act-Assert)
- Test categorization with Traits
- Coverage goals and quality metrics
- Test isolation and determinism requirements

**3. DocumentationStandards.md** - README structure and cross-references
- Per-directory README.md mandate
- Self-contained knowledge philosophy
- 8-section structure template
- Linking strategy (parent, child, dependencies)
- Maintenance triggers and pruning requirements
- Embedded diagram integration

**4. TaskManagementStandards.md** - Git workflow, branching, commits
- Branch naming conventions (feature/, test/, fix/)
- Conventional commit format (type: description)
- PR creation and description requirements
- Git workflow patterns
- Issue tracking integration

#### Domain-Specific Standards (As Relevant)

**5. DiagrammingStandards.md** (if working with architecture visualization)
- Mermaid diagram conventions
- Diagram types and appropriate usage
- Embedding vs external file storage
- Diagram maintenance requirements

#### Standards Loading Checklist
- [ ] CodingStandards.md reviewed and key patterns understood
- [ ] TestingStandards.md reviewed for test quality requirements
- [ ] DocumentationStandards.md reviewed for README structure
- [ ] TaskManagementStandards.md reviewed for Git workflow
- [ ] DiagrammingStandards.md reviewed if architectural changes planned
- [ ] Domain-specific standards loaded as relevant to task

**Resource:** See `resources/templates/standards-loading-checklist.md` for complete checklist

---

### Phase 2: Project Architecture Context

Understand overall project structure, navigation hierarchy, and cross-module dependencies.

#### Root README Discovery
**Location:** `/README.md` (project root)
- Project overview and purpose
- Technology stack summary
- Directory structure navigation
- Key architectural decisions
- Development setup requirements
- Links to all major module READMEs

#### Module Hierarchy Mapping
**Process:**
1. Review root README to understand module organization
2. Identify modules relevant to current task
3. Map dependency relationships between modules
4. Understand interface boundaries and contracts
5. Locate architectural diagrams illustrating system structure

#### Integration Points Understanding
- API contract definitions and versioning
- Service layer boundaries
- Database schema relationships
- External dependency integration patterns
- Authentication and authorization flows

#### Architecture Loading Checklist
- [ ] Root README.md reviewed for project overview
- [ ] Module hierarchy understood and mapped
- [ ] Dependency relationships between relevant modules identified
- [ ] Interface contracts and boundaries located
- [ ] Architectural diagrams reviewed for system structure understanding

**Resource:** See `resources/templates/module-context-template.md` for structured approach

---

### Phase 3: Domain-Specific Context

Deep-dive into specific module context relevant to current task.

#### Module README Analysis
**Location:** Module-specific `README.md` files throughout codebase

**Section-by-Section Analysis:**

**Section 1: Purpose & Responsibility**
- Module's functional role and scope
- Why it exists as separate unit
- Child modules and their relationships

**Section 2: Architecture & Key Concepts**
- Internal design patterns
- Key components and data structures
- Embedded Mermaid diagrams visualizing architecture
- Workflow sequences and data flows

**Section 3: Interface Contract & Assumptions** (CRITICAL)
- Preconditions required for inputs
- Postconditions expected from outputs
- Error handling patterns and specific exceptions
- Invariants and critical assumptions
- Behavioral contracts beyond method signatures

**Section 4: Local Conventions & Constraints**
- Module-specific deviations from global standards
- Configuration requirements
- Environmental dependencies
- Testing setup requirements specific to module

**Section 5: How to Work With This Code**
- Setup steps unique to module
- Module-specific testing strategy
- Key test scenarios requiring coverage
- Test data considerations
- Commands to run module-specific tests
- Known pitfalls and gotchas

**Section 6: Dependencies**
- Internal modules consumed (with README links)
- Internal modules that consume this module (with README links)
- External libraries and services
- Testing implications (mocking requirements, virtualization needs)

**Section 7: Rationale & Key Historical Context**
- Design decision explanations
- Non-obvious architectural choices
- Historical notes illuminating current state

**Section 8: Known Issues & TODOs**
- Current limitations specific to module
- Planned work and technical debt
- Integration concerns

#### Domain-Specific Loading Checklist
- [ ] Relevant module README.md identified and loaded
- [ ] Section 3 (Interface Contract) thoroughly analyzed for behavioral expectations
- [ ] Local conventions and constraints understood
- [ ] Module-specific testing strategy and scenarios identified
- [ ] Dependencies mapped with links to other module READMEs reviewed
- [ ] Historical context informing current design decisions understood
- [ ] Known issues and limitations acknowledged

**Resource:** See `resources/examples/` for agent-specific grounding demonstrations

---

## AGENT-SPECIFIC GROUNDING PATTERNS

Different agents have different context priorities based on their domain expertise and responsibilities.

### 1. CodeChanger
**Focus:** CodingStandards.md mastery, module code patterns, interface contracts
**Priority Loading:**
- Phase 1: CodingStandards.md (comprehensive), TestingStandards.md (design for testability), TaskManagementStandards.md
- Phase 2: Root README, relevant backend/frontend module hierarchy
- Phase 3: Target module README (Sections 2, 3, 5, 6 critical)

**Example:** See `resources/examples/code-changer-grounding.md`

### 2. TestEngineer
**Focus:** TestingStandards.md mastery, test project structure, coverage requirements
**Priority Loading:**
- Phase 1: TestingStandards.md (comprehensive), CodingStandards.md (SUT understanding)
- Phase 2: Test project structure (`Zarichney.Server.Tests/TechnicalDesignDocument.md`)
- Phase 3: Module under test README (Section 5 testing strategy, Section 3 interface contracts)

**Example:** See `resources/examples/test-engineer-grounding.md`

### 3. DocumentationMaintainer
**Focus:** DocumentationStandards.md mastery, README hierarchy, cross-reference validation
**Priority Loading:**
- Phase 1: DocumentationStandards.md (comprehensive), DiagrammingStandards.md
- Phase 2: Root README, full module hierarchy navigation
- Phase 3: Target module README (all 8 sections), parent/child linking validation

**Example:** See `resources/examples/documentation-maintainer-grounding.md`

### 4. BackendSpecialist
**Focus:** Backend module architecture, API patterns, database schemas, service layer design
**Priority Loading:**
- Phase 1: CodingStandards.md (DI, async patterns), TestingStandards.md
- Phase 2: Backend module hierarchy (`Code/Zarichney.Server/`)
- Phase 3: API controller README, service layer README, database schema documentation

**Grounding Emphasis:** Section 3 (Interface Contracts), Section 6 (Dependencies)

### 5. FrontendSpecialist
**Focus:** Frontend module architecture, component patterns, state management, API integration
**Priority Loading:**
- Phase 1: CodingStandards.md (applicable patterns), DocumentationStandards.md
- Phase 2: Frontend module hierarchy (`Code/Zarichney.Website/`)
- Phase 3: Component README, state management patterns, API client integration

**Grounding Emphasis:** Section 2 (Architecture), Section 5 (Local Conventions)

### 6. SecurityAuditor
**Focus:** Security standards, vulnerability patterns, compliance requirements, authentication flows
**Priority Loading:**
- Phase 1: CodingStandards.md (security patterns), TestingStandards.md (security testing)
- Phase 2: Authentication/authorization module architecture
- Phase 3: Security-critical module READMEs (auth services, API controllers, payment handlers)

**Grounding Emphasis:** Section 3 (Interface Contracts - security assumptions), Section 8 (Known Issues)

### 7. WorkflowEngineer
**Focus:** CI/CD workflows, GitHub Actions, automation patterns, build configurations
**Priority Loading:**
- Phase 1: TaskManagementStandards.md, TestingStandards.md (test execution)
- Phase 2: `.github/workflows/` structure, `Scripts/` automation
- Phase 3: Workflow-specific documentation, AI Sentinel prompts

**Grounding Emphasis:** Automation patterns, test execution strategies

### 8. BugInvestigator
**Focus:** Error patterns, diagnostic approaches, system behavior, module interactions
**Priority Loading:**
- Phase 1: CodingStandards.md (error handling), TestingStandards.md
- Phase 2: Root README system architecture
- Phase 3: Module READMEs for affected components (Sections 3, 5, 8)

**Grounding Emphasis:** Section 5 (Known Pitfalls), Section 8 (Known Issues)

### 9. ArchitecturalAnalyst
**Focus:** System design patterns, architectural decisions, structural assessment, technical debt
**Priority Loading:**
- Phase 1: All standards (comprehensive understanding)
- Phase 2: Root README, complete module hierarchy
- Phase 3: Multiple module READMEs (Section 2 Architecture, Section 7 Rationale)

**Grounding Emphasis:** Section 2 (Architecture), Section 7 (Historical Context)

### 10. PromptEngineer
**Focus:** AI prompt standards, agent definition patterns, `.claude/` structure, orchestration
**Priority Loading:**
- Phase 1: DocumentationStandards.md, TaskManagementStandards.md
- Phase 2: `.claude/` directory structure (agents, skills, commands)
- Phase 3: `CLAUDE.md` orchestration, individual agent definitions, AI Sentinel prompts

**Grounding Emphasis:** AI system configuration patterns, multi-agent coordination

### 11. ComplianceOfficer
**Focus:** ALL standards mastery, comprehensive validation, quality gate enforcement
**Priority Loading:**
- Phase 1: ALL standards documents (comprehensive mastery required)
- Phase 2: Complete project architecture
- Phase 3: All relevant module READMEs for validation scope

**Grounding Emphasis:** Comprehensive understanding - no shortcuts

---

## PROGRESSIVE LOADING INTEGRATION

### Metadata Discovery (~100 tokens)
**YAML frontmatter loaded at Claude Code startup:**
```yaml
---
name: documentation-grounding
description: Systematic framework for loading project standards...
---
```

**Purpose:** Enables skill discovery when agents need context loading
**Efficiency:** 98% token savings vs. loading full instructions

### Instructions Loading (2,000-3,000 tokens)
**SKILL.md body content loaded when grounding needed:**
- 3-Phase Systematic Loading Workflow
- Agent-Specific Grounding Patterns
- Integration with Orchestration
- Quality Gates and Validation

**Purpose:** Provides comprehensive grounding methodology
**Efficiency:** 85% savings vs. embedding in agent definitions

### Resources Access (variable tokens)
**On-demand loading of specific resources:**
- Templates: Standards loading checklist, module context template
- Examples: Agent-specific grounding demonstrations
- Documentation: Optimization guides, selective loading patterns

**Purpose:** Detailed guidance accessed only when needed
**Efficiency:** 60-80% savings through selective loading

### Context Window Optimization
**Total skill loading scenarios:**
- Minimal: ~100 tokens (frontmatter only for discovery)
- Typical: ~2,500 tokens (instructions for execution)
- Maximum: ~6,000 tokens (all resources for comprehensive guidance)
- Still 50-70% savings vs. embedding grounding protocols in all 11 agent definitions

---

## RESOURCES

This skill includes comprehensive resources for effective implementation:

### Templates (Ready-to-Use Formats)
**standards-loading-checklist.md** - Systematic checklist for Phase 1 standards mastery
- Comprehensive checklist covering all 5 core standards documents
- Validation criteria for completion
- Context optimization notes

**module-context-template.md** - Structured approach for Phase 3 domain-specific analysis
- Module README section-by-section analysis framework
- Interface contract discovery format
- Local convention documentation
- Integration point mapping

**Location:** `resources/templates/`
**Usage:** Copy template, execute systematic loading, validate completion

### Examples (Reference Implementations)
**backend-specialist-grounding.md** - Complete grounding workflow for BackendSpecialist
- Shows Phase 1-3 execution with backend-specific focus
- API module, service layer, database schema emphasis
- Demonstrates how grounding improves API work quality

**test-engineer-grounding.md** - Testing-specific grounding pattern
- TestingStandards.md mastery demonstration
- Test project structure understanding
- Coverage requirements integration
- Shows how grounding improves test quality

**documentation-maintainer-grounding.md** - Documentation grounding pattern
- DocumentationStandards.md mastery approach
- README hierarchy navigation
- Cross-reference validation technique
- Shows how grounding improves documentation quality

**Location:** `resources/examples/`
**Usage:** Review examples for realistic scenarios showing complete 3-phase workflow

### Documentation (Deep Dives)
**grounding-optimization-guide.md** - Context window optimization strategies
- Selective loading based on task type
- Performance best practices
- Token budget management
- When to use full vs. partial grounding

**selective-loading-patterns.md** - Task-based grounding strategies
- Quick reference patterns for common tasks
- Emergency grounding shortcuts
- Optimization for different agent types
- Progressive disclosure techniques

**Location:** `resources/documentation/`
**Usage:** Understand optimization strategies, troubleshoot context window issues

---

## INTEGRATION WITH ORCHESTRATION

### Claude's Context Package Integration
When delegating to agents, Claude includes grounding requirements:

```yaml
CORE_ISSUE: "[Specific technical problem]"
TARGET_FILES: "[Files requiring modification]"

Standards Context (MANDATORY GROUNDING):
- CodingStandards.md: [Specific sections relevant to task]
- TestingStandards.md: [Coverage requirements]
- DocumentationStandards.md: [README update requirements]

Module Context (MANDATORY GROUNDING):
- [Module path]/README.md: Focus on Sections 3 (Interface Contract), 5 (How to Work)
- Dependencies: Review [dependency module]/README.md for integration understanding

Grounding Validation:
- [ ] Phase 1: Standards mastery completed
- [ ] Phase 2: Project architecture understood
- [ ] Phase 3: Domain-specific context loaded
```

### Agent Grounding Compliance Reporting
Agents acknowledge grounding completion in deliverables:

```yaml
🎯 [AGENT] COMPLETION REPORT

Documentation Grounding Status: ✅ COMPLETE
- Phase 1 Standards Loaded: CodingStandards.md, TestingStandards.md, DocumentationStandards.md
- Phase 2 Architecture Context: Root README, [domain] module hierarchy
- Phase 3 Module Context: [specific module]/README.md (Sections 2, 3, 5, 6 analyzed)

Standards Compliance:
- Coding patterns aligned with CodingStandards.md Section [X]
- Tests follow TestingStandards.md requirements
- Documentation updated per DocumentationStandards.md template
```

### Working Directory Coordination Integration
Documentation grounding complements working-directory-coordination:
- **Grounding:** Loads project standards and architectural context
- **Coordination:** Manages team communication and artifact sharing
- **Together:** Ensures agents have both technical context AND team awareness

### Quality Gate Integration
Grounding protocols integrate with quality assurance:
- **AI Sentinels:** Code review informed by standards compliance from grounding
- **ComplianceOfficer:** Pre-PR validation leveraging comprehensive standards mastery
- **TestEngineer:** Test strategy informed by TestingStandards.md grounding
- **DocumentationMaintainer:** README updates following DocumentationStandards.md

---

## SUCCESS METRICS

### Grounding Effectiveness
- **100% Compliance**: All agents complete 3-phase grounding before modifications
- **Zero Standards Violations**: No coding standard, testing, or documentation violations from lack of context
- **Architectural Coherence**: All changes align with documented patterns and interface contracts
- **Reduced Rework**: Fewer corrections needed due to insufficient context understanding

### Agent Performance Quality
- **Context-Informed Decisions**: Agent modifications reflect understanding of architectural rationale
- **Standards Alignment**: Code, tests, and documentation consistently follow project conventions
- **Pattern Consistency**: New implementations match established patterns from grounding
- **Contract Compliance**: Interface changes respect documented preconditions and postconditions

### Orchestration Efficiency
- **Faster Ramp-Up**: Agents productive immediately with comprehensive context
- **Reduced Oversight**: Claude spends less time correcting context-blind mistakes
- **Better Coordination**: Shared grounding foundation enables effective multi-agent collaboration
- **Higher Quality Deliverables**: Grounding ensures all work meets project standards

---

**Skill Status:** ✅ **OPERATIONAL**
**Adoption:** Mandatory for all 11 agents
**Context Savings:** ~400 lines across agents (~3,200 tokens total)
**Progressive Loading:** frontmatter (~100 tokens) → SKILL.md (~2,800 tokens) → resources (on-demand)
