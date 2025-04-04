using Microsoft.EntityFrameworkCore;

namespace Zarichney.Server.Services.Auth;

public class RefreshTokenCleanupService : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<RefreshTokenCleanupService> _logger;
  private readonly TimeSpan _cleanupInterval;

  public RefreshTokenCleanupService(
    IServiceProvider serviceProvider,
    ILogger<RefreshTokenCleanupService> logger,
    IConfiguration configuration)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;

    // Default cleanup interval is once per day
    var cleanupHours = configuration.GetValue("JwtSettings:TokenCleanupIntervalHours", 24);
    _cleanupInterval = TimeSpan.FromHours(cleanupHours);
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("RefreshToken Cleanup Service is starting");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        await CleanupExpiredTokensAsync(stoppingToken);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while cleaning up expired refresh tokens");
      }

      // Wait for the next cleanup interval
      await Task.Delay(_cleanupInterval, stoppingToken);
    }
  }

  private async Task CleanupExpiredTokensAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Starting expired refresh token cleanup");

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
      _logger.LogInformation("Removing {Count} expired refresh tokens", tokensToDelete.Count);
      dbContext.RefreshTokens.RemoveRange(tokensToDelete);
      await dbContext.SaveChangesAsync(stoppingToken);
      _logger.LogInformation("Expired refresh tokens cleanup completed");
    }
    else
    {
      _logger.LogInformation("No expired refresh tokens to clean up");
    }
  }
}