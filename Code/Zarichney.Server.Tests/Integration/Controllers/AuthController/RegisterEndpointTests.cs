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

[Collection("IntegrationAuth")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Feature, TestCategories.Auth)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
public class RegisterEndpointTests : DatabaseIntegrationTestBase
{
    public RegisterEndpointTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
        : base(apiClientFixture, testOutputHelper)
    {
    }

    private async Task<bool> UserExistsAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        return user != null;
    }

    private async Task CleanupTestUserAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            await userManager.DeleteAsync(user);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Register_WithValidInput_ShouldCreateUser()
    {
        // Arrange
        await ResetDatabaseAsync();
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var testEmail = $"test_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(testEmail, "ValidPassword123!");

        try
        {
            // Act
            var registerResponse = await client.Register(request);

            // Assert - Follow testing standards for API response handling
            registerResponse.IsSuccessStatusCode.Should().BeTrue(
                because: "valid registration should succeed");

            var authResult = registerResponse.Content;
            authResult.Should().NotBeNull(because: "successful registration should return auth result");
            authResult.Success.Should().BeTrue(because: "registration should indicate success");
            authResult.Email.Should().Be(testEmail, because: "result should contain the registered email");
            authResult.Message.Should().NotBeNullOrEmpty(because: "result should contain a message");

            // Verify user was created in database
            var userExists = await UserExistsAsync(testEmail);
            userExists.Should().BeTrue(because: "user should be created in the database");
        }
        finally
        {
            // Cleanup
            await CleanupTestUserAsync(testEmail);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Register_WithExistingEmail_ShouldReturnConflict()
    {
        // Arrange
        await ResetDatabaseAsync();
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var testEmail = $"existing_{Guid.NewGuid()}@example.com";

        // First registration
        var firstRequest = new RegisterRequest(testEmail, "ValidPassword123!");
        await client.Register(firstRequest);

        try
        {
            // Act - Try to register with same email
            var secondRequest = new RegisterRequest(testEmail, "DifferentPassword123!");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                () => client.Register(secondRequest));

            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "duplicate email registration should return bad request");
        }
        finally
        {
            // Cleanup
            await CleanupTestUserAsync(testEmail);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var request = new RegisterRequest("invalid-email", "ValidPassword123!");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.Register(request));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "invalid email format should return bad request");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Register_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var request = new RegisterRequest("", "ValidPassword123!");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.Register(request));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "empty email should return bad request");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var testEmail = $"weak_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(testEmail, "123");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.Register(request));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "weak password should return bad request");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Register_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var testEmail = $"empty_pwd_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(testEmail, "");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.Register(request));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "empty password should return bad request");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Register_WithPasswordMissingUppercase_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var testEmail = $"no_upper_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(testEmail, "password123!");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.Register(request));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "password without uppercase should return bad request");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Register_WithPasswordMissingNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var testEmail = $"no_number_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(testEmail, "Password!");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.Register(request));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "password without number should return bad request");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Register_WithPasswordMissingSpecialChar_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var testEmail = $"no_special_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(testEmail, "Password123");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.Register(request));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "password without special character should return bad request");
    }
}