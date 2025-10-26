# Skill Integration Guide: Command-Skill Boundaries & Delegation Patterns

**Version:** 1.0.0
**Category:** Meta-Skill Resource
**Purpose:** Definitive guide to command-skill separation of concerns, delegation strategies, integration flows, and best practices ensuring clean architectural boundaries and maximum reusability

---

## TABLE OF CONTENTS

1. [Command-Skill Separation of Concerns](#1-command-skill-separation-of-concerns)
2. [Integration Patterns](#2-integration-patterns)
3. [Argument Flow Design](#3-argument-flow-design)
4. [Integration Flow Examples](#4-integration-flow-examples)
5. [Best Practices & Anti-Patterns](#5-best-practices--anti-patterns)

---

## 1. COMMAND-SKILL SEPARATION OF CONCERNS

### 1.1 Clear Boundary Definitions

#### Command Layer Responsibilities

**What Commands DO:**

```yaml
INTERFACE_LAYER:
  Argument_Parsing:
    - Extract positional, named, flags from user input
    - Handle optional vs. required argument logic
    - Support flexible argument ordering (two-pass parsing)
    - Validate argument syntax and types

  Input_Validation:
    - Check argument types (string, number, enum)
    - Verify constraint compliance (ranges, formats, patterns)
    - Detect missing required arguments
    - Handle mutual exclusivity flags

  User_Experience:
    - Provide clear error messages with examples
    - Display progress indicators for operations
    - Format success feedback with next steps
    - Maintain consistent emoji usage

  Skill_Delegation:
    - Load skills by name (claude load-skill skill-name)
    - Pass validated arguments to skill
    - Handle skill loading errors
    - Catch skill execution failures

  Output_Formatting:
    - Transform skill results into user-friendly display
    - Apply status indicators (✅ ❌ 🔄)
    - Format timestamps (relative: "3 min ago")
    - Present clickable references (URLs, IDs)
```

**Example Command Code (Thin Layer):**

```bash
#!/bin/bash
# Command: /create-issue
# Responsibility: Interface layer only

# 1. Parse arguments
type="$1"
title="$2"
shift 2

template=""
labels=()
dry_run=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --template) template="$2"; shift 2 ;;
    --label) labels+=("$2"); shift 2 ;;
    --dry-run) dry_run=true; shift ;;
    *) echo "⚠️ Unknown argument '$1'"; exit 1 ;;
  esac
done

# 2. Validate arguments
if [ -z "$type" ] || [ -z "$title" ]; then
  echo "⚠️ Missing required arguments"
  exit 1
fi

case "$type" in
  feature|bug|epic|debt|docs) ;;
  *) echo "⚠️ Invalid type '$type'"; exit 1 ;;
esac

# 3. Load skill (delegation)
claude load-skill github-issue-creation

# Skill handles:
# - Context collection (branch, commits, issues)
# - Template loading and population
# - Label validation
# - Issue creation via gh CLI

# 4. Format output (received from skill)
echo "✅ Issue Created: #456 \"$title\""
echo ""
echo "💡 Next Steps:"
echo "- Refine acceptance criteria"
echo "- Begin implementation"
```

**Why This Works:**
- ✅ **Thin Layer:** Command is ~70 lines (parsing + validation + formatting)
- ✅ **No Business Logic:** No template management, context collection, or gh CLI calls
- ✅ **Testable Layers:** Command tests focus on argument parsing, skill tests focus on workflow

---

#### Skill Layer Responsibilities

**What Skills DO:**

```yaml
IMPLEMENTATION_LAYER:
  Workflow_Execution:
    - Multi-step business logic orchestration
    - Complex decision trees and conditionals
    - State management (if needed)
    - Error handling and recovery

  Context_Collection:
    - Query git for branch, commits, status
    - Execute gh CLI for issues, PRs, workflows
    - Access filesystem for project files
    - Aggregate context from multiple sources

  Resource_Management:
    - Load templates from resources/templates/
    - Access examples from resources/examples/
    - Read documentation from resources/docs/
    - Manage configuration files

  Business_Logic:
    - Template selection (type → template mapping)
    - Placeholder population with validation
    - Label validation against standards
    - Conflict detection and resolution

  External_Integrations:
    - Execute CLI tools (gh, git, docker, npm)
    - Call APIs (GitHub, external services)
    - Process CLI output and parse results
    - Handle integration errors
```

**Example Skill Code (Business Logic):**

```yaml
# Skill: github-issue-creation
# Responsibility: Issue creation workflow

WORKFLOW_PHASES:
  Phase_1_Context_Collection:
    - current_branch=$(git rev-parse --abbrev-ref HEAD)
    - recent_commits=$(git log -5 --oneline)
    - related_issues=$(gh issue list --search "$title_keywords")

  Phase_2_Template_Selection:
    - template_file="resources/templates/issue-${type}.md"
    - template_content=$(cat "$template_file")
    - validate_template_placeholders()

  Phase_3_Template_Population:
    - Replace {{TITLE}} with $title
    - Replace {{CONTEXT}} with branch + commits
    - Replace {{RELATED_ISSUES}} with links

  Phase_4_Issue_Creation:
    - Validate labels against GitHubLabelStandards.md
    - Construct gh CLI command with all parameters
    - Execute: gh issue create --title "$title" --body "$body" --label "$labels"
    - Parse result for issue number and URL

  Phase_5_Result_Return:
    - Return structured JSON to command
    - Include issue number, URL, metadata
    - Report any warnings or validation notes
```

**Why This Works:**
- ✅ **Reusable:** Other commands (`/clone-issue`) or agents (TestEngineer) can use skill
- ✅ **Testable:** Each phase can be unit tested independently
- ✅ **Resource Access:** Skill has resources/ for templates, examples, docs
- ✅ **Complex Logic:** 4-phase workflow with validation and external integrations

---

### 1.2 Benefits of Separation

#### Simplicity

**Commands Stay Thin:**
- **Without Skill:** 300+ lines combining parsing + business logic + formatting
- **With Skill:** 70 lines (parsing + formatting), skill handles 200 lines (business logic)
- **Result:** Clear single responsibility per layer

**Example:**

```yaml
NO_SKILL_ANTI_PATTERN:
  File: .claude/commands/create-issue.md
  Lines: 320
  Content:
    - 30 lines: Argument parsing
    - 50 lines: Context collection (git, gh CLI queries)
    - 40 lines: Template loading
    - 60 lines: Template population
    - 40 lines: Label validation
    - 30 lines: gh CLI execution
    - 20 lines: Output formatting
    - 50 lines: Error handling
  Problems:
    - Testing: Monolithic unit (hard to isolate failures)
    - Reuse: Logic locked in command
    - Maintenance: Changes to any phase affect entire file

WITH_SKILL_PATTERN:
  Command: .claude/commands/create-issue.md (70 lines)
    - 30 lines: Argument parsing
    - 20 lines: Validation
    - 10 lines: Skill loading
    - 10 lines: Output formatting

  Skill: .claude/skills/github/github-issue-creation/SKILL.md (200 lines)
    - 50 lines: Context collection
    - 40 lines: Template management
    - 60 lines: Template population
    - 50 lines: Label validation + gh CLI

  Benefits:
    - Command tests: Argument parsing, validation, formatting
    - Skill tests: Context collection, template logic, label validation
    - Maintenance: Change template logic without touching command
```

---

#### Reusability

**Skills Enable Multiple Consumers:**

```yaml
SKILL: github-issue-creation

CONSUMERS:
  Command_1:
    Name: /create-issue
    Usage: "User-facing CLI for issue creation"
    Delegation: "Full workflow (context → template → create)"

  Command_2:
    Name: /clone-issue <issue-id>
    Usage: "Clone existing issue with modifications"
    Delegation: "Reuses template population + creation phases"

  Agent_1:
    Name: TestEngineer
    Usage: "Create coverage improvement issues programmatically"
    Delegation: "Automated issue creation with test coverage context"

  Agent_2:
    Name: CodeChanger
    Usage: "Create bug issues when discovering errors during implementation"
    Delegation: "Automated bug issue creation with code context"

  Agent_3:
    Name: BugInvestigator
    Usage: "Create issues from diagnostic reports"
    Delegation: "Issue creation with root cause analysis context"

Total_Consumers: 5 (2 commands + 3 agents)
Value: "Centralized workflow logic, consistent issue creation across all consumers"
```

**Without Skill (Anti-Pattern):**
- Each consumer duplicates 200 lines of issue creation logic
- Total duplication: 5 consumers × 200 lines = **1,000 lines duplicated code**
- Maintenance nightmare: Bug fix requires changing 5 files

**With Skill:**
- Skill: 200 lines (single source of truth)
- Consumers: 5 × 70 lines = 350 lines (interface layers only)
- Total: 550 lines (**45% reduction**)
- Maintenance: Bug fix changes 1 file (skill)

---

#### Testability

**Test Layers Independently:**

```yaml
COMMAND_TESTS:
  Focus: "Argument parsing, validation, output formatting"

  Test_Cases:
    - Valid positional args: type="feature", title="Add tagging"
    - Invalid type: type="typo" → Error message quality
    - Missing title: Error message shows usage
    - Repeatable labels: --label x --label y → Array parsing
    - Flag handling: --dry-run → Boolean toggle
    - Unknown argument: --invalid → Error with valid options

  Example:
    Test: "Invalid type shows helpful error"
    Input: /create-issue typo "Title"
    Expected: |
      ⚠️ Invalid Type: 'typo' is not a valid issue type
      Valid types: feature|bug|epic|debt|docs
      Try: /create-issue feature "Add recipe tagging"
    Validation: ✅ Error message is specific, educational, actionable

SKILL_TESTS:
  Focus: "Context collection, template population, label validation, gh CLI integration"

  Test_Cases:
    - Context collection: Branch, commits, related issues retrieved
    - Template selection: type="feature" → issue-feature.md loaded
    - Placeholder population: {{TITLE}} replaced with user title
    - Label validation: Invalid label → GitHubLabelStandards.md check fails
    - gh CLI execution: Issue created with correct parameters
    - Dry-run mode: Preview displayed, no gh CLI call

  Example:
    Test: "Template population includes branch context"
    Setup: Current branch = "feature/issue-123"
    Skill_Input: {type: "feature", title: "Add tagging"}
    Skill_Execution: Collect context, load template, populate
    Assertion: Populated template contains "Branch: feature/issue-123"
    Validation: ✅ Context collection working

INTEGRATION_TESTS:
  Focus: "Command → Skill → gh CLI end-to-end"

  Test_Cases:
    - Full workflow: /create-issue → Issue created in GitHub
    - Error propagation: Skill error → Command displays formatted error
    - Dry-run flow: --dry-run → Preview displayed, no issue created

  Example:
    Test: "End-to-end issue creation"
    Input: /create-issue feature "Integration test issue" --label testing
    Execution: Command parses → Skill creates → gh CLI executes
    Assertion: Issue exists in GitHub with correct title, type label, testing label
    Cleanup: gh issue delete <created-issue-id>
    Validation: ✅ Full workflow functional
```

---

#### Maintainability

**Change Impact Isolation:**

```yaml
SCENARIO_1_TEMPLATE_CHANGE:
  Change: "Update issue-feature.md template structure"
  Impact_Without_Skill:
    - Change all 5 consumers (2 commands + 3 agents)
    - Risk: Miss updating one consumer → inconsistent templates
  Impact_With_Skill:
    - Change skill template loading logic (1 file)
    - All consumers automatically use new template
    - Testing: Test skill only

SCENARIO_2_LABEL_VALIDATION_ENHANCEMENT:
  Change: "Add fuzzy matching for invalid labels"
  Impact_Without_Skill:
    - Update validation logic in all 5 consumers
    - Risk: Inconsistent fuzzy matching implementations
  Impact_With_Skill:
    - Update skill label validation phase (1 function)
    - All consumers get fuzzy matching automatically
    - Testing: Test skill validation function

SCENARIO_3_GH_CLI_API_CHANGE:
  Change: "gh CLI v3.0 changes issue create API"
  Impact_Without_Skill:
    - Update gh CLI calls in all 5 consumers
    - Risk: Different error handling across consumers
  Impact_With_Skill:
    - Update skill gh CLI integration (1 file)
    - All consumers compatible with gh CLI v3.0
    - Testing: Test skill gh CLI wrapper

RESULT:
  Maintenance_Time_Reduction: "80% (change 1 file vs. 5 files)"
  Risk_Reduction: "90% (single source of truth prevents inconsistency)"
  Testing_Effort: "70% reduction (test skill once vs. 5 integration tests)"
```

---

## 2. INTEGRATION PATTERNS

### 2.1 Pattern 1: Direct Skill Loading

**When to Use:**
- Skill is production-ready and registered in `.claude/skills/`
- Progressive loading desired (YAML frontmatter → SKILL.md → resources)
- Skill versioning and updates needed
- Multiple commands will delegate to same skill

**Command Implementation:**

```bash
#!/bin/bash
# Pattern 1: Direct Skill Loading

# Step 1: Parse and validate arguments
type="$1"
title="$2"
# ... argument parsing logic ...

# Step 2: Load skill by name
echo "🔄 Loading github-issue-creation skill..."
claude load-skill github-issue-creation

# What happens:
# - Claude Code loads .claude/skills/github/github-issue-creation/skill.yaml
# - Reads frontmatter (name, description, category)
# - Loads SKILL.md with full workflow definition
# - Accesses resources/ on-demand

# Step 3: Skill executes workflow
# Skill handles:
# - Context collection
# - Template selection
# - Placeholder population
# - Label validation
# - Issue creation

# Step 4: Format skill output
# Skill returns structured JSON:
# {
#   "status": "success",
#   "issue": {"number": 456, "url": "https://..."},
#   "metadata": {"labels": ["type: feature", "frontend"]}
# }

# Command formats for user:
echo "✅ Issue Created: #456 \"$title\""
echo "https://github.com/.../issues/456"
```

**Skill Structure:**

```
.claude/skills/github/github-issue-creation/
├── skill.yaml                          # Frontmatter
├── SKILL.md                            # Workflow definition
└── resources/
    ├── templates/
    │   ├── issue-feature.md
    │   ├── issue-bug.md
    │   ├── issue-epic.md
    │   ├── issue-debt.md
    │   └── issue-docs.md
    ├── examples/
    │   └── sample-feature-issue.md
    └── docs/
        ├── label-validation.md
        └── context-collection-guide.md
```

**Advantages:**
- ✅ **Automatic Discovery:** Claude Code finds skill by name
- ✅ **Progressive Loading:** Loads only what's needed (frontmatter first, SKILL.md if executed, resources on-demand)
- ✅ **Versioning:** Skill updates propagate to all consumers
- ✅ **Clean Separation:** Command has zero knowledge of skill internals

**Example Output:**

```
🔄 Loading github-issue-creation skill...
🔄 Collecting project context...
📊 Analyzing related issues and commits...
📝 Populating feature request template...
✅ Validating labels...

✅ Issue Created: #456 "Add recipe tagging system"

Details:
• Type: feature
• Labels: type: feature, frontend
• Context: Branch feature/issue-455, 5 commits

https://github.com/Zarichney-Development/zarichney-api/issues/456

💡 Next Steps:
- Refine acceptance criteria
- Begin implementation: git checkout -b feature/issue-456
```

---

### 2.2 Pattern 2: Task Tool Delegation with Context Package

**When to Use:**
- Skill not formally registered (rapid prototyping)
- Need explicit control over skill execution context
- Debugging skill workflows (see full context package)
- One-off skill invocations

**Command Implementation:**

```bash
#!/bin/bash
# Pattern 2: Task Tool Delegation

# Step 1: Parse and validate arguments
type="$1"
title="$2"
# ... argument parsing ...

# Step 2: Construct comprehensive context package
CONTEXT=$(cat <<EOF
Role: github-issue-creation executor

Task: Create GitHub issue with automated context collection

Arguments:
- type: $type
- title: $title
- labels: ${labels[@]}
- dry_run: $dry_run

Resources:
- Skill: .claude/skills/github/github-issue-creation/SKILL.md
- Templates: .claude/skills/github/github-issue-creation/resources/templates/
- Validation: Docs/Standards/GitHubLabelStandards.md

Workflow:
1. Collect context (branch, commits, related issues)
2. Load template: issue-$type.md
3. Populate template placeholders
4. Validate labels against GitHubLabelStandards.md
5. Create issue via gh CLI (or preview if dry-run)

Expected Output:
- Issue number and URL
- Applied labels
- Context summary

Error Handling:
- Invalid labels: Show valid labels from GitHubLabelStandards.md
- Template not found: List available templates
- gh CLI failure: Provide troubleshooting steps
EOF
)

# Step 3: Invoke via Task tool
echo "🔄 Delegating to github-issue-creation workflow..."
claude task --type github --context "$CONTEXT"

# Step 4: Format task output
# Task tool returns skill results, command formats for user
echo "✅ Issue Created: #456 \"$title\""
```

**Context Package Template:**

```yaml
CONTEXT_PACKAGE_STRUCTURE:
  Role: "[Skill executor role description]"
  Task: "[High-level objective]"

  Arguments:
    - argument_1: value
    - argument_2: value
    - flags: true/false

  Resources:
    - Skill: "Path to SKILL.md"
    - Templates: "Path to resources/"
    - Documentation: "Relevant standards docs"

  Workflow:
    - "Step 1: Action"
    - "Step 2: Action"
    - "Step N: Action"

  Expected_Output:
    - "Key result 1"
    - "Key result 2"

  Error_Handling:
    - "Error scenario 1: Recovery steps"
    - "Error scenario 2: Recovery steps"
```

**Advantages:**
- ✅ **Explicit Control:** Full visibility into skill execution context
- ✅ **Debugging:** Easy to inspect context package in logs
- ✅ **Flexibility:** Works without formal skill registration
- ✅ **Prototyping:** Rapid iteration during skill development

**Disadvantages:**
- ❌ **Verbose:** More code in command (context package construction)
- ❌ **No Progressive Loading:** Must specify all paths manually
- ❌ **Maintenance:** Context package in command couples to skill structure

**When to Use Task Tool:**
- Rapid prototyping (skill not finalized)
- Debugging complex workflows (need full context visibility)
- One-off integrations (skill won't be reused)

**When to Use Direct Loading:**
- Production skills (registered and stable)
- Multiple consumers (reusability)
- Progressive loading benefits (token efficiency)

---

### 2.3 Pattern 3: Multi-Skill Orchestration

**When to Use:**
- Command coordinates multiple skills for comprehensive workflow
- Skills have dependencies (skill A output → skill B input)
- Complex multi-phase operations

**Example: /create-epic-with-issues**

```bash
#!/bin/bash
# Pattern 3: Multi-Skill Orchestration

# Workflow:
# 1. Create epic issue (github-issue-creation skill)
# 2. Break down epic into sub-issues (epic-breakdown skill)
# 3. Create project board (github-project-management skill)
# 4. Link sub-issues to epic (issue-linking skill)

epic_title="$1"
epic_description="$2"

# Phase 1: Create Epic Issue
echo "🔄 Phase 1: Creating epic issue..."
claude load-skill github-issue-creation

EPIC_RESULT=$(create_issue \
  --type epic \
  --title "$epic_title" \
  --description "$epic_description")

epic_number=$(echo "$EPIC_RESULT" | jq -r '.issue.number')
echo "✅ Epic Created: #$epic_number"

# Phase 2: Break Down Epic
echo "🔄 Phase 2: Analyzing epic breakdown..."
claude load-skill epic-breakdown

BREAKDOWN_RESULT=$(analyze_epic \
  --epic-number "$epic_number" \
  --epic-description "$epic_description")

sub_issues=$(echo "$BREAKDOWN_RESULT" | jq -r '.sub_issues[]')
echo "✅ Identified $(echo "$sub_issues" | wc -l) sub-issues"

# Phase 3: Create Sub-Issues
echo "🔄 Phase 3: Creating sub-issues..."
for sub_issue in $sub_issues; do
  sub_title=$(echo "$sub_issue" | jq -r '.title')
  sub_type=$(echo "$sub_issue" | jq -r '.type')

  SUB_RESULT=$(create_issue \
    --type "$sub_type" \
    --title "$sub_title" \
    --milestone "Epic #$epic_number")

  sub_number=$(echo "$SUB_RESULT" | jq -r '.issue.number')
  echo "  ✅ Created: #$sub_number \"$sub_title\""
done

# Phase 4: Create Project Board
echo "🔄 Phase 4: Creating project board..."
claude load-skill github-project-management

PROJECT_RESULT=$(create_project \
  --name "$epic_title" \
  --epic-number "$epic_number")

project_url=$(echo "$PROJECT_RESULT" | jq -r '.project.url')
echo "✅ Project Board: $project_url"

# Summary
echo ""
echo "🎯 Epic Creation Complete:"
echo "  • Epic: #$epic_number \"$epic_title\""
echo "  • Sub-Issues: $(echo "$sub_issues" | wc -l) created"
echo "  • Project Board: $project_url"
echo ""
echo "💡 Next Steps:"
echo "  - Review sub-issues and refine"
echo "  - Assign team members"
echo "  - Set sprint milestones"
```

**Skill Dependency Chain:**

```yaml
SKILL_CHAIN:
  Skill_1:
    Name: github-issue-creation
    Input: {type: "epic", title, description}
    Output: {epic_number, epic_url}

  Skill_2:
    Name: epic-breakdown
    Input: {epic_number, epic_description}
    Dependency: Skill_1.epic_number
    Output: {sub_issues: [{title, type}, ...]}

  Skill_3_Loop:
    Name: github-issue-creation (reused)
    Input: {type: sub_type, title: sub_title, milestone: epic_number}
    Dependency: Skill_2.sub_issues
    Output: {sub_issue_numbers: [...]}

  Skill_4:
    Name: github-project-management
    Input: {name: epic_title, epic_number, sub_issues}
    Dependency: Skill_1.epic_number, Skill_3.sub_issue_numbers
    Output: {project_url}
```

**Advantages:**
- ✅ **Complex Workflows:** Orchestrates multi-step processes
- ✅ **Skill Reuse:** Uses github-issue-creation twice (epic + sub-issues)
- ✅ **Dependency Management:** Passes outputs between skills

**Challenges:**
- ⚠️ **Error Handling:** Need to handle failures at each phase
- ⚠️ **State Management:** Track epic_number, sub_issues across phases
- ⚠️ **Rollback:** If phase 4 fails, how to clean up phases 1-3?

---

### 2.4 Pattern 4: Skill with Workflow Trigger

**When to Use:**
- GitHub Actions workflow contains orchestration logic
- Command provides manual trigger interface
- Long-running workflows (minutes to hours)
- Monitoring and result retrieval needed

**Example: /merge-coverage-prs**

```bash
#!/bin/bash
# Pattern 4: Workflow Trigger with Monitoring

# Workflow:
# 1. Command parses arguments
# 2. Command triggers GitHub Actions workflow
# 3. Skill monitors workflow execution (optional)
# 4. Command reports results

dry_run="${1:-true}" # Default true for safety
max="${2:-8}"
labels="${3:-type: coverage,coverage,testing}"

# Phase 1: Validate Arguments
if [ "$dry_run" != "true" ] && [ "$dry_run" != "false" ]; then
  echo "⚠️ Invalid --dry-run value: $dry_run"
  exit 1
fi

# Phase 2: Trigger Workflow
echo "🔄 Triggering Coverage Excellence Merge Orchestrator..."
RUN_ID=$(gh workflow run coverage-excellence-merge-orchestrator.yml \
  --field dry_run="$dry_run" \
  --field max_prs="$max" \
  --field pr_label_filter="$labels" \
  --json databaseId \
  --jq '.databaseId')

echo "✅ Workflow Triggered: Run #$RUN_ID"

# Phase 3: Monitor Workflow (Optional Skill)
if command -v claude &> /dev/null; then
  echo "🔄 Monitoring workflow execution..."
  claude load-skill workflow-monitor

  # Skill monitors:
  # - Poll gh run view $RUN_ID every 30s
  # - Display job status updates
  # - Report completion or failure

  WORKFLOW_RESULT=$(monitor_workflow --run-id "$RUN_ID" --timeout 600)
  status=$(echo "$WORKFLOW_RESULT" | jq -r '.status')

  if [ "$status" = "success" ]; then
    echo "✅ Workflow Completed Successfully"
  else
    echo "❌ Workflow Failed: Check logs"
    gh run view "$RUN_ID" --log
    exit 1
  fi
else
  # Manual monitoring
  echo "💡 Monitor manually:"
  echo "  gh run watch $RUN_ID"
  echo "  gh run view $RUN_ID --log"
fi

# Phase 4: Report Results
echo ""
echo "🎯 Coverage PR Consolidation Complete:"
echo "  • Workflow: https://github.com/.../runs/$RUN_ID"
echo "  • Dry Run: $dry_run"
echo "  • Max PRs: $max"
echo ""
echo "💡 Next Steps:"
if [ "$dry_run" = "true" ]; then
  echo "  - Review consolidated PR preview"
  echo "  - Run with --dry-run=false to execute"
else
  echo "  - Review consolidated PR"
  echo "  - Merge when ready"
fi
```

**Skill Role (Optional):**

```yaml
SKILL: workflow-monitor

PURPOSE: "Monitor GitHub Actions workflow execution with real-time updates"

WORKFLOW:
  Poll_Status:
    - Every 30 seconds: gh run view $RUN_ID --json status
    - Display job updates
    - Check for completion or failure

  Display_Updates:
    - "🔄 Setup: In Progress..."
    - "✅ Setup: Complete (34s)"
    - "🔄 Consolidate: Running..."
    - "❌ Consolidate: Failed (see logs)"

  Return_Result:
    - {status: "success|failure", duration: "5m 12s", url: "..."}
```

**When Skill Adds Value:**
- ✅ **Real-Time Updates:** User sees progress without manual polling
- ✅ **Smart Waiting:** Skill waits for completion, not command
- ✅ **Reusability:** Other workflow trigger commands use same monitoring skill

**When Skill Not Needed:**
- ❌ **Simple Trigger:** Just launch workflow, user monitors manually
- ❌ **No Monitoring Value:** Workflow completes in <10 seconds

---

## 3. ARGUMENT FLOW DESIGN

### 3.1 Command → Skill Argument Mapping

**Mapping Strategies:**

```yaml
STRATEGY_1_DIRECT_MAPPING:
  Use_When: "Command argument names match skill parameter names"

  Example:
    Command_Args:
      - type: "feature"
      - title: "Add recipe tagging"
      - labels: ["frontend", "enhancement"]

    Skill_Params:
      - issue_type: $type          # Direct mapping
      - issue_title: $title        # Direct mapping
      - additional_labels: $labels # Direct mapping

STRATEGY_2_TRANSFORMATION:
  Use_When: "Command arguments require transformation before skill consumption"

  Example:
    Command_Args:
      - workflow_name: "build.yml"
      - limit: 10

    Skill_Params:
      - workflow_id: $(get_workflow_id "$workflow_name") # Transform name → ID
      - max_results: $limit                              # Rename for clarity

STRATEGY_3_AGGREGATION:
  Use_When: "Multiple command arguments combine into single skill parameter"

  Example:
    Command_Args:
      - type: "feature"
      - domain: "frontend"

    Skill_Params:
      - labels: ["type: $type", "$domain"] # Aggregate into labels array

STRATEGY_4_DEFAULT_INJECTION:
  Use_When: "Command provides defaults skill always needs"

  Example:
    Command_Args:
      - title: "Add tagging"
      - (no template specified)

    Skill_Params:
      - title: $title
      - template: "issue-${type}.md" # Command injects default
```

---

### 3.2 Validation Layers

**Two-Layer Validation Approach:**

```yaml
LAYER_1_COMMAND_VALIDATION:
  Purpose: "Fast-fail on syntactic and type errors before skill invocation"

  Validations:
    Syntax:
      - Required arguments present
      - Argument types correct (string, number, enum)
      - Flags are boolean (no values)

    Format:
      - Strings non-empty
      - Numbers in valid range
      - Enums match allowed values
      - Patterns match regex (email, URL, etc.)

    Mutual_Exclusivity:
      - Conflicting flags detected
      - Argument dependencies validated

  Example:
    /create-issue <type> <title>
    Command_Validation:
      - type enum check: feature|bug|epic|debt|docs
      - title length: 10-200 characters
      - labels format: lowercase-hyphenated

  Why_Here:
    - Fast failure (no skill loading overhead)
    - Clear user errors (command knows argument constraints)
    - Prevents unnecessary skill invocations

LAYER_2_SKILL_VALIDATION:
  Purpose: "Business logic and external dependency validation"

  Validations:
    Business_Rules:
      - Label exists in GitHubLabelStandards.md
      - Template contains required placeholders
      - Workflow allows requested operation

    External_Dependencies:
      - Milestone exists in repository
      - Assignee has repository access
      - Workflow file exists in .github/workflows/

    State_Validation:
      - Branch is pushed to remote
      - No merge conflicts exist
      - Required files present

  Example:
    github-issue-creation skill
    Skill_Validation:
      - Label "fronted" → Not in GitHubLabelStandards.md
        - Fuzzy match: "Did you mean 'frontend'?"
      - Milestone "Epic #999" → Not in repository
        - List available milestones
      - Template "custom.md" → File not found
        - List available templates

  Why_Here:
    - Requires resource access (GitHubLabelStandards.md, gh CLI)
    - Business logic context (fuzzy matching, validation rules)
    - External API validation (GitHub repository state)
```

**Validation Flow:**

```
User Input
  ↓
Command Layer Validation
  ├─ FAIL → User-friendly error with fix example
  │   └─ Exit (no skill invocation)
  │
  └─ PASS → Load skill
       ↓
     Skill Layer Validation
       ├─ FAIL → Business rule error with alternatives
       │   └─ Return error to command → Format for user
       │
       └─ PASS → Execute workflow
            ↓
          Success → Return results to command → Format for user
```

---

### 3.3 Error Propagation

**Command → Skill → Command Flow:**

```yaml
ERROR_FLOW:
  Command_Errors:
    Layer: "Argument parsing and validation"
    Handling: "Command displays error directly, no skill involvement"
    Format: "⚠️ [Category]: [Issue]. [Explanation]. Try: [Example]"

    Example:
      Input: /create-issue typo "Title"
      Command_Error: "⚠️ Invalid Type: 'typo' is not valid (feature|bug|epic|debt|docs)"
      Action: "Exit, no skill loaded"

  Skill_Errors:
    Layer: "Business logic execution"
    Handling: "Skill returns structured error, command formats for user"
    Format: "Skill error object → Command user-friendly message"

    Example:
      Input: /create-issue feature "Title" --label fronted
      Skill_Execution: "Validate labels → 'fronted' not in GitHubLabelStandards.md"
      Skill_Error:
        {
          "status": "error",
          "category": "validation_failed",
          "message": "Label 'fronted' not found",
          "suggestions": ["frontend", "backend"],
          "fuzzy_match": "frontend"
        }
      Command_Formats:
        ⚠️ Label Validation Failed: 'fronted' not found

        Did you mean:
        • frontend (closest match)
        • backend
        • fullstack

        Try: /create-issue feature "Title" --label frontend

  External_Errors:
    Layer: "CLI tools or APIs (gh CLI, git, etc.)"
    Handling: "Skill catches, enriches, returns to command"
    Format: "Raw error → Skill context → Command troubleshooting"

    Example:
      Skill_Execution: gh issue create → API rate limit exceeded
      Raw_Error: "HTTP 403: API rate limit exceeded"
      Skill_Enriches:
        {
          "status": "error",
          "category": "api_failure",
          "message": "GitHub API rate limit exceeded",
          "recovery": "Check rate limit: gh api /rate_limit",
          "retry_after": "23 minutes"
        }
      Command_Formats:
        ⚠️ API Rate Limit Exceeded

        Your limit: 5000 requests/hour
        Resets in: 23 minutes

        Troubleshooting:
        • Check rate limit: gh api /rate_limit
        • Authenticate for higher limit: gh auth login

        Try again in: 23 minutes
```

**Error Enrichment Benefits:**

```yaml
WITHOUT_ENRICHMENT:
  Raw_Error: "HTTP 403"
  User_Experience: "What does HTTP 403 mean? How do I fix?"
  Resolution_Time: "5-10 minutes (search error, find solution)"

WITH_ENRICHMENT:
  Skill_Error: "GitHub API rate limit exceeded (resets in 23 min)"
  User_Experience: "Clear problem, knows to wait 23 minutes or authenticate"
  Resolution_Time: "0 minutes (self-service resolution)"

ENRICHMENT_VALUE: "90% error self-resolution, 5-10 min saved per error"
```

---

### 3.4 Result Formatting Contract

**Skill → Command Data Contract:**

```yaml
SKILL_RESULT_STRUCTURE:
  status: "success|error|warning"

  data:
    # Skill-specific result data
    issue:
      number: 456
      title: "Add recipe tagging system"
      url: "https://github.com/.../issues/456"

    metadata:
      type: "feature"
      labels: ["type: feature", "frontend", "enhancement"]
      milestone: "Epic #291"
      assignee: "@zarichney"

    context:
      branch: "feature/issue-455"
      commits_count: 5
      related_issues_count: 2

  warnings: [] # Optional warnings that don't fail workflow

  errors: [] # Validation errors if status != "success"
```

**Command Formatting Responsibility:**

```bash
# Command receives skill result
SKILL_RESULT='{"status":"success","data":{"issue":{"number":456,"title":"Add recipe tagging"}}}'

# Command formats for user display
echo "✅ Issue Created: #$(echo "$SKILL_RESULT" | jq -r '.data.issue.number') \"$(echo "$SKILL_RESULT" | jq -r '.data.issue.title')\""
echo ""
echo "Details:"
echo "• Type: $(echo "$SKILL_RESULT" | jq -r '.data.metadata.type')"
echo "• Labels: $(echo "$SKILL_RESULT" | jq -r '.data.metadata.labels | join(", ")')"
echo ""
echo "$(echo "$SKILL_RESULT" | jq -r '.data.issue.url')"
echo ""
echo "💡 Next Steps:"
echo "- Refine acceptance criteria"
echo "- Begin implementation"
```

**Output Components:**
1. **Success Indicator:** ✅ emoji + action summary
2. **Key Details:** Metadata bulleted list
3. **Reference:** URL or ID for deep-dive
4. **Next Steps:** Contextual suggestions

---

## 4. INTEGRATION FLOW EXAMPLES

### 4.1 Example 1: /create-issue (Full Skill Integration)

**Command-Skill Interaction Flow:**

```yaml
PHASE_1_USER_INVOCATION:
  User_Input: "/create-issue feature \"Add recipe tagging\" --label frontend --dry-run"

  Command_Parsing:
    type: "feature"
    title: "Add recipe tagging"
    labels: ["frontend"]
    dry_run: true

PHASE_2_COMMAND_VALIDATION:
  Validation_Checks:
    - type enum: ✅ "feature" is valid
    - title length: ✅ 17 characters (10-200 range)
    - labels format: ✅ "frontend" is lowercase-hyphenated

  Result: PASS → Proceed to skill loading

PHASE_3_SKILL_LOADING:
  Command_Action: "claude load-skill github-issue-creation"

  Claude_Code_Action:
    1. Load .claude/skills/github/github-issue-creation/skill.yaml
    2. Read frontmatter (name, description)
    3. Load SKILL.md (workflow definition)
    4. Skill ready for execution

PHASE_4_SKILL_EXECUTION:
  Skill_Workflow:
    Step_1_Context_Collection:
      - git rev-parse --abbrev-ref HEAD → "feature/issue-455"
      - git log -5 --oneline → [commits list]
      - gh issue list --search "tagging" → [related issues]

    Step_2_Template_Selection:
      - Map type="feature" → "resources/templates/issue-feature.md"
      - Load template content

    Step_3_Template_Population:
      - Replace {{TITLE}} with "Add recipe tagging"
      - Replace {{BRANCH}} with "feature/issue-455"
      - Replace {{COMMITS}} with [commits list]
      - Replace {{RELATED_ISSUES}} with [issue links]

    Step_4_Label_Validation:
      - Check "type: feature" in GitHubLabelStandards.md: ✅
      - Check "frontend" in GitHubLabelStandards.md: ✅

    Step_5_Dry_Run_Preview:
      - dry_run=true → Skip gh issue create
      - Return preview data

PHASE_5_SKILL_RESULT_RETURN:
  Skill_Returns:
    {
      "status": "dry_run",
      "preview": {
        "title": "Add recipe tagging",
        "type": "feature",
        "labels": ["type: feature", "frontend"],
        "body": "[populated template markdown]"
      }
    }

PHASE_6_COMMAND_OUTPUT_FORMATTING:
  Command_Displays:
    🔄 Collecting project context...
    📊 Analyzing related issues and commits...
    📝 Populating feature request template...
    ✅ Validating labels...

    📋 DRY RUN - Issue Preview (not created):

    Title: Add recipe tagging
    Type: feature
    Template: issue-feature.md

    Labels:
    • type: feature
    • frontend

    Issue Body Preview:
    ---
    [Full markdown template with populated context]
    ---

    💡 To create this issue:
    /create-issue feature "Add recipe tagging" --label frontend

    ⚠️ Review template fields before creating

TOTAL_TIME: ~3 seconds (context collection + template population)
```

---

### 4.2 Example 2: /workflow-status (Direct CLI, No Skill)

**Command-Only Flow:**

```yaml
PHASE_1_USER_INVOCATION:
  User_Input: "/workflow-status build.yml --limit 10"

  Command_Parsing:
    workflow_name: "build.yml"
    limit: 10
    branch: "" (default current branch)
    details: false

PHASE_2_COMMAND_VALIDATION:
  Validation_Checks:
    - workflow_name format: ✅ Valid string
    - limit range: ✅ 10 is within 1-50
    - limit type: ✅ Integer

  Result: PASS → Proceed to execution

PHASE_3_DIRECT_CLI_EXECUTION:
  Command_Action:
    GH_CMD="gh run list --workflow=\"build.yml\" --limit 10"
    eval "$GH_CMD"

  gh_CLI_Output:
    [Raw table with workflow runs]

PHASE_4_COMMAND_OUTPUT_FORMATTING:
  Command_Transforms:
    - Add status emojis (✅ ❌ 🔄)
    - Format relative timestamps ("3 min ago")
    - Calculate trend (8/10 success = 80%)
    - Add next steps

  Command_Displays:
    🔄 Fetching runs for 'build.yml'...

    Build Workflow - Last 10 Runs:

    ✅ main       3 min ago    success   (2m 34s)
    ✅ main       3 hrs ago    success   (2m 41s)
    ❌ feature    6 hrs ago    failure   (1m 52s)
    ...

    Trend: 8/10 success (80%)

    💡 Next Steps:
    - Investigate failures: /workflow-status build.yml --details

TOTAL_TIME: ~2 seconds (gh CLI query + formatting)

NO_SKILL_NEEDED_BECAUSE:
  - Simple 3-step workflow (parse → execute → format)
  - No reusability (only /workflow-status needs this)
  - No complex business logic (direct gh CLI wrapper)
  - No resource management (no templates needed)
```

---

### 4.3 Example 3: /merge-coverage-prs (Workflow Trigger)

**Command-Workflow Integration:**

```yaml
PHASE_1_USER_INVOCATION:
  User_Input: "/merge-coverage-prs --dry-run=false --max 15"

  Command_Parsing:
    dry_run: false
    max: 15
    labels: "type: coverage,coverage,testing" (default)

PHASE_2_COMMAND_VALIDATION:
  Validation_Checks:
    - dry_run type: ✅ Boolean
    - max range: ✅ 15 within 1-50
    - labels format: ✅ Comma-separated string

  Result: PASS → Trigger workflow

PHASE_3_WORKFLOW_TRIGGER:
  Command_Action:
    gh workflow run coverage-excellence-merge-orchestrator.yml \
      --field dry_run=false \
      --field max_prs=15 \
      --field pr_label_filter="type: coverage,coverage,testing"

  gh_CLI_Returns:
    run_id: 12345678

  Command_Displays:
    ✅ Workflow Triggered: Run #12345678
    🔄 Monitoring workflow execution...

PHASE_4_OPTIONAL_SKILL_MONITORING:
  Skill_If_Available: workflow-monitor

  Skill_Workflow:
    1. Poll every 30s: gh run view 12345678 --json status
    2. Display updates:
       🔄 Setup: In Progress...
       ✅ Setup: Complete (34s)
       🔄 Consolidate PRs: Running...
       🔄 AI Conflict Resolution: In Progress...
    3. Detect completion or failure
    4. Return result

  Without_Skill:
    Command_Displays:
      💡 Monitor manually:
        gh run watch 12345678
        gh run view 12345678 --log

PHASE_5_RESULT_REPORTING:
  Workflow_Completes:
    status: "success"
    duration: "8m 45s"
    consolidated_pr: "#789"

  Command_Displays:
    ✅ Workflow Completed Successfully (8m 45s)

    Results:
    • PRs Consolidated: 12 PRs → PR #789
    • Conflicts Resolved: 3 (AI-powered)
    • Coverage Gain: +2.3% (78.5% → 80.8%)

    https://github.com/.../pull/789

    💡 Next Steps:
    - Review consolidated PR
    - Merge when ready: gh pr merge 789
    - Check coverage: /coverage-report

TOTAL_TIME: ~9 minutes (workflow execution time)

SKILL_VALUE:
  With_Monitoring_Skill:
    - Real-time updates during 9-minute workflow
    - User sees progress, knows when to check back
    - Automatic completion notification

  Without_Monitoring_Skill:
    - User manually polls: gh run watch 12345678
    - Less convenient but functional
    - Command provides monitoring guidance
```

---

## 5. BEST PRACTICES & ANTI-PATTERNS

### 5.1 Best Practices

#### Keep Commands Thin (70-150 Lines)

**Good Example:**

```bash
#!/bin/bash
# /create-issue - 85 lines total

# Argument parsing: 30 lines
type="$1"
title="$2"
# ... parsing logic ...

# Validation: 20 lines
if [ -z "$type" ]; then
  echo "⚠️ Missing required argument: type"
  exit 1
fi
# ... validation logic ...

# Skill delegation: 10 lines
claude load-skill github-issue-creation

# Output formatting: 25 lines
echo "✅ Issue Created: #456 \"$title\""
echo "💡 Next Steps: ..."
```

**Why Good:**
- ✅ **Single Responsibility:** Interface layer only (parsing, validation, formatting)
- ✅ **Testable:** Easy to unit test argument parsing
- ✅ **Maintainable:** Changes to business logic don't affect command

---

**Bad Example (Anti-Pattern):**

```bash
#!/bin/bash
# /create-issue - 320 lines (BLOATED)

# Argument parsing: 30 lines
type="$1"
title="$2"

# Context collection (SHOULD BE IN SKILL): 50 lines
current_branch=$(git rev-parse --abbrev-ref HEAD)
commits=$(git log -5 --oneline)
related_issues=$(gh issue list --search "$title")

# Template loading (SHOULD BE IN SKILL): 40 lines
template_file="Docs/Templates/issue-${type}.md"
template_content=$(cat "$template_file")
# ... template validation ...

# Template population (SHOULD BE IN SKILL): 60 lines
populated_body="${template_content//\{\{TITLE\}\}/$title}"
# ... complex string replacement logic ...

# Label validation (SHOULD BE IN SKILL): 40 lines
for label in "${labels[@]}"; do
  if ! grep -q "^$label$" Docs/Standards/GitHubLabelStandards.md; then
    echo "⚠️ Invalid label: $label"
  fi
done

# gh CLI execution (SHOULD BE IN SKILL): 30 lines
gh issue create --title "$title" --body "$populated_body" --label "$labels"

# Output formatting: 20 lines
echo "✅ Issue Created"

# Error handling (DUPLICATED IN EVERY PHASE): 50 lines
```

**Why Bad:**
- ❌ **Fat Command:** 320 lines with business logic embedded
- ❌ **No Reusability:** Logic locked in command, can't be used elsewhere
- ❌ **Hard to Test:** Monolithic unit, difficult to isolate failures
- ❌ **Maintenance Nightmare:** Template logic change requires command update

**Fix:** Extract lines 80-280 (context, template, validation, execution) into `github-issue-creation` skill.

---

#### Extract Complex Logic to Skills

**Decision Rule:**

```yaml
EXTRACT_TO_SKILL_WHEN:
  Logic_Exceeds: "100 lines"
  OR
  Phases_Exceed: "4 distinct phases"
  OR
  Reusable_By: "2+ consumers"
  OR
  Resources_Needed: "Templates, examples, docs"
```

**Example: When to Extract**

```yaml
SCENARIO: /create-issue Command

Command_Logic_Analysis:
  Argument_Parsing: 30 lines ← Keep in command
  Validation: 20 lines ← Keep in command
  Context_Collection: 50 lines ← EXTRACT TO SKILL
  Template_Management: 40 lines ← EXTRACT TO SKILL
  Template_Population: 60 lines ← EXTRACT TO SKILL
  Label_Validation: 40 lines ← EXTRACT TO SKILL
  gh_CLI_Execution: 30 lines ← EXTRACT TO SKILL
  Output_Formatting: 25 lines ← Keep in command

Total_Logic: 295 lines
Business_Logic: 220 lines (context → execution)
Interface_Logic: 75 lines (parsing, validation, formatting)

Decision: ✅ EXTRACT 220 lines to github-issue-creation skill

Result:
  Command: 75 lines (interface layer)
  Skill: 220 lines (business logic)
  Separation: Clear, testable, reusable
```

---

#### Use Skills for Reusable Patterns

**Example: Multi-Consumer Skill**

```yaml
SKILL: github-issue-creation

CONSUMERS:
  Command_1: /create-issue (user-facing CLI)
  Command_2: /clone-issue (duplicates existing issue)
  Agent_1: TestEngineer (creates coverage issues)
  Agent_2: CodeChanger (creates bug issues)
  Agent_3: BugInvestigator (creates diagnostic issues)

SHARED_LOGIC:
  - Context collection (branch, commits, issues)
  - Template selection and loading
  - Placeholder population
  - Label validation
  - gh CLI issue creation

VALUE:
  Without_Skill: 5 consumers × 220 lines = 1,100 lines (duplicated)
  With_Skill: 1 skill (220 lines) + 5 consumers (75 lines each) = 595 lines
  Reduction: 45% code reduction + single source of truth
```

---

#### Test Skills Independently

**Testing Strategy:**

```yaml
COMMAND_TESTS:
  Focus: "Argument parsing, validation, output formatting"
  Scope: "Command file only, no skill execution"
  Method: "Mock skill results, test command formatting"

  Example_Test:
    Test_Name: "Invalid type shows helpful error"
    Input: /create-issue typo "Title"
    Expected_Output: |
      ⚠️ Invalid Type: 'typo' is not valid
      Valid types: feature|bug|epic|debt|docs
      Try: /create-issue feature "Add tagging"
    Assertion: Error message is specific, educational, actionable
    Pass_Criteria: ✅ Error format matches template

SKILL_TESTS:
  Focus: "Context collection, template logic, label validation, gh CLI integration"
  Scope: "Skill workflow phases, no command involvement"
  Method: "Direct skill API calls, assert phase outputs"

  Example_Test:
    Test_Name: "Template population includes branch context"
    Setup: Current branch = "feature/issue-123"
    Skill_Input: {type: "feature", title: "Add tagging"}
    Skill_Execution: execute_workflow()
    Assertion: Populated template contains "Branch: feature/issue-123"
    Pass_Criteria: ✅ Context collection working

INTEGRATION_TESTS:
  Focus: "Command → Skill → gh CLI end-to-end"
  Scope: "Full workflow from user input to GitHub issue creation"
  Method: "Execute command, verify GitHub state"

  Example_Test:
    Test_Name: "End-to-end issue creation"
    Input: /create-issue feature "Integration test" --label testing
    Execution: Command → Skill → gh CLI
    Assertion: Issue exists in GitHub with correct metadata
    Cleanup: gh issue delete <created-issue-id>
    Pass_Criteria: ✅ Full workflow functional
```

**Testing Benefits:**

```yaml
ISOLATION:
  - Command tests don't depend on skill implementation
  - Skill tests don't depend on command parsing
  - Integration tests verify end-to-end flow

SPEED:
  - Command tests: <1 second (no external calls)
  - Skill tests: 1-3 seconds (mock gh CLI)
  - Integration tests: 3-5 seconds (real gh CLI)

DEBUGGING:
  - Command failure → Check parsing/validation
  - Skill failure → Check workflow phases
  - Integration failure → Check gh CLI integration
```

---

### 5.2 Anti-Patterns

#### Anti-Pattern 1: Embedding Business Logic in Commands

**Problem:**

```bash
#!/bin/bash
# BAD: Command contains template population logic

type="$1"
title="$2"

# Business logic embedded (WRONG LAYER)
template_file="Docs/Templates/issue-${type}.md"
template=$(cat "$template_file")

current_branch=$(git rev-parse --abbrev-ref HEAD)
populated="${template//\{\{TITLE\}\}/$title}"
populated="${populated//\{\{BRANCH\}\}/$current_branch}"

gh issue create --title "$title" --body "$populated"
```

**Why Bad:**
- ❌ **No Reusability:** Template logic locked in command
- ❌ **Testing Hard:** Can't test template population independently
- ❌ **Maintenance:** Template changes require command updates

**Fix:**

```bash
#!/bin/bash
# GOOD: Command delegates to skill

type="$1"
title="$2"

# Delegate to skill
claude load-skill github-issue-creation

# Skill handles:
# - Template loading
# - Context collection
# - Placeholder population
# - gh CLI execution

# Command only formats output
echo "✅ Issue Created: #456 \"$title\""
```

---

#### Anti-Pattern 2: Duplicating Skill Logic Across Commands

**Problem:**

```bash
# Command 1: /create-issue
template_file="Docs/Templates/issue-${type}.md"
template=$(cat "$template_file")
# ... 200 lines of template logic ...

# Command 2: /clone-issue
template_file="Docs/Templates/issue-${type}.md"
template=$(cat "$template_file")
# ... 200 lines of DUPLICATED template logic ...

# Agent: TestEngineer
template_file="Docs/Templates/issue-${type}.md"
template=$(cat "$template_file")
# ... 200 lines of DUPLICATED template logic ...
```

**Why Bad:**
- ❌ **Duplication:** 3 consumers × 200 lines = 600 lines duplicated
- ❌ **Inconsistency Risk:** Bug fix in one consumer missed in others
- ❌ **Maintenance:** 3× the work for every template change

**Fix:**

```yaml
SKILL: github-issue-creation (200 lines, single source of truth)

CONSUMERS:
  /create-issue: Delegates to skill (70 lines)
  /clone-issue: Delegates to skill (70 lines)
  TestEngineer: Delegates to skill (agent code)

Total: 200 lines skill + 140 lines consumers = 340 lines (43% reduction)
Maintenance: Bug fix changes 1 file (skill)
```

---

#### Anti-Pattern 3: Tight Coupling Between Command and Skill

**Problem:**

```bash
#!/bin/bash
# BAD: Command tightly coupled to skill internals

type="$1"
title="$2"

# Command KNOWS skill internal structure (WRONG)
skill_template_path=".claude/skills/github/github-issue-creation/resources/templates/issue-${type}.md"
skill_label_validation=".claude/skills/github/github-issue-creation/resources/docs/label-validation.md"

# Command accesses skill resources directly (WRONG)
if [ ! -f "$skill_template_path" ]; then
  echo "⚠️ Template not found in skill"
  exit 1
fi

# Command calls skill internal functions (WRONG)
source .claude/skills/github/github-issue-creation/SKILL.md
validate_labels "${labels[@]}" # Direct function call
```

**Why Bad:**
- ❌ **Tight Coupling:** Command breaks if skill structure changes
- ❌ **Encapsulation Violation:** Command accessing skill internals
- ❌ **Skill Evolution Blocked:** Can't refactor skill without breaking command

**Fix:**

```bash
#!/bin/bash
# GOOD: Command delegates through clean API

type="$1"
title="$2"

# Command loads skill by name (loose coupling)
claude load-skill github-issue-creation

# Skill exposes clean API
# Command doesn't know internal structure
# Skill handles template paths, validation, etc.

# Command only formats skill results
echo "✅ Issue Created"
```

**Key Principle:** Commands should treat skills as **black boxes** with defined APIs, not peek into internals.

---

#### Anti-Pattern 4: No Error Handling at Skill Layer

**Problem:**

```yaml
SKILL: github-issue-creation

WORKFLOW:
  Phase_1: Collect context (NO ERROR HANDLING)
    - git rev-parse → Fails if not in git repo
    - No error return to command

  Phase_2: Load template (NO ERROR HANDLING)
    - cat template_file → Fails if file missing
    - No error return to command

  Phase_3: Create issue (NO ERROR HANDLING)
    - gh issue create → Fails if unauthenticated
    - No error return to command

Result:
  - Command receives undefined/null from skill
  - Command has no context for what failed
  - User sees "undefined" error (TERRIBLE UX)
```

**Why Bad:**
- ❌ **Silent Failures:** Skill errors don't propagate
- ❌ **Poor UX:** User sees cryptic errors
- ❌ **No Recovery:** Command can't handle errors it doesn't know about

**Fix:**

```yaml
SKILL: github-issue-creation

WORKFLOW:
  Phase_1: Collect context
    current_branch=$(git rev-parse --abbrev-ref HEAD 2>/dev/null)
    if [ -z "$current_branch" ]; then
      return_error "not_in_git_repo" "Current directory is not a git repository"
    fi

  Phase_2: Load template
    if [ ! -f "$template_file" ]; then
      available_templates=$(ls Docs/Templates/issue-*.md | sed 's/.*\///')
      return_error "template_not_found" "Template $template_file not found. Available: $available_templates"
    fi

  Phase_3: Create issue
    gh issue create --title "$title" --body "$body" 2>&1
    if [ $? -ne 0 ]; then
      return_error "gh_cli_failed" "gh issue create failed. Check authentication: gh auth status"
    fi

Error_Return_Format:
  {
    "status": "error",
    "category": "template_not_found",
    "message": "Template not found",
    "recovery": "Available templates: issue-feature.md, issue-bug.md",
    "command_suggestion": "/create-issue feature \"Title\" --template issue-feature.md"
  }

Command_Receives_Structured_Error:
  - Knows what failed (category)
  - Knows why (message)
  - Knows how to fix (recovery, command_suggestion)
  - Displays user-friendly error
```

**Key Principle:** Skills must **catch all errors**, enrich with context, and return structured error objects to commands.

---

## SUMMARY

### Command-Skill Separation Checklist ✅

When designing command-skill integration:

#### Clear Boundaries ✅
- [ ] Command handles: Argument parsing, validation, UX, output formatting
- [ ] Skill handles: Business logic, context collection, external integrations
- [ ] No business logic in command (< 100 lines)
- [ ] No argument parsing in skill

#### Integration Pattern Selection ✅
- [ ] Direct skill loading for production skills
- [ ] Task tool for prototyping or debugging
- [ ] Multi-skill orchestration for complex workflows
- [ ] Workflow trigger for GitHub Actions integration

#### Argument Flow ✅
- [ ] Command validates syntax and types
- [ ] Skill validates business logic
- [ ] Clear argument mapping (command args → skill params)
- [ ] Structured error objects returned from skill

#### Best Practices ✅
- [ ] Commands stay thin (70-150 lines)
- [ ] Skills extract complex logic (100+ lines)
- [ ] Skills enable reusability (2+ consumers)
- [ ] Independent testing (command tests, skill tests, integration tests)

#### Anti-Pattern Avoidance ✅
- [ ] No business logic in commands
- [ ] No skill logic duplication across commands
- [ ] No tight coupling (command accesses skill internals)
- [ ] Comprehensive error handling at skill layer

---

**Next:** See `argument-handling-guide.md` for robust argument parsing strategies, validation patterns, and error handling excellence.
