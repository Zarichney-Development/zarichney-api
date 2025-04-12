# Module/Directory: Server/Services/Auth/Models

**Last Updated:** 2025-04-03

> **Parent:** [`Server/Services/Auth`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory defines the core data structures (classes and records) used within the Authentication (`Server/Services/Auth`) module.
* **Key Responsibilities:**
    * Defining Entity Framework Core entities that map to database tables (e.g., `ApiKey`, `RefreshToken`). [cite: zarichney-api/Server/Services/Auth/Models/ApiKey.cs, zarichney-api/Server/Services/Auth/Models/RefreshToken.cs] Note: `ApplicationUser` extends `IdentityUser` and is defined directly in `UserDbContext.cs`. [cite: zarichney-api/Server/Services/Auth/UserDbContext.cs]
    * Defining Data Transfer Objects (DTOs) or result types used for command/query responses and API communication (e.g., `AuthResult`, `ApiKeyResponse`, `UserRoleInfo`). [cite: zarichney-api/Server/Services/Auth/Models/AuthResult.cs, zarichney-api/Server/Services/Auth/Commands/ApiKeyCommands.cs, zarichney-api/Server/Services/Auth/Models/Roles.cs]
* **Why it exists:** To centralize the definition of data structures related to authentication, separating them from the business logic (Commands) and database context configuration.

## 2. Architecture & Key Concepts

* **Entity Framework Core Entities:** Classes like `ApiKey` and `RefreshToken` are designed as EF Core entities using data annotations (`[Key]`, `[Required]`, `[MaxLength]`, `[ForeignKey]`) to define database schema constraints and relationships. [cite: zarichney-api/Server/Services/Auth/Models/ApiKey.cs, zarichney-api/Server/Services/Auth/Models/RefreshToken.cs] These are mapped to tables in the `UserDbContext`. [cite: zarichney-api/Server/Services/Auth/UserDbContext.cs]
* **Result/DTO Classes:** Classes like `AuthResult`, `RoleCommandResult`, and `ApiKeyResponse` serve as standardized return types for MediatR handlers and API endpoints, encapsulating operation success status, messages, and relevant data. [cite: zarichney-api/Server/Services/Auth/Models/AuthResult.cs, zarichney-api/Server/Services/Auth/Commands/RoleCommands.cs, zarichney-api/Server/Services/Auth/Commands/ApiKeyCommands.cs] `UserRoleInfo` is a simple DTO for conveying user information in role queries. [cite: zarichney-api/Server/Services/Auth/Models/Roles.cs]

## 3. Interface Contract & Assumptions

* **Interface:** The public properties of the classes and records defined here constitute their interface. Their structure dictates the shape of data stored in the database (for entities) and transferred between application layers (for DTOs/results).
* **Assumptions:**
    * Consumers (e.g., command handlers, `UserDbContext`, controllers) correctly interpret the meaning and usage of the properties defined in these models.
    * The database schema generated via migrations accurately reflects the structure and constraints defined by the EF Core entities (`ApiKey`, `RefreshToken`). [cite: zarichney-api/Server/Services/Auth/Migrations/]
    * `AuthResult` provides the standard mechanism for indicating success/failure of command operations originating from [`Server/Services/Auth/Commands`](../Commands/README.md).

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Entity Configuration:** Primary reliance on data annotations for EF Core configuration within the model classes themselves. Further configuration (e.g., indexes, cascade delete behavior) is done using Fluent API within `UserDbContext.OnModelCreating`. [cite: zarichney-api/Server/Services/Auth/UserDbContext.cs]
* **Location:** Contains only data structure definitions relevant to the Auth module.

## 5. How to Work With This Code

* **Adding/Modifying Entities:**
    1. Define or modify the C# class here.
    2. Use data annotations (`[Required]`, etc.) for common constraints.
    3. Update the corresponding `DbSet` property in `UserDbContext.cs`. [cite: zarichney-api/Server/Services/Auth/UserDbContext.cs]
    4. Add any necessary Fluent API configuration in `UserDbContext.OnModelCreating`.
    5. **Crucially, add a new EF Core migration** using `dotnet ef migrations add ...` to reflect the schema change. [cite: zarichney-api/Server/Services/Auth/Migrations/]
* **Adding/Modifying DTOs/Results:** Define or modify the C# class/record here. Ensure consuming code (command handlers, controllers) is updated accordingly.
* **Testing:** Models themselves usually don't require complex unit tests unless they contain logic. Testing primarily occurs in the layers that consume these models.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Server/Services/Auth/UserDbContext.cs`](../UserDbContext.cs) - Configures the EF Core mapping for entities defined here.
* **External Library Dependencies:**
    * `System.ComponentModel.DataAnnotations`: Used for EF Core data annotations.
    * `Microsoft.AspNetCore.Identity.EntityFrameworkCore`: `ApplicationUser` inherits from `IdentityUser`. [cite: zarichney-api/Server/Services/Auth/UserDbContext.cs]
* **Dependents (Impact of Changes):**
    * [`Server/Services/Auth/Commands/*`](../Commands/README.md) - Consume and produce instances of these models (e.g., `AuthResult`, `ApiKeyResponse`).
    * [`Server/Services/Auth/UserDbContext.cs`](../UserDbContext.cs) - References entity types (`ApiKey`, `RefreshToken`) for `DbSet` properties and configuration.
    * [`Server/Services/Auth/Migrations/*`](../Migrations/README.md) - Migrations are generated based on changes to the entity models defined here. Modifying entities necessitates new migrations.
    * [`Server/Controllers/AuthController.cs`](../../../Controllers/AuthController.cs) - Uses result types (`AuthResult`, `ApiKeyResponse`, `RoleCommandResult`) for API responses.
    * Any service interacting directly with `UserDbContext` relies on the entity structures.

## 7. Rationale & Key Historical Context

* **Separation:** Placing data models in a distinct directory enhances organization and separates data structure definitions from business logic and database interaction code.
* **EF Core Code-First:** Using annotated entity classes enables the code-first approach to database schema management via EF Core Migrations.
* **Standardized Results (`AuthResult`):** Provides a consistent way for command handlers to report success/failure and associated messages back to the controller layer.

## 8. Known Issues & TODOs

* Consider adding XML documentation comments to properties within the models for improved clarity and IntelliSense.
* The `ApiKeyResponse` includes the `KeyValue`. While necessary when initially creating the key, care must be taken not to expose this property in contexts where only metadata should be shown (e.g., listing multiple keys). A separate DTO might be warranted for listing operations.