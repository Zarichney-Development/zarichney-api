# Module/Directory: /Controllers

**Last Updated:** 2025-04-14

> **Parent:** [`Server`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory defines the HTTP API endpoints for the Zarichney API application, acting as the primary interface for external clients (like the frontend application or other services).
* **Key Responsibilities:**
    * Mapping incoming HTTP requests (routes, methods, request bodies, query parameters) to specific C# action methods within controller classes (`[HttpGet]`, `[HttpPost]`, `[FromRoute]`, `[FromBody]`, etc.). [cite: api-server/Controllers/CookbookController.cs, api-server/Controllers/AuthController.cs]
    * Receiving requests for various functional areas like Authentication (`AuthController`), Cookbook Orders/Recipes (`CookbookController`), AI interactions (`AiController`), Payment processing (`PaymentController`), and general API functions (`ApiController`, `PublicController`). [cite: api-server/Controllers/AuthController.cs, api-server/Controllers/CookbookController.cs, api-server/Controllers/AiController.cs, api-server/Controllers/PaymentController.cs, api-server/Controllers/ApiController.cs, api-server/Controllers/PublicController.cs]
    * Delegating the actual business logic processing to appropriate services (e.g., `IOrderService`, `ILlmService`, `IPaymentService`) or MediatR handlers (primarily in `AuthController`). Controllers aim to be thin layers. [cite: api-server/Controllers/CookbookController.cs, api-server/Controllers/AuthController.cs]
    * Constructing appropriate HTTP responses (`Ok`, `BadRequest`, `NotFound`, `Created`, `File`, custom `ApiErrorResult`) based on the outcome of the delegated business logic. [cite: api-server/Controllers/CookbookController.cs, api-server/Controllers/AuthController.cs, api-server/Controllers/ApiErrorResult.cs]
    * Handling model binding and basic input validation.
    * Applying authorization rules using attributes (`[Authorize]`, `[Authorize(Roles = "...")]`). [cite: api-server/Controllers/AuthController.cs, api-server/Controllers/CookbookController.cs]
* **Why it exists:** To provide a clear, well-defined HTTP interface for the application's functionalities, separating the concerns of HTTP request handling from the underlying business logic implementation.

## 2. Architecture & Key Concepts

* **Pattern:** Standard ASP.NET Core Web API controllers (`[ApiController]`, inheriting `ControllerBase`). [cite: api-server/Controllers/CookbookController.cs]
* **Routing:** Uses attribute routing (`[Route("api/...")]`, `[HttpGet("{id}")]`, etc.) to map URLs to controller actions.
* **Dependency Injection:** Controllers receive dependencies (services, `IMediator`, `ILogger`) via constructor injection. [cite: api-server/Controllers/CookbookController.cs, api-server/Controllers/AuthController.cs]
* **Request/Response Handling:** Uses standard ASP.NET Core mechanisms for model binding (`[FromBody]`, `[FromQuery]`, etc.) and returning results (`IActionResult` implementations like `OkObjectResult`, `BadRequestObjectResult`, `NotFoundResult`, `FileContentResult`).
* **Error Handling:** Leverages a combination of specific `IActionResult` return types for expected errors (e.g., `BadRequest` for validation, `NotFound`) and relies on the global `ErrorHandlingMiddleware` for catching unhandled exceptions, often returning a standardized `ApiErrorResult`. [cite: api-server/Controllers/ApiErrorResult.cs, api-server/Config/ErrorHandlingMiddleware.cs]
* **API Documentation:** Uses Swagger/OpenAPI annotations (`[SwaggerOperation]`, `[ProducesResponseType]`, XML comments) to generate API documentation. [cite: api-server/Controllers/AuthController.cs, api-server/Controllers/CookbookController.cs, api-server/Program.cs] Includes a custom filter (`FormFileOperationFilter`) for handling file uploads correctly in Swagger UI. [cite: api-server/Config/FormFileOperationFilter.cs]
* **PublicController:**
    * Exposes `/api/status/config` (GET) — Returns a list of configuration item statuses for critical settings (API keys, secrets, connection strings). Useful for health checks and automation. Response: `List<ConfigurationItemStatus>` with `Name`, `Status`, and optional `Details` (never the secret value).

## 3. Interface Contract & Assumptions

* **Interface:** The HTTP API itself is the contract – the defined routes, methods, expected request formats (JSON bodies, form data, query parameters), response formats, and status codes. This is consumed by clients.
* **Assumptions:**
    * **Middleware Pipeline:** Assumes the necessary middleware components (Routing, Authentication, Authorization, Session Management, Error Handling, Logging) are configured correctly and execute in the appropriate order in `Program.cs`. [cite: api-server/Program.cs]
    * **Service Availability:** Assumes that the services injected into the controllers are correctly registered in the DI container and are functional.
    * **Client Behavior:** Assumes clients send requests matching the expected content types (`application/json`, `multipart/form-data`) and structures. Assumes clients handle standard HTTP status codes appropriately (e.g., retrying on 5xx, handling 4xx client errors). Assumes clients handle authentication (e.g., sending cookies automatically).
* **GET /api/status/config**
    * **Purpose:** Returns the status of critical configuration values (API keys, secrets, connection strings) for health checks and diagnostics.
    * **Response:** `200 OK` with `List<ConfigurationItemStatus>`. Each item has `Name`, `Status` ("Configured" or "Missing/Invalid"), and optional `Details`.
    * **Auth:** `[AllowAnonymous]` — no authentication required.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Thin Controllers:** Controllers should primarily delegate business logic to services or MediatR handlers, keeping action methods concise.
* **Standard Responses:** Use standard `IActionResult` types (`Ok`, `BadRequest`, `NotFound`, `Created`) and the custom `ApiErrorResult` for consistency.
* **Swagger Annotations:** Consistent use of `[SwaggerOperation]` and `[ProducesResponseType]` is expected for clear API documentation. XML comments on actions and models supplement this. [cite: api-server/Controllers/AuthController.cs, api-server/Controllers/CookbookController.cs]
* **Authorization:** Use `[Authorize]` attributes at the controller or action level to enforce authentication and role-based access control. [cite: api-server/Controllers/CookbookController.cs, api-server/Controllers/AuthController.cs]

## 5. How to Work With This Code

* **Adding Endpoints:** Add new action methods to existing relevant controllers or create new controller classes inheriting from `ControllerBase` and decorated with `[ApiController]` and `[Route]`. Inject necessary dependencies.
* **Modifying Endpoints:** Update action method logic, request/response models, routing attributes, authorization attributes, or Swagger annotations as needed. Ensure changes are reflected in corresponding service/handler calls.
* **Testing:**
    * Integration testing using `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>` is highly recommended to test the controller actions within the context of the middleware pipeline and DI container.
    * Unit testing controllers typically involves mocking all injected dependencies (`ILogger`, services, `IMediator`) and verifying that the correct methods are called on the mocks and the expected `IActionResult` is returned.
* **Common Pitfalls / Gotchas:** Routing conflicts. Incorrect attribute usage (`[FromBody]` vs. `[FromQuery]`). Missing authorization attributes on protected endpoints. Inconsistent error response handling. Controllers containing too much business logic instead of delegating. Outdated Swagger documentation/annotations.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Auth`](../Services/Auth/README.md) - Consumed by `AuthController` via MediatR and `[Authorize]` attributes.
    * [`/Cookbook`](../Cookbook/README.md) - Consumed by `CookbookController` via service interfaces.
    * [`/Services`](../Services/README.md) - Various services consumed by different controllers (e.g., `ILlmService`, `IPaymentService`, `IEmailService`).
    * [`/Config`](../Config/README.md) - Uses `ApiErrorResult`, influenced by middleware configured here.
* **External Library Dependencies:**
    * `Microsoft.AspNetCore.Mvc.Core`: Core ASP.NET Core MVC components.
    * `Swashbuckle.AspNetCore.Annotations`: For Swagger documentation attributes.
    * `MediatR`: Used by `AuthController` to send commands/queries.
* **Dependents (Impact of Changes):**
    * External API Clients (e.g., Frontend Application). Changes to routes, request/response models, or status codes are breaking changes for clients.
    * `Program.cs`: Maps controllers (`MapControllers`).
    * Integration tests targeting the API endpoints.

## 7. Rationale & Key Historical Context

* **Standard API Structure:** Follows conventional ASP.NET Core Web API practices for structuring controllers and actions.
* **Separation of Concerns:** Controllers are intended as a thin layer responsible for HTTP concerns, separating this from the business logic implemented in services and command handlers.
* **Swagger/OpenAPI:** Annotations and XML comments are used to facilitate automatic generation of API documentation, improving discoverability and usability for clients. [cite: api-server/Program.cs]

## 8. Known Issues & TODOs

* Review controllers for any business logic that could be further refactored into services or command handlers.
* Ensure consistent and comprehensive use of `[ProducesResponseType]` attributes for all possible success and error responses across all actions.
* Input validation using data annotations or FluentValidation could be applied more systematically to request models.