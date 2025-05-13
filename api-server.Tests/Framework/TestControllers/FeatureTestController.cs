using Microsoft.AspNetCore.Mvc;
using Zarichney.Services.Status;

namespace Zarichney.Tests.Framework.TestControllers;

/// <summary>
/// Controller for testing feature availability middleware.
/// </summary>
[ApiController]
[Route("api/test-feature")]
public class FeatureTestController : ControllerBase
{
    /// <summary>
    /// Endpoint that requires the LLM feature.
    /// </summary>
    /// <returns>A simple message.</returns>
    [HttpGet("llm")]
    [RequiresFeatureEnabled(Feature.LLM)]
    public IActionResult LlmEndpoint()
    {
        return Ok("LLM feature is available");
    }
    
    /// <summary>
    /// Endpoint that requires the Email feature.
    /// </summary>
    /// <returns>A simple message.</returns>
    [HttpGet("email")]
    [RequiresFeatureEnabled(Feature.EmailSending)]
    public IActionResult EmailEndpoint()
    {
        return Ok("Email feature is available");
    }
    
    /// <summary>
    /// Endpoint that requires multiple features.
    /// </summary>
    /// <returns>A simple message.</returns>
    [HttpGet("multi")]
    [RequiresFeatureEnabled(Feature.LLM, Feature.EmailSending, Feature.AiServices)]
    public IActionResult MultiFeatureEndpoint()
    {
        return Ok("All required features are available");
    }
    
    /// <summary>
    /// Endpoint that doesn't require any specific features.
    /// </summary>
    /// <returns>A simple message.</returns>
    [HttpGet("available")]
    public IActionResult AvailableEndpoint()
    {
        return Ok("No features required");
    }
}