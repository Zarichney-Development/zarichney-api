# Epic #291 - Issue #310 Execution Plan

**Issue:** #310 - Iteration 1.2: Core Skills - Documentation Grounding & Core Issue Focus
**Epic:** #291 - Agent Skills & Slash Commands Integration
**Iteration:** 1 - Foundation
**Branch:** `section/iteration-1` (continuing from Issue #311)
**Created:** 2025-10-25

---

## 1. Mission Objective

Create two foundational skills that establish systematic standards loading and mission discipline across all agents:
1. **documentation-grounding** - 3-phase systematic context loading framework (mandatory for all 11 agents)
2. **core-issue-focus** - Mission discipline framework preventing scope creep (6 primary agents)

**Expected Outcome:** ~4,800 tokens saved (~3,200 from documentation-grounding + ~1,600 from core-issue-focus) through elimination of ~600 lines redundancy across affected agents.

---

## 2. Dependencies

**Depends On:**
- ✅ Issue #311 complete (working-directory-coordination skill operational)
- ✅ Epic branch exists: `epic/skills-commands-291`
- ✅ Section branch exists: `section/iteration-1`
- ✅ Official skills structure established in `Docs/Specs/epic-291-skills-commands/official-skills-structure.md`
- ✅ .claude/ directory documentation standards established

**Blocks:**
- Issue #309 (Iteration 1.3: GitHub Workflow Skill - Issue Creation)
- Issue #297 (Iteration 4.2: Medium-Impact Agents Refactoring)

---

## 3. Execution Strategy

### Approach
**Iterative Agent Engagement:** PromptEngineer will create both skills sequentially on `section/iteration-1` branch with individual commits per major deliverable.

### Agent Assignment
- **Primary:** PromptEngineer (skill creation, SKILL.md authoring, resource organization)
- **Validation:** TestEngineer (integration testing), DocumentationMaintainer (standards validation)
- **Quality:** ComplianceOfficer (section-level validation after completion)

### Branch Strategy
Continue using `section/iteration-1` branch (created for Issue #311):
- All commits for Issue #310 go on this branch
- Individual commits per skill and subtask
- Section PR created after Issues #311, #310, #309, #308 all complete

---

## 4. Detailed Task Breakdown

### Skill 1: documentation-grounding (Days 1-3)

**Location:** `.claude/skills/documentation/documentation-grounding/`

**Subtasks:**

#### 4.1.1 Create Skill Directory Structure
```bash
.claude/skills/documentation/documentation-grounding/
├── SKILL.md (with YAML frontmatter)
└── resources/
    ├── templates/
    ├── examples/
    └── documentation/
```

**YAML Frontmatter Required:**
```yaml
---
name: documentation-grounding
description: Systematic framework for loading project standards, module READMEs, and architectural patterns before agent work begins. Use when starting any agent engagement, switching between modules, or before modifying code or documentation.
---
```

#### 4.1.2 Create SKILL.md (~3,000 tokens, <500 lines)
**Required Sections:**
- Purpose & Mission
- When to Use (mandatory triggers)
- 3-Phase Systematic Loading Workflow:
  - Phase 1: Standards Mastery (CodingStandards, TestingStandards, DocumentationStandards, TaskManagementStandards)
  - Phase 2: Project Architecture Context (root README, module hierarchy, diagrams, dependencies)
  - Phase 3: Domain-Specific Context (module deep-dive, interface contracts, local conventions)
- Agent-Specific Grounding Patterns (all 11 agents)
- Progressive Loading Integration
- Resources References
- Integration with Orchestration

#### 4.1.3 Create Templates (resources/templates/)
1. **standards-loading-checklist.md**
   - Systematic loading workflow with checkboxes
   - Phase 1-3 progression
   - Completion validation

2. **module-context-template.md**
   - Module README analysis structure
   - Interface contract discovery
   - Local convention recognition
   - Integration point mapping

#### 4.1.4 Create Examples (resources/examples/)
1. **backend-specialist-grounding.md**
   - Complete grounding workflow for BackendSpecialist
   - API module context loading
   - Database schema understanding
   - Service layer patterns

2. **test-engineer-grounding.md**
   - Testing-specific grounding pattern
   - TestingStandards.md focus
   - Test project structure understanding
   - Coverage requirements integration

3. **documentation-maintainer-grounding.md**
   - Documentation grounding pattern
   - DocumentationStandards.md mastery
   - README hierarchy navigation
   - Cross-reference validation

#### 4.1.5 Create Documentation (resources/documentation/)
1. **grounding-optimization-guide.md**
   - Context window optimization strategies
   - Selective loading based on task type
   - Performance best practices
   - Token budget management

2. **selective-loading-patterns.md**
   - Task-based grounding strategies
   - Full vs. partial grounding triggers
   - Quick reference patterns
   - Emergency grounding shortcuts

**Commit:** `feat: create documentation-grounding skill (#310)`

---

### Skill 2: core-issue-focus (Days 4-5)

**Location:** `.claude/skills/coordination/core-issue-focus/`

**Subtasks:**

#### 4.2.1 Create Skill Directory Structure
```bash
.claude/skills/coordination/core-issue-focus/
├── SKILL.md (with YAML frontmatter)
└── resources/
    ├── templates/
    ├── examples/
    └── documentation/
```

**YAML Frontmatter Required:**
```yaml
---
name: core-issue-focus
description: Mission discipline framework preventing scope creep and ensuring surgical focus on specific blocking technical problems. Use when receiving complex missions, during implementations with scope expansion risk, or before expanding beyond original technical problem.
---
```

#### 4.2.2 Create SKILL.md (~2,000 tokens, <500 lines)
**Required Sections:**
- Purpose & Mission Discipline Philosophy
- When to Use (scope expansion risk scenarios)
- 4-Step Mission Discipline Workflow:
  - Step 1: Identify Core Issue First (problem, minimum fix, success criteria)
  - Step 2: Surgical Scope Definition (focus, defer secondary, boundaries)
  - Step 3: Mission Drift Detection (expansion signals, validation, escalation)
  - Step 4: Core Issue Validation (testing, success verification, future work)
- Target Agent Patterns (6 primary agents)
- Integration with Context Packages
- Resources References
- Mission Drift Recovery Protocols

#### 4.2.3 Create Templates (resources/templates/)
1. **core-issue-analysis-template.md**
   - Problem identification structure
   - Blocking factor specification
   - Minimum viable fix definition
   - Success criteria specification

2. **scope-boundary-definition.md**
   - Core issue scope documentation
   - Secondary improvements list (deferred)
   - Integration boundaries
   - Out-of-scope clarification

3. **success-criteria-validation.md**
   - Testable outcome specification
   - Acceptance criteria definition
   - Validation approach
   - Regression prevention checks

#### 4.2.4 Create Examples (resources/examples/)
1. **api-bug-fix-example.md**
   - Surgical bug resolution scenario
   - Core issue: specific API endpoint failure
   - Scope discipline: fix bug only, defer optimization
   - Validation: endpoint works correctly

2. **feature-implementation-focused.md**
   - Feature with clear boundaries
   - Core issue: specific user need
   - Scope discipline: MVP implementation, defer enhancements
   - Validation: user need satisfied

3. **refactoring-scoped.md**
   - Refactoring with mission discipline
   - Core issue: specific code smell
   - Scope discipline: targeted refactor, no wholesale rewrite
   - Validation: smell eliminated, tests pass

#### 4.2.5 Create Documentation (resources/documentation/)
1. **mission-drift-patterns.md**
   - Common scope expansion triggers
   - Warning signs and detection
   - Prevention strategies
   - Recovery protocols

2. **validation-checkpoints.md**
   - When to verify core issue status
   - Testing strategies for core functionality
   - Success criteria validation approaches
   - Escalation triggers

**Commit:** `feat: create core-issue-focus skill (#310)`

---

## 5. Integration Testing (Day 6)

### 5.1 documentation-grounding Skill Validation

**Test with 2 Agents:**

1. **TestEngineer Integration:**
   - Load documentation-grounding skill
   - Execute Phase 1-3 systematic loading
   - Validate TestingStandards.md mastery
   - Verify module context understanding
   - Confirm progressive loading works

2. **DocumentationMaintainer Integration:**
   - Load documentation-grounding skill
   - Execute grounding workflow
   - Validate DocumentationStandards.md focus
   - Verify README hierarchy navigation
   - Confirm resource accessibility

**Acceptance Criteria:**
- ✅ Both agents successfully load and use skill
- ✅ Progressive loading functional (frontmatter → instructions → resources)
- ✅ All resources accessible and helpful
- ✅ No integration issues with existing workflows
- ✅ Context loading improves agent effectiveness

### 5.2 core-issue-focus Skill Validation

**Test with 1-2 Agents:**

1. **TestEngineer Mission Discipline:**
   - Load core-issue-focus skill
   - Receive mission with specific core issue
   - Apply 4-step workflow
   - Validate mission drift detection
   - Confirm surgical scope discipline

**Acceptance Criteria:**
- ✅ Agent successfully applies mission discipline framework
- ✅ Scope creep prevented through skill guidance
- ✅ Core issue validation checkpoints functional
- ✅ Templates and examples helpful
- ✅ Agent effectiveness preserved while maintaining focus

**Commit:** `test: validate documentation-grounding and core-issue-focus skills (#310)`

---

## 6. Agent-Specific Context Packages

### PromptEngineer Context Package

```yaml
CORE_ISSUE: "Create documentation-grounding and core-issue-focus skills per Epic #291 specifications"
INTENT_ANALYSIS: COMMAND - Direct skill creation (PromptEngineer owns .claude/ directory)
TARGET_FILES:
  - .claude/skills/documentation/documentation-grounding/SKILL.md
  - .claude/skills/coordination/core-issue-focus/SKILL.md
  - All resource files (templates, examples, documentation)
AGENT_SELECTION: PromptEngineer (exclusive .claude/ authority)
FLEXIBLE_AUTHORITY: Full skill creation authority
MINIMAL_SCOPE: Create 2 complete skills with all resources per specifications
SUCCESS_TEST: Both skills loadable by agents, progressive loading functional, integration testing passes

Mission Objective: Create documentation-grounding and core-issue-focus skills following official Claude Code skills structure

GitHub Issue Context: Issue #310, Epic #291 Iteration 1.2, blocks Issue #309

Technical Constraints:
- Official Skills Structure: YAML frontmatter in SKILL.md per Docs/Specs/epic-291-skills-commands/official-skills-structure.md
- SKILL.md <500 lines recommended
- Frontmatter: name (max 64 chars), description (max 1024 chars, includes WHAT and WHEN)
- Progressive loading: frontmatter (~100 tokens) → instructions (~3,000 and ~2,000 tokens) → resources (on-demand)
- Directory Documentation: Categories MUST have README.md; individual skills use SKILL.md only

INTENT RECOGNITION PATTERNS:
- Command-intent request: "Create skills per specifications"
- Direct file modifications within .claude/ directory authority
- Deliverable: Complete skill directories with SKILL.md + resources/

Working Directory Discovery: Check for Issue #311 completion artifacts and build upon established patterns
Working Directory Communication: Report skill creation progress immediately using standardized format

Integration Requirements:
- Coordinate with working-directory-coordination skill (Issue #311)
- Skills must integrate with multi-agent orchestration
- Progressive loading must work correctly
- All 11 agents can adopt documentation-grounding
- 6 primary agents can adopt core-issue-focus

Standards Context:
- DocumentationStandards.md: Section 2 (README patterns), Section 4 (Mermaid diagrams)
- Official Skills Structure: Complete specification in Docs/Specs/epic-291-skills-commands/official-skills-structure.md
- Directory Documentation Standards: .claude/ READMEs created in Issue #311

Module Context:
- .claude/skills/README.md: Skills architecture and organization
- .claude/skills/coordination/README.md: Coordination category standards
- Review working-directory-coordination skill as reference implementation

Quality Gates:
- YAML frontmatter valid per official specification
- SKILL.md <500 lines recommended
- Progressive loading validated
- Integration testing with 2+ agents
- ComplianceOfficer validation at section level (not per skill)
```

---

## 7. Success Metrics

### documentation-grounding Skill
- ✅ Complete SKILL.md with YAML frontmatter (~3,000 tokens)
- ✅ 2 templates created (standards-loading-checklist, module-context-template)
- ✅ 3 examples created (backend-specialist, test-engineer, documentation-maintainer)
- ✅ 2 documentation files created (grounding-optimization-guide, selective-loading-patterns)
- ✅ All 11 agent-specific grounding patterns documented
- ✅ Progressive loading validated
- ✅ ~400 lines redundancy eliminated (~3,200 tokens saved)

### core-issue-focus Skill
- ✅ Complete SKILL.md with YAML frontmatter (~2,000 tokens)
- ✅ 3 templates created (core-issue-analysis, scope-boundary-definition, success-criteria-validation)
- ✅ 3 examples created (api-bug-fix, feature-implementation-focused, refactoring-scoped)
- ✅ 2 documentation files created (mission-drift-patterns, validation-checkpoints)
- ✅ 6 agent patterns documented (TestEngineer, PromptEngineer, CodeChanger, BackendSpecialist, FrontendSpecialist, WorkflowEngineer)
- ✅ Mission discipline framework functional
- ✅ ~200 lines redundancy eliminated (~1,600 tokens saved)

### Combined Impact
- ✅ ~600 lines total redundancy eliminated
- ✅ ~4,800 tokens total saved
- ✅ Both skills integrated with orchestration
- ✅ Integration testing successful
- ✅ All acceptance criteria met

---

## 8. Orchestration Notes

### Iterative Execution Pattern
1. **Skill 1 Creation:** Engage PromptEngineer for documentation-grounding (Days 1-3)
2. **Review & Adapt:** Validate skill structure, adapt approach if needed
3. **Skill 2 Creation:** Engage PromptEngineer for core-issue-focus (Days 4-5)
4. **Integration Testing:** TestEngineer validation with both skills (Day 6)
5. **Quality Coordination:** Ensure standards met, documentation complete

### Communication Enforcement
- **Artifact Discovery:** PromptEngineer MUST check working-dir/ for Issue #311 completion artifacts before starting
- **Immediate Reporting:** REQUIRED when creating/updating skill files
- **Context Integration:** Build upon working-directory-coordination patterns established in Issue #311

### Section-Level Quality Gates
**NOT invoking ComplianceOfficer after Issue #310** - quality validation happens at section level after all Iteration 1 issues (#311, #310, #309, #308) complete.

---

## 9. Risk Mitigation

### Identified Risks
1. **Skills too verbose:** SKILL.md exceeds 500 line recommendation
   - Mitigation: Strict content review, move advanced topics to resources/

2. **Progressive loading not functional:** Agents can't access resources
   - Mitigation: Test with 2+ agents, validate resource accessibility

3. **Agent-specific patterns incomplete:** Not all 11 agents covered for documentation-grounding
   - Mitigation: Explicit checklist validation, comprehensive agent review

4. **Integration with orchestration breaks:** Skills disrupt existing workflows
   - Mitigation: Integration testing, preserve working directory protocols

### Success Validation
- Build succeeds with zero warnings
- All validation scripts pass (when created in Issue #308)
- Integration testing confirms skills loadable and functional
- No regression in agent effectiveness
- Context savings measurable and significant

---

## 10. Handoff to Next Issue

**Upon Completion:**
- Update working directory with Issue #310 completion summary
- Document lessons learned for Issue #309
- Confirm section branch ready for continued work
- Verify all acceptance criteria met before proceeding to Issue #309

**Artifacts for Issue #309:**
- documentation-grounding skill available for reference
- Skill creation patterns validated
- Progressive loading architecture proven
- Ready for github-issue-creation skill development

---

**Execution Plan Status:** ✅ READY FOR EXECUTION

**Next Action:** Engage PromptEngineer to create documentation-grounding skill following this execution plan.
