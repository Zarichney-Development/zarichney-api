# PromptBase v2 Component Specification

**Last Updated:** 2025-01-27
**Status:** Planning
**Owner:** PromptEngineer

> **Parent:** [`06-prompt-system-modernization`](../06-prompt-system-modernization.md)

## 1. Purpose & Responsibility

* **What it is:** Provider-agnostic prompt base architecture that enables automated schema generation, unified function calling, and multi-provider compatibility while maintaining backward compatibility with existing prompt patterns.

* **Key Objectives:**
  - Provide unified base class for all Cookbook prompts with provider abstraction
  - Enable automatic JSON schema generation from C# response models using attributes
  - Support multi-provider compatibility through IFunctionDefinition abstraction
  - Maintain dependency injection integration for prompts requiring services
  - Preserve existing prompt business logic patterns during migration

* **Success Criteria:**
  - All existing prompts can inherit from PromptBaseV2 without business logic changes
  - Schema generation works for all response model types with proper validation
  - Provider compatibility validated across OpenAI, Anthropic, and Venice.AI
  - Performance equivalent or improved compared to legacy PromptBase
  - Zero breaking changes to existing service integration patterns

* **Why it exists:** Current PromptBase is tightly coupled to OpenAI Assistants API with manual function definition management. PromptBaseV2 abstracts provider specifics while automating schema generation to reduce maintenance overhead and enable multi-provider support.

## 2. Architecture & Key Concepts

* **High-Level Design:** Interface-based architecture with automated schema generation, provider abstraction, and template method pattern. The design separates prompt business logic from provider implementation details while providing consistent patterns for function calling and response handling.

* **Core Implementation Flow:**
  1. Prompt inherits from PromptBaseV2 and implements GetPrimaryFunction()
  2. Response model decorated with JsonRequired and Description attributes
  3. CreateFunction<T>() helper automatically generates IFunctionDefinition from model
  4. PromptService uses IPrompt interface to execute via ILanguageModelService
  5. Provider adapter converts IFunctionDefinition to provider-specific format
  6. Response deserialized to strongly-typed model using JsonSerializer

* **Key Design Patterns:**
  - **Template Method Pattern**: PromptBaseV2 provides common structure, derived classes implement specifics
  - **Builder Pattern**: CreateFunction<T>() helper builds IFunctionDefinition with automatic schema generation
  - **Interface Segregation**: IPrompt interface provides minimal contract for prompt execution
  - **Dependency Injection**: Constructor injection preserved for prompts requiring services

* **Component Diagram:**
  ```mermaid
  graph TB
      subgraph "Interface Layer"
          IPROMPT[IPrompt Interface]
          IFUNC_DEF[IFunctionDefinition Interface]
      end

      subgraph "Base Implementation"
          PROMPT_BASE[PromptBaseV2 Abstract Class]
          CREATE_FUNC[CreateFunction Helper]
          MODEL_CONFIG[PromptModelConfig]
      end

      subgraph "Schema Generation"
          JSON_GEN[JsonSchemaGenerator]
          JSON_SCHEMA[JsonSchema]
          SCHEMA_PROP[JsonSchemaProperty]
      end

      subgraph "Response Models"
          ATTRIBUTES[JsonRequired, Description]
          RECIPE_ANALYSIS[RecipeAnalysis]
          RECIPE_RESPONSE[SynthesizedRecipeResponse]
      end

      subgraph "Concrete Prompts"
          ANALYZE_PROMPT[AnalyzeRecipePrompt]
          SYNTHESIZE_PROMPT[SynthesizeRecipePrompt]
          QUERY_PROMPT[GetAlternativeQueryPrompt]
      end

      IPROMPT --> PROMPT_BASE
      PROMPT_BASE --> IFUNC_DEF
      PROMPT_BASE --> CREATE_FUNC
      PROMPT_BASE --> MODEL_CONFIG

      CREATE_FUNC --> JSON_GEN
      JSON_GEN --> JSON_SCHEMA
      JSON_SCHEMA --> SCHEMA_PROP

      ATTRIBUTES --> JSON_GEN
      RECIPE_ANALYSIS --> ATTRIBUTES
      RECIPE_RESPONSE --> ATTRIBUTES

      ANALYZE_PROMPT --> PROMPT_BASE
      SYNTHESIZE_PROMPT --> PROMPT_BASE
      QUERY_PROMPT --> PROMPT_BASE

      ANALYZE_PROMPT --> RECIPE_ANALYSIS
      SYNTHESIZE_PROMPT --> RECIPE_RESPONSE
  ```

## 3. Interface Contract & Assumptions

* **Core Interfaces:**

### IPrompt Interface
```csharp
public interface IPrompt
{
    string Name { get; }
    string Description { get; }
    string SystemPrompt { get; }
    PromptModelConfig ModelConfig { get; }
    IEnumerable<IFunctionDefinition> GetFunctions();
    PromptCapabilityRequirements GetCapabilityRequirements();
}
```

### IFunctionDefinition Interface
```csharp
public interface IFunctionDefinition
{
    string Name { get; }
    string Description { get; }
    JsonSchema ParameterSchema { get; }
    bool StrictValidation { get; }
    Dictionary<string, object> ProviderMetadata { get; }
}
```

### PromptBaseV2 Abstract Class
```csharp
public abstract class PromptBaseV2 : IPrompt
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string SystemPrompt { get; }

    public virtual PromptModelConfig ModelConfig => new()
    {
        PreferredProvider = "OpenAI",
        PreferredModel = "gpt-4o-mini",
        FallbackProviders = new[] { "Venice.AI" },
        Temperature = 0.1,
        MaxTokens = 4000
    };

    public virtual IEnumerable<IFunctionDefinition> GetFunctions()
    {
        yield return GetPrimaryFunction();
    }

    protected abstract IFunctionDefinition GetPrimaryFunction();

    public virtual PromptCapabilityRequirements GetCapabilityRequirements() => new()
    {
        RequiresFunctionCalling = true,
        RequiresStrictMode = true,
        RequiresConversationHistory = false,
        MinimumContextWindow = 8000
    };

    protected static IFunctionDefinition CreateFunction<T>(
        string name,
        string description,
        bool strictValidation = true)
    {
        return new FunctionDefinition
        {
            Name = name,
            Description = description,
            ParameterSchema = JsonSchemaGenerator.Generate<T>(),
            StrictValidation = strictValidation,
            ProviderMetadata = new Dictionary<string, object>()
        };
    }
}
```

* **Critical Assumptions:**
  - Response models can be enhanced with attributes without breaking existing serialization
  - Reflection-based schema generation provides acceptable performance with caching
  - Provider adapters handle IFunctionDefinition conversion correctly
  - Existing dependency injection patterns can be preserved during migration

## 4. Implementation Details

### JsonSchemaGenerator Implementation

```csharp
public static class JsonSchemaGenerator
{
    private static readonly ConcurrentDictionary<Type, JsonSchema> _schemaCache = new();

    public static JsonSchema Generate<T>() => Generate(typeof(T));

    public static JsonSchema Generate(Type type)
    {
        return _schemaCache.GetOrAdd(type, GenerateInternal);
    }

    private static JsonSchema GenerateInternal(Type type)
    {
        var schema = new JsonSchema
        {
            Type = "object",
            AdditionalProperties = false
        };

        var properties = new Dictionary<string, JsonSchemaProperty>();
        var required = new List<string>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propertySchema = GeneratePropertySchema(property);
            var propertyName = GetJsonPropertyName(property);

            properties[propertyName] = propertySchema;

            if (IsRequired(property))
            {
                required.Add(propertyName);
            }
        }

        schema.Properties = properties;
        schema.Required = required.ToArray();

        ValidateSchema(schema, type);
        return schema;
    }

    private static JsonSchemaProperty GeneratePropertySchema(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        var schema = new JsonSchemaProperty
        {
            Description = GetDescription(property)
        };

        if (underlyingType == typeof(string))
        {
            schema.Type = "string";
            AddStringConstraints(schema, property);
        }
        else if (underlyingType == typeof(int) || underlyingType == typeof(long))
        {
            schema.Type = "integer";
            AddNumericConstraints(schema, property);
        }
        else if (underlyingType == typeof(double) || underlyingType == typeof(float) || underlyingType == typeof(decimal))
        {
            schema.Type = "number";
            AddNumericConstraints(schema, property);
        }
        else if (underlyingType == typeof(bool))
        {
            schema.Type = "boolean";
        }
        else if (underlyingType.IsEnum)
        {
            schema.Type = "string";
            schema.Enum = Enum.GetNames(underlyingType)
                .Select(name => JsonNamingPolicy.CamelCase.ConvertName(name))
                .Cast<object>()
                .ToArray();
        }
        else if (IsArrayType(underlyingType))
        {
            schema.Type = "array";
            var elementType = GetArrayElementType(underlyingType);
            schema.Items = GeneratePropertySchemaForType(elementType);
        }
        else
        {
            schema.Type = "object";
        }

        return schema;
    }

    private static string GetJsonPropertyName(PropertyInfo property)
    {
        var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        if (jsonPropertyAttribute != null)
        {
            return jsonPropertyAttribute.Name;
        }
        return JsonNamingPolicy.CamelCase.ConvertName(property.Name);
    }

    private static bool IsRequired(PropertyInfo property)
    {
        if (property.GetCustomAttribute<JsonRequiredAttribute>() != null)
        {
            return true;
        }
        var propertyType = property.PropertyType;
        return !IsNullable(propertyType);
    }

    private static string? GetDescription(PropertyInfo property)
    {
        var descriptionAttribute = property.GetCustomAttribute<DescriptionAttribute>();
        return descriptionAttribute?.Description;
    }
}
```

### Supporting Classes

```csharp
public class PromptModelConfig
{
    public string PreferredProvider { get; set; } = "OpenAI";
    public string PreferredModel { get; set; } = "gpt-4o-mini";
    public string[] FallbackProviders { get; set; } = Array.Empty<string>();
    public double Temperature { get; set; } = 0.1;
    public int MaxTokens { get; set; } = 4000;
    public Dictionary<string, object> ProviderSpecificSettings { get; set; } = new();
}

public class PromptCapabilityRequirements
{
    public bool RequiresFunctionCalling { get; set; } = true;
    public bool RequiresStrictMode { get; set; } = true;
    public bool RequiresConversationHistory { get; set; } = false;
    public bool RequiresStreaming { get; set; } = false;
    public int MinimumContextWindow { get; set; } = 4000;
    public string[] RequiredCapabilities { get; set; } = Array.Empty<string>();
}

public class JsonSchema
{
    public string Type { get; set; } = "object";
    public Dictionary<string, JsonSchemaProperty> Properties { get; set; } = new();
    public string[] Required { get; set; } = Array.Empty<string>();
    public bool AdditionalProperties { get; set; } = false;

    public string ToJson() => JsonSerializer.Serialize(this, JsonSchemaSerializerOptions);

    public static JsonSchema FromJson(string json) =>
        JsonSerializer.Deserialize<JsonSchema>(json, JsonSchemaSerializerOptions)
        ?? throw new InvalidOperationException("Failed to deserialize JSON schema");

    private static readonly JsonSerializerOptions JsonSchemaSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

public class JsonSchemaProperty
{
    public string Type { get; set; } = "string";
    public string? Description { get; set; }
    public JsonSchemaProperty? Items { get; set; }
    public object? Default { get; set; }
    public object[]? Enum { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public int? Minimum { get; set; }
    public int? Maximum { get; set; }
    public string? Pattern { get; set; }
}
```

## 5. Migration Examples

### Simple Prompt Migration (GetAlternativeQueryPrompt)

**Before (Current Implementation):**
```csharp
public class GetAlternativeQueryPrompt : PromptBase
{
    public override string Model => LlmModels.Gpt4Omini;

    public override FunctionDefinition GetFunction() => new()
    {
        Name = "GetAlternativeQuery",
        Description = "Generate an alternative search query",
        Strict = true,
        Parameters = JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                newQuery = new
                {
                    type = "string",
                    description = "An alternative search query"
                }
            },
            required = new[] { "newQuery" }
        })
    };
}
```

**After (Migrated Implementation):**
```csharp
public class GetAlternativeQueryPrompt : PromptBaseV2
{
    public override string Name => "Alternative Query Generator";
    public override string Description => "Generate alternative search queries for improved recipe discovery";
    public override string SystemPrompt => """
        You are a search optimization assistant. When given a recipe search query,
        generate an alternative query that might yield better or different results.
        Focus on synonyms, related cooking terms, and different ways to describe the same dish.
        """;

    protected override IFunctionDefinition GetPrimaryFunction()
    {
        return CreateFunction<AlternativeQueryResult>(
            "GetAlternativeQuery",
            "Generate an alternative search query for improved recipe discovery"
        );
    }
}

public class AlternativeQueryResult
{
    [JsonRequired]
    [Description("An alternative search query optimized for recipe discovery")]
    [JsonPropertyName("newQuery")]
    public string? NewQuery { get; set; }
}
```

### Complex Prompt Migration (SynthesizeRecipePrompt)

**Before (Current Implementation):**
```csharp
public class SynthesizeRecipePrompt(IMapper mapper) : PromptBase
{
    private readonly IMapper _mapper = mapper;

    public override string Model => LlmModels.Gpt4Omini;

    public override FunctionDefinition GetFunction() => new()
    {
        Name = "SynthesizeRecipe",
        Description = "Synthesize a personalized recipe",
        Strict = true,
        Parameters = JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                title = new { type = "string", description = "The requested recipe name." },
                description = new { type = "string", description = "A preface to the given recipe." },
                // ... many more properties
            },
            required = new[] { "title", "description", "servings", /* ... */ }
        })
    };
}
```

**After (Migrated Implementation):**
```csharp
public class SynthesizeRecipePrompt(IMapper mapper) : PromptBaseV2
{
    private readonly IMapper _mapper = mapper;

    public override string Name => "Recipe Synthesizer";
    public override string Description => "Synthesize personalized recipes from existing recipes and user preferences";
    public override string SystemPrompt => """
        You are an expert chef and recipe developer. Your role is to synthesize new, personalized recipes
        based on existing recipe data and specific user preferences from cookbook orders.
        // ... existing system prompt content
        """;

    public override PromptModelConfig ModelConfig => new()
    {
        PreferredProvider = "OpenAI",
        PreferredModel = "gpt-4o-mini",
        FallbackProviders = new[] { "Anthropic" },
        Temperature = 0.1,
        MaxTokens = 6000  // Higher for complex recipe generation
    };

    protected override IFunctionDefinition GetPrimaryFunction()
    {
        return CreateFunction<SynthesizedRecipeResponse>(
            "SynthesizeRecipe",
            "Synthesize a personalized recipe using existing recipes and user's cookbook order"
        );
    }

    public string GetUserPrompt(string recipeName, List<Recipe> recipes, CookbookOrder order) =>
        $"""
        Cookbook Order:
        ```md
        {order.ToMarkdown()}
        ```

        Recipe data:
        ```json
        {JsonSerializer.Serialize(_mapper.Map<List<ScrapedRecipe>>(recipes))}
        ```

        Please synthesize a personalized recipe for '{recipeName}'. Thank you.
        """;
}

public class SynthesizedRecipeResponse
{
    [JsonRequired]
    [Description("The requested recipe name")]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonRequired]
    [Description("A preface to the given recipe")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonRequired]
    [JsonPropertyName("servings")]
    public string? Servings { get; set; }

    [JsonRequired]
    [JsonPropertyName("prepTime")]
    public string? PrepTime { get; set; }

    [JsonRequired]
    [JsonPropertyName("cookTime")]
    public string? CookTime { get; set; }

    [JsonRequired]
    [JsonPropertyName("totalTime")]
    public string? TotalTime { get; set; }

    [JsonRequired]
    [Description("List of recipe ingredients")]
    [JsonPropertyName("ingredients")]
    public List<string> Ingredients { get; set; } = new();

    [JsonRequired]
    [Description("Step-by-step cooking directions")]
    [JsonPropertyName("directions")]
    public List<string> Directions { get; set; } = new();

    [JsonRequired]
    [Description("URLs that inspired this recipe")]
    [JsonPropertyName("inspiredBy")]
    public List<string> InspiredBy { get; set; } = new();

    [JsonRequired]
    [Description("Additional notes and tips")]
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
```

## 6. Testing Strategy

### Unit Tests for Base Classes
```csharp
[TestFixture]
public class PromptBaseV2Tests
{
    [Test]
    public void CreateFunction_ShouldGenerateValidFunctionDefinition()
    {
        // Arrange
        var prompt = new TestPrompt();

        // Act
        var functions = prompt.GetFunctions().ToList();

        // Assert
        Assert.AreEqual(1, functions.Count);
        var function = functions.First();
        Assert.AreEqual("TestFunction", function.Name);
        Assert.IsTrue(function.StrictValidation);
        Assert.IsNotNull(function.ParameterSchema);
    }

    [Test]
    public void ModelConfig_ShouldProvideDefaults()
    {
        // Arrange
        var prompt = new TestPrompt();

        // Act
        var config = prompt.ModelConfig;

        // Assert
        Assert.AreEqual("OpenAI", config.PreferredProvider);
        Assert.AreEqual("gpt-4o-mini", config.PreferredModel);
        Assert.AreEqual(0.1, config.Temperature, 0.001);
    }

    private class TestPrompt : PromptBaseV2
    {
        public override string Name => "Test Prompt";
        public override string Description => "Test prompt description";
        public override string SystemPrompt => "Test system prompt";

        protected override IFunctionDefinition GetPrimaryFunction()
        {
            return CreateFunction<TestResponse>("TestFunction", "Test function description");
        }
    }

    private class TestResponse
    {
        [JsonRequired]
        public string? TestProperty { get; set; }
    }
}
```

### JsonSchemaGenerator Tests
```csharp
[TestFixture]
public class JsonSchemaGeneratorTests
{
    [Test]
    public void Generate_ShouldCreateValidSchema_ForSimpleModel()
    {
        // Act
        var schema = JsonSchemaGenerator.Generate<AlternativeQueryResult>();

        // Assert
        Assert.AreEqual("object", schema.Type);
        Assert.IsFalse(schema.AdditionalProperties);
        Assert.IsTrue(schema.Properties.ContainsKey("newQuery"));
        Assert.Contains("newQuery", schema.Required);
    }

    [Test]
    public void Generate_ShouldHandleArrayProperties()
    {
        // Act
        var schema = JsonSchemaGenerator.Generate<SynthesizedRecipeResponse>();

        // Assert
        Assert.IsTrue(schema.Properties.ContainsKey("ingredients"));
        Assert.AreEqual("array", schema.Properties["ingredients"].Type);
        Assert.IsNotNull(schema.Properties["ingredients"].Items);
        Assert.AreEqual("string", schema.Properties["ingredients"].Items.Type);
    }

    [Test]
    public void Generate_ShouldCacheResults()
    {
        // Act
        var schema1 = JsonSchemaGenerator.Generate<AlternativeQueryResult>();
        var schema2 = JsonSchemaGenerator.Generate<AlternativeQueryResult>();

        // Assert
        Assert.AreSame(schema1, schema2);
    }
}
```

## 7. Dependencies & Integration

* **Core Dependencies:**
  - System.Text.Json for serialization and attribute processing
  - System.Reflection for type analysis and attribute extraction
  - System.ComponentModel.DataAnnotations for validation attributes
  - Microsoft.Extensions.DependencyInjection for service registration

* **Integration Points:**
  - ILanguageModelService v2 for provider routing and execution
  - Provider adapters for function definition conversion
  - Existing Cookbook services for business logic integration
  - Dependency injection container for service resolution

* **Service Registration:**
```csharp
// In Program.cs or DI configuration
services.AddScoped<IPromptService, PromptService>();

// Register all migrated prompts
services.AddScoped<AnalyzeRecipePrompt>();
services.AddScoped<SynthesizeRecipePrompt>();
services.AddScoped<GetAlternativeQueryPrompt>();
// ... register remaining prompts

// Register JsonSchemaGenerator as singleton for caching
services.AddSingleton<JsonSchemaGenerator>();
```

## 8. Known Issues & Future Enhancements

* **Current Limitations:**
  - Reflection-based schema generation may have performance impact for complex models
  - Attribute requirements may necessitate updates to existing response models
  - Provider-specific optimizations limited by unified interface design
  - Caching strategy for generated schemas needs optimization for memory usage

* **Future Enhancements:**
  - Source generators for compile-time schema generation to improve performance
  - Advanced validation attributes for custom schema constraints
  - Dynamic schema optimization based on provider capabilities
  - Prompt versioning system for A/B testing different prompt configurations
  - Integration with System.Text.Json source generators for optimal serialization

---

**🗂️ WORKING DIRECTORY ARTIFACT CREATED:**
- **Filename:** prompt-base-v2.md
- **Purpose:** Detailed component specification for PromptBaseV2 architecture including interfaces, implementation details, and migration examples
- **Context for Team:** Technical specification for implementing provider-agnostic prompt base class with automated schema generation
- **Dependencies:** Builds upon prompt-base-architecture-v2.md working directory artifact and integrates with LanguageModelService v2
- **Next Actions:** Create cookbook prompt migration component specification with individual prompt migration details
