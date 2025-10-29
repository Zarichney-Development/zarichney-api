---
name: skill-name-here
description: Brief description of what this skill does and when to use it. MUST include BOTH what the skill does AND when agents should use it. Keep concise but comprehensive. Max 1024 characters.
---

**Note:** YAML frontmatter is validated by pre-commit hooks. See [Scripts/validation/README.md](../../Scripts/validation/README.md) for validation details.

# Skill Name Here

[Brief introduction paragraph explaining the skill's purpose and value to agents]

## PURPOSE

[Clear mission statement - what problem does this skill solve? Why does it exist?]

### Core Mission
[1-2 sentences: What is this skill's primary responsibility?]

### Why This Matters
[1-2 sentences: What happens if agents don't use this skill? What value does it provide?]

### Mandatory Application
[When is this skill required vs. optional? Are there any exceptions?]

---

## WHEN TO USE

This skill applies in these scenarios:

### 1. [Primary Use Case]
**Trigger:** [What event or situation prompts skill usage?]
**Action:** [What should the agent do when triggered?]
**Rationale:** [Why is this skill needed for this scenario?]

### 2. [Secondary Use Case]
**Trigger:** [What event or situation prompts skill usage?]
**Action:** [What should the agent do when triggered?]
**Rationale:** [Why is this skill needed for this scenario?]

### 3. [Additional Use Case]
**Trigger:** [What event or situation prompts skill usage?]
**Action:** [What should the agent do when triggered?]
**Rationale:** [Why is this skill needed for this scenario?]

---

## WORKFLOW STEPS

### Step 1: [First Action Name]

[Brief description of what this step accomplishes]

#### Process
1. **[Sub-step 1]**: [Detailed instruction with clear action]
2. **[Sub-step 2]**: [Detailed instruction with clear action]
3. **[Sub-step 3]**: [Detailed instruction with clear action]

#### Standard Format (if applicable)
```
[Template or format agents should follow for this step]
```

#### Checklist
- [ ] [Completion criterion 1]
- [ ] [Completion criterion 2]
- [ ] [Completion criterion 3]

**Resource:** See `resources/templates/[template-name].md` for complete template (if applicable)

---

### Step 2: [Second Action Name]

[Brief description of what this step accomplishes]

#### Process
1. **[Sub-step 1]**: [Detailed instruction with clear action]
2. **[Sub-step 2]**: [Detailed instruction with clear action]
3. **[Sub-step 3]**: [Detailed instruction with clear action]

#### Required Specifications (if applicable)
**[Field/Parameter 1]:**
- [Constraint or requirement]
- [Expected format or value]
- [Example]

**[Field/Parameter 2]:**
- [Constraint or requirement]
- [Expected format or value]
- [Example]

#### Checklist
- [ ] [Completion criterion 1]
- [ ] [Completion criterion 2]
- [ ] [Completion criterion 3]

**Resource:** See `resources/examples/[example-name].md` for reference implementation (if applicable)

---

### Step 3: [Third Action Name]

[Brief description of what this step accomplishes]

#### Process
1. **[Sub-step 1]**: [Detailed instruction with clear action]
2. **[Sub-step 2]**: [Detailed instruction with clear action]
3. **[Sub-step 3]**: [Detailed instruction with clear action]

#### Checklist
- [ ] [Completion criterion 1]
- [ ] [Completion criterion 2]
- [ ] [Completion criterion 3]

---

### Step 4: [Validation/Compliance Step]

[Description of how agents verify compliance with this skill's requirements]

#### Validation Checklist
- [ ] [Validation criterion 1]
- [ ] [Validation criterion 2]
- [ ] [Validation criterion 3]

#### Common Issues and Resolution
**Issue:** [Common problem description]
**Resolution:** [How to fix this issue]

**Issue:** [Common problem description]
**Resolution:** [How to fix this issue]

---

## TARGET AGENTS

[Which agents should use this skill and how?]

### Primary Users
- **[Agent1]**: [How and when this agent uses the skill]
- **[Agent2]**: [How and when this agent uses the skill]
- **[Agent3]**: [How and when this agent uses the skill]

### Secondary Users
- **[Agent4]**: [Optional usage scenario for this agent]
- **[Agent5]**: [Optional usage scenario for this agent]

### Coordination Patterns
[How do agents coordinate when using this skill together? Any cross-agent dependencies?]

---

## RESOURCES

This skill includes comprehensive resources for effective implementation:

### Templates (Ready-to-Use Formats)
- **[template-1-name].md**: [Purpose - what agents can copy-paste for this use case]
- **[template-2-name].md**: [Purpose - what agents can copy-paste for this use case]
- **[template-3-name].md**: [Purpose - what agents can copy-paste for this use case]

**Location:** `resources/templates/`
**Usage:** Copy template, fill in specific details, use verbatim in agent work

### Examples (Reference Implementations)
- **[example-1-name].md**: [What this demonstrates - realistic scenario with annotations]
- **[example-2-name].md**: [What this demonstrates - realistic scenario with annotations]
- **[example-3-name].md**: [What this demonstrates - realistic scenario with annotations]

**Location:** `resources/examples/`
**Usage:** Review examples for realistic scenarios showing workflow steps in action

### Documentation (Deep Dives)
- **[doc-1-name].md**: [Advanced topic covered - when to reference this]
- **[doc-2-name].md**: [Advanced topic covered - when to reference this]
- **[doc-3-name].md**: [Advanced topic covered - when to reference this]

**Location:** `resources/documentation/`
**Usage:** Understand skill philosophy, troubleshoot issues, optimize effectiveness

---

## INTEGRATION WITH TEAM WORKFLOWS

### Multi-Agent Coordination Patterns
This skill enables:
- **[Coordination Pattern 1]**: [Description of how agents work together using this skill]
- **[Coordination Pattern 2]**: [Description of how agents work together using this skill]
- **[Coordination Pattern 3]**: [Description of how agents work together using this skill]

### Claude's Orchestration Enhancement
This skill helps Claude (Codebase Manager) to:
- [Orchestration benefit 1 - how Claude coordinates better with this skill]
- [Orchestration benefit 2 - how Claude coordinates better with this skill]
- [Orchestration benefit 3 - how Claude coordinates better with this skill]

### Quality Gate Integration
This skill integrates with:
- **ComplianceOfficer**: [How this skill supports pre-PR validation]
- **AI Sentinels**: [How this skill supports code review quality]
- **[Other Agent/System]**: [How this skill supports other quality gates]

### CLAUDE.md Integration
[How Claude references this skill in orchestration workflows. Quote relevant CLAUDE.md sections if applicable.]

---

## SUCCESS METRICS

### [Metric Category 1]
- **[Specific Metric]**: [Target or expectation]
- **[Specific Metric]**: [Target or expectation]
- **[Specific Metric]**: [Target or expectation]

### [Metric Category 2]
- **[Specific Metric]**: [Target or expectation]
- **[Specific Metric]**: [Target or expectation]
- **[Specific Metric]**: [Target or expectation]

### [Metric Category 3]
- **[Specific Metric]**: [Target or expectation]
- **[Specific Metric]**: [Target or expectation]
- **[Specific Metric]**: [Target or expectation]

---

## TROUBLESHOOTING

### Common Issues

#### Issue: [Problem Description]
**Symptoms:** [How to identify this issue]
**Root Cause:** [Why this happens]
**Solution:** [Step-by-step resolution]
**Prevention:** [How to avoid this issue]

#### Issue: [Problem Description]
**Symptoms:** [How to identify this issue]
**Root Cause:** [Why this happens]
**Solution:** [Step-by-step resolution]
**Prevention:** [How to avoid this issue]

#### Issue: [Problem Description]
**Symptoms:** [How to identify this issue]
**Root Cause:** [Why this happens]
**Solution:** [Step-by-step resolution]
**Prevention:** [How to avoid this issue]

### Escalation Path
When skill usage issues cannot be resolved:
1. [First escalation action - who to notify or what to check]
2. [Second escalation action - fallback approach]
3. [Final escalation - user involvement criteria]

---

**Skill Status:** [DRAFT/READY/OPERATIONAL/DEPRECATED]
**Adoption:** [Which agents currently use this skill]
**Context Savings:** [Estimated token savings vs. embedding in agent definitions]
**Progressive Loading:** YAML frontmatter (~[X] tokens) → SKILL.md (~[Y] tokens) → resources (on-demand)

---

## TEMPLATE USAGE NOTES

**For Skill Creators:**

### Required Sections (DO NOT REMOVE)
- YAML frontmatter (name, description)
- Purpose
- When to Use (at least 2 scenarios)
- Workflow Steps (at least 2 steps)
- Target Agents
- Resources (even if some subsections are empty)
- Integration with Team Workflows

### Optional Sections (Use if Applicable)
- Success Metrics (recommended for coordination skills)
- Troubleshooting (recommended for complex workflows)

### YAML Frontmatter Requirements
- **name**: Max 64 characters, lowercase letters/numbers/hyphens only, no reserved words
- **description**: Max 1024 characters, MUST include both "what it does" AND "when to use it"

### Progressive Loading Best Practices
- Keep SKILL.md body under 500 lines (highly recommended)
- Target 2,000-5,000 tokens for main instructions
- Move detailed content to resources/ for on-demand loading
- Reference resources one level deep from SKILL.md

### Content Guidelines
- Challenge each piece of information: "Does Claude really need this explanation?"
- Use appropriate degrees of freedom (text instructions vs. pseudocode vs. exact scripts)
- Maintain consistent terminology throughout
- Avoid time-sensitive information (use "old patterns" collapsible details if needed)

### Resource Organization
- **templates/**: Reusable formats with clear placeholder syntax
- **examples/**: Realistic scenarios showing complete workflows
- **documentation/**: Deep dives with table of contents for files >100 lines
- **scripts/**: Executable utilities solving error-prone operations

### Validation Before Publishing
- [ ] YAML frontmatter valid and complete
- [ ] Description optimized for discovery among 100+ skills
- [ ] SKILL.md body under 500 lines
- [ ] Resources organized appropriately
- [ ] Resource references one level deep from SKILL.md
- [ ] Tested with at least 2 target agents
- [ ] Progressive loading verified (metadata → instructions → resources)

---

**Remove this "TEMPLATE USAGE NOTES" section when creating actual skills.**
