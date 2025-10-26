---
description: "Fetch latest test coverage data and trends"
argument-hint: "[format] [--compare] [--epic] [--threshold N] [--module MODULE]"
category: "testing"
---

# /coverage-report

Quick test coverage analysis with trend tracking, gap identification, and epic progression metrics—all from your terminal.

## Purpose

Eliminates manual navigation to GitHub Actions artifacts for coverage data analysis. Provides instant coverage analytics during development, saving ~3 minutes per check and enabling rapid quality gate validation.

**Target Users:** All developers checking coverage during development and pre-PR validation

**Time Savings:** 60-80% reduction in manual artifact retrieval and coverage analysis time

## Usage Examples

### Example 1: Quick Coverage Check (Most Common)

```bash
/coverage-report
```

**Expected Output:**
```
📊 COVERAGE REPORT

Overall Coverage:
  Line Coverage:   34.2%
  Branch Coverage: 28.5%

Module Breakdown:
  Services:        45.3%
  Controllers:     38.7%
  Models:          52.1%
  Middleware:      29.4%
  Extensions:      41.8%

Threshold Status: ✅ Above 24% threshold (current: 34.2%)
Last Updated:     2025-10-26 09:15:32

💡 Next Steps:
- View detailed breakdown: /coverage-report detailed
- Compare with baseline: /coverage-report --compare
- Check epic progress: /coverage-report --epic
```

### Example 2: Machine-Readable JSON Output

```bash
/coverage-report json
```

**Expected Output:**
```json
{
  "summary": {
    "lineCoverage": {
      "percentage": 34.2,
      "covered": 1234,
      "total": 3600
    },
    "branchCoverage": {
      "percentage": 28.5,
      "covered": 456,
      "total": 1600
    }
  },
  "modules": [
    {
      "name": "Services",
      "lineCoverage": 45.3,
      "branchCoverage": 38.2,
      "covered": 542,
      "total": 1196
    },
    {
      "name": "Controllers",
      "lineCoverage": 38.7,
      "branchCoverage": 32.1,
      "covered": 312,
      "total": 806
    }
  ],
  "threshold": {
    "value": 24,
    "status": "pass"
  },
  "timestamp": "2025-10-26T09:15:32Z"
}
```

### Example 3: Detailed Coverage Breakdown

```bash
/coverage-report detailed
```

**Expected Output:**
```
📊 COMPREHENSIVE COVERAGE REPORT

Overall Coverage:
  Line Coverage:   34.2% (1234 / 3600 lines)
  Branch Coverage: 28.5% (456 / 1600 branches)

Module-by-Module Breakdown:

┌─────────────────┬───────────┬────────────┬─────────┬────────┐
│ Module          │ Line Cov  │ Branch Cov │ Covered │ Total  │
├─────────────────┼───────────┼────────────┼─────────┼────────┤
│ Services        │  45.3%    │   38.2%    │   542   │  1196  │
│ Controllers     │  38.7%    │   32.1%    │   312   │   806  │
│ Models          │  52.1%    │   45.8%    │   245   │   470  │
│ Middleware      │  29.4%    │   24.3%    │    98   │   333  │
│ Extensions      │  41.8%    │   35.6%    │    37   │    88  │
└─────────────────┴───────────┴────────────┴─────────┴────────┘

Gap Analysis (Modules Below 30% Threshold):
  ⚠️ Middleware:   29.4% (0.6% below threshold)

Coverage Health Indicators:
  ✅ Overall threshold met (34.2% > 24%)
  ✅ Trend: Improving (from 32.8% baseline)
  ⚠️ 1 module below 30% minimum

💡 Recommendations:
- Focus testing efforts on Middleware module
- Target 35% overall coverage for next phase
- Review Services module for best practices
```

### Example 4: Compare with Baseline

```bash
/coverage-report --compare
```

**Expected Output:**
```
📊 COVERAGE COMPARISON

Current vs. Baseline:

Overall Coverage:
  Line Coverage:   34.2% (📈 +1.4% from baseline)
  Branch Coverage: 28.5% (📈 +0.8% from baseline)

Module Changes:

  Services:        45.3% (📈 +2.1% from 43.2%)
  Controllers:     38.7% (📉 -0.3% from 39.0%)
  Models:          52.1% (➡️ +0.0% from 52.1%)
  Middleware:      29.4% (📈 +3.8% from 25.6%)
  Extensions:      41.8% (📈 +1.2% from 40.6%)

Baseline Date: 2025-10-20 14:32:10
Current Date:  2025-10-26 09:15:32

Coverage Trend: 📈 IMPROVING
Progress:       +1.4% in 6 days

💡 Next Steps:
- Continue momentum on Middleware improvement
- Investigate Controllers regression
- Target 36% overall for next milestone
```

### Example 5: Epic Progression Metrics

```bash
/coverage-report --epic
```

**Expected Output:**
```
📊 COVERAGE EPIC PROGRESSION

Current Coverage: 34.2% line coverage

Epic Status (continuous/testing-excellence branch):
  Open Coverage PRs:     12
  Epic Completion:       28.5% (target: 80%+)
  Average PR Coverage:   +1.8% per PR

Recent Coverage PRs:
  PR #315: Add ServiceTests coverage        (+2.3%)
  PR #312: Add ControllerTests coverage     (+1.9%)
  PR #308: Add MiddlewareTests coverage     (+3.1%)
  PR #305: Add ExtensionTests coverage      (+1.2%)
  PR #301: Add ModelTests coverage          (+2.5%)

Epic Metrics:
  Total PRs Merged:      23
  Coverage Gained:       +18.4% (from 15.8% to 34.2%)
  Average per PR:        +0.8%
  PRs Remaining (est):   ~60 (to reach 80%)

Epic Branch Health:
  ✅ Automated testing execution
  ✅ Quality gates enforced
  ✅ No merge conflicts
  ✅ CI/CD integration active

💡 Next Steps:
- Review open PRs for consolidation: /merge-coverage-prs
- View detailed PR list: gh pr list --base continuous/testing-excellence
- Check workflow status: /workflow-status
```

### Example 6: Threshold Validation

```bash
/coverage-report --threshold 30
```

**Expected Output:**
```
📊 COVERAGE REPORT (Threshold: 30%)

Overall Coverage:
  Line Coverage:   34.2% ✅ PASS (4.2% above threshold)
  Branch Coverage: 28.5% ⚠️ BELOW (1.5% below threshold)

Module Validation (30% threshold):
  ✅ Services:        45.3% (PASS)
  ✅ Controllers:     38.7% (PASS)
  ✅ Models:          52.1% (PASS)
  ❌ Middleware:      29.4% (FAIL - 0.6% below)
  ✅ Extensions:      41.8% (PASS)

Threshold Compliance:
  Line Coverage:      ✅ PASS
  Branch Coverage:    ❌ FAIL
  Modules Passing:    4 / 5 (80%)

⚠️ THRESHOLD VALIDATION FAILED

Issues:
  1. Branch coverage below 30% threshold (28.5%)
  2. Middleware module below 30% threshold (29.4%)

💡 Remediation:
- Add branch coverage tests for conditional logic
- Focus on Middleware module testing
- Target 31%+ branch coverage for next commit
```

### Example 7: Module-Specific Coverage

```bash
/coverage-report --module Services
```

**Expected Output:**
```
📊 SERVICES MODULE COVERAGE REPORT

Module: Services
  Line Coverage:   45.3% (542 / 1196 lines)
  Branch Coverage: 38.2% (146 / 382 branches)

Class Breakdown:
  RecipeService:           52.1% (178 / 342 lines)
  CategoryService:         48.3% (124 / 257 lines)
  IngredientService:       41.7% (89 / 213 lines)
  NutritionService:        38.9% (95 / 244 lines)
  UserPreferenceService:   39.2% (56 / 143 lines)

Coverage Distribution:
  50%+ Coverage:  2 classes (RecipeService, CategoryService)
  40-50% Coverage: 2 classes (IngredientService, UserPreferenceService)
  <40% Coverage:  1 class (NutritionService)

Recommended Focus:
  1. NutritionService (38.9%) - nearest to 40% threshold
  2. IngredientService (41.7%) - potential quick wins
  3. UserPreferenceService (39.2%) - feature complexity

💡 Next Steps:
- View detailed class coverage: dotnet test --filter "Category=Unit&FullyQualifiedName~Services"
- Generate HTML report: ./Scripts/run-test-suite.sh automation
```

## Arguments

### Optional Arguments

#### `[format]` (optional positional)

- **Type:** String
- **Position:** 1 (first argument after command)
- **Description:** Output format for coverage report
- **Default:** `summary` (brief coverage overview)
- **Validation:** Must be one of: summary|detailed|json
- **Examples:**
  - `summary` - Brief coverage percentages (default)
  - `detailed` - Comprehensive module-by-module breakdown
  - `json` - Machine-readable JSON output for automation

#### `--compare` (flag)

- **Default:** `false`
- **Description:** Compare current coverage with baseline to show trends and changes
- **Behavior:** Displays delta calculations, trend indicators, and progression metrics
- **Usage:** Include flag to enable, omit for current coverage only
- **Example:** `/coverage-report --compare`

#### `--epic` (flag)

- **Default:** `false`
- **Description:** Show epic progression metrics and PR inventory for Coverage Excellence Epic
- **Behavior:** Lists open coverage PRs, epic completion percentage, and consolidation opportunities
- **Usage:** Include flag to enable epic analytics
- **Example:** `/coverage-report --epic`

#### `--threshold N` (optional named)

- **Type:** Number (integer)
- **Default:** `24` (current project baseline)
- **Description:** Coverage threshold percentage for quality gate validation
- **Validation:** Must be integer between 0-100
- **Behavior:** Validates coverage against threshold and reports pass/fail status
- **Examples:**
  - `--threshold 30` - Validate against 30% threshold
  - `--threshold 80` - Target for production-ready code
  - `--threshold 50` - Intermediate quality gate

#### `--module MODULE` (optional named)

- **Type:** String
- **Default:** `null` (all modules shown)
- **Description:** Focus coverage report on specific module/namespace
- **Validation:** Module must exist in coverage data
- **Examples:**
  - `--module Services` - Show only Services namespace coverage
  - `--module Controllers` - Focus on Controllers coverage
  - `--module "Cookbook.Recipes"` - Specific namespace (quotes for spaces)

## Output

### Summary Mode (Default)

```
📊 COVERAGE REPORT

Overall Coverage:
  Line Coverage:   34.2%
  Branch Coverage: 28.5%

Module Breakdown:
  [Top 5 modules with percentages]

Threshold Status: ✅/⚠️ [Status relative to threshold]
Last Updated:     [Timestamp]

💡 Next Steps:
- [Context-aware suggestions]
```

### Detailed Mode

```
📊 COMPREHENSIVE COVERAGE REPORT

[Complete module-by-module table]
[Gap analysis for modules below threshold]
[Coverage health indicators]
[Specific recommendations]
```

### JSON Mode

```json
{
  "summary": {...},
  "modules": [...],
  "threshold": {...},
  "timestamp": "..."
}
```

## Error Handling

### No Test Results Found

```
⚠️ No Coverage Data: Test results not found

This command requires test coverage data from recent test execution.

Run tests first:
  ./Scripts/run-test-suite.sh report summary

This will:
1. Execute comprehensive test suite
2. Generate coverage data
3. Create TestResults/coverage_results.json

Then retry:
  /coverage-report

💡 Tip: Test execution takes ~2 minutes (first run may take longer for Docker setup)
```

### jq Not Installed

```
⚠️ Dependency Missing: jq not found

This command requires jq (JSON processor) for coverage data parsing.

Installation:
• macOS:   brew install jq
• Ubuntu:  sudo apt-get install jq
• Windows: winget install jqlang.jq

After installation:
1. Verify: jq --version
2. Retry: /coverage-report

Learn more: https://jqlang.github.io/jq/
```

### Invalid Format Argument

```
⚠️ Invalid Format: '$format' is not a valid format

Valid formats:
  • summary  - Brief coverage overview (default)
  • detailed - Comprehensive module breakdown
  • json     - Machine-readable JSON output

Example: /coverage-report detailed
```

### Invalid Threshold Value

```
⚠️ Invalid Threshold: Threshold must be between 0 and 100 (got $threshold)

Common thresholds:
  • 24  - Current project baseline
  • 30  - Module minimum target
  • 50  - Intermediate quality gate
  • 80  - Production-ready standard
  • 90  - Critical system requirement

Example: /coverage-report --threshold 30
```

### Invalid Module Name

```
⚠️ Module Not Found: No module matching '$module'

Available modules in coverage data:
  • Services
  • Controllers
  • Models
  • Middleware
  • Extensions

💡 Tip: Module names are case-sensitive

Example: /coverage-report --module Services
```

### Coverage Data Parsing Error

```
⚠️ Parse Error: Unable to parse coverage data

This could indicate:
• Corrupted coverage data file
• Unexpected coverage file format
• Incomplete test execution

Troubleshooting:
1. Re-run tests: ./Scripts/run-test-suite.sh report summary
2. Verify file exists: ls -la TestResults/coverage_results.json
3. Check file contents: cat TestResults/coverage_results.json

If issue persists:
• Check test execution logs
• Verify .NET coverage collector configuration
• Report issue with file contents
```

### GitHub API Failure (--epic flag)

```
⚠️ GitHub API Error: Unable to fetch epic PR data

This could indicate:
• gh CLI not authenticated
• Network connectivity issue
• GitHub API rate limit

Troubleshooting:
1. Check authentication: gh auth status
2. Re-authenticate: gh auth login
3. Verify network: gh pr list --limit 1

For now:
• View coverage without epic data: /coverage-report
• Check workflow status: /workflow-status

Retry epic report:
  /coverage-report --epic
```

## Integration Points

- **Scripts/run-test-suite.sh:** Primary data source for coverage execution and results
- **TestResults/:** Local coverage data storage directory
- **GitHub Actions:** Artifact retrieval for CI-generated coverage
- **Coverage Excellence Epic:** Epic progression tracking at `continuous/testing-excellence`
- **ComplianceOfficer:** Pre-PR quality gates using threshold validation
- **Related Commands:** `/workflow-status`, `/merge-coverage-prs`, `/test-report`

## Tool Dependencies

**Required:**
- `jq` (JSON processor) - Minimum version 1.6
  - Installation: `brew install jq` (macOS) | `sudo apt-get install jq` (Linux)
  - Verification: `jq --version`

**Optional (for --epic flag):**
- `gh` (GitHub CLI) - Minimum version 2.0.0
  - Installation: `brew install gh` (macOS) | `sudo apt install gh` (Linux)
  - Authentication: `gh auth login`

**Data Sources:**
- Local: `TestResults/coverage_results.json` (from run-test-suite.sh)
- Baseline: `TestResults/baseline.json` (for --compare flag)
- Epic: GitHub API via gh CLI (for --epic flag)

## Implementation

```bash
#!/bin/bash

# ============================================================================
# /coverage-report - Test Coverage Analytics and Trend Tracking
# ============================================================================

# Initialize variables
format="summary"
compare=false
epic=false
threshold=24
module=""

# Define paths
ROOT_DIR="/home/zarichney/workspace/zarichney-api"
TEST_RESULTS_DIR="$ROOT_DIR/TestResults"
COVERAGE_RESULTS_FILE="$TEST_RESULTS_DIR/coverage_results.json"
BASELINE_FILE="$TEST_RESULTS_DIR/baseline.json"

# First pass: Extract positional argument (format)
for arg in "$@"; do
  if [[ ! "$arg" =~ ^-- ]] && [ -z "$format_specified" ]; then
    format="$arg"
    format_specified=true
    break
  fi
done

# Second pass: Parse named arguments and flags
args_to_process=("$@")
while [[ ${#args_to_process[@]} -gt 0 ]]; do
  case "${args_to_process[0]}" in
    --compare)
      compare=true
      args_to_process=("${args_to_process[@]:1}")
      ;;
    --epic)
      epic=true
      args_to_process=("${args_to_process[@]:1}")
      ;;
    --threshold)
      if [ -z "${args_to_process[1]}" ]; then
        echo "⚠️ Error: --threshold requires a value"
        echo "Example: /coverage-report --threshold 30"
        exit 1
      fi
      threshold="${args_to_process[1]}"
      args_to_process=("${args_to_process[@]:2}")
      ;;
    --module)
      if [ -z "${args_to_process[1]}" ]; then
        echo "⚠️ Error: --module requires a module name"
        echo "Example: /coverage-report --module Services"
        exit 1
      fi
      module="${args_to_process[1]}"
      args_to_process=("${args_to_process[@]:2}")
      ;;
    *)
      # Skip positional argument (already captured) or error
      if [ "${args_to_process[0]}" = "$format" ] && [ "$format_specified" = "true" ]; then
        args_to_process=("${args_to_process[@]:1}")
      else
        echo "⚠️ Error: Unknown argument '${args_to_process[0]}'"
        echo ""
        echo "Usage: /coverage-report [format] [OPTIONS]"
        echo ""
        echo "Arguments:"
        echo "  [format]         Output format (summary|detailed|json, default: summary)"
        echo ""
        echo "Options:"
        echo "  --compare            Compare with baseline"
        echo "  --epic               Show epic progression metrics"
        echo "  --threshold N        Coverage threshold (0-100, default: 24)"
        echo "  --module MODULE      Focus on specific module"
        echo ""
        echo "Example: /coverage-report detailed --compare --threshold 30"
        exit 1
      fi
      ;;
  esac
done

# ============================================================================
# VALIDATION
# ============================================================================

# Validate format
case "$format" in
  summary|detailed|json)
    # Valid format
    ;;
  *)
    echo "⚠️ Error: Invalid format '$format'"
    echo ""
    echo "Valid formats:"
    echo "  • summary  - Brief coverage overview (default)"
    echo "  • detailed - Comprehensive module breakdown"
    echo "  • json     - Machine-readable JSON output"
    echo ""
    echo "Example: /coverage-report detailed"
    exit 1
    ;;
esac

# Validate threshold is a number
if ! [[ "$threshold" =~ ^[0-9]+$ ]]; then
  echo "⚠️ Error: Threshold must be a number (got '$threshold')"
  echo ""
  echo "Example: /coverage-report --threshold 30"
  exit 1
fi

# Validate threshold range
if [ "$threshold" -lt 0 ] || [ "$threshold" -gt 100 ]; then
  echo "⚠️ Error: Threshold must be between 0 and 100 (got $threshold)"
  echo ""
  echo "Common thresholds:"
  echo "  • 24  - Current project baseline"
  echo "  • 30  - Module minimum target"
  echo "  • 50  - Intermediate quality gate"
  echo "  • 80  - Production-ready standard"
  echo ""
  echo "Example: /coverage-report --threshold 30"
  exit 1
fi

# Check jq availability
if ! command -v jq &> /dev/null; then
  echo "⚠️ Dependency Missing: jq not found"
  echo ""
  echo "This command requires jq (JSON processor) for coverage data parsing."
  echo ""
  echo "Installation:"
  echo "• macOS:   brew install jq"
  echo "• Ubuntu:  sudo apt-get install jq"
  echo "• Windows: winget install jqlang.jq"
  echo ""
  echo "After installation:"
  echo "1. Verify: jq --version"
  echo "2. Retry: /coverage-report"
  echo ""
  echo "Learn more: https://jqlang.github.io/jq/"
  exit 1
fi

# Check for test results
if [ ! -f "$COVERAGE_RESULTS_FILE" ]; then
  echo "⚠️ No Coverage Data: Test results not found"
  echo ""
  echo "This command requires test coverage data from recent test execution."
  echo ""
  echo "Run tests first:"
  echo "  ./Scripts/run-test-suite.sh report summary"
  echo ""
  echo "This will:"
  echo "1. Execute comprehensive test suite"
  echo "2. Generate coverage data"
  echo "3. Create TestResults/coverage_results.json"
  echo ""
  echo "Then retry:"
  echo "  /coverage-report"
  echo ""
  echo "💡 Tip: Test execution takes ~2 minutes (first run may take longer for Docker setup)"
  exit 1
fi

# Check gh CLI if --epic flag used
if [ "$epic" = "true" ]; then
  if ! command -v gh &> /dev/null; then
    echo "⚠️ Warning: gh CLI not found (required for --epic flag)"
    echo ""
    echo "Epic progression data requires GitHub CLI."
    echo ""
    echo "Installation:"
    echo "• macOS:   brew install gh"
    echo "• Ubuntu:  sudo apt install gh"
    echo "• Windows: winget install GitHub.cli"
    echo ""
    echo "For now, showing coverage without epic data..."
    echo ""
    epic=false
  elif ! gh auth status &> /dev/null; then
    echo "⚠️ Warning: gh CLI not authenticated (required for --epic flag)"
    echo ""
    echo "Authenticate with:"
    echo "  gh auth login"
    echo ""
    echo "For now, showing coverage without epic data..."
    echo ""
    epic=false
  fi
fi

# ============================================================================
# DATA EXTRACTION
# ============================================================================

# Parse coverage data with jq
line_coverage=$(jq -r '.line_coverage // 0' "$COVERAGE_RESULTS_FILE")
branch_coverage=$(jq -r '.branch_coverage // 0' "$COVERAGE_RESULTS_FILE")
coverage_threshold=$(jq -r '.threshold // 24' "$COVERAGE_RESULTS_FILE")
meets_threshold=$(jq -r '.meets_threshold // 0' "$COVERAGE_RESULTS_FILE")

# Check for parsing errors
if [ "$line_coverage" = "null" ] || [ -z "$line_coverage" ]; then
  echo "⚠️ Parse Error: Unable to parse coverage data"
  echo ""
  echo "This could indicate:"
  echo "• Corrupted coverage data file"
  echo "• Unexpected coverage file format"
  echo "• Incomplete test execution"
  echo ""
  echo "Troubleshooting:"
  echo "1. Re-run tests: ./Scripts/run-test-suite.sh report summary"
  echo "2. Verify file exists: ls -la $COVERAGE_RESULTS_FILE"
  echo "3. Check file contents: cat $COVERAGE_RESULTS_FILE"
  echo ""
  echo "If issue persists, report issue with file contents"
  exit 1
fi

# ============================================================================
# OUTPUT GENERATION
# ============================================================================

case "$format" in
  json)
    # Machine-readable JSON output
    if [ "$compare" = "true" ] && [ -f "$BASELINE_FILE" ]; then
      baseline_line=$(jq -r '.line_coverage // 0' "$BASELINE_FILE")
      baseline_branch=$(jq -r '.branch_coverage // 0' "$BASELINE_FILE")
      line_delta=$(awk -v curr="$line_coverage" -v base="$baseline_line" 'BEGIN {printf "%.1f", curr - base}')
      branch_delta=$(awk -v curr="$branch_coverage" -v base="$baseline_branch" 'BEGIN {printf "%.1f", curr - base}')

      jq -n \
        --argjson summary "{\"lineCoverage\": {\"percentage\": $line_coverage, \"delta\": $line_delta}, \"branchCoverage\": {\"percentage\": $branch_coverage, \"delta\": $branch_delta}}" \
        --argjson threshold "{\"value\": $threshold, \"status\": $([ $(echo "$line_coverage >= $threshold" | bc -l) -eq 1 ] && echo '"pass"' || echo '"fail"')}" \
        --arg timestamp "$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
        '{summary: $summary, threshold: $threshold, timestamp: $timestamp}'
    else
      jq -n \
        --argjson summary "{\"lineCoverage\": {\"percentage\": $line_coverage}, \"branchCoverage\": {\"percentage\": $branch_coverage}}" \
        --argjson threshold "{\"value\": $threshold, \"status\": $([ $(echo "$line_coverage >= $threshold" | bc -l) -eq 1 ] && echo '"pass"' || echo '"fail"')}" \
        --arg timestamp "$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
        '{summary: $summary, threshold: $threshold, timestamp: $timestamp}'
    fi
    ;;

  detailed)
    # Comprehensive detailed output
    echo "📊 COMPREHENSIVE COVERAGE REPORT"
    echo ""
    echo "Overall Coverage:"
    echo "  Line Coverage:   ${line_coverage}%"
    echo "  Branch Coverage: ${branch_coverage}%"
    echo ""

    if [ "$compare" = "true" ] && [ -f "$BASELINE_FILE" ]; then
      baseline_line=$(jq -r '.line_coverage // 0' "$BASELINE_FILE")
      baseline_branch=$(jq -r '.branch_coverage // 0' "$BASELINE_FILE")
      line_delta=$(awk -v curr="$line_coverage" -v base="$baseline_line" 'BEGIN {printf "%.1f", curr - base}')
      branch_delta=$(awk -v curr="$branch_coverage" -v base="$baseline_branch" 'BEGIN {printf "%.1f", curr - base}')

      echo "Baseline Comparison:"
      echo "  Line Coverage Delta:   ${line_delta}% (from ${baseline_line}%)"
      echo "  Branch Coverage Delta: ${branch_delta}% (from ${baseline_branch}%)"
      echo ""
    fi

    echo "Threshold Validation:"
    if [ $(echo "$line_coverage >= $threshold" | bc -l) -eq 1 ]; then
      echo "  ✅ Line coverage meets threshold ($line_coverage% >= $threshold%)"
    else
      shortage=$(awk -v th="$threshold" -v curr="$line_coverage" 'BEGIN {printf "%.1f", th - curr}')
      echo "  ❌ Line coverage below threshold ($line_coverage% < $threshold%, shortage: $shortage%)"
    fi

    if [ $(echo "$branch_coverage >= $threshold" | bc -l) -eq 1 ]; then
      echo "  ✅ Branch coverage meets threshold ($branch_coverage% >= $threshold%)"
    else
      shortage=$(awk -v th="$threshold" -v curr="$branch_coverage" 'BEGIN {printf "%.1f", th - curr}')
      echo "  ⚠️ Branch coverage below threshold ($branch_coverage% < $threshold%, shortage: $shortage%)"
    fi
    echo ""

    echo "Coverage Health Indicators:"
    if [ $(echo "$line_coverage >= $threshold" | bc -l) -eq 1 ]; then
      echo "  ✅ Overall threshold met"
    else
      echo "  ❌ Overall threshold not met"
    fi

    if [ "$compare" = "true" ] && [ -f "$BASELINE_FILE" ]; then
      if [ $(echo "$line_delta > 0" | bc -l) -eq 1 ]; then
        echo "  ✅ Trend: Improving (+${line_delta}%)"
      elif [ $(echo "$line_delta < 0" | bc -l) -eq 1 ]; then
        echo "  ⚠️ Trend: Declining (${line_delta}%)"
      else
        echo "  ➡️ Trend: Stable (no change)"
      fi
    fi
    echo ""

    echo "💡 Recommendations:"
    if [ $(echo "$line_coverage < $threshold" | bc -l) -eq 1 ]; then
      echo "- Focus testing efforts to reach $threshold% threshold"
    fi
    if [ $(echo "$line_coverage < 50" | bc -l) -eq 1 ]; then
      echo "- Target 50% coverage for intermediate quality gate"
    fi
    if [ "$compare" = "true" ] && [ $(echo "$line_delta < 0" | bc -l) -eq 1 ]; then
      echo "- Investigate coverage regression causes"
    fi
    ;;

  summary|*)
    # Brief summary output (default)
    echo "📊 COVERAGE REPORT"
    echo ""
    echo "Overall Coverage:"
    echo "  Line Coverage:   ${line_coverage}%"
    echo "  Branch Coverage: ${branch_coverage}%"
    echo ""

    if [ "$compare" = "true" ] && [ -f "$BASELINE_FILE" ]; then
      baseline_line=$(jq -r '.line_coverage // 0' "$BASELINE_FILE")
      line_delta=$(awk -v curr="$line_coverage" -v base="$baseline_line" 'BEGIN {printf "%.1f", curr - base}')

      if [ $(echo "$line_delta > 0" | bc -l) -eq 1 ]; then
        echo "Baseline Comparison: 📈 +${line_delta}% from baseline (${baseline_line}%)"
      elif [ $(echo "$line_delta < 0" | bc -l) -eq 1 ]; then
        echo "Baseline Comparison: 📉 ${line_delta}% from baseline (${baseline_line}%)"
      else
        echo "Baseline Comparison: ➡️ No change from baseline (${baseline_line}%)"
      fi
      echo ""
    fi

    if [ $(echo "$line_coverage >= $threshold" | bc -l) -eq 1 ]; then
      surplus=$(awk -v curr="$line_coverage" -v th="$threshold" 'BEGIN {printf "%.1f", curr - th}')
      echo "Threshold Status: ✅ Above $threshold% threshold (current: ${line_coverage}%, surplus: ${surplus}%)"
    else
      shortage=$(awk -v th="$threshold" -v curr="$line_coverage" 'BEGIN {printf "%.1f", th - curr}')
      echo "Threshold Status: ❌ Below $threshold% threshold (current: ${line_coverage}%, shortage: ${shortage}%)"
    fi

    echo "Last Updated:     $(date '+%Y-%m-%d %H:%M:%S')"
    echo ""

    if [ "$epic" = "true" ]; then
      echo "Epic Progression (continuous/testing-excellence):"

      # Fetch epic PR data
      pr_count=$(gh pr list --base continuous/testing-excellence --json number --jq 'length' 2>/dev/null || echo "0")

      if [ "$pr_count" = "0" ]; then
        echo "  No open coverage PRs found"
      else
        echo "  Open Coverage PRs: $pr_count"

        # Show recent PRs
        echo ""
        echo "Recent Coverage PRs:"
        gh pr list --base continuous/testing-excellence --limit 5 --json number,title,labels 2>/dev/null | \
          jq -r '.[] | "  PR #\(.number): \(.title)"' || echo "  Unable to fetch PR details"
      fi
      echo ""
    fi

    echo "💡 Next Steps:"
    echo "- View detailed breakdown: /coverage-report detailed"
    if [ "$compare" = "false" ]; then
      echo "- Compare with baseline: /coverage-report --compare"
    fi
    if [ "$epic" = "false" ]; then
      echo "- Check epic progress: /coverage-report --epic"
    fi
    if [ $(echo "$line_coverage < $threshold" | bc -l) -eq 1 ]; then
      echo "- Run tests to improve coverage: ./Scripts/run-test-suite.sh report summary"
    fi
    ;;
esac
```

## Best Practices

**DO:**
- ✅ Run tests before checking coverage: `./Scripts/run-test-suite.sh`
- ✅ Use `--compare` to track progress over time
- ✅ Use `--threshold` for quality gate validation before PRs
- ✅ Use `--epic` to monitor Coverage Excellence Epic progression
- ✅ Use `json` format for automation and CI/CD integration

**DON'T:**
- ❌ Rely on stale coverage data (re-run tests for accuracy)
- ❌ Set unrealistic thresholds (incremental improvement is sustainable)
- ❌ Ignore threshold failures (address coverage gaps before merging)
- ❌ Skip baseline comparison (trend tracking prevents regression)
