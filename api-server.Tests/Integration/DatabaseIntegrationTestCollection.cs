using Zarichney.Tests.Framework.Fixtures;
using Xunit;

namespace Zarichney.Tests.Integration;

[CollectionDefinition("Database Integration Tests")]
public class DatabaseIntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>, ICollectionFixture<ApiClientFixture>, ICollectionFixture<DatabaseFixture>
{
    // Shared fixtures for database-backed integration tests
}