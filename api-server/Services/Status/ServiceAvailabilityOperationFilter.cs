using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Zarichney.Services.Status;

/// <summary>
/// Swagger operation filter that detects controllers and actions with the [RequiresFeatureEnabled]
/// attribute and modifies their summary and description to indicate when they may be unavailable
/// due to missing configuration.
/// </summary>
public class ServiceAvailabilityOperationFilter : IOperationFilter
{
  private readonly IStatusService _statusService;
  private readonly ILogger<ServiceAvailabilityOperationFilter> _logger;

  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceAvailabilityOperationFilter"/> class.
  /// </summary>
  /// <param name="statusService">The service that provides configuration status information.</param>
  /// <param name="logger">The logger.</param>
  public ServiceAvailabilityOperationFilter(
    IStatusService statusService,
    ILogger<ServiceAvailabilityOperationFilter> logger)
  {
    _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <inheritdoc />
  public void Apply(OpenApiOperation? operation, OperationFilterContext? context)
  {
    if (operation == null || context == null)
    {
      return;
    }

    // Try to gather all RequiresFeatureEnabled attributes from both the method and its containing class
    var methodAttributes = context.MethodInfo
      .GetCustomAttributes<RequiresFeatureEnabledAttribute>(inherit: true)
      .ToList();

    var classAttributes = context.MethodInfo.DeclaringType?
      .GetCustomAttributes<RequiresFeatureEnabledAttribute>(inherit: true)
      .ToList() ?? [];

    // Combine all attributes
    var allAttributes = methodAttributes.Concat(classAttributes).ToList();

    if (allAttributes.Count == 0)
    {
      // No attributes found, so no modification needed
      return;
    }

    // Track all unavailable features
    var unavailableFeatures = new Dictionary<ExternalServices, ServiceStatusInfo>();

    // Check each feature from all attributes
    foreach (var attribute in allAttributes)
    {
      foreach (var feature in attribute.Features)
      {
        // Skip if we've already processed this feature
        if (unavailableFeatures.ContainsKey(feature))
        {
          continue;
        }

        // Get status for this feature from the status service
        var statusInfo = _statusService.GetFeatureStatus(feature);

        // If the status indicates the feature is unavailable, track it
        if (statusInfo is { IsAvailable: false })
        {
          _logger.LogDebug("API endpoint '{Method}' requires feature '{Feature}' which is unavailable",
            context.MethodInfo.Name, feature);
          unavailableFeatures[feature] = statusInfo;
        }
      }
    }

    // If any features are unavailable, modify the operation summary and description
    if (unavailableFeatures.Count > 0)
    {
      var allReasons = new List<string>();
      foreach (var (feature, statusInfo) in unavailableFeatures)
      {
        allReasons.Add(statusInfo.MissingConfigurations.Count > 0
          ? $"{feature} (Missing: {string.Join(", ", statusInfo.MissingConfigurations)})"
          : feature.ToString());
      }

      var warningPrefix = $"⚠️ (Unavailable: {string.Join("; ", allReasons)}) ";

      // Prepend the warning to the summary
      operation.Summary = string.IsNullOrEmpty(operation.Summary)
        ? warningPrefix.TrimEnd()
        : warningPrefix + operation.Summary;

      // Add more detailed explanation in the description
      var warningDetail =
        $"**This endpoint is currently unavailable** due to missing configuration for: {string.Join("; ", allReasons)}";

      operation.Description = string.IsNullOrEmpty(operation.Description)
        ? warningDetail
        : warningDetail + Environment.NewLine + Environment.NewLine + operation.Description;
    }
  }
}
