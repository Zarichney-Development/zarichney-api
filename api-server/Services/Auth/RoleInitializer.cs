namespace Zarichney.Services.Auth;

public class RoleInitializer(
    IServiceProvider serviceProvider,
    ILogger<RoleInitializer> logger) : IHostedService
{

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    using var scope = serviceProvider.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();

    try
    {
      logger.LogInformation("Initializing application roles...");
      await roleManager.EnsureRolesCreatedAsync();
      logger.LogInformation("Roles initialized successfully.");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred while initializing roles");
    }
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}