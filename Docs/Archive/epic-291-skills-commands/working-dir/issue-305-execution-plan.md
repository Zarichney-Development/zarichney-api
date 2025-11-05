# Issue #305 Execution Plan: Workflow Commands - Status & Coverage Report

**Epic:** #291 - Agent Skills & Slash Commands Integration
**Iteration:** 2.3 of 2.4 in Iteration 2
**Date:** 2025-10-26
**Status:** 🔄 IN PROGRESS

---

## Issue Context

**Issue #305:** Iteration 2.3: Workflow Commands - Status & Coverage Report
**Location:** `section/iteration-2` branch (exists from Issue #307, #306)
**Dependencies Met:**
- ✅ Issue #309 (GitHub Workflow Skill) - COMPLETE
- ✅ Issue #306 (Command Creation Meta-Skill) - COMPLETE

**Blocks:**
- Issue #304 (Iteration 2.4: Workflow Commands - Create Issue & Merge Coverage PRs)
- Issue #294 (Iteration 5.1: CLAUDE.md Optimization)

---

## Core Issue

**SPECIFIC TECHNICAL PROBLEM:** Developers must navigate GitHub UI to check CI/CD workflow status and analyze test coverage data, consuming 2-3 minutes per check and breaking development flow. Need CLI-accessible commands providing immediate workflow visibility and coverage analytics.

**SUCCESS CRITERIA:**
1. `/workflow-status` provides real-time CI/CD monitoring via gh CLI
2. `/coverage-report` delivers accurate coverage analytics from test artifacts
3. Both commands handle all argument patterns with clear error messages
4. Output formatting appropriate for human consumption
5. Integration with existing Scripts/run-test-suite.sh validated
6. Documentation complete with comprehensive examples

---

## Execution Strategy

### Subtask Breakdown

#### Subtask 1: Implement /workflow-status Command
**Agent:** WorkflowEngineer (specialist with workflow domain expertise)
**Intent:** COMMAND - Direct implementation of workflow monitoring command
**Authority:** Full implementation authority over `.claude/commands/workflow-status.md`
**Estimated Effort:** 3-4 hours

**Deliverables:**
- `.claude/commands/workflow-status.md` with complete frontmatter and implementation
- gh CLI integration for workflow run listing
- Arguments: `[workflow-name] [--details] [--limit N] [--branch BRANCH]`
- Status filtering and detailed log viewing
- Real-time monitoring capabilities
- Comprehensive usage examples

**Core Implementation Requirements:**
- YAML frontmatter with description, argument-hint, category
- Bash-based argument parsing with validation
- gh CLI commands for workflow operations:
  - `gh run list` for workflow run listing
  - `gh run view` for detailed logs
  - `gh run watch` for real-time monitoring
- Error handling for missing gh CLI, authentication failures, invalid arguments
- Clear output formatting with status indicators (🔄 ✅ ❌ ⚠️)
- Help text with all usage patterns

**Quality Gates:**
- All usage examples from Issue #305 execute successfully
- Argument validation prevents invalid inputs with helpful errors
- Output is clear and actionable for developers
- Integration with GitHub Actions workflows validated

#### Subtask 2: Implement /coverage-report Command
**Agent:** WorkflowEngineer (specialist with CI/CD and test artifact expertise)
**Intent:** COMMAND - Direct implementation of coverage analytics command
**Authority:** Full implementation authority over `.claude/commands/coverage-report.md`
**Estimated Effort:** 3-4 hours

**Deliverables:**
- `.claude/commands/coverage-report.md` with complete frontmatter and implementation
- Artifact retrieval via gh CLI
- Coverage data parsing with jq
- Arguments: `[format] [--compare] [--epic] [--threshold N] [--module MODULE]`
- Formats: summary, detailed, json
- Baseline comparison capabilities
- Epic progression analytics

**Core Implementation Requirements:**
- YAML frontmatter with description, argument-hint, category
- Bash-based argument parsing with validation
- Integration with Scripts/run-test-suite.sh for latest coverage
- Artifact retrieval: `gh run download --name test-results-latest`
- Coverage parsing: `jq '.coverage' TestResults/test-results.json`
- Comparison logic for baseline differential
- Epic PR listing: `gh pr list --base continuous/testing-excellence`
- Threshold validation and warnings
- Clear output formatting for all formats

**Quality Gates:**
- All format options (summary/detailed/json) functional
- Comparison with baseline accurate
- Epic progression metrics correct
- Threshold validation prevents regressions
- Output appropriate for both human and machine consumption

#### Subtask 3: Comprehensive Documentation
**Agent:** DocumentationMaintainer (documentation standards compliance)
**Intent:** COMMAND - Direct documentation creation for command usage
**Authority:** Documentation enhancement within command files
**Estimated Effort:** 1-2 hours

**Deliverables:**
- Enhanced usage examples within both command files
- Integration point documentation
- Error scenario documentation
- Best practices for command usage
- Workflow integration guidance

**Quality Gates:**
- Documentation enables command usage without external clarification
- All edge cases and error scenarios covered
- Integration points clearly explained
- Cross-references to relevant standards and guides

#### Subtask 4: Validation & Testing
**Agent:** TestEngineer (quality validation)
**Intent:** ANALYSIS - Validation report for command functionality
**Authority:** Working directory validation artifact
**Estimated Effort:** 1-2 hours

**Deliverables:**
- Working directory validation report
- Test execution for all usage patterns
- Edge case validation
- Integration testing results
- Quality gate confirmation

**Quality Gates:**
- All 6 acceptance criteria from Issue #305 validated
- Commands execute successfully in clean environment
- Error handling robust and helpful
- Output formatting appropriate
- Integration with existing tools validated

---

## Command Specifications (From Epic #291 Catalog)

### /workflow-status

**Category:** CI/CD Monitoring
**Frontmatter:**
```yaml
---
description: "Check current status of GitHub Actions workflows"
argument-hint: "[workflow-name] [--details] [--limit N] [--branch BRANCH]"
category: "workflow"
---
```

**Usage Examples:**
```bash
/workflow-status                                    # 5 most recent runs, all workflows
/workflow-status build.yml --limit 10               # Last 10 runs of specific workflow
/workflow-status --details                          # Interactive selection with logs
/workflow-status "Testing Coverage Merge Orchestrator"  # Monitor specific workflow
/workflow-status --branch main --limit 10           # Branch-specific filtering
```

**Implementation Tool:** gh CLI
**Skill Dependency:** None (direct gh CLI, optional future monitoring skill)

### /coverage-report

**Category:** Testing & Quality
**Frontmatter:**
```yaml
---
description: "Fetch latest test coverage data and trends"
argument-hint: "[format] [--compare] [--epic] [--threshold N] [--module MODULE]"
category: "testing"
---
```

**Usage Examples:**
```bash
/coverage-report                          # Current coverage (summary format)
/coverage-report json                     # Machine-readable output
/coverage-report detailed                 # Comprehensive breakdown
/coverage-report --compare                # Compare with baseline
/coverage-report --epic                   # Epic progression metrics
/coverage-report --threshold 30           # Threshold validation
/coverage-report --module Services        # Module-specific coverage
```

**Implementation Tool:** bash + gh CLI + jq
**Skill Dependency:** `.claude/skills/testing/coverage-analysis/` (optional future skill)

---

## Integration Points

**Workflow Commands:**
- GitHub Actions: Query workflow run data via `gh run` commands
- Local Development: Immediate feedback without GitHub UI navigation
- CI/CD Pipelines: Monitoring automation from command line

**Coverage Commands:**
- Scripts/run-test-suite.sh: Comprehensive test execution
- TestResults/ artifacts: Coverage data storage
- Coverage Excellence Epic: Progression tracking at `continuous/testing-excellence`
- ComplianceOfficer: Pre-PR quality gates

---

## Quality Assurance Strategy

**Per-Command Validation:**
1. **Functionality:** Execute all usage examples successfully
2. **Argument Parsing:** Validate all argument combinations
3. **Error Handling:** Test invalid inputs with clear error messages
4. **Output Formatting:** Verify human-readable and machine-parsable formats
5. **Integration:** Confirm gh CLI and script integrations work
6. **Documentation:** Ensure examples comprehensive and accurate

**Integration Testing:**
1. Workflow status retrieval from live repository
2. Coverage data parsing from actual test results
3. Comparison with baseline functionality
4. Epic PR listing accuracy
5. Threshold validation logic

**Acceptance Criteria Validation:**
- ✅ Both commands execute successfully with all usage patterns
- ✅ Argument parsing robust with clear error messages
- ✅ gh CLI integration functional for workflow and artifact retrieval
- ✅ Output formatting clear and actionable
- ✅ Workflow integration validated
- ✅ Documentation complete with examples

---

## Commit Strategy

**Per-Subtask Commits:**
1. `feat: implement /workflow-status command (#305)` - After Subtask 1
2. `feat: implement /coverage-report command (#305)` - After Subtask 2
3. `docs: enhance workflow command documentation (#305)` - After Subtask 3
4. `test: validate workflow commands functionality (#305)` - After Subtask 4 (validation report to working-dir only)

**Branch:** `section/iteration-2` (existing from Issue #307, #306)
**Conventional Commit:** feat/docs/test prefixes with #305 reference

---

## Risk Mitigation

**Potential Risks:**
1. **gh CLI Authentication:** Mitigated by clear error messages if not authenticated
2. **Artifact Availability:** Mitigated by checking for test results before parsing
3. **jq Dependency:** Mitigated by dependency check and installation guidance
4. **Coverage Data Format:** Mitigated by validation against actual test results structure

**Contingency Plans:**
- If gh CLI unavailable: Provide installation instructions
- If artifacts missing: Guide user to run tests first
- If jq unavailable: Provide alternative parsing or installation steps
- If coverage format changes: Update parsing logic and document structure

---

## Success Metrics

**Developer Productivity:**
- Time savings: ~2 min per workflow check (GitHub UI navigation eliminated)
- Time savings: ~3 min per coverage analysis (artifact retrieval automated)
- Workflow efficiency: 60-80% reduction in manual GitHub UI operations

**Quality Gates:**
- All 6 acceptance criteria met
- Commands functional in clean environment
- Error messages helpful and actionable
- Output appropriate for target audience
- Integration seamless with existing tools

---

## Next Steps After Completion

**Immediate (within Iteration 2):**
- Issue #304: Implement /create-issue and /merge-coverage-prs commands

**Section Completion (after Issue #304):**
- ComplianceOfficer validation (section-level)
- Section PR creation: `epic/skills-commands-291` ← `section/iteration-2`

**Epic Progression:**
- Iteration 3: Documentation Alignment
- Iteration 4: Agent Refactoring
- Iteration 5: Integration & Validation

---

**Execution Plan Status:** ✅ COMPLETE
**Ready to Execute:** Subtask 1 - Implement /workflow-status command
**Agent Engagement:** WorkflowEngineer with full implementation authority
