using Zarichney.Services.Status;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Zarichney.Services.AI;

namespace Zarichney.Controllers;

public class TranscribeAudioRequest
{
  /// <summary>
  /// The audio file (e.g., WAV, MP3, WEBM) to transcribe.
  /// The parameter name in the form data must be 'AudioFile'.
  /// </summary>
  [FromForm(Name = "audioFile")]
  public IFormFile? AudioFile { get; set; }
}


/// <summary>
/// Request model for the completion endpoint, accepting either text or audio input.
/// </summary>
public class CompletionRequest
{
  /// <summary>
  /// The text prompt to send to the LLM. Use this OR audioPrompt.
  /// </summary>
  /// <example>Write a short story about a robot learning to cook.</example>
  public string? TextPrompt { get; set; }

  /// <summary>
  /// The audio prompt (e.g., WAV, MP3, WEBM) to be transcribed and then sent to the LLM. Use this OR textPrompt.
  /// The request must use multipart/form-data encoding.
  /// </summary>
  public IFormFile? AudioPrompt { get; set; }
}

/// <summary>
/// Controller for handling AI-related operations like text completion and audio transcription.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
[DependsOnService(ExternalServices.OpenAiApi)]
public class AiController(
  IAiService aiService,
  ILogger<AiController> logger
) : ControllerBase
{

  /// <summary>
  /// Gets a completion from the configured Language Model (LLM) based on either text or audio input.
  /// </summary>
  /// <remarks>
  /// This endpoint accepts a `multipart/form-data` request. You must provide *either* a `textPrompt` (as a form field) *or* an `audioPrompt` (as a file upload).
  /// If `audioPrompt` is provided, it will be transcribed first, and the resulting text will be used as the prompt for the LLM.
  /// Requires authentication. The session ID might be included in the response headers.
  /// </remarks>
  /// <param name="request">The request containing either a text prompt or an audio file prompt.</param>
  /// <returns>A JSON object containing the LLM's response, the source type (text/audio), and the transcribed prompt if applicable.</returns>
  [HttpPost("completion")]
  [Consumes("multipart/form-data")] // Crucial hint for Swagger regarding file uploads/form fields
  [SwaggerOperation(Summary = "Generates LLM completion from text or audio.",
    Description = "Accepts either textPrompt (form field) or audioPrompt (file upload) via multipart/form-data.")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetCompletion([FromForm] CompletionRequest request)
  {
    try
    {

      // Create prompt from request
      var prompt = new CompletionPrompt
      {
        TextPrompt = request.TextPrompt,
        AudioFile = request.AudioPrompt
      };

      logger.LogInformation("Processing completion request");

      // Get completion from service
      var result = await aiService.GetCompletionAsync(prompt);

      // Add session ID to response headers
      Response.Headers.Append("X-Session-Id", result.SessionId.ToString());

      // Return the result
      return Ok(new
      {
        response = result.Response,
        sourceType = result.SourceType,
        transcribedPrompt = result.TranscribedPrompt
      });
    }
    catch (ArgumentException ex)
    {
      logger.LogWarning(ex, "{Method}: Invalid prompt provided", nameof(GetCompletion));
      return BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
      logger.LogError(ex, "{Method}: Operation failed", nameof(GetCompletion));
      return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to get LLM completion", nameof(GetCompletion));
      return new ApiErrorResult(ex, $"{nameof(GetCompletion)}: Failed to get LLM completion.");
    }
  }


  /// <summary>
  /// Transcribes the provided audio file.
  /// </summary>
  /// <remarks>
  /// This endpoint accepts a `multipart/form-data` request containing a single audio file.
  /// The form field containing the file *must* be named 'audioFile'.
  /// It transcribes the audio, saves the original audio and the resulting transcript (e.g., to GitHub via `IGitHubService`), and returns the transcript along with file metadata.
  /// Requires authentication.
  /// </remarks>
  /// <param name="request">The request object containing the audio file.</param>
  /// <returns>A JSON object containing the transcript, filenames, timestamp, and a success message.</returns>
  [HttpPost("transcribe")]
  [Consumes("multipart/form-data")] // Crucial hint for Swagger
  [SwaggerOperation(Summary = "Transcribes an audio file.",
    Description =
      "Accepts an audio file via multipart/form-data (parameter name 'audioFile'), transcribes it, and optionally saves files.")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> TranscribeAudio([FromForm] TranscribeAudioRequest request)
  {
    try
    {
      // Access the file via the request object
      var audioFile = request.AudioFile;

      // Enhanced logging to debug potential binding issues with safe Request.Form access
      int fileCount = 0;
      var httpRequest = Request;
      if (httpRequest?.HasFormContentType == true)
      {
        fileCount = httpRequest.Form.Files.Count;
      }

      logger.LogInformation(
        "Received transcribe request. Request.Form.Files.Count: {FileCount}, Request DTO File null: {IsParamNull}",
        fileCount,
        audioFile == null);

      // Check for form field name issues when files are present but audioFile is null
      if (audioFile == null && fileCount > 0)
      {
        var fileNames = string.Join(", ", Request.Form.Files.Select(f => f.Name));
        logger.LogWarning(
          "Parameter 'audioFile' was null, but Request.Form.Files contains files named: {FileNames}. Ensure the form field name matches 'audioFile'.",
          fileNames);
        return BadRequest("Audio file is required. Ensure the form field name is 'audioFile'.");
      }

      // Ensure audioFile is not null before processing
      if (audioFile == null)
      {
        return BadRequest("Audio file is required.");
      }

      // Process the audio file through the service
      var result = await aiService.ProcessAudioTranscriptionAsync(audioFile);

      // Return the result
      return Ok(new
      {
        message = result.Message,
        audioFile = result.AudioFileName,
        transcriptFile = result.TranscriptFileName,
        timestamp = result.Timestamp,
        transcript = result.Transcript
      });
    }
    catch (InvalidDataException ex)
    {
      // Handle Request.Form parsing exceptions gracefully
      logger.LogDebug(ex, "{Method}: Unable to read Request.Form; proceeding with request DTO validation only", nameof(TranscribeAudio));

      // Fallback to basic audioFile validation when Request.Form is unavailable
      if (request.AudioFile == null)
      {
        return BadRequest("Audio file is required.");
      }

      // Process with available audioFile data
      var result = await aiService.ProcessAudioTranscriptionAsync(request.AudioFile);
      return Ok(new
      {
        message = result.Message,
        audioFile = result.AudioFileName,
        transcriptFile = result.TranscriptFileName,
        timestamp = result.Timestamp,
        transcript = result.Transcript
      });
    }
    catch (ArgumentNullException)
    {
      return BadRequest("Audio file is required.");
    }
    catch (ArgumentException ex)
    {
      logger.LogWarning(ex, "Invalid audio file");
      return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to process audio file", nameof(TranscribeAudio));
      return new ApiErrorResult(ex, "Failed to process audio file.");
    }
  }
}
