using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Net;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.Auth;
using Zarichney.Services.Status;
using Zarichney.Startup;
using Zarichney.TestingFramework.Attributes;
using Zarichney.TestingFramework.Fixtures;
using Zarichney.TestingFramework.Helpers;

namespace Zarichney.Tests.Integration.Services.Auth
{
  /// <summary>
  /// Tests the graceful degradation functionality when the Identity Database is not configured
  /// in non-Production environments.
  /// </summary>
  [Trait("Category", "Integration")]
  public class IdentityDbGracefulDegradationTests(ITestOutputHelper testOutputHelper)
  {
    /// <summary>
    /// Specialized factory that removes the Identity Database connection string from configuration.
    /// </summary>
    private class IdentityDbMissingWebApplicationFactory(string environmentName = "Testing")
      : CustomWebApplicationFactory
    {
      protected override void ConfigureWebHost(IWebHostBuilder builder)
      {
        // First call the base implementation to configure common services
        base.ConfigureWebHost(builder);

        // Override the environment name
        builder.UseEnvironment(environmentName);

        // Override configuration to ensure the Identity Database connection string is missing/empty
        builder.ConfigureAppConfiguration((_, config) =>
        {
          var inMemoryConfig = new Dictionary<string, string?>
          {
            // Set connection string to null to simulate it being missing
            { $"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}", null },
            // Explicitly disable mock authentication to test graceful degradation of real Identity services
            { "MockAuth:Enabled", "false" }
          };

          // Add memory configuration with higher precedence than other sources
          config.AddInMemoryCollection(inMemoryConfig);
        });
      }
    }

    /// <summary>
    /// Factory with missing Identity Database connection string in Development environment.
    /// </summary>
    private readonly IdentityDbMissingWebApplicationFactory _developmentFactoryWithNoDb = new();

    /// <summary>
    /// Factory with missing Identity Database connection string in Production environment.
    /// </summary>
    private readonly IdentityDbMissingWebApplicationFactory _productionFactoryWithNoDb = new("Production");

    /// <summary>
    /// Verifies that the application can start in Testing environment
    /// even if the Identity Database connection string is missing.
    /// </summary>
    [Fact]
    public async Task Application_CanStart_InDevelopmentWithMissingIdentityDbConfig()
    {
      // Arrange & Act - Try to create a client, which will cause the application to start
      var client = _developmentFactoryWithNoDb.CreateClient();

      // Act - Call a simple endpoint to verify the application is running
      // The /api/status endpoint appears to be secured, so we need to create a client with authentication
      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
          "Test", AuthTestHelper.GenerateTestToken("test-user", ["User"]));
      var response = await client.GetAsync("/api/status");

      // Assert - The application should be running and return a valid response
      response.Should().NotBeNull();
      response.StatusCode.Should().Be(HttpStatusCode.OK, "The status endpoint should return OK");
    }

    /// <summary>
    /// Verifies that when the Identity Database is unavailable in Development,
    /// the /status endpoint properly reports it as unavailable.
    /// </summary>
    [Fact]
    public async Task StatusEndpoint_ReportsIdentityDbUnavailable_InDevelopmentWithMissingConfig()
    {
      // Arrange
      var client = _developmentFactoryWithNoDb.CreateClient();

      // Act - Call the status endpoint to get service status
      // The /api/status endpoint appears to be secured, so we need to create a client with authentication
      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
          "Test", AuthTestHelper.GenerateTestToken("test-user", ["User"]));
      var response = await client.GetAsync("/api/status");

      // Assert
      response.Should().NotBeNull();
      response.StatusCode.Should().Be(HttpStatusCode.OK, "The status endpoint should return OK");

      // Get and parse the response content
      var content = await response.Content.ReadAsStringAsync();
      content.Should().Contain("PostgresIdentityDb", "The status response should include the PostgresIdentityDb service");
      content.Should().Contain("\"isAvailable\":false", "The PostgresIdentityDb should be reported as unavailable");
    }

    /// <summary>
    /// Verifies that the Login endpoint returns 503 Service Unavailable
    /// when the Identity Database is unavailable in Development.
    /// </summary>
    [Fact]
    public async Task LoginEndpoint_Returns503_WhenIdentityDbUnavailable()
    {
      // Arrange
      var client = _developmentFactoryWithNoDb.CreateClient();

      // Act - Call the login endpoint
      using var loginRequest = new StringContent(
        """{"email":"test@example.com","password":"test123"}""",
        System.Text.Encoding.UTF8,
        "application/json");
      var response = await client.PostAsync("/api/auth/login", loginRequest);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable,
        "The login endpoint should return 503 Service Unavailable when Identity DB is unavailable");

      // Verify the response status code is 503
      // It's sufficient to check that the endpoint returns 503, which indicates that the service is unavailable
      // The exact content of the error message is less important than the status code
      testOutputHelper.WriteLine($"Response status: {response.StatusCode}");
      var content = await response.Content.ReadAsStringAsync();
      testOutputHelper.WriteLine($"Response content: {content}");

      // Check that the response contains some indication that the service is unavailable
      content.Should().Contain("unavailable", "The response should indicate that a required service is unavailable");
    }

    /// <summary>
    /// Verifies that the Register endpoint returns 503 Service Unavailable
    /// when the Identity Database is unavailable in Development.
    /// </summary>
    [Fact]
    public async Task RegisterEndpoint_Returns503_WhenIdentityDbUnavailable()
    {
      // Arrange
      var client = _developmentFactoryWithNoDb.CreateClient();

      // Act - Call the register endpoint
      using var registerRequest = new StringContent(
        """{"email":"newuser@example.com","password":"Password123!"}""",
        System.Text.Encoding.UTF8,
        "application/json");
      var response = await client.PostAsync("/api/auth/register", registerRequest);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable,
        "The register endpoint should return 503 Service Unavailable when Identity DB is unavailable");

      // Verify the response status code is 503
      // It's sufficient to check that the endpoint returns 503, which indicates that the service is unavailable
      // The exact content of the error message is less important than the status code
      testOutputHelper.WriteLine($"Response status: {response.StatusCode}");
      var content = await response.Content.ReadAsStringAsync();
      testOutputHelper.WriteLine($"Response content: {content}");

      // Check that the response contains some indication that the service is unavailable
      content.Should().Contain("unavailable", "The response should indicate that a required service is unavailable");
    }

    /// <summary>
    /// Verifies that an unrelated endpoint still works when the Identity Database
    /// is unavailable in Development.
    /// </summary>
    [Fact]
    public async Task UnrelatedEndpoint_Returns200_WhenIdentityDbUnavailable()
    {
      // Arrange
      var client = _developmentFactoryWithNoDb.CreateClient();

      // Add authentication header for the test client
      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
          "Test", AuthTestHelper.GenerateTestToken("test-user", ["User"]));

      // Act - Call an unrelated endpoint that doesn't depend on the Identity DB
      var response = await client.GetAsync("/api/health");

      // Assert
      response.Should().NotBeNull();
      response.StatusCode.Should().Be(HttpStatusCode.OK, "The health endpoint should return OK");
    }

    /// <summary>
    /// Verifies that the ValidateStartup.IsIdentityDbAvailable property
    /// is correctly set when the Identity DB connection string is missing.
    /// </summary>
    [Fact]
    public void IdentityDbFlag_IsSetToFalse_WhenConnectionStringIsMissing()
    {
      // Arrange - Create a service scope to access services
      using var scope = _developmentFactoryWithNoDb.Services.CreateScope();

      // Act & Assert
      ValidateStartup.IsIdentityDbAvailable.Should().BeFalse(
        "IsIdentityDbAvailable should be false when the connection string is missing");
    }

    /// <summary>
    /// Verifies that the StatusService correctly reports the Identity DB status.
    /// </summary>
    [Fact]
    public void StatusService_CorrectlyReportsIdentityDbStatus_WhenConnectionStringIsMissing()
    {
      // Arrange - Create a service scope to access services
      using var scope = _developmentFactoryWithNoDb.Services.CreateScope();
      var statusService = scope.ServiceProvider.GetRequiredService<IStatusService>();

      // Act
      var dbStatus = statusService.GetFeatureStatus(ExternalServices.PostgresIdentityDb);

      // Assert
      dbStatus.Should().NotBeNull("The status service should return a status for PostgresIdentityDb");
      dbStatus.IsAvailable.Should().BeFalse("The PostgresIdentityDb status should be unavailable");
    }

    /// <summary>
    /// This test verifies that the ValidateStartup logic correctly throws an exception in Production
    /// when the Identity Database connection string is missing. This test is much faster than
    /// testing through the full WebApplicationFactory startup process.
    /// </summary>
    /// <remarks>
    /// Note that in a real environment this would cause Environment.Exit(1) to be called,
    /// but in the test environment, we've modified ValidateStartup to throw an InvalidOperationException
    /// instead of calling Environment.Exit for safer testing.
    /// </remarks>
    [Fact]
    public void ValidateStartup_ThrowsException_InProductionWithMissingIdentityDbConfig()
    {
      // Arrange - Create a minimal configuration with missing Identity DB connection string
      var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          { $"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}", null }
        })
        .Build();

      // Create a Production environment
      var environment = new TestHostEnvironment { EnvironmentName = "Production" };

      // Act & Assert
      var exception = Assert.Throws<InvalidOperationException>(() =>
      {
        ValidateStartup.ValidateProductionConfiguration(environment, configuration);
      });

      // Verify the exception message contains expected content
      exception.Message.Should().Contain("Identity Database connection string",
        "The exception should mention the missing Identity Database connection string");
    }

    /// <summary>
    /// This test verifies that the full application startup fails in Production when the
    /// Identity Database connection string is missing.
    /// Note: This test takes ~47 seconds to complete as it tests full application startup failure.
    /// Skipped by default - use dotnet test --filter "FullyQualifiedName~Application_FailsToStart_InProductionWithMissingIdentityDbConfig" to run.
    /// </summary>
    [SkippableFact]
    [Trait(TestCategories.Category, TestCategories.SlowIntegration)]
    public void Application_FailsToStart_InProductionWithMissingIdentityDbConfig()
    {
      // Skip this test by default unless explicitly targeted
      // This can be overridden by setting ZARICHNEY_RUN_SLOW_TESTS=true environment variable
      var runSlowTests = Environment.GetEnvironmentVariable("ZARICHNEY_RUN_SLOW_TESTS");
      Skip.If(string.IsNullOrEmpty(runSlowTests) || !bool.TryParse(runSlowTests, out var shouldRun) || !shouldRun,
              "This test takes ~47 seconds and is skipped by default. Set ZARICHNEY_RUN_SLOW_TESTS=true or target this test specifically to run it.");

      // Arrange & Act & Assert
      // This should cause ValidateStartup.ValidateProductionConfiguration to throw an exception
      // rather than calling Environment.Exit(1) since we're in a test environment
      Assert.Throws<InvalidOperationException>(() =>
      {
        _productionFactoryWithNoDb.CreateClient();
      });
    }

    /// <summary>
    /// Simple test environment implementation for testing ValidateStartup directly
    /// </summary>
    private class TestHostEnvironment : IWebHostEnvironment
    {
      public string WebRootPath { get; set; } = "";
      public IFileProvider WebRootFileProvider { get; set; } = null!;
      public string ApplicationName { get; set; } = "TestApp";
      public IFileProvider ContentRootFileProvider { get; set; } = null!;
      public string ContentRootPath { get; set; } = "";
      public string EnvironmentName { get; set; } = "Development";
    }
  }
}
