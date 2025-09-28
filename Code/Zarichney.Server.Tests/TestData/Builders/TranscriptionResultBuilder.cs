using Zarichney.Services.AI;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Test data builder for TranscriptionResult objects.
/// </summary>
public class TranscriptionResultBuilder
{
  private string _transcript = "This is a sample transcription.";
  private string _audioFileName = "2025-09-10T06-12-00Z_audio.webm";
  private string _transcriptFileName = "2025-09-10T06-12-00Z_audio.txt";
  private string _timestamp = "2025-09-10T06-12-00Z";

  public TranscriptionResultBuilder WithTranscript(string transcript)
  {
    _transcript = transcript;
    return this;
  }

  public TranscriptionResultBuilder WithAudioFileName(string audioFileName)
  {
    _audioFileName = audioFileName;
    return this;
  }

  public TranscriptionResultBuilder WithTranscriptFileName(string transcriptFileName)
  {
    _transcriptFileName = transcriptFileName;
    return this;
  }

  public TranscriptionResultBuilder WithTimestamp(string timestamp)
  {
    _timestamp = timestamp;
    return this;
  }

  public TranscriptionResult Build()
  {
    return new TranscriptionResult
    {
      Transcript = _transcript,
      AudioFileName = _audioFileName,
      TranscriptFileName = _transcriptFileName,
      Timestamp = _timestamp
    };
  }
}
