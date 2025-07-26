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

3. **AI-Powered Quality Analysis** ⭐ **NEW in Phase 2**
   - 🤖 **Claude AI Integration**: Intelligent analysis of test patterns and trends
   - 📈 **Quality Regression Detection**: Compare with historical baselines
   - 🎯 **Predictive Risk Assessment**: Deployment safety evaluation
   - 🚨 **Smart Gap Analysis**: AI-identified critical testing gaps
   - 🔍 **Root Cause Intelligence**: AI-powered failure pattern recognition

4. **Enhanced Reporting & Insights** ⭐ **ENHANCED in Phase 2**
   - 📝 **AI-Generated Reports**: Detailed markdown with expert recommendations
   - 🏆 Executive summary with AI-driven quality assessment
   - 📊 Advanced coverage analysis beyond simple percentages
   - 💡 **Contextual Recommendations**: AI suggestions based on code changes
   - 🔗 Specific file paths, test categories, and actionable next steps
   - 📈 **Trend Analysis**: Historical performance and quality tracking

5. **CI/CD Integration** ⭐ **NEW in Phase 2**
   - 🚀 **Automated PR Comments**: AI analysis posted to GitHub PRs
   - 🚪 **Enhanced Quality Gates**: Intelligent threshold enforcement
   - ⚡ **Deployment Decision Support**: Risk-based deployment recommendations
   - 📊 **Pipeline Analytics**: CI/CD performance optimization insights

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

# Compare with previous run (Quality Regression Detection)
/test-report --compare

# AI-powered deployment risk assessment
/test-report --risk-assessment

# Generate comprehensive analysis with trend data
/test-report --ai-insights
```

## Arguments:

- **format** (optional): Output format
  - `markdown` (default): Human-readable detailed report
  - `json`: Machine-readable output for automation
  - `summary`: Brief executive summary
  - `console`: Terminal-optimized output

- **options** (optional):
  - `--performance`: Include detailed performance analysis
  - `--compare`: Compare with previous test run results (Quality Regression Detection)
  - `--threshold=N`: Set coverage threshold (default: 24%)
  - `--save-baseline`: Save current results as baseline for future comparisons
  - `--risk-assessment`: Generate AI-powered deployment risk assessment ⭐ **NEW**
  - `--ai-insights`: Include comprehensive AI analysis with trend data ⭐ **NEW**
  - `--regression-check`: Enable automatic quality regression detection ⭐ **NEW**

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

### 🤖 **AI-Powered Insights** ⭐ **NEW in Phase 2**
- **Quality Regression Detection**: Automatic comparison with historical baselines
- **Predictive Risk Assessment**: AI-calculated deployment safety scores
- **Smart Recommendations**: Context-aware suggestions based on code changes
- **Trend Analysis**: Long-term quality and performance pattern recognition
- **Critical Gap Identification**: AI-identified high-impact areas needing tests
- **Deployment Decision Support**: Risk-based go/no-go recommendations

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