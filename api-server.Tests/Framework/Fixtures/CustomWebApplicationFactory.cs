using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Zarichney.Client;
using Zarichney.Services.AI;
using Zarichney.Services.Auth;
using Zarichney.Tests.Framework.Helpers;
using Zarichney.Tests.Framework.Mocks.Factories;

namespace Zarichney.Tests.Framework.Fixtures;

/// <summary>
/// Factory for bootstrapping an application in memory for functional end to end tests.
/// Configures the test server with test-specific services and configuration.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly DatabaseFixture? _databaseFixture;
    private readonly bool _isDatabaseAvailable;

    /// <summary>
    /// Gets a value indicating whether the database is available.
    /// </summary>
    public bool IsDatabaseAvailable => _isDatabaseAvailable;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomWebApplicationFactory"/> class.
    /// </summary>
    /// <remarks>
    /// Note: Creating a new DatabaseFixture instance here is not ideal for shared fixture usage.
    /// In a production testing environment, the fixture should be shared via xUnit's IClassFixture
    /// or ICollectionFixture mechanisms. This implementation ensures the tests work correctly
    /// while a more robust solution is developed.
    /// </remarks>
    public CustomWebApplicationFactory()
    {
        // No-op: Database initialization is handled by DatabaseFixture in DB tests
        _databaseFixture = null;
        _isDatabaseAvailable = false;
    }

    /// <summary>
    /// Gets the database connection string, or a placeholder if the database is not available.
    /// </summary>
    /// <returns>The database connection string, or a placeholder if the database is not available.</returns>
    private string GetDatabaseConnectionString()
    {
        if (_isDatabaseAvailable && _databaseFixture != null)
        {
            return _databaseFixture.ConnectionString;
        }
        
        // Return a placeholder connection string for tests that can run without a real database
        return "Host=localhost;Database=zarichney_unavailable;Username=postgres;Password=postgres";
    }

    /// <summary>
    /// Configures the test services for the application.
    /// </summary>
    /// <param name="builder">The <see cref="IWebHostBuilder"/> for configuring the application.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
        {
            // Set the environment to Testing if not already set
            hostingContext.HostingEnvironment.EnvironmentName = "Testing";
            
            // Clear existing sources to ensure our order is respected
            configBuilder.Sources.Clear();
            
            // Add configuration providers in specific order
            configBuilder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Testing.json", optional: true, reloadOnChange: false);

            // Add user secrets in development mode
            if (hostingContext.HostingEnvironment.EnvironmentName == "Development")
            {
                configBuilder.AddUserSecrets<Program>(optional: true);
            }
            
            // Add environment variables
            configBuilder.AddEnvironmentVariables();
            
            // Add the test database connection string as the very last provider to override any existing settings
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:IdentityConnection", GetDatabaseConnectionString() }
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Register test-specific services
            
            // Register test auth handler
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                "Test", _ => { });

            // Register mock external services
            RegisterMockExternalServices(services);

            // --- Ensure DbContext uses test database connection string ---
            // Remove all existing DbContext registrations (if any)
            var dbContextDescriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<UserDbContext>) ||
                d.ServiceType == typeof(UserDbContext)).ToList();
                
            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }
            
            // Register UserDbContext with the test database connection string
            // For minimal functionality tests that don't actually use the database,
            // this will be configured with a placeholder connection string
            services.AddDbContext<UserDbContext>(options =>
            {
                if (_isDatabaseAvailable && _databaseFixture != null)
                {
                    options.UseNpgsql(GetDatabaseConnectionString(), b => b.MigrationsAssembly("Zarichney"));
                }
                else
                {
                    // Use in-memory database for tests when real DB is unavailable
                    options.UseInMemoryDatabase("TestDb");
                }
            });
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
    }

    /// <summary>
    /// Registers mock implementations of external services.
    /// </summary>
    /// <param name="services">The service collection to register mocks with.</param>
    private void RegisterMockExternalServices(IServiceCollection services)
    {
        // Register mock external services using the mock factories
        services.AddSingleton(MockStripeServiceFactory.CreateMock());
        services.AddSingleton(MockOpenAIServiceFactory.CreateMock());
        services.AddSingleton(MockGitHubServiceFactory.CreateMock());
        services.AddSingleton(MockMSGraphServiceFactory.CreateMock());
        
        // Register mocks created by the factories as the actual service interfaces
        // This is required so that tests can access the mocks for verification
        services.AddSingleton(sp => sp.GetRequiredService<Mock<IStripeService>>().Object);
        services.AddSingleton(sp => sp.GetRequiredService<Mock<ILlmService>>().Object);
        services.AddSingleton(sp => sp.GetRequiredService<Mock<IGitHubService>>().Object);
        services.AddSingleton(sp => sp.GetRequiredService<Mock<IEmailService>>().Object);
    }

    /// <summary>
    /// Creates an HttpClient with authentication configured for the specified user.
    /// </summary>
    /// <param name="userId">The user ID to authenticate as.</param>
    /// <param name="roles">The roles to assign to the user.</param>
    /// <returns>An HttpClient configured with authentication.</returns>
    public HttpClient CreateAuthenticatedClient(string userId, string[] roles)
    {
        var client = CreateClient();
        
        // Configure client with authentication
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Test", AuthTestHelper.GenerateTestToken(userId, roles));
        
        return client;
    }
    
    /// <summary>
    /// Creates an HttpClient with a raw bearer token for authentication.
    /// </summary>
    public HttpClient CreateClientWithBearerToken(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Creates a configured Refit client for the API.
    /// </summary>
    /// <param name="httpClient">Optional HTTP client to use. If not provided, a new client will be created.</param>
    /// <returns>A Refit client for the API.</returns>
    public IZarichneyAPI CreateRefitClient(HttpClient? httpClient = null)
    {
        httpClient ??= CreateClient();
        return Refit.RestService.For<IZarichneyAPI>(httpClient);
    }
    
    /// <summary>
    /// Creates a configured Refit client with authentication for the specified user.
    /// </summary>
    /// <param name="userId">The user ID to authenticate as.</param>
    /// <param name="roles">The roles to assign to the user.</param>
    /// <returns>A Refit client configured with authentication.</returns>
    public IZarichneyAPI CreateAuthenticatedRefitClient(string userId, string[] roles)
    {
        var client = CreateAuthenticatedClient(userId, roles);
        return CreateRefitClient(client);
    }
    
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && _databaseFixture != null && _isDatabaseAvailable)
        {
            _databaseFixture.DisposeAsync().GetAwaiter().GetResult();
        }
        base.Dispose(disposing);
    }
}
