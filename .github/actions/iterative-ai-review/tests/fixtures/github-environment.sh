#!/bin/bash
# GitHub Actions Environment Fixtures for Iterative AI Review Action Tests
# Provides realistic test data and environment setup

set -euo pipefail

#####################################
# Setup mock GitHub Actions environment
# Globals:
#   Sets up all GitHub Actions environment variables
#####################################
setup_github_actions_environment() {
    # GitHub Actions standard environment variables
    export GITHUB_ACTIONS="true"
    export GITHUB_WORKSPACE="/github/workspace"
    export GITHUB_EVENT_NAME="pull_request"
    export GITHUB_REPOSITORY="Zarichney-Development/zarichney-api"
    export GITHUB_REPOSITORY_OWNER="Zarichney-Development"
    export GITHUB_ACTOR="test-user"
    export GITHUB_TOKEN="ghp_test_token_123456789abcdef"
    export GITHUB_SHA="abc123def456789"
    export GITHUB_REF="refs/pull/123/merge"
    export GITHUB_HEAD_REF="feature/issue-185-iterative-review"
    export GITHUB_BASE_REF="develop"
    export GITHUB_RUN_ID="123456789"
    export GITHUB_RUN_NUMBER="42"

    # GitHub API Base URL
    export GITHUB_API_URL="https://api.github.com"

    # Action-specific inputs (simulating inputs from action.yml)
    export INPUT_GITHUB_TOKEN="$GITHUB_TOKEN"
    export INPUT_OPENAI_API_KEY="sk-test_openai_key_123456789"
    export INPUT_PR_NUMBER="123"
    export INPUT_ITERATION_TRIGGER="auto"
    export INPUT_MAX_ITERATIONS="5"
    export INPUT_QUALITY_THRESHOLD="medium"
    export INPUT_EPIC_CONTEXT="Epic #181 autonomous development"
    export INPUT_DEBUG_MODE="false"
    export INPUT_FORCE_NEW_ITERATION="false"

    # Test directories
    export TEST_TEMP_DIR="/tmp/iterative-ai-review-test-$$"
    export TEST_GITHUB_CACHE_DIR="$TEST_TEMP_DIR/.github/cache"
    export TEST_WORKSPACE_DIR="$TEST_TEMP_DIR/workspace"

    # Create test directories
    mkdir -p "$TEST_TEMP_DIR"
    mkdir -p "$TEST_GITHUB_CACHE_DIR"
    mkdir -p "$TEST_WORKSPACE_DIR"
    mkdir -p "$TEST_TEMP_DIR/iterative-review"

    # GitHub event payload (simulated)
    export GITHUB_EVENT_PATH="$TEST_TEMP_DIR/event.json"
    cat > "$GITHUB_EVENT_PATH" << 'EOF'
{
  "action": "synchronize",
  "number": 123,
  "pull_request": {
    "id": 987654321,
    "number": 123,
    "state": "open",
    "draft": true,
    "title": "feat: implement iterative AI review action (#185)",
    "body": "Implements comprehensive iterative AI code review capability for Epic #181 autonomous development cycle.",
    "user": {
      "login": "test-user",
      "id": 12345
    },
    "head": {
      "sha": "abc123def456789",
      "ref": "feature/issue-185-iterative-review",
      "repo": {
        "full_name": "Zarichney-Development/zarichney-api"
      }
    },
    "base": {
      "sha": "def456abc123789",
      "ref": "develop",
      "repo": {
        "full_name": "Zarichney-Development/zarichney-api"
      }
    },
    "changed_files": 6,
    "additions": 850,
    "deletions": 15,
    "commits": 3
  },
  "repository": {
    "id": 123456789,
    "name": "zarichney-api",
    "full_name": "Zarichney-Development/zarichney-api",
    "owner": {
      "login": "Zarichney-Development",
      "id": 98765
    }
  },
  "sender": {
    "login": "test-user",
    "id": 12345
  }
}
EOF
}

#####################################
# Setup mock PR metadata for testing
# Returns:
#   Mock PR data in global variables
#####################################
setup_mock_pr_data() {
    # Mock PR metadata
    export MOCK_PR_NUMBER="123"
    export MOCK_PR_TITLE="feat: implement iterative AI review action (#185)"
    export MOCK_PR_STATUS="draft"
    export MOCK_PR_AUTHOR="test-user"
    export MOCK_SOURCE_BRANCH="feature/issue-185-iterative-review"
    export MOCK_TARGET_BRANCH="develop"
    export MOCK_CHANGED_FILES_COUNT="6"
    export MOCK_LINES_CHANGED="835"

    # Mock changed files
    export MOCK_CHANGED_FILES=$(cat << 'EOF'
.github/actions/iterative-ai-review/action.yml
.github/actions/iterative-ai-review/src/main.js
.github/actions/iterative-ai-review/src/github-api.js
.github/actions/iterative-ai-review/src/comment-manager.js
.github/actions/iterative-ai-review/src/iteration-tracker.js
.github/actions/iterative-ai-review/src/todo-manager.js
EOF
)

    # Mock PR labels
    export MOCK_PR_LABELS='["type: feature", "epic: 181", "area: workflows", "priority: high"]'
}

#####################################
# Create mock iteration state file
# Arguments:
#   $1 - Iteration count (default: 1)
#   $2 - Todo count (default: 5)
#####################################
create_mock_iteration_state() {
    local iteration_count="${1:-1}"
    local todo_count="${2:-5}"
    local state_file="$TEST_TEMP_DIR/iterative-review/pr-$MOCK_PR_NUMBER-state.json"

    mkdir -p "$(dirname "$state_file")"

    cat > "$state_file" << EOF
{
  "schema_version": "1.0",
  "pr_number": $MOCK_PR_NUMBER,
  "iteration_count": $iteration_count,
  "current_status": "draft",
  "historical_context": {
    "iteration_history": [
      {
        "iteration": 1,
        "timestamp": "2025-09-23T12:00:00Z",
        "summary": "Initial review identified $todo_count issues requiring attention",
        "todo_count": $todo_count,
        "resolved_count": 0,
        "quality_gates": {
          "tests_passing": true,
          "coverage_threshold": false,
          "security_scan": true,
          "standards_compliance": false
        }
      }
    ],
    "progress_metrics": {
      "total_todos_created": $todo_count,
      "total_todos_resolved": 0,
      "current_active_todos": $todo_count,
      "avg_resolution_time": "0 hours"
    }
  },
  "current_todo_list": [
    {
      "id": "todo-001",
      "category": "CRITICAL",
      "description": "Add comprehensive error handling for GitHub API failures",
      "file_references": ["src/github-api.js:45"],
      "epic_alignment": "Epic #181 reliability requirements",
      "iteration_added": 1,
      "iteration_updated": 1,
      "status": "pending",
      "validation_criteria": "Error handling covers all API failure scenarios",
      "dependencies": []
    },
    {
      "id": "todo-002",
      "category": "HIGH",
      "description": "Implement comment content validation and sanitization",
      "file_references": ["src/comment-manager.js:123"],
      "epic_alignment": "Epic #181 security requirements",
      "iteration_added": 1,
      "iteration_updated": 1,
      "status": "pending",
      "validation_criteria": "All comment content properly validated",
      "dependencies": []
    },
    {
      "id": "todo-003",
      "category": "MEDIUM",
      "description": "Add iteration count validation and safety limits",
      "file_references": ["src/iteration-tracker.js:67"],
      "epic_alignment": "Epic #181 autonomous cycle safety",
      "iteration_added": 1,
      "iteration_updated": 1,
      "status": "pending",
      "validation_criteria": "Iteration limits properly enforced",
      "dependencies": []
    },
    {
      "id": "todo-004",
      "category": "MEDIUM",
      "description": "Enhance todo list statistics and reporting",
      "file_references": ["src/todo-manager.js:234"],
      "epic_alignment": "Epic #181 progress tracking",
      "iteration_added": 1,
      "iteration_updated": 1,
      "status": "pending",
      "validation_criteria": "Comprehensive todo statistics available",
      "dependencies": []
    },
    {
      "id": "todo-005",
      "category": "LOW",
      "description": "Add comprehensive logging for debugging support",
      "file_references": ["src/main.js:89"],
      "epic_alignment": "Epic #181 observability requirements",
      "iteration_added": 1,
      "iteration_updated": 1,
      "status": "pending",
      "validation_criteria": "Debug logging covers all critical operations",
      "dependencies": []
    }
  ],
  "pr_metadata": {
    "initial_files_changed": 6,
    "current_files_changed": 6,
    "initial_lines_changed": 835,
    "current_lines_changed": 835,
    "epic_context": "Epic #181 autonomous development",
    "quality_gates": {
      "tests_passing": true,
      "coverage_threshold": false,
      "security_scan": true,
      "standards_compliance": false
    }
  },
  "context_for_next_iteration": {
    "focus_areas": ["error handling", "validation", "testing"],
    "carry_forward_items": ["todo-001", "todo-002", "todo-003"],
    "escalation_triggers": ["api_failure", "validation_failure"],
    "epic_progress_notes": "Contributing to autonomous development cycle reliability"
  }
}
EOF

    echo "$state_file"
}

#####################################
# Create mock existing comment for testing
# Arguments:
#   $1 - Iteration count (default: 2)
#####################################
create_mock_existing_comment() {
    local iteration_count="${1:-2}"

    cat << EOF
# 🔄 Iterative AI Code Review - Iteration $iteration_count

## 📋 To-Do List Progress

### Critical Issues (🔴 Must Fix Before Merge)
- [x] **todo-001**: Add comprehensive error handling for GitHub API failures
  - **File**: src/github-api.js:45
  - **Epic Alignment**: Epic #181 reliability requirements
  - **Status**: ✅ Completed in iteration $iteration_count

### High Priority Issues (🟡 Should Fix Before Merge)
- [ ] **todo-002**: Implement comment content validation and sanitization
  - **File**: src/comment-manager.js:123
  - **Epic Alignment**: Epic #181 security requirements
  - **Status**: 🔄 In Progress

### Medium Priority Issues (🟠 Consider for Next Iteration)
- [ ] **todo-003**: Add iteration count validation and safety limits
  - **File**: src/iteration-tracker.js:67
  - **Epic Alignment**: Epic #181 autonomous cycle safety
  - **Status**: ⏳ Pending

- [ ] **todo-004**: Enhance todo list statistics and reporting
  - **File**: src/todo-manager.js:234
  - **Epic Alignment**: Epic #181 progress tracking
  - **Status**: ⏳ Pending

### Low Priority Issues (🔵 Future Enhancement)
- [ ] **todo-005**: Add comprehensive logging for debugging support
  - **File**: src/main.js:89
  - **Epic Alignment**: Epic #181 observability requirements
  - **Status**: ⏳ Pending

## 📊 Progress Summary

- **Iteration**: $iteration_count
- **Total Issues**: 5
- **Completed**: 1 (20%)
- **In Progress**: 1 (20%)
- **Pending**: 3 (60%)

## 🎯 Epic #181 Alignment

This iteration continues to advance Epic #181 autonomous development objectives through:
- ✅ Improved reliability with enhanced error handling
- 🔄 Security validation for autonomous operations
- ⏳ Safety mechanisms for iteration control

## 🚦 Quality Gates Status

- ✅ **Tests Passing**: All automated tests are passing
- ❌ **Coverage Threshold**: Below 90% coverage requirement
- ✅ **Security Scan**: No security vulnerabilities detected
- ❌ **Standards Compliance**: Some coding standards violations remain

## 🔄 Next Iteration Focus

Based on current progress, the next iteration should prioritize:
1. **Comment validation and sanitization** (security critical)
2. **Iteration safety limits** (autonomous cycle protection)
3. **Test coverage improvement** (quality gate requirement)

## 📝 Historical Context

### Previous Iterations Summary:
- **Iteration 1**: Initial implementation with 5 identified improvement areas
- **Iteration $iteration_count**: Resolved critical error handling, progressing on security validation

---
*Last updated: $(date --iso-8601=seconds)*
*Generated by Iterative AI Code Review Action for Epic #181*
EOF
}

#####################################
# Setup mock GitHub API responses
# Creates files that simulate GitHub API responses
#####################################
setup_mock_github_api_responses() {
    local api_responses_dir="$TEST_TEMP_DIR/api-responses"
    mkdir -p "$api_responses_dir"

    # Mock PR details response
    cat > "$api_responses_dir/pr-details.json" << EOF
{
  "number": $MOCK_PR_NUMBER,
  "state": "open",
  "draft": true,
  "title": "$MOCK_PR_TITLE",
  "body": "Implements comprehensive iterative AI code review capability for Epic #181.",
  "user": {
    "login": "$MOCK_PR_AUTHOR"
  },
  "head": {
    "sha": "$GITHUB_SHA",
    "ref": "$MOCK_SOURCE_BRANCH"
  },
  "base": {
    "sha": "def456abc123789",
    "ref": "$MOCK_TARGET_BRANCH"
  },
  "changed_files": $MOCK_CHANGED_FILES_COUNT,
  "additions": 820,
  "deletions": 15,
  "labels": [
    {"name": "type: feature"},
    {"name": "epic: 181"},
    {"name": "area: workflows"},
    {"name": "priority: high"}
  ]
}
EOF

    # Mock comments list response
    cat > "$api_responses_dir/comments-list.json" << 'EOF'
[
  {
    "id": 123456789,
    "body": "Great work on implementing the iterative review action! This will be valuable for Epic #181.",
    "user": {
      "login": "reviewer-user"
    },
    "created_at": "2025-09-23T10:00:00Z",
    "updated_at": "2025-09-23T10:00:00Z"
  }
]
EOF

    # Mock rate limit response
    cat > "$api_responses_dir/rate-limit.json" << 'EOF'
{
  "rate": {
    "limit": 5000,
    "remaining": 4950,
    "reset": 1695456000,
    "used": 50
  }
}
EOF

    # Mock repository info response
    cat > "$api_responses_dir/repository-info.json" << EOF
{
  "id": 123456789,
  "name": "zarichney-api",
  "full_name": "$GITHUB_REPOSITORY",
  "owner": {
    "login": "$GITHUB_REPOSITORY_OWNER"
  },
  "default_branch": "main",
  "topics": ["dotnet", "api", "ai-assisted"]
}
EOF

    export MOCK_API_RESPONSES_DIR="$api_responses_dir"
}

#####################################
# Cleanup test environment
# Removes all temporary test files and directories
#####################################
cleanup_test_environment() {
    if [[ -n "${TEST_TEMP_DIR:-}" ]] && [[ -d "$TEST_TEMP_DIR" ]]; then
        rm -rf "$TEST_TEMP_DIR"
    fi

    # Unset test environment variables
    unset GITHUB_ACTIONS GITHUB_WORKSPACE GITHUB_EVENT_NAME
    unset GITHUB_REPOSITORY GITHUB_REPOSITORY_OWNER GITHUB_ACTOR
    unset GITHUB_TOKEN GITHUB_SHA GITHUB_REF GITHUB_HEAD_REF GITHUB_BASE_REF
    unset GITHUB_RUN_ID GITHUB_RUN_NUMBER GITHUB_API_URL GITHUB_EVENT_PATH
    unset INPUT_GITHUB_TOKEN INPUT_OPENAI_API_KEY INPUT_PR_NUMBER
    unset INPUT_ITERATION_TRIGGER INPUT_MAX_ITERATIONS INPUT_QUALITY_THRESHOLD
    unset INPUT_EPIC_CONTEXT INPUT_DEBUG_MODE INPUT_FORCE_NEW_ITERATION
    unset TEST_TEMP_DIR TEST_GITHUB_CACHE_DIR TEST_WORKSPACE_DIR
    unset MOCK_PR_NUMBER MOCK_PR_TITLE MOCK_PR_STATUS MOCK_PR_AUTHOR
    unset MOCK_SOURCE_BRANCH MOCK_TARGET_BRANCH MOCK_CHANGED_FILES_COUNT
    unset MOCK_LINES_CHANGED MOCK_CHANGED_FILES MOCK_PR_LABELS
    unset MOCK_API_RESPONSES_DIR
}

#####################################
# Initialize complete test environment
# Sets up all fixtures, mocks, and test data
#####################################
initialize_test_environment() {
    setup_github_actions_environment
    setup_mock_pr_data
    setup_mock_github_api_responses
    create_mock_iteration_state 1 5
}

# Export all functions for use in tests
export -f setup_github_actions_environment
export -f setup_mock_pr_data
export -f create_mock_iteration_state
export -f create_mock_existing_comment
export -f setup_mock_github_api_responses
export -f cleanup_test_environment
export -f initialize_test_environment