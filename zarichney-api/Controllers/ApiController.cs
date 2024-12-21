using System.Text;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Zarichney.Middleware;
using Zarichney.Services;
using ILogger = Serilog.ILogger;

namespace Zarichney.Controllers;

[ApiController]
[Route("api")]
public class ApiController(
  IEmailService emailService,
  ITranscribeService transcribeService,
  IGitHubService githubService,
  EmailConfig emailConfig,
  ILlmService llmService
) : ControllerBase
{
  private readonly ILogger _log = Log.ForContext<ApiController>();

  public class CompletionRequest
  {
    public string? TextPrompt { get; set; }
    public IFormFile? AudioPrompt { get; set; }
  }

  [HttpPost("completion")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
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
        _log.Information(
          "Received audio prompt. ContentType: {ContentType}, FileName: {FileName}, Length: {Length}",
          request.AudioPrompt.ContentType,
          request.AudioPrompt.FileName,
          request.AudioPrompt.Length);

        if (request.AudioPrompt.Length == 0)
        {
          _log.Warning("{Method}: Empty audio file received", nameof(GetCompletion));
          return BadRequest("Audio file must not be empty");
        }

        if (!request.AudioPrompt.ContentType.StartsWith("audio/"))
        {
          _log.Warning("{Method}: Invalid content type: {ContentType}",
            nameof(GetCompletion), request.AudioPrompt.ContentType);
          return BadRequest($"Invalid content type: {request.AudioPrompt.ContentType}. Expected audio/*");
        }

        // Transcribe audio to text
        using var ms = new MemoryStream();
        await request.AudioPrompt.CopyToAsync(ms);
        ms.Position = 0;

        try
        {
          prompt = await transcribeService.TranscribeAudioAsync(ms);
          _log.Information("{Method}: Successfully transcribed audio prompt", nameof(GetCompletion));
        }
        catch (Exception ex)
        {
          _log.Error(ex, "{Method}: Failed to transcribe audio prompt", nameof(GetCompletion));
          return BadRequest("Failed to transcribe audio prompt");
        }
      }
      // Handle text input
      else if (!string.IsNullOrWhiteSpace(request.TextPrompt))
      {
        prompt = request.TextPrompt;
      }
      else
      {
        _log.Warning("{Method}: No valid prompt provided", nameof(GetCompletion));
        return BadRequest("Either 'textPrompt' or 'audioPrompt' must be provided");
      }

      var response = await llmService.GetCompletionContent(prompt);

      return Ok(new
      {
        response,
        sourceType = request.AudioPrompt != null ? "audio" : "text",
        transcribedPrompt = request.AudioPrompt != null ? prompt : null
      });
    }
    catch (Exception ex)
    {
      _log.Error(ex, "{Method}: Failed to get LLM completion", nameof(GetCompletion));
      return new ApiErrorResult(ex, $"{nameof(GetCompletion)}: Failed to get LLM completion");
    }
  }

  [HttpPost("transcribe")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> TranscribeAudio([FromForm] IFormFile? audioFile)
  {
    try
    {
      _log.Information(
        "Received transcribe request. ContentType: {ContentType}, FileName: {FileName}, Length: {Length}, FormFile null: {IsNull}",
        audioFile?.ContentType,
        audioFile?.FileName,
        audioFile?.Length,
        audioFile == null);

      if (Request.Form.Files.Count == 0)
      {
        _log.Warning("No files found in form data. Form count: {Count}", Request.Form.Count);
        return BadRequest("No files found in request");
      }

      if (audioFile == null || audioFile.Length == 0)
      {
        _log.Warning("{Method}: No audio file received or empty file", nameof(TranscribeAudio));
        return BadRequest("Audio file is required and must not be empty");
      }

      // Log the content type we received
      if (!audioFile.ContentType.StartsWith("audio/"))
      {
        _log.Warning("{Method}: Invalid content type: {ContentType}",
          nameof(TranscribeAudio), audioFile.ContentType);
        return BadRequest($"Invalid content type: {audioFile.ContentType}. Expected audio/*");
      }

      // Generate timestamp-based filename
      var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ");
      var audioFileName = $"{timestamp}.webm";
      var transcriptFileName = $"{timestamp}.txt";

      // Read the audio file into memory
      using var ms = new MemoryStream();
      await audioFile.CopyToAsync(ms);
      var audioData = ms.ToArray();

      try
      {
        // First, commit the audio file
        await githubService.CommitFileAsync(
          audioFileName,
          audioData,
          "recordings",
          $"Add audio recording: {audioFileName}"
        );
      }
      catch (Exception ex)
      {
        await SendErrorNotification("GitHub Audio Commit", ex, audioFileName);
        throw;
      }

      string transcript;
      try
      {
        // Reset stream position for transcription
        ms.Position = 0;
        transcript = await transcribeService.TranscribeAudioAsync(ms);
      }
      catch (Exception ex)
      {
        await SendErrorNotification("Audio Transcription", ex, audioFileName);
        throw;
      }

      try
      {
        // Commit the transcript
        await githubService.CommitFileAsync(
          transcriptFileName,
          Encoding.UTF8.GetBytes(transcript),
          "transcripts",
          $"Add transcript: {transcriptFileName}"
        );
      }
      catch (Exception ex)
      {
        await SendErrorNotification("GitHub Transcript Commit", ex, transcriptFileName);
        throw;
      }

      return Ok(new
      {
        message = "Audio file processed and transcript stored successfully",
        audioFile = audioFileName,
        transcriptFile = transcriptFileName,
        timestamp,
        transcript
      });
    }
    catch (Exception ex)
    {
      _log.Error(ex, "{Method}: Failed to process audio file", nameof(TranscribeAudio));
      return new ApiErrorResult(ex, "Failed to process audio file");
    }
  }

  [HttpPost("email/validate")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ValidateEmail([FromQuery] string email)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(email))
      {
        _log.Warning("{Method}: Empty email received", nameof(ValidateEmail));
        return BadRequest("Email parameter is required");
      }

      await emailService.ValidateEmail(email);
      return Ok("Valid");
    }
    catch (InvalidEmailException ex)
    {
      _log.Warning(ex, "{Method}: Invalid email validation for {Email}",
        nameof(ValidateEmail), email);
      return BadRequest(new
      {
        error = ex.Message,
        email = ex.Email,
        reason = ex.Reason.ToString()
      });
    }
    catch (Exception ex)
    {
      _log.Error(ex, "{Method}: Failed to validate email: {Email}",
        nameof(ValidateEmail), email);
      return new ApiErrorResult(ex, $"{nameof(ValidateEmail)}: Failed to validate email");
    }
  }

  [HttpGet("health/secure")]
  public IActionResult HealthCheck()
  {
    return Ok(new
    {
      Success = true,
      Time = DateTime.Now.ToLocalTime()
    });
  }

  private async Task SendErrorNotification(string stage, Exception ex, string fileName)
  {
    await emailService.SendEmail(
      emailConfig.FromEmail,
      $"Transcription Service Error - {stage}",
      "error-log",
      new Dictionary<string, object>
      {
        { "timestamp", DateTime.UtcNow.ToString("O") },
        { "fileName", fileName },
        { "errorType", ex.GetType().Name },
        { "errorMessage", ex.Message },
        { "stage", stage },
        { "stackTrace", ex.StackTrace ?? "No stack trace available" },
        {
          "additionalContext", new Dictionary<string, string>
          {
            { "ProcessStage", stage },
            { "MachineName", Environment.MachineName },
            { "OsVersion", Environment.OSVersion.ToString() }
          }
        }
      }
    );
  }
}