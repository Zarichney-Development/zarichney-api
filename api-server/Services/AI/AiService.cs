using Zarichney.Services.Status;
using Zarichney.Services.Email;
using Zarichney.Services.GitHub;
using Zarichney.Services.Sessions;
namespace Zarichney.Services.AI;

/// <summary>
/// Results from processing audio for transcription.
/// </summary>
public class AudioTranscriptionResult
{
  /// <summary>
  /// A message describing the result of the operation.
  /// </summary>
  public string Message { get; set; } = "Audio file processed and transcript stored successfully";

  /// <summary>
  /// The name of the audio file that was processed.
  /// </summary>
  public required string AudioFileName { get; init; }

  /// <summary>
  /// The name of the transcript file that was generated.
  /// </summary>
  public required string TranscriptFileName { get; init; }

  /// <summary>
  /// The timestamp when the transcription was performed.
  /// </summary>
  public required string Timestamp { get; init; }

  /// <summary>
  /// The transcribed text from the audio file.
  /// </summary>
  public required string Transcript { get; init; }
}

/// <summary>
/// Results from processing a completion request.
/// </summary>
public class CompletionResult
{
  /// <summary>
  /// The LLM's response text.
  /// </summary>
  public required string Response { get; init; }

  /// <summary>
  /// The source type of the prompt (audio or text).
  /// </summary>
  public required string SourceType { get; init; }

  /// <summary>
  /// The transcribed prompt, if the source was audio.
  /// </summary>
  public string? TranscribedPrompt { get; init; }

  /// <summary>
  /// The session ID associated with this completion.
  /// </summary>
  public Guid SessionId { get; init; }
}

/// <summary>
/// Represents an audio or text prompt for LLM completion.
/// </summary>
public class CompletionPrompt
{
  /// <summary>
  /// The text prompt to send to the LLM.
  /// </summary>
  public string? TextPrompt { get; init; }

  /// <summary>
  /// The audio file to be transcribed and then sent to the LLM.
  /// </summary>
  public IFormFile? AudioFile { get; init; }
}

/// <summary>
/// Provides high-level AI functionality including audio transcription and LLM completions.
/// </summary>
public interface IAiService
{
  /// <summary>
  /// Processes an audio file for transcription, including storage of the audio and transcript files.
  /// </summary>
  /// <param name="audioFile">The audio file to transcribe.</param>
  /// <returns>The transcription result with file metadata.</returns>
  /// <exception cref="ArgumentNullException">Thrown when the audio file is null.</exception>
  /// <exception cref="ArgumentException">Thrown when the audio file is invalid.</exception>
  Task<AudioTranscriptionResult> ProcessAudioTranscriptionAsync(IFormFile audioFile);

  /// <summary>
  /// Gets a completion from the LLM based on either text or audio input.
  /// </summary>
  /// <param name="prompt">The completion prompt, which can be either text or audio.</param>
  /// <returns>The completion result containing the LLM's response.</returns>
  /// <exception cref="ArgumentException">Thrown when no valid prompt is provided or the audio is invalid.</exception>
  Task<CompletionResult> GetCompletionAsync(CompletionPrompt prompt);
}

/// <summary>
/// Implementation of the IAiService interface that coordinates the
/// transcription, storage, and LLM completion workflows.
/// </summary>
public class AiService(
  ILlmService llmService,
  ITranscribeService transcribeService,
  IGitHubService githubService,
  IEmailService emailService,
  ISessionManager sessionManager,
  IScopeContainer scope,
  ILogger<AiService> logger)
  : IAiService
{
  private readonly ILogger<AiService> _logger = logger;

  /// <inheritdoc />
  public async Task<AudioTranscriptionResult> ProcessAudioTranscriptionAsync(IFormFile audioFile)
  {
    ArgumentNullException.ThrowIfNull(audioFile, nameof(audioFile));

    // Validate the audio file
    var (isValid, errorMessage) = transcribeService.ValidateAudioFile(audioFile);
    if (!isValid)
    {
      throw new ArgumentException(errorMessage, nameof(audioFile));
    }

    _logger.LogInformation(
      "Processing audio file for transcription. ContentType: {ContentType}, FileName: {FileName}, Length: {Length}",
      audioFile.ContentType,
      audioFile.FileName,
      audioFile.Length);

    // Process the audio file (validation, transcription)
    var result = await transcribeService.ProcessAudioFileAsync(audioFile);

    // Store audio and transcript in GitHub
    await StoreAudioAndTranscript(result, audioFile);

    // Return the result
    return new AudioTranscriptionResult
    {
      AudioFileName = result.AudioFileName,
      TranscriptFileName = result.TranscriptFileName,
      Timestamp = result.Timestamp,
      Transcript = result.Transcript
    };
  }

  /// <inheritdoc />
  public async Task<CompletionResult> GetCompletionAsync(CompletionPrompt prompt)
  {
    string promptText;
    var sourceType = "text";
    string? transcribedPrompt = null;

    // Handle audio input
    if (prompt.AudioFile != null)
    {
      _logger.LogInformation(
        "Processing audio prompt. ContentType: {ContentType}, FileName: {FileName}, Length: {Length}",
        prompt.AudioFile.ContentType,
        prompt.AudioFile.FileName,
        prompt.AudioFile.Length);

      // Validate audio file
      var (isValid, errorMessage) = transcribeService.ValidateAudioFile(prompt.AudioFile);
      if (!isValid)
      {
        throw new ArgumentException(errorMessage, nameof(prompt));
      }

      // Transcribe audio to text
      using var ms = new MemoryStream();
      await prompt.AudioFile.CopyToAsync(ms);
      ms.Position = 0;

      try
      {
        promptText = await transcribeService.TranscribeAudioAsync(ms);
        transcribedPrompt = promptText;
        sourceType = "audio";
        _logger.LogInformation("Successfully transcribed audio prompt");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to transcribe audio prompt");

        // Send error notification
        await SendErrorNotification(
          "LLM Audio Transcription",
          ex,
          new Dictionary<string, string> { { "fileName", prompt.AudioFile.FileName } }
        );

        throw new InvalidOperationException("Failed to transcribe the provided audio prompt.", ex);
      }
    }
    // Handle text input
    else if (!string.IsNullOrWhiteSpace(prompt.TextPrompt))
    {
      promptText = prompt.TextPrompt;
      _logger.LogInformation("Processing text prompt");
    }
    else
    {
      _logger.LogWarning("No valid prompt provided");
      throw new ArgumentException("Either text prompt or audio file must be provided.", nameof(prompt));
    }

    // Get completion from LLM
    try
    {
      var response = await llmService.GetCompletionContent(promptText);

      // Get session information
      var session = await sessionManager.GetSessionByScope(scope.Id);

      return new CompletionResult
      {
        Response = response,
        SourceType = sourceType,
        TranscribedPrompt = transcribedPrompt,
        SessionId = session.Id
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to get LLM completion");

      // Send error notification for LLM completion errors
      await SendErrorNotification(
        "LLM Completion",
        ex,
        new Dictionary<string, string>
        {
          { "sourceType", sourceType }, { "fileName", prompt.AudioFile?.FileName ?? "N/A" }
        }
      );

      throw; // Re-throw to allow caller to handle the error
    }
  }

  private async Task SendErrorNotification(string stage, Exception ex, Dictionary<string, string> additionalContext)
  {
    try
    {
      await emailService.SendErrorNotification(stage, ex, "AiService", additionalContext);
    }
    catch (ServiceUnavailableException e)
    {
      _logger.LogError(e, "Unable to send email as email service is unavailable");
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Unable to send email as email service failed");
      throw;
    }
  }

  private async Task StoreAudioAndTranscript(TranscriptionResult transcription, IFormFile audioFile)
  {
    try
    {
      // Read the audio file into memory
      using var ms = new MemoryStream();
      await audioFile.CopyToAsync(ms);
      var audioData = ms.ToArray();

      // Store files
      await githubService.StoreAudioAndTranscriptAsync(
        transcription.AudioFileName,
        audioData,
        transcription.TranscriptFileName,
        transcription.Transcript
      );
    }
    catch (ServiceUnavailableException e)
    {
      _logger.LogError(e, "Unable to store to github as service is unavailable");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to store audio and transcript files to GitHub");
      await SendErrorNotification(
        "GitHub Storage",
        ex,
        new Dictionary<string, string>
        {
          { "audioFileName", transcription.AudioFileName }, { "transcriptFileName", transcription.TranscriptFileName }
        }
      );
      throw; // Re-throw to allow caller to handle the error
    }
  }
}
