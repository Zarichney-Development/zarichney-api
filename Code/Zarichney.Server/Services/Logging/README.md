# Module/Directory: Services/Logging

**Last Updated:** 2025-08-06

**Parent:** [`Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Modular logging system management architecture that follows Single Responsibility Principle, separating concerns into focused, testable services with clean dependency injection patterns.
* **Key Responsibilities:** 
  - **Orchestration**: `LoggingService` delegates to focused services following modern C# patterns  
  - **Status Management**: `ILoggingStatus` handles system status monitoring and configuration reporting
  - **Connectivity**: `ISeqConnectivity` manages Seq detection, testing, and Docker fallback operations
  - **Process Execution**: `IProcessExecutor` handles system command execution for Docker and systemd operations
  - **Request/Response Logging**: Enhanced middleware with proper dependency injection patterns
  - **Public API**: Monitoring endpoints accessible without authentication
* **Why it exists:** To provide a maintainable, testable logging architecture that follows SOLID principles, eliminates anti-patterns, and supports modern C# development practices while maintaining intelligent fallback capabilities.

## 2. Architecture & Key Concepts

* **High-Level Design (Refactored Architecture):** 
  - **`ILoggingService` / `LoggingService`**: Orchestrator service using primary constructor pattern that delegates to focused services
  - **`ILoggingStatus` / `LoggingStatus`**: Handles status reporting, method detection, and configuration queries
  - **`ISeqConnectivity` / `SeqConnectivity`**: Manages Seq connection testing, URL discovery, and Docker fallback with proper cancellation token propagation
  - **`IProcessExecutor` / `ProcessExecutor`**: Abstracted process execution for Docker/systemd commands with timeout handling
  - **`LoggingConfig`**: Configuration model with modern collection expressions and sensible defaults
  - **`RequestResponseLoggerMiddleware`**: Refactored to eliminate service locator anti-pattern
  - **`Models/`**: Complete set of DTOs for API responses (`LoggingStatusResult`, `SeqConnectivityResult`, etc.)

* **Key Interaction Pattern (After Refactoring):**
  ```
  PublicController → ILoggingService → ILoggingStatus → [Status Operations]
                                    ↓
                                   ISeqConnectivity → IProcessExecutor → [System Commands]
                                    ↓                      ↓
                                   HttpClient           LoggingConfig
  ```

* **Intelligent Fallback Logic:**
  1. **Primary**: Test configured Seq URL if available and responding
  2. **Secondary**: Test common Seq URLs (localhost variations)  
  3. **Tertiary**: Attempt Docker Seq container startup if enabled
  4. **Fallback**: File-based logging with clear status indication

## 3. Interface Contract & Assumptions

* **Service Contract Overview:**
  - **`ILoggingService`**: Orchestration layer that delegates to focused services, all methods properly async with cancellation support
  - **`ILoggingStatus`**: Status and configuration reporting with real-time system information
  - **`ISeqConnectivity`**: Connection testing, URL discovery, and Docker management with proper timeout handling
  - **`IProcessExecutor`**: Abstracted system command execution with configurable timeouts and cancellation token propagation
  
* **Key Assumptions:**
  - HttpClient properly injected via IHttpClientFactory pattern for connection testing
  - Docker available if `EnableDockerFallback` is true, with proper permissions for container operations
  - File system write permissions available for fallback logging scenarios
  - Seq instances respond to `/api/events/raw` endpoint for connectivity validation
  
* **Error Handling:**
  - Connectivity failures result in graceful fallback, not exceptions
  - Docker command failures are logged but don't prevent file logging fallback
  - Invalid URLs return `false` from connectivity tests rather than throwing
  - **Security Integration:** URL validation now performed before any connectivity testing to prevent SSRF attacks

* **Configuration Requirements:**
  - No required configuration - all properties have sensible defaults
  - `SeqUrl` can be null - triggers automatic discovery of common URLs
  - Process and network timeouts are configurable but have safe defaults
  - **NetworkSecurity configuration** controls URL validation behavior (see Security section below)

## 4. Local Conventions & Constraints

* **Logging Standards:** 
  - Uses structured logging with `ILogger<LoggingService>` injection following project standards
  - All service operations logged at appropriate levels (Information for normal flow, Warning for fallbacks)
  - Performance-sensitive operations (connectivity tests) include response time metrics
  
* **Docker Integration:**
  - Uses standard Docker CLI commands for container management
  - Container naming follows pattern: `{DockerContainerName}` (configurable, default: "seq-fallback")
  - Respects Docker availability and fails gracefully if Docker not accessible
  
* **Modern C# Patterns:**
  - **Primary Constructors**: All new services use C# 12 primary constructor syntax for cleaner code
  - **HttpClient Factory**: Proper IHttpClientFactory pattern eliminates socket exhaustion risks
  - **Dependency Injection**: Eliminates service locator anti-patterns in middleware
  - **Process Execution**: Abstracted into dedicated service for better testability and separation of concerns

## 5. How to Work With This Code

* **Setup Steps:**
  1. All services are properly registered in DI container via `ServiceStartup.cs` with correct lifetime scopes
  2. Services follow dependency hierarchy: `LoggingService` → `ILoggingStatus` + `ISeqConnectivity` → `IProcessExecutor`
  3. HttpClient factory pattern ensures proper resource management
  4. Optional: Configure Seq URL in app settings or leave null for auto-discovery

* **Module-Specific Testing Strategy:**
  - **Unit Tests**: Focus on testing service delegation and orchestration behavior rather than implementation details
  - **Focused Service Tests**: Each service interface can be tested independently with appropriate mocks
  - **Integration Tests**: Test actual API endpoints via `PublicController` integration tests
  - **Process Execution Tests**: Mock `IProcessExecutor` for Docker/systemd command testing

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
  - [`Services/ProcessExecution`](../ProcessExecution/README.md) - For abstracted system command execution
  - [`Services/Security`](../Security/README.md) - For URL validation and SSRF prevention
  - [`Config`](../../Config/README.md) - For `IConfig` interface implementation
  - [`Controllers/PublicController`](../../Controllers/README.md) - For API endpoint hosting

* **Internal Dependents:**
  - [`Startup/ServiceStartup`](../../Startup/README.md) - Registers all focused services with proper DI lifetime management
  - [`Startup/ApplicationStartup`](../../Startup/README.md) - Configures middleware pipeline with corrected DI patterns
  - [`Startup/MiddlewareStartup`](../../Startup/README.md) - Provides extension methods for middleware registration

* **External Libraries:**
  - `Microsoft.Extensions.Options` - Configuration binding and validation
  - `Microsoft.Extensions.Http` - IHttpClientFactory for proper HttpClient lifecycle management
  - `System.Diagnostics` - Process management abstracted through IProcessExecutor
  - `System.Net.Http` - Seq connectivity testing via HttpClient factory pattern

## 7. Rationale & Key Historical Context

* **Architectural Refactoring (Issue #83):** Major refactoring based on AI analysis identified multiple architectural improvements needed:
  - **Single Responsibility Violation**: Original `LoggingService` handled logging, process execution, Docker management, and Seq detection
  - **Service Locator Anti-Pattern**: Middleware was using `IServiceProvider.CreateScope()` instead of proper constructor injection
  - **HttpClient Lifecycle Issues**: Direct HttpClient injection without IHttpClientFactory pattern risked socket exhaustion
  - **Modern C# Patterns**: Needed primary constructor adoption and collection expression updates

* **Focused Service Design:** Split monolithic service into focused interfaces following SOLID principles:
  - `ILoggingStatus`: Status reporting and configuration queries
  - `ISeqConnectivity`: Connection testing and Docker management
  - `IProcessExecutor`: Abstracted system command execution
  - `LoggingService`: Orchestration layer that delegates to focused services

* **Dependency Injection Modernization:** Eliminated anti-patterns and implemented proper DI patterns:
  - HttpClient factory pattern for resource management
  - Constructor injection instead of service locator pattern
  - Primary constructor syntax for cleaner code

* **Public API Integration:** Maintained logging status endpoints in `PublicController` while improving underlying architecture.

## 8. Security

* **SSRF Prevention:** All URL connectivity testing now includes security validation through `IOutboundUrlSecurity` service to prevent Server-Side Request Forgery attacks
* **URL Allowlisting:** Only explicitly approved hosts can be tested via the `/api/logging/test-seq` endpoint
* **Default Security Configuration:**
  ```json
  {
    "NetworkSecurity": {
      "AllowedSeqHosts": [],
      "EnableDefaultLocalhost": true,
      "MaxRedirects": 3,
      "EnableDnsResolutionValidation": true,
      "DnsResolutionTimeoutSeconds": 2
    }
  }
  ```

* **Security Features:**
  - **Host Allowlisting:** Only configured hosts plus localhost variants (when enabled) are permitted
  - **Scheme Validation:** Only HTTP and HTTPS schemes are allowed
  - **Credential Detection:** URLs containing credentials are rejected
  - **Private IP Protection:** DNS resolution to private IP ranges is blocked
  - **Structured Logging:** Security events logged with Event IDs 5700-5703 for monitoring

* **Configuration Security:**
  - **AllowedSeqHosts:** List of explicitly allowed hosts (exact match or subdomain with leading '.')
  - **EnableDefaultLocalhost:** Controls whether localhost, 127.0.0.1, and ::1 are automatically allowed
  - **DNS Resolution:** Validates that allowed external hosts don't resolve to private IP ranges

## 9. Known Issues & TODOs

* **Test Coverage Expansion:** Consider adding focused unit tests for new service interfaces (`ILoggingStatus`, `ISeqConnectivity`, `IProcessExecutor`) to complement orchestration tests
* **Performance Optimization:** Consider caching Seq connectivity status to reduce repeated network calls during high-traffic periods
* **Health Check Integration:** Future enhancement to integrate with ASP.NET Core health checks for monitoring dashboard integration
* **Compiler Warning:** Remove unused `serviceProvider` parameter warning in middleware by implementing proper DI pattern validation