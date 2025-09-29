#!/bin/bash
# AI Sentinel Base - Secure Template Processing System
# Template Method pattern implementation with secure placeholder replacement

set -euo pipefail

# Import security validation functions
source "$(dirname "${BASH_SOURCE[0]}")/security-validation.sh"

# Template processing constants
readonly PROCESSED_TEMPLATE_PATH="/tmp/processed-template.md"
readonly TEMPLATE_CACHE_DIR="/tmp/ai-sentinel-cache"

# Template processing state
TEMPLATE_CONTENT=""
TEMPLATE_CHECKSUM=""

# Initialize template processing environment
init_template_processor() {
    security_log "INFO" "Initializing template processor"

    # Create cache directory
    mkdir -p "$TEMPLATE_CACHE_DIR"

    # Clear any existing processed template
    rm -f "$PROCESSED_TEMPLATE_PATH"

    security_log "INFO" "Template processor initialized"
}

# Load and validate template
load_template() {
    local template_path="$1"

    security_log "INFO" "Loading template: $template_path"

    # Validate template path (security check)
    if ! validate_template_path "$template_path"; then
        security_log "ERROR" "Template path validation failed"
        return 1
    fi

    # Read template content
    TEMPLATE_CONTENT=$(cat "$template_path")
    if [[ -z "$TEMPLATE_CONTENT" ]]; then
        security_log "ERROR" "Template is empty: $template_path"
        return 1
    fi

    # Generate integrity checksum
    TEMPLATE_CHECKSUM=$(generate_template_checksum "$template_path")
    security_log "INFO" "Template checksum: $TEMPLATE_CHECKSUM"

    # Validate placeholder patterns
    if ! validate_placeholder_patterns "$TEMPLATE_CONTENT"; then
        security_log "ERROR" "Template contains invalid placeholder patterns"
        return 1
    fi

    security_log "INFO" "Template loaded successfully: ${#TEMPLATE_CONTENT} characters"
    return 0
}

# Secure placeholder replacement
process_template_placeholders() {
    security_log "INFO" "Processing template placeholders"

    if [[ -z "$TEMPLATE_CONTENT" ]]; then
        security_log "ERROR" "No template loaded for placeholder processing"
        return 1
    fi

    # Initialize processed content
    local processed_content="$TEMPLATE_CONTENT"

    # Process each placeholder with security sanitization
    processed_content=$(replace_placeholder "$processed_content" "PR_NUMBER" "${PR_NUMBER:-unknown}")
    processed_content=$(replace_placeholder "$processed_content" "PR_AUTHOR" "${PR_AUTHOR:-unknown}")
    processed_content=$(replace_placeholder "$processed_content" "SOURCE_BRANCH" "${SOURCE_BRANCH:-unknown}")
    processed_content=$(replace_placeholder "$processed_content" "TARGET_BRANCH" "${TARGET_BRANCH:-unknown}")
    processed_content=$(replace_placeholder "$processed_content" "ISSUE_REF" "${ISSUE_REF:-No linked issue}")
    processed_content=$(replace_placeholder "$processed_content" "CHANGED_FILES_COUNT" "${CHANGED_FILES_COUNT:-0}")
    processed_content=$(replace_placeholder "$processed_content" "LINES_CHANGED" "${LINES_CHANGED:-0}")
    processed_content=$(replace_placeholder "$processed_content" "TIMESTAMP" "$(date -u +"%Y-%m-%d %H:%M:%S UTC")")

    # Process context data placeholder with special handling
    if [[ -n "${CONTEXT_DATA:-}" ]]; then
        processed_content=$(replace_context_placeholder "$processed_content" "$CONTEXT_DATA")
    fi

    # Write processed template to output file
    echo "$processed_content" > "$PROCESSED_TEMPLATE_PATH"

    # Verify processed template integrity
    if [[ ! -f "$PROCESSED_TEMPLATE_PATH" ]]; then
        security_log "ERROR" "Failed to write processed template"
        return 1
    fi

    local processed_size
    processed_size=$(stat -f%z "$PROCESSED_TEMPLATE_PATH" 2>/dev/null || stat -c%s "$PROCESSED_TEMPLATE_PATH" 2>/dev/null || echo "0")
    security_log "INFO" "Processed template size: $processed_size bytes"

    if [[ "${DEBUG_MODE:-false}" == "true" ]]; then
        security_log "DEBUG" "Processed template preview:"
        head -20 "$PROCESSED_TEMPLATE_PATH" | while IFS= read -r line; do
            security_log "DEBUG" "  $line"
        done
    fi

    security_log "INFO" "Template placeholder processing completed"
    return 0
}

# Secure placeholder replacement function
replace_placeholder() {
    local content="$1"
    local placeholder="$2"
    local value="$3"

    # Sanitize value for security
    local sanitized_value
    sanitized_value=$(sanitize_input "$value")

    # Log replacement for audit trail
    if [[ "${DEBUG_MODE:-false}" == "true" ]]; then
        security_log "DEBUG" "Replacing {{$placeholder}} with: $sanitized_value"
    fi

    # Perform replacement using | delimiter to avoid conflicts with forward slashes in paths/URLs
    echo "$content" | sed "s|{{$placeholder}}|$sanitized_value|g"
}

# Special handling for context data placeholder
replace_context_placeholder() {
    local content="$1"
    local context_data="$2"

    security_log "INFO" "Processing context data placeholder"

    # Validate context data format
    if ! validate_context_format "$context_data"; then
        security_log "ERROR" "Context data validation failed"
        return 1
    fi

    # Parse and sanitize context data
    local sanitized_context
    sanitized_context=$(echo "$context_data" | jq -r '.' 2>/dev/null || echo "$context_data")

    # Format context data for template insertion
    local formatted_context
    formatted_context=$(format_context_for_template "$sanitized_context")

    # Replace placeholder
    echo "$content" | sed "s|{{CONTEXT_DATA}}|$formatted_context|g"
}

# Format context data for safe template insertion
format_context_for_template() {
    local context_data="$1"

    # Escape special characters for safe insertion
    local formatted
    formatted=$(echo "$context_data" | sed 's/\\/\\\\/g' | sed 's/\//\\\//g')

    # Convert newlines to literal \n for single-line insertion
    formatted=$(echo "$formatted" | tr '\n' ' ')

    echo "$formatted"
}

# Cache processed template for performance
cache_processed_template() {
    local cache_key="$1"
    local cache_file="$TEMPLATE_CACHE_DIR/$cache_key.md"

    if [[ -f "$PROCESSED_TEMPLATE_PATH" ]]; then
        cp "$PROCESSED_TEMPLATE_PATH" "$cache_file"
        security_log "INFO" "Template cached: $cache_key"
    fi
}

# Retrieve cached template if available
get_cached_template() {
    local cache_key="$1"
    local cache_file="$TEMPLATE_CACHE_DIR/$cache_key.md"

    if [[ -f "$cache_file" ]]; then
        local cache_age
        cache_age=$(( $(date +%s) - $(stat -f%m "$cache_file" 2>/dev/null || stat -c%Y "$cache_file" 2>/dev/null || echo "0") ))

        # Cache valid for 1 hour
        if [[ "$cache_age" -lt 3600 ]]; then
            cp "$cache_file" "$PROCESSED_TEMPLATE_PATH"
            security_log "INFO" "Using cached template: $cache_key"
            return 0
        else
            security_log "INFO" "Cache expired for: $cache_key"
            rm -f "$cache_file"
        fi
    fi

    return 1
}

# Generate cache key based on template and context
generate_cache_key() {
    local template_path="$1"
    local context_hash

    # Create hash of context variables for cache key
    context_hash=$(echo "${PR_NUMBER:-}${PR_AUTHOR:-}${SOURCE_BRANCH:-}${TARGET_BRANCH:-}${ISSUE_REF:-}" | \
                  md5sum 2>/dev/null | cut -d' ' -f1 || echo "nocache")

    echo "$(basename "$template_path" .md)_$context_hash"
}

# Validate processed template before use
validate_processed_template() {
    if [[ ! -f "$PROCESSED_TEMPLATE_PATH" ]]; then
        security_log "ERROR" "Processed template file not found"
        return 1
    fi

    # Check for unprocessed placeholders
    local unprocessed_placeholders
    unprocessed_placeholders=$(grep -oE '\{\{[^}]+\}\}' "$PROCESSED_TEMPLATE_PATH" | head -5)

    if [[ -n "$unprocessed_placeholders" ]]; then
        security_log "WARN" "Unprocessed placeholders found: $unprocessed_placeholders"
        # Don't fail for warnings, but log for monitoring
    fi

    # Validate final template size
    local template_size
    template_size=$(stat -f%z "$PROCESSED_TEMPLATE_PATH" 2>/dev/null || stat -c%s "$PROCESSED_TEMPLATE_PATH" 2>/dev/null || echo "0")

    if [[ "$template_size" -gt 150000 ]]; then  # 150KB limit for processed templates
        security_log "ERROR" "Processed template too large: $template_size bytes"
        return 1
    fi

    security_log "INFO" "Processed template validation passed"
    return 0
}

# Main template processing workflow
process_template_workflow() {
    local template_path="$1"

    # Initialize processor
    init_template_processor

    # Generate cache key
    local cache_key
    cache_key=$(generate_cache_key "$template_path")

    # Try to use cached template first
    if get_cached_template "$cache_key"; then
        if validate_processed_template; then
            security_log "INFO" "Using cached processed template"
            return 0
        else
            security_log "WARN" "Cached template validation failed, reprocessing"
        fi
    fi

    # Load and process template
    if ! load_template "$template_path"; then
        security_log "ERROR" "Template loading failed"
        return 1
    fi

    if ! process_template_placeholders; then
        security_log "ERROR" "Template placeholder processing failed"
        return 1
    fi

    # Validate final processed template
    if ! validate_processed_template; then
        security_log "ERROR" "Processed template validation failed"
        return 1
    fi

    # Cache for future use
    cache_processed_template "$cache_key"

    security_log "INFO" "Template processing workflow completed successfully"
    return 0
}

# Export functions for use in main action
export -f init_template_processor
export -f load_template
export -f process_template_placeholders
export -f process_template_workflow
export -f validate_processed_template