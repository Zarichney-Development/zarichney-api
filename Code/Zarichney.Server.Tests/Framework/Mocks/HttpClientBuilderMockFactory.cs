using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Moq;

namespace Zarichney.Server.Tests.Framework.Mocks;

/// <summary>
/// Factory for creating mock IHttpClientBuilder instances for testing.
/// Provides various configurations for different test scenarios.
/// </summary>
public static class HttpClientBuilderMockFactory
{
    /// <summary>
    /// Creates a basic mock HttpClientBuilder with minimal configuration.
    /// </summary>
    public static Mock<IHttpClientBuilder> CreateDefault()
    {
        var mock = new Mock<IHttpClientBuilder>();
        var services = new ServiceCollection();

        mock.Setup(x => x.Services).Returns(services);
        mock.Setup(x => x.Name).Returns("TestHttpClient");

        return mock;
    }

    /// <summary>
    /// Creates a mock HttpClientBuilder with a specific name.
    /// </summary>
    public static Mock<IHttpClientBuilder> CreateWithName(string name)
    {
        var mock = CreateDefault();
        mock.Setup(x => x.Name).Returns(name);

        return mock;
    }

    /// <summary>
    /// Creates a mock HttpClientBuilder that tracks configuration actions.
    /// </summary>
    public static Mock<IHttpClientBuilder> CreateWithTracking(
        out List<Action<HttpClient>> configureActions,
        out List<Func<IServiceProvider, HttpMessageHandler>> handlerFactories)
    {
        var mock = CreateDefault();
        var capturedConfigureActions = new List<Action<HttpClient>>();
        var capturedHandlerFactories = new List<Func<IServiceProvider, HttpMessageHandler>>();

        mock.Setup(x => x.ConfigureHttpClient(It.IsAny<Action<HttpClient>>()))
            .Callback<Action<HttpClient>>(action => capturedConfigureActions.Add(action))
            .Returns(mock.Object);

        mock.Setup(x => x.ConfigurePrimaryHttpMessageHandler(It.IsAny<Func<HttpMessageHandler>>()))
            .Callback<Func<HttpMessageHandler>>(factory =>
                capturedHandlerFactories.Add(_ => factory()))
            .Returns(mock.Object);

        configureActions = capturedConfigureActions;
        handlerFactories = capturedHandlerFactories;

        return mock;
    }

    /// <summary>
    /// Creates a mock HttpClientBuilder configured for a specific base address.
    /// </summary>
    public static Mock<IHttpClientBuilder> CreateForBaseAddress(Uri baseAddress)
    {
        var mock = CreateDefault();

        mock.Setup(x => x.ConfigureHttpClient(It.IsAny<Action<HttpClient>>()))
            .Callback<Action<HttpClient>>(action =>
            {
                var httpClient = new HttpClient();
                action(httpClient);
                // Verify base address was set
                if (httpClient.BaseAddress != baseAddress)
                {
                    throw new InvalidOperationException(
                        $"Expected base address {baseAddress} but got {httpClient.BaseAddress}");
                }
            })
            .Returns(mock.Object);

        return mock;
    }

    /// <summary>
    /// Creates a mock HttpClientBuilder that simulates adding message handlers.
    /// </summary>
    public static Mock<IHttpClientBuilder> CreateWithHandlerChain()
    {
        var mock = CreateDefault();
        var handlerChain = new List<Type>();

        // Note: AddHttpMessageHandler<T> requires T : DelegatingHandler constraint
        // We cannot use It.IsAnyType here, so we simulate handler addition differently
        mock.Setup(x => x.Services)
            .Returns(() =>
            {
                var services = new ServiceCollection();
                foreach (var handlerType in handlerChain)
                {
                    services.AddTransient(handlerType);
                }
                return services;
            });

        mock.Setup(x => x.ConfigurePrimaryHttpMessageHandler(It.IsAny<Func<HttpMessageHandler>>()))
            .Returns(mock.Object);

        mock.SetupGet(x => x.Services)
            .Returns(() =>
            {
                var services = new ServiceCollection();
                // Simulate handler registration
                foreach (var handlerType in handlerChain)
                {
                    services.AddTransient(handlerType);
                }
                return services;
            });

        return mock;
    }

    /// <summary>
    /// Creates a mock HttpClientBuilder that throws an exception when configured.
    /// </summary>
    public static Mock<IHttpClientBuilder> CreateFaulty(Exception exception)
    {
        var mock = new Mock<IHttpClientBuilder>();

        mock.Setup(x => x.Services).Throws(exception);
        mock.Setup(x => x.ConfigureHttpClient(It.IsAny<Action<HttpClient>>())).Throws(exception);

        return mock;
    }

    /// <summary>
    /// Creates a mock HttpClientBuilder with custom service collection.
    /// </summary>
    public static Mock<IHttpClientBuilder> CreateWithServices(IServiceCollection services)
    {
        var mock = new Mock<IHttpClientBuilder>();

        mock.Setup(x => x.Services).Returns(services);
        mock.Setup(x => x.Name).Returns("CustomServicesHttpClient");
        mock.Setup(x => x.ConfigureHttpClient(It.IsAny<Action<HttpClient>>()))
            .Returns(mock.Object);

        return mock;
    }

    /// <summary>
    /// Creates a verifiable mock HttpClientBuilder for strict testing.
    /// </summary>
    public static Mock<IHttpClientBuilder> CreateVerifiable()
    {
        var mock = new Mock<IHttpClientBuilder>(MockBehavior.Strict);
        var services = new ServiceCollection();

        mock.Setup(x => x.Services).Returns(services).Verifiable();
        mock.Setup(x => x.Name).Returns("VerifiableHttpClient").Verifiable();
        mock.Setup(x => x.ConfigureHttpClient(It.IsAny<Action<HttpClient>>()))
            .Returns(mock.Object)
            .Verifiable();

        return mock;
    }

    /// <summary>
    /// Creates a mock HttpClientBuilder that captures all configuration calls.
    /// </summary>
    public static Mock<IHttpClientBuilder> CreateWithFullCapture(
        out HttpClientBuilderCaptureContext captureContext)
    {
        var mock = CreateDefault();
        var context = new HttpClientBuilderCaptureContext();

        mock.Setup(x => x.ConfigureHttpClient(It.IsAny<Action<HttpClient>>()))
            .Callback<Action<HttpClient>>(action => context.ConfigureClientActions.Add(action))
            .Returns(mock.Object);

        mock.Setup(x => x.ConfigureHttpClient(It.IsAny<Action<IServiceProvider, HttpClient>>()))
            .Callback<Action<IServiceProvider, HttpClient>>(action =>
                context.ConfigureClientWithProviderActions.Add(action))
            .Returns(mock.Object);

        mock.Setup(x => x.ConfigurePrimaryHttpMessageHandler(It.IsAny<Func<HttpMessageHandler>>()))
            .Callback<Func<HttpMessageHandler>>(factory =>
                context.PrimaryHandlerFactories.Add(factory))
            .Returns(mock.Object);

        // Note: Cannot mock AddHttpMessageHandler<T> with It.IsAnyType due to generic constraints
        // Track handler additions through alternate mechanism if needed

        captureContext = context;
        return mock;
    }

    /// <summary>
    /// Context class for capturing HttpClientBuilder configuration calls.
    /// </summary>
    public class HttpClientBuilderCaptureContext
    {
        public List<Action<HttpClient>> ConfigureClientActions { get; } = new();
        public List<Action<IServiceProvider, HttpClient>> ConfigureClientWithProviderActions { get; } = new();
        public List<Func<HttpMessageHandler>> PrimaryHandlerFactories { get; } = new();
        public List<Type> MessageHandlerTypes { get; } = new();

        /// <summary>
        /// Gets the total number of configuration calls made.
        /// </summary>
        public int TotalConfigurationCalls =>
            ConfigureClientActions.Count +
            ConfigureClientWithProviderActions.Count +
            PrimaryHandlerFactories.Count +
            MessageHandlerTypes.Count;

        /// <summary>
        /// Resets all captured data.
        /// </summary>
        public void Reset()
        {
            ConfigureClientActions.Clear();
            ConfigureClientWithProviderActions.Clear();
            PrimaryHandlerFactories.Clear();
            MessageHandlerTypes.Clear();
        }
    }
}