#!/bin/bash
# To-Do Manager for Iterative AI Review
# Handles structured to-do list management, parsing, and evolution tracking

set -euo pipefail

# Global variables for to-do management
declare -g TODO_ID_PREFIX="todo"
declare -g TODO_CATEGORIES=("CRITICAL" "HIGH" "MEDIUM" "LOW" "COMPLETED")
declare -g TODO_STATUSES=("pending" "in_progress" "completed" "deferred")

#####################################
# To-Do List Management Functions
#####################################

#####################################
# Parse to-do items from AI analysis result
# Arguments:
#   $1 - AI analysis result text
# Returns:
#   0 on success, 1 on failure
#####################################
parse_todo_items_from_analysis() {
    local analysis_result="$1"

    log_debug "Parsing to-do items from AI analysis"

    # Initialize tracking arrays
    local new_todos=()
    local updated_todos=()
    local completed_todos=()

    # Parse different sections of the analysis
    parse_new_todo_items "$analysis_result" new_todos
    parse_updated_todo_items "$analysis_result" updated_todos
    parse_completed_todo_items "$analysis_result" completed_todos

    # Export parsed results
    export PARSED_NEW_TODOS="$(printf '%s\n' "${new_todos[@]}" | jq -s .)"
    export PARSED_UPDATED_TODOS="$(printf '%s\n' "${updated_todos[@]}" | jq -s .)"
    export PARSED_COMPLETED_TODOS="$(printf '%s\n' "${completed_todos[@]}" | jq -s .)"

    log_debug "Parsed ${#new_todos[@]} new, ${#updated_todos[@]} updated, ${#completed_todos[@]} completed to-do items"
    return 0
}

#####################################
# Parse new to-do items from analysis
# Arguments:
#   $1 - Analysis text
#   $2 - Array name to store results
# Returns:
#   0 on success, 1 on failure
#####################################
parse_new_todo_items() {
    local analysis_text="$1"
    local -n result_array=$2

    log_debug "Parsing new to-do items"

    # Look for patterns indicating new to-do items
    # This would parse from the AI analysis output
    # For now, create a sample structure

    local sample_todo
    sample_todo=$(create_todo_item \
        "$(generate_todo_id)" \
        "HIGH" \
        "Sample to-do item from analysis" \
        "src/example.cs:45" \
        "pending" \
        "Epic #181 alignment example" \
        "${CURRENT_ITERATION}" \
        "Code review identified improvement opportunity")

    result_array+=("$sample_todo")

    return 0
}

#####################################
# Parse updated to-do items from analysis
# Arguments:
#   $1 - Analysis text
#   $2 - Array name to store results
# Returns:
#   0 on success, 1 on failure
#####################################
parse_updated_todo_items() {
    local analysis_text="$1"
    local -n result_array=$2

    log_debug "Parsing updated to-do items"

    # Parse existing to-do items that have status changes
    # This would cross-reference with existing CURRENT_TODO_LIST
    # For now, return empty array

    return 0
}

#####################################
# Parse completed to-do items from analysis
# Arguments:
#   $1 - Analysis text
#   $2 - Array name to store results
# Returns:
#   0 on success, 1 on failure
#####################################
parse_completed_todo_items() {
    local analysis_text="$1"
    local -n result_array=$2

    log_debug "Parsing completed to-do items"

    # Parse to-do items marked as completed
    # This would validate completion against criteria
    # For now, return empty array

    return 0
}

#####################################
# Update to-do list state with parsed items
# Arguments:
#   $1 - New to-dos JSON
#   $2 - Updated to-dos JSON
#   $3 - Completed to-dos JSON
# Returns:
#   0 on success, 1 on failure
#####################################
update_todo_list_state() {
    local new_todos="$1"
    local updated_todos="$2"
    local completed_todos="$3"

    log_debug "Updating to-do list state"

    # Load current to-do list
    local current_list="${CURRENT_TODO_LIST:-[]}"

    # Merge updates
    local updated_list
    updated_list=$(merge_todo_updates "$current_list" "$new_todos" "$updated_todos" "$completed_todos")

    # Export updated list
    export UPDATED_TODO_LIST="$updated_list"

    # Generate summary statistics
    export TODO_STATS="$(generate_todo_statistics "$updated_list")"

    log_debug "To-do list state updated successfully"
    return 0
}

#####################################
# Merge to-do updates into current list
# Arguments:
#   $1 - Current to-do list JSON
#   $2 - New to-dos JSON
#   $3 - Updated to-dos JSON
#   $4 - Completed to-dos JSON
# Returns:
#   Prints merged to-do list JSON
#####################################
merge_todo_updates() {
    local current_list="$1"
    local new_todos="$2"
    local updated_todos="$3"
    local completed_todos="$4"

    log_debug "Merging to-do list updates"

    # Start with current list
    local merged_list="$current_list"

    # Add new to-dos
    if [[ "$new_todos" != "[]" && "$new_todos" != "null" ]]; then
        merged_list=$(echo "$merged_list" | jq --argjson new "$new_todos" '. + $new')
    fi

    # Apply updates to existing to-dos
    if [[ "$updated_todos" != "[]" && "$updated_todos" != "null" ]]; then
        merged_list=$(apply_todo_updates "$merged_list" "$updated_todos")
    fi

    # Mark completed to-dos
    if [[ "$completed_todos" != "[]" && "$completed_todos" != "null" ]]; then
        merged_list=$(apply_todo_completions "$merged_list" "$completed_todos")
    fi

    # Update iteration metadata
    merged_list=$(update_iteration_metadata "$merged_list")

    echo "$merged_list"
}

#####################################
# Apply updates to existing to-do items
# Arguments:
#   $1 - Current list JSON
#   $2 - Updates JSON
# Returns:
#   Prints updated list JSON
#####################################
apply_todo_updates() {
    local current_list="$1"
    local updates="$2"

    log_debug "Applying to-do updates"

    # For each update, find the corresponding item and update it
    local updated_list="$current_list"

    local update_count
    update_count=$(echo "$updates" | jq length)

    local i=0
    while [[ $i -lt $update_count ]]; do
        local update_item
        update_item=$(echo "$updates" | jq ".[$i]")

        local todo_id
        todo_id=$(echo "$update_item" | jq -r '.id')

        # Update the item in the list
        updated_list=$(echo "$updated_list" | jq --argjson update "$update_item" \
            'map(if .id == $update.id then . + $update + {iteration_updated: '$CURRENT_ITERATION'} else . end)')

        i=$((i + 1))
    done

    echo "$updated_list"
}

#####################################
# Apply completions to to-do items
# Arguments:
#   $1 - Current list JSON
#   $2 - Completions JSON
# Returns:
#   Prints updated list JSON
#####################################
apply_todo_completions() {
    local current_list="$1"
    local completions="$2"

    log_debug "Applying to-do completions"

    local updated_list="$current_list"
    local completion_timestamp=$(date -u --iso-8601=seconds)

    local completion_count
    completion_count=$(echo "$completions" | jq length)

    local i=0
    while [[ $i -lt $completion_count ]]; do
        local completed_item
        completed_item=$(echo "$completions" | jq ".[$i]")

        local todo_id
        todo_id=$(echo "$completed_item" | jq -r '.id')

        # Mark item as completed
        updated_list=$(echo "$updated_list" | jq --arg id "$todo_id" --arg timestamp "$completion_timestamp" \
            'map(if .id == $id then . + {status: "completed", completed_at: $timestamp, iteration_completed: '$CURRENT_ITERATION'} else . end)')

        i=$((i + 1))
    done

    echo "$updated_list"
}

#####################################
# Update iteration metadata for all items
# Arguments:
#   $1 - To-do list JSON
# Returns:
#   Prints updated list JSON
#####################################
update_iteration_metadata() {
    local todo_list="$1"

    # Update last_reviewed iteration for all items
    echo "$todo_list" | jq 'map(. + {last_reviewed: '$CURRENT_ITERATION'})'
}

#####################################
# To-Do Item Creation and Management
#####################################

#####################################
# Create a structured to-do item
# Arguments:
#   $1 - ID
#   $2 - Category (CRITICAL, HIGH, MEDIUM, LOW)
#   $3 - Description
#   $4 - File references (comma-separated)
#   $5 - Status (pending, in_progress, completed, deferred)
#   $6 - Epic alignment notes
#   $7 - Iteration added
#   $8 - Validation criteria
# Returns:
#   Prints to-do item JSON
#####################################
create_todo_item() {
    local id="$1"
    local category="$2"
    local description="$3"
    local file_references="$4"
    local status="$5"
    local epic_alignment="$6"
    local iteration_added="$7"
    local validation_criteria="$8"

    # Validate category
    if ! validate_todo_category "$category"; then
        log_error "Invalid to-do category: $category"
        return 1
    fi

    # Validate status
    if ! validate_todo_status "$status"; then
        log_error "Invalid to-do status: $status"
        return 1
    fi

    # Parse file references into array
    local file_refs_array
    if [[ -n "$file_references" ]]; then
        file_refs_array=$(echo "$file_references" | tr ',' '\n' | jq -R . | jq -s .)
    else
        file_refs_array="[]"
    fi

    # Create to-do item structure
    local todo_item
    todo_item=$(jq -n \
        --arg id "$id" \
        --arg category "$category" \
        --arg description "$description" \
        --argjson file_references "$file_refs_array" \
        --arg epic_alignment "$epic_alignment" \
        --arg iteration_added "$iteration_added" \
        --arg iteration_updated "$iteration_added" \
        --arg status "$status" \
        --arg validation_criteria "$validation_criteria" \
        --arg created_at "$(date -u --iso-8601=seconds)" \
        '{
            id: $id,
            category: $category,
            description: $description,
            file_references: $file_references,
            epic_alignment: $epic_alignment,
            iteration_added: ($iteration_added | tonumber),
            iteration_updated: ($iteration_updated | tonumber),
            status: $status,
            validation_criteria: $validation_criteria,
            dependencies: [],
            created_at: $created_at,
            last_reviewed: ($iteration_added | tonumber)
        }')

    echo "$todo_item"
}

#####################################
# Generate unique to-do ID
# Returns:
#   Prints unique to-do ID
#####################################
generate_todo_id() {
    local timestamp=$(date +%s)
    local random=$(shuf -i 100-999 -n 1)
    echo "${TODO_ID_PREFIX}-${timestamp}-${random}"
}

#####################################
# Validate to-do category
# Arguments:
#   $1 - Category to validate
# Returns:
#   0 if valid, 1 if invalid
#####################################
validate_todo_category() {
    local category="$1"

    for valid_category in "${TODO_CATEGORIES[@]}"; do
        if [[ "$category" == "$valid_category" ]]; then
            return 0
        fi
    done

    return 1
}

#####################################
# Validate to-do status
# Arguments:
#   $1 - Status to validate
# Returns:
#   0 if valid, 1 if invalid
#####################################
validate_todo_status() {
    local status="$1"

    for valid_status in "${TODO_STATUSES[@]}"; do
        if [[ "$status" == "$valid_status" ]]; then
            return 0
        fi
    done

    return 1
}

#####################################
# To-Do List Analysis and Reporting
#####################################

#####################################
# Generate to-do statistics
# Arguments:
#   $1 - To-do list JSON
# Returns:
#   Prints statistics JSON
#####################################
generate_todo_statistics() {
    local todo_list="$1"

    local total_count
    total_count=$(echo "$todo_list" | jq length)

    local critical_count
    critical_count=$(echo "$todo_list" | jq '[.[] | select(.category == "CRITICAL")] | length')

    local high_count
    high_count=$(echo "$todo_list" | jq '[.[] | select(.category == "HIGH")] | length')

    local medium_count
    medium_count=$(echo "$todo_list" | jq '[.[] | select(.category == "MEDIUM")] | length')

    local low_count
    low_count=$(echo "$todo_list" | jq '[.[] | select(.category == "LOW")] | length')

    local completed_count
    completed_count=$(echo "$todo_list" | jq '[.[] | select(.status == "completed")] | length')

    local pending_count
    pending_count=$(echo "$todo_list" | jq '[.[] | select(.status == "pending")] | length')

    local in_progress_count
    in_progress_count=$(echo "$todo_list" | jq '[.[] | select(.status == "in_progress")] | length')

    local deferred_count
    deferred_count=$(echo "$todo_list" | jq '[.[] | select(.status == "deferred")] | length')

    local stats
    stats=$(jq -n \
        --arg total "$total_count" \
        --arg critical "$critical_count" \
        --arg high "$high_count" \
        --arg medium "$medium_count" \
        --arg low "$low_count" \
        --arg completed "$completed_count" \
        --arg pending "$pending_count" \
        --arg in_progress "$in_progress_count" \
        --arg deferred "$deferred_count" \
        '{
            total: ($total | tonumber),
            by_category: {
                critical: ($critical | tonumber),
                high: ($high | tonumber),
                medium: ($medium | tonumber),
                low: ($low | tonumber)
            },
            by_status: {
                completed: ($completed | tonumber),
                pending: ($pending | tonumber),
                in_progress: ($in_progress | tonumber),
                deferred: ($deferred | tonumber)
            },
            completion_rate: (if ($total | tonumber) > 0 then (($completed | tonumber) * 100 / ($total | tonumber)) else 0 end),
            active_items: (($pending | tonumber) + ($in_progress | tonumber))
        }')

    echo "$stats"
}

#####################################
# Get current to-do list as JSON
# Returns:
#   Prints current to-do list JSON
#####################################
get_current_todo_list_json() {
    echo "${UPDATED_TODO_LIST:-${CURRENT_TODO_LIST:-[]}}"
}

#####################################
# Get active to-do items (pending + in_progress)
# Returns:
#   Prints active to-do items JSON
#####################################
get_active_todo_items_json() {
    local todo_list
    todo_list=$(get_current_todo_list_json)

    echo "$todo_list" | jq '[.[] | select(.status == "pending" or .status == "in_progress")]'
}

#####################################
# Get completed to-do items for current iteration
# Returns:
#   Prints completed to-do items JSON
#####################################
get_completed_todo_items_json() {
    local todo_list
    todo_list=$(get_current_todo_list_json)

    echo "$todo_list" | jq --arg iteration "$CURRENT_ITERATION" \
        '[.[] | select(.status == "completed" and .iteration_completed == ($iteration | tonumber))]'
}

#####################################
# Get critical blocking issues
# Returns:
#   Prints critical to-do items JSON
#####################################
get_critical_blocking_issues() {
    local todo_list
    todo_list=$(get_current_todo_list_json)

    echo "$todo_list" | jq '[.[] | select(.category == "CRITICAL" and .status != "completed")]'
}

#####################################
# To-Do List Quality Assessment
#####################################

#####################################
# Assess readiness for PR advancement
# Returns:
#   0 if ready, 1 if not ready
#####################################
assess_todo_readiness_for_advancement() {
    log_debug "Assessing to-do readiness for PR advancement"

    local critical_issues
    critical_issues=$(get_critical_blocking_issues)

    local critical_count
    critical_count=$(echo "$critical_issues" | jq length)

    if [[ "$critical_count" -gt 0 ]]; then
        log_info "PR advancement blocked by $critical_count critical issues"
        return 1
    fi

    local high_issues
    high_issues=$(get_current_todo_list_json | jq '[.[] | select(.category == "HIGH" and .status != "completed")]')

    local high_count
    high_count=$(echo "$high_issues" | jq length)

    # Allow advancement with high priority items but warn
    if [[ "$high_count" -gt 5 ]]; then
        log_warning "PR has $high_count high priority items - consider addressing before merge"
    fi

    log_info "To-do list assessment: ready for advancement"
    return 0
}

#####################################
# Generate to-do list summary for outputs
# Returns:
#   Prints summary JSON
#####################################
generate_todo_summary_for_output() {
    local todo_list
    todo_list=$(get_current_todo_list_json)

    local stats
    stats=$(generate_todo_statistics "$todo_list")

    local critical_issues
    critical_issues=$(get_critical_blocking_issues)

    local summary
    summary=$(jq -n \
        --argjson stats "$stats" \
        --argjson critical "$critical_issues" \
        '{
            statistics: $stats,
            critical_blocking_issues: $critical,
            readiness_assessment: {
                ready_for_advancement: (($critical | length) == 0),
                blocking_issues_count: ($critical | length),
                recommendation: (if ($critical | length) == 0 then "ready" else "requires_resolution")
            }
        }')

    echo "$summary"
}

# Logging functions (if not already defined)
if ! declare -f log_info >/dev/null 2>&1; then
    log_info() { echo "ℹ️ $1"; }
    log_warning() { echo "⚠️ $1"; }
    log_error() { echo "❌ $1" >&2; }
    log_debug() { [[ "${DEBUG_MODE:-false}" == "true" ]] && echo "🔍 DEBUG: $1"; }
fi

# Export functions for use by other modules
export -f parse_todo_items_from_analysis update_todo_list_state create_todo_item
export -f generate_todo_id get_current_todo_list_json get_active_todo_items_json
export -f get_completed_todo_items_json get_critical_blocking_issues
export -f assess_todo_readiness_for_advancement generate_todo_summary_for_output
export -f generate_todo_statistics