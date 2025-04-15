# Module/Directory: /Services/Web

**Last Updated:** 2025-04-03

> **Parent:** [`/Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This module provides browser automation capabilities, primarily facilitating web scraping of dynamic websites.
* **Key Responsibilities:**
    * Defining an interface (`IBrowserService`) for interacting with web pages. [cite: api-server/Services/BrowserService.cs]
    * Providing an implementation (`BrowserService`) using the Playwright library to control a headless browser (Chromium). [cite: api-server/Services/BrowserService.cs]
    * Launching and managing the lifecycle of the Playwright browser instance.
    * Navigating to URLs, waiting for content (including JavaScript execution), handling potential elements like cookie banners, and extracting specific content based on CSS selectors.
    * Limiting concurrent page operations to manage resource usage.
* **Why it exists:** To encapsulate the complexities of browser automation and provide a simple interface for services (like `WebScraperService`) that need to interact with web pages as a user would, especially for scraping sites that heavily rely on JavaScript.

## 2. Architecture & Key Concepts

* **Core Technology:** Uses `Microsoft.Playwright` library for browser automation. [cite: api-server/Services/BrowserService.cs]
* **Implementation:** `BrowserService` implements `IBrowserService` and manages a singleton `IPlaywright` instance and an `IBrowser` instance (Chromium by default). [cite: api-server/Services/BrowserService.cs]
* **Concurrency Control:** Employs a `SemaphoreSlim` initialized with `WebscraperConfig.MaxParallelPages` to limit the number of concurrent browser pages being processed, preventing excessive resource consumption. [cite: api-server/Services/BrowserService.cs]
* **Browser Context:** Each `GetContentAsync` call creates a new, isolated `IBrowserContext` with specific options (JavaScript enabled, user agent set, viewport size defined) forgetPage interaction. [cite: api-server/Services/BrowserService.cs]
* **Page Interaction:** The `GetContentAsync` method navigates to a URL, waits for DOM content to load, optionally handles cookie consents, simulates mouse movement (potentially to trigger lazy-loaded content), waits for the target CSS selector, and extracts content (currently extracts `href` attributes from matching elements). [cite: api-server/Services/BrowserService.cs]
* **Resource Management:** Implements `IAsyncDisposable` to ensure the Playwright browser and related resources are properly closed and disposed of when the application shuts down. [cite: api-server/Services/BrowserService.cs]
* **Configuration:** Relies on `WebscraperConfig` for settings like `MaxParallelPages`, `MaxWaitTimeMs`, and potentially browser executable paths in different environments. [cite: api-server/Services/BrowserService.cs, api-server/appsettings.json]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces:** `IBrowserService` with its primary method:
    * `GetContentAsync(url, selector, cancellationToken)`: Retrieves content (currently list of hrefs) matching the selector from the given URL. [cite: api-server/Services/BrowserService.cs]
* **Assumptions:**
    * **Playwright Installation:** Assumes the necessary Playwright browser binaries (Chromium) are installed and accessible on the host machine where the application runs (handled via `Scripts/playwright.ps1 install` or equivalent OS package management). [cite: Docs/AwsMaintenance.md]
    * **Configuration:** Assumes `WebscraperConfig` is correctly configured and injected, providing values for `MaxParallelPages` and `MaxWaitTimeMs`. Assumes browser executable paths (`ExecutablePath`, `Channel`) are correctly set for the environment (especially Production). [cite: api-server/Services/BrowserService.cs]
    * **Network Access:** Assumes the application server has network connectivity to the target URLs being scraped.
    * **Website Behavior:** Assumes target websites load content in a way that `WaitUntil = WaitUntilState.DOMContentLoaded` and `WaitForSelectorAsync` are sufficient. Highly dynamic sites or those with aggressive anti-bot measures might require more complex interaction logic or waits.
    * **Selector Validity:** Assumes the CSS `selector` provided to `GetContentAsync` is valid and targets the desired elements.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Technology Stack:** Locked into using `Microsoft.Playwright`.
* **Resource Intensity:** Headless browser operations can be CPU and memory intensive. Concurrency is limited via `MaxParallelPages` config. [cite: api-server/Services/BrowserService.cs]
* **Environment Specifics:** Browser launch options (`_browserOptions`) differ between Development and Production (specifying executable path/channel for Linux). [cite: api-server/Services/BrowserService.cs]
* **Error Handling:** Primarily logs errors encountered during page navigation or content extraction. Returns empty lists on failure rather than throwing exceptions upwards in many cases within `GetContentAsync`. [cite: api-server/Services/BrowserService.cs]

## 5. How to Work With This Code

* **Consumption:** Inject `IBrowserService` into services requiring browser automation (like `WebScraperService`).
* **Setup:** Ensure Playwright is installed on the development/deployment machine (`playwright.ps1 install`).
* **Modifying Extraction:** To extract different data (e.g., text content instead of hrefs), the logic within the `foreach (var element in elements)` loop in `GetContentAsync` needs modification. [cite: api-server/Services/BrowserService.cs]
* **Testing:**
    * Unit testing requires extensive mocking of Playwright interfaces (`IPlaywright`, `IBrowser`, `IBrowserContext`, `IPage`, `IElementHandle`, etc.).
    * Integration testing might involve running actual Playwright instances against test websites or local HTML files, but can be slow and complex to set up reliably.
* **Common Pitfalls / Gotchas:** Playwright installation/path issues between environments. Target websites changing their structure, breaking CSS selectors. Handling CAPTCHAs or advanced anti-bot measures (currently not implemented). Timeouts (`MaxWaitTimeMs`) being too short for slow-loading pages. Headless browser resource leaks or crashes (mitigated by cleanup scripts/monitoring in production). [cite: Docs/AwsMaintenance.md, api-server/Scripts/cleanup-playwright.sh]

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Config`](../../Config/README.md): Consumes `WebscraperConfig`.
* **External Library Dependencies:**
    * `Microsoft.Playwright`: Core browser automation library.
* **Dependents (Impact of Changes):**
    * [`/Cookbook/Recipes/WebScraperService.cs`](../../Cookbook/Recipes/WebScraperService.cs): Primary consumer of `IBrowserService` for scraping recipe sites that require JavaScript.

## 7. Rationale & Key Historical Context

* **Playwright Selection:** Chosen likely for its robustness in handling modern, JavaScript-heavy websites compared to simpler HTTP clients or basic HTML parsers like AngleSharp (which is still used elsewhere for static HTML). It allows for more reliable scraping of dynamic content.
* **Abstraction (`IBrowserService`):** Provides a layer of abstraction over the Playwright API, simplifying its usage for consumers like `WebScraperService` and allowing potential future changes to the underlying automation tool with less impact.
* **Concurrency Limiting:** `SemaphoreSlim` was introduced to prevent the service from overwhelming the host system's resources by launching too many concurrent browser instances/pages.

## 8. Known Issues & TODOs

* **Resource Consumption:** Headless browsers can be resource-intensive. Monitoring and potentially more aggressive cleanup (e.g., restarting the browser instance periodically) might be necessary under heavy load. [cite: Docs/AwsMaintenance.md]
* **Error Handling:** Current error handling within `GetContentAsync` often logs warnings and returns empty results. It could potentially be enhanced to throw more specific exceptions for certain failure types if needed by the caller.
* **Selector Robustness:** Relies entirely on CSS selectors provided by consumers (via `site_selectors.json`). More resilient selection strategies (e.g., XPath, text-based selectors, visual selectors) are not used.
* **Advanced Interactions:** Does not currently handle complex scenarios like logins, multi-step forms, or sophisticated anti-bot challenges.