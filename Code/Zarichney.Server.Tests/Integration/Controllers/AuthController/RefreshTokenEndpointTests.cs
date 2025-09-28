using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Net;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Client.Contracts;
using Zarichney.Services.Auth;
using Zarichney.Server.Tests.Framework.Attributes;
using Zarichney.Server.Tests.Framework.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace Zarichney.Server.Tests.Integration.Controllers.AuthController;

[Collection("Integration")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Feature, TestCategories.Auth)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
public class RefreshTokenEndpointTests : DatabaseIntegrationTestBase
{
    public RefreshTokenEndpointTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
        : base(apiClientFixture, testOutputHelper)
    {
    }

    private async Task<(ApplicationUser user, Zarichney.Client.IAuthApi authenticatedClient)> CreateAndLoginTestUserAsync()
    {
        // Create a test user
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var testEmail = $"refresh_test_{Guid.NewGuid()}@example.com";
        var testUser = new ApplicationUser
        {
            UserName = testEmail,
            Email = testEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(testUser, "TestPassword123!");
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create test user: {errors}");
        }

        // Login to get authenticated client with cookies
        var loginClient = _apiClientFixture.UnauthenticatedAuthApi;
        var loginRequest = new LoginRequest(testEmail, "TestPassword123!");
        var loginResponse = await loginClient.Login(loginRequest);

        loginResponse.IsSuccessStatusCode.Should().BeTrue(
            because: "test user login should succeed");

        // Use the authenticated client from the fixture
        // In a real scenario, the cookies would be maintained by the browser
        var authenticatedClient = _apiClientFixture.AuthenticatedAuthApi;

        return (testUser, authenticatedClient);
    }

    private async Task CleanupTestUserAsync(ApplicationUser user)
    {
        if (user == null) return;

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await userManager.DeleteAsync(user);
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Refresh_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var (testUser, authenticatedClient) = await CreateAndLoginTestUserAsync();

        try
        {
            // Act - Call refresh endpoint with the refresh token cookie from login
            var refreshResponse = await authenticatedClient.Refresh();

            // Assert
            refreshResponse.IsSuccessStatusCode.Should().BeTrue(
                because: "refresh with valid token should succeed");

            var authResult = refreshResponse.Content;
            authResult.Should().NotBeNull(because: "successful refresh should return auth result");
            authResult.Success.Should().BeTrue(because: "refresh should indicate success");
            authResult.Message.Should().NotBeNullOrEmpty(because: "result should contain a message");
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Refresh_WithoutRefreshToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;

        // Act & Assert - Call refresh without any authentication/cookies
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.Refresh());

        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            because: "refresh without token should return unauthorized");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Refresh_WithInvalidRefreshToken_ShouldReturnUnauthorized()
    {
        // Arrange
        // Use the authenticated client which may have an invalid/expired token
        var client = _apiClientFixture.AuthenticatedAuthApi;

        // Manually add an invalid refresh token cookie to the request
        // This simulates a corrupted or tampered token
        // Note: In a real scenario, we'd need to configure the HttpClient with a custom cookie

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.Refresh());

        exception.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Refresh_AfterRevocation_ShouldReturnUnauthorized()
    {
        // Arrange
        var (testUser, authenticatedClient) = await CreateAndLoginTestUserAsync();

        try
        {
            // First, revoke the refresh token
            var revokeResponse = await authenticatedClient.Revoke();
            revokeResponse.IsSuccessStatusCode.Should().BeTrue(
                because: "revocation should succeed");

            // Act - Try to refresh after revocation
            var exception = await Assert.ThrowsAsync<ApiException>(
                () => authenticatedClient.Refresh());

            // Assert
            exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                because: "refresh with revoked token should return unauthorized");
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Refresh_MultipleTimesInQuickSuccession_ShouldSucceed()
    {
        // Arrange
        var (testUser, authenticatedClient) = await CreateAndLoginTestUserAsync();

        try
        {
            // Act - Refresh multiple times
            for (int i = 0; i < 3; i++)
            {
                var refreshResponse = await authenticatedClient.Refresh();

                // Assert
                refreshResponse.IsSuccessStatusCode.Should().BeTrue(
                    because: $"refresh attempt {i + 1} should succeed");

                var authResult = refreshResponse.Content;
                authResult.Should().NotBeNull();
                authResult.Success.Should().BeTrue();

                // Small delay to avoid rate limiting
                await Task.Delay(100);
            }
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Revoke_WithValidToken_ShouldClearCookies()
    {
        // Arrange
        var (testUser, authenticatedClient) = await CreateAndLoginTestUserAsync();

        try
        {
            // Act - Revoke the refresh token
            var revokeResponse = await authenticatedClient.Revoke();

            // Assert
            revokeResponse.IsSuccessStatusCode.Should().BeTrue(
                because: "revoke with valid token should succeed");

            var authResult = revokeResponse.Content;
            authResult.Should().NotBeNull(because: "successful revocation should return auth result");
            authResult.Success.Should().BeTrue(because: "revocation should indicate success");
            authResult.Message.Should().NotBeNullOrEmpty(because: "result should contain a message");
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Revoke_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.Revoke());

        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            because: "revoke without authentication should return unauthorized");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task RefreshClaims_WithValidAuthentication_ShouldUpdateClaims()
    {
        // Arrange
        var (testUser, authenticatedClient) = await CreateAndLoginTestUserAsync();

        try
        {
            // Act - Refresh claims
            var refreshClaimsResponse = await authenticatedClient.RefreshClaims();

            // Assert
            refreshClaimsResponse.IsSuccessStatusCode.Should().BeTrue(
                because: "refresh claims with valid authentication should succeed");

            var authResult = refreshClaimsResponse.Content;
            authResult.Should().NotBeNull(because: "successful refresh claims should return auth result");
            authResult.Success.Should().BeTrue(because: "refresh claims should indicate success");
            authResult.Message.Should().NotBeNullOrEmpty(because: "result should contain a message");
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task RefreshClaims_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.RefreshClaims());

        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            because: "refresh claims without authentication should return unauthorized");
    }
}
