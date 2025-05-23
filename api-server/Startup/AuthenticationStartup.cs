using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Zarichney.Config;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Models;
using Zarichney.Services.Status;

namespace Zarichney.Startup;

/// <summary>
/// Configures authentication, authorization, and identity services
/// </summary>
public static class AuthenticationStartup
{
  /// <summary>
  /// Configures ASP.NET Core Identity and JWT authentication
  /// </summary>
  public static void ConfigureIdentity(WebApplicationBuilder builder)
  {
    // Always register Identity services and roles
    builder.Services.AddIdentityServices(builder.Configuration);
    builder.Services.AddScoped<IRoleManager, RoleManager>();
    builder.Services.AddScoped<RoleManager<IdentityRole>>();

    // Additionally configure mock authentication if needed
    if (ShouldUseMockAuthentication(builder.Environment, builder.Configuration))
    {
      Log.Information("Identity Database unavailable in non-Production environment. Configuring mock authentication.");
      builder.Services.AddMockAuthentication(builder.Configuration);
    }
  }

  /// <summary>
  /// Adds Identity and JWT authentication services
  /// </summary>
  private static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
  {
    // Configure UserDbContext with PostgreSQL
    services.AddDbContext<UserDbContext>(options =>
      options.UseNpgsql(
        configuration.GetConnectionString(UserDbContext.UserDatabaseConnectionName),
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

        byte[] signingKeyBytes;

        // Check if the JWT secret key is missing, empty, or has a placeholder value
        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey == StatusService.PlaceholderMessage)
        {
          // Generate a secure random 32-byte key (256 bits) for the current instance
          var randomKey = new byte[32];
          RandomNumberGenerator.Fill(randomKey);
          signingKeyBytes = randomKey;

          // Create a Base64 representation for logging
          var base64KeyForLog = Convert.ToBase64String(randomKey);

          // Log a warning about the temporary key
          Log.Warning(
            "JwtSettings:SecretKey is missing or invalid in configuration. A temporary, instance-specific key has been generated for development/testing purposes only. " +
            "This key is NOT suitable for production and will cause token validation failures across restarts or multiple instances. " +
            "Set a persistent key using: dotnet user-secrets set \"JwtSettings:SecretKey\" \"YOUR_STRONG_SECRET_KEY\" " +
            "For immediate testing, you can use this generated key: {GeneratedKey}",
            base64KeyForLog);
        }
        else
        {
          // Use the configured secret key
          signingKeyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwtSettings.Issuer,
          ValidAudience = jwtSettings.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
          ClockSkew = TimeSpan.Zero,
          // Explicitly specify that ClaimTypes.Role should be used for role claims
          RoleClaimType = ClaimTypes.Role
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

  /// <summary>
  /// Adds authentication middleware
  /// </summary>
  public static void UseCustomAuthentication(this IApplicationBuilder builder)
  {
    builder.UseAuthentication();
    builder.UseMiddleware<Zarichney.Services.Auth.AuthenticationMiddleware>();
    builder.UseAuthorization();
  }

  /// <summary>
  /// Determines if mock authentication should be used based on environment and explicit configuration
  /// </summary>
  /// <param name="environment">The web host environment</param>
  /// <param name="configuration">The configuration</param>
  /// <returns>True if mock authentication should be used, false otherwise</returns>
  private static bool ShouldUseMockAuthentication(IWebHostEnvironment environment, IConfiguration configuration)
  {
    // Never use mock authentication in Production
    if (environment.EnvironmentName.Equals("Production", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    // Only enable mock authentication if explicitly enabled via configuration
    var mockAuthEnabled = configuration.GetValue<bool>("MockAuth:Enabled", false);
    if (!mockAuthEnabled)
    {
      return false;
    }

    // Additional safety check: only enable if Identity Database connection string is missing or empty
    var connectionString = configuration["ConnectionStrings:" + UserDbContext.UserDatabaseConnectionName];
    return string.IsNullOrEmpty(connectionString);
  }

  /// <summary>
  /// Configures mock authentication services for non-Production environments when Identity DB is unavailable
  /// </summary>
  /// <param name="services">The service collection</param>
  /// <param name="configuration">The configuration</param>
  private static void AddMockAuthentication(this IServiceCollection services, IConfiguration configuration)
  {
    // Configure MockAuth settings
    services.Configure<MockAuthConfig>(configuration.GetSection("MockAuth"));

    // Add mock authentication scheme to existing authentication setup
    // This modifies the existing authentication configuration rather than replacing it
    services.PostConfigure<AuthenticationOptions>(options =>
    {
      // Add MockAuth as an additional scheme
      options.AddScheme<MockAuthHandler>("MockAuth", "MockAuth");

      // Set MockAuth as the default authenticate scheme when Identity DB is unavailable
      options.DefaultAuthenticateScheme = "MockAuth";
      options.DefaultChallengeScheme = "MockAuth";
    });

    // Register the MockAuthHandler
    services.AddSingleton<MockAuthHandler>();

    Log.Information("Mock authentication configured for non-Production environment with unavailable Identity Database.");
  }
}
