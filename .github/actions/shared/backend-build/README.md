# Module/Directory: backend-build

**Last Updated:** 2025-09-21

**Parent:** [`.github/actions/shared`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Comprehensive .NET backend build execution component that extracts build logic from build.yml while adding coverage flexibility, zero-warning enforcement, and specialized testing integration for Epic #181 workflow modernization.
* **Key Responsibilities:**
    * .NET solution build execution with functional equivalence to build.yml patterns
    * Zero-warning policy enforcement with environment-aware flexibility
    * Test coverage collection and reporting with threshold management
    * Build artifact management and structured output generation
    * Integration with setup-environment and validate-test-suite shared actions
    * Epic #181 coverage workflow enablement through specialized build patterns
* **Why it exists:** To provide standardized backend build execution that supports Epic #181 build workflow modernization while maintaining exact functional equivalence with existing build.yml logic and enabling coverage-focused workflow specialization.

## 2. Architecture & Key Concepts

* **High-Level Design:** Composite GitHub Actions component that orchestrates complete .NET backend build pipeline:
    * **Environment Setup** - Uses setup-environment action for .NET 8.0.x configuration
    * **Build Execution** - Executes build-backend.sh script with proper flags and environment variables
    * **Coverage Management** - Environment-aware coverage flexibility with threshold enforcement
    * **Test Integration** - Comprehensive test execution with structured result processing
    * **Artifact Management** - Build and test artifact collection with retention policies
    * **Quality Validation** - Integration with validate-test-suite for baseline standards
* **Core Logic Flow:**
    1. **Environment Configuration** - Setup .NET development environment with automatic tool restoration
    2. **Coverage Flexibility Determination** - Analyze branch/PR context for coverage policy enforcement
    3. **Build Script Execution** - Execute build-backend.sh with appropriate flags and environment variables
    4. **Result Processing** - Extract build status, warnings, test results, and coverage data
    5. **Artifact Generation** - Collect build artifacts and test results for workflow consumption
    6. **Baseline Validation** - Run validate-test-suite for quality gate verification
* **Key Data Structures:**
    * **Build Outputs** - Comprehensive status reporting (build_success, warning_count, test_success)
    * **Coverage Results** - Coverage percentage and detailed JSON analysis data
    * **Error Analysis** - Detailed error categorization (warnings, compilation, unknown)
    * **Artifact Collection** - JSON arrays of build and test artifact paths
* **Coverage Flexibility Logic:**
    ```mermaid
    graph TD
        A[Build Start] --> B{Branch Analysis}
        B -->|test/* branch| C[Enable Coverage Flexibility]
        B -->|develop branch| C
        B -->|low-coverage-allowed label| C
        B -->|other branches| D[Standard Coverage Policy]

        C --> E[Execute with --allow-low-coverage]
        D --> F[Execute with Standard Thresholds]

        E --> G[Process Results]
        F --> G
        G --> H[Validate Baselines]
        H --> I[Upload Artifacts]

        style B fill:#E6F3FF
        style C fill:#FFF2E6
        style D fill:#FFE6E6
        style G fill:#E6FFE6
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for workflow consumption):**
    * **Component Usage**:
        * **Purpose:** Complete .NET backend build with coverage flexibility and zero-warning enforcement
        * **Critical Preconditions:** .NET solution file available, setup-environment action accessible, build-backend.sh script executable
        * **Critical Postconditions:** Build status determined, test results processed, coverage data collected, artifacts uploaded
        * **Non-Obvious Error Handling:** Warning-as-error detection, build failure categorization, graceful coverage result handling when files missing
    * **Input Parameters:**
        ```yaml
        - name: Execute backend build
          uses: ./.github/actions/shared/backend-build
          with:
            solution_path: 'zarichney-api.sln'      # Optional: Solution file path
            coverage_enabled: 'false'               # Optional: Enable coverage reporting
            warning_as_error: 'true'                # Optional: Zero-warning policy enforcement
            test_filter: ''                         # Optional: Test filtering (future enhancement)
            configuration: 'Release'                # Optional: Build configuration
            verbosity: 'normal'                     # Optional: MSBuild verbosity level
        ```
    * **Output Parameters:**
        ```yaml
        outputs:
          build_success: 'true'                     # Boolean: Build completed without errors
          warning_count: '0'                        # Number: Total build warnings detected
          test_success: 'true'                      # Boolean: All tests passed successfully
          coverage_percentage: '18.5'               # Number: Test coverage percentage
          coverage_results: '{...}'                 # JSON: Detailed coverage analysis
          build_artifacts: '[...]'                  # JSON Array: Build artifact paths
          error_details: ''                         # String: Detailed error information
        ```
* **Critical Assumptions:**
    * **Script Integration:** Assumes build-backend.sh script maintains stable interface and flag compatibility
    * **Coverage Flexibility:** Uses exact same branch/label detection patterns as build.yml for consistency
    * **Environment Variables:** Relies on CI_ENVIRONMENT, QUALITY_GATE_ENABLED, and COVERAGE_THRESHOLD patterns
    * **Test Result Format:** Assumes TestResults/parsed_results.json and coverage_results.json standard formats
    * **Zero-Warning Policy:** MSBuild TreatWarningsAsErrors enforcement with environment-aware exceptions

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:**
    * **Coverage Thresholds:** Standard 16% threshold with environment-aware flexibility patterns
    * **Warning Policy:** Zero-tolerance warning enforcement with graceful environment exceptions
    * **Build Configuration:** Release configuration default with configurable override support
    * **Artifact Retention:** 7-day retention for build and test artifacts with structured naming
* **Epic #181 Integration Patterns:**
    * **Coverage Build Support:** Enables testing-coverage-build-review.yml specialized workflow execution
    * **Path-Aware Execution:** Designed for integration with path-analysis component outputs
    * **Concurrency Coordination:** Compatible with concurrency-config resource management
* **Performance Characteristics:**
    * **Execution Time:** 2-5 minutes target (build-dependent) for complete pipeline
    * **Resource Usage:** Standard GitHub Actions runner compatibility with minimal overhead
    * **Script Integration:** Zero overhead wrapper around existing build-backend.sh patterns
* **Security & Quality Controls:**
    * **Zero-Warning Enforcement:** MSBuild TreatWarningsAsErrors with comprehensive warning detection
    * **Build Failure Analysis:** Categorized error analysis (warnings, compilation, unknown)
    * **Input Validation:** Proper sanitization of all input parameters with safe defaults
    * **Environment Security:** Read-only git operations with secure artifact management

## 5. How to Work With This Code

* **Setup:**
    * Requires .NET solution file (default: zarichney-api.sln) in repository root
    * Requires build-backend.sh script at ./.github/scripts/build-backend.sh with proper execution permissions
    * Requires setup-environment and validate-test-suite shared actions availability
    * No additional local setup required for GitHub Actions execution
* **Testing:**
    * **Location:** Integration testing through workflow execution and Epic #181 validation
    * **How to Run:** Use component in workflows or test with manual workflow dispatch
    * **Testing Strategy:**
        * **Build Integration Testing:** Validate functional equivalence with build.yml execution
        * **Coverage Flexibility Testing:** Test branch/label detection for coverage policy enforcement
        * **Error Handling Testing:** Verify warning detection and build failure categorization
        * **Epic Coordination Testing:** Ensure proper integration with path-analysis and concurrency-config
* **Common Usage Patterns:**
    ```yaml
    # Standard backend build execution
    - name: Execute backend build
      id: backend-build
      uses: ./.github/actions/shared/backend-build
      with:
        coverage_enabled: 'true'
        warning_as_error: 'true'

    # Coverage-focused build for epic workflows
    - name: Execute coverage build
      uses: ./.github/actions/shared/backend-build
      with:
        solution_path: 'zarichney-api.sln'
        coverage_enabled: 'true'
        configuration: 'Release'

    # Build status conditional execution
    - name: Process build results
      if: steps.backend-build.outputs.build_success == 'true'
      run: |
        echo "Coverage: ${{ steps.backend-build.outputs.coverage_percentage }}%"
        echo "Test success: ${{ steps.backend-build.outputs.test_success }}"
    ```
* **Common Pitfalls / Gotchas:**
    * Warning-as-error policy can cause build failures on any compiler warnings
    * Coverage flexibility depends on branch naming and PR label detection
    * Large test suites may affect execution time but maintain functional correctness
    * Build artifacts depend on proper artifacts/backend directory structure
    * Test result parsing requires standard TestResults directory structure

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`.github/actions/shared/setup-environment`](../setup-environment/README.md) - .NET environment configuration and tool restoration
    * [`.github/actions/shared/validate-test-suite`](../validate-test-suite/README.md) - Test baseline validation and quality gates
    * [`.github/scripts/build-backend.sh`](../../../Scripts/Pipeline/README.md) - Core build execution script with flag support
* **External Dependencies:**
    * `.NET 8.0.x SDK` - Development environment for compilation and testing
    * `jq` - JSON processing for test result parsing and artifact generation
    * `GitHub Actions environment` - Runner infrastructure and artifact management
* **Dependents (Impact of Changes):**
    * **Epic #181 Coverage Workflows** - testing-coverage-build-review.yml and specialized testing workflows
    * **Issue #212 build.yml Refactor** - Main build workflow modernization using this component
    * **Issue #183 Foundation Implementation** - Coverage workflow creation and validation
    * **Future Epic Workflows** - AI analysis workflows and specialized build patterns

## 7. Rationale & Key Historical Context

* **Build.yml Logic Preservation:** Extracts exact backend build execution from build.yml lines 116-189 to maintain functional equivalence during Epic #181 modernization while enabling component reusability across specialized workflows.
* **Coverage Flexibility Strategy:** Implements same branch and label detection patterns as build.yml for coverage policy enforcement to ensure consistent behavior across workflow modernization transition.
* **Zero-Warning Policy Design:** Maintains MSBuild TreatWarningsAsErrors enforcement with comprehensive warning detection to preserve code quality standards while providing detailed failure analysis.
* **Integration Pattern Choice:** Uses existing setup-environment and validate-test-suite actions to maintain stability and leverage proven infrastructure while adding specialized Epic #181 capabilities.
* **Artifact Management Approach:** Implements structured artifact collection with retention policies to support Epic #181 workflow coordination and debugging requirements.

## 8. Known Issues & TODOs

* **Test Filtering Enhancement:** Implement test_filter input parameter for advanced test execution scenarios and coverage workflow specialization.
* **Dynamic Coverage Thresholds:** Consider implementing configurable coverage thresholds for different workflow types and Epic #181 progression requirements.
* **Build Performance Optimization:** Large solution builds could benefit from incremental build patterns and dependency caching strategies.
* **Advanced Error Analysis:** Enhance build failure categorization with more detailed error type detection and remediation guidance.
* **Epic Coordination Integration:** Add specialized outputs for epic workflow coordination and multi-component build orchestration.

---
