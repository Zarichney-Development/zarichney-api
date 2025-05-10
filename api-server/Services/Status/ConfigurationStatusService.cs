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
}

/// <summary>
/// Service that checks the status of various application services based on their configuration requirements.
/// </summary>
public class ConfigurationStatusService : IConfigurationStatusService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly Assembly _assemblyToScan;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationStatusService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve IConfig instances.</param>
    /// <param name="configuration">The configuration to check values against.</param>
    public ConfigurationStatusService(IServiceProvider serviceProvider, IConfiguration configuration)
        : this(serviceProvider, configuration, Assembly.GetAssembly(typeof(Program))!) // Assuming Program is in the main assembly
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationStatusService"/> class for testing purposes.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve IConfig instances.</param>
    /// <param name="configuration">The configuration to check values against.</param>
    /// <param name="assemblyToScan">The assembly to scan for IConfig implementations.</param>
    public ConfigurationStatusService(IServiceProvider serviceProvider, IConfiguration configuration, Assembly assemblyToScan)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _assemblyToScan = assemblyToScan ?? throw new ArgumentNullException(nameof(assemblyToScan));
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, ServiceStatusInfo>> GetServiceStatusAsync()
    {
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
            // Optionally: Log if configInstance is null, though GetService returns null if not found.
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

        return results;
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
                configValue == "recommended to set in app secrets")
            {
                missingConfigurations.Add(configKey);
            }
        }

        return Task.FromResult(missingConfigurations);
    }
}
