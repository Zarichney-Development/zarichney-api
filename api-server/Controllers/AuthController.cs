using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Models;
using Zarichney.Services.Auth.Commands;

namespace Zarichney.Controllers;

/// <summary>
/// Manages user authentication, registration, token handling, password management, email confirmation, API keys, and role management.
/// </summary>
/// <remarks>
/// This controller implements a cookie-based authentication system using JWTs (JSON Web Tokens).
/// Upon successful login or registration confirmation:
/// 1. An **access token** (short-lived JWT) is generated and stored in an HttpOnly, Secure cookie (`access_token`). This token is used to authenticate subsequent API requests.
/// 2. A **refresh token** (longer-lived, opaque token) is generated and stored in another HttpOnly, Secure cookie (`refresh_token`). This token is used to obtain a new access token when the current one expires, without requiring the user to log in again.
///
/// Front-end clients generally do **not** need to manually handle these tokens. The browser will automatically include the cookies in requests to the same domain. Ensure your HTTP client configuration (e.g., Axios, Fetch) includes credentials (`withCredentials: true` or `credentials: 'include'`).
///
/// API Key authentication is also supported for specific use cases (e.g., server-to-server communication) via the `Authorization: ApiKey YOUR_API_KEY` header, managed by separate middleware.
///
/// IMPORTANT: This controller's endpoints require the Identity Database to be available and properly configured. In Production, 
/// the application won't start if the Identity DB connection string is missing. In non-Production environments (e.g., Development),
/// the application will start, but the endpoints marked with [DependsOnService(ExternalServices.PostgresIdentityDb)] will return
/// a 503 Service Unavailable response if the Identity DB is unavailable. This allows the application to gracefully degrade
/// functionality in development and testing scenarios.
/// </remarks>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(
  IMediator mediator,
  ILogger<AuthController> logger,
  ICookieAuthManager cookieManager
) : ControllerBase
{
  /// <summary>
  /// Request model for user registration.
  /// </summary>
  public class RegisterRequest
  {
    /// <summary>
    /// The user's desired email address. Must be unique.
    /// </summary>
    /// <example>user@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's chosen password. Minimum length and complexity requirements may apply (defined server-side).
    /// </summary>
    /// <example>Password123!</example>
    public string Password { get; set; } = string.Empty;
  }

  /// <summary>
  /// Request model for user login.
  /// </summary>
  public class LoginRequest
  {
    /// <summary>
    /// The user's registered email address.
    /// </summary>
    /// <example>user@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's password.
    /// </summary>
    /// <example>Password123!</example>
    public string Password { get; set; } = string.Empty;
  }

  /// <summary>
  /// Standard response model for authentication operations.
  /// </summary>
  public class AuthResponse
  {
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; init; }

    /// <summary>
    /// A message describing the outcome of the operation.
    /// </summary>
    /// <example>Login successful.</example>
    public string? Message { get; init; }

    /// <summary>
    /// The email address associated with the operation, if applicable.
    /// </summary>
    /// <example>user@example.com</example>
    public string? Email { get; init; }
  }

  /// <summary>
  /// Registers a new user account.
  /// </summary>
  /// <remarks>
  /// Creates a new user with the provided email and password.
  /// Requires email confirmation before the user can log in. An email with a confirmation link will be sent to the provided address.
  /// </remarks>
  /// <param name="request">The registration details containing email and password.</param>
  /// <returns>An AuthResponse indicating success or failure.</returns>
  [HttpPost("register")]
  [AllowAnonymous]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Registers a new user.",
    Description = "Creates a user account and sends a confirmation email.")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
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

  /// <summary>
  /// Logs in a user and sets authentication cookies.
  /// </summary>
  /// <remarks>
  /// Authenticates the user with the provided email and password.
  /// If successful, sets HttpOnly `access_token` and `refresh_token` cookies in the response.
  /// The response body only contains success status and a message, not the tokens themselves.
  /// Requires the user's email to be confirmed first.
  /// </remarks>
  /// <param name="request">The login credentials.</param>
  /// <returns>An AuthResponse indicating success or failure. On success, auth cookies are set.</returns>
  [HttpPost("login")]
  [AllowAnonymous]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Logs in a user.",
    Description = "Authenticates credentials and sets HttpOnly access and refresh token cookies.")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
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

  /// <summary>
  /// Refreshes the access token using the refresh token cookie.
  /// </summary>
  /// <remarks>
  /// Uses the `refresh_token` cookie (sent automatically by the browser) to obtain a new `access_token` and potentially a new `refresh_token`.
  /// Updates the respective HttpOnly cookies in the response if successful.
  /// This should be called by the front-end when an API request returns a 401 Unauthorized status, indicating the access token has expired.
  /// If the refresh token is invalid or expired, the user will need to log in again.
  /// </remarks>
  /// <returns>An AuthResponse indicating success or failure. On success, auth cookies are updated.</returns>
  [HttpPost("refresh")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Refreshes the authentication tokens.",
    Description = "Uses the refresh token cookie to issue new access and refresh tokens as HttpOnly cookies.")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status401Unauthorized)] // Changed from 400 for token issues
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> RefreshToken()
  {
    try
    {
      // Get refresh token from cookie instead of request body
      var refreshToken = cookieManager.GetRefreshTokenFromCookie(HttpContext);

      if (string.IsNullOrEmpty(refreshToken))
      {
        return Unauthorized(new AuthResponse // Return 401 if refresh token is missing
        {
          Success = false,
          Message = "Refresh token is missing or invalid. Please login again."
        });
      }

      var result = await mediator.Send(new RefreshTokenCommand(refreshToken));

      if (!result.Success)
      {
        // Clear cookies on error (e.g., token revoked or invalid)
        cookieManager.ClearAuthCookies(HttpContext);
        return Unauthorized(new AuthResponse // Return 401 on failed refresh
        {
          Success = false,
          Message = result.Message ?? "Failed to refresh token. Please login again."
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
        Message = result.Message ?? "Tokens refreshed successfully.",
        Email = result.Email
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during token refresh");
      // Consider clearing cookies here too, depending on desired behavior on server error
      // cookieManager.ClearAuthCookies(HttpContext);
      return new ApiErrorResult(ex, "Failed to refresh token");
    }
  }

  /// <summary>
  /// Revokes the current refresh token associated with the session.
  /// </summary>
  /// <remarks>
  /// Invalidates the refresh token stored in the `refresh_token` cookie. This effectively logs the user out on that device the next time a refresh is attempted.
  /// Requires the user to be authenticated (valid `access_token` cookie).
  /// Clears the authentication cookies (`access_token`, `refresh_token`) upon successful revocation.
  /// This is typically used for a "log out everywhere else" feature or if a token is suspected to be compromised. Use the `logout` endpoint for a standard logout.
  /// </remarks>
  /// <returns>An AuthResponse indicating success or failure.</returns>
  [Authorize] // Requires a valid access token
  [HttpPost("revoke")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Revokes the current refresh token.",
    Description =
      "Invalidates the refresh token used by the current session (identified by cookie) and clears auth cookies. Requires authentication.")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)] // If not authenticated
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> RevokeRefreshToken()
  {
    try
    {
      // Get refresh token from cookie
      var refreshToken = cookieManager.GetRefreshTokenFromCookie(HttpContext);

      if (string.IsNullOrEmpty(refreshToken))
      {
        // Should not happen if [Authorize] works, but good practice
        return BadRequestResponse("Refresh token is missing in request.");
      }

      var result = await mediator.Send(new RevokeTokenCommand(refreshToken));

      if (!result.Success)
        return BadRequestResponse(result.Message ?? "Revocation failed");

      // Clear cookies after successful revocation
      cookieManager.ClearAuthCookies(HttpContext);

      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message ?? "Token revoked successfully."
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during token revocation");
      return new ApiErrorResult(ex, "Failed to revoke token");
    }
  }

  /// <summary>
  /// Request model to initiate the forgot password process.
  /// </summary>
  public class ForgotPasswordRequest
  {
    /// <summary>
    /// The email address of the user who forgot their password.
    /// </summary>
    /// <example>user@example.com</example>
    public string Email { get; set; } = string.Empty;
  }

  /// <summary>
  /// Request model to reset the password using a token.
  /// </summary>
  public class ResetPasswordRequest
  {
    /// <summary>
    /// The email address of the user resetting their password.
    /// </summary>
    /// <example>user@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The password reset token received via email.
    /// </summary>
    /// <example>CfDJ8A... (long token string)</example>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The user's desired new password.
    /// </summary>
    /// <example>NewSecurePassword123!</example>
    public string NewPassword { get; set; } = string.Empty;
  }

  /// <summary>
  /// Initiates the password reset process for a given email address.
  /// </summary>
  /// <remarks>
  /// Sends an email containing a password reset link (with a token) to the specified user, if the email exists.
  /// Always returns a success response to prevent email enumeration attacks, even if the email is not found.
  /// </remarks>
  /// <param name="request">The request containing the user's email.</param>
  /// <returns>An AuthResponse indicating the request was processed.</returns>
  [HttpPost("email-forgot-password")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Sends a password reset email.",
    Description = "Initiates the password reset flow by sending an email with a reset link/token.")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)] // For validation errors, if any
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> EmailForgetPassword([FromBody] ForgotPasswordRequest request)
  {
    try
    {
      // Consider adding model validation check here: if (!ModelState.IsValid) return BadRequest(...)
      var result = await mediator.Send(new ForgotPasswordCommand(request.Email));
      if (!result.Success)
        logger.LogWarning("Forgot password request failed: {Message}", result.Message);

      // Even if the result fails (e.g., email not found), return OK for security.
      // The 'result.Message' might contain info not suitable for the client in failure cases.
      return Ok(new AuthResponse
      {
        Success = true, // Always true to prevent enumeration
        Message = "If an account with that email exists, a password reset link has been sent." // Generic message
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during forgot password process");
      // Still return a generic success message to the client
      return Ok(new AuthResponse
      {
        Success = true,
        Message = "If an account with that email exists, a password reset link has been sent."
      });
      // Or, if you need to signal a server error:
      // return new ApiErrorResult(ex, "Failed to process forgot password request");
    }
  }

  /// <summary>
  /// Resets the user's password using a token received via email.
  /// </summary>
  /// <param name="request">The request containing the email, reset token, and new password.</param>
  /// <returns>An AuthResponse indicating success or failure of the password reset.</returns>
  [HttpPost("reset-password")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Resets the user password.",
    Description = "Sets a new password using the email, a valid reset token, and the new password.")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
  {
    try
    {
      // Consider adding model validation check here
      var result = await mediator.Send(new ResetPasswordCommand(
        request.Email, request.Token, request.NewPassword));

      if (!result.Success)
        return BadRequestResponse(result.Message ?? "Password reset failed. The token may be invalid or expired.");

      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message ?? "Password has been reset successfully."
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during password reset");
      return new ApiErrorResult(ex, "Failed to reset password");
    }
  }

  /// <summary>
  /// Request model for email confirmation. Parameters are typically in the query string of the confirmation link.
  /// </summary>
  public class ConfirmEmailRequest
  {
    /// <summary>
    /// The unique identifier of the user confirming their email.
    /// </summary>
    /// <example>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</example>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The email confirmation token.
    /// </summary>
    /// <example>CfDJ8A... (long token string)</example>
    public string Token { get; set; } = string.Empty;
  }

  /// <summary>
  /// Confirms a user's email address using a token sent via email.
  /// </summary>
  /// <remarks>
  /// This endpoint is typically accessed via a link clicked by the user in a confirmation email.
  /// Validates the user ID and token. If successful, marks the user's email as confirmed.
  /// Upon successful confirmation, it may automatically log the user in by setting authentication cookies (`access_token`, `refresh_token`).
  /// It might redirect the user to a specific front-end URL (e.g., login page or dashboard) or return a success message.
  /// </remarks>
  /// <param name="request">The request containing the UserId and Token from the confirmation link query parameters.</param>
  /// <returns>
  /// - 302 Found: Redirects to a specified URL upon success (if configured).
  /// - 200 OK (AuthResponse): If no redirect URL is configured, returns success status. Auth cookies may be set.
  /// - 400 Bad Request (AuthResponse): If confirmation fails (e.g., invalid token, user not found).
  /// - 500 Internal Server Error (ApiErrorResult): On unexpected server errors.
  /// </returns>
  [HttpGet("confirm-email")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Confirms a user's email address.",
    Description = "Validates the user ID and token from a confirmation link. May log the user in and redirect.")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)] // Success without redirect
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)] // Confirmation failed
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status302Found)] // Success with redirect
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequest request)
  {
    try
    {
      var result = await mediator.Send(new ConfirmEmailCommand(request.UserId, request.Token));

      if (!result.Success)
        return BadRequestResponse(result.Message ?? "Email confirmation failed. The link may be invalid or expired.");

      // Set cookies after successful email confirmation (auto-login)
      if (result is { AccessToken: not null, RefreshToken: not null })
      {
        cookieManager.SetAuthCookies(HttpContext, result.AccessToken, result.RefreshToken);
      }

      // If we have a redirect URL, perform a redirect
      if (!string.IsNullOrEmpty(result.RedirectUrl))
      {
        // Use a permanent redirect if appropriate, otherwise temporary
        return Redirect(result.RedirectUrl); // Defaults to 302 Found (Temporary Redirect)
      }

      // Fallback to returning a JSON response if no redirect URL
      return Ok(new AuthResponse
      {
        Success = true,
        Message = result.Message ?? "Email confirmed successfully."
        // Optionally include Email if available in result
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during email confirmation");
      return new ApiErrorResult(ex, "Failed to confirm email");
    }
  }

  /// <summary>
  /// Request model for resending the email confirmation link.
  /// </summary>
  public class ResendConfirmationRequest
  {
    /// <summary>
    /// The email address to resend the confirmation link to.
    /// </summary>
    /// <example>user@example.com</example>
    public string Email { get; set; } = string.Empty;
  }

  /// <summary>
  /// Resends the email confirmation link to the specified email address.
  /// </summary>
  /// <remarks>
  /// Useful if the user didn't receive the initial confirmation email or it expired.
  /// Finds the user by email and sends a new confirmation link.
  /// Always returns a success response to prevent email enumeration, even if the email doesn't exist or is already confirmed.
  /// </remarks>
  /// <param name="request">The request containing the user's email.</param>
  /// <returns>An AuthResponse indicating the request was processed.</returns>
  [HttpPost("resend-confirmation")] // Changed to POST as it performs an action
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Resends the email confirmation link.",
    Description = "Sends a new confirmation email to the specified address.")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)] // For validation errors
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult>
    ResendConfirmation([FromBody] ResendConfirmationRequest request) // Changed to FromBody
  {
    try
    {
      // Consider adding model validation: if (!ModelState.IsValid) return BadRequest(...)
      var result = await mediator.Send(new ResendConfirmationCommand(request.Email));

      if (!result.Success)
        logger.LogWarning("Resend confirmation request failed: {Message}", result.Message);

      // Similar to forgot password, return a generic success message regardless of outcome
      return Ok(new AuthResponse
      {
        Success = true, // Always true
        Message =
          "If an account requiring confirmation exists for that email, a new confirmation link has been sent." // Generic message
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during confirmation email resend");
      // Return generic message on error too
      return Ok(new AuthResponse
      {
        Success = true,
        Message = "If an account requiring confirmation exists for that email, a new confirmation link has been sent."
      });
      // Or signal server error:
      // return new ApiErrorResult(ex, "Failed to resend confirmation email");
    }
  }

  /// <summary>
  /// Logs the current user out by clearing authentication cookies.
  /// </summary>
  /// <remarks>
  /// Requires the user to be authenticated (valid `access_token` cookie).
  /// Clears the `access_token` and `refresh_token` cookies, effectively ending the user's session on the current browser.
  /// Note: This does not invalidate the refresh token itself (use the `revoke` endpoint for that). If the refresh token cookie were somehow obtained by an attacker *before* logout, it could potentially still be used until it expires or is revoked.
  /// </remarks>
  /// <returns>An AuthResponse indicating successful logout.</returns>
  [Authorize] // Requires a valid access token
  [HttpPost("logout")]
  [SwaggerOperation(Summary = "Logs the current user out.",
    Description = "Clears the HttpOnly authentication cookies for the current session. Requires authentication.")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)] // If not authenticated
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)] // Added for completeness
  public IActionResult Logout()
  {
    try
    {
      // Clear the auth cookies
      cookieManager.ClearAuthCookies(HttpContext);

      return Ok(new AuthResponse
      {
        Success = true,
        Message = "Logged out successfully"
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during logout");
      // Even on error, the client perspective is effectively logged out if cookies might be partially cleared or invalid.
      // You could return Ok or an error depending on desired behavior.
      return new ApiErrorResult(ex, "An error occurred during logout.");
    }
  }

  // --- API Key Management (Admin Only) ---

  /// <summary>
  /// Creates a new API key (Admin only).
  /// </summary>
  /// <remarks>
  /// Generates an API key that can be used for non-interactive authentication, typically for server-to-server communication or specific client integrations.
  /// Requires the user performing this action to have the 'admin' role.
  /// Authentication using the generated key is handled by separate middleware, typically expecting an `Authorization: ApiKey YOUR_API_KEY` header.
  /// The generated key is returned in the response **only once** upon creation and is not stored in plain text. Store it securely immediately.
  /// </remarks>
  /// <param name="request">Command containing details for the API key (e.g., description, expiry).</param>
  /// <returns>An ApiKeyResponse containing the details of the created key, including the key itself.</returns>
  [Authorize(Roles = "admin")]
  [HttpPost("api-keys")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Creates a new API key (Admin only).",
    Description = "Generates a key for non-interactive authentication. Requires 'admin' role.")]
  [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)] // e.g., Invalid input
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)] // Not logged in
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)] // Logged in but not admin
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyCommand request)
  {
    try
    {
      // Ensure the user making the request has the necessary privileges (double-check if needed, though [Authorize] handles it)
      var result = await mediator.Send(request);
      // Important: Ensure 'result' includes the generated key for the response.
      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error creating API key");
      return new ApiErrorResult(ex, "Failed to create API key");
    }
  }

  /// <summary>
  /// Retrieves a list of all active API keys (Admin only).
  /// </summary>
  /// <remarks>
  /// Returns metadata about existing API keys (ID, description, creation/expiry dates), but **does not** return the key values themselves for security reasons.
  /// Requires the user performing this action to have the 'admin' role.
  /// </remarks>
  /// <returns>A list of ApiKeyResponse objects (without the 'Key' property populated).</returns>
  [Authorize(Roles = "admin")]
  [HttpGet("api-keys")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Retrieves all API key metadata (Admin only).",
    Description = "Lists existing API keys without revealing the key values. Requires 'admin' role.")]
  [ProducesResponseType(typeof(List<ApiKeyResponse>),
    StatusCodes.Status200OK)] // Assuming ApiKeyResponse is adjusted to omit Key here
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> GetApiKeys()
  {
    try
    {
      var result = await mediator.Send(new GetApiKeysQuery());
      // Ensure the 'Key' property is null or excluded in the response objects
      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving API keys");
      return new ApiErrorResult(ex, "Failed to retrieve API keys");
    }
  }

  /// <summary>
  /// Retrieves metadata for a specific API key by ID (Admin only).
  /// </summary>
  /// <remarks>
  /// Returns metadata (ID, description, dates) for a single API key. **Does not** return the key value.
  /// Requires the user performing this action to have the 'admin' role.
  /// </remarks>
  /// <param name="id">The unique identifier of the API key.</param>
  /// <returns>An ApiKeyResponse object (without the 'Key' property) or 404 Not Found.</returns>
  [Authorize(Roles = "admin")]
  [HttpGet("api-keys/{id:int}")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Retrieves specific API key metadata by ID (Admin only).",
    Description = "Gets metadata for one API key without revealing the key value. Requires 'admin' role.")]
  [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status200OK)] // Assuming ApiKeyResponse omits Key
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> GetApiKeyById(int id)
  {
    try
    {
      var result = await mediator.Send(new GetApiKeyByIdQuery(id));
      if (result == null)
      {
        return NotFound(new { success = false, message = $"API key with ID {id} not found." }); // Provide clearer 404
      }

      // Ensure the 'Key' property is null or excluded
      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving API key by ID {ApiKeyId}", id);
      return new ApiErrorResult(ex, $"Failed to retrieve API key with ID {id}");
    }
  }

  /// <summary>
  /// Revokes (deactivates) an existing API key (Admin or Key Owner - adjust as needed).
  /// </summary>
  /// <remarks>
  /// Marks the specified API key as inactive, preventing it from being used for authentication. This is generally irreversible.
  /// Requires appropriate authorization (e.g., 'admin' role, or potentially the user associated with the key if applicable). The current implementation uses `[Authorize]` which implies any authenticated user can revoke *any* key by ID - this should likely be restricted further, perhaps to Admins or the key's creator if tracked. **Current setup allows any logged-in user to revoke any key.**
  /// </remarks>
  /// <param name="id">The unique identifier of the API key to revoke.</param>
  /// <returns>200 OK on success, 404 Not Found if the key doesn't exist, or standard error responses.</returns>
  [Authorize]
  [HttpDelete("api-keys/{id:int}")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Revokes an API key by ID.",
    Description =
      "Deactivates the specified API key. Requires appropriate authorization (WARNING: Currently any authenticated user).")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Example success response
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)] // If role check added
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> RevokeApiKey(int id)
  {
    try
    {
      // Consider adding an authorization check here if not solely relying on [Authorize]
      // e.g., check if User.IsInRole(Roles.Admin) or if the key belongs to the current user

      var result = await mediator.Send(new RevokeApiKeyCommand(id));
      if (!result) // Assuming the command returns true on success, false if not found
      {
        return NotFound(new { success = false, message = $"API key with ID {id} not found or already revoked." });
      }

      return Ok(new { success = true, message = $"API key {id} revoked successfully." });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error revoking API key {ApiKeyId}", id);
      return new ApiErrorResult(ex, $"Failed to revoke API key {id}");
    }
  }


  // --- Role Management (Admin Only) ---

  /// <summary>
  /// Request model for assigning or removing user roles.
  /// </summary>
  public class RoleRequest
  {
    /// <summary>
    /// The unique identifier of the target user (user ID or email address).
    /// </summary>
    /// <example>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</example>
    /// <remarks>This can be either a GUID user ID or an email address.</remarks>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// The name of the role to add or remove (e.g., "admin", "editor"). Case sensitivity depends on the underlying store.
    /// </summary>
    /// <example>admin</example>
    public string RoleName { get; set; } = string.Empty;
  }

  /// <summary>
  /// Assigns a specified role to a user (Admin only).
  /// </summary>
  /// <remarks>
  /// Requires the user performing this action to have the 'admin' role.
  /// Adds the user to the role if they are not already in it.
  /// </remarks>
  /// <param name="request">The request containing the UserId and RoleName.</param>
  /// <returns>A RoleCommandResult indicating success or failure.</returns>
  [Authorize(Roles = "admin")]
  [HttpPost("roles/add")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Adds a user to a role (Admin only).",
    Description = "Assigns the specified role to the user. Requires 'admin' role.")]
  [ProducesResponseType(typeof(RoleCommandResult), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(RoleCommandResult),
    StatusCodes.Status400BadRequest)] // Includes validation errors and business logic failures
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> AddUserToRole([FromBody] RoleRequest request)
  {
    // Note: Basic validation moved inside try-catch for consistent error handling
    try
    {
      if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.RoleName))
      {
        return BadRequest(new RoleCommandResult
        {
          Success = false,
          Message = "User identifier and RoleName are required."
        });
      }

      var result = await mediator.Send(new AddUserToRoleCommand(request.Identifier, request.RoleName));
      if (!result.Success)
      {
        // Use BadRequest for logical failures (user not found, role not found, etc.)
        return BadRequest(result);
      }

      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error adding user {Identifier} to role {RoleName}", request.Identifier, request.RoleName);
      return new ApiErrorResult(ex, "Failed to add user to role");
    }
  }

  /// <summary>
  /// Removes a specified role from a user (Admin only).
  /// </summary>
  /// <remarks>
  /// Requires the user performing this action to have the 'admin' role.
  /// Removes the user from the role if they are currently in it.
  /// </remarks>
  /// <param name="request">The request containing the UserId and RoleName.</param>
  /// <returns>A RoleCommandResult indicating success or failure.</returns>
  [Authorize(Roles = "admin")]
  [HttpPost("roles/remove")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Removes a user from a role (Admin only).",
    Description = "Revokes the specified role from the user. Requires 'admin' role.")]
  [ProducesResponseType(typeof(RoleCommandResult), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(RoleCommandResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> RemoveUserFromRole([FromBody] RoleRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.RoleName))
      {
        return BadRequest(new RoleCommandResult
        {
          Success = false,
          Message = "User identifier and RoleName are required."
        });
      }

      var result = await mediator.Send(new RemoveUserFromRoleCommand(request.Identifier, request.RoleName));
      if (!result.Success)
      {
        return BadRequest(result);
      }

      return Ok(result);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error removing user {Identifier} from role {RoleName}", request.Identifier, request.RoleName);
      return new ApiErrorResult(ex, "Failed to remove user from role");
    }
  }

  /// <summary>
  /// Gets all roles assigned to a specific user (Admin only).
  /// </summary>
  /// <remarks>
  /// Requires the user performing this action to have the 'admin' role.
  /// The user can be identified either by their ID or email address.
  /// </remarks>
  /// <param name="identifier">The unique identifier of the user (can be user ID or email address).</param>
  /// <returns>A RoleCommandResult containing the list of roles for the user.</returns>
  [Authorize(Roles = "admin")]
  [HttpGet("roles/user/{identifier}")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Gets roles for a specific user (Admin only).",
    Description = "Retrieves a list of roles assigned to the given user ID or email. Requires 'admin' role.")]
  [ProducesResponseType(typeof(RoleCommandResult),
    StatusCodes.Status200OK)] // Assumes RoleCommandResult contains the list
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)] // If identifier format is invalid
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status404NotFound)] // If user not found
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> GetUserRoles(string identifier)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(identifier))
      {
        return BadRequest(new RoleCommandResult
        {
          Success = false,
          Message = "User identifier (ID or email) is required."
        });
      }

      var result = await mediator.Send(new GetUserRolesQuery(identifier));
      if (!result.Success)
      {
        // Distinguish between "user not found" (404) and other errors if possible
        return NotFound(result); // Or BadRequest depending on result details
      }

      return Ok(result); // Contains list of roles
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error getting roles for user with identifier {Identifier}", identifier);
      return new ApiErrorResult(ex, "Failed to get user roles");
    }
  }

  /// <summary>
  /// Gets all users assigned to a specific role (Admin only).
  /// </summary>
  /// <remarks>
  /// Requires the user performing this action to have the 'admin' role.
  /// </remarks>
  /// <param name="roleName">The name of the role.</param>
  /// <returns>A list of users (e.g., UserRoleInfo containing ID and Email) in the specified role.</returns>
  [Authorize(Roles = "admin")]
  [HttpGet("roles/{roleName}/users")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Gets users within a specific role (Admin only).",
    Description = "Retrieves a list of users assigned to the given role name. Requires 'admin' role.")]
  [ProducesResponseType(typeof(List<UserRoleInfo>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)] // If role name format is invalid
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status403Forbidden)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status404NotFound)] // If role not found
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> GetUsersInRole(string roleName)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(roleName))
      {
        return BadRequest(new { success = false, message = "RoleName is required." });
      }

      var result = await mediator.Send(new GetUsersInRoleQuery(roleName));
      // Add check if role exists if the query doesn't handle it implicitly
      // if (result == null || !result.Any()) return NotFound(...);
      return Ok(result); // List of users
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error getting users in role {RoleName}", roleName);
      return new ApiErrorResult(ex, "Failed to get users in role");
    }
  }

  // --- Claim & Authentication Status ---

  /// <summary>
  /// Refreshes the claims included in the user's access token. (Requires Authentication)
  /// </summary>
  /// <remarks>
  /// Use this endpoint if user details relevant to claims (like roles, email, or other profile information) have been updated externally, and you want the current session's access token to reflect these changes without requiring a full logout/login.
  /// It takes the existing valid authentication context (from the `access_token` cookie), fetches the latest user data, generates new access and refresh tokens with updated claims, and replaces the existing `access_token` and `refresh_token` cookies.
  /// Requires the user to be currently authenticated via a valid `access_token` cookie.
  /// </remarks>
  /// <returns>An AuthResponse indicating success or failure. On success, auth cookies are updated with refreshed claims.</returns>
  [Authorize] // Requires user to be logged in
  [HttpPost("refresh-claims")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Refreshes claims in the access token.",
    Description =
      "Generates new tokens with updated user claims based on the current user session and replaces auth cookies. Requires authentication.")]
  [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(AuthResponse),
    StatusCodes.Status400BadRequest)] // If user info missing or refresh fails logically
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)] // If user is not authenticated
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public async Task<IActionResult> RefreshUserClaims()
  {
    try
    {
      // The user is already authenticated via [Authorize], so we can access claims.
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Recommended helper method
      var email = User.FindFirstValue(ClaimTypes.Email);

      if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
      {
        // This shouldn't happen if [Authorize] is working correctly, but good safeguard.
        logger.LogWarning("Attempted to refresh claims for user but essential claims (UserId/Email) were missing.");
        return BadRequest(new AuthResponse
        {
          Success = false,
          Message = "User information not found in the current token. Cannot refresh claims."
        });
      }

      // Generate new tokens with potentially updated claims from the source
      var result = await mediator.Send(new RefreshUserClaimsCommand(userId, email));

      if (!result.Success)
      {
        logger.LogWarning("Failed to refresh claims for user {UserId}: {Message}", userId, result.Message);
        return BadRequest(new AuthResponse
        {
          Success = false,
          Message = result.Message ?? "Failed to refresh user claims."
        });
      }

      // Update the auth cookies with the new tokens containing refreshed claims
      if (result is { AccessToken: not null, RefreshToken: not null })
      {
        cookieManager.SetAuthCookies(HttpContext, result.AccessToken, result.RefreshToken);
        logger.LogInformation("Successfully refreshed claims and updated tokens for user {UserId}.", userId);
      }
      else
      {
        // This case implies the command succeeded but didn't return tokens, which is unexpected.
        logger.LogError("RefreshUserClaimsCommand for user {UserId} reported success but did not return tokens.",
          userId);
        return new ApiErrorResult(new Exception(
          "Failed to refresh user claims due to an internal state issue.")); // Return 500
      }

      return Ok(new AuthResponse
      {
        Success = true,
        Message = "User claims refreshed successfully.",
        Email = email // Return the email associated with the session
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error refreshing user claims for user {UserId}",
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown");
      return new ApiErrorResult(ex, "Failed to refresh user claims");
    }
  }

  /// <summary>
  /// Checks the authentication status and basic claims of the current user. (Requires Authentication)
  /// </summary>
  /// <remarks>
  /// Provides a simple way for the front-end to verify if the user is currently authenticated (based on the presence and validity of the `access_token` cookie) and retrieve basic information like User ID and roles directly from the validated token claims.
  /// Useful for initializing UI state after page load or confirming session validity.
  /// Requires the user to be currently authenticated. If the access token is missing or invalid, this endpoint will return a 401 Unauthorized status.
  /// </remarks>
  /// <returns>A dictionary containing authentication status, user ID, roles, and authentication type. Returns 401 if not authenticated.</returns>
  [Authorize] // Ensures only authenticated users can access this
  [HttpGet("check-authentication")]
  [DependsOnService(ExternalServices.PostgresIdentityDb)]
  [SwaggerOperation(Summary = "Checks current user authentication status.",
    Description =
      "Returns basic info (ID, roles, auth status) based on the current valid access token cookie. Requires authentication.")]
  [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)] // If token missing/invalid
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)] // Added for completeness
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Description = "Returns when the Identity Database is unavailable")]
  public IActionResult CheckAuthentication()
  {
    try // Added try-catch block
    {
      // If code execution reaches here, [Authorize] was successful.
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var isAdmin = User.IsInRole("admin");
      var roles = User.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value)
        .ToList();
      var identity = User.Identity;

      var response = new Dictionary<string, object>
      {
        ["userId"] = userId ?? "unknown", // Should not be null if authenticated
        ["isAdmin"] = isAdmin,
        ["roles"] = roles,
        ["authenticationType"] = identity?.AuthenticationType ?? "unknown",
        ["isAuthenticated"] = identity?.IsAuthenticated ?? false // Should be true if [Authorize] passed
      };

      logger.LogInformation("Authentication check successful for user {UserId}.", userId ?? "unknown");
      return Ok(response);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during authentication check for user {UserId}",
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown");
      return new ApiErrorResult(ex, "Failed to check authentication status.");
    }
  }
}
