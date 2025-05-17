using Xunit.Abstractions;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration;

/// <summary>
/// Base class for integration tests that require database access.
/// Combines CustomWebApplicationFactory, DatabaseFixture, and ApiClientFixture to provide a complete test environment.
/// Tests using this class should be part of the "Integration" collection.
/// </summary>
public abstract class DatabaseIntegrationTestBase : IntegrationTestBase
{
  private DatabaseFixture DatabaseFixture => _apiClientFixture.DatabaseFixture;
  private readonly ApiClientFixture _apiClientFixture;

  /// <summary>
  /// Initializes a new instance of the <see cref="DatabaseIntegrationTestBase"/> class.
  /// </summary>
  /// <param name="apiClientFixture">The API client fixture.</param>
  /// <param name="testOutputHelper"></param>
  protected DatabaseIntegrationTestBase(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
    : base(apiClientFixture, testOutputHelper)
  {
    _apiClientFixture = apiClientFixture;
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
