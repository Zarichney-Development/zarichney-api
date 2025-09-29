#!/bin/bash
# AI Sentinel Base - AI Service Client with Authentication and Retry Logic
# Secure AI service communication with comprehensive error handling

set -euo pipefail

# Import security validation functions
source "$(dirname "${BASH_SOURCE[0]}")/security-validation.sh"

# AI Service Configuration
readonly DEFAULT_MODEL="gpt-4o"
readonly DEFAULT_MAX_TOKENS=4000
readonly DEFAULT_TEMPERATURE=0.1
readonly REQUEST_TIMEOUT="${TIMEOUT_SECONDS:-300}"
readonly API_BASE_URL="https://api.openai.com/v1"

# Retry configuration
readonly RETRY_DELAYS=(1 2 5 10 15)  # Exponential backoff delays
readonly TRANSIENT_ERROR_CODES=(429 500 502 503 504)

# Response storage
AI_RESPONSE=""
AI_METADATA=""

# Initialize AI service client
init_ai_client() {
    echo "[DEBUG] Initializing AI service client" >&2
    security_log "INFO" "Initializing AI service client"

    # Validate API key presence
    if [[ -z "${OPENAI_API_KEY:-}" ]]; then
        echo "[DEBUG] OpenAI API key not provided" >&2
        security_log "ERROR" "OpenAI API key not provided"
        return 1
    fi
    echo "[DEBUG] OpenAI API key is present" >&2

    # Validate API key format (basic check)
    if [[ ! "${OPENAI_API_KEY}" =~ ^sk-[a-zA-Z0-9]{48,}$ ]]; then
        echo "[DEBUG] API key format may be invalid (not legacy format)" >&2
        security_log "WARN" "API key format may be invalid"
    else
        echo "[DEBUG] API key format appears valid (legacy format)" >&2
    fi

    # Test API connectivity
    echo "[DEBUG] Testing API connectivity..." >&2
    if ! test_api_connectivity; then
        echo "[DEBUG] API connectivity test failed" >&2
        security_log "ERROR" "API connectivity test failed"
        return 1
    fi
    echo "[DEBUG] API connectivity test passed" >&2

    security_log "INFO" "AI service client initialized successfully"
    return 0
}

# Test API connectivity
test_api_connectivity() {
    echo "[DEBUG] Testing API connectivity" >&2
    security_log "INFO" "Testing API connectivity"

    local response
    local status_code

    # Make a simple API test call
    echo "[DEBUG] Making curl request to ${API_BASE_URL}/models" >&2
    response=$(curl -s -w "\n%{http_code}" \
        -H "Authorization: Bearer ${OPENAI_API_KEY}" \
        -H "Content-Type: application/json" \
        --connect-timeout 10 \
        --max-time 30 \
        "${API_BASE_URL}/models" || echo -e "\n000")

    status_code=$(echo "$response" | tail -n1)
    response=$(echo "$response" | head -n -1)
    echo "[DEBUG] API response status code: $status_code" >&2

    if [[ "$status_code" == "200" ]]; then
        echo "[DEBUG] API connectivity test passed" >&2
        security_log "INFO" "API connectivity test passed"
        return 0
    elif [[ "$status_code" == "401" ]]; then
        echo "[DEBUG] API authentication failed - invalid API key" >&2
        echo "[DEBUG] Response body: $response" >&2
        security_log "ERROR" "API authentication failed - invalid API key"
        return 1
    else
        echo "[DEBUG] API connectivity test returned status: $status_code" >&2
        echo "[DEBUG] Response body: $response" >&2
        security_log "WARN" "API connectivity test returned status: $status_code"
        # Don't fail on warnings for other status codes
        return 0
    fi
}

# Execute AI analysis with retry logic
execute_ai_analysis_with_retry() {
    echo "[DEBUG] Starting AI analysis execution with retry logic" >&2
    security_log "INFO" "Starting AI analysis execution with retry logic"

    if [[ ! -f "${PROCESSED_TEMPLATE:-}" ]]; then
        echo "[DEBUG] Processed template not found: ${PROCESSED_TEMPLATE:-}" >&2
        security_log "ERROR" "Processed template not found: ${PROCESSED_TEMPLATE:-}"
        return 1
    fi
    echo "[DEBUG] Processed template found: ${PROCESSED_TEMPLATE}" >&2

    # Initialize AI client
    echo "[DEBUG] Initializing AI client..." >&2
    if ! init_ai_client; then
        echo "[DEBUG] AI client initialization failed" >&2
        security_log "ERROR" "AI client initialization failed"
        return 1
    fi
    echo "[DEBUG] AI client initialized successfully" >&2

    local attempt=1
    local max_attempts="${MAX_RETRIES:-3}"
    echo "[DEBUG] Starting retry loop with max_attempts=$max_attempts" >&2

    while [[ $attempt -le $max_attempts ]]; do
        echo "[DEBUG] AI analysis attempt $attempt of $max_attempts" >&2
        security_log "INFO" "AI analysis attempt $attempt of $max_attempts"

        if execute_ai_analysis; then
            echo "[DEBUG] AI analysis completed successfully on attempt $attempt" >&2
            security_log "INFO" "AI analysis completed successfully on attempt $attempt"
            return 0
        fi
        echo "[DEBUG] AI analysis failed on attempt $attempt" >&2

        if [[ $attempt -lt $max_attempts ]]; then
            local delay_index=$((attempt - 1))
            local delay=${RETRY_DELAYS[$delay_index]:-15}
            echo "[DEBUG] Retrying in ${delay} seconds..." >&2
            security_log "INFO" "Retrying in ${delay} seconds..."
            sleep "$delay"
        fi

        ((attempt++))
    done

    echo "[DEBUG] AI analysis failed after $max_attempts attempts" >&2
    security_log "ERROR" "AI analysis failed after $max_attempts attempts"
    return 1
}

# Execute single AI analysis attempt
execute_ai_analysis() {
    echo "[DEBUG] Executing AI analysis" >&2
    security_log "INFO" "Executing AI analysis"

    local template_content
    echo "[DEBUG] Reading processed template from $PROCESSED_TEMPLATE" >&2
    template_content=$(cat "$PROCESSED_TEMPLATE")

    if [[ -z "$template_content" ]]; then
        echo "[DEBUG] Template content is empty" >&2
        security_log "ERROR" "Template content is empty"
        return 1
    fi
    echo "[DEBUG] Template content loaded (${#template_content} bytes)" >&2

    # Prepare API request
    local request_payload
    echo "[DEBUG] Creating API request payload" >&2
    request_payload=$(create_api_request_payload "$template_content")

    if [[ -z "$request_payload" ]]; then
        echo "[DEBUG] Failed to create API request payload" >&2
        security_log "ERROR" "Failed to create API request payload"
        return 1
    fi
    echo "[DEBUG] API request payload created (${#request_payload} bytes)" >&2

    # Execute API request
    local response
    local status_code
    local start_time
    start_time=$(date +%s)

    echo "[DEBUG] Sending POST request to ${API_BASE_URL}/chat/completions" >&2
    response=$(curl -s -w "\n%{http_code}" \
        -X POST \
        -H "Authorization: Bearer ${OPENAI_API_KEY}" \
        -H "Content-Type: application/json" \
        --data "$request_payload" \
        --connect-timeout 30 \
        --max-time "$REQUEST_TIMEOUT" \
        "${API_BASE_URL}/chat/completions" || echo -e "\n000")

    local end_time
    end_time=$(date +%s)
    local request_duration=$((end_time - start_time))

    status_code=$(echo "$response" | tail -n1)
    response=$(echo "$response" | head -n -1)

    echo "[DEBUG] API request completed - Status: $status_code, Duration: ${request_duration}s" >&2
    security_log "INFO" "API request completed - Status: $status_code, Duration: ${request_duration}s"

    # Handle response based on status code
    echo "[DEBUG] Processing response with status code: $status_code" >&2
    case "$status_code" in
        200)
            echo "[DEBUG] Success response - processing..." >&2
            if process_successful_response "$response" "$request_duration"; then
                echo "[DEBUG] Response processed successfully" >&2
                return 0
            else
                echo "[DEBUG] Failed to process successful response" >&2
                security_log "ERROR" "Failed to process successful response"
                return 1
            fi
            ;;
        401)
            echo "[DEBUG] Authentication failed - invalid API key" >&2
            echo "[DEBUG] Response: $response" >&2
            security_log "ERROR" "Authentication failed - invalid API key"
            return 1
            ;;
        429)
            echo "[DEBUG] Rate limit exceeded - will retry" >&2
            echo "[DEBUG] Response: $response" >&2
            security_log "WARN" "Rate limit exceeded - will retry"
            return 1
            ;;
        500|502|503|504)
            echo "[DEBUG] Server error (${status_code}) - will retry" >&2
            echo "[DEBUG] Response: $response" >&2
            security_log "WARN" "Server error (${status_code}) - will retry"
            return 1
            ;;
        *)
            echo "[DEBUG] Unexpected API response - Status: $status_code" >&2
            echo "[DEBUG] Response: $response" >&2
            security_log "ERROR" "Unexpected API response - Status: $status_code"
            if [[ "${DEBUG_MODE:-false}" == "true" ]]; then
                security_log "DEBUG" "Response: $response"
            fi
            return 1
            ;;
    esac
}

# Create API request payload
create_api_request_payload() {
    local template_content="$1"

    security_log "INFO" "Creating API request payload"

    # Escape content for JSON
    local escaped_content
    escaped_content=$(echo "$template_content" | jq -R -s '.')

    if [[ -z "$escaped_content" || "$escaped_content" == "null" ]]; then
        security_log "ERROR" "Failed to escape template content for JSON"
        return 1
    fi

    # Create request payload with security considerations
    local payload
    payload=$(jq -n \
        --arg model "${DEFAULT_MODEL}" \
        --argjson max_tokens "${DEFAULT_MAX_TOKENS}" \
        --argjson temperature "${DEFAULT_TEMPERATURE}" \
        --argjson content "$escaped_content" \
        '{
            model: $model,
            messages: [
                {
                    role: "user",
                    content: $content
                }
            ],
            max_tokens: $max_tokens,
            temperature: $temperature,
            top_p: 1,
            frequency_penalty: 0,
            presence_penalty: 0
        }')

    if [[ -z "$payload" ]]; then
        security_log "ERROR" "Failed to create API request payload"
        return 1
    fi

    echo "$payload"
}

# Process successful API response
process_successful_response() {
    local response="$1"
    local request_duration="$2"

    security_log "INFO" "Processing successful API response"

    # Validate response format
    if ! echo "$response" | jq . > /dev/null 2>&1; then
        security_log "ERROR" "Response is not valid JSON"
        return 1
    fi

    # Extract AI analysis content
    local ai_content
    ai_content=$(echo "$response" | jq -r '.choices[0].message.content // empty')

    if [[ -z "$ai_content" ]]; then
        security_log "ERROR" "No content found in AI response"
        return 1
    fi

    # Verify response integrity
    if ! verify_ai_response_integrity "$ai_content"; then
        security_log "ERROR" "AI response integrity verification failed"
        return 1
    fi

    # Store response
    AI_RESPONSE="$ai_content"

    # Extract usage metadata
    local usage_data
    usage_data=$(echo "$response" | jq -r '.usage // empty')

    # Process AI response for outputs
    if ! format_ai_analysis_outputs "$ai_content" "$usage_data" "$request_duration"; then
        security_log "ERROR" "Failed to format AI analysis outputs"
        return 1
    fi

    security_log "INFO" "AI response processed successfully"
    return 0
}

# Format AI analysis outputs
format_ai_analysis_outputs() {
    local ai_content="$1"
    local usage_data="$2"
    local request_duration="$3"

    security_log "INFO" "Formatting AI analysis outputs"

    # Try to parse as JSON first (structured response)
    if echo "$ai_content" | jq . > /dev/null 2>&1; then
        # Structured JSON response
        local analysis_result
        analysis_result=$(echo "$ai_content" | jq -c '.')

        local analysis_summary
        analysis_summary=$(echo "$ai_content" | jq -r '.summary // .analysis_summary // "Analysis completed"')

        local recommendations
        recommendations=$(echo "$ai_content" | jq -c '.recommendations // []')

        # Output using heredoc format for complex values
        {
            echo "analysis_result<<EOF"
            echo "$analysis_result"
            echo "EOF"
        } >> "$GITHUB_OUTPUT"

        {
            echo "analysis_summary<<EOF"
            echo "$analysis_summary"
            echo "EOF"
        } >> "$GITHUB_OUTPUT"

        {
            echo "recommendations<<EOF"
            echo "$recommendations"
            echo "EOF"
        } >> "$GITHUB_OUTPUT"
    else
        # Unstructured text response
        local analysis_result_json
        analysis_result_json=$(jq -n --arg content "$ai_content" '{content: $content}' | jq -c '.')

        {
            echo "analysis_result<<EOF"
            echo "$analysis_result_json"
            echo "EOF"
        } >> "$GITHUB_OUTPUT"

        {
            echo "analysis_summary<<EOF"
            echo "$ai_content"
            echo "EOF"
        } >> "$GITHUB_OUTPUT"

        {
            echo "recommendations<<EOF"
            echo "[]"
            echo "EOF"
        } >> "$GITHUB_OUTPUT"
    fi

    # Generate metadata and output using heredoc format
    local metadata
    metadata=$(generate_success_metadata "$request_duration" "$usage_data")
    {
        echo "analysis_metadata<<EOF"
        echo "$metadata"
        echo "EOF"
    } >> "$GITHUB_OUTPUT"

    return 0
}

# Generate success metadata
generate_success_metadata() {
    local duration="$1"
    local usage_data="${2:-}"

    local metadata
    metadata=$(jq -n \
        --arg timestamp "$(date -u +"%Y-%m-%d %H:%M:%S UTC")" \
        --argjson duration "$duration" \
        --arg analysis_type "${ANALYSIS_TYPE:-unknown}" \
        --arg model "${DEFAULT_MODEL}" \
        --argjson usage_data "${usage_data:-null}" \
        '{
            timestamp: $timestamp,
            duration_seconds: $duration,
            analysis_type: $analysis_type,
            model: $model,
            status: "success",
            usage: $usage_data
        }')

    echo "$metadata"
}

# Generate error metadata
generate_error_metadata() {
    local duration="$1"
    local error_reason="${2:-unknown}"

    local metadata
    metadata=$(jq -n \
        --arg timestamp "$(date -u +"%Y-%m-%d %H:%M:%S UTC")" \
        --argjson duration "$duration" \
        --arg analysis_type "${ANALYSIS_TYPE:-unknown}" \
        --arg error_reason "$error_reason" \
        '{
            timestamp: $timestamp,
            duration_seconds: $duration,
            analysis_type: $analysis_type,
            status: "error",
            error_reason: $error_reason
        }')

    echo "$metadata"
}

# Cleanup function
cleanup_ai_client() {
    # Clean up any temporary files or resources
    rm -f /tmp/ai-request-*.json
    rm -f /tmp/ai-response-*.json

    security_log "INFO" "AI client cleanup completed"
}

# Set cleanup trap
trap cleanup_ai_client EXIT

# Export functions for use in main action
export -f execute_ai_analysis_with_retry
export -f generate_success_metadata
export -f generate_error_metadata
