using Microsoft.AspNetCore.Mvc;
using Zarichney.Server.Services.Status;
using Microsoft.AspNetCore.Authorization;

namespace Zarichney.Server.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api")]
public class PublicController(IStatusService statusService) : ControllerBase
{
  [HttpGet("health")]
  public IActionResult HealthCheck()
  {
    return Ok(new
    {
      Success = true,
      Time = DateTime.Now.ToLocalTime()
    });
  }

  /// <summary>
  /// Returns the status of critical configuration values (API keys, secrets, connection strings).
  /// </summary>
  /// <returns>List of configuration item statuses</returns>
  [HttpGet("status/config")]
  [ProducesResponseType(typeof(List<ConfigurationItemStatus>), 200)]
  [ProducesResponseType(500)]
  public async Task<IActionResult> GetConfigurationStatus()
  {
    var result = await statusService.GetConfigurationStatusAsync();
    return Ok(result);
  }
}