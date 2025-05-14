using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zarichney.Services.Email;
using Zarichney.Services.Status;

namespace Zarichney.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ApiController(
  IEmailService emailService,
  ILogger<ApiController> logger
) : ControllerBase
{

  [HttpPost("email/validate")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [DependsOnService(ExternalServices.EmailValidation)]
  public async Task<IActionResult> ValidateEmail([FromQuery] string email)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(email))
      {
        logger.LogWarning("{Method}: Empty email received", nameof(ValidateEmail));
        return BadRequest("Email parameter is required");
      }

      await emailService.ValidateEmail(email);
      return Ok("Valid");
    }
    catch (InvalidEmailException ex)
    {
      logger.LogWarning(ex, "{Method}: Invalid email validation for {Email}",
        nameof(ValidateEmail), email);
      return BadRequest(new
      {
        error = ex.Message,
        email = ex.Email,
        reason = ex.Reason.ToString()
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to validate email: {Email}",
        nameof(ValidateEmail), email);
      return new ApiErrorResult(ex, $"{nameof(ValidateEmail)}: Failed to validate email");
    }
  }

  [HttpGet("health/secure")]
  public IActionResult HealthCheck()
  {
    return Ok(new
    {
      Success = true,
      Time = DateTime.Now.ToLocalTime(),
      User = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "Unknown"
    });
  }

  [HttpGet("test-auth")]
  public IActionResult TestAuth()
  {
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
    var authType = User.Identity?.AuthenticationType ?? "None";
    var isAdmin = User.IsInRole("admin");
    var allRoles = User.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value)
        .ToList();

    var isApiKeyAuth = HttpContext.Items.ContainsKey("ApiKey");
    var apiKey = isApiKeyAuth ? HttpContext.Items["ApiKey"]?.ToString() : null;

    return Ok(new
    {
      userId,
      authType,
      isAuthenticated = User.Identity?.IsAuthenticated ?? false,
      isAdmin,
      roles = allRoles,
      isApiKeyAuth,
      apiKeyInfo = isApiKeyAuth ? new { keyId = apiKey } : null,
      message = "Authentication successful!"
    });
  }
}
