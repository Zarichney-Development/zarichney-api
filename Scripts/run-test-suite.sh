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
readonly COVERAGE_FORMATS="HtmlInline_AzurePipelines;Cobertura;Badges;SummaryGithub"

# Default settings
MODE="report"  # Default to report mode for backward compatibility
OUTPUT_FORMAT="markdown"
COVERAGE_THRESHOLD=25
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
    if [[ "$MODE" == "automation" || "$MODE" == "both" ]]; then
        if ! dotnet reportgenerator --help &> /dev/null; then
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
            sg docker -c "$test_command" || {
                error_exit "$test_type tests failed"
            }
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
            sg docker -c "$test_command" > "$results_dir/${category,,}_output.log" 2>&1
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
    
    dotnet reportgenerator \
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
    
    # Find the TRX file
    local trx_file=$(find "$TEST_RESULTS_DIR" -name "*.trx" | head -1)
    if [[ -z "$trx_file" ]]; then
        error_exit "No TRX test results file found"
    fi
    
    # Extract test statistics using grep and basic parsing
    local total_tests=0
    local passed_tests=0
    local failed_tests=0
    local skipped_tests=0
    
    # Parse from the console output in log file
    if grep -q "Passed:" "$LOG_FILE"; then
        local stats_line=$(grep "Passed:" "$LOG_FILE" | tail -1)
        failed_tests=$(echo "$stats_line" | grep -o "Failed:[[:space:]]*[0-9]*" | grep -o "[0-9]*" || echo "0")
        passed_tests=$(echo "$stats_line" | grep -o "Passed:[[:space:]]*[0-9]*" | grep -o "[0-9]*" || echo "0")
        skipped_tests=$(echo "$stats_line" | grep -o "Skipped:[[:space:]]*[0-9]*" | grep -o "[0-9]*" || echo "0")
        total_tests=$(echo "$stats_line" | grep -o "Total:[[:space:]]*[0-9]*" | grep -o "[0-9]*" || echo "0")
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
    
    success "Test results parsed: $passed_tests passed, $failed_tests failed, $skipped_tests skipped"
}

# Parse coverage data (report mode)
parse_coverage() {
    log "📈 Analyzing code coverage..."
    
    # Find coverage XML file
    local coverage_file=$(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" | head -1)
    if [[ -z "$coverage_file" ]]; then
        warning "No coverage file found"
        return
    fi
    
    # Extract coverage percentages using basic XML parsing
    local line_rate=$(grep -o 'line-rate="[0-9.]*"' "$coverage_file" | head -1 | grep -o '[0-9.]*' || echo "0")
    local branch_rate=$(grep -o 'branch-rate="[0-9.]*"' "$coverage_file" | head -1 | grep -o '[0-9.]*' || echo "0")
    
    # Convert to percentages
    local line_coverage=$(awk -v lr="$line_rate" 'BEGIN {printf "%.1f", lr * 100}')
    local branch_coverage=$(awk -v br="$branch_rate" 'BEGIN {printf "%.1f", br * 100}')
    
    # Store coverage results
    cat > "$TEST_RESULTS_DIR/coverage_results.json" << EOF
{
    "line_coverage": $line_coverage,
    "branch_coverage": $branch_coverage,
    "threshold": $COVERAGE_THRESHOLD,
    "meets_threshold": $(awk -v lc="$line_coverage" -v ct="$COVERAGE_THRESHOLD" 'BEGIN {print (lc >= ct) ? 1 : 0}')
}
EOF
    
    success "Coverage analyzed: ${line_coverage}% lines, ${branch_coverage}% branches"
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
        risk_score=$((risk_score + $(awk -v tr="$test_risk" 'BEGIN {print (tr > 40) ? 40 : tr}')))
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
    
    if [[ -f "$results_file" && -f "$coverage_file" ]]; then
        if command -v jq &> /dev/null; then
            jq -s '.[0] + {"coverage": .[1]}' "$results_file" "$coverage_file"
        else
            # Fallback if jq not available
            echo '{"error": "JSON processing not available", "results_file": "'$results_file'", "coverage_file": "'$coverage_file'"}'
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
        echo -e "📊 Total Tests: ${BLUE}$total${NC}"
        echo -e "✅ Passed: ${GREEN}$passed${NC}"
        echo -e "❌ Failed: ${RED}$failed${NC}"
        echo -e "📈 Pass Rate: ${BOLD}$pass_rate%${NC}"
        
        if [[ -f "$TEST_RESULTS_DIR/coverage_results.json" ]]; then
            local line_cov=$(jq -r '.line_coverage' "$TEST_RESULTS_DIR/coverage_results.json" 2>/dev/null || echo "N/A")
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
    
    if [[ ! -f "$results_file" ]]; then
        error_exit "Quality gate enforcement failed: No test results found"
    fi
    
    # Extract test metrics
    local total_tests=$(jq -r '.tests.total // 0' "$results_file" 2>/dev/null || echo "0")
    local failed_tests=$(jq -r '.tests.failed // 0' "$results_file" 2>/dev/null || echo "0")
    local pass_rate=$(jq -r '.tests.pass_rate // 0' "$results_file" 2>/dev/null || echo "0")
    
    # Test failure gate
    if [[ $failed_tests -gt 0 ]]; then
        print_error "QUALITY GATE FAILED: $failed_tests test(s) are failing"
        gate_failed=true
    fi
    
    # Coverage gate (if coverage data available)
    if [[ -f "$coverage_file" ]]; then
        local line_coverage=$(jq -r '.line_coverage // 0' "$coverage_file" 2>/dev/null || echo "0")
        local meets_threshold=$(jq -r '.meets_threshold // 0' "$coverage_file" 2>/dev/null || echo "0")
        
        if [[ "$meets_threshold" != "1" ]] && [[ "${QUALITY_GATE_ENABLED:-false}" == "true" ]]; then
            warning "QUALITY GATE WARNING: Coverage ${line_coverage}% below threshold ${COVERAGE_THRESHOLD}%"
        fi
        
        info "Coverage analysis: ${line_coverage}% (threshold: ${COVERAGE_THRESHOLD}%)"
    fi
    
    if [[ "$gate_failed" == "true" ]]; then
        print_error "Quality gates failed - CI/CD should block deployment"
        # For Phase 2: Continue with AI analysis even on quality gate failures
        # Store the failure state for later decision making
        echo "QUALITY_GATES_FAILED=true" >> "$TEST_RESULTS_DIR/quality_status.env"
        return 1  # Return failure code without exiting script
    else
        success "All quality gates passed"
        echo "QUALITY_GATES_FAILED=false" >> "$TEST_RESULTS_DIR/quality_status.env"
        return 0
    fi
}

# Execute report mode
execute_report_mode() {
    print_header "Executing Report Mode"
    parse_results
    parse_coverage
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
        exit 1
    fi
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