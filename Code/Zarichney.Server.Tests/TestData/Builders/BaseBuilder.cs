namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Base class for test data builders.
/// Provides a fluent interface for creating test data objects.
/// </summary>
/// <typeparam name="TBuilder">The builder type.</typeparam>
/// <typeparam name="TEntity">The entity type being built.</typeparam>
public abstract class BaseBuilder<TBuilder, TEntity>
    where TBuilder : BaseBuilder<TBuilder, TEntity>
    where TEntity : class, new()
{
  protected TEntity Entity { get; } = new();

  /// <summary>
  /// Creates a new instance of the entity with default values.
  /// </summary>
  /// <returns>A new instance of the entity.</returns>
  public TEntity Build()
  {
    return Entity;
  }

  /// <summary>
  /// Returns the builder for method chaining.
  /// </summary>
  /// <returns>The builder instance.</returns>
  protected TBuilder Self()
  {
    return (TBuilder)this;
  }
}
