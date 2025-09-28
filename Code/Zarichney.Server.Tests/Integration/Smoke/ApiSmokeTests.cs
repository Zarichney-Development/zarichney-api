using FluentAssertions;
using Refit;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Client.Contracts;
using Zarichney.Server.Tests.Framework.Attributes;
using Zarichney.Server.Tests.Framework.Fixtures;
using Zarichney.Services.Status;

namespace Zarichney.Tests.Integration.Smoke;

/// <summary>
/// Smoke tests for verifying that the most critical parts of the API are working.
/// These tests are lightweight and run quickly to provide confidence that the system is operational.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Category, TestCategories.Smoke)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
[Trait(TestCategories.Dependency, TestCategories.Docker)]
[Trait(TestCategories.Dependency, TestCategories.ExternalStripe)]
[Collection("IntegrationQA")]
public class ApiSmokeTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(apiClientFixture, testOutputHelper)
{
  [DependencyFact(InfrastructureDependency.Database)]
  [Trait(TestCategories.Feature, TestCategories.Auth)]
  public async Task AuthFlow_Smoke_LoginAndLogout()
  {

    // Arrange
    var apiClient = _apiClientFixture.UnauthenticatedAuthApi;
    var loginRequest = new LoginRequest("test@email.com", "Password123!");

    // Act - Login
    var loginResponse = await apiClient.Login(loginRequest);
    var loginResult = loginResponse.Content!;

    // Create authenticated Refit client and Logout
    var authenticatedClient = _apiClientFixture.AuthenticatedAuthApi;
    var logoutResponse = await authenticatedClient.Logout();
    var logoutResult = logoutResponse.Content!;

    // Assert
    Assert.True(loginResult.Success);
    Assert.NotEmpty(loginResult.Email ?? string.Empty);
    Assert.True(logoutResult.Success);
  }

  [DependencyFact(InfrastructureDependency.Database)]
  [Trait(TestCategories.Feature, TestCategories.Cookbook)]
  public async Task Cookbook_Smoke_GetRecipes()
  {

    // Arrange
    var client = _apiClientFixture.AuthenticatedCookbookApi;

    // Act
    var recipes = await client.Recipe("burger", false, default(int?), default(int?));

    // Assert
    Assert.NotNull(recipes);
  }

  [DependencyFact(Zarichney.Services.Status.ExternalServices.Stripe)]
  [Trait(TestCategories.Feature, TestCategories.Payment)]
  public async Task Payment_Smoke_ApiEndpointsAvailable()
  {

    // This test just verifies that the payment endpoints are accessible
    // It doesn't test actual payment processing

    // Arrange
    var apiClient = _apiClientFixture.AuthenticatedApiApi;

    // Act & Assert - Secure endpoint should be accessible
    var act = () => apiClient.Secure();
    await act.Should().NotThrowAsync<ApiException>(
      because: "secure endpoint should be accessible");
  }
}
