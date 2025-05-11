# ServiceUnavailabilityTests Integration Tests

## Overview
These integration tests verify the behavior of API endpoints when services are unavailable due to missing configuration.

## Current Status
The tests are currently marked with `Skip` attributes for the following reasons:

1. **Endpoint_WhenRequiredFeatureIsUnavailable_Returns503WithErrorDetails**: 
   - Status: Skipped
   - Reason: The test gets HTTP 400 Bad Request instead of the expected HTTP 503 Service Unavailable.
   - Potential solution: The request formatting for the Completion endpoint may need to be adjusted to match the API's expectations.

2. **StatusEndpoint_ReturnsServiceAvailabilityInformation**:
   - Status: Skipped
   - Reason: Cannot extract HttpClient from Refit client.
   - Potential solution: Modify the IZarichneyAPIExtensions to properly handle HttpClient extraction or use a different approach.

3. **SwaggerDocument_WhenFeaturesUnavailable_IncludesWarningsInOperations**:
   - Status: Skipped
   - Reason: Uses SkipSwaggerIntegrationFactAttribute since all Swagger integration tests are currently skipped.
   - Note: This is consistent with other Swagger tests in the codebase that use the same attribute.

## Expected Behavior When Implemented
When properly implemented, these tests will verify that:

1. API endpoints that depend on unavailable services return HTTP 503 responses.
2. The /api/status endpoint correctly reports service availability.
3. Swagger documentation includes warnings for endpoints that depend on unavailable services.

## Relationship to Other Tests
These tests complement:
- Unit tests for ServiceAvailabilityOperationFilter
- Unit tests for ConfigurationStatusService
- Integration tests for HttpClient extensions

## Future Work
To make these tests pass:
1. Fix the HTTP client setup in test fixtures to properly handle Refit clients
2. Ensure the correct endpoint paths are used for API calls
3. Properly handle form data for multipart requests