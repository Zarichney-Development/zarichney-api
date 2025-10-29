# Epic #{{EPIC_NUMBER}}: {{EPIC_FULL_NAME}}

**Status:** ARCHIVED
**Completion Date:** {{COMPLETION_DATE_YYYY-MM-DD}}
**Total Iterations:** {{ITERATION_COUNT}} iterations
**Total Issues:** {{ISSUE_COUNT}} issues

---

## Executive Summary

{{EPIC_PURPOSE_OVERVIEW}}

{{KEY_PERFORMANCE_ACHIEVEMENTS}}

{{STRATEGIC_IMPACT_STATEMENT}}

---

## Iterations Overview

{{ITERATION_LIST}}

Example format:
- **Iteration 1 (Issue #X):** {{ONE_LINE_ITERATION_SUMMARY}}
- **Iteration 2 (Issues #Y-Z):** {{ONE_LINE_ITERATION_SUMMARY}}
- **Iteration 3 (Issue #N):** {{ONE_LINE_ITERATION_SUMMARY}}

---

## Key Deliverables

### Major Outcomes
{{MAJOR_OUTCOMES_LIST}}

Example format:
- {{SKILLS_CREATED_COUNT}} skills created: {{SKILL_NAMES}}
- {{COMMANDS_IMPLEMENTED_COUNT}} commands implemented: {{COMMAND_NAMES}}
- {{MODULES_REFACTORED_COUNT}} agent definitions refactored with {{REFACTORING_IMPROVEMENTS}}
- {{SYSTEM_CAPABILITIES_ENHANCED}}
- {{TEAM_WORKFLOWS_IMPROVED}}

### Performance Results
{{PERFORMANCE_METRICS}}

Example format:
- **Context Reduction:** {{CONTEXT_REDUCTION_PERCENTAGE}}% reduction in {{CONTEXT_TYPE}}
- **Token Efficiency:** {{TOKEN_SAVINGS_PERCENTAGE}}% token savings per {{UNIT}}
- **Productivity Gains:** {{PRODUCTIVITY_GAIN_MINUTES}} min/day time savings per agent
- **ROI Calculation:** {{ROI_PERCENTAGE}}% efficiency improvement across team

### Quality Achievements
{{QUALITY_METRICS}}

Example format:
- **Test Coverage:** {{COVERAGE_IMPROVEMENT}}% improvement in {{COVERAGE_AREA}}
- **Standards Compliance:** {{STANDARDS_ACHIEVEMENTS}}
- **Technical Debt Reduction:** {{DEBT_REDUCTION_METRICS}}
- **AI Sentinel Integration:** {{SENTINEL_ENHANCEMENTS}}

---

## Documentation Network

### Committed Documentation
{{COMMITTED_DOCUMENTATION_LINKS}}

Example format:
- [{{DOCUMENT_NAME}}]({{RELATIVE_PATH_TO_DOCS_DEVELOPMENT}}) - {{DOCUMENT_PURPOSE}}
- [{{STANDARDS_DOCUMENT}}]({{RELATIVE_PATH_TO_DOCS_STANDARDS}}) - {{STANDARDS_UPDATES}}
- [{{MODULE_README}}]({{RELATIVE_PATH_TO_MODULE}}) - {{MODULE_CHANGES}}

### Performance Documentation (if applicable)
{{PERFORMANCE_DOCUMENTATION_LINKS}}

Example format:
- [Epic{{EPIC_NUMBER}}PerformanceAchievements.md]({{RELATIVE_PATH}}) - Comprehensive performance analysis
- [TokenTrackingMethodology.md]({{RELATIVE_PATH}}) - Token measurement approach validation
- [PerformanceMonitoringStrategy.md]({{RELATIVE_PATH}}) - Ongoing performance excellence framework

---

## Archive Contents

### Specs Directory
{{SPECS_DIRECTORY_SUMMARY}}

Example format:
This archive contains {{SPEC_FILE_COUNT}} specification files documenting epic planning and requirements:
- **README.md**: Epic overview with iteration structure and completion criteria
- **Iteration Specs**: {{ITERATION_SPEC_COUNT}} iteration specification files detailing phase-by-phase deliverables
- **Templates**: {{TEMPLATE_COUNT}} templates created during epic execution
- **Supporting Documentation**: {{SUPPORTING_DOC_COUNT}} additional planning documents

### Working Directory
{{WORKING_DIRECTORY_SUMMARY}}

Example format:
This archive contains {{ARTIFACT_COUNT}} artifacts from epic execution across {{ARTIFACT_CATEGORIES_COUNT}} categories:
- **Execution Plans** ({{EXECUTION_PLAN_COUNT}}): Iteration planning and task breakdown
- **Completion Reports** ({{COMPLETION_REPORT_COUNT}}): Agent deliverable summaries and team handoffs
- **Validation Reports** ({{VALIDATION_REPORT_COUNT}}): ComplianceOfficer quality gate validations
- **Coordination Artifacts** ({{COORDINATION_ARTIFACT_COUNT}}): Multi-agent workflow coordination documents

### Navigation Guidance
- **Start with `Specs/README.md`**: Provides epic overview, iteration structure, and completion timeline
- **Review `working-dir/` artifacts chronologically**: Follow epic execution progression through iteration plans and completion reports
- **Consult committed documentation**: Reference links above for integrated documentation in main repository

---

## Placeholder Guidance

### Required Placeholders

**{{EPIC_NUMBER}}:**
- **Description:** Epic numeric identifier matching GitHub issue and spec directory
- **Format:** Integer (e.g., 291, 246, 123)
- **Example:** 291

**{{EPIC_FULL_NAME}}:**
- **Description:** Complete epic name from spec directory
- **Format:** Title Case with Descriptive Phrase (e.g., "Agent Skills & Slash Commands Integration")
- **Example:** Agent Skills & Slash Commands Integration

**{{COMPLETION_DATE_YYYY-MM-DD}}:**
- **Description:** Date epic archiving completed
- **Format:** YYYY-MM-DD (ISO 8601)
- **Example:** 2025-10-27

**{{ITERATION_COUNT}}:**
- **Description:** Total number of iterations in epic
- **Format:** Integer
- **Example:** 5

**{{ISSUE_COUNT}}:**
- **Description:** Total number of GitHub issues in epic
- **Format:** Integer
- **Example:** 12

**{{EPIC_PURPOSE_OVERVIEW}}:**
- **Description:** 2-3 sentence summary of epic purpose and outcomes
- **Format:** Paragraph focusing on "what" and "why"
- **Example:** "Implemented progressive loading architecture for AI agent coordination through systematic skills and commands framework. Achieved 50-51% context reduction and 144-328% session token savings. Established scalable foundation for continuous AI system optimization."

**{{KEY_PERFORMANCE_ACHIEVEMENTS}}:**
- **Description:** Quantified performance metrics from epic execution
- **Format:** Bulleted list or paragraph with specific percentages/numbers
- **Example:** "Context reduction: 50-51% across agent definitions. Session token savings: 144-328% improvement. Productivity gains: 42-61 min/day per agent."

**{{STRATEGIC_IMPACT_STATEMENT}}:**
- **Description:** Business value and strategic importance of epic achievements
- **Format:** 1-2 sentences focusing on organizational impact
- **Example:** "Progressive loading architecture positions team for sustained AI productivity excellence with measurable ROI and scalable optimization framework."

### Optional Placeholders (Use if applicable)

**{{ITERATION_LIST}}:**
- **Description:** Bulleted list of all iterations with issue ranges and summaries
- **Format:** Markdown bulleted list
- **Example:** See Iterations Overview section above

**{{MAJOR_OUTCOMES_LIST}}:**
- **Description:** Bulleted list of primary epic deliverables
- **Format:** Markdown bulleted list with counts and names
- **Example:** See Key Deliverables > Major Outcomes section above

**{{PERFORMANCE_METRICS}}:**
- **Description:** Detailed performance results with quantified improvements
- **Format:** Markdown bulleted list with metrics and percentages
- **Example:** See Key Deliverables > Performance Results section above

**{{QUALITY_METRICS}}:**
- **Description:** Quality achievements including test coverage, standards compliance
- **Format:** Markdown bulleted list
- **Example:** See Key Deliverables > Quality Achievements section above

**{{COMMITTED_DOCUMENTATION_LINKS}}:**
- **Description:** Markdown links to documentation committed in `./Docs/Development/` and `./Docs/Standards/`
- **Format:** Markdown link list with relative paths
- **Example:** See Documentation Network > Committed Documentation section above

**{{PERFORMANCE_DOCUMENTATION_LINKS}}:**
- **Description:** Markdown links to performance analysis documents (if epic performance-critical)
- **Format:** Markdown link list with relative paths
- **Example:** See Documentation Network > Performance Documentation section above

**{{SPECS_DIRECTORY_SUMMARY}}:**
- **Description:** Paragraph summarizing spec directory contents with file counts
- **Format:** Paragraph + bulleted list
- **Example:** See Archive Contents > Specs Directory section above

**{{WORKING_DIRECTORY_SUMMARY}}:**
- **Description:** Paragraph summarizing working directory artifacts with counts and categories
- **Format:** Paragraph + bulleted list
- **Example:** See Archive Contents > Working Directory section above

---

## Validation Checklist

Before finalizing archive README, verify:

- [ ] All required placeholders replaced with epic-specific content
- [ ] Epic number and name match spec directory naming exactly
- [ ] Completion date accurate (YYYY-MM-DD format)
- [ ] Iteration count and issue count verified via GitHub or spec directory
- [ ] Executive summary concise (2-3 sentences) and accurate
- [ ] Iterations overview lists all iterations with issue references
- [ ] Key deliverables categorized into outcomes/performance/quality
- [ ] Documentation network links functional (relative paths correct)
- [ ] Archive contents summary provides clear navigation guidance
- [ ] Performance documentation linked if epic performance-critical
- [ ] No placeholder syntax ({{...}}) remaining in final README
- [ ] Markdown formatting correct (links, lists, headings)
