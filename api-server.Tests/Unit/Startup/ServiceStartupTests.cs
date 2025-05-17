using Zarichney.Services.Status;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moq;
using OpenAI;
using OpenAI.Audio;
using Xunit;
using Zarichney.Services.AI;
using Zarichney.Services.Email;
using Zarichney.Startup;

namespace Zarichney.Tests.Unit.Startup;

// We don't directly reference the proxy classes, but check their type names
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
    var mockStatusService = new Mock<IStatusService>();
    var statusDict = new Dictionary<ExternalServices, ServiceStatusInfo>
        {
            { ExternalServices.MsGraph, new ServiceStatusInfo(ExternalServices.MsGraph, true, []) }
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
    methodInfo?.Invoke(null, [services]);

    // Assert
    var serviceProvider = services.BuildServiceProvider();
    var graphClient = serviceProvider.GetService<GraphServiceClient>();
    graphClient.Should().NotBeNull();
    graphClient.GetType().Name.Should().NotBe("GraphServiceClientProxy");
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
    var mockStatusService = new Mock<IStatusService>();
    var statusDict = new Dictionary<ExternalServices, ServiceStatusInfo>
        {
            { ExternalServices.MsGraph, new ServiceStatusInfo(ExternalServices.MsGraph, false, ["EmailConfig:AzureTenantId"]) }
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
    methodInfo?.Invoke(null, [services]);

    // Assert
    var serviceProvider = services.BuildServiceProvider();
    var graphClient = serviceProvider.GetService<GraphServiceClient>();
    graphClient.Should().NotBeNull();

    // Verify it's a proxy by checking if it throws ServiceUnavailableException when accessing Me
    // Check if it's a proxy by examining its type name
    graphClient.GetType().Name.Should().Be("GraphServiceClientProxy");

    // Get Me property using reflection
    var meProperty = graphClient.GetType().GetProperty("Me", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

    // When accessing property via reflection, our exception is wrapped in a TargetInvocationException
    var exception = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
        meProperty?.GetValue(graphClient));

    // Verify the inner exception is our ServiceUnavailableException
    exception.InnerException.Should().BeOfType<ServiceUnavailableException>();
    var serviceException = exception.InnerException as ServiceUnavailableException;

    serviceException!.Message.Should().Contain("Email service is unavailable");
    serviceException.Reasons.Should().Contain("EmailConfig:AzureTenantId");
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
    var mockStatusService = new Mock<IStatusService>();
    var statusDict = new Dictionary<ExternalServices, ServiceStatusInfo>
        {
            { ExternalServices.OpenAiApi, new ServiceStatusInfo(ExternalServices.OpenAiApi, true, []) }
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
    methodInfo?.Invoke(null, [services]);

    // Assert
    var serviceProvider = services.BuildServiceProvider();
    var openAiClient = serviceProvider.GetService<OpenAIClient>();
    var audioClient = serviceProvider.GetService<AudioClient>();

    openAiClient.Should().NotBeNull();
    audioClient.Should().NotBeNull();
    openAiClient.GetType().Name.Should().NotBe("OpenAIClientProxy");
    audioClient.GetType().Name.Should().NotBe("AudioClientProxy");
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
    var mockStatusService = new Mock<IStatusService>();
    var statusDict = new Dictionary<ExternalServices, ServiceStatusInfo>
        {
            { ExternalServices.OpenAiApi, new ServiceStatusInfo(ExternalServices.OpenAiApi, false, ["LlmConfig:ApiKey"]) }
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
    methodInfo?.Invoke(null, [services]);

    // Assert
    var serviceProvider = services.BuildServiceProvider();
    var openAiClient = serviceProvider.GetService<OpenAIClient>();
    var audioClient = serviceProvider.GetService<AudioClient>();

    openAiClient.Should().NotBeNull();
    audioClient.Should().NotBeNull();

    // Verify OpenAIClient is a proxy by examining its type name
    openAiClient.GetType().Name.Should().Be("OpenAIClientProxy");

    // Get Chat property using reflection
    var chatProperty = openAiClient.GetType().GetProperty("Chat", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

    // When accessing property via reflection, our exception is wrapped in a TargetInvocationException
    var openAiException = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
        chatProperty?.GetValue(openAiClient));

    // Verify the inner exception is our ServiceUnavailableException
    openAiException.InnerException.Should().BeOfType<ServiceUnavailableException>();
    var openAiServiceException = openAiException.InnerException as ServiceUnavailableException;

    openAiServiceException!.Message.Should().Contain("LLM service is unavailable");
    openAiServiceException.Reasons.Should().Contain("LlmConfig:ApiKey");

    // Verify AudioClient is a proxy by examining its type name
    audioClient.GetType().Name.Should().Be("AudioClientProxy");

    // Check if IsThrowingProxy property exists and is true
    var proxyProperty = audioClient.GetType().GetProperty("IsThrowingProxy", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
    var isThrowingProxy = proxyProperty?.GetValue(audioClient) as bool?;
    isThrowingProxy.Should().BeTrue();

    // Get our custom TranscribeAudioAsync method
    var transcribeMethod = audioClient.GetType().GetMethod("TranscribeAudioAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly, null,
      [typeof(Stream)], null);

    // Verify calling the method throws ServiceUnavailableException wrapped in TargetInvocationException
    var audioStream = new MemoryStream();
    var audioException = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
        transcribeMethod?.Invoke(audioClient, [audioStream]));

    // Verify the inner exception is our ServiceUnavailableException
    audioException.InnerException.Should().BeOfType<ServiceUnavailableException>();
    var audioServiceException = audioException.InnerException as ServiceUnavailableException;

    audioServiceException!.Message.Should().Contain("Audio transcription service is unavailable");
    audioServiceException.Reasons.Should().Contain("LlmConfig:ApiKey");
  }
}
