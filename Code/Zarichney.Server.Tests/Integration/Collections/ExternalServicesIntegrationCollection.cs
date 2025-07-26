using Xunit;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Server.Tests.Integration.Collections;

/// <summary>
/// Integration test collection for external service integrations (AI, Payment, etc.).
/// This collection uses its own isolated fixtures to prevent mock service conflicts
/// and external API rate limiting issues during parallel execution.
/// </summary>
[CollectionDefinition("IntegrationExternal")]
public class ExternalServicesIntegrationCollection : ICollectionFixture<ApiClientFixture>
{
}