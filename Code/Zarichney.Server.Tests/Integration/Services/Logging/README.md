# Module/Directory: /Integration/Services/Logging

**Last Updated:** 2025-01-10

> **Parent:** [`Services Integration Tests`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Integration test module that validates the LoggingService's real-world connectivity to external logging infrastructure (Seq) and Docker container management capabilities.
* **Key Responsibilities:**
  - Testing actual HTTP connectivity to Seq instances (native and Docker-based)
  - Validating Docker container lifecycle management (start/stop/cleanup)
  - Verifying intelligent fallback logic between different logging methods
  - Testing comprehensive logging status reporting with real infrastructure dependencies
  - Ensuring graceful degradation when external dependencies are unavailable
* **Why it exists:** To validate the LoggingService behavior with actual external dependencies (Seq, Docker) rather than mocked implementations, ensuring real-world reliability and proper failover mechanisms.

## 2. Architecture & Key Concepts

* **High-Level Design:** These tests use the `IntegrationTestBase` framework with `DependencyFact` attributes to conditionally execute tests based on Docker availability. Tests directly interact with real Seq instances and manage Docker containers for isolated test execution.
* **Key Test Categories:**
  - **Seq Connectivity Tests:** Validate HTTP connectivity, response time measurement, and error handling
  - **Docker Container Management:** Test container lifecycle, detection logic, and name resolution
  - **Fallback Logic Tests:** Verify intelligent URL selection and priority ordering
  - **End-to-End Integration:** Comprehensive status reporting with actual infrastructure
* **Test Infrastructure Components:**
  - `CustomWebApplicationFactory` for hosting the application under test
  - Real Docker containers (datalust/seq:latest) for Seq testing
  - Container lifecycle management with proper cleanup and isolation
  - Reflection-based testing of private methods for comprehensive coverage

## 3. Interface Contract & Assumptions

* **Preconditions:**
  - Docker must be installed and running for container-based tests
  - Seq container image (datalust/seq:latest) must be available
  - Tests assume port 5341 is available for Seq connectivity testing
  - Network connectivity to localhost required for HTTP testing
* **Postconditions:**
  - All test containers are cleaned up after execution (no resource leaks)
  - Tests skip gracefully when Docker dependencies are unavailable
  - No persistent state changes to the host system
* **Critical Assumptions:**
  - Docker daemon is properly configured and accessible
  - Seq container starts within 3 seconds (test timeout)
  - HTTP connectivity tests complete within 10 seconds
  - Container names are unique to avoid conflicts between parallel test runs
* **Error Handling:** Tests use `DependencyFact` attributes to skip gracefully when infrastructure is unavailable, preventing CI/CD failures due to missing dependencies.

## 4. Local Conventions & Constraints

* **Test Container Naming:** Uses unique container names (`seq-integration-test`) to avoid conflicts
* **Port Configuration:** 
  - Test containers use port 5342 to avoid conflicts with development Seq instances on 5341
  - Alternative test URLs include localhost:5341, 127.0.0.1:5341, and localhost:8080
* **Timeout Values:** 10-second timeout for all container operations and HTTP connectivity tests
* **Collection Assignment:** Uses `IntegrationInfra` collection for lightweight infrastructure tests without database dependencies
* **Cleanup Strategy:** All containers started during tests are automatically cleaned up in finally blocks and use `--rm` flag

## 5. How to Work With This Code

### Module-Specific Testing Strategy
These are integration tests that require real Docker infrastructure. They skip automatically in environments without Docker, making them CI/CD safe while providing comprehensive validation in development environments.

### Setup Steps
1. **Install Docker:**
   ```bash
   # Ubuntu/Debian
   sudo apt install docker.io
   sudo usermod -aG docker $USER
   # Log out and back in for group changes
   ```

2. **Configure Test Environment:**
   ```bash
   # Optional: Pre-pull Seq image
   docker pull datalust/seq:latest
   
   # Optional: Start development Seq instance
   docker-compose -f docker-compose.integration.yml up -d
   ```

### Running Tests
```bash
# Run all logging integration tests
dotnet test --filter "Category=Integration&Feature=Logging"

# Run with Docker group (if needed)
sg docker -c "dotnet test --filter 'Category=Integration&Feature=Logging'"

# Using unified test suite
./Scripts/run-test-suite.sh report --integration-only
```

### Key Test Scenarios
- **Seq Connectivity Validation:** Tests actual HTTP connectivity with real response time measurement
- **Container Lifecycle Management:** Validates proper startup, detection, and cleanup of Docker containers  
- **Fallback Logic Testing:** Verifies intelligent URL selection with multiple Seq instances
- **Graceful Degradation:** Ensures tests skip appropriately when Docker is unavailable

### Test Data Considerations
- Tests use real Seq containers rather than mocked responses
- Container port allocation managed dynamically to avoid conflicts
- HTTP connectivity tested against actual Seq API endpoints
- No persistent test data - all validation uses runtime container state

### Known Pitfalls
- Tests may fail if Docker daemon is not running or misconfigured
- Container startup race conditions possible if system is under heavy load
- Port conflicts can occur if multiple test runs execute simultaneously
- Testcontainers library may require specific Docker socket configuration

## 6. Dependencies

### Key Internal Dependencies
* **Services Being Tested:**
  - [`LoggingService`](../../../../Zarichney.Server/Services/Logging/README.md) - Primary system under test
  - [`SeqConnectivity`](../../../../Zarichney.Server/Services/Logging/README.md) - Seq connectivity logic
  - [`LoggingStatus`](../../../../Zarichney.Server/Services/Logging/README.md) - Status reporting functionality
* **Test Framework Components:**
  - [`IntegrationTestBase`](../../README.md) - Base class providing DI and dependency checking
  - [`ApiClientFixture`](../../../Framework/Fixtures/README.md) - Test application hosting
  - [`DependencyFact`](../../../Framework/Attributes/README.md) - Conditional test execution

### Key External Dependencies
* **Docker Infrastructure:**
  - Docker Engine/Desktop for container management
  - `datalust/seq:latest` container image for real Seq instances
* **Testing Libraries:**
  - xUnit for test framework
  - FluentAssertions for test assertions
  - Testcontainers.NET for Docker integration (via framework)

### Key Dependents
* **CI/CD Pipeline:** Tests provide validation for logging infrastructure reliability
* **Development Workflow:** Ensures logging fallback mechanisms work correctly in real environments

## 7. Rationale & Key Historical Context

* **Real Infrastructure Testing:** These tests complement unit tests by validating actual external dependencies, ensuring the LoggingService works reliably with real Seq instances and Docker containers.
* **Graceful Degradation Design:** The `DependencyFact` approach ensures tests provide value in development environments with Docker while maintaining CI/CD compatibility in containerized build environments.
* **Container Isolation Strategy:** Uses unique container names and ports to enable parallel test execution without resource conflicts.

## 8. Known Issues & TODOs

* **Docker Configuration Complexity:** Testcontainers may require additional configuration in some development environments
* **Container Startup Timing:** Fixed 3-second wait for container readiness could be optimized with health check polling
* **Reflection-Based Private Method Testing:** Current approach for testing private container detection methods could be improved with internal visibility or public wrapper methods