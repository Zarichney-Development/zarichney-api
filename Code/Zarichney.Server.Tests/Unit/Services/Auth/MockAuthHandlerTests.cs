using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FluentAssertions;
using Zarichney.Config;
using Zarichney.Services.Auth;

namespace Zarichney.Server.Tests.Unit.Services.Auth;

/// <summary>
/// Unit tests for MockAuthHandler
/// </summary>
[Trait("Category", "Unit")]
public class MockAuthHandlerTests
{
  private readonly Mock<IOptionsMonitor<AuthenticationSchemeOptions>> _authOptionsMonitor;
  private readonly Mock<ILoggerFactory> _loggerFactory;
  private readonly Mock<UrlEncoder> _urlEncoder;
  private readonly Mock<IOptions<MockAuthConfig>> _mockAuthConfigOptions;
  private readonly MockAuthConfig _mockAuthConfig;

  public MockAuthHandlerTests()
  {
    _authOptionsMonitor = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
    _loggerFactory = new Mock<ILoggerFactory>();
    Mock<ILogger<MockAuthHandler>> logger = new();
    _urlEncoder = new Mock<UrlEncoder>();
    _mockAuthConfigOptions = new Mock<IOptions<MockAuthConfig>>();

    _mockAuthConfig = new MockAuthConfig
    {
      DefaultRoles = ["User", "Admin"],
      DefaultUsername = "TestMockUser",
      DefaultEmail = "test@mock.com",
      DefaultUserId = "test-mock-id"
    };

    _mockAuthConfigOptions.Setup(x => x.Value).Returns(_mockAuthConfig);
    _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

    _authOptionsMonitor.Setup(x => x.Get(It.IsAny<string>()))
      .Returns(new AuthenticationSchemeOptions());
  }

  [Fact]
  public async Task HandleAuthenticateAsync_DefaultConfiguration_ReturnsSuccessWithConfiguredClaims()
  {
    // Arrange
    var context = CreateHttpContext();
    var handler = CreateMockAuthHandler();
    await handler.InitializeAsync(new AuthenticationScheme("MockAuth", null, typeof(MockAuthHandler)), context);

    // Act
    var result = await handler.AuthenticateAsync();

    // Assert
    result.Should().NotBeNull();
    result.Succeeded.Should().BeTrue();
    result.Principal.Should().NotBeNull();
    result.Principal!.Identity!.IsAuthenticated.Should().BeTrue();

    // Verify claims
    var claims = result.Principal.Claims.ToList();
    claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == _mockAuthConfig.DefaultUserId);
    claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == _mockAuthConfig.DefaultUsername);
    claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == _mockAuthConfig.DefaultEmail);
    claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
    claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
  }

  [Fact]
  public async Task HandleAuthenticateAsync_WithMockRolesHeader_InDevelopment_UsesHeaderRoles()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Request.Headers["X-Mock-Roles"] = "TestRole1,TestRole2";

    var handler = CreateMockAuthHandler();
    await handler.InitializeAsync(new AuthenticationScheme("MockAuth", null, typeof(MockAuthHandler)), context);

    // Act
    var result = await handler.AuthenticateAsync();

    // Assert
    result.Should().NotBeNull();
    result.Succeeded.Should().BeTrue();
    result.Principal.Should().NotBeNull();

    // Verify role claims use header values instead of default config
    var roleClaims = result.Principal!.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
    roleClaims.Should().HaveCount(2);
    roleClaims.Should().Contain(c => c.Value == "TestRole1");
    roleClaims.Should().Contain(c => c.Value == "TestRole2");
    roleClaims.Should().NotContain(c => c.Value == "User");
    roleClaims.Should().NotContain(c => c.Value == "Admin");
  }

  [Fact]
  public async Task HandleAuthenticateAsync_WithMockRolesHeader_InProduction_IgnoresHeaderRoles()
  {
    // Arrange
    var context = CreateHttpContext("Production");
    context.Request.Headers["X-Mock-Roles"] = "TestRole1,TestRole2";

    var handler = CreateMockAuthHandler();
    await handler.InitializeAsync(new AuthenticationScheme("MockAuth", null, typeof(MockAuthHandler)), context);

    // Act
    var result = await handler.AuthenticateAsync();

    // Assert
    result.Should().NotBeNull();
    result.Succeeded.Should().BeTrue();
    result.Principal.Should().NotBeNull();

    // Verify role claims use default config values (header ignored in Production)
    var roleClaims = result.Principal!.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
    roleClaims.Should().HaveCount(2);
    roleClaims.Should().Contain(c => c.Value == "User");
    roleClaims.Should().Contain(c => c.Value == "Admin");
    roleClaims.Should().NotContain(c => c.Value == "TestRole1");
    roleClaims.Should().NotContain(c => c.Value == "TestRole2");
  }

  [Fact]
  public async Task HandleAuthenticateAsync_WithEmptyMockRolesHeader_UsesDefaultRoles()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Request.Headers["X-Mock-Roles"] = "";

    var handler = CreateMockAuthHandler();
    await handler.InitializeAsync(new AuthenticationScheme("MockAuth", null, typeof(MockAuthHandler)), context);

    // Act
    var result = await handler.AuthenticateAsync();

    // Assert
    result.Should().NotBeNull();
    result.Succeeded.Should().BeTrue();
    result.Principal.Should().NotBeNull();

    // Verify role claims use default config values
    var roleClaims = result.Principal!.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
    roleClaims.Should().HaveCount(2);
    roleClaims.Should().Contain(c => c.Value == "User");
    roleClaims.Should().Contain(c => c.Value == "Admin");
  }

  [Fact]
  public async Task HandleAuthenticateAsync_WithWhitespaceInMockRolesHeader_TrimsRoles()
  {
    // Arrange
    var context = CreateHttpContext();
    context.Request.Headers["X-Mock-Roles"] = " Role1 , Role2 , ";

    var handler = CreateMockAuthHandler();
    await handler.InitializeAsync(new AuthenticationScheme("MockAuth", null, typeof(MockAuthHandler)), context);

    // Act
    var result = await handler.AuthenticateAsync();

    // Assert
    result.Should().NotBeNull();
    result.Succeeded.Should().BeTrue();
    result.Principal.Should().NotBeNull();

    // Verify role claims are trimmed
    var roleClaims = result.Principal!.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
    roleClaims.Should().HaveCount(2);
    roleClaims.Should().Contain(c => c.Value == "Role1");
    roleClaims.Should().Contain(c => c.Value == "Role2");
  }

  private HttpContext CreateHttpContext(string environment = "Development")
  {
    var context = new DefaultHttpContext();

    // Mock IWebHostEnvironment
    var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
    mockWebHostEnvironment.Setup(x => x.EnvironmentName).Returns(environment);

    // Set up service provider
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton(mockWebHostEnvironment.Object);
    context.RequestServices = serviceCollection.BuildServiceProvider();

    return context;
  }

  private MockAuthHandler CreateMockAuthHandler()
  {
    return new MockAuthHandler(
      _authOptionsMonitor.Object,
      _loggerFactory.Object,
      _urlEncoder.Object,
      _mockAuthConfigOptions.Object);
  }
}
