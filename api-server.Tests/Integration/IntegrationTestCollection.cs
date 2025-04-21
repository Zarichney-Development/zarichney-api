using Zarichney.Tests.Framework.Fixtures;
using Xunit;

namespace Zarichney.Tests.Integration;

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<ApiClientFixture>
{
    // This class has no code and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
}