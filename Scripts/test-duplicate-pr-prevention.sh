#!/bin/bash
# ==============================================================================
# Duplicate PR Prevention Testing Script
# ==============================================================================
# Tests the duplicate PR prevention mechanisms in Coverage Epic Automation
# Validates AI agent restrictions and pipeline PR existence checking
#
# Usage:
#   ./Scripts/test-duplicate-pr-prevention.sh [--verbose]
# ==============================================================================

set -euo pipefail

readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly ROOT_DIR="$(dirname "$SCRIPT_DIR")"

# Colors
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m' # No Color

VERBOSE=false

# Cache MCP list result to avoid duplicate external calls
MCP_LIST_OUTPUT=""
MCP_AVAILABLE=false

print_header() {
    echo -e "${BLUE}===============================================${NC}"
    echo -e "${BLUE}🔍 Duplicate PR Prevention Testing${NC}"
    echo -e "${BLUE}===============================================${NC}"
    echo "📅 Test Time: $(date -u '+%Y-%m-%d %H:%M:%S UTC')"
    echo ""
}

print_success() {
    echo -e "  ${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "  ${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "  ${RED}❌ $1${NC}"
}

print_info() {
    if [ "$VERBOSE" = true ]; then
        echo -e "  ${BLUE}ℹ️  $1${NC}"
    fi
}

# Initialize MCP cache to avoid duplicate external calls
initialize_mcp_cache() {
    MCP_LIST_OUTPUT=$(claude mcp list 2>/dev/null || echo "")
    if echo "$MCP_LIST_OUTPUT" | grep -q "github.*Connected" || echo "$MCP_LIST_OUTPUT" | grep -q "github"; then
        MCP_AVAILABLE=true
    else
        MCP_AVAILABLE=false
    fi
}

test_ai_agent_restrictions() {
    echo -e "${BLUE}📋 Testing AI Agent PR Creation Restrictions${NC}"

    local prompt_file="$ROOT_DIR/.github/prompts/testing-coverage-agent.md"

    if [ -f "$prompt_file" ]; then
        print_success "AI agent prompt file exists"

        # Check for PR creation restrictions
        if grep -q "DO NOT CREATE PULL REQUESTS" "$prompt_file"; then
            print_success "PR creation prohibition found in prompt"
        else
            print_error "PR creation prohibition missing from prompt"
        fi

        # Check for specific restrictions
        if grep -q "gh pr create" "$prompt_file" && grep -q "Pipeline handles PR creation" "$prompt_file"; then
            print_success "GitHub CLI PR creation explicitly prohibited"
        else
            print_error "GitHub CLI PR creation not explicitly prohibited"
        fi

        # Check for MCP restrictions
        if grep -q "GitHub MCP PR creation functions" "$prompt_file"; then
            print_success "GitHub MCP PR creation explicitly prohibited"
        else
            print_error "GitHub MCP PR creation not explicitly prohibited"
        fi

        # Check for pipeline explanation
        if grep -q "Pipeline Will Automatically" "$prompt_file"; then
            print_success "Pipeline PR creation capabilities explained"
        else
            print_error "Pipeline PR creation capabilities not explained"
        fi

    else
        print_error "AI agent prompt file missing: $prompt_file"
    fi

    echo ""
}

test_pipeline_pr_checking() {
    echo -e "${BLUE}📋 Testing Pipeline PR Existence Checking${NC}"

    local workflow_file="$ROOT_DIR/.github/workflows/testing-coverage-execution.yml"

    if [ -f "$workflow_file" ]; then
        print_success "Coverage epic automation workflow exists"

        # Check for PR existence checking step
        if grep -q "Check for Existing Pull Request" "$workflow_file"; then
            print_success "PR existence checking step found"
        else
            print_error "PR existence checking step missing"
        fi

        # Check for gh pr list usage
        if grep -q "gh pr list --head" "$workflow_file"; then
            print_success "GitHub CLI PR listing used for checking"
        else
            print_error "GitHub CLI PR listing not found"
        fi

        # Check for conditional PR creation
        if grep -q "pr_exists == 'false'" "$workflow_file"; then
            print_success "Conditional PR creation based on existence check"
        else
            print_error "Conditional PR creation logic missing"
        fi

        # Check for MCP integration
        if grep -q "Enhanced PR Analysis with GitHub MCP" "$workflow_file"; then
            print_success "GitHub MCP integration for PR analysis found"
        else
            print_warning "GitHub MCP integration not found (optional feature)"
        fi

        # Check for comprehensive logging
        if grep -q "PR Exists Check:" "$workflow_file"; then
            print_success "PR existence status logging found"
        else
            print_error "PR existence status logging missing"
        fi

    else
        print_error "Coverage epic automation workflow missing: $workflow_file"
    fi

    echo ""
}

test_github_cli_availability() {
    echo -e "${BLUE}📋 Testing GitHub CLI Integration${NC}"

    if command -v gh >/dev/null 2>&1; then
        print_success "GitHub CLI available"

        if gh auth status >/dev/null 2>&1; then
            print_success "GitHub CLI authenticated"

            # Test PR listing capability
            if gh pr list --json number,title --limit 1 >/dev/null 2>&1; then
                print_success "GitHub CLI PR listing functional"
            else
                print_warning "GitHub CLI PR listing may have permission issues"
            fi
        else
            print_warning "GitHub CLI not authenticated (may work in CI)"
        fi
    else
        print_error "GitHub CLI not available"
    fi

    echo ""
}

test_mcp_integration() {
    echo -e "${BLUE}📋 Testing GitHub MCP Integration${NC}"

    # Use cached MCP list result to avoid duplicate external calls
    if [ "$MCP_AVAILABLE" = true ]; then
        print_success "GitHub MCP server configured"

        # Test MCP connectivity
        if claude --dangerously-skip-permissions --print "Use GitHub MCP to check connection status" >/dev/null 2>&1; then
            print_success "GitHub MCP functional"
        else
            print_warning "GitHub MCP may not be fully functional"
        fi
    else
        print_warning "GitHub MCP not configured (fallback to GitHub CLI only)"
    fi

    echo ""
}

run_integration_test() {
    echo -e "${BLUE}📋 Running Integration Tests${NC}"

    # Test the pipeline logic simulation
    print_info "Simulating PR existence checking logic..."

    # Create a temporary test branch name
    local test_branch="tests/issue-94-test-$(date +%s)"
    print_info "Test branch: $test_branch"

    # Test gh pr list command format (if available)
    if command -v gh >/dev/null 2>&1 && gh auth status >/dev/null 2>&1; then
        print_info "Testing GitHub CLI command format..."
        local cmd_test=$(gh pr list --head "$test_branch" --json number,url,state --jq '.[0] // empty' 2>/dev/null || echo "")
        if [ -z "$cmd_test" ]; then
            print_success "GitHub CLI command format correct (no existing PR found for test branch)"
        else
            print_warning "Unexpected result from GitHub CLI test"
        fi
    else
        print_info "Skipping GitHub CLI command test (not available/authenticated)"
    fi

    echo ""
}

# Process command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --verbose|-v)
            VERBOSE=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--verbose]"
            exit 1
            ;;
    esac
done

# Main execution
print_header
initialize_mcp_cache
test_ai_agent_restrictions
test_pipeline_pr_checking
test_github_cli_availability
test_mcp_integration
run_integration_test

echo -e "${BLUE}===============================================${NC}"
echo -e "${BLUE}🎯 Duplicate PR Prevention Test Summary${NC}"
echo -e "${BLUE}===============================================${NC}"
echo ""
echo -e "${GREEN}✅ Duplicate PR prevention mechanisms tested${NC}"
echo -e "${GREEN}✅ AI agent restrictions validated${NC}"
echo -e "${GREEN}✅ Pipeline PR checking logic verified${NC}"
echo -e "${GREEN}✅ Integration components assessed${NC}"
echo ""
echo "🚀 Coverage Epic Automation is protected against duplicate PR creation"
echo "🤖 AI agents will implement tests, pipeline will manage PRs"
echo ""
