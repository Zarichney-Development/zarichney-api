#!/bin/bash

# Test Script for Path Filtering Logic
# Tests the path filtering functionality without running full workflows
# Version: 1.0
# Last Updated: 2025-07-28

# Source common functions
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common-functions.sh"

# Test configuration
readonly TEST_SCENARIOS=(
    "backend-only:Code/Zarichney.Server/Controllers/RecipeController.cs"
    "frontend-only:Code/Zarichney.Website/src/app/app.component.ts"
    "docs-only:README.md Docs/Standards/CodingStandards.md"
    "pipeline-only:Scripts/Pipeline/build-backend.sh .github/workflows/build.yml"
    "mixed-changes:Code/Zarichney.Server/Program.cs Code/Zarichney.Website/package.json README.md"
    "config-only:.gitignore"
    "tests-only:Code/Zarichney.Server.Tests/Controllers/RecipeControllerTests.cs"
)

# Help function
show_help() {
    cat << EOF
Path Filtering Test Script

USAGE:
    $0 [OPTIONS]

OPTIONS:
    --scenario NAME     Run specific test scenario
    --list              List available test scenarios
    --all               Run all test scenarios [default]
    --help              Show this help message

SCENARIOS:
$(printf "    %s\n" "${TEST_SCENARIOS[@]}" | sed 's/:/ - /')

EXAMPLES:
    $0                      # Run all test scenarios
    $0 --scenario backend-only  # Test backend-only changes
    $0 --list               # List available scenarios
EOF
}

# Parse command line arguments
SCENARIO=""
LIST_SCENARIOS=false
RUN_ALL=true

while [[ $# -gt 0 ]]; do
    case $1 in
        --scenario)
            SCENARIO="$2"
            RUN_ALL=false
            shift 2
            ;;
        --list)
            LIST_SCENARIOS=true
            RUN_ALL=false
            shift
            ;;
        --all)
            RUN_ALL=true
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

# List scenarios function
list_scenarios() {
    log_section "Available Test Scenarios"
    
    for scenario in "${TEST_SCENARIOS[@]}"; do
        local name="${scenario%%:*}"
        local files="${scenario#*:}"
        log_info "Scenario: $name"
        log_info "  Files: $files"
        echo
    done
}

# Simulate file changes for testing
simulate_changes() {
    local files="$1"
    local temp_dir=$(mktemp -d -t test-changes-XXXXXX)
    
    log_info "Simulating changes to: $files"
    
    # Create temporary test files
    mkdir -p "$temp_dir"
    echo "$files" | tr ' ' '\n' > "$temp_dir/changed-files.txt"
    
    echo "$temp_dir"
}

# Test path filtering logic
test_path_filtering() {
    local scenario_name="$1"
    local files="$2"
    
    log_section "Testing Scenario: $scenario_name"
    
    # Simulate the path filtering logic from the GitHub Action
    local backend_changed=false
    local frontend_changed=false
    local docs_only=false
    local pipeline_changed=false
    local tests_changed=false
    local config_changed=false
    
    # Check each file against patterns
    while read -r file; do
        [[ -z "$file" ]] && continue
        
        case "$file" in
            Code/Zarichney.Server/*)
                backend_changed=true
                ;;
            *.sln|*.csproj)
                backend_changed=true
                ;;
            Code/Zarichney.Website/*)
                frontend_changed=true
                ;;
            Scripts/Pipeline/*|.github/workflows/*|.github/actions/*)
                pipeline_changed=true
                ;;
            *Tests/*|*.Tests/*|*Test.cs|*Tests.cs)
                tests_changed=true
                ;;
            *.md|Docs/*)
                # Only set config_changed for docs, we'll calculate docs_only later
                config_changed=true
                ;;
            .gitignore|Scripts/*.sh|Scripts/*.ps1)
                config_changed=true
                ;;
        esac
    done <<< "$(echo "$files" | tr ' ' '\n')"
    
    # Determine if only docs changed
    if [[ "$config_changed" == "true" && "$backend_changed" == "false" && "$frontend_changed" == "false" && "$pipeline_changed" == "false" && "$tests_changed" == "false" ]]; then
        # Check if all files are documentation
        local only_docs=true
        while read -r file; do
            [[ -z "$file" ]] && continue
            case "$file" in
                *.md|Docs/*)
                    # This is a doc file, continue
                    ;;
                *)
                    only_docs=false
                    break
                    ;;
            esac
        done <<< "$(echo "$files" | tr ' ' '\n')"
        
        if [[ "$only_docs" == "true" ]]; then
            docs_only=true
        fi
    fi
    
    # Display results
    log_info "Path Analysis Results:"
    log_info "  Backend Changed: $backend_changed"
    log_info "  Frontend Changed: $frontend_changed"
    log_info "  Pipeline Changed: $pipeline_changed"
    log_info "  Tests Changed: $tests_changed"
    log_info "  Config Changed: $config_changed"
    log_info "  Docs Only: $docs_only"
    
    # Determine which workflows would run
    log_info ""
    log_info "Workflow Execution Plan:"
    
    if [[ "$docs_only" == "true" ]]; then
        log_success "  01-build.yml: SKIPPED (docs only)"
        log_success "  02-quality.yml: SKIPPED (no build artifacts)"
        log_success "  03-security.yml: SKIPPED (no code changes)"
        log_success "  04-deploy.yml: SKIPPED (no deployable changes)"
    else
        # Build workflow
        if [[ "$backend_changed" == "true" || "$frontend_changed" == "true" || "$pipeline_changed" == "true" ]]; then
            log_info "  01-build.yml: RUN"
            if [[ "$backend_changed" == "true" ]]; then
                log_info "    - Backend build & test"
            fi
            if [[ "$frontend_changed" == "true" ]]; then
                log_info "    - Frontend build & test"
            fi
        else
            log_success "  01-build.yml: SKIPPED (no relevant changes)"
        fi
        
        # Quality workflow (runs on PR after build)
        if [[ "$backend_changed" == "true" || "$frontend_changed" == "true" ]]; then
            log_info "  02-quality.yml: RUN (after build completion)"
        else
            log_success "  02-quality.yml: SKIPPED (no code changes)"
        fi
        
        # Security workflow (runs on main branch)
        log_info "  03-security.yml: RUN (on main branch push)"
        
        # Deploy workflow
        if [[ "$backend_changed" == "true" || "$frontend_changed" == "true" ]]; then
            log_info "  04-deploy.yml: RUN (on main branch)"
            if [[ "$backend_changed" == "true" ]]; then
                log_info "    - Backend deployment"
            fi
            if [[ "$frontend_changed" == "true" ]]; then
                log_info "    - Frontend deployment"
            fi
        else
            log_success "  04-deploy.yml: SKIPPED (no deployable changes)"
        fi
    fi
    
    # Validate expected behavior
    validate_scenario_expectations "$scenario_name" "$backend_changed" "$frontend_changed" "$docs_only" "$pipeline_changed"
}

# Validate that the results match expected behavior for each scenario
validate_scenario_expectations() {
    local scenario="$1"
    local backend="$2"
    local frontend="$3"
    local docs_only="$4"
    local pipeline="$5"
    
    log_info ""
    log_info "Validation Results:"
    
    case "$scenario" in
        "backend-only")
            if [[ "$backend" == "true" && "$frontend" == "false" && "$docs_only" == "false" ]]; then
                log_success "  ✅ Backend-only detection: PASSED"
            else
                log_error "  ❌ Backend-only detection: FAILED"
            fi
            ;;
        "frontend-only")
            if [[ "$backend" == "false" && "$frontend" == "true" && "$docs_only" == "false" ]]; then
                log_success "  ✅ Frontend-only detection: PASSED"
            else
                log_error "  ❌ Frontend-only detection: FAILED"
            fi
            ;;
        "docs-only")
            if [[ "$docs_only" == "true" ]]; then
                log_success "  ✅ Docs-only detection: PASSED"
            else
                log_error "  ❌ Docs-only detection: FAILED"
            fi
            ;;
        "pipeline-only")
            if [[ "$pipeline" == "true" && "$backend" == "false" && "$frontend" == "false" ]]; then
                log_success "  ✅ Pipeline-only detection: PASSED"
            else
                log_error "  ❌ Pipeline-only detection: FAILED"
            fi
            ;;
        "mixed-changes")
            if [[ "$backend" == "true" && "$frontend" == "true" && "$docs_only" == "false" ]]; then
                log_success "  ✅ Mixed changes detection: PASSED"
            else
                log_error "  ❌ Mixed changes detection: FAILED"
            fi
            ;;
        *)
            log_info "  ℹ️ No specific validation for scenario: $scenario"
            ;;
    esac
}

# Main execution
main() {
    local start_time
    start_time=$(start_timer)
    
    init_pipeline
    
    log_section "Path Filtering Test Suite"
    
    if [[ "$LIST_SCENARIOS" == "true" ]]; then
        list_scenarios
        return 0
    fi
    
    if [[ "$RUN_ALL" == "true" ]]; then
        log_info "Running all test scenarios..."
        echo
        
        for scenario_def in "${TEST_SCENARIOS[@]}"; do
            local name="${scenario_def%%:*}"
            local files="${scenario_def#*:}"
            
            test_path_filtering "$name" "$files"
            echo
        done
    elif [[ -n "$SCENARIO" ]]; then
        # Find and run specific scenario
        local found=false
        for scenario_def in "${TEST_SCENARIOS[@]}"; do
            local name="${scenario_def%%:*}"
            local files="${scenario_def#*:}"
            
            if [[ "$name" == "$SCENARIO" ]]; then
                test_path_filtering "$name" "$files"
                found=true
                break
            fi
        done
        
        if [[ "$found" == "false" ]]; then
            log_error "Scenario not found: $SCENARIO"
            log_info "Available scenarios:"
            for scenario_def in "${TEST_SCENARIOS[@]}"; do
                log_info "  ${scenario_def%%:*}"
            done
            exit 1
        fi
    fi
    
    end_timer "$start_time" "path-filtering-tests"
    log_success "Path filtering tests completed"
}

# Error handling
set -euo pipefail

# Check if script is being sourced or executed
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi