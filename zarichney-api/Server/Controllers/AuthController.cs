using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zarichney.Server.Auth;
using Zarichney.Server.Middleware;

namespace Zarichney.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger)
    : ControllerBase
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
        public string? Token { get; init; }
        public string? RefreshToken { get; init; }
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
            var (success, message, user, confirmationToken) = 
                await authService.RegisterUserAsync(request.Email, request.Password);

            if (!success)
                return BadRequestResponse(message);

            // Return confirmation token for development purposes
            return Ok(new AuthResponse
            {
                Success = true,
                Message = message,
                Email = user?.Email,
                Token = confirmationToken // This would be removed in production
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
            var (success, message, user, token, refreshToken) = 
                await authService.LoginUserAsync(request.Email, request.Password);

            if (!success)
                return BadRequestResponse(message);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = message,
                Token = token,
                RefreshToken = refreshToken,
                Email = user?.Email
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

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var (success, message, user, token, refreshToken) = 
                await authService.RefreshTokenAsync(request.RefreshToken);

            if (!success)
                return BadRequestResponse(message);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = message,
                Token = token,
                RefreshToken = refreshToken,
                Email = user?.Email
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
    public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var (success, message) = await authService.RevokeRefreshTokenAsync(request.RefreshToken);

            if (!success)
                return BadRequestResponse(message);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = message
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
    
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var (success, message, resetToken) = await authService.ForgotPasswordAsync(request.Email);

            if (!success)
                return BadRequestResponse(message);

            // For development, return the token directly in the response
            return Ok(new AuthResponse
            {
                Success = true,
                Message = message,
                Token = resetToken
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
            var (success, message) = await authService.ResetPasswordAsync(
                request.Email, request.Token, request.NewPassword);

            if (!success)
                return BadRequestResponse(message);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = message
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
    
    [HttpPost("confirm-email")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        try
        {
            var (success, message) = await authService.ConfirmEmailAsync(
                request.UserId, request.Token);

            if (!success)
                return BadRequestResponse(message);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = message
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
    
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request)
    {
        try
        {
            var (success, message, confirmationToken) = 
                await authService.ResendEmailConfirmationAsync(request.Email);

            if (!success)
                return BadRequestResponse(message);

            // For development, return the token directly in the response
            return Ok(new AuthResponse
            {
                Success = true,
                Message = message,
                Token = confirmationToken
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during confirmation email resend");
            return new ApiErrorResult(ex, "Failed to resend confirmation email");
        }
    }
}