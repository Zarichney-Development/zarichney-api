# Module/Directory: Scripts

**Last Updated:** 2025-08-06

**(Optional: Link to Parent Directory's README)**
> **Parent:** [`zarichney-api`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory contains utility scripts that automate essential development, testing, deployment, and operational tasks for the zarichney-api project, including CI/CD pipeline logic and AI analysis prompts.
* **Key Responsibilities:** 
    * API client generation and synchronization between server and test projects
    * Comprehensive test automation with coverage reporting
    * Application deployment and service management
    * Development environment setup and maintenance
    * Recipe scraping functionality testing
    * Playwright browser automation setup
* **Why it exists:** To provide standardized, automated workflows that ensure consistency across development, testing, deployment, and CI/CD phases while reducing manual effort and potential human error. The modular structure separates workflow orchestration from business logic for better maintainability.

## 2. Architecture & Key Concepts

* **High-Level Design:** The scripts are organized into five main categories:
    * **Development & Testing Scripts:** `generate-api-client.*`, `run-test-suite.sh` (unified testing)
    * **Deployment & Service Management:** `start-server.sh`, `cookbook-api.service`, `cleanup-playwright.sh`
    * **Domain-Specific Testing:** `test_sites.sh` (recipe scraping validation)
    * **Configuration Files:** `.refitter` (API client generation settings), `docker-compose.integration.yml` (integration test environment)
    * **CI/CD Pipeline Logic:** Scripts moved to `.github/scripts/`
    * **AI Analysis Prompts:** Prompts relocated to `.github/prompts/`
* **Core Workflow Integration:** Scripts integrate with the CI/CD pipeline and support local development workflows by:
    1. Generating strongly-typed API clients from OpenAPI specifications
    2. Running comprehensive test suites with coverage reporting
    3. Managing production deployment and service lifecycle
* **Cross-Platform Support:** Most scripts provide both PowerShell (`.ps1`) and Bash (`.sh`) implementations for Windows and Unix-like systems
* **Docker Integration:** Scripts handle Docker access requirements for Testcontainers-based integration tests

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `generate-api-client.*`:
        * **Purpose:** Generate strongly-typed Refit client interfaces for integration testing
        * **Critical Preconditions:** `.NET 8 SDK installed`, `api-server project buildable`, `refitter tool available`, `port 5000 available`
        * **Critical Postconditions:** `api-server.Tests/Framework/Client/` populated with generated client interfaces grouped by OpenAPI tags
        * **Non-Obvious Error Handling:** 
            - Automatically detects and kills processes on port 5000 to prevent conflicts
            - Extended 90-second timeout with health checks (45 attempts × 2 seconds)
            - Captures startup logs for debugging API server issues  
            - Validates process health before attempting API calls
            - Enhanced error reporting with detailed diagnostics
    * `run-test-suite.sh` (Unified Testing):
        * **Purpose:** Consolidated test execution with multiple modes - automation (HTML reports), report (AI analysis), or both
        * **Critical Preconditions:** `Docker running for integration tests`, `.NET 8 SDK`, `ReportGenerator tool (automation mode)`, `jq for JSON processing (report mode)`
        * **Critical Postconditions:** 
            - **Automation Mode:** HTML coverage report in `CoverageReport/`, browser opens report
            - **Report Mode:** JSON/Markdown results in `TestResults/`, quality gate enforcement for CI/CD
            - **Both Mode:** All outputs from both modes
        * **Non-Obvious Error Handling:** Unified Docker group membership detection; supports `sg docker` fallback; mode-specific error handling and logging
* **Critical Assumptions:**
    * **Environment Dependencies:** Docker Desktop available for integration tests, Chrome/Chromium for Playwright
    * **Network Access:** API endpoints accessible on configured ports, external recipe sites reachable for scraping tests
    * **File System Permissions:** Write access to output directories (`TestResults/`, `CoverageReport/`, test output)
    * **Tool Availability:** Global .NET tools can be installed automatically (`refitter`, `ReportGenerator`)

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:**
    * `.refitter` file defines API client generation settings (namespace: `Zarichney.Client`, output: `api-server.Tests/Framework/Client/`)
    * `docker-compose.integration.yml` defines Seq logging infrastructure for LoggingService integration tests (ports 5342, 8080)
    * Service configuration in `cookbook-api.service` targets production environment paths
* **Directory Structure:**
    * Generated files appear in `api-server.Tests/Framework/Client/` (not within Scripts directory)
    * Test output directories: `TestResults/`, `CoverageReport/` (project root)
    * Service scripts expect deployment at `/opt/cookbook-api/`
* **Technology Choices:**
    * PowerShell Core for cross-platform PowerShell scripts
    * Bash with POSIX compliance for shell scripts
    * curl and jq for REST API testing in shell scripts
    * systemd for service management in production
* **Performance/Resource Notes:**
    * Automation suite can be resource-intensive; provides parallel execution limits
    * Recipe scraping tests support configurable parallel execution (`max_parallel=5`)
    * Service configuration includes resource limits for t3.small EC2 instances
* **Security Notes:**
    * Production scripts handle sensitive environment variables and secrets
    * Cleanup scripts require root privileges for system resource management

## 5. How to Work With This Code

* **Setup:**
    * Ensure .NET 8 SDK installed with required global tools (`dotnet tool list -g`)
    * For integration tests: Install and start Docker Desktop
    * For recipe scraping: Ensure `Config/site_selectors.json` is properly configured
* **Testing:**
    * **Location:** Scripts are primarily utilities; tested through their execution and output validation
    * **How to Run:** Each script includes usage documentation via `--help` or internal documentation
    * **Testing Strategy:** Scripts perform end-to-end validation of development and deployment workflows
* **Common Usage Patterns:**
    ```bash
    # Complete development workflow
    ./Scripts/generate-api-client.sh
    ./Scripts/run-test-suite.sh both                   # Run comprehensive testing
    
    # Mode-specific testing
    ./Scripts/run-test-suite.sh automation             # HTML coverage reports
    ./Scripts/run-test-suite.sh report                 # AI-powered analysis
    ./Scripts/run-test-suite.sh report json            # CI/CD integration
    
    # Testing specific components
    ./Scripts/test_sites.sh
    
    # Manual integration test environment setup
    docker-compose -f ./Scripts/docker-compose.integration.yml up -d
    
    # Production deployment
    ./Scripts/start-server.sh
    ```
* **Common Pitfalls / Gotchas:**
    * Docker group membership issues on Linux/WSL2 - scripts provide `sg docker` fallback
    * API server port conflicts are now automatically resolved by the generation script
    * Recipe scraping tests depend on external site availability and selector accuracy
    * API startup issues will be captured in temporary log files for debugging

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`api-server`](../api-server/README.md) - Source for API client generation and deployment artifacts
    * [`api-server.Tests`](../api-server.Tests/README.md) - Target for generated API clients and test execution
* **External Library Dependencies:**
    * `.NET 8 SDK` - Core runtime and build tools
    * `Docker Desktop` - Container runtime for integration tests
    * `refitter` - .NET tool for Refit client generation
    * `dotnet-reportgenerator-globaltool` - Coverage report generation
    * `Swashbuckle.AspNetCore.Cli` - OpenAPI/Swagger JSON generation
    * `curl` and `jq` - REST API testing in shell scripts
    * `Playwright` - Browser automation dependencies
* **Dependents (Impact of Changes):**
    * [`CI/CD Pipeline`](../.github/workflows/01-build.yml) - Automation scripts mirror CI/CD workflow steps
    * [Local Development Workflow](../CLAUDE.md) - Scripts provide essential development commands
    * Production deployment processes rely on service management scripts

## 7. Rationale & Key Historical Context

* **Cross-Platform Script Pairs:** Both PowerShell and Bash versions exist to support development on Windows, macOS, and Linux without requiring developers to install additional runtimes
* **Unified Test Suite Consolidation:** The `run-test-suite.sh` script consolidates functionality from `run-automation-suite.sh`, eliminating ~300 lines of duplicate code while providing mode-based architecture for different testing needs
* **Mode-Based Architecture:** Three modes (automation, report, both) provide flexibility for different use cases while maintaining a single, maintainable codebase
* **Backward Compatibility:** Legacy script names are preserved as simple forwarders to maintain existing workflows and CI/CD integrations
* **Docker Access Handling:** Special handling for Docker group membership reflects common issues in Linux/WSL2 environments where Testcontainers require Docker socket access
* **Service Management Design:** Production scripts follow systemd best practices with resource limits appropriate for t3.small EC2 instances and proper cleanup handling

## 8. Known Issues & TODOs

* **Recipe Scraping Brittleness:** Site selector configurations in `test_sites.sh` require ongoing maintenance as target websites change their DOM structure
* **Windows PowerShell Compatibility:** Scripts target PowerShell Core; some compatibility issues may exist with Windows PowerShell 5.1
* **Error Recovery:** Some scripts could benefit from more sophisticated error recovery and retry mechanisms
* **Documentation Updates:** Script internal documentation should be synchronized when command-line options or behavior changes

---