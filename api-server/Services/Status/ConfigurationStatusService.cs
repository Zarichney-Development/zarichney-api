using System.Reflection;
using Zarichney.Config;

namespace Zarichney.Services.Status;

/// <summary>
/// Service that reports the status of various application services based on their configuration.
/// </summary>
public interface IConfigurationStatusService
{
  /// <summary>
  /// Gets the status of all registered services and their configuration dependencies.
  /// </summary>
  /// <returns>A dictionary mapping service names to their status information.</returns>
  Task<Dictionary<string, ServiceStatusInfo>> GetServiceStatusAsync();

  /// <summary>
  /// Gets the cached status of a specific feature by name.
  /// This synchronous method is intended for use by Swagger filters.
  /// </summary>
  /// <param name="featureName">The name of the feature to check.</param>
  /// <returns>The service status information, or null if the feature is not found.</returns>
  ServiceStatusInfo? GetFeatureStatus(string featureName);

  /// <summary>
  /// Checks if a specific feature is available based on its configuration status.
  /// This synchronous method is intended for use by Swagger filters.
  /// </summary>
  /// <param name="featureName">The name of the feature to check.</param>
  /// <returns>True if the feature is properly configured and available; otherwise, false.</returns>
  bool IsFeatureAvailable(string featureName);
}

/// <summary>
/// Service that checks the status of various application services based on their configuration requirements.
/// </summary>
public class ConfigurationStatusService : IConfigurationStatusService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly IConfiguration _configuration;
  private readonly Assembly _assemblyToScan;
  private readonly ILogger<ConfigurationStatusService> _logger;

  // Cache for service status to avoid rechecking configuration on every Swagger request
  private Dictionary<string, ServiceStatusInfo>? _cachedStatus;
  private DateTime _cacheExpiration = DateTime.MinValue;
  private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
  private readonly SemaphoreSlim _cacheLock = new(1, 1);

  /// <summary>
  /// Initializes a new instance of the <see cref="ConfigurationStatusService"/> class.
  /// </summary>
  /// <param name="serviceProvider">The service provider to resolve IConfig instances.</param>
  /// <param name="configuration">The configuration to check values against.</param>
  /// <param name="logger">The logger.</param>
  public ConfigurationStatusService(
      IServiceProvider serviceProvider,
      IConfiguration configuration,
      ILogger<ConfigurationStatusService> logger)
      : this(serviceProvider, configuration, logger, Assembly.GetAssembly(typeof(Program))!) // Assuming Program is in the main assembly
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConfigurationStatusService"/> class for testing purposes.
  /// </summary>
  /// <param name="serviceProvider">The service provider to resolve IConfig instances.</param>
  /// <param name="configuration">The configuration to check values against.</param>
  /// <param name="logger">The logger.</param>
  /// <param name="assemblyToScan">The assembly to scan for IConfig implementations.</param>
  public ConfigurationStatusService(
      IServiceProvider serviceProvider,
      IConfiguration configuration,
      ILogger<ConfigurationStatusService> logger,
      Assembly assemblyToScan)
  {
    _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _assemblyToScan = assemblyToScan ?? throw new ArgumentNullException(nameof(assemblyToScan));
  }

  /// <inheritdoc />
  public async Task<Dictionary<string, ServiceStatusInfo>> GetServiceStatusAsync()
  {
    // Check if cache is valid
    if (_cachedStatus != null && DateTime.UtcNow < _cacheExpiration)
    {
      return _cachedStatus;
    }

    // Acquire lock to prevent multiple threads from refreshing the cache simultaneously
    await _cacheLock.WaitAsync();
    try
    {
      // Double-check cache after acquiring lock
      if (_cachedStatus != null && DateTime.UtcNow < _cacheExpiration)
      {
        return _cachedStatus;
      }

      _logger.LogInformation("Refreshing service configuration status cache");

      // Create a dictionary to store results
      var results = new Dictionary<string, ServiceStatusInfo>();

      // Get all types implementing IConfig (excluding interfaces/abstract classes)
      var configTypes = _assemblyToScan // Use the injected assembly
          .GetTypes()
          .Where(t => typeof(IConfig).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

      var configInstances = new List<IConfig>();
      foreach (var configTypeItem in configTypes)
      {
        // Resolve each config type from the service provider.
        // IConfig instances are registered as singletons.
        var configInstance = _serviceProvider.GetService(configTypeItem) as IConfig;
        if (configInstance != null)
        {
          configInstances.Add(configInstance);
        }
        else
        {
          _logger.LogDebug("Could not resolve config type {ConfigType} from service provider", configTypeItem.Name);
        }
      }

      // Process each config instance
      foreach (var config in configInstances)
      {
        // Determine service name from config class name
        // E.g., "ServerConfig" becomes "Server"
        var configType = config.GetType();
        var serviceName = configType.Name.Replace("Config", "");

        // Get missing configurations for this service
        var missingConfigurations = await CheckConfigurationRequirementsAsync(configType);

        // Add to results
        results[serviceName] = new ServiceStatusInfo(
            IsAvailable: missingConfigurations.Count == 0,
            MissingConfigurations: missingConfigurations
        );
      }

      // Update cache
      _cachedStatus = results;
      _cacheExpiration = DateTime.UtcNow.Add(_cacheDuration);

      return results;
    }
    finally
    {
      _cacheLock.Release();
    }
  }

  /// <inheritdoc />
  public ServiceStatusInfo? GetFeatureStatus(string featureName)
  {
    // Try to get from cache or refresh if needed
    var statusTask = GetServiceStatusAsync();

    // Wait for the task to complete (this is a synchronous method, so we need to block)
    // Note: In a production environment with high concurrency, consider
    // initializing the cache at startup and providing alternative non-blocking access
    try
    {
      statusTask.Wait();
      var status = statusTask.Result;

      // Check if the feature exists in our status dictionary
      if (status.TryGetValue(featureName, out var featureStatus))
      {
        return featureStatus;
      }

      _logger.LogWarning("Feature status requested for unknown feature: {FeatureName}. Available services: {AvailableServices}",
          featureName, string.Join(", ", status.Keys));
      return null;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting feature status for {FeatureName}", featureName);
      return null;
    }
  }

  /// <inheritdoc />
  public bool IsFeatureAvailable(string featureName)
  {
    var status = GetFeatureStatus(featureName);
    return status?.IsAvailable ?? false;
  }

  private Task<List<string>> CheckConfigurationRequirementsAsync(Type configType)
  {
    var missingConfigurations = new List<string>();

    // Find properties with RequiresConfiguration attribute
    var properties = configType.GetProperties()
        .Where(p => Attribute.IsDefined(p, typeof(RequiresConfigurationAttribute)));

    foreach (var property in properties)
    {
      var attribute = property.GetCustomAttribute<RequiresConfigurationAttribute>();
      if (attribute == null) continue;

      var configKey = attribute.ConfigurationKey;
      var configValue = _configuration[configKey];

      // Check if config is missing, empty, or using placeholder value
      if (string.IsNullOrWhiteSpace(configValue) ||
          configValue == StatusService.PlaceholderMessage)
      {
        missingConfigurations.Add(configKey);
      }
    }

    return Task.FromResult(missingConfigurations);
  }
}
