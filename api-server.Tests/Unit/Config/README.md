# Configuration Unit Tests

## Overview

This directory contains unit tests for the configuration components of the Zarichney API. These tests verify the behavior of middleware, exception handling, and service availability features in isolation.

## Test Files

### ErrorHandlingMiddlewareTests.cs

Tests for the global error handling middleware that processes all exceptions and returns consistent API error responses:

- Verifies the middleware passes through the request pipeline when no exceptions occur
- Tests handling of `ServiceUnavailableException` (returns 503 with missing configurations)
- Tests handling of `ConfigurationMissingException` (returns 503 with section information)
- Tests handling of generic exceptions (returns 500 with error details and trace ID)
- Ensures that exceptions are properly logged

### DependsOnServiceTests.cs

Tests the `DependsOnService` attribute functionality:

- Verifies that controllers and methods with this attribute require the specified service
- Ensures that the attribute correctly identifies dependent services

### RequiresConfigurationAttributeTests.cs

Tests the `RequiresConfiguration` attribute that marks endpoints requiring specific configuration:

- Validates that configuration requirements are properly recognized
- Tests the attribute's behavior with different configuration scenarios

### ServiceAvailabilityOperationFilterTests.cs

Tests the Swagger integration for service availability:

- Verifies that the filter correctly identifies endpoints with `DependsOnService` attributes
- Ensures that Swagger documentation is properly updated to indicate service dependencies

### ServiceUnavailableExceptionTests.cs

Tests the `ServiceUnavailableException` class:

- Validates that the exception correctly captures and reports missing configurations
- Tests initialization and property access

## Running the Tests

To run just the configuration unit tests:

```bash
dotnet test --filter "Category=Unit&FullyQualifiedName~Zarichney.Tests.Unit.Config"
```