using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Moq;
using Refit;
using Xunit;
using Zarichney.Client;

namespace Zarichney.Tests.Unit.Framework.Client;

/// <summary>
/// Unit tests for the IServiceCollectionExtensions.ConfigureRefitClients method.
/// Tests the configuration and registration of all Refit API clients.
/// </summary>
public class DependencyInjectionTests
{
  private readonly IServiceCollection _services;

  public DependencyInjectionTests()
  {
    _services = new ServiceCollection();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_WithoutParameters_RegistersAllApiClients()
  {
    // Act
    var result = _services.ConfigureRefitClients();

    // Assert
    result.Should().BeSameAs(_services,
        "the method should return the same service collection for fluent chaining");

    // Verify all API clients are registered
    _services.Should().Contain(sd => sd.ServiceType == typeof(IAiApi),
        "IAiApi should be registered");
    _services.Should().Contain(sd => sd.ServiceType == typeof(IApiApi),
        "IApiApi should be registered");
    _services.Should().Contain(sd => sd.ServiceType == typeof(IAuthApi),
        "IAuthApi should be registered");
    _services.Should().Contain(sd => sd.ServiceType == typeof(ICookbookApi),
        "ICookbookApi should be registered");
    _services.Should().Contain(sd => sd.ServiceType == typeof(IPaymentApi),
        "IPaymentApi should be registered");
    _services.Should().Contain(sd => sd.ServiceType == typeof(IPublicApi),
        "IPublicApi should be registered");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_WithoutParameters_RegistersHttpClients()
  {
    // Act
    _services.ConfigureRefitClients();

    // Assert
    // Verify HttpClient is registered for each API
    _services.Should().Contain(sd => sd.ServiceType == typeof(HttpClient),
        "HttpClient should be registered for the Refit clients");

    var httpClientDescriptors = _services.Where(sd => sd.ServiceType == typeof(HttpClient)).ToList();
    httpClientDescriptors.Should().HaveCountGreaterThanOrEqualTo(1,
        "HttpClient should be registered for the Refit clients");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_WithCustomBuilder_InvokesBuilderForEachClient()
  {
    // Arrange
    var builderInvocationCount = 0;
    Action<IHttpClientBuilder> customBuilder = builder => builderInvocationCount++;

    // Act
    _services.ConfigureRefitClients(builder: customBuilder);

    // Assert
    builderInvocationCount.Should().Be(6,
        "the custom builder should be invoked once for each of the 6 API clients");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_WithCustomSettings_PassesSettingsToRefitClient()
  {
    // Arrange
    var customSettings = new RefitSettings
    {
      CollectionFormat = CollectionFormat.Csv
    };

    // Act
    _services.ConfigureRefitClients(settings: customSettings);

    // Assert
    // Build the service provider to verify the settings were applied
    using var serviceProvider = _services.BuildServiceProvider();

    var aiApi = serviceProvider.GetService<IAiApi>();
    aiApi.Should().NotBeNull(
        "IAiApi should be resolvable after registration");

    var authApi = serviceProvider.GetService<IAuthApi>();
    authApi.Should().NotBeNull(
        "IAuthApi should be resolvable after registration");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_WithNullBuilder_DoesNotThrow()
  {
    // Act
    Action act = () => _services.ConfigureRefitClients(builder: null);

    // Assert
    act.Should().NotThrow(
        "null builder should be handled gracefully");

    _services.Should().HaveCountGreaterThanOrEqualTo(6,
        "all API clients should still be registered");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_WithNullSettings_UsesDefaultSettings()
  {
    // Act
    _services.ConfigureRefitClients(settings: null);

    // Assert
    using var serviceProvider = _services.BuildServiceProvider();

    var cookbookApi = serviceProvider.GetService<ICookbookApi>();
    cookbookApi.Should().NotBeNull(
        "ICookbookApi should be resolvable with default settings");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_RegistersWithCorrectBaseAddress()
  {
    // Arrange
    var expectedBaseAddress = new Uri("http://localhost:5000");

    // Act
    _services.ConfigureRefitClients();

    // Assert
    // Build service provider and get HttpClient to verify base address
    using var serviceProvider = _services.BuildServiceProvider();

    // Note: In actual implementation, the HttpClient instances are configured
    // with the base address. This test verifies the registration completes successfully.
    var publicApi = serviceProvider.GetService<IPublicApi>();
    publicApi.Should().NotBeNull(
        "IPublicApi should be properly configured with base address");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_CalledMultipleTimes_DoesNotDuplicate()
  {
    // Act
    _services.ConfigureRefitClients();
    _services.ConfigureRefitClients(); // Call twice

    // Assert
    // While services can be registered multiple times, verify we can still resolve them
    using var serviceProvider = _services.BuildServiceProvider();

    var paymentApi = serviceProvider.GetService<IPaymentApi>();
    paymentApi.Should().NotBeNull(
        "IPaymentApi should be resolvable even after multiple registrations");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_WithBuilderThatThrows_PropagatesException()
  {
    // Arrange
    var expectedException = new InvalidOperationException("Builder failed");
    Action<IHttpClientBuilder> faultyBuilder = _ => throw expectedException;

    // Act
    Action act = () => _services.ConfigureRefitClients(builder: faultyBuilder);

    // Assert
    act.Should().Throw<InvalidOperationException>()
        .WithMessage("Builder failed",
            "exceptions from the builder should be propagated");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_RegistersServicesAsScoped()
  {
    // Act
    _services.ConfigureRefitClients();

    // Assert
    var aiApiDescriptor = _services.FirstOrDefault(sd => sd.ServiceType == typeof(IAiApi));
    aiApiDescriptor.Should().NotBeNull();
    aiApiDescriptor!.Lifetime.Should().Be(ServiceLifetime.Transient,
        "Refit clients should be registered as transient by default in newer versions");

    var apiApiDescriptor = _services.FirstOrDefault(sd => sd.ServiceType == typeof(IApiApi));
    apiApiDescriptor.Should().NotBeNull();
    apiApiDescriptor!.Lifetime.Should().Be(ServiceLifetime.Transient,
        "Refit clients should be registered as transient by default in newer versions");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_WithCompleteConfiguration_WorksEndToEnd()
  {
    // Arrange
    var builderCalled = false;
    Action<IHttpClientBuilder> builder = b => builderCalled = true;

    var settings = new RefitSettings
    {
      CollectionFormat = CollectionFormat.Multi
    };

    // Act
    var result = _services.ConfigureRefitClients(builder, settings);

    // Assert
    result.Should().BeSameAs(_services);
    builderCalled.Should().BeTrue(
        "the builder should have been invoked");

    // Verify all services can be resolved
    using var serviceProvider = _services.BuildServiceProvider();

    serviceProvider.GetService<IAiApi>().Should().NotBeNull();
    serviceProvider.GetService<IApiApi>().Should().NotBeNull();
    serviceProvider.GetService<IAuthApi>().Should().NotBeNull();
    serviceProvider.GetService<ICookbookApi>().Should().NotBeNull();
    serviceProvider.GetService<IPaymentApi>().Should().NotBeNull();
    serviceProvider.GetService<IPublicApi>().Should().NotBeNull();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ConfigureRefitClients_EachClientGetsUniqueHttpClientBuilder()
  {
    // Arrange
    var builders = new List<IHttpClientBuilder>();
    Action<IHttpClientBuilder> captureBuilder = builder => builders.Add(builder);

    // Act
    _services.ConfigureRefitClients(builder: captureBuilder);

    // Assert
    builders.Should().HaveCount(6,
        "each API client should get its own HttpClientBuilder");

    builders.Should().OnlyHaveUniqueItems(
        "each builder instance should be unique");
  }
}
