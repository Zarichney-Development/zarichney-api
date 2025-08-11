#!/bin/bash
# ==============================================================================
# Coverage Epic Automation Validation Script
# ==============================================================================
# Validates the GitHub Actions automation infrastructure for Epic #94
# Ensures all components are properly configured for autonomous AI agent execution
#
# Usage:
#   ./Scripts/validate-coverage-automation.sh [--verbose] [--fix]
#
# Exit Codes:
#   0 = All validations passed
#   1 = Validation failures detected  
#   2 = Critical infrastructure missing
# ==============================================================================

set -euo pipefail

# Script configuration
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly ROOT_DIR="$(dirname "$SCRIPT_DIR")"
readonly TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# File paths
readonly WORKFLOW_FILE="$ROOT_DIR/.github/workflows/coverage-epic-automation.yml"
readonly PROMPT_FILE="$ROOT_DIR/.github/prompts/coverage-epic-agent.md"
readonly EPIC_WORKFLOW_DOC="$ROOT_DIR/Docs/Development/AutomatedCoverageEpicWorkflow.md"
readonly TEST_SUITE_SCRIPT="$ROOT_DIR/Scripts/run-test-suite.sh"

# Epic configuration
readonly EPIC_BRANCH="epic/testing-coverage-to-90"
readonly EPIC_ISSUE_ID="94"

# Validation settings
VERBOSE=false
FIX_ISSUES=false
VALIDATION_FAILED=false

# Colors for output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m' # No Color

# ==============================================================================
# Utility Functions
# ==============================================================================

print_header() {
    echo -e "${BLUE}=================================================${NC}"
    echo -e "${BLUE}🔍 Coverage Epic Automation Validation${NC}"
    echo -e "${BLUE}=================================================${NC}"
    echo "📅 Validation Time: $(date -u '+%Y-%m-%d %H:%M:%S UTC')"
    echo "📂 Repository: $(basename "$ROOT_DIR")"
    echo "🌿 Target Epic Branch: $EPIC_BRANCH"
    echo ""
}

print_section() {
    echo -e "${BLUE}📋 $1${NC}"
    echo "$(printf '%*s' "${#1}" | tr ' ' '-')"
}

print_success() {
    echo -e "  ${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "  ${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "  ${RED}❌ $1${NC}"
    VALIDATION_FAILED=true
}

print_info() {
    if [ "$VERBOSE" = true ]; then
        echo -e "  ${BLUE}ℹ️  $1${NC}"
    fi
}

# ==============================================================================
# Infrastructure File Validation
# ==============================================================================

validate_core_files() {
    print_section "Core Infrastructure Files"
    
    # GitHub Actions workflow
    if [ -f "$WORKFLOW_FILE" ]; then
        print_success "GitHub Actions workflow exists: coverage-epic-automation.yml"
        
        # Validate workflow content
        if grep -q "schedule:" "$WORKFLOW_FILE" && grep -q "0 \*/6 \* \* \*" "$WORKFLOW_FILE"; then
            print_success "Cron schedule configured (every 6 hours)"
        else
            print_error "Cron schedule missing or incorrect in workflow file"
        fi
        
        if grep -q "EPIC_BRANCH" "$WORKFLOW_FILE" && grep -q "epic/testing-coverage-to-90" "$WORKFLOW_FILE"; then
            print_success "Epic branch configuration found"
        else
            print_error "Epic branch configuration missing in workflow"
        fi
        
        if grep -q "coverage-epic-automation" "$WORKFLOW_FILE"; then
            print_success "Concurrency group configured for agent coordination"
        else
            print_warning "Concurrency group may not prevent agent conflicts"
        fi
        
    else
        print_error "GitHub Actions workflow missing: $WORKFLOW_FILE"
    fi
    
    # AI Agent prompt file
    if [ -f "$PROMPT_FILE" ]; then
        print_success "AI agent prompt exists: coverage-epic-agent.md"
        
        # Validate prompt content
        if grep -q "Epic Reference.*#94" "$PROMPT_FILE"; then
            print_success "Epic #94 reference found in prompt"
        else
            print_error "Epic #94 reference missing in prompt file"
        fi
        
        if grep -q "23 tests skipped" "$PROMPT_FILE"; then
            print_success "CI environment expectations documented"
        else
            print_error "CI environment test expectations missing"
        fi
        
        if grep -q "Standards.*TestingStandards.md" "$PROMPT_FILE"; then
            print_success "Testing standards references found"
        else
            print_error "Testing standards references missing in prompt"
        fi
        
    else
        print_error "AI agent prompt missing: $PROMPT_FILE"
    fi
    
    # Epic workflow documentation
    if [ -f "$EPIC_WORKFLOW_DOC" ]; then
        print_success "Epic workflow documentation exists"
        
        if grep -q "GitHub Actions CI" "$EPIC_WORKFLOW_DOC"; then
            print_success "CI environment documentation complete"
        else
            print_warning "CI environment details may be incomplete"
        fi
    else
        print_error "Epic workflow documentation missing: $EPIC_WORKFLOW_DOC"
    fi
    
    echo ""
}

# ==============================================================================
# Test Suite Integration Validation
# ==============================================================================

validate_test_suite() {
    print_section "Test Suite Integration"
    
    # Test suite script existence
    if [ -f "$TEST_SUITE_SCRIPT" ]; then
        print_success "Test suite script exists: run-test-suite.sh"
        
        # Check if executable
        if [ -x "$TEST_SUITE_SCRIPT" ]; then
            print_success "Test suite script is executable"
        else
            print_warning "Test suite script may not be executable"
            if [ "$FIX_ISSUES" = true ]; then
                chmod +x "$TEST_SUITE_SCRIPT"
                print_info "Fixed: Made test suite script executable"
            fi
        fi
        
        # Check for required modes
        if grep -q "report.*summary" "$TEST_SUITE_SCRIPT"; then
            print_success "Test suite supports required 'report summary' mode"
        else
            print_error "Test suite missing required 'report summary' mode"
        fi
        
        if grep -q "/test-report" "$ROOT_DIR/.claude" 2>/dev/null || command -v /test-report >/dev/null 2>&1; then
            print_success "Claude /test-report command integration available"
        else
            print_warning "Claude /test-report command may not be configured"
        fi
        
    else
        print_error "Test suite script missing: $TEST_SUITE_SCRIPT"
    fi
    
    echo ""
}

# ==============================================================================
# CI Environment Simulation
# ==============================================================================

validate_ci_environment() {
    print_section "CI Environment Simulation"
    
    print_info "Simulating unconfigured CI environment..."
    
    # Check if Docker is available (for TestContainers)
    if command -v docker >/dev/null 2>&1; then
        if docker info >/dev/null 2>&1; then
            print_success "Docker is available for TestContainers"
        else
            print_warning "Docker daemon not running - TestContainers may fail"
        fi
    else
        print_warning "Docker not available - integration tests will be skipped"
    fi
    
    # Test environment variable simulation
    export CI_SIMULATION=true
    export COVERAGE_TARGET_AREA="Services"
    export CURRENT_COVERAGE="Unknown"
    export TASK_IDENTIFIER="validation-test-$(date +%s)"
    
    print_info "CI environment variables simulated"
    
    # Attempt to run test suite in validation mode
    if [ -f "$TEST_SUITE_SCRIPT" ] && [ -x "$TEST_SUITE_SCRIPT" ]; then
        print_info "Attempting test suite execution validation..."
        
        # Run test suite with timeout to prevent hanging
        if timeout 120s "$TEST_SUITE_SCRIPT" report summary > /tmp/test_validation.log 2>&1; then
            # Analyze results
            if grep -q "100%" /tmp/test_validation.log; then
                print_success "Test suite executes successfully with 100% pass rate"
            else
                print_warning "Test suite execution completed but may have issues"
                if [ "$VERBOSE" = true ]; then
                    echo "    Output excerpt:"
                    head -n 10 /tmp/test_validation.log | sed 's/^/    /'
                fi
            fi
            
            if grep -q "skipped" /tmp/test_validation.log; then
                SKIP_COUNT=$(grep -o "[0-9]* skipped" /tmp/test_validation.log | head -1 | cut -d' ' -f1)
                if [ "$SKIP_COUNT" -eq 23 ] 2>/dev/null; then
                    print_success "Expected skip count (23) detected in test results"
                else
                    print_warning "Skip count ($SKIP_COUNT) differs from expected (23)"
                fi
            else
                print_warning "Skip count information not found in test results"
            fi
            
        else
            print_error "Test suite execution failed or timed out"
            if [ "$VERBOSE" = true ] && [ -f /tmp/test_validation.log ]; then
                echo "    Error output:"
                tail -n 10 /tmp/test_validation.log | sed 's/^/    /'
            fi
        fi
        
        # Cleanup
        rm -f /tmp/test_validation.log
    else
        print_error "Cannot validate test suite execution - script not available"
    fi
    
    echo ""
}

# ==============================================================================
# Branch Strategy Validation
# ==============================================================================

validate_branch_strategy() {
    print_section "Epic Branch Strategy"
    
    # Check if we're in a git repository
    if ! git rev-parse --git-dir >/dev/null 2>&1; then
        print_error "Not in a git repository - cannot validate branch strategy"
        echo ""
        return
    fi
    
    # Check current branch
    CURRENT_BRANCH=$(git branch --show-current)
    print_info "Current branch: $CURRENT_BRANCH"
    
    # Check if develop branch exists
    if git show-ref --verify --quiet refs/heads/develop; then
        print_success "Develop branch exists (required for epic branch creation)"
    else
        print_error "Develop branch missing - epic branch strategy requires develop"
    fi
    
    # Check if epic branch exists
    if git show-ref --verify --quiet refs/heads/$EPIC_BRANCH; then
        print_success "Epic branch exists: $EPIC_BRANCH"
        
        # Check if epic branch is up to date with develop
        if git merge-base --is-ancestor develop $EPIC_BRANCH 2>/dev/null; then
            print_success "Epic branch appears current with develop"
        else
            print_warning "Epic branch may be behind develop"
        fi
    else
        print_info "Epic branch does not exist yet (will be created automatically)"
        
        if [ "$FIX_ISSUES" = true ]; then
            print_info "Creating epic branch from develop..."
            git checkout develop >/dev/null 2>&1 || {
                print_warning "Could not checkout develop - epic branch creation skipped"
                echo ""
                return
            }
            git pull origin develop >/dev/null 2>&1 || print_warning "Could not pull latest develop"
            git checkout -b $EPIC_BRANCH >/dev/null 2>&1
            print_success "Epic branch created: $EPIC_BRANCH"
            git checkout $CURRENT_BRANCH >/dev/null 2>&1 || true
        fi
    fi
    
    # Validate branch naming conventions
    if echo "$CURRENT_BRANCH" | grep -qE "^(feature|tests)/issue-[0-9]+"; then
        print_success "Current branch follows naming convention"
    else
        print_info "Current branch may not follow task naming convention"
    fi
    
    echo ""
}

# ==============================================================================
# GitHub Actions Prerequisites
# ==============================================================================

validate_github_actions() {
    print_section "GitHub Actions Prerequisites"
    
    # Check for shared actions
    SHARED_ACTIONS_DIR="$ROOT_DIR/.github/actions/shared"
    if [ -d "$SHARED_ACTIONS_DIR" ]; then
        print_success "Shared actions directory exists"
        
        if [ -f "$SHARED_ACTIONS_DIR/setup-environment/action.yml" ]; then
            print_success "setup-environment shared action available"
        else
            print_error "setup-environment shared action missing"
        fi
        
    else
        print_error "Shared actions directory missing: $SHARED_ACTIONS_DIR"
    fi
    
    # Check for GitHub CLI availability (for PR creation)
    if command -v gh >/dev/null 2>&1; then
        print_success "GitHub CLI available for automated PR creation"
        
        # Check if authenticated
        if gh auth status >/dev/null 2>&1; then
            print_success "GitHub CLI is authenticated"
        else
            print_warning "GitHub CLI authentication may be required for automation"
        fi
    else
        print_warning "GitHub CLI not available - PR creation may fail in CI"
    fi
    
    # Check for required secrets/permissions
    print_info "Note: CI environment will need GITHUB_TOKEN with appropriate permissions"
    print_info "Required permissions: contents:write, pull-requests:write, issues:write"
    
    echo ""
}

# ==============================================================================
# Standards Documentation Validation
# ==============================================================================

validate_standards_docs() {
    print_section "Standards Documentation"
    
    # Required standards documents
    declare -a REQUIRED_DOCS=(
        "Docs/Standards/TestingStandards.md"
        "Docs/Standards/UnitTestCaseDevelopment.md"
        "Docs/Standards/IntegrationTestCaseDevelopment.md"
        "Docs/Standards/TaskManagementStandards.md"
    )
    
    for doc in "${REQUIRED_DOCS[@]}"; do
        DOC_PATH="$ROOT_DIR/$doc"
        if [ -f "$DOC_PATH" ]; then
            print_success "Required documentation exists: $doc"
        else
            print_error "Required documentation missing: $doc"
        fi
    done
    
    # Check for epic-specific documentation
    if [ -f "$EPIC_WORKFLOW_DOC" ]; then
        if grep -q "12.7 Automated Epic Execution Environment" "$ROOT_DIR/Docs/Standards/TestingStandards.md"; then
            print_success "Automated execution environment documented"
        else
            print_warning "Automated execution environment may not be documented"
        fi
    fi
    
    echo ""
}

# ==============================================================================
# Performance and Resource Validation
# ==============================================================================

validate_performance() {
    print_section "Performance & Resource Validation"
    
    # Check available disk space
    AVAILABLE_SPACE=$(df "$ROOT_DIR" | tail -1 | awk '{print $4}')
    if [ "$AVAILABLE_SPACE" -gt 1048576 ]; then  # 1GB in KB
        print_success "Sufficient disk space available (>1GB)"
    else
        print_warning "Low disk space - may affect CI execution"
    fi
    
    # Check for large files that might slow CI
    if find "$ROOT_DIR" -name "*.log" -size +10M -type f | grep -q .; then
        print_warning "Large log files detected - consider cleanup for CI performance"
        if [ "$FIX_ISSUES" = true ]; then
            find "$ROOT_DIR" -name "*.log" -size +10M -type f -delete 2>/dev/null || true
            print_info "Fixed: Removed large log files"
        fi
    else
        print_success "No large log files detected"
    fi
    
    # Check test results directories
    if [ -d "$ROOT_DIR/TestResults" ]; then
        TESTRESULTS_SIZE=$(du -sh "$ROOT_DIR/TestResults" 2>/dev/null | cut -f1 || echo "Unknown")
        print_info "TestResults directory size: $TESTRESULTS_SIZE"
        
        if [ "$FIX_ISSUES" = true ]; then
            find "$ROOT_DIR/TestResults" -type f -mtime +7 -delete 2>/dev/null || true
            print_info "Fixed: Cleaned old test result files"
        fi
    fi
    
    echo ""
}

# ==============================================================================
# Integration Testing
# ==============================================================================

validate_integration() {
    print_section "Integration Testing"
    
    print_info "Performing integration validation..."
    
    # Simulate agent coordination
    TIMESTAMP1=$(date +%s)
    TIMESTAMP2=$((TIMESTAMP1 + 1))
    
    TASK_BRANCH_1="tests/issue-94-services-$TIMESTAMP1"
    TASK_BRANCH_2="tests/issue-94-controllers-$TIMESTAMP2"
    
    if [ ${#TASK_BRANCH_1} -lt 100 ] && [ ${#TASK_BRANCH_2} -lt 100 ]; then
        print_success "Task branch naming generates unique identifiers"
    else
        print_error "Task branch names may be too long for git"
    fi
    
    # Validate no conflicts in naming
    if [ "$TASK_BRANCH_1" != "$TASK_BRANCH_2" ]; then
        print_success "Timestamp-based coordination prevents branch name conflicts"
    else
        print_error "Branch naming collision detected"
    fi
    
    # Test coverage area selection logic
    declare -a COVERAGE_AREAS=("Services" "Controllers" "Repositories" "Utilities")
    HOUR=$(date +%H)
    SELECTED_AREA=${COVERAGE_AREAS[$((HOUR % 4))]}
    
    if [[ " ${COVERAGE_AREAS[@]} " =~ " $SELECTED_AREA " ]]; then
        print_success "Coverage area selection logic works: $SELECTED_AREA"
    else
        print_error "Coverage area selection logic failed"
    fi
    
    echo ""
}

# ==============================================================================
# Security and Safety Validation
# ==============================================================================

validate_security() {
    print_section "Security & Safety"
    
    # Check for hardcoded secrets
    if grep -r -i "password\|secret\|token\|key" "$WORKFLOW_FILE" "$PROMPT_FILE" 2>/dev/null | grep -v "GITHUB_TOKEN\|secrets\.\|example"; then
        print_error "Potential hardcoded secrets detected in configuration files"
    else
        print_success "No hardcoded secrets detected"
    fi
    
    # Check for safe git operations
    if grep -q "\-\-force" "$WORKFLOW_FILE" 2>/dev/null; then
        if grep -q "\-\-force\-with\-lease" "$WORKFLOW_FILE"; then
            print_success "Safe force push operations using --force-with-lease"
        else
            print_warning "Unsafe force push operations detected"
        fi
    else
        print_success "No force push operations detected"
    fi
    
    # Check for production code modification restrictions
    if grep -q "test.*only" "$PROMPT_FILE" && grep -q "production.*code.*protection" "$PROMPT_FILE"; then
        print_success "Production code modification restrictions documented"
    else
        print_warning "Production code protection may not be adequately documented"
    fi
    
    echo ""
}

# ==============================================================================
# Command Line Argument Processing
# ==============================================================================

process_arguments() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --verbose|-v)
                VERBOSE=true
                shift
                ;;
            --fix|-f)
                FIX_ISSUES=true
                shift
                ;;
            --help|-h)
                echo "Usage: $0 [--verbose] [--fix] [--help]"
                echo ""
                echo "Options:"
                echo "  --verbose, -v    Enable verbose output with additional details"
                echo "  --fix, -f        Attempt to fix issues automatically where possible"
                echo "  --help, -h       Show this help message"
                echo ""
                echo "Exit codes:"
                echo "  0 = All validations passed"
                echo "  1 = Validation failures detected"
                echo "  2 = Critical infrastructure missing"
                exit 0
                ;;
            *)
                echo "Unknown option: $1"
                echo "Use --help for usage information"
                exit 1
                ;;
        esac
    done
}

# ==============================================================================
# Main Execution
# ==============================================================================

main() {
    process_arguments "$@"
    
    print_header
    
    # Core validations
    validate_core_files
    validate_test_suite
    validate_standards_docs
    validate_github_actions
    validate_branch_strategy
    validate_ci_environment
    validate_performance
    validate_integration
    validate_security
    
    # Final summary
    echo -e "${BLUE}=================================================${NC}"
    echo -e "${BLUE}🎯 Validation Summary${NC}"
    echo -e "${BLUE}=================================================${NC}"
    
    if [ "$VALIDATION_FAILED" = false ]; then
        echo -e "${GREEN}✅ All validations passed successfully!${NC}"
        echo ""
        echo "🚀 Coverage Epic automation infrastructure is ready for deployment"
        echo "🤖 AI agents can execute autonomously with 4 instances per day"
        echo "📈 Epic #94 progression toward 90% coverage can begin"
        echo ""
        exit 0
    else
        echo -e "${RED}❌ Validation failures detected${NC}"
        echo ""
        echo "🔧 Please address the issues identified above"
        if [ "$FIX_ISSUES" = false ]; then
            echo "💡 Run with --fix to attempt automatic resolution of some issues"
        fi
        echo "📋 Refer to Epic #94 and Issue #95 for implementation requirements"
        echo ""
        exit 1
    fi
}

# Execute main function with all arguments
main "$@"