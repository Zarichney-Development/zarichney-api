using Zarichney.Config;
using Zarichney.Services.Sessions;

namespace Zarichney.Startup;

/// <summary>
/// Configures middleware components for the application
/// </summary>
public static class MiddlewareStartup
{
  #region Request/Response Logging

  /// <summary>
  /// Adds request/response logging configuration
  /// </summary>
  public static void AddRequestResponseLogger(this IServiceCollection services,
    Action<RequestResponseLoggerOptions>? configureOptions = null)
  {
    if (configureOptions != null)
    {
      services.Configure(configureOptions);
    }
    else
    {
      services.Configure<RequestResponseLoggerOptions>(_ => { });
    }
  }

  #endregion

  #region Session Management

  /// <summary>
  /// Adds session management services
  /// </summary>
  public static void AddSessionManagement(this IServiceCollection services)
  {
    services.AddHostedService<SessionCleanupService>();
    services.AddSingleton<ISessionManager, SessionManager>();
    services.AddSingleton<IScopeFactory, ScopeFactory>();
    services.AddScoped<IScopeContainer>(provider =>
    {
      // Try to get the scope from the AsyncLocal context (background tasks)
      if (ScopeHolder.CurrentScope is not null)
      {
        return ScopeHolder.CurrentScope;
      }

      // Try to get the scope from HttpContext (web requests)
      var httpContext = provider.GetService<IHttpContextAccessor>()?.HttpContext;
      var existingScope = httpContext?.Features.Get<IScopeContainer>();
      return existingScope ??
             // Fall back to creating a new scope if all else fails
             new ScopeContainer(provider);
    });
  }

  /// <summary>
  /// Uses session management middleware
  /// </summary>
  public static void UseSessionManagement(this IApplicationBuilder app)
  {
    app.UseMiddleware<SessionMiddleware>();
  }

  #endregion
}
