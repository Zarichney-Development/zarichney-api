# Issue #304 Execution Plan: Workflow Commands - Create Issue & Merge Coverage PRs

**Epic:** #291 - Agent Skills & Slash Commands Integration
**Issue:** #304 - Iteration 2.4: Workflow Commands - Create Issue & Merge Coverage PRs
**Date:** 2025-10-26
**Status:** 🔄 IN PROGRESS

---

## Issue Context

**Issue #304:** Iteration 2.4 - Final issue in Iteration 2 (Meta-Skills & Commands)
**Location:** `section/iteration-2` branch (existing from Issues #307, #306, #305)
**Dependencies Met:**
- ✅ Issue #305 (Workflow Commands - Status & Coverage Report) - COMPLETE

**Blocks:**
- Issue #303 (Iteration 3.1: Skills & Commands Development Guides)

---

## Core Issue

**SPECIFIC TECHNICAL PROBLEM:** Developers manually create GitHub issues through UI (5 min per issue) and manually trigger Coverage Excellence Merge Orchestrator workflows. Need CLI commands for automated issue creation with context collection and safe workflow triggering.

**SUCCESS CRITERIA:**
1. `/create-issue` and `/merge-coverage-prs` execute successfully with all usage patterns
2. Argument parsing robust with clear error messages
3. gh CLI integration functional for issue creation and workflow triggering
4. Output formatting clear and actionable
5. Workflow integration validated end-to-end
6. Documentation complete with comprehensive examples

---

## Execution Strategy

### Subtask 1: Implement /create-issue Command
**Agent:** WorkflowEngineer (GitHub automation expertise)
**Intent:** COMMAND - Direct implementation of issue creation command
**Authority:** Full implementation authority over `.claude/commands/create-issue.md`
**Estimated Effort:** 2-3 hours

**Deliverables:**
- `.claude/commands/create-issue.md` with complete frontmatter and implementation
- Integration with github-issue-creation skill
- Arguments: `<type> "<title>" [--template TEMPLATE] [--label LABEL] [--dry-run]`
- Issue types: feature, bug, epic, debt, docs
- Template selection and gh CLI issue creation
- Comprehensive usage examples

**Core Implementation Requirements:**
- YAML frontmatter: `description`, `argument-hint`, `category`, `requires-skills: ["github-issue-creation"]`
- Two required positional arguments: type and title
- Optional named arguments: --template, --label (repeatable), --milestone, --assignee
- --dry-run flag for preview
- Integration with github-issue-creation skill for context collection
- gh CLI: `gh issue create --title "$title" --body-file /tmp/issue-body.md --label "$labels"`
- Error handling: invalid type, missing title, gh CLI issues, template not found

### Subtask 2: Implement /merge-coverage-prs Command
**Agent:** WorkflowEngineer (CI/CD workflow expertise)
**Intent:** COMMAND - Direct implementation of workflow trigger command
**Authority:** Full implementation authority over `.claude/commands/merge-coverage-prs.md`
**Estimated Effort:** 1-2 hours

**Deliverables:**
- `.claude/commands/merge-coverage-prs.md` with complete frontmatter and implementation
- Workflow triggering via gh CLI
- Arguments: `[--dry-run] [--no-dry-run] [--max N] [--labels LABELS] [--watch]`
- Default: --dry-run true for safety
- gh workflow run integration
- Real-time monitoring with --watch

**Core Implementation Requirements:**
- YAML frontmatter: `description`, `argument-hint`, `category`
- Optional skill reference (coverage-epic-consolidation) - future enhancement
- Boolean flags: --dry-run (default: true), --no-dry-run
- Named arguments: --max N (1-50, default: 8), --labels (comma-separated)
- --watch flag for real-time monitoring
- gh CLI: `gh workflow run "Testing Coverage Merge Orchestrator" --field dry_run=${dry_run} --field max_prs=${max}`
- Workflow monitoring: `gh run list --workflow="Testing Coverage Merge Orchestrator" --limit 1`
- Error handling: workflow not found, invalid max value, gh CLI issues

### Subtask 3: Validation & Testing
**Agent:** TestEngineer (quality validation)
**Intent:** ANALYSIS - Validation report for command functionality
**Authority:** Working directory validation artifact
**Estimated Effort:** 1 hour

**Deliverables:**
- Working directory validation report
- Test execution for all usage patterns
- End-to-end integration testing
- Quality gate confirmation

---

## Command Specifications

### /create-issue Command

**Category:** GitHub Automation
**Skill Dependency:** `.claude/skills/github/github-issue-creation/` (MANDATORY)

**Frontmatter:**
```yaml
---
description: "Create comprehensive GitHub issue with automated context collection"
argument-hint: "<type> <title> [--template TEMPLATE] [--label LABEL]"
category: "workflow"
requires-skills: ["github-issue-creation"]
---
```

**Usage Examples:**
```bash
/create-issue feature "Add recipe tagging system"
/create-issue bug "Login fails with expired token"
/create-issue epic "Backend API v2 migration" --label architecture
/create-issue debt "Refactor authentication service"
/create-issue docs "Document WebSocket patterns"
/create-issue feature "New feature" --template custom-template.md
/create-issue feature "Feature name" --label frontend --label enhancement
/create-issue feature "Title" --dry-run  # Preview only
```

**Integration Points:**
- github-issue-creation skill: Context collection, template application
- GitHubLabelStandards.md: Automated label compliance
- Docs/Templates/: Issue template sourcing
- gh CLI: Issue creation via `gh issue create`

### /merge-coverage-prs Command

**Category:** Epic Automation
**Skill Dependency:** Optional (coverage-epic-consolidation - future)

**Frontmatter:**
```yaml
---
description: "Trigger Coverage Excellence Merge Orchestrator workflow"
argument-hint: "[--dry-run] [--max N] [--labels LABELS]"
category: "workflow"
---
```

**Usage Examples:**
```bash
/merge-coverage-prs                                    # Dry-run preview (default)
/merge-coverage-prs --dry-run --max 15                 # Dry-run with 15 PRs
/merge-coverage-prs --no-dry-run --max 8               # Live execution
/merge-coverage-prs --labels "coverage,testing,ai-task"  # Custom labels
/merge-coverage-prs --no-dry-run --max 50              # Maximum consolidation
/merge-coverage-prs --watch                            # Monitor execution
```

**Integration Points:**
- .github/workflows/testing-coverage-merger.yml: Workflow execution
- Scripts/validate-coverage-automation.sh: Epic validation
- gh CLI: Workflow triggering via `gh workflow run`
- gh CLI: Monitoring via `gh run watch` and `gh run list`

---

## Quality Gates

**Per-Command Validation:**
1. All usage examples execute successfully
2. Argument parsing handles all combinations
3. Error messages clear and actionable
4. Output formatting appropriate
5. gh CLI integration functional
6. Skill integration working (for /create-issue)
7. Workflow triggering validated (for /merge-coverage-prs)
8. Documentation comprehensive

**Acceptance Criteria Validation:**
- ✅ Both commands execute successfully with all usage patterns
- ✅ Argument parsing robust with clear error messages
- ✅ gh CLI integration functional for issue creation and workflow triggering
- ✅ Output formatting clear and actionable
- ✅ Workflow integration validated end-to-end
- ✅ Documentation complete with comprehensive examples

---

## Commit Strategy

**Per-Subtask Commits:**
1. `feat: implement /create-issue command (#304)` - After Subtask 1
2. `feat: implement /merge-coverage-prs command (#304)` - After Subtask 2
3. Validation report to working-dir only (gitignored)

**Branch:** `section/iteration-2` (existing)
**Section Completion:** After Issue #304, prepare for ComplianceOfficer validation and section PR

---

## Section Completion Checklist

After Issue #304 complete, Iteration 2 will be finished (all 4 issues: #307, #306, #305, #304).

**Pre-PR Actions:**
1. Run build validation: `dotnet build zarichney-api.sln`
2. Run test suite: `./Scripts/run-test-suite.sh report summary`
3. Invoke ComplianceOfficer for section-level review
4. Address any compliance issues
5. Push section/iteration-2 to remote
6. Create Section PR: `epic/skills-commands-291` ← `section/iteration-2`

**Section PR Details:**
- **Title:** `epic: complete Iteration 2 - Meta-Skills & Commands (#291)`
- **Body:** Lists all 4 completed issues (#307, #306, #305, #304)
- **Labels:** `type: epic-task`, `priority: high`, `status: review`

---

## Success Metrics

**Developer Productivity:**
- /create-issue: ~4 min time savings (5 min → 1 min, 80% reduction)
- /merge-coverage-prs: ~10 min time savings (manual trigger + monitoring)
- Combined with /workflow-status and /coverage-report: ~19-24 min/day savings

**Iteration 2 Completion:**
- All 4 workflow commands implemented
- All 3 meta-skills operational
- Foundation for scalable agent/skill/command creation

---

**Execution Plan Status:** ✅ COMPLETE
**Ready to Execute:** Subtask 1 - Implement /create-issue command
**Agent Engagement:** WorkflowEngineer with full implementation authority
