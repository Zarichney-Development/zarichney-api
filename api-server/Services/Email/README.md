# Module/Directory: /Services/Email

**Last Updated:** 2025-04-14

> **Parent:** [`/Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This module handles all email-related operations for the application, including composing emails from templates, sending them via Microsoft Graph, and validating email addresses.
* **Key Responsibilities:**
    * Defining configuration (`EmailConfig`) and models (`EmailValidationResponse`, `InvalidEmailException`) related to email services. [cite: api-server/Services/Email/EmailModels.cs]
    * Providing a service (`IEmailService`) for sending emails, potentially with attachments, using Microsoft Graph API. [cite: api-server/Services/Email/EmailService.cs]
    * Providing a service (`IEmailService`) for validating email address syntax and deliverability using the external MailCheck API. [cite: api-server/Services/Email/EmailService.cs]
    * Providing a service (`ITemplateService`) for loading HTML email templates (from the `Templates/` directory) and rendering them using Handlebars.Net templating engine. [cite: api-server/Services/Email/TemplateService.cs]
* **Why it exists:** To centralize email functionality, abstracting the details of template rendering, email sending (via MS Graph), and validation away from the core business logic modules (like Auth or Cookbook).

## 2. Architecture & Key Concepts

* **Email Sending:** `EmailService` utilizes the `Microsoft.Graph.GraphServiceClient` SDK to authenticate and send emails via the Microsoft Graph API, using credentials specified in `EmailConfig`. [cite: api-server/Services/Email/EmailService.cs, api-server/Program.cs]
* **Template Rendering:** `TemplateService` uses `Handlebars.Net` to process `.html` files located in the directory specified by `EmailConfig.TemplateDirectory`. It compiles a base template (`base.html`) and specific content templates (e.g., `email-verification.html`), merging provided data context before returning the final HTML content. [cite: api-server/Services/Email/TemplateService.cs, api-server/Services/Email/Templates/base.html]
* **Email Validation:** `EmailService` uses `RestSharp` to make requests to the external MailCheck API (`mailcheck.p.rapidapi.com`) using the API key from `EmailConfig`. It caches validation results per domain using `IMemoryCache` to reduce redundant API calls. It throws an `InvalidEmailException` if validation fails certain criteria (invalid, blocked, disposable, high risk). [cite: api-server/Services/Email/EmailService.cs, api-server/Services/Email/EmailModels.cs]
* **Configuration:** `EmailConfig` (registered via `IConfig`) holds Azure App credentials, the sending email address (`FromEmail`), the MailCheck API key, and the template directory path. [cite: api-server/Services/Email/EmailModels.cs, api-server/appsettings.json]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces:**
    * `IEmailService`: Defines `SendEmail` and `ValidateEmail` methods. [cite: api-server/Services/Email/EmailService.cs]
    * `ITemplateService`: Defines `ApplyTemplate` method. [cite: api-server/Services/Email/TemplateService.cs]
* **Runtime Configuration Exceptions:**
    * `EmailService.SendEmail()` will throw `ConfigurationMissingException` if the `GraphServiceClient` is null due to missing Azure credentials (Tenant ID, App ID, App Secret, or FromEmail).
    * `EmailService.ValidateEmail()` will throw `ConfigurationMissingException` if the `MailCheckApiKey` is missing or invalid.
    * These exceptions are thrown at runtime when the methods are called, rather than at startup, allowing the application to start with partial functionality even when some configuration is missing.
* **Assumptions:**
    * **Configuration:** Assumes `EmailConfig` contains valid and functional credentials for Microsoft Graph (Azure Tenant/App ID/Secret) and the MailCheck API key. Assumes `FromEmail` is a valid address authorized to send via the configured Graph App. Assumes `TemplateDirectory` path is correct relative to the application root. [cite: api-server/Services/Email/EmailModels.cs]
    * **GraphServiceClient Availability:** The injected `GraphServiceClient` might be `null` if the required configuration is missing or invalid. The `EmailService` will need to handle this possibility appropriately by checking for null before using the client. [cite: api-server/Startup/ServiceStartup.cs]
    * **External Services:** Relies on the availability and correct functioning of Microsoft Graph API and the MailCheck API.
    * **Network Connectivity:** Requires network access to Microsoft Graph and MailCheck endpoints.
    * **Templates:** Assumes the `Templates` directory exists and contains valid Handlebars `.html` files, including `base.html`. Assumes templates requested by `SendEmail` exist. [cite: api-server/Services/Email/TemplateService.cs]
    * **File System Access:** `TemplateService` assumes read access to the `Templates` directory via `IFileService`. [cite: api-server/Services/Email/TemplateService.cs]

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Email Provider:** Tied specifically to Microsoft Graph API for sending. Changing providers would require significant changes to `EmailService`.
* **Validation Provider:** Tied specifically to MailCheck API via RapidAPI for validation.
* **Templating Engine:** Uses Handlebars.Net. Templates must adhere to Handlebars syntax. [cite: api-server/Services/Email/TemplateService.cs]
* **Template Location:** Templates are expected to be in the `/Services/Email/Templates/` directory relative to the application root (or as configured). [cite: api-server/Services/Email/EmailModels.cs]
* **Caching:** Email validation results are cached in memory (`IMemoryCache`). [cite: api-server/Services/Email/EmailService.cs]

## 5. How to Work With This Code

* **Sending an Email:**
    1. Inject `IEmailService`.
    2. Create a `Dictionary<string, object>` with data needed by the Handlebars template.
    3. Call `_emailService.SendEmail(recipient, subject, templateName, templateData, attachment?)`.
* **Adding/Modifying Templates:**
    1. Add/Edit `.html` files in the `/Services/Email/Templates/` directory.
    2. Use Handlebars syntax (`{{variable}}`, `{{#if condition}}`, etc.) for dynamic content.
    3. Ensure the template integrates with `base.html` correctly (uses `{{{content}}}`).
* **Testing:**
    * Mock `GraphServiceClient`, `RestClient` (or a wrapper/`IHttpClientFactory`), `ITemplateService`, `IMemoryCache`, and `IFileService`.
    * Test `TemplateService` by providing mock template content via a mocked `IFileService`.
    * Test `EmailService` logic by verifying calls to mocked clients and cache. End-to-end testing requires actual credentials and careful handling (e.g., sending to test inboxes).
* **Common Pitfalls / Gotchas:** Invalid Azure/MailCheck credentials. Network connectivity issues. Email deliverability problems (SPF/DKIM/DMARC not configured correctly for the sending domain). Errors in Handlebars template syntax. MailCheck API rate limits or changes. `GraphServiceClient` permission issues.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Config`](../../Config/README.md): Consumes `EmailConfig`.
    * [`/Services/FileSystem`](../FileSystem/README.md): Consumed by `TemplateService` (via injected `IFileService`).
* **External Library Dependencies:**
    * `Microsoft.Graph`: SDK for Microsoft Graph API interactions.
    * `RestSharp`: Used for making HTTP requests to the MailCheck API.
    * `Handlebars.Net`: Templating engine for HTML emails.
    * `Microsoft.Extensions.Caching.Memory`: For caching email validation results.
    * `Azure.Identity`: Used for authenticating `GraphServiceClient`. [cite: api-server/Program.cs]
* **Dependents (Impact of Changes):**
    * [`/Auth`](../Auth/README.md): Various command handlers (Register, ForgotPassword, ResetPassword, ResendConfirmation) consume `IEmailService`.
    * [`/Cookbook/Orders`](../../Cookbook/Orders/README.md): `OrderService` consumes `IEmailService` to send notifications and the final cookbook.
    * [`/Controllers/ApiController.cs`](../../Controllers/ApiController.cs): Consumes `IEmailService` for the `email/validate` endpoint.
    * `Program.cs`: Registers `EmailService`, `TemplateService`, `GraphServiceClient`.

## 7. Rationale & Key Historical Context

* **Centralization:** Grouping all email-related tasks (sending, templating, validation) into one module simplifies management.
* **Microsoft Graph:** Chosen as the email sending provider, likely due to potential integration with other Microsoft/Azure services or existing infrastructure.
* **Handlebars:** Selected for its flexibility and common usage in HTML templating.
* **Email Validation:** Included proactively to improve data quality for user registration and reduce email bounces.
* **Template Service:** `ITemplateService` decouples the specifics of template loading and rendering (Handlebars) from the `EmailService`'s responsibility of sending emails.

## 8. Known Issues & TODOs

* The dependency on the external MailCheck API for validation introduces potential costs, rate limits, and reliance on a third-party service's availability and accuracy.
* Email deliverability is highly dependent on external factors like sender reputation, DNS records (SPF, DKIM, DMARC), and recipient server policies, which are outside the scope of this module's code.
* Error handling for specific `Microsoft.Graph.Models.ODataErrors.ODataError` exceptions could be more granular in `EmailService`.
* Consider adding integration tests that use a mock SMTP server or a dedicated test email service account.