# Argument Parsing Patterns

Comprehensive patterns for robust argument handling in slash commands, covering all 4 argument types with validation strategies and best practices.

## Overview

This guide provides battle-tested patterns for parsing command-line arguments in slash commands. Each pattern includes parsing logic, validation strategies, error handling, and real-world examples.

### The Four Argument Types

1. **Positional Arguments**: Required arguments with fixed order
2. **Named Arguments**: Optional arguments with explicit naming (order flexible)
3. **Flags (Boolean Toggles)**: Boolean options that enable/disable behavior
4. **Defaults with Override**: Optional parameters with sensible defaults

## Pattern 1: Positional Arguments

### When to Use
- Required arguments that have a natural order
- Core inputs that are always needed
- Arguments where position is intuitive and memorable

**Limit:** Maximum 2-3 positional arguments for usability

### Basic Pattern

**Usage Example:**
```bash
/create-issue <type> <title>
```

**Parsing Logic:**
```bash
#!/bin/bash

# Capture positional arguments
type="$1"
title="$2"

# Validate presence
if [ -z "$type" ] || [ -z "$title" ]; then
  echo "⚠️ Error: Missing required arguments"
  echo ""
  echo "Usage: /create-issue <type> <title>"
  echo ""
  echo "Required:"
  echo "  <type>   Issue type (feature|bug|epic|debt|docs)"
  echo "  <title>  Brief description of the issue"
  echo ""
  echo "Examples:"
  echo "  /create-issue feature \"Add recipe tagging\""
  echo "  /create-issue bug \"Fix login validation\""
  exit 1
fi

# Type validation with enumeration
case "$type" in
  feature|bug|epic|debt|docs)
    # Valid type
    ;;
  *)
    echo "⚠️ Error: Invalid issue type '$type'"
    echo ""
    echo "Valid types:"
    echo "  • feature - New functionality"
    echo "  • bug     - Defect fix"
    echo "  • epic    - Large initiative"
    echo "  • debt    - Technical debt"
    echo "  • docs    - Documentation"
    echo ""
    echo "Example: /create-issue feature \"Add tagging\""
    exit 1
    ;;
esac

# Title validation
if [ ${#title} -lt 5 ]; then
  echo "⚠️ Error: Title too short (minimum 5 characters)"
  echo "Provided: '$title' (${#title} characters)"
  echo ""
  echo "Example: /create-issue feature \"Add recipe tagging\""
  exit 1
fi

if [ ${#title} -gt 100 ]; then
  echo "⚠️ Error: Title too long (maximum 100 characters)"
  echo "Provided: ${#title} characters"
  echo ""
  echo "💡 Tip: Keep titles concise, add details in description"
  exit 1
fi

# Proceed with validated arguments
echo "✅ Creating $type issue: $title"
```

### Advanced Pattern: Mixed Required and Optional Positional

**Usage Example:**
```bash
/deploy <environment> [version]
```

**Parsing Logic:**
```bash
#!/bin/bash

# Required positional
environment="$1"

# Optional positional with default
version="${2:-latest}"

# Validate required argument
if [ -z "$environment" ]; then
  echo "⚠️ Error: Missing required argument <environment>"
  echo ""
  echo "Usage: /deploy <environment> [version]"
  echo ""
  echo "Required:"
  echo "  <environment>  Target environment (dev|staging|prod)"
  echo ""
  echo "Optional:"
  echo "  [version]      Version to deploy (default: latest)"
  echo ""
  echo "Examples:"
  echo "  /deploy staging"
  echo "  /deploy prod v2.1.0"
  exit 1
fi

# Validate environment
case "$environment" in
  dev|staging|prod)
    # Valid environment
    ;;
  *)
    echo "⚠️ Error: Invalid environment '$environment'"
    echo "Valid environments: dev|staging|prod"
    exit 1
    ;;
esac

# Validate version format (semantic versioning or 'latest')
if [ "$version" != "latest" ]; then
  if ! [[ "$version" =~ ^v[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    echo "⚠️ Error: Invalid version format '$version'"
    echo ""
    echo "Expected formats:"
    echo "  • 'latest'  (default)"
    echo "  • 'vX.Y.Z'  (semantic version, e.g., v2.1.0)"
    echo ""
    echo "Examples:"
    echo "  /deploy staging latest"
    echo "  /deploy prod v2.1.0"
    exit 1
  fi
fi

echo "✅ Deploying version $version to $environment"
```

## Pattern 2: Named Arguments

### When to Use
- Optional arguments where order doesn't matter
- Arguments that have clear, descriptive names
- Configuration options that might be selectively used

### Basic Pattern

**Usage Example:**
```bash
/coverage-report --format json --threshold 80
```

**Parsing Logic:**
```bash
#!/bin/bash

# Set defaults
format="summary"
threshold=24

# Parse named arguments
while [[ $# -gt 0 ]]; do
  case "$1" in
    --format)
      if [ -z "$2" ]; then
        echo "⚠️ Error: --format requires a value"
        echo "Example: /coverage-report --format json"
        exit 1
      fi
      format="$2"
      shift 2
      ;;
    --threshold)
      if [ -z "$2" ]; then
        echo "⚠️ Error: --threshold requires a value"
        echo "Example: /coverage-report --threshold 80"
        exit 1
      fi
      threshold="$2"
      shift 2
      ;;
    *)
      echo "⚠️ Error: Unknown argument '$1'"
      echo ""
      echo "Usage: /coverage-report [OPTIONS]"
      echo ""
      echo "Options:"
      echo "  --format FORMAT      Output format (summary|detailed|json)"
      echo "  --threshold NUMBER   Coverage threshold (0-100)"
      echo ""
      echo "Example: /coverage-report --format json --threshold 80"
      exit 1
      ;;
  esac
done

# Validate format
case "$format" in
  summary|detailed|json)
    # Valid format
    ;;
  *)
    echo "⚠️ Error: Invalid format '$format'"
    echo ""
    echo "Valid formats:"
    echo "  • summary  - Brief coverage overview"
    echo "  • detailed - Comprehensive coverage report"
    echo "  • json     - Machine-readable JSON output"
    echo ""
    echo "Example: /coverage-report --format detailed"
    exit 1
    ;;
esac

# Validate threshold (number and range)
if ! [[ "$threshold" =~ ^[0-9]+$ ]]; then
  echo "⚠️ Error: Threshold must be a number (got '$threshold')"
  echo ""
  echo "Example: /coverage-report --threshold 80"
  exit 1
fi

if [ "$threshold" -lt 0 ] || [ "$threshold" -gt 100 ]; then
  echo "⚠️ Error: Threshold must be between 0 and 100 (got $threshold)"
  echo ""
  echo "Example: /coverage-report --threshold 80"
  exit 1
fi

echo "✅ Generating $format coverage report (threshold: $threshold%)"
```

### Advanced Pattern: Short and Long Options

**Usage Example:**
```bash
/test-report -f json -t 80  # Short options
/test-report --format json --threshold 80  # Long options
```

**Parsing Logic:**
```bash
#!/bin/bash

# Defaults
format="summary"
threshold=24

# Parse both short and long options
while [[ $# -gt 0 ]]; do
  case "$1" in
    -f|--format)
      format="$2"
      shift 2
      ;;
    -t|--threshold)
      threshold="$2"
      shift 2
      ;;
    -h|--help)
      echo "Usage: /test-report [OPTIONS]"
      echo ""
      echo "Options:"
      echo "  -f, --format FORMAT      Output format (summary|detailed|json)"
      echo "  -t, --threshold NUMBER   Coverage threshold (0-100)"
      echo "  -h, --help               Show this help message"
      echo ""
      echo "Examples:"
      echo "  /test-report -f json -t 80"
      echo "  /test-report --format detailed --threshold 90"
      exit 0
      ;;
    *)
      echo "⚠️ Error: Unknown argument '$1'"
      echo "Run '/test-report --help' for usage information"
      exit 1
      ;;
  esac
done

# Validation logic here (same as basic pattern)
```

## Pattern 3: Flags (Boolean Toggles)

### When to Use
- Binary options (enable/disable behavior)
- Feature toggles
- Mode switches

### Basic Pattern

**Usage Example:**
```bash
/workflow-status --details --watch
```

**Parsing Logic:**
```bash
#!/bin/bash

# Defaults (flags start as false)
details=false
watch=false
show_logs=false

# Parse flags
while [[ $# -gt 0 ]]; do
  case "$1" in
    --details)
      details=true
      shift
      ;;
    --watch)
      watch=true
      shift
      ;;
    --show-logs)
      show_logs=true
      shift
      ;;
    --no-details)
      details=false
      shift
      ;;
    *)
      echo "⚠️ Error: Unknown argument '$1'"
      echo ""
      echo "Usage: /workflow-status [FLAGS]"
      echo ""
      echo "Flags:"
      echo "  --details     Show detailed workflow information"
      echo "  --watch       Monitor in real-time (continuous updates)"
      echo "  --show-logs   Include workflow execution logs"
      echo ""
      echo "Example: /workflow-status --details --watch"
      exit 1
      ;;
  esac
done

# Use flags to control behavior
if [ "$details" = "true" ]; then
  echo "🔍 Fetching detailed workflow status..."
  # Show detailed information
else
  echo "📊 Fetching workflow summary..."
  # Show summary only
fi

if [ "$watch" = "true" ]; then
  echo "👀 Watching for updates (press Ctrl+C to stop)..."
  # Implement continuous monitoring
fi

if [ "$show_logs" = "true" ]; then
  echo "📜 Including execution logs..."
  # Fetch and display logs
fi
```

### Advanced Pattern: Mutually Exclusive Flags

**Usage Example:**
```bash
/run-tests --fast    # Quick smoke tests
/run-tests --full    # Comprehensive test suite
# ERROR: /run-tests --fast --full  (cannot use both)
```

**Parsing Logic:**
```bash
#!/bin/bash

# Test mode flags
fast=false
full=false
integration_only=false

# Parse flags
while [[ $# -gt 0 ]]; do
  case "$1" in
    --fast)
      fast=true
      shift
      ;;
    --full)
      full=true
      shift
      ;;
    --integration-only)
      integration_only=true
      shift
      ;;
    *)
      shift
      ;;
  esac
done

# Validate mutual exclusivity
exclusive_count=0
[ "$fast" = "true" ] && ((exclusive_count++))
[ "$full" = "true" ] && ((exclusive_count++))
[ "$integration_only" = "true" ] && ((exclusive_count++))

if [ $exclusive_count -gt 1 ]; then
  echo "⚠️ Error: Cannot use multiple test modes together"
  echo ""
  echo "Choose one test mode:"
  echo "  --fast              Quick smoke tests (~2 min)"
  echo "  --full              Comprehensive suite (~15 min)"
  echo "  --integration-only  Integration tests only (~5 min)"
  echo ""
  echo "Examples:"
  echo "  /run-tests --fast"
  echo "  /run-tests --full"
  exit 1
fi

# Determine mode (default to full if none specified)
if [ "$fast" = "true" ]; then
  echo "🚀 Running fast smoke tests..."
  # Run quick tests
elif [ "$integration_only" = "true" ]; then
  echo "🔗 Running integration tests..."
  # Run integration tests
else
  echo "🧪 Running full test suite..."
  # Run comprehensive tests (default)
fi
```

### Advanced Pattern: Flag Groups

**Usage Example:**
```bash
/analyze-code --security --performance --complexity
```

**Parsing Logic:**
```bash
#!/bin/bash

# Analysis type flags
security=false
performance=false
complexity=false
maintainability=false

# Meta flags
all=false
quick=false

# Parse flags
while [[ $# -gt 0 ]]; do
  case "$1" in
    --security)
      security=true
      shift
      ;;
    --performance)
      performance=true
      shift
      ;;
    --complexity)
      complexity=true
      shift
      ;;
    --maintainability)
      maintainability=true
      shift
      ;;
    --all)
      all=true
      shift
      ;;
    --quick)
      quick=true
      shift
      ;;
    *)
      shift
      ;;
  esac
done

# Meta flag expansion
if [ "$all" = "true" ]; then
  security=true
  performance=true
  complexity=true
  maintainability=true
fi

# Check at least one analysis type selected
if [ "$security" = "false" ] && \
   [ "$performance" = "false" ] && \
   [ "$complexity" = "false" ] && \
   [ "$maintainability" = "false" ]; then
  echo "⚠️ Error: No analysis type specified"
  echo ""
  echo "Select at least one analysis type:"
  echo "  --security         Security vulnerability analysis"
  echo "  --performance      Performance bottleneck detection"
  echo "  --complexity       Code complexity metrics"
  echo "  --maintainability  Maintainability assessment"
  echo ""
  echo "Meta flags:"
  echo "  --all    Enable all analysis types"
  echo "  --quick  Use faster (less thorough) algorithms"
  echo ""
  echo "Examples:"
  echo "  /analyze-code --security --performance"
  echo "  /analyze-code --all --quick"
  exit 1
fi

# Execute selected analyses
[ "$security" = "true" ] && echo "🔒 Running security analysis..."
[ "$performance" = "true" ] && echo "⚡ Running performance analysis..."
[ "$complexity" = "true" ] && echo "📊 Running complexity analysis..."
[ "$maintainability" = "true" ] && echo "🔧 Running maintainability analysis..."

if [ "$quick" = "true" ]; then
  echo "⚡ Using quick analysis mode"
fi
```

## Pattern 4: Defaults with Override

### When to Use
- Parameters that have sensible defaults
- Configuration options most users won't change
- Arguments where default behavior is intuitive

### Basic Pattern

**Usage Example:**
```bash
/workflow-status              # Uses defaults
/workflow-status --limit 10   # Override limit
```

**Parsing Logic:**
```bash
#!/bin/bash

# Sensible defaults
limit=5
branch=$(git branch --show-current)
format="summary"

# Parse overrides
while [[ $# -gt 0 ]]; do
  case "$1" in
    --limit)
      limit="$2"
      shift 2
      ;;
    --branch)
      branch="$2"
      shift 2
      ;;
    --format)
      format="$2"
      shift 2
      ;;
    *)
      shift
      ;;
  esac
done

# Validate limit (range)
if ! [[ "$limit" =~ ^[0-9]+$ ]]; then
  echo "⚠️ Error: Limit must be a number (got '$limit')"
  exit 1
fi

if [ "$limit" -lt 1 ] || [ "$limit" -gt 50 ]; then
  echo "⚠️ Error: Limit must be between 1 and 50 (got $limit)"
  echo ""
  echo "💡 Tip: Use smaller limits for quick checks, larger for comprehensive views"
  exit 1
fi

# Validate branch exists
if ! git rev-parse --verify "$branch" &>/dev/null; then
  echo "⚠️ Error: Branch '$branch' not found"
  echo ""
  echo "Available branches:"
  git branch --list --format="  • %(refname:short)"
  echo ""
  echo "Example: /workflow-status --branch develop"
  exit 1
fi

# Validate format
case "$format" in
  summary|detailed|json)
    # Valid
    ;;
  *)
    echo "⚠️ Error: Invalid format '$format'"
    echo "Valid formats: summary|detailed|json"
    exit 1
    ;;
esac

echo "📊 Showing $limit recent workflows for branch '$branch' (format: $format)"
```

### Advanced Pattern: Smart Defaults Based on Context

**Usage Example:**
```bash
/create-pr                    # Auto-detects base and head branches
/create-pr --base develop     # Override base branch
```

**Parsing Logic:**
```bash
#!/bin/bash

# Context-aware defaults
current_branch=$(git branch --show-current)

# Smart base branch detection
if [[ "$current_branch" =~ ^feature/ ]]; then
  default_base="epic/$(echo $current_branch | cut -d'/' -f2 | cut -d'-' -f1-2)"
elif [[ "$current_branch" =~ ^epic/ ]]; then
  default_base="develop"
else
  default_base="develop"
fi

base="${default_base}"
head="${current_branch}"
draft=false
auto_merge=false

# Parse overrides
while [[ $# -gt 0 ]]; do
  case "$1" in
    --base)
      base="$2"
      shift 2
      ;;
    --head)
      head="$2"
      shift 2
      ;;
    --draft)
      draft=true
      shift
      ;;
    --auto-merge)
      auto_merge=true
      shift
      ;;
    *)
      shift
      ;;
  esac
done

# Validate branches exist
for branch_name in "$base" "$head"; do
  if ! git rev-parse --verify "$branch_name" &>/dev/null; then
    echo "⚠️ Error: Branch '$branch_name' not found"
    exit 1
  fi
done

# Validate different branches
if [ "$base" = "$head" ]; then
  echo "⚠️ Error: Base and head branches cannot be the same"
  echo ""
  echo "Current:"
  echo "  Base: $base"
  echo "  Head: $head"
  echo ""
  echo "Example: /create-pr --base develop"
  exit 1
fi

echo "✅ Creating PR: $head → $base"
[ "$draft" = "true" ] && echo "📝 PR will be created as draft"
[ "$auto_merge" = "true" ] && echo "🔄 Auto-merge enabled"
```

## Pattern 5: Mixed Argument Types

### When to Use
- Complex commands requiring multiple input types
- Commands with required core inputs and optional configuration

### Comprehensive Example

**Usage Example:**
```bash
/create-issue feature "Add tagging" --label enhancement --priority high --dry-run
```

**Complete Parsing Logic:**
```bash
#!/bin/bash

# ============================================================================
# POSITIONAL ARGUMENTS (Required)
# ============================================================================

type="$1"
title="$2"
shift 2

# ============================================================================
# DEFAULTS FOR NAMED ARGUMENTS AND FLAGS
# ============================================================================

# Named arguments
labels=()
priority="medium"
assignee=""
milestone=""

# Flags
dry_run=false
auto_assign=false
open_in_browser=false

# ============================================================================
# PARSE NAMED ARGUMENTS AND FLAGS
# ============================================================================

while [[ $# -gt 0 ]]; do
  case "$1" in
    --label)
      if [ -z "$2" ]; then
        echo "⚠️ Error: --label requires a value"
        exit 1
      fi
      labels+=("$2")
      shift 2
      ;;
    --priority)
      if [ -z "$2" ]; then
        echo "⚠️ Error: --priority requires a value"
        exit 1
      fi
      priority="$2"
      shift 2
      ;;
    --assignee)
      assignee="$2"
      shift 2
      ;;
    --milestone)
      milestone="$2"
      shift 2
      ;;
    --dry-run)
      dry_run=true
      shift
      ;;
    --auto-assign)
      auto_assign=true
      shift
      ;;
    --open)
      open_in_browser=true
      shift
      ;;
    -h|--help)
      echo "Usage: /create-issue <type> <title> [OPTIONS]"
      echo ""
      echo "Required:"
      echo "  <type>   Issue type (feature|bug|epic|debt|docs)"
      echo "  <title>  Brief description"
      echo ""
      echo "Options:"
      echo "  --label LABEL        Add label (can specify multiple times)"
      echo "  --priority PRIORITY  Priority level (low|medium|high|critical)"
      echo "  --assignee USER      Assign to user"
      echo "  --milestone NAME     Add to milestone"
      echo ""
      echo "Flags:"
      echo "  --dry-run       Show what would be created without creating"
      echo "  --auto-assign   Auto-assign to current user"
      echo "  --open          Open issue in browser after creation"
      echo ""
      echo "Examples:"
      echo "  /create-issue feature \"Add tagging\""
      echo "  /create-issue bug \"Fix login\" --priority high --label security"
      echo "  /create-issue epic \"Q4 Platform\" --label epic --milestone \"2024-Q4\""
      exit 0
      ;;
    *)
      echo "⚠️ Error: Unknown argument '$1'"
      echo "Run '/create-issue --help' for usage information"
      exit 1
      ;;
  esac
done

# ============================================================================
# VALIDATE REQUIRED POSITIONAL ARGUMENTS
# ============================================================================

if [ -z "$type" ] || [ -z "$title" ]; then
  echo "⚠️ Error: Missing required arguments"
  echo ""
  echo "Usage: /create-issue <type> <title> [OPTIONS]"
  echo ""
  echo "Examples:"
  echo "  /create-issue feature \"Add recipe tagging\""
  echo "  /create-issue bug \"Fix login validation\""
  echo ""
  echo "Run '/create-issue --help' for more options"
  exit 1
fi

# ============================================================================
# VALIDATE TYPE (Enumeration)
# ============================================================================

case "$type" in
  feature|bug|epic|debt|docs)
    # Valid type
    ;;
  *)
    echo "⚠️ Error: Invalid issue type '$type'"
    echo ""
    echo "Valid types: feature|bug|epic|debt|docs"
    echo ""
    echo "Example: /create-issue feature \"Add tagging\""
    exit 1
    ;;
esac

# ============================================================================
# VALIDATE TITLE (Length constraints)
# ============================================================================

if [ ${#title} -lt 5 ]; then
  echo "⚠️ Error: Title too short (minimum 5 characters)"
  echo "Provided: '$title' (${#title} characters)"
  exit 1
fi

if [ ${#title} -gt 100 ]; then
  echo "⚠️ Error: Title too long (maximum 100 characters)"
  echo "Provided: ${#title} characters"
  echo ""
  echo "💡 Tip: Keep titles concise, add details in description"
  exit 1
fi

# ============================================================================
# VALIDATE PRIORITY (Enumeration)
# ============================================================================

case "$priority" in
  low|medium|high|critical)
    # Valid priority
    ;;
  *)
    echo "⚠️ Error: Invalid priority '$priority'"
    echo ""
    echo "Valid priorities:"
    echo "  • low      - Nice to have"
    echo "  • medium   - Normal priority (default)"
    echo "  • high     - Important, needs attention"
    echo "  • critical - Urgent, blocking work"
    echo ""
    echo "Example: /create-issue bug \"Fix login\" --priority high"
    exit 1
    ;;
esac

# ============================================================================
# VALIDATE LABELS (If provided)
# ============================================================================

valid_labels=("enhancement" "bug" "documentation" "security" "performance" "epic")

for label in "${labels[@]}"; do
  if [[ ! " ${valid_labels[@]} " =~ " ${label} " ]]; then
    echo "⚠️ Warning: Unrecognized label '$label'"
    echo ""
    echo "Common labels: ${valid_labels[@]}"
    echo ""
    echo "💡 Label will still be added, but verify it matches repository labels"
  fi
done

# ============================================================================
# VALIDATE ASSIGNEE (If provided)
# ============================================================================

if [ -n "$assignee" ]; then
  # Check if user exists in GitHub org (requires gh CLI)
  if ! gh api "users/$assignee" &>/dev/null; then
    echo "⚠️ Error: GitHub user '$assignee' not found"
    echo ""
    echo "💡 Verify username spelling and that user has GitHub account"
    exit 1
  fi
fi

# ============================================================================
# HANDLE AUTO-ASSIGN FLAG
# ============================================================================

if [ "$auto_assign" = "true" ]; then
  if [ -n "$assignee" ]; then
    echo "⚠️ Error: Cannot use --auto-assign with --assignee"
    echo "Choose one assignment method"
    exit 1
  fi

  # Get current GitHub user
  assignee=$(gh api user --jq '.login')
  echo "📋 Auto-assigning to current user: $assignee"
fi

# ============================================================================
# DISPLAY SUMMARY (Dry Run or Execution)
# ============================================================================

if [ "$dry_run" = "true" ]; then
  echo "🔍 DRY RUN - No issue will be created"
  echo ""
fi

echo "Issue Summary:"
echo "  Type: $type"
echo "  Title: $title"
echo "  Priority: $priority"

if [ ${#labels[@]} -gt 0 ]; then
  echo "  Labels: ${labels[*]}"
fi

if [ -n "$assignee" ]; then
  echo "  Assignee: $assignee"
fi

if [ -n "$milestone" ]; then
  echo "  Milestone: $milestone"
fi

if [ "$dry_run" = "true" ]; then
  echo ""
  echo "✅ Dry run complete - no changes made"
  echo ""
  echo "💡 Remove --dry-run to create issue"
  exit 0
fi

# ============================================================================
# EXECUTE (Create Issue)
# ============================================================================

echo ""
echo "🔄 Creating issue..."

# Build gh issue create command
gh_cmd="gh issue create --title \"$title\" --label \"type:$type\""

for label in "${labels[@]}"; do
  gh_cmd="$gh_cmd --label \"$label\""
done

if [ -n "$assignee" ]; then
  gh_cmd="$gh_cmd --assignee \"$assignee\""
fi

if [ -n "$milestone" ]; then
  gh_cmd="$gh_cmd --milestone \"$milestone\""
fi

# Execute command
issue_url=$(eval "$gh_cmd")

echo "✅ Issue created: $issue_url"

if [ "$open_in_browser" = "true" ]; then
  echo "🌐 Opening in browser..."
  gh issue view --web "${issue_url##*/}"
fi
```

## Validation Strategies

### Type Validation

#### String Enumeration
```bash
# Validate against fixed set of options
case "$value" in
  option1|option2|option3)
    # Valid
    ;;
  *)
    echo "⚠️ Error: Invalid value '$value'"
    echo "Valid options: option1|option2|option3"
    exit 1
    ;;
esac
```

#### Number Validation
```bash
# Validate numeric input
if ! [[ "$value" =~ ^[0-9]+$ ]]; then
  echo "⚠️ Error: Must be a number (got '$value')"
  exit 1
fi

# Integer range validation
if [ "$value" -lt 1 ] || [ "$value" -gt 100 ]; then
  echo "⚠️ Error: Must be between 1 and 100 (got $value)"
  exit 1
fi
```

#### Pattern Validation (Regex)
```bash
# Semantic version format
if ! [[ "$version" =~ ^v[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "⚠️ Error: Invalid version format '$version'"
  echo "Expected format: vX.Y.Z (e.g., v2.1.0)"
  exit 1
fi

# Email format
if ! [[ "$email" =~ ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$ ]]; then
  echo "⚠️ Error: Invalid email format '$email'"
  exit 1
fi

# GitHub issue/PR number
if ! [[ "$issue_num" =~ ^#?[0-9]+$ ]]; then
  echo "⚠️ Error: Invalid issue number '$issue_num'"
  echo "Expected format: 123 or #123"
  exit 1
fi
```

### Constraint Validation

#### Mutual Exclusivity
```bash
# Validate mutually exclusive flags
if [ "$flag1" = "true" ] && [ "$flag2" = "true" ]; then
  echo "⚠️ Error: Cannot use --flag1 and --flag2 together"
  echo ""
  echo "Choose one option based on your needs:"
  echo "  --flag1  {{Description of flag1 behavior}}"
  echo "  --flag2  {{Description of flag2 behavior}}"
  exit 1
fi
```

#### Dependency Validation
```bash
# Validate required dependencies
if [ -n "$dependent_arg" ] && [ -z "$required_arg" ]; then
  echo "⚠️ Error: --dependent-arg requires --required-arg to be specified"
  echo ""
  echo "Example: /command --required-arg value --dependent-arg value"
  exit 1
fi
```

#### Conditional Requirements
```bash
# Argument required only in certain modes
if [ "$mode" = "advanced" ] && [ -z "$required_in_advanced" ]; then
  echo "⚠️ Error: --required-in-advanced is required when using --mode advanced"
  echo ""
  echo "Example: /command --mode advanced --required-in-advanced value"
  exit 1
fi
```

### External Validation

#### File/Directory Existence
```bash
# Validate file exists
if [ ! -f "$file_path" ]; then
  echo "⚠️ Error: File not found: $file_path"
  echo ""
  echo "💡 Verify file path is correct and file exists"
  exit 1
fi

# Validate directory exists
if [ ! -d "$dir_path" ]; then
  echo "⚠️ Error: Directory not found: $dir_path"
  exit 1
fi
```

#### Git Branch Validation
```bash
# Validate branch exists
if ! git rev-parse --verify "$branch" &>/dev/null; then
  echo "⚠️ Error: Branch '$branch' not found"
  echo ""
  echo "Available branches:"
  git branch --list --format="  • %(refname:short)"
  exit 1
fi
```

#### GitHub Resource Validation
```bash
# Validate GitHub user exists
if ! gh api "users/$username" &>/dev/null; then
  echo "⚠️ Error: GitHub user '$username' not found"
  exit 1
fi

# Validate issue/PR exists
if ! gh issue view "$issue_num" &>/dev/null; then
  echo "⚠️ Error: Issue #$issue_num not found"
  exit 1
fi
```

## Error Message Best Practices

### Clear and Specific

**❌ Bad:**
```bash
echo "Error: invalid input"
```

**✅ Good:**
```bash
echo "⚠️ Error: Invalid issue type '$type'"
echo ""
echo "Valid types: feature|bug|epic|debt|docs"
echo ""
echo "Example: /create-issue feature \"Add tagging\""
```

### Actionable Guidance

**❌ Bad:**
```bash
echo "Error: command failed"
```

**✅ Good:**
```bash
echo "⚠️ Error: GitHub CLI (gh) not found"
echo ""
echo "💡 Install gh CLI:"
echo "  https://cli.github.com/manual/installation"
echo ""
echo "Then authenticate:"
echo "  gh auth login"
echo ""
echo "Retry command:"
echo "  /create-issue feature \"Add tagging\""
```

### Helpful Context

**❌ Bad:**
```bash
echo "Error: value out of range"
```

**✅ Good:**
```bash
echo "⚠️ Error: Threshold must be between 0 and 100 (got $threshold)"
echo ""
echo "💡 Common thresholds:"
echo "  • 80  - Industry standard"
echo "  • 90  - Rigorous quality gate"
echo "  • 70  - Acceptable for legacy code"
echo ""
echo "Example: /coverage-report --threshold 80"
```

### Progress Feedback

**For long-running commands:**
```bash
echo "🔄 Fetching workflow runs..."
gh run list --limit 10

echo "🔄 Analyzing test coverage..."
./Scripts/run-test-suite.sh

echo "✅ Analysis complete"
```

**For multi-step operations:**
```bash
echo "🔄 Creating issue..."
echo ""
echo "Step 1/3: Validating inputs..."
echo "Step 2/3: Creating GitHub issue..."
echo "Step 3/3: Applying labels and assignments..."
echo ""
echo "✅ Issue created: #123"
```

## Complete Example: Multi-Pattern Command

Here's a comprehensive example combining all patterns:

```bash
#!/bin/bash

# /analyze <type> <target> --depth <level> --format <fmt> --cache --verbose

# ============================================================================
# POSITIONAL ARGUMENTS
# ============================================================================

type="$1"
target="$2"
shift 2

# ============================================================================
# DEFAULTS
# ============================================================================

depth="medium"
format="summary"
cache=false
verbose=false

# ============================================================================
# ARGUMENT PARSING
# ============================================================================

while [[ $# -gt 0 ]]; do
  case "$1" in
    -d|--depth)
      depth="$2"
      shift 2
      ;;
    -f|--format)
      format="$2"
      shift 2
      ;;
    --cache)
      cache=true
      shift
      ;;
    -v|--verbose)
      verbose=true
      shift
      ;;
    -h|--help)
      cat <<EOF
Usage: /analyze <type> <target> [OPTIONS]

Required:
  <type>    Analysis type (security|performance|quality)
  <target>  Target to analyze (file path or component name)

Options:
  -d, --depth LEVEL    Analysis depth (quick|medium|deep)
  -f, --format FORMAT  Output format (summary|detailed|json)
  --cache              Use cached results if available
  -v, --verbose        Show detailed progress information

Examples:
  /analyze security src/Auth/
  /analyze performance UserService --depth deep
  /analyze quality . --format json --cache
EOF
      exit 0
      ;;
    *)
      echo "⚠️ Error: Unknown argument '$1'"
      echo "Run '/analyze --help' for usage"
      exit 1
      ;;
  esac
done

# ============================================================================
# VALIDATION
# ============================================================================

# Required arguments
if [ -z "$type" ] || [ -z "$target" ]; then
  echo "⚠️ Error: Missing required arguments"
  echo "Usage: /analyze <type> <target> [OPTIONS]"
  echo "Run '/analyze --help' for more information"
  exit 1
fi

# Type validation
case "$type" in
  security|performance|quality) ;;
  *)
    echo "⚠️ Error: Invalid analysis type '$type'"
    echo "Valid types: security|performance|quality"
    exit 1
    ;;
esac

# Target validation
if [ ! -e "$target" ]; then
  echo "⚠️ Error: Target not found: $target"
  exit 1
fi

# Depth validation
case "$depth" in
  quick|medium|deep) ;;
  *)
    echo "⚠️ Error: Invalid depth '$depth'"
    echo "Valid depths: quick|medium|deep"
    exit 1
    ;;
esac

# Format validation
case "$format" in
  summary|detailed|json) ;;
  *)
    echo "⚠️ Error: Invalid format '$format'"
    echo "Valid formats: summary|detailed|json"
    exit 1
    ;;
esac

# ============================================================================
# EXECUTION
# ============================================================================

[ "$verbose" = "true" ] && echo "🔍 Starting $depth $type analysis of $target"
[ "$cache" = "true" ] && echo "💾 Using cached results if available"

# Perform analysis...

echo "✅ Analysis complete"
```

This comprehensive example demonstrates all 4 argument types working together with proper validation and user-friendly error messages.
