# Module/Directory: /Integration/Controllers/PublicController

**Last Updated:** 2025-04-18

> **Parent:** [`Controllers`](../README.md)
> **Related:**
> * **Source:** [`PublicController.cs`](../../../../api-server/Controllers/PublicController.cs)
> * **Service:** [`Services/Status/StatusService.cs`](../../../../api-server/Services/Status/StatusService.cs) (Potentially)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs)

## 1. Purpose & Rationale (Why?)

This directory contains integration tests for the `PublicController` endpoints, which are designed to be accessible without requiring user authentication. These typically include basic health checks or endpoints exposing publicly permissible information.

* **Why Integration Tests?** To ensure these public endpoints function correctly within the ASP.NET Core pipeline, verifying routing, proper handling of `[AllowAnonymous]` attributes (or lack of `[Authorize]`), interaction with any underlying (mocked) services (like `IStatusService`), and validation of HTTP responses.
* **Why Mock Dependencies?** Any services injected into `PublicController` (e.g., `IStatusService`) are typically mocked in the `CustomWebApplicationFactory`. This isolates the controller's endpoint logic from the actual service implementation, allowing focused testing of the public API behavior.

## 2. Scope & Key Functionality Tested (What?)

These tests cover endpoints intended for public consumption:

* **`GET /api/public/health-check`:** A simple, unauthenticated endpoint to verify basic API responsiveness.
* **`GET /api/public/status`:** (If applicable) An endpoint exposing general system status information deemed safe for public access.
* **`GET /api/public/config`:** (If applicable) An endpoint exposing specific, non-sensitive configuration values.
* **Accessibility:** Verifying that these endpoints can be accessed *without* providing authentication tokens or cookies.
* **Response Validation:** Ensuring the endpoints return the expected data (based on mocked services or configuration) and status codes (typically 200 OK).
* **Error Handling:** Verifying behavior when underlying mocked services report errors (if applicable).

## 3. Test Environment Setup

* **Test Server:** Provided by `CustomWebApplicationFactory<Program>`.
* **Authentication:** Authentication simulation (`TestAuthHandler`) is **not** required for accessing these endpoints. Tests typically use a default, unauthenticated `HttpClient` obtained from the factory.
* **Mocked Dependencies:** Configured in `CustomWebApplicationFactory`. Potential mocks include:
    * `Zarichnyi.ApiServer.Services.Status.IStatusService` (if `/status` endpoint exists)
    * `Microsoft.Extensions.Configuration.IConfiguration` (if `/config` endpoint exists and requires mocking specific values)

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Tests should be organized logically within this directory (e.g., `HealthCheckTests.cs`, `PublicStatusTests.cs`).
* **Service Mocking:** Ensure any dependent services are mocked correctly in the factory to simulate different states (e.g., status service reporting healthy vs. unhealthy).
* **No Auth Required:** Remember that tests for these endpoints should generally succeed with an unauthenticated client. Failures might indicate accidental addition of `[Authorize]` attributes or issues in the application startup/middleware that incorrectly block public access.

## 5. Test Cases & TODOs

### Public Health Check (`HealthCheckTests.cs`)
* **TODO:** Test `GET /health-check` without authentication -> verify 200 OK and expected simple response (e.g., "OK").
* **TODO:** Test `GET /health-check` *with* authentication (optional) -> verify 200 OK (should still work).

### Public Status (`PublicStatusTests.cs` - If endpoint exists)
* **TODO:** Test `GET /status` without authentication -> mock `IStatusService` success -> verify 200 OK with expected status data.
* **TODO:** Test `GET /status` without authentication -> mock `IStatusService` failure/unhealthy -> verify 200 OK (or appropriate status code like 503) with expected status data reflecting the issue.

### Public Config (`PublicConfigTests.cs` - If endpoint exists)
* **TODO:** Test `GET /config` without authentication -> mock `IConfiguration` (if needed) -> verify 200 OK with expected public configuration values.
* **TODO:** Ensure *only* intended public values are exposed.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `PublicController` integration tests. (Gemini)

