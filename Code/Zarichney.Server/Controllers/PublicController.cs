using Microsoft.AspNetCore.Mvc;
using Zarichney.Services.Status;
using Microsoft.AspNetCore.Authorization;

namespace Zarichney.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api")]
public class PublicController(
  IStatusService statusService)
  : ControllerBase
{
  [HttpGet("health")]
  public IActionResult HealthCheck()
  {
    return Ok(new
    {
      Success = true,
      Time = DateTime.Now.ToLocalTime(),
      Version = "1.0.1",
      Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
    });
  }

  /// <summary>
  /// Returns the status of services based on their configuration availability.
  /// </summary>
  /// <returns>Dictionary mapping service names to their status information</returns>
  [HttpGet("status")]
  [ProducesResponseType(typeof(List<ServiceStatusInfo>), 200)]
  [ProducesResponseType(500)]
  public async Task<IActionResult> GetServicesStatus()
  {
    var result = await statusService.GetServiceStatusAsync();
    return Ok(result.Values.ToList());
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
