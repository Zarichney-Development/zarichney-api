using Microsoft.Extensions.DependencyInjection;
using Refit;
using Serilog.Sinks.XUnit.Injectable;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Client;
using Zarichney.Services.Auth;
using Zarichney.Services.Status;

namespace Zarichney.Server.Tests.Framework.Fixtures;

/// <summary>
/// xUnit fixture for shared API client instances (unauthenticated and authenticated).
/// Creates and manages all required fixtures internally to avoid xUnit collection fixture ordering issues.
/// </summary>
public class ApiClientFixture : IAsyncLifetime
{
  private readonly DatabaseFixture _databaseFixture;
  private readonly CustomWebApplicationFactory _factory;

  private TService GetSingletonService<TService>() where TService : notnull =>
    _factory.Services.GetRequiredService<TService>();

  /// <summary>
  /// Gets the unauthenticated Auth API client.
  /// </summary>
  public IAuthApi UnauthenticatedAuthApi { get; private set; } = null!;

  /// <summary>
  /// Gets the authenticated Auth API client.
  /// </summary>
  public IAuthApi AuthenticatedAuthApi { get; private set; } = null!;

  /// <summary>
  /// Gets the unauthenticated AI API client.
  /// </summary>
  public IAiApi UnauthenticatedAiApi { get; private set; } = null!;

  /// <summary>
  /// Gets the authenticated AI API client.
  /// </summary>
  public IAiApi AuthenticatedAiApi { get; private set; } = null!;

  /// <summary>
  /// Gets the unauthenticated Cookbook API client.
  /// </summary>
  public ICookbookApi UnauthenticatedCookbookApi { get; private set; } = null!;

  /// <summary>
  /// Gets the authenticated Cookbook API client.
  /// </summary>
  public ICookbookApi AuthenticatedCookbookApi { get; private set; } = null!;

  /// <summary>
  /// Gets the unauthenticated Payment API client.
  /// </summary>
  public IPaymentApi UnauthenticatedPaymentApi { get; private set; } = null!;

  /// <summary>
  /// Gets the authenticated Payment API client.
  /// </summary>
  public IPaymentApi AuthenticatedPaymentApi { get; private set; } = null!;

  /// <summary>
  /// Gets the unauthenticated Public API client.
  /// </summary>
  public IPublicApi UnauthenticatedPublicApi { get; private set; } = null!;

  /// <summary>
  /// Gets the authenticated Public API client.
  /// </summary>
  public IPublicApi AuthenticatedPublicApi { get; private set; } = null!;

  /// <summary>
  /// Gets the unauthenticated API client.
  /// </summary>
  public IApiApi UnauthenticatedApiApi { get; private set; } = null!;

  /// <summary>
  /// Gets the authenticated API client.
  /// </summary>
  public IApiApi AuthenticatedApiApi { get; private set; } = null!;


  /// <summary>
  /// Gets the database fixture.
  /// </summary>
  public DatabaseFixture DatabaseFixture => _databaseFixture;

  /// <summary>
  /// Gets the web application factory.
  /// </summary>
  public CustomWebApplicationFactory Factory => _factory;

  /// <summary>
  /// Initializes a new instance of the <see cref="ApiClientFixture"/> class.
  /// </summary>
  public ApiClientFixture()
  {
    // Create and initialize the database fixture with original name for backward compatibility
    _databaseFixture = new DatabaseFixture("zarichney_test");

    // Create the web application factory with the database fixture
    _factory = new CustomWebApplicationFactory(_databaseFixture);
  }

  /// <summary>
  /// Initializes the API clients.
  /// </summary>
  public async Task InitializeAsync()
  {
    // Initialize the database fixture
    await _databaseFixture.InitializeAsync();

    // Configure StatusService to report PostgreSQL as available when database container is running
    await ConfigureStatusServiceForTesting();

    // Create HTTP client for unauthenticated calls
    var unauthHttpClient = _factory.CreateClient();

    // Create authenticated HTTP client using TestAuthHandler
    const string userId = "test-user-id";
    var roles = new[] { "User", "Admin" };
    var authHttpClient = _factory.CreateAuthenticatedClient(userId, roles);

    // Initialize all API client interfaces
    UnauthenticatedAuthApi = RestService.For<IAuthApi>(unauthHttpClient);
    AuthenticatedAuthApi = RestService.For<IAuthApi>(authHttpClient);

    UnauthenticatedAiApi = RestService.For<IAiApi>(unauthHttpClient);
    AuthenticatedAiApi = RestService.For<IAiApi>(authHttpClient);

    UnauthenticatedCookbookApi = RestService.For<ICookbookApi>(unauthHttpClient);
    AuthenticatedCookbookApi = RestService.For<ICookbookApi>(authHttpClient);

    UnauthenticatedPaymentApi = RestService.For<IPaymentApi>(unauthHttpClient);
    AuthenticatedPaymentApi = RestService.For<IPaymentApi>(authHttpClient);

    UnauthenticatedPublicApi = RestService.For<IPublicApi>(unauthHttpClient);
    AuthenticatedPublicApi = RestService.For<IPublicApi>(authHttpClient);

    UnauthenticatedApiApi = RestService.For<IApiApi>(unauthHttpClient);
    AuthenticatedApiApi = RestService.For<IApiApi>(authHttpClient);

  }

  /// <summary>
  /// Disposes the API clients.
  /// </summary>
  public async Task DisposeAsync()
  {
    await _factory.DisposeAsync();
    await _databaseFixture.DisposeAsync();
  }

  public void AttachToSerilog(ITestOutputHelper testOutputHelper)
  {
    var outputSink = GetSingletonService<InjectableTestOutputSink>();
    outputSink.Inject(testOutputHelper);
  }

  /// <summary>
  /// Configures the StatusService to report PostgreSQL as available when the database container is running.
  /// This overrides the normal startup check to allow integration tests to pass.
  /// </summary>
  private async Task ConfigureStatusServiceForTesting()
  {
    if (_factory.Services.GetService<IStatusService>() is StatusService statusService)
    {
      // In test environment, mark PostgreSQL as available if we have a database container
      bool isDatabaseAvailable = _factory.IsDatabaseAvailable;
      List<string>? missingConfigurations = isDatabaseAvailable
        ? null
        : new List<string> { $"ConnectionStrings:{UserDbContext.UserDatabaseConnectionName}" };

      await statusService.SetServiceAvailabilityAsync(ExternalServices.PostgresIdentityDb, isDatabaseAvailable, missingConfigurations);
    }
  }
}
