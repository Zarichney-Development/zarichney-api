using Xunit;
using Zarichney.Server.Tests.Framework.Fixtures;

namespace Zarichney.Server.Tests.Integration.Collections;

/// <summary>
/// Integration test collection for infrastructure and status tests.
/// This collection handles lightweight tests that don't require database access
/// and can run quickly without resource contention during parallel execution.
/// </summary>
[CollectionDefinition("IntegrationInfra")]
public class InfrastructureIntegrationCollection : ICollectionFixture<ApiClientFixture>
{
}
