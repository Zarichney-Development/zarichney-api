# Module/Directory: Server/Services/Auth/Migrations

**Last Updated:** 2025-04-03

> **Parent:** [`Server/Services/Auth`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory houses the Entity Framework Core (EF Core) code-first database migrations specifically for the `UserDbContext`. [cite: zarichney-api/Server/Services/Auth/UserDbContext.cs]
* **Key Responsibilities:**
   * Defining the incremental changes required to evolve the PostgreSQL database schema to match the entity models defined in [`Server/Services/Auth/Models`](../Models/README.md).
   * Maintaining a history of schema changes (`__EFMigrationsHistory` table in the database).
   * Providing the mechanism (`Up` and `Down` methods in each migration file) for applying or reverting schema changes. [cite: zarichney-api/Server/Services/Auth/Migrations/20250330163155_InitialCreate.cs]
   * Includes helper scripts (`ApplyMigrations.sh`, `ApplyMigrations.ps1`) for managing migration application. [cite: zarichney-api/Server/Services/Auth/Migrations/ApplyMigrations.sh, zarichney-api/Server/Services/Auth/Migrations/ApplyMigrations.ps1]
* **Why it exists:** To manage database schema evolution in a version-controlled, repeatable, and automated manner alongside the application code, ensuring consistency between the code's entity models and the database structure.

## 2. Architecture & Key Concepts

* **Technology:** Entity Framework Core Code-First Migrations. [cite: zarichney-api/Docs/PostgreSqlMaintenance.md]
* **Structure:**
   * Each `.cs` file (e.g., `20250330163155_InitialCreate.cs`) represents a single, ordered migration. It contains `Up()` (apply changes) and `Down()` (revert changes) methods with EF Core Fluent API or SQL commands. [cite: zarichney-api/Server/Services/Auth/Migrations/20250330163155_InitialCreate.cs]
   * The corresponding `.Designer.cs` file is auto-generated metadata used by EF Core.
   * `UserDbContextModelSnapshot.cs` is an auto-generated representation of the complete database model *after* all migrations in this folder have been applied. It's used by EF Core to determine the changes needed for the *next* migration. [cite: zarichney-api/Server/Services/Auth/Migrations/UserDbContextModelSnapshot.cs]
* **Workflow:**
   1.  **Development:** Changes are made to entity classes in [`Server/Services/Auth/Models`](../Models/README.md) or configurations in `UserDbContext`.
   2.  **Generation:** The `dotnet ef migrations add <MigrationName>` command is used locally. It compares the current model snapshot with the updated code model to generate a new migration file (`.cs` and `.Designer.cs`) and updates the snapshot. [cite: zarichney-api/Docs/PostgreSqlMaintenance.md]
   3.  **Local Application:** `dotnet ef database update` applies pending migrations directly to the local development database. [cite: zarichney-api/Docs/PostgreSqlMaintenance.md]
   4.  **Production Application:** The CI/CD pipeline generates an *idempotent SQL script* (`dotnet ef migrations script --idempotent`). This script is deployed to the server and executed by `ApplyMigrations.sh` using `psql`, ensuring only necessary changes are applied based on the `__EFMigrationsHistory` table in the database. [cite: zarichney-api/Server/Services/Auth/Migrations/ApplyMigrations.sh, zarichney-api/Docs/PostgreSqlMaintenance.md, zarichney-api/Docs/AwsMaintenance.md]

## 3. Interface Contract & Assumptions

* **Interface:** The primary "interface" is the database schema that results from applying these migrations sequentially. Other parts of the application, particularly `UserDbContext` and repository/service layers interacting with it, rely on this schema being correctly applied.
* **Assumptions:**
   * **Database Availability:** Assumes a target PostgreSQL database exists and is accessible via the connection string used by `UserDbContextFactory` (for design-time tools) or the application runtime. [cite: zarichney-api/Server/Services/Auth/UserDbContextFactory.cs]
   * **Tooling:** Assumes `.NET SDK` and `dotnet ef` tools are available in the development environment for generating migrations. Assumes `psql` command-line tool is available in the deployment environment for production script execution. [cite: zarichney-api/Docs/PostgreSqlMaintenance.md]
   * **Migration Order:** Assumes migrations are applied in the chronological order defined by their timestamped filenames. EF Core manages this automatically based on the `__EFMigrationsHistory` table.
   * **Snapshot Integrity:** Assumes the `UserDbContextModelSnapshot.cs` file accurately reflects the state of the model *after* the latest migration file was generated. Merge conflicts in this file must be resolved carefully.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Target Context:** All migrations in this folder target the `UserDbContext`.
* **Naming Convention:** `YYYYMMDDHHMMSS_MigrationName.cs`. The timestamp ensures correct ordering.
* **Production Deployment:** Strictly relies on the generated idempotent SQL script and the `ApplyMigrations.sh` runner. Direct `dotnet ef database update` is *not* used in production. [cite: zarichney-api/Server/Services/Auth/Migrations/ApplyMigrations.sh, zarichney-api/Docs/AwsMaintenance.md]
* **Modifying Migrations:** Avoid modifying migration files (`.cs`) *after* they have been applied to any environment (especially production). If a change is needed, create a *new* migration to correct the schema.

## 5. How to Work With This Code

* **Adding a New Migration:**
   1. Make necessary changes to entity classes in [`Server/Services/Auth/Models`](../Models/README.md) or `UserDbContext`.
   2. Ensure your local database schema is up-to-date with existing migrations (`dotnet ef database update --context UserDbContext`).
   3. Run `dotnet ef migrations add YourMigrationName --context UserDbContext --output-dir Server/Services/Auth/Migrations`. [cite: zarichney-api/Docs/PostgreSqlMaintenance.md]
   4. Review the generated `.cs` file for correctness.
   5. Commit the new `.cs` file, the associated `.Designer.cs` file, and the updated `UserDbContextModelSnapshot.cs`.
* **Applying Migrations Locally:** Run `dotnet ef database update --context UserDbContext`.
* **Applying Migrations in Production:** This is handled automatically by the deployment pipeline via `ApplyMigrations.sh`. [cite: zarichney-api/Server/Services/Auth/Migrations/ApplyMigrations.sh]
* **Reverting Migrations Locally (Use with Caution):** Run `dotnet ef database update <PreviousMigrationName> --context UserDbContext`. This should only be done in development if a migration needs significant rework before being committed. Never revert applied migrations in shared or production environments.
* **Common Pitfalls / Gotchas:** Merge conflicts in `UserDbContextModelSnapshot.cs` require careful resolution. Errors in the `Up()` or `Down()` methods can leave the database in an inconsistent state. Forgetting to add a new migration after model changes leads to runtime errors. Production deployment script (`ApplyMigrations.sh`) failing due to permissions, missing `psql`, or incorrect connection details/credentials.

## 6. Dependencies

* **Internal Code Dependencies:**
   * [`Server/Services/Auth/Models`](../Models/README.md) - Defines the entities whose schema is being migrated.
   * [`Server/Services/Auth/UserDbContext.cs`](../UserDbContext.cs) - The EF Core context targeted by these migrations.
* **External Library Dependencies:**
   * `Microsoft.EntityFrameworkCore.Design`: Provides design-time functionality (used by `dotnet ef` tools).
   * `Microsoft.EntityFrameworkCore.Tools`: Provides `dotnet ef` command-line tools.
   * `Npgsql.EntityFrameworkCore.PostgreSQL`: PostgreSQL provider for EF Core.
* **Dependents (Impact of Changes):**
   * The entire application runtime depends on the database schema being correctly applied by these migrations.
   * The deployment pipeline relies on the ability to generate and apply these migrations successfully.

## 7. Rationale & Key Historical Context

* **EF Core Code-First Migrations:** Chosen as the standard .NET approach for managing relational database schema changes in conjunction with application code. It allows schema to be version-controlled alongside the code that uses it.
* **Idempotent SQL Scripts (Production):** Used for production deployments because they are generally considered safer and more transparent than direct `dotnet ef database update`. They can be reviewed before execution and only apply changes that haven't already been recorded in the `__EFMigrationsHistory` table. [cite: zarichney-api/Server/Services/Auth/Migrations/ApplyMigrations.sh, zarichney-api/Docs/PostgreSqlMaintenance.md]
* **Initial Migration (`20250330163155_InitialCreate`):** Established the baseline schema for ASP.NET Core Identity tables, plus the custom `RefreshTokens` and `ApiKeys` tables. [cite: zarichney-api/Server/Services/Auth/Migrations/20250330163155_InitialCreate.cs]

## 8. Known Issues & TODOs

* The `ApplyRefreshTokenMigration.sh` and `.ps1` scripts appear obsolete given the `InitialCreate` migration likely includes the refresh token schema. These scripts could potentially be removed to avoid confusion.
* Resolving merge conflicts in `UserDbContextModelSnapshot.cs` requires careful attention to ensure the snapshot accurately reflects the combined changes from different branches.