namespace Zarichney.Server.Services.Sessions;

/// <summary>
/// Extension methods for registering session management services
/// </summary>
public static class ServiceCollectionExtensions
{
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
}

/// <summary>
/// Extension methods for adding session middleware to the application pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
  public static void UseSessionManagement(this IApplicationBuilder app)
  {
    app.UseMiddleware<SessionMiddleware>();
  }
}