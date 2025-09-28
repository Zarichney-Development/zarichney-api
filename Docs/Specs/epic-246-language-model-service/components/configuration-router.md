# Configuration Router Component Specification

**Last Updated:** 2025-01-25
**Component Type:** Provider Selection & Routing Logic
**Implementation Priority:** Medium - After Basic Provider Support

> **Parent:** [`Components`](./README.md) | [`Epic #246`](../README.md)

## Purpose & Responsibility

The Configuration Router implements intelligent provider selection and routing logic based on capabilities, health status, load balancing, and configuration policies. It acts as the decision engine for determining which provider should handle each request while supporting failover and optimization strategies.

## 1. Core Router Interface Implementation

### 1.1 IProviderRouter Implementation

```csharp
/// <summary>
/// Routes requests to appropriate providers based on configuration and capabilities.
/// Implements intelligent selection with health monitoring and load balancing.
/// </summary>
public class ProviderRouter : IProviderRouter, IDisposable
{
    private readonly IProviderAdapterFactory _adapterFactory;
    private readonly IProviderHealthMonitor _healthMonitor;
    private readonly IOptions<LanguageModelServiceConfig> _config;
    private readonly ILogger<ProviderRouter> _logger;
    private readonly Timer _healthCheckTimer;
    private readonly ConcurrentDictionary<string, ProviderPerformanceMetrics> _performanceMetrics;
    private readonly SemaphoreSlim _routingSemaphore;
    private bool _disposed = false;

    public ProviderRouter(
        IProviderAdapterFactory adapterFactory,
        IProviderHealthMonitor healthMonitor,
        IOptions<LanguageModelServiceConfig> config,
        ILogger<ProviderRouter> logger)
    {
        _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        _healthMonitor = healthMonitor ?? throw new ArgumentNullException(nameof(healthMonitor));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _performanceMetrics = new ConcurrentDictionary<string, ProviderPerformanceMetrics>();
        _routingSemaphore = new SemaphoreSlim(1, 1);

        // Start periodic health checks
        _healthCheckTimer = new Timer(PerformHealthChecks, null,
            TimeSpan.Zero, TimeSpan.FromSeconds(_config.Value.HealthCheck.IntervalSeconds));
    }

    /// <summary>
    /// Select optimal provider for a completion request using intelligent routing.
    /// </summary>
    public async Task<string> SelectProviderAsync(
        UnifiedCompletionRequest request,
        ProviderSelectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("ProviderRouter.SelectProvider");
        var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString();

        activity?.SetTag("correlation.id", correlationId);
        activity?.SetTag("preferred.provider", options?.PreferredProvider);

        _logger.LogDebug("Starting provider selection for request {CorrelationId}", correlationId);

        await _routingSemaphore.WaitAsync(cancellationToken);

        try
        {
            // 1. Honor explicit provider preference
            if (!string.IsNullOrEmpty(options?.PreferredProvider))
            {
                var preferredProvider = await ValidateProviderSelectionAsync(
                    options.PreferredProvider, request, cancellationToken);

                if (preferredProvider != null)
                {
                    activity?.SetTag("selection.method", "preferred");
                    _logger.LogDebug("Selected preferred provider {Provider} for request {CorrelationId}",
                        preferredProvider, correlationId);
                    return preferredProvider;
                }

                if (!options.AllowFallback)
                {
                    throw new ProviderUnavailableException(options.PreferredProvider,
                        $"Preferred provider {options.PreferredProvider} is unavailable and fallback is disabled");
                }

                _logger.LogWarning("Preferred provider {Provider} unavailable, selecting alternative for request {CorrelationId}",
                    options.PreferredProvider, correlationId);
            }

            // 2. Apply intelligent routing logic
            var selectedProvider = await ApplyRoutingLogicAsync(request, cancellationToken);

            activity?.SetTag("selection.method", "intelligent");
            activity?.SetTag("selected.provider", selectedProvider);

            _logger.LogInformation("Selected provider {Provider} for request {CorrelationId} using intelligent routing",
                selectedProvider, correlationId);

            return selectedProvider;
        }
        finally
        {
            _routingSemaphore.Release();
        }
    }

    /// <summary>
    /// Get fallback provider when primary provider fails.
    /// </summary>
    public async Task<string?> GetFallbackProviderAsync(
        string failedProvider,
        UnifiedCompletionRequest request,
        List<string>? attemptedProviders = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("ProviderRouter.GetFallback");
        var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString();

        activity?.SetTag("correlation.id", correlationId);
        activity?.SetTag("failed.provider", failedProvider);

        _logger.LogDebug("Finding fallback provider for failed {Provider}, request {CorrelationId}",
            failedProvider, correlationId);

        var excludedProviders = new HashSet<string>(attemptedProviders ?? new List<string>())
        {
            failedProvider
        };

        // Record failure for future routing decisions
        RecordProviderFailure(failedProvider);

        // Find alternative providers
        var availableProviders = await GetHealthyProvidersAsync(cancellationToken);
        var candidateProviders = availableProviders
            .Where(p => !excludedProviders.Contains(p.Key))
            .Where(p => SupportsCapabilities(p.Key, DetermineRequiredCapabilities(request)))
            .OrderBy(p => GetProviderPriority(p.Key))
            .ThenByDescending(p => GetProviderPerformanceScore(p.Key))
            .ToList();

        if (!candidateProviders.Any())
        {
            _logger.LogWarning("No fallback providers available for request {CorrelationId}", correlationId);
            return null;
        }

        var fallbackProvider = candidateProviders.First().Key;

        activity?.SetTag("fallback.provider", fallbackProvider);

        _logger.LogInformation("Selected fallback provider {Provider} for request {CorrelationId}",
            fallbackProvider, correlationId);

        return fallbackProvider;
    }
}
```

### 1.2 Intelligent Routing Logic

```csharp
/// <summary>
/// Apply comprehensive routing logic considering multiple factors.
/// </summary>
private async Task<string> ApplyRoutingLogicAsync(
    UnifiedCompletionRequest request,
    CancellationToken cancellationToken)
{
    var routingConfig = _config.Value.Routing;

    // Get healthy providers
    var healthyProviders = await GetHealthyProvidersAsync(cancellationToken);

    if (!healthyProviders.Any())
    {
        throw new NoAvailableProviderException("No healthy providers available");
    }

    // Apply capability filtering
    var requiredCapabilities = DetermineRequiredCapabilities(request);
    var capableProviders = healthyProviders
        .Where(p => SupportsCapabilities(p.Key, requiredCapabilities))
        .ToList();

    if (!capableProviders.Any())
    {
        throw new NoAvailableProviderException(
            $"No providers support required capabilities: {string.Join(", ", GetRequiredCapabilityNames(requiredCapabilities))}");
    }

    // Apply routing strategy
    return routingConfig.LoadBalancing.Strategy switch
    {
        LoadBalancingStrategy.Priority => SelectByPriority(capableProviders),
        LoadBalancingStrategy.RoundRobin => SelectByRoundRobin(capableProviders),
        LoadBalancingStrategy.WeightedRoundRobin => SelectByWeightedRoundRobin(capableProviders),
        LoadBalancingStrategy.LeastConnections => SelectByLeastConnections(capableProviders),
        LoadBalancingStrategy.Random => SelectByRandom(capableProviders),
        _ => SelectByCapabilityOptimization(capableProviders, request)
    };
}

/// <summary>
/// Determine required capabilities from request characteristics.
/// </summary>
private ProviderCapabilities DetermineRequiredCapabilities(UnifiedCompletionRequest request)
{
    var messageLength = request.Messages.Sum(m => m.Content?.Length ?? 0);
    var hasTools = request.Options?.Tools?.Any() == true;
    var isStreaming = request.Options?.Stream == true;
    var requiresStrictMode = request.Options?.StrictMode == true;

    return new ProviderCapabilities
    {
        SupportsToolCalling = hasTools,
        SupportsStreaming = isStreaming,
        SupportsStrictMode = requiresStrictMode,
        MinTokens = messageLength / 4, // Rough token estimate
        MaxTokens = request.Options?.MaxTokens ?? 4096
    };
}

/// <summary>
/// Select provider based on capability optimization for specific request types.
/// </summary>
private string SelectByCapabilityOptimization(
    List<KeyValuePair<string, ProviderHealthStatus>> capableProviders,
    UnifiedCompletionRequest request)
{
    var routingConfig = _config.Value.Routing.CapabilityRouting;
    var messageLength = request.Messages.Sum(m => m.Content?.Length ?? 0);
    var hasTools = request.Options?.Tools?.Any() == true;
    var isStreaming = request.Options?.Stream == true;

    // Tool calling optimization
    if (hasTools && !string.IsNullOrEmpty(routingConfig.ToolCallingPreferredProvider))
    {
        var toolProvider = capableProviders.FirstOrDefault(p =>
            p.Key == routingConfig.ToolCallingPreferredProvider);

        if (toolProvider.Key != null)
        {
            _logger.LogDebug("Selected {Provider} for tool calling optimization", toolProvider.Key);
            return toolProvider.Key;
        }
    }

    // Long context optimization
    if (messageLength > routingConfig.LongContextTokenThreshold &&
        !string.IsNullOrEmpty(routingConfig.LongContextPreferredProvider))
    {
        var longContextProvider = capableProviders.FirstOrDefault(p =>
            p.Key == routingConfig.LongContextPreferredProvider);

        if (longContextProvider.Key != null)
        {
            _logger.LogDebug("Selected {Provider} for long context optimization", longContextProvider.Key);
            return longContextProvider.Key;
        }
    }

    // Streaming optimization
    if (isStreaming && !string.IsNullOrEmpty(routingConfig.StreamingPreferredProvider))
    {
        var streamingProvider = capableProviders.FirstOrDefault(p =>
            p.Key == routingConfig.StreamingPreferredProvider);

        if (streamingProvider.Key != null)
        {
            _logger.LogDebug("Selected {Provider} for streaming optimization", streamingProvider.Key);
            return streamingProvider.Key;
        }
    }

    // Cost optimization (prefer Venice.AI for simple requests)
    if (_config.Value.Routing.CostOptimization.Enabled &&
        messageLength < 1000 && !hasTools)
    {
        var costOptimalProvider = capableProviders.FirstOrDefault(p =>
            p.Key == "Venice.AI");

        if (costOptimalProvider.Key != null)
        {
            _logger.LogDebug("Selected {Provider} for cost optimization", costOptimalProvider.Key);
            return costOptimalProvider.Key;
        }
    }

    // Default to priority-based selection
    return SelectByPriority(capableProviders);
}
```

## 2. Load Balancing Strategies

### 2.1 Weighted Round-Robin Implementation

```csharp
private readonly ConcurrentDictionary<string, int> _roundRobinCounters = new();

/// <summary>
/// Select provider using weighted round-robin algorithm.
/// </summary>
private string SelectByWeightedRoundRobin(
    List<KeyValuePair<string, ProviderHealthStatus>> capableProviders)
{
    var weights = _config.Value.Routing.LoadBalancing.ProviderWeights;
    var totalWeight = capableProviders.Sum(p => weights.GetValueOrDefault(p.Key, 1));

    if (totalWeight == 0)
    {
        return SelectByPriority(capableProviders);
    }

    // Calculate weighted selection
    var random = new Random();
    var selection = random.Next(0, totalWeight);
    var currentWeight = 0;

    foreach (var provider in capableProviders)
    {
        var providerWeight = weights.GetValueOrDefault(provider.Key, 1);
        currentWeight += providerWeight;

        if (currentWeight > selection)
        {
            _logger.LogDebug("Selected {Provider} via weighted round-robin (weight: {Weight})",
                provider.Key, providerWeight);
            return provider.Key;
        }
    }

    // Fallback to first provider
    return capableProviders.First().Key;
}

/// <summary>
/// Select provider with least active connections.
/// </summary>
private string SelectByLeastConnections(
    List<KeyValuePair<string, ProviderHealthStatus>> capableProviders)
{
    var providerWithLeastConnections = capableProviders
        .OrderBy(p => GetActiveConnectionCount(p.Key))
        .ThenBy(p => GetProviderPriority(p.Key))
        .First();

    _logger.LogDebug("Selected {Provider} with least connections ({Connections})",
        providerWithLeastConnections.Key, GetActiveConnectionCount(providerWithLeastConnections.Key));

    return providerWithLeastConnections.Key;
}

/// <summary>
/// Select provider based on performance scoring.
/// </summary>
private string SelectByPerformanceScore(
    List<KeyValuePair<string, ProviderHealthStatus>> capableProviders)
{
    var bestPerformingProvider = capableProviders
        .OrderByDescending(p => GetProviderPerformanceScore(p.Key))
        .ThenBy(p => GetProviderPriority(p.Key))
        .First();

    _logger.LogDebug("Selected {Provider} with best performance score ({Score:F2})",
        bestPerformingProvider.Key, GetProviderPerformanceScore(bestPerformingProvider.Key));

    return bestPerformingProvider.Key;
}
```

### 2.2 Performance Metrics Tracking

```csharp
/// <summary>
/// Provider performance metrics for routing decisions.
/// </summary>
public class ProviderPerformanceMetrics
{
    public double AverageResponseTime { get; set; }
    public double SuccessRate { get; set; }
    public int ActiveConnections { get; set; }
    public int TotalRequests { get; set; }
    public int FailedRequests { get; set; }
    public DateTime LastUpdated { get; set; }
    public CircularBuffer<double> RecentResponseTimes { get; } = new(100);
    public CircularBuffer<bool> RecentResults { get; } = new(100);
}

/// <summary>
/// Calculate provider performance score for routing decisions.
/// </summary>
private double GetProviderPerformanceScore(string providerName)
{
    var metrics = _performanceMetrics.GetValueOrDefault(providerName, new ProviderPerformanceMetrics());

    if (metrics.TotalRequests == 0)
    {
        return 0.5; // Neutral score for new providers
    }

    // Weighted scoring: 60% success rate, 40% response time
    var successScore = metrics.SuccessRate;
    var responseTimeScore = Math.Max(0, 1.0 - (metrics.AverageResponseTime / 5000.0)); // 5s baseline

    var overallScore = (successScore * 0.6) + (responseTimeScore * 0.4);

    // Apply recency bonus for recently successful providers
    var recencyBonus = IsRecentlySuccessful(providerName) ? 0.1 : 0.0;

    return Math.Min(1.0, overallScore + recencyBonus);
}

/// <summary>
/// Record provider performance for future routing decisions.
/// </summary>
public void RecordProviderPerformance(
    string providerName,
    TimeSpan responseTime,
    bool success)
{
    var metrics = _performanceMetrics.GetOrAdd(providerName, new ProviderPerformanceMetrics());

    lock (metrics)
    {
        metrics.RecentResponseTimes.Add(responseTime.TotalMilliseconds);
        metrics.RecentResults.Add(success);

        metrics.TotalRequests++;
        if (!success)
        {
            metrics.FailedRequests++;
        }

        // Calculate rolling averages
        metrics.AverageResponseTime = metrics.RecentResponseTimes.Average();
        metrics.SuccessRate = metrics.RecentResults.Count(r => r) / (double)metrics.RecentResults.Count;
        metrics.LastUpdated = DateTime.UtcNow;
    }

    _logger.LogTrace("Recorded performance for {Provider}: {ResponseTime}ms, success: {Success}",
        providerName, responseTime.TotalMilliseconds, success);
}
```

## 3. Health Monitoring Integration

### 3.1 Provider Health Monitoring

```csharp
/// <summary>
/// Perform periodic health checks on all configured providers.
/// </summary>
private async void PerformHealthChecks(object? state)
{
    if (_disposed) return;

    try
    {
        var healthCheckTasks = _config.Value.Providers
            .Where(p => p.Value.Enabled)
            .Select(async p =>
            {
                try
                {
                    var healthStatus = await _healthMonitor.CheckProviderHealthAsync(p.Key);
                    _logger.LogTrace("Health check for {Provider}: {Status}",
                        p.Key, healthStatus.IsHealthy ? "Healthy" : "Unhealthy");
                    return new { Provider = p.Key, Health = healthStatus };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Health check failed for provider {Provider}", p.Key);
                    return new { Provider = p.Key, Health = (ProviderHealthStatus?)null };
                }
            });

        var results = await Task.WhenAll(healthCheckTasks);

        foreach (var result in results.Where(r => r.Health != null))
        {
            UpdateProviderHealth(result.Provider, result.Health!);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during health check cycle");
    }
}

/// <summary>
/// Get currently healthy providers for routing decisions.
/// </summary>
private async Task<Dictionary<string, ProviderHealthStatus>> GetHealthyProvidersAsync(
    CancellationToken cancellationToken = default)
{
    var healthyProviders = new Dictionary<string, ProviderHealthStatus>();

    foreach (var (providerName, providerConfig) in _config.Value.Providers)
    {
        if (!providerConfig.Enabled) continue;

        try
        {
            var healthStatus = await _healthMonitor.GetCachedHealthStatusAsync(providerName);

            if (healthStatus.IsHealthy)
            {
                healthyProviders[providerName] = healthStatus;
            }
            else
            {
                _logger.LogDebug("Provider {Provider} excluded due to health status: {Status}",
                    providerName, healthStatus.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get health status for provider {Provider}", providerName);
        }
    }

    return healthyProviders;
}
```

### 3.2 Circuit Breaker Integration

```csharp
/// <summary>
/// Validate provider selection considering circuit breaker states.
/// </summary>
private async Task<string?> ValidateProviderSelectionAsync(
    string providerName,
    UnifiedCompletionRequest request,
    CancellationToken cancellationToken)
{
    try
    {
        // Check if provider is available
        if (!_adapterFactory.IsProviderAvailable(providerName))
        {
            _logger.LogDebug("Provider {Provider} is not available", providerName);
            return null;
        }

        // Check health status
        var healthStatus = await _healthMonitor.GetCachedHealthStatusAsync(providerName);
        if (!healthStatus.IsHealthy)
        {
            _logger.LogDebug("Provider {Provider} is unhealthy: {Status}",
                providerName, healthStatus.Status);
            return null;
        }

        // Check circuit breaker state
        if (IsCircuitBreakerOpen(providerName))
        {
            _logger.LogDebug("Provider {Provider} circuit breaker is open", providerName);
            return null;
        }

        // Check capability requirements
        var requiredCapabilities = DetermineRequiredCapabilities(request);
        if (!SupportsCapabilities(providerName, requiredCapabilities))
        {
            _logger.LogDebug("Provider {Provider} does not support required capabilities", providerName);
            return null;
        }

        // Check rate limits
        if (IsRateLimited(providerName))
        {
            _logger.LogDebug("Provider {Provider} is rate limited", providerName);
            return null;
        }

        return providerName;
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Error validating provider {Provider} selection", providerName);
        return null;
    }
}

/// <summary>
/// Check if provider circuit breaker is open.
/// </summary>
private bool IsCircuitBreakerOpen(string providerName)
{
    var metrics = _performanceMetrics.GetValueOrDefault(providerName);
    if (metrics == null) return false;

    var circuitBreakerConfig = _config.Value.HealthCheck.CircuitBreaker;
    if (!circuitBreakerConfig.Enabled) return false;

    // Check failure rate threshold
    if (metrics.TotalRequests >= circuitBreakerConfig.MinimumRequestThreshold)
    {
        var failureRate = metrics.FailedRequests / (double)metrics.TotalRequests;
        if (failureRate >= circuitBreakerConfig.FailureRateThreshold)
        {
            var timeSinceLastFailure = DateTime.UtcNow - metrics.LastUpdated;
            return timeSinceLastFailure < TimeSpan.FromSeconds(circuitBreakerConfig.RecoveryTimeoutSeconds);
        }
    }

    return false;
}
```

## 4. Rate Limiting and Throttling

### 4.1 Provider Rate Limiting

```csharp
private readonly ConcurrentDictionary<string, SlidingWindowRateLimiter> _rateLimiters = new();

/// <summary>
/// Check if provider is currently rate limited.
/// </summary>
private bool IsRateLimited(string providerName)
{
    var rateLimitConfig = _config.Value.Routing.LoadBalancing;
    if (!rateLimitConfig.EnableRateLimiting) return false;

    var requestsPerMinute = rateLimitConfig.RequestsPerMinuteLimit.GetValueOrDefault(providerName, int.MaxValue);
    if (requestsPerMinute == int.MaxValue) return false;

    var rateLimiter = _rateLimiters.GetOrAdd(providerName,
        _ => new SlidingWindowRateLimiter(requestsPerMinute, TimeSpan.FromMinutes(1)));

    return !rateLimiter.TryAcquire();
}

/// <summary>
/// Record request for rate limiting tracking.
/// </summary>
public void RecordProviderRequest(string providerName)
{
    var rateLimitConfig = _config.Value.Routing.LoadBalancing;
    if (!rateLimitConfig.EnableRateLimiting) return;

    var requestsPerMinute = rateLimitConfig.RequestsPerMinuteLimit.GetValueOrDefault(providerName, int.MaxValue);
    if (requestsPerMinute == int.MaxValue) return;

    var rateLimiter = _rateLimiters.GetOrAdd(providerName,
        _ => new SlidingWindowRateLimiter(requestsPerMinute, TimeSpan.FromMinutes(1)));

    rateLimiter.TryAcquire(); // Record the request
}
```

### 4.2 Adaptive Rate Limiting

```csharp
/// <summary>
/// Sliding window rate limiter for adaptive throttling.
/// </summary>
public class SlidingWindowRateLimiter
{
    private readonly int _maxRequests;
    private readonly TimeSpan _window;
    private readonly Queue<DateTime> _requestTimes;
    private readonly object _lock = new();

    public SlidingWindowRateLimiter(int maxRequests, TimeSpan window)
    {
        _maxRequests = maxRequests;
        _window = window;
        _requestTimes = new Queue<DateTime>();
    }

    public bool TryAcquire()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var windowStart = now - _window;

            // Remove expired requests
            while (_requestTimes.Count > 0 && _requestTimes.Peek() < windowStart)
            {
                _requestTimes.Dequeue();
            }

            // Check if we can accept another request
            if (_requestTimes.Count < _maxRequests)
            {
                _requestTimes.Enqueue(now);
                return true;
            }

            return false;
        }
    }

    public int CurrentRequestCount
    {
        get
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var windowStart = now - _window;

                // Remove expired requests
                while (_requestTimes.Count > 0 && _requestTimes.Peek() < windowStart)
                {
                    _requestTimes.Dequeue();
                }

                return _requestTimes.Count;
            }
        }
    }
}
```

## 5. Configuration Integration

### 5.1 Dependency Injection Setup

```csharp
/// <summary>
/// Extension methods for configuring ProviderRouter in DI container.
/// </summary>
public static class ProviderRouterExtensions
{
    public static IServiceCollection AddProviderRouter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register router and supporting services
        services.AddScoped<IProviderRouter, ProviderRouter>();
        services.AddScoped<IProviderHealthMonitor, ProviderHealthMonitor>();

        // Register rate limiters as singletons for shared state
        services.AddSingleton<ConcurrentDictionary<string, SlidingWindowRateLimiter>>();

        // Register activity source for tracing
        services.AddSingleton<ActivitySource>(
            new ActivitySource("Zarichney.Server.ProviderRouter"));

        return services;
    }
}
```

### 5.2 Configuration Validation

```csharp
/// <summary>
/// Validate routing configuration at startup.
/// </summary>
public class RoutingConfigurationValidator : IValidateOptions<RoutingConfig>
{
    public ValidateOptionsResult Validate(string name, RoutingConfig options)
    {
        var errors = new List<string>();

        // Validate load balancing strategy
        if (!Enum.IsDefined(typeof(LoadBalancingStrategy), options.LoadBalancing.Strategy))
        {
            errors.Add($"Invalid load balancing strategy: {options.LoadBalancing.Strategy}");
        }

        // Validate provider weights
        if (options.LoadBalancing.Strategy == LoadBalancingStrategy.WeightedRoundRobin)
        {
            var hasWeights = options.LoadBalancing.ProviderWeights?.Any() == true;
            if (!hasWeights)
            {
                errors.Add("Provider weights are required for weighted round-robin strategy");
            }
            else
            {
                var invalidWeights = options.LoadBalancing.ProviderWeights
                    .Where(w => w.Value <= 0)
                    .ToList();

                if (invalidWeights.Any())
                {
                    errors.Add($"Invalid provider weights (must be > 0): {string.Join(", ", invalidWeights.Select(w => w.Key))}");
                }
            }
        }

        // Validate rate limits
        if (options.LoadBalancing.EnableRateLimiting)
        {
            var invalidLimits = options.LoadBalancing.RequestsPerMinuteLimit?
                .Where(l => l.Value <= 0)
                .ToList();

            if (invalidLimits?.Any() == true)
            {
                errors.Add($"Invalid rate limits (must be > 0): {string.Join(", ", invalidLimits.Select(l => l.Key))}");
            }
        }

        return errors.Any()
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
```

## Implementation Guidelines

### Routing Strategy Design
- Implement multiple routing strategies for different use cases
- Support dynamic strategy switching based on configuration
- Consider request characteristics in routing decisions
- Maintain performance metrics for optimization decisions

### Health Integration
- Integrate tightly with provider health monitoring
- Respect circuit breaker states in routing decisions
- Support graceful degradation when providers fail
- Implement proactive health checking and recovery

### Performance Optimization
- Use efficient data structures for routing decisions
- Cache routing decisions where appropriate
- Minimize latency in provider selection
- Support concurrent routing decisions safely

### Testing Requirements
- Unit tests for all routing strategies
- Integration tests with mock health monitoring
- Performance tests for routing latency
- Chaos engineering tests for failover scenarios

---

**🗂️ WORKING DIRECTORY ARTIFACT CREATED:**
- **Filename:** components/configuration-router.md
- **Purpose:** Complete configuration router specification with intelligent routing, load balancing, and health monitoring
- **Context for Team:** Provider selection and routing logic implementation guide for CodeChanger
- **Dependencies:** Integrates with provider adapters, health monitoring, and configuration management
- **Next Actions:** All Epic #246 specifications complete - ready for CodeChanger implementation