# Language Model Service Component Specification

**Last Updated:** 2025-01-25
**Component Type:** Core Service Implementation
**Implementation Priority:** High - Foundation Component

> **Parent:** [`Components`](./README.md) | [`Epic #246`](../README.md)

## Purpose & Responsibility

The Language Model Service is the core orchestration component that implements the ILanguageModelService interface, providing vendor-agnostic AI interactions through provider routing, session management integration, and unified error handling.

## 1. Interface Contract

### 1.1 Primary Interface Implementation

```csharp
/// <summary>
/// Core implementation of vendor-agnostic language model service.
/// Orchestrates provider selection, session management, and response handling.
/// </summary>
public class LanguageModelService : ILanguageModelService, IDisposable
{
    private readonly IProviderRouter _providerRouter;
    private readonly IProviderAdapterFactory _adapterFactory;
    private readonly ISessionManager _sessionManager;
    private readonly IMessageNormalizer _messageNormalizer;
    private readonly IOptions<LanguageModelServiceConfig> _config;
    private readonly ILogger<LanguageModelService> _logger;
    private readonly SemaphoreSlim _concurrencySemaphore;

    public LanguageModelService(
        IProviderRouter providerRouter,
        IProviderAdapterFactory adapterFactory,
        ISessionManager sessionManager,
        IMessageNormalizer messageNormalizer,
        IOptions<LanguageModelServiceConfig> config,
        ILogger<LanguageModelService> logger)
    {
        _providerRouter = providerRouter ?? throw new ArgumentNullException(nameof(providerRouter));
        _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _messageNormalizer = messageNormalizer ?? throw new ArgumentNullException(nameof(messageNormalizer));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Limit concurrent requests to prevent resource exhaustion
        _concurrencySemaphore = new SemaphoreSlim(
            _config.Value.Performance?.MaxConcurrentRequests ?? 100);
    }

    /// <summary>
    /// Get chat completion with automatic provider selection.
    /// </summary>
    public async Task<LlmResult<string>> GetCompletionAsync(
        List<UnifiedChatMessage> messages,
        CompletionOptions? options = null,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("LanguageModelService.GetCompletion");
        var correlationId = Guid.NewGuid().ToString();

        activity?.SetTag("correlation.id", correlationId);
        activity?.SetTag("conversation.id", conversationId);

        _logger.LogInformation("Starting completion request {CorrelationId} for conversation {ConversationId}",
            correlationId, conversationId);

        try
        {
            await _concurrencySemaphore.WaitAsync(cancellationToken);

            // Load conversation context if provided
            var conversationMessages = await LoadConversationContextAsync(
                messages, conversationId, cancellationToken);

            // Create unified completion request
            var request = new UnifiedCompletionRequest
            {
                Messages = conversationMessages,
                Options = options ?? new CompletionOptions(),
                ConversationId = conversationId,
                CorrelationId = correlationId
            };

            // Select provider based on configuration and capabilities
            var selectedProvider = await _providerRouter.SelectProviderAsync(
                request, new ProviderSelectionOptions(), cancellationToken);

            activity?.SetTag("provider.selected", selectedProvider);

            // Execute completion with failover support
            var response = await ExecuteCompletionWithFailoverAsync(
                request, selectedProvider, cancellationToken);

            // Save conversation state
            var newConversationId = await SaveConversationStateAsync(
                response, conversationId, cancellationToken);

            // Create LlmResult maintaining backward compatibility
            var result = new LlmResult<string>
            {
                IsSuccess = true,
                Data = response.Message.Content ?? string.Empty,
                ConversationId = newConversationId,
                Metadata = new Dictionary<string, object>
                {
                    ["provider"] = response.Provider,
                    ["model"] = response.Model,
                    ["usage"] = response.Usage,
                    ["correlationId"] = correlationId
                }
            };

            _logger.LogInformation("Completion request {CorrelationId} completed successfully using provider {Provider}",
                correlationId, selectedProvider);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Completion request {CorrelationId} failed", correlationId);

            return new LlmResult<string>
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ConversationId = conversationId,
                Metadata = new Dictionary<string, object>
                {
                    ["correlationId"] = correlationId,
                    ["errorType"] = ex.GetType().Name
                }
            };
        }
        finally
        {
            _concurrencySemaphore.Release();
        }
    }

    /// <summary>
    /// Get chat completion with explicit provider selection.
    /// </summary>
    public async Task<LlmResult<string>> GetCompletionAsync(
        List<UnifiedChatMessage> messages,
        string provider,
        CompletionOptions? options = null,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        // Validate provider is available
        if (!_adapterFactory.IsProviderAvailable(provider))
        {
            throw new ProviderUnavailableException(provider,
                $"Provider {provider} is not available or configured");
        }

        // Create selection options with preferred provider
        var selectionOptions = new ProviderSelectionOptions
        {
            PreferredProvider = provider,
            AllowFallback = false // Explicit provider selection disables fallback
        };

        // Use main completion method with explicit provider
        return await GetCompletionAsync(messages, options, conversationId, cancellationToken);
    }
}
```

### 1.2 Tool Calling Implementation

```csharp
/// <summary>
/// Call function/tool with structured response parsing.
/// </summary>
public async Task<LlmResult<T>> CallToolAsync<T>(
    List<UnifiedChatMessage> messages,
    ToolDefinition toolDefinition,
    CompletionOptions? options = null,
    string? conversationId = null,
    CancellationToken cancellationToken = default)
{
    using var activity = ActivitySource.StartActivity("LanguageModelService.CallTool");
    var correlationId = Guid.NewGuid().ToString();

    activity?.SetTag("correlation.id", correlationId);
    activity?.SetTag("tool.name", toolDefinition.Name);
    activity?.SetTag("response.type", typeof(T).Name);

    _logger.LogInformation("Starting tool call {CorrelationId} for tool {ToolName}",
        correlationId, toolDefinition.Name);

    try
    {
        await _concurrencySemaphore.WaitAsync(cancellationToken);

        // Prepare options with tool definition
        var toolOptions = options ?? new CompletionOptions();
        toolOptions.Tools = new List<ToolDefinition> { toolDefinition };
        toolOptions.ToolChoice = ToolChoice.Auto;

        // Load conversation context
        var conversationMessages = await LoadConversationContextAsync(
            messages, conversationId, cancellationToken);

        // Create request with tool calling capabilities requirement
        var request = new UnifiedCompletionRequest
        {
            Messages = conversationMessages,
            Options = toolOptions,
            ConversationId = conversationId,
            CorrelationId = correlationId,
            RequiredCapabilities = new ProviderCapabilities
            {
                SupportsToolCalling = true
            }
        };

        // Select provider that supports tool calling
        var selectedProvider = await _providerRouter.SelectProviderAsync(
            request, new ProviderSelectionOptions(), cancellationToken);

        activity?.SetTag("provider.selected", selectedProvider);

        // Execute tool calling completion
        var response = await ExecuteCompletionWithFailoverAsync(
            request, selectedProvider, cancellationToken);

        // Parse tool call response
        var toolCallResult = await ParseToolCallResponseAsync<T>(
            response, toolDefinition, cancellationToken);

        // Save conversation including tool call and response
        var newConversationId = await SaveToolCallConversationAsync(
            response, toolCallResult, conversationId, cancellationToken);

        var result = new LlmResult<T>
        {
            IsSuccess = true,
            Data = toolCallResult,
            ConversationId = newConversationId,
            Metadata = new Dictionary<string, object>
            {
                ["provider"] = response.Provider,
                ["model"] = response.Model,
                ["toolName"] = toolDefinition.Name,
                ["correlationId"] = correlationId
            }
        };

        _logger.LogInformation("Tool call {CorrelationId} completed successfully", correlationId);
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Tool call {CorrelationId} failed", correlationId);

        return new LlmResult<T>
        {
            IsSuccess = false,
            ErrorMessage = ex.Message,
            ConversationId = conversationId,
            Metadata = new Dictionary<string, object>
            {
                ["correlationId"] = correlationId,
                ["toolName"] = toolDefinition.Name,
                ["errorType"] = ex.GetType().Name
            }
        };
    }
    finally
    {
        _concurrencySemaphore.Release();
    }
}
```

## 2. Core Implementation Methods

### 2.1 Provider Execution with Failover

```csharp
/// <summary>
/// Execute completion with automatic failover to backup providers.
/// </summary>
private async Task<UnifiedCompletionResponse> ExecuteCompletionWithFailoverAsync(
    UnifiedCompletionRequest request,
    string primaryProvider,
    CancellationToken cancellationToken)
{
    var attemptedProviders = new List<string>();
    var lastException = (Exception?)null;

    try
    {
        // Attempt primary provider
        var response = await ExecuteProviderCompletionAsync(
            request, primaryProvider, cancellationToken);

        _logger.LogDebug("Primary provider {Provider} succeeded for request {CorrelationId}",
            primaryProvider, request.CorrelationId);

        return response;
    }
    catch (Exception ex)
    {
        attemptedProviders.Add(primaryProvider);
        lastException = ex;

        _logger.LogWarning(ex, "Primary provider {Provider} failed for request {CorrelationId}, attempting failover",
            primaryProvider, request.CorrelationId);
    }

    // Attempt failover providers
    var maxFailoverAttempts = _config.Value.Failover?.MaxFailoverAttempts ?? 2;

    for (int attempt = 1; attempt <= maxFailoverAttempts; attempt++)
    {
        try
        {
            var fallbackProvider = await _providerRouter.GetFallbackProviderAsync(
                primaryProvider, request, attemptedProviders);

            if (string.IsNullOrEmpty(fallbackProvider))
            {
                _logger.LogWarning("No more fallback providers available for request {CorrelationId}",
                    request.CorrelationId);
                break;
            }

            var response = await ExecuteProviderCompletionAsync(
                request, fallbackProvider, cancellationToken);

            _logger.LogInformation("Fallback provider {Provider} succeeded for request {CorrelationId} after {Attempts} attempts",
                fallbackProvider, request.CorrelationId, attempt);

            return response;
        }
        catch (Exception ex)
        {
            lastException = ex;
            _logger.LogWarning(ex, "Fallback attempt {Attempt} failed for request {CorrelationId}",
                attempt, request.CorrelationId);
        }
    }

    // All providers failed
    throw new AllProvidersFailedException(
        $"All available providers failed for request {request.CorrelationId}",
        attemptedProviders,
        lastException);
}

/// <summary>
/// Execute completion on specific provider with retry policy.
/// </summary>
private async Task<UnifiedCompletionResponse> ExecuteProviderCompletionAsync(
    UnifiedCompletionRequest request,
    string providerName,
    CancellationToken cancellationToken)
{
    var adapter = _adapterFactory.CreateAdapter(providerName);

    // Apply retry policy
    var retryPolicy = CreateRetryPolicy(providerName);

    return await retryPolicy.ExecuteAsync(async () =>
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await adapter.GetCompletionAsync(request, cancellationToken);
            stopwatch.Stop();

            // Record metrics
            RecordProviderMetrics(providerName, stopwatch.Elapsed, success: true);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            RecordProviderMetrics(providerName, stopwatch.Elapsed, success: false);

            _logger.LogError(ex, "Provider {Provider} request failed after {Duration}ms",
                providerName, stopwatch.ElapsedMilliseconds);

            throw;
        }
    });
}
```

### 2.2 Conversation Management Integration

```csharp
/// <summary>
/// Load conversation context from session manager.
/// </summary>
private async Task<List<UnifiedChatMessage>> LoadConversationContextAsync(
    List<UnifiedChatMessage> newMessages,
    string? conversationId,
    CancellationToken cancellationToken)
{
    if (string.IsNullOrEmpty(conversationId))
    {
        return newMessages;
    }

    try
    {
        var existingMessages = await _sessionManager.GetConversationMessagesAsync(
            conversationId, cancellationToken);

        // Convert existing messages to unified format
        var unifiedExistingMessages = existingMessages
            .Select(ConvertFromSessionMessage)
            .ToList();

        // Combine existing and new messages
        var allMessages = new List<UnifiedChatMessage>(unifiedExistingMessages);
        allMessages.AddRange(newMessages);

        _logger.LogDebug("Loaded {ExistingCount} existing messages for conversation {ConversationId}",
            existingMessages.Count, conversationId);

        return allMessages;
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Failed to load conversation context for {ConversationId}, using new messages only",
            conversationId);

        return newMessages;
    }
}

/// <summary>
/// Save conversation state to session manager.
/// </summary>
private async Task<string> SaveConversationStateAsync(
    UnifiedCompletionResponse response,
    string? conversationId,
    CancellationToken cancellationToken)
{
    try
    {
        var sessionMessage = ConvertToSessionMessage(response.Message);

        if (string.IsNullOrEmpty(conversationId))
        {
            // Create new conversation
            conversationId = await _sessionManager.CreateConversationAsync(
                sessionMessage, cancellationToken);

            _logger.LogDebug("Created new conversation {ConversationId}", conversationId);
        }
        else
        {
            // Add to existing conversation
            await _sessionManager.AddMessageToConversationAsync(
                conversationId, sessionMessage, cancellationToken);

            _logger.LogDebug("Added message to conversation {ConversationId}", conversationId);
        }

        return conversationId;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to save conversation state for {ConversationId}", conversationId);

        // Return existing conversation ID or generate new one
        return conversationId ?? Guid.NewGuid().ToString();
    }
}
```

## 3. Performance Optimization Features

### 3.1 Concurrency Management

```csharp
/// <summary>
/// Performance optimization configuration.
/// </summary>
public class PerformanceConfig
{
    /// <summary>
    /// Maximum concurrent requests allowed.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 100;

    /// <summary>
    /// Request timeout in milliseconds.
    /// </summary>
    public int RequestTimeoutMs { get; set; } = 120000; // 2 minutes

    /// <summary>
    /// Enable request/response caching.
    /// </summary>
    public bool EnableCaching { get; set; } = false;

    /// <summary>
    /// Cache expiration time in minutes.
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 60;
}

/// <summary>
/// Manage request concurrency to prevent resource exhaustion.
/// </summary>
private async Task<T> ExecuteWithConcurrencyControlAsync<T>(
    Func<Task<T>> operation,
    CancellationToken cancellationToken)
{
    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(
        _config.Value.Performance?.RequestTimeoutMs ?? 120000));

    await _concurrencySemaphore.WaitAsync(timeoutCts.Token);

    try
    {
        return await operation();
    }
    finally
    {
        _concurrencySemaphore.Release();
    }
}
```

### 3.2 Metrics and Monitoring

```csharp
/// <summary>
/// Record provider performance metrics.
/// </summary>
private void RecordProviderMetrics(string providerName, TimeSpan duration, bool success)
{
    var tags = new[]
    {
        new KeyValuePair<string, object?>("provider", providerName),
        new KeyValuePair<string, object?>("success", success)
    };

    // Record response time
    Metrics.CreateHistogram("languagemodel_request_duration", "Request duration in milliseconds")
        .Record(duration.TotalMilliseconds, tags);

    // Record success/failure count
    Metrics.CreateCounter("languagemodel_requests_total", "Total requests")
        .Add(1, tags);

    if (!success)
    {
        Metrics.CreateCounter("languagemodel_errors_total", "Total errors")
            .Add(1, tags);
    }
}

/// <summary>
/// Health check implementation for the service.
/// </summary>
public async Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default)
{
    try
    {
        var healthResults = new Dictionary<string, object>();

        // Check provider availability
        var providers = _config.Value.Providers.Where(p => p.Value.Enabled);

        foreach (var (providerName, _) in providers)
        {
            try
            {
                var adapter = _adapterFactory.CreateAdapter(providerName);
                var isAvailable = await adapter.IsAvailableAsync();

                healthResults[providerName] = new
                {
                    Available = isAvailable,
                    LastChecked = DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex)
            {
                healthResults[providerName] = new
                {
                    Available = false,
                    Error = ex.Message,
                    LastChecked = DateTimeOffset.UtcNow
                };
            }
        }

        var allProvidersHealthy = healthResults.Values
            .All(v => ((dynamic)v).Available);

        return allProvidersHealthy
            ? HealthCheckResult.Healthy("All providers available", healthResults)
            : HealthCheckResult.Degraded("Some providers unavailable", data: healthResults);
    }
    catch (Exception ex)
    {
        return HealthCheckResult.Unhealthy("Health check failed", ex);
    }
}
```

## 4. Error Handling and Resilience

### 4.1 Exception Handling Strategy

```csharp
/// <summary>
/// Comprehensive error handling with provider context preservation.
/// </summary>
private LlmResult<T> HandleException<T>(Exception ex, string? conversationId, string correlationId)
{
    var errorMetadata = new Dictionary<string, object>
    {
        ["correlationId"] = correlationId,
        ["errorType"] = ex.GetType().Name,
        ["timestamp"] = DateTimeOffset.UtcNow
    };

    return ex switch
    {
        ProviderUnavailableException providerEx => new LlmResult<T>
        {
            IsSuccess = false,
            ErrorMessage = $"Provider {providerEx.ProviderName} is currently unavailable",
            ConversationId = conversationId,
            Metadata = errorMetadata.Append("provider", providerEx.ProviderName)
        },

        RateLimitExceededException rateLimitEx => new LlmResult<T>
        {
            IsSuccess = false,
            ErrorMessage = "Rate limit exceeded, please try again later",
            ConversationId = conversationId,
            Metadata = errorMetadata.Append("retryAfter", rateLimitEx.RetryAfter?.TotalSeconds)
        },

        AllProvidersFailedException allFailedEx => new LlmResult<T>
        {
            IsSuccess = false,
            ErrorMessage = "All available providers failed to respond",
            ConversationId = conversationId,
            Metadata = errorMetadata.Append("attemptedProviders", allFailedEx.AttemptedProviders)
        },

        OperationCanceledException => new LlmResult<T>
        {
            IsSuccess = false,
            ErrorMessage = "Request was cancelled or timed out",
            ConversationId = conversationId,
            Metadata = errorMetadata
        },

        _ => new LlmResult<T>
        {
            IsSuccess = false,
            ErrorMessage = "An unexpected error occurred",
            ConversationId = conversationId,
            Metadata = errorMetadata
        }
    };
}
```

## 5. Dependency Injection Configuration

### 5.1 Service Registration

```csharp
/// <summary>
/// Extension methods for configuring LanguageModelService in DI container.
/// </summary>
public static class LanguageModelServiceExtensions
{
    public static IServiceCollection AddLanguageModelService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register configuration
        services.Configure<LanguageModelServiceConfig>(
            configuration.GetSection(LanguageModelServiceConfig.SectionName));

        // Register core service
        services.AddScoped<ILanguageModelService, LanguageModelService>();

        // Register supporting services
        services.AddScoped<IProviderRouter, ProviderRouter>();
        services.AddScoped<IProviderAdapterFactory, ProviderAdapterFactory>();
        services.AddScoped<IMessageNormalizer, MessageNormalizer>();

        // Register health checks
        services.AddHealthChecks()
            .AddCheck<LanguageModelService>("language-model-service");

        // Register metrics
        services.AddSingleton<ActivitySource>(
            new ActivitySource("Zarichney.Server.LanguageModelService"));

        return services;
    }
}
```

## Implementation Guidelines

### Code Quality Standards
- Implement comprehensive async/await patterns with ConfigureAwait(false)
- Use structured logging with correlation IDs for request tracing
- Implement proper disposal patterns for resources
- Follow dependency injection best practices

### Performance Requirements
- Support concurrent request processing with configurable limits
- Implement efficient provider failover with minimal latency overhead
- Use connection pooling for HTTP client communications
- Record detailed metrics for monitoring and optimization

### Testing Requirements
- Unit tests with >95% coverage for all public methods
- Integration tests with mock providers for error scenarios
- Performance tests for concurrent request handling
- End-to-end tests with real provider integration

---

**🗂️ WORKING DIRECTORY ARTIFACT CREATED:**
- **Filename:** components/language-model-service.md
- **Purpose:** Core service component specification with complete implementation patterns
- **Context for Team:** Primary service implementation guide for CodeChanger with orchestration patterns
- **Dependencies:** Integrates with provider adapters, routing, and session management components
- **Next Actions:** Create provider-adapters.md and configuration-router.md component specifications