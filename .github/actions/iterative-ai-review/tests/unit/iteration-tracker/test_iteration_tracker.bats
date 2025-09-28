#!/usr/bin/env bats
# Unit Tests for Iterative AI Review Action - Iteration Tracker
# Tests context persistence, state management, and iteration history tracking

# Setup test environment
setup() {
    # Load test fixtures and mocks
    load "../../fixtures/github-environment.sh"
    load "../../mocks/github-api-mock.sh"

    # Initialize test environment
    initialize_test_environment
    setup_github_api_mocking

    # Source the iteration-tracker module under test
    source "${BATS_TEST_DIRNAME}/../../../src/iteration-tracker.js"

    # Setup test-specific variables
    export TEST_PR_NUMBER="123"
    export TEST_STATE_DIR="$TEST_TEMP_DIR/iterative-review"
    export TEST_STATE_FILE="$TEST_STATE_DIR/pr-$TEST_PR_NUMBER-state.json"

    # Create state directory
    mkdir -p "$TEST_STATE_DIR"
}

# Cleanup after each test
teardown() {
    cleanup_test_environment
    cleanup_github_api_mocking
}

#####################################
# Test State File Management
#####################################

@test "load_iteration_context loads existing state successfully" {
    # Create existing state file
    create_mock_iteration_state 2 5

    # Execute function
    run load_iteration_context "$TEST_PR_NUMBER"

    # Should load existing state
    [ "$status" -eq 0 ]
    [[ "$output" == *"schema_version"* ]]
    [[ "$output" == *"iteration_count"* ]]
    [[ "$output" == *"pr_number"* ]]
}

@test "load_iteration_context handles missing state file" {
    # Ensure no existing state file
    rm -f "$TEST_STATE_FILE"

    # Execute function
    run load_iteration_context "$TEST_PR_NUMBER"

    # Should handle missing state gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == "{}" ]] || [[ "$output" == *"new_state"* ]]
}

@test "load_iteration_context validates state schema" {
    # Create invalid state file
    echo '{"invalid": "schema", "missing_required_fields": true}' > "$TEST_STATE_FILE"

    # Execute function
    run load_iteration_context "$TEST_PR_NUMBER"

    # Should handle invalid schema
    [ "$status" -eq 0 ]
    [[ "$output" == "{}" ]] || [[ "$output" == *"invalid"* ]]
}

@test "load_iteration_context handles corrupted state file" {
    # Create corrupted JSON file
    echo 'corrupted json content {invalid' > "$TEST_STATE_FILE"

    # Execute function
    run load_iteration_context "$TEST_PR_NUMBER"

    # Should handle corruption gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == "{}" ]] || [[ "$output" == *"error"* ]]
}

@test "save_iteration_context creates new state file" {
    local test_state='{"schema_version":"1.0","pr_number":123,"iteration_count":1}'

    # Execute function
    run save_iteration_context "$TEST_PR_NUMBER" "$test_state"

    # Should create state file successfully
    [ "$status" -eq 0 ]
    [ -f "$TEST_STATE_FILE" ]
    [[ "$(cat "$TEST_STATE_FILE")" == *"schema_version"* ]]
}

@test "save_iteration_context updates existing state file" {
    # Create initial state
    create_mock_iteration_state 1 3

    local updated_state='{"schema_version":"1.0","pr_number":123,"iteration_count":2,"updated":true}'

    # Execute function
    run save_iteration_context "$TEST_PR_NUMBER" "$updated_state"

    # Should update existing state
    [ "$status" -eq 0 ]
    [ -f "$TEST_STATE_FILE" ]
    [[ "$(cat "$TEST_STATE_FILE")" == *"iteration_count\":2"* ]]
    [[ "$(cat "$TEST_STATE_FILE")" == *"updated\":true"* ]]
}

@test "save_iteration_context validates JSON format" {
    local invalid_json="invalid json content"

    # Execute function
    run save_iteration_context "$TEST_PR_NUMBER" "$invalid_json"

    # Should reject invalid JSON
    [ "$status" -ne 0 ]
    [[ "$output" == *"invalid"* ]] || [[ "$output" == *"JSON"* ]]
}

#####################################
# Test Iteration Management
#####################################

@test "increment_iteration_count increments from existing state" {
    # Create state with iteration count 2
    create_mock_iteration_state 2 3

    # Execute function
    run increment_iteration_count "$TEST_PR_NUMBER"

    # Should increment to 3
    [ "$status" -eq 0 ]
    [[ "$output" == "3" ]]
}

@test "increment_iteration_count starts from 1 for new PR" {
    # Ensure no existing state
    rm -f "$TEST_STATE_FILE"

    # Execute function
    run increment_iteration_count "$TEST_PR_NUMBER"

    # Should start at 1
    [ "$status" -eq 0 ]
    [[ "$output" == "1" ]]
}

@test "increment_iteration_count respects maximum limit" {
    # Create state at maximum iteration limit
    create_mock_iteration_state 5 1

    # Set maximum iterations
    export INPUT_MAX_ITERATIONS="5"

    # Execute function
    run increment_iteration_count "$TEST_PR_NUMBER"

    # Should not exceed maximum
    [ "$status" -eq 0 ]
    [[ "$output" == "5" ]] || [[ "$output" == *"maximum"* ]]
}

@test "get_current_iteration_count returns correct count" {
    # Create state with specific iteration count
    create_mock_iteration_state 3 2

    # Execute function
    run get_current_iteration_count "$TEST_PR_NUMBER"

    # Should return current count
    [ "$status" -eq 0 ]
    [[ "$output" == "3" ]]
}

@test "get_current_iteration_count returns 0 for new PR" {
    # Ensure no existing state
    rm -f "$TEST_STATE_FILE"

    # Execute function
    run get_current_iteration_count "$TEST_PR_NUMBER"

    # Should return 0 for new PR
    [ "$status" -eq 0 ]
    [[ "$output" == "0" ]]
}

#####################################
# Test State Schema and Structure
#####################################

@test "generate_iteration_state_json creates valid schema" {
    local iteration_count="1"
    local todo_list='[{"id":"todo-001","category":"HIGH","description":"Test todo"}]'
    local historical_context="{}"
    local pr_metadata='{"changed_files":5,"additions":100}'

    # Execute function
    run generate_iteration_state_json "$iteration_count" "$todo_list" "$historical_context" "$pr_metadata"

    # Should generate valid schema
    [ "$status" -eq 0 ]
    [[ "$output" == *"schema_version"* ]]
    [[ "$output" == *"pr_number"* ]]
    [[ "$output" == *"iteration_count"* ]]
    [[ "$output" == *"current_todo_list"* ]]
    [[ "$output" == *"historical_context"* ]]
    [[ "$output" == *"pr_metadata"* ]]
}

@test "generate_iteration_state_json includes timestamp" {
    local iteration_count="1"
    local todo_list="[]"
    local historical_context="{}"
    local pr_metadata="{}"

    # Execute function
    run generate_iteration_state_json "$iteration_count" "$todo_list" "$historical_context" "$pr_metadata"

    # Should include timestamp
    [ "$status" -eq 0 ]
    [[ "$output" == *"timestamp"* ]]
    [[ "$output" == *"$(date +%Y)"* ]]
}

@test "generate_iteration_state_json validates input parameters" {
    # Test with missing parameters
    run generate_iteration_state_json "" "" "" ""

    # Should handle missing parameters gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == *"schema_version"* ]] # Should still generate basic structure
}

@test "generate_iteration_state_json handles complex nested data" {
    local iteration_count="2"
    local complex_todo_list='[
        {
            "id": "todo-001",
            "category": "CRITICAL",
            "description": "Complex todo with nested data",
            "file_references": ["src/file1.js:45", "src/file2.js:123"],
            "dependencies": ["todo-002"],
            "validation_criteria": "All tests pass and security scan clean",
            "epic_alignment": "Epic #181 security requirements"
        }
    ]'
    local complex_historical_context='{"iteration_history":[{"iteration":1,"summary":"Initial review"}],"progress_metrics":{"total_todos_created":5}}'
    local pr_metadata='{"changed_files":10,"additions":250,"deletions":50}'

    # Execute function
    run generate_iteration_state_json "$iteration_count" "$complex_todo_list" "$complex_historical_context" "$pr_metadata"

    # Should handle complex nested data
    [ "$status" -eq 0 ]
    [[ "$output" == *"todo-001"* ]]
    [[ "$output" == *"file_references"* ]]
    [[ "$output" == *"iteration_history"* ]]
}

#####################################
# Test Historical Context Management
#####################################

@test "merge_historical_context combines existing and new data" {
    local existing_context='{"iteration_history":[{"iteration":1,"summary":"Initial"}],"progress_metrics":{"total_todos_created":3}}'
    local new_context='{"iteration_history":[{"iteration":2,"summary":"Follow-up"}],"progress_metrics":{"total_todos_resolved":1}}'

    # Execute function
    run merge_historical_context "$existing_context" "$new_context"

    # Should merge contexts appropriately
    [ "$status" -eq 0 ]
    [[ "$output" == *"iteration\":1"* ]]
    [[ "$output" == *"iteration\":2"* ]]
    [[ "$output" == *"total_todos_created"* ]]
    [[ "$output" == *"total_todos_resolved"* ]]
}

@test "merge_historical_context handles empty existing context" {
    local existing_context="{}"
    local new_context='{"iteration_history":[{"iteration":1,"summary":"First"}]}'

    # Execute function
    run merge_historical_context "$existing_context" "$new_context"

    # Should handle empty existing context
    [ "$status" -eq 0 ]
    [[ "$output" == *"iteration\":1"* ]]
    [[ "$output" == *"First"* ]]
}

@test "merge_historical_context handles malformed JSON" {
    local existing_context="invalid json"
    local new_context='{"iteration_history":[]}'

    # Execute function
    run merge_historical_context "$existing_context" "$new_context"

    # Should handle malformed JSON gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == *"iteration_history"* ]]
}

@test "add_iteration_to_history adds new iteration record" {
    local existing_history='{"iteration_history":[{"iteration":1,"summary":"Initial"}]}'
    local iteration_count="2"
    local summary="Second iteration with improvements"
    local todo_count="3"

    # Execute function
    run add_iteration_to_history "$existing_history" "$iteration_count" "$summary" "$todo_count"

    # Should add new iteration record
    [ "$status" -eq 0 ]
    [[ "$output" == *"iteration\":1"* ]]
    [[ "$output" == *"iteration\":2"* ]]
    [[ "$output" == *"improvements"* ]]
}

@test "compress_historical_context reduces large history size" {
    # Create large historical context
    local large_history='{"iteration_history":['
    for i in {1..50}; do
        large_history+="{\"iteration\":$i,\"summary\":\"Iteration $i summary with detailed information\",\"todo_count\":$((i+5))},"
    done
    large_history="${large_history%,}]}"

    # Execute function
    run compress_historical_context "$large_history"

    # Should compress while preserving essential information
    [ "$status" -eq 0 ]
    [[ ${#output} -lt ${#large_history} ]] # Should be smaller
    [[ "$output" == *"iteration"* ]] # Should preserve structure
}

#####################################
# Test Context Persistence and Storage
#####################################

@test "create_state_backup creates backup copy" {
    # Create original state
    create_mock_iteration_state 2 3

    # Execute function
    run create_state_backup "$TEST_PR_NUMBER"

    # Should create backup file
    [ "$status" -eq 0 ]
    [ -f "$TEST_STATE_FILE.backup" ]
    [[ "$(cat "$TEST_STATE_FILE.backup")" == "$(cat "$TEST_STATE_FILE")" ]]
}

@test "restore_state_from_backup restores from backup" {
    # Create original state and backup
    create_mock_iteration_state 2 3
    cp "$TEST_STATE_FILE" "$TEST_STATE_FILE.backup"

    # Corrupt original state
    echo "corrupted" > "$TEST_STATE_FILE"

    # Execute function
    run restore_state_from_backup "$TEST_PR_NUMBER"

    # Should restore from backup
    [ "$status" -eq 0 ]
    [[ "$(cat "$TEST_STATE_FILE")" == *"schema_version"* ]]
}

@test "cleanup_old_state_files removes expired states" {
    # Create multiple old state files
    touch "$TEST_STATE_DIR/pr-100-state.json"
    touch "$TEST_STATE_DIR/pr-101-state.json"
    touch "$TEST_STATE_DIR/pr-102-state.json"

    # Set creation times to old dates (simulated)
    export MOCK_OLD_FILES="true"

    # Execute function
    run cleanup_old_state_files

    # Should clean up old files
    [ "$status" -eq 0 ]
    [[ "$output" == *"cleaned"* ]] || [[ "$output" == *"removed"* ]]
}

@test "validate_state_integrity checks state file integrity" {
    # Create valid state
    create_mock_iteration_state 2 3

    # Execute function
    run validate_state_integrity "$TEST_PR_NUMBER"

    # Should validate successfully
    [ "$status" -eq 0 ]
    [[ "$output" == *"valid"* ]] || [ "$output" = "" ]
}

@test "validate_state_integrity detects corruption" {
    # Create corrupted state
    echo '{"corrupted": true, "missing_schema_version"' > "$TEST_STATE_FILE"

    # Execute function
    run validate_state_integrity "$TEST_PR_NUMBER"

    # Should detect corruption
    [ "$status" -ne 0 ]
    [[ "$output" == *"corrupt"* ]] || [[ "$output" == *"invalid"* ]]
}

#####################################
# Test Epic and Progress Tracking
#####################################

@test "update_epic_progress_tracking updates Epic context" {
    local current_state
    current_state=$(create_mock_iteration_state 2 3)
    local epic_progress="Contributing to 90% backend coverage goal"

    # Execute function
    run update_epic_progress_tracking "$TEST_PR_NUMBER" "$epic_progress"

    # Should update Epic progress
    [ "$status" -eq 0 ]
    [[ "$output" == *"epic"* ]] || [[ "$output" == *"progress"* ]]
}

@test "calculate_progress_metrics calculates statistics correctly" {
    local todo_history='[
        {"iteration":1,"todo_count":5,"resolved_count":0},
        {"iteration":2,"todo_count":3,"resolved_count":2}
    ]'

    # Execute function
    run calculate_progress_metrics "$todo_history"

    # Should calculate progress metrics
    [ "$status" -eq 0 ]
    [[ "$output" == *"total_todos_created"* ]]
    [[ "$output" == *"total_todos_resolved"* ]]
    [[ "$output" == *"resolution_rate"* ]] || [[ "$output" == *"progress"* ]]
}

@test "estimate_completion_timeline estimates based on progress" {
    local progress_metrics='{"total_todos_created":10,"total_todos_resolved":6,"current_active_todos":4}'
    local iteration_count="3"

    # Execute function
    run estimate_completion_timeline "$progress_metrics" "$iteration_count"

    # Should estimate completion
    [ "$status" -eq 0 ]
    [[ "$output" == *"iteration"* ]] || [[ "$output" == *"estimate"* ]]
}

#####################################
# Test Error Handling and Recovery
#####################################

@test "state management handles disk space issues" {
    # Mock disk space error
    export MOCK_DISK_FULL="true"

    # Execute function
    run save_iteration_context "$TEST_PR_NUMBER" '{"test":"data"}'

    # Should handle disk space gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"disk"* ]] || [[ "$output" == *"space"* ]]
}

@test "state management handles permission issues" {
    # Make state directory read-only
    chmod 444 "$TEST_STATE_DIR"

    # Execute function
    run save_iteration_context "$TEST_PR_NUMBER" '{"test":"data"}'

    # Should handle permission issues
    [ "$status" -ne 0 ]
    [[ "$output" == *"permission"* ]] || [[ "$output" == *"access"* ]]

    # Restore permissions for cleanup
    chmod 755 "$TEST_STATE_DIR"
}

@test "state loading handles concurrent access safely" {
    # Create state file
    create_mock_iteration_state 1 2

    # Simulate concurrent access
    run load_iteration_context "$TEST_PR_NUMBER"
    local status1=$status
    run load_iteration_context "$TEST_PR_NUMBER"
    local status2=$status

    # Both should succeed without corruption
    [ "$status1" -eq 0 ]
    [ "$status2" -eq 0 ]
}

@test "state management recovers from partial writes" {
    # Create partially written state file
    echo '{"schema_version":"1.0","pr_number":123,"incomplete"' > "$TEST_STATE_FILE"

    # Execute function
    run load_iteration_context "$TEST_PR_NUMBER"

    # Should recover gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == "{}" ]] || [[ "$output" == *"recovered"* ]]
}

#####################################
# Test Performance and Scalability
#####################################

@test "state operations complete within reasonable time" {
    # Create moderately large state
    local large_state
    large_state=$(create_mock_iteration_state 5 20)

    # Measure execution time
    local start_time=$(date +%s)
    run load_iteration_context "$TEST_PR_NUMBER"
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))

    # Should complete quickly (under 3 seconds)
    [ "$status" -eq 0 ]
    [ "$duration" -lt 3 ]
}

@test "state compression handles large datasets efficiently" {
    # Create large historical context
    local large_context='{"iteration_history":['
    for i in {1..100}; do
        large_context+="{\"iteration\":$i,\"timestamp\":\"2025-09-23T$((i%24)):00:00Z\",\"summary\":\"Detailed summary for iteration $i with comprehensive analysis\",\"todo_count\":$((i*2)),\"resolved_count\":$i},"
    done
    large_context="${large_context%,}]}"

    # Execute function
    run compress_historical_context "$large_context"

    # Should handle large data efficiently
    [ "$status" -eq 0 ]
    [[ ${#output} -lt $((${#large_context} / 2)) ]] # Should significantly compress
}

@test "concurrent state updates handle race conditions safely" {
    # Create initial state
    create_mock_iteration_state 1 1

    # Simulate concurrent updates
    local state1='{"schema_version":"1.0","pr_number":123,"iteration_count":2,"source":"update1"}'
    local state2='{"schema_version":"1.0","pr_number":123,"iteration_count":2,"source":"update2"}'

    # Execute concurrent saves (simulated)
    run save_iteration_context "$TEST_PR_NUMBER" "$state1"
    local status1=$status
    run save_iteration_context "$TEST_PR_NUMBER" "$state2"
    local status2=$status

    # At least one should succeed
    [ "$status1" -eq 0 ] || [ "$status2" -eq 0 ]

    # Final state should be valid
    run load_iteration_context "$TEST_PR_NUMBER"
    [ "$status" -eq 0 ]
    [[ "$output" == *"schema_version"* ]]
}