using Microsoft.Extensions.Configuration;

namespace Zarichney.Config;

/// <summary>
/// Extension methods for adding path transformation configuration.
/// </summary>
public static class PathTransformationConfigurationExtensions
{
  /// <summary>
  /// Adds a path transformation configuration source that transforms configuration paths
  /// with the specified prefix to absolute paths.
  /// </summary>
  /// <param name="builder">The configuration builder</param>
  /// <param name="baseConfiguration">The base configuration to transform</param>
  /// <param name="basePath">The absolute base path to prepend</param>
  /// <param name="prefix">The prefix to look for and replace</param>
  /// <returns>The configuration builder</returns>
  public static IConfigurationBuilder AddPathTransformation(
      this IConfigurationBuilder builder,
      IConfiguration baseConfiguration,
      string basePath,
      string prefix)
  {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentNullException.ThrowIfNull(baseConfiguration);
    ArgumentException.ThrowIfNullOrEmpty(basePath);
    ArgumentException.ThrowIfNullOrEmpty(prefix);

    return builder.Add(new PathTransformationConfigurationProvider(baseConfiguration, basePath, prefix));
  }
}

/// <summary>
/// Custom configuration provider that transforms configuration paths from relative
/// "Data/" prefixes to absolute paths for development and testing environments.
/// Eliminates the need for reflection-based configuration manipulation.
/// </summary>
public sealed class PathTransformationConfigurationProvider : ConfigurationProvider, IConfigurationSource
{
  private readonly IConfiguration _baseConfiguration;
  private readonly string _basePath;
  private readonly string _prefix;

  /// <summary>
  /// Initializes a new instance of the PathTransformationConfigurationProvider.
  /// </summary>
  /// <param name="baseConfiguration">The base configuration to transform paths from</param>
  /// <param name="basePath">The absolute base path to prepend to transformed paths</param>
  /// <param name="prefix">The prefix to look for and replace in configuration values (e.g., "Data/")</param>
  public PathTransformationConfigurationProvider(IConfiguration baseConfiguration, string basePath, string prefix)
  {
    ArgumentNullException.ThrowIfNull(baseConfiguration);
    ArgumentException.ThrowIfNullOrEmpty(basePath);
    ArgumentException.ThrowIfNullOrEmpty(prefix);

    _baseConfiguration = baseConfiguration;
    _basePath = basePath;
    _prefix = prefix;
  }

  /// <summary>
  /// Loads and transforms configuration paths that start with the specified prefix.
  /// </summary>
  public override void Load()
  {
    var transformedPaths = TransformConfigurationPaths(_baseConfiguration, _basePath, _prefix);
    Data = transformedPaths;
  }

  /// <summary>
  /// Builds and returns this provider instance for the configuration builder.
  /// </summary>
  public IConfigurationProvider Build(IConfigurationBuilder builder) => this;

  /// <summary>
  /// Transforms paths in configuration that start with a specified prefix.
  /// This method is designed to be reusable across both production and test environments.
  /// </summary>
  /// <param name="configuration">The configuration to scan for paths</param>
  /// <param name="basePath">The base path to prepend after removing the prefix</param>
  /// <param name="prefix">The prefix to look for and remove from paths (e.g., "Data/")</param>
  /// <returns>Dictionary of transformed paths with keys being the config keys and values being the transformed paths</returns>
  private static Dictionary<string, string?> TransformConfigurationPaths(
      IConfiguration configuration,
      string basePath,
      string prefix)
  {
    // Find all paths that start with the prefix
    var pathConfigs = configuration.AsEnumerable()
        .Where(kvp => kvp.Value?.StartsWith(prefix) == true)
        .ToList();

    // Create the transformed paths dictionary
    var transformedPaths = pathConfigs
        .Select(kvp => new KeyValuePair<string, string?>(
            kvp.Key,
            Path.Combine(basePath, kvp.Value![prefix.Length..])
        ))
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    return transformedPaths;
  }
}
