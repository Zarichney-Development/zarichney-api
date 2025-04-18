
namespace Zarichney.Services.Auth;

public class RoleInitializer(
  IServiceProvider serviceProvider,
  IConfiguration configuration,
  ILogger<RoleInitializer> logger) : IHostedService
{
  private const string ConnectionStringName = "IdentityConnection";

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    // Check for connection string first
    var connectionString = configuration.GetConnectionString(ConnectionStringName);
    if (string.IsNullOrEmpty(connectionString))
    {
      logger.LogWarning("{ServiceName} requires the '{ConnectionStringName}' connection string to be configured. Skipping role initialization.",
        nameof(RoleInitializer),
        ConnectionStringName);
      return; // Exit early if no connection string
    }

    // Proceed with initialization if connection string exists
    logger.LogInformation("Initializing application roles...");
    try
    {
      using var scope = serviceProvider.CreateScope();
      var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();
      await roleManager.EnsureRolesCreatedAsync(); // EnsureRolesCreatedAsync will now only be called if DB is configured
      logger.LogInformation("Roles initialized successfully.");
    }
    catch (Exception ex)
    {
      // Catch potential errors during role creation (e.g., transient DB issues)
      logger.LogError(ex, "An error occurred while initializing roles.");
      // Depending on requirements, you might want to re-throw or handle differently
    }
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}