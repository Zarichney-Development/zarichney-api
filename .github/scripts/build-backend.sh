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
COVERAGE_THRESHOLD="${COVERAGE_THRESHOLD:-16}"

# Help function
show_help() {
    cat << EOF
Backend Build and Test Script

USAGE:
    $0 [OPTIONS]

OPTIONS:
    --config CONFIG        Build configuration (default: Release)
    --threshold NUM        Coverage threshold percentage (default: $COVERAGE_THRESHOLD)
    --parallel            Enable parallel test execution
    --max-parallel NUM     Maximum parallel collections (default: 4)
    --skip-tests          Skip test execution (build only)
    --allow-low-coverage  Allow build to succeed even with low coverage (warning only)
    --help                Show this help message

ENVIRONMENT VARIABLES:
    DOTNET_VERSION         .NET version to use (default: 8.0.x)
    COVERAGE_THRESHOLD     Code coverage threshold
    QUALITY_GATE_ENABLED   Enable quality gate validation
    CI_ENVIRONMENT         Set to true in CI/CD environment
    COVERAGE_FLEXIBLE      Allow low coverage (true/false) - same as --allow-low-coverage

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
ALLOW_LOW_COVERAGE="${COVERAGE_FLEXIBLE:-false}"

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
        --allow-low-coverage)
            ALLOW_LOW_COVERAGE=true
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
        validate_test_baselines
        # Quality gates are validated within the test suite
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

# Structured output analysis functions for cleaner parsing
is_warning_failure() {
    local output="$1"
    grep -qi "warning.*treated as error" <<< "$output" 2>/dev/null
}

is_compilation_error() {
    local output="$1"
    grep -qi "error" <<< "$output" 2>/dev/null
}

extract_warning_details() {
    local output="$1"
    echo "$output" | grep -i "warning" | head -10 || true
}

extract_error_details() {
    local output="$1"
    echo "$output" | grep -i "error" | head -10 || true
}

build_solution() {
    log_section "Building Solution"
    
    log_info "Building $SOLUTION_FILE in $BUILD_CONFIG configuration..."
    log_info "Warning enforcement: TreatWarningsAsErrors=true enabled"
    
    # Execute build with structured output capture
    local build_output
    local build_exit_code
    
    # Clean output capture pattern
    build_output=$(dotnet build "$SOLUTION_FILE" --configuration "$BUILD_CONFIG" --no-restore 2>&1)
    build_exit_code=$?
    
    if [[ $build_exit_code -eq 0 ]]; then
        log_success "Solution built successfully with zero warnings"
        
        # Validate warning-as-error enforcement is working
        if grep -q "TreatWarningsAsErrors" <<< "$build_output" 2>/dev/null; then
            log_info "✅ Warning-as-error enforcement confirmed active"
        fi
    else
        # Analyze build failure for warning-related issues
        local exit_code=$?
        
        # Structured analysis of build failure type
        if is_warning_failure "$build_output"; then
            log_error "❌ BUILD FAILED: Warnings detected and treated as errors"
            log_error "Fix all compiler warnings before proceeding:"
            extract_warning_details "$build_output"
            
            # Add GitHub Actions annotation for warning failures
            if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
                echo "build_failure_type=warnings" >> "$GITHUB_OUTPUT"
                echo "warning_enforcement_active=true" >> "$GITHUB_OUTPUT"
            fi
            
            die "Build failed due to compiler warnings (zero-warning policy enforced)"
        elif is_compilation_error "$build_output"; then
            log_error "❌ BUILD FAILED: Compilation errors detected"
            extract_error_details "$build_output"
            
            # Add GitHub Actions annotation for compilation failures
            if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
                echo "build_failure_type=compilation" >> "$GITHUB_OUTPUT"
                echo "warning_enforcement_active=true" >> "$GITHUB_OUTPUT"
            fi
            
            die "Solution build failed due to compilation errors"
        else
            log_error "❌ BUILD FAILED: Unknown build failure"
            echo "$build_output"
            
            if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
                echo "build_failure_type=unknown" >> "$GITHUB_OUTPUT"
                echo "warning_enforcement_active=true" >> "$GITHUB_OUTPUT"
            fi
            
            die "Solution build failed"
        fi
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
    export COVERAGE_FLEXIBLE="$ALLOW_LOW_COVERAGE"
    
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
    local coverage_file="./TestResults/coverage_results.json"
    
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
    
    # Extract coverage from separate coverage file
    if [[ -f "$coverage_file" ]]; then
        coverage_percentage=$(jq -r '.line_coverage // 0' "$coverage_file")
    else
        log_warning "Coverage results file not found: $coverage_file - setting coverage to 0%"
        coverage_percentage=0
    fi
    
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
    local coverage_file="./TestResults/coverage_results.json"
    
    if [[ ! -f "$results_file" ]]; then
        log_warning "Cannot validate quality gates - results file missing"
        return 0
    fi
    
    local pass_rate coverage_percentage
    pass_rate=$(jq -r '.tests.pass_rate // 0' "$results_file")
    
    # Extract coverage from separate coverage file
    if [[ -f "$coverage_file" ]]; then
        coverage_percentage=$(jq -r '.line_coverage // 0' "$coverage_file")
    else
        log_warning "Coverage file not found - treating as 0% coverage"
        coverage_percentage=0
    fi
    
    local quality_gate_failed=false
    
    # Validate test pass rate (should be 100%)
    # Convert to integer for comparison (multiply by 100 to handle decimals)
    local pass_rate_int=$(echo "$pass_rate * 100" | bc -l 2>/dev/null || echo "${pass_rate%.*}00")
    if (( pass_rate_int < 10000 )); then
        log_error "Quality gate failed: Test pass rate $pass_rate% < 100%"
        quality_gate_failed=true
    fi
    
    # Validate coverage threshold
    # Convert to integer for comparison (multiply by 100 to handle decimals)
    local coverage_int=$(echo "$coverage_percentage * 100" | bc -l 2>/dev/null || echo "${coverage_percentage%.*}00")
    local threshold_int=$(echo "$COVERAGE_THRESHOLD * 100" | bc -l 2>/dev/null || echo "${COVERAGE_THRESHOLD}00")
    if (( coverage_int < threshold_int )); then
        if [[ "$ALLOW_LOW_COVERAGE" == "true" ]]; then
            log_warning "Quality gate warning: Coverage $coverage_percentage% < $COVERAGE_THRESHOLD% (allowed due to flexibility setting)"
        else
            log_error "Quality gate failed: Coverage $coverage_percentage% < $COVERAGE_THRESHOLD%"
            quality_gate_failed=true
        fi
    fi
    
    if [[ "$quality_gate_failed" == "true" ]]; then
        log_error "One or more quality gates failed"
        return 1
    else
        log_success "All quality gates passed"
        return 0
    fi
}

validate_test_baselines() {
    log_section "Validating Test Suite Baselines"
    
    local results_file="./TestResults/parsed_results.json"
    local coverage_file="./TestResults/coverage_results.json"
    local baseline_file="./TestResults/baseline_validation.json"
    
    if [[ ! -f "$results_file" ]]; then
        log_warning "Cannot validate test baselines - results file missing"
        return 0
    fi
    
    # Check if baseline validation was already performed by the test suite script
    if [[ -f "$baseline_file" ]]; then
        log_info "Using existing baseline validation results from test suite script"
        
        # Extract validation results
        local validation_passed environment_classification skip_percentage violations_count
        validation_passed=$(jq -r '.validation.passesThresholds // false' "$baseline_file")
        environment_classification=$(jq -r '.environment.classification // "unknown"' "$baseline_file")
        skip_percentage=$(jq -r '.metrics.skipPercentage // 0' "$baseline_file")
        violations_count=$(jq -r '.validation.violationsCount // 0' "$baseline_file")
        
        # Log baseline validation summary
        log_info "Baseline Validation Results:"
        log_info "  Environment: $environment_classification"
        log_info "  Skip Percentage: ${skip_percentage}%"
        log_info "  Violations: $violations_count"
        
        if [[ "$validation_passed" == "true" ]]; then
            log_success "Test suite baselines validation passed"
        else
            log_warning "Test suite baselines validation detected issues:"
            
            # Extract and display violations
            local violations
            violations=$(jq -r '.validation.violations[]?' "$baseline_file" 2>/dev/null || echo "")
            if [[ -n "$violations" ]]; then
                while IFS= read -r violation; do
                    if [[ -n "$violation" ]]; then
                        log_warning "  ❌ $violation"
                    fi
                done <<< "$violations"
            fi
            
            # Extract and display recommendations  
            local recommendations
            recommendations=$(jq -r '.validation.recommendations[]?' "$baseline_file" 2>/dev/null || echo "")
            if [[ -n "$recommendations" ]]; then
                log_info "Recommendations:"
                while IFS= read -r recommendation; do
                    if [[ -n "$recommendation" ]]; then
                        log_info "  💡 $recommendation"
                    fi
                done <<< "$recommendations"
            fi
        fi
        
        # Phase 3: Progressive coverage analysis and dynamic quality gates
        local progressive_coverage_exists
        progressive_coverage_exists=$(jq -e '.progressiveCoverage' "$baseline_file" >/dev/null 2>&1 && echo "true" || echo "false")
        
        if [[ "$progressive_coverage_exists" == "true" ]]; then
            log_info "📈 Progressive Coverage Analysis:"
            
            # Extract progressive coverage data
            local current_phase next_target coverage_gap is_on_track required_velocity
            current_phase=$(jq -r '.progressiveCoverage.currentPhase // "Unknown"' "$baseline_file")
            next_target=$(jq -r '.progressiveCoverage.nextTarget // 0' "$baseline_file")
            coverage_gap=$(jq -r '.progressiveCoverage.coverageGap // 0' "$baseline_file")
            is_on_track=$(jq -r '.progressiveCoverage.isOnTrack // false' "$baseline_file")
            required_velocity=$(jq -r '.progressiveCoverage.requiredVelocity // 0' "$baseline_file")
            
            # Log progressive coverage status
            log_info "  Current Phase: $current_phase"
            log_info "  Next Target: ${next_target}% (Gap: ${coverage_gap}%)"
            log_info "  Required Velocity: ${required_velocity}%/month for continuous excellence"
            
            if [[ "$is_on_track" == "true" ]]; then
                log_success "  ✅ Coverage progression on track"
            else
                log_warning "  ⚠️ Coverage progression behind schedule - acceleration needed"
            fi
            
            # Dynamic quality gate adjustment based on progressive phase
            local current_coverage
            current_coverage=$(jq -r '.metrics.lineCoverage // 0' "$baseline_file")
            
            # Set progressive coverage outputs
            if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
                {
                    echo "current_coverage_phase=$current_phase"
                    echo "next_coverage_target=$next_target"
                    echo "coverage_gap=$coverage_gap"
                    echo "progression_on_track=$is_on_track"
                    echo "required_velocity=$required_velocity"
                } >> "$GITHUB_OUTPUT"
            fi
        else
            log_info "No progressive coverage analysis data available"
        fi
        
        # Set GitHub Actions outputs for baseline validation
        if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
            {
                echo "baseline_validation_passed=$validation_passed"
                echo "environment_classification=$environment_classification"
                echo "skip_percentage=$skip_percentage"
                echo "violations_count=$violations_count"
            } >> "$GITHUB_OUTPUT"
        fi
        
        # In CI/CD, baseline validation issues are warnings, not failures
        # The test suite script handles the actual quality gate logic
        return 0
    else
        log_warning "No baseline validation results found - test suite may not have run baseline validation"
        return 0
    fi
}

create_build_artifacts() {
    log_section "Creating Build Artifacts"
    
    # Copy important build outputs
    local artifact_dir="artifacts/backend"
    
    # Ensure artifact directory exists
    mkdir -p "$artifact_dir"
    
    # Copy test results if they exist
    if [[ -d "TestResults" ]]; then
        cp -r TestResults "$artifact_dir/" 2>/dev/null || true
        log_info "Test results copied to artifacts"
    else
        log_warning "No test results found to copy"
        # Create a placeholder to indicate tests were not run
        echo "Tests were not executed or completed" > "$artifact_dir/test-status.txt"
    fi
    
    # Copy coverage reports if they exist
    if [[ -d "CoverageReport" ]]; then
        cp -r CoverageReport "$artifact_dir/" 2>/dev/null || true
        log_info "Coverage reports copied to artifacts"
    fi
    
    # Copy build outputs for deployment
    local server_project="Code/Zarichney.Server"
    if [[ -d "$server_project/bin/$BUILD_CONFIG" ]]; then
        mkdir -p "$artifact_dir/publish"
        cp -r "$server_project/bin/$BUILD_CONFIG"/* "$artifact_dir/publish/" 2>/dev/null || true
        log_info "Build outputs copied to artifacts"
    else
        log_warning "No build outputs found in $server_project/bin/$BUILD_CONFIG"
        # Create a placeholder
        mkdir -p "$artifact_dir/publish"
        echo "Build outputs not available" > "$artifact_dir/publish/build-status.txt"
    fi
    
    # Copy build logs if they exist (for debugging)
    if [[ -f "build.log" ]]; then
        cp "build.log" "$artifact_dir/" 2>/dev/null || true
    fi
    
    # Generate comprehensive build info
    cat > "$artifact_dir/build-info.json" << EOF
{
    "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%S.%3NZ")",
    "commit_sha": "$(git rev-parse HEAD 2>/dev/null || echo 'unknown')",
    "branch": "$(git branch --show-current 2>/dev/null || echo 'unknown')",
    "build_config": "$BUILD_CONFIG",
    "dotnet_version": "${DOTNET_VERSION}",
    "coverage_threshold": $COVERAGE_THRESHOLD,
    "skip_tests": "$SKIP_TESTS",
    "parallel_mode": "$PARALLEL_MODE",
    "allow_low_coverage": "$ALLOW_LOW_COVERAGE"
}
EOF
    
    # Create a summary file
    {
        echo "Build Summary"
        echo "============="
        echo "Timestamp: $(date -u +"%Y-%m-%d %H:%M:%S UTC")"
        echo "Commit: $(git rev-parse HEAD 2>/dev/null || echo 'unknown')"
        echo "Branch: $(git branch --show-current 2>/dev/null || echo 'unknown')"
        echo "Configuration: $BUILD_CONFIG"
        echo ""
        echo "Artifact Contents:"
        find "$artifact_dir" -type f | sort
    } > "$artifact_dir/build-summary.txt"
    
    log_info "Build artifacts created in $artifact_dir"
    
    # List what was actually created for debugging
    if [[ -d "$artifact_dir" ]]; then
        log_info "Artifact directory contents:"
        ls -la "$artifact_dir" || true
    fi
    
    upload_artifact "backend-build" "$artifact_dir"
}

# Error handling
set -euo pipefail
trap cleanup EXIT

# Check if script is being sourced or executed
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi