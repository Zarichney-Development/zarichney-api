using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using FluentAssertions;
using Zarichney.Services.Auth;
using Zarichney.Startup;

namespace Zarichney.Tests.Unit.Startup;

/// <summary>
/// Unit tests for AuthenticationStartup logic regarding mock authentication
/// </summary>
[Trait("Category", "Unit")]
public class AuthenticationStartupTests
{
  [Theory]
  [InlineData("Development", "", true, true)]
  [InlineData("Development", null, true, true)]
  [InlineData("Staging", "", true, true)]
  [InlineData("Staging", null, true, true)]
  [InlineData("Production", "", true, false)]
  [InlineData("Production", null, true, false)]
  [InlineData("Development", "valid-connection-string", true, false)]
  [InlineData("Staging", "valid-connection-string", true, false)]
  [InlineData("Production", "valid-connection-string", true, false)]
  [InlineData("Development", "", false, false)]
  [InlineData("Development", null, false, false)]
  [InlineData("Staging", "", false, false)]
  [InlineData("Staging", null, false, false)]
  public void ShouldUseMockAuthentication_VariousScenarios_ReturnsExpectedResult(
    string environment,
    string? connectionString,
    bool mockAuthEnabled,
    bool expectedResult)
  {
    // Arrange
    var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
    mockWebHostEnvironment.Setup(x => x.EnvironmentName).Returns(environment);

    var configValues = new List<KeyValuePair<string, string?>>
    {
      new("MockAuth:Enabled", mockAuthEnabled.ToString())
    };

    if (connectionString != null)
    {
      configValues.Add(new KeyValuePair<string, string?>($"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}", connectionString));
    }

    var configurationBuilder = new ConfigurationBuilder();
    configurationBuilder.AddInMemoryCollection(configValues);
    var configuration = configurationBuilder.Build();

    // Act
    var result = TestableAuthenticationStartup.TestShouldUseMockAuthentication(
      mockWebHostEnvironment.Object,
      configuration);

    // Assert
    result.Should().Be(expectedResult,
      because: $"Environment '{environment}' with connection string '{connectionString ?? "null"}' and MockAuth:Enabled={mockAuthEnabled} should {(expectedResult ? "" : "not ")}use mock authentication");
  }

  [Fact]
  public void AddMockAuthentication_ConfiguresExpectedServices()
  {
    // Arrange
    var services = new ServiceCollection();

    // Add logging and authentication services that mock auth depends on
    services.AddLogging();
    // Add base authentication services that AddMockAuthentication builds upon
    services.AddAuthentication();
    services.AddAuthorization();

    var configurationBuilder = new ConfigurationBuilder();
    configurationBuilder.AddInMemoryCollection([
      new KeyValuePair<string, string?>("MockAuth:DefaultRoles:0", "User"),
      new KeyValuePair<string, string?>("MockAuth:DefaultRoles:1", "Admin"),
      new KeyValuePair<string, string?>("MockAuth:DefaultUsername", "TestUser"),
      new KeyValuePair<string, string?>("MockAuth:DefaultEmail", "test@example.com"),
      new KeyValuePair<string, string?>("MockAuth:DefaultUserId", "test-id")
    ]);
    var configuration = configurationBuilder.Build();

    // Act
    TestableAuthenticationStartup.TestAddMockAuthentication(services, configuration);

    // Assert
    var serviceProvider = services.BuildServiceProvider();

    // Verify authentication service is registered (from AddAuthentication())
    var authenticationService = serviceProvider.GetService<IAuthenticationService>();
    authenticationService.Should().NotBeNull("because authentication service should be registered");

    // Verify authorization service is registered (from AddAuthorization())
    var authorizationService = serviceProvider.GetService<Microsoft.AspNetCore.Authorization.IAuthorizationService>();
    authorizationService.Should().NotBeNull("because authorization service should be registered");

    // Verify MockAuthHandler is registered
    var mockAuthHandler = serviceProvider.GetService<MockAuthHandler>();
    mockAuthHandler.Should().NotBeNull("because MockAuthHandler should be registered");

    // Verify MockAuthConfig is configured
    var mockAuthConfig = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<Zarichney.Config.MockAuthConfig>>();
    mockAuthConfig.Should().NotBeNull("because MockAuthConfig should be configured");
    mockAuthConfig.Value.DefaultRoles.Should().Contain("User");
    mockAuthConfig.Value.DefaultRoles.Should().Contain("Admin");
    mockAuthConfig.Value.DefaultUsername.Should().Be("TestUser");
    mockAuthConfig.Value.DefaultEmail.Should().Be("test@example.com");
    mockAuthConfig.Value.DefaultUserId.Should().Be("test-id");
  }
}

/// <summary>
/// Testable wrapper for AuthenticationStartup to expose private methods for testing
/// </summary>
public static class TestableAuthenticationStartup
{
  /// <summary>
  /// Exposes the private ShouldUseMockAuthentication method for testing
  /// </summary>
  public static bool TestShouldUseMockAuthentication(IWebHostEnvironment environment, IConfiguration configuration)
  {
    // Use reflection to call the private method
    var type = typeof(AuthenticationStartup);
    var method = type.GetMethod("ShouldUseMockAuthentication",
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

    if (method == null)
      throw new InvalidOperationException("ShouldUseMockAuthentication method not found");

    return (bool)method.Invoke(null, [environment, configuration])!;
  }

  /// <summary>
  /// Exposes the private AddMockAuthentication method for testing
  /// </summary>
  public static void TestAddMockAuthentication(IServiceCollection services, IConfiguration configuration)
  {
    // Use reflection to call the private method
    var type = typeof(AuthenticationStartup);
    var method = type.GetMethod("AddMockAuthentication",
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

    if (method == null)
      throw new InvalidOperationException("AddMockAuthentication method not found");

    method.Invoke(null, [services, configuration]);
  }
}
