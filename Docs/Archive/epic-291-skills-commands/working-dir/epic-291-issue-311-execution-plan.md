# Epic #291 - Issue #311 Execution Plan

**Generated:** 2025-10-25
**Epic:** #291 - Agent Skills & Slash Commands Integration
**Issue:** #311 - Iteration 1.1: Core Skills - Working Directory Coordination
**Iteration:** 1 (Foundation)
**Section Branch:** section/iteration-1

---

## 🎯 EPIC & ISSUE CONTEXT

### Epic #291 Overview
**Purpose:** Comprehensive integration of Claude Code's agent skills and slash commands capabilities
**Success Metrics:**
- 62% average context reduction across all 11 agents
- All 5 core skills operational and adopted
- All 3 meta-skills enabling scalability
- All 4 priority workflow commands functional
- ~9,864 tokens saved per typical multi-agent session

### Issue #311 Details
**Title:** Iteration 1.1: Core Skills - Working Directory Coordination
**Status:** Open - Ready for Development
**Priority:** High (P0 - Foundation for entire epic)
**Blocks:** Issues #310, #307, #298 (Iterations 1.2, 2.1, 4.1)

**Purpose:**
Create working-directory-coordination skill to standardize team communication protocols, artifact management, and context flow across all 11 agents.

**Acceptance Criteria:**
✅ Skill has adheres to Docs/Specs/epic-291-skills-commands/official-skills-structure.md
✅ At least 2 agents successfully load and use working-directory-coordination skill
✅ Progressive loading validated: metadata → instructions → resources pattern works
✅ Context savings measurable (>3,600 tokens from ~450 lines redundancy elimination)
✅ Agent integration snippets validated and documented
✅ Mandatory communication protocols enforceable

---

## 🏗️ BRANCH STRATEGY

**Epic Branch:** epic/skills-commands-291 (created from main) ✅
**Section Branch:** section/iteration-1 (created from epic branch) ✅
**PR Target:** epic/skills-commands-291 ← section/iteration-1

**Commit Strategy:**
- One commit per subtask completion
- Conventional commit messages referencing #311
- All commits on section/iteration-1 branch
- Section PR created after ALL Iteration 1 issues complete (#311, #310, #309, #308)

---

## 📋 CORE ISSUE IDENTIFICATION

**CORE ISSUE:** Create foundational working-directory-coordination skill with mandatory communication protocols

**INTENT_RECOGNITION:** COMMAND_INTENT - Implementation request for new skill creation

**SCOPE_BOUNDARY:**
- `.claude/skills/coordination/working-directory-coordination/` (new directory)
- Alignment with Docs/Specs/epic-291-skills-commands/official-skills-structure.md
- Integration testing with at least 2 agents

**SUCCESS_CRITERIA:**
- Progressive loading validates correctly
- At least 2 agents successfully adopt skill
- Communication protocols enforceable
- Context savings >3,600 tokens measurable

**AUTHORITY_CHECK:**
- PromptEngineer has EXCLUSIVE FILE EDIT RIGHTS to `.claude/` directory
- Correct agent for skill creation
- No authority violations

**FLEXIBLE_AUTHORITY:**
- PromptEngineer: Direct implementation authority over `.claude/skills/`
- Command-intent confirmed: Implementation mode

---

## 🎯 SUBTASK BREAKDOWN WITH AGENT ASSIGNMENTS

### Subtask 1: Create Skill Structure & YAML Frontmatter
**Agent:** PromptEngineer
**Duration:** ~1-2 hours
**Deliverables:**
- `.claude/skills/coordination/working-directory-coordination/` directory structure
- SKILL.md with YAML frontmatter

**YAML Frontmatter Specification** (per [official Claude Code docs](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/)):
```yaml
---
name: working-directory-coordination
description: Standardize working directory usage and team communication protocols across all agents. Use when agents need to discover existing artifacts before starting work, report new deliverables immediately, or integrate work from other team members.
---
```

**Success Validation:**
- YAML frontmatter at top of SKILL.md
- `name`: 32 chars (well under 64 limit), lowercase/hyphens only ✅
- `description`: ~180 chars (well under 1024 limit), includes WHAT and WHEN ✅
- NO metadata.json file (that structure is incorrect)
- Refer to Docs/Specs/epic-291-skills-commands/official-skills-structure.md

---

### Subtask 2: Implement SKILL.md Instructions
**Agent:** PromptEngineer
**Duration:** ~3-4 hours
**Deliverables:**
- `.claude/skills/coordination/working-directory-coordination/SKILL.md` (~2,500 tokens)

**Content Requirements:**
1. **Purpose Section:**
   - Clear mission statement
   - Mandatory application across all agents
   - Team awareness and context continuity objectives

2. **When to Use Section:**
   - MANDATORY: Before starting ANY task (artifact discovery)
   - MANDATORY: When creating/updating ANY working directory file
   - MANDATORY: When building upon other agents' artifacts
   - Universal application: All agents, all tasks, no exceptions

3. **Workflow Steps:**
   - **Step 1: Pre-Work Artifact Discovery (REQUIRED)**
     - Standard discovery format template
     - Checklist for existing artifact review
   - **Step 2: Immediate Artifact Reporting (MANDATORY)**
     - Standard reporting format template
     - Required fields specification
   - **Step 3: Context Integration Reporting (REQUIRED)**
     - Standard integration format template
     - Handoff preparation protocols
   - **Step 4: Communication Compliance Requirements**
     - No exceptions policy
     - Team awareness maintenance
     - Claude's enforcement role

4. **Resources Section:**
   - Reference to templates/
   - Reference to examples/
   - Reference to documentation/

**Success Validation:**
- Complete workflow steps documented
- Clear mandatory protocols specified
- Token count ~2,500 (within 2,000-5,000 target)
- References to resources correct

---

### Subtask 3: Create Resource Templates
**Agent:** PromptEngineer
**Duration:** ~2-3 hours
**Deliverables:**
- `resources/templates/artifact-discovery-template.md`
- `resources/templates/artifact-reporting-template.md`
- `resources/templates/integration-reporting-template.md`

**Template Specifications:**

**artifact-discovery-template.md:**
```markdown
🔍 WORKING DIRECTORY DISCOVERY:
- Current artifacts reviewed: [list existing files checked]
- Relevant context found: [artifacts that inform current work]
- Integration opportunities: [how existing work will be built upon]
- Potential conflicts: [any overlapping concerns identified]
```

**artifact-reporting-template.md:**
```markdown
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: [exact-filename-with-extension]
- Purpose: [brief description of content and intended consumers]
- Context for Team: [what other agents need to know about this artifact]
- Dependencies: [what other artifacts this builds upon or relates to]
- Next Actions: [any follow-up coordination needed]
```

**integration-reporting-template.md:**
```markdown
🔗 ARTIFACT INTEGRATION:
- Source artifacts used: [specific files that informed this work]
- Integration approach: [how existing context was incorporated]
- Value addition: [what new insights or progress this provides]
- Handoff preparation: [context prepared for future agents]
```

**Success Validation:**
- All 3 templates complete and formatted correctly
- Consistent with CLAUDE.md communication protocols
- Easy to copy-paste for agents

---

### Subtask 4: Create Resource Examples
**Agent:** PromptEngineer
**Duration:** ~2-3 hours
**Deliverables:**
- `resources/examples/multi-agent-coordination-example.md`
- `resources/examples/progressive-handoff-example.md`

**Example Specifications:**

**multi-agent-coordination-example.md:**
- Scenario: TestEngineer creates coverage analysis → DocumentationMaintainer updates README
- Demonstrates artifact discovery, reporting, and integration
- Shows how protocols prevent context gaps
- Realistic multi-agent workflow

**progressive-handoff-example.md:**
- Scenario: BackendSpecialist implements API → FrontendSpecialist builds UI → TestEngineer adds tests
- Demonstrates sequential agent collaboration
- Shows context continuity through artifacts
- Highlights handoff preparation importance

**Success Validation:**
- Realistic scenarios with actual agent names
- Clear demonstration of all 4 workflow steps
- Educational value for understanding protocols

---

### Subtask 5: Create Resource Documentation
**Agent:** PromptEngineer
**Duration:** ~1-2 hours
**Deliverables:**
- `resources/documentation/communication-protocol-guide.md`
- `resources/documentation/troubleshooting-gaps.md`

**Documentation Specifications:**

**communication-protocol-guide.md:**
- Deep dive on why protocols are mandatory
- Explanation of stateless AI operation context
- Benefits of team awareness
- Claude's enforcement role details

**troubleshooting-gaps.md:**
- Common communication failures
- Symptoms of missing artifact discovery
- Recovery from communication gaps
- Best practices for preventing failures

**Success Validation:**
- Comprehensive coverage of protocol philosophy
- Actionable troubleshooting guidance
- Clear explanations of enforcement

---

### Subtask 6: Integration Testing with 2 Agents
**Agent:** PromptEngineer (coordination), TestEngineer + DocumentationMaintainer (validation)
**Duration:** ~2-3 hours
**Deliverables:**
- Validated skill loading with TestEngineer
- Validated skill loading with DocumentationMaintainer
- Integration report documenting adoption

**Testing Approach:**
1. **TestEngineer Validation:**
   - Load working-directory-coordination skill
   - Execute artifact discovery workflow
   - Create test artifact with reporting protocol
   - Validate progressive loading (metadata → instructions → resources)

2. **DocumentationMaintainer Validation:**
   - Load working-directory-coordination skill
   - Execute artifact discovery workflow
   - Create documentation artifact with reporting protocol
   - Validate resource access (templates used correctly)

**Success Validation:**
- Both agents successfully load skill
- Progressive loading works correctly
- Communication protocols enforced
- No integration issues discovered

---

### Subtask 7: Validation & Documentation
**Agent:** PromptEngineer
**Duration:** ~1-2 hours
**Deliverables:**
- Working directory completion report
- Context savings measurement
- Integration snippet documentation

**Validation Tasks:**
- Measure metadata token count (<150 tokens requirement)
- Measure SKILL.md token count (~2,500 tokens target)
- Calculate context savings (baseline: ~450 lines across 11 agents)
- Document agent integration patterns

**Success Validation:**
- All acceptance criteria from Issue #311 met
- Token budgets within requirements
- Context savings measurable and documented
- Integration snippets ready for Iteration 4

---

## 📊 EXPECTED DELIVERABLES SUMMARY

**Created Files (8 total):**
1. `.claude/skills/coordination/working-directory-coordination/SKILL.md` (with YAML frontmatter)
3. `resources/templates/artifact-discovery-template.md`
4. `resources/templates/artifact-reporting-template.md`
5. `resources/templates/integration-reporting-template.md`
6. `resources/examples/multi-agent-coordination-example.md`
7. `resources/examples/progressive-handoff-example.md`
8. `resources/documentation/communication-protocol-guide.md`
9. `resources/documentation/troubleshooting-gaps.md`

**Total Files:** 8 files (SKILL.md + 7 resources)
**Estimated Total Token Count:** ~6,000-8,000 tokens (all resources combined)
**Frontmatter Efficiency:** ~100 tokens (98% savings during skill discovery)
**Context Savings Target:** >3,600 tokens across 11 agents

**CORRECTION NOTE:** Original plan included metadata.json which is NOT part of official Claude Code structure. Metadata goes in YAML frontmatter within SKILL.md per [official specification](../Docs/Specs/epic-291-skills-commands/official-skills-structure.md).

---

## 🔗 INTEGRATION POINTS

**Standards Context:**
- CLAUDE.md Section 8: Working Directory Communication Standards (existing protocols)
- DocumentationStandards.md: Skill metadata and resource organization (to be updated in Iteration 3)

**Epic Integration:**
- Foundation for Iteration 1.2 (documentation-grounding skill depends on this pattern)
- Foundation for Iteration 2.1 (meta-skills will reference this as exemplar)
- Foundation for Iteration 4.1 (all agent refactoring uses this skill)

**Quality Gates:**
- Progressive loading validation
- Token budget compliance
- Agent adoption validation (minimum 2 agents)
- Communication protocol enforcement verification

---

## 🎯 SUCCESS METRICS

**Quantitative:**
- ✅ YAML frontmatter ~100 tokens
- ✅ SKILL.md body ~2,500 tokens (<500 lines recommended)
- ✅ Context savings >3,600 tokens
- ✅ At least 2 agents adopt skill
- ✅ All 8 files complete (SKILL.md + 7 resources)

**Qualitative:**
- ✅ Progressive loading validates correctly
- ✅ Communication protocols enforceable
- ✅ Agent integration clear and functional
- ✅ Templates easy to use
- ✅ Examples educational and realistic

---

## 🚀 NEXT ACTIONS

1. **PromptEngineer:** Execute Subtasks 1-7 sequentially
2. **Commits:** One commit per subtask completion referencing #311
3. **Validation:** After subtask 6, confirm all acceptance criteria met
4. **Handoff:** Document completion in working directory for Iteration 1.2

**Section Completion Note:**
- This is Issue #311 (first of 4 issues in Iteration 1)
- Section PR will be created AFTER Issues #310, #309, #308 complete
- ComplianceOfficer will be invoked at SECTION level, not per-issue

---

**Plan Status:** ✅ **READY FOR EXECUTION**
**Confidence Level:** High - Clear specifications, realistic breakdown, proper agent assignments
