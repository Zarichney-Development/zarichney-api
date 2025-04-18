# Module/Directory: /Unit/Cookbook/Recipes/WebScraperService

**Last Updated:** 2025-04-18

> **Parent:** [`Recipes`](../README.md)
> **Related:**
> * **Source:** [`Cookbook/Recipes/WebScraperService.cs`](../../../../../api-server/Cookbook/Recipes/WebScraperService.cs)
> * **Interface:** [`Cookbook/Recipes/IWebScraperService.cs`](../../../../../api-server/Cookbook/Recipes/WebScraperService.cs) (Implicit)
> * **Dependencies:** `IBrowserService`, `IConfiguration`, `IFileService`, `ILogger<WebScraperService>`, HtmlAgilityPack / AngleSharp
> * **Configuration:** [`Config/site_selectors.json`](../../../../../api-server/Config/site_selectors.json)
> * **Models:** [`Cookbook/Recipes/RecipeModels.cs`](../../../../../api-server/Cookbook/Recipes/RecipeModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `WebScraperService` class. This service is responsible for fetching the HTML content of a given recipe URL (using `IBrowserService`), identifying the correct CSS selectors for that site (based on `site_selectors.json`), parsing the HTML, and extracting structured recipe data (title, ingredients, instructions, etc.).

* **Why Unit Tests?** To validate the core logic of the scraper – including site identification, selector loading, HTML parsing, data extraction based on selectors, and error handling – in isolation from the actual `BrowserService` implementation and live websites.
* **Isolation:** Achieved by mocking `IBrowserService` (to return predefined sample HTML strings), `IFileService` (to return predefined selector configuration JSON), `IConfiguration`, and `ILogger`. Tests operate on static sample HTML content.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `WebScraperService` focus primarily on the `ScrapeRecipeAsync(string url)` method:

* **HTML Fetching:** Verifying `IBrowserService.GetPageHtmlAsync` is called with the correct URL.
* **Selector Configuration Loading:** Verifying `IFileService.ReadFromFileAsync` (or `IConfiguration` access) is used to load `site_selectors.json`. Handling errors if the config is missing or invalid.
* **Site Identification & Selector Matching:** Verifying the logic correctly identifies the domain from the URL and selects the appropriate set of CSS selectors from the loaded configuration. Handling unsupported domains.
* **HTML Parsing & Data Extraction:** Using an HTML parsing library (like HtmlAgilityPack or AngleSharp) on the **mocked HTML content** to:
    * Extract the recipe title based on the site's title selector.
    * Extract ingredients based on the site's ingredient selectors, including parsing quantities, units, and names.
    * Extract instructions based on the site's instruction selectors, including splitting steps.
    * Extract other fields like prep time, cook time, yield, image URL, etc., based on their respective selectors.
* **Error Handling:** Testing scenarios where:
    * `IBrowserService` fails to fetch HTML.
    * `site_selectors.json` is missing or invalid.
    * The URL is for an unsupported domain.
    * Expected HTML elements/selectors are missing in the sample HTML content.

## 3. Test Environment Setup

* **Instantiation:** `WebScraperService` is instantiated directly.
* **Mocking:** Dependencies are mocked using Moq. Key mocks include:
    * `Mock<IBrowserService>`: Setup `GetPageHtmlAsync` to return specific, representative sample HTML strings for different test cases (e.g., HTML from site A, HTML from site B, HTML missing certain elements, null/exception).
    * `Mock<IConfiguration>` / `Mock<IFileService>`: Setup to provide the content of `site_selectors.json` as a string or stream.
    * `Mock<ILogger<WebScraperService>>`.
* **Test Data:** Define sample HTML strings within tests that mimic the structure of real recipe websites for which selectors exist. Define sample JSON content for `site_selectors.json`.

## 4. Maintenance Notes & Troubleshooting

* **HTML Samples:** Tests rely heavily on the accuracy and representativeness of the sample HTML strings provided by the `IBrowserService` mock. If a target website changes its structure significantly, the corresponding sample HTML and potentially the selectors in `site_selectors.json` will need updating, along with the tests.
* **Selector Configuration:** Tests are tightly coupled to the structure and content of `site_selectors.json`. Changes there require test updates.
* **HTML Parsing Library:** Ensure tests correctly use the chosen HTML parsing library (HtmlAgilityPack/AngleSharp) to query the sample HTML based on selectors.

## 5. Test Cases & TODOs

### `WebScraperServiceTests.cs` (`ScrapeRecipeAsync` method)
* **TODO (Success - Site A):** Test with URL for known site A -> mock browser returns sample HTML A, mock file service returns selectors -> verify correct selectors chosen, verify all expected fields (title, ingredients, instructions, etc.) extracted correctly from sample HTML A.
* **TODO (Success - Site B):** Test with URL for known site B -> mock browser returns sample HTML B -> verify correct selectors chosen, verify data extracted correctly from sample HTML B.
* **TODO (Parsing - Ingredients):** Test complex ingredient lines (e.g., "1 1/2 cups flour, sifted", "salt to taste") -> verify parsing logic extracts quantity/unit/name correctly.
* **TODO (Parsing - Instructions):** Test instructions with nested steps or complex formatting -> verify steps are extracted and separated correctly.
* **TODO (Missing Elements):** Test with sample HTML missing optional elements (e.g., prep time) -> verify service handles gracefully (e.g., corresponding field is null/empty).
* **TODO (Missing Required Elements):** Test with sample HTML missing required elements (e.g., ingredients list) -> verify service handles appropriately (e.g., returns null, throws exception).
* **TODO (Unsupported Site):** Test with URL whose domain is not in `site_selectors.json` -> verify service returns null or specific indicator of failure.
* **TODO (Browser Failure):** Test when mocked `IBrowserService.GetPageHtmlAsync` returns null or throws -> verify service handles error correctly.
* **TODO (Selector Config Failure):** Test when mocked `IFileService` fails to load `site_selectors.json` -> verify service handles error correctly.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `WebScraperService` unit tests. (Gemini)

