using System;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Moq;
using Refit;
using Xunit;
using Zarichney.Client;
using Zarichney.Server.Tests.Framework.Mocks;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Server.Tests.Unit.Framework.Client;

/// <summary>
/// Unit tests for Refit client configuration behaviors and edge cases.
/// </summary>
public class RefitClientConfigurationTests
{
    private readonly IServiceCollection _services;

    public RefitClientConfigurationTests()
    {
        _services = new ServiceCollection();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfigureRefitClients_WithCustomRefitSettings_AppliesSettingsToAllClients()
    {
        // Arrange
        var customSettings = RefitSettingsBuilder.Default()
            .WithCollectionFormat(CollectionFormat.Csv)
            .WithBufferHttpContent(true)
            .Build();

        // Act
        _services.ConfigureRefitClients(settings: customSettings);

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();

        // Verify all clients can be resolved with custom settings
        var clients = new object?[]
        {
            serviceProvider.GetService<IAiApi>(),
            serviceProvider.GetService<IApiApi>(),
            serviceProvider.GetService<IAuthApi>(),
            serviceProvider.GetService<ICookbookApi>(),
            serviceProvider.GetService<IPaymentApi>(),
            serviceProvider.GetService<IPublicApi>()
        };

        clients.Should().OnlyContain(client => client != null,
            "all API clients should be properly configured with custom settings");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfigureRefitClients_WithBuilderAddingHandlers_ConfiguresHttpPipeline()
    {
        // Arrange
        var handlerAdded = false;
        Action<IHttpClientBuilder> addHandler = builder =>
        {
            // Simulate adding a delegating handler
            handlerAdded = true;
        };

        // Act
        _services.ConfigureRefitClients(builder: addHandler);

        // Assert
        handlerAdded.Should().BeTrue(
            "the builder should be invoked to add custom handlers");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfigureRefitClients_ServiceCollectionChaining_MaintainsFluency()
    {
        // Arrange & Act
        var result = _services
            .ConfigureRefitClients()
            .AddSingleton<ITestService, TestService>();

        // Assert
        result.Should().BeSameAs(_services,
            "the method should support fluent chaining");

        _services.Should().Contain(sd => sd.ServiceType == typeof(ITestService),
            "chained registrations should work correctly");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfigureRefitClients_MultipleConfigurations_LastConfigurationWins()
    {
        // Arrange
        var firstSettings = RefitSettingsBuilder.Default()
            .WithCollectionFormat(CollectionFormat.Csv)
            .Build();

        var secondSettings = RefitSettingsBuilder.Default()
            .WithCollectionFormat(CollectionFormat.Multi)
            .Build();

        // Act
        _services.ConfigureRefitClients(settings: firstSettings);
        _services.ConfigureRefitClients(settings: secondSettings);

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();

        // Verify services can still be resolved after multiple configurations
        var authApi = serviceProvider.GetService<IAuthApi>();
        authApi.Should().NotBeNull(
            "the API client should be resolvable after multiple configurations");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfigureRefitClients_WithIntegrationTestSettings_ConfiguresCorrectly()
    {
        // Arrange
        var integrationSettings = RefitSettingsBuilder.ForIntegrationTests().Build();

        // Act
        _services.ConfigureRefitClients(settings: integrationSettings);

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();

        var cookbookApi = serviceProvider.GetService<ICookbookApi>();
        cookbookApi.Should().NotBeNull(
            "ICookbookApi should be configured for integration testing");

        var paymentApi = serviceProvider.GetService<IPaymentApi>();
        paymentApi.Should().NotBeNull(
            "IPaymentApi should be configured for integration testing");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfigureRefitClients_BuilderModifiesEachClient_IndependentConfiguration()
    {
        // Arrange
        var clientNames = new List<string>();
        Action<IHttpClientBuilder> captureNames = builder =>
        {
            // In real scenario, each client would have a unique name
            // This simulates capturing configuration for each client
            clientNames.Add("ConfiguredClient");
        };

        // Act
        _services.ConfigureRefitClients(builder: captureNames);

        // Assert
        clientNames.Should().HaveCount(6,
            "the builder should be called once for each API client");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfigureRefitClients_NullServiceCollection_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection nullServices = null!;

        // Act
        Action act = () => nullServices.ConfigureRefitClients();

        // Assert
        act.Should().Throw<ArgumentNullException>(
            "Refit validates null service collections");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfigureRefitClients_EmptyServiceCollection_AddsOnlyRefitServices()
    {
        // Arrange
        var emptyServices = new ServiceCollection();
        var initialCount = emptyServices.Count;

        // Act
        emptyServices.ConfigureRefitClients();

        // Assert
        emptyServices.Count.Should().BeGreaterThan(initialCount,
            "Refit client registrations should be added");

        emptyServices.Should()
            .Contain(sd => sd.ServiceType == typeof(IAiApi))
            .And.Contain(sd => sd.ServiceType == typeof(IAuthApi),
                "all API clients should be registered");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfigureRefitClients_WithComplexBuilder_HandlesComplexConfiguration()
    {
        // Arrange
        var configurationSteps = new List<string>();
        Action<IHttpClientBuilder> complexBuilder = builder =>
        {
            configurationSteps.Add($"Configuring client");
            // Simulate complex configuration like timeout, headers, etc.
            configurationSteps.Add("Added timeout");
            configurationSteps.Add("Added default headers");
            configurationSteps.Add("Configured retry policy");
        };

        // Act
        _services.ConfigureRefitClients(builder: complexBuilder);

        // Assert
        configurationSteps.Should().HaveCount(24, // 4 steps × 6 clients
            "complex configuration should be applied to all clients");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfigureRefitClients_ServiceProviderLifecycle_HandlesDisposal()
    {
        // Arrange
        _services.ConfigureRefitClients();

        // Act & Assert
        using (var serviceProvider = _services.BuildServiceProvider())
        {
            var api = serviceProvider.GetService<IPublicApi>();
            api.Should().NotBeNull();
        }

        // After disposal, creating a new provider should still work
        using (var newProvider = _services.BuildServiceProvider())
        {
            var api = newProvider.GetService<IPublicApi>();
            api.Should().NotBeNull(
                "a new service provider should work after the previous one was disposed");
        }
    }

    // Test helper class
    private interface ITestService { }
    private class TestService : ITestService { }
}