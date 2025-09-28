# Provider Adapters Component Specification

**Last Updated:** 2025-01-25
**Component Type:** Multi-Provider Integration Layer
**Implementation Priority:** High - Core Functionality

> **Parent:** [`Components`](./README.md) | [`Epic #246`](../README.md)

## Purpose & Responsibility

Provider Adapters implement the IProviderAdapter interface for each AI provider. In Epic 246 we implement two providers: OpenAI (SDK-based) and Venice.AI (REST-based). This document also defines the standards future adapters (e.g., Anthropic, Gemini, xAI, DeepSeek) will follow.

## 1. Base Adapter Architecture

### 1.1 RestProviderAdapterBase Implementation

```csharp
/// <summary>
/// Base class for REST-based provider adapters providing common functionality.
/// </summary>
public abstract class RestProviderAdapterBase : IProviderAdapter, IDisposable
{
    protected readonly HttpClient HttpClient;
    protected readonly IProviderConfig Config;
    protected readonly ILogger Logger;
    protected readonly IMessageNormalizer MessageNormalizer;
    protected readonly JsonSerializerOptions JsonOptions;
    private readonly SemaphoreSlim _requestSemaphore;
    private bool _disposed = false;

    public abstract string ProviderName { get; }
    public abstract ProviderCapabilities Capabilities { get; }

    protected RestProviderAdapterBase(
        HttpClient httpClient,
        IProviderConfig config,
        IMessageNormalizer messageNormalizer,
        ILogger logger)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Config = config ?? throw new ArgumentNullException(nameof(config));
        MessageNormalizer = messageNormalizer ?? throw new ArgumentNullException(nameof(messageNormalizer));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure HTTP client
        ConfigureHttpClient();

        // Limit concurrent requests per provider
        _requestSemaphore = new SemaphoreSlim(config.MaxConcurrentRequests ?? 10);

        // Configure JSON serialization
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Template method for provider completion requests.
    /// </summary>
    public async Task<UnifiedCompletionResponse> GetCompletionAsync(
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        await _requestSemaphore.WaitAsync(cancellationToken);

        try
        {
            using var activity = CreateActivity("GetCompletion");
            var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString();

            activity?.SetTag("provider.name", ProviderName);
            activity?.SetTag("correlation.id", correlationId);
            activity?.SetTag("model", request.Options?.Model);

            Logger.LogDebug("Starting {Provider} completion request {CorrelationId}",
                ProviderName, correlationId);

            // Validate request against provider capabilities
            ValidateRequest(request);

            // Convert to provider-specific format
            var providerRequest = ConvertToProviderRequest(request);

            // Execute HTTP request with monitoring
            var stopwatch = Stopwatch.StartNew();
            var httpResponse = await SendRequestToProviderAsync(providerRequest, cancellationToken);
            stopwatch.Stop();

            // Convert response to unified format
            var unifiedResponse = ConvertToUnifiedResponse(httpResponse, request);

            // Record metrics
            RecordRequestMetrics(stopwatch.Elapsed, success: true);

            Logger.LogDebug("Completed {Provider} request {CorrelationId} in {Duration}ms",
                ProviderName, correlationId, stopwatch.ElapsedMilliseconds);

            return unifiedResponse;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Provider {Provider} request failed", ProviderName);
            RecordRequestMetrics(TimeSpan.Zero, success: false);

            throw MapToProviderException(ex);
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    /// <summary>
    /// Template method for streaming completions.
    /// </summary>
    public async IAsyncEnumerable<UnifiedStreamingChunk> GetStreamingCompletionAsync(
        UnifiedCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!Capabilities.SupportsStreaming)
        {
            throw new ProviderCapabilityException(ProviderName,
                "Provider does not support streaming responses");
        }

        await _requestSemaphore.WaitAsync(cancellationToken);

        try
        {
            var providerRequest = ConvertToProviderRequest(request);
            providerRequest = EnableStreaming(providerRequest);

            await foreach (var chunk in StreamResponseAsync(providerRequest, cancellationToken))
            {
                yield return ConvertToUnifiedStreamingChunk(chunk);
            }
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    // Abstract methods each provider must implement
    protected abstract object ConvertToProviderRequest(UnifiedCompletionRequest request);
    protected abstract UnifiedCompletionResponse ConvertToUnifiedResponse(object response, UnifiedCompletionRequest request);
    protected abstract HttpRequestMessage CreateHttpRequest(object providerRequest);
    protected abstract void ValidateRequest(UnifiedCompletionRequest request);
    protected abstract ProviderException MapToProviderException(Exception exception);

    // Virtual methods for optional features
    protected virtual object EnableStreaming(object providerRequest) => providerRequest;
    protected virtual async IAsyncEnumerable<object> StreamResponseAsync(object request, CancellationToken cancellationToken)
    {
        yield break; // Default: no streaming support
    }
    protected virtual UnifiedStreamingChunk ConvertToUnifiedStreamingChunk(object chunk) =>
        throw new NotSupportedException("Streaming not implemented");

    /// <summary>
    /// Execute HTTP request with proper error handling.
    /// </summary>
    protected async Task<object> SendRequestToProviderAsync(
        object providerRequest,
        CancellationToken cancellationToken)
    {
        var httpRequest = CreateHttpRequest(providerRequest);

        using var response = await HttpClient.SendAsync(httpRequest, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw CreateHttpException(response.StatusCode, content);
        }

        return DeserializeResponse(content);
    }

    protected virtual void ConfigureHttpClient()
    {
        HttpClient.Timeout = TimeSpan.FromSeconds(Config.TimeoutSeconds);
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Zarichney-API/2.0");

        // Provider-specific headers configured in derived classes
    }

    protected virtual object DeserializeResponse(string content)
    {
        return JsonSerializer.Deserialize<object>(content, JsonOptions) ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    protected virtual Exception CreateHttpException(HttpStatusCode statusCode, string content)
    {
        return statusCode switch
        {
            HttpStatusCode.TooManyRequests => new RateLimitExceededException(ProviderName, content),
            HttpStatusCode.Unauthorized => new ProviderAuthenticationException(ProviderName, "Invalid API key"),
            HttpStatusCode.BadRequest => new ProviderValidationException(ProviderName, content),
            HttpStatusCode.InternalServerError => new ProviderException(ProviderName, content),
            _ => new ProviderException(ProviderName, $"HTTP {(int)statusCode}: {content}")
        };
    }
}
```

### 1.2 SdkProviderAdapterBase Implementation

```csharp
/// <summary>
/// Base class for SDK-based provider adapters (e.g., OpenAI SDK).
/// </summary>
public abstract class SdkProviderAdapterBase : IProviderAdapter
{
    protected readonly ILogger Logger;
    protected readonly IMessageNormalizer MessageNormalizer;

    public abstract string ProviderName { get; }
    public abstract ProviderCapabilities Capabilities { get; }

    protected SdkProviderAdapterBase(ILogger logger, IMessageNormalizer messageNormalizer)
    {
        Logger = logger;
        MessageNormalizer = messageNormalizer;
    }

    public abstract Task<UnifiedCompletionResponse> GetCompletionAsync(
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default);

    public virtual IAsyncEnumerable<UnifiedStreamingChunk> GetStreamingCompletionAsync(
        UnifiedCompletionRequest request,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Streaming not implemented");

    public virtual Task<List<string>> GetAvailableModelsAsync() => Task.FromResult(new List<string>());
    public virtual Task<ConfigurationValidationResult> ValidateConfigurationAsync() =>
        Task.FromResult(ConfigurationValidationResult.Success(ProviderName, "Validated"));
    public virtual Task<bool> IsAvailableAsync() => Task.FromResult(true);
}
```

## 2. OpenAI Provider Adapter

### 2.1 OpenAI Implementation with SDK v1.5

```csharp
/// <summary>
/// OpenAI provider adapter supporting SDK v1.5 Chat Completions API.
/// </summary>
public class OpenAIProviderAdapter : SdkProviderAdapterBase
{
    private readonly OpenAIClient _openAIClient;
    private readonly OpenAIProviderConfig _openAIConfig;

    public override string ProviderName => "OpenAI";

    public override ProviderCapabilities Capabilities => new()
    {
        SupportsToolCalling = true,
        SupportsStreaming = true,
        SupportsStrictMode = true,
        SupportedRoles = new List<string> { "system", "user", "assistant", "function" },
        MaxTokens = 128000,
        MaxMessages = 1000,
        AvailableModels = _openAIConfig.AvailableModels,
        TemperatureRange = (0.0, 2.0),
        Features = new Dictionary<string, bool>
        {
            ["ResponseFormats"] = true,
            ["FunctionCallingStrict"] = true,
            ["BatchRequests"] = _openAIConfig.EnableBatching,
            ["AssistantsAPI"] = false // Deprecated in v2
        }
    };

    public OpenAIProviderAdapter(
        HttpClient httpClient,
        IOptions<OpenAIProviderConfig> config,
        IMessageNormalizer messageNormalizer,
        ILogger<OpenAIProviderAdapter> logger)
        : base(httpClient, config.Value, messageNormalizer, logger)
    {
        _openAIConfig = config.Value;

        // Initialize OpenAI client with v1.5 SDK
        _openAIClient = new OpenAIClient(_openAIConfig.ApiKey);
    }

    protected override object ConvertToProviderRequest(UnifiedCompletionRequest request)
    {
        var openAIRequest = new
        {
            model = request.Options?.Model ?? _openAIConfig.DefaultModel,
            messages = ConvertMessages(request.Messages),
            temperature = request.Options?.Temperature,
            max_tokens = request.Options?.MaxTokens,
            top_p = request.Options?.TopP,
            stop = request.Options?.Stop?.ToArray(),
            stream = request.Options?.Stream ?? false,
            tools = ConvertTools(request.Options?.Tools),
            tool_choice = ConvertToolChoice(request.Options?.ToolChoice)
        };

        Logger.LogDebug("Converted request to OpenAI format: model={Model}, messages={MessageCount}, tools={ToolCount}",
            openAIRequest.model, request.Messages.Count, request.Options?.Tools?.Count ?? 0);

        return openAIRequest;
    }

    private object[] ConvertMessages(List<UnifiedChatMessage> messages)
    {
        return messages.Select(msg => new
        {
            role = ConvertRole(msg.Role),
            content = msg.Content,
            tool_calls = ConvertToolCalls(msg.ToolCalls),
            tool_call_id = msg.ToolCallId
        }).ToArray();
    }

    private string ConvertRole(string unifiedRole)
    {
        return unifiedRole switch
        {
            MessageRoles.Tool => "function",     // OpenAI uses "function" role
            MessageRoles.Function => "function", // Direct mapping
            _ => unifiedRole.ToLowerInvariant()
        };
    }

    private object[]? ConvertTools(List<ToolDefinition>? tools)
    {
        if (tools == null || !tools.Any()) return null;

        return tools.Select(tool => new
        {
            type = "function",
            function = new
            {
                name = tool.Name,
                description = tool.Description,
                parameters = JsonSerializer.Deserialize<JsonElement>(tool.Parameters.RootElement.GetRawText()),
                strict = _openAIConfig.StrictFunctionCalling && tool.Strict
            }
        }).ToArray();
    }

    private object[]? ConvertToolCalls(List<ToolCall>? toolCalls)
    {
        if (toolCalls == null || !toolCalls.Any()) return null;

        return toolCalls.Select(tc => new
        {
            id = tc.Id,
            type = "function",
            function = new
            {
                name = tc.Function.Name,
                arguments = tc.Function.Arguments
            }
        }).ToArray();
    }

    protected override UnifiedCompletionResponse ConvertToUnifiedResponse(
        object response,
        UnifiedCompletionRequest request)
    {
        var openAIResponse = JsonSerializer.Deserialize<OpenAICompletionResponse>(
            JsonSerializer.Serialize(response, JsonOptions), JsonOptions)!;

        var choice = openAIResponse.Choices.First();
        var message = choice.Message;

        return new UnifiedCompletionResponse
        {
            Id = openAIResponse.Id,
            Provider = ProviderName,
            Model = openAIResponse.Model,
            Message = new UnifiedChatMessage
            {
                Role = MessageRoles.Assistant,
                Content = message.Content,
                ToolCalls = ConvertFromOpenAIToolCalls(message.ToolCalls),
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(openAIResponse.Created)
            },
            FinishReason = choice.FinishReason,
            Usage = new TokenUsage
            {
                PromptTokens = openAIResponse.Usage.PromptTokens,
                CompletionTokens = openAIResponse.Usage.CompletionTokens,
                TotalTokens = openAIResponse.Usage.TotalTokens
            },
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(openAIResponse.Created),
            Metadata = new Dictionary<string, object>
            {
                ["openai_id"] = openAIResponse.Id,
                ["system_fingerprint"] = openAIResponse.SystemFingerprint ?? string.Empty
            }
        };
    }

    protected override HttpRequestMessage CreateHttpRequest(object providerRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAIConfig.ApiKey);

        if (!string.IsNullOrEmpty(_openAIConfig.Organization))
        {
            request.Headers.Add("OpenAI-Organization", _openAIConfig.Organization);
        }

        var json = JsonSerializer.Serialize(providerRequest, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return request;
    }

    protected override void ValidateRequest(UnifiedCompletionRequest request)
    {
        if (request.Messages.Count == 0)
        {
            throw new ProviderValidationException(ProviderName, "At least one message is required");
        }

        if (request.Options?.MaxTokens > Capabilities.MaxTokens)
        {
            throw new ProviderValidationException(ProviderName,
                $"Max tokens {request.Options.MaxTokens} exceeds provider limit {Capabilities.MaxTokens}");
        }

        // Validate model availability
        var requestedModel = request.Options?.Model ?? _openAIConfig.DefaultModel;
        if (!Capabilities.AvailableModels.Contains(requestedModel))
        {
            throw new ProviderValidationException(ProviderName,
                $"Model {requestedModel} is not available for this provider");
        }
    }

    protected override ProviderException MapToProviderException(Exception exception)
    {
        return exception switch
        {
            HttpRequestException httpEx when httpEx.Message.Contains("429") =>
                new RateLimitExceededException(ProviderName, "OpenAI rate limit exceeded"),

            HttpRequestException httpEx when httpEx.Message.Contains("401") =>
                new ProviderAuthenticationException(ProviderName, "Invalid OpenAI API key"),

            JsonException jsonEx =>
                new ProviderResponseException(ProviderName, "Invalid JSON response from OpenAI", jsonEx),

            TaskCanceledException =>
                new ProviderTimeoutException(ProviderName, "OpenAI request timed out"),

            _ => new ProviderException(ProviderName, exception.Message, exception)
        };
    }

    public override async Task<bool> IsAvailableAsync()
    {
        try
        {
            var healthRequest = new
            {
                model = _openAIConfig.DefaultModel,
                messages = new[] { new { role = "user", content = "test" } },
                max_tokens = 1
            };

            var httpRequest = CreateHttpRequest(healthRequest);
            using var response = await HttpClient.SendAsync(httpRequest);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
```

## 3. Anthropic Provider Adapter

### 3.1 Claude API Integration

```csharp
/// <summary>
/// Anthropic provider adapter for Claude models with tool use capabilities.
/// </summary>
public class AnthropicProviderAdapter : RestProviderAdapterBase
{
    private readonly AnthropicProviderConfig _anthropicConfig;

    public override string ProviderName => "Anthropic";

    public override ProviderCapabilities Capabilities => new()
    {
        SupportsToolCalling = true,
        SupportsStreaming = true,
        SupportsStrictMode = false, // Claude doesn't support strict mode
        SupportedRoles = new List<string> { "system", "user", "assistant", "tool" },
        MaxTokens = 200000,
        MaxMessages = 500,
        AvailableModels = _anthropicConfig.AvailableModels,
        TemperatureRange = (0.0, 1.0),
        Features = new Dictionary<string, bool>
        {
            ["LongContext"] = true,
            ["MultiModal"] = true,
            ["ToolUse"] = true,
            ["SystemMessages"] = true
        }
    };

    public AnthropicProviderAdapter(
        HttpClient httpClient,
        IOptions<AnthropicProviderConfig> config,
        IMessageNormalizer messageNormalizer,
        ILogger<AnthropicProviderAdapter> logger)
        : base(httpClient, config.Value, messageNormalizer, logger)
    {
        _anthropicConfig = config.Value;
    }

    protected override object ConvertToProviderRequest(UnifiedCompletionRequest request)
    {
        var (systemMessage, conversationMessages) = ExtractSystemMessage(request.Messages);

        var anthropicRequest = new
        {
            model = request.Options?.Model ?? _anthropicConfig.DefaultModel,
            system = systemMessage?.Content,
            messages = ConvertMessages(conversationMessages),
            max_tokens = Math.Min(request.Options?.MaxTokens ?? 4096, Capabilities.MaxTokens),
            temperature = request.Options?.Temperature,
            top_p = request.Options?.TopP,
            stream = request.Options?.Stream ?? false,
            tools = ConvertAnthropicTools(request.Options?.Tools)
        };

        Logger.LogDebug("Converted request to Anthropic format: model={Model}, system={HasSystem}, messages={MessageCount}",
            anthropicRequest.model, !string.IsNullOrEmpty(anthropicRequest.system), conversationMessages.Count);

        return anthropicRequest;
    }

    private (UnifiedChatMessage?, List<UnifiedChatMessage>) ExtractSystemMessage(
        List<UnifiedChatMessage> messages)
    {
        var systemMessage = messages.FirstOrDefault(m => m.Role == MessageRoles.System);
        var conversationMessages = messages.Where(m => m.Role != MessageRoles.System).ToList();
        return (systemMessage, conversationMessages);
    }

    private object[] ConvertMessages(List<UnifiedChatMessage> messages)
    {
        return messages.Select(msg => new
        {
            role = ConvertRole(msg.Role),
            content = BuildAnthropicContent(msg)
        }).ToArray();
    }

    private string ConvertRole(string unifiedRole)
    {
        return unifiedRole switch
        {
            MessageRoles.Function => "tool",     // Map legacy function to tool
            MessageRoles.Tool => "tool",         // Direct mapping
            _ => unifiedRole.ToLowerInvariant()
        };
    }

    private object BuildAnthropicContent(UnifiedChatMessage message)
    {
        if (message.ToolCalls?.Any() == true)
        {
            // Assistant message with tool calls
            return new object[]
            {
                new { type = "text", text = message.Content ?? string.Empty },
                // Add tool calls as separate content blocks
            }.Concat(message.ToolCalls.Select(tc => new
            {
                type = "tool_use",
                id = tc.Id,
                name = tc.Function.Name,
                input = JsonSerializer.Deserialize<JsonElement>(tc.Function.Arguments)
            })).ToArray();
        }

        if (!string.IsNullOrEmpty(message.ToolCallId))
        {
            // Tool response message
            return new[]
            {
                new
                {
                    type = "tool_result",
                    tool_use_id = message.ToolCallId,
                    content = message.Content
                }
            };
        }

        // Regular text message
        return message.Content ?? string.Empty;
    }

    protected override UnifiedCompletionResponse ConvertToUnifiedResponse(
        object response,
        UnifiedCompletionRequest request)
    {
        var anthropicResponse = JsonSerializer.Deserialize<AnthropicCompletionResponse>(
            JsonSerializer.Serialize(response, JsonOptions), JsonOptions)!;

        // Extract text content and tool calls from Claude response
        var textContent = ExtractTextContent(anthropicResponse.Content);
        var toolCalls = ExtractToolCalls(anthropicResponse.Content);

        return new UnifiedCompletionResponse
        {
            Id = anthropicResponse.Id,
            Provider = ProviderName,
            Model = anthropicResponse.Model,
            Message = new UnifiedChatMessage
            {
                Role = MessageRoles.Assistant,
                Content = textContent,
                ToolCalls = toolCalls,
                CreatedAt = DateTimeOffset.UtcNow
            },
            FinishReason = anthropicResponse.StopReason,
            Usage = new TokenUsage
            {
                PromptTokens = anthropicResponse.Usage.InputTokens,
                CompletionTokens = anthropicResponse.Usage.OutputTokens,
                TotalTokens = anthropicResponse.Usage.InputTokens + anthropicResponse.Usage.OutputTokens
            },
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["anthropic_id"] = anthropicResponse.Id,
                ["stop_sequence"] = anthropicResponse.StopSequence ?? string.Empty
            }
        };
    }

    protected override HttpRequestMessage CreateHttpRequest(object providerRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "messages");

        request.Headers.Add("x-api-key", _anthropicConfig.ApiKey);
        request.Headers.Add("anthropic-version", _anthropicConfig.AnthropicVersion);

        if (_anthropicConfig.EnableBeta)
        {
            request.Headers.Add("anthropic-beta", "tools-2024-04-04");
        }

        var json = JsonSerializer.Serialize(providerRequest, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return request;
    }

    protected override void ValidateRequest(UnifiedCompletionRequest request)
    {
        if (request.Messages.Count == 0)
        {
            throw new ProviderValidationException(ProviderName, "At least one message is required");
        }

        // Anthropic requires alternating user/assistant messages after system message
        var conversationMessages = request.Messages.Where(m => m.Role != MessageRoles.System).ToList();

        if (conversationMessages.Any() && conversationMessages.First().Role != MessageRoles.User)
        {
            throw new ProviderValidationException(ProviderName,
                "First conversation message must be from user");
        }

        // Validate max tokens
        var maxTokens = request.Options?.MaxTokens ?? 4096;
        if (maxTokens > Capabilities.MaxTokens)
        {
            throw new ProviderValidationException(ProviderName,
                $"Max tokens {maxTokens} exceeds Claude limit {Capabilities.MaxTokens}");
        }
    }

    public override async Task<bool> IsAvailableAsync()
    {
        try
        {
            var healthRequest = new
            {
                model = _anthropicConfig.DefaultModel,
                messages = new[] { new { role = "user", content = "test" } },
                max_tokens = 1
            };

            var httpRequest = CreateHttpRequest(healthRequest);
            using var response = await HttpClient.SendAsync(httpRequest);

            return response.StatusCode != HttpStatusCode.Unauthorized;
        }
        catch
        {
            return false;
        }
    }
}
```

## 4. Venice.AI Provider Adapter

### 4.1 Llama Model Integration

```csharp
/// <summary>
/// Venice.AI provider adapter for accessing Llama models through Venice platform.
/// </summary>
public class VeniceAIProviderAdapter : RestProviderAdapterBase
{
    private readonly VeniceAIProviderConfig _veniceConfig;

    public override string ProviderName => "Venice.AI";

    public override ProviderCapabilities Capabilities => new()
    {
        SupportsToolCalling = true,
        SupportsStreaming = true,
        SupportsStrictMode = false,
        SupportedRoles = new List<string> { "system", "user", "assistant", "tool" },
        MaxTokens = 32768,
        MaxMessages = 200,
        AvailableModels = _veniceConfig.AvailableModels,
        TemperatureRange = (0.0, 2.0),
        Features = new Dictionary<string, bool>
        {
            ["MultipleModels"] = true,
            ["CostOptimized"] = true,
            ["OpenSource"] = true
        }
    };

    public VeniceAIProviderAdapter(
        HttpClient httpClient,
        IOptions<VeniceAIProviderConfig> config,
        IMessageNormalizer messageNormalizer,
        ILogger<VeniceAIProviderAdapter> logger)
        : base(httpClient, config.Value, messageNormalizer, logger)
    {
        _veniceConfig = config.Value;
    }

    protected override object ConvertToProviderRequest(UnifiedCompletionRequest request)
    {
        // Venice.AI uses OpenAI-compatible format
        var veniceRequest = new
        {
            model = SelectOptimalModel(request),
            messages = ConvertMessages(request.Messages),
            temperature = request.Options?.Temperature,
            max_tokens = request.Options?.MaxTokens,
            top_p = request.Options?.TopP,
            stream = request.Options?.Stream ?? false,
            tools = ConvertTools(request.Options?.Tools)
        };

        Logger.LogDebug("Converted request to Venice.AI format: model={Model}, messages={MessageCount}",
            veniceRequest.model, request.Messages.Count);

        return veniceRequest;
    }

    private string SelectOptimalModel(UnifiedCompletionRequest request)
    {
        var requestedModel = request.Options?.Model;

        if (!string.IsNullOrEmpty(requestedModel) && _veniceConfig.AvailableModels.Contains(requestedModel))
        {
            return requestedModel;
        }

        // Cost optimization: select model based on request complexity
        if (_veniceConfig.EnableCostOptimization)
        {
            var messageLength = request.Messages.Sum(m => m.Content?.Length ?? 0);
            var hasTools = request.Options?.Tools?.Any() == true;

            // Use smaller model for simple requests
            if (messageLength < 1000 && !hasTools && _veniceConfig.AvailableModels.Contains("llama-3.1-8b"))
            {
                Logger.LogDebug("Selected llama-3.1-8b for simple request (length: {Length})", messageLength);
                return "llama-3.1-8b";
            }

            // Use medium model for moderate complexity
            if (messageLength < 5000 && _veniceConfig.AvailableModels.Contains("llama-3.1-70b"))
            {
                Logger.LogDebug("Selected llama-3.1-70b for moderate request (length: {Length})", messageLength);
                return "llama-3.1-70b";
            }
        }

        return _veniceConfig.DefaultModel ?? "llama-3.1-70b";
    }

    protected override HttpRequestMessage CreateHttpRequest(object providerRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _veniceConfig.ApiKey);
        request.Headers.UserAgent.ParseAdd("Zarichney-API/2.0");

        var json = JsonSerializer.Serialize(providerRequest, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return request;
    }

    protected override UnifiedCompletionResponse ConvertToUnifiedResponse(
        object response,
        UnifiedCompletionRequest request)
    {
        // Venice.AI returns OpenAI-compatible format
        var veniceResponse = JsonSerializer.Deserialize<OpenAICompletionResponse>(
            JsonSerializer.Serialize(response, JsonOptions), JsonOptions)!;

        var choice = veniceResponse.Choices.First();
        var message = choice.Message;

        return new UnifiedCompletionResponse
        {
            Id = veniceResponse.Id,
            Provider = ProviderName,
            Model = veniceResponse.Model,
            Message = new UnifiedChatMessage
            {
                Role = MessageRoles.Assistant,
                Content = message.Content,
                ToolCalls = ConvertFromOpenAIToolCalls(message.ToolCalls), // Reuse OpenAI conversion
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(veniceResponse.Created)
            },
            FinishReason = choice.FinishReason,
            Usage = new TokenUsage
            {
                PromptTokens = veniceResponse.Usage.PromptTokens,
                CompletionTokens = veniceResponse.Usage.CompletionTokens,
                TotalTokens = veniceResponse.Usage.TotalTokens
            },
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(veniceResponse.Created),
            Metadata = new Dictionary<string, object>
            {
                ["venice_id"] = veniceResponse.Id,
                ["cost_optimization"] = _veniceConfig.EnableCostOptimization
            }
        };
    }

    public override async Task<bool> IsAvailableAsync()
    {
        try
        {
            var healthRequest = new
            {
                model = _veniceConfig.DefaultModel,
                messages = new[] { new { role = "user", content = "ping" } },
                max_tokens = 1
            };

            var httpRequest = CreateHttpRequest(healthRequest);
            using var response = await HttpClient.SendAsync(httpRequest);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
```

## 5. Provider Adapter Factory

### 5.1 Factory Implementation

```csharp
/// <summary>
/// Factory for creating and managing provider adapters.
/// </summary>
public class ProviderAdapterFactory : IProviderAdapterFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<LanguageModelServiceConfig> _config;
    private readonly ILogger<ProviderAdapterFactory> _logger;
    private readonly ConcurrentDictionary<string, IProviderAdapter> _adapterCache;

    public ProviderAdapterFactory(
        IServiceProvider serviceProvider,
        IOptions<LanguageModelServiceConfig> config,
        ILogger<ProviderAdapterFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
        _adapterCache = new ConcurrentDictionary<string, IProviderAdapter>();
    }

    public IProviderAdapter CreateAdapter(string providerName)
    {
        return _adapterCache.GetOrAdd(providerName, name =>
        {
            if (!_config.Value.Providers.ContainsKey(name))
            {
                throw new ProviderConfigurationException(name,
                    $"Provider {name} is not configured");
            }

            var providerConfig = _config.Value.Providers[name];
            if (!providerConfig.Enabled)
            {
                throw new ProviderUnavailableException(name,
                    $"Provider {name} is disabled");
            }

            var adapter = name switch
            {
                "OpenAI" => _serviceProvider.GetRequiredService<OpenAIProviderAdapter>(),
                "Venice.AI" => _serviceProvider.GetRequiredService<VeniceAIProviderAdapter>(),
                _ => throw new ProviderNotSupportedException(name)
            };

            _logger.LogDebug("Created adapter for provider {Provider}", name);
            return adapter;
        });
    }

    public IEnumerable<IProviderAdapter> GetAllAdapters()
    {
        return _config.Value.Providers
            .Where(p => p.Value.Enabled)
            .Select(p => CreateAdapter(p.Key));
    }

    public bool IsProviderAvailable(string providerName)
    {
        try
        {
            var adapter = CreateAdapter(providerName);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

## Implementation Guidelines

### Provider Integration Standards
- Implement provider-specific error handling and retry logic
- Support provider-specific authentication patterns
- Handle provider-specific rate limiting appropriately
- Maintain provider-specific metrics and monitoring

### Message Format Handling
- Implement comprehensive message format conversion
- Handle provider-specific role mappings correctly
- Support provider-specific tool calling patterns
- Preserve message metadata during conversion

### Performance Optimization
- Use connection pooling for HTTP clients
- Implement request concurrency limits per provider
- Support async streaming where provider capabilities allow
- Cache provider capabilities to avoid repeated discovery

### Testing Requirements
- Unit tests for message format conversion
- Integration tests with real provider APIs
- Contract tests for provider compliance
- Performance tests for concurrent request handling

---

**🗂️ WORKING DIRECTORY ARTIFACT CREATED:**
- **Filename:** components/provider-adapters.md
- **Purpose:** Comprehensive provider adapter implementation specification for all six AI providers
- **Context for Team:** Complete adapter patterns for CodeChanger implementation with provider-specific integration details
- **Dependencies:** Implements IProviderAdapter interface from architecture design and integrates with message normalization
- **Next Actions:** Create configuration-router.md component specification to complete component documentation
