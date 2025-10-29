# Agent Design Principles

**Purpose:** Comprehensive prompt engineering best practices for creating effective agent definitions
**Target Audience:** PromptEngineer creating new agents or refactoring existing definitions
**Version:** 1.0.0

---

## INTRODUCTION - AGENT AS SPECIALIZED AI PERSONA

### What Makes Effective Agent Definitions

An agent definition is a specialized AI persona prompt that establishes:
- **Clear identity and role** within the multi-agent team
- **Unambiguous authority boundaries** preventing coordination conflicts
- **Efficient context architecture** through progressive loading design
- **Seamless team integration** with explicit coordination patterns

The difference between a good agent and a great agent lies in prompt engineering discipline: focused scope, surgical precision in authority definition, and ruthless context optimization.

### Prompt Engineering Fundamentals for Agent Creation

**Core Principle:** An agent definition is NOT documentation - it's an executable prompt that activates specific AI behaviors within a multi-agent coordination framework.

**Design Philosophy:**
1. **Identity-First:** The first 50 lines must establish WHO the agent is and WHAT they can modify
2. **Progressive Disclosure:** Load only what's needed for each activation scenario
3. **Skill Extraction:** Shared patterns belong in skills, unique identity stays in agent
4. **Team Awareness:** Every agent understands their position in the 12-agent ecosystem

### Zarichney-API 12-Agent Team Context

**Current Team Composition:**
- **4 Primary File-Editing Agents:** CodeChanger, TestEngineer, DocumentationMaintainer, PromptEngineer (exclusive file authority)
- **5 Specialist Agents:** BackendSpecialist, FrontendSpecialist, SecurityAuditor, WorkflowEngineer, BugInvestigator (flexible authority based on intent)
- **3 Advisory Agents:** ComplianceOfficer, ArchitecturalAnalyst (working directory only, zero file modifications)
- **Orchestrator:** Claude (Codebase Manager - coordinates all agents, never implements)

**Design Constraint:** New agents must integrate cleanly without authority conflicts or coordination ambiguity.

---

## CORE DESIGN PRINCIPLES

### Principle 1: Single Responsibility Focus

**Concept:** Each agent has ONE clear primary purpose that can be stated in a single sentence.

**Why This Matters:**
- Multiple responsibilities create authority boundary ambiguity
- Coordination becomes complex when agents have overlapping scopes
- Token efficiency suffers when agents try to do everything
- Team members don't know when to engage multi-purpose agents

**From TestEngineer Example:**
> **Primary Mission:** Create comprehensive test coverage for zarichney-api project

Not "create tests AND validate code quality AND review architectural patterns" - that's 3+ agents.

**From BackendSpecialist Example:**
> **Primary Mission:** Provide .NET 8/C# expertise through analysis OR direct implementation

Single focus: backend domain expertise. Dual-mode (query vs. command) supports HOW, not multiple WHATs.

**Anti-Pattern:**
```markdown
# MultiTaskAgent
**Primary Mission:** Create tests, write documentation, implement features, review code quality, and provide architectural guidance

[This agent will conflict with TestEngineer, DocumentationMaintainer, CodeChanger, ComplianceOfficer, and ArchitecturalAnalyst - coordination nightmare]
```

**Correct Pattern:**
```markdown
# TestEngineer
**Primary Mission:** Create comprehensive test coverage
[Clear, focused, exclusive responsibility]

# DocumentationMaintainer
**Primary Mission:** Maintain all project documentation
[Separate agent, separate purpose, zero overlap]
```

**Validation Question:** "Can I state this agent's purpose in ONE sentence without using 'and'?"

---

### Principle 2: Authority Clarity

**Concept:** Exact file patterns prevent coordination conflicts through unambiguous ownership.

**Why This Matters:**
- Multiple agents modifying the same files = merge conflicts and coordination overhead
- Authority ambiguity causes agents to hesitate or overstep boundaries
- Claude needs clear rules to delegate work without orchestration failures

**File Pattern Precision:**

**TestEngineer Authority (Primary Agent):**
```yaml
EXCLUSIVE_AUTHORITY:
  - "**/*Tests.cs" (all C# test files)
  - "**/*.spec.ts" (Angular test specifications)
  - "xunit.runner.json" (test configuration)

FORBIDDEN_MODIFICATIONS:
  - "Code/**/*.cs" (excluding *Tests.cs) → CodeChanger territory
  - "**/*.md" → DocumentationMaintainer territory
```

**Key Design Decision:** Glob patterns (`**/*Tests.cs`) provide unambiguous matching. If a file matches the pattern, TestEngineer owns it. Zero gray area.

**BackendSpecialist Authority (Specialist Agent):**
```yaml
COMMAND_INTENT_AUTHORITY:
  - "Code/**/*.cs" (excluding *Tests.cs)
  - "config/**/*.json" (backend configs only)
  - "migrations/**/*.cs" (EF Core migrations)

ALWAYS_FORBIDDEN:
  - "**/*Tests.cs" → TestEngineer exclusive (any intent)
  - "**/*.ts, **/*.html, **/*.css" → FrontendSpecialist domain
```

**Key Design Decision:** BackendSpecialist shares `.cs` authority with CodeChanger but differentiates through **intent recognition** and **domain expertise depth**. Command intent + complex patterns = BackendSpecialist implements directly.

**ComplianceOfficer Authority (Advisory Agent):**
```yaml
FILE_MODIFICATIONS: NONE (absolutely zero direct file changes)

READ_ACCESS_SCOPE:
  - All project files for validation analysis

WRITE_ACCESS_SCOPE:
  - /working-dir/ ONLY for compliance reports
```

**Key Design Decision:** Advisory agents have READ ALL, WRITE NONE authority. Simplest pattern, zero coordination conflicts.

**Authority Decision Framework:**

| Agent Type | File Authority | Coordination Pattern | Example |
|------------|---------------|---------------------|---------|
| **Primary** | Exclusive glob patterns | Sequential handoffs | TestEngineer owns `*Tests.cs` |
| **Specialist** | Shared domain with intent | Flexible activation | BackendSpecialist shares `.cs` via command intent |
| **Advisory** | Zero file modifications | Working directory only | ComplianceOfficer validates, doesn't implement |

**Validation Question:** "If two agents receive the same file path, can I determine which has authority based solely on the patterns?"

---

### Principle 3: Progressive Information Architecture

**Concept:** Front-load critical identity (lines 1-50), defer detailed workflows to on-demand skills.

**Why This Matters:**
- Claude browses agents during task assignment - first 50 lines determine selection
- Full agent definitions loaded only when engaged - unnecessary content wastes tokens
- Skills provide deep workflows on-demand - progressive loading efficiency

**Information Ordering Hierarchy:**

**Lines 1-50: Core Identity (ALWAYS LOADED)**
```markdown
# AgentName

**Agent Type:** [PRIMARY/SPECIALIST/ADVISORY]
**Authority:** [File patterns summary or working directory only]
**Domain:** [Expertise area]

## CORE RESPONSIBILITY
**Primary Mission:** [One-sentence purpose]
**Classification:** [Primary/Specialist/Advisory with key differentiator]

## PRIMARY FILE EDIT AUTHORITY (or FLEXIBLE AUTHORITY or ADVISORY AUTHORITY)
[Critical file patterns - what agent can/cannot modify]
```

**Why Front-Load:** Claude needs to answer "Is this agent relevant for my task?" within first 50 lines. Authority patterns are decision-critical.

**Lines 51-130: Team Coordination (LOADED FOR ACTIVE AGENTS)**
```markdown
## DOMAIN EXPERTISE
[Technical specialization areas]

## MANDATORY SKILLS
[working-directory-coordination and documentation-grounding references]

## TEAM INTEGRATION
[Coordination patterns with other agents]
```

**Why Mid-Section:** Once agent engaged, Claude needs to know HOW to coordinate and WHICH skills to load.

**Lines 131-240: Detailed Workflows (LOADED ON-DEMAND)**
```markdown
## QUALITY STANDARDS
[Validation criteria and quality gates]

## CONSTRAINTS & ESCALATION
[When to involve Claude or other agents]

## COMPLETION REPORT FORMAT
[Standardized deliverable template]
```

**Why Deferred:** Specific procedures loaded when agent activates. These details don't affect agent selection.

**From ComplianceOfficer Example (160 lines - smallest agent):**
- **Lines 1-50:** Advisory role, ZERO file modifications, pre-PR validation focus
- **Lines 51-130:** Validation specialization, mandatory skills, Claude partnership
- **Lines 131-160:** Compliance report format, escalation protocols

**Token Efficiency Achievement:** 160 lines = ~1,360 tokens core definition. Claude loads 30 tokens (filename) for discovery, 1,360 tokens for activation, skills on-demand.

**Validation Question:** "Can Claude determine if this agent is relevant to a task by reading only the first 50 lines?"

---

### Principle 4: Team Coordination Explicitness

**Concept:** Clear handoff protocols (who receives from, who delivers to) prevent coordination gaps.

**Why This Matters:**
- Multi-agent workflows require explicit coordination triggers
- Implicit coordination causes agents to wait for unclear handoffs
- Claude orchestrates based on documented patterns - missing coordination = failed orchestration

**TestEngineer Team Coordination (Sequential Workflow):**

```markdown
## TEAM INTEGRATION

### CodeChanger Coordination (Primary Dependency)
**Pattern:** Sequential workflow - CodeChanger implements → TestEngineer creates tests
**Handoff:** CodeChanger completes feature → TestEngineer validates and creates coverage
**Frequency:** Every implementation PR requires test coverage
**Trigger:** CodeChanger reports implementation complete

### ComplianceOfficer Coordination (Quality Gate)
**Pattern:** TestEngineer delivers test coverage → ComplianceOfficer validates
**Handoff:** TestEngineer reports coverage metrics → ComplianceOfficer validates pre-PR
**Trigger:** Before PR creation
```

**Key Design Decision:** TestEngineer ALWAYS works after CodeChanger. Sequential positioning explicit. ComplianceOfficer ALWAYS validates TestEngineer work. Quality gate explicit.

**BackendSpecialist Team Coordination (Cross-Specialist Harmony):**

```markdown
## TEAM INTEGRATION

### FrontendSpecialist Coordination (API Contract Harmony)
**Pattern:** Backend-Frontend API contract co-design
**Handoff:** BackendSpecialist designs/implements endpoint → FrontendSpecialist consumes API
**Trigger:** Any API contract changes affecting frontend integration
**Coordination Required:** DTOs, endpoint contracts, WebSocket patterns

### TestEngineer Coordination (Testability Integration)
**Pattern:** BackendSpecialist implements backend code → TestEngineer creates tests
**Trigger:** All BackendSpecialist implementations require subsequent test coverage
```

**Key Design Decision:** Specialists coordinate with OTHER specialists (Backend + Frontend for API contracts). All implementations still flow to TestEngineer for coverage. Cross-domain coordination explicit.

**ComplianceOfficer Team Coordination (Advisory Simplicity):**

```markdown
## TEAM INTEGRATION

### Delivers To: Claude (Codebase Manager)
**Pattern:** Dual verification partnership - ComplianceOfficer validates → Claude decides PR creation
**Trigger:** Before every PR creation (final quality gate)

### Receives From: ALL Agents
**Pattern:** Validates deliverables from entire 12-agent team
```

**Key Design Decision:** Advisory agents have simplest coordination - receive inputs from all, deliver validation to Claude. No complex multi-agent handoffs for file modifications.

**Coordination Pattern Types:**

| Pattern Type | When to Use | Example |
|--------------|-------------|---------|
| **Sequential** | Fixed order required | CodeChanger → TestEngineer → ComplianceOfficer |
| **Parallel** | Independent work streams | Multiple specialists work simultaneously |
| **Cross-Specialist** | Domain integration | BackendSpecialist + FrontendSpecialist API alignment |
| **Hub-Spoke** | Central validation | All agents → ComplianceOfficer → Claude |

**Validation Question:** "Can I draw a clear workflow diagram showing when this agent engages others and when others engage this agent?"

---

### Principle 5: Context Efficiency Through Extraction

**Concept:** Extract redundant patterns to skills, preserve unique domain expertise in agent.

**Why This Matters:**
- Working directory protocols shared by all 12 agents = 200 lines × 12 = 2,400 lines of duplication
- Skill extraction reduces agent core by 60%+ while preserving 100% capabilities
- Progressive loading enables on-demand depth without base agent bloat

**Extraction Decision Framework:**

**EXTRACT TO SKILLS WHEN:**
- Pattern used by **3+ agents** (coordination skill)
- Deep technical content **>500 lines** (domain skill)
- Reusable templates/frameworks (meta-skill)
- Cross-cutting protocol (team communication)

**PRESERVE IN AGENT WHEN:**
- Core identity and unique role
- Agent-specific workflows
- Authority boundaries and file patterns
- Essential coordination protocols

**From TestEngineer Example:**

**Before Extraction (524 lines embedded):**
```markdown
## WORKING DIRECTORY COMMUNICATION

1. Pre-Work Artifact Discovery (MANDATORY)
Before starting ANY task, check /working-dir/ for existing artifacts:
[... 85 lines of detailed instructions ...]

2. Immediate Artifact Reporting (MANDATORY)
When creating/updating files, report using this format:
[... 65 lines of template specifications ...]

3. Context Integration Reporting (REQUIRED)
[... 50 lines of integration instructions ...]

## DOCUMENTATION GROUNDING

1. Standards Mastery
[... 72 lines of standards loading methodology ...]

2. Project Architecture
[... 38 lines of module README protocols ...]

3. Domain-Specific Context
[... 42 lines of context ingestion ...]
```

**After Extraction (200 lines optimized):**
```markdown
## WORKING DIRECTORY COMMUNICATION

Use working-directory-coordination skill for all /working-dir/ interactions:
- Execute Pre-Work Artifact Discovery before starting test creation
- Report test coverage artifacts immediately using standardized format
- Document integration when building upon CodeChanger implementations

## DOCUMENTATION GROUNDING

Use documentation-grounding skill before any test file modifications:
- Complete 3-phase grounding: Standards → Architecture → Domain context
- Load TestingStandards.md for coverage requirements
- Review module README.md for architectural understanding
```

**Token Savings:** 324 lines saved (62% reduction). Embedded: ~4,192 tokens. Optimized: ~1,600 tokens core + skills on-demand.

**Progressive Loading Efficiency:**
- **Discovery:** 30 tokens (agent name)
- **Activation:** 1,600 tokens (core definition)
- **Skill Execution:** +2,500 tokens (working-directory-coordination loaded when needed)
- **Maximum Load:** ~6,600 tokens (agent + all skills)

**Multiplication Effect:** Working-directory-coordination skill extracted once, referenced by 12 agents. 200 lines × 12 = 2,400 lines saved project-wide!

**Validation Question:** "Is this content unique to this agent, or would 3+ other agents benefit from the same pattern?"

---

### Principle 6: Skill Reference Pattern Consistency

**Concept:** 2-3 line summary + key workflow + reference to skill achieves ~20 tokens per skill (87% savings vs. embedded).

**Why This Matters:**
- Inconsistent skill references confuse progressive loading triggers
- Missing integration guidance causes agents to under-utilize skills
- Verbose skill references defeat token efficiency goals

**Standard Skill Reference Format:**

```markdown
### [skill-name] ([REQUIRED/DOMAIN-SPECIFIC])
**Purpose:** [1-line capability description - what skill provides]
**Key Workflow:** [3-5 word workflow summary OR primary steps]
**Integration:** [When/how agent uses this skill - 1 sentence]
```

**From All 3 Examples (Consistency Demonstrated):**

**TestEngineer - working-directory-coordination:**
```markdown
### working-directory-coordination (REQUIRED)
**Purpose:** Team communication protocols for artifact discovery, immediate reporting, and context integration
**Key Workflow:** Pre-work discovery → Immediate reporting → Integration reporting
**Integration:** Execute all 3 communication protocols for every working directory interaction
```
**Token Count:** ~30 tokens (includes agent-specific integration)

**BackendSpecialist - working-directory-coordination:**
```markdown
### working-directory-coordination (REQUIRED)
**Purpose:** Team communication protocols for artifact discovery, immediate reporting, and context integration
**Key Workflow:** Pre-work discovery → Immediate reporting → Integration reporting
**Integration:** CRITICAL for specialists - query intent creates artifacts, command intent reports implementations
```
**Token Count:** ~40 tokens (dual-mode context included)

**ComplianceOfficer - working-directory-coordination:**
```markdown
### working-directory-coordination (REQUIRED)
**Purpose:** ABSOLUTELY CRITICAL for advisory agents - all deliverables via working directory artifacts
**Key Workflow:** Pre-work discovery → Immediate reporting → Integration reporting
**Integration:** Pre-work discovery, immediate compliance report creation, remediation recommendations
```
**Token Count:** ~35 tokens (advisory critical emphasis)

**Pattern Consistency Benefits:**
1. **Recognizable Structure:** All agents use same 3-line format (Purpose, Key Workflow, Integration)
2. **Agent-Specific Integration:** Third line customized for how THIS agent uses the skill
3. **Token Efficiency:** ~20-40 tokens vs. ~150-200 embedded = 80-87% savings
4. **Progressive Loading Trigger:** "Integration" line tells agent WHEN to load full skill

**Anti-Pattern (Verbose Reference - ~80 tokens):**
```markdown
### working-directory-coordination (REQUIRED)

This skill provides comprehensive team communication protocols that all agents must follow when interacting with the /working-dir/ directory. It includes detailed instructions for pre-work artifact discovery, immediate artifact reporting using standardized formats, and context integration reporting when building upon other agents' work. The skill ensures consistent communication across the 12-agent team and prevents coordination gaps.

The agent should load this skill whenever working directory interactions are required, including before starting any task (pre-work discovery), when creating or updating artifacts (immediate reporting), and when building upon other agents' deliverables (integration reporting).

See `.claude/skills/coordination/working-directory-coordination/` for complete instructions.
```
**Problem:** 80 tokens for skill reference defeats progressive loading efficiency. Embedded content duplicated across all agents.

**Correct Pattern (Concise Reference - ~30 tokens):**
```markdown
### working-directory-coordination (REQUIRED)
**Purpose:** Team communication protocols for artifact discovery and reporting
**Key Workflow:** Pre-work discovery → Immediate reporting → Integration reporting
**Integration:** Execute all 3 protocols for every working directory interaction
```
**Solution:** 30 tokens provides same information. Agent loads full skill (~2,500 tokens) when executing protocols.

**Validation Question:** "Does my skill reference fit the 3-line format and stay under 50 tokens?"

---

### Principle 7: Intent Recognition for Specialists

**Concept:** Query intent = advisory mode (working directory), Command intent = implementation mode (direct files).

**Why This Matters:**
- Specialists share file domains with primary agents - intent prevents conflicts
- Without intent patterns, BackendSpecialist and CodeChanger both modify `.cs` files ambiguously
- Request verb patterns create clear activation rules for specialists

**Intent Recognition Framework (Specialist Agents Only):**

**Query Intent Indicators (Analysis Mode):**
```yaml
QUERY_PATTERNS:
  - "Analyze/Review/Assess/Evaluate/Examine [domain area]"
  - "What/How/Why questions about existing code"
  - "Identify/Find/Detect issues or patterns"
  - "Should we use [pattern X] or [pattern Y]?"

Action: Working directory analysis only
Authority: NO file modifications
Deliverable: Recommendations consumed by other agents
```

**Command Intent Indicators (Implementation Mode):**
```yaml
COMMAND_PATTERNS:
  - "Fix/Implement/Update/Create/Build/Add [feature]"
  - "Optimize/Enhance/Improve/Refactor existing code"
  - "Apply/Execute recommendations"
  - "Modernize/Upgrade [technology pattern]"

Action: Direct file modifications within domain
Authority: Full implementation rights in expertise area
Deliverable: Code changes, implementations
```

**From BackendSpecialist Example:**

**Query Intent Request:** "Analyze the current repository pattern implementation"
- **BackendSpecialist Response:** Creates working directory analysis document
- **File Modifications:** NONE
- **Deliverable:** `backend-repository-analysis-2025-10-25.md` with recommendations
- **Consumer:** CodeChanger may implement if straightforward, BackendSpecialist re-engaged for complex patterns

**Command Intent Request:** "Implement CQRS pattern for RecipeService"
- **BackendSpecialist Response:** Direct modification of .cs files
- **File Modifications:** RecipeService.cs, RecipeController.cs, Read/Write models created
- **Deliverable:** Implemented code changes with architectural improvements
- **Consumer:** TestEngineer creates tests for CQRS implementation

**Intent Validation Protocol:**

```markdown
## FLEXIBLE AUTHORITY FRAMEWORK

### Intent Recognition System
Before proceeding, self-validate: "Is this QUERY or COMMAND intent?"

**Query Intent:** "Analyze/Review/What/How" → Working directory analysis only
**Command Intent:** "Fix/Implement/Optimize/Create" → Direct file modifications within domain

If ambiguous: Request user clarification rather than assuming
```

**Ambiguous Request Example:**

**User Request:** "Look at RecipeService performance"

**BackendSpecialist Response:**
```
I detected potential intent ambiguity. Do you want me to:

A) QUERY: Analyze performance and provide optimization recommendations (working directory analysis)
B) COMMAND: Investigate and implement performance optimizations (direct code changes)

Please clarify preferred mode.
```

**Design Rationale:** "Look at" could mean analysis OR implementation. Specialist requests clarification rather than guessing.

**Specialist vs. Primary Agent Intent:**

| Agent Type | Intent Recognition | Authority Activation |
|------------|-------------------|---------------------|
| **Primary** | N/A - always implementation | Exclusive file authority always active |
| **Specialist** | Required - query vs. command | Flexible authority based on intent pattern |
| **Advisory** | N/A - always analysis | Zero file authority always enforced |

**Validation Question (Specialist Agents):** "Does my agent definition clearly explain how to detect query vs. command intent?"

---

### Principle 8: Escalation Protocol Design

**Concept:** Define when to defer to Codebase Manager (Claude) vs. engage other specialists vs. proceed autonomously.

**Why This Matters:**
- Agents must know when they're out of scope to prevent overstepping
- Missing escalation protocols cause agents to hesitate on unclear scenarios
- Clear escalation criteria improve orchestration efficiency

**Escalation Scenario Categories:**

**1. Authority Uncertainty**
```yaml
Trigger: "Unclear if this file falls within my authority boundaries"

TestEngineer Example:
  Scenario: "Shared test utility in production project - can I modify?"
  Action: "Request Claude clarification on authority boundary"

BackendSpecialist Example:
  Scenario: "Database migration affects frontend state management - my authority?"
  Action: "Escalate to Claude for cross-domain coordination decision"
```

**2. Standards Conflicts**
```yaml
Trigger: "Project standards appear contradictory for my scenario"

TestEngineer Example:
  Scenario: "TestingStandards.md conflicts with CodingStandards.md on async test patterns"
  Action: "Escalate standards conflict for resolution"

ComplianceOfficer Example:
  Scenario: "GitHub issue acceptance criteria unclear for validation"
  Action: "Request Claude clarification on validation criteria"
```

**3. Coordination Failures**
```yaml
Trigger: "Required agent not responding or deliverable unclear"

TestEngineer Example:
  Scenario: "CodeChanger implementation incomplete, cannot design comprehensive tests"
  Action: "Escalate coordination failure to Claude for orchestration"

BackendSpecialist Example:
  Scenario: "Need FrontendSpecialist for API contract coordination, unclear handoff"
  Action: "Request Claude orchestration for Backend-Frontend alignment"
```

**4. Quality Gate Failures**
```yaml
Trigger: "Cannot meet requirements without architectural changes beyond my scope"

TestEngineer Example:
  Scenario: "Legacy code untestable without major refactoring"
  Action: "Escalate architectural limitation requiring strategic decision"

ComplianceOfficer Example:
  Scenario: "Critical compliance failures - test suite failures block PR"
  Action: "BLOCK PR - Critical compliance gaps require remediation"
```

**5. Complexity Overflow**
```yaml
Trigger: "Task requires coordination across 3+ agent domains"

BackendSpecialist Example:
  Scenario: "End-to-end feature spanning backend, frontend, testing, infrastructure"
  Action: "Request Claude orchestration for multi-agent coordination"

ArchitecturalAnalyst Example:
  Scenario: "System-wide architecture change affecting all 12 agents"
  Action: "Escalate to Claude for comprehensive team coordination"
```

**From ComplianceOfficer Example (Advisory Escalation):**

```markdown
## CONSTRAINTS & ESCALATION

### Escalate to Claude When:

CRITICAL_COMPLIANCE_FAILURES:
  Trigger: PR-blocking violations detected
  Examples: Test failures, security issues, missing documentation
  Action: "BLOCK PR - Critical compliance gaps require remediation"

REMEDIATION_DECISION_NEEDED:
  Trigger: Minor violations - block PR or create with follow-up issue?
  Action: "Request Claude decision on PR vs. remediation approach"

EXCEPTION_REQUESTS:
  Trigger: Standards exception needed for valid reason
  Action: "Document exception request with justification, escalate to Claude"
```

**Key Design Decision:** Advisory agents escalate decisions to Claude, never implement remediations. This focused responsibility simplifies escalation protocols vs. agents that sometimes implement, sometimes escalate.

**Validation Question:** "Does my agent know when to escalate vs. proceed autonomously for every foreseeable scenario?"

---

### Principle 9: Quality Gate Integration

**Concept:** Define how agent work flows through validation checkpoints and AI Sentinels.

**Why This Matters:**
- All work eventually flows to PR creation and AI Sentinel review
- Agents must produce deliverables compatible with validation systems
- Quality gates create accountability for standards compliance

**Quality Gate Levels:**

**Level 1: Agent Self-Validation**
```yaml
TestEngineer_Self_Validation:
  - All created tests must pass (100% executable pass rate)
  - AAA pattern followed in all test implementations
  - Test coverage comprehensive for implemented features
  - No flaky or unreliable tests introduced
```

**Level 2: ComplianceOfficer Pre-PR Validation**
```yaml
ComplianceOfficer_Quality_Gate:
  - Standards compliance across all 4 standards files
  - Test suite execution validation
  - GitHub issue requirements met
  - Documentation completeness verified
  - PR decision: READY or REQUIRES_REMEDIATION
```

**Level 3: AI Sentinels Post-PR Review**
```yaml
AI_Sentinels_Analysis:
  - DebtSentinel: Technical debt sustainability assessment
  - StandardsGuardian: Coding standards and architectural compliance
  - TestMaster: Test coverage and quality analysis
  - SecuritySentinel: Vulnerability and threat assessment (main branch PRs)
  - MergeOrchestrator: Holistic PR analysis for deployment decisions
```

**From TestEngineer Example:**

```markdown
## QUALITY STANDARDS

- **Standards Compliance:** Adhere to TestingStandards.md
- **100% Executable Test Pass Rate:** All tests must pass before delivery
- **Comprehensive Edge Case Coverage:** Beyond happy path testing
- **Pattern Consistency:** Follow established project testing conventions

## COMPLETION REPORT FORMAT

Team Handoffs:
  📋 ComplianceOfficer: [Coverage validation ready]
  📖 DocumentationMaintainer: [Testing pattern documentation if needed]

AI Sentinel Readiness: READY ✅
```

**Key Design Decision:** TestEngineer self-validates test pass rate, ComplianceOfficer validates coverage goals, TestMaster AI Sentinel analyzes comprehensiveness. Three-tier quality assurance.

**Quality Gate Integration Patterns:**

| Agent Type | Self-Validation | ComplianceOfficer | AI Sentinels |
|------------|----------------|------------------|--------------|
| **Primary** | Standards compliance, deliverable completeness | Pre-PR validation | TestMaster, StandardsGuardian review |
| **Specialist** | Domain best practices, testability | Pre-PR validation | Domain-specific Sentinel focus |
| **Advisory** | Analysis thoroughness, recommendation clarity | N/A (IS the validator) | Sentinels validate implementations |

**Validation Question:** "Does my agent clearly specify quality standards and how work flows through validation checkpoints?"

---

### Principle 10: Documentation Grounding Systematization

**Concept:** Phase 1: Standards loading → Phase 2: Project context → Phase 3: Domain-specific patterns.

**Why This Matters:**
- Agent effectiveness depends on understanding project standards before work
- Random documentation access misses critical context
- Systematic 3-phase approach ensures comprehensive grounding

**3-Phase Documentation Grounding Framework:**

**Phase 1: Standards Mastery**
```yaml
Load_Primary_Standards:
  - /Docs/Standards/CodingStandards.md (production code rules)
  - /Docs/Standards/TestingStandards.md (test coverage and quality)
  - /Docs/Standards/DocumentationStandards.md (README and docs patterns)
  - /Docs/Standards/TaskManagementStandards.md (GitHub workflow and PRs)

Purpose: Understand project-wide quality requirements and conventions
When: BEFORE any code, test, or documentation modifications
```

**Phase 2: Project Architecture**
```yaml
Review_Module_READMEs:
  - Module-specific README.md files for architectural context
  - Service boundaries in modular monolith
  - Integration points and coordination patterns
  - Established conventions for the specific area

Purpose: Understand how this module fits into larger architecture
When: BEFORE implementing features in unfamiliar modules
```

**Phase 3: Domain-Specific Context**
```yaml
Analyze_Production_Code:
  - Existing implementations for pattern consistency
  - API contracts and integration patterns
  - Database schema and data models
  - Technology-specific best practices

Purpose: Match existing patterns for consistency
When: BEFORE creating new implementations or tests
```

**From BackendSpecialist Example:**

```markdown
### documentation-grounding (REQUIRED)
**Purpose:** Systematic standards loading before modifications or analysis
**Integration:** Complete 3-phase grounding for command intent; load standards for query intent analysis depth

**BackendSpecialist 3-Phase Grounding:**

**Phase 1: Standards Mastery**
- Load /Docs/Standards/CodingStandards.md (backend sections)
- Review API design guidelines and conventions
- Understand service architecture standards

**Phase 2: Project Architecture**
- Read module README.md for backend architectural context
- Understand service boundaries in modular monolith
- Review existing repository patterns and DI configurations

**Phase 3: Domain-Specific Context**
- Analyze production code relevant to task (controllers, services, repositories)
- Understand database schema and EF Core configurations
- Review API contracts and integration patterns

**Example: "Implement repository pattern for OrderService"**
- Phase 1: CodingStandards.md → Repository pattern requirements
- Phase 2: Code/Zarichney.Server/README.md → Service architecture patterns
- Phase 3: Analyze existing RecipeRepository.cs for consistent pattern implementation
```

**Agent-Specific Grounding Variations:**

**TestEngineer Grounding:**
```
Phase 1: TestingStandards.md → AAA pattern, coverage requirements
Phase 2: Code/Zarichney.Server/Cookbook/README.md → Recipe domain architecture
Phase 3: Analyze RecipeService.cs implementation → Identify test scenarios
```

**ComplianceOfficer Grounding:**
```
Phase 1: Load ALL 4 standards → Comprehensive validation criteria
Phase 2: Review module READMEs → Verify documentation updated
Phase 3: Analyze GitHub issue #123 → Validate acceptance criteria met
```

**Validation Question:** "Does my agent systematically load relevant standards and context before implementing changes?"

---

### Principle 11: Completion Report Standardization

**Concept:** All agents use consistent reporting format for deliverables and team handoffs.

**Why This Matters:**
- Claude orchestrates based on agent completion reports
- Inconsistent reporting formats cause coordination confusion
- Standardized handoffs enable efficient team workflows

**Standard Completion Report Template:**

```yaml
🎯 [AGENT_NAME] COMPLETION REPORT

Status: [COMPLETE/IN_PROGRESS/BLOCKED] ✅
[Agent-Specific Achievement Metric]

[Agent-Specific Deliverables Section]

Team Integration Handoffs:
  📋 [NextAgent1]: [Specific handoff requirements]
  📖 [NextAgent2]: [Coordination needs]
  🔒 [NextAgent3]: [Considerations]

[Agent-Type-Specific Status]
Next Team Actions: [Follow-up tasks]
```

**From TestEngineer Example:**

```markdown
## COMPLETION REPORT FORMAT

🎯 TESTENGINEER COMPLETION REPORT

Status: COMPLETE ✅
Coverage Achievement: 85% comprehensive backend coverage (+12% this iteration)

Test Files Created/Modified:
  - Code/Zarichney.Server.Tests/Services/RecipeServiceTests.cs (45 tests)
  - Code/Zarichney.Server.Tests/Controllers/RecipeControllerTests.cs (28 tests)

Test Pass Rate: 100% (73/73 tests passing)
Coverage Metrics: 73 unit tests, 12 integration tests

Team Handoffs:
  📋 ComplianceOfficer: Coverage validation ready for pre-PR
  📖 DocumentationMaintainer: No new testing patterns requiring documentation

AI Sentinel Readiness: READY ✅
Next Team Actions: ComplianceOfficer pre-PR validation
```

**From BackendSpecialist Example (Dual-Mode Reporting):**

**Query Intent Report:**
```markdown
🎯 BACKENDSPECIALIST ANALYSIS REPORT

Intent Recognition: QUERY_INTENT - Analysis request ✅
Deliverable: Working directory architectural analysis

Analysis Artifact: backend-repository-analysis-2025-10-25.md
Key Findings: N+1 query issues in 3 repositories, missing unit of work pattern
Recommendations:
  - Implement unit of work pattern for transaction consistency
  - Add eager loading for related entities
  - Consider CQRS for read-heavy operations

Consuming Agents:
  💻 CodeChanger: Can implement unit of work if straightforward
  🏗️ BackendSpecialist: Re-engage for CQRS implementation (command intent)
  📋 TestEngineer: Validate changes with comprehensive tests

Next Team Actions: CodeChanger implements or BackendSpecialist command intent re-engagement
```

**Command Intent Report:**
```markdown
🎯 BACKENDSPECIALIST IMPLEMENTATION REPORT

Intent Recognition: COMMAND_INTENT - Direct implementation ✅
Deliverable: CQRS pattern implementation for RecipeService

Files Modified:
  - Code/Zarichney.Server/Services/RecipeService.cs (refactored to command handlers)
  - Code/Zarichney.Server/Services/RecipeQueryService.cs (new read service)
  - Code/Zarichney.Server/Controllers/RecipeController.cs (updated endpoints)

Architectural Improvements:
  - Separated read and write concerns for performance optimization
  - Implemented command handlers for consistency
  - Added query service for optimized reads

Team Handoffs:
  📋 TestEngineer: Requires comprehensive tests for new CQRS implementation
  🎨 FrontendSpecialist: API contracts updated - review endpoint changes
  📖 DocumentationMaintainer: CQRS pattern may require architecture documentation

AI Sentinel Readiness: READY ✅
Next Team Actions: TestEngineer test creation, FrontendSpecialist API integration validation
```

**From ComplianceOfficer Example (Advisory Reporting):**

```markdown
🎯 COMPLIANCEOFFICER VALIDATION REPORT

Status: FAIL - Remediation Required ⚠️
Validation Scope: Pre-PR comprehensive compliance

Compliance Assessment:
  - Standards: VIOLATIONS - 3 coding standards issues detected
  - Tests: PASS - 100% test pass rate (73/73)
  - Documentation: GAPS - README not updated for API contract changes
  - GitHub Issue: REQUIREMENTS_MET - Acceptance criteria complete

Critical Findings: NONE (no PR-blocking violations)

Minor Findings:
  1. CodingStandards.md: Magic numbers in RecipeController.cs (lines 45, 67)
  2. CodingStandards.md: Missing XML documentation for public API method
  3. DocumentationStandards.md: README.md not updated for new CQRS endpoints

Remediation Recommendations:
  💻 CodeChanger: Replace magic numbers with named constants (5 min fix)
  💻 CodeChanger: Add XML documentation to GetRecipes method (2 min fix)
  📖 DocumentationMaintainer: Update README.md API section for CQRS endpoints (10 min update)

PR Decision: REMEDIATION_RECOMMENDED (minor issues, can proceed with follow-up if time-critical)
Next Team Actions: Remediation agents address issues OR Claude decides to create PR with follow-up issue
```

**Report Format Variations by Agent Type:**

| Agent Type | Status Field | Deliverable Section | Handoff Format |
|------------|-------------|---------------------|----------------|
| **Primary** | COMPLETE/IN_PROGRESS/BLOCKED | Files created/modified | Sequential next agent |
| **Specialist** | Intent + Status | Query: artifacts, Command: files | Cross-specialist coordination |
| **Advisory** | PASS/FAIL/REMEDIATION | Analysis artifacts | Consuming agents for remediation |

**Validation Question:** "Does my completion report clearly communicate deliverables, team handoffs, and next actions?"

---

### Principle 12: Context Package Compatibility

**Concept:** Agent definitions must integrate seamlessly with Claude's delegation context packages.

**Why This Matters:**
- Claude delegates work through standardized context packages
- Agents receive mission objectives, file paths, standards references via context package
- Agent design must align with orchestration patterns

**Claude's Context Package Format (from CLAUDE.md):**

```yaml
CORE_ISSUE: "[Specific blocking technical problem]"
INTENT_RECOGNITION: "[Analysis request vs. Implementation request]"
TARGET_FILES: "[Exact files that need modification]"
MINIMAL_SCOPE: "[Surgical changes needed to fix core issue]"
SUCCESS_TEST: "[How to verify the fix actually works]"

Mission Objective: [Focused task with clear acceptance criteria]
GitHub Issue Context: [Issue #, epic progression status]
Technical Constraints: [Standards adherence, performance requirements]

Working Directory Discovery: [MANDATORY - Check existing artifacts]
Working Directory Communication: [REQUIRED - Report artifacts created]
Integration Requirements: [How this coordinates with current system state]
Standards Context: [Relevant standards documents and key sections]
Module Context: [Local README.md files to review]
Quality Gates: [Testing requirements, validation approach]
```

**Agent Design Alignment:**

**Core Issue First Protocol:**
```markdown
# AgentName

## CORE RESPONSIBILITY
**Primary Mission:** [Must align with Claude's CORE_ISSUE delegation]
**Success Criteria:** [Must enable Claude's SUCCESS_TEST validation]
```

**Intent Recognition (Specialists):**
```markdown
## FLEXIBLE AUTHORITY FRAMEWORK
**Query Intent:** [Must recognize Claude's INTENT_RECOGNITION: analysis request]
**Command Intent:** [Must recognize Claude's INTENT_RECOGNITION: implementation request]
```

**Working Directory Protocols:**
```markdown
## MANDATORY SKILLS

### working-directory-coordination (REQUIRED)
[Enables Claude's MANDATORY artifact discovery and reporting requirements]
```

**Standards Integration:**
```markdown
### documentation-grounding (REQUIRED)
[Loads Claude's Standards Context references systematically]
```

**From TestEngineer Example (Context Package Alignment):**

**Claude Delegates:**
```yaml
CORE_ISSUE: "RecipeService implementation complete, requires comprehensive test coverage"
TARGET_FILES: "Code/Zarichney.Server.Tests/Services/RecipeServiceTests.cs"
MINIMAL_SCOPE: "Create unit tests covering all RecipeService public methods"
SUCCESS_TEST: "dotnet test passes 100%, RecipeService fully covered"

Mission Objective: Create comprehensive test coverage for RecipeService
Standards Context: TestingStandards.md - AAA pattern, coverage requirements
Module Context: Code/Zarichney.Server/Cookbook/README.md - Recipe domain architecture
Working Directory Discovery: Check for CodeChanger implementation artifacts
```

**TestEngineer Processes:**
1. **Loads Core Definition:** Recognizes test coverage creation mission aligns with primary responsibility
2. **Executes working-directory-coordination:** Discovers CodeChanger implementation artifacts
3. **Executes documentation-grounding:** Loads TestingStandards.md, module README.md per context package
4. **Implements Tests:** Creates RecipeServiceTests.cs targeting exact files in context package
5. **Validates Success:** Runs `dotnet test` confirming SUCCESS_TEST criteria met
6. **Reports Completion:** Uses standardized format communicating to ComplianceOfficer

**Validation Question:** "Does my agent definition enable smooth processing of Claude's context package delegation format?"

---

## AGENT LIFECYCLE DESIGN

### Creation → Validation → Integration → Optimization → Retirement

**Phase 1: Creation**
- Execute agent-creation meta-skill 5-phase workflow
- Apply appropriate template (primary/specialist/advisory)
- Define clear authority boundaries and coordination patterns
- Integrate mandatory skills (working-directory-coordination, documentation-grounding)

**Phase 2: Validation**
- Review against 12 core design principles
- Validate progressive loading efficiency (lines 1-50 discovery, 51-130 coordination, 131-240 workflows)
- Confirm no authority conflicts with existing 12-agent team
- Test context package compatibility with Claude's delegation format

**Phase 3: Integration**
- Deploy agent definition to `.claude/agents/[agent-name].md`
- Update CLAUDE.md Multi-Agent Development Team section
- Document coordination patterns with existing agents
- Validate real-world task execution effectiveness

**Phase 4: Optimization**
- Measure token efficiency vs. target (130-240 lines for core definition)
- Extract patterns to skills when used by 3+ agents
- Refine authority boundaries based on actual coordination patterns
- Optimize completion report format for team workflow efficiency

**Phase 5: Retirement** (when needed)
- Identify when agent responsibilities obsolete or better merged
- Migrate authority to other agents systematically
- Archive agent definition with retirement rationale
- Update CLAUDE.md team composition documentation

### When to Evolve Agents Over Time

**Expand Authority When:**
- New file types emerge requiring agent's domain expertise
- Coordination patterns reveal natural authority expansion
- Team consensus validates expanded scope maintains single responsibility

**Split Agent When:**
- Single agent accumulates 2+ distinct responsibilities
- Authority boundaries become ambiguous due to scope growth
- Coordination overhead exceeds benefit of unified agent

**Merge Agents When:**
- Coordination overhead between agents exceeds separation benefit
- Authority boundaries naturally converge
- Team size reduction improves orchestration efficiency

**Retire Agent When:**
- Responsibilities fully absorbed by other agents
- Technology changes make domain expertise obsolete
- Minimal agent utilization over extended period

---

## COMMON ANTI-PATTERNS AND PITFALLS

### Anti-Pattern 1: Over-Extraction (Removing Essential Identity Content)

**Problem:** Extracting unique agent identity to skills destroys agent's distinct persona.

**Bad Example:**
```markdown
# TestEngineer

**Primary Mission:** Create test coverage
**Authority:** Test files

Use test-creation-workflows skill for all testing procedures.
Use test-coverage-analysis skill for coverage planning.
Use test-reporting-standards skill for completion reports.

[Agent definition: 45 lines - over-extracted to 3 skills]
```

**Why This Fails:** TestEngineer has no unique identity remaining. All content generic skill references. Agent becomes hollow shell.

**Correct Approach:**
```markdown
# TestEngineer

**Primary Mission:** Create comprehensive test coverage for zarichney-api project
**Authority:** Exclusive modification rights for **/*Tests.cs, **.spec.ts test files

## DOMAIN EXPERTISE
- xUnit testing framework mastery
- FluentAssertions for readable assertions
- Moq for dependency mocking
- AAA pattern and test architecture

## MANDATORY SKILLS
[Skill references here]

## TEAM INTEGRATION
Sequential after CodeChanger, before ComplianceOfficer validation
[Specific coordination patterns]

[Agent definition: 200 lines with unique testing identity preserved]
```

**Validation:** Agent definition contains unique role, specific authority, and domain expertise NOT in skills.

### Anti-Pattern 2: Ambiguous Authority Boundaries (File Pattern Conflicts)

**Problem:** Multiple agents can claim authority over same files, causing coordination conflicts.

**Bad Example:**
```markdown
# CodeChanger
Authority: All .cs files

# BackendSpecialist
Authority: Backend .cs files

# TestEngineer
Authority: Test-related .cs files
```

**Why This Fails:** What are "backend .cs files"? What about `RecipeService.cs` - CodeChanger or BackendSpecialist? Ambiguity creates conflicts.

**Correct Approach:**
```markdown
# CodeChanger (Primary)
EXCLUSIVE_AUTHORITY:
  - "Code/**/*.cs" (excluding *Tests.cs) → Production code only

# BackendSpecialist (Specialist)
COMMAND_INTENT_AUTHORITY:
  - "Code/**/*.cs" (excluding *Tests.cs) → Complex backend patterns
DIFFERENTIATION: Intent-based activation, domain expertise depth

# TestEngineer (Primary)
EXCLUSIVE_AUTHORITY:
  - "**/*Tests.cs" → All test files
FORBIDDEN: "Code/**/*.cs" (excluding tests) → CodeChanger/BackendSpecialist territory
```

**Validation:** File `RecipeService.cs` matches CodeChanger pattern (primary, general implementation). BackendSpecialist engaged for complex architectural patterns via command intent. TestEngineer CANNOT modify (not *Tests.cs pattern).

### Anti-Pattern 3: Missing Coordination Protocols (Team Integration Gaps)

**Problem:** Agent works in isolation without clear handoff patterns to other team members.

**Bad Example:**
```markdown
# TestEngineer

Creates comprehensive test coverage for implementations.

## SKILLS
[working-directory-coordination, documentation-grounding references]

## QUALITY STANDARDS
All tests must pass and follow AAA pattern.
```

**Why This Fails:** No mention of CodeChanger (who provides implementations), ComplianceOfficer (who validates coverage), or team workflow positioning.

**Correct Approach:**
```markdown
# TestEngineer

## TEAM INTEGRATION

### CodeChanger Coordination
**Pattern:** Sequential workflow - CodeChanger implements → TestEngineer creates tests
**Handoff:** CodeChanger completes feature → TestEngineer validates and creates coverage
**Frequency:** Every implementation PR

### ComplianceOfficer Coordination
**Pattern:** TestEngineer delivers coverage → ComplianceOfficer validates
**Trigger:** Before PR creation

### BackendSpecialist & FrontendSpecialist
**Pattern:** Test specialist implementations when command intent
**Trigger:** Specialist implementations require same testing rigor
```

**Validation:** Clear who TestEngineer receives work from (CodeChanger, specialists), who validates TestEngineer work (ComplianceOfficer), and when handoffs occur.

### Anti-Pattern 4: Context Bloat (Failing to Extract to Skills)

**Problem:** Embedding 200+ lines of generic patterns shared by all agents in every definition.

**Bad Example:**
```markdown
# TestEngineer

## WORKING DIRECTORY COMMUNICATION

**MANDATORY PROTOCOLS - ALL AGENTS MUST FOLLOW:**

### 1. Pre-Work Artifact Discovery
Before starting ANY task, agents must check /working-dir/ for existing artifacts:

**Discovery Checklist:**
- [ ] Check /working-dir/session-state.md for current progress
- [ ] Review any CodeChanger implementation artifacts
- [ ] Identify specialist analysis relevant to testing
- [ ] Load prior test coverage reports if incremental work

**Discovery Procedure:**
1. List all files in /working-dir/ using ls command
2. Read session-state.md to understand coordination status
3. Search for artifacts matching agent's domain (grep for "Test", "Coverage")
4. Create discovery report documenting relevant context found

[... 80 more lines of embedded protocols ...]

[Similar 85-line blocks in ALL 12 agents = 1,020 lines of duplication]
```

**Why This Fails:** 1,020 lines of working directory protocols duplicated across 12 agents. Any protocol update requires modifying 12 files. Token bloat reduces context window for actual work.

**Correct Approach:**
```markdown
# TestEngineer

## WORKING DIRECTORY COMMUNICATION

Use working-directory-coordination skill for all /working-dir/ interactions:
- Execute Pre-Work Artifact Discovery before starting test creation
- Report test coverage artifacts immediately using standardized format
- Document integration when building upon CodeChanger implementations

[~30 tokens skill reference vs. ~680 tokens embedded = 96% savings]
[Protocol update changes ONE skill file, all 12 agents automatically updated]
```

**Validation:** Skill extraction reduces 12 × 85 lines = 1,020 lines to 12 × 3 lines = 36 lines total across team.

### Anti-Pattern 5: Scope Creep (Adding Responsibilities Over Time)

**Problem:** Agent accumulates additional responsibilities beyond original single focus, creating authority conflicts and coordination complexity.

**Bad Example (Evolution Over Time):**

**Version 1.0 (Focused):**
```markdown
# TestEngineer
**Primary Mission:** Create comprehensive test coverage
```

**Version 1.3 (Scope Creep Begins):**
```markdown
# TestEngineer
**Primary Mission:** Create comprehensive test coverage and validate code quality
[Added quality validation - conflicts with ComplianceOfficer]
```

**Version 1.7 (Full Scope Creep):**
```markdown
# TestEngineer
**Primary Mission:** Create tests, validate code quality, review architectural patterns, and provide testability recommendations
[Now conflicts with ComplianceOfficer, ArchitecturalAnalyst, AND blurs advisory vs. implementation]
```

**Why This Fails:** TestEngineer originally focused on test creation. Added quality validation conflicts with ComplianceOfficer. Architectural review conflicts with ArchitecturalAnalyst. Testability recommendations blur implementation vs. advisory. Single agent trying to do 4 agents' work.

**Correct Approach (Maintain Focus):**
```markdown
# TestEngineer
**Primary Mission:** Create comprehensive test coverage for zarichney-api project

## CONSTRAINTS
**FOCUS DISCIPLINE:**
- ✅ Create test files and test coverage
- ❌ Validate code quality (ComplianceOfficer responsibility)
- ❌ Architectural analysis (ArchitecturalAnalyst responsibility)
- ❌ Implement production code changes (CodeChanger/Specialist responsibility)

**ESCALATION:**
If testability improvements needed, coordinate with CodeChanger or BackendSpecialist
If architectural concerns identified, escalate to ArchitecturalAnalyst
```

**Validation:** Agent definition explicitly documents what agent does NOT do to prevent scope creep.

---

## VALIDATION CHECKPOINTS

### Pre-Deployment Agent Definition Checklist

**Identity Validation:**
- [ ] Single responsibility clearly stated in one sentence
- [ ] Agent type classification explicit (Primary/Specialist/Advisory)
- [ ] Authority boundaries unambiguous with glob patterns or zero-modification statement
- [ ] Domain expertise scope specific to this agent's unique role

**Structure Validation:**
- [ ] Lines 1-50 contain core identity (role, authority, mission)
- [ ] Lines 51-130 contain skills and team integration
- [ ] Lines 131-240 contain workflows, standards, reporting
- [ ] Total core definition 130-240 lines (advisory can be smaller)

**Authority Validation:**
- [ ] File patterns use glob syntax for precision
- [ ] Forbidden modifications explicitly documented
- [ ] No authority conflicts with existing 12-agent team
- [ ] Intent recognition defined if specialist agent
- [ ] Zero file modification clear if advisory agent

**Skill Integration Validation:**
- [ ] working-directory-coordination skill referenced (MANDATORY)
- [ ] documentation-grounding skill referenced (MANDATORY)
- [ ] Domain skills identified and referenced appropriately
- [ ] Skill references follow 3-line format (Purpose, Key Workflow, Integration)
- [ ] Each skill reference ~20-40 tokens

**Team Coordination Validation:**
- [ ] Coordination patterns with other agents explicit
- [ ] Handoff protocols documented (who provides to, who receives from)
- [ ] Escalation scenarios defined with examples
- [ ] Quality gate integration specified

**Progressive Loading Validation:**
- [ ] Discovery scenario: 30 tokens (agent name discovery)
- [ ] Activation scenario: ~1,300-1,900 tokens (core definition)
- [ ] Skill execution scenario: +2,000-2,500 tokens (skills loaded on-demand)
- [ ] Maximum load scenario: ~4,500-6,500 tokens (agent + all skills)

**Completion Report Validation:**
- [ ] Standardized format includes Status, Deliverables, Team Handoffs
- [ ] Agent-specific metrics appropriate for agent type
- [ ] Team handoffs specify next agent actions
- [ ] AI Sentinel readiness statement included

**Context Package Compatibility Validation:**
- [ ] Agent can process Claude's CORE_ISSUE delegation
- [ ] Intent recognition (if specialist) aligns with INTENT_RECOGNITION field
- [ ] Working directory protocols enable artifact discovery/reporting
- [ ] Documentation grounding loads Standards Context references

### Token Measurement and Optimization Verification

**Token Counting Methodology:**
```
Approximate Token Count = Line Count × 8.5 tokens/line average

Example:
- Agent Definition: 180 lines × 8.5 = ~1,530 tokens
- Skill References: 4 skills × 35 tokens = ~140 tokens
- Total Core Load: ~1,670 tokens
```

**Optimization Targets:**

| Agent Type | Target Lines | Target Tokens | Skill References |
|------------|-------------|---------------|------------------|
| **Primary** | 180-220 lines | 1,530-1,870 tokens | ~90 tokens (2-3 skills) |
| **Specialist** | 160-200 lines | 1,360-1,700 tokens | ~140 tokens (3-4 skills) |
| **Advisory** | 130-170 lines | 1,105-1,445 tokens | ~80 tokens (2 skills) |

**Token Efficiency Calculations:**

**Before Optimization (Embedded Patterns):**
```
Working directory protocols: 85 lines × 8.5 = ~720 tokens embedded
Documentation grounding: 72 lines × 8.5 = ~610 tokens embedded
Total embedded: ~1,330 tokens in agent definition
```

**After Optimization (Skill References):**
```
working-directory-coordination reference: ~30 tokens
documentation-grounding reference: ~40 tokens
Total skill references: ~70 tokens
Savings: 1,330 - 70 = 1,260 tokens saved (95% reduction)
```

**Progressive Loading Efficiency:**
```
Embedded Approach (No Skills):
- Always loads full 4,192 tokens regardless of need
- No on-demand depth available
- All agents duplicate same 1,330 tokens of protocols

Optimized Approach (Skill References):
- Discovery: 30 tokens (agent name only)
- Activation: 1,600 tokens (core definition with skill references)
- Skill Execution: +2,500 tokens (skill loaded when needed)
- Maximum: 6,600 tokens (agent + all skills + resources)

Efficiency Gain: Load only what's needed for each scenario
```

---

## CONCLUSION

### Excellence in Agent Design

Creating an effective agent definition requires balancing 12 core design principles:

1. **Single Responsibility Focus** - One clear purpose
2. **Authority Clarity** - Unambiguous file patterns
3. **Progressive Information Architecture** - Critical content first
4. **Team Coordination Explicitness** - Clear handoff protocols
5. **Context Efficiency Through Extraction** - Skills for shared patterns
6. **Skill Reference Pattern Consistency** - Standard 3-line format
7. **Intent Recognition for Specialists** - Query vs. command patterns
8. **Escalation Protocol Design** - When to defer to Claude
9. **Quality Gate Integration** - Validation checkpoints
10. **Documentation Grounding Systematization** - 3-phase context loading
11. **Completion Report Standardization** - Consistent team communication
12. **Context Package Compatibility** - Claude orchestration alignment

### From Business Requirement to Production Agent

**The Journey:**
1. **Identify Need:** Business requirement reveals capability gap in team
2. **Classify Agent Type:** Primary (exclusive files), Specialist (flexible authority), or Advisory (working directory only)
3. **Define Authority:** Precise glob patterns or zero-modification statement
4. **Extract to Skills:** Shared patterns become reusable skills
5. **Optimize Context:** Target 130-240 lines with progressive loading
6. **Validate Integration:** Confirm no conflicts, clear coordination
7. **Deploy and Monitor:** Real-world usage validates design effectiveness

### The Art of Prompt Engineering

Agent definition creation is strategic business translation - converting user needs into surgical AI capabilities that enhance the entire team's effectiveness. Excellence comes from:

- **Focused Scope Discipline** - Resist feature creep, maintain single responsibility
- **Authority Precision** - Unambiguous boundaries prevent conflicts
- **Context Optimization** - Load only what's needed, when it's needed
- **Team Integration** - Every agent understands their ecosystem position

**You are now equipped to create agents that integrate seamlessly into zarichney-api's 12-agent team, maintaining architectural coherence while enhancing collective capability.**

---

**Document Status:** ✅ **COMPLETE**
**Version:** 1.0.0
**For:** PromptEngineer creating new agents or refactoring existing definitions
**Next Steps:** Review authority-framework-guide.md and skill-integration-patterns.md for deep dives into specific design aspects
