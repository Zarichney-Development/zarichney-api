# Official Claude Code Skills Structure Specification

**Last Updated:** 2025-10-25
**Purpose:** Authoritative reference for Epic #291 aligned with official Claude Code documentation
**Official Docs:** https://docs.claude.com/en/docs/agents-and-tools/agent-skills/ & https://docs.claude.com/en/docs/agents-and-tools/agent-skills/best-practices

---

## ⚠️ CRITICAL CORRECTION

This specification was created after discovering misalignment between Epic #291 initial planning assumptions and official Claude Code documentation. **This document is now the authoritative reference** for all skill creation in this epic.

### What Changed
- **REMOVED:** Separate `metadata.json` file (incorrect assumption)
- **ADDED:** YAML frontmatter within SKILL.md (official structure)
- **SIMPLIFIED:** Metadata fields to only `name` and `description` (official requirements)

---

## 1. Official Skill Structure

### Directory Organization

```
.claude/skills/category/skill-name/
├── SKILL.md                 # YAML frontmatter + instructions (<500 lines recommended)
└── resources/               # Optional: On-demand progressive disclosure
    ├── templates/           # Reusable formats
    ├── examples/            # Reference implementations
    ├── documentation/       # Deep dives
    └── scripts/             # Executable utilities
```

### YAML Frontmatter (Required in SKILL.md)

**Location:** Top of SKILL.md file, before any markdown content

**Format:**
```yaml
---
name: skill-name
description: Brief description of what this skill does and when to use it
---
```

**Field Specifications:**

#### `name` (Required)
- **Max Length:** 64 characters
- **Allowed Characters:** Lowercase letters, numbers, hyphens only
- **Restrictions:**
  - Cannot contain XML tags
  - Cannot use reserved words: "anthropic", "claude"
- **Examples:**
  - ✅ `working-directory-coordination`
  - ✅ `documentation-grounding`
  - ✅ `github-issue-creation`
  - ❌ `WorkingDirectory` (uppercase not allowed)
  - ❌ `working_directory` (underscores not allowed)

#### `description` (Required)
- **Max Length:** 1024 characters
- **Requirements:**
  - Non-empty
  - Cannot contain XML tags
  - Should be written in third person
  - **Must include BOTH:**
    - What the skill does
    - When to use it
- **Critical for Discovery:** Claude uses this to choose from 100+ available skills
- **Examples:**
  - ✅ "Standardize working directory usage and team communication protocols. Use when agents need to discover existing artifacts, report new deliverables, or integrate work from other team members."
  - ✅ "Extract text and tables from PDF files, fill forms, merge documents. Use when working with PDFs or document extraction."
  - ❌ "Helps with documents" (too vague, missing 'when')

---

## 2. Progressive Loading Architecture

### Level 1: Metadata Discovery (~100 tokens)
**What:** YAML frontmatter loaded at Claude Code startup
**Purpose:** Enable skill discovery and selection from potentially 100+ skills
**Token Budget:** ~100 tokens for frontmatter (name + description)

### Level 2: Instructions Loading (<5k tokens)
**What:** SKILL.md body content loaded when skill is relevant and triggered
**Purpose:** Provide comprehensive workflow instructions and guidance
**Token Budget:** <5,000 tokens recommended, <500 lines highly recommended
**Best Practice:** Keep main instructions concise, split advanced topics to separate files

### Level 3: Resources Access (variable tokens)
**What:** Additional files in resources/ loaded on-demand
**Purpose:** Detailed templates, examples, documentation accessed only when needed
**Token Budget:** Variable based on resource size
**Best Practice:**
- Reference one level deep from SKILL.md
- Include table of contents for files >100 lines
- Use descriptive filenames

---

## 3. SKILL.md Content Structure

### Recommended Sections

```markdown
---
name: skill-name
description: What it does and when to use it
---

# Skill Name

[Brief introduction paragraph]

## Purpose

[Clear mission statement - what problem does this solve?]

## When to Use

[Specific trigger scenarios when agents should invoke this skill]
- Trigger 1: [scenario description]
- Trigger 2: [scenario description]

## Workflow Steps

[Numbered workflow with clear actions]
1. **Step Name**: [What to do and why]
2. **Step Name**: [What to do and why]

## Resources

[References to additional files]
- Templates: [List files in resources/templates/]
- Examples: [List files in resources/examples/]
- Documentation: [List files in resources/documentation/]

## Integration

[How this skill integrates with other workflows, quality gates, etc.]

## Troubleshooting

[Common issues and solutions - optional but helpful]
```

### Content Guidelines

**Keep It Concise:**
- Challenge each piece of information: "Does Claude really need this explanation?"
- Context window is shared with conversation history and other skills
- Claude loads resources on-demand, so split content appropriately

**Appropriate Degrees of Freedom:**
- **High freedom** (text instructions): Multiple valid approaches exist
- **Medium freedom** (pseudocode): Preferred pattern with some variation
- **Low freedom** (specific scripts): Error-prone operations requiring consistency

**Maintain Consistent Terminology:**
- Choose one term and use it throughout
- Example: "API endpoint" (not mixing "URL," "route," "path")

**Avoid Time-Sensitive Information:**
- Use "old patterns" sections with deprecated methods in collapsible details
- Don't use conditional date logic

---

## 4. Progressive Disclosure Patterns

### Pattern 1: High-Level Guide with References

**SKILL.md:**
```markdown
## Basic Usage
[Core workflow steps]

## Advanced Topics
For detailed guidance on specific scenarios:
- Form filling: See FORMS.md
- API integration: See API-REFERENCE.md
- Complex examples: See EXAMPLES.md
```

**When to Use:** Skill has both basic and advanced use cases

### Pattern 2: Domain-Specific Organization

**SKILL.md:**
```markdown
## Domain-Specific Workflows
- Finance workflows: See resources/documentation/finance.md
- Sales workflows: See resources/documentation/sales.md
- Product workflows: See resources/documentation/product.md
```

**When to Use:** Skill applies to multiple domains with different patterns

### Pattern 3: Conditional Details

**SKILL.md:**
```markdown
## Basic Workflow
[90% of use cases - keep in main file]

## Edge Cases
For rare scenarios requiring special handling: See resources/documentation/edge-cases.md
```

**When to Use:** Most usage is straightforward, but edge cases need extensive documentation

---

## 5. Resource Organization Best Practices

### templates/
**Purpose:** Reusable formats agents can copy-paste
**Examples:**
- `artifact-discovery-template.md`
- `issue-template.md`
- `commit-message-template.md`

**Best Practices:**
- Clear placeholder syntax: `[description]` or `{value}`
- Self-contained (minimal external context needed)
- Include inline documentation/comments

### examples/
**Purpose:** Reference implementations demonstrating skill usage
**Examples:**
- `multi-agent-coordination-example.md`
- `api-integration-example.md`
- `complete-workflow-example.md`

**Best Practices:**
- Realistic scenarios with actual system context
- Show complete workflows, not fragments
- Annotate key decision points
- Demonstrate all major workflow steps

### documentation/
**Purpose:** Deep dives, philosophy, troubleshooting
**Examples:**
- `protocol-philosophy.md`
- `troubleshooting-guide.md`
- `performance-optimization.md`

**Best Practices:**
- Table of contents for files >100 lines
- Comprehensive but focused on single topic
- Link back to relevant SKILL.md sections

### scripts/
**Purpose:** Executable utilities for error-prone operations
**Examples:**
- `validate-metadata.sh`
- `generate-report.py`
- `transform-data.js`

**Best Practices:**
- Solve problems, don't punt to Claude
- Handle error conditions in scripts
- Document dependencies explicitly
- Make clear whether Claude should execute or read as reference

---

## 6. Validation Requirements

### YAML Frontmatter Validation

**Required Checks:**
- ✅ Frontmatter exists at top of SKILL.md
- ✅ `name` field present and valid (max 64 chars, lowercase/numbers/hyphens)
- ✅ `description` field present and valid (non-empty, max 1024 chars)
- ✅ No XML tags in either field
- ✅ No reserved words in `name` field

**Error if:**
- ❌ Separate `metadata.json` file exists (incorrect structure)
- ❌ Frontmatter missing from SKILL.md
- ❌ Required fields missing or invalid

### Content Validation

**Recommended Checks:**
- ⚠️ SKILL.md body <500 lines (best practice)
- ⚠️ SKILL.md total tokens <5,000 (guideline)
- ⚠️ Resource references one level deep from SKILL.md
- ⚠️ Files >100 lines include table of contents

---

## 7. Token Budget Guidelines

### Discovery Phase (Metadata Only)
**Target:** ~100 tokens
**Includes:** YAML frontmatter (name + description)
**Efficiency:** 98.6% savings vs. loading full skill content
**Usage:** Claude Code startup, skill browsing

### Invocation Phase (Instructions)
**Target:** 2,000-5,000 tokens (optimally ~2,500)
**Includes:** SKILL.md body content
**Efficiency:** 85% savings vs. monolithic agent definitions
**Usage:** When skill is relevant and triggered

### Execution Phase (Resources)
**Target:** Variable, load only what's needed
**Includes:** Specific templates, examples, or documentation
**Efficiency:** 60-80% savings through selective loading
**Usage:** On-demand when agent needs specific guidance

### Maximum Context (Full Skill)
**Target:** ~4,000-8,000 tokens all-in
**Includes:** Frontmatter + SKILL.md + all resources
**Efficiency:** Still 59-70% savings vs. embedding in agent definition
**Scalability:** Unlimited skills without context bloat

---

## 8. Common Anti-Patterns to Avoid

### ❌ Separate metadata.json File
**Problem:** Not part of official structure
**Solution:** Use YAML frontmatter in SKILL.md

### ❌ Windows-Style Paths
**Problem:** `resources\templates\file.md` breaks on Unix systems
**Solution:** Use forward slashes: `resources/templates/file.md`

### ❌ Too Many Options Without Default
**Problem:** Analysis paralysis for agents
**Solution:** Provide a sensible default with escape hatch for alternatives

### ❌ Vague Skill Names
**Problem:** "helper," "utils," "tools" don't convey purpose
**Solution:** Specific, descriptive names: "github-issue-creation," "pdf-extraction"

### ❌ Deeply Nested File References
**Problem:** SKILL.md → file1.md → file2.md → file3.md creates cognitive load
**Solution:** Keep references one level deep from SKILL.md

### ❌ Assuming Tools Are Installed
**Problem:** Scripts fail without explicit installation steps
**Solution:** Document dependencies, include installation instructions

### ❌ Vague Descriptions
**Problem:** "Helps with documents" doesn't enable proper discovery
**Solution:** "Extract text and tables from PDF files. Use when working with PDFs or document extraction."

---

## 9. Skill Creation Workflow

### Phase 1: Scope Definition
1. Identify cross-cutting concern or domain specialization
2. Validate skill is needed by 3+ agents OR provides deep technical pattern
3. Define clear purpose and "when to use" triggers
4. Estimate token budget for metadata, instructions, resources

### Phase 2: Structure Design
1. Draft YAML frontmatter (name + description)
2. Outline SKILL.md sections (purpose, when to use, workflow, resources)
3. Identify resources needed (templates, examples, documentation)
4. Plan progressive disclosure strategy

### Phase 3: Content Creation
1. Write YAML frontmatter focusing on discovery-optimized description
2. Create SKILL.md with concise main instructions (<500 lines)
3. Develop templates for reusable formats
4. Create realistic examples demonstrating complete workflows
5. Write deep-dive documentation for complex topics

### Phase 4: Validation
1. Verify YAML frontmatter valid and complete
2. Check token budgets (frontmatter ~100, SKILL.md <5k)
3. Test progressive loading (metadata → instructions → resources)
4. Validate with at least 2 agents for real-world usage
5. Measure context savings vs. embedded approach

### Phase 5: Integration
1. Document agent integration snippets
2. Update agent definitions with skill references
3. Test agent effectiveness with skill loading
4. Monitor actual token usage and optimize if needed

---

## 10. Official Documentation References

**Primary References:**
- [Agent Skills Overview](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/overview) - Official structure specification
- [Agent Skills Best Practices](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/best-practices) - Authoring guidance

**Key Takeaways from Official Docs:**
- Metadata in YAML frontmatter, not separate JSON file
- Only `name` and `description` required in frontmatter
- Keep SKILL.md under 500 lines recommended
- Progressive disclosure through resource files
- Description critical for skill discovery
- Challenge each piece of information for necessity

---

## 11. Skill Category Documentation Requirements

### Mandatory README.md for Each Category

**Requirement:** Every skill category directory MUST have a README.md file

**Purpose:**
- Explain category purpose and scope
- List all skills in the category
- Provide maintenance guidance
- Enable navigation and discoverability

**Required Sections:**

```markdown
# Category Name Skills Category

## 1. Purpose & Responsibility
[What this category contains and why it exists]

## 2. Current Skills
[List of all skills with brief descriptions and links]

## 3. When to Create a Skill in This Category
[Clear criteria for category membership]
[What belongs vs. what doesn't]

## 4. Maintenance Notes
[How to add/update/deprecate skills]
[Category-specific patterns and standards]

## 5. Related Documentation
[Links to parent docs, specs, standards]
```

**Example:** See `.claude/skills/coordination/README.md` for reference implementation

### Directory Structure with Documentation

```
.claude/skills/
├── README.md                    # Skills root documentation
├── coordination/                # Cross-cutting coordination patterns
│   ├── README.md               # REQUIRED: Category documentation
│   ├── working-directory-coordination/
│   │   ├── SKILL.md
│   │   └── resources/
│   └── core-issue-focus/
│       ├── SKILL.md
│       └── resources/
└── technical/                   # Domain-specific patterns (future)
    ├── README.md               # REQUIRED: Category documentation
    └── <skills>/
```

### Individual Skills Do NOT Need README.md

**Important:** Individual skill directories use SKILL.md (with YAML frontmatter) as their primary documentation. Do NOT create README.md files within individual skill directories.

**Correct Structure:**
```
working-directory-coordination/
├── SKILL.md                 # ✅ Primary skill documentation
└── resources/               # ✅ Additional resources
```

**Incorrect Structure:**
```
working-directory-coordination/
├── README.md                # ❌ NOT NEEDED - use SKILL.md instead
├── SKILL.md
└── resources/
```

**Rationale:** SKILL.md with YAML frontmatter is the official Claude Code structure. Adding README.md creates redundancy and confusion about which file is authoritative.

### .claude/ Root Directory Documentation

**Required Documentation Hierarchy:**

```
.claude/
├── README.md                # ✅ REQUIRED: Root directory overview
├── agents/
│   ├── README.md           # ✅ REQUIRED: Agents directory overview
│   └── *.md                # Agent definitions
├── commands/
│   ├── README.md           # ✅ REQUIRED: Commands directory overview
│   └── *.md                # Command definitions
└── skills/
    ├── README.md           # ✅ REQUIRED: Skills root overview
    └── <category>/
        ├── README.md       # ✅ REQUIRED: Category documentation
        └── <skill-name>/
            ├── SKILL.md    # ✅ Skill documentation (NOT README.md)
            └── resources/
```

**Purpose:**
- Self-contained knowledge at each directory level
- Clear navigation from root to leaves
- Maintenance guidance at appropriate granularity
- Consistent with DocumentationStandards.md philosophy

**Reference:** See `.claude/README.md`, `.claude/agents/README.md`, `.claude/commands/README.md`, `.claude/skills/README.md` for implementation examples

---

## 12. Epic #291 Application

### All Skills Created in This Epic Must:
1. ✅ Use YAML frontmatter in SKILL.md (NOT separate metadata.json)
2. ✅ Include only `name` and `description` in frontmatter
3. ✅ Keep SKILL.md body under 500 lines
4. ✅ Organize resources appropriately (templates, examples, documentation)
5. ✅ Reference resources one level deep from SKILL.md
6. ✅ Optimize description for discovery among 100+ skills
7. ✅ Validate progressive loading works correctly
8. ✅ Test with at least 2 agents for real-world usage

### Validation Scripts Must Enforce:
1. ✅ YAML frontmatter exists in SKILL.md
2. ✅ Required fields present (name, description)
3. ✅ Field constraints met (length, character restrictions)
4. ❌ NO metadata.json file allowed
5. ⚠️ SKILL.md <500 lines recommended (warning if exceeded)

---

**Status:** ✅ **OFFICIAL SPECIFICATION - AUTHORITATIVE FOR EPIC #291**

**Next Actions:**
1. Update all Epic #291 specification files to reference this document
2. Rectify already-created skills to match official structure
3. Use this specification for all future skill creation (Issues #310, #309, remaining iterations)
4. Include in Iteration 1.3 templates as authoritative source
