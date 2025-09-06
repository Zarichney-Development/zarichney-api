# Module/Directory: /Unit/Services/PdfGeneration

**Last Updated:** 2025-04-18

> **Parent:** [`Services`](../README.md)
> *(Note: A README for `/Unit/Services/` may be needed)*
> **Related:**
> * **Source:** [`Services/PdfGeneration/PdfCompiler.cs`](../../../../Zarichney.Server/Services/PdfGeneration/PdfCompiler.cs)
> * **Dependencies:** `IOrderService`, `IRecipeService`, `IFileService` (potentially for images), `ILogger<PdfCompiler>`, QuestPDF library
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Standards/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `PdfCompiler` service. This service is responsible for taking application data (like cookbook orders and recipes) and using a PDF generation library (like QuestPDF) to create a PDF document.

* **Why Unit Tests?** To validate the logic of the `PdfCompiler` in isolation from its data-providing dependencies and the intricacies of the PDF rendering engine. Tests focus on ensuring the service:
    * Correctly fetches the required data (via mocked services like `IOrderService`, `IRecipeService`).
    * Handles cases where required data is missing.
    * Correctly structures the document definition passed to the PDF library (e.g., verifying that appropriate QuestPDF components are used or configured based on input data, where feasible).
    * Invokes the final PDF generation method of the library.
* **Isolation & Scope:** Achieved by mocking data-fetching services (`IOrderService`, `IRecipeService`, `IFileService`). Importantly, these tests **do not** typically validate the pixel-perfect visual output or binary content of the generated PDF. They verify the *preparation* and *invocation* of the PDF generation process.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `PdfCompiler` focus primarily on its main compilation method (e.g., `CompileCookbookAsync`):

* **Data Fetching:** Verifying that methods on mocked `IOrderService`, `IRecipeService`, etc., are called with the correct identifiers (e.g., order ID) to retrieve the necessary data for the PDF.
* **Data Handling:** Testing how the service handles scenarios where dependencies return null or empty data (e.g., order not found, no recipes).
* **Document Definition:** Verifying (where practical) that the data retrieved from mocks is correctly processed and used to configure the document structure passed to the PDF library. This might involve checking arguments passed to specific setup methods if the PDF library's API allows for it, or simply ensuring the main generation path is reached with valid inputs.
* **PDF Library Invocation:** Ensuring the final PDF generation method (e.g., QuestPDF's `GeneratePdf(stream)` or `GeneratePdf()`) is called. Tests might assert that the returned stream/byte array is not null or empty, or that the method completes without throwing exceptions under normal conditions.
* **Error Handling:** Testing how exceptions from mocked data services or potential exceptions during the PDF generation call itself are handled.

## 3. Test Environment Setup

* **Instantiation:** `PdfCompiler` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<IOrderService>`
    * `Mock<IRecipeService>`
    * `Mock<IFileService>` (if used, e.g., for loading images referenced in recipes)
    * `Mock<ILogger<PdfCompiler>>`
* **PDF Library Usage:** The tests will invoke methods from the PDF library (e.g., QuestPDF), but assertions focus on the inputs to and successful invocation of these methods, rather than their detailed output.

## 4. Maintenance Notes & Troubleshooting

* **Focus on Logic, Not Output:** Remember these tests verify data gathering and the setup for PDF generation. Visual testing or snapshot testing might be needed separately if pixel-perfect output validation is required, but that's outside the scope of these unit tests.
* **Data Service Mocks:** Ensure mocks for `IOrderService`, `IRecipeService`, etc., return realistic data structures (or nulls) needed by `PdfCompiler` for different test scenarios.
* **PDF Library API:** Changes to the PDF library's API (e.g., QuestPDF method signatures or document definition structure) may require test updates.

## 5. Test Cases & TODOs

### `PdfCompilerTests.cs`
*(Assuming a method like `CompileCookbookAsync(int orderId)`)*

* **TODO:** Test successful compilation path:
    * Mock `IOrderService.GetOrderByIdAsync` returns valid order with recipe IDs.
    * Mock `IRecipeService.GetRecipeByIdAsync` returns valid recipes for IDs.
    * Mock `IFileService` calls if images are loaded.
    * Verify the QuestPDF `GeneratePdf` (or equivalent) method is invoked.
    * Verify the method returns a non-null, potentially non-empty byte array or stream.
* **TODO:** Test scenario where Order is not found -> mock `IOrderService.GetOrderByIdAsync` returns null -> verify appropriate exception thrown or error result returned.
* **TODO:** Test scenario where a required Recipe is not found -> mock `IRecipeService.GetRecipeByIdAsync` returns null for an ID -> verify handling (e.g., skip recipe, throw exception).
* **TODO:** Test scenario where Order has no associated recipes -> verify handling (e.g., generate PDF with specific message, throw exception).
* **TODO:** Test handling of exception thrown by mocked `IOrderService`.
* **TODO:** Test handling of exception thrown by mocked `IRecipeService`.
* **TODO:** Test handling of exception potentially thrown by the PDF library's generation method (if possible to simulate via setup).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `PdfCompiler` unit tests, clarifying focus on logic over output validation. (Gemini)

