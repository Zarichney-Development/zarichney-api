using Microsoft.AspNetCore.Mvc;

namespace Zarichney.Server.Controllers;

[ApiController]
[Route("api")]
public class PublicController : ControllerBase
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
}