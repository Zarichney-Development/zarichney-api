#!/bin/bash

# Coverage Context Processor for AI Testing Analysis
# Validates and structures coverage data for intelligent AI analysis

set -euo pipefail

# Coverage data validation functions
validate_coverage_inputs() {
    local validation_success=true

    if [[ -z "$COVERAGE_DATA" ]]; then
        echo "❌ Coverage data is required but not provided"
        validation_success=false
    fi

    if [[ -z "$BASELINE_COVERAGE" ]]; then
        echo "❌ Baseline coverage is required but not provided"
        validation_success=false
    fi

    if [[ -z "$TEST_RESULTS" ]]; then
        echo "❌ Test results are required but not provided"
        validation_success=false
    fi

    # Validate coverage data is numeric
    if ! [[ "$COVERAGE_DATA" =~ ^[0-9]+(\.[0-9]+)?$ ]]; then
        # Try to extract numeric coverage from string format
        local extracted_coverage=$(echo "$COVERAGE_DATA" | grep -oE '[0-9]+(\.[0-9]+)?%?' | head -1 | sed 's/%$//')
        if [[ -n "$extracted_coverage" ]]; then
            export COVERAGE_DATA="$extracted_coverage"
            echo "📊 Extracted coverage percentage: $COVERAGE_DATA%"
        else
            echo "❌ Coverage data must be numeric (e.g., 15.5 or 15.5%)"
            validation_success=false
        fi
    fi

    # Validate baseline coverage is numeric
    if ! [[ "$BASELINE_COVERAGE" =~ ^[0-9]+(\.[0-9]+)?$ ]]; then
        local extracted_baseline=$(echo "$BASELINE_COVERAGE" | grep -oE '[0-9]+(\.[0-9]+)?%?' | head -1 | sed 's/%$//')
        if [[ -n "$extracted_baseline" ]]; then
            export BASELINE_COVERAGE="$extracted_baseline"
            echo "📊 Extracted baseline coverage: $BASELINE_COVERAGE%"
        else
            echo "❌ Baseline coverage must be numeric (e.g., 14.22)"
            validation_success=false
        fi
    fi

    # Validate improvement target is numeric
    if ! [[ "$IMPROVEMENT_TARGET" =~ ^[0-9]+(\.[0-9]+)?$ ]]; then
        echo "❌ Improvement target must be numeric (e.g., 90)"
        validation_success=false
    fi

    if [[ "$validation_success" == "true" ]]; then
        echo "✅ Coverage input validation successful"
        return 0
    else
        return 1
    fi
}

# Read and process enhanced coverage data files
read_enhanced_coverage_data() {
    debug_log "Reading enhanced coverage data files..."

    # Initialize enhanced data variables
    enhanced_coverage_data=""
    enhanced_delta_data=""
    enhanced_trends_data=""

    # Read coverage_data_file (coverage_results.json) if available
    local coverage_data_file="${COVERAGE_DATA_FILE:-TestResults/coverage_results.json}"
    if [[ -f "$coverage_data_file" ]]; then
        debug_log "Reading coverage data file: $coverage_data_file"
        if enhanced_coverage_data=$(cat "$coverage_data_file" 2>/dev/null) && echo "$enhanced_coverage_data" | jq . >/dev/null 2>&1; then
            debug_log "✅ Successfully loaded coverage data file"
        else
            debug_log "⚠️ Coverage data file exists but is not valid JSON"
            enhanced_coverage_data=""
        fi
    else
        debug_log "ℹ️ Coverage data file not found: $coverage_data_file"
    fi

    # Read coverage_delta_file (coverage_delta.json) if available
    local coverage_delta_file="${COVERAGE_DELTA_FILE:-TestResults/coverage_delta.json}"
    if [[ -f "$coverage_delta_file" ]]; then
        debug_log "Reading coverage delta file: $coverage_delta_file"
        if enhanced_delta_data=$(cat "$coverage_delta_file" 2>/dev/null) && echo "$enhanced_delta_data" | jq . >/dev/null 2>&1; then
            debug_log "✅ Successfully loaded coverage delta file"
        else
            debug_log "⚠️ Coverage delta file exists but is not valid JSON"
            enhanced_delta_data=""
        fi
    else
        debug_log "ℹ️ Coverage delta file not found: $coverage_delta_file"
    fi

    # Read coverage_trends_file (health_trends.json) if available
    local coverage_trends_file="${COVERAGE_TRENDS_FILE:-TestResults/health_trends.json}"
    if [[ -f "$coverage_trends_file" ]]; then
        debug_log "Reading coverage trends file: $coverage_trends_file"
        if enhanced_trends_data=$(cat "$coverage_trends_file" 2>/dev/null) && echo "$enhanced_trends_data" | jq . >/dev/null 2>&1; then
            debug_log "✅ Successfully loaded coverage trends file"
        else
            debug_log "⚠️ Coverage trends file exists but is not valid JSON"
            enhanced_trends_data=""
        fi
    else
        debug_log "ℹ️ Coverage trends file not found: $coverage_trends_file"
    fi

    # Export for use in other functions
    export enhanced_coverage_data
    export enhanced_delta_data
    export enhanced_trends_data

    echo "📊 Enhanced coverage data loading complete"
    return 0
}

# Process coverage context for AI analysis
process_coverage_context() {
    echo "🔧 Processing coverage context..."

    # Read enhanced coverage data files
    if ! read_enhanced_coverage_data; then
        echo "⚠️ Enhanced coverage data reading failed, continuing with basic context"
    fi

    # Calculate coverage change
    local coverage_change
    coverage_change=$(awk "BEGIN {printf \"%.2f\", $COVERAGE_DATA - $BASELINE_COVERAGE}")

    # Determine coverage trend
    local coverage_trend="stable"
    if (( $(awk "BEGIN {print ($coverage_change > 0.5)}") )); then
        coverage_trend="improving"
    elif (( $(awk "BEGIN {print ($coverage_change < -0.5)}") )); then
        coverage_trend="declining"
    fi

    # Calculate progress toward target
    local target_progress
    target_progress=$(awk "BEGIN {printf \"%.2f\", ($COVERAGE_DATA / $IMPROVEMENT_TARGET) * 100}")

    # Build base coverage context
    local base_coverage_context
    base_coverage_context=$(cat <<EOF
{
  "coverage": {
    "current": $COVERAGE_DATA,
    "baseline": $BASELINE_COVERAGE,
    "change": $coverage_change,
    "trend": "$coverage_trend",
    "target": $IMPROVEMENT_TARGET,
    "targetProgress": $target_progress
  },
  "testResults": $TEST_RESULTS,
  "analysisPhase": "$COVERAGE_PHASE",
  "epicContext": "$EPIC_CONTEXT",
  "analysisDepth": "$ANALYSIS_DEPTH"
}
EOF
    )

    # Enhance with delta data if available
    local coverage_context="$base_coverage_context"
    if [[ -n "$enhanced_delta_data" ]]; then
        debug_log "Enhancing context with coverage delta data"
        coverage_context=$(echo "$coverage_context" | jq --argjson delta "$enhanced_delta_data" '. + {"coverageDelta": $delta}')

        # Override calculated values with delta file values if they exist
        local delta_current=$(echo "$enhanced_delta_data" | jq -r '.current_coverage // empty' 2>/dev/null)
        local delta_baseline=$(echo "$enhanced_delta_data" | jq -r '.baseline_coverage // empty' 2>/dev/null)
        local delta_change=$(echo "$enhanced_delta_data" | jq -r '.coverage_delta // empty' 2>/dev/null)
        local delta_trend=$(echo "$enhanced_delta_data" | jq -r '.coverage_trend // empty' 2>/dev/null)

        if [[ -n "$delta_current" && "$delta_current" != "null" ]]; then
            coverage_context=$(echo "$coverage_context" | jq --arg val "$delta_current" '.coverage.current = ($val | tonumber)')
        fi
        if [[ -n "$delta_baseline" && "$delta_baseline" != "null" ]]; then
            coverage_context=$(echo "$coverage_context" | jq --arg val "$delta_baseline" '.coverage.baseline = ($val | tonumber)')
        fi
        if [[ -n "$delta_change" && "$delta_change" != "null" ]]; then
            coverage_context=$(echo "$coverage_context" | jq --arg val "$delta_change" '.coverage.change = ($val | tonumber)')
        fi
        if [[ -n "$delta_trend" && "$delta_trend" != "null" ]]; then
            coverage_context=$(echo "$coverage_context" | jq --arg val "$delta_trend" '.coverage.trend = $val')
        fi

        debug_log "✅ Coverage context enhanced with delta data"
    else
        debug_log "ℹ️ No coverage delta data available for enhancement"
    fi

    # Enhance with trends data if available
    if [[ -n "$enhanced_trends_data" ]]; then
        debug_log "Enhancing context with coverage trends data"
        coverage_context=$(echo "$coverage_context" | jq --argjson trends "$enhanced_trends_data" '. + {"coverageTrends": $trends}')
        debug_log "✅ Coverage context enhanced with trends data"
    else
        debug_log "ℹ️ No coverage trends data available for enhancement"
    fi

    # Enhance with detailed coverage data if available
    if [[ -n "$enhanced_coverage_data" ]]; then
        debug_log "Enhancing context with detailed coverage data"
        coverage_context=$(echo "$coverage_context" | jq --argjson details "$enhanced_coverage_data" '. + {"coverageDetails": $details}')
        debug_log "✅ Coverage context enhanced with detailed coverage data"
    else
        debug_log "ℹ️ No detailed coverage data available for enhancement"
    fi

    # Validate final JSON format
    if ! echo "$coverage_context" | jq . >/dev/null 2>&1; then
        echo "❌ Generated coverage context is not valid JSON"
        debug_log "Invalid JSON context: $coverage_context"
        return 1
    fi

    # Set global variable for access in other functions
    export coverage_context

    echo "coverage_context<<EOF" >> "$GITHUB_OUTPUT"
    echo "$coverage_context" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    # Log enhancement status
    local enhancement_status="basic"
    if [[ -n "$enhanced_delta_data" ]]; then
        enhancement_status="delta-enhanced"
    fi
    if [[ -n "$enhanced_trends_data" ]]; then
        enhancement_status="trends-enhanced"
    fi
    if [[ -n "$enhanced_coverage_data" ]]; then
        enhancement_status="fully-enhanced"
    fi

    debug_log "Coverage context enhancement status: $enhancement_status"
    echo "✅ Coverage context processing complete (status: $enhancement_status)"
    return 0
}

# Generate phase-aware context
generate_phase_context() {
    echo "🎯 Generating phase-aware context..."

    # Define phase characteristics
    local phase_config
    case "$COVERAGE_PHASE" in
        "initial")
            phase_config='{
                "focus": "baseline establishment",
                "priorities": ["core functionality", "API contracts", "business logic"],
                "depth": "foundational coverage",
                "timeline": "immediate"
            }'
            ;;
        "iterative-improvement")
            phase_config='{
                "focus": "progressive enhancement",
                "priorities": ["service layers", "integration scenarios", "edge cases"],
                "depth": "comprehensive testing",
                "timeline": "sprint-based"
            }'
            ;;
        "milestone")
            phase_config='{
                "focus": "target achievement",
                "priorities": ["gap closure", "quality assurance", "optimization"],
                "depth": "complete coverage",
                "timeline": "milestone-driven"
            }'
            ;;
        *)
            phase_config='{
                "focus": "standard analysis",
                "priorities": ["general improvements"],
                "depth": "balanced",
                "timeline": "flexible"
            }'
            ;;
    esac

    # Extract coverage context from GITHUB_OUTPUT
    local coverage_context_content
    if ! coverage_context_content=$(grep -A 1000 "coverage_context<<EOF" "$GITHUB_OUTPUT" | grep -B 1000 "^EOF$" | head -n -1 | tail -n +2); then
        echo "❌ Failed to extract coverage context from GITHUB_OUTPUT"
        return 1
    fi

    # Add phase configuration to context
    local enhanced_context
    enhanced_context=$(echo "$coverage_context_content" | jq --argjson phase "$phase_config" '. + {"phaseConfiguration": $phase}')

    echo "enhanced_context<<EOF" >> "$GITHUB_OUTPUT"
    echo "$enhanced_context" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    echo "✅ Phase-aware context generation complete"
    return 0
}

# Debug logging function
debug_log() {
    if [[ "$DEBUG_MODE" == "true" ]]; then
        echo "🐛 DEBUG: $1" >&2
    fi
}

# Utility function to calculate coverage velocity
calculate_coverage_velocity() {
    local current_coverage="$1"
    local baseline_coverage="$2"
    local time_period_months="${3:-1}"  # Default to 1 month if not specified

    local velocity
    velocity=$(awk "BEGIN {printf \"%.2f\", ($current_coverage - $baseline_coverage) / $time_period_months}")
    echo "$velocity"
}

# Utility function to estimate time to target
estimate_time_to_target() {
    local current_coverage="$1"
    local target_coverage="$2"
    local monthly_velocity="$3"

    if (( $(awk "BEGIN {print ($monthly_velocity <= 0)}") )); then
        echo "infinite"  # No progress or negative progress
        return
    fi

    local remaining_coverage
    remaining_coverage=$(awk "BEGIN {printf \"%.2f\", $target_coverage - $current_coverage}")

    local months_to_target
    months_to_target=$(awk "BEGIN {printf \"%.1f\", $remaining_coverage / $monthly_velocity}")

    echo "$months_to_target"
}

# Export functions for use in action
export -f validate_coverage_inputs
export -f read_enhanced_coverage_data
export -f process_coverage_context
export -f generate_phase_context
export -f debug_log
export -f calculate_coverage_velocity
export -f estimate_time_to_target