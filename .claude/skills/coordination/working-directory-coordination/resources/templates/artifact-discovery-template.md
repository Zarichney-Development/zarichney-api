# Artifact Discovery Template

**Purpose:** Standard format for Pre-Work Artifact Discovery (Step 1 of working-directory-coordination workflow)

**When to Use:** BEFORE STARTING ANY TASK - all agents, all scenarios, no exceptions

---

## Template Format

```
🔍 WORKING DIRECTORY DISCOVERY:
- Current artifacts reviewed: [list existing files checked]
- Relevant context found: [artifacts that inform current work]
- Integration opportunities: [how existing work will be built upon]
- Potential conflicts: [any overlapping concerns identified]
```

---

## Field Guidance

### Current Artifacts Reviewed
**What to include:**
- List all files currently in `/working-dir/`
- Note if working directory is empty
- Mention specific files examined for relevance

**Examples:**
- `epic-291-issue-311-execution-plan.md, coverage-analysis-report.md`
- `None found (empty working directory)`
- `session-state.md, backend-analysis.md, frontend-recommendations.md`

### Relevant Context Found
**What to include:**
- Which artifacts contain information relevant to current task?
- What key insights do these artifacts provide?
- How do these artifacts inform current mission?

**Examples:**
- `epic-291-issue-311-execution-plan.md provides complete 7-subtask breakdown`
- `N/A - starting fresh analysis`
- `coverage-analysis-report.md identifies gaps in Recipe service testing`

### Integration Opportunities
**What to include:**
- How will existing artifacts be built upon?
- What sections/insights will inform current work?
- How does this prevent duplication?

**Examples:**
- `Plan provides all necessary context for execution - will follow subtask sequence`
- `N/A - initial work for this issue`
- `Will use identified gaps to prioritize test creation order`

### Potential Conflicts
**What to include:**
- Any overlapping concerns with existing artifacts?
- Are multiple agents working on related areas?
- Any timing or dependency concerns?

**Examples:**
- `None - greenfield skill creation`
- `TestEngineer may be working on same service - need to coordinate`
- `BackendSpecialist API changes could affect my UI work`

---

## Usage Instructions

1. **Copy Template:** Use exact template format above
2. **Check Working Directory:** List contents of `/working-dir/`
3. **Fill Each Field:** Provide specific, actionable details
4. **Report Immediately:** Include in agent communication before starting work
5. **Update Plan:** Adjust approach based on discovered context

---

## Examples

### Example 1: Empty Working Directory
```
🔍 WORKING DIRECTORY DISCOVERY:
- Current artifacts reviewed: None found (empty working directory)
- Relevant context found: N/A - starting fresh analysis
- Integration opportunities: N/A - initial work for this issue
- Potential conflicts: None identified
```

### Example 2: Relevant Context Found
```
🔍 WORKING DIRECTORY DISCOVERY:
- Current artifacts reviewed: epic-291-issue-311-execution-plan.md
- Relevant context found: Complete 7-subtask breakdown with specifications
- Integration opportunities: Plan provides all necessary context for execution
- Potential conflicts: None - greenfield skill creation
```

### Example 3: Multi-Agent Coordination
```
🔍 WORKING DIRECTORY DISCOVERY:
- Current artifacts reviewed: backend-api-design.md, test-coverage-gaps.md, session-state.md
- Relevant context found: BackendSpecialist designed new Recipe endpoints; TestEngineer identified coverage gaps
- Integration opportunities: Will implement frontend UI consuming new API; can create tests for identified gaps
- Potential conflicts: Need to coordinate with BackendSpecialist on timing - API implementation in progress
```

---

**Resource Status:** ✅ Ready for Use
**Skill:** working-directory-coordination
**Step:** 1 - Pre-Work Artifact Discovery
