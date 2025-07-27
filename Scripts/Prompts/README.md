# Module/Directory: Scripts/Prompts

**Last Updated:** 2025-07-27

**Parent:** [`Scripts`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory contains Claude AI analysis prompts extracted from GitHub Actions to provide consistent, maintainable, and version-controlled AI-powered analysis across the CI/CD pipeline.
* **Key Responsibilities:** 
    * Security analysis prompts for comprehensive vulnerability assessment
    * Tech debt analysis prompts for code quality evaluation
    * Standards compliance checking prompts for coding standard validation
    * Test coverage analysis prompts for quality gate enforcement
    * PR analysis prompts for automated code review assistance
* **Why it exists:** To centralize and version control AI prompts, enabling consistent analysis results, easier prompt evolution, and maintainable AI integration in CI/CD processes. This separation allows for prompt testing and refinement without modifying workflow files.

## 2. Architecture & Key Concepts

* **High-Level Design:** Prompts are organized by analysis type with shared patterns:
    * **Security Prompts:** `security-analysis.md` - Comprehensive security vulnerability analysis
    * **Quality Prompts:** `tech-debt-analysis.md`, `standards-compliance.md` - Code quality and standards evaluation
    * **Test Prompts:** `test-coverage-analysis.md` - Test coverage and quality assessment
    * **Review Prompts:** `pr-analysis.md` - Pull request automated review
* **Core Integration Pattern:** Prompts follow a consistent structure:
    1. Context setting and role definition for Claude AI
    2. Input data specification and format requirements
    3. Analysis methodology and evaluation criteria
    4. Output format specification with required sections
    5. Quality gates and decision criteria
* **Versioning Strategy:** Prompts include version headers and change tracking to maintain analysis consistency
* **Output Standardization:** All prompts generate structured markdown reports with consistent sections and formatting

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for pipeline scripts):**
    * **Security Analysis** (`security-analysis.md`):
        * **Critical Preconditions:** `Security scan results in JSON format`, `Source code diff context`, `Previous vulnerability baseline`
        * **Critical Postconditions:** `Structured security report with severity levels`, `Quality gate decisions (DEPLOY/BLOCK)`, `Auto-issue creation recommendations`
        * **Non-Obvious Error Handling:** Graceful degradation when scan results incomplete; provides partial analysis with clear limitations
    * **Tech Debt Analysis** (`tech-debt-analysis.md`):
        * **Critical Preconditions:** `Source code changes`, `Complexity metrics`, `Test coverage data`
        * **Critical Postconditions:** `Debt score calculation`, `Prioritized improvement recommendations`, `Architectural impact assessment`
        * **Non-Obvious Error Handling:** Handles missing metrics gracefully; focuses analysis on available data
    * **Standards Compliance** (`standards-compliance.md`):
        * **Critical Preconditions:** `Source code to analyze`, `Project coding standards reference`, `Previous compliance baseline`
        * **Critical Postconditions:** `Compliance violations categorized by severity`, `Remediation instructions with file/line references`, `Compliance score calculation`
        * **Non-Obvious Error Handling:** Validates standards against code patterns; reports unknown patterns for manual review
* **Critical Assumptions:**
    * **AI Service Availability:** Claude AI service accessible with valid authentication
    * **Input Data Quality:** Scan results and source code are properly formatted and complete
    * **Context Limitations:** Prompts work within Claude's context window limitations
    * **Consistency Requirements:** Same input data should produce consistent analysis results

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:**
    * All prompts use markdown format with YAML frontmatter for metadata
    * Claude AI context is set consistently across all prompts
    * Input/output specifications follow JSON schema patterns
    * Quality gate thresholds are parameterized for easy adjustment
* **File Organization:**
    * Prompts follow `{analysis-type}-{scope}.md` naming convention
    * Version history maintained in file headers
    * Related prompts cross-reference each other
    * Example inputs/outputs included for testing
* **Technology Choices:**
    * Markdown for human readability and version control
    * YAML frontmatter for structured metadata
    * JSON for data exchange with pipeline scripts
    * Template variables for dynamic content injection
* **Quality Standards:**
    * All prompts include clear role definitions for Claude AI
    * Input validation requirements specified
    * Output format strictly defined with examples
    * Error handling scenarios documented
* **Integration Notes:**
    * Prompts designed for Claude Code OAuth integration
    * Support both local testing and CI/CD execution
    * Compatible with artifact-based data passing

## 5. How to Work With This Code

* **Setup:**
    * No special setup required - prompts are text files
    * For testing: Install Claude CLI with OAuth token configuration
    * For development: Text editor with markdown syntax highlighting
    * For validation: Markdown linter and YAML parser
* **Testing:**
    * **Location:** Prompts can be tested with sample data using Claude CLI
    * **How to Run:** Use `claude --print "$(cat prompt-file.md)"` with sample data
    * **Testing Strategy:** Test prompts with various input scenarios including edge cases
* **Common Usage Patterns:**
    ```bash
    # Test security analysis prompt with sample data
    echo "Sample security scan data" | claude --print "$(cat Scripts/Prompts/security-analysis.md)"
    
    # Validate prompt format
    yamllint Scripts/Prompts/*.md
    markdownlint Scripts/Prompts/*.md
    
    # Version control integration
    git diff Scripts/Prompts/ # Review prompt changes
    ```
* **Common Pitfalls / Gotchas:**
    * Claude context limits require careful input size management
    * Prompt versioning important for consistent CI/CD results
    * Template variables must be properly substituted before use
    * Output format changes require pipeline script updates

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Scripts/Pipeline/`](./Pipeline/README.md) - Scripts that execute these prompts
    * [`.github/actions/`](../../.github/actions/) - GitHub Actions that reference these prompts
    * [`Docs/Standards/`](../../Docs/Standards/README.md) - Coding standards referenced in prompts
* **External Service Dependencies:**
    * `Claude AI API` - Core AI analysis service
    * `GitHub API` - For context gathering and issue creation
    * `Git` - For diff generation and change analysis
* **Dependents (Impact of Changes):**
    * [`Pipeline Scripts`](./Pipeline/README.md) - Analysis scripts depend on prompt consistency
    * [CI/CD Workflows](../../.github/workflows/) - Automated analysis depends on prompt reliability
    * Quality Gates - Deployment decisions rely on prompt-generated analysis

## 7. Rationale & Key Historical Context

* **Prompt Extraction:** Moving prompts from GitHub Actions to dedicated files enables version control, testing, and collaborative improvement of AI analysis quality
* **Structured Format:** Consistent prompt structure ensures reliable analysis results and makes prompt maintenance easier
* **Parameterization:** Template variables in prompts allow customization for different projects or analysis contexts
* **Documentation Integration:** Prompts reference project standards and documentation to ensure analysis aligns with project requirements
* **Quality Gate Integration:** Prompts are designed to generate actionable outputs that integrate directly with CI/CD quality gates

## 8. Known Issues & TODOs

* **Context Size Management:** Large inputs may exceed Claude's context limits and need chunking strategies
* **Prompt Versioning:** Need systematic approach to prompt version management and compatibility
* **Response Validation:** Output format validation could be automated to ensure prompt effectiveness
* **Collaborative Editing:** Multiple team members modifying prompts need conflict resolution strategies
* **Performance Optimization:** Some prompts could be optimized for faster analysis without losing quality

---