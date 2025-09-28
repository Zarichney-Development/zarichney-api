#!/bin/bash
# Issue #187 Coverage Delta Implementation Validation Suite
# Comprehensive validation for coverage delta functionality

set -euo pipefail

# Colors for output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m' # No Color

# Script directory
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly VALIDATION_DIR="${SCRIPT_DIR}/validation"
readonly ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

# Validation results
declare -a VALIDATION_RESULTS=()
declare -i TOTAL_TESTS=0
declare -i PASSED_TESTS=0

# Helper functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

record_test_result() {
    local test_name="$1"
    local result="$2"
    local details="${3:-}"

    TOTAL_TESTS=$((TOTAL_TESTS + 1))

    if [[ "$result" == "PASS" ]]; then
        PASSED_TESTS=$((PASSED_TESTS + 1))
        log_success "✅ $test_name"
        VALIDATION_RESULTS+=("✅ $test_name")
    else
        log_error "❌ $test_name"
        VALIDATION_RESULTS+=("❌ $test_name: $details")
    fi
}

check_prerequisites() {
    log_info "🔍 Checking validation prerequisites..."

    local missing_tools=()

    # Check for required tools
    if ! command -v jq >/dev/null 2>&1; then
        missing_tools+=("jq")
    fi

    if ! command -v bc >/dev/null 2>&1; then
        missing_tools+=("bc")
    fi

    if ! command -v python3 >/dev/null 2>&1; then
        missing_tools+=("python3")
    fi

    if [[ ${#missing_tools[@]} -gt 0 ]]; then
        log_error "Missing required tools: ${missing_tools[*]}"
        log_info "Please install missing tools before running validation"
        return 1
    fi

    # Check for schema file
    if [[ ! -f "$ROOT_DIR/Docs/Templates/schemas/coverage_delta.schema.json" ]]; then
        log_error "Coverage delta schema not found at expected location"
        return 1
    fi

    # Check for workflow file
    if [[ ! -f "$ROOT_DIR/.github/workflows/testing-coverage-build-review.yml" ]]; then
        log_error "Coverage build workflow not found"
        return 1
    fi

    log_success "Prerequisites check passed"
    return 0
}

run_validation_suite() {
    log_info "🚀 Starting Issue #187 Coverage Delta Validation Suite"
    echo "=================================================="

    # Run individual validation components
    log_info "📋 Running schema validation tests..."
    if "${VALIDATION_DIR}/test-coverage-delta-schema.sh"; then
        record_test_result "Schema Validation" "PASS"
    else
        record_test_result "Schema Validation" "FAIL" "Schema validation failed"
    fi

    log_info "🔄 Running workflow scenario tests..."
    if "${VALIDATION_DIR}/test-coverage-delta-workflow.sh"; then
        record_test_result "Workflow Scenarios" "PASS"
    else
        record_test_result "Workflow Scenarios" "FAIL" "Workflow scenario tests failed"
    fi

    log_info "🤖 Running AI integration tests..."
    if "${VALIDATION_DIR}/test-ai-integration.sh"; then
        record_test_result "AI Integration" "PASS"
    else
        record_test_result "AI Integration" "FAIL" "AI integration tests failed"
    fi

    log_info "💬 Running PR comment validation tests..."
    if "${VALIDATION_DIR}/test-pr-comment-validation.sh"; then
        record_test_result "PR Comment Enhancement" "PASS"
    else
        record_test_result "PR Comment Enhancement" "FAIL" "PR comment validation failed"
    fi

    log_info "🔗 Running end-to-end workflow tests..."
    if "${VALIDATION_DIR}/test-coverage-delta-e2e.sh"; then
        record_test_result "End-to-End Workflow" "PASS"
    else
        record_test_result "End-to-End Workflow" "FAIL" "End-to-end workflow tests failed"
    fi

    log_info "⚠️ Running edge case and error handling tests..."
    if "${VALIDATION_DIR}/test-edge-cases.sh"; then
        record_test_result "Edge Cases & Error Handling" "PASS"
    else
        record_test_result "Edge Cases & Error Handling" "FAIL" "Edge case tests failed"
    fi
}

generate_validation_report() {
    echo ""
    echo "=================================================="
    log_info "📊 Issue #187 Validation Summary Report"
    echo "=================================================="

    # Overall results
    local pass_rate=$((PASSED_TESTS * 100 / TOTAL_TESTS))
    echo "Total Tests: $TOTAL_TESTS"
    echo "Passed: $PASSED_TESTS"
    echo "Failed: $((TOTAL_TESTS - PASSED_TESTS))"
    echo "Pass Rate: ${pass_rate}%"
    echo ""

    # Detailed results
    echo "Detailed Results:"
    for result in "${VALIDATION_RESULTS[@]}"; do
        echo "  $result"
    done
    echo ""

    # Final assessment
    if [[ $PASSED_TESTS -eq $TOTAL_TESTS ]]; then
        log_success "🎉 ALL VALIDATIONS PASSED - Issue #187 implementation is ready!"
        log_info "✅ Coverage delta functionality is working correctly"
        log_info "✅ AI integration is functional"
        log_info "✅ PR comment enhancements are operational"
        log_info "✅ Error handling is robust"
        echo ""
        log_info "🚀 Issue #187 is ready for Epic #181 Phase 1 completion"
        return 0
    else
        log_error "❌ VALIDATION FAILURES DETECTED"
        log_warning "Issue #187 implementation requires fixes before Epic #181 Phase 1 completion"
        echo ""
        log_info "Please review failed tests and address issues before proceeding"
        return 1
    fi
}

main() {
    # Change to root directory for consistent paths
    cd "$ROOT_DIR"

    # Check prerequisites
    if ! check_prerequisites; then
        exit 1
    fi

    # Run validation suite
    run_validation_suite

    # Generate report and exit with appropriate code
    if generate_validation_report; then
        exit 0
    else
        exit 1
    fi
}

# Handle script arguments
case "${1:-}" in
    --help|-h)
        echo "Usage: $0 [--help]"
        echo "Comprehensive validation suite for Issue #187 coverage delta implementation"
        echo ""
        echo "This script validates:"
        echo "  • Schema compliance for coverage_delta.json"
        echo "  • Workflow scenario handling"
        echo "  • AI integration functionality"
        echo "  • PR comment enhancements"
        echo "  • End-to-end workflow operation"
        echo "  • Edge cases and error handling"
        exit 0
        ;;
    "")
        main
        ;;
    *)
        log_error "Unknown argument: $1"
        echo "Use --help for usage information"
        exit 1
        ;;
esac