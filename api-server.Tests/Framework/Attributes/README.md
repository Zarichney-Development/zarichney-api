# Module/Directory: /Framework/Attributes

**Last Updated:** 2025-04-18

> **Parent:** [`Framework`](../README.md)
> **Related:**
> * **Attribute Sources:** [`DockerAvailableFactAttribute.cs`](DockerAvailableFactAttribute.cs), [`DependencyFactAttribute.cs`](DependencyFactAttribute.cs), [`SkipMissingDependencyDiscoverer.cs`](SkipMissingDependencyDiscoverer.cs), [`SkipMissingDependencyTestCase.cs`](SkipMissingDependencyTestCase.cs) (List may vary)
> * **Usage Context:** Applied to test methods in `/Unit` and `/Integration`.
> * **Standards:** [`TestingStandards.md`](../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains **custom xUnit attributes** (classes deriving from `FactAttribute`, `TheoryAttribute`, or related xUnit extensibility points) used within the test project. These attributes provide specialized logic to control the execution or reporting of tests based on external factors or specific conditions.

* **Why Custom Attributes?** To conditionally skip tests based on environment state (e.g., Docker not running) or configuration (e.g., a required external dependency like an API key is not configured), preventing test failures caused by missing prerequisites rather than actual code errors. They can also be used for custom test discovery or categorization beyond standard `Trait` attributes.

## 2. Scope & Key Functionality

This directory houses attributes that modify xUnit's test execution behavior:

* **`DockerAvailableFactAttribute.cs`:** An attribute (likely deriving from `FactAttribute`) that checks if the Docker daemon/engine is running on the test execution machine. If Docker is not available, tests decorated with this attribute are skipped, typically with an informative message.
* **`DependencyFactAttribute.cs`:** An attribute that derives from `FactAttribute` and works with custom test case discoverer/executer to check for the presence and validity of specific configuration settings or external dependencies needed for a test to run successfully. It can be used in two ways:
  1. **With ApiFeature enum values:** `[DependencyFact(ApiFeature.LLM, ApiFeature.Transcription)]` - This preferred approach directly checks if the specified features are available using the `IStatusService`. It provides a type-safe way to declare dependencies.
  2. **With string-based trait dependencies:** `[DependencyFact]` combined with `[Trait(TestCategories.Dependency, TestCategories.ExternalOpenAI)]` - This approach uses the `ConfigurationStatusHelper` and is maintained for backward compatibility.
* **`SkipMissingDependencyDiscoverer.cs` / `SkipMissingDependencyTestCase.cs`:** Supporting classes for `DependencyFactAttribute` that implement xUnit's test discovery and execution extensibility points to enable the conditional skipping logic.

## 3. Test Environment Setup

* **Usage:** These attributes are applied directly to test methods within `/Unit` or `/Integration` test classes, replacing standard `[Fact]` or `[Theory]` attributes where conditional execution is needed.
    ```csharp
    // Check if Docker is available
    [DockerAvailableFact]
    public async Task MyIntegrationTest_RequiresDocker_RunsSuccessfully() { /* ... */ }

    // Using ApiFeature enum (preferred approach)
    [DependencyFact(ApiFeature.Payments)]
    public void MyStripeTest_RequiresPaymentFeature_RunsSuccessfully() { /* ... */ }

    // Backward compatible string-based traits approach
    [DependencyFact]
    [Trait(TestCategories.Dependency, TestCategories.ExternalStripe)]
    public void MyLegacyTest_RequiresStripe_RunsSuccessfully() { /* ... */ }

    // Multiple ApiFeature dependencies
    [DependencyFact(ApiFeature.LLM, ApiFeature.GitHubAccess)]
    public void MyAiAndGitHubTest_RunsSuccessfully() { /* ... */ }
    ```
* **Dependencies:** Relies heavily on the xUnit.net testing framework's extensibility features (`FactAttribute`, `TheoryAttribute`, `ITestCaseDiscoverer`, `IXunitTestCase`).

## 4. Maintenance Notes & Troubleshooting

* **Dependency Checking Logic:** The accuracy of attributes like `DockerAvailableFactAttribute` and `DependencyFactAttribute` depends on the reliability of their internal checks (e.g., how Docker presence is detected, how configuration status is determined). Updates to this checking logic may be needed if the environment or configuration methods change.
* **xUnit Extensibility:** Modifying or creating custom xUnit attributes requires understanding xUnit's discovery and execution pipeline. Errors can sometimes be non-obvious.
* **New Conditions:** If tests need to be skipped based on other conditions, new custom attributes could be created and added to this directory.

## 5. Test Cases & TODOs

* **N/A:** Custom xUnit attributes themselves are framework extensions used *by* tests. Their internal logic (e.g., the code that checks for Docker) could potentially be unit tested if it becomes sufficiently complex (e.g., in `/Unit/Attributes/`), but the primary validation is observing their effect on test execution in different environments.

## 6. Changelog

* **2025-05-12:** Enhanced DependencyFactAttribute to support ApiFeature-based dependencies. Added dual approach with backward compatibility for string-based traits. (#1)
* **2025-04-18:** Initial creation - Moved custom attributes from Helpers. Defined purpose and scope. (Gemini)

