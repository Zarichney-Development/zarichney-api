using Xunit;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Server.Tests.Integration.Collections;

/// <summary>
/// Integration test collection for core API functionality (Cookbook, Public endpoints).
/// This collection focuses on internal API operations with isolated database access
/// to prevent data conflicts during parallel execution.
/// </summary>
[CollectionDefinition("IntegrationCore")]
public class CoreApiIntegrationCollection : ICollectionFixture<ApiClientFixture>
{
}
