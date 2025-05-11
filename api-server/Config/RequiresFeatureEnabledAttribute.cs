namespace Zarichney.Config;

/// <summary>
/// Attribute that indicates a controller or action requires specific feature(s) to be properly configured.
/// When applied, this attribute is used by Swagger to mark endpoints that may be unavailable due to
/// missing required configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequiresFeatureEnabledAttribute : Attribute
{
  /// <summary>
  /// Gets the features that are required for this controller or action to function.
  /// </summary>
  public Feature[] Features { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="RequiresFeatureEnabledAttribute"/> class.
  /// </summary>
  /// <param name="features">One or more features that are required for this controller or action to function.</param>
  /// <exception cref="ArgumentException">Thrown when no features are provided.</exception>
  public RequiresFeatureEnabledAttribute(params Feature[] features)
  {
    if (features == null || features.Length == 0)
    {
      throw new ArgumentException("At least one feature must be provided", nameof(features));
    }

    Features = features;
  }

  /// <summary>
  /// For backward compatibility and testing - gets feature names as strings.
  /// </summary>
  internal string[] FeatureNames => Features.Select(f => f.ToString()).ToArray();
}
