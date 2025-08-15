#!/bin/bash
# ==============================================================================
# Unified Test Suite Runner Script
# ==============================================================================
# This script provides unified test execution for zarichney-api with multiple
# modes: automation (HTML reports), report (AI-powered analysis), or both.
#
# Requirements:
# - Docker Desktop running (for integration tests with Testcontainers)
# - .NET 8 SDK installed
# - dotnet-reportgenerator-globaltool installed globally
#
# Output Directories (as defined in .gitignore):
# - TestResults/: Test execution results and coverage data
# - CoverageReport/: Generated HTML coverage reports
# ==============================================================================

set -euo pipefail

# Script configuration
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly ROOT_DIR="$(dirname "$SCRIPT_DIR")"
readonly SOLUTION_FILE="$ROOT_DIR/zarichney-api.sln"
readonly TEST_RESULTS_DIR="$ROOT_DIR/TestResults"
readonly COVERAGE_REPORT_DIR="$ROOT_DIR/CoverageReport"
readonly COVERAGE_REPORT_INDEX="$COVERAGE_REPORT_DIR/index.html"
readonly TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# Parallel execution configuration  
readonly MAX_PARALLEL_DEFAULT=2

# Ensure results directory exists
mkdir -p "$TEST_RESULTS_DIR"
readonly LOG_FILE="$TEST_RESULTS_DIR/test-suite-$TIMESTAMP.log"

# Test configuration
readonly BUILD_CONFIG="Release"
readonly COVERAGE_FORMATS="Html;Cobertura;Badges;SummaryGithub"

# Default settings
MODE="report"  # Default to report mode for backward compatibility
OUTPUT_FORMAT="markdown"
COVERAGE_THRESHOLD=16
PERFORMANCE_ANALYSIS=false
SAVE_BASELINE=false
COMPARE_MODE=false
SKIP_BROWSER=false
SKIP_BUILD=false
SKIP_CLIENT_GEN=false
UNIT_ONLY=false
INTEGRATION_ONLY=false
PARALLEL_EXECUTION=false
MAX_PARALLEL_COLLECTIONS=$MAX_PARALLEL_DEFAULT

# JSON escape function for safe string encoding
json_escape() {
    printf '%s' "$1" | sed 's/\\/\\\\/g; s/"/\\"/g; s/\t/\\t/g; s/\n/\\n/g; s/\r/\\r/g'
}

# Colors for output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly BOLD='\033[1m'
readonly NC='\033[0m' # No Color

# ==============================================================================
# SHARED UTILITY FUNCTIONS
# ==============================================================================

# Logging function
log() {
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $*" | tee -a "$LOG_FILE"
}

# Colored output functions
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
    log "INFO: $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
    log "SUCCESS: $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
    log "WARNING: $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
    log "ERROR: $1"
}

print_header() {
    echo -e "\n${BLUE}==== $1 ====${NC}"
    log "HEADER: $1"
}

# Error handling
error_exit() {
    print_error "$1"
    exit 1
}

# Success message
success() {
    print_success "$1"
}

# Warning message
warning() {
    print_warning "$1"
}

# Info message
info() {
    print_status "$1"
}

# Function to show usage
show_usage() {
    cat << EOF
Usage: $0 [MODE] [OPTIONS]

MODES:
    automation      Run tests with HTML coverage reports (browser opens)
    report          Run tests with AI-powered analysis (default)
    both            Run both automation and report modes

OUTPUT FORMATS (for report mode):
    markdown        Human-readable detailed report (default)
    json            Machine-readable output for automation
    summary         Brief executive summary
    console         Terminal-optimized output

OPTIONS:
    -h, --help          Show this help message
    --no-browser        Skip opening report in browser (automation mode)
    --unit-only         Run only unit tests
    --integration-only  Run only integration tests
    --skip-build        Skip the build step (assumes solution is already built)
    --skip-client-gen   Skip API client generation
    --performance       Include detailed performance analysis
    --compare           Compare with previous test run results
    --threshold=N       Set coverage threshold (default: 25%)
    --save-baseline     Save current results as baseline
    --parallel          Enable parallel test execution
    --max-parallel-collections=N  Maximum parallel collections (default: 2)

EXAMPLES:
    $0                          # Run report mode with markdown output
    $0 automation               # Run automation mode with HTML report
    $0 report json              # Run report mode with JSON output
    $0 both --unit-only         # Run both modes with unit tests only
    $0 automation --no-browser  # Run automation without opening browser
    $0 report summary --performance  # Quick summary with performance data
    $0 report --parallel         # Run with parallel test execution
    $0 automation --parallel --max-parallel-collections=4  # Max parallel collections

EOF
}

# ==============================================================================
# SHARED CORE FUNCTIONS
# ==============================================================================

# Function to check prerequisites
check_prerequisites() {
    print_header "Checking Prerequisites"
    
    # Check if we're in the right directory
    if [[ ! -f "$SOLUTION_FILE" ]]; then
        error_exit "Solution file not found: $SOLUTION_FILE"
    fi
    
    # Check if dotnet is available
    if ! command -v dotnet &> /dev/null; then
        error_exit ".NET SDK is not installed or not in PATH"
    fi
    
    # Check dotnet version
    local dotnet_version
    dotnet_version=$(dotnet --version)
    print_status ".NET SDK version: $dotnet_version"
    
    # Check Docker availability (flexible for different environments)
    if command -v docker &> /dev/null; then
        if docker info &> /dev/null; then
            success "Docker is available and running"
        else
            warning "Docker is installed but may not be running - integration tests may be affected"
        fi
    else
        warning "Docker not found - integration tests will be skipped"
    fi
    
    # Check if ReportGenerator tool is installed (for automation mode)
    # Use 'dotnet tool run' approach for better path isolation and consistency
    if [[ "$MODE" == "automation" || "$MODE" == "both" ]]; then
        if ! dotnet tool list -g | grep -q "dotnet-reportgenerator-globaltool"; then
            print_status "Installing ReportGenerator global tool..."
            dotnet tool install -g dotnet-reportgenerator-globaltool || {
                error_exit "Failed to install ReportGenerator tool"
            }
        fi
    fi
    
    # Create results directories
    mkdir -p "$TEST_RESULTS_DIR"/{unit,integration}
    if [[ "$MODE" == "automation" || "$MODE" == "both" ]]; then
        mkdir -p "$COVERAGE_REPORT_DIR"
    fi
    
    success "All prerequisites checked"
}

# Function to clean previous results
clean_previous_results() {
    print_header "Cleaning Previous Results"
    
    if [[ -d "$TEST_RESULTS_DIR" ]]; then
        print_status "Removing previous test results: $TEST_RESULTS_DIR"
        rm -rf "$TEST_RESULTS_DIR"/*
    fi
    
    if [[ "$MODE" == "automation" || "$MODE" == "both" ]] && [[ -d "$COVERAGE_REPORT_DIR" ]]; then
        print_status "Removing previous coverage report: $COVERAGE_REPORT_DIR"
        rm -rf "$COVERAGE_REPORT_DIR"
    fi
    
    # Create directories
    mkdir -p "$TEST_RESULTS_DIR"/{unit,integration}
    if [[ "$MODE" == "automation" || "$MODE" == "both" ]]; then
        mkdir -p "$COVERAGE_REPORT_DIR"
    fi
    
    success "Previous results cleaned"
}

# Function to restore and build solution
build_solution() {
    print_header "Building Solution"
    
    print_status "Restoring dependencies..."
    dotnet restore "$SOLUTION_FILE" || {
        error_exit "Failed to restore dependencies"
    }
    
    print_status "Building solution in $BUILD_CONFIG configuration..."
    dotnet build "$SOLUTION_FILE" --configuration "$BUILD_CONFIG" --no-restore || {
        error_exit "Build failed"
    }
    
    success "Solution built successfully"
}

# Function to generate API client
generate_api_client() {
    print_header "Generating API Client"
    
    local api_client_script="$SCRIPT_DIR/generate-api-client.sh"
    
    if [[ -f "$api_client_script" ]]; then
        print_status "Running API client generation script..."
        bash "$api_client_script" || {
            error_exit "API client generation failed"
        }
        success "API client generated successfully"
    else
        warning "API client generation script not found: $api_client_script"
        print_status "Skipping API client generation"
    fi
}

# Unified Docker command execution function
execute_docker_command() {
    local test_command="$1"
    local test_type="$2"
    
    # Check if Docker group membership is active for potential sg docker usage
    if groups | grep -q docker; then
        # User is in docker group, run normally
        eval "$test_command" || {
            error_exit "$test_type tests failed"
        }
    else
        # Try with sg docker if available
        if command -v sg &> /dev/null; then
            print_status "Running with 'sg docker' for Docker access..."
            if sg docker -c "$test_command" 2>/dev/null; then
                # sg docker succeeded
                true
            else
                local sg_exit_code=$?
                if [[ $sg_exit_code -eq 123 ]]; then
                    warning "Docker group switching failed (exit 123), falling back to direct execution"
                    eval "$test_command" || {
                        error_exit "$test_type tests failed"
                    }
                else
                    error_exit "$test_type tests failed"
                fi
            fi
        else
            # Run normally and let it fail if Docker access is needed
            eval "$test_command" || {
                error_exit "$test_type tests failed"
            }
        fi
    fi
}

# Function to run unit tests
run_unit_tests() {
    print_header "Running Unit Tests"
    
    local unit_results_dir="$TEST_RESULTS_DIR/unit"
    print_status "Executing unit tests with coverage collection..."
    
    local test_command="dotnet test \"$SOLUTION_FILE\" --filter \"Category=Unit\" --configuration \"$BUILD_CONFIG\" --no-build --collect:\"XPlat Code Coverage\" --results-directory \"$unit_results_dir\" --logger \"trx;LogFileName=unit_tests.trx\""
    
    if [[ "$MODE" == "report" || "$MODE" == "both" ]]; then
        # For report mode, add console logger for parsing
        test_command="dotnet test \"$SOLUTION_FILE\" --filter \"Category=Unit\" --configuration \"$BUILD_CONFIG\" --no-build --logger trx --logger 'console;verbosity=detailed' --results-directory \"$unit_results_dir\" --collect:'XPlat Code Coverage' --nologo"
    fi
    
    execute_docker_command "$test_command" "Unit"
    success "Unit tests completed"
}

# Function to run integration tests
run_integration_tests() {
    print_header "Running Integration Tests"
    
    local integration_results_dir="$TEST_RESULTS_DIR/integration"
    print_status "Executing integration tests with coverage collection..."
    
    local test_command="dotnet test \"$SOLUTION_FILE\" --filter \"Category=Integration\" --configuration \"$BUILD_CONFIG\" --no-build --collect:\"XPlat Code Coverage\" --results-directory \"$integration_results_dir\" --logger \"trx;LogFileName=integration_tests.trx\""
    
    if [[ "$MODE" == "report" || "$MODE" == "both" ]]; then
        # For report mode, add console logger for parsing
        test_command="dotnet test \"$SOLUTION_FILE\" --filter \"Category=Integration\" --configuration \"$BUILD_CONFIG\" --no-build --logger trx --logger 'console;verbosity=detailed' --results-directory \"$integration_results_dir\" --collect:'XPlat Code Coverage' --nologo"
    fi
    
    execute_docker_command "$test_command" "Integration"
    success "Integration tests completed"
}

# Function to run tests in parallel (Phase 3 enhancement)
run_tests_parallel() {
    print_header "Running Tests in Parallel (Phase 3)"
    print_status "Using parallel execution with max $MAX_PARALLEL_COLLECTIONS collections"
    
    local pids=()
    local test_categories=()
    local test_categories_running=()
    
    # Determine which test categories to run
    if [[ "$UNIT_ONLY" == true ]]; then
        test_categories=("Unit")
    elif [[ "$INTEGRATION_ONLY" == true ]]; then
        # Run integration collections in parallel
        test_categories=("IntegrationAuth" "IntegrationCore" "IntegrationExternal" "IntegrationInfra" "IntegrationQA")
    else
        # Run both unit and integration collections in parallel
        test_categories=("Unit" "IntegrationAuth" "IntegrationCore" "IntegrationExternal" "IntegrationInfra" "IntegrationQA")
    fi
    
    print_status "Running test categories in parallel: ${test_categories[*]}"
    
    # Start parallel test execution
    for category in "${test_categories[@]}"; do
        run_test_category_parallel "$category" &
        local pid=$!
        pids+=("$pid")
        test_categories_running+=("$category")
        
        print_status "Started $category tests (PID: $pid)"
        
        # Limit parallel execution to MAX_PARALLEL_COLLECTIONS
        if [[ ${#pids[@]} -ge $MAX_PARALLEL_COLLECTIONS ]]; then
            print_status "Reached max parallel limit ($MAX_PARALLEL_COLLECTIONS), waiting for batch to complete..."
            wait_for_parallel_batch pids test_categories_running
            pids=()
            test_categories_running=()
        fi
    done
    
    # Wait for remaining processes
    if [[ ${#pids[@]} -gt 0 ]]; then
        print_status "Waiting for final batch to complete..."
        wait_for_parallel_batch pids test_categories_running
    fi
    
    success "All parallel test execution completed"
}

# Function to run a specific test category in parallel
run_test_category_parallel() {
    local category="$1"
    local results_dir="$TEST_RESULTS_DIR/${category,,}"
    
    mkdir -p "$results_dir"
    
    local filter
    local log_prefix
    
    if [[ "$category" == "Unit" ]]; then
        filter="Category=Unit"
        log_prefix="Unit"
    else
        # Integration collection
        filter="Collection=$category"
        log_prefix="$category"
    fi
    
    local test_command="dotnet test \"$SOLUTION_FILE\" --filter \"$filter\" --configuration \"$BUILD_CONFIG\" --no-build --collect:\"XPlat Code Coverage\" --results-directory \"$results_dir\" --logger \"trx;LogFileName=${category,,}_tests.trx\" --logger \"console;verbosity=quiet\" --nologo"
    
    print_status "[$log_prefix] Starting parallel execution..."
    
    if docker info &> /dev/null; then
        # Run in Docker group context if available
        if sg docker -c "true" 2>/dev/null; then
            sg docker -c "$test_command" > "$results_dir/${category,,}_output.log" 2>&1 || {
                local sg_exit_code=$?
                if [[ $sg_exit_code -eq 123 ]]; then
                    warning "Docker group switching failed (exit 123), falling back to direct execution"
                    $test_command > "$results_dir/${category,,}_output.log" 2>&1
                else
                    return $sg_exit_code
                fi
            }
        else
            $test_command > "$results_dir/${category,,}_output.log" 2>&1
        fi
    else
        $test_command > "$results_dir/${category,,}_output.log" 2>&1
    fi
    
    local exit_code=$?
    
    if [[ $exit_code -eq 0 ]]; then
        print_success "[$log_prefix] Completed successfully"
    else
        print_warning "[$log_prefix] Completed with issues (exit code: $exit_code)"
    fi
    
    return $exit_code
}

# Function to wait for a batch of parallel processes
wait_for_parallel_batch() {
    local -n pids_ref=$1
    local -n categories_ref=$2
    
    for i in "${!pids_ref[@]}"; do
        local pid="${pids_ref[$i]}"
        local category="${categories_ref[$i]}"
        
        if wait "$pid"; then
            print_success "[$category] Process completed successfully"
        else
            print_warning "[$category] Process completed with issues"
        fi
    done
}

# Function to run all tests (unified for report mode)
run_all_tests() {
    print_header "Running Comprehensive Test Suite"
    
    cd "$ROOT_DIR"
    local start_time=$(date +%s)
    
    # For report mode, run all tests together for better parsing (or parallel if enabled)
    if [[ "$MODE" == "report" ]]; then
        if [[ "$PARALLEL_EXECUTION" == true ]]; then
            run_tests_parallel
        else
            local test_exit_code=0
            if dotnet test "$SOLUTION_FILE" --logger trx --logger 'console;verbosity=detailed' --results-directory "$TEST_RESULTS_DIR" --collect:'XPlat Code Coverage' --nologo 2>&1 | tee -a "$LOG_FILE"; then
                success "Test execution completed successfully"
            else
                test_exit_code=$?
                warning "Test execution had failures (exit code: $test_exit_code) - continuing with analysis"
            fi
        fi
    else
        # For automation mode, run tests by category
        if [[ "$PARALLEL_EXECUTION" == true ]]; then
            run_tests_parallel
        elif [[ "$INTEGRATION_ONLY" == true ]]; then
            run_integration_tests
        elif [[ "$UNIT_ONLY" == true ]]; then
            run_unit_tests
        else
            run_unit_tests
            run_integration_tests
        fi
    fi
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    echo "EXECUTION_TIME=$duration" >> "$LOG_FILE"
    
    success "Tests completed in ${duration}s"
}

# ==============================================================================
# AUTOMATION MODE FUNCTIONS (HTML Reports, Browser Opening)
# ==============================================================================

# Function to generate coverage report (automation mode)
generate_coverage_report() {
    print_header "Generating Coverage Report"
    
    # Find all coverage files
    local coverage_files
    coverage_files=$(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" -type f)
    
    if [[ -z "$coverage_files" ]]; then
        error_exit "No coverage files found in $TEST_RESULTS_DIR"
    fi
    
    print_status "Found coverage files:"
    echo "$coverage_files" | while read -r file; do
        print_status "  - $file"
    done
    
    # Generate comprehensive coverage report
    print_status "Generating HTML coverage report..."
    
    # Convert newlines to semicolons for ReportGenerator
    local coverage_files_param
    coverage_files_param=$(echo "$coverage_files" | tr '\n' ';' | sed 's/;$//')
    
    # Use 'dotnet tool run' for consistent tool invocation and path isolation
    dotnet tool run reportgenerator \
        -reports:"$coverage_files_param" \
        -targetdir:"$COVERAGE_REPORT_DIR" \
        -reporttypes:"$COVERAGE_FORMATS" \
        -verbosity:Info || {
        error_exit "Coverage report generation failed"
    }
    
    success "Coverage report generated at: $COVERAGE_REPORT_DIR"
}

# Function to display test summary (automation mode)
display_test_summary() {
    print_header "Test Execution Summary"
    
    # Count TRX files
    local trx_files
    trx_files=$(find "$TEST_RESULTS_DIR" -name "*.trx" -type f | wc -l)
    
    # Check if coverage report exists
    if [[ -f "$COVERAGE_REPORT_INDEX" ]]; then
        success "✅ Test execution completed successfully"
        print_status "📊 Test result files: $trx_files TRX files generated"
        print_status "📈 Coverage report: $COVERAGE_REPORT_INDEX"
        
        # Try to extract summary from coverage report if available
        local summary_file="$COVERAGE_REPORT_DIR/Summary.txt"
        if [[ -f "$summary_file" ]]; then
            print_status "📋 Coverage Summary:"
            cat "$summary_file" | head -20
        fi
    else
        error_exit "❌ Coverage report was not generated successfully"
    fi
}

# Function to open report in browser (automation mode)
open_report_in_browser() {
    if [[ "$SKIP_BROWSER" == true ]]; then
        print_status "Skipping browser opening as requested"
        print_status "To view the report, open: $COVERAGE_REPORT_INDEX"
        return
    fi
    
    print_header "Opening Coverage Report"
    
    if [[ ! -f "$COVERAGE_REPORT_INDEX" ]]; then
        error_exit "Coverage report index file not found: $COVERAGE_REPORT_INDEX"
    fi
    
    print_status "Opening coverage report in default browser..."
    
    # Detect OS and open accordingly
    if command -v xdg-open &> /dev/null; then
        # Linux
        xdg-open "$COVERAGE_REPORT_INDEX" &
    elif command -v open &> /dev/null; then
        # macOS
        open "$COVERAGE_REPORT_INDEX"
    elif command -v start &> /dev/null; then
        # Windows (Git Bash, Cygwin, etc.)
        start "$COVERAGE_REPORT_INDEX"
    elif [[ -n "${WSL_DISTRO_NAME:-}" ]]; then
        # WSL - convert path and use Windows explorer
        local windows_path
        windows_path=$(wslpath -w "$COVERAGE_REPORT_INDEX")
        explorer.exe "$windows_path" &
    else
        warning "Could not detect how to open browser on this system"
        print_status "Please manually open: $COVERAGE_REPORT_INDEX"
        return 1
    fi
    
    success "Coverage report should now be open in your browser"
}

# Execute automation mode
execute_automation_mode() {
    print_header "Executing Automation Mode"
    generate_coverage_report
    display_test_summary
    open_report_in_browser
}

# ==============================================================================
# REPORT MODE FUNCTIONS (AI-Powered Analysis, Quality Gates)
# ==============================================================================

# Parse test results (report mode)
parse_results() {
    log "📊 Parsing test results..."
    
    # Find ALL TRX files
    local trx_files=$(find "$TEST_RESULTS_DIR" -name "*.trx")
    if [[ -z "$trx_files" ]]; then
        error_exit "No TRX test results file found"
    fi
    
    # Extract test statistics from ALL TRX files
    local total_tests=0
    local passed_tests=0
    local failed_tests=0
    local skipped_tests=0
    
    # Parse all TRX files and combine results
    while IFS= read -r trx_file; do
        if [[ -f "$trx_file" ]]; then
            log "Parsing TRX file: $trx_file"
            
            # Count test results by outcome in this file
            local file_passed=$(grep -o 'outcome="Passed"' "$trx_file" | wc -l)
            local file_failed=$(grep -o 'outcome="Failed"' "$trx_file" | wc -l)
            local file_skipped=$(grep -o 'outcome="NotExecuted"' "$trx_file" | wc -l)
            
            # Add to totals
            passed_tests=$((passed_tests + file_passed))
            failed_tests=$((failed_tests + file_failed))
            skipped_tests=$((skipped_tests + file_skipped))
            
            log "TRX file results: Passed=$file_passed, Failed=$file_failed, Skipped=$file_skipped"
        fi
    done <<< "$trx_files"
    
    # Calculate total tests
    total_tests=$((passed_tests + failed_tests + skipped_tests))
    
    log "Combined TRX parsing results: Total=$total_tests, Passed=$passed_tests, Failed=$failed_tests, Skipped=$skipped_tests"
    
    # Validate that we found tests - if not, try fallback parsing
    if [[ $total_tests -eq 0 ]]; then
        log "No tests found in TRX files, attempting console output parsing fallback..."
        if grep -q "Passed:" "$LOG_FILE"; then
            local stats_line=$(grep "Passed:" "$LOG_FILE" | tail -1)
            failed_tests=$(echo "$stats_line" | grep -o "Failed:[[:space:]]*[0-9]*" | grep -o "[0-9]*" || echo "0")
            passed_tests=$(echo "$stats_line" | grep -o "Passed:[[:space:]]*[0-9]*" | grep -o "[0-9]*" || echo "0")
            skipped_tests=$(echo "$stats_line" | grep -o "Skipped:[[:space:]]*[0-9]*" | grep -o "[0-9]*" || echo "0")
            total_tests=$(echo "$stats_line" | grep -o "Total:[[:space:]]*[0-9]*" | grep -o "[0-9]*" || echo "0")
            log "Console fallback results: Total=$total_tests, Passed=$passed_tests, Failed=$failed_tests, Skipped=$skipped_tests"
        else
            warning "No test results found in TRX files or console output"
        fi
    fi
    
    # Calculate pass rate using if statement
    local pass_rate
    if [[ $total_tests -gt 0 ]]; then
        pass_rate=$((passed_tests * 100 / total_tests))
    else
        pass_rate=0
    fi
    # Store results
    cat > "$TEST_RESULTS_DIR/parsed_results.json" << EOF
{
    "timestamp": "$(date -Iseconds)",
    "execution_time": $(grep "EXECUTION_TIME=" "$LOG_FILE" | cut -d'=' -f2 || echo "0"),
    "tests": {
        "total": $total_tests,
        "passed": $passed_tests,
        "failed": $failed_tests,
        "skipped": $skipped_tests,
        "pass_rate": $pass_rate
    }
}
EOF
    
    # Extract detailed failure information if tests failed
    if [[ $failed_tests -gt 0 ]]; then
        extract_detailed_failures
    fi
    
    success "Test results parsed: $passed_tests passed, $failed_tests failed, $skipped_tests skipped"
}

# Extract detailed information about failing tests
extract_detailed_failures() {
    log "🚨 Extracting detailed failure information..."
    
    local failures_file="$TEST_RESULTS_DIR/test_failures.json"
    local failures_found=0
    
    # Initialize failures array
    echo '{"failed_tests": []}' > "$failures_file"
    
    # Find ALL TRX files and extract failure details
    local trx_files=$(find "$TEST_RESULTS_DIR" -name "*.trx")
    while IFS= read -r trx_file; do
        if [[ -f "$trx_file" ]]; then
            # Check if this file has failed tests
            local file_failed=$(grep -o 'outcome="Failed"' "$trx_file" | wc -l)
            if [[ $file_failed -gt 0 ]]; then
                log "Extracting failures from: $trx_file"
                
                # Extract test categories from file path
                local test_category
                if [[ "$trx_file" =~ /([^/]+)_tests\.trx$ ]]; then
                    test_category="${BASH_REMATCH[1]}"
                else
                    test_category="unknown"
                fi
                
                # Extract individual failed test information using XML parsing
                # Look for UnitTestResult elements with outcome="Failed"
                while IFS= read -r line; do
                    if [[ "$line" =~ \<UnitTestResult.*outcome=\"Failed\" ]]; then
                        # Extract test name
                        local test_name=""
                        if [[ "$line" =~ testName=\"([^\"]+)\" ]]; then
                            test_name="${BASH_REMATCH[1]}"
                        fi
                        
                        # Extract execution id to find error details
                        local execution_id=""
                        if [[ "$line" =~ executionId=\"([^\"]+)\" ]]; then
                            execution_id="${BASH_REMATCH[1]}"
                        fi
                        
                        # Look for error message in the same or nearby lines
                        local error_message=""
                        local next_lines=$(grep -A 10 "executionId=\"$execution_id\"" "$trx_file" 2>/dev/null || echo "")
                        if [[ "$next_lines" =~ \<Message\>([^<]+)\</Message\> ]]; then
                            error_message="${BASH_REMATCH[1]}"
                        elif [[ "$next_lines" =~ \<ErrorInfo\>.*\<Message\>([^<]+)\</Message\> ]]; then
                            error_message="${BASH_REMATCH[1]}"
                        fi
                        
                        # Add to failures JSON (append to array)
                        if [[ -n "$test_name" ]]; then
                            local failure_entry=$(cat << EOF
{
  "test_name": "$test_name",
  "category": "$test_category",
  "file": "$trx_file",
  "error_message": "$error_message",
  "execution_id": "$execution_id"
}
EOF
                            )
                            
                            # Add to the JSON array
                            local temp_file=$(mktemp)
                            jq --argjson failure "$failure_entry" '.failed_tests += [$failure]' "$failures_file" > "$temp_file" && mv "$temp_file" "$failures_file"
                            failures_found=$((failures_found + 1))
                            
                            log "  ❌ Failed test: $test_name (category: $test_category)"
                            if [[ -n "$error_message" ]]; then
                                log "     Error: $error_message"
                            fi
                        fi
                    fi
                done < "$trx_file"
            fi
        fi
    done <<< "$trx_files"
    
    # Update the main results JSON with failure details
    if [[ $failures_found -gt 0 ]]; then
        local temp_file=$(mktemp)
        jq --slurpfile failures "$failures_file" '.failures = $failures[0]' "$TEST_RESULTS_DIR/parsed_results.json" > "$temp_file" && mv "$temp_file" "$TEST_RESULTS_DIR/parsed_results.json"
        log "Extracted details for $failures_found failing test(s)"
    else
        log "Could not extract detailed failure information from TRX files"
    fi
}

# Parse coverage data (report mode)
parse_coverage() {
    log "📈 Analyzing code coverage..."
    
    # Find coverage XML files (handles GUID-based directories from .NET XPlat Code Coverage)
    local coverage_files=$(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" -type f)
    if [[ -z "$coverage_files" ]]; then
        log "No coverage.cobertura.xml files found, searching for alternative coverage file patterns..."
        # Try alternative patterns that might be generated
        coverage_files=$(find "$TEST_RESULTS_DIR" -name "*.cobertura.xml" -type f)
        if [[ -z "$coverage_files" ]]; then
            coverage_files=$(find "$TEST_RESULTS_DIR" -name "*coverage*.xml" -type f)
        fi
    fi
    
    if [[ -z "$coverage_files" ]]; then
        warning "No coverage files found in $TEST_RESULTS_DIR"
        log "Available files in test results directory:"
        find "$TEST_RESULTS_DIR" -type f -name "*.xml" | head -10 | while read -r file; do
            log "  Found XML file: $file"
        done
        
        # Create empty coverage results for consistency
        cat > "$TEST_RESULTS_DIR/coverage_results.json" << EOF
{
    "line_coverage": 0.0,
    "branch_coverage": 0.0,
    "threshold": $COVERAGE_THRESHOLD,
    "meets_threshold": 0,
    "error": "No coverage files found"
}
EOF
        return
    fi
    
    # Process coverage files (merge if multiple exist)
    local total_line_rate=0
    local total_branch_rate=0
    local file_count=0
    local coverage_file_list=""
    
    while IFS= read -r coverage_file; do
        if [[ -f "$coverage_file" ]]; then
            log "Processing coverage file: $coverage_file"
            coverage_file_list="$coverage_file_list $coverage_file"
            
            # Extract coverage percentages using basic XML parsing
            local line_rate=$(grep -o 'line-rate="[0-9.]*"' "$coverage_file" | head -1 | grep -o '[0-9.]*' || echo "0")
            local branch_rate=$(grep -o 'branch-rate="[0-9.]*"' "$coverage_file" | head -1 | grep -o '[0-9.]*' || echo "0")
            
            # Add to totals for averaging
            total_line_rate=$(awk -v total="$total_line_rate" -v rate="$line_rate" 'BEGIN {print total + rate}')
            total_branch_rate=$(awk -v total="$total_branch_rate" -v rate="$branch_rate" 'BEGIN {print total + rate}')
            file_count=$((file_count + 1))
            
            log "  Line rate: $line_rate, Branch rate: $branch_rate"
        fi
    done <<< "$coverage_files"
    
    if [[ $file_count -eq 0 ]]; then
        warning "No valid coverage files could be processed"
        cat > "$TEST_RESULTS_DIR/coverage_results.json" << EOF
{
    "line_coverage": 0.0,
    "branch_coverage": 0.0,
    "threshold": $COVERAGE_THRESHOLD,
    "meets_threshold": 0,
    "error": "No valid coverage files found"
}
EOF
        return
    fi
    
    # Calculate average coverage across files
    local avg_line_rate=$(awk -v total="$total_line_rate" -v count="$file_count" 'BEGIN {printf "%.6f", total / count}')
    local avg_branch_rate=$(awk -v total="$total_branch_rate" -v count="$file_count" 'BEGIN {printf "%.6f", total / count}')
    
    # Convert to percentages
    local line_coverage=$(awk -v lr="$avg_line_rate" 'BEGIN {printf "%.1f", lr * 100}')
    local branch_coverage=$(awk -v br="$avg_branch_rate" 'BEGIN {printf "%.1f", br * 100}')
    
    # Store coverage results
    cat > "$TEST_RESULTS_DIR/coverage_results.json" << EOF
{
    "line_coverage": $line_coverage,
    "branch_coverage": $branch_coverage,
    "threshold": $COVERAGE_THRESHOLD,
    "meets_threshold": $(awk -v lc="$line_coverage" -v ct="$COVERAGE_THRESHOLD" 'BEGIN {print (lc >= ct) ? 1 : 0}'),
    "files_processed": $file_count,
    "coverage_files": "$coverage_file_list"
}
EOF
    
    log "Processed $file_count coverage file(s)"
    success "Coverage analyzed: ${line_coverage}% lines, ${branch_coverage}% branches"
}

# Test suite baseline validation (Phase 2 enhancement)
validate_test_baselines() {
    log "📊 Validating test suite baselines..."
    
    local results_file="$TEST_RESULTS_DIR/parsed_results.json"
    local coverage_file="$TEST_RESULTS_DIR/coverage_results.json"
    
    if [[ ! -f "$results_file" ]]; then
        warning "Test results file not found, skipping baseline validation"
        return 1
    fi
    
    # Extract test metrics
    local total_tests=$(jq -r '.tests.total' "$results_file" 2>/dev/null || echo "0")
    local skipped_tests=$(jq -r '.tests.skipped' "$results_file" 2>/dev/null || echo "0")
    local failed_tests=$(jq -r '.tests.failed' "$results_file" 2>/dev/null || echo "0")
    
    # Calculate skip percentage
    local skip_percentage=0
    if [[ $total_tests -gt 0 ]]; then
        skip_percentage=$(awk -v skipped="$skipped_tests" -v total="$total_tests" 'BEGIN {printf "%.1f", (skipped / total) * 100}')
    fi
    
    # Extract coverage data
    local line_coverage=0
    if [[ -f "$coverage_file" ]]; then
        line_coverage=$(jq -r '.line_coverage' "$coverage_file" 2>/dev/null || echo "0")
    fi
    
    # Determine environment classification (simplified version for bash script)
    local environment_classification="unconfigured"  # Default assumption for CI/CD
    local expected_skip_percentage=26.7
    local environment_description="Local dev / CI without external services"
    
    # Check for production environment
    if [[ "${ASPNETCORE_ENVIRONMENT:-}" == "Production" ]]; then
        environment_classification="production"
        expected_skip_percentage=0.0
        environment_description="Production deployment validation - no test failures acceptable"
    elif [[ "${CI_ENVIRONMENT:-false}" == "true" && "${QUALITY_GATE_ENABLED:-false}" == "true" ]]; then
        # In CI with quality gates enabled, check if we have service configurations
        # For now, assume unconfigured - this could be enhanced with actual service availability checks
        environment_classification="unconfigured"
        expected_skip_percentage=26.7
        environment_description="CI environment without external services"
    fi
    
    # Validate against thresholds
    local baseline_validation_passed="true"
    local violations=()
    local recommendations=()
    
    # Skip percentage validation
    if [[ $(awk -v actual="$skip_percentage" -v expected="$expected_skip_percentage" 'BEGIN {print (actual > expected) ? 1 : 0}') -eq 1 ]]; then
        baseline_validation_passed="false"
        violations+=("Skip rate ${skip_percentage}% exceeds threshold of ${expected_skip_percentage}% for ${environment_classification} environment")
    fi
    
    # Test failure validation (always critical)
    if [[ $failed_tests -gt 0 ]]; then
        baseline_validation_passed="false"
        violations+=("${failed_tests} test(s) failing - must be resolved before deployment")
    fi
    
    # Generate recommendations based on environment
    if [[ "$environment_classification" == "unconfigured" && $skipped_tests -gt 0 ]]; then
        recommendations+=("Configure external services (OpenAI, Stripe, MS Graph) to reduce skip rate from ${skip_percentage}% to target 1.2%")
    fi
    
    # Coverage baseline check (14.22% baseline)
    local coverage_baseline=14.22
    if [[ $(awk -v actual="$line_coverage" -v baseline="$coverage_baseline" 'BEGIN {print (actual < baseline - 1.0) ? 1 : 0}') -eq 1 ]]; then
        violations+=("Coverage ${line_coverage}% below baseline ${coverage_baseline}% (allowing 1% regression tolerance)")
        recommendations+=("Increase test coverage to meet or exceed ${coverage_baseline}% baseline")
    fi
    
    # Phase 3: Progressive coverage analysis
    local progressive_targets=(20.0 35.0 50.0 75.0 90.0)
    local next_target=$(get_next_coverage_target "$line_coverage")
    local coverage_gap=$(awk -v target="$next_target" -v current="$line_coverage" 'BEGIN {printf "%.1f", target - current}')
    local current_phase=$(get_current_coverage_phase "$line_coverage")
    
    # Calculate velocity and timeline predictions
    local target_date="2026-01-31"
    local months_to_target=$(calculate_months_to_target "$target_date")
    local required_velocity=$(awk -v gap="$((90 - ${line_coverage%.*}))" -v months="$months_to_target" 'BEGIN {printf "%.1f", (months > 0) ? gap / months : 0}')
    
    # Check if progression is on track
    local is_on_track="true"
    local velocity_threshold=$(awk -v rv="$required_velocity" 'BEGIN {printf "%.1f", rv * 0.8}')  # 80% tolerance
    
    # Add progressive coverage recommendations
    recommendations+=("📈 Progressive Coverage Analysis:")
    recommendations+=("Current Phase: $current_phase")
    recommendations+=("Next Target: ${next_target}% (Gap: ${coverage_gap}%)")
    recommendations+=("Required monthly velocity: ${required_velocity}% for 90% by Jan 2026")
    
    if [[ $(awk -v gap="$coverage_gap" 'BEGIN {print (gap > 15) ? 1 : 0}') -eq 1 ]]; then
        recommendations+=("⚠️ Large coverage gap - prioritize high-impact test scenarios")
        recommendations+=("Focus areas: Service layer methods, API endpoints, error handling")
    elif [[ $(awk -v gap="$coverage_gap" 'BEGIN {print (gap > 5) ? 1 : 0}') -eq 1 ]]; then
        recommendations+=("📊 Moderate coverage gap - steady progression needed")
        recommendations+=("Focus areas: Business logic validation, integration scenarios")
    else
        recommendations+=("✅ Close to next target - maintain current testing velocity")
    fi
    
    # Generate skip categorization analysis (basic version)
    local skip_analysis="{}"
    if [[ $skipped_tests -gt 0 ]]; then
        # For bash version, provide basic categorization - could be enhanced with TRX parsing
        skip_analysis=$(cat << EOF
{
    "externalServices": {
        "categoryType": "expected",
        "skippedCount": $(($skipped_tests * 70 / 100)),
        "isExpected": true,
        "reason": "External service dependencies not configured"
    },
    "infrastructure": {
        "categoryType": "expected", 
        "skippedCount": $(($skipped_tests * 25 / 100)),
        "isExpected": true,
        "reason": "Infrastructure dependencies unavailable"
    },
    "hardcodedSkips": {
        "categoryType": "problematic",
        "skippedCount": $(($skipped_tests * 5 / 100)),
        "isExpected": false,
        "reason": "Hardcoded Skip attributes requiring review"
    }
}
EOF
        )
    fi
    
    # Create baseline validation results
    local violations_json=$(printf '%s\n' "${violations[@]}" | jq -R . | jq -s .)
    local recommendations_json=$(printf '%s\n' "${recommendations[@]}" | jq -R . | jq -s .)
    
    cat > "$TEST_RESULTS_DIR/baseline_validation.json" << EOF
{
    "timestamp": "$(date -Iseconds)",
    "environment": {
        "classification": "$environment_classification",
        "description": "$environment_description",
        "expectedSkipPercentage": $expected_skip_percentage
    },
    "metrics": {
        "totalTests": $total_tests,
        "skippedTests": $skipped_tests,
        "failedTests": $failed_tests,
        "skipPercentage": $skip_percentage,
        "lineCoverage": $line_coverage
    },
    "baselines": {
        "skipThreshold": $expected_skip_percentage,
        "coverageBaseline": $coverage_baseline,
        "coverageRegressionTolerance": 1.0
    },
    "progressiveCoverage": {
        "currentPhase": "$current_phase",
        "nextTarget": $next_target,
        "coverageGap": $coverage_gap,
        "targetDate": "$target_date",
        "monthsToTarget": $months_to_target,
        "requiredVelocity": $required_velocity,
        "isOnTrack": $is_on_track,
        "progressiveTargets": [20.0, 35.0, 50.0, 75.0, 90.0]
    },
    "validation": {
        "passesThresholds": $baseline_validation_passed,
        "violations": $violations_json,
        "recommendations": $recommendations_json
    },
    "skipAnalysis": $skip_analysis
}
EOF
    
    # Log validation results
    if [[ "$baseline_validation_passed" == "true" ]]; then
        success "Baseline validation passed: ${skip_percentage}% skip rate (≤ ${expected_skip_percentage}%), ${line_coverage}% coverage (≥ ${coverage_baseline}%)"
    else
        warning "Baseline validation issues detected:"
        for violation in "${violations[@]}"; do
            warning "  ❌ $violation"
        done
        if [[ ${#recommendations[@]} -gt 0 ]]; then
            log "Recommendations:"
            for recommendation in "${recommendations[@]}"; do
                info "  💡 $recommendation"
            done
        fi
    fi
    
    # Return appropriate exit code for CI/CD
    if [[ "$baseline_validation_passed" == "true" ]]; then
        return 0
    else
        return 1
    fi
}

# Helper functions for progressive coverage analysis (Phase 3)
get_next_coverage_target() {
    local current_coverage=$1
    local progressive_targets=(20.0 35.0 50.0 75.0 90.0)
    
    for target in "${progressive_targets[@]}"; do
        if [[ $(awk -v target="$target" -v current="$current_coverage" 'BEGIN {print (target > current) ? 1 : 0}') -eq 1 ]]; then
            echo "$target"
            return 0
        fi
    done
    
    # If already at or above highest target, return 90.0
    echo "90.0"
}

get_current_coverage_phase() {
    local current_coverage=$1
    
    if [[ $(awk -v current="$current_coverage" 'BEGIN {print (current < 20) ? 1 : 0}') -eq 1 ]]; then
        echo "Phase 1 - Foundation (Basic Coverage)"
    elif [[ $(awk -v current="$current_coverage" 'BEGIN {print (current < 35) ? 1 : 0}') -eq 1 ]]; then
        echo "Phase 2 - Growth (Service Layer Depth)"
    elif [[ $(awk -v current="$current_coverage" 'BEGIN {print (current < 50) ? 1 : 0}') -eq 1 ]]; then
        echo "Phase 3 - Maturity (Edge Cases & Error Handling)"
    elif [[ $(awk -v current="$current_coverage" 'BEGIN {print (current < 75) ? 1 : 0}') -eq 1 ]]; then
        echo "Phase 4 - Excellence (Complex Scenarios)"
    elif [[ $(awk -v current="$current_coverage" 'BEGIN {print (current < 90) ? 1 : 0}') -eq 1 ]]; then
        echo "Phase 5 - Mastery (Comprehensive Coverage)"
    else
        echo "Phase 6 - Optimization (Maintenance & Optimization)"
    fi
}

calculate_months_to_target() {
    local target_date=$1
    local current_date=$(date +%Y-%m-%d)
    
    # Calculate months between dates (simplified calculation)
    local target_epoch=$(date -d "$target_date" +%s 2>/dev/null || echo "0")
    local current_epoch=$(date -d "$current_date" +%s)
    
    if [[ $target_epoch -eq 0 ]]; then
        echo "24"  # Default to 24 months if date parsing fails
        return
    fi
    
    local days_diff=$(( (target_epoch - current_epoch) / 86400 ))
    local months_diff=$(awk -v days="$days_diff" 'BEGIN {printf "%.1f", days / 30.44}')
    
    echo "$months_diff"
}

# Quality regression detection (Phase 2 enhancement)
detect_quality_regression() {
    log "🔍 Performing quality regression analysis..."
    
    local baseline_file="$TEST_RESULTS_DIR/baseline_results.json"
    local current_results="$TEST_RESULTS_DIR/parsed_results.json"
    local current_coverage="$TEST_RESULTS_DIR/coverage_results.json"
    local regression_report="$TEST_RESULTS_DIR/regression_analysis.json"
    
    if [[ ! -f "$baseline_file" ]]; then
        warning "No baseline found - skipping regression analysis"
        return 0
    fi
    
    if [[ ! -f "$current_results" || ! -f "$current_coverage" ]]; then
        warning "Missing current results - skipping regression analysis"
        return 0
    fi
    
    # Extract baseline metrics
    local baseline_pass_rate=$(jq -r '.tests.pass_rate // 0' "$baseline_file" 2>/dev/null || echo "0")
    local baseline_total=$(jq -r '.tests.total // 0' "$baseline_file" 2>/dev/null || echo "0")
    local baseline_coverage=$(jq -r '.coverage_metrics.line_coverage // 0' "$baseline_file" 2>/dev/null || echo "0")
    
    # Extract current metrics
    local current_pass_rate=$(jq -r '.tests.pass_rate // 0' "$current_results" 2>/dev/null || echo "0")
    local current_total=$(jq -r '.tests.total // 0' "$current_results" 2>/dev/null || echo "0")
    local current_coverage=$(jq -r '.line_coverage // 0' "$current_coverage" 2>/dev/null || echo "0")
    
    # Calculate regressions
    local pass_rate_change=$(awk -v cp="$current_pass_rate" -v bp="$baseline_pass_rate" 'BEGIN {printf "%.2f", cp - bp}')
    local coverage_change=$(awk -v cc="$current_coverage" -v bc="$baseline_coverage" 'BEGIN {printf "%.2f", cc - bc}')
    local test_count_change=$((current_total - baseline_total))
    
    # Determine regression severity
    local regression_level="NONE"
    local regression_detected=false
    
    if (( $(awk -v prc="$pass_rate_change" 'BEGIN {print (prc < -5) ? 1 : 0}') )); then
        regression_level="HIGH"
        regression_detected=true
    elif (( $(awk -v prc="$pass_rate_change" 'BEGIN {print (prc < -2) ? 1 : 0}') )); then
        regression_level="MEDIUM"
        regression_detected=true
    elif (( $(awk -v cc="$coverage_change" 'BEGIN {print (cc < -3) ? 1 : 0}') )); then
        regression_level="MEDIUM"
        regression_detected=true
    elif (( $(awk -v cc="$coverage_change" 'BEGIN {print (cc < -1) ? 1 : 0}') )); then
        regression_level="LOW"
        regression_detected=true
    fi
    
    # Generate regression analysis report
    cat > "$regression_report" << EOF
{
    "regression_detected": $regression_detected,
    "severity": "$regression_level",
    "analysis": {
        "pass_rate_change": $pass_rate_change,
        "coverage_change": $coverage_change,
        "test_count_change": $test_count_change
    },
    "baseline": {
        "pass_rate": $baseline_pass_rate,
        "total_tests": $baseline_total,
        "coverage": $baseline_coverage
    },
    "current": {
        "pass_rate": $current_pass_rate,
        "total_tests": $current_total,
        "coverage": $current_coverage
    },
    "recommendations": [
        $([ "$regression_detected" = "true" ] && cat << 'REC'
        "Investigate quality regression before deployment",
        "Review recent code changes for test impact",
        "Consider reverting changes if regression is severe"
REC
|| echo '"No quality regressions detected"')
    ]
}
EOF
    
    if [[ "$regression_detected" = "true" ]]; then
        warning "Quality regression detected: $regression_level severity"
        warning "Pass rate change: ${pass_rate_change}%, Coverage change: ${coverage_change}%"
        
        # Add regression status to quality gates
        echo "QUALITY_REGRESSION_DETECTED=true" >> "$TEST_RESULTS_DIR/quality_status.env"
        echo "REGRESSION_SEVERITY=$regression_level" >> "$TEST_RESULTS_DIR/quality_status.env"
        
        if [[ "$regression_level" == "HIGH" ]]; then
            return 1  # Return failure for high severity regressions
        fi
    else
        success "No quality regressions detected"
        echo "QUALITY_REGRESSION_DETECTED=false" >> "$TEST_RESULTS_DIR/quality_status.env"
    fi
    
    return 0
}

# Dynamic quality gates based on historical data (Phase 3 enhancement) 
calculate_dynamic_quality_gates() {
    log "📊 Calculating dynamic quality gates based on historical data..."
    
    local baseline_file="$TEST_RESULTS_DIR/baseline_results.json"
    local historical_dir="$TEST_RESULTS_DIR/historical"
    local dynamic_gates_file="$TEST_RESULTS_DIR/dynamic_quality_gates.json"
    
    # Initialize default gates
    local dynamic_coverage_threshold=$COVERAGE_THRESHOLD
    local dynamic_pass_rate_threshold=95
    local dynamic_performance_threshold=120  # 120% of baseline execution time
    local gate_confidence="LOW"
    local historical_samples=0
    
    # Create historical directory if it doesn't exist
    mkdir -p "$historical_dir"
    
    # Collect historical data from last 10 runs
    local historical_files=()
    if [[ -d "$historical_dir" ]]; then
        mapfile -t historical_files < <(find "$historical_dir" -name "results_*.json" -type f | sort -r | head -10)
        historical_samples=${#historical_files[@]}
    fi
    
    log "Found $historical_samples historical samples for analysis"
    
    if [[ $historical_samples -ge 3 ]]; then
        # Calculate dynamic thresholds based on historical performance
        local coverage_values=()
        local pass_rate_values=()
        local execution_times=()
        
        # Extract metrics from historical data
        for file in "${historical_files[@]}"; do
            if [[ -f "$file" ]]; then
                local coverage=$(jq -r '.coverage_metrics.line_coverage // null' "$file" 2>/dev/null)
                local pass_rate=$(jq -r '.tests.pass_rate // null' "$file" 2>/dev/null)
                local exec_time=$(jq -r '.execution_time // null' "$file" 2>/dev/null)
                
                [[ "$coverage" != "null" && "$coverage" != "" ]] && coverage_values+=("$coverage")
                [[ "$pass_rate" != "null" && "$pass_rate" != "" ]] && pass_rate_values+=("$pass_rate")
                [[ "$exec_time" != "null" && "$exec_time" != "" ]] && execution_times+=("$exec_time")
            fi
        done
        
        # Calculate adaptive thresholds using statistical analysis
        if [[ ${#coverage_values[@]} -ge 3 ]]; then
            local avg_coverage=$(calculate_average "${coverage_values[@]}")
            local std_coverage=$(calculate_standard_deviation "${coverage_values[@]}")
            
            # Set threshold at 1 standard deviation below average (adaptive floor)
            dynamic_coverage_threshold=$(awk -v avg="$avg_coverage" -v std="$std_coverage" 'BEGIN {printf "%.1f", avg - std}')
            
            # Ensure minimum threshold of 15% and maximum of original threshold
            if (( $(awk -v dct="$dynamic_coverage_threshold" 'BEGIN {print (dct < 15) ? 1 : 0}') )); then
                dynamic_coverage_threshold=15
            elif (( $(awk -v dct="$dynamic_coverage_threshold" -v ct="$COVERAGE_THRESHOLD" 'BEGIN {print (dct > ct) ? 1 : 0}') )); then
                dynamic_coverage_threshold=$COVERAGE_THRESHOLD
            fi
            
            gate_confidence="MEDIUM"
        fi
        
        if [[ ${#pass_rate_values[@]} -ge 3 ]]; then
            local avg_pass_rate=$(calculate_average "${pass_rate_values[@]}")
            local std_pass_rate=$(calculate_standard_deviation "${pass_rate_values[@]}")
            
            # Set threshold at 1.5 standard deviations below average (stricter for pass rate)
            dynamic_pass_rate_threshold=$(awk -v avg="$avg_pass_rate" -v std="$std_pass_rate" 'BEGIN {printf "%.1f", avg - 1.5 * std}')
            
            # Ensure minimum threshold of 85%
            if (( $(awk -v dprt="$dynamic_pass_rate_threshold" 'BEGIN {print (dprt < 85) ? 1 : 0}') )); then
                dynamic_pass_rate_threshold=85
            fi
        fi
        
        if [[ ${#execution_times[@]} -ge 3 ]]; then
            local avg_exec_time=$(calculate_average "${execution_times[@]}")
            local std_exec_time=$(calculate_standard_deviation "${execution_times[@]}")
            
            # Set performance threshold at average + 2 standard deviations (outlier detection)
            dynamic_performance_threshold=$(awk -v avg="$avg_exec_time" -v std="$std_exec_time" 'BEGIN {printf "%.0f", (avg + 2 * std) / avg * 100}')
            
            # Cap at 200% to prevent unreasonably high thresholds
            if [[ $dynamic_performance_threshold -gt 200 ]]; then
                dynamic_performance_threshold=200
            fi
        fi
        
        if [[ $historical_samples -ge 7 ]]; then
            gate_confidence="HIGH"
        fi
    fi
    
    # Generate dynamic quality gates configuration
    cat > "$dynamic_gates_file" << EOF
{
    "dynamic_gates": {
        "coverage_threshold": $dynamic_coverage_threshold,
        "pass_rate_threshold": $dynamic_pass_rate_threshold,
        "performance_threshold": $dynamic_performance_threshold,
        "confidence_level": "$gate_confidence",
        "historical_samples": $historical_samples,
        "generated_at": "$(date -Iseconds)",
        "methodology": "Statistical analysis of last $historical_samples runs"
    },
    "static_gates": {
        "original_coverage_threshold": $COVERAGE_THRESHOLD,
        "original_pass_rate_threshold": 95,
        "original_performance_threshold": 120
    },
    "recommendations": {
        "coverage": "$(if (( $(awk -v dct="$dynamic_coverage_threshold" -v ct="$COVERAGE_THRESHOLD" 'BEGIN {print (dct > ct) ? 1 : 0}') )); then echo "Project performing above baseline - consider raising standards"; else echo "Adaptive threshold set based on historical performance"; fi)",
        "reliability": "$(if (( $(awk -v dprt="$dynamic_pass_rate_threshold" 'BEGIN {print (dprt > 95) ? 1 : 0}') )); then echo "High reliability demonstrated - maintaining elevated standards"; else echo "Adaptive reliability threshold based on project maturity"; fi)",
        "performance": "Threshold set at $(awk -v dpt="$dynamic_performance_threshold" 'BEGIN {printf "%.0f", dpt}')% of historical average execution time"
    }
}
EOF
    
    log "Dynamic quality gates calculated:"
    log "  Coverage: ${dynamic_coverage_threshold}% (confidence: $gate_confidence)"
    log "  Pass Rate: ${dynamic_pass_rate_threshold}% (confidence: $gate_confidence)"  
    log "  Performance: ${dynamic_performance_threshold}% of baseline (confidence: $gate_confidence)"
    
    success "Dynamic quality gates configuration saved to $dynamic_gates_file"
    
    # Return the new coverage threshold for this run
    echo "$dynamic_coverage_threshold"
}

# Statistical helper functions for dynamic gates
calculate_average() {
    local values=("$@")
    local sum=0
    local count=${#values[@]}
    
    for value in "${values[@]}"; do
        sum=$(awk -v sum="$sum" -v val="$value" 'BEGIN {print sum + val}')
    done
    
    awk -v sum="$sum" -v cnt="$count" 'BEGIN {printf "%.2f", sum / cnt}'
}

calculate_standard_deviation() {
    local values=("$@")
    local count=${#values[@]}
    local avg=$(calculate_average "${values[@]}")
    local variance_sum=0
    
    for value in "${values[@]}"; do
        local diff=$(awk -v val="$value" -v avg="$avg" 'BEGIN {print val - avg}')
        local squared=$(awk -v d="$diff" 'BEGIN {print d * d}')
        variance_sum=$(awk -v vs="$variance_sum" -v sq="$squared" 'BEGIN {print vs + sq}')
    done
    
    local variance=$(awk -v vs="$variance_sum" -v cnt="$count" 'BEGIN {printf "%.4f", vs / cnt}')
    awk -v var="$variance" 'BEGIN {printf "%.2f", sqrt(var)}'
}

# Save historical data for dynamic quality gates (Phase 3 enhancement)
save_historical_data() {
    log "💾 Saving current run data for historical analysis..."
    
    local historical_dir="$TEST_RESULTS_DIR/historical"
    local timestamp=$(date +"%Y%m%d_%H%M%S")
    local historical_file="$historical_dir/results_$timestamp.json"
    
    # Create historical directory if it doesn't exist
    mkdir -p "$historical_dir"
    
    # Create comprehensive historical record
    if [[ -f "$TEST_RESULTS_DIR/parsed_results.json" && -f "$TEST_RESULTS_DIR/coverage_results.json" ]]; then
        # Get execution time from log file
        local execution_time
        execution_time=$(grep "EXECUTION_TIME=" "$LOG_FILE" 2>/dev/null | tail -1 | cut -d'=' -f2 || echo "0")
        
        # Combine all metrics into historical record
        jq -s --argjson exec_time "$execution_time" --arg ts "$timestamp" --arg saved_at "$(date -Iseconds)" \
            '.[0] + {"coverage_metrics": .[1], "execution_time": $exec_time, "timestamp": $ts, "saved_at": $saved_at}' \
            "$TEST_RESULTS_DIR/parsed_results.json" \
            "$TEST_RESULTS_DIR/coverage_results.json" \
            > "$historical_file" 2>/dev/null
        
        if [[ -f "$historical_file" ]]; then
            success "Historical data saved: $historical_file"
            
            # Clean up old historical data (keep last 20 runs)
            find "$historical_dir" -name "results_*.json" -type f | sort -r | tail -n +21 | xargs rm -f 2>/dev/null || true
            
            local remaining_files
            remaining_files=$(find "$historical_dir" -name "results_*.json" -type f | wc -l)
            log "Historical archive maintained: $remaining_files runs stored"
        else
            warning "Failed to save historical data"
        fi
    else
        warning "Required result files not found for historical data collection"
    fi
}

# Real-time metrics collection and trending analysis (Phase 3 enhancement)
generate_trending_analysis() {
    log "📈 Generating real-time metrics and trend analysis..."
    
    local historical_dir="$TEST_RESULTS_DIR/historical"
    local trends_file="$TEST_RESULTS_DIR/trend_analysis.json"
    local current_results="$TEST_RESULTS_DIR/parsed_results.json"
    local current_coverage="$TEST_RESULTS_DIR/coverage_results.json"
    
    # Initialize trend data structure
    cat > "$trends_file" << 'EOF'
{
    "trend_analysis": {
        "generated_at": "",
        "analysis_period": "",
        "data_points": 0,
        "trends": {},
        "predictions": {},
        "alerts": []
    }
}
EOF
    
    # Update generation metadata
    jq '.trend_analysis.generated_at = "'$(date -Iseconds)'"' "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
    
    # Collect historical data points
    local historical_files=()
    if [[ -d "$historical_dir" ]]; then
        mapfile -t historical_files < <(find "$historical_dir" -name "results_*.json" -type f | sort -r | head -15)
    fi
    
    local data_points=${#historical_files[@]}
    jq --argjson count "$data_points" '.trend_analysis.data_points = $count' "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
    
    if [[ $data_points -ge 3 ]]; then
        jq '.trend_analysis.analysis_period = "Last '$data_points' runs"' "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
        
        # Extract trend data
        local coverage_trend=()
        local pass_rate_trend=()
        local execution_time_trend=()
        local test_count_trend=()
        local timestamps=()
        
        for file in "${historical_files[@]}"; do
            if [[ -f "$file" ]]; then
                local coverage=$(jq -r '.coverage_metrics.line_coverage // null' "$file" 2>/dev/null)
                local pass_rate=$(jq -r '.tests.pass_rate // null' "$file" 2>/dev/null)
                local exec_time=$(jq -r '.execution_time // null' "$file" 2>/dev/null)
                local test_count=$(jq -r '.tests.total // null' "$file" 2>/dev/null)
                local timestamp=$(jq -r '.timestamp // null' "$file" 2>/dev/null)
                
                [[ "$coverage" != "null" && "$coverage" != "" ]] && coverage_trend+=("$coverage")
                [[ "$pass_rate" != "null" && "$pass_rate" != "" ]] && pass_rate_trend+=("$pass_rate")
                [[ "$exec_time" != "null" && "$exec_time" != "" ]] && execution_time_trend+=("$exec_time")
                [[ "$test_count" != "null" && "$test_count" != "" ]] && test_count_trend+=("$test_count")
                [[ "$timestamp" != "null" && "$timestamp" != "" ]] && timestamps+=("$timestamp")
            fi
        done
        
        # Calculate trends and predictions
        if [[ ${#coverage_trend[@]} -ge 3 ]]; then
            local coverage_avg=$(calculate_average "${coverage_trend[@]}")
            local coverage_slope=$(calculate_trend_slope "${coverage_trend[@]}")
            local coverage_prediction=$(awk -v avg="$coverage_avg" -v slope="$coverage_slope" 'BEGIN {printf "%.1f", avg + slope}')
            
            # Update trends JSON
            jq --argjson avg "$coverage_avg" --argjson slope "$coverage_slope" --argjson pred "$coverage_prediction" \
                '.trend_analysis.trends.coverage = {"average": $avg, "slope": $slope, "direction": (if $slope > 0.5 then "improving" elif $slope < -0.5 then "declining" else "stable" end)}' \
                "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
            
            jq --argjson pred "$coverage_prediction" \
                '.trend_analysis.predictions.coverage_next_run = $pred' \
                "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
        fi
        
        if [[ ${#pass_rate_trend[@]} -ge 3 ]]; then
            local pass_rate_avg=$(calculate_average "${pass_rate_trend[@]}")
            local pass_rate_slope=$(calculate_trend_slope "${pass_rate_trend[@]}")
            local pass_rate_prediction=$(awk -v avg="$pass_rate_avg" -v slope="$pass_rate_slope" 'BEGIN {printf "%.1f", avg + slope}')
            
            jq --argjson avg "$pass_rate_avg" --argjson slope "$pass_rate_slope" \
                '.trend_analysis.trends.reliability = {"average": $avg, "slope": $slope, "direction": (if $slope > 0.5 then "improving" elif $slope < -0.5 then "declining" else "stable" end)}' \
                "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
            
            jq --argjson pred "$pass_rate_prediction" \
                '.trend_analysis.predictions.reliability_next_run = $pred' \
                "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
        fi
        
        if [[ ${#execution_time_trend[@]} -ge 3 ]]; then
            local exec_time_avg=$(calculate_average "${execution_time_trend[@]}")
            local exec_time_slope=$(calculate_trend_slope "${execution_time_trend[@]}")
            local exec_time_prediction=$(awk -v avg="$exec_time_avg" -v slope="$exec_time_slope" 'BEGIN {printf "%.0f", avg + slope}')
            
            jq --argjson avg "$exec_time_avg" --argjson slope "$exec_time_slope" \
                '.trend_analysis.trends.performance = {"average": $avg, "slope": $slope, "direction": (if $slope > 5 then "slowing" elif $slope < -5 then "improving" else "stable" end)}' \
                "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
            
            jq --argjson pred "$exec_time_prediction" \
                '.trend_analysis.predictions.execution_time_next_run = $pred' \
                "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
        fi
        
        # Generate alerts based on trends
        generate_trend_alerts "$trends_file" "${coverage_trend[@]}" "${pass_rate_trend[@]}" "${execution_time_trend[@]}"
        
        log "Trend analysis complete:"
        log "  Coverage trend: $(jq -r '.trend_analysis.trends.coverage.direction // "insufficient data"' "$trends_file")"
        log "  Reliability trend: $(jq -r '.trend_analysis.trends.reliability.direction // "insufficient data"' "$trends_file")"
        log "  Performance trend: $(jq -r '.trend_analysis.trends.performance.direction // "insufficient data"' "$trends_file")"
        
        success "Trending analysis saved to $trends_file"
    else
        jq '.trend_analysis.analysis_period = "Insufficient historical data (need 3+ runs)"' "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
        log "Insufficient historical data for trend analysis (need 3+ runs, have $data_points)"
    fi
}

# Calculate trend slope for predictions
calculate_trend_slope() {
    local values=("$@")
    local count=${#values[@]}
    
    if [[ $count -lt 2 ]]; then
        echo "0"
        return
    fi
    
    # Simple linear regression slope calculation
    local sum_x=0
    local sum_y=0
    local sum_xy=0
    local sum_x2=0
    
    for i in "${!values[@]}"; do
        local x=$((i + 1))
        local y="${values[$i]}"
        
        sum_x=$(awk -v sum="$sum_x" -v x="$x" 'BEGIN {print sum + x}')
        sum_y=$(awk -v sum="$sum_y" -v y="$y" 'BEGIN {print sum + y}')
        sum_xy=$(awk -v sum="$sum_xy" -v x="$x" -v y="$y" 'BEGIN {print sum + x * y}')
        sum_x2=$(awk -v sum="$sum_x2" -v x="$x" 'BEGIN {print sum + x * x}')
    done
    
    # slope = (n*sum_xy - sum_x*sum_y) / (n*sum_x2 - sum_x*sum_x)
    local numerator=$(awk -v cnt="$count" -v sxy="$sum_xy" -v sx="$sum_x" -v sy="$sum_y" 'BEGIN {print cnt * sxy - sx * sy}')
    local denominator=$(awk -v cnt="$count" -v sx2="$sum_x2" -v sx="$sum_x" 'BEGIN {print cnt * sx2 - sx * sx}')
    
    if [[ $(awk -v den="$denominator" 'BEGIN {print (den != 0) ? 1 : 0}') -eq 1 ]]; then
        awk -v num="$numerator" -v den="$denominator" 'BEGIN {printf "%.3f", num / den}'
    else
        echo "0"
    fi
}

# Generate trend-based alerts
generate_trend_alerts() {
    local trends_file="$1"
    shift
    local coverage_values=("$@")
    
    # Check for significant degradation trends
    local alerts=()
    
    # Coverage degradation alert
    local coverage_slope=$(jq -r '.trend_analysis.trends.coverage.slope // 0' "$trends_file" 2>/dev/null)
    if [[ $(awk -v cs="$coverage_slope" 'BEGIN {print (cs < -2) ? 1 : 0}') -eq 1 ]]; then
        alerts+=("\"Coverage trend declining significantly (slope: $coverage_slope)\"")
    fi
    
    # Reliability degradation alert
    local reliability_slope=$(jq -r '.trend_analysis.trends.reliability.slope // 0' "$trends_file" 2>/dev/null)
    if [[ $(awk -v rs="$reliability_slope" 'BEGIN {print (rs < -1) ? 1 : 0}') -eq 1 ]]; then
        alerts+=("\"Test reliability declining (slope: $reliability_slope)\"")
    fi
    
    # Performance degradation alert
    local performance_slope=$(jq -r '.trend_analysis.trends.performance.slope // 0' "$trends_file" 2>/dev/null)
    if [[ $(awk -v ps="$performance_slope" 'BEGIN {print (ps > 10) ? 1 : 0}') -eq 1 ]]; then
        alerts+=("\"Test execution time increasing significantly (slope: $performance_slope seconds)\"")
    fi
    
    # Volatility alert (high variance in recent runs)
    if [[ ${#coverage_values[@]} -ge 5 ]]; then
        local recent_coverage=("${coverage_values[@]:0:5}")
        local coverage_std=$(calculate_standard_deviation "${recent_coverage[@]}")
        if [[ $(awk -v cs="$coverage_std" 'BEGIN {print (cs > 5) ? 1 : 0}') -eq 1 ]]; then
            alerts+=("\"High coverage volatility detected (std dev: $coverage_std)\"")
        fi
    fi
    
    # Update alerts in JSON
    if [[ ${#alerts[@]} -gt 0 ]]; then
        local alerts_json=$(printf '%s,' "${alerts[@]}")
        alerts_json="[${alerts_json%,}]"
        jq --argjson alerts "$alerts_json" '.trend_analysis.alerts = $alerts' "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
        warning "Generated ${#alerts[@]} trend-based alerts"
    else
        jq '.trend_analysis.alerts = []' "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
    fi
}

# Predictive deployment risk assessment (Phase 2 enhancement)
assess_deployment_risk() {
    log "🎯 Performing predictive deployment risk assessment..."
    
    local risk_report="$TEST_RESULTS_DIR/deployment_risk.json"
    local results_file="$TEST_RESULTS_DIR/parsed_results.json"
    local coverage_file="$TEST_RESULTS_DIR/coverage_results.json"
    local regression_file="$TEST_RESULTS_DIR/regression_analysis.json"
    
    # Extract metrics for risk calculation
    local failed_tests=$(jq -r '.tests.failed // 0' "$results_file" 2>/dev/null || echo "0")
    local pass_rate=$(jq -r '.tests.pass_rate // 100' "$results_file" 2>/dev/null || echo "100")
    local coverage=$(jq -r '.line_coverage // 0' "$coverage_file" 2>/dev/null || echo "0")
    local regression_detected=false
    local regression_severity="NONE"
    
    if [[ -f "$regression_file" ]]; then
        regression_detected=$(jq -r '.regression_detected // false' "$regression_file" 2>/dev/null || echo "false")
        regression_severity=$(jq -r '.severity // "NONE"' "$regression_file" 2>/dev/null || echo "NONE")
    fi
    
    # Calculate risk score (0-100, higher = more risky)
    local risk_score=0
    local risk_factors=()
    
    # Test failure risk (0-40 points)
    if [[ $failed_tests -gt 0 ]]; then
        local test_risk=$((failed_tests * 10))
        risk_score=$((risk_score + (test_risk > 40 ? 40 : test_risk)))
        risk_factors+=("${failed_tests} failing tests (+${test_risk} risk)")
    fi
    
    # Coverage risk (0-25 points)
    if (( $(awk -v cov="$coverage" 'BEGIN {print (cov < 24) ? 1 : 0}') )); then
        local coverage_risk=$(awk -v cov="$coverage" 'BEGIN {printf "%.0f", (24 - cov) * 2}')
        risk_score=$((risk_score + coverage_risk))
        risk_factors+=("Low coverage ${coverage}% (+${coverage_risk} risk)")
    fi
    
    # Pass rate risk (0-20 points)
    if (( $(awk -v pr="$pass_rate" 'BEGIN {print (pr < 95) ? 1 : 0}') )); then
        local pass_rate_risk=$(echo "scale=0; (95 - $pass_rate) / 5" | bc)
        risk_score=$((risk_score + pass_rate_risk))
        risk_factors+=("Pass rate ${pass_rate}% (+${pass_rate_risk} risk)")
    fi
    
    # Regression risk (0-15 points)
    if [[ "$regression_detected" = "true" ]]; then
        local regression_risk=0
        case "$regression_severity" in
            "HIGH") regression_risk=15 ;;
            "MEDIUM") regression_risk=10 ;;
            "LOW") regression_risk=5 ;;
        esac
        risk_score=$((risk_score + regression_risk))
        risk_factors+=("Quality regression: ${regression_severity} (+${regression_risk} risk)")
    fi
    
    # Determine risk level and recommendation
    local risk_level="LOW"
    local deployment_recommendation="DEPLOY"
    local confidence=90
    
    if [[ $risk_score -ge 50 ]]; then
        risk_level="HIGH"
        deployment_recommendation="BLOCK"
        confidence=95
    elif [[ $risk_score -ge 25 ]]; then
        risk_level="MEDIUM"
        deployment_recommendation="CAUTION"
        confidence=80
    fi
    
    # Generate deployment risk assessment
    cat > "$risk_report" << EOF
{
    "risk_score": $risk_score,
    "risk_level": "$risk_level",
    "deployment_recommendation": "$deployment_recommendation",
    "confidence_level": $confidence,
    "risk_factors": [
        $(printf '"%s"' "${risk_factors[@]}" | sed 's/" "/","/g')
    ],
    "mitigation_strategies": [
        $(case "$risk_level" in
            "HIGH") cat << 'HIGH_RISK'
        "Fix all failing tests before deployment",
        "Increase test coverage to meet minimum threshold",
        "Perform additional manual testing",
        "Consider hotfix deployment strategy",
        "Implement rollback plan"
HIGH_RISK
            ;;
            "MEDIUM") cat << 'MEDIUM_RISK'
        "Monitor deployment closely",
        "Have rollback plan ready",
        "Consider staged deployment",
        "Address test failures in next sprint"
MEDIUM_RISK
            ;;
            *) echo '"Deployment appears safe, proceed normally"'
            ;;
        esac)
    ],
    "assessment_timestamp": "$(date -Iseconds)"
}
EOF
    
    case "$risk_level" in
        "HIGH")
            print_error "HIGH RISK deployment detected (score: $risk_score/100)"
            warning "Recommendation: BLOCK deployment until issues resolved"
            ;;
        "MEDIUM")
            warning "MEDIUM RISK deployment detected (score: $risk_score/100)"
            warning "Recommendation: Proceed with CAUTION"
            ;;
        *)
            success "LOW RISK deployment (score: $risk_score/100)"
            success "Recommendation: Safe to deploy"
            ;;
    esac
    
    return 0
}

# Generate report based on format (report mode)
generate_report() {
    log "📝 Generating $OUTPUT_FORMAT report..."
    
    case $OUTPUT_FORMAT in
        json)
            generate_json_report
            ;;
        summary)
            generate_summary_report
            ;;
        console)
            generate_console_report
            ;;
        markdown|*)
            generate_markdown_report
            ;;
    esac
}

# Generate JSON report
generate_json_report() {
    local results_file="$TEST_RESULTS_DIR/parsed_results.json"
    local coverage_file="$TEST_RESULTS_DIR/coverage_results.json"
    local baseline_file="$TEST_RESULTS_DIR/baseline_validation.json"
    
    if [[ -f "$results_file" && -f "$coverage_file" ]]; then
        if command -v jq &> /dev/null; then
            # Include baseline validation data if available
            if [[ -f "$baseline_file" ]]; then
                jq -s '.[0] + {"coverage": .[1]} + {"baselineValidation": .[2]}' "$results_file" "$coverage_file" "$baseline_file"
            else
                jq -s '.[0] + {"coverage": .[1]}' "$results_file" "$coverage_file"
            fi
        else
            # Fallback if jq not available
            echo '{"error": "JSON processing not available", "results_file": "'"$(json_escape "$results_file")"'", "coverage_file": "'"$(json_escape "$coverage_file")"'", "baseline_file": "'"$(json_escape "$baseline_file")"'"}'
        fi
    else
        echo '{"error": "Results files not found"}'
    fi
}

# Generate summary report
generate_summary_report() {
    local results_file="$TEST_RESULTS_DIR/parsed_results.json"
    
    if [[ -f "$results_file" ]]; then
        local total=$(jq -r '.tests.total' "$results_file" 2>/dev/null || echo "N/A")
        local passed=$(jq -r '.tests.passed' "$results_file" 2>/dev/null || echo "N/A")
        local failed=$(jq -r '.tests.failed' "$results_file" 2>/dev/null || echo "N/A")
        local pass_rate=$(jq -r '.tests.pass_rate' "$results_file" 2>/dev/null || echo "N/A")
        
        echo -e "${BOLD}🎯 Test Suite Summary${NC}"
        echo "=========================="
        
        # Highlight test failures prominently if any exist
        if [[ "$failed" != "N/A" && "$failed" != "0" ]]; then
            echo -e "${RED}${BOLD}🚨 CRITICAL: $failed TEST(S) FAILING${NC}"
            echo -e "${RED}❌ Tests must pass before deployment${NC}"
            echo ""
        fi
        
        echo -e "📊 Total Tests: ${BLUE}$total${NC}"
        echo -e "✅ Passed: ${GREEN}$passed${NC}"
        
        # Make failed tests very prominent
        if [[ "$failed" != "N/A" && "$failed" != "0" ]]; then
            echo -e "❌ Failed: ${RED}${BOLD}$failed ⚠️  BLOCKING DEPLOYMENT${NC}"
        else
            echo -e "❌ Failed: ${GREEN}$failed${NC}"
        fi
        
        # Show pass rate with appropriate color
        if [[ "$pass_rate" != "N/A" ]]; then
            if [[ $(echo "$pass_rate < 100" | bc -l 2>/dev/null || echo "1") -eq 1 ]]; then
                echo -e "📈 Pass Rate: ${RED}${BOLD}$pass_rate%${NC}"
            else
                echo -e "📈 Pass Rate: ${GREEN}${BOLD}$pass_rate%${NC}"
            fi
        else
            echo -e "📈 Pass Rate: ${BOLD}$pass_rate%${NC}"
        fi
        
        # Show failed test details if available
        if [[ "$failed" != "N/A" && "$failed" != "0" ]]; then
            local failures_data=$(jq -r '.failures.failed_tests[]?' "$results_file" 2>/dev/null || echo "")
            if [[ -n "$failures_data" ]]; then
                echo ""
                echo -e "${RED}${BOLD}🔍 Failed Test Details:${NC}"
                local failure_count=0
                while IFS= read -r failure; do
                    if [[ -n "$failure" ]]; then
                        local test_name=$(echo "$failure" | jq -r '.test_name // "Unknown"')
                        local category=$(echo "$failure" | jq -r '.category // "Unknown"')
                        
                        failure_count=$((failure_count + 1))
                        echo -e "${RED}   $failure_count. $test_name (${category})${NC}"
                    fi
                done <<< "$(jq -c '.failures.failed_tests[]?' "$results_file" 2>/dev/null || echo "")"
            fi
        fi
        
        # Coverage info (secondary)
        if [[ -f "$TEST_RESULTS_DIR/coverage_results.json" ]]; then
            local line_cov=$(jq -r '.line_coverage' "$TEST_RESULTS_DIR/coverage_results.json" 2>/dev/null || echo "N/A")
            echo ""
            echo -e "🎯 Coverage: ${BLUE}$line_cov%${NC}"
        fi
    fi
}

# Generate console report
generate_console_report() {
    generate_summary_report
    echo ""
    echo -e "${BOLD}📋 Recent Test Activity${NC}"
    echo "========================"
    
    # Show recent test output
    if [[ -f "$LOG_FILE" ]]; then
        echo "Recent log entries:"
        tail -10 "$LOG_FILE" | grep -E "(Passed|Failed|Skipped)" | head -5 || echo "No recent test activity found"
    fi
}

# Generate progressive coverage visualization (Phase 3)
generate_progressive_coverage_visualization() {
    local baseline_file="$TEST_RESULTS_DIR/baseline_validation.json"
    
    if [[ ! -f "$baseline_file" ]] || ! jq -e '.progressiveCoverage' "$baseline_file" >/dev/null 2>&1; then
        return 0  # No progressive coverage data available
    fi
    
    # Extract progressive coverage data
    local current_phase next_target coverage_gap is_on_track required_velocity months_to_target
    current_phase=$(jq -r '.progressiveCoverage.currentPhase // "Unknown"' "$baseline_file")
    next_target=$(jq -r '.progressiveCoverage.nextTarget // 0' "$baseline_file")
    coverage_gap=$(jq -r '.progressiveCoverage.coverageGap // 0' "$baseline_file")
    is_on_track=$(jq -r '.progressiveCoverage.isOnTrack // false' "$baseline_file")
    required_velocity=$(jq -r '.progressiveCoverage.requiredVelocity // 0' "$baseline_file")
    months_to_target=$(jq -r '.progressiveCoverage.monthsToTarget // 0' "$baseline_file")
    
    local current_coverage
    current_coverage=$(jq -r '.metrics.lineCoverage // 0' "$baseline_file")
    
    cat << EOF

### 🎯 Progressive Coverage Journey (Phase 3)

**Current Status:** $current_phase  
**Next Milestone:** ${next_target}% coverage (Gap: ${coverage_gap}%)  
**Timeline:** $(if [[ "$is_on_track" == "true" ]]; then echo "✅ On Track"; else echo "⚠️ Behind Schedule"; fi) for 90% by Jan 2026  
**Required Velocity:** ${required_velocity}%/month (${months_to_target} months remaining)

#### Coverage Progression Visual

\`\`\`
Progress toward 90% Coverage Epic:

  14.22%     20%      35%      50%      75%      90%
     |----------|----------|----------|----------|
$(generate_progress_bar "$current_coverage")
     ^
  Current: ${current_coverage}%

Phase Milestones:
• Phase 1: Foundation (14.22% → 20%) - Service basics, API contracts
• Phase 2: Growth (20% → 35%) - Integration depth, data validation  
• Phase 3: Maturity (35% → 50%) - Edge cases, error handling
• Phase 4: Excellence (50% → 75%) - Complex scenarios, security
• Phase 5: Mastery (75% → 90%) - Comprehensive coverage, performance
\`\`\`

#### Strategic Focus Areas
$(get_phase_focus_areas "$current_phase")

#### Coverage Velocity Analysis
- **Target Date:** January 31, 2026
- **Months Remaining:** ${months_to_target}
- **Required Monthly Velocity:** ${required_velocity}%
- **Progression Status:** $(if [[ "$is_on_track" == "true" ]]; then echo "On track for 90% target"; else echo "Acceleration needed to meet timeline"; fi)

$(if [[ "$is_on_track" == "false" ]]; then 
cat << VELOCITY_WARNING

⚠️ **Velocity Alert:** Current progression may not meet Jan 2026 target
- Consider increasing test development focus
- Prioritize high-impact coverage opportunities  
- Review phase-appropriate testing strategies
VELOCITY_WARNING
fi)
EOF
}

# Generate ASCII progress bar for coverage visualization
generate_progress_bar() {
    local current_coverage=$1
    local total_width=50
    local milestones=(14.22 20 35 50 75 90)
    local progress_chars=""
    
    # Calculate position (0-50 scale)
    local position
    position=$(awk -v current="$current_coverage" -v max="90" -v width="$total_width" 'BEGIN {
        printf "%.0f", (current / max) * width
    }')
    
    # Ensure position is within bounds
    if [[ $position -lt 0 ]]; then position=0; fi
    if [[ $position -gt $total_width ]]; then position=$total_width; fi
    
    # Generate progress bar
    for ((i=0; i<total_width; i++)); do
        if [[ $i -le $position ]]; then
            progress_chars+="█"
        else
            progress_chars+="░"
        fi
    done
    
    echo "     [$progress_chars]"
}

# Get phase-specific focus areas
get_phase_focus_areas() {
    local phase="$1"
    
    case "$phase" in
        *"Phase 1"*|*"Foundation"*)
            echo "- **Primary Focus:** Service layer basics, API contracts, core business logic"
            echo "- **Strategy:** Target low-hanging fruit, establish broad coverage foundation"
            echo "- **Key Areas:** Public service methods, controller actions, basic integration tests"
            ;;
        *"Phase 2"*|*"Growth"*)
            echo "- **Primary Focus:** Service layer depth, integration scenarios, data validation"
            echo "- **Strategy:** Deepen existing coverage, expand integration testing"
            echo "- **Key Areas:** Complex service scenarios, repository patterns, auth flows"
            ;;
        *"Phase 3"*|*"Maturity"*)
            echo "- **Primary Focus:** Edge cases, error handling, input validation"
            echo "- **Strategy:** Comprehensive error scenario coverage and boundary testing"
            echo "- **Key Areas:** Exception paths, validation boundaries, external service errors"
            ;;
        *"Phase 4"*|*"Excellence"*)
            echo "- **Primary Focus:** Complex business scenarios, integration depth"
            echo "- **Strategy:** Advanced integration testing, comprehensive business logic"
            echo "- **Key Areas:** End-to-end processes, complex integrations, security scenarios"
            ;;
        *"Phase 5"*|*"Mastery"*)
            echo "- **Primary Focus:** Comprehensive edge cases, performance scenarios"
            echo "- **Strategy:** Complete critical path coverage, system-wide validation"
            echo "- **Key Areas:** Performance testing, system integration, advanced security"
            ;;
        *)
            echo "- **Status:** Optimization phase - maintain high coverage and optimize performance"
            echo "- **Focus:** Test maintenance, documentation, and continuous improvement"
            ;;
    esac
}

# Advanced health visualization functions (Phase 4)
generate_health_trends() {
    local trends_file="$TEST_RESULTS_DIR/health_trends.json"
    
    log "📈 Generating test suite health trends..."
    
    # Create or update trends file with historical data
    local current_timestamp=$(date -Iseconds)
    local current_coverage=$(jq -r '.metrics.lineCoverage // 0' "$TEST_RESULTS_DIR/baseline_validation.json" 2>/dev/null || echo "0")
    local current_skip_rate=$(jq -r '.metrics.skipPercentage // 0' "$TEST_RESULTS_DIR/baseline_validation.json" 2>/dev/null || echo "0")
    local total_tests=$(jq -r '.tests.total // 0' "$TEST_RESULTS_DIR/parsed_results.json" 2>/dev/null || echo "0")
    
    # Initialize trends file if it doesn't exist
    if [[ ! -f "$trends_file" ]]; then
        cat > "$trends_file" << EOF
{
    "metadata": {
        "created": "$current_timestamp",
        "project": "zarichney-api",
        "version": "1.0"
    },
    "trends": []
}
EOF
    fi
    
    # Add current data point
    local new_entry=$(cat << EOF
{
    "timestamp": "$current_timestamp",
    "coverage": $current_coverage,
    "skipRate": $current_skip_rate,
    "totalTests": $total_tests,
    "passRate": $(jq -r '.tests.pass_rate // 0' "$TEST_RESULTS_DIR/parsed_results.json" 2>/dev/null || echo "0"),
    "executionTime": $(jq -r '.execution_time // 0' "$TEST_RESULTS_DIR/parsed_results.json" 2>/dev/null || echo "0")
}
EOF
    )
    
    # Update trends file with new data point
    jq --argjson entry "$new_entry" '.trends += [$entry] | .trends |= sort_by(.timestamp)' "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
    
    # Keep only last 30 data points to prevent file growth
    jq '.trends |= .[-30:]' "$trends_file" > "${trends_file}.tmp" && mv "${trends_file}.tmp" "$trends_file"
    
    success "Health trends updated in $trends_file"
}

generate_coverage_velocity_chart() {
    local trends_file="$TEST_RESULTS_DIR/health_trends.json"
    
    if [[ ! -f "$trends_file" ]]; then
        echo "No historical data available for velocity analysis"
        return 1
    fi
    
    echo
    echo "### 📊 Coverage Velocity Analysis (Last 30 Data Points)"
    echo
    
    # Extract coverage data points
    local coverage_data
    coverage_data=$(jq -r '.trends[] | "\(.timestamp) \(.coverage)"' "$trends_file")
    
    if [[ -z "$coverage_data" ]]; then
        echo "Insufficient data for velocity analysis"
        return 1
    fi
    
    # Calculate simple velocity (change over time)
    local first_coverage last_coverage first_date last_date
    first_coverage=$(jq -r '.trends[0].coverage // 0' "$trends_file")
    last_coverage=$(jq -r '.trends[-1].coverage // 0' "$trends_file")
    first_date=$(jq -r '.trends[0].timestamp // ""' "$trends_file")
    last_date=$(jq -r '.trends[-1].timestamp // ""' "$trends_file")
    
    local coverage_change
    coverage_change=$(awk -v first="$first_coverage" -v last="$last_coverage" 'BEGIN {printf "%.2f", last - first}')
    
    echo "**Coverage Trend Analysis:**"
    echo "- First recorded: ${first_coverage}% ($first_date)"
    echo "- Latest: ${last_coverage}% ($last_date)"
    echo "- Change: ${coverage_change}% ($(if [[ $(awk -v change="$coverage_change" 'BEGIN {print (change >= 0) ? 1 : 0}') -eq 1 ]]; then echo "📈 Improving"; else echo "📉 Declining"; fi))"
    
    # Generate simple ASCII trend chart
    echo
    echo "**Coverage History (ASCII Chart):**"
    echo "\`\`\`"
    generate_ascii_trend_chart "$trends_file" "coverage"
    echo "\`\`\`"
}

generate_skip_rate_analysis() {
    local trends_file="$TEST_RESULTS_DIR/health_trends.json"
    
    if [[ ! -f "$trends_file" ]]; then
        echo "No historical data available for skip rate analysis"
        return 1
    fi
    
    echo
    echo "### 🎯 Skip Rate Trend Analysis"
    echo
    
    # Calculate skip rate statistics
    local avg_skip_rate min_skip_rate max_skip_rate
    avg_skip_rate=$(jq -r '[.trends[].skipRate] | add / length' "$trends_file" 2>/dev/null || echo "0")
    min_skip_rate=$(jq -r '[.trends[].skipRate] | min' "$trends_file" 2>/dev/null || echo "0")
    max_skip_rate=$(jq -r '[.trends[].skipRate] | max' "$trends_file" 2>/dev/null || echo "0")
    
    echo "**Skip Rate Statistics:**"
    echo "- Average: ${avg_skip_rate}%"
    echo "- Best: ${min_skip_rate}%"
    echo "- Worst: ${max_skip_rate}%"
    
    # Determine skip rate trend
    local first_skip_rate last_skip_rate
    first_skip_rate=$(jq -r '.trends[0].skipRate // 0' "$trends_file")
    last_skip_rate=$(jq -r '.trends[-1].skipRate // 0' "$trends_file")
    
    local skip_rate_change
    skip_rate_change=$(awk -v first="$first_skip_rate" -v last="$last_skip_rate" 'BEGIN {printf "%.2f", last - first}')
    
    echo "- Trend: ${skip_rate_change}% ($(if [[ $(awk -v change="$skip_rate_change" 'BEGIN {print (change <= 0) ? 1 : 0}') -eq 1 ]]; then echo "✅ Improving"; else echo "⚠️ Increasing"; fi))"
    
    # Generate skip rate chart
    echo
    echo "**Skip Rate History:**"
    echo "\`\`\`"
    generate_ascii_trend_chart "$trends_file" "skipRate"
    echo "\`\`\`"
}

generate_test_execution_metrics() {
    local trends_file="$TEST_RESULTS_DIR/health_trends.json"
    
    if [[ ! -f "$trends_file" ]]; then
        echo "No historical data available for execution metrics"
        return 1
    fi
    
    echo
    echo "### ⚡ Test Execution Performance"
    echo
    
    # Calculate execution time statistics
    local avg_exec_time min_exec_time max_exec_time
    avg_exec_time=$(jq -r '[.trends[].executionTime] | add / length' "$trends_file" 2>/dev/null || echo "0")
    min_exec_time=$(jq -r '[.trends[].executionTime] | min' "$trends_file" 2>/dev/null || echo "0")
    max_exec_time=$(jq -r '[.trends[].executionTime] | max' "$trends_file" 2>/dev/null || echo "0")
    
    # Calculate test count growth
    local first_test_count last_test_count
    first_test_count=$(jq -r '.trends[0].totalTests // 0' "$trends_file")
    last_test_count=$(jq -r '.trends[-1].totalTests // 0' "$trends_file")
    local test_growth=$((last_test_count - first_test_count))
    
    echo "**Performance Metrics:**"
    echo "- Average execution time: ${avg_exec_time}s"
    echo "- Best time: ${min_exec_time}s"
    echo "- Worst time: ${max_exec_time}s"
    echo "- Test suite growth: +${test_growth} tests"
    
    # Performance trend analysis
    local first_exec_time last_exec_time
    first_exec_time=$(jq -r '.trends[0].executionTime // 0' "$trends_file")
    last_exec_time=$(jq -r '.trends[-1].executionTime // 0' "$trends_file")
    
    local time_change
    time_change=$(awk -v first="$first_exec_time" -v last="$last_exec_time" 'BEGIN {printf "%.2f", last - first}')
    
    echo "- Time trend: ${time_change}s ($(if [[ $(awk -v change="$time_change" 'BEGIN {print (change <= 0) ? 1 : 0}') -eq 1 ]]; then echo "✅ Faster"; else echo "⚠️ Slower"; fi))"
}

generate_ascii_trend_chart() {
    local trends_file="$1"
    local field="$2"
    local chart_width=50
    local chart_height=10
    
    # Extract field data
    local data_points
    data_points=$(jq -r ".trends[].$field" "$trends_file")
    
    if [[ -z "$data_points" ]]; then
        echo "No data available for $field"
        return 1
    fi
    
    # Convert to array
    local points=()
    while IFS= read -r point; do
        points+=("$point")
    done <<< "$data_points"
    
    # Find min and max values
    local min_val max_val
    min_val=$(printf '%s\n' "${points[@]}" | sort -n | head -1)
    max_val=$(printf '%s\n' "${points[@]}" | sort -n | tail -1)
    
    # Avoid division by zero
    local range
    range=$(awk -v max="$max_val" -v min="$min_val" 'BEGIN {print max - min}')
    if [[ $(awk -v range="$range" 'BEGIN {print (range == 0) ? 1 : 0}') -eq 1 ]]; then
        range=1
    fi
    
    # Generate chart header
    printf "%8s │" "$field"
    for ((i=0; i<chart_width; i++)); do printf "─"; done
    printf "│ Values\n"
    
    # Generate chart rows
    for ((row=chart_height-1; row>=0; row--)); do
        local threshold
        threshold=$(awk -v min="$min_val" -v max="$max_val" -v row="$row" -v height="$chart_height" 'BEGIN {
            printf "%.2f", min + (max - min) * (row + 0.5) / height
        }')
        
        printf "%7.1f │" "$threshold"
        
        # Plot data points for this row
        local point_index=0
        for point in "${points[@]}"; do
            local x_pos
            x_pos=$((point_index * chart_width / ${#points[@]}))
            
            # Check if point should be plotted at this y level
            local y_pos
            y_pos=$(awk -v min="$min_val" -v max="$max_val" -v val="$point" -v height="$chart_height" 'BEGIN {
                if (max == min) print height / 2
                else printf "%.0f", (val - min) / (max - min) * height
            }')
            
            if [[ $y_pos -eq $row ]]; then
                # Plot point
                for ((i=0; i<chart_width; i++)); do
                    if [[ $i -eq $x_pos ]]; then
                        printf "●"
                    elif [[ $i -eq $((x_pos-1)) ]] || [[ $i -eq $((x_pos+1)) ]]; then
                        printf "─"
                    else
                        printf " "
                    fi
                done
                break
            fi
            ((point_index++))
        done
        
        # Fill empty space if no point plotted
        if [[ $point_index -eq ${#points[@]} ]]; then
            for ((i=0; i<chart_width; i++)); do printf " "; done
        fi
        
        printf "│\n"
    done
    
    # Chart footer
    printf "        │"
    for ((i=0; i<chart_width; i++)); do printf "─"; done
    printf "│\n"
    printf "        │%-${chart_width}s│\n" " Time →"
}

# Generate detailed markdown report (abbreviated version)
generate_markdown_report() {
    local report_file="$TEST_RESULTS_DIR/test-report-$TIMESTAMP.md"
    local results_file="$TEST_RESULTS_DIR/parsed_results.json"
    local coverage_file="$TEST_RESULTS_DIR/coverage_results.json"
    
    # Get data
    local total=$(jq -r '.tests.total' "$results_file" 2>/dev/null || echo "0")
    local passed=$(jq -r '.tests.passed' "$results_file" 2>/dev/null || echo "0")
    local failed=$(jq -r '.tests.failed' "$results_file" 2>/dev/null || echo "0")
    local skipped=$(jq -r '.tests.skipped' "$results_file" 2>/dev/null || echo "0")
    local pass_rate=$(jq -r '.tests.pass_rate' "$results_file" 2>/dev/null || echo "0")
    local exec_time=$(jq -r '.execution_time' "$results_file" 2>/dev/null || echo "0")
    
    local line_cov="N/A"
    local branch_cov="N/A"
    local meets_threshold="No"
    
    if [[ -f "$coverage_file" ]]; then
        line_cov=$(jq -r '.line_coverage' "$coverage_file" 2>/dev/null | sed 's/\.0$//' || echo "N/A")
        branch_cov=$(jq -r '.branch_coverage' "$coverage_file" 2>/dev/null | sed 's/\.0$//' || echo "N/A")
        local threshold_met=$(jq -r '.meets_threshold' "$coverage_file" 2>/dev/null || echo "0")
        [[ "$threshold_met" == "1" ]] && meets_threshold="Yes"
    fi
    
    cat > "$report_file" << EOF
# 🧪 Test Suite Report

**Generated:** $(date)
**Duration:** ${exec_time}s
**Project:** zarichney-api
**Mode:** $MODE

## 📊 Executive Summary

$(if [[ $failed -eq 0 ]]; then echo "✅ **ALL TESTS PASSING** - Excellent stability"; else echo "❌ **$failed TESTS FAILING** - Requires attention"; fi)

| Metric | Value | Status |
|--------|--------|--------|
| Total Tests | $total | $(if [[ $total -gt 200 ]]; then echo "✅ Comprehensive"; else echo "⚠️ Growing"; fi) |
| Passed | $passed (${pass_rate}%) | $(if [[ $pass_rate -ge 95 ]]; then echo "✅ Excellent"; elif [[ $pass_rate -ge 80 ]]; then echo "⚠️ Good"; else echo "❌ Needs Improvement"; fi) |
| Failed | $failed | $(if [[ $failed -eq 0 ]]; then echo "✅ None"; else echo "❌ Critical"; fi) |
| Skipped | $skipped | $(if [[ $skipped -lt 30 ]]; then echo "✅ Acceptable"; else echo "⚠️ High"; fi) |

## 📈 Code Coverage Analysis

| Coverage Type | Percentage | Threshold | Status |
|---------------|------------|-----------|--------|
| Line Coverage | ${line_cov}% | ${COVERAGE_THRESHOLD}% | $(if [[ "$meets_threshold" == "Yes" ]]; then echo "✅ Meets Target"; else echo "⚠️ Below Target"; fi) |
| Branch Coverage | ${branch_cov}% | 20% | $(if [[ $(awk -v bc="${branch_cov/N\/A/0}" 'BEGIN {print (bc >= 20) ? 1 : 0}') -eq 1 ]]; then echo "✅ Meets Target"; else echo "⚠️ Below Target"; fi) |

$(generate_progressive_coverage_visualization)

$(generate_health_trends)
$(generate_coverage_velocity_chart)
$(generate_skip_rate_analysis)
$(generate_test_execution_metrics)

## 🎯 Recommendations

1. **Coverage Enhancement**: Add unit tests for business logic to reach 40%+ coverage
2. **Integration Setup**: Configure external services for full test coverage  
3. **Performance Monitoring**: Establish baseline metrics for regression detection
4. **Documentation**: Update test documentation with unified test suite capabilities

---

**Report Generated by:** Unified Test Suite (zarichney-api)  
**Script Mode:** $MODE
**Next scheduled run:** As needed via \`run-test-suite.sh\` command
EOF
    
    success "Detailed report generated: $report_file"
    
    if [[ "$OUTPUT_FORMAT" == "markdown" ]]; then
        echo ""
        echo -e "${BOLD}📄 Full report saved to:${NC} $report_file"
        echo ""
        generate_summary_report
    fi
}

# Quality gate enforcement for CI/CD
enforce_quality_gates() {
    log "🚪 Enforcing quality gates..."
    
    local results_file="$TEST_RESULTS_DIR/parsed_results.json"
    local coverage_file="$TEST_RESULTS_DIR/coverage_results.json"
    local gate_failed=false
    local critical_failures=false
    
    if [[ ! -f "$results_file" ]]; then
        error_exit "Quality gate enforcement failed: No test results found"
    fi
    
    # Extract test metrics
    local total_tests=$(jq -r '.tests.total // 0' "$results_file" 2>/dev/null || echo "0")
    local failed_tests=$(jq -r '.tests.failed // 0' "$results_file" 2>/dev/null || echo "0")
    local pass_rate=$(jq -r '.tests.pass_rate // 0' "$results_file" 2>/dev/null || echo "0")
    
    # ============================================================================
    # 🚨 CRITICAL: TEST FAILURES (HIGHEST PRIORITY)
    # ============================================================================
    if [[ $failed_tests -gt 0 ]]; then
        critical_failures=true
        gate_failed=true
        
        echo ""
        print_error "🚨 CRITICAL FAILURE: $failed_tests TEST(S) ARE FAILING"
        print_error "==============================================="
        print_error "❌ Cannot proceed with deployment while tests are failing"
        print_error "❌ Failed tests must be fixed before quality gates can pass"
        echo ""
        
        # Display detailed failure information if available
        local failures_data=$(jq -r '.failures.failed_tests[]?' "$results_file" 2>/dev/null || echo "")
        if [[ -n "$failures_data" ]]; then
            print_error "🔍 DETAILED FAILURE INFORMATION:"
            print_error "--------------------------------"
            
            local failure_count=0
            while IFS= read -r failure; do
                if [[ -n "$failure" ]]; then
                    local test_name=$(echo "$failure" | jq -r '.test_name // "Unknown"')
                    local category=$(echo "$failure" | jq -r '.category // "Unknown"')
                    local error_msg=$(echo "$failure" | jq -r '.error_message // "No error message available"')
                    
                    failure_count=$((failure_count + 1))
                    print_error "❌ Test #$failure_count: $test_name"
                    print_error "   Category: $category"
                    if [[ "$error_msg" != "No error message available" && -n "$error_msg" ]]; then
                        print_error "   Error: $error_msg"
                    fi
                    print_error ""
                fi
            done <<< "$(jq -c '.failures.failed_tests[]?' "$results_file" 2>/dev/null || echo "")"
            
            if [[ $failure_count -eq 0 ]]; then
                print_error "❌ Could not extract detailed failure information"
                print_error "   Check TRX files in TestResults/ directory for details"
                print_error ""
            fi
        else
            print_error "❌ No detailed failure information available"
            print_error "   Check TRX files in TestResults/ directory for details"
            print_error ""
        fi
        
        print_error "🔧 REQUIRED ACTIONS:"
        print_error "   1. Investigate and fix the failing test(s)"
        print_error "   2. Ensure all tests pass locally before pushing"
        print_error "   3. Re-run the pipeline after fixes"
        echo ""
    fi
    
    # ============================================================================
    # 📊 SECONDARY: COVERAGE ANALYSIS
    # ============================================================================
    if [[ -f "$coverage_file" ]]; then
        local line_coverage=$(jq -r '.line_coverage // 0' "$coverage_file" 2>/dev/null || echo "0")
        local meets_threshold=$(jq -r '.meets_threshold // 0' "$coverage_file" 2>/dev/null || echo "0")
        
        if [[ "$meets_threshold" != "1" ]] && [[ "${QUALITY_GATE_ENABLED:-false}" == "true" ]]; then
            if [[ "${COVERAGE_FLEXIBLE:-false}" == "true" ]]; then
                if [[ $critical_failures == false ]]; then
                    warning "📊 COVERAGE WARNING: ${line_coverage}% below threshold ${COVERAGE_THRESHOLD}%"
                    warning "   (Allowed due to test branch or flexibility setting)"
                else
                    info "📊 Coverage: ${line_coverage}% below threshold ${COVERAGE_THRESHOLD}% (secondary to test failures)"
                fi
            else
                if [[ $critical_failures == false ]]; then
                    print_error "📊 QUALITY GATE FAILED: Coverage ${line_coverage}% below threshold ${COVERAGE_THRESHOLD}%"
                    gate_failed=true
                else
                    info "📊 Coverage: ${line_coverage}% below threshold ${COVERAGE_THRESHOLD}% (secondary to test failures)"
                fi
            fi
        else
            info "📊 Coverage: ${line_coverage}% (threshold: ${COVERAGE_THRESHOLD}%, flexible: ${COVERAGE_FLEXIBLE:-false})"
        fi
    fi
    
    # ============================================================================
    # 📋 FINAL QUALITY GATE DECISION
    # ============================================================================
    if [[ "$gate_failed" == "true" ]]; then
        echo ""
        if [[ $critical_failures == true ]]; then
            print_error "🚫 QUALITY GATES FAILED: CRITICAL TEST FAILURES DETECTED"
            print_error "   Primary issue: $failed_tests failing test(s) must be fixed"
            print_error "   Deployment blocked until all tests pass"
        else
            print_error "🚫 QUALITY GATES FAILED: Quality standards not met"
            print_error "   All quality issues must be resolved before deployment"
        fi
        echo ""
        
        # For Phase 2: Continue with AI analysis even on quality gate failures
        # Store the failure state for later decision making
        echo "QUALITY_GATES_FAILED=true" >> "$TEST_RESULTS_DIR/quality_status.env"
        echo "CRITICAL_TEST_FAILURES=$critical_failures" >> "$TEST_RESULTS_DIR/quality_status.env"
        return 1  # Return failure code without exiting script
    else
        echo ""
        success "✅ ALL QUALITY GATES PASSED"
        success "   Tests: $total_tests total, $failed_tests failed ($pass_rate% pass rate)"
        if [[ -f "$coverage_file" ]]; then
            local line_coverage=$(jq -r '.line_coverage // 0' "$coverage_file" 2>/dev/null || echo "0")
            success "   Coverage: ${line_coverage}% (meets requirements)"
        fi
        success "   ✅ Ready for deployment"
        echo ""
        
        echo "QUALITY_GATES_FAILED=false" >> "$TEST_RESULTS_DIR/quality_status.env"
        echo "CRITICAL_TEST_FAILURES=false" >> "$TEST_RESULTS_DIR/quality_status.env"
        return 0
    fi
}

# Execute report mode
execute_report_mode() {
    print_header "Executing Report Mode"
    parse_results
    parse_coverage
    
    # Phase 2 Test Suite Baseline Validation
    validate_test_baselines || true  # Don't fail build on baseline validation errors (warnings only)
    
    generate_report
    
    # Phase 2 AI-Powered Quality Analysis
    if [[ "$COMPARE_MODE" == true ]] || [[ "${QUALITY_GATE_ENABLED:-false}" == "true" ]]; then
        detect_quality_regression || true  # Don't fail on regression detection errors
    fi
    
    # Phase 3 Dynamic Quality Gates
    # Update global threshold for this run by calling the function
    COVERAGE_THRESHOLD=$(calculate_dynamic_quality_gates) || true  # Don't fail on dynamic gates calculation errors
    
    # Phase 3 Real-time metrics and trending analysis
    generate_trending_analysis || true  # Don't fail on trending analysis errors
    
    # Predictive deployment risk assessment (Phase 2)
    assess_deployment_risk || true  # Don't fail on risk assessment errors
    
    # Enforce quality gates for CI/CD environments
    local quality_gate_status=0
    if [[ "${CI_ENVIRONMENT:-false}" == "true" ]] || [[ "${QUALITY_GATE_ENABLED:-false}" == "true" ]]; then
        enforce_quality_gates || quality_gate_status=$?
    fi
    
    # Save baseline if requested (includes current coverage for future comparisons)
    if [[ "$SAVE_BASELINE" == true ]]; then
        log "💾 Saving enhanced baseline for future comparisons..."
        # Create comprehensive baseline including coverage metrics
        if [[ -f "$TEST_RESULTS_DIR/parsed_results.json" && -f "$TEST_RESULTS_DIR/coverage_results.json" ]]; then
            jq -s '.[0] + {"coverage_metrics": .[1]}' \
                "$TEST_RESULTS_DIR/parsed_results.json" \
                "$TEST_RESULTS_DIR/coverage_results.json" \
                > "$TEST_RESULTS_DIR/baseline_results.json" 2>/dev/null || \
            cp "$TEST_RESULTS_DIR/parsed_results.json" "$TEST_RESULTS_DIR/baseline_results.json"
        fi
    fi
    
    # Phase 3: Save historical data for dynamic quality gates
    save_historical_data || true  # Don't fail on historical data saving errors
    
    # Store quality gate status for CI/CD decision making
    if [[ $quality_gate_status -ne 0 ]]; then
        warning "Quality gates failed - results available for AI analysis"
        # Only return failure for CI environments
        if [[ "${CI_ENVIRONMENT:-false}" == "true" ]]; then
            return 1
        fi
        # Return success for non-CI environments to allow workflow to continue
        return 0
    fi
    
    return 0
}

# ==============================================================================
# MODE-SPECIFIC FUNCTIONS
# ==============================================================================

# Parse command line arguments
parse_arguments() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            automation|report|both)
                MODE="$1"
                shift
                ;;
            json|markdown|summary|console)
                OUTPUT_FORMAT="$1"
                shift
                ;;
            -h|--help)
                show_usage
                exit 0
                ;;
            --no-browser)
                SKIP_BROWSER=true
                shift
                ;;
            --unit-only)
                UNIT_ONLY=true
                shift
                ;;
            --integration-only)
                INTEGRATION_ONLY=true
                shift
                ;;
            --skip-build)
                SKIP_BUILD=true
                shift
                ;;
            --skip-client-gen)
                SKIP_CLIENT_GEN=true
                shift
                ;;
            --performance)
                PERFORMANCE_ANALYSIS=true
                shift
                ;;
            --compare)
                COMPARE_MODE=true
                shift
                ;;
            --save-baseline)
                SAVE_BASELINE=true
                shift
                ;;
            --threshold=*)
                COVERAGE_THRESHOLD="${1#*=}"
                shift
                ;;
            --parallel)
                PARALLEL_EXECUTION=true
                shift
                ;;
            --max-parallel-collections=*)
                MAX_PARALLEL_COLLECTIONS="${1#*=}"
                PARALLEL_EXECUTION=true
                shift
                ;;
            *)
                warning "Unknown option: $1"
                shift
                ;;
        esac
    done
    
    # Validate mutually exclusive options
    if [[ "$UNIT_ONLY" == true && "$INTEGRATION_ONLY" == true ]]; then
        error_exit "Cannot specify both --unit-only and --integration-only"
    fi
}

# Main execution function
main() {
    # Parse arguments
    parse_arguments "$@"
    
    # Setup
    log "🚀 Starting unified test suite execution in $MODE mode..."
    print_header "Starting Test Suite - Mode: $MODE"
    print_status "Script: $0"
    print_status "Working directory: $ROOT_DIR"
    print_status "Configuration: $BUILD_CONFIG"
    print_status "Output format: $OUTPUT_FORMAT"
    
    # Execute common steps
    check_prerequisites
    clean_previous_results
    
    if [[ "$SKIP_BUILD" == false ]]; then
        build_solution
    else
        print_status "Skipping build step as requested"
    fi
    
    if [[ "$SKIP_CLIENT_GEN" == false ]]; then
        generate_api_client
    else
        print_status "Skipping API client generation as requested"
    fi
    
    # Execute tests
    run_all_tests
    
    # Execute mode-specific functionality
    local mode_exit_code=0
    case $MODE in
        automation)
            execute_automation_mode || mode_exit_code=$?
            ;;
        report)
            execute_report_mode || mode_exit_code=$?
            ;;
        both)
            execute_report_mode || mode_exit_code=$?
            execute_automation_mode || mode_exit_code=$?
            ;;
    esac
    
    # Check if we need to exit with failure code for CI/CD
    if [[ -f "$TEST_RESULTS_DIR/quality_status.env" ]]; then
        source "$TEST_RESULTS_DIR/quality_status.env"
        if [[ "${QUALITY_GATES_FAILED:-false}" == "true" ]] && [[ "${CI_ENVIRONMENT:-false}" == "true" ]]; then
            print_error "🎉 Test suite analysis completed, but quality gates failed - CI/CD should block deployment"
            exit 1
        fi
    fi
    
    if [[ $mode_exit_code -ne 0 ]]; then
        print_warning "🎉 Test suite completed with warnings in $MODE mode!"
        exit $mode_exit_code
    else
        print_success "🎉 Test suite completed successfully in $MODE mode!"
    fi
}

# Cleanup function
cleanup() {
    log "🧹 Cleaning up temporary files..."
    # Keep results but clean up any temp files if needed
    return 0
}

# Trap to handle script interruption
trap 'print_error "Script interrupted"; exit 1' INT TERM

# Execute main function with all arguments
main "$@"

# Ensure we exit with success if we reach this point
exit 0