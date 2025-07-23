#!/bin/bash
# Project-specific aliases for zarichney-api test automation
# Source this file or add to your shell configuration

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Main test report aliases
alias test-report="$SCRIPT_DIR/run-test-report.sh"
alias test-report-json="$SCRIPT_DIR/run-test-report.sh json"
alias test-report-summary="$SCRIPT_DIR/run-test-report.sh summary"
alias test-quick="$SCRIPT_DIR/run-test-report.sh summary"

# Development workflow aliases
alias test-with-coverage="$SCRIPT_DIR/run-test-report.sh markdown --performance"
alias test-baseline="$SCRIPT_DIR/run-test-report.sh markdown --save-baseline"
alias test-compare="$SCRIPT_DIR/run-test-report.sh markdown --compare"

# Claude automation integration
alias test-claude='claude-full "Use the /test-report command to run comprehensive test analysis"'
alias test-auto='claude --dangerously-skip-permissions --print "Run the test automation suite using /test-report and provide a summary of the results"'

# CI/CD ready commands
alias test-ci="$SCRIPT_DIR/run-test-report.sh json --threshold=25"
alias test-gate="$SCRIPT_DIR/run-test-report.sh console"

# Utility functions
test_help() {
    echo "🧪 Zarichney API Test Automation Aliases"
    echo "========================================"
    echo ""
    echo "Basic Commands:"
    echo "  test-report         - Full markdown report with analysis"  
    echo "  test-quick          - Quick summary for daily use"
    echo "  test-report-json    - JSON output for automation"
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