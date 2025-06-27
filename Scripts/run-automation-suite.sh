#!/bin/bash

# ==============================================================================
# Automation Suite Runner Script
# ==============================================================================
# This script runs the complete automation test suite for zarichney-api,
# generates comprehensive coverage reports, and opens the report in a browser.
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

# Colors for output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m' # No Color

# Script configuration
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly ROOT_DIR="$(dirname "$SCRIPT_DIR")"
readonly SOLUTION_FILE="$ROOT_DIR/zarichney-api.sln"
readonly TEST_RESULTS_DIR="$ROOT_DIR/TestResults"
readonly COVERAGE_REPORT_DIR="$ROOT_DIR/CoverageReport"
readonly COVERAGE_REPORT_INDEX="$COVERAGE_REPORT_DIR/index.html"

# Test configuration
readonly BUILD_CONFIG="Release"
readonly COVERAGE_FORMATS="HtmlInline_AzurePipelines;Cobertura;Badges;SummaryGithub"

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "\n${BLUE}==== $1 ====${NC}"
}

# Function to check prerequisites
check_prerequisites() {
    print_header "Checking Prerequisites"
    
    # Check if dotnet is available
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed or not in PATH"
        exit 1
    fi
    
    # Check dotnet version
    local dotnet_version
    dotnet_version=$(dotnet --version)
    print_status ".NET SDK version: $dotnet_version"
    
    # Check if Docker is running (required for integration tests)
    if ! docker info &> /dev/null; then
        print_warning "Docker is not running. Integration tests requiring Testcontainers will be skipped."
        print_status "To run integration tests, ensure Docker Desktop is running."
    else
        print_success "Docker is running and accessible"
    fi
    
    # Check if solution file exists
    if [[ ! -f "$SOLUTION_FILE" ]]; then
        print_error "Solution file not found: $SOLUTION_FILE"
        exit 1
    fi
    
    # Check if ReportGenerator tool is installed
    if ! dotnet reportgenerator --help &> /dev/null; then
        print_status "Installing ReportGenerator global tool..."
        dotnet tool install -g dotnet-reportgenerator-globaltool || {
            print_error "Failed to install ReportGenerator tool"
            exit 1
        }
    fi
    
    print_success "All prerequisites checked"
}

# Function to clean previous results
clean_previous_results() {
    print_header "Cleaning Previous Results"
    
    if [[ -d "$TEST_RESULTS_DIR" ]]; then
        print_status "Removing previous test results: $TEST_RESULTS_DIR"
        rm -rf "$TEST_RESULTS_DIR"
    fi
    
    if [[ -d "$COVERAGE_REPORT_DIR" ]]; then
        print_status "Removing previous coverage report: $COVERAGE_REPORT_DIR"
        rm -rf "$COVERAGE_REPORT_DIR"
    fi
    
    # Create directories
    mkdir -p "$TEST_RESULTS_DIR"/{unit,integration}
    mkdir -p "$COVERAGE_REPORT_DIR"
    
    print_success "Previous results cleaned"
}

# Function to restore and build solution
build_solution() {
    print_header "Building Solution"
    
    print_status "Restoring dependencies..."
    dotnet restore "$SOLUTION_FILE" || {
        print_error "Failed to restore dependencies"
        exit 1
    }
    
    print_status "Building solution in $BUILD_CONFIG configuration..."
    dotnet build "$SOLUTION_FILE" --configuration "$BUILD_CONFIG" --no-restore || {
        print_error "Build failed"
        exit 1
    }
    
    print_success "Solution built successfully"
}

# Function to generate API client
generate_api_client() {
    print_header "Generating API Client"
    
    local api_client_script="$SCRIPT_DIR/generate-api-client.sh"
    
    if [[ -f "$api_client_script" ]]; then
        print_status "Running API client generation script..."
        bash "$api_client_script" || {
            print_error "API client generation failed"
            exit 1
        }
        print_success "API client generated successfully"
    else
        print_warning "API client generation script not found: $api_client_script"
        print_status "Skipping API client generation"
    fi
}

# Function to run unit tests
run_unit_tests() {
    print_header "Running Unit Tests"
    
    local unit_results_dir="$TEST_RESULTS_DIR/unit"
    
    print_status "Executing unit tests with coverage collection..."
    
    # Check if Docker group membership is active for potential sg docker usage
    local test_command="dotnet test \"$SOLUTION_FILE\" --filter \"Category=Unit\" --configuration \"$BUILD_CONFIG\" --no-build --collect:\"XPlat Code Coverage\" --results-directory \"$unit_results_dir\" --logger \"trx;LogFileName=unit_tests.trx\""
    
    if groups | grep -q docker; then
        # User is in docker group, run normally
        eval "$test_command" || {
            print_error "Unit tests failed"
            exit 1
        }
    else
        # Try with sg docker if available
        if command -v sg &> /dev/null; then
            print_status "Running with 'sg docker' for Docker access..."
            sg docker -c "$test_command" || {
                print_error "Unit tests failed"
                exit 1
            }
        else
            # Run normally and let it fail if Docker access is needed
            eval "$test_command" || {
                print_error "Unit tests failed"
                exit 1
            }
        fi
    fi
    
    print_success "Unit tests completed"
}

# Function to run integration tests
run_integration_tests() {
    print_header "Running Integration Tests"
    
    local integration_results_dir="$TEST_RESULTS_DIR/integration"
    
    print_status "Executing integration tests with coverage collection..."
    
    # Check if Docker group membership is active for potential sg docker usage
    local test_command="dotnet test \"$SOLUTION_FILE\" --filter \"Category=Integration\" --configuration \"$BUILD_CONFIG\" --no-build --collect:\"XPlat Code Coverage\" --results-directory \"$integration_results_dir\" --logger \"trx;LogFileName=integration_tests.trx\""
    
    if groups | grep -q docker; then
        # User is in docker group, run normally
        eval "$test_command" || {
            print_error "Integration tests failed"
            exit 1
        }
    else
        # Try with sg docker if available
        if command -v sg &> /dev/null; then
            print_status "Running with 'sg docker' for Docker access..."
            sg docker -c "$test_command" || {
                print_error "Integration tests failed"
                exit 1
            }
        else
            # Run normally and let it fail if Docker access is needed
            eval "$test_command" || {
                print_error "Integration tests failed"
                exit 1
            }
        fi
    fi
    
    print_success "Integration tests completed"
}

# Function to generate coverage report
generate_coverage_report() {
    print_header "Generating Coverage Report"
    
    # Find all coverage files
    local coverage_files
    coverage_files=$(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" -type f)
    
    if [[ -z "$coverage_files" ]]; then
        print_error "No coverage files found in $TEST_RESULTS_DIR"
        exit 1
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
        print_error "Coverage report generation failed"
        exit 1
    }
    
    print_success "Coverage report generated at: $COVERAGE_REPORT_DIR"
}

# Function to display test summary
display_test_summary() {
    print_header "Test Execution Summary"
    
    # Count TRX files
    local trx_files
    trx_files=$(find "$TEST_RESULTS_DIR" -name "*.trx" -type f | wc -l)
    
    # Check if coverage report exists
    if [[ -f "$COVERAGE_REPORT_INDEX" ]]; then
        print_success "✅ Test execution completed successfully"
        print_status "📊 Test result files: $trx_files TRX files generated"
        print_status "📈 Coverage report: $COVERAGE_REPORT_INDEX"
        
        # Try to extract summary from coverage report if available
        local summary_file="$COVERAGE_REPORT_DIR/Summary.txt"
        if [[ -f "$summary_file" ]]; then
            print_status "📋 Coverage Summary:"
            cat "$summary_file" | head -20
        fi
    else
        print_error "❌ Coverage report was not generated successfully"
        exit 1
    fi
}

# Function to open report in browser
open_report_in_browser() {
    print_header "Opening Coverage Report"
    
    if [[ ! -f "$COVERAGE_REPORT_INDEX" ]]; then
        print_error "Coverage report index file not found: $COVERAGE_REPORT_INDEX"
        exit 1
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
        print_warning "Could not detect how to open browser on this system"
        print_status "Please manually open: $COVERAGE_REPORT_INDEX"
        return 1
    fi
    
    print_success "Coverage report should now be open in your browser"
}

# Function to show usage
show_usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Run the complete automation test suite with coverage reporting.

OPTIONS:
    -h, --help          Show this help message
    --no-browser        Skip opening the report in browser
    --unit-only         Run only unit tests
    --integration-only  Run only integration tests
    --skip-build        Skip the build step (assumes solution is already built)
    --skip-client-gen   Skip API client generation

EXAMPLES:
    $0                      # Run full automation suite and open report
    $0 --no-browser         # Run tests but don't open browser
    $0 --unit-only          # Run only unit tests
    $0 --skip-build         # Skip build step for faster iteration

EOF
}

# Main execution function
main() {
    local skip_browser=false
    local unit_only=false
    local integration_only=false
    local skip_build=false
    local skip_client_gen=false
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--help)
                show_usage
                exit 0
                ;;
            --no-browser)
                skip_browser=true
                shift
                ;;
            --unit-only)
                unit_only=true
                shift
                ;;
            --integration-only)
                integration_only=true
                shift
                ;;
            --skip-build)
                skip_build=true
                shift
                ;;
            --skip-client-gen)
                skip_client_gen=true
                shift
                ;;
            *)
                print_error "Unknown option: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    # Validate mutually exclusive options
    if [[ "$unit_only" == true && "$integration_only" == true ]]; then
        print_error "Cannot specify both --unit-only and --integration-only"
        exit 1
    fi
    
    # Start execution
    print_header "Starting Automation Suite"
    print_status "Script: $0"
    print_status "Working directory: $ROOT_DIR"
    print_status "Configuration: $BUILD_CONFIG"
    
    # Execute steps
    check_prerequisites
    clean_previous_results
    
    if [[ "$skip_build" == false ]]; then
        build_solution
    else
        print_status "Skipping build step as requested"
    fi
    
    if [[ "$skip_client_gen" == false ]]; then
        generate_api_client
    else
        print_status "Skipping API client generation as requested"
    fi
    
    # Run tests based on options
    if [[ "$integration_only" == true ]]; then
        run_integration_tests
    elif [[ "$unit_only" == true ]]; then
        run_unit_tests
    else
        # Run both
        run_unit_tests
        run_integration_tests
    fi
    
    generate_coverage_report
    display_test_summary
    
    if [[ "$skip_browser" == false ]]; then
        open_report_in_browser
    else
        print_status "Skipping browser opening as requested"
        print_status "To view the report, open: $COVERAGE_REPORT_INDEX"
    fi
    
    print_success "🎉 Automation suite completed successfully!"
}

# Trap to handle script interruption
trap 'print_error "Script interrupted"; exit 1' INT TERM

# Execute main function with all arguments
main "$@"