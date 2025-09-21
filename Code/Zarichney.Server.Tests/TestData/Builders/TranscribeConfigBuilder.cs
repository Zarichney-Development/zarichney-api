using Zarichney.Services.AI;

namespace Zarichney.Server.Tests.TestData.Builders;

/// <summary>
/// Test data builder for TranscribeConfig objects.
/// </summary>
public class TranscribeConfigBuilder
{
  private string _modelName = "whisper-1";
  private int _retryAttempts = 5;

  public TranscribeConfigBuilder WithDefaults()
  {
    _modelName = "whisper-1";
    _retryAttempts = 5;
    return this;
  }

  public TranscribeConfigBuilder WithModelName(string modelName)
  {
    _modelName = modelName;
    return this;
  }

  public TranscribeConfigBuilder WithRetryAttempts(int retryAttempts)
  {
    _retryAttempts = retryAttempts;
    return this;
  }

  public TranscribeConfig Build()
  {
    return new TranscribeConfig
    {
      ModelName = _modelName,
      RetryAttempts = _retryAttempts
    };
  }
}
