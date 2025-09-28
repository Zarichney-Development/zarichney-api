# Epic #246: LanguageModelService v2 (OpenAI SDK + Venice REST)

**Last Updated:** 2025-01-25
**Epic Status:** Planning
**Epic Owner:** BackendSpecialist

> **Parent:** [`Specs`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Comprehensive architecture transformation creating a vendor-agnostic Language Model Service abstraction. Epic 246 delivers two providers: OpenAI via official SDK patterns and Venice.AI via a standardized OpenAI-compatible REST route, establishing standards for future providers. Backward compatibility is used only during development and fully removed before the epic closes.

* **Key Objectives:**
  - Implement vendor-agnostic abstraction with two providers: OpenAI (SDK) and Venice.AI (REST)
  - Migrate OpenAI to SDK v1.5 and transition from Assistants API to Chat Completions/Responses API
  - Establish unified interface contracts and routing patterns that scale to additional providers post‑epic
  - Use backward compatibility only during development; fully remove legacy by epic completion

* **Success Criteria:**
  - Existing Cookbook services and AI controller functionality preserved during transition and fully migrated to v2
  - Two providers implemented and production-ready: OpenAI (SDK) and Venice.AI (REST)
  - Performance improvement of 25%+ for OpenAI via optimized Chat Completions/Responses
  - >90% test coverage for service, router, and both provider adapters
  - Legacy ILlmService and any backward-compatibility adapters are removed before closing the epic
  - No new feature flag mechanisms introduced by this epic; a single cutover completes the replacement

* **Why it exists:** Current LlmService creates vendor lock-in with OpenAI and relied on APIs that changed. This architecture modernizes to current OpenAI APIs and introduces a clean, standards-based REST path via Venice.AI, establishing the foundation to scale to additional providers later while delivering a complete v2 replacement now.

> Epic 246 Scope
> - In-scope providers: OpenAI (SDK) and Venice.AI (REST). All other providers are future work.
> - Backward compatibility is transitional only and fully removed at epic completion.
> - No feature flags are introduced by this epic; cutover happens as a single, validated switch.

### Component Specifications
* **Core Architecture:** [`01-architecture-design`](./01-architecture-design.md) - Primary service interfaces and component relationships
* **Provider Integration:** [`02-provider-integration`](./02-provider-integration.md) - OpenAI (SDK) and Venice (REST) adapter patterns and standards for future providers
* **Migration Strategy:** [`03-migration-strategy`](./03-migration-strategy.md) - Phased migration with transitional compatibility and final cleanup/cutover
* **Testing Strategy:** [`04-testing-strategy`](./04-testing-strategy.md) - Comprehensive testing framework for provider adapters
* **Configuration Management:** [`05-configuration-management`](./05-configuration-management.md) - Configuration and routing for OpenAI/Venice, with patterns for future providers
* **Prompt System Modernization:** [`06-prompt-system-modernization`](./06-prompt-system-modernization.md) - Cookbook prompt migration to provider-agnostic architecture
* **Implementation Roadmap:** [`07-implementation-roadmap`](./07-implementation-roadmap.md) - Phased plan culminating in full cutover and legacy removal
* **Phase 1 Completion Criteria:** [`phase-1-completion-criteria`](./phase-1-completion-criteria.md) - Foundation phase acceptance criteria and validation requirements
* **Service Interfaces:** [`components/language-model-service`](./components/language-model-service.md) - ILanguageModelService core interface
* **Provider Adapters:** [`components/provider-adapters`](./components/provider-adapters.md) - Provider-specific adapter implementations
* **Configuration Router:** [`components/configuration-router`](./components/configuration-router.md) - Routing and provider selection logic
* **Prompt Base v2:** [`components/prompt-base-v2`](./components/prompt-base-v2.md) - Provider-agnostic prompt architecture with automated schema generation
* **Cookbook Migration:** [`components/cookbook-prompt-migration`](./components/cookbook-prompt-migration.md) - Individual prompt migration specifications and testing requirements

## 2. Architecture & Key Concepts

* **High-Level Design:** Multi-layered architecture with client abstraction layer, provider routing layer, adapter abstraction layer, and individual provider implementations. The design isolates provider-specific logic behind unified interfaces while maintaining session management integration and enabling configuration-driven provider selection.

* **Core Implementation Flow:**
  1. Client requests via ILanguageModelService interface
  2. Provider router evaluates capabilities and configuration
  3. Selected provider adapter normalizes request format
  4. Provider-specific service executes API call
  5. Response normalizer converts to unified format
  6. Session manager persists conversation state
  7. Unified response returned to client

* **Key Architectural Decisions:**
  - **Adapter Pattern Implementation**: Isolates provider differences behind IProviderAdapter interface enabling uniform integration
  - **Unified Message Protocol**: Common message format with provider-specific adapters handling translation and role mapping
  - **Configuration-Driven Routing**: Runtime provider selection based on capabilities, health status, and configuration preferences
  - **Backward Compatibility Layer**: Legacy ILlmService interface preserved through adapter pattern during migration
  - **Template Method Pattern**: RestProviderAdapterBase provides common HTTP client functionality for consistent provider implementation

* **Integration Points:**
  - Session management via existing ISessionManager for conversation persistence
  - Dependency injection integration with current DI container patterns
  - Cookbook services maintain existing interfaces during transition
  - AI Controller preserves current endpoint contracts
  - Health check system integration for provider monitoring

* **Architecture Diagram:**
  ```mermaid
  graph TB
      subgraph "Client Layer"
          CLIENT[Application Code]
          COOKBOOK[Cookbook Services]
          CONTROLLER[AI Controller]
          LEGACY[Legacy ILlmService]
      end

      subgraph "Service Abstraction Layer"
          ILS[ILanguageModelService]
          COMPAT[Compatibility Adapter]
          ROUTER[Provider Router]
          CONFIG[Configuration Manager]
      end

      subgraph "Provider Adapters"
          OPENAI_ADAPTER[OpenAI Adapter]
          ANTHROPIC_ADAPTER[Anthropic Adapter]
          VENICE_ADAPTER[Venice.AI Adapter]
          GEMINI_ADAPTER[Google Gemini Adapter]
          XAI_ADAPTER[xAI Adapter]
          DEEPSEEK_ADAPTER[DeepSeek Adapter]
      end

      subgraph "Provider Services"
          OPENAI_SVC[OpenAI Service]
          ANTHROPIC_SVC[Anthropic Service]
          VENICE_SVC[Venice.AI Service]
          GEMINI_SVC[Gemini Service]
          XAI_SVC[xAI Service]
          DEEPSEEK_SVC[DeepSeek Service]
      end

      subgraph "Cross-Cutting"
          NORMALIZER[Message Normalizer]
          HEALTH[Health Monitor]
          RETRY[Retry Policy]
          SESSION[Session Manager]
      end

      subgraph "External APIs"
          OPENAI_API[OpenAI API v1.5]
          ANTHROPIC_API[Anthropic API]
          VENICE_API[Venice.AI API]
          GEMINI_API[Google Gemini API]
          XAI_API[xAI API]
          DEEPSEEK_API[DeepSeek API]
      end

      CLIENT --> ILS
      COOKBOOK --> ILS
      CONTROLLER --> ILS
      LEGACY --> COMPAT
      COMPAT --> ILS

      ILS --> ROUTER
      ROUTER --> CONFIG
      ROUTER --> HEALTH

      ROUTER --> OPENAI_ADAPTER
      ROUTER --> ANTHROPIC_ADAPTER
      ROUTER --> VENICE_ADAPTER
      ROUTER --> GEMINI_ADAPTER
      ROUTER --> XAI_ADAPTER
      ROUTER --> DEEPSEEK_ADAPTER

      OPENAI_ADAPTER --> OPENAI_SVC
      ANTHROPIC_ADAPTER --> ANTHROPIC_SVC
      VENICE_ADAPTER --> VENICE_SVC
      GEMINI_ADAPTER --> GEMINI_SVC
      XAI_ADAPTER --> XAI_SVC
      DEEPSEEK_ADAPTER --> DEEPSEEK_SVC

      OPENAI_SVC --> NORMALIZER
      ANTHROPIC_SVC --> NORMALIZER
      VENICE_SVC --> NORMALIZER

      OPENAI_SVC --> RETRY
      ANTHROPIC_SVC --> RETRY
      VENICE_SVC --> RETRY
      GEMINI_SVC --> RETRY
      XAI_SVC --> RETRY
      DEEPSEEK_SVC --> RETRY

      ILS --> SESSION

      OPENAI_SVC --> OPENAI_API
      ANTHROPIC_SVC --> ANTHROPIC_API
      VENICE_SVC --> VENICE_API
      GEMINI_SVC --> GEMINI_API
      XAI_SVC --> XAI_API
      DEEPSEEK_SVC --> DEEPSEEK_API
  ```

## 3. Interface Contract & Assumptions

* **Key Epic Deliverables:**
  - **Core Service Interface:**
    * **Purpose:** Vendor-agnostic ILanguageModelService providing unified completion, streaming, and tool calling
    * **Dependencies:** Provider adapters, configuration system, session management integration
    * **Outputs:** LlmResult<T> responses maintaining backward compatibility with existing return types
    * **Quality Gates:** 100% backward compatibility, >90% test coverage, zero breaking changes

  - **Multi-Provider Adapters:**
    * **Purpose:** Six provider implementations (OpenAI, Anthropic, Venice.AI, Gemini, xAI, DeepSeek) with unified interfaces
    * **Dependencies:** Provider-specific SDKs, configuration validation, message normalization
    * **Outputs:** Normalized responses in unified format with provider metadata
    * **Quality Gates:** Contract compliance testing, provider-specific integration tests, capability validation

  - **Configuration & Routing System:**
    * **Purpose:** Intelligent provider selection with capability matching and health-based failover
    * **Dependencies:** Provider health checks, capability discovery, configuration validation
    * **Outputs:** Optimal provider selection with fallback mechanisms
    * **Quality Gates:** Routing logic testing, failover validation, performance benchmarking

  - **Backward Compatibility Layer:**
    * **Purpose:** Seamless transition preserving all existing ILlmService functionality
    * **Dependencies:** Legacy interface contracts, session management patterns
    * **Outputs:** Adapter implementing ILlmService using new architecture
    * **Quality Gates:** Existing test suite passes, functional equivalence validation

* **Critical Assumptions:**
  - **Technical Assumptions:** OpenAI SDK v1.5 provides stable Chat Completions API, provider APIs maintain contract stability, existing HttpClient patterns support multi-provider usage
  - **Resource Assumptions:** Development bandwidth for 6-8 weeks implementation, staging environment for multi-provider testing, provider API access for development and testing
  - **External Dependencies:** Provider API availability during development, stable provider authentication mechanisms, consistent tool calling standards across providers

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Epic-Specific Standards:**
  - Provider adapter naming convention: `{Provider}ProviderAdapter` (e.g., `OpenAIProviderAdapter`)
  - Unified message format with `UnifiedChatMessage` base type for all provider communications
  - Configuration pattern using strongly-typed classes inheriting from `IProviderConfig`
  - Error handling with provider-specific exception hierarchy under `ProviderException` base
  - Async/await patterns throughout with CancellationToken support for all external calls

* **Technology Constraints:**
  - .NET 8 and C# 12 language features required for record types and primary constructors
  - HttpClient factory pattern mandatory for all provider implementations
  - System.Text.Json for serialization to maintain existing project patterns
  - IOptions pattern for configuration binding following project standards
  - Serilog structured logging for all provider interactions with correlation IDs

* **Timeline Constraints:**
  - Phase 1 (Weeks 1-2): Core interfaces and OpenAI adapter implementation
  - Phase 2 (Weeks 3-4): Additional provider adapters and routing logic
  - Phase 3 (Weeks 5-6): Migration and backward compatibility validation
  - Phase 4 (Weeks 7-8): Testing completion and production deployment
  - Hard dependency: OpenAI SDK v1.5 migration must complete before additional providers

## 5. How to Work With This Epic

* **Implementation Approach:**
  - Begin with interface-first design establishing all contracts before provider implementation
  - Implement OpenAI adapter first by wrapping existing LlmService functionality
  - Add providers incrementally with comprehensive testing before moving to next provider
  - Maintain backward compatibility throughout with existing tests passing at each phase
  - Use feature flags for gradual rollout of provider capabilities

* **Quality Assurance:**
  - **Testing Strategy:** Unit tests for all interfaces, integration tests per provider, contract tests for API compliance, end-to-end tests for complete flows
  - **Validation Approach:** Existing functionality preserved through automated regression testing, new provider capabilities validated through comprehensive test matrix
  - **Performance Validation:** Benchmark OpenAI SDK v1.5 performance improvements, validate provider response times, monitor resource usage patterns

* **Common Implementation Pitfalls:**
  - Provider API differences in message role handling (OpenAI "function" vs Claude "tool" roles)
  - Tool calling schema variations requiring careful normalization logic
  - Authentication patterns vary significantly across providers requiring flexible configuration
  - Rate limiting and error handling patterns differ substantially between providers
  - Session management integration complexity with multiple provider conversation states

## 6. Dependencies

* **Internal Code Dependencies:**
  - [`LlmService`](../../Code/Zarichney.Server/Services/AI/README.md) - Current implementation providing foundation for OpenAI adapter
  - [`ISessionManager`](../../Code/Zarichney.Server/Services/Sessions/README.md) - Conversation persistence and session management
  - [`AI Controllers`](../../Code/Zarichney.Server/Controllers/README.md) - HTTP endpoints requiring interface preservation
  - [`Cookbook Services`](../../Code/Zarichney.Server/Services/Cookbook/README.md) - AI-powered services requiring backward compatibility

* **External Dependencies:**
  - OpenAI SDK v1.5 - Chat Completions API and new response handling patterns
  - Anthropic SDK - Claude API integration for advanced reasoning capabilities
  - Venice.AI API - Direct HTTP client integration for Venice.AI provider
  - Google Gemini API - Google AI integration with API key authentication
  - xAI API - Grok model integration for diverse AI capabilities
  - DeepSeek API - Cost-effective AI provider for specific use cases
  - Polly - Resilience policies for retry and circuit breaker patterns

* **Dependent Epics/Features:**
  - Cookbook prompt migration to support multi-provider function calling formats
  - AI controller enhancements for provider selection and health monitoring
  - Configuration management updates for multi-provider settings

## 7. Rationale & Key Historical Context

* **Strategic Context:** Current vendor lock-in with OpenAI creates resilience risks and limits capability optimization. Migration to Chat Completions API provides performance improvements while multi-provider support enables cost optimization and feature matching based on specific use case requirements.

* **Historical Evolution:** Started as OpenAI SDK v1.5 migration requirement, expanded to multi-provider architecture based on organizational strategy for AI provider diversity and vendor risk mitigation.

* **Architectural Decision Records:**
  - ADR-001: Adapter pattern selection for provider abstraction over service locator pattern
  - ADR-002: Unified message format over provider-specific message types throughout system
  - ADR-003: Backward compatibility preservation over clean slate rewrite for existing integrations

* **Alternative Approaches Considered:**
  - Service locator pattern rejected due to testing complexity and dependency hiding
  - Provider-specific message types throughout system rejected due to coupling concerns
  - Breaking change migration rejected due to extensive existing integration requirements

## 8. Known Issues & TODOs

* **Outstanding Design Decisions:**
  - Provider capability discovery implementation pattern (polling vs cached vs configuration-driven)
  - Message normalization strategy for providers with significant format differences
  - Tool calling schema unification approach across providers with different function calling patterns
  - Health check implementation frequency and failure threshold configuration

* **Implementation Risks:**
  - Provider API changes during development may require adapter updates
  - Tool calling format differences between providers may require complex normalization
  - Performance impact of provider routing logic on high-frequency operations
  - Configuration complexity with six providers may impact deployment and maintenance

* **Future Enhancements:**
  - Streaming response support across all providers with unified streaming interface
  - Advanced provider selection algorithms based on cost, latency, and capability optimization
  - Response caching layer for identical requests to improve performance and reduce costs
  - Provider analytics and usage tracking for optimization and cost management
  - Auto-scaling provider selection based on load and provider response times

---

**🗂️ WORKING DIRECTORY ARTIFACT CREATED:**
- **Filename:** README.md
- **Purpose:** Main Epic #246 specification document following TEMPLATE-epic-spec.md structure with comprehensive architectural overview
- **Context for Team:** Primary specification document for LanguageModelService v2 transformation providing strategic context and implementation roadmap
- **Dependencies:** Builds upon all working directory architectural artifacts and follows Epic #181 specification patterns
- **Next Actions:** Create detailed component specifications (01-05) and component interface documents
