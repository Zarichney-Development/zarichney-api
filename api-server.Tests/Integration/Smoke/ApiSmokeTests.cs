using FluentAssertions;
using Refit;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Client;
using Zarichney.Client.Contracts;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Smoke;

/// <summary>
/// Smoke tests for verifying that the most critical parts of the API are working.
/// These tests are lightweight and run quickly to provide confidence that the system is operational.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Smoke)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
[Trait(TestCategories.Dependency, TestCategories.Docker)]
[Trait(TestCategories.Dependency, TestCategories.ExternalStripe)]
[Collection("Integration")]
public class ApiSmokeTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(apiClientFixture, testOutputHelper)
{
  [DependencyFact]
  [Trait(TestCategories.Feature, TestCategories.Auth)]
  public async Task AuthFlow_Smoke_LoginAndLogout()
  {
    // Set a skip reason for the test to be properly handled
    SetSkipReason("Test requires authentication service which is missing configuration");

    // Arrange
    var apiClient = _apiClientFixture.UnauthenticatedAuthApi;
    var loginRequest = new LoginRequest
    {
      Email = "zarichney@gmail.com",
      Password = "Hershey6"
    };

    // Act - Login
    var loginResponse = await apiClient.Login(loginRequest);
    var loginResult = loginResponse.Content;

    // Create authenticated Refit client and Logout
    var authenticatedClient = _apiClientFixture.AuthenticatedAuthApi;
    var logoutResponse = await authenticatedClient.Logout();
    var logoutResult = logoutResponse.Content;

    // Assert
    Assert.True(loginResult.Success);
    Assert.NotEmpty(loginResult.Email);
    Assert.True(logoutResult.Success);
  }

  [Fact(Skip = "Skipping to keep runtime time low. TODO: remove after support test specific data")]
  [Trait(TestCategories.Feature, TestCategories.Cookbook)]
  public async Task Cookbook_Smoke_GetRecipes()
  {
    // Set a skip reason for the test to be properly handled
    SetSkipReason("Test requires cookbook service which is missing configuration");

    // Arrange
    var client = _apiClientFixture.AuthenticatedCookbookApi;

    // Act
    var recipes = await client.Recipe("burger", false, null, null);

    // Assert
    Assert.NotNull(recipes);
  }

  [DependencyFact]
  [Trait(TestCategories.Feature, TestCategories.Payment)]
  public async Task Payment_Smoke_ApiEndpointsAvailable()
  {
    // Set a skip reason for the test to be properly handled
    SetSkipReason("Test requires payment service which is missing configuration");

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
