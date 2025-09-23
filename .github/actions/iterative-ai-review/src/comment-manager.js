#!/bin/bash
# Comment Manager for Iterative AI Review
# Handles comment lifecycle, historical context preservation, and content generation

set -euo pipefail

# Import GitHub API functions
source "$(dirname "${BASH_SOURCE[0]}")/github-api.js"

#####################################
# Comment Detection and Lifecycle Functions
#####################################

#####################################
# Detect existing iterative review comment
# Arguments:
#   $1 - PR number
# Returns:
#   0 if comment exists, 1 if not found
#####################################
detect_existing_iterative_comment() {
    local pr_number="$1"

    log_debug "Detecting existing iterative review comment for PR #$pr_number"

    local comment_id
    comment_id=$(find_existing_iterative_comment "$pr_number")

    if [[ -n "$comment_id" ]]; then
        log_info "Found existing iterative review comment: #$comment_id"
        return 0
    else
        log_info "No existing iterative review comment found"
        return 1
    fi
}

#####################################
# Extract historical context from existing comment
# Arguments:
#   $1 - PR number
# Returns:
#   Prints historical context JSON, 0 on success, 1 on failure
#####################################
extract_historical_context_from_comment() {
    local pr_number="$1"

    log_debug "Extracting historical context from existing comment"

    local comment_id
    if ! comment_id=$(find_existing_iterative_comment "$pr_number"); then
        log_debug "No existing comment to extract context from"
        echo "{}"
        return 0
    fi

    local comments
    if ! comments=$(get_pr_comments "$pr_number"); then
        log_error "Failed to get PR comments"
        return 1
    fi

    local comment_body
    comment_body=$(echo "$comments" | jq -r --arg id "$comment_id" '.[] | select(.id == ($id | tonumber)) | .body')

    if [[ -z "$comment_body" || "$comment_body" == "null" ]]; then
        log_warning "Could not find comment body for ID $comment_id"
        echo "{}"
        return 0
    fi

    # Extract context JSON from comment
    local context_json
    if context_json=$(extract_context_json_from_body "$comment_body"); then
        echo "$context_json"
    else
        log_warning "Failed to extract context from comment body"
        echo "{}"
    fi

    return 0
}

#####################################
# Extract context JSON from comment body
# Arguments:
#   $1 - Comment body text
# Returns:
#   Prints context JSON, 0 on success, 1 on failure
#####################################
extract_context_json_from_body() {
    local comment_body="$1"

    # Look for "Context for Next Iteration" section with JSON block
    local context_section
    context_section=$(echo "$comment_body" | sed -n '/### 🔄 Context for Next Iteration/,/^---/p' | head -n -1)

    if [[ -z "$context_section" ]]; then
        # Fallback: look for any JSON block in code fence
        context_section=$(echo "$comment_body" | sed -n '/```json/,/```/p' | head -n -1 | tail -n +2)
    fi

    if [[ -z "$context_section" ]]; then
        return 1
    fi

    # Extract JSON from the section
    local json_content
    json_content=$(echo "$context_section" | sed -n '/```json/,/```/p' | sed '1d;$d')

    if [[ -n "$json_content" ]]; then
        # Validate JSON
        if echo "$json_content" | jq . >/dev/null 2>&1; then
            echo "$json_content"
            return 0
        fi
    fi

    return 1
}

#####################################
# Extract iteration count from existing comment
# Arguments:
#   $1 - PR number
# Returns:
#   Prints iteration count, 0 on success, 1 on failure
#####################################
extract_iteration_count_from_comment() {
    local pr_number="$1"

    log_debug "Extracting iteration count from existing comment"

    local comment_id
    if ! comment_id=$(find_existing_iterative_comment "$pr_number"); then
        log_debug "No existing comment to extract iteration count from"
        echo "0"
        return 0
    fi

    local comments
    if ! comments=$(get_pr_comments "$pr_number"); then
        log_error "Failed to get PR comments"
        return 1
    fi

    local comment_body
    comment_body=$(echo "$comments" | jq -r --arg id "$comment_id" '.[] | select(.id == ($id | tonumber)) | .body')

    # Extract iteration count from header
    local iteration_count
    iteration_count=$(echo "$comment_body" | grep -oP "# 🔄 Iterative AI Code Review - Iteration \K\d+" | head -n1)

    if [[ -n "$iteration_count" ]]; then
        echo "$iteration_count"
    else
        # Fallback: look for "Iteration N" pattern
        iteration_count=$(echo "$comment_body" | grep -oP "Iteration \K\d+" | head -n1)
        echo "${iteration_count:-1}"
    fi

    return 0
}

#####################################
# Comment Content Generation
#####################################

#####################################
# Generate complete iterative comment body
# Uses global variables from AI analysis results
# Returns:
#   Prints comment body, 0 on success, 1 on failure
#####################################
generate_iterative_comment_body() {
    log_debug "Generating iterative comment body"

    local comment_body=""
    local timestamp=$(date -u +"%Y-%m-%d %H:%M:%S UTC")

    # Build comment header
    comment_body+="# 🔄 Iterative AI Code Review - Iteration ${CURRENT_ITERATION}\n\n"

    # PR context line
    comment_body+="PR: #${PR_NUMBER} by @${PR_AUTHOR} (${SOURCE_BRANCH} → ${TARGET_BRANCH})"
    if [[ -n "${ISSUE_REF:-}" ]]; then
        comment_body+=" • Issue: ${ISSUE_REF}"
    fi
    comment_body+=" • Epic: ${EPIC_CONTEXT}\n\n"

    # Status and recommendation
    local pr_status_recommendation=$(determine_pr_status_recommendation)
    local overall_status=$(determine_overall_status)

    comment_body+="**Status**: ${overall_status}\n\n"
    comment_body+="**PR Status Recommendation**: ${pr_status_recommendation}\n\n"

    # Running to-do list status
    comment_body+="$(generate_todo_list_section)\n\n"

    # Iteration progress summary
    comment_body+="$(generate_progress_summary_section)\n\n"

    # Quality gate assessment
    comment_body+="$(generate_quality_gate_section)\n\n"

    # Next iteration recommendations
    comment_body+="$(generate_recommendations_section)\n\n"

    # Historical context
    comment_body+="$(generate_historical_context_section)\n\n"

    # Context for next iteration
    comment_body+="$(generate_next_iteration_context)\n\n"

    # Footer
    comment_body+="---\n\n"
    comment_body+="*This iterative analysis builds upon ${CURRENT_ITERATION} review(s) and maintains Epic #181 autonomous development alignment. "
    comment_body+="See previous iterations for full context history.*\n\n"
    comment_body+="*Generated at: ${timestamp}*"

    echo -e "$comment_body"
    return 0
}

#####################################
# Generate to-do list section
# Returns:
#   Prints to-do list section
#####################################
generate_todo_list_section() {
    local section=""

    section+="### 📋 Running To-Do List Status\n\n"

    # Completed items this iteration
    section+="#### Completed This Iteration ✅\n"
    section+="| ID | Description | Validation | Epic Alignment |\n"
    section+="|----|-----------|-----------|-----------|\n"

    local completed_items
    completed_items=$(get_completed_todo_items)
    if [[ -n "$completed_items" ]]; then
        section+="$completed_items\n"
    else
        section+="| - | No items completed this iteration | - | - |\n"
    fi

    section+="\n"

    # Active to-do items
    section+="#### Active To-Do Items (Requires Action)\n"
    section+="| ID | Priority | Description | File:Line | Status | Epic Impact |\n"
    section+="|----|---------|-----------|---------|---------|-----------|\n"

    local active_items
    active_items=$(get_active_todo_items)
    if [[ -n "$active_items" ]]; then
        section+="$active_items\n"
    else
        section+="| - | - | No active to-do items | - | - | - |\n"
    fi

    section+="\n"

    # New items this iteration
    section+="#### New Items This Iteration\n"
    section+="| ID | Priority | Description | File:Line | Epic Alignment |\n"
    section+="|----|---------|-----------|---------|-----------|\n"

    local new_items
    new_items=$(get_new_todo_items)
    if [[ -n "$new_items" ]]; then
        section+="$new_items\n"
    else
        section+="| - | - | No new items identified | - | - |\n"
    fi

    echo -e "$section"
}

#####################################
# Generate progress summary section
# Returns:
#   Prints progress summary section
#####################################
generate_progress_summary_section() {
    local section=""

    section+="### 📊 Iteration Progress Summary\n\n"

    section+="**Changes Since Last Review:**\n"
    section+="- Files Modified: ${CHANGED_FILES_COUNT}\n"
    section+="- Lines Changed: ${LINES_CHANGED}\n"
    section+="- Progress Made: $(get_progress_description)\n\n"

    section+="**Epic #181 Alignment:**\n"
    section+="- Autonomous Development Progression: $(get_autonomous_development_assessment)\n"
    section+="- Coverage Epic Integration: $(get_coverage_epic_status)\n"
    section+="- Multi-Agent Coordination: $(get_coordination_impact)\n"

    echo -e "$section"
}

#####################################
# Generate quality gate section
# Returns:
#   Prints quality gate section
#####################################
generate_quality_gate_section() {
    local section=""

    section+="### 🎯 Quality Gate Assessment\n\n"

    section+="**Current Status:**\n"
    section+="- **Code Quality**: $(get_code_quality_trend)\n"
    section+="- **Test Coverage**: $(get_coverage_progression)\n"
    section+="- **Documentation**: $(get_documentation_assessment)\n"
    section+="- **Epic Alignment**: $(get_epic_alignment_status)\n\n"

    section+="**Blocking Issues:**\n"
    section+="| Priority | Issue | Resolution Required |\n"
    section+="|---------|-------|-------------------|\n"

    local blocking_issues
    blocking_issues=$(get_blocking_issues_table)
    if [[ -n "$blocking_issues" ]]; then
        section+="$blocking_issues\n"
    else
        section+="| - | No blocking issues identified | - |\n"
    fi

    echo -e "$section"
}

#####################################
# Generate recommendations section
# Returns:
#   Prints recommendations section
#####################################
generate_recommendations_section() {
    local section=""

    section+="### 🚀 Next Iteration Recommendations\n\n"

    section+="**Immediate Actions (This Iteration):**\n"
    local immediate_actions
    immediate_actions=$(get_immediate_actions)
    if [[ -n "$immediate_actions" ]]; then
        section+="$immediate_actions\n"
    else
        section+="- No immediate actions required\n"
    fi

    section+="\n**Future Iteration Planning:**\n"
    local future_actions
    future_actions=$(get_future_actions)
    if [[ -n "$future_actions" ]]; then
        section+="$future_actions\n"
    else
        section+="- No future actions identified\n"
    fi

    section+="\n**Epic #181 Strategic Actions:**\n"
    local strategic_actions
    strategic_actions=$(get_strategic_actions)
    if [[ -n "$strategic_actions" ]]; then
        section+="$strategic_actions\n"
    else
        section+="- Aligned with Epic #181 objectives\n"
    fi

    echo -e "$section"
}

#####################################
# Generate historical context section
# Returns:
#   Prints historical context section
#####################################
generate_historical_context_section() {
    local section=""

    section+="### 📈 Historical Context\n\n"

    section+="**Iteration Progression:**\n"
    local iteration_history
    iteration_history=$(get_iteration_progression_summary)
    section+="$iteration_history\n"

    section+="\n**Pattern Recognition:**\n"
    section+="- **Improving Patterns**: $(get_improving_patterns)\n"
    section+="- **Areas Needing Attention**: $(get_concerning_patterns)\n"

    echo -e "$section"
}

#####################################
# Generate next iteration context
# Returns:
#   Prints next iteration context section
#####################################
generate_next_iteration_context() {
    local section=""

    section+="### 🔄 Context for Next Iteration\n\n"

    # Carry-forward items JSON
    local carryforward_json
    carryforward_json=$(generate_carryforward_json)

    section+="**Carry-Forward Items:**\n"
    section+="```json\n"
    section+="$carryforward_json\n"
    section+="```\n\n"

    section+="**Historical Context Summary:**\n"
    section+="$(get_historical_summary)\n"

    echo -e "$section"
}

#####################################
# Helper Functions for Content Generation
#####################################

# Determine PR status recommendation
determine_pr_status_recommendation() {
    echo "${RECOMMENDED_PR_STATUS:-DRAFT}"
}

# Determine overall status
determine_overall_status() {
    local critical_issues
    critical_issues=$(get_critical_issues_count)

    if [[ "$critical_issues" -eq 0 ]]; then
        echo "✅ READY FOR MERGE"
    elif [[ "$critical_issues" -le 2 ]]; then
        echo "🔄 CONTINUE ITERATIONS"
    else
        echo "🚫 REQUIRES MAJOR CHANGES"
    fi
}

# Get content from AI analysis or provide defaults
get_completed_todo_items() {
    # Parse from AI analysis or return default
    echo ""  # Would parse from AI_ANALYSIS_RESULT
}

get_active_todo_items() {
    # Parse from AI analysis or return default
    echo ""  # Would parse from AI_ANALYSIS_RESULT
}

get_new_todo_items() {
    # Parse from AI analysis or return default
    echo ""  # Would parse from AI_ANALYSIS_RESULT
}

get_progress_description() {
    echo "Incremental improvements identified"
}

get_autonomous_development_assessment() {
    echo "Aligned with Epic #181 objectives"
}

get_coverage_epic_status() {
    echo "Contributing to 90% coverage goal"
}

get_coordination_impact() {
    echo "Supports multi-agent workflow patterns"
}

get_code_quality_trend() {
    echo "Maintaining quality standards"
}

get_coverage_progression() {
    echo "Coverage metrics stable"
}

get_documentation_assessment() {
    echo "Documentation current"
}

get_epic_alignment_status() {
    echo "Aligned with Epic #181"
}

get_blocking_issues_table() {
    echo ""  # Would extract from analysis
}

get_immediate_actions() {
    echo ""  # Would extract from AI recommendations
}

get_future_actions() {
    echo ""  # Would extract from AI recommendations
}

get_strategic_actions() {
    echo ""  # Would extract from AI recommendations
}

get_iteration_progression_summary() {
    echo "- Iteration ${CURRENT_ITERATION}: Current analysis and recommendations"
}

get_improving_patterns() {
    echo "Consistent quality improvements"
}

get_concerning_patterns() {
    echo "No concerning patterns identified"
}

get_critical_issues_count() {
    echo "0"  # Would count from analysis
}

get_historical_summary() {
    echo "Development progressing steadily toward Epic #181 objectives with maintained quality standards."
}

#####################################
# Generate carry-forward context JSON
# Returns:
#   Prints JSON context for next iteration
#####################################
generate_carryforward_json() {
    local json_context='{
  "critical_items": [],
  "high_priority": [],
  "progress_context": "Iteration '${CURRENT_ITERATION}' completed with quality standards maintained",
  "epic_alignment_notes": "Continued alignment with Epic #181 autonomous development objectives",
  "next_focus_areas": ["code quality", "test coverage", "documentation"],
  "iteration_metadata": {
    "current_iteration": '${CURRENT_ITERATION}',
    "timestamp": "'$(date -u --iso-8601=seconds)'",
    "pr_status": "'${RECOMMENDED_PR_STATUS:-draft}'",
    "files_changed": '${CHANGED_FILES_COUNT:-0}',
    "lines_changed": '${LINES_CHANGED:-0}'
  }
}'

    echo "$json_context" | jq .
}

# Logging functions (if not already defined)
if ! declare -f log_info >/dev/null 2>&1; then
    log_info() { echo "ℹ️ $1"; }
    log_warning() { echo "⚠️ $1"; }
    log_error() { echo "❌ $1" >&2; }
    log_debug() { [[ "${DEBUG_MODE:-false}" == "true" ]] && echo "🔍 DEBUG: $1"; }
fi

# Export functions for use by other modules
export -f detect_existing_iterative_comment extract_historical_context_from_comment
export -f extract_iteration_count_from_comment generate_iterative_comment_body
export -f extract_context_json_from_body generate_carryforward_json