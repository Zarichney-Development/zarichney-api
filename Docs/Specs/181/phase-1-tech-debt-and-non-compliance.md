# Epic #181 — Phase 1 Merge: Reported Tech Debt & Non‑Compliance Backlog (for Phase 2 Planning)

> We are merging Phase 1. This document compiles the reported tech debt and non‑compliances identified during Phase 1 reviews. Some items may be superseded or undone by Phase 2 implementation decisions; treat this as an input to Phase 2 planning and not a rigid mandate.

## 1. Overview & Intent

- Epic: Build Workflows Enhancement (Epic #181)
- Phase: Phase 1 complete; Phase 2 planning input
- Purpose: Provide a single, actionable backlog grouped by workstream with checklist items and traceability back to sources.

## 2. Scope & Sources

- Primary PR: PR #280 — epic/issue-181-standardize-build-workflows → main
- Consolidated review extraction: `working-dir/pr-280-review-mentions.md`
- Curated backlog baseline: `working-dir/pr-280-review-consolidated-backlog.md`
- Issue checklists (copy/paste ready): `working-dir/pr-280-issues-checklists.md`

## 3. Backlog by Workstream (Checklists)

### 3.1 Documentation & Workflow Docs

- [ ] `.github/actions/iterative-ai-review` — Add `README.md` (origin: Standards)
  - Include purpose, inputs/outputs, architecture diagrams, examples, and autonomous cycle integration
- [ ] `.github/workflows/README.md` — Add `testing-coverage-build-review.yml` to list (origin: Standards)
- [ ] `.github/workflows/README.md` — Update dependencies diagram with `testing-coverage-build-review.yml` (origin: Standards)
- [ ] `.github/workflows/README.md` — Update mermaid diagram to include `testing-coverage-build-review.yml` (origin: Standards)
- [ ] `.github/workflows/README.md` — Add interface contract for `testing-coverage-build-review.yml` (origin: Standards)
- [ ] `.github/workflows/README.md` — Add usage examples for `testing-coverage-build-review.yml` (origin: Standards)
- [ ] `.claude/agents/` — Add `README.md` describing agent system and flexible authority (origin: Standards)
- [ ] `.github/actions/shared/path-analysis/action.yml:160` — Document Git empty tree SHA magic hash (origin: Security)

### 3.2 Shell Actions Hardening

- [ ] `.github/actions/shared/ai-sentinel-base/src/template-processor.sh` — Centralize `sed` replacement escaping; ensure `\\`, `/`, `\"`, and `&` are handled (origin: Tech Debt)
- [ ] `.github/actions/iterative-ai-review/src/main.js:637-639` — Escape `&` in JSON template injections; reuse centralized escaping (origin: Tech Debt)
- [ ] `.github/actions/shared/ai-sentinel-base/src/template-processor.sh` — Replace fixed `/tmp` paths with `mktemp` and add cleanup `trap` (origin: Tech Debt)
- [ ] `.github/actions/shared/ai-standards-analysis/src/standards-intelligence-processor.sh:275-293` — Remove `$RANDOM` usage; implement deterministic scoring (origin: Tech Debt)

### 3.3 Cross‑Platform & Portability

- [ ] `.github/actions/shared/ai-testing-analysis/src/coverage-trends-analyzer.sh:302-303` — Add BSD/macOS fallback for GNU `date -d` (origin: Tech Debt/Testing)

### 3.4 Maintainability & Complexity

- [ ] `.github/actions/iterative-ai-review/src/main.js` — Decompose 100+ line functions to smaller helpers (origin: Tech Debt)
- [ ] `.github/actions/shared/ai-standards-analysis/src/epic-alignment-analyzer.sh:334-516` — Refactor duplicate assessment helpers using parameterization/tables (origin: Tech Debt)

### 3.5 Testing & Test Coverage

- [ ] `Code/Zarichney.Server.Tests/Framework/Helpers/AuditorPromptValidationHelper.cs` — Add comprehensive unit tests (origin: Tech Debt)
- [ ] `.github/actions/iterative-ai-review/tests/` — Add security injection attack tests (XSS, command injection) (origin: Testing)
- [ ] `.github/actions/iterative-ai-review/tests/` — Add performance/load tests for large PR comment and iteration scenarios (origin: Testing)

### 3.6 Concurrency & Config Consistency

- [ ] `.github/actions/shared/concurrency-config/action.yml:114-151` — Use numeric assignments and consistent numeric checks (origin: Tech Debt/Security)
- [ ] `.github/workflows/*.yml` — Centralize thresholds and timeouts in shared config/vars (origin: Tech Debt)

### 3.7 Infrastructure Naming & File Extensions

- [ ] `.github/actions/iterative-ai-review/src/*.js` — Rename bash modules to `.sh` and update all `source` references (origin: Tech Debt)

### 3.8 Auth & Secrets Practices

- [ ] `.github/workflows/testing-coverage-execution.yml:1164` — Prefer GitHub App auth over OAuth token (origin: Security)
- [ ] `.github/actions/shared/ai-sentinel-base/src/ai-service-client.sh:35-37` — Validate OpenAI API key by probing `/v1/models` (origin: Security)

### 3.9 API/Logging Hygiene

- [ ] `.github/actions/iterative-ai-review/src/github-api.js` — Sanitize error logs (avoid exposing internal GitHub API response details) (origin: Security)

### 3.10 Workflow Refactors (Size/Duplication)

- [ ] `.github/workflows/testing-coverage-*.yml` — Extract shared steps into composite actions to reduce duplication (origin: Tech Debt)

### 3.11 Additional Items To Triage (from CI Logs)

- [ ] +5 additional Do Now items listed by Merge Review — review job logs; assign to groups above
- [ ] +17 additional Do Later items listed by Merge Review — review job logs; assign to groups above

## 4. Batching & Dependencies (Suggested Order)

1) Documentation & Workflow Docs (low‑risk clarity)
2) Shell Actions Hardening (centralized escaping + mktemp)
3) Cross‑Platform & Portability (date fallback)
4) Maintainability & Complexity (modularize main.js; parameterize epic analyzer)
5) Testing & Test Coverage (unit + security + performance)
6) Concurrency & Config Consistency (numeric assignments; central config)
7) Infrastructure Naming & Extensions (rename `.js` bash files → `.sh`)
8) Auth & Secrets Practices (GitHub App; API key validation)
9) API/Logging Hygiene (error sanitization)
10) Workflow Refactors (extract composite actions)

## 5. Notes on Phase 2 Impact

- Several Phase 1 hardening/refactors may be superseded by Phase 2 scope (e.g., consolidation into new composite actions, broader standardization). Use this backlog as a starting point and prune items that Phase 2 design renders obsolete.
- Items with cross‑cutting value regardless of Phase 2 direction: shell escaping centralization, `mktemp` usage, portability fallbacks, and documentation completeness.

