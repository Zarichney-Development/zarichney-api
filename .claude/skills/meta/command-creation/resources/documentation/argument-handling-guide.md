# Argument Handling Guide: Robust Parsing, Validation & Error Excellence

**Version:** 1.0.0
**Category:** Meta-Skill Resource
**Purpose:** Comprehensive guide to argument type selection, validation strategies, error message quality, and parsing patterns ensuring robust command interfaces with exceptional user experience

---

## TABLE OF CONTENTS

1. [Argument Type Selection Guide](#1-argument-type-selection-guide)
2. [Validation Strategies](#2-validation-strategies)
3. [Error Message Excellence](#3-error-message-excellence)
4. [Parsing Patterns](#4-parsing-patterns)
5. [Best Practices & Anti-Patterns](#5-best-practices--anti-patterns)

---

## 1. ARGUMENT TYPE SELECTION GUIDE

### 1.1 Positional Arguments

#### When to Use Positional Arguments

**Use Positional When:**

```yaml
REQUIRED_CORE_ARGUMENTS:
  Scenario: "Arguments essential to command purpose, always required"
  Rationale: "Natural sentence structure: verb + noun + object"

  Examples:
    /create-issue <type> <title>
      - type: Issue category (feature, bug, epic)
      - title: Issue title (always required)
      - Natural: "Create [type] issue titled [title]"

    /deploy <environment>
      - environment: Target deployment (staging, prod)
      - Natural: "Deploy to [environment]"

CLEAR_ARGUMENT_ORDER:
  Scenario: "Logical ordering exists, users expect specific sequence"
  Rationale: "Intuitive flow matches user mental model"

  Examples:
    /clone-issue <source-id> <new-title>
      - source-id: Issue to clone (comes first logically)
      - new-title: New issue title (modification comes second)
      - Natural: "Clone issue [source] with new title [title]"

CONCISE_SYNTAX:
  Scenario: "Frequently-used command, minimize typing"
  Rationale: "Positional arguments are shorter than named"

  Examples:
    /test <category>
      - category: unit|integration|e2e
      - Positional: /test unit (10 characters)
      - Named: /test --category unit (22 characters)
      - 55% typing reduction
```

**Benefits:**
- ✅ **Concise:** Fewer characters to type
- ✅ **Natural:** Reads like English sentence
- ✅ **Intuitive:** Order matches logical flow

**Drawbacks:**
- ❌ **Order Dependency:** Arguments must be in exact sequence
- ❌ **Harder to Extend:** Adding new required argument breaks existing usage
- ❌ **Less Self-Documenting:** User must remember order

---

#### When to Avoid Positional Arguments

```yaml
MANY_ARGUMENTS:
  Problem: "More than 3 positional arguments = order confusion"

  Bad_Example:
    /create-user <name> <email> <role> <department> <manager>
    # User must remember: name → email → role → department → manager

  Fix:
    /create-user <email> --name NAME --role ROLE --department DEPT --manager MGR
    # Only email required positional, rest are named (self-documenting)

OPTIONAL_ARGUMENTS:
  Problem: "Positional optional arguments confuse parsing"

  Bad_Example:
    /workflow-status [workflow-name] [limit]
    # Is "10" a workflow name or limit? Ambiguous.

  Fix:
    /workflow-status [workflow-name] --limit N
    # workflow-name positional optional, limit named (clear)

EXTENSIBILITY_NEEDED:
  Problem: "Future arguments will break command compatibility"

  Bad_Example:
    /deploy <environment>
    # Adding required <version> later: /deploy <environment> <version>
    # Breaks all existing usage

  Fix:
    /deploy <environment> [--version VERSION]
    # version optional named, extensible without breaking change
```

---

#### Positional Argument Best Practices

**Design Principles:**

```yaml
LIMIT_TO_1_3_ARGUMENTS:
  Guideline: "1-2 positional is ideal, 3 is maximum"
  Rationale: "More than 3 = order confusion and poor UX"

  Examples:
    ✅ /create-issue <type> <title> (2 positional)
    ✅ /clone-issue <source-id> <new-title> (2 positional)
    ⚠️ /create-user <name> <email> <role> (3 positional, acceptable)
    ❌ /create-pr <base> <head> <title> <body> <reviewers> (5 positional, BAD)

REQUIRED_FIRST:
  Guideline: "Required positional arguments come before optional"
  Rationale: "Clear parsing, no ambiguity"

  Examples:
    ✅ /workflow-status [workflow-name] --limit N (optional positional, named)
    ❌ /workflow-status --limit N [workflow-name] (confusing order)

NATURAL_ORDERING:
  Guideline: "Order matches logical workflow or sentence structure"
  Rationale: "Intuitive for users, matches mental model"

  Examples:
    ✅ /create-issue <type> <title> (type determines template, comes first)
    ✅ /clone-issue <source-id> <new-title> (source → modification)
    ❌ /create-issue <title> <type> (unnatural, type after title)
```

---

### 1.2 Named Arguments

#### When to Use Named Arguments

**Use Named When:**

```yaml
OPTIONAL_ARGUMENTS:
  Scenario: "Argument has sensible default, not always needed"
  Rationale: "Named syntax makes optional arguments self-documenting"

  Examples:
    /workflow-status --limit N
      - Default: 5 (balances context vs. overwhelming output)
      - Override: --limit 10 (expand history)
      - Self-documenting: User sees "--limit" understands purpose

    /create-issue --template TEMPLATE
      - Default: Auto-select based on type
      - Override: --template custom.md (use custom template)
      - Self-documenting: Explicit template choice

MANY_ARGUMENTS:
  Scenario: "More than 3 arguments total (positional + optional)"
  Rationale: "Named arguments reduce order confusion"

  Examples:
    /create-issue <type> <title> --template T --label L --milestone M --assignee A
      - 2 positional + 4 named = 6 total (manageable)
      - Named args: Order-independent, self-documenting

EXTENSIBILITY:
  Scenario: "Command will evolve, new arguments needed"
  Rationale: "Adding named arguments is non-breaking change"

  Examples:
    v1.0: /deploy <env>
    v1.1: /deploy <env> --version V (backward compatible)
    v1.2: /deploy <env> --version V --rollback-on-failure (backward compatible)
```

**Benefits:**
- ✅ **Self-Documenting:** Argument name clarifies purpose
- ✅ **Order-Independent:** Can specify in any sequence
- ✅ **Extensible:** Add new arguments without breaking compatibility
- ✅ **Explicit:** Clear intent when reading command

**Drawbacks:**
- ❌ **Verbose:** More characters to type
- ❌ **Redundant:** Name + value (e.g., `--type feature` vs. just `feature`)

---

#### Named Argument Naming Conventions

**Standards:**

```yaml
LOWERCASE_HYPHENATED:
  Format: "--lowercase-hyphenated"
  Examples:
    ✅ --dry-run
    ✅ --pr-label-filter
    ✅ --assignee
    ❌ --dryRun (camelCase, inconsistent with CLI conventions)
    ❌ --DRY_RUN (SCREAMING_SNAKE_CASE, too aggressive)

DESCRIPTIVE_NOT_ABBREVIATED:
  Format: "Full words, avoid abbreviations unless universally understood"
  Examples:
    ✅ --limit (universally understood)
    ✅ --template (clear)
    ✅ --assignee (full word)
    ❌ --tmpl (unclear abbreviation)
    ❌ --assgn (confusing)
    ⚠️ --max (acceptable if context is clear: --max-prs)

VALUE_PLACEHOLDER_UPPERCASE:
  Format: "--argument VALUE" (VALUE in uppercase)
  Examples:
    ✅ --template TEMPLATE
    ✅ --label LABEL
    ✅ --milestone MILESTONE
    ❌ --template template (confusing, looks like default)
    ❌ --label label (unclear)

SHORT_AND_LONG_OPTIONS:
  Format: "-s|--long-option" (optional short form for common args)
  Examples:
    /test-report -f json --format json (both work)
    /test-report -v --verbose (both work)

  When_To_Provide_Short:
    - Frequently used arguments (--limit → -l)
    - Industry standard shortcuts (--version → -v, --help → -h)

  When_To_Skip_Short:
    - Rarely used arguments (avoid -t for --template)
    - Risk of confusion (--type and --template both -t?)
```

---

#### Named Argument Best Practices

**Design Principles:**

```yaml
SENSIBLE_DEFAULTS:
  Guideline: "Every named argument should have smart default"
  Rationale: "Minimize required user input for common use case"

  Examples:
    --limit 5 (default: balances context vs. overwhelming)
    --branch "" (default: auto-detect current branch)
    --dry-run false (default: execute, not preview)

CONSISTENT_NAMING:
  Guideline: "Same concept = same argument name across all commands"
  Rationale: "Users learn once, apply everywhere"

  Examples:
    --dry-run: /create-issue, /deploy, /merge-coverage-prs (all use --dry-run)
    --label: /create-issue, /create-pr, /tag-release (all use --label)

  Violations_To_Avoid:
    ❌ /create-issue --dry-run vs. /deploy --preview (inconsistent)
    ❌ /create-issue --label vs. /create-pr --tag (same concept, different names)

VALUE_TYPE_CLARITY:
  Guideline: "Argument name implies expected value type"
  Rationale: "User knows what to provide without reading docs"

  Examples:
    --limit N (N implies number)
    --template TEMPLATE (implies string, file name)
    --assignee USER (implies GitHub username)
    --milestone MILESTONE (implies milestone name or number)
```

---

### 1.3 Flags (Boolean Toggles)

#### When to Use Flags

**Use Flags When:**

```yaml
BOOLEAN_TOGGLE:
  Scenario: "Argument is true/false, no value needed"
  Rationale: "Flag presence = true, absence = false (clear semantics)"

  Examples:
    /create-issue --dry-run
      - Present: dry_run=true (preview mode)
      - Absent: dry_run=false (execute mode)

    /workflow-status --details
      - Present: details=true (show full logs)
      - Absent: details=false (summary mode)

FEATURE_SWITCH:
  Scenario: "Enable optional feature or behavior"
  Rationale: "Flag makes feature opt-in, default is standard behavior"

  Examples:
    /test-report --verbose
      - Present: Verbose output (all test names)
      - Absent: Summary output (pass/fail counts)

    /coverage-report --include-skipped
      - Present: Include skipped tests in report
      - Absent: Only include executed tests

SAFETY_MECHANISM:
  Scenario: "Prevent accidental destructive operation"
  Rationale: "Flag makes user explicitly confirm intent"

  Examples:
    /deploy staging --skip-tests
      - Requires explicit flag to skip safety check
      - Default: Run tests before deploying

    /delete-branch --force
      - Requires explicit flag to delete unmerged branch
      - Default: Prevent accidental deletions
```

**Benefits:**
- ✅ **Concise:** No value needed (just `--flag`)
- ✅ **Clear Intent:** Presence = enabled, absence = disabled
- ✅ **No Ambiguity:** Can't have invalid value (true/false only)

**Drawbacks:**
- ❌ **Limited to Boolean:** Can't express multi-value options
- ❌ **No Tri-State:** Can't distinguish "not specified" from "false"

---

#### Flag Naming Conventions

**Standards:**

```yaml
POSITIVE_PHRASING:
  Guideline: "Flags should be positive actions, not negative"
  Rationale: "Double negatives confuse users"

  Examples:
    ✅ --verbose (enable verbosity)
    ✅ --include-skipped (include skipped tests)
    ✅ --dry-run (enable preview mode)
    ❌ --no-verbose (confusing double negative)
    ❌ --exclude-passed (negative phrasing, harder to understand)

ACTION_ORIENTED:
  Guideline: "Flag name describes what happens when enabled"
  Rationale: "User immediately understands effect"

  Examples:
    ✅ --watch (watch for changes, continuous mode)
    ✅ --details (show detailed information)
    ✅ --force (force operation, skip confirmations)
    ❌ --mode (unclear what this toggles)
    ❌ --option (generic, meaningless)

CONSISTENT_PATTERNS:
  Guideline: "Common flags use standard names across commands"
  Rationale: "Users learn once, apply everywhere"

  Examples:
    --dry-run: Preview mode across all write operations
    --verbose: Detailed output across all commands
    --watch: Continuous monitoring across all monitoring commands
    --force: Skip confirmations across all destructive commands
```

---

#### Flag Best Practices

**Design Principles:**

```yaml
DEFAULT_TO_SAFE:
  Guideline: "Flag default should be safest option"
  Rationale: "Prevent accidental destructive operations"

  Examples:
    /deploy <env> --skip-tests
      - Default: Run tests (safe)
      - Flag: Skip tests (faster but risky)

    /create-issue <type> <title> --dry-run
      - Default: Create issue (standard operation)
      - Wait, this violates principle! Fix:
      - Better: /create-issue <type> <title> (default: create)
      - Preview: /create-issue <type> <title> --dry-run (safe for testing)

MUTUALLY_EXCLUSIVE_FLAGS:
  Guideline: "Detect and reject conflicting flags"
  Rationale: "Prevent user confusion and undefined behavior"

  Examples:
    /test-report --summary --details
      - Error: "Cannot use --summary and --details together"
      - Suggest: "Use --summary OR --details, not both"

    /deploy staging --rollback --skip-rollback
      - Error: "Conflicting flags: --rollback and --skip-rollback"

  Implementation:
    if [ "$summary" = "true" ] && [ "$details" = "true" ]; then
      echo "⚠️ Conflicting Flags: --summary and --details are mutually exclusive"
      echo "Use --summary for condensed output OR --details for verbose output"
      exit 1
    fi

FLAG_DEPENDENCIES:
  Guideline: "Detect and enforce flag dependencies"
  Rationale: "Some flags only make sense with others"

  Examples:
    /workflow-status --details --format json
      - --format requires --details (JSON format only available in detailed mode)
      - Error if --format without --details

  Implementation:
    if [ -n "$format" ] && [ "$details" != "true" ]; then
      echo "⚠️ Dependency Error: --format requires --details"
      echo "Try: /workflow-status --details --format json"
      exit 1
    fi
```

---

### 1.4 Default Value Design

#### Principles for Smart Defaults

**Design Framework:**

```yaml
OPTIMIZE_FOR_80_PERCENT:
  Principle: "Defaults should serve most common use case"
  Analysis: "Review usage patterns, optimize for majority"

  Examples:
    /workflow-status --limit 5
      - Analysis: 80% of usage checks recent 3-7 runs
      - Default: 5 (midpoint, balances context vs. overwhelming)

    /create-issue --template ""
      - Analysis: 95% of time, type determines template
      - Default: Auto-select based on type (empty = auto)

SAFE_BY_DEFAULT:
  Principle: "Defaults should never be destructive"
  Analysis: "Prevent accidental data loss or production changes"

  Examples:
    /deploy <env> --dry-run false
      - Wait, should be: dry-run true for safety!
      - Analysis: Deployment is destructive, preview first
      - Better Default: dry-run=true (safe), use --execute to override

    /delete-branch <name> --force false
      - Default: Prevent unmerged branch deletion
      - Force required for risky operation

CONTEXTUAL_DEFAULTS:
  Principle: "Defaults should adapt to current context"
  Analysis: "Use environment information to set smart defaults"

  Examples:
    /workflow-status --branch ""
      - Default: Auto-detect current branch
      - Context: git rev-parse --abbrev-ref HEAD
      - Value: Relevant to user's active work

    /create-issue --milestone ""
      - Default: Auto-detect current epic (if on epic branch)
      - Context: Branch name like "epic/testing-coverage" → Epic milestone
      - Value: Contextually relevant
```

---

#### Default Documentation

**Communicate Defaults Clearly:**

```yaml
IN_HELP_TEXT:
  Format: "Optional:\n  --arg VALUE  Description (default: DEFAULT_VALUE)"

  Example:
    Optional:
      --limit N        Number of runs to show (default: 5)
      --branch BRANCH  Filter by branch (default: current branch)
      --dry-run        Preview mode (default: false for reads, true for writes)

IN_ARGUMENT_SPECIFICATION:
  Format: "Document default with rationale"

  Example:
    #### --limit N (optional named)
    - Type: Number (integer)
    - Default: 5
    - Rationale: 5 runs provide recent context without overwhelming output
    - Override: --limit 10 for expanded history

IN_OUTPUT:
  Format: "Show user what defaults were applied"

  Example:
    /workflow-status
    🔄 Fetching recent workflow runs (limit: 5, branch: feature/issue-123)...

    User sees:
    - limit=5 was default
    - branch=feature/issue-123 was auto-detected
```

---

## 2. VALIDATION STRATEGIES

### 2.1 Type Validation

#### String Validation

**Enumeration Validation (Case Statement):**

```bash
# Validate argument against allowed values
type="$1"

case "$type" in
  feature|bug|epic|debt|docs)
    # Valid type, proceed
    ;;
  *)
    echo "⚠️ Invalid Type: '$type' is not a valid issue type"
    echo ""
    echo "Valid types:"
    echo "  • feature - New feature request"
    echo "  • bug     - Bug report"
    echo "  • epic    - Epic milestone planning"
    echo "  • debt    - Technical debt tracking"
    echo "  • docs    - Documentation request"
    echo ""
    echo "Did you mean one of these?"
    # Fuzzy matching (see Error Message Excellence section)
    echo ""
    echo "Try: /create-issue feature \"Add recipe tagging\""
    exit 1
    ;;
esac
```

**Why Case Statement:**
- ✅ **Readable:** Clear list of valid values
- ✅ **Fast:** O(1) lookup for small enums
- ✅ **Maintainable:** Easy to add new values

---

**Length Validation:**

```bash
# Validate string length constraints
title="$2"

if [ ${#title} -lt 10 ]; then
  echo "⚠️ Title Too Short: Title must be at least 10 characters (got ${#title})"
  echo ""
  echo "Why 10 characters?"
  echo "  • Ensures descriptive, actionable titles"
  echo "  • Prevents vague titles like 'fix bug'"
  echo ""
  echo "Try: /create-issue feature \"Add recipe tagging system\""
  exit 1
fi

if [ ${#title} -gt 200 ]; then
  echo "⚠️ Title Too Long: Title must be at most 200 characters (got ${#title})"
  echo ""
  echo "Why 200 characters?"
  echo "  • GitHub issue title limit"
  echo "  • Encourages concise, focused titles"
  echo ""
  echo "Suggestion: Move details to issue description"
  exit 1
fi
```

---

**Pattern Matching (Regex):**

```bash
# Validate string format using regex
branch="$1"

if [[ ! "$branch" =~ ^[a-zA-Z0-9/_-]+$ ]]; then
  echo "⚠️ Invalid Branch Name: '$branch' contains invalid characters"
  echo ""
  echo "Branch names can only contain:"
  echo "  • Letters (a-z, A-Z)"
  echo "  • Numbers (0-9)"
  echo "  • Slashes (/)"
  echo "  • Hyphens (-)"
  echo "  • Underscores (_)"
  echo ""
  echo "Try: /workflow-status --branch feature/issue-123"
  exit 1
fi
```

**Common Regex Patterns:**

```yaml
BRANCH_NAME: ^[a-zA-Z0-9/_-]+$
EMAIL: ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
URL: ^https?://[a-zA-Z0-9.-]+.*$
SEMVER: ^[0-9]+\.[0-9]+\.[0-9]+$
ISSUE_NUMBER: ^[0-9]+$
LABEL_FORMAT: ^[a-z]+(-[a-z]+)*$ (lowercase-hyphenated)
```

---

#### Number Validation

**Integer Validation:**

```bash
# Validate argument is integer (not float, not string)
limit="$1"

if ! [[ "$limit" =~ ^[0-9]+$ ]]; then
  echo "⚠️ Invalid Type: --limit must be a number (got '$limit')"
  echo ""
  echo "Valid range: 1-50"
  echo "Example: /workflow-status --limit 10"
  exit 1
fi
```

**Why Regex `^[0-9]+$` over `[[ "$limit" -eq "$limit" ]]`:**
- ✅ **Explicit:** Clear integer validation
- ✅ **No Side Effects:** Doesn't perform arithmetic
- ✅ **Readable:** Obvious regex pattern

---

**Range Validation:**

```bash
# Validate number within acceptable bounds
limit="$1"

if [ "$limit" -lt 1 ] || [ "$limit" -gt 50 ]; then
  echo "⚠️ Out of Range: --limit must be between 1-50 (got $limit)"
  echo ""
  echo "Why this range?"
  echo "  • Minimum 1: Need at least one result"
  echo "  • Maximum 50: Prevents overwhelming output and API limits"
  echo ""
  echo "Example: /workflow-status --limit 25"
  exit 1
fi
```

**Educational Errors:**
- ✅ **Explain Constraint:** Why 1-50 range?
- ✅ **Show Example:** Midpoint value (25) as suggestion
- ✅ **Rationale:** API limits, UX considerations

---

### 2.2 Constraint Validation

#### Mutual Exclusivity

**Pattern: Conflicting Flags Detection**

```bash
# Detect mutually exclusive arguments
summary=false
details=false

# ... argument parsing sets summary=true or details=true ...

# Validate mutual exclusivity
if [ "$summary" = "true" ] && [ "$details" = "true" ]; then
  echo "⚠️ Conflicting Arguments: --summary and --details are mutually exclusive"
  echo ""
  echo "You must choose one:"
  echo "  • --summary for condensed output (test counts only)"
  echo "  • --details for verbose output (all test names and failures)"
  echo ""
  echo "Examples:"
  echo "  /test-report --summary"
  echo "  /test-report --details"
  exit 1
fi
```

**Why This Works:**
- ✅ **Clear Error:** User knows exactly what's wrong
- ✅ **Actionable:** Shows valid alternatives
- ✅ **Educational:** Explains difference between options

---

#### Argument Dependencies

**Pattern: Dependent Arguments Validation**

```bash
# Validate argument dependencies
format=""
details=false

# ... argument parsing ...

# Validate dependency: --format requires --details
if [ -n "$format" ] && [ "$details" != "true" ]; then
  echo "⚠️ Missing Dependency: --format requires --details"
  echo ""
  echo "Rationale:"
  echo "  • JSON/XML formats only available in detailed mode"
  echo "  • Summary mode uses fixed text format"
  echo ""
  echo "Try: /test-report --details --format json"
  exit 1
fi
```

---

#### Conditional Requirements

**Pattern: If A Then B Must Be Present**

```bash
# Validate conditional requirements
assignee=""
milestone=""

# ... argument parsing ...

# Validate: If assigning to epic, milestone required
if [ -n "$assignee" ] && [ -z "$milestone" ]; then
  echo "⚠️ Missing Argument: --assignee requires --milestone"
  echo ""
  echo "Rationale:"
  echo "  • Assigned issues must belong to milestone for tracking"
  echo "  • Prevents orphaned assigned issues"
  echo ""
  echo "Available milestones:"
  gh milestone list --json number,title --jq '.[] | "  • #\(.number): \(.title)"'
  echo ""
  echo "Try: /create-issue feature \"Title\" --assignee @user --milestone \"Epic #291\""
  exit 1
fi
```

---

### 2.3 External Validation

#### File/Directory Existence

**Pattern: Validate File Exists Before Use**

```bash
# Validate template file exists
template="${1:-issue-feature.md}"
template_path="Docs/Templates/$template"

if [ ! -f "$template_path" ]; then
  echo "⚠️ Template Not Found: $template does not exist"
  echo ""
  echo "Expected location: $template_path"
  echo ""
  echo "Available templates:"
  ls Docs/Templates/issue-*.md 2>/dev/null | sed 's|.*/||' | sed 's/^/  • /'
  echo ""
  echo "Try: /create-issue feature \"Title\" --template issue-feature.md"
  exit 1
fi
```

**Why This Pattern:**
- ✅ **Early Failure:** Catch missing file before workflow starts
- ✅ **Helpful Context:** Shows expected location
- ✅ **Actionable:** Lists available alternatives

---

#### Git Branch Validation

**Pattern: Validate Branch Exists**

```bash
# Validate branch exists locally or remotely
branch="$1"

# Check local branches
if git rev-parse --verify "$branch" >/dev/null 2>&1; then
  # Branch exists locally
  :
# Check remote branches
elif git ls-remote --heads origin "$branch" | grep -q "$branch"; then
  # Branch exists remotely
  echo "ℹ️ Branch '$branch' exists remotely but not locally"
  echo ""
  echo "Fetch remote branch:"
  echo "  git fetch origin $branch:$branch"
  echo ""
  read -p "Fetch now? (y/N): " confirm
  if [ "$confirm" = "y" ]; then
    git fetch origin "$branch:$branch"
  fi
else
  echo "⚠️ Branch Not Found: '$branch' does not exist locally or remotely"
  echo ""
  echo "Available branches:"
  git branch -a | grep -v "HEAD" | sed 's|remotes/origin/||' | sort -u | sed 's/^/  • /'
  echo ""
  echo "Try: /workflow-status --branch main"
  exit 1
fi
```

---

#### GitHub Resource Validation

**Pattern: Validate Milestone Exists**

```bash
# Validate milestone exists in repository
milestone="$1"

if ! gh milestone list --json number,title --jq '.[] | select(.title == "'"$milestone"'")' | grep -q .; then
  echo "⚠️ Milestone Not Found: '$milestone' does not exist"
  echo ""
  echo "Available milestones:"
  gh milestone list --json number,title --jq '.[] | "  • #\(.number): \(.title)"'
  echo ""
  echo "Create milestone:"
  echo "  gh milestone create \"$milestone\""
  echo ""
  echo "Or use existing milestone from list above"
  exit 1
fi
```

---

#### Tool Availability Validation

**Pattern: Check Required CLI Tools**

```bash
# Validate required tools are installed
required_tools=("gh" "git" "jq")

for tool in "${required_tools[@]}"; do
  if ! command -v "$tool" &> /dev/null; then
    echo "⚠️ Dependency Missing: $tool not found"
    echo ""
    echo "This command requires $tool to be installed."
    echo ""

    case "$tool" in
      gh)
        echo "Installation:"
        echo "  • macOS:   brew install gh"
        echo "  • Ubuntu:  sudo apt install gh"
        echo "  • Windows: winget install GitHub.cli"
        echo ""
        echo "After installation:"
        echo "  1. Authenticate: gh auth login"
        echo "  2. Verify: gh --version"
        ;;
      git)
        echo "Installation:"
        echo "  • macOS:   brew install git"
        echo "  • Ubuntu:  sudo apt install git"
        echo "  • Windows: Download from https://git-scm.com/"
        ;;
      jq)
        echo "Installation:"
        echo "  • macOS:   brew install jq"
        echo "  • Ubuntu:  sudo apt install jq"
        echo "  • Windows: Download from https://stedolan.github.io/jq/"
        ;;
    esac

    echo ""
    echo "Learn more: [tool documentation URL]"
    exit 1
  fi
done
```

---

### 2.4 Validation Timing

#### Early Validation (Fail Fast)

**Principle:** Validate as early as possible to prevent wasted work

```yaml
VALIDATION_ORDER:
  1_Argument_Presence:
    - Check required arguments exist
    - Fail immediately if missing

  2_Argument_Types:
    - Validate string, number, enum types
    - Fail before any external calls

  3_Argument_Constraints:
    - Range validation (1-50)
    - Format validation (regex)
    - Mutual exclusivity

  4_External_Dependencies:
    - Tool availability (gh CLI installed?)
    - Authentication (gh auth status)

  5_Business_Logic_Validation:
    - File existence (template found?)
    - Resource existence (milestone exists?)
    - Delegate to skill for complex validation

RATIONALE:
  - Cheap checks first (type, range) - milliseconds
  - Expensive checks later (API calls) - seconds
  - Fail fast prevents wasted workflow execution
```

**Example:**

```bash
# BAD: Late validation (after slow operations)
git_context=$(git log -5 --oneline) # 1 second
related_issues=$(gh issue list) # 2 seconds
template=$(cat template.md) # instant

# NOW validate limit argument
if [ "$limit" -lt 1 ]; then
  echo "Invalid limit" # Wasted 3 seconds on slow ops
  exit 1
fi

# GOOD: Early validation (before slow operations)
if [ "$limit" -lt 1 ]; then
  echo "Invalid limit" # Fail in milliseconds
  exit 1
fi

git_context=$(git log -5 --oneline)
related_issues=$(gh issue list)
template=$(cat template.md)
```

---

#### Late Validation (External Resources)

**Principle:** Some validation requires external resources, do these last

```yaml
LATE_VALIDATION_EXAMPLES:
  GitHub_API_Validation:
    - Milestone exists? (requires: gh milestone list)
    - Assignee has access? (requires: gh api /repos/.../collaborators)
    - Label exists? (requires: check GitHubLabelStandards.md)

  Filesystem_Validation:
    - Template file exists? (requires: filesystem access)
    - Project directory structure? (requires: directory traversal)

  Git_Validation:
    - Branch pushed to remote? (requires: git ls-remote)
    - No merge conflicts? (requires: git merge-base check)

TIMING:
  - After all cheap validations pass
  - Right before skill invocation or workflow execution
  - Aggregate late validations to minimize external calls

EXAMPLE:
  # Group external validations together
  echo "🔄 Validating external resources..."

  # Single gh CLI call for multiple checks
  VALIDATION_RESULT=$(gh api graphql -f query='
    query {
      repository(owner: "...", name: "...") {
        milestone(number: '$milestone_number') { id }
        labels(first: 100) { nodes { name } }
      }
    }
  ')

  # Parse results
  milestone_exists=$(echo "$VALIDATION_RESULT" | jq -r '.data.repository.milestone.id')
  label_exists=$(echo "$VALIDATION_RESULT" | jq -r '.data.repository.labels.nodes[].name' | grep -q "$label")
```

---

## 3. ERROR MESSAGE EXCELLENCE

### 3.1 Clear and Specific Error Messages

#### Template: `⚠️ [Category]: [Specific Issue]. [Explanation]. Try: [Example]`

**Components:**

```yaml
EMOJI:
  Purpose: "Visual indicator of message type"
  Options:
    - ⚠️ Warnings and errors (user input issues)
    - ❌ Failures (workflow execution issues)
    - 🔒 Security concerns (permission issues)

CATEGORY:
  Purpose: "Classification for quick understanding"
  Examples:
    - "Invalid Argument" (user input error)
    - "Dependency Missing" (environment issue)
    - "Validation Failed" (business rule violation)
    - "Execution Failed" (runtime error)

SPECIFIC_ISSUE:
  Purpose: "Exact problem with values"
  Good: "Invalid type 'typo' (expected: feature|bug|epic|debt|docs)"
  Bad: "Invalid type" (what was invalid? what's expected?)

EXPLANATION:
  Purpose: "Why this is wrong, what constraint was violated"
  Good: "Minimum 1: Need at least one result. Maximum 50: Prevents overwhelming output."
  Bad: "Range error" (no context)

ACTIONABLE_FIX:
  Purpose: "Specific command to try or steps to resolve"
  Good: "Try: /create-issue feature \"Add recipe tagging\""
  Bad: "Fix your command" (how?)
```

---

#### Example: Invalid Argument Error

**Excellent Error Message:**

```
⚠️ Invalid Argument: Issue type 'typo' is not valid

Valid types:
  • feature - New feature request
  • bug     - Bug report
  • epic    - Epic milestone planning
  • debt    - Technical debt tracking
  • docs    - Documentation request

Did you mean 'debt'? (closest match)

Try: /create-issue debt "Refactor authentication module"
```

**Why Excellent:**
- ✅ **Specific:** Shows exact invalid value ('typo')
- ✅ **Educational:** Lists all valid types with descriptions
- ✅ **Helpful:** Suggests closest match (fuzzy matching)
- ✅ **Actionable:** Provides correct example command

---

**Poor Error Message (Anti-Pattern):**

```
Error: Invalid input
```

**Why Poor:**
- ❌ **Vague:** What was invalid?
- ❌ **No Context:** What are valid values?
- ❌ **Not Actionable:** How to fix?

---

### 3.2 Educational Error Messages

#### Explain WHY Constraints Exist

**Example: Limit Range Error**

```
⚠️ Out of Range: --limit must be between 1-50 (got 100)

Why this range?
  • Minimum 1: Need at least one result for context
  • Maximum 50: Prevents overwhelming terminal output and API rate limits

GitHub API limits:
  • Authenticated: 5000 requests/hour
  • Large limits consume quota faster

Recommended values:
  • Quick check: --limit 5 (default)
  • Trend analysis: --limit 25
  • Comprehensive review: --limit 50

Try: /workflow-status --limit 25
```

**Why Educational:**
- ✅ **Explains Rationale:** API limits, UX considerations
- ✅ **Provides Context:** GitHub API quota information
- ✅ **Recommends Values:** Use case → limit mapping
- ✅ **Actionable:** Example with reasonable value

---

#### Show Common Patterns

**Example: Template Not Found**

```
⚠️ Template Not Found: 'custom-feature.md' does not exist

Expected location: Docs/Templates/custom-feature.md

Available templates:
  • issue-feature.md    - Feature request (default for type: feature)
  • issue-bug.md        - Bug report (default for type: bug)
  • issue-epic.md       - Epic planning (default for type: epic)
  • issue-debt.md       - Technical debt (default for type: debt)
  • issue-docs.md       - Documentation (default for type: docs)

Template selection:
  • Auto: /create-issue feature "Title" (uses issue-feature.md)
  • Custom: /create-issue feature "Title" --template custom-feature.md

Create custom template:
  cp Docs/Templates/issue-feature.md Docs/Templates/custom-feature.md
  # Edit custom-feature.md with your specific fields

Try: /create-issue feature "Add recipe tagging"
```

**Why Helpful:**
- ✅ **Shows Available Options:** Lists all valid templates
- ✅ **Explains Auto-Selection:** Type → template mapping
- ✅ **Teaches Custom Templates:** How to create custom templates
- ✅ **Multiple Examples:** Auto vs. custom usage

---

### 3.3 Contextual Guidance

#### Suggest Likely Alternatives

**Example: Fuzzy Matching for Typos**

```bash
# Implementation: Fuzzy matching for close matches
invalid_type="$1"
valid_types=("feature" "bug" "epic" "debt" "docs")

# Find closest match using Levenshtein distance (simplified)
closest_match=""
min_distance=999

for valid in "${valid_types[@]}"; do
  distance=$(levenshtein "$invalid_type" "$valid")
  if [ "$distance" -lt "$min_distance" ]; then
    min_distance=$distance
    closest_match=$valid
  fi
done

# Suggest if close match found
if [ "$min_distance" -le 2 ]; then
  echo ""
  echo "Did you mean '$closest_match'? (closest match)"
  echo ""
  echo "Try: /create-issue $closest_match \"Add recipe tagging\""
fi
```

**Fuzzy Match Examples:**

```yaml
USER_INPUT: "typo" → SUGGESTION: "type" (nope, we don't have "type")
Actually: "typo" is 3 edits from "epic" (best match among our enum)

USER_INPUT: "bugg" → SUGGESTION: "bug" (1 edit distance)
USER_INPUT: "dbt" → SUGGESTION: "debt" (1 edit distance)
USER_INPUT: "fet" → SUGGESTION: "feature" (4 edits, don't suggest if >2)
```

---

#### Show Common Values/Thresholds

**Example: Workflow Name Error**

```
⚠️ Workflow Not Found: No workflow named 'test.yml'

Common workflow names in this repository:
  • build.yml                           - Build and compile (runs on every PR)
  • testing-coverage.yml                - Test execution and coverage (epic branches)
  • coverage-excellence-merge-orchestrator.yml - PR consolidation (manual trigger)
  • deploy.yml                          - Deployment pipeline (main branch)
  • lint-and-format.yml                 - Code quality checks (every commit)

List all workflows:
  gh workflow list

Recent runs:
  build.yml: ✅ success (3 min ago)
  testing-coverage.yml: ❌ failure (12 min ago)

Try: /workflow-status build.yml
```

**Why Helpful:**
- ✅ **Shows All Options:** Complete workflow list
- ✅ **Provides Context:** When each workflow runs
- ✅ **Suggests Common:** Most frequently checked workflow
- ✅ **Real-Time Status:** Recent run status helps user choose

---

### 3.4 Troubleshooting Guidance

#### Dependency Missing Errors

**Example: gh CLI Not Installed**

```
⚠️ Dependency Missing: gh CLI not found

This command requires GitHub CLI (gh) to query workflow status.

Installation:
  • macOS:   brew install gh
  • Ubuntu:  sudo apt install gh
  • Windows: winget install GitHub.cli

After installation:
  1. Authenticate: gh auth login
  2. Verify installation: gh --version
  3. Retry command: /workflow-status

Troubleshooting:
  • Check PATH: echo $PATH (ensure gh is in PATH)
  • Verify installation: which gh
  • Reinstall if needed: brew reinstall gh

Learn more:
  • Documentation: https://cli.github.com/
  • Installation guide: https://cli.github.com/manual/installation
  • Authentication: https://cli.github.com/manual/gh_auth_login

Need help? Ask in #dev-tools channel
```

**Components:**
- ✅ **Platform-Specific Installation:** macOS, Ubuntu, Windows commands
- ✅ **Complete Workflow:** Install → Authenticate → Verify → Retry
- ✅ **Troubleshooting Steps:** PATH check, reinstall option
- ✅ **Documentation Links:** Official resources
- ✅ **Support Channel:** Where to get help

---

#### Authentication Errors

**Example: gh CLI Not Authenticated**

```
⚠️ Authentication Required: gh CLI not authenticated

Run this command to authenticate:
  gh auth login

Follow the prompts to:
  1. Choose authentication method (browser or token)
  2. Select GitHub.com or GitHub Enterprise
  3. Complete browser OAuth flow

Verify authentication:
  gh auth status

Expected output:
  ✓ Logged in to github.com as <username>

Troubleshooting:
  • Re-authenticate: gh auth refresh
  • Switch accounts: gh auth switch
  • Check credentials: gh auth status

If problems persist:
  • Clear credentials: gh auth logout
  • Reauthenticate: gh auth login

Retry command after authentication:
  /workflow-status
```

---

#### API Rate Limit Errors

**Example: GitHub API Rate Limit Exceeded**

```
⚠️ API Rate Limit Exceeded

Your GitHub API limit: 5000 requests/hour
Current usage: 5000/5000 (100%)
Limit resets: in 23 minutes

What caused this?
  • Multiple workflow status checks (each consumes 1-3 requests)
  • Concurrent commands or automated scripts
  • Other tools using gh CLI (IDE integrations, CI/CD)

Immediate solutions:
  1. Wait 23 minutes for limit reset
  2. Check current limit: gh api /rate_limit
  3. Reduce request frequency

Long-term solutions:
  • Use authenticated requests (5000/hour vs. 60/hour unauthenticated)
  • Implement request caching
  • Batch API calls where possible

Monitor your usage:
  gh api /rate_limit --jq '.resources.core'

Retry command after reset (23 minutes):
  /workflow-status
```

**Why Helpful:**
- ✅ **Shows Quota Status:** Current usage vs. limit
- ✅ **Explains Cause:** Why rate limit was hit
- ✅ **Immediate Fix:** Wait with exact time
- ✅ **Long-Term Fix:** Prevent future issues
- ✅ **Monitoring:** How to check usage

---

## 4. PARSING PATTERNS

### 4.1 Pattern: Mixed Arguments (Positional + Named + Flags)

**Use Case:** Command has required positional, optional named, and boolean flags

**Example: /create-issue <type> <title> [--template T] [--label L] [--dry-run]**

```bash
#!/bin/bash

# ============================================================================
# STEP 1: Initialize Variables
# ============================================================================
type=""
title=""
template=""
labels=()
dry_run=false

# ============================================================================
# STEP 2: Extract Positional Arguments (Two-Pass for Flexibility)
# ============================================================================

# First pass: Identify positional arguments (not starting with --)
positional_args=()
for arg in "$@"; do
  if [[ ! "$arg" =~ ^-- ]]; then
    positional_args+=("$arg")
  fi
done

# Assign positional arguments
type="${positional_args[0]:-}"
title="${positional_args[1]:-}"

# ============================================================================
# STEP 3: Parse Named Arguments and Flags
# ============================================================================
while [[ $# -gt 0 ]]; do
  case "$1" in
    --template)
      if [ -z "$2" ]; then
        echo "⚠️ Error: --template requires a value"
        exit 1
      fi
      template="$2"
      shift 2
      ;;
    --label)
      if [ -z "$2" ]; then
        echo "⚠️ Error: --label requires a value"
        exit 1
      fi
      labels+=("$2")
      shift 2
      ;;
    --dry-run)
      dry_run=true
      shift
      ;;
    --*)
      echo "⚠️ Error: Unknown argument '$1'"
      exit 1
      ;;
    *)
      # Skip positional arguments (already captured)
      shift
      ;;
  esac
done

# ============================================================================
# STEP 4: Validate Required Positional Arguments
# ============================================================================
if [ -z "$type" ]; then
  echo "⚠️ Missing Required Argument: type"
  echo ""
  echo "Usage: /create-issue <type> <title> [OPTIONS]"
  echo ""
  echo "Try: /create-issue feature \"Add recipe tagging\""
  exit 1
fi

if [ -z "$title" ]; then
  echo "⚠️ Missing Required Argument: title"
  echo ""
  echo "Usage: /create-issue <type> <title> [OPTIONS]"
  echo ""
  echo "Try: /create-issue feature \"Add recipe tagging\""
  exit 1
fi

# ============================================================================
# STEP 5: Validate Argument Constraints
# ============================================================================

# Type enumeration
case "$type" in
  feature|bug|epic|debt|docs)
    ;;
  *)
    echo "⚠️ Invalid Type: '$type' is not valid"
    echo "Valid types: feature|bug|epic|debt|docs"
    exit 1
    ;;
esac

# Title length
if [ ${#title} -lt 10 ] || [ ${#title} -gt 200 ]; then
  echo "⚠️ Invalid Title Length: ${#title} characters (must be 10-200)"
  exit 1
fi

# ============================================================================
# SUCCESS: All arguments parsed and validated
# ============================================================================
echo "Parsed Arguments:"
echo "  type: $type"
echo "  title: $title"
echo "  template: ${template:-auto}"
echo "  labels: ${labels[@]:-none}"
echo "  dry_run: $dry_run"
```

**Key Features:**
- ✅ **Two-Pass Parsing:** Extract positional args first, then parse named/flags
- ✅ **Flexible Order:** `/create-issue feature "Title" --label x` OR `/create-issue --label x feature "Title"`
- ✅ **Repeatable Args:** `--label` can be specified multiple times (array)
- ✅ **Value Validation:** Check named args have values (`--template` requires value)

---

### 4.2 Pattern: Optional Positional Arguments

**Use Case:** Command has optional positional argument that might be omitted

**Example: /workflow-status [workflow-name] [--limit N] [--details]**

**Challenge:** Distinguish between positional argument and named argument

```bash
#!/bin/bash

# ============================================================================
# STEP 1: Initialize Variables
# ============================================================================
workflow_name=""
limit=5
details=false

# ============================================================================
# STEP 2: Two-Pass Parsing (Extract Positional First)
# ============================================================================

# First pass: Find first argument NOT starting with --
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
      limit="$2"
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
      elif [[ "$1" =~ ^-- ]]; then
        echo "⚠️ Unknown argument '$1'"
        exit 1
      else
        shift
      fi
      ;;
  esac
done

# ============================================================================
# SUCCESS: Optional positional handled gracefully
# ============================================================================
echo "Parsed Arguments:"
echo "  workflow_name: ${workflow_name:-all workflows}"
echo "  limit: $limit"
echo "  details: $details"
```

**Example Invocations:**

```yaml
/workflow-status:
  workflow_name: "" (empty, show all workflows)
  limit: 5
  details: false

/workflow-status build.yml:
  workflow_name: "build.yml"
  limit: 5
  details: false

/workflow-status --limit 10:
  workflow_name: "" (no positional, first arg is --limit)
  limit: 10
  details: false

/workflow-status build.yml --limit 10 --details:
  workflow_name: "build.yml"
  limit: 10
  details: true

/workflow-status --limit 10 build.yml --details:
  workflow_name: "build.yml" (positional extracted from mixed order)
  limit: 10
  details: true
```

---

### 4.3 Pattern: Repeatable Arguments

**Use Case:** Argument can be specified multiple times (e.g., multiple labels)

**Example: /create-issue feature "Title" --label frontend --label enhancement**

```bash
#!/bin/bash

# ============================================================================
# STEP 1: Initialize Array for Repeatable Argument
# ============================================================================
labels=()

# ============================================================================
# STEP 2: Parse Repeatable Argument (Accumulate in Array)
# ============================================================================
while [[ $# -gt 0 ]]; do
  case "$1" in
    --label)
      if [ -z "$2" ]; then
        echo "⚠️ Error: --label requires a value"
        exit 1
      fi
      labels+=("$2") # Append to array
      shift 2
      ;;
    # ... other arguments ...
  esac
done

# ============================================================================
# STEP 3: Process Repeatable Argument Array
# ============================================================================

# Check if any labels provided
if [ ${#labels[@]} -eq 0 ]; then
  echo "ℹ️ No labels specified, using type label only"
  labels=("type: $type")
else
  # Validate each label
  for label in "${labels[@]}"; do
    # Check label format (lowercase-hyphenated)
    if [[ ! "$label" =~ ^[a-z]+(-[a-z]+)*$ ]]; then
      echo "⚠️ Invalid Label Format: '$label' must be lowercase-hyphenated"
      echo "Examples: frontend, type: feature, high-priority"
      exit 1
    fi

    # Check label exists in standards (delegate to skill for this)
  done
fi

# ============================================================================
# STEP 4: Use Repeatable Argument
# ============================================================================

# Join labels with commas for gh CLI
label_string=$(IFS=,; echo "${labels[*]}")

# Execute gh CLI with labels
gh issue create --title "$title" --label "$label_string"

# Display applied labels
echo "✅ Issue Created with Labels:"
for label in "${labels[@]}"; do
  echo "  • $label"
done
```

**Array Handling Techniques:**

```bash
# Append to array
labels+=("frontend")
labels+=("enhancement")
# Result: labels=("frontend" "enhancement")

# Array length
${#labels[@]} # Returns: 2

# Iterate array
for label in "${labels[@]}"; do
  echo "$label"
done

# Join array with delimiter
label_string=$(IFS=,; echo "${labels[*]}")
# Result: "frontend,enhancement"
```

---

### 4.4 Pattern: Short and Long Options

**Use Case:** Provide both short (`-f`) and long (`--format`) option syntax

**Example: /test-report [-f|--format FORMAT] [-v|--verbose]**

```bash
#!/bin/bash

# ============================================================================
# STEP 1: Initialize Variables
# ============================================================================
format="text"
verbose=false

# ============================================================================
# STEP 2: Parse Short and Long Options
# ============================================================================
while [[ $# -gt 0 ]]; do
  case "$1" in
    -f|--format)
      if [ -z "$2" ]; then
        echo "⚠️ Error: -f|--format requires a value"
        exit 1
      fi
      format="$2"
      shift 2
      ;;
    -v|--verbose)
      verbose=true
      shift
      ;;
    -h|--help)
      echo "Usage: /test-report [OPTIONS]"
      echo ""
      echo "Options:"
      echo "  -f, --format FORMAT   Output format (text|json|xml, default: text)"
      echo "  -v, --verbose         Verbose output (show all test names)"
      echo "  -h, --help            Show this help message"
      exit 0
      ;;
    *)
      echo "⚠️ Unknown argument '$1'"
      echo "Use -h or --help for usage"
      exit 1
      ;;
  esac
done

# ============================================================================
# STEP 3: Validate Format Argument
# ============================================================================
case "$format" in
  text|json|xml)
    ;;
  *)
    echo "⚠️ Invalid Format: '$format' is not valid"
    echo "Valid formats: text|json|xml"
    echo ""
    echo "Try: /test-report --format json"
    exit 1
    ;;
esac

# ============================================================================
# SUCCESS: Short and long options both work
# ============================================================================
echo "Parsed Arguments:"
echo "  format: $format"
echo "  verbose: $verbose"
```

**When to Provide Short Options:**

```yaml
PROVIDE_SHORT_WHEN:
  Frequently_Used:
    - --format → -f (common in reporting commands)
    - --verbose → -v (universal standard)
    - --help → -h (universal standard)
    - --version → -V (universal standard, capital to avoid conflict with -v)

  Industry_Standard:
    - grep -i (case insensitive)
    - ls -l (long format)
    - ps -a (all processes)

SKIP_SHORT_WHEN:
  Rarely_Used:
    - --template (specific to create-issue, not frequent enough)
    - --milestone (project management, infrequent)

  Conflict_Risk:
    - --type and --template both -t? (confusing)
    - --branch and --body both -b? (ambiguous)
```

---

## 5. BEST PRACTICES & ANTI-PATTERNS

### 5.1 Best Practices

#### Validate Early, Fail Fast

**Good Example:**

```bash
#!/bin/bash

# STEP 1: Parse arguments (fast)
type="$1"
limit="$2"

# STEP 2: Validate immediately (fast, milliseconds)
if [ -z "$type" ]; then
  echo "⚠️ Missing required argument: type"
  exit 1
fi

if ! [[ "$limit" =~ ^[0-9]+$ ]]; then
  echo "⚠️ Invalid limit: must be number"
  exit 1
fi

if [ "$limit" -lt 1 ] || [ "$limit" -gt 50 ]; then
  echo "⚠️ Limit out of range: 1-50"
  exit 1
fi

# STEP 3: Now safe to execute expensive operations
git_context=$(git log -5 --oneline) # 1 second
gh_issues=$(gh issue list) # 2 seconds
```

**Why Good:**
- ✅ **Fast Failure:** Invalid arguments caught in milliseconds
- ✅ **No Wasted Work:** Expensive operations only run if arguments valid
- ✅ **Clear Errors:** User sees problem immediately

---

**Bad Example (Anti-Pattern):**

```bash
#!/bin/bash

# BAD: Expensive operations BEFORE validation
git_context=$(git log -5 --oneline) # 1 second
gh_issues=$(gh issue list) # 2 seconds
template=$(cat template.md) # instant

# NOW validate (wasted 3 seconds if validation fails)
if [ -z "$type" ]; then
  echo "⚠️ Missing required argument"
  exit 1
fi
```

---

#### Provide Clear Usage Examples in Errors

**Good Example:**

```
⚠️ Missing Required Argument: type

Usage: /create-issue <type> <title> [OPTIONS]

Required:
  <type>   Issue type (feature|bug|epic|debt|docs)
  <title>  Clear, actionable issue title

Optional:
  --template TEMPLATE  Custom template (default: auto-select by type)
  --label LABEL        Issue label (repeatable)
  --dry-run            Preview issue without creating

Examples:
  # Basic feature request
  /create-issue feature "Add recipe tagging system"

  # Bug report with labels
  /create-issue bug "Fix login redirect" --label frontend --label high-priority

  # Epic planning with custom template
  /create-issue epic "Q1 2025 Goals" --template epic-quarterly.md

Try: /create-issue feature "Add recipe tagging system"
```

**Why Good:**
- ✅ **Complete Syntax:** Shows full usage pattern
- ✅ **Argument Descriptions:** Explains each argument
- ✅ **Multiple Examples:** Basic, intermediate, advanced
- ✅ **Immediate Fix:** Specific command to try

---

#### Test All Validation Edge Cases

**Testing Checklist:**

```yaml
ARGUMENT_PARSING_TESTS:
  Zero_Arguments:
    Input: /command
    Expected: "Missing required argument" error

  Required_Arguments_Only:
    Input: /command <required>
    Expected: Success with defaults for optional args

  Optional_Arguments:
    Input: /command <required> --optional value
    Expected: Success with optional value applied

  Flags:
    Input: /command <required> --flag
    Expected: flag=true

  Repeatable_Arguments:
    Input: /command --label x --label y
    Expected: labels=["x", "y"]

  Flexible_Order:
    Input: /command --optional x <required> --flag
    Expected: All arguments parsed correctly

VALIDATION_TESTS:
  Invalid_Type:
    Input: /command invalid-type
    Expected: "Invalid type" error with valid types listed

  Out_Of_Range:
    Input: /command --limit 100
    Expected: "Out of range 1-50" error

  Invalid_Format:
    Input: /command --branch "invalid name with spaces"
    Expected: "Invalid format" error with valid pattern

  Missing_Dependency:
    Input: /command --format json (without --details)
    Expected: "Missing dependency: --format requires --details"

  Mutual_Exclusivity:
    Input: /command --summary --details
    Expected: "Conflicting arguments" error

EXTERNAL_VALIDATION_TESTS:
  File_Not_Found:
    Setup: Delete template file
    Input: /command --template missing.md
    Expected: "Template not found" with available list

  Tool_Not_Installed:
    Setup: Mock `which gh` to return false
    Input: /command
    Expected: "Dependency missing: gh CLI" with installation guide

  API_Failure:
    Setup: Mock gh CLI to return rate limit error
    Input: /command
    Expected: "API rate limit" error with reset time
```

---

#### Document Argument Constraints

**In Command Frontmatter:**

```yaml
---
description: "Create GitHub issue with automated context collection"
argument-hint: "<type> <title> [--template T] [--label L] [--dry-run]"
category: "workflow"
requires-skills: ["github-issue-creation"]
---
```

**In Arguments Section:**

```markdown
### Required Arguments

#### `<type>` (required positional)
- **Type:** String (enumeration)
- **Position:** 1 (first argument)
- **Description:** Issue type determining template and workflow
- **Valid Values:** feature|bug|epic|debt|docs
- **Validation:** Case-sensitive exact match
- **Examples:**
  - `feature` - New feature request
  - `bug` - Bug report
  - `epic` - Epic milestone planning

#### `<title>` (required positional)
- **Type:** String
- **Position:** 2 (second argument)
- **Description:** Clear, actionable issue title
- **Validation Rules:**
  - Minimum length: 10 characters (ensures descriptive titles)
  - Maximum length: 200 characters (GitHub issue title limit)
  - Non-empty (cannot be whitespace only)
- **Examples:**
  - ✅ `"Add recipe tagging system"` (clear, specific)
  - ✅ `"Fix authentication redirect loop"` (actionable)
  - ❌ `"Bug"` (too short, not descriptive)
  - ❌ `"This is a really long title that goes on and on..."` (too long)
```

---

### 5.2 Anti-Patterns

#### Anti-Pattern 1: Silent Failures or Unclear Errors

**Problem:**

```bash
# BAD: Silent failure, no user feedback
if [ -z "$type" ]; then
  exit 1 # User sees nothing, no error message
fi

# BAD: Unclear error, no context
if [ "$limit" -gt 50 ]; then
  echo "Error" # What error? What's wrong?
  exit 1
fi
```

**Why Bad:**
- ❌ **No Feedback:** User doesn't know what went wrong
- ❌ **No Context:** No explanation of constraint
- ❌ **Not Actionable:** No suggestion for fix

**Fix:**

```bash
# GOOD: Clear, specific, actionable error
if [ -z "$type" ]; then
  echo "⚠️ Missing Required Argument: type"
  echo ""
  echo "Usage: /create-issue <type> <title>"
  echo ""
  echo "Try: /create-issue feature \"Add tagging\""
  exit 1
fi

if [ "$limit" -gt 50 ]; then
  echo "⚠️ Out of Range: --limit must be 1-50 (got $limit)"
  echo ""
  echo "Why 50 maximum? Prevents overwhelming output and API limits"
  echo ""
  echo "Try: /create-issue --limit 25"
  exit 1
fi
```

---

#### Anti-Pattern 2: No Validation (Trusting User Input)

**Problem:**

```bash
# BAD: No validation, trust user input
type="$1"
title="$2"
limit="$3"

# Directly use without validation
gh issue create --title "$title"
```

**Risks:**
- ❌ **Empty Values:** title="" creates blank issue
- ❌ **Invalid Types:** type="invalid" breaks workflow
- ❌ **Injection:** title="; rm -rf /" shell injection risk
- ❌ **API Errors:** Invalid values cause cryptic gh CLI errors

**Fix:**

```bash
# GOOD: Comprehensive validation
type="$1"
title="$2"
limit="$3"

# Validate type
case "$type" in
  feature|bug|epic|debt|docs) ;;
  *) echo "⚠️ Invalid type"; exit 1 ;;
esac

# Validate title
if [ -z "$title" ]; then
  echo "⚠️ Missing title"
  exit 1
fi

if [ ${#title} -lt 10 ] || [ ${#title} -gt 200 ]; then
  echo "⚠️ Title length must be 10-200 characters"
  exit 1
fi

# Validate limit
if ! [[ "$limit" =~ ^[0-9]+$ ]]; then
  echo "⚠️ Limit must be number"
  exit 1
fi

if [ "$limit" -lt 1 ] || [ "$limit" -gt 50 ]; then
  echo "⚠️ Limit must be 1-50"
  exit 1
fi

# NOW safe to use
gh issue create --title "$title"
```

---

#### Anti-Pattern 3: Inconsistent Argument Naming

**Problem:**

```bash
# Command 1: /create-issue
/create-issue feature "Title" --dry-run

# Command 2: /deploy
/deploy staging --preview

# Command 3: /merge-prs
/merge-prs --test-mode
```

**Why Bad:**
- ❌ **User Confusion:** Same concept (preview mode) has 3 different names
- ❌ **Cognitive Load:** User must remember different flag for each command
- ❌ **Inconsistent UX:** No predictable patterns

**Fix:**

```bash
# Consistent naming: --dry-run for preview mode across ALL commands
/create-issue feature "Title" --dry-run
/deploy staging --dry-run
/merge-prs --dry-run
```

**Consistency Benefits:**
- ✅ **Predictable:** User knows --dry-run means preview everywhere
- ✅ **Learnable:** Learn once, apply to all commands
- ✅ **Professional:** Consistent UX signals quality

---

#### Anti-Pattern 4: No Help Text or Usage Examples

**Problem:**

```bash
#!/bin/bash
# Command with no --help implementation

type="$1"
title="$2"

# ... no help text, no usage examples ...
```

**User Experience:**

```
$ /create-issue --help
⚠️ Unknown argument '--help' # BAD: No help available

$ /create-issue
⚠️ Missing required argument # No guidance on what's required
```

**Fix:**

```bash
#!/bin/bash

# Implement --help
if [ "$1" = "--help" ] || [ "$1" = "-h" ]; then
  cat <<EOF
/create-issue - Create GitHub issue with automated context collection

Usage:
  /create-issue <type> <title> [OPTIONS]

Required:
  <type>   Issue type (feature|bug|epic|debt|docs)
  <title>  Clear, actionable issue title

Optional:
  --template TEMPLATE  Custom template (default: auto-select by type)
  --label LABEL        Issue label (repeatable)
  --milestone MS       Milestone name or number
  --assignee USER      GitHub username to assign
  --dry-run            Preview issue without creating

Examples:
  # Basic feature request
  /create-issue feature "Add recipe tagging system"

  # Bug report with labels
  /create-issue bug "Fix login redirect" --label frontend --label high-priority

  # Epic planning
  /create-issue epic "Q1 2025 Goals" --milestone "Q1 2025"

Learn more:
  Documentation: Docs/Commands/create-issue.md
  GitHub Labels: Docs/Standards/GitHubLabelStandards.md
EOF
  exit 0
fi
```

---

#### Anti-Pattern 5: Complex Parsing Logic (Extract to Function)

**Problem:**

```bash
#!/bin/bash

# BAD: Monolithic parsing logic (200+ lines)
while [[ $# -gt 0 ]]; do
  case "$1" in
    --template)
      template="$2"
      # 30 lines of template validation logic inline
      ;;
    --label)
      labels+=("$2")
      # 40 lines of label validation logic inline
      ;;
    --milestone)
      milestone="$2"
      # 50 lines of milestone validation logic inline
      ;;
    # ... 10 more arguments with inline validation ...
  esac
done
```

**Why Bad:**
- ❌ **Unreadable:** 200+ line while loop is overwhelming
- ❌ **Hard to Test:** Can't test validation logic independently
- ❌ **Maintenance:** Changes to validation require editing monolithic loop

**Fix:**

```bash
#!/bin/bash

# GOOD: Extract validation to functions

validate_template() {
  local template="$1"

  if [ -n "$template" ] && [ ! -f "Docs/Templates/$template" ]; then
    echo "⚠️ Template not found: $template"
    echo "Available: $(ls Docs/Templates/issue-*.md | sed 's|.*/||')"
    return 1
  fi
  return 0
}

validate_labels() {
  local label="$1"

  # Label format validation
  if [[ ! "$label" =~ ^[a-z]+(-[a-z]+)*$ ]]; then
    echo "⚠️ Invalid label format: $label"
    return 1
  fi

  # Label existence check (delegate to skill)
  return 0
}

# NOW parsing is clean and readable
while [[ $# -gt 0 ]]; do
  case "$1" in
    --template)
      validate_template "$2" || exit 1
      template="$2"
      shift 2
      ;;
    --label)
      validate_labels "$2" || exit 1
      labels+=("$2")
      shift 2
      ;;
    # ... much more readable ...
  esac
done
```

**Benefits:**
- ✅ **Readable:** Main parsing loop is concise
- ✅ **Testable:** Functions can be unit tested
- ✅ **Reusable:** Validation functions used across commands
- ✅ **Maintainable:** Changes to validation isolated to function

---

## SUMMARY CHECKLIST

When implementing argument handling:

### Argument Type Selection ✅
- [ ] Use positional for 1-2 required arguments with clear order
- [ ] Use named for optional arguments and extensibility
- [ ] Use flags for boolean toggles
- [ ] Limit positional arguments to maximum 3
- [ ] Provide sensible defaults for all optional arguments

### Validation Strategies ✅
- [ ] Validate argument types (string, number, enum)
- [ ] Validate constraints (ranges, formats, patterns)
- [ ] Check mutual exclusivity and dependencies
- [ ] Validate external resources (files, tools, API resources)
- [ ] Fail fast: validate early before expensive operations

### Error Message Quality ✅
- [ ] Use template: `⚠️ [Category]: [Issue]. [Explanation]. Try: [Example]`
- [ ] Provide specific error messages (show invalid value)
- [ ] Include educational context (explain WHY constraint exists)
- [ ] Suggest alternatives (fuzzy matching, available options)
- [ ] Give actionable fixes (specific command to try)

### Parsing Patterns ✅
- [ ] Two-pass parsing for flexible argument order
- [ ] Support repeatable arguments (arrays)
- [ ] Implement --help and -h flags
- [ ] Extract complex validation to functions
- [ ] Test all edge cases (zero args, invalid types, out of range)

### Best Practices ✅
- [ ] Document all argument constraints
- [ ] Provide clear usage examples in errors
- [ ] Use consistent naming across commands
- [ ] Test validation thoroughly
- [ ] Avoid anti-patterns (silent failures, no validation, complex monolithic parsing)

---

**Completion:** All 3 documentation guides created successfully. See `command-design-guide.md`, `skill-integration-guide.md`, and this `argument-handling-guide.md` for comprehensive command creation guidance.
