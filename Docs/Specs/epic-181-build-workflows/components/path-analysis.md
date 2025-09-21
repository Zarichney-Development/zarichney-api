# Component: Path Analysis

**Last Updated:** 2025-09-21
**Component Status:** Planning
**Feature Context:** [Feature: Coverage-Build Workflow](../README.md)
**Epic Context:** [Epic #181: Standardize Build Workflows](../README.md)

> **Parent:** [`Epic #181 Components`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** GitHub Actions component providing intelligent path-based change detection and categorization for workflow optimization, enabling coverage-focused builds and selective execution patterns.

* **Key Technical Responsibilities:**
  - Analyze git changes between base reference and current commit to determine affected file categories
  - Provide boolean outputs for backend, frontend, documentation, and configuration changes
  - Generate detailed change summary for workflow decision-making and optimization
  - Integrate with existing check-paths shared action for consistent change detection patterns

* **Implementation Success Criteria:**
  - Path categorization produces deterministic, accurate results matching current build.yml logic
  - Integration with coverage-build.yml enables intelligent filtering for coverage-focused workflows
  - Performance improvement through selective execution based on change categories
  - Zero false negatives for change detection affecting build requirements

* **Why it exists:** Enables coverage-build.yml to execute selectively based on actual code changes, supporting Epic #181's goal of specialized workflows while maintaining the intelligent filtering currently embedded in the monolithic build.yml.

## 2. Architecture & Key Concepts

* **Technical Design:** Pure function component leveraging git diff analysis with deterministic path categorization logic, implemented as reusable GitHub Actions shared action with clear input/output contract.

* **Implementation Logic Flow:**
  1. Determine base reference branch for comparison (feature→epic→develop→main progression)
  2. Execute git diff to identify changed files between base and current commit
  3. Apply path categorization rules to classify changes by system area
  4. Generate boolean outputs for each category and detailed change summary
  5. Provide workflow-consumable outputs for selective execution decisions

* **Key Technical Elements:**
  - **Base Reference Determination**: Branch-aware logic supporting epic workflow patterns
  - **Path Categorization Engine**: Rule-based classification of file changes by system impact
  - **Change Summary Generation**: Detailed analysis for workflow optimization and debugging
  - **Integration Interface**: Clean contract with existing check-paths shared action

* **Data Structures:**
  - Input: Base reference, change threshold, categorization rules configuration
  - Output: Boolean change indicators, categorized file lists, change impact summary
  - Internal: Git diff parsing, path pattern matching, change classification logic

* **Processing Pipeline:** Git diff execution → Path pattern matching → Category classification → Summary generation → Output formatting

* **Component Architecture:**
  ```mermaid
  graph TD
      A[Workflow Trigger] --> B[Base Reference Determination];
      B --> C[Git Diff Execution];
      C --> D[Path Pattern Matching];
      D --> E{Category Classification};
      E -->|Backend| F[Backend Changes];
      E -->|Frontend| G[Frontend Changes];
      E -->|Docs| H[Documentation Changes];
      E -->|Config| I[Configuration Changes];
      F --> J[Change Summary Generation];
      G --> J;
      H --> J;
      I --> J;
      J --> K[Workflow Outputs];
  ```

## 3. Interface Contract & Assumptions

* **Key Technical Interfaces:**
  - **Primary Action Interface:**
    * **Purpose:** Provide path-based change detection for workflow optimization and selective execution
    * **Input Specifications:**
      - `base_ref` (string, optional): Base branch for comparison (defaults to branch-aware logic)
      - `change_threshold` (number, optional): Minimum file changes to trigger category (default: 1)
      - `category_rules` (string, optional): JSON configuration for path categorization patterns
    * **Output Specifications:**
      - `has_backend_changes` (boolean): True if backend code files modified
      - `has_frontend_changes` (boolean): True if frontend code files modified
      - `has_docs_changes` (boolean): True if documentation files modified
      - `has_config_changes` (boolean): True if configuration files modified
      - `change_summary` (string): Detailed JSON summary of all changes by category
      - `total_changes` (number): Total number of files changed
    * **Error Handling:** Graceful failure with empty outputs if git operations fail, detailed error logging for debugging
    * **Performance Characteristics:** <5 seconds execution time, minimal memory usage, supports repositories up to 100k files

* **Critical Technical Assumptions:**
  - **Platform Assumptions:** GitHub Actions environment with git CLI available, repository checkout completed before component execution
  - **Integration Assumptions:** check-paths shared action interface remains stable, git history available for diff operations
  - **Configuration Assumptions:** Repository uses standard branch patterns (feature/epic/develop/main), path categorization rules follow established patterns

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Implementation Patterns:**
  - Branch-aware base reference determination following epic workflow patterns
  - Path pattern matching using glob patterns for maximum flexibility and maintainability
  - JSON output formatting for structured data consumption by subsequent workflow steps
  - Error handling with graceful degradation and comprehensive logging

* **Technology Stack:**
  - GitHub Actions composite action with shell script implementation
  - Git CLI for change detection and diff analysis
  - jq for JSON processing and output formatting
  - Standard Unix utilities for path processing and pattern matching

* **Resource Requirements:**
  - Minimal memory usage (estimated <50MB)
  - Fast execution time target (<5 seconds for typical repositories)
  - No external network dependencies beyond GitHub Actions environment
  - Compatible with GitHub Actions concurrent execution patterns

## 5. How to Work With This Component

* **Development Environment:**
  - Local git repository with commit history for testing change detection logic
  - GitHub Actions environment or act tool for local action testing
  - jq installed for JSON processing and output validation

* **Testing Approach:**
  - **Unit Testing:** Test path categorization logic with known file sets and expected category outputs
  - **Integration Testing:** Validate integration with check-paths shared action and workflow consumption patterns
  - **Contract Testing:** Verify output format consistency and error handling across different repository states
  - **Performance Testing:** Validate execution time with large change sets and repository sizes

* **Debugging and Troubleshooting:**
  - Enable debug logging for git diff operations and path pattern matching
  - Validate base reference determination logic for epic branch scenarios
  - Test categorization rules with edge cases and complex path patterns
  - Monitor execution time and resource usage in GitHub Actions environment

## 6. Dependencies

* **Direct Technical Dependencies:**
  - [`check-paths`](/.github/actions/shared/check-paths/) - Existing shared action for consistent change detection patterns
  - Git CLI - Version control operations and diff analysis
  - jq - JSON processing for structured output formatting

* **External Dependencies:**
  - GitHub Actions runtime environment with standard Unix utilities
  - Repository git history and branch structure for diff operations
  - No external APIs or services required for component operation

* **Component Dependencies:**
  - Required by: coverage-build.yml, specialized workflows requiring change-based execution
  - Provides input to: backend-build, frontend-build, security-framework components
  - Integrates with: workflow-infrastructure for enhanced reporting and coordination

## 7. Rationale & Key Historical Context

* **Implementation Approach:** Extracted from monolithic build.yml lines 42-84 to enable reusable path analysis across multiple workflows, preserving exact logic while improving modularity and testability.

* **Technology Selection:** Composite GitHub Action chosen for maximum portability and integration with existing shared action patterns, shell script implementation for simplicity and maintainability.

* **Performance Optimization:** Minimal resource usage design enables concurrent execution across multiple workflows without GitHub Actions runner resource contention.

* **Security Considerations:** Read-only git operations with no external dependencies minimize attack surface, input validation prevents command injection through categorization rules.

## 8. Known Issues & TODOs

* **Technical Limitations:**
  - Path categorization rules currently static, future enhancement could support dynamic rule configuration
  - Git diff analysis limited to file-level changes, not line-level analysis for more granular categorization

* **Implementation Debt:**
  - Initial implementation focuses on boolean category outputs, could be enhanced with change impact scoring
  - Error handling could be enhanced with retry logic for transient git operation failures

* **Enhancement Opportunities:**
  - Integration with AI analysis for intelligent change impact assessment
  - Enhanced change summary with file-level impact analysis and dependency graph awareness
  - Support for custom categorization rules through repository configuration files

* **Monitoring and Observability:**
  - Execution time metrics for performance monitoring and optimization opportunities
  - Change pattern analysis for workflow optimization insights and pattern recognition
  - Integration with workflow-infrastructure component for comprehensive pipeline observability

---