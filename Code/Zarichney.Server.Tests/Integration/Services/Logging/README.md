# LoggingService Integration Tests

This directory contains integration tests for the LoggingService that validate real-world connectivity to external logging infrastructure (Seq).

## Overview

The LoggingService integration tests verify:
- Real connectivity to Seq instances (native and Docker-based)
- Docker container lifecycle management
- Intelligent fallback logic between different logging methods
- Comprehensive logging status reporting with actual infrastructure

## Test Categories

### 1. Seq Connectivity Tests
- `TestSeqConnectivityAsync_WithRealSeqInstance_ReturnsSuccessResult`
  - Validates HTTP connectivity to real Seq instances
  - Tests connection timeout and retry logic
  - Verifies response time measurement

### 2. Docker Container Management Tests
- `TryStartDockerSeqAsync_WithDockerAvailable_AttemptsContainerStartup`
  - Tests automatic Seq container startup
  - Validates port mapping and configuration
  - Ensures proper container cleanup

- `IsDockerSeqContainerRunningAsync_ChecksContainerStatus`
  - Verifies container detection logic
  - Tests Docker CLI integration

- `GetDockerSeqContainerNameAsync_ReturnsContainerInfo`
  - Tests container name resolution
  - Validates container metadata retrieval

### 3. Fallback Logic Tests
- `GetBestAvailableSeqUrlAsync_WithMultipleUrls_FindsBestOption`
  - Tests intelligent URL selection
  - Verifies priority ordering (Native > Docker > Configured URLs)
  - Validates real-world failover scenarios

- `GetActiveSeqUrlAsync_WithRealSeq_ReturnsActiveUrl`
  - Tests active URL detection
  - Validates connectivity status

### 4. Comprehensive Status Tests
- `GetAvailableLoggingMethodsAsync_WithRealEnvironment_ShowsActualAvailability`
  - Tests complete method discovery
  - Validates all logging method availability
  - Verifies Docker and native Seq detection

- `GetLoggingStatusAsync_WithRealEnvironment_ReturnsComprehensiveStatus`
  - Tests full status reporting pipeline
  - Validates field consistency
  - Ensures accurate real-time status

## Prerequisites

### Required Software
- Docker Desktop or Docker Engine
- .NET 8 SDK
- (Optional) Native Seq service via systemd

### Docker Setup
```bash
# Install Docker (Ubuntu/Debian)
sudo apt install docker.io
sudo usermod -aG docker $USER
# Log out and back in for group changes to take effect

# Verify Docker installation
docker --version
docker ps
```

### Seq Container Setup
The tests can use an existing Seq container or start their own:

```bash
# Manual Seq container for development (optional)
docker run --name seq -d \
  -e ACCEPT_EULA=Y \
  -p 5341:80 \
  datalust/seq:latest

# Or use the provided docker-compose file
docker-compose -f docker-compose.integration.yml up -d
```

## Running the Tests

### Run All Integration Tests
```bash
# With Docker group membership active
dotnet test --filter "Category=Integration&Feature=Logging"

# If Docker group membership isn't active in current shell
sg docker -c "dotnet test --filter 'Category=Integration&Feature=Logging'"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~LoggingServiceIntegrationTests.TestSeqConnectivityAsync"
```

### Using the Test Suite Script
```bash
# Run integration tests only with detailed report
./Scripts/run-test-suite.sh report --integration-only

# Run with performance analysis
./Scripts/run-test-suite.sh report --integration-only --performance
```

## Test Environment Configuration

### Docker Compose Configuration
The `docker-compose.integration.yml` file provides:
- Primary Seq instance on port 5342
- Alternative Seq instance on port 8080
- Health checks for container readiness
- Isolated test network

### Environment Variables
```bash
# Enable/disable Docker fallback
export LOGGING__ENABLEDOCKERFALLBACK=true

# Configure Seq URL
export LOGGING__SEQURL=http://localhost:5341

# Set process timeout
export LOGGING__PROCESSTIMEOUTMS=10000
```

## Troubleshooting

### Common Issues

1. **Tests Skip with "Docker is not available"**
   - Ensure Docker is installed and running
   - Check Docker permissions: `docker ps`
   - Add user to docker group: `sudo usermod -aG docker $USER`

2. **Container Port Conflicts**
   - Check for existing containers: `docker ps`
   - Stop conflicting containers: `docker stop <container-name>`
   - Use alternative ports in configuration

3. **Seq Connection Timeouts**
   - Verify Seq container is healthy: `docker logs seq`
   - Check firewall rules for port 5341
   - Increase timeout in configuration

4. **Permission Denied Errors**
   ```bash
   # Fix Docker socket permissions
   sudo chmod 666 /var/run/docker.sock
   
   # Or use Docker group (recommended)
   sudo usermod -aG docker $USER
   newgrp docker
   ```

### Debugging Tests

Enable detailed logging:
```bash
# Set logging level
export Serilog__MinimumLevel__Default=Debug

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"
```

Check container status:
```bash
# List all Seq containers
docker ps --filter name=seq

# View container logs
docker logs seq-integration-test

# Inspect container health
docker inspect seq-integration-test --format='{{.State.Health.Status}}'
```

## CI/CD Considerations

### GitHub Actions
Tests automatically skip when Docker isn't available in CI:
- Uses `DependencyFact` attribute for conditional execution
- No build failures due to missing dependencies
- Clear skip reasons in test output

### Local Development
For full test coverage locally:
1. Ensure Docker is running
2. Run `docker-compose -f docker-compose.integration.yml up -d`
3. Execute tests with `./Scripts/run-test-suite.sh`
4. Clean up with `docker-compose -f docker-compose.integration.yml down`

## Best Practices

1. **Container Cleanup**
   - Tests clean up containers after execution
   - Use unique container names to avoid conflicts
   - Implement proper disposal in test fixtures

2. **Network Isolation**
   - Use dedicated Docker networks for tests
   - Avoid port conflicts with development services
   - Clean up networks after test runs

3. **Timeout Configuration**
   - Set appropriate timeouts for container startup
   - Use health checks for readiness detection
   - Handle timeout scenarios gracefully

4. **Error Handling**
   - Tests should handle container startup failures
   - Provide clear error messages for debugging
   - Skip tests gracefully when dependencies are missing

## Related Documentation

- [Testing Standards](../../../../Docs/Standards/TestingStandards.md)
- [LoggingService Documentation](../../../../../Zarichney.Server/Services/Logging/README.md)
- [Integration Test Framework](../../../Framework/README.md)
- [Docker Setup Guide](../../../../Docs/Development/DockerSetup.md)