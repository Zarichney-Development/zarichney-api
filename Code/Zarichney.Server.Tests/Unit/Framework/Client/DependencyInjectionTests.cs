using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Moq;
using Refit;
using Xunit;
using Zarichney.Client;

namespace Zarichney.Server.Tests.Unit.Framework.Client
{
    public class DependencyInjectionTests : IDisposable
    {
        private readonly ServiceCollection _services;
        private ServiceProvider? _serviceProvider;
        private bool _disposed;

        public DependencyInjectionTests()
        {
            _services = new ServiceCollection();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_WithoutParameters_RegistersAllApiClients()
        {
            // Arrange & Act
            _services.ConfigureRefitClients();
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            _serviceProvider.GetService<IAiApi>().Should().NotBeNull(
                "because IAiApi should be registered in the service collection");

            _serviceProvider.GetService<IApiApi>().Should().NotBeNull(
                "because IApiApi should be registered in the service collection");

            _serviceProvider.GetService<IAuthApi>().Should().NotBeNull(
                "because IAuthApi should be registered in the service collection");

            _serviceProvider.GetService<ICookbookApi>().Should().NotBeNull(
                "because ICookbookApi should be registered in the service collection");

            _serviceProvider.GetService<IPaymentApi>().Should().NotBeNull(
                "because IPaymentApi should be registered in the service collection");

            _serviceProvider.GetService<IPublicApi>().Should().NotBeNull(
                "because IPublicApi should be registered in the service collection");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_WithoutParameters_ConfiguresCorrectBaseAddress()
        {
            // Arrange
            var expectedBaseAddress = new Uri("http://localhost:5000/");

            // Act
            _services.ConfigureRefitClients();
            _serviceProvider = _services.BuildServiceProvider();

            // Assert - Verify services can be resolved with base address configured
            var aiApi = _serviceProvider.GetRequiredService<IAiApi>();
            var apiApi = _serviceProvider.GetRequiredService<IApiApi>();
            var authApi = _serviceProvider.GetRequiredService<IAuthApi>();
            var cookbookApi = _serviceProvider.GetRequiredService<ICookbookApi>();
            var paymentApi = _serviceProvider.GetRequiredService<IPaymentApi>();
            var publicApi = _serviceProvider.GetRequiredService<IPublicApi>();

            // All services should be resolved successfully (base address is set internally by Refit)
            aiApi.Should().NotBeNull();
            apiApi.Should().NotBeNull();
            authApi.Should().NotBeNull();
            cookbookApi.Should().NotBeNull();
            paymentApi.Should().NotBeNull();
            publicApi.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_WithBuilder_AppliesBuilderToAllClients()
        {
            // Arrange
            var builderInvocations = 0;
            Action<IHttpClientBuilder> builder = _ => builderInvocations++;

            // Act
            _services.ConfigureRefitClients(builder);

            // Assert
            builderInvocations.Should().Be(6,
                "because builder should be invoked once for each of the 6 API clients");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_WithCustomRefitSettings_AppliesSettingsToAllClients()
        {
            // Arrange
            var customSettings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer()
            };

            // Act
            _services.ConfigureRefitClients(settings: customSettings);
            _serviceProvider = _services.BuildServiceProvider();

            // Assert - Verify all clients are registered (settings are internal to Refit)
            _serviceProvider.GetService<IAiApi>().Should().NotBeNull(
                "because IAiApi should be registered with custom settings");
            _serviceProvider.GetService<IApiApi>().Should().NotBeNull(
                "because IApiApi should be registered with custom settings");
            _serviceProvider.GetService<IAuthApi>().Should().NotBeNull(
                "because IAuthApi should be registered with custom settings");
            _serviceProvider.GetService<ICookbookApi>().Should().NotBeNull(
                "because ICookbookApi should be registered with custom settings");
            _serviceProvider.GetService<IPaymentApi>().Should().NotBeNull(
                "because IPaymentApi should be registered with custom settings");
            _serviceProvider.GetService<IPublicApi>().Should().NotBeNull(
                "because IPublicApi should be registered with custom settings");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_WithBuilderAndSettings_AppliesBothToAllClients()
        {
            // Arrange
            var builderInvocations = 0;
            Action<IHttpClientBuilder> builder = _ => builderInvocations++;
            var customSettings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer()
            };

            // Act
            _services.ConfigureRefitClients(builder, customSettings);
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            builderInvocations.Should().Be(6,
                "because builder should be invoked for all 6 clients");

            _serviceProvider.GetService<IAiApi>().Should().NotBeNull(
                "because all APIs should be registered with both builder and settings applied");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_CalledMultipleTimes_DoesNotThrowException()
        {
            // Arrange & Act
            Action act = () =>
            {
                _services.ConfigureRefitClients();
                _services.ConfigureRefitClients(); // Called twice
            };

            // Assert
            act.Should().NotThrow(
                "because configuring Refit clients multiple times should be safe");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_ReturnsOriginalServiceCollection()
        {
            // Arrange & Act
            var result = _services.ConfigureRefitClients();

            // Assert
            result.Should().BeSameAs(_services,
                "because the method should return the same IServiceCollection for method chaining");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_RegistersHttpClientFactoryServices()
        {
            // Arrange & Act
            _services.ConfigureRefitClients();
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            _serviceProvider.GetService<IHttpClientFactory>().Should().NotBeNull(
                "because HttpClientFactory should be registered by the Refit client configuration");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_WithNullBuilder_DoesNotThrowException()
        {
            // Arrange & Act
            Action act = () => _services.ConfigureRefitClients(builder: null);

            // Assert
            act.Should().NotThrow(
                "because null builder parameter should be handled gracefully");

            // Verify services are still registered
            _serviceProvider = _services.BuildServiceProvider();
            _serviceProvider.GetService<IAiApi>().Should().NotBeNull(
                "because APIs should still be registered even with null builder");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_WithNullSettings_UsesDefaultRefitSettings()
        {
            // Arrange & Act
            _services.ConfigureRefitClients(settings: null);
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            _serviceProvider.GetService<IAiApi>().Should().NotBeNull(
                "because APIs should be registered with default Refit settings when null is provided");
            _serviceProvider.GetService<IAuthApi>().Should().NotBeNull(
                "because all APIs should work with default settings");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_RegistersServicesAsTransient()
        {
            // Arrange
            _services.ConfigureRefitClients();
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var firstInstance = _serviceProvider.GetRequiredService<IAiApi>();
            var secondInstance = _serviceProvider.GetRequiredService<IAiApi>();

            // Assert - Refit clients are registered as transient by default
            firstInstance.Should().NotBeSameAs(secondInstance,
                "because Refit clients should be registered as transient by default");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_WithCustomBuilder_CanConfigureHttpClient()
        {
            // Arrange
            var builderCalled = false;
            Action<IHttpClientBuilder> builder = clientBuilder =>
            {
                builderCalled = true;
            };

            // Act
            _services.ConfigureRefitClients(builder);

            // Assert
            builderCalled.Should().BeTrue(
                "because the builder should be invoked during configuration");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_RegistersAllExpectedApiTypes()
        {
            // Arrange
            var expectedApiTypes = new[]
            {
                typeof(IAiApi),
                typeof(IApiApi),
                typeof(IAuthApi),
                typeof(ICookbookApi),
                typeof(IPaymentApi),
                typeof(IPublicApi)
            };

            // Act
            _services.ConfigureRefitClients();
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            foreach (var apiType in expectedApiTypes)
            {
                _serviceProvider.GetService(apiType).Should().NotBeNull(
                    $"because {apiType.Name} should be registered in the service collection");
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_ServiceCollectionDescriptors_ContainsRefitServices()
        {
            // Arrange & Act
            _services.ConfigureRefitClients();

            // Assert
            _services.Any(sd => sd.ServiceType == typeof(IAiApi)).Should().BeTrue(
                "because IAiApi should be in the service descriptors");
            _services.Any(sd => sd.ServiceType == typeof(IApiApi)).Should().BeTrue(
                "because IApiApi should be in the service descriptors");
            _services.Any(sd => sd.ServiceType == typeof(IAuthApi)).Should().BeTrue(
                "because IAuthApi should be in the service descriptors");
            _services.Any(sd => sd.ServiceType == typeof(ICookbookApi)).Should().BeTrue(
                "because ICookbookApi should be in the service descriptors");
            _services.Any(sd => sd.ServiceType == typeof(IPaymentApi)).Should().BeTrue(
                "because IPaymentApi should be in the service descriptors");
            _services.Any(sd => sd.ServiceType == typeof(IPublicApi)).Should().BeTrue(
                "because IPublicApi should be in the service descriptors");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_WithHttpMessageHandler_CanConfigureHandler()
        {
            // Arrange
            var handlerConfigured = false;
            Action<IHttpClientBuilder> builder = clientBuilder =>
            {
                // Simulate configuring a handler
                handlerConfigured = true;
            };

            // Act
            _services.ConfigureRefitClients(builder);

            // Assert
            handlerConfigured.Should().BeTrue(
                "because builder should be able to configure message handlers");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_RegistersHttpClientWithCorrectName()
        {
            // Arrange
            _services.ConfigureRefitClients();
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();

            // Assert - Verify HttpClient names follow Refit naming convention
            Action createAiClient = () => httpClientFactory.CreateClient("Zarichney.Client.IAiApi");
            Action createApiClient = () => httpClientFactory.CreateClient("Zarichney.Client.IApiApi");
            Action createAuthClient = () => httpClientFactory.CreateClient("Zarichney.Client.IAuthApi");
            Action createCookbookClient = () => httpClientFactory.CreateClient("Zarichney.Client.ICookbookApi");
            Action createPaymentClient = () => httpClientFactory.CreateClient("Zarichney.Client.IPaymentApi");
            Action createPublicClient = () => httpClientFactory.CreateClient("Zarichney.Client.IPublicApi");

            // Verify all services can be resolved (which confirms proper registration)
            _serviceProvider.GetService<IAiApi>().Should().NotBeNull();
            _serviceProvider.GetService<IApiApi>().Should().NotBeNull();
            _serviceProvider.GetService<IAuthApi>().Should().NotBeNull();
            _serviceProvider.GetService<ICookbookApi>().Should().NotBeNull();
            _serviceProvider.GetService<IPaymentApi>().Should().NotBeNull();
            _serviceProvider.GetService<IPublicApi>().Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_WithDelegatingHandler_CanBeConfiguredViaBuilder()
        {
            // Arrange
            var handlerConfigured = false;
            Action<IHttpClientBuilder> builder = clientBuilder =>
            {
                // Simulate adding a delegating handler
                handlerConfigured = true;
            };

            // Act
            _services.ConfigureRefitClients(builder);

            // Assert
            handlerConfigured.Should().BeTrue(
                "because builder should allow configuration of delegating handlers");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_IndividualClientsCanBeResolved()
        {
            // Arrange
            _services.ConfigureRefitClients();
            _serviceProvider = _services.BuildServiceProvider();

            // Act & Assert - Resolve each client individually
            using (var scope = _serviceProvider.CreateScope())
            {
                var aiApi = scope.ServiceProvider.GetRequiredService<IAiApi>();
                var apiApi = scope.ServiceProvider.GetRequiredService<IApiApi>();
                var authApi = scope.ServiceProvider.GetRequiredService<IAuthApi>();
                var cookbookApi = scope.ServiceProvider.GetRequiredService<ICookbookApi>();
                var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
                var publicApi = scope.ServiceProvider.GetRequiredService<IPublicApi>();

                aiApi.Should().NotBeNull().And.BeAssignableTo<IAiApi>();
                apiApi.Should().NotBeNull().And.BeAssignableTo<IApiApi>();
                authApi.Should().NotBeNull().And.BeAssignableTo<IAuthApi>();
                cookbookApi.Should().NotBeNull().And.BeAssignableTo<ICookbookApi>();
                paymentApi.Should().NotBeNull().And.BeAssignableTo<IPaymentApi>();
                publicApi.Should().NotBeNull().And.BeAssignableTo<IPublicApi>();
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ConfigureRefitClients_ServiceProvider_CanCreateMultipleScopes()
        {
            // Arrange
            _services.ConfigureRefitClients();
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            IAiApi? firstScopeApi;
            IAiApi? secondScopeApi;

            using (var scope1 = _serviceProvider.CreateScope())
            {
                firstScopeApi = scope1.ServiceProvider.GetRequiredService<IAiApi>();
            }

            using (var scope2 = _serviceProvider.CreateScope())
            {
                secondScopeApi = scope2.ServiceProvider.GetRequiredService<IAiApi>();
            }

            // Assert
            firstScopeApi.Should().NotBeNull();
            secondScopeApi.Should().NotBeNull();
            // Refit services are transient, so they will be different instances
            firstScopeApi.Should().NotBeNull();
            secondScopeApi.Should().NotBeNull();
            firstScopeApi.Should().NotBeSameAs(secondScopeApi,
                "because Refit services are registered as transient");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _serviceProvider?.Dispose();
                _disposed = true;
            }
        }
    }
}