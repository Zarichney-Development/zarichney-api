# Epic #246: Provider Integration Specification

**Last Updated:** 2025-01-25
**Document Type:** Provider Integration Design
**Epic Phase:** Multi-Provider Implementation

> **Parent:** [`Epic #246`](./README.md)

## Purpose & Responsibility

This document defines the provider integration patterns, adapter implementations, routing strategies, and configuration management for Epic 246 scope (OpenAI via SDK and Venice.AI via generic REST). It also establishes standards that future providers (e.g., Anthropic, Gemini, xAI, DeepSeek) will follow in subsequent epics.

## 1. Provider Adapter Patterns

### 1.1 Provider-Specific Adapter Implementations

Each provider requires specific adapter handling due to API differences:

#### OpenAI Provider Adapter (SDK-based)

```csharp
/// <summary>
/// OpenAI provider adapter supporting SDK v1.5 Chat Completions API.
/// </summary>
public class OpenAIProviderAdapter : SdkProviderAdapterBase
{
    public override string ProviderName => "OpenAI";

    public override ProviderCapabilities Capabilities => new()
    {
        SupportsToolCalling = true,
        SupportsStreaming = true,
        SupportsStrictMode = true,
        SupportedRoles = ["system", "user", "assistant", "function"],
        MaxTokens = 128000,
        AvailableModels = ["gpt-4", "gpt-4-turbo", "gpt-3.5-turbo", "gpt-4o"],
        Features = new Dictionary<string, bool>
        {
            ["ResponseFormats"] = true,
            ["FunctionCallingStrict"] = true,
            ["BatchRequests"] = true,
            ["AssistantsAPI"] = true
        }
    };

    // SDK-based implementation uses the official OpenAI client instead of building HTTP requests here.
    // See roadmap/component docs for end-to-end usage with Chat Completions/Responses.

    private List<OpenAIMessage> ConvertMessages(List<UnifiedChatMessage> messages)
    {
        return messages.Select(msg => new OpenAIMessage
        {
            Role = ConvertRole(msg.Role),
            Content = msg.Content,
            ToolCalls = ConvertToolCalls(msg.ToolCalls),
            ToolCallId = msg.ToolCallId
        }).ToList();
    }

    private string ConvertRole(string unifiedRole)
    {
        return unifiedRole switch
        {
            MessageRoles.Tool => "function",     // OpenAI uses "function" role
            MessageRoles.Function => "function", // Direct mapping
            _ => unifiedRole
        };
    }
}
```

#### Anthropic Provider Adapter (Deferred)

Out of scope for Epic 246. This document sets the patterns (REST and SDK bases) that future adapters will follow.

#### Generic REST Client for OpenAI-compatible endpoints

To standardize REST integrations (e.g., Venice.AI), define a minimal client interface for OpenAI-compatible routes:

```csharp
public interface IRestAiChatClient
{
    Task<RestChatCompletionResponse> CreateChatCompletionAsync(
        RestChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<RestChatCompletionChunk> CreateChatCompletionStreamAsync(
        RestChatCompletionRequest request,
        CancellationToken cancellationToken = default);
}

public record RestChatCompletionRequest
{
    public required string Model { get; init; }
    public required object[] Messages { get; init; }
    public double? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public bool Stream { get; init; }
    public object[]? Tools { get; init; }
}

public record RestChatCompletionResponse
{
    public required string Id { get; init; }
    public required string Model { get; init; }
    public required long Created { get; init; }
    public required RestChoice[] Choices { get; init; }
    public required RestUsage Usage { get; init; }
}
```

#### Venice.AI Provider Adapter

```csharp
/// <summary>
/// Venice.AI provider adapter for accessing multiple models through Venice platform.
/// </summary>
public class VeniceAIProviderAdapter : RestProviderAdapterBase
{
    public override string ProviderName => "Venice.AI";

    public override ProviderCapabilities Capabilities => new()
    {
        SupportsToolCalling = true,
        SupportsStreaming = true,
        SupportsStrictMode = false,
        SupportedRoles = ["system", "user", "assistant", "tool"],
        MaxTokens = 32768,
        AvailableModels = ["llama-3.1-8b", "llama-3.1-70b", "llama-3.1-405b"],
        Features = new Dictionary<string, bool>
        {
            ["MultipleModels"] = true,
            ["CostOptimized"] = true,
            ["OpenSource"] = true
        }
    };

    protected override HttpRequestMessage CreateHttpRequest(object providerRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_config.ApiKey}");
        request.Headers.Add("User-Agent", "Zarichney-API/2.0");

        var json = JsonSerializer.Serialize(providerRequest, _jsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return request;
    }
}
```

### 1.2 Provider Adapter Factory

Dynamic provider adapter creation based on configuration:

```csharp
/// <summary>
/// Factory for creating provider adapters based on configuration.
/// </summary>
public interface IProviderAdapterFactory
{
    /// <summary>
    /// Create provider adapter for specified provider.
    /// </summary>
    IProviderAdapter CreateAdapter(string providerName);

    /// <summary>
    /// Get all configured provider adapters.
    /// </summary>
    IEnumerable<IProviderAdapter> GetAllAdapters();

    /// <summary>
    /// Check if provider is configured and available.
    /// </summary>
    bool IsProviderAvailable(string providerName);
}

public class ProviderAdapterFactory : IProviderAdapterFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<LanguageModelServiceConfig> _config;
    private readonly ILogger<ProviderAdapterFactory> _logger;

    public ProviderAdapterFactory(
        IServiceProvider serviceProvider,
        IOptions<LanguageModelServiceConfig> config,
        ILogger<ProviderAdapterFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
    }

    public IProviderAdapter CreateAdapter(string providerName)
    {
        if (!_config.Value.Providers.ContainsKey(providerName))
        {
            throw new ProviderConfigurationException(providerName,
                $"Provider {providerName} is not configured");
        }

        var providerConfig = _config.Value.Providers[providerName];
        if (!providerConfig.Enabled)
        {
            throw new ProviderUnavailableException(providerName,
                $"Provider {providerName} is disabled");
        }

        return providerName switch
        {
            "OpenAI" => _serviceProvider.GetRequiredService<OpenAIProviderAdapter>(),
            "Venice.AI" => _serviceProvider.GetRequiredService<VeniceAIProviderAdapter>(),
            _ => throw new ProviderNotSupportedException(providerName)
        };
    }
}
```

## 2. Provider Routing and Selection

### 2.1 IProviderRouter Implementation

Intelligent provider selection based on capabilities and configuration:

```csharp
/// <summary>
/// Routes requests to appropriate providers based on configuration and capabilities.
/// </summary>
public class ProviderRouter : IProviderRouter
{
    private readonly IProviderAdapterFactory _adapterFactory;
    private readonly IOptions<LanguageModelServiceConfig> _config;
    private readonly ILogger<ProviderRouter> _logger;

    public async Task<string> SelectProviderAsync(
        UnifiedCompletionRequest request,
        ProviderSelectionOptions? options = null)
    {
        // 1. Check if specific provider requested
        if (!string.IsNullOrEmpty(options?.PreferredProvider))
        {
            if (await IsProviderAvailable(options.PreferredProvider))
            {
                _logger.LogDebug("Using preferred provider: {Provider}", options.PreferredProvider);
                return options.PreferredProvider;
            }
            _logger.LogWarning("Preferred provider {Provider} unavailable, selecting alternative",
                options.PreferredProvider);
        }

        // 2. Check capability requirements
        var requiredCapabilities = DetermineRequiredCapabilities(request);
        var availableProviders = await GetAvailableProvidersAsync();

        var compatibleProviders = availableProviders
            .Where(p => SupportsCapabilities(p.Key, requiredCapabilities))
            .OrderBy(p => GetProviderPriority(p.Key))
            .ToList();

        if (!compatibleProviders.Any())
        {
            throw new NoAvailableProviderException(
                "No providers available that support required capabilities");
        }

        var selectedProvider = compatibleProviders.First().Key;
        _logger.LogDebug("Selected provider {Provider} based on capabilities", selectedProvider);
        return selectedProvider;
    }

    private ProviderCapabilities DetermineRequiredCapabilities(UnifiedCompletionRequest request)
    {
        return new ProviderCapabilities
        {
            SupportsToolCalling = request.Options?.Tools?.Any() == true,
            SupportsStreaming = request.Options?.Stream == true,
            SupportsStrictMode = request.Options?.StrictMode == true
        };
    }

    private int GetProviderPriority(string providerName)
    {
        return _config.Value.Routing.ProviderPriority.TryGetValue(providerName, out var priority)
            ? priority
            : int.MaxValue;
    }

    public async Task<string?> GetFallbackProviderAsync(
        string failedProvider,
        UnifiedCompletionRequest request)
    {
        var requiredCapabilities = DetermineRequiredCapabilities(request);
        var availableProviders = await GetAvailableProvidersAsync();

        var fallbackProviders = availableProviders
            .Where(p => p.Key != failedProvider)
            .Where(p => SupportsCapabilities(p.Key, requiredCapabilities))
            .OrderBy(p => GetProviderPriority(p.Key))
            .ToList();

        var fallback = fallbackProviders.FirstOrDefault().Key;
        if (fallback != null)
        {
            _logger.LogInformation("Selected fallback provider {Provider} after {FailedProvider} failed",
                fallback, failedProvider);
        }

        return fallback;
    }
}
```

### 2.2 Health Monitoring and Circuit Breaker

Provider health tracking with circuit breaker pattern:

```csharp
/// <summary>
/// Monitors provider health and implements circuit breaker pattern.
/// </summary>
public class ProviderHealthMonitor
{
    private readonly ConcurrentDictionary<string, ProviderHealthState> _healthStates = new();
    private readonly ILogger<ProviderHealthMonitor> _logger;

    public async Task<ProviderHealthStatus> CheckProviderHealthAsync(string providerName)
    {
        var healthState = _healthStates.GetOrAdd(providerName, _ => new ProviderHealthState());

        if (healthState.CircuitState == CircuitState.Open)
        {
            if (DateTime.UtcNow - healthState.LastFailureTime > TimeSpan.FromMinutes(5))
            {
                healthState.CircuitState = CircuitState.HalfOpen;
                _logger.LogInformation("Circuit breaker for {Provider} moved to half-open", providerName);
            }
            else
            {
                return new ProviderHealthStatus
                {
                    ProviderName = providerName,
                    IsHealthy = false,
                    Status = "Circuit breaker open",
                    LastChecked = DateTime.UtcNow
                };
            }
        }

        try
        {
            var adapter = _adapterFactory.CreateAdapter(providerName);
            var isAvailable = await adapter.IsAvailableAsync();

            if (isAvailable)
            {
                healthState.ConsecutiveFailures = 0;
                healthState.CircuitState = CircuitState.Closed;
                healthState.LastSuccessTime = DateTime.UtcNow;

                return new ProviderHealthStatus
                {
                    ProviderName = providerName,
                    IsHealthy = true,
                    Status = "Available",
                    LastChecked = DateTime.UtcNow,
                    ResponseTime = healthState.AverageResponseTime
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for provider {Provider}", providerName);
            RecordFailure(healthState, providerName);
        }

        return new ProviderHealthStatus
        {
            ProviderName = providerName,
            IsHealthy = false,
            Status = "Unavailable",
            LastChecked = DateTime.UtcNow
        };
    }

    private void RecordFailure(ProviderHealthState healthState, string providerName)
    {
        healthState.ConsecutiveFailures++;
        healthState.LastFailureTime = DateTime.UtcNow;

        if (healthState.ConsecutiveFailures >= 3)
        {
            healthState.CircuitState = CircuitState.Open;
            _logger.LogWarning("Circuit breaker opened for provider {Provider} after {Failures} failures",
                providerName, healthState.ConsecutiveFailures);
        }
    }
}

public class ProviderHealthState
{
    public int ConsecutiveFailures { get; set; }
    public DateTime LastFailureTime { get; set; }
    public DateTime LastSuccessTime { get; set; }
    public CircuitState CircuitState { get; set; } = CircuitState.Closed;
    public TimeSpan AverageResponseTime { get; set; }
}

public enum CircuitState
{
    Closed,   // Normal operation
    Open,     // Circuit breaker tripped
    HalfOpen  // Testing if provider is back
}
```

## 3. Message Normalization

### 3.1 Universal Message Normalizer

Handles conversion between unified format and provider-specific formats:

```csharp
/// <summary>
/// Handles conversion between unified message format and provider-specific formats.
/// </summary>
public class MessageNormalizer : IMessageNormalizer
{
    private readonly ILogger<MessageNormalizer> _logger;

    public object ConvertToProviderFormat(List<UnifiedChatMessage> messages, string provider)
    {
        return provider switch
        {
            "OpenAI" => ConvertToOpenAIFormat(messages),
            "Anthropic" => ConvertToAnthropicFormat(messages),
            "Venice.AI" => ConvertToVeniceFormat(messages),
            "Gemini" => ConvertToGeminiFormat(messages),
            "xAI" => ConvertToXAIFormat(messages),
            "DeepSeek" => ConvertToDeepSeekFormat(messages),
            _ => throw new ProviderNotSupportedException(provider)
        };
    }

    private List<OpenAIMessage> ConvertToOpenAIFormat(List<UnifiedChatMessage> messages)
    {
        return messages.Select(msg => new OpenAIMessage
        {
            Role = ConvertRoleForOpenAI(msg.Role),
            Content = msg.Content,
            ToolCalls = ConvertToolCallsForOpenAI(msg.ToolCalls),
            ToolCallId = msg.ToolCallId
        }).ToList();
    }

    private List<AnthropicMessage> ConvertToAnthropicFormat(List<UnifiedChatMessage> messages)
    {
        // Filter out system messages (handled separately in Anthropic)
        var conversationMessages = messages.Where(m => m.Role != MessageRoles.System);

        return conversationMessages.Select(msg => new AnthropicMessage
        {
            Role = ConvertRoleForAnthropic(msg.Role),
            Content = BuildAnthropicContent(msg)
        }).ToList();
    }

    private string ConvertRoleForOpenAI(string unifiedRole)
    {
        return unifiedRole switch
        {
            MessageRoles.Tool => "function",
            MessageRoles.Function => "function",
            _ => unifiedRole
        };
    }

    private string ConvertRoleForAnthropic(string unifiedRole)
    {
        return unifiedRole switch
        {
            MessageRoles.Function => "tool",
            MessageRoles.Tool => "tool",
            _ => unifiedRole
        };
    }
}
```

### 3.2 Tool Call Normalization

Standardizes function/tool calling across providers:

```csharp
/// <summary>
/// Normalizes tool/function calling formats across providers.
/// </summary>
public class ToolCallNormalizer
{
    public List<UnifiedToolCall> NormalizeToolCalls(object providerToolCalls, string provider)
    {
        return provider switch
        {
            "OpenAI" => NormalizeOpenAIToolCalls(providerToolCalls),
            "Anthropic" => NormalizeAnthropicToolCalls(providerToolCalls),
            "Venice.AI" => NormalizeVeniceToolCalls(providerToolCalls),
            _ => new List<UnifiedToolCall>()
        };
    }

    private List<UnifiedToolCall> NormalizeOpenAIToolCalls(object toolCalls)
    {
        if (toolCalls is not List<OpenAIToolCall> openAIToolCalls)
            return new List<UnifiedToolCall>();

        return openAIToolCalls.Select(tc => new UnifiedToolCall
        {
            Id = tc.Id,
            Type = "function",
            Function = new UnifiedFunction
            {
                Name = tc.Function.Name,
                Arguments = tc.Function.Arguments
            }
        }).ToList();
    }

    private List<UnifiedToolCall> NormalizeAnthropicToolCalls(object toolCalls)
    {
        if (toolCalls is not List<AnthropicToolUse> anthropicToolUse)
            return new List<UnifiedToolCall>();

        return anthropicToolUse.Select(tu => new UnifiedToolCall
        {
            Id = tu.Id,
            Type = "tool_use",
            Function = new UnifiedFunction
            {
                Name = tu.Name,
                Arguments = JsonSerializer.Serialize(tu.Input)
            }
        }).ToList();
    }
}
```

## 4. Provider-Specific Configuration

### 4.1 Configuration Classes

Strongly-typed configuration for each provider:

```csharp
/// <summary>
/// Base interface for provider configurations.
/// </summary>
public interface IProviderConfig
{
    string ProviderName { get; }
    bool Enabled { get; }
    string? ApiKey { get; }
    string? BaseUrl { get; }
    string? DefaultModel { get; }
    int TimeoutSeconds { get; }
    int MaxRetries { get; }
}

/// <summary>
/// OpenAI provider configuration.
/// </summary>
public class OpenAIProviderConfig : IProviderConfig
{
    public string ProviderName => "OpenAI";
    [Required] public bool Enabled { get; set; } = true;
    [Required] public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string? DefaultModel { get; set; } = "gpt-4";
    [Range(5, 300)] public int TimeoutSeconds { get; set; } = 60;
    [Range(0, 10)] public int MaxRetries { get; set; } = 3;

    // OpenAI-specific settings
    public string? Organization { get; set; }
    public bool EnableBatching { get; set; } = false;
    public bool StrictFunctionCalling { get; set; } = true;
}

/// <summary>
/// Anthropic provider configuration.
/// </summary>
public class AnthropicProviderConfig : IProviderConfig
{
    public string ProviderName => "Anthropic";
    [Required] public bool Enabled { get; set; } = true;
    [Required] public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; } = "https://api.anthropic.com/v1/";
    public string? DefaultModel { get; set; } = "claude-3-sonnet";
    [Range(5, 300)] public int TimeoutSeconds { get; set; } = 90;
    [Range(0, 10)] public int MaxRetries { get; set; } = 3;

    // Anthropic-specific settings
    public string AnthropicVersion { get; set; } = "2023-06-01";
    public bool EnableBeta { get; set; } = false;
}
```

### 4.2 Configuration Validation

Runtime configuration validation with detailed error reporting:

```csharp
/// <summary>
/// Validates provider configurations at startup.
/// </summary>
public class ProviderConfigurationValidator
{
    public async Task<List<ConfigurationValidationResult>> ValidateAllProvidersAsync(
        LanguageModelServiceConfig config)
    {
        var results = new List<ConfigurationValidationResult>();

        foreach (var (providerName, providerConfig) in config.Providers)
        {
            if (!providerConfig.Enabled)
            {
                results.Add(ConfigurationValidationResult.Success(providerName, "Provider disabled"));
                continue;
            }

            var result = await ValidateProviderConfigAsync(providerName, providerConfig);
            results.Add(result);
        }

        return results;
    }

    private async Task<ConfigurationValidationResult> ValidateProviderConfigAsync(
        string providerName,
        IProviderConfig config)
    {
        var errors = new List<string>();

        // Basic validation
        if (string.IsNullOrEmpty(config.ApiKey))
        {
            errors.Add("API key is required");
        }

        if (string.IsNullOrEmpty(config.DefaultModel))
        {
            errors.Add("Default model is required");
        }

        if (config.TimeoutSeconds < 5 || config.TimeoutSeconds > 300)
        {
            errors.Add("Timeout must be between 5 and 300 seconds");
        }

        // Provider-specific validation
        errors.AddRange(await ValidateProviderSpecificConfig(providerName, config));

        // Test connectivity if configuration is valid
        if (!errors.Any())
        {
            try
            {
                var adapter = CreateAdapter(providerName, config);
                var isAvailable = await adapter.IsAvailableAsync();
                if (!isAvailable)
                {
                    errors.Add("Provider is not accessible with current configuration");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Configuration test failed: {ex.Message}");
            }
        }

        return errors.Any()
            ? ConfigurationValidationResult.Failure(providerName, errors)
            : ConfigurationValidationResult.Success(providerName, "Configuration valid");
    }
}
```

## 5. Implementation Guidelines

### Provider Adapter Development
- Inherit from `RestProviderAdapterBase` for consistent HTTP client management
- Implement provider-specific message format conversion in `ConvertToProviderRequest`
- Handle provider-specific error codes and map to unified exception hierarchy
- Support all provider capabilities declared in the capabilities object

### Configuration Management
- Use strongly-typed configuration classes with validation attributes
- Implement `IProviderConfig` for consistent configuration patterns
- Support environment-specific configuration overrides
- Validate configuration at startup with detailed error reporting

### Error Handling Standards
- Map provider-specific errors to unified exception hierarchy
- Implement retry policies appropriate for each provider's rate limiting
- Support circuit breaker patterns for provider unavailability
- Log all provider interactions with correlation IDs for tracing

### Performance Optimization
- Use HttpClient factory pattern for connection pooling
- Implement async/await patterns throughout with ConfigureAwait(false)
- Support parallel provider health checking
- Cache provider capabilities to avoid repeated discovery calls

---

**🗂️ WORKING DIRECTORY ARTIFACT CREATED:**
- **Filename:** 02-provider-integration.md
- **Purpose:** Comprehensive provider integration specification detailing adapter patterns, routing strategies, and multi-provider configuration
- **Context for Team:** Implementation guide for CodeChanger covering all six provider adapters with specific API handling patterns
- **Dependencies:** Builds upon provider-adapter-patterns.md and configuration-routing-design.md from working directory
- **Next Actions:** Create migration strategy specification (03) and testing strategy specification (04)
