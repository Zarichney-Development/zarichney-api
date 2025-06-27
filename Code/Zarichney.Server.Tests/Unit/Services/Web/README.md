# Module/Directory: /Unit/Services/Web

**Last Updated:** 2025-04-18

> **Parent:** [`Services`](../README.md)
> *(Note: A README for `/Unit/Services/` may be needed)*
> **Related:**
> * **Source:** [`Services/Web/BrowserService.cs`](../../../../Zarichney.Server/Services/Web/BrowserService.cs)
> * **Dependencies:** Browser Automation Library Interfaces (e.g., Playwright `IBrowser`, `IPage`, `ILocator`), `ILogger<BrowserService>`
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `BrowserService` class. This service encapsulates logic for interacting with a headless browser using a browser automation library (like Playwright), enabling tasks such as web scraping, data extraction, or automated navigation.

* **Why Unit Tests?** To validate the orchestration logic within `BrowserService` in isolation from actually launching or controlling a real browser instance. Tests ensure the service correctly uses the (mocked) browser automation library interfaces to perform navigation, locate elements, interact with elements (click, type), extract data, and handle potential browser-related errors gracefully.
* **Isolation:** Achieved by mocking the interfaces provided by the browser automation library (e.g., Playwright's `IBrowser`, `IBrowserContext`, `IPage`, `ILocator`, `IElementHandle`) and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `BrowserService` focus on its public methods, such as:

* **Initialization/Disposal:** Verifying browser instances and pages are created and disposed of correctly (interaction with mocked `IBrowser`, `IContext`, `IPage` creation/disposal methods).
* **Navigation (`NavigateAsync`, etc.):** Ensuring `IPage.GotoAsync` is called with the correct URL and options. Handling navigation success and failure (mocked).
* **Element Location & Interaction (`ClickAsync`, `TypeTextAsync`, `SelectOptionAsync`, etc.):** Verifying the use of correct selectors with methods like `IPage.Locator(...)`. Ensuring interaction methods (`ClickAsync`, `FillAsync`, `SelectOptionAsync`) on the mocked `ILocator` are called. Handling element-not-found scenarios (mocked).
* **Data Extraction (`GetPageHtmlAsync`, `GetElementTextAsync`, `GetElementAttributeAsync`, etc.):** Verifying calls to methods like `IPage.ContentAsync`, `ILocator.TextContentAsync`, `ILocator.GetAttributeAsync` on mocked objects. Handling cases where text/attributes are present or missing.
* **Waiting Logic (`WaitForSelectorAsync`, etc.):** Verifying calls to explicit wait methods on the mocked `IPage` or `ILocator`. Handling timeout exceptions (mocked).
* **Error Handling:** Testing how exceptions thrown by the mocked browser automation library interfaces (e.g., `TimeoutException`, `PlaywrightException`) are caught, logged, and potentially translated.

## 3. Test Environment Setup

* **Instantiation:** `BrowserService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * Mocks for the core interfaces of the chosen browser automation library (e.g., `Mock<Microsoft.Playwright.IBrowser>`, `Mock<Microsoft.Playwright.IBrowserContext>`, `Mock<Microsoft.Playwright.IPage>`, `Mock<Microsoft.Playwright.ILocator>`). These mocks need to be set up to return other mocks (e.g., `IPage` mock returns `ILocator` mock) and simulate method calls (e.g., `GotoAsync`, `ClickAsync`, `TextContentAsync`).
    * `Mock<ILogger<BrowserService>>`.

## 4. Maintenance Notes & Troubleshooting

* **Browser Library Mocking:** Mocking the interfaces of libraries like Playwright can be involved due to the chained nature of calls (Browser -> Context -> Page -> Locator -> Action). Ensure the mock setup correctly reflects the expected call sequence and return values/exceptions for each test scenario. Refer extensively to the library's interface documentation.
* **Selectors:** Changes to CSS or XPath selectors used within `BrowserService` logic will require updating the corresponding arguments in test mock verifications (`It.Is<string>(s => s == ".my-selector")`).
* **Library Updates:** Updates to the browser automation library might introduce breaking changes to its interfaces, requiring significant test updates.

## 5. Test Cases & TODOs

### `BrowserServiceTests.cs`
*(Examples assume Playwright interfaces)*

* **TODO (`NavigateAsync`):** Test calls `_page.GotoAsync` with correct URL and options. Test handles success/failure response from mock.
* **TODO (`GetPageHtmlAsync`):** Test calls `_page.ContentAsync`. Test returns content from mock. Test handles exception from mock.
* **TODO (`GetElementTextAsync`):** Test calls `_page.Locator(selector).TextContentAsync`. Test returns text from mock. Test handles locator not found (e.g., mock throws or returns null).
* **TODO (`ClickAsync`):** Test calls `_page.Locator(selector).ClickAsync`. Test handles success. Test handles locator not found or click error (mock throws).
* **TODO (`TypeTextAsync`):** Test calls `_page.Locator(selector).FillAsync` with correct text. Test handles success/failure.
* **TODO (Waiting):** Test calls to `_page.WaitForSelectorAsync` or `_page.WaitForNavigationAsync` with correct parameters/options. Test handles timeout exception from mock.
* **TODO (Error Handling):** Test various methods when underlying mocked Playwright calls throw common exceptions (`TimeoutException`, `PlaywrightException`) -> verify logging and appropriate return value/exception propagation.
* **TODO (Resource Management):** Test that `IPage.CloseAsync`, `IBrowserContext.CloseAsync`, `IBrowser.CloseAsync` are called appropriately if `BrowserService` manages their lifecycle (e.g., in `DisposeAsync`).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `BrowserService` unit tests. (Gemini)

