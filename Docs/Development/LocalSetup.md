# Local Development Setup & Configuration

**Last Updated:** 2025-05-19

> **Parent:** [`README.md`](./README.md)

## 1. Introduction

This document outlines the configuration and setup requirements for local development of the Zarichney API application. It covers environment setup, database configuration, required services, and graceful degradation patterns that enable development without all external dependencies.

## 2. Core Environment Setup

### Prerequisites

* **.NET 8.0 SDK or later**: Required to build and run the application.
* **Docker Desktop** (Recommended): For integration testing with local databases.
* **PostgreSQL** (Optional): For local Identity Database setup.
* **Git**: For source control management.
* **IDE**: Visual Studio 2025, VS Code with C# extension, or JetBrains Rider.

### Basic Commands

* **Build Application:**
  ```bash
  dotnet build Zarichney.sln
  ```

* **Run Application:**
  ```bash
  dotnet run --project api-server
  ```

* **Run Tests:**
  ```bash
  # Run all tests
  dotnet test Zarichney.sln
  
  # Run specific test categories
  dotnet test --filter "Category=Unit"
  dotnet test --filter "Category=Integration"
  ```

* **Format Code:**
  ```bash
  # Check formatting
  dotnet format --verify-no-changes --verbosity diagnostic
  
  # Apply formatting
  dotnet format
  ```

## 3. Configuration & Environment Variables

The application is configured using the following sources (in order of precedence):

1. **Environment Variables**: Highest precedence for overriding settings.
2. **User Secrets** (Development): Secure storage for local development secrets.
3. **`appsettings.{Environment}.json`**: Environment-specific settings.
4. **`appsettings.json`**: Default settings (lowest precedence).

### Critical Configuration Settings

* **Database Connection Strings:**
  ```json
  "ConnectionStrings": {
    "UserDatabase": "Host=localhost;Database=zarichney_identity;Username=postgres;Password=yourpassword"
  }
  ```

* **JWT Authentication Settings:**
  ```json
  "JwtSettings": {
    "SecretKey": "your-secure-key-at-least-32-characters-long",
    "Issuer": "https://api.zarichney.com",
    "Audience": "https://zarichney.com",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
  ```

* **External Service API Keys:**
  ```json
  "LlmConfig:ApiKey": "your-openai-api-key",
  "EmailConfig:AzureTenantId": "your-azure-tenant-id",
  // ... other external service keys
  ```

## 4. Graceful Degradation Patterns

The Zarichney API implements graceful degradation patterns that allow local development without all external dependencies configured. This section describes the behavior when key dependencies are unavailable.

### Identity Database (PostgreSQL)

The Identity Database is required for authentication and user management. The application handles missing Identity Database configuration differently based on the environment:

* **Production Environment**: 
  * The application will **fail to start** if the Identity Database connection string is missing or invalid.
  * This strict validation ensures that authentication and user management are always available in production.

* **Non-Production Environments** (Development, Testing, etc.):
  * The application will **start successfully** even if the Identity Database connection string is missing or invalid.
  * The `/status` endpoint will report `PostgresIdentityDb` as unavailable.
  * Authentication-related endpoints in the `AuthController` (e.g., `/api/auth/login`, `/api/auth/register`) will return HTTP 503 (Service Unavailable).
  * Other endpoints that don't depend on authentication will function normally.

This allows developers to run and test parts of the application even without a properly configured Identity Database.

Example `/status` response when Identity Database is unavailable:
```json
{
  "services": [
    {
      "name": "PostgresIdentityDb",
      "isAvailable": false,
      "missingConfigurations": ["ConnectionStrings:UserDatabase"]
    },
    // Other services...
  ]
}
```

### Other External Services

Similar graceful degradation patterns apply to other external services:

* **OpenAI API**: Required for AI features.
* **Microsoft Graph API**: Required for email sending functionality.
* **Stripe API**: Required for payment processing.
* **GitHub API**: Required for GitHub integration.

For each service, the application:
1. Uses the `ExternalServices` enum to track service status.
2. Updates the `/status` endpoint to report unavailable services.
3. Returns 503 (Service Unavailable) for endpoints that depend on unavailable services via the `[DependsOnService]` attribute.

## 5. Running Without PostgreSQL Identity Database

To run the application without setting up a PostgreSQL Identity Database:

1. Start the application in Development environment:
   ```bash
   # Set the environment to Development
   $env:ASPNETCORE_ENVIRONMENT="Development"  # Windows PowerShell
   export ASPNETCORE_ENVIRONMENT="Development" # Linux/macOS
   
   # Run without configuring UserDatabase connection string
   dotnet run --project api-server
   ```

2. Verify the application starts successfully and check the status:
   ```bash
   curl http://localhost:5000/status
   ```

3. Note that authentication endpoints will return 503 Service Unavailable:
   ```bash
   curl -X POST http://localhost:5000/api/auth/login -H "Content-Type: application/json" -d '{"email":"test@example.com","password":"test"}'
   ```

4. Other endpoints should function normally, allowing you to test non-authentication features.

## 6. Setting Up Local PostgreSQL Identity Database

For full functionality, we recommend setting up a local PostgreSQL database:

1. **Install PostgreSQL** (if not already installed).

2. **Create a Database**:
   ```sql
   CREATE DATABASE zarichney_identity;
   ```

3. **Configure Connection String**:
   ```bash
   # Using .NET User Secrets
   dotnet user-secrets set "ConnectionStrings:UserDatabase" "Host=localhost;Database=zarichney_identity;Username=postgres;Password=yourpassword" --project api-server
   ```

4. **Apply Migrations**:
   ```bash
   # Navigate to Migrations directory
   cd api-server/Services/Auth/Migrations
   
   # Run migration script
   ./ApplyMigrations.sh  # Linux/macOS
   ./ApplyMigrations.ps1 # Windows
   ```

## 7. Next Steps

* Review [`/api-server/Services/Status/README.md`](../../api-server/Services/Status/README.md) for details on the Status Service implementation.
* See [`/api-server/Controllers/AuthController.cs`](../../api-server/Controllers/AuthController.cs) for authentication endpoints.
* Explore [`/Docs/Maintenance/PostgreSqlDatabase.md`](../Maintenance/PostgreSqlDatabase.md) for database maintenance guidance.

---