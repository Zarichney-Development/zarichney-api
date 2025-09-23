#!/usr/bin/env bats
# Unit Tests for Iterative AI Review Action - Comment Manager
# Tests comment lifecycle management, content generation, and context preservation

# Setup test environment
setup() {
    # Load test fixtures and mocks
    load "../../fixtures/github-environment.sh"
    load "../../mocks/github-api-mock.sh"

    # Initialize test environment
    initialize_test_environment
    setup_github_api_mocking

    # Source the comment-manager module under test
    source "${BATS_TEST_DIRNAME}/../../../src/comment-manager.js"

    # Setup test-specific variables
    export TEST_PR_NUMBER="123"
    export TEST_ITERATION_COUNT="2"
    export TEST_COMMENT_ID="123456789"
}

# Cleanup after each test
teardown() {
    cleanup_test_environment
    cleanup_github_api_mocking
}

#####################################
# Test Comment Detection and Parsing
#####################################

@test "detect_existing_iterative_comment finds existing comment" {
    setup_scenario_subsequent_iteration

    # Execute function
    run detect_existing_iterative_comment "$TEST_PR_NUMBER"

    # Should find existing iterative comment
    [ "$status" -eq 0 ]
    [[ "$output" == *"$TEST_COMMENT_ID"* ]] || [ "$output" != "" ]
}

@test "detect_existing_iterative_comment handles no existing comment" {
    setup_scenario_first_iteration

    # Execute function
    run detect_existing_iterative_comment "$TEST_PR_NUMBER"

    # Should handle no existing comment
    [ "$status" -eq 0 ]
    [ "$output" = "" ] || [[ "$output" == "null" ]]
}

@test "detect_existing_iterative_comment validates header pattern" {
    setup_scenario_subsequent_iteration

    # Create comment with valid header
    local test_comment="# 🔄 Iterative AI Code Review - Iteration 2"

    # Execute function
    run detect_existing_iterative_comment "$TEST_PR_NUMBER"

    # Should validate header pattern correctly
    [ "$status" -eq 0 ]
}

@test "detect_existing_iterative_comment ignores non-iterative comments" {
    setup_scenario_first_iteration

    # Mock regular comments without iterative header
    export MOCK_REGULAR_COMMENTS_ONLY="true"

    # Execute function
    run detect_existing_iterative_comment "$TEST_PR_NUMBER"

    # Should ignore non-iterative comments
    [ "$status" -eq 0 ]
    [ "$output" = "" ]
}

#####################################
# Test Historical Context Extraction
#####################################

@test "extract_historical_context_from_comment extracts context correctly" {
    local test_comment
    test_comment=$(create_mock_existing_comment 2)

    # Execute function
    run extract_historical_context_from_comment "$test_comment"

    # Should extract historical context
    [ "$status" -eq 0 ]
    [[ "$output" == *"iteration"* ]]
    [[ "$output" == *"progress"* ]]
}

@test "extract_historical_context_from_comment handles malformed comment" {
    local malformed_comment="Invalid comment without proper structure"

    # Execute function
    run extract_historical_context_from_comment "$malformed_comment"

    # Should handle malformed comment gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == "{}" ]] || [[ "$output" == *"error"* ]]
}

@test "extract_historical_context_from_comment extracts todo progress" {
    local test_comment
    test_comment=$(create_mock_existing_comment 3)

    # Execute function
    run extract_historical_context_from_comment "$test_comment"

    # Should extract todo progress information
    [ "$status" -eq 0 ]
    [[ "$output" == *"completed"* ]] || [[ "$output" == *"pending"* ]]
}

@test "extract_iteration_count_from_comment extracts iteration number" {
    local test_comment="# 🔄 Iterative AI Code Review - Iteration 5"

    # Execute function
    run extract_iteration_count_from_comment "$test_comment"

    # Should extract iteration count
    [ "$status" -eq 0 ]
    [[ "$output" == "5" ]]
}

@test "extract_iteration_count_from_comment handles missing iteration" {
    local test_comment="# Regular comment without iteration number"

    # Execute function
    run extract_iteration_count_from_comment "$test_comment"

    # Should handle missing iteration gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == "0" ]] || [[ "$output" == "" ]]
}

#####################################
# Test Comment Content Generation
#####################################

@test "generate_iterative_comment_body creates structured comment" {
    local ai_analysis="Test AI analysis with recommendations"
    local iteration_count="1"
    local todo_list='[{"id":"todo-001","category":"HIGH","description":"Test todo"}]'
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should generate structured comment
    [ "$status" -eq 0 ]
    [[ "$output" == *"Iterative AI Code Review"* ]]
    [[ "$output" == *"Iteration $iteration_count"* ]]
    [[ "$output" == *"To-Do List"* ]]
}

@test "generate_iterative_comment_body includes Epic context" {
    local ai_analysis="Analysis with Epic context"
    local iteration_count="2"
    local todo_list="[]"
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should include Epic #181 context
    [ "$status" -eq 0 ]
    [[ "$output" == *"Epic #181"* ]]
}

@test "generate_iterative_comment_body formats todo items correctly" {
    local ai_analysis="Analysis"
    local iteration_count="1"
    local todo_list='[
        {
            "id": "todo-001",
            "category": "CRITICAL",
            "description": "Fix security vulnerability",
            "file_references": ["src/auth.js:45"],
            "status": "pending"
        },
        {
            "id": "todo-002",
            "category": "HIGH",
            "description": "Add unit tests",
            "file_references": ["src/utils.js:123"],
            "status": "completed"
        }
    ]'
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should format todo items with proper categorization
    [ "$status" -eq 0 ]
    [[ "$output" == *"CRITICAL"* ]]
    [[ "$output" == *"HIGH"* ]]
    [[ "$output" == *"Fix security vulnerability"* ]]
    [[ "$output" == *"Add unit tests"* ]]
    [[ "$output" == *"✅"* ]] # Completed items should show checkmark
}

@test "generate_iterative_comment_body includes progress statistics" {
    local ai_analysis="Analysis"
    local iteration_count="3"
    local todo_list='[
        {"id":"todo-001","category":"HIGH","status":"completed"},
        {"id":"todo-002","category":"MEDIUM","status":"pending"},
        {"id":"todo-003","category":"LOW","status":"pending"}
    ]'
    local historical_context='{"progress_metrics":{"total_todos_created":5,"total_todos_resolved":2}}'

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should include progress statistics
    [ "$status" -eq 0 ]
    [[ "$output" == *"Progress Summary"* ]]
    [[ "$output" == *"Completed"* ]]
    [[ "$output" == *"1"* ]] # One completed item
    [[ "$output" == *"2"* ]] # Two pending items
}

@test "generate_iterative_comment_body handles empty todo list" {
    local ai_analysis="Analysis with no todos"
    local iteration_count="1"
    local todo_list="[]"
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should handle empty todo list gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == *"No specific"* ]] || [[ "$output" == *"good shape"* ]]
}

#####################################
# Test Context Preservation and Carry-Forward
#####################################

@test "generate_carryforward_json creates context for next iteration" {
    local current_todos='[
        {"id":"todo-001","category":"CRITICAL","status":"pending"},
        {"id":"todo-002","category":"HIGH","status":"completed"}
    ]'
    local progress_metrics='{"total_todos_created":5,"total_todos_resolved":2}'
    local iteration_count="2"

    # Execute function
    run generate_carryforward_json "$current_todos" "$progress_metrics" "$iteration_count"

    # Should generate carry-forward context
    [ "$status" -eq 0 ]
    [[ "$output" == *"carry_forward_items"* ]]
    [[ "$output" == *"focus_areas"* ]]
    [[ "$output" == *"epic_progress_notes"* ]]
}

@test "generate_carryforward_json identifies focus areas" {
    local current_todos='[
        {"id":"todo-001","category":"CRITICAL","description":"Security fix","status":"pending"},
        {"id":"todo-002","category":"HIGH","description":"Add tests","status":"pending"}
    ]'
    local progress_metrics="{}"
    local iteration_count="1"

    # Execute function
    run generate_carryforward_json "$current_todos" "$progress_metrics" "$iteration_count"

    # Should identify focus areas based on pending todos
    [ "$status" -eq 0 ]
    [[ "$output" == *"security"* ]] || [[ "$output" == *"testing"* ]]
}

@test "generate_carryforward_json handles completed todos" {
    local current_todos='[
        {"id":"todo-001","category":"HIGH","status":"completed"},
        {"id":"todo-002","category":"MEDIUM","status":"completed"}
    ]'
    local progress_metrics='{"total_todos_resolved":2}'
    local iteration_count="3"

    # Execute function
    run generate_carryforward_json "$current_todos" "$progress_metrics" "$iteration_count"

    # Should handle all completed todos appropriately
    [ "$status" -eq 0 ]
    [[ "$output" == *"ready"* ]] || [[ "$output" == *"complete"* ]]
}

#####################################
# Test Template Integration
#####################################

@test "comment generation uses iterative review template structure" {
    local ai_analysis="Template test analysis"
    local iteration_count="1"
    local todo_list="[]"
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should follow template structure
    [ "$status" -eq 0 ]
    [[ "$output" == *"🔄"* ]] # Header emoji
    [[ "$output" == *"📋"* ]] # Todo list emoji
    [[ "$output" == *"📊"* ]] # Progress emoji
    [[ "$output" == *"🎯"* ]] # Epic alignment emoji
    [[ "$output" == *"🚦"* ]] # Quality gates emoji
}

@test "comment generation includes timestamp" {
    local ai_analysis="Timestamp test"
    local iteration_count="1"
    local todo_list="[]"
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should include timestamp
    [ "$status" -eq 0 ]
    [[ "$output" == *"Last updated"* ]]
    [[ "$output" == *"$(date +%Y)"* ]] # Current year should be present
}

@test "comment generation includes action attribution" {
    local ai_analysis="Attribution test"
    local iteration_count="1"
    local todo_list="[]"
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should include action attribution
    [ "$status" -eq 0 ]
    [[ "$output" == *"Generated by"* ]]
    [[ "$output" == *"Iterative AI"* ]]
}

#####################################
# Test Quality Gate Integration
#####################################

@test "comment generation includes quality gate status" {
    local ai_analysis="Quality gates analysis"
    local iteration_count="2"
    local todo_list="[]"
    local historical_context='{"quality_gates":{"tests_passing":true,"coverage_threshold":false,"security_scan":true}}'

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should include quality gate status
    [ "$status" -eq 0 ]
    [[ "$output" == *"Quality Gates"* ]]
    [[ "$output" == *"Tests Passing"* ]]
    [[ "$output" == *"Coverage"* ]]
    [[ "$output" == *"Security"* ]]
    [[ "$output" == *"✅"* ]] # Passing gates should show checkmark
    [[ "$output" == *"❌"* ]] # Failing gates should show X
}

@test "comment generation provides next iteration guidance" {
    local ai_analysis="Next iteration guidance analysis"
    local iteration_count="2"
    local todo_list='[{"id":"todo-001","category":"HIGH","status":"pending"}]'
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should provide next iteration guidance
    [ "$status" -eq 0 ]
    [[ "$output" == *"Next Iteration"* ]]
    [[ "$output" == *"Focus"* ]]
}

#####################################
# Test Error Handling and Edge Cases
#####################################

@test "comment generation handles malformed JSON input" {
    local ai_analysis="Analysis"
    local iteration_count="1"
    local todo_list="invalid json"
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should handle malformed JSON gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == *"Error"* ]] || [[ "$output" != *"invalid json"* ]]
}

@test "comment generation handles missing fields" {
    local ai_analysis=""
    local iteration_count=""
    local todo_list=""
    local historical_context=""

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should handle missing fields gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == *"Iterative AI"* ]] # Should still generate basic structure
}

@test "comment generation handles very large content" {
    local ai_analysis=$(printf 'Large analysis %.0s' {1..500})
    local iteration_count="1"
    local large_todo_list='['
    for i in {1..100}; do
        large_todo_list+="{\"id\":\"todo-$(printf "%03d" $i)\",\"category\":\"MEDIUM\",\"description\":\"Todo item $i\",\"status\":\"pending\"},"
    done
    large_todo_list="${large_todo_list%,}]"
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$large_todo_list" "$historical_context"

    # Should handle large content appropriately
    [ "$status" -eq 0 ]
    # Should either complete successfully or truncate appropriately
    [[ ${#output} -lt 100000 ]] # GitHub comment size limit consideration
}

@test "comment generation sanitizes dangerous content" {
    local ai_analysis="Analysis with <script>alert('xss')</script> content"
    local iteration_count="1"
    local todo_list='[{"id":"todo-001","description":"<img src=x onerror=alert(1)>","category":"HIGH","status":"pending"}]'
    local historical_context="{}"

    # Execute function
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"

    # Should sanitize dangerous content
    [ "$status" -eq 0 ]
    [[ "$output" != *"<script>"* ]]
    [[ "$output" != *"onerror="* ]]
}

#####################################
# Test Performance and Efficiency
#####################################

@test "comment generation completes within reasonable time" {
    local ai_analysis="Performance test analysis"
    local iteration_count="2"
    local todo_list='[{"id":"todo-001","category":"HIGH","description":"Performance test","status":"pending"}]'
    local historical_context="{}"

    # Measure execution time
    local start_time=$(date +%s)
    run generate_iterative_comment_body "$ai_analysis" "$iteration_count" "$todo_list" "$historical_context"
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))

    # Should complete quickly (under 5 seconds)
    [ "$status" -eq 0 ]
    [ "$duration" -lt 5 ]
}

@test "comment detection handles large comment lists efficiently" {
    # Mock large number of comments
    export MOCK_LARGE_COMMENT_LIST="true"

    # Execute function
    run detect_existing_iterative_comment "$TEST_PR_NUMBER"

    # Should handle large lists efficiently
    [ "$status" -eq 0 ]
}

@test "historical context extraction handles complex nested data" {
    local complex_comment
    complex_comment=$(cat << 'EOF'
# 🔄 Iterative AI Code Review - Iteration 3

## 📋 To-Do List Progress

### Critical Issues (🔴 Must Fix Before Merge)
- [x] **todo-001**: Complex security fix with nested validation
  - **Sub-items**: Input validation, Output sanitization, Error handling
  - **Dependencies**: External library updates, Configuration changes

### Historical Context Summary:
- Iteration 1: Initial implementation (15 todos)
- Iteration 2: Security hardening (8 todos resolved)
- Iteration 3: Performance optimization (5 todos remaining)

## 📊 Progress Summary
- Total Issues: 28
- Completed: 23 (82%)
- Critical Remaining: 2
- High Priority: 2
- Medium Priority: 1
EOF
)

    # Execute function
    run extract_historical_context_from_comment "$complex_comment"

    # Should handle complex nested data
    [ "$status" -eq 0 ]
    [[ "$output" == *"iteration"* ]]
}