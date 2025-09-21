using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;
using Zarichney.Services.Auth;

namespace Zarichney.Server.Tests.Framework.Fixtures;

/// <summary>
/// Fixture for managing a PostgreSQL test database using Testcontainers.
/// Implements IAsyncLifetime to handle container lifecycle and provides
/// methods for resetting the database between tests.
/// </summary>
public class DatabaseFixture : IAsyncLifetime, IDisposable
{
  private readonly PostgreSqlBuilder _builder;
  private PostgreSqlContainer? _dbContainer;
  private Respawner _respawner = null!;
  private readonly ILogger<DatabaseFixture> _logger;
  private bool _isContainerAvailable;

  /// <summary>
  /// Gets the connection string for the PostgreSQL test database.
  /// </summary>
  public string ConnectionString => _dbContainer?.GetConnectionString() ?? throw new InvalidOperationException("Database container not started.");

  /// <summary>
  /// Initializes a new instance of the <see cref="DatabaseFixture"/> class.
  /// </summary>
  /// <param name="databaseName">Optional unique database name for isolation. If null, uses a random GUID.</param>
  // Logger factory that will be properly disposed in Dispose method
  private readonly ILoggerFactory _loggerFactory;

  public DatabaseFixture(string? databaseName = null)
  {
    // Create a logger factory and logger for the fixture
    _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    _logger = _loggerFactory.CreateLogger<DatabaseFixture>();

    // Use provided database name or generate a unique one for parallel isolation
    var dbName = databaseName ?? $"zarichney_test_{Guid.NewGuid():N}";

    // Prepare the PostgreSQL container builder; actual build will be in InitializeAsync
    _builder = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .WithDatabase(dbName)
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true);
  }

  /// <summary>
  /// Initializes the database container and Respawner.
  /// </summary>
  public async Task InitializeAsync()
  {
    _logger.LogInformation("Starting PostgreSQL test container...");
    try
    {
      _dbContainer = _builder.Build();
      await _dbContainer.StartAsync();
      _isContainerAvailable = true;
      _logger.LogInformation("PostgreSQL test container started: {ConnectionString}", ConnectionString);
    }
    catch (Exception ex)
    {
      _logger.LogWarning("Database unavailable, skipping DB tests: {Message}", ex.Message);
      _isContainerAvailable = false;
      return;
    }

    try
    {
      // Apply EF Core migrations programmatically
      _logger.LogInformation("Applying database migrations...");
      await ApplyMigrationsAsync();
      _logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to apply database migrations");
      _isContainerAvailable = false;
      await _dbContainer.DisposeAsync();
      throw;
    }

    // Initialize Respawner
    await InitializeRespawner();

    _logger.LogInformation("Database fixture initialized successfully");
  }

  /// <summary>
  /// Applies EF Core migrations to the database.
  /// </summary>
  private async Task ApplyMigrationsAsync()
  {
    // Create a DbContext using the container connection string
    var serviceProvider = new ServiceCollection()
        .AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(ConnectionString, b => b.MigrationsAssembly("Zarichney")))
        .BuildServiceProvider();

    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

    // Apply migrations
    await dbContext.Database.MigrateAsync();
  }

  /// <summary>
  /// Disposes the database container.
  /// </summary>
  public async Task DisposeAsync()
  {
    if (_dbContainer != null && _isContainerAvailable) await _dbContainer.DisposeAsync();

    _logger.LogInformation("PostgreSQL test container stopped");
  }

  /// <summary>
  /// Resets the database to a clean state.
  /// This should be called at the beginning of each test that requires a clean database.
  /// </summary>
  public async Task ResetDatabaseAsync()
  {
    if (!_isContainerAvailable || _respawner == null)
    {
      throw new InvalidOperationException("Database container is not available or not properly initialized. Cannot reset database.");
    }

    _logger.LogInformation("Resetting database to clean state...");

    await using var connection = new NpgsqlConnection(ConnectionString);
    await connection.OpenAsync();

    await _respawner.ResetAsync(connection);

    _logger.LogInformation("Database reset completed");
  }

  /// <summary>
  /// Creates a new database connection.
  /// </summary>
  /// <returns>A new database connection.</returns>
  public async Task<DbConnection> CreateConnectionAsync()
  {
    if (!_isContainerAvailable)
    {
      throw new InvalidOperationException("Database container is not available. Cannot create connection.");
    }

    var connection = new NpgsqlConnection(ConnectionString);
    await connection.OpenAsync();
    return connection;
  }

  /// <summary>
  /// Initializes the Respawner for database cleanup.
  /// </summary>
  private async Task InitializeRespawner()
  {
    await using var connection = new NpgsqlConnection(ConnectionString);
    await connection.OpenAsync();

    _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
    {
      DbAdapter = DbAdapter.Postgres,
      SchemasToInclude = ["public"],
      TablesToIgnore =
        [
            // Add any tables that should not be cleaned between tests
            "__EFMigrationsHistory"
        ]
    });
  }

  /// <summary>
  /// Indicates whether the database container is available.
  /// </summary>
  public bool IsContainerAvailable => _isContainerAvailable;

  /// <summary>
  /// Disposes managed resources like the logger factory.
  /// </summary>
  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Disposes managed and unmanaged resources.
  /// </summary>
  /// <param name="disposing">True to dispose managed resources, false otherwise.</param>
  protected virtual void Dispose(bool disposing)
  {
    if (disposing)
    {
      _loggerFactory.Dispose();
    }
  }
}

public class TestSkippedException(string message) : Exception(message)
{
  // Properly implemented constructor just passes message to base Exception
}
