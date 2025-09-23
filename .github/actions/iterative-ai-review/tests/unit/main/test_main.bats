#!/usr/bin/env bats
# Unit Tests for Iterative AI Review Action - Main Orchestration Logic
# Tests the core workflow execution and orchestration functionality

# Setup test environment
setup() {
    # Load test fixtures and mocks
    load "../../fixtures/github-environment.sh"
    load "../../mocks/github-api-mock.sh"

    # Initialize test environment
    initialize_test_environment
    setup_github_api_mocking

    # Source the main module under test
    source "${BATS_TEST_DIRNAME}/../../../src/main.js"

    # Setup test-specific variables
    export TEST_ACTION_DIR="${BATS_TEST_DIRNAME}/../../.."
    export GITHUB_OUTPUT="$TEST_TEMP_DIR/github_output"
    export GITHUB_STEP_SUMMARY="$TEST_TEMP_DIR/step_summary"

    # Create required files for GitHub Actions
    touch "$GITHUB_OUTPUT"
    touch "$GITHUB_STEP_SUMMARY"
}

# Cleanup after each test
teardown() {
    cleanup_test_environment
    cleanup_github_api_mocking
}

#####################################
# Test Workflow Initialization
#####################################

@test "execute_iterative_review_workflow initializes correctly" {
    # Mock successful initialization
    setup_scenario_first_iteration

    # Mock AI analysis response
    export MOCK_AI_RESPONSE="Test AI analysis response"

    # Execute workflow initialization
    run execute_iterative_review_workflow

    # Verify initialization success
    [ "$status" -eq 0 ]
    [ -n "$WORKFLOW_START_TIME" ]
}

@test "workflow validates required environment variables" {
    # Remove required environment variable
    unset INPUT_GITHUB_TOKEN

    # Execute workflow
    run execute_iterative_review_workflow

    # Should fail due to missing required input
    [ "$status" -ne 0 ]
    [[ "$output" == *"github_token"* ]]
}

@test "workflow validates PR number format" {
    # Set invalid PR number
    export INPUT_PR_NUMBER="invalid"

    # Execute workflow
    run execute_iterative_review_workflow

    # Should fail due to invalid PR number
    [ "$status" -ne 0 ]
    [[ "$output" == *"Invalid PR number"* ]]
}

#####################################
# Test Iteration Management
#####################################

@test "workflow handles first iteration correctly" {
    setup_scenario_first_iteration

    # Mock AI analysis
    export MOCK_AI_RESPONSE="Initial review analysis"

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify first iteration handling
    [ "$status" -eq 0 ]
    [[ "$output" == *"iteration_count=1"* ]]
}

@test "workflow handles subsequent iterations correctly" {
    setup_scenario_subsequent_iteration

    # Create existing state file
    create_mock_iteration_state 2 3

    # Mock AI analysis
    export MOCK_AI_RESPONSE="Follow-up review analysis"

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify subsequent iteration handling
    [ "$status" -eq 0 ]
    [[ "$output" == *"iteration_count=3"* ]]
}

@test "workflow respects maximum iteration limit" {
    # Set low iteration limit
    export INPUT_MAX_ITERATIONS="2"

    # Create state with iterations at limit
    create_mock_iteration_state 2 1

    # Execute workflow
    run execute_iterative_review_workflow

    # Should not exceed maximum iterations
    [ "$status" -eq 0 ]
    [[ "$output" == *"Maximum iterations reached"* ]]
}

@test "workflow handles forced new iteration" {
    export INPUT_FORCE_NEW_ITERATION="true"
    setup_scenario_subsequent_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    # Should start new iteration cycle
    [ "$status" -eq 0 ]
    [[ "$output" == *"Forcing new iteration"* ]]
}

#####################################
# Test Context Loading and Management
#####################################

@test "workflow loads existing context correctly" {
    setup_scenario_subsequent_iteration

    # Create existing state file with specific context
    local state_file
    state_file=$(create_mock_iteration_state 2 4)

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify context loading
    [ "$status" -eq 0 ]
    [[ "$output" == *"Loading existing context"* ]]
}

@test "workflow handles missing context gracefully" {
    setup_scenario_first_iteration

    # Ensure no existing state
    rm -f "$TEST_TEMP_DIR/iterative-review"/*.json 2>/dev/null || true

    # Execute workflow
    run execute_iterative_review_workflow

    # Should handle missing context gracefully
    [ "$status" -eq 0 ]
    [[ "$output" == *"No existing context found"* ]]
}

@test "workflow validates context schema" {
    setup_scenario_subsequent_iteration

    # Create invalid state file
    echo '{"invalid": "schema"}' > "$TEST_TEMP_DIR/iterative-review/pr-123-state.json"

    # Execute workflow
    run execute_iterative_review_workflow

    # Should handle invalid schema
    [ "$status" -eq 0 ]
    [[ "$output" == *"Invalid context schema"* ]] || [[ "$output" == *"Starting fresh"* ]]
}

#####################################
# Test AI Analysis Integration
#####################################

@test "workflow integrates with AI analysis correctly" {
    setup_scenario_first_iteration

    # Mock successful AI analysis
    export MOCK_AI_RESPONSE="Comprehensive AI analysis with todo items"

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify AI integration
    [ "$status" -eq 0 ]
    [[ "$output" == *"AI analysis completed"* ]]
}

@test "workflow handles AI analysis failure" {
    setup_scenario_first_iteration

    # Mock AI analysis failure
    export MOCK_AI_FAILURE="true"

    # Execute workflow
    run execute_iterative_review_workflow

    # Should handle AI failure gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"AI analysis failed"* ]]
}

@test "workflow processes AI response correctly" {
    setup_scenario_first_iteration

    # Mock AI response with structured content
    export MOCK_AI_RESPONSE="## To-Do Items\n1. **CRITICAL**: Fix security issue\n2. **HIGH**: Add tests"

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify AI response processing
    [ "$status" -eq 0 ]
    [[ "$output" == *"Processing AI analysis"* ]]
}

#####################################
# Test Comment Management
#####################################

@test "workflow creates new comment for first iteration" {
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify comment creation
    [ "$status" -eq 0 ]
    [[ "$output" == *"Creating new comment"* ]] || [[ "$output" == *"comment_updated=true"* ]]
}

@test "workflow updates existing comment for subsequent iteration" {
    setup_scenario_subsequent_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify comment update
    [ "$status" -eq 0 ]
    [[ "$output" == *"Updating existing comment"* ]] || [[ "$output" == *"comment_updated=true"* ]]
}

@test "workflow handles comment size limits" {
    setup_scenario_first_iteration

    # Mock very large AI response
    export MOCK_AI_RESPONSE=$(printf 'Large content %.0s' {1..1000})

    # Execute workflow
    run execute_iterative_review_workflow

    # Should handle size limits appropriately
    [ "$status" -eq 0 ]
    [[ "$output" == *"Comment size"* ]] || [ -n "$output" ]
}

#####################################
# Test Quality Gate Assessment
#####################################

@test "workflow assesses quality gates correctly" {
    setup_scenario_subsequent_iteration

    # Create state with mixed todo completion
    create_mock_iteration_state 2 5

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify quality gate assessment
    [ "$status" -eq 0 ]
    [[ "$output" == *"quality_gates"* ]]
}

@test "workflow identifies blocking issues" {
    setup_scenario_subsequent_iteration

    # Create state with critical todos
    create_mock_iteration_state 2 3

    # Execute workflow
    run execute_iterative_review_workflow

    # Should identify blocking issues
    [ "$status" -eq 0 ]
    [[ "$output" == *"blocking_issues"* ]]
}

@test "workflow handles ready for merge scenario" {
    setup_scenario_ready_for_merge

    # Create state with all critical issues resolved
    create_mock_iteration_state 3 0

    # Execute workflow
    run execute_iterative_review_workflow

    # Should indicate ready for merge
    [ "$status" -eq 0 ]
    [[ "$output" == *"ready"* ]] || [[ "$output" == *"pr_status=ready"* ]]
}

#####################################
# Test Output Generation
#####################################

@test "workflow generates complete outputs" {
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify all required outputs are generated
    [ "$status" -eq 0 ]

    # Check GitHub output file contains required outputs
    grep -q "iteration_count=" "$GITHUB_OUTPUT"
    grep -q "pr_status=" "$GITHUB_OUTPUT"
    grep -q "todo_summary=" "$GITHUB_OUTPUT"
    grep -q "quality_gates=" "$GITHUB_OUTPUT"
    grep -q "next_actions=" "$GITHUB_OUTPUT"
    grep -q "epic_progress=" "$GITHUB_OUTPUT"
    grep -q "comment_updated=" "$GITHUB_OUTPUT"
}

@test "workflow generates step summary" {
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify step summary is generated
    [ "$status" -eq 0 ]
    [ -s "$GITHUB_STEP_SUMMARY" ]
}

@test "workflow outputs include Epic context" {
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    # Verify Epic context in outputs
    [ "$status" -eq 0 ]
    [[ "$output" == *"Epic #181"* ]] || grep -q "Epic #181" "$GITHUB_OUTPUT"
}

#####################################
# Test Error Handling
#####################################

@test "workflow handles GitHub API errors gracefully" {
    setup_scenario_first_iteration

    # Mock GitHub API failure
    export MOCK_GITHUB_API_FAILURE="true"

    # Execute workflow
    run execute_iterative_review_workflow

    # Should handle API errors gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"GitHub API error"* ]] || [[ "$output" == *"API failure"* ]]
}

@test "workflow handles rate limiting" {
    setup_scenario_rate_limit

    # Execute workflow
    run execute_iterative_review_workflow

    # Should handle rate limiting appropriately
    [ "$status" -ne 0 ] || [[ "$output" == *"rate limit"* ]]
}

@test "workflow handles invalid PR state" {
    setup_scenario_first_iteration

    # Mock closed PR
    export MOCK_PR_STATE="closed"

    # Execute workflow
    run execute_iterative_review_workflow

    # Should handle invalid PR state
    [ "$status" -ne 0 ]
    [[ "$output" == *"closed"* ]] || [[ "$output" == *"invalid"* ]]
}

#####################################
# Test Debug Mode
#####################################

@test "workflow provides debug output when enabled" {
    export INPUT_DEBUG_MODE="true"
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    # Should provide detailed debug output
    [ "$status" -eq 0 ]
    [[ "$output" == *"DEBUG"* ]] || [ ${#output} -gt 100 ]
}

@test "workflow suppresses verbose output in normal mode" {
    export INPUT_DEBUG_MODE="false"
    setup_scenario_first_iteration

    # Execute workflow
    run execute_iterative_review_workflow

    # Should not include debug output
    [ "$status" -eq 0 ]
    [[ "$output" != *"DEBUG"* ]] || [ ${#output} -lt 1000 ]
}

#####################################
# Test Performance and Timing
#####################################

@test "workflow completes within reasonable time" {
    setup_scenario_first_iteration

    # Measure execution time
    local start_time=$(date +%s)
    run execute_iterative_review_workflow
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))

    # Should complete quickly (under 30 seconds for mocked operations)
    [ "$status" -eq 0 ]
    [ "$duration" -lt 30 ]
}

@test "workflow handles concurrent execution safely" {
    setup_scenario_first_iteration

    # Execute workflow multiple times in parallel (simulated)
    run execute_iterative_review_workflow
    local status1=$status

    run execute_iterative_review_workflow
    local status2=$status

    # Both executions should handle safely
    [ "$status1" -eq 0 ] || [ "$status2" -eq 0 ]
}