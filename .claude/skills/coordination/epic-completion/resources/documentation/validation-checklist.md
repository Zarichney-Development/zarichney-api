# Epic Completion Validation Checklist

**Purpose:** Comprehensive validation checklists for pre-completion and post-completion epic archiving verification

**Source:** Extracted from TaskManagementStandards.md Section 10: Epic Completion Workflow

---

## Pre-Completion Validation Checklist

Execute before initiating any archiving operations (Phase 1 of epic-completion workflow).

### Category 1: Issue Closure (5 checks)

**Objective:** Verify all epic issues resolved and closed

- [ ] **1.1 All epic issues closed**
  - **Validation:** `gh issue list --label "epic:EPIC_NAME" --state open`
  - **Expected:** No open issues returned
  - **Failure:** If open issues exist, escalate to user with issue list

- [ ] **1.2 All issue acceptance criteria met**
  - **Validation:** Review closed issues for acceptance criteria completion confirmations
  - **Expected:** All closed issues have acceptance criteria checkboxes completed or explicit completion statements
  - **Failure:** Re-open issues with incomplete acceptance criteria

- [ ] **1.3 All issue deliverables committed**
  - **Validation:** Review closed issues for commit references or PR links
  - **Expected:** All deliverables mentioned in issues have corresponding commits in repository
  - **Failure:** Identify missing deliverables, escalate for completion

- [ ] **1.4 No outstanding blockers**
  - **Validation:** Search issue comments for "blocked", "blocker", "dependency" keywords
  - **Expected:** All blocker comments resolved or marked resolved
  - **Failure:** Address outstanding blockers before archiving

- [ ] **1.5 No outstanding dependencies**
  - **Validation:** Review issue dependencies (if using GitHub Projects or issue links)
  - **Expected:** All dependent issues closed or dependencies removed
  - **Failure:** Resolve or remove dependencies before archiving

**Category 1 Result:** ___/5 checks passed

---

### Category 2: PR Integration (4 checks)

**Objective:** Verify all epic PRs merged successfully

- [ ] **2.1 All section PRs merged to epic branch**
  - **Validation:** `gh pr list --base epic/EPIC_NAME --state open`
  - **Expected:** No open PRs returned
  - **Failure:** If open PRs exist, complete review and merge before archiving

- [ ] **2.2 Epic branch merged to main (or final section PR ready)**
  - **Validation:** `gh pr list --head epic/EPIC_NAME --state merged` OR verify final section PR exists
  - **Expected:** Epic branch merged or final section PR #XXX ready for review
  - **Failure:** Complete epic integration or create final section PR

- [ ] **2.3 All merge conflicts resolved**
  - **Validation:** Review merged PRs for conflict resolution confirmations
  - **Expected:** No unresolved merge conflict indicators in PR history
  - **Failure:** Investigate conflict resolution, verify correctness

- [ ] **2.4 No failed CI/CD checks on epic branch**
  - **Validation:** `gh workflow view --ref epic/EPIC_NAME` or check GitHub Actions tab
  - **Expected:** All CI/CD workflows passing on epic branch head
  - **Failure:** Fix failing workflows before archiving

**Category 2 Result:** ___/4 checks passed

---

### Category 3: Quality Validation (5 checks)

**Objective:** Verify epic changes meet all quality gates

- [ ] **3.1 Build passes with zero warnings/errors**
  - **Validation:** `dotnet build zarichney-api.sln`
  - **Expected:** "Build succeeded. 0 Warning(s). 0 Error(s)."
  - **Failure:** Fix build warnings/errors before archiving

- [ ] **3.2 Test suite passes with >99% executable pass rate**
  - **Validation:** `./Scripts/run-test-suite.sh report summary`
  - **Expected:** Executable pass rate ≥99%, no failing tests
  - **Failure:** Fix failing tests or skip tests with justification

- [ ] **3.3 ComplianceOfficer validation complete with GO decision**
  - **Validation:** Review ComplianceOfficer final validation report in working directory
  - **Expected:** GO decision with all quality gates passed
  - **Failure:** Address NO-GO findings before archiving

- [ ] **3.4 All AI Sentinels operational and compatible**
  - **Validation:** Verify AI Sentinels (DebtSentinel, StandardsGuardian, TestMaster, SecuritySentinel, MergeOrchestrator) compatible with epic changes
  - **Expected:** No AI Sentinel errors or incompatibilities introduced
  - **Failure:** Fix AI Sentinel issues before archiving

- [ ] **3.5 No breaking changes introduced**
  - **Validation:** Review PR descriptions and ComplianceOfficer report for breaking change indicators
  - **Expected:** Zero breaking changes OR breaking changes documented and approved
  - **Failure:** Document breaking changes or revert if unapproved

**Category 3 Result:** ___/5 checks passed

---

### Category 4: Documentation Currency (4 checks)

**Objective:** Verify all documentation updated and current

- [ ] **4.1 All affected module READMEs updated**
  - **Validation:** Review module READMEs modified by epic (check git history)
  - **Expected:** All READMEs reflect epic changes per DocumentationStandards.md
  - **Failure:** Update outdated READMEs before archiving

- [ ] **4.2 All standards documents reflect epic changes (if applicable)**
  - **Validation:** Review standards documents in `./Docs/Standards/` for epic-related updates
  - **Expected:** Standards documents updated if epic modified standards or introduced new patterns
  - **Failure:** Update standards documents before archiving

- [ ] **4.3 DOCUMENTATION_INDEX.md includes all epic-related documentation**
  - **Validation:** Review DOCUMENTATION_INDEX.md for epic documentation references
  - **Expected:** All major epic documentation files listed in appropriate sections
  - **Failure:** Update DOCUMENTATION_INDEX.md with missing documentation

- [ ] **4.4 No broken links in documentation network (spot check)**
  - **Validation:** Spot check 5-10 documentation links from key epic documents
  - **Expected:** All checked links functional (no 404s, correct relative paths)
  - **Failure:** Fix broken links before archiving

**Category 4 Result:** ___/4 checks passed

---

### Category 5: Performance Validation (4 checks, if applicable)

**Objective:** Verify performance-critical epic achievements documented

**Note:** Skip this category if epic not performance-focused (mark N/A)

- [ ] **5.1 All performance targets met or exceeded**
  - **Validation:** Review epic objectives against achieved performance metrics
  - **Expected:** All performance targets met or exceeded, with quantified results
  - **Failure:** Document performance shortfalls or adjust targets

- [ ] **5.2 Token efficiency validated and documented**
  - **Validation:** Review token tracking measurements in performance documentation
  - **Expected:** Token efficiency improvements validated with before/after measurements
  - **Failure:** Complete token efficiency validation before archiving

- [ ] **5.3 Productivity gains quantified with ROI calculation**
  - **Validation:** Review productivity documentation for time savings and ROI calculation
  - **Expected:** Productivity gains quantified in min/day and ROI percentage calculated
  - **Failure:** Complete productivity ROI calculation before archiving

- [ ] **5.4 Performance documentation committed to ./Docs/Development/**
  - **Validation:** `ls -la ./Docs/Development/Epic{N}PerformanceAchievements.md`
  - **Expected:** Performance documentation file exists and comprehensive
  - **Failure:** Create or complete performance documentation before archiving

**Category 5 Result:** ___/4 checks passed OR N/A

---

### Pre-Completion Validation Summary

**Total Checks:** 21 (or 17 if Category 5 N/A)

**Results:**
- Category 1 (Issue Closure): ___/5
- Category 2 (PR Integration): ___/4
- Category 3 (Quality Validation): ___/5
- Category 4 (Documentation Currency): ___/4
- Category 5 (Performance Validation): ___/4 or N/A

**Overall Status:** ☐ PASS (all categories passed) | ☐ FAIL (one or more categories failed)

**Decision:**
- **PASS:** Proceed to Phase 2 (Archive Directory Creation)
- **FAIL:** Address failing checks before proceeding with archiving

---

## Post-Completion Validation Checklist

Execute after all archiving operations complete (Phase 8 of epic-completion workflow).

### Category 6: Archive Integrity (5 checks)

**Objective:** Verify archive structure complete and correct

- [ ] **6.1 Archive directory structure matches DocumentationStandards.md Section 7**
  - **Validation:** `ls -la ./Docs/Archive/epic-{N}-{name}/`
  - **Expected:** Root directory with Specs/, working-dir/, and README.md
  - **Failure:** Recreate missing directories or files

- [ ] **6.2 All expected spec files present in archive Specs/**
  - **Validation:** Count spec files in archive, compare to pre-archiving inventory
  - **Expected:** Spec file count matches pre-archiving count (no missing files)
  - **Failure:** Identify missing spec files, restore from backup or original location

- [ ] **6.3 All expected working directory artifacts present in archive working-dir/**
  - **Validation:** Count artifacts in archive, compare to Phase 4 inventory
  - **Expected:** Artifact count matches Phase 4 inventory (no missing artifacts)
  - **Failure:** Identify missing artifacts, restore from backup or original location

- [ ] **6.4 Archive README comprehensive with all 6 required sections**
  - **Validation:** Review archive README.md for required sections
  - **Expected:** Epic header, executive summary, iterations overview, key deliverables, documentation network, archive contents
  - **Failure:** Complete missing sections in archive README

- [ ] **6.5 No missing files or broken directory structure**
  - **Validation:** Recursive directory check: `find ./Docs/Archive/epic-{N}-{name}/ -type d -empty`
  - **Expected:** No empty directories (except intentionally empty)
  - **Failure:** Investigate empty directories, restore missing content

**Category 6 Result:** ___/5 checks passed

---

### Category 7: Cleanup Completeness (3 checks)

**Objective:** Verify original directories properly cleaned

- [ ] **7.1 Original spec directory removed (or empty)**
  - **Validation:** `ls -la ./Docs/Specs/ | grep "epic-{N}"`
  - **Expected:** No match (spec directory removed) OR directory empty
  - **Failure:** Remove original spec directory or confirm intentional retention

- [ ] **7.2 Working directory contains only README.md (no other artifacts)**
  - **Validation:** `ls -1 ./working-dir/ | grep -v README.md | wc -l`
  - **Expected:** Count = 0 (only README.md present)
  - **Failure:** Archive remaining artifacts or confirm intentional retention

- [ ] **7.3 No orphaned files in unexpected locations**
  - **Validation:** `find ./Docs -maxdepth 1 -type f -name "*{epic-number}*"`
  - **Expected:** No matches (no orphaned files in Docs root or unexpected locations)
  - **Failure:** Move orphaned files to archive or appropriate location

**Category 7 Result:** ___/3 checks passed

---

### Category 8: Documentation Integration (5 checks)

**Objective:** Verify documentation network updated and functional

- [ ] **8.1 DOCUMENTATION_INDEX.md updated with "Completed Epics" section**
  - **Validation:** `grep -A 10 "## Completed Epics" ./Docs/DOCUMENTATION_INDEX.md`
  - **Expected:** Section exists with epic entry
  - **Failure:** Add "Completed Epics" section or update with epic entry

- [ ] **8.2 Archive entry includes archive link, completion date, summary, deliverables**
  - **Validation:** Review epic entry in DOCUMENTATION_INDEX.md
  - **Expected:** All required fields present (archive link, completion date, summary, deliverables, performance doc link if applicable)
  - **Failure:** Complete missing fields in epic entry

- [ ] **8.3 Archive README links to committed documentation (functional links)**
  - **Validation:** Spot check 3-5 documentation links in archive README
  - **Expected:** All checked links functional (correct relative paths, files exist)
  - **Failure:** Fix broken links in archive README

- [ ] **8.4 No broken links in documentation network (spot check key docs)**
  - **Validation:** Spot check 5-10 links from DOCUMENTATION_INDEX.md and archive README
  - **Expected:** All checked links functional
  - **Failure:** Fix broken links before finalizing archiving

- [ ] **8.5 Archive properly integrated into documentation hierarchy**
  - **Validation:** Navigate documentation network from DOCUMENTATION_INDEX.md → Archive README → Specs
  - **Expected:** Complete navigation path functional, archive discoverable
  - **Failure:** Update documentation network for proper integration

**Category 8 Result:** ___/5 checks passed

---

### Category 9: Quality Gates (4 checks)

**Objective:** Verify final quality validation complete

- [ ] **9.1 ComplianceOfficer confirms archiving completeness**
  - **Validation:** Engage ComplianceOfficer for final archiving validation
  - **Expected:** ComplianceOfficer GO decision on archiving completeness
  - **Failure:** Address ComplianceOfficer findings before finalizing

- [ ] **9.2 All 8 archiving phases completed successfully**
  - **Validation:** Review archiving workflow execution log
  - **Expected:** Phases 1-8 all completed with success indicators
  - **Failure:** Identify incomplete phases, re-execute as needed

- [ ] **9.3 Epic completion summary preserved in archive**
  - **Validation:** Check for archiving completion summary in archive working-dir/
  - **Expected:** Completion summary present documenting archiving operations
  - **Failure:** Generate and add completion summary to archive

- [ ] **9.4 All completion operations logged (before archiving working-dir)**
  - **Validation:** Review archiving completion summary for operation documentation
  - **Expected:** All archiving operations documented with results
  - **Failure:** Complete operation documentation in summary

**Category 9 Result:** ___/4 checks passed

---

### Post-Completion Validation Summary

**Total Checks:** 17

**Results:**
- Category 6 (Archive Integrity): ___/5
- Category 7 (Cleanup Completeness): ___/3
- Category 8 (Documentation Integration): ___/5
- Category 9 (Quality Gates): ___/4

**Overall Status:** ☐ PASS (all categories passed) | ☐ FAIL (one or more categories failed)

**Decision:**
- **PASS:** Epic archiving complete, proceed to epic closure
- **FAIL:** Address failing checks before finalizing archiving

---

## Combined Validation Summary

**Pre-Completion Validation:** ___/21 checks (or ___/17 if Category 5 N/A)
**Post-Completion Validation:** ___/17 checks

**Total Validation Checks:** ___/38 checks (or ___/34 if Category 5 N/A)

**Final Epic Archiving Status:**
- ☐ **COMPLETE:** All validation checks passed, epic successfully archived
- ☐ **INCOMPLETE:** One or more validation checks failed, archiving requires attention
- ☐ **BLOCKED:** Critical validation failures prevent archiving completion

---

## Validation Execution Procedures

### Pre-Completion Validation Execution

**When:** Before Phase 2 (Archive Directory Creation)

**Process:**
1. Review epic completion triggers (all issues closed, PRs merged, ComplianceOfficer validation complete)
2. Execute Category 1-5 validation checks sequentially
3. Document results for each category (pass/fail counts)
4. If any category fails: Halt archiving, escalate failures to user, wait for resolution
5. If all categories pass: Document validation summary, proceed to Phase 2

**Output:** Pre-completion validation summary with pass/fail status

### Post-Completion Validation Execution

**When:** After Phase 7 (Cleanup Verification), at start of Phase 8 (Final Validation)

**Process:**
1. Review archiving operations completed (Phases 2-7)
2. Execute Category 6-9 validation checks sequentially
3. Document results for each category (pass/fail counts)
4. If any category fails: Identify failure root cause, apply recovery procedures
5. If all categories pass: Document validation summary, finalize archiving

**Output:** Post-completion validation summary with pass/fail status

### Escalation Procedures

**Pre-Completion Validation Failures:**
- Document specific failing checks with details
- Escalate to user: "Epic archiving blocked - pre-completion validation failures detected"
- Provide actionable remediation guidance per failing check
- Wait for user resolution before retrying validation

**Post-Completion Validation Failures:**
- Document specific failing checks with details
- Apply recovery procedures from error-recovery-guide.md
- If recovery successful: Re-execute validation
- If recovery unsuccessful: Escalate to user with detailed failure analysis

---

## Related Resources

**Standards References:**
- [TaskManagementStandards.md Section 10](../../../../../../../Docs/Standards/TaskManagementStandards.md#10-epic-completion-workflow) - Epic completion workflow
- [DocumentationStandards.md Section 7](../../../../../../../Docs/Standards/DocumentationStandards.md#7-epic-archiving-standards) - Epic archiving standards

**Skill Resources:**
- [archiving-procedures.md](./archiving-procedures.md) - Detailed archiving operations
- [error-recovery-guide.md](./error-recovery-guide.md) - Failure recovery procedures

**Skill Sections:**
- Phase 1: Pre-Completion Validation
- Phase 8: Final Validation
