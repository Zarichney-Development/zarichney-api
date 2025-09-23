#!/bin/bash

# Coverage Trends Analyzer for AI Testing Analysis
# Analyzes coverage velocity, trends, and epic progression alignment

set -euo pipefail

# Analyze coverage velocity and trends
analyze_coverage_velocity() {
    echo "📈 Analyzing coverage velocity..."

    local current_coverage="$COVERAGE_DATA"
    local baseline_coverage="$BASELINE_COVERAGE"
    local target_coverage="$IMPROVEMENT_TARGET"

    # Calculate coverage velocity (assuming 1-month period for current measurement)
    local coverage_change
    coverage_change=$(awk "BEGIN {printf \"%.2f\", $current_coverage - $baseline_coverage}")

    local velocity_trend="stable"
    if (( $(awk "BEGIN {print ($coverage_change > 1.0)}") )); then
        velocity_trend="accelerating"
    elif (( $(awk "BEGIN {print ($coverage_change < -0.5)}") )); then
        velocity_trend="declining"
    elif (( $(awk "BEGIN {print ($coverage_change > 0.1)}") )); then
        velocity_trend="improving"
    fi

    # Target velocity for 90% by January 2026 (approximately 2.8%/month)
    local target_velocity=2.8
    local velocity_status="on-track"

    if (( $(awk "BEGIN {print ($coverage_change < 1.4)}") )); then  # 50% of target
        velocity_status="below-target"
    elif (( $(awk "BEGIN {print ($coverage_change > 4.2)}") )); then  # 150% of target
        velocity_status="above-target"
    fi

    # Generate velocity analysis
    local velocity_analysis
    velocity_analysis=$(cat <<EOF
{
  "currentVelocity": $coverage_change,
  "targetVelocity": $target_velocity,
  "velocityTrend": "$velocity_trend",
  "velocityStatus": "$velocity_status",
  "velocityEfficiency": $(awk "BEGIN {printf \"%.1f\", ($coverage_change / $target_velocity) * 100}"),
  "measurementPeriod": "1 month",
  "analysisDate": "$(date -u +"%Y-%m-%d")"
}
EOF
    )

    echo "velocity_analysis<<EOF" >> "$GITHUB_OUTPUT"
    echo "$velocity_analysis" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    debug_log "Coverage velocity analyzed: $velocity_trend ($coverage_change%/month)"
    return 0
}

# Analyze epic progression toward 90% coverage goal
analyze_epic_progression() {
    echo "🎯 Analyzing epic progression..."

    local current_coverage="$COVERAGE_DATA"
    local target_coverage="$IMPROVEMENT_TARGET"
    local epic_context="$EPIC_CONTEXT"

    # Calculate overall epic progress
    local epic_progress
    epic_progress=$(awk "BEGIN {printf \"%.1f\", ($current_coverage / $target_coverage) * 100}")

    # Determine current phase based on coverage level
    local current_phase="Foundation"
    local next_milestone=20

    if (( $(awk "BEGIN {print ($current_coverage >= 75)}") )); then
        current_phase="Mastery"
        next_milestone=90
    elif (( $(awk "BEGIN {print ($current_coverage >= 50)}") )); then
        current_phase="Excellence"
        next_milestone=75
    elif (( $(awk "BEGIN {print ($current_coverage >= 35)}") )); then
        current_phase="Maturity"
        next_milestone=50
    elif (( $(awk "BEGIN {print ($current_coverage >= 20)}") )); then
        current_phase="Growth"
        next_milestone=35
    fi

    # Calculate progress toward next milestone
    local milestone_progress
    milestone_progress=$(awk "BEGIN {printf \"%.1f\", ($current_coverage / $next_milestone) * 100}")

    # Estimate timeline to epic completion
    local monthly_velocity=2.8  # Target velocity
    local coverage_remaining
    coverage_remaining=$(awk "BEGIN {printf \"%.2f\", $target_coverage - $current_coverage}")

    local months_to_completion
    months_to_completion=$(awk "BEGIN {printf \"%.1f\", $coverage_remaining / $monthly_velocity}")

    # Determine epic status
    local epic_status="on-track"
    if (( $(awk "BEGIN {print ($months_to_completion > 15)}") )); then
        epic_status="at-risk"
    elif (( $(awk "BEGIN {print ($months_to_completion < 6)}") )); then
        epic_status="ahead-of-schedule"
    fi

    # Generate epic progression analysis
    local epic_analysis
    epic_analysis=$(cat <<EOF
{
  "epicProgress": $epic_progress,
  "currentPhase": "$current_phase",
  "nextMilestone": $next_milestone,
  "milestoneProgress": $milestone_progress,
  "monthsToCompletion": $months_to_completion,
  "epicStatus": "$epic_status",
  "targetDate": "2026-01-31",
  "contextNotes": "$epic_context"
}
EOF
    )

    echo "epic_progression<<EOF" >> "$GITHUB_OUTPUT"
    echo "$epic_analysis" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    debug_log "Epic progression analyzed: $current_phase phase, $epic_status"
    return 0
}

# Calculate timeline alignment with epic goals
calculate_timeline_alignment() {
    echo "⏰ Calculating timeline alignment..."

    local current_coverage="$COVERAGE_DATA"
    local target_coverage="$IMPROVEMENT_TARGET"

    # Target date for 90% coverage: January 31, 2026
    local target_date="2026-01-31"
    local current_date=$(date +"%Y-%m-%d")

    # Calculate months remaining to target date
    local months_remaining
    months_remaining=$(calculate_months_between "$current_date" "$target_date")

    # Required monthly velocity to meet deadline
    local coverage_remaining
    coverage_remaining=$(awk "BEGIN {printf \"%.2f\", $target_coverage - $current_coverage}")

    local required_velocity
    if (( $(awk "BEGIN {print ($months_remaining > 0)}") )); then
        required_velocity=$(awk "BEGIN {printf \"%.2f\", $coverage_remaining / $months_remaining}")
    else
        required_velocity=0
    fi

    # Current velocity (from recent change)
    local current_velocity
    current_velocity=$(awk "BEGIN {printf \"%.2f\", $COVERAGE_DATA - $BASELINE_COVERAGE}")

    # Timeline status assessment
    local timeline_status="aligned"
    if (( $(awk "BEGIN {print ($required_velocity > 4.0)}") )); then
        timeline_status="aggressive"
    elif (( $(awk "BEGIN {print ($required_velocity > 3.5)}") )); then
        timeline_status="challenging"
    elif (( $(awk "BEGIN {print ($required_velocity < 2.0)}") )); then
        timeline_status="comfortable"
    fi

    # Risk assessment
    local risk_level="medium"
    if (( $(awk "BEGIN {print ($current_velocity < ($required_velocity * 0.7))}") )); then
        risk_level="high"
    elif (( $(awk "BEGIN {print ($current_velocity > ($required_velocity * 1.3))}") )); then
        risk_level="low"
    fi

    # Generate timeline alignment analysis
    local timeline_analysis
    timeline_analysis=$(cat <<EOF
{
  "targetDate": "$target_date",
  "monthsRemaining": $months_remaining,
  "coverageRemaining": $coverage_remaining,
  "requiredVelocity": $required_velocity,
  "currentVelocity": $current_velocity,
  "velocityGap": $(awk "BEGIN {printf \"%.2f\", $required_velocity - $current_velocity}"),
  "timelineStatus": "$timeline_status",
  "riskLevel": "$risk_level",
  "recommendation": "$(generate_timeline_recommendation "$timeline_status" "$risk_level")"
}
EOF
    )

    echo "timeline_alignment<<EOF" >> "$GITHUB_OUTPUT"
    echo "$timeline_analysis" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    # Generate final coverage trends output
    generate_coverage_trends_output

    debug_log "Timeline alignment calculated: $timeline_status risk level $risk_level"
    return 0
}

# Generate comprehensive coverage trends output
generate_coverage_trends_output() {
    echo "📊 Generating comprehensive coverage trends..."

    # Combine all trend analyses into single output
    local coverage_trends
    coverage_trends=$(cat <<EOF
{
  "velocity": $(cat <<< "$velocity_analysis"),
  "epicProgression": $(cat <<< "$epic_progression"),
  "timelineAlignment": $(cat <<< "$timeline_analysis"),
  "summary": {
    "currentCoverage": $COVERAGE_DATA,
    "targetCoverage": $IMPROVEMENT_TARGET,
    "overallStatus": "$(determine_overall_status)",
    "keyMetrics": {
      "monthlyVelocity": $(awk "BEGIN {printf \"%.2f\", $COVERAGE_DATA - $BASELINE_COVERAGE}"),
      "epicProgress": $(awk "BEGIN {printf \"%.1f\", ($COVERAGE_DATA / $IMPROVEMENT_TARGET) * 100}"),
      "timelineAlignment": "$(get_timeline_status)"
    }
  },
  "generatedAt": "$(date -u +"%Y-%m-%d %H:%M:%S UTC")"
}
EOF
    )

    echo "coverage_trends<<EOF" >> "$GITHUB_OUTPUT"
    echo "$coverage_trends" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    # Generate next steps based on analysis
    generate_next_steps

    debug_log "Coverage trends analysis complete"
    return 0
}

# Generate immediate next steps for coverage improvement
generate_next_steps() {
    echo "📋 Generating next steps..."

    local overall_status=$(determine_overall_status)
    local next_steps_json

    case "$overall_status" in
        "ahead-of-schedule")
            next_steps_json='[
                "Continue current testing velocity to maintain lead",
                "Focus on test quality and maintainability improvements",
                "Consider contributing to other testing initiatives"
            ]'
            ;;
        "on-track")
            next_steps_json='[
                "Maintain current testing momentum and velocity",
                "Focus on high-impact service layer test coverage",
                "Add integration tests for core API endpoints"
            ]'
            ;;
        "at-risk")
            next_steps_json='[
                "Accelerate testing efforts with focused sprints",
                "Prioritize high-impact coverage improvements",
                "Consider parallel testing development streams",
                "Review testing strategy and resource allocation"
            ]'
            ;;
        *)
            next_steps_json='[
                "Assess current testing approach and priorities",
                "Focus on foundational service layer coverage",
                "Establish consistent testing velocity"
            ]'
            ;;
    esac

    echo "next_steps<<EOF" >> "$GITHUB_OUTPUT"
    echo "$next_steps_json" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    debug_log "Next steps generated for status: $overall_status"
    return 0
}

# Utility functions
calculate_months_between() {
    local start_date="$1"
    local end_date="$2"

    # Simple month calculation (approximate)
    local start_epoch=$(date -d "$start_date" +%s)
    local end_epoch=$(date -d "$end_date" +%s)
    local seconds_diff=$((end_epoch - start_epoch))
    local months_diff=$(awk "BEGIN {printf \"%.1f\", $seconds_diff / (30 * 24 * 3600)}")

    echo "$months_diff"
}

generate_timeline_recommendation() {
    local timeline_status="$1"
    local risk_level="$2"

    case "$timeline_status" in
        "aggressive")
            echo "Consider extending timeline or increasing testing resources"
            ;;
        "challenging")
            echo "Focus on high-impact test coverage and efficient testing strategies"
            ;;
        "aligned")
            echo "Maintain current testing velocity and focus on quality"
            ;;
        "comfortable")
            echo "Opportunity to improve test quality and explore advanced testing patterns"
            ;;
        *)
            echo "Review testing strategy and timeline expectations"
            ;;
    esac
}

determine_overall_status() {
    local velocity_status=$(echo "$velocity_analysis" | jq -r '.velocityStatus // "unknown"')
    local epic_status=$(echo "$epic_progression" | jq -r '.epicStatus // "unknown"')
    local timeline_status=$(echo "$timeline_analysis" | jq -r '.timelineStatus // "unknown"')

    # Simple logic to determine overall status
    if [[ "$epic_status" == "ahead-of-schedule" ]]; then
        echo "ahead-of-schedule"
    elif [[ "$epic_status" == "at-risk" || "$velocity_status" == "below-target" ]]; then
        echo "at-risk"
    else
        echo "on-track"
    fi
}

get_timeline_status() {
    echo "$timeline_analysis" | jq -r '.timelineStatus // "unknown"'
}

# Debug logging function
debug_log() {
    if [[ "$DEBUG_MODE" == "true" ]]; then
        echo "🐛 DEBUG: $1" >&2
    fi
}

# Export functions for use in action
export -f analyze_coverage_velocity
export -f analyze_epic_progression
export -f calculate_timeline_alignment
export -f generate_coverage_trends_output
export -f generate_next_steps
export -f calculate_months_between
export -f generate_timeline_recommendation
export -f determine_overall_status
export -f get_timeline_status
export -f debug_log