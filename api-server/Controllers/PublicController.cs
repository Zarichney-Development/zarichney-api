using Microsoft.AspNetCore.Mvc;
using Zarichney.Services.Status;
using Microsoft.AspNetCore.Authorization;

namespace Zarichney.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api")]
public class PublicController(
  IStatusService statusService,
  ILogger<PublicController> logger)
  : ControllerBase
{
  [HttpGet("health")]
  public IActionResult HealthCheck()
  {
    logger.LogError("testing log sink for automation test suite execution. This should be part of the test execution output window");
    return Ok(new
    {
      Success = true,
      Time = DateTime.Now.ToLocalTime()
    });
  }

  /// <summary>
  /// Returns the status of services based on their configuration availability.
  /// </summary>
  /// <returns>Dictionary mapping service names to their status information</returns>
  [HttpGet("status")]
  [ProducesResponseType(typeof(Dictionary<string, ServiceStatusInfo>), 200)]
  [ProducesResponseType(500)]
  public async Task<IActionResult> GetServicesStatus()
  {
    var result = await statusService.GetServiceStatusAsync();
    return Ok(result);
  }

  /// <summary>
  /// Returns the configuration item status.
  /// </summary>
  /// <returns>List of configuration item statuses</returns>
  [HttpGet("config")]
  [ProducesResponseType(typeof(IEnumerable<ConfigurationItemStatus>), 200)]
  [ProducesResponseType(500)]
  public async Task<IActionResult> Config()
  {
    var result = await statusService.GetConfigurationStatusAsync();
    return Ok(result);
  }
}
