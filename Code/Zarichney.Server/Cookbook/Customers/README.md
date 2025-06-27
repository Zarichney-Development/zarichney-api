# Module/Directory: /Cookbook/Customers

**Last Updated:** 2025-04-03

> **Parent:** [`/Cookbook`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This module manages customer data specifically related to the Cookbook Factory feature.
* **Key Responsibilities:**
    * Defining the `Customer` data model, primarily tracking email identity and available recipe credits. [cite: Zarichney.Server/Cookbook/Customers/CustomerModels.cs]
    * Providing services (`CustomerService`) to retrieve existing customers or create new ones with an initial credit allotment. [cite: Zarichney.Server/Cookbook/Customers/CustomerService.cs]
    * Handling the persistence of customer data (currently via file system). [cite: Zarichney.Server/Cookbook/Customers/CustomerRepository.cs]
    * Managing the logic for decrementing available credits upon recipe usage and incrementing credits upon purchase. [cite: Zarichney.Server/Cookbook/Customers/CustomerService.cs, Zarichney.Server/Services/Payment/PaymentService.cs]
* **Why it exists:** To isolate customer-specific data (especially recipe credits) and logic from the broader order processing or recipe generation concerns.

## 2. Architecture & Key Concepts

* **Structure:** Consists of:
    * `CustomerModels.cs`: Defines the `Customer` entity.
    * `CustomerService.cs`: Contains the primary business logic (`ICustomerService` interface and `CustomerService` implementation) and configuration class (`CustomerConfig`).
    * `CustomerRepository.cs`: Handles data persistence (`ICustomerRepository` interface and `CustomerFileRepository` implementation).
* **Data Persistence:** Uses a file-based repository (`CustomerFileRepository`) which interacts with the core `IFileService`. Each customer's data is stored in a separate JSON file. [cite: Zarichney.Server/Cookbook/Customers/CustomerRepository.cs, Zarichney.Server/Services/FileService.cs]
* **Identification:** Customers are primarily identified and stored using their email address. Filenames for storage are derived from sanitized email addresses. [cite: Zarichney.Server/Cookbook/Customers/CustomerRepository.cs]
* **Configuration:** Uses `CustomerConfig` (registered via `IConfig` pattern) to define the initial free recipe credits and the output directory for data files. [cite: Zarichney.Server/Cookbook/Customers/CustomerService.cs, Zarichney.Server/appsettings.json]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces:** `ICustomerService` is the main interface used by other modules. [cite: Zarichney.Server/Cookbook/Customers/CustomerService.cs]
    * `GetOrCreateCustomer(email)`: Retrieves customer by email or creates a new one if not found, initializing credits based on `CustomerConfig`.
    * `DecrementRecipes(customer, count)`: Reduces `AvailableRecipes` and increments `LifetimeRecipesUsed`. Does not save automatically.
    * `AddRecipes(customer, count)`: Increases `AvailableRecipes` and `LifetimePurchasedRecipes`. Saves the customer record.
    * `SaveCustomer(customer)`: Explicitly saves the customer state via the repository.
* **Assumptions:**
    * **File Service:** Assumes the injected `IFileService` correctly handles file read/write operations, including potential concurrency if multiple requests modify the same customer file simultaneously (though `FileService` aims to handle this). [cite: Zarichney.Server/Services/FileService.cs]
    * **Configuration:** Assumes `CustomerConfig` is correctly configured and injected, providing `InitialFreeRecipes` and `OutputDirectory`.
    * **Email Uniqueness:** Assumes email addresses serve as unique identifiers for customers within this system's context.
    * **Filename Safety:** Assumes the email sanitization logic (`MakeSafeFileName` in `CustomerFileRepository`) produces valid and unique filenames for the underlying filesystem. [cite: Zarichney.Server/Cookbook/Customers/CustomerRepository.cs]

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Persistence:** Strictly file-based via `CustomerFileRepository`. Data is stored under the path defined in `CustomerConfig.OutputDirectory`. [cite: Zarichney.Server/Cookbook/Customers/CustomerRepository.cs]
* **Configuration:** Requires the `CustomerConfig` section in application configuration (`appsettings.json`, etc.). [cite: Zarichney.Server/appsettings.json]
* **Filename Strategy:** Customer data files are named using a sanitized version of the email address (e.g., `user_at_example_dot_com.json`). [cite: Zarichney.Server/Cookbook/Customers/CustomerRepository.cs]

## 5. How to Work With This Code

* **Interaction:** Primarily interact with this module via the `ICustomerService` interface injected into other services (like `OrderService`, `PaymentService`).
* **Modifying Customer Data:** Changes to the `Customer` entity structure require updating `CustomerModels.cs`, potentially `CustomerRepository.cs` if serialization is affected, and adding relevant database/file schema migrations if persistence changes.
* **Testing:**
    * `CustomerService` can be unit tested by mocking `ICustomerRepository` and `ILogger`.
    * `CustomerFileRepository` can be tested by mocking `IFileService`. Integration tests might use a temporary directory.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Services/FileService.cs`](../../Services/FileService.cs) - Used by `CustomerFileRepository` for persistence.
    * [`/Config`](../../Config/README.md) - Consumes `CustomerConfig`.
* **External Library Dependencies:** None specific beyond core .NET dependencies.
* **Dependents (Impact of Changes):**
    * [`/Cookbook/Orders/OrderService.cs`](../Orders/OrderService.cs) - Consumes `ICustomerService` to check/decrement credits during order processing.
    * [`/Services/Payment/PaymentService.cs`](../../Services/Payment/PaymentService.cs) - Consumes `ICustomerService` to add credits after successful payment and process pending orders.

## 7. Rationale & Key Historical Context

* **Separation of Concerns:** Customer credit management is distinct from order processing or recipe generation, justifying its own module.
* **File-Based Storage:** Chosen initially for simplicity, allowing easy inspection and modification of individual customer data during development.
* **Service Abstraction:** `ICustomerService` decouples the business logic from the specific persistence implementation (`CustomerFileRepository`), allowing the repository to be potentially swapped (e.g., to a database implementation) with minimal changes to consuming services.

## 8. Known Issues & TODOs

* **Scalability:** The file-based storage approach might become inefficient with a very large number of customers due to filesystem limitations and the overhead of reading/writing individual files. Consider migrating to a database if scale becomes an issue.
* **Concurrency:** While `FileService` attempts to manage write concurrency, high contention on a single customer file could still potentially lead to issues, although less likely than with recipe files which might be updated more concurrently by background tasks.
* **Filename Collisions:** While unlikely with email sanitization, extremely unusual email addresses could potentially lead to filename collisions if the sanitization is not perfectly robust across all possible inputs and filesystems.