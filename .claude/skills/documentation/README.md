# Documentation Skills Category

**Purpose:** Documentation and context management skills for stateless AI operation
**Last Updated:** 2025-10-25
**Parent:** [Skills Directory](../README.md)

---

## 1. Purpose & Responsibility

**What it is:** Collection of skills related to documentation creation, maintenance, and systematic context loading for stateless AI agents.

**Key Objectives:**
- **Stateless Operation Support:** Enable AI agents to operate effectively without prior session memory
- **Documentation Excellence:** Maintain comprehensive, accurate, and navigable documentation
- **Context Loading Efficiency:** Optimize systematic loading of project standards and module context
- **Knowledge Preservation:** Ensure self-contained knowledge at every directory level

**Why it exists:** Stateless AI agents have no inherent memory of project structure, coding standards, or architectural decisions. Documentation skills transform context-blind agents into fully-informed contributors through systematic context loading and documentation maintenance protocols.

---

## 2. Current Skills

### [documentation-grounding](./documentation-grounding/)
**Purpose:** Systematic framework for loading project standards, module READMEs, and architectural patterns before agent work begins

**When to Use:**
- Starting any agent engagement
- Switching between modules or domains
- Before modifying code or documentation
- After receiving context package from Claude

**Key Features:**
- 3-Phase systematic loading workflow (Standards → Architecture → Domain-Specific)
- Agent-specific grounding patterns for all 11 agents
- Progressive loading integration (~100 tokens metadata → ~2,800 tokens instructions → variable resources)
- Context window optimization strategies

**Resources:**
- **Templates:** Standards loading checklist, module context template
- **Examples:** BackendSpecialist, TestEngineer, DocumentationMaintainer grounding workflows
- **Documentation:** Optimization guide, selective loading patterns

**Adoption:** Mandatory for all 11 agents
**Context Savings:** ~400 lines across agent definitions (~3,200 tokens total)

---

## 3. When to Create a Skill in This Category

### Appropriate for Documentation Category

**Documentation Management:**
- README creation/maintenance workflows
- Diagram generation and embedding standards
- Cross-reference validation automation
- Documentation quality assurance

**Context Loading:**
- Module-specific context ingestion patterns
- Dependency documentation analysis
- Historical context preservation
- Interface contract analysis frameworks

**Knowledge Organization:**
- Documentation navigation strategies
- Self-contained knowledge validation
- Parent-child linking automation
- Documentation pruning workflows

### NOT Appropriate (Belongs in Other Categories)

**Coordination Category:**
- Team communication protocols (working-directory-coordination)
- Multi-agent orchestration
- Artifact sharing workflows

**Technical Category (Future):**
- Code analysis patterns
- Performance optimization workflows
- Database migration strategies

**Meta Category (Future):**
- Agent creation workflows
- Skill creation frameworks
- Command creation patterns

---

## 4. Maintenance Notes

### Adding New Documentation Skills

**Creation Process:**
1. Validate need: Is this documentation/context-related?
2. Verify no duplication with existing skills
3. Follow official skills structure (YAML frontmatter in SKILL.md)
4. Create progressive disclosure resources (templates, examples, documentation)
5. Test with at least 2 agents
6. Update this README with new skill entry

**Required Structure:**
```
.claude/skills/documentation/new-skill-name/
├── SKILL.md                 # YAML frontmatter + instructions (<500 lines)
└── resources/
    ├── templates/           # Reusable formats
    ├── examples/            # Reference implementations
    └── documentation/       # Deep dives
```

### Documentation Category Standards

**Skill Naming:**
- Use descriptive, hyphenated names: `documentation-grounding`, `readme-validation`
- Avoid generic terms: not "helper", "utils", "tools"

**Description Requirements:**
- Must include WHAT the skill does
- Must include WHEN to use it
- Max 1024 characters
- Optimized for discovery among 100+ skills

**Progressive Loading:**
- Frontmatter: ~100 tokens
- SKILL.md body: 2,000-3,000 tokens target, <5,000 tokens maximum
- Keep under 500 lines recommended
- Resources: On-demand variable loading

---

## 5. Related Documentation

### Official Specifications
- [Official Skills Structure](../../Docs/Specs/epic-291-skills-commands/official-skills-structure.md) - AUTHORITATIVE
- [Skills Catalog](../../Docs/Specs/epic-291-skills-commands/skills-catalog.md) - All 8 Epic #291 skills
- [Implementation Iterations](../../Docs/Specs/epic-291-skills-commands/implementation-iterations.md)

### Standards
- [Documentation Standards](../../Docs/Standards/DocumentationStandards.md) - README structure and philosophy
- [Diagramming Standards](../../Docs/Standards/DiagrammingStandards.md) - Mermaid diagram conventions
- [Claude Code Official Docs](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/)

### Related Skills
- [working-directory-coordination](../coordination/working-directory-coordination/) - Team communication protocols

### Parent Documentation
- [Skills Directory README](../README.md) - Skills architecture overview
- [.claude/ README](../../.claude/README.md) - Root directory context

---

## 6. Future Skills (Potential)

### readme-validation
**Purpose:** Automated validation of README.md files against DocumentationStandards.md template
**Triggers:** Pre-commit hook, PR creation, documentation updates
**Benefits:** Ensures consistency, validates linking, checks 8-section structure

### diagram-generation
**Purpose:** Systematic generation of Mermaid diagrams for common architectural patterns
**Triggers:** Creating new modules, documenting workflows, architectural changes
**Benefits:** Visual consistency, reduces diagram creation time, follows DiagrammingStandards.md

### context-ingestion
**Purpose:** Automated extraction of key context from large codebases for efficient loading
**Triggers:** Starting work in unfamiliar modules, large refactoring analysis
**Benefits:** Faster context acquisition, optimized token usage

---

## 7. Quick Reference

**Current Skill Count:** 1 skill (documentation-grounding)

**By Type:**
- Context Loading: 1 skill (documentation-grounding)
- Documentation Maintenance: 0 skills (future)
- Validation: 0 skills (future)

**Epic #291 Integration:**
- Iteration 1.2: documentation-grounding created (Issue #310)
- Future iterations: Additional documentation skills as needed

**Adoption:** Mandatory for all 11 agents

**Context Savings:**
- documentation-grounding: ~3,200 tokens across agent definitions
- Progressive loading: 98% metadata savings, 85% instruction savings

---

**Category Status:** ✅ Active Development (Epic #291 Iteration 1)

**Next Updates:**
- Issue #310: documentation-grounding skill complete
- Future: Additional documentation skills as patterns emerge
