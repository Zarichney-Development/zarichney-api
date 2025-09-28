# Prompt System Modernization Specification

**Last Updated:** 2025-01-27
**Status:** Planning
**Owner:** PromptEngineer

> **Parent:** [`Epic #246: LanguageModelService v2`](./README.md)

## 1. Purpose & Responsibility

* **What it is:** Comprehensive modernization of the Cookbook prompt system to leverage the new LanguageModelService v2 architecture, migrating all 8 prompts from OpenAI Assistants API to provider-agnostic Chat Completions + Responses API while maintaining functional equivalence and enabling multi-provider support.

* **Key Objectives:**
  - Migrate all 8 Cookbook prompts to provider-agnostic PromptBaseV2 architecture
  - Transform OpenAI Assistants API function calling to unified IFunctionDefinition pattern
  - Implement automatic JSON schema generation from C# response models
  - Establish provider compatibility across OpenAI and Venice.AI for all prompts (additional providers in future epics)
  - Maintain 100% functional equivalence during migration with zero business logic disruption

* **Success Criteria:**
  - All 8 prompts successfully migrated with comprehensive test coverage (>95%)
  - Provider compatibility validated across OpenAI and Venice.AI with consistent output quality
  - Performance improved by 20%+ through Chat Completions API efficiency gains
  - Schema generation system supports all existing response models with validation
  - Zero breaking changes to existing Cookbook service integration patterns

* **Why it exists:** Current Cookbook prompts are tightly coupled to OpenAI Assistants API with manual function definition management creating maintenance overhead and vendor lock-in. This modernization enables provider flexibility, improves maintainability through automated schema generation, and establishes foundation for future prompt expansion while preserving existing business logic.

### Component Specifications
* **Prompt Base v2:** [`components/prompt-base-v2`](./components/prompt-base-v2.md) - New provider-agnostic prompt architecture
* **Cookbook Migration:** [`components/cookbook-prompt-migration`](./components/cookbook-prompt-migration.md) - Individual prompt migration specifications

## 2. Architecture & Key Concepts

* **High-Level Design:** Provider-agnostic prompt architecture with automated schema generation, unified function calling abstraction, and backward compatibility layer. The design separates prompt business logic from provider-specific implementation details while maintaining existing service integration patterns.

* **Core Implementation Flow:**
  1. Prompt inherits from PromptBaseV2 with response model attributes
  2. JsonSchemaGenerator automatically creates function definitions from C# models
  3. PromptService routes to appropriate provider through ILanguageModelService
  4. Provider adapter converts unified function definition to provider-specific format
  5. Provider executes request and returns normalized response
  6. Response deserialized to strongly-typed C# model
  7. Result returned through existing LlmResult<T> pattern

* **Key Architectural Decisions:**
  - **Attribute-Based Schema Generation**: Use JsonRequired and Description attributes on response models to generate function definitions automatically
  - **Provider Abstraction Through Interface**: IPrompt interface with PromptBaseV2 base class isolates prompts from provider specifics
  - **Unified Function Definition**: IFunctionDefinition provides provider-agnostic function calling with automatic adapter conversion
  - **Backward Compatibility Preservation**: Existing Cookbook service interfaces unchanged during migration
  - **Template Method Pattern**: PromptBaseV2 provides common functionality while derived classes focus on business logic

* **Integration Points:**
  - LanguageModelService v2 architecture for provider routing and execution
  - Existing Cookbook services maintain current integration patterns
  - ISessionManager integration preserved for conversation continuity
  - Dependency injection patterns maintained for prompts requiring services (e.g., IMapper)

* **Architecture Diagram:**
  ```mermaid
  graph TB
      subgraph "Cookbook Services Layer"
          RECIPE_SVC[Recipe Service]
          ORDER_SVC[Order Service]
          SEARCH_SVC[Search Service]
      end

      subgraph "Prompt Layer"
          ANALYZE[AnalyzeRecipePrompt]
          SYNTHESIZE[SynthesizeRecipePrompt]
          PROCESS[ProcessOrderPrompt]
          QUERY[GetAlternativeQueryPrompt]
          CLEAN[CleanRecipePrompt]
          CHOOSE[ChooseRecipesPrompt]
          NAMER[RecipeNamerPrompt]
          RANK[RankRecipePrompt]
      end

      subgraph "Prompt Service Layer"
          PROMPT_SVC[IPromptService]
          PROMPT_BASE[PromptBaseV2]
          IPROMPT[IPrompt Interface]
      end

      subgraph "Schema Generation"
          JSON_GEN[JsonSchemaGenerator]
          FUNC_DEF[IFunctionDefinition]
          RESPONSE_MODELS[Response Models]
      end

      subgraph "Provider Integration"
          LMS_V2[ILanguageModelService v2]
          OPENAI_ADAPTER[OpenAI Adapter]
          ANTHROPIC_ADAPTER[Anthropic Adapter]
          VENICE_ADAPTER[Venice Adapter]
      end

      subgraph "Response Models"
          RECIPE_ANALYSIS[RecipeAnalysis]
          RECIPE_RESPONSE[SynthesizedRecipeResponse]
          QUERY_RESULT[AlternativeQueryResult]
      end

      RECIPE_SVC --> ANALYZE
      RECIPE_SVC --> SYNTHESIZE
      ORDER_SVC --> PROCESS
      SEARCH_SVC --> QUERY

      ANALYZE --> PROMPT_BASE
      SYNTHESIZE --> PROMPT_BASE
      PROCESS --> PROMPT_BASE
      QUERY --> PROMPT_BASE
      CLEAN --> PROMPT_BASE
      CHOOSE --> PROMPT_BASE
      NAMER --> PROMPT_BASE
      RANK --> PROMPT_BASE

      PROMPT_BASE --> IPROMPT
      PROMPT_BASE --> JSON_GEN
      JSON_GEN --> FUNC_DEF
      JSON_GEN --> RESPONSE_MODELS

      IPROMPT --> PROMPT_SVC
      PROMPT_SVC --> LMS_V2

      LMS_V2 --> OPENAI_ADAPTER
      LMS_V2 --> ANTHROPIC_ADAPTER
      LMS_V2 --> VENICE_ADAPTER

      RECIPE_ANALYSIS --> JSON_GEN
      RECIPE_RESPONSE --> JSON_GEN
      QUERY_RESULT --> JSON_GEN
  ```

## 3. Interface Contract & Assumptions

* **Key Deliverables:**
  - **PromptBaseV2 Architecture:**
    * **Purpose:** Provider-agnostic base class with automated schema generation and unified function calling
    * **Dependencies:** JsonSchemaGenerator, ILanguageModelService v2, response model attributes
    * **Outputs:** IFunctionDefinition objects with provider-specific adapters handling conversion
    * **Quality Gates:** All response models generate valid schemas, provider compatibility tested

  - **8 Migrated Cookbook Prompts:**
    * **Purpose:** All existing prompts converted to new architecture with preserved functionality
    * **Dependencies:** PromptBaseV2, strongly-typed response models, provider adapters
    * **Outputs:** Same business logic outputs through new architecture
    * **Quality Gates:** Functional equivalence testing, multi-provider validation, performance benchmarking

  - **JsonSchemaGenerator System:**
    * **Purpose:** Automatic JSON schema generation from C# models with validation and caching
    * **Dependencies:** System.Text.Json, reflection utilities, attribute processing
    * **Outputs:** JSON schemas compatible with all provider function calling formats
    * **Quality Gates:** Schema validation for all response models, performance testing for generation

  - **Provider Compatibility Layer:**
    * **Purpose:** Seamless function calling across OpenAI, Anthropic, and Venice.AI providers
    * **Dependencies:** Provider adapters, function definition conversion, response normalization
    * **Outputs:** Consistent prompt behavior regardless of provider selection
    * **Quality Gates:** Cross-provider testing, response format validation, quality consistency

* **Critical Assumptions:**
  - **Technical Assumptions:** LanguageModelService v2 provides stable provider routing, C# attribute system supports comprehensive schema generation, provider APIs maintain function calling compatibility
  - **Resource Assumptions:** Existing prompt business logic can be preserved during migration, response models can be enhanced with attributes without breaking existing serialization
  - **External Dependencies:** Provider SDKs support required function calling patterns, existing test infrastructure can validate migration equivalence

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Prompt-Specific Standards:**
  - Response model naming convention: `{PromptName}Response` (e.g., `RecipeAnalysis`, `SynthesizedRecipeResponse`)
  - All response properties must use `[JsonRequired]` and `[Description]` attributes for schema generation
  - Prompt naming convention: `{BusinessFunction}Prompt` following existing patterns
  - Function names should match prompt business purpose (e.g., "AnalyzeRecipe", "SynthesizeRecipe")
  - Provider preferences specified through ModelConfig for optimization

* **Technology Constraints:**
  - System.Text.Json for all serialization to maintain project consistency
  - C# 12 language features for record types and primary constructors where beneficial
  - Reflection-based schema generation with caching for performance
  - Async/await patterns with CancellationToken support throughout
  - Strong typing required for all function definitions and response models

* **Migration Constraints:**
  - Zero breaking changes to existing Cookbook service interfaces during transition
  - Functional equivalence required for all 8 prompts before architecture completion
  - Provider compatibility testing required for OpenAI, Anthropic, and Venice.AI minimum
  - Performance baseline maintenance or improvement required during migration
  - Existing conversation management patterns must be preserved

## 5. How to Work With This Specification

* **Implementation Approach:**
  - Begin with PromptBaseV2 and JsonSchemaGenerator implementation and testing
  - Migrate prompts in priority order: AnalyzeRecipe, SynthesizeRecipe, ProcessOrder (core business)
  - Validate functional equivalence for each prompt before proceeding to next
  - Test provider compatibility incrementally as prompts are migrated
  - Use feature flags for gradual rollout of migrated prompts

* **Quality Assurance:**
  - **Testing Strategy:** Unit tests for schema generation, integration tests for prompt execution, contract tests for provider compatibility
  - **Validation Approach:** Side-by-side comparison of legacy vs migrated prompt outputs, provider response consistency validation
  - **Performance Validation:** Response time benchmarking, schema generation performance testing, memory usage validation

* **Common Implementation Pitfalls:**
  - JSON schema generation complexity for nested objects and arrays requiring careful reflection handling
  - Provider-specific function calling format differences requiring robust adapter implementation
  - Response model attribute requirements may require updates to existing models
  - Conversation context preservation across provider switching requires careful session management
  - Performance impact of schema generation requires appropriate caching strategies

## 6. Dependencies

* **Internal Code Dependencies:**
  - [`LanguageModelService v2`](./01-architecture-design.md) - Core provider abstraction and routing
  - [`Provider Adapters`](./02-provider-integration.md) - Provider-specific function calling implementations
  - [`Cookbook Services`](../../Code/Zarichney.Server/Services/Cookbook/README.md) - Existing service integration requiring preservation
  - [`Session Management`](../../Code/Zarichney.Server/Services/Sessions/README.md) - Conversation persistence patterns

* **External Dependencies:**
  - System.Text.Json - JSON serialization and schema generation
  - System.Reflection - Attribute processing and type analysis for schema generation
  - Provider SDKs - OpenAI v1.5, Anthropic, and Venice.AI client libraries
  - AutoMapper - Required for SynthesizeRecipePrompt dependency injection pattern

* **Component Dependencies:**
  - JsonSchemaGenerator must be implemented before prompt migration begins
  - PromptBaseV2 architecture must be validated before individual prompt conversion
  - Provider adapters must support function calling before prompt compatibility testing
  - Response models may require attribute enhancement for optimal schema generation

## 7. Rationale & Key Historical Context

* **Strategic Context:** Current prompt system creates maintenance overhead with manual function definitions and vendor lock-in with OpenAI Assistants API. Migration to provider-agnostic architecture enables vendor flexibility while improving maintainability through automated schema generation.

* **Historical Evolution:** Started as OpenAI SDK v1.5 migration requirement, expanded to comprehensive prompt modernization based on need for provider diversity and improved maintainability. Schema generation approach evolved from manual definitions to attribute-based automation.

* **Architectural Decision Records:**
  - ADR-001: Attribute-based schema generation over manual function definitions for maintainability
  - ADR-002: Provider abstraction through interfaces over provider-specific prompt implementations
  - ADR-003: Backward compatibility preservation over clean slate rewrite for business continuity

* **Alternative Approaches Considered:**
  - Manual function definition migration rejected due to maintenance overhead and error potential
  - Provider-specific prompt implementations rejected due to code duplication and maintenance complexity
  - Breaking change migration rejected due to extensive existing Cookbook service integration

## 8. Known Issues & TODOs

* **Outstanding Design Decisions:**
  - Response model attribute requirements for optimal schema generation (required vs optional attributes)
  - Caching strategy for generated schemas (memory vs distributed cache for performance)
  - Error handling patterns for schema generation failures and provider-specific validation errors
  - Migration rollback strategy if functional equivalence cannot be achieved

* **Implementation Risks:**
  - Complex response models may require sophisticated schema generation logic
  - Provider function calling differences may require extensive adapter complexity
  - Performance impact of reflection-based schema generation may require optimization
  - Existing response models may need updates for optimal attribute support

* **Future Enhancements:**
  - Advanced schema validation with custom validation attributes
  - Prompt versioning system for A/B testing different prompt variants
  - Dynamic schema optimization based on provider capabilities
  - Automated prompt performance optimization based on provider response patterns
  - Schema generation extension for complex nested objects and validation rules

## Cookbook Prompt Migration Priority

### Phase 1: Core Business Prompts (High Priority)
1. **AnalyzeRecipePrompt** - Critical for recipe quality assurance workflow
2. **SynthesizeRecipePrompt** - Core recipe generation functionality
3. **ProcessOrderPrompt** - Essential order processing workflow

### Phase 2: Search & Discovery Prompts (Medium Priority)
4. **GetAlternativeQueryPrompt** - Search optimization
5. **RankRecipePrompt** - Result ranking and relevancy
6. **ChooseRecipesPrompt** - Recipe selection and curation

### Phase 3: Enhancement Prompts (Lower Priority)
7. **CleanRecipePrompt** - Data quality improvement
8. **RecipeNamerPrompt** - Content enhancement and organization

## Provider Compatibility Requirements

* **OpenAI**: Full compatibility with strict mode function calling
* **Anthropic**: Tool calling format with enhanced reasoning capabilities
* **Venice.AI**: Basic compatibility with text-based fallback for limited function calling
* **Future Providers**: Extensible architecture for additional provider integration

---

**🗂️ WORKING DIRECTORY ARTIFACT CREATED:**
- **Filename:** 06-prompt-system-modernization.md
- **Purpose:** Main prompt system modernization specification document providing comprehensive migration strategy and architecture
- **Context for Team:** Primary specification for transforming all 8 Cookbook prompts to provider-agnostic architecture with automated schema generation
- **Dependencies:** Builds upon working directory prompt migration analysis and integrates with LanguageModelService v2 architecture
- **Next Actions:** Create component specifications for PromptBaseV2 and cookbook migration details
