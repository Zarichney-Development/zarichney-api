using System.Reflection;
using Zarichney.Config;
using Zarichney.Services.AI;
using Zarichney.Services.Auth;
using Zarichney.Services.Email;
using Zarichney.Services.GitHub;
using Zarichney.Services.Payment;

namespace Zarichney.Services.Status;

/// <summary>
/// Service that reports the status of various application services and configuration items.
/// </summary>
public interface IStatusService
{
  /// <summary>
  /// Gets the status of specific configuration items like API keys, secrets, etc.
  /// </summary>
  /// <returns>A list of status objects for each configuration item.</returns>
  Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync();

  /// <summary>
  /// Gets the status of all registered services and their configuration dependencies.
  /// </summary>
  /// <returns>A dictionary mapping service names to their status information.</returns>
  Task<Dictionary<ExternalServices, ServiceStatusInfo>> GetServiceStatusAsync();

  /// <summary>
  /// Gets the cached status of a specific feature by enum value.
  /// This synchronous method is intended for use by Swagger filters.
  /// </summary>
  /// <param name="externalService">The feature to check.</param>
  /// <returns>The service status information, or null if the feature is not found.</returns>
  ServiceStatusInfo? GetFeatureStatus(ExternalServices externalService);

  /// <summary>
  /// Checks if a specific feature is available based on its configuration status.
  /// This synchronous method is intended for use by Swagger filters.
  /// </summary>
  /// <param name="service">The feature to check.</param>
  /// <returns>True if the feature is properly configured and available; otherwise, false.</returns>
  bool IsFeatureAvailable(ExternalServices service);
  
  /// <summary>
  /// Sets the availability status of a service directly.
  /// This is used for services like the Identity database that are checked at startup.
  /// </summary>
  /// <param name="service">The service to set the status for.</param>
  /// <param name="isAvailable">Whether the service is available or not.</param>
  /// <param name="missingConfigurations">Optional list of missing configuration items that make the service unavailable.</param>
  void SetServiceAvailability(ExternalServices service, bool isAvailable, List<string>? missingConfigurations = null);
}

/// <summary>
/// Service that provides comprehensive status information about application configuration and feature availability.
/// </summary>
public class StatusService : IStatusService
{
  public const string PlaceholderMessage = "recommended to set in app secrets";

  private readonly LlmConfig _llmConfig;
  private readonly EmailConfig _emailConfig;
  private readonly GitHubConfig _gitHubConfig;
  private readonly PaymentConfig _paymentConfig;
  private readonly IConfiguration _configuration;
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<StatusService> _logger;
  private readonly Assembly _assemblyToScan;

  // Cache for service status to avoid rechecking configuration on every Swagger request
  private Dictionary<ExternalServices, ServiceStatusInfo>? _cachedStatus;

  // Additional mapping from Feature enum to service names
  private Dictionary<ExternalServices, ExternalServices[]>? _featureServiceMap;
  private DateTime _cacheExpiration = DateTime.MinValue;
  private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
  private readonly SemaphoreSlim _cacheLock = new(1, 1);

  /// <summary>
  /// Initializes a new instance of the <see cref="StatusService"/> class.
  /// </summary>
  /// <param name="llmConfig">The LLM configuration.</param>
  /// <param name="emailConfig">The email configuration.</param>
  /// <param name="gitHubConfig">The GitHub configuration.</param>
  /// <param name="paymentConfig">The payment configuration.</param>
  /// <param name="configuration">The configuration to check values against.</param>
  /// <param name="serviceProvider">The service provider to resolve IConfig instances.</param>
  /// <param name="logger">The logger.</param>
  public StatusService(
    LlmConfig llmConfig,
    EmailConfig emailConfig,
    GitHubConfig gitHubConfig,
    PaymentConfig paymentConfig,
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<StatusService> logger)
    : this(llmConfig, emailConfig, gitHubConfig, paymentConfig, configuration, serviceProvider, logger,
      Assembly.GetAssembly(typeof(Program))!) // Assuming Program is in the main assembly
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="StatusService"/> class for testing purposes.
  /// </summary>
  /// <param name="llmConfig">The LLM configuration.</param>
  /// <param name="emailConfig">The email configuration.</param>
  /// <param name="gitHubConfig">The GitHub configuration.</param>
  /// <param name="paymentConfig">The payment configuration.</param>
  /// <param name="configuration">The configuration to check values against.</param>
  /// <param name="serviceProvider">The service provider to resolve IConfig instances.</param>
  /// <param name="logger">The logger.</param>
  /// <param name="assemblyToScan">The assembly to scan for IConfig implementations.</param>
  public StatusService(
    LlmConfig llmConfig,
    EmailConfig emailConfig,
    GitHubConfig gitHubConfig,
    PaymentConfig paymentConfig,
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<StatusService> logger,
    Assembly assemblyToScan)
  {
    _llmConfig = llmConfig ?? throw new ArgumentNullException(nameof(llmConfig));
    _emailConfig = emailConfig ?? throw new ArgumentNullException(nameof(emailConfig));
    _gitHubConfig = gitHubConfig ?? throw new ArgumentNullException(nameof(gitHubConfig));
    _paymentConfig = paymentConfig ?? throw new ArgumentNullException(nameof(paymentConfig));
    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _assemblyToScan = assemblyToScan ?? throw new ArgumentNullException(nameof(assemblyToScan));
  }

  /// <summary>
  /// Checks the status of a specific configuration item.
  /// </summary>
  /// <param name="itemName">The display name of the item to check.</param>
  /// <param name="itemValue">The value of the item to check.</param>
  /// <param name="propertyName">The name of the property being checked (for error messages).</param>
  /// <returns>A status object for the configuration item.</returns>
  private ConfigurationItemStatus CheckConfigurationItem(string itemName, string? itemValue, string propertyName)
  {
    if (!string.IsNullOrWhiteSpace(itemValue) && itemValue != PlaceholderMessage)
      return new ConfigurationItemStatus(itemName, "Configured");

    return new ConfigurationItemStatus(itemName, "Missing/Invalid", $"{propertyName} is missing or placeholder");
  }

  /// <inheritdoc />
  public Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync()
  {
    var statusList = new List<ConfigurationItemStatus>
    {
      // OpenAI
      CheckConfigurationItem("OpenAI API Key", _llmConfig.ApiKey, nameof(_llmConfig.ApiKey)),

      // Email
      CheckConfigurationItem("Email AzureTenantId", _emailConfig.AzureTenantId, nameof(_emailConfig.AzureTenantId)),
      CheckConfigurationItem("Email AzureAppId", _emailConfig.AzureAppId, nameof(_emailConfig.AzureAppId)),
      CheckConfigurationItem("Email AzureAppSecret", _emailConfig.AzureAppSecret, nameof(_emailConfig.AzureAppSecret)),
      CheckConfigurationItem("Email FromEmail", _emailConfig.FromEmail, nameof(_emailConfig.FromEmail)),
      CheckConfigurationItem("Email MailCheckApiKey", _emailConfig.MailCheckApiKey,
        nameof(_emailConfig.MailCheckApiKey)),

      // GitHub
      CheckConfigurationItem("GitHub Access Token", _gitHubConfig.AccessToken, nameof(_gitHubConfig.AccessToken)),

      // Stripe
      CheckConfigurationItem("Stripe Secret Key", _paymentConfig.StripeSecretKey,
        nameof(_paymentConfig.StripeSecretKey)),
      CheckConfigurationItem("Stripe Webhook Secret", _paymentConfig.StripeWebhookSecret,
        nameof(_paymentConfig.StripeWebhookSecret)),

      // Identity Database Connection
      CheckConfigurationItem("Identity Database Connection", _configuration.GetConnectionString(UserDbContext.UserDatabaseConnectionName),
        UserDbContext.UserDatabaseConnectionName)
    };

    return Task.FromResult(statusList);
  }

  /// <inheritdoc />
  public void SetServiceAvailability(ExternalServices service, bool isAvailable, List<string>? missingConfigurations = null)
  {
    // Acquire lock first to prevent race conditions with other cache updates
    _cacheLock.Wait();
    try
    {
      // Initialize the cache if needed
      if (_cachedStatus == null)
      {
        _cachedStatus = new Dictionary<ExternalServices, ServiceStatusInfo>();
        _featureServiceMap = new Dictionary<ExternalServices, ExternalServices[]>();
      }
      
      // Create or update the feature mapping (we need it for consistency)
      if (!_featureServiceMap!.ContainsKey(service))
      {
        _featureServiceMap[service] = [service];
      }

      // Create or update the service status
      _cachedStatus[service] = new ServiceStatusInfo(
        serviceName: service,
        IsAvailable: isAvailable,
        MissingConfigurations: missingConfigurations ?? new List<string>()
      );
      
      // Update the cache expiration
      _cacheExpiration = DateTime.UtcNow.Add(_cacheDuration);
      
      _logger.LogInformation(
        "Service {Service} availability set to {Available} {Reason}", 
        service, 
        isAvailable ? "Available" : "Unavailable",
        !isAvailable && missingConfigurations?.Any() == true 
          ? $"due to missing: {string.Join(", ", missingConfigurations)}" 
          : string.Empty
      );
    }
    finally
    {
      _cacheLock.Release();
    }
  }

  public async Task<Dictionary<ExternalServices, ServiceStatusInfo>> GetServiceStatusAsync()
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

      // Initialize or preserve existing cache
      var results = _cachedStatus ?? new Dictionary<ExternalServices, ServiceStatusInfo>();
      var featureMap = _featureServiceMap ?? new Dictionary<ExternalServices, List<ExternalServices>>();

      // Get all types implementing IConfig (excluding interfaces/abstract classes)
      var configTypes = _assemblyToScan // Use the injected assembly
        .GetTypes()
        .Where(t => typeof(IConfig).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

      var configInstances = new List<IConfig>();
      foreach (var configTypeItem in configTypes)
      {
        // Resolve each config type from the service provider.
        // IConfig instances are registered as singletons.
        if (_serviceProvider.GetService(configTypeItem) is IConfig configInstance)
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
        var configType = config.GetType();

        // Get missing configurations and feature associations for this service
        var (missingConfigurations, featureAssociations) = await CheckConfigurationRequirementsAsync(configType);

        // Group configurations by feature from the ExternalServicesEnum
        foreach (var (feature, properties) in featureAssociations)
        {
          // Skip the PostgresIdentityDb as it's handled separately by the SetServiceAvailability method
          if (feature == ExternalServices.PostgresIdentityDb)
          {
            continue;
          }
          
          // Use the enum name as the service name

          // Determine if this feature's required configurations are available
          var featureMissingConfigs = properties
            .Where(prop => missingConfigurations.Contains(prop))
            .ToList();

          // Add or update this feature's status in results
          if (results.TryGetValue(feature, out var existingStatus))
          {
            // If we already have an entry for this feature, merge the missing configurations
            var combinedMissing = existingStatus.MissingConfigurations.ToList();
            combinedMissing.AddRange(featureMissingConfigs);

            results[feature] = new ServiceStatusInfo(
              serviceName: feature,
              IsAvailable: combinedMissing.Count == 0,
              MissingConfigurations: combinedMissing.Distinct().ToList()
            );
          }
          else
          {
            // First entry for this feature
            results[feature] = new ServiceStatusInfo(
              serviceName: feature,
              IsAvailable: featureMissingConfigs.Count == 0,
              MissingConfigurations: featureMissingConfigs
            );
          }

          // Update feature mapping
          if (!featureMap.TryGetValue(feature, out var services))
          {
            services = [];
            featureMap[feature] = services;
          }

          if (!services.Contains(feature))
          {
            services.Add(feature);
          }
        }
      }

      // Convert feature map to service array dictionary
      _featureServiceMap = featureMap.ToDictionary(
        kvp => kvp.Key,
        kvp => kvp.Value.ToArray()
      );

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
  public ServiceStatusInfo? GetFeatureStatus(ExternalServices externalService)
  {
    try
    {
      EnsureCacheIsValid();

      // Direct lookup for the service status (this covers PostgresIdentityDb and others set directly)
      if (_cachedStatus != null && _cachedStatus.TryGetValue(externalService, out var directStatus))
      {
        return directStatus;
      }
      
      // For indirectly mapped services
      if (_featureServiceMap == null || !_featureServiceMap.TryGetValue(externalService, out var serviceNames) ||
          _cachedStatus == null)
      {
        _logger.LogWarning("Feature status requested for feature: {Feature}, but no associated services found",
          externalService);
        return null;
      }

      // Check if all required services are available
      var missingConfigurations = new List<string>();
      foreach (var serviceName in serviceNames)
      {
        if (_cachedStatus.TryGetValue(serviceName, out var serviceStatus) && !serviceStatus.IsAvailable)
        {
          missingConfigurations.AddRange(serviceStatus.MissingConfigurations);
        }
      }

      return new ServiceStatusInfo(
        serviceName: externalService,
        IsAvailable: missingConfigurations.Count == 0,
        MissingConfigurations: missingConfigurations
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting feature status for {Feature}", externalService);
      return null;
    }
  }

  /// <inheritdoc />
  public bool IsFeatureAvailable(ExternalServices externalServices)
  {
    var status = GetFeatureStatus(externalServices);
    return status?.IsAvailable ?? false;
  }

  private void EnsureCacheIsValid()
  {
    // If cache is not valid, refresh it synchronously
    if (_cachedStatus == null || DateTime.UtcNow >= _cacheExpiration)
    {
      GetServiceStatusAsync().Wait();
    }
  }

  private Task<(List<string> MissingConfigurations, Dictionary<ExternalServices, List<string>> FeatureAssociations)>
    CheckConfigurationRequirementsAsync(Type configType)
  {
    var missingConfigurations = new List<string>();
    var featureAssociations = new Dictionary<ExternalServices, List<string>>();

    // Find properties with RequiresConfiguration attribute
    var properties = configType.GetProperties()
      .Where(p => Attribute.IsDefined(p, typeof(RequiresConfigurationAttribute)));

    foreach (var property in properties)
    {
      var attribute = property.GetCustomAttribute<RequiresConfigurationAttribute>();
      if (attribute == null) continue;

      // Derive configuration key from containing type and property name
      var configKey = $"{configType.Name}:{property.Name}";
      var configValue = _configuration[configKey];

      // Check if config is missing, empty, or using placeholder value
      var isConfigMissing = string.IsNullOrWhiteSpace(configValue) || configValue == PlaceholderMessage;

      if (isConfigMissing)
      {
        missingConfigurations.Add(configKey);
      }

      // Associate this property with its dependent features
      foreach (var feature in attribute.Features)
      {
        if (!featureAssociations.TryGetValue(feature, out var propertyList))
        {
          propertyList = new List<string>();
          featureAssociations[feature] = propertyList;
        }

        if (!propertyList.Contains(configKey))
        {
          propertyList.Add(configKey);
        }
      }
    }

    return Task.FromResult((missingConfigurations, featureAssociations));
  }
}
