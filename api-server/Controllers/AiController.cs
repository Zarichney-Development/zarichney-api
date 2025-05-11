using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Zarichney.Config;
using Zarichney.Services.AI;
using Zarichney.Services.Email;
using Zarichney.Services.GitHub;
using Zarichney.Services.Sessions;

namespace Zarichney.Controllers;

public class TranscribeAudioRequest
{
  /// <summary>
  /// The audio file (e.g., WAV, MP3, WEBM) to transcribe.
  /// The parameter name in the form data must be 'AudioFile'.
  /// </summary>
  [FromForm(Name =
    "audioFile")] // Explicitly match the expected form field name if needed, otherwise name matching works
  public IFormFile? AudioFile { get; set; }
}

/// <summary>
/// Controller for handling AI-related operations like text completion and audio transcription.
/// </summary>
[ApiController]
[Route("api")]
[Authorize] // Requires authentication for all endpoints in this controller
[Produces("application/json")]
public class AiController(
  IEmailService emailService,
  ITranscribeService transcribeService,
  IGitHubService githubService,
  EmailConfig emailConfig,
  ILlmService llmService,
  ILogger<AiController> logger,
  ISessionManager sessionManager,
  IScopeContainer scope
) : ControllerBase
{
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
  [RequiresFeatureEnabled("Llm")]
  [SwaggerOperation(Summary = "Generates LLM completion from text or audio.",
    Description = "Accepts either textPrompt (form field) or audioPrompt (file upload) via multipart/form-data.")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // More specific type if possible
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetCompletion([FromForm] CompletionRequest request)
  {
    try
    {
      string prompt;

      // Handle audio input
      if (request.AudioPrompt != null)
      {
        logger.LogInformation(
          "Received audio prompt. ContentType: {ContentType}, FileName: {FileName}, Length: {Length}",
          request.AudioPrompt.ContentType,
          request.AudioPrompt.FileName,
          request.AudioPrompt.Length);

        if (request.AudioPrompt.Length == 0)
        {
          logger.LogWarning("{Method}: Empty audio file received", nameof(GetCompletion));
          return BadRequest("Audio file must not be empty.");
        }

        // Basic content type check - consider more specific checks if needed
        if (string.IsNullOrEmpty(request.AudioPrompt.ContentType) ||
            !request.AudioPrompt.ContentType.StartsWith("audio/"))
        {
          logger.LogWarning("{Method}: Invalid content type: {ContentType}",
            nameof(GetCompletion), request.AudioPrompt.ContentType);
          return BadRequest($"Invalid or missing content type: {request.AudioPrompt.ContentType}. Expected audio/*.");
        }

        // Transcribe audio to text
        using var ms = new MemoryStream();
        await request.AudioPrompt.CopyToAsync(ms);
        ms.Position = 0;

        try
        {
          prompt = await transcribeService.TranscribeAudioAsync(ms);
          logger.LogInformation("{Method}: Successfully transcribed audio prompt", nameof(GetCompletion));
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "{Method}: Failed to transcribe audio prompt", nameof(GetCompletion));
          // Avoid exposing internal details in BadRequest
          return BadRequest("Failed to transcribe the provided audio prompt.");
        }
      }
      // Handle text input
      else if (!string.IsNullOrWhiteSpace(request.TextPrompt))
      {
        prompt = request.TextPrompt;
        logger.LogInformation("Received text prompt.");
      }
      else
      {
        logger.LogWarning("{Method}: No valid prompt provided", nameof(GetCompletion));
        return BadRequest("Either 'textPrompt' form field or 'audioPrompt' file must be provided.");
      }

      var response = await llmService.GetCompletionContent(prompt);

      var session = await sessionManager.GetSessionByScope(scope.Id);
      Response.Headers.Append("X-Session-Id", session.Id.ToString()); // Use Append for headers

      return Ok(new
      {
        response,
        sourceType = request.AudioPrompt != null ? "audio" : "text",
        transcribedPrompt = request.AudioPrompt != null ? prompt : null
      });
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
  /// <param name="request">The request object containing the audio file.</param> // Add parameter docs for the new request object
  /// <returns>A JSON object containing the transcript, filenames, timestamp, and a success message.</returns>
  [HttpPost("transcribe")]
  [Consumes("multipart/form-data")] // Crucial hint for Swagger
  [RequiresFeatureEnabled("Llm", "GitHub")] // Requires both LLM for transcription and GitHub for storage
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

      // Enhanced logging to debug potential binding issues
      logger.LogInformation(
        "Received transcribe request. Request.Form.Files.Count: {FileCount}, Request DTO File null: {IsParamNull}",
        Request.Form.Files.Count,
        audioFile == null);

      if (audioFile == null) // Primary check on the bound parameter
      {
        // Check if maybe the name was wrong in the client request
        if (Request.Form.Files.Count > 0)
        {
          var fileNames = string.Join(", ", Request.Form.Files.Select(f => f.Name));
          logger.LogWarning(
            "Parameter 'audioFile' was null, but Request.Form.Files contains files named: {FileNames}. Ensure the form field name matches 'audioFile'.",
            fileNames);
          return BadRequest("Audio file is required. Ensure the form field name is 'audioFile'.");
        }

        logger.LogWarning(
          "{Method}: No audio file received. Parameter 'audioFile' was null and Request.Form.Files was empty.",
          nameof(TranscribeAudio));
        return BadRequest("Audio file is required.");
      }

      if (audioFile.Length == 0)
      {
        logger.LogWarning("{Method}: Empty audio file received ('{FileName}')", nameof(TranscribeAudio),
          audioFile.FileName);
        return BadRequest("Audio file must not be empty.");
      }

      logger.LogInformation(
        "Processing audio file. ContentType: {ContentType}, FileName: {FileName}, Length: {Length}",
        audioFile.ContentType,
        audioFile.FileName,
        audioFile.Length);


      // Basic content type check - refine if necessary (e.g., allow specific types)
      if (string.IsNullOrEmpty(audioFile.ContentType) || !audioFile.ContentType.StartsWith("audio/"))
      {
        logger.LogWarning("{Method}: Invalid content type: {ContentType}",
          nameof(TranscribeAudio), audioFile.ContentType);
        return BadRequest($"Invalid or missing content type: '{audioFile.ContentType}'. Expected audio/*.");
      }

      // Generate timestamp-based filename (Consider using original filename part if desired)
      var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ");
      // Sanitize original filename if using it
      var safeOriginalName = Path.GetFileNameWithoutExtension(audioFile.FileName ?? "audio").Replace(" ", "_");
      var extension = Path.GetExtension(audioFile.FileName ?? ".tmp"); // Get original extension or default
      if (string.IsNullOrWhiteSpace(extension) && audioFile.ContentType.Contains('/'))
      {
        // Try to guess extension from ContentType if missing
        extension = "." + audioFile.ContentType.Split('/')[1]; // Basic guess
      }


      var audioFileName = $"{timestamp}_{safeOriginalName}{extension}";
      var transcriptFileName = $"{timestamp}_{safeOriginalName}.txt";

      // Read the audio file into memory
      using var ms = new MemoryStream();
      await audioFile.CopyToAsync(ms);
      var audioData = ms.ToArray(); // Get bytes for saving if needed immediately

      // --- GitHub Operations ---
      // Wrapped in try-catch blocks for targeted error handling & notifications
      try
      {
        await githubService.EnqueueCommitAsync(
          audioFileName,
          audioData, // Use byte array
          "recordings",
          $"Add audio recording: {audioFileName}"
        );
        logger.LogInformation("Successfully enqueued audio file commit: {FileName}", audioFileName);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to enqueue GitHub commit for audio file {FileName}", audioFileName);
        await SendErrorNotification("GitHub Audio Commit", ex, audioFileName);
        // Decide if you should stop here or continue transcription
        throw; // Re-throw to trigger the outer catch block and return 500
      }

      // --- Transcription ---
      string transcript;
      try
      {
        // Reset stream position for transcription
        ms.Position = 0;
        transcript = await transcribeService.TranscribeAudioAsync(ms);
        logger.LogInformation("Successfully transcribed audio file: {FileName}", audioFileName);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to transcribe audio file {FileName}", audioFileName);
        await SendErrorNotification("Audio Transcription", ex, audioFileName);
        throw; // Re-throw
      }

      // --- Transcript Commit ---
      try
      {
        await githubService.EnqueueCommitAsync(
          transcriptFileName,
          Encoding.UTF8.GetBytes(transcript),
          "transcripts",
          $"Add transcript: {transcriptFileName}"
        );
        logger.LogInformation("Successfully enqueued transcript file commit: {FileName}", transcriptFileName);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Failed to enqueue GitHub commit for transcript file {FileName}", transcriptFileName);
        await SendErrorNotification("GitHub Transcript Commit", ex, transcriptFileName);
        // Decide if failure here should still return 200 OK with the transcript
        // For now, we'll let it throw to the outer catch
        throw; // Re-throw
      }

      // --- Success Response ---
      return Ok(new
      {
        message = "Audio file processed and transcript stored successfully",
        audioFile = audioFileName,
        transcriptFile = transcriptFileName,
        timestamp,
        transcript
      });
    }
    catch (Exception ex) // Catch exceptions from transcription or GitHub commits
    {
      // Logged already in specific catches or here if it's a different exception
      logger.LogError(ex, "{Method}: Failed to process audio file", nameof(TranscribeAudio));
      // Ensure ApiErrorResult is defined and handles the exception appropriately
      return new ApiErrorResult(ex, "Failed to process audio file.");
    }
  }


  private async Task SendErrorNotification(string stage, Exception ex, string fileName)
  {
    try // Prevent email sending from crashing the request
    {
      var templateData = TemplateService.GetErrorTemplateData(ex); // Assuming this exists
      templateData["stage"] = stage;
      templateData["serviceName"] = "TranscriptionService"; // Service name context
      // Ensure additionalContext exists before adding to it
      if (!templateData.TryGetValue("additionalContext", out var value) ||
          value is not Dictionary<string, string>)
      {
        value = new Dictionary<string, string>();
        templateData["additionalContext"] = value;
      }

      ((Dictionary<string, string>)value)["fileName"] = fileName;

      await emailService.SendEmail(
        emailConfig.FromEmail, // Assuming emailConfig is injected and has FromEmail
        $"Transcription Service Error - {stage}",
        "error-log", // Template name
        templateData
      );
      logger.LogInformation("Error notification email sent for stage: {Stage}, file: {FileName}", stage, fileName);
    }
    catch (Exception emailEx)
    {
      logger.LogError(emailEx, "Failed to send error notification email for stage: {Stage}, file: {FileName}", stage,
        fileName);
    }
  }
}
