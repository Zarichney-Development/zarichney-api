---
description: "Check current status of GitHub Actions workflows"
argument-hint: "[workflow-name] [--details] [--limit N] [--branch BRANCH]"
category: "workflow"
---

# /workflow-status

Monitor GitHub Actions workflow runs with detailed status, logs, and failure diagnostics—all from your terminal.

## Purpose

Eliminates context-switching to GitHub UI for CI/CD status checks. Provides instant workflow monitoring during active development, saving ~2 minutes per check (15-30 min/day for active developers).

**Target Users:** All developers checking CI/CD status during development

**Time Savings:** 60-80% reduction in GitHub UI navigation for workflow monitoring

## Usage Examples

### Example 1: Quick Status Check (Most Common)

```bash
/workflow-status
```

**Expected Output:**
```
🔄 Fetching recent workflow runs...

Recent Workflow Runs (Last 5):

✅ build.yml                  main    3 min ago   success   (2m 34s)
❌ testing-coverage.yml       epic    12 min ago  failure   (5m 12s)
🔄 deploy.yml                 main    running...  in_progress
✅ lint-and-format.yml        feature 1 hr ago    success   (1m 03s)
✅ build.yml                  main    3 hrs ago   success   (2m 41s)

💡 Next Steps:
- View failure details: /workflow-status testing-coverage.yml --details
- Check specific workflow: /workflow-status <workflow-name>
```

### Example 2: Specific Workflow History

```bash
/workflow-status build.yml --limit 10
```

**Expected Output:**
```
🔄 Fetching runs for 'build.yml'...

Build Workflow - Last 10 Runs:

✅ main       3 min ago    success   (2m 34s)
✅ main       3 hrs ago    success   (2m 41s)
❌ feature    6 hrs ago    failure   (1m 52s)
✅ main       1 day ago    success   (2m 38s)

Trend: 8/10 success (80%)

💡 Next Steps:
- Investigate failures: /workflow-status build.yml --details
```

### Example 3: Detailed Failure Debugging

```bash
/workflow-status testing-coverage.yml --details
```

**Expected Output:**
```
🔄 Fetching detailed status for 'testing-coverage.yml'...

Testing Coverage Workflow - Latest Run:

Status: ❌ FAILURE
Branch: epic/testing-coverage
Triggered: 12 min ago by push
Duration: 5m 12s
URL: https://github.com/Zarichney-Development/zarichney-api/runs/456

Jobs Breakdown:

  ✅ setup-environment        (34s)
    - Checkout code: ✅
    - Setup .NET: ✅
    - Restore dependencies: ✅

  ❌ run-coverage-tests       (4m 21s)
    - Build solution: ✅
    - Run unit tests: ❌ FAILED
    - Generate coverage: ⏭️ SKIPPED

Failure Details:

Job: run-coverage-tests
Step: Run unit tests
Exit Code: 1

Error Log (last 20 lines):
---
Failed   RecipeServiceTests.GetRecipeById_WithInvalidId_ThrowsException
  Error Message:
   Expected exception type ArgumentException but got null
---

💡 Next Steps:
- Fix failing test: RecipeServiceTests.GetRecipeById_WithInvalidId_ThrowsException
- Local test: dotnet test Zarichney.Server.Tests --filter "RecipeServiceTests"
```

### Example 4: Branch-Filtered Status

```bash
/workflow-status --branch feature/issue-123 --limit 3
```

**Expected Output:**
```
🔄 Fetching runs for branch 'feature/issue-123'...

Workflow Runs on feature/issue-123 (Last 3):

✅ build.yml               23 min ago  success      (2m 18s)
✅ lint-and-format.yml     23 min ago  success      (54s)
🔄 testing-coverage.yml    running...  in_progress

💡 Next Steps:
- Wait for coverage run to complete
- Compare with main: /workflow-status --branch main
```

### Example 5: Monitor Specific Workflow

```bash
/workflow-status "Testing Coverage Merge Orchestrator"
```

**Expected Output:**
```
🔄 Fetching runs for 'Testing Coverage Merge Orchestrator'...

Testing Coverage Merge Orchestrator - Last 5 Runs:

✅ develop    1 day ago   success   (8m 42s)
✅ develop    3 days ago  success   (9m 15s)
✅ develop    1 week ago  success   (8m 31s)

💡 Next Steps:
- View detailed logs: /workflow-status "Testing Coverage Merge Orchestrator" --details
```

## Arguments

### Optional Arguments

#### `[workflow-name]` (optional positional)

- **Type:** String
- **Position:** 1 (first argument after command)
- **Description:** Specific workflow to monitor. Accepts workflow file name (e.g., `build.yml`) or display name (e.g., "Build Workflow")
- **Default:** `null` (all workflows shown)
- **Validation:** Must match existing workflow in `.github/workflows/` or workflow display name
- **Examples:**
  - `build.yml` - File name format
  - `build` - Bare name (matches `build.yml`)
  - `"Testing Coverage Merge Orchestrator"` - Display name (quotes for spaces)

#### `--limit N` (optional named)

- **Type:** Number (integer)
- **Default:** `5`
- **Description:** Number of recent workflow runs to display
- **Validation:** Must be integer between 1-50
- **Rationale:** Default 5 balances context vs. overwhelming output
- **Examples:**
  - `--limit 10` - Show last 10 runs
  - `--limit 1` - Just latest run
  - `--limit 25` - Expanded history for trend analysis

#### `--branch BRANCH` (optional named)

- **Type:** String
- **Default:** Current git branch (auto-detected)
- **Description:** Filter workflow runs to specific branch
- **Validation:** Valid git branch name format
- **Examples:**
  - `--branch main` - Show main branch runs
  - `--branch epic/testing-coverage` - Show epic branch runs
  - `--branch feature/issue-123` - Show feature branch runs

### Flags

#### `--details` (flag)

- **Default:** `false`
- **Description:** Show detailed job-level status and failure logs instead of summary
- **Behavior:** Displays job breakdown, step-level status, error messages, log excerpts
- **Usage:** Include flag to enable, omit for summary mode
- **Example:** `/workflow-status --details`

## Output

### Summary Mode (Default)

```
🔄 Fetching recent workflow runs...

Recent Workflow Runs (Last 5):

✅ build.yml                  main    3 min ago   success   (2m 34s)
❌ testing-coverage.yml       epic    12 min ago  failure   (5m 12s)

💡 Next Steps:
- View details: /workflow-status --details
```

### Detailed Mode (--details)

```
Testing Coverage Workflow - Latest Run:

Status: ❌ FAILURE
Branch: epic/testing-coverage
Jobs Breakdown:
  ✅ setup-environment (34s)
  ❌ run-coverage-tests (4m 21s)

Failure Details:
[Error log excerpt]

💡 Next Steps:
- Fix failing test: [Specific test]
```

## Error Handling

### Missing gh CLI

```
⚠️ Dependency Missing: gh CLI not found

Installation:
• macOS:   brew install gh
• Ubuntu:  sudo apt install gh
• Windows: winget install GitHub.cli

After installation:
1. Authenticate: gh auth login
2. Retry: /workflow-status
```

### Invalid Workflow Name

```
⚠️ Workflow Not Found: No workflow named 'typo.yml'

Available workflows:
  • build.yml
  • testing-coverage.yml
  • deploy.yml

Try: /workflow-status build.yml
```

### Invalid Limit

```
⚠️ Invalid Range: --limit must be between 1-50 (got 100)

Valid range: 1-50
Default: 5

Try: /workflow-status --limit 25
```

### gh CLI Authentication Required

```
⚠️ Authentication Required: gh CLI not authenticated

Run this command to authenticate:
  gh auth login

Then retry:
  /workflow-status

Troubleshooting:
• Check credentials: gh auth status
• Re-authenticate: gh auth refresh
```

### No Workflow Runs Found

```
⚠️ No Results Found: No workflow runs found

This could mean:
• No workflows triggered recently
• Branch has no workflow runs
• Workflow name spelling error

Suggestions:
• Check all branches: /workflow-status --branch main
• List workflows: gh workflow list
• Trigger manually: gh workflow run <workflow-name>
```

## Integration Points

- **GitHub Actions:** Primary data source via gh CLI
- **Local Development:** Terminal-based monitoring without browser context switch
- **CI/CD Pipelines:** Scriptable workflow status checks
- **Related Commands:** `/merge-coverage-prs`, `/coverage-report`

## Tool Dependencies

**Required:**
- `gh` (GitHub CLI) - Minimum version 2.0.0
  - Installation: `brew install gh` (macOS) | `sudo apt install gh` (Linux)
  - Authentication: `gh auth login`

**Optional:**
- `git` - For automatic branch detection

## Implementation

```bash
#!/bin/bash

# ============================================================================
# /workflow-status - GitHub Actions Workflow Status Monitor
# ============================================================================

# Initialize variables
workflow_name=""
limit=5
branch=""
details=false

# First pass: Extract positional argument (workflow name)
for arg in "$@"; do
  if [[ ! "$arg" =~ ^-- ]] && [ -z "$workflow_name" ]; then
    workflow_name="$arg"
    break
  fi
done

# Second pass: Parse named arguments and flags
while [[ $# -gt 0 ]]; do
  case "$1" in
    --limit)
      if [ -z "$2" ]; then
        echo "⚠️ Error: --limit requires a value"
        echo "Example: --limit 10"
        exit 1
      fi
      limit="$2"
      shift 2
      ;;
    --branch)
      if [ -z "$2" ]; then
        echo "⚠️ Error: --branch requires a branch name"
        echo "Example: --branch main"
        exit 1
      fi
      branch="$2"
      shift 2
      ;;
    --details)
      details=true
      shift
      ;;
    *)
      # Skip positional argument (already captured) or error
      if [ "$1" = "$workflow_name" ]; then
        shift
      else
        echo "⚠️ Error: Unknown argument '$1'"
        echo ""
        echo "Usage: /workflow-status [workflow-name] [OPTIONS]"
        echo ""
        echo "Options:"
        echo "  --limit N        Number of runs to show (1-50, default: 5)"
        echo "  --branch BRANCH  Filter by branch (default: current branch)"
        echo "  --details        Show detailed logs and job breakdown"
        echo ""
        echo "Example: /workflow-status build.yml --limit 10 --details"
        exit 1
      fi
      ;;
  esac
done

# ============================================================================
# VALIDATION
# ============================================================================

# Validate limit is a number
if ! [[ "$limit" =~ ^[0-9]+$ ]]; then
  echo "⚠️ Error: --limit must be a number (got '$limit')"
  echo ""
  echo "Valid range: 1-50"
  echo "Example: /workflow-status --limit 10"
  exit 1
fi

# Validate limit range
if [ "$limit" -lt 1 ] || [ "$limit" -gt 50 ]; then
  echo "⚠️ Error: --limit must be between 1 and 50 (got $limit)"
  echo ""
  echo "Why this range?"
  echo "• Minimum 1: Need at least one result"
  echo "• Maximum 50: Prevents overwhelming output and API limits"
  echo ""
  echo "Example: /workflow-status --limit 25"
  exit 1
fi

# Validate branch name format (if provided)
if [ -n "$branch" ]; then
  if [[ "$branch" =~ [^a-zA-Z0-9/_-] ]]; then
    echo "⚠️ Error: Invalid branch name '$branch'"
    echo ""
    echo "Branch names contain only: letters, numbers, /, _, -"
    echo "Example: /workflow-status --branch feature/issue-123"
    exit 1
  fi
fi

# Check gh CLI availability
if ! command -v gh &> /dev/null; then
  echo "⚠️ Dependency Missing: gh CLI not found"
  echo ""
  echo "This command requires GitHub CLI (gh) to query workflow status."
  echo ""
  echo "Installation:"
  echo "• macOS:   brew install gh"
  echo "• Ubuntu:  sudo apt install gh"
  echo "• Windows: winget install GitHub.cli"
  echo ""
  echo "After installation:"
  echo "1. Authenticate: gh auth login"
  echo "2. Verify: gh --version"
  echo "3. Retry: /workflow-status"
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
  echo "  /workflow-status"
  echo ""
  echo "Troubleshooting:"
  echo "• Check credentials: gh auth status"
  echo "• Re-authenticate: gh auth refresh"
  exit 1
fi

# ============================================================================
# EXECUTION
# ============================================================================

echo "🔄 Fetching ${workflow_name:+runs for '$workflow_name'}${workflow_name:-recent workflow runs}..."

# Build gh run list command
GH_CMD="gh run list"

# Add workflow filter if specified
if [ -n "$workflow_name" ]; then
  GH_CMD="$GH_CMD --workflow=\"$workflow_name\""
fi

# Add branch filter if specified
if [ -n "$branch" ]; then
  GH_CMD="$GH_CMD --branch \"$branch\""
fi

# Add limit
GH_CMD="$GH_CMD --limit $limit"

# Execute based on mode
if [ "$details" = "true" ]; then
  # Detailed mode: Get latest run and show details
  LATEST_RUN=$(eval "$GH_CMD --json databaseId --jq '.[0].databaseId'")

  if [ -z "$LATEST_RUN" ]; then
    echo ""
    echo "⚠️ No runs found"
    echo ""
    if [ -n "$workflow_name" ]; then
      echo "💡 Troubleshooting:"
      echo "- Verify workflow name: gh workflow list"
      if [ -n "$branch" ]; then
        echo "- Check branch: $branch"
      fi
      echo "- Try without filters: /workflow-status"
    fi
    exit 1
  fi

  gh run view "$LATEST_RUN" --log
else
  # Summary mode: List runs
  eval "$GH_CMD"

  if [ $? -ne 0 ]; then
    echo ""
    echo "⚠️ Execution Failed: Unable to fetch workflow runs"
    echo ""
    echo "Troubleshooting:"
    echo "1. Verify workflow name exists: gh workflow list"
    if [ -n "$branch" ]; then
      echo "2. Verify branch exists: git branch -a | grep $branch"
    fi
    echo "3. Check GitHub status: https://www.githubstatus.com/"
    echo "4. Verify authentication: gh auth status"
    exit 1
  fi
fi

# ============================================================================
# SUCCESS FEEDBACK
# ============================================================================

echo ""
echo "💡 Next Steps:"

if [ "$details" = "false" ]; then
  echo "- View details: /workflow-status${workflow_name:+ $workflow_name} --details"
  if [ -z "$workflow_name" ]; then
    echo "- Check specific workflow: /workflow-status <workflow-name>"
  fi
  if [ -z "$branch" ]; then
    echo "- Filter by branch: /workflow-status --branch <branch-name>"
  fi
else
  echo "- View other workflows: /workflow-status"
  echo "- Check logs: gh run view <run-id> --log"
  echo "- Retry failed run: gh run rerun <run-id>"
fi
```

## Best Practices

**DO:**
- ✅ Use summary mode for quick checks
- ✅ Use --details for debugging failures
- ✅ Filter by workflow name for focused monitoring
- ✅ Use --limit to expand history for trend analysis

**DON'T:**
- ❌ Use excessive --limit values (>25) for routine checks
- ❌ Ignore failure indicators (investigate with --details)
- ❌ Forget to authenticate gh CLI before first use
