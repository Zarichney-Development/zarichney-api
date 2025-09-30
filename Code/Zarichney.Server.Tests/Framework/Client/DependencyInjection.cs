

#nullable enable
namespace Zarichney.Client
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;

    public static partial class IServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureRefitClients(
            this IServiceCollection services, 
            Action<IHttpClientBuilder>? builder = default, 
            RefitSettings? settings = default)
        {
            var clientBuilderIAiApi = services
                .AddRefitClient<IAiApi>(settings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5000"));

            builder?.Invoke(clientBuilderIAiApi);

            var clientBuilderIApiApi = services
                .AddRefitClient<IApiApi>(settings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5000"));

            builder?.Invoke(clientBuilderIApiApi);

            var clientBuilderIAuthApi = services
                .AddRefitClient<IAuthApi>(settings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5000"));

            builder?.Invoke(clientBuilderIAuthApi);

            var clientBuilderICookbookApi = services
                .AddRefitClient<ICookbookApi>(settings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5000"));

            builder?.Invoke(clientBuilderICookbookApi);

            var clientBuilderIPaymentApi = services
                .AddRefitClient<IPaymentApi>(settings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5000"));

            builder?.Invoke(clientBuilderIPaymentApi);

            var clientBuilderIPublicApi = services
                .AddRefitClient<IPublicApi>(settings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5000"));

            builder?.Invoke(clientBuilderIPublicApi);

            return services;
        }
    }
}

