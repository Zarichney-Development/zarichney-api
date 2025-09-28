using Microsoft.AspNetCore.Mvc.Testing;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Framework.Helpers;

/// <summary>
/// Extension methods for test helpers.
/// </summary>
public static class TestExtensions
{
  /// <summary>
  /// Creates an authenticated client from a WebApplicationFactory.
  /// </summary>
  /// <param name="factory">The WebApplicationFactory to create the client from.</param>
  /// <param name="userId">The user ID to authenticate as.</param>
  /// <param name="roles">The roles to assign to the user.</param>
  /// <returns>An HttpClient with authentication.</returns>
  public static HttpClient CreateAuthenticatedClient(
      this WebApplicationFactory<Program> factory, string userId, string[] roles)
  {
    if (factory is CustomWebApplicationFactory customFactory)
    {
      return customFactory.CreateAuthenticatedClient(userId, roles);
    }

    // Handle the case where the factory isn't a CustomWebApplicationFactory
    // This should never happen in our tests, but it's good to handle it gracefully
    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
        "Test", AuthTestHelper.GenerateTestToken(userId, roles));
    return client;
  }
}
