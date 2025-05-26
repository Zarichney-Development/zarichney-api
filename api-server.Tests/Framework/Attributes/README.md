# README: /Framework/Attributes Directory

**Version:** 1.2
**Last Updated:** 2025-05-25
**Parent:** `../README.md`

## 1. Purpose & Responsibility

This directory contains custom xUnit attributes developed to extend the standard testing capabilities and enforce project-specific conventions. These attributes play a crucial role in:

* **Conditional Test Execution:** Enabling tests to be dynamically skipped based on environmental conditions, configuration status, or the availability of external dependencies (e.g., `DependencyFactAttribute`, `DockerAvailableFactAttribute`, `ServiceUnavailableFactAttribute`).
* **Test Categorization and Filtering:** While xUnit's `[Trait]` attribute is used directly for categorization, some attributes like `DependencyFactAttribute` implicitly assign or relate to categories defined in `TestCategories.cs`.
* **Specialized Test Discovery/Execution Logic:** Providing tailored behavior for test discovery and execution beyond standard `[Fact]` and `[Theory]` attributes.

These attributes are integral to maintaining an efficient, reliable, and context-aware test suite, particularly in diverse development and CI/CD environments.

## 2. Architecture & Key Concepts

The primary attributes and their mechanisms are:

* **`DependencyFactAttribute.cs`**:
    * **Purpose:** An advanced `[Fact]` attribute that conditionally skips tests if specified dependencies are not met. This is crucial for integration tests that rely on external services or specific configurations.
    * **Mechanism:** It leverages the `IStatusService` from the `api-server` (to check feature availability via `ExternalServices` enum) and `ConfigurationStatusHelper` (to check for presence of required configuration values). It can also check for infrastructure dependencies like database connectivity via `InfrastructureDependency` enum.
    * **Usage:** Decorated on test methods, e.g., `[DependencyFact(ExternalServices.Llm, InfrastructureDependency.Database)]`.
    * **Details:** See Section 7 of `../../TechnicalDesignDocument.md` and Section 7 of `../../../Docs/Standards/IntegrationTestCaseDevelopment.md`.

* **`DockerAvailableFactAttribute.cs`**:
    * **Purpose:** A specialized `[Fact]` attribute that skips tests if a running Docker environment is not detected.
    * **Mechanism:** It typically attempts a simple Docker command (e.g., `docker version`) to ascertain availability.
    * **Usage:** Decorated on test methods that directly require Docker for Testcontainers, independent of specific service configurations (e.g., tests for `DatabaseFixture` itself).

* **`ServiceUnavailableFactAttribute.cs`**:
    * **Purpose:** A specialized `[Fact]` attribute that runs tests ONLY when the specified external service is UNAVAILABLE. This is the inverse of `DependencyFactAttribute` - it's used for integration tests that verify HTTP 503 responses from dependency-aware API endpoints when their specific external dependencies are unavailable.
    * **Mechanism:** It leverages the `IStatusService` from the `api-server` to check feature availability via `ExternalServices` enum. If the service is available, the test is skipped. If the service is unavailable, the test runs normally.
    * **Usage:** Decorated on test methods, e.g., `[ServiceUnavailableFact(ExternalServices.OpenAiApi)]` for testing that an endpoint returns HTTP 503 when OpenAI is unavailable.

* **`TestCategories.cs`**:
    * **Purpose:** This static class does not define attributes itself but provides string constants for standardized test category names used with xUnit's `[Trait("Category", TestCategories.Unit)]` attribute.
    * **Mechanism:** Ensures consistency in trait naming across the test suite, facilitating accurate test filtering.
    * **Usage:** Referenced in `[Trait]` attributes on test methods. The `DependencyFactAttribute` also uses these categories for its enum-to-trait mapping.

These attributes often work in conjunction with custom xUnit test case discoverers and executors (like `SkipMissingDependencyDiscoverer` and `SkipMissingDependencyTestCase` for `DependencyFactAttribute`, or `ServiceUnavailableFactDiscoverer` and `ServiceUnavailableTestCase` for `ServiceUnavailableFactAttribute`) to implement their conditional logic.

## 3. Interface Contract & Assumptions

* **For Test Writers:**
    * Attributes are applied directly to test methods (e.g., `[DependencyFact(...)]`, `[DockerAvailableFact]`, `[ServiceUnavailableFact(...)]`) or test classes.
    * When using `DependencyFactAttribute` with `ExternalServices` or `InfrastructureDependency` enums, the attribute handles checking the status via the application's `IStatusService` or framework-level checks.
    * When using `ServiceUnavailableFactAttribute` with an `ExternalServices` enum, the attribute checks service availability via `IStatusService` and skips if the service is available (opposite behavior from `DependencyFactAttribute`).
    * Tests using these attributes should clearly indicate their dependencies also via appropriate `[Trait("Category", ...)]` attributes (though `DependencyFactAttribute` handles some implicit trait mapping).
* **Assumptions Made by Attributes:**
    * `DependencyFactAttribute` assumes:
        * The `IStatusService` (when checking `ExternalServices`) is correctly implemented in the `api-server` and reflects the true availability of features based on configuration.
        * The `ConfigurationStatusHelper` (used for string-based dependency checks or by `IStatusService`) has access to the relevant `IConfiguration` instance.
        * For `InfrastructureDependency.Database`, it assumes the `DatabaseFixture` correctly reports its operational status.
    * `DockerAvailableFactAttribute` assumes that the mechanism it uses to check Docker's presence (e.g., running `docker version`) is reliable in the test execution environment.
    * `ServiceUnavailableFactAttribute` assumes:
        * The `IStatusService` is correctly implemented in the `api-server` and reflects the true availability of features based on configuration.
        * The test environment has proper access to the `CustomWebApplicationFactory` through the `ApiClientFixture` constructor argument.
        * The test is designed to verify behavior specifically when the service is unavailable (e.g., testing HTTP 503 responses).

## 4. Local Conventions & Constraints

* **Naming:** New custom fact/theory attributes should end with `FactAttribute` or `TheoryAttribute` respectively.
* **Documentation:** Any new attribute **must** have clear XML documentation explaining its purpose, parameters, and usage. Its README entry here must also be updated.
* **Testing:** Complex custom attributes, especially those with discovery or execution logic, **must** have their own unit tests in `../../Unit/Framework/Attributes/`. (e.g., `../../Unit/Framework/Attributes/DependencyFactAttributeTests.cs`).
* **Scope:** New attributes should be general-purpose and provide clear benefits for test management or execution across a range of scenarios.

## 5. How to Work With This Code

### Using Existing Attributes

* **`DependencyFactAttribute`**:
    ```csharp
    // Example: Test requires LLM feature (via IStatusService) AND Database infrastructure
    [DependencyFact(ExternalServices.Llm, InfrastructureDependency.Database)]
    [Trait("Category", "Integration")]
    [Trait("Category", TestCategories.ExternalOpenAI)] // Explicit trait for filtering
    [Trait("Category", TestCategories.Database)]     // Explicit trait for filtering
    public async Task MyFeature_ThatUsesLlmAndDb_WorksCorrectly()
    {
        // ... test logic ...
    }
    ```
* **`DockerAvailableFactAttribute`**:
    ```csharp
    [DockerAvailableFact]
    [Trait("Category", "Integration")]
    [Trait("Category", "Infrastructure")] // Custom trait for infrastructure-specific tests
    public async Task DatabaseFixture_WhenDockerIsAvailable_StartsSuccessfully()
    {
        // ... test logic for DatabaseFixture itself ...
    }
    ```
* **`ServiceUnavailableFactAttribute`**:
    ```csharp
    // Example: Test that verifies endpoint returns HTTP 503 when OpenAI is unavailable
    [ServiceUnavailableFact(ExternalServices.OpenAiApi)]
    [Trait("Category", "Integration")]
    [Trait("Feature", TestCategories.AI)]
    public async Task GetAiResponse_WhenOpenAiUnavailable_Returns503()
    {
        var response = await client.GetAiResponse();
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
    ```
* **`TestCategories.cs`**:
    ```csharp
    [Fact]
    [Trait("Category", TestCategories.Unit)] // Using constant from TestCategories.cs
    public void MyService_UnitLogic_IsCorrect()
    {
        // ...
    }
    ```

### Adding New Custom Attributes

1.  Create the new attribute class in this directory, typically inheriting from `Xunit.Sdk.FactAttribute` or `Xunit.Sdk.TheoryAttribute`.
2.  If custom discovery or execution logic is needed, implement `Xunit.Sdk.ITestCaseDiscoverer` and/or `Xunit.Sdk.IXunitTestCase`.
3.  Register the discoverer using `Xunit.Sdk.TestCaseDiscovererAttribute` on your custom attribute if applicable.
4.  Add unit tests for the new attribute in `../../Unit/Framework/Attributes/`.
5.  Update this README to document the new attribute.

## 6. Dependencies

### Internal Dependencies

* **`api-server`**:
    * `DependencyFactAttribute` may rely on `IStatusService` and enums like `ExternalServices` from the main application.
* **`../Helpers/`**:
    * `DependencyFactAttribute` uses `ConfigurationStatusHelper`, `SkipMissingDependencyDiscoverer`, and `SkipMissingDependencyTestCase`.
    * `ServiceUnavailableFactAttribute` uses `ServiceUnavailableFactDiscoverer` and `ServiceUnavailableTestCase`.
* **`../Fixtures/`**:
    * `DependencyFactAttribute` may interact with properties of fixtures like `DatabaseFixture` when checking `InfrastructureDependency.Database`.

### Key External Libraries

* **`Xunit.net` (xunit.core, xunit.extensibility.core, xunit.extensibility.execution):** Essential for creating custom attributes that integrate with the xUnit test execution pipeline.

## 7. Rationale & Key Historical Context

Custom attributes like `DependencyFactAttribute` and `DockerAvailableFactAttribute` were developed to address the need for more sophisticated control over test execution in varying environments. Standard `[Fact]` attributes lack the ability to dynamically skip tests based on runtime conditions or external dependencies without custom logic. These attributes centralize this skipping logic, making tests cleaner and the test suite more robust when run in environments where not all dependencies are available (e.g., local development without certain API keys vs. a fully configured CI environment).

The use of `TestCategories.cs` ensures standardized and type-safe categorization for test filtering, which is critical for managing a large test suite and enabling efficient test runs during development and CI.

## 8. Known Issues & TODOs

* **Trait Mapping Review:** The implicit trait mapping within `DependencyFactAttribute` should be periodically reviewed to ensure it aligns with all defined `TestCategories` and filtering needs.
* **Extensibility:** Consider if a more generic "ConditionalFactAttribute" base class could simplify the creation of new conditional attributes in the future.
* Refer to the "Framework Augmentation Roadmap (TODOs)" in `../../TechnicalDesignDocument.md` for broader framework enhancements.

---
