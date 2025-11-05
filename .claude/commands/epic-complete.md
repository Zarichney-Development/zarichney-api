---
description: "Archive completed epic specs and working directory with validation"
argument-hint: "<epic-number> [--dry-run] [--skip-validation]"
category: "workflow"
requires-skills: ["epic-completion"]
---

# /epic-complete

Systematic epic archiving with comprehensive validation, documentation generation, and clean workspace restoration—all through a single CLI command.

## Purpose

Transforms epic completion from manual, error-prone archiving into a disciplined 8-phase workflow that preserves comprehensive historical context while maintaining clean active workspace directories. Eliminates inconsistent archiving, prevents context loss, reduces workspace clutter, and ensures documentation continuity.

**Target Users:** Codebase Manager (Claude) and developers completing epics with closed issues, merged PRs, and passed ComplianceOfficer validation

**Time Savings:** 80-90% reduction in manual archiving operations, eliminating 20-30 minutes of error-prone file moves, documentation generation, and validation checks

## Usage Examples

### Example 1: Standard Epic Completion (Most Common)

```bash
/epic-complete 291
```

**Expected Output:**
```
🗂️  Epic Completion: Archiving Epic #291

📋 Pre-Flight Checks:
✅ Spec directory found: ./Docs/Specs/epic-291-skills-commands/ (6 files)
✅ Working directory has artifacts: 88 files
✅ Archive directory available: ./Docs/Archive/epic-291-skills-commands/
✅ DOCUMENTATION_INDEX.md exists and writable

🔄 Executing 8-Phase Archiving Workflow:

Phase 1: Pre-Completion Validation
  Validating issue closure...
    ✅ All epic issues closed (0 open issues)
  Validating PR integration...
    ✅ All section PRs merged
    ✅ No merge conflicts detected
  Validating quality gates...
    ✅ Build passes with zero errors/warnings
    ✅ Test suite: 99.8% pass rate (2,847 / 2,852 tests)
    ✅ ComplianceOfficer: GO decision confirmed
  Validating documentation currency...
    ✅ Module READMEs updated
    ✅ Standards documents current
    ✅ No broken links detected
  Validating performance metrics...
    ✅ All performance targets met
    ✅ Token efficiency documented: 50-51% context reduction
    ✅ ROI calculation complete: 144-328% productivity gains

  ✅ All 21 pre-completion criteria passed

Phase 2: Archive Directory Creation
  Creating archive structure...
    ✅ Created ./Docs/Archive/epic-291-skills-commands/
    ✅ Created Specs/ subdirectory
    ✅ Created working-dir/ subdirectory

  ✅ Archive directory structure ready

Phase 3: Specs Archiving
  Moving spec directory contents...
    ✅ Moved 6 files to archive Specs/
       - README.md
       - iteration-1-foundation-spec.md
       - iteration-2-skills-implementation-spec.md
       - iteration-3-documentation-alignment-spec.md
       - iteration-4-agent-refactoring-spec.md
       - iteration-5-integration-testing-spec.md
    ✅ Removed original spec directory

  ✅ Specs archiving complete

Phase 4: Working Directory Archiving
  Inventorying artifacts...
    Found 88 artifacts across 5 iterations
  Moving artifacts to archive...
    ✅ Moved 88 artifacts to archive working-dir/
       - Execution plans: 23 files
       - Completion reports: 18 files
       - Validation reports: 12 files
       - Coordination artifacts: 35 files
    ✅ Preserved working-dir/README.md in active workspace

  ✅ Working directory archiving complete

Phase 5: Archive Documentation Generation
  Gathering epic metadata...
    Epic: #291 Skills & Commands Coordination System
    Completion: 2025-10-27
    Iterations: 5 iterations across 6 issues
    Performance: 50-51% context reduction, 144-328% token savings
  Generating comprehensive README...
    ✅ Executive summary with performance achievements
    ✅ Iterations overview with issue references
    ✅ Key deliverables (10 skills, 3 commands, 12 agent refactors)
    ✅ Performance documentation links
    ✅ Documentation network integration
    ✅ Archive navigation guidance

  ✅ Archive README generated (6 sections, 487 lines)

Phase 6: Documentation Index Update
  Locating DOCUMENTATION_INDEX.md...
    ✅ Found at ./Docs/DOCUMENTATION_INDEX.md
  Updating Completed Epics section...
    ✅ Added Epic #291 entry with archive link
    ✅ Included completion date: 2025-10-27
    ✅ Added one-line summary with key metrics
    ✅ Linked performance documentation

  ✅ DOCUMENTATION_INDEX.md updated successfully

Phase 7: Cleanup Verification
  Verifying original directories cleaned...
    ✅ Spec directory removed successfully
    ✅ Working directory cleaned (only README.md remains)
  Verifying archive integrity...
    ✅ Archive structure complete (Specs/, working-dir/, README.md)
    ✅ All 6 spec files present in archive
    ✅ All 88 artifacts present in archive
  Checking for orphaned files...
    ✅ No orphaned files in ./Docs/ root
    ✅ No unexpected files in ./Docs/Specs/

  ✅ Cleanup verification passed

Phase 8: Final Validation
  Validating archive integrity...
    ✅ Archive structure matches DocumentationStandards.md Section 7
    ✅ All expected spec files present (6 files)
    ✅ All expected artifacts present (88 files)
    ✅ Archive README comprehensive (6 sections)
  Validating cleanup completeness...
    ✅ Original spec directory removed
    ✅ Working directory contains only README.md
    ✅ No orphaned files detected
  Validating documentation integration...
    ✅ DOCUMENTATION_INDEX.md updated with Completed Epics entry
    ✅ Archive README links functional
    ✅ Documentation network integrity maintained

  ✅ All 17 post-completion criteria passed

✅ Epic #291 Archiving Complete!

📂 Archive Location: ./Docs/Archive/epic-291-skills-commands/

📊 Files Archived:
   - Specs: 6 files
   - Working Directory: 88 artifacts (5 categories)
   - Archive README: 1 file (487 lines, 6 sections)

📋 Documentation Integration:
   - DOCUMENTATION_INDEX.md updated with Completed Epics entry
   - Archive README links to Epic291PerformanceAchievements.md
   - All documentation network links functional

📈 Epic Performance Summary:
   - Context Reduction: 50-51% (documentation-grounding skill)
   - Token Savings: 144-328% (session-level efficiency)
   - ROI Calculation: 77.6% productivity gain
   - Skills Created: 10 coordination/documentation/meta skills
   - Commands Created: 3 workflow automation commands
   - Agents Refactored: 12 agents with systematic skill integration

📋 Next Steps:
1. Review archive README: ./Docs/Archive/epic-291-skills-commands/README.md
2. Verify DOCUMENTATION_INDEX.md entry accuracy
3. Create final section PR (section/iteration-5 → epic → main)
4. ComplianceOfficer validates archiving completeness
5. Celebrate Epic #291 completion and document lessons learned

💡 Tip: Epic #291 created the meta-skills that created the skill/command that archived Epic #291 itself—a recursive meta-achievement!
```

### Example 2: Dry Run (Preview Operations)

```bash
/epic-complete 291 --dry-run
```

**Expected Output:**
```
🗂️  Epic Completion: DRY RUN for Epic #291

📋 Pre-Flight Checks:
✅ Spec directory found: ./Docs/Specs/epic-291-skills-commands/ (6 files)
✅ Working directory has artifacts: 88 files
✅ Archive directory available: ./Docs/Archive/epic-291-skills-commands/
✅ DOCUMENTATION_INDEX.md exists and writable

🔍 Operations that WOULD be executed:

Phase 1: Pre-Completion Validation
  → Would validate 21 criteria across 5 categories:
     - Issue closure validation (4 checks)
     - PR integration validation (4 checks)
     - Quality validation (5 checks)
     - Documentation currency validation (4 checks)
     - Performance validation (4 checks)
  → Would verify all issues closed, PRs merged, tests passing
  → Would confirm ComplianceOfficer GO decision

Phase 2: Archive Directory Creation
  → Would create ./Docs/Archive/epic-291-skills-commands/
  → Would create Specs/ and working-dir/ subdirectories
  → Would verify directory structure matches DocumentationStandards.md Section 7

Phase 3: Specs Archiving
  → Would move 6 files from ./Docs/Specs/epic-291-skills-commands/ to archive Specs/
     - README.md
     - iteration-1-foundation-spec.md
     - iteration-2-skills-implementation-spec.md
     - iteration-3-documentation-alignment-spec.md
     - iteration-4-agent-refactoring-spec.md
     - iteration-5-integration-testing-spec.md
  → Would remove original spec directory after successful move

Phase 4: Working Directory Archiving
  → Would move 88 artifacts to archive working-dir/
     - Execution plans: 23 files
     - Completion reports: 18 files
     - Validation reports: 12 files
     - Coordination artifacts: 35 files
  → Would preserve working-dir/README.md in active workspace

Phase 5: Archive Documentation Generation
  → Would gather epic metadata:
     - Epic: #291 Skills & Commands Coordination System
     - Completion: 2025-10-27
     - Iterations: 5 iterations across 6 issues
     - Key deliverables: 10 skills, 3 commands, 12 agent refactors
     - Performance: 50-51% context reduction, 144-328% token savings
  → Would generate comprehensive archive README.md with 6 sections:
     - Executive summary with performance achievements
     - Iterations overview with issue references
     - Key deliverables categorized
     - Documentation network links
     - Archive contents summary
     - Navigation guidance

Phase 6: Documentation Index Update
  → Would add Epic #291 entry to DOCUMENTATION_INDEX.md:
     - Archive link: ./Archive/epic-291-skills-commands/README.md
     - Completion date: 2025-10-27
     - Summary: Skills/commands coordination with 50-51% context reduction
     - Key deliverables: 10 skills, 3 commands, 12 agent refactors
     - Performance docs: Epic291PerformanceAchievements.md

Phase 7: Cleanup Verification
  → Would verify original directories cleaned:
     - Spec directory removed or empty
     - Working directory contains only README.md
  → Would verify archive integrity:
     - All 6 spec files present in archive
     - All 88 artifacts present in archive
     - Archive README comprehensive and complete
  → Would check for orphaned files in unexpected locations

Phase 8: Final Validation
  → Would validate 17 post-completion criteria:
     - Archive integrity validation (5 checks)
     - Cleanup completeness validation (3 checks)
     - Documentation integration validation (5 checks)
     - Quality gates validation (4 checks)
  → Would generate archiving completion summary

💡 No changes made. Remove --dry-run to execute archiving.

📋 Dry Run Summary:
   - 6 spec files ready for archiving
   - 88 working directory artifacts ready for archiving
   - Archive directory available (no conflicts)
   - DOCUMENTATION_INDEX.md ready for update
   - All 21 pre-completion criteria appear valid
   - All 8 phases ready for execution

📋 Next Steps:
1. Review operations summary above
2. Verify file counts match expectations
3. Execute archiving: /epic-complete 291
4. Or skip validation if already verified: /epic-complete 291 --skip-validation
```

### Example 3: Skip Validation (Re-Run After Validation Already Performed)

```bash
/epic-complete 291 --skip-validation
```

**Expected Output:**
```
🗂️  Epic Completion: Archiving Epic #291 (Validation Skipped)

⚠️  Pre-completion validation skipped per --skip-validation flag
    Assuming validation already performed in prior run

📋 Pre-Flight Checks:
✅ Spec directory found: ./Docs/Specs/epic-291-skills-commands/ (6 files)
✅ Working directory has artifacts: 88 files
✅ Archive directory available: ./Docs/Archive/epic-291-skills-commands/

🔄 Executing Archiving Workflow (Phases 2-8):

Phase 2: Archive Directory Creation
  ✅ Archive directory structure ready

Phase 3: Specs Archiving
  ✅ Moved 6 files to archive Specs/
  ✅ Removed original spec directory

Phase 4: Working Directory Archiving
  ✅ Moved 88 artifacts to archive working-dir/
  ✅ Preserved working-dir/README.md

Phase 5: Archive Documentation Generation
  ✅ Archive README generated (6 sections)

Phase 6: Documentation Index Update
  ✅ DOCUMENTATION_INDEX.md updated successfully

Phase 7: Cleanup Verification
  ✅ Cleanup verification passed

Phase 8: Final Validation
  ✅ All 17 post-completion criteria passed

✅ Epic #291 Archiving Complete!

📂 Archive Location: ./Docs/Archive/epic-291-skills-commands/

⚠️  Reminder: Pre-completion validation was skipped
    Ensure all issues closed, PRs merged, and quality gates passed

📋 Next Steps:
1. Review archive README: ./Docs/Archive/epic-291-skills-commands/README.md
2. Verify DOCUMENTATION_INDEX.md entry accuracy
3. ComplianceOfficer validates archiving completeness
```

### Example 4: Preview and Execute Combined

```bash
# First: Preview operations
/epic-complete 291 --dry-run

# Review output, verify operations correct

# Then: Execute archiving with validation skipped (already reviewed)
/epic-complete 291 --skip-validation
```

### Example 5: Dry Run Without Validation

```bash
/epic-complete 291 --dry-run --skip-validation
```

**Expected Output:**
```
🗂️  Epic Completion: DRY RUN for Epic #291 (Validation Skipped)

📋 Pre-Flight Checks:
✅ Spec directory found: ./Docs/Specs/epic-291-skills-commands/ (6 files)
✅ Working directory has artifacts: 88 files
✅ Archive directory available: ./Docs/Archive/epic-291-skills-commands/

🔍 Operations that WOULD be executed (Phases 2-8):

Phase 2: Archive Directory Creation
  → Would create archive structure

Phase 3: Specs Archiving
  → Would move 6 spec files to archive

Phase 4: Working Directory Archiving
  → Would move 88 artifacts to archive

Phase 5: Archive Documentation Generation
  → Would generate archive README.md

Phase 6: Documentation Index Update
  → Would update DOCUMENTATION_INDEX.md

Phase 7: Cleanup Verification
  → Would verify directories cleaned

Phase 8: Final Validation
  → Would validate 17 post-completion criteria

💡 No changes made. Remove --dry-run to execute archiving.
⚠️  Note: Pre-completion validation skipped per --skip-validation flag
```

## Prerequisites

Before invoking `/epic-complete`, ensure the following conditions are met:

### Epic Completion Requirements
- [ ] All epic issues closed with acceptance criteria met
- [ ] All section PRs merged to epic branch (or final section PR ready)
- [ ] Epic branch merged to main (or ready for final PR)
- [ ] Build passes with zero warnings/errors
- [ ] Test suite passes with >99% executable pass rate
- [ ] ComplianceOfficer validation complete with GO decision
- [ ] All affected module READMEs updated per DocumentationStandards.md
- [ ] Performance documentation committed (if performance-critical epic)

### Epic Artifacts Verification
- [ ] Spec directory exists at `./Docs/Specs/epic-{number}-{name}/`
- [ ] Working directory has artifacts to archive (check `./working-dir/`)
- [ ] DOCUMENTATION_INDEX.md exists and is writable
- [ ] Archive directory does not already exist (no conflicts)

### Environment Requirements
- [ ] Repository clean (no uncommitted changes blocking operations)
- [ ] Sufficient disk space for archive operations
- [ ] Write permissions to `./Docs/Archive/` and `./Docs/DOCUMENTATION_INDEX.md`

**Validation Tip:** Use `--dry-run` flag to verify prerequisites without executing operations

## Arguments

### Required Positional Arguments

#### `<epic-number>` (required)

- **Type:** Integer
- **Position:** 1 (first argument after command)
- **Description:** Epic number to archive (e.g., 291 for Epic #291)
- **Validation:** Must be positive integer, spec directory must exist at `./Docs/Specs/epic-{number}-*`
- **Examples:**
  - `291` - Archive Epic #291
  - `42` - Archive Epic #42
  - `123` - Archive Epic #123

**Error if Missing:**
```
⚠️ Error: Epic number required

Usage: /epic-complete <epic-number> [--dry-run] [--skip-validation]

Example: /epic-complete 291
```

**Error if Invalid:**
```
⚠️ Error: Invalid epic number

The epic number must be a positive integer.
You provided: "foo"

Usage: /epic-complete <epic-number> [--dry-run] [--skip-validation]
Example: /epic-complete 291
```

### Optional Flags

#### `--dry-run` (flag)

- **Default:** `false` (execute archiving operations)
- **Description:** Preview archiving operations without executing file moves, directory creation, or documentation updates
- **Behavior:** Displays comprehensive preview of all 8 phases with file counts, operations, and validation checks
- **Usage:** Include flag to enable dry-run mode, omit to execute archiving
- **Recommended:** Use before first execution to verify operations correct
- **Examples:**
  - `/epic-complete 291 --dry-run` - Preview Epic #291 archiving
  - `/epic-complete 42 --dry-run` - Preview Epic #42 archiving

**Dry Run Output Characteristics:**
- Shows "DRY RUN" header prominently
- Uses "Would" language for all operations (e.g., "Would move 6 files...")
- Displays file counts and categories without modifications
- Reports "No changes made" at end
- Provides command to execute: "Remove --dry-run to execute archiving"

#### `--skip-validation` (flag)

- **Default:** `false` (execute Phase 1 pre-completion validation)
- **Description:** Skip Phase 1 validation checks (21 criteria across 5 categories)
- **Behavior:** Proceeds directly to Phase 2 (archive directory creation), bypassing issue closure, PR integration, quality, documentation, and performance validation
- **Usage:** Include flag when validation already performed in prior run or when re-running archiving after interruption
- **Caution:** Only use if confident all completion criteria met
- **Examples:**
  - `/epic-complete 291 --skip-validation` - Execute archiving without validation
  - `/epic-complete 42 --skip-validation` - Re-run archiving after prior validation

**Skip Validation Warnings:**
- Output displays "Validation Skipped" prominently
- Includes reminder: "Assuming validation already performed in prior run"
- Final output reminds: "Pre-completion validation was skipped"

### Flag Combinations

#### Dry Run + Skip Validation

```bash
/epic-complete 291 --dry-run --skip-validation
```

**Use Case:** Preview archiving operations (Phases 2-8) without validation overhead
**Output:** Shows operations preview without validation checks
**Benefit:** Faster preview for re-runs or when validation already confirmed

**Combination Behavior:**
- `--dry-run` takes precedence (no operations executed)
- `--skip-validation` reduces preview scope (Phases 2-8 only)
- Both flags clearly indicated in output header
- Validation warnings included in dry-run summary

## Workflow

The `/epic-complete` command executes a disciplined 4-phase orchestration workflow:

### Phase 1: Argument Parsing & Validation

**Purpose:** Parse and validate user input before skill delegation

**Operations:**
1. Parse `<epic-number>` as positive integer
2. Detect `--dry-run` and `--skip-validation` flags
3. Validate epic number format (positive integer check)
4. Locate spec directory: `./Docs/Specs/epic-{number}-*` (glob match)
5. Handle multiple matches or no matches with clear error messages
6. Prepare parameters for skill delegation

**Validation Errors:**
- Epic number not provided → Usage error with examples
- Epic number not integer → Format error with correction guidance
- Epic number negative → Validation error requiring positive integer
- Spec directory not found → Directory error with troubleshooting steps
- Multiple spec directories match → Conflict error requiring manual resolution

**Success Criteria:** Epic number valid, spec directory located uniquely, flags parsed correctly

### Phase 2: Pre-Flight Checks

**Purpose:** Verify environment ready for archiving before skill delegation

**Checks Performed:**
1. **Spec Directory Verification**
   - Spec directory exists and accessible
   - Count spec files for user reporting
   - Verify not empty (at least README.md expected)

2. **Working Directory Verification** (unless `--skip-validation`)
   - Working directory has artifacts to archive
   - Count artifacts for user reporting
   - Distinguish between execution plans, reports, coordination artifacts

3. **Archive Availability Check**
   - Archive directory does not already exist
   - No conflicts with prior archiving attempts
   - Parent directory (`./Docs/Archive/`) exists and writable

4. **Documentation Index Verification**
   - DOCUMENTATION_INDEX.md exists at `./Docs/DOCUMENTATION_INDEX.md`
   - File is writable for updates
   - No corruption detected (basic syntax check)

**Pre-Flight Output:**
```
📋 Pre-Flight Checks:
✅ Spec directory found: ./Docs/Specs/epic-291-skills-commands/ (6 files)
✅ Working directory has artifacts: 88 files
✅ Archive directory available: ./Docs/Archive/epic-291-skills-commands/
✅ DOCUMENTATION_INDEX.md exists and writable
```

**Error Scenarios:**
- Spec directory not found → See "Spec Directory Not Found" error handling
- Archive already exists → See "Archive Already Exists" error handling
- DOCUMENTATION_INDEX.md missing → See "Documentation Index Missing" error handling

**Success Criteria:** All pre-flight checks pass, environment ready for archiving

### Phase 3: Skill Delegation

**Purpose:** Load epic-completion skill and delegate 8-phase archiving workflow

**Delegation Pattern:**
```yaml
COMMAND: Parse args → Validate environment → Load skill → Pass parameters → Monitor execution
SKILL: Receive parameters → Execute 8 phases → Return structured results → Report errors
```

**Skill Loading:**
```markdown
## Epic Completion Workflow
**SKILL REFERENCE**: `.claude/skills/coordination/epic-completion/`

The command delegates all business logic to the epic-completion skill:
- Phase 1-8 workflow execution
- Validation checklists (21 pre-completion, 17 post-completion)
- File operations (create, move, generate)
- Documentation updates (README generation, index update)
- Error recovery procedures

[Skill instructions loaded on-demand from SKILL.md]
```

**Parameters Passed to Skill:**
- `epic_number`: Integer epic number (e.g., 291)
- `spec_directory_path`: Absolute path to spec directory
- `archive_directory_path`: Absolute path where archive will be created
- `dry_run`: Boolean flag (true = preview only, false = execute)
- `skip_validation`: Boolean flag (true = skip Phase 1, false = full validation)

**Skill Execution:** Epic-completion skill executes 8-phase archiving workflow:

**Phase 1: Pre-Completion Validation** (skipped if `--skip-validation`)
- Issue closure validation (4 checks)
- PR integration validation (4 checks)
- Quality validation (5 checks)
- Documentation currency validation (4 checks)
- Performance validation (4 checks)
- Total: 21 validation criteria across 5 categories

**Phase 2: Archive Directory Creation**
- Create archive root: `./Docs/Archive/epic-{number}-{name}/`
- Create Specs subdirectory: `$ARCHIVE/Specs/`
- Create working-dir subdirectory: `$ARCHIVE/working-dir/`
- Verify directory structure matches DocumentationStandards.md Section 7

**Phase 3: Specs Archiving**
- Move all spec files from `./Docs/Specs/epic-{number}-{name}/` to `$ARCHIVE/Specs/`
- Preserve spec directory structure in archive
- Remove original spec directory after successful move
- Verify all files moved (no orphaned files)

**Phase 4: Working Directory Archiving**
- Inventory all working directory artifacts (count, categorize)
- Move all artifacts except README.md to `$ARCHIVE/working-dir/`
- Preserve working directory structure in archive
- Restore working-dir/README.md if accidentally moved
- Verify working directory cleaned (only README.md remains)

**Phase 5: Archive Documentation Generation**
- Gather epic metadata (number, name, completion date, iterations, issues, deliverables)
- Generate comprehensive archive README.md with 6 sections:
  1. Executive summary with performance achievements
  2. Iterations overview with issue references
  3. Key deliverables (outcomes, performance, quality)
  4. Documentation network links
  5. Archive contents summary
  6. Navigation guidance
- Write README to archive root
- Verify README comprehensive and well-structured

**Phase 6: Documentation Index Update**
- Locate DOCUMENTATION_INDEX.md at `./Docs/DOCUMENTATION_INDEX.md`
- Create or update "Completed Epics" section
- Add epic entry with archive link, completion date, summary, deliverables
- Link performance documentation (if applicable)
- Verify entry formatting consistent with template

**Phase 7: Cleanup Verification**
- Verify original spec directory removed or empty
- Verify working directory cleaned (only README.md remains)
- Verify archive structure complete (Specs/, working-dir/, README.md)
- Count archived files for verification (spec count, artifact count)
- Check for orphaned files in unexpected locations

**Phase 8: Final Validation**
- Archive integrity validation (5 checks)
- Cleanup completeness validation (3 checks)
- Documentation integration validation (5 checks)
- Quality gates validation (4 checks)
- Total: 17 post-completion validation criteria

**Progress Reporting:**
Command displays real-time progress during skill execution:
```
🔄 Executing 8-Phase Archiving Workflow:

Phase 1: Pre-Completion Validation
  Validating issue closure...
    ✅ All epic issues closed (0 open issues)
  Validating PR integration...
    ✅ All section PRs merged
  ...

Phase 2: Archive Directory Creation
  Creating archive structure...
    ✅ Created ./Docs/Archive/epic-291-skills-commands/
  ...

[Continues through all 8 phases with detailed status updates]
```

**Error Handling:**
- Skill throws structured errors with recovery guidance
- Command catches errors and formats user-friendly messages
- Command never bypasses skill business logic
- All error recovery delegated to skill procedures

**Success Criteria:** Skill completes all 8 phases successfully, returns structured results

### Phase 4: Results Reporting

**Purpose:** Format skill results into comprehensive user-friendly completion report

**Report Structure:**

**1. Completion Status Header**
```
✅ Epic #291 Archiving Complete!
```

**2. Archive Location**
```
📂 Archive Location: ./Docs/Archive/epic-291-skills-commands/
```

**3. Files Archived Summary**
```
📊 Files Archived:
   - Specs: 6 files
   - Working Directory: 88 artifacts (5 categories)
   - Archive README: 1 file (487 lines, 6 sections)
```

**4. Documentation Integration Confirmation**
```
📋 Documentation Integration:
   - DOCUMENTATION_INDEX.md updated with Completed Epics entry
   - Archive README links to Epic291PerformanceAchievements.md
   - All documentation network links functional
```

**5. Epic Performance Summary** (if performance-critical epic)
```
📈 Epic Performance Summary:
   - Context Reduction: 50-51% (documentation-grounding skill)
   - Token Savings: 144-328% (session-level efficiency)
   - ROI Calculation: 77.6% productivity gain
   - Skills Created: 10 coordination/documentation/meta skills
   - Commands Created: 3 workflow automation commands
   - Agents Refactored: 12 agents with systematic skill integration
```

**6. Next Steps Guidance**
```
📋 Next Steps:
1. Review archive README: ./Docs/Archive/epic-291-skills-commands/README.md
2. Verify DOCUMENTATION_INDEX.md entry accuracy
3. Create final section PR (section/iteration-5 → epic → main)
4. ComplianceOfficer validates archiving completeness
5. Celebrate epic completion and document lessons learned
```

**7. Special Notes** (if applicable)
```
💡 Tip: Epic #291 created the meta-skills that created the skill/command that archived Epic #291 itself—a recursive meta-achievement!
```

**Report Formatting:**
- Emoji indicators for visual clarity (✅, 📂, 📊, 📋, 📈, 💡)
- Hierarchical structure with clear sections
- File counts and categories for transparency
- Absolute paths for easy navigation
- Actionable next steps with specific commands/files

**Success Criteria:** User understands exactly what was archived, where to find it, and what to do next

## Output Format

### Standard Execution Output

```
🗂️  Epic Completion: Archiving Epic #291

📋 Pre-Flight Checks:
[4 check results with file counts]

🔄 Executing 8-Phase Archiving Workflow:

Phase 1: Pre-Completion Validation
[21 validation criteria results]

Phase 2: Archive Directory Creation
[Directory creation confirmation]

Phase 3: Specs Archiving
[File move operations with counts]

Phase 4: Working Directory Archiving
[Artifact move operations with categories]

Phase 5: Archive Documentation Generation
[README generation with section counts]

Phase 6: Documentation Index Update
[Index update confirmation]

Phase 7: Cleanup Verification
[Cleanup verification results]

Phase 8: Final Validation
[17 post-completion validation results]

✅ Epic #291 Archiving Complete!

[Comprehensive completion report with 7 sections]
```

### Dry-Run Output

```
🗂️  Epic Completion: DRY RUN for Epic #291

📋 Pre-Flight Checks:
[4 check results with file counts]

🔍 Operations that WOULD be executed:

Phase 1: Pre-Completion Validation
  → Would validate 21 criteria across 5 categories
  → [Preview of validation checks]

Phase 2: Archive Directory Creation
  → Would create [archive paths]
  → Would verify directory structure

[Phases 3-8 with "Would" language for all operations]

💡 No changes made. Remove --dry-run to execute archiving.

📋 Dry Run Summary:
[File counts, validation status, readiness confirmation]
```

### Skip-Validation Output

```
🗂️  Epic Completion: Archiving Epic #291 (Validation Skipped)

⚠️  Pre-completion validation skipped per --skip-validation flag
    Assuming validation already performed in prior run

📋 Pre-Flight Checks:
[Reduced checks without validation]

🔄 Executing Archiving Workflow (Phases 2-8):

[Phases 2-8 execution without Phase 1]

✅ Epic #291 Archiving Complete!

⚠️  Reminder: Pre-completion validation was skipped
    Ensure all issues closed, PRs merged, and quality gates passed

[Completion report with validation reminder]
```

### Combined Flags Output (Dry-Run + Skip-Validation)

```
🗂️  Epic Completion: DRY RUN for Epic #291 (Validation Skipped)

📋 Pre-Flight Checks:
[Reduced pre-flight checks]

🔍 Operations that WOULD be executed (Phases 2-8):

[Phases 2-8 preview without validation]

💡 No changes made. Remove --dry-run to execute archiving.
⚠️  Note: Pre-completion validation skipped per --skip-validation flag
```

## Error Handling

### Error 1: Epic Number Missing

```
⚠️ Error: Epic number required

The command requires an epic number as the first argument.

Usage: /epic-complete <epic-number> [--dry-run] [--skip-validation]

Examples:
  /epic-complete 291                    # Archive Epic #291
  /epic-complete 291 --dry-run          # Preview Epic #291 archiving
  /epic-complete 291 --skip-validation  # Archive without validation

💡 Tip: Epic number should match GitHub epic issue number (e.g., 291 for Epic #291)
```

### Error 2: Invalid Epic Number Format

```
⚠️ Error: Invalid epic number

The epic number must be a positive integer.
You provided: "foo"

Valid Examples:
  /epic-complete 291    # Archive Epic #291
  /epic-complete 42     # Archive Epic #42
  /epic-complete 123    # Archive Epic #123

Usage: /epic-complete <epic-number> [--dry-run] [--skip-validation]
```

### Error 3: Negative Epic Number

```
⚠️ Error: Invalid epic number

The epic number must be a positive integer (greater than 0).
You provided: -5

Valid Examples:
  /epic-complete 291    # Archive Epic #291
  /epic-complete 1      # Archive Epic #1
  /epic-complete 999    # Archive Epic #999

Usage: /epic-complete <epic-number> [--dry-run] [--skip-validation]
```

### Error 4: Spec Directory Not Found

```
⚠️ Error: Epic spec directory not found

Could not find spec directory for Epic #291.
Expected: ./Docs/Specs/epic-291-* (no matches)

Possible Causes:
- Epic number incorrect (verify GitHub epic issue number)
- Spec directory already archived (check ./Docs/Archive/)
- Spec directory named differently (check ./Docs/Specs/)

Troubleshooting:
1. List all spec directories: ls -la ./Docs/Specs/
2. Search for epic: find ./Docs/Specs -name "*291*"
3. Check if already archived: ls -la ./Docs/Archive/ | grep 291
4. Verify epic number from GitHub: gh issue view 291

💡 Tip: Spec directory naming convention is epic-{number}-{name}
```

### Error 5: Multiple Spec Directories Match

```
⚠️ Error: Multiple spec directories found for Epic #291

Found 2 matching directories:
  1. ./Docs/Specs/epic-291-skills-commands/
  2. ./Docs/Specs/epic-291-skills-backup/

This command requires a unique spec directory match.

Resolution Options:
1. Manually rename or remove duplicate directories
2. Specify exact directory name (not currently supported)
3. Archive directories separately if legitimately different epics

Recommended Action:
Review both directories and determine which is correct:
  ls -la ./Docs/Specs/epic-291-skills-commands/
  ls -la ./Docs/Specs/epic-291-skills-backup/

Rename or remove the incorrect directory, then retry:
  /epic-complete 291
```

### Error 6: Archive Already Exists

```
⚠️ Error: Archive directory already exists

Archive directory already exists for Epic #291:
Location: ./Docs/Archive/epic-291-skills-commands/

This indicates:
- Epic #291 already archived in prior run
- Incomplete previous archiving attempt
- Duplicate epic number (verify correct epic)

Resolution Options:

1. If prior complete archiving:
   - Verify this is the correct epic number
   - Check archive contents: ls -la ./Docs/Archive/epic-291-skills-commands/
   - If complete, no action needed (epic already archived)

2. If incomplete previous archiving:
   - Backup existing archive: mv ./Docs/Archive/epic-291-skills-commands/ ./Docs/Archive/epic-291-skills-commands-backup/
   - Retry archiving: /epic-complete 291
   - Compare archives after successful retry

3. If duplicate epic number:
   - Verify correct epic number from GitHub: gh issue view 291
   - Adjust epic number and retry with correct value

⚠️  DO NOT overwrite archives without backup!

Manual backup command:
  mv ./Docs/Archive/epic-291-skills-commands/ ./Docs/Archive/epic-291-skills-commands-$(date +%Y%m%d-%H%M%S)/
```

### Error 7: Working Directory Empty (No Artifacts)

```
⚠️ Warning: Working directory empty

Working directory has no artifacts to archive (only README.md found).

Expected Artifacts:
- Execution plans from iteration planning
- Completion reports from agent deliverables
- Validation reports from ComplianceOfficer
- Coordination artifacts from multi-agent workflows

Possible Causes:
- Epic completed without using working directory
- Artifacts already archived in prior run
- Working directory cleaned prematurely

Options:
1. Proceed without working directory archiving:
   - Use --skip-validation to bypass warning
   - Archive will contain only specs (no working-dir artifacts)

2. Verify artifacts not accidentally deleted:
   - Check git history: git log --oneline ./working-dir/
   - Check if artifacts elsewhere: find ./Docs -name "*epic-291*"

3. Recover artifacts if needed:
   - Restore from git history or backups
   - Retry archiving after recovery

Continue anyway? (Ctrl+C to cancel)
  /epic-complete 291 --skip-validation
```

### Error 8: DOCUMENTATION_INDEX.md Not Found

```
⚠️ Error: Documentation index not found

DOCUMENTATION_INDEX.md not found at expected location:
Expected: ./Docs/DOCUMENTATION_INDEX.md

This file is required for epic archiving to update the Completed Epics section.

Resolution:
1. Verify file location: ls -la ./Docs/DOCUMENTATION_INDEX.md
2. Check if moved: find ./Docs -name "DOCUMENTATION_INDEX.md"
3. Create missing file if needed (see DocumentationStandards.md Section 7)

If file exists elsewhere:
  mv [actual-location]/DOCUMENTATION_INDEX.md ./Docs/DOCUMENTATION_INDEX.md

If file missing entirely:
  touch ./Docs/DOCUMENTATION_INDEX.md
  # Then add standard structure per DocumentationStandards.md

Retry after resolving:
  /epic-complete 291
```

### Error 9: Pre-Completion Validation Failures

```
⚠️ Error: Pre-completion validation failed

Epic #291 failed validation checks (Phase 1).

Issue Closure Validation (1 failure):
  ❌ Open issues remaining: 2 issues still open
     Issue #312: Update TestEngineer agent definition
     Issue #315: Add ComplianceOfficer validation

     View: gh issue list --label "epic:skills-commands-291" --state open

Quality Validation (1 failure):
  ❌ Test suite failures: 5 tests failing
     Failure rate: 0.2% (5 / 2,852 tests)

     Run: ./Scripts/run-test-suite.sh report summary

❌ Epic completion blocked until validation passes (21 validation criteria).

Resolution Options:

1. Resolve Validation Failures:
   - Close open issues after completion verification
   - Fix failing tests before archiving
   - Re-run validation: /epic-complete 291

2. Skip Validation (USE WITH CAUTION):
   - Only if failures are known and acceptable
   - Only if re-running after prior validation passed
   - Command: /epic-complete 291 --skip-validation
   - Risk: Archiving incomplete or broken epic

Recommended Action:
Resolve all validation failures before proceeding to ensure complete epic archiving.

💡 Tip: Use --dry-run to preview operations without validation overhead
```

### Error 10: Skill Execution Failure (Mid-Operation)

```
⚠️ Error: Archiving workflow failed at Phase 4

Epic #291 archiving encountered an error during Phase 4: Working Directory Archiving

Error Details:
  Phase: Phase 4 (Working Directory Archiving)
  Operation: Moving artifacts to archive
  Cause: Permission denied writing to ./Docs/Archive/epic-291-skills-commands/working-dir/

Partial Operations Completed:
  ✅ Phase 1: Pre-completion validation passed
  ✅ Phase 2: Archive directory created
  ✅ Phase 3: Specs archived (6 files moved)
  ❌ Phase 4: Working directory archiving FAILED
  ⏸️  Phases 5-8: Not executed (workflow halted)

Current State:
  - Spec directory: ARCHIVED (original removed)
  - Working directory: UNCHANGED (artifacts remain)
  - Archive README: NOT CREATED
  - DOCUMENTATION_INDEX.md: NOT UPDATED

Recovery Steps:

1. Identify Root Cause:
   - Check permissions: ls -la ./Docs/Archive/epic-291-skills-commands/
   - Verify disk space: df -h ./Docs/Archive/
   - Review error details above

2. Fix Environment Issue:
   - Correct permissions: chmod -R u+w ./Docs/Archive/epic-291-skills-commands/
   - Free disk space if needed
   - Verify write access: touch ./Docs/Archive/epic-291-skills-commands/test && rm ./Docs/Archive/epic-291-skills-commands/test

3. Resume or Restart Archiving:

   Option A: Manual Recovery (Recommended)
   - Manually complete Phase 4 operations
   - Run final phases: Archive README, index update
   - Verify with Phase 8 validation checklist

   Option B: Rollback and Retry
   - Restore spec directory from archive
   - Remove incomplete archive
   - Fix root cause and retry: /epic-complete 291

⚠️  DO NOT proceed with partial archiving. Complete recovery before continuing.

Need Help?
Escalate to user with error details and recovery steps above.
```

### Error 11: Archive README Generation Failure

```
⚠️ Error: Archive README generation failed

Epic #291 archiving encountered an error during Phase 5: Archive Documentation Generation

Error Details:
  Phase: Phase 5 (Archive Documentation Generation)
  Operation: Writing archive README.md
  Cause: Unable to gather epic metadata (GitHub API rate limit exceeded)

Impact:
  - Archive structure complete (Specs/, working-dir/)
  - All files archived successfully
  - Archive README missing (cannot generate without metadata)

Workaround:

1. Manual README Creation:
   - Use template: .claude/skills/coordination/epic-completion/resources/templates/archive-readme-template.md
   - Manually gather epic metadata from GitHub issue
   - Create README: ./Docs/Archive/epic-291-skills-commands/README.md

2. Retry After Rate Limit Reset:
   - Check GitHub API rate limit: gh api rate_limit
   - Wait for reset (typically 1 hour)
   - Create README only: (manual operation, not automated)

3. Proceed Without Archive README:
   - Use --skip-validation for phases 6-8
   - Archive functional without README (but less discoverable)

Recommended Action:
Wait for GitHub API rate limit reset, then manually create archive README using template.

Current State:
  ✅ Specs archived (6 files)
  ✅ Working directory archived (88 artifacts)
  ❌ Archive README missing
  ⏸️  Phases 6-8 not executed

Resume options:
  - Manual README creation + manual index update
  - Wait for rate limit reset and retry (full archiving re-run)
```

### Error 12: DOCUMENTATION_INDEX.md Update Failure

```
⚠️ Error: Documentation index update failed

Epic #291 archiving encountered an error during Phase 6: Documentation Index Update

Error Details:
  Phase: Phase 6 (Documentation Index Update)
  Operation: Adding Completed Epics entry
  Cause: DOCUMENTATION_INDEX.md has uncommitted changes (git conflict)

Current State:
  ✅ Archive complete (Specs, working-dir, README)
  ✅ Cleanup verification passed
  ❌ DOCUMENTATION_INDEX.md not updated

Resolution:

1. Review Uncommitted Changes:
   git status ./Docs/DOCUMENTATION_INDEX.md
   git diff ./Docs/DOCUMENTATION_INDEX.md

2. Resolve Conflict:

   Option A: Commit existing changes first
   - Review: git diff ./Docs/DOCUMENTATION_INDEX.md
   - Commit: git add ./Docs/DOCUMENTATION_INDEX.md && git commit -m "docs: update documentation index"
   - Retry: /epic-complete 291 --skip-validation

   Option B: Stash changes temporarily
   - Stash: git stash push ./Docs/DOCUMENTATION_INDEX.md
   - Retry: /epic-complete 291 --skip-validation
   - Apply stash: git stash pop

   Option C: Manual index update
   - Add Epic #291 entry manually to DOCUMENTATION_INDEX.md
   - Use template: .claude/skills/coordination/epic-completion/resources/templates/documentation-index-entry-template.md
   - Format:
     ### Epic #291: Skills & Commands Coordination System
     **Archive:** [./Archive/epic-291-skills-commands/](./Archive/epic-291-skills-commands/README.md)
     **Completion:** 2025-10-27
     **Summary:** Skills/commands coordination with 50-51% context reduction
     **Key Deliverables:** 10 skills, 3 commands, 12 agent refactors

Workaround Complete:
Archive is functional and complete. DOCUMENTATION_INDEX.md can be updated manually or via retry after resolving git conflict.

💡 Tip: Commit all changes before epic archiving to avoid conflicts
```

## Integration with Epic-Completion Skill

### Command-Skill Separation Philosophy

**Command Responsibilities (CLI Interface Layer):**
- Argument parsing and validation (epic number, flags)
- User-friendly error messages with actionable recovery steps
- Output formatting and presentation (progress updates, completion reports)
- Pre-flight environment checks (spec directory exists, no archive conflicts)

**Skill Responsibilities (Business Logic Layer):**
- 8-phase archiving workflow execution (validation → archiving → documentation → verification)
- Validation checklists (21 pre-completion, 17 post-completion criteria)
- File operations (directory creation, file moves, README generation)
- Documentation updates (archive README, DOCUMENTATION_INDEX.md entry)
- Error recovery procedures and rollback strategies

### Delegation Pattern

```
USER INPUT: /epic-complete 291 --dry-run

⬇️ COMMAND LAYER ⬇️
1. Parse arguments: epic_number=291, dry_run=true, skip_validation=false
2. Validate: Epic number positive integer ✅
3. Locate spec directory: ./Docs/Specs/epic-291-skills-commands/ ✅
4. Pre-flight checks: Archive available, index writable ✅
5. Load skill: .claude/skills/coordination/epic-completion/SKILL.md

⬇️ SKILL LAYER ⬇️
6. Receive parameters: {epic: 291, spec_path: ..., dry_run: true}
7. Execute Phase 1: Validate 21 pre-completion criteria
8. Execute Phase 2-8: Preview operations (dry-run mode)
9. Return structured results: {status: "preview", files: {...}, operations: [...]}

⬆️ COMMAND LAYER ⬆️
10. Format results: User-friendly dry-run output with "Would" language
11. Display preview: File counts, operations, validation status
12. Provide guidance: "Remove --dry-run to execute archiving"

USER OUTPUT: Comprehensive dry-run preview with actionable next steps
```

### Error Boundary Contract

**Command Catches:**
- Argument parsing errors → User-friendly format/validation errors
- Pre-flight check failures → Environment setup guidance
- Skill loading failures → Skill availability errors

**Skill Throws:**
- Validation failures → Structured error with failed criteria and recovery steps
- File operation failures → Detailed error with partial state and rollback procedures
- Documentation errors → Specific error with workaround guidance

**Command Never:**
- Bypasses skill business logic (all archiving logic in skill)
- Implements file operations directly (delegates to skill)
- Makes archiving decisions (skill owns workflow)

### Skill Integration Verification

**Command-Skill Contract Validation:**

1. **Parameter Passing:**
   - Command passes: epic_number, spec_directory_path, archive_directory_path, dry_run, skip_validation
   - Skill receives: All parameters with correct types and values
   - Verification: Skill echoes parameters in Phase 1 output

2. **Progress Reporting:**
   - Skill reports: Phase number, operation description, status (✅/❌)
   - Command displays: Real-time progress updates with emoji indicators
   - Verification: User sees 8 phase headers with operation details

3. **Error Handling:**
   - Skill throws: Structured error with phase, operation, cause, recovery
   - Command catches: Formats user-friendly error with troubleshooting steps
   - Verification: All error scenarios testable with error injection

4. **Results Return:**
   - Skill returns: {status, files_archived, operations_completed, validation_results}
   - Command formats: Completion report with file counts, next steps
   - Verification: Completion report includes all expected sections

### Skill Resource Access

**Command References Skill Resources:**

**Templates (Archive Documentation):**
- `archive-readme-template.md` - 6-section archive README structure
- `documentation-index-entry-template.md` - DOCUMENTATION_INDEX.md entry format
- `archive-directory-structure.md` - Directory setup specifications

**Examples (Reference Implementations):**
- `epic-291-archive-example.md` - Complete Epic #291 archiving demonstration
- Shows realistic metadata, file counts, performance metrics

**Documentation (Deep Dives):**
- `validation-checklist.md` - 21 pre-completion + 17 post-completion criteria
- `archiving-procedures.md` - Detailed file operations and rollback procedures
- `error-recovery-guide.md` - Common failures and recovery strategies

**Resource Location:** `.claude/skills/coordination/epic-completion/resources/`

**Command Access:** Skill loads resources on-demand, command references in error messages and guidance

### Integration Testing Validation

**Command-Skill Integration Tests:**

1. **Standard Execution:** `/ epic-complete 291` → Verify all 8 phases execute, archive created, index updated
2. **Dry-Run Mode:** `/epic-complete 291 --dry-run` → Verify preview only, no file modifications
3. **Skip Validation:** `/epic-complete 291 --skip-validation` → Verify Phase 1 skipped, Phases 2-8 execute
4. **Combined Flags:** `/epic-complete 291 --dry-run --skip-validation` → Verify preview without validation
5. **Error Scenarios:** Invalid epic number, missing spec directory, archive conflict → Verify error messages actionable
6. **Partial Failure:** Mid-operation error (Phase 4) → Verify partial state documented, recovery guidance provided

**Success Criteria:** All integration tests pass, command-skill contract validated, user experience seamless

## Integration Points

### Primary Dependencies

**epic-completion Skill:**
- Location: `.claude/skills/coordination/epic-completion/SKILL.md`
- Provides: 8-phase archiving workflow, validation checklists, file operations, documentation generation
- Required: Always loaded for command execution
- Version: 1.0.0 (matches command version)

**DocumentationStandards.md:**
- Location: `./Docs/Standards/DocumentationStandards.md`
- Provides: Archive directory structure specifications (Section 7)
- Used For: Archive structure validation, directory naming conventions
- Referenced: Phase 2 (directory creation), Phase 8 (final validation)

**TaskManagementStandards.md:**
- Location: `./Docs/Standards/TaskManagementStandards.md`
- Provides: Epic completion procedures (Section 10)
- Used For: Pre-completion validation checklist, epic lifecycle management
- Referenced: Phase 1 (validation criteria)

### Secondary Dependencies

**DOCUMENTATION_INDEX.md:**
- Location: `./Docs/DOCUMENTATION_INDEX.md`
- Provides: Central documentation navigation hub
- Updated: Phase 6 (Completed Epics section entry)
- Format: Markdown with structured sections

**GitHub CLI (gh):**
- Used For: Issue and PR validation queries (Phase 1)
- Required: For automatic issue/PR validation
- Optional: Manual verification possible if gh unavailable
- Version: 2.0.0+ recommended

**Git:**
- Used For: Repository state validation, history checks
- Required: For all file operations (ensures clean state)
- Used: Pre-flight checks, cleanup verification

### Related Commands

**Integration with Command Ecosystem:**

**/coverage-report:**
- Relationship: Epic completion may reference coverage achievements
- Usage: Document performance metrics in archive README
- Example: Epic #291 achieved 50-51% context reduction via testing excellence

**/workflow-status:**
- Relationship: Verify CI/CD workflows passing before archiving
- Usage: Phase 1 validation (quality gates)
- Example: Ensure all workflows green before epic completion

**/merge-coverage-prs:**
- Relationship: Epic completion often follows PR consolidation
- Usage: Close coverage excellence epics after PR merges
- Example: Merge coverage PRs, then archive epic

**/create-issue:**
- Relationship: Future enhancements documented during archiving
- Usage: Create follow-up issues for lessons learned
- Example: Document improvements discovered during epic execution

### Agent Coordination

**Codebase Manager (Claude):**
- Primary orchestrator of epic completion
- Detects epic completion triggers (issues closed, PRs merged)
- Invokes `/epic-complete` command with appropriate flags
- Reviews completion report and communicates to user

**ComplianceOfficer:**
- Pre-completion validation (Phase 1)
- Final validation after archiving (Phase 8)
- Quality gates enforcement and archiving completeness verification

**DocumentationMaintainer:**
- Archive README review for comprehensiveness
- DOCUMENTATION_INDEX.md entry accuracy verification
- Documentation network integrity validation

### File System Integration

**Directory Structure:**
```
./Docs/
├── Archive/                          # Archive destination
│   └── epic-{number}-{name}/        # Created by command
│       ├── README.md                # Generated Phase 5
│       ├── Specs/                   # Populated Phase 3
│       └── working-dir/             # Populated Phase 4
├── Specs/                           # Spec directory source
│   └── epic-{number}-{name}/        # Archived in Phase 3
├── DOCUMENTATION_INDEX.md           # Updated Phase 6
└── Standards/
    ├── DocumentationStandards.md    # Referenced Phase 2, 8
    └── TaskManagementStandards.md   # Referenced Phase 1

./working-dir/                       # Working directory source
├── README.md                        # Preserved in place
└── [artifacts]                      # Archived Phase 4
```

**File Operations:**
- **Read:** Spec directory, working directory, DOCUMENTATION_INDEX.md
- **Write:** Archive directory (create), archive README (generate), DOCUMENTATION_INDEX.md (update)
- **Move:** Spec files, working directory artifacts
- **Delete:** Original spec directory (after successful archive)

### Workflow Integration

**Epic Lifecycle Phases:**
1. Epic Planning → Spec directory creation
2. Epic Execution → Working directory artifact accumulation
3. Epic Completion → `/epic-complete` command invocation
4. Epic Archiving → 8-phase workflow execution
5. Epic Retirement → Clean workspace for next initiative

**Command Timing:**
- **Trigger:** All epic issues closed, PRs merged, ComplianceOfficer GO
- **Execution:** Single CLI command invocation
- **Duration:** 2-5 minutes (depending on file counts, validation complexity)
- **Outcome:** Complete archive with historical preservation

## Tool Dependencies

### Required Tools

**bash:**
- Version: 4.0+ (for array operations, string manipulation)
- Usage: Command implementation language
- Verification: `bash --version`
- Availability: Pre-installed on Linux/macOS, available on Windows via WSL

**find:**
- Version: Any POSIX-compliant version
- Usage: Spec directory location, artifact inventory, orphaned file detection
- Verification: `find --version`
- Availability: Pre-installed on Linux/macOS/WSL

**mv:**
- Version: Any POSIX-compliant version
- Usage: File and directory moves (Specs, working directory artifacts)
- Verification: `mv --version`
- Availability: Pre-installed on Linux/macOS/WSL

**mkdir:**
- Version: Any POSIX-compliant version
- Usage: Archive directory structure creation
- Verification: `mkdir --version`
- Availability: Pre-installed on Linux/macOS/WSL

**ls:**
- Version: Any POSIX-compliant version
- Usage: Directory listing, file counting, pre-flight checks
- Verification: `ls --version`
- Availability: Pre-installed on Linux/macOS/WSL

**cat:**
- Version: Any POSIX-compliant version
- Usage: README generation, file content manipulation
- Verification: `cat --version`
- Availability: Pre-installed on Linux/macOS/WSL

### Optional Tools (Enhanced Features)

**gh (GitHub CLI):**
- Version: 2.0.0+ recommended
- Usage: Issue validation (Phase 1), PR validation (Phase 1), epic metadata gathering (Phase 5)
- Verification: `gh --version`
- Installation:
  - macOS: `brew install gh`
  - Ubuntu: `sudo apt install gh`
  - Windows: `winget install GitHub.cli`
- Authentication: `gh auth login`
- Fallback: Manual validation via GitHub web UI if gh unavailable

**git:**
- Version: 2.20.0+ recommended
- Usage: Repository state verification, history checks, uncommitted changes detection
- Verification: `git --version`
- Installation: Pre-installed on most systems, available via package managers
- Required: For all file operations to ensure clean state

### Data Sources

**Spec Directory:**
- Location: `./Docs/Specs/epic-{number}-{name}/`
- Format: Markdown files (README, iteration specs, templates)
- Usage: Source for Phase 3 archiving
- Validation: Must exist, not empty, unique match

**Working Directory:**
- Location: `./working-dir/`
- Format: Mixed (execution plans, reports, artifacts)
- Usage: Source for Phase 4 archiving
- Validation: Should have artifacts (warning if empty)

**DOCUMENTATION_INDEX.md:**
- Location: `./Docs/DOCUMENTATION_INDEX.md`
- Format: Markdown with structured sections
- Usage: Target for Phase 6 update (Completed Epics entry)
- Validation: Must exist, writable, not corrupted

**Epic Metadata:**
- Source: GitHub API (via gh CLI), spec directory README
- Format: Issue numbers, dates, deliverables, performance metrics
- Usage: Archive README generation (Phase 5)
- Fallback: Manual metadata extraction if GitHub API unavailable

### Environment Requirements

**Disk Space:**
- Required: ~10-50 MB per epic (depending on artifact size)
- Verification: `df -h ./Docs/Archive/`
- Recommendation: 1+ GB free space for multiple epic archives

**Permissions:**
- Required: Read/write access to `./Docs/` hierarchy
- Verification: `touch ./Docs/Archive/test && rm ./Docs/Archive/test`
- Common Issue: Permission denied → `chmod -R u+w ./Docs/`

**Repository State:**
- Required: Clean working directory (no uncommitted changes to archiving targets)
- Verification: `git status ./Docs/`
- Recommendation: Commit or stash changes before archiving

## Best Practices

### DO (Recommended Patterns)

✅ **Always Dry-Run First**
```bash
# Preview operations before execution
/epic-complete 291 --dry-run

# Review output, verify file counts and operations correct

# Then execute
/epic-complete 291
```
**Why:** Prevents accidental file moves, verifies environment ready, builds confidence

✅ **Validate Epic Completion Criteria Before Archiving**
```bash
# Verify all issues closed
gh issue list --label "epic:skills-commands-291" --state open

# Verify all PRs merged
gh pr list --head epic/skills-commands-291 --state merged

# Run tests
./Scripts/run-test-suite.sh report summary

# Then archive
/epic-complete 291
```
**Why:** Ensures comprehensive epic completion before archiving, prevents premature retirement

✅ **Review Archive README After Creation**
```bash
# Complete archiving
/epic-complete 291

# Review generated README
cat ./Docs/Archive/epic-291-skills-commands/README.md

# Verify accuracy, comprehensiveness, links functional
```
**Why:** Archive README is primary navigation hub, must be accurate and comprehensive

✅ **Verify DOCUMENTATION_INDEX.md Entry**
```bash
# Complete archiving
/epic-complete 291

# Verify entry added
grep "Epic #291" ./Docs/DOCUMENTATION_INDEX.md

# Verify link functional
# Navigate to archive via documentation index
```
**Why:** Ensures epic discoverable via central documentation hub

✅ **Use Skip-Validation for Re-Runs Only**
```bash
# First run: Full validation
/epic-complete 291

# If interrupted and need to re-run
/epic-complete 291 --skip-validation
```
**Why:** Validation ensures completeness, skip only when validation already performed

✅ **Commit Archive After Successful Completion**
```bash
# Complete archiving
/epic-complete 291

# Verify archive complete
ls -la ./Docs/Archive/epic-291-skills-commands/

# Commit archive
git add ./Docs/Archive/epic-291-skills-commands/
git add ./Docs/DOCUMENTATION_INDEX.md
git commit -m "docs: archive Epic #291 Skills & Commands Coordination System"
```
**Why:** Preserves archive in repository history, enables version control

### DON'T (Anti-Patterns)

❌ **Don't Archive Incomplete Epics**
```bash
# BAD: Archiving with open issues
/epic-complete 291  # While issues still open

# GOOD: Close all issues first
gh issue list --label "epic:skills-commands-291" --state open
# Close remaining issues, then archive
/epic-complete 291
```
**Why:** Incomplete epic archives lose context, create confusion about completion status

❌ **Don't Skip Validation Without Good Reason**
```bash
# BAD: Skipping validation on first run
/epic-complete 291 --skip-validation  # First archiving attempt

# GOOD: Full validation on first run
/epic-complete 291

# ACCEPTABLE: Skip on re-run after interruption
/epic-complete 291 --skip-validation  # After prior validation passed
```
**Why:** Validation catches issues before archiving, skipping risks incomplete archives

❌ **Don't Manually Move Files After Command Invocation**
```bash
# BAD: Manual file moves after command
/epic-complete 291
mv ./Docs/Specs/epic-291-skills-commands/extra-file.md ./Docs/Archive/epic-291-skills-commands/Specs/

# GOOD: Ensure all files in spec directory before command
ls -la ./Docs/Specs/epic-291-skills-commands/
# Move any extra files first, then archive
/epic-complete 291
```
**Why:** Manual moves after archiving break file counts, verification, and documentation

❌ **Don't Overwrite Existing Archives**
```bash
# BAD: Deleting existing archive without backup
rm -rf ./Docs/Archive/epic-291-skills-commands/
/epic-complete 291

# GOOD: Backup existing archive first
mv ./Docs/Archive/epic-291-skills-commands/ ./Docs/Archive/epic-291-skills-commands-backup-$(date +%Y%m%d)/
/epic-complete 291
```
**Why:** Existing archives may contain valuable historical context not reproducible

❌ **Don't Archive Without Working Directory Artifacts**
```bash
# BAD: Archiving empty working directory
rm -rf ./working-dir/*  # Accidentally deleted artifacts
/epic-complete 291

# GOOD: Verify artifacts present before archiving
ls -la ./working-dir/
# If empty, recover from git history or backups
git log --oneline ./working-dir/
/epic-complete 291
```
**Why:** Working directory artifacts preserve coordination context, execution plans, completion reports

❌ **Don't Modify DOCUMENTATION_INDEX.md Manually During Archiving**
```bash
# BAD: Editing index while command running
/epic-complete 291 &
# While command running...
vi ./Docs/DOCUMENTATION_INDEX.md  # Manual edit

# GOOD: Let command update index
/epic-complete 291
# Then review and adjust entry if needed
vi ./Docs/DOCUMENTATION_INDEX.md
```
**Why:** Concurrent modifications cause git conflicts, incomplete index entries

### Recommended Workflow

**Standard Epic Completion Sequence:**

1. **Pre-Archiving Verification** (5-10 minutes)
   ```bash
   # Verify all issues closed
   gh issue list --label "epic:EPIC_NAME" --state open

   # Verify all PRs merged
   gh pr list --head epic/EPIC_NAME --state merged

   # Run tests
   ./Scripts/run-test-suite.sh report summary

   # ComplianceOfficer validation
   # (Delegate to ComplianceOfficer agent)
   ```

2. **Dry-Run Preview** (30 seconds)
   ```bash
   # Preview archiving operations
   /epic-complete 291 --dry-run

   # Review output: file counts, operations, validation status
   ```

3. **Execute Archiving** (2-5 minutes)
   ```bash
   # Execute full archiving workflow
   /epic-complete 291

   # Monitor progress through 8 phases
   # Verify completion report
   ```

4. **Post-Archiving Verification** (2-3 minutes)
   ```bash
   # Review archive README
   cat ./Docs/Archive/epic-291-skills-commands/README.md

   # Verify DOCUMENTATION_INDEX.md entry
   grep "Epic #291" ./Docs/DOCUMENTATION_INDEX.md

   # Check archive structure
   ls -la ./Docs/Archive/epic-291-skills-commands/
   ```

5. **Commit Archive** (1 minute)
   ```bash
   # Add archive and index update
   git add ./Docs/Archive/epic-291-skills-commands/
   git add ./Docs/DOCUMENTATION_INDEX.md

   # Commit with conventional message
   git commit -m "docs: archive Epic #291 Skills & Commands Coordination System"
   ```

6. **Final PR Creation** (2-3 minutes)
   ```bash
   # Create final section PR (if applicable)
   gh pr create --base epic/skills-commands-291 --title "epic: complete Iteration 5 - Integration Testing (#291)"

   # Or merge epic to main
   gh pr create --base main --title "Epic #291: Skills & Commands Coordination System"
   ```

**Total Time:** 12-20 minutes from pre-verification to committed archive

### Performance Tips

**Optimize Archiving Speed:**
- Use `--skip-validation` for re-runs (saves 1-2 minutes)
- Ensure fast disk I/O for large artifact counts
- Pre-verify GitHub connectivity (gh CLI authenticated)

**Reduce Error Rates:**
- Always dry-run first (catches 90% of environment issues)
- Commit all changes before archiving (prevents git conflicts)
- Verify spec directory naming matches convention

**Improve Archive Quality:**
- Document performance metrics in spec README before archiving
- Ensure working directory has comprehensive artifact coverage
- Review archive README template customizations for epic-specific content

## See Also

### Related Skills

**epic-completion:**
- Location: `.claude/skills/coordination/epic-completion/SKILL.md`
- Purpose: 8-phase archiving workflow business logic
- Usage: Automatically loaded by `/epic-complete` command
- Resources: Templates, examples, validation checklists

**documentation-grounding:**
- Location: `.claude/skills/documentation/documentation-grounding/SKILL.md`
- Purpose: Systematic context loading before archiving
- Usage: Phase 1 validation (documentation currency checks)
- Relevance: Archive README links to grounded documentation

**working-directory-coordination:**
- Location: `.claude/skills/coordination/working-directory-coordination/SKILL.md`
- Purpose: Team communication protocols via working directory
- Usage: Phase 4 artifact archiving (execution plans, reports)
- Relevance: Working directory artifacts preserve coordination context

### Related Commands

**/coverage-report:**
- Purpose: Test coverage analytics and trend tracking
- Relationship: Epic performance metrics (coverage achievements)
- Integration: Archive README references coverage improvements

**/workflow-status:**
- Purpose: GitHub Actions workflow monitoring
- Relationship: Phase 1 validation (CI/CD quality gates)
- Integration: Verify workflows passing before archiving

**/merge-coverage-prs:**
- Purpose: Multi-PR consolidation for coverage excellence
- Relationship: Epic completion often follows PR merges
- Integration: Close coverage epics after consolidation

### Documentation Standards

**DocumentationStandards.md:**
- Location: `./Docs/Standards/DocumentationStandards.md`
- Section: Section 7 (Epic Lifecycle & Archiving)
- Content: Archive directory structure, naming conventions, README requirements
- Usage: Phase 2 directory creation, Phase 8 final validation

**TaskManagementStandards.md:**
- Location: `./Docs/Standards/TaskManagementStandards.md`
- Section: Section 10 (Epic Completion Criteria)
- Content: Pre-completion validation checklist, epic retirement procedures
- Usage: Phase 1 validation (21 criteria across 5 categories)

### Skill Resources

**Templates:**
- `archive-readme-template.md` - 6-section README structure with placeholders
- `documentation-index-entry-template.md` - DOCUMENTATION_INDEX.md entry format
- `archive-directory-structure.md` - Directory setup specifications

**Examples:**
- `epic-291-archive-example.md` - Complete Epic #291 archiving demonstration

**Documentation:**
- `validation-checklist.md` - 21 pre-completion + 17 post-completion criteria
- `archiving-procedures.md` - Detailed file operations and rollback procedures
- `error-recovery-guide.md` - Common failures and recovery strategies

**Location:** `.claude/skills/coordination/epic-completion/resources/`

### GitHub Resources

**Epic Issues:**
- View epic issues: `gh issue list --label "epic:EPIC_NAME"`
- Check epic status: `gh issue view EPIC_NUMBER`
- Close epic: `gh issue close EPIC_NUMBER`

**Epic PRs:**
- View epic PRs: `gh pr list --head epic/EPIC_NAME`
- Check PR status: `gh pr view PR_NUMBER`
- Merge final PR: `gh pr merge PR_NUMBER`

### Additional References

**CLAUDE.md:**
- Section: Section 6 (Orchestration Workflows)
- Content: Epic completion coordination, archiving integration
- Relevance: Codebase Manager invokes `/epic-complete` during epic retirement

**AgentOrchestrationGuide.md:**
- Section: Multi-agent epic coordination patterns
- Content: Epic lifecycle phases, coordination artifacts
- Relevance: Understanding working directory artifact categories

**Epic291PerformanceAchievements.md:**
- Location: `./Docs/Development/Epic291PerformanceAchievements.md`
- Content: Performance metrics, ROI calculation, token efficiency
- Relevance: Example of performance documentation linked in archive README

---

**Command Status:** ✅ **OPERATIONAL**
**Version:** 1.0.0
**Target Users:** Codebase Manager (Claude), Developers completing epics
**Efficiency Gains:** 80-90% reduction in manual archiving time, eliminating 20-30 minutes of error-prone operations
**Progressive Loading:** YAML frontmatter (~100 tokens) → Full command documentation (~4,800 tokens) → Skill delegation (on-demand)
**Recursive Achievement:** Epic #291 created the meta-skills that created this command that archives Epic #291 itself! 🎯
