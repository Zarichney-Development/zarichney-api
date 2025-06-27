# Module/Directory: /Unit/Cookbook/Customers/CustomerService

**Last Updated:** 2025-04-18

> **Parent:** [`Customers`](../README.md)
> **Related:**
> * **Source:** [`Cookbook/Customers/CustomerService.cs`](../../../../../Zarichney.Server/Cookbook/Customers/CustomerService.cs)
> * **Interface:** [`Cookbook/Customers/ICustomerService.cs`](../../../../../Zarichney.Server/Cookbook/Customers/CustomerService.cs) (Implicit)
> * **Dependencies:** `ICustomerRepository`, `UserManager<IdentityUser>` (potentially), `ILogger<CustomerService>`
> * **Models:** [`Cookbook/Customers/CustomerModels.cs`](../../../../../Zarichney.Server/Cookbook/Customers/CustomerModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `CustomerService` class. This service encapsulates the business logic and orchestration related to customer operations within the cookbook feature, acting as an intermediary between controllers/other services and the `CustomerRepository`.

* **Why Unit Tests?** To validate the service's logic (e.g., retrieving customer profiles, handling updates, applying business rules) in isolation from the data access layer (`ICustomerRepository`) and Identity framework (`UserManager`).
* **Isolation:** Achieved by mocking `ICustomerRepository`, `UserManager` (if used), and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `CustomerService` focus on its public methods:

* **`GetCustomerByUserIdAsync(string userId)`:**
    * Verifying `_customerRepository.GetCustomerByUserIdAsync` is called with the correct ID.
    * Handling the `Customer` object or null returned by the mocked repository.
* **`GetCustomerByIdAsync(int customerId)`:**
    * Verifying `_customerRepository.GetCustomerByIdAsync` is called with the correct ID.
    * Handling found/not found scenarios based on mocked repository response.
* **`GetAllCustomersAsync()`:**
    * Verifying `_customerRepository.GetAllCustomersAsync` is called.
    * Handling the list of `Customer` objects returned by the mocked repository.
* **`UpdateCustomerAsync(Customer customer)` / `CreateCustomerAsync(...)`:**
    * Performing any validation or business logic before calling the repository.
    * Verifying `_customerRepository.UpdateCustomerAsync` or `_customerRepository.AddCustomerAsync` is called with the correct customer object.
    * Handling success/failure results from the mocked repository call.
* **Interaction with `UserManager`:** (If applicable) Verifying calls to mocked `UserManager` for retrieving user details related to customers.

## 3. Test Environment Setup

* **Instantiation:** `CustomerService` is instantiated directly.
* **Mocking:** Dependencies are mocked using Moq. Key mocks include:
    * `Mock<ICustomerRepository>`: Setup methods (`GetCustomerByUserIdAsync`, `UpdateCustomerAsync`, etc.) to return specific `Customer` objects, lists, nulls, or throw exceptions.
    * `Mock<UserManager<IdentityUser>>`: If used. Requires mocking `IUserStore`. Mock relevant methods like `FindByIdAsync`.
    * `Mock<ILogger<CustomerService>>`.

## 4. Maintenance Notes & Troubleshooting

* **Repository Mocking:** Ensure `ICustomerRepository` mocks accurately simulate database responses needed to test different logic paths within the service.
* **Business Logic:** Focus tests on any specific business rules implemented within the service layer itself, beyond simple calls to the repository.
* **User Manager Mocking:** If `UserManager` is used, ensure its mock setup is correct.

## 5. Test Cases & TODOs

### `CustomerServiceTests.cs`
* **TODO (`GetCustomerByUserIdAsync`):** Test found -> mock repo returns customer -> verify customer returned.
* **TODO (`GetCustomerByUserIdAsync`):** Test not found -> mock repo returns null -> verify null returned.
* **TODO (`GetCustomerByIdAsync`):** Test found -> mock repo returns customer -> verify customer returned.
* **TODO (`GetCustomerByIdAsync`):** Test not found -> mock repo returns null -> verify null returned.
* **TODO (`GetAllCustomersAsync`):** Test returns list from mock repo.
* **TODO (`UpdateCustomerAsync`):** Test success -> verify `_customerRepository.UpdateCustomerAsync` called, verify success result returned.
* **TODO (`UpdateCustomerAsync`):** Test failure (e.g., repo throws exception) -> verify exception handled/propagated.
* **TODO (`CreateCustomerAsync`):** Test success -> verify `_customerRepository.AddCustomerAsync` called, verify success result returned.
* **TODO:** Add tests for any specific business logic or validation within the service methods.
* **TODO:** Add tests for interactions with mocked `UserManager` if applicable.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `CustomerService` unit tests. (Gemini)

