# Project Context & Operating Guide for AI Coding Assistant (Claude)

**Version:** 1.2
**Last Updated:** 2025-07-23

## 1. My Purpose & Your Role

* **My Purpose:** I am a set of standing instructions and key references for an AI Coding Assistant (like you, Claude) working on the `zarichney-api` repository. My goal is to help you understand project conventions, key commands, and essential documentation quickly.
* **Your Role:** When working on tasks in this repository, always consider the information here alongside any specific task prompt you receive. If the task prompt conflicts with a general standard mentioned here, the task prompt takes precedence for that specific instruction, but be mindful of project-wide standards.

## 2. Core Project Structure

* **`/Code/Zarichney.Server/`**: Main ASP.NET 8 application code. ([View README](../Code/Zarichney.Server/README.md))
* **`/Code/Zarichney.Server.Tests/`**: Unit and integration tests. ([View README](../Code/Zarichney.Server.Tests/README.md))
* **`/Code/Zarichney.Website/`**: Angular frontend application. ([View README](../Code/Zarichney.Website/README.md))
* **`/Docs/`**: All project documentation.
    * **`/Docs/Standards/`**: **CRITICAL STANDARDS** - Review these first. ([View README](../Docs/Standards/README.md))
    * **`/Docs/Development/`**: AI-assisted workflow definitions. ([View README](../Docs/Development/README.md))
    * **`/Docs/Templates/`**: Templates for prompts, issues, etc. ([View README](../Docs/Templates/README.md))
* **Module-Specific `README.md` files:** Each significant directory within `/Code/Zarichney.Server/` and `/Code/Zarichney.Server.Tests/` has its own `README.md`. **Always review the local `README.md` for the specific module you are working on.**

## 3. High-Level Development Workflow (When I give you a task)

Generally, your work will follow these phases. Refer to `/Docs/Standards/TaskManagementStandards.md` and the specific workflow file (e.g., `StandardWorkflow.md`) referenced in your task prompt for full details.

1.  **Understand Task:** Review the task prompt thoroughly, the related github issue and the project tree structure.
2.  **Review Context:** Use read tool on all standards and relevant local `README.md` files.
3.  **Branch:** Ensure you are on the correct branch, switch if needed or create a feature/test branch (`feature/issue-XXX-desc` or `test/issue-XXX-desc`), no committing on main.
4.  **Code/Test:** Implement changes and add/update tests. **Use `/test-report summary` for quick validation or `/test-report` for comprehensive analysis with AI insights.**
5.  **Format:** Verify and apply formatting (`dotnet format`).
6.  **Document:** Update relevant `README.md` files and diagrams if architecture/behavior changed.
7.  **Commit:** Use Conventional Commits referencing the Issue ID.
8.  **Pull Request:** Utilize the open PR or create a new PR using `gh pr create`.

## 4. Essential Commands & Tools

* **Build Project:**
    ```bash
    dotnet build Zarichney.sln
    ```
* **Run Project:**
    ```bash
    dotnet run --project Code/Zarichney.Server
    ```
* **🧪 Unified Test Suite:** (Comprehensive testing with multiple modes)
    ```bash
    # Unified script with mode selection
    ./Scripts/run-test-suite.sh                    # Default: report mode with markdown
    ./Scripts/run-test-suite.sh automation         # HTML coverage reports + browser
    ./Scripts/run-test-suite.sh report json        # JSON output for CI/CD
    ./Scripts/run-test-suite.sh both               # Run both modes
    
    # Claude custom command - Full AI-powered analysis
    /test-report                    # Detailed markdown report with recommendations
    /test-report summary            # Quick executive summary
    /test-report json               # Machine-readable output for CI/CD
    /test-report --performance      # Include performance analysis
    
    # Bash aliases (after sourcing Scripts/test-aliases.sh)
    test-report                     # Full analysis
    test-quick                      # Daily status check
    test-claude                     # AI-powered insights via Claude
    ```

* **Unified Test Suite Options:** (All-in-one testing solution)
    ```bash
    # Mode selection
    ./Scripts/run-test-suite.sh automation         # HTML reports, browser opening
    ./Scripts/run-test-suite.sh report             # AI analysis, quality gates
    ./Scripts/run-test-suite.sh both               # Execute both modes
    
    # Output format options (report mode)
    ./Scripts/run-test-suite.sh report markdown    # Detailed markdown report
    ./Scripts/run-test-suite.sh report json        # Machine-readable JSON
    ./Scripts/run-test-suite.sh report summary     # Quick executive summary
    ./Scripts/run-test-suite.sh report console     # Terminal-optimized output
    
    # Common options
    ./Scripts/run-test-suite.sh automation --no-browser     # Skip browser opening
    ./Scripts/run-test-suite.sh report --unit-only          # Unit tests only
    ./Scripts/run-test-suite.sh both --integration-only     # Integration tests only
    ./Scripts/run-test-suite.sh report --performance        # Include performance analysis
    ```

* **Run All Tests (Traditional):** (Ensure Docker Desktop is running for integration tests)
    ```bash
    # Standard execution (if Docker group membership is active)
    dotnet test zarichney-api.sln
    
    # For environments where Docker group membership isn't active in current shell
    sg docker -c "dotnet test zarichney-api.sln"
    ```
* **Run Specific Test Categories:**
    ```bash
    # Standard execution (if Docker group membership is active)
    dotnet test --filter "Category=Unit"
    dotnet test --filter "Category=Integration"
    
    # For environments where Docker group membership isn't active in current shell
    sg docker -c "dotnet test --filter 'Category=Unit'"
    sg docker -c "dotnet test --filter 'Category=Integration'"
    ```
* **Code Formatting:**
    * Check: `dotnet format --verify-no-changes --verbosity diagnostic`
    * Apply: `dotnet format`
* **Git Operations (Summary - See `TaskManagementStandards.md` for full details):**
    * Create branch: `git checkout -b [branch-name]` (e.g., `feature/issue-123-my-feature`)
    * Commit: `git commit -m "<type>: <description> (#ISSUE_ID)"`
    * Create PR: `gh pr create --base [target-branch] --title "<type>: <description> (#ISSUE_ID)" --body "Closes #ISSUE_ID. [Summary]"`
    * **Enhanced GitHub Operations** (See Section 7 for AI-powered alternatives):
        * Issue analysis: `claude --dangerously-skip-permissions --print "Use GitHub MCP to analyze issue #ID"`
        * PR enhancement: `claude --dangerously-skip-permissions --print "Use GitHub MCP to review and enhance my PR"`
        * Repository health: `claude --dangerously-skip-permissions --print "Use GitHub MCP to check zarichney-api status"`
* **Regenerate API Client (for `/Code/Zarichney.Server.Tests/`):** If API contracts change.
    ```powershell
    # PowerShell
    ./Scripts/GenerateApiClient.ps1
    
    # Bash
    ./Scripts/generate-api-client.sh
    ```

## 5. MUST ALWAYS CONSULT: Key Standards Documents

Before implementing any significant code, test, or documentation changes, you **MUST** be familiar with and adhere to the following standards. The task prompt will list specific documents, but these are foundational:

* **Primary Code Rules:** [`/Docs/Standards/CodingStandards.md`](../Docs/Standards/CodingStandards.md) (Includes `/.editorconfig` reference)
* **Task/Git Rules:** [`/Docs/Standards/TaskManagementStandards.md`](../Docs/Standards/TaskManagementStandards.md)
* **Testing Rules:** [`/Docs/Standards/TestingStandards.md`](../Docs/Standards/TestingStandards.md)
* **Documentation Rules (READMEs):** [`/Docs/Standards/DocumentationStandards.md`](../Docs/Standards/DocumentationStandards.md) (Uses [`/Docs/Templates/ReadmeTemplate.md`](../Docs/Templates/ReadmeTemplate.md))
* **Localized README.md:** Each module has its own `README.md` file. Always check the local `README.md` for specific instructions or context.

## 6. Important Reminders

* **Focus on the Given Task:** Address the specific objectives outlined in your current prompt and linked GitHub Issue.
* **Statelessness:** Assume you have no memory of prior interactions unless explicitly provided in the current prompt.
* **Clarity & Explicitness:** If instructions are unclear, state your interpretation or ask for clarification (if interacting with a human).
* **Adhere to Boundaries:** Respect any "what *not* to change" instructions in the prompt, you're at liberty if not specified.
* **Update Documentation:** Changes to code or tests that impact documented purpose, architecture, contracts, or diagrams **MUST** be accompanied by updates to the relevant `README.md` and diagrams.

## 7. GitHub Integration & Automation

### GitHub Repository Context
This project operates within the **Zarichney-Development** organization:
- **Repository**: `Zarichney-Development/zarichney-api`
- **Status**: Public repository
- **Current Issues**: 5 open issues requiring attention
- **Recent Focus**: Monorepo consolidation and CI/CD unification

### GitHub Connection Methods (Reference System Documentation)
For comprehensive GitHub setup information, reference the system-wide documentation:
- **System GitHub Setup**: `@/home/zarichney/CLAUDE.md` (GitHub Integration section)
- **Connection Hierarchy**: MCP → GitHub CLI → Direct API
- **Authentication Status**: All methods configured and working

### Project-Specific GitHub Operations

#### **Enhanced Pull Request Workflow**
Beyond the basic `gh pr create` command, leverage AI-powered GitHub operations:

```bash
# Standard PR creation (existing workflow step 8)
gh pr create --base main --title "feat: implement feature (#ISSUE_ID)" --body "Closes #ISSUE_ID. [Summary]"

# AI-enhanced PR creation with comprehensive analysis
claude --dangerously-skip-permissions --print "Use GitHub MCP to analyze my recent commits and create a detailed PR description for the zarichney-api repository"

# Automated PR review preparation
claude --dangerously-skip-permissions --print "Use GitHub MCP to review open PRs in zarichney-api and suggest improvements"
```

#### **Issue-Driven Development Integration**
Since workflow step 1 references "related github issue", enhance issue management:

```bash
# Standard issue analysis
gh issue view ISSUE_ID

# AI-powered issue analysis and task planning
claude --dangerously-skip-permissions --print "Use GitHub MCP to analyze issue #ISSUE_ID in zarichney-api and create an implementation plan"

# Cross-issue pattern analysis
claude --dangerously-skip-permissions --print "Use GitHub MCP to analyze all open issues in zarichney-api and identify common themes"
```

#### **Repository Health Monitoring**
Integrate GitHub monitoring into development workflow:

```bash
# Project status before starting work
claude --dangerously-skip-permissions --print "Use GitHub MCP to provide a status summary of zarichney-api including recent activity and open issues"

# Pre-development environment check
claude --dangerously-skip-permissions --print "Use GitHub MCP to check if there are any security alerts or critical updates needed for zarichney-api"
```

### Claude Self-Delegation for Project Tasks

#### **Development Workflow Enhancement**
Integrate GitHub AI operations into the standard workflow steps:

1. **Enhanced Task Understanding** (Step 1 expansion):
   ```bash
   # After reviewing task prompt and issue
   claude --dangerously-skip-permissions --print "Use GitHub MCP to provide context for issue #ISSUE_ID including related issues and recent commits in zarichney-api"
   ```

2. **Intelligent Branch Strategy** (Step 3 enhancement):
   ```bash
   # Before creating feature branch
   claude --dangerously-skip-permissions --print "Use GitHub MCP to suggest an appropriate branch naming strategy based on the current issue and repository conventions"
   ```

3. **Test Validation** (Step 4 enhancement):
   ```bash
   # After implementing changes, validate with automated test analysis
   claude --dangerously-skip-permissions --print "Run /test-report summary to validate my changes and ensure all tests pass"
   ```

4. **Pre-PR Validation** (Between steps 7-8):
   ```bash
   # Before creating PR, run comprehensive test analysis
   claude --dangerously-skip-permissions --print "Run /test-report and use GitHub MCP to review my changes and suggest an appropriate PR title and description for zarichney-api based on test results"
   ```

#### **Automated Test Suite Integration**
Streamline test execution and analysis with intelligent automation:

```bash
# Comprehensive test analysis after code changes
claude --dangerously-skip-permissions --print "Run /test-report to validate my changes and provide detailed analysis with recommendations"

# Quick validation during development
claude --dangerously-skip-permissions --print "Run /test-report summary to check test status and identify any immediate issues"

# Pre-deployment quality gate
claude --dangerously-skip-permissions --print "Run /test-report --performance and analyze results for production readiness"

# CI/CD integration validation
claude --dangerously-skip-permissions --print "Run /test-report json and validate that all quality gates pass for automated deployment"

# Test failure investigation
claude --dangerously-skip-permissions --print "Run /test-report and analyze any failing tests, providing root cause analysis and fix recommendations"
```

#### **Project-Specific Automation Patterns**
```bash
# Monorepo-specific analysis
claude --dangerously-skip-permissions --print "Use GitHub MCP to analyze the impact of my changes across both Code/Zarichney.Server and Code/Zarichney.Website"

# CI/CD pipeline awareness
claude --dangerously-skip-permissions --print "Use GitHub MCP to check the status of GitHub Actions workflows for zarichney-api and identify any failures"

# Documentation impact assessment
claude --dangerously-skip-permissions --print "Use GitHub MCP to identify which documentation files might need updates based on my code changes"

# Combined test and GitHub analysis
claude --dangerously-skip-permissions --print "Run /test-report and then use GitHub MCP to check if there are related issues or PRs that need attention based on test results"
```

### Integration with Existing Standards
- **TaskManagementStandards.md**: GitHub operations complement conventional commit and branching strategies
- **TestingStandards.md**: Use `/test-report` for comprehensive test analysis and GitHub MCP to suggest additional test scenarios
- **DocumentationStandards.md**: Leverage GitHub MCP to ensure documentation stays current with code changes
- **Test Automation**: The `/test-report` command integrates with all development workflows to provide intelligent test analysis, coverage validation, and quality gate enforcement

## 8. Automated Standards Compliance Check

### Overview
The project includes an automated Standards Compliance Check that runs on every pull request to ensure adherence to all project standards defined in `/Docs/Standards/`. This system provides immediate feedback to contributors and enforces quality gates.

### How It Works
- **Trigger**: Automatically runs after the main CI/CD workflow completes for pull requests
- **Scope**: Validates code formatting, Git standards, testing practices, and documentation
- **Output**: Detailed PR comment with categorized violations and remediation guidance

### Violation Categories
- **🚫 Mandatory**: Critical violations that block merging (formatting, missing tests, etc.)
- **⚠️ Recommended**: Important quality improvements that should be addressed
- **💡 Optional**: Suggestions for code quality enhancement and best practices

### Standards Checked
1. **Code Formatting & Style**: `.editorconfig` compliance, modern C# features, logging patterns
2. **Git & Task Management**: Conventional commits, branch naming, issue references
3. **Testing Standards**: Test naming, categorization, framework usage, coverage
4. **Documentation**: README.md coverage, XML docs, linking structure

### Quality Gates
- PRs with mandatory violations are automatically blocked from merging
- Compliance score calculated based on violation severity
- Clear remediation instructions provided for each violation type

### Integration with Development Workflow
- Complements existing test reporter and AI-powered analysis
- Uses same infrastructure pattern as other automated checks
- Provides actionable feedback linked to specific standards documentation

### For Developers
- Address mandatory violations to unblock PR merging
- Review recommended improvements for enhanced code quality
- Use provided links to standards documentation for detailed guidance
- Run `dotnet format` locally to fix most formatting violations

**Workflow File**: [`.github/workflows/standards-compliance-check.yml`](.github/workflows/standards-compliance-check.yml)

## 9. Automated Tech Debt Analysis

### Overview
The project includes an AI-powered tech debt analysis system that automatically evaluates pull requests for technical debt across multiple dimensions and provides actionable recommendations for both immediate fixes and future improvements.

### How It Works
- **Trigger**: Automatically runs after the main CI/CD workflow completes for pull requests
- **AI Analysis**: Uses Claude AI to provide expert-level technical debt assessment
- **Multi-Dimensional**: Analyzes complexity, performance, security, maintainability, and documentation
- **Auto-Issue Creation**: Generates GitHub issues for significant tech debt items requiring future work

### Analysis Categories

#### **🔍 Code Complexity Assessment**
- Cyclomatic and cognitive complexity analysis
- Method length and class size violations  
- Nesting depth and parameter count evaluation
- SOLID principle adherence checking

#### **⚡ Performance Debt Analysis**
- Inefficient algorithms and patterns identification
- Resource leak detection (missing using statements)
- Database query optimization opportunities
- Memory allocation and async/await pattern analysis

#### **🛡️ Security & Quality Issues**
- Potential security vulnerabilities (SQL injection, XSS, hard-coded secrets)
- Error handling gaps and input validation missing
- Authentication/authorization pattern violations
- Logging and monitoring deficiencies

#### **📚 Documentation & Testing Debt**
- Missing XML documentation for public APIs
- Inadequate test coverage for new complexity
- TODO/FIXME/HACK comment accumulation
- README and documentation update requirements

### Tech Debt Scoring
- **Debt Score**: 0-100 (lower is better) based on weighted categories
- **Quality Gates**: Critical issues block merge, high/medium issues create follow-up work
- **Trend Tracking**: Historical debt progression monitoring
- **Impact Assessment**: Business and development velocity impact analysis

### Auto-Issue Creation
The system automatically creates GitHub issues for identified tech debt:

#### **🚨 Critical Issues** (Block Merge)
- Immediate security vulnerabilities
- Critical performance regressions
- Architecture violations breaking existing patterns

#### **⚠️ High Priority** (Current Sprint)
- Significant complexity increases
- Performance bottlenecks affecting user experience
- Maintainability risks requiring prompt attention

#### **💡 Medium Priority** (Next Sprint)
- Code duplication opportunities for refactoring
- Missing abstractions improving design
- Documentation gaps impacting maintainability

#### **📝 Low Priority** (Technical Roadmap)
- Minor optimizations and improvements
- Code style and consistency enhancements
- Future enhancement opportunities

### Configuration
Tech debt analysis behavior is controlled by [`.github/config/tech-debt-config.yml`](.github/config/tech-debt-config.yml):

#### **Thresholds**
```yaml
complexity:
  method_max_cyclomatic: 10    # Maximum method complexity
  class_max_lines: 500         # Maximum class size
  nesting_max_depth: 4         # Maximum nesting levels

performance:
  query_timeout_warn_ms: 30000 # Database query timeout warnings
  memory_allocation_warn_mb: 100 # Memory allocation warnings
```

#### **Issue Creation Rules**
```yaml
issue_creation:
  auto_create_threshold: "medium"    # Minimum severity for auto-issues
  max_issues_per_pr: 10             # Limit issues per analysis
  labels: ["tech-debt", "auto-generated"]
```

#### **Analysis Patterns**
- **Include Patterns**: `**/*.cs`, `**/*.ts`, `**/*.sql`
- **Exclude Patterns**: `**/bin/**`, `**/Migrations/**`, `**/*.g.cs`
- **Security Rules**: Hard-coded passwords, SQL injection risks, HTTP URLs
- **Performance Rules**: Blocking async calls, string concatenation, uninitialized collections

### Quality Gates
- **Critical Issues**: Block PR merge until resolved
- **High Priority**: Must be addressed within current sprint
- **Medium Priority**: Plan for next sprint or milestone  
- **Low Priority**: Include in technical roadmap

### Integration with Development Workflow
1. **Automatic Analysis**: Runs on every PR targeting main/develop branches
2. **AI-Powered Insights**: Claude AI provides expert-level architectural review
3. **Structured Reporting**: Detailed markdown reports with specific recommendations
4. **Issue Tracking**: Auto-generated GitHub issues with proper labeling and milestones
5. **Quality Gate Enforcement**: Prevents merging PRs with critical tech debt

### AI Analysis Capabilities
- **Expert-Level Assessment**: Senior software architect quality analysis
- **Context-Aware**: Understands business impact and development velocity effects
- **Actionable Recommendations**: Specific file paths, line numbers, and remediation steps
- **Pattern Recognition**: Identifies complex architectural debt beyond simple pattern matching
- **Impact Analysis**: Evaluates cumulative effect on system maintainability

### Usage Commands
```bash
# View tech debt analysis for specific PR
gh pr view <PR_NUMBER> --comments | grep -A 50 "Tech Debt Analysis"

# Check tech debt workflow status
gh run list --workflow="Tech Debt Analysis" --limit 5

# Review auto-generated tech debt issues
gh issue list --label="tech-debt,auto-generated" --state=open
```

### Benefits
- **Proactive Debt Management**: Identifies debt before it becomes critical
- **Informed Decision Making**: Clear severity levels and impact assessment
- **Automated Tracking**: GitHub issues ensure tech debt doesn't get forgotten
- **Quality Improvement**: Continuous improvement through measurable debt reduction
- **Team Education**: AI insights help developers learn best practices

**Workflow File**: [`.github/workflows/tech-debt-analysis.yml`](.github/workflows/tech-debt-analysis.yml)  
**Configuration**: [`.github/config/tech-debt-config.yml`](.github/config/tech-debt-config.yml)

## 10. Security: Comprehensive Analysis

### Overview
The project includes a comprehensive security analysis system that follows the established workflow pattern used by standards compliance and tech debt analysis. Security scanning is integrated into the main CI/CD pipeline, with AI-powered analysis performed by a separate workflow that posts consolidated security insights to PR comments.

### Architecture Pattern (Consistent with Project Standards)
1. **Security Scans in Build Workflow**: Security scanning jobs run alongside build/test in `01-build.yml`
2. **Security Analysis Workflow**: `03-security.yml` triggers on workflow completion
3. **AI-Powered Analysis**: Custom GitHub Action analyzes all security data with Claude
4. **Single PR Comment**: Consolidated security analysis posted to PR (like standards compliance)

### How It Works
- **Security Scans**: Run as parallel jobs in main CI/CD workflow alongside existing build/test jobs
- **Artifact Collection**: Security results uploaded as artifacts for analysis workflow
- **AI Analysis**: Triggered on `workflow_run` completion, downloads artifacts, runs Claude analysis
- **PR Integration**: Single comprehensive security comment posted to PR with deployment decision

### Security Scanning Jobs (Integrated in Main CI/CD)

#### **🔍 CodeQL Analysis** (`security_codeql_analysis`)
- Multi-language static analysis (C# and JavaScript) 
- Security-extended and security-and-quality query suites
- Uses `.github/codeql/codeql-config.yml` configuration
- Matrix strategy for parallel language analysis

#### **🔒 Dependency Security Scanning** (`security_dependency_scan`)
- .NET vulnerability scanning with `dotnet list package --vulnerable`
- Node.js vulnerability scanning with `npm audit`
- Comprehensive vulnerability categorization (Critical, High, Moderate, Low)
- Automated vulnerability counting and impact assessment

#### **📋 Security Policy Compliance** (`security_policy_compliance`)
- SECURITY.md file validation
- GitHub Actions workflow permission auditing
- Hard-coded secrets detection (basic patterns)
- HTTPS enforcement checking

#### **🕵️ Secrets Detection** (`security_secrets_detection`)
- TruffleHog OSS integration for comprehensive secret scanning
- Historical commit analysis with full git history
- Verified secrets detection with detailed reporting

### AI-Powered Security Analysis Workflow

#### **🤖 Security Analysis Workflow** (`security-analysis.yml`)
- **Trigger**: `workflow_run` after main CI/CD completion (consistent with project pattern)
- **Custom Action**: `.github/actions/analyze-security` consolidates all security data
- **Claude Integration**: Expert cybersecurity assessment using Claude Code Action
- **Comprehensive Analysis**:
  - Security posture evaluation (Excellent/Good/Fair/Poor/Critical)
  - Vulnerability impact analysis and prioritization
  - Policy compliance assessment and recommendations
  - Threat modeling and risk evaluation
  - Actionable remediation roadmap with priority ranking
  - Deployment security decision (DEPLOY/BLOCK/CONDITIONAL)

#### **🚨 Auto-Issue Creation** (`.github/actions/create-security-issues`)
- **Critical Vulnerabilities**: Creates urgent issues for immediate attention
- **Secrets Detection**: Creates issues for credential management
- **High Volume Dependencies**: Creates tracking issues for dependency updates

### Security Decision Matrix
- **Critical Issues**: Block deployment for secrets or critical vulnerabilities
- **High Risk**: Require security review for high-severity findings
- **Medium Risk**: Track and plan remediation for moderate issues
- **AI Validation**: Expert security assessment for all deployment decisions

### Quality Gates
- Automated deployment blocking for critical security issues
- AI-driven security gates for production deployments
- PR comment integration with detailed security analysis
- Security artifact generation for downstream consumption

### Performance & Architecture Benefits
- **Integrated Execution**: Security scans run alongside build/test jobs in main CI/CD
- **Parallel Processing**: All security jobs run simultaneously for optimal performance
- **Consistent Pattern**: Follows same architecture as standards compliance and tech debt analysis
- **Resource Efficiency**: No separate workflow scheduling - runs with every build

### Integration Points
- **Dependabot Integration**: Enhanced security labels (`security`, `vulnerability-fix`)
- **Main CI/CD Pipeline**: Security gates before deployment decisions
- **Standards Compliance**: Complements existing quality workflows
- **Test Reporter**: Security metrics in unified reporting

### For Developers
- **PR Comments**: Review comprehensive security analysis in PR comments (single consolidated comment)
- **Critical Issues**: Address critical security issues to unblock deployments  
- **Auto-Generated Issues**: Monitor and address security issues automatically created for significant findings
- **Consistent Experience**: Same workflow pattern as standards compliance and tech debt analysis

**Security Analysis Workflow**: [`.github/workflows/security-analysis.yml`](.github/workflows/security-analysis.yml)  
**Security Analysis Action**: [`.github/actions/analyze-security/action.yml`](.github/actions/analyze-security/action.yml)  
**Issue Creation Action**: [`.github/actions/create-security-issues/action.yml`](.github/actions/create-security-issues/action.yml)  
**CodeQL Configuration**: [`.github/codeql/codeql-config.yml`](.github/codeql/codeql-config.yml)

## 11. Consolidated Mega Build Workflow Architecture

### Current Architecture (Post-Consolidation)
As of 2025-07-28, all CI/CD functionality has been consolidated into a single comprehensive workflow for optimal Claude AI integration and performance.

#### **Active Workflow**
- **`01 • Build & Test`** (`01-build.yml`) - **Consolidated mega build pipeline**
  - Universal PR triggering with `branches: ['**']`
  - Branch-aware conditional logic for different analysis scenarios
  - All Claude AI analysis integrated (Testing, Standards, Tech Debt, Security)
  - Automatic concurrency control to prevent duplicate runs

#### **Deprecated Workflows** (Maintained for Reference)
- **`02 • Quality Analysis [DEPRECATED]`** (`02-quality.yml`) - Functionality moved to 01-build.yml
- **`03 • Security Analysis [DEPRECATED]`** (`03-security.yml`) - Functionality moved to 01-build.yml

### Branch-Aware Execution Logic
- **Feature → Epic Branch PRs**: Build + Test only (no AI analysis)
- **Epic → Develop Branch PRs**: Build + Test + Quality Analysis (Testing, Standards, Tech Debt AI)
- **Any → Main Branch PRs**: Build + Test + Quality + Security Analysis (Full AI suite)

### Concurrency Control
The consolidated workflow includes automatic concurrency control to optimize resource usage:

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true
```

**Benefits:**
- **Resource Efficiency**: Automatically cancels previous runs when new commits are pushed
- **Faster Feedback**: Focuses CI resources on the latest changes
- **Handles Edge Cases**: Gracefully manages rapid commits, rebases, or force pushes
- **Clean History**: Prevents cluttered workflow run logs

**Behavior:**
- When a new commit is pushed to a PR, any running workflow for that PR is automatically cancelled
- Only the latest workflow run continues, providing feedback on the most recent changes
- Applies per-branch/PR, so workflows on different branches run independently

### Architecture Benefits
- **Claude AI Compatibility**: Single workflow enables proper Claude AI integration via `pull_request` events
- **Sequential Dependencies**: Proper build gates with `needs:` declarations ensure quality control
- **Performance Optimized**: Parallel execution where safe, sequential where dependencies exist
- **Comprehensive Coverage**: All analysis types (quality, security, testing) in one pipeline
- **Developer Experience**: Single workflow to monitor with consolidated feedback

### For Developers
- **Single Workflow**: Monitor one comprehensive pipeline instead of multiple separate workflows
- **Automatic Cancellation**: No need to manually cancel old runs - happens automatically
- **Real Claude AI**: All AI analysis uses genuine Claude insights, no fake fallback content
- **Branch-Appropriate**: Different analysis depths based on PR target (feature/epic/develop/main)

---
# important-instruction-reminders
Do what has been asked; nothing more, nothing less.
NEVER create files unless they're absolutely necessary for achieving your goal.
ALWAYS prefer editing an existing file to creating a new one.
NEVER proactively create documentation files (*.md) or README files. Only create documentation files if explicitly requested by the User.