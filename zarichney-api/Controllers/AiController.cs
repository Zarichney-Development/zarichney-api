using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zarichney.Middleware;
using Zarichney.Services;
using Zarichney.Services.Emails;
using Zarichney.Services.Sessions;

namespace Zarichney.Controllers;

[ApiController]
[Route("api")]
[Authorize]
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
  public class CompletionRequest
  {
    public string? TextPrompt { get; set; }
    public IFormFile? AudioPrompt { get; set; }
  }

  [HttpPost("completion")]
  [AcceptsSession]
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
        logger.LogInformation(
          "Received audio prompt. ContentType: {ContentType}, FileName: {FileName}, Length: {Length}",
          request.AudioPrompt.ContentType,
          request.AudioPrompt.FileName,
          request.AudioPrompt.Length);

        if (request.AudioPrompt.Length == 0)
        {
          logger.LogWarning("{Method}: Empty audio file received", nameof(GetCompletion));
          return BadRequest("Audio file must not be empty");
        }

        if (!request.AudioPrompt.ContentType.StartsWith("audio/"))
        {
          logger.LogWarning("{Method}: Invalid content type: {ContentType}",
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
          logger.LogInformation("{Method}: Successfully transcribed audio prompt", nameof(GetCompletion));
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "{Method}: Failed to transcribe audio prompt", nameof(GetCompletion));
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
        logger.LogWarning("{Method}: No valid prompt provided", nameof(GetCompletion));
        return BadRequest("Either 'textPrompt' or 'audioPrompt' must be provided");
      }

      var response = await llmService.GetCompletionContent(prompt);

      var session = await sessionManager.GetSessionByScope(scope.Id);
      Response.Headers["X-Session-Id"] = session.Id.ToString();

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
      return new ApiErrorResult(ex, $"{nameof(GetCompletion)}: Failed to get LLM completion");
    }
  }

  [HttpPost("transcribe")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> TranscribeAudio([FromForm] IFormFile? audioFile)
  {
    // return Ok(new
    // {
    //   message = "Audio file processed and transcript stored successfully",
    //   // audioFile = audioFileName,
    //   // transcriptFile = transcriptFileName,
    //   transcript = "test"
    // });
    try
    {
      logger.LogInformation(
        "Received transcribe request. ContentType: {ContentType}, FileName: {FileName}, Length: {Length}, FormFile null: {IsNull}",
        audioFile?.ContentType,
        audioFile?.FileName,
        audioFile?.Length,
        audioFile == null);

      if (Request.Form.Files.Count == 0)
      {
        logger.LogWarning("No files found in form data. Form count: {Count}", Request.Form.Count);
        return BadRequest("No files found in request");
      }

      if (audioFile == null || audioFile.Length == 0)
      {
        logger.LogWarning("{Method}: No audio file received or empty file", nameof(TranscribeAudio));
        return BadRequest("Audio file is required and must not be empty");
      }

      // Log the content type we received
      if (!audioFile.ContentType.StartsWith("audio/"))
      {
        logger.LogWarning("{Method}: Invalid content type: {ContentType}",
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
        await githubService.EnqueueCommitAsync(
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
        await githubService.EnqueueCommitAsync(
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
      logger.LogError(ex, "{Method}: Failed to process audio file", nameof(TranscribeAudio));
      return new ApiErrorResult(ex, "Failed to process audio file");
    }
  }


  private async Task SendErrorNotification(string stage, Exception ex, string fileName)
  {
    var templateData = TemplateService.GetErrorTemplateData(ex);
    templateData["stage"] = stage;
    templateData["serviceName"] = "Transcription";
    ((Dictionary<string, string>)templateData["additionalContext"])["fileName"] = fileName;

    await emailService.SendEmail(
      emailConfig.FromEmail,
      $"Transcription Service Error - {stage}",
      "error-log",
      templateData
    );
  }
}