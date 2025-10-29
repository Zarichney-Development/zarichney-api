# Skill Resource Organization Guide

**Version:** 1.0
**Purpose:** Systematic framework for organizing skill resources to maximize reusability, progressive loading efficiency, and agent effectiveness
**Target Audience:** PromptEngineer during Phase 4 (Resource Organization) of skill-creation workflow

---

## OVERVIEW

This guide explains how to organize resources within a skill's `resources/` directory for maximum effectiveness. Proper resource organization enables progressive loading, prevents skill bloat, and ensures agents can access exactly what they need when they need it.

### Why Resource Organization Matters

**Without systematic resource organization:**
- Skills bloat beyond 5,000 token budget with embedded content
- Agents load unnecessary context during skill invocation
- Templates and examples difficult to discover
- Documentation duplicates SKILL.md content inefficiently

**With disciplined resource organization:**
- SKILL.md stays within 2,000-5,000 token budget
- Agents load resources on-demand (progressive loading efficiency)
- Clear resource categories (templates/examples/documentation) with distinct purposes
- Context efficiency: 50-87% token reduction vs. embedded approach

### Core Principles

1. **Progressive Loading:** SKILL.md provides workflow guidance, resources provide detailed content
2. **Single Purpose per Resource:** Each file addresses one specific need clearly
3. **One-Level Deep References:** SKILL.md references resources directly, no nested loading
4. **Token Efficiency:** Resources sized appropriately (templates <500 tokens, examples <1,500, docs <3,000)
5. **Discoverability:** Clear naming conventions and resource overview section

---

## DIRECTORY STRUCTURE PATTERN

### Standard Skill Resource Hierarchy

```
.claude/skills/[category]/[skill-name]/
├── SKILL.md (YAML frontmatter + workflow, 2,000-5,000 tokens)
└── resources/
    ├── templates/ (reusable formats, 200-500 tokens each)
    │   ├── [purpose]-template.md
    │   ├── [use-case]-template.md
    │   └── [scenario]-checklist-template.md
    ├── examples/ (reference implementations, 500-1,500 tokens each)
    │   ├── [scenario]-example.md
    │   ├── [agent-name]-workflow-example.md
    │   └── [multi-agent]-coordination-example.md
    └── documentation/ (deep dive guides, 1,000-3,000 tokens each)
        ├── [topic]-guide.md
        ├── [concept]-deep-dive.md
        └── troubleshooting-[area].md
```

### Category Breakdown

**Skill Categories and Resource Allocation:**

| Category | Templates | Examples | Documentation | Total Resources |
|----------|-----------|----------|---------------|-----------------|
| Coordination | 2-4 | 2-3 | 1-2 | 5-9 files |
| Documentation | 1-3 | 2-4 | 2-3 | 5-10 files |
| Technical | 3-6 | 3-5 | 3-5 | 9-16 files |
| Meta | 4-8 | 3-6 | 4-6 | 11-20 files |
| Workflow | 2-5 | 2-4 | 1-3 | 5-12 files |

**Guideline:** Technical and Meta skills require more extensive resources due to domain depth; Coordination and Workflow skills focus on concise templates and examples.

---

## RESOURCE CATEGORIES

### Templates Directory (`resources/templates/`)

**Purpose:** Reusable formats, scaffolding, and standardized structures agents can copy-paste with minimal customization.

#### When to Create Templates

Create templates when:
- **Standardized Format Needed:** Agent outputs should follow consistent structure (artifact reports, issue formats, PR descriptions)
- **Copy-Paste Efficiency:** Agent can copy entire template and fill placeholders in <5 minutes
- **Ambiguity Elimination:** Exact format specification prevents inconsistent agent interpretations
- **Team Consistency:** Multiple agents need identical output format for coordination

**Do NOT create templates when:**
- Content requires extensive customization (use examples with annotations instead)
- Format changes frequently (maintain in /Docs/Standards/ for easier updates)
- Only one possible usage scenario (embed directly in SKILL.md workflow step)

#### Template Naming Convention

**Format:** `[purpose]-template.md` or `[use-case]-template.md`

**Examples:**
- `artifact-reporting-template.md` - Working directory artifact communication format
- `github-issue-template.md` - Standardized issue creation structure
- `pr-description-template.md` - Pull request description format
- `validation-checklist-template.md` - Quality validation framework
- `context-package-template.md` - Agent context package structure

**Naming Best Practices:**
- Use descriptive purpose-focused names
- Avoid versioning in filename (use internal version header if needed)
- Keep names concise (2-4 words max)
- Use consistent terminology across skill ecosystem

#### Template Structure Pattern

```markdown
# [Template Name] Template

**Purpose:** [What this template provides - 1 sentence]
**Use When:** [Specific scenario triggering template usage - 1 sentence]

---

## Template Format

[EXACT FORMAT WITH {{PLACEHOLDERS}} FOR AGENT CUSTOMIZATION]

{{PLACEHOLDER_1}}: [Brief description]
{{PLACEHOLDER_2}}: [Brief description]
{{PLACEHOLDER_3}}: [Brief description]

[TEMPLATE CONTENT WITH CLEAR STRUCTURE]

---

## Placeholder Guidance

### {{PLACEHOLDER_1}}
- **Description:** [What goes here - detailed explanation]
- **Format:** [Expected format - constraints/requirements]
- **Example:** [Realistic example from zarichney-api context]
- **Validation:** [How to verify this placeholder filled correctly]

### {{PLACEHOLDER_2}}
- **Description:** [What goes here - detailed explanation]
- **Format:** [Expected format - constraints/requirements]
- **Example:** [Realistic example from zarichney-api context]
- **Validation:** [How to verify this placeholder filled correctly]

### {{PLACEHOLDER_3}}
- **Description:** [What goes here - detailed explanation]
- **Format:** [Expected format - constraints/requirements]
- **Example:** [Realistic example from zarichney-api context]
- **Validation:** [How to verify this placeholder filled correctly]

---

## Validation Checklist

Before finalizing output from this template:
- [ ] All {{PLACEHOLDERS}} replaced with specific content
- [ ] [Domain-specific validation criterion]
- [ ] [Quality standard verification]
- [ ] [Integration requirement check]

---

## Example Usage

[COMPLETE EXAMPLE SHOWING TEMPLATE FILLED IN WITH REALISTIC VALUES]

**Context:** [Scenario where this template used]
**Result:** [What agent produced using this template]

---

**Template Version:** 1.0
**Last Updated:** [Date]
**Related Resources:** See `resources/examples/[example-name].md` for realistic demonstration
```

#### Template Size Guidelines

**Target:** 30-60 lines (200-500 tokens)

**Structure Allocation:**
- Purpose and Use When: 5-10 lines
- Template Format: 10-20 lines (core content)
- Placeholder Guidance: 10-25 lines (3-5 placeholders typical)
- Validation Checklist: 5-8 lines
- Example Usage: 5-10 lines

**If template exceeds 60 lines:**
- Extract complex examples to `resources/examples/`
- Move detailed guidance to `resources/documentation/`
- Keep template focused on format specification only

#### Template Quality Checklist

Before deploying template:
- [ ] Clear purpose statement (1 sentence)
- [ ] Specific usage trigger (when to use this template)
- [ ] Consistent placeholder syntax ({{PLACEHOLDER_NAME}} format)
- [ ] Placeholder guidance for each customization point
- [ ] Validation checklist ensuring template completion
- [ ] Example usage showing filled-in template
- [ ] Token count <500 (approximately <60 lines)
- [ ] Standalone usability (agent can use without reading documentation)

---

### Examples Directory (`resources/examples/`)

**Purpose:** Reference implementations demonstrating skill usage patterns in realistic scenarios with annotations explaining decisions.

#### When to Create Examples

Create examples when:
- **Complete Workflow Demonstration:** Show skill workflow steps executed from start to finish in realistic context
- **Pattern Recognition:** Help agents recognize correct patterns through annotated implementations
- **Decision Points Highlighted:** Illustrate key decisions agents make during workflow execution
- **Multi-Agent Integration:** Demonstrate coordination between multiple agents using skill

**Do NOT create examples when:**
- Single workflow step sufficient (embed in SKILL.md instead)
- Example identical to template (redundant - template already shows format)
- Scenario too simple (2-3 line example can go in SKILL.md)

#### Example Naming Convention

**Format:** `[scenario]-example.md` or `[agent-name]-[workflow]-example.md`

**Examples:**
- `backend-specialist-coordination-example.md` - Backend→Frontend artifact coordination
- `multi-agent-workflow-example.md` - 3+ agents collaborating using skill
- `github-issue-creation-example.md` - Complete issue creation workflow
- `complex-refactoring-workflow.md` - Advanced usage scenario
- `testing-integration-example.md` - TestEngineer workflow demonstration

**Naming Best Practices:**
- Scenario-based for single-agent examples
- Agent-focused for role-specific workflows
- Multi-agent prefix for coordination examples
- Descriptive of workflow complexity (simple, complex, advanced)

#### Example Structure Pattern

```markdown
# [Example Title]

**Scenario:** [Realistic task description from zarichney-api context - 2-3 sentences]
**Agents Involved:** [Which agents participate - list all agents and roles]
**Skill Demonstration:** [Which workflow steps this example covers]
**Learning Objectives:** [What patterns agent learns from this example]

---

## CONTEXT

### Task Overview
[What task agent received - realistic GitHub issue or user request from zarichney-api]

### Project State
[Relevant codebase state - what branch, what files exist, what prior work completed]

### Skill Application
[Why agent chose to use this skill for this task - trigger recognition]

---

## WORKFLOW EXECUTION

### Step 1: [Workflow Step Name]

**Agent Action:**
[Detailed description of what agent did in this step]

```[language if code/format]
[ACTUAL CONTENT AGENT PRODUCED]
```

**Decision Point:** [Key decision agent made]
- **Context:** [Information agent considered]
- **Options:** [Alternatives agent evaluated]
- **Choice:** [What agent selected and why]

**Rationale:** [Why agent took this approach - pattern explanation]

**Annotation:** [Commentary explaining best practices demonstrated]

---

### Step 2: [Next Workflow Step]

**Agent Action:**
[Detailed description of what agent did in this step]

```[language if code/format]
[ACTUAL CONTENT AGENT PRODUCED]
```

**Integration Point:** [How this step coordinates with other agents/systems]
- **Handoff:** [What artifact passed to next agent]
- **Communication:** [How coordination communicated]
- **Validation:** [How agent verified successful handoff]

**Rationale:** [Why agent took this approach - pattern explanation]

**Annotation:** [Commentary explaining best practices demonstrated]

---

### Step 3: [Subsequent Workflow Step]

[Continue pattern for all workflow steps in complete execution]

---

## OUTCOME

### Deliverables Produced
[What agent created through skill execution - specific artifacts/files/outputs]

### Success Validation
[How agent verified successful skill completion - quality gates passed]

### Efficiency Gains
[Quantifiable improvements - time saved, quality enhanced, coordination improved]

---

## KEY TAKEAWAYS

### Pattern 1: [Pattern Name]
**What:** [Pattern demonstrated]
**Why:** [Rationale for this approach]
**When:** [Scenarios where this pattern applies]

### Pattern 2: [Pattern Name]
**What:** [Pattern demonstrated]
**Why:** [Rationale for this approach]
**When:** [Scenarios where this pattern applies]

### Pattern 3: [Pattern Name]
**What:** [Pattern demonstrated]
**Why:** [Rationale for this approach]
**When:** [Scenarios where this pattern applies]

---

## ALTERNATIVE APPROACHES

### Approach Not Taken: [Alternative Method]
**Description:** [What agent could have done differently]
**Trade-offs:** [Why chosen approach superior in this scenario]
**When Alternative Better:** [Scenarios where alternative approach preferred]

---

## ANNOTATIONS

### Design Decisions

**Decision:** [Key design choice made during workflow]
**Rationale:** [Why this decision optimal for zarichney-api context]
**Generalization:** [How this applies to similar scenarios]

**Decision:** [Another key design choice]
**Rationale:** [Why this decision optimal]
**Generalization:** [How this applies broadly]

### Learning Reinforcement

**Best Practice:** [Skill usage best practice demonstrated]
**Anti-Pattern Avoided:** [What agent avoided doing - common mistake]
**Quality Standard:** [How example meets zarichney-api standards]

---

**Example Status:** [VALIDATED/REFERENCE/DEPRECATED]
**Source:** [Real task from zarichney-api or realistic composite]
**Related Templates:** See `resources/templates/[template-name].md`
**Related Documentation:** See `resources/documentation/[doc-name].md`
```

#### Example Size Guidelines

**Target:** 100-200 lines (500-1,500 tokens)

**Structure Allocation:**
- Context and Setup: 15-25 lines
- Workflow Execution (3-5 steps): 50-100 lines (bulk of content)
- Outcome and Validation: 10-15 lines
- Key Takeaways: 15-25 lines
- Annotations: 10-20 lines

**If example exceeds 200 lines:**
- Split into multiple focused examples (simple vs. complex scenarios)
- Extract detailed explanations to `resources/documentation/`
- Focus example on workflow execution, move philosophy to docs

#### Example Quality Checklist

Before deploying example:
- [ ] Realistic scenario from zarichney-api context
- [ ] Complete workflow demonstrated (start to finish)
- [ ] Annotations explaining key decisions
- [ ] Decision points highlighted with rationale
- [ ] Key takeaways summarized clearly
- [ ] Alternative approaches considered
- [ ] Token count <1,500 (approximately <200 lines)
- [ ] Learning objectives achieved

---

### Documentation Directory (`resources/documentation/`)

**Purpose:** Deep dive guides for complex concepts requiring comprehensive explanation, philosophy background, or extensive troubleshooting.

#### When to Create Documentation

Create documentation when:
- **Complex Concept Requires >500 Lines:** Topic too deep for SKILL.md body
- **Multi-Faceted Topic:** Concept has 3+ distinct aspects requiring systematic coverage
- **Philosophy/Rationale:** Design principles behind skill need explanation
- **Advanced Troubleshooting:** Edge cases and complex issue resolution
- **Optimization Techniques:** Expert-level skill usage patterns

**Do NOT create documentation when:**
- Content fits in SKILL.md workflow steps (<200 lines)
- Simple concept explainable in 1-2 paragraphs (keep in SKILL.md)
- Content duplicates existing /Docs/Standards/ (reference instead)
- Rapidly changing content (maintain in /Docs/Standards/ for easier updates)

#### Documentation Naming Convention

**Format:** `[topic]-guide.md` or `[concept]-deep-dive.md` or `troubleshooting-[area].md`

**Examples:**
- `progressive-loading-architecture.md` - Context efficiency design philosophy
- `anti-bloat-framework.md` - Preventing unnecessary skill creation
- `skill-categorization-guide.md` - Decision framework for categorizing skills
- `agent-integration-patterns.md` - Effective skill reference optimization
- `troubleshooting-coordination.md` - Advanced coordination issue resolution

**Naming Best Practices:**
- Topic-based for conceptual guides
- Troubleshooting prefix for issue resolution docs
- Deep-dive suffix for comprehensive explorations
- Guide suffix for systematic frameworks

#### Documentation Structure Pattern

```markdown
# [Topic Title]

**Purpose:** [What this document explains - 1-2 sentences]
**Target Audience:** [Which agents benefit from reading this - specific roles]
**Prerequisites:** [What agent should understand before reading - foundational knowledge]
**Estimated Reading Time:** [X] minutes for [Y] lines

---

## TABLE OF CONTENTS

1. [Section 1: Overview](#section-1-overview)
2. [Section 2: Core Concepts](#section-2-core-concepts)
3. [Section 3: Implementation Strategies](#section-3-implementation-strategies)
4. [Section 4: Advanced Techniques](#section-4-advanced-techniques)
5. [Section 5: Common Pitfalls](#section-5-common-pitfalls)
6. [References](#references)

---

## SECTION 1: OVERVIEW

### High-Level Introduction
[Comprehensive introduction to topic - what, why, when]

### Why This Matters
[Value proposition - consequences of not understanding this concept]

### Scope of This Guide
[What this document covers and what it doesn't - boundary setting]

---

## SECTION 2: CORE CONCEPTS

### Concept 1: [Name]

**Definition:** [Clear explanation of concept]

**Rationale:** [Why this concept important in zarichney-api context]

**Example:**
[Realistic example demonstrating concept]

**Key Principles:**
- **[Principle 1]:** [Explanation]
- **[Principle 2]:** [Explanation]
- **[Principle 3]:** [Explanation]

---

### Concept 2: [Name]

[Continue pattern for 3-5 core concepts]

---

## SECTION 3: IMPLEMENTATION STRATEGIES

### Strategy 1: [Approach Name]

**When to Use:** [Scenarios where this strategy optimal]

**Process:**
1. [Step-by-step implementation]
2. [Detailed guidance]
3. [Validation criteria]

**Example Implementation:**
[Detailed realistic example]

**Trade-offs:**
- **Benefits:** [Advantages of this approach]
- **Costs:** [Disadvantages or limitations]
- **Alternatives:** [When other strategies better]

---

### Strategy 2: [Approach Name]

[Continue pattern for 3-5 implementation strategies]

---

## SECTION 4: ADVANCED TECHNIQUES

### Technique 1: [Expert-Level Pattern]

**Complexity:** [Intermediate/Advanced/Expert]
**Prerequisites:** [What agent must master first]

**Description:** [Comprehensive explanation of technique]

**Application Scenarios:**
[Specific situations where this technique valuable]

**Step-by-Step Process:**
[Detailed implementation guidance]

**Pitfalls to Avoid:**
[Common mistakes when applying this technique]

---

### Technique 2: [Expert-Level Pattern]

[Continue pattern for 2-4 advanced techniques]

---

## SECTION 5: COMMON PITFALLS

### Pitfall 1: [Anti-Pattern Name]

**Description:** [What agents commonly do wrong]

**Why This Happens:** [Root cause of mistake]

**Consequences:** [Impact of this anti-pattern]

**Correct Approach:**
[Step-by-step resolution showing proper pattern]

**Prevention:**
[How to avoid this mistake in future]

---

### Pitfall 2: [Anti-Pattern Name]

[Continue pattern for 3-5 common pitfalls]

---

## TROUBLESHOOTING

### Issue Category 1: [Problem Area]

#### Symptom: [Observable Problem]
**Diagnosis:** [How to confirm this specific issue]
**Root Cause:** [Why this occurs]
**Resolution:**
1. [Detailed resolution step]
2. [Detailed resolution step]
3. [Detailed resolution step]

**Validation:** [How to verify issue resolved]

---

### Issue Category 2: [Problem Area]

[Continue pattern for major troubleshooting categories]

---

## OPTIMIZATION TECHNIQUES

### Optimization 1: [Efficiency Improvement]

**Baseline Performance:** [Current state without optimization]
**Optimized Performance:** [Expected improvement]

**Implementation:**
[Step-by-step optimization process]

**Measurement:**
[How to validate optimization effectiveness]

---

## REFERENCES

### Internal Resources
- **Related Skills:** [Other skills that complement this topic]
- **Templates:** [Relevant templates from this skill's resources]
- **Examples:** [Examples demonstrating concepts from this guide]

### Project Documentation
- **Standards:** [Relevant /Docs/Standards/ files]
- **CLAUDE.md Sections:** [Orchestration patterns related to this topic]
- **Agent Definitions:** [Agent-specific integration patterns]

### External Resources (if applicable)
- [Industry best practices]
- [Relevant technical documentation]
- [Academic or professional references]

---

**Documentation Status:** [CURRENT/UNDER_REVIEW/DEPRECATED]
**Version:** [X.Y]
**Last Updated:** [Date]
**Maintenance Notes:** [How to keep this documentation current]
```

#### Documentation Size Guidelines

**Target:** 250-400 lines (1,000-3,000 tokens)

**Structure Allocation:**
- Table of Contents: 10-15 lines
- Overview: 20-30 lines
- Core Concepts (3-5 concepts): 80-120 lines
- Implementation Strategies (3-5 strategies): 60-100 lines
- Advanced Techniques (2-4 techniques): 40-80 lines
- Common Pitfalls (3-5 pitfalls): 40-60 lines
- Troubleshooting: 30-50 lines
- References: 10-20 lines

**If documentation exceeds 400 lines:**
- Split into multiple focused guides (one per major concept)
- Extract advanced techniques to separate advanced-guide
- Create dedicated troubleshooting document

#### Documentation Quality Checklist

Before deploying documentation:
- [ ] Clear purpose and target audience
- [ ] Prerequisites specified
- [ ] Table of contents for >100 lines
- [ ] Core concepts clearly explained
- [ ] Implementation strategies with examples
- [ ] Advanced techniques for expert users
- [ ] Common pitfalls documented
- [ ] Troubleshooting section comprehensive
- [ ] References to related resources
- [ ] Token count <3,000 (approximately <400 lines)
- [ ] Systematic topic coverage

---

## PROGRESSIVE LOADING OPTIMIZATION

### Context Efficiency Framework

**Goal:** Minimize token load during skill invocation while maximizing agent effectiveness.

#### Progressive Loading Flow

```
Phase 1 - Discovery (Minimal Context):
  Load: YAML frontmatter from all skills (~100 tokens each)
  Total: ~1,000 tokens for scanning 10 skills
  Decision: Identify relevant skill for current task

Phase 2 - Invocation (Core Context):
  Load: Complete SKILL.md instructions (~2,500 tokens average)
  Total: ~2,500 tokens for skill execution guidance
  Decision: Execute workflow steps, identify resource needs

Phase 3 - Resource Access (Targeted Context):
  Load: Specific template/example/doc (~500 tokens average)
  Total: ~3,000 tokens for complete workflow with resources
  Decision: Apply resource to resolve specific need

Phase 4 - Deep Dive (Comprehensive Context):
  Load: Additional documentation (~2,000 tokens)
  Total: ~5,000 tokens for complex scenario resolution
  Decision: Resolve edge case or optimize approach
```

#### Token Efficiency Targets

**YAML Frontmatter:**
- Target: <150 tokens
- Content: Name + description only
- Purpose: Skill discovery and relevance checking

**SKILL.md Instructions:**
- Target: 2,000-5,000 tokens (varies by skill category)
- Content: Purpose, workflow steps, resources overview
- Purpose: Core skill execution without resources

**Resources (On-Demand):**
- Templates: 200-500 tokens each
- Examples: 500-1,500 tokens each
- Documentation: 1,000-3,000 tokens each
- Purpose: Deep dive content without bloating SKILL.md

### Token Measurement Techniques

#### Estimation Formula

**Average:** 1 line ≈ 8 tokens (varies by content density)

**YAML Frontmatter:**
```
5-10 lines → ~50-100 tokens
```

**SKILL.md Sections:**
```
Purpose (10 lines) → ~80 tokens
When to Use (30 lines) → ~240 tokens
Workflow Steps (150 lines) → ~1,200 tokens
Resources Overview (40 lines) → ~320 tokens
Total: ~230 lines → ~1,840 tokens
```

**Resources:**
```
Template (30 lines) → ~240 tokens
Example (100 lines) → ~800 tokens
Documentation (250 lines) → ~2,000 tokens
```

#### Validation Method

1. **Draft Content:** Write SKILL.md or resource content
2. **Count Lines:** Use editor line count feature
3. **Estimate Tokens:** Lines × 8 (conservative estimate)
4. **Validate Budget:** Compare to skill category token budget
5. **Extract if Over:** Move excess content to resources/

**Refinement:** Actual token counts vary based on:
- Code blocks (higher token density)
- Headers and whitespace (lower token density)
- Complex formatting (variable density)

Use 8 tokens/line as conservative estimate for planning.

---

## INTEGRATION WITH SKILL.md

### Resource References in SKILL.md

#### One-Level Deep Reference Pattern (Required)

**Correct:**
```markdown
**Resource:** See `resources/templates/artifact-reporting-template.md` for complete format
```

**Also Correct:**
```markdown
**Resource:** See `resources/examples/coordination-example.md` for realistic demonstration
```

**AVOID Nested References:**
```markdown
❌ BAD: "See resources/templates/ directory for formats (each template lists examples in resources/examples/)"
```

This creates two-level loading: SKILL.md → template → example (inefficient)

**GOOD Alternative:**
```markdown
✅ GOOD: "See resources/templates/artifact-reporting-template.md for format. See resources/examples/coordination-example.md for realistic demonstration."
```

This maintains one-level loading: SKILL.md → template AND SKILL.md → example (efficient)

#### Resources Overview Section Template

Include this section in every SKILL.md after Workflow Steps:

```markdown
## RESOURCES

This skill includes the following resources for progressive loading:

### Templates (Ready-to-Use Formats)

**[template-1-name].md:**
- **Purpose:** [What this template provides - 1 sentence]
- **When to Use:** [Scenario triggering template usage - 1 sentence]
- **Customization:** [What agent needs to fill in - brief]

**[template-2-name].md:**
[Continue pattern for each template]

**Location:** `resources/templates/`
**Usage:** Copy template, fill in specific details, use verbatim in agent work

### Examples (Reference Implementations)

**[example-1-name].md:**
- **Scenario:** [Realistic scenario demonstrated - 1 sentence]
- **Workflow Coverage:** [Which steps this example shows - brief]
- **Learning Points:** [Key patterns agent learns - brief]

**[example-2-name].md:**
[Continue pattern for each example]

**Location:** `resources/examples/`
**Usage:** Review examples for realistic scenarios showing workflow steps in action

### Documentation (Deep Dives)

**[doc-1-name].md:**
- **Topic:** [Advanced concept covered - 1 sentence]
- **Prerequisites:** [What agent should understand before reading - brief]
- **Value:** [What deep understanding this provides - brief]

**[doc-2-name].md:**
[Continue pattern for each documentation file]

**Location:** `resources/documentation/`
**Usage:** Understand skill philosophy, troubleshoot issues, optimize effectiveness
```

**Key Principle:** SKILL.md lists resources with brief descriptions (~10 tokens each), agents load full resources on-demand.

### Workflow Step Resource Integration

Reference resources inline during workflow steps:

**Template Reference Example:**
```markdown
### Step 2: Create Artifact Report

**Purpose:** Document working directory file creation for team awareness

#### Process

1. **Use Standard Format:** Follow artifact reporting template structure
2. **Fill Placeholders:** Customize for specific artifact details
3. **Report Immediately:** Communicate to team using standardized format

**Resource:** See `resources/templates/artifact-reporting-template.md` for exact format
```

**Example Reference Pattern:**
```markdown
### Step 3: Execute Multi-Agent Coordination

**Purpose:** Coordinate handoff between Backend and Frontend specialists

#### Process

1. **Review Coordination Pattern:** Study realistic workflow demonstration
2. **Identify Integration Points:** Determine handoff artifacts
3. **Execute Coordinated Workflow:** Apply pattern to current task

**Resource:** See `resources/examples/backend-frontend-coordination-example.md` for complete workflow
```

**Documentation Reference Pattern:**
```markdown
### Step 4: Optimize Progressive Loading

**Purpose:** Ensure skill resource organization maximizes context efficiency

#### Process

1. **Understand Progressive Loading:** Review architecture philosophy
2. **Apply Token Budgets:** Validate resources within category targets
3. **Test Loading Scenarios:** Verify discovery → invocation → resources flow

**Resource:** See `resources/documentation/progressive-loading-architecture.md` for comprehensive design guide
```

---

## RESOURCE CREATION WORKFLOW

### Phase-by-Phase Resource Development

#### Phase 1: Identify Resource Needs

**During SKILL.md Drafting:**
1. Write workflow steps with full detail
2. Identify content exceeding SKILL.md token budget
3. Determine resource category for extraction (template/example/documentation)
4. Mark extraction points in SKILL.md draft

**Decision Criteria:**
- **Template:** Standardized format agents copy-paste
- **Example:** Complete workflow demonstration with annotations
- **Documentation:** Complex concept requiring >500 lines explanation

#### Phase 2: Create Resources

**For Each Identified Resource:**
1. **Create File:** Use appropriate naming convention
2. **Apply Structure:** Follow category structure pattern
3. **Draft Content:** Write complete resource following size guidelines
4. **Validate Quality:** Use resource-specific quality checklist
5. **Measure Tokens:** Verify within category token budget

**Progressive Creation:**
- Create templates first (immediate application value)
- Then examples (workflow demonstrations)
- Finally documentation (deep understanding)

#### Phase 3: Integrate with SKILL.md

**After Resources Created:**
1. **Update Resources Overview:** Add resource to SKILL.md resources section
2. **Add Inline References:** Link resources from workflow steps
3. **Remove Extracted Content:** Delete content now in resources from SKILL.md body
4. **Validate One-Level Deep:** Confirm no nested resource references
5. **Measure Total Context:** Verify SKILL.md within token budget after extraction

#### Phase 4: Validate Progressive Loading

**Test Loading Scenarios:**
1. **Discovery:** Can agent identify skill from YAML description?
2. **Invocation:** Can agent execute workflow from SKILL.md alone?
3. **Resource Access:** Can agent find and use templates/examples when needed?
4. **Deep Dive:** Can agent access documentation for complex scenarios?

**Efficiency Validation:**
- SKILL.md token count within category budget (2,000-5,000)
- Resources loaded on-demand (not always required)
- Total context <5,000 tokens for basic workflow execution
- Agent can complete task without loading all resources

---

## RESOURCE MAINTENANCE

### Adding New Resources

**When to Add Resource:**
- Agents frequently ask for format specification (create template)
- Workflow pattern reused across multiple tasks (create example)
- Complex concept causes agent confusion (create documentation)

**Addition Process:**
1. **Validate Need:** Confirm resource fills gap not addressed by existing resources
2. **Create Resource:** Follow structure patterns and size guidelines
3. **Update SKILL.md:** Add to resources overview section
4. **Link from Workflow:** Reference from relevant workflow steps
5. **Test with Agents:** Validate resource improves agent effectiveness

### Deprecating Resources

**When to Deprecate:**
- Resource no longer relevant to current skill workflow
- Superseded by better resource with same purpose
- Content merged into /Docs/Standards/ for easier maintenance

**Deprecation Process:**
1. **Mark [DEPRECATED]:** Update resource header with deprecation notice
2. **Provide Migration Path:** Link to replacement resource or standard
3. **Update SKILL.md:** Remove from resources overview, note deprecation
4. **Maintain File:** Keep deprecated file for 12 months (backward compatibility)
5. **Remove After Period:** Delete file after deprecation period expires

### Resource Versioning

**Version Management:**
- Resources evolve independently of SKILL.md
- Maintain backward compatibility when possible
- Document breaking changes in resource header
- Consider skill version bumping for major resource changes

**Version Header Pattern:**
```markdown
**Resource Version:** 2.1
**Last Updated:** 2025-10-25
**Breaking Changes:** [Description if version >2.0]
**Migration Guide:** [How to update usage if breaking change]
```

---

## QUALITY VALIDATION

### Pre-Deployment Checklist

Before finalizing skill with resources:

#### Structure Check
- [ ] All three resource directories present (templates/examples/documentation)
- [ ] Naming conventions followed consistently
- [ ] Each resource in appropriate category directory
- [ ] Resources overview section in SKILL.md complete

#### Content Check
- [ ] Templates have clear placeholders and usage examples
- [ ] Examples demonstrate complete workflows with annotations
- [ ] Documentation covers topics comprehensively with table of contents

#### Integration Check
- [ ] SKILL.md resources overview section lists all resources
- [ ] Resource references use consistent format
- [ ] Progressive loading validated (brief mention in SKILL.md, full content in resource)
- [ ] No nested resource references (one level deep only)

#### Token Efficiency Check
- [ ] SKILL.md within 2,000-5,000 token budget for skill category
- [ ] Templates <500 tokens each (~60 lines)
- [ ] Examples <1,500 tokens each (~200 lines)
- [ ] Documentation <3,000 tokens each (~400 lines)

#### Usability Check
- [ ] Agent can execute basic workflow from SKILL.md alone
- [ ] Agent can find relevant resource when needed
- [ ] Resource provides clear value (not redundant with SKILL.md)
- [ ] Resources tested with at least 2 target agents

---

## TOKEN EFFICIENCY EXAMPLES

### ❌ Anti-Pattern: Embedded Content in SKILL.md

```markdown
## Workflow Steps

### Step 1: Create GitHub Issue

Use this template:

# Issue Title

## Problem Description
[Detailed description...]

## Acceptance Criteria
- [ ] Criterion 1
- [ ] Criterion 2
...
[Embedded 200-line template taking up SKILL.md space]
```

**Problems:**
- Bloats SKILL.md with 200 lines (~1,600 tokens)
- Loads unnecessarily when agent doesn't need template
- Makes SKILL.md harder to navigate
- Violates progressive loading principle

### ✅ Correct Pattern: Progressive Loading

```markdown
## Workflow Steps

### Step 1: Create GitHub Issue

**Purpose:** Document issue using standardized format for team visibility

#### Process

1. **Use Standard Template:** Follow GitHub issue template structure
2. **Fill Required Sections:** Problem description, acceptance criteria, context
3. **Validate Completeness:** Ensure all required fields populated

**Resource:** See `resources/templates/github-issue-template.md` for complete format
```

**Benefits:**
- SKILL.md stays lean (~15 lines, ~120 tokens)
- Agent loads template only when creating issue
- Clear workflow guidance without format bloat
- Progressive loading: SKILL.md → template (on-demand)

**Total Context Efficiency:**
- SKILL.md always loaded: ~120 tokens
- Template loaded when needed: ~1,600 tokens
- Savings when template not needed: ~1,480 tokens (93% reduction)

---

## COMMON ORGANIZATION MISTAKES

### Mistake 1: Nested Resource References

**Problem:**
```markdown
# SKILL.md
**Resource:** See resources/templates/ directory for all formats

# resources/templates/template-1.md
**Examples:** See resources/examples/ for usage demonstrations
```

This creates two-level loading: SKILL.md → template → example

**Solution:**
```markdown
# SKILL.md
**Resource:** See resources/templates/template-1.md for format. See resources/examples/example-1.md for demonstration.
```

This maintains one-level loading: SKILL.md → template AND SKILL.md → example

### Mistake 2: Oversized Templates

**Problem:**
Template file with 150 lines including:
- Detailed format specification
- Placeholder guidance
- 5 complete examples
- Background philosophy explanation

**Solution:**
Split into focused resources:
- **Template:** 40 lines (format + basic placeholder guidance)
- **Example:** 80 lines (1-2 realistic demonstrations)
- **Documentation:** 100 lines (philosophy and advanced usage)

### Mistake 3: Redundant Examples

**Problem:**
Three examples all demonstrating same workflow with minor variations

**Solution:**
- Keep one comprehensive example showing complete workflow
- Add variations section within single example showing alternatives
- Create second example only if demonstrating distinctly different scenario

### Mistake 4: Documentation Without Table of Contents

**Problem:**
300-line documentation file without structure or navigation

**Solution:**
- Add table of contents at top (for files >100 lines)
- Use clear section headers
- Provide internal links for navigation
- Include prerequisites and estimated reading time

### Mistake 5: Missing Resources Overview in SKILL.md

**Problem:**
SKILL.md references resources inline but no consolidated overview section

**Solution:**
- Add Resources section after Workflow Steps
- List all templates/examples/documentation with brief descriptions
- Provide usage guidance for each category
- Enable agent to browse available resources efficiently

---

## RECOMMENDED RESOURCE COMBINATIONS

### Coordination Skills (2,000-3,500 token SKILL.md)

**Typical Resources:**
- 2-3 templates (artifact reporting, communication formats)
- 2-3 examples (single-agent, multi-agent coordination)
- 1-2 documentation (coordination philosophy, troubleshooting)

**Total Resources:** 5-8 files

### Technical Skills (3,000-5,000 token SKILL.md)

**Typical Resources:**
- 3-5 templates (API design, architecture patterns, validation checklists)
- 3-5 examples (simple, intermediate, complex implementations)
- 3-4 documentation (design principles, advanced techniques, troubleshooting)

**Total Resources:** 9-14 files

### Meta Skills (3,500-5,000 token SKILL.md)

**Typical Resources:**
- 4-6 templates (creation templates, structure scaffolding, validation frameworks)
- 3-5 examples (complete creation workflows, before/after comparisons)
- 4-5 documentation (design philosophy, decision frameworks, optimization guides)

**Total Resources:** 11-16 files

### Workflow Skills (2,000-3,500 token SKILL.md)

**Typical Resources:**
- 2-4 templates (GitHub issue/PR formats, checklists, reporting templates)
- 2-3 examples (workflow execution demonstrations, integration scenarios)
- 1-2 documentation (workflow optimization, troubleshooting)

**Total Resources:** 5-9 files

---

## SUMMARY: RESOURCE ORGANIZATION PRINCIPLES

1. **Progressive Loading:** SKILL.md provides workflow guidance, resources provide detailed content
2. **Category Clarity:** Templates (copy-paste), examples (demonstrations), documentation (deep dives)
3. **Token Efficiency:** Resources sized appropriately (templates <500, examples <1,500, docs <3,000 tokens)
4. **One-Level References:** SKILL.md references resources directly, no nested loading
5. **Quality Standards:** Each resource follows structure pattern and meets quality checklist
6. **Discoverability:** Resources overview section enables efficient agent browsing
7. **Maintenance:** Clear versioning, deprecation process, backward compatibility

**By following this guide, PromptEngineer creates skill resources that maximize agent effectiveness through progressive loading efficiency and systematic organization.**

---

**Guide Status:** OPERATIONAL
**Version:** 1.0
**Last Updated:** 2025-10-25
**Usage:** Apply during Phase 4 (Resource Organization) of skill-creation workflow
