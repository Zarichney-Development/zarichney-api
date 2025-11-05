# .claude/ Directory

**Purpose:** Claude Code customization directory for zarichney-api project
**Last Updated:** 2025-10-28
**Epic:** #291 - Agent Skills & Slash Commands Integration (COMPLETE - Archived 2025-10-27)

---

## 1. Purpose & Responsibility

* **What it is:** Customization directory for Claude Code CLI containing agent definitions, slash commands, and progressive loading skills

* **Key Objectives:**
  - **Agent Orchestration:** Define the 12-agent development team structure and coordination patterns
  - **Workflow Automation:** Provide slash commands for common development operations (7 commands delivered)
  - **Context Efficiency:** Progressive skill loading achieving 50% average context reduction
  - **Scalability:** Support unlimited agents, skills, and commands through meta-skills framework

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
├── commands/                    # Slash commands for workflows (7 commands)
│   ├── README.md               # Commands directory documentation
│   ├── test-report.md          # Comprehensive test suite with AI analysis
│   ├── coverage-report.md      # Coverage data with trend analysis
│   ├── create-issue.md         # Automated GitHub issue creation
│   ├── workflow-status.md      # GitHub Actions workflow monitoring
│   ├── merge-coverage-prs.md   # Coverage Excellence Merge Orchestrator
│   ├── tackle-epic-issue.md    # Epic issue execution workflow
│   └── epic-complete.md        # Epic archival and completion
└── skills/                      # Progressive loading skills (8 skills)
    ├── README.md               # Skills root documentation
    ├── coordination/           # Cross-cutting coordination patterns (3 skills)
    │   ├── README.md
    │   ├── working-directory-coordination/
    │   ├── core-issue-focus/
    │   └── epic-completion/
    ├── documentation/          # Documentation workflows (1 skill)
    │   └── documentation-grounding/
    ├── github/                 # GitHub automation (1 skill)
    │   └── github-issue-creation/
    └── meta/                   # Skills for creating agents/skills/commands (3 skills)
        ├── agent-creation/
        ├── skill-creation/
        └── command-creation/
```

---

## 3. Component Overview

### Agents (`/agents/`)
**Purpose:** Define specialized agents in the 12-agent development team

**Key Characteristics:**
- Agent definitions written in Markdown
- Define role, authority, integration patterns, coordination requirements
- Optimized through Epic #291 for 50% average context reduction (2,631 lines saved)
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
- 7 commands delivering 42-61 min/day productivity gains

**Current Commands:**
- **Testing:** test-report, coverage-report
- **GitHub/Workflow:** create-issue, workflow-status, merge-coverage-prs
- **Epic/Project:** tackle-epic-issue, epic-complete

**Documentation:** [commands/README.md](./commands/README.md)

---

### Skills (`/skills/`)
**Purpose:** Progressive loading capabilities for context-efficient agent operations

**Key Characteristics:**
- Filesystem-based hierarchical loading (frontmatter → instructions → resources)
- Organized by category (coordination/, documentation/, github/, meta/)
- YAML frontmatter for skill discovery (~100 tokens)
- SKILL.md with comprehensive instructions (<500 lines recommended)
- Optional resources/ for templates, examples, documentation
- 8 skills achieving 50% average context reduction

**Current Skills by Category:**
- **Coordination (3):** working-directory-coordination, core-issue-focus, epic-completion
- **Documentation (1):** documentation-grounding
- **GitHub (1):** github-issue-creation
- **Meta (3):** agent-creation, skill-creation, command-creation

**Documentation:** [skills/README.md](./skills/README.md)

**Official Structure:** [`Docs/Archive/epic-291-skills-commands/Specs/official-skills-structure.md`](../Docs/Archive/epic-291-skills-commands/Specs/official-skills-structure.md)

---

## 4. Epic #291 Context

This directory structure was established through **Epic #291: Agent Skills & Slash Commands Integration** (COMPLETE - Archived 2025-10-27).

### Epic Achievements (All Objectives Met)
- ✅ Reduced agent definition context load by 50% average through skill references (2,631 lines saved)
- ✅ Enabled progressive skill loading (metadata → instructions → resources)
- ✅ Delivered 7 workflow automation commands with 42-61 min/day productivity gains
- ✅ Established 3 meta-skills for unlimited agent/skill/command creation
- ✅ Achieved 50-51% context reduction, 144-328% of session token savings targets

### Documentation Hierarchy
**Archive Location:** [Docs/Archive/epic-291-skills-commands/](../Docs/Archive/epic-291-skills-commands/README.md)

1. **Epic Summary:** `Docs/Archive/epic-291-skills-commands/README.md` (comprehensive achievements)
2. **Official Structure:** `Docs/Archive/epic-291-skills-commands/Specs/official-skills-structure.md` (authoritative)
3. **Skills Catalog:** `Docs/Archive/epic-291-skills-commands/Specs/skills-catalog.md` (all 8 skills)
4. **Commands Catalog:** `Docs/Archive/epic-291-skills-commands/Specs/commands-catalog.md` (all 7 commands)
5. **Implementation Plan:** `Docs/Archive/epic-291-skills-commands/Specs/implementation-iterations.md` (5 iterations)

---

## 5. Maintenance Guidelines

### Adding a New Agent
1. Review existing agents in `/agents/` for patterns
2. Use `agent-creation` meta-skill (`.claude/skills/meta/agent-creation/`)
3. Follow agent definition structure from CLAUDE.md
4. Reference appropriate skills for cross-cutting patterns
5. Update `/agents/README.md` with new agent entry
6. Test agent engagement through Claude orchestration

### Adding a New Command
1. Review existing commands in `/commands/` for patterns
2. Use `command-creation` meta-skill (`.claude/skills/meta/command-creation/`)
3. Follow command structure: YAML frontmatter + usage examples
4. Delegate to skills for implementation logic
5. Update `/commands/README.md` with new command entry
6. Test command execution and argument handling

### Adding a New Skill
1. Determine appropriate category (coordination/, documentation/, github/, meta/)
2. Create category directory with README.md if new category needed
3. Use `skill-creation` meta-skill (`.claude/skills/meta/skill-creation/`)
4. Follow official structure: YAML frontmatter in SKILL.md + optional resources/
5. Validate with at least 2 agents before integration
6. Update category README.md with new skill entry

### Adding a New Skill Category
1. Create directory: `.claude/skills/<category-name>/`
2. Create `README.md` following existing category patterns
3. Document category purpose, scope, and when to use
4. Update `/skills/README.md` with category entry
5. Follow official structure guidelines in archived epic documentation

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

**Epic #291 Archived Specifications:**
- [Epic Overview](../Docs/Archive/epic-291-skills-commands/README.md)
- [Official Skills Structure](../Docs/Archive/epic-291-skills-commands/Specs/official-skills-structure.md)
- [Skills Catalog](../Docs/Archive/epic-291-skills-commands/Specs/skills-catalog.md)
- [Commands Catalog](../Docs/Archive/epic-291-skills-commands/Specs/commands-catalog.md)
- [Implementation Iterations](../Docs/Archive/epic-291-skills-commands/Specs/implementation-iterations.md)

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
- [Agents Directory](./agents/README.md) - 12-agent team definitions (50% context reduction achieved)
- [Commands Directory](./commands/README.md) - 7 workflow automation commands (42-61 min/day savings)
- [Skills Directory](./skills/README.md) - 8 progressive loading skills across 4 categories

**By Skill Category:**
- [Coordination Skills](./skills/coordination/README.md) - 3 cross-cutting team patterns
- [Documentation Skills](./skills/documentation/README.md) - 1 documentation workflow
- [GitHub Skills](./skills/github/README.md) - 1 GitHub automation skill
- [Meta Skills](./skills/meta/README.md) - 3 agent/skill/command creation frameworks

**Epic #291 Quick Reference:**

**Current Skill Count:** 8 skills (COMPLETE)
- Coordination: 3 skills (working-directory-coordination, core-issue-focus, epic-completion)
- Documentation: 1 skill (documentation-grounding)
- GitHub: 1 skill (github-issue-creation)
- Meta: 3 skills (agent-creation, skill-creation, command-creation)

**Current Command Count:** 7 commands (COMPLETE)
- Testing: 2 commands (test-report, coverage-report)
- GitHub/Workflow: 3 commands (create-issue, workflow-status, merge-coverage-prs)
- Epic/Project: 2 commands (tackle-epic-issue, epic-complete)

**Epic #291 Status:** COMPLETE and ARCHIVED (2025-10-27)
- All 5 iterations delivered successfully
- Issues #286-#294 complete (excluding abandoned #295)
- Savings: 50-51% context reduction, 144-328% of session token savings targets

---

**Directory Status:** ✅ Production (Epic #291 Complete - Archived 2025-10-27)

**Maintenance:** Update this README when adding new agents, commands, skills, or categories using the meta-skills framework. Use the meta/ skills for consistent creation patterns.
