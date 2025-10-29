---
description: "{{COMMAND_PURPOSE}}"
argument-hint: "{{USAGE_PATTERN}}"
category: "{{CATEGORY}}"
requires-skills: ["{{skill-name}}"]
---

# {{Command Name}}

**Purpose:** {{Command interface purpose - user-facing CLI wrapper}}

## Command-Skill Separation Pattern

This template demonstrates the recommended pattern for commands that delegate to skills for workflow execution. The command provides CLI interface and UX, while the skill contains implementation logic and resources.

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        USER                                  │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  │ /{{command-name}} {{args}}
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                   COMMAND LAYER                              │
│  .claude/commands/{{command-name}}.md                        │
│                                                               │
│  Responsibilities:                                           │
│  • Argument parsing and validation                          │
│  • User-friendly error messages                             │
│  • Output formatting and display                            │
│  • Skill invocation and coordination                        │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  │ Delegate to skill
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                    SKILL LAYER                               │
│  .claude/skills/{{category}}/{{skill-name}}/                 │
│                                                               │
│  Responsibilities:                                           │
│  • Workflow execution logic                                 │
│  • Business rule enforcement                                │
│  • Resource access (templates/examples/docs)               │
│  • Technical implementation details                         │
│  • Reusable patterns and utilities                          │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  │ Return results
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                   COMMAND LAYER                              │
│  • Format results for display                               │
│  • Add user-friendly context                                │
│  • Provide next steps                                       │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  │ Display output
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                        USER                                  │
└─────────────────────────────────────────────────────────────┘
```

## Skill Integration Details

### Skill Reference
**Location:** `.claude/skills/{{category}}/{{skill-name}}/`

**Skill Structure:**
```
.claude/skills/{{category}}/{{skill-name}}/
├── SKILL.md                    # Main skill definition and workflow
├── resources/
│   ├── templates/              # Reusable templates
│   ├── examples/               # Reference examples
│   └── docs/                   # Skill-specific documentation
└── README.md                   # Skill overview and usage
```

### Command Responsibilities

**1. Argument Parsing and Validation**
- Parse command-line arguments
- Validate argument types and constraints
- Provide clear error messages for invalid inputs
- Set defaults for optional arguments

**2. User Experience**
- Format error messages with helpful context
- Show progress indicators for long operations
- Display results in readable format
- Suggest next steps after completion

**3. Skill Invocation**
- Load skill using Task tool or Skill tool
- Pass validated arguments to skill
- Handle skill execution errors gracefully
- Coordinate multi-skill workflows if needed

**4. Output Formatting**
- Transform skill results into user-friendly display
- Add contextual information and interpretation
- Highlight important findings or metrics
- Provide actionable recommendations

### Skill Responsibilities

**1. Workflow Execution Logic**
- Implement core workflow steps
- Handle business logic and rules
- Manage state and dependencies
- Orchestrate sub-workflows if complex

**2. Resource Management**
- Access templates for code generation
- Load examples for reference
- Read skill-specific documentation
- Manage skill configuration

**3. Technical Implementation**
- File system operations
- Tool integrations (git, gh, dotnet, etc.)
- Data processing and analysis
- Result computation and aggregation

**4. Reusability**
- Provide interfaces for different invocation patterns
- Support parameterization for flexibility
- Enable composition with other skills
- Maintain skill independence

### Integration Flow

```bash
# Step 1: Command receives user input
/{{command-name}} {{arg1}} {{arg2}} --{{option}} {{value}}

# Step 2: Command parses and validates arguments
{{arg1_name}}="{{arg1}}"
{{arg2_name}}="{{arg2}}"
{{option_name}}="{{value}}"

# Validation happens here (command responsibility)
if [ -z "${{arg1_name}}" ]; then
  echo "⚠️ Error: Missing required argument <{{arg1_name}}>"
  exit 1
fi

# Step 3: Command loads and delegates to skill
claude load-skill {{skill-name}}

# Step 4: Skill executes workflow
# Skill reads SKILL.md, accesses resources, executes logic
# Returns structured results

# Step 5: Command formats and displays results
echo "✅ {{Operation}} completed"
echo ""
echo "{{Formatted results from skill}}"
echo ""
echo "💡 Next Steps:"
echo "- {{Suggested action based on results}}"
```

## Usage Examples

### Example 1: Basic Skill Delegation

**Command:**
```bash
/{{command-name}} {{basic-arg}}
```

**Behind the Scenes:**
```
User Input → Command validates → Skill executes → Command formats → User Output
```

**Expected Output:**
```
🔄 {{Loading skill and starting workflow}}
📊 {{Skill execution progress}}

{{Formatted Results}}
{{Key findings or metrics}}

✅ {{Operation}} completed successfully

💡 Next Steps:
- {{Action based on skill results}}
- {{Follow-up suggestion}}
```

### Example 2: With Options Passed to Skill

**Command:**
```bash
/{{command-name}} {{arg1}} --{{option-1}} {{value1}} --{{flag}}
```

**Behind the Scenes:**
```
User Input → Command validates options
          → Skill executes with parameters
          → Command formats with option context
          → User Output
```

**Expected Output:**
```
🔄 {{Executing with specified options}}

{{Modified Results Based on Options}}
{{Skill output tailored to parameters}}

✅ Completed with {{option-1}} = {{value1}}, {{flag}} enabled

💡 Next Steps:
- {{Option-specific suggestion}}
```

### Example 3: Complex Multi-Step Workflow

**Command:**
```bash
/{{command-name}} {{arg1}} {{arg2}} --comprehensive
```

**Behind the Scenes:**
```
User Input → Command validates
          → Skill orchestrates multi-step workflow:
             1. {{Workflow step 1}}
             2. {{Workflow step 2}}
             3. {{Workflow step 3}}
          → Command provides detailed progress
          → Command formats comprehensive results
          → User Output
```

**Expected Output:**
```
🔄 Starting comprehensive {{workflow_name}}

Step 1/3: {{Step 1 description}}
✅ {{Step 1 completed}}

Step 2/3: {{Step 2 description}}
✅ {{Step 2 completed}}

Step 3/3: {{Step 3 description}}
✅ {{Step 3 completed}}

{{Comprehensive Results}}

✅ Comprehensive {{workflow_name}} completed

💡 Next Steps:
- {{Comprehensive follow-up action}}
```

## Arguments

### Required Arguments

These arguments are validated by the command and passed to the skill:

#### `<{{arg1_name}}>` (required)
- **Type:** {{string|number|enum}}
- **Description:** {{What this represents and why skill needs it}}
- **Validation:** {{Command-level validation rules}}
- **Skill Usage:** {{How skill uses this argument}}
- **Examples:**
  - `{{valid-example-1}}`
  - `{{valid-example-2}}`

### Optional Arguments

These arguments modify skill behavior:

#### `--{{option-1}}` VALUE (optional)
- **Type:** {{string|number|enum}}
- **Default:** `{{default-value}}`
- **Description:** {{How this modifies skill execution}}
- **Skill Impact:** {{Specific workflow changes when provided}}
- **Examples:**
  - `--{{option-1}} {{example-1}}`
  - `--{{option-1}} {{example-2}}`

### Flags

These boolean flags control skill workflow branches:

#### `--{{flag-1}}` (flag)
- **Default:** `false`
- **Description:** {{What additional skill functionality this enables}}
- **Skill Impact:** {{Specific workflow extension when enabled}}
- **Example:** `--{{flag-1}}`

## Implementation

### Command Script Structure

```bash
#!/bin/bash

# ============================================================================
# COMMAND LAYER: Argument Parsing and Validation
# ============================================================================

# Required positional arguments
{{arg1_name}}="$1"
{{arg2_name}}="$2"
shift 2

# Defaults for optional arguments
{{option_1}}="{{default-value}}"
{{flag_1}}=false

# Parse optional arguments and flags
while [[ $# -gt 0 ]]; do
  case "$1" in
    --{{option-1}})
      {{option_1}}="$2"
      shift 2
      ;;
    --{{flag-1}})
      {{flag_1}}=true
      shift
      ;;
    *)
      echo "⚠️ Error: Unknown argument '$1'"
      echo "Usage: /{{command-name}} <{{arg1_name}}> <{{arg2_name}}> [OPTIONS]"
      exit 1
      ;;
  esac
done

# Validate required arguments (COMMAND RESPONSIBILITY)
if [ -z "${{arg1_name}}" ] || [ -z "${{arg2_name}}" ]; then
  echo "⚠️ Error: Missing required arguments"
  echo ""
  echo "Usage: /{{command-name}} <{{arg1_name}}> <{{arg2_name}}> [OPTIONS]"
  echo ""
  echo "Examples:"
  echo "  /{{command-name}} {{example-arg1}} {{example-arg2}}"
  echo "  /{{command-name}} {{example-arg1}} {{example-arg2}} --{{option-1}} {{example-value}}"
  exit 1
fi

# Validate argument constraints (COMMAND RESPONSIBILITY)
case "${{arg1_name}}" in
  {{valid-option-1}}|{{valid-option-2}}|{{valid-option-3}})
    # Valid
    ;;
  *)
    echo "⚠️ Error: Invalid {{arg1_name}} '${{arg1_name}}'"
    echo "Valid options: {{valid-option-1}}|{{valid-option-2}}|{{valid-option-3}}"
    exit 1
    ;;
esac

# ============================================================================
# SKILL LAYER: Workflow Execution
# ============================================================================

echo "🔄 {{Starting workflow description}}"

# Method 1: Load skill via Skill tool (if skill-specific tool exists)
# claude load-skill {{skill-name}}

# Method 2: Invoke skill via Task tool with context package
# Prepare context for skill
SKILL_CONTEXT=$(cat <<EOF
Execute {{skill-name}} workflow:

Arguments:
- {{arg1_name}}: ${{arg1_name}}
- {{arg2_name}}: ${{arg2_name}}
- {{option_1}}: ${{option_1}}
- {{flag_1}}: ${{flag_1}}

Skill Location: .claude/skills/{{category}}/{{skill-name}}/

Instructions:
1. Load SKILL.md for workflow definition
2. Access resources/ for templates and examples
3. Execute workflow steps with provided arguments
4. Return structured results for command formatting
EOF
)

# Delegate to skill (SKILL RESPONSIBILITY)
# This is where skill executes its workflow logic
# Skill has access to:
# - SKILL.md (workflow definition)
# - resources/templates/ (code generation)
# - resources/examples/ (reference patterns)
# - resources/docs/ (skill-specific documentation)

# For demonstration, showing manual skill invocation:
# In practice, use Task or Skill tool with above context

# ============================================================================
# COMMAND LAYER: Output Formatting and Display
# ============================================================================

# After skill execution completes, command formats results

echo ""
echo "{{Formatted Results from Skill}}"
echo "{{Additional context and interpretation}}"
echo ""
echo "✅ {{Operation}} completed successfully"
echo ""
echo "💡 Next Steps:"
echo "- {{Action based on skill results}}"
echo "- {{Follow-up suggestion}}"
```

### Skill Invocation Patterns

#### Pattern 1: Direct Skill Tool Usage (Recommended)
```bash
# Load skill by name
claude load-skill {{skill-name}}

# Skill automatically:
# - Loads SKILL.md
# - Accesses resources/
# - Executes workflow
# - Returns results
```

#### Pattern 2: Task Tool with Skill Context (Flexible)
```bash
# Prepare comprehensive context package
CONTEXT=$(cat <<EOF
Role: {{skill-name}} executor

Task: {{Specific workflow description}}

Arguments:
- {{arg1_name}}: ${{arg1_name}}
- {{arg2_name}}: ${{arg2_name}}

Resources:
- Skill definition: .claude/skills/{{category}}/{{skill-name}}/SKILL.md
- Templates: .claude/skills/{{category}}/{{skill-name}}/resources/templates/
- Examples: .claude/skills/{{category}}/{{skill-name}}/resources/examples/

Expected Output:
{{Structure of expected results}}
EOF
)

# Invoke via Task tool
claude task --type {{skill-type}} --context "$CONTEXT"
```

#### Pattern 3: Multi-Skill Orchestration
```bash
# For complex workflows requiring multiple skills

# Step 1: Skill A provides foundation
claude load-skill {{skill-a}}
# Capture Skill A results

# Step 2: Skill B builds on Skill A output
SKILL_B_CONTEXT="Build on Skill A results: {{skill_a_output}}"
claude load-skill {{skill-b}} --context "$SKILL_B_CONTEXT"
# Capture Skill B results

# Step 3: Command integrates all results
echo "{{Integrated results from multiple skills}}"
```

## Output

### Skill Result Structure

**Skill Returns:**
```json
{
  "status": "success|failure",
  "results": {
    "{{key1}}": "{{value1}}",
    "{{key2}}": "{{value2}}"
  },
  "metadata": {
    "execution_time": "{{duration}}",
    "steps_completed": {{count}}
  },
  "recommendations": [
    "{{Recommendation 1}}",
    "{{Recommendation 2}}"
  ]
}
```

**Command Formats As:**
```
✅ {{Operation}} completed in {{duration}}

Results:
• {{key1}}: {{value1}}
• {{key2}}: {{value2}}

📊 Summary:
{{Interpretation of results}}

💡 Next Steps:
- {{Recommendation 1}}
- {{Recommendation 2}}
```

## Error Handling

### Command-Level Errors (Argument Validation)

#### Invalid Arguments
```
⚠️ Error: Invalid {{argument_name}} '{{provided_value}}'

Valid options: {{valid-option-1}}|{{valid-option-2}}|{{valid-option-3}}

Examples:
  /{{command-name}} {{valid-option-1}} {{other-args}}
  /{{command-name}} {{valid-option-2}} {{other-args}}

💡 Tip: {{Helpful context about choosing the right option}}
```

#### Missing Required Arguments
```
⚠️ Error: Missing required argument <{{arg_name}}>

Usage: /{{command-name}} <{{arg1_name}}> <{{arg2_name}}> [OPTIONS]

Required Arguments:
  <{{arg1_name}}>  {{Description}}
  <{{arg2_name}}>  {{Description}}

Example:
  /{{command-name}} {{example-arg1}} {{example-arg2}}
```

#### Argument Type Mismatch
```
⚠️ Error: {{argument_name}} must be {{expected_type}} (got '{{provided_value}}')

Expected: {{Format description}}
Provided: {{provided_value}}

Examples of valid values:
  • {{valid-example-1}}
  • {{valid-example-2}}
  • {{valid-example-3}}
```

### Skill-Level Errors (Workflow Execution)

#### Skill Not Found
```
⚠️ Error: Skill '{{skill-name}}' not found

Expected location: .claude/skills/{{category}}/{{skill-name}}/

💡 Verify:
- Skill directory exists
- SKILL.md is present in skill directory
- Skill name is spelled correctly

Available skills in {{category}}:
{{List available skills}}
```

#### Workflow Execution Failure
```
⚠️ Error: {{skill-name}} workflow failed

Skill error: {{error_message_from_skill}}

💡 Troubleshooting:
1. {{Troubleshooting step 1}}
2. {{Troubleshooting step 2}}
3. {{Troubleshooting step 3}}

Skill diagnostics:
- Workflow step: {{failed_step}}
- Error type: {{error_type}}
- Context: {{error_context}}
```

#### Missing Skill Resources
```
⚠️ Error: Required skill resource not found

Skill: {{skill-name}}
Missing: {{resource_path}}

💡 The skill requires this resource for workflow execution.

To resolve:
1. Check skill directory structure
2. Verify resource files are present
3. Ensure skill is properly installed

Expected structure:
.claude/skills/{{category}}/{{skill-name}}/
├── SKILL.md
└── resources/
    └── {{expected_resource}}
```

#### Skill Dependency Not Met
```
⚠️ Error: Skill dependency not satisfied

Skill: {{skill-name}}
Requires: {{dependency_description}}

💡 Installation:
{{Dependency installation instructions}}

Example:
  {{Installation command}}
  /{{command-name}} {{args}}  # Retry after installation
```

## Success Feedback

### Standard Success Pattern

```
✅ {{Operation}} completed successfully

{{Skill Results Summary}}

📊 Metrics:
- {{Metric 1}}: {{value}}
- {{Metric 2}}: {{value}}

💡 Next Steps:
- {{Skill recommendation 1}}
- {{Skill recommendation 2}}
- {{Command suggestion based on results}}
```

### Success with Skill Insights

```
✅ {{Operation}} completed

{{Skill Results Details}}

🔍 Skill Insights:
{{Interpretation from skill analysis}}

💡 Recommendations:
- {{Priority 1 action based on skill findings}}
- {{Priority 2 action based on skill findings}}

📁 Artifacts Created:
- {{artifact-1}}: {{location}}
- {{artifact-2}}: {{location}}
```

## Integration Points

### Skill Integration

**Primary Skill:** `.claude/skills/{{category}}/{{skill-name}}/`

**Skill Dependencies:**
- Templates: `{{skill}}/resources/templates/*.md`
- Examples: `{{skill}}/resources/examples/*.md`
- Documentation: `{{skill}}/resources/docs/*.md`

**Integration Pattern:**
```
Command (CLI) ──delegates to──> Skill (Workflow)
     │                              │
     │                              ├── Loads SKILL.md
     │                              ├── Accesses templates
     │                              ├── Executes workflow
     │                              └── Returns results
     │
     └── Formats and displays results
```

### Workflow Integration

**Typical Workflow:**
1. User identifies need for {{workflow_purpose}}
2. User runs `/{{command-name}} {{args}}`
3. Command validates arguments and loads skill
4. Skill executes {{workflow_name}} using resources
5. Command formats skill results for display
6. User proceeds with next steps based on output

### Tool Dependencies

**Command Dependencies:**
- Basic shell utilities (bash, case, shift)
- Claude CLI (for skill invocation)

**Skill Dependencies:**
- {{tool-1}}: {{Why skill needs it}}
- {{tool-2}}: {{Why skill needs it}}

## Best Practices

### Command Design

**DO:**
- ✅ Keep command logic thin (validation + formatting only)
- ✅ Delegate workflow execution to skill
- ✅ Provide clear, actionable error messages
- ✅ Format skill results for readability
- ✅ Add contextual interpretation to skill output

**DON'T:**
- ❌ Implement workflow logic in command
- ❌ Duplicate skill functionality in command
- ❌ Bypass skill for "quick" implementations
- ❌ Mix command and skill responsibilities

### Skill Integration

**DO:**
- ✅ Define clear interface between command and skill
- ✅ Pass validated arguments to skill
- ✅ Handle skill errors gracefully
- ✅ Preserve skill results integrity
- ✅ Document command-skill contract

**DON'T:**
- ❌ Modify skill results in command
- ❌ Assume skill internal structure
- ❌ Tightly couple command to skill implementation
- ❌ Bypass skill error reporting

### Separation of Concerns

**Command Layer:**
- CLI interface and UX
- Argument parsing and validation
- Output formatting and display
- User-facing error messages

**Skill Layer:**
- Workflow execution logic
- Business rules and validation
- Resource management
- Technical implementation
- Reusable patterns

**Benefits:**
- Commands remain simple and consistent
- Skills are reusable across commands
- Testing is easier (test layers independently)
- Maintenance is simpler (clear boundaries)
- Evolution is flexible (change skill without changing command)
