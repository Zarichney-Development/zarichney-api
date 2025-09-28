---
name: AI Test Coverage Task (Automated Excellence)
about: Automated task for AI agents increasing test coverage as part of the Coverage Excellence Initiative
title: 'test: Increase Coverage for [Module/Class Name] - Coverage Excellence'
labels: 'ai-task, testing, coverage, epic-subtask' # Add relevant module:X labels
assignees: '' # Automated execution - no human assignee

---

## 1. Epic Context

**Coverage Excellence Reference:** Backend Testing Coverage Excellence Initiative
**Execution Model:** Automated AI agent in GitHub Actions CI environment
**Coverage Branch:** `epic/testing-coverage` (off `develop`)
**Task Branch Pattern:** `tests/coverage-excellence-[description]-[timestamp]`

**Orchestrator Integration:** Individual task PRs will be consolidated by the [Coverage Excellence Merge Orchestrator](../../Docs/Development/CoverageEpicMergeOrchestration.md) for efficient coverage progression. Your task contributes to systematic coverage improvement while the orchestrator handles PR coordination and conflict resolution.

## 2. Automated Task Objective

**AI Agent Mission:** Systematically increase backend test coverage through self-directed scope selection and autonomous implementation of comprehensive test suites in an unconfigured CI environment.

**Coverage Phase Alignment:** Reference current coverage level and align implementation with appropriate phase:
- **Phase 1 (Basic Coverage):** Service layer basics, API contracts, core business logic
- **Phase 2 (Expanded Coverage):** Service depth, integration scenarios, data validation
- **Phase 3 (Comprehensive Coverage):** Edge cases, error handling, boundary conditions
- **Phase 4 (Advanced Coverage):** Complex business scenarios, integration depth
- **Phase 5 (World-Class Coverage):** Comprehensive edge cases, performance scenarios

## 3. Orchestrator Consolidation Context

**Coverage Excellence Pipeline Understanding:**
- **Your Task (Phase 1):** Create focused coverage improvements in individual PR
- **Orchestrator Role (Phase 2):** Automatic direct merging of multiple coverage PRs into epic branch
- **Epic Integration (Phase 3):** Product owner integration to develop/main

**Individual Task Responsibilities:**
- Focus on 1-3 related files to minimize orchestrator conflicts
- Implement 15-30 test methods with comprehensive framework enhancements
- Ensure 100% test pass rate and standards compliance
- Create clear, descriptive PR with consolidation-friendly structure

**Orchestrator Benefits for Your Work:**
- Automatic conflict resolution for test framework overlaps
- Coordinated integration with other AI agent contributions
- Comprehensive quality validation across consolidated changes
- Reduced manual PR management overhead for product owners

## 4. Self-Directed Scope Selection

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
- **Agent Coordination:** Select different modules/namespaces from concurrent agents (orchestrator-aware)

### Target Area Selection
AI agent identifies specific production code files based on coverage analysis:
- Primary focus files in `/Zarichney.Server/[module]/`
- Related supporting classes requiring coverage
- Integration endpoints needing test coverage

## 5. CI Environment Execution Context

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

## 6. Implementation Requirements

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
- [ ] FluentAssertions: Use for all test assertions and include clear reasons via the assertion's optional message parameter

### Framework Enhancement Opportunities (Orchestrator-Aware)
When implementing tests, consider these framework improvements that coordinate well with orchestrator consolidation:
- [ ] **Test Data Builders:** Create/enhance builders for complex objects (designed for multi-agent reuse)
- [ ] **Mock Factories:** Develop reusable mock configurations (orchestrator will consolidate overlaps)
- [ ] **Helper Utilities:** Add testing utilities for common patterns (framework-wide integration)
- [ ] **AutoFixture Customizations:** Improve test data generation (orchestrator-friendly patterns)

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

## 7. Automated Validation & Delivery

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
gh pr create --base epic/testing-coverage \
  --title "test: [Description] (#94)" \
  --body "[Generated PR body with coverage summary]"
```

## 8. Production Issue Discovery Protocol

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

## 9. Task Completion & Orchestrator Integration Criteria

Each automated task is considered successful when:

**Individual Task Success:**
- [ ] Epic branch updated from develop before starting work
- [ ] Coverage gaps identified through `/test-report` analysis
- [ ] Appropriate scope selected (1-3 files, 15-30 test methods)
- [ ] All new tests implemented following established standards
- [ ] 100% pass rate maintained on executable tests (23 skipped acceptable)
- [ ] Measurable coverage improvement achieved
- [ ] Framework enhancements included where beneficial
- [ ] Pull request created with comprehensive documentation
- [ ] Production issues properly handled if discovered

**Orchestrator Integration Readiness:**
- [ ] PR labeled appropriately for orchestrator discovery (`coverage`, `testing`, `ai-task`)
- [ ] Focused scope (1-3 files) to minimize consolidation conflicts
- [ ] Clear PR description enabling effective orchestrator consolidation
- [ ] Test framework enhancements compatible with multi-agent coordination
- [ ] Production changes limited to testability improvements (orchestrator-safe)

## 10. Coverage Excellence Merge Orchestrator Integration

**Orchestrator Documentation:** See [Coverage Excellence Merge Orchestration Guide](../../Docs/Development/CoverageEpicMergeOrchestration.md) for complete consolidation workflow understanding.

**Orchestrator Execution:** Product owners execute orchestrator manually or via scheduling:
```bash
# Consolidate multiple coverage PRs
gh workflow run "Coverage Excellence Merge Orchestrator" \
  --field dry_run=false \
  --field max_prs=8 \
  --field pr_label_filter="coverage,testing,ai-task"
```

**Your Task's Role in Consolidation:**
- Individual excellence enables effective orchestrator consolidation
- Clear PR structure facilitates automatic conflict resolution
- Framework enhancements become coordinated epic-wide improvements
- Quality standards ensure consolidated changes maintain project excellence

### Epic Contribution
Each task contributes to:
- **Coverage Velocity:** Sustained monthly increase toward comprehensive coverage goals
- **Framework Scalability:** Enhanced testing infrastructure
- **Quality Maintenance:** ≥99% test pass rate throughout progression
- **Excellence Goal:** World-class backend code coverage achievement
- **Orchestrator Efficiency:** Reduced coordination overhead through systematic consolidation

---

**Execution Model:** Autonomous AI Agent (GitHub Actions CI)
**Coverage Timeline:** Ongoing continuous excellence
**Success Definition:** Measurable coverage improvement with enhanced framework scalability
