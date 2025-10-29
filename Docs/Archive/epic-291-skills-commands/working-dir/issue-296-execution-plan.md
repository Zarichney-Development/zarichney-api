# Issue #296 Execution Plan - Iteration 4.3: Lower-Impact Agents Refactoring

**Epic:** #291 - Agent Skills & Slash Commands Integration
**Iteration:** 4.3 - Lower-Impact Agents Refactoring (3 agents)
**Branch:** section/iteration-4 (continuing from Issues #298 and #297)
**Status:** In Progress
**Started:** 2025-10-26

---

## Context Summary

### Issues #298 + #297 Completion (Prerequisites)
✅ **COMPLETE** - 8 of 11 agents refactored
- **Issue #298:** 1,261 lines saved (47.5% avg, 5 agents)
- **Issue #297:** 782 lines saved (56.3% avg, 3 agents)
- **Cumulative:** 2,043 lines saved (50.5% avg across 8 agents)
- **Pattern Validated:** 40-70% reduction range
  - Lower Bound: 40.5-43.2% (specialists with domain architecture)
  - Middle Range: 43-48% (moderate streamlinable content)
  - Upper-Middle: 52-53% (extensive streamlinable frameworks)
  - Near Upper Bound: 64-65% (extensive verbose frameworks + aggressive streamlining)
  - Upper Bound: 68.9% (perfect skill alignment + extensive streamlinable content)

### Issue #296 Objectives
Refactor final 3 lower-impact agents to complete Iteration 4 agent refactoring:
1. **ArchitecturalAnalyst** (437 lines → target 43-48% reduction, ~190-210 lines saved)
2. **PromptEngineer** (413 lines → target 43-48% reduction, ~178-198 lines saved)
3. **ComplianceOfficer** (316 lines → target 43-48% reduction, ~136-152 lines saved)

**Realistic Target:** ~504-560 lines saved (conservative 43-48% baseline)

---

## Validated Pattern Application

### From Issues #298 + #297 Learnings

**Lower Bound Pattern (40-45%):** Agents with substantial domain-specific architecture
- FrontendSpecialist: 40.5%
- WorkflowEngineer: 41.6%
- Apply to: All 3 remaining agents as conservative baseline

**Middle Range (43-48%):** Advisory/specialist agents with moderate streamlinable content
- BackendSpecialist: 43.2%
- TestEngineer: 43.1%
- Expected for: ArchitecturalAnalyst, PromptEngineer, ComplianceOfficer

**Upper-Middle Pattern (52-53%):** Agents with extensive streamlinable coordination frameworks
- CodeChanger: 52.0%
- BugInvestigator: 52.3%
- Potential upside if verbose frameworks discovered

**Universal Skill Extraction:**
- working-directory-coordination (~60 lines per agent)
- documentation-grounding (~100-120 lines per agent)
- core-issue-focus (if mission discipline applicable)

**Streamlining Opportunities:**
- Verbose coordination frameworks → Concise essential-only sections
- Multi-section guidelines → Single focused paragraphs
- Redundant patterns → Merged coordination sections
- Intent recognition frameworks → Streamlined authority patterns

---

## Implementation Strategy

### Delegation Protocol
**CORE ISSUE:** Eliminate redundant cross-cutting patterns from final 3 agent definitions while preserving agent effectiveness through skill references.

**INTENT RECOGNITION:** Implementation request - PromptEngineer has direct authority over `.claude/agents/*.md` files.

**SCOPE BOUNDARY:** Modification of 3 agent definition files using validated skill extraction patterns from Issues #298 and #297.

**SUCCESS CRITERIA:**
- 43-48% context reduction per agent (validated conservative baseline)
- Agent effectiveness preserved (validated through test engagements)
- Skill references clear with 2-3 line summaries
- Progressive loading functional
- Cumulative: ~504-560 lines saved (11 agents total: 2,547-2,603 lines)

**FLEXIBLE AUTHORITY:** PromptEngineer owns `.claude/` directory with direct edit rights.

---

## Agent-Specific Plans

### Agent 9: ArchitecturalAnalyst
**Owner:** PromptEngineer
**Current:** 437 lines
**Realistic Target:** 230-250 lines (43-48% reduction, ~187-207 lines saved)

**Agent Profile:**
- **Type:** SPECIALIST (advisory/architectural analysis focus)
- **Mission:** System architecture analysis, design pattern evaluation, technical debt assessment
- **Integration:** Reviews implementations from all agents, provides architectural guidance via working directory
- **Authority:** Advisory only (working directory artifacts, technical documentation elevation for command intents)

**Skills to Reference:**
- working-directory-coordination (architectural analysis artifact generation and reporting)
- documentation-grounding (systematic architecture context loading for analysis)
- core-issue-focus (if analysis discipline patterns found)

**Preservation Requirements:**
- Architectural analysis frameworks and design pattern evaluation expertise
- SOLID principles enforcement and modular monolith understanding
- Testability architecture excellence patterns
- Team coordination protocols and cross-agent impact analysis
- Intent recognition framework (query vs. command intent for documentation authority)

**Expected Pattern:** Middle range (43-48%) due to advisory agent with architectural analysis frameworks

**Streamlining Opportunities:**
- 100-line documentation grounding extraction
- 60-line working directory coordination extraction
- Verbose architectural analysis frameworks → Concise focused sections
- Redundant team coordination patterns → Merged sections
- Intent recognition framework streamlining (similar to SecurityAuditor pattern)

---

### Agent 10: PromptEngineer
**Owner:** PromptEngineer (self-refactoring)
**Current:** 413 lines
**Realistic Target:** 215-235 lines (43-48% reduction, ~178-198 lines saved)

**Agent Profile:**
- **Type:** PRIMARY file-editing agent (exclusive authority over 28 AI prompt files)
- **Mission:** Strategic business translator converting requirements into surgical prompt modifications
- **Integration:** Coordinates with all agents for prompt optimization, direct edit authority over `.claude/agents/*.md`
- **Authority:** Direct modification of all AI prompt files (CLAUDE.md, agents, AI Sentinels, business logic prompts)

**Skills to Reference:**
- working-directory-coordination (prompt optimization artifact sharing)
- documentation-grounding (contextual prompt optimization requiring standards loading)
- core-issue-focus (mission discipline for prompt optimization scope management)

**Preservation Requirements:**
- Exclusive file-editing authority over 28 AI prompt files
- Strategic business translation methodology and contextual modification workflow
- Regression prevention protocols and surgical precision approach
- Comprehensive context loading protocol
- No Time Estimates Policy enforcement

**Expected Pattern:** Middle range (43-48%) due to primary agent with specialized domain

**Streamlining Opportunities:**
- 100-line documentation grounding extraction
- 60-line working directory coordination extraction
- Verbose context loading protocols → Concise essential-only sections
- Redundant file authority descriptions → Merged sections
- Core issue focus skill extraction (prompt optimization discipline)

---

### Agent 11: ComplianceOfficer
**Owner:** PromptEngineer
**Current:** 316 lines
**Realistic Target:** 160-180 lines (43-48% reduction, ~136-156 lines saved)

**Agent Profile:**
- **Type:** SPECIALIST (advisory/validation focus)
- **Mission:** Final pre-PR validation, comprehensive compliance check, dual validation partnership with Claude
- **Integration:** Partners with Claude for section-level validation, reviews all agent deliverables
- **Authority:** Advisory only (working directory artifacts, technical documentation elevation for command intents)

**Skills to Reference:**
- working-directory-coordination (validation artifact generation and reporting)
- documentation-grounding (comprehensive standards loading for validation)

**Preservation Requirements:**
- Pre-PR validation responsibilities and dual validation protocol with Claude
- Standards compliance validation (coding, testing, documentation, security)
- Differentiation from StandardsGuardian AI Sentinel
- Inter-agent work validation expertise
- Intent recognition framework (query vs. command intent for documentation authority)

**Expected Pattern:** Middle range (43-48%) due to advisory agent with validation frameworks

**Streamlining Opportunities:**
- 100-line documentation grounding extraction (6 standards files loading)
- 60-line working directory coordination extraction
- Verbose validation checklists → Concise focused sections
- Redundant partnership protocols → Merged sections
- Intent recognition framework streamlining

---

## Quality Gates

### Per-Agent Quality Checkpoints
- ✅ 43-48% context reduction achieved (validated conservative baseline)
- ✅ Agent effectiveness preserved (test engagement successful)
- ✅ Skill references clear with 2-3 line summaries
- ✅ Orchestration integration validated
- ✅ Progressive loading works correctly
- ✅ Build succeeds with zero warnings
- ✅ Conventional commit with issue reference

### Issue #296 Completion Quality Gates
**After all 3 agents refactored:**
- ✅ Cumulative savings: ~504-560 lines (Issue #296)
- ✅ Total savings: ~2,547-2,603 lines (Issues #298 + #297 + #296, all 11 agents)
- ✅ All 3 agents functional with skill loading
- ✅ No agent effectiveness regression
- ✅ Orchestration patterns preserved
- ✅ All commits on section/iteration-4 branch
- ✅ Ready for Issue #295 (Validation & Testing)

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
- Review Issues #298 and #297 completion reports and validated patterns
- Identify relevant context from 8 completed agent refactorings
- Report discovered patterns
- Apply validated techniques to remaining 3 agents

---

## Branch Strategy

**Epic Branch:** `epic/skills-commands-291` (created from main)
**Section Branch:** `section/iteration-4` (continuing from Issues #298 and #297)
**Current Branch:** `section/iteration-4` ✅

### Commit Strategy
- One commit per agent refactored
- Conventional commit messages: `feat: refactor [AgentName] with skill references (#296)`
- All commits on section/iteration-4 branch (same as Issues #298 and #297)
- Section PR created after ALL Iteration 4 issues complete

---

## Success Metrics

### Quantitative Targets (Conservative Baseline)
- **ArchitecturalAnalyst:** 43-48% reduction (~187-207 lines saved)
- **PromptEngineer:** 43-48% reduction (~178-198 lines saved)
- **ComplianceOfficer:** 43-48% reduction (~136-156 lines saved)
- **Issue #296 Total:** ~504-560 lines saved
- **Cumulative (11 agents):** ~2,547-2,603 lines saved
- **Overall Average:** ~49% reduction across 11 agents

### Qualitative Targets
- Agent effectiveness: 100% preserved
- Orchestration integration: Seamless
- Skill loading: Functional
- Team coordination: Clear artifacts

---

## Risk Mitigation

### Identified Risks
1. **Breaking orchestration integration** → Test after each refactor (validated from Issues #298 + #297)
2. **Over-extraction losing agent identity** → Preserve domain expertise (validated approach)
3. **Skill loading failures** → Use validated progressive loading patterns
4. **Context reduction falls short** → Apply realistic 43-48% baseline (conservative)

### Mitigation Strategy
- Incremental agent-by-agent approach (proven successful)
- Validation after each completion (working pattern)
- Test engagement before committing (quality gate)
- Apply Issues #298 + #297 patterns (40-70% validated range)

---

## Next Actions

**Immediate:**
1. ✅ Create working directory and execution plan (COMPLETE)
2. 🔄 Engage PromptEngineer for ArchitecturalAnalyst refactoring (NEXT)
3. ⏳ Continue iterative agent refactoring (PromptEngineer, ComplianceOfficer)

**Upon Issue #296 Completion:**
- Document cumulative savings (Issues #298 + #297 + #296)
- Update working directory with progress
- Coordinate handoff to Issue #295 (Validation & Testing)

**Upon Iteration 4 Completion:**
- Invoke ComplianceOfficer for section-level validation
- Create section PR against epic branch
- Prepare for Iteration 5 integration

---

**Confidence Level:** High - Validated patterns from Issues #298 and #297, clear acceptance criteria, proven incremental approach, realistic conservative baseline expectations.
