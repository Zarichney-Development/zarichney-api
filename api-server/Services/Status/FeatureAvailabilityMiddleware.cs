using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Http.Features;

namespace Zarichney.Services.Status;

/// <summary>
/// Middleware that checks if a requested endpoint requires specific features to be enabled,
/// and prevents access to the endpoint if any required feature is unavailable.
/// </summary>
public class FeatureAvailabilityMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<FeatureAvailabilityMiddleware> _logger;
  private readonly IStatusService _statusService;

  // Cache to avoid repeatedly extracting attributes for the same route
  private static readonly ConcurrentDictionary<string, List<ExternalServices>> AttributeCache = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="FeatureAvailabilityMiddleware"/> class.
  /// </summary>
  /// <param name="next">The next request delegate in the pipeline.</param>
  /// <param name="logger">The logger instance.</param>
  /// <param name="statusService">The service providing feature availability information.</param>
  public FeatureAvailabilityMiddleware(
    RequestDelegate next,
    ILogger<FeatureAvailabilityMiddleware> logger,
    IStatusService statusService)
  {
    _next = next ?? throw new ArgumentNullException(nameof(next));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
  }

  /// <summary>
  /// Processes the request, checking if the target endpoint has any feature requirements.
  /// If any required feature is unavailable, a ServiceUnavailableException is thrown with details.
  /// </summary>
  /// <param name="context">The HTTP context for the request.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  public async Task InvokeAsync(HttpContext context)
  {
    // Skip feature checks if we can't determine the endpoint (this happens before endpoint routing)
    var endpoint = context.GetEndpoint();
    if (endpoint == null)
    {
      await _next(context);
      return;
    }

    // Get a unique identifier for this endpoint - allows caching of attribute checks
    var endpointId = GetEndpointCacheKey(endpoint);

    // Check if the required features for this endpoint are available
    var requiredFeatures = GetRequiredFeatures(endpoint, endpointId);
    if (requiredFeatures.Count == 0)
    {
      // No feature requirements - proceed with the request
      await _next(context);
      return;
    }

    // Check each required feature's availability
    var unavailableFeatures = new Dictionary<ExternalServices, ServiceStatusInfo>();
    foreach (var feature in requiredFeatures)
    {
      var featureStatus = _statusService.GetFeatureStatus(feature);
      if (featureStatus == null || !featureStatus.IsAvailable)
      {
        unavailableFeatures[feature] = featureStatus ?? new ServiceStatusInfo(false, new List<string> { $"Unknown feature: {feature}" });
      }
    }

    if (unavailableFeatures.Count == 0)
    {
      await _next(context);
      return;
    }

    // At least one required feature is unavailable - collect all missing configurations
    var missingConfigurations = new List<string>();
    var unavailableFeaturesList = new List<string>();

    foreach (var (feature, statusInfo) in unavailableFeatures)
    {
      unavailableFeaturesList.Add(feature.ToString());

      if (statusInfo.MissingConfigurations != null && statusInfo.MissingConfigurations.Count > 0)
      {
        missingConfigurations.AddRange(statusInfo.MissingConfigurations);
      }
    }

    var displayPath = context.Request.Path.HasValue ? context.Request.Path.Value : "(unknown path)";
    _logger.LogWarning(
      "Request to {Path} requires features that are unavailable: {Features}. Missing configurations: {MissingConfigs}",
      displayPath,
      string.Join(", ", unavailableFeaturesList),
      string.Join(", ", missingConfigurations));

    throw new ServiceUnavailableException(
      $"This API endpoint requires features that are unavailable: {string.Join(", ", unavailableFeaturesList)}",
      missingConfigurations);

  }

  /// <summary>
  /// Gets a cache key for the endpoint.
  /// </summary>
  /// <param name="endpoint">The endpoint to identify.</param>
  /// <returns>A string that uniquely identifies the endpoint.</returns>
  private static string GetEndpointCacheKey(Endpoint endpoint)
    => endpoint.DisplayName ?? endpoint.GetType().Name;

  /// <summary>
  /// Gets all features required for an endpoint from both controller and action level attributes.
  /// </summary>
  /// <param name="endpoint">The endpoint to check.</param>
  /// <param name="cacheKey">A key to use for caching the results.</param>
  /// <returns>A list of all required features for the endpoint.</returns>
  private List<ExternalServices> GetRequiredFeatures(Endpoint endpoint, string cacheKey)
  {
    // Check if we have cached the attributes for this endpoint
    if (AttributeCache.TryGetValue(cacheKey, out var cachedFeatures))
    {
      return cachedFeatures;
    }

    var features = new List<ExternalServices>();

    // Get the endpoint metadata collection
    var metadata = endpoint.Metadata;
    if (metadata == null)
    {
      // Add empty list to cache to avoid repeated lookups
      AttributeCache[cacheKey] = features;
      return features;
    }

    // Extract all RequiresFeatureEnabled attributes from the endpoint metadata
    var featureAttributes = metadata.GetOrderedMetadata<RequiresFeatureEnabledAttribute>();
    if (featureAttributes != null && featureAttributes.Any())
    {
      foreach (var attribute in featureAttributes)
      {
        if (attribute.Features != null && attribute.Features.Length > 0)
        {
          features.AddRange(attribute.Features);
        }
      }
    }

    // Add to cache and return
    AttributeCache[cacheKey] = features;
    return features;
  }
}

/// <summary>
/// Extension methods for the <see cref="FeatureAvailabilityMiddleware"/>.
/// </summary>
public static class FeatureAvailabilityMiddlewareExtensions
{
  /// <summary>
  /// Adds the <see cref="FeatureAvailabilityMiddleware"/> to the application's request pipeline.
  /// </summary>
  /// <param name="app">The application builder.</param>
  /// <returns>The application builder for chaining.</returns>
  public static IApplicationBuilder UseFeatureAvailability(
    this IApplicationBuilder app)
  {
    return app.UseMiddleware<FeatureAvailabilityMiddleware>();
  }
}
