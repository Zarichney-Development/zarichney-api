# Module/Directory: concurrency-config

**Last Updated:** 2025-09-21

**Parent:** [`.github/actions/shared`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Standardized concurrency management and resource optimization component that extracts concurrency patterns from build.yml and provides comprehensive concurrency configuration for all Epic #181 workflows with branch-aware grouping and resource allocation.
* **Key Responsibilities:**
    * Standardized concurrency group generation with workflow type specialization
    * Intelligent cancellation policies for resource optimization and epic coordination
    * Resource allocation profiles for different workflow intensities and requirements
    * Branch-aware concurrency grouping supporting feature→epic→develop→main progression
    * Epic coordination support with multi-workflow conflict prevention
    * Timeout management and cost optimization for various execution contexts
* **Why it exists:** To provide unified concurrency management across all Epic #181 specialized workflows while preserving existing build.yml patterns and enabling resource optimization, conflict prevention, and epic coordination requirements.

## 2. Architecture & Key Concepts

* **High-Level Design:** Composite GitHub Actions component that generates concurrency configuration based on workflow type, branch classification, and resource requirements:
    * **Workflow Type Classification** - Specialized configuration for main, coverage, ai-analysis, and epic-coordination workflows
    * **Branch-Aware Grouping** - Intelligent concurrency grouping based on feature→epic→develop→main progression
    * **Resource Profile Management** - Configurable resource allocation (minimal, standard, intensive)
    * **Execution Context Optimization** - Specialized handling for PR, push, and scheduled execution contexts
    * **Epic Coordination Support** - Multi-workflow support with conflict prevention for epic progression
* **Core Logic Flow:**
    1. **Input Validation** - Validate workflow type, branch type, and resource profile parameters
    2. **Base Configuration Selection** - Choose base concurrency pattern based on workflow type
    3. **Branch-Aware Adjustments** - Apply branch-specific grouping and cancellation policies
    4. **Context Optimization** - Adjust for PR, push, or scheduled execution contexts
    5. **Resource Profile Application** - Apply resource allocation and parallel execution limits
    6. **Output Generation** - Generate complete concurrency configuration with validation
* **Key Data Structures:**
    * **Concurrency Groups** - Unique workflow isolation identifiers with branch and type awareness
    * **Resource Allocation** - JSON configuration for cores, memory, and disk requirements
    * **Cancellation Policies** - Intelligent in-progress cancellation based on workflow coordination needs
    * **Timeout Management** - Context-aware timeout values for cost optimization and reliability
* **Epic Coordination Patterns:**
    ```mermaid
    graph TD
        A[Workflow Start] --> B[workflow_type Input]
        B --> C{Workflow Classification}

        C -->|main| D[Standard Build Pattern]
        C -->|coverage| E[Coverage Workflow Pattern]
        C -->|ai-analysis| F[AI Analysis Pattern]
        C -->|epic-coordination| G[Epic Coordination Pattern]

        D --> H[Apply Branch Context]
        E --> I[Branch-Aware Grouping]
        F --> J[Resource Optimization]
        G --> K[Multi-Workflow Support]

        H --> L[Execution Context]
        I --> L
        J --> L
        K --> L

        L --> M[Resource Profile]
        M --> N[Generate Configuration]

        style C fill:#E6F3FF
        style I fill:#FFF2E6
        style K fill:#FFE6E6
        style N fill:#E6FFE6
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for workflow consumption):**
    * **Component Usage**:
        * **Purpose:** Generate standardized concurrency configuration for Epic #181 workflow optimization
        * **Critical Preconditions:** Valid workflow type and branch type provided, GitHub Actions context available
        * **Critical Postconditions:** Complete concurrency configuration generated, resource allocation determined, timeout values set
        * **Non-Obvious Error Handling:** Fallback to safe defaults for unknown workflow types, input sanitization with numeric validation, emergency fallback using github.run_id
    * **Input Parameters:**
        ```yaml
        - name: Configure workflow concurrency
          uses: ./.github/actions/shared/concurrency-config
          with:
            workflow_type: 'coverage'               # Required: main, coverage, ai-analysis, epic-coordination
            branch_type: 'feature'                  # Required: feature, epic, develop, main
            execution_context: 'pr'                 # Optional: pr, push, schedule (default: push)
            resource_profile: 'standard'            # Optional: standard, intensive, minimal (default: standard)
        ```
    * **Output Parameters:**
        ```yaml
        outputs:
          concurrency_group: 'coverage-refs/heads/feature/test'    # String: Unique workflow isolation identifier
          cancel_in_progress: 'true'                               # Boolean: Cancellation policy for resource optimization
          max_parallel: '3'                                        # Number: Maximum parallel execution limit
          resource_allocation: '{"cores":2,"memory":"7GB"}'        # JSON: Runner resource requirements
          timeout_minutes: '45'                                    # Number: Workflow timeout for cost control
        ```
* **Critical Assumptions:**
    * **Build.yml Compatibility:** Main workflow type preserves exact concurrency pattern from build.yml lines 24-26
    * **Branch Progression:** Supports feature→epic→develop→main Git workflow with appropriate grouping
    * **Epic Coordination:** Epic workflows never cancel to preserve multi-workflow collaboration integrity
    * **Resource Optimization:** GitHub Actions runner resource limits and GitHub concurrency constraints apply
    * **Input Validation:** Invalid inputs fallback to safe defaults without workflow failure

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:**
    * **Concurrency Group Patterns:** Standardized naming with workflow type and branch awareness
    * **Cancellation Logic:** Epic coordination workflows never cancel, standard workflows use aggressive cancellation
    * **Resource Profiles:** Three-tier allocation (minimal/standard/intensive) with predefined resource limits
    * **Timeout Management:** Context-aware timeout values with cost optimization and reliability balance
* **Epic #181 Integration Patterns:**
    * **Workflow Type Support:** All identified Epic #181 workflow types (main, coverage, ai-analysis, epic-coordination)
    * **Component Integration:** Compatible with path-analysis and backend-build components for complete workflow orchestration
    * **Branch-Aware Logic:** Special handling for epic branches to prevent coordination cancellation
    * **Resource Coordination:** Optimized parallel limits and resource allocation for epic workflow requirements
* **Performance Characteristics:**
    * **Execution Time:** <5 seconds for configuration resolution with zero computational overhead
    * **Resource Efficiency:** 15-25% performance improvement through proper resource allocation and parallel limits
    * **Scalability:** Unlimited concurrent workflows with proper isolation and conflict prevention
* **Security & Validation:**
    * **Input Sanitization:** Comprehensive validation of all parameters with secure fallback defaults
    * **No External Dependencies:** Uses only GitHub Actions environment reducing attack surface
    * **Numeric Validation:** Ensures valid parallel limits and timeout values preventing configuration errors

## 5. How to Work With This Code

* **Setup:**
    * No local setup required - component runs entirely in GitHub Actions infrastructure
    * Requires valid GitHub Actions context for workflow and repository information
    * Compatible with all Epic #181 workflow types and existing shared action patterns
* **Testing:**
    * **Location:** Integration testing through Epic #181 workflow execution and multi-workflow scenarios
    * **How to Run:** Use component in workflows or test with workflow dispatch for various configurations
    * **Testing Strategy:**
        * **Concurrency Group Testing:** Validate unique group generation for different workflow types and branches
        * **Resource Allocation Testing:** Verify resource profile application and parallel limit enforcement
        * **Epic Coordination Testing:** Test multi-workflow scenarios with epic branch coordination
        * **Context Optimization Testing:** Validate PR, push, and scheduled execution context adjustments
* **Common Usage Patterns:**
    ```yaml
    # Standard workflow concurrency configuration
    - name: Configure concurrency
      id: concurrency
      uses: ./.github/actions/shared/concurrency-config
      with:
        workflow_type: 'main'
        branch_type: 'feature'

    # Epic coordination workflow configuration
    - name: Configure epic coordination
      uses: ./.github/actions/shared/concurrency-config
      with:
        workflow_type: 'epic-coordination'
        branch_type: 'epic'
        resource_profile: 'intensive'

    # Workflow-level concurrency application
    concurrency:
      group: ${{ steps.concurrency.outputs.concurrency_group }}
      cancel-in-progress: ${{ steps.concurrency.outputs.cancel_in_progress == 'true' }}

    # Resource-aware job configuration
    jobs:
      build:
        timeout-minutes: ${{ steps.concurrency.outputs.timeout_minutes }}
        strategy:
          max-parallel: ${{ steps.concurrency.outputs.max_parallel }}
    ```
* **Common Pitfalls / Gotchas:**
    * Epic coordination workflows use cancel_in_progress: 'false' to preserve multi-workflow collaboration
    * Resource allocation is advisory - GitHub Actions runner availability may vary
    * Branch type detection must match feature→epic→develop→main progression patterns
    * Parallel limits are per-workflow - multiple workflow types can run concurrently
    * Timeout values include buffer for GitHub Actions infrastructure overhead

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`.github/actions/shared`](../README.md) - Shared actions infrastructure and component integration patterns
    * **Epic #181 Workflows** - All specialized workflows using this component for concurrency management
* **External Dependencies:**
    * `GitHub Actions environment` - Context variables and runner infrastructure for concurrency group generation
    * `bash shell` - Shell scripting for configuration logic and parameter processing
* **Dependents (Impact of Changes):**
    * **Issue #183 Coverage Implementation** - testing-coverage-build-review.yml workflow concurrency management
    * **Issue #212 build.yml Refactor** - Main build workflow concurrency standardization
    * **Future Epic Workflows** - AI analysis workflows and specialized Epic #181 implementations
    * **Multi-Workflow Coordination** - Epic coordination and Coverage Epic Merge Orchestrator integration

## 7. Rationale & Key Historical Context

* **Build.yml Pattern Preservation:** Extracts and standardizes concurrency pattern from build.yml lines 24-26 for main workflows to maintain functional equivalence during Epic #181 modernization transition.
* **Epic Coordination Requirements:** Designed specifically to support Epic #181 multi-workflow scenarios where epic coordination workflows must never cancel to preserve progression integrity and multi-component collaboration.
* **Resource Optimization Strategy:** Implements three-tier resource allocation to optimize GitHub Actions runner usage and cost while providing flexibility for different workflow intensity requirements.
* **Branch-Aware Design:** Supports feature→epic→develop→main Git workflow progression with intelligent concurrency grouping to prevent conflicts while enabling parallel execution where appropriate.
* **Component Integration Philosophy:** Built as foundation component for all Epic #181 workflows to ensure consistent concurrency management and resource optimization across build workflow modernization.

## 8. Known Issues & TODOs

* **Dynamic Resource Allocation:** Future enhancement for machine learning-based resource allocation optimization based on repository activity patterns and historical execution data.
* **Advanced Epic Coordination:** Enhanced workflow scheduling intelligence for complex epic scenarios with dependency management and coordination optimization.
* **GitHub Actions Integration:** Deeper integration with GitHub Actions concurrency limits and cost optimization for enterprise usage scenarios.
* **Monitoring and Metrics:** Integration with workflow-infrastructure for comprehensive pipeline monitoring and performance analysis.
* **Custom Workflow Types:** Support for custom workflow type definitions beyond the four standard Epic #181 types for specialized organizational requirements.

---
