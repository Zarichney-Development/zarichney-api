# Module/Directory: Services/Logging

**Last Updated:** 2025-01-26

**Parent:** [`Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Centralized logging system management service that consolidates logging functionality previously scattered across `LoggingController`, `LoggingMiddleware`, and `ConfigurationStartup` into a cohesive, testable service architecture.
* **Key Responsibilities:** 
  - Intelligent Seq detection and fallback management
  - Logging system status monitoring and reporting  
  - Connectivity testing for Seq instances
  - Request/response logging middleware with correlation tracking
  - Docker Seq container management as fallback option
  - Public API endpoints for logging system operations
* **Why it exists:** To provide a single, testable service for all logging system operations while maintaining intelligent fallback capabilities and supporting both development and production logging scenarios.

## 2. Architecture & Key Concepts

* **High-Level Design:** 
  - `ILoggingService` / `LoggingService`: Core service implementing intelligent Seq detection with native → Docker → file fallback
  - `LoggingConfig`: Configuration model with sensible defaults for all logging operations
  - `RequestResponseLoggerMiddleware`: Enhanced HTTP request/response logging with session correlation
  - `Models/`: Complete set of DTOs for API responses (`LoggingStatusResult`, `SeqConnectivityResult`, etc.)
  - Integration with `PublicController` for monitoring endpoints accessible without authentication

* **Key Interaction Pattern:**
  ```
  PublicController → ILoggingService → [Native Seq | Docker Seq | File Logging]
                                   ↓
                              LoggingConfig (fallback URLs, timeouts)
  ```

* **Intelligent Fallback Logic:**
  1. **Primary**: Test configured Seq URL if available and responding
  2. **Secondary**: Test common Seq URLs (localhost variations)  
  3. **Tertiary**: Attempt Docker Seq container startup if enabled
  4. **Fallback**: File-based logging with clear status indication

## 3. Interface Contract & Assumptions

* **Core Service Contract (`ILoggingService`):**
  - All async methods accept `CancellationToken` with sensible defaults
  - Methods never throw exceptions during fallback scenarios - graceful degradation to file logging
  - Connectivity tests respect configured timeouts (default 3 seconds)
  - Status methods return real-time information, not cached data
  
* **Key Assumptions:**
  - HTTP connectivity available for Seq URL testing via injected `HttpClient`
  - Docker available if `EnableDockerFallback` is true
  - File system write permissions available for fallback logging
  - Seq instances respond to `/api/events/raw` endpoint for connectivity validation
  
* **Error Handling:**
  - Connectivity failures result in graceful fallback, not exceptions
  - Docker command failures are logged but don't prevent file logging fallback
  - Invalid URLs return `false` from connectivity tests rather than throwing

* **Configuration Requirements:**
  - No required configuration - all properties have sensible defaults
  - `SeqUrl` can be null - triggers automatic discovery of common URLs
  - Process and network timeouts are configurable but have safe defaults

## 4. Local Conventions & Constraints

* **Logging Standards:** 
  - Uses structured logging with `ILogger<LoggingService>` injection following project standards
  - All service operations logged at appropriate levels (Information for normal flow, Warning for fallbacks)
  - Performance-sensitive operations (connectivity tests) include response time metrics
  
* **Docker Integration:**
  - Uses standard Docker CLI commands for container management
  - Container naming follows pattern: `{DockerContainerName}` (configurable, default: "seq-fallback")
  - Respects Docker availability and fails gracefully if Docker not accessible
  
* **HTTP Client Usage:**
  - Depends on injected `HttpClient` with default configuration
  - Implements proper timeout handling using `CancellationTokenSource.CreateLinkedTokenSource`
  - Tests Seq-specific endpoint `/api/events/raw` for validation

## 5. How to Work With This Code

* **Setup Steps:**
  1. Ensure `LoggingConfig` is registered in DI container via `ServiceStartup.cs`
  2. Register `ILoggingService` as scoped service with `HttpClient` dependency
  3. Optional: Configure Seq URL in app settings or leave null for auto-discovery

* **Module-Specific Testing Strategy:**
  - **Unit Tests**: Mock all dependencies (`ILogger`, `IConfiguration`, `HttpClient`, `IOptions<LoggingConfig>`)
  - **Integration Tests**: Test actual API endpoints via `PublicController` integration tests
  - Focus on fallback logic testing with different connectivity scenarios

* **Key Test Scenarios:**
  - Seq connectivity success/failure with various URLs
  - Docker fallback activation and container management
  - Configuration validation with missing/invalid values
  - Timeout handling for network operations
  - API endpoint authentication (should be none) and response formats

* **Test Data Considerations:**
  - Use `AutoFixture` for `LoggingConfig` generation with sensible overrides
  - Mock HTTP responses for Seq connectivity testing
  - Test with both valid and invalid Docker configurations

* **Commands:** Use standard project test commands - no special setup required

* **Known Pitfalls:**
  - Seq connectivity tests require actual HTTP calls unless properly mocked
  - Docker commands require proper permissions - may need `sg docker` in some environments
  - Integration tests require API client regeneration if endpoints change

## 6. Dependencies

* **Internal Dependencies:**
  - [`Services/Sessions`](../Sessions/README.md) - For session correlation in request logging middleware
  - [`Config`](../../Config/README.md) - For `IConfig` interface implementation
  - [`Controllers/PublicController`](../../Controllers/README.md) - For API endpoint hosting

* **Internal Dependents:**
  - [`Startup/ServiceStartup`](../../Startup/README.md) - Registers services in DI container
  - [`Startup/ApplicationStartup`](../../Startup/README.md) - Configures middleware pipeline
  - [`Startup/MiddlewareStartup`](../../Startup/README.md) - Provides extension methods for middleware registration

* **External Libraries:**
  - `Microsoft.Extensions.Options` - Configuration binding and validation
  - `System.Diagnostics` - Process management for Docker operations
  - `System.Net.Http` - Seq connectivity testing

## 7. Rationale & Key Historical Context

* **Centralization Decision:** Previously, logging functionality was scattered across multiple files (`LoggingController.cs`, middleware in `Config/`, startup configuration). Centralizing into a service provides better testability, maintainability, and separation of concerns.

* **Intelligent Fallback Design:** Based on need to support both development (native Seq) and containerized environments (Docker Seq) while ensuring the application never fails due to logging configuration issues.

* **Public API Integration:** Logging status endpoints placed in `PublicController` rather than dedicated controller to reduce surface area and ensure monitoring capabilities are always available without authentication.

* **Middleware Integration:** Request/response logging moved from `Config/` namespace to align with service-based architecture while maintaining existing functionality.

## 8. Known Issues & TODOs

* **Test Framework Issues:** Unit and integration tests currently disabled due to FluentAssertions syntax compatibility issues - needs resolution for full test coverage
* **API Client Generation:** Integration tests require API client regeneration to include new logging endpoints
* **Performance Optimization:** Consider caching Seq connectivity status to reduce repeated network calls during high-traffic periods
* **Health Check Integration:** Future enhancement to integrate with ASP.NET Core health checks for monitoring dashboard integration