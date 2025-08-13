# Automated Coverage Epic Workflow for AI Agents

**Version:** 1.0
**Last Updated:** 2025-08-11
**Epic Reference:** [Issue #94](https://github.com/Zarichney-Development/zarichney-api/issues/94)

## 1. Purpose & Context

This workflow defines the **automated execution process** for AI agents working on the Backend Testing Coverage Epic in a GitHub Actions CI environment. Unlike human-centric workflows, this process is designed for autonomous, non-interactive execution with multiple agents working in parallel.

**Key Characteristics:**
- **Environment:** GitHub Actions CI (unconfigured - external services unavailable)
- **Frequency:** 4 AI agent instances per day (6-hour cron intervals)
- **Coordination:** Multiple agents working simultaneously with conflict prevention
- **Success Criteria:** 100% pass rate on ~65 executable tests (23 skipped acceptable)

## 2. Pre-Execution Environment Validation

### 2.1 Expected Test Environment State
Before beginning any coverage work, validate the CI environment:

```bash
# Quick environment check
/test-report summary

# Expected output characteristics:
# - Total Tests: ~88 
# - Executable Tests: ~65
# - Skipped Tests: 23 (OpenAI: 6, Stripe: 6, MS Graph: 4, Database: 6, Production: 1)
# - Pass Rate: 100% on executable tests
```

### 2.2 Success Criteria Validation

The following conditions **MUST** be met before proceeding:
- ✅ All executable tests pass (100% pass rate)
- ✅ Skip count matches EXPECTED_SKIP_COUNT (default: 23; see Docs/Standards/TestingStandards.md section 12.7 for rationale)
- ✅ No unexpected test failures or errors
- ✅ Coverage baseline can be established

**If validation fails:** Abort execution and report environment issues.

## 3. Epic Branch Management

### 3.1 Epic Branch Creation/Update
Always ensure the epic branch is current with develop before starting work:

```bash
# Step 1: Ensure clean working directory
git status
git reset --hard HEAD  # Clean any uncommitted changes

# Step 2: Update develop branch
git checkout develop
git pull origin develop

# Step 3: Create or update epic branch
if git rev-parse --verify epic/testing-coverage-to-90 >/dev/null 2>&1; then
    # Epic branch exists - update it
    git checkout epic/testing-coverage-to-90
    git merge develop --no-edit  # Merge latest develop changes
else
    # Epic branch doesn't exist - create it
    git checkout -b epic/testing-coverage-to-90
fi

# Step 4: Push epic branch (creates/updates remote)
git push origin epic/testing-coverage-to-90
```

### 3.2 Epic Branch Validation
Verify epic branch is ready for task branches:

```bash
# Confirm epic branch is current
git log --oneline -n 5  # Should show recent develop commits

# Validate test suite still passes on epic branch
/test-report summary
# Must maintain 100% pass rate on executable tests
```

## 4. Coverage Analysis & Task Scoping

### 4.1 Coverage Gap Identification
Use the unified test suite to identify coverage opportunities:

```bash
# Get comprehensive coverage analysis
/test-report

# Focus areas for analysis:
# 1. Uncovered files/classes (Priority 1)
# 2. Low coverage areas (Priority 2) 
# 3. Missing edge cases (Priority 3)
# 4. Integration gaps (Priority 4)
```

### 4.2 Automated Scope Selection
AI agents must self-select appropriate scope based on:

#### **Scope Selection Criteria**
- **File-Level Focus:** Choose 1-3 related files to avoid conflicts with other agents
- **Reasonable Size:** 15-30 new test methods maximum per task
- **Phase Alignment:** Match current coverage phase (see Epic #94 for phases)
- **Impact Assessment:** Prefer high-impact, low-effort opportunities

#### **Coverage Phase Guidelines**
Reference current coverage percentage and align scope:
- **Phase 1 (Current-20%):** Basic service methods, controller actions, entity validation
- **Phase 2 (20%-35%):** Complex service scenarios, repository patterns, auth flows
- **Phase 3 (35%-50%):** Edge cases, error handling, boundary conditions
- **Phase 4 (50%-75%):** Complex business scenarios, deep integration
- **Phase 5 (75%-90%):** Comprehensive edge cases, performance scenarios

#### **Agent Coordination Strategy**
To prevent conflicts between simultaneous agents:
- **File Selection:** Choose files in different modules/namespaces
- **Time-Based Offset:** Use current timestamp to influence selection
- **Coverage Gap Partitioning:** Focus on different coverage dimensions (unit vs integration)

## 5. Task Branch Creation & Implementation

### 5.1 Task Branch Naming & Creation
Create descriptive task branches off the epic branch:

```bash
# Generate unique task branch name
ISSUE_ID=94  # Epic issue ID
TIMESTAMP=$(date +%s)  # Add timestamp for uniqueness
TASK_AREA="[module-name]"  # e.g., "cookbook-service", "auth-controller"

# Create task branch
BRANCH_NAME="tests/issue-${ISSUE_ID}-${TASK_AREA}-${TIMESTAMP}"
git checkout -b $BRANCH_NAME

# Example: tests/issue-94-cookbook-service-1628712345
```

### 5.2 Standards Review (Mandatory)
Before implementation, review **ALL** relevant standards:

```bash
# Required reading (verify files exist and review):
# 1. Core testing standards
cat /home/zarichney/workspace/zarichney-api/Docs/Standards/TestingStandards.md

# 2. Unit test development guide  
cat /home/zarichney/workspace/zarichney-api/Docs/Standards/UnitTestCaseDevelopment.md

# 3. Integration test development guide
cat /home/zarichney/workspace/zarichney-api/Docs/Standards/IntegrationTestCaseDevelopment.md

# 4. Task management standards
cat /home/zarichney/workspace/zarichney-api/Docs/Standards/TaskManagementStandards.md

# 5. Module-specific README (if applicable)
find . -name "README.md" -path "*/[target-module]/*"
```

### 5.3 Implementation Guidelines

#### **Test Development Priorities**
1. **Uncovered Code First:** Target 0% coverage files/methods
2. **Framework Enhancements:** Improve testing infrastructure when beneficial
3. **Edge Case Coverage:** Address boundary conditions and error scenarios
4. **Integration Depth:** Expand API endpoint coverage

#### **Quality Standards (Non-Negotiable)**
- **100% Pass Rate:** All new tests must pass consistently
- **Standards Compliance:** Follow all testing standards exactly
- **Deterministic Behavior:** Tests must be repeatable in CI environment
- **Performance Conscious:** Optimize for CI execution time

#### **Framework Enhancement Opportunities**
When implementing tests, consider these framework improvements:
- **Test Data Builders:** Create/enhance builders for complex objects
- **Mock Factories:** Develop reusable mock configurations
- **Helper Utilities:** Add testing utilities for common patterns
- **AutoFixture Customizations:** Improve test data generation

## 6. Validation & Quality Gates

### 6.1 Pre-Commit Validation
Before committing, ensure all quality gates pass:

```bash
# Step 1: Comprehensive test execution
/test-report summary

# Required outcomes:
# ✅ All tests pass (100% pass rate on executable)  
# ✅ 23 tests skipped (expected)
# ✅ No new test failures
# ✅ Coverage improvement visible

# Step 2: Build validation
dotnet build zarichney-api.sln
# Must complete successfully

# Step 3: Code formatting
dotnet format zarichney-api.sln --verify-no-changes
# Should require no changes (already formatted)
```

### 6.2 Coverage Impact Validation
Confirm coverage improvement:

```bash
# Generate detailed coverage report
/test-report --performance

# Validate improvements:
# ✅ Line coverage increased
# ✅ Branch coverage improved  
# ✅ New test methods added
# ✅ Framework enhancements documented
```

## 7. Commit & Pull Request Creation

### 7.1 Automated Commit Process
Use consistent commit messages following Conventional Commits:

```bash
# Stage all changes
git add .

# Create descriptive commit message
COMMIT_MSG="test: increase coverage for [Area] (#94)

- Added [X] unit tests for [Class/Module]
- Added [Y] integration tests for [API/Feature]  
- Enhanced [Framework Component] for better scalability
- Achieved [Coverage %] improvement in [Area]

Refs #94"

# Commit changes
git commit -m "$COMMIT_MSG"

# Push task branch
git push origin $BRANCH_NAME
```

### 7.2 Automated Pull Request Creation
Create standardized PRs targeting the epic branch:

```bash
# Generate PR title and body
PR_TITLE="test: increase coverage for [Area] (#94)"

PR_BODY="## Coverage Improvement Summary

**Target Area:** [Module/Class name]
**Coverage Phase:** [1-5 based on current phase]
**Test Types Added:** [Unit/Integration/Both]

### Changes Made
- Added [X] unit tests covering [specific functionality]
- Added [Y] integration tests covering [API endpoints/scenarios]
- Enhanced testing framework: [specific improvements]
- Achieved [X]% coverage improvement in target area

### Quality Validation
- ✅ All tests pass (100% pass rate on $X executable tests)
- ✅ 23 tests properly skipped in CI environment
- ✅ No existing tests broken
- ✅ Follows all testing standards

### Framework Enhancements (if applicable)
- [List any testing framework improvements made]

**Epic Reference:** Closes #94 (partial - coverage epic ongoing)
**Phase Alignment:** [Description of how this aligns with current coverage phase]"

# Create Pull Request
gh pr create \
  --base epic/testing-coverage-to-90 \
  --title "$PR_TITLE" \
  --body "$PR_BODY" \
  --label "ai-task,testing,coverage,epic-subtask"
```

## 8. Error Handling & Production Issue Discovery

### 8.1 Test Failure Investigation
If new tests reveal production issues:

```bash
# Step 1: Analyze failure
/test-report --performance
# Determine if failure is due to:
# - Test implementation error
# - Production code bug
# - Environment/configuration issue

# Step 2: Document findings in PR
# If production bug discovered, update PR body with:
# "⚠️ PRODUCTION ISSUE DISCOVERED: [Description]
#  - Recommend creating separate issue for bug fix
#  - This PR demonstrates the issue via failing tests
#  - Suggest fixing production code before merging coverage improvements"
```

### 8.2 Production Issue Reporting
When production bugs are discovered:

```bash
# Create separate GitHub issue for production bug
gh issue create \
  --title "bug: [Description] discovered by coverage testing" \
  --body "**Discovered By:** Coverage testing in PR #[PR_NUM]
  
  **Issue Description:** [Detailed description]
  
  **Reproduction:** See failing tests in [PR link]
  
  **Recommended Action:** Fix production code before merging coverage improvements
  
  **Priority:** [High/Medium/Low based on impact]" \
  --label "bug,production-issue,coverage-discovered"
```

## 9. CI Environment Considerations

### 9.1 Resource Optimization
Optimize for GitHub Actions CI constraints:

- **Parallel Execution:** Leverage xUnit parallel test execution
- **Memory Usage:** Monitor memory consumption in test implementations  
- **Execution Time:** Target test methods that complete in <1 second each
- **Docker Resources:** Efficient TestContainer usage for integration tests

### 9.2 Deterministic Test Design
Ensure tests work reliably in CI:

- **No External Dependencies:** All external services properly mocked/virtualized
- **Time-Independent:** Avoid time-based assertions that could be flaky
- **Resource Cleanup:** Proper disposal of test resources
- **Data Isolation:** Complete test data isolation between test runs

## 10. Success Metrics & Reporting

### 10.1 Task Completion Criteria
Each automated task is considered successful when:

- ✅ Epic branch updated from develop
- ✅ Coverage gaps identified and scoped appropriately  
- ✅ New tests implemented following all standards
- ✅ 100% pass rate maintained on executable tests
- ✅ Coverage improvement measurable
- ✅ Framework enhancements included (when applicable)
- ✅ Pull request created with comprehensive documentation
- ✅ No production issues discovered (or properly reported if found)

### 10.2 Daily Success Targets
Each day should result in:
- **4 Pull Requests** created against epic branch
- **Measurable Coverage Improvement** across different areas
- **Zero Test Regressions** in existing test suite
- **Framework Enhancements** improving testing scalability

## 11. Troubleshooting & Common Issues

### 11.1 Epic Branch Merge Conflicts
If epic branch conflicts with develop:

```bash
# Resolve conflicts automatically where possible
git checkout epic/testing-coverage-to-90
git merge develop

# If conflicts occur, reset epic branch (CAUTION: Only in CI)
git reset --hard develop
git push --force-with-lease origin epic/testing-coverage-to-90
```

### 11.2 Test Suite Failure Recovery
If test suite fails unexpectedly:

```bash
# Clean build and retry
dotnet clean zarichney-api.sln
dotnet build zarichney-api.sln
/test-report summary

# If still failing, abort task and report issue
echo "CI environment test failure - aborting automated task"
exit 1
```

### 11.3 Coverage Analysis Errors
If coverage tools fail:

```bash
# Alternative coverage analysis
dotnet test zarichney-api.sln --collect:"XPlat Code Coverage"
# Manual analysis of coverage files in TestResults/

# Fallback to file-based analysis
find . -name "*.cs" -path "*/Zarichney.Server/*" ! -path "*/bin/*" ! -path "*/obj/*"
# Identify uncovered files manually
```

---

**Document Owner:** Development Team  
**Epic Reference:** [Issue #94](https://github.com/Zarichney-Development/zarichney-api/issues/94)  
**Execution Context:** GitHub Actions CI - Autonomous AI Agents  
**Success Definition:** 4 coverage improvement PRs per day with 100% test pass rate