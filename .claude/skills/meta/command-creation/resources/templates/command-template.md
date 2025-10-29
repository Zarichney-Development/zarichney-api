---
description: "{{ONE_SENTENCE_COMMAND_PURPOSE}}"
argument-hint: "{{USAGE_PATTERN}}"
category: "{{testing|security|architecture|workflow}}"
---

# {{Command Name}}

**Purpose:** {{Why this command exists and what user problem it solves}}

## Overview

This template provides the standard structure for creating slash commands in the Claude Code CLI. Follow this pattern to ensure consistency, robust error handling, and excellent user experience.

## Usage Examples

### Example 1: Basic Usage
```bash
/{{command-name}} {{basic-example-args}}
```

**Expected Output:**
```
🔄 {{Action in progress message}}
✅ {{Success confirmation}}

{{Formatted results}}

💡 Next Steps:
- {{Suggested action 1}}
- {{Suggested action 2}}
```

### Example 2: With Optional Arguments
```bash
/{{command-name}} {{required-args}} --{{optional-flag}} --{{named-arg}} {{value}}
```

**Expected Output:**
```
🔄 {{Action with options message}}
✅ {{Success confirmation with options applied}}

{{Modified results based on options}}
```

### Example 3: Edge Case or Advanced Usage
```bash
/{{command-name}} {{advanced-scenario-args}}
```

**Expected Output:**
```
{{Special case handling output}}
```

## Arguments

### Required Arguments

#### `<{{arg1_name}}>` (required)
- **Type:** {{string|number|boolean|enum}}
- **Description:** {{What this argument represents and why it's required}}
- **Validation:** {{Validation rules and constraints}}
- **Examples:**
  - `{{valid-example-1}}`
  - `{{valid-example-2}}`

#### `<{{arg2_name}}>` (required)
- **Type:** {{string|number|boolean|enum}}
- **Description:** {{What this argument represents and why it's required}}
- **Validation:** {{Validation rules and constraints}}
- **Examples:**
  - `{{valid-example-1}}`
  - `{{valid-example-2}}`

### Optional Arguments

#### `--{{named-arg-1}}` VALUE (optional)
- **Type:** {{string|number|boolean|enum}}
- **Default:** `{{default-value}}`
- **Description:** {{What this optional argument controls}}
- **Validation:** {{Validation rules if any}}
- **Examples:**
  - `--{{named-arg-1}} {{example-value-1}}`
  - `--{{named-arg-1}} {{example-value-2}}`

#### `--{{named-arg-2}}` VALUE (optional)
- **Type:** {{string|number|boolean|enum}}
- **Default:** `{{default-value}}`
- **Description:** {{What this optional argument controls}}
- **Valid Values:** {{enumerated-options if applicable}}
- **Examples:**
  - `--{{named-arg-2}} {{valid-option-1}}`
  - `--{{named-arg-2}} {{valid-option-2}}`

### Flags (Boolean Toggles)

#### `--{{flag-1}}` (flag)
- **Default:** `false`
- **Description:** {{What behavior this flag enables/disables}}
- **Usage:** Include flag to enable, omit to disable
- **Example:** `--{{flag-1}}`

#### `--{{flag-2}}` (flag)
- **Default:** `false`
- **Description:** {{What behavior this flag enables/disables}}
- **Mutual Exclusivity:** {{Cannot be used with --other-flag if applicable}}
- **Example:** `--{{flag-2}}`

## Implementation Details

### Argument Parsing Pattern

```bash
#!/bin/bash

# Required positional arguments
{{arg1_name}}="$1"
{{arg2_name}}="$2"
shift 2

# Defaults for optional arguments and flags
{{named_arg_1}}="{{default-value}}"
{{named_arg_2}}="{{default-value}}"
{{flag_1}}=false
{{flag_2}}=false

# Parse optional arguments and flags
while [[ $# -gt 0 ]]; do
  case "$1" in
    --{{named-arg-1}})
      {{named_arg_1}}="$2"
      shift 2
      ;;
    --{{named-arg-2}})
      {{named_arg_2}}="$2"
      shift 2
      ;;
    --{{flag-1}})
      {{flag_1}}=true
      shift
      ;;
    --{{flag-2}})
      {{flag_2}}=true
      shift
      ;;
    *)
      echo "⚠️ Error: Unknown argument '$1'"
      echo "Usage: /{{command-name}} <{{arg1_name}}> <{{arg2_name}}> [OPTIONS]"
      exit 1
      ;;
  esac
done

# Validate required arguments
if [ -z "${{arg1_name}}" ] || [ -z "${{arg2_name}}" ]; then
  echo "⚠️ Error: Missing required arguments"
  echo "Usage: /{{command-name}} <{{arg1_name}}> <{{arg2_name}}> [OPTIONS]"
  exit 1
fi
```

### Validation Logic

```bash
# Validate {{arg1_name}} (enumeration example)
case "${{arg1_name}}" in
  {{valid-option-1}}|{{valid-option-2}}|{{valid-option-3}})
    # Valid value
    ;;
  *)
    echo "⚠️ Error: Invalid {{arg1_name}} '${{arg1_name}}'"
    echo "Valid options: {{valid-option-1}}|{{valid-option-2}}|{{valid-option-3}}"
    echo "Example: /{{command-name}} {{valid-option-1}} {{arg2_example}}"
    exit 1
    ;;
esac

# Validate {{named_arg_2}} (range example)
if ! [[ "${{named_arg_2}}" =~ ^[0-9]+$ ]]; then
  echo "⚠️ Error: {{named_arg_2}} must be a number (got '${{named_arg_2}}')"
  exit 1
fi

if [ "${{named_arg_2}}" -lt {{min_value}} ] || [ "${{named_arg_2}}" -gt {{max_value}} ]; then
  echo "⚠️ Error: {{named_arg_2}} must be between {{min_value}} and {{max_value}} (got ${{named_arg_2}})"
  exit 1
fi

# Validate mutual exclusivity (if applicable)
if [ "${{flag_1}}" = "true" ] && [ "${{flag_2}}" = "true" ]; then
  echo "⚠️ Error: Cannot use --{{flag-1}} and --{{flag-2}} together"
  echo "Choose one flag based on your needs"
  exit 1
fi

# Validate dependencies (if applicable)
if [ -n "${{optional_arg}}" ] && [ -z "${{required_with_arg}}" ]; then
  echo "⚠️ Error: --{{optional_arg}} requires --{{required_with_arg}} to be specified"
  exit 1
fi
```

## Output

### Standard Output Format

```
🔄 {{Action description with context}}

{{Main Results Section}}
{{Formatted data, tables, lists, or summaries}}

{{Additional Context Section if applicable}}
{{Supplementary information or related findings}}

✅ {{Success confirmation}}

💡 Next Steps:
- {{Actionable suggestion 1}}
- {{Actionable suggestion 2}}
- {{Actionable suggestion 3}}
```

### Output Components

**Progress Indicators:**
- 🔄 Use for in-progress operations
- ⏳ Use for longer-running tasks
- 📊 Use for data analysis operations

**Success Confirmations:**
- ✅ Primary success indicator
- 🎯 Success with specific achievement
- 🚀 Success launching or deploying

**Results Formatting:**
- Use tables for structured data
- Use bullet lists for enumerated items
- Use code blocks for technical output
- Use clear section headers for organization

**Next Steps:**
- Always provide 2-4 actionable next steps
- Make suggestions specific and executable
- Prioritize most valuable actions first

## Error Handling

### Invalid Arguments

**Scenario:** User provides invalid argument values

```
⚠️ Error: Invalid {{argument_name}} '{{provided_value}}'

Valid options: {{valid-option-1}}|{{valid-option-2}}|{{valid-option-3}}

Example: /{{command-name}} {{valid-option-1}} {{other-args}}

💡 Tip: {{Helpful context about choosing the right option}}
```

### Missing Required Arguments

**Scenario:** User omits required arguments

```
⚠️ Error: Missing required arguments

Usage: /{{command-name}} <{{arg1_name}}> <{{arg2_name}}> [OPTIONS]

Required:
  <{{arg1_name}}>  {{Brief description}}
  <{{arg2_name}}>  {{Brief description}}

Optional:
  --{{option-1}} VALUE  {{Brief description}}
  --{{flag-1}}          {{Brief description}}

Example: /{{command-name}} {{example-arg1}} {{example-arg2}} --{{option-1}} {{example-value}}
```

### Missing Dependencies

**Scenario:** Required tools or resources are unavailable

```
⚠️ Error: {{Dependency name}} not found

This command requires {{dependency_name}} to be installed and configured.

💡 Installation:
{{Installation instructions or link}}

💡 Configuration:
{{Configuration steps if applicable}}

Example:
  {{Installation command}}
  {{Configuration command}}
  /{{command-name}} {{args}}  # Retry command
```

### Permission Errors

**Scenario:** Insufficient permissions for operation

```
⚠️ Error: Insufficient permissions for {{operation_description}}

Required permissions:
- {{permission-1}}: {{why needed}}
- {{permission-2}}: {{why needed}}

💡 To resolve:
{{Steps to obtain necessary permissions}}

Example:
  {{Permission grant command}}
  /{{command-name}} {{args}}  # Retry command
```

### Execution Failures

**Scenario:** Command executes but operation fails

```
⚠️ Error: {{Operation}} failed

Reason: {{Specific failure reason}}

💡 Troubleshooting:
1. {{First troubleshooting step}}
2. {{Second troubleshooting step}}
3. {{Third troubleshooting step}}

Common causes:
- {{Common cause 1}}
- {{Common cause 2}}
- {{Common cause 3}}

Example resolution:
  {{Corrective command or action}}
  /{{command-name}} {{args}}  # Retry command
```

### Resource Not Found

**Scenario:** Referenced resource doesn't exist

```
⚠️ Error: {{Resource type}} '{{resource_identifier}}' not found

💡 Verify:
- {{Resource_identifier}} exists in {{location}}
- {{Spelling and format are correct}}
- {{Access permissions are sufficient}}

Example:
  {{Command to list available resources}}
  /{{command-name}} {{corrected-args}}  # Retry with valid resource
```

## Success Feedback

### Standard Success Message

```
✅ {{Operation}} completed successfully

{{Summary of what was accomplished}}

Results:
{{Key metrics or outcomes}}

💡 Next Steps:
- {{Most important follow-up action}}
- {{Secondary follow-up action}}
- {{Optional enhancement or related action}}
```

### Success with Warnings

```
✅ {{Operation}} completed with warnings

{{Summary of what was accomplished}}

⚠️ Warnings:
- {{Warning 1 with explanation}}
- {{Warning 2 with explanation}}

💡 Recommendations:
- {{Action to address warning 1}}
- {{Action to address warning 2}}

💡 Next Steps:
- {{Follow-up action}}
```

### Dry Run Success

```
✅ Dry run completed - no changes made

{{Summary of what WOULD happen}}

Planned actions:
- {{Action 1 that would be taken}}
- {{Action 2 that would be taken}}
- {{Action 3 that would be taken}}

💡 To execute for real:
/{{command-name}} {{args}} --execute

⚠️ Review planned actions carefully before executing
```

## Integration Points

### Workflow Integration

**Related Workflows:**
- {{Workflow-1}}: {{How this command integrates}}
- {{Workflow-2}}: {{How this command integrates}}

**Typical Workflow Sequence:**
1. {{Step 1 - possibly another command}}
2. `/{{command-name}} {{args}}` ← This command
3. {{Step 3 - follow-up action}}
4. {{Step 4 - verification or completion}}

### Tool Dependencies

**Required Tools:**
- `{{tool-1}}`: {{Why needed and minimum version}}
- `{{tool-2}}`: {{Why needed and minimum version}}

**Optional Tools:**
- `{{optional-tool-1}}`: {{Enhanced functionality if available}}
- `{{optional-tool-2}}`: {{Enhanced functionality if available}}

### File System Integration

**Input Locations:**
- `{{input-path-1}}`: {{What input is read from here}}
- `{{input-path-2}}`: {{What input is read from here}}

**Output Locations:**
- `{{output-path-1}}`: {{What output is written here}}
- `{{output-path-2}}`: {{What output is written here}}

**Working Directory:**
- Command operates relative to: `{{working-directory}}`
- Creates artifacts in: `{{artifact-location}}`

### Agent Integration

**Agent Coordination:**
- **{{Agent-1}}**: {{How this command supports agent's work}}
- **{{Agent-2}}**: {{How this command supports agent's work}}

**Command-Agent Workflow:**
1. User runs `/{{command-name}} {{args}}`
2. Command gathers {{information}} and prepares {{context}}
3. {{Agent}} uses command output for {{agent-task}}
4. {{Result or follow-up}}

### Skill Integration

**Related Skills:**
- `.claude/skills/{{category}}/{{skill-name}}/`: {{How skill relates}}

**Command-Skill Pattern:**
- Command: Provides CLI interface and argument parsing
- Skill: Contains workflow execution logic and resources
- Integration: {{Specific integration mechanism}}

## Best Practices

### User Experience

**DO:**
- ✅ Provide clear, actionable error messages
- ✅ Include usage examples in error messages
- ✅ Show progress for long-running operations
- ✅ Offer next steps after success
- ✅ Use consistent emoji patterns

**DON'T:**
- ❌ Show cryptic error messages without context
- ❌ Fail silently without user feedback
- ❌ Provide errors without suggested solutions
- ❌ Use inconsistent formatting or terminology

### Argument Design

**DO:**
- ✅ Use positional arguments for required, ordered inputs
- ✅ Use named arguments for optional, order-independent inputs
- ✅ Provide sensible defaults for optional arguments
- ✅ Validate all inputs before execution
- ✅ Use clear, descriptive argument names

**DON'T:**
- ❌ Require more than 2-3 positional arguments
- ❌ Use ambiguous or cryptic argument names
- ❌ Accept invalid inputs without clear error messages
- ❌ Mix conventions inconsistently

### Output Design

**DO:**
- ✅ Format output for readability
- ✅ Highlight important information
- ✅ Provide context for results
- ✅ Include relevant metrics or summaries
- ✅ Suggest logical next actions

**DON'T:**
- ❌ Output raw data without formatting
- ❌ Overwhelm users with excessive detail
- ❌ Omit critical context or explanation
- ❌ Leave users wondering "what now?"

## Template Customization Guide

### Placeholder Replacement

**Required Replacements:**
- `{{command-name}}`: The actual command name (lowercase-with-dashes)
- `{{ONE_SENTENCE_COMMAND_PURPOSE}}`: Concise purpose for frontmatter
- `{{USAGE_PATTERN}}`: Quick usage hint (e.g., "<type> <title> [options]")
- `{{category}}`: One of: testing|security|architecture|workflow

**Argument Replacements:**
- `{{arg1_name}}`, `{{arg2_name}}`: Actual argument names
- `{{named-arg-1}}`, `{{flag-1}}`: Actual option/flag names
- `{{default-value}}`: Actual default values

**Validation Replacements:**
- `{{valid-option-1}}`: Actual valid enumeration values
- `{{min_value}}`, `{{max_value}}`: Actual numeric ranges

**Description Replacements:**
- All `{{description}}` placeholders with actual explanations
- All `{{example}}` placeholders with realistic examples

### Section Customization

**Keep These Sections:**
- Purpose and Overview (always required)
- Usage Examples (minimum 3 examples)
- Arguments (detailed specifications)
- Error Handling (comprehensive coverage)
- Success Feedback (clear confirmations)

**Customize These Sections:**
- Implementation Details (based on actual logic)
- Integration Points (based on actual dependencies)
- Output format (based on actual data)

**Optional Sections:**
- Add command-specific sections as needed
- Remove unused argument types if not applicable
- Expand validation logic for complex cases

### Testing Your Command

**Validation Checklist:**
- [ ] All placeholders replaced with actual values
- [ ] Argument parsing handles all specified arguments
- [ ] Validation logic matches documented constraints
- [ ] Error messages tested for all failure scenarios
- [ ] Success feedback tested for all success paths
- [ ] Integration points verified with actual dependencies
- [ ] Examples tested and produce documented output

**Quality Checks:**
- [ ] Consistent emoji usage throughout
- [ ] Clear, actionable error messages
- [ ] Comprehensive usage examples
- [ ] Helpful next steps provided
- [ ] No orphaned placeholders remaining
