using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Zarichney.Server.Services.Auth;
using Zarichney.Server.Services.Auth.Models;

namespace Zarichney.Server.Startup;

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