using Zarichney.Services.Status;

using OpenAI.Audio;
using Polly;
using Polly.Retry;
using Zarichney.Config;
using ILogger = Serilog.ILogger;

namespace Zarichney.Services.AI;

public class TranscribeConfig : IConfig
{
  [RequiresConfiguration(Feature.Transcription, Feature.AiServices)]
  public string ModelName { get; init; } = "whisper-1";
  public int RetryAttempts { get; init; } = 5;
}

public interface ITranscribeService
{
  Task<string> TranscribeAudioAsync(Stream audioStream, AudioTranscriptionOptions? options = null);
}

public class TranscribeService : ITranscribeService
{
  private static readonly ILogger Log = Serilog.Log.ForContext<TranscribeService>();
  private readonly AudioClient _client;
  private readonly AsyncRetryPolicy _retryPolicy;

  public TranscribeService(AudioClient client, TranscribeConfig config)
  {
    _client = client;

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
