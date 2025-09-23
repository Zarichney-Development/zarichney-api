#!/bin/bash
# GitHub API Mock Functions for Iterative AI Review Action Tests
# Provides realistic mock responses for all GitHub API interactions

set -euo pipefail

#####################################
# Mock GitHub API curl wrapper
# Arguments:
#   All arguments passed to curl
# Returns:
#   Mock response based on URL pattern
#####################################
mock_curl() {
    local url=""
    local method="GET"
    local data=""
    local headers=()

    # Parse curl arguments to extract URL and method
    while [[ $# -gt 0 ]]; do
        case $1 in
            -X|--request)
                method="$2"
                shift 2
                ;;
            -d|--data)
                data="$2"
                shift 2
                ;;
            -H|--header)
                headers+=("$2")
                shift 2
                ;;
            -s|--silent|-f|--fail|-L|--location)
                # Ignore common curl flags
                shift
                ;;
            https://api.github.com/*)
                url="$1"
                shift
                ;;
            *)
                shift
                ;;
        esac
    done

    # Route to appropriate mock response based on URL pattern
    case "$url" in
        */repos/*/pulls/*)
            if [[ "$method" == "PATCH" ]]; then
                mock_update_pr_response
            else
                mock_get_pr_response
            fi
            ;;
        */repos/*/issues/*/comments*)
            if [[ "$method" == "POST" ]]; then
                mock_create_comment_response
            elif [[ "$method" == "PATCH" ]]; then
                mock_update_comment_response
            else
                mock_list_comments_response
            fi
            ;;
        */rate_limit*)
            mock_rate_limit_response
            ;;
        */repos/*)
            mock_repository_info_response
            ;;
        *)
            echo '{"error": "Mock API endpoint not implemented for: '"$url"'"}' >&2
            return 1
            ;;
    esac
}

#####################################
# Mock PR details response
#####################################
mock_get_pr_response() {
    if [[ -f "${MOCK_API_RESPONSES_DIR:-}/pr-details.json" ]]; then
        cat "${MOCK_API_RESPONSES_DIR}/pr-details.json"
    else
        cat << EOF
{
  "number": ${MOCK_PR_NUMBER:-123},
  "state": "open",
  "draft": ${MOCK_PR_DRAFT:-true},
  "title": "${MOCK_PR_TITLE:-feat: test PR}",
  "body": "Test PR body",
  "user": {
    "login": "${MOCK_PR_AUTHOR:-test-user}"
  },
  "head": {
    "sha": "${GITHUB_SHA:-abc123}",
    "ref": "${MOCK_SOURCE_BRANCH:-feature/test}"
  },
  "base": {
    "sha": "def456",
    "ref": "${MOCK_TARGET_BRANCH:-develop}"
  },
  "changed_files": ${MOCK_CHANGED_FILES_COUNT:-5},
  "additions": 100,
  "deletions": 10,
  "labels": []
}
EOF
    fi
}

#####################################
# Mock PR update response
#####################################
mock_update_pr_response() {
    # Parse the update data to determine new state
    local new_draft="false"
    if [[ "${data:-}" == *'"draft":true'* ]]; then
        new_draft="true"
    fi

    cat << EOF
{
  "number": ${MOCK_PR_NUMBER:-123},
  "state": "open",
  "draft": $new_draft,
  "title": "${MOCK_PR_TITLE:-feat: test PR}",
  "message": "PR status updated successfully"
}
EOF
}

#####################################
# Mock comments list response
#####################################
mock_list_comments_response() {
    # Check if there's an existing iterative comment in the mock data
    if [[ "${MOCK_HAS_EXISTING_COMMENT:-false}" == "true" ]]; then
        cat << EOF
[
  {
    "id": 123456789,
    "body": "$(create_mock_existing_comment | sed 's/"/\\"/g' | tr '\n' '\\' | sed 's/\\\$/\\n/g')",
    "user": {
      "login": "github-actions[bot]"
    },
    "created_at": "2025-09-23T10:00:00Z",
    "updated_at": "2025-09-23T11:00:00Z"
  },
  {
    "id": 123456790,
    "body": "Regular comment from reviewer",
    "user": {
      "login": "reviewer-user"
    },
    "created_at": "2025-09-23T09:00:00Z",
    "updated_at": "2025-09-23T09:00:00Z"
  }
]
EOF
    else
        cat << 'EOF'
[
  {
    "id": 123456790,
    "body": "Regular comment from reviewer",
    "user": {
      "login": "reviewer-user"
    },
    "created_at": "2025-09-23T09:00:00Z",
    "updated_at": "2025-09-23T09:00:00Z"
  }
]
EOF
    fi
}

#####################################
# Mock create comment response
#####################################
mock_create_comment_response() {
    cat << EOF
{
  "id": $((RANDOM + 100000000)),
  "body": "Mock comment created successfully",
  "user": {
    "login": "github-actions[bot]"
  },
  "created_at": "$(date --iso-8601=seconds)",
  "updated_at": "$(date --iso-8601=seconds)",
  "html_url": "https://github.com/${GITHUB_REPOSITORY:-test/repo}/pull/${MOCK_PR_NUMBER:-123}#issuecomment-$((RANDOM + 100000000))"
}
EOF
}

#####################################
# Mock update comment response
#####################################
mock_update_comment_response() {
    cat << EOF
{
  "id": 123456789,
  "body": "Mock comment updated successfully",
  "user": {
    "login": "github-actions[bot]"
  },
  "created_at": "2025-09-23T10:00:00Z",
  "updated_at": "$(date --iso-8601=seconds)",
  "html_url": "https://github.com/${GITHUB_REPOSITORY:-test/repo}/pull/${MOCK_PR_NUMBER:-123}#issuecomment-123456789"
}
EOF
}

#####################################
# Mock rate limit response
#####################################
mock_rate_limit_response() {
    cat << 'EOF'
{
  "rate": {
    "limit": 5000,
    "remaining": 4950,
    "reset": 1695456000,
    "used": 50
  }
}
EOF
}

#####################################
# Mock repository info response
#####################################
mock_repository_info_response() {
    cat << EOF
{
  "id": 123456789,
  "name": "${GITHUB_REPOSITORY##*/}",
  "full_name": "${GITHUB_REPOSITORY:-test/repo}",
  "owner": {
    "login": "${GITHUB_REPOSITORY_OWNER:-test-owner}"
  },
  "default_branch": "main",
  "topics": ["dotnet", "api", "testing"]
}
EOF
}

#####################################
# Mock AI service response (OpenAI API)
#####################################
mock_openai_api_response() {
    cat << 'EOF'
{
  "id": "chatcmpl-test123",
  "object": "chat.completion",
  "created": 1695456000,
  "model": "gpt-4",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "# AI Code Review Analysis\n\n## Summary\nThe code changes look good overall. Here are some recommendations:\n\n## To-Do Items\n1. **MEDIUM**: Add error handling for edge cases\n2. **LOW**: Improve code documentation\n3. **MEDIUM**: Add unit tests for new functionality\n\n## Quality Assessment\n- Code follows established patterns\n- No security concerns identified\n- Performance looks acceptable\n\n## Recommendations\n- Consider adding more comprehensive tests\n- Documentation could be enhanced\n- Overall implementation is solid"
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 1500,
    "completion_tokens": 200,
    "total_tokens": 1700
  }
}
EOF
}

#####################################
# Setup GitHub API mocking
# Overrides curl and gh commands with mock implementations
#####################################
setup_github_api_mocking() {
    # Override curl command for GitHub API calls
    curl() {
        mock_curl "$@"
    }
    export -f curl

    # Override gh command for GitHub CLI calls
    gh() {
        case "$1" in
            api)
                shift
                mock_gh_api "$@"
                ;;
            pr)
                shift
                mock_gh_pr "$@"
                ;;
            *)
                echo "Mock gh command not implemented for: $1" >&2
                return 1
                ;;
        esac
    }
    export -f gh
}

#####################################
# Mock gh api command
#####################################
mock_gh_api() {
    local endpoint=""
    local method="GET"

    while [[ $# -gt 0 ]]; do
        case $1 in
            -X|--method)
                method="$2"
                shift 2
                ;;
            repos/*/pulls/*)
                endpoint="$1"
                shift
                ;;
            *)
                shift
                ;;
        esac
    done

    case "$endpoint" in
        *pulls*)
            mock_get_pr_response
            ;;
        *)
            echo '{"error": "Mock gh api endpoint not implemented"}' >&2
            return 1
            ;;
    esac
}

#####################################
# Mock gh pr command
#####################################
mock_gh_pr() {
    case "$1" in
        view)
            mock_get_pr_response
            ;;
        edit)
            mock_update_pr_response
            ;;
        *)
            echo "Mock gh pr command not implemented for: $1" >&2
            return 1
            ;;
    esac
}

#####################################
# Test scenario setup functions
#####################################

# Setup scenario: First iteration, no existing comment
setup_scenario_first_iteration() {
    export MOCK_HAS_EXISTING_COMMENT="false"
    export MOCK_ITERATION_COUNT="1"
    export MOCK_PR_DRAFT="true"
}

# Setup scenario: Subsequent iteration with existing comment
setup_scenario_subsequent_iteration() {
    export MOCK_HAS_EXISTING_COMMENT="true"
    export MOCK_ITERATION_COUNT="2"
    export MOCK_PR_DRAFT="true"
}

# Setup scenario: Ready for merge
setup_scenario_ready_for_merge() {
    export MOCK_HAS_EXISTING_COMMENT="true"
    export MOCK_ITERATION_COUNT="3"
    export MOCK_PR_DRAFT="false"
    export MOCK_ALL_CRITICAL_TODOS_RESOLVED="true"
}

# Setup scenario: Rate limit hit
setup_scenario_rate_limit() {
    # Override rate limit response to show exhausted limits
    mock_rate_limit_response() {
        cat << 'EOF'
{
  "rate": {
    "limit": 5000,
    "remaining": 0,
    "reset": 1695459600,
    "used": 5000
  }
}
EOF
    }
    export -f mock_rate_limit_response
}

#####################################
# Cleanup mocking
#####################################
cleanup_github_api_mocking() {
    unset -f curl gh mock_curl mock_gh_api mock_gh_pr
    unset -f mock_get_pr_response mock_update_pr_response
    unset -f mock_list_comments_response mock_create_comment_response
    unset -f mock_update_comment_response mock_rate_limit_response
    unset -f mock_repository_info_response mock_openai_api_response
    unset MOCK_HAS_EXISTING_COMMENT MOCK_ITERATION_COUNT MOCK_PR_DRAFT
    unset MOCK_ALL_CRITICAL_TODOS_RESOLVED
}

# Export all mock functions
export -f mock_curl mock_gh_api mock_gh_pr
export -f mock_get_pr_response mock_update_pr_response
export -f mock_list_comments_response mock_create_comment_response
export -f mock_update_comment_response mock_rate_limit_response
export -f mock_repository_info_response mock_openai_api_response
export -f setup_github_api_mocking cleanup_github_api_mocking
export -f setup_scenario_first_iteration setup_scenario_subsequent_iteration
export -f setup_scenario_ready_for_merge setup_scenario_rate_limit