# Coordination Skills Category

**Purpose:** Cross-cutting team communication and coordination patterns
**Last Updated:** 2025-10-25
**Parent:** [`/skills/`](../README.md)

---

## 1. Purpose & Responsibility

* **What it is:** Collection of skills that standardize multi-agent team coordination, communication protocols, and collaborative workflows

* **Key Objectives:**
  - **Team Awareness:** Ensure all agents maintain visibility into team activities
  - **Context Continuity:** Preserve context across agent handoffs and engagements
  - **Communication Standards:** Standardize artifact reporting and integration patterns
  - **Workflow Discipline:** Enforce mission focus and scope management

* **Why this category exists:** Multi-agent AI systems operate statelessly - agents have no inherent awareness of each other's work unless explicitly communicated. Coordination skills provide mandatory patterns that enable effective orchestration and prevent communication gaps, context loss, and duplicated work.

---

## 2. Current Skills

### [working-directory-coordination](./working-directory-coordination/)

**Status:** ✅ Production-ready (Issue #311 complete)
**Agents:** ALL (mandatory for all 11 agents)
**Priority:** P0 - Foundation for all team coordination

**Purpose:**
Standardize working directory usage and team communication protocols across all agents through mandatory artifact discovery, immediate reporting, and integration workflows.

**When to Use:**
- MANDATORY: Before starting ANY task (artifact discovery)
- MANDATORY: When creating/updating ANY working directory file
- MANDATORY: When building upon other agents' artifacts

**Key Features:**
- 4-step mandatory workflow (discovery, reporting, integration, compliance)
- Standardized communication templates
- Progressive handoff examples
- Troubleshooting guidance

**Context Savings:**
- Eliminates ~450 lines across 11 agents
- ~3,600 tokens saved through skill reference
- Mandatory protocol enforcement by Claude

**Integration Validation:**
- ✅ Tested with TestEngineer (PASS WITH EXCELLENCE)
- ✅ Tested with DocumentationMaintainer (pending)
- ✅ Progressive loading validated
- ✅ Resource accessibility confirmed

---

### [core-issue-focus](./core-issue-focus/)

**Status:** ✅ Production-ready (Issue #310 complete)
**Agents:** TestEngineer, PromptEngineer, CodeChanger, BackendSpecialist, FrontendSpecialist, WorkflowEngineer (6 primary agents)
**Priority:** P1 - Mission discipline foundation

**Purpose:**
Mission discipline framework preventing scope creep during agent implementations through surgical focus on specific blocking technical problems. Provides 4-step workflow for maintaining mission discipline throughout implementation lifecycle.

**When to Use:**
- When receiving complex missions from Claude with scope expansion risk
- During implementations involving multiple files or components
- When tempted by improvement opportunities not directly related to core issue
- Before expanding scope beyond original technical problem definition

**Key Features:**
- 4-step mission discipline workflow (Identify, Define, Detect, Validate)
- Agent-specific patterns for 6 primary agents
- 3 ready-to-use templates (core issue analysis, scope boundary definition, success criteria validation)
- 3 comprehensive examples (API bug fix, feature implementation, targeted refactoring)
- 2 deep-dive documentation files (mission drift patterns catalog, validation checkpoints guide)
- Integration with CLAUDE.md CORE ISSUE FIRST PROTOCOL (MANDATORY)

**Context Savings:**
- Eliminates ~200 lines across 6 agents
- ~1,600 tokens saved through skill reference
- Progressive loading: frontmatter (~100 tokens) → SKILL.md (~2,500 tokens) → resources (on-demand)

**Integration Validation:**
- ✅ YAML frontmatter validated per official specification
- ✅ SKILL.md under 500 lines (optimized for progressive loading)
- ✅ 8 resource files created (templates, examples, documentation)
- ✅ Integration with CLAUDE.md protocols confirmed
- ✅ Agent-specific patterns for all 6 target agents

---

### Future Coordination Skills (Planned)

**flexible-authority-management** *(Issue #309 - Iteration 1.3)*
**Purpose:** Intent recognition framework for specialist agents
**Agents:** BackendSpecialist, FrontendSpecialist, SecurityAuditor, WorkflowEngineer, BugInvestigator
**Priority:** P1

**Capabilities:**
- Query intent vs. command intent recognition
- Authority boundary validation
- Response mode selection (analysis vs. implementation)
- Domain compliance verification

**Benefits:**
- Specialists distinguish analysis from implementation requests
- Flexible authority within domain boundaries
- Clear escalation patterns for cross-domain work

---

## 3. When to Create a Coordination Skill

### Create Coordination Skill When:

**Cross-Cutting Pattern (3+ Agents):**
- Used by multiple agents across different domains
- Not specific to backend, frontend, testing, or any single specialty
- Benefits from consistent application across team

**Communication/Collaboration Protocol:**
- Standardizes how agents communicate with each other
- Defines artifact management patterns
- Establishes handoff workflows

**Stateless Operation Support:**
- Helps agents operate effectively without memory
- Preserves context across engagements
- Enables Claude's orchestration through visibility

**Team Workflow Enhancement:**
- Improves multi-agent coordination
- Reduces communication gaps
- Prevents duplicated work

### DON'T Create Coordination Skill When:

**Domain-Specific:**
- Only used by one specialist (backend, frontend, etc.)
- Better suited for `technical/` category

**Implementation Detail:**
- Specific algorithm or technical pattern
- Not about team coordination

**Single-Agent Pattern:**
- Used by only one or two agents
- Better embedded in agent definition

---

## 4. Coordination Skill Patterns

### Common Characteristics

**Mandatory Application:**
- Often required for all agents (e.g., working-directory-coordination)
- No exceptions to protocols
- Enforcement by Claude orchestration

**Standardized Formats:**
- Templates for consistent communication
- Structured reporting patterns
- Clear placeholder syntax

**Integration Focus:**
- How agents work together
- Handoff protocols between specialists
- Context preservation mechanisms

**Educational Resources:**
- Examples demonstrating multi-agent coordination
- Troubleshooting common collaboration failures
- Best practices for team awareness

### Resource Organization Patterns

**Templates:**
- Communication formats (discovery, reporting, integration)
- Standardized artifact structures
- Workflow checklists

**Examples:**
- Multi-agent coordination scenarios
- Progressive handoff demonstrations
- Cross-specialist collaboration patterns

**Documentation:**
- Protocol philosophy and rationale
- Communication failure troubleshooting
- Integration with quality gates

---

## 5. Integration with Other Categories

### Coordination + Technical
**Pattern:** Coordination skills provide the "how to communicate," technical skills provide the "what to do"

**Example:**
- **Coordination:** working-directory-coordination (how to report analysis)
- **Technical:** performance-profiling (what analysis to perform)
- **Together:** Agent performs technical analysis AND reports results using standard format

### Coordination + Meta
**Pattern:** Meta-skills use coordination patterns when creating new agents/skills

**Example:**
- **Coordination:** working-directory-coordination (artifact reporting)
- **Meta:** agent-creation (how to create agents)
- **Together:** Agent creation process reports progress using standard communication protocols

### Coordination + Workflow
**Pattern:** Workflow skills orchestrate operations that leverage coordination protocols

**Example:**
- **Coordination:** working-directory-coordination (context integration)
- **Workflow:** github-issue-creation (issue creation automation)
- **Together:** Issue creation workflow integrates existing context using standard discovery patterns

---

## 6. Maintenance Notes

### Adding Skills to This Category

**Prerequisites:**
1. Validate cross-cutting pattern (3+ agents benefit)
2. Define clear communication protocols
3. Create standardized templates
4. Provide multi-agent examples

**Quality Standards:**
- Mandatory where applicable (communicate exceptions clearly)
- Enforcement mechanisms documented
- Integration with Claude orchestration explained
- Troubleshooting guidance comprehensive

### Updating Existing Skills

**When to Update:**
- New agents join team and need protocol adoption
- Integration patterns evolve
- Communication gaps identified
- Quality gate requirements change

**Update Process:**
1. Review SKILL.md and identify sections to modify
2. Update resources (templates, examples, documentation)
3. Test with affected agents
4. Update category README.md if scope changes
5. Document update in skill version history (if applicable)

### Deprecating Skills

**When to Deprecate:**
- Pattern no longer relevant (team structure changed)
- Better alternative skill created
- Consolidated into more comprehensive skill

**Deprecation Process:**
1. Add deprecation notice to SKILL.md
2. Reference replacement skill
3. Maintain for one iteration to allow transition
4. Archive to `deprecated/` subdirectory
5. Update category README.md

---

## 7. Common Anti-Patterns

### ❌ Overly Specific Coordination

**Problem:** Coordination skill targets single use case or agent pair
**Example:** "backend-frontend-handoff" skill only for BackendSpecialist → FrontendSpecialist
**Solution:** Generalize to "api-contract-coordination" usable by all agents involved in API work

### ❌ Implementation Details in Coordination

**Problem:** Coordination skill includes technical implementation instead of just communication protocol
**Example:** Including SQL query patterns in working-directory-coordination
**Solution:** Keep coordination focused on "how to communicate about work," not "how to do the work"

### ❌ Optional Protocols Disguised as Coordination

**Problem:** Skill presents as coordination but protocols are actually optional
**Example:** "Consider reporting your work to the working directory if you feel like it"
**Solution:** Make protocols mandatory with clear enforcement, or don't create the skill

### ❌ Missing Integration Examples

**Problem:** Skill describes protocols but doesn't show multi-agent collaboration
**Example:** Templates exist but no realistic multi-agent scenarios
**Solution:** Provide 2-3 examples demonstrating actual agent handoffs and coordination

---

## 8. Epic #291 Context

### Coordination Skills Timeline

**Iteration 1.1 (Issue #311):** ✅ working-directory-coordination
- **Status:** Complete and validated
- **Agents:** ALL (mandatory)
- **Savings:** ~3,600 tokens across 11 agents

**Iteration 1.2 (Issue #310):** ✅ core-issue-focus
- **Status:** Complete and validated
- **Agents:** 6 primary agents (TestEngineer, PromptEngineer, CodeChanger, BackendSpecialist, FrontendSpecialist, WorkflowEngineer)
- **Savings:** ~1,600 tokens

**Iteration 1.3 (Issue #309):** flexible-authority-management
- **Status:** Pending
- **Agents:** 5 specialist agents
- **Savings:** ~4,800 tokens

**Total Coordination Category Impact:**
- 3 skills eliminating ~1,250 lines
- ~10,000 tokens saved
- Foundation for Epic #291 success

### Integration with Iteration 4 (Agent Refactoring)

All 11 agents will reference coordination skills:
- Extract embedded protocols to skill references
- 2-3 line summaries in agent definitions
- Complete protocols available on-demand
- ~1,250 lines eliminated from agent definitions

---

## 9. Quality Validation

### Skill Quality Checklist

**Context Efficiency:**
- ✅ Frontmatter ~100 tokens
- ✅ SKILL.md 2,000-5,000 tokens
- ✅ Resources organized appropriately
- ✅ Progressive loading validated

**Protocol Clarity:**
- ✅ Mandatory vs. optional clearly specified
- ✅ Enforcement mechanisms documented
- ✅ Standardized templates provided
- ✅ Examples demonstrate realistic usage

**Multi-Agent Validation:**
- ✅ Tested with at least 2 agents
- ✅ Cross-agent coordination demonstrated
- ✅ Handoff patterns validated
- ✅ Context continuity confirmed

**Integration Verification:**
- ✅ Claude orchestration compatibility
- ✅ Quality gate integration
- ✅ Working directory communication effective
- ✅ No agent effectiveness regression

---

## 10. Related Documentation

**Epic #291 Specifications:**
- [Official Skills Structure](../../../Docs/Specs/epic-291-skills-commands/official-skills-structure.md)
- [Skills Catalog](../../../Docs/Specs/epic-291-skills-commands/skills-catalog.md)
- [Implementation Iterations](../../../Docs/Specs/epic-291-skills-commands/implementation-iterations.md)

**Parent Documentation:**
- [Skills Directory](../README.md)
- [.claude Directory](../../README.md)

**Orchestration:**
- [CLAUDE.md](../../../CLAUDE.md) - Multi-agent orchestration guide

**Standards:**
- [Documentation Standards](../../../Docs/Standards/DocumentationStandards.md)
- [Task Management Standards](../../../Docs/Standards/TaskManagementStandards.md)

---

## 11. Quick Reference

**Current Skill Count:** 2 skills (working-directory-coordination, core-issue-focus)

**Planned Skills:** 1 additional (flexible-authority-management)

**Total Impact:** ~10,000 tokens saved, 3 skills, foundation for all multi-agent coordination

**Mandatory Skills:** 1 (working-directory-coordination for ALL agents)
**Recommended Skills:** 1 (core-issue-focus for 6 primary agents)

**Epic #291 Status:** Iteration 1 in progress

---

**Category Status:** ✅ Active Development (Epic #291 Iteration 1)

**Next Updates:**
- Issue #310: Add core-issue-focus skill
- Issue #309: Add flexible-authority-management skill
- Iteration 4: All 11 agents reference coordination skills
