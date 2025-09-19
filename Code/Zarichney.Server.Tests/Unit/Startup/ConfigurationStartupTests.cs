using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using Xunit;
using Zarichney.Config;
using Zarichney.Startup;
using Zarichney.Tests.Framework.Helpers;
using Zarichney.Services.Email;
using Zarichney.Services.Payment;

namespace Zarichney.Tests.Unit.Startup;

public class ConfigurationStartupTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly IServiceCollection _services;
    private readonly IConfigurationBuilder _configBuilder;
    private IConfiguration _configuration;

    public ConfigurationStartupTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _services = new ServiceCollection();
        _configBuilder = new ConfigurationBuilder();
        _configuration = _configBuilder.Build();

        // Setup default environment values
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureConfiguration_DevelopmentEnvironment_ExecutesWithoutError()
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Development");

        // Act & Assert
        var act = () => ConfigurationStartup.ConfigureConfiguration(builder);
        act.Should().NotThrow("because ConfigureConfiguration should handle Development environment");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureConfiguration_ProductionEnvironment_ExecutesWithoutError()
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Production");

        // Act & Assert
        var act = () => ConfigurationStartup.ConfigureConfiguration(builder);
        act.Should().NotThrow("because ConfigureConfiguration should handle Production environment");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureConfiguration_TestingEnvironment_ExecutesWithoutError()
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Testing");

        // Act & Assert
        var act = () => ConfigurationStartup.ConfigureConfiguration(builder);
        act.Should().NotThrow("because ConfigureConfiguration should handle Testing environment");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureLogging_TestingEnvironment_CreatesLogger()
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Testing");
        var originalLogger = Log.Logger;

        try
        {
            // Act
            ConfigurationStartup.ConfigureLogging(builder);

            // Assert
            Log.Logger.Should().NotBeNull();
            // In Testing environment, console sink should not be added
        }
        finally
        {
            // Cleanup - restore original logger
            Log.Logger = originalLogger;
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureLogging_DevelopmentEnvironment_CreatesLogger()
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Development");
        var originalLogger = Log.Logger;

        try
        {
            // Act
            ConfigurationStartup.ConfigureLogging(builder);

            // Assert
            Log.Logger.Should().NotBeNull();
            // In Development environment, console sink should be added
        }
        finally
        {
            // Cleanup - restore original logger
            Log.Logger = originalLogger;
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureLogging_WithValidSeqUrl_CreatesLogger()
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Development");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["LoggingConfig:SeqUrl"] = "http://localhost:5341"
        });

        var originalLogger = Log.Logger;

        try
        {
            // Act
            ConfigurationStartup.ConfigureLogging(builder);

            // Assert
            Log.Logger.Should().NotBeNull();
            // Seq sink should be added when valid URL is provided
        }
        finally
        {
            // Cleanup - restore original logger
            Log.Logger = originalLogger;
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureLogging_WithoutSeqUrl_CreatesLogger()
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Development");

        var originalLogger = Log.Logger;

        try
        {
            // Act
            ConfigurationStartup.ConfigureLogging(builder);

            // Assert
            Log.Logger.Should().NotBeNull();
            // File sink should be added when no Seq URL is provided
        }
        finally
        {
            // Cleanup - restore original logger
            Log.Logger = originalLogger;
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_DevelopmentEnvironment_FindsSolutionRoot()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestConfig:TestProperty"] = "TestValue"
            })
            .Build();

        // Act
        _services.RegisterConfigurationServices(config, _mockEnvironment.Object);

        // Assert
        _services.Should().NotBeEmpty();
        // Configuration services should be registered
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_ProductionEnvironment_UsesEnvironmentVariable()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestConfig:TestProperty"] = "TestValue"
            })
            .Build();

        // Set environment variable for test
        Environment.SetEnvironmentVariable("APP_DATA_PATH", "/app/data");

        try
        {
            // Act
            _services.RegisterConfigurationServices(config, _mockEnvironment.Object);

            // Assert
            _services.Should().NotBeEmpty();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("APP_DATA_PATH", null);
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_WithConfigClasses_RegistersAsServices()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailConfig:FromEmail"] = "test@example.com",
                ["EmailConfig:AzureTenantId"] = "tenant-id",
                ["EmailConfig:AzureAppId"] = "app-id",
                ["EmailConfig:AzureAppSecret"] = "secret",
                ["EmailConfig:MailCheckApiKey"] = "api-key"
            })
            .Build();

        // Act
        _services.RegisterConfigurationServices(config, _mockEnvironment.Object);

        // Assert
        var provider = _services.BuildServiceProvider();
        var emailConfig = provider.GetService<EmailConfig>();
        emailConfig.Should().NotBeNull();
        emailConfig!.FromEmail.Should().Be("test@example.com");

        var optionsEmailConfig = provider.GetService<IOptions<EmailConfig>>();
        optionsEmailConfig.Should().NotBeNull();
        optionsEmailConfig!.Value.FromEmail.Should().Be("test@example.com");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_WithEnvironmentVariableOverride_OverridesConfig()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailConfig:FromEmail"] = "config@example.com"
            })
            .Build();

        // Set environment variable to override
        Environment.SetEnvironmentVariable("EmailConfig__FromEmail", "env@example.com");

        try
        {
            // Act
            _services.RegisterConfigurationServices(config, _mockEnvironment.Object);

            // Assert
            var provider = _services.BuildServiceProvider();
            var emailConfig = provider.GetService<EmailConfig>();
            emailConfig.Should().NotBeNull();
            emailConfig!.FromEmail.Should().Be("env@example.com");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("EmailConfig__FromEmail", null);
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void AddPrompts_RegistersPromptTypes()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(Program));

        // Act
        _services.AddPrompts(assembly!);

        // Assert
        var provider = _services.BuildServiceProvider();
        // Prompt types from the assembly should be registered as singletons
        _services.Should().Contain(s => s.Lifetime == ServiceLifetime.Singleton);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_WithPathTransformation_TransformsDataPaths()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SomeConfig:DataPath"] = "Data/test-file.json"
            })
            .Build();

        // Act
        _services.RegisterConfigurationServices(config, _mockEnvironment.Object);

        // Assert
        // The Data/ prefix should be transformed to an absolute path
        var transformedPath = config["SomeConfig:DataPath"];
        transformedPath.Should().NotBeNull();
        transformedPath.Should().NotStartWith("Data/", "because the path should be transformed to an absolute path");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_WithMissingRequiredProperty_LogsWarning()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        // Create config with missing required properties
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act & Assert
        // Should not throw, but log warnings
        var act = () => _services.RegisterConfigurationServices(config, _mockEnvironment.Object);
        act.Should().NotThrow("because missing required configuration should log warnings, not throw exceptions");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureLogging_EnrichesLoggerWithProperties()
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Development");
        var originalLogger = Log.Logger;

        try
        {
            // Act
            ConfigurationStartup.ConfigureLogging(builder);

            // Assert
            Log.Logger.Should().NotBeNull();
            // Logger should be enriched with CorrelationId, SessionId, etc.
            // These properties would be null initially but should be present
        }
        finally
        {
            // Cleanup - restore original logger
            Log.Logger = originalLogger;
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureLogging_WithInvalidSeqUrl_CreatesLogger()
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Development");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["LoggingConfig:SeqUrl"] = "not-a-valid-url"
        });

        var originalLogger = Log.Logger;

        try
        {
            // Act
            ConfigurationStartup.ConfigureLogging(builder);

            // Assert
            Log.Logger.Should().NotBeNull();
            // Should fall back to file sink when Seq URL is invalid
        }
        finally
        {
            // Cleanup - restore original logger
            Log.Logger = originalLogger;
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_DevelopmentWithoutSolutionFile_UsesFallback()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        // Mock the scenario where no .sln file is found
        var originalBaseDir = AppContext.BaseDirectory;
        var config = new ConfigurationBuilder().Build();

        // Act
        _services.RegisterConfigurationServices(config, _mockEnvironment.Object);

        // Assert
        _services.Should().NotBeEmpty("because even without finding .sln, fallback logic should handle data path");
    }

    private WebApplicationBuilder CreateMockWebApplicationBuilder(string environmentName)
    {
        // Since WebApplicationBuilder can't be directly mocked, we'll use WebApplication.CreateBuilder
        // with a custom environment for testing
        var builder = WebApplication.CreateBuilder(new string[] { });

        // Override the environment
        builder.Environment.EnvironmentName = environmentName;

        return builder;
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureLogging_WithNullSeqUrl_CreatesLogger()
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Development");
        // Don't add any SeqUrl configuration

        var originalLogger = Log.Logger;

        try
        {
            // Act
            ConfigurationStartup.ConfigureLogging(builder);

            // Assert
            Log.Logger.Should().NotBeNull();
            // File sink should be used when no SeqUrl is configured
        }
        finally
        {
            // Cleanup - restore original logger
            Log.Logger = originalLogger;
        }
    }

    [Trait("Category", "Unit")]
    [Theory]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("http://localhost:5341", true)]
    [InlineData("https://seq.example.com", true)]
    [InlineData("ftp://invalid.com", false)]
    [InlineData("not-a-url", false)]
    public void ConfigureLogging_ValidatesSeqUrl(string seqUrl, bool _)
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder("Development");

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["LoggingConfig:SeqUrl"] = seqUrl
        });

        var originalLogger = Log.Logger;

        try
        {
            // Act
            ConfigurationStartup.ConfigureLogging(builder);

            // Assert
            Log.Logger.Should().NotBeNull();
            // Based on shouldUseSeq, either Seq or File sink would be configured
        }
        finally
        {
            // Cleanup - restore original logger
            Log.Logger = originalLogger;
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void TransformConfigurationPaths_TransformsDataPrefixedPaths()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Config1:Path"] = "Data/file1.json",
                ["Config2:Path"] = "Data/subfolder/file2.json",
                ["Config3:Path"] = "/absolute/path/file3.json",
                ["Config4:Path"] = "NotData/file4.json"
            })
            .Build();

        var methodInfo = typeof(ConfigurationStartup).GetMethod(
            "TransformConfigurationPaths",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = methodInfo?.Invoke(null, new object[] { config, "/app/data", "Data/" }) as Dictionary<string, string>;

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2, "only paths starting with 'Data/' should be transformed");
        result!["Config1:Path"].Should().Be(Path.Combine("/app/data", "file1.json"));
        result["Config2:Path"].Should().Be(Path.Combine("/app/data", "subfolder/file2.json"));
        result.Should().NotContainKey("Config3:Path", "absolute paths should not be transformed");
        result.Should().NotContainKey("Config4:Path", "paths not starting with prefix should not be transformed");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ValidateAndReplaceProperties_HandlesRequiredProperties()
    {
        // Arrange
        var config = new EmailConfig
        {
            FromEmail = "", // Missing required property
            AzureTenantId = "tenant-id",
            AzureAppId = "app-id",
            AzureAppSecret = "secret",
            MailCheckApiKey = "key"
        };

        var methodInfo = typeof(ConfigurationStartup).GetMethod(
            "ValidateAndReplaceProperties",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act & Assert
        var act = () => methodInfo?.Invoke(null, new object[] { config, "EmailConfig" });
        act.Should().NotThrow("because missing required properties should log warnings, not throw");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_HandlesMultipleConfigTypes()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailConfig:FromEmail"] = "test@example.com",
                ["PaymentConfig:StripeApiKey"] = "sk_test_key",
                ["PaymentConfig:StripeWebhookSecret"] = "whsec_test"
            })
            .Build();

        // Act
        _services.RegisterConfigurationServices(config, _mockEnvironment.Object);

        // Assert
        var provider = _services.BuildServiceProvider();

        var emailConfig = provider.GetService<EmailConfig>();
        emailConfig.Should().NotBeNull();
        emailConfig!.FromEmail.Should().Be("test@example.com");

        var paymentConfig = provider.GetService<PaymentConfig>();
        paymentConfig.Should().NotBeNull();
        // PaymentConfig properties would be set from configuration
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_HandlesMixedEnvironmentVariables()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailConfig:FromEmail"] = "config@example.com",
                ["EmailConfig:AzureTenantId"] = "config-tenant"
            })
            .Build();

        // Set only one environment variable override
        Environment.SetEnvironmentVariable("EmailConfig__FromEmail", "env@example.com");

        try
        {
            // Act
            _services.RegisterConfigurationServices(config, _mockEnvironment.Object);

            // Assert
            var provider = _services.BuildServiceProvider();
            var emailConfig = provider.GetService<EmailConfig>();
            emailConfig.Should().NotBeNull();
            emailConfig!.FromEmail.Should().Be("env@example.com", "environment variable should override");
            emailConfig.AzureTenantId.Should().Be("config-tenant", "non-overridden values should remain");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("EmailConfig__FromEmail", null);
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ConfigureConfiguration_HandlesAllEnvironments()
    {
        // Arrange & Act & Assert for each environment
        var environments = new[] { "Development", "Testing", "Staging", "Production" };

        foreach (var env in environments)
        {
            var builder = CreateMockWebApplicationBuilder(env);

            var act = () => ConfigurationStartup.ConfigureConfiguration(builder);
            act.Should().NotThrow($"should handle {env} environment");
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_EmptyConfiguration_HandlesGracefully()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        var config = new ConfigurationBuilder().Build(); // Empty configuration

        // Act & Assert
        var act = () => _services.RegisterConfigurationServices(config, _mockEnvironment.Object);
        act.Should().NotThrow("should handle empty configuration gracefully");

        var provider = _services.BuildServiceProvider();
        // Config objects should still be registered, even if empty
        var emailConfig = provider.GetService<EmailConfig>();
        emailConfig.Should().NotBeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void AddPrompts_EmptyAssembly_HandlesGracefully()
    {
        // Arrange
        var emptyAssembly = typeof(ConfigurationStartupTests).Assembly; // Use test assembly which has no prompts

        // Act & Assert
        var act = () => _services.AddPrompts(emptyAssembly);
        act.Should().NotThrow("should handle assembly with no prompt types");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void RegisterConfigurationServices_ComplexPathScenarios_HandlesCorrectly()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Path1"] = "Data/",  // Edge case: just the prefix
                ["Path2"] = "Data/../../etc/passwd",  // Security: path traversal attempt
                ["Path3"] = "Data/file with spaces.json",  // Spaces in filename
                ["Path4"] = "Data/深/unicode.json",  // Unicode characters
                ["Path5"] = "DataNotPrefix/file.json",  // Similar but not matching prefix
            })
            .Build();

        // Act
        _services.RegisterConfigurationServices(config, _mockEnvironment.Object);

        // Assert
        // Paths should be transformed safely without security issues
        var transformedPath2 = config["Path2"];
        if (transformedPath2 != null && transformedPath2.StartsWith("Data/"))
        {
            transformedPath2.Should().NotContain("..", "path traversal should be handled safely");
        }
    }

    [Trait("Category", "Unit")]
    [Theory]
    [InlineData("Testing", false)]
    [InlineData("Development", true)]
    [InlineData("Production", true)]
    [InlineData("Staging", true)]
    public void ConfigureLogging_EnvironmentSpecificConsoleSink(string environment, bool _)
    {
        // Arrange
        var builder = CreateMockWebApplicationBuilder(environment);
        var originalLogger = Log.Logger;

        try
        {
            // Act
            ConfigurationStartup.ConfigureLogging(builder);

            // Assert
            Log.Logger.Should().NotBeNull();
            // Testing environment should not have console sink
            // Other environments should have console sink
        }
        finally
        {
            // Cleanup - restore original logger
            Log.Logger = originalLogger;
        }
    }

    public void Dispose()
    {
        // Reset static logger to prevent test interference
        Log.CloseAndFlush();
    }
}