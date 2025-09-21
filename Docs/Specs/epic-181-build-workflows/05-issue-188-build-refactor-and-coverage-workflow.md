# Issue #188: Refactor build.yml to Reusable Actions and Deliver coverage-build.yml

**Last Updated:** 2025-09-21
**Status:** Planning
**Owner:** WorkflowEngineer

> Parent: `Epic #181: Build Workflows Enhancement`

## 1. Purpose & Scope

- Purpose: Refactor the monolithic `.github/workflows/build.yml` to consume extracted reusable composite actions from Issue #183, then create a new `.github/workflows/coverage-build.yml` built entirely from those reusable components with coverage-focused variations.
- Why: Ensure both the primary build workflow and the new coverage workflow share identical, modular foundations to maximize consistency, maintainability, and reuse.

## 2. Dependencies

- Depends on: Issue #183 (foundation components extracted: `path-analysis`, `backend-build`, optional `frontend-build`, and standard shared actions)
- Blocks: Issue #184 (iterative AI review framework extraction and integration)

## 3. Deliverables

- Refactored `.github/workflows/build.yml` consuming reusable components:
  - `/.github/actions/shared/path-analysis`
  - `/.github/actions/shared/backend-build`
  - `/.github/actions/shared/frontend-build` (if applicable to current repo)
  - Continue using `/.github/actions/handle-ai-analysis-failure` for AI jobs
- New `.github/workflows/coverage-build.yml` using the same components with coverage-centric settings:
  - Path-aware execution (based on `path-analysis` outputs)
  - Backend build with coverage flexibility flags
  - Uses `validate-test-suite` and `run-tests` shared actions for results
  - No AI sentinel analysis in this workflow unless explicitly enabled later
- Documentation updates:
  - Epic README, Component Analysis, Architectural Assessment, and Implementation Roadmap updated to reflect new dependency ordering `#183 → #188 → #184` and coverage-build placement after refactor.

## 4. Acceptance Criteria

- Parity: Refactored `build.yml` preserves existing behavior, triggers, permissions, concurrency, and branch-aware conditions (including zero-warning policy and coverage flexibility logic).
- Composability: `build.yml` jobs call the new composite actions for path analysis and builds; no duplicated logic remains in the workflow.
- Coverage Workflow: `coverage-build.yml` exists and runs end-to-end using the same composites, collecting and uploading coverage/test artifacts.
- Quality Gates: Zero-warning enforcement and baseline validation are maintained where applicable.
- Validation: On PRs to `develop` and `main`, job structure, artifacts, and summaries match prior behavior for build.yml; coverage-build.yml runs correctly when targeted.

## 5. Non-Goals / Notes

- Concurrency: The standard workflow-level `concurrency` block remains defined in each workflow (cannot be extracted into a composite action).
- AI Sentinel Extraction: Detailed AI composite extractions (`ai-sentinel-base`, etc.) occur in Issue #184; this issue keeps existing AI jobs functional while moving build/path logic to reusables.

## 6. Work Plan

1. Update build.yml to consume `path-analysis` and `backend-build` composites; keep triggers, permissions, and concurrency unchanged.
2. Wire in `frontend-build` composite if frontend components exist.
3. Create `coverage-build.yml` based on composites; ensure path-aware execution and coverage artifacts.
4. Validate functional parity of `build.yml` via sample PRs; verify coverage workflow behavior.
5. Update Epic documentation to reflect new dependency ordering and scope.

## 7. Risks & Mitigation

- Risk: Behavioral drift during refactor. Mitigation: Stepwise replacement; verify job outcomes and artifacts against baseline runs.
- Risk: Concurrency misconceptions. Mitigation: Document that concurrency is a workflow-level construct and standardize a shared snippet.

---

