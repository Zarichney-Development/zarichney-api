using System.Text;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Graph;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.OpenApi.Models;
using OpenAI;
using OpenAI.Audio;
using Serilog;
using Zarichney.Server.Auth;
using Zarichney.Server.Config;
using Zarichney.Server.Cookbook.Customers;
using Zarichney.Server.Cookbook.Orders;
using Zarichney.Server.Cookbook.Recipes;
using Zarichney.Server.Services;
using Zarichney.Server.Services.AI;
using Zarichney.Server.Services.Emails;
using Zarichney.Server.Services.Payment;
using Zarichney.Server.Services.Sessions;

var builder = WebApplication.CreateBuilder(args);

ConfigureBuilder(builder);

var app = builder.Build();

await ConfigureApplication(app);

try
{
  app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}

return;

void ConfigureBuilder(WebApplicationBuilder webBuilder)
{
  ConfigureEncoding();
  ConfigureKestrel(webBuilder);
  ConfigureConfiguration(webBuilder);
  ConfigureLogging(webBuilder);
  ConfigureServices(webBuilder);
  ConfigureIdentity(webBuilder);
  ConfigureSwagger(webBuilder);
  ConfigureCors(webBuilder);
}

// Configuration Methods
void ConfigureEncoding()
{
  Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
}

void ConfigureKestrel(WebApplicationBuilder webBuilder)
{
  webBuilder.WebHost.ConfigureKestrel(serverOptions =>
  {
    serverOptions.ConfigureEndpointDefaults(listenOptions =>
    {
      listenOptions.KestrelServerOptions.ConfigureEndpointDefaults(_ => { });
    });

    serverOptions.ListenAnyIP(5000,
      options => { options.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2; });
  });
}

void ConfigureConfiguration(WebApplicationBuilder webBuilder)
{
  webBuilder.Configuration
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddSystemsManager("/cookbook-api", new Amazon.Extensions.NETCore.Setup.AWSOptions
    {
      Region = Amazon.RegionEndpoint.USEast2
    });
}

void ConfigureLogging(WebApplicationBuilder webBuilder)
{
  var logger = new LoggerConfiguration()
    .WriteTo.Console(
      outputTemplate:
      "[{Timestamp:HH:mm:ss} {Level:u3}] {SessionId} {ScopeId} {Message:lj}{NewLine}{Exception}"
    )
    .Enrich.FromLogContext()
    .Enrich.WithProperty("SessionId", null)
    .Enrich.WithProperty("ScopeId", null);

  var seqUrl = webBuilder.Configuration["LoggingConfig:SeqUrl"];
  if (!string.IsNullOrEmpty(seqUrl) && Uri.IsWellFormedUriString(seqUrl, UriKind.Absolute))
  {
    logger = logger.WriteTo.Seq(seqUrl);
  }
  else
  {
    var logPath = Path.Combine("logs", "zarichney-api.log");
    logger = logger.WriteTo.File(
      logPath,
      rollingInterval: RollingInterval.Day,
      retainedFileCountLimit: 7
    );
  }

  Log.Logger = logger.CreateLogger();
  Log.Information("Starting up Zarichney API...");

  webBuilder.Host.UseSerilog();
}

void ConfigureServices(WebApplicationBuilder webBuilder)
{
  var services = webBuilder.Services;

  services.RegisterConfigurationServices(webBuilder.Configuration);
  services.AddSingleton(Log.Logger);

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

void ConfigureIdentity(WebApplicationBuilder webBuilder)
{
  webBuilder.Services.AddIdentityServices(webBuilder.Configuration);

  // Add role manager
  webBuilder.Services.AddScoped<IRoleManager, RoleManager>();

  // Register RoleManager<IdentityRole> if not already registered by AddIdentity
  webBuilder.Services.AddScoped<RoleManager<IdentityRole>>();
}

void ConfigureEmailServices(IServiceCollection services)
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

void ConfigureOpenAiServices(IServiceCollection services)
{
  var llmConfig = services.GetService<LlmConfig>();
  var apiKey = llmConfig.ApiKey ??
               throw new InvalidConfigurationException("Missing required configuration for OpenAI API key.");

  services.AddSingleton(new OpenAIClient(apiKey));
  services.AddSingleton(sp => new AudioClient(sp.GetRequiredService<TranscribeConfig>().ModelName, apiKey));
}

void ConfigureApplicationServices(IServiceCollection services)
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

void ConfigureSwagger(WebApplicationBuilder webBuilder)
{
  webBuilder.Services.AddEndpointsApiExplorer();
  webBuilder.Services.AddSwaggerGen(c =>
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

void ConfigureCors(WebApplicationBuilder webBuilder)
{
  webBuilder.Services.AddCors(options =>
  {
    options.AddPolicy("AllowSpecificOrigin",
      policyBuilder =>
      {
        policyBuilder
          .WithOrigins(
            "http://localhost:3001",
            "http://localhost:4200",
            "http://localhost:5000",
            "https://zarichney.com"
          )
          .AllowAnyHeader()
          .AllowAnyMethod();
      });
  });

  webBuilder.Services.AddRequestResponseLogger(options =>
  {
    options.LogRequests = true;
    options.LogResponses = true;
    options.SensitiveHeaders = ["Authorization", "Cookie", "X-API-Key"];
    options.RequestFilter = context => !context.Request.Path.StartsWithSegments("/api/swagger");
    options.LogDirectory = Path.Combine(webBuilder.Environment.ContentRootPath, "Logs");
  });
}

async Task ConfigureApplication(WebApplication application)
{
  application.UseMiddleware<RequestResponseLoggerMiddleware>();
  application.UseMiddleware<ErrorHandlingMiddleware>();

  application
    .UseSwagger(c =>
    {
      Log.Information("Configuring Swagger JSON at: api/swagger/swagger.json");
      c.RouteTemplate = "api/swagger/{documentName}.json";
    })
    .UseSwaggerUI(c =>
    {
      c.SwaggerEndpoint("/api/swagger/swagger.json", "Zarichney API");
      c.RoutePrefix = "api/swagger";
    });

  application.UseSessionManagement();
  application.UseCors("AllowSpecificOrigin");
  application.UseHttpsRedirection();
  application.UseAuthentication();
  application.UseApiKeyAuth();
  application.UseAuthorization();
  application.MapControllers();

  if (application.Environment.IsProduction())
  {
    await application.Services.GetRequiredService<IRecipeRepository>().InitializeAsync();
  }
}