# Issue #298 Execution Plan - Iteration 4.1: High-Impact Agents Refactoring

**Epic:** #291 - Agent Skills & Slash Commands Integration
**Iteration:** 4.1 - High-Impact Agents Refactoring (5 agents)
**Branch:** section/iteration-4
**Status:** In Progress
**Started:** 2025-10-26

---

## Context Summary

### Epic #291 Progress
- ✅ **Iteration 1 Complete:** Core skills operational (working-directory-coordination, documentation-grounding, core-issue-focus, github-issue-creation)
- ✅ **Iteration 2 Complete:** Meta-skills operational (agent-creation, skill-creation, command-creation)
- ✅ **Iteration 3 Complete:** Documentation alignment finished, validation framework in place
- 🔄 **Iteration 4 In Progress:** Agent refactoring phase

### Issue #298 Objectives
Refactor the 5 largest agents to achieve maximum context savings early in Iteration 4:
1. **FrontendSpecialist** (550 → 180 lines, 67% reduction, 370 lines saved)
2. **BackendSpecialist** (536 → 180 lines, 66% reduction, 356 lines saved)
3. **TestEngineer** (524 → 200 lines, 62% reduction, 324 lines saved)
4. **DocumentationMaintainer** (534 → 190 lines, 64% reduction, 344 lines saved)
5. **WorkflowEngineer** (510 → 200 lines, 61% reduction, 310 lines saved)

**Target:** ~1,800 lines eliminated (cumulative savings for 5 agents)

---

## Implementation Strategy

### Delegation Protocol
**CORE ISSUE:** Eliminate redundant cross-cutting patterns from 5 largest agent definitions while preserving agent effectiveness through skill references.

**INTENT RECOGNITION:** Implementation request - PromptEngineer has direct authority over `.claude/agents/*.md` files.

**SCOPE BOUNDARY:** Modification of 5 agent definition files using skill extraction and reference patterns.

**SUCCESS CRITERIA:**
- 60%+ context reduction per agent
- Agent effectiveness preserved (validated through test engagements)
- Skill references clear with 2-3 line summaries
- Progressive loading functional

**FLEXIBLE AUTHORITY:** PromptEngineer owns `.claude/` directory with direct edit rights.

### Per-Agent Refactoring Process
For each of the 5 agents, execute these steps:

#### Phase 1: Analysis & Extraction Planning
- Identify redundant patterns eligible for skill extraction:
  - Working directory coordination protocols
  - Documentation grounding workflows
  - Core issue focus frameworks
  - Flexible authority management (for specialists)
  - Domain-specific deep technical patterns
- Calculate context reduction potential
- Plan skill reference integration points

#### Phase 2: Skill Reference Integration
- Replace extracted patterns with skill references using format:
  ```markdown
  ## [Capability] Implementation
  **SKILL REFERENCE**: `.claude/skills/category/skill-name/`

  [2-3 line summary of skill purpose]

  Key Workflow: [Step 1 | Step 2 | Step 3]

  [See skill for complete instructions]
  ```
- Preserve core agent identity (130-240 lines target)
- Maintain authority boundaries and orchestration patterns

#### Phase 3: Validation
- Verify agent definition compiles and validates
- Check line count reduction (target met?)
- Ensure no broken cross-references
- Validate orchestration integration preserved

---

## Execution Plan - Iterative Agent Engagement

### Agent 1: FrontendSpecialist (Day 1)
**Owner:** PromptEngineer
**Current:** 550 lines
**Target:** 180 lines (67% reduction)
**Skills to Reference:**
- working-directory-coordination
- documentation-grounding
- flexible-authority-management
- Core frontend patterns (component design, NgRx, API integration)

**Subtasks:**
1. Engage PromptEngineer with context package for FrontendSpecialist refactoring
2. Review refactored agent definition
3. Validate line count reduction
4. Test agent engagement
5. Commit changes: `feat: refactor FrontendSpecialist with skill references (#298)`

---

### Agent 2: BackendSpecialist (Day 2)
**Owner:** PromptEngineer
**Current:** 536 lines
**Target:** 180 lines (66% reduction)
**Skills to Reference:**
- working-directory-coordination
- documentation-grounding
- flexible-authority-management
- Core backend patterns (API architecture, EF Core, service design)

**Subtasks:**
1. Engage PromptEngineer with context package for BackendSpecialist refactoring
2. Review refactored agent definition
3. Validate line count reduction
4. Test agent engagement
5. Commit changes: `feat: refactor BackendSpecialist with skill references (#298)`

---

### Agent 3: TestEngineer (Day 3)
**Owner:** PromptEngineer
**Current:** 524 lines
**Target:** 200 lines (62% reduction)
**Skills to Reference:**
- working-directory-coordination
- documentation-grounding
- core-issue-focus
- Testing patterns (coverage tracking, test creation, validation)

**Subtasks:**
1. Engage PromptEngineer with context package for TestEngineer refactoring
2. Review refactored agent definition
3. Validate line count reduction
4. Test agent engagement
5. Commit changes: `feat: refactor TestEngineer with skill references (#298)`

---

### Agent 4: DocumentationMaintainer (Day 4)
**Owner:** PromptEngineer
**Current:** 534 lines
**Target:** 190 lines (64% reduction)
**Skills to Reference:**
- working-directory-coordination
- documentation-grounding
- Documentation patterns (README structure, standards compliance)

**Subtasks:**
1. Engage PromptEngineer with context package for DocumentationMaintainer refactoring
2. Review refactored agent definition
3. Validate line count reduction
4. Test agent engagement
5. Commit changes: `feat: refactor DocumentationMaintainer with skill references (#298)`

---

### Agent 5: WorkflowEngineer (Day 5)
**Owner:** PromptEngineer
**Current:** 510 lines
**Target:** 200 lines (61% reduction)
**Skills to Reference:**
- working-directory-coordination
- documentation-grounding
- flexible-authority-management
- CI/CD patterns (workflow optimization, GitHub Actions)

**Subtasks:**
1. Engage PromptEngineer with context package for WorkflowEngineer refactoring
2. Review refactored agent definition
3. Validate line count reduction
4. Test agent engagement
5. Commit changes: `feat: refactor WorkflowEngineer with skill references (#298)`

---

## Quality Gates

### Per-Agent Quality Checkpoints
- ✅ 60%+ context reduction achieved
- ✅ Agent effectiveness preserved (test engagement successful)
- ✅ Skill references clear with 2-3 line summaries
- ✅ Orchestration integration validated
- ✅ Progressive loading works correctly
- ✅ Build succeeds with zero warnings
- ✅ Conventional commit with issue reference

### Issue Completion Quality Gates
**After all 5 agents refactored:**
- ✅ Cumulative savings: ~1,800 lines eliminated
- ✅ All 5 agents functional with skill loading
- ✅ No agent effectiveness regression
- ✅ Orchestration patterns preserved
- ✅ All commits on section/iteration-4 branch
- ✅ Issue #298 ready for section completion validation

### Section Completion (After All Iteration 4 Issues)
**Note:** ComplianceOfficer validation occurs AFTER all Iteration 4 issues (#298, #297, #296, #295) complete, not per-issue.

---

## Coordination Notes

### Working Directory Communication Protocol
All PromptEngineer engagements will follow mandatory artifact reporting:

```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: [exact-filename-with-extension]
- Purpose: [brief description]
- Context for Team: [integration notes]
- Next Actions: [follow-up coordination]
```

### Artifact Discovery Mandate
Before each agent engagement, PromptEngineer must:
- Review existing working-dir/ artifacts
- Identify relevant context from prior work
- Report discovered artifacts
- Build upon existing analysis

---

## Branch Strategy

**Epic Branch:** `epic/skills-commands-291` (created from main)
**Section Branch:** `section/iteration-4` (created from epic branch)
**Current Branch:** `section/iteration-4` ✅

### Commit Strategy
- One commit per agent refactored
- Conventional commit messages: `feat: refactor [AgentName] with skill references (#298)`
- All commits on section/iteration-4 branch
- Section PR created after all Iteration 4 issues complete

---

## Success Metrics

### Quantitative Targets
- **FrontendSpecialist:** 67% reduction (370 lines)
- **BackendSpecialist:** 66% reduction (356 lines)
- **TestEngineer:** 62% reduction (324 lines)
- **DocumentationMaintainer:** 64% reduction (344 lines)
- **WorkflowEngineer:** 61% reduction (310 lines)
- **Total:** ~1,800 lines eliminated

### Qualitative Targets
- Agent effectiveness: 100% preserved
- Orchestration integration: Seamless
- Skill loading: Functional
- Team coordination: Clear artifacts

---

## Risk Mitigation

### Identified Risks
1. **Breaking orchestration integration** → Test after each refactor
2. **Over-extraction losing agent identity** → Preserve 130-240 line core
3. **Skill loading failures** → Validate progressive loading patterns
4. **Context reduction falls short** → Re-analyze extraction opportunities

### Mitigation Strategy
- Incremental agent-by-agent approach
- Validation after each completion
- Test engagement before committing
- Rollback capability maintained

---

## Next Actions

**Immediate:**
1. ✅ Create working directory and execution plan (COMPLETE)
2. 🔄 Engage PromptEngineer for FrontendSpecialist refactoring (NEXT)
3. ⏳ Continue iterative agent refactoring (Days 2-5)

**Upon Issue Completion:**
- Document cumulative savings
- Update working directory with progress
- Coordinate handoff to Issue #297 (Medium-Impact Agents)

**Upon Section Completion:**
- Invoke ComplianceOfficer for section-level validation
- Create section PR against epic branch
- Prepare for Iteration 5 integration

---

**Confidence Level:** High - Prerequisites met, skills operational, clear acceptance criteria, incremental approach minimizes risk.
