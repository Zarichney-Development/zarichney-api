using System.Text.Json.Serialization;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph;
using Microsoft.OpenApi.Models;
using OpenAI;
using OpenAI.Audio;
using Zarichney.Config;
using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services.AI;
using Zarichney.Services.Auth;
using Zarichney.Services.BackgroundTasks;
using Zarichney.Services.Email;
using Zarichney.Services.FileSystem;
using Zarichney.Services.GitHub;
using Zarichney.Services.Payment;
using Zarichney.Services.PdfGeneration;
using Zarichney.Services.Status;
using Zarichney.Services.Status.Proxies;
using Zarichney.Services.ProcessExecution;
using Zarichney.Services.Web;
using Zarichney.Services.Logging;
using Zarichney.Services.Security;

namespace Zarichney.Startup;

/// <summary>
/// Configures application services and dependencies
/// </summary>
public class ServiceStartup
{
  /// <summary>
  /// Configures core application services
  /// </summary>
  public static void ConfigureServices(WebApplicationBuilder builder)
  {
    var services = builder.Services;

    services.RegisterConfigurationServices(builder.Configuration, builder.Environment);
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
      .AddJsonOptions(options =>
      {
        options.JsonSerializerOptions.Converters.Add(new JsonTypeConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
      });
    services.AddAutoMapper(typeof(Program));
    services.AddMemoryCache();
    services.AddSessionManagement();
    services.AddRequestResponseLogger();

    ConfigureEmailServices(services);
    ConfigureOpenAiServices(services);
    ConfigureApplicationServices(services);
  }

  /// <summary>
  /// Configures email services using Microsoft Graph
  /// </summary>
  private static void ConfigureEmailServices(IServiceCollection services)
  {
    // Register MailCheckClient
    services.AddScoped<IMailCheckClient>(sp =>
    {
      var emailConfig = sp.GetRequiredService<EmailConfig>();
      var logger = sp.GetRequiredService<ILogger<ServiceStartup>>();

      // Check if MailCheck API key is available
      if (string.IsNullOrEmpty(emailConfig.MailCheckApiKey) ||
          emailConfig.MailCheckApiKey == StatusService.PlaceholderMessage)
      {
        logger.LogWarning("Email validation service is unavailable due to missing MailCheck API key");
        var reasons = new List<string> { "Missing MailCheck API key" };
        // Return a proxy that throws ServiceUnavailableException when methods are called
        return new MailCheckClientProxy(reasons);
      }

      return new MailCheckClient(emailConfig, sp.GetRequiredService<IMemoryCache>(),
        sp.GetRequiredService<ILogger<MailCheckClient>>());
    });

    // Register GraphServiceClient
    services.AddScoped<GraphServiceClient>(sp =>
    {
      var emailConfig = sp.GetRequiredService<EmailConfig>();
      var logger = sp.GetRequiredService<ILogger<ServiceStartup>>();
      var statusService = sp.GetRequiredService<IStatusService>();

      var serviceStatus = statusService.GetServiceStatusAsync().GetAwaiter().GetResult();
      var emailServiceAvailable =
        serviceStatus.TryGetValue(ExternalServices.MsGraph, out var status) && status.IsAvailable;

      if (!emailServiceAvailable)
      {
        logger.LogWarning("Email service is unavailable due to missing configuration: {MissingConfigs}",
          status?.MissingConfigurations == null ? "unknown" : string.Join(", ", status.MissingConfigurations));

        // Return a proxy that throws ServiceUnavailableException when methods are called
        return new GraphServiceClientProxy(status?.MissingConfigurations.ToList());
      }

      try
      {
        return new GraphServiceClient(
          new ClientSecretCredential(
            emailConfig.AzureTenantId,
            emailConfig.AzureAppId,
            emailConfig.AzureAppSecret,
            new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud }
          ),
          ["https://graph.microsoft.com/.default"]
        );
      }
      catch (Exception ex)
      {
        logger.LogWarning(ex, "Failed to create GraphServiceClient. Email functionality will not be available.");
        var errorMessage = $"Failed to create GraphServiceClient: {ex.Message}";
        var reasons = new List<string> { errorMessage };
        return new GraphServiceClientProxy(reasons);
      }
    });
  }

  /// <summary>
  /// Configures OpenAI services and dependencies
  /// </summary>
  private static void ConfigureOpenAiServices(IServiceCollection services)
  {
    // Add OpenAIClient with factory method
    services.AddScoped<OpenAIClient>(sp =>
    {
      var llmConfig = sp.GetRequiredService<LlmConfig>();
      var logger = sp.GetRequiredService<ILogger<ServiceStartup>>();
      var statusService = sp.GetRequiredService<IStatusService>();

      var serviceStatus = statusService.GetServiceStatusAsync().GetAwaiter().GetResult();
      var llmServiceAvailable =
        serviceStatus.TryGetValue(ExternalServices.OpenAiApi, out var status) && status.IsAvailable;

      if (!llmServiceAvailable)
      {
        logger.LogWarning("LLM service is unavailable due to missing configuration: {MissingConfigs}",
          status?.MissingConfigurations == null ? "unknown" : string.Join(", ", status.MissingConfigurations));

        // Return a proxy that throws ServiceUnavailableException when methods are called
        return new OpenAIClientProxy(status?.MissingConfigurations.ToList());
      }

      try
      {
        return new OpenAIClient(llmConfig.ApiKey);
      }
      catch (Exception ex)
      {
        logger.LogWarning(ex, "Failed to create OpenAIClient. LLM functionality will not be available.");
        var errorMessage = $"Failed to create OpenAIClient: {ex.Message}";
        var reasons = new List<string> { errorMessage };
        return new OpenAIClientProxy(reasons);
      }
    });

    // Add AudioClient with factory method
    services.AddScoped<AudioClient>(sp =>
    {
      var transcribeConfig = sp.GetRequiredService<TranscribeConfig>();
      var llmConfig = sp.GetRequiredService<LlmConfig>();
      var logger = sp.GetRequiredService<ILogger<ServiceStartup>>();
      var statusService = sp.GetRequiredService<IStatusService>();

      var serviceStatus = statusService.GetServiceStatusAsync().GetAwaiter().GetResult();
      var llmServiceAvailable =
        serviceStatus.TryGetValue(ExternalServices.OpenAiApi, out var status) && status.IsAvailable;

      if (!llmServiceAvailable)
      {
        logger.LogWarning("Audio transcription service is unavailable due to missing configuration: {MissingConfigs}",
          status?.MissingConfigurations == null ? "unknown" : string.Join(", ", status.MissingConfigurations));

        // Return a proxy that throws ServiceUnavailableException when methods are called
        return new AudioClientProxy(status?.MissingConfigurations.ToList());
      }

      try
      {
        return new AudioClient(transcribeConfig.ModelName, llmConfig.ApiKey);
      }
      catch (Exception ex)
      {
        logger.LogWarning(ex, "Failed to create AudioClient. Audio transcription functionality will not be available.");
        var errorMessage = $"Failed to create AudioClient: {ex.Message}";
        var reasons = new List<string> { errorMessage };
        return new AudioClientProxy(reasons);
      }
    });
  }

  /// <summary>
  /// Configures application-specific services
  /// </summary>
  private static void ConfigureApplicationServices(IServiceCollection services)
  {
    // Background Services
    services.AddHostedService<BackgroundTaskService>();
    services.AddHostedService<RefreshTokenCleanupService>();
    services.AddHostedService<RoleInitializer>();
    services.AddSingleton<IBackgroundWorker>(_ => new BackgroundWorker(100));

    // Prompts and Core Services
    services.AddPrompts(typeof(PromptBase).Assembly);
    services.AddSingleton<IFileService, FileService>();
    services.AddSingleton<IFileWriteQueueService, FileWriteQueueService>();
    services.AddSingleton<ITemplateService, TemplateService>();
    services.AddSingleton<IBrowserService, BrowserService>();

    // Status Service - provides configuration and feature availability status
    services.AddSingleton<IStatusService, StatusService>();

    // System Services
    services.AddScoped<IProcessExecutor, ProcessExecutor>();
    
    // Security Services - provides SSRF protection and URL validation
    services.AddHttpClient<IOutboundUrlSecurity, OutboundUrlSecurity>();
    services.AddScoped<IOutboundUrlSecurity, OutboundUrlSecurity>();
    
    // Logging Services - provides centralized logging system management
    services.AddHttpClient<ISeqConnectivity, SeqConnectivity>();
    services.AddScoped<ILoggingStatus, LoggingStatus>();
    services.AddScoped<ILoggingService, LoggingService>();

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
    services.AddScoped<IAiService, AiService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IApiKeyService, ApiKeyService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddTransient<IRecipeService, RecipeService>();
    services.AddTransient<IOrderService, OrderService>();
    services.AddTransient<ICustomerService, CustomerService>();
    services.AddTransient<IWebScraperService, WebScraperService>();
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
        Description = "API for the Cookbook application and AI Service. " +
                      "Authenticate using the 'Authorize' button and provide your API key." +
                      "\n\n**Note:** Endpoints marked with ⚠️ are currently unavailable due to missing configuration."
      });

      c.EnableAnnotations();

      var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
      c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

      // Register operation filters
      c.OperationFilter<FormFileOperationFilter>();
      c.OperationFilter<ServiceAvailabilityOperationFilter>();

      c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
      {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Description = "API key authentication. Enter your API key here.",
      });

      c.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
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
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
          },
          Array.Empty<string>()
        },
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
          },
          Array.Empty<string>()
        }
      });
    });
  }
}
