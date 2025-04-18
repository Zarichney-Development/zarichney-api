# Zarichney API Tests

This project contains tests for the Zarichney API Server.

## Test Categories

Tests are categorized using traits to allow for targeted test runs:

- `Category`: Unit, Integration, E2E, Smoke, Performance, Load
- `Feature`: Auth, Cookbook, Payment, Email, AI
- `Dependency`: Database, ExternalStripe, ExternalOpenAI, ExternalGitHub, ExternalMSGraph

## Dependency-Aware Tests

The test system includes a dependency awareness mechanism that handles tests with external dependencies.

### How It Works

1. Tests that depend on external services (database, Stripe, OpenAI, etc.) should be marked with:
   - Appropriate `[Trait(TestCategories.Dependency, TestCategories.Database)]` attributes
   - The `[DependencyFact]` attribute instead of the standard `[Fact]`

2. The system will:
   - Check for the required dependencies during test initialization
   - Skip tests when dependencies are missing
   - Provide clear skip reasons in the test output

### Example Usage

```csharp
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Feature, TestCategories.Auth)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
public class AuthTests : IntegrationTestBase
{
    public AuthTests(CustomWebApplicationFactory factory) : base(factory) { }

    [DependencyFact]  // Use DependencyFact instead of Fact
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        // This test will be skipped if the database is unavailable
        // ...test code...
    }
}
```

### Docker Runtime Dependency

For tests that specifically depend on the Docker runtime (not just configured services), use the `[DockerAvailableFact]` attribute:

```csharp
[DockerAvailableFact]
public async Task Test_RequiringDockerRuntime_ShouldSucceed()
{
    // This test will be skipped if Docker is not running
    // ...test code...
}
```

Note that for most tests, the `[DependencyFact]` attribute along with appropriate `[Trait(TestCategories.Dependency, ...)]` attributes should be used instead, as this provides more granular control over which dependencies are required.

### Running Tests With Missing Dependencies

When dependencies are missing, tests will be properly skipped rather than failing:

```
dotnet test --filter "Category=Integration"
```

### Adding New Dependencies

To add a new dependency type:

1. Add a constant in `TestCategories.cs`
2. Update the `_traitToConfigNamesMap` in `IntegrationTestBase.cs` to map the dependency to required configuration items
3. Update `CheckDependenciesAsync()` in `IntegrationTestBase.cs` if special handling is needed

## Test Data

- Test data fixtures are located in the `TestData` directory
- Mock services are available in the `Mocks` directory

## Configuration

Test configuration is managed through:
- `appsettings.Testing.json` - Default test settings
- Environment variables - Can override settings for CI/CD pipelines

Last Updated: April 18, 2025
