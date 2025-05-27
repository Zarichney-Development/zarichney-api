# Module/Directory: /Unit/Services/Email/EmailService

**Last Updated:** 2025-04-18

> **Parent:** [`Email`](../README.md)
> **Related:**
> * **Source:** [`Services/Email/EmailService.cs`](../../../../../api-server/Services/Email/EmailService.cs)
> * **Dependencies:** `ITemplateService`, Email Provider SDK/Framework/Client (e.g., `ISendGridClient`, `IMailKitClient`, `ISmtpClient`), `IOptions<EmailSettings>`, `ILogger<EmailService>`
> * **Models:** [`Services/Email/EmailModels.cs`](../../../../../api-server/Services/Email/EmailModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `EmailService` class. This service is responsible for orchestrating the sending of emails, typically involving fetching and populating a template (via `ITemplateService`), constructing the email message, and using an external email provider's SDK or client to perform the actual sending.

* **Why Unit Tests?** To validate the logic of constructing and attempting to send emails in isolation from the `TemplateService` implementation and the actual external email delivery system. Tests ensure the service correctly uses configuration, calls the template service, builds the email object, interacts with the (mocked) sender client, and handles success/failure responses from the client.
* **Isolation:** Achieved by mocking `ITemplateService`, the specific email sender client interface/wrapper (e.g., `ISendGridClient`), `IOptions<EmailSettings>`, and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `EmailService` focus on its core responsibilities, primarily within methods like `SendEmailAsync`:

* **Template Service Interaction:** Verifying that `ITemplateService.LoadAndPopulateTemplateAsync` (or similar) is called with the correct template name and data model.
* **Configuration Usage:** Ensuring that settings from the mocked `IOptions<EmailSettings>` (e.g., "From" address, sender name) are correctly used when constructing the email message.
* **Email Message Construction:** Verifying that the recipient address(es), subject, and the populated body (from the mocked `ITemplateService`) are correctly assembled into the object/parameters expected by the sender client.
* **Sender Client Interaction:** Ensuring the correct method on the mocked email sender client (e.g., `SendEmailAsync`) is invoked with the fully constructed email message/parameters.
* **Error Handling:** Testing how exceptions or failure indicators returned by the mocked `ITemplateService` or the mocked sender client are handled (e.g., logged, exceptions propagated or swallowed).

## 3. Test Environment Setup

* **Instantiation:** `EmailService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<ITemplateService>`: Set up to return specific populated HTML strings for given template names/data, or throw exceptions.
    * `Mock<IEmailSenderClient>`: (Using a generic name here - replace with the actual interface/class being used, e.g., `ISendGridClient`, `ISmtpClientWrapper`). Mock the specific `Send` method to simulate success or failure (e.g., throw an exception).
    * `Mock<IOptions<EmailSettings>>`: Provide mock `EmailSettings` values.
    * `Mock<ILogger<EmailService>>`.

## 4. Maintenance Notes & Troubleshooting

* **Sender Client Mocking:** Mocking the specific email sending client requires understanding its API (methods, arguments, return types, exceptions). Simulate both successful sending and various failure modes.
* **Dependency on `ITemplateService`:** Ensure the `ITemplateService` mock returns realistic populated HTML for the `EmailService` to use when constructing the final message for the sender client mock.
* **Configuration:** Ensure the mocked `EmailSettings` provide necessary values like the "From" address used in tests.

## 5. Test Cases & TODOs

### `EmailServiceTests.cs`
* **TODO (`SendEmailAsync` - Success):** Test sending a specific email type (e.g., password reset).
    * Verify `_templateService.LoadAndPopulateTemplateAsync` called with correct template name and data.
    * Verify sender client's `Send` method called with correct 'To', 'From' (from settings), 'Subject', and populated 'Body'.
    * Mock sender client `Send` success -> verify method completes without error.
* **TODO (`SendEmailAsync` - Template Failure):** Test when `_templateService.LoadAndPopulateTemplateAsync` throws exception -> verify exception handled/logged, verify sender client *not* called.
* **TODO (`SendEmailAsync` - Sender Failure):** Test when sender client's `Send` method throws exception -> verify exception handled/logged.
* **TODO (`SendEmailAsync` - Different Email Types):** Add tests for other logical email types (e.g., email verification, cookbook ready) to ensure correct templates, subjects, and potentially different 'From' addresses or configurations are used.
* **TODO (`SendEmailAsync` - Input Validation):** Test with invalid recipient email address (if validation occurs in `EmailService`).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `EmailService` unit tests. Extracted from parent Email README. (Gemini)

