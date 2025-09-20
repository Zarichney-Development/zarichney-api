using Xunit;
using Zarichney.Server.Tests.Framework.Fixtures;

namespace Zarichney.Server.Tests.Integration;

/// <summary>
/// Collection definition for all integration tests.
/// Provides shared instances of the required fixtures.
/// All integration tests should use [Collection("Integration")] to access these shared fixtures.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationCollection :
    ICollectionFixture<ApiClientFixture>
{
  // The ordering of fixture declarations matters:
  // 1. First DatabaseFixture - provides connection string if available
  // 2. Then CustomWebApplicationFactory - uses DatabaseFixture if available
  // 3. Finally ApiClientFixture - uses CustomWebApplicationFactory

  // This class has no code and is never created. Its purpose is simply
  // to be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
}
