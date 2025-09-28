---
name: AI Coder Task (General)
about: Epic parent issue for LanguageModelService v2.
title: 'feat: Epic 246 — LanguageModelService v2 (OpenAI SDK + Venice REST)'
labels: 'status: epic-active, type: feature, priority: high, effort: epic, component: api, component: docs, component: architecture, tech: dotnet, epic: language-model-service-v2'
assignees: ''

---

## 1. Background and Vision

We are delivering a complete replacement for the current `LlmService` with a vendor‑agnostic `LanguageModelService` v2. This epic establishes robust abstractions, routing, resilience, and prompt modernization. Epic 246 intentionally scopes to two providers:

- OpenAI via official .NET SDK (v1.5) using Chat Completions/Responses
- Venice.AI via an OpenAI‑compatible REST route

Backward compatibility (adapting `ILlmService` to v2) is transitional only. By the end of this epic, all legacy paths are removed with a single DI cutover. No feature flags are introduced in this epic.

Why: current `LlmService` is tightly coupled to OpenAI and affected by Assistants API changes and GPT‑5 defaults; we need a future‑proof, provider‑agnostic foundation.

## 2. Specs Directory — Authoritative Technical Context

All implementation work MUST align with the Specs documents. Treat them as contract and shared context for every subtask:

- Overview: `Docs/Specs/epic-246-language-model-service/README.md`
- Architecture: `01-architecture-design.md`
- Provider Integration (OpenAI SDK + Venice REST): `02-provider-integration.md`
- Migration Strategy (Assistants → Chat/Responses, SDK v1.5): `03-migration-strategy.md`
- Testing Strategy: `04-testing-strategy.md`
- Configuration Management: `05-configuration-management.md`
- Prompt Modernization: `06-prompt-system-modernization.md`
- Implementation Roadmap: `07-implementation-roadmap.md`
- Phase 1 Criteria: `phase-1-completion-criteria.md`
- Component specs: `components/*`

Research reference: `Docs/Research/AI_Provider_API_Standardization.md`

All sub‑issues are intentionally lightweight and reference this epic for context and cross‑constraints. Always review this epic issue + specs before starting a task.

## 3. Scope and Non‑Goals

In Scope (Epic 246):
- New `ILanguageModelService` v2, unified DTOs, normalizer
- Two providers: OpenAI (SDK) and Venice (REST)
- Provider router (capabilities + priority), health monitor + circuit breaker, resilience (retry/backoff)
- Metrics + health checks
- PromptBaseV2 + schema generation; migrate cookbook prompts
- Transitional `LegacyLlmServiceAdapter`, then DI cutover and legacy removal

Out of Scope (later epics):
- Additional providers (Anthropic, Gemini, xAI, DeepSeek)
- Full streaming implementation across providers
- Feature management framework / flags

## 4. Key Technical Decisions and Constraints

- Two adapter bases: `SdkProviderAdapterBase` (OpenAI) and `RestProviderAdapterBase` (Venice)
- Unified message roles: `system`, `user`, `assistant`, `tool` (OpenAI maps `tool` ⇄ `function`)
- Config‑driven routing with provider priority and capability checks
- Performance controls: service concurrency, timeouts; optional per‑provider concurrency
- Transitional compatibility only; end state deletes legacy

Assistants → Responses Migration:
- Official guide: https://platform.openai.com/docs/assistants/migration
- Replace Assistants primitives with Chat Completions/Responses + tools (function calling)
- During migration, pin a GPT‑4 family model (e.g., `gpt-4o-mini`) for smoke tests; restore default after removal of Assistants paths

## 5. Milestones and Sub‑Issues

Foundation
- #248 v2 core interfaces and models
- #249 message normalization and role mapping
- #250 adapter bases and exception hierarchy
- #251 configuration and DI scaffolding
- #252 CI test harness updates

OpenAI Modernization
- #253 upgrade OpenAI .NET SDK to 1.5
- #254 OpenAIProviderAdapter (SDK) with tool calling and responses
- #255 migrate Assistants API usage to Chat Completions/Responses
- #256 OpenAI tests and resilience

Venice REST
- #257 generic OpenAI‑compatible REST client
- #258 VeniceAIProviderAdapter (REST)
- #259 Venice routing preferences and cost toggles

Routing, Health, Resilience, Metrics
- #260 ProviderRouter (capability + priority)
- #261 provider health monitor + circuit breaker
- #262 retry/backoff and rate‑limit handling
- #263 metrics and health endpoints

Prompt Modernization and Migration
- #264 PromptBaseV2 and JsonSchemaGenerator
- #265 migrate core prompts (AnalyzeRecipe, SynthesizeRecipe, ProcessOrder)
- #266 migrate remaining prompts batch 2 (AlternativeQuery, Rank, Choose, Clean, Namer)
- #267 prompt service and cookbook wiring
- #268 prompt performance validation

Cutover and Cleanup
- #269 LegacyLlmServiceAdapter (transitional)
- #270 migrate tests and call sites to v2
- #271 DI cutover (single switch)
- #272 remove legacy implementation

Docs and Ops
- #273 provider maintenance docs (OpenAI, Venice)
- #274 ExternalServices.AIServices degradation pattern update
- #275 configuration validation and security hardening

## 6. Acceptance Criteria (Epic‑Level)

- Complete replacement: no production references to legacy `ILlmService`
- Two providers implemented and passing contract/integration tests
- Router, health, and resilience validated; metrics/health endpoints available
- Prompt system migrated; cookbook flows run via v2
- Performance acceptable (OpenAI improved vs. Assistants; routing overhead minimal)
- Maintenance docs present

## 7. Labeling and Workflow Guidance (for Sub‑tasks)

- Apply labels per standards: exactly one `type:`, one `priority:`, one `effort:`, ≥1 `component:`; include `epic: language-model-service-v2`
- Status progression: `status: ready` → `status: in-progress` → `status: review` → `status: done`
- Test profile: unit tests always; integration tests gated by secrets (`OPENAI_API_KEY`, `VENICE_API_KEY`)
- Each subtask must consider outputs as inputs for the next task (e.g., DTOs → adapters → router → prompts)

## 8. Developer Checklist (Each Sub‑task)

- Review this epic issue and relevant Specs docs before coding
- Confirm inputs/outputs meet the needs of downstream tasks
- Follow config and DI patterns; avoid provider‑specific leakage into interfaces
- Provide or update tests per Testing Strategy (unit first, then integration/contract/perf as applicable)
- Update docs or cross‑references when behavior or configuration changes

## 9. Links

- Epic Specs: `Docs/Specs/epic-246-language-model-service/README.md`
- Migration strategy: `Docs/Specs/epic-246-language-model-service/03-migration-strategy.md`
- Implementation roadmap: `Docs/Specs/epic-246-language-model-service/07-implementation-roadmap.md`
- Testing: `Docs/Specs/epic-246-language-model-service/04-testing-strategy.md`
- Configuration: `Docs/Specs/epic-246-language-model-service/05-configuration-management.md`

---
*Epic parent issue providing shared context for all subtasks.*

