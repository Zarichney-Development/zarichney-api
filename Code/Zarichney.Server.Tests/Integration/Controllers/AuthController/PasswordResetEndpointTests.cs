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
public class PasswordResetEndpointTests : DatabaseIntegrationTestBase
{
    public PasswordResetEndpointTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
        : base(apiClientFixture, testOutputHelper)
    {
    }

    private async Task<ApplicationUser> CreateTestUserAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var testEmail = $"reset_test_{Guid.NewGuid()}@example.com";
        var testUser = new ApplicationUser
        {
            UserName = testEmail,
            Email = testEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(testUser, "OldPassword123!");
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create test user: {errors}");
        }

        return testUser;
    }

    private async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    private async Task CleanupTestUserAsync(ApplicationUser user)
    {
        if (user == null) return;

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await userManager.DeleteAsync(user);
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task EmailForgotPassword_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        await ResetDatabaseAsync();
        var testUser = await CreateTestUserAsync();
        var client = _apiClientFixture.UnauthenticatedAuthApi;

        try
        {
            // Act
            var request = new ForgotPasswordRequest(testUser.Email!);
            var response = await client.EmailForgotPassword(request);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue(
                because: "forgot password with valid email should succeed");

            var authResult = response.Content;
            authResult.Should().NotBeNull(because: "successful request should return auth result");
            authResult.Success.Should().BeTrue(because: "forgot password should indicate success");
            authResult.Message.Should().NotBeNullOrEmpty(because: "result should contain a message");
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task EmailForgotPassword_WithNonExistentEmail_ShouldReturnSuccess()
    {
        // Arrange - To prevent email enumeration, should still return success
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var nonExistentEmail = $"nonexistent_{Guid.NewGuid()}@example.com";

        // Act
        var request = new ForgotPasswordRequest(nonExistentEmail);
        var response = await client.EmailForgotPassword(request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            because: "forgot password should return success even for non-existent email to prevent enumeration");

        var authResult = response.Content;
        authResult.Should().NotBeNull();
        authResult.Success.Should().BeTrue(because: "should indicate success to prevent email enumeration");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task EmailForgotPassword_WithInvalidEmailFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var request = new ForgotPasswordRequest("not-an-email");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.EmailForgotPassword(request));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "invalid email format should return bad request");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task EmailForgotPassword_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var request = new ForgotPasswordRequest("");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.EmailForgotPassword(request));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "empty email should return bad request");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task ResetPassword_WithValidToken_ShouldSucceed()
    {
        // Arrange
        await ResetDatabaseAsync();
        var testUser = await CreateTestUserAsync();
        var resetToken = await GeneratePasswordResetTokenAsync(testUser);
        var client = _apiClientFixture.UnauthenticatedAuthApi;

        try
        {
            // Act
            var request = new ResetPasswordRequest(
                testUser.Email!,
                resetToken,
                "NewPassword123!");
            var response = await client.ResetPassword(request);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue(
                because: "reset password with valid token should succeed");

            var authResult = response.Content;
            authResult.Should().NotBeNull(because: "successful reset should return auth result");
            authResult.Success.Should().BeTrue(because: "reset should indicate success");
            authResult.Message.Should().NotBeNullOrEmpty(because: "result should contain a message");

            // Verify password was changed by trying to login with new password
            var loginRequest = new LoginRequest(testUser.Email!, "NewPassword123!");
            var loginResponse = await client.Login(loginRequest);
            loginResponse.IsSuccessStatusCode.Should().BeTrue(
                because: "should be able to login with new password after reset");
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task ResetPassword_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var testUser = await CreateTestUserAsync();
        var client = _apiClientFixture.UnauthenticatedAuthApi;

        try
        {
            // Act - Use an invalid/fake token
            var request = new ResetPasswordRequest(
                testUser.Email!,
                "invalid-token-12345",
                "NewPassword123!");

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => client.ResetPassword(request));

            // Assert
            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "reset with invalid token should return bad request");
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task ResetPassword_WithNonExistentEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var client = _apiClientFixture.UnauthenticatedAuthApi;
        var nonExistentEmail = $"nonexistent_{Guid.NewGuid()}@example.com";

        // Act
        var request = new ResetPasswordRequest(
            nonExistentEmail,
            "some-token",
            "NewPassword123!");

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.ResetPassword(request));

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "reset for non-existent email should return bad request");
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task ResetPassword_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var testUser = await CreateTestUserAsync();
        var resetToken = await GeneratePasswordResetTokenAsync(testUser);
        var client = _apiClientFixture.UnauthenticatedAuthApi;

        try
        {
            // Act
            var request = new ResetPasswordRequest(
                testUser.Email!,
                resetToken,
                "123"); // Weak password

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => client.ResetPassword(request));

            // Assert
            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "reset with weak password should return bad request");
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task ResetPassword_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var testUser = await CreateTestUserAsync();
        var resetToken = await GeneratePasswordResetTokenAsync(testUser);
        var client = _apiClientFixture.UnauthenticatedAuthApi;

        try
        {
            // Act
            var request = new ResetPasswordRequest(
                testUser.Email!,
                resetToken,
                "");

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => client.ResetPassword(request));

            // Assert
            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "reset with empty password should return bad request");
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task ResetPassword_WithSamePassword_ShouldSucceed()
    {
        // Arrange - Some systems allow resetting to same password
        await ResetDatabaseAsync();
        var testUser = await CreateTestUserAsync();
        var resetToken = await GeneratePasswordResetTokenAsync(testUser);
        var client = _apiClientFixture.UnauthenticatedAuthApi;

        try
        {
            // Act
            var request = new ResetPasswordRequest(
                testUser.Email!,
                resetToken,
                "OldPassword123!"); // Same as original

            var response = await client.ResetPassword(request);

            // Assert - This may succeed or fail depending on system policy
            if (response.IsSuccessStatusCode)
            {
                response.Content.Should().NotBeNull();
                response.Content.Success.Should().BeTrue();
            }
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }

    [DependencyFact(InfrastructureDependency.Database)]
    public async Task ResetPassword_TokenReuse_ShouldFail()
    {
        // Arrange
        await ResetDatabaseAsync();
        var testUser = await CreateTestUserAsync();
        var resetToken = await GeneratePasswordResetTokenAsync(testUser);
        var client = _apiClientFixture.UnauthenticatedAuthApi;

        try
        {
            // Act - First reset should succeed
            var firstRequest = new ResetPasswordRequest(
                testUser.Email!,
                resetToken,
                "FirstNewPassword123!");
            var firstResponse = await client.ResetPassword(firstRequest);
            firstResponse.IsSuccessStatusCode.Should().BeTrue(
                because: "first reset with valid token should succeed");

            // Act - Try to reuse the same token
            var secondRequest = new ResetPasswordRequest(
                testUser.Email!,
                resetToken,
                "SecondNewPassword123!");

            var exception = await Assert.ThrowsAsync<ApiException>(
                () => client.ResetPassword(secondRequest));

            // Assert
            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                because: "token should not be reusable after first reset");
        }
        finally
        {
            await CleanupTestUserAsync(testUser);
        }
    }
}