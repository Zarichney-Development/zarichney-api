using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using Serilog.Sinks.XUnit.Injectable;
using Serilog.Sinks.XUnit.Injectable.Extensions;
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

  /// <summary>
  /// Gets a value indicating whether the database is available.
  /// </summary>
  public bool IsDatabaseAvailable => _databaseFixture?.IsContainerAvailable ?? false;

  /// <summary>
  /// Gets a value indicating whether Docker is available on the system.
  /// </summary>
  public bool IsDockerAvailable => CheckDockerAvailability();

  /// <summary>
  /// Checks if Docker is available and properly configured.
  /// </summary>
  private bool CheckDockerAvailability()
  {
    try
    {
      var psi = new System.Diagnostics.ProcessStartInfo("docker", "info")
      {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };
      using var proc = System.Diagnostics.Process.Start(psi);
      if (proc == null) return false;

      // Set a shorter timeout
      if (!proc.WaitForExit(1000))
      {
        try
        {
          proc.Kill();
        }
        catch
        {
          // Ignore errors when killing the process
        }

        return false;
      }

      return proc.ExitCode == 0;
    }
    catch
    {
      return false;
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="CustomWebApplicationFactory"/> class.
  /// </summary>
  protected CustomWebApplicationFactory()
  {
    // No database fixture available
    _databaseFixture = null;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="CustomWebApplicationFactory"/> class.
  /// </summary>
  /// <param name="databaseFixture">The database fixture to use. This is injected by the collection fixture system.</param>
  public CustomWebApplicationFactory(DatabaseFixture databaseFixture)
  {
    _databaseFixture = databaseFixture;
  }

  /// <summary>
  /// Configures the test services for the application.
  /// </summary>
  /// <param name="builder">The <see cref="IWebHostBuilder"/> for configuring the application.</param>
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    // Force the environment to Testing for all tests
    builder.UseEnvironment("Testing");

    builder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
    {
      var env = hostingContext.HostingEnvironment;
      if (string.IsNullOrEmpty(env.EnvironmentName))
      {
        env.EnvironmentName = "Testing";
      }

      // Clear existing sources to ensure our order is respected
      configBuilder.Sources.Clear();

      // Add configuration providers in specific order
      configBuilder
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);

      // Add user secrets in development mode
      if (env.EnvironmentName == "Development")
      {
        configBuilder.AddUserSecrets<Program>(optional: true);
      }

      // Add environment variables (highest precedence)
      configBuilder.AddEnvironmentVariables();
    });

    builder.ConfigureServices((hostingContext, services) =>
    {
      // Register test-specific services

      // Register test auth handler
      services.AddAuthentication(options =>
        {
          options.DefaultAuthenticateScheme = "Test";
          options.DefaultChallengeScheme = "Test";
        })
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

      // Register mock external services
      RegisterMockExternalServices(services);

      // Implement prioritized database selection logic
      ConfigureTestDatabase(services);

      var testOutputSink = new InjectableTestOutputSink();
      services.AddSingleton(testOutputSink);

      // Use configuration from hosting context instead of building service provider
      var configuration = hostingContext.Configuration;

      var xunitLogger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Enrich.FromLogContext()
        .WriteTo.InjectableTestOutput(testOutputSink)
        // Filter out noisy Microsoft logs during startup
        .Filter.ByExcluding(logEvent =>
          logEvent.Properties.ContainsKey("SourceContext") &&
          logEvent.Properties["SourceContext"].ToString().Contains("Microsoft") &&
          logEvent.Level < Serilog.Events.LogEventLevel.Warning)
        .CreateLogger();

      Log.Logger = xunitLogger;
    });

    // builder.ConfigureLogging(logging =>
    // {
    //   logging.ClearProviders();
    //   logging.AddConsole();
    // });
  }

  /// <summary>
  /// Configures the test database with prioritized selection logic:
  /// 1. Use connection string from IConfiguration if valid
  /// 2. Else, use connection string from the shared DatabaseFixture if available and running
  /// 3. Else, fallback to UseInMemoryDatabase
  /// </summary>
  /// <param name="services">The service collection to configure.</param>
  private void ConfigureTestDatabase(IServiceCollection services)
  {
    // Get the loaded configuration to check for connection string
    var configuration = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", optional: true)
      .AddUserSecrets<Program>()
      .AddJsonFile("appsettings.Testing.json", optional: true)
      .AddEnvironmentVariables()
      .Build();

    var connectionString = configuration.GetConnectionString(UserDbContext.UserDatabaseConnectionName);

    // Remove existing DbContext registrations
    var dbContextDescriptors = services.Where(d =>
      d.ServiceType == typeof(DbContextOptions<UserDbContext>) ||
      d.ServiceType == typeof(UserDbContext)).ToList();

    foreach (var descriptor in dbContextDescriptors)
    {
      services.Remove(descriptor);
    }

    // Add DbContext with the appropriate provider based on prioritized logic
    services.AddDbContext<UserDbContext>(options =>
    {
      // Priority 1: Valid connection string from configuration
      if (!string.IsNullOrEmpty(connectionString) &&
          !connectionString.Contains("zarichney_unavailable"))
      {
        options.UseNpgsql(connectionString, b => b.MigrationsAssembly("Zarichney"));
      }
      // Priority 2: DatabaseFixture's container if available
      else if (IsDatabaseAvailable && _databaseFixture != null)
      {
        options.UseNpgsql(_databaseFixture.ConnectionString, b => b.MigrationsAssembly("Zarichney"));
      }
      // Priority 3: In-memory database as fallback
      else
      {
        options.UseInMemoryDatabase("TestDb");
      }
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
  /// Replaces services in the service collection with custom implementations.
  /// Useful for overriding specific services in integration tests.
  /// </summary>
  /// <param name="configureServices">An action to configure the service collection.</param>
  /// <returns>A new WebApplicationFactory with the modified services.</returns>
  public WebApplicationFactory<Program> ReplaceService(Action<IServiceCollection> configureServices)
  {
    // Create a new WebApplicationFactory with modified services
    return WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(configureServices);
    });
  }

  /// <inheritdoc/>
  protected override void Dispose(bool disposing)
  {
    if (disposing && _databaseFixture is { IsContainerAvailable: true })
    {
      _databaseFixture.DisposeAsync().GetAwaiter().GetResult();
    }

    base.Dispose(disposing);
  }
}
