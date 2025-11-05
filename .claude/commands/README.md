# Slash Commands Directory

**Purpose:** Workflow automation commands for zarichney-api development
**Last Updated:** 2025-10-28
**Parent:** [`.claude/`](../README.md)

---

## 1. Purpose & Responsibility

* **What it is:** Collection of slash commands that provide CLI-style interfaces for common development workflows

* **Key Objectives:**
  - **Workflow Automation:** Streamline repetitive development operations
  - **Developer Productivity:** Reduce manual context gathering and execution steps
  - **Consistent UX:** Standardized argument handling and error messaging
  - **Skill Integration:** Delegate implementation logic to skills for reusability

* **Why it exists:** Slash commands provide user-facing shortcuts for complex workflows, automating context collection, template application, and multi-step processes. They act as thin CLI wrappers around skill-based implementation logic.

---

## 2. Current Commands

### Development & Testing

**[test-report.md](./test-report.md)**
- **Purpose:** Comprehensive test suite execution with AI-powered analysis
- **Usage:** `/test-report [format] [options]`
- **Features:**
  - Multiple output formats (summary, detailed, json)
  - AI-powered coverage gap analysis
  - Epic progression tracking
  - Intelligent recommendations
- **Integration:** Leverages testing skills and AI analysis prompts
- **Time Saved:** ~3 minutes per coverage analysis

---

### Epic & Project Management

**[tackle-epic-issue.md](./tackle-epic-issue.md)**
- **Purpose:** Execute GitHub issues within Epic #291 context with proper branching strategy
- **Usage:** `/tackle-epic-issue <issue-number>`
- **Features:**
  - Specification review from `Docs/Specs/epic-291-skills-commands/`
  - Epic branch strategy (epic → section → PRs)
  - Multi-agent coordination per CLAUDE.md
  - ComplianceOfficer validation at section level
- **Integration:** Specialized for Epic #291 workflow
- **Time Saved:** ~10-15 minutes per epic issue setup

---

## 3. Command Structure

### YAML Frontmatter (Required)

All commands use YAML frontmatter at the top of the file:

```yaml
---
description: "Brief command purpose (one sentence)"
argument-hint: "[required] [optional]"
category: "testing|security|architecture|workflow"
---
```

**Field Specifications:**
- `description`: One-sentence purpose, visible in command palette
- `argument-hint`: Shows expected argument pattern
- `category`: Groups commands by functional area

### Markdown Body

Following the frontmatter, the markdown body includes:

```markdown
# Command Name

[Brief introduction paragraph]

## Purpose

[Detailed explanation of what this command does and when to use it]

## Usage

\```bash
/command-name <required-arg> [optional-arg]
\```

### Arguments

- `<required-arg>`: [Description with constraints]
- `[optional-arg]`: [Description with default value]

### Options

- `--flag`: [Boolean toggle description]
- `--named=value`: [Named parameter description]

## Examples

\```bash
# Example 1: Basic usage
/command-name arg1

# Example 2: With options
/command-name arg1 --flag --named=value
\```

## Output

[Description of expected output and formatting]

## Error Handling

[Common errors and how to resolve them]

## Integration

[How this command integrates with skills, workflows, other commands]
```

---

## 4. How to Add a New Command

### Before Creating

**Prerequisites:**
1. Identify clear workflow to automate (saves >5 minutes per use)
2. Determine appropriate skill integration (command = interface, skill = logic)
3. Review existing commands to avoid duplication
4. Define clear argument structure and validation

### Creation Process

**1. Use command-creation Meta-Skill (Available Iteration 2.3):**
```
Location: .claude/skills/meta/command-creation/
Purpose: Standardized command creation framework
Benefit: Consistent UX, clear skill integration
```

**2. Manual Creation (Before Iteration 2.3):**
```bash
# Create command file
touch .claude/commands/new-command-name.md

# Follow structure from existing commands
# Reference: test-report.md (comprehensive example)
# Reference: tackle-epic-issue.md (epic-specific workflow)
```

**3. Required Content:**

**YAML Frontmatter:**
```yaml
---
description: "Brief one-sentence purpose visible in command palette"
argument-hint: "<required> [optional] [--flags]"
category: "testing|security|architecture|workflow"
---
```

**Markdown Sections:**
- Purpose: What the command does and when to use it
- Usage: Command syntax with examples
- Arguments: Detailed argument specifications
- Options: Flag and named parameter descriptions
- Examples: Multiple realistic usage scenarios
- Output: Expected results description
- Error Handling: Common issues and solutions
- Integration: Skill delegation and workflow context

**4. Argument Handling Patterns:**

**Positional Arguments:**
```bash
/command arg1 arg2  # Order matters
```

**Named Parameters:**
```bash
/command --format json --threshold 80  # Order flexible
```

**Boolean Flags:**
```bash
/command --verbose --dry-run  # Toggle options
```

**Defaults:**
```bash
/command  # Uses sensible defaults, document clearly
```

**5. Skill Integration:**

Commands should be thin wrappers that delegate to skills:

```markdown
## Implementation

This command delegates to the following skills:

- **`workflow-automation`**: [Specific workflow execution]
- **`github-integration`**: [Issue creation, PR management]
- **`validation-framework`**: [Input validation, error handling]

The command handles:
- ✅ Argument parsing and validation
- ✅ User messaging and error feedback
- ✅ Workflow orchestration

The skills handle:
- ✅ Business logic and implementation
- ✅ Resource templates and examples
- ✅ Deep domain knowledge
```

**6. Validation:**
- Test command execution with various argument combinations
- Validate error messages are clear and actionable
- Confirm skill integration works correctly
- Verify cross-platform compatibility (Windows, macOS, Linux)

**7. Documentation:**
- Update this README.md with new command entry
- Add to command palette registration (if applicable)
- Document in Epic #291 if part of iteration deliverables

---

## 5. Command Categories

### testing
**Purpose:** Test execution, coverage analysis, quality reporting
**Examples:** test-report
**When to Create:** Automate test workflows, coverage tracking, quality gates

### security
**Purpose:** Security audits, vulnerability scanning, compliance checks
**Examples:** (Future: security-audit, vulnerability-scan)
**When to Create:** Security analysis automation, OWASP validation

### architecture
**Purpose:** Architectural analysis, design reviews, technical debt
**Examples:** (Future: architectural-review, tech-debt-analysis)
**When to Create:** System design automation, pattern analysis

### workflow
**Purpose:** Development workflows, project management, automation
**Examples:** tackle-epic-issue, (Future: workflow-status, coverage-report, create-issue, merge-coverage-prs)
**When to Create:** Streamline repetitive workflows, project operations

---

## 6. Command-Skill Separation Philosophy

### Command Responsibilities (Interface Layer)
✅ Argument parsing and validation
✅ User-facing error messages
✅ CLI-style help text
✅ Output formatting for display
✅ Workflow orchestration (delegating to skills)

### Skill Responsibilities (Implementation Layer)
✅ Business logic and algorithms
✅ Resource templates and examples
✅ Deep domain knowledge
✅ Reusable workflows
✅ Context-efficient documentation

### Benefits of Separation
- **Reusability:** Skills can be used by multiple commands or agents
- **Testing:** Skills can be tested independently
- **Maintenance:** Changes to logic don't affect command interface
- **Scalability:** Unlimited skills without command bloat

---

## 7. Epic #291 Integration

### Workflow Commands (Iteration 2.4)

Four priority workflow commands will be implemented:

**1. /workflow-status**
- Purpose: CI/CD workflow monitoring with gh CLI integration
- Time Saved: ~2 minutes per check
- Skill: `workflow-monitoring`

**2. /coverage-report**
- Purpose: Coverage analytics with trend tracking
- Time Saved: ~3 minutes per analysis
- Skill: `coverage-analytics`

**3. /create-issue**
- Purpose: GitHub automation with context collection
- Time Saved: 5 min → 1 min (80% reduction)
- Skill: `github-issue-creation`

**4. /merge-coverage-prs**
- Purpose: Epic consolidation trigger for Coverage Excellence Merge Orchestrator
- Time Saved: ~10 minutes per trigger
- Skill: `coverage-merge-orchestration`

### Timeline
- Iteration 2.3: command-creation meta-skill
- Iteration 2.4: Four workflow commands implementation
- Total Time Savings: 15-20 minutes per day per active developer

---

## 8. Naming Conventions

**File Naming:**
- Format: `lowercase-with-hyphens.md`
- Examples: `test-report.md`, `tackle-epic-issue.md`
- Avoid: Underscores, CamelCase, spaces

**Command Invocation:**
- Format: `/lowercase-with-hyphens`
- Examples: `/test-report`, `/create-issue`
- Match filename without .md extension

**Argument Naming:**
- Required: `<argument-name>` (angle brackets)
- Optional: `[argument-name]` (square brackets)
- Flags: `--flag-name` (double dash, lowercase-with-hyphens)

---

## 9. Common Patterns

### Help Text Pattern
```markdown
## Usage

\```bash
/command-name <required> [optional] [--flag]
\```

Run `/command-name --help` for detailed usage information.
```

### Argument Validation Pattern
```markdown
## Arguments

- `<issue-number>`: GitHub issue number (e.g., 311, 310, 307)
  - Validation: Must be positive integer
  - Error: "Invalid issue number. Expected positive integer."

- `[--format]`: Output format (default: summary)
  - Options: summary, detailed, json
  - Validation: Must be one of allowed options
  - Error: "Invalid format. Choose: summary, detailed, json"
```

### Error Handling Pattern
```markdown
## Error Handling

### Issue Not Found
\```
❌ Error: Issue #XXX not found in repository
→ Verify issue number and try again
→ Epic #291: https://github.com/Zarichney-Development/zarichney-api/issues/291
\```

### Dependencies Not Met
\```
⚠️ Warning: Issue #XXX has unmet dependencies
→ Blocking issues that must complete first:
  - #YYY: [Issue title]
  - #ZZZ: [Issue title]
→ Complete blocking issues before executing this command
\```
```

---

## 10. Troubleshooting

**Problem:** Command not executing
- **Check:** File exists in `.claude/commands/` directory
- **Check:** Filename matches `lowercase-with-hyphens.md` pattern
- **Check:** YAML frontmatter is valid
- **Check:** Command registered in Claude Code command palette

**Problem:** Arguments not parsing correctly
- **Solution:** Review argument-hint in frontmatter
- **Solution:** Validate argument specification in usage section
- **Solution:** Test with various argument combinations

**Problem:** Skill integration not working
- **Solution:** Verify skill exists and is loadable
- **Solution:** Check skill metadata includes command in dependencies
- **Solution:** Validate skill instructions match command expectations

**Problem:** Cross-platform compatibility issues
- **Solution:** Use forward slashes in paths (not backslashes)
- **Solution:** Test on Windows, macOS, and Linux
- **Solution:** Avoid platform-specific commands (use gh CLI, not git.exe)

---

## 11. Related Documentation

**Skills:**
- [Skills Directory](../skills/README.md) - Implementation logic for commands
- [command-creation Meta-Skill](../skills/meta/command-creation/) - When available (Iteration 2.3)

**Epic #291 Archive:**
- [Commands Catalog](../../Docs/Archive/epic-291-skills-commands/Specs/commands-catalog.md)
- [Epic Overview](../../Docs/Archive/epic-291-skills-commands/README.md)

**Standards:**
- [Documentation Standards](../../Docs/Standards/DocumentationStandards.md)
- [Task Management Standards](../../Docs/Standards/TaskManagementStandards.md)

---

## 12. Quick Reference

**Current Command Count:** 7 commands (COMPLETE)

**By Category:**
- Testing: 2 commands (test-report, coverage-report)
- GitHub/Workflow: 3 commands (create-issue, workflow-status, merge-coverage-prs)
- Epic/Project: 2 commands (tackle-epic-issue, epic-complete)

**Epic #291 Status:** COMPLETE and ARCHIVED (2025-10-27)
- All 7 commands delivered successfully
- Command creation meta-skill operational
- Productivity gains: 42-61 min/day achieved

---

**Directory Status:** ✅ Production (Epic #291 Complete - Archived 2025-10-27)

**Maintenance:** Update this README when adding new commands using the command-creation meta-skill
