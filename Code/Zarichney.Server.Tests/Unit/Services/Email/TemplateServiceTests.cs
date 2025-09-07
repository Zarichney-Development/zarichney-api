using FluentAssertions;
using Moq;
using Xunit;
using Zarichney.Services.Email;
using Zarichney.Services.FileSystem;

namespace Zarichney.Tests.Unit.Services.Email;

[Trait("Category", "Unit")]
public class TemplateServiceTests
{
    private readonly Mock<IFileService> _mockFileService;
    private readonly EmailConfig _emailConfig;
    private readonly string _baseTemplateContent = """
        <html>
            <head><title>{{title}}</title></head>
            <body>
                <h1>{{company_name}}</h1>
                <div>{{{content}}}</div>
                <footer>Copyright {{current_year}}</footer>
            </body>
        </html>
        """;

    public TemplateServiceTests()
    {
        _mockFileService = new Mock<IFileService>();
        _emailConfig = new EmailConfig
        {
            AzureTenantId = "test-tenant-id",
            AzureAppId = "test-app-id",
            AzureAppSecret = "test-secret",
            FromEmail = "from@test.com",
            TemplateDirectory = "/test/templates",
            MailCheckApiKey = "test-api-key"
        };

        // Setup base template
        _mockFileService
            .Setup(x => x.GetFile("/test/templates/base.html"))
            .Returns(_baseTemplateContent);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ValidConfig_CompilesBaseTemplate()
    {
        // Act
        var sut = new TemplateService(_emailConfig, _mockFileService.Object);

        // Assert
        _mockFileService.Verify(
            x => x.GetFile("/test/templates/base.html"),
            Times.Once,
            "base template should be loaded and compiled during construction"
        );
    }

    #endregion

    #region ApplyTemplate Tests

    [Fact]
    public async Task ApplyTemplate_ExistingTemplate_AppliesDataAndReturnsContent()
    {
        // Arrange
        var templateName = "welcome";
        var templateData = new Dictionary<string, object>
        {
            { "username", "John Doe" },
            { "activation_url", "https://example.com/activate" }
        };
        var title = "Welcome to Our Service";
        var templateContent = """
            <div>
                <h2>Welcome {{username}}!</h2>
                <p>Please click <a href="{{activation_url}}">here</a> to activate your account.</p>
            </div>
            """;

        _mockFileService
            .Setup(x => x.GetFileAsync("/test/templates/welcome.html"))
            .ReturnsAsync(templateContent);

        var sut = new TemplateService(_emailConfig, _mockFileService.Object);

        // Act
        var result = await sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        result.Should().NotBeNullOrEmpty("template should return processed content");
        result.Should().Contain("John Doe", "template should contain the username");
        result.Should().Contain("https://example.com/activate", "template should contain the activation URL");
        result.Should().Contain("Welcome to Our Service", "base template should contain the title");
        result.Should().Contain("Zarichney Development", "base template should contain the company name");
        result.Should().Contain(DateTime.Now.Year.ToString(), "base template should contain the current year");
    }

    [Fact]
    public async Task ApplyTemplate_NewTemplate_LoadsCompilesAndCachesTemplate()
    {
        // Arrange
        var templateName = "notification";
        var templateData = new Dictionary<string, object>
        {
            { "message", "Your order has been processed" }
        };
        var title = "Order Notification";
        var templateContent = "<div>{{message}}</div>";

        _mockFileService
            .Setup(x => x.GetFileAsync("/test/templates/notification.html"))
            .ReturnsAsync(templateContent);

        var sut = new TemplateService(_emailConfig, _mockFileService.Object);

        // Act - Call twice to test caching
        var result1 = await sut.ApplyTemplate(templateName, templateData, title);
        var result2 = await sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        result1.Should().Contain("Your order has been processed", "first call should process template correctly");
        result2.Should().Contain("Your order has been processed", "second call should use cached template");

        _mockFileService.Verify(
            x => x.GetFileAsync("/test/templates/notification.html"),
            Times.Once,
            "template file should only be loaded once due to caching"
        );
    }

    [Fact]
    public async Task ApplyTemplate_ComplexTemplateData_ProcessesNestedObjects()
    {
        // Arrange
        var templateName = "invoice";
        var templateData = new Dictionary<string, object>
        {
            { "customer", new { name = "Jane Smith", email = "jane@example.com" } },
            { "order_id", "ORD-12345" },
            { "items", new[] { 
                new { name = "Product A", price = 29.99 },
                new { name = "Product B", price = 19.99 }
            }},
            { "total", 49.98 }
        };
        var title = "Invoice #ORD-12345";
        var templateContent = """
            <div>
                <p>Dear {{customer.name}} ({{customer.email}}),</p>
                <p>Order ID: {{order_id}}</p>
                <ul>
                    {{#each items}}
                    <li>{{this.name}}: ${{this.price}}</li>
                    {{/each}}
                </ul>
                <p>Total: ${{total}}</p>
            </div>
            """;

        _mockFileService
            .Setup(x => x.GetFileAsync("/test/templates/invoice.html"))
            .ReturnsAsync(templateContent);

        var sut = new TemplateService(_emailConfig, _mockFileService.Object);

        // Act
        var result = await sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        result.Should().Contain("Jane Smith", "template should process nested customer.name");
        result.Should().Contain("jane@example.com", "template should process nested customer.email");
        result.Should().Contain("ORD-12345", "template should contain order ID");
        result.Should().Contain("Product A", "template should process array items");
        result.Should().Contain("Product B", "template should process array items");
        result.Should().Contain("$49.98", "template should contain total");
    }

    [Fact]
    public async Task ApplyTemplate_EmptyTemplateData_ProcessesWithDefaults()
    {
        // Arrange
        var templateName = "simple";
        var templateData = new Dictionary<string, object>();
        var title = "Simple Template";
        var templateContent = "<div>Simple content without variables</div>";

        _mockFileService
            .Setup(x => x.GetFileAsync("/test/templates/simple.html"))
            .ReturnsAsync(templateContent);

        var sut = new TemplateService(_emailConfig, _mockFileService.Object);

        // Act
        var result = await sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        result.Should().Contain("Simple content without variables", "template should process static content");
        result.Should().Contain("Simple Template", "base template should contain the title");
        result.Should().Contain("Zarichney Development", "base template should add company name");
        result.Should().Contain(DateTime.Now.Year.ToString(), "base template should add current year");
    }

    [Fact]
    public async Task ApplyTemplate_FileServiceThrows_PropagatesException()
    {
        // Arrange
        var templateName = "missing";
        var templateData = new Dictionary<string, object>();
        var title = "Missing Template";
        var fileException = new FileNotFoundException("Template file not found");

        _mockFileService
            .Setup(x => x.GetFileAsync("/test/templates/missing.html"))
            .ThrowsAsync(fileException);

        var sut = new TemplateService(_emailConfig, _mockFileService.Object);

        // Act
        Func<Task> act = async () => await sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>("file service exception should be propagated")
            .WithMessage("Template file not found");
    }

    [Fact]
    public async Task ApplyTemplate_TemplateCompilationFails_ThrowsException()
    {
        // Arrange
        var templateName = "invalid";
        var templateData = new Dictionary<string, object>();
        var title = "Invalid Template";
        var invalidTemplateContent = "{{#each}}{{/invalid}}"; // Invalid Handlebars syntax

        _mockFileService
            .Setup(x => x.GetFileAsync("/test/templates/invalid.html"))
            .ReturnsAsync(invalidTemplateContent);

        var sut = new TemplateService(_emailConfig, _mockFileService.Object);

        // Act
        Func<Task> act = async () => await sut.ApplyTemplate(templateName, templateData, title);

        // Assert
        await act.Should().ThrowAsync<Exception>("invalid template syntax should cause compilation to fail");
    }

    #endregion

    #region GetErrorTemplateData Tests

    [Fact]
    public void GetErrorTemplateData_StandardException_ReturnsCompleteData()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error message");

        // Act
        var result = TemplateService.GetErrorTemplateData(exception);

        // Assert
        result.Should().ContainKey("timestamp", "error template should include timestamp");
        result.Should().ContainKey("errorType", "error template should include error type");
        result.Should().ContainKey("errorMessage", "error template should include error message");
        result.Should().ContainKey("stackTrace", "error template should include stack trace");
        result.Should().ContainKey("additionalContext", "error template should include additional context");

        result["errorType"].Should().Be("InvalidOperationException", "error type should match exception type");
        result["errorMessage"].Should().Be("Test error message", "error message should match exception message");

        var additionalContext = result["additionalContext"] as Dictionary<string, string>;
        additionalContext.Should().NotBeNull("additional context should be a dictionary");
        additionalContext!.Should().ContainKey("MachineName", "additional context should include machine name");
        additionalContext.Should().ContainKey("OsVersion", "additional context should include OS version");
    }

    [Fact]
    public void GetErrorTemplateData_ExceptionWithStackTrace_IncludesStackTrace()
    {
        // Arrange
        Exception exception;
        try
        {
            throw new ArgumentException("Test exception with stack trace");
        }
        catch (Exception ex)
        {
            exception = ex; // This will have a stack trace
        }

        // Act
        var result = TemplateService.GetErrorTemplateData(exception);

        // Assert
        result["stackTrace"].Should().NotBe("No stack trace available", "exception with stack trace should include actual trace");
        result["stackTrace"].ToString().Should().Contain("GetErrorTemplateData_ExceptionWithStackTrace_IncludesStackTrace", 
            "stack trace should include current method name");
    }

    [Fact]
    public void GetErrorTemplateData_ExceptionWithoutStackTrace_UsesDefaultMessage()
    {
        // Arrange
        var exception = new Exception("Test exception") { HelpLink = null }; // New exception without throw

        // Act
        var result = TemplateService.GetErrorTemplateData(exception);

        // Assert
        result["stackTrace"].Should().Be("No stack trace available", "exception without stack trace should use default message");
    }

    [Fact]
    public void GetErrorTemplateData_TimestampFormat_IsIso8601()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = TemplateService.GetErrorTemplateData(exception);
        var afterCall = DateTime.UtcNow;

        // Assert
        var timestamp = result["timestamp"].ToString();
        timestamp.Should().NotBeNullOrEmpty("timestamp should be present");
        
        var parsedTimestamp = DateTime.Parse(timestamp!);
        parsedTimestamp.Should().BeOnOrAfter(beforeCall.AddSeconds(-1), "timestamp should be recent");
        parsedTimestamp.Should().BeOnOrBefore(afterCall.AddSeconds(1), "timestamp should be recent");
        
        // Check ISO 8601 format (should contain 'T' and end with 'Z' or have timezone info)
        timestamp.Should().Contain("T", "ISO 8601 format should contain T separator");
    }

    [Fact]
    public void GetErrorTemplateData_AdditionalContextStructure_IsCorrect()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        var result = TemplateService.GetErrorTemplateData(exception);

        // Assert
        var additionalContext = result["additionalContext"] as Dictionary<string, string>;
        additionalContext.Should().NotBeNull("additional context should exist");
        additionalContext!.Should().HaveCountGreaterThan(1, "additional context should have at least machine and OS info");
        
        additionalContext["MachineName"].Should().NotBeNullOrEmpty("machine name should be populated");
        additionalContext["OsVersion"].Should().NotBeNullOrEmpty("OS version should be populated");
    }

    #endregion
}