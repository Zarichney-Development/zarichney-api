# README: Code/Zarichney.Server.Tests Project

**Version:** 1.3
**Last Updated:** 2025-06-26
**Parent:** `../Zarichney.Server/README.md` (Conceptual link to the main application's README)

## 1. Purpose & Responsibility

This project, `Code/Zarichney.Server.Tests`, contains all automated tests for the `Code/Zarichney.Server` project. Its primary purpose is to ensure the quality, stability, and correctness of the API server through comprehensive unit and integration testing.

This project and its contents are governed by the strategies and requirements outlined in the following key documents:
* **`./TechnicalDesignDocument.md`**: The blueprint for this test project's architecture, tools, and advanced strategies.
* **`../../Docs/Standards/TestingStandards.md`**: The overarching testing standards for the entire solution.
* **`../../Docs/Standards/UnitTestCaseDevelopment.md`**: Detailed guidance for writing unit tests.
* **`../../Docs/Standards/IntegrationTestCaseDevelopment.md`**: Detailed guidance for writing integration tests.

Adherence to these documents is mandatory for all test development and maintenance, enabling effective collaboration between human and AI developers and ensuring the test suite remains robust and maintainable.

## 2. Test Framework Overview & Key Concepts

The testing strategy employed aims for high confidence through realistic test scenarios while maintaining performance and reliability. Key aspects include:

* **In-Memory API Hosting:** Integration tests run against an in-memory instance of the `Zarichney.Server` using `CustomWebApplicationFactory<Program>`, allowing for fast execution without network overhead.
* **Database Testing:** Real database interactions are tested using PostgreSQL managed by **Testcontainers** via the `DatabaseFixture`. This ensures tests run against a clean, consistent database schema with migrations applied. Database state is reset between tests using **Respawn**.
* **Test Environment Logging:** Enhanced configurable logging system for test environment using Serilog with Warning default level and configuration-driven overrides. Test-specific logging configuration available in `appsettings.Testing.json`. Full configuration guide at [`../Docs/Development/LoggingGuide.md`](../Docs/Development/LoggingGuide.md).
* **External HTTP Service Virtualization:** Interactions with external third-party HTTP APIs (e.g., Stripe, OpenAI) will be managed using **WireMock.Net** (as per FRMK-004 in `TechnicalDesignDocument.md`). This ensures deterministic behavior and isolates tests from external flakiness.
* **API Client Interaction:** Integration tests interact with the API using type-safe **Refit clients** (multiple granular interfaces), which are auto-generated from the API's OpenAPI specification.
* **Authentication Simulation:** The `TestAuthHandler` allows for simulating various authenticated users and authorization scenarios.
* **Core Tooling:**
    * **xUnit:** Test execution framework.
    * **FluentAssertions:** Expressive and readable assertions.
    * **Moq:** Mocking framework for unit tests.
    * **AutoFixture:** Test data generation, including advanced customizations for complex objects.
* **Fixture Strategy:** A consolidated fixture strategy is employed, primarily using xUnit's `ICollectionFixture` with the `"Integration"` collection to share expensive resources like `CustomWebApplicationFactory` and `DatabaseFixture` across test classes, optimizing performance.

## 3. Project Structure

This test project is organized into the following main directories:

* **`/Framework/`**: Contains the core testing infrastructure, including:
    * `Attributes/`: Custom xUnit attributes like `[DependencyFact]` and `[DockerAvailableFact]`. (See `./Framework/Attributes/README.md`)
    * `Client/`: The auto-generated Refit API clients (multiple interface files). (See `./Framework/Client/README.md`)
    * `Fixtures/`: Shared test fixtures like `CustomWebApplicationFactory`, `DatabaseFixture`, and `ApiClientFixture`. (See `./Framework/Fixtures/README.md`)
    * `Helpers/`: Utility classes for testing, such as `AuthTestHelper` and `TestConfigurationHelper`. (See `./Framework/Helpers/README.md`)
    * `Mocks/`: Contains mock factories (e.g., `MockStripeServiceFactory`) for configuring mocked services within the `CustomWebApplicationFactory`, and will include configurations for WireMock.Net. (See `./Framework/Mocks/README.md`)
    * `TestData/AutoFixtureCustomizations/`: (New) Will house advanced AutoFixture customizations.
    * (See `./Framework/README.md`)
* **`/Unit/`**: Contains all unit tests, mirroring the structure of the `Zarichney.Server` project. (See `./Unit/README.md`)
* **`/Integration/`**: Contains all integration tests, generally organized by API controllers or features. (See `./Integration/README.md`)
* **`/TestData/`**: Contains test data builders and sample data files.
    * `Builders/`: Custom test data builders (e.g., `RecipeBuilder`). (See `./TestData/Builders/README.md`)
    * `Recipes/`: Sample JSON data like `Burger.json`.
    * (See `./TestData/README.md`)

## 4. Key Standards & Development Guides

All test development **must** adhere to the following standards documents:

* **Core Technical Design:**
    * `./TechnicalDesignDocument.md` - The primary architectural blueprint for this test suite.
* **Overarching Testing Principles:**
    * `../../Docs/Standards/TestingStandards.md` - General testing philosophy, tooling, and quality expectations.
* **Specific Test Type Development:**
    * `../../Docs/Standards/UnitTestCaseDevelopment.md` - Detailed "how-to" for writing unit tests.
    * `../../Docs/Standards/IntegrationTestCaseDevelopment.md` - Detailed "how-to" for writing integration tests.
* **Code & Documentation Quality:**
    * `../../Docs/Standards/CodingStandards.md` - Standards for writing C# code (applies to test code too).
    * `../../Docs/Standards/DocumentationStandards.md` - Standards for writing per-directory README files within this project.

A commitment to high test coverage (>=90% for unit tests) and rigorous adherence to these standards is expected to ensure a reliable and maintainable API.

## 5. How to Work With This Code

### Running Tests

#### Automation Suite (Recommended)

* **Complete Test Suite with Coverage Report:**
    ```bash
    # Run all tests with comprehensive coverage reporting and open report in browser
    ../Scripts/run-test-suite.sh
    
    # Run without opening browser automatically
    ../Scripts/run-test-suite.sh --no-browser
    
    # Run only specific test categories
    ../Scripts/run-test-suite.sh --unit-only
    ../Scripts/run-test-suite.sh --integration-only
    
    # Skip build step for faster iteration (assumes solution already built)
    ../Scripts/run-test-suite.sh --skip-build
    ```
    
    The automation suite script provides:
    - ✅ Comprehensive test execution (unit + integration)
    - 📊 Code coverage collection and HTML report generation
    - 🌐 Automatic browser opening of coverage report
    - 🔍 Test results in TRX format for detailed analysis
    - 🐳 Smart Docker access handling for Testcontainers

#### Individual Test Commands

* **Unit Tests:**
    ```bash
    dotnet test --filter "Category=Unit"
    ```
* **Integration Tests:**
    * Ensure Docker is running if tests rely on `DatabaseFixture` or other Testcontainers.
    * Ensure necessary configurations are available (e.g., in `appsettings.Testing.json` or user secrets) for tests using `[DependencyFact]`. Tests for unavailable dependencies will be skipped.
    ```bash
    # Standard execution (if Docker group membership is active)
    dotnet test --filter "Category=Integration"
    
    # For environments where Docker group membership isn't active in current shell
    # (Required in some Linux/WSL2 setups for Testcontainers)
    sg docker -c "dotnet test --filter 'Category=Integration'"
    sg docker -c "dotnet test --filter 'Category=Integration&Dependency=Database'"
    ```
* **Specific Tests:** Use appropriate `--filter` options with `dotnet test`.
* Refer to the `TechnicalDesignDocument.md` Section 11 for CI/CD testing execution details.

### API Client Generation

* If you make changes to the `Zarichney.Server`'s API contracts (controller signatures, routes, DTOs), you **must** regenerate the Refit client:
    ```powershell
    ./Scripts/generate-api-client.ps1
    ```
  or for bash/zsh:
    ```bash
    ./Scripts/generate-api-client.sh
    ```
  This ensures the test clients are synchronized.

### Contribution Guidelines

* Follow all linked standards documents when writing or modifying tests.
* Ensure all tests pass locally before submitting changes.
* Update any relevant documentation (including this README or sub-directory READMEs) if your changes impact the testing framework, strategies, or setup.
* New tests must accompany new features or bug fixes in the `Zarichney.Server`.
* Refactor existing tests for clarity, performance, or adherence to evolving standards as needed.

## 6. Dependencies

### Internal Dependencies

* **`Code/Zarichney.Server`**: The primary dependency, as this project tests the API server.

### Key External Libraries & Tools

* **xUnit**: Test framework.
* **FluentAssertions**: Assertion library.
* **Moq**: Mocking library.
* **AutoFixture**: Test data generation.
* **Testcontainers**: For managing Dockerized dependencies (e.g., PostgreSQL).
* **Refit**: For generating the type-safe HTTP client.
* **Respawn**: For database cleanup.
* **Coverlet**: For code coverage.
* **WireMock.Net** (Planned): For HTTP service virtualization.
* **PactNet** (Future Consideration): For contract testing.

## 7. Rationale & Key Historical Context

This test suite has evolved with an increasing emphasis on rigor and AI-coder compatibility. A significant past effort involved refactoring to a consolidated fixture strategy (using `ICollectionFixture`) to improve the performance and maintainability of integration tests by sharing expensive resources like the `CustomWebApplicationFactory` and `DatabaseFixture`.

The current focus is on implementing the framework augmentations detailed in the `TechnicalDesignDocument.md` and achieving comprehensive test coverage adhering to the newly established detailed testing guides.

## 8. Known Issues & TODOs

* **Framework Augmentation:** This test framework is actively being enhanced. Refer to the **"Framework Augmentation Roadmap (TODOs)" (Section 16)** in `TechnicalDesignDocument.md` for a list of planned improvements (e.g., WireMock.Net integration, advanced AutoFixture customizations).
* **Test Coverage:** While the goal is >=90% unit test coverage and comprehensive integration test coverage, this is an ongoing effort. Specific coverage gaps may exist and are being progressively addressed. (Refer to `../../Docs/Development/TestCovergeWorkflow.md`).

---
