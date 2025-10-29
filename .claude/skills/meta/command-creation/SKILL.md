---
name: command-creation
description: Systematic framework for creating slash commands with consistent UX, clear skill integration boundaries, and robust argument handling. Use when PromptEngineer or WorkflowEngineer needs to design slash commands, create command templates, or establish command-skill integration patterns. Ensures commands provide orchestration value beyond simple CLI wrapping while maintaining consistent user experience.
---

# Command Creation Meta-Skill

**Version:** 1.0.0
**Category:** Meta (Tools for PromptEngineer and WorkflowEngineer)
**Target Users:** PromptEngineer (primary), WorkflowEngineer (secondary)

---

## PURPOSE

### Core Mission
Transform command creation from ad-hoc CLI wrapper design into a disciplined 5-phase framework producing user-friendly slash commands with clear skill integration boundaries, robust argument handling, and consistent UX preventing command bloat.

### Why This Matters
Without systematic command creation methodology:
- **Command Bloat:** Redundant CLI wrappers without orchestration value
- **Inconsistent UX:** Different argument patterns, error messages, output formats
- **Unclear Boundaries:** Commands embed business logic instead of delegating to skills
- **Fragile Arguments:** Positional/named/flags/defaults handled inconsistently
- **Poor Errors:** Vague messages preventing independent resolution

This meta-skill ensures commands follow consistent patterns, delegate to skills appropriately, and provide intuitive UX across zarichney-api.

### Mandatory Application
- **Required For:** All new slash command creation by PromptEngineer or WorkflowEngineer
- **Authority:** PromptEngineer EXCLUSIVE over `.claude/commands/`, WorkflowEngineer coordinates for CI/CD
- **Quality Gate:** No deployment without 5-phase completion
- **Anti-Bloat:** Commands must provide orchestration value beyond simple CLI wrapping

---

## WHEN TO USE

### 1. Creating Workflow Orchestration Command (PRIMARY)
**Trigger:** Multi-step workflow needing argument parsing, validation, user-friendly interface
**Action:** Execute complete 5-phase workflow
**Examples:** `/merge-coverage-prs`, `/create-issue`, `/coverage-report`

### 2. Wrapping Skill with User Interface (SKILL INTEGRATION)
**Trigger:** Production-ready skill needs user-facing command
**Action:** Execute Phases 1-3 focusing on skill delegation
**Examples:** `/workflow-status` wrapping cicd-monitoring skill

### 3. Creating CI/CD Automation Trigger (WORKFLOW ENGINEER)
**Trigger:** GitHub Actions workflow needs manual trigger with configurable parameters
**Action:** Execute all 5 phases emphasizing workflow input mapping, dry-run support
**Examples:** `/merge-coverage-prs --dry-run --max 15` triggering workflow

### 4. Preventing Command Bloat (ANTI-BLOAT)
**Trigger:** Uncertain if command provides orchestration value
**Action:** Execute Phase 1 anti-bloat decision framework
**Anti-Bloat Examples:** ❌ `/pr-create` (redundant), ✅ `/workflow-status --details` (orchestrates monitoring)

### 5. Standardizing Existing Command UX (IMPROVEMENT)
**Trigger:** Existing command has inconsistent arguments or unclear errors
**Action:** Execute Phases 4-5 to refactor argument patterns and error handling

---

## 5-PHASE WORKFLOW STEPS

### Phase 1: Command Scope Definition

**Purpose:** Validate command creation appropriateness, define boundaries, prevent bloat

#### Anti-Bloat Decision Framework
```yaml
CREATE_COMMAND_WHEN:
  - Multi-step workflow requiring argument orchestration
  - Complex CLI operation benefiting from user-friendly interface
  - Skill delegation providing reusable implementation access
  - Workflow trigger with configurable parameters and monitoring

DO_NOT_CREATE_WHEN:
  - Simple 1:1 CLI wrapper (use CLI directly)
  - No argument validation or output formatting needed
  - Single-use operation not benefiting from abstraction
```

**Examples:**
- ✅ `/merge-coverage-prs`: Orchestrates multi-PR consolidation with dry-run, monitoring, AI conflict resolution
- ✅ `/create-issue`: Automates issue creation with template selection, context collection, labels
- ❌ `/pr-create`: Direct `gh pr create` wrapper without added value
- ❌ `/commit-check`: Simple validation better in pre-commit hook

#### Command-Skill Boundary Definition

**Command Responsibilities:**
- Argument parsing and validation (positional, named, flags, defaults)
- User-friendly error messages with actionable guidance
- Output formatting and presentation
- Workflow triggering and monitoring

**Skill Responsibilities:**
- Workflow execution logic and business rules
- Resource access (templates, examples, documentation)
- Technical implementation details
- Data processing and transformation

**Integration Pattern:**
Command → Parse/Validate Args → Load Skill → Pass to Skill → Format Output → Handle Errors
Skill → Receive Args → Execute Workflow → Return Results → Report Errors

#### Argument Requirements Specification

**Argument Types:**
- **Positional:** Order matters, typically required (e.g., `/command arg1 arg2`)
- **Named:** Order flexible, explicit naming (e.g., `/command --option value`)
- **Flags:** Boolean toggles, no values (e.g., `/command --dry-run --watch`)
- **Defaults:** Sensible fallbacks for optional parameters

**Specification Template:**
```yaml
POSITIONAL: {name, description, required, validation}
NAMED: {name, type, default, validation}
FLAGS: {name, description, default, conflicts_with}
```

#### UX Consistency Patterns
- Error: `⚠️ [Error Type]: [Issue]. [Fix]. Try: [Example]`
- Success: `✅ [Action]: [Summary]. [Details]. Next: [Actions]`
- Argument hint: `[optional] <required> [--named VALUE] [--flag]`
- Emoji: ⚠️ errors, ✅ success, 🔄 progress, 💡 tips

**Checklist:**
- [ ] Orchestration value validated
- [ ] Anti-bloat framework applied
- [ ] Command-skill boundary defined
- [ ] Arguments specified with types, defaults, validation
- [ ] UX consistency patterns identified

**Resource:** `resources/templates/command-scope-template.md`

---

### Phase 2: Command Structure Template Application

**Purpose:** Apply standardized command markdown with frontmatter and comprehensive documentation

#### Frontmatter Design
```yaml
---
description: "[One sentence: what and primary use case]"
argument-hint: "[arg1] [--option VALUE] [--flag]"
category: "testing|security|architecture|workflow|cicd"
requires-skills: ["skill-name-1", "skill-name-2"]
---
```

**Field Specifications:**
- **description:** One sentence, max 512 chars, includes "what" and "when"
- **argument-hint:** `[optional] <required> [--named VALUE] [--flag]` format
- **category:** Single category for organization
- **requires-skills:** Array of skill dependencies (optional)

#### Required Command Sections
```markdown
# Command Name
[2-3 line introduction]

## Purpose
[Problem solved, target users, value proposition]

## Usage Examples
[5-10 realistic scenarios with outputs]

## Arguments
[Comprehensive specs: positional, named, flags with defaults and validation]

## Output
[Success, dry-run, error patterns]

## Integration
[Skills, CLI tools, workflows, project systems]

## Error Handling
[Common errors with actionable troubleshooting]
```

#### Command Naming Conventions
- **Pattern:** Verb-noun structure (`/create-issue`, `/merge-prs`, `/check-status`)
- **Specific:** Descriptive, unambiguous (`/workflow-status` not `/status`)
- **Format:** Hyphen-separated, lowercase (`/merge-coverage-prs`)
- **Optional Prefixes:** Category-based (`/coverage-*`, `/workflow-*`, `/security-*`)

**Checklist:**
- [ ] Frontmatter complete with all required fields
- [ ] All sections present in standard order
- [ ] Usage examples comprehensive (5-10 scenarios)
- [ ] Arguments fully specified
- [ ] Output includes success, dry-run, error patterns
- [ ] Command naming follows conventions

**Resource:** `resources/templates/command-structure-template.md`

---

### Phase 3: Skill Integration Design

**Purpose:** Define precise command-skill delegation patterns ensuring separation of concerns

#### Delegation Pattern Selection

**Pattern 1: Direct Skill Delegation (Preferred)**
```yaml
USE_WHEN: Complex workflow with reusable business logic
EXAMPLE: /create-issue → github-issue-creation skill
COMMAND: Parse args → Validate → Load skill → Pass to skill → Format output
SKILL: Execute workflow → Access resources → Return results
```

**Pattern 2: CLI Wrapper with Optional Skill Enhancement**
```yaml
USE_WHEN: Simple CLI operation, optional output enhancement
EXAMPLE: /workflow-status → gh CLI + cicd-monitoring skill
COMMAND: Parse args → Execute gh CLI → Optional skill formatting → Present results
SKILL: Parse output → Trend analysis → Rich visualization → Insights
```

**Pattern 3: Workflow Trigger (Minimal Skill)**
```yaml
USE_WHEN: GitHub Actions workflow exists, command provides interface
EXAMPLE: /merge-coverage-prs → workflow trigger + monitoring
COMMAND: Parse args → Map to workflow inputs → gh workflow run → Monitor → Report status
SKILL: Workflow validation → Argument mapping → Post-execution analysis (optional)
```

#### Argument Flow Design
```yaml
USER_INPUT: /create-issue feature "Add tagging" --label frontend --dry-run

COMMAND_PARSING:
  type: "feature"
  title: "Add tagging"
  template: null  # Auto-select
  labels: ["frontend"]
  dry_run: true

SKILL_PARAMETERS:
  issue_type: "feature"
  issue_title: "Add tagging"
  template_override: null
  additional_labels: ["frontend"]  # Appended to defaults
  preview_mode: true
```

#### Error Handling Contract

**Command Validation Errors (Before skill):**
- Invalid types, missing required, argument conflicts, format validation
- Response: `⚠️ Invalid Argument: [Issue]. [Explanation]. Try: [Example]`

**Skill Execution Errors (During workflow):**
- Dependency missing, authentication failure, resource not found, business logic failure
- Response: `⚠️ [Category]: [Issue]. [Fix steps]. Try: [Example]`

**Command Formatting Errors (After skill):**
- Output parsing failure, display rendering issues
- Response: `⚠️ Unexpected Output: [Issue]. Check logs for details.`

#### Output Formatting Contract
```markdown
✅ [Action Completed]: [Result Summary]

Details:
- Field 1: Value
- Field 2: Value

[URL or Reference]

Next: [Suggested follow-ups]
```

**Checklist:**
- [ ] Delegation pattern selected
- [ ] Argument mapping defined
- [ ] Transformation logic specified
- [ ] Error handling contract established
- [ ] Success output formatted consistently
- [ ] Skill dependency documented in frontmatter

**Resource:** `resources/templates/skill-integration-template.md`

---

### Phase 4: Argument Handling Patterns

**Purpose:** Implement robust argument parsing with comprehensive validation

#### Positional Argument Handling
**Characteristics:** Order matters, typically required, parsed by position index
**Example:** `/create-issue <type> <title>`
```bash
/create-issue feature "Add recipe tagging"
# arg_1 = "feature", arg_2 = "Add recipe tagging"
```
**Validation:** Check count, validate types, non-empty strings, trim whitespace

#### Named Argument Handling
**Characteristics:** Order flexible, explicit `--` prefix, optional with defaults
**Example:** `/coverage-report [format] --threshold N --module MODULE`
```bash
/coverage-report detailed --threshold 80 --module Cookbook
# positional_arg = "detailed", named_args = {threshold: 80, module: "Cookbook"}
```
**Validation:** Type checking (number/string/path/enum), range validation, default fallbacks

#### Flag Handling
**Characteristics:** Boolean toggles, no values, mutually exclusive detection
**Example:** `/merge-coverage-prs --dry-run --watch`
```bash
/merge-coverage-prs --dry-run --watch
# flags = {dry_run: true, no_dry_run: false, watch: true}
```
**Validation:** Conflict detection (e.g., `--dry-run` vs `--no-dry-run`)

#### Default Value Design
**Principles:**
- **Safe by Default:** Non-destructive defaults (prefer `--dry-run=true`)
- **Common Use Case:** Optimize for most frequent usage
- **Explicit Override:** Document defaults clearly
- **Type Consistency:** Defaults match argument type

**Example:**
```yaml
--dry-run: true  # SAFE: Preview mode prevents accidents
--max: 8  # COMMON: Reasonable batch size
--labels: "type: coverage,coverage,testing"  # COMMON: Epic labels
```

#### Validation Framework
```yaml
LAYER_1_SYNTAX: Argument count, naming, flag syntax, quote handling
LAYER_2_TYPE: Number parsing, string validation, enum checks, path validation
LAYER_3_SEMANTIC: Range validation, format patterns, existence checks, mutual exclusivity
LAYER_4_BUSINESS: Workflow constraints, skill requirements, system state preconditions
```

#### Error Message Quality
**Template:** `⚠️ [Category]: [Specific Issue]. [Explanation]. Try: [Example]`
**Criteria:** Specific, actionable, educational, consistent

**Examples:**
```bash
# GOOD: Specific, actionable, educational
⚠️ Invalid Format: Threshold must be 0-100. Provided: 150
Threshold represents percentage of code covered by tests.
Try: /coverage-report --threshold 80

# GOOD: Clear fix steps
⚠️ Dependency Missing: gh CLI required but not installed
Install: brew install gh (macOS) | sudo apt install gh (Linux)
Authenticate: gh auth login
Try: /workflow-status
```

**Checklist:**
- [ ] Positional args defined with order, types, validation
- [ ] Named args support flexible ordering
- [ ] Flags as boolean toggles without values
- [ ] Defaults documented with rationale
- [ ] Mutually exclusive flags detected
- [ ] Validation covers all layers
- [ ] Error messages specific, actionable, educational

**Resource:** `resources/templates/argument-validation-template.md`

---

### Phase 5: Error Handling & Feedback

**Purpose:** Comprehensive error handling with actionable troubleshooting and consistent feedback

#### Error Categorization

**Invalid Arguments (User Input):**
- **Cause:** Incorrect syntax, types, values
- **Timing:** During argument parsing
- **Recovery:** User corrects arguments
- **Response:** `⚠️ Invalid Argument: [Issue]. [Explanation]. Try: [Example]`

**Missing Dependencies (Environment):**
- **Cause:** CLI tool, authentication, dependency unavailable
- **Timing:** During execution
- **Recovery:** Install/configure dependency
- **Response:** `⚠️ Dependency Missing: [What]. [Install steps]. Try: [Command]`

**Execution Failures (Runtime):**
- **Cause:** Command/skill runtime failure
- **Timing:** During workflow execution
- **Recovery:** Troubleshoot issue, manual cleanup
- **Response:** `⚠️ Execution Failed: [What]. Troubleshooting: [Steps]. Try: [Alternative]`

**Business Logic Failures (Validation):**
- **Cause:** Valid arguments violate business rules
- **Timing:** During skill execution
- **Recovery:** Adjust arguments to satisfy rules
- **Response:** `⚠️ Validation Failed: [Rule]. Constraint: [Explanation]. Try: [Adjusted]`

#### Success Feedback Patterns
```markdown
✅ [Action Completed]: [Result Summary]

Details:
- Key Detail 1: Value
- Key Detail 2: Value

[Reference: URL, ID, or Path]

Next: [Suggested follow-up actions]
```

**Example:**
```bash
✅ Issue Created: #456 "Add recipe tagging system"

Details:
- Type: feature
- Labels: type: feature, enhancement, frontend
- Milestone: Epic #291 Skills & Commands

https://github.com/Zarichney-Development/zarichney-api/issues/456

Next: Begin implementation or refine acceptance criteria
```

#### Progress Feedback (Long-Running)
**Immediate:** `🔄 Triggering workflow: [Name]. Monitoring...`
**Periodic:** `🔄 Status: In Progress (3m 20s). Jobs: ✅ Setup, 🔄 Consolidate, ⏳ Validate`
**Completion:** `✅ Completed: [Duration]. Results: [Summary]. [URL]`

#### Contextual Guidance
- **Next Steps:** Suggested actions after success
- **Alternatives:** Other approaches on error
- **Tips:** Educational guidance for first-time usage

**Checklist:**
- [ ] All error categories identified
- [ ] Error response templates consistent
- [ ] Success feedback with actionable next steps
- [ ] Progress feedback for long-running commands
- [ ] Contextual guidance provided
- [ ] Emoji usage consistent (⚠️, ✅, 🔄, 💡)

**Resource:** `resources/templates/error-handling-template.md`

---

## TARGET AGENTS

### Primary User: PromptEngineer
**Authority:** EXCLUSIVE over `.claude/commands/`
**Use Cases:** Create slash commands, refactor for consistency, establish delegation patterns, validate anti-bloat

**Workflow:**
1. User requests command or PromptEngineer identifies opportunity
2. Execute 5-phase command-creation workflow
3. Create `.claude/commands/command-name.md`
4. Validate skill integration
5. Deploy and verify UX consistency

### Secondary User: WorkflowEngineer
**Authority:** CI/CD expertise, coordinates with PromptEngineer
**Use Cases:** CI/CD automation triggers, workflow parameter mappings, dry-run patterns

**Coordination:**
- WorkflowEngineer: Define workflow requirements and parameter mappings
- PromptEngineer: Implement command structure following meta-skill
- Joint validation: Test command-workflow integration
- Deploy and document trigger patterns

---

## RESOURCES

### Templates (Ready-to-Use Formats)
- **command-scope-template.md**: Phase 1 assessment questionnaire
- **command-structure-template.md**: Phase 2 complete scaffolding
- **skill-integration-template.md**: Phase 3 delegation patterns
- **argument-validation-template.md**: Phase 4 validation scaffolding
- **error-handling-template.md**: Phase 5 error scenarios

**Location:** `resources/templates/`

### Examples (Reference Implementations)
- **workflow-status-command.md**: CLI wrapper with optional skill enhancement
- **create-issue-command.md**: Direct skill delegation pattern
- **merge-coverage-prs-command.md**: Workflow trigger with monitoring

**Location:** `resources/examples/`

### Documentation (Deep Dives)
- **command-skill-separation.md**: Boundary responsibilities and integration patterns
- **argument-parsing-guide.md**: Robust argument handling deep dive
- **ux-consistency-patterns.md**: Standardized UX across commands
- **anti-bloat-framework.md**: Preventing command ecosystem bloat

**Location:** `resources/documentation/`

---

## INTEGRATION WITH TEAM WORKFLOWS

### Multi-Agent Coordination
- **Consistent UX:** All commands follow same patterns
- **Clear Boundaries:** Commands delegate to skills
- **Reduced Bloat:** Anti-bloat framework prevents wrappers
- **Reusable Skills:** Multiple commands can delegate to same skill

### Claude's Orchestration
- Delegate command creation systematically
- Validate orchestration value before deployment
- Understand command-skill integration patterns
- Enforce anti-bloat framework

### Quality Gate Integration
- PromptEngineer self-validation using 5-phase methodology
- WorkflowEngineer coordination for CI/CD commands
- Real-world testing via user execution
- UX consistency audits across command ecosystem

### CLAUDE.md Integration
Supports CLAUDE.md Section 7 (Essential Tools & Commands):
- Command creation methodology
- Command-skill separation philosophy
- Anti-bloat framework
- Standardized UX patterns

---

## SUCCESS METRICS

### Command Creation Efficiency
- 5-phase systematic process vs. ad-hoc design
- 100% structural consistency (frontmatter, sections)
- Anti-bloat validation enforced
- First-time deployment success

### UX Consistency
- Standardized error messages across all commands
- Success confirmation pattern consistency
- Argument hint consistency
- Emoji usage standardization

### Command-Skill Separation
- Clear boundaries (0% overlap)
- Reusable skills (1 skill → multiple commands)
- Delegation effectiveness
- Integration testing passed

### Developer Experience
- 60-80% reduction in manual GitHub UI navigation
- 90%+ error self-resolution through actionable messages
- 15-20 min/day saved per developer
- High adoption for workflow commands

---

## TROUBLESHOOTING

### Issue: Command Without Orchestration Value (Bloat)
**Symptoms:** Simple 1:1 CLI wrapper without added value
**Solution:** Re-evaluate scope, delete if no orchestration value, escalate if unclear
**Prevention:** Phase 1 checklist validates orchestration value

### Issue: Unclear Command-Skill Boundary
**Symptoms:** Command contains business logic instead of delegating
**Solution:** Extract business logic to skill, refactor command to delegate
**Prevention:** Phase 3 checklist validates clear boundary

### Issue: Inconsistent Argument Handling
**Symptoms:** Different argument patterns across commands
**Solution:** Audit all commands, standardize patterns, document defaults
**Prevention:** Phase 4 checklist ensures consistency

### Issue: Vague Error Messages
**Symptoms:** Users cannot self-resolve, escalate to developers
**Solution:** Enhance with specific issue, constraint, actionable fix, examples
**Prevention:** Phase 5 checklist validates message quality

### Issue: Missing Frontmatter Fields
**Symptoms:** Command doesn't appear in palette or unclear hints
**Solution:** Validate description, argument-hint, category, requires-skills
**Prevention:** Phase 2 checklist validates completeness

### Escalation Path
1. PromptEngineer: Re-execute problematic phase
2. WorkflowEngineer: Validate workflow parameter mappings (CI/CD)
3. Claude: Validate command need via user requirements
4. Anti-Bloat: Re-apply Phase 1 framework
5. User: Fundamental design issues requiring guidance

---

**Skill Status:** ✅ **OPERATIONAL**
**Target Users:** PromptEngineer (primary), WorkflowEngineer (secondary)
**Efficiency Gains:** Systematic 5-phase framework replacing ad-hoc design
**Progressive Loading:** YAML frontmatter (~100 tokens) → SKILL.md (~3,200 tokens) → resources (on-demand)
