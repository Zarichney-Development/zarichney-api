# Workflow Commands Catalog - Epic #291

**Last Updated:** 2025-10-25
**Purpose:** Complete catalog of all 4 priority slash commands with comprehensive specifications

---

## Command Philosophy

**Commands vs. Skills:**
- **Commands:** User-facing CLI interfaces providing quick access to workflows
- **Skills:** Implementation logic containing workflows, business rules, resources
- **Integration:** Commands delegate to skills for execution while handling UX concerns

**Command Responsibilities:**
- Argument parsing and validation
- User-friendly error messages
- Output formatting and presentation
- Workflow triggering and monitoring

**Skill Responsibilities:**
- Workflow execution logic
- Business rule enforcement
- Resource access (templates, examples, docs)
- Technical implementation details

---

## Priority 1 Workflow Commands

### Command 1: /workflow-status

**Category:** CI/CD Monitoring
**Priority:** P1 - High developer value
**Location:** `.claude/commands/workflow-status.md`

#### Purpose
Real-time GitHub Actions workflow status monitoring with detailed execution logs and failure diagnostics.

#### Command Structure
```markdown
---
description: "Check current status of GitHub Actions workflows"
argument-hint: "[workflow-name] [--details] [--limit N]"
category: "workflow"
---
```

#### Usage Examples
```bash
# List 5 most recent runs across all workflows
/workflow-status

# Last 10 runs of specific workflow
/workflow-status build.yml --limit 10

# Interactive selection with detailed logs
/workflow-status --details

# Monitor specific workflow
/workflow-status "Testing Coverage Merge Orchestrator"
```

#### Arguments
- `[workflow-name]` (optional): Specific workflow to monitor (default: all workflows)
- `--details` (flag): Show detailed logs and step-level status
- `--limit N` (optional): Number of recent runs to display (default: 5)
- `--branch BRANCH` (optional): Filter by branch (default: current branch)

#### Implementation
**Tool:** gh CLI
**Execution:**
```bash
# Default: All recent runs
gh run list --limit ${limit:-5}

# Specific workflow
gh run list --workflow="${workflow_name}" --limit ${limit:-10}

# Detailed view
gh run view ${run_id} --log

# Branch filtering
gh run list --branch ${branch} --limit ${limit}
```

#### Output Includes
- Run status (queued, in_progress, completed)
- Conclusion (success, failure, cancelled, skipped)
- Duration and timestamps
- Job-level status breakdown (with --details)
- Failure diagnostics and error messages
- Workflow run URLs for deep-dive

#### Use Cases
- Quick CI/CD status check during development
- Monitoring long-running workflows (coverage automation, deployments)
- Debugging workflow failures with detailed logs
- Verifying automation execution after push
- Team coordination around workflow status

#### Integration Points
- GitHub Actions: Query workflow run data via gh CLI
- Local Development: Immediate feedback without GitHub UI navigation
- CI/CD Pipelines: Monitoring automation from command line

**Skill Dependency:** None (direct gh CLI usage, optional future skill for advanced monitoring)

**Estimated Development:** 2 days
**Dependencies:** gh CLI installed and authenticated

---

### Command 2: /coverage-report

**Category:** Testing & Quality
**Priority:** P1 - Critical for coverage progression
**Location:** `.claude/commands/coverage-report.md`

#### Purpose
Quick test coverage analysis with trend tracking, gap identification, and epic progression metrics.

#### Command Structure
```markdown
---
description: "Fetch latest test coverage data and trends"
argument-hint: "[format] [--compare] [--epic] [--threshold N]"
category: "testing"
---
```

#### Usage Examples
```bash
# Current coverage percentages (default: summary)
/coverage-report

# Machine-readable JSON output
/coverage-report json

# Detailed coverage breakdown
/coverage-report detailed

# Compare with baseline
/coverage-report --compare

# Epic progression metrics
/coverage-report --epic

# Threshold validation
/coverage-report --threshold 30
```

#### Arguments
- `[format]` (optional): Output format (summary|detailed|json, default: summary)
- `--compare` (flag): Compare current coverage with baseline
- `--epic` (flag): Show epic progression metrics and PR inventory
- `--threshold N` (optional): Coverage threshold percentage for validation (default: 24%)
- `--module MODULE` (optional): Focus on specific module coverage

#### Implementation
**Tool:** bash + gh CLI + jq
**Skill Dependency:** `.claude/skills/testing/coverage-analysis/`
**Execution:**
```bash
# Fetch latest test results from artifacts
gh run download --name test-results-latest

# Parse coverage data
jq '.coverage' TestResults/test-results.json

# Compare with baseline (if --compare)
jq '.coverage' TestResults/baseline.json
diff <(jq '.coverage' TestResults/baseline.json) <(jq '.coverage' TestResults/test-results.json)

# Epic progression (if --epic)
gh pr list --base epic/testing-coverage --json labels,number,title

# Threshold validation
if [ $line_coverage -lt $threshold ]; then
  echo "⚠️ Coverage below threshold: ${line_coverage}% < ${threshold}%"
fi
```

#### Output Includes
- Line coverage percentage (overall and per-module)
- Branch coverage percentage
- Module-specific coverage breakdowns
- Coverage trends (with --compare): Δ from baseline
- Epic PR inventory (with --epic): Open coverage PRs
- Gap identification: Modules below threshold
- Quality metrics: Test health indicators

#### Use Cases
- Pre-PR coverage validation
- Coverage progression tracking during development
- Epic advancement monitoring (Coverage Excellence)
- Quality gate verification before merge
- Module-specific coverage deep-dive

#### Integration Points
- Scripts/run-test-suite.sh: Comprehensive test execution
- TestResults/ artifacts: Coverage data storage
- Coverage Excellence Epic: Progression tracking
- ComplianceOfficer: Pre-PR quality gates

**Skill Dependency:** coverage-analysis skill (wraps /test-report with coverage focus)

**Estimated Development:** 2 days
**Dependencies:** gh CLI, jq, Scripts/run-test-suite.sh

---

### Command 3: /create-issue

**Category:** GitHub Automation
**Priority:** P1 - Eliminates manual effort
**Location:** `.claude/commands/create-issue.md`

#### Purpose
Comprehensive GitHub issue creation with automated context collection, template application, and proper labeling.

#### Command Structure
```markdown
---
description: "Create comprehensive GitHub issue with automated context collection"
argument-hint: "<type> <title> [--template TEMPLATE] [--label LABEL]"
category: "workflow"
requires-skills: ["github-issue-creation"]
---
```

#### Usage Examples
```bash
# Feature request with automated context
/create-issue feature "Add recipe tagging system"

# Bug report with reproduction context
/create-issue bug "Login fails with expired token"

# Epic milestone planning
/create-issue epic "Backend API v2 migration" --label architecture

# Technical debt tracking
/create-issue debt "Refactor authentication service"

# Documentation request
/create-issue docs "Document WebSocket patterns"

# Custom template override
/create-issue feature "New feature" --template custom-template.md

# Multiple labels
/create-issue feature "Feature name" --label frontend --label enhancement
```

#### Arguments
- `<type>` (required): Issue type (feature|bug|epic|debt|docs)
- `<title>` (required): Issue title (clear, actionable)
- `--template TEMPLATE` (optional): Override default template for type
- `--label LABEL` (optional, repeatable): Additional labels beyond type defaults
- `--milestone MILESTONE` (optional): Link to specific milestone/epic
- `--assignee USER` (optional): Assign to specific user
- `--dry-run` (flag): Preview issue without creating

#### Implementation
**Tool:** gh CLI + skill delegation
**Skill Dependency:** `.claude/skills/github/github-issue-creation/`
**Execution:**
```bash
# Load github-issue-creation skill for workflow
claude load-skill github-issue-creation

# Skill executes 4-phase workflow:
# 1. Context Collection (automated)
# 2. Template Selection (based on type)
# 3. Issue Construction (with labels, milestone)
# 4. Validation & Submission (via gh CLI)

# Dry-run preview
if [ "$dry_run" = "true" ]; then
  echo "Issue Preview:"
  cat /tmp/issue-preview.md
else
  gh issue create --title "$title" --body-file /tmp/issue-body.md --label "$labels"
fi
```

#### Output Includes
- Issue number and URL
- Applied labels (type-based + custom)
- Linked milestone (if applicable)
- Assigned users (if specified)
- Related issues discovered and linked
- Context collection summary

#### Use Cases
- Feature request creation from user requirements (5 min → 1 min)
- Bug documentation with comprehensive reproduction steps
- Epic milestone planning with strategic context
- Technical debt tracking systematically
- Documentation gap identification

#### Integration Points
- github-issue-creation skill: Full workflow orchestration
- GitHubLabelStandards.md: Label compliance automation
- /Docs/Templates/: Issue template sourcing
- gh CLI: Issue creation and management

**Skill Dependency:** github-issue-creation (MANDATORY - skill contains workflow logic)

**Estimated Development:** 2 days (1 day command wrapper, 1 day skill integration)
**Dependencies:** gh CLI, github-issue-creation skill operational

---

### Command 4: /merge-coverage-prs

**Category:** Epic Automation
**Priority:** P1 - Critical for coverage epic
**Location:** `.claude/commands/merge-coverage-prs.md`

#### Purpose
Trigger Coverage Excellence Merge Orchestrator for multi-PR consolidation with AI conflict resolution.

#### Command Structure
```markdown
---
description: "Trigger Coverage Excellence Merge Orchestrator workflow"
argument-hint: "[--dry-run] [--max N] [--labels LABELS]"
category: "workflow"
requires-skills: ["coverage-epic-consolidation"]
---
```

#### Usage Examples
```bash
# Dry-run consolidation (default: safe preview)
/merge-coverage-prs

# Dry-run with 15 PRs
/merge-coverage-prs --dry-run --max 15

# Live execution (8 PRs)
/merge-coverage-prs --no-dry-run --max 8

# Custom label filtering
/merge-coverage-prs --labels "coverage,testing,ai-task"

# Maximum consolidation (up to 50 PRs)
/merge-coverage-prs --no-dry-run --max 50

# Monitor execution
/merge-coverage-prs --watch
```

#### Arguments
- `--dry-run` (flag, default: true): Preview without executing merges
- `--no-dry-run` (flag): Execute live PR consolidation
- `--max N` (optional): Maximum PRs to consolidate (1-50, default: 8)
- `--labels LABELS` (optional): Comma-separated label filter with OR logic (default: "type: coverage,coverage,testing")
- `--watch` (flag): Monitor workflow execution in real-time

#### Implementation
**Tool:** gh CLI + workflow trigger
**Skill Dependency:** `.claude/skills/cicd/coverage-epic-consolidation/`
**Execution:**
```bash
# Trigger Coverage Excellence Merge Orchestrator workflow
gh workflow run "Testing Coverage Merge Orchestrator" \
  --field dry_run=${dry_run:-true} \
  --field max_prs=${max:-8} \
  --field pr_label_filter="${labels:-type: coverage,coverage,testing}"

# Monitor execution (if --watch)
if [ "$watch" = "true" ]; then
  gh run watch --interval 30
fi

# Fetch latest run status
gh run list --workflow="Testing Coverage Merge Orchestrator" --limit 1
```

#### Output Includes
- Workflow run URL for detailed monitoring
- Real-time execution status (with --watch)
- Successful merge count
- Failed merge count requiring manual intervention
- AI conflict resolution status
- Epic branch validation results (build + tests)
- Dry-run preview (what would happen)

#### Use Cases
- Manual epic consolidation trigger during active development
- Testing orchestrator configuration with dry-run
- Urgent PR consolidation when scheduler inactive
- Validation before live execution
- Recovery from failed automated consolidation

#### Integration Points
- .github/workflows/testing-coverage-merger.yml: Workflow execution
- Scripts/validate-coverage-automation.sh: Epic validation
- .github/prompts/testing-coverage-merge-orchestrator.md: AI conflict resolution
- TestEngineer: Coverage PR creation coordination
- ComplianceOfficer: Quality gate validation post-consolidation

**Skill Dependency:** coverage-epic-consolidation (orchestrates complex multi-PR workflow)

**Estimated Development:** 1 day (workflow already exists, command is thin wrapper)
**Dependencies:** gh CLI, existing workflow, coverage-epic-consolidation skill

---

## Command Implementation Checklist

**Per Command:**
- [ ] Create command markdown file in `.claude/commands/`
- [ ] Define frontmatter with description, argument-hint, category
- [ ] Document comprehensive usage examples
- [ ] Specify all arguments with defaults and validation
- [ ] Implement execution logic (bash, gh CLI, skill delegation)
- [ ] Create error handling and helpful error messages
- [ ] Test argument parsing robustly
- [ ] Validate output formatting for user clarity
- [ ] Document integration points
- [ ] Integrate with underlying skills (if applicable)

**Quality Assurance:**
- [ ] Functionality testing: Execute all usage examples successfully
- [ ] Argument validation: Test invalid inputs with clear errors
- [ ] Integration testing: Verify skill delegation works
- [ ] UX validation: Output is clear and actionable
- [ ] Documentation: Examples comprehensive and accurate

---

## Command Registration (Phase 2 Requirement)

**Current Status:** Commands defined but not executable by subagents

**Identified Gap:** TestEngineer validation revealed SlashCommand tool requires command registration with Claude Code command palette. Commands exist as `.md` files but aren't recognized dynamically.

**Phase 2 Requirements:**
1. **Investigate:** Claude Code command registration mechanism
2. **Implement:** Dynamic command discovery from `.claude/commands/`
3. **Enable:** Subagent command execution (if architecture supports)
4. **Document:** Command registration process and limitations

**Alternative Approaches (If Registration Fails):**
- Bash-executable scripts for command functionality
- Commands remain user-facing (not agent-executable)
- Agent reads command docs for guidance pattern

**Decision:** Defer command registration to Phase 2; focus Phase 1 on skills (production-ready)

---

## Commands NOT Created (Rationale)

### AI Sentinel Precheck Commands - DEFERRED

**Considered:**
- `/precheck-security` - Local security analysis before PR
- `/precheck-standards` - Local standards compliance check
- `/precheck-debt` - Local technical debt assessment
- `/precheck-testing` - Local test quality analysis
- `/precheck-all` - All AI Sentinels locally

**Rationale for Deferral:**
- AI Sentinels effective in GitHub cloud environment via workflows
- Local execution requires AI quota management
- Context package construction complex for local runs
- Marginal value over waiting for PR creation
- Precheck demand unvalidated

**Future Consideration:** If local precheck demand emerges, revisit with:
- Local AI execution integration
- Context packaging from local codebase
- AI quota budgeting
- Value demonstration vs. PR-based analysis

### Redundant Workflow Commands - SKIP

**Not Creating:**
- `/pr-create` - Use `gh pr create` directly (minimal added value)
- `/branch-status` - Use `git status` + `git branch` (simple git commands)
- `/commit-check` - Conventional commit validation in pre-commit hook

**Philosophy:** Commands should provide orchestration value beyond simple CLI wrapping

---

## Command Development Priority

**Iteration 2 Implementation Order:**
1. **/workflow-status** (2 days) - Immediate dev experience improvement
2. **/coverage-report** (2 days) - Critical for testing excellence
3. **/create-issue** (2 days) - Automation efficiency gain
4. **/merge-coverage-prs** (1 day) - Epic consolidation enabler

**Total Estimated Effort:** 7 days command development + integration

---

## Command Usage Metrics (Projected)

| Command | Use Case | Time Savings | Adoption Target |
|---------|----------|--------------|-----------------|
| /workflow-status | CI/CD monitoring | ~2 min per check | High (daily use) |
| /coverage-report | Coverage validation | ~3 min per analysis | High (PR workflow) |
| /create-issue | Issue creation | ~4 min per issue (5→1 min) | Medium (as needed) |
| /merge-coverage-prs | Epic consolidation | ~10 min per trigger | Low (weekly) |

**Total Developer Time Savings:** ~15-20 min/day per active developer

**Workflow Efficiency:** Commands eliminate 60-80% of manual GitHub UI navigation for common operations

---

**Commands Catalog Status:** ✅ **COMPLETE**

**Next Actions:**
1. Begin command implementation in Iteration 2
2. Create command markdown files following specifications
3. Implement argument parsing and validation
4. Integrate with underlying skills where applicable
5. Test all usage examples for UX validation
6. Phase 2: Investigate command registration with Claude Code
