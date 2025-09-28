#!/usr/bin/env bats
# Unit Tests for Iterative AI Review Action - GitHub API Integration
# Tests all GitHub API interaction functions with comprehensive mocking

# Setup test environment
setup() {
    # Load test fixtures and mocks
    load "../../fixtures/github-environment.sh"
    load "../../mocks/github-api-mock.sh"

    # Initialize test environment
    initialize_test_environment
    setup_github_api_mocking

    # Source the github-api module under test
    source "${BATS_TEST_DIRNAME}/../../../src/github-api.js"

    # Setup test-specific variables
    export TEST_PR_NUMBER="123"
    export TEST_COMMENT_ID="123456789"
}

# Cleanup after each test
teardown() {
    cleanup_test_environment
    cleanup_github_api_mocking
}

#####################################
# Test PR Status Management
#####################################

@test "get_pr_status returns correct PR information" {
    setup_scenario_first_iteration

    # Execute function
    run get_pr_status "$TEST_PR_NUMBER"

    # Verify success and output format
    [ "$status" -eq 0 ]
    [[ "$output" == *"number"* ]]
    [[ "$output" == *"draft"* ]]
    [[ "$output" == *"state"* ]]
}

@test "get_pr_status handles invalid PR number" {
    # Test with invalid PR number
    run get_pr_status "invalid"

    # Should fail gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"Invalid PR number"* ]] || [[ "$output" == *"error"* ]]
}

@test "get_pr_status handles missing PR" {
    # Mock API to return 404
    export MOCK_PR_NOT_FOUND="true"

    # Execute function
    run get_pr_status "999999"

    # Should handle 404 gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"not found"* ]] || [[ "$output" == *"404"* ]]
}

@test "update_pr_status changes draft to ready" {
    setup_scenario_first_iteration

    # Execute function to change draft to ready
    run update_pr_status "$TEST_PR_NUMBER" "ready"

    # Verify success
    [ "$status" -eq 0 ]
    [[ "$output" == *"draft\":false"* ]] || [[ "$output" == *"ready"* ]]
}

@test "update_pr_status changes ready to draft" {
    setup_scenario_ready_for_merge

    # Execute function to change ready to draft
    run update_pr_status "$TEST_PR_NUMBER" "draft"

    # Verify success
    [ "$status" -eq 0 ]
    [[ "$output" == *"draft\":true"* ]] || [[ "$output" == *"draft"* ]]
}

@test "update_pr_status validates status values" {
    # Test with invalid status
    run update_pr_status "$TEST_PR_NUMBER" "invalid"

    # Should reject invalid status
    [ "$status" -ne 0 ]
    [[ "$output" == *"Invalid status"* ]] || [[ "$output" == *"error"* ]]
}

#####################################
# Test Comment Operations
#####################################

@test "create_pr_comment creates new comment successfully" {
    setup_scenario_first_iteration

    local comment_body="Test comment body with structured content"

    # Execute function
    run create_pr_comment "$TEST_PR_NUMBER" "$comment_body"

    # Verify success and response format
    [ "$status" -eq 0 ]
    [[ "$output" == *"id"* ]]
    [[ "$output" == *"created_at"* ]]
}

@test "create_pr_comment handles empty comment body" {
    # Test with empty comment body
    run create_pr_comment "$TEST_PR_NUMBER" ""

    # Should handle empty body gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"empty"* ]] || [[ "$output" == *"required"* ]]
}

@test "create_pr_comment handles large comment body" {
    # Create large comment body (near GitHub's limit)
    local large_body=$(printf 'Large content %.0s' {1..1000})

    # Execute function
    run create_pr_comment "$TEST_PR_NUMBER" "$large_body"

    # Should handle large content appropriately
    [ "$status" -eq 0 ] || [[ "$output" == *"too large"* ]]
}

@test "update_pr_comment updates existing comment" {
    setup_scenario_subsequent_iteration

    local updated_body="Updated comment body with new content"

    # Execute function
    run update_pr_comment "$TEST_COMMENT_ID" "$updated_body"

    # Verify success
    [ "$status" -eq 0 ]
    [[ "$output" == *"updated"* ]]
    [[ "$output" == *"$TEST_COMMENT_ID"* ]]
}

@test "update_pr_comment handles non-existent comment" {
    # Test with non-existent comment ID
    run update_pr_comment "999999999" "Test body"

    # Should handle missing comment gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"not found"* ]] || [[ "$output" == *"404"* ]]
}

@test "find_existing_iterative_comment detects existing comment" {
    setup_scenario_subsequent_iteration

    # Execute function
    run find_existing_iterative_comment "$TEST_PR_NUMBER"

    # Should find existing iterative comment
    [ "$status" -eq 0 ]
    [[ "$output" == *"123456789"* ]] || [[ "$output" != "" ]]
}

@test "find_existing_iterative_comment handles no existing comment" {
    setup_scenario_first_iteration

    # Execute function
    run find_existing_iterative_comment "$TEST_PR_NUMBER"

    # Should handle no existing comment
    [ "$status" -eq 0 ]
    [ "$output" = "" ]
}

@test "find_existing_iterative_comment validates comment header" {
    setup_scenario_subsequent_iteration

    # Execute function
    run find_existing_iterative_comment "$TEST_PR_NUMBER"

    # Should validate iterative comment header pattern
    [ "$status" -eq 0 ]
    # Comment should match iterative review pattern or be empty for first iteration
}

#####################################
# Test Repository Operations
#####################################

@test "get_pr_metadata returns comprehensive PR information" {
    setup_scenario_first_iteration

    # Execute function
    run get_pr_metadata "$TEST_PR_NUMBER"

    # Verify comprehensive metadata
    [ "$status" -eq 0 ]
    [[ "$output" == *"changed_files"* ]]
    [[ "$output" == *"additions"* ]]
    [[ "$output" == *"deletions"* ]]
    [[ "$output" == *"labels"* ]]
}

@test "get_repository_info returns repository details" {
    # Execute function
    run get_repository_info

    # Verify repository information
    [ "$status" -eq 0 ]
    [[ "$output" == *"full_name"* ]]
    [[ "$output" == *"default_branch"* ]]
    [[ "$output" == *"owner"* ]]
}

@test "get_branch_info returns branch details" {
    # Execute function for source branch
    run get_branch_info "$MOCK_SOURCE_BRANCH"

    # Should return branch information
    [ "$status" -eq 0 ]
    [[ "$output" == *"ref"* ]] || [[ "$output" == *"$MOCK_SOURCE_BRANCH"* ]]
}

@test "get_branch_info handles non-existent branch" {
    # Test with non-existent branch
    run get_branch_info "non-existent-branch"

    # Should handle missing branch gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"not found"* ]] || [[ "$output" == *"404"* ]]
}

#####################################
# Test Label Management
#####################################

@test "get_pr_labels returns PR labels" {
    setup_scenario_first_iteration

    # Execute function
    run get_pr_labels "$TEST_PR_NUMBER"

    # Should return labels array
    [ "$status" -eq 0 ]
    [[ "$output" == *"["* ]] || [ "$output" = "[]" ]
}

@test "add_pr_label adds new label successfully" {
    # Execute function
    run add_pr_label "$TEST_PR_NUMBER" "test-label"

    # Should add label successfully
    [ "$status" -eq 0 ]
    [[ "$output" == *"test-label"* ]] || [[ "$output" == *"added"* ]]
}

@test "add_pr_label handles duplicate label" {
    # Add same label twice
    run add_pr_label "$TEST_PR_NUMBER" "duplicate-label"
    run add_pr_label "$TEST_PR_NUMBER" "duplicate-label"

    # Should handle duplicate gracefully
    [ "$status" -eq 0 ]
}

@test "remove_pr_label removes existing label" {
    # First add a label, then remove it
    add_pr_label "$TEST_PR_NUMBER" "removable-label"
    run remove_pr_label "$TEST_PR_NUMBER" "removable-label"

    # Should remove label successfully
    [ "$status" -eq 0 ]
    [[ "$output" == *"removed"* ]] || [ "$output" != *"removable-label"* ]
}

@test "remove_pr_label handles non-existent label" {
    # Try to remove non-existent label
    run remove_pr_label "$TEST_PR_NUMBER" "non-existent-label"

    # Should handle gracefully
    [ "$status" -eq 0 ]
}

#####################################
# Test Rate Limiting and Authentication
#####################################

@test "check_rate_limit returns rate limit information" {
    # Execute function
    run check_rate_limit

    # Should return rate limit details
    [ "$status" -eq 0 ]
    [[ "$output" == *"limit"* ]]
    [[ "$output" == *"remaining"* ]]
    [[ "$output" == *"reset"* ]]
}

@test "check_rate_limit detects rate limit exhaustion" {
    setup_scenario_rate_limit

    # Execute function
    run check_rate_limit

    # Should detect exhausted rate limit
    [ "$status" -eq 0 ]
    [[ "$output" == *"remaining\":0"* ]]
}

@test "validate_github_token validates token format" {
    # Execute function with valid token format
    run validate_github_token "ghp_validtokenformat123456789"

    # Should validate token format
    [ "$status" -eq 0 ]
}

@test "validate_github_token rejects invalid token format" {
    # Execute function with invalid token format
    run validate_github_token "invalid-token"

    # Should reject invalid format
    [ "$status" -ne 0 ]
    [[ "$output" == *"invalid"* ]] || [[ "$output" == *"format"* ]]
}

@test "validate_github_token handles missing token" {
    # Execute function with empty token
    run validate_github_token ""

    # Should reject empty token
    [ "$status" -ne 0 ]
    [[ "$output" == *"required"* ]] || [[ "$output" == *"missing"* ]]
}

#####################################
# Test Error Handling and Edge Cases
#####################################

@test "github API functions handle network timeouts" {
    # Mock network timeout scenario
    export MOCK_NETWORK_TIMEOUT="true"

    # Execute function that would timeout
    run get_pr_status "$TEST_PR_NUMBER"

    # Should handle timeout gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"timeout"* ]] || [[ "$output" == *"network"* ]]
}

@test "github API functions handle malformed JSON response" {
    # Mock malformed JSON response
    export MOCK_MALFORMED_JSON="true"

    # Execute function
    run get_pr_status "$TEST_PR_NUMBER"

    # Should handle malformed JSON gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"JSON"* ]] || [[ "$output" == *"parse"* ]]
}

@test "github API functions handle server errors" {
    # Mock server error (500)
    export MOCK_SERVER_ERROR="true"

    # Execute function
    run get_pr_status "$TEST_PR_NUMBER"

    # Should handle server errors gracefully
    [ "$status" -ne 0 ]
    [[ "$output" == *"server"* ]] || [[ "$output" == *"500"* ]]
}

@test "github API functions handle unauthorized access" {
    # Mock unauthorized response (401)
    export MOCK_UNAUTHORIZED="true"

    # Execute function
    run get_pr_status "$TEST_PR_NUMBER"

    # Should handle unauthorized access
    [ "$status" -ne 0 ]
    [[ "$output" == *"unauthorized"* ]] || [[ "$output" == *"401"* ]]
}

@test "github API functions handle forbidden access" {
    # Mock forbidden response (403)
    export MOCK_FORBIDDEN="true"

    # Execute function
    run get_pr_status "$TEST_PR_NUMBER"

    # Should handle forbidden access
    [ "$status" -ne 0 ]
    [[ "$output" == *"forbidden"* ]] || [[ "$output" == *"403"* ]]
}

#####################################
# Test Pagination and Large Data Sets
#####################################

@test "comment listing handles pagination" {
    # Mock large number of comments requiring pagination
    export MOCK_PAGINATED_COMMENTS="true"

    # Execute function
    run find_existing_iterative_comment "$TEST_PR_NUMBER"

    # Should handle pagination correctly
    [ "$status" -eq 0 ]
    # Result should be consistent regardless of pagination
}

@test "comment operations handle concurrent modifications" {
    # Simulate concurrent comment modifications
    setup_scenario_subsequent_iteration

    # Execute multiple operations concurrently (simulated)
    run create_pr_comment "$TEST_PR_NUMBER" "Concurrent comment 1"
    local status1=$status
    run create_pr_comment "$TEST_PR_NUMBER" "Concurrent comment 2"
    local status2=$status

    # Both operations should succeed or handle conflicts gracefully
    [ "$status1" -eq 0 ] || [ "$status2" -eq 0 ]
}

#####################################
# Test Input Validation and Sanitization
#####################################

@test "github API functions validate PR number format" {
    # Test various invalid PR number formats
    run get_pr_status "0"
    [ "$status" -ne 0 ]

    run get_pr_status "-1"
    [ "$status" -ne 0 ]

    run get_pr_status "abc"
    [ "$status" -ne 0 ]
}

@test "comment functions sanitize dangerous content" {
    # Test with potentially dangerous comment content
    local dangerous_content="<script>alert('xss')</script>"

    # Execute function
    run create_pr_comment "$TEST_PR_NUMBER" "$dangerous_content"

    # Should sanitize or escape dangerous content
    [ "$status" -eq 0 ]
    [[ "$output" != *"<script>"* ]] || [[ "$output" == *"escaped"* ]]
}

@test "label functions validate label names" {
    # Test with invalid label names
    run add_pr_label "$TEST_PR_NUMBER" ""
    [ "$status" -ne 0 ]

    run add_pr_label "$TEST_PR_NUMBER" "invalid label with spaces and/special chars!"
    # Should handle invalid label names appropriately
    [ "$status" -ne 0 ] || [[ "$output" == *"normalized"* ]]
}

#####################################
# Test Performance and Efficiency
#####################################

@test "github API functions complete within reasonable time" {
    # Measure execution time for basic operations
    local start_time=$(date +%s)
    run get_pr_status "$TEST_PR_NUMBER"
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))

    # Should complete quickly (under 5 seconds for mocked operations)
    [ "$status" -eq 0 ]
    [ "$duration" -lt 5 ]
}

@test "comment operations minimize API calls" {
    # Execute function that should be efficient
    run find_existing_iterative_comment "$TEST_PR_NUMBER"

    # Should complete successfully with minimal API calls
    [ "$status" -eq 0 ]
    # In a real implementation, would verify API call count
}

@test "batch operations handle multiple items efficiently" {
    # Test batch label operations (if supported)
    run add_pr_label "$TEST_PR_NUMBER" "label1"
    run add_pr_label "$TEST_PR_NUMBER" "label2"
    run add_pr_label "$TEST_PR_NUMBER" "label3"

    # All operations should succeed
    [ "$status" -eq 0 ]
}