using MediatR;

namespace Zarichney.Server.Auth.Commands;

public record AddUserToRoleCommand(string UserId, string RoleName) : IRequest<RoleCommandResult>;

public record RemoveUserFromRoleCommand(string UserId, string RoleName) : IRequest<RoleCommandResult>;

public record GetUserRolesQuery(string UserId) : IRequest<RoleCommandResult>;

public record GetUsersInRoleQuery(string RoleName) : IRequest<List<UserRoleInfo>>;

public class UserRoleInfo
{
  public string UserId { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
}

public class RoleCommandResult
{
  public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
  public List<string> Roles { get; set; } = new();
}

public class AddUserToRoleCommandHandler(IRoleManager roleManager) : IRequestHandler<AddUserToRoleCommand, RoleCommandResult>
{
  private readonly IRoleManager roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));

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

public class GetUserRolesQueryHandler(IRoleManager roleManager) : IRequestHandler<GetUserRolesQuery, RoleCommandResult>
{
  public async Task<RoleCommandResult> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
  {
    var roles = await roleManager.GetUserRolesAsync(request.UserId);
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