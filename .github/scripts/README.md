# Module/Directory: .github/scripts

**Last Updated:** 2025-08-05

**Parent:** [`.github`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory contains the core CI/CD pipeline logic extracted from GitHub Actions workflows to create maintainable, testable, and reusable automation scripts.
* **Key Responsibilities:** 
    * Backend build, test, and deployment automation for .NET applications
    * Frontend build, test, and deployment automation for Angular applications
    * Security scanning orchestration (CodeQL, dependency scanning, secrets detection)
    * Quality analysis coordination (standards compliance, tech debt analysis)
    * Test result analysis and reporting automation
    * Common utility functions for all pipeline operations
* **Why it exists:** To separate workflow orchestration from business logic, enabling local testing, easier debugging, and maintainable CI/CD processes. This architecture follows the principle of keeping GitHub Actions files minimal while moving complex logic into testable scripts.

## 2. Architecture & Key Concepts

* **High-Level Design:** The scripts are organized into functional categories with shared utilities:
    * **Build Scripts:** `build-backend.sh`, `build-frontend.sh` - Handle compilation, testing, and artifact creation
    * **Deployment Scripts:** `deploy-backend.sh`, `deploy-frontend.sh` - Manage EC2 deployments with health checks
    * **Analysis Scripts:** `run-security-scans.sh` - Coordinate security analysis
    * **Shared Utilities:** `common-functions.sh` - Logging, error handling, Docker access, artifact management
* **Core Workflow Integration:** Scripts integrate with GitHub Actions through:
    1. Environment variable consumption for configuration
    2. GitHub Actions output generation for workflow communication
    3. Artifact upload/download coordination
    4. Exit code conventions for workflow decision making
* **Error Handling:** Consistent error handling with colored logging, quality gates, and proper exit codes
* **Docker Integration:** Automatic detection and handling of Docker group membership issues for Testcontainers

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for workflow callers):**
    * **Build Scripts** (`build-backend.sh`, `build-frontend.sh`):
        * **Critical Preconditions:** `.NET 8 SDK installed (backend)`, `Node.js 18.x (frontend)`, `clean working directory`, `Docker running for integration tests`
        * **Critical Postconditions:** `Artifacts created in specified directories`, `test results available for analysis`, `exit code 0 on success`
        * **Non-Obvious Error Handling:** 
            - Automatically handles Docker group membership issues
            - Creates detailed test reports and artifacts even on failure
            - Ensures artifact directories always contain meaningful content for debugging
            - Enhanced build artifact creation with comprehensive metadata
    * **Deployment Scripts** (`deploy-backend.sh`, `deploy-frontend.sh`):
        * **Critical Preconditions:** `AWS credentials configured`, `deployment artifacts available`, `target EC2 instances accessible`
        * **Critical Postconditions:** `Application deployed and healthy`, `health checks passed`, `rollback artifacts created`
        * **Non-Obvious Error Handling:** Automatic rollback on deployment failure; health check validation with retries
    * **Analysis Scripts** (`run-security-scans.sh`):
        * **Critical Preconditions:** `Source code available`, `previous build artifacts for context`, `API tokens for AI analysis`
        * **Critical Postconditions:** `Analysis reports generated`, `quality gates evaluated`, `auto-issues created for findings`
        * **Non-Obvious Error Handling:** Graceful degradation when AI services unavailable; consolidates multiple scan results
* **Critical Assumptions:**
    * **Environment Dependencies:** GitHub Actions runner environment with standard tools
    * **Network Access:** Internet access for package downloads, AWS API access, external AI services
    * **File System Permissions:** Write access to workspace, temporary directories, and artifact locations
    * **Docker Access:** Docker daemon available for integration testing with proper user permissions

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:**
    * All scripts source `common-functions.sh` for shared functionality
    * Environment variables follow `UPPERCASE_SNAKE_CASE` convention
    * Output variables use `GITHUB_OUTPUT` for workflow communication
    * Artifact naming follows `{type}-{timestamp}-{commit-sha}` pattern
* **Directory Structure:**
    * Scripts are executable and follow `#!/bin/bash` shebang
    * Temporary files use `/tmp/pipeline-{operation}-{timestamp}` naming
    * Artifacts are organized by type: `artifacts/{build|test|security|quality}/`
* **Technology Choices:**
    * Bash with `set -euo pipefail` for strict error handling
    * POSIX-compliant commands for portability
    * jq for JSON processing and API interactions
    * curl for HTTP requests and health checks
* **Performance/Resource Notes:**
    * Build scripts support parallel execution where beneficial
    * Docker operations include timeout and retry logic
    * Large artifact handling with compression and cleanup
* **Security Notes:**
    * All scripts handle secrets through environment variables only
    * No secrets are logged or written to files
    * Temporary files are cleaned up automatically
    * **AWS OIDC Authentication:** Deployment scripts use short-lived OIDC tokens instead of static AWS credentials
* **AWS OIDC Integration:**
    * Scripts support both traditional AWS credentials and OIDC token authentication
    * OIDC authentication provides enhanced security with repository-scoped, time-limited access
    * AWS credentials are automatically configured by GitHub Actions before script execution
    * No manual AWS credential management required in deployment scripts

## 5. How to Work With This Code

* **Setup:**
    * Ensure script execute permissions: `chmod +x Scripts/Pipeline/*.sh`
    * Install required tools: `.NET 8 SDK`, `Node.js 18.x`, `Docker Desktop`
    * For AI analysis: Configure `CLAUDE_CODE_OAUTH_TOKEN` environment variable
    * For deployments: AWS credentials are automatically configured via OIDC in GitHub Actions
    * For local testing: Configure AWS credentials manually or use `aws configure`
* **Testing:**
    * **Location:** Scripts are tested through local execution and integration tests in CI/CD
    * **How to Run:** Each script includes `--help` option and can be executed locally
    * **Testing Strategy:** Scripts can be tested independently with mock environment variables
* **Common Usage Patterns:**
    ```bash
    # Local development testing
    ./Scripts/Pipeline/build-backend.sh
    ./Scripts/Pipeline/build-frontend.sh
    
    # Security analysis (requires tokens)
    CLAUDE_CODE_OAUTH_TOKEN=xxx ./Scripts/Pipeline/run-security-scans.sh
    
    # Deployment (requires AWS credentials)
    ./Scripts/Pipeline/deploy-backend.sh production
    ./Scripts/Pipeline/deploy-frontend.sh production
    ```
* **Common Pitfalls / Gotchas:**
    * Docker group membership issues on Linux/WSL2 - scripts auto-detect and handle
    * AWS credentials must be configured before deployment scripts
    * AI analysis scripts require network access to external services
    * Integration tests require Docker daemon running

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Scripts/run-test-suite.sh`](../run-test-suite.sh) - Comprehensive test execution
    * [`Scripts/generate-api-client.sh`](../generate-api-client.sh) - API client generation
    * [`Code/Zarichney.Server`](../../Code/Zarichney.Server/README.md) - Backend application source
    * [`Code/Zarichney.Website`](../../Code/Zarichney.Website/README.md) - Frontend application source
* **External Library Dependencies:**
    * `.NET 8 SDK` - Backend build and test execution
    * `Node.js 18.x` and `npm` - Frontend build and dependency management
    * `Docker Desktop` - Container runtime for integration tests
    * `AWS CLI` - Cloud deployment and service management
    * `curl` and `jq` - HTTP requests and JSON processing
    * `git` - Source control operations and change detection
* **Dependents (Impact of Changes):**
    * [GitHub Workflows](../../.github/workflows/) - All new workflows depend on these scripts
    * [Local Development](../../CLAUDE.md) - Development commands reference these scripts
    * CI/CD Pipeline - Deployment automation relies on these scripts

## 7. Rationale & Key Historical Context

* **Script Extraction Philosophy:** Moving logic from GitHub Actions YAML to bash scripts enables local testing, easier debugging, and version control of complex automation logic
* **Common Functions Design:** Centralized logging, error handling, and utility functions reduce duplication and ensure consistent behavior across all pipeline operations
* **Docker Access Handling:** Special handling for Docker group membership reflects common issues in CI/CD environments where Testcontainers require Docker socket access
* **Artifact Management:** Standardized artifact creation and management supports both local development and CI/CD workflows with consistent naming and organization
* **Error Handling Strategy:** Strict error handling with proper exit codes ensures workflow decision making is reliable and failures are properly propagated

## 8. Known Issues & TODOs

* **Windows Compatibility:** Scripts are bash-focused; PowerShell equivalents may be needed for Windows development
* **Secrets Management:** Currently relies on environment variables; could benefit from more sophisticated secrets handling
* **Performance Optimization:** Large artifact handling could be optimized with streaming and incremental uploads
* **Error Recovery:** Some operations could benefit from automatic retry mechanisms with exponential backoff
* **Documentation Sync:** Script help text should be automatically synchronized with this documentation

---