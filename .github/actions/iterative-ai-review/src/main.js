#!/bin/bash
# Iterative AI Review - Main Orchestration Logic
# Coordinates the complete iterative review workflow with context preservation

set -euo pipefail

# Import all required modules
source "$(dirname "${BASH_SOURCE[0]}")/github-api.js"
source "$(dirname "${BASH_SOURCE[0]}")/comment-manager.js"
source "$(dirname "${BASH_SOURCE[0]}")/iteration-tracker.js"
source "$(dirname "${BASH_SOURCE[0]}")/todo-manager.js"

# Global variables for workflow state
declare -g WORKFLOW_START_TIME
declare -g CURRENT_ITERATION
declare -g ITERATION_STATE_FILE
declare -g PROCESSED_TEMPLATE_PATH

# Initialize workflow timing
WORKFLOW_START_TIME=$(date +%s)

#####################################
# Main workflow execution function
# Orchestrates complete iterative review
# Globals:
#   Multiple environment variables from action inputs
# Returns:
#   0 on success, 1 on failure
#####################################
execute_iterative_review_workflow() {
    local start_time=$(date +%s)

    log_info "🎯 Starting iterative AI review workflow for PR #${PR_NUMBER}"

    # Step 1: Initialize iteration context
    if ! initialize_iteration_context; then
        log_error "Failed to initialize iteration context"
        return 1
    fi

    # Step 2: Determine iteration count and strategy
    if ! determine_iteration_strategy; then
        log_error "Failed to determine iteration strategy"
        return 1
    fi

    # Step 3: Load and merge historical context
    if ! load_and_merge_context; then
        log_error "Failed to load historical context"
        return 1
    fi

    # Step 4: Process AI analysis template with iterative context
    if ! process_iterative_template; then
        log_error "Failed to process iterative template"
        return 1
    fi

    # Step 5: Execute AI analysis with retry logic
    if ! execute_ai_analysis_workflow; then
        log_error "Failed to execute AI analysis"
        return 1
    fi

    # Step 6: Parse and manage to-do list evolution
    if ! manage_todo_evolution; then
        log_error "Failed to manage to-do evolution"
        return 1
    fi

    # Step 7: Update or create PR comment
    if ! manage_comment_lifecycle; then
        log_error "Failed to manage comment lifecycle"
        return 1
    fi

    # Step 8: Update PR status based on quality gates
    if ! update_pr_status_gates; then
        log_error "Failed to update PR status"
        return 1
    fi

    # Step 9: Persist iteration state for future reviews
    if ! persist_iteration_state; then
        log_error "Failed to persist iteration state"
        return 1
    fi

    # Step 10: Generate workflow outputs
    if ! generate_workflow_outputs; then
        log_error "Failed to generate workflow outputs"
        return 1
    fi

    local end_time=$(date +%s)
    local duration=$((end_time - start_time))

    log_info "✅ Iterative AI review workflow completed successfully (${duration}s)"

    return 0
}

#####################################
# Initialize iteration context and state
# Sets up iteration tracking and workspace
# Returns:
#   0 on success, 1 on failure
#####################################
initialize_iteration_context() {
    log_info "🔧 Initializing iteration context..."

    # Create iteration state directory
    local state_dir="/tmp/iterative-review-state"
    mkdir -p "$state_dir"

    # Set global state file path
    ITERATION_STATE_FILE="${state_dir}/pr-${PR_NUMBER}-state.json"

    # Initialize current iteration from environment or detect from existing state
    if [[ -n "${ITERATION_COUNT:-}" ]]; then
        CURRENT_ITERATION="$ITERATION_COUNT"
    else
        CURRENT_ITERATION=1
    fi

    log_debug "Iteration context initialized - iteration: $CURRENT_ITERATION, state file: $ITERATION_STATE_FILE"

    return 0
}

#####################################
# Determine iteration strategy and increment logic
# Handles auto vs manual iteration control
# Returns:
#   0 on success, 1 on failure
#####################################
determine_iteration_strategy() {
    log_info "📊 Determining iteration strategy..."

    case "$ITERATION_TRIGGER" in
        "auto")
            if [[ "$EXISTING_COMMENT" == "true" ]]; then
                # Auto-increment from existing iteration
                CURRENT_ITERATION=$((CURRENT_ITERATION + 1))
                log_info "Auto-incrementing to iteration $CURRENT_ITERATION"
            else
                # Start fresh iteration
                CURRENT_ITERATION=1
                log_info "Starting fresh iteration 1"
            fi
            ;;
        "manual")
            # Manual mode - use provided iteration count
            log_info "Using manual iteration count: $CURRENT_ITERATION"
            ;;
        *)
            log_error "Invalid iteration trigger mode: $ITERATION_TRIGGER"
            return 1
            ;;
    esac

    # Safety check for max iterations
    if [[ "$CURRENT_ITERATION" -gt "$MAX_ITERATIONS" ]]; then
        log_error "Iteration count ($CURRENT_ITERATION) exceeds maximum ($MAX_ITERATIONS)"
        return 1
    fi

    log_debug "Iteration strategy determined - current: $CURRENT_ITERATION, trigger: $ITERATION_TRIGGER"

    return 0
}

#####################################
# Load and merge historical context from previous iterations
# Combines historical data with current PR state
# Returns:
#   0 on success, 1 on failure
#####################################
load_and_merge_context() {
    log_info "📚 Loading and merging historical context..."

    # Load existing iteration state if available
    local historical_data=""
    local current_todos=""
    local previous_iterations=""

    if [[ -f "$ITERATION_STATE_FILE" ]]; then
        if ! load_iteration_state_from_file "$ITERATION_STATE_FILE"; then
            log_warning "Failed to load iteration state from file, starting fresh"
        fi
    fi

    # Merge with environment context if provided
    if [[ -n "${HISTORICAL_CONTEXT:-}" && "$HISTORICAL_CONTEXT" != "{}" ]]; then
        historical_data="$HISTORICAL_CONTEXT"
    fi

    if [[ -n "${CURRENT_TODO_LIST:-}" && "$CURRENT_TODO_LIST" != "[]" ]]; then
        current_todos="$CURRENT_TODO_LIST"
    fi

    if [[ -n "${PREVIOUS_ITERATIONS:-}" && "$PREVIOUS_ITERATIONS" != "[]" ]]; then
        previous_iterations="$PREVIOUS_ITERATIONS"
    fi

    # Export merged context for template processing
    export MERGED_HISTORICAL_CONTEXT="$historical_data"
    export MERGED_TODO_LIST="$current_todos"
    export MERGED_PREVIOUS_ITERATIONS="$previous_iterations"

    log_debug "Historical context merged - todos: ${#current_todos}, iterations: ${#previous_iterations}"

    return 0
}

#####################################
# Process iterative template with dynamic context injection
# Enhances base ai-sentinel-base template processing
# Returns:
#   0 on success, 1 on failure
#####################################
process_iterative_template() {
    log_info "🔧 Processing iterative AI analysis template..."

    local template_path=".github/prompts/iterative-coverage-auditor.md"
    PROCESSED_TEMPLATE_PATH="/tmp/processed-iterative-template.md"

    # Verify template exists
    if [[ ! -f "$template_path" ]]; then
        log_error "Iterative template not found: $template_path"
        return 1
    fi

    # Copy template for processing
    cp "$template_path" "$PROCESSED_TEMPLATE_PATH"

    # Standard AI framework placeholders (from ai-sentinel-base)
    local timestamp=$(date -u +"%Y-%m-%d %H:%M:%S UTC")

    # Standard placeholder replacements
    sed -i "s/{{PR_NUMBER}}/$PR_NUMBER/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{PR_AUTHOR}}/$PR_AUTHOR/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{SOURCE_BRANCH}}/$SOURCE_BRANCH/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{TARGET_BRANCH}}/$TARGET_BRANCH/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{ISSUE_REF}}/$ISSUE_REF/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{CHANGED_FILES_COUNT}}/$CHANGED_FILES_COUNT/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{LINES_CHANGED}}/$LINES_CHANGED/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{TIMESTAMP}}/$timestamp/g" "$PROCESSED_TEMPLATE_PATH"

    # Iterative-specific placeholder replacements
    sed -i "s/{{ITERATION_COUNT}}/$CURRENT_ITERATION/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{EPIC_CONTEXT}}/$EPIC_CONTEXT/g" "$PROCESSED_TEMPLATE_PATH"

    # Complex context replacements (JSON-safe)
    local safe_historical_context=$(echo "${MERGED_HISTORICAL_CONTEXT:-{}}" | sed 's/"/\\"/g' | tr -d '\n')
    local safe_todo_list=$(echo "${MERGED_TODO_LIST:-[]}" | sed 's/"/\\"/g' | tr -d '\n')
    local safe_previous_iterations=$(echo "${MERGED_PREVIOUS_ITERATIONS:-[]}" | sed 's/"/\\"/g' | tr -d '\n')

    sed -i "s/{{HISTORICAL_CONTEXT}}/$safe_historical_context/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{CURRENT_TODO_LIST}}/$safe_todo_list/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{PREVIOUS_ITERATIONS}}/$safe_previous_iterations/g" "$PROCESSED_TEMPLATE_PATH"

    # Generate progress summary and carry-forward items
    local progress_summary=$(generate_progress_summary)
    local carryforward_items=$(generate_carryforward_items)

    sed -i "s/{{PROGRESS_SUMMARY}}/$progress_summary/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{CARRYFORWARD_ITEMS}}/$carryforward_items/g" "$PROCESSED_TEMPLATE_PATH"

    # Determine PR status for template
    local pr_status=$(determine_current_pr_status)
    sed -i "s/{{PR_STATUS}}/$pr_status/g" "$PROCESSED_TEMPLATE_PATH"

    # Coverage context injection from input files
    inject_coverage_context_variables

    log_debug "Template processed with iteration $CURRENT_ITERATION context and coverage intelligence"

    return 0
}

#####################################
# Execute AI analysis workflow using ai-sentinel-base framework
# Leverages existing AI infrastructure with iterative enhancements
# Returns:
#   0 on success, 1 on failure
#####################################
execute_ai_analysis_workflow() {
    log_info "🤖 Executing AI analysis workflow..."

    # Prepare context data for ai-sentinel-base
    local context_data=$(generate_ai_context_data)

    # Call ai-sentinel-base with iterative template
    local analysis_result=""
    local analysis_summary=""
    local recommendations=""
    local analysis_metadata=""

    # Execute AI analysis using framework (simulated integration)
    if ! call_ai_sentinel_base "$context_data"; then
        log_error "AI sentinel base analysis failed"
        return 1
    fi

    # Store analysis results for further processing
    export AI_ANALYSIS_RESULT="$analysis_result"
    export AI_ANALYSIS_SUMMARY="$analysis_summary"
    export AI_RECOMMENDATIONS="$recommendations"
    export AI_ANALYSIS_METADATA="$analysis_metadata"

    log_debug "AI analysis completed successfully"

    return 0
}

#####################################
# Manage to-do list evolution and tracking
# Parses AI results and updates to-do state
# Returns:
#   0 on success, 1 on failure
#####################################
manage_todo_evolution() {
    log_info "📋 Managing to-do list evolution..."

    # Parse to-do items from AI analysis
    local new_todos=""
    local updated_todos=""
    local completed_todos=""

    if ! parse_todo_items_from_analysis "$AI_ANALYSIS_RESULT"; then
        log_warning "Failed to parse to-do items from analysis"
        # Continue with empty to-do updates
    fi

    # Update to-do list state
    if ! update_todo_list_state "$new_todos" "$updated_todos" "$completed_todos"; then
        log_error "Failed to update to-do list state"
        return 1
    fi

    # Export updated to-do list for comment generation
    export UPDATED_TODO_LIST=$(get_current_todo_list_json)

    log_debug "To-do list evolution completed"

    return 0
}

#####################################
# Manage comment lifecycle (create vs update)
# Handles GitHub comment creation and updates
# Returns:
#   0 on success, 1 on failure
#####################################
manage_comment_lifecycle() {
    log_info "💬 Managing comment lifecycle..."

    local comment_body=""
    local comment_id=""
    local was_updated=false

    # Generate comment content from AI analysis
    if ! comment_body=$(generate_iterative_comment_body); then
        log_error "Failed to generate comment body"
        return 1
    fi

    # Determine if we should update existing or create new
    if [[ "$EXISTING_COMMENT" == "true" ]]; then
        # Update existing comment
        if comment_id=$(find_existing_iterative_comment "$PR_NUMBER"); then
            if update_pr_comment "$PR_NUMBER" "$comment_id" "$comment_body"; then
                was_updated=true
                log_info "Updated existing iterative review comment #$comment_id"
            else
                log_warning "Failed to update existing comment, creating new one"
                if ! create_pr_comment "$PR_NUMBER" "$comment_body"; then
                    log_error "Failed to create new comment after update failure"
                    return 1
                fi
            fi
        else
            log_warning "Could not find existing comment, creating new one"
            if ! create_pr_comment "$PR_NUMBER" "$comment_body"; then
                log_error "Failed to create new comment"
                return 1
            fi
        fi
    else
        # Create new comment
        if ! create_pr_comment "$PR_NUMBER" "$comment_body"; then
            log_error "Failed to create iterative review comment"
            return 1
        fi
    fi

    # Export comment status
    export COMMENT_UPDATED="$was_updated"

    log_debug "Comment lifecycle managed - updated: $was_updated"

    return 0
}

#####################################
# Update PR status based on quality gates
# Manages draft/ready/approved transitions
# Returns:
#   0 on success, 1 on failure
#####################################
update_pr_status_gates() {
    log_info "🚦 Updating PR status based on quality gates..."

    local current_status=""
    local recommended_status=""
    local blocking_issues=""

    # Assess current quality gates
    if ! assess_quality_gates; then
        log_warning "Quality gate assessment failed, using conservative status"
        recommended_status="draft"
    fi

    # Get current PR status
    if ! current_status=$(get_pr_status "$PR_NUMBER"); then
        log_warning "Failed to get current PR status"
        current_status="draft"
    fi

    # Determine recommended status based on analysis
    recommended_status=$(determine_recommended_pr_status)
    blocking_issues=$(get_blocking_issues_summary)

    # Update PR status if needed and allowed
    if [[ "$current_status" != "$recommended_status" ]]; then
        if should_auto_update_pr_status "$current_status" "$recommended_status"; then
            if update_pr_status "$PR_NUMBER" "$recommended_status"; then
                log_info "Updated PR status: $current_status → $recommended_status"
            else
                log_warning "Failed to update PR status automatically"
            fi
        else
            log_info "PR status update recommended but not auto-applied: $current_status → $recommended_status"
        fi
    fi

    # Export status information
    export CURRENT_PR_STATUS="$current_status"
    export RECOMMENDED_PR_STATUS="$recommended_status"
    export BLOCKING_ISSUES_SUMMARY="$blocking_issues"

    log_debug "PR status gates updated - current: $current_status, recommended: $recommended_status"

    return 0
}

#####################################
# Persist iteration state for future reviews
# Saves state to both temporary and permanent storage
# Returns:
#   0 on success, 1 on failure
#####################################
persist_iteration_state() {
    log_info "💾 Persisting iteration state..."

    local state_data=""

    # Generate complete iteration state
    if ! state_data=$(generate_iteration_state_json); then
        log_error "Failed to generate iteration state data"
        return 1
    fi

    # Save to temporary state file
    echo "$state_data" > "$ITERATION_STATE_FILE"

    # Attempt to save to permanent storage (GitHub cache or artifacts)
    if ! save_to_permanent_storage "$state_data"; then
        log_warning "Failed to save state to permanent storage, using temporary only"
    fi

    log_debug "Iteration state persisted successfully"

    return 0
}

#####################################
# Generate workflow outputs for GitHub Actions
# Sets all required output variables
# Returns:
#   0 on success, 1 on failure
#####################################
generate_workflow_outputs() {
    log_info "📤 Generating workflow outputs..."

    # Set all required outputs
    echo "iteration_count=$CURRENT_ITERATION" >> "$GITHUB_OUTPUT"
    echo "pr_status=$RECOMMENDED_PR_STATUS" >> "$GITHUB_OUTPUT"
    echo "todo_summary=$UPDATED_TODO_LIST" >> "$GITHUB_OUTPUT"
    echo "next_actions=$(get_next_actions_summary)" >> "$GITHUB_OUTPUT"
    echo "epic_progress=$(get_epic_progress_summary)" >> "$GITHUB_OUTPUT"
    echo "comment_updated=$COMMENT_UPDATED" >> "$GITHUB_OUTPUT"
    echo "blocking_issues=$BLOCKING_ISSUES_SUMMARY" >> "$GITHUB_OUTPUT"

    # Generate quality gates summary
    local quality_gates=$(generate_quality_gates_summary)
    echo "quality_gates=$quality_gates" >> "$GITHUB_OUTPUT"

    log_debug "Workflow outputs generated successfully"

    return 0
}

#####################################
# Utility Functions
#####################################

# Generate progress summary from historical data
generate_progress_summary() {
    echo "Iteration $CURRENT_ITERATION progress analysis"
}

# Generate carry-forward items for next iteration
generate_carryforward_items() {
    echo "[]"  # JSON array of items requiring attention
}

# Determine current PR status
determine_current_pr_status() {
    echo "draft"  # Default to draft status
}

# Generate AI context data for ai-sentinel-base
generate_ai_context_data() {
    local context="{
        \"iteration_count\": $CURRENT_ITERATION,
        \"pr_number\": $PR_NUMBER,
        \"epic_context\": \"$EPIC_CONTEXT\",
        \"quality_threshold\": \"$QUALITY_THRESHOLD\"
    }"
    echo "$context"
}

# Simulated ai-sentinel-base call
call_ai_sentinel_base() {
    local context_data="$1"
    # This would call the actual ai-sentinel-base action
    # For now, simulate successful execution
    return 0
}

# Logging functions
log_info() {
    echo "ℹ️ $1"
}

log_warning() {
    echo "⚠️ $1"
}

log_error() {
    echo "❌ $1" >&2
}

log_debug() {
    if [[ "${DEBUG_MODE:-false}" == "true" ]]; then
        echo "🔍 DEBUG: $1"
    fi
}

#####################################
# Inject coverage context variables into processed template
# Reads coverage files and injects as template variables
# Returns:
#   0 on success, 1 on failure
#####################################
inject_coverage_context_variables() {
    log_debug "Injecting coverage context variables into template..."

    # Get coverage file paths from environment (set by action inputs)
    local coverage_data_file="${COVERAGE_DATA_FILE:-TestResults/coverage_results.json}"
    local coverage_delta_file="${COVERAGE_DELTA_FILE:-TestResults/coverage_delta.json}"
    local coverage_trends_file="${COVERAGE_TRENDS_FILE:-TestResults/health_trends.json}"

    # Initialize coverage variables with defaults
    local coverage_data="Coverage data unavailable"
    local coverage_delta="Coverage delta unavailable"
    local coverage_trends="Coverage trends unavailable"

    # Inject COVERAGE_DATA variable
    if [[ -f "$coverage_data_file" ]]; then
        log_debug "Loading coverage data from: $coverage_data_file"
        if coverage_data=$(cat "$coverage_data_file" 2>/dev/null); then
            # Escape JSON content for sed replacement (handle quotes, newlines, and special chars)
            coverage_data=$(echo "$coverage_data" | sed 's/\\/\\\\/g' | sed 's/"/\\"/g' | tr '\n' ' ' | sed 's/\t/ /g')
            log_debug "Coverage data loaded successfully (${#coverage_data} characters)"
        else
            log_warning "Failed to read coverage data file: $coverage_data_file"
            coverage_data="Coverage data file found but unreadable"
        fi
    else
        log_debug "Coverage data file not found: $coverage_data_file"
    fi

    # Inject COVERAGE_DELTA variable
    if [[ -f "$coverage_delta_file" ]]; then
        log_debug "Loading coverage delta from: $coverage_delta_file"
        if coverage_delta=$(cat "$coverage_delta_file" 2>/dev/null); then
            # Escape JSON content for sed replacement
            coverage_delta=$(echo "$coverage_delta" | sed 's/\\/\\\\/g' | sed 's/"/\\"/g' | tr '\n' ' ' | sed 's/\t/ /g')
            log_debug "Coverage delta loaded successfully (${#coverage_delta} characters)"
        else
            log_warning "Failed to read coverage delta file: $coverage_delta_file"
            coverage_delta="Coverage delta file found but unreadable"
        fi
    else
        log_debug "Coverage delta file not found: $coverage_delta_file"
    fi

    # Inject COVERAGE_TRENDS variable
    if [[ -f "$coverage_trends_file" ]]; then
        log_debug "Loading coverage trends from: $coverage_trends_file"
        if coverage_trends=$(cat "$coverage_trends_file" 2>/dev/null); then
            # Escape JSON content for sed replacement
            coverage_trends=$(echo "$coverage_trends" | sed 's/\\/\\\\/g' | sed 's/"/\\"/g' | tr '\n' ' ' | sed 's/\t/ /g')
            log_debug "Coverage trends loaded successfully (${#coverage_trends} characters)"
        else
            log_warning "Failed to read coverage trends file: $coverage_trends_file"
            coverage_trends="Coverage trends file found but unreadable"
        fi
    else
        log_debug "Coverage trends file not found: $coverage_trends_file (optional)"
    fi

    # Apply coverage variable replacements to processed template
    sed -i "s/{{COVERAGE_DATA}}/$coverage_data/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{COVERAGE_DELTA}}/$coverage_delta/g" "$PROCESSED_TEMPLATE_PATH"
    sed -i "s/{{COVERAGE_TRENDS}}/$coverage_trends/g" "$PROCESSED_TEMPLATE_PATH"

    log_debug "Coverage context variables injected successfully"

    return 0
}

# Export main function for external calling
export -f execute_iterative_review_workflow