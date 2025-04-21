\# Zarichney API Server Tests (\`api-server.Tests\`)

\*\*Last Updated:\*\* 2025-04-21

This project contains automated tests for the \`api-server\` application, providing comprehensive coverage through both unit and integration tests. It aims to establish a robust, maintainable, and efficient testing framework adhering to the standards outlined in the \[Technical Design Document\](./TechnicalDesignDocument.md).

\*(Note: This project is undergoing refactoring to consolidate fixture management for improved efficiency and consistency. See "Refactoring Status" below).\*

\#\# Project Structure

The test project is organized to separate concerns and mirror the main application structure where applicable:

\* \*\*\`/Framework/\`\*\*: Contains the core testing infrastructure, including:  
\* \`Fixtures/\`: Shared test environment setup (\`CustomWebApplicationFactory\`, \`DatabaseFixture\`, \`ApiClientFixture\`).  
\* \`Helpers/\`: Utility classes for authentication simulation, random data, dependency checking, etc.  
\* \`Mocks/\`: Mock implementations and factories for external services.  
\* \`Client/\`: Auto-generated Refit client for API interaction.  
\* \`Attributes/\`: Custom xUnit attributes (\`DependencyFact\`, \`DockerAvailableFact\`).  
\* \`Configuration/\`: Configuration helpers.  
\* Base classes (\`IntegrationTestBase\`, \`DatabaseIntegrationTestBase\`).  
\* \*\*\`/Unit/\`\*\*: Contains unit tests focusing on isolated component logic. Organized mirroring the \`api-server\` project structure.  
\* \*\*\`/Integration/\`\*\*: Contains integration tests verifying end-to-end API behavior. Organized mirroring the \`api-server/Controllers\` structure.  
\* \*\*\`/TestData/\`\*\*: Contains test data builders and sample data artifacts.

\#\# Core Testing Strategies (Target State)

\* \*\*Unit Testing:\*\* Uses xUnit, Moq, FluentAssertions, and AutoFixture/Builders to test components in isolation. Aims for high coverage of business logic. See \[\`./Unit/README.md\`\](./Unit/README.md).  
\* \*\*Integration Testing:\*\*  
\* Uses \`CustomWebApplicationFactory\` to host the application in-memory.  
\* Interacts with the API via a generated Refit client (\`IZarichneyAPI\`).  
\* Leverages Testcontainers (\`DatabaseFixture\`) for realistic PostgreSQL database testing in CI/CD and optionally locally.  
\* Employs a refined configuration loading strategy supporting User Secrets (local dev), Environment Variables (CI/CD), and \`appsettings.Testing.json\` (base test settings/overrides).  
\* Uses a prioritized database connection approach (Config \-\> Testcontainers \-\> InMemory Fallback).  
\* Requires automatic database schema migration via \`DatabaseFixture\`.  
\* Uses mocks for external service boundaries (Stripe, OpenAI, etc.).  
\* Simulates authentication using \`TestAuthHandler\`.  
See \[\`./Integration/README.md\`\](./Integration/README.md).  
\* \*\*Fixture Management (Target):\*\* A single xUnit collection (\`\[Collection("Integration")\]\`) manages shared instances of \`CustomWebApplicationFactory\`, \`DatabaseFixture\`, and \`ApiClientFixture\` via \`ICollectionFixture\<\>\` for all integration tests, ensuring efficiency and consistency. \`ApiClientFixture\` is refactored to use the shared factory. Base classes (\`IntegrationTestBase\`, \`DatabaseIntegrationTestBase\`) are simplified to consume these shared fixtures without declaring \`IClassFixture\<\>\`. See \[\`./Framework/Fixtures/README.md\`\](./Framework/Fixtures/README.md).  
\* \*\*Dependency Awareness:\*\* Uses custom attributes (\`\[DependencyFact\]\`, \`\[DockerAvailableFact\]\`) and base class logic to automatically skip tests when required external configurations or Docker are unavailable. See "Dependency-Aware Testing" below and \[\`./Framework/Helpers/README.md\`\](./Framework/Helpers/README.md).

\#\# Test Categorization

Tests are categorized using \`\[TraitAttribute\]\` for granular filtering:

\* \*\*\`Category\`\*\*: \`Unit\`, \`Integration\`, \`MinimalFunctionality\`, etc.  
\* \*\*\`Feature\`\*\*: \`Auth\`, \`Cookbook\`, \`Payment\`, etc. (Domain-specific)  
\* \*\*\`Dependency\`\*\*: \`Database\`, \`ExternalStripe\`, \`ExternalOpenAI\`, \`ExternalGitHub\`, \`ExternalMSGraph\`, etc.  
\* \*\*\`Mutability\`\*\*: \`ReadOnly\`, \`DataMutating\` (For safe execution filtering).

\#\# Running Tests

Execute tests using the \`dotnet test\` command with optional filters.

\`\`\`bash  
\# Run all tests  
dotnet test

\# Run unit tests only  
dotnet test \--filter "Category=Unit"

\# Run integration tests only  
dotnet test \--filter "Category=Integration"

\# Run specific feature tests (e.g., Authentication)  
dotnet test \--filter "Category=Integration\&Feature=Auth"

\# Run integration tests that don't require the database  
dotnet test \--filter "Category=Integration\&Dependency\!=Database"

\# Run non-mutating integration tests (safer for prod-like checks)  
dotnet test \--filter "Category=Integration\&Mutability=ReadOnly"

## **Dependency-Aware Testing**

The test suite includes a dependency awareness mechanism that handles tests with external dependencies.

### **How It Works**

1. Tests that depend on external services or specific runtime conditions (like Docker) should be marked with:
  * Appropriate dependency traits: e.g., \[Trait(TestCategories.Dependency, TestCategories.Database)\]
  * The \[DependencyFact\] attribute instead of the standard \[Fact\].
  * Alternatively, \[DockerAvailableFact\] for tests requiring only the Docker runtime.
2. The system (primarily IntegrationTestBase and the custom Fact attributes/discoverers) will:
  * Check for the required configuration or runtime conditions during test discovery/initialization.
  * Skip tests automatically when dependencies are missing.
  * Provide clear skip reasons in the test output.

### **Example Usage**

\[Trait(TestCategories.Category, TestCategories.Integration)\]  
\[Trait(TestCategories.Feature, TestCategories.Auth)\]  
\[Trait(TestCategories.Dependency, TestCategories.Database)\] // Depends on DB config/availability  
\[Trait(TestCategories.Mutability, TestCategories.DataMutating)\]  
public class AuthTests : DatabaseIntegrationTestBase // Use DB base class  
{  
// Constructor receives fixtures from collection  
public AuthTests(CustomWebApplicationFactory factory, DatabaseFixture dbFixture, ApiClientFixture clientFixture)  
: base(factory, dbFixture, clientFixture) { }

    \[DependencyFact\]  // Use DependencyFact instead of Fact  
    public async Task Register\_WithValidInput\_ShouldCreateUser()  
    {  
        // This test will be skipped if the database dependency check fails  
        await ResetDatabaseAsync(); // Example DB interaction setup  
        // ...test code...  
    }  
}

\[Trait(TestCategories.Category, TestCategories.Integration)\]  
\[Trait(TestCategories.Feature, "Infrastructure")\]  
public class DockerTests : IntegrationTestBase // Doesn't need DB specific base  
{  
public DockerTests(CustomWebApplicationFactory factory, ApiClientFixture clientFixture)  
: base(factory, clientFixture) { }

    \[DockerAvailableFact\] // Requires Docker runtime  
    public async Task SomeDockerDependentCheck()  
    {  
        // This test will be skipped if Docker is not running  
        // ...test code...  
    }  
}

## **Integration Testing Setup (Target State Overview)**

Integration tests rely on the shared infrastructure provided by the Framework/ directory and managed via the single "Integration" xUnit collection:

1. **CustomWebApplicationFactory**: The shared factory configures the test server with appropriate settings for the environment (Dev/CI/Testing) and handles DbContext setup based on configuration/fixture availability.
2. **DatabaseFixture**: The shared fixture manages the PostgreSQL Testcontainer (including applying migrations) for tests needing a real database.
3. **ApiClientFixture**: The shared fixture provides pre-configured unauthenticated and authenticated IZarichneyAPI clients, using the shared factory instance.
4. **Base Classes**: IntegrationTestBase and DatabaseIntegrationTestBase provide common functionality and access to the shared fixtures via constructor injection.
5. **Refit API Client**: Tests interact with the API using the clients provided by ApiClientFixture via the base classes.

### **Database Reset**

Integration tests inheriting DatabaseIntegrationTestBase **must** call await ResetDatabaseAsync() at the beginning of each test method that modifies or relies on specific database state to ensure isolation.

\[DependencyFact\]  
public async Task Test\_RequiringCleanDatabase\_ShouldWork()  
{  
// Reset database to clean state before test execution  
await ResetDatabaseAsync();

    // Now the database is clean for this test  
    // ...test code...  
}

## **Test Data Management**

Use standard approaches for test data:

1. **AutoFixture**: Via the GetRandom helper (Framework/Helpers/GetRandom.cs) for simple random values.
2. **Test Data Builders**: (TestData/Builders/) For complex object creation with a fluent syntax (e.g., RecipeBuilder).

Refer to [./TestData/README.md](http://docs.google.com/TestData/README.md) for details.

## **Maintaining the API Client**

After changing API contracts (controllers, routes, models) in api-server, **regenerate the Refit client**:

\# From the solution root directory  
./Scripts/GenerateApiClient.ps1

Failure to do so may result in compilation errors or runtime failures in integration tests.

## **Configuration Management (Target State Overview)**

Test configuration follows a layered approach managed primarily by CustomWebApplicationFactory:

1. appsettings.json (Optional base config)
2. appsettings.{EnvironmentName}.json (Optional environment-specific)
3. appsettings.Testing.json (Optional base test settings/overrides)
4. User Secrets (Loaded in Development environment for local secrets)
5. Environment Variables (Highest precedence, for CI/local overrides)

This allows flexibility for different testing scenarios (local dev with secrets, CI with env vars/Testcontainers). See the TDD and Fixtures README for details.

## **CI/CD Integration**

The test suite is integrated with GitHub Actions (/.github/workflows/main.yml):

* Workflow runs on PRs to main and merges to main.
* Tests are executed in phases (unit → integration).
* Coverage reports are generated and published.

See /Docs/Maintenance/TestingSetup.md for details.

## **Key Documents & Further Reading**

* **Technical Design Document:** [./TechnicalDesignDocument.md](http://docs.google.com/TechnicalDesignDocument.md) \- Detailed testing strategy and requirements.
* **Framework README:** [./Framework/README.md](http://docs.google.com/Framework/README.md) \- Overview of the core testing infrastructure.
* **Fixtures README:** [./Framework/Fixtures/README.md](http://docs.google.com/Framework/Fixtures/README.md) \- Details on fixtures (includes refactoring TODOs).
* **Integration Tests README:** [./Integration/README.md](http://docs.google.com/Integration/README.md) \- Conventions for integration tests (includes refactoring TODOs).
* **Testing Standards:** /Docs/Standards/TestingStandards.md (in solution root) \- Coding and documentation standards for tests.

## **Refactoring Status**

**Note:** The integration testing framework is currently being refactored to implement the consolidated fixture strategy described above and in the TDD. This involves moving to a single test collection (\[Collection("Integration")\]), refactoring ApiClientFixture to use the shared CustomWebApplicationFactory, simplifying base classes, and ensuring consistent configuration and database handling (including automatic migrations). Specific TODOs are tracked in the README files within the /Framework/Fixtures/ and \`/Integration/