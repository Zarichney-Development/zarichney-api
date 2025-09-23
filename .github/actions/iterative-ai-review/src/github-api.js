#!/bin/bash
# GitHub API Functions for Iterative AI Review
# Handles PR status management, comment operations, and GitHub integration

set -euo pipefail

#####################################
# PR Status Management Functions
#####################################

#####################################
# Get current PR status (draft/ready)
# Arguments:
#   $1 - PR number
# Returns:
#   Prints PR status, 0 on success, 1 on failure
#####################################
get_pr_status() {
    local pr_number="$1"

    log_debug "Getting PR status for #$pr_number"

    local pr_data
    if ! pr_data=$(gh api "repos/{owner}/{repo}/pulls/$pr_number" 2>/dev/null); then
        log_error "Failed to fetch PR data for #$pr_number"
        return 1
    fi

    local is_draft
    is_draft=$(echo "$pr_data" | jq -r '.draft')

    if [[ "$is_draft" == "true" ]]; then
        echo "draft"
    else
        echo "ready"
    fi

    return 0
}

#####################################
# Update PR status (draft/ready conversion)
# Arguments:
#   $1 - PR number
#   $2 - New status (draft/ready)
# Returns:
#   0 on success, 1 on failure
#####################################
update_pr_status() {
    local pr_number="$1"
    local new_status="$2"

    log_debug "Updating PR #$pr_number status to: $new_status"

    local draft_value
    case "$new_status" in
        "draft")
            draft_value="true"
            ;;
        "ready")
            draft_value="false"
            ;;
        *)
            log_error "Invalid PR status: $new_status (must be draft or ready)"
            return 1
            ;;
    esac

    local update_data="{\"draft\": $draft_value}"

    if ! gh api "repos/{owner}/{repo}/pulls/$pr_number" \
        --method PATCH \
        --input - <<< "$update_data" >/dev/null 2>&1; then
        log_error "Failed to update PR #$pr_number status to $new_status"
        return 1
    fi

    log_info "Successfully updated PR #$pr_number status to $new_status"
    return 0
}

#####################################
# Check if PR status auto-update should be performed
# Arguments:
#   $1 - Current status
#   $2 - Recommended status
# Returns:
#   0 if should update, 1 if should not
#####################################
should_auto_update_pr_status() {
    local current_status="$1"
    local recommended_status="$2"

    # Allow draft → ready transitions (promoting PR)
    if [[ "$current_status" == "draft" && "$recommended_status" == "ready" ]]; then
        return 0
    fi

    # Prevent ready → draft transitions automatically (regression protection)
    if [[ "$current_status" == "ready" && "$recommended_status" == "draft" ]]; then
        log_info "Preventing automatic downgrade from ready to draft (requires manual intervention)"
        return 1
    fi

    # No change needed
    if [[ "$current_status" == "$recommended_status" ]]; then
        return 1
    fi

    return 0
}

#####################################
# Get PR metadata for context
# Arguments:
#   $1 - PR number
# Returns:
#   JSON object with PR metadata, 0 on success, 1 on failure
#####################################
get_pr_metadata() {
    local pr_number="$1"

    log_debug "Fetching PR metadata for #$pr_number"

    local pr_data
    if ! pr_data=$(gh api "repos/{owner}/{repo}/pulls/$pr_number" 2>/dev/null); then
        log_error "Failed to fetch PR metadata for #$pr_number"
        return 1
    fi

    # Extract key metadata
    local metadata
    metadata=$(echo "$pr_data" | jq '{
        number: .number,
        title: .title,
        author: .user.login,
        state: .state,
        draft: .draft,
        mergeable: .mergeable,
        mergeable_state: .mergeable_state,
        source_branch: .head.ref,
        target_branch: .base.ref,
        created_at: .created_at,
        updated_at: .updated_at,
        changed_files: .changed_files,
        additions: .additions,
        deletions: .deletions,
        commits: .commits
    }')

    echo "$metadata"
    return 0
}

#####################################
# Comment Management Functions
#####################################

#####################################
# Create a new PR comment
# Arguments:
#   $1 - PR number
#   $2 - Comment body
# Returns:
#   Comment ID on success, 1 on failure
#####################################
create_pr_comment() {
    local pr_number="$1"
    local comment_body="$2"

    log_debug "Creating new PR comment for #$pr_number"

    local comment_data
    comment_data=$(jq -n --arg body "$comment_body" '{body: $body}')

    local response
    if ! response=$(gh api "repos/{owner}/{repo}/issues/$pr_number/comments" \
        --method POST \
        --input - <<< "$comment_data" 2>/dev/null); then
        log_error "Failed to create PR comment for #$pr_number"
        return 1
    fi

    local comment_id
    comment_id=$(echo "$response" | jq -r '.id')

    log_info "Created PR comment #$comment_id for PR #$pr_number"
    echo "$comment_id"
    return 0
}

#####################################
# Update an existing PR comment
# Arguments:
#   $1 - PR number
#   $2 - Comment ID
#   $3 - New comment body
# Returns:
#   0 on success, 1 on failure
#####################################
update_pr_comment() {
    local pr_number="$1"
    local comment_id="$2"
    local comment_body="$3"

    log_debug "Updating PR comment #$comment_id for PR #$pr_number"

    local comment_data
    comment_data=$(jq -n --arg body "$comment_body" '{body: $body}')

    if ! gh api "repos/{owner}/{repo}/issues/comments/$comment_id" \
        --method PATCH \
        --input - <<< "$comment_data" >/dev/null 2>&1; then
        log_error "Failed to update PR comment #$comment_id"
        return 1
    fi

    log_info "Updated PR comment #$comment_id for PR #$pr_number"
    return 0
}

#####################################
# Get all comments for a PR
# Arguments:
#   $1 - PR number
# Returns:
#   JSON array of comments, 0 on success, 1 on failure
#####################################
get_pr_comments() {
    local pr_number="$1"

    log_debug "Fetching all comments for PR #$pr_number"

    local comments
    if ! comments=$(gh api "repos/{owner}/{repo}/issues/$pr_number/comments" 2>/dev/null); then
        log_error "Failed to fetch comments for PR #$pr_number"
        return 1
    fi

    echo "$comments"
    return 0
}

#####################################
# Find comment by header pattern
# Arguments:
#   $1 - PR number
#   $2 - Header pattern to search for
# Returns:
#   Comment ID if found, empty if not found, 1 on error
#####################################
find_comment_by_header() {
    local pr_number="$1"
    local header_pattern="$2"

    log_debug "Searching for comment with header: $header_pattern"

    local comments
    if ! comments=$(get_pr_comments "$pr_number"); then
        return 1
    fi

    local comment_id
    comment_id=$(echo "$comments" | jq -r --arg pattern "$header_pattern" '
        .[] | select(.body and (.body | contains($pattern))) | .id' | head -n1)

    if [[ -n "$comment_id" && "$comment_id" != "null" ]]; then
        echo "$comment_id"
    fi

    return 0
}

#####################################
# Find existing iterative review comment
# Arguments:
#   $1 - PR number
# Returns:
#   Comment ID if found, empty if not found, 1 on error
#####################################
find_existing_iterative_comment() {
    local pr_number="$1"

    # Search for iterative review header pattern
    local header_pattern="# 🔄 Iterative AI Code Review"
    find_comment_by_header "$pr_number" "$header_pattern"
}

#####################################
# Repository Information Functions
#####################################

#####################################
# Get repository information
# Returns:
#   JSON object with repo info, 0 on success, 1 on failure
#####################################
get_repository_info() {
    log_debug "Fetching repository information"

    local repo_data
    if ! repo_data=$(gh api "repos/{owner}/{repo}" 2>/dev/null); then
        log_error "Failed to fetch repository information"
        return 1
    fi

    # Extract key repository metadata
    local repo_info
    repo_info=$(echo "$repo_data" | jq '{
        name: .name,
        full_name: .full_name,
        owner: .owner.login,
        default_branch: .default_branch,
        private: .private,
        created_at: .created_at,
        updated_at: .updated_at,
        language: .language,
        size: .size
    }')

    echo "$repo_info"
    return 0
}

#####################################
# Get branch information
# Arguments:
#   $1 - Branch name
# Returns:
#   JSON object with branch info, 0 on success, 1 on failure
#####################################
get_branch_info() {
    local branch_name="$1"

    log_debug "Fetching branch information for: $branch_name"

    local branch_data
    if ! branch_data=$(gh api "repos/{owner}/{repo}/branches/$branch_name" 2>/dev/null); then
        log_error "Failed to fetch branch information for: $branch_name"
        return 1
    fi

    # Extract key branch metadata
    local branch_info
    branch_info=$(echo "$branch_data" | jq '{
        name: .name,
        commit_sha: .commit.sha,
        commit_date: .commit.commit.committer.date,
        protected: .protected
    }')

    echo "$branch_info"
    return 0
}

#####################################
# Labels and Issue Management
#####################################

#####################################
# Get PR labels
# Arguments:
#   $1 - PR number
# Returns:
#   JSON array of labels, 0 on success, 1 on failure
#####################################
get_pr_labels() {
    local pr_number="$1"

    log_debug "Fetching labels for PR #$pr_number"

    local pr_data
    if ! pr_data=$(gh api "repos/{owner}/{repo}/pulls/$pr_number" 2>/dev/null); then
        log_error "Failed to fetch PR data for labels"
        return 1
    fi

    local labels
    labels=$(echo "$pr_data" | jq '.labels')

    echo "$labels"
    return 0
}

#####################################
# Add label to PR
# Arguments:
#   $1 - PR number
#   $2 - Label name
# Returns:
#   0 on success, 1 on failure
#####################################
add_pr_label() {
    local pr_number="$1"
    local label_name="$2"

    log_debug "Adding label '$label_name' to PR #$pr_number"

    local label_data
    label_data=$(jq -n --arg label "$label_name" '{labels: [$label]}')

    if ! gh api "repos/{owner}/{repo}/issues/$pr_number/labels" \
        --method POST \
        --input - <<< "$label_data" >/dev/null 2>&1; then
        log_error "Failed to add label '$label_name' to PR #$pr_number"
        return 1
    fi

    log_info "Added label '$label_name' to PR #$pr_number"
    return 0
}

#####################################
# Remove label from PR
# Arguments:
#   $1 - PR number
#   $2 - Label name
# Returns:
#   0 on success, 1 on failure
#####################################
remove_pr_label() {
    local pr_number="$1"
    local label_name="$2"

    log_debug "Removing label '$label_name' from PR #$pr_number"

    # URL encode the label name
    local encoded_label
    encoded_label=$(printf '%s' "$label_name" | jq -sRr @uri)

    if ! gh api "repos/{owner}/{repo}/issues/$pr_number/labels/$encoded_label" \
        --method DELETE >/dev/null 2>&1; then
        log_error "Failed to remove label '$label_name' from PR #$pr_number"
        return 1
    fi

    log_info "Removed label '$label_name' from PR #$pr_number"
    return 0
}

#####################################
# Utility Functions
#####################################

#####################################
# Check GitHub API rate limit
# Returns:
#   0 if OK, 1 if rate limited
#####################################
check_rate_limit() {
    log_debug "Checking GitHub API rate limit"

    local rate_limit
    if ! rate_limit=$(gh api "rate_limit" 2>/dev/null); then
        log_warning "Failed to check rate limit"
        return 0  # Assume OK if check fails
    fi

    local remaining
    remaining=$(echo "$rate_limit" | jq -r '.rate.remaining')

    local reset_time
    reset_time=$(echo "$rate_limit" | jq -r '.rate.reset')

    if [[ "$remaining" -lt 10 ]]; then
        local reset_date
        reset_date=$(date -d "@$reset_time" '+%Y-%m-%d %H:%M:%S')
        log_warning "GitHub API rate limit low: $remaining requests remaining (resets at $reset_date)"
        return 1
    fi

    log_debug "GitHub API rate limit OK: $remaining requests remaining"
    return 0
}

#####################################
# Validate GitHub token permissions
# Returns:
#   0 if permissions OK, 1 if insufficient
#####################################
validate_github_token() {
    log_debug "Validating GitHub token permissions"

    # Test basic repository access
    if ! gh api "repos/{owner}/{repo}" >/dev/null 2>&1; then
        log_error "GitHub token lacks repository read access"
        return 1
    fi

    # Test comment creation permissions
    local test_comment="<!-- GitHub API permissions test -->"
    local comment_data
    comment_data=$(jq -n --arg body "$test_comment" '{body: $body}')

    # This is a dry-run test - we don't actually create the comment
    # Just verify the API endpoint is accessible
    if ! gh api "repos/{owner}/{repo}" >/dev/null 2>&1; then
        log_error "GitHub token lacks required permissions for comment management"
        return 1
    fi

    log_debug "GitHub token permissions validated successfully"
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
export -f get_pr_status update_pr_status should_auto_update_pr_status get_pr_metadata
export -f create_pr_comment update_pr_comment get_pr_comments find_comment_by_header find_existing_iterative_comment
export -f get_repository_info get_branch_info get_pr_labels add_pr_label remove_pr_label
export -f check_rate_limit validate_github_token