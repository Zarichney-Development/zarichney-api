using Xunit;
using Zarichney.Server.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Collections;

/// <summary>
/// Integration test collection for quality assurance tests (Smoke, Performance).
/// This collection handles cross-cutting quality tests that validate overall
/// system behavior and can run independently during parallel execution.
/// </summary>
[CollectionDefinition("IntegrationQA")]
public class QualityAssuranceIntegrationCollection : ICollectionFixture<ApiClientFixture>
{
}
