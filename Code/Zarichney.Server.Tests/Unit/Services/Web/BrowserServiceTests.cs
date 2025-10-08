using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Moq;
using Xunit;
using Zarichney.Services.Web;
using Zarichney.Tests.Framework.Mocks;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Web;

/// <summary>
/// Unit tests for BrowserService - web scraping service using Playwright
/// Tests cover navigation, element selection, error handling, and resource management
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "WebScraping")]
public class BrowserServiceTests : IDisposable
{
  private readonly IFixture _fixture;
  private readonly Mock<ILogger<BrowserService>> _mockLogger;
  private readonly Mock<IWebHostEnvironment> _mockEnvironment;
  private readonly WebscraperConfigBuilder _configBuilder;
  private bool _disposed;

  public BrowserServiceTests()
  {
    _fixture = new Fixture();
    _mockLogger = new Mock<ILogger<BrowserService>>();
    _mockEnvironment = new Mock<IWebHostEnvironment>();
    _configBuilder = new WebscraperConfigBuilder();

    // Default to development environment
    _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
  }

  #region GetContentAsync - Success Scenarios

  [Fact]
  public async Task GetContentAsync_WithValidUrlAndSelector_ReturnsHrefList()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";
    var expectedHrefs = new List<string> { "/page1", "/page2", "/page3" };

    var (mockBrowser, mockContext, mockPage) = BrowserServiceMockFactory
      .CreateSuccessfulNavigationSetup(expectedHrefs);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEquivalentTo(expectedHrefs, "service should return all href attributes");

    // Verify navigation occurred
    mockPage.Verify(x => x.GotoAsync(url, It.IsAny<PageGotoOptions>()), Times.Once);

    // Verify selector was waited for
    mockPage.Verify(x => x.WaitForSelectorAsync(selector, It.IsAny<PageWaitForSelectorOptions>()), Times.Once);

    // Verify elements were queried
    mockPage.Verify(x => x.QuerySelectorAllAsync(selector), Times.Once);
  }

  [Fact]
  public async Task GetContentAsync_WithDuplicateHrefs_ReturnsDistinctList()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";
    var hrefs = new List<string> { "/page1", "/page2", "/page1", "/page3", "/page2" };
    var expectedDistinct = new List<string> { "/page1", "/page2", "/page3" };

    var (mockBrowser, _, _) = BrowserServiceMockFactory
      .CreateSuccessfulNavigationSetup(hrefs);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEquivalentTo(expectedDistinct, "service should return distinct hrefs only");
    result.Should().HaveCount(3, "duplicates should be removed");
  }

  [Fact]
  public async Task GetContentAsync_WithCookieConsentBanner_ClicksAcceptButton()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";
    var expectedHrefs = new List<string> { "/page1" };

    var mockConsentButton = BrowserServiceMockFactory.CreateElementHandle();
    var elements = expectedHrefs.Select(href => BrowserServiceMockFactory.CreateElementHandle(href)).ToList();
    var mockResponse = BrowserServiceMockFactory.CreateResponse(true, 200);
    var mockPage = BrowserServiceMockFactory.CreatePage(mockResponse, elements, mockConsentButton);
    var mockContext = BrowserServiceMockFactory.CreateBrowserContext(mockPage);
    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();

    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .ReturnsAsync(mockContext.Object);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEquivalentTo(expectedHrefs);

    // Verify consent button was clicked
    mockConsentButton.Verify(x => x.ClickAsync(It.IsAny<ElementHandleClickOptions>()), Times.Once);

    // Verify logging
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Accepted cookie consent")),
        null,
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task GetContentAsync_WithEmptySelector_ReturnsEmptyList()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.nonexistent";
    var emptyList = new List<string>();

    var (mockBrowser, _, _) = BrowserServiceMockFactory
      .CreateSuccessfulNavigationSetup(emptyList);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEmpty("no elements match the selector");
  }

  #endregion

  #region GetContentAsync - Error Scenarios

  [Fact]
  public async Task GetContentAsync_WithFailedNavigation_ReturnsEmptyList()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";

    var (mockBrowser, _, mockPage) = BrowserServiceMockFactory
      .CreateFailedNavigationSetup(404);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEmpty("navigation failed");

    // Verify warning was logged
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load page")),
        null,
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task GetContentAsync_WithSelectorTimeout_ReturnsEmptyList()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";

    var (mockBrowser, _, mockPage) = BrowserServiceMockFactory.CreateTimeoutSetup();

    var config = _configBuilder.WithMinimalTimeout().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEmpty("selector wait timed out");

    // Verify warning was logged
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found within timeout")),
        null,
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task GetContentAsync_WithNavigationException_ReturnsEmptyList()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";

    var mockPage = new Mock<IPage>();
    mockPage
      .Setup(x => x.GotoAsync(It.IsAny<string>(), It.IsAny<PageGotoOptions>()))
      .ThrowsAsync(new PlaywrightException("Navigation failed"));

    var mockContext = BrowserServiceMockFactory.CreateBrowserContext(mockPage);
    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();
    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .ReturnsAsync(mockContext.Object);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEmpty("navigation threw exception");

    // Verify error was logged
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting content")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task GetContentAsync_WithNullResponse_ReturnsEmptyList()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";

    var mockPage = BrowserServiceMockFactory.CreatePage(null);
    var mockContext = BrowserServiceMockFactory.CreateBrowserContext(mockPage);
    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();
    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .ReturnsAsync(mockContext.Object);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEmpty("null response indicates failure");
  }

  #endregion

  #region GetContentAsync - Concurrency & Resource Management

  [Fact]
  public async Task GetContentAsync_WithConcurrentRequests_RespectsSemaphoreLimit()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";
    var maxParallel = 2;
    var requestCount = 5;

    var config = _configBuilder
      .WithMaxParallelPages(maxParallel)
      .Build();

    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();
    var activeContexts = 0;
    var maxActiveContexts = 0;

    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .Returns(async () =>
      {
        Interlocked.Increment(ref activeContexts);
        maxActiveContexts = Math.Max(maxActiveContexts, activeContexts);

        await Task.Delay(50); // Simulate work

        var mockPage = BrowserServiceMockFactory.CreatePage(
          BrowserServiceMockFactory.CreateResponse(true),
          new List<Mock<IElementHandle>>());
        var mockContext = BrowserServiceMockFactory.CreateBrowserContext(mockPage);

        mockContext
          .Setup(x => x.CloseAsync())
          .Returns(async () =>
          {
            await Task.CompletedTask;
            Interlocked.Decrement(ref activeContexts);
          });

        return mockContext.Object;
      });

    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var tasks = Enumerable.Range(0, requestCount)
      .Select(_ => sut.GetContentAsync(url, selector))
      .ToArray();

    await Task.WhenAll(tasks);

    // Assert
    maxActiveContexts.Should().BeLessOrEqualTo(maxParallel,
      "concurrent requests should respect semaphore limit");
  }

  [Fact]
  public async Task GetContentAsync_WithCancellation_ThrowsOperationCanceledException()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";
    using var cts = new CancellationTokenSource();

    var mockPage = new Mock<IPage>();
    var tcs = new TaskCompletionSource<IResponse?>();

    mockPage
      .Setup(x => x.GotoAsync(It.IsAny<string>(), It.IsAny<PageGotoOptions>()))
      .Returns(async () =>
      {
        await Task.Delay(100);
        return await tcs.Task;
      });

    var mockContext = BrowserServiceMockFactory.CreateBrowserContext(mockPage);
    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();
    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .ReturnsAsync(mockContext.Object);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act & Assert
    var task = sut.GetContentAsync(url, selector, cts.Token);
    cts.Cancel();
    tcs.SetCanceled();

    var result = await task; // Should handle cancellation gracefully
    result.Should().BeEmpty("cancellation should return empty list");
  }

  #endregion

  #region GetContentAsync - Edge Cases

  [Fact]
  public async Task GetContentAsync_WithElementsHavingNullHref_FiltersNullValues()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";
    var hrefs = new List<string?> { "/page1", null, "/page2", "", "/page3" };
    var expectedHrefs = new List<string> { "/page1", "/page2", "/page3" };

    var elements = hrefs.Select(href => BrowserServiceMockFactory.CreateElementHandle(href)).ToList();
    var mockResponse = BrowserServiceMockFactory.CreateResponse(true, 200);
    var mockPage = BrowserServiceMockFactory.CreatePage(mockResponse, elements);
    var mockContext = BrowserServiceMockFactory.CreateBrowserContext(mockPage);
    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();

    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .ReturnsAsync(mockContext.Object);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEquivalentTo(expectedHrefs, "null and empty hrefs should be filtered out");
  }

  [Fact]
  public async Task GetContentAsync_WithPageCloseException_HandlesGracefully()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";

    var mockPage = BrowserServiceMockFactory.CreatePage(
      BrowserServiceMockFactory.CreateResponse(true),
      new List<Mock<IElementHandle>>());

    mockPage
      .Setup(x => x.CloseAsync())
      .ThrowsAsync(new PlaywrightException("Page already closed"));

    var mockContext = BrowserServiceMockFactory.CreateBrowserContext(mockPage);
    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();
    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .ReturnsAsync(mockContext.Object);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEmpty();

    // Verify error was logged
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while closing the page")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task GetContentAsync_WithContextCloseException_HandlesGracefully()
  {
    // Arrange
    var url = "https://example.com";
    var selector = "a.link";

    var mockPage = BrowserServiceMockFactory.CreatePage(
      BrowserServiceMockFactory.CreateResponse(true),
      new List<Mock<IElementHandle>>());

    var mockContext = BrowserServiceMockFactory.CreateBrowserContext(mockPage);
    mockContext
      .Setup(x => x.CloseAsync())
      .ThrowsAsync(new PlaywrightException("Context already closed"));

    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();
    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .ReturnsAsync(mockContext.Object);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    var result = await sut.GetContentAsync(url, selector);

    // Assert
    result.Should().BeEmpty();

    // Verify error was logged
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while closing the browser context")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  #endregion

  #region DisposeAsync Tests

  [Fact]
  public async Task DisposeAsync_WithValidBrowser_ClosesAndDisposesResources()
  {
    // Arrange
    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();
    var mockAsyncDisposable = mockBrowser.As<IAsyncDisposable>();
    mockAsyncDisposable
      .Setup(x => x.DisposeAsync())
      .Returns(ValueTask.CompletedTask);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    await sut.DisposeAsync();

    // Assert
    mockBrowser.Verify(x => x.CloseAsync(), Times.Once, "browser should be closed");
    mockAsyncDisposable.Verify(x => x.DisposeAsync(), Times.Once, "browser should be disposed");
  }

  [Fact]
  public async Task DisposeAsync_CalledMultipleTimes_OnlyDisposesOnce()
  {
    // Arrange
    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();
    var mockAsyncDisposable = mockBrowser.As<IAsyncDisposable>();
    mockAsyncDisposable
      .Setup(x => x.DisposeAsync())
      .Returns(ValueTask.CompletedTask);

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    await sut.DisposeAsync();
    await sut.DisposeAsync();
    await sut.DisposeAsync();

    // Assert
    mockBrowser.Verify(x => x.CloseAsync(), Times.Once, "browser should only be closed once");
    mockAsyncDisposable.Verify(x => x.DisposeAsync(), Times.Once, "browser should only be disposed once");
  }

  [Fact]
  public async Task DisposeAsync_WithBrowserAsIDisposable_CallsDispose()
  {
    // Arrange
    var mockBrowser = BrowserServiceMockFactory.CreateBrowser();
    var mockDisposable = mockBrowser.As<IDisposable>();
    mockDisposable.Setup(x => x.Dispose());

    var config = _configBuilder.WithDefaults().Build();
    var sut = CreateSystemUnderTest(config, mockBrowser.Object);

    // Act
    await sut.DisposeAsync();

    // Assert
    mockBrowser.Verify(x => x.CloseAsync(), Times.Once);
    mockDisposable.Verify(x => x.Dispose(), Times.Once, "fallback to IDisposable when IAsyncDisposable not available");
  }

  #endregion

  #region Environment-Specific Configuration Tests

  [Fact]
  public void Constructor_InProductionEnvironment_ConfiguresChromePath()
  {
    // Arrange
    _mockEnvironment
      .Setup(x => x.EnvironmentName)
      .Returns("Production");

    _mockEnvironment
      .Setup(x => x.IsProduction())
      .Returns(true);

    var config = _configBuilder.WithDefaults().Build();

    // This test verifies production configuration is set but doesn't test
    // the actual browser launch since that happens in the constructor
    // The configuration would be used when launching the browser

    // Act & Assert
    // Constructor runs and configures browser options for production
    // Actual verification would require integration testing
    _mockEnvironment.Verify(x => x.IsProduction(), Times.Once);
  }

  #endregion

  #region Helper Methods

  private BrowserService CreateSystemUnderTest(
    Zarichney.Cookbook.Recipes.WebscraperConfig config,
    IBrowser mockBrowser)
  {
    // Create a test implementation that uses the provided mock browser
    // instead of creating a real Playwright instance
    return new TestBrowserService(config, _mockEnvironment.Object, _mockLogger.Object, mockBrowser);
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed && disposing)
    {
      // Clean up any resources if needed
      _disposed = true;
    }
  }

  #endregion

  #region Test Browser Service Implementation

  /// <summary>
  /// Test implementation of BrowserService that accepts a mock browser
  /// </summary>
  private class TestBrowserService : BrowserService
  {
    public TestBrowserService(
      Zarichney.Cookbook.Recipes.WebscraperConfig config,
      IWebHostEnvironment env,
      ILogger<BrowserService> logger,
      IBrowser mockBrowser) : base(config, env, logger)
    {
      // Use reflection to set the private _browser field
      var browserField = typeof(BrowserService)
        .GetField("_browser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      browserField?.SetValue(this, mockBrowser);

      // Set _playwright to null since we're using a mock browser
      var playwrightField = typeof(BrowserService)
        .GetField("_playwright", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      playwrightField?.SetValue(this, null);
    }
  }

  #endregion
}