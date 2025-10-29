# Progressive Loading Guide: Context Optimization Through Skill Architecture

**Purpose:** Comprehensive deep dive into progressive loading design patterns enabling 87% token reduction through metadata discovery, instruction invocation, and on-demand resource access

**Target Audience:** PromptEngineer creating new skills, optimizing existing skills, or troubleshooting context efficiency issues

**Prerequisites:** Understanding of YAML frontmatter, token budgets, and multi-agent orchestration from SKILL.md Phase 2-3

---

## Table of Contents

1. [Progressive Loading Philosophy](#progressive-loading-philosophy)
2. [Three-Phase Loading Architecture](#three-phase-loading-architecture)
3. [Metadata Discovery Phase](#metadata-discovery-phase)
4. [Instructions Loading Phase](#instructions-loading-phase)
5. [Resource Access Phase](#resource-access-phase)
6. [Token Efficiency Measurement](#token-efficiency-measurement)
7. [Section Ordering Optimization](#section-ordering-optimization)
8. [Content Extraction Decision Framework](#content-extraction-decision-framework)
9. [Multi-Agent Ecosystem Efficiency](#multi-agent-ecosystem-efficiency)
10. [Progressive Loading Patterns from Examples](#progressive-loading-patterns-from-examples)
11. [Token Budget Allocation Strategies](#token-budget-allocation-strategies)
12. [Advanced Optimization Techniques](#advanced-optimization-techniques)
13. [Troubleshooting Context Inefficiency](#troubleshooting-context-inefficiency)

---

## Progressive Loading Philosophy

### Core Principle: Load Only What's Needed, When It's Needed

Progressive loading is the foundational architecture pattern enabling zarichney-api's multi-agent system to scale from 5 agents to 12+ agents without exhausting context windows. The philosophy centers on **three-phase context delivery**:

1. **Discovery Phase:** Minimal metadata (~100 tokens) enabling skill relevance assessment
2. **Invocation Phase:** Complete instructions (~2,000-5,000 tokens) providing workflow execution guidance
3. **Resource Phase:** On-demand templates/examples/documentation (variable tokens) supporting specific workflow steps

This contrasts with **embedded approach** where all content always loaded in agent definitions regardless of task needs.

### Why Progressive Loading Matters

**Context Window Constraints:**
- Claude Sonnet 4.5: 200,000 token context window
- Average agent definition (embedded patterns): ~2,800 tokens
- 12-agent team (embedded approach): 33,600 tokens minimum
- **Remaining context for orchestration, code, documentation:** 166,400 tokens

**With Progressive Loading:**
- Average agent definition (skill references): ~1,440 tokens
- 12-agent team (skill reference approach): 17,280 tokens minimum
- **Remaining context for orchestration, code, documentation:** 182,720 tokens
- **Efficiency gain:** 16,320 tokens (49% reduction) enabling 120% more simultaneous agent capacity

### The Token Efficiency Cascade

**Single Agent Perspective:**
```yaml
Embedded_Pattern_Agent:
  Working Directory Protocol: 150 tokens (always loaded)
  Documentation Grounding: 150 tokens (always loaded)
  API Design Patterns: 500 tokens (always loaded)
  Test Architecture: 400 tokens (always loaded)
  Total Always-Loaded: 1,200 tokens

  Scenario: Agent task is simple bug fix (doesn't need any of these patterns)
  Wasted Context: 1,200 tokens loaded but never referenced

Skill_Reference_Agent:
  working-directory-coordination reference: 20 tokens (always loaded)
  documentation-grounding reference: 20 tokens (always loaded)
  api-design-patterns reference: 20 tokens (loaded only when invoked)
  test-architecture reference: 20 tokens (loaded only when invoked)
  Total Always-Loaded: 80 tokens

  Scenario: Same simple bug fix task
  Wasted Context: 80 tokens in references (never invoked)
  Efficiency: 93% token reduction for this task
```

**Multi-Agent Orchestration Perspective:**
```yaml
Complex_Task_Requiring_5_Agents:
  Claude loads: BackendSpecialist, FrontendSpecialist, TestEngineer, SecurityAuditor, DocumentationMaintainer

  Embedded Approach:
    5 agents × 2,800 tokens = 14,000 tokens base
    + Task context (issue, files, standards) = ~30,000 tokens
    + Agent outputs and coordination = ~40,000 tokens
    Total Context Used: ~84,000 tokens (42% of window)

  Skill Reference Approach:
    5 agents × 1,440 tokens = 7,200 tokens base
    + Invoked skills (2 per agent average) = ~10,000 tokens
    + Task context (same as above) = ~30,000 tokens
    + Agent outputs and coordination = ~40,000 tokens
    Total Context Used: ~87,200 tokens (44% of window)

    But: Skills loaded only when needed per agent
    Actual skills loaded: 6 unique skills across 5 agents (not 10)
    Adjusted skill context: ~6,000 tokens
    Actual Total: ~83,200 tokens (42% of window)

  Real Efficiency: Enables more complex tasks without window exhaustion
```

### Progressive Loading Success Stories

**From working-directory-coordination Skill Creation (coordination-skill-example.md):**
- **Before Extraction:** 12 agents × 150 tokens embedded = 1,800 tokens always loaded
- **After Skill Creation:** 12 agents × 20 token reference = 240 tokens in references + ~2,500 token SKILL.md loaded when needed
- **Efficiency:** When skill not needed: 87% reduction (1,800 → 240 tokens)
- **Ecosystem Impact:** Skill used by all 12 agents, loaded contextually when working directory interaction occurs

**From api-design-patterns Skill Creation (technical-skill-example.md):**
- **Before Extraction:** BackendSpecialist (500 tokens embedded) + FrontendSpecialist (300 tokens embedded) = 800 tokens always loaded
- **After Skill Creation:** 2 agents × 24 token reference = 48 tokens in references + ~4,500 token SKILL.md + ~2,000 tokens resources loaded when API design needed
- **Efficiency:** For non-API tasks: 94% reduction (800 → 48 tokens)
- **Task-Specific:** When API design actually needed: Still 88% more efficient because resources loaded progressively

**From skill-creation Meta-Skill (meta-skill-example.md):**
- **Before Extraction:** PromptEngineer definition included ~2,500 tokens of embedded skill creation methodology
- **After Skill Creation:** 21 token reference + ~3,600 token SKILL.md + ~8,000 tokens comprehensive resources
- **Efficiency:** For non-skill-creation tasks: 99% reduction (2,500 → 21 tokens)
- **Meta-Capability:** When creating skills: 100% of needed guidance available through progressive resource loading

---

## Three-Phase Loading Architecture

### Architectural Overview

Progressive loading divides skill content across three distinct phases, each optimized for specific discovery and execution scenarios:

```
┌─────────────────────────────────────────────────────────────────┐
│ PHASE 1: METADATA DISCOVERY                                     │
│ Context Load: ~100 tokens per skill                             │
│ Files Loaded: YAML frontmatter only                             │
│ Purpose: Scan 10+ skills rapidly to identify relevance          │
│ Decision Point: Does this skill match current task needs?       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
                    [SKILL RELEVANT: INVOKE]
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ PHASE 2: INSTRUCTIONS LOADING                                   │
│ Context Load: 2,000-5,000 tokens                                │
│ Files Loaded: Complete SKILL.md instructions                    │
│ Purpose: Execute workflow steps with comprehensive guidance     │
│ Decision Point: Which workflow steps apply? Need resources?     │
└─────────────────────────────────────────────────────────────────┘
                              ↓
              [RESOURCE NEEDED: LOAD SPECIFIC FILE]
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ PHASE 3: RESOURCE ACCESS                                        │
│ Context Load: Variable (200-3,000 tokens per resource)          │
│ Files Loaded: Specific template/example/documentation           │
│ Purpose: Apply template, review example, understand concept     │
│ Decision Point: Does this resource resolve current need?        │
└─────────────────────────────────────────────────────────────────┘
```

### Phase Transition Triggers

**Discovery → Invocation Trigger:**
- **Event:** Agent (or Claude) identifies skill matches current task from metadata
- **Action:** Load complete SKILL.md file for workflow execution
- **Validation:** Description included clear "when to use" triggers enabling confident invocation

**Invocation → Resource Access Trigger:**
- **Event:** Agent executing workflow step encounters template/example/documentation reference
- **Action:** Load specific resource file from resources/ subdirectory
- **Validation:** SKILL.md workflow includes explicit "Resource:" callouts indicating when to load

**Resource Access → Deep Dive Trigger:**
- **Event:** Agent encounters complex scenario or needs conceptual understanding beyond template
- **Action:** Load documentation files from resources/documentation/
- **Validation:** Resources overview section clearly categorizes documentation vs. templates/examples

### Phase Boundary Design Principles

**Clear Separation of Concerns:**
- **Metadata:** Discovery information only (name, description, category)
- **Instructions:** Workflow steps, process guidance, integration patterns
- **Resources:** Detailed content supporting workflow execution

**No Cross-Phase Redundancy:**
- **Anti-Pattern:** Description duplicates Purpose section from SKILL.md
- **Best Practice:** Description provides discovery context, Purpose provides execution context
- **Validation:** Each phase adds new information, no repetition across phases

**One-Level Deep Resource References:**
- **Anti-Pattern:** SKILL.md references templates/ directory, templates reference examples/
- **Best Practice:** SKILL.md directly references specific template AND specific example
- **Validation:** Agent never loads resource that references another resource for core workflow

---

## Metadata Discovery Phase

### YAML Frontmatter Design for Optimal Discovery

The metadata discovery phase is the first filter agents encounter when browsing skills. Effective YAML frontmatter enables agents to:
1. Scan 10+ skills in ~1,000 tokens
2. Identify 1-2 relevant skills for current task
3. Make confident invocation decision without reading SKILL.md

**Official YAML Frontmatter Specification:**
```yaml
---
name: skill-name-here
description: Brief description of what this skill does and when to use it. MUST include BOTH what the skill does AND when agents should use it. Keep concise but comprehensive. Max 1024 characters.
---
```

### Name Design Patterns

**Constraint Requirements:**
- Max 64 characters
- Lowercase letters, numbers, hyphens only
- No spaces, underscores, special characters
- No reserved words (help, list, all, none)

**Discovery Optimization Strategies:**

**1. Functional Naming (Coordination Skills):**
```yaml
working-directory-coordination  # ✅ EXCELLENT: Immediately conveys cross-agent workflow pattern
artifact-reporting              # ✅ GOOD: Clear purpose, implies team communication
wdc                             # ❌ BAD: Acronym requires prior knowledge, poor discovery
coordination-patterns           # ⚠️ ACCEPTABLE: Generic, less specific than working-directory focus
```

**2. Domain Expertise Naming (Technical Skills):**
```yaml
api-design-patterns             # ✅ EXCELLENT: Clear domain (API) and content type (patterns)
rest-graphql-architecture       # ✅ GOOD: Specific technologies, architectural focus
backend-stuff                   # ❌ BAD: Vague, unprofessional, unclear scope
design-patterns                 # ⚠️ ACCEPTABLE: Too generic without domain context
```

**3. Meta-Capability Naming (Meta-Skills):**
```yaml
agent-creation                  # ✅ EXCELLENT: Precise capability, clear target (agents)
skill-creation                  # ✅ EXCELLENT: Self-documenting, matches purpose exactly
create-new-things               # ❌ BAD: Vague, doesn't specify what things
meta-skill-framework            # ⚠️ ACCEPTABLE: Conceptually accurate but less precise
```

**4. Process Automation Naming (Workflow Skills):**
```yaml
github-issue-creation           # ✅ EXCELLENT: Platform + action clearly specified
pr-analysis-workflow            # ✅ GOOD: Artifact (PR) + process (analysis) clear
issue-stuff                     # ❌ BAD: Unprofessional, unclear scope
github-automation               # ⚠️ ACCEPTABLE: Platform clear but automation too generic
```

### Description Design Patterns

**Dual-Element Requirement: What + When**

Every description must answer:
1. **What does this skill do?** (Capability description)
2. **When should agents use it?** (Trigger scenarios)

**Formula Pattern:**
```
[Capability description explaining what the skill provides].
Use when [primary trigger scenario] to [outcome benefit].
[Optional: efficiency metric or scope clarification].
```

**Example Breakdowns:**

**Coordination Skill Description:**
```yaml
description: Team communication protocols for artifact discovery, immediate reporting, and context integration across multi-agent workflows. Use when creating/discovering working directory files to maintain team awareness. Prevents communication gaps and enables seamless context flow between agent engagements.
```

**Analysis:**
- **What:** "Team communication protocols for artifact discovery, immediate reporting, and context integration"
- **When:** "Use when creating/discovering working directory files"
- **Benefit:** "maintain team awareness...prevents communication gaps...enables seamless context flow"
- **Token Count:** ~60 tokens (well within 1024 character limit)
- **Discovery Effectiveness:** Agent knows exactly when to invoke (working directory interaction)

**Technical Skill Description:**
```yaml
description: Comprehensive REST and GraphQL API design patterns following zarichney-api's .NET 8 backend architecture. Use when BackendSpecialist or FrontendSpecialist designing new endpoints, optimizing existing APIs, or resolving contract integration issues. Ensures consistency and best practices across full stack.
```

**Analysis:**
- **What:** "Comprehensive REST and GraphQL API design patterns following .NET 8 backend architecture"
- **When:** "Use when...designing new endpoints, optimizing existing APIs, or resolving contract integration issues"
- **Target Agents:** "BackendSpecialist or FrontendSpecialist" (specificity aids discovery)
- **Benefit:** "Ensures consistency and best practices across full stack"
- **Token Count:** ~65 tokens
- **Discovery Effectiveness:** Multi-scenario triggers (new design, optimization, integration) cover broad use cases

**Meta-Skill Description:**
```yaml
description: Systematic framework for creating new skills with consistent structure, metadata, and progressive loading design, preventing skill bloat while ensuring quality and reusability. Use when PromptEngineer needs to create new skills, refactor embedded patterns into skills, or establish skill templates for cross-agent workflows. Enables 87% token reduction through skill extraction.
```

**Analysis:**
- **What:** "Systematic framework for creating new skills with consistent structure, metadata, progressive loading design"
- **When:** "Use when PromptEngineer needs to create new skills, refactor embedded patterns, or establish skill templates"
- **Target Agent:** "PromptEngineer" (exclusive audience)
- **Benefit:** "preventing skill bloat while ensuring quality and reusability"
- **Efficiency Metric:** "Enables 87% token reduction" (quantifiable value proposition)
- **Token Count:** ~80 tokens
- **Discovery Effectiveness:** Clear user (PromptEngineer), multiple scenarios, measurable outcome

### Metadata Token Efficiency Validation

**Target: ~100 tokens per skill for discovery phase**

**Measurement Approach:**
```yaml
YAML Frontmatter Components:
  name: "skill-creation" → ~2-4 tokens
  description: "[80 token description]" → ~80-85 tokens
  YAML syntax overhead: --- markers, keys → ~10-15 tokens
  Total: ~95-105 tokens per skill

Discovery Phase Efficiency:
  Scanning 10 skills: 10 × 100 = ~1,000 tokens
  Agent identifies 1-2 relevant skills: Decision made in ~1,000 token budget
  Compared to reading 10 complete SKILL.md files: 10 × 3,000 = ~30,000 tokens
  Discovery Efficiency: 97% token reduction vs. full content scanning
```

### Discovery Phase Examples from Existing Skills

**From coordination-skill-example.md (working-directory-coordination):**
```yaml
---
name: working-directory-coordination
description: Team communication protocols for artifact discovery, immediate reporting, and context integration across multi-agent workflows. Use when creating/discovering working directory files to maintain team awareness. Prevents communication gaps and enables seamless context flow between agent engagements.
---
```

**Discovery Scenario:**
```
Agent Task: "Create analysis report in working directory for other agents to review"

Scans Skills:
- api-design-patterns: ~100 tokens
  → Description mentions "API design" and "endpoints"
  → NOT RELEVANT (task is about working directory, not APIs)
  → SKIP

- working-directory-coordination: ~100 tokens
  → Description mentions "creating/discovering working directory files"
  → MATCH! This is exactly the task scenario
  → INVOKE: Load complete SKILL.md

Total Discovery Context: ~200 tokens (scanned 2 skills)
Decision Made: Load working-directory-coordination SKILL.md
```

**From technical-skill-example.md (api-design-patterns):**
```yaml
---
name: api-design-patterns
description: Comprehensive REST and GraphQL API design patterns following zarichney-api's .NET 8 backend architecture. Use when BackendSpecialist or FrontendSpecialist designing new endpoints, optimizing existing APIs, or resolving contract integration issues. Ensures consistency and best practices across full stack.
---
```

**Discovery Scenario:**
```
Agent: BackendSpecialist
Task: "Create new REST endpoint for recipe search functionality"

Scans Skills:
- test-architecture-best-practices: ~100 tokens
  → Description mentions "test coverage" and "testing patterns"
  → NOT IMMEDIATELY RELEVANT (task is implementation, not testing)
  → SKIP for now (may invoke later for test guidance)

- api-design-patterns: ~100 tokens
  → Description mentions "designing new endpoints"
  → MATCH! Primary task scenario explicitly listed
  → INVOKE: Load complete SKILL.md

- working-directory-coordination: ~100 tokens
  → Description mentions "working directory files"
  → RELEVANT (may need to report artifacts)
  → DEFER: Invoke when creating artifacts, not for API design phase

Total Discovery Context: ~300 tokens (scanned 3 skills)
Primary Decision: Load api-design-patterns SKILL.md
Secondary Decision: Queue working-directory-coordination for artifact phase
```

---

## Instructions Loading Phase

### SKILL.md Structure for Efficient Invocation

Once agent identifies relevant skill from metadata, Phase 2 loads complete SKILL.md instructions. This phase must provide:
1. Comprehensive workflow execution guidance
2. Clear integration patterns with team processes
3. Resource references for detailed support
4. All content within 2,000-5,000 token budget based on skill category

**Token Budget by Skill Category:**

| Category       | Token Budget | Content Density | Resource Depth |
|----------------|--------------|-----------------|----------------|
| Coordination   | 2,000-3,500  | Moderate        | Moderate       |
| Documentation  | 2,500-4,000  | High            | Moderate       |
| Technical      | 3,000-5,000  | High            | Extensive      |
| Meta           | 3,500-5,000  | Very High       | Comprehensive  |
| Workflow       | 2,000-3,500  | Moderate        | Moderate       |

### Front-Loading Critical Content

**Progressive Disclosure Within SKILL.md:**

**Lines 1-80 (Always Read First - ~600 tokens):**
Purpose: Enable agent to confirm skill relevance and understand primary workflow without reading entire file

Content Priority:
1. **YAML Frontmatter:** Reconfirm skill identity (already seen in discovery)
2. **Title and Introduction:** 2-3 line overview contextualizing skill
3. **PURPOSE Section:** Core mission, why matters, mandatory application
4. **WHEN TO USE Section:** 3-5 primary scenarios with triggers/actions/rationale
5. **TARGET AGENTS Overview:** Who uses this skill and how

**Example: Front-Loaded Content from working-directory-coordination (coordination-skill-example.md):**
```markdown
---
name: working-directory-coordination
description: [already scanned in Phase 1]
---

# Working Directory Coordination Skill

**Multi-agent communication protocol ensuring comprehensive team awareness through
systematic artifact discovery, immediate reporting, and context integration.**

## PURPOSE

### Core Mission
Eliminate communication gaps across zarichney-api's 12-agent team through mandatory
working directory artifact discovery and immediate reporting protocols.

### Why This Matters
Without systematic artifact communication, agents work in isolation creating duplicate
analysis, conflicting recommendations, and context fragmentation requiring manual
Claude intervention.

### Mandatory Application
- **Required For:** All agents before starting any task (pre-work discovery)
- **Required For:** All agents when creating/updating working directory files (immediate reporting)
- **No Exceptions:** Communication protocols are non-negotiable team standards

---

## WHEN TO USE

### 1. Pre-Work Artifact Discovery (MANDATORY BEFORE ALL TASKS)
**Trigger:** Agent receives task assignment from Claude
**Action:** Scan `/working-dir/` for existing artifacts from other agents before beginning work
**Rationale:** Build upon team context rather than duplicate effort

### 2. Immediate Artifact Reporting (MANDATORY WHEN CREATING FILES)
**Trigger:** Agent creates or updates any file in `/working-dir/`
**Action:** Report artifact using standardized format immediately
**Rationale:** Maintain team awareness enabling seamless coordination

### 3. Context Integration Documentation (MANDATORY WHEN USING ARTIFACTS)
**Trigger:** Agent builds upon artifacts from other agents
**Action:** Document integration approach and value addition
**Rationale:** Show how team context informed work and prepared future handoffs
```

**Token Count for Lines 1-80:** ~620 tokens
**Agent Decision After Reading:** "This skill definitely applies - I need to check working directory before starting AND report when creating artifacts. Let me continue reading workflow steps."

**Lines 81-300 (Detailed Execution Guidance - ~1,800 tokens):**
Purpose: Provide step-by-step workflow execution with checklists, templates references, and integration patterns

Content Priority:
1. **WORKFLOW STEPS:** Detailed processes for each "When to Use" scenario
2. **Checklists:** Completion validation for each workflow step
3. **Resource References:** Explicit callouts to templates/examples when needed
4. **Integration Patterns:** How this skill coordinates with other team workflows
5. **Quality Gates:** How skill supports ComplianceOfficer, AI Sentinels

**Example: Detailed Workflow from working-directory-coordination:**
```markdown
## WORKFLOW STEPS

### Step 1: Pre-Work Artifact Discovery

**Purpose:** Scan existing working directory artifacts before beginning task to build upon team context

#### Process

1. **List Working Directory Contents:**
   ```bash
   ls -la /working-dir/
   ```
   Review all files, note timestamps and names for relevance assessment

2. **Identify Relevant Artifacts:**
   - Analysis reports from BugInvestigator or ArchitecturalAnalyst
   - Implementation plans from BackendSpecialist or FrontendSpecialist
   - Testing artifacts from TestEngineer
   - Security assessments from SecurityAuditor

3. **Load and Review Relevant Context:**
   - Read artifact content thoroughly
   - Identify integration opportunities with current task
   - Note dependencies or constraints from previous work
   - Assess potential conflicts requiring coordination

4. **Report Discovery Using Standard Format:**
   ```markdown
   🔍 WORKING DIRECTORY DISCOVERY:
   - Current artifacts reviewed: [list files checked]
   - Relevant context found: [artifacts informing current work]
   - Integration opportunities: [how existing work will be built upon]
   - Potential conflicts: [any overlapping concerns identified]
   ```

#### Checklist
- [ ] Working directory contents listed and reviewed
- [ ] All relevant artifacts identified and loaded
- [ ] Discovery report formatted using standard template
- [ ] Integration approach documented before beginning work

**Resource:** See `resources/templates/discovery-report-template.md` for complete format specification
**Resource:** See `resources/examples/backend-specialist-discovery.md` for realistic discovery scenario

---

### Step 2: Immediate Artifact Reporting
[Detailed workflow continues...]
```

**Token Count for Lines 81-300:** ~1,850 tokens
**Agent Capability After Reading:** Can execute complete workflow without loading resources (though templates/examples available if needed for format clarification)

**Lines 301-500 (Advanced Context and Resources Overview - ~1,600 tokens):**
Purpose: Provide resources navigation, troubleshooting, success metrics, and advanced optimization

Content Priority:
1. **RESOURCES Section:** Complete overview of templates/examples/documentation available
2. **SUCCESS METRICS:** How to validate skill execution effectiveness
3. **TROUBLESHOOTING:** Common issues with resolutions
4. **INTEGRATION WITH TEAM WORKFLOWS:** Advanced coordination patterns
5. **CLAUDE.md Integration:** How Claude orchestrates using this skill

**Example: Resources Overview from working-directory-coordination:**
```markdown
## RESOURCES

### Templates (Ready-to-Use Formats)

**discovery-report-template.md** - Standardized format for pre-work artifact discovery reporting
- Use when: Starting any task requiring working directory context scan
- Content: Complete format with placeholder guidance and validation checklist
- Token Load: ~280 tokens

**immediate-reporting-template.md** - Standardized format for artifact creation/update reporting
- Use when: Creating or updating any working directory file
- Content: Complete format with context package guidance for team awareness
- Token Load: ~320 tokens

**context-integration-template.md** - Format for documenting integration with other agents' work
- Use when: Building upon artifacts from other agents
- Content: Integration approach and value addition documentation format
- Token Load: ~240 tokens

**Location:** `resources/templates/`
**Usage:** Copy template, fill specific details, use verbatim in agent work

### Examples (Reference Implementations)

**backend-specialist-discovery.md** - Complete discovery workflow for API implementation task
- Demonstrates: BackendSpecialist scanning working directory before creating new REST endpoints
- Highlights: Integration with ArchitecturalAnalyst's API design recommendations
- Token Load: ~850 tokens

**multi-agent-workflow.md** - Complex scenario with 3+ agents using working directory coordination
- Demonstrates: Sequential artifacts from BugInvestigator → BackendSpecialist → TestEngineer
- Highlights: Context integration and handoff preparation across agent boundaries
- Token Load: ~1,200 tokens

**Location:** `resources/examples/`
**Usage:** Review examples for realistic scenarios showing workflow steps in action

### Documentation (Deep Dives)

**progressive-loading-architecture.md** - Comprehensive guide to context efficiency design principles
- Deep Dive: Discovery → Invocation → Resources loading flow
- Use when: Understanding skill design philosophy or optimizing new skill creation
- Token Load: ~2,400 tokens

**troubleshooting-coordination.md** - Advanced resolution for complex coordination scenarios
- Deep Dive: Edge cases, conflict resolution, escalation paths
- Use when: Encountering coordination issues beyond standard workflow resolution
- Token Load: ~1,800 tokens

**Location:** `resources/documentation/`
**Usage:** Deep understanding of concepts, troubleshooting, optimization techniques
```

**Token Count for Lines 301-500:** ~1,600 tokens
**Total SKILL.md:** ~4,070 tokens (within 2,000-5,000 budget, appropriate for coordination skill trending toward comprehensive)

### Invocation Phase Token Efficiency

**Complete Instruction Loading:**
```yaml
Phase 2 Context Load:
  SKILL.md File: ~4,070 tokens
  Agent Reads: Lines 1-80 for confirmation (~600 tokens processed first)
  Agent Continues: Lines 81-500 for complete workflow (~3,470 tokens)

  Decision After Phase 2:
    - Can execute workflow? YES (all steps clearly documented)
    - Need template for exact format? MAYBE (resources available if needed)
    - Total context loaded: 4,070 tokens

  Compared to Embedded Approach:
    - Embedded in agent definition: 150 tokens (insufficient detail for execution)
    - Agent must infer format and process: Results in inconsistent implementation
    - With skill: 4,070 tokens comprehensive guidance = Consistent, validated execution
```

**Progressive Disclosure Efficiency:**
Agents read SKILL.md sequentially:
1. Lines 1-80 (600 tokens): Confirms relevance, understands primary workflow
2. Lines 81-300 (1,850 tokens): Loads detailed execution guidance
3. Lines 301-500 (1,620 tokens): References resources but doesn't load them yet

**Key Efficiency Insight:** Agent loads ~4,000 tokens of instructions but may only load ~500 tokens of actual resources (templates), achieving comprehensive guidance with targeted detail

---

## Resource Access Phase

### On-Demand Resource Loading Patterns

Phase 3 activates when agent executing SKILL.md workflow encounters resource reference requiring detailed content. This phase is **variable and selective** - not all resources loaded for every workflow execution.

**Resource Categories and Loading Triggers:**

### Templates: Immediate Format Application

**Purpose:** Ready-to-use formats agents copy-paste with minimal modification

**Token Budget:** 200-500 tokens per template

**Loading Trigger Patterns:**

**Explicit Workflow Reference:**
```markdown
**Resource:** See `resources/templates/discovery-report-template.md` for complete format specification
```
Agent thinks: "I need exact format for discovery report. Load this template now."

**Uncertainty About Format:**
Agent executes workflow but unsure of exact output format → Loads template for clarification

**First-Time Usage:**
Agent encountering workflow for first time → Loads template as reference, may not need subsequently

**Example Template Loading Scenario (from coordination-skill-example.md):**
```
Agent: BackendSpecialist
Task: "Implement new recipe search endpoint"
Skill Invoked: working-directory-coordination

Workflow Execution:
Step 1: Pre-Work Artifact Discovery
  → Agent reads workflow step in SKILL.md
  → Sees: "Report discovery using standard format"
  → Sees: "Resource: See resources/templates/discovery-report-template.md"
  → Decision: Load template for exact format

Template Loading:
  File: resources/templates/discovery-report-template.md
  Token Load: ~280 tokens
  Content: Complete format with {{PLACEHOLDERS}}

Agent Action:
  → Copies template
  → Replaces placeholders with specific discoveries
  → Reports in working directory communication

Result: Standardized format achieved, team awareness maintained
```

**Template Token Efficiency:**
```yaml
Without Template (Agent Improvises):
  Agent reads: "Report discovery using standard format"
  Agent thinks: "What's the standard format?"
  Agent searches: CLAUDE.md, working directory examples, previous artifacts
  Context loaded: ~2,000 tokens searching, inconsistent results
  Outcome: Non-standard format, team confusion

With Template (Progressive Loading):
  Agent loads: discovery-report-template.md (~280 tokens)
  Agent applies: Fills placeholders
  Outcome: Standard format, consistent team communication
  Efficiency: 86% token reduction (2,000 → 280) + guaranteed consistency
```

### Examples: Pattern Recognition and Realistic Demonstrations

**Purpose:** Show complete workflows in realistic agent contexts with annotations explaining decisions

**Token Budget:** 500-1,500 tokens per example

**Loading Trigger Patterns:**

**Complex Workflow Understanding:**
Agent familiar with workflow steps but unsure how they apply in realistic scenario → Loads example for context

**Decision Point Clarification:**
Workflow has branch points or conditional logic → Example shows how agent chose path

**Multi-Agent Coordination:**
Workflow involves handoffs or integration with other agents → Example demonstrates coordination patterns

**Best Practice Demonstration:**
Agent wants to understand optimal approach beyond minimum workflow → Example shows expert execution

**Example Loading Scenario (from technical-skill-example.md):**
```
Agent: FrontendSpecialist
Task: "Integrate with new backend recipe search endpoint"
Skill Invoked: api-design-patterns

Workflow Execution:
Step 3: API Contract Integration
  → Agent reads: "Coordinate with BackendSpecialist on contract design"
  → Agent thinks: "How does this coordination actually work in practice?"
  → Sees: "Resource: See resources/examples/backend-frontend-api-coordination.md"
  → Decision: Load example for realistic coordination demonstration

Example Loading:
  File: resources/examples/backend-frontend-api-coordination.md
  Token Load: ~1,150 tokens
  Content: Complete scenario showing:
    - BackendSpecialist creates API contract proposal
    - Posts in working directory for review
    - FrontendSpecialist loads artifact, provides feedback
    - Iteration on DTOs, error handling, pagination
    - Final agreement documented

Agent Action:
  → Follows demonstrated pattern
  → Creates similar artifact in working directory for BackendSpecialist
  → Annotates with specific needs from frontend perspective

Result: Effective coordination, contract meets both backend and frontend needs
```

**Example Token Efficiency:**
```yaml
Without Example (Agent Guesses Pattern):
  Agent reads: "Coordinate with BackendSpecialist"
  Agent thinks: "Should I message Claude? Update agent definition? Create artifact?"
  Agent context: Unclear coordination mechanism
  Outcome: Inefficient coordination, possible escalation to Claude
  Token Cost: ~3,000 tokens in back-and-forth clarification

With Example (Progressive Loading):
  Agent loads: backend-frontend-api-coordination.md (~1,150 tokens)
  Agent sees: Working directory artifact approach clearly demonstrated
  Outcome: Successful coordination without escalation
  Efficiency: 62% token reduction (3,000 → 1,150) + autonomous execution
```

### Documentation: Deep Conceptual Understanding

**Purpose:** Comprehensive guides for complex concepts, troubleshooting, and optimization techniques

**Token Budget:** 1,000-3,000 tokens per document

**Loading Trigger Patterns:**

**Edge Case Encountered:**
Agent executing workflow hits scenario not covered in standard steps → Loads documentation for comprehensive guidance

**Conceptual Understanding Needed:**
Agent executing workflow but wants to understand "why" behind approach → Loads documentation for philosophy

**Troubleshooting Complex Issue:**
Standard workflow not resolving agent's specific scenario → Loads troubleshooting documentation

**Optimization Opportunity:**
Agent successfully executed workflow but wants to improve efficiency or effectiveness → Loads optimization documentation

**Documentation Loading Scenario (from meta-skill-example.md):**
```
Agent: PromptEngineer
Task: "Create new skill for security-threat-modeling"
Skill Invoked: skill-creation

Workflow Execution:
Phase 1: Skill Scope Definition
  → PromptEngineer applies anti-bloat decision framework
  → Question arises: "This skill will be used by SecurityAuditor (1 agent), but also potentially by BackendSpecialist and FrontendSpecialist for security reviews (2 more agents). Does this meet the 3+ agent threshold?"
  → Workflow guidance: "Reusability threshold: 3+ agents currently or anticipated within 6 months"
  → PromptEngineer thinks: "Need deeper understanding of reusability threshold edge cases"
  → Sees: "Resource: See resources/documentation/anti-bloat-framework.md for comprehensive threshold analysis"
  → Decision: Load documentation for edge case guidance

Documentation Loading:
  File: resources/documentation/anti-bloat-framework.md
  Token Load: ~2,100 tokens
  Content: Comprehensive analysis including:
    - Current usage (agents using now) vs. anticipated usage (will use within 6 months)
    - Edge case: 1 primary agent + 2 secondary optional agents = Meets threshold
    - Decision framework: If primary+secondary = 3+ and usage validated, proceed
    - Examples of accepted edge cases and rejected bloat

PromptEngineer Action:
  → Validates: SecurityAuditor (primary) + BackendSpecialist (secondary) + FrontendSpecialist (secondary) = 3 agents
  → Documents: Anticipated usage pattern supporting skill creation
  → Proceeds: Phase 2 with confidence in anti-bloat compliance

Result: Skill creation justified, bloat prevented, edge case properly evaluated
```

**Documentation Token Efficiency:**
```yaml
Without Documentation (PromptEngineer Escalates):
  PromptEngineer uncertain: Escalates to Claude or user
  Claude/user provides guidance: ~1,500 tokens in conversation
  Delay: Asynchronous back-and-forth
  Outcome: Decision eventually made but inefficient process

With Documentation (Progressive Loading):
  PromptEngineer loads: anti-bloat-framework.md (~2,100 tokens)
  Finds edge case guidance: Immediate decision capability
  Outcome: Autonomous resolution without escalation
  Efficiency: Slightly higher token load but no delay, comprehensive understanding

Value: Documentation enables autonomous complex decision-making
```

### Resource Access Efficiency Patterns

**Selective Loading Based on Need:**

**Scenario 1: Experienced Agent, Standard Workflow**
```yaml
Agent: BackendSpecialist (has used working-directory-coordination 20+ times)
Task: Create working directory artifact for API implementation plan

Phase 1 - Discovery: ~100 tokens (scans metadata, identifies skill)
Phase 2 - Invocation: ~2,500 tokens (loads SKILL.md for workflow refresh)
Phase 3 - Resources: ~0 tokens (agent familiar with format, no template needed)

Total Context: ~2,600 tokens
Efficiency: Agent executes workflow from memory + SKILL.md refresher
```

**Scenario 2: New Agent, First-Time Workflow**
```yaml
Agent: SecurityAuditor (first time using working-directory-coordination)
Task: Create working directory security assessment report

Phase 1 - Discovery: ~100 tokens (scans metadata, identifies skill)
Phase 2 - Invocation: ~2,500 tokens (loads SKILL.md for complete workflow)
Phase 3 - Resources:
  - Template: immediate-reporting-template.md (~320 tokens) for exact format
  - Example: security-assessment-artifact.md (~900 tokens) for realistic demonstration
  Resources Total: ~1,220 tokens

Total Context: ~3,820 tokens
Efficiency: Agent learns workflow comprehensively with format and pattern guidance
```

**Scenario 3: Complex Edge Case**
```yaml
Agent: PromptEngineer (creating skill with edge case scope decision)
Task: Validate anti-bloat framework application for 1 primary + 2 secondary agents

Phase 1 - Discovery: ~100 tokens (scans metadata, identifies skill-creation)
Phase 2 - Invocation: ~3,600 tokens (loads SKILL.md for Phase 1 scope definition)
Phase 3 - Resources:
  - Documentation: anti-bloat-framework.md (~2,100 tokens) for edge case guidance
  Resources Total: ~2,100 tokens

Total Context: ~5,800 tokens
Efficiency: Comprehensive understanding enables autonomous complex decision without escalation
```

**Resource Loading Optimization Insights:**

1. **Not All Resources Loaded:** Phase 3 is selective based on agent needs
2. **Experience Reduces Loading:** Repeat skill users load fewer resources over time
3. **Complex Scenarios Load More:** Edge cases or first-time usage triggers documentation loading
4. **Template vs. Example vs. Documentation:** Different token budgets serve different needs

---

## Token Efficiency Measurement

### Methodologies for Calculating Token Budgets

Accurate token measurement is essential for validating progressive loading efficiency. Several approaches exist with varying precision levels:

### Method 1: Line Count Estimation (Quick, ~85% Accurate)

**Formula:** `Estimated Tokens ≈ Line Count × 8 tokens/line`

**Rationale:**
- Average English prose: ~10-12 tokens per line
- Markdown with code blocks: ~6-8 tokens per line
- YAML/structured content: ~4-6 tokens per line
- **Blended Average for Skills:** ~8 tokens per line

**Application:**
```yaml
SKILL.md Token Estimation:
  Total Lines: 450 lines
  Calculation: 450 × 8 = 3,600 tokens
  Actual (if measured precisely): ~3,450-3,750 tokens
  Accuracy: ±5-10%

Template Token Estimation:
  Total Lines: 35 lines
  Calculation: 35 × 8 = 280 tokens
  Actual: ~260-300 tokens
  Accuracy: ±7%

Documentation Token Estimation:
  Total Lines: 280 lines
  Calculation: 280 × 8 = 2,240 tokens
  Actual: ~2,100-2,400 tokens
  Accuracy: ±6%
```

**Use Case:** Quick validation during skill creation - "Am I within my token budget category?"

**Example from coordination-skill-example.md:**
```
working-directory-coordination SKILL.md:
  Drafted Content: 312 lines
  Estimation: 312 × 8 = 2,496 tokens
  Budget for Coordination Skill: 2,000-3,500 tokens
  Validation: ✅ Within budget (trending toward lower end)
  Decision: Can add ~1,000 tokens (125 lines) more detail if needed
```

### Method 2: Character Count Conversion (~90% Accurate)

**Formula:** `Estimated Tokens ≈ Character Count ÷ 4 characters/token`

**Rationale:**
- OpenAI tokenization: Average ~4 characters per token for English text
- Markdown syntax overhead: Slightly fewer characters per token (~3.5-4)
- Code blocks: More characters per token (~4.5-5)
- **Blended Average:** ~4 characters per token

**Application:**
```yaml
SKILL.md Character Count:
  File Size: 14,850 characters (from wc -m)
  Calculation: 14,850 ÷ 4 = 3,712 tokens
  Accuracy: ±5%

YAML Frontmatter Character Count:
  Description: 245 characters
  Full Frontmatter: 280 characters
  Calculation: 280 ÷ 4 = 70 tokens
  Accuracy: ±5% (closer to 65-75 actual tokens)
```

**Use Case:** More precise validation when line count varies significantly (heavily formatted content)

**Bash Command for Character Count:**
```bash
wc -m /path/to/SKILL.md
# Output: 14850 /path/to/SKILL.md
# Divide by 4: ~3,712 tokens
```

### Method 3: Word Count Approximation (~80% Accurate, Roughest)

**Formula:** `Estimated Tokens ≈ Word Count × 1.3 tokens/word`

**Rationale:**
- English prose: Average word length ~5 characters
- Tokenization: Average token ~4 characters
- **Ratio:** 1 word ≈ 1.3 tokens (accounts for punctuation, syntax)

**Application:**
```yaml
SKILL.md Word Count:
  Total Words: 2,850 words (from wc -w)
  Calculation: 2,850 × 1.3 = 3,705 tokens
  Accuracy: ±10-15% (varies with technical density)
```

**Use Case:** Extremely quick estimate, less reliable for technical content with code blocks

**Bash Command:**
```bash
wc -w /path/to/SKILL.md
# Output: 2850 /path/to/SKILL.md
# Multiply by 1.3: ~3,705 tokens
```

### Method 4: Section-Specific Measurement (Most Accurate, ~95%)

**Approach:** Measure different sections with category-appropriate multipliers

**Section-Specific Multipliers:**

| Section Type | Multiplier | Rationale |
|--------------|------------|-----------|
| YAML Frontmatter | 6 tokens/line | Structured syntax, less dense than prose |
| Headers (# ##) | 4 tokens/line | Short, minimal content |
| Prose Paragraphs | 10 tokens/line | Standard English text |
| Code Blocks | 6 tokens/line | Syntax overhead, less tokenization |
| Lists/Bullets | 8 tokens/line | Mixed structure and content |
| Tables | 7 tokens/line | Structured with moderate density |

**Application:**
```yaml
SKILL.md Section Breakdown:
  YAML Frontmatter: 5 lines × 6 = 30 tokens
  Headers: 20 lines × 4 = 80 tokens
  Prose Paragraphs: 180 lines × 10 = 1,800 tokens
  Code Blocks: 40 lines × 6 = 240 tokens
  Lists/Bullets: 90 lines × 8 = 720 tokens
  Tables: 15 lines × 7 = 105 tokens
  Total: 350 lines = 2,975 tokens

  Accuracy: ±2-5% (most precise without actual tokenization)
```

**Use Case:** Final validation before skill deployment, ensuring precision within budget

### Method 5: Actual Tokenization (100% Accurate, External Tool Required)

**Approach:** Use OpenAI tiktoken library or online tokenizer

**Python Example:**
```python
import tiktoken

# Load appropriate encoding (Claude uses similar to GPT-4)
encoding = tiktoken.get_encoding("cl100k_base")

# Read skill file
with open("/path/to/SKILL.md", "r") as f:
    content = f.read()

# Count tokens
tokens = encoding.encode(content)
token_count = len(tokens)

print(f"Exact token count: {token_count}")
```

**Use Case:** Absolute precision for critical validation, though typically unnecessary for skill creation (estimation methods sufficiently accurate)

### Practical Token Measurement Workflow

**During Skill Creation (Phase 2-3):**

1. **Initial Draft:** Use line count estimation (Method 1)
   - Quick validation: "Am I trending toward budget ceiling?"
   - Decision point: Extract content to resources if exceeding budget

2. **Pre-Resource Organization:** Use character count (Method 2)
   - More precise validation before resource extraction decisions
   - Determines how much content can stay in SKILL.md vs. move to resources/

3. **Final Validation:** Use section-specific measurement (Method 4)
   - Precise budget confirmation before deployment
   - Validates skill fits category expectations

**From Existing Examples:**

**coordination-skill-example.md Token Measurement:**
```yaml
working-directory-coordination SKILL.md:
  Method 1 (Line Count): 312 lines × 8 = 2,496 tokens
  Method 2 (Character Count): 10,240 characters ÷ 4 = 2,560 tokens
  Method 4 (Section-Specific):
    YAML: 5 lines × 6 = 30 tokens
    Headers: 18 lines × 4 = 72 tokens
    Prose: 165 lines × 10 = 1,650 tokens
    Lists: 85 lines × 8 = 680 tokens
    Code Blocks: 25 lines × 6 = 150 tokens
    Total: 2,582 tokens

  Budget: 2,000-3,500 tokens (coordination skill)
  Status: ✅ Within budget, room for expansion if needed
```

**technical-skill-example.md Token Measurement:**
```yaml
api-design-patterns SKILL.md:
  Method 1 (Line Count): 562 lines × 8 = 4,496 tokens
  Method 2 (Character Count): 18,650 characters ÷ 4 = 4,662 tokens
  Method 4 (Section-Specific):
    YAML: 6 lines × 6 = 36 tokens
    Headers: 28 lines × 4 = 112 tokens
    Prose: 245 lines × 10 = 2,450 tokens
    Lists: 120 lines × 8 = 960 tokens
    Code Blocks: 95 lines × 6 = 570 tokens
    Tables: 18 lines × 7 = 126 tokens
    Total: 4,254 tokens

  Budget: 3,000-5,000 tokens (technical skill)
  Status: ✅ Within budget, trending toward middle-upper range (appropriate for deep technical content)
```

### Token Savings Calculation

**Formula for Skill Extraction Efficiency:**

```
Token Savings = (Embedded Pattern Tokens) - (Skill Reference Tokens)
Efficiency % = (Token Savings ÷ Embedded Pattern Tokens) × 100
```

**Application:**

**Before Extraction (Embedded in Agent):**
```yaml
BackendSpecialist Definition:
  Working Directory Protocol: 150 tokens (embedded)
  API Design Patterns: 500 tokens (embedded)
  Total Always-Loaded: 650 tokens
```

**After Extraction (Skill References):**
```yaml
BackendSpecialist Definition:
  working-directory-coordination reference: 20 tokens
  api-design-patterns reference: 24 tokens
  Total Always-Loaded: 44 tokens

Skill Loading (When Invoked):
  working-directory-coordination SKILL.md: ~2,580 tokens (loaded when needed)
  api-design-patterns SKILL.md: ~4,254 tokens (loaded when needed)
```

**Efficiency Calculation:**
```yaml
Scenario 1: Task requires both skills
  Before: 650 tokens (always loaded)
  After: 44 tokens (references) + 2,580 (SKILL 1) + 4,254 (SKILL 2) = 6,878 tokens
  Analysis: Higher context load BUT agent has comprehensive guidance (embedded patterns were insufficient for execution)
  Real Comparison: Embedded patterns + external documentation lookups (~3,000 tokens) = Still ~3,878 tokens saved

Scenario 2: Task requires only working-directory-coordination
  Before: 650 tokens (always loaded, api-design wasted)
  After: 44 tokens (references) + 2,580 (SKILL 1 only) = 2,624 tokens
  Savings: 650 - 2,624 = Actually slightly higher, BUT consider...
  Real Before: 650 embedded + template lookup (~400 tokens) = 1,050 tokens
  Real After: 2,624 tokens (comprehensive)
  Trade-off: Slightly higher tokens but drastically better execution quality

Scenario 3: Task requires neither skill (simple bug fix)
  Before: 650 tokens (always loaded, completely wasted)
  After: 44 tokens (references only, skills never invoked)
  Savings: 650 - 44 = 606 tokens
  Efficiency: 93% reduction
```

**Key Insight from Token Efficiency Measurement:**

Progressive loading optimizes for:
1. **Minimal Base Load:** Agent definitions slim with references only
2. **Comprehensive Execution:** When skills needed, full guidance available
3. **Selective Loading:** Only relevant skills invoked for current task
4. **Quality over Minimal Tokens:** Slight token increase for comprehensive guidance worth trade-off vs. incomplete embedded patterns

---

## Section Ordering Optimization

### Progressive Disclosure Within SKILL.md Files

Strategic section ordering enables agents to:
1. Quickly confirm skill relevance (lines 1-80)
2. Execute workflow without loading resources (lines 81-300)
3. Access advanced content only when needed (lines 301-500)

This mirrors the **three-phase progressive loading** at a micro level within individual SKILL.md files.

### Optimal SKILL.md Section Order

**Tier 1: Critical Confirmation Content (Lines 1-80, ~600 tokens)**

**Priority Ranking:**
1. **YAML Frontmatter** (lines 1-6)
   - Already scanned in Phase 1, reconfirms identity
   - No additional token load (already counted in discovery)

2. **Title and Introduction** (lines 8-11)
   - 2-3 line overview contextualizing skill purpose
   - Confirms agent loaded correct skill
   - ~30-50 tokens

3. **PURPOSE Section** (lines 13-35)
   - Core Mission: 1-2 sentences explaining primary responsibility
   - Why This Matters: 1-2 sentences on value and consequences
   - Mandatory Application: When required vs. optional, exceptions
   - ~180-240 tokens

4. **WHEN TO USE Section** (lines 37-78)
   - 3-5 primary scenarios with Trigger/Action/Rationale structure
   - Enables agent to identify which workflow steps apply
   - ~300-350 tokens

**Rationale for Tier 1 Ordering:**
Agent reading lines 1-80 can answer:
- Is this the right skill for my task? (Title, Purpose)
- Which scenarios apply to my current work? (When to Use)
- Should I continue reading or is this not relevant? (Decision point at line 80)

**Tier 2: Execution Guidance Content (Lines 81-300, ~1,800 tokens)**

**Priority Ranking:**
5. **WORKFLOW STEPS Section** (lines 82-240)
   - Detailed process for each "When to Use" scenario
   - Sub-steps with specific actions
   - Checklists for completion validation
   - Resource references for templates/examples
   - ~1,200-1,400 tokens

6. **TARGET AGENTS Section** (lines 242-275)
   - Primary Users: Which agents use this skill and how
   - Secondary Users: Optional consumers
   - Integration patterns with agent workflows
   - ~240-300 tokens

**Rationale for Tier 2 Ordering:**
Agent reading lines 81-300 can:
- Execute complete workflow steps (WORKFLOW STEPS)
- Understand integration with team (TARGET AGENTS)
- Identify resource needs (references throughout)

**Tier 3: Advanced and Reference Content (Lines 301-500, ~1,600 tokens)**

**Priority Ranking:**
7. **RESOURCES Section** (lines 302-380)
   - Templates overview with usage triggers
   - Examples overview with scenarios demonstrated
   - Documentation overview with deep dive topics
   - ~580-650 tokens

8. **INTEGRATION WITH TEAM WORKFLOWS Section** (lines 382-425)
   - Multi-Agent Coordination Patterns
   - Claude's Orchestration Enhancement
   - Quality Gate Integration
   - CLAUDE.md Integration
   - ~320-380 tokens

9. **SUCCESS METRICS Section** (lines 427-455, Optional)
   - Measurable outcomes validating skill execution
   - Efficiency targets
   - ~200-240 tokens

10. **TROUBLESHOOTING Section** (lines 457-500, Optional)
    - Common issues with symptoms/solutions
    - Escalation paths
    - ~300-400 tokens

**Rationale for Tier 3 Ordering:**
Agent reading lines 301-500 gains:
- Resource navigation (RESOURCES)
- Advanced coordination understanding (INTEGRATION)
- Validation criteria (SUCCESS METRICS)
- Problem resolution (TROUBLESHOOTING)

### Anti-Patterns to Avoid

**❌ Back-Loading Critical Content:**
```markdown
# Skill Name

## WORKFLOW STEPS
[300 lines of detailed workflow]

## PURPOSE
Why this skill matters...

## WHEN TO USE
Scenarios where...
```
**Problem:** Agent reads 300 lines before confirming skill relevance
**Efficiency Loss:** ~2,400 tokens processed before decision point

**✅ Front-Loading Critical Content:**
```markdown
# Skill Name

## PURPOSE
Why this skill matters...

## WHEN TO USE
Scenarios where...

## WORKFLOW STEPS
[300 lines of detailed workflow]
```
**Benefit:** Agent reads ~80 lines (~600 tokens) to confirm relevance, then proceeds to workflow
**Efficiency Gain:** Early decision point prevents unnecessary reading

**❌ Interleaving Resources with Workflow:**
```markdown
## WORKFLOW STEPS

### Step 1: First Action
Process details...

## RESOURCES
Templates available...

### Step 2: Second Action
Process details...
```
**Problem:** Agent must jump between sections, resources interrupt workflow flow
**Usability:** Confusing navigation, unclear section boundaries

**✅ Consolidated Workflow, Separate Resources:**
```markdown
## WORKFLOW STEPS

### Step 1: First Action
Process details...
**Resource:** See `resources/templates/template-1.md`

### Step 2: Second Action
Process details...
**Resource:** See `resources/templates/template-2.md`

## RESOURCES

### Templates
- **template-1.md**: [Description]
- **template-2.md**: [Description]
```
**Benefit:** Workflow flows sequentially, resources referenced inline then detailed in dedicated section
**Usability:** Clear structure, easy navigation

### Section Ordering Examples from Existing Skills

**From coordination-skill-example.md (working-directory-coordination):**
```yaml
Actual Section Order:
  1. YAML Frontmatter (lines 1-6)
  2. Title + Introduction (lines 8-11)
  3. PURPOSE (lines 13-31)
  4. WHEN TO USE (lines 33-72)
  5. WORKFLOW STEPS (lines 74-215)
  6. TARGET AGENTS (lines 217-248)
  7. RESOURCES (lines 250-305)
  8. INTEGRATION WITH TEAM WORKFLOWS (lines 307-342)
  9. SUCCESS METRICS (lines 344-368)
  10. TROUBLESHOOTING (lines 370-410)

Tier Breakdown:
  Tier 1 (Confirmation): Lines 1-72 (~550 tokens)
  Tier 2 (Execution): Lines 74-248 (~1,400 tokens)
  Tier 3 (Advanced): Lines 250-410 (~1,280 tokens)
  Total: 410 lines (~3,230 tokens)

Progressive Disclosure Effectiveness:
  ✅ Agent can confirm relevance in ~550 tokens (Tier 1)
  ✅ Agent can execute workflow in ~1,950 tokens (Tier 1 + 2)
  ✅ Agent accesses advanced content optionally (~3,230 tokens total)
```

**From technical-skill-example.md (api-design-patterns):**
```yaml
Actual Section Order:
  1. YAML Frontmatter (lines 1-7)
  2. Title + Introduction (lines 9-13)
  3. PURPOSE (lines 15-38)
  4. WHEN TO USE (lines 40-92)
  5. WORKFLOW STEPS (lines 94-310)
  6. TARGET AGENTS (lines 312-355)
  7. RESOURCES (lines 357-445)
  8. INTEGRATION WITH TEAM WORKFLOWS (lines 447-495)
  9. SUCCESS METRICS (lines 497-530)
  10. TROUBLESHOOTING (lines 532-580)

Tier Breakdown:
  Tier 1 (Confirmation): Lines 1-92 (~720 tokens)
  Tier 2 (Execution): Lines 94-355 (~2,100 tokens)
  Tier 3 (Advanced): Lines 357-580 (~1,780 tokens)
  Total: 580 lines (~4,600 tokens)

Progressive Disclosure Effectiveness:
  ✅ Agent can confirm relevance in ~720 tokens (Tier 1)
  ✅ Agent can execute workflow in ~2,820 tokens (Tier 1 + 2)
  ✅ Agent accesses extensive resources optionally (~4,600 tokens total)

Note: Technical skill has higher Tier 2 token count due to comprehensive workflow depth
```

### Optimization Validation Checklist

Use this checklist when ordering sections in new SKILL.md files:

- [ ] **YAML Frontmatter first** (lines 1-6, identity confirmation)
- [ ] **Title + Introduction within first 15 lines** (skill context established)
- [ ] **PURPOSE before WHEN TO USE** (understand "why" before "when")
- [ ] **WHEN TO USE before WORKFLOW STEPS** (identify scenarios before executing)
- [ ] **WORKFLOW STEPS consolidated** (all process guidance together, not scattered)
- [ ] **TARGET AGENTS before RESOURCES** (understand who uses before accessing details)
- [ ] **RESOURCES as dedicated section** (not interleaved with workflow)
- [ ] **INTEGRATION and SUCCESS METRICS in Tier 3** (advanced content, optional for basic execution)
- [ ] **TROUBLESHOOTING last** (accessed only when issues encountered)
- [ ] **Tier 1 < 80 lines** (~600 tokens, confirmation content)
- [ ] **Tier 2 < 300 cumulative lines** (~2,400 tokens, execution content)
- [ ] **Tier 3 remaining lines** (~1,600 tokens, advanced content)

---

## Content Extraction Decision Framework

### When to Extract Content from SKILL.md to Resources

As skills evolve, SKILL.md files can bloat beyond 5,000 token budgets. This framework helps determine what content stays in SKILL.md vs. moves to resources/.

### Extraction Decision Matrix

| Content Type | Token Density | Frequency of Use | Extraction Decision |
|--------------|---------------|------------------|---------------------|
| Workflow step overview | Moderate | Every execution | KEEP in SKILL.md |
| Detailed sub-process | High | Every execution | KEEP in SKILL.md |
| Exact format specification | High | First-time/uncertain | EXTRACT to templates/ |
| Realistic scenario | Very High | Understanding/validation | EXTRACT to examples/ |
| Conceptual deep dive | Very High | Edge cases only | EXTRACT to documentation/ |
| Troubleshooting details | High | Problem scenarios | EXTRACT to documentation/ |
| Code examples | High | Reference only | EXTRACT to examples/ |
| Decision trees/matrices | Moderate | Decision points | KEEP in SKILL.md (if <300 tokens) |
| Checklists | Low-Moderate | Validation | KEEP in SKILL.md |
| Resource references | Low | Navigation | KEEP in SKILL.md |

### Extraction Triggers

**Trigger 1: SKILL.md Exceeds Category Token Budget**
```yaml
Scenario:
  Skill Category: Coordination
  Token Budget: 2,000-3,500 tokens
  Current SKILL.md: 4,200 tokens (OVER BUDGET)

Action:
  1. Identify highest token-density sections not critical to every execution
  2. Extract to resources/ with appropriate categorization
  3. Replace in SKILL.md with summary + resource reference
  4. Validate post-extraction: SKILL.md within 2,000-3,500 tokens

Example:
  Section: "Common Issues and Resolutions" (850 tokens)
  Analysis: Accessed only when problems occur, not every execution
  Extraction: Move to resources/documentation/troubleshooting-coordination.md
  SKILL.md Replacement: "See resources/documentation/troubleshooting-coordination.md for issue resolution"
  Token Savings: 850 - 20 (reference) = 830 tokens saved
```

**Trigger 2: Detailed Format Specifications Embedded**
```yaml
Scenario:
  SKILL.md includes: Complete format specification with 15 field descriptions (420 tokens)
  Workflow Step: "Report using standardized format"

Action:
  Extract format to template with placeholders and guidance
  SKILL.md Reference: "**Resource:** See resources/templates/format-template.md"

Example:
  Before Extraction (in SKILL.md):
    ```markdown
    Report Format:
    - Field 1: {{DESCRIPTION}} - [3 lines of guidance]
    - Field 2: {{DESCRIPTION}} - [3 lines of guidance]
    ...
    - Field 15: {{DESCRIPTION}} - [3 lines of guidance]
    Total: 420 tokens
    ```

  After Extraction (in SKILL.md):
    ```markdown
    **Resource:** See `resources/templates/standardized-report-template.md` for complete format
    Total: 18 tokens
    ```

  In resources/templates/standardized-report-template.md:
    [Complete format with all 15 fields and guidance]
    Total: 420 tokens (loaded only when needed)

  Efficiency: SKILL.md reduced 402 tokens, template available on-demand
```

**Trigger 3: Multiple Realistic Scenarios Included**
```yaml
Scenario:
  SKILL.md includes: 3 complete workflow examples (2,100 tokens total)
  Purpose: Demonstrate workflow in different contexts

Action:
  Extract examples to resources/examples/ with one example per file
  SKILL.md Reference: List examples in RESOURCES section

Example:
  Before Extraction (in SKILL.md):
    Example 1: Backend Specialist Coordination (700 tokens)
    Example 2: Multi-Agent Workflow (750 tokens)
    Example 3: Security Assessment Artifact (650 tokens)
    Total: 2,100 tokens

  After Extraction (in SKILL.md):
    RESOURCES Section:
    - backend-specialist-coordination.md: [1-line description]
    - multi-agent-workflow.md: [1-line description]
    - security-assessment-artifact.md: [1-line description]
    Total: ~85 tokens for references

  In resources/examples/:
    - backend-specialist-coordination.md (700 tokens)
    - multi-agent-workflow.md (750 tokens)
    - security-assessment-artifact.md (650 tokens)
    Each loaded individually when needed

  Efficiency: SKILL.md reduced 2,015 tokens, examples selectively loaded
```

**Trigger 4: Conceptual Philosophy Sections**
```yaml
Scenario:
  SKILL.md includes: "Progressive Loading Architecture Philosophy" (1,200 tokens)
  Purpose: Explain design rationale and principles

Action:
  Extract philosophy to resources/documentation/
  SKILL.md: Keep 2-3 line summary, reference documentation for deep dive

Example:
  Before Extraction (in SKILL.md):
    ## Progressive Loading Philosophy
    [12 paragraphs explaining architecture rationale, token efficiency cascade, comparison with embedded approach]
    Total: 1,200 tokens

  After Extraction (in SKILL.md):
    Progressive loading optimizes context efficiency through metadata discovery, instruction invocation, and on-demand resource access. See `resources/documentation/progressive-loading-architecture.md` for comprehensive design philosophy.
    Total: ~45 tokens

  In resources/documentation/progressive-loading-architecture.md:
    [Complete 12-paragraph philosophy with examples, comparisons, efficiency analysis]
    Total: 1,200 tokens (loaded only when deep understanding needed)

  Efficiency: SKILL.md reduced 1,155 tokens, philosophy available for those seeking it
```

### Extraction Best Practices

**1. Maintain SKILL.md Workflow Completeness:**
After extraction, agent must still be able to execute workflow from SKILL.md alone. Resources should enhance, not replace workflow guidance.

**✅ Good Extraction:**
```markdown
## WORKFLOW STEPS

### Step 1: Create Artifact Report

**Process:**
1. Scan working directory for existing artifacts
2. Identify relevant context
3. Report using standardized format (see resource reference below)
4. Validate completeness

**Resource:** See `resources/templates/artifact-report-template.md` for exact format specification
```

Agent can execute Step 1 from SKILL.md (knows what to do), loads template if unsure of exact format.

**❌ Bad Extraction:**
```markdown
## WORKFLOW STEPS

### Step 1: Create Artifact Report

**Resource:** See `resources/templates/artifact-report-template.md` for process
```

Agent cannot execute from SKILL.md - workflow completely outsourced to template. Template should provide format, not process.

**2. Resource References Inline with Workflow:**
Reference resources at the point in workflow where agent would use them, not only in RESOURCES section.

**✅ Good Reference Placement:**
```markdown
### Step 2: Format Output

Create report following standardized structure:
- Header with artifact identification
- Content summary
- Next actions

**Resource:** See `resources/templates/report-format.md` for complete specification

**Checklist:**
- [ ] All sections included
- [ ] Format matches template
```

**❌ Bad Reference Placement:**
```markdown
### Step 2: Format Output

Create report following standardized structure.

[No resource reference here]

...

## RESOURCES
- report-format.md: Complete format specification
```

Agent must read entire SKILL.md, reach RESOURCES section, then return to Step 2 - inefficient navigation.

**3. Categorize Resources Appropriately:**

- **Templates:** Use for exact formats, specifications, standardized outputs
- **Examples:** Use for realistic scenarios, complete workflows, pattern demonstrations
- **Documentation:** Use for philosophy, troubleshooting, optimization techniques

Don't mix categories (e.g., example masquerading as template, documentation embedded in template).

**4. Avoid Nested Extraction:**

Resources should be one level deep from SKILL.md. Don't create resources that reference other resources.

**❌ Bad Nested Reference:**
```markdown
In SKILL.md:
  **Resource:** See `resources/templates/report-template.md`

In report-template.md:
  For examples of this format, see `resources/examples/report-example.md`
```

Agent must load SKILL.md → template → example (three-level loading).

**✅ Good Direct Reference:**
```markdown
In SKILL.md:
  **Resource:** See `resources/templates/report-template.md` for format
  **Resource:** See `resources/examples/report-example.md` for realistic demonstration
```

Agent loads SKILL.md → template OR example (two-level loading, parallel not serial).

---

## Multi-Agent Ecosystem Efficiency

### Scaling Progressive Loading Across 12-Agent Team

Progressive loading's value compounds when applied ecosystem-wide. This section analyzes efficiency gains across zarichney-api's entire multi-agent team.

### Ecosystem Efficiency Calculation

**Baseline: Embedded Approach (Pre-Progressive Loading)**

```yaml
Team Composition: 12 agents
  - Claude (Codebase Manager): 2,200 tokens
  - CodeChanger: 2,850 tokens
  - TestEngineer: 2,900 tokens
  - DocumentationMaintainer: 2,750 tokens
  - PromptEngineer: 3,200 tokens
  - ComplianceOfficer: 2,950 tokens
  - BackendSpecialist: 3,100 tokens
  - FrontendSpecialist: 3,050 tokens
  - SecurityAuditor: 2,900 tokens
  - WorkflowEngineer: 2,850 tokens
  - BugInvestigator: 2,650 tokens
  - ArchitecturalAnalyst: 2,800 tokens

Total Baseline Context: 34,200 tokens

Context Window: 200,000 tokens
Remaining for Orchestration: 165,800 tokens (82.9% available)

Simultaneous Agent Capacity: 200,000 ÷ 2,850 (average agent) ≈ 70 agents
  (Practical limit: ~5-6 agents simultaneously due to task context overhead)
```

**Progressive Loading: Skill Reference Approach**

```yaml
Team After Skill Extraction:

Agent Definition Reductions:
  - Working directory protocol: 150 tokens embedded → 20 token reference (87% reduction)
  - Documentation grounding: 150 tokens embedded → 20 token reference (87% reduction)
  - Core issue focus: 120 tokens embedded → 18 token reference (85% reduction)
  - Domain technical skills: 400-500 tokens embedded → 20-24 token reference (95% reduction)

Average Reduction per Agent: 820 tokens (from embedded patterns → skill references)

Revised Agent Sizes:
  - Claude: 2,200 - 420 = 1,780 tokens
  - CodeChanger: 2,850 - 820 = 2,030 tokens
  - TestEngineer: 2,900 - 850 = 2,050 tokens
  - DocumentationMaintainer: 2,750 - 800 = 1,950 tokens
  - PromptEngineer: 3,200 - 900 = 2,300 tokens (more meta-skills)
  - ComplianceOfficer: 2,950 - 870 = 2,080 tokens
  - BackendSpecialist: 3,100 - 920 = 2,180 tokens
  - FrontendSpecialist: 3,050 - 900 = 2,150 tokens
  - SecurityAuditor: 2,900 - 850 = 2,050 tokens
  - WorkflowEngineer: 2,850 - 820 = 2,030 tokens
  - BugInvestigator: 2,650 - 780 = 1,870 tokens
  - ArchitecturalAnalyst: 2,800 - 810 = 1,990 tokens

Total Revised Context: 24,460 tokens

Ecosystem Savings: 34,200 - 24,460 = 9,740 tokens (28.5% reduction)

Context Window: 200,000 tokens
Remaining for Orchestration: 175,540 tokens (87.8% available)
Efficiency Gain: +9,740 tokens = 5.9% more context window available

Simultaneous Agent Capacity: 200,000 ÷ 2,037 (average revised agent) ≈ 98 agents
  (Practical: ~8-9 agents simultaneously with task context overhead)
  Improvement: +40% more simultaneous agent capacity
```

### Skill Invocation Overhead Analysis

**Critical Question:** Don't skills loaded on-demand negate token savings?

**Answer:** Selective loading + reusability across agents = Net efficiency gain

**Scenario 1: Complex Multi-Agent Task (API Implementation)**
```yaml
Agents Involved: BackendSpecialist, FrontendSpecialist, TestEngineer, DocumentationMaintainer

Baseline (Embedded Approach):
  Agent Definitions: 4 × 2,900 (average) = 11,600 tokens
  Task Context: Issue, files, standards = ~30,000 tokens
  Total Context: 41,600 tokens

Progressive Loading (Skill Reference Approach):
  Agent Definitions: 4 × 2,053 (average revised) = 8,212 tokens
  Invoked Skills:
    - working-directory-coordination: ~2,580 tokens (used by all 4 agents, loaded once)
    - api-design-patterns: ~4,250 tokens (used by Backend + Frontend, loaded once)
    - test-architecture-best-practices: ~3,800 tokens (used by TestEngineer, loaded once)
  Skill Context: 2,580 + 4,250 + 3,800 = 10,630 tokens
  Task Context: ~30,000 tokens (same as baseline)
  Total Context: 8,212 + 10,630 + 30,000 = 48,842 tokens

Comparison:
  Progressive Loading: 48,842 tokens
  Baseline: 41,600 tokens
  Apparent Overhead: +7,242 tokens (17% increase)

But Consider:
  Baseline Embedded Patterns: Insufficient detail for complex task execution
  Real Baseline: 41,600 + documentation lookups (~8,000 tokens) = 49,600 tokens
  Real Progressive: 48,842 tokens
  Actual Savings: 758 tokens + comprehensive guidance without external lookups
```

**Scenario 2: Simple Task (Bug Fix)**
```yaml
Agents Involved: CodeChanger only

Baseline (Embedded Approach):
  Agent Definition: 2,850 tokens
  Task Context: ~5,000 tokens
  Total Context: 7,850 tokens

Progressive Loading (Skill Reference Approach):
  Agent Definition: 2,030 tokens
  Invoked Skills: None (simple bug fix doesn't need working directory artifacts or domain patterns)
  Task Context: ~5,000 tokens
  Total Context: 7,030 tokens

Savings: 820 tokens (10% reduction)
Efficiency: Skills not invoked = Zero overhead for simple tasks
```

**Scenario 3: Skill Creation Task (Meta-Capability)**
```yaml
Agents Involved: PromptEngineer

Baseline (Embedded Approach):
  Agent Definition: 3,200 tokens (includes embedded skill creation methodology)
  Task Context: ~8,000 tokens
  Skill Creation Documentation: ~12,000 tokens (external references, scattered guidance)
  Total Context: 23,200 tokens

Progressive Loading (Skill Reference Approach):
  Agent Definition: 2,300 tokens
  Invoked Skills:
    - skill-creation: ~3,600 tokens (SKILL.md)
    - Resources loaded progressively:
      - skill-template.md: ~480 tokens
      - coordination-skill-example.md: ~1,350 tokens
      - progressive-loading-guide.md: ~2,400 tokens (this document)
  Skill Context: 3,600 + 480 + 1,350 + 2,400 = 7,830 tokens
  Task Context: ~8,000 tokens
  Total Context: 2,300 + 7,830 + 8,000 = 18,130 tokens

Savings: 23,200 - 18,130 = 5,070 tokens (22% reduction)
Efficiency: Resources loaded selectively (only 3 out of 9 total resources needed), organized, comprehensive
```

### Skill Reusability Multiplier Effect

**Critical Insight:** Skills used by multiple agents simultaneously only load once

**Example: working-directory-coordination Used by All Agents**
```yaml
Baseline (Embedded):
  12 agents × 150 tokens embedded = 1,800 tokens total across ecosystem

Progressive Loading (First Usage):
  12 agents × 20 token reference = 240 tokens in references
  1 × 2,580 token SKILL.md = 2,580 tokens (loaded once, reused by all)
  Total: 2,820 tokens

Apparent Overhead: 2,820 - 1,800 = 1,020 tokens (56% increase)

But Real Usage:
  Scenario: 3 agents engaged simultaneously (BackendSpecialist, FrontendSpecialist, TestEngineer)
  All 3 need working directory coordination

  Baseline: 3 × 150 = 450 tokens (loaded in each agent)
  Progressive: 3 × 20 = 60 tokens (references) + 2,580 tokens (SKILL.md loaded once, shared)
  Total Progressive: 2,640 tokens

  Comparison: 2,640 vs. 450 embedded

  Wait - this seems worse! But consider:

  Real Baseline: 450 embedded (insufficient) + template lookup across 3 agents (~900 tokens) = 1,350 tokens
  Real Progressive: 2,640 tokens (comprehensive, shared)

  Trade-off: +1,290 tokens BUT:
    - All 3 agents get complete guidance (not just 150 token summary)
    - Template/format included in SKILL.md (no separate lookups)
    - Consistent execution across all agents
    - Single source of truth (no divergence)

Key Point: Progressive loading optimizes for quality and consistency, not just token minimization
```

### When Progressive Loading Excels

**1. Simple Tasks Not Requiring Skills:**
Massive efficiency - agents load with references only, skills never invoked

**2. Diverse Tasks Across Agents:**
Different agents invoke different skills = Selective loading prevents universal bloat

**3. Skill Reference Without Invocation:**
Agent knows skill exists (from reference) but doesn't need it for current task = Awareness without overhead

**4. Multi-Agent Skill Sharing:**
Single skill loaded once, reused by multiple agents = Reduced redundancy

**5. Progressive Resource Loading:**
Agent loads SKILL.md but not all resources = Graduated context based on needs

### Ecosystem Health Metrics

**Skill Coverage Ratio:**
```yaml
Formula: (Agents Using Skill) ÷ (Total Agents) × 100

working-directory-coordination:
  Users: 12 agents
  Total: 12 agents
  Coverage: 100% (universal coordination skill)

api-design-patterns:
  Users: BackendSpecialist, FrontendSpecialist
  Total: 12 agents
  Coverage: 16.7% (domain-specific technical skill)

skill-creation:
  Users: PromptEngineer
  Total: 12 agents
  Coverage: 8.3% (exclusive meta-skill)

Interpretation:
  - High coverage (>50%): Coordination/documentation skills, reusability critical
  - Medium coverage (20-50%): Workflow skills, selective applicability
  - Low coverage (10-20%): Technical skills, domain-specific
  - Exclusive coverage (<10%): Meta-skills, single-agent capabilities
```

**Skill Bloat Detection:**
```yaml
Formula: (Skills with Coverage <20%) ÷ (Total Skills) × 100

Threshold: >30% indicates potential bloat (too many low-reuse skills)

Current Ecosystem:
  Total Skills: 15 (hypothetical)
  Coverage >50%: 3 skills (working-directory-coordination, documentation-grounding, core-issue-focus)
  Coverage 20-50%: 5 skills (workflow automation skills)
  Coverage 10-20%: 5 skills (domain technical skills)
  Coverage <10%: 2 skills (meta-skills for PromptEngineer)

  Bloat Ratio: 2 ÷ 15 = 13.3%
  Status: ✅ Healthy ecosystem (well below 30% threshold)
```

**Average Skill Invocation Rate:**
```yaml
Formula: (Skills Invoked per Task) ÷ (Total Available Skills) × 100

Scenario Analysis Across 10 Tasks:
  Task 1 (Bug Fix): 0 skills invoked
  Task 2 (API Implementation): 3 skills invoked
  Task 3 (Test Creation): 2 skills invoked
  Task 4 (Documentation Update): 1 skill invoked
  Task 5 (Security Audit): 2 skills invoked
  Task 6 (Skill Creation): 4 skills invoked
  Task 7 (Workflow Optimization): 2 skills invoked
  Task 8 (Architecture Review): 3 skills invoked
  Task 9 (Frontend Component): 2 skills invoked
  Task 10 (Backend Service): 3 skills invoked

  Total Skill Invocations: 22 skills across 10 tasks
  Average per Task: 2.2 skills
  Total Available: 15 skills
  Invocation Rate: 2.2 ÷ 15 = 14.7% per task

Interpretation:
  - Low rate (~10-15%): Selective loading, skills invoked only when needed
  - Medium rate (~20-30%): Moderate skill usage, may indicate skill discovery issues
  - High rate (>40%): Potential over-invocation, skills too generic or agents over-reliant

Status: ✅ Optimal selective loading (14.7% indicates precise skill targeting)
```

---

## Progressive Loading Patterns from Examples

### Consolidated Insights from Existing Skill Examples

This section extracts progressive loading patterns demonstrated in coordination-skill-example.md, technical-skill-example.md, and meta-skill-example.md, providing concrete strategies for new skill creation.

### Pattern 1: Tiered Resource Depth (From api-design-patterns)

**Source:** technical-skill-example.md

**Pattern:**
Technical skills require extensive resources. Structure resources in three tiers matching agent expertise levels:

**Tier 1 - Quick Reference Templates:**
- **Purpose:** Immediate format application for experienced agents
- **Token Budget:** 200-400 tokens per template
- **Examples:** API endpoint template, DTO structure template, error response template
- **Usage:** Agent familiar with API design needs format reminder, not conceptual guidance

**Tier 2 - Pattern Demonstration Examples:**
- **Purpose:** Realistic scenarios showing patterns in zarichney-api context
- **Token Budget:** 800-1,500 tokens per example
- **Examples:** REST endpoint implementation example, GraphQL schema design example
- **Usage:** Agent understanding API concepts but needs context-specific application guidance

**Tier 3 - Comprehensive Documentation:**
- **Purpose:** Deep dives into philosophy, trade-offs, optimization
- **Token Budget:** 2,000-3,000 tokens per document
- **Examples:** REST vs. GraphQL decision framework, API versioning strategy guide
- **Usage:** Architectural decisions requiring comprehensive understanding of options

**Application to New Technical Skills:**
```yaml
When creating technical skill:
  1. Identify core technical patterns (API design, test architecture, security frameworks)
  2. Create Tier 1 templates for each pattern's format specification
  3. Create Tier 2 examples demonstrating patterns in zarichney-api projects
  4. Create Tier 3 documentation for complex decision points and optimizations

  Result: Graduated resource depth serving experienced practitioners and learners alike
```

### Pattern 2: Mandatory Coordination Protocols (From working-directory-coordination)

**Source:** coordination-skill-example.md

**Pattern:**
Coordination skills enforce team-wide protocols. Structure workflow as MANDATORY requirements, not optional suggestions:

**Mandatory Workflow Structure:**
```markdown
## WHEN TO USE

### 1. Pre-Work Artifact Discovery (MANDATORY BEFORE ALL TASKS)
**Trigger:** Agent receives task assignment
**Action:** [Required action]
**Rationale:** [Why non-negotiable]

### 2. Immediate Artifact Reporting (MANDATORY WHEN CREATING FILES)
[Same structure]
```

**Enforcement Mechanisms:**
- **Trigger Specification:** "MANDATORY BEFORE ALL TASKS" leaves no ambiguity
- **No Exceptions Clause:** "Required For: All agents, No Exceptions" in Mandatory Application
- **Integration with Orchestration:** CLAUDE.md references coordination skill as non-negotiable team standard

**Application to New Coordination Skills:**
```yaml
When creating coordination skill:
  1. Identify truly universal team requirements (not domain-specific)
  2. Frame "When to Use" scenarios as MANDATORY with explicit triggers
  3. Include "No Exceptions" clause in Purpose > Mandatory Application
  4. Ensure CLAUDE.md orchestration enforces coordination skill usage

  Result: Consistent team execution, no protocol drift or selective compliance
```

### Pattern 3: Meta-Skill Comprehensive Resource Bundling (From skill-creation)

**Source:** meta-skill-example.md (this skill itself)

**Pattern:**
Meta-skills enable PromptEngineer capabilities. Bundle extensive resources covering entire creation lifecycle:

**Resource Bundle Structure:**
```yaml
Templates (Creation Phase Support):
  - Scope definition template (Phase 1)
  - Structure template (Phase 2)
  - Resource organization guide (Phase 4)

Examples (Complete Workflow Demonstrations):
  - Coordination skill creation (working-directory-coordination)
  - Technical skill creation (api-design-patterns)
  - Meta-skill creation (skill-creation itself)

Documentation (Deep Methodology):
  - Progressive loading architecture (design philosophy)
  - Anti-bloat framework (decision methodology)
  - Skill categorization guide (classification framework)
  - Agent integration patterns (implementation strategies)
```

**Comprehensive Coverage:**
- **Templates:** One template per creation phase enabling systematic execution
- **Examples:** One example per skill category demonstrating category-specific patterns
- **Documentation:** One document per complex concept requiring deep understanding

**Application to New Meta-Skills:**
```yaml
When creating meta-skill:
  1. Map creation workflow phases (for agent-creation: identity, structure, authority, skills, optimization)
  2. Create template for each phase's primary artifact
  3. Create complete example for each target category/variant
  4. Create documentation for each complex decision point or philosophy

  Result: PromptEngineer has complete toolkit for systematic creation without external lookups
```

### Pattern 4: Inline Resource References (From All Examples)

**Source:** All three examples consistently apply this pattern

**Pattern:**
Reference resources immediately after workflow step where agent would use them, not only in consolidated RESOURCES section:

**Inline Reference Structure:**
```markdown
### Step 2: Apply Standard Format

**Process:**
1. Review template structure
2. Fill placeholders with specific content
3. Validate completeness

**Resource:** See `resources/templates/format-template.md` for complete specification

**Checklist:**
- [ ] All placeholders replaced
- [ ] Format matches template
```

**Why This Works:**
- **Contextual Loading:** Agent loads resource at moment of need
- **Clear Triggers:** "See template for complete specification" provides explicit loading signal
- **Navigation Efficiency:** Agent doesn't hunt for resource reference elsewhere
- **Dual Reference Pattern:** Inline reference for immediate use + RESOURCES section for comprehensive overview

**Application to All New Skills:**
```yaml
For every workflow step:
  1. Identify point where agent might need template/example/documentation
  2. Add inline resource reference immediately after that decision point
  3. Use format: "**Resource:** See `resources/[category]/[filename].md` for [specific purpose]"
  4. Maintain consolidated RESOURCES section for comprehensive navigation

  Result: Resources easy to find and load at moment of use
```

### Pattern 5: Progressive Token Budgets by Skill Category (From All Examples)

**Source:** Pattern observed across all three skill categories

**Pattern:**
Different skill categories have different token budget targets reflecting content density needs:

**Category Token Budget Table:**
| Skill Category | SKILL.md Budget | Resource Depth | Total Potential Context |
|----------------|-----------------|----------------|-------------------------|
| Coordination (working-directory-coordination) | 2,000-3,500 tokens | Moderate (3-5 resources) | ~5,000-8,000 tokens |
| Technical (api-design-patterns) | 3,000-5,000 tokens | Extensive (8-12 resources) | ~12,000-18,000 tokens |
| Meta (skill-creation) | 3,500-5,000 tokens | Comprehensive (12-18 resources) | ~20,000-30,000 tokens |

**Rationale:**
- **Coordination Skills:** Process-focused, standardize workflows, moderate complexity
- **Technical Skills:** Domain expertise, deep patterns, architectural decisions
- **Meta-Skills:** Systematic frameworks, complete methodologies, extensive examples

**Application to Token Budget Planning:**
```yaml
When planning skill token allocation:
  1. Identify skill category (coordination/documentation/technical/meta/workflow)
  2. Apply category token budget to SKILL.md planning
  3. Resource depth scales with category complexity
  4. Validate total potential context (SKILL.md + all resources) appropriate for category

  Result: Token budgets match skill complexity and content depth requirements
```

### Pattern 6: Workflow Step Checklist Validation (From All Examples)

**Source:** Consistently applied across all workflow steps in all examples

**Pattern:**
Every workflow step includes completion checklist enabling agent self-validation:

**Checklist Structure:**
```markdown
### Step 3: Create Resource Files

**Process:**
[Detailed sub-steps]

**Checklist:**
- [ ] Template files created with placeholder guidance
- [ ] Example files demonstrate realistic scenarios
- [ ] Documentation files provide deep dives
- [ ] All resources referenced from SKILL.md
```

**Checklist Design Principles:**
- **Actionable Items:** Each checkbox represents testable completion criterion
- **Comprehensive Coverage:** Checklist covers all sub-steps in process
- **Self-Validation:** Agent can verify completion without external review
- **Quality Gates:** Critical requirements included (e.g., "All resources referenced from SKILL.md")

**Application to All Workflow Steps:**
```yaml
For every workflow step:
  1. Draft detailed process sub-steps
  2. Identify completion criteria for each sub-step
  3. Create checklist with 3-6 items covering all criteria
  4. Ensure checklist items are testable (agent can verify yes/no)

  Result: Agent validates workflow completion systematically, reduces oversight requirements
```

### Pattern 7: Category-Specific "When to Use" Scenarios (From All Examples)

**Source:** Pattern variations across three skill categories

**Pattern:**
"When to Use" scenarios tailored to skill category characteristics:

**Coordination Skill "When to Use" (Universal Triggers):**
```markdown
### 1. Pre-Work Artifact Discovery (MANDATORY BEFORE ALL TASKS)
**Trigger:** Agent receives task assignment from Claude
**Action:** Scan working directory for existing artifacts
**Rationale:** Build upon team context rather than duplicate effort
```
- **Characteristics:** Universal triggers (ALL agents, ALL tasks), mandatory framing, team-centric rationale

**Technical Skill "When to Use" (Domain-Specific Triggers):**
```markdown
### 1. Designing New REST Endpoint
**Trigger:** BackendSpecialist implementing new API functionality
**Action:** Apply REST design patterns from skill workflow
**Rationale:** Ensure consistency with existing API architecture
```
- **Characteristics:** Domain triggers (specific agents, specific technical tasks), optional framing, architectural consistency rationale

**Meta-Skill "When to Use" (Methodological Triggers):**
```markdown
### 1. Creating New Cross-Cutting Coordination Skill (PRIMARY USE CASE)
**Trigger:** Pattern used by 3+ agents requiring extraction
**Action:** Execute complete 5-phase workflow from scope definition through agent integration
**Rationale:** Coordination skills reduce redundancy across agent team
```
- **Characteristics:** Methodological triggers (creation scenarios, refactoring needs), framework application, systematic process rationale

**Application to Skill Category Design:**
```yaml
When crafting "When to Use" scenarios:
  Coordination Skills:
    - Universal triggers applicable to all agents
    - Mandatory framing ("MANDATORY BEFORE...")
    - Team-centric rationale (communication, consistency, awareness)

  Technical Skills:
    - Domain-specific triggers (agent + task type)
    - Optional framing (use when specific technical need arises)
    - Architectural rationale (consistency, best practices, patterns)

  Meta-Skills:
    - Methodological triggers (creation, refactoring, optimization scenarios)
    - Framework application framing (systematic process)
    - Capability-building rationale (enables future work, standardization)
```

---

## Token Budget Allocation Strategies

### Strategic Token Distribution Across Skill Components

Effective token budget allocation balances comprehensive guidance with context efficiency. This section provides decision frameworks for distributing tokens across SKILL.md sections and resources.

### SKILL.md Section Token Allocation

**Coordination Skill Token Budget: 2,000-3,500 Tokens**

**Recommended Allocation:**
```yaml
YAML Frontmatter: 80-100 tokens (5%)
  - Name + description optimization for discovery

Title + Introduction: 40-60 tokens (2%)
  - Brief contextualization

PURPOSE: 180-240 tokens (10%)
  - Core mission, why matters, mandatory application

WHEN TO USE: 300-400 tokens (15%)
  - 3-5 scenarios with triggers/actions/rationale

WORKFLOW STEPS: 1,200-1,600 tokens (50%)
  - Detailed processes, checklists, inline resource references
  - Largest section - core execution guidance

TARGET AGENTS: 120-180 tokens (6%)
  - Primary/secondary users, integration patterns

RESOURCES: 200-280 tokens (10%)
  - Templates/examples/documentation overview

INTEGRATION WITH TEAM WORKFLOWS: 120-180 tokens (6%)
  - Multi-agent coordination, Claude orchestration

SUCCESS METRICS: 80-120 tokens (4%)
  - Measurable outcomes, validation criteria

TROUBLESHOOTING: 100-150 tokens (5%)
  - Common issues, escalation paths

Total: 2,420-3,410 tokens (within 2,000-3,500 budget)
```

**Allocation Rationale:**
- **50% to WORKFLOW STEPS:** Primary value is execution guidance - justify largest allocation
- **15% to WHEN TO USE:** Clear triggers critical for coordination skills used by all agents
- **10% to PURPOSE:** Establish importance and mandatory nature
- **Remaining 25%:** Supporting sections enabling navigation, integration, troubleshooting

**Technical Skill Token Budget: 3,000-5,000 Tokens**

**Recommended Allocation:**
```yaml
YAML Frontmatter: 90-110 tokens (2%)
  - Comprehensive description for domain targeting

Title + Introduction: 50-80 tokens (1.5%)
  - Technical context establishment

PURPOSE: 240-320 tokens (6%)
  - Deep domain expertise rationale

WHEN TO USE: 400-550 tokens (11%)
  - 4-6 domain-specific scenarios with detailed triggers

WORKFLOW STEPS: 1,800-2,800 tokens (56%)
  - Extensive technical processes, decision trees, pattern applications
  - Justified by technical complexity

TARGET AGENTS: 180-260 tokens (5%)
  - Specialist users, cross-domain applications

RESOURCES: 350-480 tokens (10%)
  - Extensive templates/examples/documentation navigation

INTEGRATION WITH TEAM WORKFLOWS: 160-240 tokens (5%)
  - Backend-frontend coordination, testing integration

SUCCESS METRICS: 120-180 tokens (3.5%)
  - Technical validation criteria

TROUBLESHOOTING: 180-280 tokens (5%)
  - Complex issue resolution, optimization techniques

Total: 3,570-5,300 tokens (targeting 3,000-5,000 budget, upper bound slightly over for comprehensive technical guidance)
```

**Allocation Rationale:**
- **56% to WORKFLOW STEPS:** Technical skills require extensive pattern guidance - largest allocation justified
- **11% to WHEN TO USE:** Domain-specific scenarios need detailed technical context
- **10% to RESOURCES:** Extensive resource ecosystem requires comprehensive navigation section
- **Remaining 23%:** Supporting sections with emphasis on troubleshooting for complex technical issues

**Meta-Skill Token Budget: 3,500-5,000 Tokens**

**Recommended Allocation:**
```yaml
YAML Frontmatter: 90-120 tokens (2%)
  - Comprehensive methodology description

Title + Introduction: 60-90 tokens (1.5%)
  - Meta-capability contextualization

PURPOSE: 280-360 tokens (7%)
  - Systematic framework rationale, PromptEngineer focus

WHEN TO USE: 480-620 tokens (12%)
  - 5-6 creation/refactoring scenarios with anti-bloat context

5-PHASE WORKFLOW: 2,200-3,000 tokens (60%)
  - Most extensive section: complete systematic methodology
  - Each phase 400-600 tokens (detailed sub-processes)

TARGET AGENTS: 140-200 tokens (4%)
  - PromptEngineer primary, Claude secondary understanding

RESOURCES: 400-550 tokens (11%)
  - Comprehensive resource bundle navigation (templates/examples/documentation)

INTEGRATION WITH TEAM WORKFLOWS: 180-260 tokens (5%)
  - How meta-skill enhances team capabilities

SUCCESS METRICS: 140-200 tokens (4%)
  - Skill creation efficiency, ecosystem health

TROUBLESHOOTING: 200-320 tokens (6%)
  - Edge cases, anti-bloat violations, complex scenarios

Total: 4,170-5,720 tokens (targeting 3,500-5,000, upper bound exceeds for comprehensive meta-capability - acceptable for meta-skills)
```

**Allocation Rationale:**
- **60% to 5-PHASE WORKFLOW:** Meta-skills are methodological frameworks - workflow is core value
- **12% to WHEN TO USE:** Creation scenarios require comprehensive anti-bloat context
- **11% to RESOURCES:** Extensive resource bundles need detailed navigation
- **Remaining 17%:** Supporting sections with emphasis on troubleshooting for complex creation decisions

### Resource Token Allocation

**Template Token Budgets:**

```yaml
Simple Format Template: 200-300 tokens
  - Single format specification (e.g., artifact reporting format)
  - 3-5 fields with placeholder guidance
  - Validation checklist
  Example: discovery-report-template.md

Moderate Complexity Template: 300-450 tokens
  - Multi-section format (e.g., GitHub issue template)
  - 8-12 fields with detailed guidance
  - Examples for each field type
  - Comprehensive validation checklist
  Example: github-issue-template.md

Complex Template: 450-600 tokens
  - Comprehensive format with conditional sections
  - 15+ fields with contextual guidance
  - Decision trees for optional sections
  - Multi-stage validation
  Example: pr-description-template.md (with AI Sentinel triggers)
```

**Example Token Budgets:**

```yaml
Basic Workflow Example: 500-800 tokens
  - Single-agent workflow demonstration
  - 3-4 workflow steps executed
  - Annotations explaining decisions
  Example: backend-specialist-discovery.md

Complex Workflow Example: 800-1,200 tokens
  - Multi-agent coordination demonstration
  - 5-7 workflow steps across agents
  - Handoff patterns, integration points
  - Outcome and key takeaways
  Example: multi-agent-workflow.md

Comprehensive Scenario: 1,200-1,800 tokens
  - End-to-end feature implementation
  - Multiple skills invoked
  - Cross-domain collaboration
  - Lessons learned, optimization opportunities
  Example: api-implementation-full-workflow.md
```

**Documentation Token Budgets:**

```yaml
Focused Deep Dive: 1,000-1,500 tokens
  - Single concept comprehensive exploration
  - 3-5 main sections
  - Practical applications
  Example: anti-bloat-framework.md (focused on reusability threshold)

Comprehensive Guide: 1,500-2,500 tokens
  - Multi-faceted topic coverage
  - 6-10 main sections
  - Examples, decision frameworks, best practices
  - Table of contents for navigation
  Example: skill-categorization-guide.md

Extensive Reference: 2,500-3,500 tokens
  - Exhaustive topic coverage
  - 10+ main sections with subsections
  - Multiple decision frameworks, extensive examples
  - Comprehensive troubleshooting
  - Table of contents essential
  Example: progressive-loading-guide.md (this document)
```

### Token Allocation Trade-Offs

**Trade-Off 1: SKILL.md Comprehensiveness vs. Resource Depth**

**Scenario:** Technical skill approaching 5,000 token ceiling for SKILL.md

**Option A: Comprehensive SKILL.md, Moderate Resources**
```yaml
SKILL.md: 4,800 tokens
  - Extensive workflow steps with decision trees embedded
  - Inline pattern demonstrations
  - Troubleshooting within workflow sections

Resources:
  - 3 templates (~900 tokens total)
  - 2 examples (~1,600 tokens total)
  - 1 documentation (~1,800 tokens)
  Total Resources: ~4,300 tokens

Total Potential Context: 9,100 tokens

Pros:
  - Agent can execute most workflows from SKILL.md alone
  - Fewer resource loading decisions required

Cons:
  - SKILL.md approaching token ceiling limits future expansion
  - Pattern demonstrations in SKILL.md not reusable separately
```

**Option B: Focused SKILL.md, Extensive Resources**
```yaml
SKILL.md: 3,600 tokens
  - Focused workflow steps with resource references
  - Decision trees extracted to documentation
  - Troubleshooting overview only

Resources:
  - 5 templates (~1,800 tokens total)
  - 4 examples (~4,200 tokens total)
  - 3 documentation guides (~6,500 tokens)
  Total Resources: ~12,500 tokens

Total Potential Context: 16,100 tokens

Pros:
  - SKILL.md has room for future workflow expansion
  - Resources independently reusable and updatable
  - Graduated loading: Agent loads only needed resources

Cons:
  - Agent must load resources more frequently
  - More navigation between SKILL.md and resources
```

**Recommendation:** Option B for technical skills - Extensive resource depth serves specialists better, graduated loading more efficient

**Trade-Off 2: Multiple Specific Templates vs. Single Comprehensive Template**

**Scenario:** Coordination skill needs artifact reporting format guidance

**Option A: Single Comprehensive Template**
```yaml
Template: comprehensive-artifact-reporting.md
Token Budget: 580 tokens
Content:
  - Pre-work discovery format (180 tokens)
  - Immediate reporting format (220 tokens)
  - Context integration format (180 tokens)

Pros:
  - All formats in one place
  - Agent loads once for all scenarios

Cons:
  - Agent loads 580 tokens even when needing only one format (180 tokens needed, 400 wasted)
  - Template becomes bloated as formats added
```

**Option B: Multiple Specific Templates**
```yaml
Templates:
  - discovery-report-template.md (220 tokens)
  - immediate-reporting-template.md (260 tokens)
  - context-integration-template.md (220 tokens)
  Total if all loaded: 700 tokens

Pros:
  - Agent loads only needed format (~220-260 tokens selective)
  - Each template focused and clear
  - Easy to update individual formats

Cons:
  - Slightly higher total token count (700 vs. 580)
  - Agent must choose correct template (more decision points)
```

**Recommendation:** Option B for coordination skills - Selective loading efficiency outweighs slight total token increase

---

## Advanced Optimization Techniques

### Expert-Level Progressive Loading Strategies

This section covers advanced techniques for maximizing progressive loading efficiency beyond standard patterns.

### Technique 1: Lazy Resource Bundling

**Concept:** Group related resources into bundles loaded together when agent crosses specific complexity threshold

**Standard Approach:**
```yaml
Agent loads SKILL.md → Workflow Step 3 → Loads template-3.md → Continues
                     → Workflow Step 5 → Loads example-5.md → Continues
                     → Workflow Step 8 → Loads doc-8.md → Continues

Total Loading Events: 4 (SKILL.md + 3 individual resources)
Context Overhead: ~200 tokens in loading decisions and resource navigation
```

**Lazy Bundling Approach:**
```yaml
SKILL.md includes:
  "For complex scenario workflows (Steps 3-8), load resources/bundles/complex-scenario-bundle.md"

complex-scenario-bundle.md contains:
  - Template 3 content
  - Example 5 content
  - Documentation 8 summaries
  - References to full documentation if needed

Total Bundle: 1,200 tokens (vs. 1,400 tokens loading individually)

Agent loads: SKILL.md → Complex scenario identified → Loads bundle → Executes Steps 3-8

Total Loading Events: 2 (SKILL.md + 1 bundle)
Context Savings: 200 tokens from reduced navigation + 200 tokens from bundle optimization = 400 tokens
```

**When to Use:**
- Workflow has clear complexity tiers (simple vs. complex paths)
- Resources naturally cluster around specific scenarios
- Loading overhead (multiple small files) exceeds bundling overhead

**Implementation Example:**
```markdown
## WORKFLOW STEPS

### Simple Scenario Path (Steps 1-4)
[Execute with SKILL.md guidance only, no resources needed]

### Complex Scenario Path (Steps 5-10)
**Trigger:** Workflow involves multi-agent coordination OR edge case handling
**Resource Bundle:** Load `resources/bundles/complex-coordination-bundle.md` before proceeding
- Contains: Templates for Steps 5, 7, 9
- Contains: Example demonstrating full Steps 5-10 execution
- Contains: Decision tree for edge case routing

[Detailed Steps 5-10 with references to bundle content]
```

### Technique 2: Conditional Section Expansion

**Concept:** SKILL.md includes collapsed sections agents expand only when specific conditions met

**Standard Approach:**
```yaml
SKILL.md Troubleshooting Section: 320 tokens
  - Issue 1: Symptoms, root cause, solution (80 tokens)
  - Issue 2: Symptoms, root cause, solution (85 tokens)
  - Issue 3: Symptoms, root cause, solution (75 tokens)
  - Issue 4: Symptoms, root cause, solution (80 tokens)

Agent Reads: All 320 tokens even if encountering Issue 2 only
Wasted Context: 240 tokens (issues 1, 3, 4 never needed)
```

**Conditional Expansion Approach:**
```yaml
SKILL.md Troubleshooting Section: 120 tokens
  - Issue 1: Symptoms only (15 tokens) → "See resources/troubleshooting/issue-1.md for resolution"
  - Issue 2: Symptoms only (18 tokens) → "See resources/troubleshooting/issue-2.md for resolution"
  - Issue 3: Symptoms only (14 tokens) → "See resources/troubleshooting/issue-3.md for resolution"
  - Issue 4: Symptoms only (16 tokens) → "See resources/troubleshooting/issue-4.md for resolution"
  - Common Resolution Patterns (57 tokens)

Agent Encountering Issue 2:
  1. Reads all symptoms (120 tokens)
  2. Identifies Issue 2 match
  3. Loads resources/troubleshooting/issue-2.md (180 tokens)
  Total Context: 300 tokens

Savings vs. Standard: 320 - 300 = 20 tokens (modest, but scales with troubleshooting depth)
```

**When to Use:**
- Content has clear symptom → detailed resolution pattern
- Most agents encounter 1-2 issues max, not all issues
- Detailed resolutions are token-heavy (>100 tokens each)

**Implementation Example:**
```markdown
## TROUBLESHOOTING

### Common Issues (Symptom Quick Reference)

#### Issue: SKILL.md Exceeds Token Budget
**Symptoms:** Line count >625 lines, estimated tokens >5,000, skill feels bloated
**Resolution:** See `resources/troubleshooting/token-budget-exceeded.md` for comprehensive extraction strategy

#### Issue: Agents Not Using Resources
**Symptoms:** Agents execute workflow without loading templates/examples, inconsistent outputs
**Resolution:** See `resources/troubleshooting/resource-adoption.md` for trigger optimization techniques

#### Issue: Multiple Agents Reference Skill Inconsistently
**Symptoms:** Some agents embed partial content, others reference correctly, mixed efficiency
**Resolution:** See `resources/troubleshooting/integration-inconsistency.md` for standardization approach

### Quick Fixes (Common Resolution Patterns)
[Brief patterns applicable across multiple issues - 50-80 tokens]
```

### Technique 3: Version-Specific Resource Loading

**Concept:** Skills evolve. Maintain resource versions for different skill maturity levels.

**Challenge:**
```yaml
Skill Evolution:
  v1.0: Basic workflow, 3 templates, 2 examples
  v1.5: Enhanced workflow, 5 templates, 4 examples, 2 documentation guides
  v2.0: Comprehensive workflow, 8 templates, 6 examples, 5 documentation guides

Problem:
  Agent using skill-creation v1.0 methodology sees references to v2.0 resources (don't exist yet in their context)
  OR
  All resources loaded for all versions (bloat)
```

**Version-Specific Loading:**
```yaml
resources/
  ├── v1/
  │   ├── templates/ (3 templates from v1.0)
  │   ├── examples/ (2 examples from v1.0)
  ├── v2/
  │   ├── templates/ (8 templates from v2.0, includes v1 + new)
  │   ├── examples/ (6 examples from v2.0)
  │   └── documentation/ (5 guides from v2.0)

SKILL.md includes version marker:
  **Skill Version:** 2.0
  **Resources:** This version uses resources/v2/ directory

Agent loading v2.0 skill → Loads v2 resources
Agent with legacy v1.0 reference → Loads v1 resources (if maintained) or upgrades reference
```

**When to Use:**
- Skill undergoing significant evolution (not minor updates)
- Breaking changes in resource structure or workflow
- Need to support legacy agent definitions temporarily during migration

**Implementation Consideration:**
```markdown
## RESOURCES

**Resource Version:** 2.0
**Location:** All resources in `resources/v2/` directory for this skill version

### Migration Note
If using skill-creation v1.x, resources are in `resources/v1/`. Consider updating agent reference to skill-creation v2.0 for enhanced capabilities.

### Templates (v2.0)
[List v2 templates with v2-specific features]
```

### Technique 4: Skill Composition Patterns

**Concept:** Complex skills reference other skills, creating composable capability layers

**Example Scenario:**
```yaml
Skill: comprehensive-api-implementation
Workflow involves:
  - API design patterns (from api-design-patterns skill)
  - Working directory coordination (from working-directory-coordination skill)
  - Test architecture (from test-architecture-best-practices skill)
  - Documentation standards (from documentation-grounding skill)

Standard Approach:
  comprehensive-api-implementation SKILL.md duplicates patterns from 4 other skills
  Token Load: ~8,000 tokens (comprehensive but redundant)

Composition Approach:
  comprehensive-api-implementation SKILL.md:
    - High-level workflow orchestrating 4 sub-skills
    - Explicit skill invocation points
    - Integration patterns between skills
  Token Load: ~2,400 tokens (orchestration only)

  Agent loads: comprehensive-api-implementation SKILL.md
  Agent encounters: "Step 3: Apply API Design Patterns (invoke api-design-patterns skill)"
  Agent loads: api-design-patterns SKILL.md (~4,200 tokens)
  Agent executes: API design workflow
  Agent returns: comprehensive-api-implementation workflow
  Agent continues: Step 4...
```

**When to Use:**
- Complex multi-phase workflows naturally decompose into existing skill capabilities
- Avoid duplication across skills (DRY principle for skills)
- Enable flexible composition (different agents may skip certain sub-skills)

**Implementation Example:**
```markdown
## WORKFLOW STEPS

### Phase 1: API Contract Design

**Process:**
1. Define API requirements and constraints
2. **Invoke api-design-patterns skill** for comprehensive contract design workflow
3. Document contract decisions in working directory artifact

**Skill Invocation:**
- **Skill:** api-design-patterns
- **Specific Workflow:** "Designing New REST Endpoint" scenario
- **Output:** API contract specification following zarichney-api patterns

**Integration:**
After completing api-design-patterns workflow, return here with contract specification for Phase 2 validation.

---

### Phase 2: Contract Validation
[Continue with comprehensive-api-implementation-specific steps]
```

**Benefit:**
- Reduces token duplication (DRY skills)
- Maintains single source of truth (api-design-patterns owns API design guidance)
- Enables composition flexibility (agents can invoke comprehensive workflow OR individual sub-skills)

### Technique 5: Dynamic Resource Recommendation

**Concept:** SKILL.md workflow includes agent-context-aware resource recommendations

**Standard Approach:**
```markdown
**Resource:** See `resources/examples/backend-frontend-coordination.md` for realistic demonstration
```
All agents see same generic resource reference regardless of their specific context.

**Dynamic Recommendation Approach:**
```markdown
**Resource Recommendations:**
- **For BackendSpecialist:** See `resources/examples/backend-api-implementation.md` for backend-focused workflow
- **For FrontendSpecialist:** See `resources/examples/frontend-integration.md` for frontend-focused workflow
- **For Multi-Agent Coordination:** See `resources/examples/backend-frontend-coordination.md` for full-stack collaboration

**Agent Decision:** Load example matching your current role and task context
```

**When to Use:**
- Skill used by multiple agent types with different perspectives (BackendSpecialist vs. FrontendSpecialist)
- Examples demonstrate same pattern from different viewpoints
- Resource loading can be optimized by agent-specific targeting

**Benefit:**
- Agent loads most relevant resource (800 tokens) vs. generic example (1,200 tokens) covering all perspectives
- Precision increases efficiency and clarity

---

## Troubleshooting Context Inefficiency

### Diagnosing and Resolving Progressive Loading Issues

When skills don't achieve expected token efficiency, systematic diagnosis identifies root causes.

### Issue 1: SKILL.md Token Budget Exceeded

**Symptoms:**
- Line count >625 lines (coordination) or >750 lines (technical/meta)
- Estimated tokens >3,500 (coordination) or >5,000 (technical/meta)
- Skill feels bloated, agents report difficulty navigating

**Diagnostic Questions:**
1. Are there detailed format specifications embedded in SKILL.md? → Extract to templates/
2. Are there multiple realistic scenario demonstrations? → Extract to examples/
3. Are there conceptual deep dives or philosophy sections? → Extract to documentation/
4. Is troubleshooting section exhaustive (>400 tokens)? → Extract detailed resolutions to documentation/
5. Are decision trees or matrices very large (>300 tokens)? → Evaluate if summary + reference to documentation would suffice

**Resolution Workflow:**
```yaml
Step 1: Identify High-Token Sections
  - Use section-specific token measurement (Method 4)
  - List sections >400 tokens

Step 2: Categorize Content Type
  - Format specifications → templates/
  - Scenario demonstrations → examples/
  - Conceptual explanations → documentation/
  - Troubleshooting details → documentation/

Step 3: Extract to Resources
  - Move content to appropriate resource category
  - Replace in SKILL.md with summary (20-40 tokens) + resource reference (15-20 tokens)

Step 4: Validate Post-Extraction
  - Recalculate SKILL.md tokens using line count or character count method
  - Ensure within budget for skill category
  - Verify agent can still execute workflow from SKILL.md (resources enhance, not replace)

Step 5: Update Resource References
  - Add extracted resources to RESOURCES section overview
  - Ensure inline references at workflow step usage points
```

**Example Resolution:**
```yaml
Before Extraction:
  SKILL.md Section: "API Endpoint Design Patterns" (680 tokens)
  Content: Detailed REST patterns with code examples and decision matrices

After Extraction:
  SKILL.md Section: "API Endpoint Design Patterns" (120 tokens)
    - High-level pattern overview
    - Reference: "See resources/documentation/rest-design-patterns.md for comprehensive guidance"

  New Resource: resources/documentation/rest-design-patterns.md (680 tokens)
    - Complete pattern catalog with code examples
    - Decision matrices for pattern selection
    - Loaded only when agent needs deep REST guidance

Token Savings in SKILL.md: 680 - 120 = 560 tokens
Total Context When Loaded: 120 (SKILL.md section) + 680 (resource) = 800 tokens (same as before)
Efficiency: Resource loaded selectively, not always present in SKILL.md
```

### Issue 2: Agents Not Loading Resources

**Symptoms:**
- Agents execute workflow from SKILL.md but never load templates/examples
- Output inconsistent (agents improvising format instead of using templates)
- Resource files created but unused

**Diagnostic Questions:**
1. Are resource references inline at workflow step usage points? → Should be, not only in RESOURCES section
2. Do resource references specify explicit usage triggers? → "See template for exact format" not just "See template"
3. Are workflow steps self-sufficient without resources? → If yes, agents may not perceive resource value
4. Are too many resources available? → Agent overwhelmed, chooses to proceed without loading any

**Resolution Workflow:**
```yaml
Step 1: Audit Resource Reference Placement
  - Check each workflow step for inline resource references
  - Add references at decision points where agent would benefit from template/example

Step 2: Enhance Resource Reference Clarity
  Before: "**Resource:** See resources/templates/format-template.md"
  After: "**Resource:** See `resources/templates/format-template.md` for exact format specification. Use this when uncertain about field structure."

Step 3: Validate Workflow Self-Sufficiency Balance
  - Workflow steps should provide process guidance
  - Resources should provide detailed content (formats, examples, deep dives)
  - Agent should benefit from loading resource but can proceed without in simple cases

Step 4: Consolidate Redundant Resources
  - If 8 templates for slight format variations, consider 2-3 comprehensive templates
  - Reduce decision paralysis from too many choices

Step 5: Add Resource Loading to Checklists
  Example:
    **Checklist:**
    - [ ] Loaded format-template.md for exact specification
    - [ ] All required fields included
    - [ ] Format validated against template
```

**Example Resolution:**
```yaml
Before Enhancement:
  Workflow Step: "Create artifact report"
  Resource Reference: In RESOURCES section only (agent doesn't connect workflow step to template)

After Enhancement:
  Workflow Step: "Create artifact report following standardized format"
  Inline Reference: "**Resource:** See `resources/templates/artifact-report-template.md` for exact format. Template includes all required fields with placeholder guidance."
  Checklist: "- [ ] Format matches artifact-report-template.md structure"

Result: Agent explicitly directed to template, checklist validates usage
```

### Issue 3: Resource Bloat (Too Many Resources)

**Symptoms:**
- Skill has 15+ templates, 10+ examples, 8+ documentation files
- RESOURCES section overwhelming to navigate
- Agent spends significant tokens scanning resource options
- Many resources rarely loaded

**Diagnostic Questions:**
1. Are there redundant templates for similar formats? → Consolidate
2. Are examples demonstrating same pattern in slight variations? → Reduce to 1-2 canonical examples
3. Is documentation covering overlapping concepts? → Merge into comprehensive guides
4. Are resources created "just in case" without validated usage? → Remove low-usage resources

**Resolution Workflow:**
```yaml
Step 1: Audit Resource Usage
  - Review agent engagement logs: Which resources actually loaded?
  - Identify resources loaded <5% of skill invocations

Step 2: Consolidate Redundant Resources
  Templates:
    - Merge similar formats into single comprehensive template with conditional sections
  Examples:
    - Combine variations into single example demonstrating pattern thoroughly
  Documentation:
    - Merge overlapping concept docs into comprehensive guides with table of contents

Step 3: Archive Low-Usage Resources
  - Move resources loaded <5% to archived/ subdirectory
  - Remove from RESOURCES section overview
  - Can be restored if future usage validates need

Step 4: Reorganize RESOURCES Section for Clarity
  - Group resources by usage scenario, not just type
  - Add usage guidance: "Most agents need only 1-2 resources per workflow execution"

Step 5: Validate Resource Navigation Efficiency
  - Agent should identify needed resource in <200 tokens of RESOURCES section scanning
  - If requires >300 tokens to find right resource, organization needs improvement
```

**Example Resolution:**
```yaml
Before Consolidation:
  Templates (8 total):
    - simple-format-template.md
    - simple-format-with-examples.md
    - moderate-format-template.md
    - moderate-format-detailed.md
    - complex-format-template.md
    - complex-format-annotated.md
    - edge-case-format-1.md
    - edge-case-format-2.md
  Agent loads: Scans 8 options (~800 tokens), often chooses wrong one or gives up

After Consolidation:
  Templates (3 total):
    - basic-format-template.md (covers simple + moderate, ~320 tokens)
    - advanced-format-template.md (covers complex scenarios, ~450 tokens)
    - edge-case-format-template.md (consolidates edge cases, ~380 tokens)
  Agent loads: Scans 3 options (~300 tokens), clear differentiation

Efficiency: Resource scanning reduced 62%, clearer decision points
```

### Issue 4: Skill Invocation Overhead Exceeds Embedded Efficiency

**Symptoms:**
- Token measurements show skill invocation (SKILL.md + resources) consistently exceeds embedded pattern approach
- Agents loading skill multiple times per task (cache inefficiency)
- Skill provides comprehensive content but task only needed brief guidance

**Diagnostic Questions:**
1. Is skill comprehensive when agents need surgical guidance? → Consider splitting into focused sub-skills
2. Are agents invoking skill repeatedly instead of retaining instructions? → May indicate skill navigation issues
3. Is embedded pattern truly insufficient? → Validate whether extraction was justified
4. Are resources always loaded even when not needed? → Improve conditional loading triggers

**Resolution Workflow:**
```yaml
Step 1: Analyze Task Complexity Distribution
  - Review last 20 skill invocations
  - Categorize: Simple (need 20% of skill content), Moderate (50%), Complex (80%+)

  If >60% Simple Tasks:
    → Skill over-engineered for typical usage
    → Consider creating skill-name-lite variant for common simple cases
    → Keep comprehensive skill for complex scenarios

Step 2: Validate Anti-Bloat Framework Application
  - Re-apply Phase 1 reusability threshold
  - If skill now used by <3 agents or <2 times/week, consider retiring
  - If embedded pattern sufficient for 80%+ use cases, skill extraction may have been premature

Step 3: Optimize Resource Loading Triggers
  - Ensure resources loaded conditionally, not automatically
  - Add clear "Simple scenarios: Execute from SKILL.md without resources" guidance
  - Add "Complex scenarios: Load resources/bundles/advanced.md for comprehensive guidance"

Step 4: Consider Skill Tiering
  Create skill variants:
    - skill-name-essentials: Core 20% workflow for simple cases (~1,500 tokens)
    - skill-name-comprehensive: Full workflow for complex cases (~4,500 tokens)

  Agent references: essentials for routine work, comprehensive for advanced needs

Step 5: Improve Skill Caching Awareness
  - If Claude orchestration loads skill for Agent 1, note for reuse by Agent 2-3 in same context
  - Avoid re-loading same skill unnecessarily across sequential agent engagements
```

**Example Resolution:**
```yaml
Scenario: working-directory-coordination skill used 40 times/week
  - 28 times (70%): Simple artifact reporting (need template only)
  - 8 times (20%): Moderate coordination (need workflow + template)
  - 4 times (10%): Complex multi-agent scenarios (need full skill + examples + documentation)

Problem:
  All invocations load full SKILL.md (~2,580 tokens) even when only template needed (~280 tokens)

Resolution Approach A: Skill Tiering
  Create working-directory-coordination-essentials skill:
    - YAML frontmatter + Purpose + Immediate Reporting workflow only
    - Token Budget: ~1,200 tokens
    - Reference template for format

  Agents use essentials for simple reporting (70% of cases)
  Agents use comprehensive working-directory-coordination for multi-agent coordination (30% of cases)

Resolution Approach B: Conditional Loading Guidance
  Enhance working-directory-coordination SKILL.md:
    **Usage Tiers:**
    - Simple Reporting: Load template only (resources/templates/immediate-reporting-template.md)
    - Multi-Agent Coordination: Load full workflow + examples
    - Complex Scenarios: Load full workflow + documentation

  Agent assesses task complexity, loads appropriately

Efficiency Impact:
  Approach A: 70% of invocations use 1,200 tokens vs. 2,580 (53% reduction for common cases)
  Approach B: Simple tasks load template only (280 tokens vs. 2,580 = 89% reduction)

Recommendation: Approach B (conditional loading) - More flexible, no skill proliferation
```

---

**Progressive Loading Guide Status:** ✅ **COMPLETE**
**Token Estimate:** ~19,200 tokens (~2,400 lines × 8 tokens/line)
**Purpose:** Comprehensive deep dive into progressive loading architecture, token efficiency, and optimization techniques
**Target Audience:** PromptEngineer creating/optimizing skills
**Integration:** Core resource for skill-creation meta-skill Phase 3 (Progressive Loading Design)
