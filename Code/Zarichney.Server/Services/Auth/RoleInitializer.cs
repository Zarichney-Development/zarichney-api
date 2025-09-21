
using Microsoft.AspNetCore.Identity;
using Zarichney.Config;
using Zarichney.Services.Email;

namespace Zarichney.Services.Auth;

/// <summary>
/// Hosted service responsible for initializing application roles and seeding a default administrator user
/// in non-Production environments when a real database is available.
/// </summary>
public class RoleInitializer(
  IServiceProvider serviceProvider,
  IConfiguration configuration,
  IWebHostEnvironment environment,
  ILogger<RoleInitializer> logger) : IHostedService
{
  private static readonly string ConnectionStringName = UserDbContext.UserDatabaseConnectionName;
  private const string AdminRoleName = "admin";

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

      // Seed default admin user in non-Production environments
      await SeedDefaultAdminUserAsync(scope);
    }
    catch (Exception ex)
    {
      // Catch potential errors during role creation (e.g., transient DB issues)
      logger.LogError(ex, "An error occurred while initializing roles and admin user.");
      // Depending on requirements, you might want to re-throw or handle differently
    }
  }

  /// <summary>
  /// Seeds a default administrator user in non-Production environments if configured and the user doesn't already exist.
  /// This operation is idempotent and will not create duplicate users.
  /// Uses EmailConfig.FromEmail as fallback for admin username if DefaultAdminUserConfig is incomplete.
  /// </summary>
  /// <param name="scope">The dependency injection scope.</param>
  private async Task SeedDefaultAdminUserAsync(IServiceScope scope)
  {
    // Only seed admin user in non-Production environments
    if (environment.IsProduction())
    {
      logger.LogDebug("Skipping default admin user seeding in Production environment.");
      return;
    }

    // Get the default admin user configuration
    var adminConfig = configuration.GetSection("DefaultAdminUser").Get<DefaultAdminUserConfig>();
    var emailConfig = configuration.GetSection("EmailConfig").Get<EmailConfig>();

    // Determine admin user details with fallback logic
    string adminEmail;
    string adminUserName;
    string adminPassword;

    if (adminConfig != null &&
        !string.IsNullOrWhiteSpace(adminConfig.Email) &&
        !string.IsNullOrWhiteSpace(adminConfig.UserName) &&
        !string.IsNullOrWhiteSpace(adminConfig.Password))
    {
      // Use complete configuration
      adminEmail = adminConfig.Email;
      adminUserName = adminConfig.UserName;
      adminPassword = adminConfig.Password;
      logger.LogInformation("Using DefaultAdminUser configuration for admin user seeding.");
    }
    else
    {
      // Use fallback logic
      adminEmail = !string.IsNullOrWhiteSpace(emailConfig?.FromEmail)
        ? emailConfig.FromEmail
        : "test@gmail.com";
      adminUserName = adminEmail; // Use email as username for fallback
      adminPassword = "nimda";

      logger.LogInformation("DefaultAdminUser configuration incomplete. Using fallback: email='{Email}', password='nimda'", adminEmail);
    }

    // Validate email format
    if (!adminEmail.Contains('@') || adminEmail.Length > 254)
    {
      logger.LogError("Admin email '{Email}' is invalid. Skipping admin user seeding.", adminEmail);
      return;
    }

    // Validate username
    if (adminUserName.Length > 256)
    {
      logger.LogError("Admin username is too long. Skipping admin user seeding.");
      return;
    }

    logger.LogInformation("Seeding default admin user in {Environment} environment...", environment.EnvironmentName);

    try
    {
      var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

      // Check if user already exists by email or username
      var existingUserByEmail = await userManager.FindByEmailAsync(adminEmail);
      var existingUserByUserName = await userManager.FindByNameAsync(adminUserName);

      if (existingUserByEmail != null || existingUserByUserName != null)
      {
        logger.LogInformation("Default admin user already exists. Ensuring admin role assignment...");

        // Ensure existing user has admin role (idempotent operation)
        var existingUser = existingUserByEmail ?? existingUserByUserName;
        if (existingUser != null)
        {
          await EnsureUserInAdminRoleAsync(userManager, existingUser);
        }
        return;
      }

      // Create new admin user
      var adminUser = new ApplicationUser
      {
        UserName = adminUserName,
        Email = adminEmail,
        EmailConfirmed = true // Auto-confirm email for dev environment
      };

      var createResult = await userManager.CreateAsync(adminUser, adminPassword);

      if (createResult.Succeeded)
      {
        logger.LogInformation("Default admin user created successfully: {Email}", adminEmail);

        // Assign admin role to the new user
        await EnsureUserInAdminRoleAsync(userManager, adminUser);
      }
      else
      {
        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
        logger.LogError("Failed to create default admin user: {Errors}", errors);
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred while seeding the default admin user.");
    }
  }

  /// <summary>
  /// Ensures that the specified user is assigned to the admin role.
  /// This operation is idempotent and will not fail if the user is already in the role.
  /// </summary>
  /// <param name="userManager">The user manager.</param>
  /// <param name="user">The user to assign the admin role to.</param>
  private async Task EnsureUserInAdminRoleAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
  {
    try
    {
      if (!await userManager.IsInRoleAsync(user, AdminRoleName))
      {
        var roleResult = await userManager.AddToRoleAsync(user, AdminRoleName);
        if (roleResult.Succeeded)
        {
          logger.LogInformation("Admin role assigned to user: {Email}", user.Email);
        }
        else
        {
          var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
          logger.LogError("Failed to assign admin role to user {Email}: {Errors}", user.Email, errors);
        }
      }
      else
      {
        logger.LogDebug("User {Email} already has admin role.", user.Email);
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred while assigning admin role to user {Email}.", user.Email);
    }
  }

  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
