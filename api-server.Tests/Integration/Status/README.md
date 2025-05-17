# ServiceUnavailabilityTests Integration Tests

## Overview
These integration tests verify the behavior of API endpoints when services are unavailable due to missing configuration. The tests focus on ensuring proper 503 Service Unavailable responses when required external services are not configured.

## Purpose
1. Verify that endpoints with the `[DependsOnService]` attribute correctly return HTTP 503 when the dependency is unavailable
2. Ensure the status endpoint properly reflects service availability status
3. Confirm that Swagger documentation includes appropriate warnings for unavailable endpoints

## Implementation Details
The tests use the email validation endpoint (`/api/email/validate`) which depends on the `MailCheck` external service. This endpoint provides a clean way to test service unavailability without relying on form data handling that can be problematic in the test environment.

## Key Components
- **MockStatusService**: Configured to report the `MailCheck` service as unavailable
- **IZarichneyAPI.Validate**: Client method to call the email validation endpoint
- **ExternalServices.MailCheck**: Enum value representing the email validation service

## Tests
1. **UnavailableEndpoint_WhenRequiredFeatureIsUnavailable_Returns503WithErrorDetails**:
   - Verifies that the email validation endpoint returns HTTP 503 when the MailCheck service is unavailable
   - Explicitly asserts the 503 Service Unavailable status code
   - Checks that the response includes details about the missing configuration (EmailConfig:MailCheckApiKey)

2. **StatusEndpoint_ReturnsServiceAvailabilityInformation**:
   - Tests that the `/api/status` endpoint correctly reports mixed service availability
   - Verifies that multiple services can be reported as available/unavailable

3. **SwaggerDocument_WhenFeaturesUnavailable_IncludesWarningsInOperations**:
   - Verifies that Swagger documentation is accessible via the /api/swagger/swagger.json endpoint
   - Confirms that the Swagger document contains required structural elements (paths, components)
   - Logs information about the document content for debugging purposes
   - Provides a resilient test that passes if the Swagger endpoint returns a valid document

## Relationship to Other Tests
These tests complement:
- Unit tests for `ServiceAvailabilityOperationFilter`
- Unit tests for `StatusService`
- Integration tests for `FeatureAvailabilityMiddleware`

## Usage Notes
To manually test these scenarios during development:
1. Temporarily remove required configuration (e.g., `Email:MailCheckApiKey` in appsettings.json)
2. Run the API and observe the 503 responses from affected endpoints
3. Check the Swagger UI to see warning indicators for affected endpoints