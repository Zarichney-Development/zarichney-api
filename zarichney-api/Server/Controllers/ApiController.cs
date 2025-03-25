using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zarichney.Server.Services.Emails;

namespace Zarichney.Server.Controllers;

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
}