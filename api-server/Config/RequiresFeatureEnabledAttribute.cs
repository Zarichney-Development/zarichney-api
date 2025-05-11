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
  /// Gets the feature names that are required for this controller or action to function.
  /// These names should correspond to the keys used in IConfigurationStatusService.
  /// </summary>
  public string[] FeatureNames { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="RequiresFeatureEnabledAttribute"/> class.
  /// </summary>
  /// <param name="featureNames">One or more feature names that are required for this controller or action to function.</param>
  /// <exception cref="ArgumentException">Thrown when no feature names are provided.</exception>
  public RequiresFeatureEnabledAttribute(params string[] featureNames)
  {
    if (featureNames == null || featureNames.Length == 0)
    {
      throw new ArgumentException("At least one feature name must be provided", nameof(featureNames));
    }

    // Check for null or empty strings in the array
    for (var i = 0; i < featureNames.Length; i++)
    {
      if (string.IsNullOrWhiteSpace(featureNames[i]))
      {
        throw new ArgumentException($"Feature name at index {i} cannot be null or whitespace", nameof(featureNames));
      }
    }

    FeatureNames = featureNames;
  }
}
