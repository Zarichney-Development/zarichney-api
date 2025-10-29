# Issue #297 Execution Plan - Iteration 4.2: Medium-Impact Agents Refactoring

**Epic:** #291 - Agent Skills & Slash Commands Integration
**Iteration:** 4.2 - Medium-Impact Agents Refactoring (3 agents)
**Branch:** section/iteration-4 (continuing from Issue #298)
**Status:** In Progress
**Started:** 2025-10-26

---

## Context Summary

### Issue #298 Completion (Prerequisite)
✅ **COMPLETE** - All 5 high-impact agents refactored
- **Cumulative Savings:** 1,261 lines (47.5% average reduction)
- **Pattern Validated:** 40-70% reduction range
  - Lower Bound: 40.5-43.2% (specialists & specialist-like)
  - Upper Bound: 68.9% (primary with strong skill alignment)
- **Universal Skills:** working-directory-coordination, documentation-grounding
- **Primary Agent Skill:** core-issue-focus (TestEngineer)

### Issue #297 Objectives
Refactor 3 medium-impact agents to continue progressive context optimization:
1. **CodeChanger** (488 lines → target 40-50% reduction, ~200-240 lines saved)
2. **SecurityAuditor** (453 lines → target 40-50% reduction, ~180-220 lines saved)
3. **BugInvestigator** (449 lines → target 40-50% reduction, ~180-220 lines saved)

**Realistic Target:** ~600-680 lines saved (vs. original ~870 target)

---

## Validated Pattern Application

### From Issue #298 Learnings

**Lower Bound Pattern (40-45%):** Agents with substantial domain-specific architecture
- Apply to: CodeChanger (primary file-editing with extensive integration patterns)
- Apply to: BugInvestigator (diagnostic specialist with systematic analysis frameworks)

**Middle Range (42-48%):** Primary agents with disciplined workflows
- Apply to: SecurityAuditor (security analysis with flexible authority framework)

**Universal Skill Extraction:**
- working-directory-coordination (~60 lines per agent)
- documentation-grounding (~100-120 lines per agent)
- core-issue-focus (if mission discipline applicable)

**Streamlining Opportunities:**
- Verbose coordination frameworks → Concise essential-only sections
- Multi-section guidelines → Single focused paragraphs
- Redundant patterns → Merged coordination sections

---

## Implementation Strategy

### Delegation Protocol
**CORE ISSUE:** Eliminate redundant cross-cutting patterns from 3 medium-impact agent definitions while preserving agent effectiveness through skill references.

**INTENT RECOGNITION:** Implementation request - PromptEngineer has direct authority over `.claude/agents/*.md` files.

**SCOPE BOUNDARY:** Modification of 3 agent definition files using validated skill extraction patterns from Issue #298.

**SUCCESS CRITERIA:**
- 40-50% context reduction per agent (validated baseline)
- Agent effectiveness preserved (validated through test engagements)
- Skill references clear with 2-3 line summaries
- Progressive loading functional
- Cumulative: ~600-680 lines saved (8 agents total: 1,861-1,941 lines)

**FLEXIBLE AUTHORITY:** PromptEngineer owns `.claude/` directory with direct edit rights.

---

## Agent-Specific Plans

### Agent 6: CodeChanger
**Owner:** PromptEngineer
**Current:** 488 lines
**Realistic Target:** 240-260 lines (45-48% reduction, ~220-248 lines saved)

**Agent Profile:**
- **Type:** PRIMARY file-editing agent
- **Mission:** Feature implementation, bug fixes, refactoring
- **Integration:** Coordinates with all specialists, hands off to TestEngineer
- **Authority:** Direct modification of all source code files

**Skills to Reference:**
- working-directory-coordination (team artifact sharing)
- documentation-grounding (systematic standards loading)
- core-issue-focus (mission discipline - highly relevant for focused implementation)

**Preservation Requirements:**
- Code modification authority and file-editing workflows
- Specialist coordination patterns (receives requirements, implements features)
- TestEngineer handoff protocols
- Quality gates and validation patterns

**Expected Pattern:** Middle range (45-48%) due to primary agent with disciplined implementation workflows

---

### Agent 7: SecurityAuditor
**Owner:** PromptEngineer
**Current:** 453 lines
**Realistic Target:** 230-250 lines (42-48% reduction, ~203-223 lines saved)

**Agent Profile:**
- **Type:** SPECIALIST with flexible authority
- **Mission:** Security analysis, vulnerability assessment, threat mitigation
- **Integration:** Reviews implementations from all agents, coordinates with BackendSpecialist
- **Authority:** Advisory mode (working directory analysis) + Implementation mode (vulnerability fixes)

**Skills to Reference:**
- working-directory-coordination (security analysis artifact sharing)
- documentation-grounding (security standards and patterns loading)
- flexible-authority-management (if this becomes a universal specialist pattern)

**Preservation Requirements:**
- Intent recognition framework (query vs. command intent)
- Security domain expertise (OWASP, authentication, vulnerability patterns)
- Flexible authority boundaries (analysis vs. implementation modes)
- Cross-agent security coordination

**Expected Pattern:** Middle range (42-48%) due to specialist with flexible authority framework

---

### Agent 8: BugInvestigator
**Owner:** PromptEngineer
**Current:** 449 lines
**Realistic Target:** 225-245 lines (44-48% reduction, ~204-224 lines saved)

**Agent Profile:**
- **Type:** SPECIALIST (advisory/diagnostic focus)
- **Mission:** Root cause analysis, diagnostic reporting, systematic debugging
- **Integration:** Receives bug reports, provides diagnostic analysis via working directory
- **Authority:** Advisory only (working directory artifacts, no direct file modification)

**Skills to Reference:**
- working-directory-coordination (diagnostic artifact generation and reporting)
- documentation-grounding (systematic context loading for analysis)

**Preservation Requirements:**
- Diagnostic analysis frameworks and systematic debugging workflows
- Root cause investigation patterns
- Performance analysis and stack trace interpretation
- Working directory artifact generation expertise

**Expected Pattern:** Middle range (44-48%) due to advisory agent with systematic analysis frameworks

---

## Quality Gates

### Per-Agent Quality Checkpoints
- ✅ 40-50% context reduction achieved (validated baseline)
- ✅ Agent effectiveness preserved (test engagement successful)
- ✅ Skill references clear with 2-3 line summaries
- ✅ Orchestration integration validated
- ✅ Progressive loading works correctly
- ✅ Build succeeds with zero warnings
- ✅ Conventional commit with issue reference

### Issue #297 Completion Quality Gates
**After all 3 agents refactored:**
- ✅ Cumulative savings: ~600-680 lines (Issue #297)
- ✅ Total savings: ~1,861-1,941 lines (Issues #298 + #297, 8 of 11 agents)
- ✅ All 3 agents functional with skill loading
- ✅ No agent effectiveness regression
- ✅ Orchestration patterns preserved
- ✅ All commits on section/iteration-4 branch
- ✅ Ready for Issue #296 (Lower-Impact Agents)

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
- Review Issue #298 completion report and validated patterns
- Identify relevant context from 5 high-impact agent refactorings
- Report discovered patterns
- Apply validated techniques to medium-impact agents

---

## Branch Strategy

**Epic Branch:** `epic/skills-commands-291` (created from main)
**Section Branch:** `section/iteration-4` (continuing from Issue #298)
**Current Branch:** `section/iteration-4` ✅

### Commit Strategy
- One commit per agent refactored
- Conventional commit messages: `feat: refactor [AgentName] with skill references (#297)`
- All commits on section/iteration-4 branch (same as Issue #298)
- Section PR created after ALL Iteration 4 issues complete

---

## Success Metrics

### Quantitative Targets (Validated from Issue #298)
- **CodeChanger:** 45-48% reduction (~220-248 lines saved)
- **SecurityAuditor:** 42-48% reduction (~203-223 lines saved)
- **BugInvestigator:** 44-48% reduction (~204-224 lines saved)
- **Issue #297 Total:** ~600-680 lines saved
- **Cumulative (8 agents):** ~1,861-1,941 lines saved

### Qualitative Targets
- Agent effectiveness: 100% preserved
- Orchestration integration: Seamless
- Skill loading: Functional
- Team coordination: Clear artifacts

---

## Risk Mitigation

### Identified Risks
1. **Breaking orchestration integration** → Test after each refactor (validated from Issue #298)
2. **Over-extraction losing agent identity** → Preserve domain expertise (validated approach)
3. **Skill loading failures** → Use validated progressive loading patterns
4. **Context reduction falls short** → Apply realistic 40-50% baseline

### Mitigation Strategy
- Incremental agent-by-agent approach (proven successful)
- Validation after each completion (working pattern)
- Test engagement before committing (quality gate)
- Apply Issue #298 patterns (40-70% validated range)

---

## Next Actions

**Immediate:**
1. ✅ Create working directory and execution plan (COMPLETE)
2. 🔄 Engage PromptEngineer for CodeChanger refactoring (NEXT)
3. ⏳ Continue iterative agent refactoring (SecurityAuditor, BugInvestigator)

**Upon Issue #297 Completion:**
- Document cumulative savings (Issues #298 + #297)
- Update working directory with progress
- Coordinate handoff to Issue #296 (Lower-Impact Agents: 3 remaining)

**Upon Iteration 4 Completion:**
- Invoke ComplianceOfficer for section-level validation
- Create section PR against epic branch
- Prepare for Iteration 5 integration

---

**Confidence Level:** High - Validated patterns from Issue #298, clear acceptance criteria, proven incremental approach, realistic baseline expectations.
