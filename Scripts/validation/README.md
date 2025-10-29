# Frontmatter Validation Framework

Automated validation ensuring skill YAML frontmatter and command frontmatter comply with JSON schemas.

## Overview

- **Purpose:** Enforce frontmatter quality and consistency
- **Trigger:** Pre-commit hooks block invalid commits automatically
- **Schemas:** skill-metadata.schema.json, command-definition.schema.json
- **Tools:** Python3 + jsonschema + PyYAML

## Pre-Commit Hook (Automatic)

When you commit changes to:
- `.claude/skills/**/SKILL.md` files
- `.claude/commands/*.md` files

The pre-commit hook automatically validates frontmatter and blocks commits if validation fails.

**Example:**
```bash
$ git commit -m "feat: add new skill"
Validating frontmatter for staged files...
❌ ERROR: .claude/skills/my-skill/SKILL.md
  - Field 'description': String exceeds maximum length of 1024
❌ Commit blocked: Frontmatter validation failed
Fix validation errors or use --no-verify to bypass
```

## Manual Validation

### Validate All Skills
```bash
./Scripts/validation/validate-skills-frontmatter.sh
```

### Validate All Commands
```bash
./Scripts/validation/validate-commands-frontmatter.sh
```

### Validate Specific Files
```bash
./Scripts/validation/validate-skills-frontmatter.sh .claude/skills/my-skill/SKILL.md
```

### Validate Everything
```bash
./Scripts/validation/validate-all-frontmatter.sh
```

## Validation Rules

### Skill Frontmatter (SKILL.md)

**Schema:** `/Docs/Templates/schemas/skill-metadata.schema.json`

**Required:**
- `name`: Max 64 chars, kebab-case (lowercase, numbers, hyphens only)
- `description`: Max 1024 chars, must describe what skill does AND when to use it

**Example:**
```yaml
---
name: my-skill-name
description: Brief description of what this skill does and when agents should use it.
---
```

### Command Frontmatter (*.md in .claude/commands/)

**Schema:** `/Docs/Templates/schemas/command-definition.schema.json`

**Required:**
- `description`: Max 200 chars, brief one-sentence purpose

**Optional:**
- `argument-hint`: Shows arguments in palette (e.g., `"<required> [optional]"`)
- `category`: Enum value (testing|security|architecture|workflow|documentation|coordination|epic)
- `requires-skills`: Array of skill dependencies (e.g., `["skill-name-1", "skill-name-2"]`)

**Example:**
```yaml
---
description: "Brief one-sentence purpose of this command"
argument-hint: "<required-arg> [optional-arg]"
category: "workflow"
requires-skills: ["github-issue-creation"]
---
```

## Troubleshooting

### Common Errors

**Error: "Additional properties are not allowed"**
- **Cause:** Frontmatter includes fields not in schema
- **Fix:** Remove unrecognized fields or update schema if field is legitimate

**Error: "String exceeds maximum length of X"**
- **Cause:** Field value too long
- **Fix:** Shorten description/name to meet character limits

**Error: "Does not match pattern"**
- **Cause:** Value doesn't match required format (e.g., kebab-case for skill name)
- **Fix:** Follow format requirements (lowercase, hyphens only, etc.)

**Error: "'field_name' is a required property"**
- **Cause:** Missing required frontmatter field
- **Fix:** Add required field to YAML frontmatter

### Bypassing Validation

**When to bypass:**
- Work-in-progress commits that will be fixed before PR
- Emergency hotfixes requiring immediate commit
- Testing validation framework itself

**How to bypass:**
```bash
git commit --no-verify -m "WIP: incomplete skill frontmatter"
```

**Warning:** Never bypass validation for PRs to develop/main branches.

## Integration

- **Pre-Commit Hook:** `.git/hooks/pre-commit` (automatically installed)
- **Validation Scripts:** `Scripts/validation/validate-*.sh`
- **JSON Schemas:** `Docs/Templates/schemas/*.schema.json`
- **Templates:** SkillTemplate.md and CommandTemplate.md include validation guidance

## Development

**Adding New Validation:**
1. Update JSON schema in `/Docs/Templates/schemas/`
2. Validation scripts automatically use updated schema (no code changes needed)
3. Test with `./Scripts/validation/validate-all-frontmatter.sh`

**Testing Validation:**
```bash
# Test with intentionally invalid frontmatter
./Scripts/validation/validate-skills-frontmatter.sh path/to/invalid-skill/SKILL.md

# Expected: validation error with clear message
```

---

**Validation Framework Version:** 1.0.0 (Epic #291 - Issue #299)
