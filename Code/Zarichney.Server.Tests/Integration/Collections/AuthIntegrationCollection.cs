using Xunit;
using Zarichney.Server.Tests.Framework.Fixtures;

namespace Zarichney.Server.Tests.Integration.Collections;

/// <summary>
/// Integration test collection for authentication and identity-related tests.
/// This collection uses its own isolated database instance to prevent conflicts
/// with other parallel test collections during user creation and auth operations.
/// </summary>
[CollectionDefinition("IntegrationAuth")]
public class AuthIntegrationCollection : ICollectionFixture<ApiClientFixture>
{
}
