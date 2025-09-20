using Microsoft.Extensions.Logging;
using Moq;
using Zarichney.Services.AI;
using Zarichney.Services.Email;
using OpenAI.Audio;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace Zarichney.Server.Tests.Framework.Mocks.Factories;

/// <summary>
/// Factory for creating mock dependencies for TranscribeService testing.
/// </summary>
public static class AudioClientMockFactory
{
  private const string DefaultModel = "whisper-1";
  private const string FakeApiKey = "fake_api_key";

  public static Mock<IEmailService> CreateEmailServiceMock()
  {
    var mock = new Mock<IEmailService>();

    mock.Setup(x => x.SendErrorNotification(
            It.IsAny<string>(),
            It.IsAny<Exception>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.CompletedTask);

    return mock;
  }

  public static Mock<ILogger<TranscribeService>> CreateLoggerMock()
  {
    return new Mock<ILogger<TranscribeService>>();
  }

  public static Mock<AudioClient> CreateDefault()
  {
    return new Mock<AudioClient>(DefaultModel, FakeApiKey);
  }

  public static Mock<AudioClient> CreateWithSuccessfulTranscription(string expectedTranscript = "Transcribed text")
  {
    var mock = new Mock<AudioClient>(DefaultModel, FakeApiKey);

    // Build an AudioTranscription result via reflection (non-public constructors)
    var transcriptionType = typeof(AudioTranscription);
    var defaultCtor = transcriptionType.GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, binder: null, types: Type.EmptyTypes, modifiers: null);
    var transcription = (AudioTranscription)defaultCtor!.Invoke(null);
    // Set private backing fields via reflection
    transcriptionType.GetField("<Text>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
        .SetValue(transcription, expectedTranscript);
    transcriptionType.GetField("<Language>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
        .SetValue(transcription, "en");

    // Create ClientResult<AudioTranscription> using non-public constructor and null PipelineResponse
    // Create instance via non-public constructor using reflection APIs that accept binder/args array
    var clientResultCtor = typeof(ClientResult<AudioTranscription>)
        .GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .First();
    var pipelineResponse = new FakePipelineResponse();
    var clientResult = (ClientResult<AudioTranscription>)clientResultCtor.Invoke(new object?[] { transcription!, pipelineResponse });

    mock.Setup(x => x.TranscribeAudioAsync(
            It.IsAny<string>(),
            It.IsAny<AudioTranscriptionOptions>()))
        .ReturnsAsync(clientResult);

    return mock;
  }

  public static Mock<AudioClient> CreateWithFailure(Exception ex)
  {
    var mock = new Mock<AudioClient>(DefaultModel, FakeApiKey);
    mock.Setup(x => x.TranscribeAudioAsync(
            It.IsAny<string>(),
            It.IsAny<AudioTranscriptionOptions>()))
        .ThrowsAsync(ex);
    return mock;
  }
}

#pragma warning disable CS8765
internal sealed class FakePipelineResponse : PipelineResponse
{
  private Stream _stream = new MemoryStream();
  public override int Status { get; } = 200;
  public override string ReasonPhrase => "OK";
  protected override PipelineResponseHeaders HeadersCore { get; } = new FakeHeaders();
  public override Stream ContentStream { get => _stream; set => _stream = value; }
  protected override bool IsErrorCore { get; set; }
  public override BinaryData Content => BinaryData.FromString(string.Empty);
  public override BinaryData BufferContent(CancellationToken cancellationToken = default) => BinaryData.FromString(string.Empty);
  public override ValueTask<BinaryData> BufferContentAsync(CancellationToken cancellationToken = default) => new ValueTask<BinaryData>(BinaryData.FromString(string.Empty));
  public override void Dispose() { _stream.Dispose(); }
}
#pragma warning restore CS8765

internal sealed class FakeHeaders : PipelineResponseHeaders
{
  public override bool TryGetValue(string name, out string value)
  {
    value = string.Empty;
    return false;
  }

  public override bool TryGetValues(string name, out IEnumerable<string> values)
  {
    values = Array.Empty<string>();
    return false;
  }

  public override IEnumerator<KeyValuePair<string, string>> GetEnumerator()
  {
    return (new List<KeyValuePair<string, string>>()).GetEnumerator();
  }
}
