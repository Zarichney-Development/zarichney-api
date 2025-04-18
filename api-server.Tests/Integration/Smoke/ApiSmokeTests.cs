using System.Net;
using Xunit;
using Zarichney.Client;
using Zarichney.Tests.Fixtures;
using Zarichney.Tests.Helpers;

namespace Zarichney.Tests.Integration.Smoke;

/// <summary>
/// Smoke tests for verifying that the most critical parts of the API are working.
/// These tests are lightweight and run quickly to provide confidence that the system is operational.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Smoke)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
[Trait(TestCategories.Dependency, TestCategories.ExternalStripe)]
public class ApiSmokeTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [DependencyFact]
    [Trait(TestCategories.Feature, TestCategories.Auth)]
    public async Task AuthFlow_Smoke_LoginAndLogout()
    {
        // Arrange
        var apiClient = Factory.CreateRefitClient();
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act - Login
        var loginResult = await apiClient.Login(loginRequest);
        
        // Create a new client with the token
        var httpClient = Factory.CreateAuthenticatedClient("test-user-id", new[] { "User" });
        var refitClient = Factory.CreateRefitClient(httpClient);
        
        // Act - Logout
        var logoutResult = await refitClient.Logout();

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
        var client = Factory.CreateAuthenticatedRefitClient(userId, roles);

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
        var httpClient = Factory.CreateAuthenticatedClient(userId, roles);

        // Act - Check that payment endpoints are accessible 
        var response = await httpClient.GetAsync("/api/payment/config");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}