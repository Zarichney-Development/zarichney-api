using Xunit;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration;

/// <summary>
/// Base class for integration tests that require database access.
/// Combines CustomWebApplicationFactory and DatabaseFixture to provide a complete test environment.
/// Tests using this class should be part of the "Integration Tests" collection.
/// </summary>
public abstract class DatabaseIntegrationTestBase : IntegrationTestBase, IClassFixture<DatabaseFixture>, IClassFixture<ApiClientFixture>
{
  protected readonly DatabaseFixture DatabaseFixture;

  /// <summary>
  /// Initializes a new instance of the <see cref="DatabaseIntegrationTestBase"/> class.
  /// </summary>
  /// <param name="factory">The web application factory.</param>
  /// <param name="databaseFixture">The database fixture.</param>
  /// <param name="apiClientFixture">The API client fixture.</param>
  protected DatabaseIntegrationTestBase(
    CustomWebApplicationFactory factory,
    DatabaseFixture databaseFixture,
    ApiClientFixture apiClientFixture)
    : base(factory, apiClientFixture)
  {
    DatabaseFixture = databaseFixture;
    if (!DatabaseFixture.IsContainerAvailable)
    {
        // Skip all database-backed tests if container is unavailable
        SetSkipReason("Database unavailable, skipping database-backed integration tests.");
    }
  }

  /// <summary>
  /// Resets the database to a clean state.
  /// Call this at the beginning of each test that requires a clean database.
  /// </summary>
  protected async Task ResetDatabaseAsync()
  {
    await DatabaseFixture.ResetDatabaseAsync();
  }
}