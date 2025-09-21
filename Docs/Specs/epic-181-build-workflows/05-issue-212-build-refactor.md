# Issue #212: Refactor build.yml to Reusable Actions

**Last Updated:** 2025-09-21
**Status:** Planning
**Owner:** WorkflowEngineer

> Parent: `Epic #181: Build Workflows Enhancement`

## 1. Purpose & Scope

- **Purpose:** Refactor the monolithic `.github/workflows/build.yml` to consume extracted reusable composite actions from Issue #183, establishing the canonical pattern for future workflow files.
- **Why:** Eliminate duplicated inline logic, improve maintainability, and create a standard reference implementation for future build-customized workflows (e.g., coverage pipelines in #184).
- **Note:** This issue does NOT create `coverage-build.yml`; that will be implemented in Issue #184 using this refactored pattern.

## 2. Dependencies

- **Depends on:** Issue #183 (extraction of reusable components: `path-analysis`, `backend-build`, `frontend-build`, and shared actions)
- **Blocks:** Issue #184 (create `coverage-build.yml` using the refactored pattern)

## 3. Deliverables

### 3.1 Refactored build.yml
- Consume reusable composite actions:
  - `/.github/actions/shared/path-analysis`
  - `/.github/actions/shared/backend-build`
  - `/.github/actions/shared/frontend-build` (if applicable)
  - Continue using `/.github/actions/handle-ai-analysis-failure` for AI jobs
- Preserve all existing behavior: triggers, permissions, concurrency, branch-aware conditions
- Maintain zero-warning policy and baseline validation
- Eliminate duplicated inline logic

### 3.2 Documentation Updates
- Workflow comments documenting interfaces for consumed composites
- Updated epic documentation to reflect canonical pattern establishment

## 4. Acceptance Criteria

- **Parity Preserved:** Refactored `build.yml` maintains existing behavior, triggers, permissions, concurrency, and branch-aware conditions (including zero-warning policy and coverage flexibility logic)
- **Composability:** `build.yml` jobs call the new composite actions; no duplicated logic remains in the workflow
- **Canonical Pattern:** Becomes the standard reference implementation for future workflows
- **Interface Documentation:** Inputs/outputs and usage notes for each consumed composite are referenced in workflow comments or linked docs
- **Functional Validation:** Job structure, artifacts, and summaries match prior behavior on PRs to `develop` and `main`
- **Epic Tracking:** Dependency order confirmed as `#183 → #212 → #184`

## 5. Non-Goals

- **Coverage Workflow Creation:** `coverage-build.yml` creation is explicitly out of scope (deferred to Issue #184)
- **Concurrency Extraction:** Workflow-level `concurrency` block remains defined in each workflow (cannot be extracted into composite action)
- **AI Sentinel Changes:** Detailed AI composite extractions occur in Issue #184; this issue keeps existing AI jobs functional

## 6. Work Plan

1. **Update build.yml Structure:**
   - Replace path analysis logic with `path-analysis` composite consumption
   - Replace backend build steps with `backend-build` composite consumption
   - Wire in `frontend-build` composite if frontend components exist
   - Preserve workflow-level policies (triggers, permissions, concurrency)

2. **Validation & Testing:**
   - Validate functional parity via sample PRs to `develop` and `main`
   - Verify job outcomes and artifacts match baseline behavior
   - Confirm zero-warning policy enforcement

3. **Documentation Updates:**
   - Update workflow comments with composite interface documentation
   - Update Epic documentation to reflect canonical pattern

## 7. Technical Constraints

- **Workflow-Level Constructs:** Concurrency, triggers, and permissions remain at workflow level
- **Behavioral Equivalence:** Must maintain exact functional parity with current build.yml
- **AI Job Preservation:** Existing AI analysis jobs continue functioning unchanged

## 8. Risks & Mitigation

- **Risk:** Behavioral drift during refactor
  - **Mitigation:** Stepwise replacement; verify job outcomes and artifacts against baseline runs
- **Risk:** Concurrency misconceptions
  - **Mitigation:** Document that concurrency is workflow-level construct; standardize shared snippet
- **Risk:** Integration issues with extracted composites
  - **Mitigation:** Comprehensive testing with sample PRs to all target branches

## 9. Success Metrics

- **Functional Equivalence:** 100% behavioral parity with pre-refactor build.yml
- **Code Reduction:** Elimination of duplicated inline logic in workflow file
- **Pattern Establishment:** Clear canonical reference for Issue #184 implementation
- **Integration Readiness:** Clean foundation for subsequent epic workflow enhancements

---