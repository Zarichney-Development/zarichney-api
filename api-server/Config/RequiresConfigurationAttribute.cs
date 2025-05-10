namespace Zarichney.Config;

/// <summary>
/// Attribute that indicates a configuration property is required for a service to function.
/// This attribute is used to identify properties that should be checked for valid configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class RequiresConfigurationAttribute : Attribute
{
    /// <summary>
    /// Gets the configuration key path (e.g., "Section:Key").
    /// </summary>
    public string ConfigurationKey { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresConfigurationAttribute"/> class.
    /// </summary>
    /// <param name="configurationKey">The configuration key path (e.g., "Section:Key").</param>
    /// <exception cref="ArgumentException">Thrown when configurationKey is null or whitespace.</exception>
    public RequiresConfigurationAttribute(string configurationKey)
    {
        if (string.IsNullOrWhiteSpace(configurationKey))
        {
            throw new ArgumentException("Configuration key cannot be null or whitespace", nameof(configurationKey));
        }
        
        ConfigurationKey = configurationKey;
    }
}