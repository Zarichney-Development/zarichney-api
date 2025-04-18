using System.Data.Common;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Sdk;

namespace Zarichney.Tests.Fixtures;

/// <summary>
/// Fixture for managing a PostgreSQL test database using Testcontainers.
/// Implements IAsyncLifetime to handle container lifecycle and provides
/// methods for resetting the database between tests.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlBuilder _builder;
    private PostgreSqlContainer? _dbContainer;
    private Respawner _respawner = null!;
    private readonly ILogger<DatabaseFixture> _logger;

    /// <summary>
    /// Gets the connection string for the PostgreSQL test database.
    /// </summary>
    public string ConnectionString => _dbContainer?.GetConnectionString() ?? throw new InvalidOperationException("Database container not started.");

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseFixture"/> class.
    /// </summary>
    public DatabaseFixture()
    {
        // Create a logger factory and logger for the fixture
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<DatabaseFixture>();

        // Prepare the PostgreSQL container builder; actual build will be in InitializeAsync
        _builder = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("zarichney_test")
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
            // Build and start the container
            _dbContainer = _builder.Build();
            await _dbContainer.StartAsync();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Skipping database tests: Docker misconfigured: {Message}", ex.Message);
            throw new TestSkippedException("Docker is not running or misconfigured, skipping database-backed tests.");
        }

        _logger.LogInformation("PostgreSQL test container started. Connection string: {ConnectionString}", 
            ConnectionString.Replace("Password=postgres", "Password=********"));

        // Initialize Respawner
        await InitializeRespawner();
        
        _logger.LogInformation("Database fixture initialized successfully");
    }

    /// <summary>
    /// Disposes the database container.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_dbContainer != null) await _dbContainer.DisposeAsync();

        _logger.LogInformation("PostgreSQL test container stopped");
    }

    /// <summary>
    /// Resets the database to a clean state.
    /// This should be called at the beginning of each test that requires a clean database.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
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
}

public class TestSkippedException(string message) : Exception(message)
{
    // Properly implemented constructor just passes message to base Exception
}

/// <summary>
/// Collection definition for tests that require a database.
/// Tests using this collection will share a single DatabaseFixture instance.
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
