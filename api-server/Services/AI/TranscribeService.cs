using Zarichney.Services.Status;
using OpenAI.Audio;
using Polly;
using Polly.Retry;
using Zarichney.Config;
using Zarichney.Services.Email;
using ILogger = Serilog.ILogger;

namespace Zarichney.Services.AI;

public class TranscribeConfig : IConfig
{
  public string ModelName { get; init; } = "whisper-1";
  public int RetryAttempts { get; init; } = 5;
}

/// <summary>
/// Represents the result of audio transcription processing.
/// </summary>
public class TranscriptionResult
{
  /// <summary>
  /// The transcribed text from the audio file.
  /// </summary>
  public required string Transcript { get; init; }

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
}

public interface ITranscribeService
{
  /// <summary>
  /// Transcribes audio content from the provided stream.
  /// </summary>
  /// <param name="audioStream">The audio stream to transcribe.</param>
  /// <param name="options">Optional transcription options.</param>
  /// <returns>The transcribed text.</returns>
  Task<string> TranscribeAudioAsync(Stream audioStream, AudioTranscriptionOptions? options = null);

  /// <summary>
  /// Validates an audio file from an HTTP request and returns validation information.
  /// </summary>
  /// <param name="audioFile">The audio file from the HTTP request.</param>
  /// <returns>A tuple with validation result (success/failure) and error message if any.</returns>
  (bool isValid, string? errorMessage) ValidateAudioFile(IFormFile? audioFile);

  /// <summary>
  /// Processes an audio file, including validation, creating filenames, transcription, and error handling.
  /// </summary>
  /// <param name="audioFile">The audio file to process.</param>
  /// <returns>The transcription result containing the transcript text and metadata.</returns>
  /// <exception cref="ArgumentException">Thrown when the audioFile is invalid.</exception>
  /// <exception cref="ArgumentNullException">Thrown when the audioFile is null.</exception>
  Task<TranscriptionResult> ProcessAudioFileAsync(IFormFile audioFile);
}

public class TranscribeService : ITranscribeService
{
  private static readonly ILogger Log = Serilog.Log.ForContext<TranscribeService>();
  private readonly AudioClient _client;
  private readonly IEmailService _emailService;
  private readonly AsyncRetryPolicy _retryPolicy;

  public TranscribeService(AudioClient client, TranscribeConfig config, IEmailService emailService)
  {
    _client = client;
    _emailService = emailService;

    _retryPolicy = Policy
      .Handle<Exception>()
      .WaitAndRetryAsync(
        retryCount: config.RetryAttempts,
        sleepDurationProvider: _ => TimeSpan.FromSeconds(1),
        onRetry: (exception, _, retryCount, context) =>
        {
          Log.Warning(exception,
            "Transcription attempt {retryCount}: Retrying due to {exception}. Retry Context: {@Context}",
            retryCount, exception.Message, context);
        }
      );
  }

  /// <summary>
  /// Transcribes audio content from the provided stream using the OpenAI Whisper model.
  /// </summary>
  /// <param name="audioStream">The audio stream to transcribe.</param>
  /// <param name="options">Optional transcription options.</param>
  /// <returns>The transcribed text.</returns>
  /// <exception cref="ServiceUnavailableException">Thrown when the service is unavailable due to missing configuration.</exception>
  public async Task<string> TranscribeAudioAsync(Stream audioStream, AudioTranscriptionOptions? options = null)
  {
    // ServiceUnavailableException will be thrown by the proxy if LLM service is unavailable
    ArgumentNullException.ThrowIfNull(_client, nameof(_client));

    try
    {
      Log.Information("Starting audio transcription");

      return await _retryPolicy.ExecuteAsync(async () =>
      {
        try
        {
          var tempFile = await SaveStreamToTempFile(audioStream);

          try
          {
            options ??= new AudioTranscriptionOptions
            {
              ResponseFormat = AudioTranscriptionFormat.Text
            };

            var transcriptionResult = await _client.TranscribeAudioAsync(tempFile, options);

            var transcription = transcriptionResult.Value;

            Log.Information(
              "Verbose audio transcription completed successfully. Segments: {SegmentCount}, Words: {WordCount}",
              transcription.Segments?.Count ?? 0,
              transcription.Words?.Count ?? 0);

            return transcription.Text;
          }
          finally
          {
            // Clean up temp file
            if (File.Exists(tempFile))
            {
              File.Delete(tempFile);
            }
          }
        }
        catch (Exception e)
        {
          Log.Error(e, "Error occurred during verbose audio transcription");
          throw;
        }
      });
    }
    catch (Exception e)
    {
      Log.Error(e, "Failed to transcribe audio verbosely after all retry attempts");
      throw;
    }
  }

  /// <summary>
  /// Validates an audio file from an HTTP request and returns validation information.
  /// </summary>
  /// <param name="audioFile">The audio file from the HTTP request.</param>
  /// <returns>A tuple with validation result (success/failure) and error message if any.</returns>
  public (bool isValid, string? errorMessage) ValidateAudioFile(IFormFile? audioFile)
  {
    // Check if file exists
    if (audioFile == null)
    {
      return (false, "Audio file is required.");
    }

    // Check if file is empty
    if (audioFile.Length == 0)
    {
      Log.Warning("Empty audio file received ('{FileName}')", audioFile.FileName);
      return (false, "Audio file must not be empty.");
    }

    // Check content type
    if (string.IsNullOrEmpty(audioFile.ContentType) || !audioFile.ContentType.StartsWith("audio/"))
    {
      Log.Warning("Invalid content type: {ContentType}", audioFile.ContentType);
      return (false, $"Invalid or missing content type: '{audioFile.ContentType}'. Expected audio/*.");
    }

    // All checks passed
    return (true, null);
  }

  /// <summary>
  /// Processes an audio file, including validation, creating filenames, transcription, and error handling.
  /// </summary>
  /// <param name="audioFile">The audio file to process.</param>
  /// <returns>The transcription result containing the transcript text and metadata.</returns>
  /// <exception cref="ArgumentException">Thrown when the audioFile is invalid.</exception>
  /// <exception cref="ArgumentNullException">Thrown when the audioFile is null.</exception>
  public async Task<TranscriptionResult> ProcessAudioFileAsync(IFormFile audioFile)
  {
    ArgumentNullException.ThrowIfNull(audioFile, nameof(audioFile));

    // Validate the file
    var (isValid, errorMessage) = ValidateAudioFile(audioFile);
    if (!isValid)
    {
      throw new ArgumentException(errorMessage, nameof(audioFile));
    }

    Log.Information(
      "Processing audio file. ContentType: {ContentType}, FileName: {FileName}, Length: {Length}",
      audioFile.ContentType,
      audioFile.FileName,
      audioFile.Length);

    // Generate timestamp-based filename
    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ");

    // Sanitize original filename
    var safeOriginalName = Path.GetFileNameWithoutExtension(audioFile.FileName ?? "audio").Replace(" ", "_");
    var extension = Path.GetExtension(audioFile.FileName ?? ".tmp"); // Get original extension or default

    // Try to guess extension from ContentType if missing
    if (string.IsNullOrWhiteSpace(extension) && audioFile.ContentType.Contains('/'))
    {
      extension = "." + audioFile.ContentType.Split('/')[1]; // Basic guess
    }

    var audioFileName = $"{timestamp}_{safeOriginalName}{extension}";
    var transcriptFileName = $"{timestamp}_{safeOriginalName}.txt";

    // Read the audio file into memory
    using var ms = new MemoryStream();
    await audioFile.CopyToAsync(ms);
    ms.Position = 0; // Reset stream position for transcription

    // Transcribe the audio
    string transcript;
    try
    {
      transcript = await TranscribeAudioAsync(ms);
      Log.Information("Successfully transcribed audio file: {FileName}", audioFileName);
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Failed to transcribe audio file {FileName}", audioFileName);
      await _emailService.SendErrorNotification(
        "Audio Transcription",
        ex,
        "TranscriptionService",
        new Dictionary<string, string> { { "fileName", audioFileName } });
      throw; // Re-throw the exception
    }

    // Return the result
    return new TranscriptionResult
    {
      Transcript = transcript,
      AudioFileName = audioFileName,
      TranscriptFileName = transcriptFileName,
      Timestamp = timestamp
    };
  }

  private static async Task<string> SaveStreamToTempFile(Stream stream)
  {
    var tempFile = Path.Combine(Path.GetTempPath(), $"audio_{Utils.GenerateId()}.webm");

    try
    {
      await using var fileStream = File.Create(tempFile);
      stream.Position = 0; // Reset stream position
      await stream.CopyToAsync(fileStream);
      return tempFile;
    }
    catch (Exception e)
    {
      // Clean up on error
      if (File.Exists(tempFile))
      {
        File.Delete(tempFile);
      }

      Log.Error(e, "Failed to save audio stream to temporary file");
      throw;
    }
  }
}
