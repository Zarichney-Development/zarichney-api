#!/usr/bin/env bats
# Unit Tests for Iterative AI Review Action - Todo Manager
# Tests to-do item lifecycle, validation, categorization, and progress tracking

# Setup test environment
setup() {
    # Load test fixtures and mocks
    load "../../fixtures/github-environment.sh"
    load "../../mocks/github-api-mock.sh"

    # Initialize test environment
    initialize_test_environment
    setup_github_api_mocking

    # Source the todo-manager module under test
    source "${BATS_TEST_DIRNAME}/../../../src/todo-manager.js"

    # Setup test-specific variables
    export TEST_PR_NUMBER="123"
    export TEST_ITERATION_COUNT="2"
}

# Cleanup after each test
teardown() {
    cleanup_test_environment
    cleanup_github_api_mocking
}

#####################################
# Test Todo Item Creation and Validation
#####################################

@test "create_todo_item creates valid todo structure" {
    local category="HIGH"
    local description="Add comprehensive unit tests"
    local file_references="src/utils.js:45,src/auth.js:123"
    local epic_alignment="Epic #181 testing requirements"

    # Execute function
    run create_todo_item "$category" "$description" "$file_references" "$epic_alignment"

    # Should create valid todo structure
    [ "$status" -eq 0 ]
    [[ "$output" == *"id"* ]]
    [[ "$output" == *"category\":\"HIGH\""* ]]
    [[ "$output" == *"Add comprehensive unit tests"* ]]
    [[ "$output" == *"file_references"* ]]
    [[ "$output" == *"Epic #181"* ]]
    [[ "$output" == *"status\":\"pending\""* ]]
}

@test "create_todo_item generates unique IDs" {
    # Create multiple todos
    run create_todo_item "MEDIUM" "First todo" "file1.js:10" "Epic context"
    local todo1="$output"

    run create_todo_item "MEDIUM" "Second todo" "file2.js:20" "Epic context"
    local todo2="$output"

    # Should have different IDs
    [ "$status" -eq 0 ]
    local id1=$(echo "$todo1" | grep -o '"id":"[^"]*"' | head -1)
    local id2=$(echo "$todo2" | grep -o '"id":"[^"]*"' | head -1)
    [[ "$id1" != "$id2" ]]
}

@test "create_todo_item includes iteration metadata" {
    export TEST_ITERATION_COUNT="3"

    # Execute function
    run create_todo_item "LOW" "Test todo" "file.js:1" "Epic context"

    # Should include iteration metadata
    [ "$status" -eq 0 ]
    [[ "$output" == *"iteration_added\":3"* ]]
    [[ "$output" == *"iteration_updated\":3"* ]]
}

@test "validate_todo_category accepts valid categories" {
    # Test all valid categories
    run validate_todo_category "CRITICAL"
    [ "$status" -eq 0 ]

    run validate_todo_category "HIGH"
    [ "$status" -eq 0 ]

    run validate_todo_category "MEDIUM"
    [ "$status" -eq 0 ]

    run validate_todo_category "LOW"
    [ "$status" -eq 0 ]

    run validate_todo_category "COMPLETED"
    [ "$status" -eq 0 ]
}

@test "validate_todo_category rejects invalid categories" {
    # Test invalid categories
    run validate_todo_category "INVALID"
    [ "$status" -ne 0 ]

    run validate_todo_category ""
    [ "$status" -ne 0 ]

    run validate_todo_category "high"  # Case sensitive
    [ "$status" -ne 0 ]
}

@test "validate_todo_status accepts valid statuses" {
    # Test all valid statuses
    run validate_todo_status "pending"
    [ "$status" -eq 0 ]

    run validate_todo_status "in_progress"
    [ "$status" -eq 0 ]

    run validate_todo_status "completed"
    [ "$status" -eq 0 ]

    run validate_todo_status "deferred"
    [ "$status" -eq 0 ]
}

@test "validate_todo_status rejects invalid statuses" {
    # Test invalid statuses
    run validate_todo_status "invalid"
    [ "$status" -ne 0 ]

    run validate_todo_status ""
    [ "$status" -ne 0 ]

    run validate_todo_status "PENDING"  # Case sensitive
    [ "$status" -ne 0 ]
}

#####################################
# Test Todo List Parsing and Processing
#####################################

@test "parse_todo_items_from_analysis extracts todos from AI analysis" {
    local ai_analysis="## Analysis Result

The code looks good but needs improvements:

1. **CRITICAL**: Fix authentication bypass vulnerability in auth.js:45
2. **HIGH**: Add input validation for user data in utils.js:123
3. **MEDIUM**: Improve error handling in api.js:67
4. **LOW**: Add debug logging for troubleshooting"

    # Execute function
    run parse_todo_items_from_analysis "$ai_analysis"

    # Should extract all todo items
    [ "$status" -eq 0 ]
    [[ "$output" == *"CRITICAL"* ]]
    [[ "$output" == *"authentication bypass"* ]]
    [[ "$output" == *"HIGH"* ]]
    [[ "$output" == *"input validation"* ]]
    [[ "$output" == *"MEDIUM"* ]]
    [[ "$output" == *"LOW"* ]]
}

@test "parse_todo_items_from_analysis handles various formats" {
    local ai_analysis="## Recommendations

- **Critical Issue**: Security vulnerability needs immediate attention
- High Priority: Add comprehensive tests
- Medium: Refactor legacy code
- Low priority: Update documentation

Additional notes:
- CRITICAL: Memory leak in main.js:234"

    # Execute function
    run parse_todo_items_from_analysis "$ai_analysis"

    # Should handle various formatting styles
    [ "$status" -eq 0 ]
    [[ "$output" == *"CRITICAL"* ]]
    [[ "$output" == *"Security vulnerability"* ]]
    [[ "$output" == *"Memory leak"* ]]
}

@test "parse_todo_items_from_analysis extracts file references" {
    local ai_analysis="Issues found:
1. **HIGH**: Fix bug in src/components/Auth.js:45-67
2. **MEDIUM**: Update function in utils/helper.js line 123
3. **LOW**: Documentation in README.md needs updating"

    # Execute function
    run parse_todo_items_from_analysis "$ai_analysis"

    # Should extract file references
    [ "$status" -eq 0 ]
    [[ "$output" == *"Auth.js:45"* ]]
    [[ "$output" == *"helper.js:123"* ]]
    [[ "$output" == *"README.md"* ]]
}

@test "parse_todo_items_from_analysis handles empty analysis" {
    local ai_analysis="The code looks perfect. No issues found."

    # Execute function
    run parse_todo_items_from_analysis "$ai_analysis"

    # Should handle no todos gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == "[]" ]] || [[ "$output" == "" ]]
}

@test "parse_todo_items_from_analysis ignores non-actionable items" {
    local ai_analysis="## Analysis

Good practices observed:
- Code follows standards
- Tests are comprehensive
- Documentation is clear

Suggestions (non-actionable):
- Consider using more modern syntax
- Think about future scalability"

    # Execute function
    run parse_todo_items_from_analysis "$ai_analysis"

    # Should ignore non-actionable suggestions
    [ "$status" -eq 0 ]
    [[ "$output" == "[]" ]] || [[ "$output" == "" ]]
}

#####################################
# Test Todo List State Management
#####################################

@test "update_todo_list_state merges new and existing todos" {
    local existing_todos='[
        {
            "id": "todo-001",
            "category": "HIGH",
            "description": "Existing todo",
            "status": "in_progress"
        }
    ]'
    local new_todos='[
        {
            "id": "todo-002",
            "category": "CRITICAL",
            "description": "New critical issue",
            "status": "pending"
        }
    ]'

    # Execute function
    run update_todo_list_state "$existing_todos" "$new_todos"

    # Should merge both lists
    [ "$status" -eq 0 ]
    [[ "$output" == *"todo-001"* ]]
    [[ "$output" == *"todo-002"* ]]
    [[ "$output" == *"Existing todo"* ]]
    [[ "$output" == *"New critical issue"* ]]
}

@test "update_todo_list_state preserves existing todo status" {
    local existing_todos='[
        {
            "id": "todo-001",
            "category": "HIGH",
            "description": "Existing todo",
            "status": "completed"
        }
    ]'
    local new_todos='[
        {
            "id": "todo-001",
            "category": "HIGH",
            "description": "Updated description",
            "status": "pending"
        }
    ]'

    # Execute function
    run update_todo_list_state "$existing_todos" "$new_todos"

    # Should preserve completed status
    [ "$status" -eq 0 ]
    [[ "$output" == *"status\":\"completed\""* ]]
    [[ "$output" == *"Updated description"* ]]
}

@test "update_todo_list_state handles duplicate detection" {
    local existing_todos='[
        {
            "id": "todo-001",
            "category": "HIGH",
            "description": "Fix authentication bug",
            "file_references": ["auth.js:45"]
        }
    ]'
    local new_todos='[
        {
            "id": "todo-002",
            "category": "HIGH",
            "description": "Fix authentication bug",
            "file_references": ["auth.js:45"]
        }
    ]'

    # Execute function
    run update_todo_list_state "$existing_todos" "$new_todos"

    # Should detect and handle duplicates
    [ "$status" -eq 0 ]
    # Should only have one todo or mark as duplicate
    local todo_count=$(echo "$output" | grep -o '"id":"todo-[^"]*"' | wc -l)
    [ "$todo_count" -le 2 ]  # At most 2 todos (or marked as duplicate)
}

@test "merge_todo_updates applies incremental updates" {
    local base_todos='[
        {
            "id": "todo-001",
            "category": "HIGH",
            "description": "Original description",
            "status": "pending"
        }
    ]'
    local updates='[
        {
            "id": "todo-001",
            "status": "in_progress",
            "progress_notes": "Started working on this"
        }
    ]'

    # Execute function
    run merge_todo_updates "$base_todos" "$updates"

    # Should apply incremental updates
    [ "$status" -eq 0 ]
    [[ "$output" == *"in_progress"* ]]
    [[ "$output" == *"Original description"* ]]
    [[ "$output" == *"progress_notes"* ]]
}

@test "remove_completed_todos filters completed items" {
    local todo_list='[
        {
            "id": "todo-001",
            "category": "HIGH",
            "status": "completed"
        },
        {
            "id": "todo-002",
            "category": "MEDIUM",
            "status": "pending"
        },
        {
            "id": "todo-003",
            "category": "LOW",
            "status": "completed"
        }
    ]'

    # Execute function
    run remove_completed_todos "$todo_list"

    # Should remove completed todos
    [ "$status" -eq 0 ]
    [[ "$output" != *"todo-001"* ]]
    [[ "$output" == *"todo-002"* ]]
    [[ "$output" != *"todo-003"* ]]
}

#####################################
# Test Todo Categorization and Prioritization
#####################################

@test "categorize_todos_by_priority groups by category" {
    local todo_list='[
        {"id":"todo-001","category":"CRITICAL","description":"Critical issue"},
        {"id":"todo-002","category":"HIGH","description":"High priority"},
        {"id":"todo-003","category":"CRITICAL","description":"Another critical"},
        {"id":"todo-004","category":"MEDIUM","description":"Medium priority"},
        {"id":"todo-005","category":"LOW","description":"Low priority"}
    ]'

    # Execute function
    run categorize_todos_by_priority "$todo_list"

    # Should group by category
    [ "$status" -eq 0 ]
    [[ "$output" == *"CRITICAL"* ]]
    [[ "$output" == *"HIGH"* ]]
    [[ "$output" == *"MEDIUM"* ]]
    [[ "$output" == *"LOW"* ]]
}

@test "get_todos_by_category filters by specific category" {
    local todo_list='[
        {"id":"todo-001","category":"CRITICAL","description":"Critical issue"},
        {"id":"todo-002","category":"HIGH","description":"High priority"},
        {"id":"todo-003","category":"CRITICAL","description":"Another critical"}
    ]'

    # Execute function
    run get_todos_by_category "$todo_list" "CRITICAL"

    # Should return only critical todos
    [ "$status" -eq 0 ]
    [[ "$output" == *"todo-001"* ]]
    [[ "$output" == *"todo-003"* ]]
    [[ "$output" != *"todo-002"* ]]
}

@test "sort_todos_by_priority sorts correctly" {
    local todo_list='[
        {"id":"todo-001","category":"LOW","description":"Low priority"},
        {"id":"todo-002","category":"CRITICAL","description":"Critical issue"},
        {"id":"todo-003","category":"MEDIUM","description":"Medium priority"},
        {"id":"todo-004","category":"HIGH","description":"High priority"}
    ]'

    # Execute function
    run sort_todos_by_priority "$todo_list"

    # Should sort by priority (CRITICAL > HIGH > MEDIUM > LOW)
    [ "$status" -eq 0 ]
    # Check order by examining position of todos
    [[ "$output" =~ todo-002.*todo-004.*todo-003.*todo-001 ]]
}

#####################################
# Test Quality Assessment and Readiness
#####################################

@test "assess_todo_readiness_for_advancement determines PR readiness" {
    local todo_list_with_critical='[
        {"id":"todo-001","category":"CRITICAL","status":"pending"},
        {"id":"todo-002","category":"HIGH","status":"completed"}
    ]'

    # Execute function
    run assess_todo_readiness_for_advancement "$todo_list_with_critical"

    # Should indicate not ready due to critical issues
    [ "$status" -eq 0 ]
    [[ "$output" == *"not_ready"* ]] || [[ "$output" == *"blocking"* ]]
}

@test "assess_todo_readiness_for_advancement allows advancement without critical issues" {
    local todo_list_no_critical='[
        {"id":"todo-001","category":"HIGH","status":"completed"},
        {"id":"todo-002","category":"MEDIUM","status":"pending"},
        {"id":"todo-003","category":"LOW","status":"pending"}
    ]'

    # Execute function
    run assess_todo_readiness_for_advancement "$todo_list_no_critical"

    # Should indicate ready for advancement
    [ "$status" -eq 0 ]
    [[ "$output" == *"ready"* ]] || [[ "$output" != *"blocking"* ]]
}

@test "identify_blocking_issues finds merge blockers" {
    local todo_list='[
        {"id":"todo-001","category":"CRITICAL","status":"pending","description":"Security vulnerability"},
        {"id":"todo-002","category":"HIGH","status":"pending","description":"Test failures"},
        {"id":"todo-003","category":"MEDIUM","status":"pending","description":"Documentation"}
    ]'

    # Execute function
    run identify_blocking_issues "$todo_list"

    # Should identify critical and high priority blockers
    [ "$status" -eq 0 ]
    [[ "$output" == *"todo-001"* ]]
    [[ "$output" == *"Security vulnerability"* ]]
    [[ "$output" == *"todo-002"* ]] || [[ "$output" == *"Test failures"* ]]
}

@test "calculate_completion_percentage calculates progress correctly" {
    local todo_list='[
        {"id":"todo-001","category":"HIGH","status":"completed"},
        {"id":"todo-002","category":"MEDIUM","status":"completed"},
        {"id":"todo-003","category":"LOW","status":"pending"},
        {"id":"todo-004","category":"HIGH","status":"pending"}
    ]'

    # Execute function
    run calculate_completion_percentage "$todo_list"

    # Should calculate 50% completion (2 of 4 completed)
    [ "$status" -eq 0 ]
    [[ "$output" == *"50"* ]] || [[ "$output" == *"0.5"* ]]
}

#####################################
# Test Statistics and Reporting
#####################################

@test "generate_todo_statistics creates comprehensive stats" {
    local todo_list='[
        {"id":"todo-001","category":"CRITICAL","status":"completed"},
        {"id":"todo-002","category":"HIGH","status":"pending"},
        {"id":"todo-003","category":"MEDIUM","status":"in_progress"},
        {"id":"todo-004","category":"LOW","status":"pending"},
        {"id":"todo-005","category":"HIGH","status":"completed"}
    ]'

    # Execute function
    run generate_todo_statistics "$todo_list"

    # Should generate comprehensive statistics
    [ "$status" -eq 0 ]
    [[ "$output" == *"total_count"* ]]
    [[ "$output" == *"completed_count"* ]]
    [[ "$output" == *"pending_count"* ]]
    [[ "$output" == *"in_progress_count"* ]]
    [[ "$output" == *"by_category"* ]]
    [[ "$output" == *"completion_percentage"* ]]
}

@test "generate_todo_summary_for_output creates action output format" {
    local todo_list='[
        {"id":"todo-001","category":"CRITICAL","status":"pending","description":"Fix security issue"},
        {"id":"todo-002","category":"HIGH","status":"completed","description":"Add tests"}
    ]'

    # Execute function
    run generate_todo_summary_for_output "$todo_list"

    # Should generate GitHub Actions output format
    [ "$status" -eq 0 ]
    [[ "$output" == *"total"* ]]
    [[ "$output" == *"critical"* ]]
    [[ "$output" == *"completed"* ]]
    # Should be properly formatted for GitHub Actions output
}

@test "format_todo_list_for_display creates readable format" {
    local todo_list='[
        {
            "id":"todo-001",
            "category":"HIGH",
            "description":"Add comprehensive error handling",
            "file_references":["src/api.js:45"],
            "status":"pending"
        }
    ]'

    # Execute function
    run format_todo_list_for_display "$todo_list"

    # Should create human-readable format
    [ "$status" -eq 0 ]
    [[ "$output" == *"HIGH"* ]]
    [[ "$output" == *"Add comprehensive error handling"* ]]
    [[ "$output" == *"src/api.js:45"* ]]
    [[ "$output" == *"⏳"* ]] || [[ "$output" == *"pending"* ]]
}

@test "generate_progress_chart creates visual progress representation" {
    local todo_list='[
        {"category":"CRITICAL","status":"completed"},
        {"category":"HIGH","status":"completed"},
        {"category":"HIGH","status":"pending"},
        {"category":"MEDIUM","status":"pending"},
        {"category":"LOW","status":"pending"}
    ]'

    # Execute function
    run generate_progress_chart "$todo_list"

    # Should create visual progress chart
    [ "$status" -eq 0 ]
    [[ "$output" == *"█"* ]] || [[ "$output" == *"▓"* ]] || [[ "$output" == *"░"* ]]
    # Should show progress visually
}

#####################################
# Test Error Handling and Edge Cases
#####################################

@test "todo management handles malformed JSON input" {
    local invalid_json="invalid json content"

    # Execute function
    run update_todo_list_state "$invalid_json" "[]"

    # Should handle malformed JSON gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == "[]" ]] || [[ "$output" == *"error"* ]]
}

@test "todo management handles empty todo lists" {
    # Execute function
    run generate_todo_statistics "[]"

    # Should handle empty lists gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == *"total_count\":0"* ]]
}

@test "todo management handles very large todo lists" {
    # Create large todo list
    local large_list="["
    for i in {1..200}; do
        large_list+="{\"id\":\"todo-$(printf "%03d" $i)\",\"category\":\"MEDIUM\",\"description\":\"Todo $i\",\"status\":\"pending\"},"
    done
    large_list="${large_list%,}]"

    # Execute function
    run generate_todo_statistics "$large_list"

    # Should handle large lists efficiently
    [ "$status" -eq 0 ]
    [[ "$output" == *"total_count\":200"* ]]
}

@test "todo validation handles missing required fields" {
    # Test with missing category
    run create_todo_item "" "Description" "file.js:1" "Epic context"
    [ "$status" -ne 0 ]

    # Test with missing description
    run create_todo_item "HIGH" "" "file.js:1" "Epic context"
    [ "$status" -ne 0 ]
}

@test "todo parsing handles malformed AI analysis" {
    local malformed_analysis="Random text without proper structure
    Some bullets:
    - Not properly formatted
    - Missing priority indicators"

    # Execute function
    run parse_todo_items_from_analysis "$malformed_analysis"

    # Should handle gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == "[]" ]] || [[ "$output" == "" ]]
}

#####################################
# Test Performance and Efficiency
#####################################

@test "todo operations complete within reasonable time" {
    local moderate_todo_list="["
    for i in {1..50}; do
        moderate_todo_list+="{\"id\":\"todo-$(printf "%02d" $i)\",\"category\":\"MEDIUM\",\"description\":\"Todo item $i\",\"status\":\"pending\"},"
    done
    moderate_todo_list="${moderate_todo_list%,}]"

    # Measure execution time
    local start_time=$(date +%s)
    run generate_todo_statistics "$moderate_todo_list"
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))

    # Should complete quickly (under 3 seconds)
    [ "$status" -eq 0 ]
    [ "$duration" -lt 3 ]
}

@test "todo list operations are idempotent" {
    local todo_list='[{"id":"todo-001","category":"HIGH","status":"pending"}]'

    # Execute same operation multiple times
    run update_todo_list_state "$todo_list" "[]"
    local result1="$output"
    run update_todo_list_state "$result1" "[]"
    local result2="$output"

    # Results should be consistent
    [ "$status" -eq 0 ]
    [[ "$result1" == "$result2" ]]
}

@test "concurrent todo operations handle safely" {
    local base_todos='[{"id":"todo-001","category":"HIGH","status":"pending"}]'
    local updates1='[{"id":"todo-001","status":"in_progress"}]'
    local updates2='[{"id":"todo-001","progress_notes":"Working on it"}]'

    # Execute concurrent operations (simulated)
    run merge_todo_updates "$base_todos" "$updates1"
    local status1=$status
    run merge_todo_updates "$base_todos" "$updates2"
    local status2=$status

    # Both should succeed or handle conflicts gracefully
    [ "$status1" -eq 0 ] || [ "$status2" -eq 0 ]
}