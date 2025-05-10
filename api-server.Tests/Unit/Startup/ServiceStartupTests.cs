using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moq;
using OpenAI;
using OpenAI.Audio;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.AI;
using Zarichney.Services.Email;
using Zarichney.Services.Status;
using Zarichney.Startup;

namespace Zarichney.Tests.Unit.Startup;

public class ServiceStartupTests
{
  [Trait("Category", "Unit")]
  [Fact]
  public void ConfigureEmailServices_WhenEmailConfigIsAvailable_RegistersRealGraphServiceClient()
  {
    // Arrange
    var services = new ServiceCollection();

    // Mock dependencies
    var mockLogger = new Mock<ILogger<ServiceStartup>>();
    services.AddSingleton(mockLogger.Object);

    // Mock configuration status service that reports Email service as available
    var mockStatusService = new Mock<IConfigurationStatusService>();
    var statusDict = new Dictionary<string, ServiceStatusInfo>
        {
            { "Email", new ServiceStatusInfo(true, new List<string>()) }
        };
    mockStatusService.Setup(s => s.GetServiceStatusAsync())
        .ReturnsAsync(statusDict);
    services.AddSingleton(mockStatusService.Object);

    // Add a valid EmailConfig 
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "tenant-id",
      AzureAppId = "app-id",
      AzureAppSecret = "app-secret",
      FromEmail = "from@example.com",
      MailCheckApiKey = "mail-check-key"
    };
    services.AddSingleton(emailConfig);

    // Act - Call the method under test via reflection since it's private
    var methodInfo = typeof(ServiceStartup).GetMethod("ConfigureEmailServices",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    methodInfo?.Invoke(null, new object[] { services });

    // Assert
    var serviceProvider = services.BuildServiceProvider();
    var graphClient = serviceProvider.GetService<GraphServiceClient>();
    graphClient.Should().NotBeNull();
    graphClient.Should().NotBeOfType<GraphServiceClientProxy>();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ConfigureEmailServices_WhenEmailConfigIsUnavailable_RegistersProxyGraphServiceClient()
  {
    // Arrange
    var services = new ServiceCollection();

    // Mock dependencies
    var mockLogger = new Mock<ILogger<ServiceStartup>>();
    services.AddSingleton(mockLogger.Object);

    // Mock configuration status service that reports Email service as unavailable
    var mockStatusService = new Mock<IConfigurationStatusService>();
    var statusDict = new Dictionary<string, ServiceStatusInfo>
        {
            { "Email", new ServiceStatusInfo(false, new List<string> { "EmailConfig:AzureTenantId" }) }
        };
    mockStatusService.Setup(s => s.GetServiceStatusAsync())
        .ReturnsAsync(statusDict);
    services.AddSingleton(mockStatusService.Object);

    // Add an invalid EmailConfig
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "", // Invalid/empty value
      AzureAppId = "app-id",
      AzureAppSecret = "app-secret",
      FromEmail = "from@example.com",
      MailCheckApiKey = "mail-check-key"
    };
    services.AddSingleton(emailConfig);

    // Act - Call the method under test via reflection since it's private
    var methodInfo = typeof(ServiceStartup).GetMethod("ConfigureEmailServices",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    methodInfo?.Invoke(null, new object[] { services });

    // Assert
    var serviceProvider = services.BuildServiceProvider();
    var graphClient = serviceProvider.GetService<GraphServiceClient>();
    graphClient.Should().NotBeNull();

    // Verify it's a proxy by checking if it throws ServiceUnavailableException
    var exception = Assert.Throws<ServiceUnavailableException>(() =>
        graphClient.Me);

    exception.Message.Should().Contain("Email service is unavailable");
    exception.Reasons.Should().Contain("EmailConfig:AzureTenantId");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ConfigureOpenAiServices_WhenLlmConfigIsAvailable_RegistersRealOpenAIClient()
  {
    // Arrange
    var services = new ServiceCollection();

    // Mock dependencies
    var mockLogger = new Mock<ILogger<ServiceStartup>>();
    services.AddSingleton(mockLogger.Object);

    // Mock configuration status service that reports LLM service as available
    var mockStatusService = new Mock<IConfigurationStatusService>();
    var statusDict = new Dictionary<string, ServiceStatusInfo>
        {
            { "Llm", new ServiceStatusInfo(true, new List<string>()) }
        };
    mockStatusService.Setup(s => s.GetServiceStatusAsync())
        .ReturnsAsync(statusDict);
    services.AddSingleton(mockStatusService.Object);

    // Add valid LlmConfig and TranscribeConfig
    var llmConfig = new LlmConfig { ApiKey = "valid-api-key" };
    var transcribeConfig = new TranscribeConfig();
    services.AddSingleton(llmConfig);
    services.AddSingleton(transcribeConfig);

    // Act - Call the method under test via reflection since it's private
    var methodInfo = typeof(ServiceStartup).GetMethod("ConfigureOpenAiServices",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    methodInfo?.Invoke(null, new object[] { services });

    // Assert
    var serviceProvider = services.BuildServiceProvider();
    var openAiClient = serviceProvider.GetService<OpenAIClient>();
    var audioClient = serviceProvider.GetService<AudioClient>();

    openAiClient.Should().NotBeNull();
    audioClient.Should().NotBeNull();
    openAiClient.Should().NotBeOfType<OpenAIClientProxy>();
    audioClient.Should().NotBeOfType<AudioClientProxy>();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void ConfigureOpenAiServices_WhenLlmConfigIsUnavailable_RegistersProxyClients()
  {
    // Arrange
    var services = new ServiceCollection();

    // Mock dependencies
    var mockLogger = new Mock<ILogger<ServiceStartup>>();
    services.AddSingleton(mockLogger.Object);

    // Mock configuration status service that reports LLM service as unavailable
    var mockStatusService = new Mock<IConfigurationStatusService>();
    var statusDict = new Dictionary<string, ServiceStatusInfo>
        {
            { "Llm", new ServiceStatusInfo(false, new List<string> { "LlmConfig:ApiKey" }) }
        };
    mockStatusService.Setup(s => s.GetServiceStatusAsync())
        .ReturnsAsync(statusDict);
    services.AddSingleton(mockStatusService.Object);

    // Add invalid LlmConfig and valid TranscribeConfig
    var llmConfig = new LlmConfig { ApiKey = "" }; // Invalid/empty API key
    var transcribeConfig = new TranscribeConfig();
    services.AddSingleton(llmConfig);
    services.AddSingleton(transcribeConfig);

    // Act - Call the method under test via reflection since it's private
    var methodInfo = typeof(ServiceStartup).GetMethod("ConfigureOpenAiServices",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    methodInfo?.Invoke(null, new object[] { services });

    // Assert
    var serviceProvider = services.BuildServiceProvider();
    var openAiClient = serviceProvider.GetService<OpenAIClient>();
    var audioClient = serviceProvider.GetService<AudioClient>();

    openAiClient.Should().NotBeNull();
    audioClient.Should().NotBeNull();

    // Verify OpenAIClient is a proxy by checking if it throws ServiceUnavailableException
    var openAiException = Assert.Throws<ServiceUnavailableException>(() =>
        openAiClient.Chat);

    openAiException.Message.Should().Contain("LLM service is unavailable");
    openAiException.Reasons.Should().Contain("LlmConfig:ApiKey");

    // Verify AudioClient is a proxy by checking if it throws ServiceUnavailableException
    var audioStream = new MemoryStream();
    var audioException = Assert.Throws<ServiceUnavailableException>(() =>
        audioClient.TranscribeAudioAsync(audioStream).GetAwaiter().GetResult());

    audioException.Message.Should().Contain("Audio transcription service is unavailable");
    audioException.Reasons.Should().Contain("LlmConfig:ApiKey");
  }
}
