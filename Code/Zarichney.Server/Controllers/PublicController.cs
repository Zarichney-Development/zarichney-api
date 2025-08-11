using Microsoft.AspNetCore.Mvc;
using Zarichney.Services.Status;
using Zarichney.Services.Logging;
using Zarichney.Services.Logging.Models;
using Microsoft.AspNetCore.Authorization;
using Zarichney.Controllers.Responses;

namespace Zarichney.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api")]
public class PublicController(
  IStatusService statusService,
  ILoggingService loggingService)
  : ControllerBase
{
  [HttpGet("health")]
  [ProducesResponseType(typeof(HealthCheckResponse), 200)]
  public IActionResult HealthCheck()
  {
    return Ok(new HealthCheckResponse(
      Success: true,
      Time: DateTime.Now.ToLocalTime(),
      Environment: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
    ));
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

  /// <summary>
  /// Gets the current logging system status and configuration
  /// </summary>
  /// <returns>Detailed logging status information</returns>
  [HttpGet("logging/status")]
  [ProducesResponseType(typeof(LoggingStatusResult), 200)]
  [ProducesResponseType(500)]
  public async Task<IActionResult> GetLoggingStatus()
  {
    try
    {
      var result = await loggingService.GetLoggingStatusAsync(HttpContext.RequestAborted);
      return Ok(result);
    }
    catch (Exception ex)
    {
      return StatusCode(500, new { error = "Failed to retrieve logging status", details = ex.Message });
    }
  }

  /// <summary>
  /// Tests connectivity to the specified Seq URL
  /// </summary>
  /// <param name="request">The Seq URL test request (optional, uses configured URL if not provided)</param>
  /// <returns>Connectivity test results</returns>
  [HttpPost("logging/test-seq")]
  [ProducesResponseType(typeof(SeqConnectivityResult), 200)]
  public async Task<IActionResult> TestSeqConnectivity([FromBody] TestSeqRequest? request = null)
  {
    var result = await loggingService.TestSeqConnectivityAsync(request?.Url, HttpContext.RequestAborted);
    return Ok(result);
  }

  /// <summary>
  /// Gets information about available logging methods
  /// </summary>
  /// <returns>Information about all available logging options</returns>
  [HttpGet("logging/methods")]
  [ProducesResponseType(typeof(LoggingMethodsResult), 200)]
  public async Task<IActionResult> GetAvailableLoggingMethods()
  {
    var result = await loggingService.GetAvailableLoggingMethodsAsync(HttpContext.RequestAborted);
    return Ok(result);
  }
}
