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

# Process coverage context for AI analysis
process_coverage_context() {
    echo "🔧 Processing coverage context..."

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

    # Generate coverage context JSON
    local coverage_context
    coverage_context=$(cat <<EOF
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

    # Validate JSON format
    if ! echo "$coverage_context" | jq . >/dev/null 2>&1; then
        echo "❌ Generated coverage context is not valid JSON"
        return 1
    fi

    echo "coverage_context<<EOF" >> "$GITHUB_OUTPUT"
    echo "$coverage_context" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    echo "✅ Coverage context processing complete"
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

    # Add phase configuration to context
    local enhanced_context
    enhanced_context=$(echo "$coverage_context" | jq --argjson phase "$phase_config" '. + {"phaseConfiguration": $phase}')

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
export -f process_coverage_context
export -f generate_phase_context
export -f debug_log
export -f calculate_coverage_velocity
export -f estimate_time_to_target