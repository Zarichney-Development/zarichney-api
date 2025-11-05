# Epic Archiving Error Recovery Guide

**Purpose:** Comprehensive error handling and recovery procedures for failed epic archiving operations

**Principle:** Epic archiving must be atomic - either complete successfully or rollback to pre-archiving state

---

## Common Failure Scenarios

### Scenario 1: Spec Directory Not Found

**Symptoms:**
- Phase 3 cannot locate `./Docs/Specs/epic-{number}-{name}/` directory
- Error: "Spec directory not found at expected location"

**Root Causes:**
1. Spec directory naming mismatch (different pattern than archive expects)
2. Spec directory already moved in previous archiving attempt
3. Spec directory never created for this epic
4. Epic number incorrect in archiving command

**Recovery Procedure:**

```bash
# Step 1: Search for spec directory with epic number
echo "Searching for spec directory with epic number ${EPIC_NUMBER}..."
find ./Docs/Specs -type d -name "*${EPIC_NUMBER}*"

# If found with different naming:
# - Adjust SPEC_DIR variable to match actual location
# - Continue archiving with corrected path

# Step 2: Check if already archived
echo "Checking if spec directory already archived..."
ls -la ./Docs/Archive/ | grep "${EPIC_NUMBER}"

# If found in archive:
# - Verify archive completeness
# - Skip Phase 3 (Specs already archived)
# - Document in completion summary

# Step 3: Verify epic number correctness
echo "Verifying epic number ${EPIC_NUMBER}..."
gh issue view ${EPIC_NUMBER}

# If incorrect epic number:
# - Correct EPIC_NUMBER variable
# - Retry search with corrected number

# Step 4: If truly missing
if [ "$SPEC_DIR_EXISTS" = false ]; then
  echo "ESCALATION: Spec directory truly missing"
  echo "Possible reasons:"
  echo "1. Epic had no spec directory (uncommon)"
  echo "2. Spec directory deleted prematurely"
  echo "3. Wrong repository or branch"
  echo ""
  echo "User decision required:"
  echo "A) Continue archiving without specs (document absence)"
  echo "B) Abort archiving until specs located"
  echo "C) Create minimal spec directory from issue history"
fi
```

**Prevention:**
- Verify spec directory exists before starting Phase 2
- Use consistent naming patterns for spec directories (epic-{number}-{name})
- Document spec directory location in epic planning

---

### Scenario 2: Archive Directory Already Exists

**Symptoms:**
- Phase 2 detects existing archive at `./Docs/Archive/epic-{number}-{name}/`
- Error: "Archive directory already exists - conflict detected"

**Root Causes:**
1. Previous incomplete archiving attempt
2. Duplicate epic numbers in different contexts
3. Archive already complete (archiving invoked twice)

**Recovery Procedure:**

```bash
# Step 1: Inspect existing archive
echo "Inspecting existing archive..."
ls -la ./Docs/Archive/epic-${EPIC_NUMBER}-${EPIC_NAME}/

# Check archive completeness indicators
ARCHIVE_README_EXISTS=$([ -f "$ARCHIVE_DIR/README.md" ] && echo "true" || echo "false")
SPECS_DIR_EXISTS=$([ -d "$ARCHIVE_DIR/Specs" ] && echo "true" || echo "false")
WORKING_DIR_EXISTS=$([ -d "$ARCHIVE_DIR/working-dir" ] && echo "true" || echo "false")

echo "Archive completeness:"
echo "- README.md: $ARCHIVE_README_EXISTS"
echo "- Specs/: $SPECS_DIR_EXISTS"
echo "- working-dir/: $WORKING_DIR_EXISTS"

# Step 2: Determine archive state
if [ "$ARCHIVE_README_EXISTS" = "true" ] && [ "$SPECS_DIR_EXISTS" = "true" ] && [ "$WORKING_DIR_EXISTS" = "true" ]; then
  echo "Archive appears COMPLETE"
  echo ""
  echo "Options:"
  echo "A) Verify completeness via validation checklist - if complete, archiving already done"
  echo "B) If incomplete, backup and re-archive"
  echo "C) Escalate to user for manual inspection"

elif [ "$SPECS_DIR_EXISTS" = "true" ] || [ "$WORKING_DIR_EXISTS" = "true" ]; then
  echo "Archive appears PARTIALLY COMPLETE (incomplete previous attempt)"
  echo ""
  echo "Recommended: Backup and retry archiving"

else
  echo "Archive appears INCOMPLETE (empty directory structure)"
  echo ""
  echo "Recommended: Remove empty archive and retry"
fi

# Step 3A: Backup existing archive (if partially complete)
if [ "$BACKUP_REQUIRED" = "true" ]; then
  BACKUP_DIR="${ARCHIVE_DIR}_backup_$(date +%Y%m%d_%H%M%S)"
  echo "Backing up existing archive to: $BACKUP_DIR"
  mv "$ARCHIVE_DIR" "$BACKUP_DIR"

  echo "Backup complete - safe to retry archiving"
fi

# Step 3B: Remove empty archive (if empty structure)
if [ "$REMOVE_EMPTY" = "true" ]; then
  echo "Removing empty archive structure..."
  rm -rf "$ARCHIVE_DIR"

  echo "Empty archive removed - safe to retry archiving"
fi

# Step 3C: Verify epic number uniqueness (if duplicate suspected)
echo "Verifying epic number uniqueness..."
gh issue list --search "${EPIC_NUMBER}" --state all

# If multiple issues with same number in different contexts:
# - Adjust archive naming to include disambiguating context
# - Example: epic-291-skills-commands-main vs epic-291-skills-commands-experimental
```

**Prevention:**
- Check for existing archive before Phase 2 (add to pre-validation)
- Document archiving attempts in working directory before archiving working-dir
- Use timestamped backups if uncertainty about archive state

---

### Scenario 3: File Move Operation Fails Mid-Transfer

**Symptoms:**
- `mv` command fails partway through moving files
- Error: "Permission denied", "No space left on device", "Operation not permitted"
- Partial files in both original and archive locations

**Root Causes:**
1. Insufficient disk space on target filesystem
2. Permission issues on source or destination
3. Filesystem limits (max files, inode limits)
4. File in use by another process

**Recovery Procedure:**

```bash
# Step 1: Identify partial move state
echo "Assessing partial move state..."

# Count files in original location
ORIGINAL_COUNT=$(find "$SPEC_DIR" -type f 2>/dev/null | wc -l)
echo "Files remaining in original location: $ORIGINAL_COUNT"

# Count files in archive location
ARCHIVE_COUNT=$(find "$ARCHIVE_DIR/Specs" -type f 2>/dev/null | wc -l)
echo "Files successfully moved to archive: $ARCHIVE_COUNT"

# Compare with expected total
EXPECTED_TOTAL=$SPEC_FILE_COUNT
echo "Expected total: $EXPECTED_TOTAL"
echo "Accounted for: $((ORIGINAL_COUNT + ARCHIVE_COUNT))"

if [ $((ORIGINAL_COUNT + ARCHIVE_COUNT)) -ne "$EXPECTED_TOTAL" ]; then
  echo "WARNING: File count discrepancy - some files may be lost"
fi

# Step 2: Check for specific failure cause
echo "Checking failure cause..."

# Disk space check
df -h "$ARCHIVE_DIR"

# Permission check
ls -la "$ARCHIVE_DIR"
ls -la "$SPEC_DIR"

# File-in-use check (macOS/Linux)
lsof +D "$SPEC_DIR" 2>/dev/null

# Step 3: Rollback partial move
echo "Rolling back partial move..."

# Move archived files back to original location
if [ -d "$ARCHIVE_DIR/Specs" ] && [ "$(ls -A $ARCHIVE_DIR/Specs 2>/dev/null)" ]; then
  echo "Moving ${ARCHIVE_COUNT} files back to original location..."
  mv "$ARCHIVE_DIR/Specs"/* "$SPEC_DIR/" 2>/dev/null

  # Verify rollback
  RESTORED_COUNT=$(find "$SPEC_DIR" -type f | wc -l)
  echo "Restored $RESTORED_COUNT files"

  if [ "$RESTORED_COUNT" -eq "$EXPECTED_TOTAL" ]; then
    echo "✓ Rollback successful - all files restored"
  else
    echo "⚠ Partial rollback - manual inspection required"
    echo "Expected: $EXPECTED_TOTAL, Restored: $RESTORED_COUNT"
  fi
fi

# Step 4: Clean up partial archive
echo "Cleaning up partial archive..."
rm -rf "$ARCHIVE_DIR/Specs"
echo "Partial archive removed"

# Step 5: Address root cause before retry
echo "Addressing root cause..."

# If disk space issue:
if [ "$DISK_SPACE_LOW" = "true" ]; then
  echo "Free disk space required before retrying archiving"
  echo "Current available: $(df -h $ARCHIVE_DIR | tail -1 | awk '{print $4}')"
fi

# If permission issue:
if [ "$PERMISSION_ERROR" = "true" ]; then
  echo "Fix file/directory permissions before retrying"
  echo "May require: sudo chown or sudo chmod"
fi

# If file-in-use issue:
if [ "$FILE_IN_USE" = "true" ]; then
  echo "Close processes accessing files before retrying"
  echo "Processes detected: $(lsof +D $SPEC_DIR | tail -n +2 | awk '{print $1}' | sort -u)"
fi
```

**Prevention:**
- Check disk space before archiving: `df -h ./Docs/Archive/`
- Verify permissions on archive directory
- Close all processes that might access epic files before archiving
- Use `-v` flag with `mv` for verbose output showing progress

---

### Scenario 4: Working Directory README.md Accidentally Moved

**Symptoms:**
- Phase 4 verification reveals `./working-dir/README.md` missing
- Working directory empty (all files moved including README.md)

**Root Causes:**
1. Move command didn't properly exclude README.md
2. Regex pattern error in `find` command
3. Manual move operation included README.md

**Recovery Procedure:**

```bash
# Step 1: Verify README.md missing from working directory
if [ ! -f "./working-dir/README.md" ]; then
  echo "CONFIRMED: README.md missing from working directory"

  # Step 2: Check if README.md moved to archive
  if [ -f "$ARCHIVE_DIR/working-dir/README.md" ]; then
    echo "README.md found in archive - restoring to working directory"

    # Step 3: Restore README.md to working directory
    mv "$ARCHIVE_DIR/working-dir/README.md" ./working-dir/
    echo "README.md restored"

    # Step 4: Verify restoration successful
    if [ -f "./working-dir/README.md" ]; then
      echo "✓ README.md restoration successful"
    else
      echo "✗ README.md restoration failed"
      exit 1
    fi

  else
    echo "CRITICAL: README.md not found in working directory OR archive"
    echo ""
    echo "Recovery options:"
    echo "A) Restore from git history: git checkout HEAD -- working-dir/README.md"
    echo "B) Restore from backup (if available)"
    echo "C) Recreate README.md manually from template"

    # Attempt git restoration
    echo "Attempting git restoration..."
    git checkout HEAD -- working-dir/README.md

    if [ -f "./working-dir/README.md" ]; then
      echo "✓ README.md restored from git"
    else
      echo "✗ Git restoration failed - manual intervention required"
      echo "Template location: /working-dir/README.md in repository"
    fi
  fi
else
  echo "✓ README.md present in working directory (no recovery needed)"
fi
```

**Prevention:**
- Use explicit exclusion in move command: `find ./working-dir -mindepth 1 -maxdepth 1 ! -name "README.md"`
- Verify README.md exclusion before executing move
- Test move command on dry-run first
- Add README.md preservation check to Phase 4 validation

---

### Scenario 5: Broken Links in Archive README or DOCUMENTATION_INDEX.md

**Symptoms:**
- Phase 6 or Phase 8 link verification fails
- Links point to non-existent files or incorrect relative paths
- Navigation broken in documentation network

**Root Causes:**
1. Archive README links use incorrect relative paths from archive location
2. Documentation moved/renamed after archive README generated
3. Performance documentation not committed before archiving
4. DOCUMENTATION_INDEX.md entry uses wrong archive path

**Recovery Procedure:**

```bash
# Step 1: Identify broken links
echo "Checking for broken links..."

# Check archive README links
echo "Archive README links:"
grep -o '\[.*\](\.\.\/.*\.md)' "$ARCHIVE_DIR/README.md" | while read -r link; do
  # Extract path from markdown link
  LINK_PATH=$(echo "$link" | sed -n 's/.*(\(.*\))/\1/p')

  # Resolve relative path from archive location
  RESOLVED_PATH="$ARCHIVE_DIR/$LINK_PATH"

  if [ -f "$RESOLVED_PATH" ]; then
    echo "✓ $LINK_PATH"
  else
    echo "✗ $LINK_PATH (BROKEN)"
    BROKEN_LINKS+=("$LINK_PATH")
  fi
done

# Check DOCUMENTATION_INDEX.md epic entry
echo "DOCUMENTATION_INDEX.md links:"
ARCHIVE_LINK="./Docs/Archive/epic-${EPIC_NUMBER}-${EPIC_KEBAB_NAME}/README.md"

if [ -f "$ARCHIVE_LINK" ]; then
  echo "✓ Archive link functional"
else
  echo "✗ Archive link broken"
fi

# Step 2: Fix broken archive README links
echo "Fixing broken archive README links..."

for broken_link in "${BROKEN_LINKS[@]}"; do
  echo "Processing broken link: $broken_link"

  # Determine correct relative path
  # Archive location: ./Docs/Archive/epic-{N}-{name}/README.md
  # Target (typically): ./Docs/Development/{doc}.md

  # Calculate correct relative path from archive to Docs/Development
  # From Archive: ../../Development/{doc}.md

  FILENAME=$(basename "$broken_link")
  CORRECT_PATH="../../Development/$FILENAME"

  echo "Updating $broken_link -> $CORRECT_PATH"

  # Update link in archive README
  sed -i '' "s|$broken_link|$CORRECT_PATH|g" "$ARCHIVE_DIR/README.md"
done

# Step 3: Verify fixes
echo "Verifying link fixes..."
# Re-run link check from Step 1

# Step 4: Update DOCUMENTATION_INDEX.md if archive path wrong
if [ "$ARCHIVE_LINK_BROKEN" = "true" ]; then
  echo "Fixing DOCUMENTATION_INDEX.md archive link..."

  # Correct path format: ./Archive/epic-{N}-{name}/README.md
  CORRECT_INDEX_PATH="./Archive/epic-${EPIC_NUMBER}-${EPIC_KEBAB_NAME}/README.md"

  # Update DOCUMENTATION_INDEX.md
  sed -i '' "s|./Archive/epic-${EPIC_NUMBER}-.*README\.md|$CORRECT_INDEX_PATH|g" ./Docs/DOCUMENTATION_INDEX.md

  echo "DOCUMENTATION_INDEX.md link updated"
fi
```

**Prevention:**
- Use relative path calculator: From `./Docs/Archive/epic-{N}-{name}/` to `./Docs/Development/` = `../../Development/`
- Verify all documentation committed before archiving
- Test links by navigating documentation network before finalizing
- Include link verification in Phase 8 validation checklist

---

### Scenario 6: Validation Failures Detected Post-Archiving

**Symptoms:**
- Phase 8 validation reveals completeness issues
- Archive missing expected files
- Documentation inconsistencies
- Quality gate failures

**Root Causes:**
1. Phases 2-7 executed with skipped validation steps
2. Concurrent modifications during archiving
3. Archive generation errors not caught immediately
4. ComplianceOfficer validation bypassed

**Recovery Procedure:**

```bash
# Step 1: Identify specific validation failures
echo "Reviewing validation failures..."

# Run comprehensive validation
source ./.claude/skills/coordination/epic-completion/resources/documentation/validation-checklist.md

# Document failing checks
echo "Validation failures:"
echo "- Archive integrity: ${ARCHIVE_INTEGRITY_FAILURES}"
echo "- Cleanup completeness: ${CLEANUP_FAILURES}"
echo "- Documentation integration: ${DOCUMENTATION_FAILURES}"
echo "- Quality gates: ${QUALITY_GATE_FAILURES}"

# Step 2: Assess severity
if [ "$CRITICAL_FAILURES" -gt 0 ]; then
  echo "CRITICAL FAILURES DETECTED - Full rollback required"
  ROLLBACK_REQUIRED=true
elif [ "$MAJOR_FAILURES" -gt 0 ]; then
  echo "MAJOR FAILURES DETECTED - Partial fixes required"
  PARTIAL_FIX_REQUIRED=true
else
  echo "MINOR FAILURES DETECTED - Quick fixes possible"
  QUICK_FIX_REQUIRED=true
fi

# Step 3A: Full rollback (if critical failures)
if [ "$ROLLBACK_REQUIRED" = "true" ]; then
  echo "Initiating full archiving rollback..."

  # Restore specs from archive to original location
  if [ -d "$ARCHIVE_DIR/Specs" ]; then
    echo "Restoring specs to original location..."
    mkdir -p "$SPEC_DIR"
    mv "$ARCHIVE_DIR/Specs"/* "$SPEC_DIR/"
  fi

  # Restore working directory artifacts
  if [ -d "$ARCHIVE_DIR/working-dir" ]; then
    echo "Restoring working directory artifacts..."
    mv "$ARCHIVE_DIR/working-dir"/* ./working-dir/

    # Restore README.md if needed
    if [ ! -f "./working-dir/README.md" ]; then
      git checkout HEAD -- working-dir/README.md
    fi
  fi

  # Remove incomplete archive
  echo "Removing incomplete archive..."
  rm -rf "$ARCHIVE_DIR"

  # Remove DOCUMENTATION_INDEX.md entry
  echo "Removing DOCUMENTATION_INDEX.md entry..."
  # (Edit to remove epic entry)

  echo "Full rollback complete - address validation failures before retrying"
fi

# Step 3B: Partial fixes (if major failures)
if [ "$PARTIAL_FIX_REQUIRED" = "true" ]; then
  echo "Applying partial fixes..."

  # Fix archive README if incomplete
  if [ "$ARCHIVE_README_INCOMPLETE" = "true" ]; then
    echo "Regenerating archive README..."
    # Re-run Phase 5 (Archive Documentation Generation)
  fi

  # Fix documentation integration if broken
  if [ "$DOCUMENTATION_INTEGRATION_BROKEN" = "true" ]; then
    echo "Fixing documentation integration..."
    # Re-run Phase 6 (Documentation Index Update) with corrected links
  fi

  # Re-validate after fixes
  echo "Re-running validation..."
  # Re-execute Phase 8 validation
fi

# Step 3C: Quick fixes (if minor failures)
if [ "$QUICK_FIX_REQUIRED" = "true" ]; then
  echo "Applying quick fixes..."

  # Examples: Fix typos in README, add missing links, update completion date
  # Apply specific fixes per validation failure report

  echo "Quick fixes complete"
fi

# Step 4: ComplianceOfficer re-validation
echo "Requesting ComplianceOfficer final validation..."
# Engage ComplianceOfficer for comprehensive validation confirmation
```

**Prevention:**
- Execute all Phase 1 pre-completion validation checks before starting archiving
- Validate each phase before proceeding to next phase
- Engage ComplianceOfficer for Phase 1 and Phase 8 validation
- Lock epic files during archiving to prevent concurrent modifications
- Document all archiving operations in real-time for traceability

---

## Complete Rollback Procedures

### Full Archiving Rollback (Restore Pre-Archiving State)

**When to Use:** Critical failures requiring complete archiving reversal

```bash
#!/bin/bash
# Epic Archiving Complete Rollback Script

set -e  # Exit on error

EPIC_NUMBER=$1
EPIC_NAME=$2

if [ -z "$EPIC_NUMBER" ] || [ -z "$EPIC_NAME" ]; then
  echo "Usage: $0 <epic-number> <epic-name>"
  exit 1
fi

ARCHIVE_DIR="./Docs/Archive/epic-${EPIC_NUMBER}-${EPIC_NAME}"
SPEC_DIR="./Docs/Specs/epic-${EPIC_NUMBER}-${EPIC_NAME}"

echo "=== EPIC ARCHIVING ROLLBACK ==="
echo "Epic: #${EPIC_NUMBER} (${EPIC_NAME})"
echo "Archive: $ARCHIVE_DIR"
echo ""

# Step 1: Verify archive exists
if [ ! -d "$ARCHIVE_DIR" ]; then
  echo "ERROR: Archive not found at $ARCHIVE_DIR"
  echo "Nothing to rollback"
  exit 1
fi

echo "Step 1: Archive found - proceeding with rollback"

# Step 2: Restore specs directory
if [ -d "$ARCHIVE_DIR/Specs" ] && [ "$(ls -A $ARCHIVE_DIR/Specs)" ]; then
  echo "Step 2: Restoring specs directory..."

  # Recreate original spec directory
  mkdir -p "$SPEC_DIR"

  # Move specs back to original location
  mv "$ARCHIVE_DIR/Specs"/* "$SPEC_DIR/"

  echo "Specs restored to: $SPEC_DIR"
else
  echo "Step 2: No specs to restore (archive Specs empty or missing)"
fi

# Step 3: Restore working directory artifacts
if [ -d "$ARCHIVE_DIR/working-dir" ] && [ "$(ls -A $ARCHIVE_DIR/working-dir)" ]; then
  echo "Step 3: Restoring working directory artifacts..."

  # Move artifacts back to working directory
  find "$ARCHIVE_DIR/working-dir" -mindepth 1 -maxdepth 1 ! -name "README.md" -exec mv {} ./working-dir/ \;

  echo "Working directory artifacts restored"
else
  echo "Step 3: No working directory artifacts to restore"
fi

# Step 4: Ensure working directory README.md present
if [ ! -f "./working-dir/README.md" ]; then
  echo "Step 4: Restoring working directory README.md from git..."
  git checkout HEAD -- working-dir/README.md
fi

# Step 5: Remove archive directory
echo "Step 5: Removing incomplete archive..."
rm -rf "$ARCHIVE_DIR"
echo "Archive removed: $ARCHIVE_DIR"

# Step 6: Remove DOCUMENTATION_INDEX.md entry (manual step)
echo "Step 6: MANUAL ACTION REQUIRED"
echo "Remove Epic #${EPIC_NUMBER} entry from ./Docs/DOCUMENTATION_INDEX.md"
echo ""

# Step 7: Verification
echo "Step 7: Verifying rollback completeness..."

# Verify spec directory restored
if [ -d "$SPEC_DIR" ]; then
  SPEC_COUNT=$(find "$SPEC_DIR" -type f | wc -l)
  echo "✓ Spec directory restored with $SPEC_COUNT files"
else
  echo "⚠ Spec directory not restored (may not have existed originally)"
fi

# Verify working directory has artifacts
WORKING_COUNT=$(ls -1 ./working-dir/ | grep -v README.md | wc -l)
echo "✓ Working directory has $WORKING_COUNT artifacts"

# Verify README.md present
if [ -f "./working-dir/README.md" ]; then
  echo "✓ Working directory README.md present"
else
  echo "⚠ Working directory README.md missing"
fi

# Verify archive removed
if [ ! -d "$ARCHIVE_DIR" ]; then
  echo "✓ Archive directory removed"
else
  echo "✗ Archive directory still exists"
fi

echo ""
echo "=== ROLLBACK COMPLETE ==="
echo "Pre-archiving state restored"
echo "Address validation failures before retrying archiving"
```

**Usage:**
```bash
./rollback-epic-archiving.sh 291 skills-commands
```

---

## Escalation Procedures

### When to Escalate to User

**Immediate Escalation Scenarios:**
1. **Data Loss Detected:** File count discrepancies suggesting lost files
2. **Critical Infrastructure Failure:** Filesystem corruption, disk failure, permission lockout
3. **Ambiguous State:** Unable to determine if archive complete, partial, or failed
4. **Conflict Resolution Required:** Duplicate archives, conflicting epic numbers, ownership disputes

**Escalation Message Template:**
```
EPIC ARCHIVING BLOCKED - USER INTERVENTION REQUIRED

Epic: #{EPIC_NUMBER} ({EPIC_NAME})
Phase: {CURRENT_PHASE}
Issue: {SPECIFIC_PROBLEM}

Current State:
- Spec Directory: {STATE}
- Working Directory: {STATE}
- Archive Directory: {STATE}

Failure Details:
{DETAILED_ERROR_DESCRIPTION}

Recovery Options:
A) {OPTION_1_DESCRIPTION}
B) {OPTION_2_DESCRIPTION}
C) {OPTION_3_DESCRIPTION}

Recommended Action: {RECOMMENDATION}

User decision required to proceed.
```

### Automated Recovery vs. Manual Intervention

**Automated Recovery Appropriate:**
- Single file missing (restorable from git or backup)
- Broken link (correctable via path adjustment)
- README.md accidentally moved (restorable from archive)
- Empty archive directory (safe to remove and retry)

**Manual Intervention Required:**
- Multiple files missing with no backup
- Filesystem permission issues requiring elevated privileges
- Conflicting archives requiring business logic decision
- Data integrity concerns with unclear recovery path

---

## Related Resources

**Skill Sections:**
- Phase 1: Pre-Completion Validation (prevent issues before starting)
- Phase 8: Final Validation (detect issues before finalizing)
- Troubleshooting Section: Common issues with solutions

**Other Resources:**
- [validation-checklist.md](./validation-checklist.md) - Comprehensive validation procedures
- [archiving-procedures.md](./archiving-procedures.md) - Standard archiving operations
- [epic-291-archive-example.md](../examples/epic-291-archive-example.md) - Complete successful archiving workflow

**Standards References:**
- [TaskManagementStandards.md Section 10](../../../../../../../Docs/Standards/TaskManagementStandards.md#10-epic-completion-workflow) - Epic completion workflow
- [DocumentationStandards.md Section 7](../../../../../../../Docs/Standards/DocumentationStandards.md#7-epic-archiving-standards) - Epic archiving standards
