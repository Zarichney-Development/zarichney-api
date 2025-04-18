using Refit;
using Xunit;
using Zarichney.Client;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Controllers.AuthController;

[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Feature, TestCategories.Auth)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
public class LoginEndpointsTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [DependencyFact]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var client = Factory.CreateRefitClient();
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await client.Login(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.Email);
    }

    [DependencyFact]
    public async Task Login_WithInvalidCredentials_ShouldFail()
    {
        // Arrange
        var client = Factory.CreateRefitClient();
        var request = new LoginRequest
        {
            Email = "invalid@example.com",
            Password = "WrongPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ApiException>(() => client.Login(request));
    }

    [DependencyFact]
    public async Task Login_WithEmptyEmail_ShouldFail()
    {
        // Arrange
        var client = Factory.CreateRefitClient();
        var request = new LoginRequest
        {
            Email = "",
            Password = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ApiException>(() => client.Login(request));
    }
}
