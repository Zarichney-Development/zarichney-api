using Zarichney.Services.Status;
using Zarichney.TestingFramework.Helpers;

namespace Zarichney.TestingFramework.TestData.Builders;

/// <summary>
/// Builder for creating test configuration item status objects.
/// Demonstrates the builder pattern for test data creation.
/// </summary>
public class ConfigurationItemStatusBuilder
{
  private string _name = GetRandom.String();
  private string _status = "Configured";
  private string? _message;

  /// <summary>
  /// Initializes a new instance of the <see cref="ConfigurationItemStatusBuilder"/> class.
  /// </summary>
  public ConfigurationItemStatusBuilder()
  {
  }

  /// <summary>
  /// Creates a new builder instance with random values.
  /// </summary>
  /// <returns>A new builder instance with random values.</returns>
  public static ConfigurationItemStatusBuilder CreateRandom()
  {
    return new ConfigurationItemStatusBuilder()
        .WithName(GetRandom.String());
  }

  /// <summary>
  /// Sets the name of the configuration item.
  /// </summary>
  /// <param name="name">The name to set.</param>
  /// <returns>The builder instance for method chaining.</returns>
  public ConfigurationItemStatusBuilder WithName(string name)
  {
    _name = name;
    return this;
  }

  /// <summary>
  /// Sets the status of the configuration item to "Configured".
  /// </summary>
  /// <returns>The builder instance for method chaining.</returns>
  public ConfigurationItemStatusBuilder AsConfigured()
  {
    _status = "Configured";
    _message = null;
    return this;
  }

  /// <summary>
  /// Sets the status of the configuration item to "Missing/Invalid" with an optional message.
  /// </summary>
  /// <param name="message">The optional message explaining why the item is missing or invalid.</param>
  /// <returns>The builder instance for method chaining.</returns>
  public ConfigurationItemStatusBuilder AsMissingOrInvalid(string? message = null)
  {
    _status = "Missing/Invalid";
    _message = message ?? "Item is missing or invalid";
    return this;
  }

  /// <summary>
  /// Builds a configuration item status object with the configured properties.
  /// </summary>
  /// <returns>A new configuration item status object.</returns>
  public ConfigurationItemStatus Build()
  {
    return new ConfigurationItemStatus(_name, _status, _message);
  }

  /// <summary>
  /// Builds a list of configuration item status objects.
  /// </summary>
  /// <param name="count">The number of objects to create.</param>
  /// <returns>A list of configuration item status objects.</returns>
  public List<ConfigurationItemStatus> BuildList(int count)
  {
    var list = new List<ConfigurationItemStatus>();
    for (var i = 0; i < count; i++)
    {
      list.Add(new ConfigurationItemStatusBuilder()
          .WithName($"{GetRandom.String()}_{i}")
          .Build());
    }
    return list;
  }

  /// <summary>
  /// Creates a new builder instance.
  /// </summary>
  /// <returns>A new builder instance.</returns>
  public static ConfigurationItemStatusBuilder Create()
  {
    return new ConfigurationItemStatusBuilder();
  }
}
