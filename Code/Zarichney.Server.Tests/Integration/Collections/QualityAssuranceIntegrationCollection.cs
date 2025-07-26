using Xunit;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Server.Tests.Integration.Collections;

/// <summary>
/// Integration test collection for quality assurance tests (Smoke, Performance).
/// This collection handles cross-cutting quality tests that validate overall
/// system behavior and can run independently during parallel execution.
/// </summary>
[CollectionDefinition("IntegrationQA")]
public class QualityAssuranceIntegrationCollection : ICollectionFixture<ApiClientFixture>
{
}
