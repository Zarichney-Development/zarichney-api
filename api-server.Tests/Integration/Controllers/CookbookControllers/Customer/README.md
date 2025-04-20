# Module/Directory: /Integration/Controllers/CookbookControllers/Customer

**Last Updated:** 2025-04-18

> **Parent:** [`Cookbook`](../README.md) 
> *(Note: A README for `/Integration/Controllers/CookbookControllers/` may be needed)*
> **Related:**
> * **Source:** [`CookbookController.cs`](../../../../../api-server/Controllers/CookbookController.cs)
> * **Service:** [`Cookbook/Customers/CustomerService.cs`](../../../../../api-server/Cookbook/Customers/CustomerService.cs)
> * **Models:** [`Cookbook/Customers/CustomerModels.cs`](../../../../../api-server/Cookbook/Customers/CustomerModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../../../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../../../../Framework/Fixtures/CustomWebApplicationFactory.cs), [`AuthTestHelper.cs`](../../../../Framework/Helpers/AuthTestHelper.cs)

## 1. Purpose & Rationale (Why?)

This directory contains integration tests specifically for the **Customer-related endpoints** exposed by the `CookbookController`. These tests validate the behavior of actions managing customer profiles or data within the cookbook context.

* **Why Integration Tests?** To ensure these endpoints function correctly within the full ASP.NET Core pipeline, including routing, authentication/authorization, model binding, middleware interactions, and crucially, the interaction between the controller and the underlying (mocked) `ICustomerService`.
* **Why Mock Dependencies?** The primary dependency, `ICustomerService`, is typically mocked in the `CustomWebApplicationFactory`. This isolates the controller's endpoint logic from the actual data storage or complex business logic within the service layer, allowing for focused testing of the controller's request handling and response generation.

## 2. Scope & Key Functionality Tested (What?)

These tests cover endpoints related to customer management within the cookbook feature:

* **`GET /api/cookbook/customers/me`:** Retrieving the profile details for the currently authenticated user.
* **`PUT /api/cookbook/customers/me`:** Updating the profile details for the currently authenticated user.
* **`GET /api/cookbook/customers/{id}` (Admin):** Retrieving profile details for a specific customer by ID (requires Admin privileges).
* **`GET /api/cookbook/customers` (Admin):** Listing all customer profiles (requires Admin privileges).
* **Authorization:** Ensuring endpoints correctly enforce authentication and specific roles (e.g., Admin for accessing other users' data).
* **Validation:** Testing input validation for update operations.
* **Error Handling:** Verifying correct responses for scenarios like "Customer Not Found" or validation failures.

## 3. Test Environment Setup

* **Test Server:** Provided by `CustomWebApplicationFactory<Program>`.
* **Authentication:** Simulated using `TestAuthHandler` and `AuthTestHelper`. Essential for accessing `/me` endpoints and testing Admin-only endpoints.
* **Mocked Dependencies:** Configured in `CustomWebApplicationFactory`. The key mock for these tests is:
    * `Zarichnyi.ApiServer.Cookbook.Customers.ICustomerService`
    * Potentially `UserManager` if customer data is tightly linked to `IdentityUser`.

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Tests related to customer endpoints should reside within this directory (e.g., `CustomerProfileTests.cs`, `AdminCustomerManagementTests.cs`).
* **`ICustomerService` Mocking:** Ensure the mock is configured in `CustomWebApplicationFactory` to return appropriate `Customer` objects, nulls (for not found), or throw exceptions for specific test scenarios.
* **Auth Context:** Pay close attention to creating clients with the correct authentication state and roles (`Admin`) using `AuthTestHelper` when testing different access levels.
* **DTOs:** Changes to `Customer` related DTOs used in requests/responses will require test updates.

## 5. Test Cases & TODOs

### Customer Profile (`/me`) (`CustomerProfileTests.cs`)
* **TODO:** Test `GET /me` authenticated -> mock `ICustomerService.GetCustomerByUserIdAsync` success -> verify 200 OK with customer data.
* **TODO:** Test `GET /me` authenticated, customer not found -> mock `ICustomerService.GetCustomerByUserIdAsync` returns null -> verify 404 Not Found.
* **TODO:** Test `GET /me` unauthenticated -> verify 401 Unauthorized.
* **TODO:** Test `PUT /me` authenticated with valid data -> mock `ICustomerService.UpdateCustomerAsync` success -> verify 200 OK / 204 No Content.
* **TODO:** Test `PUT /me` authenticated with invalid data (e.g., invalid email format if applicable) -> verify 400 Bad Request.
* **TODO:** Test `PUT /me` authenticated, customer not found -> mock `ICustomerService.UpdateCustomerAsync` indicates failure/not found -> verify 404 Not Found.
* **TODO:** Test `PUT /me` unauthenticated -> verify 401 Unauthorized.

### Admin Management (`AdminCustomerManagementTests.cs`)
* **TODO:** Test `GET /customers/{id}` as Admin -> mock `ICustomerService.GetCustomerByIdAsync` success -> verify 200 OK with data.
* **TODO:** Test `GET /customers/{id}` as Admin, customer not found -> mock `ICustomerService.GetCustomerByIdAsync` returns null -> verify 404 Not Found.
* **TODO:** Test `GET /customers/{id}` as non-Admin -> verify 403 Forbidden.
* **TODO:** Test `GET /customers/{id}` unauthenticated -> verify 401 Unauthorized.
* **TODO:** Test `GET /customers` as Admin -> mock `ICustomerService.GetAllCustomersAsync` returns list -> verify 200 OK with list.
* **TODO:** Test `GET /customers` as non-Admin -> verify 403 Forbidden.
* **TODO:** Test `GET /customers` unauthenticated -> verify 401 Unauthorized.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for Customer-related `CookbookController` integration tests. (Gemini)

