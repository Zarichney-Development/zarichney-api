# Success Criteria Validation Template

**Date:** [YYYY-MM-DD]
**Agent:** [Agent Name]
**GitHub Issue:** #[number]
**Core Issue:** [Brief one-sentence description]

---

## SUCCESS CRITERIA STATUS

### Criterion 1: [Testable outcome 1]
- **Status:** [✅ MET / ❌ NOT MET / ⚠️ PARTIAL]
- **Evidence:** [Specific test results, manual validation, automated test output]
- **Validation Approach:** [How this was tested/verified]
- **Notes:** [Any additional context about this criterion]

### Criterion 2: [Testable outcome 2]
- **Status:** [✅ MET / ❌ NOT MET / ⚠️ PARTIAL]
- **Evidence:** [Specific test results, manual validation, automated test output]
- **Validation Approach:** [How this was tested/verified]
- **Notes:** [Any additional context about this criterion]

### Criterion 3: [Testable outcome 3]
- **Status:** [✅ MET / ❌ NOT MET / ⚠️ PARTIAL]
- **Evidence:** [Specific test results, manual validation, automated test output]
- **Validation Approach:** [How this was tested/verified]
- **Notes:** [Any additional context about this criterion]

### [Additional criteria as defined in core issue analysis]

**Overall Success Criteria Status:** [ALL MET ✅ / PARTIAL ⚠️ / NOT MET ❌]

---

## CORE FUNCTIONALITY VALIDATION

### Problem Before Fix
**Blocking Issue:** [Description of original problem]

**Evidence of Problem:**
- [Error messages, failed tests, broken workflows]
- [User impact, functional gaps]
- [System behavior demonstrating issue]

### Problem After Fix
**Resolution Status:** [CORE_ISSUE_RESOLVED ✅ / PARTIAL ⚠️ / UNRESOLVED ❌]

**Evidence of Resolution:**
- [Successful test results, working workflows]
- [Fixed functionality demonstration]
- [System behavior proving issue resolved]

### Validation Approach
**Manual Testing:**
- [Test scenario 1]: [Result]
- [Test scenario 2]: [Result]
- [Test scenario 3]: [Result]

**Automated Testing:**
- [Test case 1]: [Pass/Fail with details]
- [Test case 2]: [Pass/Fail with details]
- [Test case 3]: [Pass/Fail with details]

**Integration Validation:**
- [Affected system 1]: [Compatibility verified]
- [Affected system 2]: [Compatibility verified]

---

## SCOPE COMPLIANCE REVIEW

### Modified Files
**Planned Modifications (from scope boundary definition):**
- [File 1]: [Planned change]
- [File 2]: [Planned change]
- [File 3]: [Planned change]

**Actual Modifications (implementation reality):**
- [File 1]: [Actual change made]
- [File 2]: [Actual change made]
- [File 3]: [Actual change made]
- [Additional file (if any)]: [Justification for unplanned modification]

**File Count Analysis:**
- **Planned:** [Number]
- **Actual:** [Number]
- **Delta:** [Actual - Planned]
- **Justification:** [Explanation if delta > 0, otherwise "Scope maintained as planned"]

### Scope Boundary Match
**Status:** [COMPLIANT ✅ / VIOLATION_DETECTED ❌]

**Compliance Details:**
- [ ] All modified files were in original scope boundary definition
- [ ] No OUT OF SCOPE improvements were implemented
- [ ] All integration boundaries respected
- [ ] Forbidden scope expansions avoided

**If VIOLATION_DETECTED:**
- **Violation Description:** [What scope expansion occurred]
- **Rationale:** [Why this violation happened]
- **Remediation:** [How this was/will be addressed]

### Mission Drift Analysis
**Status:** [NONE ✅ / DETECTED ❌]

**Mission Drift Checkpoints:**
- [ ] No "while I'm here" syndrome violations
- [ ] No continuation beyond success criteria ("perfect is enemy of done")
- [ ] No consistency refactoring across unrelated code
- [ ] No opportunistic testing beyond core validation
- [ ] No infrastructure enhancements during core fix

**If DETECTED:**
- **Drift Pattern:** [Which mission drift pattern occurred]
- **Description:** [What scope expansion happened]
- **Impact:** [How this affected mission timeline/scope]
- **Remediation:** [What was done to refocus]

---

## DEFERRED IMPROVEMENTS DOCUMENTED

### Improvement 1: [Improvement name]
- **Description:** [What improvement was identified but not implemented]
- **Future Work Location:** [Issue #ABC / Epic #DEF / Tech debt backlog]
- **Priority:** [High / Medium / Low]
- **Estimated Effort:** [Small / Medium / Large]
- **Expected Impact:** [Benefit if implemented]

### Improvement 2: [Improvement name]
- **Description:** [What improvement was identified but not implemented]
- **Future Work Location:** [Issue #ABC / Epic #DEF / Tech debt backlog]
- **Priority:** [High / Medium / Low]
- **Estimated Effort:** [Small / Medium / Large]
- **Expected Impact:** [Benefit if implemented]

### [Additional improvements as identified during implementation]

**Total Deferred Improvements:** [Number]
**All Improvements Captured:** [✅ YES / ❌ INCOMPLETE]

---

## REGRESSION VALIDATION

### Existing Functionality Preserved
- [ ] All existing unit tests passing (100% pass rate maintained)
- [ ] All existing integration tests passing
- [ ] No breaking changes to public contracts/APIs
- [ ] No performance degradation detected

### Regression Test Results
**Test Suite:** [Suite name - e.g., UserServiceTests]
- **Tests Run:** [Number]
- **Tests Passed:** [Number]
- **Tests Failed:** [Number]
- **Pass Rate:** [Percentage]

**Notable Results:**
- [Any test failures and their resolution]
- [Any test modifications required and justification]
- [Any new tests added for regression prevention]

---

## QUALITY VALIDATION

### Code Quality
- [ ] Code follows project coding standards
- [ ] No new compiler warnings introduced
- [ ] No code smells introduced (linting clean)
- [ ] Code reviews completed (if applicable)

### Test Quality
- [ ] New tests follow testing standards
- [ ] Test coverage meets project requirements
- [ ] Tests are maintainable and clear
- [ ] Tests validate core functionality comprehensively

### Documentation Quality
- [ ] Code changes documented (XML comments, inline docs)
- [ ] README updated if contracts changed
- [ ] API documentation updated if applicable
- [ ] Deferred improvements documented for team

---

## MISSION COMPLETION ASSESSMENT

### Core Issue Resolution
**Question:** Is the specific blocking technical problem resolved?
**Answer:** [YES ✅ / NO ❌ / PARTIAL ⚠️]

**Evidence:**
- [All success criteria met]
- [Core functionality validated]
- [No blocking problem remains]

### Scope Discipline Maintained
**Question:** Was surgical focus maintained throughout implementation?
**Answer:** [YES ✅ / NO ❌]

**Evidence:**
- [Scope boundary compliance verified]
- [No unauthorized scope expansion]
- [Mission drift avoided or remediated]

### Ready for Next Actions
**Question:** Is the mission complete and ready to move forward?
**Answer:** [YES ✅ / NO ❌ / BLOCKED ⚠️]

**Next Actions:**
- [If YES]: Mission complete, ready for Claude's next agent engagement
- [If NO]: [What remains to complete core issue resolution]
- [If BLOCKED]: [What blocker prevents completion, escalation needed]

---

## ALIGNMENT WITH CONTEXT PACKAGE

**SUCCESS_CRITERIA from Claude's context package:**
```
[Paste exact SUCCESS_CRITERIA field from context package Claude provided]
```

**Alignment Verification:**
- [ ] All context package success criteria addressed
- [ ] Validation approach matches context package expectations
- [ ] Core issue resolution confirmed per context package definition

---

## EXAMPLE USAGE

### Example 1: API Bug Fix (UserService.GetUserById)

**SUCCESS CRITERIA STATUS:**
- ✅ GetUserById returns 200 OK for users with complete profiles
- ✅ GetUserById returns 200 OK for users with null profiles
- ✅ No NullReferenceException thrown
- ✅ Unit test added validating null profile handling
- ✅ All existing UserService tests passing

**CORE FUNCTIONALITY:** CORE_ISSUE_RESOLVED ✅
- Problem Before: 500 errors for users with null profiles
- Problem After: All user retrieval scenarios return 200 OK
- Validation: Manual testing + automated test suite (100% pass rate)

**SCOPE COMPLIANCE:** COMPLIANT ✅
- Modified: UserService.cs (null check), UserServiceTests.cs (new test)
- No scope expansion, mission drift avoided

**DEFERRED IMPROVEMENTS:**
- UserService async refactoring → Issue #ABC
- API error handling standardization → Epic #DEF

**MISSION COMPLETION:** YES ✅ - Ready for next actions

---

**Template Status:** Ready to use
**Usage:** Copy this template, fill in all bracketed placeholders, use to validate mission completion
**Integration:** Use with core-issue-focus skill Step 4 (Core Issue Validation)
