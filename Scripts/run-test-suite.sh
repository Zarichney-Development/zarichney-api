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

EXAMPLES:
    $0                          # Run report mode with markdown output
    $0 automation               # Run automation mode with HTML report
    $0 report json              # Run report mode with JSON output
    $0 both --unit-only         # Run both modes with unit tests only
    $0 automation --no-browser  # Run automation without opening browser
    $0 report summary --performance  # Quick summary with performance data

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

# Function to run all tests (unified for report mode)
run_all_tests() {
    print_header "Running Comprehensive Test Suite"
    
    cd "$ROOT_DIR"
    local start_time=$(date +%s)
    
    # For report mode, run all tests together for better parsing
    if [[ "$MODE" == "report" ]]; then
        local test_exit_code=0
        if dotnet test "$SOLUTION_FILE" --logger trx --logger 'console;verbosity=detailed' --results-directory "$TEST_RESULTS_DIR" --collect:'XPlat Code Coverage' --nologo 2>&1 | tee -a "$LOG_FILE"; then
            success "Test execution completed successfully"
        else
            test_exit_code=$?
            warning "Test execution had failures (exit code: $test_exit_code) - continuing with analysis"
        fi
    else
        # For automation mode, run tests by category
        if [[ "$INTEGRATION_ONLY" == true ]]; then
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
        "pass_rate": $(( total_tests > 0 ? (passed_tests * 100) / total_tests : 0 ))
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
    local line_coverage=$(echo "scale=1; $line_rate * 100" | bc 2>/dev/null || echo "0.0")
    local branch_coverage=$(echo "scale=1; $branch_rate * 100" | bc 2>/dev/null || echo "0.0")
    
    # Store coverage results
    cat > "$TEST_RESULTS_DIR/coverage_results.json" << EOF
{
    "line_coverage": $line_coverage,
    "branch_coverage": $branch_coverage,
    "threshold": $COVERAGE_THRESHOLD,
    "meets_threshold": $(echo "$line_coverage >= $COVERAGE_THRESHOLD" | bc 2>/dev/null || echo "0")
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
    local pass_rate_change=$(echo "scale=2; $current_pass_rate - $baseline_pass_rate" | bc 2>/dev/null || echo "0")
    local coverage_change=$(echo "scale=2; $current_coverage - $baseline_coverage" | bc 2>/dev/null || echo "0")
    local test_count_change=$((current_total - baseline_total))
    
    # Determine regression severity
    local regression_level="NONE"
    local regression_detected=false
    
    if (( $(echo "$pass_rate_change < -5" | bc -l) )); then
        regression_level="HIGH"
        regression_detected=true
    elif (( $(echo "$pass_rate_change < -2" | bc -l) )); then
        regression_level="MEDIUM"
        regression_detected=true
    elif (( $(echo "$coverage_change < -3" | bc -l) )); then
        regression_level="MEDIUM"
        regression_detected=true
    elif (( $(echo "$coverage_change < -1" | bc -l) )); then
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
    if (( $(echo "$coverage < 24" | bc -l) )); then
        local coverage_risk=$(echo "scale=0; (24 - $coverage) * 2" | bc)
        risk_score=$((risk_score + coverage_risk))
        risk_factors+=("Low coverage ${coverage}% (+${coverage_risk} risk)")
    fi
    
    # Pass rate risk (0-20 points)
    if (( $(echo "$pass_rate < 95" | bc -l) )); then
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
| Branch Coverage | ${branch_cov}% | 20% | $(if command -v bc >/dev/null && [[ $(echo "${branch_cov/N\/A/0} >= 20" | bc 2>/dev/null || echo 0) -eq 1 ]]; then echo "✅ Meets Target"; else echo "⚠️ Below Target"; fi) |

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
    
    # Store quality gate status for CI/CD decision making
    if [[ $quality_gate_status -ne 0 ]]; then
        warning "Quality gates failed - results available for AI analysis"
        return 1
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