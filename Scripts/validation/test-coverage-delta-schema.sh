#!/bin/bash
# Schema validation tests for coverage_delta.json

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
readonly SCHEMA_FILE="$ROOT_DIR/Docs/Templates/schemas/coverage_delta.schema.json"
readonly TEST_DATA_DIR="/tmp/coverage-delta-test"

# Test results
declare -i TOTAL_TESTS=0
declare -i PASSED_TESTS=0

log_info() {
    echo -e "${BLUE}[SCHEMA]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SCHEMA]${NC} $1"
}

log_error() {
    echo -e "${RED}[SCHEMA]${NC} $1"
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
    log_info "Setting up schema validation test environment"

    # Create test directory
    mkdir -p "$TEST_DATA_DIR"

    # Verify schema file exists
    if [[ ! -f "$SCHEMA_FILE" ]]; then
        log_error "Schema file not found: $SCHEMA_FILE"
        return 1
    fi

    # Check if we have Python and jsonschema for validation
    if ! command -v python3 >/dev/null 2>&1; then
        log_error "Python3 is required for schema validation"
        return 1
    fi

    # Try to install jsonschema if not available
    if ! python3 -c "import jsonschema" 2>/dev/null; then
        log_info "Installing jsonschema for validation..."
        pip3 install jsonschema 2>/dev/null || {
            log_error "Could not install jsonschema - falling back to basic JSON validation"
            return 0
        }
    fi

    return 0
}

create_valid_coverage_delta() {
    local filename="$1"
    local current_coverage="${2:-15.5}"
    local baseline_coverage="${3:-16.0}"
    local trend="${4:-decreased}"

    local delta
    delta=$(echo "$current_coverage - $baseline_coverage" | bc -l)

    cat > "$filename" << EOF
{
  "current_coverage": $current_coverage,
  "baseline_coverage": $baseline_coverage,
  "coverage_delta": $delta,
  "coverage_trend": "$trend",
  "base_ref": "develop",
  "base_sha": "abc123456789",
  "run_number": 123,
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "baseline_source": "threshold",
  "baseline_unavailable": true,
  "notes": "Test generated coverage delta for validation"
}
EOF
}

validate_json_against_schema() {
    local json_file="$1"
    local test_name="$2"

    # First check if it's valid JSON
    if ! jq empty "$json_file" 2>/dev/null; then
        record_test "$test_name (JSON validity)" "FAIL"
        return 1
    fi

    # If jsonschema is available, do full schema validation
    if python3 -c "import jsonschema" 2>/dev/null; then
        local validation_result
        validation_result=$(python3 -c "
import json
import jsonschema
import sys

try:
    with open('$SCHEMA_FILE', 'r') as schema_file:
        schema = json.load(schema_file)

    with open('$json_file', 'r') as data_file:
        data = json.load(data_file)

    jsonschema.validate(data, schema)
    print('VALID')
except jsonschema.ValidationError as e:
    print(f'INVALID: {e.message}')
    sys.exit(1)
except Exception as e:
    print(f'ERROR: {str(e)}')
    sys.exit(1)
" 2>&1)

        if [[ "$validation_result" == "VALID" ]]; then
            record_test "$test_name (schema validation)" "PASS"
            return 0
        else
            log_error "Schema validation failed: $validation_result"
            record_test "$test_name (schema validation)" "FAIL"
            return 1
        fi
    else
        # Fallback to basic validation
        record_test "$test_name (basic JSON validation)" "PASS"
        return 0
    fi
}

test_valid_coverage_delta_schemas() {
    log_info "Testing valid coverage delta schemas..."

    # Test 1: Coverage improved scenario
    local improved_file="$TEST_DATA_DIR/coverage_delta_improved.json"
    create_valid_coverage_delta "$improved_file" "17.5" "16.0" "improved"
    validate_json_against_schema "$improved_file" "Valid schema - coverage improved"

    # Test 2: Coverage decreased scenario
    local decreased_file="$TEST_DATA_DIR/coverage_delta_decreased.json"
    create_valid_coverage_delta "$decreased_file" "15.5" "16.0" "decreased"
    validate_json_against_schema "$decreased_file" "Valid schema - coverage decreased"

    # Test 3: Coverage stable scenario
    local stable_file="$TEST_DATA_DIR/coverage_delta_stable.json"
    create_valid_coverage_delta "$stable_file" "16.0" "16.0" "stable"
    validate_json_against_schema "$stable_file" "Valid schema - coverage stable"

    # Test 4: Explicit input baseline
    cat > "$TEST_DATA_DIR/coverage_delta_explicit.json" << EOF
{
  "current_coverage": 18.5,
  "baseline_coverage": 17.0,
  "coverage_delta": 1.5,
  "coverage_trend": "improved",
  "base_ref": "main",
  "base_sha": "def987654321",
  "run_number": 456,
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "baseline_source": "explicit_input",
  "baseline_unavailable": false,
  "notes": "Manual baseline input for testing"
}
EOF
    validate_json_against_schema "$TEST_DATA_DIR/coverage_delta_explicit.json" "Valid schema - explicit baseline"
}

test_invalid_coverage_delta_schemas() {
    log_info "Testing invalid coverage delta schemas..."

    # Test 1: Missing required field
    cat > "$TEST_DATA_DIR/coverage_delta_missing_field.json" << EOF
{
  "current_coverage": 15.5,
  "baseline_coverage": 16.0,
  "coverage_trend": "decreased",
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")"
}
EOF

    if validate_json_against_schema "$TEST_DATA_DIR/coverage_delta_missing_field.json" "Invalid schema - missing required field" 2>/dev/null; then
        record_test "Invalid schema detection - missing required field" "FAIL"
    else
        record_test "Invalid schema detection - missing required field" "PASS"
    fi

    # Test 2: Invalid enum value
    cat > "$TEST_DATA_DIR/coverage_delta_invalid_enum.json" << EOF
{
  "current_coverage": 15.5,
  "baseline_coverage": 16.0,
  "coverage_delta": -0.5,
  "coverage_trend": "invalid_trend",
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")"
}
EOF

    if validate_json_against_schema "$TEST_DATA_DIR/coverage_delta_invalid_enum.json" "Invalid schema - invalid enum value" 2>/dev/null; then
        record_test "Invalid schema detection - invalid enum value" "FAIL"
    else
        record_test "Invalid schema detection - invalid enum value" "PASS"
    fi

    # Test 3: Invalid coverage percentage (out of range)
    cat > "$TEST_DATA_DIR/coverage_delta_invalid_range.json" << EOF
{
  "current_coverage": 150.0,
  "baseline_coverage": 16.0,
  "coverage_delta": 134.0,
  "coverage_trend": "improved",
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")"
}
EOF

    if validate_json_against_schema "$TEST_DATA_DIR/coverage_delta_invalid_range.json" "Invalid schema - coverage out of range" 2>/dev/null; then
        record_test "Invalid schema detection - coverage out of range" "FAIL"
    else
        record_test "Invalid schema detection - coverage out of range" "PASS"
    fi
}

test_schema_field_validation() {
    log_info "Testing individual field validation..."

    # Test timestamp format
    local timestamp_test="$TEST_DATA_DIR/coverage_delta_timestamp.json"
    create_valid_coverage_delta "$timestamp_test" "16.0" "15.5" "improved"

    # Verify timestamp is ISO 8601 format
    local timestamp
    timestamp=$(jq -r '.timestamp' "$timestamp_test")
    if [[ "$timestamp" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}Z$ ]]; then
        record_test "Timestamp format (ISO 8601)" "PASS"
    else
        record_test "Timestamp format (ISO 8601)" "FAIL"
    fi

    # Test baseline_source enum values
    local valid_sources=("threshold" "explicit_input" "baseline_file" "base_branch_measurement" "unknown")
    for source in "${valid_sources[@]}"; do
        local source_test="$TEST_DATA_DIR/coverage_delta_source_${source}.json"
        create_valid_coverage_delta "$source_test" "16.0" "15.5" "improved"

        # Update baseline_source
        jq --arg source "$source" '.baseline_source = $source' "$source_test" > "${source_test}.tmp" && mv "${source_test}.tmp" "$source_test"

        validate_json_against_schema "$source_test" "Baseline source enum - $source"
    done

    # Test coverage_trend enum values
    local valid_trends=("improved" "decreased" "stable")
    for trend in "${valid_trends[@]}"; do
        local trend_test="$TEST_DATA_DIR/coverage_delta_trend_${trend}.json"
        create_valid_coverage_delta "$trend_test" "16.0" "15.5" "$trend"

        validate_json_against_schema "$trend_test" "Coverage trend enum - $trend"
    done
}

cleanup_test_environment() {
    log_info "Cleaning up schema validation test environment"
    rm -rf "$TEST_DATA_DIR"
}

main() {
    log_info "🧪 Starting Coverage Delta Schema Validation Tests"

    if ! setup_test_environment; then
        log_error "Failed to set up test environment"
        return 1
    fi

    test_valid_coverage_delta_schemas
    test_invalid_coverage_delta_schemas
    test_schema_field_validation

    cleanup_test_environment

    # Report results
    log_info "Schema validation tests completed: $PASSED_TESTS/$TOTAL_TESTS passed"

    if [[ $PASSED_TESTS -eq $TOTAL_TESTS ]]; then
        log_success "All schema validation tests passed"
        return 0
    else
        log_error "Some schema validation tests failed"
        return 1
    fi
}

# Run tests
main "$@"