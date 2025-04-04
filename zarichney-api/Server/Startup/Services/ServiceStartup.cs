using Azure.Identity;
using Microsoft.Graph;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.OpenApi.Models;
using OpenAI;
using OpenAI.Audio;
using Zarichney.Server.Auth;
using Zarichney.Server.Config;
using Zarichney.Server.Cookbook.Customers;
using Zarichney.Server.Cookbook.Orders;
using Zarichney.Server.Cookbook.Recipes;
using Zarichney.Server.Services.AI;
using Zarichney.Server.Services.Auth;
using Zarichney.Server.Services.BackgroundTasks;
using Zarichney.Server.Services.Emails;
using Zarichney.Server.Services.FileSystem;
using Zarichney.Server.Services.GitHub;
using Zarichney.Server.Services.Payment;
using Zarichney.Server.Services.PdfGeneration;
using Zarichney.Server.Services.Web;

namespace Zarichney.Server.Startup.Services;

/// <summary>
/// Configures application services and dependencies
/// </summary>
public static class ServiceStartup
{
    /// <summary>
    /// Configures core application services
    /// </summary>
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.RegisterConfigurationServices(builder.Configuration);
        services.AddSingleton(Serilog.Log.Logger);

        if (builder.Environment.IsProduction())
        {
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status302Found;
                options.HttpsPort = 443;
            });
        }

        services.AddHttpContextAccessor();
        services.AddControllers()
          .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonTypeConverter()); });
        services.AddAutoMapper(typeof(Program));
        services.AddMemoryCache();
        services.AddSessionManagement();

        ConfigureEmailServices(services);
        ConfigureOpenAiServices(services);
        ConfigureApplicationServices(services);
    }

    /// <summary>
    /// Configures email services using Microsoft Graph
    /// </summary>
    public static void ConfigureEmailServices(IServiceCollection services)
    {
        var emailConfig = services.GetService<EmailConfig>();
        var graphClient = new GraphServiceClient(
          new ClientSecretCredential(
            emailConfig.AzureTenantId,
            emailConfig.AzureAppId,
            emailConfig.AzureAppSecret,
            new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            }
          ),
          ["https://graph.microsoft.com/.default"]
        );
        services.AddSingleton(graphClient);
    }

    /// <summary>
    /// Configures OpenAI services and dependencies
    /// </summary>
    public static void ConfigureOpenAiServices(IServiceCollection services)
    {
        var llmConfig = services.GetService<LlmConfig>();
        var apiKey = llmConfig.ApiKey ??
                     throw new InvalidConfigurationException("Missing required configuration for OpenAI API key.");

        services.AddSingleton(new OpenAIClient(apiKey));
        services.AddSingleton(sp => new AudioClient(sp.GetRequiredService<TranscribeConfig>().ModelName, apiKey));
    }

    /// <summary>
    /// Configures application-specific services
    /// </summary>
    public static void ConfigureApplicationServices(IServiceCollection services)
    {
        // Background Services
        services.AddHostedService<BackgroundTaskService>();
        services.AddHostedService<RefreshTokenCleanupService>();
        services.AddHostedService<RoleInitializer>();
        services.AddSingleton<IBackgroundWorker>(_ => new BackgroundWorker(100));

        // Prompts and Core Services
        services.AddPrompts(typeof(PromptBase).Assembly);
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<ITemplateService, TemplateService>();
        services.AddSingleton<IBrowserService, BrowserService>();

        // Repositories
        services.AddSingleton<ILlmRepository, LlmRepository>();
        services.AddSingleton<IRecipeRepository, RecipeFileRepository>();
        services.AddSingleton<IRecipeIndexer, RecipeIndexer>();
        services.AddSingleton<IRecipeSearcher, RecipeSearcher>();
        services.AddSingleton<IOrderRepository, OrderFileRepository>();
        services.AddSingleton<ICustomerRepository, CustomerFileRepository>();
        services.AddSingleton<GitHubService>();
        services.AddSingleton<IGitHubService>(sp => sp.GetRequiredService<GitHubService>());
        services.AddHostedService(sp => sp.GetRequiredService<GitHubService>());

        // Scoped and Transient Services
        services.AddScoped<ICookieAuthManager, CookieAuthManager>();
        services.AddScoped<ILlmService, LlmService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddTransient<IRecipeService, RecipeService>();
        services.AddTransient<IOrderService, OrderService>();
        services.AddTransient<ICustomerService, CustomerService>();
        services.AddTransient<WebScraperService>();
        services.AddTransient<PdfCompiler>();
        services.AddTransient<ITranscribeService, TranscribeService>();
        services.AddTransient<IStripeService, StripeService>();
        services.AddTransient<IPaymentService, PaymentService>();

        // Misc Utilities
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    }

    /// <summary>
    /// Configures Swagger/OpenAPI documentation
    /// </summary>
    public static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("swagger", new OpenApiInfo
            {
                Title = "Zarichney API",
                Version = "v1",
                Description = "API for the Cookbook application and Transcription Service. " +
                              "Authenticate using the 'Authorize' button and provide your API key."
            });

            c.OperationFilter<FormFileOperationFilter>();
            c.EnableAnnotations();

            var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-Api-Key",
                Description = "API key authentication. Enter your API key here.",
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}