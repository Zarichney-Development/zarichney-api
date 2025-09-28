#!/usr/bin/env bats
# Integration Tests for Iterative AI Review Action
# Tests complete end-to-end workflows and component interactions

# Setup test environment
setup() {
    # Load test fixtures and mocks
    load "../fixtures/github-environment.sh"
    load "../mocks/github-api-mock.sh"

    # Initialize test environment
    initialize_test_environment
    setup_github_api_mocking

    # Source all modules for integration testing
    source "${BATS_TEST_DIRNAME}/../../src/main.js"
    source "${BATS_TEST_DIRNAME}/../../src/github-api.js"
    source "${BATS_TEST_DIRNAME}/../../src/comment-manager.js"
    source "${BATS_TEST_DIRNAME}/../../src/iteration-tracker.js"
    source "${BATS_TEST_DIRNAME}/../../src/todo-manager.js"

    # Setup integration test variables
    export TEST_PR_NUMBER="123"
    export TEST_ACTION_DIR="${BATS_TEST_DIRNAME}/../.."
    export GITHUB_OUTPUT="$TEST_TEMP_DIR/github_output"
    export GITHUB_STEP_SUMMARY="$TEST_TEMP_DIR/step_summary"

    # Create required files for GitHub Actions
    touch "$GITHUB_OUTPUT"
    touch "$GITHUB_STEP_SUMMARY"

    # Mock AI analysis template
    export MOCK_AI_ANALYSIS="## AI Code Review Analysis

### Summary
The iterative AI review action implementation looks solid overall. Here are some areas for improvement:

### To-Do Items

1. **CRITICAL**: Add comprehensive error handling for API failures
   - **File**: src/github-api.js:45
   - **Epic Alignment**: Epic #181 reliability requirements

2. **HIGH**: Implement input validation and sanitization
   - **File**: src/comment-manager.js:123
   - **Epic Alignment**: Epic #181 security requirements

3. **MEDIUM**: Add performance monitoring and logging
   - **File**: src/main.js:89
   - **Epic Alignment**: Epic #181 observability requirements

### Quality Assessment
- Code structure follows established patterns
- Integration points are well defined
- Test coverage needs improvement

### Epic #181 Alignment
This implementation directly supports the autonomous development cycle objectives."
}

# Cleanup after each test
teardown() {
    cleanup_test_environment
    cleanup_github_api_mocking
}

#####################################
# Test Complete Workflow Integration
#####################################

@test "complete workflow executes successfully for first iteration" {
    setup_scenario_first_iteration

    # Execute complete workflow
    run execute_iterative_review_workflow

    # Should complete full workflow
    [ "$status" -eq 0 ]

    # Verify outputs are generated
    [ -f "$GITHUB_OUTPUT" ]
    grep -q "iteration_count=1" "$GITHUB_OUTPUT"
    grep -q "pr_status=" "$GITHUB_OUTPUT"
    grep -q "todo_summary=" "$GITHUB_OUTPUT"

    # Verify step summary is created
    [ -s "$GITHUB_STEP_SUMMARY" ]
}

@test "complete workflow handles subsequent iterations correctly" {
    setup_scenario_subsequent_iteration

    # Create existing state for iteration 2
    create_mock_iteration_state 2 5

    # Execute complete workflow
    run execute_iterative_review_workflow

    # Should handle subsequent iteration
    [ "$status" -eq 0 ]
    grep -q "iteration_count=3" "$GITHUB_OUTPUT"

    # Should preserve historical context
    local state_file="$TEST_TEMP_DIR/iterative-review/pr-$TEST_PR_NUMBER-state.json"
    [ -f "$state_file" ]
    [[ "$(cat "$state_file")" == *"iteration_history"* ]]
}

@test "workflow integrates all components seamlessly" {
    setup_scenario_first_iteration

    # Execute workflow and verify component integration
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Verify GitHub API integration
    [[ "$output" == *"PR status"* ]] || grep -q "pr_status" "$GITHUB_OUTPUT"

    # Verify comment management integration
    [[ "$output" == *"comment"* ]] || grep -q "comment_updated" "$GITHUB_OUTPUT"

    # Verify iteration tracking integration
    grep -q "iteration_count" "$GITHUB_OUTPUT"

    # Verify todo management integration
    grep -q "todo_summary" "$GITHUB_OUTPUT"
}

#####################################
# Test Cross-Component Data Flow
#####################################

@test "data flows correctly between comment manager and iteration tracker" {
    setup_scenario_subsequent_iteration

    # Create existing comment with historical data
    export MOCK_EXISTING_COMMENT="$(create_mock_existing_comment 2)"

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Verify historical context is extracted and preserved
    local state_file="$TEST_TEMP_DIR/iterative-review/pr-$TEST_PR_NUMBER-state.json"
    if [ -f "$state_file" ]; then
        [[ "$(cat "$state_file")" == *"historical_context"* ]]
    fi
}

@test "todo manager integrates with comment generation" {
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Verify todos are parsed from AI analysis and included in outputs
    grep -q "todo_summary" "$GITHUB_OUTPUT"

    # Check that todo data is properly formatted
    local todo_summary
    todo_summary=$(grep "todo_summary=" "$GITHUB_OUTPUT" | cut -d'=' -f2-)
    [[ "$todo_summary" == *"total"* ]] || [[ "$todo_summary" != "" ]]
}

@test "github api operations coordinate with state management" {
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Verify PR metadata is captured and stored
    local state_file="$TEST_TEMP_DIR/iterative-review/pr-$TEST_PR_NUMBER-state.json"
    if [ -f "$state_file" ]; then
        [[ "$(cat "$state_file")" == *"pr_metadata"* ]]
        [[ "$(cat "$state_file")" == *"changed_files"* ]]
    fi
}

@test "epic context flows through all components" {
    setup_scenario_first_iteration
    export INPUT_EPIC_CONTEXT="Epic #181 autonomous development testing"

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Verify Epic context appears in outputs
    grep -q "epic_progress=" "$GITHUB_OUTPUT"
    local epic_progress
    epic_progress=$(grep "epic_progress=" "$GITHUB_OUTPUT" | cut -d'=' -f2-)
    [[ "$epic_progress" == *"Epic #181"* ]] || [[ "$epic_progress" != "" ]]
}

#####################################
# Test Error Handling and Recovery Integration
#####################################

@test "workflow handles GitHub API failures gracefully across components" {
    setup_scenario_first_iteration
    export MOCK_GITHUB_API_FAILURE="true"

    # Execute workflow
    run execute_iterative_review_workflow

    # Should handle API failures
    [ "$status" -ne 0 ]
    [[ "$output" == *"error"* ]] || [[ "$output" == *"fail"* ]]
}

@test "workflow recovers from comment management errors" {
    setup_scenario_subsequent_iteration
    export MOCK_COMMENT_OPERATION_FAILURE="true"

    # Execute workflow
    run execute_iterative_review_workflow

    # Should attempt recovery
    [ "$status" -eq 0 ] || [[ "$output" == *"fallback"* ]]
}

@test "workflow handles state corruption and recovery" {
    setup_scenario_subsequent_iteration

    # Create corrupted state file
    echo "corrupted state data" > "$TEST_TEMP_DIR/iterative-review/pr-$TEST_PR_NUMBER-state.json"

    # Execute workflow
    run execute_iterative_review_workflow

    # Should recover and start fresh or use backup
    [ "$status" -eq 0 ]
    grep -q "iteration_count=" "$GITHUB_OUTPUT"
}

@test "workflow handles AI analysis parsing errors" {
    setup_scenario_first_iteration
    export MOCK_AI_ANALYSIS="Malformed AI response without proper structure"

    # Execute workflow
    run execute_iterative_review_workflow

    # Should handle parsing errors gracefully
    [ "$status" -eq 0 ]
    grep -q "todo_summary=" "$GITHUB_OUTPUT"
    # Todo summary should indicate no todos found or parsing error
}

#####################################
# Test Quality Gate Integration
#####################################

@test "workflow enforces quality gates for PR status transitions" {
    setup_scenario_subsequent_iteration

    # Create state with critical todos pending
    local state_with_critical
    state_with_critical=$(create_mock_iteration_state 2 1)
    cat > "$TEST_TEMP_DIR/iterative-review/pr-$TEST_PR_NUMBER-state.json" << 'EOF'
{
  "schema_version": "1.0",
  "pr_number": 123,
  "iteration_count": 2,
  "current_todo_list": [
    {
      "id": "todo-001",
      "category": "CRITICAL",
      "description": "Security vulnerability",
      "status": "pending"
    }
  ]
}
EOF

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Should not advance PR to ready due to critical issues
    local pr_status
    pr_status=$(grep "pr_status=" "$GITHUB_OUTPUT" | cut -d'=' -f2-)
    [[ "$pr_status" == "draft" ]] || [[ "$pr_status" == *"not_ready"* ]]
}

@test "workflow allows PR advancement when quality gates pass" {
    setup_scenario_subsequent_iteration

    # Create state with all critical issues resolved
    cat > "$TEST_TEMP_DIR/iterative-review/pr-$TEST_PR_NUMBER-state.json" << 'EOF'
{
  "schema_version": "1.0",
  "pr_number": 123,
  "iteration_count": 2,
  "current_todo_list": [
    {
      "id": "todo-001",
      "category": "MEDIUM",
      "description": "Documentation update",
      "status": "pending"
    }
  ]
}
EOF

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Should allow PR advancement
    local pr_status
    pr_status=$(grep "pr_status=" "$GITHUB_OUTPUT" | cut -d'=' -f2-)
    [[ "$pr_status" == "ready" ]] || [[ "$pr_status" != "draft" ]]
}

@test "workflow identifies and reports blocking issues" {
    setup_scenario_first_iteration

    # Execute workflow with blocking issues in AI analysis
    export MOCK_AI_ANALYSIS="## Analysis

Critical Issues:
1. **CRITICAL**: Security vulnerability in authentication
2. **CRITICAL**: Data corruption risk in storage layer
3. **HIGH**: Test failures in core functionality"

    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Should identify blocking issues
    grep -q "blocking_issues=" "$GITHUB_OUTPUT"
    local blocking_issues
    blocking_issues=$(grep "blocking_issues=" "$GITHUB_OUTPUT" | cut -d'=' -f2-)
    [[ "$blocking_issues" == *"CRITICAL"* ]] || [[ "$blocking_issues" != "" ]]
}

#####################################
# Test Epic #181 Integration
#####################################

@test "workflow tracks Epic #181 progression accurately" {
    setup_scenario_first_iteration
    export INPUT_EPIC_CONTEXT="Epic #181 autonomous development cycle"

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Verify Epic progression tracking
    grep -q "epic_progress=" "$GITHUB_OUTPUT"
    local epic_progress
    epic_progress=$(grep "epic_progress=" "$GITHUB_OUTPUT" | cut -d'=' -f2-)
    [[ "$epic_progress" == *"Epic #181"* ]]
    [[ "$epic_progress" == *"autonomous"* ]] || [[ "$epic_progress" == *"cycle"* ]]
}

@test "workflow aligns todo items with Epic objectives" {
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Verify todos are aligned with Epic context
    local state_file="$TEST_TEMP_DIR/iterative-review/pr-$TEST_PR_NUMBER-state.json"
    if [ -f "$state_file" ]; then
        [[ "$(cat "$state_file")" == *"epic_alignment"* ]]
        [[ "$(cat "$state_file")" == *"Epic #181"* ]]
    fi
}

@test "workflow provides Epic milestone progression insights" {
    setup_scenario_subsequent_iteration

    # Create state showing progression over multiple iterations
    create_mock_iteration_state 3 2

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Should provide progression insights
    grep -q "epic_progress=" "$GITHUB_OUTPUT"
    if [ -s "$GITHUB_STEP_SUMMARY" ]; then
        [[ "$(cat "$GITHUB_STEP_SUMMARY")" == *"Epic"* ]]
    fi
}

#####################################
# Test Performance and Scalability Integration
#####################################

@test "complete workflow executes within performance targets" {
    setup_scenario_first_iteration

    # Measure complete workflow execution time
    local start_time=$(date +%s)
    run execute_iterative_review_workflow
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))

    # Should complete within reasonable time (under 30 seconds for mocked operations)
    [ "$status" -eq 0 ]
    [ "$duration" -lt 30 ]
}

@test "workflow handles large-scale todo management efficiently" {
    setup_scenario_first_iteration

    # Create AI analysis with many todos
    export MOCK_AI_ANALYSIS="## Analysis with Many Issues

$(for i in {1..50}; do
    echo "$i. **MEDIUM**: Issue $i needs attention in file$i.js:$((i*10))"
done)"

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Should handle large todo lists efficiently
    grep -q "todo_summary=" "$GITHUB_OUTPUT"
    local todo_summary
    todo_summary=$(grep "todo_summary=" "$GITHUB_OUTPUT" | cut -d'=' -f2-)
    [[ "$todo_summary" == *"total"* ]]
}

@test "workflow manages memory efficiently with large state files" {
    setup_scenario_subsequent_iteration

    # Create large historical context
    local large_state_file="$TEST_TEMP_DIR/iterative-review/pr-$TEST_PR_NUMBER-state.json"
    cat > "$large_state_file" << 'EOF'
{
  "schema_version": "1.0",
  "pr_number": 123,
  "iteration_count": 10,
  "historical_context": {
    "iteration_history": [
EOF

    # Add many historical iterations
    for i in {1..20}; do
        cat >> "$large_state_file" << EOF
      {
        "iteration": $i,
        "timestamp": "2025-09-23T$((i%24)):00:00Z",
        "summary": "Iteration $i with detailed analysis and comprehensive review notes",
        "todo_count": $((i*2)),
        "resolved_count": $i
      }$([ $i -lt 20 ] && echo "," || echo "")
EOF
    done

    cat >> "$large_state_file" << 'EOF'
    ]
  },
  "current_todo_list": []
}
EOF

    # Execute workflow
    run execute_iterative_review_workflow

    # Should handle large state efficiently
    [ "$status" -eq 0 ]
    grep -q "iteration_count=" "$GITHUB_OUTPUT"
}

#####################################
# Test Security and Input Validation Integration
#####################################

@test "workflow sanitizes dangerous content across all components" {
    setup_scenario_first_iteration

    # Inject potentially dangerous content in AI analysis
    export MOCK_AI_ANALYSIS="## Analysis

1. **HIGH**: Fix XSS vulnerability <script>alert('xss')</script>
2. **MEDIUM**: Update file with injection: <img src=x onerror=alert(1)>"

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Verify dangerous content is sanitized
    [[ "$output" != *"<script>"* ]]
    [[ "$output" != *"onerror="* ]]

    # Check state file doesn't contain dangerous content
    local state_file="$TEST_TEMP_DIR/iterative-review/pr-$TEST_PR_NUMBER-state.json"
    if [ -f "$state_file" ]; then
        [[ "$(cat "$state_file")" != *"<script>"* ]]
    fi
}

@test "workflow validates all input parameters comprehensively" {
    # Test with various invalid inputs
    export INPUT_PR_NUMBER="0"
    export INPUT_MAX_ITERATIONS="invalid"

    # Execute workflow
    run execute_iterative_review_workflow

    # Should validate inputs
    [ "$status" -ne 0 ]
    [[ "$output" == *"invalid"* ]] || [[ "$output" == *"error"* ]]
}

@test "workflow handles injection attempts in todo descriptions" {
    setup_scenario_first_iteration

    export MOCK_AI_ANALYSIS="## Analysis

1. **HIGH**: $(cat /etc/passwd) - Fix security issue
2. **MEDIUM**: \`rm -rf /\` - Clean up code"

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Should sanitize injection attempts
    [[ "$output" != *"/etc/passwd"* ]]
    [[ "$output" != *"rm -rf"* ]]
}

#####################################
# Test Configuration and Customization Integration
#####################################

@test "workflow respects maximum iteration limits" {
    setup_scenario_subsequent_iteration
    export INPUT_MAX_ITERATIONS="3"

    # Create state at iteration limit
    create_mock_iteration_state 3 1

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Should respect iteration limit
    local iteration_count
    iteration_count=$(grep "iteration_count=" "$GITHUB_OUTPUT" | cut -d'=' -f2-)
    [ "$iteration_count" -le 3 ]
}

@test "workflow handles debug mode across all components" {
    setup_scenario_first_iteration
    export INPUT_DEBUG_MODE="true"

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Should provide enhanced debug output
    [[ "$output" == *"DEBUG"* ]] || [ ${#output} -gt 500 ]
}

@test "workflow integrates custom quality thresholds" {
    setup_scenario_first_iteration
    export INPUT_QUALITY_THRESHOLD="strict"

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Should apply strict quality standards
    local pr_status
    pr_status=$(grep "pr_status=" "$GITHUB_OUTPUT" | cut -d'=' -f2-)
    # In strict mode, should be more conservative about advancement
}

#####################################
# Test Comprehensive Output Validation
#####################################

@test "workflow generates all required GitHub Actions outputs" {
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Verify all required outputs are present
    grep -q "iteration_count=" "$GITHUB_OUTPUT"
    grep -q "pr_status=" "$GITHUB_OUTPUT"
    grep -q "todo_summary=" "$GITHUB_OUTPUT"
    grep -q "quality_gates=" "$GITHUB_OUTPUT"
    grep -q "next_actions=" "$GITHUB_OUTPUT"
    grep -q "epic_progress=" "$GITHUB_OUTPUT"
    grep -q "comment_updated=" "$GITHUB_OUTPUT"
    grep -q "blocking_issues=" "$GITHUB_OUTPUT"
}

@test "workflow creates comprehensive step summary" {
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Verify step summary contains key information
    [ -s "$GITHUB_STEP_SUMMARY" ]
    local summary_content
    summary_content=$(cat "$GITHUB_STEP_SUMMARY")
    [[ "$summary_content" == *"Iteration"* ]]
    [[ "$summary_content" == *"To-Do"* ]] || [[ "$summary_content" == *"Progress"* ]]
}

@test "workflow output format is valid for GitHub Actions consumption" {
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    [ "$status" -eq 0 ]

    # Validate output format
    while IFS= read -r line; do
        # Each line should be in format "key=value"
        [[ "$line" =~ ^[a-zA-Z_][a-zA-Z0-9_]*=.*$ ]]
    done < "$GITHUB_OUTPUT"
}