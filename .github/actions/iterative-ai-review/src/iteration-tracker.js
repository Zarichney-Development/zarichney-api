#!/bin/bash
# Iteration Tracker for Iterative AI Review
# Handles context persistence, retrieval, and historical state management

set -euo pipefail

# Global variables for state management
declare -g STATE_SCHEMA_VERSION="1.0"
declare -g STATE_BASE_DIR="/tmp/iterative-review-state"
declare -g PERMANENT_STATE_DIR=".github/cache/iterative-review"

#####################################
# State Management Functions
#####################################

#####################################
# Initialize iteration context and workspace
# Sets up state directories and validates environment
# Returns:
#   0 on success, 1 on failure
#####################################
initialize_iteration_workspace() {
    log_debug "Initializing iteration workspace"

    # Create temporary state directory
    mkdir -p "$STATE_BASE_DIR"

    # Create permanent state directory if it doesn't exist
    mkdir -p "$PERMANENT_STATE_DIR"

    # Validate required environment variables
    if [[ -z "${PR_NUMBER:-}" ]]; then
        log_error "PR_NUMBER environment variable is required"
        return 1
    fi

    log_debug "Iteration workspace initialized successfully"
    return 0
}

#####################################
# Load iteration context from storage
# Arguments:
#   $1 - PR number
# Returns:
#   0 on success, 1 on failure
#####################################
load_iteration_context() {
    local pr_number="$1"

    log_debug "Loading iteration context for PR #$pr_number"

    # Try to load from permanent storage first
    local permanent_state_file="${PERMANENT_STATE_DIR}/pr-${pr_number}-state.json"
    local temp_state_file="${STATE_BASE_DIR}/pr-${pr_number}-state.json"

    local state_data=""

    # Check permanent storage
    if [[ -f "$permanent_state_file" ]]; then
        log_debug "Loading state from permanent storage: $permanent_state_file"
        if state_data=$(cat "$permanent_state_file" 2>/dev/null); then
            if validate_state_schema "$state_data"; then
                parse_and_export_state "$state_data"
                return 0
            else
                log_warning "Invalid state schema in permanent storage, trying temporary"
            fi
        fi
    fi

    # Check temporary storage
    if [[ -f "$temp_state_file" ]]; then
        log_debug "Loading state from temporary storage: $temp_state_file"
        if state_data=$(cat "$temp_state_file" 2>/dev/null); then
            if validate_state_schema "$state_data"; then
                parse_and_export_state "$state_data"
                return 0
            else
                log_warning "Invalid state schema in temporary storage"
            fi
        fi
    fi

    # No valid state found, initialize fresh
    log_info "No existing iteration state found, initializing fresh context"
    initialize_fresh_state "$pr_number"
    return 0
}

#####################################
# Load iteration state from specific file
# Arguments:
#   $1 - State file path
# Returns:
#   0 on success, 1 on failure
#####################################
load_iteration_state_from_file() {
    local state_file="$1"

    log_debug "Loading iteration state from file: $state_file"

    if [[ ! -f "$state_file" ]]; then
        log_debug "State file does not exist: $state_file"
        return 1
    fi

    local state_data
    if ! state_data=$(cat "$state_file" 2>/dev/null); then
        log_error "Failed to read state file: $state_file"
        return 1
    fi

    if ! validate_state_schema "$state_data"; then
        log_error "Invalid state schema in file: $state_file"
        return 1
    fi

    parse_and_export_state "$state_data"
    return 0
}

#####################################
# Validate state data schema
# Arguments:
#   $1 - State data JSON
# Returns:
#   0 if valid, 1 if invalid
#####################################
validate_state_schema() {
    local state_data="$1"

    # Check if it's valid JSON
    if ! echo "$state_data" | jq . >/dev/null 2>&1; then
        log_debug "State data is not valid JSON"
        return 1
    fi

    # Check required fields
    local schema_version
    schema_version=$(echo "$state_data" | jq -r '.schema_version // empty')

    if [[ -z "$schema_version" ]]; then
        log_debug "Missing schema_version in state data"
        return 1
    fi

    local pr_number
    pr_number=$(echo "$state_data" | jq -r '.pr_number // empty')

    if [[ -z "$pr_number" ]]; then
        log_debug "Missing pr_number in state data"
        return 1
    fi

    log_debug "State schema validation passed"
    return 0
}

#####################################
# Parse state data and export to environment
# Arguments:
#   $1 - State data JSON
# Returns:
#   0 on success, 1 on failure
#####################################
parse_and_export_state() {
    local state_data="$1"

    log_debug "Parsing and exporting state data"

    # Extract key values
    local iteration_count
    iteration_count=$(echo "$state_data" | jq -r '.iteration_count // 1')

    local current_status
    current_status=$(echo "$state_data" | jq -r '.current_status // "draft"')

    local historical_context
    historical_context=$(echo "$state_data" | jq -c '.historical_context // {}')

    local current_todo_list
    current_todo_list=$(echo "$state_data" | jq -c '.current_todo_list // []')

    local previous_iterations
    previous_iterations=$(echo "$state_data" | jq -c '.historical_context.iteration_history // []')

    # Export to environment for action workflow
    echo "iteration_count=$iteration_count" >> "$GITHUB_OUTPUT"
    echo "historical_context=$historical_context" >> "$GITHUB_OUTPUT"
    echo "current_todo_list=$current_todo_list" >> "$GITHUB_OUTPUT"
    echo "previous_iterations=$previous_iterations" >> "$GITHUB_OUTPUT"

    # Export as variables for scripts
    export ITERATION_COUNT="$iteration_count"
    export CURRENT_STATUS="$current_status"
    export HISTORICAL_CONTEXT="$historical_context"
    export CURRENT_TODO_LIST="$current_todo_list"
    export PREVIOUS_ITERATIONS="$previous_iterations"

    log_debug "State data parsed and exported successfully"
    return 0
}

#####################################
# Initialize fresh state for new PR
# Arguments:
#   $1 - PR number
# Returns:
#   0 on success, 1 on failure
#####################################
initialize_fresh_state() {
    local pr_number="$1"

    log_debug "Initializing fresh state for PR #$pr_number"

    local timestamp=$(date -u --iso-8601=seconds)

    # Create initial state structure
    local fresh_state='{
  "schema_version": "'${STATE_SCHEMA_VERSION}'",
  "pr_number": '${pr_number}',
  "iteration_count": 1,
  "current_status": "draft",
  "created_at": "'${timestamp}'",
  "updated_at": "'${timestamp}'",
  "historical_context": {
    "iteration_history": [],
    "progress_metrics": {
      "total_todos_created": 0,
      "total_todos_resolved": 0,
      "current_active_todos": 0,
      "avg_resolution_time": null
    }
  },
  "current_todo_list": [],
  "pr_metadata": {
    "initial_files_changed": '${CHANGED_FILES_COUNT:-0}',
    "initial_lines_changed": '${LINES_CHANGED:-0}',
    "epic_context": "'${EPIC_CONTEXT:-Epic #181 autonomous development}'",
    "quality_gates": {
      "tests_passing": null,
      "coverage_threshold": null,
      "security_scan": null,
      "standards_compliance": null
    }
  },
  "context_for_next_iteration": {
    "focus_areas": ["initial_analysis"],
    "carry_forward_items": [],
    "escalation_triggers": [],
    "epic_progress_notes": "Starting iterative review process"
  }
}'

    # Parse and export the fresh state
    parse_and_export_state "$fresh_state"

    log_debug "Fresh state initialized successfully"
    return 0
}

#####################################
# State Persistence Functions
#####################################

#####################################
# Generate current iteration state JSON
# Returns:
#   Prints state JSON, 0 on success, 1 on failure
#####################################
generate_iteration_state_json() {
    log_debug "Generating current iteration state JSON"

    local timestamp=$(date -u --iso-8601=seconds)

    # Build iteration history entry
    local current_iteration_entry='{
  "iteration": '${CURRENT_ITERATION}',
  "timestamp": "'${timestamp}'",
  "summary": "Iteration '${CURRENT_ITERATION}' analysis completed",
  "todo_count": '$(get_current_todo_count)',
  "resolved_count": '$(get_resolved_todo_count)',
  "pr_status": "'${RECOMMENDED_PR_STATUS:-draft}'",
  "files_changed": '${CHANGED_FILES_COUNT:-0}',
  "lines_changed": '${LINES_CHANGED:-0}'
}'

    # Get existing history and append current iteration
    local existing_history="${PREVIOUS_ITERATIONS:-[]}"
    local updated_history
    updated_history=$(echo "$existing_history" | jq --argjson new_entry "$current_iteration_entry" '. + [$new_entry]')

    # Generate complete state
    local complete_state='{
  "schema_version": "'${STATE_SCHEMA_VERSION}'",
  "pr_number": '${PR_NUMBER}',
  "iteration_count": '${CURRENT_ITERATION}',
  "current_status": "'${RECOMMENDED_PR_STATUS:-draft}'",
  "created_at": "'$(get_creation_timestamp)'",
  "updated_at": "'${timestamp}'",
  "historical_context": {
    "iteration_history": '${updated_history}',
    "progress_metrics": '$(generate_progress_metrics)'
  },
  "current_todo_list": '${UPDATED_TODO_LIST:-[]}',
  "pr_metadata": {
    "initial_files_changed": '$(get_initial_files_changed)',
    "current_files_changed": '${CHANGED_FILES_COUNT:-0}',
    "initial_lines_changed": '$(get_initial_lines_changed)',
    "current_lines_changed": '${LINES_CHANGED:-0}',
    "epic_context": "'${EPIC_CONTEXT}'",
    "quality_gates": '$(generate_quality_gates_status)'
  },
  "context_for_next_iteration": '$(generate_next_iteration_context_json)'
}'

    echo "$complete_state" | jq .
    return 0
}

#####################################
# Save state to permanent storage
# Arguments:
#   $1 - State data JSON
# Returns:
#   0 on success, 1 on failure
#####################################
save_to_permanent_storage() {
    local state_data="$1"

    log_debug "Saving state to permanent storage"

    local permanent_file="${PERMANENT_STATE_DIR}/pr-${PR_NUMBER}-state.json"

    # Create directory if it doesn't exist
    mkdir -p "$(dirname "$permanent_file")"

    # Save to permanent storage
    if echo "$state_data" > "$permanent_file"; then
        log_debug "State saved to permanent storage: $permanent_file"
    else
        log_warning "Failed to save state to permanent storage"
        return 1
    fi

    # Also save to GitHub Actions cache if available
    if command -v gh >/dev/null 2>&1; then
        save_to_github_cache "$state_data"
    fi

    return 0
}

#####################################
# Save state to GitHub Actions cache
# Arguments:
#   $1 - State data JSON
# Returns:
#   0 on success, 1 on failure
#####################################
save_to_github_cache() {
    local state_data="$1"

    log_debug "Attempting to save state to GitHub Actions cache"

    # This would use GitHub Actions cache API
    # For now, just create a cache file
    local cache_file="/tmp/github-cache-pr-${PR_NUMBER}-state.json"
    echo "$state_data" > "$cache_file"

    log_debug "State cached to: $cache_file"
    return 0
}

#####################################
# Context Retrieval and Merging
#####################################

#####################################
# Get historical context summary
# Returns:
#   Prints historical context summary
#####################################
get_historical_context_summary() {
    local history="${PREVIOUS_ITERATIONS:-[]}"

    if [[ "$history" == "[]" ]]; then
        echo "No previous iterations available"
        return 0
    fi

    local summary=""
    local iteration_count
    iteration_count=$(echo "$history" | jq length)

    summary="Historical context from $iteration_count previous iteration(s):\n"

    # Generate summary from each iteration
    local i=0
    while [[ $i -lt $iteration_count ]]; do
        local iteration_data
        iteration_data=$(echo "$history" | jq -r ".[$i]")

        local iter_num
        iter_num=$(echo "$iteration_data" | jq -r '.iteration')

        local iter_summary
        iter_summary=$(echo "$iteration_data" | jq -r '.summary')

        summary+="- Iteration $iter_num: $iter_summary\n"

        i=$((i + 1))
    done

    echo -e "$summary"
}

#####################################
# Get carry-forward items from context
# Returns:
#   Prints carry-forward items JSON array
#####################################
get_carryforward_items() {
    local context="${HISTORICAL_CONTEXT:-{}}"

    local carryforward
    carryforward=$(echo "$context" | jq -c '.context_for_next_iteration.carry_forward_items // []')

    echo "$carryforward"
}

#####################################
# Helper Functions for State Generation
#####################################

get_current_todo_count() {
    echo "${UPDATED_TODO_LIST:-[]}" | jq 'length'
}

get_resolved_todo_count() {
    echo "${UPDATED_TODO_LIST:-[]}" | jq '[.[] | select(.status == "completed")] | length'
}

get_creation_timestamp() {
    # Try to get from existing context, fallback to current time
    echo "${HISTORICAL_CONTEXT:-{}}" | jq -r '.created_at // "'$(date -u --iso-8601=seconds)'"'
}

get_initial_files_changed() {
    echo "${HISTORICAL_CONTEXT:-{}}" | jq -r '.pr_metadata.initial_files_changed // '${CHANGED_FILES_COUNT:-0}''
}

get_initial_lines_changed() {
    echo "${HISTORICAL_CONTEXT:-{}}" | jq -r '.pr_metadata.initial_lines_changed // '${LINES_CHANGED:-0}''
}

generate_progress_metrics() {
    local current_todos
    current_todos=$(get_current_todo_count)

    local resolved_todos
    resolved_todos=$(get_resolved_todo_count)

    local metrics='{
  "total_todos_created": '${current_todos}',
  "total_todos_resolved": '${resolved_todos}',
  "current_active_todos": '$((current_todos - resolved_todos))',
  "avg_resolution_time": null,
  "iteration_count": '${CURRENT_ITERATION}'
}'

    echo "$metrics"
}

generate_quality_gates_status() {
    local gates='{
  "tests_passing": null,
  "coverage_threshold": null,
  "security_scan": null,
  "standards_compliance": null,
  "last_updated": "'$(date -u --iso-8601=seconds)'"
}'

    echo "$gates"
}

generate_next_iteration_context_json() {
    local context='{
  "focus_areas": ["code_quality", "test_coverage", "documentation"],
  "carry_forward_items": [],
  "escalation_triggers": [],
  "epic_progress_notes": "Iteration '${CURRENT_ITERATION}' completed successfully",
  "recommended_next_actions": []
}'

    echo "$context"
}

#####################################
# Context Cleanup Functions
#####################################

#####################################
# Clean up old iteration state files
# Arguments:
#   $1 - Maximum age in days (optional, default 7)
# Returns:
#   0 on success, 1 on failure
#####################################
cleanup_old_state_files() {
    local max_age_days="${1:-7}"

    log_debug "Cleaning up state files older than $max_age_days days"

    # Clean temporary files
    find "$STATE_BASE_DIR" -name "pr-*-state.json" -mtime +$max_age_days -delete 2>/dev/null || true

    # Clean permanent files (be more conservative)
    local permanent_max_age=$((max_age_days * 2))
    find "$PERMANENT_STATE_DIR" -name "pr-*-state.json" -mtime +$permanent_max_age -delete 2>/dev/null || true

    log_debug "State file cleanup completed"
    return 0
}

# Logging functions (if not already defined)
if ! declare -f log_info >/dev/null 2>&1; then
    log_info() { echo "ℹ️ $1"; }
    log_warning() { echo "⚠️ $1"; }
    log_error() { echo "❌ $1" >&2; }
    log_debug() { [[ "${DEBUG_MODE:-false}" == "true" ]] && echo "🔍 DEBUG: $1"; }
fi

# Export functions for use by other modules
export -f load_iteration_context load_iteration_state_from_file generate_iteration_state_json
export -f save_to_permanent_storage get_historical_context_summary get_carryforward_items
export -f initialize_iteration_workspace cleanup_old_state_files