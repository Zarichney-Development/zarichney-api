#!/bin/bash
# AI Sentinel Base - Security Validation Framework
# Comprehensive security controls for prompt injection prevention and input validation

set -euo pipefail

# Security Constants
readonly ALLOWED_ANALYSIS_TYPES=("testing" "standards" "security" "debt" "merge")
readonly TEMPLATE_BASE_PATH=".github/prompts"
readonly MAX_CONTEXT_SIZE=50000  # 50KB limit for context data
readonly MAX_TEMPLATE_SIZE=100000  # 100KB limit for template files

# Security logging function
security_log() {
    local level="$1"
    local message="$2"
    local timestamp=$(date -u +"%Y-%m-%d %H:%M:%S UTC")

    if [[ "${DEBUG_MODE:-false}" == "true" ]]; then
        echo "[$timestamp] [SECURITY-$level] $message" >&2
    fi

    # Log security events for audit trail
    echo "[$timestamp] [SECURITY-$level] $message" >> "/tmp/ai-sentinel-security.log"
}

# Validate analysis type against whitelist
validate_analysis_type() {
    local analysis_type="$1"

    security_log "INFO" "Validating analysis type: $analysis_type"

    # Check against whitelist
    for allowed_type in "${ALLOWED_ANALYSIS_TYPES[@]}"; do
        if [[ "$analysis_type" == "$allowed_type" ]]; then
            security_log "INFO" "Analysis type validation passed: $analysis_type"
            return 0
        fi
    done

    security_log "ERROR" "Analysis type validation failed: $analysis_type not in whitelist"
    return 1
}

# Validate template path for security
validate_template_path() {
    local template_path="$1"

    security_log "INFO" "Validating template path: $template_path"

    # Check path traversal attempts
    if [[ "$template_path" =~ \.\./|\.\.\\ ]]; then
        security_log "ERROR" "Path traversal attempt detected: $template_path"
        return 1
    fi

    # Ensure path starts with allowed base path
    if [[ ! "$template_path" =~ ^\.github/prompts/ ]]; then
        security_log "ERROR" "Template path outside allowed directory: $template_path"
        return 1
    fi

    # Check file exists and is readable
    if [[ ! -f "$template_path" ]]; then
        security_log "ERROR" "Template file not found: $template_path"
        return 1
    fi

    if [[ ! -r "$template_path" ]]; then
        security_log "ERROR" "Template file not readable: $template_path"
        return 1
    fi

    # Check file size
    local file_size=$(stat -f%z "$template_path" 2>/dev/null || stat -c%s "$template_path" 2>/dev/null || echo "0")
    if [[ "$file_size" -gt "$MAX_TEMPLATE_SIZE" ]]; then
        security_log "ERROR" "Template file too large: $file_size bytes > $MAX_TEMPLATE_SIZE"
        return 1
    fi

    security_log "INFO" "Template path validation passed: $template_path"
    return 0
}

# Validate context data format and content
validate_context_format() {
    local context_data="$1"

    security_log "INFO" "Validating context data format"

    # Check size limit
    local data_size=${#context_data}
    if [[ "$data_size" -gt "$MAX_CONTEXT_SIZE" ]]; then
        security_log "ERROR" "Context data too large: $data_size bytes > $MAX_CONTEXT_SIZE"
        return 1
    fi

    # Validate JSON format
    if ! echo "$context_data" | jq . > /dev/null 2>&1; then
        security_log "ERROR" "Context data is not valid JSON"
        return 1
    fi

    # Check for potential injection patterns
    if echo "$context_data" | grep -qE "(system|assistant|user):\s*[\"\']"; then
        security_log "WARN" "Potential prompt injection pattern detected in context data"
        # Don't fail for warnings, but log for monitoring
    fi

    # Check for suspicious script patterns
    if echo "$context_data" | grep -qEi "(script|javascript|eval|exec|system|cmd)"; then
        security_log "WARN" "Suspicious script patterns detected in context data"
    fi

    security_log "INFO" "Context data validation passed"
    return 0
}

# Sanitize input for safe processing
sanitize_input() {
    local input="$1"
    local sanitized

    # Remove null bytes
    sanitized=$(echo "$input" | tr -d '\0')

    # Limit line length to prevent buffer overflow attempts
    sanitized=$(echo "$sanitized" | cut -c1-10000)

    # Remove potential shell injection patterns in template context
    sanitized=$(echo "$sanitized" | sed 's/[`$(){};&|<>]/\\&/g')

    echo "$sanitized"
}

# Validate placeholder patterns in templates
validate_placeholder_patterns() {
    local template_content="$1"

    security_log "INFO" "Validating placeholder patterns"

    # Define allowed placeholder patterns
    local allowed_patterns=(
        "{{PR_NUMBER}}"
        "{{PR_AUTHOR}}"
        "{{SOURCE_BRANCH}}"
        "{{TARGET_BRANCH}}"
        "{{ISSUE_REF}}"
        "{{CHANGED_FILES_COUNT}}"
        "{{LINES_CHANGED}}"
        "{{TIMESTAMP}}"
        "{{CONTEXT_DATA}}"
    )

    # Extract all placeholder patterns from template
    local found_patterns
    found_patterns=$(echo "$template_content" | grep -oE '\{\{[^}]+\}\}' | sort -u)

    # Validate each found pattern
    while IFS= read -r pattern; do
        [[ -z "$pattern" ]] && continue

        local pattern_allowed=false
        for allowed in "${allowed_patterns[@]}"; do
            if [[ "$pattern" == "$allowed" ]]; then
                pattern_allowed=true
                break
            fi
        done

        if [[ "$pattern_allowed" != "true" ]]; then
            security_log "ERROR" "Unauthorized placeholder pattern detected: $pattern"
            return 1
        fi
    done <<< "$found_patterns"

    security_log "INFO" "Placeholder pattern validation passed"
    return 0
}

# Generate integrity checksum for templates
generate_template_checksum() {
    local template_path="$1"

    # Generate SHA256 checksum
    if command -v sha256sum >/dev/null 2>&1; then
        sha256sum "$template_path" | cut -d' ' -f1
    elif command -v shasum >/dev/null 2>&1; then
        shasum -a 256 "$template_path" | cut -d' ' -f1
    else
        # Fallback to md5 if SHA256 not available
        md5sum "$template_path" | cut -d' ' -f1
    fi
}

# Verify AI response integrity
verify_ai_response_integrity() {
    local response="$1"

    security_log "INFO" "Verifying AI response integrity"

    # Check response size
    local response_size=${#response}
    if [[ "$response_size" -gt 100000 ]]; then  # 100KB limit for responses
        security_log "ERROR" "AI response too large: $response_size bytes"
        return 1
    fi

    # Check for suspicious patterns that might indicate tampering
    if echo "$response" | grep -qE "(system:|assistant:|user:)" &&
       echo "$response" | grep -qE "(ignore|override|bypass).*instructions"; then
        security_log "WARN" "Potential response tampering detected"
    fi

    # Validate JSON structure if response should be JSON
    if [[ "$response" =~ ^\s*\{ ]] && [[ "$response" =~ \}\s*$ ]]; then
        if ! echo "$response" | jq . > /dev/null 2>&1; then
            security_log "ERROR" "AI response is malformed JSON"
            return 1
        fi
    fi

    security_log "INFO" "AI response integrity verification passed"
    return 0
}

# Export functions for use in other scripts
export -f validate_analysis_type
export -f validate_template_path
export -f validate_context_format
export -f sanitize_input
export -f validate_placeholder_patterns
export -f generate_template_checksum
export -f verify_ai_response_integrity
export -f security_log