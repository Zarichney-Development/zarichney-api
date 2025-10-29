# Skill Integration Patterns

**Purpose:** Progressive loading design and skill integration best practices
**Target Audience:** PromptEngineer integrating skills into agent definitions
**Version:** 1.0.0

---

## INTRODUCTION - PROGRESSIVE LOADING ARCHITECTURE

### Why Skill Extraction Enables Context Efficiency

**The Token Efficiency Problem:**

Without skill extraction, zarichney-api's 12-agent team faces massive token duplication:

```yaml
EMBEDDED_APPROACH_TOKEN_WASTE:
  Working_Directory_Protocols: 85 lines × 12 agents = 1,020 lines duplicated
  Documentation_Grounding: 72 lines × 12 agents = 864 lines duplicated
  Validation_Checklists: 68 lines × 3 advisory agents = 204 lines duplicated

  Total_Duplication: 2,088 lines of identical content across team
  Token_Waste: ~17,748 tokens (2,088 × 8.5 tokens/line)

  Impact: Every agent load includes redundant protocols, reducing context window for actual work
```

**Skill Extraction Solution:**

```yaml
SKILL_EXTRACTION_EFFICIENCY:
  Working_Directory_Coordination_Skill: 85 lines (once)
  Documentation_Grounding_Skill: 72 lines (once)
  Validation_Checklist_Skill: 68 lines (once)

  Total_Skill_Lines: 225 lines (reusable resources)
  Agent_References: 12 agents × ~3 lines/skill = ~110 lines total

  Token_Savings: 2,088 - 335 = 1,753 lines saved (83% reduction)
  Context_Efficiency: ~14,900 tokens freed for productive work
```

**Progressive Loading Advantage:** Agents load skills ON-DEMAND when needed, not always in core definition.

### Metadata Discovery → Instructions Loading → Resources Access

**3-Tier Progressive Loading Architecture:**

**Tier 1: Metadata Discovery (Agent Selection Phase)**
```yaml
Claude_Browsing_Agents:
  Context: "Which agent should I engage for test coverage creation?"
  Loaded: Agent filenames and first 2-3 lines (role statements)
  Token_Load_Per_Agent: ~30 tokens
  Total_Discovery_Load: 12 agents × 30 = ~360 tokens

  Decision: "TestEngineer matches task requirements (test coverage creation)"
  Next_Action: Load full TestEngineer agent definition
```

**Tier 2: Instructions Loading (Agent Activation Phase)**
```yaml
Claude_Engaging_TestEngineer:
  Context: "TestEngineer engaged with context package for test creation"
  Loaded: Full TestEngineer core definition (200 lines)
  Token_Load: ~1,700 tokens (agent definition + skill references)

  Skills_Identified_From_References:
    - working-directory-coordination (mandatory)
    - documentation-grounding (mandatory)
    - test-coverage-analysis (domain-specific)

  Decision: "Which skills does TestEngineer need for THIS specific task?"
  Next_Action: Load relevant skill SKILL.md files on-demand
```

**Tier 3: Resources Access (Skill Execution Phase)**
```yaml
TestEngineer_Executing_Skill:
  Context: "TestEngineer needs working directory artifact reporting template"
  Loaded: working-directory-coordination SKILL.md (~2,500 tokens)

  Resource_Discovery:
    - Skill references templates/artifact-reporting-template.md
    - Template provides exact format specification

  Resource_Load: +300 tokens (artifact reporting template)
  Total_Context: ~4,500 tokens (agent + skill + template)

  Execution: TestEngineer creates artifact following template format
```

**Progressive Loading Efficiency Comparison:**

| Scenario | Embedded Approach | Progressive Loading Approach |
|----------|------------------|----------------------------|
| **Discovery** | 4,192 tokens (full agent) | 30 tokens (metadata only) |
| **Activation** | 4,192 tokens (always same) | 1,700 tokens (core definition) |
| **Skill Execution** | 4,192 tokens (no additional depth) | +2,500 tokens (skill instructions) |
| **Deep Resources** | N/A (embedded patterns shallow) | +300 tokens (specific templates) |

**Key Advantage:** Load only what's needed at each phase. Discovery = 99% lighter. Execution = Deeper capabilities through modular skills.

### Token Efficiency Gains Across Zarichney-API Agents

**Actual Measurements from Examples:**

**TestEngineer (Primary Agent):**
```yaml
Before_Optimization: 524 lines (~4,192 tokens embedded)
After_Optimization: 200 lines (~1,600 tokens) + skill references (~90 tokens)
Core_Load: ~1,690 tokens
Reduction: 62% token savings while preserving 100% capabilities
```

**BackendSpecialist (Specialist Agent):**
```yaml
Before_Optimization: 536 lines (~4,288 tokens embedded)
After_Optimization: 180 lines (~1,440 tokens) + skill references (~140 tokens)
Core_Load: ~1,580 tokens
Reduction: 63% token savings, dual-mode operation fully supported
```

**ComplianceOfficer (Advisory Agent):**
```yaml
Before_Optimization: 316 lines (~2,688 tokens embedded)
After_Optimization: 160 lines (~1,360 tokens) + skill references (~80 tokens)
Core_Load: ~1,440 tokens
Reduction: 46% token savings, smallest agent in team achieved
```

**Team-Wide Impact:**
```yaml
12_Agents_Optimized:
  Average_Reduction: 57% tokens saved per agent
  Context_Window_Freed: ~14,900 tokens across team
  Skill_Reusability: working-directory-coordination used by all 12 agents (1,560 token savings vs. embedding in each)
```

---

## SKILL INTEGRATION WORKFLOW

### Step 1: Skill Identification

**Analyzing Agent Requirements for Skill Applicability:**

**Process:**
1. Review agent draft for patterns appearing in 3+ existing agents
2. Identify deep technical content exceeding 500 lines
3. Assess reusable templates/frameworks opportunities
4. Recognize cross-cutting protocols (working directory, grounding)

**From TestEngineer Creation Example:**

**Draft Review (524 lines embedded):**
```markdown
## WORKING DIRECTORY COMMUNICATION

[85 lines of pre-work discovery protocols]
[65 lines of immediate reporting formats]
[50 lines of context integration instructions]

## DOCUMENTATION GROUNDING

[72 lines of standards loading methodology]
[38 lines of module README protocols]
[42 lines of domain context ingestion]

## QUALITY STANDARDS
[45 lines of test quality validation procedures]

## COMPLETION REPORTING
[30 lines of standardized report format]
```

**Skill Identification Analysis:**

| Content Section | Lines | Used By Other Agents? | Skill Extraction Candidate? |
|----------------|-------|---------------------|---------------------------|
| Working directory protocols | 200 | ALL 12 agents ✅ | **YES** - Mandatory coordination skill |
| Documentation grounding | 152 | All implementation agents (8+) ✅ | **YES** - Mandatory grounding skill |
| Test quality standards | 45 | TestEngineer ONLY ❌ | **NO** - Agent-specific expertise |
| Completion report format | 30 | Standardized across team ✅ | **MAYBE** - Consider future skill when format stabilizes |

**Extraction Decision:**
- **Extract working directory protocols:** Used by all 12 agents (200 lines × 12 = 2,400 lines duplication)
- **Extract documentation grounding:** Used by 8+ implementation agents (152 lines × 8 = 1,216 lines duplication)
- **Preserve test quality standards:** TestEngineer-specific expertise (unique identity content)
- **Defer completion report extraction:** Standardized but format evolving (future optimization opportunity)

### Mandatory Skills (ALL Agents)

**Every agent must reference these core coordination skills:**

**Skill 1: working-directory-coordination**

**Purpose:** Team communication protocols for artifact discovery, immediate reporting, and context integration

**Why Mandatory:**
- Multi-agent coordination depends on working directory for information sharing
- Prevents coordination gaps when agents work sequentially or in parallel
- Enables Claude to monitor progress through artifact discovery
- Standardizes communication format across entire team

**Integration Pattern in Agent Definitions:**

```markdown
## MANDATORY SKILLS

### working-directory-coordination (REQUIRED)
**Purpose:** Team communication protocols for artifact discovery, immediate reporting, and context integration
**Key Workflow:** Pre-work discovery → Immediate reporting → Integration reporting
**Integration:** Execute all 3 communication protocols for every working directory interaction
```

**Token Efficiency:** ~30 tokens reference vs. ~200 tokens embedded = 85% savings per agent

**Agent-Specific Usage Examples:**

**TestEngineer Usage:**
```markdown
### working-directory-coordination (REQUIRED)
**Integration:** Execute protocols for every working directory interaction

**TestEngineer-Specific Usage:**
- Before creating tests: Check /working-dir/ for CodeChanger implementation artifacts
- After test creation: Report test coverage artifacts immediately using standard format
- When building upon specialist implementations: Document integration with architectural decisions
```

**BackendSpecialist Usage (Dual-Mode):**
```markdown
### working-directory-coordination (REQUIRED)
**Integration:** CRITICAL for specialists - query intent creates artifacts, command intent reports implementations

**Dual-Mode Usage:**
- Query Intent: Create working directory analysis (e.g., backend-architecture-analysis.md)
- Command Intent: Report implementation artifacts (files modified, team coordination requirements)
```

**ComplianceOfficer Usage (Advisory Critical):**
```markdown
### working-directory-coordination (REQUIRED)
**Integration:** ABSOLUTELY CRITICAL for advisory agents - all deliverables via working directory artifacts

**Advisory Usage:**
- Pre-validation: Check for agent implementation artifacts
- Validation reporting: Create compliance validation reports immediately
- Remediation coordination: Specify consuming agents in recommendations
```

**Skill 2: documentation-grounding**

**Purpose:** Systematic standards loading before modifications

**Why Mandatory:**
- Agent effectiveness depends on understanding project standards before work
- Prevents standards violations through comprehensive context loading
- Ensures consistency with established patterns and conventions
- 3-phase systematic approach (Standards → Architecture → Domain)

**Integration Pattern in Agent Definitions:**

```markdown
### documentation-grounding (REQUIRED)
**Purpose:** Systematic standards loading before modifications
**Key Workflow:** Standards mastery → Project architecture → Domain-specific context
**Integration:** Complete 3-phase grounding before any code, test, or documentation modifications
```

**Token Efficiency:** ~40 tokens reference vs. ~180 tokens embedded = 78% savings per agent

**Agent-Specific 3-Phase Grounding Examples:**

**TestEngineer Grounding:**
```
Task: "Create tests for RecipeService"

Phase 1: Standards Mastery
  - Load TestingStandards.md → AAA pattern, comprehensive coverage requirements

Phase 2: Project Architecture
  - Read Code/Zarichney.Server/Cookbook/README.md → Recipe domain architecture

Phase 3: Domain-Specific Context
  - Analyze RecipeService.cs implementation → Identify test scenarios
```

**BackendSpecialist Grounding (Command Intent):**
```
Task: "Implement repository pattern for OrderService"

Phase 1: Standards Mastery
  - Load CodingStandards.md → Repository pattern requirements

Phase 2: Project Architecture
  - Read Code/Zarichney.Server/README.md → Service architecture patterns

Phase 3: Domain-Specific Context
  - Analyze existing RecipeRepository.cs → Ensure consistent pattern implementation
```

**ComplianceOfficer Grounding (Validation Depth):**
```
Task: "Pre-PR comprehensive compliance validation"

Phase 1: Standards Mastery
  - Load ALL 4 standards files → Comprehensive validation criteria

Phase 2: Project Architecture
  - Review module READMEs → Verify documentation updated for changes

Phase 3: Domain-Specific Context
  - Analyze GitHub issue #123 → Validate acceptance criteria met
```

### Domain-Specific Skills Discovery

**Identifying Skills Matching Agent's Domain Expertise:**

**Skill Discovery Process:**

1. **Analyze Agent's Domain Expertise Scope**
2. **Identify Technical Patterns Requiring Deep Workflows**
3. **Assess Reusability Across Similar Domain Agents**
4. **Determine Skill Extraction Benefit vs. Agent Identity Preservation**

**Domain Skill Examples by Agent Type:**

**BackendSpecialist Domain Skills:**

```markdown
### backend-api-design-patterns (DOMAIN-SPECIFIC)
**Purpose:** RESTful API architecture best practices and endpoint optimization
**Key Workflow:** API contract design → Versioning strategy → DTOs → Performance patterns
**Integration:** Apply when designing or analyzing backend API architectures

**When to Use:**
- Query Intent: "Analyze RecipeController API design" → Load skill for comprehensive API analysis
- Command Intent: "Implement new OrderController with REST best practices" → Load skill for implementation guidance
- Coordination: Working with FrontendSpecialist on API contracts

### ef-core-optimization-strategies (DOMAIN-SPECIFIC)
**Purpose:** Database access optimization and query performance patterns
**Key Workflow:** Query analysis → N+1 detection → Eager loading strategies → Index recommendations
**Integration:** Use when optimizing database access or investigating performance issues

**When to Use:**
- Query Intent: "Identify database performance bottlenecks" → Load skill for comprehensive analysis
- Command Intent: "Fix N+1 query issue in RecipeRepository" → Load skill for optimization implementation
```

**FrontendSpecialist Domain Skills (Future):**

```markdown
### angular-component-design-patterns (DOMAIN-SPECIFIC)
**Purpose:** Angular component architecture and reactive programming patterns
**Key Workflow:** Component lifecycle → State management → Change detection optimization
**Integration:** Apply when designing complex Angular components or analyzing component architecture

### ngrx-state-management-patterns (DOMAIN-SPECIFIC)
**Purpose:** NgRx store architecture, actions, reducers, effects patterns
**Key Workflow:** Store design → Action patterns → Side effects → Selector optimization
**Integration:** Use for state management design or implementation
```

**TestEngineer Domain Skills (Future):**

```markdown
### test-coverage-analysis (DOMAIN-SPECIFIC)
**Purpose:** Systematic coverage gap identification and test prioritization
**Key Workflow:** Coverage analysis → Gap prioritization → Test design → Implementation
**Integration:** Apply when planning comprehensive test suite for new implementations

### test-data-builder-patterns (DOMAIN-SPECIFIC)
**Purpose:** Test data construction best practices and fixture management
**Key Workflow:** Builder design → Fixture reusability → Test data maintenance
**Integration:** Use when establishing test data patterns for complex domain models
```

**SecurityAuditor Domain Skills (Future):**

```markdown
### vulnerability-assessment-methodology (DOMAIN-SPECIFIC)
**Purpose:** OWASP Top 10 vulnerability scanning and threat modeling
**Key Workflow:** Threat identification → Risk assessment → Mitigation strategies
**Integration:** Apply for security analysis or vulnerability remediation

### security-pattern-implementation (DOMAIN-SPECIFIC)
**Purpose:** Authentication, authorization, encryption best practices
**Key Workflow:** Pattern selection → Implementation guidance → Validation
**Integration:** Use when implementing security-critical features
```

**Domain Skill Extraction Criteria:**

| Factor | Threshold for Extraction | Example |
|--------|-------------------------|---------|
| **Content Depth** | >500 lines of technical patterns | EF Core optimization strategies (500+ lines) |
| **Reusability** | Used by 2+ agents in domain | API design patterns (BackendSpecialist + FrontendSpecialist coordination) |
| **Technical Complexity** | Requires step-by-step workflows | Vulnerability assessment methodology |
| **Update Frequency** | Pattern evolves independently of agent | Testing frameworks update independently |

### Optional Skills Evaluation

**Context-Dependent Secondary Capabilities:**

**Optional Skill Categories:**

**1. Mission Discipline Skills:**

```markdown
### core-issue-focus (OPTIONAL)
**Purpose:** Surgical scope discipline for implementation-focused agents
**Key Workflow:** Core issue identification → Minimal scope → Validation → No scope creep
**Integration:** Use when agent receives implementation tasks requiring focused execution

**Agents Benefiting:**
- CodeChanger: Prevents feature creep during implementations
- Specialists (command intent): Maintains implementation focus
- NOT advisory agents (no implementation scope)
```

**2. GitHub Integration Skills:**

```markdown
### github-issue-creation (OPTIONAL)
**Purpose:** Technical debt and bug tracking issue creation
**Key Workflow:** Issue template selection → Context documentation → Label assignment
**Integration:** Use when agent identifies technical debt or bugs requiring tracking

**Agents Benefiting:**
- ArchitecturalAnalyst: Creates technical debt issues from analysis
- BugInvestigator: Creates bug tracking issues from diagnostics
- ComplianceOfficer: Creates follow-up issues for deferred compliance items
```

**3. Meta-Skills (PromptEngineer Exclusive):**

```markdown
### agent-creation (META-SKILL)
**Purpose:** Systematic agent definition creation following prompt engineering best practices
**Integration:** PromptEngineer ONLY when creating new agents

### skill-creation (META-SKILL)
**Purpose:** Skill design and implementation workflow
**Integration:** PromptEngineer ONLY when creating new skills

### command-creation (META-SKILL)
**Purpose:** Slash command development for Claude Code CLI
**Integration:** PromptEngineer ONLY when creating slash commands
```

**Optional Skill Decision Framework:**

| Skill Type | Include When | Exclude When |
|------------|-------------|-------------|
| **Mission Discipline** | Agent implements features (scope creep risk) | Advisory agents (no implementation) |
| **GitHub Integration** | Agent creates issues/PRs as deliverable | Agent only modifies files |
| **Meta-Skills** | PromptEngineer creating agents/skills | All other agents |

---

## STEP 2: SKILL REFERENCE FORMAT

### Standardized Skill Integration Using Concise Summaries

**Standard 3-Line Skill Reference Format:**

```markdown
### [skill-name] ([REQUIRED/DOMAIN-SPECIFIC/OPTIONAL])
**Purpose:** [1-line capability description - what skill provides]
**Key Workflow:** [3-5 word workflow summary OR primary steps]
**Integration:** [When/how agent uses this skill - 1 sentence]
```

**Format Breakdown:**

**Line 1: Skill Name and Classification**
- Skill name matches directory: `.claude/skills/[category]/[skill-name]/`
- Classification indicates mandatory level:
  - `(REQUIRED)` = Mandatory for all agents
  - `(DOMAIN-SPECIFIC)` = Specific to agent's expertise area
  - `(OPTIONAL)` = Context-dependent, not always needed

**Line 2: Purpose Statement**
- **1-line maximum** concise capability description
- Focus on WHAT skill provides, not HOW
- Example: "Team communication protocols for artifact discovery and reporting"

**Line 3: Key Workflow**
- **Option 1:** 3-5 word workflow summary (e.g., "Pre-work discovery → Immediate reporting → Integration")
- **Option 2:** Primary workflow steps (e.g., "Standards mastery → Project architecture → Domain context")
- Provides quick understanding of skill's operational flow

**Line 4: Integration Guidance**
- **1 sentence maximum** describing when/how agent uses skill
- Agent-specific customization explaining skill application for THIS agent
- Example: "Execute all 3 protocols for every working directory interaction"

### Token Efficiency: ~20 Tokens Per Skill

**Detailed Token Analysis:**

**Skill Reference (Optimized):**
```markdown
### working-directory-coordination (REQUIRED)
**Purpose:** Team communication protocols for artifact discovery and reporting
**Key Workflow:** Pre-work discovery → Immediate reporting → Integration
**Integration:** Execute all 3 protocols for every working directory interaction
```
**Token Count:** ~30 tokens (4 lines × 7-8 tokens/line average)

**Embedded Content (Before Optimization):**
```markdown
## WORKING DIRECTORY COMMUNICATION

All agents must follow these protocols for effective team coordination:

**1. Pre-Work Artifact Discovery (MANDATORY)**
Before starting ANY task, agents must check /working-dir/ for existing artifacts:

Discovery Checklist:
- [ ] Check /working-dir/session-state.md for current progress
- [ ] Review any relevant agent deliverables from prior work
- [ ] Identify potential integration opportunities with existing artifacts
- [ ] Load prior analysis or implementation context if incremental work

[... 80 more lines of detailed protocols ...]
```
**Token Count:** ~720 tokens (85 lines × 8.5 tokens/line average)

**Token Savings:** 720 - 30 = 690 tokens saved (96% reduction) per agent

**Multiplication Effect (12 Agents):**
```yaml
Embedded_Approach: 720 tokens × 12 agents = 8,640 tokens total
Optimized_References: 30 tokens × 12 agents = 360 tokens total
Team_Savings: 8,280 tokens freed (96% reduction across team)

Plus_Skill_Creation: +1 skill (~2,500 tokens full instructions)
Net_Team_Savings: 8,640 - (360 + 2,500) = 5,780 tokens (67% reduction)
```

**Progressive Loading Benefit:** Skill instructions (~2,500 tokens) loaded only when agent executes skill, not in every agent discovery/activation.

### Agent-Specific Integration Customization Examples

**Same Skill, Different Agent Integration:**

**working-directory-coordination Skill:**

**TestEngineer Integration:**
```markdown
### working-directory-coordination (REQUIRED)
**Purpose:** Team communication protocols for artifact discovery and reporting
**Key Workflow:** Pre-work discovery → Immediate reporting → Integration
**Integration:** Check for CodeChanger artifacts before tests, report coverage artifacts after test creation
```
**Customization:** Specifies TestEngineer checks CodeChanger's work, reports to ComplianceOfficer

**BackendSpecialist Integration (Dual-Mode):**
```markdown
### working-directory-coordination (REQUIRED)
**Purpose:** Team communication protocols for artifact discovery and reporting
**Key Workflow:** Pre-work discovery → Immediate reporting → Integration
**Integration:** Query intent creates analysis artifacts, command intent reports implementation changes
```
**Customization:** Dual-mode context added explaining query vs. command intent usage

**ComplianceOfficer Integration (Advisory Critical):**
```markdown
### working-directory-coordination (REQUIRED)
**Purpose:** ABSOLUTELY CRITICAL for advisory agents - all deliverables via artifacts
**Key Workflow:** Pre-work discovery → Immediate reporting → Integration
**Integration:** All compliance validation delivered through working directory reports
```
**Customization:** Emphasizes advisory dependence on working directory (zero file modifications)

**Key Design Principle:** Same skill, agent-specific integration line customizes HOW this agent uses it.

---

## STEP 3: INTEGRATION POINTS DESIGN

### Where in Agent Definition to Reference Skills

**Skill Reference Section Positioning:**

**Lines 51-130: Mandatory Skills Section**

```markdown
# AgentName

[Lines 1-50: Core identity, authority, mission]

## MANDATORY SKILLS ← Section starts ~line 51

### working-directory-coordination (REQUIRED)
[Skill reference - ~30 tokens]

### documentation-grounding (REQUIRED)
[Skill reference - ~40 tokens]

### [domain-skill-1] (DOMAIN-SPECIFIC)
[Skill reference - ~25 tokens]

### [domain-skill-2] (DOMAIN-SPECIFIC)
[Skill reference - ~25 tokens]

[Lines 131-240: Detailed workflows, quality standards, reporting]
```

**Rationale for Mid-Section Positioning:**
- **After Core Identity (lines 1-50):** Agent selection complete, skills irrelevant for discovery
- **Before Detailed Workflows (lines 131-240):** Skills inform how agent executes, loaded during activation
- **Grouped Together:** All skill references in single section for clarity

**Progressive Loading Trigger Design:**

**Discovery Phase (Lines 1-50 Only):**
```yaml
Claude_Decision_Making:
  Question: "Is TestEngineer relevant for test coverage creation?"
  Loaded: Lines 1-50 (agent name, role, authority)
  Skills: NOT LOADED (skill references in lines 51-130 not needed for selection)
  Token_Load: ~400 tokens (core identity only)
```

**Activation Phase (Lines 1-130):**
```yaml
Claude_Engaging_Agent:
  Context: "TestEngineer selected, load full definition for task execution"
  Loaded: Lines 1-130 (identity + skills + team integration)
  Skills_Identified: working-directory-coordination, documentation-grounding, test-coverage-analysis
  Decision: "Which skills does TestEngineer need for THIS task?"
  Token_Load: ~1,100 tokens (core + skills section)
```

**Skill Execution Phase (On-Demand):**
```yaml
Agent_Executing_Skill:
  Trigger: TestEngineer needs working directory artifact reporting
  Loaded: working-directory-coordination SKILL.md (~2,500 tokens)
  Resources: Artifact reporting template (~300 tokens)
  Total_Context: Agent core (~1,100) + skill (~2,500) + template (~300) = ~3,900 tokens
```

### Indicating When Agent Should Load Skill

**Integration Line Triggers:**

**Explicit Trigger Indicators:**

```markdown
### documentation-grounding (REQUIRED)
**Integration:** Complete 3-phase grounding **before any code modifications**
```
**Trigger:** "before any code modifications" = load skill at start of implementation task

```markdown
### working-directory-coordination (REQUIRED)
**Integration:** Execute protocols **for every working directory interaction**
```
**Trigger:** "for every working directory interaction" = load skill when creating/discovering artifacts

```markdown
### backend-api-design-patterns (DOMAIN-SPECIFIC)
**Integration:** Apply **when designing or analyzing backend API architectures**
```
**Trigger:** "when designing or analyzing backend API" = load skill for API-specific work only

**Conditional Trigger Examples:**

```markdown
### test-coverage-analysis (DOMAIN-SPECIFIC)
**Integration:** Apply **when planning comprehensive test suite for new implementations**
```
**Conditional:** Only load when creating NEW comprehensive test suite, not modifying existing tests

```markdown
### ef-core-optimization-strategies (DOMAIN-SPECIFIC)
**Integration:** Use **when optimizing database access or investigating performance issues**
```
**Conditional:** Only load for database performance work, not general backend implementations

**Progressive Disclosure Principle:**

Agent definition provides:
- **WHAT** skill provides (Purpose line)
- **WHEN** to use skill (Integration line)

Skill SKILL.md provides:
- **HOW** to execute skill (detailed workflow steps)
- **WHY** patterns matter (rationale and best practices)

---

## PROGRESSIVE LOADING DESIGN PATTERNS

### Pattern 1: Discovery Phase (Metadata)

**Scenario:** Claude browsing agents to determine task assignment

**What Gets Loaded:**
- Agent filename (e.g., `test-engineer.md`)
- First 2-3 lines (agent name, role statement)

**Token Load:** ~30 tokens per agent

**Decision Point:** "Does this agent's role match my task requirements?"

**Example:**
```
Claude Task: "Create comprehensive test coverage for RecipeService"

Agent Discovery:
  - test-engineer.md: "TestEngineer - Comprehensive test coverage specialist" ✅ MATCH
  - code-changer.md: "CodeChanger - Production code implementation specialist" ❌ NO MATCH
  - backend-specialist.md: "BackendSpecialist - .NET 8/C# domain expert" ❌ NO MATCH (specialists for complex patterns)

Result: Select TestEngineer for engagement
```

**Token Efficiency Gain:** Load 30 tokens per agent to evaluate 12 agents = 360 tokens total, vs. loading full definitions (4,000+ tokens × 12 = 48,000+ tokens)

### Pattern 2: Activation Phase (Instructions)

**Scenario:** Claude engages selected agent with context package

**What Gets Loaded:**
- Full agent core definition (lines 1-130)
- Skill references (summarized in ~20-40 tokens each)

**Token Load:** ~1,300-1,900 tokens

**Decision Point:** "Which skills does this agent need for THIS specific task?"

**Example:**
```yaml
Claude_Engages_TestEngineer:
  Context_Package:
    CORE_ISSUE: "RecipeService implementation complete, requires comprehensive test coverage"
    TARGET_FILES: "Code/Zarichney.Server.Tests/Services/RecipeServiceTests.cs"

  Agent_Core_Loaded: ~1,700 tokens (TestEngineer definition + skill references)

  Skills_Identified_From_References:
    - working-directory-coordination: Check for CodeChanger implementation artifacts
    - documentation-grounding: Load TestingStandards.md and RecipeService architecture
    - test-coverage-analysis: Plan comprehensive test suite

  Next_Action: Load working-directory-coordination SKILL.md for artifact discovery
```

### Pattern 3: Execution Phase (Workflow)

**Scenario:** Agent invokes skill to execute specific capability

**What Gets Loaded:**
- Skill SKILL.md full instructions (~2,500-5,000 tokens depending on skill complexity)

**Token Load:** Agent core + skill instructions = ~4,000-7,000 tokens

**Decision Point:** "Does agent need skill resources for detailed guidance?"

**Example:**
```yaml
TestEngineer_Executing_working-directory-coordination:
  Current_Context: Agent core loaded (~1,700 tokens)

  Skill_Invocation: "Need to check /working-dir/ for CodeChanger artifacts"
  Skill_Loaded: working-directory-coordination SKILL.md (~2,500 tokens)

  Total_Context: ~4,200 tokens (agent + skill)

  Skill_Workflow_Executed:
    1. Pre-Work Artifact Discovery: List /working-dir/ contents
    2. Discovery Protocol: Read code-changer-implementation-complete.md
    3. Integration Analysis: Understand RecipeService implementation from artifact
    4. Proceed to Test Creation: Context loaded, ready for test design

  Resource_Needed: NO (workflow instructions sufficient, no template loading required)
```

### Pattern 4: Deep Resources (Deep Dives)

**Scenario:** Agent needs specific template or example from skill resources

**What Gets Loaded:**
- Skill resource file (template, example, or documentation) ~300-1,000 tokens

**Token Load:** Agent + skill + resource = ~4,500-8,000 tokens

**Decision Point:** "Template/example provides needed format specification"

**Example:**
```yaml
TestEngineer_Needs_Artifact_Reporting_Template:
  Current_Context: Agent core + working-directory-coordination skill (~4,200 tokens)

  Resource_Discovery:
    - Skill workflow mentions "Use artifact-reporting-template.md for format"
    - TestEngineer unfamiliar with exact format specification

  Resource_Load: working-directory-coordination/resources/templates/artifact-reporting-template.md (~300 tokens)

  Total_Context: ~4,500 tokens (agent + skill + template)

  Template_Application:
    ```
    🗂️ WORKING DIRECTORY ARTIFACT CREATED:
    - Filename: test-coverage-complete-recipeservice-2025-10-25.md
    - Purpose: RecipeService comprehensive test coverage implementation
    - Context for Team: 45 unit tests created, 100% method coverage achieved
    - Consuming Agents: ComplianceOfficer for pre-PR validation
    - Next Actions: ComplianceOfficer validates coverage goals met
    ```

  Result: Artifact created following exact template format
```

**Progressive Loading Efficiency Summary:**

| Phase | Tokens Loaded | Purpose | Efficiency Gain |
|-------|--------------|---------|----------------|
| **Discovery** | ~30 | Agent selection | 99% lighter than full load |
| **Activation** | ~1,700 | Task execution setup | 60% lighter than embedded |
| **Skill Execution** | +2,500 | Detailed workflow guidance | Depth not possible in embedded |
| **Deep Resources** | +300 | Specific templates/examples | On-demand only when needed |

---

## CONTEXT OPTIMIZATION STRATEGIES

### Extraction Decision Framework

**EXTRACT TO SKILLS WHEN:**

✅ **Pattern Used by 3+ Agents (Coordination Skill)**

**Example:** working-directory-coordination
- Used by: ALL 12 agents (mandatory)
- Embedded: 200 lines × 12 agents = 2,400 lines
- Extracted: 1 skill + 12 references (~36 lines total)
- Savings: 2,364 lines (98% reduction)

✅ **Deep Technical Content >500 Lines (Domain Skill)**

**Example:** backend-api-design-patterns (future)
- Content depth: 600+ lines of API architecture patterns
- Agents: BackendSpecialist (primary user), FrontendSpecialist (API coordination)
- Embedded in 2 agents: 1,200 lines total
- Extracted: 1 skill + 2 references = ~650 lines
- Savings: 550 lines (46% reduction) + progressive loading efficiency

✅ **Reusable Templates/Frameworks (Meta-Skill)**

**Example:** agent-creation meta-skill
- Content: 5-phase workflow, templates, examples, documentation
- Users: PromptEngineer exclusively
- Without skill: 4,000+ lines embedded in PromptEngineer definition
- With skill: ~40 token reference + on-demand loading
- Savings: Massive reduction, enables deep resources without agent bloat

✅ **Cross-Cutting Protocol (Team Communication)**

**Example:** documentation-grounding
- Pattern: 3-phase systematic standards loading
- Users: All implementation agents (8+)
- Embedded: 152 lines × 8 agents = 1,216 lines
- Extracted: 1 skill + 8 references = ~200 lines
- Savings: 1,016 lines (84% reduction)

**PRESERVE IN AGENT WHEN:**

✅ **Core Identity and Unique Role**

**Example:** TestEngineer test-specific workflows
- "Create tests following AAA pattern"
- "Ensure comprehensive edge case coverage"
- "100% executable test pass rate validation"
**Rationale:** TestEngineer's unique identity, not shared pattern

✅ **Agent-Specific Workflows**

**Example:** BackendSpecialist intent recognition framework
- Query intent verb patterns specific to backend domain
- Command intent activation for .NET implementations
**Rationale:** Specialist-specific dual-mode operation, not general pattern

✅ **Authority Boundaries and File Patterns**

**Example:** All agents' file authority specifications
- TestEngineer: `**/*Tests.cs` exclusive patterns
- DocumentationMaintainer: `**/*.md` exclusive patterns
**Rationale:** Agent identity defined by file authority, cannot extract

✅ **Essential Coordination Protocols**

**Example:** Agent-specific team integration patterns
- TestEngineer → ComplianceOfficer handoff
- BackendSpecialist → FrontendSpecialist API coordination
**Rationale:** Unique to this agent's position in team workflow

### Content Extraction Decision Examples

**From TestEngineer Optimization:**

**EXTRACTED TO SKILLS:**
```yaml
Working_Directory_Protocols:
  Before: 200 lines embedded in agent definition
  After: ~30 token skill reference
  Skill: working-directory-coordination (~2,500 tokens full instructions)
  Savings: 170 lines per agent × 12 agents = 2,040 lines team-wide

Documentation_Grounding:
  Before: 152 lines embedded in agent definition
  After: ~40 token skill reference
  Skill: documentation-grounding (~2,200 tokens full instructions)
  Savings: 112 lines per agent × 8 implementation agents = 896 lines team-wide
```

**PRESERVED IN AGENT:**
```yaml
Test_Specific_Expertise:
  Content: "AAA pattern requirements, comprehensive coverage expectations, test pass rate validation"
  Lines: 45 (unique TestEngineer identity)
  Rationale: No other agent creates tests, TestEngineer-specific quality standards

File_Authority_Patterns:
  Content: "EXCLUSIVE_AUTHORITY: **/*Tests.cs, **/*.spec.ts"
  Lines: 15 (core identity)
  Rationale: TestEngineer's unique file ownership, cannot generalize

Team_Coordination:
  Content: "Works after CodeChanger, delivers to ComplianceOfficer"
  Lines: 25 (TestEngineer workflow position)
  Rationale: Unique sequential positioning in team workflow
```

**Extraction Decision Matrix:**

| Content Type | Extract to Skill? | Preserve in Agent? | Rationale |
|--------------|------------------|-------------------|-----------|
| Working directory protocols | ✅ YES | ❌ NO | Used by all 12 agents, 2,400 line duplication |
| Documentation grounding | ✅ YES | ❌ NO | Used by 8+ agents, cross-cutting protocol |
| Test-specific quality standards | ❌ NO | ✅ YES | TestEngineer unique identity |
| File authority patterns | ❌ NO | ✅ YES | Core agent identity, cannot generalize |
| Completion report template | **MAYBE** | ✅ YES (for now) | Standardized but evolving, future extraction |

### Token Measurement and Validation Methodologies

**Baseline Measurement (Before Optimization):**

```bash
# Count agent definition lines
wc -l .claude/agents/test-engineer.md
# Output: 524 test-engineer.md

# Estimate tokens (8.5 tokens/line average for Markdown)
echo "524 * 8.5" | bc
# Output: ~4,454 tokens estimated
```

**Optimized Measurement (After Skill Extraction):**

```bash
# Count optimized agent lines
wc -l .claude/agents/test-engineer-optimized.md
# Output: 200 test-engineer-optimized.md

# Calculate core definition tokens
echo "200 * 8.5" | bc
# Output: ~1,700 tokens

# Add skill reference tokens (manual count)
# working-directory-coordination: ~30 tokens
# documentation-grounding: ~40 tokens
# test-coverage-analysis: ~20 tokens
# Total skill references: ~90 tokens

# Total optimized core load
echo "1700 + 90" | bc
# Output: ~1,790 tokens core load
```

**Savings Calculation:**

```bash
# Token reduction
echo "4454 - 1790" | bc
# Output: 2,664 tokens saved

# Percentage reduction
echo "scale=2; (2664 / 4454) * 100" | bc
# Output: 59.80% reduction (60% savings)
```

**Progressive Loading Validation:**

```yaml
SCENARIO_1_DISCOVERY:
  Tokens_Loaded: 30 (agent name + first 2 lines)
  Validation: "Can Claude select agent based on role statement alone?"
  Result: YES - "TestEngineer - Comprehensive test coverage specialist"

SCENARIO_2_ACTIVATION:
  Tokens_Loaded: 1,790 (core definition + skill references)
  Validation: "Can agent execute task with core definition only?"
  Result: NO - Needs working-directory-coordination skill for artifact discovery

SCENARIO_3_SKILL_EXECUTION:
  Tokens_Loaded: 1,790 + 2,500 = 4,290 (core + working-directory skill)
  Validation: "Does skill provide workflow guidance for artifact discovery?"
  Result: YES - Skill includes pre-work discovery protocols

SCENARIO_4_DEEP_RESOURCES:
  Tokens_Loaded: 4,290 + 300 = 4,590 (core + skill + artifact template)
  Validation: "Does template provide exact artifact format specification?"
  Result: YES - Template specifies required fields and structure
```

**Efficiency Comparison:**

| Metric | Embedded Approach | Optimized Approach | Improvement |
|--------|------------------|-------------------|-------------|
| **Discovery** | 4,454 tokens | 30 tokens | 99.3% lighter |
| **Activation** | 4,454 tokens | 1,790 tokens | 59.8% lighter |
| **With Skills** | 4,454 tokens (no depth) | 4,290 tokens (full depth) | +3.8% depth for similar load |
| **Maximum Load** | 4,454 tokens | 4,590 tokens (with templates) | +3% for deeper resources |

**Key Insight:** Progressive loading enables LIGHTER discovery/activation AND DEEPER execution capabilities simultaneously.

---

## INTEGRATION WITH TEAM WORKFLOWS

### Multi-Agent Coordination Enhancement

**How Skill Extraction Improves Team Coordination:**

**Before Skill Extraction (Coordination Challenges):**

```yaml
PROBLEM_1_INCONSISTENT_PROTOCOLS:
  - TestEngineer working directory protocols: 85 lines (version A)
  - BackendSpecialist working directory protocols: 92 lines (version B with specialist additions)
  - ComplianceOfficer working directory protocols: 78 lines (version C with advisory focus)
  Result: 3 different protocol versions = inconsistent artifact communication

PROBLEM_2_UPDATE_PROPAGATION:
  - Protocol improvement identified in TestEngineer
  - Must manually update all 12 agents with new protocol
  - Risk: Agents diverge as updates miss some agents
  Result: Protocol drift across team over time

PROBLEM_3_CONTEXT_BLOAT:
  - Every agent loads 200+ lines of working directory protocols
  - Context window filled with coordination overhead
  - Less room for actual domain expertise and task work
  Result: Reduced effectiveness due to token allocation
```

**After Skill Extraction (Coordination Benefits):**

```yaml
BENEFIT_1_PROTOCOL_CONSISTENCY:
  - ALL 12 agents reference working-directory-coordination skill
  - Single source of truth for protocols (~2,500 tokens in skill)
  - Agent-specific integration lines customize usage (30-40 tokens)
  Result: 100% consistency, agent-specific customization preserved

BENEFIT_2_SINGLE_UPDATE_PROPAGATION:
  - Protocol improvement made in working-directory-coordination skill ONCE
  - All 12 agents automatically use updated skill on next load
  - No manual propagation needed across agent definitions
  Result: Zero protocol drift, instant team-wide updates

BENEFIT_3_CONTEXT_EFFICIENCY:
  - Agents load 30-40 token skill reference (vs. 200+ embedded)
  - Context window freed for domain expertise and task execution
  - Skills loaded on-demand only when needed
  Result: 60%+ more context available for productive work
```

### Skill-Based Reusability Examples

**Coordination Skill Reusability (working-directory-coordination):**

```yaml
SKILL: working-directory-coordination
USERS: ALL 12 agents (mandatory)

Agent_Integration_Variations:
  TestEngineer:
    Usage: Check CodeChanger artifacts → Create test coverage artifacts
    Customization: "Report test coverage artifacts after creation"

  BackendSpecialist:
    Usage: Query intent analysis artifacts OR command intent implementation reporting
    Customization: "Dual-mode - query creates artifacts, command reports implementations"

  ComplianceOfficer:
    Usage: Advisory deliverables exclusively via working directory
    Customization: "ABSOLUTELY CRITICAL - all deliverables via artifacts"

  CodeChanger:
    Usage: Implementation completion reporting
    Customization: "Report implementation complete for sequential handoff to TestEngineer"

Reusability_Achievement:
  - Single skill serves 12 agents with customized integration
  - 200 lines × 12 agents = 2,400 lines avoided through extraction
  - Team-wide protocol consistency maintained automatically
```

**Domain Skill Reusability (backend-api-design-patterns - future):**

```yaml
SKILL: backend-api-design-patterns (future)
PRIMARY_USER: BackendSpecialist
SECONDARY_USERS: FrontendSpecialist (API coordination), CodeChanger (general API implementations)

BackendSpecialist_Integration:
  Query_Intent: "Analyze RecipeController API architecture" → Load skill for comprehensive review
  Command_Intent: "Implement new OrderController following REST best practices" → Load skill for implementation guidance

FrontendSpecialist_Integration:
  Query_Intent: "Review backend API contract for frontend compatibility" → Load skill to understand backend patterns
  Command_Intent: "Update API client for new backend endpoints" → Load skill to align with backend conventions

CodeChanger_Integration:
  Implementation: "Create new CustomerController" → Load skill for API pattern guidance

Reusability_Achievement:
  - Deep API architecture patterns (~600 lines) extracted once
  - 3 agents benefit from same expertise with different integration contexts
  - Backend + Frontend alignment through shared skill understanding
```

### Documentation Grounding Systematic Benefits

**3-Phase Grounding Consistency Across Team:**

```yaml
SKILL: documentation-grounding
USERS: All implementation agents (8+)

Phase_1_Standards_Mastery:
  - ALL agents load same standards files (CodingStandards.md, TestingStandards.md, etc.)
  - Ensures team-wide standards awareness before any modifications
  - Prevents standards violations through systematic loading

Phase_2_Project_Architecture:
  - ALL agents review module README.md for architectural context
  - Ensures implementations align with documented patterns
  - Facilitates architectural consistency across team

Phase_3_Domain_Specific_Context:
  - Agent-specific implementation: TestEngineer analyzes code to test, BackendSpecialist reviews existing patterns
  - Ensures context-aware implementations matching project conventions

Team_Benefit:
  - 100% of implementations grounded in standards before work
  - Architectural consistency through systematic module README review
  - Domain expertise applied with comprehensive project context
  - Standards violations reduced through preventive loading
```

---

## TROUBLESHOOTING AND OPTIMIZATION

### Common Skill Integration Issues

**Issue 1: Agent Over-Extracted (Hollow Shell Problem)**

**Symptoms:**
- Agent definition <130 lines with minimal unique content
- All domain expertise extracted to skills
- Agent feels generic, lacks distinct identity

**Root Cause:** Extraction too aggressive, removed core agent identity content

**Solution:**
```yaml
RE_EVALUATION:
  1. Review agent's unique role and domain expertise
  2. Identify content ONLY this agent needs (not shared by 3+ agents)
  3. Restore unique identity content to agent definition
  4. Keep extraction limited to truly shared patterns

Example_Fix:
  - RESTORE: TestEngineer-specific AAA pattern requirements (unique testing identity)
  - RESTORE: BackendSpecialist .NET 8 architecture patterns (unique domain expertise)
  - KEEP EXTRACTED: Working directory protocols (all 12 agents)
```

**Issue 2: Skill Reference Ambiguity (Unclear When to Load Skill)**

**Symptoms:**
- Agent definition references skill but unclear when agent should load it
- Integration line vague or missing
- Agent hesitates on skill execution

**Root Cause:** Integration line lacks explicit trigger specification

**Solution:**
```yaml
INTEGRATION_LINE_IMPROVEMENT:
  Before: "Use this skill for API design work"
  After: "Apply **when designing or analyzing backend API architectures**"

  Before: "Load this skill as needed"
  After: "Execute **before any code modifications** for standards context"

  Before: "Reference this skill for testing"
  After: "Use **when planning comprehensive test suite for new implementations**"
```

**Issue 3: Skill Load Trigger Unclear (Progressive Loading Breakdown)**

**Symptoms:**
- Skill loaded when not needed (inefficient)
- Skill NOT loaded when needed (missing capability)
- Agent unsure when to invoke skill

**Root Cause:** Agent definition doesn't clearly indicate progressive loading triggers

**Solution:**
```yaml
EXPLICIT_TRIGGERING:
  Integration_Line_Pattern:
    - "**before** [agent action]" = Load skill at task start
    - "**when** [specific scenario]" = Load skill conditionally
    - "**for every** [repeated action]" = Load skill on each occurrence

  Examples:
    - "Complete 3-phase grounding **before any code modifications**" ✅ CLEAR
    - "Execute protocols **for every working directory interaction**" ✅ CLEAR
    - "Apply **when optimizing database access**" ✅ CLEAR (conditional)
    - "Use as needed" ❌ VAGUE (when is "needed"?)
```

**Issue 4: Token Measurement Inaccurate**

**Symptoms:**
- Optimization targets missed (agent still >240 lines)
- Token counts don't match expectations
- Savings percentages lower than anticipated

**Root Cause:** Inaccurate token estimation or miscounting

**Solution:**
```bash
# Accurate line counting
wc -l .claude/agents/agent-name.md

# Conservative token estimation (use 8.5-9 tokens/line for Markdown with formatting)
echo "[line_count] * 8.5" | bc

# Skill reference token counts (manual verification)
# Count actual tokens in skill reference sections
# Typical: 20-40 tokens per skill reference (3-4 lines)

# Validation against targets
# Primary agents: 180-220 lines target
# Specialist agents: 160-200 lines target
# Advisory agents: 130-170 lines target
```

### Optimization Validation Checklist

**Pre-Deployment Skill Integration Validation:**

**Step 1: Mandatory Skills Integration**
- [ ] working-directory-coordination skill referenced (ALL agents)
- [ ] documentation-grounding skill referenced (ALL implementation agents)
- [ ] Skill references use standardized 3-line format
- [ ] Integration lines specify clear usage triggers

**Step 2: Domain Skills Appropriateness**
- [ ] Domain skills match agent's expertise area
- [ ] Domain skills NOT generic patterns (should be agent-specific depth)
- [ ] Domain skills have clear conditional triggers (when to load)
- [ ] Domain skills provide depth not achievable in agent core definition

**Step 3: Optional Skills Justification**
- [ ] Optional skills relevant to agent's mission
- [ ] Optional skills have clear use cases (not "nice to have")
- [ ] Optional skill inclusion enhances agent effectiveness measurably

**Step 4: Token Efficiency Achievement**
- [ ] Agent core definition 130-240 lines (depending on agent type)
- [ ] Skill references ~20-40 tokens each
- [ ] Total core load <2,000 tokens
- [ ] 50-70% token reduction vs. embedded approach measured

**Step 5: Progressive Loading Validation**
- [ ] Discovery scenario: ~30 tokens (agent name + role)
- [ ] Activation scenario: ~1,300-1,900 tokens (core + skill references)
- [ ] Skill execution scenario: Core + skill instructions functional
- [ ] Deep resources scenario: Templates/examples load on-demand

**Step 6: Team Coordination Verification**
- [ ] Skill integration enables coordination (not hinders)
- [ ] Working directory protocols consistent across team
- [ ] Documentation grounding systematic for all implementations
- [ ] No skill conflicts or duplication across agents

---

## ADVANCED PATTERNS

### Skill Composition (Skills Referencing Other Skills)

**Concept:** Skills can reference other skills to build layered capabilities

**Example Pattern:**

```yaml
SKILL: test-coverage-analysis (domain skill)

REFERENCES:
  - working-directory-coordination (for artifact reporting)
  - documentation-grounding (for TestingStandards.md loading)

WORKFLOW:
  1. Load TestingStandards.md via documentation-grounding
  2. Analyze coverage gaps systematically
  3. Create coverage plan artifact via working-directory-coordination
  4. Report plan to TestEngineer for implementation

BENEFIT:
  - test-coverage-analysis builds upon existing coordination skills
  - Avoids duplicating working directory or grounding protocols
  - Layered skill architecture enables progressive depth
```

**Dependency Management:**

```yaml
SKILL_DEPENDENCIES:
  Primary_Skill: test-coverage-analysis

  Depends_On:
    - working-directory-coordination (for artifact creation)
    - documentation-grounding (for standards context)

  Loading_Sequence:
    1. Agent loads test-coverage-analysis skill instructions
    2. Skill references working-directory-coordination for artifact step
    3. Agent loads working-directory-coordination if not already loaded
    4. Agent executes combined workflow with both skills active
```

**Avoiding Circular References:**

```yaml
CIRCULAR_REFERENCE_PREVENTION:
  Rule: "Skills can reference other skills, but never create circular dependencies"

  Valid:
    - Skill A references Skill B (one-way)
    - Skill A and Skill B both reference Skill C (shared dependency)

  Invalid:
    - Skill A references Skill B, Skill B references Skill A (circular)
    - Skill A → Skill B → Skill C → Skill A (circular chain)

  Validation: Build dependency graph, detect cycles before deployment
```

### Conditional Skill Loading (Context-Dependent Activation)

**Concept:** Skills loaded only when specific conditions met

**Example Patterns:**

**Pattern 1: Intent-Based Loading (Specialist Agents)**

```markdown
### backend-api-design-patterns (DOMAIN-SPECIFIC)
**Integration:** Apply **when designing or analyzing backend API architectures**

Agent_Logic:
  IF request_contains("API" OR "endpoint" OR "controller"):
    Load backend-api-design-patterns skill
  ELSE:
    Proceed without API design skill (not needed for this task)
```

**Pattern 2: Complexity-Based Loading**

```markdown
### ef-core-optimization-strategies (DOMAIN-SPECIFIC)
**Integration:** Use **when optimizing database access or investigating performance issues**

Agent_Logic:
  IF request_contains("performance" OR "optimization" OR "N+1 query"):
    Load ef-core-optimization-strategies skill
  ELSE IF request_is_simple_implementation:
    Proceed without optimization skill (general patterns sufficient)
```

**Pattern 3: Phase-Based Loading**

```markdown
### documentation-grounding (REQUIRED)
**Integration:** Complete 3-phase grounding **before any code modifications**

Agent_Logic:
  IF task_type == "implementation" OR task_type == "modification":
    Load documentation-grounding skill (implementation requires context)
  ELSE IF task_type == "analysis_only":
    Load documentation-grounding for analysis depth (standards context)
  ELSE IF task_type == "artifact_reporting":
    Skip documentation-grounding (not needed for reporting)
```

### Skill Version Management

**Challenge:** Skills evolve over time, how to manage versions without breaking agents?

**Versioning Strategy:**

```yaml
SKILL_VERSIONING_APPROACH:
  Location: .claude/skills/[category]/[skill-name]/SKILL.md

  Versioning_Metadata:
    name: skill-name
    version: "1.2.0"
    compatibility: "All agents using references compatible"
    breaking_changes: false

  Backward_Compatibility:
    - Preserve existing workflow steps (add, don't remove)
    - Extend templates (add fields, don't remove required fields)
    - Enhance resources (add examples, don't delete)

  Breaking_Change_Protocol:
    IF breaking_change_required:
      1. Create skill-name-v2 skill (new version)
      2. Migrate agents to v2 systematically
      3. Deprecate v1 after all agents migrated
      4. Document migration in CHANGELOG.md
```

**Example Evolution:**

```yaml
SKILL: working-directory-coordination

VERSION_1.0.0 (Initial):
  - Pre-work artifact discovery
  - Immediate artifact reporting

VERSION_1.1.0 (Enhancement - Backward Compatible):
  - Added: Context integration reporting (new protocol)
  - Preserved: Pre-work discovery and immediate reporting unchanged
  - Impact: Agents using v1.0.0 references still work, new protocol optional

VERSION_2.0.0 (Breaking Change - Requires Migration):
  - Changed: Artifact reporting format (required fields modified)
  - Migration: All agents must update integration lines
  - Approach: Create working-directory-coordination-v2, migrate systematically
```

---

## CONCLUSION

### Progressive Loading Architecture Mastery

You now understand zarichney-api's skill integration framework:

1. **Skill Identification:** Recognize patterns used by 3+ agents for extraction
2. **Skill Reference Format:** Standardized 3-line format (~20-40 tokens each)
3. **Integration Points:** Mid-section positioning (lines 51-130) with clear triggers
4. **Progressive Loading:** Discovery → Activation → Execution → Resources
5. **Context Optimization:** 50-70% token savings while preserving 100% capabilities

### Token Efficiency Achievements

**Team-Wide Impact:**
- **Embedded Approach:** ~52,000 tokens (12 agents × ~4,300 tokens average)
- **Optimized Approach:** ~21,000 tokens (12 agents × ~1,750 tokens average)
- **Team Savings:** 31,000 tokens freed (60% reduction)
- **Progressive Loading:** Deeper capabilities through on-demand skill depth

### Integration with Agent Creation Workflow

**Agent-Creation Meta-Skill Phase 4: Skill Integration**

Apply skill integration patterns from this guide:
1. Identify mandatory skills (working-directory-coordination, documentation-grounding)
2. Discover domain skills matching agent expertise
3. Evaluate optional skills for mission enhancement
4. Use standardized reference format (~20-40 tokens per skill)
5. Specify clear integration triggers for progressive loading
6. Validate token efficiency targets (130-240 lines core definition)

**Your Mastery Enables:**
- Creating agents with 60%+ token efficiency vs. embedded patterns
- Integrating skills systematically for progressive loading optimization
- Maintaining architectural coherence across zarichney-api's 12-agent team
- Achieving smallest agent definitions while preserving deepest capabilities

**Skill integration is the difference between token-bloated agents and context-efficient specialists. You are now equipped to optimize any agent through strategic skill extraction and progressive loading design.**

---

**Document Status:** ✅ **COMPLETE**
**Version:** 1.0.0
**For:** PromptEngineer integrating skills into agent definitions for maximum token efficiency
**Companion Guides:** Review agent-design-principles.md and authority-framework-guide.md for complete agent creation mastery
