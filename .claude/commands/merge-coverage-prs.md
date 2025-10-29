---
description: "Trigger Coverage Excellence Merge Orchestrator workflow"
argument-hint: "[--dry-run] [--max N] [--labels LABELS] [--watch]"
category: "workflow"
---

# /merge-coverage-prs

Trigger the Testing Coverage Merge Orchestrator workflow for multi-PR consolidation with AI conflict resolution—directly from your terminal with safety-first dry-run default.

## Purpose

Eliminates manual GitHub UI navigation for Coverage Excellence Merge Orchestrator workflow triggering. Provides CLI-based workflow execution with dry-run safety, flexible configuration, and real-time monitoring, saving ~10 minutes per orchestration cycle.

**Target Users:** Developers managing coverage epic progression and PR consolidation

**Time Savings:** 90% reduction in GitHub UI navigation and workflow configuration time

**Safety-First Design:** Defaults to dry-run preview mode; requires explicit `--no-dry-run` flag for live execution

## Usage Examples

### Example 1: Safe Dry-Run Preview (Most Common - Default Behavior)

```bash
/merge-coverage-prs
```

**Expected Output:**
```
🔄 Triggering Coverage Excellence Merge Orchestrator...

Configuration:
  Dry-run: true
  Max PRs: 8
  Label filter: type: coverage,coverage,testing

⚠️ DRY-RUN MODE: Preview only - no actual merges will be executed
→ This is a safe preview of what would happen during live execution
→ Review dry-run results before executing with --no-dry-run flag

✅ Workflow triggered successfully!
📊 Workflow URL: https://github.com/Zarichney-Development/zarichney-api/actions/runs/12345

💡 Next Steps:
- View dry-run results: Visit workflow URL above
- Execute live run: /merge-coverage-prs --no-dry-run --max 8
- Monitor status: /workflow-status "Testing Coverage Merge Orchestrator"
```

### Example 2: Dry-Run with Larger Batch Size

```bash
/merge-coverage-prs --dry-run --max 15
```

**Expected Output:**
```
🔄 Triggering Coverage Excellence Merge Orchestrator...

Configuration:
  Dry-run: true
  Max PRs: 15
  Label filter: type: coverage,coverage,testing

⚠️ DRY-RUN MODE: Preview only - no actual merges will be executed
→ Testing larger batch size (15 PRs) before live execution

✅ Workflow triggered successfully!
📊 Workflow URL: https://github.com/Zarichney-Development/zarichney-api/actions/runs/12346

💡 Dry-Run Strategy:
- Validates PR discovery for 15 PRs
- Previews merge order and AI conflict detection
- No changes to epic branch or PRs
- Use results to optimize --max value for live execution
```

### Example 3: Live Execution with Safety Confirmation (Explicit Opt-In Required)

```bash
/merge-coverage-prs --no-dry-run --max 8
```

**Expected Output:**
```
⚠️ WARNING: Live execution mode enabled
→ This will trigger actual PR merges to continuous/testing-excellence
→ Press Ctrl+C within 3 seconds to cancel...

🔄 Triggering Coverage Excellence Merge Orchestrator...

Configuration:
  Dry-run: false
  Max PRs: 8
  Label filter: type: coverage,coverage,testing

🚨 LIVE EXECUTION MODE: Real PR consolidation will be performed
→ Sequential PR merging into epic branch
→ AI conflict resolution for complex merges
→ Epic branch validation (build + tests)

✅ Workflow triggered successfully!
📊 Workflow URL: https://github.com/Zarichney-Development/zarichney-api/actions/runs/12347

💡 Next Steps:
- Monitor execution: /merge-coverage-prs --watch
- Check workflow status: /workflow-status "Testing Coverage Merge Orchestrator"
- View epic progression: /coverage-report --epic
- Validate results: gh pr list --base continuous/testing-excellence
```

### Example 4: Custom Label Filtering (Flexible OR Logic)

```bash
/merge-coverage-prs --labels "coverage,testing,ai-task"
```

**Expected Output:**
```
🔄 Triggering Coverage Excellence Merge Orchestrator...

Configuration:
  Dry-run: true
  Max PRs: 8
  Label filter: coverage,testing,ai-task

🏷️ Custom Label Filtering:
→ Matches PRs with any of: coverage OR testing OR ai-task
→ Flexible matching: "type: coverage", "coverage", "testing", etc.

⚠️ DRY-RUN MODE: Preview only - no actual merges will be executed

✅ Workflow triggered successfully!
📊 Workflow URL: https://github.com/Zarichney-Development/zarichney-api/actions/runs/12348

💡 Label Filter Guidance:
- Default filter: "type: coverage,coverage,testing"
- Custom filter used: "coverage,testing,ai-task"
- Expands PR discovery beyond default coverage labels
- Review dry-run results to validate PR selection
```

### Example 5: Maximum Consolidation (50 PRs - Enterprise Scale)

```bash
/merge-coverage-prs --no-dry-run --max 50
```

**Expected Output:**
```
⚠️ WARNING: Live execution mode enabled
→ This will trigger actual PR merges to continuous/testing-excellence
→ Maximum consolidation: Up to 50 PRs
→ Press Ctrl+C within 3 seconds to cancel...

🔄 Triggering Coverage Excellence Merge Orchestrator...

Configuration:
  Dry-run: false
  Max PRs: 50
  Label filter: type: coverage,coverage,testing

🚨 MAXIMUM CONSOLIDATION MODE:
→ Processing up to 50 PRs in single orchestration run
→ Estimated execution time: 15-25 minutes
→ AI conflict resolution for complex scenarios
→ Epic branch integrity validation post-consolidation

✅ Workflow triggered successfully!
📊 Workflow URL: https://github.com/Zarichney-Development/zarichney-api/actions/runs/12349

💡 Maximum Consolidation Strategy:
- Ideal for batch processing accumulated coverage PRs
- Requires stable epic branch state before execution
- Monitor workflow progress: /merge-coverage-prs --watch
- Validation critical for large batch sizes
```

### Example 6: Real-Time Monitoring with --watch Flag

```bash
/merge-coverage-prs --watch
```

**Expected Output:**
```
🔄 Triggering Coverage Excellence Merge Orchestrator...

Configuration:
  Dry-run: true
  Max PRs: 8
  Label filter: type: coverage,coverage,testing

⚠️ DRY-RUN MODE: Preview only - no actual merges will be executed

✅ Workflow triggered successfully!
📊 Workflow URL: https://github.com/Zarichney-Development/zarichney-api/actions/runs/12350

👀 Watching workflow execution...
   Press Ctrl+C to stop watching (workflow continues running)

Refreshing run status every 30 seconds...

🔄 Testing Coverage Merge Orchestrator - In Progress
   Started: 2025-10-26 14:32:15
   Job: Discover Coverage PRs - ✅ Completed
   Job: Process PRs with Direct Sequential Merging - 🔄 Running

🔄 Testing Coverage Merge Orchestrator - In Progress
   Job: Execute AI Conflict Resolution for Failed PRs - ✅ Completed
   Job: Validate Epic Branch After Sequential Processing - 🔄 Running

✅ Testing Coverage Merge Orchestrator - Completed
   Duration: 8m 42s
   Status: Success

📊 Final Status:
✅  Testing Coverage Merge Orchestrator  continuous/testing-excel...  main  8m42s  12350

💡 Next Steps:
- View detailed results: Visit workflow URL above
- Check consolidated epic: gh pr list --base continuous/testing-excellence
- Validate coverage impact: /coverage-report --epic
- Execute live run: /merge-coverage-prs --no-dry-run --max 8
```

### Example 7: Live Execution with Real-Time Monitoring

```bash
/merge-coverage-prs --no-dry-run --watch
```

**Expected Output:**
```
⚠️ WARNING: Live execution mode enabled
→ This will trigger actual PR merges to continuous/testing-excellence
→ Press Ctrl+C within 3 seconds to cancel...

🔄 Triggering Coverage Excellence Merge Orchestrator...

Configuration:
  Dry-run: false
  Max PRs: 8
  Label filter: type: coverage,coverage,testing

🚨 LIVE EXECUTION MODE: Real PR consolidation will be performed

✅ Workflow triggered successfully!
📊 Workflow URL: https://github.com/Zarichney-Development/zarichney-api/actions/runs/12351

👀 Watching workflow execution...
   Press Ctrl+C to stop watching (workflow continues running)

[Real-time workflow monitoring output...]

✅ Testing Coverage Merge Orchestrator - Completed
   Duration: 9m 15s
   Status: Success

📊 Live Execution Results:
✅  Testing Coverage Merge Orchestrator  continuous/testing-excel...  main  9m15s  12351

🎉 SUCCESS: Coverage PRs consolidated into epic branch

💡 Post-Consolidation Actions:
- Validate epic branch: /workflow-status build.yml --branch continuous/testing-excellence
- Check coverage impact: /coverage-report --epic --compare
- Review consolidated changes: gh pr view <epic-pr-number>
- Continue coverage progression: Create more coverage PRs
```

## Arguments

### Flags

#### `--dry-run` (flag - default behavior)

- **Default:** `true` (automatically enabled if neither flag specified)
- **Description:** Preview workflow execution without performing actual PR merges (safety-first design)
- **Behavior:** Workflow validates PR discovery, merge order, and conflict detection without modifying epic branch or closing PRs
- **Safety:** Default mode prevents accidental live execution
- **Usage:** Can be explicitly specified for clarity, or omitted (default behavior)
- **Example:** `/merge-coverage-prs --dry-run` (explicit) or `/merge-coverage-prs` (implicit)

#### `--no-dry-run` (flag - explicit opt-in required)

- **Default:** `false` (live execution disabled by default)
- **Description:** Execute live PR consolidation with actual merges into epic branch (requires explicit opt-in)
- **Behavior:**
  - Displays 3-second safety confirmation before triggering
  - Performs sequential PR merging directly into epic branch
  - Executes AI conflict resolution for complex merge scenarios
  - Validates epic branch integrity (build + tests)
  - Closes successfully merged PRs
- **Safety:** Requires explicit flag + confirmation countdown to prevent accidents
- **Usage:** Include flag to enable live execution, omit for safe dry-run mode
- **Example:** `/merge-coverage-prs --no-dry-run --max 8`

#### `--watch` (flag)

- **Default:** `false`
- **Description:** Monitor workflow execution in real-time with 30-second refresh interval
- **Behavior:**
  - Triggers workflow and immediately begins monitoring
  - Displays job-level status updates every 30 seconds
  - Shows workflow progress, duration, and completion status
  - Allows Ctrl+C to stop watching (workflow continues running)
  - Displays final workflow status and results
- **Usage:** Include flag to enable real-time monitoring, omit for fire-and-forget execution
- **Example:** `/merge-coverage-prs --watch` or `/merge-coverage-prs --no-dry-run --watch`

### Optional Arguments

#### `--max N` (optional named)

- **Type:** Number (integer)
- **Default:** `8`
- **Description:** Maximum number of PRs to consolidate in single orchestration run
- **Validation:** Must be integer between 1-50
- **Rationale:**
  - Default 8 balances efficiency vs. workflow complexity
  - Maximum 50 prevents GitHub API limitations and excessive execution time
  - Minimum 1 allows single-PR testing scenarios
- **Examples:**
  - `--max 8` - Default batch size (8 PRs)
  - `--max 15` - Larger batch for accumulated PRs
  - `--max 50` - Maximum consolidation for enterprise scale
  - `--max 1` - Single-PR testing or focused consolidation

#### `--labels LABELS` (optional named)

- **Type:** String (comma-separated labels)
- **Default:** `"type: coverage,coverage,testing"`
- **Description:** Comma-separated label filter using OR logic with flexible matching
- **Validation:** Must contain at least one valid label after parsing
- **Behavior:**
  - Flexible OR matching: PRs match if they have ANY label in filter
  - Case-insensitive matching: "Coverage" matches "coverage"
  - Partial matching: "type: coverage" matches "type:coverage" (flexible spacing)
  - Comma-separated: "coverage,testing,ai-task" → coverage OR testing OR ai-task
- **Examples:**
  - `--labels "type: coverage,coverage,testing"` - Default coverage labels
  - `--labels "coverage,testing,ai-task"` - Expanded discovery
  - `--labels "ai-task"` - Only AI-generated coverage PRs

## Output

### Dry-Run Mode (Default)

```
🔄 Triggering Coverage Excellence Merge Orchestrator...

Configuration:
  Dry-run: true
  Max PRs: 8
  Label filter: type: coverage,coverage,testing

⚠️ DRY-RUN MODE: Preview only - no actual merges will be executed

✅ Workflow triggered successfully!
📊 Workflow URL: [URL]

💡 Next Steps:
- View dry-run results: Visit workflow URL
- Execute live run: /merge-coverage-prs --no-dry-run --max 8
```

### Live Execution Mode (--no-dry-run)

```
⚠️ WARNING: Live execution mode enabled
→ This will trigger actual PR merges to continuous/testing-excellence
→ Press Ctrl+C within 3 seconds to cancel...

🔄 Triggering Coverage Excellence Merge Orchestrator...

Configuration:
  Dry-run: false
  Max PRs: 8
  Label filter: type: coverage,coverage,testing

🚨 LIVE EXECUTION MODE: Real PR consolidation will be performed

✅ Workflow triggered successfully!
📊 Workflow URL: [URL]

💡 Next Steps:
- Monitor execution: /merge-coverage-prs --watch
- Check status: /workflow-status
```

### Real-Time Monitoring Mode (--watch)

```
👀 Watching workflow execution...
   Press Ctrl+C to stop watching (workflow continues running)

Refreshing run status every 30 seconds...

🔄 Testing Coverage Merge Orchestrator - In Progress
   Job: [Job status updates every 30 seconds]

✅ Testing Coverage Merge Orchestrator - Completed
   Duration: [Duration]
   Status: [Status]

📊 Final Status:
[Workflow run summary]
```

## Error Handling

### Missing gh CLI

```
⚠️ Dependency Missing: gh CLI not found

This command requires GitHub CLI (gh) to trigger workflows.

Installation:
• macOS:   brew install gh
• Ubuntu:  sudo apt install gh
• Windows: winget install GitHub.cli

After installation:
1. Authenticate: gh auth login
2. Verify: gh --version
3. Retry: /merge-coverage-prs

Learn more: https://cli.github.com/
```

### gh CLI Not Authenticated

```
⚠️ Authentication Required: gh CLI not authenticated

Run this command to authenticate:
  gh auth login

Then retry:
  /merge-coverage-prs

Troubleshooting:
• Check credentials: gh auth status
• Re-authenticate: gh auth refresh
```

### Invalid --max Value

```
❌ Error: Invalid --max value: 100

→ Must be a number between 1 and 50

💡 Common values:
  • 8  - Default batch size (balanced efficiency)
  • 15 - Larger batch for accumulated PRs
  • 50 - Maximum consolidation (enterprise scale)

Valid range: 1-50

Example: /merge-coverage-prs --max 15
```

### Invalid --max Value (Non-Numeric)

```
❌ Error: Invalid --max value: abc

→ Must be a number between 1 and 50

💡 Tip: --max expects an integer value

Example: /merge-coverage-prs --max 8
```

### Workflow Not Found

```
⚠️ Workflow Not Found: "Testing Coverage Merge Orchestrator"

This could indicate:
• Workflow file missing from .github/workflows/
• Incorrect workflow name in gh CLI
• Repository permissions issue

Troubleshooting:
1. List available workflows: gh workflow list
2. Verify workflow file exists: ls -la .github/workflows/testing-coverage-merger.yml
3. Check repository: gh repo view

If workflow exists:
• Ensure you're in zarichney-api repository
• Verify gh CLI targeting correct repository
• Check workflow naming in testing-coverage-merger.yml

Manual trigger alternative:
  gh workflow run testing-coverage-merger.yml --field dry_run=true
```

### Workflow Trigger Failure

```
❌ Error: Failed to trigger workflow

GitHub API response: [Error message]

Troubleshooting:
1. Verify authentication: gh auth status
2. Check repository permissions: gh repo view
3. Validate workflow_dispatch trigger exists in workflow file
4. Review GitHub Actions status: https://www.githubstatus.com/

If issue persists:
• Try manual trigger via GitHub UI
• Verify workflow has workflow_dispatch trigger
• Check GitHub Actions permissions for repository

Retry command:
  /merge-coverage-prs
```

### Network/API Failure

```
⚠️ Network Error: Unable to connect to GitHub API

This could indicate:
• Network connectivity issue
• GitHub API rate limiting
• GitHub service outage

Troubleshooting:
1. Check internet connection
2. Verify GitHub status: https://www.githubstatus.com/
3. Check API rate limits: gh api rate_limit

If rate limited:
• Wait for rate limit reset
• Authenticate with different credentials
• Use GitHub UI as alternative

Retry command:
  /merge-coverage-prs
```

### Missing --max Argument Value

```
❌ Error: --max requires a value

Usage: /merge-coverage-prs --max N

Where N is a number between 1 and 50

Examples:
  /merge-coverage-prs --max 8
  /merge-coverage-prs --max 15
  /merge-coverage-prs --max 50
```

### Missing --labels Argument Value

```
❌ Error: --labels requires a value

Usage: /merge-coverage-prs --labels "label1,label2,label3"

Examples:
  /merge-coverage-prs --labels "type: coverage,coverage,testing"
  /merge-coverage-prs --labels "coverage,testing,ai-task"
  /merge-coverage-prs --labels "ai-task"
```

### Unknown Argument

```
❌ Error: Unknown argument: --invalid-flag

Valid arguments:
  --dry-run              Preview without executing merges (default)
  --no-dry-run           Execute live PR consolidation
  --max N                Maximum PRs to consolidate (1-50, default: 8)
  --labels LABELS        Comma-separated label filter (default: "type: coverage,coverage,testing")
  --watch                Monitor workflow execution in real-time

Examples:
  /merge-coverage-prs
  /merge-coverage-prs --dry-run --max 15
  /merge-coverage-prs --no-dry-run --max 8
  /merge-coverage-prs --labels "coverage,testing"
  /merge-coverage-prs --watch
```

## Integration Points

- **Workflow:** `.github/workflows/testing-coverage-merger.yml` - Coverage Excellence Merge Orchestrator
- **Epic Branch:** `continuous/testing-excellence` - Target branch for PR consolidation
- **AI Conflict Resolution:** `.github/prompts/testing-coverage-merge-orchestrator.md` - AI-powered merge conflict resolution
- **Quality Gates:** Epic branch validation (build + tests) post-consolidation
- **TestEngineer:** Coverage PR creation coordination and epic progression tracking
- **ComplianceOfficer:** Post-consolidation quality gate validation
- **Related Commands:** `/workflow-status`, `/coverage-report --epic`

## Tool Dependencies

**Required:**
- `gh` (GitHub CLI) - Minimum version 2.0.0
  - Installation: `brew install gh` (macOS) | `sudo apt install gh` (Linux)
  - Authentication: `gh auth login`

**Workflow Requirements:**
- `.github/workflows/testing-coverage-merger.yml` must exist
- Workflow must have `workflow_dispatch` trigger with required inputs
- Epic branch `continuous/testing-excellence` must exist
- GitHub Actions permissions: write access to contents, pull-requests, issues

## Implementation

```bash
#!/bin/bash

# ============================================================================
# /merge-coverage-prs - Coverage Excellence Merge Orchestrator Trigger
# ============================================================================

# Initialize variables with safety-first defaults
dry_run="true"     # Default: safety-first dry-run mode
max_prs="8"        # Default: moderate batch size
labels="type: coverage,coverage,testing"  # Default: flexible label matching
watch="false"      # Default: fire-and-forget

# ============================================================================
# ARGUMENT PARSING
# ============================================================================

# Parse command-line arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --dry-run)
      dry_run="true"
      shift
      ;;
    --no-dry-run)
      dry_run="false"
      shift
      ;;
    --max)
      if [ -z "$2" ]; then
        echo "❌ Error: --max requires a value"
        echo ""
        echo "Usage: /merge-coverage-prs --max N"
        echo ""
        echo "Where N is a number between 1 and 50"
        echo ""
        echo "Examples:"
        echo "  /merge-coverage-prs --max 8"
        echo "  /merge-coverage-prs --max 15"
        echo "  /merge-coverage-prs --max 50"
        exit 1
      fi
      max_prs="$2"
      shift 2
      ;;
    --labels)
      if [ -z "$2" ]; then
        echo "❌ Error: --labels requires a value"
        echo ""
        echo "Usage: /merge-coverage-prs --labels \"label1,label2,label3\""
        echo ""
        echo "Examples:"
        echo "  /merge-coverage-prs --labels \"type: coverage,coverage,testing\""
        echo "  /merge-coverage-prs --labels \"coverage,testing,ai-task\""
        echo "  /merge-coverage-prs --labels \"ai-task\""
        exit 1
      fi
      labels="$2"
      shift 2
      ;;
    --watch)
      watch="true"
      shift
      ;;
    *)
      echo "❌ Error: Unknown argument: $1"
      echo ""
      echo "Valid arguments:"
      echo "  --dry-run              Preview without executing merges (default)"
      echo "  --no-dry-run           Execute live PR consolidation"
      echo "  --max N                Maximum PRs to consolidate (1-50, default: 8)"
      echo "  --labels LABELS        Comma-separated label filter (default: \"type: coverage,coverage,testing\")"
      echo "  --watch                Monitor workflow execution in real-time"
      echo ""
      echo "Examples:"
      echo "  /merge-coverage-prs"
      echo "  /merge-coverage-prs --dry-run --max 15"
      echo "  /merge-coverage-prs --no-dry-run --max 8"
      echo "  /merge-coverage-prs --labels \"coverage,testing\""
      echo "  /merge-coverage-prs --watch"
      exit 1
      ;;
  esac
done

# ============================================================================
# VALIDATION
# ============================================================================

# Validate max_prs is a number
if ! [[ "$max_prs" =~ ^[0-9]+$ ]]; then
  echo "❌ Error: Invalid --max value: $max_prs"
  echo ""
  echo "→ Must be a number between 1 and 50"
  echo ""
  echo "💡 Tip: --max expects an integer value"
  echo ""
  echo "Example: /merge-coverage-prs --max 8"
  exit 1
fi

# Validate max_prs range
if [[ ! "$max_prs" =~ ^[0-9]+$ ]] || [ "$max_prs" -lt 1 ] || [ "$max_prs" -gt 50 ]; then
  echo "❌ Error: Invalid --max value: $max_prs"
  echo ""
  echo "→ Must be a number between 1 and 50"
  echo ""
  echo "💡 Common values:"
  echo "  • 8  - Default batch size (balanced efficiency)"
  echo "  • 15 - Larger batch for accumulated PRs"
  echo "  • 50 - Maximum consolidation (enterprise scale)"
  echo ""
  echo "Valid range: 1-50"
  echo ""
  echo "Example: /merge-coverage-prs --max 15"
  exit 1
fi

# Check gh CLI availability
if ! command -v gh &> /dev/null; then
  echo "⚠️ Dependency Missing: gh CLI not found"
  echo ""
  echo "This command requires GitHub CLI (gh) to trigger workflows."
  echo ""
  echo "Installation:"
  echo "• macOS:   brew install gh"
  echo "• Ubuntu:  sudo apt install gh"
  echo "• Windows: winget install GitHub.cli"
  echo ""
  echo "After installation:"
  echo "1. Authenticate: gh auth login"
  echo "2. Verify: gh --version"
  echo "3. Retry: /merge-coverage-prs"
  echo ""
  echo "Learn more: https://cli.github.com/"
  exit 1
fi

# Check gh authentication
if ! gh auth status &> /dev/null; then
  echo "⚠️ Authentication Required: gh CLI not authenticated"
  echo ""
  echo "Run this command to authenticate:"
  echo "  gh auth login"
  echo ""
  echo "Then retry:"
  echo "  /merge-coverage-prs"
  echo ""
  echo "Troubleshooting:"
  echo "• Check credentials: gh auth status"
  echo "• Re-authenticate: gh auth refresh"
  exit 1
fi

# ============================================================================
# SAFETY CONFIRMATION FOR LIVE EXECUTION
# ============================================================================

if [ "$dry_run" = "false" ]; then
  echo "⚠️ WARNING: Live execution mode enabled"
  echo "→ This will trigger actual PR merges to continuous/testing-excellence"
  echo "→ Press Ctrl+C within 3 seconds to cancel..."
  sleep 3
  echo ""
fi

# ============================================================================
# WORKFLOW TRIGGER
# ============================================================================

echo "🔄 Triggering Coverage Excellence Merge Orchestrator..."
echo ""
echo "Configuration:"
echo "  Dry-run: $dry_run"
echo "  Max PRs: $max_prs"
echo "  Label filter: $labels"
echo ""

# Display mode-specific warnings
if [ "$dry_run" = "true" ]; then
  echo "⚠️ DRY-RUN MODE: Preview only - no actual merges will be executed"
  echo "→ This is a safe preview of what would happen during live execution"
  echo "→ Review dry-run results before executing with --no-dry-run flag"
else
  echo "🚨 LIVE EXECUTION MODE: Real PR consolidation will be performed"
  echo "→ Sequential PR merging into epic branch"
  echo "→ AI conflict resolution for complex merges"
  echo "→ Epic branch validation (build + tests)"
fi
echo ""

# Trigger workflow via gh CLI
workflow_trigger_output=$(gh workflow run "Testing Coverage Merge Orchestrator" \
  --field dry_run="$dry_run" \
  --field max_prs="$max_prs" \
  --field pr_label_filter="$labels" \
  2>&1)

trigger_exit_code=$?

if [ $trigger_exit_code -eq 0 ]; then
  echo "✅ Workflow triggered successfully!"

  # Wait briefly for run to register
  sleep 5

  # Get latest run URL
  latest_run=$(gh run list --workflow="Testing Coverage Merge Orchestrator" --limit 1 --json url --jq '.[0].url' 2>/dev/null)

  if [ -n "$latest_run" ]; then
    echo "📊 Workflow URL: $latest_run"
  fi
else
  echo "❌ Error: Failed to trigger workflow"
  echo ""
  echo "GitHub API response: $workflow_trigger_output"
  echo ""
  echo "Troubleshooting:"
  echo "1. Verify authentication: gh auth status"
  echo "2. Check repository permissions: gh repo view"
  echo "3. Validate workflow_dispatch trigger exists in workflow file"
  echo "4. Review GitHub Actions status: https://www.githubstatus.com/"
  echo ""
  echo "If issue persists:"
  echo "• Try manual trigger via GitHub UI"
  echo "• Verify workflow has workflow_dispatch trigger"
  echo "• Check GitHub Actions permissions for repository"
  echo ""
  echo "Retry command:"
  echo "  /merge-coverage-prs"
  exit 1
fi

# ============================================================================
# REAL-TIME MONITORING (--watch flag)
# ============================================================================

if [ "$watch" = "true" ]; then
  echo ""
  echo "👀 Watching workflow execution..."
  echo "   Press Ctrl+C to stop watching (workflow continues running)"
  echo ""

  # Wait for run to start
  sleep 5

  # Watch latest run
  echo "Refreshing run status every 30 seconds..."
  echo ""
  gh run watch --interval 30

  # Show final status
  echo ""
  echo "📊 Final Status:"
  gh run list --workflow="Testing Coverage Merge Orchestrator" --limit 1
fi

# ============================================================================
# SUCCESS FEEDBACK
# ============================================================================

echo ""
echo "💡 Next Steps:"

if [ "$dry_run" = "true" ]; then
  echo "- View dry-run results: Visit workflow URL above"
  echo "- Execute live run: /merge-coverage-prs --no-dry-run --max $max_prs"
  echo "- Monitor status: /workflow-status \"Testing Coverage Merge Orchestrator\""
  if [ "$watch" = "false" ]; then
    echo "- Watch execution: /merge-coverage-prs --watch"
  fi
else
  echo "- Monitor execution: /workflow-status \"Testing Coverage Merge Orchestrator\""
  echo "- Check consolidated epic: gh pr list --base continuous/testing-excellence"
  echo "- Validate coverage impact: /coverage-report --epic --compare"
  if [ "$watch" = "false" ]; then
    echo "- Watch execution: /merge-coverage-prs --watch"
  fi
fi
```

## Best Practices

**DO:**
- ✅ Use dry-run mode first to validate PR discovery and merge order
- ✅ Start with default --max 8 for balanced efficiency
- ✅ Increase --max to 15-50 for accumulated coverage PRs
- ✅ Use --watch flag to monitor complex consolidation runs
- ✅ Validate epic branch state before live execution
- ✅ Review dry-run results before executing --no-dry-run

**DON'T:**
- ❌ Skip dry-run validation before live execution (safety-first design prevents this)
- ❌ Use excessive --max values without dry-run testing
- ❌ Trigger live execution without reviewing epic branch status
- ❌ Ignore workflow failure alerts (check logs for root cause)
- ❌ Run live execution during active epic branch development

## Safety-First Design Philosophy

This command implements a **safety-first design** to prevent accidental PR consolidation:

1. **Default Dry-Run:** Command defaults to dry-run mode if no flags specified
2. **Explicit Opt-In:** Live execution requires explicit `--no-dry-run` flag
3. **Safety Countdown:** 3-second cancellation window before live execution triggers
4. **Clear Warnings:** Visual distinction between dry-run and live execution modes
5. **Validation Emphasis:** Encourages dry-run validation before live execution

**Design Rationale:**
- Prevents accidental live execution (default dry-run)
- Requires deliberate action for PR consolidation (explicit flag + countdown)
- Promotes validation workflow (dry-run → review → live execution)
- Reduces risk of unintended epic branch modifications
- Aligns with enterprise safety standards for automation

## Epic Progression Context

**Coverage Excellence Epic:** Backend Testing Coverage Excellence Initiative
- **Target:** Comprehensive backend test coverage through continuous improvement
- **Epic Branch:** `continuous/testing-excellence`
- **Strategy:** Sequential PR consolidation with AI conflict resolution
- **Quality Gates:** Build validation + test execution + standards compliance

**Integration with Coverage Excellence Workflow:**
1. TestEngineer creates individual coverage PRs targeting epic branch
2. PRs accumulate over time (automated creation via testing-coverage-execution.yml)
3. Developer triggers consolidation: `/merge-coverage-prs --dry-run --max 15`
4. Reviews dry-run results and validates PR selection
5. Executes live consolidation: `/merge-coverage-prs --no-dry-run --max 15 --watch`
6. Monitors real-time execution and validates epic branch integrity
7. Continues coverage progression with next batch of PRs

**Workflow Coordination:**
- `.github/workflows/testing-coverage-execution.yml` - Automated PR creation (4x daily)
- `.github/workflows/testing-coverage-merger.yml` - Multi-PR consolidation (manual/auto-trigger)
- `Scripts/run-test-suite.sh` - Test execution and coverage validation
- `/test-report` - Comprehensive test suite analysis with AI insights
- `/coverage-report --epic` - Epic progression metrics and PR inventory
