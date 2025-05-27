using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Net;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Client.Contracts;
using Zarichney.Services.Auth;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Controllers.AuthController;

[Collection("Integration")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Feature, TestCategories.Auth)]
// Using the infrastructure trait for XUnit test filtering - DependencyFact will check the actual dependency
[Trait(TestCategories.Dependency, TestCategories.Database)]
public class LoginEndpointsTests : DatabaseIntegrationTestBase
{
  public LoginEndpointsTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
    : base(apiClientFixture, testOutputHelper)
  {
  }

  private async Task SeedTestUserAsync()
  {
    // Get UserManager from the test factory - using ApplicationUser
    using var scope = Factory.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
    
    // Check if user already exists
    var existingUser = await userManager.FindByEmailAsync("test@example.com");
    if (existingUser != null)
    {
      // User already exists, no need to create again
      return;
    }
    
    // Create test user using ApplicationUser
    var testUser = new ApplicationUser
    {
      UserName = "test@example.com",
      Email = "test@example.com",
      EmailConfirmed = true
    };
    
    var result = await userManager.CreateAsync(testUser, "TestPassword123!");
    if (!result.Succeeded)
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Description));
      throw new InvalidOperationException($"Failed to create test user: {errors}");
    }
    
    // Verify user was created successfully and check password
    var createdUser = await userManager.FindByEmailAsync("test@example.com");
    if (createdUser == null)
    {
      throw new InvalidOperationException("Test user was not found after creation");
    }
    
    // Verify password is correctly set
    var passwordCheck = await userManager.CheckPasswordAsync(createdUser, "TestPassword123!");
    if (!passwordCheck)
    {
      throw new InvalidOperationException("Test user password verification failed after creation");
    }
  }
  [Fact]
  public async Task Login_WithValidCredentials_ShouldSucceed()
  {
    // Arrange - Reset database and seed test user
    await ResetDatabaseAsync();
    await SeedTestUserAsync();
    
    var client = _apiClientFixture.UnauthenticatedAuthApi;
    var request = new LoginRequest("test@example.com", "TestPassword123!");

    // Act
    var loginResponse = await client.Login(request);
    
    // Assert - Follow testing standards for API response handling
    loginResponse.IsSuccessStatusCode.Should().BeTrue(
      because: $"login with valid credentials should succeed, but got {loginResponse.StatusCode} with error: {loginResponse.Error?.Content}");
    
    loginResponse.Content.Should().NotBeNull(because: "successful login response should contain auth result data");
    
    var authResult = loginResponse.Content!;
    authResult.Success.Should().BeTrue(because: "auth result should indicate successful login");
    authResult.Email.Should().NotBeNullOrEmpty(because: "auth result should contain the user's email");
    authResult.Email.Should().Be("test@example.com", because: "auth result should contain the correct user email");
    authResult.Message.Should().NotBeNullOrEmpty(because: "auth result should contain a success message");
  }

  [DependencyFact(InfrastructureDependency.Database)]
  public async Task Login_WithInvalidCredentials_ShouldFail()
  {
    // Arrange
    var client = _apiClientFixture.UnauthenticatedAuthApi;
    var request = new LoginRequest("invalid@example.com", "WrongPassword123!");

    // Act & Assert
    await Assert.ThrowsAsync<ApiException>(() => client.Login(request));
  }

  [DependencyFact(InfrastructureDependency.Database)]
  public async Task Login_WithEmptyEmail_ShouldFail()
  {
    // Arrange
    var client = _apiClientFixture.UnauthenticatedAuthApi;
    var request = new LoginRequest("", "Password123!");

    // Act & Assert
    await Assert.ThrowsAsync<ApiException>(() => client.Login(request));
  }
}
