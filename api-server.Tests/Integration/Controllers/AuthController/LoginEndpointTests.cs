using Refit;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Client.Contracts;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Controllers.AuthController;

[Collection("Integration")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Feature, TestCategories.Auth)]
// Using the infrastructure trait for XUnit test filtering - DependencyFact will check the actual dependency
[Trait(TestCategories.Dependency, TestCategories.Database)]
public class LoginEndpointsTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
    : DatabaseIntegrationTestBase(apiClientFixture, testOutputHelper)
{
  // Using the new InfrastructureDependency enum instead of Trait + empty DependencyFact
  [DependencyFact(InfrastructureDependency.Database)]
  public async Task Login_WithValidCredentials_ShouldSucceed()
  {
    // Arrange
    var client = _apiClientFixture.UnauthenticatedAuthApi;
    // todo replace with the test user values that resides in config
    var request = new LoginRequest("zarichney@gmail.com", "password");

    // Act
    var loginResponse = await client.Login(request);
    var authResult = loginResponse.Content;

    // Assert
    Assert.True(authResult.Success);
    Assert.NotEmpty(authResult.Email);
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
