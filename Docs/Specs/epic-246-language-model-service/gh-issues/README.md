# Epic 246 — GitHub Issues Index

This folder contains the epic parent issue and all sub‑issues for Epic 246 (LanguageModelService v2). Review the epic issue first for shared context, scope, and links to the Specs, then open the specific subtask.

- Epic issue: [#246 — LanguageModelService v2 (OpenAI SDK + Venice REST)](issue-246-epic-language-model-service-v2.md)

## Milestone Index

### Foundation
- #248 — [v2 core interfaces and models](issue-248-v2-core-interfaces-and-models.md)
- #249 — [message normalization and role mapping](issue-249-message-normalization-and-role-mapping.md)
- #250 — [adapter bases and exception hierarchy](issue-250-adapter-bases-and-exception-hierarchy.md)
- #251 — [configuration scaffolding and DI registration](issue-251-configuration-and-di-scaffolding.md)
- #252 — [CI test harness updates](issue-252-ci-test-harness-updates.md)

### OpenAI Modernization
- #253 — [upgrade OpenAI .NET SDK to v1.5](issue-253-upgrade-openai-dotnet-sdk-to-1-5.md)
- #254 — [OpenAIProviderAdapter (SDK) with tool calling and responses](issue-254-openai-provider-adapter-sdk.md)
- #255 — [migrate Assistants API to Chat Completions/Responses](issue-255-migrate-assistants-to-chat-completions-responses.md)
- #256 — [OpenAI adapter tests and resilience](issue-256-openai-tests-and-resilience.md)

### Venice REST
- #257 — [generic OpenAI‑compatible REST client](issue-257-generic-openai-compatible-rest-client.md)
- #258 — [VeniceAIProviderAdapter (REST)](issue-258-venice-ai-provider-adapter-rest.md)
- #259 — [Venice routing preferences and cost toggles](issue-259-venice-routing-preferences-and-cost-toggles.md)

### Routing, Health, Resilience, Metrics
- #260 — [ProviderRouter (capability + priority)](issue-260-provider-router-capability-priority.md)
- #261 — [provider health monitor + circuit breaker](issue-261-provider-health-monitor-and-circuit-breaker.md)
- #262 — [retry/backoff and rate‑limit handling](issue-262-retry-backoff-and-rate-limit-handling.md)
- #263 — [metrics and health endpoints](issue-263-metrics-and-health-endpoints.md)

### Prompt Modernization and Migration
- #264 — [PromptBaseV2 and JsonSchemaGenerator](issue-264-promptbasev2-and-jsonschemagenerator.md)
- #265 — [migrate core prompts (Analyze, Synthesize, Process)](issue-265-migrate-core-prompts-analyze-synthesize-process.md)
- #266 — [migrate remaining prompts (AlternativeQuery, Rank, Choose, Clean, Namer)](issue-266-migrate-remaining-prompts-batch-2.md)
- #267 — [prompt service and cookbook wiring](issue-267-prompt-service-and-cookbook-wiring.md)
- #268 — [prompt performance validation](issue-268-prompt-performance-validation.md)

### Cutover and Cleanup
- #269 — [LegacyLlmServiceAdapter (transitional)](issue-269-legacy-llmservice-adapter-transitional.md)
- #270 — [migrate tests and call sites to v2](issue-270-migrate-tests-and-call-sites-to-v2.md)
- #271 — [DI cutover (single switch)](issue-271-di-cutover-single-switch.md)
- #272 — [remove legacy implementation](issue-272-remove-legacy-implementation.md)

### Docs and Ops
- #273 — [provider maintenance docs (OpenAI, Venice)](issue-273-provider-maintenance-docs-openai-venice.md)
- #274 — [ExternalServices.AIServices degradation pattern update](issue-274-externalservices-aiservices-degradation-pattern-update.md)
- #275 — [configuration validation and security hardening](issue-275-configuration-and-security-hardening.md)

## References
- Epic Specs: ../README.md
- Architecture: ../01-architecture-design.md
- Migration Strategy: ../03-migration-strategy.md
- Roadmap: ../07-implementation-roadmap.md
- Testing Strategy: ../04-testing-strategy.md
- Configuration: ../05-configuration-management.md

