namespace Zarichney.Server.Config;

/// <summary>
/// Marker interface for configuration classes automatically registered by the system
/// </summary>
public interface IConfig;

/// <summary>
/// Helper extension method for service collection 
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Gets a service from the service collection (for use during configuration)
    /// </summary>
    public static T GetService<T>(this IServiceCollection services) where T : class
    {
        return services.BuildServiceProvider().GetRequiredService<T>();
    }
}