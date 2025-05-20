using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
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
    builder.Services.AddIdentityServices(builder.Configuration);

    // Add role manager
    builder.Services.AddScoped<IRoleManager, RoleManager>();

    // Register RoleManager<IdentityRole> if not already registered by AddIdentity
    builder.Services.AddScoped<RoleManager<IdentityRole>>();
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
    builder.UseMiddleware<AuthenticationMiddleware>();
    builder.UseAuthorization();
  }
}
