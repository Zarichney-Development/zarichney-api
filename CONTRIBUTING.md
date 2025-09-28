# Contributing to Zarichney API

**Version:** 1.0
**Last Updated:** 2025-09-07
**Parent:** [`README.md`](README.md)

---

## Overview: AI-Orchestrated Development Model

The Zarichney API project has evolved beyond traditional development workflows to embrace a **multi-agent orchestrated development model** as the primary contribution method. This revolutionary approach leverages AI specialization and coordination to deliver comprehensive, standards-compliant development work while maintaining the highest quality standards.

**Key Evolution:** We have transitioned from manual workflow execution to intelligent orchestration, where a **Strategic Codebase Manager** coordinates specialized AI agents to handle all aspects of development, testing, documentation, and quality assurance.

## Primary Contribution Method: AI Orchestration

### Activating Claude Code with CLAUDE.md

The definitive way to contribute to this project is through **AI orchestration** using Claude Code with the project's `CLAUDE.md` system prompt:

```bash
# Activate Claude Code with project context
claude --project zarichney-api --include CLAUDE.md

# The AI will automatically:
# 1. Load project context and standards
# 2. Analyze GitHub issues for work assignment
# 3. Coordinate specialized subagents
# 4. Execute comprehensive development workflows
# 5. Ensure quality gates and standards compliance
```

### The Multi-Agent Development Team

When Claude Code activates, you gain access to a complete development team:

#### **Strategic Coordination (You + ComplianceOfficer)**
- **Strategic Codebase Manager:** Overall orchestration, mission understanding, and integration oversight
- **ComplianceOfficer:** Pre-PR validation, dual verification, and standards compliance enforcement

#### **Core Development Agents**
- **CodeChanger:** Feature implementation, bug fixes, refactoring (language-agnostic)
- **TestEngineer:** Test coverage, quality assurance, framework enhancement
- **DocumentationMaintainer:** README updates, standards compliance, context-rich documentation

#### **Technology Specialists**
- **FrontendSpecialist:** Angular 19, TypeScript, NgRx, Material Design
- **BackendSpecialist:** .NET 8, C#, EF Core, ASP.NET Core
- **SecurityAuditor:** Security hardening, vulnerability assessment
- **WorkflowEngineer:** GitHub Actions, CI/CD automation, Coverage Excellence Merge Orchestrator
  - **Core Capabilities:** Workflow creation, deployment strategies, automation design
  - **Coverage Excellence Orchestrator:** Autonomous consolidation of multiple coverage PRs with AI conflict resolution
  - **Integration Patterns:** Coordinates with TestEngineer coverage automation and epic management workflows

#### **Analysis & Investigation**
- **BugInvestigator:** Root cause analysis, diagnostic reporting
- **ArchitecturalAnalyst:** Design decisions, system architecture

### Working Directory Communication System

The team operates through a sophisticated **working directory communication protocol** at `/working-dir/`:

- **Session State Tracking:** Real-time progress monitoring
- **Agent Artifacts:** Analysis reports, design decisions, implementation notes
- **Handoff Protocols:** Rich context transfer between specialized agents
- **Compliance Tracking:** Validation checklists and quality reports

### Enhanced Development Workflow

The AI orchestration model automatically handles:

1. **Mission Understanding:** Comprehensive GitHub issue analysis and requirement extraction
2. **Context Ingestion:** Loading relevant standards, module READMEs, and codebase knowledge
3. **Task Decomposition:** Breaking issues into specialized subtasks for appropriate agents
4. **Adaptive Delegation:** Intelligent assignment with comprehensive context packages
5. **Integration Oversight:** Ensuring coherent integration of all agent deliverables
6. **Pre-PR Validation:** Dual verification through ComplianceOfficer partnership
7. **Quality Assurance:** Comprehensive testing, documentation, and standards validation
8. **Final Assembly:** Professional commit, push, and PR creation with AI Sentinel integration

#### **Enhanced Multi-Agent Coordination with Orchestrator**

**Individual Agent Excellence → Orchestrator Consolidation:**
- Multiple TestEngineer instances create focused coverage improvements
- WorkflowEngineer Orchestrator consolidates overlapping work automatically
- ComplianceOfficer validates both individual PRs and consolidated results
- SecurityAuditor ensures all changes maintain security standards throughout consolidation

**Conflict Prevention & Resolution:**
- Agents coordinate through timestamp-based scope selection
- Orchestrator resolves framework conflicts through AI-powered analysis
- Recovery branches preserve work during complex conflict scenarios
- Quality gates maintain excellence throughout consolidation pipeline

## Alternative Contribution Methods

### Automated Coverage Enhancement

For continuous test coverage improvement without manual intervention:

#### **Scheduled Automation**
```bash
# Coverage epic automation runs 4 times daily via GitHub Actions
# Reference: Docs/Development/AutomatedCoverageEpicWorkflow.md

# Key characteristics:
# - Autonomous execution in GitHub Actions CI
# - Multiple agent coordination with conflict prevention
# - Target: Comprehensive backend coverage excellence
# - Success criteria: 100% pass rate on ~65 executable tests
```

#### **Advanced Automation Capabilities**

**Coverage Excellence Orchestration:**
The WorkflowEngineer provides sophisticated Coverage Excellence Merge Orchestrator capabilities:
- **Multi-PR Consolidation:** Automatically discovers and consolidates coverage PRs targeting epic branches
- **AI Conflict Resolution:** Uses specialized AI prompts for safe conflict resolution with strict production constraints
- **Quality Validation:** Comprehensive testing and standards compliance before consolidation
- **Batch Processing:** Handles 3-50 PRs per execution with staging branch safety protocols

**Orchestrator Integration with Coverage Automation:**
```bash
# Individual coverage PRs created by TestEngineer
gh workflow run "Coverage Excellence Automation" --field target_area="Services"

# Multiple PRs consolidated by WorkflowEngineer Orchestrator
gh workflow run "Coverage Excellence Merge Orchestrator" --field max_prs=8 --field dry_run=false
```

#### **Coverage Excellence Integration**
- **Epic Branch:** `epic/testing-coverage`
- **Execution Frequency:** Every 6 hours via cron schedule
- **Quality Gates:** 100% pass rate, 23 expected skipped tests
- **Framework Enhancement:** Continuous testing infrastructure improvements
 - **AI Execution Behavior:** During subscription refresh windows, scheduled runs classify AI failures as `skipped_quota_window` and the workflow remains successful (it will retry next interval). Manual runs fail on unexpected AI errors to keep the signal strong. To emulate scheduler semantics in a manual run, set the `scheduled_trigger=true` input.

### Remote Development via Claude Dispatch

For on-the-go development through web interface integration:

```bash
# Experimental workflow using GitHub Actions
# Reference: .github/workflows/claude-dispatch.yml

# Capabilities:
# - Web interface triggered development
# - Remote task execution
# - GitHub Actions integration
# - Suitable for quick fixes and minor enhancements
```

## Development Standards Integration

### Mandatory Standards Review

Before any contribution (AI or human), **MUST** review all relevant standards:

#### **Core Standards Documentation**
- **[`/Docs/Standards/CodingStandards.md`](Docs/Standards/CodingStandards.md):** Code quality, architecture, and implementation standards
- **[`/Docs/Standards/TaskManagementStandards.md`](Docs/Standards/TaskManagementStandards.md):** Git workflows, branching, and commit conventions
- **[`/Docs/Standards/TestingStandards.md`](Docs/Standards/TestingStandards.md):** Testing philosophy, frameworks, and quality requirements
- **[`/Docs/Standards/DocumentationStandards.md`](Docs/Standards/DocumentationStandards.md):** README maintenance and diagramming requirements
- **[`/Docs/Standards/DiagrammingStandards.md`](Docs/Standards/DiagrammingStandards.md):** Mermaid diagram conventions and architectural visualization

#### **Module-Specific Context**
Always review local README.md files for specific module context:
- **[`/Code/Zarichney.Server/README.md`](Code/Zarichney.Server/README.md):** Backend architecture and patterns
- **[`/Code/Zarichney.Server.Tests/README.md`](Code/Zarichney.Server.Tests/README.md):** Testing framework and strategies
- **[`/Code/Zarichney.Website/README.md`](Code/Zarichney.Website/README.md):** Frontend architecture and conventions

### Working Directory Protocols

The `/working-dir/` serves as the communication hub for all development work:

#### **Session State Management**
```bash
# Session tracking for multi-agent coordination
/working-dir/session-state.md        # Current development status
/working-dir/agent-artifacts/        # Analysis reports and design decisions
/working-dir/compliance-reports/     # Quality validation and standards checking
```

#### **Agent Communication Standards**
- **Context Package Delivery:** Comprehensive technical constraints and integration requirements
- **Standardized Reporting:** Status updates, team coordination, and AI Sentinel readiness
- **Handoff Protocols:** Rich context transfer ensuring seamless agent collaboration

## Consolidated Development Workflows

While AI orchestration is the primary method, understanding the consolidated workflow patterns provides valuable context for manual contributions or troubleshooting:

### Universal Development Pattern

All contribution methods follow this core pattern:

1. **Task Analysis:** GitHub issue understanding and requirements extraction
2. **Standards Integration:** Mandatory review of all relevant project standards
3. **Branch Management:** Feature/test branch creation with conventional naming
4. **Implementation:** Code changes following established patterns and constraints
5. **Quality Validation:** Comprehensive testing with unified test suite integration
6. **Documentation Updates:** README and Mermaid diagram maintenance as needed
7. **Conventional Commits:** Standardized commit messages with issue references
8. **Pull Request Creation:** GitHub CLI usage with comprehensive descriptions
9. **AI Sentinel Review:** Automated quality analysis by five specialized AI agents

### Workflow Specializations

#### **Standard Implementation Process**
- **Use Case:** Straightforward features, bug fixes, simple refactoring
- **Approach:** Direct implementation with comprehensive testing
- **Quality Gates:** Full test suite execution with AI-powered analysis
- **Tools:** Unified test suite (`/test-report`), coverage validation

#### **Test-Driven Development Process**
- **Use Case:** Complex features, ambiguous requirements, significant refactoring
- **Approach:** Plan-first methodology with upfront design validation
- **Unique Elements:** Implementation planning phase, failing tests verification, red-green-refactor cycle
- **Quality Gates:** Enhanced validation with performance analysis

#### **Coverage Enhancement Process (Enhanced with Orchestrator)**
- **Use Case:** Systematic test coverage improvement, quality assurance
- **Approach:** Analytics-driven gap identification and targeted test development
- **Unique Elements:** Coverage report analysis, testability assessment, iterative refinement
- **Quality Gates:** Measurable coverage improvement with framework enhancements

**3-Phase Coverage Excellence Pipeline:**

**Phase 1 - Individual Agent Execution:**
- TestEngineer creates focused coverage improvements in individual task branches
- AI agents work simultaneously on different coverage areas to prevent conflicts
- Each task produces comprehensive test implementations with framework enhancements

**Phase 2 - Orchestrator Consolidation (NEW):**
- WorkflowEngineer's Coverage Excellence Merge Orchestrator automatically consolidates multiple coverage PRs
- AI-powered conflict resolution handles test framework overlaps and integration
- Quality gates ensure consolidated changes maintain standards compliance

**Phase 3 - Epic Integration:**
- Product owners integrate consolidated epic progress into develop/main branches
- Continuous coverage excellence tracking toward comprehensive backend coverage advancement
- Continuous progression monitoring and velocity optimization

## Tools and Commands

### Unified Test Suite Integration

The project features a comprehensive testing system with multiple execution modes:

#### **Primary Test Commands**
```bash
# AI-powered comprehensive analysis (Primary method)
/test-report                    # Full analysis with recommendations
/test-report summary            # Quick executive summary
/test-report json               # Machine-readable output for CI/CD
/test-report --performance      # Include performance analysis

# Script-based execution
./Scripts/run-test-suite.sh                    # Default: report mode
./Scripts/run-test-suite.sh automation         # HTML coverage + browser
./Scripts/run-test-suite.sh report json        # JSON output
./Scripts/run-test-suite.sh both               # Both modes
```

#### **Coverage Analysis Tools**
```bash
# Traditional .NET testing (requires Docker Desktop)
dotnet test zarichney-api.sln
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"

# Coverage report generation
dotnet test --collect:"XPlat Code Coverage"
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" \
                 "-targetdir:coveragereport" "-reporttypes:Html"
```

### GitHub Integration Commands

#### **Standard GitHub Operations**
```bash
# Build and development
dotnet build zarichney-api.sln
dotnet run --project Code/Zarichney.Server

# Git workflow (examples)
git checkout -b feature/issue-123-description
git commit -m "feat: implement feature (#123)"
gh pr create --base develop --title "feat: description (#123)" \
             --body "Closes #123. Summary of changes"

# API client regeneration (when contracts change)
./Scripts/GenerateApiClient.ps1      # PowerShell
./Scripts/generate-api-client.sh     # Bash
```

#### **Coverage Excellence Orchestrator Usage**

**Testing Current PR Backlog:**
```bash
# Dry run with current coverage PRs
claude --project zarichney-api
# AI will analyze available PRs and recommend orchestrator execution

# Manual orchestrator execution
gh workflow run "Coverage Excellence Merge Orchestrator" \
  --field dry_run=true \
  --field max_prs=8
```

**Production Consolidation:**
```bash
# Consolidate multiple coverage improvements
gh workflow run "Coverage Excellence Merge Orchestrator" \
  --field dry_run=false \
  --field max_prs=8 \
  --field merge_strategy=merge
```

#### **Enhanced AI-Powered GitHub Operations**
```bash
# Comprehensive repository analysis
claude --dangerously-skip-permissions --print \
  "Use GitHub MCP to analyze zarichney-api repository status"

# AI-enhanced PR creation with analysis
claude --dangerously-skip-permissions --print \
  "Use GitHub MCP to create detailed PR description for recent commits"

# Issue-driven development planning
claude --dangerously-skip-permissions --print \
  "Use GitHub MCP to analyze issue #123 and create implementation plan"
```

## AI-Powered Code Review System

The project features an advanced **five-agent AI review system** that automatically analyzes all pull requests:

### The Five AI Sentinels

#### **Quality Analysis Agents**
- **DebtSentinel:** Technical debt analysis and sustainability assessment
- **StandardsGuardian:** Coding standards and architectural compliance verification
- **TestMaster:** Test coverage and quality analysis with recommendations
- **SecuritySentinel:** Security vulnerability and threat assessment

#### **Integration Orchestrator**
- **MergeOrchestrator:** Holistic PR analysis and final deployment decisions

### Automatic Review Activation
- **PR to `develop`:** Testing + Standards + Technical Debt analysis
- **PR to `main`:** Full analysis including comprehensive Security assessment
- **Quality Gates:** Critical findings can block deployment with remediation guidance
- **Educational Value:** Each analysis teaches sustainable development patterns

## Quality Gates and Requirements

### Non-Negotiable Quality Standards

#### **Testing Requirements**
- **100% Pass Rate:** All executable tests must pass consistently
- **Coverage Targets:** Backend coverage progression toward comprehensive excellence
- **Test Categories:** Proper unit/integration test categorization and execution
- **Framework Enhancement:** Continuous improvement of testing infrastructure

#### **Code Quality Standards**
- **Standards Compliance:** Adherence to all project coding standards
- **Documentation Maintenance:** README and diagram updates for all changes
- **Conventional Commits:** Standardized commit message formatting with issue references
- **Security Validation:** Security considerations integrated throughout development

#### **Integration Requirements**
- **Cross-Agent Coordination:** Seamless integration of multi-agent deliverables
- **Working Directory Usage:** Proper artifact creation and communication protocols
- **Pre-PR Validation:** ComplianceOfficer partnership for dual verification
- **AI Sentinel Readiness:** Preparation for automated quality review

### Benefits of AI Orchestration (Enhanced)

**Core Orchestration Benefits:**
- **Comprehensive Coverage:** All aspects of development handled systematically
- **Quality Consistency:** Standardized application of all project requirements
- **Efficiency Gains:** Parallel processing and specialized expertise application
- **Knowledge Preservation:** Complete context maintenance across all development activities

**Enhanced with Orchestrator Automation:**
- **Reduced Manual Overhead:** Automatic consolidation of multiple coverage improvements
- **Conflict Resolution Excellence:** AI-powered resolution of test framework overlaps
- **Coverage Progression Efficiency:** Systematic consolidation enables faster excellence achievement
- **Quality Assurance Integration:** All existing AI Sentinels apply to consolidated changes
- **Scalable Coverage Improvement:** Handle 3-50 simultaneous coverage PRs effectively

### Development Environment Requirements

#### **Local Setup Prerequisites**
- **.NET 8 SDK:** Latest version for backend development
- **Docker Desktop:** Required for integration tests and database containers
- **Node.js/npm:** For frontend development and tooling
- **GitHub CLI:** For enhanced GitHub integration and automation

#### **Repository Health Validation**
```bash
# Environment validation
git status                           # Clean working directory
dotnet --version                     # .NET 8 SDK installed
docker --version                     # Docker available
node --version && npm --version      # Node.js/npm available
gh --version                         # GitHub CLI configured

# Project health check
/test-report summary                 # Test suite baseline
dotnet build zarichney-api.sln      # Build verification
```

## Troubleshooting and Support

### Common Issues and Solutions

#### **Test Suite Issues**
```bash
# Test execution failures
sg docker -c "dotnet test zarichney-api.sln"  # Docker group membership
/test-report summary                          # Quick diagnostic

# Coverage analysis problems
./Scripts/run-test-suite.sh report json      # Alternative analysis
dotnet clean zarichney-api.sln && dotnet build  # Clean build
```

#### **Git Workflow Issues**
```bash
# Branch synchronization
git fetch origin
git checkout develop
git pull origin develop

# Merge conflict resolution
git status
git merge --abort                    # If needed
git checkout -b new-branch develop   # Start fresh
```

#### **GitHub Integration Issues**
```bash
# GitHub CLI authentication
gh auth status
gh auth refresh

# Repository access verification
gh repo view Zarichney-Development/zarichney-api
```

### Agent Orchestration Support

#### **Working Directory Issues**
```bash
# Session state verification
ls -la /working-dir/
cat /working-dir/session-state.md   # Current status

# Clean working directory setup
mkdir -p /working-dir/agent-artifacts
mkdir -p /working-dir/compliance-reports
```

#### **Agent Communication Problems**
- **Context Package Issues:** Verify all standards documentation is accessible
- **Integration Failures:** Check working directory artifact creation
- **Quality Gate Failures:** Review ComplianceOfficer reports for specific issues

### Support Resources

#### **Documentation References**
- **Agent Instructions:** [`/.claude/agents/`](.claude/agents/) - Detailed agent specializations
- **Development Workflows:** [`/Docs/Development/`](Docs/Development/) - Legacy workflow patterns
- **Standards Documentation:** [`/Docs/Standards/`](Docs/Standards/) - All project standards
- **GitHub Workflows:** [`/.github/workflows/`](.github/workflows/) - CI/CD automation

#### **Contact and Assistance**
- **GitHub Issues:** Create issues for bugs, feature requests, or workflow problems
- **AI Code Review:** The five AI Sentinels provide detailed analysis and guidance
- **Documentation Updates:** All documentation questions should reference relevant README files

---

## Legacy Workflow Reference

**Historical Note:** This project previously operated under three distinct manual workflows that have been consolidated into the AI orchestration model:

- **StandardWorkflow.md:** 12-step standard development process
- **ComplexTaskWorkflow.md:** 15-step TDD/Plan-First methodology
- **TestCoverageWorkflow.md:** 10-step coverage enhancement process

These workflows have been analyzed, their unique value propositions extracted, and their patterns integrated into the comprehensive AI orchestration approach documented above. The legacy files remain available for historical reference and manual fallback scenarios but are deprecated in favor of the multi-agent orchestration model.

**Evolution Rationale:** The transition from manual workflows to AI orchestration provides:
- **Comprehensive Coverage:** All aspects of development handled systematically
- **Quality Consistency:** Standardized application of all project requirements
- **Efficiency Gains:** Parallel processing and specialized expertise application
- **Knowledge Preservation:** Complete context maintenance across all development activities

For contributors requiring manual workflow patterns, reference the consolidated Universal Development Pattern and Workflow Specializations sections above, which preserve all essential elements from the legacy workflows while integrating them into the modern contribution framework.

---

**Document Ownership:** Development Team
**Primary Method:** Multi-Agent AI Orchestration via CLAUDE.md
**Quality Assurance:** Five AI Sentinels + ComplianceOfficer Partnership
**Success Metrics:** Comprehensive Backend Coverage Excellence + 100% Test Pass Rate + Comprehensive Standards Compliance
