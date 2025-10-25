# Core Issue Analysis Template

**Date:** [YYYY-MM-DD]
**Agent:** [Agent Name - TestEngineer, PromptEngineer, CodeChanger, BackendSpecialist, FrontendSpecialist, WorkflowEngineer]
**GitHub Issue:** #[number]
**Epic Context:** [Epic name/number if applicable]

---

## Core Blocking Problem

**What specific technical problem is blocking progress?**

[One-sentence clear description of the blocking issue. Be precise and specific.]

**Example:** "UserService.GetUserById endpoint throws NullReferenceException when user has no associated profile"

---

## Why This Blocks Progress

**What can't be done until this is fixed?**

[Explanation of impact - what functionality is broken, what user workflow is blocked, what development is stalled]

**Example:** "Users with incomplete profiles cannot be retrieved, breaking user profile display functionality and preventing admin user management workflows"

---

## Minimum Viable Fix

**What is the minimum change needed to resolve the blocker?**

[Description of surgical fix scope - what specific files/methods need modification, what exact change resolves the issue]

**Example:** "Add null check for user.Profile property before accessing Profile.DisplayName in UserService.GetUserById method"

---

## What Approaches are OUT OF SCOPE (Secondary Improvements)

**List valid improvements that should be deferred to future work:**

- [Improvement 1 - defer to future issue/epic]
  - **Why Valid:** [Rationale for why this is a good improvement]
  - **Why Deferred:** [Rationale for why this is out-of-scope for current mission]

- [Improvement 2 - defer to future issue/epic]
  - **Why Valid:** [Rationale for why this is a good improvement]
  - **Why Deferred:** [Rationale for why this is out-of-scope for current mission]

- [Improvement 3 - defer to future issue/epic]
  - **Why Valid:** [Rationale for why this is a good improvement]
  - **Why Deferred:** [Rationale for why this is out-of-scope for current mission]

**Example:**
- Refactoring UserService to async/await pattern
  - **Why Valid:** Improves performance and scalability for user data operations
  - **Why Deferred:** Not required to fix null reference exception, separate architectural improvement

- Standardizing error handling across all API endpoints
  - **Why Valid:** Creates consistent error responses and improves API reliability
  - **Why Deferred:** Addresses broader concern than current blocker, requires comprehensive API review

---

## Success Criteria

**How will we know the core issue is resolved?**

1. [ ] [Testable outcome 1 - specific, measurable, achievable]
2. [ ] [Testable outcome 2 - specific, measurable, achievable]
3. [ ] [Testable outcome 3 - specific, measurable, achievable]
4. [ ] [Testable outcome 4 - specific, measurable, achievable - optional]
5. [ ] [Testable outcome 5 - specific, measurable, achievable - optional]

**Example:**
1. [ ] GetUserById returns 200 OK for users with complete profiles (existing functionality preserved)
2. [ ] GetUserById returns 200 OK for users with null/incomplete profiles (blocker resolved)
3. [ ] No NullReferenceException thrown in any user retrieval scenario
4. [ ] Unit test added validating null profile handling
5. [ ] All existing UserService tests continue passing (no regression)

---

## Testing Approach

**How to validate the fix actually works:**

[Description of manual testing steps, automated test scenarios, validation approach]

**Example:**
- **Manual Testing:**
  1. Create test user with complete profile - verify GetUserById returns 200 OK
  2. Create test user with null profile - verify GetUserById returns 200 OK without exception
  3. Validate response data structure matches expected format

- **Automated Testing:**
  1. Add unit test: GetUserById_UserWithNullProfile_ReturnsSuccessResponse
  2. Run full UserServiceTests suite - confirm 100% pass rate
  3. Run integration tests - verify end-to-end user retrieval workflows

---

## Scope Boundaries

### IN SCOPE (Core Issue Resolution)

**Specific file modifications needed:**
- [File path 1]: [Exact modification - method, property, logic change]
- [File path 2]: [Exact modification - method, property, logic change]
- [Test file path]: [New test case or test modification]

**Example:**
- Code/Zarichney.Server/Services/UserService.cs: Add null check for user.Profile before accessing Profile.DisplayName
- Code/Zarichney.Server.Tests/Services/UserServiceTests.cs: Add GetUserById_UserWithNullProfile_ReturnsSuccessResponse test

### OUT OF SCOPE (Deferred Improvements)

**Valid improvements to defer to future work:**
- [Secondary improvement 1] → Create separate issue: [Issue name/epic]
- [Secondary improvement 2] → Create separate issue: [Issue name/epic]
- [Opportunistic refactoring] → Add to tech debt backlog

**Example:**
- Refactor UserService to async/await → Create Issue #ABC: "Migrate UserService to async patterns"
- Standardize API error handling → Add to Epic #DEF: "API consistency improvements"
- Improve user profile validation → Add to tech debt backlog: "Data validation enhancements"

---

## Mission Drift Checkpoints

### Checkpoint 1: After Implementation
- [ ] Review all file modifications against IN SCOPE list
- [ ] Validate all changes directly resolve core blocking problem
- [ ] Confirm no files modified outside scope boundary definition

### Checkpoint 2: Deferred List Review
- [ ] Review OUT OF SCOPE list - confirm no deferred work was implemented
- [ ] Validate deferred improvements captured in GitHub issues or backlog
- [ ] Confirm tempting improvements were resisted

### Checkpoint 3: Success Criteria Validation
- [ ] All success criteria met (100% completion)
- [ ] Core functionality works as intended (manual and automated testing)
- [ ] No scope expansion occurred during implementation

---

## Alignment with Context Package

**CORE_ISSUE from Claude's context package:**
```
[Paste exact CORE_ISSUE field from context package Claude provided]
```

**Alignment Verification:**
- [ ] Core blocking problem matches CORE_ISSUE definition
- [ ] Minimum viable fix addresses CORE_ISSUE scope
- [ ] Success criteria validate CORE_ISSUE resolution
- [ ] Agent and Claude aligned on mission scope

---

**Template Status:** Ready to use
**Usage:** Copy this template, fill in all bracketed placeholders, use to maintain mission discipline
**Integration:** Use with core-issue-focus skill Step 1 (Identify Core Issue First)
