# Validation Checkpoints & Testing Strategies

**Purpose:** When to verify core issue status during implementation milestones, testing approaches for core functionality validation, success criteria measurement
**Audience:** All agents using core-issue-focus skill
**Usage:** Reference during Step 4 (Core Issue Validation) and throughout implementation for checkpoint validation

---

## TABLE OF CONTENTS

1. [Implementation Milestone Checkpoints](#implementation-milestone-checkpoints)
2. [Testing Strategies for Core Functionality](#testing-strategies-for-core-functionality)
3. [Success Criteria Validation Approaches](#success-criteria-validation-approaches)
4. [Escalation Triggers & Guidance](#escalation-triggers--guidance)

---

## IMPLEMENTATION MILESTONE CHECKPOINTS

### Checkpoint Philosophy

**Core Principle:** Regular validation prevents mission drift and ensures progress toward core issue resolution.

**Checkpoint Objectives:**
1. Confirm core issue remains focus (not distracted by improvements)
2. Validate progress toward success criteria (measurable advancement)
3. Detect scope creep early (before significant drift occurs)
4. Maintain team coordination (Claude aware of progress and blockers)

**Frequency Guidelines:**
- **After Each File Modification:** Quick drift check (30 seconds)
- **After Each Significant Change:** Progress validation (2-3 minutes)
- **Before Completion:** Comprehensive validation (5-10 minutes)

---

### Checkpoint 1: Planning Complete (Before Implementation)

**Timing:** After Step 1 (Identify Core Issue) and Step 2 (Surgical Scope Definition), before writing code

**Validation Questions:**
- [ ] Core blocking problem clearly understood (can explain in one sentence)
- [ ] Minimum viable fix identified (surgical scope, not comprehensive overhaul)
- [ ] Success criteria defined (3-5 testable outcomes)
- [ ] Scope boundaries documented (IN SCOPE vs. OUT OF SCOPE clear)
- [ ] Context package alignment confirmed (CORE_ISSUE, SCOPE_BOUNDARY, SUCCESS_CRITERIA match)

**Validation Approach:**
```
# Planning Complete Validation

**Core Issue:** [One-sentence blocking problem]
**Minimum Fix:** [Surgical scope description]
**Success Criteria:** [List 3-5 testable outcomes]
**File Count:** [Number of files to modify]
**Estimated Time:** [Implementation duration estimate]

**Alignment Check:**
- Context Package CORE_ISSUE: [Match / Mismatch - clarify if mismatch]
- Context Package SCOPE_BOUNDARY: [Match / Mismatch - clarify if mismatch]
- Context Package SUCCESS_CRITERIA: [Match / Mismatch - clarify if mismatch]

**Ready to Implement:** [YES / NO - address gaps if no]
```

**Red Flags at This Checkpoint:**
- Cannot articulate core issue in one sentence (unclear mission)
- Scope boundary definition includes many files (not surgical)
- Success criteria vague or unmeasurable (no objective completion)
- Context package misalignment (need Claude clarification)

**Action if Red Flags:**
- Escalate to Claude for mission clarification before implementing
- Refine scope definition to be more surgical
- Make success criteria specific and testable

---

### Checkpoint 2: First File Modified (Early Drift Detection)

**Timing:** Immediately after first file modification committed

**Validation Questions:**
- [ ] Modified file listed in scope boundary definition (authorized change)
- [ ] Change directly resolves core blocking problem (not improvement/enhancement)
- [ ] No new dependencies/abstractions added (minimal change)
- [ ] No tempting improvements implemented (deferred list used instead)

**Validation Approach:**
```
# First File Modified Validation

**File:** [Absolute file path]
**Change Description:** [What was modified and why]
**Core Issue Alignment:** [How this directly resolves blocking problem]
**Scope Compliance:** [COMPLIANT / VIOLATION - explain if violation]

**Temptations Deferred:**
- [Improvement 1 considered but deferred to deferred list]
- [Improvement 2 considered but deferred to deferred list]

**Ready to Continue:** [YES / NO - address issues if no]
```

**Red Flags at This Checkpoint:**
- File not in scope boundary definition (early scope creep)
- Change is improvement not core fix (mission drift starting)
- New dependencies added (infrastructure scope creep)
- No deferred improvements documented (not identifying temptations)

**Action if Red Flags:**
- STOP implementation immediately
- Review change against scope boundary definition
- Revert if scope violation, document improvement in deferred list
- Re-align with core issue before continuing

---

### Checkpoint 3: Halfway Through Implementation

**Timing:** When approximately 50% of planned changes complete (based on file count or time estimate)

**Validation Questions:**
- [ ] Modified files match scope boundary definition (no additional files)
- [ ] Implementation time on track with estimate (not significantly over)
- [ ] All changes resolve core blocking problem (no improvements implemented)
- [ ] Deferred list growing as improvements identified (disciplined deferral)
- [ ] Success criteria still guide implementation (not distracted by enhancements)

**Validation Approach:**
```
# Halfway Implementation Validation

**Progress:**
- Files Modified: [Actual count] / [Planned count]
- Time Elapsed: [Actual time] / [Estimated time]
- Success Criteria Progress: [Which criteria can be validated now]

**Scope Compliance:**
- Modified Files: [List actual files]
- Scope Boundary Match: [COMPLIANT / DRIFT_DETECTED]
- Additional Files Rationale: [If any unplanned files modified]

**Deferred Improvements:**
- [Improvement 1]: [Brief description, deferred to ...]
- [Improvement 2]: [Brief description, deferred to ...]
- Count: [Number of improvements deferred so far]

**Course Correction Needed:** [NO / YES - specify if yes]
```

**Red Flags at This Checkpoint:**
- File count exceeds scope by 2+ files (scope creep)
- Time 50%+ over estimate (drift or complexity underestimation)
- No deferred improvements documented (missing temptations or not deferring)
- Cannot explain how changes resolve core issue (lost focus)

**Action if Red Flags:**
- Execute mission drift classification (Step 3 of skill)
- Review all changes for scope compliance
- Consider STOP and refocus if significant drift detected
- Escalate to Claude if core issue unclear

---

### Checkpoint 4: Implementation Complete (Before Testing)

**Timing:** After all code changes made, before validation testing begins

**Validation Questions:**
- [ ] Modified files exactly match scope boundary definition (perfect alignment or justified deviation)
- [ ] All changes directly resolve core blocking problem (no enhancements implemented)
- [ ] Implementation time within reasonable range of estimate (minor variance acceptable)
- [ ] Deferred improvements comprehensively documented (captured all temptations)
- [ ] Ready to validate success criteria (implementation complete, testing ready)

**Validation Approach:**
```
# Implementation Complete Validation

**Scope Compliance Review:**
- Planned Files: [List from scope boundary definition]
- Actual Files: [List of files actually modified]
- Delta: [Additional files or perfect match]
- Justification: [If delta > 0, explain why additional files needed]

**Core Issue Alignment:**
- Blocking Problem: [Restate core issue]
- Implementation Summary: [How changes resolve blocking problem]
- Success Criteria Readiness: [Which criteria can now be tested]

**Deferred Improvements Summary:**
- Total Deferred: [Count]
- High Priority: [Count and examples]
- Medium Priority: [Count and examples]
- Low Priority: [Count and examples]

**Scope Compliance Status:** [COMPLIANT / MINOR_DEVIATION / SIGNIFICANT_DRIFT]
**Ready for Validation:** [YES / NO - address issues if no]
```

**Red Flags at This Checkpoint:**
- Significant file count deviation without justification (scope creep)
- Cannot clearly articulate how changes resolve core issue (lost focus)
- Deferred list empty (either no temptations identified or all implemented)
- Implementation time 2x+ estimate (complexity underestimation or drift)

**Action if Red Flags:**
- Review all changes for necessity (can any be reverted as scope creep?)
- Ensure deferred list comprehensive (capture all improvements considered)
- Validate alignment with context package before testing
- Consider partial implementation if drift significant

---

### Checkpoint 5: Success Criteria Validation (Before Completion)

**Timing:** After testing complete, before marking mission complete

**Validation Questions:**
- [ ] All success criteria met (100% completion)
- [ ] Core functionality works as intended (blocking problem resolved)
- [ ] No regressions detected (existing tests passing)
- [ ] Scope compliance confirmed (no unauthorized scope expansion)
- [ ] Deferred improvements documented for future work (captured not lost)

**Validation Approach:**
```
# Success Criteria Validation

**Criteria Status:**
1. [Criterion 1]: [✅ MET / ❌ NOT MET / ⚠️ PARTIAL] - [Evidence]
2. [Criterion 2]: [✅ MET / ❌ NOT MET / ⚠️ PARTIAL] - [Evidence]
3. [Criterion 3]: [✅ MET / ❌ NOT MET / ⚠️ PARTIAL] - [Evidence]
4. [Criterion 4]: [✅ MET / ❌ NOT MET / ⚠️ PARTIAL] - [Evidence]
5. [Criterion 5]: [✅ MET / ❌ NOT MET / ⚠️ PARTIAL] - [Evidence]

**Overall Status:** [ALL MET ✅ / PARTIAL ⚠️ / NOT MET ❌]

**Core Functionality:**
- Problem Before: [Description of blocking issue]
- Problem After: [Status after implementation]
- Resolution: [RESOLVED ✅ / PARTIAL ⚠️ / UNRESOLVED ❌]

**Regression Validation:**
- Test Suite: [Suite name]
- Pass Rate: [Percentage]
- Failures: [None / List failed tests and resolution]

**Scope Compliance Final:**
- Modified Files: [Count and list]
- Scope Match: [COMPLIANT ✅ / VIOLATION ❌]
- Mission Drift: [NONE ✅ / DETECTED ❌]

**Mission Complete:** [YES ✅ / NO ❌ / BLOCKED ⚠️]
```

**Red Flags at This Checkpoint:**
- Any success criteria not met (incomplete implementation)
- Core functionality partially working (core issue not fully resolved)
- Regression detected (existing tests failing)
- Scope violations identified (unauthorized expansion)

**Action if Red Flags:**
- Complete remaining success criteria before marking complete
- Debug partial functionality until fully working
- Fix regressions before completion
- Document scope violations and remediation
- DO NOT mark mission complete until all criteria met

---

## TESTING STRATEGIES FOR CORE FUNCTIONALITY

### Manual Testing Approaches

**Purpose:** Validate core functionality works as intended in realistic scenarios

**When to Use:**
- Quick validation during implementation (smoke testing)
- User-facing functionality that's difficult to automate
- Integration scenarios spanning multiple systems
- Edge cases not covered by automated tests

**Manual Testing Template:**
```
# Manual Testing Checklist

**Core Functionality:** [What you're validating]

**Test Scenarios:**
1. [Scenario 1 - happy path]
   - Steps: [Detailed steps to reproduce]
   - Expected: [Expected outcome]
   - Actual: [Actual outcome]
   - Result: [PASS / FAIL]

2. [Scenario 2 - edge case]
   - Steps: [Detailed steps to reproduce]
   - Expected: [Expected outcome]
   - Actual: [Actual outcome]
   - Result: [PASS / FAIL]

3. [Scenario 3 - error handling]
   - Steps: [Detailed steps to reproduce]
   - Expected: [Expected outcome]
   - Actual: [Actual outcome]
   - Result: [PASS / FAIL]

**Overall Manual Testing:** [PASS / FAIL - retest if fail]
```

**Manual Testing Best Practices:**
- Test happy path first (most common use case)
- Validate edge cases mentioned in core issue
- Verify error handling graceful (no crashes/exceptions)
- Document steps for reproducibility (others can verify)

---

### Automated Unit Testing

**Purpose:** Validate specific method/function behavior in isolation

**When to Use:**
- Core logic changes (business rules, validation, calculations)
- Bug fixes requiring regression prevention
- Algorithm implementations
- Service layer modifications

**Unit Testing Strategy:**
```
# Unit Testing Approach

**Target:** [Method/function being tested]
**Core Issue Validation:** [Which success criteria this validates]

**Test Cases:**
1. [TestName_Scenario_ExpectedBehavior]
   - Arrange: [Test data setup]
   - Act: [Method invocation]
   - Assert: [Expected outcome validation]
   - Purpose: [Which success criterion this validates]

2. [TestName_EdgeCase_ExpectedBehavior]
   - Arrange: [Test data setup]
   - Act: [Method invocation]
   - Assert: [Expected outcome validation]
   - Purpose: [Edge case coverage]

3. [TestName_ErrorCondition_ExpectedBehavior]
   - Arrange: [Test data setup]
   - Act: [Method invocation]
   - Assert: [Expected exception/error handling]
   - Purpose: [Error handling validation]

**Test Coverage:** [Percentage for modified methods]
**Pass Rate:** [Percentage]
```

**Unit Testing Best Practices:**
- Focus on code directly resolving core issue (not comprehensive coverage)
- Use descriptive test names (TestName_Scenario_ExpectedBehavior pattern)
- Keep tests simple and isolated (one assertion per test ideal)
- Validate success criteria explicitly (link tests to criteria)

---

### Automated Integration Testing

**Purpose:** Validate end-to-end functionality across system boundaries

**When to Use:**
- API endpoint changes (full request/response cycle)
- Database operations (data persistence verification)
- Multi-component workflows (service → repository → database)
- Frontend-backend integration

**Integration Testing Strategy:**
```
# Integration Testing Approach

**Workflow:** [End-to-end scenario being validated]
**Core Issue Validation:** [Which success criteria this validates]

**Integration Test Cases:**
1. [IntegrationTestName_Scenario]
   - Setup: [Test environment, database state, dependencies]
   - Execute: [Full workflow from entry point to completion]
   - Validate: [Expected outcome at each integration point]
   - Teardown: [Cleanup test data]
   - Purpose: [Which success criterion this validates]

2. [IntegrationTestName_EdgeCase]
   - [Same structure as above]

**Integration Points Validated:**
- [Component A] → [Component B]: [Validation approach]
- [Component B] → [Component C]: [Validation approach]

**Pass Rate:** [Percentage]
```

**Integration Testing Best Practices:**
- Test realistic end-to-end workflows (as users would invoke)
- Validate data persistence (database state after operations)
- Check integration points explicitly (boundaries between components)
- Clean up test data (avoid test pollution)

---

### Regression Testing

**Purpose:** Ensure existing functionality unaffected by core issue fix

**When to Use:**
- Always (every core issue implementation requires regression validation)
- Especially critical for bug fixes and refactoring
- Service/component modifications with existing test coverage

**Regression Testing Strategy:**
```
# Regression Testing Approach

**Scope:** [What existing functionality is being validated]

**Test Suites to Run:**
1. [TestSuiteName - e.g., UserServiceTests]
   - Test Count: [Number]
   - Pass Rate Before: [Baseline percentage]
   - Pass Rate After: [Current percentage]
   - Failures: [None / List failures]

2. [TestSuiteName - e.g., IntegrationTests]
   - Test Count: [Number]
   - Pass Rate Before: [Baseline percentage]
   - Pass Rate After: [Current percentage]
   - Failures: [None / List failures]

**Regression Detection:**
- New Failures: [None / List tests that previously passed]
- Failure Analysis: [Root cause if any new failures]
- Resolution: [How failures were addressed]

**Regression Status:** [NONE DETECTED ✅ / RESOLVED ✅ / UNRESOLVED ❌]
```

**Regression Testing Best Practices:**
- Run full test suite for modified component/service
- Baseline test results before changes (know starting pass rate)
- Investigate all new failures (don't ignore regressions)
- Fix regressions before marking mission complete (non-negotiable)

---

## SUCCESS CRITERIA VALIDATION APPROACHES

### Quantitative Criteria (Measurable Outcomes)

**Examples:**
- "API returns 200 OK for valid requests"
- "Test coverage increases from 60% to 80%"
- "Query performance under 100ms"
- "Zero NullReferenceExceptions thrown"

**Validation Approach:**
```
# Quantitative Criteria Validation

**Criterion:** [Measurable outcome from success criteria]

**Measurement Method:**
- [How to measure this quantitatively]
- [Tool/approach used for measurement]
- [Baseline value (if applicable)]

**Measurement Results:**
- Expected: [Target value]
- Actual: [Measured value]
- Status: [✅ MET / ❌ NOT MET / ⚠️ PARTIAL]

**Evidence:**
- [Test results, logs, metrics, screenshots]
- [Specific data points proving measurement]
```

**Best Practices:**
- Use objective measurements (numbers, percentages, counts)
- Establish baseline before implementation (know starting point)
- Measure consistently (same tool, same approach)
- Document evidence (screenshots, logs, test output)

---

### Qualitative Criteria (Behavioral Outcomes)

**Examples:**
- "Users can filter recipes by ingredient"
- "Error messages are user-friendly"
- "Code follows project standards"
- "Documentation is comprehensive"

**Validation Approach:**
```
# Qualitative Criteria Validation

**Criterion:** [Behavioral outcome from success criteria]

**Validation Method:**
- [How to validate this qualitatively]
- [Manual testing, code review, documentation review]
- [Specific scenarios to validate]

**Validation Results:**
- Scenario 1: [Description and result]
- Scenario 2: [Description and result]
- Scenario 3: [Description and result]
- Status: [✅ MET / ❌ NOT MET / ⚠️ PARTIAL]

**Evidence:**
- [Manual test results, code review findings]
- [User feedback, documentation review]
```

**Best Practices:**
- Define specific validation scenarios (make qualitative measurable)
- Use checklist approach (multiple scenarios = comprehensive validation)
- Document subjective assessments (rationale for qualitative judgment)
- Seek peer validation if uncertain (code review for standards compliance)

---

### Binary Criteria (Pass/Fail Outcomes)

**Examples:**
- "All existing tests pass"
- "No compiler warnings"
- "Build succeeds"
- "No breaking changes to API contracts"

**Validation Approach:**
```
# Binary Criteria Validation

**Criterion:** [Pass/fail outcome from success criteria]

**Validation Method:**
- [Command/tool to validate]
- [Expected output for pass]

**Validation Results:**
- Command: [Exact command executed]
- Output: [Relevant output]
- Status: [✅ PASS / ❌ FAIL]

**Evidence:**
- [Console output, CI/CD logs, build results]
```

**Best Practices:**
- Use automated validation where possible (build, test suite, linting)
- Document exact commands for reproducibility
- Zero tolerance for FAIL on binary criteria (must be addressed)
- Integrate into CI/CD for continuous validation

---

## ESCALATION TRIGGERS & GUIDANCE

### When to Escalate to Claude

**Escalation Trigger 1: Core Issue Unclear**
- **Symptom:** Cannot determine if change is required for core issue resolution
- **Example:** "Is adding logging essential for fixing this bug, or is it an enhancement?"
- **Escalation:** Request Claude clarify scope boundary and core issue requirements

**Escalation Trigger 2: Success Criteria Ambiguous**
- **Symptom:** Cannot objectively measure if success criterion is met
- **Example:** "Criterion says 'user-friendly error messages' - how to validate objectively?"
- **Escalation:** Request Claude provide specific validation approach or make criterion measurable

**Escalation Trigger 3: Scope Conflict**
- **Symptom:** Core issue resolution seems to require change outside scope boundary
- **Example:** "Fixing API bug requires database schema change, but schema not in scope"
- **Escalation:** Request Claude confirm scope expansion necessary or alternative approach

**Escalation Trigger 4: Success Criteria Not Achievable**
- **Symptom:** Success criterion cannot be met within scope boundary constraints
- **Example:** "Criterion requires 80% coverage but scope limits changes to 1 file with 50% coverage"
- **Escalation:** Request Claude clarify criterion or expand scope boundary

**Escalation Trigger 5: Recurring Mission Drift**
- **Symptom:** Multiple drift instances despite mission discipline attempts
- **Example:** "3rd time implementing improvements not in success criteria, unclear what's in scope"
- **Escalation:** Request Claude provide clearer scope definition or validate understanding

**Escalation Format:**
```
🚨 VALIDATION CHECKPOINT ESCALATION

**Checkpoint:** [Which checkpoint triggered escalation]
**Trigger:** [Which escalation trigger applies]

**Core Issue:** [Restate from context package]
**Success Criteria:** [List relevant criteria]
**Scope Boundary:** [Relevant scope constraints]

**Conflict/Ambiguity:**
[Specific question or conflict preventing validation]

**Attempted Resolution:**
[What was tried to resolve without escalation]

**Request:**
[Specific guidance needed from Claude to proceed]

**Proposed Options:**
1. [Option A with pros/cons]
2. [Option B with pros/cons]

**Recommendation:** [Agent's suggested approach]
```

---

### When to Request User Guidance

**User Guidance Trigger 1: Architectural Decision Required**
- **Symptom:** Core issue resolution requires architectural decision beyond agent authority
- **Example:** "Bug fix requires choosing between microservices split or monolith refactoring"
- **Escalation Path:** Claude → User for architectural decision

**User Guidance Trigger 2: Business Priority Conflict**
- **Symptom:** Core issue resolution conflicts with business requirements or user expectations
- **Example:** "Fixing bug breaks existing user workflow that relies on buggy behavior"
- **Escalation Path:** Claude → User for business priority decision

**User Guidance Trigger 3: Resource Constraints**
- **Symptom:** Core issue resolution requires resources beyond agent capability
- **Example:** "Integration testing requires external service access not available"
- **Escalation Path:** Claude → User for resource provisioning

**User Guidance Trigger 4: Timeline Conflict**
- **Symptom:** Success criteria cannot be met within expected timeline
- **Example:** "Comprehensive coverage requires 3 days, deadline is tomorrow"
- **Escalation Path:** Claude → User for timeline or scope adjustment

---

## SUMMARY: VALIDATION CHECKPOINT FRAMEWORK

### 5 Critical Checkpoints
1. **Planning Complete:** Before implementation, validate alignment and readiness
2. **First File Modified:** Early drift detection after initial change
3. **Halfway Through:** Mid-implementation progress and scope validation
4. **Implementation Complete:** Before testing, comprehensive scope review
5. **Success Criteria Validation:** Before completion, all criteria met confirmation

### 4 Testing Strategies
1. **Manual Testing:** User-facing validation and realistic scenarios
2. **Unit Testing:** Isolated method/function behavior validation
3. **Integration Testing:** End-to-end workflow and cross-component validation
4. **Regression Testing:** Existing functionality preservation validation

### 3 Criteria Types
1. **Quantitative:** Measurable outcomes (numbers, percentages, metrics)
2. **Qualitative:** Behavioral outcomes (user experience, code quality)
3. **Binary:** Pass/fail outcomes (build, tests, standards)

### 5 Escalation Triggers
1. **Core Issue Unclear:** Cannot determine if change is essential
2. **Criteria Ambiguous:** Cannot objectively measure criterion
3. **Scope Conflict:** Required change outside boundary
4. **Criteria Not Achievable:** Constraint prevents criterion completion
5. **Recurring Drift:** Multiple scope expansion instances

---

**Document Status:** ✅ Comprehensive validation checkpoint guide
**Usage:** Reference throughout implementation for milestone validation
**Integration:** Complements core-issue-focus skill with specific validation timing and approaches
