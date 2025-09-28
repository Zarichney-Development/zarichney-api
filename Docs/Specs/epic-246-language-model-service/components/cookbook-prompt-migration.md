# Cookbook Prompt Migration Component Specification

**Last Updated:** 2025-01-27
**Status:** Planning
**Owner:** PromptEngineer

> **Parent:** [`06-prompt-system-modernization`](../06-prompt-system-modernization.md)

## 1. Purpose & Responsibility

* **What it is:** Comprehensive migration specification for all 8 Cookbook prompts from OpenAI Assistants API to provider-agnostic PromptBaseV2 architecture, detailing specific transformation requirements, provider compatibility considerations, and validation criteria for each prompt.

* **Key Objectives:**
  - Define migration approach for each of the 8 Cookbook prompts with specific requirements
  - Establish provider compatibility matrix for OpenAI and Venice.AI (additional providers in future epics)
  - Detail response model transformations from manual JSON to attribute-based C# models
  - Specify testing requirements and validation criteria for functional equivalence
  - Document priority order and dependencies for phased migration implementation

* **Success Criteria:**
  - All 8 prompts successfully migrated with preserved business logic and output quality
  - Provider compatibility validated across OpenAI and Venice.AI with consistent response quality
  - Performance maintained or improved through Chat Completions API efficiency gains
  - Comprehensive test coverage (>95%) for all migrated prompts with multi-provider validation
  - Zero breaking changes to existing Cookbook service integration patterns

* **Why it exists:** Each Cookbook prompt has unique business logic, response model complexity, and integration patterns requiring specific migration considerations. This specification ensures systematic migration preserving functionality while enabling multi-provider capabilities.

## 2. Prompt Migration Matrix

### Migration Priority & Dependencies

| Priority | Prompt | Business Impact | Complexity | Provider Requirements | Dependencies |
|----------|--------|----------------|------------|---------------------|-------------|
| **Phase 1 (High)** | AnalyzeRecipePrompt | Critical - Quality assurance | Medium | OpenAI, Anthropic | PromptBaseV2 |
| **Phase 1 (High)** | SynthesizeRecipePrompt | Critical - Core generation | High | OpenAI, Anthropic | PromptBaseV2, IMapper |
| **Phase 1 (High)** | ProcessOrderPrompt | Critical - Order workflow | High | OpenAI, Anthropic | PromptBaseV2 |
| **Phase 2 (Medium)** | GetAlternativeQueryPrompt | Important - Search optimization | Low | All providers | Phase 1 complete |
| **Phase 2 (Medium)** | RankRecipePrompt | Important - Result ranking | Medium | All providers | Phase 1 complete |
| **Phase 2 (Medium)** | ChooseRecipesPrompt | Important - Recipe selection | Medium | All providers | Phase 1 complete |
| **Phase 3 (Lower)** | CleanRecipePrompt | Enhancement - Data quality | Medium | All providers | Phase 2 complete |
| **Phase 3 (Lower)** | RecipeNamerPrompt | Enhancement - Content naming | Low | All providers | Phase 2 complete |

### Provider Compatibility Assessment (Epic 246 scope)

| Prompt | OpenAI | Venice.AI | Notes |
|--------|--------|-----------|-------|
| AnalyzeRecipePrompt | ✅ Full | ⚠️ Limited | Venice may need text parsing fallback |
| SynthesizeRecipePrompt | ✅ Full | ⚠️ Partial | Complex schema may need simplification |
| ProcessOrderPrompt | ✅ Full | ⚠️ Limited | Workflow complexity challenges Venice |
| GetAlternativeQueryPrompt | ✅ Full | ✅ Good | Simple response works well |
| RankRecipePrompt | ✅ Full | ✅ Good | Scoring task suitable for both |
| ChooseRecipesPrompt | ✅ Full | ✅ Good | Selection logic works across providers |
| CleanRecipePrompt | ✅ Full | ⚠️ Limited | Data transformation complexity |
| RecipeNamerPrompt | ✅ Full | ✅ Good | Creative naming works well |

## 3. Individual Prompt Migration Specifications

### 3.1 AnalyzeRecipePrompt Migration

**Current Implementation Analysis:**
- Uses `RecipeAnalysis` response model with QualityScore, Analysis, Suggestions
- Requires conversationId for iterative refinement
- Critical for recipe quality assurance workflow
- Moderate complexity with well-defined JSON schema

**Migration Implementation:**
```csharp
public class AnalyzeRecipePrompt : PromptBaseV2
{
    public override string Name => "Recipe Quality Analyzer";
    public override string Description => "Analyze synthesized recipes for quality and alignment with cookbook specifications";

    public override string SystemPrompt => """
        # Recipe Quality Assurance System

        You are a professional cookbook editor and culinary expert responsible for analyzing synthesized recipes.
        Your role is to evaluate recipes against cookbook order specifications and provide actionable feedback.

        ## Analysis Framework

        Evaluate recipes across these dimensions:
        1. **Specification Alignment**: How well does the recipe match the cookbook order requirements?
        2. **Culinary Accuracy**: Are the techniques, ingredients, and cooking methods sound?
        3. **Clarity & Completeness**: Are instructions clear and complete for the target skill level?
        4. **Practicality**: Is the recipe realistic for home cooking with available ingredients?

        ## Scoring Guidelines

        Quality Score (1-100):
        - 90-100: Exceptional - Ready for publication with minimal editing
        - 80-89: Very Good - Minor refinements needed
        - 70-79: Good - Some improvements required
        - 60-69: Acceptable - Significant improvements needed
        - Below 60: Needs major revision or replacement

        ## Output Requirements

        Provide a critical, constructive analysis that helps improve recipe quality.
        Focus on specific, actionable suggestions that address identified issues.
        """;

    public override PromptModelConfig ModelConfig => new()
    {
        PreferredProvider = "OpenAI",
        PreferredModel = "gpt-4o-mini",
        FallbackProviders = new[] { "Venice.AI" },
        Temperature = 0.1,  // Low for consistent analysis
        MaxTokens = 4000
    };

    protected override IFunctionDefinition GetPrimaryFunction()
    {
        return CreateFunction<RecipeAnalysis>(
            "AnalyzeRecipe",
            "Analyze a synthesized recipe based on cookbook order specifications"
        );
    }

    public string GetUserPrompt(SynthesizedRecipe recipe, CookbookOrder order, string? recipeName) =>
        $"""
        ## Analysis Request

        Please analyze the following synthesized recipe for quality and alignment with the cookbook order specifications.

        ### Cookbook Order Context:
        ```md
        {order.ToMarkdown()}
        ```

        ### Recipe to Analyze:
        **Recipe Name:** {recipeName ?? recipe.Title}

        ```json
        {JsonSerializer.Serialize(recipe, new JsonSerializerOptions { WriteIndented = true })}
        ```

        Please provide a comprehensive analysis with quality score and specific suggestions for improvement.
        """;
}

public class RecipeAnalysis
{
    [JsonRequired]
    [Description("A score from 1 to 100 indicating the overall quality and alignment with cookbook specifications")]
    [JsonPropertyName("qualityScore")]
    [Range(1, 100)]
    public int QualityScore { get; set; }

    [JsonRequired]
    [Description("A detailed, critical evaluation of the recipe's alignment with cookbook requirements, culinary accuracy, and completeness")]
    [JsonPropertyName("analysis")]
    [MinLength(100)]
    public string? Analysis { get; set; }

    [JsonRequired]
    [Description("Specific, actionable suggestions for improving the recipe to better meet cookbook standards")]
    [JsonPropertyName("suggestions")]
    [MinLength(50)]
    public string? Suggestions { get; set; }
}
```

**Provider-Specific Considerations:**
- **OpenAI**: Uses strict mode for consistent scoring, excellent for analytical tasks
- **Anthropic**: Leverages Claude's strong reasoning for detailed analysis
- **Venice.AI**: May require text parsing fallback, simplified scoring guidance

**Testing Requirements:**
- Validate quality scores are within 1-100 range and meaningful
- Ensure analysis text provides actionable feedback (>100 characters)
- Test conversation continuity for iterative refinement
- Verify provider consistency with side-by-side analysis comparison

### 3.2 SynthesizeRecipePrompt Migration

**Current Implementation Analysis:**
- Most complex prompt with detailed recipe structure output
- Requires IMapper dependency injection for data transformation
- Uses large context for multiple source recipes
- Critical for core recipe generation business logic

**Migration Implementation:**
```csharp
public class SynthesizeRecipePrompt(IMapper mapper) : PromptBaseV2
{
    private readonly IMapper _mapper = mapper;

    public override string Name => "Recipe Synthesizer";
    public override string Description => "Synthesize personalized recipes from existing recipes and user preferences";

    public override string SystemPrompt => """
        # Recipe Synthesis Expert System

        You are a master chef and recipe developer with expertise in creating personalized recipes.
        Your role is to synthesize new recipes by combining the best elements from existing recipes
        while adapting them to specific user preferences and cookbook requirements.

        ## Synthesis Guidelines

        1. **Preserve Culinary Integrity**: Maintain proper cooking techniques and flavor balance
        2. **Honor User Preferences**: Adapt recipes to dietary restrictions, skill level, and preferences
        3. **Optimize for Success**: Ensure recipes are achievable for the target audience
        4. **Cultural Sensitivity**: Respect traditional cooking methods and cultural contexts

        ## Recipe Structure Requirements

        - **Title**: Clear, appetizing name that reflects the dish
        - **Description**: Engaging introduction that sets expectations
        - **Servings**: Practical serving size for typical households
        - **Timing**: Realistic prep, cook, and total time estimates
        - **Ingredients**: Precise measurements with common availability
        - **Directions**: Step-by-step instructions with cooking techniques
        - **Inspiration**: Credit source recipes that influenced the synthesis
        - **Notes**: Helpful tips, variations, or serving suggestions

        ## Quality Standards

        Every synthesized recipe must be:
        - Technically sound with proper cooking methods
        - Achievable with commonly available ingredients
        - Clearly written for the specified skill level
        - Delicious and satisfying for the intended audience
        """;

    public override PromptModelConfig ModelConfig => new()
    {
        PreferredProvider = "OpenAI",
        PreferredModel = "gpt-4o-mini",
        FallbackProviders = new[] { "Venice.AI" },
        Temperature = 0.2,  // Slightly higher for creativity
        MaxTokens = 6000    // Higher for complex recipe generation
    };

    protected override IFunctionDefinition GetPrimaryFunction()
    {
        return CreateFunction<SynthesizedRecipeResponse>(
            "SynthesizeRecipe",
            "Synthesize a personalized recipe using existing recipes and user's cookbook order preferences"
        );
    }

    public string GetUserPrompt(string recipeName, List<Recipe> recipes, CookbookOrder order) =>
        $"""
        ## Recipe Synthesis Request

        Please synthesize a personalized recipe for **{recipeName}** based on the provided source recipes and cookbook order specifications.

        ### Cookbook Order Requirements:
        ```md
        {order.ToMarkdown()}
        ```

        ### Source Recipe Data:
        ```json
        {JsonSerializer.Serialize(_mapper.Map<List<ScrapedRecipe>>(recipes), new JsonSerializerOptions { WriteIndented = true })}
        ```

        Create a unique, personalized recipe that combines the best elements from the source recipes while meeting the cookbook order requirements.
        """;
}

public class SynthesizedRecipeResponse
{
    [JsonRequired]
    [Description("The descriptive, appetizing name for the synthesized recipe")]
    [JsonPropertyName("title")]
    [MinLength(5)]
    [MaxLength(100)]
    public string? Title { get; set; }

    [JsonRequired]
    [Description("An engaging introduction that describes the dish and sets expectations")]
    [JsonPropertyName("description")]
    [MinLength(50)]
    [MaxLength(500)]
    public string? Description { get; set; }

    [JsonRequired]
    [Description("Number of servings this recipe yields")]
    [JsonPropertyName("servings")]
    [RegularExpression(@"^\d+(-\d+)?( servings?)?$")]
    public string? Servings { get; set; }

    [JsonRequired]
    [Description("Time required for preparation work")]
    [JsonPropertyName("prepTime")]
    [RegularExpression(@"^\d+\s*(minutes?|mins?|hours?|hrs?)$")]
    public string? PrepTime { get; set; }

    [JsonRequired]
    [Description("Active cooking time")]
    [JsonPropertyName("cookTime")]
    [RegularExpression(@"^\d+\s*(minutes?|mins?|hours?|hrs?)$")]
    public string? CookTime { get; set; }

    [JsonRequired]
    [Description("Total time from start to finish")]
    [JsonPropertyName("totalTime")]
    [RegularExpression(@"^\d+\s*(minutes?|mins?|hours?|hrs?)$")]
    public string? TotalTime { get; set; }

    [JsonRequired]
    [Description("Complete list of ingredients with precise measurements")]
    [JsonPropertyName("ingredients")]
    [MinLength(3)]
    public List<string> Ingredients { get; set; } = new();

    [JsonRequired]
    [Description("Step-by-step cooking instructions with proper technique guidance")]
    [JsonPropertyName("directions")]
    [MinLength(3)]
    public List<string> Directions { get; set; } = new();

    [JsonRequired]
    [Description("Source recipes or cooking techniques that inspired this synthesis")]
    [JsonPropertyName("inspiredBy")]
    public List<string> InspiredBy { get; set; } = new();

    [JsonRequired]
    [Description("Additional cooking tips, variations, storage instructions, or serving suggestions")]
    [JsonPropertyName("notes")]
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
```

**Provider-Specific Considerations:**
- **OpenAI**: Excellent for creative synthesis with consistent structure
- **Anthropic**: Superior reasoning for complex recipe adaptation and cultural sensitivity
- **Venice.AI**: May need simplified schema or multiple-step generation for complex recipes

**Testing Requirements:**
- Validate all required fields are populated with realistic content
- Test ingredient/direction coherence with automated parsing validation
- Verify time estimates are reasonable and properly formatted
- Test with various cookbook order types (dietary restrictions, cuisines, skill levels)

### 3.3 GetAlternativeQueryPrompt Migration

**Current Implementation Analysis:**
- Simplest prompt with single-property response
- Used for search optimization workflow
- Low complexity, high provider compatibility
- Good candidate for early migration validation

**Migration Implementation:**
```csharp
public class GetAlternativeQueryPrompt : PromptBaseV2
{
    public override string Name => "Search Query Optimizer";
    public override string Description => "Generate alternative search queries for improved recipe discovery";

    public override string SystemPrompt => """
        # Search Optimization Assistant

        You are a search optimization expert specializing in culinary and recipe discovery.
        When given a recipe search query, generate alternative queries that might yield
        better or more diverse results.

        ## Optimization Strategies

        1. **Synonym Expansion**: Use cooking synonyms and alternative terms
        2. **Cultural Variations**: Include international names for dishes
        3. **Technique Focus**: Emphasize cooking methods or ingredients
        4. **Dietary Angles**: Consider health, dietary, or lifestyle variations
        5. **Specificity Adjustment**: Make queries more or less specific as appropriate

        ## Quality Guidelines

        Alternative queries should:
        - Maintain the original intent while expanding possibilities
        - Use natural language that real users would search for
        - Include relevant cooking terminology and ingredient names
        - Be concise and focused (typically 2-6 words)
        """;

    public override PromptModelConfig ModelConfig => new()
    {
        PreferredProvider = "OpenAI",
        PreferredModel = "gpt-4o-mini",
        FallbackProviders = new[] { "Venice.AI" },
        Temperature = 0.3,  // Higher for creative query generation
        MaxTokens = 1000    // Lower - simple response
    };

    protected override IFunctionDefinition GetPrimaryFunction()
    {
        return CreateFunction<AlternativeQueryResult>(
            "GetAlternativeQuery",
            "Generate an alternative search query for improved recipe discovery"
        );
    }

    public string GetUserPrompt(string originalQuery, string? conversationId = null) =>
        $"""
        ## Search Query Optimization Request

        Original search query: "{originalQuery}"

        Please generate an alternative search query that maintains the original intent but might
        yield better or more diverse recipe results. Consider synonyms, cultural variations,
        cooking techniques, or dietary angles that could improve search effectiveness.
        """;
}

public class AlternativeQueryResult
{
    [JsonRequired]
    [Description("An alternative search query optimized for recipe discovery that maintains the original intent while potentially yielding better results")]
    [JsonPropertyName("newQuery")]
    [MinLength(2)]
    [MaxLength(50)]
    public string? NewQuery { get; set; }
}
```

**Provider-Specific Considerations:**
- **OpenAI**: Good creative query generation with consistent quality
- **Anthropic**: Excellent understanding of culinary terminology and cultural context
- **Venice.AI**: Works well for simple query transformation tasks

**Testing Requirements:**
- Validate alternative queries maintain original intent
- Test query quality with real search scenarios
- Verify provider consistency with parallel query generation
- Performance testing for rapid query optimization

### 3.4 ProcessOrderPrompt Migration

**Current Implementation Analysis:**
- Complex workflow orchestration prompt
- Generates processing instructions and recipe lists
- Critical for order initialization and planning
- High business impact requiring careful migration

**Migration Implementation:**
```csharp
public class ProcessOrderPrompt : PromptBaseV2
{
    public override string Name => "Order Processing Orchestrator";
    public override string Description => "Generate comprehensive processing workflows and recipe lists for cookbook orders";

    public override string SystemPrompt => """
        # Cookbook Order Processing System

        You are an expert cookbook project manager responsible for analyzing cookbook orders
        and creating comprehensive processing workflows. Your role is to break down complex
        cookbook requirements into actionable steps and organized recipe lists.

        ## Processing Framework

        1. **Order Analysis**: Understand scope, constraints, and priorities
        2. **Recipe Categorization**: Organize recipes by type, difficulty, and dependencies
        3. **Workflow Planning**: Create logical sequence for recipe development
        4. **Resource Planning**: Identify ingredients, techniques, and timing requirements
        5. **Quality Assurance**: Plan testing and refinement phases

        ## Workflow Components

        - **Recipe Lists**: Organized by category, priority, and development sequence
        - **Processing Steps**: Detailed workflow for recipe development and testing
        - **Timeline**: Realistic scheduling for recipe creation and validation
        - **Dependencies**: Identify recipes that build upon each other
        - **Quality Gates**: Checkpoints for review and approval

        ## Success Criteria

        Every processing plan must:
        - Address all cookbook order requirements comprehensively
        - Provide realistic timelines and resource estimates
        - Include quality assurance and testing phases
        - Be actionable and clearly sequenced
        """;

    public override PromptModelConfig ModelConfig => new()
    {
        PreferredProvider = "OpenAI",
        PreferredModel = "gpt-4o-mini",
        FallbackProviders = new[] { "Venice.AI" },
        Temperature = 0.1,  // Low for consistent workflow planning
        MaxTokens = 8000    // High for complex workflow outputs
    };

    public override PromptCapabilityRequirements GetCapabilityRequirements() => new()
    {
        RequiresFunctionCalling = true,
        RequiresStrictMode = true,
        RequiresConversationHistory = false,
        MinimumContextWindow = 16000  // Higher for complex order analysis
    };

    protected override IFunctionDefinition GetPrimaryFunction()
    {
        return CreateFunction<OrderProcessingPlan>(
            "ProcessOrder",
            "Generate comprehensive processing workflow and recipe lists for cookbook order"
        );
    }

    public string GetUserPrompt(CookbookOrder order) =>
        $"""
        ## Cookbook Order Processing Request

        Please analyze the following cookbook order and create a comprehensive processing plan
        with organized recipe lists and detailed workflow steps.

        ### Cookbook Order Details:
        ```md
        {order.ToMarkdown()}
        ```

        Generate a complete processing plan that addresses all requirements and provides
        actionable steps for successful cookbook development.
        """;
}

public class OrderProcessingPlan
{
    [JsonRequired]
    [Description("Organized list of recipes categorized by type, priority, and development sequence")]
    [JsonPropertyName("recipeLists")]
    [MinLength(1)]
    public List<RecipeCategory> RecipeLists { get; set; } = new();

    [JsonRequired]
    [Description("Detailed workflow steps for recipe development, testing, and validation")]
    [JsonPropertyName("processingSteps")]
    [MinLength(3)]
    public List<ProcessingStep> ProcessingSteps { get; set; } = new();

    [JsonRequired]
    [Description("Estimated timeline for cookbook development with key milestones")]
    [JsonPropertyName("timeline")]
    public TimelineEstimate Timeline { get; set; } = new();

    [JsonRequired]
    [Description("Quality assurance checkpoints and validation criteria")]
    [JsonPropertyName("qualityGates")]
    [MinLength(1)]
    public List<QualityGate> QualityGates { get; set; } = new();

    [JsonRequired]
    [Description("Additional notes, considerations, or recommendations for successful execution")]
    [JsonPropertyName("notes")]
    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class RecipeCategory
{
    [JsonRequired]
    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [JsonRequired]
    [JsonPropertyName("recipes")]
    public List<string> Recipes { get; set; } = new();

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "Medium";

    [JsonPropertyName("estimatedTime")]
    public string? EstimatedTime { get; set; }
}

public class ProcessingStep
{
    [JsonRequired]
    [JsonPropertyName("stepName")]
    public string? StepName { get; set; }

    [JsonRequired]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("dependencies")]
    public List<string> Dependencies { get; set; } = new();

    [JsonPropertyName("estimatedDuration")]
    public string? EstimatedDuration { get; set; }
}

public class TimelineEstimate
{
    [JsonRequired]
    [JsonPropertyName("totalDuration")]
    public string? TotalDuration { get; set; }

    [JsonRequired]
    [JsonPropertyName("phases")]
    public List<ProjectPhase> Phases { get; set; } = new();
}

public class ProjectPhase
{
    [JsonRequired]
    [JsonPropertyName("phaseName")]
    public string? PhaseName { get; set; }

    [JsonRequired]
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("deliverables")]
    public List<string> Deliverables { get; set; } = new();
}

public class QualityGate
{
    [JsonRequired]
    [JsonPropertyName("gateName")]
    public string? GateName { get; set; }

    [JsonRequired]
    [JsonPropertyName("criteria")]
    public string? Criteria { get; set; }

    [JsonPropertyName("timing")]
    public string? Timing { get; set; }
}
```

**Provider-Specific Considerations:**
- **OpenAI**: Excellent structured planning with consistent organization
- **Anthropic**: Superior reasoning for complex workflow orchestration and dependencies
- **Venice.AI**: May need simplified structure or multi-step approach for complex planning

**Testing Requirements:**
- Validate workflow completeness addresses all order requirements
- Test timeline reasonableness and dependency logic
- Verify quality gate coverage for cookbook development phases
- Cross-provider consistency testing for workflow structure

## 4. Testing & Validation Framework

### 4.1 Functional Equivalence Testing

```csharp
[TestFixture]
public class PromptMigrationEquivalenceTests
{
    [Test]
    [TestCase("AnalyzeRecipePrompt")]
    [TestCase("SynthesizeRecipePrompt")]
    [TestCase("ProcessOrderPrompt")]
    public async Task MigratedPrompt_ShouldProduceEquivalentResults(string promptName)
    {
        // Arrange
        var legacyPrompt = CreateLegacyPrompt(promptName);
        var migratedPrompt = CreateMigratedPrompt(promptName);
        var testInput = GetStandardTestInput(promptName);

        // Act
        var legacyResult = await ExecuteLegacyPrompt(legacyPrompt, testInput);
        var migratedResult = await ExecuteMigratedPrompt(migratedPrompt, testInput);

        // Assert
        AssertFunctionalEquivalence(legacyResult, migratedResult, promptName);
    }

    private void AssertFunctionalEquivalence(object legacy, object migrated, string promptName)
    {
        switch (promptName)
        {
            case "AnalyzeRecipePrompt":
                var legacyAnalysis = (LegacyRecipeAnalysis)legacy;
                var migratedAnalysis = (RecipeAnalysis)migrated;

                Assert.AreEqual(legacyAnalysis.QualityScore, migratedAnalysis.QualityScore, 5);
                Assert.IsTrue(SemanticSimilarity(legacyAnalysis.Analysis, migratedAnalysis.Analysis) > 0.8);
                break;

            case "SynthesizeRecipePrompt":
                AssertRecipeSynthesisEquivalence(legacy, migrated);
                break;

            case "ProcessOrderPrompt":
                AssertOrderProcessingEquivalence(legacy, migrated);
                break;
        }
    }
}
```

### 4.2 Provider Compatibility Testing

```csharp
[TestFixture]
public class ProviderCompatibilityTests
{
    [Test]
    [TestCase("OpenAI", "AnalyzeRecipePrompt")]
    [TestCase("Anthropic", "AnalyzeRecipePrompt")]
    [TestCase("Venice", "AnalyzeRecipePrompt")]
    public async Task Prompt_ShouldWorkAcrossProviders(string provider, string promptName)
    {
        // Arrange
        var prompt = CreateMigratedPrompt(promptName);
        var testInput = GetStandardTestInput(promptName);

        // Override provider preference
        prompt.ModelConfig.PreferredProvider = provider;

        // Act
        var result = await ExecuteWithProvider(prompt, testInput, provider);

        // Assert
        Assert.IsNotNull(result);
        ValidateResponseQuality(result, promptName, provider);
    }

    [Test]
    public async Task ProvidersShould_ProduceConsistentResults()
    {
        // Arrange
        var prompt = new AnalyzeRecipePrompt();
        var testInput = GetStandardAnalysisInput();
        var providers = new[] { "OpenAI", "Anthropic" };
        var results = new List<RecipeAnalysis>();

        // Act
        foreach (var provider in providers)
        {
            prompt.ModelConfig.PreferredProvider = provider;
            var result = await ExecutePrompt<RecipeAnalysis>(prompt, testInput);
            results.Add(result);
        }

        // Assert
        var scoreVariance = results.Max(r => r.QualityScore) - results.Min(r => r.QualityScore);
        Assert.LessOrEqual(scoreVariance, 15, "Quality scores should be consistent across providers");

        // Validate semantic consistency
        for (int i = 1; i < results.Count; i++)
        {
            var similarity = SemanticSimilarity(results[0].Analysis, results[i].Analysis);
            Assert.Greater(similarity, 0.7, $"Analysis similarity should be high across providers");
        }
    }
}
```

### 4.3 Performance Validation Testing

```csharp
[TestFixture]
public class PromptPerformanceTests
{
    [Test]
    [TestCase("AnalyzeRecipePrompt", 3000)]
    [TestCase("SynthesizeRecipePrompt", 8000)]
    [TestCase("GetAlternativeQueryPrompt", 1500)]
    [TestCase("ProcessOrderPrompt", 10000)]
    public async Task MigratedPrompt_ShouldMeetPerformanceTargets(string promptName, int maxLatencyMs)
    {
        // Arrange
        var prompt = CreateMigratedPrompt(promptName);
        var testInput = GetPerformanceTestInput(promptName);
        var measurements = new List<long>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            await ExecutePrompt(prompt, testInput);
            stopwatch.Stop();
            measurements.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert
        var p95Latency = measurements.OrderBy(x => x).Skip(4).First();
        Assert.Less(p95Latency, maxLatencyMs,
            $"P95 latency {p95Latency}ms exceeds target {maxLatencyMs}ms for {promptName}");
    }

    [Test]
    public async Task SchemaGeneration_ShouldBeCached()
    {
        // Arrange
        var prompt1 = new AnalyzeRecipePrompt();
        var prompt2 = new AnalyzeRecipePrompt();

        // Act
        var schema1 = prompt1.GetFunctions().First().ParameterSchema;
        var schema2 = prompt2.GetFunctions().First().ParameterSchema;

        // Assert
        Assert.AreSame(schema1, schema2, "Schemas should be cached for performance");
    }
}
```

## 5. Migration Implementation Guidelines

### 5.1 Phase 1: Core Business Prompts

**Implementation Order:**
1. **AnalyzeRecipePrompt** - Test schema generation and provider compatibility
2. **SynthesizeRecipePrompt** - Validate complex response models and dependency injection
3. **ProcessOrderPrompt** - Verify complex workflow orchestration capabilities

**Validation Criteria:**
- Functional equivalence tests pass with >95% similarity
- Provider compatibility validated for OpenAI and Anthropic minimum
- Performance within 10% of baseline or improved
- All existing Cookbook service integration tests pass

### 5.2 Phase 2: Search & Discovery Prompts

**Implementation Order:**
4. **GetAlternativeQueryPrompt** - Simple prompt for validation
5. **RankRecipePrompt** - Medium complexity scoring logic
6. **ChooseRecipesPrompt** - Selection logic with multiple options

**Validation Criteria:**
- All providers including Venice.AI compatibility tested
- Search workflow integration validated end-to-end
- Performance improvements from Chat Completions API measured

### 5.3 Phase 3: Enhancement Prompts

**Implementation Order:**
7. **CleanRecipePrompt** - Data transformation and cleaning
8. **RecipeNamerPrompt** - Creative naming and organization

**Validation Criteria:**
- Complete migration with all prompts using PromptBaseV2
- Comprehensive test coverage >95% achieved
- Performance and quality metrics meet or exceed baseline

### 5.4 Risk Mitigation Strategies

**Rollback Procedures:**
- Feature flags for gradual prompt migration rollout
- Automated rollback triggers based on quality degradation
- Preserved legacy prompt implementations during transition
- Real-time monitoring for quality and performance metrics

**Quality Assurance:**
- Side-by-side testing with legacy implementations
- Provider-specific testing with quality thresholds
- Performance benchmarking and regression detection
- User acceptance testing with actual cookbook workflows

## 6. Dependencies & Integration

* **Migration Dependencies:**
  - PromptBaseV2 architecture completed and tested
  - JsonSchemaGenerator implementation with validation
  - ILanguageModelService v2 with provider routing
  - Provider adapters for function calling conversion

* **Service Integration:**
  - Existing Cookbook services maintain current interfaces
  - Dependency injection patterns preserved for service-dependent prompts
  - Session management integration for conversation continuity
  - Error handling and retry patterns maintained

* **Testing Infrastructure:**
  - Provider mock implementations for unit testing
  - Integration test environment with real provider access
  - Performance testing framework with baseline measurements
  - Quality validation tools for semantic similarity testing

## 7. Success Metrics & Monitoring

### 7.1 Migration Success Criteria

* **Functional Equivalence**: 95%+ similarity in output quality and business logic
* **Provider Compatibility**: All prompts work with OpenAI and Anthropic, 6+ work with Venice.AI
* **Performance**: Maintain or improve response times through Chat Completions efficiency
* **Test Coverage**: >95% code coverage for all migrated prompts
* **Zero Breaking Changes**: All existing Cookbook service integrations preserved

### 7.2 Post-Migration Monitoring

* **Quality Metrics**: Continuous monitoring of response quality and consistency
* **Performance Tracking**: Latency, throughput, and resource usage monitoring
* **Provider Health**: Success rates and failover patterns across providers
* **Error Rates**: Function calling failures and schema validation errors
* **Business Impact**: Recipe generation success rates and user satisfaction metrics

---

**🗂️ WORKING DIRECTORY ARTIFACT CREATED:**
- **Filename:** cookbook-prompt-migration.md
- **Purpose:** Comprehensive migration specification for all 8 Cookbook prompts with detailed implementation guidance, testing requirements, and provider compatibility matrix
- **Context for Team:** Technical roadmap for migrating existing prompts to PromptBaseV2 architecture with preservation of business logic and multi-provider support
- **Dependencies:** Builds upon cookbook-prompt-migration-analysis.md working directory analysis and integrates with PromptBaseV2 component specification
- **Next Actions:** Validate specifications follow epic standards and integrate with overall architecture documentation
