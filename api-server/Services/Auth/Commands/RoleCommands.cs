using MediatR;
using Microsoft.AspNetCore.Identity;
using Zarichney.Services.Auth.Models;

namespace Zarichney.Services.Auth.Commands;

public record AddUserToRoleCommand(string UserId, string RoleName) : IRequest<RoleCommandResult>;

public record RemoveUserFromRoleCommand(string UserId, string RoleName) : IRequest<RoleCommandResult>;

public record GetUserRolesQuery(string Identifier) : IRequest<RoleCommandResult>;

public record GetUsersInRoleQuery(string RoleName) : IRequest<List<UserRoleInfo>>;

public class RoleCommandResult
{
  public bool Success { get; init; }
  public string Message { get; init; } = string.Empty;
  public List<string> Roles { get; init; } = [];
}

public class AddUserToRoleCommandHandler(IRoleManager roleManager) : IRequestHandler<AddUserToRoleCommand, RoleCommandResult>
{
  public async Task<RoleCommandResult> Handle(AddUserToRoleCommand request, CancellationToken cancellationToken)
  {
    var result = await roleManager.AddUserToRoleAsync(request.UserId, request.RoleName);
    if (!result)
    {
      return new RoleCommandResult
      {
        Success = false,
        Message = $"Failed to add user to role {request.RoleName}"
      };
    }

    var roles = await roleManager.GetUserRolesAsync(request.UserId);
    return new RoleCommandResult
    {
      Success = true,
      Message = $"User added to role {request.RoleName}",
      Roles = roles.ToList()
    };
  }
}

public class RemoveUserFromRoleCommandHandler(IRoleManager roleManager) : IRequestHandler<RemoveUserFromRoleCommand, RoleCommandResult>
{
  public async Task<RoleCommandResult> Handle(RemoveUserFromRoleCommand request, CancellationToken cancellationToken)
  {
    var result = await roleManager.RemoveUserFromRoleAsync(request.UserId, request.RoleName);
    if (!result)
    {
      return new RoleCommandResult
      {
        Success = false,
        Message = $"Failed to remove user from role {request.RoleName}"
      };
    }

    var roles = await roleManager.GetUserRolesAsync(request.UserId);
    return new RoleCommandResult
    {
      Success = true,
      Message = $"User removed from role {request.RoleName}",
      Roles = roles.ToList()
    };
  }
}

public class GetUserRolesQueryHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<GetUserRolesQuery, RoleCommandResult>
{
  public async Task<RoleCommandResult> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
  {
    ApplicationUser? user;
    
    if (request.Identifier.Contains("@"))
    {
      // Looks like an email address
      user = await userManager.FindByEmailAsync(request.Identifier);
      if (user == null)
      {
        return new RoleCommandResult
        {
          Success = false,
          Message = "User not found with the provided email."
        };
      }
    }
    else
    {
      // Assume it's a user ID
      user = await userManager.FindByIdAsync(request.Identifier);
      if (user == null)
      {
        return new RoleCommandResult
        {
          Success = false,
          Message = "User not found with the provided ID."
        };
      }
    }
    
    // User found - get their roles
    var roles = await userManager.GetRolesAsync(user);
    return new RoleCommandResult
    {
      Success = true,
      Message = "Roles retrieved successfully",
      Roles = roles.ToList()
    };
  }
}

public class GetUsersInRoleQueryHandler(IRoleManager roleManager) : IRequestHandler<GetUsersInRoleQuery, List<UserRoleInfo>>
{
  public async Task<List<UserRoleInfo>> Handle(GetUsersInRoleQuery request, CancellationToken cancellationToken)
  {
    var users = await roleManager.GetUsersInRoleAsync(request.RoleName);
    return users.Select(u => new UserRoleInfo
    {
      UserId = u.Id,
      UserName = u.UserName ?? string.Empty,
      Email = u.Email ?? string.Empty
    }).ToList();
  }
}