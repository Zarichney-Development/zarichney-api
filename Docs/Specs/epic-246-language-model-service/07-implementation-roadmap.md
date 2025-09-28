# Epic #246: Implementation Roadmap - Language Model Service v2

**Last Updated:** 2025-01-25
**Specification Version:** 1.0
**Epic:** #246 LanguageModelService v2 (OpenAI SDK + Venice REST)

> **Parent:** [`Epic #246 Specifications`](./README.md)

## 1. Purpose & Responsibility

* **What it is:** Comprehensive implementation roadmap translating architectural design into actionable development phases with clear deliverables, dependencies, and success criteria for CodeChanger to implement vendor-agnostic Language Model Service v2.

* **Key Objectives:**
  - Define 4-phase implementation approach with incremental delivery and risk mitigation
  - Provide detailed code examples and implementation patterns for each development milestone
  - Establish clear quality gates and completion criteria enabling autonomous CodeChanger execution
  - Enable iterative progress with transitional backward compatibility that is removed at cutover

* **Success Criteria:**
  - CodeChanger can independently execute each phase with clear acceptance criteria
  - Implementation maintains functional parity during development; legacy is removed at cutover
  - Quality gates ensure comprehensive testing and validation at each milestone
  - Roadmap enables seamless coordination with TestEngineer, SecurityAuditor, and other team members

* **Why it exists:** Transforms high-level architectural specifications into concrete development tasks with implementation guidance, code patterns, and quality assurance requirements enabling systematic progression through complex multi-provider architecture transformation.

## 2. Architecture & Implementation Flow

* **High-Level Implementation Strategy:** Four-phase approach starting with foundation interfaces and OpenAI adapter, progressing through SDK migration, Venice REST adapter + routing, and then cutover/cleanup. Each phase builds incrementally with comprehensive testing and validation.

* **Phase Progression Logic:**
  1. **Foundation (Weeks 1-2)**: Core interfaces and OpenAI adapter wrapping existing logic
  2. **Modernization (Weeks 3-4)**: OpenAI SDK v1.5 migration and direct API implementation (Assistants → Chat Completions/Responses per official guide)
  3. **Venice + Routing (Weeks 5-8)**: Venice.AI REST adapter, routing system, health monitoring
  4. **Cutover & Cleanup (Weeks 9-10)**: Migrate tests/calls to v2, switch DI, remove legacy, hardening

* **Implementation Dependencies:**
  ```mermaid
  graph TD
      subgraph "Phase 1: Foundation"
          P1A[Core Interfaces]
          P1B[Provider Adapter Base]
          P1C[OpenAI Adapter Wrapper]
          P1D[Backward Compatibility]
      end

      subgraph "Phase 2: Modernization"
          P2A[OpenAI SDK v1.5]
          P2B[Chat Completions API]
          P2C[Direct Implementation]
          P2D[Session Integration]
      end

      subgraph "Phase 3: Venice + Routing"
          P3A[Venice.AI Provider]
          P3B[Provider Routing]
          P3C[Health Monitoring]
          P3D[Configuration System]
      end

      subgraph "Phase 4: Cutover & Cleanup"
          P4A[DI Cutover]
          P4B[Legacy Removal]
          P4C[Error Handling/Perf Hardening]
      end

      P1A --> P1B
      P1B --> P1C
      P1C --> P1D
      P1D --> P2A
      P2A --> P2B
      P2B --> P2C
      P2C --> P2D
      P2D --> P3A
      P3A --> P3B
      P3B --> P3C
      P3C --> P3D
      P3D --> P4A
      P4A --> P4B
      P4B --> P4C
```

## 3. Phase 1: Foundation Implementation (Weeks 1-2)

### 3.1 Core Interface Definitions

**Implementation Priority 1: Unified Data Transfer Objects**

```csharp
// Location: /Services/AI/Models/UnifiedChatMessage.cs
public record UnifiedChatMessage
{
    public required string Role { get; init; } // "system", "user", "assistant", "tool"
    public required string Content { get; init; }
    public string? Name { get; init; } // For function/tool calling
    public List<ToolCall>? ToolCalls { get; init; }
    public string? ToolCallId { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

// Location: /Services/AI/Models/UnifiedCompletionRequest.cs
public record UnifiedCompletionRequest
{
    public required List<UnifiedChatMessage> Messages { get; init; }
    public CompletionOptions? Options { get; init; }
    public string? ConversationId { get; init; }
    public Dictionary<string, object>? ProviderSpecific { get; init; }
}

// Location: /Services/AI/Models/UnifiedCompletionResponse.cs
public record UnifiedCompletionResponse
{
    public required string Id { get; init; }
    public required string Provider { get; init; }
    public required string Model { get; init; }
    public required UnifiedChatMessage Message { get; init; }
    public required string FinishReason { get; init; }
    public UsageInfo? Usage { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

// Location: /Services/AI/Models/CompletionOptions.cs
public record CompletionOptions
{
    public string? Model { get; init; }
    public int? MaxTokens { get; init; }
    public double? Temperature { get; init; }
    public double? TopP { get; init; }
    public List<ToolDefinition>? Tools { get; init; }
    public ToolChoice? ToolChoice { get; init; }
    public ResponseFormat? ResponseFormat { get; init; }
    public Dictionary<string, object>? ProviderSpecific { get; init; }
}
```

**Implementation Priority 2: Service Interfaces**

```csharp
// Location: /Services/AI/Interfaces/ILanguageModelService.cs
public interface ILanguageModelService
{
    // Core completion methods
    Task<LlmResult<string>> GetCompletionAsync(
        List<UnifiedChatMessage> messages,
        CompletionOptions? options = null,
        string? conversationId = null,
        CancellationToken cancellationToken = default);

    Task<LlmResult<T>> CallToolAsync<T>(
        List<UnifiedChatMessage> messages,
        ToolDefinition toolDefinition,
        CompletionOptions? options = null,
        string? conversationId = null,
        CancellationToken cancellationToken = default);

    // Streaming support (Phase 4)
    IAsyncEnumerable<StreamingResponse> GetStreamingCompletionAsync(
        List<UnifiedChatMessage> messages,
        CompletionOptions? options = null,
        string? conversationId = null,
        CancellationToken cancellationToken = default);

    // Provider management
    Task<bool> IsProviderAvailableAsync(string providerName, CancellationToken cancellationToken = default);
    Task<List<string>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default);
}

// Location: /Services/AI/Interfaces/IProviderAdapter.cs
public interface IProviderAdapter
{
    string ProviderName { get; }
    ProviderCapabilities Capabilities { get; }

    Task<UnifiedCompletionResponse> GetCompletionAsync(
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    // Streaming support (Phase 4)
    IAsyncEnumerable<StreamingChunk> GetStreamingCompletionAsync(
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default);
}

// Location: /Services/AI/Interfaces/IProviderRouter.cs
public interface IProviderRouter
{
    Task<string> SelectProviderAsync(
        UnifiedCompletionRequest request,
        ProviderSelectionOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<string?> GetFallbackProviderAsync(
        string failedProvider,
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default);
}
```

### 3.2 Provider Adapter Base Infrastructure

**Implementation Priority 3: Abstract Base Class**

```csharp
// Location: /Services/AI/Providers/RestProviderAdapterBase.cs
public abstract class RestProviderAdapterBase : IProviderAdapter
{
    protected readonly HttpClient HttpClient;
    protected readonly IProviderConfig Config;
    protected readonly ILogger Logger;
    protected readonly IMessageNormalizer MessageNormalizer;

    public abstract string ProviderName { get; }
    public abstract ProviderCapabilities Capabilities { get; }

    protected RestProviderAdapterBase(
        HttpClient httpClient,
        IProviderConfig config,
        IMessageNormalizer messageNormalizer,
        ILogger logger)
    {
        HttpClient = httpClient;
        Config = config;
        MessageNormalizer = messageNormalizer;
        Logger = logger;
    }

    public async Task<UnifiedCompletionResponse> GetCompletionAsync(
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Starting completion request for provider {Provider}", ProviderName);

            var normalizedRequest = MessageNormalizer.NormalizeRequest(request, ProviderName);
            var providerRequest = await ConvertToProviderRequestAsync(normalizedRequest, cancellationToken);
            var httpResponse = await SendRequestToProviderAsync(providerRequest, cancellationToken);
            var unifiedResponse = await ConvertToUnifiedResponseAsync(httpResponse, normalizedRequest, cancellationToken);

            Logger.LogDebug("Completed request for provider {Provider} with finish reason {FinishReason}",
                ProviderName, unifiedResponse.FinishReason);

            return unifiedResponse;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in completion request for provider {Provider}", ProviderName);
            throw new ProviderException(ProviderName, $"Completion request failed: {ex.Message}", ex);
        }
    }

    public virtual async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var healthCheckRequest = CreateHealthCheckRequest();
            var response = await SendRequestToProviderAsync(healthCheckRequest, cancellationToken);
            return IsHealthCheckSuccessful(response);
        }
        catch
        {
            return false;
        }
    }

    // Abstract methods for provider-specific implementation
    protected abstract Task<object> ConvertToProviderRequestAsync(
        UnifiedCompletionRequest request, CancellationToken cancellationToken);

    protected abstract Task<HttpResponseMessage> SendRequestToProviderAsync(
        object providerRequest, CancellationToken cancellationToken);

    protected abstract Task<UnifiedCompletionResponse> ConvertToUnifiedResponseAsync(
        HttpResponseMessage response, UnifiedCompletionRequest request, CancellationToken cancellationToken);

    protected virtual object CreateHealthCheckRequest() => throw new NotImplementedException();
    protected virtual bool IsHealthCheckSuccessful(HttpResponseMessage response) => response.IsSuccessStatusCode;
}
```

### 3.3 OpenAI Provider Adapter (Wrapping Existing Logic)

**Implementation Priority 4: Compatibility Wrapper**

```csharp
// Location: /Services/AI/Providers/OpenAIProviderAdapter.cs
public class OpenAIProviderAdapter : IProviderAdapter
{
    private readonly LlmService _legacyService; // Temporary composition
    private readonly IMapper _mapper;
    private readonly ILogger<OpenAIProviderAdapter> _logger;

    public string ProviderName => "OpenAI";
    public ProviderCapabilities Capabilities => new()
    {
        SupportsToolCalling = true,
        SupportsStreaming = true,
        SupportsSystemMessages = true,
        MaxTokens = 128000,
        SupportedModels = ["gpt-4o", "gpt-4o-mini", "gpt-4-turbo", "gpt-3.5-turbo"]
    };

    public OpenAIProviderAdapter(
        LlmService legacyService,
        IMapper mapper,
        ILogger<OpenAIProviderAdapter> logger)
    {
        _legacyService = legacyService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UnifiedCompletionResponse> GetCompletionAsync(
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Converting unified request to legacy format for OpenAI");

            // Convert unified request to legacy format
            var legacyMessages = ConvertToLegacyChatMessages(request.Messages);
            var legacyOptions = ConvertToLegacyChatCompletionOptions(request.Options);

            // Use existing service temporarily
            var legacyResult = await _legacyService.GetCompletionContent(
                legacyMessages,
                request.ConversationId,
                legacyOptions,
                cancellationToken);

            // Convert back to unified format
            return new UnifiedCompletionResponse
            {
                Id = Guid.NewGuid().ToString(),
                Provider = ProviderName,
                Model = legacyOptions?.Model ?? "gpt-4o-mini",
                Message = ConvertToUnifiedMessage(legacyResult),
                FinishReason = "stop", // Extract from legacy result if available
                Usage = ExtractUsageFromLegacyResult(legacyResult),
                Metadata = new Dictionary<string, object>
                {
                    ["LegacyAdapterUsed"] = true,
                    ["ConversationId"] = request.ConversationId ?? string.Empty
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OpenAI legacy adapter");
            throw new ProviderException(ProviderName, $"OpenAI request failed: {ex.Message}", ex);
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple health check using existing service
            var testMessages = new List<ChatMessage>
            {
                new() { Role = "user", Content = "test" }
            };

            var result = await _legacyService.GetCompletionContent(
                testMessages,
                cancellationToken: cancellationToken);

            return !string.IsNullOrEmpty(result.Data);
        }
        catch
        {
            return false;
        }
    }

    private List<ChatMessage> ConvertToLegacyChatMessages(List<UnifiedChatMessage> messages)
    {
        return messages.Select(msg => new ChatMessage
        {
            Role = msg.Role,
            Content = msg.Content,
            Name = msg.Name
            // Map tool calls and other properties as needed
        }).ToList();
    }

    private ChatCompletionOptions? ConvertToLegacyChatCompletionOptions(CompletionOptions? options)
    {
        if (options == null) return null;

        return new ChatCompletionOptions
        {
            Model = options.Model,
            MaxTokens = options.MaxTokens,
            Temperature = options.Temperature,
            TopP = options.TopP
            // Map tool definitions and other properties as needed
        };
    }

    private UnifiedChatMessage ConvertToUnifiedMessage(LlmResult<string> legacyResult)
    {
        return new UnifiedChatMessage
        {
            Role = "assistant",
            Content = legacyResult.Data ?? string.Empty,
            Metadata = new Dictionary<string, object>
            {
                ["ConversationId"] = legacyResult.ConversationId ?? string.Empty
            }
        };
    }

    private UsageInfo? ExtractUsageFromLegacyResult(LlmResult<string> legacyResult)
    {
        // Extract usage information if available in legacy result
        // This may require accessing internal properties or metadata
        return null; // Implement based on legacy service capabilities
    }

    public IAsyncEnumerable<StreamingChunk> GetStreamingCompletionAsync(
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Streaming support will be implemented in Phase 4");
    }
}
```

### 3.4 Backward Compatibility Layer

**Implementation Priority 5: Legacy Interface Adapter**

```csharp
// Location: /Services/AI/Compatibility/LegacyLlmServiceAdapter.cs
public class LegacyLlmServiceAdapter : ILlmService
{
    private readonly ILanguageModelService _modernService;
    private readonly ILogger<LegacyLlmServiceAdapter> _logger;

    public LegacyLlmServiceAdapter(
        ILanguageModelService modernService,
        ILogger<LegacyLlmServiceAdapter> logger)
    {
        _modernService = modernService;
        _logger = logger;
    }

    public async Task<LlmResult<string>> GetCompletionContent(
        List<ChatMessage> messages,
        string? conversationId = null,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Legacy adapter: Converting ChatMessage list to unified format");

            // Convert legacy types to unified types
            var unifiedMessages = ConvertToUnifiedMessages(messages);
            var unifiedOptions = ConvertToUnifiedOptions(options);

            // Call modern service
            var result = await _modernService.GetCompletionAsync(
                unifiedMessages,
                unifiedOptions,
                conversationId,
                cancellationToken);

            _logger.LogDebug("Legacy adapter: Successfully converted modern response to legacy format");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in legacy LLM service adapter");
            throw;
        }
    }

    public async Task<LlmResult<T>> CallFunction<T>(
        string systemPrompt,
        string userPrompt,
        FunctionToolDefinition function,
        string? conversationId = null,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Legacy adapter: Converting function call to tool call format");

            var messages = new List<UnifiedChatMessage>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = userPrompt }
            };

            var toolDefinition = ConvertToToolDefinition(function);
            var result = await _modernService.CallToolAsync<T>(
                messages,
                toolDefinition,
                ConvertToUnifiedOptions(options),
                conversationId,
                cancellationToken);

            _logger.LogDebug("Legacy adapter: Successfully executed function call");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in legacy function call adapter");
            throw;
        }
    }

    // Implement all other ILlmService methods with similar conversion patterns
    public async Task<string> CreateAssistant(PromptBase prompt, CancellationToken cancellationToken = default)
    {
        // For backward compatibility, this will need to be mapped to modern patterns
        // Implementation details depend on how assistants are handled in v2
        throw new NotImplementedException("Assistant creation will be handled in Phase 2 migration");
    }

    // Additional conversion methods
    private List<UnifiedChatMessage> ConvertToUnifiedMessages(List<ChatMessage> legacyMessages)
    {
        return legacyMessages.Select(msg => new UnifiedChatMessage
        {
            Role = msg.Role,
            Content = msg.Content,
            Name = msg.Name
        }).ToList();
    }

    private CompletionOptions? ConvertToUnifiedOptions(ChatCompletionOptions? legacyOptions)
    {
        if (legacyOptions == null) return null;

        return new CompletionOptions
        {
            Model = legacyOptions.Model,
            MaxTokens = legacyOptions.MaxTokens,
            Temperature = legacyOptions.Temperature,
            TopP = legacyOptions.TopP
        };
    }

    private ToolDefinition ConvertToToolDefinition(FunctionToolDefinition legacyFunction)
    {
        return new ToolDefinition
        {
            Name = legacyFunction.Name,
            Description = legacyFunction.Description,
            Parameters = legacyFunction.Parameters
        };
    }
}
```

## 4. Phase 1 Quality Gates & Success Criteria

### 4.1 Acceptance Criteria

**Core Interface Validation:**
- [ ] All interfaces compile and support dependency injection registration
- [ ] ILanguageModelService interface provides equivalent functionality to ILlmService
- [ ] Provider adapter pattern enables consistent implementation across providers
- [ ] Configuration classes support validation and binding from appsettings.json

**OpenAI Adapter Validation:**
- [ ] Wrapper produces equivalent results to existing LlmService for identical inputs
- [ ] All existing function calling scenarios work through adapter
- [ ] Error handling preserves existing behavior patterns
- [ ] Performance impact is minimal (< 10% overhead)

**Backward Compatibility Validation:**
- [ ] All existing ILlmService consumers work unchanged
- [ ] Legacy test suite passes with new adapter implementation
- [ ] Session management integration preserved
- [ ] Configuration migration works seamlessly

### 4.2 Testing Requirements

**Unit Testing Coverage (Minimum 95%):**
```csharp
// Example test structure for Phase 1
[TestClass]
public class OpenAIProviderAdapterTests
{
    [TestMethod]
    public async Task GetCompletionAsync_WithValidRequest_ShouldReturnResponse()
    {
        // Arrange - Mock legacy service
        var mockLegacyService = new Mock<LlmService>();
        var expectedLegacyResult = new LlmResult<string> { Data = "test response" };
        mockLegacyService.Setup(s => s.GetCompletionContent(It.IsAny<List<ChatMessage>>(), It.IsAny<string>(), It.IsAny<ChatCompletionOptions>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedLegacyResult);

        var adapter = new OpenAIProviderAdapter(mockLegacyService.Object, mapper, logger);
        var request = CreateTestUnifiedRequest();

        // Act
        var result = await adapter.GetCompletionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("OpenAI", result.Provider);
        Assert.AreEqual("test response", result.Message.Content);
    }

    [TestMethod]
    public async Task GetCompletionAsync_WhenLegacyServiceThrows_ShouldWrapInProviderException()
    {
        // Test error handling conversion
    }
}
```

**Integration Testing:**
- OpenAI adapter integration with actual LlmService
- Backward compatibility adapter with existing controller endpoints
- Configuration binding and validation testing
- Session management integration validation

## 5. Phase 2: OpenAI SDK v1.5 Migration (Weeks 3-4)

### 5.1 Package Update and Direct Implementation

**Implementation Priority 1: SDK Update**

```xml
<!-- Update Package Reference -->
<PackageReference Include="OpenAI" Version="1.5.0" />
```

**Implementation Priority 2: Direct OpenAI Implementation**

```csharp
// Location: /Services/AI/Providers/OpenAIProviderAdapter.cs (Replace wrapper implementation)
public class OpenAIProviderAdapter : RestProviderAdapterBase
{
    private readonly OpenAIClient _client;
    private readonly OpenAIProviderConfig _config;

    public override string ProviderName => "OpenAI";
    public override ProviderCapabilities Capabilities => new()
    {
        SupportsToolCalling = true,
        SupportsStreaming = true,
        SupportsSystemMessages = true,
        MaxTokens = 128000,
        SupportedModels = ["gpt-4o", "gpt-4o-mini", "gpt-4-turbo", "gpt-3.5-turbo"],
        SupportsResponseFormat = true
    };

    public OpenAIProviderAdapter(
        OpenAIClient client,
        OpenAIProviderConfig config,
        HttpClient httpClient,
        IMessageNormalizer messageNormalizer,
        ILogger<OpenAIProviderAdapter> logger)
        : base(httpClient, config, messageNormalizer, logger)
    {
        _client = client;
        _config = config;
    }

    protected override async Task<object> ConvertToProviderRequestAsync(
        UnifiedCompletionRequest request, CancellationToken cancellationToken)
    {
        var chatClient = _client.GetChatClient(request.Options?.Model ?? _config.DefaultModel);
        var openAIMessages = ConvertToOpenAIMessages(request.Messages);
        var options = CreateChatCompletionOptions(request.Options);

        return new { ChatClient = chatClient, Messages = openAIMessages, Options = options };
    }

    protected override async Task<HttpResponseMessage> SendRequestToProviderAsync(
        object providerRequest, CancellationToken cancellationToken)
    {
        // This method signature needs adjustment for OpenAI SDK v1.5
        // The actual implementation will use ChatClient directly
        throw new NotImplementedException("This will be replaced with direct ChatClient usage");
    }

    protected override async Task<UnifiedCompletionResponse> ConvertToUnifiedResponseAsync(
        HttpResponseMessage response, UnifiedCompletionRequest request, CancellationToken cancellationToken)
    {
        // This will be replaced with direct OpenAI response handling
        throw new NotImplementedException("This will be replaced with direct OpenAI response conversion");
    }

    // Direct implementation method
    public override async Task<UnifiedCompletionResponse> GetCompletionAsync(
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var chatClient = _client.GetChatClient(request.Options?.Model ?? _config.DefaultModel);
            var openAIMessages = ConvertToOpenAIMessages(request.Messages);
            var options = CreateChatCompletionOptions(request.Options);

            Logger.LogDebug("Sending request to OpenAI Chat Completions API with model {Model}",
                request.Options?.Model ?? _config.DefaultModel);

            var response = await chatClient.CompleteChatAsync(openAIMessages, options, cancellationToken);

            return ConvertToUnifiedResponse(response, request);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OpenAI Chat Completions request");
            throw new ProviderException(ProviderName, $"OpenAI request failed: {ex.Message}", ex);
        }
    }

    private List<ChatMessage> ConvertToOpenAIMessages(List<UnifiedChatMessage> messages)
    {
        return messages.Select(msg => msg.Role switch
        {
            "system" => ChatMessage.CreateSystemMessage(msg.Content),
            "user" => ChatMessage.CreateUserMessage(msg.Content),
            "assistant" => CreateAssistantMessage(msg),
            "tool" => ChatMessage.CreateToolMessage(msg.ToolCallId!, msg.Content),
            _ => throw new ArgumentException($"Unknown message role: {msg.Role}")
        }).ToList();
    }

    private ChatMessage CreateAssistantMessage(UnifiedChatMessage msg)
    {
        if (msg.ToolCalls != null && msg.ToolCalls.Any())
        {
            var toolCalls = msg.ToolCalls.Select(tc =>
                ChatToolCall.CreateFunctionToolCall(tc.Id, tc.Function.Name, tc.Function.Arguments)).ToList();

            return ChatMessage.CreateAssistantMessage(toolCalls, msg.Content);
        }

        return ChatMessage.CreateAssistantMessage(msg.Content);
    }

    private ChatCompletionOptions CreateChatCompletionOptions(CompletionOptions? options)
    {
        var chatOptions = new ChatCompletionOptions();

        if (options == null) return chatOptions;

        if (options.MaxTokens.HasValue)
            chatOptions.MaxTokens = options.MaxTokens.Value;

        if (options.Temperature.HasValue)
            chatOptions.Temperature = (float)options.Temperature.Value;

        if (options.TopP.HasValue)
            chatOptions.TopP = (float)options.TopP.Value;

        if (options.Tools != null && options.Tools.Any())
        {
            chatOptions.Tools.AddRange(ConvertToOpenAITools(options.Tools));
        }

        if (options.ResponseFormat != null)
        {
            chatOptions.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                options.ResponseFormat.Name,
                BinaryData.FromString(options.ResponseFormat.Schema.RootElement.GetRawText()));
        }

        return chatOptions;
    }

    private List<ChatTool> ConvertToOpenAITools(List<ToolDefinition> tools)
    {
        return tools.Select(tool => ChatTool.CreateFunctionTool(
            functionName: tool.Name,
            functionDescription: tool.Description,
            functionParameters: BinaryData.FromString(tool.Parameters.RootElement.GetRawText()),
            strict: tool.Strict ?? false
        )).ToList();
    }

    private UnifiedCompletionResponse ConvertToUnifiedResponse(
        ChatCompletion response, UnifiedCompletionRequest request)
    {
        var choice = response.Choices.First();
        var message = choice.Message;

        var unifiedMessage = new UnifiedChatMessage
        {
            Role = "assistant",
            Content = message.Content?.FirstOrDefault()?.Text ?? string.Empty,
            ToolCalls = message.ToolCalls?.Select(tc => new ToolCall
            {
                Id = tc.Id,
                Type = "function",
                Function = new ToolFunction
                {
                    Name = tc.FunctionName,
                    Arguments = tc.FunctionArguments
                }
            }).ToList()
        };

        return new UnifiedCompletionResponse
        {
            Id = response.Id,
            Provider = ProviderName,
            Model = response.Model,
            Message = unifiedMessage,
            FinishReason = choice.FinishReason.ToString().ToLowerInvariant(),
            Usage = response.Usage != null ? new UsageInfo
            {
                PromptTokens = response.Usage.InputTokens,
                CompletionTokens = response.Usage.OutputTokens,
                TotalTokens = response.Usage.TotalTokens
            } : null,
            Metadata = new Dictionary<string, object>
            {
                ["SystemFingerprint"] = response.SystemFingerprint ?? string.Empty,
                ["CreatedAt"] = response.CreatedAt
            }
        };
    }
}
```

## 6. Phase 3: Additional Provider Implementation (Weeks 5-8)

### 6.1 Anthropic Claude Provider

**Implementation Priority 1: Claude Adapter**

```csharp
// Location: /Services/AI/Providers/AnthropicProviderAdapter.cs
public class AnthropicProviderAdapter : RestProviderAdapterBase
{
    private readonly AnthropicProviderConfig _config;

    public override string ProviderName => "Anthropic";
    public override ProviderCapabilities Capabilities => new()
    {
        SupportsToolCalling = true,
        SupportsStreaming = true,
        SupportsSystemMessages = true,
        MaxTokens = 200000, // Claude 3.5 Sonnet
        SupportedModels = ["claude-3-5-sonnet-20241022", "claude-3-opus-20240229", "claude-3-haiku-20240307"],
        RequiresSystemMessageHoisting = true
    };

    protected override async Task<object> ConvertToProviderRequestAsync(
        UnifiedCompletionRequest request, CancellationToken cancellationToken)
    {
        var (systemMessage, conversationMessages) = ExtractSystemMessage(request.Messages);

        var claudeRequest = new ClaudeRequest
        {
            Model = request.Options?.Model ?? _config.DefaultModel,
            MaxTokens = request.Options?.MaxTokens ?? 4096,
            Temperature = NormalizeTemperature(request.Options?.Temperature), // 0-2 -> 0-1
            System = systemMessage,
            Messages = ConvertToClaudeMessages(conversationMessages),
            Tools = request.Options?.Tools != null ? ConvertToClaudeTools(request.Options.Tools) : null
        };

        return claudeRequest;
    }

    private (string? systemMessage, List<UnifiedChatMessage> conversationMessages) ExtractSystemMessage(
        List<UnifiedChatMessage> messages)
    {
        var systemMessages = messages.Where(m => m.Role == "system").ToList();
        var conversationMessages = messages.Where(m => m.Role != "system").ToList();

        var systemMessage = systemMessages.Any()
            ? string.Join("\n\n", systemMessages.Select(m => m.Content))
            : null;

        return (systemMessage, conversationMessages);
    }

    private double? NormalizeTemperature(double? temperature)
    {
        // Convert OpenAI temperature range (0-2) to Claude range (0-1)
        return temperature.HasValue ? Math.Min(temperature.Value / 2.0, 1.0) : null;
    }

    private List<ClaudeMessage> ConvertToClaudeMessages(List<UnifiedChatMessage> messages)
    {
        return messages.Select(msg => new ClaudeMessage
        {
            Role = msg.Role == "assistant" ? "assistant" : "user",
            Content = CreateClaudeContent(msg)
        }).ToList();
    }

    private ClaudeContent CreateClaudeContent(UnifiedChatMessage message)
    {
        if (message.ToolCalls != null && message.ToolCalls.Any())
        {
            // Claude handles tool calls differently - create appropriate content blocks
            return new ClaudeContent
            {
                Type = "text",
                Text = message.Content,
                ToolUse = message.ToolCalls.Select(tc => new ClaudeToolUse
                {
                    Id = tc.Id,
                    Name = tc.Function.Name,
                    Input = JsonSerializer.Deserialize<Dictionary<string, object>>(tc.Function.Arguments)
                }).ToList()
            };
        }

        return new ClaudeContent
        {
            Type = "text",
            Text = message.Content
        };
    }

    private List<ClaudeTool> ConvertToClaudeTools(List<ToolDefinition> tools)
    {
        return tools.Select(tool => new ClaudeTool
        {
            Name = tool.Name,
            Description = tool.Description,
            InputSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(
                tool.Parameters.RootElement.GetRawText())
        }).ToList();
    }
}
```

### 6.1.1 Assistants → Responses Migration Checklist (OpenAI v1.5)

Official guide: https://platform.openai.com/docs/assistants/migration

- Inventory and remove Assistants primitives
  - [ ] Search repo for `Assistants`, `Threads`, `Runs`, `code_interpreter`, `file_search`
  - [ ] Replace flows with Chat Completions messages and tool calls
- Normalize messages
  - [ ] Use roles `system`, `user`, `assistant`, `tool`
  - [ ] Link tool invocations with `tool_call_id` and reply via `role: tool`
- Function/tool definitions
  - [ ] Provide `tools` with JSON schemas (strict where supported)
  - [ ] Map provider output tool calls to unified `ToolCall` in response
- Session continuity
  - [ ] Persist conversation as message history; replay on each call as needed
- Streaming (optional in this epic)
  - [ ] Defer until after parity achieved; Responses/Chat deltas differ from Assistants
- Interim model selection during migration
  - [ ] For smoke tests, pin `gpt-4o-mini` (or GPT‑4 family); GPT‑5 default will fail lingering Assistants paths
  - [ ] After migration, restore configured default model

Assistants → Chat/Responses mapping

| Assistants concept | Chat/Responses equivalent | Notes |
| --- | --- | --- |
| Thread + Run | Single Chat Completions request (messages[]) | Persist messages for continuity |
| code_interpreter/file_search tools | Omit; use external services or tool functions | Out of scope for this epic |
| Function tool call | tools[] (type=function) + tool_calls in response | Use JSON schema, strict where supported |
| Tool output message | role=tool with tool_call_id | Adapter links tool_call_id round‑trip |
| Run steps/events | Not applicable | Use standard streaming or polling as needed later |

Example (C#) – replace Assistants flow with Chat Completions + tools

```csharp
// Build messages
var messages = new List<ChatMessage>
{
    ChatMessage.CreateSystemMessage("You are a helpful assistant."),
    ChatMessage.CreateUserMessage("Get current weather for Paris")
};

// Define tool (function)
var tools = new List<ChatTool>
{
    ChatTool.CreateFunctionTool(
        functionName: "get_weather",
        functionDescription: "Return current weather for a location",
        functionParameters: BinaryData.FromString(@"{
            \"type\": \"object\",
            \"properties\": { \"location\": { \"type\": \"string\" } },
            \"required\": [\"location\"]
        }"),
        strict: true)
};

var options = new ChatCompletionOptions
{
    MaxTokens = 300,
    Temperature = 0.2f
};
options.Tools.AddRange(tools);

// Call Chat Completions
var response = await _openAIClient.Chat.Completions.CreateAsync(
    new ChatCompletionRequest
    {
        Model = "gpt-4o-mini", // pin during migration if needed
        Messages = messages,
        Tools = tools
    },
    cancellationToken);

var choice = response.Choices.First();
var toolCalls = choice.Message.ToolCalls;

if (toolCalls?.Any() == true)
{
    // Extract first tool call
    var tc = toolCalls.First();
    var args = tc.Function.Arguments; // JSON string

    // Execute tool externally and post result back as role=tool
    var resultJson = await WeatherApi.GetAsync(args);
    messages.Add(ChatMessage.CreateToolMessage(tc.Id, resultJson));

    // Ask model to produce final answer using tool result
    var followUp = await _openAIClient.Chat.Completions.CreateAsync(
        new ChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = messages
        },
        cancellationToken);

    // Convert final response to unified format...
}
else
{
    // Handle assistant text response directly
}
```

Checklist outcome
- All Assistants references removed
- Chat/Responses calls produce same or better results
- Interim GPT‑4 smoke tests pass; default restored after migration

### 6.2 Provider Routing System

**Implementation Priority 2: Intelligent Router**

```csharp
// Location: /Services/AI/Routing/ProviderRouter.cs
public class ProviderRouter : IProviderRouter
{
    private readonly IProviderAdapterFactory _adapterFactory;
    private readonly LanguageModelServiceConfig _config;
    private readonly IProviderHealthService _healthService;
    private readonly ILogger<ProviderRouter> _logger;

    public async Task<string> SelectProviderAsync(
        UnifiedCompletionRequest request,
        ProviderSelectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var availableProviders = await GetAvailableProvidersAsync(cancellationToken);
        var strategy = options?.Strategy ?? _config.DefaultRoutingStrategy;

        _logger.LogDebug("Selecting provider using strategy {Strategy} from {ProviderCount} available providers",
            strategy, availableProviders.Count);

        return strategy switch
        {
            RoutingStrategy.Priority => await SelectByPriorityAsync(availableProviders, request),
            RoutingStrategy.Capability => await SelectByCapabilityAsync(availableProviders, request),
            RoutingStrategy.LeastBusy => await SelectByLoadAsync(availableProviders),
            RoutingStrategy.CostOptimal => await SelectByCostAsync(availableProviders, request),
            _ => await SelectByPriorityAsync(availableProviders, request)
        };
    }

    private async Task<string> SelectByCapabilityAsync(
        List<string> availableProviders, UnifiedCompletionRequest request)
    {
        var requiredCapabilities = AnalyzeRequiredCapabilities(request);

        foreach (var provider in availableProviders.OrderBy(p => _config.Providers[p].Priority))
        {
            var adapter = _adapterFactory.CreateProvider(provider);
            if (HasRequiredCapabilities(adapter.Capabilities, requiredCapabilities))
            {
                _logger.LogDebug("Selected provider {Provider} based on capability requirements", provider);
                return provider;
            }
        }

        throw new NoSuitableProviderException($"No provider available with required capabilities: {string.Join(", ", requiredCapabilities)}");
    }

    private RequiredCapabilities AnalyzeRequiredCapabilities(UnifiedCompletionRequest request)
    {
        return new RequiredCapabilities
        {
            ToolCalling = request.Options?.Tools != null && request.Options.Tools.Any(),
            Streaming = false, // Will be determined by method called
            LargeContext = request.Messages.Sum(m => m.Content.Length) > 50000,
            ResponseFormat = request.Options?.ResponseFormat != null
        };
    }

    private bool HasRequiredCapabilities(ProviderCapabilities capabilities, RequiredCapabilities required)
    {
        return (!required.ToolCalling || capabilities.SupportsToolCalling) &&
               (!required.Streaming || capabilities.SupportsStreaming) &&
               (!required.ResponseFormat || capabilities.SupportsResponseFormat) &&
               (!required.LargeContext || capabilities.MaxTokens >= 100000);
    }

    public async Task<string?> GetFallbackProviderAsync(
        string failedProvider,
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Provider {Provider} failed, seeking fallback", failedProvider);

        var availableProviders = await GetAvailableProvidersAsync(cancellationToken);
        var alternatives = availableProviders.Where(p => p != failedProvider).ToList();

        if (!alternatives.Any())
        {
            _logger.LogError("No fallback providers available for failed provider {Provider}", failedProvider);
            return null;
        }

        // Select best alternative based on capabilities
        var fallback = await SelectByCapabilityAsync(alternatives, request);
        _logger.LogInformation("Selected fallback provider {FallbackProvider} for failed provider {FailedProvider}",
            fallback, failedProvider);

        return fallback;
    }

    public async Task<List<string>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default)
    {
        var enabledProviders = _config.Providers
            .Where(p => p.Value.Enabled)
            .Select(p => p.Key)
            .ToList();

        var healthyProviders = new List<string>();

        foreach (var provider in enabledProviders)
        {
            var isHealthy = await _healthService.IsProviderHealthyAsync(provider, cancellationToken);
            if (isHealthy)
            {
                healthyProviders.Add(provider);
            }
            else
            {
                _logger.LogWarning("Provider {Provider} is not healthy, excluding from selection", provider);
            }
        }

        return healthyProviders;
    }
}
```

## 7. Phase 4: Enhanced Features (Weeks 9-12)

### 7.1 Streaming Response Support

**Implementation Priority 1: Streaming Interface**

```csharp
// Location: /Services/AI/Streaming/StreamingLanguageModelService.cs
public class StreamingLanguageModelService : ILanguageModelService
{
    private readonly IProviderRouter _router;
    private readonly IProviderAdapterFactory _adapterFactory;
    private readonly ILogger<StreamingLanguageModelService> _logger;

    public async IAsyncEnumerable<StreamingResponse> GetStreamingCompletionAsync(
        List<UnifiedChatMessage> messages,
        CompletionOptions? options = null,
        string? conversationId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = CreateUnifiedRequest(messages, options, conversationId);
        var provider = await _router.SelectProviderAsync(request, cancellationToken: cancellationToken);
        var adapter = _adapterFactory.CreateProvider(provider);

        if (!adapter.Capabilities.SupportsStreaming)
        {
            throw new ProviderException(provider, "Provider does not support streaming responses");
        }

        _logger.LogDebug("Starting streaming completion with provider {Provider}", provider);

        var streamingStartTime = DateTime.UtcNow;
        var tokenCount = 0;

        try
        {
            await foreach (var chunk in adapter.GetStreamingCompletionAsync(request, cancellationToken))
            {
                tokenCount++;

                yield return new StreamingResponse
                {
                    Id = chunk.Id,
                    Provider = provider,
                    Model = chunk.Model,
                    Delta = chunk.Delta,
                    FinishReason = chunk.FinishReason,
                    Usage = chunk.Usage,
                    Metadata = new Dictionary<string, object>
                    {
                        ["TokenIndex"] = tokenCount,
                        ["ElapsedTime"] = DateTime.UtcNow - streamingStartTime,
                        ["ConversationId"] = conversationId ?? string.Empty
                    }
                };

                if (!string.IsNullOrEmpty(chunk.FinishReason))
                {
                    _logger.LogDebug("Streaming completion finished for provider {Provider} with reason {FinishReason} after {TokenCount} tokens",
                        provider, chunk.FinishReason, tokenCount);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during streaming completion with provider {Provider}", provider);
            throw new ProviderException(provider, $"Streaming completion failed: {ex.Message}", ex);
        }
    }
}
```

### 7.2 Advanced Error Handling and Retry Policies

**Implementation Priority 2: Resilient Service**

```csharp
// Location: /Services/AI/Resilience/ResilientLanguageModelService.cs
public class ResilientLanguageModelService : ILanguageModelService
{
    private readonly ILanguageModelService _innerService;
    private readonly IProviderRouter _router;
    private readonly ResiliencePolicyConfig _resilienceConfig;
    private readonly ILogger<ResilientLanguageModelService> _logger;

    public async Task<LlmResult<string>> GetCompletionAsync(
        List<UnifiedChatMessage> messages,
        CompletionOptions? options = null,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateUnifiedRequest(messages, options, conversationId);
        var provider = await _router.SelectProviderAsync(request, cancellationToken: cancellationToken);

        try
        {
            return await ExecuteWithRetryAndFallbackAsync(provider, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "All retry and fallback attempts failed for request");
            throw;
        }
    }

    private async Task<LlmResult<string>> ExecuteWithRetryAndFallbackAsync(
        string provider,
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var retryPolicy = CreateRetryPolicy(provider);
        var circuitBreakerPolicy = CreateCircuitBreakerPolicy(provider);
        var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

        try
        {
            return await combinedPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var result = await _innerService.GetCompletionAsync(
                        request.Messages, request.Options, request.ConversationId, cancellationToken);

                    _logger.LogDebug("Request successful with provider {Provider}", provider);
                    return result;
                }
                catch (ProviderException ex) when (ShouldRetryWithFallback(ex))
                {
                    _logger.LogWarning("Provider {Provider} failed with retryable error: {Error}", provider, ex.Message);
                    throw;
                }
                catch (ProviderException ex) when (ex is RateLimitExceededException rateLimitEx)
                {
                    _logger.LogWarning("Rate limit exceeded for provider {Provider}, retry after {RetryAfter}",
                        provider, rateLimitEx.RetryAfter);

                    if (rateLimitEx.RetryAfter.HasValue)
                    {
                        await Task.Delay(rateLimitEx.RetryAfter.Value, cancellationToken);
                    }
                    throw;
                }
            });
        }
        catch (ProviderException ex) when (ShouldTryFallback(ex))
        {
            _logger.LogWarning("Provider {Provider} failed, attempting fallback", provider);
            return await TryFallbackProviderAsync(provider, request, cancellationToken);
        }
    }

    private async Task<LlmResult<string>> TryFallbackProviderAsync(
        string failedProvider,
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var fallbackProvider = await _router.GetFallbackProviderAsync(failedProvider, request, cancellationToken);

        if (fallbackProvider == null)
        {
            throw new NoSuitableProviderException("No fallback providers available");
        }

        _logger.LogInformation("Retrying request with fallback provider {FallbackProvider}", fallbackProvider);

        // Attempt with fallback provider (with limited retries)
        var fallbackRetryPolicy = CreateLimitedRetryPolicy(fallbackProvider);

        return await fallbackRetryPolicy.ExecuteAsync(async () =>
        {
            return await _innerService.GetCompletionAsync(
                request.Messages, request.Options, request.ConversationId, cancellationToken);
        });
    }

    private IAsyncPolicy CreateRetryPolicy(string provider)
    {
        var config = _resilienceConfig.Providers.GetValueOrDefault(provider, _resilienceConfig.Default);

        return Policy
            .Handle<ProviderException>(ex => ShouldRetry(ex))
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: config.MaxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} for provider {Provider} after {Delay}ms due to {Exception}",
                        retryCount, provider, timespan.TotalMilliseconds, outcome.Exception?.Message);
                });
    }

    private IAsyncPolicy CreateCircuitBreakerPolicy(string provider)
    {
        var config = _resilienceConfig.Providers.GetValueOrDefault(provider, _resilienceConfig.Default);

        return Policy
            .Handle<ProviderException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: config.CircuitBreakerThreshold,
                durationOfBreak: TimeSpan.FromMinutes(config.CircuitBreakerDurationMinutes),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError("Circuit breaker opened for provider {Provider} for {Duration}ms due to {Exception}",
                        provider, duration.TotalMilliseconds, exception.Message);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset for provider {Provider}", provider);
                });
    }

    private bool ShouldRetry(ProviderException ex)
    {
        return ex is not (AuthenticationException or InvalidRequestException or QuotaExceededException);
    }

    private bool ShouldRetryWithFallback(ProviderException ex)
    {
        return ex is (TimeoutException or ServiceUnavailableException or RateLimitExceededException);
    }

    private bool ShouldTryFallback(ProviderException ex)
    {
        return ex is not (AuthenticationException or InvalidRequestException);
    }
}
```

## 8. Implementation Guidelines & Best Practices

### 8.1 Code Quality Standards

**Coding Standards Compliance:**
- Use primary constructors for dependency injection where appropriate
- Implement interfaces for all major components to enable testing and flexibility
- Follow async/await patterns consistently with proper CancellationToken propagation
- Use record types for immutable data structures (DTOs, configuration classes)
- Implement comprehensive validation and structured error handling

**Dependency Injection Registration Pattern:**
```csharp
// Location: /Services/AI/Extensions/ServiceCollectionExtensions.cs
public static class LanguageModelServiceExtensions
{
    public static IServiceCollection AddLanguageModelServiceV2(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration binding
        services.Configure<LanguageModelServiceConfig>(
            configuration.GetSection("LanguageModelService"));

        // Core service interfaces
        services.AddSingleton<IProviderRouter, ProviderRouter>();
        services.AddSingleton<IProviderAdapterFactory, ProviderAdapterFactory>();
        services.AddSingleton<IMessageNormalizer, UniversalMessageNormalizer>();
        services.AddSingleton<IProviderHealthService, ProviderHealthService>();

        // Provider-specific HTTP clients
        services.AddHttpClient<OpenAIProviderAdapter>("openai-client", client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<AnthropicProviderAdapter>("anthropic-client", client =>
        {
            client.BaseAddress = new Uri("https://api.anthropic.com");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        // Provider adapters
        services.AddTransient<OpenAIProviderAdapter>();
        services.AddTransient<AnthropicProviderAdapter>();
        services.AddTransient<VeniceAIProviderAdapter>();

        // Main service with feature flags and configuration
        if (IsFeatureEnabled(configuration, "LanguageModelServiceV2"))
        {
            if (IsFeatureEnabled(configuration, "ResilientService"))
            {
                services.AddScoped<ILanguageModelService>(provider =>
                {
                    var innerService = ActivatorUtilities.CreateInstance<LanguageModelService>(provider);
                    return new ResilientLanguageModelService(innerService,
                        provider.GetRequiredService<IProviderRouter>(),
                        provider.GetRequiredService<IOptions<ResiliencePolicyConfig>>().Value,
                        provider.GetRequiredService<ILogger<ResilientLanguageModelService>>());
                });
            }
            else
            {
                services.AddScoped<ILanguageModelService, LanguageModelService>();
            }
        }

        // Backward compatibility
        if (ShouldEnableLegacySupport(configuration))
        {
            services.AddScoped<ILlmService, LegacyLlmServiceAdapter>();
        }

        return services;
    }

    private static bool IsFeatureEnabled(IConfiguration configuration, string featureName)
    {
        return configuration.GetValue<bool>($"FeatureFlags:{featureName}");
    }

    private static bool ShouldEnableLegacySupport(IConfiguration configuration)
    {
        return configuration.GetValue<bool>("LanguageModelService:BackwardCompatibility:EnableLegacyInterface", true);
    }
}
```

### 8.2 Testing Strategy Integration

**Test-Driven Development Approach:**
- Write interfaces first, then implementations with comprehensive unit tests
- Create integration tests for each provider adapter with mock external services
- Implement contract tests ensuring cross-provider consistency and compatibility
- Add end-to-end tests validating complete flows through the service

**Provider Contract Testing Pattern:**
```csharp
// Location: /Tests/Services/AI/Providers/ProviderContractTests.cs
[TestClass]
public class ProviderContractTests
{
    [TestMethod]
    [DataRow("OpenAI")]
    [DataRow("Anthropic")]
    [DataRow("Venice.AI")]
    public async Task AllProviders_ShouldReturnConsistentResponseFormat(string providerName)
    {
        // Arrange
        var adapter = CreateProviderAdapter(providerName);
        var standardRequest = CreateStandardTestRequest();

        // Act
        var response = await adapter.GetCompletionAsync(standardRequest);

        // Assert
        Assert.IsNotNull(response.Id);
        Assert.AreEqual(providerName, response.Provider);
        Assert.IsNotNull(response.Message);
        Assert.IsNotNull(response.FinishReason);
        // Additional contract validations
    }

    [TestMethod]
    [DataRow("OpenAI")]
    [DataRow("Anthropic")]
    public async Task ProvidersWithToolSupport_ShouldHandleToolCallsConsistently(string providerName)
    {
        // Test tool calling consistency across providers
        var adapter = CreateProviderAdapter(providerName);
        var toolRequest = CreateToolCallTestRequest();

        var response = await adapter.GetCompletionAsync(toolRequest);

        Assert.IsTrue(response.Message.ToolCalls?.Any());
        // Validate tool call format consistency
    }
}
```

## 9. Risk Mitigation & Quality Assurance

### 9.1 Phase-by-Phase Risk Management

**Phase 1 Risks & Mitigation:**
- **Risk:** Backward compatibility breaks existing functionality
- **Mitigation:** Comprehensive test suite validation, wrapper approach for OpenAI adapter
- **Quality Gate:** All existing ILlmService tests pass with new implementation

**Phase 2 Risks & Mitigation:**
- **Risk:** OpenAI SDK v1.5 introduces breaking changes or performance regression
- **Mitigation:** Side-by-side testing, performance benchmarking, gradual rollout with feature flags
- **Quality Gate:** Performance within 110% of baseline, all functionality preserved

**Phase 3 Risks & Mitigation:**
- **Risk:** Provider integration complexity and API differences cause instability
- **Mitigation:** Incremental provider addition, comprehensive error handling, health checking
- **Quality Gate:** Each provider independently validated before next provider addition

**Phase 4 Risks & Mitigation:**
- **Risk:** Advanced features introduce complexity that impacts reliability
- **Mitigation:** Feature flags for each enhancement, comprehensive monitoring, graceful degradation
- **Quality Gate:** Production readiness validation, load testing, monitoring baseline establishment

### 9.2 Success Metrics & Validation

**Functional Success Criteria:**
- [ ] **Phase 1:** All existing ILlmService functionality preserved with equivalent results
- [ ] **Phase 2:** OpenAI SDK v1.5 migration complete with performance improvements
- [ ] **Phase 3:** Multiple providers operational with intelligent routing and failover
- [ ] **Phase 4:** Advanced features operational with production-grade reliability

**Quality Metrics:**
- **Test Coverage:** Unit tests ≥95%, Integration tests ≥90%, Contract tests 100% for all providers
- **Performance:** Response time within 110% of current implementation, no memory usage increase
- **Reliability:** Provider availability ≥99.9%, failover time <1 second, graceful error handling

**Operational Metrics:**
- **Provider Health:** Continuous monitoring with automatic circuit breaker activation
- **Configuration Management:** Runtime provider enable/disable without service restart
- **Observability:** Comprehensive logging, metrics, and alerting for production operations

## 10. Team Coordination & Next Steps

### 10.1 CodeChanger Immediate Actions

**Week 1 Priority Tasks:**
1. **Environment Setup:** Create feature branch `feature/epic-246-language-model-service-v2`
2. **Foundation Review:** Study all working directory artifacts and architectural specifications
3. **Phase 1 Implementation:** Begin core interface definitions and DTOs
4. **Testing Framework:** Set up testing infrastructure for new components

### 10.2 Cross-Team Coordination Requirements

**TestEngineer Coordination:**
- Share interface definitions for early test development and validation strategy alignment
- Coordinate on mock framework patterns and integration testing approaches
- Align on testing scenarios supporting 90% coverage goals for Epic progression

**SecurityAuditor Coordination:**
- Review multi-provider security patterns and authentication handling approaches
- Validate error handling prevents sensitive data exposure in logs and responses
- Ensure provider API key management follows security best practices

**WorkflowEngineer Coordination:**
- Plan feature flag implementation strategy and CI/CD pipeline integration
- Coordinate deployment strategy with gradual rollout and rollback procedures
- Plan monitoring and alerting infrastructure for multi-provider operations

### 10.3 Epic Completion Quality Gates

**Before Phase 2 Advancement:**
- [ ] All Phase 1 unit and integration tests passing with ≥95% coverage
- [ ] Backward compatibility validated through existing test suite execution
- [ ] Code review completed with architectural compliance verification
- [ ] Performance baseline established for comparison with OpenAI SDK v1.5

**Before Phase 3 Advancement:**
- [ ] OpenAI SDK migration validated with no functional regression
- [ ] Performance improvements documented and verified
- [ ] Direct API implementation tested thoroughly with session management integration
- [ ] Migration testing completed with legacy service comparison validation

**Before Epic Completion:**
- [ ] Multiple providers operational with routing and health checking functional
- [ ] Configuration system validated across all deployment environments
- [ ] Load testing completed with production-scale validation
- [ ] Documentation updated and team training completed for production deployment

This implementation roadmap provides comprehensive guidance for systematic development of the Language Model Service v2 architecture while maintaining backward compatibility and ensuring robust testing throughout the transformation process.

---

🗂️ **WORKING DIRECTORY ARTIFACT CREATED:**
- **Filename:** 07-implementation-roadmap.md
- **Purpose:** Comprehensive implementation roadmap transforming architectural design into actionable development phases with code examples and quality gates
- **Context for Team:** Detailed guidance for CodeChanger enabling autonomous implementation with clear coordination points for TestEngineer, SecurityAuditor, and WorkflowEngineer
- **Dependencies:** Builds upon all working directory architectural artifacts and Epic #246 specifications 01-06
- **Next Actions:** Create Phase 1 completion criteria specification and validate cross-references
