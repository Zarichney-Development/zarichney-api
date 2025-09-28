using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI;
using Zarichney.Config;
using Zarichney.Services.AI;
using Zarichney.Services.Sessions;
using Zarichney.Services.Status;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Builder for creating LlmService instances for testing.
/// Provides fluent API for configuring various test scenarios.
/// </summary>
public class LlmServiceBuilder
{
  private OpenAIClient? _client;
  private IMapper? _mapper;
  private LlmConfig _config;
  private Mock<ISessionManager>? _sessionManager;
  private IScopeContainer? _scope;
  private Mock<ILogger<LlmService>>? _logger;
  private bool _withNullClient;

  public LlmServiceBuilder()
  {
    // Default valid configuration
    _config = new LlmConfig
    {
      ApiKey = "test-api-key",
      ModelName = LlmModels.Gpt4Omini,
      RetryAttempts = 3
    };
  }

  public LlmServiceBuilder WithClient(OpenAIClient? client)
  {
    _client = client;
    return this;
  }

  public LlmServiceBuilder WithNullClient()
  {
    _withNullClient = true;
    _client = null;
    return this;
  }

  public LlmServiceBuilder WithMapper(IMapper mapper)
  {
    _mapper = mapper;
    return this;
  }

  public LlmServiceBuilder WithConfig(LlmConfig config)
  {
    _config = config;
    return this;
  }

  public LlmServiceBuilder WithSessionManager(Mock<ISessionManager> sessionManager)
  {
    _sessionManager = sessionManager;
    return this;
  }

  public LlmServiceBuilder WithScope(IScopeContainer scope)
  {
    _scope = scope;
    return this;
  }

  public LlmServiceBuilder WithLogger(Mock<ILogger<LlmService>> logger)
  {
    _logger = logger;
    return this;
  }

  public LlmServiceBuilder WithInvalidConfiguration()
  {
    _config = new LlmConfig { ApiKey = string.Empty }; // Invalid config
    _withNullClient = true;
    return this;
  }

  public LlmServiceBuilder WithDefaults()
  {
    if (!_withNullClient && _client == null)
    {
      _client = new Mock<OpenAIClient>().Object;
    }
    _mapper ??= new Mock<IMapper>().Object;
    _sessionManager ??= new Mock<ISessionManager>();
    _scope ??= new Mock<IScopeContainer>().Object;
    _logger ??= new Mock<ILogger<LlmService>>();
    return this;
  }

  public LlmService Build()
  {
    // Apply defaults for missing components unless explicitly null
    if (!_withNullClient && _client == null)
    {
      _client = new Mock<OpenAIClient>().Object;
    }
    _mapper ??= new Mock<IMapper>().Object;
    _sessionManager ??= new Mock<ISessionManager>();
    _scope ??= new Mock<IScopeContainer>().Object;
    _logger ??= new Mock<ILogger<LlmService>>();

    return new LlmService(
        _client!,
        _mapper,
        _config,
        _sessionManager.Object,
        _scope,
        _logger.Object
    );
  }
}
