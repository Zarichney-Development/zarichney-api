# Testing Setup and Maintenance

**Version:** 1.0
**Last Updated:** 2025-04-18

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

Integration tests that interact with the database use Testcontainers to provision a PostgreSQL instance dynamically:

- The database is provisioned automatically when running tests marked with `[Trait(TestCategories.Dependency, TestCategories.Database)]`.
- Tests requiring a database must use `DatabaseFixture` and call `ResetDatabaseAsync()` at the beginning.
- Database-dependent tests automatically skip if Docker is not available.

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