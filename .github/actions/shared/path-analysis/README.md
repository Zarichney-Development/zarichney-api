# Module/Directory: path-analysis

**Last Updated:** 2025-09-21

**Parent:** [`.github/actions/shared`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Intelligent path-based change detection and categorization component that extracts base reference determination logic from build.yml and enhances existing check-paths functionality with structured JSON outputs for workflow optimization.
* **Key Responsibilities:**
    * Base reference determination supporting feature→epic→develop→main progression patterns
    * Enhanced path categorization with structured JSON outputs
    * Boolean outputs for workflow filtering and conditional execution
    * Integration with existing check-paths shared action for comprehensive change analysis
    * Epic #181 workflow specialization enablement through path-aware decision making
* **Why it exists:** To enable intelligent workflow execution by providing standardized path analysis that supports Epic #181 build workflow modernization, coverage-focused execution patterns, and branch-aware change detection for specialized workflows.

## 2. Architecture & Key Concepts

* **High-Level Design:** Composite GitHub Actions component that wraps and enhances the existing check-paths action while extracting base reference determination logic from build.yml:
    * **Base Reference Determination** - Extracts exact logic from build.yml lines 57-68 with feature→epic→develop→main support
    * **Path Change Detection** - Uses existing check-paths action for core functionality
    * **Enhanced Summary Generation** - Adds structured JSON output layer with category-based analysis
    * **Boolean Workflow Outputs** - Provides direct integration points for workflow conditional execution
* **Core Logic Flow:**
    1. **Base Reference Resolution** - Analyzes GitHub context (event type, branch, PR) to determine appropriate base reference
    2. **Path Analysis Execution** - Delegates to check-paths action with resolved base reference
    3. **Change Categorization** - Processes check-paths outputs through change threshold and category logic
    4. **Structured Output Generation** - Creates JSON summary with metadata and boolean outputs for workflow consumption
* **Key Data Structures:**
    * **Boolean Outputs** - Direct workflow integration (has_backend_changes, has_frontend_changes, has_docs_changes, has_config_changes)
    * **JSON Change Summary** - Comprehensive analysis with categories, metadata, and raw summary data
    * **Base Reference Resolution** - Branch-aware reference determination for accurate change detection
* **Epic #181 Integration Pattern:**
    ```mermaid
    graph TD
        A[Workflow Start] --> B[path-analysis]
        B --> C{Has Backend Changes?}
        B --> D{Has Frontend Changes?}
        B --> E{Has Config Changes?}

        C -->|true| F[Execute Backend Build]
        D -->|true| G[Execute Frontend Build]
        E -->|true| H[Execute Config Validation]

        C -->|false| I[Skip Backend]
        D -->|false| J[Skip Frontend]
        E -->|false| K[Skip Config]

        F --> L[Coverage Analysis]
        G --> L
        H --> L
        L --> M[Epic Coordination]

        style B fill:#E6F3FF
        style F fill:#FFF2E6
        style G fill:#FFF2E6
        style H fill:#FFF2E6
        style L fill:#E6FFE6
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for workflow consumption):**
    * **Component Usage**:
        * **Purpose:** Path-based change detection with Epic #181 workflow optimization support
        * **Critical Preconditions:** Git repository with valid history, GitHub Actions context available, check-paths action accessible
        * **Critical Postconditions:** Boolean change indicators set, JSON change summary generated, total change count available
        * **Non-Obvious Error Handling:** Graceful fallback to origin/main for unknown event types, input validation with safe defaults, threshold sanitization prevents invalid values
    * **Input Parameters:**
        ```yaml
        - name: Analyze repository changes
          uses: ./.github/actions/shared/path-analysis
          with:
            base_ref: ''                    # Optional: Override base reference determination
            change_threshold: '1'           # Optional: Minimum changes to trigger category (default: 1)
            category_rules: ''              # Optional: Future enhancement for custom categorization
        ```
    * **Output Parameters:**
        ```yaml
        outputs:
          has_backend_changes: 'true'       # Boolean: Backend code files modified
          has_frontend_changes: 'false'     # Boolean: Frontend code files modified
          has_docs_changes: 'true'          # Boolean: Documentation files modified
          has_config_changes: 'false'       # Boolean: Configuration files modified
          change_summary: '{...}'           # JSON: Detailed change analysis with metadata
          total_changes: '3'                # Number: Total file changes detected
        ```
* **Critical Assumptions:**
    * **Base Reference Logic:** Uses exact build.yml patterns for feature→epic→develop→main progression support
    * **Integration Dependency:** Assumes check-paths shared action maintains stable interface and output format
    * **GitHub Context:** Relies on github.event_name, github.ref_name, github.head_ref for branch detection
    * **Change Detection:** Performance depends on Git history depth and repository size
    * **JSON Compatibility:** Assumes jq tool available in GitHub Actions environment for JSON processing

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:**
    * **Input Validation:** change_threshold must be positive integer (defaults to 1 if invalid)
    * **Base Reference Override:** base_ref input bypasses automatic detection when provided
    * **Boolean Output Format:** Uses 'true'/'false' strings for GitHub Actions compatibility
    * **JSON Structure:** Follows standardized format with categories, summary, and metadata sections
* **Epic #181 Integration Patterns:**
    * **Branch-Aware Logic:** Special handling for test/* branches and develop branch detection
    * **Coverage Workflow Support:** Boolean outputs enable coverage-build.yml conditional execution
    * **Component Reusability:** Designed for use across all Epic #181 specialized workflows
* **Performance Characteristics:**
    * **Execution Time:** <5 seconds target for path analysis operations
    * **Memory Usage:** Minimal (~50MB estimated) with no external dependencies
    * **Integration Overhead:** Zero compilation overhead, instant configuration resolution
* **Security Considerations:**
    * **Read-Only Operations:** Only performs git operations for change detection
    * **Input Sanitization:** Validates and sanitizes all input parameters
    * **No External Dependencies:** Uses only GitHub Actions environment and existing shared actions

## 5. How to Work With This Code

* **Setup:**
    * No local setup required - component runs in GitHub Actions infrastructure
    * Requires existing check-paths shared action in `.github/actions/shared/check-paths`
    * Git repository with valid history for change detection functionality
* **Testing:**
    * **Location:** Integration testing through workflow execution and Epic #181 component validation
    * **How to Run:** Use component in actual workflows or test with workflow dispatch events
    * **Testing Strategy:**
        * **Base Reference Logic Testing:** Validate branch detection for different GitHub event types
        * **Integration Testing:** Ensure clean integration with check-paths action outputs
        * **Epic Coordination Testing:** Verify boolean outputs enable proper workflow specialization
        * **Edge Case Testing:** Test with empty repositories, large change sets, merge conflicts
* **Common Usage Patterns:**
    ```yaml
    # Basic path analysis for workflow optimization
    - name: Analyze repository changes
      id: path-analysis
      uses: ./.github/actions/shared/path-analysis

    # Coverage workflow conditional execution
    - name: Execute coverage build
      if: steps.path-analysis.outputs.has_backend_changes == 'true'
      uses: ./.github/actions/shared/backend-build
      with:
        coverage_enabled: 'true'

    # Epic coordination with change summary
    - name: Process change summary
      run: |
        echo "Change analysis: ${{ steps.path-analysis.outputs.change_summary }}"
        echo "Total changes: ${{ steps.path-analysis.outputs.total_changes }}"
    ```
* **Common Pitfalls / Gotchas:**
    * Path analysis accuracy depends on proper fetch depth in checkout action
    * Boolean outputs are strings ('true'/'false') not actual booleans for GitHub Actions
    * Base reference determination follows feature→epic→develop→main progression patterns
    * Large change sets may affect performance but maintain functional correctness
    * Component requires check-paths action availability for core functionality

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`.github/actions/shared/check-paths`](../check-paths/README.md) - Core path change detection functionality
    * [`.github/actions/shared`](../README.md) - Shared actions infrastructure and common patterns
* **External Dependencies:**
    * `git` - Version control operations for change detection and base reference resolution
    * `jq` - JSON processing for structured output generation and format validation
    * GitHub Actions environment - Context variables and runner infrastructure
* **Dependents (Impact of Changes):**
    * **Epic #181 Workflows** - All specialized workflows (coverage-build.yml, future AI analysis workflows)
    * **Issue #183 Coverage Implementation** - coverage-build.yml workflow requires path-analysis outputs
    * **Issue #212 build.yml Refactor** - Main build workflow modernization using this component
    * **Future Specialized Workflows** - AI framework workflows and epic coordination patterns

## 7. Rationale & Key Historical Context

* **Build.yml Logic Extraction:** Preserves exact base reference determination logic from build.yml lines 57-68 to maintain functional equivalence during Epic #181 modernization while enabling component reusability across specialized workflows.
* **Check-Paths Enhancement Strategy:** Built as enhancement layer over existing check-paths action rather than replacement to maintain stability and leverage proven change detection while adding structured outputs for Epic #181 requirements.
* **Epic Progression Support:** Designed specifically to support feature→epic→develop→main branch progression patterns with branch-aware base reference determination enabling proper change detection across the Git workflow.
* **Boolean Output Design:** Provides direct boolean outputs for workflow conditional execution to eliminate complex string parsing in workflow YAML while maintaining structured JSON for advanced analysis scenarios.

## 8. Known Issues & TODOs

* **Category Rules Enhancement:** Future implementation of category_rules input parameter for custom path categorization patterns beyond default backend/frontend/docs/config detection.
* **Performance Optimization:** Large repository change sets could benefit from parallel processing optimization, though current performance meets <5 second target.
* **Advanced Base Reference Logic:** Consider supporting custom base reference strategies for complex Git workflows beyond standard branch progression patterns.
* **Integration Testing:** Enhance testing coverage for edge cases including empty repositories, complex merge scenarios, and very large change sets.

---