using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.Auth;
using Zarichney.Startup;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Services.Auth;

/// <summary>
/// Integration tests for mock authentication functionality in non-Production environments
/// when the Identity Database is unavailable
/// Note: These are configuration-level tests that don't require full application startup
/// </summary>
[Trait("Category", "Integration")]
public class MockAuthenticationTests
{

  // Note: This test class focuses on testing the AuthenticationStartup configuration logic
  // rather than full application integration, which would require complex DI setup
  // when mock authentication is enabled alongside Identity-dependent services.

  [Fact]
  public void MockAuthConfiguration_ShouldBeRegisteredWhenIdentityDbMissing()
  {
    // This test verifies that the ShouldUseMockAuthentication logic works correctly
    // by testing the AuthenticationStartup logic directly rather than trying to start the full app
    
    // Arrange - Development environment with missing DB connection
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
    
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        // No UserDatabase connection string - should trigger mock auth
        ["MockAuth:Enabled"] = "true",
        ["MockAuth:DefaultRoles:0"] = "User",
        ["MockAuth:DefaultUsername"] = "TestUser"
      })
      .Build();

    // Act - Test the ShouldUseMockAuthentication logic
    var shouldUseMock = TestableAuthenticationStartup.TestShouldUseMockAuthentication(
      mockEnvironment.Object, configuration);

    // Assert
    shouldUseMock.Should().BeTrue(because: "Development environment with missing DB should use mock auth");
  }

  [Fact]
  public void ShouldUseMockAuthentication_ProductionEnvironment_ReturnsFalse()
  {
    // Arrange - Production environment (should never use mock auth)
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");
    
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        // Even with missing DB connection and enabled flag, Production should not use mock auth
        ["MockAuth:Enabled"] = "true"
      })
      .Build();

    // Act
    var shouldUseMock = TestableAuthenticationStartup.TestShouldUseMockAuthentication(
      mockEnvironment.Object, configuration);

    // Assert
    shouldUseMock.Should().BeFalse(because: "Production environment should never use mock authentication");
  }

  [Fact]
  public void ShouldUseMockAuthentication_DevelopmentWithValidDB_ReturnsFalse()
  {
    // Arrange - Development environment with valid DB connection
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
    
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["MockAuth:Enabled"] = "true",
        [$"ConnectionStrings:{Zarichney.Services.Auth.UserDbContext.UserDatabaseConnectionName}"] = "valid-connection-string"
      })
      .Build();

    // Act
    var shouldUseMock = TestableAuthenticationStartup.TestShouldUseMockAuthentication(
      mockEnvironment.Object, configuration);

    // Assert
    shouldUseMock.Should().BeFalse(because: "Development environment with valid DB should use real authentication");
  }

  [Fact]
  public void ShouldUseMockAuthentication_StagingWithMissingDB_ReturnsTrue()
  {
    // Arrange - Staging environment (non-Production) with missing DB
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.Setup(x => x.EnvironmentName).Returns("Staging");
    
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        // No UserDatabase connection string - should trigger mock auth
        ["MockAuth:Enabled"] = "true"
      })
      .Build();

    // Act
    var shouldUseMock = TestableAuthenticationStartup.TestShouldUseMockAuthentication(
      mockEnvironment.Object, configuration);

    // Assert
    shouldUseMock.Should().BeTrue(because: "Staging environment with missing DB should use mock authentication");
  }

  [Fact]
  public void AddMockAuthentication_ConfiguresServices()
  {
    // Arrange - Set up services collection with required dependencies
    var services = new ServiceCollection();
    services.AddLogging();
    // Add base authentication services that AddMockAuthentication builds upon
    services.AddAuthentication();
    services.AddAuthorization();
    
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["MockAuth:DefaultRoles:0"] = "TestRole1",
        ["MockAuth:DefaultRoles:1"] = "TestRole2",
        ["MockAuth:DefaultUsername"] = "TestMockUser",
        ["MockAuth:DefaultEmail"] = "test@mock.com",
        ["MockAuth:DefaultUserId"] = "test-mock-id"
      })
      .Build();

    // Act - Configure mock authentication
    TestableAuthenticationStartup.TestAddMockAuthentication(services, configuration);

    // Assert - Verify services are registered
    var serviceProvider = services.BuildServiceProvider();
    
    var authenticationService = serviceProvider.GetService<Microsoft.AspNetCore.Authentication.IAuthenticationService>();
    authenticationService.Should().NotBeNull("because authentication service should be registered");
    
    var authorizationService = serviceProvider.GetService<Microsoft.AspNetCore.Authorization.IAuthorizationService>();
    authorizationService.Should().NotBeNull("because authorization service should be registered");
    
    // Verify MockAuthHandler is registered
    var mockAuthHandler = serviceProvider.GetService<MockAuthHandler>();
    mockAuthHandler.Should().NotBeNull("because MockAuthHandler should be registered");
    
    var mockAuthConfig = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<Zarichney.Config.MockAuthConfig>>();
    mockAuthConfig.Should().NotBeNull("because MockAuthConfig should be registered");
    mockAuthConfig!.Value.DefaultRoles.Should().Contain("TestRole1");
    mockAuthConfig.Value.DefaultRoles.Should().Contain("TestRole2");
  }

  [Theory]
  [InlineData("Development", "", true, true)]
  [InlineData("Development", null, true, true)]
  [InlineData("Staging", "", true, true)]
  [InlineData("Testing", null, true, true)]
  [InlineData("Production", "", true, false)]
  [InlineData("Production", null, true, false)]
  [InlineData("Development", "valid-connection", true, false)]
  [InlineData("Development", "", false, false)]
  [InlineData("Development", null, false, false)]
  public void ShouldUseMockAuthentication_VariousScenarios_ReturnsExpectedResult(
    string environment, string? connectionString, bool mockAuthEnabled, bool expectedResult)
  {
    // Arrange
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.Setup(x => x.EnvironmentName).Returns(environment);
    
    var configValues = new Dictionary<string, string?>
    {
      ["MockAuth:Enabled"] = mockAuthEnabled.ToString()
    };
    
    if (connectionString != null)
    {
      configValues[$"ConnectionStrings:{Zarichney.Services.Auth.UserDbContext.UserDatabaseConnectionName}"] = connectionString;
    }
    
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(configValues)
      .Build();

    // Act
    var result = TestableAuthenticationStartup.TestShouldUseMockAuthentication(
      mockEnvironment.Object, configuration);

    // Assert
    result.Should().Be(expectedResult, 
      because: $"Environment '{environment}' with connection '{connectionString ?? "null"}' and MockAuth:Enabled={mockAuthEnabled} should {(expectedResult ? "" : "not ")}use mock auth");
  }

  // Note: The MockAuthHandler unit tests already verify header processing behavior
  // These integration tests focus on the AuthenticationStartup configuration logic
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