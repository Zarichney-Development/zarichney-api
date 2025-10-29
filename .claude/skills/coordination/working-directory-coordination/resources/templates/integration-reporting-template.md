# Integration Reporting Template

**Purpose:** Standard format for Context Integration Reporting (Step 3 of working-directory-coordination workflow)

**When to Use:** WHEN BUILDING UPON OTHER AGENTS' ARTIFACTS - document how prior work informs current deliverable

---

## Template Format

```
🔗 ARTIFACT INTEGRATION:
- Source artifacts used: [specific files that informed this work]
- Integration approach: [how existing context was incorporated]
- Value addition: [what new insights or progress this provides]
- Handoff preparation: [context prepared for future agents]
```

---

## Field Guidance

### Source Artifacts Used
**What to include:**
- List specific filenames consumed as input
- Note relevant sections or key insights from each
- Acknowledge original agent contributors when known

**Examples:**
- `backend-api-design.md (endpoint specifications), test-coverage-gaps.md (missing test cases)`
- `epic-291-issue-311-execution-plan.md (subtask specifications and acceptance criteria)`
- `coverage-analysis-report.md (TestEngineer's gap analysis), architectural-patterns.md (existing testing patterns)`

### Integration Approach
**What to include:**
- How was existing context incorporated into current work?
- What synthesis or analysis was performed?
- How did prior work shape current decisions?

**Examples:**
- `Used endpoint specifications to design UI components; incorporated missing test cases into test implementation plan`
- `Followed subtask sequence and specifications verbatim; adapted based on progressive discoveries`
- `Synthesized coverage gaps with architectural patterns to create comprehensive test suite addressing identified deficiencies`

### Value Addition
**What to include:**
- What new insights does current artifact provide beyond sources?
- How does this advance progress toward issue objectives?
- What gaps in prior context are now filled?

**Examples:**
- `Added UI mockups and component architecture not present in API design; filled frontend implementation gap`
- `Transformed specifications into executable deliverables with concrete file structures and token measurements`
- `Created 25 new tests addressing all 15 identified gaps plus additional edge cases discovered during implementation`

### Handoff Preparation
**What to include:**
- What context is now ready for future agents?
- What decisions enable subsequent work?
- What dependencies or blockers are resolved?

**Examples:**
- `FrontendSpecialist can now implement UI with clear API contract and backend implementation complete`
- `All subtasks executed - skill ready for integration testing and validation by TestEngineer and DocumentationMaintainer`
- `Coverage gaps resolved - feature now testable; DocumentationMaintainer can update README with confidence`

---

## Usage Instructions

1. **Copy Template:** Use exact template format above
2. **Identify Sources:** List all artifacts consumed as input
3. **Explain Integration:** Describe how sources informed current work
4. **Document Value:** Specify what new insights or progress current work provides
5. **Prepare Handoff:** Identify what's ready for future agents

---

## Examples

### Example 1: Multi-Agent Synthesis
```
🔗 ARTIFACT INTEGRATION:
- Source artifacts used: backend-api-design.md (BackendSpecialist's endpoint specs), test-coverage-gaps.md (TestEngineer's analysis)
- Integration approach: Used endpoint specifications to design UI components consuming new API; incorporated missing test cases into frontend integration test plan
- Value addition: Added UI mockups, component architecture, state management patterns not present in backend design; created comprehensive frontend testing strategy
- Handoff preparation: Complete frontend architecture ready for implementation; TestEngineer can create integration tests with clear component interfaces
```

### Example 2: Specification to Implementation
```
🔗 ARTIFACT INTEGRATION:
- Source artifacts used: epic-291-issue-311-execution-plan.md (complete subtask breakdown), skills-catalog.md and implementation-iterations.md (specifications)
- Integration approach: Followed 7-subtask sequence systematically; used specification details to create metadata, SKILL.md, and all 9 resource files
- Value addition: Transformed specifications into executable skill structure with concrete implementations, token measurements, and validation criteria
- Handoff preparation: Skill creation complete - ready for integration testing with TestEngineer and DocumentationMaintainer; validation can proceed per acceptance criteria
```

### Example 3: Progressive Enhancement
```
🔗 ARTIFACT INTEGRATION:
- Source artifacts used: coverage-analysis-report.md (TestEngineer's 15 identified gaps), architectural-patterns.md (existing testing patterns)
- Integration approach: Synthesized coverage gaps with architectural testing patterns to create comprehensive test suite following project conventions
- Value addition: Created 25 new tests addressing all 15 identified gaps plus additional edge cases discovered during implementation; achieved 95% Recipe service coverage
- Handoff preparation: Coverage gaps resolved - feature now fully testable; DocumentationMaintainer can update README with coverage metrics; BackendSpecialist unblocked for next feature
```

### Example 4: Cross-Domain Coordination
```
🔗 ARTIFACT INTEGRATION:
- Source artifacts used: security-audit-findings.md (SecurityAuditor's vulnerabilities), api-implementation.md (BackendSpecialist's current code)
- Integration approach: Applied SecurityAuditor's recommended remediations to BackendSpecialist's API implementation; integrated OWASP best practices throughout
- Value addition: Resolved all 8 critical vulnerabilities through targeted code changes; added comprehensive input validation and authentication checks
- Handoff preparation: API now security-hardened - SecurityAuditor can validate remediations; FrontendSpecialist can integrate with confidence; TestEngineer can add security test cases
```

---

## Integration Scenarios

### Sequential Agent Workflow
**Pattern:** Agent A completes work → Agent B builds upon it → Agent C extends further

**Integration Focus:**
- How does each agent's work enable the next?
- What context flows through the agent chain?
- How does final deliverable reflect all contributions?

### Parallel Agent Coordination
**Pattern:** Multiple agents work simultaneously on related areas

**Integration Focus:**
- How do parallel efforts avoid conflicts?
- What coordination points ensure compatibility?
- How are parallel deliverables synthesized?

### Iterative Agent Re-engagement
**Pattern:** Same agent engaged multiple times, building upon own prior work

**Integration Focus:**
- How does current work extend prior deliverable?
- What new insights emerged since last engagement?
- How does iteration advance toward completion?

### Cross-Domain Synthesis
**Pattern:** Multiple domain specialists contribute to unified deliverable

**Integration Focus:**
- How are diverse specialist insights integrated?
- What trade-offs or design decisions emerge from synthesis?
- How does unified deliverable serve all domains?

---

## Common Pitfalls to Avoid

### DON'T: Skip source attribution
❌ `- Source artifacts used: Some existing files`
✅ `- Source artifacts used: backend-api-design.md (endpoint specs), test-coverage-gaps.md (missing tests)`

### DON'T: Generic integration descriptions
❌ `- Integration approach: Used existing work`
✅ `- Integration approach: Used endpoint specifications to design UI components consuming new API; incorporated missing test cases into frontend test plan`

### DON'T: Miss value addition
❌ `- Value addition: Completed the work`
✅ `- Value addition: Added UI mockups, component architecture, state management patterns not present in backend design`

### DON'T: Leave handoff vague
❌ `- Handoff preparation: Next agent can proceed`
✅ `- Handoff preparation: Complete frontend architecture ready for implementation; TestEngineer can create integration tests with clear component interfaces`

---

**Resource Status:** ✅ Ready for Use
**Skill:** working-directory-coordination
**Step:** 3 - Context Integration Reporting
