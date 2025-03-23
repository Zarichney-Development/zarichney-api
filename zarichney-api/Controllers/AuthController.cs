using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Zarichney.Auth;
using Zarichney.Middleware;

namespace Zarichney.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IOptions<JwtSettings> jwtSettings,
    ILogger<AuthController> logger)
    : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

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
            // Validate the request
            if (string.IsNullOrEmpty(request.Email))
                return BadRequestResponse("Email is required");

            if (string.IsNullOrEmpty(request.Password))
                return BadRequestResponse("A password is required");

            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequestResponse("User with this email already exists");

            // Create the user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                logger.LogInformation("User {Email} created successfully", request.Email);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "User registered successfully",
                    Email = user.Email
                });
            }

            // If we get here, something went wrong
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Failed to create user {Email}: {Errors}", request.Email, errors);

            return BadRequestResponse(errors);
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
            // Validate the request
            if (string.IsNullOrEmpty(request.Email))
                return BadRequestResponse("Email is required");

            if (string.IsNullOrEmpty(request.Password))
                return BadRequestResponse("A password is required");

            // Find the user
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return BadRequestResponse("Invalid email or password");

            // Verify password
            var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                return BadRequestResponse("Invalid email or password");

            // Generate JWT token
            var token = GenerateJwtToken(user);

            logger.LogInformation("User {Email} logged in successfully", request.Email);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                Email = user.Email
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

    private string GenerateJwtToken(ApplicationUser user)
    {
        // Get secret key bytes
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        // Create signing credentials
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);

        // Create claims for the token
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Calculate expiration time
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        // Create token descriptor
        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: signingCredentials
        );

        // Create and return the token
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}