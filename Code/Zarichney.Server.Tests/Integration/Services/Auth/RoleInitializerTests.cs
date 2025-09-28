using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.Auth;
using Zarichney.Server.Tests.Framework.Attributes;
using Zarichney.Server.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Services.Auth;

/// <summary>
/// Integration tests for the RoleInitializer admin user seeding functionality.
/// These tests use Testcontainers to provide a real PostgreSQL database.
/// </summary>
[Collection("IntegrationAuth")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Feature, TestCategories.Auth)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
public class RoleInitializerTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
  : DatabaseIntegrationTestBase(apiClientFixture, testOutputHelper)
{
  [DependencyFact(InfrastructureDependency.Database)]
  public async Task SeedAdminUser_CreatesUser_WhenNotExists_And_AssignsAdminRole()
  {
    // Arrange
    await ResetDatabaseAsync();

    var testConfig = new Dictionary<string, string?>
    {
      { "DefaultAdminUser:Email", "test-admin@example.com" },
      { "DefaultAdminUser:UserName", "testadmin" },
      { "DefaultAdminUser:Password", "TestPassword123!" },
      { $"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}", "Server=localhost;Database=test;User Id=test;Password=test;" }
    };

    await using var factory = Factory.ReplaceService(services =>
    {
      // Override configuration to include test admin user settings
      services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
        .AddInMemoryCollection(testConfig)
        .Build());

      // Override environment to be Development (non-Production)
      services.AddSingleton<IWebHostEnvironment>(_ =>
        new TestWebHostEnvironment { EnvironmentName = "Development" });
    });

    using var scope = factory.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<RoleInitializer>>();

    // Ensure roles exist first
    await roleManager.EnsureRolesCreatedAsync();

    var roleInitializer = new RoleInitializer(
      scope.ServiceProvider,
      configuration,
      environment,
      logger);

    // Act
    await roleInitializer.StartAsync(CancellationToken.None);

    // Assert
    var createdUser = await userManager.FindByEmailAsync("test-admin@example.com");
    createdUser.Should().NotBeNull("the admin user should be created");
    createdUser.UserName.Should().Be("testadmin");
    createdUser.Email.Should().Be("test-admin@example.com");
    createdUser.EmailConfirmed.Should().BeTrue("email should be auto-confirmed in dev environment");

    // Verify user is in admin role
    var isInAdminRole = await userManager.IsInRoleAsync(createdUser, "admin");
    isInAdminRole.Should().BeTrue("the admin user should be assigned to the admin role");

    // Verify user can authenticate with the configured password
    var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
    var signInResult = await signInManager.CheckPasswordSignInAsync(createdUser, "TestPassword123!", false);
    signInResult.Succeeded.Should().BeTrue("the admin user should be able to authenticate with the configured password");
  }

  [DependencyFact(InfrastructureDependency.Database)]
  public async Task SeedAdminUser_IsIdempotent_WhenUserExists()
  {
    // Arrange
    await ResetDatabaseAsync();

    var testConfig = new Dictionary<string, string?>
    {
      { "DefaultAdminUser:Email", "existing-admin@example.com" },
      { "DefaultAdminUser:UserName", "existingadmin" },
      { "DefaultAdminUser:Password", "ExistingPassword123!" },
      { $"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}", "Server=localhost;Database=test;User Id=test;Password=test;" }
    };

    await using var factory = Factory.ReplaceService(services =>
    {
      services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
        .AddInMemoryCollection(testConfig)
        .Build());

      services.AddSingleton<IWebHostEnvironment>(_ =>
        new TestWebHostEnvironment { EnvironmentName = "Development" });
    });

    using var scope = factory.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<RoleInitializer>>();

    // Ensure roles exist first
    await roleManager.EnsureRolesCreatedAsync();

    // Pre-create the admin user
    var existingUser = new ApplicationUser
    {
      UserName = "existingadmin",
      Email = "existing-admin@example.com",
      EmailConfirmed = true
    };
    var createResult = await userManager.CreateAsync(existingUser, "ExistingPassword123!");
    createResult.Should().Be(IdentityResult.Success);

    var roleInitializer = new RoleInitializer(
      scope.ServiceProvider,
      configuration,
      environment,
      logger);

    // Act - Run initialization twice to test idempotency
    await roleInitializer.StartAsync(CancellationToken.None);
    await roleInitializer.StartAsync(CancellationToken.None);

    // Assert
    // Verify only one user exists with this email
    var users = userManager.Users.Where(u => u.Email == "existing-admin@example.com").ToList();
    users.Should().HaveCount(1, "no duplicate users should be created");

    var user = users.First();
    user.UserName.Should().Be("existingadmin");

    // Verify user is in admin role
    var isInAdminRole = await userManager.IsInRoleAsync(user, "admin");
    isInAdminRole.Should().BeTrue("the existing user should be assigned to the admin role");
  }

  [DependencyFact(InfrastructureDependency.Database)]
  public async Task SeedAdminUser_DoesNotRun_InProductionEnvironment()
  {
    // Arrange
    await ResetDatabaseAsync();

    var testConfig = new Dictionary<string, string?>
    {
      { "DefaultAdminUser:Email", "prod-admin@example.com" },
      { "DefaultAdminUser:UserName", "prodadmin" },
      { "DefaultAdminUser:Password", "ProdPassword123!" },
      { $"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}", "Server=localhost;Database=test;User Id=test;Password=test;" }
    };

    await using var factory = Factory.ReplaceService(services =>
    {
      services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
        .AddInMemoryCollection(testConfig)
        .Build());

      // Set environment to Production
      services.AddSingleton<IWebHostEnvironment>(_ =>
        new TestWebHostEnvironment { EnvironmentName = "Production" });
    });

    using var scope = factory.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<RoleInitializer>>();

    // Ensure roles exist first
    await roleManager.EnsureRolesCreatedAsync();

    var roleInitializer = new RoleInitializer(
      scope.ServiceProvider,
      configuration,
      environment,
      logger);

    // Act
    await roleInitializer.StartAsync(CancellationToken.None);

    // Assert
    var adminUser = await userManager.FindByEmailAsync("prod-admin@example.com");
    adminUser.Should().BeNull("admin user should not be created in Production environment");
  }

  [DependencyFact(InfrastructureDependency.Database)]
  public async Task SeedAdminUser_UsesFallback_WhenEmailConfigFromEmailProvided()
  {
    // Arrange
    await ResetDatabaseAsync();

    var testConfig = new Dictionary<string, string?>
    {
      // Missing DefaultAdminUser configuration but EmailConfig.FromEmail provided
      { "EmailConfig:FromEmail", "fallback@example.com" },
      { $"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}", "Server=localhost;Database=test;User Id=test;Password=test;" }
    };

    await using var factory = Factory.ReplaceService(services =>
    {
      services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
        .AddInMemoryCollection(testConfig)
        .Build());

      services.AddSingleton<IWebHostEnvironment>(_ =>
        new TestWebHostEnvironment { EnvironmentName = "Development" });
    });

    using var scope = factory.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<RoleInitializer>>();

    // Ensure roles exist first
    await roleManager.EnsureRolesCreatedAsync();

    var roleInitializer = new RoleInitializer(
      scope.ServiceProvider,
      configuration,
      environment,
      logger);

    // Act
    await roleInitializer.StartAsync(CancellationToken.None);

    // Assert
    var createdUser = await userManager.FindByEmailAsync("fallback@example.com");
    createdUser.Should().NotBeNull("admin user should be created using EmailConfig.FromEmail fallback");
    createdUser.UserName.Should().Be("fallback@example.com", "username should use email as fallback");
    createdUser.Email.Should().Be("fallback@example.com");
    createdUser.EmailConfirmed.Should().BeTrue("email should be auto-confirmed in dev environment");

    // Verify user is in admin role
    var isInAdminRole = await userManager.IsInRoleAsync(createdUser, "admin");
    isInAdminRole.Should().BeTrue("the fallback admin user should be assigned to the admin role");

    // Verify user can authenticate with fallback password
    var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
    var signInResult = await signInManager.CheckPasswordSignInAsync(createdUser, "nimda", false);
    signInResult.Succeeded.Should().BeTrue("the fallback admin user should authenticate with 'nimda' password");
  }

  [DependencyFact(InfrastructureDependency.Database)]
  public async Task SeedAdminUser_UsesDefaultFallback_WhenAllConfigMissing()
  {
    // Arrange
    await ResetDatabaseAsync();

    var testConfig = new Dictionary<string, string?>
    {
      // Missing both DefaultAdminUser and EmailConfig configuration
      { $"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}", "Server=localhost;Database=test;User Id=test;Password=test;" }
    };

    await using var factory = Factory.ReplaceService(services =>
    {
      services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
        .AddInMemoryCollection(testConfig)
        .Build());

      services.AddSingleton<IWebHostEnvironment>(_ =>
        new TestWebHostEnvironment { EnvironmentName = "Development" });
    });

    using var scope = factory.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<RoleInitializer>>();

    // Ensure roles exist first
    await roleManager.EnsureRolesCreatedAsync();

    var roleInitializer = new RoleInitializer(
      scope.ServiceProvider,
      configuration,
      environment,
      logger);

    // Act
    await roleInitializer.StartAsync(CancellationToken.None);

    // Assert
    var createdUser = await userManager.FindByEmailAsync("test@gmail.com");
    createdUser.Should().NotBeNull("admin user should be created using default fallback email");
    createdUser.UserName.Should().Be("test@gmail.com", "username should use default fallback email");
    createdUser.Email.Should().Be("test@gmail.com");
    createdUser.EmailConfirmed.Should().BeTrue("email should be auto-confirmed in dev environment");

    // Verify user is in admin role
    var isInAdminRole = await userManager.IsInRoleAsync(createdUser, "admin");
    isInAdminRole.Should().BeTrue("the default fallback admin user should be assigned to the admin role");

    // Verify user can authenticate with fallback password
    var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
    var signInResult = await signInManager.CheckPasswordSignInAsync(createdUser, "nimda", false);
    signInResult.Succeeded.Should().BeTrue("the default fallback admin user should authenticate with 'nimda' password");
  }

  [DependencyFact(InfrastructureDependency.Database)]
  public async Task SeedAdminUser_PrefersDefaultAdminUserConfig_OverFallback()
  {
    // Arrange
    await ResetDatabaseAsync();

    var testConfig = new Dictionary<string, string?>
    {
      // Both DefaultAdminUser and EmailConfig provided - should prefer DefaultAdminUser
      { "DefaultAdminUser:Email", "primary@example.com" },
      { "DefaultAdminUser:UserName", "primaryadmin" },
      { "DefaultAdminUser:Password", "PrimaryPassword123!" },
      { "EmailConfig:FromEmail", "fallback@example.com" },
      { $"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}", "Server=localhost;Database=test;User Id=test;Password=test;" }
    };

    await using var factory = Factory.ReplaceService(services =>
    {
      services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
        .AddInMemoryCollection(testConfig)
        .Build());

      services.AddSingleton<IWebHostEnvironment>(_ =>
        new TestWebHostEnvironment { EnvironmentName = "Development" });
    });

    using var scope = factory.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<RoleInitializer>>();

    // Ensure roles exist first
    await roleManager.EnsureRolesCreatedAsync();

    var roleInitializer = new RoleInitializer(
      scope.ServiceProvider,
      configuration,
      environment,
      logger);

    // Act
    await roleInitializer.StartAsync(CancellationToken.None);

    // Assert
    var createdUser = await userManager.FindByEmailAsync("primary@example.com");
    createdUser.Should().NotBeNull("admin user should be created using DefaultAdminUser config");
    createdUser.UserName.Should().Be("primaryadmin", "username should use DefaultAdminUser config");
    createdUser.Email.Should().Be("primary@example.com");

    // Verify fallback user was NOT created
    var fallbackUser = await userManager.FindByEmailAsync("fallback@example.com");
    fallbackUser.Should().BeNull("fallback user should not be created when DefaultAdminUser config is complete");

    // Verify user can authenticate with primary password (not fallback)
    var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
    var signInResult = await signInManager.CheckPasswordSignInAsync(createdUser, "PrimaryPassword123!", false);
    signInResult.Succeeded.Should().BeTrue("should authenticate with DefaultAdminUser password");

    var fallbackSignInResult = await signInManager.CheckPasswordSignInAsync(createdUser, "nimda", false);
    fallbackSignInResult.Succeeded.Should().BeFalse("should not authenticate with fallback password");
  }

  /// <summary>
  /// Test implementation of IWebHostEnvironment for controlling the environment name in tests.
  /// </summary>
  private class TestWebHostEnvironment : IWebHostEnvironment
  {
    public string WebRootPath { get; set; } = "";
    public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "TestApp";
    public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = "";
    public string EnvironmentName { get; set; } = "Testing";
  }
}
