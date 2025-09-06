---
name: AI Test Coverage Task (Automated Epic)
about: Automated task for AI agents increasing test coverage as part of the 90% Coverage Epic
title: 'test: Increase Coverage for [Module/Class Name] - Epic #94'
labels: 'ai-task, testing, coverage, epic-subtask' # Add relevant module:X labels
assignees: '' # Automated execution - no human assignee

---

## 1. Epic Context

**Epic Reference:** [Backend Automation Testing Code Coverage to 90%](https://github.com/Zarichney-Development/zarichney-api/issues/94)  
**Execution Model:** Automated AI agent in GitHub Actions CI environment  
**Epic Branch:** `epic/testing-coverage-to-90` (off `develop`)  
**Task Branch Pattern:** `tests/issue-94-[description]-[timestamp]`

## 2. Automated Task Objective

**AI Agent Mission:** Systematically increase backend test coverage through self-directed scope selection and autonomous implementation of comprehensive test suites in an unconfigured CI environment.

**Coverage Phase Alignment:** Reference current coverage percentage and align implementation with appropriate phase:
- **Phase 1 (Current-20%):** Service layer basics, API contracts, core business logic
- **Phase 2 (20%-35%):** Service depth, integration scenarios, data validation  
- **Phase 3 (35%-50%):** Edge cases, error handling, boundary conditions
- **Phase 4 (50%-75%):** Complex business scenarios, integration depth
- **Phase 5 (75%-90%):** Comprehensive edge cases, performance scenarios

## 3. Self-Directed Scope Selection

**AI Agent must analyze and select appropriate scope:**

### Coverage Analysis Command
```bash
/test-report summary
# Analyze output to identify high-impact coverage opportunities
```

### Scope Selection Criteria
- **File-Level Focus:** Choose 1-3 related files to minimize conflicts with other agents
- **Reasonable Size:** 15-30 new test methods maximum per task
- **Impact Priority:** Prefer uncovered files first, then low-coverage areas
- **Agent Coordination:** Select different modules/namespaces from concurrent agents

### Target Area Selection
AI agent identifies specific production code files based on coverage analysis:
- Primary focus files in `/Zarichney.Server/[module]/`
- Related supporting classes requiring coverage
- Integration endpoints needing test coverage

## 4. CI Environment Execution Context

### Expected Test Environment
- **Execution Environment:** GitHub Actions CI (unconfigured)
**Expected Skip Count:** EXPECTED_SKIP_COUNT tests (fallback: 23 if unset; OpenAI: 6, Stripe: 6, MS Graph: 4, Database: 6, Production: 1; see Docs/Standards/TestingStandards.md section 12.7 for rationale)
- **Success Criteria:** 100% pass rate on ~65 executable tests
- **Framework:** All external dependencies properly mocked/virtualized

### Quality Gates (Non-Negotiable)
- [ ] All executable tests pass (100% pass rate maintained)
- [ ] Exactly EXPECTED_SKIP_COUNT tests skipped (fallback: 23 if unset; expected for unconfigured environment; see Docs/Standards/TestingStandards.md section 12.7 for rationale)
- [ ] If AI step shows `skipped_quota_window` during scheduled runs, treat as a successful interval and re-run after subscription refresh window
- [ ] No existing tests broken by new implementations
- [ ] All new tests deterministic and repeatable in CI
- [ ] Coverage improvement measurable and documented

## 5. Implementation Requirements

### Mandatory Standards Adherence
**Review before implementation:** 
- [ ] `/Docs/Standards/TestingStandards.md` - Core testing principles
- [ ] `/Docs/Standards/UnitTestCaseDevelopment.md` - Unit test implementation
- [ ] `/Docs/Standards/IntegrationTestCaseDevelopment.md` - Integration test patterns
- [ ] `/Docs/Development/AutomatedCoverageEpicWorkflow.md` - Automated workflow process
- [ ] Local module `README.md` files for target areas

### Test Implementation Standards (MUST)
- [ ] **Framework Usage (Non-Negotiable):**
  - [ ] Integration tests inherit from `IntegrationTestBase` or `DatabaseIntegrationTestBase`
  - [ ] Use `ApiClientFixture`, `CustomWebApplicationFactory`, Refit clients
  - [ ] Apply `[DependencyFact]` with proper traits from `TestCategories`
  - [ ] Use test data builders/AutoFixture (no manual DTO construction)
  - [ ] Mock all external dependencies with framework mocks
- [ ] **Zero Brittle Tests:**
  - [ ] No sleeps or time-based waits
  - [ ] Deterministic data generation with fixed seeds
  - [ ] Proper fixture usage for isolation
- [ ] Unit tests: Complete isolation with Moq for all dependencies
- [ ] Integration tests: Use `CustomWebApplicationFactory`, `DatabaseFixture`, Refit clients
- [ ] Test categories: Appropriate `[Trait("Category", "...")]` attributes applied
- [ ] Naming conventions: `[MethodName]_[Scenario]_[ExpectedOutcome]` pattern
- [ ] AAA pattern: Clear Arrange-Act-Assert structure in all tests
- [ ] FluentAssertions: Use for all test assertions with `.Because("...")` explanations

### Framework Enhancement (In Scope)
When beneficial for scalability, include:
- [ ] Test data builders for complex objects (`/TestData/Builders/`)
- [ ] Mock factories for reusable configurations (`/Framework/Mocks/`)
- [ ] Testing utilities for common patterns (`/Framework/Helpers/`)
- [ ] AutoFixture customizations (`/Framework/TestData/AutoFixtureCustomizations/`)

### Production Changes Protocol
If tests reveal production issues:
1. **Evaluate fix scope** using decision tree in AutomatedCoverageEpicWorkflow.md
2. **If safe for inline fix:**
   - Keep changes minimal and behavior-preserving
   - Add tests proving the fix
   - Document in Implementation Summary
3. **If requires separate issue:**
   - Create GitHub issue with discovered problem
   - Continue with coverage work
   - Reference issue in PR description

## 6. Automated Validation & Delivery

### Pre-Commit Validation
```bash
# Comprehensive test execution
/test-report summary

# Build validation  
dotnet build zarichney-api.sln

# Format verification
dotnet format zarichney-api.sln --verify-no-changes
```

### Automated PR Creation
```bash
# Commit with epic reference
git commit -m "test: increase coverage for [Area] (#94)"

# Create PR against epic branch
gh pr create --base epic/testing-coverage-to-90 \
  --title "test: [Description] (#94)" \
  --body "[Generated PR body with coverage summary]"
```

## 7. Production Issue Discovery Protocol

### If New Tests Reveal Production Bugs
- **Document in PR:** Clearly note discovered issues in PR description
- **Separate Issue:** Create dedicated GitHub issue for production bug fix
- **Recommendation:** Advise fixing production code before merging coverage improvements
- **Continue Work:** Coverage improvements remain valid regardless of production issues

### Production Bug Issue Template
```markdown
**Discovered By:** Coverage testing in PR #[PR_NUM]
**Issue:** [Description of production bug]
**Reproduction:** See failing tests in [PR link]
**Action Required:** Fix production code before merging coverage improvements
```

## 8. Success Metrics

### Task Completion Criteria
- [ ] Epic branch updated from develop before starting work
- [ ] Coverage gaps identified through `/test-report` analysis
- [ ] Appropriate scope selected (1-3 files, 15-30 test methods)
- [ ] All new tests implemented following established standards
- [ ] 100% pass rate maintained on executable tests (23 skipped acceptable)
- [ ] Measurable coverage improvement achieved  
- [ ] Framework enhancements included where beneficial
- [ ] Pull request created with comprehensive documentation
- [ ] Production issues properly handled if discovered

### Epic Contribution
Each task contributes to:
- **Coverage Velocity:** ~2.8% monthly increase toward 90% target
- **Framework Scalability:** Enhanced testing infrastructure
- **Quality Maintenance:** ≥99% test pass rate throughout progression
- **January 2026 Goal:** 90% backend code coverage achievement

---

**Execution Model:** Autonomous AI Agent (GitHub Actions CI)  
**Epic Timeline:** Now → January 2026  
**Success Definition:** Measurable coverage improvement with enhanced framework scalability
