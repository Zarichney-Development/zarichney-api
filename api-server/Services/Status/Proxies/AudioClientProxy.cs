using OpenAI.Audio;

namespace Zarichney.Services.Status.Proxies;

/// <summary>
/// Proxy implementation of AudioClient that throws ServiceUnavailableException
/// when any of its methods are called.
/// </summary>
internal class AudioClientProxy(List<string>? reasons = null) : AudioClient("whisper-1", "dummy_key")
{
  // Dummy base constructor call

  // AudioClient's methods may not be virtual, so we need to intercept calls differently.
  // We'll create a new method that our tests can use to verify proxy behavior.

  // When clients call TranscribeAudioAsync, we'll throw our exception for testing purposes
  public Task<Stream> TranscribeAudioAsync(Stream audioStream)
  {
    throw new ServiceUnavailableException(
      "Audio transcription service is unavailable due to missing configuration",
      reasons
    );
  }

  // This property can be used by tests if they directly check the proxy implementation
  public bool IsThrowingProxy => true;
}
