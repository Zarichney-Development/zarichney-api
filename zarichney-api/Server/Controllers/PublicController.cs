using Microsoft.AspNetCore.Mvc;
using Zarichney.Server.Auth;
using Zarichney.Server.Middleware;
using ILogger = Serilog.ILogger;

namespace Zarichney.Server.Controllers;

[ApiController]
[Route("api")]
public class PublicController(
  ILogger logger,
  ApiKeyConfig apiKeyConfig
) : ControllerBase
{
  public record KeyValidationRequest(string Key);
  
  [HttpGet("health")]
  public IActionResult HealthCheck()
  {
    return Ok(new
    {
      Success = true,
      Time = DateTime.Now.ToLocalTime()
    });
  }

  [HttpPost("key/validate")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status401Unauthorized)]
  public IActionResult ValidateKey([FromBody] KeyValidationRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.Key))
      {
        logger.Warning("{Method}: Empty api key received", nameof(ValidateKey));
        return BadRequest("Password is required");
      }

      // Check if the password matches any valid API key
      if (!apiKeyConfig.ValidApiKeys.Contains(request.Key))
      {
        logger.Warning("{Method}: Invalid api key attempt", nameof(ValidateKey));
        return Unauthorized(new
        {
          error = "Invalid api key",
          timestamp = DateTimeOffset.UtcNow
        });
      }

      return Ok(new
      {
        message = "Valid api key",
        timestamp = DateTimeOffset.UtcNow
      });
    }
    catch (Exception ex)
    {
      logger.Error(ex, "{Method}: Unexpected error during key validation", nameof(ValidateKey));
      return new ApiErrorResult(ex, $"{nameof(ValidateKey)}: Failed to validate key");
    }
  }
}