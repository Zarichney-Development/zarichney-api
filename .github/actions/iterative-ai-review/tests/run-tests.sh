#!/bin/bash
# Test Suite Runner for Iterative AI Review Action
# Comprehensive test execution with coverage analysis for Issue #185

set -euo pipefail

# Test configuration
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly ACTION_ROOT="$(dirname "$SCRIPT_DIR")"
readonly TEST_OUTPUT_DIR="${SCRIPT_DIR}/output"
readonly COVERAGE_THRESHOLD=90

# Color output for test results
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m' # No Color

# Test execution statistics
declare -i TESTS_RUN=0
declare -i TESTS_PASSED=0
declare -i TESTS_FAILED=0

#####################################
# Display usage information
#####################################
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Test Suite Runner for Iterative AI Review Action

OPTIONS:
    -u, --unit          Run only unit tests
    -i, --integration   Run only integration tests
    -c, --coverage      Generate coverage report
    -v, --verbose       Verbose output
    -h, --help          Show this help message

EXAMPLES:
    $0                  # Run all tests
    $0 -u -c           # Run unit tests with coverage
    $0 -i -v           # Run integration tests with verbose output
    $0 --coverage      # Run all tests with coverage report

EOF
}

#####################################
# Setup test environment
#####################################
setup_test_environment() {
    log_info "Setting up test environment..."

    # Create output directory
    mkdir -p "$TEST_OUTPUT_DIR"

    # Setup BATS if not available
    if ! command -v bats >/dev/null 2>&1; then
        log_warn "BATS testing framework not found. Installing..."
        if command -v npm >/dev/null 2>&1; then
            npm install -g bats
        else
            log_error "npm not found. Please install BATS manually: https://github.com/bats-core/bats-core"
            exit 1
        fi
    fi

    # Verify action components exist
    local -a required_files=(
        "$ACTION_ROOT/action.yml"
        "$ACTION_ROOT/src/main.js"
        "$ACTION_ROOT/src/github-api.js"
        "$ACTION_ROOT/src/comment-manager.js"
        "$ACTION_ROOT/src/iteration-tracker.js"
        "$ACTION_ROOT/src/todo-manager.js"
    )

    for file in "${required_files[@]}"; do
        if [[ ! -f "$file" ]]; then
            log_error "Required action file not found: $file"
            exit 1
        fi
    done

    log_success "Test environment setup complete"
}

#####################################
# Run unit tests for specific component
# Arguments:
#   $1 - Component name (main, github-api, etc.)
#####################################
run_unit_tests() {
    local component="$1"
    local test_file="${SCRIPT_DIR}/unit/${component}/test_${component//-/_}.bats"

    if [[ ! -f "$test_file" ]]; then
        log_warn "Unit test file not found: $test_file"
        return 0
    fi

    log_info "Running unit tests for $component..."

    if bats "$test_file" --formatter tap > "${TEST_OUTPUT_DIR}/${component}_unit_results.tap" 2>&1; then
        local test_count
        test_count=$(grep -c "^ok " "${TEST_OUTPUT_DIR}/${component}_unit_results.tap" || echo "0")
        TESTS_RUN=$((TESTS_RUN + test_count))
        TESTS_PASSED=$((TESTS_PASSED + test_count))
        log_success "Unit tests for $component: $test_count tests passed"
    else
        local failed_count
        failed_count=$(grep -c "^not ok " "${TEST_OUTPUT_DIR}/${component}_unit_results.tap" || echo "0")
        local passed_count
        passed_count=$(grep -c "^ok " "${TEST_OUTPUT_DIR}/${component}_unit_results.tap" || echo "0")
        TESTS_RUN=$((TESTS_RUN + failed_count + passed_count))
        TESTS_PASSED=$((TESTS_PASSED + passed_count))
        TESTS_FAILED=$((TESTS_FAILED + failed_count))
        log_error "Unit tests for $component: $failed_count tests failed, $passed_count tests passed"
        cat "${TEST_OUTPUT_DIR}/${component}_unit_results.tap"
    fi
}

#####################################
# Run all unit tests
#####################################
run_all_unit_tests() {
    log_info "=== Running Unit Tests ==="

    local -a components=(
        "main"
        "github-api"
        "comment-manager"
        "iteration-tracker"
        "todo-manager"
    )

    for component in "${components[@]}"; do
        run_unit_tests "$component"
    done
}

#####################################
# Run integration tests
#####################################
run_integration_tests() {
    log_info "=== Running Integration Tests ==="

    local integration_test_file="${SCRIPT_DIR}/integration/test_integration.bats"

    if [[ ! -f "$integration_test_file" ]]; then
        log_warn "Integration test file not found: $integration_test_file"
        return 0
    fi

    log_info "Running integration tests..."

    if bats "$integration_test_file" --formatter tap > "${TEST_OUTPUT_DIR}/integration_results.tap" 2>&1; then
        local test_count
        test_count=$(grep -c "^ok " "${TEST_OUTPUT_DIR}/integration_results.tap" || echo "0")
        TESTS_RUN=$((TESTS_RUN + test_count))
        TESTS_PASSED=$((TESTS_PASSED + test_count))
        log_success "Integration tests: $test_count tests passed"
    else
        local failed_count
        failed_count=$(grep -c "^not ok " "${TEST_OUTPUT_DIR}/integration_results.tap" || echo "0")
        local passed_count
        passed_count=$(grep -c "^ok " "${TEST_OUTPUT_DIR}/integration_results.tap" || echo "0")
        TESTS_RUN=$((TESTS_RUN + failed_count + passed_count))
        TESTS_PASSED=$((TESTS_PASSED + passed_count))
        TESTS_FAILED=$((TESTS_FAILED + failed_count))
        log_error "Integration tests: $failed_count tests failed, $passed_count tests passed"
        cat "${TEST_OUTPUT_DIR}/integration_results.tap"
    fi
}

#####################################
# Generate coverage report
#####################################
generate_coverage_report() {
    log_info "=== Generating Coverage Report ==="

    # Use bash coverage analysis for shell scripts
    if command -v bashcov >/dev/null 2>&1; then
        log_info "Running coverage analysis with bashcov..."
        bashcov bats "${SCRIPT_DIR}/unit"/**/*.bats "${SCRIPT_DIR}/integration"/*.bats 2>/dev/null || true
    else
        log_warn "bashcov not found. Using basic coverage analysis..."
    fi

    # Manual coverage analysis for shell functions
    local -a source_files=(
        "$ACTION_ROOT/src/main.js"
        "$ACTION_ROOT/src/github-api.js"
        "$ACTION_ROOT/src/comment-manager.js"
        "$ACTION_ROOT/src/iteration-tracker.js"
        "$ACTION_ROOT/src/todo-manager.js"
    )

    local total_functions=0
    local tested_functions=0

    for source_file in "${source_files[@]}"; do
        if [[ -f "$source_file" ]]; then
            local functions_in_file
            functions_in_file=$(grep -c "^[[:space:]]*[a-zA-Z_][a-zA-Z0-9_]*[[:space:]]*(" "$source_file" || echo "0")
            total_functions=$((total_functions + functions_in_file))

            # Check if functions are tested (basic heuristic)
            local component_name
            component_name=$(basename "$source_file" .js)
            local test_file="${SCRIPT_DIR}/unit/${component_name}/test_${component_name//-/_}.bats"

            if [[ -f "$test_file" ]]; then
                local tested_in_file
                tested_in_file=$(grep -c "test.*${component_name}" "$test_file" || echo "0")
                tested_functions=$((tested_functions + tested_in_file))
            fi
        fi
    done

    local coverage_percentage=0
    if [[ $total_functions -gt 0 ]]; then
        coverage_percentage=$((tested_functions * 100 / total_functions))
    fi

    log_info "Coverage Analysis:"
    log_info "  Total functions: $total_functions"
    log_info "  Tested functions: $tested_functions"
    log_info "  Coverage: ${coverage_percentage}%"

    if [[ $coverage_percentage -ge $COVERAGE_THRESHOLD ]]; then
        log_success "Coverage threshold met: ${coverage_percentage}% >= ${COVERAGE_THRESHOLD}%"
    else
        log_warn "Coverage below threshold: ${coverage_percentage}% < ${COVERAGE_THRESHOLD}%"
    fi

    # Save coverage report
    cat > "${TEST_OUTPUT_DIR}/coverage_report.txt" << EOF
Iterative AI Review Action - Coverage Report
Generated: $(date)

Total Functions: $total_functions
Tested Functions: $tested_functions
Coverage Percentage: ${coverage_percentage}%
Coverage Threshold: ${COVERAGE_THRESHOLD}%
Status: $([[ $coverage_percentage -ge $COVERAGE_THRESHOLD ]] && echo "PASS" || echo "FAIL")

Test Execution Summary:
- Tests Run: $TESTS_RUN
- Tests Passed: $TESTS_PASSED
- Tests Failed: $TESTS_FAILED
- Success Rate: $([[ $TESTS_RUN -gt 0 ]] && echo "scale=2; $TESTS_PASSED * 100 / $TESTS_RUN" | bc || echo "0")%
EOF

    log_info "Coverage report saved to: ${TEST_OUTPUT_DIR}/coverage_report.txt"
}

#####################################
# Display test results summary
#####################################
display_test_summary() {
    log_info "=== Test Execution Summary ==="
    log_info "Tests Run: $TESTS_RUN"
    log_info "Tests Passed: $TESTS_PASSED"
    log_info "Tests Failed: $TESTS_FAILED"

    if [[ $TESTS_FAILED -eq 0 ]]; then
        log_success "All tests passed! ✅"
        return 0
    else
        log_error "Some tests failed! ❌"
        return 1
    fi
}

#####################################
# Logging functions
#####################################
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

#####################################
# Main execution function
#####################################
main() {
    local run_unit=false
    local run_integration=false
    local generate_coverage=false
    local verbose=false

    # Default to running all tests if no specific type specified
    local run_all=true

    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            -u|--unit)
                run_unit=true
                run_all=false
                shift
                ;;
            -i|--integration)
                run_integration=true
                run_all=false
                shift
                ;;
            -c|--coverage)
                generate_coverage=true
                shift
                ;;
            -v|--verbose)
                verbose=true
                set -x
                shift
                ;;
            -h|--help)
                usage
                exit 0
                ;;
            *)
                log_error "Unknown option: $1"
                usage
                exit 1
                ;;
        esac
    done

    # Setup test environment
    setup_test_environment

    # Run tests based on options
    if [[ $run_all == true ]] || [[ $run_unit == true ]]; then
        run_all_unit_tests
    fi

    if [[ $run_all == true ]] || [[ $run_integration == true ]]; then
        run_integration_tests
    fi

    # Generate coverage report if requested
    if [[ $generate_coverage == true ]]; then
        generate_coverage_report
    fi

    # Display summary and return appropriate exit code
    display_test_summary
}

# Execute main function with all arguments
main "$@"