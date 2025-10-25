# .claude/ Directory

**Purpose:** Claude Code customization directory for zarichney-api project
**Last Updated:** 2025-10-25
**Epic:** #291 - Agent Skills & Slash Commands Integration

---

## 1. Purpose & Responsibility

* **What it is:** Customization directory for Claude Code CLI containing agent definitions, slash commands, and progressive loading skills

* **Key Objectives:**
  - **Agent Orchestration:** Define the 12-agent development team structure and coordination patterns
  - **Workflow Automation:** Provide slash commands for common development operations
  - **Context Efficiency:** Enable progressive skill loading for 62% average context reduction
  - **Scalability:** Support unlimited agents, skills, and commands without context bloat

* **Why it exists:** Claude Code's extensibility system allows project-specific customization through filesystem-based agents, skills, and commands. This directory contains all zarichney-api customizations optimized for the multi-agent orchestration framework.

---

## 2. Directory Structure

```
.claude/
├── README.md                    # This file - directory overview
├── agents/                      # Agent definitions (12-agent team)
│   ├── README.md               # Agents directory documentation
│   ├── backend-specialist.md   # Backend domain specialist
│   ├── frontend-specialist.md  # Frontend domain specialist
│   ├── test-engineer.md        # Testing specialist
│   ├── documentation-maintainer.md
│   ├── workflow-engineer.md
│   ├── code-changer.md
│   ├── security-auditor.md
│   ├── bug-investigator.md
│   ├── compliance-officer.md
│   ├── architectural-analyst.md
│   └── prompt-engineer.md
├── commands/                    # Slash commands for workflows
│   ├── README.md               # Commands directory documentation
│   ├── test-report.md          # Comprehensive test analysis
│   └── tackle-epic-issue.md    # Epic #291 specialized workflow
└── skills/                      # Progressive loading skills
    ├── README.md               # Skills root documentation
    ├── coordination/           # Cross-cutting coordination patterns
    │   ├── README.md          # Coordination category documentation
    │   └── working-directory-coordination/
    │       ├── SKILL.md       # Skill instructions with YAML frontmatter
    │       └── resources/     # Templates, examples, documentation
    └── <future-categories>/    # technical/, meta/, workflow/, etc.
```

---

## 3. Component Overview

### Agents (`/agents/`)
**Purpose:** Define specialized agents in the 12-agent development team

**Key Characteristics:**
- Agent definitions written in Markdown
- Define role, authority, integration patterns, coordination requirements
- Optimized through Epic #291 for 62% average context reduction
- Reference skills for cross-cutting patterns

**Documentation:** [agents/README.md](./agents/README.md)

---

### Commands (`/commands/`)
**Purpose:** Slash commands for workflow automation and developer productivity

**Key Characteristics:**
- Markdown files with YAML frontmatter
- CLI-style interface (argument parsing, validation, user messaging)
- Delegate to skills for implementation logic
- Command-skill separation of concerns

**Documentation:** [commands/README.md](./commands/README.md)

---

### Skills (`/skills/`)
**Purpose:** Progressive loading capabilities for context-efficient agent operations

**Key Characteristics:**
- Filesystem-based hierarchical loading (frontmatter → instructions → resources)
- Organized by category (coordination/, technical/, meta/, workflow/)
- YAML frontmatter for skill discovery (~100 tokens)
- SKILL.md with comprehensive instructions (<500 lines recommended)
- Optional resources/ for templates, examples, documentation

**Documentation:** [skills/README.md](./skills/README.md)

**Official Structure:** [`Docs/Specs/epic-291-skills-commands/official-skills-structure.md`](../Docs/Specs/epic-291-skills-commands/official-skills-structure.md)

---

## 4. Epic #291 Context

This directory structure is central to **Epic #291: Agent Skills & Slash Commands Integration**.

### Epic Objectives
- ✅ Reduce agent definition context load by 62% average through skill references
- ✅ Enable progressive skill loading (metadata → instructions → resources)
- ✅ Provide workflow automation through slash commands
- ✅ Establish meta-skills for unlimited agent/skill/command creation
- ✅ Achieve ~9,864 tokens saved per typical multi-agent session

### Documentation Hierarchy
1. **Official Structure:** `Docs/Specs/epic-291-skills-commands/official-skills-structure.md` (authoritative)
2. **Skills Catalog:** `Docs/Specs/epic-291-skills-commands/skills-catalog.md` (all 8 skills)
3. **Commands Catalog:** `Docs/Specs/epic-291-skills-commands/commands-catalog.md` (all 4 commands)
4. **Implementation Plan:** `Docs/Specs/epic-291-skills-commands/implementation-iterations.md` (5 iterations)

---

## 5. Maintenance Guidelines

### Adding a New Agent
1. Review existing agents in `/agents/` for patterns
2. Use `agent-creation` meta-skill when available (Iteration 2.1)
3. Follow agent definition structure from CLAUDE.md
4. Reference appropriate skills for cross-cutting patterns
5. Update `/agents/README.md` with new agent entry
6. Test agent engagement through Claude orchestration

### Adding a New Command
1. Review existing commands in `/commands/` for patterns
2. Use `command-creation` meta-skill when available (Iteration 2.3)
3. Follow command structure: YAML frontmatter + usage examples
4. Delegate to skills for implementation logic
5. Update `/commands/README.md` with new command entry
6. Test command execution and argument handling

### Adding a New Skill
1. Determine appropriate category (coordination/, technical/, meta/, workflow/)
2. Create category directory with README.md if new category
3. Use `skill-creation` meta-skill when available (Iteration 2.2)
4. Follow official structure: YAML frontmatter in SKILL.md + optional resources/
5. Validate with at least 2 agents before epic integration
6. Update category README.md with new skill entry

### Adding a New Skill Category
1. Create directory: `.claude/skills/<category-name>/`
2. Create `README.md` following coordination/ pattern
3. Document category purpose, scope, and when to use
4. Update `/skills/README.md` with category entry
5. Reference in Epic #291 specifications

---

## 6. Standards & Conventions

### Naming Conventions
- **Agents:** `lowercase-with-hyphens.md` (e.g., `backend-specialist.md`)
- **Commands:** `lowercase-with-hyphens.md` (e.g., `test-report.md`)
- **Skills:** `lowercase-with-hyphens/` directory (e.g., `working-directory-coordination/`)
- **Categories:** `lowercase-singular/` directory (e.g., `coordination/`, not `coordinations/`)

### File Organization
- **Agents:** Single markdown file per agent in `/agents/`
- **Commands:** Single markdown file per command in `/commands/`
- **Skills:** Directory per skill in appropriate category with SKILL.md + resources/
- **README Files:** Every directory has README.md for navigation and maintenance

### Documentation Requirements
- **Self-Contained:** Each README explains its own purpose and structure
- **Navigation Links:** Cross-references to related documentation
- **Maintenance Focus:** How to add, modify, and maintain components
- **Standards Compliance:** Follow DocumentationStandards.md patterns

---

## 7. Related Documentation

**Epic #291 Specifications:**
- [Epic Overview](../Docs/Specs/epic-291-skills-commands/README.md)
- [Official Skills Structure](../Docs/Specs/epic-291-skills-commands/official-skills-structure.md)
- [Skills Catalog](../Docs/Specs/epic-291-skills-commands/skills-catalog.md)
- [Commands Catalog](../Docs/Specs/epic-291-skills-commands/commands-catalog.md)
- [Implementation Iterations](../Docs/Specs/epic-291-skills-commands/implementation-iterations.md)

**Project Standards:**
- [Documentation Standards](../Docs/Standards/DocumentationStandards.md)
- [Task Management Standards](../Docs/Standards/TaskManagementStandards.md)
- [CLAUDE.md](../CLAUDE.md) - Orchestration guide

**Official Claude Code Documentation:**
- [Agent Skills Overview](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/overview)
- [Agent Skills Best Practices](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/best-practices)

---

## 8. Quick Navigation

**By Component Type:**
- [Agents Directory](./agents/README.md) - 12-agent team definitions
- [Commands Directory](./commands/README.md) - Workflow automation
- [Skills Directory](./skills/README.md) - Progressive loading capabilities

**By Category:**
- [Coordination Skills](./skills/coordination/README.md) - Cross-cutting team patterns
- Future categories (technical/, meta/, workflow/) - To be created in Iterations 2-3

**By Epic Phase:**
- Iteration 1 (Current): Core coordination skills + templates
- Iteration 2 (Upcoming): Meta-skills + workflow commands
- Iteration 3: Documentation alignment
- Iteration 4: Agent refactoring with skill references
- Iteration 5: Integration & validation

---

**Directory Status:** ✅ Active Development (Epic #291 Iteration 1)

**Maintenance:** Update this README when adding new agents, commands, skills, or categories. Last updated during Issue #311 (Iteration 1.1).
