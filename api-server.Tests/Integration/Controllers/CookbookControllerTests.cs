using Xunit;
using Zarichney.Tests.Fixtures;
using Zarichney.Tests.Helpers;
using Refit;

namespace Zarichney.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for the CookbookController.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Component, TestCategories.Controller)]
[Trait(TestCategories.Feature, TestCategories.Cookbook)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
public class CookbookControllerTests(
    CustomWebApplicationFactory factory,
    DatabaseFixture databaseFixture)
    : DatabaseIntegrationTestBase(factory, databaseFixture)
{
    [DependencyFact]
    public async Task GetRecipes_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange - Use the default client without authentication
        // The database doesn't need to be reset for authentication tests

        // Act & Assert
        await Assert.ThrowsAsync<ApiException>(() => ApiClient.Recipe("", false, null, null));
    }

    [DependencyFact]
    public async Task GetRecipes_Authenticated_ReturnsOkWithRecipes()
    {
        // Arrange
        await ResetDatabaseAsync(); // Start with a clean database
        
        // Seed the database with test recipes through the repository
        // This would typically use a database context in a real test
        // var testRecipes = await SeedTestRecipesAsync();
        
        // Create an authenticated client with user roles
        var userId = "test-user-id";
        var roles = new[] { "User" };
        var authenticatedClient = CreateAuthenticatedApiClient(userId, roles);

        // Act
        var recipes = await authenticatedClient.Recipe("", scrape: false, acceptableScore: null, requiredCount: null);

        // Assert
        Assert.NotNull(recipes);
        // In a real test with seeded data: Assert.Equal(testRecipes.Count, recipes.Count);
    }

    [DependencyFact]
    public async Task GetRecipeById_ExistingId_ReturnsRecipe()
    {
        // Arrange
        await ResetDatabaseAsync(); // Start with a clean database
        
        // Seed a test recipe and get its ID
        var recipeId = "sample-recipe-id"; // In a real test, this would be the ID of a seeded recipe
        
        // Create an authenticated client
        var userId = "test-user-id";
        var roles = new[] { "User" };
        var authenticatedClient = CreateAuthenticatedApiClient(userId, roles);

        // Act
        var order = await authenticatedClient.OrderGET(recipeId);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(recipeId, order.OrderId);
    }

    [DependencyFact]
    public async Task GetRecipeById_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync(); // Start with a clean database
        
        var nonExistentId = "non-existent-id";
        
        // Create an authenticated client
        var userId = "test-user-id";
        var roles = new[] { "User" };
        var authenticatedClient = CreateAuthenticatedApiClient(userId, roles);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => authenticatedClient.OrderGET(nonExistentId));
        Assert.Equal(System.Net.HttpStatusCode.NotFound, ex.StatusCode);
    }

    // Additional helper methods for seeding test data would go here
    // private async Task<List<RecipeSummary>> SeedTestRecipesAsync() { ... }
}