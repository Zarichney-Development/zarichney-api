# Skill Discovery Mechanics: How Agents Find and Use Skills Effectively

**Purpose:** Comprehensive guide to skill discovery patterns, categorization systems, agent-skill matching algorithms, and trigger scenario identification enabling agents to find relevant skills among 100+ skill ecosystem

**Target Audience:** PromptEngineer designing skill metadata for optimal discovery, Claude orchestrating agent-skill matches, agents understanding skill navigation

**Prerequisites:** Understanding of YAML frontmatter from progressive-loading-guide.md, skill categories from SKILL.md Phase 1

---

## Table of Contents

1. [Skill Discovery Philosophy](#skill-discovery-philosophy)
2. [Discovery Phase Mechanics](#discovery-phase-mechanics)
3. [Metadata Scanning Algorithms](#metadata-scanning-algorithms)
4. [Skill Categorization System](#skill-categorization-system)
5. [Agent-Skill Matching Patterns](#agent-skill-matching-patterns)
6. [Trigger Scenario Identification](#trigger-scenario-identification)
7. [Discovery Efficiency Optimization](#discovery-efficiency-optimization)
8. [Integration Point Design](#integration-point-design)
9. [Multi-Skill Coordination Discovery](#multi-skill-coordination-discovery)
10. [Discovery Patterns from Examples](#discovery-patterns-from-examples)
11. [Scaling Discovery to 100+ Skills](#scaling-discovery-to-100-plus-skills)
12. [Discovery Failure Diagnosis](#discovery-failure-diagnosis)

---

## Skill Discovery Philosophy

### The Discovery Challenge

As zarichney-api's skill ecosystem scales from 5 skills to 100+ skills, discovery becomes the critical bottleneck:

**Without Systematic Discovery:**
- Agents browse 100+ SKILL.md files (~300,000 tokens) to find relevant capability
- Trial-and-error skill invocation wastes context window
- Relevant skills missed due to poor naming or description
- Duplicate capabilities created because existing skills not discovered

**With Systematic Discovery:**
- Agents scan 100 YAML frontmatter entries (~10,000 tokens) for relevance assessment
- Metadata-driven matching identifies 2-3 candidate skills efficiently
- Clear categorization and naming enable predictable skill location
- Comprehensive coverage through well-designed discovery triggers

### Discovery as Three-Stage Funnel

```
┌─────────────────────────────────────────────────────────────────┐
│ STAGE 1: BROAD SCANNING                                         │
│ Scope: All skills in ecosystem (100+ skills)                    │
│ Method: Category-based filtering and name pattern matching      │
│ Output: 10-15 potentially relevant skills                       │
│ Token Load: ~1,500 tokens (scan categories and names only)      │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ STAGE 2: METADATA EVALUATION                                    │
│ Scope: 10-15 candidate skills from Stage 1                      │
│ Method: YAML description analysis for trigger matching          │
│ Output: 2-3 highly relevant skills                              │
│ Token Load: ~1,000 tokens (read descriptions of candidates)     │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ STAGE 3: SKILL INVOCATION                                       │
│ Scope: 2-3 highly relevant skills                               │
│ Method: Load complete SKILL.md for workflow execution           │
│ Output: 1-2 skills actually used for task                       │
│ Token Load: ~5,000 tokens (load SKILL.md instructions)          │
└─────────────────────────────────────────────────────────────────┘

Total Discovery Efficiency: ~7,500 tokens to find right skills
vs. Brute Force: ~300,000 tokens to read all SKILL.md files
Efficiency Gain: 97.5% token reduction through systematic discovery
```

### Discovery Success Criteria

Effective skill discovery achieves:

1. **High Precision:** >80% of discovered skills are actually relevant to task
2. **High Recall:** >90% of relevant skills in ecosystem are discovered when needed
3. **Low Latency:** Discovery completes in <10,000 tokens context budget
4. **Predictable Navigation:** Agents can anticipate where to find skill categories
5. **Scalable:** Discovery efficiency maintains as ecosystem grows from 10 to 100+ skills

### Discovery Failure Modes

**Poor Precision (False Positives):**
```yaml
Scenario: Agent searches for "API testing patterns"
Poor Discovery: Returns api-design-patterns, test-architecture-best-practices, testing-workflows, api-documentation-standards (4 skills)
Agent loads: All 4 SKILL.md files (~15,000 tokens)
Actual Relevance: Only test-architecture-best-practices directly applicable (1/4 = 25% precision)
Waste: 75% of loaded context irrelevant

Root Cause: Overly broad descriptions mentioning "API" and "testing" without clear differentiation
```

**Poor Recall (False Negatives):**
```yaml
Scenario: Agent needs working directory artifact reporting guidance
Poor Discovery: Scans skills, finds artifact-creation-workflow (generic artifact handling)
Agent loads: artifact-creation-workflow SKILL.md (~2,800 tokens)
Missed Skill: working-directory-coordination specifically covers artifact reporting protocols
Result: Agent improvises reporting format (inconsistent with team standards)

Root Cause: working-directory-coordination description didn't emphasize "artifact reporting" trigger
```

**High Latency (Inefficient Scanning):**
```yaml
Scenario: Agent needs skill for GitHub issue creation
Poor Discovery: No category system, scans all 100 skill descriptions sequentially (~10,000 tokens)
Timeline: Reads descriptions 1-by-1, identifies github-issue-creation at position 73
Token Waste: Scanned 72 irrelevant skill descriptions before finding match

Root Cause: No categorization enabling targeted search (should check "workflow" category first)
```

---

## Discovery Phase Mechanics

### How Agents (and Claude) Discover Skills

Discovery occurs in two primary contexts:

**Context 1: Agent Self-Discovery During Task Execution**
```yaml
Trigger: Agent executing task, encounters need for guidance not in agent definition
Example: BackendSpecialist implementing API endpoint, realizes needs design pattern guidance

Discovery Flow:
  1. Agent recognizes knowledge gap: "I need API design best practices"
  2. Agent scans .claude/skills/ directory for relevant capabilities
  3. Agent identifies potential match: api-design-patterns skill
  4. Agent loads SKILL.md to validate relevance
  5. Agent executes workflow from skill

Token Progression:
  - Knowledge gap recognition: 0 tokens (internal reasoning)
  - Directory scan (Stage 1): ~1,500 tokens (categories and names)
  - Metadata evaluation (Stage 2): ~800 tokens (read 8 candidate descriptions)
  - Skill invocation (Stage 3): ~4,200 tokens (load api-design-patterns SKILL.md)
  Total Discovery: ~6,500 tokens
```

**Context 2: Claude Orchestration-Driven Discovery**
```yaml
Trigger: Claude crafting context package for agent engagement
Example: Claude delegating API implementation to BackendSpecialist, proactively includes skill references

Discovery Flow:
  1. Claude analyzes task requirements from GitHub issue
  2. Claude maps requirements to skill capabilities (meta-knowledge)
  3. Claude includes skill references in agent context package
  4. Agent receives pre-discovered skill pointers, loads as needed

Token Progression:
  - Claude's task analysis: Included in orchestration overhead
  - Skill reference in context package: ~60 tokens (2-3 skill references)
  - Agent loads referenced skills: ~8,000 tokens (if 2 skills invoked)
  Total Discovery: Minimal (Claude pre-filtered, agent loads specific skills)

Efficiency Advantage: Claude's meta-knowledge prevents agent trial-and-error discovery
```

### Directory Structure Navigation

Skills organized in categorized directory hierarchy enable efficient Stage 1 filtering:

**Flat Structure (Anti-Pattern):**
```
.claude/skills/
├── skill-1.md
├── skill-2.md
├── skill-3.md
...
├── skill-100.md
```
**Problem:** Agent must scan all 100 skills sequentially, no filtering optimization

**Categorized Structure (Best Practice):**
```
.claude/skills/
├── coordination/
│   ├── working-directory-coordination/SKILL.md
│   ├── core-issue-focus/SKILL.md
│   └── documentation-grounding/SKILL.md
├── technical/
│   ├── api-design-patterns/SKILL.md
│   ├── test-architecture-best-practices/SKILL.md
│   └── security-threat-modeling/SKILL.md
├── workflow/
│   ├── github-issue-creation/SKILL.md
│   ├── pr-analysis-workflow/SKILL.md
│   └── testing-execution/SKILL.md
├── meta/
│   ├── agent-creation/SKILL.md
│   ├── skill-creation/SKILL.md
│   └── command-creation/SKILL.md
└── documentation/
    └── standards-compliance-validation/SKILL.md
```

**Benefit:** Agent narrows search to relevant category (e.g., "Need API guidance" → Check `technical/` category first)

**Stage 1 Filtering Efficiency:**
```yaml
Flat Structure:
  Scan All: 100 skills × 100 tokens (name + description) = 10,000 tokens

Categorized Structure:
  Identify Category: "technical" for API design needs
  Scan Category: 15 skills in technical/ × 100 tokens = 1,500 tokens
  Efficiency: 85% token reduction in Stage 1 (10,000 → 1,500 tokens)
```

### Metadata Accessibility Patterns

**YAML Frontmatter Position:**
Skills use standardized YAML frontmatter at file beginning enabling rapid metadata access:

```markdown
---
name: skill-name
description: Brief description including what skill does and when to use it.
---

# Skill Title
[Content follows...]
```

**Scanning Algorithm:**
```yaml
Agent Discovery Process:
  1. List directory: .claude/skills/technical/
  2. Identify SKILL.md files: api-design-patterns/SKILL.md, test-architecture-best-practices/SKILL.md
  3. For each SKILL.md:
     a. Read lines 1-6 (YAML frontmatter only)
     b. Parse name and description
     c. Evaluate relevance to current task
     d. If relevant: Add to candidates list
  4. Rank candidates by description match quality
  5. Load top 2-3 candidates' full SKILL.md for execution

Token Efficiency:
  - Reading YAML only: 6 lines × 8 tokens/line = ~50 tokens per skill
  - Scanning 15 technical skills: 15 × 50 = ~750 tokens
  - Loading top 2 full skills: 2 × 4,000 = ~8,000 tokens
  Total: ~8,750 tokens vs. ~60,000 tokens if loading all 15 complete SKILL.md files
  Efficiency: 85% reduction through metadata-first scanning
```

---

## Metadata Scanning Algorithms

### Algorithmic Approaches to Skill Discovery

Agents (and Claude) employ several scanning algorithms depending on discovery context:

### Algorithm 1: Keyword Matching (Basic Discovery)

**Approach:** Agent extracts keywords from task, matches against skill descriptions

**Implementation:**
```yaml
Agent Task: "Implement REST endpoint for recipe search with pagination"

Keyword Extraction:
  - Primary: "REST", "endpoint", "API"
  - Secondary: "design", "implementation", "patterns"

Metadata Scan:
  For each skill description:
    Count keyword matches
    Rank by match count

Results:
  api-design-patterns: 3 matches ("REST", "API design", "patterns")
  test-architecture-best-practices: 1 match ("implementation" mentioned in testing context)
  github-issue-creation: 0 matches

Candidate Selection: Load api-design-patterns (highest match count)
```

**Strengths:**
- Simple to implement
- Fast execution (~1,000 tokens for keyword extraction + matching)
- Works well for specific technical tasks

**Weaknesses:**
- Misses semantic matches (e.g., "artifact reporting" doesn't match "working directory coordination" without domain knowledge)
- Can over-match on generic terms (e.g., "implementation" appears in many descriptions)

### Algorithm 2: Category-First Filtering (Hierarchical Discovery)

**Approach:** Agent identifies task category, narrows search to specific directory

**Implementation:**
```yaml
Agent Task: "Create standardized artifact report for other agents to review"

Category Identification:
  Task involves: Multi-agent communication, standardized reporting, team coordination
  Category Match: "coordination" (cross-agent workflow patterns)

Hierarchical Scan:
  1. Navigate to .claude/skills/coordination/
  2. List skills in category: working-directory-coordination, core-issue-focus, documentation-grounding
  3. Read descriptions of 3 skills (~300 tokens)
  4. Identify best match: working-directory-coordination (mentions "artifact reporting" and "team coordination")
  5. Load SKILL.md (~2,500 tokens)

Total Discovery: ~2,800 tokens vs. ~10,000 tokens scanning all categories
Efficiency: 72% reduction through category-first filtering
```

**Strengths:**
- Highly efficient for well-categorized ecosystems
- Predictable navigation (agents learn category semantics)
- Scales well to 100+ skills (categories remain manageable)

**Weaknesses:**
- Requires correct category identification (miscategorization causes misses)
- Cross-category skills may be missed (e.g., skill applicable to both coordination and technical)

### Algorithm 3: Trigger Phrase Matching (Scenario-Driven Discovery)

**Approach:** Skill descriptions include explicit "Use when..." trigger phrases matched against task scenarios

**Implementation:**
```yaml
Agent Task: "Backend and Frontend need to agree on API contract before implementation"

Trigger Extraction:
  Scenario: "API contract agreement between backend and frontend"
  Trigger Phrases: "API contract", "backend-frontend coordination", "contract design"

Metadata Scan:
  api-design-patterns description:
    "Use when BackendSpecialist or FrontendSpecialist designing new endpoints, optimizing existing APIs, or resolving contract integration issues."
    Match: "designing new endpoints" (weak), "contract integration issues" (STRONG)
    Relevance Score: HIGH

  working-directory-coordination description:
    "Use when creating/discovering working directory files to maintain team awareness."
    Match: "team awareness" (moderate, but not specific to API contracts)
    Relevance Score: MODERATE

  backend-frontend-patterns description (hypothetical):
    "Use when coordinating API contracts between backend and frontend teams."
    Match: "API contracts between backend and frontend" (EXACT)
    Relevance Score: VERY HIGH

Candidate Selection: Load backend-frontend-patterns first, api-design-patterns second
```

**Strengths:**
- High precision (trigger phrases explicitly describe use cases)
- Agents quickly identify applicable scenarios
- Descriptions guide correct skill selection

**Weaknesses:**
- Requires comprehensive trigger phrase coverage in descriptions (missing trigger = missed discovery)
- Descriptions can become lengthy with many trigger phrases (approaching 1024 character limit)

### Algorithm 4: Multi-Stage Refinement (Comprehensive Discovery)

**Approach:** Combine algorithms in progressive stages for high precision and recall

**Implementation:**
```yaml
Stage 1: Category-First Filtering
  Task: "Implement security vulnerability scanning for API endpoints"
  Category Candidates: "technical" (API focus) OR "security" (vulnerability focus)
  Scan Both: technical/ + security/ directories
  Results: 25 skills identified across 2 categories

Stage 2: Keyword Matching
  Keywords: "security", "vulnerability", "API", "scanning"
  Scan 25 Candidate Descriptions:
    security-threat-modeling: 3 matches ("security", "vulnerability", "threat assessment")
    api-design-patterns: 2 matches ("API", "security best practices")
    security-auditing-workflow: 3 matches ("security", "vulnerability", "audit")
    test-architecture-best-practices: 1 match ("security testing" mentioned)
  Narrow to: 4 skills with 2+ matches

Stage 3: Trigger Phrase Matching
  Trigger: "vulnerability scanning for API endpoints"
  Evaluate 4 Candidate Descriptions:
    security-threat-modeling: "Use when...assessing security vulnerabilities in application architecture"
      → STRONG MATCH (vulnerability assessment focus)
    api-design-patterns: "Use when...designing new endpoints..."
      → WEAK MATCH (design focus, not vulnerability scanning)
    security-auditing-workflow: "Use when...executing security audits including vulnerability scanning"
      → VERY STRONG MATCH (explicitly mentions vulnerability scanning)
    test-architecture-best-practices: "Use when...designing test strategies including security testing"
      → MODERATE MATCH (testing focus, security secondary)

Stage 4: Ranking and Selection
  Rank by Match Strength:
    1. security-auditing-workflow (VERY STRONG)
    2. security-threat-modeling (STRONG)
    3. test-architecture-best-practices (MODERATE)
    4. api-design-patterns (WEAK)

  Load Top 2: security-auditing-workflow SKILL.md + security-threat-modeling SKILL.md

Total Discovery Token Progression:
  Stage 1: ~2,500 tokens (scan 2 categories)
  Stage 2: ~2,000 tokens (read 25 candidate descriptions for keyword matching)
  Stage 3: ~400 tokens (detailed trigger evaluation of 4 finalists)
  Stage 4: ~8,000 tokens (load 2 SKILL.md files)
  Total: ~12,900 tokens

Efficiency vs. Loading All Technical+Security Skills:
  All Skills: ~50 skills × 4,000 tokens = ~200,000 tokens
  Multi-Stage: ~12,900 tokens
  Efficiency: 93.5% reduction with high precision (2/2 loaded skills highly relevant)
```

**Strengths:**
- Highest precision and recall
- Systematic elimination of false positives at each stage
- Adaptable to complex tasks requiring multiple skills

**Weaknesses:**
- Higher token cost than simpler algorithms (~12,900 vs. ~2,800 for category-first alone)
- More complex implementation requiring multiple evaluation passes

**When to Use Each Algorithm:**

| Algorithm | Best For | Token Budget | Precision | Recall |
|-----------|----------|--------------|-----------|--------|
| Keyword Matching | Simple technical tasks with clear terminology | ~3,000 tokens | Medium | Medium |
| Category-First | Well-defined task categories, efficient discovery | ~2,800 tokens | High | Medium |
| Trigger Phrase | Scenario-driven tasks with specific use cases | ~4,500 tokens | Very High | High |
| Multi-Stage Refinement | Complex tasks, unfamiliar domains, critical accuracy | ~12,900 tokens | Very High | Very High |

---

## Skill Categorization System

### Hierarchical Category Framework

Zarichney-api skill ecosystem uses five primary categories reflecting skill purpose and target users:

### Category 1: Coordination Skills

**Purpose:** Cross-agent communication and team workflow standardization

**Characteristics:**
- Used by 3+ agents (often all 12 agents)
- Enforce mandatory team protocols
- Enable multi-agent collaboration
- Prevent communication gaps and coordination failures

**Examples:**
- `working-directory-coordination`: Artifact discovery, reporting, context integration protocols
- `core-issue-focus`: Mission discipline ensuring core blocking issues resolved first
- `documentation-grounding`: Standards loading and contextual grounding patterns

**Discovery Triggers:**
- Agent task involves working directory interaction → working-directory-coordination
- Agent task has clearly defined core technical issue requiring focus → core-issue-focus
- Agent starting task requiring standards compliance → documentation-grounding

**Category Directory:** `.claude/skills/coordination/`

**Token Budget:** 2,000-3,500 tokens per SKILL.md (process-focused, moderate complexity)

**Categorization Decision Framework:**
```yaml
Is this skill used by 3+ agents? YES
Does it standardize team communication or workflow? YES
Is it mandatory for team coordination? YES
→ CATEGORY: Coordination

Place in: .claude/skills/coordination/[skill-name]/SKILL.md
```

### Category 2: Technical Skills

**Purpose:** Deep domain expertise and architectural pattern guidance

**Characteristics:**
- Domain-specific (API design, testing, security, frontend, backend)
- Used by 1-3 specialist agents
- Comprehensive technical depth (3,000-5,000 token SKILL.md)
- Extensive resources (8-12 templates/examples/documentation files)

**Examples:**
- `api-design-patterns`: REST and GraphQL design for BackendSpecialist and FrontendSpecialist
- `test-architecture-best-practices`: Comprehensive testing patterns for TestEngineer
- `security-threat-modeling`: OWASP threat assessment for SecurityAuditor

**Discovery Triggers:**
- Agent task involves API design or optimization → api-design-patterns
- Agent creating test strategy or coverage plan → test-architecture-best-practices
- Agent assessing security vulnerabilities → security-threat-modeling

**Category Directory:** `.claude/skills/technical/`

**Token Budget:** 3,000-5,000 tokens per SKILL.md (technical depth, architectural guidance)

**Categorization Decision Framework:**
```yaml
Is this deep domain expertise (not general workflow)? YES
Does it serve 1-3 specialist agents? YES
Does it require 3,000-5,000 token comprehensive guidance? YES
→ CATEGORY: Technical

Place in: .claude/skills/technical/[skill-name]/SKILL.md
```

### Category 3: Meta-Skills

**Purpose:** Creating agents, skills, commands, or other AI system components

**Characteristics:**
- Used exclusively by PromptEngineer (or occasionally Claude)
- Systematic creation frameworks (5-phase workflows)
- Comprehensive resource bundles (12-18 resources)
- Enable AI system evolution and standardization

**Examples:**
- `agent-creation`: Systematic framework for creating new agent definitions
- `skill-creation`: This meta-skill enabling consistent skill design
- `command-creation`: Future meta-skill for slash command development

**Discovery Triggers:**
- PromptEngineer creating new agent → agent-creation
- PromptEngineer creating new skill → skill-creation
- PromptEngineer creating new slash command → command-creation

**Category Directory:** `.claude/skills/meta/`

**Token Budget:** 3,500-5,000 tokens per SKILL.md (methodological frameworks, comprehensive)

**Categorization Decision Framework:**
```yaml
Is this for creating AI system components (agents/skills/commands)? YES
Is this used exclusively by PromptEngineer? YES
Does it provide systematic creation methodology? YES
→ CATEGORY: Meta

Place in: .claude/skills/meta/[skill-name]/SKILL.md
```

### Category 4: Workflow Skills

**Purpose:** Repeatable processes with clear validation steps

**Characteristics:**
- Used by 2+ agents for specific procedural needs
- Step-by-step automation workflows
- Error reduction through standardized processes
- Validation checklists ensuring completion quality

**Examples:**
- `github-issue-creation`: Standardized issue creation for BugInvestigator, ArchitecturalAnalyst
- `pr-analysis-workflow`: Multi-step PR review for ComplianceOfficer, specialists
- `testing-execution`: Test running and reporting for TestEngineer, developers

**Discovery Triggers:**
- Agent creating GitHub issue → github-issue-creation
- Agent reviewing pull request → pr-analysis-workflow
- Agent executing test suite → testing-execution

**Category Directory:** `.claude/skills/workflow/`

**Token Budget:** 2,000-3,500 tokens per SKILL.md (process automation, clear steps)

**Categorization Decision Framework:**
```yaml
Is this a repeatable step-by-step process? YES
Is it used by 2+ agents for specific procedures? YES
Does it automate or standardize workflow execution? YES
→ CATEGORY: Workflow

Place in: .claude/skills/workflow/[skill-name]/SKILL.md
```

### Category 5: Documentation Skills

**Purpose:** Standards loading and contextual grounding patterns

**Characteristics:**
- Used by all file-editing agents (CodeChanger, TestEngineer, DocumentationMaintainer)
- Focus on comprehensive context ingestion
- Enable standards compliance and architectural consistency
- Load project documentation before task execution

**Examples:**
- `documentation-grounding`: Primary standards loading for all file-editing agents
- `standards-compliance-validation`: ComplianceOfficer validation framework
- `readme-maintenance-patterns`: DocumentationMaintainer documentation update guidance

**Discovery Triggers:**
- File-editing agent starting task → documentation-grounding
- ComplianceOfficer validating standards → standards-compliance-validation
- DocumentationMaintainer updating README → readme-maintenance-patterns

**Category Directory:** `.claude/skills/documentation/`

**Token Budget:** 2,500-4,000 tokens per SKILL.md (comprehensive context, standards focus)

**Categorization Decision Framework:**
```yaml
Is this about loading standards or documentation context? YES
Is it used by file-editing agents for grounding? YES
Does it enable standards compliance through context? YES
→ CATEGORY: Documentation

Place in: .claude/skills/documentation/[skill-name]/SKILL.md
```

### Cross-Category Skills (Special Handling)

**Challenge:** Some skills legitimately apply to multiple categories

**Example:**
```yaml
Skill: api-security-patterns
Applicability:
  - Technical: Deep API security architectural patterns (technical category fit)
  - Security: Threat modeling and vulnerability assessment (could be security subcategory)

Decision:
  Primary Category: technical (API domain expertise is primary focus)
  Secondary Tags: security, backend, frontend (metadata tags for cross-category discovery)

Implementation:
  Place in: .claude/skills/technical/api-security-patterns/SKILL.md
  YAML Frontmatter Enhancement:
    ---
    name: api-security-patterns
    description: API security architectural patterns for BackendSpecialist and FrontendSpecialist. Use when designing secure API endpoints, implementing authentication/authorization, or assessing API security vulnerabilities. Integrates security threat modeling with API design best practices.
    tags: security, api-design, backend, frontend, authentication
    ---

Discovery:
  Agent searching "security" category: Won't find in .claude/skills/security/
  Agent searching "technical" category: Finds in .claude/skills/technical/
  Agent keyword matching "security" in descriptions: Matches api-security-patterns description
  Agent using tags (if supported): Matches "security" tag

Result: Keyword and tag matching compensate for single-category placement
```

**Cross-Category Strategy:**
- **Primary Category:** Where skill primarily belongs based on purpose
- **Rich Description:** Include secondary category keywords in description
- **Tags (Optional):** If skill system supports tagging, add cross-category tags
- **Cross-References:** In category README, note cross-category skills

**Example Category README Cross-Reference:**
```markdown
# Technical Skills Category

Primary technical skills in this category focus on domain expertise (API design, testing, security architecture, performance optimization).

## Cross-Category Skills to Consider

Some workflow and security skills may also be relevant for technical tasks:
- **Security Category:** `security-threat-modeling` (threat assessment methodology)
- **Workflow Category:** `pr-analysis-workflow` (technical PR review process)
- **Coordination Category:** `core-issue-focus` (technical implementation discipline)

Check these categories if technical skill search doesn't yield needed capability.
```

---

## Agent-Skill Matching Patterns

### How Different Agents Discover Skills Based on Their Roles

Each agent type has characteristic discovery patterns reflecting their domain and responsibilities:

### Pattern 1: File-Editing Agents (CodeChanger, TestEngineer, DocumentationMaintainer)

**Typical Discovery Needs:**
- Coordination skills (mandatory team protocols)
- Documentation skills (standards grounding)
- Domain-specific technical skills (occasionally)

**Discovery Flow Example: CodeChanger**
```yaml
Task: "Implement new RecipeService method for bulk recipe import"

Discovery Sequence:
  1. Mandatory Coordination Check:
     - Invoke working-directory-coordination (check for existing artifacts)
     - Invoke documentation-grounding (load CodingStandards.md, module README)

  2. Domain Technical Assessment:
     - Task involves API? → Consider api-design-patterns
     - Task involves database? → Consider database-design-patterns
     - Task involves testing? → Defer to TestEngineer (not CodeChanger's domain)

  3. Workflow Automation:
     - Creating new service? → Consider service-implementation-checklist (if exists)

Discovery Pattern: Mandatory coordination first, domain technical as needed, workflow optional

Token Progression:
  - Coordination skills: ~5,000 tokens (working-directory + documentation-grounding)
  - Technical skills: ~4,200 tokens (api-design-patterns if API involved)
  - Workflow skills: ~2,500 tokens (implementation checklist if complex service)
  Total: ~11,700 tokens for comprehensive implementation
```

**Discovery Optimization for File-Editing Agents:**
- **Pre-Load Coordination Skills:** Agent definitions reference mandatory coordination skills
- **Just-in-Time Technical Skills:** Load based on specific task domain
- **Optional Workflow Skills:** Agent assesses task complexity, loads automation if needed

### Pattern 2: Specialist Agents (BackendSpecialist, FrontendSpecialist, SecurityAuditor)

**Typical Discovery Needs:**
- Domain-specific technical skills (primary expertise)
- Coordination skills (team integration)
- Cross-domain technical skills (for integration scenarios)

**Discovery Flow Example: BackendSpecialist**
```yaml
Task: "Design REST API for recipe search with pagination, filtering, and sorting"

Discovery Sequence:
  1. Primary Domain Technical:
     - Invoke api-design-patterns (core backend expertise)
     - Invoke rest-api-best-practices (if exists as specialized skill)

  2. Integration Coordination:
     - Invoke working-directory-coordination (document API contract for FrontendSpecialist)
     - Consider backend-frontend-coordination (if API contract negotiation needed)

  3. Quality and Security:
     - Consider security-threat-modeling (API security assessment)
     - Consider test-architecture-best-practices (API testing strategy)

Discovery Pattern: Domain technical first, coordination for integration, quality/security as needed

Token Progression:
  - Technical skills: ~8,400 tokens (api-design-patterns + rest-api-best-practices)
  - Coordination skills: ~5,000 tokens (working-directory + backend-frontend if needed)
  - Quality skills: ~7,000 tokens (security + testing if comprehensive implementation)
  Total: ~20,400 tokens for comprehensive API design (high token load justified by complexity)
```

**Discovery Optimization for Specialist Agents:**
- **Domain Technical Prominence:** Agent definitions emphasize primary domain skills
- **Integration Awareness:** Cross-domain coordination skills referenced for multi-agent tasks
- **Quality Integration:** Security and testing skills available but not mandatory for all tasks

### Pattern 3: Analysis Agents (BugInvestigator, ArchitecturalAnalyst)

**Typical Discovery Needs:**
- Workflow skills (systematic investigation/analysis processes)
- Coordination skills (working directory artifact creation)
- Technical skills (domain knowledge for assessment)

**Discovery Flow Example: BugInvestigator**
```yaml
Task: "Investigate intermittent recipe search failures in production"

Discovery Sequence:
  1. Analysis Workflow:
     - Invoke bug-investigation-workflow (if exists - systematic root cause analysis)
     - Invoke diagnostic-reporting-patterns (structured bug report format)

  2. Coordination for Handoff:
     - Invoke working-directory-coordination (create diagnostic artifact for CodeChanger)

  3. Domain Technical for Understanding:
     - Consider api-design-patterns (understand API architecture if bug in API layer)
     - Consider database-query-optimization (if bug related to database performance)

Discovery Pattern: Workflow first (systematic process), coordination for handoff, technical for domain understanding

Token Progression:
  - Workflow skills: ~5,000 tokens (investigation + reporting)
  - Coordination skills: ~2,500 tokens (working-directory for artifact creation)
  - Technical skills: ~4,200 tokens (domain understanding for root cause)
  Total: ~11,700 tokens for systematic investigation
```

**Discovery Optimization for Analysis Agents:**
- **Workflow Emphasis:** Agent definitions prioritize systematic analysis processes
- **Artifact Creation Mandatory:** Coordination skills for working directory handoffs to implementation agents
- **Domain Knowledge Optional:** Technical skills loaded when investigation requires architectural understanding

### Pattern 4: Meta-Agent (PromptEngineer)

**Typical Discovery Needs:**
- Meta-skills exclusively (agent/skill/command creation)
- Documentation skills (standards compliance for AI system components)
- Coordination skills (minimal - PromptEngineer often works independently)

**Discovery Flow Example: PromptEngineer**
```yaml
Task: "Create new skill for GitHub Actions workflow optimization"

Discovery Sequence:
  1. Meta-Skill Framework:
     - Invoke skill-creation (systematic 5-phase workflow)
     - Consider agent-creation (reference for skill-agent integration patterns)

  2. Domain Research (Not Skills):
     - Review existing workflow skills for pattern consistency
     - Review WorkflowEngineer agent definition for integration requirements

  3. Standards Compliance:
     - Invoke documentation-grounding (ensure skill follows project standards)

Discovery Pattern: Meta-skill primary, domain research through existing artifacts, standards compliance validation

Token Progression:
  - Meta-skills: ~7,200 tokens (skill-creation + agent-creation reference)
  - Documentation skills: ~3,200 tokens (standards grounding)
  - Total: ~10,400 tokens for systematic skill creation
```

**Discovery Optimization for PromptEngineer:**
- **Meta-Skill Exclusive Domain:** Agent definition emphasizes meta-capabilities
- **Minimal Coordination:** PromptEngineer works on AI system components, less multi-agent coordination
- **Standards Compliance Mandatory:** All created components must follow project standards

### Pattern 5: Orchestrator (Claude - Codebase Manager)

**Typical Discovery Needs:**
- All skill categories (meta-knowledge for delegation)
- Coordination skills (enforcing team protocols)
- Meta-understanding of skill ecosystem (skill-creation for validation)

**Discovery Flow Example: Claude**
```yaml
Task: "Orchestrate multi-agent API implementation (Backend + Frontend + Testing + Documentation)"

Discovery Sequence:
  1. Orchestration Meta-Knowledge:
     - Reference working-directory-coordination (mandatory for all agent engagements)
     - Reference core-issue-focus (ensure agents maintain mission discipline)

  2. Domain Skill Mapping:
     - BackendSpecialist needs: api-design-patterns, backend-specific-patterns
     - FrontendSpecialist needs: api-design-patterns, frontend-integration-patterns
     - TestEngineer needs: test-architecture-best-practices, api-testing-patterns
     - DocumentationMaintainer needs: api-documentation-standards

  3. Context Package Assembly:
     - Include skill references in each agent's context package
     - Agent loads skills as needed during execution (Claude doesn't load all skills, just references them)

Discovery Pattern: Orchestration awareness, skill-agent mapping, context package delegation

Token Progression (Claude's Orchestration):
  - Skill reference awareness: ~200 tokens (Claude knows which skills apply to which agents)
  - Context package skill references: ~80 tokens (4 agents × 20 token reference average)
  - Agent skill loading (not Claude's token budget): ~25,000 tokens across 4 agents
  Total Claude Discovery: ~280 tokens (delegated discovery, not direct loading)
```

**Discovery Optimization for Claude:**
- **Skill Ecosystem Map:** Claude maintains meta-knowledge of all skills and agent applicability
- **Reference-Based Delegation:** Claude includes skill pointers in context packages, agents load as needed
- **Coordination Enforcement:** Claude ensures agents use mandatory coordination skills (working-directory-coordination, etc.)

---

## Trigger Scenario Identification

### Designing Clear Triggers in Skill Descriptions

Effective trigger scenarios in YAML descriptions enable agents to confidently match tasks to skills:

### Trigger Design Patterns

**Pattern 1: Action-Based Triggers (Imperative Verbs)**

**Format:** "Use when [AGENT] [ACTION_VERB] [OBJECT]"

**Examples:**
```yaml
api-design-patterns:
  "Use when BackendSpecialist or FrontendSpecialist **designing** new endpoints, **optimizing** existing APIs, or **resolving** contract integration issues."

working-directory-coordination:
  "Use when **creating**/discovering working directory files to **maintain** team awareness."

github-issue-creation:
  "Use when BugInvestigator or ArchitecturalAnalyst **creating** standardized GitHub issues for **tracking** bugs or technical debt."
```

**Why This Works:**
- Agent tasks often phrased as actions: "Design API", "Create artifact", "Optimize endpoint"
- Action verb matching enables direct task-to-trigger correlation
- Imperative tone aligns with agent task execution mindset

**Pattern 2: Problem-Based Triggers (Situations Requiring Resolution)**

**Format:** "Use when [PROBLEM_SCENARIO] to [DESIRED_OUTCOME]"

**Examples:**
```yaml
core-issue-focus:
  "Use when implementation tasks **risk scope creep or mission drift** to **ensure core blocking issues resolved first**."

security-threat-modeling:
  "Use when assessing **security vulnerabilities** or **designing threat mitigation strategies** to **prevent security breaches**."

bug-investigation-workflow:
  "Use when **encountering production issues** or **diagnosing intermittent failures** to **systematically identify root causes**."
```

**Why This Works:**
- Agents recognize problem scenarios from task context
- Problem-solution framing clarifies skill value proposition
- Agents motivated by desired outcomes, not just processes

**Pattern 3: Artifact-Based Triggers (Document/Output Types)**

**Format:** "Use when creating [ARTIFACT_TYPE] for [PURPOSE]"

**Examples:**
```yaml
documentation-grounding:
  "Use when starting tasks requiring **standards compliance** to **load comprehensive project context** from documentation."

pr-analysis-workflow:
  "Use when reviewing **pull requests** to **validate quality gates** and **provide structured feedback**."

test-report-generation:
  "Use when executing **test suites** to **generate comprehensive coverage reports** and **AI-powered analysis**."
```

**Why This Works:**
- Agents know what artifacts they need to produce
- Artifact type matching is explicit and unambiguous
- Purpose clarification ensures skill addresses agent's specific goal

**Pattern 4: Role-Based Triggers (Agent Type Specification)**

**Format:** "Use when [SPECIFIC_AGENT] [TASK_CONTEXT]"

**Examples:**
```yaml
api-design-patterns:
  "Use when **BackendSpecialist** or **FrontendSpecialist** designing new endpoints..."

skill-creation:
  "Use when **PromptEngineer** needs to create new skills, refactor embedded patterns, or establish skill templates..."

compliance-validation:
  "Use when **ComplianceOfficer** validating standards compliance before PR creation..."
```

**Why This Works:**
- Agents know their own role and can filter by role-specific triggers
- Prevents specialist agents from loading skills meant for other specialists
- Enables Claude to pre-filter skills by agent type during orchestration

**Pattern 5: Complexity-Based Triggers (Conditional Invocation)**

**Format:** "Use when [TASK_COMPLEXITY_INDICATOR]"

**Examples:**
```yaml
advanced-optimization-patterns:
  "Use when **performance bottlenecks identified** and **standard optimizations insufficient**..."

edge-case-handling-guide:
  "Use when **standard workflow doesn't cover scenario** and **custom approach needed**..."

comprehensive-integration-testing:
  "Use when **simple unit tests insufficient** and **multi-component integration validation required**..."
```

**Why This Works:**
- Agents can assess task complexity and selectively load advanced skills
- Prevents over-engineering (simple tasks don't trigger complex skill loading)
- Graduated skill loading based on demonstrated need

### Multi-Trigger Descriptions (Comprehensive Coverage)

**Strategy:** Combine multiple trigger patterns in single description for broad coverage

**Example:**
```yaml
working-directory-coordination:
  description: "Team communication protocols for artifact discovery, immediate reporting, and context integration across multi-agent workflows. Use when **creating/discovering working directory files** to **maintain team awareness**. Prevents communication gaps and enables seamless context flow between agent engagements."

Trigger Analysis:
  - Action-Based: "creating/discovering working directory files"
  - Problem-Based: "Prevents communication gaps"
  - Artifact-Based: "working directory files"
  - Role-Based: Implicit (multi-agent workflows = all agents)
  - Outcome-Based: "maintain team awareness", "seamless context flow"

Coverage: Agent tasks involving working directory interaction match via multiple trigger pathways
```

**Balance:** Include 2-3 trigger patterns max (avoid description bloat approaching 1024 character limit)

### Trigger Clarity Validation Checklist

When designing skill description triggers, validate:

- [ ] **Specificity:** Trigger is specific enough to differentiate this skill from similar skills
- [ ] **Action Clarity:** Agent knows what action/scenario invokes this skill
- [ ] **Role Appropriateness:** Trigger mentions target agents if skill is role-specific
- [ ] **Outcome Visibility:** Agent understands what problem skill solves or artifact it helps create
- [ ] **Brevity:** Trigger phrasing concise (entire description <1024 characters)
- [ ] **Keyword Richness:** Trigger includes keywords agents likely to use in task descriptions

---

## Discovery Efficiency Optimization

### Strategies for Maintaining Sub-10,000 Token Discovery Budgets

As skill ecosystem scales, discovery efficiency requires proactive optimization:

### Optimization Strategy 1: Category README Navigation Guides

**Approach:** Each category directory includes README.md with skill summaries and decision trees

**Example: `.claude/skills/technical/README.md`**
```markdown
# Technical Skills Category

Domain expertise and architectural pattern guidance for specialist agents.

## Quick Navigation

**API & Backend:**
- `api-design-patterns` - REST/GraphQL design for BackendSpecialist and FrontendSpecialist
- `database-design-patterns` - Schema design and query optimization for BackendSpecialist
- `backend-service-architecture` - Microservices and service layer patterns

**Testing:**
- `test-architecture-best-practices` - Comprehensive testing patterns for TestEngineer
- `integration-testing-strategies` - Multi-component testing workflows

**Security:**
- `security-threat-modeling` - OWASP threat assessment for SecurityAuditor
- `authentication-patterns` - Auth implementation for BackendSpecialist

**Frontend:**
- `component-design-patterns` - Angular component architecture for FrontendSpecialist
- `state-management-patterns` - NgRx state design for FrontendSpecialist

## Decision Tree

**Task involves API?** → `api-design-patterns`
**Task involves database?** → `database-design-patterns`
**Task involves testing strategy?** → `test-architecture-best-practices`
**Task involves security assessment?** → `security-threat-modeling`
**Task involves frontend components?** → `component-design-patterns`
**Task involves state management?** → `state-management-patterns`
```

**Benefit:**
- Agent loads category README (~800 tokens) instead of scanning all skill descriptions (~3,000 tokens)
- Decision tree provides fast navigation to correct skill
- Skill summaries enable quick relevance assessment before loading full SKILL.md

**Discovery Flow with README:**
```yaml
Agent Task: "Design database schema for recipe tags and categories"

Discovery:
  1. Identify Category: "technical" (database design is technical domain)
  2. Load Category README: .claude/skills/technical/README.md (~800 tokens)
  3. Scan Quick Navigation: "Database: database-design-patterns"
  4. Validate via Summary: "Schema design and query optimization for BackendSpecialist"
  5. Load Skill: database-design-patterns/SKILL.md (~4,200 tokens)

Total Discovery: ~5,000 tokens vs. ~8,000 tokens scanning all technical skill descriptions
Efficiency: 38% reduction through category README navigation
```

### Optimization Strategy 2: Skill Naming Convention Consistency

**Approach:** Standardize naming patterns enabling predictive discovery

**Naming Patterns:**
```yaml
Domain-Pattern Format:
  [domain]-[capability]-[type]
  Examples:
    - api-design-patterns (domain: api, capability: design, type: patterns)
    - test-architecture-best-practices (domain: test, capability: architecture, type: best-practices)
    - security-threat-modeling (domain: security, capability: threat, type: modeling)

Workflow Format:
  [platform/tool]-[action]-[type]
  Examples:
    - github-issue-creation (platform: github, action: issue-creation, type: workflow)
    - pr-analysis-workflow (platform: pr, action: analysis, type: workflow)

Coordination Format:
  [pattern]-[type]
  Examples:
    - working-directory-coordination (pattern: working-directory, type: coordination)
    - core-issue-focus (pattern: core-issue, type: focus)
```

**Benefit:**
- Agents learn naming conventions, can predict skill names from task domain
- Consistency reduces discovery ambiguity
- Alphabetical sorting in directories clusters related skills

**Predictive Discovery Example:**
```yaml
Agent Task: "Implement authentication for API endpoints"

Agent Reasoning:
  "Task involves authentication (domain) and implementation (capability). Likely skill name: authentication-implementation-patterns or similar."

Agent Discovery:
  1. Navigate to technical/ category (domain expertise)
  2. Scan skill names for "authentication":
     - authentication-implementation-patterns (FOUND)
     OR
     - api-security-patterns (broader, includes authentication)

  3. Load identified skill

Discovery Efficiency: Name-based prediction reduces need to read all descriptions
```

### Optimization Strategy 3: Tiered Skill Complexity Labeling

**Approach:** Skills labeled by complexity tier enabling graduated discovery

**Complexity Tiers:**
```yaml
Tier 1 - Essentials (2,000-2,500 tokens):
  - Core workflows without extensive resources
  - For routine tasks requiring standard processes
  - Examples: working-directory-coordination-essentials, api-design-basics

Tier 2 - Standard (2,500-3,500 tokens):
  - Complete workflows with moderate resources
  - For typical tasks requiring comprehensive guidance
  - Examples: api-design-patterns, test-architecture-best-practices

Tier 3 - Comprehensive (3,500-5,000 tokens):
  - Extensive workflows with comprehensive resources
  - For complex tasks requiring deep expertise
  - Examples: security-threat-modeling-comprehensive, architectural-decision-framework

Tier 4 - Advanced (5,000+ tokens):
  - Expert-level guidance with extensive resources
  - For edge cases and optimization scenarios
  - Examples: performance-optimization-advanced, distributed-systems-patterns
```

**YAML Frontmatter Enhancement:**
```yaml
---
name: api-design-patterns
description: [Standard description]
complexity: standard
tier: 2
---
```

**Discovery Benefit:**
```yaml
Agent Task: "Quick API endpoint creation following existing patterns"

Agent Discovery:
  1. Identifies need for API design guidance
  2. Assesses task complexity: Simple/routine
  3. Filters to Tier 1-2 skills (avoid loading Tier 3-4 comprehensive guides)
  4. Loads api-design-essentials (Tier 1, ~2,200 tokens) instead of api-design-patterns (Tier 2, ~4,200 tokens)

Token Savings: 2,000 tokens for simple task getting essential guidance vs. comprehensive
```

### Optimization Strategy 4: Skill Dependency Mapping

**Approach:** Skills declare dependencies on other skills, enabling discovery chains

**YAML Frontmatter Enhancement:**
```yaml
---
name: comprehensive-api-implementation
description: [Description]
depends_on:
  - api-design-patterns
  - working-directory-coordination
  - test-architecture-best-practices
related:
  - documentation-grounding
  - security-threat-modeling
---
```

**Discovery Benefit:**
```yaml
Agent Task: "Comprehensive API implementation from design through testing"

Agent Discovery:
  1. Loads comprehensive-api-implementation SKILL.md (~3,000 tokens)
  2. SKILL.md workflow references: "Invoke api-design-patterns for Phase 1 design"
  3. Agent checks depends_on: api-design-patterns listed
  4. Agent loads api-design-patterns (~4,200 tokens) when reaching Phase 1
  5. Continues through workflow, loading test-architecture-best-practices for testing phase

Efficiency:
  - Sequential loading based on workflow phases (not all dependencies loaded upfront)
  - Dependency metadata prepares agent for upcoming skill invocations
  - Related skills available but not mandatory (loaded if needed)

Dependency-Aware Discovery: Agents follow skill-defined paths rather than trial-and-error
```

---

## Integration Point Design

### How Skills Reference Each Other and Coordinate

Skills don't exist in isolation - effective skill ecosystem requires clear integration patterns:

### Integration Pattern 1: Skill Composition (Skills Invoking Skills)

**Use Case:** Complex workflows decomposing into existing skill capabilities

**Example: comprehensive-api-implementation Skill**
```markdown
## WORKFLOW STEPS

### Phase 1: API Contract Design

**Process:**
1. Define API requirements and constraints
2. **Invoke api-design-patterns skill** for comprehensive contract design
3. Document contract in working directory using **working-directory-coordination skill**

**Skill Invocations:**

#### api-design-patterns
- **Scenario:** "Designing New REST Endpoint"
- **Expected Output:** API contract specification following zarichney-api patterns
- **Integration:** Use contract output as input to Phase 2 validation

#### working-directory-coordination
- **Scenario:** "Immediate Artifact Reporting"
- **Expected Output:** Artifact created in working directory for FrontendSpecialist review
- **Integration:** Contract artifact enables multi-agent coordination
```

**Benefit:**
- DRY principle for skills (api-design-patterns owns API design, no duplication)
- Clear invocation points guide agent through complex workflow
- Integration expectations set upfront (agent knows what skill produces and how to use it)

### Integration Pattern 2: Skill Handoff (Sequential Skill Usage)

**Use Case:** Agent completes one skill workflow, hands off to another agent via artifact

**Example Flow:**
```yaml
Agent 1 (BugInvestigator):
  Skill: bug-investigation-workflow
  Workflow:
    - Systematically diagnose issue
    - Identify root cause
    - Create diagnostic artifact using working-directory-coordination skill
  Output: /working-dir/bug-diagnosis-recipe-search-failure.md

Agent 2 (CodeChanger):
  Skill: working-directory-coordination (discovery phase)
  Workflow:
    - Scan working directory (finds bug-diagnosis artifact)
    - Load artifact context
    - Implement fix based on root cause from artifact
  Integration: Artifact from Agent 1 informs Agent 2's implementation approach
```

**Integration Point Design in Skills:**

**In bug-investigation-workflow SKILL.md:**
```markdown
### Step 4: Create Diagnostic Artifact

**Process:**
1. Synthesize investigation findings
2. **Invoke working-directory-coordination skill** for artifact reporting
3. Format diagnostic artifact for CodeChanger handoff

**Handoff Preparation:**
- **Target Agent:** CodeChanger (or domain specialist)
- **Artifact Contains:** Root cause analysis, reproduction steps, proposed fix approach
- **Expected Next Action:** Implementation agent loads artifact, implements fix
```

**In working-directory-coordination SKILL.md:**
```markdown
### Step 1: Pre-Work Artifact Discovery

**Process:**
1. Scan /working-dir/ for existing artifacts
2. Identify relevant context from other agents
   - BugInvestigator: Diagnostic reports
   - ArchitecturalAnalyst: Design recommendations
   - Specialists: Implementation plans

**Integration with Agent Workflows:**
- **After Discovery:** Load artifact content informs current task approach
- **Handoff Recognition:** Artifacts explicitly prepared for current agent type
```

**Benefit:**
- Explicit handoff preparation in skill workflows
- Agents know what to expect from other agents' skill outputs
- Working directory becomes structured coordination mechanism

### Integration Pattern 3: Skill Complementarity (Parallel Skill Usage)

**Use Case:** Agent uses multiple skills simultaneously for comprehensive task execution

**Example: BackendSpecialist Implementing Secure API**
```yaml
Skills Invoked in Parallel:
  1. api-design-patterns:
     - Provides REST endpoint design guidance
     - Contract structure and validation patterns

  2. security-threat-modeling:
     - Provides authentication/authorization patterns
     - Threat assessment for API security

  3. working-directory-coordination:
     - Artifact reporting for FrontendSpecialist coordination
     - Context integration from ArchitecturalAnalyst's design

  4. test-architecture-best-practices:
     - API testing strategy for validation
     - Coverage requirements for secure endpoints

Complementarity:
  - api-design-patterns: "What" (endpoint structure)
  - security-threat-modeling: "How secure" (security implementation)
  - working-directory-coordination: "Team awareness" (coordination)
  - test-architecture-best-practices: "Validation" (quality assurance)
```

**Integration Point Design for Complementary Skills:**

**In api-design-patterns SKILL.md:**
```markdown
### Step 3: Security Integration

**Process:**
1. Design API endpoint authentication requirements
2. **Consider security-threat-modeling skill** for comprehensive threat assessment
3. Implement security patterns recommended by threat modeling

**Complementary Skill:**
- **Skill:** security-threat-modeling
- **When to Invoke:** API handles sensitive data OR requires authentication
- **Integration:** Threat model informs which security patterns to apply in API design
```

**In security-threat-modeling SKILL.md:**
```markdown
### Step 2: Identify Threats

**Process:**
1. Analyze API attack surface
2. Reference **api-design-patterns skill** for understanding API architecture
3. Apply OWASP threat categories

**Complementary Skill:**
- **Skill:** api-design-patterns
- **When to Reference:** Understanding existing API structure before threat assessment
- **Integration:** API design documentation informs threat modeling inputs
```

**Benefit:**
- Skills explicitly acknowledge complementary relationships
- Agents guided to invoke multiple skills for comprehensive execution
- Clear integration points prevent skill overlap or gaps

### Integration Pattern 4: Skill Prerequisite Chains

**Use Case:** Skill A requires outputs from Skill B before execution

**Example: pr-creation-workflow Requires working-directory-coordination**
```yaml
Prerequisite Chain:
  1. Agent completes implementation work
  2. Agent uses working-directory-coordination to create artifacts
  3. Artifacts exist in /working-dir/ documenting changes
  4. Agent invokes pr-creation-workflow
  5. pr-creation-workflow references artifacts in PR description

Dependency:
  - pr-creation-workflow depends on working-directory-coordination artifacts
  - Cannot create comprehensive PR without artifacts documenting work
```

**Integration Point Design in Dependent Skill:**

**In pr-creation-workflow SKILL.md:**
```markdown
## PREREQUISITE SKILLS

Before invoking pr-creation-workflow, ensure:

- [ ] **working-directory-coordination** executed
  - Artifacts created documenting implementation approach
  - Context integration documented if building on other agents' work

- [ ] **test-execution-workflow** completed (if applicable)
  - Test results available for PR validation
  - Coverage reports included in PR context

**If Prerequisites Not Met:**
1. Complete prerequisite skills first
2. Validate artifacts exist: `ls /working-dir/`
3. Then proceed with pr-creation-workflow

### Step 1: Gather PR Context

**Process:**
1. Load working directory artifacts created during implementation
2. Extract key points for PR description
3. Reference artifact URLs in PR body
```

**Benefit:**
- Explicit prerequisite declaration prevents incomplete workflow execution
- Agents validate prerequisites before invoking dependent skills
- Integration points clearly defined (where prerequisite outputs used)

---

## Multi-Skill Coordination Discovery

### Discovering Skill Combinations for Complex Tasks

Complex tasks often require multiple skills - agents must discover complementary skill sets:

### Coordination Discovery Pattern 1: Task Decomposition

**Agent Analysis:**
```yaml
Agent: BackendSpecialist
Task: "Implement comprehensive recipe search API with security, testing, and documentation"

Task Decomposition:
  1. API Design → Skill: api-design-patterns
  2. Security Implementation → Skill: security-threat-modeling
  3. Testing Strategy → Skill: test-architecture-best-practices
  4. Documentation → Skill: api-documentation-standards
  5. Team Coordination → Skill: working-directory-coordination

Discovery Sequence:
  Stage 1: Identify primary domain skill (api-design-patterns)
  Stage 2: Load primary skill, identify integration points
  Stage 3: Primary skill references complementary skills (security, testing, documentation)
  Stage 4: Agent loads complementary skills as workflow progresses

Token Progression:
  - Stage 1-2: ~4,200 tokens (api-design-patterns)
  - Stage 3: ~7,000 tokens (security + testing skills)
  - Stage 4: ~5,500 tokens (documentation + coordination)
  Total: ~16,700 tokens for comprehensive implementation
```

**How Skills Guide Multi-Skill Discovery:**

**In api-design-patterns SKILL.md:**
```markdown
### Step 5: Security and Quality Integration

**Process:**
1. Assess API security requirements
2. **If API handles authentication OR sensitive data:**
   - **Invoke security-threat-modeling skill** for comprehensive threat assessment
3. **If API critical or complex:**
   - **Invoke test-architecture-best-practices skill** for testing strategy
4. **Always:**
   - **Invoke api-documentation-standards skill** for API documentation

**Multi-Skill Coordination:**
Skills working together for comprehensive API implementation:
- **api-design-patterns:** Core API structure (this skill)
- **security-threat-modeling:** Security assessment and mitigation
- **test-architecture-best-practices:** Testing and validation strategy
- **api-documentation-standards:** API contract documentation
- **working-directory-coordination:** Team coordination and artifact sharing
```

**Benefit:**
- Primary skill guides agent to complementary skills
- Conditional invocation (security skill only if needed)
- Coordination section provides ecosystem view

### Coordination Discovery Pattern 2: Agent Role-Based Skill Sets

**Claude Orchestration Perspective:**

```yaml
Claude's Agent-Skill Mapping (Meta-Knowledge):

BackendSpecialist Default Skill Set:
  - Mandatory: working-directory-coordination, documentation-grounding
  - Primary Domain: api-design-patterns, database-design-patterns, backend-service-architecture
  - Complementary: security-threat-modeling, test-architecture-best-practices

FrontendSpecialist Default Skill Set:
  - Mandatory: working-directory-coordination, documentation-grounding
  - Primary Domain: component-design-patterns, state-management-patterns, api-integration-patterns
  - Complementary: security-threat-modeling (client-side security)

TestEngineer Default Skill Set:
  - Mandatory: working-directory-coordination, documentation-grounding
  - Primary Domain: test-architecture-best-practices, integration-testing-strategies, coverage-analysis
  - Complementary: api-design-patterns (for API testing context)

Claude's Context Package Assembly:
  When delegating to BackendSpecialist for API task:
    - Include references to: api-design-patterns, security-threat-modeling
    - Agent loads skills as needed during execution
    - Claude doesn't load full skills (just knows which to reference)
```

**Benefit:**
- Claude pre-filters relevant skills by agent role
- Agents receive skill references in context package
- Discovery already partially completed by orchestration

### Coordination Discovery Pattern 3: Skill Discovery from Examples

**Learning from Existing Artifacts:**

```yaml
Scenario: Agent unfamiliar with task, searches working directory for similar past work

Agent Discovery Flow:
  1. Agent searches: `ls /working-dir/` for related artifacts
  2. Agent finds: previous-api-implementation-plan.md
  3. Agent loads artifact, reads:
     "Implementation followed api-design-patterns skill for REST contract design,
      integrated security-threat-modeling for authentication patterns, and used
      test-architecture-best-practices for test strategy."
  4. Agent discovers: 3 skills used in similar past task
  5. Agent invokes same 3 skills for current task

Discovery via Example Artifacts:
  - Past agent work documents skills used
  - Current agent learns from artifact history
  - Skill discovery through team knowledge repository
```

**How to Design Artifacts to Aid Skill Discovery:**

**Artifact Template Enhancement:**
```markdown
# API Implementation Plan: Recipe Search Endpoint

## Skills Used

This implementation followed these skill workflows:
- **api-design-patterns:** REST endpoint contract design (Phase 1-3)
- **security-threat-modeling:** Authentication and authorization patterns (Phase 2)
- **test-architecture-best-practices:** API testing strategy (Phase 4)
- **working-directory-coordination:** Artifact creation for Frontend coordination

**Rationale:** Comprehensive API implementation requires design, security, testing, and coordination skills working together.

[Rest of artifact content...]
```

**Benefit:**
- Future agents learn skill combinations from past successful implementations
- Skill discovery becomes institutional knowledge encoded in artifacts
- Reduces discovery trial-and-error for unfamiliar tasks

---

## Discovery Patterns from Examples

### Extracting Discovery Strategies from Existing Skill Examples

This section consolidates discovery patterns demonstrated in coordination-skill-example.md, technical-skill-example.md, and meta-skill-example.md:

### Discovery Pattern A: Mandatory Universal Discovery (From working-directory-coordination)

**Source:** coordination-skill-example.md

**Discovery Characteristic:**
- Skill used by ALL agents (100% coverage)
- Discovery is mandatory, not optional
- Agent definitions include explicit skill reference

**How Agents Discover:**
```yaml
Method: Agent Definition Embedded Reference

In BackendSpecialist Agent Definition:
  ### working-directory-coordination
  **Purpose:** Team communication protocols for artifact discovery and reporting
  **Key Workflow:** Pre-work discovery → Immediate reporting → Context integration
  **Integration:** Execute all 3 protocols for every working directory interaction

  **TRIGGER:** Before starting ANY task (pre-work discovery) and when creating/updating
              working directory files (immediate reporting)

Discovery: Agent doesn't "discover" this skill dynamically - it's always present in agent definition
Loading: Agent loads SKILL.md at start of any task involving working directory

Token Efficiency:
  - Agent definition reference: ~22 tokens
  - SKILL.md loaded when needed: ~2,580 tokens
  - Total: ~2,602 tokens vs. ~150 tokens if embedded (but embedded insufficient for execution)
```

**Insight for New Coordination Skills:**
- If skill used by 100% of agents 100% of time, embed reference in ALL agent definitions
- Discovery becomes "always known" rather than "must search"
- Trade-off: Slightly higher base agent token count, but guaranteed coordination compliance

### Discovery Pattern B: Domain-Specific Targeted Discovery (From api-design-patterns)

**Source:** technical-skill-example.md

**Discovery Characteristic:**
- Skill used by 2-3 specialist agents (BackendSpecialist, FrontendSpecialist)
- Discovery triggered by specific domain task (API design/implementation)
- Category and naming enable predictive discovery

**How Agents Discover:**
```yaml
Method: Category-First + Keyword Matching

Agent: BackendSpecialist
Task: "Design REST endpoint for recipe search"

Discovery Flow:
  1. Agent recognizes: "Design" + "REST endpoint" = API design domain
  2. Agent searches: .claude/skills/technical/ (domain expertise category)
  3. Agent scans names: api-design-patterns (EXACT MATCH on "api" and "design")
  4. Agent validates description: "REST and GraphQL design following .NET 8 backend architecture"
  5. Agent loads: api-design-patterns/SKILL.md

Token Progression:
  - Category navigation: ~800 tokens (scan technical/ README)
  - Name matching: 0 additional tokens (found immediately)
  - Description validation: ~100 tokens (confirm relevance)
  - Skill loading: ~4,200 tokens
  Total: ~5,100 tokens
```

**Insight for New Technical Skills:**
- Name skills with domain keywords agents recognize (api, database, security, testing)
- Place in technical/ category for specialist discovery
- Description emphasizes domain and applicable agents

### Discovery Pattern C: Meta-Skill Exclusive Discovery (From skill-creation)

**Source:** meta-skill-example.md (skill-creation itself)

**Discovery Characteristic:**
- Skill used exclusively by PromptEngineer (single agent)
- Discovery occurs only during specific creation tasks
- Meta-skill category clearly differentiates from operational skills

**How Agent Discovers:**
```yaml
Method: Role-Aware Category Navigation

Agent: PromptEngineer
Task: "Create new skill for GitHub Actions workflow optimization"

Discovery Flow:
  1. Agent recognizes: "Create new skill" = Meta-capability need
  2. Agent navigates: .claude/skills/meta/ (exclusive PromptEngineer category)
  3. Agent scans 3 meta-skills: agent-creation, skill-creation, command-creation
  4. Agent matches: "Create new skill" → skill-creation (EXACT PURPOSE)
  5. Agent loads: skill-creation/SKILL.md

Token Progression:
  - Category navigation: ~300 tokens (meta/ category very small)
  - Skill scanning: ~300 tokens (only 3 skills in category)
  - Skill loading: ~3,600 tokens
  Total: ~4,200 tokens

Efficiency: Category exclusivity (meta/ only for PromptEngineer) prevents other agents from wasting tokens scanning meta-skills
```

**Insight for New Meta-Skills:**
- Clearly separate meta-skills (agent/skill/command creation) from operational skills
- Category name signals exclusive PromptEngineer domain
- Descriptions emphasize "PromptEngineer" as user to prevent misuse by other agents

### Discovery Pattern D: Workflow Trigger-Based Discovery (From github-issue-creation)

**Source:** Hypothetical workflow skill (pattern observed across workflow skills)

**Discovery Characteristic:**
- Skill triggered by specific action (creating GitHub issue)
- Used by 2+ analysis agents (BugInvestigator, ArchitecturalAnalyst)
- Discovery based on action verb matching

**How Agents Discover:**
```yaml
Method: Action-Verb + Artifact-Type Matching

Agent: BugInvestigator
Task: "Create GitHub issue documenting production bug for team tracking"

Discovery Flow:
  1. Agent extracts action: "Create" + artifact: "GitHub issue"
  2. Agent searches workflow/ category: Process automation for repeatable tasks
  3. Agent keyword matches: "github" + "issue" + "creation"
  4. Agent finds: github-issue-creation skill
  5. Agent validates description: "Standardized issue creation for BugInvestigator, ArchitecturalAnalyst"
  6. Agent loads: github-issue-creation/SKILL.md

Token Progression:
  - Category identification: ~600 tokens (workflow/ category README)
  - Keyword matching: ~800 tokens (scan 8 workflow skill descriptions)
  - Skill loading: ~2,800 tokens
  Total: ~4,200 tokens
```

**Insight for New Workflow Skills:**
- Name skills with action verb + platform/artifact type (github-issue-creation, pr-analysis-workflow)
- Description includes target agents to enable role-based filtering
- Workflow category signals repeatable process automation

---

## Scaling Discovery to 100+ Skills

### Maintaining Discovery Efficiency as Ecosystem Grows

Discovery strategies must adapt as skill count increases from 10 to 100+:

### Challenge: Linear Scanning Doesn't Scale

**Current Small Ecosystem (15 Skills):**
```yaml
Discovery Approach: Scan all skill descriptions
Token Cost: 15 skills × 100 tokens = ~1,500 tokens
Acceptable: 1,500 tokens is reasonable discovery overhead

Future Large Ecosystem (100 Skills):
Discovery Approach: Scan all skill descriptions
Token Cost: 100 skills × 100 tokens = ~10,000 tokens
Problem: 10,000 tokens purely for discovery (before loading any skills)
```

### Scaling Solution 1: Hierarchical Category Structure

**Expand Categories with Subcategories:**

```
.claude/skills/
├── coordination/
│   ├── artifact-management/
│   ├── team-communication/
│   └── quality-gates/
├── technical/
│   ├── backend/
│   │   ├── api/
│   │   ├── database/
│   │   └── services/
│   ├── frontend/
│   │   ├── components/
│   │   ├── state/
│   │   └── routing/
│   ├── testing/
│   ├── security/
│   └── performance/
├── workflow/
│   ├── github/
│   ├── ci-cd/
│   └── deployment/
├── meta/
└── documentation/
```

**Discovery Efficiency:**
```yaml
Before Subcategories:
  Agent searches technical/ category: 40 skills × 100 tokens = ~4,000 tokens

After Subcategories:
  Agent searches technical/backend/api/ subcategory: 6 skills × 100 tokens = ~600 tokens
  Efficiency: 85% reduction (4,000 → 600 tokens)
```

**Navigation Example:**
```yaml
Agent Task: "Optimize database query performance for recipe search"

Hierarchical Discovery:
  1. Top Category: technical (domain expertise)
  2. Subcategory: backend (backend specialist domain)
  3. Sub-subcategory: database (database-specific)
  4. Scan 6 database skills: query-optimization, schema-design, indexing-strategies, etc.
  5. Match: query-optimization-patterns

Token Progression: ~800 tokens (vs. ~4,000 scanning all technical skills)
```

### Scaling Solution 2: Skill Tagging and Faceted Search

**YAML Frontmatter Enhancement:**
```yaml
---
name: api-security-patterns
description: [Standard description]
tags:
  - api
  - security
  - backend
  - frontend
  - authentication
  - authorization
agents:
  - BackendSpecialist
  - FrontendSpecialist
  - SecurityAuditor
complexity: standard
---
```

**Tag-Based Discovery:**
```yaml
Agent Task: "Implement OAuth2 authentication for API"

Tag-Based Search:
  1. Agent extracts tags from task: authentication, api, security
  2. Agent queries: Skills with tags matching ["authentication", "api", "security"]
  3. Results: api-security-patterns (all 3 tags match), authentication-implementation (2 tags match)
  4. Rank by match count: api-security-patterns (3/3 = 100%)
  5. Load top result

Token Efficiency:
  - Tag extraction: ~200 tokens (agent reasoning)
  - Tag matching: ~500 tokens (scan tags only, not full descriptions)
  - Skill loading: ~4,200 tokens
  Total: ~4,900 tokens vs. ~10,000 scanning all 100 skill descriptions
  Efficiency: 51% reduction
```

**Faceted Search Example:**
```yaml
Agent Filters:
  - Agent: BackendSpecialist (filter to skills applicable to BackendSpecialist)
  - Domain: API (filter to API-related skills)
  - Complexity: standard (filter out advanced/comprehensive tiers for routine task)

Results:
  - api-design-patterns (matches all filters)
  - api-security-patterns (matches all filters)
  - 2 candidates vs. 100 total skills

Discovery Efficiency: 98% reduction in scanning scope
```

### Scaling Solution 3: Skill Discovery Index

**Centralized Skill Index File:**

**`.claude/skills/SKILL_INDEX.md`:**
```markdown
# Skill Ecosystem Index

**Total Skills:** 100
**Last Updated:** 2025-10-25

## Quick Reference by Agent

### BackendSpecialist (18 skills)
- **API Design:** api-design-patterns, rest-best-practices, graphql-patterns
- **Database:** database-design-patterns, query-optimization, migration-strategies
- **Services:** backend-service-architecture, microservices-patterns
- **Security:** api-security-patterns, authentication-implementation
- [Full list...]

### FrontendSpecialist (15 skills)
- **Components:** component-design-patterns, angular-best-practices
- **State:** state-management-patterns, ngrx-architecture
- **API Integration:** api-integration-patterns, http-client-patterns
- [Full list...]

### TestEngineer (12 skills)
- **Testing Strategy:** test-architecture-best-practices, coverage-analysis
- **Frameworks:** unit-testing-patterns, integration-testing-strategies
- [Full list...]

## Quick Reference by Domain

### API & Backend (22 skills)
api-design-patterns, rest-best-practices, graphql-patterns, database-design-patterns,
query-optimization, backend-service-architecture, api-security-patterns, [...]

### Frontend & UI (18 skills)
component-design-patterns, state-management-patterns, angular-best-practices, [...]

### Testing & Quality (15 skills)
test-architecture-best-practices, coverage-analysis, unit-testing-patterns, [...]

### Security (12 skills)
security-threat-modeling, api-security-patterns, authentication-implementation, [...]

## Quick Reference by Task Type

### Creating/Implementing
- **New API Endpoint:** api-design-patterns
- **Database Schema:** database-design-patterns
- **Frontend Component:** component-design-patterns
- **Test Suite:** test-architecture-best-practices
- **GitHub Issue:** github-issue-creation

### Analyzing/Reviewing
- **Security Assessment:** security-threat-modeling
- **Code Review:** code-review-checklist
- **Architecture Review:** architectural-decision-framework
- **Performance Analysis:** performance-optimization-patterns

### Optimizing/Refactoring
- **Query Performance:** query-optimization-patterns
- **Code Quality:** refactoring-patterns
- **Test Coverage:** coverage-improvement-strategies
```

**Discovery with Index:**
```yaml
Agent Task: "Create new API endpoint"

Index-Based Discovery:
  1. Agent loads: .claude/skills/SKILL_INDEX.md (~6,000 tokens)
  2. Agent searches: "Creating/Implementing → New API Endpoint"
  3. Index recommends: api-design-patterns
  4. Agent loads: api-design-patterns/SKILL.md (~4,200 tokens)
  Total: ~10,200 tokens

vs. Scanning 100 Skills:
  - Scan all descriptions: ~10,000 tokens
  - Find api-design-patterns after scanning 30 skills: wasted ~3,000 tokens on irrelevant
  - Load skill: ~4,200 tokens
  Total: ~14,200 tokens

Efficiency: 28% reduction + faster navigation through curated index
```

**Index Maintenance:**
- Updated automatically when new skills added (PromptEngineer updates index as part of skill-creation workflow)
- Organized by multiple facets (agent, domain, task type)
- Token cost amortized across multiple discoveries (one index load enables multiple skill finds)

---

## Discovery Failure Diagnosis

### Troubleshooting When Agents Can't Find Relevant Skills

Discovery failures waste tokens and delay task execution. Systematic diagnosis resolves issues:

### Failure Mode 1: Skill Exists But Not Discovered

**Symptoms:**
- Agent searches for skill capability
- Scans multiple skills, doesn't find match
- Agent improvises solution or escalates to Claude
- Post-task analysis reveals relevant skill existed

**Diagnostic Questions:**
1. Is skill in expected category for agent's discovery path?
2. Does skill name include keywords agent would search for?
3. Does skill description mention agent's trigger scenario?
4. Is skill description clear about "when to use"?

**Example Failure:**
```yaml
Skill: working-directory-coordination
Agent: SecurityAuditor
Task: "Create security assessment artifact in working directory for team review"

Discovery Failure:
  Agent searches: "security" keyword → Finds security-threat-modeling, api-security-patterns
  Agent searches: "artifact" keyword → Finds artifact-creation-workflow (generic, not working directory specific)
  Agent misses: working-directory-coordination (description emphasizes "team communication" and "artifact reporting")
  Agent doesn't recognize: "team communication" = security assessment artifact reporting

Root Cause: Description lacks "security" keyword or "security assessment artifact" trigger phrase
```

**Resolution:**
```yaml
Enhanced Description:
  Before: "Team communication protocols for artifact discovery, immediate reporting, and context
          integration across multi-agent workflows. Use when creating/discovering working directory
          files to maintain team awareness."

  After: "Team communication protocols for artifact discovery, immediate reporting, and context
         integration across multi-agent workflows. Use when creating/discovering working directory
         files (analysis reports, security assessments, implementation plans, diagnostic artifacts)
         to maintain team awareness."

Change: Added specific artifact types (security assessments) to trigger phrase
Result: SecurityAuditor keyword "security assessment" now matches description
```

### Failure Mode 2: Multiple Similar Skills Cause Confusion

**Symptoms:**
- Agent discovers 4-5 candidate skills
- Skill names/descriptions very similar
- Agent uncertain which to load
- Agent loads wrong skill (false positive)

**Example Failure:**
```yaml
Agent Task: "Test new API endpoint integration"

Discovery:
  - test-architecture-best-practices (comprehensive testing patterns)
  - api-testing-patterns (API-specific testing guidance)
  - integration-testing-strategies (integration test workflows)
  - unit-testing-patterns (unit test creation)

Agent Confusion:
  - All 4 skills mention "testing" and "API"
  - Descriptions lack clear differentiation
  - Agent loads test-architecture-best-practices (too broad)
  - Needed api-testing-patterns (API-specific)

Root Cause: Overlapping skill scopes, descriptions don't clarify differentiation
```

**Resolution:**
```yaml
Skill Differentiation in Descriptions:

test-architecture-best-practices:
  "**Comprehensive testing strategy** for TestEngineer designing test approach across unit,
   integration, and E2E testing. Use when creating **test architecture plan** or establishing
   **coverage goals**. For specific test types, see focused skills: api-testing-patterns,
   unit-testing-patterns, integration-testing-strategies."

api-testing-patterns:
  "**API-specific testing patterns** for validating REST/GraphQL endpoints, contracts, and
   integrations. Use when testing **API functionality, request/response validation, or endpoint
   integration**. Part of test-architecture-best-practices strategy; complements broader testing
   approach with API domain expertise."

Change: Each skill description now explicitly differentiates scope and references complementary skills
Result: Agent matches "test API endpoint" → api-testing-patterns (API domain specific), understands relationship to test-architecture
```

### Failure Mode 3: Category Mismatch

**Symptoms:**
- Agent searches category where skill logically belongs
- Skill actually in different category
- Discovery fails despite skill existing

**Example Failure:**
```yaml
Agent: BugInvestigator
Task: "Create GitHub issue documenting production bug"

Discovery:
  Agent searches: .claude/skills/workflow/ (GitHub issue creation is workflow)
  Skill Actually Located: .claude/skills/coordination/ (issue creation viewed as team coordination)
  Discovery Fails: Agent scans workflow/, doesn't find github-issue-creation
  Agent Misses: Skill exists in coordination/ category

Root Cause: Categorization decision didn't match agent's mental model
```

**Resolution Options:**

**Option A: Recategorize Skill**
```yaml
Move: github-issue-creation from coordination/ to workflow/
Rationale: Issue creation is repeatable workflow, not cross-agent coordination protocol
```

**Option B: Cross-Category Reference**
```yaml
In .claude/skills/workflow/README.md:
  ## Related Skills in Other Categories
  - **github-issue-creation** (coordination/): Standardized issue creation for team tracking
    → While categorized as coordination, this is workflow automation for issue creation

In .claude/skills/coordination/github-issue-creation/SKILL.md:
  **Note:** This skill could also be categorized as workflow automation. If you searched workflow/
  category and didn't find this, you're in the right place now!
```

**Option C: Skill Duplication (Last Resort)**
```yaml
Create: Symbolic link or reference in workflow/ pointing to coordination/github-issue-creation
Trade-off: Slight duplication, but enables discovery from multiple logical entry points
```

### Failure Mode 4: Skill Name Not Discoverable

**Symptoms:**
- Skill name uses unfamiliar terminology
- Agent searches using different vocabulary
- Name doesn't match agent's mental model of capability

**Example Failure:**
```yaml
Skill: working-directory-coordination
Agent Mental Model: "artifact reporting", "file sharing", "document exchange"

Discovery:
  Agent searches: "artifact reporting" → No skill name matches
  Agent searches: "file sharing" → Finds file-management-patterns (wrong domain)
  Agent misses: working-directory-coordination (name emphasizes "coordination" not "artifacts")

Root Cause: Skill name optimized for one terminology, agents use different vocabulary
```

**Resolution:**
```yaml
Enhanced Description to Include Vocabulary Variations:
  "Team communication protocols for **artifact discovery, file reporting, and document exchange**
   via working directory. Use when **creating artifacts, sharing analysis reports, or coordinating
   through files** in /working-dir/. Enables **seamless team communication through structured
   documentation**."

Change: Description includes multiple vocabulary variations (artifacts, files, documents, reports)
Result: Agent searching any of these terms matches description
```

**Alternative: Skill Naming Alias (If Supported):**
```yaml
---
name: working-directory-coordination
aliases:
  - artifact-reporting
  - working-directory-management
  - team-file-coordination
description: [Enhanced with vocabulary variations]
---
```

---

**Skill Discovery Mechanics Guide Status:** ✅ **COMPLETE**
**Token Estimate:** ~17,600 tokens (~2,200 lines × 8 tokens/line)
**Purpose:** Comprehensive guide to skill discovery patterns, categorization, agent-skill matching, and trigger identification
**Target Audience:** PromptEngineer designing skill metadata, Claude orchestrating, agents navigating ecosystem
**Integration:** Core resource for skill-creation meta-skill Phase 2 (Skill Structure Design) and scaling to 100+ skills
