using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.Auth;
using Zarichney.Services.Status;
using Zarichney.Startup;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Services.Auth
{
  /// <summary>
  /// Tests the graceful degradation functionality when the Identity Database is not configured
  /// in non-Production environments.
  /// </summary>
  [Trait("Category", "Integration")]
  public class IdentityDbGracefulDegradationTests
  {
    /// <summary>
    /// Specialized factory that removes the Identity Database connection string from configuration.
    /// </summary>
    private class IdentityDbMissingWebApplicationFactory : CustomWebApplicationFactory
    {
      private readonly string _environmentName;

      public IdentityDbMissingWebApplicationFactory(string environmentName = "Development")
      {
        _environmentName = environmentName;
      }

      protected override void ConfigureWebHost(IWebHostBuilder builder)
      {
        // First call the base implementation to configure common services
        base.ConfigureWebHost(builder);

        // Override the environment name
        builder.UseEnvironment(_environmentName);

        // Override configuration to ensure the Identity Database connection string is missing/empty
        builder.ConfigureAppConfiguration((context, config) =>
        {
          var inMemoryConfig = new Dictionary<string, string?>
          {
            // Set connection string to null to simulate it being missing
            { $"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}", null }
          };

          // Add memory configuration with higher precedence than other sources
          config.AddInMemoryCollection(inMemoryConfig!);
        });
      }
    }

    /// <summary>
    /// Factory with missing Identity Database connection string in Development environment.
    /// </summary>
    private readonly IdentityDbMissingWebApplicationFactory _developmentFactoryWithNoDb;

    /// <summary>
    /// Factory with missing Identity Database connection string in Production environment.
    /// </summary>
    private readonly IdentityDbMissingWebApplicationFactory _productionFactoryWithNoDb;

    private readonly ITestOutputHelper _testOutputHelper;
    
    public IdentityDbGracefulDegradationTests(ITestOutputHelper testOutputHelper)
    {
      _testOutputHelper = testOutputHelper;
      _developmentFactoryWithNoDb = new IdentityDbMissingWebApplicationFactory("Development");
      _productionFactoryWithNoDb = new IdentityDbMissingWebApplicationFactory("Production");
    }

    /// <summary>
    /// Verifies that the application can start in Development environment
    /// even if the Identity Database connection string is missing.
    /// </summary>
    [Fact]
    public async Task Application_CanStart_InDevelopmentWithMissingIdentityDbConfig()
    {
      // Arrange & Act - Try to create a client, which will cause the application to start
      var client = _developmentFactoryWithNoDb.CreateClient();
      
      // Act - Call a simple endpoint to verify the application is running
      // The /status endpoint appears to be secured, so we need to create a client with authentication
      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
          "Test", Framework.Helpers.AuthTestHelper.GenerateTestToken("test-user", ["User"]));
      var response = await client.GetAsync("/status");
      
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
      // The /status endpoint appears to be secured, so we need to create a client with authentication
      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
          "Test", Framework.Helpers.AuthTestHelper.GenerateTestToken("test-user", ["User"]));
      var response = await client.GetAsync("/status");
      
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
      
      // Prepare login request
      var loginRequest = new StringContent(
        """{"email":"test@example.com","password":"test123"}""", 
        System.Text.Encoding.UTF8, 
        "application/json");
      
      // Act - Call the login endpoint
      var response = await client.PostAsync("/api/auth/login", loginRequest);
      
      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable, 
        "The login endpoint should return 503 Service Unavailable when Identity DB is unavailable");
      
      // Verify the response status code is 503
      // It's sufficient to check that the endpoint returns 503, which indicates that the service is unavailable
      // The exact content of the error message is less important than the status code
      _testOutputHelper.WriteLine($"Response status: {response.StatusCode}");
      var content = await response.Content.ReadAsStringAsync();
      _testOutputHelper.WriteLine($"Response content: {content}");
      
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
      
      // Prepare register request
      var registerRequest = new StringContent(
        """{"email":"newuser@example.com","password":"Password123!"}""", 
        System.Text.Encoding.UTF8, 
        "application/json");
      
      // Act - Call the register endpoint
      var response = await client.PostAsync("/api/auth/register", registerRequest);
      
      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable, 
        "The register endpoint should return 503 Service Unavailable when Identity DB is unavailable");
      
      // Verify the response status code is 503
      // It's sufficient to check that the endpoint returns 503, which indicates that the service is unavailable
      // The exact content of the error message is less important than the status code
      _testOutputHelper.WriteLine($"Response status: {response.StatusCode}");
      var content = await response.Content.ReadAsStringAsync();
      _testOutputHelper.WriteLine($"Response content: {content}");
      
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
      
      // Act - Call an unrelated endpoint that doesn't depend on the Identity DB
      var response = await client.GetAsync("/api/public/version");
      
      // Assert
      response.Should().NotBeNull();
      response.StatusCode.Should().Be(HttpStatusCode.OK, "The version endpoint should return OK");
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
      dbStatus!.IsAvailable.Should().BeFalse("The PostgresIdentityDb status should be unavailable");
    }

    /// <summary>
    /// This test verifies that the application fails to start in Production
    /// when the Identity Database connection string is missing.
    /// </summary>
    /// <remarks>
    /// Note that in a real environment this would cause Environment.Exit(1) to be called,
    /// but in the test environment, we've modified ValidateStartup to throw an InvalidOperationException
    /// instead of calling Environment.Exit for safer testing.
    /// </remarks>
    [Fact]
    public void Application_FailsToStart_InProductionWithMissingIdentityDbConfig()
    {
      // Arrange & Act & Assert
      Assert.Throws<InvalidOperationException>(() => 
      {
        // This should cause ValidateStartup.ValidateProductionConfiguration to throw an exception
        // rather than calling Environment.Exit(1) since we're in a test environment
        _productionFactoryWithNoDb.CreateClient();
      });
    }
  }
}