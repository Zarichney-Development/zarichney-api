# Skills Directory

**Purpose:** Progressive loading skills for context-efficient agent operations
**Last Updated:** 2025-10-25
**Parent:** [`.claude/`](../README.md)

---

## 1. Purpose & Responsibility

* **What it is:** Collection of reusable skills organized by category, enabling agents to load capabilities on-demand through progressive disclosure architecture

* **Key Objectives:**
  - **Context Efficiency:** 62% average reduction in agent definition context load
  - **Progressive Loading:** Metadata (~100 tokens) → Instructions (~2,500 tokens) → Resources (variable)
  - **Reusability:** Skills shared across multiple agents eliminate redundancy
  - **Scalability:** Unlimited skills without context window bloat

* **Why it exists:** Agent definitions previously contained extensive redundancy (~3,230 lines across 11 agents). Skills extract cross-cutting patterns and domain-specific capabilities into reusable, progressively-loadable units, achieving ~9,864 tokens saved per typical multi-agent session.

---

## 2. Skills Architecture

### Progressive Disclosure Model

**Level 1: Metadata Discovery (~100 tokens)**
```yaml
---
name: skill-name
description: What it does and when to use it
---
```
- Loaded at Claude Code startup
- Enables skill discovery from 100+ potential skills
- 98% token savings vs. loading full skill content

**Level 2: Instructions Loading (2,000-5,000 tokens)**
- SKILL.md body content loaded when skill is relevant
- Comprehensive workflow instructions and guidance
- <500 lines recommended for readability
- 85% savings vs. monolithic agent definitions

**Level 3: Resources Access (variable tokens)**
- Templates, examples, documentation loaded on-demand
- Selective loading based on agent needs
- 60-80% savings through targeted resource access

**Maximum Context (all resources): 4,000-8,000 tokens**
- Still 59-70% savings vs. embedding in agent definition
- Scalable: Unlimited skills without context bloat

---

## 3. Current Skill Categories

### [coordination/](./coordination/README.md)
**Purpose:** Cross-cutting team communication and coordination patterns

**Current Skills:**
1. [working-directory-coordination](./coordination/working-directory-coordination/) - Mandatory team communication protocols

**When to Create Coordination Skill:**
- Pattern used by 3+ agents
- Cross-cutting team workflow (not domain-specific)
- Communication or collaboration protocol
- Stateless operation support (context preservation)

---

### [documentation/](./documentation/README.md)
**Purpose:** Documentation and context management skills for stateless AI operation

**Current Skills:**
1. [documentation-grounding](./documentation/documentation-grounding/) - Systematic framework for loading project standards and module context

**When to Create Documentation Skill:**
- Documentation creation or maintenance workflows
- Context loading and analysis patterns
- Knowledge organization and navigation
- Stateless operation support (comprehensive grounding)

---

### Future Categories (Iterations 2-3)

**technical/** *(Planned)*
**Purpose:** Domain-specific technical patterns and deep expertise

**Examples:**
- API integration patterns
- Database optimization workflows
- Performance profiling procedures
- Error handling frameworks

**When to Create Technical Skill:**
- Deep domain knowledge required
- Specific technology stack expertise
- Reusable technical workflows
- Error-prone operations needing consistency

---

**meta/** *(Iteration 2 - Planned)*
**Purpose:** Skills for creating agents, skills, and commands

**Planned Skills:**
1. agent-creation - 50% faster agent creation with 100% consistency
2. skill-creation - Standardized skill structure and quality assurance
3. command-creation - Consistent UX and clear skill integration

**When to Create Meta-Skill:**
- Team scalability enabler
- Framework/template creation
- Quality assurance automation
- Consistency enforcement

---

**workflow/** *(Iteration 2-3 - Planned)*
**Purpose:** Automated development workflows and operations

**Examples:**
- GitHub issue creation automation
- CI/CD monitoring and orchestration
- Coverage analytics and reporting
- PR consolidation workflows

**When to Create Workflow Skill:**
- Repetitive development operation
- Multi-step process automation
- Tool integration (gh CLI, git, etc.)
- Context collection automation

---

## 4. Official Skills Structure

All skills in this directory **MUST** follow the official Claude Code skills structure:

**Authoritative Reference:** [`Docs/Specs/epic-291-skills-commands/official-skills-structure.md`](../../Docs/Specs/epic-291-skills-commands/official-skills-structure.md)

**Required Structure:**
```
.claude/skills/category/skill-name/
├── SKILL.md                 # YAML frontmatter + instructions (<500 lines recommended)
└── resources/               # Optional: On-demand progressive disclosure
    ├── templates/           # Reusable formats
    ├── examples/            # Reference implementations
    ├── documentation/       # Deep dives
    └── scripts/             # Executable utilities
```

**YAML Frontmatter (in SKILL.md):**
```yaml
---
name: skill-name
description: What it does and when to use it (max 1024 chars)
---
```

**Key Requirements:**
- ✅ YAML frontmatter at top of SKILL.md (NOT separate metadata.json)
- ✅ Only `name` and `description` fields required
- ✅ SKILL.md body <500 lines recommended
- ✅ Resources organized appropriately
- ✅ References one level deep from SKILL.md

**Official Documentation:**
- [Agent Skills Overview](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/overview)
- [Agent Skills Best Practices](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/best-practices)

---

## 5. How to Add a New Skill

### Before Creating

**Prerequisites:**
1. Validate need (used by 3+ agents OR deep domain expertise)
2. Determine appropriate category (coordination, technical, meta, workflow)
3. Review existing skills to avoid duplication
4. Estimate context savings vs. embedding in agent definitions

### Creation Process

**1. Use skill-creation Meta-Skill (Available Iteration 2.2):**
```
Location: .claude/skills/meta/skill-creation/
Purpose: Standardized skill creation framework
Benefit: Consistent structure, quality assurance, optimization patterns
```

**2. Manual Creation (Before Iteration 2.2):**

**Step 1: Create Directory Structure**
```bash
# Create skill directory in appropriate category
mkdir -p .claude/skills/category/skill-name/resources/{templates,examples,documentation}

# Create SKILL.md
touch .claude/skills/category/skill-name/SKILL.md
```

**Step 2: Write YAML Frontmatter**
```yaml
---
name: skill-name
description: Brief description of what this skill does and when to use it. Must include BOTH what AND when.
---
```

**Requirements:**
- `name`: Max 64 chars, lowercase/numbers/hyphens only, no reserved words
- `description`: Max 1024 chars, third person, discovery-optimized

**Step 3: Write SKILL.md Body**

**Recommended Sections:**
```markdown
# Skill Name

## Purpose
[Clear mission statement - what problem does this solve?]

## When to Use
[Specific trigger scenarios when agents should invoke this skill]

## Workflow Steps
[Numbered workflow with clear actions]

## Resources
[References to templates/, examples/, documentation/]

## Integration
[How this skill integrates with other workflows, quality gates]
```

**Step 4: Create Resources**

**Templates:** (resources/templates/)
- Reusable formats agents can copy-paste
- Clear placeholder syntax: `[description]` or `{value}`
- Self-contained with inline documentation

**Examples:** (resources/examples/)
- Realistic scenarios demonstrating skill usage
- Show complete workflows, not fragments
- Annotate key decision points

**Documentation:** (resources/documentation/)
- Deep dives on philosophy and patterns
- Troubleshooting guides
- Table of contents for files >100 lines

**Scripts:** (resources/scripts/)
- Executable utilities for error-prone operations
- Handle error conditions in scripts
- Document dependencies explicitly

**Step 5: Validation**
- Test with at least 2 agents
- Validate progressive loading (frontmatter → instructions → resources)
- Measure context savings vs. embedded approach
- Verify resource accessibility and usability

**Step 6: Documentation**
- Update category README.md with new skill entry
- Add to Epic #291 skills catalog if part of epic
- Document in agent definitions that use the skill

---

## 6. How to Add a New Category

### When to Create New Category

**Create New Category When:**
- Clear functional grouping emerges (5+ skills with common theme)
- Existing categories don't fit the purpose
- Category enables better discoverability
- Pattern validated across multiple agents

**DON'T Create Category When:**
- Only 1-2 skills planned
- Fits existing category with slight stretch
- Temporary or experimental skills

### Creation Process

**Step 1: Create Directory**
```bash
mkdir .claude/skills/new-category
```

**Step 2: Create Category README.md**
```bash
touch .claude/skills/new-category/README.md
```

**Follow Pattern:** Use [coordination/README.md](./coordination/README.md) as template

**Required Sections:**
- Purpose & Responsibility
- When to Create Skills in This Category
- Current Skills (list)
- Maintenance Notes
- Related Documentation

**Step 3: Update Skills Root README.md**
- Add category to section 3 (Current Skill Categories)
- Update quick reference
- Document category purpose

**Step 4: Update Epic #291 Specs**
- Add to official-skills-structure.md if part of epic
- Document in skills-catalog.md

---

## 7. Skill Usage Patterns

### Agent Skill Reference Pattern

**In Agent Definitions:**
```markdown
## [Capability] Implementation
**SKILL REFERENCE**: `.claude/skills/category/skill-name/`

[2-3 line summary of skill purpose]

Key Workflow: [Step 1 | Step 2 | Step 3]

[See skill for complete instructions]
```

**Benefits:**
- Agents reference skills, don't duplicate content
- Skills updated once, all agents benefit
- Clear separation: agent identity vs. cross-cutting patterns

### Progressive Loading Pattern

**1. Agent Startup:**
- Loads YAML frontmatter for all skills (~100 tokens each)
- Determines relevant skills based on description

**2. Task Engagement:**
- Loads SKILL.md when skill is triggered
- Reads workflow instructions

**3. Execution:**
- Accesses specific resources as needed
- Templates, examples, documentation on-demand

**Efficiency:**
- Maximum: ~8,000 tokens (all resources)
- Typical: ~2,500-3,000 tokens (instructions only)
- Discovery: ~100 tokens (frontmatter only)

---

## 8. Epic #291 Integration

### Implementation Timeline

**Iteration 1 (Current): Foundation**
- Issue #311: working-directory-coordination (COMPLETE)
- Issue #310: documentation-grounding (COMPLETE) + core-issue-focus
- Issue #309: github-issue-creation + flexible-authority-management
- Issue #308: Validation framework + templates

**Iteration 2: Meta-Skills & Workflow Skills**
- Issue #307: agent-creation + skill-creation meta-skills
- Issue #306: command-creation meta-skill
- Future: Workflow automation skills

**Iteration 3: Documentation Alignment**
- Skills development guide
- Template creation
- Standards updates

**Iteration 4: Agent Refactoring**
- All 11 agents reference skills
- 62% average context reduction
- ~25,840 tokens saved across team

**Iteration 5: Integration & Validation**
- Comprehensive testing
- Performance validation
- ~9,864 tokens saved per session confirmed

### Expected Savings

**Total Context Reduction:**
- Agent definitions: 5,210 → 1,980 lines (62% reduction)
- Per agent: ~300 lines average eliminated
- Total tokens: ~25,840 saved across 11 agents
- Session savings: ~9,864 tokens per multi-agent workflow

**Skills Created:**
- Core skills: 5 (coordination, documentation, github, technical patterns)
- Meta-skills: 3 (agent-creation, skill-creation, command-creation)
- Total: 8 skills eliminating ~1,850 lines redundancy

---

## 9. Validation & Quality

### Skill Quality Standards

**Context Efficiency:**
- Frontmatter <150 tokens (target: ~100)
- Instructions 2,000-5,000 tokens (optimal: ~2,500)
- SKILL.md <500 lines recommended

**Clarity & Usability:**
- Clear purpose and triggers
- Specific, actionable workflow steps
- Practical templates and examples

**Integration:**
- No quality gate bypass
- AI Sentinel compatible
- Agent coordination validated

**Testing:**
- Validated with at least 2 agents
- Progressive loading verified
- Resource accessibility confirmed

### Validation Scripts (Iteration 1.4)

**Frontmatter Validation:**
- YAML parsing from SKILL.md
- Required fields present (name, description)
- Name constraints (max 64 chars, lowercase/numbers/hyphens)
- Description constraints (max 1024 chars, non-empty)
- Error if metadata.json found (incorrect structure)

**Structure Validation:**
- SKILL.md exists in skill directory
- Resources directory properly organized
- File references one level deep

---

## 10. Common Patterns & Anti-Patterns

### ✅ Good Patterns

**Progressive Disclosure:**
```
SKILL.md (concise main instructions)
├── Basic workflow (90% of use cases)
├── References to advanced topics
└── resources/
    ├── templates/ (immediate usability)
    ├── examples/ (educational depth)
    └── documentation/ (comprehensive coverage)
```

**Discovery-Optimized Description:**
```yaml
description: Extract text and tables from PDF files, fill forms, merge documents. Use when working with PDFs or document extraction.
```
- Includes WHAT: "Extract text and tables from PDF files"
- Includes WHEN: "when working with PDFs or document extraction"
- Specific terms for discovery

**Maintainable Structure:**
- Clear section headers
- One level deep references
- Table of contents for long files
- Consistent terminology

### ❌ Anti-Patterns

**Vague Description:**
```yaml
description: Helps with documents
```
- Missing WHAT specifically
- Missing WHEN to use
- Too generic for discovery

**Deeply Nested References:**
```
SKILL.md → file1.md → file2.md → file3.md
```
- Creates cognitive load
- Hard to navigate
- Keep one level deep from SKILL.md

**Monolithic SKILL.md:**
```
SKILL.md (2,000 lines, 15,000 tokens)
```
- Defeats progressive loading purpose
- Split to SKILL.md + resources/

**Windows-Style Paths:**
```
resources\templates\file.md  # WRONG
resources/templates/file.md  # CORRECT
```

---

## 11. Troubleshooting

**Problem:** Skill not loading correctly
- **Check:** YAML frontmatter exists at top of SKILL.md
- **Check:** Name and description fields present
- **Check:** No metadata.json file (incorrect structure)

**Problem:** Agent can't find skill resources
- **Check:** Resources directory exists
- **Check:** File paths use forward slashes
- **Check:** References are one level deep from SKILL.md

**Problem:** Skill description not appearing in discovery
- **Check:** Description includes WHAT and WHEN
- **Check:** Description <1024 characters
- **Check:** Specific terms for searchability

**Problem:** Context savings not realized
- **Solution:** Review SKILL.md length (<500 lines recommended)
- **Solution:** Move advanced content to resources/
- **Solution:** Validate progressive loading pattern

---

## 12. Related Documentation

**Official Structure:**
- [Official Skills Structure Spec](../../Docs/Specs/epic-291-skills-commands/official-skills-structure.md) - AUTHORITATIVE
- [Skills Catalog](../../Docs/Specs/epic-291-skills-commands/skills-catalog.md) - All 8 skills
- [Implementation Iterations](../../Docs/Specs/epic-291-skills-commands/implementation-iterations.md)

**Category Documentation:**
- [Coordination Skills](./coordination/README.md) - Team communication protocols
- [Documentation Skills](./documentation/README.md) - Context loading and documentation management

**Meta-Skills (Future):**
- [skill-creation](./meta/skill-creation/) - When available (Iteration 2.2)
- [agent-creation](./meta/agent-creation/) - When available (Iteration 2.1)

**Standards:**
- [Documentation Standards](../../Docs/Standards/DocumentationStandards.md)
- [Claude Code Official Docs](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/)

---

## 13. Quick Reference

**Current Skill Count:** 2 skills

**By Category:**
- Coordination: 1 skill (working-directory-coordination)
- Documentation: 1 skill (documentation-grounding)
- Technical: 0 skills (future)
- Meta: 0 skills (Iteration 2)
- Workflow: 0 skills (Iteration 2-3)

**Epic #291 Status:**
- Iteration 1: Foundation (current - Issues #311, #310 complete)
- Target: 8 skills total (5 core + 3 meta)
- Savings: ~9,864 tokens per session

**Category Count:** 2 categories (coordination/, documentation/)

**Future Categories:** technical/, meta/, workflow/

---

**Directory Status:** ✅ Active Development (Epic #291 Iteration 1)

**Next Updates:**
- Issue #310: Add core-issue-focus skill (in progress)
- Issue #309: Add github-issue-creation, flexible-authority-management skills
- Iteration 2: Create meta/ category with 3 meta-skills
