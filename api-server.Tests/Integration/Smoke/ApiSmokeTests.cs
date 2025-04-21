using FluentAssertions;
using Refit;
using Xunit;
using Zarichney.Client;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Smoke;

/// <summary>
/// Smoke tests for verifying that the most critical parts of the API are working.
/// These tests are lightweight and run quickly to provide confidence that the system is operational.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Smoke)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
[Trait(TestCategories.Dependency, TestCategories.ExternalStripe)]
[Collection("Integration Tests")]
public class ApiSmokeTests(CustomWebApplicationFactory factory, ApiClientFixture apiClientFixture) : IntegrationTestBase(factory, apiClientFixture)
{
  [DependencyFact]
  [Trait(TestCategories.Feature, TestCategories.Auth)]
  public async Task AuthFlow_Smoke_LoginAndLogout()
  {
    // Arrange
    var apiClient = ApiClient;
    var loginRequest = new LoginRequest
    {
      Email = "test@example.com",
      Password = "Password123!"
    };

    // Act - Login
    var loginResult = await apiClient.Login(loginRequest);

    // Create authenticated Refit client and Logout
    var authenticatedClient = AuthenticatedApiClient;
    var logoutResult = await authenticatedClient.Logout();

    // Assert
    Assert.True(loginResult.Success);
    Assert.NotEmpty(loginResult.Email);
    Assert.True(logoutResult.Success);
  }

  [DependencyFact]
  [Trait(TestCategories.Feature, TestCategories.Cookbook)]
  public async Task Cookbook_Smoke_GetRecipes()
  {
    // Arrange
    var userId = "test-user-id";
    var roles = new[] { "User" };
    var client = AuthenticatedApiClient;

    // Act
    var recipes = await client.Recipe("", false, null, null);

    // Assert
    Assert.NotNull(recipes);
  }

  [DependencyFact]
  [Trait(TestCategories.Feature, TestCategories.Payment)]
  public async Task Payment_Smoke_ApiEndpointsAvailable()
  {
    // This test just verifies that the payment endpoints are accessible
    // It doesn't test actual payment processing

    // Arrange
    var userId = "test-user-id";
    var roles = new[] { "User" };
    var apiClient = AuthenticatedApiClient;

    // Act & Assert - Secure endpoint should be accessible
    Func<Task> act = () => apiClient.Secure();
    await act.Should().NotThrowAsync<ApiException>(
      because: "secure endpoint should be accessible");
  }
}