using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Xunit;
using Zarichney.ApiClient.Configuration;
using Zarichney.ApiClient.Interfaces;

namespace Zarichney.ApiClient.Tests.Integration;

/// <summary>
/// Integration tests for PublicApi client.
/// These tests verify that the API client can successfully communicate with a live server.
/// 
/// NOTE: These tests require a running instance of the api-server to work properly.
/// For now, they are marked with Skip to avoid failures in CI environments.
/// TODO: Once Zarichney.TestingFramework is available (GH-14), update these to use proper test server infrastructure.
/// </summary>
public class PublicApiClientTests
{
  [Fact(Skip = "Requires running api-server instance. TODO: Use test server once Zarichney.TestingFramework is available")]
  public async Task PublicApi_Health_ShouldReturnSuccessfully()
  {
    // Arrange
    var services = new ServiceCollection();
    services.ConfigureRefitClients();
    var serviceProvider = services.BuildServiceProvider();

    var publicApi = serviceProvider.GetRequiredService<IPublicApi>();

    // Act & Assert
    var act = () => publicApi.Health();
    await act.Should().NotThrowAsync<ApiException>("health endpoint should be accessible");
  }

  [Fact]
  public void PublicApi_CanBeResolvedFromDI()
  {
    // Arrange
    var services = new ServiceCollection();
    services.ConfigureRefitClients();
    var serviceProvider = services.BuildServiceProvider();

    // Act
    var publicApi = serviceProvider.GetService<IPublicApi>();

    // Assert
    publicApi.Should().NotBeNull("IPublicApi should be resolvable from DI container");
  }
}
