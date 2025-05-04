using Microsoft.EntityFrameworkCore;

namespace Zarichney.Services.Auth;

public class RefreshTokenCleanupService : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<RefreshTokenCleanupService> _logger;
  private readonly IConfiguration _configuration;
  private readonly TimeSpan _cleanupInterval;
  private const string ConnectionStringName = "IdentityConnection";

  public RefreshTokenCleanupService(
    IServiceProvider serviceProvider,
    ILogger<RefreshTokenCleanupService> logger,
    IConfiguration configuration)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
    _configuration = configuration; // Store configuration

    // Default cleanup interval is once per day
    var cleanupHours = _configuration.GetValue("JwtSettings:TokenCleanupIntervalHours", 24);
    _cleanupInterval = TimeSpan.FromHours(cleanupHours);
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("RefreshToken Cleanup Service is starting.");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        // Check connection string before attempting cleanup
        var connectionString = _configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrEmpty(connectionString))
        {
          _logger.LogWarning(
            "{ServiceName} requires the '{ConnectionStringName}' connection string to be configured. Skipping token cleanup.",
            nameof(RefreshTokenCleanupService),
            ConnectionStringName);
        }
        else
        {
          await CleanupExpiredTokensAsync(stoppingToken);
        }
      }
      catch (Exception ex)
      {
        // Log errors unrelated to the missing connection string check above
        _logger.LogError(ex, "Error occurred during refresh token cleanup cycle.");
      }

      // Wait for the next cleanup interval regardless of whether cleanup ran
      try
      {
        await Task.Delay(_cleanupInterval, stoppingToken);
      }
      catch (OperationCanceledException)
      {
        // Ignore cancellation exceptions during delay
      }
    }

    _logger.LogInformation("RefreshToken Cleanup Service is stopping.");
  }

  private async Task CleanupExpiredTokensAsync(CancellationToken stoppingToken)
  {
    // This method now assumes the connection string exists, as the check is done in ExecuteAsync
    _logger.LogInformation("Starting expired refresh token cleanup.");

    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

    // Current time for comparison
    var utcNow = DateTime.UtcNow;

    // Find all tokens that are:
    // 1. Expired, or
    // 2. Used, or
    // 3. Revoked
    var tokensToDelete = await dbContext.RefreshTokens
      .Where(t => t.ExpiresAt < utcNow || t.IsUsed || t.IsRevoked)
      .ToListAsync(stoppingToken);

    if (tokensToDelete.Any())
    {
      _logger.LogInformation("Removing {Count} expired/invalid refresh tokens.", tokensToDelete.Count);
      dbContext.RefreshTokens.RemoveRange(tokensToDelete);
      await dbContext.SaveChangesAsync(stoppingToken);
      _logger.LogInformation("Expired/invalid refresh tokens cleanup completed.");
    }
    else
    {
      _logger.LogInformation("No expired/invalid refresh tokens found to clean up.");
    }
  }
}
