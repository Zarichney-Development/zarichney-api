using System.Collections.Immutable;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Zarichney.Server.Auth;

public class ApiKeyConfig
{
  public string AllowedKeys { get; set; } = string.Empty;

  private ImmutableHashSet<string>? _validApiKeys;

  public ImmutableHashSet<string> ValidApiKeys
  {
    get
    {
      return _validApiKeys ??= AllowedKeys
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToImmutableHashSet();
    }
  }
}

public class JwtSettings
{
  public string SecretKey { get; init; } = string.Empty;
  public string Issuer { get; init; } = string.Empty;
  public string Audience { get; init; } = string.Empty;
  public int ExpiryMinutes { get; init; } = 60;
  public int RefreshTokenExpiryDays { get; init; } = 30;
}

public static class MiddlewareConfiguration
{
  public static class Routes
  {
    // Explicitly define all bypass paths
    private static readonly HashSet<string> ExactBypassPaths = new(StringComparer.OrdinalIgnoreCase)
    {
      "/api/health",
      "/api/key/validate"
    };

    private static readonly HashSet<string> PrefixBypassPaths = new(StringComparer.OrdinalIgnoreCase)
    {
      "/api/swagger",
      "/api/auth"
    };

    public static bool ShouldBypass(string path)
    {
      return ExactBypassPaths.Contains(path) ||
             PrefixBypassPaths.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
  }
}

public static class AuthStartupExtensions
{
  public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
  {
    // Configure UserDbContext with PostgreSQL
    services.AddDbContext<UserDbContext>(options =>
      options.UseNpgsql(
        configuration.GetConnectionString("IdentityConnection"),
        b => b.MigrationsAssembly("Zarichney")
      ));

    // Configure Identity
    services.AddIdentity<ApplicationUser, IdentityRole>(options =>
      {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 8;

        // User settings
        options.User.RequireUniqueEmail = true;
      })
      .AddEntityFrameworkStores<UserDbContext>()
      .AddDefaultTokenProviders();

    // Configure JWT
    var jwtSettings = new JwtSettings();
    configuration.GetSection("JwtSettings").Bind(jwtSettings);
    services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

    services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        options.SaveToken = true;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwtSettings.Issuer,
          ValidAudience = jwtSettings.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
          ClockSkew = TimeSpan.Zero
        };
        
        // Configure to get token from the cookie
        options.Events = new JwtBearerEvents
        {
          OnMessageReceived = context =>
          {
            // Try to get the token from the cookie
            context.Token = context.Request.Cookies["AuthAccessToken"];
            return Task.CompletedTask;
          }
        };
      });
  }

  public static void UseApiKeyAuth(this IApplicationBuilder builder)
  {
    builder.UseMiddleware<ApiKeyAuthMiddleware>();
  }
}