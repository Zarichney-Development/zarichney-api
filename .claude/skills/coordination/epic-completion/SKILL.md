---
name: epic-completion
description: Systematic framework for archiving completed epic artifacts (specs and working-dir) with validation, documentation generation, and index updates. Use when all epic issues are closed, PRs merged, and ComplianceOfficer validation shows GO decision. Enables consistent epic retirement with comprehensive historical preservation.
---

# Epic Completion Coordination Skill

Systematic framework for archiving completed epic artifacts and transitioning epic lifecycle from active development to historical preservation.

## PURPOSE

### Core Mission
Transform epic completion from manual, error-prone archiving into a disciplined 8-phase workflow that preserves comprehensive historical context while maintaining clean active workspace directories.

### Why This Matters
Without systematic epic completion protocols:
- **Inconsistent Archiving:** Ad-hoc epic retirement leads to missing artifacts, incomplete documentation, broken links
- **Context Loss:** Critical epic learnings and performance achievements scattered across working directory without preservation
- **Workspace Clutter:** Completed epic artifacts remain in active directories, obscuring current work
- **Documentation Drift:** DOCUMENTATION_INDEX.md becomes outdated, epic summaries incomplete or inaccurate

This skill ensures every completed epic follows standardized archiving procedures with comprehensive validation, preserving historical context while enabling clean workspace for next initiative.

### Mandatory Application
- **Required For:** All epic completions with closed issues, merged PRs, and passed ComplianceOfficer validation
- **Authority Scope:** Codebase Manager coordinates archiving (may delegate to `/epic-complete` command)
- **Quality Gate:** No epic closure without completion of all 8 archiving phases
- **Historical Preservation:** Epic archives remain immutable except for critical corrections

---

## WHEN TO USE

### 1. Standard Epic Completion (PRIMARY USE CASE)
**Trigger:** All epic issues closed, all section PRs merged, ComplianceOfficer validation complete with GO decision
**Action:** Execute complete 8-phase archiving workflow from pre-validation through final verification
**Rationale:** Systematic archiving ensures comprehensive historical preservation with validated completeness

### 2. Section-Based Epic Completion (ITERATIVE EPIC)
**Trigger:** Epic structured as sections with final section PR ready for review
**Action:** Execute archiving workflow after final section PR merged, capturing all iterative deliverables
**Rationale:** Section-based epics require complete artifact collection across multiple iterations and PRs

### 3. Performance-Critical Epic Archiving (DOCUMENTATION PRESERVATION)
**Trigger:** Epic achieved significant performance metrics requiring dedicated documentation
**Action:** Execute archiving with enhanced documentation linking to committed performance reports
**Rationale:** Ensures ROI documentation and performance achievements permanently accessible

### 4. Multi-Agent Epic Retirement (COORDINATION INTENSIVE)
**Trigger:** Epic involved 3+ agents with extensive working directory artifacts requiring preservation
**Action:** Execute archiving with comprehensive working-dir artifact collection and team coordination summary
**Rationale:** Multi-agent epics generate extensive coordination artifacts requiring systematic preservation

---

## 8-PHASE WORKFLOW STEPS

### Phase 1: Pre-Completion Validation

**Purpose:** Verify all epic completion criteria met before initiating archiving operations

#### Process

**1. Issue Closure Validation**
Verify all epic issues resolved:
```bash
# List any open issues for epic
gh issue list --label "epic:EPIC_NAME" --state open

# Expected: No open issues
```

**Validation Criteria:**
- [ ] All epic issues closed (query returns no results)
- [ ] All issue acceptance criteria met per GitHub Issue close confirmations
- [ ] All issue deliverables committed to repository
- [ ] No outstanding blockers or dependencies

**2. PR Integration Validation**
Verify all epic PRs merged:
```bash
# List any open PRs for epic branch
gh pr list --base epic/EPIC_NAME --state open

# Check if epic branch merged to main (or final section PR ready)
gh pr list --head epic/EPIC_NAME --state merged
```

**Validation Criteria:**
- [ ] All section PRs merged to epic branch
- [ ] Epic branch merged to main (or final section PR ready for review)
- [ ] All merge conflicts resolved
- [ ] No failed CI/CD checks on epic branch

**3. Quality Validation**
Execute comprehensive quality checks:
```bash
# Build validation
dotnet build zarichney-api.sln

# Test suite validation
./Scripts/run-test-suite.sh report summary

# ComplianceOfficer validation (if not already executed)
# (Delegate to ComplianceOfficer agent for final validation report)
```

**Validation Criteria:**
- [ ] Build passes with zero warnings/errors
- [ ] Test suite passes with >99% executable pass rate
- [ ] ComplianceOfficer validation complete with GO decision
- [ ] All AI Sentinels operational and compatible with epic changes
- [ ] No breaking changes introduced

**4. Documentation Currency Validation**
Verify documentation network up-to-date:

**Validation Criteria:**
- [ ] All affected module READMEs updated per DocumentationStandards.md
- [ ] All standards documents reflect epic changes (if applicable)
- [ ] DOCUMENTATION_INDEX.md includes all epic-related documentation
- [ ] No broken links in documentation network (spot check key docs)

**5. Performance Validation (If Applicable)**
For performance-critical epics, verify metrics documented:

**Validation Criteria:**
- [ ] All performance targets met or exceeded per epic objectives
- [ ] Token efficiency validated and documented (if applicable)
- [ ] Productivity gains quantified with ROI calculation
- [ ] Performance documentation committed to `./Docs/Development/`

#### Pre-Completion Validation Checklist
- [ ] Issue closure validation passed (5 checks)
- [ ] PR integration validation passed (4 checks)
- [ ] Quality validation passed (5 checks)
- [ ] Documentation currency validation passed (4 checks)
- [ ] Performance validation passed (4 checks) OR marked N/A
- [ ] Total: 21 validation criteria reviewed and confirmed

**Resource:** See `resources/documentation/validation-checklist.md` for comprehensive checklist with detailed procedures

---

### Phase 2: Archive Directory Creation

**Purpose:** Establish standardized archive directory structure per DocumentationStandards.md Section 7

#### Process

**1. Create Archive Root Directory**
```bash
# Epic number and name from original spec directory
EPIC_NUMBER=291
EPIC_NAME="skills-commands"
ARCHIVE_DIR="./Docs/Archive/epic-${EPIC_NUMBER}-${EPIC_NAME}"

# Create archive root
mkdir -p "$ARCHIVE_DIR"
```

**2. Create Archive Subdirectories**
```bash
# Create Specs subdirectory for spec directory contents
mkdir -p "$ARCHIVE_DIR/Specs"

# Create working-dir subdirectory for working directory artifacts
mkdir -p "$ARCHIVE_DIR/working-dir"
```

**3. Verify Directory Structure**
```bash
# Expected structure:
# ./Docs/Archive/epic-{number}-{name}/
# ├── Specs/
# └── working-dir/
ls -la "$ARCHIVE_DIR"
```

#### Archive Directory Validation
- [ ] Archive root directory created with correct naming: `epic-{number}-{name}`
- [ ] Specs subdirectory created at `$ARCHIVE_DIR/Specs/`
- [ ] working-dir subdirectory created at `$ARCHIVE_DIR/working-dir/`
- [ ] Directory structure matches DocumentationStandards.md Section 7 specification

**Resource:** See `resources/templates/archive-directory-structure.md` for directory setup template

---

### Phase 3: Specs Archiving

**Purpose:** Move complete spec directory to archive preserving all epic planning and requirements documentation

#### Process

**1. Verify Spec Directory Exists**
```bash
SPEC_DIR="./Docs/Specs/epic-${EPIC_NUMBER}-${EPIC_NAME}"

# Verify spec directory exists
if [ -d "$SPEC_DIR" ]; then
  echo "Found spec directory: $SPEC_DIR"
  ls -la "$SPEC_DIR"
else
  echo "WARNING: Spec directory not found at $SPEC_DIR"
  # Escalate to user for clarification
fi
```

**2. Move Spec Directory Contents to Archive**
```bash
# Move all spec files and subdirectories to archive Specs/
mv "$SPEC_DIR"/* "$ARCHIVE_DIR/Specs/"

# Verify move successful
ls -la "$ARCHIVE_DIR/Specs/"
```

**3. Remove Original Spec Directory**
```bash
# Remove now-empty spec directory
rmdir "$SPEC_DIR"

# Verify removal
if [ ! -d "$SPEC_DIR" ]; then
  echo "Spec directory successfully archived and removed"
fi
```

#### Specs Archiving Validation
- [ ] Original spec directory located and verified
- [ ] All spec files moved to archive Specs/ subdirectory
- [ ] Spec directory structure preserved in archive
- [ ] Original spec directory removed (or empty)
- [ ] No spec files orphaned in unexpected locations

**Resource:** See `resources/documentation/archiving-procedures.md` for detailed move operations and rollback procedures

---

### Phase 4: Working Directory Archiving

**Purpose:** Move all working directory artifacts to archive while preserving working-dir/README.md for next epic

#### Process

**1. Inventory Working Directory Artifacts**
```bash
# List all working directory files (excluding README.md)
ls -la ./working-dir/ | grep -v README.md

# Count artifacts for archive summary
ARTIFACT_COUNT=$(ls -1 ./working-dir/ | grep -v README.md | wc -l)
echo "Found $ARTIFACT_COUNT artifacts to archive"
```

**2. Move Artifacts to Archive (Preserve Structure)**
```bash
# Move all files except README.md to archive working-dir/
find ./working-dir -mindepth 1 -maxdepth 1 ! -name "README.md" -exec mv {} "$ARCHIVE_DIR/working-dir/" \;

# Verify working directory cleaned
ls -la ./working-dir/
```

**3. Verify Working Directory README Preserved**
```bash
# Confirm README.md still present in active workspace
if [ -f "./working-dir/README.md" ]; then
  echo "Working directory README preserved for next epic"
else
  echo "WARNING: Working directory README missing - restore from backup"
fi
```

**4. Restore Working Directory README (If Moved)**
```bash
# If README accidentally moved, restore from archive
if [ -f "$ARCHIVE_DIR/working-dir/README.md" ]; then
  mv "$ARCHIVE_DIR/working-dir/README.md" ./working-dir/
  echo "Restored working directory README to active workspace"
fi
```

#### Working Directory Archiving Validation
- [ ] All working directory artifacts inventoried (count documented)
- [ ] All artifacts except README.md moved to archive working-dir/
- [ ] Working directory structure preserved in archive
- [ ] Working directory cleaned (only README.md remains)
- [ ] No artifacts orphaned in unexpected locations

**Resource:** See `resources/documentation/archiving-procedures.md` for working directory artifact patterns and categories

---

### Phase 5: Archive Documentation Generation

**Purpose:** Create comprehensive archive README.md summarizing epic achievements and providing navigation guidance

#### Process

**1. Gather Epic Metadata**
Collect epic summary information:
- Epic number and full name
- Completion date (current date)
- Total iterations count (from spec directory structure or issue labels)
- Total issues count (from GitHub)
- Key deliverables and achievements
- Performance metrics (if applicable)

**2. Generate Archive README from Template**
Use archive README template to create comprehensive summary:

**Template Structure (from `resources/templates/archive-readme-template.md`):**
```markdown
# Epic #{number}: {Full Name}
**Status:** ARCHIVED
**Completion Date:** YYYY-MM-DD
**Total Iterations:** X iterations
**Total Issues:** Y issues

## Executive Summary
[2-3 sentence overview of epic purpose and outcomes]
[Key performance achievements with quantified metrics]
[Strategic impact statement]

## Iterations Overview
- **Iteration 1**: Issue #X - [One-line summary]
- **Iteration 2**: Issue #Y-Z - [One-line summary]
- **Iteration N**: Issue #N - [One-line summary]

## Key Deliverables
### Major Outcomes
- [Skills created, commands implemented, modules refactored]
- [System capabilities enhanced]
- [Team workflows improved]

### Performance Results
- [Context reduction: X%]
- [Productivity gains: Y min/day]
- [ROI calculation: Z% efficiency improvement]

### Quality Achievements
- [Test coverage improvements]
- [Standards compliance enhancements]
- [Technical debt reduction]

## Documentation Network
### Committed Documentation
- [Link to committed documentation in ./Docs/Development/]
- [Link to related standards updates]
- [Link to module READMEs affected by epic]

### Performance Documentation (if applicable)
- [Link to Epic{N}PerformanceAchievements.md]
- [Link to token tracking methodology]
- [Link to performance monitoring strategy]

## Archive Contents
### Specs Directory
[Summary of spec directory contents - README, iteration specs, templates]

### Working Directory
[Summary of working-dir artifacts - count, categories (execution plans, completion reports, validation reports, coordination artifacts)]

### Navigation Guidance
- **Specs/**: Complete epic planning and requirements documentation
- **working-dir/**: All coordination artifacts from epic execution
- Start with `Specs/README.md` for epic overview and iteration structure
```

**3. Populate Template with Epic-Specific Content**
Fill all template sections with specific epic information collected in Step 1.

**4. Write Archive README to Archive Root**
```bash
# Write completed README to archive root
# (Content prepared in previous step)
cat > "$ARCHIVE_DIR/README.md" <<'EOF'
[Epic-specific README content]
EOF

# Verify README created
ls -la "$ARCHIVE_DIR/README.md"
```

#### Archive Documentation Validation
- [ ] Epic metadata gathered (number, name, dates, counts, deliverables)
- [ ] Archive README generated from template with all 6 sections
- [ ] Executive summary concise and accurate (2-3 sentences)
- [ ] Iterations overview complete with issue references
- [ ] Key deliverables categorized (outcomes, performance, quality)
- [ ] Documentation network links functional and comprehensive
- [ ] Archive contents summary provides clear navigation guidance

**Resource:** See `resources/templates/archive-readme-template.md` for complete template with placeholder guidance

---

### Phase 6: Documentation Index Update

**Purpose:** Update DOCUMENTATION_INDEX.md with "Completed Epics" section linking to archive

#### Process

**1. Locate DOCUMENTATION_INDEX.md**
```bash
INDEX_FILE="./Docs/DOCUMENTATION_INDEX.md"

# Verify index file exists
if [ -f "$INDEX_FILE" ]; then
  echo "Found documentation index: $INDEX_FILE"
else
  echo "WARNING: DOCUMENTATION_INDEX.md not found - create before updating"
fi
```

**2. Prepare Completed Epics Section Entry**
Use documentation index entry template:

**Template Structure (from `resources/templates/documentation-index-entry-template.md`):**
```markdown
## Completed Epics

Archived epics with complete historical context including specs and working directory artifacts.

### Epic #{number}: {Full Name}
**Archive:** [./Archive/epic-{number}-{name}/](./Archive/epic-{number}-{name}/README.md)
**Completion:** YYYY-MM-DD
**Summary:** [One-line executive summary with key performance metrics]
**Key Deliverables:** [Comma-separated major outcomes]
**Performance Documentation:** [Link to Epic{N}PerformanceAchievements.md if applicable]
```

**3. Add or Update Completed Epics Section**
```bash
# Check if "Completed Epics" section exists
if grep -q "## Completed Epics" "$INDEX_FILE"; then
  # Append new epic entry to existing section
  # (Edit tool or manual insertion after "## Completed Epics" section)
else
  # Create new "Completed Epics" section
  # (Append complete section to end of DOCUMENTATION_INDEX.md)
fi
```

**4. Verify DOCUMENTATION_INDEX.md Updated**
```bash
# Verify epic entry present in index
grep "Epic #${EPIC_NUMBER}" "$INDEX_FILE"

# Verify link to archive functional
# (Spot check link format matches archive directory created in Phase 2)
```

#### Documentation Index Validation
- [ ] DOCUMENTATION_INDEX.md located successfully
- [ ] Completed Epics section exists (created if missing)
- [ ] Epic entry added with archive link, completion date, summary
- [ ] Key deliverables and performance documentation linked
- [ ] All links functional (archive README, performance docs)
- [ ] Entry formatting consistent with template structure

**Resource:** See `resources/templates/documentation-index-entry-template.md` for entry format and placeholder guidance

---

### Phase 7: Cleanup Verification

**Purpose:** Verify archiving operations completed successfully with no orphaned files or broken links

#### Process

**1. Verify Original Directories Cleaned**
```bash
# Verify spec directory removed or empty
if [ -d "$SPEC_DIR" ]; then
  echo "WARNING: Spec directory still exists - archiving incomplete"
  ls -la "$SPEC_DIR"
else
  echo "Spec directory successfully removed"
fi

# Verify working directory cleaned (only README.md remains)
WORKING_DIR_FILES=$(ls -1 ./working-dir/ | grep -v README.md | wc -l)
if [ "$WORKING_DIR_FILES" -eq 0 ]; then
  echo "Working directory successfully cleaned"
else
  echo "WARNING: Working directory has $WORKING_DIR_FILES unexpected files"
  ls -la ./working-dir/
fi
```

**2. Verify Archive Integrity**
```bash
# Verify archive directory structure complete
if [ -d "$ARCHIVE_DIR/Specs" ] && [ -d "$ARCHIVE_DIR/working-dir" ] && [ -f "$ARCHIVE_DIR/README.md" ]; then
  echo "Archive structure complete"
else
  echo "WARNING: Archive structure incomplete"
  ls -la "$ARCHIVE_DIR"
fi

# Count archived files for verification
SPEC_FILES=$(find "$ARCHIVE_DIR/Specs" -type f | wc -l)
WORKING_FILES=$(find "$ARCHIVE_DIR/working-dir" -type f | wc -l)
echo "Archived $SPEC_FILES spec files and $WORKING_FILES working directory artifacts"
```

**3. Spot Check Documentation Links**
```bash
# Verify archive README exists and non-empty
if [ -s "$ARCHIVE_DIR/README.md" ]; then
  echo "Archive README verified"
else
  echo "WARNING: Archive README missing or empty"
fi

# Verify DOCUMENTATION_INDEX.md references archive
grep "epic-${EPIC_NUMBER}-${EPIC_NAME}" ./Docs/DOCUMENTATION_INDEX.md
```

**4. Identify Any Orphaned Files**
```bash
# Check for unexpected files in Docs/ root
find ./Docs -maxdepth 1 -type f -name "*${EPIC_NUMBER}*"

# Check for unexpected directories in Specs/
find ./Docs/Specs -maxdepth 1 -type d -name "*${EPIC_NUMBER}*"
```

#### Cleanup Verification Checklist
- [ ] Original spec directory removed or empty
- [ ] Working directory cleaned (only README.md remains)
- [ ] Archive directory structure complete (Specs/, working-dir/, README.md)
- [ ] Archive README non-empty and comprehensive
- [ ] DOCUMENTATION_INDEX.md references archive correctly
- [ ] No orphaned files in unexpected locations
- [ ] Archive file counts documented for reference

**Resource:** See `resources/documentation/error-recovery-guide.md` for cleanup failure scenarios and recovery procedures

---

### Phase 8: Final Validation

**Purpose:** Execute comprehensive post-archiving validation confirming completeness and correctness

#### Process

**1. Archive Integrity Validation**
Execute comprehensive archive structure verification:

**Validation Criteria:**
- [ ] Archive directory structure matches DocumentationStandards.md Section 7 specification
- [ ] All expected spec files present in archive Specs/ (count matches pre-archiving inventory)
- [ ] All expected working directory artifacts present in archive working-dir/ (count matches Phase 4 inventory)
- [ ] Archive README comprehensive with all 6 required sections populated
- [ ] No missing files or broken directory structure

**2. Cleanup Completeness Validation**
Verify original directories properly cleaned:

**Validation Criteria:**
- [ ] Original spec directory removed (or empty if removal failed)
- [ ] Working directory contains only README.md (no other artifacts)
- [ ] No orphaned files in `./Docs/` root or unexpected locations

**3. Documentation Integration Validation**
Verify documentation network updated and functional:

**Validation Criteria:**
- [ ] DOCUMENTATION_INDEX.md updated with "Completed Epics" section
- [ ] Archive entry includes archive link, completion date, summary, deliverables
- [ ] Archive README links to committed documentation (functional links)
- [ ] No broken links in documentation network (spot check key docs)
- [ ] Archive properly integrated into documentation hierarchy

**4. Quality Gates Validation**
Execute final ComplianceOfficer validation (if not already done):

**Validation Criteria:**
- [ ] ComplianceOfficer confirms archiving completeness
- [ ] All 8 archiving phases completed successfully
- [ ] Epic completion summary preserved in archive
- [ ] All completion operations logged (before archiving working-dir artifacts)

**5. Generate Archiving Completion Summary**
Create final completion report summarizing archiving operations:

```markdown
# Epic #{number} Archiving Completion Summary

**Archive Directory:** `./Docs/Archive/epic-{number}-{name}/`
**Archiving Date:** YYYY-MM-DD
**Archiving Status:** COMPLETE

## Archiving Operations Summary

### Specs Archiving
- **Files Archived:** {count} spec files
- **Original Directory:** Removed successfully
- **Archive Location:** `./Docs/Archive/epic-{number}-{name}/Specs/`

### Working Directory Archiving
- **Artifacts Archived:** {count} artifacts
- **Categories:** {list categories: execution plans, completion reports, validation reports, coordination artifacts}
- **Original Directory:** Cleaned (README.md preserved)
- **Archive Location:** `./Docs/Archive/epic-{number}-{name}/working-dir/`

### Documentation Integration
- **Archive README:** Created with 6 sections
- **DOCUMENTATION_INDEX.md:** Updated with Completed Epics entry
- **Documentation Links:** All functional

## Validation Results

- [x] Pre-completion validation passed (21 criteria)
- [x] Archive directory structure correct
- [x] Specs archiving complete
- [x] Working directory archiving complete
- [x] Archive documentation generated
- [x] Documentation index updated
- [x] Cleanup verification passed
- [x] Final validation passed

## Next Steps
- Epic closure on GitHub (close epic tracking issue if applicable)
- Team communication of epic completion
- Celebrate achievements and document lessons learned
```

#### Final Validation Checklist
- [ ] Archive integrity validation passed (5 checks)
- [ ] Cleanup completeness validation passed (3 checks)
- [ ] Documentation integration validation passed (5 checks)
- [ ] Quality gates validation passed (4 checks)
- [ ] Archiving completion summary generated
- [ ] Total: 17 validation criteria reviewed and confirmed

**Resource:** See `resources/documentation/validation-checklist.md` for comprehensive post-completion validation procedures

---

## TARGET AGENTS

### Primary User: Codebase Manager (Claude)
**Authority:** Coordinates epic completion orchestration
**Use Cases:**
- Detecting epic completion triggers (all issues closed, PRs merged, validation passed)
- Executing 8-phase archiving workflow (or delegating to `/epic-complete` command)
- Validating archiving completeness through ComplianceOfficer engagement
- Communicating epic retirement to user and team

**Integration with Claude Orchestration:**
1. User requests epic archiving or Claude detects epic completion triggers
2. Claude loads epic-completion skill for systematic workflow
3. Claude executes Phases 1-8 sequentially (or delegates to `/epic-complete` command)
4. Claude engages ComplianceOfficer for final validation
5. Claude reports archiving completion with summary

### Secondary User: ComplianceOfficer
**Use Cases:**
- Pre-completion validation (Phase 1) ensuring all quality gates passed
- Final validation (Phase 8) confirming archiving completeness
- Post-archiving quality assurance verifying documentation network integrity

**Integration with ComplianceOfficer Workflow:**
- ComplianceOfficer validates epic completion criteria before archiving initiated
- ComplianceOfficer performs final validation after archiving operations complete
- ComplianceOfficer confirms archiving meets DocumentationStandards.md Section 7 requirements

---

## RESOURCES

### Templates (Ready-to-Use Formats)

**archive-readme-template.md** - Complete archive README structure with placeholders
- Epic header section (number, name, status, completion date, iteration/issue counts)
- Executive summary section (2-3 sentence overview, performance achievements, strategic impact)
- Iterations overview section (bulleted list with issue ranges and summaries)
- Key deliverables section (outcomes, performance results, quality achievements)
- Documentation network section (committed docs, related standards, affected modules)
- Archive contents section (Specs summary, working-dir summary, navigation guidance)

**documentation-index-entry-template.md** - DOCUMENTATION_INDEX.md "Completed Epics" entry format
- Section header with archive purpose description
- Epic entry structure (archive link, completion date, summary, deliverables, performance docs)
- Placeholder guidance for epic-specific content

**archive-directory-structure.md** - Directory setup template for Phase 2
- Standard structure specification (root, Specs/, working-dir/)
- Naming conventions (epic-{number}-{name})
- Verification commands

**Location:** `resources/templates/`
**Usage:** Copy template, fill placeholders with epic-specific content, use for archive creation

### Examples (Reference Implementations)

**epic-291-archive-example.md** - Complete Epic #291 archiving workflow demonstration
- Pre-validation execution showing all 21 criteria
- Archive creation steps with actual Epic #291 metadata
- Archive README generation with real Epic #291 content (iterations, deliverables, performance)
- Documentation index update with Epic #291 entry
- Post-validation verification showing completeness

**Location:** `resources/examples/`
**Usage:** Review for realistic demonstration of complete 8-phase archiving workflow

### Documentation (Deep Dives)

**validation-checklist.md** - Comprehensive validation checklists extracted from TaskManagementStandards.md Section 10
- Pre-completion validation (5 categories, 21 items): Issue closure, PR integration, quality, documentation, performance
- Post-completion validation (4 categories, 15 items): Archive integrity, cleanup completeness, documentation integration, quality gates
- Validation execution procedures and escalation paths

**archiving-procedures.md** - Detailed archiving procedures from DocumentationStandards.md Section 7
- Archive directory creation step-by-step
- File move operations with rollback procedures
- Archive README generation workflow
- Index update procedures with section creation
- Cleanup verification commands and expected outputs

**error-recovery-guide.md** - Error handling and recovery procedures
- Common failure scenarios (missing spec directory, archive conflicts, broken links, validation failures)
- Mid-operation failure recovery (partial moves, rollback procedures)
- Rollback procedures for failed archiving operations
- Conflict resolution strategies (existing archives, duplicate entries, orphaned files)

**Location:** `resources/documentation/`
**Usage:** Deep understanding of validation procedures, archiving operations, troubleshooting complex scenarios

---

## INTEGRATION WITH TEAM WORKFLOWS

### Multi-Agent Coordination Patterns
This skill enables:
- **Systematic Epic Retirement:** Codebase Manager coordinates archiving with predictable workflow
- **Historical Preservation:** Complete epic context preserved for future reference and learning
- **Clean Workspace:** Active directories cleaned enabling clear focus on next initiatives
- **Documentation Continuity:** DOCUMENTATION_INDEX.md maintains complete epic history with accessible archives

### Claude's Orchestration Enhancement
Epic-completion skill helps Claude to:
- Execute disciplined archiving workflow without ad-hoc decision-making
- Validate epic completion criteria systematically before archiving initiated
- Delegate archiving to `/epic-complete` command with clear success criteria
- Communicate epic retirement to user with comprehensive completion summary
- Maintain documentation network integrity through systematic index updates

### Quality Gate Integration
This skill integrates with:
- **ComplianceOfficer Pre-Completion Validation:** Phase 1 ensures all quality gates passed before archiving
- **ComplianceOfficer Final Validation:** Phase 8 confirms archiving completeness and correctness
- **DocumentationStandards.md Compliance:** All archiving operations follow Section 7 requirements
- **TaskManagementStandards.md Alignment:** Workflow implements Section 10 epic completion procedures

### CLAUDE.md Integration
This skill directly supports CLAUDE.md Section 6: Orchestration Workflows by:
- Providing systematic epic lifecycle completion protocol
- Enabling consistent epic retirement across all initiatives
- Preserving comprehensive historical context for organizational learning
- Maintaining clean workspace directories through disciplined archiving

---

## SUCCESS METRICS

### Archiving Completeness
- **8-Phase Systematic Workflow:** Structured process vs. ad-hoc manual archiving
- **100% Artifact Preservation:** All spec files and working directory artifacts archived
- **Comprehensive Archive README:** 6 required sections with complete epic summary
- **Documentation Network Integrity:** DOCUMENTATION_INDEX.md updated, all links functional

### Validation Rigor
- **21 Pre-Completion Criteria:** Comprehensive validation before archiving initiated
- **17 Post-Completion Criteria:** Final validation confirms archiving correctness
- **ComplianceOfficer Integration:** Quality gates enforced through pre/post validation
- **Zero Broken Links:** Documentation network integrity maintained

### Workspace Cleanliness
- **Specs Directory Removed:** Original spec directory cleaned after successful archiving
- **Working Directory Cleaned:** Only README.md remains in active workspace
- **No Orphaned Files:** All epic artifacts systematically archived, none scattered
- **Clean Slate:** Next epic begins with uncluttered workspace

### Historical Preservation Quality
- **Immutable Archives:** Epic context permanently preserved for future reference
- **Comprehensive Navigation:** Archive README provides clear exploration guidance
- **Performance Documentation Linked:** ROI and productivity gains permanently accessible
- **Team Learning Enabled:** Historical context supports future epic planning and estimation

---

## TROUBLESHOOTING

### Common Issues

#### Issue: Pre-Completion Validation Fails (Open Issues or PRs Remain)
**Symptoms:** Phase 1 validation reveals open issues or unmerged PRs, preventing archiving
**Root Cause:** Epic completion initiated prematurely before all issues resolved or PRs merged
**Solution:**
1. Identify open issues: `gh issue list --label "epic:EPIC_NAME" --state open`
2. Identify open PRs: `gh pr list --head epic/EPIC_NAME --state open`
3. Escalate to user: "Epic archiving blocked - X open issues and Y open PRs remain"
4. Wait for issue closure and PR merges before retrying archiving
**Prevention:** Pre-completion validation (Phase 1) must pass before proceeding to Phase 2

#### Issue: Spec Directory Not Found at Expected Location
**Symptoms:** Phase 3 cannot locate `./Docs/Specs/epic-{number}-{name}/` directory
**Root Cause:** Spec directory naming mismatch or already moved in previous archiving attempt
**Solution:**
1. Search for spec directory: `find ./Docs/Specs -type d -name "*{epic-number}*"`
2. If found with different naming: Adjust `SPEC_DIR` variable to match actual location
3. If not found: Check if already archived: `ls -la ./Docs/Archive/`
4. If already archived: Skip Phase 3, document in completion summary
5. If truly missing: Escalate to user for clarification
**Prevention:** Verify spec directory naming matches original spec creation pattern before archiving

#### Issue: Archive Directory Already Exists (Conflict)
**Symptoms:** Phase 2 fails because `./Docs/Archive/epic-{number}-{name}/` already exists
**Root Cause:** Previous incomplete archiving attempt or duplicate epic number
**Solution:**
1. Inspect existing archive: `ls -la ./Docs/Archive/epic-{number}-{name}/`
2. If incomplete previous attempt: Backup existing, remove, retry archiving
3. If complete previous archive: Verify epic number correct, adjust naming if duplicate
4. If uncertain: Escalate to user: "Archive conflict detected - existing archive found"
**Prevention:** Phase 2 should check for existing archive before creation, escalate conflicts immediately

#### Issue: Working Directory README.md Accidentally Archived
**Symptoms:** Phase 4 verification reveals `./working-dir/README.md` missing from active workspace
**Root Cause:** Move operation included README.md instead of excluding it
**Solution:**
1. Restore from archive: `mv ./Docs/Archive/epic-{number}-{name}/working-dir/README.md ./working-dir/`
2. Verify restoration: `ls -la ./working-dir/README.md`
3. Document recovery in completion summary
**Prevention:** Phase 4 move command must explicitly exclude README.md: `find ./working-dir -mindepth 1 -maxdepth 1 ! -name "README.md"`

#### Issue: DOCUMENTATION_INDEX.md Update Fails (Broken Link Format)
**Symptoms:** Phase 6 link verification reveals broken archive link in DOCUMENTATION_INDEX.md
**Root Cause:** Archive path mismatch between Phase 2 directory creation and Phase 6 link format
**Solution:**
1. Verify actual archive location: `ls -la ./Docs/Archive/`
2. Update DOCUMENTATION_INDEX.md link to match actual archive path
3. Test link: Navigate to archive README via documentation index
4. Document correction in completion summary
**Prevention:** Phase 6 link format must exactly match Phase 2 archive directory path

### Escalation Path
When epic completion issues cannot be resolved through troubleshooting:
1. **Immediate Halt:** Stop archiving operations at current phase, do not proceed
2. **Document Failure:** Record exact failure point and symptoms in working directory
3. **User Escalation:** "Epic archiving blocked at Phase {N} - {specific issue description}"
4. **Recovery Planning:** User provides guidance for conflict resolution or manual intervention
5. **Retry After Resolution:** Re-execute archiving workflow from Phase 1 after issues resolved

---

**Skill Status:** ✅ **OPERATIONAL**
**Adoption:** Codebase Manager, ComplianceOfficer
**Context Savings:** N/A (first-use skill for epic lifecycle management)
**Progressive Loading:** YAML frontmatter (~100 tokens) → SKILL.md (~3,400 tokens) → resources (on-demand)
