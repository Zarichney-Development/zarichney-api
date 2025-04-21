using Microsoft.Extensions.Configuration;
using Refit;
using Xunit;
using Zarichney.Client;
using Zarichney.Tests.Framework.Configuration;

namespace Zarichney.Tests.Framework.Fixtures;

/// <summary>
/// xUnit fixture for shared API client instances (unauthenticated and authenticated) using CustomWebApplicationFactory.
/// </summary>
public class ApiClientFixture : IAsyncLifetime
{
  private readonly CustomWebApplicationFactory _factory = new();
  private readonly IConfiguration _configuration = TestConfigurationHelper.GetConfiguration();

  public IZarichneyAPI UnauthenticatedClient { get; private set; } = null!;
  public IZarichneyAPI AuthenticatedClient { get; private set; } = null!;

  public async Task InitializeAsync()
  {
    // Create HTTP client and Refit client for unauthenticated calls
    var httpClient = _factory.CreateClient();
    UnauthenticatedClient = RestService.For<IZarichneyAPI>(httpClient);

    try
    {
        // Read test user credentials from configuration
        var username = _configuration["TestUser:Username"];
        var password = _configuration["TestUser:Password"];
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            // Perform login to obtain authentication cookies
            var authResponse = await UnauthenticatedClient.Login(new LoginRequest
            {
                Email = username,
                Password = password
            });
            AuthenticatedClient = authResponse.Success ? RestService.For<IZarichneyAPI>(httpClient) :
                // Fallback to unauthenticated client on login failure
                UnauthenticatedClient;
        }
        else
        {
            // No credentials provided: use unauthenticated client
            AuthenticatedClient = UnauthenticatedClient;
        }
    }
    catch
    {
        // On any error, fallback to unauthenticated client
        AuthenticatedClient = UnauthenticatedClient;
    }
  }

  public async Task DisposeAsync()
  {
    // Dispose the factory (which disposes the test server and container)
    await _factory.DisposeAsync();
  }
}