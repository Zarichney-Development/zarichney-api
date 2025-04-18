using Zarichney.Client;
using Zarichney.Tests.Fixtures;
using Xunit;

namespace Zarichney.Tests.Integration;

/// <summary>
/// Base class for integration tests that require database access.
/// Combines CustomWebApplicationFactory and DatabaseFixture to provide a complete test environment.
/// Tests using this class should be part of the "Database" collection.
/// </summary>
[Collection("Database")]
public abstract class DatabaseIntegrationTestBase : IntegrationTestBase
{
    protected readonly DatabaseFixture DatabaseFixture;
    protected readonly IZarichneyAPI ApiClient;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseIntegrationTestBase"/> class.
    /// </summary>
    /// <param name="factory">The web application factory.</param>
    /// <param name="databaseFixture">The database fixture.</param>
    protected DatabaseIntegrationTestBase(
        CustomWebApplicationFactory factory,
        DatabaseFixture databaseFixture) 
        : base(factory)
    {
        DatabaseFixture = databaseFixture;
        ApiClient = factory.CreateRefitClient();
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