#!/bin/bash

# Backend Build and Test Script
# Extracted from .github/workflows/main.yml for maintainability
# Version: 1.0
# Last Updated: 2025-07-27

# Source common functions
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common-functions.sh"

# Script configuration
readonly SOLUTION_FILE="zarichney-api.sln"
readonly COVERAGE_THRESHOLD="${COVERAGE_THRESHOLD:-24}"

# Help function
show_help() {
    cat << EOF
Backend Build and Test Script

USAGE:
    $0 [OPTIONS]

OPTIONS:
    --config CONFIG     Build configuration (default: Release)
    --threshold NUM     Coverage threshold percentage (default: $COVERAGE_THRESHOLD)
    --parallel         Enable parallel test execution
    --max-parallel NUM  Maximum parallel collections (default: 4)
    --skip-tests       Skip test execution (build only)
    --help             Show this help message

ENVIRONMENT VARIABLES:
    DOTNET_VERSION      .NET version to use (default: 8.0.x)
    COVERAGE_THRESHOLD  Code coverage threshold
    QUALITY_GATE_ENABLED Enable quality gate validation
    CI_ENVIRONMENT      Set to true in CI/CD environment

EXAMPLES:
    $0                          # Standard build and test
    $0 --parallel --threshold 30 # Parallel execution with custom threshold
    $0 --skip-tests             # Build only
EOF
}

# Parse command line arguments
PARALLEL_MODE=false
MAX_PARALLEL=4
SKIP_TESTS=false
BUILD_CONFIG="Release"

while [[ $# -gt 0 ]]; do
    case $1 in
        --config)
            BUILD_CONFIG="$2"
            shift 2
            ;;
        --threshold)
            COVERAGE_THRESHOLD="$2"
            shift 2
            ;;
        --parallel)
            PARALLEL_MODE=true
            shift
            ;;
        --max-parallel)
            MAX_PARALLEL="$2"
            shift 2
            ;;
        --skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        --help)
            show_help
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Main execution
main() {
    local start_time
    start_time=$(start_timer)
    
    # Initialize environment
    init_pipeline
    
    log_section "Backend Build and Test Execution"
    
    # Check prerequisites
    check_required_tools dotnet git
    check_required_env DOTNET_VERSION
    
    # Create artifact directory
    create_artifact_dir "artifacts/backend"
    
    # Setup .NET environment
    setup_dotnet_environment
    
    # Build the solution
    build_solution
    
    # Generate API client if not skipped
    generate_api_client
    
    # Run tests if not skipped
    if [[ "$SKIP_TESTS" != "true" ]]; then
        run_comprehensive_tests
        parse_test_results
        validate_quality_gates
    fi
    
    # Create build artifacts
    create_build_artifacts
    
    end_timer "$start_time" "backend-build"
    log_success "Backend build and test completed successfully"
}

setup_dotnet_environment() {
    log_section "Setting up .NET Environment"
    
    # Install required tools with retry
    log_info "Installing/updating .NET tools..."
    for attempt in {1..3}; do
        if dotnet tool update --global dotnet-ef --version 8.*; then
            log_success "dotnet-ef tool updated successfully"
            break
        else
            log_warning "Tool update attempt $attempt failed, retrying in 5 seconds..."
            sleep 5
        fi
        
        if [[ $attempt -eq 3 ]]; then
            die "Failed to update dotnet-ef after 3 attempts"
        fi
    done
    
    # Restore local tools
    if [[ -f ".config/dotnet-tools.json" ]]; then
        log_info "Restoring local .NET tools..."
        dotnet tool restore || log_warning "Failed to restore local tools"
    fi
    
    # Restore dependencies with retry
    log_info "Restoring NuGet dependencies..."
    for attempt in {1..3}; do
        if dotnet restore "$SOLUTION_FILE"; then
            log_success "Dependencies restored successfully"
            return 0
        else
            log_warning "Restore attempt $attempt failed, retrying in 10 seconds..."
            sleep 10
        fi
    done
    
    die "Failed to restore dependencies after 3 attempts"
}

build_solution() {
    log_section "Building Solution"
    
    log_info "Building $SOLUTION_FILE in $BUILD_CONFIG configuration..."
    
    if dotnet build "$SOLUTION_FILE" --configuration "$BUILD_CONFIG" --no-restore; then
        log_success "Solution built successfully"
    else
        die "Solution build failed"
    fi
}

generate_api_client() {
    log_section "Generating API Client"
    
    local client_script
    if [[ -f "./Scripts/generate-api-client.sh" ]]; then
        client_script="./Scripts/generate-api-client.sh"
        chmod +x "$client_script"
    elif [[ -f "./Scripts/generate-api-client.ps1" ]]; then
        client_script="pwsh ./Scripts/generate-api-client.ps1"
    else
        log_warning "No API client generation script found, skipping..."
        return 0
    fi
    
    log_info "Running API client generation..."
    if eval "$client_script"; then
        log_success "API client generated successfully"
    else
        log_warning "API client generation failed, continuing with build..."
    fi
}

run_comprehensive_tests() {
    log_section "Running Comprehensive Test Suite"
    
    # Check if Docker is needed for integration tests
    if ! check_docker_access; then
        log_warning "Docker access issues detected, some integration tests may fail"
    fi
    
    # Set up test environment
    export COVERAGE_THRESHOLD
    export QUALITY_GATE_ENABLED="${QUALITY_GATE_ENABLED:-true}"
    export CI_ENVIRONMENT="${CI_ENVIRONMENT:-false}"
    
    # Build test command
    local test_cmd="./Scripts/run-test-suite.sh report json --threshold=$COVERAGE_THRESHOLD"
    
    if [[ "$PARALLEL_MODE" == "true" ]]; then
        test_cmd="$test_cmd --parallel --max-parallel-collections=$MAX_PARALLEL"
    fi
    
    # Make test script executable
    chmod +x ./Scripts/run-test-suite.sh
    
    log_info "Running unified test suite with AI-powered analysis..."
    log_info "Command: $test_cmd"
    
    if eval "$test_cmd"; then
        log_success "All tests passed and quality gates met"
    else
        log_error "Tests failed or quality gates not met"
        
        # Still try to parse results for reporting
        parse_test_results || true
        
        return 1
    fi
}

parse_test_results() {
    log_section "Parsing and Validating Test Results"
    
    local results_file="./TestResults/parsed_results.json"
    
    if [[ ! -f "$results_file" ]]; then
        log_warning "Test results file not found: $results_file"
        return 1
    fi
    
    log_info "Analyzing test results for quality gate validation..."
    
    # Extract key metrics
    local total_tests failed_tests pass_rate coverage_percentage
    total_tests=$(jq -r '.tests.total // 0' "$results_file")
    failed_tests=$(jq -r '.tests.failed // 0' "$results_file")
    pass_rate=$(jq -r '.tests.pass_rate // 0' "$results_file")
    coverage_percentage=$(jq -r '.coverage.line_rate_percentage // 0' "$results_file")
    
    # Log test metrics
    log_info "Test Metrics:"
    log_info "  Total Tests: $total_tests"
    log_info "  Failed Tests: $failed_tests"
    log_info "  Pass Rate: $pass_rate%"
    log_info "  Coverage: $coverage_percentage%"
    
    # Set GitHub Actions outputs
    if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
        {
            echo "total_tests=$total_tests"
            echo "failed_tests=$failed_tests"
            echo "pass_rate=$pass_rate"
            echo "coverage_percentage=$coverage_percentage"
        } >> "$GITHUB_OUTPUT"
    fi
    
    # Copy results to artifacts
    cp "$results_file" "artifacts/backend/" 2>/dev/null || true
}

validate_quality_gates() {
    log_section "Validating Quality Gates"
    
    local results_file="./TestResults/parsed_results.json"
    
    if [[ ! -f "$results_file" ]]; then
        log_warning "Cannot validate quality gates - results file missing"
        return 0
    fi
    
    local pass_rate coverage_percentage
    pass_rate=$(jq -r '.tests.pass_rate // 0' "$results_file")
    coverage_percentage=$(jq -r '.coverage.line_rate_percentage // 0' "$results_file")
    
    local quality_gate_failed=false
    
    # Validate test pass rate (should be 100%)
    if (( $(echo "$pass_rate < 100" | bc -l) )); then
        log_error "Quality gate failed: Test pass rate $pass_rate% < 100%"
        quality_gate_failed=true
    fi
    
    # Validate coverage threshold
    if (( $(echo "$coverage_percentage < $COVERAGE_THRESHOLD" | bc -l) )); then
        log_error "Quality gate failed: Coverage $coverage_percentage% < $COVERAGE_THRESHOLD%"
        quality_gate_failed=true
    fi
    
    if [[ "$quality_gate_failed" == "true" ]]; then
        log_error "One or more quality gates failed"
        return 1
    else
        log_success "All quality gates passed"
        return 0
    fi
}

create_build_artifacts() {
    log_section "Creating Build Artifacts"
    
    # Copy important build outputs
    local artifact_dir="artifacts/backend"
    
    # Copy test results if they exist
    if [[ -d "TestResults" ]]; then
        cp -r TestResults "$artifact_dir/" 2>/dev/null || true
    fi
    
    # Copy coverage reports if they exist
    if [[ -d "CoverageReport" ]]; then
        cp -r CoverageReport "$artifact_dir/" 2>/dev/null || true
    fi
    
    # Copy build outputs for deployment
    local server_project="Code/Zarichney.Server"
    if [[ -d "$server_project/bin/$BUILD_CONFIG" ]]; then
        mkdir -p "$artifact_dir/publish"
        cp -r "$server_project/bin/$BUILD_CONFIG"/* "$artifact_dir/publish/" 2>/dev/null || true
    fi
    
    # Generate build info
    cat > "$artifact_dir/build-info.json" << EOF
{
    "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%S.%3NZ")",
    "commit_sha": "$(git rev-parse HEAD 2>/dev/null || echo 'unknown')",
    "branch": "$(git branch --show-current 2>/dev/null || echo 'unknown')",
    "build_config": "$BUILD_CONFIG",
    "dotnet_version": "${DOTNET_VERSION}",
    "coverage_threshold": $COVERAGE_THRESHOLD
}
EOF
    
    log_info "Build artifacts created in $artifact_dir"
    upload_artifact "backend-build" "$artifact_dir"
}

# Error handling
set -euo pipefail
trap cleanup EXIT

# Check if script is being sourced or executed
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi