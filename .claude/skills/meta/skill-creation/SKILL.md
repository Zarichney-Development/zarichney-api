---
name: skill-creation
description: Systematic framework for creating new skills with consistent structure, metadata, and progressive loading design, preventing skill bloat while ensuring quality and reusability. Use when PromptEngineer needs to create new skills, refactor embedded patterns into skills, or establish skill templates for cross-agent workflows. Enables 87% token reduction through skill extraction.
---

# Skill Creation Meta-Skill

**Version:** 1.0.0
**Category:** Meta (Tools for PromptEngineer)
**Target User:** PromptEngineer exclusively

---

## PURPOSE

This meta-skill provides PromptEngineer with a systematic framework for creating new skills following zarichney-api's progressive loading architecture, ensuring skills prevent bloat while maintaining quality, reusability, and effective context efficiency.

### Core Mission
Transform skill creation from ad-hoc documentation into a disciplined 5-phase design process that produces optimized, reusable skills achieving 50-87% token savings vs. embedded patterns in agent definitions.

### Why This Matters
Without systematic skill creation methodology:
- **Skill Bloat:** Unnecessary skills created for single-agent patterns, cluttering skill ecosystem
- **Inconsistent Structure:** Skills lack standard sections, making discovery and usage inefficient
- **Context Inefficiency:** Skills embed content better suited for progressive loading via resources
- **Integration Gaps:** Skills fail to reference core coordination patterns (working-directory-coordination, documentation-grounding)
- **Quality Variability:** No validation framework ensuring skills meet token budget and clarity standards

This meta-skill ensures every new skill integrates seamlessly into zarichney-api's progressive loading architecture while maintaining 2,000-5,000 token SKILL.md instructions optimized for context efficiency.

### Mandatory Application
- **Required For:** All new skill creation or comprehensive skill refactoring by PromptEngineer
- **Authority Scope:** PromptEngineer has EXCLUSIVE authority over `.claude/skills/` directory
- **Quality Gate:** No skill deployment without completion of all 5 phases
- **Efficiency Target:** Skills enable 87% token reduction per integration vs. embedded patterns

---

## WHEN TO USE

This meta-skill applies in these scenarios:

### 1. Creating New Cross-Cutting Coordination Skill (PRIMARY USE CASE)
**Trigger:** Pattern used by 3+ agents (e.g., working directory communication, documentation grounding, core issue focus)
**Action:** Execute complete 5-phase workflow from scope definition through agent integration pattern design
**Rationale:** Coordination skills reduce redundancy across agent team through shared reusable patterns

### 2. Extracting Domain Technical Skill from Agent Definition (OPTIMIZATION)
**Trigger:** Agent definition exceeds 240 lines or contains deep technical content >500 lines applicable to multiple specialists
**Action:** Execute Phases 1-4 to extract pattern into domain technical skill (e.g., API design patterns, test architecture)
**Rationale:** Reduces agent token load while enabling cross-domain knowledge sharing

### 3. Creating Meta-Skill for Agent/Skill/Command Creation (META-CAPABILITY)
**Trigger:** PromptEngineer needs systematic framework for creating AI system components
**Action:** Execute all 5 phases to establish reusable meta-capability with comprehensive resources
**Rationale:** Meta-skills accelerate AI system evolution through standardized creation methodologies

### 4. Designing Workflow Automation Skill (PRODUCTIVITY)
**Trigger:** Repeatable process with clear steps used by 2+ agents (e.g., GitHub issue creation, PR analysis, testing workflows)
**Action:** Execute Phases 1-4 focusing on actionable workflow steps and validation checklists
**Rationale:** Workflow skills automate repetitive tasks reducing agent cognitive load and errors

### 5. Preventing Skill Bloat - Validating Skill Need (ANTI-BLOAT)
**Trigger:** PromptEngineer considering new skill but uncertain if extraction justified
**Action:** Execute Phase 1 Skill Scope Definition to apply anti-bloat decision framework
**Rationale:** Ensures skills created only when reusability threshold met, preventing ecosystem clutter

---

## 5-PHASE WORKFLOW STEPS

### Phase 1: Skill Scope Definition

**Purpose:** Determine if skill creation is appropriate and define clear boundaries, preventing skill bloat

#### Process

**1. Cross-Cutting Concern Assessment**
Evaluate if pattern qualifies as coordination skill:
- **Reusability Threshold:** Is this pattern used by 3+ agents currently or anticipated within 6 months?
- **Coordination Value:** Does extracting this reduce agent token load by 100+ tokens per integration?
- **Standardization Need:** Does embedding cause inconsistencies across agent implementations?
- **Team Awareness:** Does this pattern enable multi-agent communication or shared protocols?

**Examples:**
- ✅ **working-directory-coordination:** Used by all 12 agents for artifact reporting (meets threshold)
- ✅ **documentation-grounding:** Used by all file-editing agents for standards loading (meets threshold)
- ❌ **single-agent-unique-workflow:** Used only by BugInvestigator for diagnostic approach (below threshold - keep in agent)

**2. Domain Specialization Evaluation**
Assess if pattern qualifies as technical skill:
- **Content Depth:** Is this deep technical content >500 lines for specific domain?
- **Specialist Relevance:** Does this enhance multiple specialists' effectiveness (Backend + Frontend, Security + Testing)?
- **Knowledge Preservation:** Does this capture critical architectural patterns or best practices?
- **Progressive Loading Value:** Can this content be loaded on-demand vs. always present in agent?

**Examples:**
- ✅ **api-design-patterns:** Deep REST/GraphQL guidance for BackendSpecialist and FrontendSpecialist
- ✅ **test-architecture-best-practices:** Comprehensive testing patterns for TestEngineer and specialists
- ❌ **simple-coding-convention:** 2-line pattern better suited for CodingStandards.md reference

**3. Meta-Skill Identification**
Determine if pattern qualifies as meta-capability:
- **Agent Creation:** Does this enable creating new agents, skills, or commands?
- **System Evolution:** Does this accelerate AI system development and standardization?
- **PromptEngineer Exclusive:** Is this primarily for PromptEngineer's optimization work?
- **Methodological Framework:** Does this establish reusable creation methodology?

**Examples:**
- ✅ **agent-creation:** Systematic framework for creating new agent definitions
- ✅ **skill-creation:** This meta-skill enabling consistent skill design
- ✅ **command-creation:** Future meta-skill for slash command development
- ❌ **code-review-optimization:** Operational skill, not meta-capability

**4. Workflow Automation Analysis**
Assess if pattern qualifies as workflow skill:
- **Repeatable Process:** Does this have clear step-by-step procedure agents follow?
- **Multiple Consumers:** Will 2+ agents execute this workflow?
- **Error Reduction:** Does automation prevent common mistakes or inconsistencies?
- **Validation Framework:** Can this include checklists ensuring completion quality?

**Examples:**
- ✅ **github-issue-creation:** Standardized issue creation for BugInvestigator, ArchitecturalAnalyst
- ✅ **pr-analysis-workflow:** Multi-step PR review for ComplianceOfficer, specialists
- ❌ **one-time-migration-script:** Single-use process, not reusable workflow

#### Anti-Bloat Decision Framework

**CREATE SKILL WHEN:**
- Pattern used by 3+ agents (coordination skill)
- Deep technical content >500 lines applicable to multiple specialists (technical skill)
- Meta-capability enabling agent/skill/command creation (meta-skill)
- Repeatable workflow with clear steps used by 2+ agents (workflow skill)

**DO NOT CREATE SKILL WHEN:**
- Single-agent unique pattern (preserve in agent definition - maintains agent identity)
- Simple 1-2 line reference (unnecessary extraction - use direct documentation link)
- Rapidly changing content (maintain in /Docs/Standards/ instead - easier updates)
- Agent-specific identity content (core to agent role - shouldn't be extracted)
- Content <100 tokens embedded (extraction overhead exceeds savings)

#### Skill Categorization

Once skill justified, categorize for appropriate structure:

**Coordination Skills:**
- Purpose: Cross-agent communication and team workflow patterns
- Examples: working-directory-coordination, documentation-grounding, core-issue-focus
- Target: 3+ agents, focus on standardization and team awareness
- Token Budget: 2,000-3,500 tokens SKILL.md

**Documentation Skills:**
- Purpose: Standards loading and contextual grounding patterns
- Examples: documentation-grounding, standards-compliance-validation
- Target: All file-editing agents, focus on comprehensive context
- Token Budget: 2,500-4,000 tokens SKILL.md

**Technical Skills:**
- Purpose: Deep domain expertise and architectural patterns
- Examples: api-design-patterns, test-architecture-best-practices, security-threat-modeling
- Target: Domain specialists, focus on technical depth
- Token Budget: 3,000-5,000 tokens SKILL.md + extensive resources

**Meta-Skills:**
- Purpose: Creating agents, skills, commands, or other AI system components
- Examples: agent-creation, skill-creation, command-creation
- Target: PromptEngineer exclusively, focus on systematic frameworks
- Token Budget: 3,500-5,000 tokens SKILL.md + comprehensive resources

**Workflow Skills:**
- Purpose: Repeatable processes with clear validation steps
- Examples: github-issue-creation, pr-analysis-workflow, testing-execution
- Target: 2+ agents with specific procedural needs
- Token Budget: 2,000-3,500 tokens SKILL.md

#### Scope Definition Checklist
- [ ] Reusability threshold validated (3+ agents for coordination, 2+ for workflow, specialist applicability for technical)
- [ ] Anti-bloat decision framework applied - skill creation justified
- [ ] Skill category identified (coordination/documentation/technical/meta/workflow)
- [ ] Token savings calculated vs. embedded approach (target 100+ tokens saved per agent integration)
- [ ] Integration with existing skills assessed (complementary, not redundant)
- [ ] Progressive loading design feasible (metadata + instructions + resources structure works)

**Resource:** See `resources/templates/skill-scope-definition-template.md` for structured assessment questionnaire

---

### Phase 2: Skill Structure Design

**Purpose:** Apply consistent SKILL.md structure with YAML frontmatter and required sections optimized for progressive loading

#### Process

**1. YAML Frontmatter Design**
Craft metadata enabling efficient skill discovery:

```yaml
---
name: skill-name-here
description: Brief description of what this skill does and when to use it. MUST include BOTH what the skill does AND when agents should use it. Keep concise but comprehensive. Max 1024 characters.
---
```

**Name Constraints (Official Specification):**
- Max 64 characters
- Lowercase letters, numbers, hyphens only (e.g., `working-directory-coordination`)
- No spaces, underscores, or special characters
- No reserved words (avoid: `help`, `list`, `all`, `none`)
- Descriptive and discoverable among 100+ skills

**Description Constraints (Official Specification):**
- Non-empty string
- Max 1024 characters
- **MUST include both "what" and "when"**: What the skill does + When agents should use it
- Optimized for discovery context where agents browse skills
- Include efficiency metrics if quantifiable (e.g., "Enables 87% token reduction")

**Examples:**
```yaml
# Coordination Skill Example
---
name: working-directory-coordination
description: Team communication protocols for artifact discovery, immediate reporting, and context integration across multi-agent workflows. Use when creating/discovering working directory files to maintain team awareness. Prevents communication gaps and enables seamless context flow between agent engagements.
---

# Technical Skill Example
---
name: api-design-patterns
description: Comprehensive REST and GraphQL API design patterns following zarichney-api's .NET 8 backend architecture. Use when BackendSpecialist or FrontendSpecialist designing new endpoints, optimizing existing APIs, or resolving contract integration issues. Ensures consistency and best practices across full stack.
---

# Meta-Skill Example
---
name: agent-creation
description: Standardized framework for creating new agent definitions following prompt engineering best practices. Use when PromptEngineer needs to create new agents, refactor existing agent definitions, or establish consistent agent structure patterns. Enables 50% faster agent creation with 100% consistency.
---
```

**2. Apply Required SKILL.md Sections**
All skills must include these core sections in optimal progressive loading order:

```markdown
# Skill Name Here

[2-3 line introduction paragraph]

## PURPOSE

### Core Mission
[1-2 sentences: primary responsibility]

### Why This Matters
[1-2 sentences: value and consequences of not using]

### Mandatory Application
[When required vs. optional, exceptions]

---

## WHEN TO USE

### 1. [Primary Use Case]
**Trigger:** [Event prompting usage]
**Action:** [What agent should do]
**Rationale:** [Why skill needed]

### 2-4. [Additional Use Cases]
[Same structure for each scenario]

---

## WORKFLOW STEPS (or 5-PHASE WORKFLOW for meta-skills)

### Step 1: [First Action Name]
**Purpose:** [What this step accomplishes]

#### Process
1. **[Sub-step 1]:** [Detailed instruction]
2. **[Sub-step 2]:** [Detailed instruction]
3. **[Sub-step 3]:** [Detailed instruction]

#### Checklist
- [ ] [Completion criterion 1]
- [ ] [Completion criterion 2]

**Resource:** See `resources/templates/[name].md` for template

---

## TARGET AGENTS

### Primary Users
- **[Agent1]**: [How/when used]
- **[Agent2]**: [How/when used]

### Secondary Users (if applicable)
- **[Agent3]**: [Optional usage]

---

## RESOURCES

### Templates (Ready-to-Use Formats)
- **[template-name].md**: [Purpose]

**Location:** `resources/templates/`
**Usage:** Copy template, fill specifics, use in agent work

### Examples (Reference Implementations)
- **[example-name].md**: [What demonstrated]

**Location:** `resources/examples/`
**Usage:** Review for realistic workflow demonstrations

### Documentation (Deep Dives)
- **[doc-name].md**: [Advanced topic]

**Location:** `resources/documentation/`
**Usage:** Deep understanding, troubleshooting

---

## INTEGRATION WITH TEAM WORKFLOWS

### Multi-Agent Coordination Patterns
[How agents work together using this skill]

### Claude's Orchestration Enhancement
[How this skill helps Claude coordinate better]

### Quality Gate Integration
[How skill supports ComplianceOfficer, AI Sentinels]

### CLAUDE.md Integration
[How Claude references this in orchestration]

---

## SUCCESS METRICS (Optional but recommended)

### [Metric Category 1]
- **[Specific Metric]**: [Target/expectation]

---

## TROUBLESHOOTING (Optional but recommended)

### Common Issues

#### Issue: [Problem Description]
**Symptoms:** [How to identify]
**Root Cause:** [Why happens]
**Solution:** [Step-by-step resolution]
**Prevention:** [How to avoid]

### Escalation Path
[When/how to escalate unresolved issues]

---

**Skill Status:** [DRAFT/READY/OPERATIONAL]
**Adoption:** [Which agents use this]
**Context Savings:** [Token savings vs. embedding]
**Progressive Loading:** YAML frontmatter (~[X] tokens) → SKILL.md (~[Y] tokens) → resources (on-demand)
```

**3. Optimize Section Ordering for Progressive Loading**
Critical content front-loaded for efficiency:

**Lines 1-80 (Always Loaded - ~600 tokens):**
- YAML frontmatter
- Purpose and Core Mission
- When to Use scenarios
- Target Agents overview

**Lines 81-300 (Loaded When Skill Invoked - ~1,800 tokens):**
- Workflow Steps with detailed processes
- Integration patterns
- Success metrics

**Lines 301-500 (Loaded On-Demand - ~1,600 tokens):**
- Resources overview (full resources loaded separately)
- Troubleshooting deep dives
- Advanced optimization techniques

**Total SKILL.md Target:** 2,000-5,000 tokens depending on skill category

**4. Token Budget Allocation**

| Skill Category | YAML Frontmatter | SKILL.md Body | Resources | Total Context Load |
|----------------|------------------|---------------|-----------|-------------------|
| Coordination   | ~100 tokens      | 2,000-3,500   | Variable  | 2,100-3,600+      |
| Documentation  | ~100 tokens      | 2,500-4,000   | Variable  | 2,600-4,100+      |
| Technical      | ~100 tokens      | 3,000-5,000   | Extensive | 3,100-5,000+      |
| Meta           | ~100 tokens      | 3,500-5,000   | Comprehensive | 3,600-5,000+ |
| Workflow       | ~100 tokens      | 2,000-3,500   | Variable  | 2,100-3,600+      |

**Progressive Loading Efficiency:**
- Discovery phase: Load YAML only (~100 tokens)
- Invocation phase: Load SKILL.md instructions (2,000-5,000 tokens)
- Resource phase: Load templates/examples/docs as needed (variable)

#### Structure Design Checklist
- [ ] YAML frontmatter valid (name and description meet official specification)
- [ ] Description includes both "what" and "when" elements
- [ ] All required sections present in optimal progressive loading order
- [ ] Section ordering front-loads critical content (lines 1-80)
- [ ] Token budget appropriate for skill category
- [ ] Progressive loading scenarios validated (discovery → invocation → resources)
- [ ] No deprecated metadata.json file (YAML frontmatter only)

**Resource:** See `resources/templates/skill-structure-template.md` for complete section scaffolding

---

### Phase 3: Progressive Loading Design

**Purpose:** Optimize context efficiency through metadata discovery, instruction loading, and on-demand resource access

#### Process

**1. Metadata Discovery Optimization**
Design YAML frontmatter for efficient skill browsing:

**Discovery Scenario:**
- **Context:** Agent (or Claude) browsing skills directory to find relevant capability
- **Loaded:** YAML frontmatter only from all skills in `.claude/skills/`
- **Token Load:** ~100 tokens per skill
- **Decision Point:** Does skill name and description match current task needs?

**Optimization Strategy:**
- **Name:** Descriptive enough to convey purpose without reading description
- **Description:** Includes clear triggers ("Use when...") for immediate relevance assessment
- **Efficiency:** Agent scans 10+ skills in ~1,000 tokens to find match

**Example Discovery Flow:**
```
Agent Task: "Need to coordinate artifact reporting with other agents"

Scans Skills:
- api-design-patterns: ~100 tokens [SKIP - not relevant to coordination]
- working-directory-coordination: ~100 tokens [MATCH - "artifact discovery, immediate reporting"]
  → Load this skill's SKILL.md for full instructions
```

**2. Instruction Loading Design**
Structure SKILL.md for efficient invocation:

**Invocation Scenario:**
- **Context:** Agent identified relevant skill and loads complete instructions
- **Loaded:** Full SKILL.md file content
- **Token Load:** 2,000-5,000 tokens depending on skill category
- **Decision Point:** Which workflow steps apply to current task? Which resources needed?

**Optimization Strategy:**
- **Front-Load Critical Content:** Purpose, When to Use, primary workflow steps in first 80 lines
- **Progressive Disclosure:** Detailed processes, checklists, troubleshooting in middle sections
- **Resource References:** Overview of available resources at end, actual resources loaded separately
- **Target:** Agent can execute basic workflow without loading any resources

**Example Invocation Flow:**
```
Agent Loads: working-directory-coordination SKILL.md (~2,500 tokens)

Reads:
- Purpose: "Team communication protocols..." [Confirms relevance]
- When to Use: "Pre-work discovery, immediate reporting, context integration" [Identifies applicable scenario]
- Workflow Steps: "1. Pre-Work Artifact Discovery..." [Executes process]
- Resources: "See templates/artifact-reporting-template.md for format" [Notes resource if needed]

Decision: Can execute basic workflow from SKILL.md alone, load template if unsure of format
```

**3. Resource Access Design**
Organize resources for on-demand loading:

**Resource Access Scenario:**
- **Context:** Agent executing skill workflow, needs template/example/documentation
- **Loaded:** Specific resource file from `resources/` subdirectory
- **Token Load:** Variable (templates ~200-500 tokens, examples ~500-1,500 tokens, docs ~1,000-3,000 tokens)
- **Decision Point:** Does this resource resolve current need?

**Resource Categories and Loading Patterns:**

**Templates (Immediate Application):**
- **Purpose:** Ready-to-use formats agents copy-paste
- **Token Load:** 200-500 tokens per template
- **Loading Trigger:** Agent needs exact format specification
- **Example:** `resources/templates/artifact-reporting-template.md` for working-directory-coordination

**Examples (Pattern Demonstration):**
- **Purpose:** Realistic scenarios showing complete workflow execution
- **Token Load:** 500-1,500 tokens per example
- **Loading Trigger:** Agent needs to understand workflow in realistic context
- **Example:** `resources/examples/backend-specialist-coordination.md` showing actual artifact reporting

**Documentation (Deep Understanding):**
- **Purpose:** Advanced concepts, troubleshooting, optimization techniques
- **Token Load:** 1,000-3,000 tokens per document
- **Loading Trigger:** Agent encounters complex scenario or needs philosophy understanding
- **Example:** `resources/documentation/progressive-loading-architecture.md` for skill design deep dive

**4. Total Context Efficiency Validation**

**Progressive Loading Flow:**
```
Phase 1 - Discovery (Minimal Context):
  Load: YAML frontmatter from all skills (~100 tokens each)
  Total: ~1,000 tokens for scanning 10 skills
  Decision: Identify relevant skill

Phase 2 - Invocation (Core Context):
  Load: Complete SKILL.md instructions (~2,500 tokens average)
  Total: ~2,500 tokens for skill execution guidance
  Decision: Execute workflow, identify resource needs

Phase 3 - Resource Access (Targeted Context):
  Load: Specific template/example/doc (~500 tokens average)
  Total: ~3,000 tokens for complete workflow with resources
  Decision: Apply resource to resolve specific need

Phase 4 - Deep Dive (Comprehensive Context):
  Load: Additional documentation (~2,000 tokens)
  Total: ~5,000 tokens for complex scenario resolution
  Decision: Resolve edge case or optimize approach
```

**Efficiency Comparison vs. Embedded Approach:**
- **Embedded in Agent:** ~2,500 tokens always loaded in agent definition
- **Skill Reference:** ~20 tokens in agent + ~2,500 tokens when invoked
- **Savings:** 2,480 tokens (99% reduction when skill not needed)
- **Agent Integration:** With 5 skills embedded vs. referenced = ~12,400 tokens saved per agent

**5. Token Measurement Techniques**

**Estimate Tokens from Lines:**
```
Average: 1 line ≈ 8 tokens (varies by density)

YAML Frontmatter:
  5-10 lines → ~50-100 tokens

SKILL.md Sections:
  Purpose (10 lines) → ~80 tokens
  When to Use (30 lines) → ~240 tokens
  Workflow Steps (150 lines) → ~1,200 tokens
  Resources (40 lines) → ~320 tokens
  Total: ~230 lines → ~1,840 tokens

Resources:
  Template (30 lines) → ~240 tokens
  Example (100 lines) → ~800 tokens
  Documentation (250 lines) → ~2,000 tokens
```

**Validation Method:**
1. Draft SKILL.md content
2. Count total lines
3. Estimate tokens: lines × 8
4. Validate within skill category budget (2,000-5,000 tokens)
5. If over budget, extract content to resources/

#### Progressive Loading Design Checklist
- [ ] YAML frontmatter optimized for discovery (<150 tokens, clear triggers)
- [ ] SKILL.md body within token budget (2,000-5,000 tokens based on category)
- [ ] Critical content front-loaded (purpose, when to use, basic workflow in first 80 lines)
- [ ] Resource references one level deep from SKILL.md (not nested)
- [ ] Templates actionable without reading documentation
- [ ] Examples demonstrate complete workflows realistically
- [ ] Documentation provides deep dives for complex scenarios
- [ ] Total context efficiency validated (discovery → invocation → resources flow tested)
- [ ] Token savings vs. embedded approach measured (target 87% reduction per agent integration)

**Resource:** See `resources/documentation/progressive-loading-architecture.md` for comprehensive design philosophy

---

### Phase 4: Resource Organization

**Purpose:** Structure skill resources for maximum reusability, clarity, and progressive loading efficiency

#### Process

**1. Resource Directory Structure**
Organize resources following standardized hierarchy:

```
.claude/skills/[category]/[skill-name]/
├── SKILL.md (YAML frontmatter + workflow steps, 2,000-5,000 tokens)
└── resources/
    ├── templates/ (reusable formats, 200-500 tokens each)
    │   ├── template-1.md
    │   ├── template-2.md
    │   └── template-3.md
    ├── examples/ (reference implementations, 500-1,500 tokens each)
    │   ├── example-1.md
    │   ├── example-2.md
    │   └── example-3.md
    └── documentation/ (deep guides, 1,000-3,000 tokens each)
        ├── deep-dive-1.md
        ├── deep-dive-2.md
        └── deep-dive-3.md
```

**2. Templates Directory Design**
Create ready-to-use formats for immediate application:

**Purpose:**
- Provide exact formats agents copy-paste with minimal modification
- Eliminate ambiguity in standardized outputs (artifact reports, issue formats, PR descriptions)
- Enable consistent team-wide patterns through shared templates

**Content Characteristics:**
- **Actionable:** Agent can copy entire template and fill placeholders
- **Concise:** 30-60 lines maximum (200-500 tokens)
- **Clear Placeholders:** Use consistent syntax: `{{PLACEHOLDER_NAME}}`
- **Standalone:** Usable without reading documentation

**Template Naming Convention:**
- Descriptive: `artifact-reporting-template.md`, `github-issue-template.md`
- Purpose-focused: `[use-case]-template.md`
- Avoid versioning in name unless multiple variants needed

**Example Template Structure:**
```markdown
# [Template Purpose]

**Use this template when:** [Specific scenario description]

## Template Format

[Exact format with {{PLACEHOLDERS}} for agent customization]

## Placeholder Guidance

**{{PLACEHOLDER_NAME}}:**
- Description: [What goes here]
- Format: [Expected format]
- Example: [Realistic example]

## Validation Checklist

- [ ] All placeholders replaced with specific content
- [ ] [Validation criterion 2]
- [ ] [Validation criterion 3]
```

**3. Examples Directory Design**
Demonstrate complete workflows in realistic scenarios:

**Purpose:**
- Show skill workflow steps executed in realistic agent context
- Provide pattern recognition through annotated implementations
- Demonstrate integration with other skills and team coordination

**Content Characteristics:**
- **Realistic:** Based on actual agent tasks from zarichney-api workflows
- **Complete:** Shows entire workflow from start to finish, not fragments
- **Annotated:** Includes commentary explaining decisions and rationale
- **Moderate Length:** 100-200 lines (500-1,500 tokens)

**Example Naming Convention:**
- Scenario-based: `backend-specialist-coordination-example.md`, `multi-agent-workflow-example.md`
- Agent-focused: `[agent-name]-[workflow]-example.md`
- Demonstrates specific use case: `[scenario]-example.md`

**Example Structure:**
```markdown
# [Example Title]

**Scenario:** [Realistic task description from zarichney-api]
**Agents Involved:** [Which agents participate]
**Skill Demonstration:** [Which workflow steps shown]

## Context

[Setup: What task agent received, what state project in]

## Workflow Execution

### Step 1: [Workflow Step Name]

[Agent action with annotations]

**Rationale:** [Why agent took this approach]

### Step 2: [Next Workflow Step]

[Continued execution with decision points highlighted]

## Outcome

[Result of workflow execution, success metrics]

## Key Takeaways

- [Pattern demonstrated]
- [Decision rationale highlighted]
- [Integration point shown]
```

**4. Documentation Directory Design**
Provide deep dives for complex concepts and troubleshooting:

**Purpose:**
- Explain skill philosophy and design rationale
- Provide comprehensive troubleshooting for complex scenarios
- Enable optimization techniques for advanced users

**Content Characteristics:**
- **Comprehensive:** Covers topic in depth with multiple sections
- **Structured:** Uses clear headings, table of contents for >100 lines
- **Conceptual:** Focuses on understanding, not just execution
- **Extended Length:** 250-400 lines (1,000-3,000 tokens)

**Documentation Naming Convention:**
- Topic-based: `progressive-loading-architecture.md`, `authority-framework-guide.md`
- Purpose-focused: `[topic-name]-guide.md`, `[concept]-deep-dive.md`
- Troubleshooting: `troubleshooting-[area].md`

**Documentation Structure:**
```markdown
# [Topic Title]

**Purpose:** [What this document explains]
**Target Audience:** [Which agents benefit from reading this]
**Prerequisites:** [What to understand before reading]

## Table of Contents (for docs >100 lines)

1. [Section 1]
2. [Section 2]
3. [Section 3]

---

## Section 1: [Concept Name]

### Overview
[High-level explanation]

### Deep Dive
[Detailed exploration with examples]

### Practical Application
[How agents use this understanding]

---

## Section 2: [Advanced Topic]

[Continue comprehensive coverage]

---

## Troubleshooting

[Common issues with detailed resolutions]

---

## References

- [Related skills]
- [Project documentation]
- [External resources]
```

**5. Resource Reference Patterns**
Link resources from SKILL.md efficiently:

**One-Level Deep Reference (Required):**
```markdown
**Resource:** See `resources/templates/artifact-reporting-template.md` for complete format
```

**Avoid Nested References:**
```markdown
❌ BAD: "See resources/templates/ directory for formats (each template lists examples in resources/examples/)"
✅ GOOD: "See resources/templates/artifact-reporting-template.md for format. See resources/examples/coordination-example.md for realistic demonstration."
```

**Resource Overview Section in SKILL.md:**
```markdown
## RESOURCES

### Templates (Ready-to-Use Formats)
- **artifact-reporting-template.md**: Standardized format for working directory artifact communication
- **context-integration-template.md**: Format for documenting integration with other agents' work

**Location:** `resources/templates/`
**Usage:** Copy template, fill in specific details, use verbatim in agent work

### Examples (Reference Implementations)
- **backend-specialist-coordination.md**: Complete workflow showing Backend→Frontend artifact coordination
- **multi-agent-workflow.md**: Complex scenario with 3+ agents using working directory

**Location:** `resources/examples/`
**Usage:** Review examples for realistic scenarios showing workflow steps in action

### Documentation (Deep Dives)
- **progressive-loading-architecture.md**: Comprehensive guide to context efficiency design
- **troubleshooting-coordination.md**: Advanced resolution for complex coordination scenarios

**Location:** `resources/documentation/`
**Usage:** Understand skill philosophy, troubleshoot issues, optimize effectiveness
```

#### Resource Organization Checklist
- [ ] Directory structure follows standard hierarchy (templates/examples/documentation)
- [ ] Templates actionable and standalone (200-500 tokens each)
- [ ] Examples realistic and complete (500-1,500 tokens each)
- [ ] Documentation comprehensive with table of contents for >100 lines (1,000-3,000 tokens)
- [ ] Resource references one level deep from SKILL.md
- [ ] Consistent naming conventions across resource types
- [ ] Resources overview section in SKILL.md provides clear navigation
- [ ] No nested resource references requiring multiple loading steps

**Resource:** See `resources/templates/resource-organization-template.md` for directory setup guide

---

### Phase 5: Agent Integration Pattern

**Purpose:** Define how agents reference and use this skill effectively, achieving maximum token efficiency

#### Process

**1. Skill Reference Format Design**
Standardize skill integration in agent definitions:

**Standard Skill Reference Template (Target ~20 tokens):**
```markdown
### [skill-name]
**Purpose:** [1-line capability description]
**Key Workflow:** [3-5 word workflow summary or primary steps]
**Integration:** [When/how agent uses this skill - 1 sentence]
```

**Example: Coordination Skill Reference**
```markdown
### working-directory-coordination
**Purpose:** Team communication protocols for artifact discovery and reporting
**Key Workflow:** Pre-work discovery → Immediate reporting → Context integration
**Integration:** Execute all 3 protocols for every working directory interaction
```

**Token Count:** ~22 tokens (vs. ~150 tokens for embedded pattern - 87% reduction)

**Example: Technical Skill Reference**
```markdown
### api-design-patterns
**Purpose:** REST and GraphQL design following .NET 8 backend architecture
**Key Workflow:** Contract design | Validation | Error handling | Documentation
**Integration:** Use when designing new endpoints or optimizing existing APIs
```

**Token Count:** ~24 tokens (vs. ~500 tokens for embedded deep technical content - 95% reduction)

**Example: Meta-Skill Reference**
```markdown
### agent-creation
**Purpose:** Systematic framework for creating new agent definitions
**Key Workflow:** Identity | Structure | Authority | Skills | Optimization
**Integration:** Execute complete 5-phase workflow when creating new agents
```

**Token Count:** ~21 tokens (vs. ~2,500 tokens for embedded methodology - 99% reduction)

**2. Integration Point Positioning**
Determine where in agent definition to reference skill:

**Mandatory Skills Section (All Agents):**
- Position: Lines 80-120 in agent definition (mid-section, loaded during activation)
- Purpose: Core coordination skills required for all agent operations
- Skills: working-directory-coordination, documentation-grounding
- Format: MANDATORY SKILLS heading with 2-3 skill references

**Domain-Specific Skills Section:**
- Position: Lines 120-160 in agent definition (mid-section, domain context)
- Purpose: Technical or workflow skills specific to agent's domain expertise
- Skills: api-design-patterns (Backend), component-patterns (Frontend), test-architecture (Testing)
- Format: DOMAIN EXPERTISE or TECHNICAL SKILLS heading with relevant skill references

**Optional Skills Section (Advanced Agents):**
- Position: Lines 160-200 in agent definition (late section, advanced capabilities)
- Purpose: Context-dependent secondary capabilities not always needed
- Skills: core-issue-focus (implementation agents), github-issue-creation (analysis agents)
- Format: ADVANCED CAPABILITIES or OPTIONAL SKILLS heading with selective skill references

**Meta-Skills Section (PromptEngineer Only):**
- Position: Lines 100-140 in PromptEngineer definition (core to role)
- Purpose: Agent/skill/command creation capabilities
- Skills: agent-creation, skill-creation, command-creation
- Format: META-SKILL CAPABILITIES heading with systematic framework references

**3. Usage Trigger Definition**
Specify when agent should load complete skill instructions:

**Always Load (Mandatory Skills):**
```markdown
### working-directory-coordination
**Purpose:** Team communication protocols for artifact discovery and reporting
**Key Workflow:** Pre-work discovery → Immediate reporting → Context integration
**Integration:** Execute all 3 protocols for every working directory interaction

**TRIGGER:** Before starting ANY task (pre-work discovery) and when creating/updating working directory files (immediate reporting)
```

**Conditional Load (Domain Skills):**
```markdown
### api-design-patterns
**Purpose:** REST and GraphQL design following .NET 8 backend architecture
**Key Workflow:** Contract design | Validation | Error handling | Documentation
**Integration:** Use when designing new endpoints or optimizing existing APIs

**TRIGGER:** When task involves API contract creation, endpoint design, or API architectural decisions
```

**Selective Load (Optional Skills):**
```markdown
### core-issue-focus
**Purpose:** Mission discipline ensuring core blocking issues resolved first
**Key Workflow:** Core issue identification → Scope boundary → Success criteria → Validation
**Integration:** Apply when implementation tasks risk scope creep or mission drift

**TRIGGER:** When task has clearly defined core technical issue requiring surgical focus
```

**4. Progressive Disclosure Pattern**
Structure agent definition to load skills progressively:

**Agent Definition Progressive Loading:**
```
Lines 1-50 (Identity Context - Always Loaded):
  - Agent role and authority
  - Primary file edit rights
  - Domain expertise overview
  → NO SKILL CONTENT HERE (preserve for core identity)

Lines 51-120 (Coordination Context - Loaded on Activation):
  - Mandatory skills references (~40 tokens for working-directory-coordination + documentation-grounding)
  - Core team integration patterns
  - Quality standards overview
  → SKILL REFERENCES ONLY (load full skill SKILL.md when needed)

Lines 121-200 (Domain Context - Loaded for Specialist Tasks):
  - Domain-specific skill references (~60 tokens for 3 technical skills)
  - Advanced coordination patterns
  - Completion report format
  → SKILL REFERENCES ONLY (load full skill SKILL.md when triggered)

Lines 201-240 (Advanced Context - Loaded on Demand):
  - Optional skill references (~20 tokens for 1 optional skill)
  - Constraints and escalation
  - Troubleshooting patterns
  → SKILL REFERENCES ONLY (load full skill SKILL.md when edge cases encountered)
```

**5. Token Efficiency Validation**
Measure and document savings:

**Agent Integration Token Calculation:**
```yaml
BEFORE_SKILL_EXTRACTION:
  Agent Definition Lines: 350
  Estimated Tokens: 350 × 8 = 2,800 tokens
  Embedded Patterns: working-directory (~150), documentation-grounding (~150), domain skills (~500)
  Total Agent Context: 2,800 tokens (always loaded)

AFTER_SKILL_EXTRACTION:
  Agent Definition Lines: 180
  Estimated Tokens: 180 × 8 = 1,440 tokens
  Skill References: working-directory (~20), documentation-grounding (~20), domain skills (~60)
  Total Agent Context: 1,440 tokens (base) + skills loaded on-demand

SAVINGS_PER_AGENT:
  Base Context: 1,360 tokens saved (49% reduction)
  On-Demand Loading: Skills loaded only when needed (additional savings)
  Total Efficiency: 50-70% reduction in agent context load
```

**Multi-Agent Ecosystem Efficiency:**
```yaml
12_AGENT_TEAM_SCENARIO:
  Agents: 12 agent definitions
  Embedded Approach: 12 × 2,800 = 33,600 tokens (all agents loaded with embedded patterns)
  Skill Reference Approach: 12 × 1,440 = 17,280 tokens (agents loaded) + skills on-demand
  Ecosystem Savings: 16,320 tokens (49% reduction across team)

  Context Window Impact:
    - Embedded: Can load ~5 agents simultaneously in 200k context window
    - Skill References: Can load ~11 agents simultaneously with same window
    - Orchestration Efficiency: 120% increase in simultaneous agent capacity
```

**6. Integration Testing**
Validate skill reference effectiveness:

**Test Scenarios:**
1. **Discovery Test:** Agent scans skill references, identifies relevant skill for current task
2. **Loading Test:** Agent invokes skill, loads SKILL.md instructions successfully
3. **Resource Test:** Agent executes workflow, loads template/example when needed
4. **Completion Test:** Agent completes task using skill without requiring embedded content

**Validation Checklist:**
- [ ] Agent can identify relevant skill from 2-3 line reference
- [ ] Skill reference provides enough context to know when to load full skill
- [ ] SKILL.md instructions sufficient for workflow execution without resources
- [ ] Resources loaded only when agent needs template/example/documentation
- [ ] No content duplication between agent definition and skill (references only)
- [ ] Token efficiency measured: Agent integration achieves 50-87% reduction vs. embedded
- [ ] Multiple agents can reference same skill (reusability validated)

#### Agent Integration Pattern Checklist
- [ ] Skill reference format standardized (~20 tokens per reference)
- [ ] Integration point positioning appropriate (mandatory vs. domain vs. optional)
- [ ] Usage triggers clearly defined (when agent loads full skill)
- [ ] Progressive disclosure pattern applied (references only in agent, content in skill)
- [ ] Token efficiency calculated and validated (target 50-87% reduction)
- [ ] Integration testing completed with 2+ target agents
- [ ] Reusability confirmed (same skill referenced by multiple agents)
- [ ] No embedded skill content in agent definitions (references only)

**Resource:** See `resources/examples/skill-integration-examples.md` for agent-specific demonstrations

---

## TARGET AGENTS

### Primary User: PromptEngineer
**Authority:** EXCLUSIVE modification rights over `.claude/skills/` directory
**Use Cases:**
- Creating new cross-cutting coordination skills (working-directory-coordination, core-issue-focus)
- Extracting domain technical skills from bloated agent definitions (api-design, test-architecture)
- Creating meta-skills for agent/skill/command creation workflows
- Designing workflow automation skills for repeatable processes
- Validating skill need through anti-bloat decision framework

**Integration with PromptEngineer Workflow:**
1. User requests new skill or PromptEngineer identifies pattern extraction opportunity
2. PromptEngineer executes 5-phase skill-creation workflow
3. New skill created at `.claude/skills/[category]/[skill-name]/SKILL.md`
4. PromptEngineer creates resources (templates/examples/documentation) as needed
5. PromptEngineer updates agent definitions to reference new skill (replacing embedded patterns)
6. Agents validate new skill effectiveness through real task execution
7. Token efficiency measured and documented in skill metadata

### Secondary User: Codebase Manager (Claude)
**Use Cases:**
- Understanding skill creation methodology for better orchestration
- Validating skills follow progressive loading architecture when delegating to PromptEngineer
- Assessing skill reusability threshold before approving new skill creation
- Preventing skill bloat through anti-bloat decision framework awareness

**Integration with Claude Orchestration:**
- Claude references this meta-skill when delegating skill creation to PromptEngineer
- Claude validates new skills follow team integration patterns documented here
- Claude adapts context packages based on progressive loading scenarios defined here
- Claude enforces anti-bloat framework preventing unnecessary skill proliferation

---

## RESOURCES

This meta-skill includes comprehensive resources for effective skill creation:

### Templates (Ready-to-Use Formats)

**skill-scope-definition-template.md** - Structured assessment questionnaire for Phase 1
- Reusability threshold checklist (3+ agents, 100+ token savings)
- Anti-bloat decision framework application
- Skill categorization decision tree
- Token savings calculation worksheet

**skill-structure-template.md** - Complete SKILL.md scaffolding for Phase 2
- YAML frontmatter format with examples
- Required section structure
- Progressive loading section ordering
- Token budget allocation guidelines

**resource-organization-template.md** - Directory setup guide for Phase 4
- Standard directory structure
- Template format specifications
- Example structure patterns
- Documentation outline templates

**Location:** `resources/templates/`
**Usage:** Copy template matching skill creation phase, customize for specific skill

### Examples (Reference Implementations)

**coordination-skill-creation.md** - Complete 5-phase workflow for working-directory-coordination skill
- Shows scope definition for 12-agent reusability
- Demonstrates YAML frontmatter design for coordination category
- Illustrates progressive loading with 3-step workflow
- Validates 87% token savings per agent integration

**technical-skill-creation.md** - Complete 5-phase workflow for api-design-patterns skill
- Shows scope definition for specialist domain expertise
- Demonstrates extensive resources organization (templates/examples/docs)
- Illustrates 3,000-5,000 token SKILL.md design
- Validates progressive loading with on-demand deep documentation

**meta-skill-creation.md** - Self-referential example of skill-creation meta-skill development
- Shows meta-skill categorization and scope justification
- Demonstrates 5-phase workflow structure for meta-capabilities
- Illustrates comprehensive resources for PromptEngineer
- Validates 99% token reduction vs. embedded methodology in agent

**Location:** `resources/examples/`
**Usage:** Review for realistic demonstrations of complete skill creation workflows

### Documentation (Deep Dives)

**progressive-loading-architecture.md** - Comprehensive guide to context efficiency design
- Discovery → Invocation → Resources loading flow deep dive
- Token measurement methodologies and validation techniques
- Content extraction decision frameworks (skill vs. documentation vs. agent)
- Multi-agent ecosystem efficiency optimization strategies

**anti-bloat-framework.md** - Detailed guide to preventing unnecessary skill creation
- Reusability threshold analysis with quantitative metrics
- Single-agent pattern preservation rationale
- Rapidly changing content management (skills vs. documentation)
- Skill ecosystem health monitoring and maintenance

**skill-categorization-guide.md** - Comprehensive categorization decision framework
- Coordination skills: Team workflow patterns (2,000-3,500 tokens)
- Documentation skills: Standards loading patterns (2,500-4,000 tokens)
- Technical skills: Domain expertise deep dives (3,000-5,000 tokens)
- Meta-skills: Agent/skill/command creation frameworks (3,500-5,000 tokens)
- Workflow skills: Repeatable automation processes (2,000-3,500 tokens)

**agent-integration-patterns.md** - Effective skill reference and usage patterns
- Skill reference format optimization (target ~20 tokens)
- Integration point positioning (mandatory vs. domain vs. optional)
- Progressive disclosure design in agent definitions
- Token efficiency calculation and validation methodologies

**Location:** `resources/documentation/`
**Usage:** Deep understanding of design principles, troubleshooting complex scenarios

---

## INTEGRATION WITH TEAM WORKFLOWS

### Multi-Agent Coordination Enhancement
This meta-skill enables:
- **Reduced Redundancy:** Coordination patterns extracted to skills eliminate duplication across 12-agent team
- **Consistent Interfaces:** All skills follow same structure making discovery and usage efficient
- **Progressive Loading Efficiency:** Context window optimization enables more agents loaded simultaneously
- **Reusability Standards:** Anti-bloat framework ensures only valuable patterns extracted to skills

### Claude's Orchestration Enhancement
Skill-creation meta-skill helps Claude (Codebase Manager) to:
- Delegate skill creation systematically to PromptEngineer with clear quality gates
- Validate new skills integrate with progressive loading architecture before deployment
- Prevent skill bloat through reusability threshold enforcement
- Understand skill usage patterns when crafting context packages for agent engagements
- Optimize context windows through skill-based agent definitions (50-70% token reduction)

### Quality Gate Integration
This meta-skill integrates with:
- **PromptEngineer Validation:** Self-validation of skill creation following 5-phase methodology
- **ComplianceOfficer:** Pre-deployment validation ensuring skill follows team standards
- **Real-World Testing:** New skills validated through actual agent task execution
- **Token Efficiency Audits:** Regular measurement of skill token savings vs. embedded approach

### CLAUDE.md Integration
This meta-skill directly supports CLAUDE.md Section 2: Multi-Agent Development Team by:
- Establishing skill creation methodology ensuring consistency across skill ecosystem
- Defining progressive loading architecture enabling context efficiency
- Documenting anti-bloat framework preventing unnecessary skill proliferation
- Standardizing agent integration patterns for team-wide reusability
- Supporting flexible authority framework through technical skill extraction

---

## SUCCESS METRICS

### Skill Creation Efficiency
- **5-Phase Systematic Process:** Structured workflow vs. unguided ad-hoc skill documentation
- **100% Structural Consistency:** All skills follow same YAML frontmatter and SKILL.md structure
- **Anti-Bloat Validation:** Reusability threshold enforced preventing unnecessary skill creation
- **First-Time Deployment Success:** Skills pass validation without rework

### Context Optimization Effectiveness
- **2,000-5,000 Token SKILL.md:** Within budget for skill category
- **Progressive Loading Validated:** Discovery → Invocation → Resources scenarios tested
- **87% Token Savings Per Integration:** Skill reference (~20 tokens) vs. embedded pattern (~150 tokens)
- **50-70% Agent Definition Reduction:** Agent context load reduced through skill extraction

### Ecosystem Health Metrics
- **Reusability Threshold Met:** Skills used by 3+ agents (coordination) or 2+ agents (workflow/technical)
- **No Skill Bloat:** Zero skills created for single-agent unique patterns
- **Progressive Loading Compliance:** All skills follow metadata → instructions → resources architecture
- **Integration Testing Passed:** Skills validated with 2+ target agents before deployment

### Multi-Agent Team Efficiency
- **16,320 Token Ecosystem Savings:** 49% reduction across 12-agent team vs. embedded approach
- **120% Orchestration Capacity Increase:** Can load 11 agents simultaneously vs. 5 with embedded patterns
- **Seamless Skill Adoption:** Agents integrate new skills without coordination conflicts
- **Consistent Resource Patterns:** Templates/examples/documentation follow standard organization

---

## TROUBLESHOOTING

### Common Issues

#### Issue: Skill Created for Single-Agent Pattern (Bloat Violation)
**Symptoms:** Skill referenced by only 1 agent, no anticipated expansion to other agents
**Root Cause:** Phase 1 anti-bloat decision framework insufficiently applied, reusability threshold not validated
**Solution:**
1. Re-evaluate skill scope: Is this truly cross-cutting or agent-specific identity?
2. If agent-specific: Delete skill, move content back to agent definition as unique pattern
3. If potentially reusable: Document which other agents could use this within 6 months
4. If unclear: Escalate to Claude for business requirement clarification
**Prevention:** Phase 1 checklist must validate 3+ agent consumers before proceeding to Phase 2

#### Issue: SKILL.md Exceeds 5,000 Token Budget
**Symptoms:** Line count >625 lines, estimated tokens >5,000, skill feels bloated
**Root Cause:** Phase 3 progressive loading design incomplete, content not extracted to resources
**Solution:**
1. Re-execute Phase 4 Resource Organization with aggressive extraction
2. Move detailed templates to resources/templates/ (reference from SKILL.md)
3. Move realistic examples to resources/examples/ (reference from SKILL.md)
4. Move deep conceptual content to resources/documentation/ (reference from SKILL.md)
5. Target: SKILL.md provides workflow guidance, resources provide detailed content
**Prevention:** Phase 3 token measurement must validate budget before resource creation begins

#### Issue: YAML Frontmatter Description Missing "When to Use" Element
**Symptoms:** Description explains what skill does but agents uncertain when to load it
**Root Cause:** Phase 2 YAML frontmatter design focused on "what" without "when"
**Solution:**
1. Review Phase 2 "When to Use" scenarios from SKILL.md body
2. Extract 1-2 key trigger phrases
3. Update description to include: "[What skill does]. Use when [trigger scenarios]."
4. Validate description <1024 characters after enhancement
**Prevention:** Phase 2 YAML frontmatter checklist must validate both "what" and "when" present

#### Issue: Agents Load Skill But Don't Use Resources Effectively
**Symptoms:** Agents execute workflow without loading templates/examples, inconsistent outputs
**Root Cause:** Phase 4 resource organization lacks clear triggers for when to load resources
**Solution:**
1. Review SKILL.md workflow steps - add explicit resource references
2. Add "Resource:" callouts after each workflow step indicating applicable template/example
3. Update resources overview section with clear usage triggers
4. Test with agent: Does workflow step clearly indicate when to load which resource?
**Prevention:** Phase 4 resource organization must include usage triggers in SKILL.md workflow steps

#### Issue: Multiple Agents Reference Skill with Inconsistent Integration Patterns
**Symptoms:** Some agents embed partial skill content, others reference correctly, inconsistent token efficiency
**Root Cause:** Phase 5 agent integration pattern not standardized across agent definitions
**Solution:**
1. Audit all agent definitions referencing this skill
2. Standardize to ~20 token reference format: Purpose | Key Workflow | Integration
3. Remove any embedded skill content from agent definitions
4. Update agents to reference skill SKILL.md for full instructions
5. Validate token efficiency: Each agent achieves 50-87% reduction vs. previous embedded approach
**Prevention:** Phase 5 integration testing must validate consistency across all target agents before deployment

### Escalation Path
When skill creation issues cannot be resolved through troubleshooting:
1. **PromptEngineer Review:** Re-execute problematic phase with comprehensive resource review
2. **Claude Orchestration:** Validate skill need through business requirement assessment
3. **Anti-Bloat Re-Evaluation:** Apply Phase 1 decision framework - is skill truly justified?
4. **User Escalation:** Fundamental design issues requiring clarification or architectural guidance

---

**Skill Status:** ✅ **OPERATIONAL**
**Target User:** PromptEngineer exclusively
**Efficiency Gains:** 87% token reduction per skill integration vs. embedded patterns
**Progressive Loading:** YAML frontmatter (~100 tokens) → SKILL.md (~3,600 tokens) → resources (on-demand)
