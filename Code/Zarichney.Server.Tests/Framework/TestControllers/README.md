# README: /Framework/TestControllers Directory

**Version:** 1.0
**Last Updated:** 2025-05-22
**Parent:** `../README.md`

## 1. Purpose & Responsibility

This directory contains minimal ASP.NET Core controllers that are **not part of the main `Zarichney.Server` application**. They are designed and used **exclusively by the testing framework (`Zarichney.Server.Tests`)**.

The primary responsibilities of these test controllers are:
* To provide simple, controllable HTTP endpoints that can be targeted by integration tests.
* To facilitate the testing of specific ASP.NET Core pipeline features, such as middleware (e.g., `FeatureAvailabilityMiddleware`), filters, routing, or model binding, in a more isolated manner than using the full application controllers.
* To act as probes for verifying framework-level behaviors or configurations set up by `CustomWebApplicationFactory`.

These controllers help in creating focused integration tests for cross-cutting concerns or pipeline components without the overhead or business logic complexity of the main application's controllers.

## 2. Architecture & Key Concepts

* **Test-Environment Only:** These controllers are typically only discovered, registered, and routed when the `Zarichney.Server` application is hosted within the test environment by `CustomWebApplicationFactory`. They should not be accessible in a production deployment.
* **Minimal Logic:** Test controllers should contain minimal to no business logic. Their action methods are usually simple, returning predefined responses or reflecting input back, to allow tests to focus on the behavior of the pipeline component being tested.
* **Example Component:**
    * **`FeatureTestController.cs`:** This controller is specifically designed to aid in testing feature availability.
        * It exposes various endpoints, some of which are decorated with the `[DependsOnService(...)]` attribute.
        * Integration tests (like `SwaggerFeatureAvailabilityTests.cs` or `FeatureAvailabilityMiddlewareTests.cs`) can call these endpoints to:
            * Verify how `FeatureAvailabilityMiddleware` blocks or allows requests based on the configuration status of the features declared by `[DependsOnService(...)]`.
            * Check how Swagger documents reflect the availability of these test endpoints based on feature status (via `ServiceAvailabilityOperationFilter`).

## 3. Interface Contract & Assumptions

* **Interface:** The "contract" provided by these controllers is their defined HTTP endpoints (routes, HTTP methods, expected parameters). Integration tests will use an HTTP client (typically the Refit `ApiClient` if these endpoints are included in the OpenAPI spec for testing, or a direct `HttpClient`) to make requests to these test-specific routes.
* **Assumptions:**
    * These controllers are only mapped and accessible when the application is run via `CustomWebApplicationFactory` during integration tests.
    * They should not rely on complex application services unless those services are part of the specific pipeline feature being tested and are appropriately configured/mocked by the test setup.
    * The routes defined for these test controllers must not conflict with actual application routes.

## 4. Local Conventions & Constraints

* **Naming:** Test controller classes should clearly indicate their specific testing purpose and end with `TestController.cs` (e.g., `FeatureTestController.cs`, `MiddlewareValidationTestController.cs`).
* **Simplicity:** Controllers in this directory must remain simple. Avoid adding business logic, database interactions, or dependencies on application services unless strictly necessary for the specific framework aspect being tested.
* **Registration:** Ensure that any new test controllers are properly discovered and routed by ASP.NET Core when hosted by `CustomWebApplicationFactory`. This usually happens automatically if they are in the test assembly and inherit from `ControllerBase`.
* **OpenAPI Specification:** By default, these test controllers might appear in the `swagger.json` generated during client creation if they are discoverable by Swashbuckle. If they should *not* appear in the client API surface used by most integration tests, consider using `[ApiExplorerSettings(IgnoreApi = true)]` unless their endpoints are specifically intended to be called via the generated Refit client for certain types of tests (like testing the `ServiceAvailabilityOperationFilter`).

## 5. How to Work With This Code

### Using Existing Test Controllers

* Test controllers like `FeatureTestController.cs` are typically targeted by specific integration tests designed to validate framework or pipeline behavior.
* **Example (Conceptual - testing `FeatureAvailabilityMiddleware`):**
    ```csharp
    // In an integration test (e.g., FeatureAvailabilityMiddlewareTests.cs)

    // Arrange: Configure a specific feature (e.g., "MyFeature") to be unavailable
    // (This might involve setting mock IStatusService behavior in CustomWebApplicationFactory)
    var client = Factory.CreateClient(); // From CustomWebApplicationFactory

    // Act: Call an endpoint on FeatureTestController that is decorated with [DependsOnService("MyFeature")]
    var response = await client.GetAsync("/test/feature/my-feature-endpoint");

    // Assert: Verify that the middleware blocked the request
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    ```

### Adding a New Test Controller

1.  **Define the Purpose:** Clearly identify which pipeline component or framework feature requires a dedicated test controller.
2.  **Create the Controller:** Add a new class in this directory (e.g., `MyPipelineTestController.cs`), inheriting from `Microsoft.AspNetCore.Mvc.ControllerBase`.
3.  **Define Minimal Actions:** Implement simple action methods with specific routes that allow the targeted feature to be exercised.
4.  **Register/Configure (if needed):** Ensure `CustomWebApplicationFactory` correctly sets up the environment for this controller to function as expected (e.g., registering any specific minimal services it might need just for the test scenario).
5.  **Write Integration Tests:** Create new integration tests in the `../../Integration/` directory that target the endpoints of your new test controller to validate the desired behavior.
6.  **Document:** Update this README to include the new test controller and its purpose.

## 6. Dependencies

### Internal Dependencies

* **`../Fixtures/CustomWebApplicationFactory.cs`:** Responsible for hosting these controllers within the test environment.
* **`../../Integration/` Tests:** Integration tests are the consumers of the endpoints exposed by these test controllers.
* **`Zarichney.Server` Project:** May depend on attributes (like `[DependsOnService]`) or enums from the main application if testing features related to them.

### Key External Libraries

* **`Microsoft.AspNetCore.Mvc.Core`**: For base controller functionalities and attributes (`ControllerBase`, `[HttpGet]`, etc.).

## 7. Rationale & Key Historical Context

Test-specific controllers were introduced to provide a clean and isolated way to test ASP.NET Core pipeline features and other cross-cutting concerns. Using full application controllers for such tests can be cumbersome due to their potentially complex dependencies, business logic, and more extensive setup requirements.

`FeatureTestController.cs`, for example, allows for precise testing of the `FeatureAvailabilityMiddleware` and related attributes without needing to set up the full context that a business-logic controller (like `RecipeController`) would require. This leads to more focused, faster, and more reliable tests for these specific framework aspects.

## 8. Known Issues & TODOs

* **Discovery in OpenAPI:** Review if all controllers in this directory should be excluded from the default test client generation via `[ApiExplorerSettings(IgnoreApi = true)]`, or if some are intentionally included for specific types of integration tests (e.g., `FeatureTestController.cs` is useful for testing Swagger filter behavior).
* **More Test Scenarios:** As new middleware or complex pipeline features are added to the `Zarichney.Server`, new test controllers might be needed in this directory to facilitate their isolated testing.
* Refer to the "Framework Augmentation Roadmap (TODOs)" in `../../TechnicalDesignDocument.md` for broader framework enhancements.

---
