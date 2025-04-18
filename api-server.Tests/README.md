# Zarichney API Tests

This project contains automated tests for the Zarichney API Server, providing comprehensive coverage of the api-server functionality through both unit and integration tests.

## Project Structure

- **Unit Tests** (`Unit/`): Tests of individual components in isolation, organized to mirror the api-server project structure
- **Integration Tests** (`Integration/`): End-to-end tests of API endpoints using in-memory hosting
- **Fixtures** (`Fixtures/`): Shared test infrastructure like `CustomWebApplicationFactory` and `DatabaseFixture`
- **Helpers** (`Helpers/`): Utilities for testing including `GetRandom`, `AuthTestHelper`, and dependency management
- **Mocks/Factories** (`Mocks/Factories/`): Factories for creating consistent external service mocks
- **Test Data** (`TestData/`): Test models and builder classes for complex test data creation
- **Client** (`Client/`): Auto-generated Refit client for API testing

## Test Categorization

Tests are categorized using traits to allow for targeted test runs:

- **`Category`**: Unit, Integration, E2E, Smoke, Performance, Load, MinimalFunctionality, Controller, Component, Service
- **`Feature`**: Auth, Cookbook, Payment, Email, AI
- **`Dependency`**: Database, ExternalStripe, ExternalOpenAI, ExternalGitHub, ExternalMSGraph
- **`Mutability`**: ReadOnly, DataMutating

## Running Tests

### Basic Commands

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter "Category=Unit"

# Run integration tests only
dotnet test --filter "Category=Integration"

# Run specific feature tests
dotnet test --filter "Feature=Auth"

# Run integration tests that don't mutate data (safer for prod-like environments)
dotnet test --filter "Category=Integration&Mutability=ReadOnly"
```

## Dependency-Aware Testing

The test suite includes a dependency awareness mechanism that handles tests with external dependencies.

### How It Works

1. Tests that depend on external services should be marked with:
   - Appropriate dependency traits: `[Trait(TestCategories.Dependency, TestCategories.Database)]`
   - The `[DependencyFact]` attribute instead of the standard `[Fact]`

2. The system will:
   - Check for the required configuration during test initialization
   - Skip tests automatically when dependencies are missing
   - Provide clear skip reasons in the test output

### Example Usage

```csharp
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Feature, TestCategories.Auth)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
[Trait(TestCategories.Mutability, TestCategories.DataMutating)]
public class AuthTests : IntegrationTestBase
{
    public AuthTests(CustomWebApplicationFactory factory) : base(factory) { }

    [DependencyFact]  // Use DependencyFact instead of Fact
    public async Task Register_WithValidInput_ShouldCreateUser()
    {
        // This test will be skipped if the database is unavailable
        // ...test code...
    }
}
```

### Docker Runtime Dependency

For tests that specifically depend on the Docker runtime, use the `[DockerAvailableFact]` attribute:

```csharp
[DockerAvailableFact]
public async Task Database_ShouldBeAccessible_WhenContainerStarted()
{
    // This test will be skipped if Docker is not running
    // ...test code...
}
```

## Integration Testing Setup

Integration tests use:

1. `CustomWebApplicationFactory`: Configures the test server with:
   - Test-specific configuration
   - In-memory hosting of the API
   - Test database connection
   - Mocked external services

2. `DatabaseFixture`: Manages a PostgreSQL test database with Testcontainers

3. `IntegrationTestBase`: Base class for integration tests that:
   - Implements dependency checking logic
   - Provides test authentication utilities
   - Offers access to services and mocks

4. Refit API Client: Strongly-typed client generated from Swagger for API interaction

### Database Reset

Integration tests must call `ResetDatabaseAsync()` at the beginning of each test:

```csharp
[DependencyFact]
public async Task Test_RequiringCleanDatabase_ShouldWork()
{
    // Reset database to clean state
    await ResetDatabaseAsync();
    
    // Now the database is clean for this test
    // ...test code...
}
```

## Test Data Management

The test suite provides two primary ways to create test data:

1. **AutoFixture**: Via the `GetRandom` helper for simple random values:

```csharp
// Get random primitive values
string randomString = GetRandom.String();
int randomNumber = GetRandom.Int(1, 100);
DateTime randomDate = GetRandom.DateTime();
```

2. **Test Data Builders**: For complex object creation with fluent syntax:

```csharp
// Create a recipe with specific properties
var recipe = new RecipeBuilder()
    .WithTitle("Test Recipe")
    .WithDescription("A test recipe")
    .WithIngredients(["Ingredient 1", "Ingredient 2"])
    .Build();

// Or create a random recipe
var randomRecipe = RecipeBuilder.CreateRandom().Build();
```

## Maintaining the API Client

After making changes to API contracts (controllers, routes, models), regenerate the Refit client:

```powershell
# From the solution root
./Scripts/GenerateApiClient.ps1
```

## Configuration Management

Test configuration is managed through multiple layers:

1. `appsettings.json`: Base configuration 
2. `appsettings.{EnvironmentName}.json`: Environment-specific settings
3. `appsettings.Testing.json`: Test-specific overrides
4. User Secrets: Local developer settings (Development only)
5. Environment Variables: CI/CD overrides

## CI/CD Integration

The test suite is integrated with GitHub Actions:

- Workflow runs on PRs to `main` and merges to `main`
- Tests are executed in phases (unit → integration)
- Coverage reports are generated and published

For more details, see the `/Docs/Maintenance/TestingSetup.md` document.

Last Updated: April 18, 2025
