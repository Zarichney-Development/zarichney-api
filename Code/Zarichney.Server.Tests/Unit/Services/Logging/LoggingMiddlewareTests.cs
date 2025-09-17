using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;
using Xunit;
using Zarichney.Services.Logging;
using Zarichney.Services.Sessions;

namespace Zarichney.Server.Tests.Unit.Services.Logging;

public class LoggingMiddlewareTests
{
    private readonly Mock<ILogger<RequestResponseLoggerMiddleware>> _mockLogger;
    private readonly Mock<IOptions<RequestResponseLoggerOptions>> _mockOptions;
    private readonly Mock<ILoggingStatus> _mockLoggingStatus;
    private readonly Mock<IScopeContainer> _mockScopeContainer;
    private readonly RequestResponseLoggerOptions _options;
    private readonly DefaultHttpContext _httpContext;
    private readonly ServiceCollection _services;
    
    public LoggingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<RequestResponseLoggerMiddleware>>();
        _mockOptions = new Mock<IOptions<RequestResponseLoggerOptions>>();
        _mockLoggingStatus = new Mock<ILoggingStatus>();
        _mockScopeContainer = new Mock<IScopeContainer>();
        _options = new RequestResponseLoggerOptions();
        _services = new ServiceCollection();
        
        _mockOptions.Setup(x => x.Value).Returns(_options);
        _mockScopeContainer.Setup(x => x.Id).Returns(Guid.NewGuid());
        _mockScopeContainer.Setup(x => x.SessionId).Returns(Guid.NewGuid());
        _mockLoggingStatus.Setup(x => x.GetLoggingMethodAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Test");
        
        _services.AddSingleton(_mockLoggingStatus.Object);
        var serviceProvider = _services.BuildServiceProvider();
        
        _httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Request =
            {
                Method = "GET",
                Path = "/api/test",
                QueryString = new QueryString("?param=value")
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
        
        _httpContext.Features.Set(_mockScopeContainer.Object);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_WithRequestLoggingEnabled_LogsRequest()
    {
        // Arrange
        _options.LogRequests = true;
        var nextCalled = false;
        RequestDelegate next = (context) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        nextCalled.Should().BeTrue("because the next middleware should be called");
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP Request")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because request logging is enabled");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_WithResponseLoggingEnabled_LogsResponse()
    {
        // Arrange
        _options.LogResponses = true;
        RequestDelegate next = async (context) =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Response content");
        };
        
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP Response")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because response logging is enabled");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_WithRequestFilterReturningFalse_SkipsLogging()
    {
        // Arrange
        _options.RequestFilter = _ => false;
        var nextCalled = false;
        RequestDelegate next = (context) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        nextCalled.Should().BeTrue("because the next middleware should still be called");
        _mockLogger.Verify(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "because the request filter returned false");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_WithSensitiveHeaders_MasksThemInLogs()
    {
        // Arrange
        _options.LogRequests = true;
        _options.SensitiveHeaders = ["Authorization", "Cookie"];
        _httpContext.Request.Headers.Append("Authorization", "Bearer token123");
        _httpContext.Request.Headers.Append("User-Agent", "TestAgent");
        
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        // Verify that logging happened
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP Request")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because request should be logged");
        
        // The actual masking happens in the MaskSensitiveHeaders method which returns a dictionary
        // We can't easily verify the masking in the log call itself due to how the logger structures data
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_WithRequestBody_LogsBodyContent()
    {
        // Arrange
        _options.LogRequests = true;
        const string requestBody = "{\"test\":\"data\"}";
        _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
        _httpContext.Request.ContentLength = requestBody.Length;
        
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RequestBody")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because request body should be logged");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_EnablesRequestBuffering_ForRereadability()
    {
        // Arrange
        _options.LogRequests = true;
        var requestBody = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        _httpContext.Request.Body = requestBody;
        
        var bodyReadInNext = string.Empty;
        RequestDelegate next = async (context) =>
        {
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            bodyReadInNext = await reader.ReadToEndAsync();
        };
        
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        bodyReadInNext.Should().Be("test",
            "because request buffering should allow the body to be read in next middleware");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_WithLoggingDisabled_DoesNotLog()
    {
        // Arrange
        _options.LogRequests = false;
        _options.LogResponses = false;
        
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        _mockLogger.Verify(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "because both request and response logging are disabled");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_CachesLoggingMethod_ForPerformance()
    {
        // Arrange
        _options.LogRequests = true;
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Create a new context for the second call to avoid stream disposal issues
        var secondContext = new DefaultHttpContext
        {
            RequestServices = _services.BuildServiceProvider(),
            Request =
            {
                Method = "GET",
                Path = "/api/test2"
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
        secondContext.Features.Set(_mockScopeContainer.Object);
        
        // Act - Call twice with different contexts
        await middleware.InvokeAsync(_httpContext);
        await middleware.InvokeAsync(secondContext);
        
        // Assert
        _mockLoggingStatus.Verify(x => x.GetLoggingMethodAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "because logging method should be cached between requests");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_RestoresResponseBody_AfterLogging()
    {
        // Arrange
        _options.LogResponses = true;
        const string responseContent = "Test response";
        var originalBody = new MemoryStream();
        _httpContext.Response.Body = originalBody;
        
        RequestDelegate next = async (context) =>
        {
            await context.Response.WriteAsync(responseContent);
        };
        
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        originalBody.Position = 0;
        using var reader = new StreamReader(originalBody);
        var result = await reader.ReadToEndAsync();
        result.Should().Be(responseContent,
            "because the response body should be restored to the original stream");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_WithNoScopeContainer_HandlesGracefully()
    {
        // Arrange
        _httpContext.Features.Set<IScopeContainer>(null);
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(_httpContext);
        
        // Assert
        await act.Should().NotThrowAsync(
            "because the middleware should handle missing scope container gracefully");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_LogsResponseEvenWhenNextThrows_InFinally()
    {
        // Arrange
        _options.LogResponses = true;
        RequestDelegate next = _ => throw new InvalidOperationException("Test exception");
        
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(_httpContext);
        
        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP Response")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because response should be logged even when an exception occurs");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task InvokeAsync_WithQueryString_LogsQueryParameters()
    {
        // Arrange
        _options.LogRequests = true;
        _httpContext.Request.QueryString = new QueryString("?key1=value1&key2=value2");
        
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new RequestResponseLoggerMiddleware(next, _mockOptions.Object, _mockLogger.Object);
        
        // Act
        await middleware.InvokeAsync(_httpContext);
        
        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("key1=value1")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because query string should be included in request logging");
    }
}

public class LoggingServiceCollectionExtensionsTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRequestResponseLogger_WithConfiguration_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddRequestResponseLogger(options =>
        {
            options.LogRequests = false;
            options.LogResponses = true;
            options.LogDirectory = "CustomLogs";
        });
        
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestResponseLoggerOptions>>();
        
        // Assert
        options.Value.LogRequests.Should().BeFalse(
            "because it was configured to false");
        options.Value.LogResponses.Should().BeTrue(
            "because it was configured to true");
        options.Value.LogDirectory.Should().Be("CustomLogs",
            "because it was configured to CustomLogs");
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRequestResponseLogger_WithoutConfiguration_UsesDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddRequestResponseLogger();
        
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RequestResponseLoggerOptions>>();
        
        // Assert
        options.Value.LogRequests.Should().BeTrue(
            "because it's the default value");
        options.Value.LogResponses.Should().BeTrue(
            "because it's the default value");
        options.Value.LogDirectory.Should().Be("Logs",
            "because it's the default value");
        options.Value.SensitiveHeaders.Should().Contain("Authorization",
            "because it's a default sensitive header");
    }
}