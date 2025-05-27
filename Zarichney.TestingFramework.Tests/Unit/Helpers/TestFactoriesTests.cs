using FluentAssertions;
using Xunit;
using Zarichney.Services.Status;
using Zarichney.TestingFramework.Helpers;

namespace Zarichney.TestingFramework.Tests.Unit.Helpers;

/// <summary>
/// Unit tests for TestFactories utility class.
/// </summary>
public class TestFactoriesTests
{
    [Fact]
    public void CreateServiceStatus_WithDefaults_ShouldReturnServiceStatusWithExpectedDefaults()
    {
        // Act
        var result = TestFactories.CreateServiceStatus();

        // Assert
        result.Should().NotBeNull();
        result.serviceName.Should().Be(ExternalServices.OpenAiApi);
        result.IsAvailable.Should().BeTrue();
        result.MissingConfigurations.Should().NotBeNull();
        result.MissingConfigurations.Should().BeEmpty();
    }

    [Fact]
    public void CreateServiceStatus_WithCustomValues_ShouldReturnServiceStatusWithSpecifiedValues()
    {
        // Arrange
        const ExternalServices serviceName = ExternalServices.GitHubAccess;
        const bool isAvailable = false;
        var missingConfigurations = new List<string> { "ApiKey", "Secret" };

        // Act
        var result = TestFactories.CreateServiceStatus(serviceName, isAvailable, missingConfigurations);

        // Assert
        result.Should().NotBeNull();
        result.serviceName.Should().Be(serviceName);
        result.IsAvailable.Should().Be(isAvailable);
        result.MissingConfigurations.Should().BeEquivalentTo(missingConfigurations);
    }

    [Fact]
    public void CreateServiceStatus_WithNullMissingConfigurations_ShouldReturnEmptyList()
    {
        // Act
        var result = TestFactories.CreateServiceStatus(missingConfigurations: null);

        // Assert
        result.MissingConfigurations.Should().NotBeNull();
        result.MissingConfigurations.Should().BeEmpty();
    }

    [Fact]
    public void CreateConfigurationStatus_WithDefaults_ShouldReturnConfigurationStatusWithExpectedDefaults()
    {
        // Act
        var result = TestFactories.CreateConfigurationStatus();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Configuration");
        result.Status.Should().Be("Configured");
        result.Details.Should().BeNull();
    }

    [Fact]
    public void CreateConfigurationStatus_WithCustomValues_ShouldReturnConfigurationStatusWithSpecifiedValues()
    {
        // Arrange
        const string name = "Custom Config";
        const string status = "Missing";
        const string details = "Configuration not found";

        // Act
        var result = TestFactories.CreateConfigurationStatus(name, status, details);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.Status.Should().Be(status);
        result.Details.Should().Be(details);
    }

    [Fact]
    public void CreateServiceStatusList_WithDefaults_ShouldReturnListWithCorrectDefaults()
    {
        // Act
        var result = TestFactories.CreateServiceStatusList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().OnlyContain(s => s.IsAvailable);
        result.Should().OnlyContain(s => s.MissingConfigurations.Count == 0);
    }

    [Fact]
    public void CreateServiceStatusList_WithCustomCount_ShouldReturnListWithSpecifiedCount()
    {
        // Arrange
        const int count = 2;

        // Act
        var result = TestFactories.CreateServiceStatusList(count);

        // Assert
        result.Should().HaveCount(count);
    }

    [Fact]
    public void CreateServiceStatusList_WithAllUnavailable_ShouldReturnListWithUnavailableServices()
    {
        // Arrange
        const bool allAvailable = false;

        // Act
        var result = TestFactories.CreateServiceStatusList(allAvailable: allAvailable);

        // Assert
        result.Should().OnlyContain(s => !s.IsAvailable);
        result.Should().OnlyContain(s => s.MissingConfigurations.Count > 0);
    }

    [Fact]
    public void CreateServiceStatusList_WithCountGreaterThanAvailableServices_ShouldNotExceedAvailableServices()
    {
        // Arrange
        const int count = 10; // More than available services

        // Act
        var result = TestFactories.CreateServiceStatusList(count);

        // Assert
        result.Should().HaveCount(5); // Should be limited to available services
    }

    [Fact]
    public void CreateServiceStatusList_ShouldReturnDifferentServices()
    {
        // Act
        var result = TestFactories.CreateServiceStatusList(3);

        // Assert
        var serviceNames = result.Select(s => s.serviceName).ToList();
        serviceNames.Should().OnlyHaveUniqueItems();
        serviceNames.Should().Contain(ExternalServices.OpenAiApi);
        serviceNames.Should().Contain(ExternalServices.GitHubAccess);
        serviceNames.Should().Contain(ExternalServices.Stripe);
    }

    [Fact]
    public void CreateServiceStatusDictionary_WithDefaults_ShouldReturnDictionaryWithCorrectDefaults()
    {
        // Act
        var result = TestFactories.CreateServiceStatusDictionary();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Values.Should().OnlyContain(s => s.IsAvailable);
        result.Values.Should().OnlyContain(s => s.MissingConfigurations.Count == 0);
    }

    [Fact]
    public void CreateServiceStatusDictionary_WithCustomCount_ShouldReturnDictionaryWithSpecifiedCount()
    {
        // Arrange
        const int count = 2;

        // Act
        var result = TestFactories.CreateServiceStatusDictionary(count);

        // Assert
        result.Should().HaveCount(count);
    }

    [Fact]
    public void CreateServiceStatusDictionary_WithAllUnavailable_ShouldReturnDictionaryWithUnavailableServices()
    {
        // Arrange
        const bool allAvailable = false;

        // Act
        var result = TestFactories.CreateServiceStatusDictionary(allAvailable: allAvailable);

        // Assert
        result.Values.Should().OnlyContain(s => !s.IsAvailable);
        result.Values.Should().OnlyContain(s => s.MissingConfigurations.Count > 0);
    }

    [Fact]
    public void CreateServiceStatusDictionary_KeysShouldMatchServiceNames()
    {
        // Act
        var result = TestFactories.CreateServiceStatusDictionary(3);

        // Assert
        foreach (var kvp in result)
        {
            kvp.Value.serviceName.Should().Be(kvp.Key);
        }
    }

    [Fact]
    public void CreateServiceStatusDictionary_ShouldContainExpectedServices()
    {
        // Act
        var result = TestFactories.CreateServiceStatusDictionary(3);

        // Assert
        result.Keys.Should().Contain(ExternalServices.OpenAiApi);
        result.Keys.Should().Contain(ExternalServices.GitHubAccess);
        result.Keys.Should().Contain(ExternalServices.Stripe);
    }

    [Fact]
    public void CreateServiceStatusDictionary_WithZeroCount_ShouldReturnEmptyDictionary()
    {
        // Act
        var result = TestFactories.CreateServiceStatusDictionary(0);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CreateServiceStatusList_WithZeroCount_ShouldReturnEmptyList()
    {
        // Act
        var result = TestFactories.CreateServiceStatusList(0);

        // Assert
        result.Should().BeEmpty();
    }
}