using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.Status;

namespace Zarichney.Tests.Unit.Services.Status;

public class ConfigurationStatusServiceTests
{
  private readonly Mock<IServiceProvider> _mockServiceProvider;
  private readonly Mock<IConfiguration> _mockConfiguration;
  private readonly ConfigurationStatusService _service;
  private readonly Mock<Assembly> _mockAssembly;

  public ConfigurationStatusServiceTests()
  {
    _mockServiceProvider = new Mock<IServiceProvider>();
    _mockConfiguration = new Mock<IConfiguration>();
    _mockAssembly = new Mock<Assembly>(); // Mock the assembly
    var mockLogger = new Mock<ILogger<ConfigurationStatusService>>();

    // Setup mock assembly to return our test config types
    _mockAssembly.Setup(asm => asm.GetTypes()).Returns([typeof(TestService1Config), typeof(TestService2Config)]);

    // Pass the mocked assembly directly to the service constructor
    _service = new ConfigurationStatusService(_mockServiceProvider.Object, _mockConfiguration.Object, mockLogger.Object, _mockAssembly.Object);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetServiceStatusAsync_WithNoRegisteredConfigs_ReturnsEmptyDictionary()
  {
    // Arrange
    SetupEmptyServiceProvider();

    // Act
    var result = await _service.GetServiceStatusAsync();

    // Assert
    result.Should().BeEmpty();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetServiceStatusAsync_WithAllConfigsAvailable_ReturnsAvailableServices()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithValidValues();

    // Act
    var result = await _service.GetServiceStatusAsync();

    // Assert
    result.Should().NotBeEmpty();
    result["TestService1"].IsAvailable.Should().BeTrue();
    result["TestService1"].MissingConfigurations.Should().BeEmpty();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetServiceStatusAsync_WithMissingConfigs_ReturnsUnavailableServices()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithMissingValues();

    // Act
    var result = await _service.GetServiceStatusAsync();

    // Assert
    result.Should().NotBeEmpty();
    result["TestService1"].IsAvailable.Should().BeFalse();
    result["TestService1"].MissingConfigurations.Should().Contain("TestService1Config:ApiKey");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetServiceStatusAsync_WithPlaceholderValues_ReturnsUnavailableServices()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithPlaceholderValues();

    // Act
    var result = await _service.GetServiceStatusAsync();

    // Assert
    result.Should().NotBeEmpty();
    result["TestService1"].IsAvailable.Should().BeFalse();
    result["TestService1"].MissingConfigurations.Should().Contain("TestService1Config:ApiKey");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetFeatureStatus_WithValidFeature_ReturnsCorrectStatus()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithValidValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _service.GetServiceStatusAsync();
    var result = _service.GetFeatureStatus(Feature.LLM);

    // Assert
    result.Should().NotBeNull();
    result.IsAvailable.Should().BeTrue();
    result.MissingConfigurations.Should().BeEmpty();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetFeatureStatus_WithInvalidFeature_ReturnsNull()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithValidValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _service.GetServiceStatusAsync();
    // Use string overload for non-existent feature
    var result = _service.GetFeatureStatus("NonExistentFeature");

    // Assert
    result.Should().BeNull();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task GetFeatureStatus_WithUnavailableFeature_ReturnsCorrectStatus()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithMissingValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _service.GetServiceStatusAsync();
    var result = _service.GetFeatureStatus(Feature.LLM);

    // Assert
    result.Should().NotBeNull();
    result.IsAvailable.Should().BeFalse();
    result.MissingConfigurations.Should().Contain("TestService1Config:ApiKey");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task IsFeatureAvailable_WithAvailableFeature_ReturnsTrue()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithValidValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _service.GetServiceStatusAsync();
    var result = _service.IsFeatureAvailable(Feature.LLM);

    // Assert
    result.Should().BeTrue();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task IsFeatureAvailable_WithUnavailableFeature_ReturnsFalse()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithMissingValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _service.GetServiceStatusAsync();
    var result = _service.IsFeatureAvailable(Feature.LLM);

    // Assert
    result.Should().BeFalse();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public async Task IsFeatureAvailable_WithNonExistentFeature_ReturnsFalse()
  {
    // Arrange
    SetupServiceProviderWithTestConfigs();
    SetupConfigurationWithValidValues();

    // Act - Call GetServiceStatusAsync to populate cache first
    await _service.GetServiceStatusAsync();
    // Use string overload for non-existent feature
    var result = _service.IsFeatureAvailable("NonExistentFeature");

    // Assert
    result.Should().BeFalse();
  }

  private void SetupEmptyServiceProvider()
  {
    // Ensure GetTypes returns an empty array for this test case
    _mockAssembly.Setup(asm => asm.GetTypes()).Returns(Type.EmptyTypes);

    // Mock GetService to return null for any type, simulating no registered IConfig services
    _mockServiceProvider.Setup(p => p.GetService(It.IsAny<Type>())).Returns(null!);
  }

  private void SetupServiceProviderWithTestConfigs()
  {
    // Reset GetTypes to return test config types for other tests
    _mockAssembly.Setup(asm => asm.GetTypes()).Returns([typeof(TestService1Config), typeof(TestService2Config)]);

    _mockServiceProvider.Setup(p => p.GetService(typeof(TestService1Config)))
        .Returns(new TestService1Config());
    _mockServiceProvider.Setup(p => p.GetService(typeof(TestService2Config)))
        .Returns(new TestService2Config());
  }

  private void SetupConfigurationWithValidValues()
  {
    _mockConfiguration.Setup(c => c["TestService1Config:ApiKey"]).Returns("valid-api-key");
    _mockConfiguration.Setup(c => c["TestService1Config:ApiSecret"]).Returns("valid-api-secret");
    _mockConfiguration.Setup(c => c["TestService2Config:Setting"]).Returns("valid-setting");
  }

  private void SetupConfigurationWithMissingValues()
  {
    _mockConfiguration.Setup(c => c["TestService1Config:ApiKey"]).Returns((string?)null);
  }

  private void SetupConfigurationWithPlaceholderValues()
  {
    _mockConfiguration.Setup(c => c["TestService1Config:ApiKey"]).Returns(StatusService.PlaceholderMessage);
  }

  // Test Config Classes
  private class TestService1Config : IConfig
  {
    [RequiresConfiguration(Feature.LLM)]
    public string ApiKey { get; set; } = "valid_key"; // Default to valid for simplicity in some tests

    [RequiresConfiguration(Feature.LLM)]
    public string ApiSecret { get; set; } = "valid_secret";
  }

  private class TestService2Config : IConfig
  {
    [RequiresConfiguration(Feature.Core)]
    public string Setting { get; set; } = "valid_setting";
  }
}
