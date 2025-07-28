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

  /// <summary>
  /// Test endpoint for validating AI workflow analysis - intentionally includes various code quality issues
  /// Updated: Testing REAL Claude AI analysis (no more fake content) - expecting 3 AI comments
  /// </summary>
  [HttpGet("test-validation")]
  public async Task<IActionResult> TestValidation(string input)
  {
    // TODO: This method needs proper validation and error handling
    var results = new List<object>();
    
    // Potential security issue: SQL injection risk (for security analysis)
    var query = $"SELECT * FROM test WHERE value = '{input}'";
    
    // Code complexity issue: nested loops and conditions (for tech debt analysis)
    for (int i = 0; i < 100; i++)
    {
      for (int j = 0; j < 50; j++)
      {
        if (i % 2 == 0)
        {
          if (j % 3 == 0)
          {
            if (input != null && input.Length > 0)
            {
              results.Add(new { Index = i, Value = j, Query = query });
            }
          }
        }
      }
    }
    
    // Performance issue: blocking call without timeout (for quality analysis)
    await Task.Delay(5000);
    
    return Ok(results);
  }
}
