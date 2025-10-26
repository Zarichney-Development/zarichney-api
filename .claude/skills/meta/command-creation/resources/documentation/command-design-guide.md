# Command Design Guide: Architecture, Philosophy & Anti-Bloat Framework

**Version:** 1.0.0
**Category:** Meta-Skill Resource
**Purpose:** Comprehensive command architecture guidance providing decision frameworks for when to create commands, UX design principles, and anti-bloat validation preventing command ecosystem pollution

---

## TABLE OF CONTENTS

1. [Command Architecture & Philosophy](#1-command-architecture--philosophy)
2. [Command vs. Skill Decision Framework](#2-command-vs-skill-decision-framework)
3. [UX Design Principles](#3-ux-design-principles)
4. [Anti-Bloat Framework](#4-anti-bloat-framework)
5. [Command Lifecycle Management](#5-command-lifecycle-management)

---

## 1. COMMAND ARCHITECTURE & PHILOSOPHY

### 1.1 What is a Command?

**Definition:** A command is a user-facing CLI interface providing streamlined access to complex workflows, tool orchestration, or frequently-used operations within the Claude Code ecosystem.

**Core Responsibilities:**
- **Argument Parsing:** Transform user input into validated, typed parameters
- **Validation:** Ensure inputs meet constraints before workflow execution
- **User Experience:** Provide intuitive interface with helpful errors and progress feedback
- **Skill Delegation:** Invoke business logic from reusable skills when appropriate
- **Output Formatting:** Present results in readable, actionable format
- **Error Handling:** Translate technical failures into user-friendly guidance

**What Commands Are NOT:**
- ❌ **Business Logic Containers:** Complex workflows belong in skills
- ❌ **Simple Aliases:** Commands must provide orchestration value beyond direct CLI wrapping
- ❌ **Data Storage:** Commands are stateless, ephemeral execution interfaces
- ❌ **Service Implementations:** Long-running processes belong elsewhere

---

### 1.2 Command Anatomy

**Structural Components:**

```yaml
FRONTMATTER:
  description: "One-sentence purpose with primary use case"
  argument-hint: "[optional] <required> [--named VALUE] [--flag]"
  category: "testing|security|architecture|workflow|cicd"
  requires-skills: ["skill-name"] # Optional

DOCUMENTATION_SECTIONS:
  Purpose: "Why this command exists, problem solved, target users"
  Usage_Examples: "5-10 realistic scenarios with expected outputs"
  Arguments: "Comprehensive specifications: positional, named, flags"
  Output: "Success, dry-run, error patterns with examples"
  Integration: "Skills, tools, workflows, system dependencies"
  Error_Handling: "Common errors with troubleshooting guidance"
```

**Execution Flow:**
1. **Parse Arguments:** Extract positional, named, flags from user input
2. **Validate Constraints:** Type checking, range validation, dependency existence
3. **Execute Workflow:** Direct CLI invocation OR skill delegation
4. **Format Results:** Transform technical output into user-friendly presentation
5. **Provide Next Steps:** Contextual guidance for follow-up actions

---

### 1.3 Command Categories

#### Testing Commands
**Purpose:** Test execution, coverage analysis, quality reporting

**Examples:**
- `/test-report` - Comprehensive test suite execution with AI-powered analysis
- `/coverage-report` - Coverage metrics and improvement recommendations
- `/run-tests <category>` - Targeted test execution (unit, integration, e2e)

**Characteristics:**
- High-frequency usage during development
- Integration with testing frameworks (dotnet test, pytest, jest)
- AI analysis for quality insights
- Coverage tracking for continuous excellence goals

---

#### Security Commands
**Purpose:** Vulnerability scanning, compliance checks, security hardening

**Examples:**
- `/security-scan` - OWASP compliance and vulnerability assessment
- `/audit-dependencies` - Dependency vulnerability analysis
- `/check-secrets` - Secret detection and prevention

**Characteristics:**
- Pre-PR and pre-deployment execution
- Integration with security tools (OWASP ZAP, dependency-check)
- Critical findings block progression
- Compliance framework integration

---

#### Architecture Commands
**Purpose:** System design decisions, technical debt analysis, architecture review

**Examples:**
- `/analyze-debt` - Technical debt identification and prioritization
- `/architecture-review` - Design pattern compliance and structural assessment
- `/dependency-graph` - Visualize module dependencies and coupling

**Characteristics:**
- Periodic execution (weekly, per epic)
- AI-powered pattern recognition
- Decision support matrices
- Educational reinforcement for architectural principles

---

#### Workflow Commands
**Purpose:** GitHub automation, issue management, PR orchestration

**Examples:**
- `/create-issue <type> <title>` - Comprehensive issue creation with context collection
- `/workflow-status [workflow]` - CI/CD monitoring and failure debugging
- `/merge-coverage-prs` - Multi-PR consolidation with AI conflict resolution

**Characteristics:**
- GitHub CLI (`gh`) integration
- Context-aware (current branch, recent commits, related issues)
- Dry-run support for safety
- Real-time monitoring for long-running workflows

---

#### CI/CD Commands
**Purpose:** Pipeline automation, deployment orchestration, continuous integration support

**Examples:**
- `/deploy <environment>` - Controlled deployment with rollback support
- `/pipeline-status` - CI/CD health monitoring
- `/trigger-workflow <workflow>` - Manual workflow invocation with parameters

**Characteristics:**
- GitHub Actions integration
- Multi-environment support (dev, staging, prod)
- Deployment gates and approval workflows
- Automated rollback on failure

---

### 1.4 Command Lifecycle Phases

#### Phase 1: Design & Scoping
**Activities:**
- Validate orchestration value (anti-bloat framework)
- Define command-skill boundaries
- Specify argument requirements
- Identify UX consistency patterns

**Deliverables:**
- Scope definition document (from `command-scope-template.md`)
- Argument specification with types, defaults, validation
- Skill dependency assessment

---

#### Phase 2: Implementation
**Activities:**
- Apply command structure template
- Implement argument parsing with validation
- Integrate with skills or CLI tools
- Design error handling and output formatting

**Deliverables:**
- Complete command markdown (`.claude/commands/command-name.md`)
- Bash implementation with robust parsing
- Comprehensive usage examples

---

#### Phase 3: Testing & Validation
**Activities:**
- Test all argument combinations (valid and invalid)
- Verify error message clarity and actionability
- Validate skill integration (if applicable)
- Ensure output formatting consistency

**Deliverables:**
- Test execution results (all scenarios passed)
- Error message validation (user comprehension confirmed)
- Integration validation (skill communication works)

---

#### Phase 4: Documentation & Training
**Activities:**
- Document command in project README
- Create training examples for team
- Add to command catalog with priority and value proposition
- Update related documentation (workflow guides, standards)

**Deliverables:**
- Command catalog entry
- Training materials and examples
- Updated documentation cross-references

---

#### Phase 5: Maintenance & Evolution
**Activities:**
- Monitor usage patterns and user feedback
- Enhance based on emerging requirements
- Refactor for consistency with new patterns
- Deprecate if no longer providing value

**Deliverables:**
- Usage analytics (frequency, success rate, common errors)
- Enhancement roadmap
- Deprecation notices (if applicable)

---

## 2. COMMAND VS. SKILL DECISION FRAMEWORK

### 2.1 When to Create a Command

**Decision Criteria:**

```yaml
CREATE_COMMAND_WHEN:
  Multi_Step_Workflow:
    Indicator: "Workflow requires 3+ distinct steps or tool coordination"
    Example: "/create-issue - Parse args, collect context, load template, populate, validate labels, create issue"
    Value: "Orchestrates complex workflow into single command"

  Complex_CLI_Operation:
    Indicator: "gh CLI or git operation requires multiple flags or complex syntax"
    Example: "/workflow-status - Combines gh run list, view, log with filtering and formatting"
    Value: "Simplifies gh CLI complexity with smart defaults"

  Skill_Delegation:
    Indicator: "Reusable business logic exists or should be extracted"
    Example: "/create-issue → github-issue-creation skill"
    Value: "Provides user-friendly interface to skill workflows"

  Workflow_Trigger:
    Indicator: "GitHub Actions workflow needs manual trigger with configurable parameters"
    Example: "/merge-coverage-prs --dry-run --max 15"
    Value: "Simplifies workflow invocation, monitoring, result retrieval"

  Frequent_Operation:
    Indicator: "Operation performed 5+ times per week saving 2+ minutes each time"
    Example: "/coverage-report - Eliminates manual test execution and report generation"
    Value: "15+ min/week saved per developer"
```

**Orchestration Value Threshold:**

Commands must provide **meaningful value beyond simple CLI wrapping**. This means:
- **Argument Orchestration:** Combining multiple gh CLI patterns into intuitive interface
- **Intelligent Defaults:** Smart defaults reducing required user input (e.g., current branch detection)
- **Output Enhancement:** Status indicators, formatted timestamps, actionable next steps
- **Context Integration:** Automatic context collection (branch, commits, related issues)
- **Error Improvement:** User-friendly errors with troubleshooting guidance

**Minimum Value Bar:** Command must save **1+ minute per invocation** OR eliminate **significant cognitive overhead** (e.g., no context switching to GitHub UI).

---

### 2.2 When NOT to Create a Command

**Anti-Patterns:**

```yaml
DO_NOT_CREATE_WHEN:
  Simple_CLI_Wrapper:
    Example: "/pr-create → just calls gh pr create with no added value"
    Problem: "No orchestration, no defaults, no output enhancement"
    Alternative: "Use gh CLI directly or create alias"

  Single_Use_Operation:
    Example: "/initialize-project (used once during setup)"
    Problem: "No recurring value, maintenance overhead exceeds benefit"
    Alternative: "Document manual steps in README"

  No_Argument_Validation:
    Example: "/deploy <env> (no validation, just passes to script)"
    Problem: "No safety, no error prevention, just indirection"
    Alternative: "Call deployment script directly"

  Redundant_Functionality:
    Example: "/status (duplicates /workflow-status)"
    Problem: "Command bloat, user confusion over which to use"
    Alternative: "Enhance existing command with additional arguments"
```

**Red Flags Indicating Poor Command Candidates:**
- ❌ **Less than 3 invocations per month** across all team members
- ❌ **No argument validation or output formatting** (pure pass-through)
- ❌ **Duplicates existing command** functionality
- ❌ **Saves less than 30 seconds** per invocation
- ❌ **No error improvement** over direct CLI usage

---

### 2.3 Command vs. Skill Separation

**When to Extract Skill:**

```yaml
SKILL_EXTRACTION_CRITERIA:
  Reusability_Threshold:
    Trigger: "2+ commands need same business logic OR other agents will use workflow"
    Example: "github-issue-creation skill used by /create-issue, /clone-issue, TestEngineer agent"
    Action: "Extract shared patterns into skill"

  Complexity_Threshold:
    Trigger: "Workflow logic exceeds 100 lines OR 4+ distinct phases"
    Example: "Issue creation: context collection, template management, population, validation"
    Action: "Separate business logic for testability and maintainability"

  Resource_Management:
    Trigger: "Need templates, examples, documentation bundles"
    Example: "5 issue templates, label validation rules, context collection guides"
    Action: "Create skill with resources/ for resource bundles"

  Testing_Requirements:
    Trigger: "Business logic requires unit testing independent from CLI interface"
    Example: "Template population logic with placeholder validation"
    Action: "Extract to skill for isolated testing"
```

**Decision Tree:**

```
User Request → Command Needed?
  ├─ YES → Workflow Complexity?
  │   ├─ SIMPLE (1-3 steps) → Direct CLI Implementation
  │   │   └─ Example: /workflow-status (gh run list + formatting)
  │   │
  │   └─ COMPLEX (4+ steps, business logic) → Skill Required?
  │       ├─ REUSABLE (2+ consumers) → Skill Integration Pattern
  │       │   └─ Example: /create-issue → github-issue-creation skill
  │       │
  │       └─ NOT REUSABLE → Direct Implementation (monitor for reuse)
  │           └─ Example: Single-use workflow (but keep modular for future extraction)
  │
  └─ NO → Use gh CLI directly or document manual steps
```

---

### 2.4 Skill Integration Patterns

**Pattern 1: Direct Skill Delegation (Preferred for Complex Workflows)**

```yaml
PATTERN: Direct_Skill_Delegation
USE_WHEN: "Complex multi-step workflow with reusable business logic"
EXAMPLE: "/create-issue → github-issue-creation skill"

WORKFLOW:
  Command_Layer:
    - Parse arguments: type, title, labels, milestone, dry-run
    - Validate argument syntax: type enum, title length
    - Load skill: github-issue-creation
    - Pass validated args to skill
    - Format skill output for user display

  Skill_Layer:
    - Collect context: branch, commits, related issues
    - Load template: map type → issue-{type}.md
    - Populate template with collected context
    - Validate labels against GitHubLabelStandards.md
    - Create issue via gh CLI or preview (dry-run)
    - Return structured results

  Result: "Command stays thin (70 lines), skill handles complexity (200 lines)"
```

**Pattern 2: CLI Wrapper with Optional Skill Enhancement**

```yaml
PATTERN: CLI_Wrapper_with_Optional_Enhancement
USE_WHEN: "Simple CLI operation, optional output enhancement or analysis"
EXAMPLE: "/workflow-status → gh CLI + optional cicd-monitoring skill"

WORKFLOW:
  Command_Layer:
    - Parse arguments: workflow, limit, branch, details
    - Execute gh CLI: gh run list with filters
    - Optional: Load cicd-monitoring skill for trend analysis
    - Format results: status emojis, relative timestamps
    - Present output with next steps

  Skill_Layer_Optional:
    - Parse gh CLI output
    - Trend analysis: success rate over time
    - Failure pattern detection
    - Rich visualization with insights

  Result: "Command provides immediate value (gh CLI wrapper), skill adds optional depth (trend analysis)"
```

**Pattern 3: Workflow Trigger (Minimal or No Skill)**

```yaml
PATTERN: Workflow_Trigger
USE_WHEN: "GitHub Actions workflow exists, command provides invocation interface"
EXAMPLE: "/merge-coverage-prs → trigger workflow + monitor + report"

WORKFLOW:
  Command_Layer:
    - Parse arguments: dry-run, max, labels
    - Map arguments to workflow inputs
    - Execute: gh workflow run coverage-excellence-merge-orchestrator
    - Monitor: gh run watch <run-id>
    - Report status: success/failure with URL

  Skill_Layer_Optional:
    - Workflow validation: verify workflow exists
    - Argument mapping: command args → workflow inputs
    - Post-execution analysis: conflict summary, PR links

  Result: "Command provides UI, workflow contains orchestration logic"
```

---

## 3. UX DESIGN PRINCIPLES

### 3.1 Consistency Patterns

#### 3.1.1 Emoji Usage Standards

**Purpose:** Visual consistency and quick status recognition across all commands

**Standard Emoji Patterns:**

```yaml
PROGRESS_INDICATORS:
  🔄: "Operation in progress (loading, fetching, executing)"
  ⏳: "Long-running operation (multi-minute workflows)"
  📊: "Data analysis operation (processing, analyzing)"

  Examples:
    - "🔄 Fetching workflow runs..."
    - "⏳ Consolidating 8 PRs... (may take 3-5 minutes)"
    - "📊 Analyzing test coverage patterns..."

SUCCESS_CONFIRMATIONS:
  ✅: "Primary success indicator (operation completed)"
  🎯: "Success with specific achievement (target met)"
  🚀: "Success launching or deploying (production changes)"

  Examples:
    - "✅ Issue Created: #456 'Add recipe tagging'"
    - "🎯 Coverage Target Achieved: 82% (goal: 80%)"
    - "🚀 Deployed to Staging: v2.1.3"

WARNINGS_AND_ERRORS:
  ⚠️: "Error or warning requiring attention"
  ❌: "Failure indicator (test failed, workflow failed)"
  🔒: "Security concern or permission issue"

  Examples:
    - "⚠️ Invalid Argument: --limit must be 1-50 (got 100)"
    - "❌ Build Failed: Compilation errors detected"
    - "🔒 Permission Denied: Requires repository admin access"

INFORMATIONAL:
  💡: "Tips, suggestions, next steps, educational guidance"
  ℹ️: "General information or status update"
  📋: "Dry-run preview or summary information"

  Examples:
    - "💡 Next Steps: Run /coverage-report to see detailed metrics"
    - "ℹ️ No recent runs found for this workflow"
    - "📋 DRY RUN - Issue Preview (not created)"

STATUS_INDICATORS:
  🔄: "In progress (running workflows)"
  ⏭️: "Skipped or cancelled"
  ⏸️: "Paused or waiting"

  Examples:
    - "🔄 deploy.yml running... (in_progress)"
    - "⏭️ Generate coverage: SKIPPED"
    - "⏸️ Deployment Paused: Awaiting approval"
```

**Emoji Usage Guidelines:**
- **One Emoji Per Line Prefix:** Avoid emoji overload (maximum 1 emoji at line start)
- **Semantic Consistency:** Same emoji always means same thing across all commands
- **Graceful Degradation:** Output remains clear even if terminal doesn't render emojis
- **Accessibility:** Include text descriptions alongside emojis

---

#### 3.1.2 Error Message Format

**Template:** `⚠️ [Category]: [Specific Issue]. [Explanation]. Try: [Example]`

**Components:**
1. **Emoji:** `⚠️` for warnings/errors, `❌` for failures
2. **Category:** Classification (Invalid Argument, Dependency Missing, Execution Failed, Validation Failed)
3. **Specific Issue:** Exact problem with values (e.g., "got 'abc'" instead of "invalid input")
4. **Explanation:** Why this is wrong, what constraint was violated
5. **Actionable Fix:** Specific command to try or steps to resolve

**Examples:**

```
⚠️ Invalid Argument: Issue type must be feature|bug|epic|debt|docs (got 'typo')

Valid types:
• feature - New feature request
• bug - Bug report
• epic - Epic milestone planning
• debt - Technical debt tracking
• docs - Documentation request

Did you mean 'debt'?

Try: /create-issue feature "Add recipe tagging system"
```

**Why Effective:**
- ✅ **Specific:** Shows exact invalid value ('typo')
- ✅ **Educational:** Lists all valid types with descriptions
- ✅ **Helpful:** Suggests closest match ("Did you mean 'debt'?")
- ✅ **Actionable:** Provides correct example command

---

#### 3.1.3 Success Feedback Pattern

**Template:**

```
✅ [Action Completed]: [Result Summary]

Details:
• Key Detail 1: Value
• Key Detail 2: Value

[Reference: URL, ID, or Path]

💡 Next Steps:
- [Most important follow-up action]
- [Secondary follow-up action]
```

**Example:**

```
✅ Issue Created: #456 "Add recipe tagging system"

Details:
• Type: feature
• Labels: type: feature, frontend, enhancement
• Milestone: Epic #291
• Context: Branch feature/issue-455, 5 commits, 2 related issues

https://github.com/Zarichney-Development/zarichney-api/issues/456

💡 Next Steps:
- Refine acceptance criteria in issue description
- Begin implementation: git checkout -b feature/issue-456
- Assign to team member: gh issue edit 456 --add-assignee @user
```

**Components:**
1. **Success Confirmation:** ✅ emoji + action + summary (issue number and title)
2. **Details Section:** Bulleted metadata (type, labels, milestone, context)
3. **Reference:** Direct link to created resource (clickable URL)
4. **Next Steps:** 2-4 actionable suggestions prioritized by importance

---

#### 3.1.4 Argument Hint Format

**Conventions:**

```yaml
POSITIONAL_REQUIRED:
  Format: "<argument>"
  Example: "<type> <title>"

POSITIONAL_OPTIONAL:
  Format: "[argument]"
  Example: "[workflow-name]"

NAMED_OPTIONAL:
  Format: "[--name VALUE]"
  Example: "[--label LABEL] [--milestone MILESTONE]"

FLAGS:
  Format: "[--flag]"
  Example: "[--dry-run] [--details]"

COMBINED:
  Format: "<required> [optional] [--named VALUE] [--flag]"
  Example: "<type> <title> [--template TEMPLATE] [--label LABEL] [--dry-run]"
```

**Guidelines:**
- **Order:** Positional required → Positional optional → Named optional → Flags
- **Value Placeholders:** Use UPPERCASE for value placeholders (TEMPLATE, LABEL, N)
- **Repeatable Arguments:** Indicate in description, not hint (e.g., `--label` is repeatable)
- **Conciseness:** Show 2-3 most common options, omit rarely-used arguments

---

### 3.2 User-Friendly Defaults

**Principles:**

```yaml
SAFE_BY_DEFAULT:
  Principle: "Defaults should never be destructive or irreversible"
  Examples:
    - dry_run: false (for read operations like /workflow-status)
    - dry_run: true (for write operations like /deploy - preview first)
  Why: "Prevents accidental data loss or production changes"

COMMON_USE_CASE:
  Principle: "Optimize defaults for 80% of invocations"
  Examples:
    - limit: 5 (balances context vs. overwhelming output)
    - branch: "" (auto-detect current branch for contextual relevance)
  Why: "Minimize required user input for routine operations"

EXPLICIT_OVERRIDE:
  Principle: "Make defaults obvious and easy to override"
  Examples:
    - workflow_name: "" (empty = all workflows, document in help)
    - template: null (auto-select based on type, override with --template)
  Why: "Users understand what's happening and can customize easily"
```

**Examples:**

```yaml
/create-issue:
  template: "" # Auto-select based on type (feature → issue-feature.md)
  labels: []   # Type label auto-applied (type: feature)
  dry_run: false # Most usage is direct creation

  Rationale:
    - Template: "Type determines template 95% of the time"
    - Labels: "Type label always needed, user labels supplement"
    - Dry-run: "Preview less common, optimize for direct creation"

/workflow-status:
  limit: 5       # Recent context without overwhelming output
  branch: ""     # Auto-detect current branch
  details: false # Summary sufficient for 90% of checks

  Rationale:
    - Limit: "5 runs shows trends without scrolling"
    - Branch: "Contextual to developer's active work"
    - Details: "Quick check most common, opt-in verbosity"

/merge-coverage-prs:
  dry_run: true  # Preview before consolidating (SAFE for write ops)
  max: 8         # Reasonable batch size
  labels: "type: coverage,coverage,testing" # Epic labels

  Rationale:
    - Dry-run: "Destructive operation requires preview by default"
    - Max: "8 PRs balances efficiency vs. conflict complexity"
    - Labels: "Coverage epic has consistent label patterns"
```

---

### 3.3 Progressive Disclosure

**Concept:** Present information complexity progressively, from simple to advanced

**Levels:**

```yaml
LEVEL_1_BASIC_USAGE:
  Target: "First-time users, quick status checks"
  Example: "/workflow-status"
  Output: "Simple table, 5 runs, status indicators"

LEVEL_2_FILTERED_USAGE:
  Target: "Users familiar with command, need specific view"
  Example: "/workflow-status build.yml --limit 10"
  Output: "Filtered results, expanded history"

LEVEL_3_ADVANCED_USAGE:
  Target: "Power users, debugging, customization"
  Example: "/workflow-status testing-coverage.yml --details --branch epic"
  Output: "Full logs, job breakdown, error excerpts"

LEVEL_4_EXPERT_USAGE:
  Target: "Automation, scripting, integration"
  Example: "/workflow-status --json --limit 50"
  Output: "Machine-readable JSON for parsing"
```

**Usage Examples Design:**

Structure examples to flow from simplest to most complex:

1. **Example 1: Zero Arguments (Most Common - 60%+ usage)**
   - Show default behavior with no options
   - Demonstrate quickest invocation

2. **Example 2: Single Argument (Common - 30% usage)**
   - Add one argument (workflow filter, label, etc.)
   - Show incremental customization

3. **Example 3: Multiple Arguments (Advanced - 8% usage)**
   - Combine arguments for specific use case
   - Demonstrate full customization

4. **Example 4: Dry-Run or Preview (Safety - 2% usage)**
   - Show preview mode preventing mistakes
   - Educate on safety features

5. **Example 5: Edge Case (Rare but instructive)**
   - Handle unusual scenario (zero results, error recovery)
   - Demonstrate error handling quality

---

### 3.4 Actionable Output

**Principles:**

```yaml
ALWAYS_PROVIDE_NEXT_STEPS:
  Requirement: "Every success message must include 2-4 actionable next steps"
  Format: "💡 Next Steps:\n- [Action 1]\n- [Action 2]"
  Why: "Users shouldn't wonder 'what now?'"

CONTEXTUAL_SUGGESTIONS:
  Requirement: "Next steps should be context-aware based on results"
  Examples:
    - Failures detected: "Investigate with --details"
    - All success: "Proceed to deployment"
    - In progress: "Monitor with /workflow-status"
  Why: "Guidance tailored to current state"

CLICKABLE_REFERENCES:
  Requirement: "Include URLs, IDs, or paths for quick navigation"
  Examples:
    - Issue URL: "https://github.com/.../issues/456"
    - Run ID: "Run #123456"
    - File path: "./Code/Zarichney.Server/..."
  Why: "Single click to deep-dive"
```

**Examples:**

```yaml
Success_with_Failures_Detected:
  Message: "✅ Workflow Status Retrieved"

  Output:
    - ✅ build.yml: success
    - ❌ testing-coverage.yml: FAILURE
    - 🔄 deploy.yml: in_progress

  Next_Steps:
    - "💡 Investigate failure: /workflow-status testing-coverage.yml --details"
    - "💡 Monitor deployment: gh run watch <run-id>"
    - "💡 View logs: gh run view <run-id> --log"

  Why: "Prioritizes investigation of failures"

Success_All_Green:
  Message: "✅ All Workflows Successful"

  Output:
    - ✅ build.yml: success (2m 34s)
    - ✅ testing-coverage.yml: success (5m 12s)
    - ✅ lint-and-format.yml: success (54s)

  Next_Steps:
    - "💡 Create PR: gh pr create"
    - "💡 View coverage: /coverage-report"
    - "💡 Deploy to staging: /deploy staging"

  Why: "Guides toward next development phase"
```

---

### 3.5 Help Text Quality

**Requirements:**

```yaml
CLEAR_USAGE_PATTERN:
  Format: "Usage: /command <required> [optional] [--named VALUE] [--flag]"
  Example: "Usage: /create-issue <type> <title> [--template TEMPLATE] [--label LABEL] [--dry-run]"
  Why: "Users immediately understand syntax"

COMPREHENSIVE_ARGUMENT_LIST:
  Required_Section:
    Format: "Required:\n  <arg>  Description"
    Example: "Required:\n  <type>   Issue type (feature|bug|epic|debt|docs)\n  <title>  Clear, actionable title"

  Optional_Section:
    Format: "Optional:\n  --arg VALUE  Description (default: VALUE)"
    Example: "Optional:\n  --limit N  Number of runs (default: 5)"

MULTIPLE_USAGE_EXAMPLES:
  Requirement: "Provide 3-5 realistic examples covering common scenarios"
  Examples:
    - Basic: "/create-issue feature \"Add tagging\""
    - With labels: "/create-issue feature \"Add tagging\" --label frontend"
    - Full: "/create-issue epic \"API v2\" --milestone \"Q1 2025\" --assignee @user"
  Why: "Examples show real usage patterns"
```

---

## 4. ANTI-BLOAT FRAMEWORK

### 4.1 Orchestration Value Test

**Question:** Does this command provide value BEYOND simple CLI wrapping?

**Value Categories:**

```yaml
1_ARGUMENT_ORCHESTRATION:
  Test: "Does command combine multiple CLI patterns into simpler interface?"
  Example: "/workflow-status combines gh run list, view, log with filtering"
  Value: "User doesn't need to remember gh CLI syntax variations"

2_INTELLIGENT_DEFAULTS:
  Test: "Does command provide smart defaults reducing required input?"
  Example: "/workflow-status defaults: limit=5, branch=current"
  Value: "Zero-argument invocation optimized for common use case"

3_OUTPUT_ENHANCEMENT:
  Test: "Does command improve output beyond raw CLI response?"
  Example: "Status emojis (✅ ❌ 🔄), relative timestamps ('3 min ago')"
  Value: "At-a-glance status understanding"

4_CONTEXT_INTEGRATION:
  Test: "Does command automatically collect relevant project context?"
  Example: "/create-issue collects branch, commits, related issues"
  Value: "Eliminates 2-3 minutes of manual context gathering"

5_ERROR_IMPROVEMENT:
  Test: "Does command provide better errors than underlying CLI?"
  Example: "Fuzzy matching for invalid workflow name suggestions"
  Value: "90% of errors self-resolve without escalation"
```

**Minimum Threshold:** Command must provide value in **2+ categories** to justify creation.

---

### 4.2 Reusability Assessment

**Question:** If this workflow is reusable, should it be a SKILL instead of command?

**Decision Matrix:**

```yaml
EXTRACT_TO_SKILL_WHEN:
  Multiple_Consumers:
    Trigger: "2+ commands OR other agents need same workflow logic"
    Example: "github-issue-creation skill used by /create-issue, /clone-issue, TestEngineer"
    Action: "Create skill, commands delegate to it"

  Complex_Business_Logic:
    Trigger: "Workflow exceeds 100 lines OR 4+ distinct phases"
    Example: "Issue creation: context, template, population, validation"
    Action: "Separate logic into testable skill"

  Resource_Management:
    Trigger: "Need templates, examples, documentation bundles"
    Example: "5 issue templates, label validation rules"
    Action: "Create skill with resources/ directory"

KEEP_IN_COMMAND_WHEN:
  Single_Consumer:
    Indicator: "Only this command needs workflow logic"
    Example: "/workflow-status gh CLI wrapping"
    Action: "Direct implementation, monitor for reuse opportunity"

  Simple_Workflow:
    Indicator: "Less than 100 lines, 1-3 steps"
    Example: "Parse args → Execute gh CLI → Format output"
    Action: "Command contains logic directly"
```

---

### 4.3 UX Improvement Threshold

**Question:** Does this command save meaningful time or simplify complex workflow?

**Time Savings Threshold:**

```yaml
MINIMUM_TIME_SAVINGS:
  Per_Invocation: "1+ minute saved compared to manual process"
  Frequency: "3+ invocations per week per developer"
  Total_Savings: "3+ minutes/week per developer"

CALCULATION_EXAMPLE:
  Command: "/create-issue"
  Manual_Process: "5 minutes (navigate UI, gather context, fill template)"
  With_Command: "1 minute (type command, automated context)"
  Savings: "4 minutes per issue"
  Frequency: "3 issues/week"
  Total: "12 minutes/week per developer"
  Verdict: "✅ JUSTIFIED - Significant time savings"
```

**Cognitive Overhead Threshold:**

```yaml
CONTEXT_SWITCHING:
  Test: "Does command eliminate browser context switching?"
  Example: "/workflow-status keeps developer in terminal"
  Value: "Maintains flow state during active development"

SYNTAX_COMPLEXITY:
  Test: "Does command simplify gh CLI syntax remembering?"
  Example: "/workflow-status vs. gh run list --workflow X --branch Y --limit Z"
  Value: "Don't need to look up gh CLI docs every time"
```

**Minimum Threshold:** Command must save **1+ minute per invocation** OR eliminate **significant cognitive overhead**.

---

### 4.4 Decision Criteria Examples

#### Example 1: CREATE vs. REJECT Decision

**Candidate:** `/pr-create`

**Proposed Functionality:**
```bash
/pr-create --title "PR Title" --body "Description"
# Calls: gh pr create --title "$title" --body "$body"
```

**Orchestration Value Test:**
```yaml
Argument_Orchestration: ❌ "No - just passes args to gh pr create"
Intelligent_Defaults: ❌ "No - no defaults, all manual input"
Output_Enhancement: ❌ "No - returns raw gh CLI output"
Context_Integration: ❌ "No - doesn't collect context"
Error_Improvement: ❌ "No - uses gh CLI errors directly"

Value_Categories: 0/5
Threshold: FAIL (need 2+)
```

**Reusability Assessment:**
```yaml
Multiple_Consumers: ❌ "No - only /pr-create needs this"
Complex_Logic: ❌ "No - single gh CLI call"
Resource_Management: ❌ "No - no templates needed"

Skill_Extraction: NOT_NEEDED
```

**Time Savings:**
```yaml
Manual: "gh pr create --title \"Title\" --body \"Description\""
With_Command: "/pr-create --title \"Title\" --body \"Description\""
Savings: 0 seconds (identical typing)
Verdict: NO_VALUE_ADDED
```

**Decision:** ❌ **REJECT** - No orchestration value, use gh CLI directly

---

**Alternative with Value:** `/create-pr-with-context`

**Enhanced Functionality:**
```bash
/create-pr-with-context <title>
# Automated:
# 1. Collects recent commits for body
# 2. Finds related issues (links in commits)
# 3. Suggests labels based on files changed
# 4. Auto-assigns reviewers from CODEOWNERS
# 5. Creates PR with comprehensive context
```

**Orchestration Value Test:**
```yaml
Argument_Orchestration: ✅ "Combines commit log, issue search, label detection"
Intelligent_Defaults: ✅ "Auto body, auto reviewers, auto labels"
Output_Enhancement: ✅ "Shows what context was added"
Context_Integration: ✅ "Commits, issues, files changed, CODEOWNERS"
Error_Improvement: ✅ "Validates branch is pushed, conflicts detected"

Value_Categories: 5/5
Threshold: PASS
```

**Time Savings:**
```yaml
Manual:
  1. Git log → copy commits (1 min)
  2. Search issues → find related (1 min)
  3. Check files → determine labels (30s)
  4. Check CODEOWNERS → add reviewers (30s)
  5. gh pr create with all context (1 min)
  Total: 4 minutes

With_Command:
  /create-pr-with-context "Add recipe tagging"
  Total: 30 seconds

Savings: 3.5 minutes per PR
Frequency: 5 PRs/week
Total: 17.5 min/week per developer
Verdict: ✅ HIGH_VALUE
```

**Decision:** ✅ **CREATE** - High orchestration value, significant time savings

---

#### Example 2: SIMPLIFY Recommendation

**Candidate:** `/git-status-enhanced`

**Proposed Functionality:**
```bash
/git-status-enhanced
# Shows git status with color coding and emojis
```

**Analysis:**
```yaml
Orchestration_Value:
  - Output_Enhancement: ✅ "Color coding, emojis"
  - But: "Git already has color output: git status --short --branch"
  - Verdict: "Marginal improvement"

Time_Savings:
  Manual: "git status (2 seconds)"
  With_Command: "/git-status-enhanced (2 seconds)"
  Savings: "0 seconds"

Cognitive_Overhead:
  - No context switching (both terminal)
  - No syntax complexity reduction
  - Verdict: "Minimal benefit"
```

**Recommendation:** ❌ **REJECT** - Use git alias instead

**Alternative:**
```bash
# .gitconfig
[alias]
  s = status --short --branch

# Usage: git s (same brevity, standard git)
```

**Why Better:** Standard git, no custom command maintenance, same UX benefit

---

### 4.5 Anti-Patterns to Avoid

```yaml
PATTERN_1_SIMPLE_WRAPPER:
  Example: "/commit \"message\" → git commit -m \"message\""
  Problem: "No value beyond git CLI, just indirection"
  Fix: "Use git directly or create git alias"

PATTERN_2_REDUNDANT_COMMAND:
  Example: "/status → duplicates /workflow-status"
  Problem: "Command proliferation, user confusion"
  Fix: "Enhance existing command with arguments"

PATTERN_3_OVER_ABSTRACTION:
  Example: "/run \"any command\" → just executes bash command"
  Problem: "Generic wrapper with no specialized value"
  Fix: "Use bash directly, no abstraction needed"

PATTERN_4_ONE_TIME_USE:
  Example: "/setup-project → only used during initial setup"
  Problem: "Maintenance overhead exceeds single use benefit"
  Fix: "Document setup steps in README, manual execution"

PATTERN_5_MISSING_DEFAULTS:
  Example: "/deploy <env> <version> <config> <flags> → all required"
  Problem: "No intelligent defaults, just required input relay"
  Fix: "Add defaults: env=staging, version=latest, config=default"
```

---

## 5. COMMAND LIFECYCLE MANAGEMENT

### 5.1 Creation Phase

**Trigger:** User requirement or identified workflow pain point

**Process:**
1. **Validate Need:** Apply anti-bloat framework (orchestration value, time savings)
2. **Scope Definition:** Define arguments, command-skill boundaries, integration points
3. **Design:** Apply command structure template, plan UX consistency
4. **Implement:** Create command markdown, bash implementation, validation logic
5. **Test:** Validate all argument combinations, error messages, output formatting
6. **Document:** Add to command catalog, update README, create training examples

**Deliverables:**
- `.claude/commands/command-name.md` (complete command specification)
- Bash implementation with robust parsing
- Command catalog entry with priority and value proposition
- Usage examples in documentation

---

### 5.2 Maintenance Phase

**Triggers:**
- User feedback (clarity issues, missing features)
- Usage pattern changes (new common use case emerges)
- Dependency updates (gh CLI version changes)
- Integration opportunities (new skill becomes available)

**Activities:**
1. **Monitor Usage:** Track invocation frequency, success rate, common errors
2. **Analyze Feedback:** Identify enhancement opportunities or pain points
3. **Enhance:** Add arguments, improve errors, integrate with new skills
4. **Refactor:** Update for consistency with new project patterns
5. **Document:** Update usage examples, argument specifications

**Frequency:** Monthly review of high-usage commands, quarterly for low-usage

---

### 5.3 Evolution Criteria

**When to Add Arguments:**

```yaml
NEW_ARGUMENT_JUSTIFIED_WHEN:
  Frequent_Request:
    Trigger: "5+ users request same feature OR 10+ times requested"
    Example: "Add --assignee to /create-issue (requested 15 times)"
    Action: "Add optional named argument with default"

  Coverage_Gap:
    Trigger: "Existing command can't handle valid use case"
    Example: "/workflow-status can't filter by branch"
    Action: "Add --branch argument with current branch default"

  Integration_Opportunity:
    Trigger: "New skill or tool provides additional functionality"
    Example: "cicd-monitoring skill enables trend analysis"
    Action: "Add --trend flag to /workflow-status"
```

**When to Extract to Skill:**

```yaml
SKILL_EXTRACTION_TRIGGERED_WHEN:
  Reuse_Threshold:
    Trigger: "2nd command needs same workflow logic"
    Example: "/clone-issue needs issue creation logic"
    Action: "Extract github-issue-creation skill, refactor both commands"

  Complexity_Threshold:
    Trigger: "Command exceeds 150 lines"
    Example: "/create-issue grows to 200 lines with new features"
    Action: "Extract business logic to skill"

  Testing_Need:
    Trigger: "Business logic requires independent unit testing"
    Example: "Template population logic needs validation testing"
    Action: "Extract to skill with testable API"
```

---

### 5.4 Deprecation Criteria

**When to Deprecate Command:**

```yaml
DEPRECATE_WHEN:
  Low_Usage:
    Trigger: "Less than 3 invocations/month for 3 consecutive months"
    Example: "/setup-project used once during initial setup"
    Action: "Deprecate, document manual steps in README"

  Superseded:
    Trigger: "New command provides same functionality better"
    Example: "/status superseded by /workflow-status"
    Action: "Deprecate old, migrate users to new"

  Tool_Evolution:
    Trigger: "gh CLI now provides native functionality"
    Example: "gh CLI adds native --details flag"
    Action: "Deprecate wrapper, use gh CLI directly"

  Maintenance_Burden:
    Trigger: "Command requires constant updates due to dependency changes"
    Example: "Command breaks with every gh CLI update"
    Action: "Evaluate if value justifies maintenance cost"
```

**Deprecation Process:**

1. **Announce:** Add deprecation notice to command frontmatter
   ```yaml
   deprecated: true
   deprecation_notice: "Use /workflow-status instead. This command will be removed in v2.0."
   alternative: "/workflow-status"
   ```

2. **Warning:** Display deprecation warning on execution
   ```
   ⚠️ DEPRECATED: This command will be removed in v2.0

   Use instead: /workflow-status

   Migration guide: https://github.com/.../migration-guide.md

   [Command executes normally below]
   ```

3. **Grace Period:** Minimum 2 months before removal
4. **Removal:** Delete command file, update documentation, remove catalog entry

---

### 5.5 Version Management

**Semantic Versioning for Commands:**

```yaml
MAJOR_VERSION:
  Trigger: "Breaking change (argument removal, behavior change)"
  Example: "v1 → v2: /deploy requires --environment (was optional)"
  Action: "Deprecate v1, release v2 with migration guide"

MINOR_VERSION:
  Trigger: "New argument or non-breaking feature"
  Example: "v1.0 → v1.1: /create-issue adds --assignee"
  Action: "Update command, backward compatible"

PATCH_VERSION:
  Trigger: "Bug fix or error message improvement"
  Example: "v1.1.0 → v1.1.1: Fix validation edge case"
  Action: "Silent update, no user impact"
```

**Version Tracking:**
- Frontmatter includes `version: "1.2.0"`
- Changelog in command file documenting version history
- Migration guides for major version changes

---

## SUMMARY CHECKLIST

When designing a new command, validate:

### Orchestration Value ✅
- [ ] Provides value in 2+ categories (argument orchestration, defaults, output, context, errors)
- [ ] Saves 1+ minute per invocation OR eliminates cognitive overhead
- [ ] Used 3+ times per week across team

### Command-Skill Separation ✅
- [ ] Workflow complexity assessed (simple = direct, complex = skill)
- [ ] Reusability evaluated (2+ consumers = skill)
- [ ] Resource management considered (templates = skill)

### UX Consistency ✅
- [ ] Emoji usage follows standards (🔄 ✅ ⚠️ 💡)
- [ ] Error messages follow template (Category, Issue, Explanation, Fix)
- [ ] Success feedback includes next steps
- [ ] Argument hint follows conventions

### Anti-Bloat Validation ✅
- [ ] Not a simple CLI wrapper
- [ ] Not redundant with existing command
- [ ] Not a one-time use operation
- [ ] Provides intelligent defaults

### Lifecycle Planning ✅
- [ ] Creation justification documented
- [ ] Maintenance plan defined
- [ ] Evolution criteria identified
- [ ] Deprecation conditions understood

---

**Next:** See `skill-integration-guide.md` for command-skill boundary patterns and delegation strategies.
