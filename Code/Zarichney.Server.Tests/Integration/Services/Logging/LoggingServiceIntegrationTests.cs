using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zarichney.Services.Logging;
using Zarichney.Services.ProcessExecution;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;
using Xunit.Abstractions;
using Zarichney.Tests.Integration;

namespace Zarichney.Server.Tests.Integration.Services.Logging;

/// <summary>
/// Integration tests for LoggingService that require external dependencies.
/// These tests verify real connectivity to external services like Seq and Docker,
/// and will be skipped in environments where these dependencies are not available.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Component, TestCategories.Service)]
[Trait(TestCategories.Feature, "Logging")]
[Collection("IntegrationInfra")]
public class LoggingServiceIntegrationTests : IntegrationTestBase
{
  private const int TestTimeoutMs = 10000;
  private const string TestSeqContainerName = "seq-integration-test";
  
  public LoggingServiceIntegrationTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper) 
    : base(apiClientFixture, testOutputHelper)
  {
  }
  
  #region Real Seq Connectivity Tests

  /// <summary>
  /// Tests connectivity to a real Seq instance running on standard port.
  /// This test will be skipped if Docker is not available to start a Seq container.
  /// </summary>
  [DependencyFact(InfrastructureDependency.Docker)]
  [Trait(TestCategories.Dependency, TestCategories.Docker)]
  public async Task TestSeqConnectivityAsync_WithRealSeqInstance_ReturnsSuccessResult()
  {
    // Arrange
    using var scope = Factory.Services.CreateScope();
    var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();
    
    // Act
    var result = await loggingService.TestSeqConnectivityAsync("http://localhost:5341");
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("http://localhost:5341", result.Url);
    
    // If Seq is running, we expect a successful connection
    if (await IsSeqAvailable())
    {
      Assert.True(result.IsConnected, "Expected successful connection to running Seq instance");
      Assert.True(result.ResponseTime >= 0, "Response time should be non-negative for successful connection");
      Assert.Null(result.Error);
    }
    else
    {
      Assert.False(result.IsConnected, "Expected failed connection when Seq is not available");
      Assert.Equal(-1, result.ResponseTime);
    }
  }

  /// <summary>
  /// Tests the Docker Seq fallback functionality.
  /// This test will be skipped if Docker is not available.
  /// </summary>
  [DependencyFact(InfrastructureDependency.Docker)]
  [Trait(TestCategories.Dependency, TestCategories.Docker)]
  public async Task TryStartDockerSeqAsync_WithDockerAvailable_AttemptsContainerStartup()
  {
    // Arrange
    using var scope = Factory.Services.CreateScope();
    var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();
    var processExecutor = scope.ServiceProvider.GetRequiredService<IProcessExecutor>();
    
    // Ensure test container is not running
    await StopTestContainer(processExecutor);
    
    try
    {
      // Act
      var result = await loggingService.TryStartDockerSeqAsync();
      
      // Assert
      // The result depends on whether the default container name is already in use
      // or if we can successfully start a new container
      if (result)
      {
        // Verify container is running
        var containerRunning = await IsContainerRunning(processExecutor, "seq-fallback");
        Assert.True(containerRunning, "Container should be running after successful start");
      }
    }
    finally
    {
      // Cleanup: ensure we don't leave test containers running
      await StopContainer(processExecutor, "seq-fallback");
    }
  }

  /// <summary>
  /// Tests intelligent Seq URL discovery with multiple URLs.
  /// This test will be skipped if Docker is not available for fallback testing.
  /// </summary>
  [DependencyFact(InfrastructureDependency.Docker)]
  [Trait(TestCategories.Dependency, TestCategories.Docker)]
  public async Task GetBestAvailableSeqUrlAsync_WithMultipleUrls_FindsBestOption()
  {
    // Arrange
    using var scope = Factory.Services.CreateScope();
    var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();
    
    // Act
    var bestUrl = await loggingService.GetBestAvailableSeqUrlAsync();
    
    // Assert
    if (await IsSeqAvailable())
    {
      Assert.NotNull(bestUrl);
      Assert.Contains(bestUrl, new[] { "http://localhost:5341", "http://127.0.0.1:5341", "http://localhost:8080" });
      
      // Verify the returned URL is actually accessible
      var connectivityResult = await loggingService.TestSeqConnectivityAsync(bestUrl);
      Assert.True(connectivityResult.IsConnected, $"Best URL {bestUrl} should be accessible");
    }
    else
    {
      // If no Seq is available and Docker fallback fails, we expect null
      Assert.True(bestUrl == null || await IsUrlAccessible(loggingService, bestUrl), 
        "Returned URL should be accessible or null");
    }
  }

  #endregion

  #region Docker Container Management Tests

  /// <summary>
  /// Tests Docker container detection for running Seq containers.
  /// This test will be skipped if Docker is not available.
  /// </summary>
  [DependencyFact(InfrastructureDependency.Docker)]
  [Trait(TestCategories.Dependency, TestCategories.Docker)]
  public async Task IsDockerSeqContainerRunningAsync_ChecksContainerStatus()
  {
    // Arrange
    using var scope = Factory.Services.CreateScope();
    var processExecutor = scope.ServiceProvider.GetRequiredService<IProcessExecutor>();
    var seqConnectivity = scope.ServiceProvider.GetRequiredService<ISeqConnectivity>();
    
    // Start a test container
    await StartTestContainer(processExecutor);
    
    try
    {
      // Act - Use reflection to test private method
      var method = seqConnectivity.GetType()
        .GetMethod("IsDockerSeqContainerRunningAsync", 
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      
      Assert.NotNull(method);
      
      var task = method.Invoke(seqConnectivity, new object[] { CancellationToken.None }) as Task<bool>;
      Assert.NotNull(task);
      
      var isRunning = await task;
      
      // Assert
      Assert.True(isRunning, "Should detect running Seq container");
    }
    finally
    {
      // Cleanup
      await StopTestContainer(processExecutor);
    }
  }

  /// <summary>
  /// Tests Docker container name resolution.
  /// This test will be skipped if Docker is not available.
  /// </summary>
  [DependencyFact(InfrastructureDependency.Docker)]
  [Trait(TestCategories.Dependency, TestCategories.Docker)]
  public async Task GetDockerSeqContainerNameAsync_ReturnsContainerInfo()
  {
    // Arrange
    using var scope = Factory.Services.CreateScope();
    var processExecutor = scope.ServiceProvider.GetRequiredService<IProcessExecutor>();
    var loggingStatus = scope.ServiceProvider.GetRequiredService<ILoggingStatus>();
    
    // Start a test container
    await StartTestContainer(processExecutor);
    
    try
    {
      // Act - Use reflection to test private method
      var method = loggingStatus.GetType()
        .GetMethod("GetDockerSeqContainerNameAsync", 
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      
      Assert.NotNull(method);
      
      var task = method.Invoke(loggingStatus, new object[] { CancellationToken.None }) as Task<string?>;
      Assert.NotNull(task);
      
      var containerName = await task;
      
      // Assert
      Assert.NotNull(containerName);
      Assert.Contains("seq", containerName.ToLower());
    }
    finally
    {
      // Cleanup
      await StopTestContainer(processExecutor);
    }
  }

  #endregion

  #region End-to-End Integration Tests

  /// <summary>
  /// Tests complete logging method discovery with real dependencies.
  /// This provides comprehensive validation of intelligent fallback logic.
  /// </summary>
  [DependencyFact(InfrastructureDependency.Docker)]
  [Trait(TestCategories.Dependency, TestCategories.Docker)]
  public async Task GetAvailableLoggingMethodsAsync_WithRealEnvironment_ShowsActualAvailability()
  {
    // Arrange
    using var scope = Factory.Services.CreateScope();
    var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();
    
    // Act
    var methods = await loggingService.GetAvailableLoggingMethodsAsync();
    
    // Assert
    Assert.NotNull(methods);
    
    // File logging should always be available
    Assert.NotNull(methods.FileLogging);
    Assert.True(methods.FileLogging.Available, "File logging should always be available");
    Assert.Equal("File-based logging (always available)", methods.FileLogging.Method);
    Assert.NotNull(methods.FileLogging.Path);
    
    // Docker Seq availability depends on Docker being available
    Assert.NotNull(methods.DockerSeq);
    if (await IsDockerAvailable())
    {
      // Docker is available, so we can check container status
      var containerRunning = await IsAnySeqContainerRunning();
      Assert.Equal(containerRunning, methods.DockerSeq.Available);
      
      if (containerRunning)
      {
        Assert.NotNull(methods.DockerSeq.ServiceName);
        Assert.Equal(5341, methods.DockerSeq.Port);
      }
    }
    
    // Native Seq check (may or may not be running)
    Assert.NotNull(methods.NativeSeq);
    Assert.Equal("seq", methods.NativeSeq.ServiceName);
    Assert.Equal(5341, methods.NativeSeq.Port);
    Assert.Equal("Native systemd service", methods.NativeSeq.Method);
    
    // Current method should be set
    Assert.NotNull(methods.CurrentMethod);
    Assert.Contains(methods.CurrentMethod, new[] 
    { 
      "File Logging (Fallback)", 
      "Native Seq Service", 
      "Docker Container", 
      "Unknown Seq Method" 
    });
  }

  /// <summary>
  /// Tests complete logging status reporting with real environment.
  /// This validates the full status reporting pipeline with actual dependencies.
  /// </summary>
  [DependencyFact(InfrastructureDependency.Docker)]
  [Trait(TestCategories.Dependency, TestCategories.Docker)]
  public async Task GetLoggingStatusAsync_WithRealEnvironment_ReturnsComprehensiveStatus()
  {
    // Arrange
    using var scope = Factory.Services.CreateScope();
    var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();
    
    // Act
    var status = await loggingService.GetLoggingStatusAsync();
    
    // Assert
    Assert.NotNull(status);
    
    // Basic fields should always be populated
    Assert.NotNull(status.Method);
    Assert.NotNull(status.LogLevel);
    Assert.NotNull(status.FileLoggingPath);
    Assert.True(status.Timestamp <= DateTime.UtcNow, "Timestamp should not be in the future");
    Assert.True(status.Timestamp > DateTime.UtcNow.AddMinutes(-1), "Timestamp should be recent");
    
    // Seq availability and URL consistency
    if (status.SeqAvailable)
    {
      Assert.NotNull(status.SeqUrl);
      Assert.False(status.FallbackActive, "Fallback should not be active when Seq is available");
      
      // Verify the reported URL is actually accessible
      var connectivityResult = await loggingService.TestSeqConnectivityAsync(status.SeqUrl);
      Assert.True(connectivityResult.IsConnected, 
        $"Reported Seq URL {status.SeqUrl} should be accessible");
    }
    else
    {
      Assert.True(status.FallbackActive, "Fallback should be active when Seq is not available");
      Assert.Equal("File Logging (Fallback)", status.Method);
    }
    
    // Method consistency
    Assert.Contains(status.Method, new[] 
    { 
      "File Logging (Fallback)", 
      "Native Seq Service", 
      "Docker Container", 
      "Unknown Seq Method" 
    });
  }

  #endregion
  
  #region Helper Methods
  
  private async Task<bool> IsSeqAvailable()
  {
    try
    {
      using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
      var response = await httpClient.GetAsync("http://localhost:5341/api/events/raw");
      return response.IsSuccessStatusCode;
    }
    catch
    {
      return false;
    }
  }
  
  private async Task<bool> IsDockerAvailable()
  {
    using var scope = Factory.Services.CreateScope();
    var processExecutor = scope.ServiceProvider.GetRequiredService<IProcessExecutor>();
    
    try
    {
      var result = await processExecutor.RunCommandAsync("docker", "version", 5000);
      return result.exitCode == 0;
    }
    catch
    {
      return false;
    }
  }
  
  private async Task<bool> IsAnySeqContainerRunning()
  {
    using var scope = Factory.Services.CreateScope();
    var processExecutor = scope.ServiceProvider.GetRequiredService<IProcessExecutor>();
    
    try
    {
      var result = await processExecutor.RunCommandAsync(
        "docker", 
        "ps --filter name=seq --format {{.Names}}", 
        5000);
      return result.exitCode == 0 && !string.IsNullOrWhiteSpace(result.output);
    }
    catch
    {
      return false;
    }
  }
  
  private async Task<bool> IsContainerRunning(IProcessExecutor processExecutor, string containerName)
  {
    try
    {
      var result = await processExecutor.RunCommandAsync(
        "docker", 
        $"ps --filter name={containerName} --format {{{{.Names}}}}", 
        5000);
      return result.exitCode == 0 && result.output.Contains(containerName);
    }
    catch
    {
      return false;
    }
  }
  
  private async Task<bool> IsUrlAccessible(ILoggingService loggingService, string url)
  {
    var result = await loggingService.TestSeqConnectivityAsync(url);
    return result.IsConnected;
  }
  
  private async Task StartTestContainer(IProcessExecutor processExecutor)
  {
    // First, ensure any existing test container is stopped
    await StopTestContainer(processExecutor);
    
    // Start new test container
    var arguments = $"run --name {TestSeqContainerName} -d --rm -e ACCEPT_EULA=Y -p 5342:80 datalust/seq:latest";
    await processExecutor.RunCommandAsync("docker", arguments, TestTimeoutMs);
    
    // Wait for container to be ready
    await Task.Delay(3000);
  }
  
  private async Task StopTestContainer(IProcessExecutor processExecutor)
  {
    await StopContainer(processExecutor, TestSeqContainerName);
  }
  
  private async Task StopContainer(IProcessExecutor processExecutor, string containerName)
  {
    try
    {
      // Stop container
      await processExecutor.RunCommandAsync("docker", $"stop {containerName}", TestTimeoutMs);
      
      // Remove container (in case it wasn't started with --rm)
      await processExecutor.RunCommandAsync("docker", $"rm -f {containerName}", TestTimeoutMs);
    }
    catch
    {
      // Ignore errors - container might not exist
    }
  }
  
  #endregion
}