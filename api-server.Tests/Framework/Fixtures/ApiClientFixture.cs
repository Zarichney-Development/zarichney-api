using Microsoft.Extensions.DependencyInjection;
using Refit;
using Serilog.Sinks.XUnit.Injectable;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Client;

namespace Zarichney.Tests.Framework.Fixtures;

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
  /// Gets the unauthenticated API client.
  /// </summary>
  public IZarichneyAPI UnauthenticatedClient { get; private set; } = null!;

  /// <summary>
  /// Gets the authenticated API client.
  /// </summary>
  public IZarichneyAPI AuthenticatedClient { get; private set; } = null!;

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
    // Create and initialize the database fixture
    _databaseFixture = new DatabaseFixture();

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

    // Create HTTP client for unauthenticated calls
    var unauthHttpClient = _factory.CreateClient();
    UnauthenticatedClient = RestService.For<IZarichneyAPI>(unauthHttpClient);

    // Create authenticated HTTP client using TestAuthHandler
    const string userId = "test-user-id";
    var roles = new[] { "User", "Admin" };
    var authHttpClient = _factory.CreateAuthenticatedClient(userId, roles);
    AuthenticatedClient = RestService.For<IZarichneyAPI>(authHttpClient);
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
}
