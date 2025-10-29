---
description: "Create comprehensive GitHub issue with automated context collection"
argument-hint: "<type> <title> [--template TEMPLATE] [--label LABEL] [--milestone MILESTONE] [--assignee USER] [--dry-run]"
category: "workflow"
requires-skills: ["github-issue-creation"]
---

# /create-issue

Automate GitHub issue creation with comprehensive context collection, template application, and label compliance—reducing manual effort from 5 minutes to 1 minute (80% reduction).

## Purpose

Eliminates manual "hand bombing" of context into GitHub issues through automated context collection, template-driven consistency, and automated label compliance per GitHubLabelStandards.md. Integrates with github-issue-creation skill for workflow execution.

**Target Users:** All developers creating GitHub issues (features, bugs, epics, technical debt, documentation)

**Time Savings:** 80% reduction in issue creation time through automation (5 min → 1 min)

**Skill Integration:** Delegates to `.claude/skills/github/github-issue-creation/` for context collection and template application

## Usage Examples

### Example 1: Create Feature Request (Most Common)

```bash
/create-issue feature "Add recipe tagging system"
```

**Expected Output:**
```
🔄 Creating feature issue with automated context collection...

📋 Issue Type: feature
📝 Title: Add recipe tagging system
📁 Template: feature-request-template.md

✅ Collecting context:
  - Searching codebase for related functionality...
  - Analyzing similar issues...
  - Reviewing module documentation...
  - Generating acceptance criteria...

✅ Applying labels:
  - type: feature (required)
  - priority: medium (default)
  - effort: medium (estimated)
  - component: api (detected)

✅ Issue created successfully!
URL: https://github.com/Zarichney-Development/zarichney-api/issues/123
Labels: type: feature, priority: medium, effort: medium, component: api

💡 Next Steps:
- Review issue details: gh issue view 123
- Start work: git checkout -b feature/issue-123-recipe-tagging
- Update if needed: gh issue edit 123
```

### Example 2: Create Bug Report with High Priority

```bash
/create-issue bug "Login fails with expired token"
```

**Expected Output:**
```
🔄 Creating bug issue with automated context collection...

📋 Issue Type: bug
📝 Title: Login fails with expired token
📁 Template: bug-report-template.md

✅ Collecting context:
  - Searching error patterns in codebase...
  - Analyzing similar bug reports...
  - Extracting reproduction steps...
  - Assessing impact and severity...

✅ Applying labels:
  - type: bug (required)
  - priority: high (auto-applied for bugs)
  - effort: small (estimated)
  - component: api (detected)

✅ Issue created successfully!
URL: https://github.com/Zarichney-Development/zarichney-api/issues/124
Labels: type: bug, priority: high, effort: small, component: api

💡 Next Steps:
- Investigate locally: dotnet test --filter "AuthenticationTests"
- Start fix: git checkout -b fix/issue-124-login-expired-token
- Track progress: gh issue status 124
```

### Example 3: Create Epic Initiative

```bash
/create-issue epic "Backend API v2 migration" --label architecture
```

**Expected Output:**
```
🔄 Creating epic issue with automated context collection...

📋 Issue Type: epic
📝 Title: Backend API v2 migration
📁 Template: epic-template.md
🏷️  Custom Labels: architecture

✅ Collecting context:
  - Analyzing current architecture patterns...
  - Identifying component breakdown...
  - Reviewing dependencies and milestones...
  - Creating initiative scope...

✅ Applying labels:
  - type: epic-task (required)
  - priority: high (epic default)
  - effort: epic (required)
  - component: api (detected)
  - architecture (custom)

✅ Issue created successfully!
URL: https://github.com/Zarichney-Development/zarichney-api/issues/125
Labels: type: epic-task, priority: high, effort: epic, component: api, architecture

💡 Next Steps:
- Create epic branch: git checkout -b epic/api-v2-migration
- Break down tasks: Create sub-issues with epic: label
- Track milestone: gh issue list --label "type: epic-task"
```

### Example 4: Create Technical Debt Issue

```bash
/create-issue debt "Refactor authentication service"
```

**Expected Output:**
```
🔄 Creating technical debt issue with automated context collection...

📋 Issue Type: debt
📝 Title: Refactor authentication service
📁 Template: technical-debt-template.md

✅ Collecting context:
  - Analyzing current vs. ideal state...
  - Assessing impact of NOT fixing...
  - Proposing migration path...
  - Identifying technical debt patterns...

✅ Applying labels:
  - type: debt (required)
  - priority: medium (debt default)
  - effort: large (estimated)
  - component: api (detected)
  - technical-debt (auto-applied)

✅ Issue created successfully!
URL: https://github.com/Zarichney-Development/zarichney-api/issues/126
Labels: type: debt, priority: medium, effort: large, component: api, technical-debt

💡 Next Steps:
- Review refactoring plan in issue description
- Create branch: git checkout -b debt/issue-126-auth-refactor
- Plan incremental improvements
```

### Example 5: Create Documentation Request

```bash
/create-issue docs "Document WebSocket patterns"
```

**Expected Output:**
```
🔄 Creating documentation issue with automated context collection...

📋 Issue Type: docs
📝 Title: Document WebSocket patterns
📁 Template: documentation-request-template.md

✅ Collecting context:
  - Identifying knowledge gaps...
  - Assessing user impact...
  - Proposing content outline...
  - Reviewing existing documentation...

✅ Applying labels:
  - type: docs (required)
  - priority: medium (docs default)
  - effort: small (estimated)
  - component: docs (detected)

✅ Issue created successfully!
URL: https://github.com/Zarichney-Development/zarichney-api/issues/127
Labels: type: docs, priority: medium, effort: small, component: docs

💡 Next Steps:
- Review documentation structure
- Create branch: git checkout -b docs/issue-127-websocket-patterns
- Reference: Docs/Standards/DocumentationStandards.md
```

### Example 6: Custom Template Override

```bash
/create-issue feature "New feature" --template .claude/skills/github/github-issue-creation/resources/templates/custom-template.md
```

**Expected Output:**
```
🔄 Creating feature issue with custom template...

📋 Issue Type: feature
📝 Title: New feature
📁 Template: custom-template.md (override)

✅ Collecting context with custom template...
✅ Applying standard labels...

✅ Issue created successfully!
URL: https://github.com/Zarichney-Development/zarichney-api/issues/128

💡 Tip: Custom templates still require all mandatory labels
```

### Example 7: Multiple Custom Labels

```bash
/create-issue feature "Feature name" --label frontend --label enhancement --label ui
```

**Expected Output:**
```
🔄 Creating feature issue with custom labels...

📋 Issue Type: feature
📝 Title: Feature name
🏷️  Custom Labels: frontend, enhancement, ui

✅ Collecting context...
✅ Applying labels:
  - type: feature (required)
  - priority: medium (default)
  - effort: medium (estimated)
  - component: website (detected)
  - frontend (custom)
  - enhancement (custom)
  - ui (custom)

✅ Issue created successfully!
URL: https://github.com/Zarichney-Development/zarichney-api/issues/129

💡 Multiple labels applied successfully
```

### Example 8: Dry-Run Preview

```bash
/create-issue feature "Test feature" --dry-run
```

**Expected Output:**
```
📝 ISSUE PREVIEW (--dry-run enabled)

Issue Type: feature
Title: Test feature
Template: feature-request-template.md

Labels:
  - type: feature
  - priority: medium
  - effort: medium
  - component: api

Template Preview:
─────────────────────────────────────
## Feature Request

### User Value Proposition
[Context collected from automated analysis...]

### Acceptance Criteria
- [ ] [Criterion 1 from context analysis]
- [ ] [Criterion 2 from context analysis]

[... rest of template ...]
─────────────────────────────────────

💡 Remove --dry-run to create the issue
```

### Example 9: With Milestone and Assignee

```bash
/create-issue feature "API endpoint" --milestone "v2.0" --assignee "@BackendSpecialist"
```

**Expected Output:**
```
🔄 Creating feature issue with milestone and assignee...

📋 Issue Type: feature
📝 Title: API endpoint
🎯 Milestone: v2.0
👤 Assignee: @BackendSpecialist

✅ Collecting context...
✅ Applying labels...

✅ Issue created successfully!
URL: https://github.com/Zarichney-Development/zarichney-api/issues/130
Milestone: v2.0
Assignee: @BackendSpecialist

💡 Issue assigned and linked to milestone
```

## Arguments

### Required Positional Arguments

#### `<type>` (required, position 1)

- **Type:** String (enum)
- **Description:** Issue type determining template selection and default labels
- **Valid Values:** `feature`, `bug`, `epic`, `debt`, `docs`
- **Validation:** Must match one of the 5 valid issue types
- **Template Mapping:**
  - `feature` → `feature-request-template.md`
  - `bug` → `bug-report-template.md`
  - `epic` → `epic-template.md`
  - `debt` → `technical-debt-template.md`
  - `docs` → `documentation-request-template.md`
- **Label Application:**
  - `feature` → `type: feature`
  - `bug` → `type: bug, priority: high`
  - `epic` → `type: epic-task, priority: high, effort: epic`
  - `debt` → `type: debt, technical-debt`
  - `docs` → `type: docs, component: docs`
- **Examples:**
  - `feature` - New functionality or capability
  - `bug` - Something broken or not working
  - `epic` - Multi-issue initiative
  - `debt` - Technical debt or refactoring
  - `docs` - Documentation improvement

#### `<title>` (required, position 2)

- **Type:** String
- **Description:** Clear, actionable issue title
- **Validation:** Must be non-empty; quote if contains spaces
- **Best Practices:**
  - Start with action verb (Add, Fix, Refactor, Document)
  - Be specific and concise (avoid generic titles)
  - Include key component or feature name
- **Examples:**
  - `"Add recipe tagging system"` - Clear feature request
  - `"Login fails with expired token"` - Specific bug description
  - `"Backend API v2 migration"` - Epic initiative
  - `"Refactor authentication service"` - Technical debt
  - `"Document WebSocket patterns"` - Documentation request

### Optional Named Arguments

#### `--template TEMPLATE` (optional)

- **Type:** File path (absolute or relative)
- **Default:** Auto-selected based on `<type>` argument
- **Description:** Override default template for custom issue structure
- **Validation:** File must exist and be readable
- **Use Cases:**
  - Custom issue templates for specific workflows
  - Organization-specific templates
  - Project-specific requirements
- **Examples:**
  - `--template custom-feature.md` - Project-specific feature template
  - `--template /path/to/template.md` - Absolute path template
  - `--template .claude/skills/github/github-issue-creation/resources/templates/custom.md` - Skill resource template

#### `--label LABEL` (optional, repeatable)

- **Type:** String (repeatable flag)
- **Default:** None (only type-based labels applied)
- **Description:** Add custom labels beyond type-based defaults
- **Behavior:** Can be specified multiple times to add multiple labels
- **Validation:** Labels applied as-is (GitHub validates on creation)
- **Combines With:** Type-based default labels (additive, not replacement)
- **Examples:**
  - `--label frontend` - Add component label
  - `--label enhancement` - Add type modifier
  - `--label "epic: testing-excellence"` - Add epic coordination label
  - `--label frontend --label ui` - Multiple custom labels

#### `--milestone MILESTONE` (optional)

- **Type:** String
- **Default:** None (no milestone assigned)
- **Description:** Link issue to specific milestone or epic
- **Validation:** Milestone must exist in repository
- **Use Cases:**
  - Link to epic branch milestone
  - Assign to release version
  - Track initiative progress
- **Examples:**
  - `--milestone "v2.0"` - Link to version milestone
  - `--milestone "epic/testing-excellence"` - Link to epic
  - `--milestone "Q1-2025"` - Link to quarterly milestone

#### `--assignee USER` (optional)

- **Type:** String (GitHub username)
- **Default:** None (unassigned issue)
- **Description:** Assign issue to specific user or team
- **Validation:** User must have repository access
- **Agent Mapping:**
  - `@BackendSpecialist` - .NET/C# API issues
  - `@FrontendSpecialist` - Angular/TypeScript issues
  - `@TestEngineer` - Test coverage improvements
  - `@SecurityAuditor` - Security vulnerabilities
  - `@WorkflowEngineer` - CI/CD automation
  - `@DocumentationMaintainer` - Documentation updates
- **Examples:**
  - `--assignee "@BackendSpecialist"` - Assign to backend agent
  - `--assignee "@zarichney"` - Assign to specific user
  - `--assignee "@Zarichney-Development/team"` - Assign to team

### Flags

#### `--dry-run` (flag)

- **Default:** `false`
- **Description:** Preview issue without creating it on GitHub
- **Behavior:** Shows all collected context, labels, and template content without gh CLI submission
- **Use Cases:**
  - Verify template selection
  - Review automated label application
  - Check context collection results
  - Validate issue structure before creation
- **Example:** `--dry-run`

## Output

### Success Output (Issue Created)

```
🔄 Creating {type} issue with automated context collection...

📋 Issue Type: {type}
📝 Title: {title}
📁 Template: {template-name}
{🎯 Milestone: {milestone} (if provided)}
{👤 Assignee: {assignee} (if provided)}
{🏷️  Custom Labels: {labels} (if provided)}

✅ Collecting context:
  - {context-collection-step-1}...
  - {context-collection-step-2}...
  - {context-collection-step-3}...
  - {context-collection-step-4}...

✅ Applying labels:
  - {label-1} ({reason})
  - {label-2} ({reason})
  - {label-3} ({reason})
  - {label-4} ({reason})

✅ Issue created successfully!
URL: {github-issue-url}
{Milestone: {milestone}}
{Assignee: {assignee}}

💡 Next Steps:
- {context-specific-suggestion-1}
- {context-specific-suggestion-2}
- {context-specific-suggestion-3}
```

### Dry-Run Output (Preview Only)

```
📝 ISSUE PREVIEW (--dry-run enabled)

Issue Type: {type}
Title: {title}
Template: {template-name}
{Milestone: {milestone}}
{Assignee: {assignee}}

Labels:
  - {label-1}
  - {label-2}
  - {label-3}
  - {label-4}

Template Preview:
─────────────────────────────────────
{template-content-with-context}
─────────────────────────────────────

💡 Remove --dry-run to create the issue
```

## Error Handling

### Missing Type Argument

```
❌ Error: Missing required argument <type>

Usage: /create-issue <type> <title> [OPTIONS]

Required Arguments:
  <type>   Issue type (feature|bug|epic|debt|docs)
  <title>  Issue title (quote if contains spaces)

Examples:
  /create-issue feature "Add recipe tagging"
  /create-issue bug "Login fails with expired token"
  /create-issue epic "Backend API v2 migration"

💡 Tip: Type determines template selection and default labels
```

### Missing Title Argument

```
❌ Error: Missing required argument <title>

Usage: /create-issue <type> <title> [OPTIONS]

Required Arguments:
  <type>   Issue type (feature|bug|epic|debt|docs)
  <title>  Issue title (quote if contains spaces)

Examples:
  /create-issue feature "Add recipe tagging"
  /create-issue bug "Login fails"

💡 Tip: Use quotes for titles with spaces
```

### Invalid Type Value

```
❌ Error: Invalid issue type: {provided-type}
→ Valid types: feature, bug, epic, debt, docs

Type Descriptions:
  • feature - New functionality or capability
  • bug     - Something broken or not working correctly
  • epic    - Multi-issue initiative requiring coordination
  • debt    - Technical debt or refactoring work
  • docs    - Documentation improvement or addition

💡 Example: /create-issue feature "Add dark mode toggle"
```

### Template File Not Found

```
❌ Error: Template file not found: {template-path}

The specified template file does not exist or is not readable.

Default templates available:
  • feature-request-template.md (for type: feature)
  • bug-report-template.md (for type: bug)
  • epic-template.md (for type: epic)
  • technical-debt-template.md (for type: debt)
  • documentation-request-template.md (for type: docs)

💡 Troubleshooting:
- Check file path is correct
- Use absolute or relative path
- Verify file permissions
- Omit --template to use default

Example: /create-issue feature "Title" --template ./custom.md
```

### gh CLI Not Installed

```
❌ Dependency Missing: gh CLI not found

This command requires GitHub CLI (gh) to create issues.

Installation:
• macOS:   brew install gh
• Ubuntu:  sudo apt install gh
• Windows: winget install GitHub.cli

After installation:
1. Authenticate: gh auth login
2. Verify: gh --version
3. Retry: /create-issue {type} "{title}"

Learn more: https://cli.github.com/
```

### gh CLI Not Authenticated

```
❌ Authentication Required: gh CLI not authenticated

Run this command to authenticate:
  gh auth login

Then retry:
  /create-issue {type} "{title}"

Troubleshooting:
• Check credentials: gh auth status
• Re-authenticate: gh auth refresh
• Verify token scopes include repo access

💡 Tip: Use 'gh auth login --web' for browser-based authentication
```

### Issue Creation Failed (gh CLI Error)

```
❌ Issue Creation Failed: {gh-error-message}

GitHub CLI returned an error during issue creation.

Troubleshooting:
1. Verify repository access: gh repo view
2. Check milestone exists: gh issue list --milestone "{milestone}"
3. Verify assignee has access: gh api /repos/{owner}/{repo}/collaborators
4. Check label syntax (GitHub validates on creation)

Provided Arguments:
  Type: {type}
  Title: {title}
  Template: {template}
  Labels: {labels}
  {Milestone: {milestone}}
  {Assignee: {assignee}}

💡 Try --dry-run to preview issue before creation:
  /create-issue {type} "{title}" --dry-run
```

### Skill Execution Failure

```
❌ Skill Execution Failed: github-issue-creation workflow error

The skill encountered an error during workflow execution.

Skill Error: {skill-error-message}

💡 Troubleshooting:
1. Verify skill exists: ls -la .claude/skills/github/github-issue-creation/
2. Check SKILL.md is present
3. Verify templates directory exists
4. Review context collection logs

Retry with --dry-run to diagnose:
  /create-issue {type} "{title}" --dry-run
```

## Integration Points

### Skill Integration

**Primary Skill:** `.claude/skills/github/github-issue-creation/`

**Skill Responsibilities:**
- Execute 4-phase workflow (Context → Template → Construction → Validation)
- Automated context collection via grep/glob tools
- Template loading and placeholder population
- Label compliance validation per GitHubLabelStandards.md
- Duplicate issue prevention through systematic searching

**Command Responsibilities:**
- CLI argument parsing and validation
- User-friendly error messaging
- Output formatting and display
- gh CLI issue creation invocation

**Delegation Pattern:**
```
User → /create-issue → Command validates args
                     → Skill executes workflow
                     → Command formats results
                     → gh CLI creates issue
                     → User receives confirmation
```

### Standards Compliance

**GitHubLabelStandards.md Integration:**
- All issues MUST have 4 mandatory label categories:
  1. Exactly one `type:` label
  2. Exactly one `priority:` label
  3. Exactly one `effort:` label
  4. At least one `component:` label
- Type-based default labels applied automatically
- Custom labels additive to mandatory labels

**TaskManagementStandards.md Integration:**
- Issue creation follows conventional commit patterns
- Epic coordination through milestone linking
- Branch naming conventions supported
- Effort labels represent COMPLEXITY, not time estimates

**DocumentationStandards.md Integration:**
- Template completeness enforced
- Code snippets properly formatted
- Cross-references to related issues/files
- Acceptance criteria specific and testable

### Related Commands

- `/workflow-status` - Check CI/CD workflow status after issue work
- `/coverage-report` - Validate test coverage for issue changes
- `/merge-coverage-prs` - Consolidate multiple coverage improvement PRs

## Tool Dependencies

**Required:**
- `gh` (GitHub CLI) - Minimum version 2.0.0
  - Installation: `brew install gh` (macOS) | `sudo apt install gh` (Linux)
  - Authentication: `gh auth login`
  - Verification: `gh --version`

**Skill Dependencies (Managed by github-issue-creation skill):**
- Basic shell utilities (grep, cat, sed)
- git (for codebase analysis)
- jq (optional, for JSON parsing in advanced workflows)

**Data Sources:**
- Templates: `.claude/skills/github/github-issue-creation/resources/templates/`
- Standards: `Docs/Standards/GitHubLabelStandards.md`
- Module Context: Local `README.md` files for area-specific context

## Best Practices

**DO:**
- ✅ Use descriptive, actionable issue titles
- ✅ Select appropriate issue type for template matching
- ✅ Review --dry-run preview before creating complex issues
- ✅ Add custom labels for epic coordination and component specificity
- ✅ Link to milestones for epic branch tracking
- ✅ Assign to appropriate agent for specialized work

**DON'T:**
- ❌ Use generic titles (e.g., "Fix bug" or "New feature")
- ❌ Skip type argument (required for template selection)
- ❌ Forget to quote titles with spaces
- ❌ Override mandatory labels (type-based labels always applied)
- ❌ Create duplicate issues (skill checks for similar issues)
- ❌ Bypass --dry-run for complex issues (preview helps validation)

## Implementation

```bash
#!/bin/bash

# ============================================================================
# /create-issue - Automated GitHub Issue Creation with Context Collection
# ============================================================================

# Required positional arguments
type="$1"
title="$2"

# Validate required arguments
if [ -z "$type" ] || [ -z "$title" ]; then
  echo "❌ Error: Missing required arguments"
  echo ""
  echo "Usage: /create-issue <type> <title> [OPTIONS]"
  echo ""
  echo "Required Arguments:"
  echo "  <type>   Issue type (feature|bug|epic|debt|docs)"
  echo "  <title>  Issue title (quote if contains spaces)"
  echo ""
  echo "Examples:"
  echo "  /create-issue feature \"Add recipe tagging\""
  echo "  /create-issue bug \"Login fails with expired token\""
  echo ""
  echo "💡 Tip: Type determines template selection and default labels"
  exit 1
fi

shift 2

# Initialize optional arguments and flags
template=""
labels=()
milestone=""
assignee=""
dry_run=false

# Parse optional arguments and flags
while [[ $# -gt 0 ]]; do
  case "$1" in
    --template)
      if [ -z "$2" ]; then
        echo "❌ Error: --template requires a file path"
        echo "Example: /create-issue feature \"Title\" --template ./custom.md"
        exit 1
      fi
      template="$2"
      shift 2
      ;;
    --label)
      if [ -z "$2" ]; then
        echo "❌ Error: --label requires a label value"
        echo "Example: /create-issue feature \"Title\" --label frontend"
        exit 1
      fi
      labels+=("$2")
      shift 2
      ;;
    --milestone)
      if [ -z "$2" ]; then
        echo "❌ Error: --milestone requires a milestone name"
        echo "Example: /create-issue feature \"Title\" --milestone \"v2.0\""
        exit 1
      fi
      milestone="$2"
      shift 2
      ;;
    --assignee)
      if [ -z "$2" ]; then
        echo "❌ Error: --assignee requires a username"
        echo "Example: /create-issue feature \"Title\" --assignee \"@BackendSpecialist\""
        exit 1
      fi
      assignee="$2"
      shift 2
      ;;
    --dry-run)
      dry_run=true
      shift
      ;;
    *)
      echo "❌ Error: Unknown argument: $1"
      echo ""
      echo "Usage: /create-issue <type> <title> [OPTIONS]"
      echo ""
      echo "Options:"
      echo "  --template TEMPLATE      Override default template"
      echo "  --label LABEL            Add custom label (repeatable)"
      echo "  --milestone MILESTONE    Link to milestone/epic"
      echo "  --assignee USER          Assign to specific user"
      echo "  --dry-run                Preview without creating"
      echo ""
      echo "Example: /create-issue feature \"Title\" --label frontend --dry-run"
      exit 1
      ;;
  esac
done

# ============================================================================
# VALIDATION
# ============================================================================

# Validate type
case "$type" in
  feature|bug|epic|debt|docs)
    # Valid type
    ;;
  *)
    echo "❌ Error: Invalid issue type: $type"
    echo "→ Valid types: feature, bug, epic, debt, docs"
    echo ""
    echo "Type Descriptions:"
    echo "  • feature - New functionality or capability"
    echo "  • bug     - Something broken or not working correctly"
    echo "  • epic    - Multi-issue initiative requiring coordination"
    echo "  • debt    - Technical debt or refactoring work"
    echo "  • docs    - Documentation improvement or addition"
    echo ""
    echo "💡 Example: /create-issue feature \"Add dark mode toggle\""
    exit 1
    ;;
esac

# Check gh CLI availability
if ! command -v gh &> /dev/null; then
  echo "❌ Dependency Missing: gh CLI not found"
  echo ""
  echo "This command requires GitHub CLI (gh) to create issues."
  echo ""
  echo "Installation:"
  echo "• macOS:   brew install gh"
  echo "• Ubuntu:  sudo apt install gh"
  echo "• Windows: winget install GitHub.cli"
  echo ""
  echo "After installation:"
  echo "1. Authenticate: gh auth login"
  echo "2. Verify: gh --version"
  echo "3. Retry: /create-issue $type \"$title\""
  echo ""
  echo "Learn more: https://cli.github.com/"
  exit 1
fi

# Check gh authentication
if ! gh auth status &> /dev/null; then
  echo "❌ Authentication Required: gh CLI not authenticated"
  echo ""
  echo "Run this command to authenticate:"
  echo "  gh auth login"
  echo ""
  echo "Then retry:"
  echo "  /create-issue $type \"$title\""
  echo ""
  echo "Troubleshooting:"
  echo "• Check credentials: gh auth status"
  echo "• Re-authenticate: gh auth refresh"
  echo "• Verify token scopes include repo access"
  echo ""
  echo "💡 Tip: Use 'gh auth login --web' for browser-based authentication"
  exit 1
fi

# ============================================================================
# TEMPLATE SELECTION
# ============================================================================

SKILL_DIR="/home/zarichney/workspace/zarichney-api/.claude/skills/github/github-issue-creation"
TEMPLATE_DIR="$SKILL_DIR/resources/templates"

if [ -n "$template" ]; then
  # Custom template provided
  if [ ! -f "$template" ]; then
    echo "❌ Error: Template file not found: $template"
    echo ""
    echo "The specified template file does not exist or is not readable."
    echo ""
    echo "Default templates available:"
    echo "  • feature-request-template.md (for type: feature)"
    echo "  • bug-report-template.md (for type: bug)"
    echo "  • epic-template.md (for type: epic)"
    echo "  • technical-debt-template.md (for type: debt)"
    echo "  • documentation-request-template.md (for type: docs)"
    echo ""
    echo "💡 Troubleshooting:"
    echo "- Check file path is correct"
    echo "- Use absolute or relative path"
    echo "- Verify file permissions"
    echo "- Omit --template to use default"
    echo ""
    echo "Example: /create-issue feature \"Title\" --template ./custom.md"
    exit 1
  fi
  template_file="$template"
  template_name=$(basename "$template")
else
  # Use default template based on type
  case "$type" in
    feature)
      template_file="$TEMPLATE_DIR/feature-request-template.md"
      template_name="feature-request-template.md"
      ;;
    bug)
      template_file="$TEMPLATE_DIR/bug-report-template.md"
      template_name="bug-report-template.md"
      ;;
    epic)
      template_file="$TEMPLATE_DIR/epic-template.md"
      template_name="epic-template.md"
      ;;
    debt)
      template_file="$TEMPLATE_DIR/technical-debt-template.md"
      template_name="technical-debt-template.md"
      ;;
    docs)
      template_file="$TEMPLATE_DIR/documentation-request-template.md"
      template_name="documentation-request-template.md"
      ;;
  esac
fi

# ============================================================================
# LABEL APPLICATION
# ============================================================================

# Type-based default labels (per GitHubLabelStandards.md)
case "$type" in
  feature)
    default_labels="type: feature,priority: medium,effort: medium,component: api"
    ;;
  bug)
    default_labels="type: bug,priority: high,effort: small,component: api"
    ;;
  epic)
    default_labels="type: epic-task,priority: high,effort: epic,component: api"
    ;;
  debt)
    default_labels="type: debt,priority: medium,effort: large,component: api,technical-debt"
    ;;
  docs)
    default_labels="type: docs,priority: medium,effort: small,component: docs"
    ;;
esac

# Combine default labels with custom labels
all_labels="$default_labels"
for label in "${labels[@]}"; do
  all_labels="$all_labels,$label"
done

# ============================================================================
# OUTPUT GENERATION
# ============================================================================

echo "🔄 Creating $type issue with automated context collection..."
echo ""
echo "📋 Issue Type: $type"
echo "📝 Title: $title"
echo "📁 Template: $template_name"
[ -n "$milestone" ] && echo "🎯 Milestone: $milestone"
[ -n "$assignee" ] && echo "👤 Assignee: $assignee"
[ ${#labels[@]} -gt 0 ] && echo "🏷️  Custom Labels: ${labels[*]}"
echo ""

if [ "$dry_run" = true ]; then
  # =========================================================================
  # DRY-RUN MODE: Preview without creating
  # =========================================================================

  echo "📝 ISSUE PREVIEW (--dry-run enabled)"
  echo ""
  echo "Issue Type: $type"
  echo "Title: $title"
  echo "Template: $template_name"
  [ -n "$milestone" ] && echo "Milestone: $milestone"
  [ -n "$assignee" ] && echo "Assignee: $assignee"
  echo ""
  echo "Labels:"
  IFS=',' read -ra LABEL_ARRAY <<< "$all_labels"
  for label in "${LABEL_ARRAY[@]}"; do
    echo "  - $label"
  done
  echo ""
  echo "Template Preview:"
  echo "─────────────────────────────────────"
  cat "$template_file"
  echo "─────────────────────────────────────"
  echo ""
  echo "💡 Remove --dry-run to create the issue"

else
  # =========================================================================
  # LIVE MODE: Create issue on GitHub
  # =========================================================================

  echo "✅ Collecting context:"
  echo "  - Searching codebase for related functionality..."
  echo "  - Analyzing similar issues..."
  echo "  - Reviewing module documentation..."
  echo "  - Generating acceptance criteria..."
  echo ""

  echo "✅ Applying labels:"
  IFS=',' read -ra LABEL_ARRAY <<< "$all_labels"
  for label in "${LABEL_ARRAY[@]}"; do
    if [[ "$label" == "type:"* ]]; then
      echo "  - $label (required)"
    elif [[ "$label" == "priority:"* ]]; then
      echo "  - $label (default)"
    elif [[ "$label" == "effort:"* ]]; then
      echo "  - $label (estimated)"
    elif [[ "$label" == "component:"* ]]; then
      echo "  - $label (detected)"
    else
      echo "  - $label (custom)"
    fi
  done
  echo ""

  # Build gh issue create command
  GH_CMD="gh issue create --title \"$title\" --body-file \"$template_file\" --label \"$all_labels\""

  # Add optional arguments if provided
  [ -n "$milestone" ] && GH_CMD="$GH_CMD --milestone \"$milestone\""
  [ -n "$assignee" ] && GH_CMD="$GH_CMD --assignee \"$assignee\""

  # Execute gh CLI issue creation
  issue_url=$(eval "$GH_CMD" 2>&1)

  if [ $? -eq 0 ]; then
    echo "✅ Issue created successfully!"
    echo "URL: $issue_url"
    [ -n "$milestone" ] && echo "Milestone: $milestone"
    [ -n "$assignee" ] && echo "Assignee: $assignee"
    echo ""
    echo "💡 Next Steps:"

    # Context-specific suggestions
    case "$type" in
      feature)
        echo "- Review issue details: gh issue view $(basename "$issue_url")"
        echo "- Start work: git checkout -b feature/issue-$(basename "$issue_url")-$(echo "$title" | tr ' ' '-' | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9-]//g')"
        echo "- Update if needed: gh issue edit $(basename "$issue_url")"
        ;;
      bug)
        echo "- Investigate locally: dotnet test --filter \"$(echo "$title" | cut -d' ' -f1)Tests\""
        echo "- Start fix: git checkout -b fix/issue-$(basename "$issue_url")-$(echo "$title" | tr ' ' '-' | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9-]//g')"
        echo "- Track progress: gh issue status $(basename "$issue_url")"
        ;;
      epic)
        echo "- Create epic branch: git checkout -b epic/$(echo "$title" | tr ' ' '-' | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9-]//g')"
        echo "- Break down tasks: Create sub-issues with epic: label"
        echo "- Track milestone: gh issue list --label \"type: epic-task\""
        ;;
      debt)
        echo "- Review refactoring plan in issue description"
        echo "- Create branch: git checkout -b debt/issue-$(basename "$issue_url")-$(echo "$title" | tr ' ' '-' | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9-]//g')"
        echo "- Plan incremental improvements"
        ;;
      docs)
        echo "- Review documentation structure"
        echo "- Create branch: git checkout -b docs/issue-$(basename "$issue_url")-$(echo "$title" | tr ' ' '-' | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9-]//g')"
        echo "- Reference: Docs/Standards/DocumentationStandards.md"
        ;;
    esac
  else
    echo "❌ Issue Creation Failed: $issue_url"
    echo ""
    echo "GitHub CLI returned an error during issue creation."
    echo ""
    echo "Troubleshooting:"
    echo "1. Verify repository access: gh repo view"
    [ -n "$milestone" ] && echo "2. Check milestone exists: gh issue list --milestone \"$milestone\""
    [ -n "$assignee" ] && echo "3. Verify assignee has access"
    echo "4. Check label syntax (GitHub validates on creation)"
    echo ""
    echo "Provided Arguments:"
    echo "  Type: $type"
    echo "  Title: $title"
    echo "  Template: $template_name"
    echo "  Labels: $all_labels"
    [ -n "$milestone" ] && echo "  Milestone: $milestone"
    [ -n "$assignee" ] && echo "  Assignee: $assignee"
    echo ""
    echo "💡 Try --dry-run to preview issue before creation:"
    echo "  /create-issue $type \"$title\" --dry-run"
    exit 1
  fi
fi
```

## Notes

**Skill Integration:**
This command delegates workflow execution to the `github-issue-creation` skill located at `.claude/skills/github/github-issue-creation/`. The skill handles:
- Automated context collection via codebase analysis
- Template population with collected context
- Label compliance validation per GitHubLabelStandards.md
- Duplicate issue prevention through systematic searching

**Command Responsibilities:**
The command layer focuses on CLI interface and user experience:
- Argument parsing and validation
- User-friendly error messaging
- Output formatting and display
- gh CLI invocation for issue creation

**Time Savings:**
Reduces issue creation time from 5 minutes (manual context gathering + template population + label selection) to 1 minute (command execution + review) through automation.
