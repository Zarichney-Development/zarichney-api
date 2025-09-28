#!/bin/bash
# AI integration validation tests for coverage delta implementation

set -euo pipefail

# Colors for output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m'

# Paths
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly ROOT_DIR="$(cd "${SCRIPT_DIR}/../.." && pwd)"
readonly WORKFLOW_FILE="$ROOT_DIR/.github/workflows/testing-coverage-build-review.yml"
readonly AI_TESTING_ACTION="$ROOT_DIR/.github/actions/shared/ai-testing-analysis/action.yml"
readonly TEST_DATA_DIR="/tmp/ai-integration-test"

# Test results
declare -i TOTAL_TESTS=0
declare -i PASSED_TESTS=0

log_info() {
    echo -e "${BLUE}[AI-INTEGRATION]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[AI-INTEGRATION]${NC} $1"
}

log_error() {
    echo -e "${RED}[AI-INTEGRATION]${NC} $1"
}

record_test() {
    local test_name="$1"
    local result="$2"

    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    if [[ "$result" == "PASS" ]]; then
        PASSED_TESTS=$((PASSED_TESTS + 1))
        log_success "✅ $test_name"
    else
        log_error "❌ $test_name"
    fi
}

setup_test_environment() {
    log_info "Setting up AI integration test environment"

    # Create test directory
    mkdir -p "$TEST_DATA_DIR"

    # Verify required files exist
    if [[ ! -f "$WORKFLOW_FILE" ]]; then
        log_error "Workflow file not found: $WORKFLOW_FILE"
        return 1
    fi

    if [[ ! -f "$AI_TESTING_ACTION" ]]; then
        log_error "AI testing action not found: $AI_TESTING_ACTION"
        return 1
    fi

    return 0
}

create_mock_coverage_artifacts() {
    log_info "Creating mock coverage artifacts for AI integration testing..."

    # Create TestResults directory structure
    mkdir -p "$TEST_DATA_DIR/TestResults"

    # Create mock coverage_results.json
    cat > "$TEST_DATA_DIR/TestResults/coverage_results.json" << 'EOF'
{
  "summary": {
    "coveragePercentage": 16.5,
    "linesCovered": 165,
    "linesTotal": 1000,
    "branchesCovered": 45,
    "branchesTotal": 200
  },
  "modules": [
    {
      "name": "Zarichney.Server",
      "coverage": 16.5,
      "linesCovered": 165,
      "linesTotal": 1000
    }
  ]
}
EOF

    # Create mock parsed_results.json
    cat > "$TEST_DATA_DIR/TestResults/parsed_results.json" << 'EOF'
{
  "testResults": {
    "total": 85,
    "passed": 85,
    "failed": 0,
    "skipped": 23
  },
  "coverage": {
    "percentage": 16.5,
    "lines": {
      "covered": 165,
      "total": 1000
    },
    "branches": {
      "covered": 45,
      "total": 200
    }
  }
}
EOF

    # Create mock coverage_delta.json
    cat > "$TEST_DATA_DIR/TestResults/coverage_delta.json" << EOF
{
  "current_coverage": 16.5,
  "baseline_coverage": 16.0,
  "coverage_delta": 0.5,
  "coverage_trend": "improved",
  "base_ref": "develop",
  "base_sha": "abc123456789",
  "run_number": 123,
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "baseline_source": "threshold",
  "baseline_unavailable": true,
  "notes": "Mock coverage delta for AI integration testing"
}
EOF

    # Create mock health_trends.json
    cat > "$TEST_DATA_DIR/TestResults/health_trends.json" << 'EOF'
{
  "trends": {
    "coverage": {
      "current": 16.5,
      "previous": 16.0,
      "trend": "improving",
      "velocity": 0.5
    },
    "testHealth": {
      "passRate": 100.0,
      "trend": "stable"
    }
  }
}
EOF

    # Create mock CoverageReport directory
    mkdir -p "$TEST_DATA_DIR/CoverageReport"
    echo "<html><body>Mock Coverage Report</body></html>" > "$TEST_DATA_DIR/CoverageReport/index.html"
}

test_artifact_structure_validation() {
    log_info "Testing artifact structure validation..."

    create_mock_coverage_artifacts

    # Test coverage_results.json structure
    if [[ -f "$TEST_DATA_DIR/TestResults/coverage_results.json" ]] && jq empty "$TEST_DATA_DIR/TestResults/coverage_results.json" 2>/dev/null; then
        local coverage_percentage
        coverage_percentage=$(jq -r '.summary.coveragePercentage' "$TEST_DATA_DIR/TestResults/coverage_results.json" 2>/dev/null)
        if [[ "$coverage_percentage" =~ ^[0-9]+\.?[0-9]*$ ]]; then
            record_test "Coverage results artifact structure" "PASS"
        else
            record_test "Coverage results artifact structure" "FAIL"
        fi
    else
        record_test "Coverage results artifact structure" "FAIL"
    fi

    # Test coverage_delta.json structure
    if [[ -f "$TEST_DATA_DIR/TestResults/coverage_delta.json" ]] && jq empty "$TEST_DATA_DIR/TestResults/coverage_delta.json" 2>/dev/null; then
        local required_fields=("current_coverage" "baseline_coverage" "coverage_delta" "coverage_trend" "timestamp")
        local all_fields_present=true

        for field in "${required_fields[@]}"; do
            if ! jq -e ".$field" "$TEST_DATA_DIR/TestResults/coverage_delta.json" >/dev/null 2>&1; then
                all_fields_present=false
                break
            fi
        done

        if [[ "$all_fields_present" == "true" ]]; then
            record_test "Coverage delta artifact structure" "PASS"
        else
            record_test "Coverage delta artifact structure" "FAIL"
        fi
    else
        record_test "Coverage delta artifact structure" "FAIL"
    fi

    # Test health_trends.json structure
    if [[ -f "$TEST_DATA_DIR/TestResults/health_trends.json" ]] && jq empty "$TEST_DATA_DIR/TestResults/health_trends.json" 2>/dev/null; then
        record_test "Health trends artifact structure" "PASS"
    else
        record_test "Health trends artifact structure" "FAIL"
    fi
}

test_workflow_variable_mapping() {
    log_info "Testing workflow variable mapping for AI integration..."

    # Test that workflow defines variable mappings
    local workflow_content
    workflow_content=$(cat "$WORKFLOW_FILE")

    # Check for COVERAGE_DATA mapping
    if echo "$workflow_content" | grep -q "coverage_data_file.*coverage_results.json"; then
        record_test "COVERAGE_DATA variable mapping" "PASS"
    else
        record_test "COVERAGE_DATA variable mapping" "FAIL"
    fi

    # Check for COVERAGE_DELTA mapping
    if echo "$workflow_content" | grep -q "coverage_delta_file.*coverage_delta.json"; then
        record_test "COVERAGE_DELTA variable mapping" "PASS"
    else
        record_test "COVERAGE_DELTA variable mapping" "FAIL"
    fi

    # Check for COVERAGE_TRENDS mapping
    if echo "$workflow_content" | grep -q "coverage_trends_file.*health_trends.json"; then
        record_test "COVERAGE_TRENDS variable mapping" "PASS"
    else
        record_test "COVERAGE_TRENDS variable mapping" "FAIL"
    fi
}

test_ai_action_input_parameters() {
    log_info "Testing AI action input parameters..."

    # Check AI testing analysis action inputs
    if [[ -f "$AI_TESTING_ACTION" ]]; then
        local action_content
        action_content=$(cat "$AI_TESTING_ACTION")

        # Check for coverage data inputs
        if echo "$action_content" | grep -q "coverage_data:"; then
            record_test "AI action has coverage_data input" "PASS"
        else
            record_test "AI action has coverage_data input" "FAIL"
        fi

        # Check for baseline coverage input
        if echo "$action_content" | grep -q "baseline_coverage:"; then
            record_test "AI action has baseline_coverage input" "PASS"
        else
            record_test "AI action has baseline_coverage input" "FAIL"
        fi

        # Check for test results input
        if echo "$action_content" | grep -q "test_results:"; then
            record_test "AI action has test_results input" "PASS"
        else
            record_test "AI action has test_results input" "FAIL"
        fi
    else
        record_test "AI action file accessibility" "FAIL"
    fi
}

test_artifact_download_simulation() {
    log_info "Testing artifact download simulation..."

    create_mock_coverage_artifacts

    # Simulate artifact download step functionality
    local download_dir="$TEST_DATA_DIR/download_simulation"
    mkdir -p "$download_dir"

    # Copy mock artifacts to simulate download
    cp -r "$TEST_DATA_DIR/TestResults" "$download_dir/"
    cp -r "$TEST_DATA_DIR/CoverageReport" "$download_dir/"

    # Verify downloaded artifacts are accessible
    if [[ -f "$download_dir/TestResults/coverage_delta.json" ]] && \
       [[ -f "$download_dir/TestResults/coverage_results.json" ]] && \
       [[ -f "$download_dir/TestResults/health_trends.json" ]]; then
        record_test "Artifact download simulation" "PASS"
    else
        record_test "Artifact download simulation" "FAIL"
    fi

    # Test file path accessibility for AI actions
    local coverage_data_path="$download_dir/TestResults/coverage_results.json"
    local coverage_delta_path="$download_dir/TestResults/coverage_delta.json"
    local coverage_trends_path="$download_dir/TestResults/health_trends.json"

    if [[ -r "$coverage_data_path" && -r "$coverage_delta_path" && -r "$coverage_trends_path" ]]; then
        record_test "AI action file path accessibility" "PASS"
    else
        record_test "AI action file path accessibility" "FAIL"
    fi
}

test_workflow_ai_integration_chain() {
    log_info "Testing workflow AI integration chain..."

    # Verify workflow has AI coverage analysis job
    if grep -q "ai_coverage_analysis:" "$WORKFLOW_FILE"; then
        record_test "Workflow has AI coverage analysis job" "PASS"
    else
        record_test "Workflow has AI coverage analysis job" "FAIL"
    fi

    # Verify AI coverage analysis job depends on coverage_analysis
    if grep -A 5 "ai_coverage_analysis:" "$WORKFLOW_FILE" | grep -q "needs:.*coverage_analysis"; then
        record_test "AI coverage analysis depends on coverage analysis" "PASS"
    else
        record_test "AI coverage analysis depends on coverage analysis" "FAIL"
    fi

    # Verify AI coverage analysis downloads artifacts
    if grep -A 20 "ai_coverage_analysis:" "$WORKFLOW_FILE" | grep -q "download-artifact"; then
        record_test "AI coverage analysis downloads artifacts" "PASS"
    else
        record_test "AI coverage analysis downloads artifacts" "FAIL"
    fi

    # Verify AI testing analysis action is called
    if grep -q "ai-testing-analysis" "$WORKFLOW_FILE"; then
        record_test "AI testing analysis action is called" "PASS"
    else
        record_test "AI testing analysis action is called" "FAIL"
    fi
}

test_variable_substitution_simulation() {
    log_info "Testing variable substitution simulation..."

    create_mock_coverage_artifacts

    # Simulate variable substitution that would occur in workflow
    local coverage_data coverage_baseline coverage_delta

    # Extract values from mock artifacts
    coverage_data=$(jq -r '.summary.coveragePercentage' "$TEST_DATA_DIR/TestResults/coverage_results.json")
    coverage_baseline=$(jq -r '.baseline_coverage' "$TEST_DATA_DIR/TestResults/coverage_delta.json")
    coverage_delta=$(jq -r '.coverage_delta' "$TEST_DATA_DIR/TestResults/coverage_delta.json")

    # Verify values are accessible and numeric
    if [[ "$coverage_data" =~ ^[0-9]+\.?[0-9]*$ ]] && \
       [[ "$coverage_baseline" =~ ^[0-9]+\.?[0-9]*$ ]] && \
       [[ "$coverage_delta" =~ ^-?[0-9]+\.?[0-9]*$ ]]; then
        record_test "Variable extraction from artifacts" "PASS"
    else
        record_test "Variable extraction from artifacts" "FAIL"
    fi

    # Test file path variables
    local coverage_data_file="TestResults/coverage_results.json"
    local coverage_delta_file="TestResults/coverage_delta.json"
    local coverage_trends_file="TestResults/health_trends.json"

    if [[ -f "$TEST_DATA_DIR/$coverage_data_file" ]] && \
       [[ -f "$TEST_DATA_DIR/$coverage_delta_file" ]] && \
       [[ -f "$TEST_DATA_DIR/$coverage_trends_file" ]]; then
        record_test "File path variable accessibility" "PASS"
    else
        record_test "File path variable accessibility" "FAIL"
    fi
}

test_iterative_ai_review_integration() {
    log_info "Testing iterative AI review integration..."

    # Verify iterative AI review job exists
    if grep -q "iterative_ai_review:" "$WORKFLOW_FILE"; then
        record_test "Iterative AI review job exists" "PASS"
    else
        record_test "Iterative AI review job exists" "FAIL"
    fi

    # Verify iterative AI review downloads coverage artifacts
    if grep -A 30 "iterative_ai_review:" "$WORKFLOW_FILE" | grep -q "download-artifact"; then
        record_test "Iterative AI review downloads artifacts" "PASS"
    else
        record_test "Iterative AI review downloads artifacts" "FAIL"
    fi

    # Verify iterative AI review uses coverage data files
    if grep -A 50 "iterative_ai_review:" "$WORKFLOW_FILE" | grep -q "coverage_delta_file"; then
        record_test "Iterative AI review uses coverage delta file" "PASS"
    else
        record_test "Iterative AI review uses coverage delta file" "FAIL"
    fi
}

cleanup_test_environment() {
    log_info "Cleaning up AI integration test environment"
    rm -rf "$TEST_DATA_DIR"
}

main() {
    log_info "🤖 Starting AI Integration Validation Tests"

    if ! setup_test_environment; then
        log_error "Failed to set up test environment"
        return 1
    fi

    test_artifact_structure_validation
    test_workflow_variable_mapping
    test_ai_action_input_parameters
    test_artifact_download_simulation
    test_workflow_ai_integration_chain
    test_variable_substitution_simulation
    test_iterative_ai_review_integration

    cleanup_test_environment

    # Report results
    log_info "AI integration tests completed: $PASSED_TESTS/$TOTAL_TESTS passed"

    if [[ $PASSED_TESTS -eq $TOTAL_TESTS ]]; then
        log_success "All AI integration tests passed"
        return 0
    else
        log_error "Some AI integration tests failed"
        return 1
    fi
}

# Run tests
main "$@"