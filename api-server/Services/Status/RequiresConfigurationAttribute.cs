namespace Zarichney.Services.Status;

/// <summary>
/// Attribute that indicates a configuration property is required for specific feature(s) to function.
/// This attribute is used to identify properties that should be checked for valid configuration
/// and associates them with the features they enable.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class RequiresConfigurationAttribute : Attribute
{
  /// <summary>
  /// Gets the features that depend on this configuration property.
  /// </summary>
  public ExternalServices[] Features { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="RequiresConfigurationAttribute"/> class.
  /// </summary>
  /// <param name="features">One or more features that depend on this configuration property.</param>
  /// <exception cref="ArgumentException">Thrown when no features are provided.</exception>
  public RequiresConfigurationAttribute(params ExternalServices[] features)
  {
    if (features == null || features.Length == 0)
    {
      throw new ArgumentException("At least one feature must be provided", nameof(features));
    }

    Features = features;
  }
}
