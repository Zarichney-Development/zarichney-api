---
description: "Brief one-sentence purpose of this command and what it accomplishes"
argument-hint: "<required-arg> [optional-arg] [--flags]"
category: "coordination|epic|testing|security|architecture|workflow|documentation"
requires-skills: ["skill-name-1", "skill-name-2"]
---

**Note:** YAML frontmatter is validated by pre-commit hooks. See [Scripts/validation/README.md](../../Scripts/validation/README.md) for validation details.

# Command Name

[Brief introduction paragraph explaining the command's purpose and primary use case]

## Purpose

[Detailed explanation of what this command does, when to use it, and what problems it solves]

### Core Capabilities
- [Key capability 1]
- [Key capability 2]
- [Key capability 3]

### Target Users
- **[User Type 1]**: [When this user would invoke this command]
- **[User Type 2]**: [When this user would invoke this command]
- **[User Type 3]**: [When this user would invoke this command]

---

## Usage

```bash
/command-name <required-arg> [optional-arg] [--flag] [--named=value]
```

### Basic Examples

```bash
# Example 1: Minimal usage with required arguments only
/command-name value1

# Example 2: With optional argument
/command-name value1 value2

# Example 3: With flags
/command-name value1 --flag

# Example 4: With named options
/command-name value1 --output=json --verbose
```

---

## Arguments

### Required Arguments

#### `<required-arg-1>`
**Type:** [string|number|path|issue-number|etc.]
**Description:** [Detailed description of what this argument represents]
**Constraints:** [Any validation rules, format requirements, or value restrictions]
**Examples:**
- ✅ `valid-example-1`: [Why this is valid]
- ✅ `valid-example-2`: [Why this is valid]
- ❌ `invalid-example`: [Why this is invalid]

#### `<required-arg-2>` (if applicable)
**Type:** [string|number|path|issue-number|etc.]
**Description:** [Detailed description of what this argument represents]
**Constraints:** [Any validation rules, format requirements, or value restrictions]
**Examples:**
- ✅ `valid-example-1`: [Why this is valid]
- ✅ `valid-example-2`: [Why this is valid]
- ❌ `invalid-example`: [Why this is invalid]

### Optional Arguments

#### `[optional-arg-1]`
**Type:** [string|number|path|etc.]
**Description:** [Detailed description of what this argument represents]
**Default:** [Default value if omitted]
**Constraints:** [Any validation rules, format requirements, or value restrictions]
**Examples:**
- `example-1`: [Usage scenario]
- `example-2`: [Usage scenario]

---

## Options

### Boolean Flags

#### `--flag-name`
**Description:** [What this flag enables or toggles]
**Default:** [false/true - default behavior when not specified]
**Example:**
```bash
/command-name arg1 --flag-name
```

#### `--another-flag`
**Description:** [What this flag enables or toggles]
**Default:** [false/true - default behavior when not specified]
**Example:**
```bash
/command-name arg1 --another-flag
```

### Named Parameters

#### `--param-name=<value>`
**Type:** [string|number|path|enum]
**Description:** [What this parameter controls]
**Default:** [Default value if omitted]
**Valid Values:** [List valid options if constrained]
**Examples:**
```bash
# Example usage 1
/command-name arg1 --param-name=value1

# Example usage 2
/command-name arg1 --param-name=value2
```

#### `--another-param=<value>`
**Type:** [string|number|path|enum]
**Description:** [What this parameter controls]
**Default:** [Default value if omitted]
**Valid Values:** [List valid options if constrained]
**Examples:**
```bash
# Example usage
/command-name arg1 --another-param=custom-value
```

---

## What This Command Does

### Phase 1: [First Major Phase]
[Description of what happens in this phase]
- ✅ [Specific action 1]
- ✅ [Specific action 2]
- ✅ [Specific action 3]

### Phase 2: [Second Major Phase]
[Description of what happens in this phase]
- 🔍 [Specific action 1]
- 📊 [Specific action 2]
- 🎯 [Specific action 3]

### Phase 3: [Third Major Phase]
[Description of what happens in this phase]
- 📝 [Specific action 1]
- 🏆 [Specific action 2]
- 🔗 [Specific action 3]

### Phase 4: [Final Phase]
[Description of what happens in this phase]
- 📦 [Specific action 1]
- ✅ [Specific action 2]
- 🚀 [Specific action 3]

---

## Output

### Standard Output Format

[Description of expected output structure and formatting]

```
[Example output showing typical successful execution]
```

### Output Variations

#### Format: `markdown` (Default)
```
[Example markdown output]
```

#### Format: `json`
```json
{
  "example": "json output structure",
  "field1": "value1",
  "field2": ["array", "values"]
}
```

#### Format: `summary`
```
[Example summary output - brief version]
```

#### Format: `console`
```
[Example console-optimized output with colors/formatting]
```

---

## Detailed Examples

### Example 1: [Common Use Case Scenario]

**Scenario:** [Description of when and why this is used]

**Command:**
```bash
/command-name specific-value --option=example
```

**Expected Output:**
```
[Detailed output example]
```

**Explanation:**
[Step-by-step walkthrough of what happens]

---

### Example 2: [Advanced Use Case Scenario]

**Scenario:** [Description of when and why this is used]

**Command:**
```bash
/command-name specific-value optional-value --flag --param=advanced
```

**Expected Output:**
```
[Detailed output example]
```

**Explanation:**
[Step-by-step walkthrough of what happens]

---

### Example 3: [Edge Case or Special Scenario]

**Scenario:** [Description of when and why this is used]

**Command:**
```bash
/command-name edge-case-value --special-handling
```

**Expected Output:**
```
[Detailed output example]
```

**Explanation:**
[Step-by-step walkthrough of what happens]

---

## Error Handling & Validation

### Error Type 1: [Common Error Category]

**Symptoms:**
```
❌ Error: [Error message displayed to user]
```

**Cause:** [Explanation of what causes this error]

**Resolution:**
→ [Step 1 to resolve]
→ [Step 2 to resolve]
→ [Step 3 to resolve]

**Prevention:** [How to avoid this error in the future]

---

### Error Type 2: [Common Error Category]

**Symptoms:**
```
❌ Error: [Error message displayed to user]
```

**Cause:** [Explanation of what causes this error]

**Resolution:**
→ [Step 1 to resolve]
→ [Step 2 to resolve]
→ [Step 3 to resolve]

**Prevention:** [How to avoid this error in the future]

---

### Warning Type 1: [Common Warning Category]

**Symptoms:**
```
⚠️ Warning: [Warning message displayed to user]
```

**Meaning:** [What this warning indicates]

**Action Required:**
→ [What user should check or consider]
→ [Whether execution can continue or must stop]

---

## Skill Delegation

This command delegates to the following skills for implementation logic:

### Primary Skills

#### **[skill-name-1]**
**Purpose:** [What this skill provides to the command]
**Invocation:** [When/how the command loads this skill]
**Resources Used:** [Which skill resources the command leverages]

#### **[skill-name-2]**
**Purpose:** [What this skill provides to the command]
**Invocation:** [When/how the command loads this skill]
**Resources Used:** [Which skill resources the command leverages]

### Supporting Skills (Optional)

#### **[supporting-skill-1]**
**Purpose:** [What this skill provides to the command]
**Invocation:** [When/how the command loads this skill]
**Resources Used:** [Which skill resources the command leverages]

### Command-Skill Coordination
[Explanation of how the command orchestrates multiple skills, if applicable]

---

## Integration

### Agent Engagement

This command coordinates the following agents:

**[Agent1]**
- **Role:** [What this agent does in the workflow]
- **Trigger:** [When the command engages this agent]
- **Deliverables:** [What this agent produces]

**[Agent2]**
- **Role:** [What this agent does in the workflow]
- **Trigger:** [When the command engages this agent]
- **Deliverables:** [What this agent produces]

**[Agent3]**
- **Role:** [What this agent does in the workflow]
- **Trigger:** [When the command engages this agent]
- **Deliverables:** [What this agent produces]

### Workflow Integration

#### CLAUDE.md Orchestration
[How this command integrates with CLAUDE.md delegation protocols]

#### Working Directory Usage
[How this command uses /working-dir/ for context and artifacts]
- **Artifacts Created:** [What files the command creates in working directory]
- **Artifacts Consumed:** [What existing artifacts the command references]
- **Communication Protocols:** [How the command enforces team communication standards]

#### Quality Gates
[How this command integrates with quality systems]
- **ComplianceOfficer:** [When/how compliance validation occurs]
- **AI Sentinels:** [How the command prepares for PR review]
- **Build/Test Validation:** [What validation steps are included]

### CI/CD Integration

[How this command integrates with continuous integration/deployment]
- **Pipeline Triggers:** [What CI/CD workflows this command affects]
- **Automation Hooks:** [How this command is used in automated workflows]
- **Deployment Considerations:** [Any deployment-related aspects]

---

## Technical Implementation

### Dependencies

**Required:**
- [Dependency 1]: [Why this is required and what it provides]
- [Dependency 2]: [Why this is required and what it provides]
- [Dependency 3]: [Why this is required and what it provides]

**Optional:**
- [Optional Dependency 1]: [What additional functionality this enables]
- [Optional Dependency 2]: [What additional functionality this enables]

### System Requirements
- [Requirement 1 - e.g., Docker service running]
- [Requirement 2 - e.g., Environment variables configured]
- [Requirement 3 - e.g., Specific tools installed]

### Configuration

[Any configuration files or settings this command uses]

**Configuration Files:**
- `path/to/config1`: [Purpose and key settings]
- `path/to/config2`: [Purpose and key settings]

**Environment Variables:**
- `ENV_VAR_1`: [Purpose and expected value]
- `ENV_VAR_2`: [Purpose and expected value]

### Performance Considerations

- **Execution Time:** [Typical execution time range]
- **Resource Usage:** [Memory, CPU, disk usage considerations]
- **Scalability:** [How performance scales with input size]

---

## Quality Gates

### Pre-Execution Validation
- [ ] [Validation check 1 - what must be true before command runs]
- [ ] [Validation check 2 - what must be true before command runs]
- [ ] [Validation check 3 - what must be true before command runs]

### During Execution
- [ ] [Quality check 1 - what is verified during execution]
- [ ] [Quality check 2 - what is verified during execution]
- [ ] [Quality check 3 - what is verified during execution]

### Post-Execution Validation
- [ ] [Validation check 1 - what must be true after command completes]
- [ ] [Validation check 2 - what must be true after command completes]
- [ ] [Validation check 3 - what must be true after command completes]

### Success Criteria
- ✅ [Criterion 1 - what indicates successful execution]
- ✅ [Criterion 2 - what indicates successful execution]
- ✅ [Criterion 3 - what indicates successful execution]

---

## Perfect For

- **[Use Case 1]**: [Detailed scenario and benefits]
- **[Use Case 2]**: [Detailed scenario and benefits]
- **[Use Case 3]**: [Detailed scenario and benefits]
- **[Use Case 4]**: [Detailed scenario and benefits]
- **[Use Case 5]**: [Detailed scenario and benefits]

---

## Troubleshooting

### Common Issues

#### Issue: [Problem Title]
**Symptoms:** [How to recognize this issue]
**Likely Cause:** [What typically causes this]
**Diagnostic Steps:**
1. [First diagnostic step]
2. [Second diagnostic step]
3. [Third diagnostic step]

**Solution:**
1. [First resolution step]
2. [Second resolution step]
3. [Third resolution step]

**Prevention:** [How to avoid this issue]

---

#### Issue: [Problem Title]
**Symptoms:** [How to recognize this issue]
**Likely Cause:** [What typically causes this]
**Diagnostic Steps:**
1. [First diagnostic step]
2. [Second diagnostic step]
3. [Third diagnostic step]

**Solution:**
1. [First resolution step]
2. [Second resolution step]
3. [Third resolution step]

**Prevention:** [How to avoid this issue]

---

### Debug Mode

**Enable Debug Output:**
```bash
/command-name arg1 --debug
```

**What Debug Mode Provides:**
- [Debug output type 1]
- [Debug output type 2]
- [Debug output type 3]

---

### Getting Help

**Display Command Help:**
```bash
/command-name --help
```

**Additional Resources:**
- [Resource 1]: `path/to/documentation.md`
- [Resource 2]: `path/to/examples.md`
- [Resource 3]: GitHub Issues for troubleshooting

---

## Version History

**Current Version:** 1.0.0

### Changelog
- **v1.0.0** ([Date]): Initial release
  - [Feature 1 description]
  - [Feature 2 description]
  - [Feature 3 description]

---

## TEMPLATE USAGE NOTES

**For Command Creators:**

### Required YAML Frontmatter
- **description**: Brief one-sentence purpose (required)
- **argument-hint**: CLI syntax showing arguments/flags (recommended)
- **category**: Command category for organization (recommended)
- **requires-skills**: Array of skill names this command delegates to (recommended)

### Required Sections (DO NOT REMOVE)
- YAML frontmatter
- Purpose
- Usage (with basic examples)
- Arguments (required and optional)
- Options (flags and named parameters)
- What This Command Does (major phases)
- Output (format and examples)
- Error Handling & Validation
- Skill Delegation (critical for thin wrapper principle)
- Integration (agents, workflows, quality gates)

### Optional Sections (Use if Applicable)
- Technical Implementation (recommended for complex commands)
- Quality Gates (recommended for validation-heavy commands)
- Perfect For (recommended for discovery)
- Troubleshooting (recommended for error-prone operations)
- Version History (recommended for evolving commands)

### Command Design Principles

**Thin Wrapper Philosophy:**
- Commands handle CLI interface, argument parsing, output formatting
- Skills contain actual business logic and implementation details
- Delegate to skills for all substantial work
- Clear separation: Command = interface, Skills = implementation

**User Experience:**
- Clear, actionable error messages with resolution steps
- Examples covering 80%+ of use cases
- Consistent argument naming across commands
- Helpful defaults to reduce required arguments

**Documentation Quality:**
- Every argument documented with type, constraints, examples
- Every error condition documented with resolution
- Every phase explained with specific actions
- Integration points explicitly documented

### Content Guidelines
- Use emoji sparingly but consistently (✅ ❌ ⚠️ for status, 🔍 📊 🎯 for categories)
- Provide realistic examples showing actual usage patterns
- Document the "happy path" first, then variations and edge cases
- Include both simple and advanced usage examples
- Make error messages actionable with specific next steps

### Validation Before Publishing
- [ ] YAML frontmatter complete and valid
- [ ] All required arguments documented with constraints and examples
- [ ] All options (flags and parameters) documented with defaults
- [ ] At least 3 detailed usage examples provided
- [ ] Error handling comprehensive with resolution steps
- [ ] Skill delegation clearly documented (which skills, when, why)
- [ ] Integration with agents and workflows explained
- [ ] Tested with typical use cases and edge cases
- [ ] Help text accurate and comprehensive

### Skills vs. Commands Decision Tree
**Create a SKILL when:**
- Cross-cutting concern used by multiple agents
- Deep technical knowledge or complex workflow
- Reusable business logic needed in multiple contexts
- Progressive disclosure benefits (templates, examples, documentation)

**Create a COMMAND when:**
- User-facing CLI interface needed
- Specific orchestration of existing skills
- One-time or user-initiated workflow
- Combines multiple agents or skills for a specific outcome

---

**Remove this "TEMPLATE USAGE NOTES" section when creating actual commands.**
