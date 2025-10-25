# Artifact Reporting Template

**Purpose:** Standard format for Immediate Artifact Reporting (Step 2 of working-directory-coordination workflow)

**When to Use:** WHEN CREATING/UPDATING ANY WORKING DIRECTORY FILE - immediate reporting required

---

## Template Format

```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: [exact-filename-with-extension]
- Purpose: [brief description of content and intended consumers]
- Context for Team: [what other agents need to know about this artifact]
- Dependencies: [what other artifacts this builds upon or relates to]
- Next Actions: [any follow-up coordination needed]
```

---

## Field Guidance

### Filename
**What to include:**
- Exact filename with extension
- No paths, just filename for clarity

**Examples:**
- `epic-291-issue-311-execution-plan.md`
- `coverage-analysis-report.md`
- `api-design-decisions.md`

### Purpose
**What to include:**
- Brief 1-2 sentence description of artifact content
- Identify intended consumers (which agents/roles benefit)
- Specify artifact type (analysis, design, implementation notes, handoff context)

**Examples:**
- `Complete 7-subtask breakdown for Issue #311 implementation; intended for PromptEngineer execution`
- `Test coverage gap analysis for Recipe service; intended for TestEngineer prioritization`
- `API endpoint design for new Recipe features; intended for FrontendSpecialist UI implementation`

### Context for Team
**What to include:**
- What do other agents need to know about this artifact?
- How does this artifact affect team workflows or coordination?
- What decisions or insights does this capture?

**Examples:**
- `This provides comprehensive specifications for all deliverables - reduces clarification overhead`
- `Coverage gaps identified block feature completion - priority coordination needed`
- `API contract finalized - frontend implementation can begin`

### Dependencies
**What to include:**
- Which other artifacts does this build upon or reference?
- What prior work informed this artifact's creation?
- Note if artifact is independent or part of larger context chain

**Examples:**
- `None - foundational execution plan`
- `Builds upon backend-api-design.md and test-coverage-gaps.md`
- `References specifications from skills-catalog.md and implementation-iterations.md`

### Next Actions
**What to include:**
- What follow-up coordination is needed?
- Which agents should engage with this artifact next?
- What decisions or validations are pending?

**Examples:**
- `PromptEngineer to execute 7 subtasks sequentially`
- `BackendSpecialist to implement API; FrontendSpecialist to consume endpoints`
- `TestEngineer to validate coverage improvements in next sprint`

---

## Usage Instructions

1. **Copy Template:** Use exact template format above
2. **Report Immediately:** Don't batch - report when file is created/updated
3. **Fill Each Field:** Provide specific, actionable details
4. **One Report Per File:** Each artifact gets individual reporting
5. **Be Explicit:** Include all required fields with clear information

---

## Examples

### Example 1: Execution Plan Artifact
```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: epic-291-issue-311-execution-plan.md
- Purpose: Complete 7-subtask breakdown for Issue #311 implementation; intended for PromptEngineer execution
- Context for Team: This provides comprehensive specifications for all deliverables - reduces clarification overhead
- Dependencies: References specifications from skills-catalog.md and implementation-iterations.md
- Next Actions: PromptEngineer to execute 7 subtasks sequentially
```

### Example 2: Analysis Report
```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: coverage-analysis-report.md
- Purpose: Test coverage gap analysis for Recipe service identifying 15 missing test cases; intended for TestEngineer
- Context for Team: Coverage gaps block feature completion - priority coordination needed with BackendSpecialist
- Dependencies: Builds upon backend-implementation.md service analysis
- Next Actions: TestEngineer to create missing tests; BackendSpecialist to review testability of new methods
```

### Example 3: Design Decisions
```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: api-design-decisions.md
- Purpose: REST endpoint specifications for Recipe CRUD operations; intended for FrontendSpecialist UI implementation
- Context for Team: API contract finalized - frontend implementation can begin without backend blocking
- Dependencies: Builds upon feature-requirements.md and architectural-patterns.md
- Next Actions: FrontendSpecialist to implement UI consuming these endpoints; TestEngineer to create integration tests
```

### Example 4: Session State Tracking
```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: session-state.md
- Purpose: Multi-agent coordination progress tracking for Epic #291; intended for Claude orchestration
- Context for Team: Claude uses this to track progress, adapt strategy, and coordinate next agent engagements
- Dependencies: Updated after each agent engagement - cumulative context
- Next Actions: Claude to review progress and determine next agent selection based on current state
```

---

## Common Pitfalls to Avoid

### DON'T: Generic or vague descriptions
❌ `- Purpose: Analysis report`
✅ `- Purpose: Test coverage gap analysis for Recipe service identifying 15 missing test cases; intended for TestEngineer`

### DON'T: Skip intended consumers
❌ `- Purpose: API design`
✅ `- Purpose: REST endpoint specifications for Recipe CRUD operations; intended for FrontendSpecialist UI implementation`

### DON'T: Miss dependencies
❌ `- Dependencies: None`
✅ `- Dependencies: Builds upon backend-implementation.md service analysis and feature-requirements.md`

### DON'T: Leave next actions vague
❌ `- Next Actions: Follow-up needed`
✅ `- Next Actions: TestEngineer to create missing tests; BackendSpecialist to review testability of new methods`

---

**Resource Status:** ✅ Ready for Use
**Skill:** working-directory-coordination
**Step:** 2 - Immediate Artifact Reporting
