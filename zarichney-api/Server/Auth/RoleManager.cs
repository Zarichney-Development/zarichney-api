using Microsoft.AspNetCore.Identity;

namespace Zarichney.Server.Auth;

public interface IRoleManager
{
  Task EnsureRolesCreatedAsync();
  Task<bool> IsUserInRoleAsync(string userId, string roleName);
  Task<bool> AddUserToRoleAsync(string userId, string roleName);
  Task<bool> RemoveUserFromRoleAsync(string userId, string roleName);
  Task<IList<string>> GetUserRolesAsync(string userId);
  Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleName);
}

public class RoleManager(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ILogger<RoleManager> logger) : IRoleManager
{
  private static readonly string[] DefaultRoles = ["admin"];

  public async Task EnsureRolesCreatedAsync()
  {
    foreach (var roleName in DefaultRoles)
    {
      if (!await roleManager.RoleExistsAsync(roleName))
      {
        logger.LogInformation("Creating role: {RoleName}", roleName);
        var result = await roleManager.CreateAsync(new IdentityRole(roleName));

        if (!result.Succeeded)
        {
          var errors = string.Join(", ", result.Errors.Select(e => e.Description));
          logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, errors);
          throw new InvalidOperationException($"Failed to create role {roleName}: {errors}");
        }
      }
    }
  }

  public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
  {
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
      return false;
    }

    return await userManager.IsInRoleAsync(user, roleName);
  }

  public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
  {
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
      logger.LogWarning("User not found with ID: {UserId}", userId);
      return false;
    }

    if (!await roleManager.RoleExistsAsync(roleName))
    {
      logger.LogWarning("Role does not exist: {RoleName}", roleName);
      return false;
    }

    var result = await userManager.AddToRoleAsync(user, roleName);
    if (!result.Succeeded)
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Description));
      logger.LogError("Failed to add user {UserId} to role {RoleName}: {Errors}", userId, roleName, errors);
      return false;
    }

    logger.LogInformation("Added user {UserId} to role {RoleName}", userId, roleName);
    return true;
  }

  public async Task<bool> RemoveUserFromRoleAsync(string userId, string roleName)
  {
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
      logger.LogWarning("User not found with ID: {UserId}", userId);
      return false;
    }

    var result = await userManager.RemoveFromRoleAsync(user, roleName);
    if (!result.Succeeded)
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Description));
      logger.LogError("Failed to remove user {UserId} from role {RoleName}: {Errors}", userId, roleName, errors);
      return false;
    }

    logger.LogInformation("Removed user {UserId} from role {RoleName}", userId, roleName);
    return true;
  }

  public async Task<IList<string>> GetUserRolesAsync(string userId)
  {
    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
      logger.LogWarning("User not found with ID: {UserId}", userId);
      return new List<string>();
    }

    return await userManager.GetRolesAsync(user);
  }

  public async Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleName)
  {
    return (await userManager.GetUsersInRoleAsync(roleName)).ToList();
  }
}