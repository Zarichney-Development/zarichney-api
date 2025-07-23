---
description: "Run comprehensive test suite and generate intelligent test report"
argument-hint: "[format] [options]"
---

# Automated Test Suite Report Generator

Execute the complete test automation suite and generate a comprehensive, intelligent test report with coverage analysis, performance metrics, and actionable insights.

## What this command does:

1. **Environment Validation**
   - ✅ Check Docker service status (required for integration tests)
   - ✅ Verify project structure and dependencies
   - ✅ Validate test environment configuration

2. **Test Execution**
   - 🧪 Run full test suite: Unit + Integration + Smoke tests
   - 📊 Generate code coverage reports (line and branch coverage)
   - ⏱️ Capture performance metrics and test duration
   - 🔍 Collect detailed test results in TRX format

3. **Intelligent Analysis**
   - 📈 Parse test results and categorize outcomes
   - 🎯 Analyze code coverage with quality thresholds
   - 🚨 Identify and categorize skipped tests by dependency type
   - 🔍 Root cause analysis for any performance issues
   - 📋 Generate actionable recommendations

4. **Comprehensive Reporting**
   - 📝 Create detailed markdown report with metrics
   - 🏆 Provide executive summary with key findings
   - 📊 Include coverage breakdown by module/class
   - 💡 Suggest improvements and next steps
   - 🔗 Reference specific test files and line numbers

## Usage Examples:

```bash
# Basic execution with full report
/test-report

# Generate JSON output for CI/CD
/test-report json

# Quick summary only
/test-report summary

# Include performance analysis
/test-report --performance

# Compare with previous run
/test-report --compare
```

## Arguments:

- **format** (optional): Output format
  - `markdown` (default): Human-readable detailed report
  - `json`: Machine-readable output for automation
  - `summary`: Brief executive summary
  - `console`: Terminal-optimized output

- **options** (optional):
  - `--performance`: Include detailed performance analysis
  - `--compare`: Compare with previous test run results
  - `--threshold=N`: Set coverage threshold (default: 25%)
  - `--save-baseline`: Save current results as baseline for future comparisons

## Output Includes:

### 📊 **Test Statistics**
- Total tests executed, passed, failed, skipped
- Test execution time and performance metrics
- Pass rate percentage and trend analysis

### 📈 **Coverage Analysis** 
- Line coverage: X% (target: 25%+)
- Branch coverage: Y% (target: 20%+)
- Coverage by module with detailed breakdown
- Uncovered critical paths identification

### 🎯 **Quality Metrics**
- Test reliability score
- Infrastructure dependency health
- Environment-specific test behavior
- Performance regression detection

### 🔍 **Actionable Insights**
- Recommended tests to add for better coverage
- Skipped test analysis and remediation steps
- Performance optimization opportunities
- Technical debt identification

### 📋 **Next Steps**
- Priority improvements ranked by impact
- Configuration recommendations
- Integration opportunities with CI/CD

## Technical Implementation:

This command uses the project's established testing infrastructure:
- **Docker**: Integration test containers (PostgreSQL, etc.)
- **Coverage**: XPlat Code Coverage with Cobertura reports
- **Formats**: TRX test results + XML coverage data
- **Analysis**: Intelligent parsing and categorization
- **Standards**: Follows project TestingStandards.md guidelines

The command automatically handles Docker group permissions using `sg docker -c` when needed, ensuring reliable execution across different shell environments.

Perfect for:
- 🔄 **CI/CD Pipelines**: Automated quality gates
- 👥 **Team Reviews**: Comprehensive test health reports  
- 🚀 **Pre-deployment**: Release readiness validation
- 📈 **Sprint Reviews**: Testing progress tracking
- 🔍 **Debugging**: Test failure investigation

After execution, you'll have a complete understanding of your test suite health, coverage status, and clear next steps for improvement.