# Epic Archiving Procedures

**Purpose:** Detailed step-by-step procedures for executing epic archiving operations with rollback capabilities

**Source:** Extracted from DocumentationStandards.md Section 7 and expanded with operational details

---

## Archive Directory Creation Procedures

### Standard Directory Structure

```
./Docs/Archive/epic-{number}-{name}/
├── README.md                      # Archive overview with epic summary
├── Specs/                         # Complete spec directory contents
│   ├── README.md
│   ├── [iteration spec files]
│   └── [supporting documentation]
└── working-dir/                   # All working directory artifacts
    ├── [execution plans]
    ├── [completion reports]
    ├── [validation reports]
    └── [coordination artifacts]
```

### Naming Convention Specifications

**Archive Directory Name:**
- **Format:** `epic-{number}-{kebab-case-name}`
- **Epic Number:** Numeric identifier from GitHub issue (e.g., 291)
- **Kebab Case Name:** Lowercase with hyphens (e.g., skills-commands)
- **Examples:** `epic-291-skills-commands`, `epic-246-language-model-service`

**Subdirectory Names:**
- **Specs/**: Exactly this name (capital S, lowercase remaining, trailing slash)
- **working-dir/**: Exactly this name (lowercase, hyphen, trailing slash)

**README.md Location:**
- **Required:** At archive root only (`./Docs/Archive/epic-{N}-{name}/README.md`)
- **Not Required:** In subdirectories (Specs/ and working-dir/ may have READMEs from original locations)

### Directory Creation Commands

```bash
# Set variables for epic archiving
EPIC_NUMBER=291  # Replace with actual epic number
EPIC_NAME="skills-commands"  # Replace with actual epic name
ARCHIVE_DIR="./Docs/Archive/epic-${EPIC_NUMBER}-${EPIC_NAME}"

# Create archive root directory
mkdir -p "$ARCHIVE_DIR"

# Verify archive root created
if [ -d "$ARCHIVE_DIR" ]; then
  echo "Archive root created: $ARCHIVE_DIR"
else
  echo "ERROR: Archive root creation failed"
  exit 1
fi

# Create Specs subdirectory
mkdir -p "$ARCHIVE_DIR/Specs"

# Verify Specs created
if [ -d "$ARCHIVE_DIR/Specs" ]; then
  echo "Specs subdirectory created"
else
  echo "ERROR: Specs subdirectory creation failed"
  exit 1
fi

# Create working-dir subdirectory
mkdir -p "$ARCHIVE_DIR/working-dir"

# Verify working-dir created
if [ -d "$ARCHIVE_DIR/working-dir" ]; then
  echo "working-dir subdirectory created"
else
  echo "ERROR: working-dir subdirectory creation failed"
  exit 1
fi

# Verify complete structure
ls -la "$ARCHIVE_DIR"

# Expected output:
# drwxr-xr-x  4 user  staff   128 Oct 27 14:30 .
# drwxr-xr-x  3 user  staff    96 Oct 27 14:30 ..
# drwxr-xr-x  2 user  staff    64 Oct 27 14:30 Specs
# drwxr-xr-x  2 user  staff    64 Oct 27 14:30 working-dir
```

### Conflict Detection and Resolution

**Check for Existing Archive:**
```bash
# Before creating archive, check if already exists
if [ -d "$ARCHIVE_DIR" ]; then
  echo "WARNING: Archive directory already exists at $ARCHIVE_DIR"
  echo "Possible scenarios:"
  echo "1. Previous incomplete archiving attempt"
  echo "2. Duplicate epic number"
  echo "3. Archive already complete"

  # Inspect existing archive
  ls -la "$ARCHIVE_DIR"

  # Decision points:
  # - If incomplete: Backup existing, remove, retry archiving
  # - If complete: Verify epic number correct, adjust naming if duplicate
  # - If uncertain: Escalate to user for conflict resolution
  exit 1
fi
```

**Backup Strategy for Conflicts:**
```bash
# If archive exists and needs replacement
BACKUP_DIR="${ARCHIVE_DIR}_backup_$(date +%Y%m%d_%H%M%S)"
mv "$ARCHIVE_DIR" "$BACKUP_DIR"
echo "Existing archive backed up to: $BACKUP_DIR"

# Proceed with archive creation
mkdir -p "$ARCHIVE_DIR"
mkdir -p "$ARCHIVE_DIR/Specs"
mkdir -p "$ARCHIVE_DIR/working-dir"
```

---

## Specs Directory Archiving Procedures

### Pre-Archiving Inventory

**Purpose:** Document spec directory contents before move for post-archiving verification

```bash
# Locate spec directory
SPEC_DIR="./Docs/Specs/epic-${EPIC_NUMBER}-${EPIC_NAME}"

# Verify spec directory exists
if [ ! -d "$SPEC_DIR" ]; then
  echo "ERROR: Spec directory not found at $SPEC_DIR"
  echo "Possible locations:"
  find ./Docs/Specs -type d -name "*${EPIC_NUMBER}*"
  exit 1
fi

# Count spec files
SPEC_FILE_COUNT=$(find "$SPEC_DIR" -type f | wc -l)
echo "Spec directory contains $SPEC_FILE_COUNT files"

# List spec files for verification
echo "Spec files to archive:"
find "$SPEC_DIR" -type f -exec basename {} \;

# Document inventory
echo "Epic #${EPIC_NUMBER} Spec Inventory: $SPEC_FILE_COUNT files" > /tmp/spec_inventory.txt
find "$SPEC_DIR" -type f >> /tmp/spec_inventory.txt
```

### Move Operations

**Complete Move Strategy:**
```bash
# Move all spec files and subdirectories to archive
mv "$SPEC_DIR"/* "$ARCHIVE_DIR/Specs/"

# Verify move successful
ARCHIVED_FILE_COUNT=$(find "$ARCHIVE_DIR/Specs" -type f | wc -l)

if [ "$ARCHIVED_FILE_COUNT" -eq "$SPEC_FILE_COUNT" ]; then
  echo "Specs archiving successful: $ARCHIVED_FILE_COUNT files moved"
else
  echo "ERROR: File count mismatch"
  echo "Expected: $SPEC_FILE_COUNT, Archived: $ARCHIVED_FILE_COUNT"
  echo "Rollback required - see rollback procedures"
  exit 1
fi
```

### Directory Removal

**Remove Original Spec Directory:**
```bash
# Verify spec directory empty before removal
REMAINING_FILES=$(ls -A "$SPEC_DIR" | wc -l)

if [ "$REMAINING_FILES" -eq 0 ]; then
  # Directory empty, safe to remove
  rmdir "$SPEC_DIR"
  echo "Original spec directory removed"
else
  echo "WARNING: Spec directory not empty after move"
  echo "Remaining files:"
  ls -la "$SPEC_DIR"
  echo "Manual inspection required before removal"
fi

# Verify removal
if [ ! -d "$SPEC_DIR" ]; then
  echo "Spec directory removal confirmed"
else
  echo "Spec directory still exists - manual removal may be required"
fi
```

### Rollback Procedures

**If Specs Move Fails Mid-Operation:**
```bash
# Step 1: Verify partial move state
echo "Checking partial move state..."
ls -la "$SPEC_DIR"
ls -la "$ARCHIVE_DIR/Specs"

# Step 2: Move archived files back to original location
echo "Rolling back specs archiving..."
if [ -d "$ARCHIVE_DIR/Specs" ] && [ "$(ls -A $ARCHIVE_DIR/Specs)" ]; then
  mv "$ARCHIVE_DIR/Specs"/* "$SPEC_DIR/"
  echo "Files restored to $SPEC_DIR"
fi

# Step 3: Verify rollback successful
RESTORED_COUNT=$(find "$SPEC_DIR" -type f | wc -l)
echo "Restored $RESTORED_COUNT files to original spec directory"

# Step 4: Compare with pre-archiving inventory
if [ "$RESTORED_COUNT" -eq "$SPEC_FILE_COUNT" ]; then
  echo "Rollback successful: All files restored"
else
  echo "WARNING: File count mismatch after rollback"
  echo "Expected: $SPEC_FILE_COUNT, Restored: $RESTORED_COUNT"
  echo "Manual inspection required"
fi

# Step 5: Clean up partial archive
rm -rf "$ARCHIVE_DIR/Specs"
echo "Partial archive cleaned up"
```

---

## Working Directory Archiving Procedures

### Artifact Inventory

**Purpose:** Document working directory artifacts before move

```bash
# List all working directory files (excluding README.md)
echo "Working directory artifacts:"
ls -la ./working-dir/ | grep -v README.md

# Count artifacts
ARTIFACT_COUNT=$(ls -1 ./working-dir/ | grep -v README.md | wc -l)
echo "Found $ARTIFACT_COUNT artifacts to archive"

# Categorize artifacts
echo "Artifact categories:"
echo "- Execution plans: $(ls -1 ./working-dir/ | grep -c "execution-plan")"
echo "- Completion reports: $(ls -1 ./working-dir/ | grep -c "completion-report")"
echo "- Validation reports: $(ls -1 ./working-dir/ | grep -c "validation-report")"
echo "- Other coordination artifacts: $(ls -1 ./working-dir/ | grep -v "execution-plan\|completion-report\|validation-report\|README.md" | wc -l)"

# Document inventory
echo "Epic #${EPIC_NUMBER} Working Directory Inventory: $ARTIFACT_COUNT artifacts" > /tmp/working_dir_inventory.txt
ls -1 ./working-dir/ | grep -v README.md >> /tmp/working_dir_inventory.txt
```

### Selective Move (Exclude README.md)

**Critical Requirement:** Working directory README.md must NOT be moved to archive

```bash
# Move all files EXCEPT README.md to archive
find ./working-dir -mindepth 1 -maxdepth 1 ! -name "README.md" -exec mv {} "$ARCHIVE_DIR/working-dir/" \;

# Verify README.md preservation
if [ -f "./working-dir/README.md" ]; then
  echo "Working directory README.md preserved ✓"
else
  echo "ERROR: README.md missing from working directory"
  echo "Attempting restoration from archive..."

  # If accidentally moved, restore from archive
  if [ -f "$ARCHIVE_DIR/working-dir/README.md" ]; then
    mv "$ARCHIVE_DIR/working-dir/README.md" ./working-dir/
    echo "README.md restored to working directory"
  else
    echo "CRITICAL ERROR: README.md not found in archive or working directory"
    echo "Manual restoration from backup required"
  fi
fi
```

### Post-Move Verification

```bash
# Verify working directory cleaned
REMAINING_FILES=$(ls -1 ./working-dir/ | grep -v README.md | wc -l)

if [ "$REMAINING_FILES" -eq 0 ]; then
  echo "Working directory cleaned successfully ✓"
  echo "Only README.md remains"
else
  echo "WARNING: $REMAINING_FILES unexpected files remain in working directory"
  echo "Remaining files:"
  ls -la ./working-dir/ | grep -v README.md
fi

# Verify archive contains all artifacts
ARCHIVED_ARTIFACTS=$(find "$ARCHIVE_DIR/working-dir" -type f ! -name "README.md" | wc -l)

if [ "$ARCHIVED_ARTIFACTS" -eq "$ARTIFACT_COUNT" ]; then
  echo "Artifact count verified ✓"
  echo "$ARCHIVED_ARTIFACTS artifacts archived (expected $ARTIFACT_COUNT)"
else
  echo "ERROR: Artifact count mismatch"
  echo "Expected: $ARTIFACT_COUNT, Archived: $ARCHIVED_ARTIFACTS"
  echo "Rollback may be required"
fi
```

### Rollback Procedures

**If Working Directory Move Fails:**
```bash
# Step 1: Verify partial move state
echo "Checking partial move state..."
ls -la ./working-dir/
ls -la "$ARCHIVE_DIR/working-dir/"

# Step 2: Move archived artifacts back to working directory
echo "Rolling back working directory archiving..."
if [ -d "$ARCHIVE_DIR/working-dir" ] && [ "$(ls -A $ARCHIVE_DIR/working-dir)" ]; then
  # Move everything back EXCEPT if README.md exists in both locations
  find "$ARCHIVE_DIR/working-dir" -mindepth 1 -maxdepth 1 ! -name "README.md" -exec mv {} ./working-dir/ \;
  echo "Artifacts restored to working directory"
fi

# Step 3: Verify rollback successful
RESTORED_COUNT=$(ls -1 ./working-dir/ | grep -v README.md | wc -l)
echo "Restored $RESTORED_COUNT artifacts to working directory"

# Step 4: Compare with pre-archiving inventory
if [ "$RESTORED_COUNT" -eq "$ARTIFACT_COUNT" ]; then
  echo "Rollback successful: All artifacts restored"
else
  echo "WARNING: Artifact count mismatch after rollback"
  echo "Expected: $ARTIFACT_COUNT, Restored: $RESTORED_COUNT"
fi

# Step 5: Clean up partial archive
rm -rf "$ARCHIVE_DIR/working-dir"
echo "Partial archive cleaned up"
```

---

## Archive README Generation Procedures

### Template Application Workflow

**Step 1: Gather Epic Metadata**
```bash
# Epic identification
EPIC_NUMBER=291
EPIC_FULL_NAME="Agent Skills & Slash Commands Integration"
EPIC_KEBAB_NAME="skills-commands"
COMPLETION_DATE=$(date +%Y-%m-%d)

# Iteration and issue counts
ITERATION_COUNT=5
ISSUE_COUNT=316

# Deliverables summary
SKILLS_CREATED=7
COMMANDS_CREATED=4
AGENTS_REFACTORED=11

# Performance metrics
CONTEXT_REDUCTION="50-51%"
TOKEN_SAVINGS="144-328%"
PRODUCTIVITY_GAINS="42-61 min/day"
ROI_PERCENTAGE="90%"
```

**Step 2: Load Archive README Template**
```bash
# Copy template to temporary file for customization
cp ./.claude/skills/coordination/epic-completion/resources/templates/archive-readme-template.md /tmp/archive_readme_draft.md

# Template path for reference
TEMPLATE_PATH="./.claude/skills/coordination/epic-completion/resources/templates/archive-readme-template.md"
```

**Step 3: Replace Placeholders**
```bash
# Replace required placeholders in draft
sed -i '' "s/{{EPIC_NUMBER}}/${EPIC_NUMBER}/g" /tmp/archive_readme_draft.md
sed -i '' "s/{{EPIC_FULL_NAME}}/${EPIC_FULL_NAME}/g" /tmp/archive_readme_draft.md
sed -i '' "s/{{COMPLETION_DATE_YYYY-MM-DD}}/${COMPLETION_DATE}/g" /tmp/archive_readme_draft.md
sed -i '' "s/{{ITERATION_COUNT}}/${ITERATION_COUNT}/g" /tmp/archive_readme_draft.md
sed -i '' "s/{{ISSUE_COUNT}}/${ISSUE_COUNT}/g" /tmp/archive_readme_draft.md

# Replace performance metrics
sed -i '' "s/{{CONTEXT_REDUCTION_PERCENTAGE}}/${CONTEXT_REDUCTION}/g" /tmp/archive_readme_draft.md

# Continue for all placeholders...
```

**Note:** For complex multi-line placeholders (executive summary, iterations list, deliverables), manual editing or scripted content generation recommended.

**Step 4: Write Archive README**
```bash
# Move completed README to archive
mv /tmp/archive_readme_draft.md "$ARCHIVE_DIR/README.md"

# Verify README created
if [ -f "$ARCHIVE_DIR/README.md" ]; then
  echo "Archive README created successfully"
  wc -l "$ARCHIVE_DIR/README.md"
else
  echo "ERROR: Archive README creation failed"
  exit 1
fi
```

### README Validation

```bash
# Verify all required sections present
REQUIRED_SECTIONS=(
  "# Epic #"
  "## Executive Summary"
  "## Iterations Overview"
  "## Key Deliverables"
  "## Documentation Network"
  "## Archive Contents"
)

for section in "${REQUIRED_SECTIONS[@]}"; do
  if grep -q "$section" "$ARCHIVE_DIR/README.md"; then
    echo "✓ $section"
  else
    echo "✗ $section MISSING"
  fi
done

# Verify no placeholder syntax remaining
if grep -q "{{" "$ARCHIVE_DIR/README.md"; then
  echo "WARNING: Placeholders still present in README"
  grep "{{" "$ARCHIVE_DIR/README.md"
else
  echo "✓ No placeholders remaining"
fi
```

---

## Documentation Index Update Procedures

### Locate or Create "Completed Epics" Section

```bash
INDEX_FILE="./Docs/DOCUMENTATION_INDEX.md"

# Verify index file exists
if [ ! -f "$INDEX_FILE" ]; then
  echo "ERROR: DOCUMENTATION_INDEX.md not found"
  exit 1
fi

# Check if "Completed Epics" section exists
if grep -q "## Completed Epics" "$INDEX_FILE"; then
  echo "Completed Epics section exists"
  SECTION_EXISTS=true
else
  echo "Completed Epics section does not exist - will create"
  SECTION_EXISTS=false
fi
```

### Add Epic Entry

**If Section Exists:**
```bash
# Find line number of "## Completed Epics" section
SECTION_LINE=$(grep -n "## Completed Epics" "$INDEX_FILE" | cut -d: -f1)

# Insert new epic entry after section description (usually 2 lines after section header)
INSERT_LINE=$((SECTION_LINE + 3))

# Prepare epic entry
EPIC_ENTRY=$(cat <<EOF

### Epic #${EPIC_NUMBER}: ${EPIC_FULL_NAME}
**Archive:** [./Archive/epic-${EPIC_NUMBER}-${EPIC_KEBAB_NAME}/](./Archive/epic-${EPIC_NUMBER}-${EPIC_KEBAB_NAME}/README.md)
**Completion:** ${COMPLETION_DATE}
**Summary:** ${ONE_LINE_SUMMARY}
**Key Deliverables:** ${DELIVERABLES_LIST}
**Performance Documentation:** [Epic${EPIC_NUMBER}PerformanceAchievements.md](./Development/Epic${EPIC_NUMBER}PerformanceAchievements.md)
EOF
)

# Insert entry (manual editing or sed scripting)
# sed -i '' "${INSERT_LINE}i\\
# ${EPIC_ENTRY}" "$INDEX_FILE"
```

**If Section Does Not Exist:**
```bash
# Append new "Completed Epics" section to end of file
cat >> "$INDEX_FILE" <<EOF

## Completed Epics

Archived epics with complete historical context including specs and working directory artifacts.

### Epic #${EPIC_NUMBER}: ${EPIC_FULL_NAME}
**Archive:** [./Archive/epic-${EPIC_NUMBER}-${EPIC_KEBAB_NAME}/](./Archive/epic-${EPIC_NUMBER}-${EPIC_KEBAB_NAME}/README.md)
**Completion:** ${COMPLETION_DATE}
**Summary:** ${ONE_LINE_SUMMARY}
**Key Deliverables:** ${DELIVERABLES_LIST}
**Performance Documentation:** [Epic${EPIC_NUMBER}PerformanceAchievements.md](./Development/Epic${EPIC_NUMBER}PerformanceAchievements.md)
EOF
```

### Link Verification

```bash
# Verify archive link functional
ARCHIVE_LINK="./Docs/Archive/epic-${EPIC_NUMBER}-${EPIC_KEBAB_NAME}/README.md"
if [ -f "$ARCHIVE_LINK" ]; then
  echo "✓ Archive link functional"
else
  echo "✗ Archive link broken - README not found at $ARCHIVE_LINK"
fi

# Verify performance documentation link functional (if applicable)
PERF_DOC_LINK="./Docs/Development/Epic${EPIC_NUMBER}PerformanceAchievements.md"
if [ -f "$PERF_DOC_LINK" ]; then
  echo "✓ Performance documentation link functional"
else
  echo "✗ Performance documentation link broken (or N/A for this epic)"
fi
```

---

## Cleanup Verification Procedures

### Comprehensive Cleanup Checklist

```bash
# 1. Verify spec directory removed or empty
if [ -d "$SPEC_DIR" ]; then
  echo "⚠ Spec directory still exists: $SPEC_DIR"
  ls -la "$SPEC_DIR"
else
  echo "✓ Spec directory removed"
fi

# 2. Verify working directory cleaned (only README.md)
WORKING_DIR_EXTRA=$(ls -1 ./working-dir/ | grep -v README.md | wc -l)
if [ "$WORKING_DIR_EXTRA" -eq 0 ]; then
  echo "✓ Working directory cleaned (only README.md)"
else
  echo "⚠ Working directory has $WORKING_DIR_EXTRA unexpected files"
  ls -la ./working-dir/ | grep -v README.md
fi

# 3. Verify archive structure complete
if [ -d "$ARCHIVE_DIR/Specs" ] && [ -d "$ARCHIVE_DIR/working-dir" ] && [ -f "$ARCHIVE_DIR/README.md" ]; then
  echo "✓ Archive structure complete"
else
  echo "✗ Archive structure incomplete"
fi

# 4. Verify no orphaned files
ORPHANED=$(find ./Docs -maxdepth 1 -type f -name "*${EPIC_NUMBER}*" | wc -l)
if [ "$ORPHANED" -eq 0 ]; then
  echo "✓ No orphaned files in Docs root"
else
  echo "⚠ $ORPHANED orphaned files found"
  find ./Docs -maxdepth 1 -type f -name "*${EPIC_NUMBER}*"
fi
```

---

## Related Resources

**Skill Sections:**
- Phase 2: Archive Directory Creation
- Phase 3: Specs Archiving
- Phase 4: Working Directory Archiving
- Phase 5: Archive Documentation Generation
- Phase 6: Documentation Index Update
- Phase 7: Cleanup Verification

**Other Resources:**
- [validation-checklist.md](./validation-checklist.md) - Pre/post completion validation
- [error-recovery-guide.md](./error-recovery-guide.md) - Failure recovery procedures
- [archive-readme-template.md](../templates/archive-readme-template.md) - Archive README format
- [documentation-index-entry-template.md](../templates/documentation-index-entry-template.md) - Index entry format
