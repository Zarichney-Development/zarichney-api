# Module/Directory: /Unit/Services/Email/TemplateService

**Last Updated:** 2025-04-18

> **Parent:** [`Email`](../README.md)
> **Related:**
> * **Source:** [`Services/Email/TemplateService.cs`](../../../../../Zarichney.Server/Services/Email/TemplateService.cs)
> * **Templates:** [`Services/Email/Templates/`](../../../../../Zarichney.Server/Services/Email/Templates/)
> * **Dependencies:** `IFileService` (potentially), `ILogger<TemplateService>`
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `TemplateService` class. This service is responsible for loading HTML email templates from the file system (or an abstraction like `IFileService`) and populating them with dynamic data.

* **Why Unit Tests?** To validate the logic for locating, reading, and populating templates in isolation from the actual file system and the email sending process. Tests ensure placeholders are correctly replaced and errors (like missing templates) are handled gracefully.
* **Isolation:** Achieved primarily by mocking file system access (e.g., via `IFileService` or direct file system mocking interfaces if applicable) and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `TemplateService` focus on its core responsibilities:

* **Template Loading (`LoadAndPopulateTemplateAsync` or similar):**
    * Constructing the correct file path for a given template name.
    * Interacting with the mocked `IFileService` (or file system abstraction) to read the template file content.
    * Handling `FileNotFoundException` or similar errors when a template doesn't exist.
    * Implementing template caching logic (if applicable).
* **Template Population:**
    * Correctly identifying and replacing placeholders (e.g., `{{PlaceholderName}}`) within the template string using provided data (e.g., a dictionary or model object).
    * Handling scenarios where provided data is missing keys expected by the template.
    * Handling scenarios where extra data is provided but not used in the template.
    * Returning the final, populated HTML string.

## 3. Test Environment Setup

* **Instantiation:** `TemplateService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<IFileService>`: Set up `ReadFileAsStringAsync` (or similar) to return specific template strings for given paths, or throw `FileNotFoundException`.
    * `Mock<ILogger<TemplateService>>`.
* **Test Data:** Sample HTML template strings and dictionaries/objects representing the data to be merged are defined within the tests.

## 4. Maintenance Notes & Troubleshooting

* **File Service Mocking:** Ensure the `IFileService` mock accurately simulates reading files based on the paths constructed by `TemplateService`. Test both success and file-not-found scenarios.
* **Placeholder Syntax:** If the placeholder syntax (e.g., `{{...}}`) changes, tests verifying replacement logic will need updating.
* **Template Content Changes:** Changes to the actual HTML templates might require updating the sample template strings used in tests if the placeholders or structure change significantly. Caching logic, if added, needs specific tests.

## 5. Test Cases & TODOs

### `TemplateServiceTests.cs`
* **TODO:** Test loading an existing template -> mock `IFileService.ReadFileAsStringAsync` returns content -> verify correct content returned by service method (before population).
* **TODO:** Test loading a non-existent template -> mock `IFileService.ReadFileAsStringAsync` throws `FileNotFoundException` -> verify service handles error appropriately (e.g., throws custom exception, returns null/empty string, logs error).
* **TODO:** Test populating a simple template with all required data -> verify all placeholders replaced correctly in the output string.
* **TODO:** Test populating a template with nested data or complex objects if supported.
* **TODO:** Test populating a template when the provided data dictionary is missing a key used as a placeholder -> verify behavior (e.g., placeholder remains, default value used, exception thrown).
* **TODO:** Test populating a template when the provided data dictionary contains extra keys not used as placeholders -> verify they are ignored and population succeeds.
* **TODO:** Test case sensitivity of placeholder replacement if relevant.
* **TODO:** Test template caching logic: Load template, load again, verify file service mock called only once. Invalidate cache (if possible) and verify reload.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `TemplateService` unit tests. Extracted from parent Email README. (Gemini)

