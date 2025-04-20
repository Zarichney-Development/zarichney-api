# Module/Directory: /Integration/Controllers/PublicController

**Last Updated:** 2025-04-18

> **Parent:** [`Controllers`](../README.md)
> **Related:**
> * **Source:** [`PublicController.cs`](../../../../api-server/Controllers/PublicController.cs)
> * **Service:** [`Services/Status/IStatusService.cs`](../../../../api-server/Services/Status/IStatusService.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs)

## 1. Purpose & Rationale (Why?)

This directory contains integration tests for the `PublicController` endpoints, which are designed to be accessible without requiring user authentication. These typically include basic health checks or endpoints exposing publicly permissible information.

* **Why Integration Tests?** To ensure these public endpoints function correctly within the ASP.NET Core pipeline, verifying routing, proper handling of `[AllowAnonymous]` attributes (or lack of `[Authorize]`), interaction with any underlying services (like `IStatusService`), and validation of HTTP responses.
* **Why Mock Dependencies?** While these tests run against a real server instance via `CustomWebApplicationFactory`, the factory *can* be configured to mock dependencies like `IStatusService` if needed for specific scenarios (though the current tests primarily verify behavior with the default service implementation).

## 2. Scope & Key Functionality Tested (What?)

These tests cover endpoints intended for public consumption:

* **`GET /api/health`:** A simple, unauthenticated endpoint to verify basic API responsiveness and expected response structure.
* **`GET /api/status/config`:** An endpoint exposing the status of various configuration items (API keys, connection strings, etc.) via the `IStatusService`.
* **Accessibility:** Verifying that these endpoints can be accessed *without* providing authentication tokens or cookies.
* **Response Validation:** Ensuring the endpoints return the expected data structure and status codes (typically 200 OK).

## 3. Test Environment Setup

* **Test Server:** Provided by `CustomWebApplicationFactory<Program>`.
* **Authentication:** Authentication simulation (`TestAuthHandler`) is **not** required for accessing these endpoints. Tests use a default, unauthenticated `HttpClient` obtained from the factory.
* **Mocked Dependencies:** Configured in `CustomWebApplicationFactory`. The primary dependency is:
    * `Zarichney.ApiServer.Services.Status.IStatusService` (used by `/api/status/config`)

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Tests are located in `PublicControllerTests.cs`.
* **Service Interaction:** The `/api/status/config` endpoint relies on `IStatusService`. Ensure this service behaves as expected in the test environment or mock it appropriately in the factory if specific service states need simulation.
* **No Auth Required:** Remember that tests for these endpoints should generally succeed with an unauthenticated client. Failures might indicate accidental addition of `[Authorize]` attributes or issues in the application startup/middleware that incorrectly block public access.

## 5. Test Cases & Status

### Public Health Check (`PublicControllerTests.cs`)
* **DONE:** Test `GET /api/health` without authentication -> verify 200 OK and expected response structure (`GetHealth_WhenCalled_ReturnsOkStatusAndTimeInfo`).
* **Optional:** Test `GET /api/health` *with* authentication -> verify 200 OK (should still work).

### Public Configuration Status (`PublicControllerTests.cs`)
* **DONE:** Test `GET /api/status/config` without authentication -> verify 200 OK with expected configuration item names and status details (`GetConfigurationStatus_WhenCalled_ReturnsAllExpectedConfigItems`).
* **TODO:** (If needed) Test `GET /api/status/config` when `IStatusService` is mocked to return specific statuses (e.g., all configured, specific errors).

## 6. Changelog

* **2025-04-18:** Updated README to reflect implemented tests (`GetHealth_WhenCalled_ReturnsOkStatusAndTimeInfo`, `GetConfigurationStatus_WhenCalled_ReturnsAllExpectedConfigItems`), clarified scope, dependencies, and marked TODOs as DONE. (AI Assistant)
* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `PublicController` integration tests. (Gemini)

