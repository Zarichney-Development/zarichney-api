# Coverage Trends Reporting

This directory documents how long‑term coverage growth is tracked and reported.

## Phase 1 (Artifact‑First)
- Keep trend data as workflow artifacts only (no repo commits on PRs):
  - `TestResults/health_trends.json` — time series (last N points) for coverage, skip rate, pass rate, execution time
  - `TestResults/coverage_results.json` — current coverage snapshot
  - `TestResults/coverage_delta.json` — baseline vs current delta (Issue #187)
- Surface summaries in PR comments; download artifacts in AI analysis jobs as needed.

## Phase 2 (Repo‑Committed History)
- On merges to main/develop, append/update a repo‑committed history file for transparency and easy diffs, e.g.:
  - `Docs/Reports/CoverageTrends/coverage-history.json`
  - Optional monthly rollups: `Docs/Reports/CoverageTrends/2025-09.md`

## Notes
- Prefer artifact‑only in Phase 1 to reduce churn during implementation.
- When promoting to repo‑committed history, ensure updates occur only on trusted branches (e.g., main) via CI.

