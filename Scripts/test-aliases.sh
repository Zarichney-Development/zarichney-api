#!/bin/bash
# Project-specific aliases for zarichney-api test automation
# Source this file or add to your shell configuration

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Main test suite aliases (unified script)
alias test-suite="$SCRIPT_DIR/run-test-suite.sh"
alias test-report="$SCRIPT_DIR/run-test-suite.sh report"
alias test-report-json="$SCRIPT_DIR/run-test-suite.sh report json"
alias test-report-summary="$SCRIPT_DIR/run-test-suite.sh report summary"
alias test-quick="$SCRIPT_DIR/run-test-suite.sh report summary"
alias test-automation="$SCRIPT_DIR/run-test-suite.sh automation"
alias test-both="$SCRIPT_DIR/run-test-suite.sh both"

# Development workflow aliases
alias test-with-coverage="$SCRIPT_DIR/run-test-suite.sh report markdown --performance"
alias test-baseline="$SCRIPT_DIR/run-test-suite.sh report markdown --save-baseline"
alias test-compare="$SCRIPT_DIR/run-test-suite.sh report markdown --compare"

# Claude automation integration
alias test-claude='claude-full "Use the /test-report command to run comprehensive test analysis"'
alias test-auto='claude --dangerously-skip-permissions --print "Run the test automation suite using /test-report and provide a summary of the results"'

# CI/CD ready commands
alias test-ci="$SCRIPT_DIR/run-test-suite.sh report json --threshold=25"
alias test-gate="$SCRIPT_DIR/run-test-suite.sh report console"

# Utility functions
test_help() {
    echo "🧪 Zarichney API Unified Test Suite Aliases"
    echo "==========================================="
    echo ""
    echo "Core Commands:"
    echo "  test-suite          - Unified test suite (default: report mode)"
    echo "  test-report         - AI-powered analysis with markdown report"  
    echo "  test-automation     - HTML coverage reports with browser"
    echo "  test-both           - Run both automation and report modes"
    echo ""
    echo "Quick Access:"
    echo "  test-quick          - Quick summary for daily use"
    echo "  test-report-json    - JSON output for automation"
    echo "  test-report-summary - Brief executive summary"
    echo ""
    echo "Advanced Options:"
    echo "  test-with-coverage  - Detailed performance analysis"
    echo "  test-baseline       - Save current results as baseline"
    echo "  test-compare        - Compare with previous results"
    echo ""
    echo "Claude Integration:"
    echo "  test-claude         - Run via Claude with AI analysis"
    echo "  test-auto           - Fully automated Claude execution"
    echo ""
    echo "CI/CD Integration:"
    echo "  test-ci             - JSON output with quality gates"
    echo "  test-gate           - Console output for pipelines"
    echo ""
    echo "Examples:"
    echo "  test-quick                    # Daily status check"
    echo "  test-automation               # HTML coverage + browser"
    echo "  test-both --unit-only         # Both modes, unit tests only"
    echo "  test-report --performance     # Deep analysis"
    echo "  test-claude                   # AI-powered insights"
    echo ""
}

# Load message
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    echo "🧪 Test automation aliases available. Run 'test_help' for usage guide."
else
    echo "🧪 Zarichney API test aliases loaded. Use 'test_help' for options."
fi