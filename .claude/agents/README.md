# Agent Definitions Directory

**Purpose:** Multi-agent development team definitions for zarichney-api orchestration
**Last Updated:** 2025-10-25
**Parent:** [`.claude/`](../README.md)

---

## 1. Purpose & Responsibility

* **What it is:** Collection of specialized agent definitions that comprise the zarichney-api multi-agent development team, orchestrated by Claude (codebase manager)

* **Key Objectives:**
  - **Specialized Expertise:** Each agent brings domain-specific knowledge and capabilities
  - **Clear Authority:** Defined file edit rights and implementation domains
  - **Team Coordination:** Explicit integration patterns and handoff protocols
  - **Context Efficiency:** Optimized through Epic #291 for 62% average context reduction via skill references

* **Why it exists:** The multi-agent architecture enables parallel work, specialized expertise, and scalable team operations. Each agent definition serves as a comprehensive prompt for Claude Code to assume that agent's role.

---

## 2. Current Agent Team (11 Agents)

### File-Editing Agents (Primary Content Creators)

**1. [code-changer.md](./code-changer.md)**
- **Role:** Feature implementation, bug fixes, refactoring
- **Authority:** All source code files (.cs, .ts, .js, .json, etc.)
- **Integration:** Coordinates with TestEngineer, DocumentationMaintainer, specialists

**2. [test-engineer.md](./test-engineer.md)**
- **Role:** Test coverage creation and quality assurance
- **Authority:** Test files (*Tests.cs, *.spec.ts, test configurations)
- **Integration:** Coverage excellence tracking, validation framework

**3. [documentation-maintainer.md](./documentation-maintainer.md)**
- **Role:** Standards compliance, README updates, project documentation
- **Authority:** Documentation files (README.md, *.md, docs)
- **Integration:** Coordinates with all agents for documentation updates

**4. [prompt-engineer.md](./prompt-engineer.md)**
- **Role:** AI prompt optimization, agent definitions, skills/commands creation
- **Authority:** AI prompt files (`.claude/agents/*.md`, `.github/prompts/*.md`, `Code/Zarichney.Server/Cookbook/Prompts/*.cs`)
- **Integration:** **EXCLUSIVE FILE EDIT RIGHTS** to `.claude/` directory

---

### Specialist Agents (Flexible Authority Framework)

**5. [frontend-specialist.md](./frontend-specialist.md)**
- **Role:** Angular 19, TypeScript, NgRx, Material Design expertise
- **Authority:** Frontend files (.ts, .html, .css, .scss) - implementation requests only
- **Integration:** Intent recognition (query vs. command), API coordination with BackendSpecialist

**6. [backend-specialist.md](./backend-specialist.md)**
- **Role:** .NET 8, C#, EF Core, ASP.NET Core expertise
- **Authority:** Backend files (.cs), configurations, migrations - implementation requests only
- **Integration:** Intent recognition (query vs. command), API design coordination

**7. [security-auditor.md](./security-auditor.md)**
- **Role:** Security hardening, vulnerability assessment, threat analysis
- **Authority:** Security configurations, vulnerability fixes - implementation requests only
- **Integration:** OWASP compliance, authentication implementation

**8. [workflow-engineer.md](./workflow-engineer.md)**
- **Role:** GitHub Actions, CI/CD automation, pipeline optimization
- **Authority:** Workflows (`.github/workflows/*`), scripts (`Scripts/*`), build configs - implementation requests only
- **Integration:** Coverage Excellence Merge Orchestrator, CI/CD monitoring

**9. [bug-investigator.md](./bug-investigator.md)**
- **Role:** Root cause analysis, diagnostic reporting, systematic debugging
- **Authority:** Working directory artifacts for analysis (advisory mode)
- **Integration:** Performance bottlenecks, error interpretation, reproduction analysis

**10. [architectural-analyst.md](./architectural-analyst.md)**
- **Role:** System design decisions, architecture review, technical debt assessment
- **Authority:** Working directory artifacts for recommendations (advisory mode)
- **Integration:** Design patterns evaluation, structural assessment

**11. [compliance-officer.md](./compliance-officer.md)**
- **Role:** Pre-PR validation, standards verification, comprehensive quality gates
- **Authority:** Validation and review (no direct file editing)
- **Integration:** Dual verification partnership with Claude before PR creation

---

## 3. Agent Definition Structure

### Required Sections

Each agent definition follows this structure:

```markdown
# Agent Name

[Brief identity and role description]

## Purpose & Core Mission

[What problems this agent solves, when to engage]

## Authority & File Edit Rights

[Exact file patterns agent can modify]
[Intent recognition for specialists (query vs. command)]

## Team Integration Patterns

[How agent coordinates with other team members]
[Handoff protocols and dependencies]

## Skills References (Epic #291)

[Cross-cutting patterns delegated to skills]
- working-directory-coordination (mandatory)
- documentation-grounding (mandatory)
- domain-specific skills

## Quality Gates & Standards

[Standards adherence requirements]
[Testing requirements]
[Documentation requirements]
```

---

## 4. How to Add a New Agent

### Before Creating

**Prerequisites:**
1. Identify clear need (new domain expertise or specialized capability)
2. Define non-overlapping authority with existing agents
3. Determine integration patterns with team
4. Review existing agents to avoid duplication

### Creation Process

**1. Use agent-creation Meta-Skill (Available Iteration 2.1):**
```
Location: .claude/skills/meta/agent-creation/
Purpose: Standardized agent creation framework
Benefit: 50% faster creation with 100% consistency
```

**2. Manual Creation (Before Iteration 2.1):**
```bash
# Create agent file
touch .claude/agents/new-agent-name.md

# Follow structure from existing agents
# Reference: architectural-analyst.md (good minimal example)
# Reference: frontend-specialist.md (specialist with flexible authority)
# Reference: test-engineer.md (primary file-editing agent)
```

**3. Required Content:**
- Clear role definition and mission
- Explicit file edit authority (or advisory-only)
- Team integration patterns
- Skill references (at minimum: working-directory-coordination, documentation-grounding)
- Quality gate requirements

**4. Context Optimization:**
- Target: 130-240 lines core definition
- Extract cross-cutting patterns to skills
- Reference detailed workflows in skills, not embedded
- Follow Epic #291 progressive loading patterns

**5. Validation:**
- Test agent engagement through Claude orchestration
- Verify skill loading works correctly
- Confirm coordination with existing agents
- Validate authority boundaries respected

**6. Documentation:**
- Update this README.md with new agent entry
- Add to CLAUDE.md multi-agent team section
- Document in Epic #291 if part of refactoring

---

## 5. Agent Categories Explained

### File-Editing Agents
**Characteristics:**
- Direct authority to create/modify files
- Primary content creators
- Execute specific implementation tasks
- Examples: CodeChanger, TestEngineer, DocumentationMaintainer

**When to Create:**
- Need direct file modification capability
- Clear file pattern authority (*.cs, *.md, etc.)
- Frequent content creation/modification tasks

### Specialist Agents (Flexible Authority)
**Characteristics:**
- Domain expertise (backend, frontend, security, workflows)
- Intent recognition: Query (advisory) vs. Command (implementation)
- Direct file modification for implementation requests only
- Examples: BackendSpecialist, FrontendSpecialist, WorkflowEngineer

**When to Create:**
- Deep technical domain expertise needed
- Both analysis and implementation capabilities required
- Can distinguish between advisory and implementation modes

### Advisory Agents
**Characteristics:**
- Working directory artifacts only (no direct file editing)
- Analysis, recommendations, guidance
- Examples: BugInvestigator, ArchitecturalAnalyst

**When to Create:**
- Analysis and diagnostic capabilities needed
- Recommendations inform other agents' work
- No direct implementation required

### Quality Gate Agents
**Characteristics:**
- Validation and review focus
- No direct file editing
- Final approval authority
- Example: ComplianceOfficer

**When to Create:**
- Quality assurance and validation needed
- Pre-PR or pre-deployment gates
- Standards enforcement required

---

## 6. Epic #291 Integration

### Agent Refactoring (Iteration 4)

All 11 agents will be refactored to achieve 62% average context reduction:

**Refactoring Pattern:**
1. Extract cross-cutting patterns to skills
2. Add skill references with 2-3 line summaries
3. Maintain core identity and authority (130-240 lines)
4. Validate effectiveness preservation

**Timeline:**
- Iteration 4.1: High-impact agents (5 agents)
- Iteration 4.2: Medium-impact agents (3 agents)
- Iteration 4.3: Lower-impact agents (3 agents)

**Expected Savings:**
- Total: 3,230 lines eliminated
- Per agent: ~300 lines average reduction
- Context: ~25,840 tokens saved across team

### Skill Integration

**Mandatory Skills (All Agents):**
- `working-directory-coordination` - Team communication protocols
- `documentation-grounding` - Systematic standards loading

**Domain Skills (Specialist Agents):**
- `flexible-authority-management` - Intent recognition framework
- `core-issue-focus` - Mission discipline for primary agents
- Domain-specific technical skills (future iterations)

---

## 7. Naming Conventions

**File Naming:**
- Format: `lowercase-with-hyphens.md`
- Examples: `backend-specialist.md`, `test-engineer.md`
- Avoid: Underscores, CamelCase, spaces

**Agent Identity Naming:**
- Match filename: BackendSpecialist (in agent definition) = backend-specialist.md (filename)
- Consistent capitalization in CLAUDE.md references
- Use in skills metadata `agents` field

---

## 8. Common Patterns

### Authority Declaration
```markdown
## Authority & File Edit Rights

**Direct Modification Authority:**
- `*.cs` files in backend codebase
- `config/*.json` configuration files
- `migrations/*` database migrations

**Flexible Authority (Intent Recognition):**
- Query Intent → Working directory analysis
- Command Intent → Direct file modifications

**Coordination Required:**
- Cross-domain changes require specialist coordination
- API contract changes coordinate with FrontendSpecialist
```

### Skills Reference Pattern
```markdown
## [Capability] Implementation
**SKILL REFERENCE**: `.claude/skills/category/skill-name/`

[2-3 line summary of skill purpose]

Key Workflow: [Step 1 | Step 2 | Step 3]

[See skill for complete instructions]
```

### Team Integration Pattern
```markdown
## Team Integration

**Coordinates With:**
- TestEngineer: Test coverage for implementations
- DocumentationMaintainer: README updates for API changes
- SecurityAuditor: Security review for authentication code

**Handoff Protocol:**
[Specific artifacts and communication patterns]
```

---

## 9. Troubleshooting

**Problem:** Agent not loading correctly
- **Check:** File exists in `.claude/agents/` directory
- **Check:** Filename matches `lowercase-with-hyphens.md` pattern
- **Check:** Agent definition has clear role and authority sections

**Problem:** Agent authority conflicts with other agents
- **Solution:** Review all agent definitions for overlapping file edit rights
- **Solution:** Use intent recognition for specialists (query vs. command)
- **Solution:** Define clear handoff protocols

**Problem:** Agent context too large
- **Solution:** Extract cross-cutting patterns to skills
- **Solution:** Reference detailed workflows in skills, not embedded
- **Solution:** Target 130-240 lines core definition

**Problem:** Agent not receiving skill updates
- **Solution:** Ensure skill references are current
- **Solution:** Validate skill metadata includes agent in `agents` field
- **Solution:** Test progressive loading of skills

---

## 10. Related Documentation

**Orchestration:**
- [CLAUDE.md](../../CLAUDE.md) - Codebase manager orchestration guide
- [Epic #291 Specifications](../../Docs/Specs/epic-291-skills-commands/README.md)

**Skills:**
- [Skills Directory](../skills/README.md) - Progressive loading capabilities
- [agent-creation Meta-Skill](../skills/meta/agent-creation/) - When available (Iteration 2.1)

**Standards:**
- [Documentation Standards](../../Docs/Standards/DocumentationStandards.md)
- [Task Management Standards](../../Docs/Standards/TaskManagementStandards.md)

---

## 11. Quick Reference

**Current Agent Count:** 11 agents

**By Category:**
- File-Editing: 4 agents (CodeChanger, TestEngineer, DocumentationMaintainer, PromptEngineer)
- Specialists: 4 agents (Frontend, Backend, Security, Workflow)
- Advisory: 2 agents (BugInvestigator, ArchitecturalAnalyst)
- Quality Gate: 1 agent (ComplianceOfficer)

**Epic #291 Status:**
- Iteration 1: Foundation (current)
- Iteration 4: Agent refactoring with skill references
- Target: 62% average context reduction

---

**Directory Status:** ✅ Active Development (Epic #291)

**Next Updates:** Iteration 4 (Agent Refactoring) will add skill references to all 11 agents
