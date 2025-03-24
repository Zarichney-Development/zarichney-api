using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zarichney.Server.Auth;
using Zarichney.Server.Auth.Commands;
using Zarichney.Server.Auth.Common;
using Zarichney.Server.Middleware;

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
            if (result.AccessToken != null && result.RefreshToken != null)
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
            if (result.AccessToken != null && result.RefreshToken != null)
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
            if (result.AccessToken != null && result.RefreshToken != null)
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
}