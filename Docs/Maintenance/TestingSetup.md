# Testing Setup and Maintenance

**Version:** 1.0
**Last Updated:** 2025-05-22

## Overview

This document describes the setup, configuration, and maintenance procedures for the automated testing infrastructure of the Zarichney API.

## CI/CD Integration

The test suite is integrated with GitHub Actions for continuous integration:

- Workflow runs on PRs to `main` and merges to `main`.
- Tests are divided into categories to allow for staged execution:
  - Unit Tests
  - Integration Tests (ReadOnly)
  - Integration Tests (DataMutating)
- Coverage reports are generated and published.

## Local Development Setup

### Prerequisites

1. **.NET 8 SDK:** Required for building and running tests.
2. **Docker:** Required for database integration tests (via Testcontainers).
3. **PowerShell:** Required for running the API client generation script.

### Local Test Execution

#### Running All Tests

```bash
dotnet test
```

#### Running Specific Test Categories

```bash
# Run unit tests only
dotnet test --filter "Category=Unit"

# Run integration tests only
dotnet test --filter "Category=Integration"

# Run database-dependent tests only
dotnet test --filter "Category=Integration&Dependency=Database"

# Run read-only integration tests
dotnet test --filter "Category=Integration&Mutability=ReadOnly"
```

### Client Generation

After making changes to API contracts (controllers, endpoints, models), you must regenerate the Refit client:

```powershell
# From the solution root
./Scripts/GenerateApiClient.ps1
```

This script generates:
- `api-server.Tests/Client/ZarichneyAPI.cs` file containing the `IZarichneyAPI` interface and supporting models

This ensures integration tests use the most up-to-date API client.

## Database Setup

### Testcontainers for PostgreSQL

Integration tests that require a real database for authentication logic use Testcontainers to provision a PostgreSQL instance dynamically:

- **Automatic Container Management**: The `DatabaseFixture` in `api-server.Tests/Framework/Fixtures/DatabaseFixture.cs` uses Testcontainers to start a PostgreSQL container automatically when needed.
- **Migration Application**: EF Core migrations are automatically applied to the containerized database during fixture initialization.
- **Test Isolation**: The `Respawn` library resets the database to a clean state between tests, ensuring test isolation.
- **Dependency Skipping**: Tests requiring a database automatically skip if Docker is not available, with clear skip reasons.

### Using Database-Dependent Tests

Tests that require a real database for authentication logic should:

1. **Inherit from `DatabaseIntegrationTestBase`**: This provides access to the database fixture and automatic skipping if the database is unavailable.
2. **Use `[DependencyFact(InfrastructureDependency.Database)]`**: This attribute handles dependency checking and test skipping.
3. **Call `ResetDatabaseAsync()`**: At the beginning of each test to ensure a clean database state.

Example:
```csharp
[Collection("Integration")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
public class AuthenticationTests : DatabaseIntegrationTestBase
{
    [DependencyFact(InfrastructureDependency.Database)]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        await ResetDatabaseAsync();
        // ... test implementation
    }
}
```

### Docker Requirements

- **Docker Desktop**: Must be running for Testcontainers to function properly.
- **PostgreSQL Image**: Tests use the `postgres:15` image, which is automatically pulled if not available locally.
- **Container Cleanup**: Containers are automatically cleaned up after test execution.

## Mock Authentication for Testing

The API includes a mock authentication system specifically designed for testing scenarios where the Identity Database is unavailable or when testing authentication-protected endpoints without needing real user accounts.

### How Mock Authentication Works in Tests

1. **Automatic Activation**: When integration tests run with a missing Identity Database connection string, mock authentication is automatically enabled.

2. **Test Authentication Handler**: The existing `TestAuthHandler` in `api-server.Tests/Framework/Helpers/TestAuthHandler.cs` is used by the `CustomWebApplicationFactory` to simulate authenticated users.

3. **Mock Authentication Handler**: For application-level mock authentication (used in development and some test scenarios), the `MockAuthHandler` in `api-server/Services/Auth/MockAuthHandler.cs` provides automatic authentication with configurable mock users.

### Using Mock Authentication in Integration Tests

Most integration tests that require authentication should use the existing `TestAuthHandler` via:

```csharp
// Create an authenticated client for a specific user and roles
var client = Factory.CreateAuthenticatedClient("test-user-id", ["Admin", "User"]);

// Or use the AuthTestHelper for more control
var client = Factory.CreateClient();
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
    "Test", AuthTestHelper.GenerateTestToken("user-id", ["Admin"]));
```

### Mock Authentication Configuration

For tests that specifically test the mock authentication functionality:

```json
{
  "MockAuth": {
    "DefaultRoles": ["User", "Admin"],
    "DefaultUsername": "TestMockUser", 
    "DefaultEmail": "test@mock.com",
    "DefaultUserId": "test-mock-id"
  }
}
```

### Benefits for Integration Testing

- **No Database Required**: Tests can verify authorization logic without needing a real Identity Database
- **Configurable Roles**: Easy to test different authorization scenarios by adjusting mock user roles
- **Consistent Test Environment**: Mock authentication provides predictable, repeatable test conditions
- **Development Support**: Developers can test protected endpoints during development without database setup

## Configuration Management

The test suite manages configuration through multiple layers:

1. `appsettings.json`: Base configuration (from api-server)
2. `appsettings.{EnvironmentName}.json`: Environment-specific settings (e.g., Development)
3. `appsettings.Testing.json`: Test-specific overrides
4. User Secrets: Local developer settings (Development only)
5. Environment Variables: CI/CD overrides

## Adding New Test Dependencies

To add a new dependency (e.g., a new external service):

1. Add a new constant in `TestCategories.cs`
2. Update the `_traitToConfigNamesMap` in `IntegrationTestBase.cs`
3. Create a mock factory in `Mocks/Factories/`
4. Register the mock in `CustomWebApplicationFactory.RegisterMockExternalServices()`
5. Update the `StatusService` to include configuration checks for the new dependency

## Troubleshooting

Common issues and solutions:

### Tests Skip Unexpectedly

- Check if Docker is running (for database tests)
- Check configuration status via `/api/status/config` endpoint
- Review TraitAttribute mismatches

### Client Generation Fails

- Ensure Swashbuckle is properly installed
- Verify API server builds successfully
- Check your .NET version (must be 8.0)

### CI Pipeline Failures

- Review specific test logs in the GitHub Actions artifacts
- Ensure all dependencies are correctly mocked
- Check for inconsistent database state between tests

## Maintenance Tasks

The following tasks should be performed periodically:

- Validate test coverage against the TDD goal (>=90%)
- Update Swashbuckle and Refitter versions when upgrading .NET
- Refresh test data to match changes in external services
- Review and update mock responses to match current external API behaviors