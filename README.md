---
name: AI Coder Task (General)
about: Define a specific, incremental coding task (feature, fix, refactor) suitable for delegation to an AI Coder agent.
title: 'feat: Implement FeatureAvailabilityMiddleware for proactive endpoint status checking'
labels: 'ai-task,type:feature,module:middleware,module:config,module:startup,module:status'
assignees: '' # Assign to the human orchestrator

---

## 1. Overall Goal / User Story Context

Enhance application robustness and provide immediate feedback to API consumers by implementing a middleware that proactively checks if an endpoint can be served based on its declared feature dependencies (derived from configuration status). This prevents unnecessary processing for unavailable features and ensures the `ErrorHandlingMiddleware` returns a clear HTTP 503 with reasons.

## 2. Requirements (Mandatory Expectations)

- A new ASP.NET Core middleware, `FeatureAvailabilityMiddleware`, **MUST** be created.
- The middleware **MUST** inspect the target endpoint for `[RequiresFeatureEnabled(params Feature[] features)]` attributes (defined in the previous refactor task) at both controller and action levels.
- It **MUST** use the `IConfigurationStatusService` to determine if all features declared by the attribute(s) are currently available.
- If any required feature is unavailable, the middleware **MUST** throw a `ServiceUnavailableException`, populated with the reasons for unavailability (e.g., list of missing configuration keys) obtained from `IConfigurationStatusService`.
- If all required features are available, or if no `[RequiresFeatureEnabled]` attribute is present on the endpoint or its controller, the middleware **MUST** pass the request to the next component in the pipeline.
- The existing `ErrorHandlingMiddleware` will catch the `ServiceUnavailableException` and return an HTTP 503 response with a detailed JSON body.
- The `FeatureAvailabilityMiddleware` **MUST** be registered in the ASP.NET Core pipeline in `ApplicationStartup.cs` *after* routing, authentication, and authorization middleware but *before* the `MapControllers()` call that executes endpoint logic.
- No functional regressions in existing features or tests.
- Comprehensive unit and integration tests **MUST** be added for the new middleware.

## 3. Specific Task Objective

1.  Develop the `FeatureAvailabilityMiddleware` class.
2.  Implement the logic within the middleware to:
    a.  Access endpoint metadata to find `[RequiresFeatureEnabled]` attributes.
    b.  Aggregate all required `Feature` enums from class and method level attributes.
    c.  Inject and utilize `IConfigurationStatusService` to check the availability of each required feature.
    d.  If any feature is unavailable, construct and throw a `ServiceUnavailableException` detailing all missing configurations for all unavailable required features.
3.  Register `FeatureAvailabilityMiddleware` in the correct order in `ApplicationStartup.ConfigureApplication`.
4.  Add unit tests for the middleware's logic, covering various scenarios (attribute present/absent, features available/unavailable).
5.  Add integration tests to verify the end-to-end flow, including the 503 response when a feature is unavailable.
6.  Update relevant documentation (READMEs for middleware, startup process).

## 4. Acceptance Criteria

- [ ] `FeatureAvailabilityMiddleware.cs` is implemented correctly.
- [ ] When an HTTP request targets an endpoint decorated with `[RequiresFeatureEnabled(Feature.SomeFeature)]`:
    - [ ] If `Feature.SomeFeature` is reported as unavailable by `IConfigurationStatusService`, the middleware throws `ServiceUnavailableException` before the endpoint action executes.
    - [ ] The `ErrorHandlingMiddleware` catches this exception and returns an HTTP 503 response with a JSON body detailing the missing configurations.
    - [ ] If `Feature.SomeFeature` is available, the request proceeds to the endpoint action.
- [ ] Endpoints *not* decorated with `[RequiresFeatureEnabled]` are processed without interference from this middleware.
- [ ] The middleware correctly aggregates features from both class-level and action-level attributes.
- [ ] `FeatureAvailabilityMiddleware` is registered in the correct order in `ApplicationStartup.cs`.
- [ ] Unit tests for `FeatureAvailabilityMiddleware` achieve good coverage of its logic paths.
- [ ] Integration tests verify that protected endpoints correctly return HTTP 503 when underlying configurations are missing, and function normally when configurations are present.
- [ ] All existing application tests pass.
- [ ] Relevant documentation (`/Config/README.md` or a new `/Middleware/README.md`, `/Startup/README.md`) is updated.

## 5. Affected Components

- New file: `api-server/Config/FeatureAvailabilityMiddleware.cs` (or `api-server/Middleware/FeatureAvailabilityMiddleware.cs`)
- `api-server/Startup/ApplicationStartup.cs` (Middleware registration)
- `api-server/Config/README.md` (or new `/Middleware/README.md`)
- `api-server/Startup/README.md`
- `api-server.Tests/Unit/Middleware/FeatureAvailabilityMiddlewareTests.cs` (New test file)
- `api-server.Tests/Integration/Middleware/FeatureAvailabilityMiddlewareTests.cs` (New test file, or add to existing relevant integration tests)

## 6. Relevant Background / Links

- **Original Epic:** [https://github.com/Zarichney-Development/zarichney-api/issues/1](https://github.com/Zarichney-Development/zarichney-api/issues/1)
- **PR for Child Issue 1.1 (Core Infrastructure):** {Orchestrator: Please provide the PR link}
- **PR for Child Issue 1.2 (Service/DI Adaptation):** {Orchestrator: Please provide the PR link}
- **PR for Child Issue 1.3 (Swagger Integration):** {Orchestrator: Please provide the PR link}
- **PR for Attribute Refactor (Task immediately preceding this one):** {Orchestrator: Please provide the PR link}
- Key components to use:
    - `RequiresFeatureEnabledAttribute.cs` (Now uses `Feature` enum)
    - `IConfigurationStatusService` (Provides `IsFeatureAvailable(Feature feature)` and `GetFeatureStatus(Feature feature)`)
    - `ServiceUnavailableException.cs`
    - `ErrorHandlingMiddleware.cs`

## 7. (Optional) Implementation Notes / Plan

- The middleware should be placed in the pipeline to execute after routing has determined the endpoint, and after authentication/authorization have run, but before the endpoint itself is invoked.
- Ensure the `ServiceUnavailableException` is populated with a comprehensive list of all reasons (missing configs) if multiple required features are unavailable.

---
*Generated by AI Planning Assistant*