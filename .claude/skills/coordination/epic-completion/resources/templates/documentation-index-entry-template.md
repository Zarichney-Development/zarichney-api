# DOCUMENTATION_INDEX.md "Completed Epics" Entry Template

## Template Structure

Use this template to add completed epic entries to `./Docs/DOCUMENTATION_INDEX.md` under the "Completed Epics" section.

---

## Section Header (Create if not exists)

```markdown
## Completed Epics

Archived epics with complete historical context including specs and working directory artifacts.
```

---

## Epic Entry Format

```markdown
### Epic #{{EPIC_NUMBER}}: {{EPIC_FULL_NAME}}
**Archive:** [./Archive/epic-{{EPIC_NUMBER}}-{{EPIC_KEBAB_NAME}}/](./Archive/epic-{{EPIC_NUMBER}}-{{EPIC_KEBAB_NAME}}/README.md)
**Completion:** {{COMPLETION_DATE_YYYY-MM-DD}}
**Summary:** {{ONE_LINE_EXECUTIVE_SUMMARY_WITH_KEY_METRICS}}
**Key Deliverables:** {{COMMA_SEPARATED_MAJOR_OUTCOMES}}
**Performance Documentation:** [{{PERFORMANCE_DOC_NAME}}]({{RELATIVE_PATH_TO_PERFORMANCE_DOC}})
```

---

## Placeholder Guidance

### Required Placeholders

**{{EPIC_NUMBER}}:**
- **Description:** Epic numeric identifier
- **Format:** Integer
- **Example:** 291

**{{EPIC_FULL_NAME}}:**
- **Description:** Complete epic name in Title Case
- **Format:** Descriptive phrase matching archive README header
- **Example:** Agent Skills & Slash Commands Integration

**{{EPIC_KEBAB_NAME}}:**
- **Description:** Epic name in kebab-case for directory path
- **Format:** Lowercase with hyphens
- **Example:** skills-commands

**{{COMPLETION_DATE_YYYY-MM-DD}}:**
- **Description:** Epic archiving completion date
- **Format:** YYYY-MM-DD (ISO 8601)
- **Example:** 2025-10-27

**{{ONE_LINE_EXECUTIVE_SUMMARY_WITH_KEY_METRICS}}:**
- **Description:** Single sentence summarizing epic purpose and key performance metrics
- **Format:** One sentence with quantified achievements
- **Example:** "Progressive loading architecture achieving 50-51% context reduction, 144-328% session token savings, 42-61 min/day productivity gains"

**{{COMMA_SEPARATED_MAJOR_OUTCOMES}}:**
- **Description:** Brief list of primary deliverables
- **Format:** Comma-separated items (3-5 items max)
- **Example:** "7 skills, 4 commands, 11 agents refactored, comprehensive performance documentation"

### Optional Placeholders (Use if applicable)

**{{PERFORMANCE_DOC_NAME}}:**
- **Description:** Performance documentation file name (if performance-critical epic)
- **Format:** Markdown file name
- **Example:** Epic291PerformanceAchievements.md

**{{RELATIVE_PATH_TO_PERFORMANCE_DOC}}:**
- **Description:** Relative path from DOCUMENTATION_INDEX.md to performance documentation
- **Format:** Relative path starting with ./Development/
- **Example:** ./Development/Epic291PerformanceAchievements.md

---

## Complete Example: Epic #291

```markdown
### Epic #291: Agent Skills & Slash Commands Integration
**Archive:** [./Archive/epic-291-skills-commands/](./Archive/epic-291-skills-commands/README.md)
**Completion:** 2025-10-27
**Summary:** Progressive loading architecture achieving 50-51% context reduction, 144-328% session token savings, 42-61 min/day productivity gains
**Key Deliverables:** 7 skills, 4 commands, 11 agents refactored, comprehensive performance documentation
**Performance Documentation:** [Epic291PerformanceAchievements.md](./Development/Epic291PerformanceAchievements.md)
```

---

## Integration Instructions

### Step 1: Locate "Completed Epics" Section

Open `./Docs/DOCUMENTATION_INDEX.md` and search for:
```markdown
## Completed Epics
```

If section does not exist, add it at the end of the file:
```markdown
## Completed Epics

Archived epics with complete historical context including specs and working directory artifacts.
```

### Step 2: Add Epic Entry

Insert new epic entry under "Completed Epics" section header, maintaining reverse chronological order (newest first).

### Step 3: Verify Links

Validate archive link and performance documentation link functional:
```bash
# Verify archive README exists
ls -la ./Docs/Archive/epic-{{EPIC_NUMBER}}-{{EPIC_KEBAB_NAME}}/README.md

# Verify performance documentation exists (if applicable)
ls -la ./Docs/Development/{{PERFORMANCE_DOC_NAME}}
```

### Step 4: Commit Update

Include DOCUMENTATION_INDEX.md update in epic archiving commit:
```bash
git add ./Docs/DOCUMENTATION_INDEX.md
git commit -m "epic: complete Epic #{{EPIC_NUMBER}} archiving with documentation index update (#{{EPIC_NUMBER}})"
```

---

## Validation Checklist

Before committing DOCUMENTATION_INDEX.md update, verify:

- [ ] "Completed Epics" section exists (created if missing)
- [ ] Epic entry added with all required fields
- [ ] Archive link format correct: `[./Archive/epic-{N}-{name}/](./Archive/epic-{N}-{name}/README.md)`
- [ ] Archive README exists at linked path
- [ ] Completion date accurate (YYYY-MM-DD format)
- [ ] Summary concise (one sentence with key metrics)
- [ ] Key deliverables brief (3-5 comma-separated items)
- [ ] Performance documentation linked if performance-critical epic
- [ ] Performance documentation exists at linked path (if applicable)
- [ ] Entry positioned in reverse chronological order (newest first)
- [ ] No placeholder syntax ({{...}}) remaining
- [ ] Markdown formatting correct (links, bold fields)
