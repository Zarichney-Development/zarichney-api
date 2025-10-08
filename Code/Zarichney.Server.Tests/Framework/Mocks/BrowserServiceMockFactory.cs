using Microsoft.Playwright;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zarichney.Tests.Framework.Mocks;

/// <summary>
/// Factory for creating mock Playwright objects for BrowserService testing
/// </summary>
public static class BrowserServiceMockFactory
{
  /// <summary>
  /// Creates a mock IPlaywright with a configured browser
  /// </summary>
  public static Mock<IPlaywright> CreatePlaywright(Mock<IBrowser> mockBrowser)
  {
    var mockPlaywright = new Mock<IPlaywright>();
    var mockBrowserType = new Mock<IBrowserType>();

    mockBrowserType
      .Setup(x => x.LaunchAsync(It.IsAny<BrowserTypeLaunchOptions>()))
      .ReturnsAsync(mockBrowser.Object);

    mockPlaywright
      .Setup(x => x.Chromium)
      .Returns(mockBrowserType.Object);

    return mockPlaywright;
  }

  /// <summary>
  /// Creates a mock IBrowser with basic configuration
  /// </summary>
  public static Mock<IBrowser> CreateBrowser()
  {
    var mockBrowser = new Mock<IBrowser>();

    // Setup Disconnected event - it's required by the constructor
    mockBrowser
      .SetupAdd(x => x.Disconnected += It.IsAny<System.EventHandler<IBrowser>>());

    mockBrowser
      .SetupRemove(x => x.Disconnected -= It.IsAny<System.EventHandler<IBrowser>>());

    // Setup CloseAsync
    mockBrowser
      .Setup(x => x.CloseAsync())
      .Returns(Task.CompletedTask);

    return mockBrowser;
  }

  /// <summary>
  /// Creates a mock IBrowserContext with configurable page
  /// </summary>
  public static Mock<IBrowserContext> CreateBrowserContext(Mock<IPage> mockPage)
  {
    var mockContext = new Mock<IBrowserContext>();

    mockContext
      .Setup(x => x.NewPageAsync())
      .ReturnsAsync(mockPage.Object);

    mockContext
      .Setup(x => x.SetDefaultTimeout(It.IsAny<float>()));

    mockContext
      .Setup(x => x.SetDefaultNavigationTimeout(It.IsAny<float>()));

    mockContext
      .Setup(x => x.CloseAsync())
      .Returns(Task.CompletedTask);

    return mockContext;
  }

  /// <summary>
  /// Creates a mock IPage with configurable navigation and element selection
  /// </summary>
  public static Mock<IPage> CreatePage(
    Mock<IResponse>? navigationResponse = null,
    List<Mock<IElementHandle>>? elements = null,
    Mock<IElementHandle>? consentButton = null)
  {
    var mockPage = new Mock<IPage>();
    var mockMouse = new Mock<IMouse>();

    // Setup page properties
    mockPage
      .Setup(x => x.ViewportSize)
      .Returns(new ViewportSize { Width = 1280, Height = 720 });

    mockPage
      .Setup(x => x.Mouse)
      .Returns(mockMouse.Object);

    // Setup mouse movement
    mockMouse
      .Setup(x => x.MoveAsync(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<MouseMoveOptions>()))
      .Returns(Task.CompletedTask);

    // Setup navigation
    mockPage
      .Setup(x => x.GotoAsync(It.IsAny<string>(), It.IsAny<PageGotoOptions>()))
      .ReturnsAsync(navigationResponse?.Object);

    // Setup wait methods
    mockPage
      .Setup(x => x.WaitForTimeoutAsync(It.IsAny<float>()))
      .Returns(Task.CompletedTask);

    mockPage
      .Setup(x => x.WaitForSelectorAsync(It.IsAny<string>(), It.IsAny<PageWaitForSelectorOptions>()))
      .ReturnsAsync((IElementHandle?)null);

    // Setup element selection
    if (elements != null)
    {
      mockPage
        .Setup(x => x.QuerySelectorAllAsync(It.IsAny<string>()))
        .ReturnsAsync(elements.Select(e => e.Object).ToArray());
    }

    // Setup cookie consent button
    mockPage
      .Setup(x => x.QuerySelectorAsync("button#cookie-consent-accept"))
      .ReturnsAsync(consentButton?.Object);

    // Setup event handling
    mockPage
      .SetupAdd(x => x.Console += It.IsAny<System.EventHandler<IConsoleMessage>>());
    mockPage
      .SetupRemove(x => x.Console -= It.IsAny<System.EventHandler<IConsoleMessage>>());

    mockPage
      .SetupAdd(x => x.PageError += It.IsAny<System.EventHandler<object>>());
    mockPage
      .SetupRemove(x => x.PageError -= It.IsAny<System.EventHandler<object>>());

    // Setup page closing
    mockPage
      .Setup(x => x.CloseAsync())
      .Returns(Task.CompletedTask);

    return mockPage;
  }

  /// <summary>
  /// Creates a mock IResponse with configurable status
  /// </summary>
  public static Mock<IResponse> CreateResponse(bool isOk = true, int status = 200)
  {
    var mockResponse = new Mock<IResponse>();

    mockResponse
      .Setup(x => x.Ok)
      .Returns(isOk);

    mockResponse
      .Setup(x => x.Status)
      .Returns(status);

    return mockResponse;
  }

  /// <summary>
  /// Creates a mock IElementHandle with configurable href attribute
  /// </summary>
  public static Mock<IElementHandle> CreateElementHandle(string? href = null)
  {
    var mockElement = new Mock<IElementHandle>();

    mockElement
      .Setup(x => x.GetAttributeAsync("href"))
      .ReturnsAsync(href);

    mockElement
      .Setup(x => x.ClickAsync(It.IsAny<ElementHandleClickOptions>()))
      .Returns(Task.CompletedTask);

    return mockElement;
  }

  /// <summary>
  /// Creates a complete mock setup for successful page navigation
  /// </summary>
  public static (Mock<IBrowser> browser, Mock<IBrowserContext> context, Mock<IPage> page) CreateSuccessfulNavigationSetup(
    List<string> hrefs)
  {
    var elements = hrefs.Select(href => CreateElementHandle(href)).ToList();
    var mockResponse = CreateResponse(true, 200);
    var mockPage = CreatePage(mockResponse, elements);
    var mockContext = CreateBrowserContext(mockPage);
    var mockBrowser = CreateBrowser();

    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .ReturnsAsync(mockContext.Object);

    return (mockBrowser, mockContext, mockPage);
  }

  /// <summary>
  /// Creates a complete mock setup for failed page navigation
  /// </summary>
  public static (Mock<IBrowser> browser, Mock<IBrowserContext> context, Mock<IPage> page) CreateFailedNavigationSetup(
    int statusCode = 404)
  {
    var mockResponse = CreateResponse(false, statusCode);
    var mockPage = CreatePage(mockResponse);
    var mockContext = CreateBrowserContext(mockPage);
    var mockBrowser = CreateBrowser();

    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .ReturnsAsync(mockContext.Object);

    return (mockBrowser, mockContext, mockPage);
  }

  /// <summary>
  /// Creates a mock setup where selector wait times out
  /// </summary>
  public static (Mock<IBrowser> browser, Mock<IBrowserContext> context, Mock<IPage> page) CreateTimeoutSetup()
  {
    var mockResponse = CreateResponse(true, 200);
    var mockPage = new Mock<IPage>();

    // Setup successful navigation
    mockPage
      .Setup(x => x.GotoAsync(It.IsAny<string>(), It.IsAny<PageGotoOptions>()))
      .ReturnsAsync(mockResponse.Object);

    mockPage
      .Setup(x => x.WaitForTimeoutAsync(It.IsAny<float>()))
      .Returns(Task.CompletedTask);

    // Setup timeout on selector wait
    mockPage
      .Setup(x => x.WaitForSelectorAsync(It.IsAny<string>(), It.IsAny<PageWaitForSelectorOptions>()))
      .ThrowsAsync(new TimeoutException("Timeout waiting for selector"));

    // Setup other required properties
    mockPage
      .Setup(x => x.ViewportSize)
      .Returns(new ViewportSize { Width = 1280, Height = 720 });

    var mockMouse = new Mock<IMouse>();
    mockPage.Setup(x => x.Mouse).Returns(mockMouse.Object);

    mockPage
      .Setup(x => x.QuerySelectorAsync("button#cookie-consent-accept"))
      .ReturnsAsync((IElementHandle?)null);

    // Setup events
    mockPage.SetupAdd(x => x.Console += It.IsAny<System.EventHandler<IConsoleMessage>>());
    mockPage.SetupRemove(x => x.Console -= It.IsAny<System.EventHandler<IConsoleMessage>>());
    mockPage.SetupAdd(x => x.PageError += It.IsAny<System.EventHandler<object>>());
    mockPage.SetupRemove(x => x.PageError -= It.IsAny<System.EventHandler<object>>());

    mockPage.Setup(x => x.CloseAsync()).Returns(Task.CompletedTask);

    var mockContext = CreateBrowserContext(mockPage);
    var mockBrowser = CreateBrowser();

    mockBrowser
      .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
      .ReturnsAsync(mockContext.Object);

    return (mockBrowser, mockContext, mockPage);
  }
}