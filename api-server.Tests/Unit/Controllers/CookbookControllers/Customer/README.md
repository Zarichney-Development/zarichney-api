# Module/Directory: /Unit/Controllers/CookbookControllers/Customer

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md)
> *(Note: A README for `/Unit/Controllers/CookbookControllers/` may be needed)*
> **Related:**
> * **Source:** [`CookbookController.cs`](../../../../../api-server/Controllers/CookbookController.cs)
> * **Service:** [`Cookbook/Customers/ICustomerService.cs`](../../../../../api-server/Cookbook/Customers/ICustomerService.cs)
> * **Models:** [`Cookbook/Customers/CustomerModels.cs`](../../../../../api-server/Cookbook/Customers/CustomerModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests specifically for the **internal logic of the Customer-related action methods** within the `CookbookController`, isolated from the ASP.NET Core pipeline and real service implementations.

* **Why Unit Tests?** To provide fast, focused feedback on the controller's logic for handling customer data requests. They ensure the controller correctly:
    * Extracts user identifiers (e.g., from `HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)`).
    * Maps request DTOs (if any, e.g., for updates) to service method parameters.
    * Calls the appropriate methods on the mocked `ICustomerService`.
    * Handles the results (e.g., `Customer` objects, nulls, booleans) returned by the mocked service.
    * Translates service results or exceptions into the correct `ActionResult` (e.g., `OkObjectResult`, `NotFoundResult`, `ForbidResult`).
* **Why Mock Dependencies?** To isolate the controller's logic. The primary dependency `ICustomerService` (and `ILogger`) is mocked. If actions rely directly on `HttpContext` (e.g., for user ID), the context and its `User` property also need mocking.

## 2. Scope & Key Functionality Tested (What?)

These tests focus on the code *within* the Customer-related actions of `CookbookController`:

* **User Identification:** Verifying correct extraction of the current user's ID from the mocked `HttpContext.User`.
* **Service Invocation:** Ensuring the correct `ICustomerService` methods (`GetCustomerByUserIdAsync`, `UpdateCustomerAsync`, `GetCustomerByIdAsync`, `GetAllCustomersAsync`) are called with the expected parameters.
* **Result Handling:** Testing how `Customer` objects, nulls, or boolean results from the mocked `ICustomerService` are mapped to `ActionResult` types (`OkObjectResult`, `NotFoundResult`, `NoContentResult`, etc.).
* **Authorization Logic:** While full authorization is tested in integration, unit tests can verify checks within the action method if simple role checks are performed directly (though often this is left to attributes).
* **Input Mapping/Validation:** Testing the mapping of request DTOs to service parameters for update actions.

## 3. Test Environment Setup

* **Instantiation:** `CookbookController` is instantiated directly in tests.
* **Mocking:** Mocks for `Zarichnyi.ApiServer.Cookbook.Customers.ICustomerService` and `Microsoft.Extensions.Logging.ILogger<CookbookController>` must be provided.
* **HttpContext Mocking:** A mock `HttpContext` with a mock `ClaimsPrincipal` (representing the `User`) containing necessary claims (like `ClaimTypes.NameIdentifier`) is required for actions accessing user context. This context needs to be assigned to the controller's `ControllerContext`.

## 4. Maintenance Notes & Troubleshooting

* **`ICustomerService` Mocking:** Ensure mocks accurately reflect the expected return values for different scenarios (customer found, customer not found, update success/failure). Verify mock interactions (`Mock.Verify(...)`).
* **User Context:** When testing actions requiring authentication, ensure the mocked `HttpContext.User` is set up with a valid `NameIdentifier` claim.
* **DTOs/Models:** Changes to customer DTOs or the `Customer` model might require updates to tests checking mapping or returned data.

## 5. Test Cases & TODOs

### `GetMyCustomerProfile` Action (GET /me)
* **TODO:** Test user ID extraction from mocked `HttpContext`.
* **TODO:** Test `_customerService.GetCustomerByUserIdAsync` called with correct user ID.
* **TODO:** Test handling successful service result -> verify `OkObjectResult` with customer data.
* **TODO:** Test handling null service result (customer not found) -> verify `NotFoundResult`.
* **TODO:** Test handling exception from service -> verify exception propagation.

### `UpdateMyCustomerProfile` Action (PUT /me)
* **TODO:** Test user ID extraction from mocked `HttpContext`.
* **TODO:** Test mapping request DTO to service parameters.
* **TODO:** Test `_customerService.UpdateCustomerAsync` called with correct user ID and data.
* **TODO:** Test handling successful update -> verify `OkObjectResult` or `NoContentResult`.
* **TODO:** Test handling update failure (e.g., service returns false/throws) -> verify `NotFoundResult` or `BadRequestObjectResult`.

### `GetCustomerById` Action (Admin GET /customers/{id})
* **TODO:** Test `_customerService.GetCustomerByIdAsync` called with correct ID.
* **TODO:** Test handling successful service result -> verify `OkObjectResult` with customer data.
* **TODO:** Test handling null service result -> verify `NotFoundResult`.
* **TODO:** Test handling exception from service -> verify exception propagation.
* **TODO:** (Optional) If simple role check exists in method: Test role check logic using mocked `HttpContext.User`.

### `GetAllCustomers` Action (Admin GET /customers)
* **TODO:** Test `_customerService.GetAllCustomersAsync` called.
* **TODO:** Test handling successful service result (list of customers) -> verify `OkObjectResult` with list.
* **TODO:** Test handling exception from service -> verify exception propagation.
* **TODO:** (Optional) If simple role check exists in method: Test role check logic using mocked `HttpContext.User`.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for Customer-related `CookbookController` unit tests. (Gemini)

