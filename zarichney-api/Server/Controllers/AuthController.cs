using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zarichney.Server.Auth;
using Zarichney.Server.Auth.Commands;
using Zarichney.Server.Auth.Models;

namespace Zarichney.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
  IMediator mediator,
  ILogger<AuthController> logger,
  ICookieAuthManager cookieManager
) : ControllerBase
{
  public class RegisterRequest
  {
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
  }

  public class LoginRequest
  {
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
  }

  public class AuthResponse
  {
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? Email { get; init; }
  }

  [HttpPost("register")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Register([FromBody] RegisterRequest request)
  {
    try
    {
      var result = await mediator.Send(new RegisterCommand(request.Email, request.Password));

      if (!result.Success)
        return BadRequestResponse(result.Message ?? "Registration failed");

      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message,
        Email = result.Email
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during user registration");
      return new ApiErrorResult(ex, "Failed to register user");
    }
  }

  [HttpPost("login")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Login([FromBody] LoginRequest request)
  {
    try
    {
      var result = await mediator.Send(new LoginCommand(request.Email, request.Password));

      if (!result.Success)
        return BadRequestResponse(result.Message ?? "Login failed");

      // Set HTTP-only cookies for access and refresh tokens
      if (result is { AccessToken: not null, RefreshToken: not null })
      {
        cookieManager.SetAuthCookies(HttpContext, result.AccessToken, result.RefreshToken);
      }

      // Only return success and message in the response body, not the tokens
      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message,
        Email = result.Email
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during user login");
      return new ApiErrorResult(ex, "Failed to login");
    }
  }

  // Helper method for creating BadRequest responses
  private BadRequestObjectResult BadRequestResponse(string message) =>
    BadRequest(new AuthResponse { Success = false, Message = message });

  [HttpPost("refresh")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> RefreshToken()
  {
    try
    {
      // Get refresh token from cookie instead of request body
      var refreshToken = cookieManager.GetRefreshTokenFromCookie(HttpContext);

      if (string.IsNullOrEmpty(refreshToken))
      {
        return Unauthorized(new AuthResponse
        {
          Success = false,
          Message = "Refresh token is missing"
        });
      }

      var result = await mediator.Send(new RefreshTokenCommand(refreshToken));

      if (!result.Success)
      {
        // Clear cookies on error
        cookieManager.ClearAuthCookies(HttpContext);
        return Unauthorized(new AuthResponse
        {
          Success = false,
          Message = result.Message
        });
      }

      // Set new HTTP-only cookies for updated tokens
      if (result is { AccessToken: not null, RefreshToken: not null })
      {
        cookieManager.SetAuthCookies(HttpContext, result.AccessToken, result.RefreshToken);
      }

      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message,
        Email = result.Email
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during token refresh");
      return new ApiErrorResult(ex, "Failed to refresh token");
    }
  }

  [Authorize]
  [HttpPost("revoke")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> RevokeRefreshToken()
  {
    try
    {
      // Get refresh token from cookie instead of request body
      var refreshToken = cookieManager.GetRefreshTokenFromCookie(HttpContext);

      if (string.IsNullOrEmpty(refreshToken))
      {
        return BadRequestResponse("Refresh token is missing");
      }

      var result = await mediator.Send(new RevokeTokenCommand(refreshToken));

      if (!result.Success)
        return BadRequestResponse(result.Message ?? "Revocation failed");

      // Clear cookies after successful revocation
      cookieManager.ClearAuthCookies(HttpContext);

      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during token revocation");
      return new ApiErrorResult(ex, "Failed to revoke token");
    }
  }

  public class ForgotPasswordRequest
  {
    public string Email { get; set; } = string.Empty;
  }

  public class ResetPasswordRequest
  {
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
  }

  [HttpPost("email-forgot-password")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> EmailForgetPassword([FromBody] ForgotPasswordRequest request)
  {
    try
    {
      var result = await mediator.Send(new ForgotPasswordCommand(request.Email));

      if (!result.Success)
        return BadRequestResponse(result.Message ?? "Request failed");

      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during forgot password process");
      return new ApiErrorResult(ex, "Failed to process forgot password request");
    }
  }

  [HttpPost("reset-password")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
  {
    try
    {
      var result = await mediator.Send(new ResetPasswordCommand(
        request.Email, request.Token, request.NewPassword));

      if (!result.Success)
        return BadRequestResponse(result.Message ?? "Reset failed");

      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during password reset");
      return new ApiErrorResult(ex, "Failed to reset password");
    }
  }

  public class ConfirmEmailRequest
  {
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
  }

  [HttpGet("confirm-email")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status302Found)]
  public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequest request)
  {
    try
    {
      var result = await mediator.Send(new ConfirmEmailCommand(request.UserId, request.Token));

      if (!result.Success)
        return BadRequestResponse(result.Message ?? "Email confirmation failed");

      // Set cookies after successful email confirmation
      if (result is { AccessToken: not null, RefreshToken: not null })
      {
        cookieManager.SetAuthCookies(HttpContext, result.AccessToken, result.RefreshToken);
      }

      // If we have a redirect URL, perform a redirect
      if (!string.IsNullOrEmpty(result.RedirectUrl))
      {
        return Redirect(result.RedirectUrl);
      }

      // Fallback to returning a JSON response if no redirect URL
      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during email confirmation");
      return new ApiErrorResult(ex, "Failed to confirm email");
    }
  }

  public class ResendConfirmationRequest
  {
    public string Email { get; set; } = string.Empty;
  }

  [HttpGet("resend-confirmation")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ResendConfirmation([FromQuery] ResendConfirmationRequest request)
  {
    try
    {
      var result = await mediator.Send(new ResendConfirmationCommand(request.Email));

      if (!result.Success)
        return BadRequestResponse(result.Message ?? "Resend confirmation failed");

      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during confirmation email resend");
      return new ApiErrorResult(ex, "Failed to resend confirmation email");
    }
  }

  [Authorize]
  [HttpPost("logout")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  public IActionResult Logout()
  {
    // Clear the auth cookies
    cookieManager.ClearAuthCookies(HttpContext);

    return Ok(new AuthResponse
    {
      Success = true,
      Message = "Logged out successfully"
    });
  }

  [Authorize(Roles = "admin")]
  [HttpPost("api-keys")]
  [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyCommand request)
  {
    try
    {
      var result = await mediator.Send(request);
      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error creating API key");
      return new ApiErrorResult(ex, "Failed to create API key");
    }
  }

  [Authorize(Roles = "admin")]
  [HttpGet("api-keys")]
  [ProducesResponseType(typeof(List<ApiKeyResponse>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetApiKeys()
  {
    try
    {
      var result = await mediator.Send(new GetApiKeysQuery());
      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving API keys");
      return new ApiErrorResult(ex, "Failed to retrieve API keys");
    }
  }

  [Authorize(Roles = "admin")]
  [HttpGet("api-keys/{id:int}")]
  [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetApiKeyById(int id)
  {
    try
    {
      var result = await mediator.Send(new GetApiKeyByIdQuery(id));
      if (result == null)
      {
        return NotFound();
      }

      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving API key by ID");
      return new ApiErrorResult(ex, "Failed to retrieve API key");
    }
  }

  [Authorize]
  [HttpDelete("api-keys/{id:int}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> RevokeApiKey(int id)
  {
    try
    {
      var result = await mediator.Send(new RevokeApiKeyCommand(id));
      if (!result)
      {
        return NotFound();
      }

      return Ok(new { success = true, message = "API key revoked successfully" });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error revoking API key");
      return new ApiErrorResult(ex, "Failed to revoke API key");
    }
  }

  // Role management endpoints - Admin only

  public class RoleRequest
  {
    public string UserId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
  }

  [Authorize(Roles = "admin")]
  [HttpPost("roles/add")]
  [ProducesResponseType(typeof(RoleCommandResult), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> AddUserToRole([FromBody] RoleRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.RoleName))
      {
        return BadRequest(new { success = false, message = "UserId and RoleName are required" });
      }

      var result = await mediator.Send(new AddUserToRoleCommand(request.UserId, request.RoleName));
      if (!result.Success)
      {
        return BadRequest(result);
      }

      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error adding user to role");
      return new ApiErrorResult(ex, "Failed to add user to role");
    }
  }

  [Authorize(Roles = "admin")]
  [HttpPost("roles/remove")]
  [ProducesResponseType(typeof(RoleCommandResult), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> RemoveUserFromRole([FromBody] RoleRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.RoleName))
      {
        return BadRequest(new { success = false, message = "UserId and RoleName are required" });
      }

      var result = await mediator.Send(new RemoveUserFromRoleCommand(request.UserId, request.RoleName));
      if (!result.Success)
      {
        return BadRequest(result);
      }

      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error removing user from role");
      return new ApiErrorResult(ex, "Failed to remove user from role");
    }
  }

  [Authorize(Roles = "admin")]
  [HttpGet("roles/user/{userId}")]
  [ProducesResponseType(typeof(RoleCommandResult), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetUserRoles(string userId)
  {
    try
    {
      var result = await mediator.Send(new GetUserRolesQuery(userId));
      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error getting user roles");
      return new ApiErrorResult(ex, "Failed to get user roles");
    }
  }

  [Authorize(Roles = "admin")]
  [HttpGet("roles/{roleName}/users")]
  [ProducesResponseType(typeof(List<UserRoleInfo>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetUsersInRole(string roleName)
  {
    try
    {
      var result = await mediator.Send(new GetUsersInRoleQuery(roleName));
      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error getting users in role");
      return new ApiErrorResult(ex, "Failed to get users in role");
    }
  }

  [HttpPost("setup-admin")]
  // [Authorize(Roles = "admin")]
  [ProducesResponseType(typeof(RoleCommandResult), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> SetupAdminUser([FromBody] RoleRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.UserId))
      {
        return BadRequest(new { success = false, message = "UserId is required" });
      }

      var result = await mediator.Send(new AddUserToRoleCommand(request.UserId, request.RoleName));
      if (!result.Success)
      {
        return BadRequest(result);
      }

      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error setting up admin user");
      return new ApiErrorResult(ex, "Failed to set up admin user");
    }
  }

  [Authorize]
  [HttpPost("refresh-claims")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> RefreshUserClaims()
  {
    try
    {
      // The user is already authenticated, so we can access their user ID
      var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var email = User.FindFirst(ClaimTypes.Email)?.Value;

      if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
      {
        return BadRequest(new AuthResponse
        {
          Success = false,
          Message = "User information not found in the current token"
        });
      }

      // Generate new tokens with updated claims
      var result = await mediator.Send(new RefreshUserClaimsCommand(userId, email));

      if (!result.Success)
      {
        return BadRequest(new AuthResponse
        {
          Success = false,
          Message = result.Message ?? "Failed to refresh user claims"
        });
      }

      // Update the auth cookies with the new tokens
      if (result is { AccessToken: not null, RefreshToken: not null })
      {
        cookieManager.SetAuthCookies(HttpContext, result.AccessToken, result.RefreshToken);
      }

      return Ok(new AuthResponse
      {
        Success = true,
        Message = "User claims refreshed successfully",
        Email = email
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error refreshing user claims");
      return new ApiErrorResult(ex, "Failed to refresh user claims");
    }
  }
  
  [Authorize]
  [HttpGet("check-admin")]
  [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
  public IActionResult CheckAdminRole()
  {
      // Get user ID from claims
      var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      
      // Check if user is in admin role
      var isAdmin = User.IsInRole("admin");
      
      // Get all roles from claims
      var roles = User.Claims
          .Where(c => c.Type == ClaimTypes.Role)
          .Select(c => c.Value)
          .ToList();
      
      return Ok(new Dictionary<string, object>
      {
          ["userId"] = userId ?? "unknown",
          ["isAdmin"] = isAdmin,
          ["roles"] = roles,
          ["authenticationType"] = User.Identity?.AuthenticationType ?? "unknown",
          ["isAuthenticated"] = User.Identity?.IsAuthenticated ?? false
      });
  }
}