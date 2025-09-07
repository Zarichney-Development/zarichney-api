using FluentAssertions;
using Moq;
using Xunit;
using Zarichney.Services.Email;
using Zarichney.Services.FileSystem;

namespace Zarichney.Tests.Unit.Services.Email;

public class TemplateServiceTests : IDisposable
{
    private readonly Mock<IFileService> _mockFileService;
    private readonly EmailConfig _emailConfig;
    private readonly TemplateService _sut;
    private readonly string _testTemplateDirectory;

    public TemplateServiceTests()
    {
        _mockFileService = new Mock<IFileService>();
        _testTemplateDirectory = Path.Combine(Path.GetTempPath(), "TemplateService_Tests");
        _emailConfig = new EmailConfig
        {
            AzureTenantId = "test-tenant-id",
            AzureAppId = "test-app-id",
            AzureAppSecret = "test-app-secret",
            FromEmail = "test@example.com",
            TemplateDirectory = _testTemplateDirectory,
            MailCheckApiKey = "test-api-key"
        };

        // Setup base template for constructor
        var baseTemplateContent = """
            <html>
                <head><title>{{title}}</title></head>
                <body>
                    <h1>{{company_name}}</h1>
                    {{{content}}}
                    <footer>&copy; {{current_year}}</footer>
                </body>
            </html>
            """;

        _mockFileService.Setup(fs => fs.GetFile(Path.Combine(_testTemplateDirectory, "base.html")))
            .Returns(baseTemplateContent);

        _sut = new TemplateService(_emailConfig, _mockFileService.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithValidConfig_InitializesSuccessfully()
    {
        // Arrange & Act - Constructor is called in the setup
        
        // Assert
        _sut.Should().NotBeNull("because the service should be properly initialized");
        
        _mockFileService.Verify(
            fs => fs.GetFile(Path.Combine(_testTemplateDirectory, "base.html")),
            Times.AtLeastOnce,
            "because the base template should be loaded during initialization");
    }


    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyTemplate_WithNewTemplate_LoadsAndCompilesTemplate()
    {
        // Arrange
        var templateName = "welcome";
        var templateContent = "<div>Welcome {{username}}! You have {{count}} messages.</div>";
        var templateData = new Dictionary<string, object>
        {
            ["username"] = "John Doe",
            ["count"] = 5
        };
        var title = "Welcome Email";

        _mockFileService.Setup(fs => fs.GetFileAsync(Path.Combine(_testTemplateDirectory, $"{templateName}.html")))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        result.Should().NotBeNullOrEmpty("because the template should be processed successfully");
        result.Should().Contain("Welcome John Doe!", "because the username should be interpolated");
        result.Should().Contain("You have 5 messages", "because the count should be interpolated");
        result.Should().Contain("Zarichney Development", "because company name should be added");
        result.Should().Contain($"{DateTime.Now.Year}", "because current year should be added");
        result.Should().Contain("Welcome Email", "because the title should be added");

        _mockFileService.Verify(
            fs => fs.GetFileAsync(Path.Combine(_testTemplateDirectory, $"{templateName}.html")),
            Times.Once,
            "because the template file should be loaded once");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyTemplate_WithCachedTemplate_DoesNotReloadTemplate()
    {
        // Arrange
        var templateName = "cached_template";
        var templateContent = "<div>Hello {{name}}!</div>";
        var templateData = new Dictionary<string, object> { ["name"] = "Alice" };
        var title = "Test Email";

        _mockFileService.Setup(fs => fs.GetFileAsync(Path.Combine(_testTemplateDirectory, $"{templateName}.html")))
            .ReturnsAsync(templateContent);

        // Act - First call
        await _sut.ApplyTemplate(templateName, templateData, title);
        // Act - Second call
        var result = await _sut.ApplyTemplate(templateName, new Dictionary<string, object> { ["name"] = "Bob" }, title);

        // Assert
        result.Should().Contain("Hello Bob!", "because the cached template should be used with new data");
        
        _mockFileService.Verify(
            fs => fs.GetFileAsync(Path.Combine(_testTemplateDirectory, $"{templateName}.html")),
            Times.Once,
            "because the template should be loaded only once and then cached");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyTemplate_AddsCompanyNameAutomatically()
    {
        // Arrange
        var templateName = "company_test";
        var templateContent = "<div>{{message}}</div>";
        var templateData = new Dictionary<string, object> { ["message"] = "Test message" };
        var title = "Company Test";

        _mockFileService.Setup(fs => fs.GetFileAsync(It.IsAny<string>()))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        result.Should().Contain("Zarichney Development", "because company name should be automatically added to template data");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyTemplate_AddsCurrentYearAutomatically()
    {
        // Arrange
        var templateName = "year_test";
        var templateContent = "<div>Current year test</div>";
        var templateData = new Dictionary<string, object>();
        var title = "Year Test";

        _mockFileService.Setup(fs => fs.GetFileAsync(It.IsAny<string>()))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        result.Should().Contain(DateTime.Now.Year.ToString(), "because current year should be automatically added");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyTemplate_AddsProvidedTitleToTemplateData()
    {
        // Arrange
        var templateName = "title_test";
        var templateContent = "<div>Title test content</div>";
        var templateData = new Dictionary<string, object> { ["content"] = "Some content" };
        var title = "Custom Email Title";

        _mockFileService.Setup(fs => fs.GetFileAsync(It.IsAny<string>()))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        result.Should().Contain("Custom Email Title", "because the provided title should be added to template data");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyTemplate_WrapsContentInBaseTemplate()
    {
        // Arrange
        var templateName = "content_wrap_test";
        var innerTemplateContent = "<p>Inner content with {{data}}</p>";
        var templateData = new Dictionary<string, object> { ["data"] = "test data" };
        var title = "Content Wrap Test";

        _mockFileService.Setup(fs => fs.GetFileAsync(Path.Combine(_testTemplateDirectory, $"{templateName}.html")))
            .ReturnsAsync(innerTemplateContent);

        // Act
        var result = await _sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        result.Should().Contain("<html>", "because the result should be wrapped in the base template");
        result.Should().Contain("</html>", "because the result should be wrapped in the base template");
        result.Should().Contain("<p>Inner content with test data</p>", "because the inner template should be processed and included");
        result.Should().Contain("Content Wrap Test", "because the title should appear in the base template");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyTemplate_WithEmptyTemplateData_HandlesGracefully()
    {
        // Arrange
        var templateName = "empty_data_test";
        var templateContent = "<div>Static content only</div>";
        var emptyTemplateData = new Dictionary<string, object>();
        var title = "Empty Data Test";

        _mockFileService.Setup(fs => fs.GetFileAsync(It.IsAny<string>()))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _sut.ApplyTemplate(templateName, emptyTemplateData, title);

        // Assert
        result.Should().NotBeNullOrEmpty("because the template should process even with empty data");
        result.Should().Contain("Static content only", "because static content should be preserved");
        result.Should().Contain("Zarichney Development", "because company name should still be added");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyTemplate_WithComplexTemplateData_ProcessesCorrectly()
    {
        // Arrange
        var templateName = "complex_data_test";
        var templateContent = """
            <div>
                <h2>Welcome {{user.name}}!</h2>
                <p>You have {{notifications.count}} notifications:</p>
                <ul>
                    {{#each notifications.items}}
                    <li>{{this.message}} - {{this.date}}</li>
                    {{/each}}
                </ul>
            </div>
            """;
        
        var complexTemplateData = new Dictionary<string, object>
        {
            ["user"] = new { name = "Jane Smith" },
            ["notifications"] = new
            {
                count = 2,
                items = new[]
                {
                    new { message = "New message received", date = "2023-01-15" },
                    new { message = "Task completed", date = "2023-01-14" }
                }
            }
        };
        var title = "Complex Data Test";

        _mockFileService.Setup(fs => fs.GetFileAsync(It.IsAny<string>()))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _sut.ApplyTemplate(templateName, complexTemplateData, title);

        // Assert
        result.Should().Contain("Welcome Jane Smith!", "because nested object data should be accessible");
        result.Should().Contain("You have 2 notifications", "because nested count should be interpolated");
        result.Should().Contain("New message received", "because array items should be iterated");
        result.Should().Contain("Task completed", "because array items should be iterated");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetErrorTemplateData_WithException_ReturnsFormattedErrorData()
    {
        // Arrange
        var testException = new InvalidOperationException("Test error message");
        testException.Data["CustomKey"] = "CustomValue";

        // Act
        var errorData = TemplateService.GetErrorTemplateData(testException);

        // Assert
        errorData.Should().ContainKey("timestamp", "because error timestamp should be included");
        errorData.Should().ContainKey("errorType", "because error type should be included");
        errorData.Should().ContainKey("errorMessage", "because error message should be included");
        errorData.Should().ContainKey("stackTrace", "because stack trace should be included");
        errorData.Should().ContainKey("additionalContext", "because additional context should be included");

        errorData["errorType"].Should().Be("InvalidOperationException", "because the exception type should be captured");
        errorData["errorMessage"].Should().Be("Test error message", "because the exception message should be captured");
        
        var additionalContext = errorData["additionalContext"] as Dictionary<string, string>;
        additionalContext.Should().ContainKey("MachineName", "because machine name should be in additional context");
        additionalContext.Should().ContainKey("OsVersion", "because OS version should be in additional context");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetErrorTemplateData_WithNullStackTrace_HandlesGracefully()
    {
        // Arrange - Create exception without stack trace
        var testException = new Exception("Test message");

        // Act
        var errorData = TemplateService.GetErrorTemplateData(testException);

        // Assert
        errorData["stackTrace"].Should().Be("No stack trace available", 
            "because null stack traces should be handled with a default message");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetErrorTemplateData_TimestampFormat_IsISO8601()
    {
        // Arrange
        var testException = new Exception("Test message");

        // Act
        var errorData = TemplateService.GetErrorTemplateData(testException);

        // Assert
        var timestamp = errorData["timestamp"] as string;
        timestamp.Should().NotBeNullOrEmpty("because timestamp should be provided");
        
        // Verify it's a valid ISO 8601 format
        DateTime.TryParse(timestamp, out _).Should().BeTrue("because timestamp should be in a parseable format");
        timestamp.Should().Contain("T", "because ISO 8601 format should contain T separator");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetErrorTemplateData_AdditionalContext_ContainsEnvironmentInfo()
    {
        // Arrange
        var testException = new Exception("Test message");

        // Act
        var errorData = TemplateService.GetErrorTemplateData(testException);

        // Assert
        var additionalContext = errorData["additionalContext"] as Dictionary<string, string>;
        additionalContext.Should().NotBeNull("because additional context should be provided");
        
        additionalContext["MachineName"].Should().Be(Environment.MachineName, 
            "because machine name should match current environment");
        additionalContext["OsVersion"].Should().Be(Environment.OSVersion.ToString(), 
            "because OS version should match current environment");
    }

    public void Dispose()
    {
        // TemplateService doesn't implement IDisposable, so no disposal needed
        GC.SuppressFinalize(this);
    }
}