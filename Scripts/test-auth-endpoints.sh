#!/bin/bash
# AuthController Endpoint Testing Script
# This script tests all authentication-related endpoints and edge cases

# Configuration
BASE_URL="http://localhost:5173/api/auth"
TEST_EMAIL="placeholder"
TEST_PASSWORD="placeholder"
CONTENT_TYPE="Content-Type: application/json"

# Text formatting
BLUE='\033[0;34m'
CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Helper Functions
invoke_request() {
    endpoint=$1
    body=$2
    method=${3:-POST}
    auth_header=$4
    
    url="${BASE_URL}/${endpoint}"
    
    echo -e "\n${CYAN}=== Testing: ${method} ${url} ===${NC}\n"
    echo -e "${YELLOW}Request:${NC}"
    echo "$body"
    echo ""
    
    headers=(-H "$CONTENT_TYPE")
    
    if [ ! -z "$auth_header" ]; then
        headers+=(-H "$auth_header")
    fi
    
    response=$(curl -s -X "$method" "${url}" -d "$body" "${headers[@]}" -w "\n%{http_code}")
    
    status_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [[ $status_code -ge 200 && $status_code -lt 300 ]]; then
        echo -e "${GREEN}Response (Status: $status_code):${NC}"
        echo "$body" | jq '.'
        echo "$body"
    else
        echo -e "${RED}Error (Status: $status_code):${NC}"
        echo "$body" | jq '.' 2>/dev/null || echo "$body"
    fi
    
    echo "$body"
}

print_test_header() {
    title=$1
    
    echo -e "\n\n${BLUE}=============================================="
    echo -e "   $title"
    echo -e "==============================================${NC}"
}

# Global variables to store tokens
ACCESS_TOKEN=""
REFRESH_TOKEN=""
CONFIRMATION_TOKEN=""
USER_ID=""
RESET_TOKEN=""

# ==== Registration Tests ====
print_test_header "REGISTRATION TESTS"

# Test: Register with valid credentials
register_result=$(invoke_request "register" "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}")

if echo "$register_result" | jq -e '.Success == true' >/dev/null 2>&1; then
    CONFIRMATION_TOKEN=$(echo "$register_result" | jq -r '.Token')
    USER_ID=$(echo "$register_result" | jq -r '.UserId // "unknown"')
    echo -e "${GREEN}Successfully registered user and got confirmation token${NC}"
fi

# Test: Register with the same email (should fail)
invoke_request "register" "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}"

# Test: Register with invalid data
invoke_request "register" "{\"email\":\"\",\"password\":\"$TEST_PASSWORD\"}"
invoke_request "register" "{\"email\":\"$TEST_EMAIL\",\"password\":\"\"}"

# ==== Email Confirmation Tests ====
print_test_header "EMAIL CONFIRMATION TESTS"

# Test: Try to login without confirming email (should fail)
invoke_request "login" "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}"

# If we didn't get the userId from the register response, we could get it here (would require login without email verification)
if [ -z "$USER_ID" ] || [ "$USER_ID" = "unknown" ] || [ "$USER_ID" = "null" ]; then
    echo -e "${YELLOW}Note: UserId was not returned in registration. In a real scenario, you'd extract it from the confirmation URL in the email.${NC}"
    # For testing purposes, you might need to set this manually if not returned by the API
    # USER_ID="some-user-id"
fi

# Test: Confirm email with valid token
if [ ! -z "$CONFIRMATION_TOKEN" ] && [ "$CONFIRMATION_TOKEN" != "null" ] && [ ! -z "$USER_ID" ] && [ "$USER_ID" != "unknown" ] && [ "$USER_ID" != "null" ]; then
    confirm_result=$(invoke_request "confirm-email" "{\"userId\":\"$USER_ID\",\"token\":\"$CONFIRMATION_TOKEN\"}")
else
    echo -e "${YELLOW}Skipping email confirmation test since we don't have a confirmation token or user ID${NC}"
fi

# Test: Confirm email with invalid token
invoke_request "confirm-email" "{\"userId\":\"${USER_ID:-invalid-user-id}\",\"token\":\"invalid-token\"}"

# Test: Resend confirmation email
resend_result=$(invoke_request "resend-confirmation" "{\"email\":\"$TEST_EMAIL\"}")

# If the first confirmation was successful, this should say "already confirmed"
if echo "$resend_result" | jq -e '.Token != null' >/dev/null 2>&1; then
    CONFIRMATION_TOKEN=$(echo "$resend_result" | jq -r '.Token')
    echo -e "${GREEN}Got new confirmation token from resend endpoint${NC}"
    
    # Test: Confirm email with the new token
    if [ ! -z "$USER_ID" ] && [ "$USER_ID" != "unknown" ] && [ "$USER_ID" != "null" ]; then
        invoke_request "confirm-email" "{\"userId\":\"$USER_ID\",\"token\":\"$CONFIRMATION_TOKEN\"}"
    fi
fi

# ==== Login Tests ====
print_test_header "LOGIN TESTS"

# Test: Login with valid credentials after confirmation
login_result=$(invoke_request "login" "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}")

if echo "$login_result" | jq -e '.Success == true' >/dev/null 2>&1; then
    ACCESS_TOKEN=$(echo "$login_result" | jq -r '.Token')
    REFRESH_TOKEN=$(echo "$login_result" | jq -r '.RefreshToken')
    echo -e "${GREEN}Successfully logged in and got tokens${NC}"
fi

# Test: Login with invalid password
invoke_request "login" "{\"email\":\"$TEST_EMAIL\",\"password\":\"wrong-password\"}"

# Test: Login with invalid email
invoke_request "login" "{\"email\":\"nonexistent@example.com\",\"password\":\"$TEST_PASSWORD\"}"

# ==== Token Refresh Tests ====
print_test_header "TOKEN REFRESH TESTS"

# Test: Refresh token
if [ ! -z "$REFRESH_TOKEN" ] && [ "$REFRESH_TOKEN" != "null" ]; then
    refresh_result=$(invoke_request "refresh" "{\"refreshToken\":\"$REFRESH_TOKEN\"}")
    
    if echo "$refresh_result" | jq -e '.Success == true' >/dev/null 2>&1; then
        ACCESS_TOKEN=$(echo "$refresh_result" | jq -r '.Token')
        REFRESH_TOKEN=$(echo "$refresh_result" | jq -r '.RefreshToken')
        echo -e "${GREEN}Successfully refreshed tokens${NC}"
    fi
else
    echo -e "${YELLOW}Skipping token refresh test since we don't have a refresh token${NC}"
fi

# Test: Refresh with invalid token
invoke_request "refresh" "{\"refreshToken\":\"invalid-refresh-token\"}"

# ==== Revoke Token Tests ====
print_test_header "REVOKE TOKEN TESTS"

# Test: Revoke token (requires authorization header)
if [ ! -z "$REFRESH_TOKEN" ] && [ "$REFRESH_TOKEN" != "null" ] && [ ! -z "$ACCESS_TOKEN" ] && [ "$ACCESS_TOKEN" != "null" ]; then
    auth_header="Authorization: Bearer $ACCESS_TOKEN"
    
    revoke_result=$(invoke_request "revoke" "{\"refreshToken\":\"$REFRESH_TOKEN\"}" "POST" "$auth_header")
    
    if echo "$revoke_result" | jq -e '.Success == true' >/dev/null 2>&1; then
        echo -e "${GREEN}Successfully revoked refresh token${NC}"
        REFRESH_TOKEN=""
    fi
    
    # Test: Try to refresh with the revoked token (should fail)
    invoke_request "refresh" "{\"refreshToken\":\"${REFRESH_TOKEN:-revoked-token}\"}"
else
    echo -e "${YELLOW}Skipping token revocation test since we don't have tokens${NC}"
fi

# ==== Password Reset Tests ====
print_test_header "PASSWORD RESET TESTS"

# Test: Forgot password
forgot_result=$(invoke_request "forgot-password" "{\"email\":\"$TEST_EMAIL\"}")

if echo "$forgot_result" | jq -e '.Success == true' >/dev/null 2>&1; then
    RESET_TOKEN=$(echo "$forgot_result" | jq -r '.Token')
    echo -e "${GREEN}Successfully requested password reset and got token${NC}"
fi

# Test: Forgot password for non-existent user
invoke_request "forgot-password" "{\"email\":\"nonexistent@example.com\"}"

# Test: Reset password with valid token
if [ ! -z "$RESET_TOKEN" ] && [ "$RESET_TOKEN" != "null" ]; then
    NEW_PASSWORD="new-placeholder-password"
    
    reset_result=$(invoke_request "reset-password" "{\"email\":\"$TEST_EMAIL\",\"token\":\"$RESET_TOKEN\",\"newPassword\":\"$NEW_PASSWORD\"}")
    
    if echo "$reset_result" | jq -e '.Success == true' >/dev/null 2>&1; then
        echo -e "${GREEN}Successfully reset password${NC}"
        
        # Test: Login with new password
        login_result=$(invoke_request "login" "{\"email\":\"$TEST_EMAIL\",\"password\":\"$NEW_PASSWORD\"}")
        
        if echo "$login_result" | jq -e '.Success == true' >/dev/null 2>&1; then
            echo -e "${GREEN}Successfully logged in with new password${NC}"
            ACCESS_TOKEN=$(echo "$login_result" | jq -r '.Token')
            REFRESH_TOKEN=$(echo "$login_result" | jq -r '.RefreshToken')
        fi
        
        # Test: Login with old password (should fail)
        invoke_request "login" "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}"
    fi
else
    echo -e "${YELLOW}Skipping password reset test since we don't have a reset token${NC}"
fi

# Test: Reset password with invalid token
invoke_request "reset-password" "{\"email\":\"$TEST_EMAIL\",\"token\":\"invalid-reset-token\",\"newPassword\":\"another-new-password\"}"

echo -e "\n\n${GREEN}All tests completed!${NC}"