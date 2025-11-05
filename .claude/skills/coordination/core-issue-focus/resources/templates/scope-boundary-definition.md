# Scope Boundary Definition Template

**Date:** [YYYY-MM-DD]
**Agent:** [Agent Name]
**GitHub Issue:** #[number]
**Core Issue:** [Brief one-sentence description]

---

## IN SCOPE (Core Issue Resolution)

### Specific File Modifications

**File 1:** [Absolute file path]
- **Current State:** [Brief description of current implementation/problem]
- **Required Change:** [Exact modification needed - method, property, logic change]
- **Rationale:** [Why this change is essential for core issue resolution]

**File 2:** [Absolute file path]
- **Current State:** [Brief description of current implementation/problem]
- **Required Change:** [Exact modification needed - method, property, logic change]
- **Rationale:** [Why this change is essential for core issue resolution]

**File 3 (Test):** [Absolute test file path]
- **Current State:** [Current test coverage status]
- **Required Change:** [New test case or test modification needed]
- **Rationale:** [Why this test validates core issue resolution]

**[Add additional files as needed - keep surgical, minimize file count]**

---

### Validation Tests

**Test 1:** [Test name/description]
- **Purpose:** [What success criteria this test validates]
- **Expected Outcome:** [Specific test result proving core issue resolved]

**Test 2:** [Test name/description]
- **Purpose:** [What success criteria this test validates]
- **Expected Outcome:** [Specific test result proving core issue resolved]

**[Add additional tests as needed - focus on core functionality validation]**

---

## OUT OF SCOPE (Deferred Improvements)

### Secondary Improvement 1: [Improvement name]
- **Description:** [What improvement was identified]
- **Why Valid:** [Rationale for why this is a good improvement]
- **Why Deferred:** [Rationale for why this is out-of-scope for current mission]
- **Future Work:** [Where this should be addressed - issue #, epic name, backlog location]
- **Estimated Impact:** [Potential benefit if implemented in future]

### Secondary Improvement 2: [Improvement name]
- **Description:** [What improvement was identified]
- **Why Valid:** [Rationale for why this is a good improvement]
- **Why Deferred:** [Rationale for why this is out-of-scope for current mission]
- **Future Work:** [Where this should be addressed - issue #, epic name, backlog location]
- **Estimated Impact:** [Potential benefit if implemented in future]

### Opportunistic Refactoring: [Refactoring opportunity]
- **Description:** [What refactoring was identified]
- **Why Deferred:** [Why this doesn't directly resolve core blocking problem]
- **Future Work:** [Tech debt backlog, separate refactoring epic]

**[Add additional deferred improvements as identified - comprehensive documentation]**

---

## INTEGRATION BOUNDARIES

### Affected System 1: [System/component name]
- **Integration Type:** [Read-only dependency / Modification required / No interaction]
- **Changes Needed:** [Specific integration changes if any]
- **Validation:** [How to verify integration compatibility]

### Affected System 2: [System/component name]
- **Integration Type:** [Read-only dependency / Modification required / No interaction]
- **Changes Needed:** [Specific integration changes if any]
- **Validation:** [How to verify integration compatibility]

**[Add additional affected systems - keep minimal, document why each is necessary]**

---

## FORBIDDEN SCOPE EXPANSIONS (From Context Package)

**Prohibited Actions:**
- [ ] Infrastructure improvements while core issue unfixed
- [ ] Working directory protocols during syntax error fixes
- [ ] Feature additions not directly related to core problem
- [ ] Cross-agent coordination enhancements during single-issue fixes
- [ ] [Custom forbidden pattern 1 from context package]
- [ ] [Custom forbidden pattern 2 from context package]

**Validation:**
- [ ] No forbidden scope expansions attempted during implementation
- [ ] All temptations to expand scope documented in deferred list
- [ ] Mission discipline maintained throughout

---

## SCOPE COMPLIANCE VALIDATION

### File Count Validation
- **Planned File Modifications:** [Number from IN SCOPE section]
- **Actual File Modifications:** [Number after implementation - fill during validation]
- **Delta:** [Actual - Planned]
- **Explanation if Delta > 0:** [Justification for any additional files modified]

### Scope Boundary Adherence
- [ ] All modified files listed in IN SCOPE section
- [ ] No OUT OF SCOPE improvements implemented
- [ ] All deferred improvements documented for future work
- [ ] Integration boundaries respected (minimal necessary changes)

### Mission Drift Detection
- [ ] No "while I'm here" syndrome violations
- [ ] No "perfect is the enemy of done" continuation beyond success criteria
- [ ] No "consistency refactoring" across unrelated code
- [ ] No "opportunistic testing" beyond core issue validation
- [ ] No "infrastructure enhancement" during core fix

---

## ALIGNMENT WITH CONTEXT PACKAGE

**SCOPE_BOUNDARY from Claude's context package:**
```
[Paste exact SCOPE_BOUNDARY field from context package Claude provided]
```

**Alignment Verification:**
- [ ] IN SCOPE files match SCOPE_BOUNDARY definition
- [ ] No modifications planned outside SCOPE_BOUNDARY authorization
- [ ] Integration boundaries align with context package constraints
- [ ] Agent authority validated for all planned modifications

---

## EXAMPLE USAGE

### Example 1: API Bug Fix (UserService.GetUserById)

**IN SCOPE:**
- Code/Zarichney.Server/Services/UserService.cs: Add null check for user.Profile
- Code/Zarichney.Server.Tests/Services/UserServiceTests.cs: Add null profile test case

**OUT OF SCOPE:**
- Refactor UserService to async/await → Issue #ABC
- Standardize error handling across API → Epic #DEF
- Add caching for user lookups → Tech debt backlog

**INTEGRATION BOUNDARIES:**
- Database: Read-only dependency, no schema changes needed
- API Controllers: No changes needed, UserService contract unchanged

### Example 2: Test Coverage Addition (RecipeService)

**IN SCOPE:**
- Code/Zarichney.Server.Tests/Services/RecipeServiceTests.cs: Add tests for CreateRecipe validation

**OUT OF SCOPE:**
- Refactor RecipeServiceTests base class → Tech debt backlog
- Add tests for other RecipeService methods → Separate coverage issue
- Improve test data builders → Test infrastructure epic

**INTEGRATION BOUNDARIES:**
- RecipeService: Read-only analysis, no production code changes
- Test Database: Use existing test fixtures, no schema changes

---

**Template Status:** Ready to use
**Usage:** Copy this template, fill in all bracketed placeholders, use to define surgical scope boundaries
**Integration:** Use with core-issue-focus skill Step 2 (Surgical Scope Definition)
