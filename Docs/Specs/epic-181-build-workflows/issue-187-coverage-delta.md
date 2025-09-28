# Issue #187 — Coverage Delta Analysis

Last Updated: 2025-09-28
Status: Pending (Phase 1 Critical)
Parent: Docs/Specs/epic-181-build-workflows/README.md

## Purpose
Establish a reliable, machine-readable coverage delta signal for PRs and dispatch runs by comparing current coverage to a documented baseline, generating a `coverage_delta.json` artifact, and wiring this structured data into AI analysis and PR reporting. This enables precise progression tracking and autonomous decision-making for the coverage epic.

## Scope
- In scope:
  - Baseline vs current coverage comparison (operational, not just threshold)
  - JSON artifact generation: `TestResults/coverage_delta.json`
  - PR summary context (baseline/current/delta/trend)
  - AI framework integration: provide `COVERAGE_DATA`, `COVERAGE_DELTA`, `COVERAGE_TRENDS` to templates
- Out of scope:
  - Auto‑ratcheting baseline management (Issue #234)
  - Merge orchestration and multi‑PR consolidation (Issue #220)

## Deliverables
- Machine‑readable artifact written during coverage build: `TestResults/coverage_delta.json`
- Artifact uploaded with other coverage/test results
- PR summary includes baseline/current/delta/trend
- AI analyzer(s) receive coverage context variables populated from artifacts

## JSON Schema & Example
- Schema: `Docs/Templates/schemas/coverage_delta.schema.json`

Example (`coverage_delta.json`):
```json
{
  "current_coverage": 27.5,
  "baseline_coverage": 25.9,
  "coverage_delta": 1.6,
  "coverage_trend": "improved",
  "base_ref": "develop",
  "base_sha": "abc1234...",
  "run_number": 1234,
  "timestamp": "2025-09-28T08:30:00Z",
  "baseline_source": "baseline_file",
  "baseline_unavailable": false,
  "notes": "Baseline read from repo file on base branch"
}
```

## Baseline Sourcing Plan
- Phase 1 (Minimal, Reliable):
  - Read a baseline coverage value from a file on the base branch or from a last recorded baseline artifact when available.
  - When unavailable, fall back to a configured threshold or explicit input and set `baseline_unavailable=true`.
- Phase 2 (Enhanced):
  - Optionally run a base‑branch coverage measurement for precise comparison.

## AI Integration
- Template variables used by Coverage Auditor / Iterative Review:
  - `{{COVERAGE_DATA}}`: current overall coverage percentage and key metrics
  - `{{COVERAGE_DELTA}}`: baseline, current, delta, trend (JSON object)
  - `{{COVERAGE_TRENDS}}`: structured trend/velocity snapshot (e.g., from `health_trends.json`)
- Source mapping:
  - `COVERAGE_DATA` <- `TestResults/coverage_results.json`
  - `COVERAGE_DELTA` <- `TestResults/coverage_delta.json`
  - `COVERAGE_TRENDS` <- `TestResults/health_trends.json` (if available)

## Acceptance Criteria
- Baseline vs delta comparison is operational (not threshold‑only):
  - Produces `TestResults/coverage_delta.json` conforming to schema v1.0
  - `coverage_trend` ∈ {improved, stable, decreased}
  - Includes metadata: `base_ref`, `run_number`, `timestamp`, and `baseline_source` when known
- Artifact is uploaded with coverage reports
- PR summary displays baseline/current/delta with trend icon
- AI analysis jobs can ingest the context variables (no missing placeholders in processed templates)

## Implementation Notes (Guidance)
- Add JSON write step right after delta computation in `testing-coverage-build-review.yml` and include in `upload-artifact` list.
- Prefer reading baseline from a base‑branch file or artifact; fall back to configured value when unavailable with explicit flagging in JSON.
- Keep failure modes non‑disruptive: if tests fail or coverage insufficient, still emit coverage_delta with `coverage_trend` based on available values.

## Risks & Fallbacks
- Baseline unavailable: use fallback with `baseline_unavailable=true` and avoid blocking builds.
- Multi-source ambiguity: record `baseline_source` to aid debugging and auditability.

## References
- Acceptance gates: `Docs/Specs/epic-181-build-workflows/phase-1-completion-criteria.md`
- Workflow: `.github/workflows/testing-coverage-build-review.yml`
- Test/coverage parsing: `Scripts/run-test-suite.sh`

