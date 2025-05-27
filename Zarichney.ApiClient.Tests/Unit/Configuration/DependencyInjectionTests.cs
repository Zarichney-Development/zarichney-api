using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Xunit;
using Zarichney.ApiClient.Configuration;
using Zarichney.ApiClient.Interfaces;

namespace Zarichney.ApiClient.Tests.Unit.Configuration;

/// <summary>
/// Tests for the dependency injection configuration of API clients.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public void ConfigureRefitClients_ShouldRegisterAllApiClients()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.ConfigureRefitClients();
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        serviceProvider.GetService<IAiApi>().Should().NotBeNull();
        serviceProvider.GetService<IApiApi>().Should().NotBeNull();
        serviceProvider.GetService<IAuthApi>().Should().NotBeNull();
        serviceProvider.GetService<ICookbookApi>().Should().NotBeNull();
        serviceProvider.GetService<IPaymentApi>().Should().NotBeNull();
        serviceProvider.GetService<IPublicApi>().Should().NotBeNull();
    }
    
    [Fact]
    public void ConfigureRefitClients_WithCustomBuilder_ShouldApplyCustomConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var customBaseUrl = "https://custom.example.com";
        var builderWasCalled = false;
        
        // Act
        services.ConfigureRefitClients(builder => 
        {
            builder.ConfigureHttpClient(c => c.BaseAddress = new Uri(customBaseUrl));
            builderWasCalled = true;
        });
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        builderWasCalled.Should().BeTrue("the custom builder should have been called");
        
        // Verify that at least one client can be created
        var aiApi = serviceProvider.GetService<IAiApi>();
        aiApi.Should().NotBeNull();
    }
}